# I-62 — Paquete de revisión del Architect (enmienda A-2: presupuestos de recuperación de F6)

```text
A-2            = PROPUESTA (candidata) — sin revisión
Architect      = REVIEW REQUIRED: una revisión formal acreditada de la A-2 exacta (cambio MATERIAL: LIFECYCLE §6 y decisiones §55, punto 7, exigen
                 Architect independiente + Coordinator)
Coordinator    = veredicto PENDING
Owner          = sin decisión identificada para el acuerdo; si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = UNA, preparada según decisiones §55, punto 8; no hay transporte automático elegible (§5): HUMAN_LAUNCH_REQUIRED. Sin reintento automático
Aplicación     = ninguna antes de AGREED: sin B3 de FX-04a y sin bloque de medición nuevo de codex-cli (decisiones §55, puntos 2, 11 y 13)

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-2.md                                          blob d47f71b66ba86c31f857f0d8c2d2437636947af0
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Enmienda anterior (sin cambio): docs/initiatives/I-62-A-1.md blob c01899a72b940503bb85a0fab42bc085c603fd0f (AGREED, decisiones §43)
Base de main: bb0d5522e8411f66a51fdfb3f1f0d0514b737453
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-2.md` = `d47f71b6…` en el commit del recibo de
> publicación. Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Además:    disposición de las preguntas Q-A2-01..Q-A2-05 (A-2 §8) como REQUIRED, OPTIONAL o sin hallazgo; necesidad o no de una decisión del Owner;
           confirmación de que A-2 no cambia contratos B.1-B.11, state/v2, §20, A-1, AUTOMATION_PLAN 16.x ni F4.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-2 exacta, junto con el veredicto del Coordinator, la convierte en enmienda acordada del Freeze. A partir de ahí, una disposición del
