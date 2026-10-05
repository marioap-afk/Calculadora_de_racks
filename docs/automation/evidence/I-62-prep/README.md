# I-62 — Preparación de los gates posteriores a F2

> **Sin autoridad normativa.** Documentación de preparación ordenada por el Coordinator ([decisiones](../../decisions/I-62.md) §33; clase B: «preparación
> autorizada»). No materializa F3, F4, F6 ni F7. No crea esquemas, pruebas ni textos normativos, no cambia el Freeze (Proposal V14, blob `34ad80ea`) y no
> pide ninguna decisión. Lo que contradiga una autoridad o el Freeze no vale. F3 y siguientes se implementan solo con una orden válida del Coordinator.

| Archivo | Contenido |
|---|---|
| [f3-dossier.md](f3-dossier.md) | F3: mapa de archivos con blobs, planes de los nueve esquemas, plan de pruebas de C-11..C-14, C-30 y C-40, plan de B.11, DC-07 y disposición de la deuda nc2 de I-64 |
| [f4-dossier.md](f4-dossier.md) | F4: mapa de archivos, orden de materialización, matriz de transiciones y su auditoría, plan de validadores, plan de compatibilidad (§16.13, WORKFLOW §12, mapa de cláusulas, C-20b, C-20c) y plan de pruebas de C-15..C-42 |
| [freeze-issues.md](freeze-issues.md) | posibles conflictos del Freeze hallados al preparar F4 (FC-01, FC-02) con sus A-n candidatas, sin aplicar, y observaciones no materiales (SM-01..SM-03) |
| [f6-dossier.md](f6-dossier.md) | F6: escenarios FX-01..FX-06, grafo de dependencias y lo que avanza sin OD-3, OD-4, OD-5 u OD-7 |
| [owner-decision-packets.md](owner-decision-packets.md) | paquetes OD-2, OD-3, OD-4, OD-5, OD-7 y DEP-F4-YAML, cada uno con su selección corta; recordatorio de OD-1 |
| [ov-scripts.md](ov-scripts.md) | guiones de OV-I62-01..06 |
| [f7-ready-closure.md](f7-ready-closure.md) | borrador factual de FOUNDATIONS, disposición de ideas futuras, paquete de OV, READY-01..09, requisitos del Candidato y plan de cierre e integración |

Estado de referencia: F2 en `1eddbf48dbefd9685e68d63c0560bacf484d2dcb` (CI 37075239120, 4/4), con su paquete de gate en
[I-62-F2/f2-gate-packet.json](../I-62-F2/f2-gate-packet.json).

## Revisión del 2026-10-04 (orden de continuación)

| Archivo | Contenido |
|---|---|
| `a1-review-kit/` | kit de la revisión del Architect de A-1, listo para lanzar (prompt, cierre, preflight de fidelidad, esquema del resultado, auditoría posterior con autoprueba, plantilla de custodia) |
| `f4/` | matriz de campos de `state/v2`, mapa de impacto de A-1, plano de pruebas C-15..C-42 y cuatro prototipos medidos (YAML, oráculo, rebase, resolver) |
| `f4/ndc-proto/` | clausura normativa de las 1 825 unidades sobre el manifiesto B.11: riesgo operativo NDC-01 (`f4-dossier.md` §8) |
| `f6/recipes.md`, `f6/fx04a/` | recetas de FX-01..FX-06, diseño del fixture y ensayo mecánico de FX-04a |
| `owner-decision-packets.md` | paquetes en formato de una línea; OD-2 con la medición del 2026-10-04; DEP-F4-YAML ya no hace falta |
| `ready-candidate.md`, `ov-scripts.md`, `closure-plan.md` | READY-01..09 ejecutable, plantillas de READY-06 y del Candidato, guiones de OV y plan de cierre |
| `transport-options.md`, `repo-research.md` | transportes para una revisión limpia y hechos del repositorio para F4 |
