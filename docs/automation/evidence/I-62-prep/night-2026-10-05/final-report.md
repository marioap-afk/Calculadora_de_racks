# I-62 — Informe final de la orden nocturna del 2026-10-05

1. **Qué cambió.**
   - A-1, todavía PROPUESTA, se corrigió tres veces: `39c2f831` (A62-A1R-01..03), `cdcd98d2` (A62-A1A-01, hallado por el autor) y `03dd822d`
     (A62-A1S-01..02, O1..O4).
   - Se custodió la revisión formal 1.
   - **Experimental** (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION): los dossiers P4 y P8 con sus arneses, los helpers P1/P2/P3/P5/P6, F4 experimental
     (secuencias combinadas y C#), la precisión de B.11 y la portabilidad.
2. **A-1.**
   - **Objeto:** `docs/initiatives/I-62-A-1.md`, blob `03dd822d`, commit `411e01ce`.
   - **Revisión formal 1** (R20261005T063359Z-86e3, sobre `39c2f831`): CHANGES REQUIRED, A62-A1S-01..02 y O1..O4. Auditor v3 NOT_ACCREDITED por un
     defecto propio; v3.1 ACCREDITED. Ciclo 1 aplicado.
   - **Revisión 2** (la última invocación): lanzada por el Owner, pero parada desde las 08:08Z en una aprobación de permiso de su sesión. No hay
     veredicto.
   - **Pendiente:** acreditación y disposición del Coordinator; acuerdo Architect + Coordinator.
3. **P4 y P8.**
   - **Estado:** dossiers listos para la evaluación material (MATERIAL; nada aplicado; ningún tope elegido).
   - **Decisiones:** A-2 propia o conjunta; forma del registro de custodia; dónde vive el mapa de custodia; lectura de ADR-0046 #7 frente a los topes
     locales; quién fija los valores.
4. **Pruebas.**
   - **Nuevas:** arnés de A-1 de 73 a 100 trazas; 16 secuencias combinadas; P4 con 20 escenarios sintéticos y 2 reales; P8 con 10 escenarios × 3;
     18 pruebas de helpers; 9 en C#; 6 aristas de B.11; reconstrucción 7/7.
   - **Defectos:** A62-A1A-01; falso positivo del auditor v3; PORT-01..03; B11-P1/P2; observaciones F4X-OBS-01/02.
5. **Commits y CI.**
   - De `252be61e` a `c66137b9`, todos con CI push 4/4 salvo los dos últimos, todavía en curso al escribir (ver la cola).
   - Árbol limpio. Sin escrituras en `main`, ROADMAP, HANDOFF, FOUNDATIONS ni el índice ADR.
6. **Bloqueo y próxima decisión.**
   - **Owner:** aprobar o detener el permiso pendiente en la sesión revisora `local_8e95d755`.
   - **Coordinator:** disponer la revisión R20261005T063359Z-86e3 y su acreditación.

## Enlaces

- **Matriz de tareas:** [queue.md](queue.md).
- **Resultados reproducibles:** evidencia de I-62 §44-§53; [portability/README.md](portability/README.md).
- **Código experimental:** [p4/](p4/), [p8/](p8/), [helpers/](helpers/), [f4-exp/](f4-exp/) (incluye `csharp-experimental.patch`),
  [b11-precision/](b11-precision/), [portability/](portability/).
- **Dossiers:** [p4-dossier.md](p4-dossier.md), [p8-dossier.md](p8-dossier.md).
- **Revisiones:** [registro r3](../../../../initiatives/I-62-architect-review-A-1-r3.md); kits en `docs/automation/evidence/I-62-architect-A-1/`
  (R20261005T063359Z-86e3 lanzado; R20261005T072212Z-e65c retirado; R20261005T073911Z-2dfe lanzado y parado).
- **Hallazgos abiertos:**
  - A62-A1S-01..02, corregidos y pendientes de verificación;
  - OBS-A1-01, STILL_OPEN en la revisión 1;
  - F4X-OBS-01/02;
  - B11-P1/P2;
  - PORT-01.
- **Decisiones pendientes:**
  - **Coordinator:** disposición y acreditación de la revisión 1; P4/P8 (A-2).
  - **Architect:** revisión del manifiesto B.11.
  - **Owner:** la sesión revisora parada.
