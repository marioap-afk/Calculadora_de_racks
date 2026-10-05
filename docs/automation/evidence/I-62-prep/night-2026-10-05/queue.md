# I-62 — Cola de trabajo de la orden nocturna del 2026-10-05 (recuperable; no es un contrato del protocolo)

```text
Orden            = D:\IDs\I-62\I-62_orden_nocturna_autonoma.md (texto pegado; identidad en decisiones §41)
Recepción        = 2026-10-05T05:58Z · límite = 13:58Z (ocho horas como máximo, no como mínimo)
Perfil observado = claude-opus-5-5 / xhigh (get_session self); requerido por el mandato = claude-opus-5-5 / xhigh
Estados          = TODO | RUNNING | DONE | BLOCKED_AUTHORITY | BLOCKED_TECHNICAL | SUPERSEDED (solo de esta cola; no son enums del estado)
Método           = sin Workflow ni subagentes: la orden no autoriza paralelismo ni otros roles («no añadas paralelismo si no existe autoridad y
                   aislamiento demostrables»; las dos invocaciones del Architect «no autorizan otros roles»)
```

| ID | Entrada exacta | Autoridad | Resultado esperado | Depende de | Estado | Evidencia | Próxima acción |
|---|---|---|---|---|---|---|---|
| N-00 | rama `architecture/portabilidad-coordinador-principal` en `1614908d`; `origin/main` = `bb0d5522` (I-63 integrada) | orden §4; WORKFLOW §4.2 (rebase al abrir si el trunk avanzó) | arranque comprobado; rebase con mapa original → imagen; Core en la base nueva | — | DONE | [rebase-map.json](rebase-map.json); punta rebasada `83035d99`; Core 12618/12618; push `--force-with-lease` sobre `1614908d`; CI 37270782878 (4/4) | — |
| N-01 | A-1 blob `9c621fce` (PROPUESTA) y el resultado literal de la revisión R20261005T044948Z-ac67 (`output.json`) | orden §3.A, §5 | A-1 corregida: A62-A1R-01..03 y O1..O5, con O2 preparado | N-00 | DONE | A-1 blob `39c2f831` (evidencia §44.2) | — |
| N-02 | arnés `a1-counterexamples.py` (73 trazas) | orden §3.A, §5 | trazas nuevas para R01..R03 y los opcionales; conjuntos exactos; cobertura | N-01 | DONE | 94 trazas, todas PASS; arnés `078bd48c`, resultado `1e534996` | — |
| N-03 | paquete, disposición, decisiones, evidencia, estado y contrato | orden §3.A, §3.G, §6 | entrega coherente publicada con CI exacta | N-01, N-02 | DONE | `252be61e`, CI 37272010368 (push, 4/4); coherencia del paquete en el commit siguiente (evidencia §44.4) | — |
| N-04 | kit de revisión v3 (lecciones de R20261005T044948Z-ac67) | orden §3.E, §6 | kit preparado; tarea de la app creada una vez: HUMAN_LAUNCH_REQUIRED | N-03 | DONE | objeto `9dcfc08d` (CI 37272599376, 4/4); kit custodiado en [R20261005T063359Z-86e3](../../I-62-architect-A-1/R20261005T063359Z-86e3/README.md); autoprueba del auditor v3 PASS; preflight 3/3 fiel; custodia `ff424a48` con CI 37273621297 (4/4) | — |
| N-04L | lanzamiento de la revisión R20261005T063359Z-86e3 | orden §3.E, §6 | sesión revisora separada sobre `D:\r62-arch-a1r3` | N-04 | DONE: el Owner la lanzó (06:40Z); CHANGES REQUIRED A62-A1S-01..02; auditor v3 NOT_ACCREDITED por un defecto propio, v3.1 ACCREDITED (evidencia §49) · invocación 1 de 2 | HUMAN_LAUNCH_REQUIRED: el único transporte limpio es la tarea de la app `task_7b4dd3ee`, que exige el clic del Owner (Codex: OD-2; Claude CLI: OD-3) | si el Owner la lanza: custodia, auditoría v3 y CI; si no, nada |
| N-05 | P4 (VerificationTargetSha / CustodyHeadSha) | orden §3.B, §7 | dossier de propuesta para una A-2 futura y arnés Git desechable | — | DONE | [p4-dossier.md](p4-dossier.md); arnés `p4/` (20 escenarios sintéticos y 2 reales de I-63, todos PASS; 10 contraejemplos de 16.1; determinista); commit `75e93e91`, CI 37275024919 (4/4) | decisiones del dossier §10 |
| N-06 | P8 (granularidad de reintentos) | orden §3.B, §7 | dossier de propuesta y prototipo de contadores | — | DONE | [p8-dossier.md](p8-dossier.md); prototipo `p8/` (10 escenarios × literal y dos perfiles de prueba, todos PASS; ningún tope elegido) | decisiones del dossier §10 |
| N-07 | P1, P2, P3, P5 y P6 | orden §3.C, §8 | helpers experimentales probados, sin rol nuevo | — | DONE | [helpers/README.md](helpers/README.md); 18 pruebas OK; lecturas reales de G1-G4 y de 4 TRX de I-63 en `helpers/real-readings.json` (bytes fuera del repo, `D:\r62-exp-night\trx`); commit `4eb3ce6b`, CI 37275825561 (4/4) | — |
| N-08 | prototipos de F4 de `I-62-prep/f4/` | orden §3.D, §9 | prototipos al último A-1; secuencias combinadas; versión C# en un clon aislado si queda capacidad | N-01 | RUNNING | [f4-exp/README.md](f4-exp/README.md): 16 secuencias combinadas PASS; hallazgo **A62-A1A-01** corregido en A-1 (blob `cdcd98d2`) y observaciones F4X-OBS-01/02 | C# experimental en un clon aislado |
| N-04R | kit v3b para `cdcd98d2` | orden §3.E, §6 | kit regenerado sobre el commit nuevo | N-08 | SUPERSEDED: preparado (`D:\r62-arch-a1r4-run`, R20261005T072212Z-e65c) y su tarea `task_008b4c96` retirada al saber que la revisión 1 ya corría | — |
| N-12 | ciclo de corrección 1 (A62-A1S-01..02, O1..O4) | orden §3.F | A-1 corregida (blob `03dd822d`), arnés de 100 trazas, registros | N-04L | DONE | evidencia §49 | — |
| N-13 | revisión 2 (última invocación) del objeto corregido | orden §3.E, §6 | kit v3c y tarea de la app creada una vez | N-12 | TODO | — | kit sobre el commit de N-12 |
| N-09 | manifiesto B.11 (copia experimental) | orden §10 | precisión: control, informativa, ambigüedad, faltante; clausuras pequeñas | — | TODO | — | — |
| N-10 | portabilidad | orden §10 | reconstrucción desde artefactos custodiados en un clon `--no-local` | N-08 | TODO | — | — |
| N-11 | informe final | orden §12 | resumen de diez líneas y enlaces | todo | TODO | — | — |
