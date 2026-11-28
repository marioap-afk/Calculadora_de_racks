# I-64 — Comprobación de las filas abiertas en la v6 del paquete de F4-T4B

**Objeto:** la v6 de `I-64-f4-t4b-gate-contract-package.md` (frente a `_package-v5.md`), solo en las filas que dejó abiertas `independent-check-v5.md`:
- N-1 a N-6;
- los restos de m-5 y m-7;
- la precisión opcional de I-1.

La disposición está en el §10.5 de la v6.

**Base:** `7c29a5e5`. Solo lectura con `git show` y `git grep`. No se compiló ni se ejecutó nada. «v6 `:N`» es la línea N del paquete, y las rutas son relativas al repositorio.

**Método:**
- `diff` de la v5 frente a la v6.
- Lectura de cada pasaje corregido.
- Contraste con:
  - `src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs`;
  - `src/RackCad.Plugin/RackPushBackCommands.cs:183-345`;
  - `src/RackCad.Application/Systems/Shared/RackViewBaseName.cs:161-191`;
  - `src/RackCad.Plugin/RackCommandSupport.cs:103-104`;
  - `src/RackCad.Application/BlockNaming.cs:12-17`;
  - `src/RackCad.Plugin/Views/RackSiblingScan.cs`.
- `validation-v6.json` (99/99 PASS) se tomó como insumo.

## Estado por fila

| Fila | Estado | Comprobación | ¿Defecto nuevo? |
|---|---|---|---|
| N-1 | **RESUELTO** | El campo es `ForeignScopeOpenAtPost` en sus cuatro apariciones (v6 `:287`, `:340`, `:395`, `:914`), y no queda `TopTransactionAtPost` salvo en la fila de disposición (`:938`). Lo que el paquete pone en código de la corrida, del puerto o de la ayuda ya no lleva la palabra: tipos `PushBackPilot*`, `sealedReturned`, `RejectionCode`, `DesiredNames`, `NamesByKey`, `RegenFailure`, `IsComposite` y `RackViewBaseName`/`RackViewCodec`/`BlockNaming`. `TOP_TRANSACTION_OPEN` como valor no activa la regla, que distingue mayúsculas | Uno menor: N-7 (el identificador de la constante no está fijado) |
| N-2 | **RESUELTO** | Los dos ámbitos se liberan en `finally` propios, el de `m` dentro del de `scope` (v6 `:309`, `:326`, `:358-359`). La fila del `Dispose` exterior cubre ya todas las salidas (`:354`), y la prueba con el doble cubre el `Dispose` exterior fuera de POST (`:364`). Un CommitThrew o un `ABORT_FAILED` ya no pueden acabar en NotAppliedFailure | Uno menor: N-8 («se añade a la causa» no vale para todas las formas de `DraftApplyOutcome`) |
| N-3 | **RESUELTO** | (d) dice `m = start.Scope`, liberado en el `finally` de (6) (v6 `:326`). No queda ningún `using (var m` ni `using (var scope` en el paquete. Es coherente con INV-14 (`:624`) y con «los `finally` de la corrida» entre las guardas que fallan en el RED (`:493`) | No |
| N-4 | **RESUELTO** | La precedencia está en `:360-363`. Con `DraftApplyOutcome`: CommitThrew lleva causa (`DraftApplyOutcome.cs:82`), así que «CommitThrew por `Seal`, con la causa del aborto añadida» es expresable. `ABORT_FAILED` solo se aplica desde un NotAppliedFailure provisional con algún `WriteUnit` empezado. Es coherente con la fila `:348`: después del primer `WriteUnit` las únicas salidas son el fallo de una unidad, `Seal` lanza o `Seal` vuelve | El mismo N-8 para «un Conflict … sigue siéndolo»: NotAppliedConflict no tiene causa a la que añadir |
| N-5 | **RESUELTO** | La regla por vista (v6 `:401-406`) coincide con el clásico: `base` = `Name.Trim()` o nulo (`RackPushBackCommands.cs:189`); planta `LinkedPlanta(base)` (`:282`); corte `LinkedPushBackCut(base, end, side, system.IsComposite)` (`:300-309`); lateral `LinkedLateral(base, postIndex)` (`:339-342`). Pasar el extremo y el lado tal como salen de `RackViewCodec` da lo mismo que el mapeo del clásico, porque `LinkedPushBackCut` trata como entrada-salida todo lo que no es `Posterior`, y como A todo lo que no es B (`RackViewBaseName.cs:180-191`). El clásico decodifica con el mismo `RackViewCodec.Decode` (`RackCommandSupport.cs:103-104`). Con `base` nulo no hay nombre deseado ni comparación, y se explica por qué: `SanitizeBlockName(null)` da `"Cabecera"` (`:338`, `BlockNaming.cs:14-17`) | No. **Precisión opcional:** decir que `postIndex` es el `Variant.Index` del mismo `RackViewCodec` (`RackPushBackCommands.cs:324`), como se dice del extremo y el lado |
| N-6 | **RESUELTO** | La región son los dos miembros, localizados por firma, y el núcleo va justo después de `Traverse` (v6 `:464-465`); C09 se adapta (`:785`). Es satisfacible: en la base, `:98` empieza en la firma de `Traverse` y `:170` es su llave de cierre. Quitar los dos miembros por su tramo y normalizar (comentarios fuera y espacio colapsado) deja `:1-97` + `:171-181`, igual que la base sin `:98-170`. El núcleo no necesita ninguna directiva `using` nueva | No |
| m-5 (resto) | **RESUELTO** | D-T4B-04 (8) (v6 `:476-494`) declara las guardas que pasan y las que fallan en el RED, también las de D-T4B-09. La de M-1 pasa en el RED con razón: vigila `PushBackPilotMaterializer.ComposeForWrite` (`PushBackPilotMaterializer.cs:196`), que está en el prohibido y no cambia | No |
| m-7 (resto) | **RESUELTO** | Los tres caminos son los de `T4P:530`, y POST va aparte (v6 `:502`) | Uno menor: N-9 (el «por construcción» de `ReadForPrepare` depende de un mecanismo que el paquete no fija) |
| I-1 (precisión) | **RESUELTO** | «Sin las llaves del método» (v6 `:452`) | No |

