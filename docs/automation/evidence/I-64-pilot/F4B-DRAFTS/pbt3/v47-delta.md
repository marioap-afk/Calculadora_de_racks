# I-64 F4B PB-T3 — Delta del paquete, v4.6 → v4.7 (el cierre de PB-T3b, F4B-E46, y la §13 de CA-T3b)

**Qué es:** la v4.7 del paquete del contrato de PB-T3, `I-64-f4b-pbt3-gate-contract-package-v47.md`. Parte de la v4.6 custodiada (313 702 bytes, sha256 `09210abe…`; la custodiaron la X de la reemisión de PB-T3b, `2bb3b759`, y el cierre de PB-T3b, `fe1c67f4`). La arma `make_v47.py`: cada cambio es una sustitución o una inserción exacta, que tiene que aplicar una sola vez. `check_v47.py` la comprueba frente a la v4.6 y, por objetos git, frente a `fe1c67f4`; su salida está en `check-v47-out.txt`.

**La v4.7 es el paquete de la emisión de PB-T3c** (F4B-E47, con la evidencia 274 para X y 275 para BASE). La remedición sobre el cierre de PB-T3b es R-23 del registro.

## Por qué

- **PB-T3b está CERRADA** (F4B-E46; custodia `fe1c67f4`, hija directa de su GREEN `c7582785`; intento 2 de 3). Los marcadores `<tras el cierre de PB-T3b>` de la v4.6 se pueden rellenar ya, con lo medido en `fe1c67f4`.
- **La decisión del Owner sobre el nc3 de PB-T3b** («Aceptar por disposición (Recomendado)», `F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md`): desde PB-T3c, una evidencia esperada que dependa de cómo cuente la TRX lleva **el total exacto de cada rama**, y no un solo total con una alternativa en las omitidas. El GREEN previsto de PB-T3c de la v4.6 («19 133, o 19 129 si…») era justo eso.
- **La §13 de CA-T3b** (EMITIBLE, B 0 / I 0 / M 2) dejó **M-07** para esta revisión. El §5.0 y la precondición 12 pedían compilar el esqueleto «con las pruebas que hacen reflexión sobre esos tipos». Lo que se hizo, y lo único que cubre los escaneos de ensamblado de `tests/`, fue **ejecutar la suite completa de Core** sobre la copia exportada.
- **La revisión independiente de PB-T3b** (0/0/9, `docs/ideas-futuras.md`, PBT3B-IR-M01 a M09). Tres menores tocan a las recetas: M-01 y OWNER-2 (recetas «por construcción»), M-02 (`InvariantCulture`) y M-07 (`IsDirty` justo antes).
- **CA-T3a M-03**, la fila de PB-T3c de la tabla de RR-3, que la v4.6 dejó aplazada a la emisión de PB-T3c.

## Lo que no cambia

- **El diseño, D-1, ninguna guarda, ninguna prueba y ninguna firma existente.**
- **Lo emitido de PB-T3a y PB-T3b.** `check_v47.py` lo comprueba por secciones: 46 de las 54 secciones de la v4.6 siguen byte a byte. Entre ellas están las quince D-PBT3, §2, §5.2, §5.3, §6.1, §6.2 y §8.2. Cambian solo las ocho que dice el registro.
- **Lo que el generador de PB-T3c copia del paquete al contrato** (34 textos). Cambian solo dos:
  - el `Objective`, solo en «v4.6» → «v4.7»;
  - la línea de la CI.

  En el §6.0 entra una viñeta, la del total por rama. Los invariantes, las paradas, la evidencia y el bloque citado de `PBT3x-C11` no cambian.
- **La fila del doble de PB-T3b** en la tabla de RR-3 (`PilotEraseScopeDouble`) sigue igual. El generador de PB-T3c solo lee las filas `| PB-T3c |`.

## Los cambios

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| **V47-01** | **F4B-E46** (el cierre de PB-T3b) | **Los marcadores, rellenos** con lo medido en `fe1c67f4`, cada uno con su fuente (abajo) | Precondiciones 1 y 9; §4.1; §6.3 |
| **V47-02** | **CA-T3b §13 M-07** | **La comprobación de un esqueleto ejecuta la suite completa de Core** sobre la copia exportada:<br>• en el §5.0;<br>• en la precondición 12, como historia: 19 107 de 19 107, según `compile-check.md` y `out-run3.txt` de la custodia de PB-T3b;<br>• en la precondición 13, nueva, para PB-T3c, con el esqueleto de la fachada | §5.0; precondiciones 12 y 13 |
| **V47-03** | **F4B-E46 (Owner)** | **El total exacto de cada rama:**<br>• la regla, en el §5.0 y en el §6.0;<br>• en el §5.4, CT-PBT3C-01 a 05 son un `[Fact]` cada una;<br>• en el §6.3, el RED y el GREEN previstos, en su única rama, sin alternativa | §5.0; §5.4; §6.0; §6.3 |
| **V47-04** | **CA-T3a M-03** | **La fila de CT-05 de la tabla de RR-3, enumerada.** Las ocho fichas de D2 (`PushBackSharedReferenceGuardTests.cs:35-36` en `fe1c67f4`) y los nombres de las reglas `Rule1`, `Rule1b` y `Rule1c` (en sus líneas 57, 60 y 68) | §6.0 |
| **V47-05** | La sesión (V46-07 para PB-T3c) | **El barrido de sobrecargas, de reflexión y de nombres de PB-T3c** (T3C-1), sobre `fe1c67f4`: sin colisión | §4.1; precondición 13 |
| **V47-06** | La revisión independiente de PB-T3b (PBT3B-IR-M01, M02 y M07) | **Las lecciones de las recetas,** en una nota que no va al contrato. Las CT de PB-T3c son guardas de texto, sin recetas con borradores. Sus autopruebas, si formatean un número, usan `InvariantCulture` | §5.4 (nota) |
| V47-07 | La sesión | **La versión, el estado y la numeración:**<br>• el `Objective` dice «v4.7»;<br>• la cabecera;<br>• §8.1 gana F4B-E46;<br>• F4B-E47, la evidencia 274 y 275, y R-23 | Cabecera; precondición 9; §6.3; §8.1 |

