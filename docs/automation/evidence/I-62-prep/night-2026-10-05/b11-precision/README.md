# I-62 — Precisión del manifiesto B.11 en clausuras pequeñas (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION)

```text
Autoridad:  orden nocturna §10 (decisiones §41). No reemite NDC-01: aquello medía tamaños e incompletitud; esto mide la PRECISIÓN de las aristas
Copia:      el manifiesto F3 regenerado con gen-manifest.py (su informe coincide semánticamente con el custodiado); el manifiesto aprobado no se toca
Complete:   ninguna entrada pasa a true (CompleteFlippedToTrue = 0)
```

## Reproducción

```text
python docs/automation/evidence/I-62-F3/gen-manifest.py <repo con 4c617e82> manifest.json report.json
git show 4c617e82:docs/initiatives/I-62-proposal-v14.md > v14.md
python b11_precision.py manifest.json v14.md b11-precision-result.json
```

## Casos (texto de V14 `4c617e82`; resultado en `b11-precision-result.json`)

| Unidad → objetivo | Clase | Texto | Original (clausura/UNKNOWN, ¿acreditable?) | Copia experimental |
|---|---|---|---|---|
| §8.4#p1 → B.8.3 | INFORMATIVE | «Tabla completa en B.8.3.» (l. 382) | 12/0, sí | 1/0, sí |
| B.3#p1 → C-08 | INFORMATIVE | «… valida sin tocar el núcleo (C-08).» (l. 1849) | 12/2, **no** | 1/0, sí |
| §8.5#row6 → T10 | CONTROL | «`fetch` y clasificar según T10» (l. 416) | 7/5, no | 7/5, no (se conserva) |
| C-08 → B.3 | CONTROL | «B.3: (a) adapter `test-null` …» (l. 2345) | 12/2, no | 12/2, no (se conserva) |
| §8.5#row4 | AMBIGUOUS | «… (16.1, sin cambio)» (l. 405) | 1/1, no | 1/1, no (falla cerrado; su resolución correcta también sería externa) |
| §8.5#p1 → AUTOMATION_PLAN 16.9 #5 | **MISSING** | «`Identity` (`HEAD` = remoto = `CurrentSha`) **no cambia**.» (l. 408) | 1/0, **sí** | 2/1, no |

## Hallazgos (para la revisión del manifiesto por el Architect, B.11)

- **B11-P1 — exceso de aristas por punteros y referencias de prueba.** Un resumen que apunta a su tabla completa, o una regla que nombra la obligación
  que la prueba, se convierte en arista de control (regla O del generador).
  - En `B.3#p1` esa arista arrastra una unidad externa incompleta (S-04, a través de C-08) y vuelve UNKNOWN una premisa que es autosuficiente.
  - Falla cerrado, pero quita utilidad.
- **B11-P2 — omisión peligrosa de aristas de control expresadas por nombre.** Una cláusula que afirma la vigencia de una comprobación de 16.9 por su nombre
  entre comillas inversas no lleva referencia de sección, así que el generador no emite arista. La unidad queda Complete con clausura {sí misma}.
  - Una premisa que la cite se acreditaría sin ver la regla que gobierna.
  - Con la arista, la premisa pasa a UNKNOWN.
  - Medición global: el manifiesto **no tiene ninguna unidad de 16.9**, y **66 líneas** de V14, en **25 secciones**, nombran una de sus 14 comprobaciones
    (Authority 11, Identity 9, Ci 9, Scope 8, Handoff 8…). Cada una es una arista de control candidata ausente. La cifra es aproximada y no está confirmada
    una a una.
  - Es el mismo pasaje que el dossier P4 señala (§8.5 «`Identity` no cambia»).
- **Ambigüedad legítima.** «16.1» sin documento ya falla cerrado (regla R5). No hace falta cambiarlo para la seguridad. Una resolución explícita a
  AUTOMATION_PLAN 16.1 (externa) solo mejoraría la trazabilidad.

## Límites

Seis aristas clasificadas a mano no son una revisión del manifiesto. La clasificación y la medición son evidencia de apoyo para la revisión B.11 del
Architect, que es quien decide qué aristas retirar o añadir. No se cambia el manifiesto F3 ni el generador.
