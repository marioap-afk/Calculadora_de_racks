> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — PB-T3: de la v4.4 a la v4.5 (la decisión del Owner F4B-E42)

**Base:** `I-64-f4b-pbt3-gate-contract-package-v44.md`, sha256 `a96063acfe857625c85c3b23f37d17026e0bc2f6b28105ccc2c00898bfc36c14`, 256 921 bytes, LF; la custodiada en `2d876831`.
**Resultado:** `I-64-f4b-pbt3-gate-contract-package-v45.md`, sha256 `64ab03df27cc8f441883442ca999036e5f7555ede9269eb3f4f768ab8a07fdbd`, 282 234 bytes, LF, con V45-06 (la §11 de CA-T3a, M-06). Antes de V45-06 era `6941108474a74cc1c7bdeeec8b3ef07c1b6d885bdb5a2459aec9566279bc374f`, de 281 130 bytes.
**El código,** leído por objetos git en `18e4b783` (`git -C <checkout-principal> show 18e4b783:<ruta>`), sin abrir ningún worktree.

## 1. Origen

- **La parada:** el Worker `R20261120T001738Z-7d92` de PB-T3a (delegación `R20261120T001442Z-903c`, intento 1) paró con **PBT3A-C02 antes del commit RED**, sin commits ni push. HEAD sigue en `18e4b783`.
- **Lo que falla:** `WorkspacePilotUpdateIntegrationGuardTests.G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts` (:703), sobre el esqueleto exacto de la v4.4.
- **La causa, un defecto del paquete:** `Draft/PushBackFullComposition.cs` nombra el piloto (`PushBackPilotMaterializer.NonPilotContentReason`, que reconoce `NamesThePilot`, :679) y declara `ForWrite`, el nombre cerrado de D-PBT3-05. Ese nombre es el término `\bForWrite\b` de G-9 (:603), y G-9 barre todo Application y el Plugin (:691).
- **La decisión del Owner** (F4B-E42; `owner-pbt3a-g9-decision.md`): «Renombrar ForWrite (Recomendado)». G-9 no se toca.

## 2. El nombre: `ForFullWrite`

`public static PushBackFullWriteComposition ForFullWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog);`

**Por qué ese:**
- dice lo mismo que antes (la composición para la escritura del modelo completo) y casa con su tipo de resultado, `PushBackFullWriteComposition`;
- **no coincide con ningún término de guarda.** No es el token `\bForWrite\b` (G-9, `IntegrationGuard:603`; `UpdateGuard:195`; `WorkspaceEditIntentGuardTests:290`; `CustomPropertiesEdgeGuardTests:480`). Tampoco es `\bWrite\s*\(` (`CustomPropertiesEdgeGuardTests:480`), porque dentro del identificador no hay `\b` delante de `Write`;
- **el `Write` suelto:** ninguna guarda prohíbe un `Write` suelto sobre el producto. Los dos literales `"Write"` (`CustomPropertiesEdgeGuardTests:622` y `:728`) comparan el nombre exacto de un miembro del acceso físico y del ejecutor de las propiedades personalizadas, dos archivos fijos. El único patrón amplio con `Write` es `File\.Write\w*`;
- **comprobado con todos los literales:** los 22 346 de los 164 archivos de `tests/` que leen código, contra las líneas donde aparece. Solo coinciden expresiones genéricas que extraen identificadores o sentencias, y no prohíben nombres;
- **libre:** ningún archivo de `src/` ni de `tests/` lo usa en `18e4b783`.

## 3. Los cambios, por apartado