## Los marcadores `<tras el cierre de PB-T3b>`, con su fuente

La v4.6 los nombraba en cinco grupos; son seis sitios del texto, porque el de los `MinSelected` del §6.3 tiene su fila y su nota. Todo se leyó por objetos git en `fe1c67f4`, y `check_v47.py` vuelve a comprobar cada hecho en su fuente.

| # | Sitio | Lo que dice ahora | Fuente, en `fe1c67f4` |
|---|---|---|---|
| 1 | Precondición 1: la `BaseSha` de PB-T3c y el estado | PB-T3b, CERRADA:<br>• custodia `fe1c67f4`, hija directa del GREEN `c7582785`;<br>• RED `9fda277e`;<br>• reemisión con la v4.6, X `2bb3b759` y BASE `c7441653`;<br>• intento 2 de 3; verificación 14/14;<br>• nc1 por el sustituto, nc2 PASS y nc3 por disposición.<br>La CI del GREEN, `44707482956`:<br>• Core 19 124 = 19 118 `Passed` + 6 `Skipped`;<br>• UI 2 451 = 2 430 + 21.<br>La `BaseSha` es la BASE de la emisión, con el `src/` y los `tests/` de `fe1c67f4` | `f4b-dispositions.md` (F4B-E46, la última entrada); `F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt` (la línea del GREEN) |
| 2 | Precondición 9: la custodia de la emisión de PB-T3c | El paquete v4.7 sobre la v4.6, con:<br>• el delta y sus guiones;<br>• la remedición (`pbt3c-anchors-out.txt`, `measure_pbt3c.py` y R-23);<br>• los tres barridos;<br>• el esqueleto (`pbt3c-skeleton.patch`, con `make_pbt3c_skeleton.py`);<br>• CA-T3c con su encargo;<br>• el seco.<br>F4B-E47, con la evidencia 274 y 275 | `f4b-dispositions.md`, `I-64-evidence.md` y el registro de mediciones: las últimas son F4B-E46, la 273 y R-22. La v4.6 es la de `2bb3b759` |
| 3 | §4.1: el barrido de las guardas sobre el cierre | De 92 a 94 clases que leen código, las dos de PB-T3b. Hay 9 coincidencias nuevas, genéricas, de `PushBackWritePathGuardTests`, y ninguna perdida. Su CT-13 (ii) no mira WR ni la fachada. Sin colisión | `sweep-pbt3c-close.txt` (con `guard_sweep.py` y `sweep_t3b_diff.py`, custodiados en el cierre) |
| 4 | §6.3: los `MinSelected` de las guardas del escritor (la fila y la nota de lo heredado) | **86 y 123:** los `Passed` de cada clase en la TRX del GREEN de PB-T3b, sin los `Skip` de D-1 (1 y 2). Lo heredado no baja del contrato reemitido de PB-T3b (`Workspace` de Core, 3 261) | `count-rows-out.txt`: `WorkspacePilotUpdateGuardTests` con 86 `Passed` y 1 `NotExecuted`, y `WorkspacePilotUpdateIntegrationGuardTests` con 123 y 2. El contrato reemitido de PB-T3b y el seco del generador |
| 5 | §6.3: la línea base de la CI y lo previsto | **Base:** Core 19 124 (19 118 + 6 `Skipped`) y UI 2 451 (2 430 + 21).<br>**Una sola rama:**<br>• **RED:** Core 19 129 = 19 119 `Passed` + 4 `Failed` + 6 `Skipped`;<br>• **GREEN:** Core 19 129 = 19 123 + 0 + 6;<br>• **UI, en los dos:** 2 451 = 2 430 + 21 | F4B-E46 y `count-rows-out.txt`. Lo previsto es la cuenta de cinco `[Fact]` nuevos: cuatro que fallan en el RED (CT-01 a 04) y uno que no (CT-05) |

## Para emitir PB-T3c (la precondición 13)

- **El barrido de T3C-1:** hecho sobre `fe1c67f4`, sin colisión (`overload-sweep-pbt3c-fe1c67f4.txt` y `name-check-pbt3c-fe1c67f4.txt`).
- **La compilación de comprobación,** que hace la sesión: el esqueleto de la fachada (`pbt3c-skeleton.patch`, un archivo nuevo, `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`) sobre una copia exportada de la base, fuera del worktree, con:
  - la compilación de Core, UI y el Plugin con `-o`;
  - **la suite completa de Core ejecutada** sobre esa copia.
- **CA-T3c,** con 0 bloqueantes y 0 importantes sin resolver.

## Lo que se deja como estaba

- **La historia:** los registros de cambios, las notas de las reemisiones de PB-T3a y PB-T3b, y las menciones de la v4.6 en lo ya emitido.
- **El GREEN previsto de PB-T3b,** en su línea de la CI del §6.2, que es historia: Core 19 128, con los `Skipped` en + 6 o + 10. El cierre midió 19 124, con 6 `Skipped`, porque la TRX cuenta la `Theory` S-2 con `Skip` como una fila. Es el caso que llevó a F4B-E46.
- **Las tres menciones del marcador** que quedan en la v4.7. Son la cabecera, la fila V47-01 y la fila V46-05 del registro, y ninguna es un sitio por rellenar.