## Hallazgos nuevos

| Id | Severidad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| N-7 | MENOR | La corrida tiene la constante de causa `TOP_TRANSACTION_OPEN`, pero el paquete no fija su **identificador**. Un nombre idiomático en C# como `TopTransactionOpen` o `TopTransactionOpenReason` contiene `Transaction` y rompe la guarda de pureza, por la misma regla de subcadena que motivó N-1 | v6 `:296-303`, `:617`; `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs:35` | Fijar el identificador sin la palabra (por ejemplo, `ForeignScopeOpenCause = "TOP_TRANSACTION_OPEN"`), o declarar que la constante se llama en mayúsculas igual que su valor |
| N-8 | MENOR | La regla «se conserva la clasificación y el `Dispose` solo añade su causa» (`:354`, `:360-361`) no se puede aplicar a todas las formas. (i) **NotAppliedConflict no tiene causa** (`DraftApplyOutcome.cs:69-74`, con `cause` nula), y D-T4B-06 acepta expresamente un Conflicto sin causa: no hay dónde «añadir» la del `Dispose`, ni de `m` ni del exterior, y la corrida pura no puede registrar. (ii) «Después de `Post` es un aviso» y a la vez «la clasificación ya decidida no cambia»: si lo decidido era `Applied`, que no lleva avisos (`:87-91`), el aviso obliga a pasar a `AppliedWithWarnings`, que es otro tipo. Las dos dan Clean, pero el texto se contradice | v6 `:354`, `:360-363`; `DraftApplyOutcome.cs:69-74`, `:87-99`; D-T4B-06 | Declarar las dos excepciones: en NotAppliedConflict la excepción del `Dispose` se descarta (limitación declarada, con su prueba); después de `Post`, `Applied` pasa a `AppliedWithWarnings` con el aviso, como única salvedad admitida a «no cambia» |
| N-9 | MENOR | F4-E26 afirma que `ReadForPrepare` usa el mismo núcleo «por construcción» (`:502`), pero D-T4B-03 no dice cómo lee `ReadForPrepare`: su lista de operaciones (`:386-408`) no lo incluye. Por eliminación tendría que ir por `CaptureDrawing`: está fuera de MUTATE, sin transacción poseída, y el censo admite solo dos llamadas al núcleo. Pero ninguna guarda impide que el escritor haga su propio barrido: las cuentas antiduplicación (`RackBlockData.Read(` 1 y demás) solo miran `RackSiblingScan.cs` (`TW/WorkspaceEditIntentGuardTests.cs:77-86`) | v6 `:269`, `:311`, `:386-408`, `:502`; `TW/WorkspaceEditIntentGuardTests.cs:77-86` | Añadir a D-T4B-03 «`ReadForPrepare`: `CaptureDrawing` sin cambios, más el catálogo», y a D-T4B-09 una guarda: el escritor no nombra `RackBlockData.Read`, `BlockTableId` ni `GetBlockReferenceIds`, y solo lee por `RackSiblingScan` |

## Veredicto

**EMITIBLE.** Recuento: **0 bloqueantes, 0 importantes y 3 menores** (N-7, N-8 y N-9).

**Lo resuelto:** todas las filas abiertas de la v5 (N-1 a N-6, los restos de m-5 y m-7, y la precisión de I-1). En particular:
- ningún identificador declarado de la corrida, del puerto ni de la ayuda contiene `Transaction`;
- los dos `finally`, con su precedencia, impiden convertir un CommitThrew o un `ABORT_FAILED` en NotAppliedFailure;
- la regla de `DesiredNames` reproduce la del clásico vista por vista;
- la región de (4b) localizada por firma es satisfacible con el GREEN previsto.

**Lo que queda:** los tres menores son de texto y no cambian el diseño. Conviene aplicarlos en el texto de la emisión, y no necesitan otra revisión.
