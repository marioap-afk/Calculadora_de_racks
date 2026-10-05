# I-62 — Dossier P8: granularidad de los reintentos (propuesta para una A-2 futura)

```text
Estado:      PROPUESTA DE PREPARACIÓN — sin aplicar, sin vigencia y sin consumir A-2 (orden nocturna §3.B y §7; decisiones §41)
Triaje:      P8 = propuesta de cambio material (Coordinator); separada de A-1 y de su revisión
Prototipo:   p8/p8-counters-harness.py — EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION; resultado p8/p8-counters-result.json
Topes:       NINGUNO elegido. Los perfiles A (3/3/3) y B (6/2/3) del prototipo son fixtures de prueba, no propuestas
Supuesto:    P8 se liga a I-63 evidencia §27, §35 (Q-G2-03), §39 (Q-G3-04) y §50.4: el presupuesto por unidad dejó una sola corrección
             para G2-G4 tras dos correcciones en G1
```

## 1. El caso de I-63 (artefactos canónicos)

- G1 consumió dos correcciones de clases distintas: `Ci` y, después, `Authority` con `analysis.md` (evidencia §27.4). `attempts` quedó en 2 de 3.
- Q-G2-03: «`attempts` es por unidad y no se reinicia al cerrar un gate (16.8): G2 empieza con `attempts` = 2 y `AttemptsRemaining` = 1».
- Q-G3-04: queda **una** corrección para G3 y G4 juntos, y G3 es el gate de mayor riesgo (el núcleo).
- En el STOP de G4 (§50.4), la opción 2 habría llevado `attempts` de 2 a 3, y cualquier otro fallo habría acabado en STOP S-11.
- El Coordinator resolvió por la opción 1, sin consumo.

Un fallo del gate menos arriesgado agotó el margen de los gates siguientes. El prototipo lo reproduce en `p8-01`.

## 2. Cláusulas anteriores

- **AUTOMATION_PLAN 16.8:**
  - «`attempts` (estado canónico, por unidad) … sube en 1 cuando la sesión lanza una corrección»;
  - «Contador por clase = correcciones lanzadas en la cadena para una misma `FailureClass`»;
  - «STOP si, ante un nuevo REWORK de clase X, el contador de X ≥ 3, o si `attempts` ≥ `max_attempts` (S-11)»;
  - «Sin reinicios: cambiar de modelo, rol o sesión, o renombrar el error, no reinicia `attempts`, los contadores por clase ni los topes».
- **Proposal V14 §9.3:**
  - tabla de presupuestos: `attempts`, por clase (`TaskId`, `FailureClass`), reejecuciones BLOCKED, recuperaciones e invocaciones;
  - «Un rebinding conserva `TaskId` y los contadores. Una `TaskId` nueva solo por decisión del Coordinator. Si continúa el mismo trabajo, declara
    `ContinuesTaskId` y **hereda** los contadores».
- **Proposal V14 B.8.8:**
  - `counters.correction_launches[]` `{seq, task_id, failure_class, correction_of_run_id, attempts_after, record_version}`;
  - I-P05 (`attempts` no decrece);
  - I-S05 e I-S12.
- **ADR-0046 #7** (aceptado): «Presupuesto único de reintentos. `attempts` del estado canónico, sin reinicios por cambio de modelo, rol o sesión. Un
  máximo de tres correcciones por clase de fallo dentro de una tarea, acotado por los intentos restantes …».
- **ADR-0048** (propuesto; OD-1): conserva #7 «con la autoridad de la Proposal V14 §9.3».

## 3. Delta propuesto (para una A-2; nada se aplica)

