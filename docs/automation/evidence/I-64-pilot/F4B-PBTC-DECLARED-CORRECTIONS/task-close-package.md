# Paquete de cierre — F4B-PBTC-DECLARED-CORRECTIONS (PB-TC)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `4440f67d4c76fb2ec019a566231353acdcca61f6`, con attempts **1 de 3**.

**Autoridades:**
- el contrato `F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json` (AuthorityRevision `a9e7195a`; BaseSha = ChainBaseSha `48fe199e`);
- el paquete `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` v2.2 y su registro de mediciones;
- las decisiones del Owner antes de emitir (F4B-E06) y su orden de emitir (F4B-E07);
- las decisiones del Owner sobre OWNER-1 (`F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md`) y sobre nc1 (`F4B-DRAFTS/coordinator/owner-pbtc-nc1-decision.md`), las dos en F4B-E08.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Planificación | `R20261118T020031Z-8db0` | **RECHAZADA** solo por la copia en la comparación byte a byte: el invariante del índice 15 (INV-PBTC-16) se copió sin las palabras finales que nombran `f4b-dispositions.md`. A5 fail; STOP P-03; sin Worker |
| Replanificación única (PBTC-C08, F4B-E03, pregunta 2) | `R20261118T020622Z-7171` | Comparación byte a byte PASS en las 8 colecciones; A1-A8 8/8; 10 AcceptanceCriteria del Controller; sin OpenQuestions. No gasta intento |
| nc4 | `R20261118T021200Z-b7b5` | Oráculo cumplido: solo A3 falla con el AllowedWriteScope ampliado |
| Worker (`claude-sonnet-5-5`/high, 220 turnos) | `R20261118T021306Z-069b` | RED `f665edac`: los 6 archivos nuevos de prueba (10 métodos) y la línea del atributo de SUS-01 a SUS-04. GREEN `4440f67d`: solo W, WC y WX |
| Medición previa de la base (D-PBTC-10) | `m10`, en la entrega | F1 y M-02 dan solo `PalletDepth: 48 -> 48.00044`, con K1 a K4 vacíos o en falso y `maxDelta` 0 en los 8 planos. **PBTC-C14 no se activa** |
| CI del RED | `44223960356` | ui-tests en failure; tests, build-ui y build-plugin en success; sin cuelgue ni timeout. UI: 2 418 resultados, 10 Failed (los nuevos, por aserción), 21 Skipped y 2 387 Passed. Core: 19 070/19 070. Ninguna prueba existente falla |
| CI del GREEN | `44225456328` | 4/4 success. UI: 2 397 Passed y 21 Skipped de 2 418. Core: 19 070/19 070. Son **exactamente** las cifras previstas en el §6.9 |
| Revisión directa | — | Conforme. D-1 (menor): la entrega cita los artefactos con el prefijo erróneo `System.Xml.XmlElement/`, aunque los archivos están en su sitio |
| Revisión adversarial independiente | — | **ACEPTABLE CON MENORES 0/0/5**. M-01 = OWNER-1 |
| Disposición del Owner sobre OWNER-1 | chat | **«Registrar y arreglarlo luego (Recomendado)»** (§3) |
| Verificación del Controller | `R20261118T031948Z-a6f6` | `EXECUTION_VERIFIED` / NONE, 14/14. Su único hallazgo es D-1, que no cambia los resultados |
| nc1 | `R20261118T032437Z-2812` | **CONTROL NOT PASSED**: Identity y S-04 detectados, pero con fallos de más (Handoff primera; además Remote, Scope, Tests y Trailer, cada uno con evidencia que nombra el CurrentSha falso). No es la firma EXACTA de PD-F4-04 → Owner: **«Aceptarlo solo esta vez (Recomendado)»**, `nc1 = SATISFIED_BY_OWNER_DISPOSITION` (§3), sin sustituto |
| nc2 | `R20261118T032822Z-2db5` | CONTROL PASS: Scope detectado; sin la firma de PD-F4-01 y sin sustituto |
| nc3 | `R20261118T033422Z-412a` | CONTROL PASS: FreeText, REWORK |

**Relevos:**
- 0 participantes y 0 no atribuibles persistentes en toda la cadena;
- la Snapshot I y el binario acreditado `5449c1f0`;
- `main` = `812baf87`, sin tocar.

## 2. Alcance e identidad (medido con git)

- `git diff --name-only 48fe199e 4440f67d`: 13 rutas, todas del AllowedWriteScope y ninguna bajo el ForbiddenWriteScope.
- El RED (`48fe199e..f665edac`) solo toca `tests/`; el GREEN (`f665edac..4440f67d`) solo toca W, WC y WX.
- En los cuatro archivos de SUS, el diff es exactamente la línea del atributo, con el literal del §5.2.
- El literal del aviso en WC es idéntico byte a byte al de `owner-pbtc-decisions.md` (pregunta 3).
- El diff no toca nada del caso H-31.

## 3. Lo que corrige PB-TC (cambios declarados de RACKEDITAR)

