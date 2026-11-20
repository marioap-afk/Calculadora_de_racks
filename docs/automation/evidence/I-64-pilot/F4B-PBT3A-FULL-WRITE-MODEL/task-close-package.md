# Paquete de cierre — F4B-PBT3A-FULL-WRITE-MODEL (PB-T3a)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `6919e17af9fdd5c702d454a97a3c5b6cc6be1739`, con attempts **2 de 3**.

El modelo de escritura completa del borrador de Push Back queda en Application, en ocho archivos nuevos de `Draft/`:
- el estado completo y su comparador;
- el asiento del estado editable;
- el materializador y el lector completos;
- la composición, con `ForFullWrite` y `KeepOn`;
- la disposición de vistas;
- el clasificador de conflictos.

Ni la ventana, ni el piloto, ni el Plugin cambian.

**Autoridades:**
- **El contrato reemitido:** `F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json`, con el paquete de PB-T3 v4.5, AuthorityRevision `5bda5569` y ChainBaseSha `485b5651`.
- **El análisis de la corrección:** `.../1/R20261120T001738Z-7d92/analysis.md`.
- **Las decisiones del Owner:**
  - F4B-E35 (pregunta 3, RR);
  - F4B-E37 (Snapshot K);
  - F4B-E40 (PD-F4B-01);
  - F4B-E42 (`owner-pbt3a-g9-decision.md`);
  - F4B-E43 (`owner-pbt3a-nc2-decision.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `2d876831` y BASE `18e4b783` | F4B-E41, con la v4.4. CA-T3a con la recomprobación de la v4.4 |
| Planificación, intento 1 | `R20261120T001442Z-903c` | Aceptada: 8/8, A1-A8 8/8 y 16 criterios. nc4 `R20261120T001714Z-7aa0` |
| Worker, intento 1 | `R20261120T001738Z-7d92` | **BLOCKED, PBT3A-C02, antes del RED.** `ForWrite` en un archivo que nombra el piloto, contra la guarda G9. Sin commits |
| Reemisión y corrección | X `5bda5569` y BASE `485b5651` | F4B-E42. El Owner eligió «Renombrar ForWrite». La v4.5 trae `ForFullWrite` y el barrido de las guardas. La §11 de CA-T3a dio EMITIBLE 0/0/2 |
| Planificación, intento 2 | `R20261120T024916Z-fc50` | Aceptada: 8/8, A1-A8 8/8, `CorrectionOf` exacto y 17 criterios. nc4 `R20261120T025200Z-9811` |
| Worker, intento 2 | `R20261120T025240Z-f006` | IMPLEMENTATION_COMPLETE. RED `5dbc9661` y GREEN `6919e17a`, sin reparación. G9 pasa en los dos |
| CI del RED | `44666242222` | tests y ui-tests en failure, build-ui en success, build-plugin omitido |
| CI del GREEN | `44668584248` | 4/4. Core 19 107; UI 2 451 (2 430 pasadas y 21 omitidas) |
| Revisión directa | — | Sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/9** |
| Verificación | `R20261120T034433Z-6d41` | **EXECUTION_VERIFIED** / NONE, 14/14 |
| nc1 | `R20261120T035005Z-c736` | **CONTROL PASS** (Identity, STOP) |
| nc2 | `R20261120T035748Z-ad66` | CONTROL NOT PASSED, con la firma exacta de PD-F4-01 |
| Sustituto sobre nc2 | `R20261120T040923Z-882e` | **FAIL** en la propiedad 8, porque el literal de PB-T2 no reconoce el texto de PB-T3. **El Owner eligió «Aceptar y ampliar el sustituto»:** `nc2 = SATISFIED_BY_OWNER_DISPOSITION` |
| nc3 | `R20261120T040354Z-5eac` | **CONTROL PASS** (FreeText, REWORK) |

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| PBT3A-C02 (intento 1) | El nombre cerrado `ForWrite` chocaba con la guarda G9 | Corregido en la v4.5 y en el intento 2 (F4B-E42) |
| PBT3A-IR-M01 a M09 | Recetas y oráculos de CT-06, CT-12 y CT-13: un caso tautológico, una lectura tardía, `Read` no total con un escaneo nulo y ramas del lector sin ejercer | `docs/ideas-futuras.md` |
| La preparación `c71e` | Se abortó antes de crear la carpeta porque los archivos de base eran de la emisión anterior | Declarada |
| El sustituto de nc2 | La propiedad 8 está atada al literal de PB-T2 | Se amplía con una lista cerrada, con revisión, antes de los controles de PB-T3b |

**Siguiente:** emitir PB-T3b, por F4B-E35 (pregunta 3).
