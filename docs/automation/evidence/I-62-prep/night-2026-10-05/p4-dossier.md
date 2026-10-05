# I-62 — Dossier P4: `VerificationTargetSha` y `CustodyHeadSha` (propuesta para una A-2 futura)

```text
Estado:      PROPUESTA DE PREPARACIÓN — sin aplicar, sin vigencia y sin consumir A-2 (orden nocturna §3.B y §7; decisiones §41)
Triaje:      P4 = propuesta de cambio material (Coordinator); se guarda aparte de A-1 y no entra en su revisión
Arnés:       p4/p4-custody-harness.py — EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION; resultado p4/p4-custody-result.json (determinista)
Variantes:   (1) Freeze literal = validador «V14 literal»; (3) experimento P4 = validador «P4 propuesto»; la variante Freeze + A-1 no cambia nada de esto
Supuesto:    la numeración P1..P9 de la orden no figura en el repositorio; este dossier liga P4 a I-63 evidencia §51.1 (excepción de Identity) y
             §61.4 (cierre sobre el Candidato), los únicos casos canónicos con un SHA funcional verificado distinto del HEAD que lo custodia
```

## 1. El caso de I-63, reproducido desde sus artefactos

| Hecho | Fuente | Medición de esta sesión |
|---|---|---|
| GREEN de G4 = `CurrentSha` de la entrega | `worker-handoff.json` de `R20261003T005720Z-2b14`, custodiado en `7bd5a743` | `CurrentSha` = `4f45f4464f5d434f7fa478fea1117d82283ee997` (leído del JSON, no del prompt) |
| CI funcional del GREEN | evidencia de I-63 §50.2 | `gh run view 37085650331`: push, `headSha` = `4f45f446…`, cuatro jobs en `success` |
| Commit de custodia encima del GREEN tras el STOP S-03/C-10 | evidencia §51.1 | `7bd5a743`: 1 commit, 20 rutas, todas bajo `docs/automation/evidence/I-63*` y `docs/automation/state/I-63.yml` |
| Reverificación con `VerifiedSha` = GREEN y HEAD = custodia | evidencia §51.3 (`R20261005T011832Z-cc42`) | el Controller aplicó una «excepción de Identity» que 16.9 #5 no define |
| Árboles funcionales iguales | evidencia §51.1 | `src` `5d3bba74…` y `tests` `28bc25cb…` iguales en `4f45f446` y `7bd5a743` |
| Cierre documental sobre el Candidato | tag `integration/I-63`; evidencia §61.4 | `55a66b3c..3c5019bf`: 9 rutas, entre ellas `docs/HANDOFF.md`, `docs/ROADMAP.md` y `docs/ideas-futuras.md` |

**Resultado del arnés con los objetos reales** (solo lectura del repositorio):
- **custodia de G4:** V14 literal ALLOWED y P4 propuesto ALLOWED. `EvidenceSha` = `4f45f446…`, y la CI de la custodia no se usa como evidencia de X.
- **cierre de integración:** V14 literal BLOCKED (V14-PREFIX) y P4 BLOCKED (P4-P1). Es lo correcto: el cierre no es custodia de un gate. Lo rige
  WORKFLOW §11.5 («La CI del cierre nunca sustituye la CI del Candidato»), no esta excepción.

## 2. Cláusulas anteriores (literales del texto vigente o congelado)

- **AUTOMATION_PLAN 16.1** (I-61, consumida por V14 sin cambio):
  - «Trabajo delegado terminado: solo con `EXECUTION_VERIFIED` cuyo `VerifiedSha` es el `CurrentSha` de la entrega o, tras un rebase registrado (16.7),
    su imagen;
  - y con el SHA evaluado del gate igual a ese SHA más commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad, sin rutas
    `EXTERNAL` ni rutas con secciones `UNIT_CHANGE` (16.3).
  - El Coordinator lo comprueba con `git diff --name-only`»;