| Fila | Dónde | Qué |
|---|---|---|
| V45-05 | Título y cabecera | v4.5. El estado se mide en `18e4b783`: PB-T2 completa (F4B-E40), PB-T3a emitida con la v4.4 (F4B-E41), parada del intento 1 (F4B-E42) y reemisión en el intento 2. La recomprobación de CA-T3a (§10, EMITIBLE 0/0/1) se añade a la lista: son ocho revisiones |
| — | Registro v4.4 → v4.5 (nuevo) | V45-01 a V45-05 |
| V45-05 | Precondición 1 | Una viñeta más: PB-T2c CERRADA (F4B-E40, `f273eae1`; CI `44619492077`, Core 19 094 y UI 2 450). La remedición de F4B-E41 no cambió nada material |
| V45-05 | Precondición 9 | Se custodian la v4.5 y `v45-delta.md`; en la reemisión, también la decisión del Owner, el análisis de la sesión y la comprobación acotada de la v4.5 |
| V45-04 | Precondición 11 (nueva) | La reemisión de PB-T3a: la comprobación acotada de la v4.5, a 0/0; la remedición (`git diff f273eae1 <BaseSha> -- src` vacío); y F4B-E42 en la `AuthorityRevision` nueva |
| V45-01 | D-PBT3-05 | El API cerrado (el bloque de código), el texto «`ForFullWrite`, pura y total», con su nota del nombre de la v4.4, y la fila 4 de la tabla |
| V45-01 | D-PBT3-09 | `BuildFull` llama a `PushBackFullComposition.ForFullWrite(`, con la nota de por qué: con el nombre viejo, PP nombraría el piloto y el término de G-9 |
| V45-02 | §4, la fila de G-9 | Se corrige el «Cómo»: G-9 barre todo Application y el Plugin; los archivos de `Draft/` que nombran el piloto (el materializador, el lector y la composición) y PP no llevan ningún término con el nombre nuevo |
| V45-02 | §4.1 (nueva) | El barrido entero (el §4 de esta nota lo resume) |
| V45-03 | §5.0 | La lección «los archivos nuevos se comprueban contra las guardas existentes antes de fijar el esqueleto»: qué se comprueba, cómo, cuándo y lo que la hizo falta |
| V45-01 | §5.2 | CT-PBT3A-06 (el rótulo) y CT-PBT3A-11 (la lista de operaciones y, por ella, la `<operacion>` de su línea) |
| V45-01 | §5.3 | La justificación de I-02 (`ForFullWrite`, en PB-T3a, termina en el editor y las entradas) |
| V45-04 | §6.0, «El RED de la cadena en una corrección» | La viñeta de PB-T3a dice que `git restore --source=<RED del intento 1>` vale si el intento anterior acreditó un RED; si no, el RED es el entero |
| V45-04 | §6.0, «La reemisión de PB-T3a en el intento 2» (nuevo) | El análisis de la sesión, la clasificación (`StopCondition`, `ChainRedSha` null, `ChainRedFiles` vacío, intento 2 de 3), la corrección C-1, el RED entero, los parches del intento 1 con el nombre cambiado y la reemisión |
| V45-01 y V45-05 | §6.1 | El `Objective` dice «v4.5», y el esqueleto, `ForFullWrite devuelve Rejected(...)`. **Es lo único que cambia en §6.1** |
| V45-05 | §6.2 y §6.3 | El `Objective` dice «v4.5» |
| V45-01 | §7, R-T3-07 | «en `ForFullWrite`, la materialización otra vez» |
| V45-05 | §8.1 | Gana F4B-E42; la viñeta de D-PB-10 dice que PB-T3a va en el intento 2 |
| V45-06 (CA-T3a §11 M-06) | §6.0, la identidad | «attempts 1 de 3» pasa a: cada subtarea empieza en 1 de 3, y PB-T3a, en su reemisión, va en el intento 2 de 3 |
| V45-06 | §6.0, la reemisión (la clasificación) | `BaseSha` y `ChainBaseSha` = el commit BASE de la reemisión, con el precedente de F4-T4A1 (el análisis de la sesión, §3); en `src/` y `tests/` igual a `18e4b783`; la cadena se mide con `-- src tests`; `attempts` 2 |
| V45-06 | D-PBT3-01 | El presupuesto de 1/3 dice que PB-T3a va en el intento 2 desde F4B-E42 |
| V45-01 | §8.3 | «`Origin`, que `ForFullWrite` y `KeepOn` necesitan» |

**Lo que no se renombra:**
- los `ForWrite` que son `OpenMode.ForWrite` de la fachada de PB-T3c: D-PBT3-11 (paso 3 y «Lo que sigue en verde por su letra») e INV-PBT3C-03;
- la historia de los registros (V2-21).

**Comprobado:**
- ningún bloque de código lleva ya `ForWrite`;
- cada `\bForWrite\b` que queda es uno de esos o es texto de análisis de la v4.5 (el registro, §4, §4.1, §5.0, §6.0, la precondición 11 y §8.1);
- `check_tables.py`: 0 problemas;
- los tres `Objective`, ASCII entre las comillas;
- LF, sin CR;
- §6.0 sigue con 6 notas «no va al contrato», y §6.1 con 3.

## 4. El barrido de colisiones (V45-02; §4.1 del paquete)

**Qué entra:**
- **los archivos:** los nuevos y tocados de PB-T3a (los ocho de `Draft/`), PB-T3b (PP, UR y `Pilot/IPushBackPilotViewEraser.cs`) y PB-T3c (WR y `Plugin/Views/RackViewBlockEraser.cs`), con el texto que les da el paquete;
- **las guardas:** las 91 clases que barren una carpeta o todo `src/`, más las siete que llaman a sus ayudantes compartidos;
- **el método:** cada coincidencia se leyó en su prueba, por su alcance y su sentido.

