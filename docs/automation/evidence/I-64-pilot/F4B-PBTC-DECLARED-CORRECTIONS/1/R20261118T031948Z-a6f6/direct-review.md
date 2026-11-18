# I-64 F4B PB-TC — Revisión directa de la sesión (intento 1)

**Objeto:**
- Worker `R20261118T021306Z-069b`, sobre la delegación `R20261118T020622Z-7171`, que es la replanificación única de PBTC-C08 tras `8db0`.
- BaseSha = ChainBaseSha `48fe199e`; RED `f665edac`; GREEN `4440f67d`.

**Método:**
- Git en solo lectura sobre la rama.
- La entrega `worker-handoff.json`.
- La CI exacta: RED `44223960356`, GREEN `44225456328`.
- Los TRX descargados y el relevo del trabajo (`relay-record.json`, válido).

## Resultado

**Conforme.** Hay un menor de forma en la entrega (D-1) y ninguna decisión para el Owner.

| Punto | Comprobado | Resultado |
|---|---|---|
| Alcance | `git diff --name-only 48fe199e 4440f67d`: 13 rutas, todas del `AllowedWriteScope`. El RED (`48fe199e..f665edac`) solo toca `tests/`: los 6 archivos nuevos y los 4 de SUS. El GREEN (`f665edac..4440f67d`) solo toca W, WC y WX | Conforme |
| SUS-01 a SUS-04 | En cada uno de los cuatro archivos el diff es exactamente la línea del atributo (`[Fact]` o `[Theory]` → `…(Skip = "<literal>")`), con el literal del §5.2 del paquete y la decisión que corresponde: D-PB-06 en SUS-01 a SUS-03 y D-PB-11 en SUS-04 | Conforme |
| D-PBTC-01 (a) | Las llamadas `SetNumber` no cambian. Los cuatro portadores (`rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale`) se asignan en una línea propia con la misma expresión que su `SetNumber`, y el fondo usa `generalPallet.Depth`. En `ReadInputs`, `generalPallet` se sustituye por un objeto nuevo y no se muta. Diez `Val(` pasan a `Keep(`. `OptionalVal` conserva el nulo y conserva el exacto si los dos se ven igual. `ShownFormat`, `SameShown` y `Keep` van justo después de `OptionalVal`. `Val` se conserva para W:1865 | Conforme |
| D-PBTC-02 (b) | `loadedDimensionStyle` se toma en `LoadFromModel` desde `options.DimensionStyle` y viaja en `ReadInputs`. Sin control nuevo | Conforme |
| D-PBTC-03 (D-PB-08) | `TextBlock SideBEmptyNoticeText`, último hijo de `StatusContent`, detrás de `StatusText`, con los atributos de `StatusText` y colapsado. `UpdateSideBEmptyNotice()` se evalúa en cada pasada efectiva, después de `RecomputePassesForTest++`. La regla es `SideBPresent` y ninguna ranura de B presente. El literal de la constante es **idéntico byte a byte** al de `owner-pbtc-decisions.md` (pregunta 3; comprobado con Python: 141 caracteres) y está en CT-PBTC-07 | Conforme |
| D-PBTC-11 (D-PB-11) | Una línea antes de `ClearModuleCommit()` lee `StandardRestoreRequested`. Después, un `if` cuyo cuerpo son exactamente `state.RestoreAdvancedRackParameters(advanced);` y `LoadAdvancedRackParameters();`. Sin `RequestRecompute(` y sin `forceRebuild:` ni `forceRebuild)` | Conforme |
| H-31 fuera | El diff no toca `Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, el manejador del panel avanzado ni `SetOptional` | Conforme |
| PBTC-C14 (D-PBTC-10) | Corrida temporal `m10` sobre la base. F1 y M-02 dan solo `PalletDepth: 48 -> 48.00044`. K1, K2, K3 y K4 están vacíos o en falso en los ocho planos (maxDelta 0). **No se activa.** La reconstrucción de módulos que hoy hace RACKEDITAR con un fondo con decimales no cambia nada más en esos dos casos | Conforme; dato para el Owner en el cierre |
| RED en la CI | `44223960356`: ui-tests en failure; tests, build-ui y build-plugin en success. UI: 2 418 resultados, 10 Failed (todos `PushBackDeclaredCorrection`), 21 Skipped y 2 387 Passed. Core: 19 070/19 070. Los 10 fallan por aserción; ninguna prueba existente falla | Conforme; las cifras son exactamente las del §6.9 |
| GREEN en la CI | `44225456328`: 4/4. UI: 2 418, con 2 397 Passed y 21 Skipped. Core: 19 070 | Conforme; las cifras son exactamente las del §6.9 |
| Comprobación temporal de SUS | SUS-01: fallan pb-06, pb-07 y pb-08, y pasan 11. SUS-02: falla el estilo (`ESTILO_PB`). SUS-03: difieren 10 líneas y F4 coincide. SUS-04: difieren 4 y coinciden 6. Artefactos en `tests/s1` a `tests/s4` | Conforme con el §5.2 |
| Contención | Prueba de limpieza antes y después idéntica. Builds Release iguales a la línea base. 3 corridas seguidas del filtro con `ExpectRed` en verde. Suite de Core en local en verde. Nunca la suite completa de UI en local. `build-server shutdown` | Conforme |
| Relevo | Sonnet 5.5, effort high (220 turnos). 0 denegaciones, 1 error de herramienta resuelto. 0 participantes. Snapshot I a la entrada y a la salida | Conforme |

## Hallazgos

| Id | Gravedad | Contenido | Disposición |
|---|---|---|---|
| D-1 | MENOR (forma de la entrega) | La entrega cita los artefactos con el prefijo `System.Xml.XmlElement/tests/…`. Parece una variable mal interpolada. Los archivos existen en `artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T021306Z-069b/tests/…`: lo medido por la sesión es `m10`, `s1` a `s4`, `e1`, `p1` a `p11`, `red1` a `red12`, `g1` a `g3`, `gu1` a `gu7`, `gcore`, `redci`, `greenci`, `b0`, `r1`, `rg1`, `cleanup-*.txt`, `inv-checks.txt` y `diff-src-u0.patch`. El contenido de la evidencia es correcto; solo el prefijo de la ruta está mal | Se informa al Controller como hecho medido por la sesión. Lección para la plantilla del prompt del Worker |
| D-2 | INFORMATIVO | F2 (el peralte del poste) trae en la base, además de su campo, las cuatro líneas `Modules[n].Header.PostPeralte`. Es la copia del valor del propio campo en las cabeceras, coherente con T-3 R2. PBTC-C14 no gobierna esa fila | Sin acción |
| D-3 | INFORMATIVO | Desviaciones declaradas: el shell de herramientas pasó por su cuenta dos esperas largas a segundo plano; la política de scripts de PowerShell obligó a ejecutar la prueba de limpieza como bloque en memoria; las corridas sin commit tienen `TreeSha` null | Aceptables; no afectan al contrato |

**Al Owner no va nada.** Hay un dato para el cierre: con un fondo con decimales, la reconstrucción de módulos que hoy hace RACKEDITAR no cambia piezas, niveles ni BOM en los casos medidos.
