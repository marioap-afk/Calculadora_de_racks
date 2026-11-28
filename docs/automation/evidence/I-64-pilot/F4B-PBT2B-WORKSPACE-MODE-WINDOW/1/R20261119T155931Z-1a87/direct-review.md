# Revisión directa de la sesión: PB-T2b, intento 1 (primera delegación)

**Objeto:**
- WorkRunId `R20261119T155931Z-1a87`, sobre la delegación `R20261119T154505Z-69c3`;
- BaseSha = ChainBaseSha `59839c71`, el commit de emisión;
- RED `a3b86134`, reparación del RED `6bc3497f` (RR, F4B-E35) y GREEN `03b16c88`.

Lo medí por objetos git desde el checkout principal, sin tocar el worktree durante la verificación.

| Comprobación | Resultado |
|---|---|
| `git diff --stat 59839c71 a3b86134` | el RED tiene 8 archivos: el archivo del modo (13 líneas), W (6) y las seis clases de prueba (2 331) |
| La costura inerte en W | `ModeForTest` fijo en `Classic`; `ForDraft` crea una ventana CLASSIC sin insertar; y dos costuras, `BomDialog` y `LibraryPathDialog`, sin uso. Son cuatro declaraciones y ninguna línea quitada |
| La reparación del RED `6bc3497f` | es un solo commit, hijo directo del RED, titulado «RED repair» con la tarea y con el trailer `Co-Authored-By: Claude Sonnet 5.5`. Solo cambia `PushBackWorkspaceApplyCloseTests.cs` y `PushBackWorkspaceModeTests.cs`, dos clases que añadió el RED, con 21 líneas añadidas y 0 quitadas: un `Sta(() => { });` y su comentario antes de cada `EditorDiscardPrompt.Substitute`. No cambia ninguna línea esperada, ningún literal ni ningún `Assert` (RR-3). El relevo midió la lista cerrada (56 líneas del paquete) sin cambios y sin indicios |
| `git diff --stat 6bc3497f 03b16c88` | el GREEN solo toca W: 162 líneas añadidas y 25 quitadas, nada de `tests/` |
| D-PBT2-04 y 05 | `mode` es `readonly`. La factoría `ForDraft` comprueba `launch` y llama al constructor privado de WORKSPACE, que: encadena el constructor de siempre, que deja un `LoadNew` en CLASSIC; fija el modo y el puerto; carga una **copia** del estado (`PushBackEditableStateComparer.Copy`); adopta la identidad; descarta lo que dejó el `LoadNew` encadenado (`lastComputation = null`, `SetModel(null, null)`); carga desde el modelo; toma W0 como copia; y aplica el cromo del modo. Las tres cargas públicas rechazan WORKSPACE |
| D-PBT2-06 | `ApplyEditsToEditingState` es el bloque de `RequestDraw` extraído sin cambiar su orden, y CLASSIC le pasa sus dos textos de siempre. En WORKSPACE, `Update_Click` llama a `ApplyToDraft`: aplica la edición, avisa si no hay nada que aplicar, entrega una **copia** por el puerto, toma la nueva referencia de «aplicado» y no cierra. No llama a `SetName` ni a `SyncName` |
| D-PBT2-07 | `PendingWork` da primero el ámbito de módulos, el de siempre, y después «sin aplicar al borrador». Ese segundo ámbito solo vale en WORKSPACE |
| D-PBT2-08 | `RequestDraw` e `InsertBatch_Click` rechazan WORKSPACE con su texto antes de cualquier otro efecto |
| D-PBT2-09 | están los once literales del paquete |
| CI del RED `44572405777` y del RED reparado `44581043462` | tests y ui-tests en failure, build-ui en success y build-plugin omitido. Fallan solo CT-PBT2B-08 en Core y CT-PBT2B-01 a 07 en UI, por aserción. Los registró el relevo |
| CI del GREEN `44586045707` | 4/4: Core 19 094; UI 2 447 (2 426 pasadas y 21 omitidas), como se preveía |
| Relevo del trabajo | válido: 0 participantes, Snapshot K, HEAD igual al remoto y árbol limpio. Registra las comprobaciones de la reparación |

**Valoración:** no encuentro nada bloqueante ni importante.

**Para la revisión independiente:**
- **La instrumentación temporal:** el Worker declara cinco corridas locales sobre el filtro de CT-PBT2B-04 con instrumentación temporal de la clase de prueba, revertida y fuera de todo commit, para diagnosticar el fallo que llevó a la reparación. Hay que juzgarla frente a 16.8 y a RR.
- **El orden de la carga de WORKSPACE:** frente a D-PBT2-05, en especial el `LoadNew` encadenado en CLASSIC antes de fijar el modo.
- **D2:** la copia de entrada y de salida.
- **El momento de las lecturas en CT-PBT2B-01 a 07.**
- **La cuenta de `descartes`** con el aviso sustituido.
