# I-64 F4-T4B-MUTATE-POST, intento 1: revisión directa de la sesión

**Objeto:** `3633f08d..4314c180`, con el RED `f4476bc4` (esqueleto inerte de los archivos nuevos, pruebas nuevas y cambiadas) y el GREEN `4314c180` (la implementación). Se revisa contra el contrato `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json` (18 INV, 26 paradas) y contra el paquete v7 (`docs/initiatives/I-64-f4-t4b-gate-contract-package.md`, D-T4B-02 a D-T4B-09).

**Veredicto:** la entrega cumple el contrato. No hay bloqueantes ni importantes. Hay 3 menores y 5 observaciones; dos de ellas son decisiones de implementación del Worker que se comunican al Owner.

## Comprobado por la sesión

- **Alcance (INV-13).**
  - `git diff --name-only BaseSha CurrentSha` lista 21 archivos, todos del AllowedWriteScope (23). Ninguno es del ForbiddenWriteScope.
  - Quedan sin tocar dos archivos permitidos: `WorkspacePilotPrepareGuardTests.cs` y `WorkspaceHostTestSupport.cs`.
  - `git diff RedSha CurrentSha -- tests` sale vacío.
- **El RED (INV-15).**
  - Se leyó el diff de `src/` del RED (8 archivos, 478+/2−). Todos los cuerpos son inertes:
    - `=> default`, `=> null`, `=> false`, `=> new()`;
    - `Go()` y `Stop()` del precheck devuelven los dos `Proceeds = false`;
    - las fábricas de `EditUpdateOutcome` devuelven `default`;
    - `AppliedPostRereadFailed(cause, warnings)` ignora los avisos;
    - `plan.Envelope(key)` devuelve `null`.
  - `RackSiblingScan.cs` no cambia en el RED: el movimiento va en el GREEN.
  - **La CI del RED** es `43667312134`:
    - Core en failure: 132 fallos de 18684, todos dentro del filtro de Workspace (3103 seleccionadas);
    - UI en success: 601/601 en su filtro;
    - build-ui en success y build-plugin omitido.
  - **El criterio mecánico**, medido por la sesión sobre el TRX de esa CI (`_trx_check_t4b.py`):
    - 0 de los 132 fallos llevan el prefijo de tipo cualificado, y la primera línea de su traza está siempre bajo `tests/`;
    - 129 son de clases nuevas o de métodos que el diff del RED toca;
    - los otros 3 (`TheWriteGuardTrips…ExplicitReconciliation`, `TheMarkGuardDetects…` y `EveryOperationOfTheEditPort…EndsTheMarkOfKnownClean`) son pruebas existentes cuyos datos cambia el RED: `PortMutatingOperations`, `ChangesADraft`, `ReconciliationBodies` y una `InlineData(3)` nueva. Son los cambios de guarda declarados en D-T4B-07, así que son pruebas cambiadas.
- **La CI del GREEN** es `43668817337`, con los 4 jobs en success: Core 18684/18684 y UI 2293 superadas de 2310, con las 17 omitidas de la línea base. Los filtros dan 3103/3103 y 601/601. El registro de relevo del trabajo es válido.
- **El movimiento de `Traverse` (INV-17, D-T4B-04).**
  - Se leyó el diff completo de `RackSiblingScan.cs`. `Traverse` es exactamente el envoltorio de los cinco pasos.
  - El núcleo `internal static TraverseCore` va justo después y lleva la firma declarada.
  - **La sesión comprobó, de forma independiente, los literales que embeben las guardas** (`_check_traverse_literals.py`):
    - `TraverseBaseline.CoreLiteral` es igual a `c3ae1c97:RackSiblingScan.cs:103-110 + :115-161 + :166-169`, con la única sustitución;
    - `RestLiteral` es igual al archivo de `c3ae1c97` sin `:98-170`.
    - La comparación colapsa el espacio en blanco y **no** quita comentarios, así que es más estricta que la normalización de la guarda.
    - Las líneas `:111-114` y `:162-165` son las del envoltorio, como dice el paquete.
  - Las guardas (1), (2), (3), (4) y (4b) llevan autopruebas negativas: `directOnly`, `IsAnonymous`, `envelope != null &&`, `forceValidity`, `ForWrite`, el `Commit`, el `return`, el `LockDocument`, un `try`, una llamada de más o de menos y un miembro entre los dos.