| # | Ancla | Delta |
|---|---|---|
| D8-1 | 16.8 y §9.3 | Tres contadores de **correcciones lanzadas**: (a) **global** de la unidad = `attempts`, que sigue siendo el presupuesto único de ADR-0046 #7 y nunca se reinicia; (b) **por gate**, con clave en el **linaje del gate** (el contrato que lo abre y sus reemisiones), de modo que renombrar el gate no abre presupuesto; (c) **por (`TaskId`, `FailureClass`)**, como hoy |
| D8-2 | topes | un techo **global** finito y topes **locales** finitos por gate y por clase. Una corrección se lanza solo si los tres contadores están por debajo de sus topes; si no, STOP. Los valores se fijan en el Freeze o en el contrato de la unidad; **este dossier no elige ninguno** |
| D8-3 | linaje del defecto | cada corrección lleva `DefectLineage`: el linaje del hallazgo o la comprobación que corrige. Una corrección de un linaje ya visto bajo otra `TaskId` sin `ContinuesTaskId` declarado → STOP: el mismo defecto con otro nombre no abre presupuesto |
| D8-4 | reconstrucción | los contadores se derivan **solo** del registro durable append-only (`correction_launches` con `gate_lineage` y `defect_lineage`). Un reinicio del proceso, un cambio de modelo, sesión o binding no cambia nada |
| D8-5 | desconocido | un contador sin historia reconstruible (unidad migrada sin linaje de gate, entrada ilegible) es UNKNOWN y **falla cerrado**: STOP hasta que el Coordinator lo reconstruya con evidencia. Nunca cuenta como cero (la misma regla que B.8.5 para las ventanas) |
| D8-6 | separación | estos contadores son de **correcciones del Worker**. Los presupuestos del bucle de revisión (A-1: `architect_budgets[]`; REVIEWER: `budgets`) son otra familia: una ronda de revisión no consume corrección ni al revés |

## 4. Contraejemplos y propiedades (prototipo; conjuntos de reglas exactos)

| Escenario | V14 literal | Propuesto (A / B) | Qué muestra |
|---|---|---|---|
| `p8-01-replica-i63-presupuesto-por-unidad` | la 4.ª corrección, STOP S-11 | A: P8-G; B: admitida | con un techo global igual al de hoy (A) nada cambia; la independencia entre gates depende del valor global, que no se elige aquí |
| `p8-02-gates-independientes` | G2 agota la unidad y G3 queda en STOP | B: G2 se para en P8-LG y G3 sigue | el tope local contiene un gate problemático sin consumir el margen local de otro |
| `p8-03a-mismo-defecto-con-otro-TaskId-sin-continuidad` | **admite** (`T3b` empieza en cero por clase) | P8-R1 | renombrar la tarea no abre presupuesto |
| `p8-03b-…-con-continuidad-declarada` | hereda; STOP en la 4.ª | hereda; topes por gate o clase | la continuidad declarada se respeta |
| `p8-04-cambio-de-modelo-y-sesion` | sin reinicio | sin reinicio | igual que hoy |
| `p8-05-reinicio-del-proceso` | sin cambio | sin cambio (reconstrucción del registro) | el reinicio no abre presupuesto |
| `p8-06-contador-desconocido` | **admite** (no existe el concepto) | P8-U hasta reconstruir | falla cerrado |
| `p8-07-techo-global-agotado` | STOP S-11 desde la 4.ª | A: P8-G desde la 4.ª; B: P8-G en la 7.ª | el techo global manda sobre los locales |
| `p8-08-renombre-de-gate` | sin efecto | B: P8-LG tras el renombre | el linaje del gate sobrevive al renombre |
| `p8-09-rondas-de-revision-no-cuentan` | sin efecto | sin efecto | las familias están separadas |

## 5. Alternativas

| Alternativa | Efecto | Coste o riesgo |
|---|---|---|
| A. statu quo | un presupuesto por unidad | reproduce I-63: un gate agota a los siguientes; el mismo defecto con otra `TaskId` solo se frena por la disciplina del Coordinator |
| B. presupuesto por gate, sin techo global | gates realmente independientes | **contradice ADR-0046 #7** («presupuesto único»): M-08 SÍ y, vía ADR-0048, materia de OD-1 (Owner). El total de la unidad deja de estar acotado |
| C. subir `max_attempts` | más margen sin cambiar el modelo | es solo un número: no separa gates ni detecta renombres. Elegirlo no corresponde a esta sesión |
| D. **propuesta (D8-1..D8-6)** | techo global único más topes locales y linaje del defecto | campos nuevos en el registro, una regla de linaje y valores que alguien debe fijar |

