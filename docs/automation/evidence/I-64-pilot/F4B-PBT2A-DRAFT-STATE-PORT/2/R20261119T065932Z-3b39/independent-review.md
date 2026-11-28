# I-64 F4B PB-T2a, intento 2 (la corrección): revisión adversarial independiente del código

**Objeto:** `git diff 0dc623ca7a4bf1640654440276aa3d27147f48d4 5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`. Lo componen:
- el RED del intento 1, `c33ac6ac`;
- el commit de la sesión `17872be8` (la BaseSha);
- el RED de la corrección, `fb90c823`;
- el GREEN, `5ddeb3b4`.

WorkRunId `R20261119T065932Z-3b39`.

**Cómo se leyó:**
- el código y los documentos, solo por objetos git desde el checkout principal (`git show`, `diff`, `log`, `ls-tree`, `rev-parse`);
- la entrega, desde su copia (`rv/`).

No se leyó, listó ni nombró el worktree. No se ejecutó dotnet ni ninguna prueba. No se escribió en el repositorio.

## Veredicto

**ACEPTABLE CON MENORES. B/I/M = 0/0/5.** No hay ningún hallazgo para el Owner.

- **C-1** está aplicada exactamente y no debilita CT-PBT2A-02. **C-2** se cumple.
- **El GREEN** es exactamente D-PBT2-02 y D-PBT2-03.
- **D2:** ninguna línea nueva toca las dos referencias compartidas. G-L1 es una copia exacta de G-L1c.
- **La desviación de CT-PBT2A-04 es aceptable.** Es la lectura literal de «en el texto de cada archivo nuevo», y es más estricta que inyectar en un solo archivo.
- **La CI** del RED y del GREEN coincide con lo previsto.

Los cinco menores no piden rehacer nada. Son huecos de discriminación que ya venían del paquete o del RED del intento 1, textos de documentación y lecturas de invariantes que la verificación debe dejar escritas.

## Tabla de hallazgos

