# Paquete de cierre — F4B-PBT1C-LOSSLESS-STATE-WINDOW (PB-T1c)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `161a0dadccaf5b71886344d834146737b90bb4f3`, con attempts **2 de 3**.
- La ventana clásica de Push Back (W y WC) guarda su estado editable en el `PushBackEditableState` de PB-T1b, en modo CLASSIC y sin cambio observable, con el cambio mínimo de D-PBT1-13.
- **PB-T1 queda completa:** PB-T1a, PB-T1b y PB-T1c.

**Autoridades:**
- el contrato `F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json` (paquete v2.8; AuthorityRevision `af46fcd5`; ChainBaseSha `833ea6bf`);
- el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.8;
- el análisis de la corrección, en `.../1/R20261119T025420Z-ee6c/analysis.md`;
- las decisiones del Owner F4B-E09, F4B-E11 (D2) y F4B-E25;
- las disposiciones F4B-E27 y F4B-E30.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `af46fcd5` y BASE `833ea6bf` | F4B-E27, con la autorización condicional de F4B-E09 y F4B-E11 y la comprobación acotada de la v2.8 aplicada |
| Planificación | `R20261119T025114Z-43f7` | Aceptada al primer lanzamiento: fidelidad PASS 8/8 (las colecciones opacas las pone la sesión, F4B-E21) y A1-A8 8/8. nc4 `R20261119T025329Z-6a10` |
| Worker, intento 1 | `R20261119T025420Z-ee6c` | RED `e5a21989`: la costura inerte y las tres clases; CI `44450941610` conforme. **Paró con PBT1C-C07:** CT-PBT1C-02 comparaba antes del recálculo neutro, y el diseño del compuesto va una pasada por detrás en tres gestos (ya existía). La implementación se validó sin commit |
| Análisis y preparación | `d8ccbcb8` | F4B-E30: corrección en el intento 2 de 3, con FailureClass StopCondition. C-1 (la parte 1 de CT-02 tras el recálculo), C-2 y C-3 |
| Planificación de la corrección | `R20261119T041233Z-cf1f` | Aceptada al primer lanzamiento: fidelidad 8/8 y A1-A8 8/8, con CorrectionOf exacto. nc4 `R20261119T041442Z-2c93` |
| Worker de la corrección | `R20261119T041505Z-11b9` | IMPLEMENTATION_COMPLETE. RED `2faae9da` (solo C-1) y GREEN `161a0dad` (solo W y WC) |
| CI del RED | `44459636816` | tests y ui-tests en failure, build-ui en success y build-plugin omitido. CT-03, CT-01 y CT-02 fallan por aserción; CT-04 pasa |
| CI del GREEN | `44463212627` | 4/4. Core 19 088; UI 2 439 (2 418 pasadas y 21 omitidas). Es la línea base del cierre de PB-T1b más 2 y más 2, como se preveía |
| Revisión directa | — | Sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/5** y OWNER-01, que es informativo |
| Verificación | `R20261119T051834Z-3612` | **EXECUTION_VERIFIED** / NONE, 14/14. Authority pasa con F4B-E25 |
| nc1 | `R20261119T052147Z-e146` | **CONTROL PASS** (Identity, STOP) |
| nc2 | `R20261119T052614Z-1c08` | **CONTROL PASS** (Scope, STOP) |
| nc3 | `R20261119T052958Z-f007` | **CONTROL PASS** (FreeText, REWORK). La coincidencia «limit reached» de los relevos es el texto de S-11 |

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot J y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| PBT1C-C07 (intento 1) | CT-02 comparaba antes del recálculo neutro | Corregido en el intento 2 (F4B-E30, C-1) |
| El desfase de una pasada | El diseño calculado del compuesto va una pasada por detrás tras GapBox, el separador central y la estructura propia. Ya existía. Las salidas que escriben recalculan antes de usarlo; «Restaurar» no (OWNER-01) | `docs/ideas-futuras.md`; a revisar en PB-T2 |
| El aislamiento del configurador | Pruebas de UI existentes escriben `RackFrameConfigurator.layout` en el perfil del usuario al cerrar el configurador real | `docs/ideas-futuras.md`; una tarea propia. Se informa al Owner |
| PBT1CC2-IR-M01 a M05 | Dos comentarios desfasados en W y tres huecos de cobertura de las pruebas | `docs/ideas-futuras.md` |
| ChainRedSha | El RED del intento 1 no se acreditó (null en la corrección; precedente de F4-T4C2) | Sin efecto; la entrega exigía RED |

**Siguiente:** emitir PB-T2a con el paquete de PB-T2 v2.2, por la autorización condicional de F4B-E28 (pregunta 4); su comprobación acotada está hecha (0/0/6, aplicada). PB-T3 espera la autorización de emisión del Owner.