- **AUTOMATION_PLAN 16.9 #5 `Identity`:** «`HEAD` = `refs/remotes/origin/<rama>` = `CurrentSha`; tras un rebase, sobre los originales del `RebaseMap` y sus
  imágenes, con `patch-id` igual»;
- **Proposal V14 §8.5:** fila «SHA evaluado del gate: `CurrentSha` + commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad
  (16.1, sin cambio)», y «`Identity` (`HEAD` = remoto = `CurrentSha`) **no cambia**»;
- **Proposal V14 §8.2 W-2:** entre el Q0 y el cierre no hay escrituras Git de la sesión;
- **Proposal V14 §8.4:** «Ningún SHA se escribe dentro de su propio commit».

**La brecha.**
- **16.9 #5** exige HEAD = `CurrentSha` sin excepción.
- **16.1** admite commits de la sesión por **prefijo de ruta del diff final**.
- **Reverificación de la misma entrega tras una custodia** (el STOP resuelto sin cambio de trabajo de I-63, o la reverificación de un Q0 nuevo en una
  unidad I62): HEAD ya no es `CurrentSha`. Hoy, o la verificación hace STOP por `Identity`, o el Controller improvisa la excepción, como en I-63.
- **El prefijo y el diff final** no prueban que la custodia sea solo custodia (§4).

## 3. Delta propuesto (para una A-2; nada se aplica)

| # | Ancla | Delta |
|---|---|---|
| D4-1 | §8.5 y B.8.8 | Dos identidades separadas. **`VerificationTargetSha`** (X) = el `CurrentSha` **parseado de la entrega estructurada** y comprobado con Git, o su imagen por un `RebaseMap` custodiado; igual al `VerifiedSha` del resultado. **`CustodyHeadSha`** (Y) = el HEAD (= remoto) en que se verifica o se custodia. X es la identidad de la evidencia (CI, pruebas, RED); Y solo es la cabeza de custodia |
| D4-2 | 16.9 #5 `Identity` (para unidades I62) | `Identity` pasa con HEAD = remoto = Y ≠ X solo si se cumple **CUSTODY_DESCENT(X, Y, mapa)**; si no, STOP como hoy. Con Y = X no cambia nada |
| D4-3 | nuevo: CUSTODY_DESCENT | (a) X existe y es ancestro de Y (`merge-base --is-ancestor`); (b) `X..Y` es lineal: ningún merge; (c) **cada** commit de `X..Y` cambia solo rutas del **mapa de custodia** de la unidad, comparadas **sin detección de renombres** (un renombre es borrado + alta), y ninguna ruta prohibida; (d) ninguna entrada no regular (enlace simbólico, gitlink); (e) cada raíz protegida (`src/`, `tests/` y las que fije el contrato) conserva el árbol de X en cada commit del rango; (f) la CI exacta de X existe y es la evidencia. La CI de Y se registra aparte como salud de custodia y nunca la sustituye |
| D4-4 | nuevo: mapa de custodia | lista **explícita** de rutas permitidas (evidencia de la unidad, su estado, sus decisiones, sus directorios de custodia) y de rutas **prohibidas** (autoridades normativas aunque estén bajo `docs/automation/`, por ejemplo `docs/automation/agent-execution/**`; AGENTS, CLAUDE, WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, FOUNDATIONS, ADR, Freeze/Proposal; configuración: `.github/**`, `global.json`, `*.props`, `*.targets`, `*.csproj`, `*.sln`, `.gitattributes`, `.gitignore`). Sin mapa custodiado, no hay excepción |
| D4-5 | B.8.8 (estado) | registro durable `custody.verification_custody[]` · `{verification_target, custody_head, custody_map: StateRef, check: StateRef}` · append-only. Se fija en el punto durable **siguiente** a Y, por §8.4 |
| D4-6 | 16.1 (para unidades I62) | la frase «limitados a `docs/automation/` y a los documentos de la unidad … con `git diff --name-only`» se sustituye por CUSTODY_DESCENT con el mapa de D4-4. La identidad de la evidencia sigue siendo X |
| D4-7 | límites | la excepción **no** traslada CI, READY-05/06 ni evidencia de gate a otro SHA. No se apoya en `EquivalentReviewedObject` de A-1 (D2-12), que compara objetos revisados y no evidencia. Un cambio funcional o normativo, un mapa ausente o una descendencia no probada la impiden: hay que reverificar sobre un SHA nuevo o emitir un contrato nuevo |

