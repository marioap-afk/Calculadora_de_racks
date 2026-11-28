# Revisión directa de la sesión: PB-T2a, intento 2 (corrección)

**Objeto:**
- WorkRunId `R20261119T065932Z-3b39`, con la delegación `R20261119T065628Z-7de7`;
- BaseSha `17872be8`, que ya trae el RED `c33ac6ac` del intento 1;
- RED `fb90c823` y GREEN `5ddeb3b4`.

Lo medí por objetos git, desde el checkout principal y sin tocar el worktree.

| Comprobación | Resultado |
|---|---|
| `git diff --name-only 17872be8 fb90c823` | solo `tests/RackCad.Tests/PushBackDraftStatePortTests.cs`, con 5 líneas añadidas y 2 quitadas |
| C-1 del análisis | exacta. En la lectura de `LaunchFor(draft, p)`, `estado-es-el-actual` e `id` se toman antes de `ApplyToDraft(edited)`, y `puerto-es-del-borrador` después. La línea esperada no cambia |
| C-2 | se cumple: las demás pruebas y el esqueleto de `src/` siguen como en `c33ac6ac` |
| `git diff --name-only fb90c823 5ddeb3b4` | solo los dos archivos de Application, `PushBackDraftEditorLaunch.cs` y `PushBackDraftStatePort.cs`, con 27 líneas añadidas y 4 quitadas; nada de `tests/` |
| D-PBT2-03, el lanzamiento | el constructor comprueba `state`, `rackId` (con `IsNullOrWhiteSpace`) y `port`, en ese orden y con las sentencias de excepción del paquete, y guarda las referencias sin copiar. `SourceProject` puede ser nulo |
| D-PBT2-02, el puerto | el adaptador guarda el borrador y solo llama a `SetCurrentDraftState`. `Updated` da `Applied`, y cualquier otro estado, `NotEditable`, con el `IsDirty` del núcleo. `LaunchFor` comprueba el borrador y entrega su estado actual, su `RackId` y un puerto nuevo sobre él. El adaptador no copia ni valida nada más: la copia la hace el núcleo |
| INV-PBT2A-01 | ningún archivo existente de `src/` cambia en la cadena (`-- src`, desde ChainBaseSha `0dc623ca`): solo los dos archivos nuevos |
| Trailer | `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` en los dos commits |
| CI del RED `44478438623` | tests en failure (CT-PBT2A-01 a 03, por aserción); ui-tests y build-ui en success; build-plugin omitido. Es lo que pide `ExpectedEvidence[1]` |
| CI del GREEN `44480898605` | 4/4: Core 19 092; UI 2 439 (2 418 pasadas y 21 omitidas). Es el cierre de PB-T1c más 4 en Core |
| Relevo del trabajo | válido, tras corregir el alcance de la cadena a `-- src tests`: 0 participantes, Snapshot J, HEAD igual al remoto y árbol limpio |

**Valoración:** no encuentro nada bloqueante ni importante. Para la revisión independiente quedan:
- la cobertura de CT-02 y CT-03, que no ejercen el orden de las comprobaciones del constructor;
- los comentarios XML heredados del esqueleto.
