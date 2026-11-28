> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: comprobación acotada de la v2 del paquete de PB-TC2, el texto que se emite

Eres el revisor de la v1 de PB-TC2 (`pbtc2/independent-review-v1.md`, EMITIBLE TRAS CORRECCIONES 0/3/7). Sigues siendo independiente y adversarial, y trabajas con las mismas reglas:
- solo lectura;
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c`;
- HEAD `5597c05b`.

**Es la última barrera antes de emitir.** Si das 0 bloqueantes, 0 importantes sin resolver y ninguna decisión nueva para el Owner, la sesión emite PB-TC2 sin volver al Owner (F4B-E09, pregunta 4).

## Entradas, en `<scratchpad>\hs65\pbtc2\`

- `I-64-f4b-pbtc2-gate-contract-package-v2.md`, con su registro de cambios v1 → v2;
- `make_pbtc2_contract.py`, el generador. Puedes ejecutarlo solo en seco, con `python make_pbtc2_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`, que no escribe nada;
- `superseding-section.md`;
- `pbtc2-measurements.md`, apartado I;
- `v2-instructions.md`: lo que pidió la sesión;
- tu informe de la v1.

## Qué haces

1. **El estado de I-01 a I-03 y de M-01 a M-07:** RESUELTO, PARCIAL o NO RESUELTO, con la evidencia en la v2 y en el código.
2. **Lo nuevo de la v2, atacado igual que en la v1:**
   - el miembro `MixedBoth` y su condición frente a WC:452 y WC:604-607;
   - las dos ventanas nuevas de CT-PBTC2-04 (sin lado B y cambio de celda): su fallo en la base y sus líneas en el GREEN;
   - el orden exacto de `descartes=<n>`;
   - las órdenes de INV-PBTC2-04;
   - el umbral de M-04;
   - R2-08.
3. **El generador:** que el JSON que produciría coincida con el §6 de la v2:
   - las autoridades;
   - los 4 `AllowedWriteScope` y los 129 `ForbiddenWriteScope`;
   - los 14 invariantes, en sustancia;
   - los 18 `RequiredTests`;
   - las 31 paradas, con las transcripciones de [DP] literales;
   - las celdas;
   - `IssuedBy`.
4. **`superseding-section.md`:** si es fiel y si está lista para un repositorio público.
5. **Decisiones materiales nuevas para el Owner:** sé estricto.

## El informe

En español, en `pbtc2/independent-review-v2-check.md`, con este formato:
- el veredicto arriba;
- §1, método y límites;
- §2, el estado de los hallazgos de la v1;
- §3, los hallazgos nuevos;
- §4, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M abierto y lo que deba ir al Owner.

Sin rutas absolutas locales.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, si hay alguna decisión nueva y la ruta.
