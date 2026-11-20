# Paquete de cierre — F4B-PBT3B-FULL-WRITE-RUN (PB-T3b)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `c758278548bcdd66e7c55ca15ca1faeb1a431e53`, con attempts **2 de 3**.

El plan y la corrida de la actualización del Workspace de Push Back quedan generalizados en su sitio para el modelo del piloto, conectado, y para el modelo completo de PB-T3a, sin conectar. Es la variante (A) de D-1: un solo código.
- **Los dos modelos:**
  - la barrera de diagnósticos con `PushBackEditorOutputGate`, el único cambio visible del piloto, sobre un diseño bloqueado;
  - `ComparePost` genérico;
  - un solo `BeginMutation` y un solo `Seal`.
- **El modelo completo:**
  - el borrado de laterales obsoletos con la regla del último vínculo;
  - el origen de POST reanclado cuando se borra el origen;
  - el rechazo del frontal B de un rack sin lado B y el renombrado con el nombre del borrador;
  - los métodos públicos propios `PrepareFull`, `ExecuteFull` y `ComparePostFull`.
- **La interfaz nueva** `IPushBackPilotViewEraser`, para borrar una vista por su clave.
- **Las seis pruebas de F4 que sustituye D-1** (S-1 a S-6) llevan `Skip` con su motivo literal y su equivalente, de E-1 a E-6.

Ni el Plugin, ni Domain, ni el anfitrión, ni el puerto del panel, ni la ventana cambian, y no se crean vistas.

**Autoridades:**
- **El contrato reemitido:** `F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json`, con el paquete de PB-T3 v4.6, AuthorityRevision `2bb3b759` y ChainBaseSha `c7441653`.
- **El análisis de la corrección:** `.../1/R20261120T055235Z-a45b/analysis.md`.
- **Las decisiones del Owner:**
  - F4B-E29 (D-1, la variante (A));
  - F4B-E35 (pregunta 3, RR);
  - F4B-E37 (Snapshot K);
  - F4B-E40 (PD-F4B-01);
  - F4B-E43 (el sustituto de nc2 ampliado);
  - F4B-E45 (`owner-pbt3b-overload-decision.md`);
  - F4B-E46 (`owner-pbt3b-nc3-decision.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `05b6f04e` y BASE `eab27a9e` | F4B-E44, con la v4.5. CA-T3b dio 0/0/4, y su recomprobación 0/0/1 |
| Planificación, intento 1 | `R20261120T054837Z-814c` | Aceptada: inyección PASS, A1-A8 8/8 y 17 criterios. nc4 `R20261120T055127Z-1ca3` |
| Worker, intento 1 | `R20261120T055235Z-a45b` | **BLOCKED, PBT3B-C02 y PBT3B-C07, antes del RED.** Con el esqueleto de la v4.5, `RackCad.Tests` no compilaba: dos CS0121 en una prueba existente de F4. Sin commits |
| Reemisión y corrección | X `2bb3b759` y BASE `c7441653` | F4B-E45. El Owner eligió «Renombrar los métodos nuevos»: `ExecuteFull` y `ComparePostFull`, y `PrepareFull` por un tercer choque, de reflexión. La compilación de comprobación da Core 19 107/19 107. La §13 de CA-T3b dio EMITIBLE 0/0/2 |
| Planificación, intento 2 | `R20261120T075023Z-ba4a` | Aceptada: inyección PASS, A1-A8 8/8, `CorrectionOf` exacto y 17 criterios. nc4 `R20261120T075216Z-0d48` |
| Worker, intento 2 | `R20261120T075251Z-a9e0` | IMPLEMENTATION_COMPLETE. RED `9fda277e` y GREEN `c7582785`, sin reparación del RED. INV-PBT3B-15 da exactamente la variante (A) |
| CI del RED | `44704503614` | tests en failure; ui-tests y build-ui en success; build-plugin omitido. Fallan por aserción los 13 métodos de `PushBackFullWrite` y los 6 de la equivalencia, y los seis de D-1 salen en `Skipped` |
| CI del GREEN | `44707482956` | 4/4. Core 19 124: 19 118 pasadas y 6 omitidas, las de D-1. UI 2 451 (2 430 pasadas y 21 omitidas), sin cambio |
| Revisión directa | — | Sin hallazgos bloqueantes ni importantes |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/9.** OWNER-1 y OWNER-2 son solo informativos |
| Verificación | `R20261120T091621Z-2b95` | **EXECUTION_VERIFIED** / NONE, 14/14 |
| nc1 | `R20261120T092052Z-0d2f` | CONTROL NOT PASSED, con la firma exacta de PD-F4B-01, la cascada del SHA falso |
| Sustituto sobre nc1 | `R20261120T093907Z-71d6` | **PASS:** nc1 = SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE (`41440edc`, `--task PBT3B`) |
| nc2 | `R20261120T092518Z-aea7` | **CONTROL PASS** (Scope, STOP). El sustituto de nc2 no hizo falta |
| nc3 | `R20261120T093218Z-fd73` | CONTROL NOT PASSED. FreeText detectó la mutación, pero antes falló Tests por el «19 128» del contrato, frente a los 19 124 reales. **El Owner eligió «Aceptar por disposición»:** nc3 = SATISFIED_BY_OWNER_DISPOSITION |

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| PBT3B-C02 y C07 (intento 1) | Las sobrecargas nuevas con el mismo nombre volvían ambiguo un `null` en una prueba existente, y un tercer choque era de reflexión | Corregido en la v4.6 y en el intento 2 (F4B-E45) |
| La planificación `814c` | La sesión ejecutó por error `fidelity_pbt3b.py`, que es de F4, y sobrescribió la copia de la comparación byte a byte. Se restauró desde `session-inject` | Declarada en su relevo. Desde entonces el guion se niega a correr fuera de `inject_opaque.py` |
| El relevo del trabajo | El guion ponía `Skipped` en TestArtifacts, campo que el esquema no admite. Se quitó y se volvió a escribir el registro | Declarado |
| nc3 | La evidencia esperada daba un solo total («19 128») con dos ramas de omitidas. En la rama «+ 6», la real, el total es 19 124 (`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/`) | Disposición del Owner (F4B-E46). Desde PB-T3c, el total exacto de cada rama |
| PBT3B-IR-M01 a M09 | Fidelidad de recetas (CT-04, CT-05, CT-06, CT-09, CT-11 y E-3), una línea que depende de la cultura, el reparto de los delegados, `IsDirty`, CT-13 (ii) y el registro de la entrega | `docs/ideas-futuras.md` |
| M-07 de CA-T3b | Redactar el §5.0 y la precondición 12 como «ejecutar la suite completa de Core» | La revisión del paquete para PB-T3c |
| Sin AutoCAD | El borrado en el dibujo, el escritor y la fachada son de PB-T3c; aquí los cubre el doble `PilotEraseScopeDouble` | PB-T3c |

**Siguiente:** emitir PB-T3c, por F4B-E35 (pregunta 3), tras la revisión del paquete sobre este cierre y su comprobación acotada.