| Id | Sev. | Dónde (sha:ruta:línea) | Hallazgo | Qué hacer |
|---|---|---|---|---|
| PBT2A-IR2-M01 | M | `5ddeb3b4:tests/RackCad.Tests/PushBackDraftStatePortTests.cs:391-421`; paquete `:353-356` y `:767` | **CT-PBT2A-02 no discrimina dos cosas que fija la API.** (a) La línea `launchfor` no mira `SourceProject`: un `LaunchFor` que pasara `null` en vez de `sourceProject` pasaría. (b) Cada caso pasa un solo nulo, así que el **orden** de las tres comprobaciones del constructor (D-PBT2-03, «en este orden») no se ejerce. Las dos cosas son correctas **por lectura** (`PushBackDraftStatePort.cs:64`; `PushBackDraftEditorLaunch.cs:15-17`) y por el chequeo literal de INV-PBT2A-03 del Worker (`rv/inv/inv-base.txt`). El hueco es del §5.2 del paquete, no del Worker. | Nada en PB-T2a (C-2 lo prohíbe). La verificación puede anotar que (a) y (b) los cubre la lectura literal. Si se reabre el paquete, añadir `proyecto-mismo=` a la línea `launchfor` y un caso con dos nulos. |
| PBT2A-IR2-M02 | M | `c33ac6ac` = `5ddeb3b4:tests/RackCad.Tests/PushBackDraftStatePortTests.cs:457-460`, `:68-72` y `:148-152` | **CT-PBT2A-03 podría pasar en vacío para X-01 y F-01.** Si `X01Design` o `F01Design` dan `null`, lo que pasa en silencio cuando `Pasada` traga una excepción (`:72`), la fila usa `new PushBackEditableState()`. Con un estado vacío salen `iguales=True`, `diseno-igual=True` (los dos `<sin diseno>`), `compartidos=0` y `huellas=True` sin ejercer nada. Lo mismo pasa con `diseno-igual` en cualquier diseño cuyas dos pasadas den `<sin diseno>`. **En la práctica no ocurre:** son las mismas recetas que las redes cerradas de PB-T1b ejercen con diseño. Pero no lo puedo medir sin ejecutar. | Nada en PB-T2a (C-2). Para PB-T2b y PB-T2c: si una receta no produce diseño, que dé una línea `EXCEPCION`, nunca un estado vacío. |
| PBT2A-IR2-M03 | M | `5ddeb3b4:src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs:10` y `:28`; `DraftSession.cs:150-156`, `:298`, `:331-332` | **Dos comentarios XML imprecisos** (vienen del esqueleto). (1) `NotEditable` dice «discarded, retired or in error». El núcleo da `NotEditable` también en `Applying` y `Unknown`, y en `Conflict` acepta (`CanEdit`), así que ahí da `Applied`. (2) `IsDirty` dice «differs from its initial state». El `IsDirty` del núcleo también cuenta el texto pendiente. No cambia ningún comportamiento ni ninguna API. | Corregir los textos cuando se toque el archivo (PB-T4). No es parada. |
| PBT2A-IR2-M04 | M | contrato `gate-contract.json:407` y `:410`; `rv/inv/inv-chain.txt` y `rv/inv/inv-base.txt` | **Dos lecturas de invariantes que la verificación tiene que dejar escritas.** (1) **INV-PBT2A-01** no se cumple a la letra contra una sola base. Desde ChainBaseSha, «el diff dentro de las 4 rutas» da False por los cuatro documentos de `17872be8`, el commit de la sesión. Desde BaseSha, «ningún archivo existente de `Systems/PushBack/` cambia» da False, porque el esqueleto ya existe en `17872be8`. Los tres commits del Worker solo tocan las 4 rutas, y desde ChainBaseSha los dos archivos son nuevos (`A`). Se cumple lo que el invariante quiere decir. El Worker lo declara en `UnexpectedFindings`. (2) **INV-PBT2A-04** («Ningún campo estático») no dice su ámbito. Los dos archivos de `src/` cumplen. Las pruebas tienen campos `static readonly`: los de G-L1c, que el paquete manda copiar (`PushBackDraftPortSharedReferenceGuardTests.cs:26-43`), y los dos ensambladores (`PushBackDraftStatePortTests.cs:35-36`), como `95373af6:tests/RackCad.Tests/PushBackEditableStateCopyTests.cs:31-32`. Leo el invariante como de producto. | La verificación escribe las dos lecturas: INV-01 por commit del Worker desde ChainBaseSha, e INV-04 solo sobre `src/`. |
| PBT2A-IR2-M05 | M | `rv/worker-handoff.json`, `Deviations` | **Desviación de procedimiento del Worker, declarada.** Lanzó las corridas largas como tareas en segundo plano y esperó con bucles con `sleep`, contra la letra de su delegación (no tengo el texto de la delegación). Ninguna corrida se solapó. Los resultados son coherentes: los árboles que midió el Worker, `055c3895…` y `dbe89e60…`, son exactamente los de `fb90c823^{tree}` y `5ddeb3b4^{tree}`. No afecta al código. | La sesión decide si es una incidencia que hay que registrar. No es parada del contrato. |

## 0. La corrección