## 4. Contraejemplos de la cláusula anterior (arnés; `Deltas` del resultado)

Cada fila es un escenario en el que el validador literal **admite** y el propuesto **bloquea**. Conjuntos de reglas exactos.

| Escenario | Por qué el literal admite | P4 |
|---|---|---|
| `p4-02-cambia-y-revierte-produccion` | el diff final no toca `src/` | P4-P1, P4-P2 |
| `p4-03-autoridad-normativa-bajo-docs-automation` | `docs/automation/agent-execution/README.md` cumple el prefijo | P4-P1, P4-N1 |
| `p4-05-Y-no-desciende-de-X` | `git diff X..Y` funciona sin ascendencia y el árbol funcional coincide | P4-D1 |
| `p4-06-merge-dentro-del-rango` | el merge solo trae custodia | P4-D2, P4-P2 |
| `p4-08-objetivo-solo-en-el-prompt` | 16.1 no fija de dónde sale X (nc1 de I-63) | P4-T1 |
| `p4-09-objetivo-inexistente` | ídem: `CurrentSha` mutado `0123…` | P4-T2 |
| `p4-13-renombre-de-produccion-hacia-la-custodia` | `git diff --name-only` con renombres muestra solo el destino | P4-P1, P4-P2 |
| `p4-15-enlace-simbolico-en-la-custodia` | la ruta cumple el prefijo | P4-P3 |
| `p4-16-sin-mapa-de-custodia` | no hay mapa que exigir | P4-M0 |
| `p4-17-documento-de-la-unidad-fuera-del-mapa` | 16.1 admite cualquier documento de la unidad | P4-P1 |

**Coinciden los dos validadores en los demás casos.**
- **Admiten:** la réplica de I-63, el rango vacío y el rebase con mapa y CI en la imagen.
- **Bloquean:** un `.md` bajo `src/`, `docs/WORKFLOW.md`, la configuración, la CI solo en Y, el rebase con CI solo en X (16.7 ya exige la corrida de
  `CurrentSha'`), el rebase sin mapa y el borrado de una prueba.

En total: 20 escenarios sintéticos y 2 reales, todos con el resultado esperado. Se ejercitan las 12 reglas.

## 5. Alternativas

| Alternativa | Qué hace | Por qué no basta o qué cuesta |
|---|---|---|
| A. statu quo (prefijo + diff final) | 16.1 tal cual | falla en los diez contraejemplos de §4; `Identity` sigue sin excepción definida |
| B. sin excepción: la custodia espera a la decisión | lección de I-63 §51.1: tras un STOP reverificable, no se commitea custodia hasta la decisión | simple y sin campos nuevos; pero contradice la custodia en Q7 de V14 (§8.4) para la reverificación, y una caída entre la verificación y la custodia pierde la evidencia transitoria |
| C. igualdad de árboles protegidos en X y en Y | compara `src/` y `tests/` en los dos extremos | no detecta el cambio y reversión intermedio, ni una autoridad normativa bajo `docs/`, ni la falta de ascendencia |
| D. **propuesta (D4-1..D4-7)** | descendencia, linealidad, comprobación commit a commit, mapa explícito, identidad de la evidencia en X | exige un mapa por unidad, un registro durable y una regla nueva en `Identity` |
| E. reverificar siempre sobre Y | trata Y como un SHA nuevo | Y no tiene entrega ni CI propias de trabajo; obligaría a una CI funcional de un commit de custodia: el mismo traslado de identidad que la orden prohíbe |

## 6. Afectación de esquemas y textos (si se aprobara)

