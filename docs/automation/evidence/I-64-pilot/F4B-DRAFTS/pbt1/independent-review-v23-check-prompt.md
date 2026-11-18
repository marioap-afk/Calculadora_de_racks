> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: comprobación acotada de la v2.3 de PB-T1 (remedición) y del generador del contrato de PB-T1a. Es la última barrera antes de emitir PB-T1a

Eres el revisor de PB-T1: hiciste IRT1-1 y la comprobación de la v2. Sigues siendo independiente y adversarial, y trabajas con las mismas reglas:
- solo lectura;
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.

La rama está en `cd47523b`; el producto es el GREEN de PB-TC2, `b5c09f0b`. No hay ningún Worker en el worktree.

**Qué está decidido:**
- **El Owner resolvió L-1 en D2** (F4B-E11), con 10 condiciones de seguridad y un límite. El texto literal está en `…\scratchpad\hs65\pbtc2\owner-d1-d2-decisions.md` y custodiado en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`.
- **Su orden:** «Re-measure it against the final PB-TC2 product before emission. If that remeasurement changes no material premise: issue the first PB-T1 task normally.»
- **Si das** 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva, la sesión emite PB-T1a sin volver al Owner.

## Entradas, en `<scratchpad>\hs65\pbt1\`

- `I-64-f4b-pbt1-gate-contract-package-v23.md`, con los registros de cambios v2.1 → v2.2 (la incorporación de D2) y v2.2 → v2.3 (la remedición);
- `pbt1-measurements.md`, apartados N y anteriores;
- `make_pbt1a_contract.py`. Puedes ejecutarlo solo en seco, con `python make_pbt1a_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`;
- `remeasure-instructions.md` y `v22-instructions.md`: lo que pidió la sesión;
- tus informes `independent-review-v1.md` e `independent-review-v2-check.md`.

## Qué haces

1. **La incorporación de D2** (v2.2): ¿las 10 condiciones y el límite están cubiertos de forma fiel y comprobable, por subtarea o como arrastre? Mira las lecturas L-4, L-5 y L-6. L-5 aplica el límite también a PB-T4 y a la vida de edición clásica: ¿es una restricción adicional legítima o una decisión nueva?
2. **La remedición** (v2.3):
   - ¿las diferencias declaradas «no materiales» lo son de verdad?
   - Ataca sobre todo el censo corregido de CT-PBT1B-15 (los escritores de `Name`), la huella de CT-PBT1B-16 (el recorrido profundo por valores) y la regla 1b de G-L1 con sus autopruebas (c) y (e).
   - ¿Hay alguna premisa material que la remedición debía detectar y no detectó?
3. **El generador de PB-T1a:** ¿el JSON que produciría coincide con el §6 de PB-T1a de la v2.3?
   - las autoridades;
   - el `AllowedWriteScope` (9) y el `ForbiddenWriteScope` (131);
   - los invariantes;
   - los `RequiredTests` (19);
   - las paradas (30), con las de [DP] literales, C04 y C17 (C15 pasó a un invariante por N-M-07);
   - las celdas;
   - `IssuedBy`;
   - los 15 mutantes.
4. **PB-T1a por sí misma:** ¿se puede emitir y cerrar? Mira la tabla de mutantes sobre `b5c09f0b`, los CT, el RED por mutantes revertidos y la red que fija para PB-T1b y PB-T1c.
5. **Decisiones materiales nuevas para el Owner:** sé estricto.

## El informe

En español, en `pbt1/independent-review-v23-check.md`, con este formato:
- el veredicto arriba;
- §1, método y límites;
- §2, D2;
- §3, la remedición;
- §4, el generador;
- §5, PB-T1a;
- §6, los hallazgos (Id, gravedad, contenido, evidencia y corrección);
- §7, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M y las decisiones para el Owner.

Sin rutas absolutas locales.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, las decisiones nuevas y la ruta.