| Pregunta | Respuesta | Evidencia |
|---|---|---|
| ¿Está aplicada C-1 exactamente? | **Sí.** `estado-es-el-actual` (`ReferenceEquals(desdeBorrador.State, draft.CurrentDraftState)`) e `id` se toman antes de `Copy`, de la edición y de `ApplyToDraft`. `puerto-es-del-borrador` se toma después. La línea esperada y la regla «EXCEPCION» no cambian. | `fb90c823:tests/RackCad.Tests/PushBackDraftStatePortTests.cs:410-421` (`:413`, `:414`, `:417` y `:420`) y `:436`; diff `c33ac6ac..fb90c823`: 5+/2-, todo dentro de la lambda `lectura` |
| ¿Se debilita CT-PBT2A-02? | **No.** Las dos comprobaciones miden lo que pide el §5.2, cada una en su momento. | ver las dos filas siguientes |
| ¿`estado-es-el-actual` sigue mirando de verdad? | **Sí.** Compara la identidad con el objeto actual del borrador justo después de `LaunchFor`. Un `LaunchFor` que copiara el estado, o que usara `InitialDraftState` (que es una copia), daría `False`. D-PBT2-02 y D-PBT2-03 piden la referencia sin copiar, y `CurrentDraftState` es por referencia (`DraftSession.cs:287`). | paquete `:329` y `:359` |
| ¿`puerto-es-del-borrador` sigue mirando de verdad? | **Sí.** Exige `Igual(draft.CurrentDraftState, edited) && draft.IsDirty`. El borrador de CT-02 está limpio hasta ese momento (`puerto` no se usa), y `PalletTolerance = 4.5` difiere de la base (CT-01 lo confirma con `sucio=True`). Un puerto de otro borrador, o uno que no hiciera nada, daría `False`. | `:415-420` |
| ¿Sigue sin pasar sobre el esqueleto? | **Sí.** `LaunchFor` da `null`, la lectura lanza `NullReferenceException` en `:413` y se convierte en una línea `EXCEPCION`. La CI del RED la muestra (`Actual: [... "EXCEPCION NullReferenceException: Object reference"...]`). | `rv/red-ci-core/core.trx`; `rv/inv/red-messages.txt` |
| C-2: `git diff c33ac6ac fb90c823 -- tests` | Solo `tests/RackCad.Tests/PushBackDraftStatePortTests.cs`. | `git diff --stat` |
| C-2: `git diff c33ac6ac fb90c823 -- src` | **Vacío.** | `git diff --stat` |
| C-2: ¿CT-01, CT-03 y la guarda, como en `c33ac6ac`? | **Sí.** La guarda tiene el blob `5ed45fad` en `c33ac6ac` y en `5ddeb3b4`. En la clase corregida no cambia nada fuera de `:412-420`. `fb90c823..5ddeb3b4` no toca `tests/`: el blob `d4f34e42` es el mismo. | `git rev-parse` |
| La desviación de CT-PBT2A-04 | **Aceptable, sin debilitar nada.** Cada autoprueba se inyecta en los dos archivos, y `falla` exige que las reglas la vean en **todos** (`texts.All(...)`, `:220`). Si falta un archivo, todas dan `pasa` y la prueba falla. Es más estricto que inyectar en uno solo: detecta un archivo cuya forma ciegue las reglas. Las 15 líneas esperadas son las del §5.2. El único coste es de diagnóstico: la línea no dice qué archivo falló. | `5ddeb3b4:tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs:217-222`; paquete `:738` y `:769` |

## 1. Alcance y forma

- **Commits del Worker:**
  - `c33ac6ac` toca las 4 rutas del AllowedWriteScope;
  - `fb90c823` toca una (la clase corregida);
  - `5ddeb3b4` toca las dos de `src/`.
- `17872be8` es de la sesión: solo `docs/`.
- `git diff --name-status 0dc623ca 5ddeb3b4 -- src`: los dos archivos en `A`. No cambia nada de UI, Plugin, Domain, `Workspace/` ni `Pilot/`, ni ningún archivo existente de `Systems/PushBack/`.
- **El RED de la cadena es el esqueleto exacto del §5.2** (`c33ac6ac`):
  - el enum, el resultado y la interfaz, completos;
  - el constructor del adaptador, vacío;
  - `ApplyToDraft` → `new PushBackDraftApplyResult(NotEditable, false)`;
  - `LaunchFor` → `null`;
  - el constructor del lanzamiento, vacío.
- **El GREEN** solo toca los dos archivos de `src/`.
- **C13:** las declaraciones son exactamente las de la lista cerrada, más los constructores:
  - en el puerto: `PushBackDraftApplyStatus`, `NotEditable`, `Applied`, `PushBackDraftApplyResult`, `Status`, `IsDirty`, `IPushBackDraftStatePort`, `ApplyToDraft`, `PushBackDraftSessionPort`, `draft` y `LaunchFor`;
  - en el lanzamiento: `PushBackDraftEditorLaunch`, `State`, `RackId`, `SourceProject` y `Port`.

  `result` es una variable local, no un miembro.
- **INV-03:** las cinco sentencias de excepción son literales. El único miembro privado es `draft` (`:44`).
- **INV-04:** en `src/`, ningún campo estático y un solo namespace de bloque.
- **INV-05:** ningún token prohibido. Lo comprobé con grep sobre los dos blobs, y los dos son ASCII.
- **INV-06:** una sola llamada, `draft.SetCurrentDraftState(edited)` (`:55`).
- **Las lecturas de INV-01 y de INV-04:** ver M04.