| Guarda | Término | ¿Lo tiene un archivo nuevo? | Resultado |
|---|---|---|---|
| G-9 (IntegrationGuard:600-608, :677-699) | Los 20 términos de escritura (entre ellos `\bForWrite\b`), en los archivos de Application y del Plugin que nombran el piloto | **Sí, con la v4.4:** `ForWrite` en `Draft/PushBackFullComposition.cs` y en PP (`BuildFull`) | **Colisión, la única. Resuelta por `ForFullWrite`** |
| G-1, G-3 a G-7 (IntegrationGuard) | Los censos de las primitivas, del runner, de `TryUpdatePilot`, del API de escritura del adaptador y de las llamadas del puerto (solo en UR) | No | Sin colisión |
| El escritor (UpdateGuard:182-215) | `\bErase\s*\(`, `GetBlockReferenceIds`, `EraseViewBlocks`, `\bForWrite\b`, `UpgradeOpen`… en WR | No: van en la fachada | Sin colisión |
| `Pilot/` sin `Transaction`; la pureza de UP, UR y SR | El texto crudo, comentarios incluidos | No | Sin colisión |
| PrepareGuard | La lista prohibida de PP (`OpenMode.ForWrite`…), sus tipos, los que componen, `ComposeForWrite` | No | Sin colisión |
| EditIntent | Lo que escribe el adaptador; los censos de `SetEditIntentReader(`, `new DraftRegistry(`, `.ReadIntent(`… | No | Sin colisión |
| La navegación (Host, Authority, Privacy) | Los censos de sus tipos y de los 16 nombres; `GetBlockReferenceIds(directOnly: true` solo en su adaptador | La fachada usa esa llamada, pero la regla no la lee | Sin colisión |
| `WorkspaceHostSourceGuardTests` | Las API del lector (`GetObject`, `RackEmbedStore`…) solo dentro de `Plugin/Workspace/`; los censos del anfitrión | El asiento usa `new RackEmbedStore().Deserialize(`, y WR y la fachada `GetObject`, todos fuera de esa carpeta | Sin colisión |
| Los puentes | Los enganches de eventos fuera de `Plugin/Workspace/`; los nombres del puente en el Plugin | No | Sin colisión |
| Los demás del panel | Los censos de sus tipos y llamadas | No | Sin colisión |
| `PushBackSharedReferenceGuardTests` CT-PBT1B-15 | Los archivos del Plugin con `PushBack` (14) y sus asignaciones de `Name` (7) | No | Sin colisión, **si la fachada sigue sin `PushBack`** |
| Los sobres (TGrd01, T-ENV-12) | Construir un `RackEmbedDocument` fuera del compositor; las 11 composiciones | No | Sin colisión, **si ningún ayudante devuelve un sobre con `return new(`** |
| Las propiedades personalizadas | Reglas por ruta, `\bAutodesk\b` fuera del Plugin, la clave del NOD; sus `Write` y `ForWrite`, solo en sus archivos fijos | No | Sin colisión |
| `NamespaceFolderGuardTests` | El `namespace` igual a la carpeta; el `using` suelto de `RackCad.Application.Systems` | No | Sin colisión, **si los `namespace` son `...PushBack.Draft`, `...PushBack.Pilot` y `RackCad.Plugin.Views`** |
| `Autodesk` fuera del Plugin | `\bAutodesk\.` en Application, UI y Domain | No | Sin colisión |
| Las unidades | `25.4`, los tokens de unidad, los nombres de rol; `\bProviders\b` en `Systems/` | No | Sin colisión |
| Los censos de órdenes | `[CommandMethod(` (40) | No | Sin colisión |
| Los demás censos de `src/` o del Plugin | Elevaciones, `cellOverride ?? frontDefault`, `RackDefinitionCreator`, `Insunits`, las secciones, `CantileverLineResolver`, RAM Elements | No | Sin colisión |
| Las de archivos fijos | Archivos con nombre o carpetas que PB-T3 no toca | — | No aplican |

**No hay otra colisión.** Las tres «exige» de la tabla ya las cumplía el diseño (D-PBT3-11, INV-PBT3A-04, D-PBT3-02): no son decisiones nuevas.

## 5. Lo que no cambia

