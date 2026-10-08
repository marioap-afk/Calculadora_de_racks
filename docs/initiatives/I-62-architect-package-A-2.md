# I-62 — Paquete de revisión del Architect (enmienda A-2 corregida: A62-A2-01 y A62-A2-O1..O3)

```text
A-2            = PROPUESTA (versión corregida) — sin revisión formal acreditada
Revisión previa = la corrida R20261008T014941Z-68fe sobre d47f71b6 dio CHANGES REQUIRED (A62-A2-01 REQUIRED; A62-A2-O1..O3 OPTIONAL) y quedó
                 FORMAL_ACCREDITATION = NOT_ACCREDITED (decisiones §56, punto 1); su análisis queda como evidencia. El Coordinator aceptó A62-A2-01
                 como ACCEPTED REQUIRED y A62-A2-O1..O3 como ACCEPTED OPTIONAL (decisiones §56, punto 2)
Architect      = REVIEW REQUIRED: una re-revisión formal acreditada de la A-2 corregida exacta (cambio MATERIAL: LIFECYCLE §6; decisiones §55, punto 7;
                 §56, punto 3)
Coordinator    = veredicto PENDING
Owner          = sin decisión identificada para el acuerdo; si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = UNA re-revisión (decisiones §56, puntos 3 y 9). Transporte preferido: una celda `claude-cli` medida y elegible (decisiones §56, puntos
                 6-9; autorización del Owner CLAUDE-CLI-I62 = A); si no es elegible, HUMAN_LAUNCH_REQUIRED. Sin reintento automático. No se reutiliza la
                 sesión de la revisión anterior
Aplicación     = ninguna antes de AGREED: sin B3 de FX-04a y sin bloque de medición nuevo de codex-cli (decisiones §56, punto 3)

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-2.md                                          blob f1e1d6f08d3cf677500794c7d0019433a4dc7d4e
Blob anterior revisado:                                                 d47f71b66ba86c31f857f0d8c2d2437636947af0 (commit b280f707; publicado en 4a059afa)
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Enmienda anterior (sin cambio): docs/initiatives/I-62-A-1.md blob c01899a72b940503bb85a0fab42bc085c603fd0f (AGREED, decisiones §43)
Base de main: bb0d5522e8411f66a51fdfb3f1f0d0514b737453
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-2.md` = `f1e1d6f0…` en el commit del recibo de
> publicación. Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Cierre:    disposición explícita de A62-A2-01 (CLOSED | STILL_OPEN) y de las precisiones A62-A2-O1..O3 sobre la versión exacta.
Además:    disposición de las preguntas Q-A2-01..Q-A2-07 (A-2 §8; Q-A2-02 y Q-A2-05 ya resueltas en el delta) como REQUIRED, OPTIONAL o sin hallazgo;
           necesidad o no de una decisión del Owner; confirmación de que A-2 no cambia contratos B.1-B.11, state/v2, §20, A-1, AUTOMATION_PLAN 16.x ni F4.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-2 exacta, junto con el veredicto del Coordinator, la convierte en enmienda acordada del Freeze. A partir de ahí, una disposición del
