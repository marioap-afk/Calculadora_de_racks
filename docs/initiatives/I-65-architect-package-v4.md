# I-65 — Paquete de revisión del Architect, ronda 4 (confirmación de la Proposal V3)

## 1. Encargo

Revisión de **confirmación** (ronda 3, respuesta AQ3-03): sustituir los valores [P1] por los medidos y registrar las decisiones del Owner
no debía requerir otra ronda de diseño, salvo falla de clase STOP o resultado Z arriba (ninguno ocurrió). Se pide confirmar que la
Proposal V3 es la **versión congelable** (`AGREED`) o señalar `REQUIRED` concretos. Versión completa: la Proposal V3 es autocontenida.

## 2. Entradas

| Entrada | Ruta |
|---|---|
| Proposal V3 (completa; §16 = disposición) | `docs/initiatives/I-65-proposal-v3.md` |
| Resultados de P1 (matriz de capacidades, problemas) | `docs/automation/evidence/I-65-f0/p1-results.md` |
| Informe crudo, copia ejecutada, capturas | `docs/automation/evidence/I-65-f0/p1-run/` |
| Discovery (adenda §14) | `docs/initiatives/I-65-discovery.md` |
| ADR-0050 (borrador final) | `docs/adr/0050-interoperabilidad-analisis-estructural.md` |
| Decisiones del Owner y del Coordinator | `docs/automation/decisions/I-65.md` §5-§8 |
| Proposal V2.1 (sustituida) | `docs/initiatives/I-65-proposal-v2.1.md` |

## 3. Delta V2.1 → V3 (sustituciones y cambios)

1. §0: FP-01..03 satisfechas, FP-05 nueva (sin falla de clase STOP).
2. D-03: par `LB_IN` medido (lectura exacta, escala verificada, momento lb·in); error propio de RAM en pares métricos.
3. D-04: Y arriba medido; mapeo `X=X, Y=Z, Z=−Y` congelado; **regla de ejes locales de RAM medida** y fórmula del giro emitido.
4. D-06: espacio de nombres global de RAM (`IPE 200` aceptado sin configurar la base); fila con tabla y país, confirmada por un ingeniero.
5. D-07: **sin regla por familia** (Owner Q-O2): material en la fila explícita y verificada de cada sección.
6. D-08: modelo vacío = 0 nudos, 0 miembros, ≤ 1 caso sin cargas (por defecto `CM`, dependiente del idioma); huérfanos verificados.
7. D-09: reglas 3, 5 y 7 confirmadas con P1; regla 9 con tolerancias concretas en `LB_IN`; **regla 10 nueva: no analizar** (diálogo modal).
8. D-11: **cambio material**: el volcado fija `LB_IN` por la API y restaura el par (antes: nunca `units.set` y cambio manual en la
   interfaz); casos sin cargas ignorados.
9. D-14: hechos de análisis y resultados (solo caracterización).
10. INV-07 con la regla medida como fixture; §4 fila de casos sin cargas; OV-I65-03 con doble cara / `NegativeY` (A65-V21-02); separación
    equivalencia ≠ validación de ingeniería (Owner Q-O3).
11. §10: decisiones del Owner (Q-O1 y Q-O7 provisionales con condición, Q-O5 aprobada, Q-O6 pendiente, Q-O8 autorización de F1).
12. §15 nuevo: especificación de la referencia manual `REF-CANT-01` (la tabla numérica golden se deriva en F1; no se inventa).
13. ADR-0050: decisión 5 (equivalencia ≠ validación) y 7 (hechos verificados).

## 4. Preguntas

| ID | Pregunta |
|---|---|
| AQ4-01 | ¿Cierra A65-V1-01 con la corrida de P1 v3.2 y las sustituciones [P1✓]? |
| AQ4-02 | ¿El cambio de D-11 (volcado con `units.set(LB_IN)` por la API y restauración) es aceptable a la luz de P1-U1/P2, o se mantiene la regla V2.1? |
| AQ4-03 | ¿Las tolerancias de la regla 9 y la fixture de INV-07 son ejecutables y no ciegas? |
| AQ4-04 | ¿Provisionalidad de Q-O1/Q-O7 con su condición STOP es compatible con congelar? |
| AQ4-05 | ¿Queda algún `REQUIRED`? Si no, `AGREED` sobre el commit revisado. |

## 5. Formato

El de las rondas anteriores, con `PRIOR_FINDINGS` (A65-V1-01, A65-V21-02) y nuevos hallazgos `A65-V3-nn`.
