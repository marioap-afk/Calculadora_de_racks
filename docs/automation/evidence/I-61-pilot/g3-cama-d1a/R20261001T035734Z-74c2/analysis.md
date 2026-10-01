# Análisis — STOP S-04 de la verificación (g3-cama-d1a, R20261001T035734Z-74c2)

Redacta: Coordinator de I-61 (SAME-SESSION ROLE). AUTOMATION_PLAN 16.11: un STOP exige causa raíz en `analysis.md` y decisión del Coordinator, o del Owner si la materia es
OWNER-RESERVED. La interpretación de un artefacto de CI no lo es.

## Hecho

La verificación del Controller Codex (salida válida y coherente según el README §8) devolvió `EXECUTION_BLOCKED / STOP` con `FailureClass = StopCondition` y
`TriggeredStopConditions = [S-04]`.

- 13 de las 14 comprobaciones están en `pass`, incluidas `Ci` y `Tests.RedPart` (RED acreditado, `ChainRedFiles` = las dos pruebas nuevas de Core).
- `Tests` está en `fail` por un único motivo: el TRX de UI de la corrida 36812102463 tiene 17 `UnitTestResult` con `outcome="NotExecuted"`, mientras su `ResultSummary/Counters`
  dice `notExecuted="0"`. El Controller lo trató como evidencia contradictoria.

## Causa raíz (medida)

- `tests/RackCad.UI.Tests` contiene exactamente **17** atributos `Skip = …` (`git grep -c`: 8 + 3 + 4 + 2 en cuatro archivos de caracterización). Son los mismos en
  `7b8662c5` (cierre de G2) y en `b8dfc900` (`CurrentSha`). La entrega no añade ni quita ninguno: solo toca `FlowBedEditorWindowTests.cs` en UI.
- Los 17 `NotExecuted` del TRX son esas 17 pruebas omitidas, con los nombres de las clases que llevan `Skip`.
- El logger TRX de VSTest registra una prueba omitida como `UnitTestResult` con `outcome="NotExecuted"`, la cuenta en `total` y no en `executed`, y no incrementa `notExecuted`. En la
  corrida actual: `total` 1654 − `executed` 1637 = 17.
- La misma forma aparece en la corrida del `RedSha` (36811668907: 1653/1636, 17 `NotExecuted`, `notExecuted=0`) y en la del cierre de G2, anterior al piloto (36809531027: 1653/1636,
  17 `NotExecuted`, `notExecuted=0`, `ui.trx` SHA-256 `810DC4A5…6CEE06`).
- Conclusión: no hay contradicción. Los contadores siguen la convención del logger para pruebas omitidas, que es la base del repositorio y no la introduce la entrega. Los filtros del
  contrato concuerdan (el propio Controller lo anotó).

## Decisión del Coordinator

1. S-04 queda resuelto **sin cambio del trabajo**: no hay corrección, no se incrementa `attempts` (16.8) y no hay Worker nuevo.
2. Se completa el registro del trabajo con el hecho explicativo anterior, como señal de la sesión en `Notes`, con las órdenes reproducibles.
3. Se reejecuta la fase VERIFICATION con `RunId` nuevo, el **mismo prompt** (misma plantilla; solo cambian la ruta de entrada y el `RunId`) y copias nuevas de las entradas.
4. **Interpretación registrada:** el Freeze no enumera qué sigue a un STOP resuelto sin cambio del trabajo cuando la clasificación fue `EXECUTION_BLOCKED`. Se trata como la recuperación
   de BLOCKED de 16.11: el Coordinator resuelve la precondición, aquí el hecho ausente del registro, y se reejecuta dentro del tope de 16.8. Es la reejecución **1 de 2** de
   (g3-cama-d1a, VERIFICATION) y cuenta en el tope de invocaciones de G3 como reejecución de §9 (Freeze §16.2).
5. Se **propone**, sin bloquear, para la decisión de G3: hacer explícito en 16.11 el camino «STOP resuelto sin cambio del trabajo → reverificación como reejecución de la fase», y añadir a
   los hechos remotos el número de omitidas.