## 2. El diseño frente al código

| Diseño | Código (`5ddeb3b4`) | ¿Igual? |
|---|---|---|
| D-PBT2-02, el enum `{ NotEditable = 0, Applied = 1 }` | `PushBackDraftStatePort.cs:8-15` | Sí |
| D-PBT2-02, `readonly struct PushBackDraftApplyResult(status, isDirty)` con `Status` e `IsDirty` | `:18-30` | Sí (M03, solo el texto) |
| D-PBT2-02, `IPushBackDraftStatePort.ApplyToDraft(PushBackEditableState)`, sin tipos de `Drafts` | `:33-36` | Sí |
| D-PBT2-02, el constructor: lanza con `draft` nulo y guarda `private readonly ... draft` | `:44-50` | Sí |
| D-PBT2-02, `ApplyToDraft`: lanza con `edited` nulo; solo `SetCurrentDraftState`; `Updated`→`Applied`, `NotEditable`→`NotEditable`; el `IsDirty` del núcleo | `:52-59`; `DraftSession.cs:345-350` | Sí. El adaptador no copia ni valida |
| D-PBT2-02, `LaunchFor`: lanza con `draft` nulo; `new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, sourceProject, new PushBackDraftSessionPort(draft))` | `:61-65` | Sí, literal |
| D-PBT2-03, el constructor: `state`, `rackId` (texto de DR:145) y `port`, en ese orden; `sourceProject` puede ser nulo; no copia | `PushBackDraftEditorLaunch.cs:13-22` | Sí (el orden lo fija la lectura, M01) |
| D-PBT2-03, las cuatro propiedades de solo lectura | `:24-31` | Sí |
| F4B-E28, pregunta 2: el rechazo de un diseño bloqueante | El puerto **no valida** (D-PBT2-02, «Lo que el adaptador no hace»). El rechazo es de `ApplyEditsToEditingState` en la ventana (PB-T2b, CT-PBT2B-05) | Respetado por construcción; nada en PB-T2a lo puede incumplir |
| V2-07 (IR1 I-07): «Aplicar» sin `SetName` ni `SyncName` | Ninguno de los dos archivos nombra `SetName`, `SyncName` ni `Name` | Sí |

**¿Algún comportamiento cambia sin estar declarado?** No. No hay ningún llamador de producto de los tipos nuevos. RACKEDITAR, RPB, el menú, la ventana, el piloto y el host no cambian (C03 no se dispara).

## 3. D2 y su límite

- **¿Alguna línea muta el rack resuelto o el informe de módulos?** **No.** Los dos archivos no nombran `PushBackSystem`, `RackModuleReconciliationResult`, `WorkingBaseline` ni `LastModuleReconciliation`. La guarda da `reglas-sin-coincidencias=True` en el RED y en el GREEN. El núcleo que llama el puerto (`Workspace/Drafts/DraftSession.cs`) sigue bajo CT-PBT1B-14, que recorre todo `src/RackCad.Application/Workspace` (`95373af6:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:189-197`).
- **¿El puerto devuelve al borrador una copia sin alias?**
  - **Sí.** `SetCurrentDraftState` guarda `_copy(state)` (`DraftSession.cs:348`).
  - Lo fijan:
    - CT-01 (`mismo-objeto=False`, `mutar-tras-aplicar sin-cambio=True`);
    - CT-03 (`compartidos-fuera-de-D2=0`, `huellas-iguales=True` en los 18).
  - **Por diseño, la copia la hace el copiador del borrador, no el adaptador** («no copia: copia el borrador»). La garantía depende de que quien abra el `DraftSession` inyecte `PushBackEditableStateComparer.Copy` (A2-4, PB-T4).