Coordinator puede nombrar la B3 de FX-04a y autorizar el bloque de medición nuevo. Un REQUIRED abierto solo lo cierra o lo rebaja quien lo emitió o
quien tenga esa autoridad. La sesión no declara ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-2.md` | `f1e1d6f0…` | el objeto completo, con §11 «Cambios frente al blob `d47f71b6`» |
| delta exacto: `git diff b553608c <commit> -- docs/initiatives/I-62-A-2.md` | — | el cambio frente al blob revisado `d47f71b6`, que b553608c contiene sin cambios desde b280f707 |
| `docs/initiatives/I-62-architect-review-A-2.md` y `docs/automation/evidence/I-62-architect-A-2/R20261008T014941Z-68fe/output.json` | (en el mismo commit) | registro y resultado literal de la revisión anterior (A62-A2-01, O1..O3) |
| `docs/automation/decisions/I-62.md` §51-§56 | (en el mismo commit) | OD-2d-PROBE, selección de B, B1, la disposición §55 y la disposición §56 (hallazgos aceptados, `claude-cli`) |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | cláusulas que se enmiendan o se leen: Anexo D (D.3 completo, D.4, D.5, D.6 y D.8), §17 (fila F6), §18 (fila OD-2) y la fila que nombra P-07 en la tabla de §9.3 (línea 657) |
| `docs/AUTOMATION_PLAN.md` §16.8 y §16.11 | (en el mismo commit) | definición de P-07 (§16.11) y su contexto (§16.8) |
| `docs/initiatives/I-62-A-1.md` | `c01899a7…` | formato de una A-n acordada; presupuestos de bucle y principio sin reinicios que A-2 no debe alterar |
| `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608…` | identidad del Freeze |
| `docs/INITIATIVE_LIFECYCLE.md` §3, §5 y §6 | `f19896a8…` | formato de A-n, REQUIRED y M-01..M-08 |
| `docs/automation/evidence/I-62-evidence.md` §64, §67, §70-§75 y §78-§83 | (en el mismo commit) | sondas consumidas, actualizaciones automáticas de Codex, B1, B2, instantánea del origen y revisiones |
| `docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md` y `comparison-v2-analysis.json` | (en el mismo commit) | hechos de B2 (KICKOFF_SCHEMA_NOT_DELIVERED, FAIL bruto 2/23) |
| `docs/automation/evidence/I-62-A2/a2-guards.py` | `b72d26ea…` | guardas mecánicas G1-G5 |
| `docs/automation/evidence/I-62-A2/a2-guards-result.json` | (en el commit de publicación, el siguiente al de la corrección) | resultado de `a2-guards.py run --head <commit de la corrección>`; su campo `Head` nombra ese commit |

Para las cláusulas de V14 basta leer su sección. El archivo es grande, y no hace falta expandir los documentos que cita su prosa.

## 3. Resumen del delta corregido

- **A2-P1** (al final de V14 D.3). Una sesión de Principal de F6 que el Coordinator acredite INVALID_LAUNCH queda como evidencia histórica. No puede
  ser PASS ni FAIL por las diferencias de su comparación, y el escenario sigue UNVERIFIED.
  - **Nuevo (A62-A2-01):** una corrida con una lectura prohibida (D.6) o con otra violación observada del protocolo que fija el FAIL de su escenario
    (D.4; filas FAIL de FX-04b y de D.8) no se acredita INVALID_LAUNCH: conserva ese FAIL y no habilita la reejecución.
  - **Nuevo (O1):** otra acreditación ordinaria del Coordinator (p. ej., INVALID_TEST_ORACLE) no es PASS ni FAIL de D.4 ni habilita por sí sola la
    reejecución.
  - Como máximo una reejecución limpia extraordinaria por escenario en F6, con disposición del Coordinator y consumo autorizado por el Owner (OD-5
    no lo cubre); +1 al tope de la fila y, si lo hay, al total de la ronda, como máximo una vez por fila; P-07 la comprueba.
  - **Nuevo (O3):** antes de autorizarla, el Coordinator y el Owner confirman que el presupuesto restante basta; la reejecución solo dispone de los
    presupuestos restantes y no repone ninguno.
  - Sin reinicios; N11 no participa; FX-04b sigue la sesión de FX-04a; un nuevo fallo del arnés no da otra reejecución; no se aplica a D.5 ni a
    sesiones que no sean de Principal.
- **A2-P2** (al final de V14 D.3). Un solo bloque nuevo de como máximo dos sondas de solo lectura por actualización observada, para el par exacto,
  con autorización por bloque (Coordinator y consumo del Owner) y OD-2 sobre la huella resultante. Las sondas anteriores siguen contadas, no hay
  trabajo de modelo con observación obsoleta y las sondas no usadas caducan con otra actualización; P-07 comprueba el tope del bloque.
  - **Nuevo (O2):** el par sucesor solo cuenta como observado con su huella resultante estable; una observación durante una actualización a medio
    completar no fija el par ni una medición reutilizable.
- **Ninguna otra cláusula cambia de texto.**

## 4. Preguntas para la revisión

1. ¿Cierra la versión exacta A62-A2-01 sin efectos laterales (D.4, D.6, paso 7 de FX-04a, FX-04b, D.8)?
2. ¿Implementan O1, O2 y O3 exactamente lo que el Coordinator aceptó (decisiones §56, punto 2)?
3. ¿Sigue cada delta fiel a las reglas candidatas del Coordinator (decisiones §55, puntos 5 y 6) y mínimo?
4. ¿Es correcta la materialidad (M-03 y M-04 sí; las demás no)?
5. ¿Prueban las guardas G1-G5 que A-2 no cambia producción, F4 ni las superficies normativas, y modelan bien cada regla numerada, incluidas las
   correcciones?
6. Q-A2-01..Q-A2-07 de A-2 §8.

## 5. Condiciones de la invocación

- **Frontera:** decisiones §56, puntos 3 y 6-9; autorización del Owner CLAUDE-CLI-I62 = A (medición y uso read-only de `claude-cli` para ARCHITECT y
  REVIEWER de I-62, incluida esta re-revisión, sujeta a celda elegible, consumo cubierto y Actor/Session/Context de OD-6).
- **Transporte preferido:** una invocación `claude-cli` lanzada por la sesión principal tras la caracterización acotada de decisiones §56, punto 7,
  y solo si la celda es elegible (punto 8). Se ejecuta en un clon limpio del commit de publicación, sin la transcripción de la sesión autora ni del
  Coordinator, sin memoria de otras sesiones, con el cierre de insumos cerrado y permisos de solo lectura. Si la celda no es elegible:
  HUMAN_LAUNCH_REQUIRED, con una sesión nueva distinta de la anterior.
- **Independencia (OD-6 alternativa 1):** Actor, Session y Context REQUIRED; Provider PREFERRED. El revisor es un proceso nuevo, sin el contexto de la
  sesión autora; mismo proveedor que el autor, y se declara.
- **Contrato de acciones y auditoría:** los del kit custodiado para esta re-revisión. Los falsos positivos del auditor anterior (`python - <arg>` y
  `python -X <opción>`) se corrigen **antes** del lanzamiento. La regla de los scripts propios queda explícita: un script en línea no lee archivos del
  cierre; las lecturas del cierre se hacen con las acciones de lectura permitidas. Ni el cierre ni las acciones se amplían después de ejecutar.
- **Insumos:** los de §2, en el commit del recibo de publicación.

## 6. Lo que este paquete no hace

- no aplica A-2: ningún texto congelado, materializado ni de producción cambia;
- no autoriza B3 ni ninguna sonda;
- no declara veredictos;
- no modifica A-1, el Freeze, ADR-0048 ni la candidata A-3.