## 6. Afectación de esquemas y textos (si se aprobara)

- **Esquemas:**
  - B.8.8 `counters.correction_launches[]` gana `gate_lineage` y `defect_lineage` (M-02);
  - el contrato de gate o el Freeze fija los topes por gate y por clase;
  - `BudgetSnapshot` (B.9) copia los tres contadores.
- **Textos:**
  - 16.8 y §9.3 para las unidades I62;
  - ADR-0048, en su referencia a #7: solo si el Architect concluye que «presupuesto único» admite topes locales debajo del global (propuesta) y no
    hace falta reescribirlo.
- **Unidades I61:** sin efecto.

## 7. Materialidad (evaluación de la sesión)

| M | Valor | Motivo |
|---|---|---|
| M-01 | NO | el dueño del presupuesto sigue siendo el estado canónico |
| M-02 | SÍ | campos nuevos en `correction_launches` y en `BudgetSnapshot` |
| M-03 | SÍ | casos que hoy se admiten pasan a STOP (`p8-03a`, `p8-06`), y otros se paran antes por un tope local |
| M-04 | SÍ | UNKNOWN falla cerrado; la regla del linaje del defecto |
| M-05 | SÍ | cambia la semántica consumida de 16.8 para las unidades I62 |
| M-06 | NO | — |
| M-07 | NO | — |
| M-08 | **a decidir** | con D (techo global único) la sesión lo lee como NO, porque los topes locales solo estrechan ADR-0046 #7. Con B sería SÍ. Lo confirma el Architect |

Hay decisiones reservadas posibles:
- **OD-1**, si la lectura de #7 obligara a cambiar ADR-0048, que acepta el Owner;
- los **valores** de los topes, si se tratan como política del Owner y no como constantes del Freeze (decisión 3).

## 8. Obligaciones y pruebas para F4 (si se aprobara)

- **Trazas del prototipo:** las diez, con las dos variantes separadas (Freeze literal y propuesta) y sin atribuir al Freeze los resultados de la
  ampliada.
- **Reconstrucción** desde el registro durable tras un reinicio del proceso; prueba con un clon sucesor sin memoria (orden §10).
- **Linaje del defecto:** fuente mecánica (el `FindingId` o la comprobación de 16.9 que falla). Una corrección sin linaje parseable es UNKNOWN.

## 9. Matriz de autoridad

| Decisión | Quién |
|---|---|
| aceptar P8 como A-2 (o junto con P4) | Architect + Coordinator (MATERIAL) |
| lectura de ADR-0046 #7 frente a los topes locales (M-08) | Architect; si exige cambiar ADR-0048, Owner (OD-1) |
| valores del techo global y de los topes locales | Coordinator en el Freeze o el contrato; Owner si se considera política (decisión 3) |
| reconstrucción de un contador UNKNOWN | Coordinator, con evidencia custodiada |

## 10. Decisiones necesarias (mínimas)

1. ¿El techo global sigue siendo `attempts` con su valor de contrato (lectura conservadora) o se define aparte? (Coordinator y Architect).
2. ¿Los topes locales por gate son uniformes o por gate del plan? (Coordinator).
3. ¿Quién fija los valores: el Freeze (A-n) o una política del Owner? (Coordinator; el Owner si es política).
4. ¿El linaje del defecto sale del `FindingId` (A-1, `findings[]`) o de la comprobación de 16.9? (Architect).

## 11. Límites

No se elige ni se aprueba ningún tope numérico. Nada de esto modifica V14, el Freeze, `/v1`, I-61 o I-63. El prototipo es EXPERIMENTAL — NOT
AUTHORIZED FOR PRODUCTION y no es evidencia de gate.
