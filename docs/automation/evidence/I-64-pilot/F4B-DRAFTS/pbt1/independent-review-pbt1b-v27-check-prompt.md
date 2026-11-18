# Encargo: comprobación acotada de la v2.7 de PB-T1 (corrección de CT-PBT1B-02 y CT-06 tras la parada C15)

Eres el revisor que hizo la comprobación acotada de PB-T1b sobre la v2.5 (`pbt1/independent-review-pbt1b-check.md`). Las reglas son las mismas:
- solo lectura;
- sin compilar ni ejecutar pruebas;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.

**Qué pasó desde tu informe:**
- La v2.6 aplicó tus seis hallazgos, y PB-T1b se emitió con ella (X `1464193f`, BASE `7905b3a9`).
- El Worker `R20261118T193423Z-fb01` paró con PBT1B-C15 antes del RED, sin commits. La mitad estado → diseño → estado de CT-PBT1B-02 da «distintos» en `pb-15-sin-composite`: el frente 0 del lado B pasa de activo a inactivo, con B no presente. Es la normalización `AbsentSlotsB[0]: <absent> -> 0` de `LegacyDeltas`.
- Su lectura de CT-06 (mutar por reflexión) dio falsos fallos.
- La entrega está en `artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T193423Z-fb01/`, con `worker-handoff.json`, `tests/dev/` y el borrador en un parche.

**Las decisiones del Owner (F4B-E20)** están en `…\scratchpad\hs65\pbt1b\owner-pbt1b-c15-decisions.md`:
- «Normalización conocida»: se corrige CT-02;
- «Sin gastar intento».

## Entradas, en `…\scratchpad\hs65\pbt1\`

- `I-64-f4b-pbt1-gate-contract-package-v27.md`, con el registro de cambios v2.6 → v2.7;
- `make_pbt1b_contract.py`. Puedes ejecutarlo solo en seco: `python make_pbt1b_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`;
- `make_pbt1b_contract-dry.txt`;
- la v2.6 (`I-64-f4b-pbt1-gate-contract-package-v26.md`) para comparar.

## Qué haces

1. **CT-PBT1B-02 corregido:**
   - ¿aplica exactamente la decisión del Owner, sin ir más allá?
   - ¿La mitad estado → diseño → estado desde el estado normalizado es medible y discrimina? ¿Lo hace el esqueleto del RED?
   - ¿Las diferencias de los seis racks antiguos, vistas desde el estado, se fijan con lo medido o con el paso 1, sin inventar?
   - ¿Esconde alguna pérdida nueva?
2. **CT-PBT1B-06 aclarado:**
   - ¿la exclusión de las ranuras que el editor vuelve a sincronizar y de `SafetyFacePiece` tiene anclas en el código?
   - ¿Debilita la condición 9 de D2 o el censo?
   - Lee `ct06-failures.txt` y dime si alguno de esos fallos podría ser un defecto real de la copia y no de la lectura del CT.
3. **PBT1B-C15:** ¿la excepción solo cubre la normalización conocida?
4. **El generador:** ¿el JSON coincide con el §6.2 de la v2.7? ¿Cambia algo más del contrato aparte del `Objective` y de `IssuedBy`?
5. **Decisiones materiales nuevas para el Owner.** Sé estricto: una decisión que el Owner ya tomó no es nueva.

## El informe

En español, en `pbt1/independent-review-pbt1b-v27-check.md`, sin rutas absolutas locales. Con:
- el veredicto arriba;
- §1, método y límites;
- §2, CT-02;
- §3, CT-06;
- §4, C15;
- §5, el generador;
- §6, los hallazgos (Id, gravedad, contenido, evidencia y corrección);
- §7, el veredicto: EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE, con B/I/M y las decisiones para el Owner.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, las decisiones nuevas y la ruta.