Coordinator puede nombrar la B3 de FX-04a y autorizar el bloque de medición nuevo de `codex-cli`. Un REQUIRED abierto solo lo cierra o lo rebaja quien
lo emitió o quien tenga esa autoridad. La sesión no declara ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-2.md` | `d47f71b6…` | el objeto completo: alcance, cláusulas anteriores, los dos deltas, aplicación prevista, semántica antes y después, materialidad, no impacto, consecuencias del Owner, preguntas, evidencia y veredictos |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | cláusulas que se enmiendan o se leen: Anexo D (D.3 completo, D.4, D.5, D.6 y D.8), §17 (fila F6), §18 (fila OD-2) y la fila de P-07 en §8 |
| `docs/initiatives/I-62-A-1.md` | `c01899a7…` | formato de una A-n acordada; presupuestos de bucle y principio sin reinicios que A-2 no debe alterar |
| `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608…` | identidad del Freeze |
| `docs/INITIATIVE_LIFECYCLE.md` §3, §5 y §6 | `f19896a8…` | formato de A-n, REQUIRED y M-01..M-08 |
| `docs/automation/decisions/I-62.md` §51-§55 | (en el mismo commit) | OD-2d-PROBE, selección de B, B1 INVALID_TEST_ORACLE y la disposición §55 con el texto candidato del Coordinator |
| `docs/automation/evidence/I-62-evidence.md` §64, §67, §70-§75 y §78-§80 | (en el mismo commit) | sondas consumidas, actualizaciones automáticas de Codex, B1, B2 e instantánea del origen |
| `docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md` y `comparison-v2-analysis.json` | (en el mismo commit) | hechos de B2 (KICKOFF_SCHEMA_NOT_DELIVERED, FAIL bruto 2/23) |
| `docs/automation/evidence/I-62-A2/a2-guards.py` | `91c9c2a4…` | guardas mecánicas G1-G5 |
| `docs/automation/evidence/I-62-A2/a2-guards-result.json` | (en el commit de publicación, el siguiente al de A-2) | resultado de `a2-guards.py run --head <commit de A-2>`; su campo `Head` nombra ese commit |

Para las cláusulas de V14 basta leer su sección. El archivo es grande, y no hace falta expandir los documentos que cita su prosa.

## 3. Resumen del delta

- **A2-P1** (al final de V14 D.3). Una sesión de Principal de F6 que el Coordinator acredite INVALID_LAUNCH queda como evidencia histórica. No puede
  ser PASS ni FAIL por las diferencias de su comparación, y el escenario sigue UNVERIFIED. Con una lectura prohibida registrada no se acredita
  INVALID_LAUNCH: rige el FAIL de aislamiento de D.6. La sesión cuenta en el tope y habilita como máximo una reejecución limpia extraordinaria por
  escenario en toda F6, con estas condiciones:
  - una disposición del Coordinator que la nombre y el consumo autorizado por el Owner, que OD-5 no cubre;
  - +1 al tope de la fila y, si lo hay, al total de la ronda, como máximo una vez por fila, también en las filas compartidas; P-07 la comprueba;
  - ningún contador se reinicia, la fila N11 no participa y FX-04b sigue la sesión de FX-04a;
  - un nuevo fallo del arnés no da otra reejecución;
  - no se aplica a D.5 ni a sesiones que no sean de Principal.
- **A2-P2** (al final de V14 D.3). Una actualización automática invalida un runtime ya medido después de agotar sus sondas de solo lectura de «Sondas
  previas». Las sondas anteriores siguen contadas, sin descontarse ni ampliar topes, y no hay trabajo de modelo con la observación obsoleta. Puede
  autorizarse un solo bloque nuevo por actualización:
  - como máximo dos sondas de solo lectura para el par exacto (`BinaryHash`, `AppVersion`) ya observado;
  - autorización por bloque (disposición del Coordinator y consumo del Owner), nunca anticipada, y sin segundo bloque para el mismo par;
  - OD-2 sobre la huella resultante; nada se reinicia;
  - otra actualización, antes, durante o después de las sondas, invalida el bloque, y sus sondas no usadas caducan;
  - las sondas del bloque se cuentan aparte de los lanzamientos de Codex y del «+ 2 sondas» de los totales, y P-07 comprueba el tope de dos del
    bloque.
- **Ninguna otra cláusula cambia de texto**: ni una celda de las tablas de D.3 o D.8, ni D.4, D.5, D.6, §17 o §18, ni contratos, `state/v2`, §20, A-1
  o F4.

## 4. Preguntas para la revisión

1. ¿Implementan los deltas exactamente las reglas candidatas del Coordinator (decisiones §55, puntos 5 y 6)? ¿Es necesaria y mínima cada precisión
   añadida (exclusión de D.5, solo sesiones de Principal, autorización por bloque con consumo del Owner, conteo aparte de las sondas)?
2. ¿Quedan los dos deltas libres de contradicciones con D.4 (resultados), D.6 (FAIL de aislamiento), P-07, los totales de D.3, D.8, §18 (OD-2) y A-1?
3. ¿Es correcta la materialidad (M-03 y M-04 sí; las demás no)?
4. ¿Prueban las guardas G1-G5 que A-2 no cambia producción, F4 ni las superficies normativas, y modelan bien cada regla numerada?
5. Q-A2-01..Q-A2-05 de A-2 §8.

## 5. Condiciones de la invocación

- **Frontera:** decisiones §55, punto 8. No hay transporte automático elegible, así que la sesión prepara el paquete completo y registra
  HUMAN_LAUNCH_REQUIRED, sin afirmar que la revisión arrancó.
  - `codex-cli` está en P-01 / STOP, con la asignación de sondas agotada (decisiones §55, punto 11): la celda del Architect no tiene invocación medida
    con el binario vigente.
  - `claude-cli` no está autenticado (OD-3 RECHAZADA).
  - La sesión principal no puede abrir sesiones, y un subagente comparte su contexto, lo que no cumple Session ni Context de OD-6 alternativa 1.
- **Transporte preparado:** el Owner abre con un clic una sesión nueva de `claude-desktop-session` desde la tarjeta de tarea que deja la sesión
  principal. Esa sesión trabaja sobre un clon limpio del commit de publicación, sin remoto y sin memoria de proyecto, con el texto de la orden y el
  cierre custodiados antes del lanzamiento. Precedente: la revisión r5 de A-1, lanzada igual, quedó ACCREDITED (evidencia §57 y §58).
- **Independencia (OD-6 alternativa 1):** Actor, Session y Context REQUIRED; Provider PREFERRED. El revisor no es la sesión autora ni comparte su
  contexto. Es del mismo proveedor que el autor, y eso se declara.
- **Contrato de acciones y auditoría:** los del kit custodiado (cierre de insumos, acciones permitidas, auditor posterior y prueba previa del auditor),
  adaptados del kit v4 de la revisión r5 de A-1. Ni el cierre ni las acciones se amplían después de ejecutar.
- **Insumos:** los de §2, en el commit del recibo de publicación, sin la transcripción ni la memoria de la sesión autora.

## 6. Lo que este paquete no hace

- no aplica A-2: ningún texto congelado, materializado ni de producción cambia;
- no autoriza B3 ni ninguna sonda;
- no declara veredictos;
- no modifica A-1, el Freeze ni ADR-0048.
