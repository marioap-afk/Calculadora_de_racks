# I-64 — Comprobación dirigida de la v5 del paquete de F4-T4B

**Objeto:** la v5 de `I-64-f4-t4b-gate-contract-package.md`, contra la tabla de `independent-check-v4.md` (B-1, I-1 a I-4, m-1 a m-13). La v4 se compara desde `_package-v4.md`, y la disposición de la v5 está en su §10.4.

**Base:** `7c29a5e5`. Solo lectura de fuentes con `git show` y `git grep`. No se compiló ni se ejecutó nada. «v5 `:N`» es la línea N del paquete revisado, y las rutas son relativas al repositorio.

**Método:**
- `diff` de la v4 frente a la v5.
- Lectura de cada pasaje corregido.
- Contraste con el código en los cuatro puntos pedidos:
  - `DraftApplyOutcome.cs`, `BlockNaming.cs`, `RackViewBaseName.cs` y `RackPushBackCommands.cs` (I-3 e I-4);
  - `RackSiblingScan.cs` (I-2);
  - las guardas de pureza de `Systems/PushBack/Pilot/` (I-3).
- `validation-v5.json` (94/94 PASS) se tomó como insumo; no sustituye a la comprobación.

## Estado por hallazgo

| Id | Estado | Comprobación contra la v5 y el código | ¿La corrección introduce un defecto? |
|---|---|---|---|
| B-1 | **RESUELTO** | `internal static` (v5 `:414-415`), fijado por la guarda (2) (`:440`). `RackSiblingScan` es `internal static class` (`RackSiblingScan.cs:77`), y el escritor está en el mismo ensamblado | No |
| I-1 | **RESUELTO** | El literal es `:103-110` + `:115-161` + `:166-169`, con una única sustitución en `:115` (v5 `:432`); `:111-114` y `:162-165` pasan al envoltorio (`:418`); la normalización está fijada (`:433-437`). Sin la llave de `:114`, `:115-161` queda equilibrado (el `foreach` abre en `:117` y cierra en `:161`), y con `:166-169` forma un cuerpo que compila | No. **Precisión opcional:** decir que «el cuerpo del núcleo» se toma sin las llaves del método, porque el literal no las lleva |
| I-2 | **RESUELTO** | (4b) (v5 `:449`) y C09 ampliada (`:751`). **Es satisfacible:** el GREEN previsto no necesita ninguna directiva `using` nueva (`Transaction`, `Database`, `ObjectId` y `Func` ya están importados en `:1-10`), y fuera de `:98-170` quedan intactos `:1-97` y `:171-181`, con `AddLayer` en `:172-179` | Uno menor: N-6 (la región depende de dónde se coloque el núcleo) |
| I-3 | **RESUELTO en el fondo** | `Post(PushBackPilotPostRequest)` recibe `DesiredNames` y devuelve hechos (v5 `:285-286`, `:386-392`). La corrida decide el nombre esperado, los avisos y el caso `TopTransactionAtPost` (`:334-338`). El escritor no puede nombrar `BlockNaming`, `RackViewBaseName` ni textos de aviso (`:592`). La lectura de nombres va con `StartOpenCloseTransaction`, solo en `Post` y sin `ForWrite` ni `UpgradeOpen` (`:390`, `:587`). Es coherente con las demás guardas del escritor: `StartOpenCloseTransaction` no contiene la subcadena `StartTransaction`, así que «un `StartTransaction` … y ninguno más» se cumple; la transacción de lectura no confirma, así que «un `Commit()` … y ninguno más» también; y la guarda de orden incluye la lectura (`:598`). `BlockNaming.SanitizeBlockName` y `RackViewBaseName` son de Application, y la corrida puede usarlos | **Sí:** N-1 (importante: `TopTransactionAtPost` choca con la guarda de pureza) y N-5 (menor: la regla de los nombres deseados está incompleta y se pierde «sin nombre, sin aviso») |
| I-4 | **PARCIAL** | La fila `ABORT_FAILED` (v5 `:346`) es válida frente a `DraftApplyOutcome`: `CommitThrew(string cause)` existe (`DraftApplyOutcome.cs:82`), y su clase es Unknown(CommitThrew) (`:26-27`, H-01). `Reason` = `LastApplyCause` llega a `EditUpdateStatus` (fila 8-15). Las tres filas del aborto (`:345-347`) y el `finally` propio (`:356`) cierran el caso pedido | **Sí:** N-2 (importante: el ámbito exterior sigue en un `using` sin clasificar fuera de POST), N-3 (menor: (d) sigue diciendo `using (var m …)`) y N-4 (menor: la precedencia entre filas) |
| m-1 | RESUELTO | D-T4B-03 (`:393`) y §9 I-15 (`:808`) | No |
| m-2 | RESUELTO | La guarda de orden del cuerpo de `Post`, con cuentas de una aparición (`:598`) | No |
| m-3 | RESUELTO | AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`), declarado imposible por construcción (`:338`, `:717`) | Sí, N-1 (el nombre del campo) |
| m-4 | RESUELTO | El residuo, declarado y no observado (`:768`) | No |
| m-5 | **PARCIAL** | D-T4B-04 (8) (`:457-460`) pone el movimiento en el GREEN y declara las guardas de D-T4B-04 que pasan en el RED. **No declara** las de D-T4B-09 que también pasan en el RED (ver «Qué pasa en el RED») | No |
| m-6 | RESUELTO | H-23 y H-25 precisados (`:214-215`, `:232-234`) | No |
| m-7 | **PARCIAL** | Ya no dice «los cuatro caminos», pero la nueva lista, «Begin/“Comprobar ahora”, `Reread` y POST» (`:468`), no es la de F4-E26. `T4P:530` define los tres caminos como «Comprobar ahora», **la vía de O-04 en contexto de comando** (en T4B, `ReadForPrepare` bajo Applying) y la lectura caller-owned. La v5 cambia O-04 por POST | No; queda un resto menor |
| m-8 | RESUELTO | El prefijo de clase y el suelo de 2923 (`:673`) | No |
| m-9 | RESUELTO | `:24` | No |
| m-10 | RESUELTO | I-5 solo en la purga (`:785`, `:790`); las filas I-5 e I-6 (`:798-799`); el §10.3 (`:862`); `:166-169` (`:464`) | No |
| m-11 | RESUELTO | D-T4B-09 (`:599-602`) e INV-15 (`:714`) | No |
| m-12 | RESUELTO | La transacción es un campo del ámbito y la libera su `Dispose` (`:382`); INV-14 está redactado así (`:590`) | La contradicción con (d) va en N-3 |
| m-13 | RESUELTO | Fila nueva en el §10.3 (`:865`) | No |

## Qué pasa en el RED (m-5)

**El supuesto.** El RED tiene:
- las pruebas nuevas;
- las pruebas existentes cambiadas;
- un esqueleto inerte de los archivos nuevos (INV-15);
- `RackSiblingScan.cs` sin tocar, porque el movimiento va en el GREEN.

**Fallan en el RED:**
- de D-T4B-04, (1), (2), (4) y la parte de (4b) que exige el núcleo. Lo declara la v5;
- de D-T4B-09, las positivas:
  - un `StartTransaction` en `BeginMutation`;
  - un `StartOpenCloseTransaction` en `Post`;
  - un `Commit()` en `Seal`;
  - `PurgeAfterCommit` una vez en `Post`;
  - la comprobación de `TopTransaction` antes de purgar;
  - la guarda de orden de `Post`;
  - la liberación del ámbito en el `finally` de la corrida;
  - las de la inyección Debug.

**Pasan ya en el RED.** Lo admite la regla vigente («fallan solo pruebas nuevas o la cambiada», `A2P` INV-14).
- **Las declara la v5:**
  - (3), porque `Capture` y `CaptureDrawing` no cambian;
  - la igualdad de (4b) fuera de la región;
  - las negativas de INV-39;
  - la extensión de `CallerOwnedFacadeGuardTests`.
- **No las declara**, aunque pasan igual sobre un esqueleto inerte:
  - la pureza de la corrida, del puerto y de la ayuda;
  - los «sin …» del escritor: `LockDocument`, `Erase(`, `EraseViewBlocks`, `RackUnitsGuard`, `async`, `await`, `ExecuteInCommandContextAsync` y `NavigationTargets`;
  - la parte de campos de INV-14;
  - «sin `BlockNaming`, `RackViewBaseName` ni textos de aviso»;
  - el ámbito de mutación sin purga;
  - las trampas de censo de m-11;
  - previsiblemente, la guarda de M-1 sobre `ComposeForWrite`, que ya tiene la forma pedida;
  - y «`PrepareRedraw(` solo en `PrepareUnits`», si se escribe como «solo en» y no como «exactamente uno en».

## Hallazgos nuevos

| Id | Severidad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| N-1 | IMPORTANTE | El campo `TopTransactionAtPost` de `PushBackPilotPostValues`, que la corrida lee, contiene la subcadena `Transaction`. La guarda de pureza de la corrida, del puerto y de la ayuda prohíbe `Transaction`, y la lista de campos se declara exacta. Los precedentes de la misma carpeta usan la subcadena sin `\b` (`new Regex(@"Transaction")`), con texto y comentarios. La v5 incluso explica que `TOP_TRANSACTION_OPEN` solo se libra por las mayúsculas. Con esa lectura, la guarda falla en el RED y en el GREEN, o el Worker tiene que cambiar un campo declarado | v5 `:286`, `:293`, `:338`, `:583`; `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs:35`; `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:36` | Renombrar el campo sin la palabra (por ejemplo, `ForeignScopeOpenAtPost` o `HostBusyAtPost`), o fijar en la emisión que la pureza usa `\bTransaction\b`. Lo primero es lo coherente con los precedentes |
| N-2 | IMPORTANTE | El ámbito exterior sigue en un `using` (v5 `:308`), y la tabla solo clasifica su `Dispose` «después de `Post`» (`:352`). En CommitThrew, en `ABORT_FAILED` o en NotApplied, una excepción del `Dispose` exterior sale del `using` y tapa el resultado ya decidido. Si la corrida, que es total, lo clasifica por `sealedReturned` falso, un CommitThrew o un `ABORT_FAILED` acaba en NotAppliedFailure: la B del Owner convertida en A, justo lo que I-4 quería cerrar | v5 `:308`, `:345-352`, `:356`; disposición `:203-211` | Liberar también el ámbito exterior en un `finally` propio, con la regla «una clasificación ya decidida no cambia por el `Dispose` exterior; fuera de POST se anota en la causa o se registra», más su fila y su prueba con el doble |
| N-3 | MENOR | (d) sigue diciendo «Si no, `using (var m = start.Scope)`» (v5 `:325`), mientras que (6) dice «La corrida no libera `m` con un `using`» (`:356`) e INV-14 exige el `finally` (`:590`) | v5 `:325`, `:356`, `:590` | Reescribir (d): «Si no, `m = start.Scope`, liberado en el `finally` de (6)» |
| N-4 | MENOR | Las filas de la tabla no fijan la precedencia: (i) si `Seal` lanza y después el aborto también, salen a la vez la fila de `Seal` y la de `ABORT_FAILED`, las dos CommitThrew pero con causas distintas; (ii) si MUTATE decide NotAppliedConflict o Preflight sin ningún `WriteUnit` y después el `Dispose` de `m` lanza, la fila `:347` lo convierte en NotAppliedFailure y se pierden las diferencias del conflicto | v5 `:326-329`, `:345-348` | Una frase de precedencia: se conserva la clasificación ya decidida (Conflict o Preflight con sus diferencias, o CommitThrew por `Seal`), y el `Dispose` solo añade la causa |
| N-5 | MENOR | `DesiredNames` se describe solo como «con `RackViewBaseName` a partir del `Name` del sobre del plan». No se declara la regla por vista del clásico: planta `LinkedPlanta(base)`; lateral `LinkedLateral(base, postIndex)`; corte `LinkedPushBackCut(base, end, side, system.IsComposite)`, con `includeSide` según el sistema, que solo da la ayuda pura; y `base` = `Name.Trim()` o nulo. Además se perdió «sin nombre, sin `SyncName` y sin aviso» (RR R-M-10, que la v4 tenía): `SanitizeBlockName(null)` devuelve `"Cabecera"`, así que una comparación ingenua daría un aviso falso | v5 `:285`, `:336`, `:389`; `src/RackCad.Plugin/RackPushBackCommands.cs:189`, `:282`, `:300-309`, `:339-342`; `src/RackCad.Application/Systems/Shared/RackViewBaseName.cs:164-191`; `src/RackCad.Application/BlockNaming.cs:12-17` | Declarar la regla por vista, con `IsComposite` sacado de `PushBackPilotSystemRebuild` y el corte de `RackViewCodec`. Restaurar: «una clave sin nombre deseado no se renombra y no da aviso». Añadir una prueba de cada vista y una de nombre en blanco |
| N-6 | MENOR | (4b) habla de «la región `Traverse` + núcleo» sin decir dónde va el núcleo. Si una guarda toma la región como un tramo contiguo y el núcleo se coloca después de `AddLayer`, la región engloba `AddLayer` y lo deja sin congelar | v5 `:417-418`, `:449` | Fijar el núcleo inmediatamente después de `Traverse`, o definir la región como los dos miembros localizados por firma, sin contar lo que haya entre ellos |

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Recuento de lo abierto: **0 bloqueantes, 2 importantes y 6 menores.**
- Los nuevos: N-1 y N-2 (importantes) y N-3 a N-6 (menores).
- Los restos de m-5 y m-7 (menores).

**Lo resuelto:**
- B-1, I-1 e I-2 están resueltos, y (4b) es satisfacible con el GREEN previsto;
- I-3 está resuelto en el fondo: el escritor solo hace operaciones primitivas y devuelve hechos, y la corrida interpreta POST;
- de los trece menores, once están resueltos.

**Lo que queda:**
- N-1 sale de la propia corrección de I-3 y m-3;
- N-2 deja abierta, por el ámbito exterior, la misma conversión de B en A que I-4 cerró para el ámbito de mutación.

Las dos son correcciones de texto: renombrar un campo, y aplicar al ámbito exterior el mismo `finally` con la regla de precedencia. Basta una comprobación de esas filas, sin otra revisión completa.
