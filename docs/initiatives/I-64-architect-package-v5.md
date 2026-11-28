# I-64 — Paquete de re-revisión del Architect (Proposal V5)

```text
PROPOSAL V5 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (sobre la misma V5, después del Architect)
Architect      = RE-REVIEW REQUIRED por el MISMO Architect que emitió A64-PV3-04 (STILL OPEN) y A64-PV4-01
Consensus      = NOT REACHED
Freeze         = NONE (aunque el Architect emita AGREED, este paso no crea Freeze; el Coordinator lo ordena después)
Implementation = NOT AUTHORIZED

Objeto de la re-revisión = docs/initiatives/I-64-proposal-v5.md, en el commit que publica este paquete (rama
                           architecture/workspace-persistente-rackcad). SHA, blob y CI de ese commit no se escriben aquí para no
                           autorreferenciarse: se toman de `git log -1 -- docs/initiatives/I-64-proposal-v5.md`, de `git rev-parse` y de la
                           corrida de CI de ese SHA, que el informe de entrega da exactos.
Versión revisada antes   = V4: commit e87ba32cf4132cd29677c49cf208d4ca6fb9ea49, proposal blob ef3aa9994bf0028aa1966761ccdfb4dcf42b3c56,
                           package blob 580beba108911a97bcc6a33db5bd2d025d5a2388, CI 36937865502 (push, head_sha exacto, 4/4 success)
Historia intacta         = V1 (dcc16bed), V2 (ab4efe86), V3 (1f1530be), V4 (e87ba32c): sin cambios en este commit
Dictamen previo          = sobre V4: A64-PV3-01, 02, 03, 05, 06 = CLOSED; A64-PV3-04 = STILL OPEN; A64-PV4-01 = REQUIRED (evidencia §17)
Orden del Coordinator    = F0 / Proposal V5, con reglas vinculantes para A64-PV3-04 y A64-PV4-01 y opcionales a adoptar (evidencia §17)
Base de main             = 819955d61a6da4c811a11fbd11b5dca13f634b7c (sin cambio; sin rebase)
```

**Identidad del revisor** (la misma de los dictámenes sobre V1, V3 y V4, según la declaró el propio Architect):

| Campo | Valor |
|---|---|
| Rol | Architect |
| Modo | SEPARATE SESSION; revisor = autor: NO |
| Sesión | «I-64 Architect Review Proposal V1» (`local_172c5d51-ea91-4f95-8c17-23a1563e61b7`) |
| Perfil | ARCHITECTURE_REVIEW / Deep |
| Límite de independencia declarado | mismo modelo (Claude) que la sesión autora |
| Autoridad sobre sus REQUIRED | solo este revisor puede cerrar o rebajar A64-PV3-04 y A64-PV4-01 |

## 1. Veredicto que se solicita

```text
A64-PV3-04 = CLOSED | STILL OPEN (razón)
A64-PV4-01 = CLOSED | STILL OPEN (razón)
NEW FINDINGS: A64-PV5-nn REQUIRED | OPTIONAL, con sección, evidencia y cambio exigido (o «ninguno»)
CONSENSUS STATUS: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Modo: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona
Versión: el veredicto vale solo para el SHA y el blob exactos revisados
```

Si ambos quedan CLOSED y no hay REQUIRED nuevos: Architect = AGREED. **Aun así:** no Freeze, no implementación, no GATE PASS. El
Coordinator revisa después la misma V5 y ordena el Consensus Freeze. Se pide además confirmar que nada de V5 reabre los CLOSED anteriores.

## 2. Lectura mínima

1. [Proposal V5](I-64-proposal-v5.md), completa: en especial D-04 (excepción de `BeginDocumentClose` y modo degradado), D-05 (privacidad),
   D-07 (revalidación), D-12, D-17, D-18, §10 (INV-20, INV-36, INV-39, INV-40), §11, §12.1, §12.5 y §21.
2. Este paquete: delta (§3), disposición (§4), historia (§5) y opcionales (§6).
3. [Evidencia](../automation/evidence/I-64-evidence.md) §17: orden de V5, procedencia del dictamen sobre V4, CI de V4 y puntas.

## 3. Delta V4 → V5

