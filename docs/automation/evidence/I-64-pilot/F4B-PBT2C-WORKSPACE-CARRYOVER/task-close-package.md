# Paquete de cierre — F4B-PBT2C-WORKSPACE-CARRYOVER (PB-T2c)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `b19683412ec1202bac141aa80654eddad274a023`, con attempts **1 de 3**. Las obligaciones heredadas de PB-TC y PB-TC2 se cumplen en el modo WORKSPACE, y lo fijan:
- **CT-PBT2C-01:** el valor exacto y el estilo de cotas llegan al borrador;
- **CT-PBT2C-02:** el aviso del lado B sin frentes;
- **CT-PBT2C-03:** los 49 gestos de CT-PBT1C-02 llevan su edición al borrador.

Cada una tiene un mutante de una línea que la hace fallar. **PB-T2 queda completa.**

**Autoridades:**
- **El contrato** `F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json`: paquete de PB-T2 v2.9, AuthorityRevision `92232ed3` y ChainBaseSha `05beabfd`.
- **Las decisiones del Owner:**
  - F4B-E28;
  - F4B-E35 (RR);
  - F4B-E37 (Snapshot K);
  - F4B-E40 (`F4B-DRAFTS/coordinator/owner-pbt2c-nc1-decision.md`).
- **Las disposiciones:** F4B-E39 y F4B-E40.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `92232ed3` y BASE `05beabfd` | F4B-E39, por F4B-E28 (pregunta 4). CA-T2c en seis rondas; la última, EMITIBLE 0/0/2. La tabla de CT-PBT2C-03, medida por la sesión: aplican 42, en 6 no hay nada que aplicar y 1 se niega |
| Preparaciones no lanzadas | `b5fa` y `R20261119T203049Z-a19d` | La expresión del pie de la tabla, ya corregida, y un P-02 falso |
| Planificación | `R20261119T203120Z-65ce` | Aceptada al primer lanzamiento: fidelidad 8/8, A1-A8 8/8 y 17 criterios. nc4 `R20261119T203312Z-d138` |
| Worker | `R20261119T203340Z-05b7` | IMPLEMENTATION_COMPLETE. RED `7aeb6170` y GREEN `b1968341`, sin reparación |
| Corridas aisladas | `m1` a `m3` | Cada mutante hace fallar por aserción el CT de su fila; m3, además, CT-02 |
| CI del RED | `44618262418` | tests y ui-tests en failure, build-ui en success, build-plugin omitido |
| CI del GREEN | `44619492077` | 4/4. Core 19 094; UI 2 450 (2 429 pasadas y 21 omitidas) |
| Revisión directa | — | Sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/2**, más OWNER-01 (informativo) |
| Verificación | `R20261119T220812Z-e59c` | **EXECUTION_VERIFIED** / NONE, 14/14 |
| nc1 | `R20261119T221258Z-ea01` | CONTROL NOT PASSED. Variación de PD-F4-04: la cascada del SHA falso; STOP S-04. **El Owner: «Aceptar y ampliar la firma»**, de modo que `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, y nace la firma PD-F4B-01 para el resto de F4B |
| nc2 | `R20261119T222128Z-1ff5` | **CONTROL PASS** (Scope, STOP) |
| nc3 | `R20261119T222719Z-4f9f` | **CONTROL PASS** (FreeText, REWORK) |

**Relevos:**
- 0 participantes en la cadena lanzada;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| PBT2C-IR-M01 | El gesto y el clic de CT-01 no pasan por `Paso`; además, un comentario inexacto | `docs/ideas-futuras.md` |
| PBT2C-IR-M02 | El Worker leyó INV-PBT2C-10 de forma más estricta que el contrato; la atribución es correcta igualmente | `docs/ideas-futuras.md` |
| OWNER-01 (informativo) | En WORKSPACE, un lado B declarado sin frentes sobrevive en el borrador; en CLASSIC se pierde al reabrir | Se informa al Owner. Qué escribe el panel queda para PB-T3 y PB-T4 |
| CT-PBT2C-03 (M-13 de CA-T2c) | `PostBox`, `CellInOutBeamBox` y `CellInOutPeralteBox` son gestos inertes con este catálogo: no se prueban en el camino al borrador | `docs/ideas-futuras.md` |
| PD-F4B-01 | La firma nueva de nc1 y su sustituto, antes de los controles de PB-T3a | F4B-E40 |

**Siguiente:** emitir PB-T3a con el paquete de PB-T3 v4.4, por F4B-E35 (pregunta 3), tras la remedición sobre este cierre.
