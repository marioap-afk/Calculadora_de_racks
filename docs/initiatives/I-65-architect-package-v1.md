# I-65 — Paquete de revisión del Architect, ronda 1 (Proposal V1)

## 1. Encargo

Revisión adversarial de **NEW ARCHITECTURE** ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5) sobre la Proposal V1 de I-65,
antes del Freeze. Veredicto: `AGREED` (cero `REQUIRED` abiertos sobre esta versión exacta) o `CHANGES REQUIRED`, o
`BLOCKED — OWNER DECISION` si falta una decisión reservada. La implementación sigue **no autorizada** hasta que Coordinator y Architect
acuerden el mismo Freeze.

## 2. Entradas (versión completa; ronda 1, sin delta previo)

| Entrada | Ruta | Identidad |
|---|---|---|
| Mandato del Owner | `docs/automation/decisions/I-65-owner-mandate.original.txt` | SHA-256 `ae1a0412…6e56` |
| Contrato | `docs/initiatives/I-65-interop-analisis-estructural.md` | blob en el commit de la revisión |
| Decisiones del Coordinator | `docs/automation/decisions/I-65.md` | ídem |
| Discovery F0 | `docs/initiatives/I-65-discovery.md` | ídem |
| Proposal V1 | `docs/initiatives/I-65-proposal-v1.md` | ídem |
| Sonda P1 (diseño, no ejecutada) | `docs/automation/evidence/I-65-f0/ram-probe-p1.py` | ídem |
| Código de la base | `origin/main` `bb0d5522` (la rama solo añade documentación) | — |
| Entorno instalado | `C:\Program Files\Bentley\Engineering\RAM Elements\` (solo lectura; **no ejecutar RAM Elements ni ningún Python de esa carpeta**) | 26.00.01.015 |

El commit exacto revisado se fija en la orden de revisión.

## 3. Desafíos obligatorios (mandato, «ARCHITECT REVIEW»)

Para cada uno: ¿la Proposal lo impide con una regla congelable **y** una obligación de prueba o de OV?

1. Acoplamiento a RAM filtrándose a Domain o al modelo neutral (D-01, D-02, INV-01, INV-12).
2. Geometría gráfica confundida con geometría analítica (D-05, INV-06).
3. Errores de unidades (D-03, INV-02/03; RC-01/RC-02 de Discovery).
4. Nombre de producto usado como identidad de sección (D-06).
5. Supuestos ocultos de conexión, apoyo y carga (D-10, «Supuestos» de D-02).
6. Archivo del proveedor convertido en autoridad de RackCad (§1, D-12).
7. Supuestos de API no verificados (Discovery §3, §5; D-09; sonda P1).
8. Automatización de interfaz en lugar de interfaz soportada (D-09).
9. Resultados obsoletos (D-12).
10. Sobregeneralización antes de la primera rebanada (D-02 mínimo, IDL-06).
11. Intentar cubrir todos los sistemas en V1 (§9, Q-O1).

## 4. Preguntas específicas del Coordinator

| ID | Pregunta |
|---|---|
| AQ-01 | ¿Basta carpeta + guardas de fuente (D-01) para la frontera de proveedor, o se exige un ensamblado propio en V1? |
| AQ-02 | ¿SI base como almacenamiento canónico (D-03) es correcto frente a guardar en la unidad interna de RackCad (pulgadas) y convertir solo al proveedor? ¿La regla anti-M-01 (reutilizar `LengthUnits`) es suficiente? |
| AQ-03 | ¿La idealización sin offsets rígidos (IDL-03) es aceptable para la primera equivalencia, dado que la referencia manual sigue la misma regla? |
| AQ-04 | ¿La sonda P1 mide lo necesario para congelar unidades y orientación? ¿Falta alguna magnitud o algún riesgo de efecto lateral? |
| AQ-05 | ¿El comando mínimo del Plugin (D-13) invade la propiedad de ID30 («AutoCAD selection/event architecture»), o es compatible como comando de línea sin UI? |
| AQ-06 | ¿Fusionar F1 y F2 del mandato en un gate es correcto según §7 del ciclo de vida? |
| AQ-07 | ¿La plantilla Python fija + carga útil embebida, sin ejecutar Python en la CI (INV-14), deja algún invariante del script sin obligación de prueba antes de F4? |
| AQ-08 | ¿Elegir Cantilever en vez de la cabecera Selectivo (Discovery §7) está suficientemente fundado, o falta evidencia? |
| AQ-09 | ¿El tratamiento de ADR-0017 (consumido; ningún veredicto en RackCad; M-08 si se presentaran ratios) es correcto? ¿Se necesita ADR-0050 antes del Freeze o antes de implementar? |
| AQ-10 | ¿El modelo de errores (D-08) cubre los casos del mandato y evita cualquier exportación parcial, también dentro de RAM? |

## 5. Formato del veredicto

```text
ARCHITECT_VERDICT: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
REVIEW_MODE: <SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN> (+ aclaración)
REVIEWED_COMMIT: <sha>
FINDINGS:
  - ID: A65-V1-nn
    CLASS: REQUIRED | OPTIONAL
    TARGET: <D-xx | INV-xx | IDL-xx | §>
    ISSUE: <qué está mal o falta>
    WHY: <M-xx o regla del ciclo de vida que lo hace REQUIRED>
    FIX: <cambio mínimo aceptable>
ANSWERS: AQ-01..AQ-10
MANDATORY_CHALLENGES: 1..11 → COVERED | GAP(<finding id>)
```

Solo quien emitió un `REQUIRED` puede rebajarlo. Una re-revisión recibirá la versión completa, el delta y la disposición por ID.