- el diseño, D-1 (la variante (A), F4B-E29), las listas cerradas CT-PBT3A-01..14, CT-PBT3B-01..15 y CT-PBT3C-01..05, sus `MinSelected` y las previsiones del GREEN (PB-T3a: Core 19 107 y UI 2 451; PB-T3b: 19 128; PB-T3c: 19 133);
- G-9 y ninguna otra prueba existente;
- la tabla de RR-3, que no tenía el nombre;
- la regla RR, PBT3x-C11 y P-01 (la Snapshot K).

## 6. Lo que el generador tiene que cambiar (`make_pbt3a_contract.py`, custodiado en `18e4b783`)

1. **`PKG_VERSION = 'v4.5'`**, con la docstring y los comentarios que dicen v4.4 (`:1`, `:9`, `:26`, `:38`, `:46`, `:49`, `:91`, `:93`).
2. **`CUSTODY_SHA256`:** `64ab03df27cc8f441883442ca999036e5f7555ede9269eb3f4f768ab8a07fdbd` (282 234 bytes, LF).
3. **`SKELETON` (`:1020-1026`):** `ForWrite devuelve Rejected(PushBackPilotMaterializer.NonPilotContentReason)` pasa a **`ForFullWrite devuelve ...`**. Es lo que compara con la viñeta del esqueleto de §6.1.
4. **`CLOSED_EXPECTED['PushBackFullComposition']` (`:1213`):** `['ForFullWrite', 'KeepOn']`. `closed_list` lee el bloque de código de D-PBT3-05, y `CLOSED_COUNT` no cambia.
5. **`OBJECTIVE` (`:1016`):** «segun el paquete del contrato de PB-T3 v4.5». El `assert ('PB-T3 ' + PKG_VERSION) in OBJECTIVE` lo sigue.
6. **La custodia:**
   - `CUSTODY` y `CUSTODY_EXTRA` ganan `v45-delta.md`, la decisión `F4B-DRAFTS/coordinator/owner-pbt3a-g9-decision.md`, el análisis del intento 1 y la comprobación acotada de la v4.5, con los recuentos de su `assert` (`len(CUSTODY) == 6`, `len(CUSTODY_EXTRA) == 27`) al día;
   - **`CUSTODY_FREE`, en la reemisión:** solo los nombres nuevos tienen que estar libres. Los de la v4.4 ya están en `2d876831`.
7. **La autoridad del paquete (`:1732-1733`):** los registros v1 → v4.5, con V45-01 a V45-05, las precondiciones 1 a 11 y el §4.1.
8. **`BOUNDED_RESULT` y `--bounded-file`:** el literal de la comprobación acotada de la v4.5, como fila nueva junto a la de CA-T3a (`:1745`), que sigue.
9. **`IssuedBy`:** la reemisión por F4B-E42, la corrección en el intento 2 de 3, con F4B-E41 (la primera emisión) y F4B-E42 (la decisión, como autoridad `UNIT_DOC`).
10. **Lo que no hay que tocar, comprobado:**
    - `PT2C_CLOSE`, `CI_BASE_T2C`, `CI_LINE_PKG` y la línea base de §6.1;
    - las seis notas de §6.0 y las tres de §6.1;
    - los 31 textos copiados (`_copied`): ninguna viñeta nueva de §6.0 empieza por `07.` a `14.` ni por `Cnn`;
    - la regla RR y la tabla de RR-3;
    - el título, que el regex `, (v\d+(?:\.\d+)?)\s*$` lee como `v4.5`.

    Hay que comprobar con la corrida en seco que ningún `one(B60, …)` ni `rr_pkg_check` lee la viñeta de PB-T3a de «El RED de la cadena en una corrección», que ahora es más larga.

**En la delegación, no en el contrato** (los generadores de `pbt3acc`):
- `Attempt` 2 de 3, `FailureClass` `StopCondition`, `ChainRedSha` null, `ChainRedFiles` vacío y `ChainBaseSha` igual a la `BaseSha` nueva, el commit BASE de la reemisión (V45-06, precedente de F4-T4A1);
- el análisis de la sesión;
- los tres parches del intento 1, con su ruta y su sha256, y la orden de cambiar el nombre (§6.0 del paquete).

## 7. Para la sesión, no para el paquete

- **R-6 del registro custodiado** (`docs/initiatives/I-64-f4b-pbt3-measurements.md`). Su fila de G-9 dice «PB-T3b no toca el Plugin», y lo que barre G-9 es todo Application y el Plugin. Lo corrige la remedición de la reemisión, sobre el registro custodiado y no sobre la copia de la v3 de esta carpeta: el paquete ya lo dice (§4, §4.1 y §5.0).
- **Los parches del intento 1:** el paquete los nombra por su nombre de archivo. La ruta y el sha256 los pone la sesión en la delegación.