- **Esquemas:**
  - `controller-verification` y `relay-record/v2`: `VerifiedSha` ya es X; hace falta el campo `CustodyHeadSha` o una referencia al registro de D4-5
    → cambio de forma (M-02);
  - `state/v2` (B.8.8): `custody.verification_custody[]`.
- **Textos F3** materializados e inactivos, para unidades I62:
  - AUTOMATION_PLAN 16.1 («Trabajo delegado terminado»), 16.9 #5 y 16.22;
  - README de agent-execution, en la comprobación `Identity`;
  - igual que D2-11 de A-1: enmienda de semántica propuesta, sin vigencia hasta su acuerdo.
- **Sin efecto en I-61:** las unidades I61 conservan 16.1 y 16.9 tal cual. No se reabre I-61 ni I-63.

## 7. Materialidad (LIFECYCLE §3), evaluación de la sesión

| M | Valor | Motivo |
|---|---|---|
| M-01 | NO | la evidencia sigue siendo de X y del Controller; no aparece otra autoridad |
| M-02 | SÍ | campos nuevos (D4-5; `CustodyHeadSha`) |
| M-03 | SÍ | casos que hoy pasan por prefijo pasarían a STOP (§4) |
| M-04 | SÍ | cambia qué falla: `Identity` con custodia deja de ser improvisable |
| M-05 | SÍ | cambia la semántica consumida de 16.1 y 16.9 #5 para las unidades I62 |
| M-06 | NO | ningún punto de extensión |
| M-07 | NO | ningún mecanismo transversal nuevo: es una regla de verificación |
| M-08 | NO, a confirmar por el Architect | ADR-0046 #2 y #6 (`VerifiedSha` del SHA de la entrega; CI de push del SHA exacto) se conservan y se refuerzan. El cambio estrecha una regla del plan, no un ADR |

Clase propuesta: **MATERIAL**, con autoridad Architect + Coordinator (LIFECYCLE §6). No se identifica ninguna consecuencia OWNER-RESERVED, aunque el
mapa de custodia por unidad podría tocar la política de evidencia de AGENTS (decisión necesaria 3).

## 8. Obligaciones y pruebas para F4 (si se aprobara)

- **Trazas:** las 20 sintéticas del arnés, como pruebas de la verificación mecánica, con conjuntos de reglas exactos y ambas variantes separadas (Freeze
  literal y Freeze + A-n).
- **Caso real:** la custodia `7bd5a743` sobre `4f45f446` como prueba de aceptación, y el cierre `3c5019bf` como negativo informativo.
- **Controles negativos de la verificación** (en la línea de nc1/nc2): un `CurrentSha` mutado y una ruta normativa en la custodia deben dar STOP. No
  basta con la palabra del Controller (DEBT-I63-PROTOCOL-01).

## 9. Matriz de autoridad

| Decisión | Quién |
|---|---|
| aceptar P4 como A-2 (o como parte de otra A-n) | Architect + Coordinator (MATERIAL) |
| contenido del mapa de custodia de cada unidad | Coordinator, en el contrato de gate |
| aplicar la excepción en un caso concreto | mecánico (CUSTODY_DESCENT); el Controller la registra y el Coordinator puede rechazarla (16.1) |
| política de evidencia, si el mapa toca AGENTS | Owner, si resultara OWNER-RESERVED |

## 10. Decisiones necesarias (mínimas)

1. ¿P4 entra en una A-2 propia o se agrupa con P8? (Coordinator).
2. ¿Forma del registro: un campo en la verificación o el registro durable D4-5? (Architect).
3. ¿El mapa de custodia vive en el contrato de gate (propuesta) o en una política global? (Coordinator; el Owner si toca AGENTS).
4. ¿Se acepta la alternativa B (esperar a la decisión) como regla provisional para I-62 mientras no haya A-2? (Coordinator).

## 11. Límites

Nada de esto modifica V14, el Freeze, `/v1`, I-61 o I-63. No es una regla vigente: el arnés es EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION y sus veredictos
no son evidencia de gate.