- **D-PB-06 (a):** los diez campos (el fondo y la tolerancia de la tarima, el peralte del poste, la escala de anotación, la altura del primer nivel, y frente, alto, peso, claro y largo manual de la celda) conservan su valor exacto con la regla `Keep`.
  - Se aplica en la carga, «Restaurar», la selección de celda, «Ambos lados», el alcance, los datos del frente y la biblioteca.
  - Las cajas siguen mostrando tres decimales. El peralte IN/OUT base no era una pérdida: lo normaliza el catálogo.
- **D-PB-06 (b):** el estilo de cotas cargado viaja en cada pasada, sin control nuevo.
- **D-PB-08:** el aviso de solo texto en la banda de estado, con el texto que eligió el Owner.
- **D-PB-11:** tras «Restaurar estándar», los controles avanzados muestran el estándar y «Actualizar» escribe lo reiniciado.

**La disposición del Owner sobre OWNER-1 (F4B-E08):**
- La revisión independiente encontró un hueco del diseño del paquete, no una desviación del Worker. En «Ambos lados», con una caja **mixta**, escribir el número que se ve del exacto de un lado no iguala los dos lados: ese lado conserva su exacto y la caja vuelve a quedar mixta. El efecto numérico es menor que 0,0005, y antes de la corrección los dos lados quedaban iguales.
- El Owner eligió «Registrar y arreglarlo luego (Recomendado)». Queda como **limitación conocida**.
- La propuesta de arreglo vuelve al Owner **antes del contrato de PB-T1**, junto con el caso H-31, porque es la misma regla.

**La disposición del Owner sobre nc1 (F4B-E08):**
- nc1 detectó el CurrentSha falso y paró con S-04, pero marcó en fail más comprobaciones de las que admite la firma EXACTA de PD-F4-04: Remote, Scope, Tests y Trailer además de Handoff, Identity y Ci. La evidencia de cada una nombra el SHA falso.
- Por PBTC-C11 y [DP] (pregunta 1), la variación volvió al Owner, que eligió «Aceptarlo solo esta vez (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbtc-nc1-decision.md`).
- Queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION` para esta ocurrencia. No se ejecuta ningún sustituto y las firmas autorizadas siguen siendo EXACTAS.

## 4. Límites y arrastres

**Límites de PB-TC:**
- sin AutoCAD: el dibujo real con el estilo y los valores exactos, la geometría del reinicio de D-PB-11 y el aspecto del aviso quedan para Smoke-PB;
- el modo WORKSPACE del paso 4 de D-PB-06 y del aviso queda para PB-T2 (obligación anotada en F4B-E06).

| Id | Qué | Va a |
|---|---|---|
| OWNER-1 / M-01 | «Ambos lados» con una caja mixta (§3) | Vuelta al Owner antes del contrato de PB-T1, con H-31 |
| M-02 | CT-PBTC-08 es en parte tautológica: `control=False` no puede fallar, `enfocable` lee el valor por defecto y «no bloquea» solo mira «Actualizar» | Paquete de PB-T1 o PB-T2: comparar `IsEnabled` de todos los botones de acción con el aviso visible y sin él |
| M-03 | Ningún CT fija la extensión de (a) en «Ambos lados», el alcance, los datos del frente, «Restaurar» ni la ruta de los portadores del rack; D-PB-11 no tiene caso negativo | Paquete de PB-T1, como red antes de mover el estado; Smoke-PB |
| M-04 / D-1 | Evidencia de la entrega: el prefijo `System.Xml.XmlElement/` en las rutas, tokens pegados en `Command` y `ResultsFile`, `TreeSha` con el commit que se creó después y el CS0184 situado en la build equivocada | Lección para la plantilla del prompt del Worker: rutas relativas comprobadas, y `TreeSha` = el árbol real (`git write-tree`) o null |
| M-05 | Riesgo latente en CT-PBTC-14: si el segundo `Click` lanzara, el `finally` cerraría con cambios de módulos pendientes y saldría un `MessageBox` real (patrón heredado de CT-PB0-05). No ha ocurrido | Paquete de PB-T1: confirmar o cancelar en un `finally` interno |
| Obs. | La reconstrucción de módulos que hoy hace RACKEDITAR con un fondo con decimales no cambia piezas, niveles ni BOM en los casos medidos (F1, M-02) | Dato para el Owner, ya cerrado por PBTC-C14 |

**Vueltas al Owner previstas, que este cierre alimenta, antes del contrato de PB-T1 (OB-06.5):**
- el caso H-31 (`GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables) y OWNER-1;
- OWNER-3 de PB-T0: `pb-21-cola` pierde la cola y `SideB` al Actualizar;
- los demás casos alcanzables de T-2.

**Lección de la sesión:** la regla «sin latitud» del prompt del Worker funcionó: el Worker no reinterpretó nada. El hueco OWNER-1 salió del diseño del paquete, y lo encontró la revisión independiente, que es la barrera prevista.