- **El lanzamiento lleva `CurrentDraftState` por referencia,** según D-PBT2-03 («no copia; copia la ventana»), y CT-02 lo exige. Es lo que pide el paquete. Para PB-T2b, la `Copy` del constructor privado (D-PBT2-05) es lo que impide editar S en sitio. No es un defecto de PB-T2a.
- **¿G-L1 aplica las reglas y las autopruebas exactas?** **Sí.**
  - **Las reglas:** `diff` de `95373af6:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:33-207` contra `5ddeb3b4:tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs:26-182`. Las diferencias son solo `GuardedFiles`, que tiene los dos archivos nuevos. `Tokens`, `Rebind`, los patrones, `Rule1`, `Rule1b`, `Rule1c`, `Clean`, `Collapse`, `Members`, `Aliases`, `Violations` e `Inject` son idénticos byte a byte.
  - **Las 15 autopruebas** (`:198-214` contra G-L1c `:223-239`) son idénticas, (k1) y (k2) incluidas. Solo cambia un comentario.
  - **G-L1c no cambió** entre `95373af6` y `5ddeb3b4`.
  - **Las 15 comunes de CT-PBT1C-04** (`PushBackWindowSharedReferenceGuardTests.cs:211-228`) son las mismas. La (d) es propia de la ventana y no se pide aquí (V21-06).
  - **No se deja nada.**

## 4. Los CT

| CT | ¿Fija lo que dice el §5.2? | Aserciones que no discriminan, o huecos |
|---|---|---|
| 01 | **Sí**, las ocho líneas literales y en orden (`:364-376`) | `inicio sucio=False`, `mutar-tras-aplicar`, `error` y `descartado` también salen en el esqueleto. Son líneas de contexto o de rechazo; discriminan junto con `aplicar ... Applied` y `volver ... Applied sucio=False` (un `IsDirty` constante, o leído antes de la llamada, falla en una de las dos). No hay tautologías. |
| 02 | **Sí**, las nueve líneas (`:426-439`), con C-1 | M01: `SourceProject` en `LaunchFor` y el orden de las comprobaciones no se discriminan. `proyecto=<null>` también sale en el esqueleto, pero `proyecto-mismo=True` cubre el caso contrario. |
| 03 | **Sí**: el C18 en el orden de CT-PBT1B-02 (16 fixtures ordinales sin `pb-21-cola`, más X-01 y F-01), los pasos 1 a 5, las huellas antes del paso 4 y `n` como CT-PBT1B-07, parte 1. `Huella`, `AllObjects`, `IsImmutable`, `Mutable`, `InstanceFields` y `SharedRoots`, copiados byte a byte de `95373af6:.../PushBackEditableStateCopyTests.cs:247-336` y `:1080` (`Fingerprint` pasa a `Huella`) | M02: el posible vacío de X-01 y F-01. `compartidos` y `huellas` también salen en el esqueleto: miden el copiador de PB-T1b (C15), y el RED falla por `iguales=False diseno-igual=False`. |
| 04 | **Sí**: `reglas-sin-coincidencias=True` y las 15 `falla` (`:224-232`) | Ninguno: la desviación es más estricta (§0). |

**Las recetas:**
- `Estado`, como en el §5.0 (`:84-94`);
- `Pasada`, la de [PT1] §5.0 con los ensambladores en campos (la misma forma que en PB-T1b);
- `Borrador`, literal (`:135-146`);
- `Catalogo` = `PilotKit.Catalog` = `JsonRackCatalogProvider.FromBaseDirectory().Load()`.

## 5. El RED de la corrección (`fb90c823`)

- **La CI del RED (`rv/red-ci-core/core.trx`):**
  - Core: 19 092 en total, 19 089 `Passed` y 3 `Failed`, que son exactamente CT-PBT2A-01, 02 y 03.
  - Los tres fallan con `Assert.Equal() Failure: Collections differ`, y su pila empieza en la propia prueba (`:364`, `:426` y `:495`). **Todo es por aserción** (C04 no se dispara).
  - Esas líneas son las de `fb90c823`. En `c33ac6ac`, CT-02 y CT-03 estarían en `:423` y `:492`, así que la CI corrió la clase corregida sobre el esqueleto.
- **La guarda:** `Passed`.
- **Ninguna prueba existente falla:** 19 089 = 19 088 de la base + la guarda.
- **UI (`rv/red-ci-ui/ui.trx`):** 2 439 en total, 2 418 `Passed`, 0 `Failed` y 21 sin ejecutar, como la línea base.
- **En local** (`rv/tests/r1/r1-statePort.trx`, `rv/tests/r2/r2-core-full.trx`): lo mismo, 3/3 en fallo por aserción y 19 089 de 19 092.