- **La corrida (D-T4B-02)**, en `PushBackPilotUpdateRun.cs`:
  - el precheck pasa `(null, null)` y solo para con NotApplicable, PendingText y NothingToApply;
  - el origen se elige en puro, por el texto crudo y ordinal. `BuildStaleBase` no depende de la clave del origen (`PushBackPilotIntentReader.Analyze`), así que elegir con la clave vacía es correcto;
  - los supervivientes y las capas se comprueban antes de importar y otra vez en MUTATE;
  - la relectura vuelve a elegir el origen y lo compara con `plan.OriginKey`, compara la StaleBase y después el destino, y solo entonces viene `WriteUnit`;
  - `_writeStarted` se marca antes de cada `WriteUnit`;
  - `Seal` se distingue con su propia marca (`sealing`);
  - los dos `Dispose` van en `finally` propios, el de `m` dentro del de `scope`;
  - la precedencia y las dos salvedades siguen la tabla del paquete;
  - POST:
    - la relectura usa la clave `plan.OriginKey`;
    - los nombres se comparan con `SanitizeBlockName` y `OrdinalIgnoreCase`, y solo para las claves con nombre deseado;
    - `ComparePost` es ordinal y no normaliza nada;
    - `ForeignScopeOpenAtPost` da AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`) con los avisos.
- **La regla de los nombres, contra el clásico.** Se comparó con `RackPushBackCommands.cs:188-189` y `:282-342`:
  - planta → `LinkedPlanta(base)`;
  - lateral → `LinkedLateral(base, Variant.Index)`, que es el `postIndex` del clásico;
  - corte → `LinkedPushBackCut(base, end, side, IsComposite)`. El mapeo A/B y el de los extremos es equivalente, porque `RackPushBackSide` solo tiene A y B y `LinkedPushBackCut` trata como A todo lo que no es B.
- **El escritor (D-T4B-03)**, en `RackPilotUpdateWriter.cs`:
  - tres clases, y el puerto no tiene campos;
  - los objetos de AutoCAD solo viven en los dos ámbitos;
  - la comprobación del documento es por `UnmanagedObject.ToInt64()`;
  - `ReadForPrepare` es `CaptureDrawing` más el catálogo;
  - `PrepareRedraw` solo está en `PrepareUnits`, con el mismo mapeo de extremo y lado que el clásico;
  - `BeginMutation` comprueba `TopTransaction` y después hace un único `StartTransaction`;
  - `Reread` es `TraverseCore(transaction, database, ObjectId.Null, null)`, y `EnsureOwned` compara la transacción por `UnmanagedObject`;
  - `WriteUnit` comprueba que el texto es el `PayloadJson` preparado para esa clave;
  - `Seal` = `Commit()`, con la inyección antes y después;
  - `Post` sigue el orden: TopTransaction, `PurgeAfterCommit` una vez con la unión de las `staleDefinitions`, `SyncName` solo para los nombres deseados, la lectura de nombres con `StartOpenCloseTransaction` y `ForRead`, `ApplyRegen(document, true)` y por último `CaptureDrawing`;
  - no hay `LockDocument`, `Erase(` ni `RackUnitsGuard`;
  - `PurgeAfterCommit` es `LateralHeaderDrawer.PurgeUnreferenced`: categoría b, según el mapa 5 de la revalidación.
- **La inyección Debug:**
  - dos métodos `[Conditional("DEBUG")]`, y solo la lectura de la variable va en `#if DEBUG`;
  - el `#else return null;` es la forma declarada en el paquete, «frente a la regla `#if DEBUG` sin `#else`», que solo rige `SiblingRedrawDebugFaultInjection.cs`;
  - una guarda fija el patrón.
- **`CompleteApply` por rama (D-T4B-06):**
  - `StageApplied` copia y evalúa el comparador antes de liberar la ranura, y solo si el borrador está registrado;
  - `ClassifyContained` y `Recover` dan, por rama: Unknown(CommitThrew) con causa; Unknown(PostReread) con los avisos; o Error con `ErrorReason` para NotApplied*;
  - el bloque del error diferido no cambia, y lo fija su guarda;
  - `AppliedPostRereadFailed` conserva los avisos (F4-E16).
- **`TryUpdatePilot` (D-T4B-07):**
  - va en este orden: la puerta propia, el borrador (`NoDraftReason`), el precheck, `TryBeginApply`, la corrida y `CompleteApply` en `finally` una vez;
  - Failed solo antes de la ranura;
  - el fin de la marca de limpio va solo en los resultados que cambian el borrador;
  - `Differences` solo en un Conflict;
  - `UpdateGate` sigue el orden documento → modal → `Authorize`, con la forma exacta, sin reposo ni lector, y el identificador del documento es el que validó la puerta.
- **La contención de las pruebas (INV-18):**
  - las pruebas nuevas no escriben archivos, no tocan el entorno y no abren ventanas, dispatchers ni modales;
  - `Dispatcher` y `new RackEmbedDocument()` solo aparecen como texto inyectado de autopruebas negativas;
  - la entrega declara que en local solo corrió los dos filtros de UI, nunca la suite completa.

## Hallazgos

| Id | Gravedad | Contenido | Evidencia | Disposición propuesta |
|---|---|---|---|---|
| DR-m1 | MENOR | Dos constantes pierden el espacio entre `=` y el literal. El valor no cambia; es cosmético | `PushBackPilotUpdatePlan.cs:301` (ya en el RED) y `WorkspaceHostCore.cs:49` | A ideas futuras, o a la siguiente unidad que toque esos archivos |
| DR-m2 | MENOR | El host gana tres textos nuevos: `UpdateNotRunReason` y los dos motivos de `UpdateGate`. D-T4B-07 «Textos» pide que las salidas tempranas usen las constantes que ya existen. La entrega declara los dos de la puerta como decisión conservadora (no tocar la tabla de 30 literales), pero no `UpdateNotRunReason`, que es la causa de respaldo del paso 6 | `WorkspaceHostCore.cs:48`, `:915`, `:921` | Ratificar como contenida: las cuentas de 30 literales no cambian y su guarda pasa |
| DR-m3 | MENOR | `EditUpdateOutcome.Closed` guarda las listas por referencia, aunque la prueba se llama «…AndCopiesOfTheLists» y no comprueba ninguna copia. Los llamadores actuales pasan arrays nuevos o los arrays de solo lectura del borrador | `WorkspaceEditIntent.cs:477-483`; `WorkspacePilotUpdateHostTests.cs:436-457` | A ideas futuras: copiar al recibir, o renombrar la prueba |
| DR-O1 | OBSERVACIÓN | **Decisión del Worker:** si el cálculo de los nombres deseados lanza, POST sigue, se da un aviso y se puede terminar en AppliedWithWarnings (Clean). La tabla de frontera no tiene esa fila; la más cercana es «el comparador o el constructor de un resultado lanzan» → AppliedPostRereadFailed. Los nombres no afectan al contenido authored, que sí verifica la relectura | `PushBackPilotUpdateRun.cs:478-489`; WorkCompleted | Comunicar al Owner con el cierre; la revisión independiente la pondera |
| DR-O2 | OBSERVACIÓN | **Decisión del Worker:** una importación que devuelve `Ok = false` (no una excepción) es NotAppliedPreflight con su causa. La tabla solo fija la excepción en `PrepareUnits` (NotAppliedFailure). El residuo OM-5 está declarado | `PushBackPilotUpdateRun.cs:322` | Comunicar al Owner con el cierre |
| DR-O3 | OBSERVACIÓN | **Proceso.** Al terminar, el Worker hizo `git restore --source=HEAD -- src` y dejó cinco archivos con CRLF en disco y LF en el índice. Git los marcaba como modificados por el tamaño, aunque el contenido filtrado era idéntico. La sesión lo comprobó por `hash-object` y volvió a añadir los mismos blobs: el árbol del índice sigue siendo el de HEAD (`0cb46ccc`) | relevo de entrada | Ninguna acción |
| DR-O4 | OBSERVACIÓN | **Proceso.** Las corridas locales del RED se repitieron después del commit GREEN, restaurando el `src/` del RED en el árbol de trabajo. La entrega registra el TreeSha del RED. La autoridad es la CI del SHA exacto del RED | transcripción del Worker, 02:50Z | Ninguna acción |
| DR-O5 | OBSERVACIÓN | H-1 (la suite de UI existente escribe en el perfil real) sigue abierta: es una deuda aparte, como dispuso el Owner (F4-E59) | — | Sin cambio |
