# I-64 F4B — PB-T2: de la v2.8 a la v2.9 del paquete (solo PB-T2c)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **La recomprobación de la v2.8 para PB-T2c,** la §11 de `independent-review-pbt2c-v25-check.md` (**CA-T2c**): **EMITIBLE, nuevos B 0 / I 0 / M 2**, sin decisiones nuevas para el Owner.
  - M-09 es del paquete;
  - M-10 es de la cadena (las comprobaciones de RR por guion), y no la toca este paquete.
- **Su recomprobación sobre la v2.9,** la §12 del mismo informe: **EMITIBLE, nuevos B 0 / I 0 / M 2**, sin decisiones nuevas para el Owner. M-11 es del paquete y M-12 del generador; las dos se aplican en esta misma v2.9, que aún no está custodiada.
- **Los mensajes de la sesión:**
  - PB-T2b está emitida (F4B-E36: X `a12ea09a`, BASE `59839c71`) con la v2.8, así que su generador ya no cambia;
  - aplicar M-09 y ensanchar las ventanas de D-23;
  - el generador de PB-T2c pasa a la v2.9, con su seco sobre `59839c71`;
  - tras la §12: aplicar M-11 y M-12 en la misma v2.9, con las filas V29-04 y V29-05, y poner el `BOUNDED_RESULT` de PB-T2c;
  - **la Snapshot K** (la decisión del Owner F4B-E37): V29-06 en la misma v2.9 y en el generador, con las rutas que fija la sesión, que quedarán en la custodia del cierre de PB-T2b.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v29.md`. Sale del paquete custodiado en `59839c71`, la v2.8 con la línea de emisión de PB-T2b, y no de la copia local de la v2.8, así que conserva esa línea. Lleva el registro v2.8 → v2.9 (V29-01 a V29-06) delante de los anteriores;
- este archivo;
- `make_pbt2c_contract.py` en la v2.9, con su `BOUNDED_RESULT`, y su ejecutor, con pruebas en negativo nuevas.

**Resultado:** solo cambia PB-T2c. El diseño no cambia, ni lo que se informa al Owner.

## 1. Los cambios

| Id | Fuente | Qué cambia | Dónde |
|---|---|---|---|
| **V29-01** | CA-T2c §11 M-09 | **Las líneas fijas de CT-PBT2C-01 en la ayuda de RR-3:** se añaden la línea de `pb-08-redondeo` y las dos de estilo (STANDARD-ISO y SIMPLEX), que faltaban. Son literales del §5.4, y su comparación byte a byte entre `RedSha` y `RedRepairSha` es parada C07, como la de las demás | §6.0; la `ExpectedEvidence` de PB-T2c |
| **V29-02** | La sesión (D-23) | **Las ventanas del generador de PB-T2c:**<br>- la cabecera son las líneas hasta «Lo que pide el Owner», buscado en las 40 primeras;<br>- la fila de [OD] se busca en las 40 primeras.<br>Caben las líneas de emisión de PB-T2b y de PB-T2c, y las cabeceras que vengan. No cambia el texto del paquete | `make_pbt2c_contract.py` |
| **V29-03** | La sesión | **La custodia:**<br>- PB-T2b quedó emitida con «la v2.8»;<br>- la de PB-T2c gana «esta v2.9» y `v29-delta.md`;<br>- cambian el **Estado** y la cabecera | §6.0, «Custodia»; cabecera |
| **V29-04** | CA-T2c §12 M-11 | **Las doce de CT-PBTC-07, enumerables sin interpretar:** la ayuda de RR-3 de PB-T2c nombra su referencia, `PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`, y el sufijo ` descartes=1` de la última de cada ventana. No son literales del paquete, sino de esa prueba existente, que PB-T2c no toca (C02). El generador comprueba la línea entera del paquete y la `ExpectedEvidence` | §6.0; la `ExpectedEvidence` de PB-T2c |
| **V29-05** | CA-T2c §12 M-12 | **La línea de IR1** se busca en toda la cabecera, hasta «Lo que pide el Owner», como las demás de V29-02, y no en las 12 primeras líneas. No cambia el texto del paquete | `make_pbt2c_contract.py` |
| **V29-06** | F4B-E37 (Owner) | **La Snapshot K en P-01 de PB-T2c:**<br>- P-01 dice «today Snapshot K, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json»;<br>- su autoridad es la decisión de la K (`F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md`), en lugar de la de la J;<br>- el SHA-256 de la K es `E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE`, y la sonda neutra, `R20261119T154313Z-2a9a`.<br>Los dos archivos se custodian con el cierre de PB-T2b. PB-T2a y PB-T2b se emitieron con la J | las condiciones para emitir; §1; §6.0, «`StopConditions` estándar»; §8.1; §9; `make_pbt2c_contract.py` |

## 2. El contrato de PB-T2c

**Cambia:**
- la entrada de la `ExpectedEvidence` con lo que lee la verificación: la ayuda de RR-3 lleva las tres líneas nuevas (V29-01) y la referencia de las doce de CT-PBTC-07 con su sufijo (V29-04);
- la versión del paquete en el Objective, en `IssuedBy` y en su autoridad, que nombra el registro V29 (V29-01 a V29-06);
- **P-01** pasa a la Snapshot K, y la autoridad de F4B-E19 (la decisión de la J) pasa a la de F4B-E37 (la decisión de la K); siguen 50 autoridades. `IssuedBy` nombra F4B-E37 (V29-06);
- el rango de `f4b-dispositions.md` de las autoridades, que en `59839c71` llega a F4B-E36;
- **`BOUNDED_RESULT`,** que deja de estar pendiente. Es el literal de la §13 de CA-T2c, la recomprobación final sobre el cierre de PB-T2b, con la mención de M-13 y M-14, en ASCII:
  `EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2; M-11 y M-12 aplicadas en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2 (seccion 13, final sobre el cierre de PB-T2b); M-13 y M-14 aplicadas)`

  Pasa el `bounded_check` con el informe entero, con su §13, y las líneas de la v2.9. Lo comprueban `recheck_bounded_final.py` del revisor, con su literal, y el ejecutor, con este.
- **M-13 y M-14 no cambian el paquete:**
  - M-13 pone en la tabla el porqué de las tres filas inertes;
  - M-14 custodia la medición sin el TRX.

  Están en `measure_pbt2c.py`, `issue_pbt2c.py`, F4B-E39 y la evidencia 261.

**`CUSTODY_EXTRA`** gana `v29-delta.md`.

**No cambian:** el alcance, los invariantes, las paradas, los RequiredTests ni la línea base.

**Lo que comprueba el generador, nuevo:**
- la cabecera de la v2.9 con sus dos frases, la de la §11 y la de la §12;
- el registro V29-01 a V29-05, cada fila con su fuente exacta;
- la línea de M-11 en el paquete y en la `ExpectedEvidence`;
- la línea de IR1 en toda la cabecera (M-12);
- **la Snapshot K (V29-06):** P-01 y la fila del §1, con la frase de la cabecera y la fila V29-06. Sin el registro y la decisión de la K en la revisión, un AVISO del cierre de PB-T2b en seco y una parada al escribir. Con los dos, `snapk_check`: el registro es el de la K, con su SHA-256 y la sonda neutra con Pass, y la decisión trae la respuesta literal del Owner.

## 3. El seco sobre `59839c71`

Con la emisión de PB-T2b ya en la revisión, desaparecen los AVISOS de:
- su contrato emitido, cuya C11 se contrasta ya con la de PB-T2c;
- CA-T2b;
- el cierre de PB-T2a;
- F4B-E35, cuyo archivo pasa `rr_check`;
- `v26-delta.md` a `v28-delta.md`.

**Quedan 15 AVISOS:**
- **8 del cierre de PB-T2b:**
  - `PT2B_CLOSE`;
  - la Snapshot K, su registro y su decisión (V29-06);
  - el archivo del modo;
  - sus archivos y sus clases;
  - la línea de MU-T2C-03;
  - `CI_BASE_T2B`;
  - su `task-close-package.md`;
- **7 de la emisión de PB-T2c:**
  - la v2.9 en HEAD;
  - CA-T2c;
  - `pbt2c-anchors-out.txt`;
  - `v29-delta.md`;
  - el encargo;
  - el seco;
  - la entrada de emisión.

**El contrato en seco, con M-11, M-12, el `BOUNDED_RESULT` y la Snapshot K (V29-06):** 57 426 bytes, SHA-256 `13A1FAF6AFFA5F3F1F4E509B08A76FD59BE5F9D8BC2201627B67C45CABF21285`. Los recuentos no cambian: 3 permitidas, 173 prohibidas, 11 invariantes, 33 paradas, 39 RequiredTests, 10 entradas de evidencia y 50 autoridades.

## 4. Lo que no cambia, a propósito

- `make_pbt2b_contract.py` y lo emitido de PB-T2b.
- M-10, que es de la cadena.

## 5. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`17872be8`, `5ddeb3b4`, `b29551ae` y `59839c71`). Sin tocar el worktree, en el que hay delegaciones de PB-T2b en curso (P-08).
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura: `_work/c/make_v29.py`, `patch_t2c_v29.py`, `patch_t2c_v29b.py`, `patch_t2c_v29c.py`, `patch_runc_v29.py`, `patch_runc_v29b.py` y `patch_runc_v29c.py`.

## 6. La remedición sobre el cierre de PB-T2b

`measure_pbt2c.py` escribe `pbt2c-anchors-out.txt`:
- **las anclas de PB-T2c:** los textos de los tres mutantes sobre el cierre, que localiza el generador en seco;
- **la tabla de CT-PBT2C-03:** sale del TRX de una corrida temporal de UI de los 49 gestos en WORKSPACE, fuera de todo commit (V26-02).

La corrida usa una clase temporal, `PushBackPbt2cCt03ProbeTests.cs.txt`:
- toma los gestos por reflexión de la clase cerrada de CT-PBT1C-02;
- usa las recetas de `PushBackWorkspaceSupport`;
- tiene marcadores para la API que solo existirá con el GREEN de PB-T2b.

La ejecuta la sesión tras el cierre de PB-T2b. Los dos archivos se custodian con la emisión de PB-T2c.
- **Ni dotnet ni git de escritura.**