## 6. El GREEN y la estabilidad

- **La CI del GREEN (`rv/ci-core/core.trx`, `rv/ci-ui/ui.trx`):**
  - Core: 19 092 `Passed`, 0 `Failed` = 19 088 + 4;
  - UI: 2 439 en total, 2 418 `Passed` y 21 omitidas, sin cambio.

  Es la línea base del contrato (`gate-contract.json:536`). El TRX de UI de la CI del GREEN es otra corrida (otro `TestRun id` y otra hora), no una copia de la del RED.
- **En local:**
  - las tres corridas del filtro con ExpectRed, 3/3 cada una (`g1`, `g2`, `g3`);
  - Core completo, 19 092 `Passed`;
  - los filtros del contrato, con sus mínimos (`Workspace` 3 482 ≥ 3 271).
- **Los avisos de build:** iguales en RED y GREEN (Core 132, UI 28, Plugin 104 `MSB3277`), y ninguno nombra los archivos nuevos.
- **Dependencias:**
  - **Orden:** el corpus va en orden ordinal, y cada prueba tiene su `DraftRegistry`.
  - **Cultura:** las huellas usan «R» e invariante. Los mensajes de excepción son los del runtime en inglés, y pasaron igual en local (sistema en español) y en la CI (Linux).
  - **Estado estático:** solo los ensambladores `static readonly`, con campos `readonly`, privados de la clase, cuyas pruebas no corren en paralelo entre sí. Es el patrón de PB-T1b.
  - No veo ningún riesgo.

## 7. Contención

- Las pruebas de PB-T2a no escriben en disco: solo leen los fixtures y el texto de los dos archivos.
- No hay ventanas, modales ni AutoCAD.
- El Plugin no cambia. El build del Plugin salió a una carpeta de `artifacts/` de la corrida, no al perfil ni a AutoCAD.
- La prueba de limpieza de antes y de después: ver «Lo no verificado».

## 8. Hallazgos para el Owner

**Ninguno.** No hay ninguna diferencia observable sin declarar: los tipos nuevos no tienen ningún llamador de producto, y CLASSIC, la ventana, el piloto y el host no cambian.

Para informar, sin pregunta: la garantía de F4B-E28, pregunta 2 («el borrador nunca guarda un diseño que no se puede dibujar»), es de la ventana (PB-T2b) y del envoltorio del host (A2-1, PB-T4). Según D-PBT2-02, el puerto público acepta cualquier estado.

## Lo no verificado

- **No se ejecutó nada** (ni dotnet ni pruebas). Todo es por lectura de objetos git y de los TRX y salidas copiados.
- **Las conclusiones de los cuatro jobs** de las CI `44478438623` (RED) y `44480898605` (GREEN): la copia solo trae los TRX. Que `build-plugin` saliera omitido en el RED y que el GREEN diera 4/4 no lo comprobé. Los TRX no llevan el SHA. La CI del RED la ato a `fb90c823` por las líneas de pila y la hora; la del GREEN, solo por la hora (07:36 UTC el commit y 07:37 UTC la corrida).
- **La prueba de limpieza** (`clean-before.txt` y `clean-after.txt`) no está en la copia. Que el perfil y `RackFrameConfigurator.layout` no cambiaran es lo que dice el Worker.
- **El texto de la delegación** (para M05) y el parche `green-wip-uncommitted.patch` del intento 1 no se leyeron: viven fuera de los objetos git o en el worktree.
- **El posible vacío de M02** (que X-01 y F-01 den diseño en esta clase) no se puede medir sin ejecutar.
- **La afirmación del Worker** de que no tocó AutoCAD no es verificable desde aquí.

**Una incidencia de procedimiento del revisor:** al leer `worker-handoff.json`, lancé por error un `python -I -` con un heredoc **vacío**, contra la regla «Python nunca con `-c` ni con un heredoc». No ejecutó ningún código, se quedó esperando y lo detuve. No tuvo ningún efecto: no leyó ni escribió nada. No se repitió.

Como ayuda de lectura, extraje con `git show` copias de objetos (el paquete, G-L1c y las clases) a una carpeta del scratchpad, para compararlas con `diff`. No se escribió nada más.
