# Revisión directa de la sesión: PB-T2c, intento 1 (primera delegación)

**Objeto:**
- WorkRunId `R20261119T203340Z-05b7`, sobre la delegación `R20261119T203120Z-65ce`;
- BaseSha = ChainBaseSha `05beabfd`, el commit de emisión;
- RED `7aeb6170` y GREEN `b1968341`, sin reparación del RED.

Lo medí por objetos git desde el checkout principal, sin tocar el worktree durante la verificación.

| Comprobación | Resultado |
|---|---|
| `git diff --stat 05beabfd 7aeb6170` | **3 archivos.** W (MU-T2C-01 y MU-T2C-03) y WC (MU-T2C-02) llevan una línea cada mutante. Además, la clase nueva `PushBackWorkspaceCarryoverTests.cs` (549 líneas) |
| **Los tres mutantes** | Son los de la tabla custodiada (W:3713, WC:95 y W:3273), una línea cada uno:<br>- **MU-T2C-01:** `Keep` sin `SameShown`;<br>- **MU-T2C-02:** el aviso del lado B siempre oculto;<br>- **MU-T2C-03:** `ApplyToDraft` entrega `appliedState` en vez de `editable` |
| `git diff --stat 05beabfd b1968341 -- src` | **vacío.** El GREEN deja W y WC exactamente como en la base |
| `git diff --stat 7aeb6170 b1968341` | Solo los tres mutantes revertidos, sin `tests/` |
| **Las corridas aisladas** (`m1` a `m3`) | Cada mutante, solo, hace fallar el CT de su fila:<br>- `m1`: CT-01;<br>- `m2`: CT-02;<br>- `m3`: CT-02 y CT-03, porque MU-T2C-03 afecta a «Aplicar» en las dos.<br>El relevo lo registra, con TRX |
| **Trailer** | `Co-Authored-By: Claude Sonnet 5.5` en los dos commits |
| **CI del RED** (`44618262418`) | Fallan tests y ui-tests, build-ui pasa y build-plugin se omite.<br>- **Core:** solo falla CT-PBT2B-08, la guarda de fuente del modo, que detecta un mutante; el contrato la atribuye a un mutante (INV-PBT2C-04).<br>- **UI:** fallan las tres CT nuevas, más las existentes que el relevo atribuye |
| **CI del GREEN** (`44619492077`) | 4/4: Core 19 094 y UI 2 450 (2 429 pasadas y 21 omitidas), como se preveía |
| **Relevo del trabajo** | Válido: 0 participantes, Snapshot K, HEAD igual al remoto y árbol limpio. Mutantes conformes y sin reparación |

**Valoración:** no encuentro nada bloqueante ni importante.

**Pendiente para la revisión independiente:**
- la atribución completa de los fallos existentes del RED a cada mutante (INV-PBT2C-04);
- las fuentes de CT-PBT2C-01 y 02, leídas tras «Aplicar» sobre `Pasada(Copy(draft.CurrentDraftState), A)` (I-01 de CA-T2c);
- las 49 filas de CT-PBT2C-03 frente a la tabla custodiada.