| Sección | V4 | V5 |
|---|---|---|
| Cabecera y fuentes | revisión V4 pendiente | dictamen sobre V4 registrado; orden V5 como fuente; revisión V5 pendiente |
| D-04 | excepción de `BeginDocumentClose` sin vida definida; el modo degradado lo conservaba solo «mientras» hubiera dirty | **`BeginDocumentClose` suscrito toda la vida de la sesión**, también degradada, sin depender del dirty al degradar; el manejador solo lee memoria, decide el veto y emite un aviso no modal; con dirty o texto pendiente al cerrar: VETO; el modo degradado retira solo la sincronización (A64-PV3-04) |
| D-05 | «no API pública» sin precisar; guarda de privacidad en F6 | «no API pública» = contrato no reutilizable sin consumo fuera del panel; sin `internal` obligatorio ni `InternalsVisibleTo` (O4-01); guarda desde F2 con `NavigationTargets` y de nuevo en F6 (O4-02) |
| D-07 | — | revalidación en contexto de comando de documento, existencia y atribución del objetivo; si falla, no actúa e invalida (O4-06) |
| D-12 | variante de lectura caller-owned | + incluida en las guardas de fuente de la costura caller-owned (O4-05) |
| D-17 | veto vigente en modo degradado | la suscripción vive toda la sesión: protege también borradores ensuciados después de degradar |
| D-18 | P-03 con excepción; Smoke-PERF «tras F6» | P-03 con la regla completa; Smoke-PERF tras F7 PASS |
| §10 | INV-20 con sesión degradada; INV-36 en F6 | **INV-20** con la secuencia sesión limpia → degradado → edición → dirty → cierre → veto; **INV-36** desde F2; nuevos **INV-39** (lectura caller-owned bajo guarda) e **INV-40** (revalidación de navegación) |
| §11 | F7 con Smoke-PERF «tras F6 y antes de READY» | **F7 PASS → Smoke-PERF → si PASS → READY**; F2 cierra también con INV-36 (guarda) e INV-40 |
| §12.1, §12.5 | Smoke-PERF sin criterio de PASS | **PASS** = SP-01..05 completos y válidos + SP-06 PASS + aceptación explícita del Owner; NOT PASS → STOP antes de READY y A-n del Coordinator (umbral o gate correctivo), con repetición de la evidencia afectada (A64-PV4-01) |
| §15 | puntas de V4 | I-52 `51a66245`, I-62 `3d7ec77f`, I-63 `dddc215f` |
| §16 | — | riesgo «rendimiento no aceptado en Smoke-PERF»; la lectura caller-owned bajo guarda |
| §20 | disposición de A64-PV3 | estado tras V4 (01, 02, 03, 05, 06 CLOSED; 04 STILL OPEN) sin reescribir; frase de Smoke-PERF «tras F6» sustituida |
| §21 | — | **nueva**: finding → disposición → cláusula V5 → oráculo para A64-PV3-04 y A64-PV4-01; opcionales O4 |
| Anexo A | decisión 3 con «mientras haya dirty» | decisión 3: el manejador de cierre vive toda la sesión |

## 4. Disposición

| Finding | Cláusula de V5 | Oráculo |
|---|---|---|
| A64-PV3-04 | D-04 (excepción acotada; modo degradado); D-17; P-03 | INV-20 con la secuencia limpio → degradado → editar → dirty → cerrar → veto; S2-06 |
| A64-PV4-01 | §12.5 (criterio de PASS, NOT PASS y STOP); §11 (F7, READY, momento); §12.1; D-18 | criterio de PASS de §12.5 y entrada de READY |

Tabla completa en la Proposal V5 §21.

## 5. Historia

- A64-PV1-01..22 = **CLOSED** (dictamen sobre V3).
- A64-PV3-01, 02, 03, 05 y 06 = **CLOSED** (dictamen sobre V4).
- V5 no reabre ninguno: sus cambios se limitan a D-04, D-05, D-07, D-12, D-17, D-18, §10, §11, §12.1, §12.5, §15, §16, §20 (estado y una
  frase), §21 y la decisión 3 del anexo A.

## 6. Opcionales

| Opcional | Estado en V5 |
|---|---|
| O4-01 | adoptada (D-05) |
| O4-02 | adoptada (D-05, INV-36, §11) |
| O4-03 | sigue opcional |
| O4-04 | sigue opcional |
| O4-05 | adoptada (D-12, INV-39) |
| O4-06 | adoptada (D-07, INV-40) |
| O3-04, O3-05, O3-07 | siguen opcionales |

## 7. Lo que este paquete no hace

No realiza la re-revisión. No declara AGREED, consenso, Freeze, GATE PASS ni independencia. No crea Freeze aunque el Architect emita
AGREED. No autoriza implementación, delegaciones, sondas, pilotos ni ningún uso de AutoCAD. `IMPLEMENTATION AUTHORIZATION = NO`.
