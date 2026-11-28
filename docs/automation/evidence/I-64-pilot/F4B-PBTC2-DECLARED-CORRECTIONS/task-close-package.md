# Paquete de cierre — F4B-PBTC2-DECLARED-CORRECTIONS (PB-TC2)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4`, con attempts **1 de 3**. La parada prevista PBTC2-C14 no gastó intento, por la orden D1 del Owner.

**Autoridades:**
- el contrato `F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json` (AuthorityRevision `8708d28f`; BaseSha = ChainBaseSha `eedb069c`);
- el paquete `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md` v2;
- las decisiones del Owner F4B-E09 (`owner-pbt1-pre-decisions.md`), F4B-E11 (`owner-d1-d2-decisions.md`, D1) y la de nc1 (`owner-pbtc2-nc1-decisions.md`, F4B-E12).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Planificación | `R20261118T054551Z-6ac2` | **RECHAZADA** solo por la copia en la comparación byte a byte: ExpectedEvidence[1] cortada; en ForbiddenWriteScope, el índice 38 con otro nombre y `tests/RackCad.Tests/` repetido (130 entradas). A4 fail; STOP P-03; sin Worker |
| Replanificación única (PBTC2-C08) | `R20261118T055036Z-c73e` | Fidelidad PASS en las 8 colecciones y A1-A8 8/8; 11 AcceptanceCriteria del Controller. No gasta intento |
| nc4 | `R20261118T055514Z-6700` | Oráculo cumplido |
| Worker, primera invocación | `R20261118T055535Z-38d3` | **BLOCKED / STOP PBTC2-C14**, la parada prevista en la medición previa (D-PBTC2-07). Fila T2-3: el BOM tiene 15 líneas con el valor exacto frente a 14 con el redondeado (postes 150 x32 y 150,0004 x4 frente a 150 x36). K1, K1o, K2 y K4 vacías; MaxDelta 0,00044. Sin commits |
| Vuelta al Owner | `F4B-DRAFTS/pbtc2/owner-return-c14.md` | **D1 (F4B-E11): «CONTINUE WITH THE FIX… resume/relaunch the same correction work WITHOUT consuming a product attempt»** |
| Worker, reanudación | `R20261118T113927Z-037c` | La medición `m8b` es **idéntica** a la de `38d3` (82 de 82 líneas). RED `3677847c` (4 métodos nuevos) y GREEN `b5c09f0b` (W y WC) |
| CI del RED | `44298811020` | ui-tests en failure, por los 4 nuevos y por aserción; tests, build-ui y build-plugin en success |
| CI del GREEN | `44300874797` | 4/4. UI 2 401 Passed y 21 Skipped de 2 422; Core 19 070. Son **exactamente** las cifras previstas |
| Revisión directa | — | Conforme |
| Revisión adversarial independiente | — | **ACEPTABLE CON MENORES 0/0/4**, más IRC-O01 informativo |
| Verificación del Controller | `R20261118T123744Z-a2c6` | `EXECUTION_VERIFIED` / NONE, 14/14, con D1 como disposición de entrada |
| nc1 | `R20261118T124024Z-1d1d` y `R20261118T130053Z-d6fd` | `R20261118T124024Z-1d1d`: CONTROL NOT PASSED con Handoff, Identity y Ci y Remote en pass. Owner: «Es la firma exacta: aplica el sustituto (Recomendado)». El sustituto `PBTC2-NC1-IDENTITY-SIGNAL-01` dio **FAIL** en la propiedad 5: la evidencia de Identity no escribe el SHA falso. Por error se ejecutó dos veces, `R20261118T125901Z-2803` y `R20261118T125908Z-aba4`, con el mismo resultado. Owner: «Repetir nc1; si falla, aceptarlo (Recomendado)». `R20261118T130053Z-d6fd`: CONTROL NOT PASSED, además con Remote. Queda **`nc1 = SATISFIED_BY_OWNER_DISPOSITION`** solo para este caso, sin cambiar las firmas |
| nc2 | `R20261118T124320Z-683c` | CONTROL PASS: Scope detectado, sin la firma de PD-F4-01 y sin sustituto |
| nc3 | `R20261118T124722Z-efea` | CONTROL PASS: FreeText, REWORK |

**Relevos:**
- 0 participantes en toda la cadena;
- la Snapshot I y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Lo que corrige PB-TC2 (cambios declarados de RACKEDITAR)

- **H-31:**
  - el hueco entre lados, la longitud del módulo y las cuatro avanzadas anulables dejan de redondearse al pasar por la caja, con la regla `Keep`;
  - un módulo calculado ya no pasa a manual al pasar por su longitud;
  - R-03 (i) vale para las seis cajas: escribir algo que se ve igual conserva el exacto (IRC-M04).
- **OWNER-1:** en «Ambos lados», con el lado B presente, una caja mixta que recibe un valor lo aplica a los dos lados. Sin B, o fuera de «Ambos», una caja sin editar conserva el exacto.
- **El BOM:** con valores exactos distintos, el BOM los distingue (D1).
  - **IRC-O01, informativo:** la línea separada sale con la descripción larga del catálogo para el mismo perfil, porque `BomBuilder.Merge` toma la primera descripción no vacía.
  - Ya ocurría sin tocar el panel de avanzados.
  - Cae en las reglas de presentación existentes que D1 admite. No se propone ningún cambio.

## 3. Límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| IRC-M01 | `PushBackDeclaredCorrection2MixedBoxTests` declara solo `NumR`. Está dentro de la cota de INV-PBTC2-07, pero la entrega no lo lista en `Deviations` | Desviación declarada en este cierre |
| IRC-M02 | El método temporal del paso 1 no corría dentro de `EditorDiscardPrompt.Substitute` (riesgo de un `MessageBox` real); no se materializó | Lección: las corridas temporales también van con el aviso de descarte sustituido |
| IRC-M03 | D1 no estaba custodiada en el GREEN | Se resuelve en este cierre: D1, D2 y las dos vueltas quedan custodiadas, y F4B-E11 queda anotada |
| IRC-M04 | R-03 (i) solo se declaraba para el módulo | Se añade una nota en [SC] (§2) |
| — | El paso 4 en modo WORKSPACE | PB-T2 (OB-06.3) |
| — | La regla de «Ambos lados» en el alcance y en los datos del frente, sin prueba propia; M-07 | Red de PB-T1 |

**Siguiente:** volver a medir el paquete de PB-T1 v2.2 sobre `b5c09f0b`, con una comprobación acotada, y emitir PB-T1a (F4B-E09 y F4B-E11).
