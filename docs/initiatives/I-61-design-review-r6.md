# I-61 — Revisión de diseño, ronda 6: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v6.md (blob 422fa47c) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob d6602de6), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_cc7ae11f-349 de la sesión de I-61
Veredictos: las cuatro lentes CHANGES REQUIRED
Ronda 5: RESOLVED salvo D-R5-01, C-R5-01, B-R5-01 y A-R5-01 (PARTIAL; sus residuos son D-R6-01, C-R6-01, B-R6-01 y A-R6-01/D-R6-02)
Nuevos: 6 REQUIRED (D-R6-01 = C-R6-01 = B-R6-01; D-R6-02 = A-R6-01; C-R6-02) y 8 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V7 y ADR-0046 (solo la referencia a la versión)
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v7.md](I-61-proposal-v7.md).

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R6-01 / C-R6-01 / B-R6-01 | `ChainRedFiles` del último commit RED puede quedar vacío cuando el RED de una corrección solo desactiva el fix | Aceptado: `ChainRedFiles` = anterior ∪ `git diff --name-only <ChainBaseSha> <RedSha> -- tests/`; `ChainBaseSha` = base de la primera delegación de la tarea (o su imagen); el conjunto nunca decrece; se retira «un único commit» | §9; §8.2; §3.4 A8; §3.6 `RebaseMap`; §12.1 pasos 6 y 8 |
| C-R6-02 | Las pruebas pueden cambiar en el GREEN sin verse fallar | Aceptado: `RedPart` de `Tests` exige que `git diff --name-only RedSha CurrentSha` no toque `RT` ∪ `ChainRedFiles`; los cambios de pruebas van en el commit RED | §8.3 fila 10; §9; §12.1 paso 6 |
| D-R6-02 / A-R6-01 | El predicado de X («diff limitado a `UNIT_DOC`») es inejecutable tras escribir en `src/` o tras un rebase | Aceptado: X válido si `git diff --name-only <B> X` no toca rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE`, con `B` = cierre de G2 o su imagen en el último `RebaseMap` | §9 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R6-03 / C-R6-03 `ChainRedFiles` en el paso 8 y en el escenario | Aceptado | §12.1 paso 8; §9 escenarios |
| A-R6-02 / B-R6-03 reemisión tras todo rebase; registro de apertura de sesión | Aceptado (ruta `session-rebase/<RunId>/`, `TaskId` = `SESSION`) | §3.4; §3.5 |
| A-R6-03 commits de la sesión y rutas `UNIT_CHANGE`; igualdad por blob en el `RebaseMap` | Aceptado (la sesión no las toca sin contrato nuevo; igualdad por secciones `UNIT_CHANGE`) | §3.1; §3.6 |
| B-R6-02 huérfano no observable | Aceptado (padre inexistente en el `Entry` o PID reutilizado) | §3.3 |
| B-R6-04 corrida del `RedSha` ausente | Aceptado (con la de `CurrentSha` terminada → `RedPart` = `fail`, REWORK; BLOCKED solo si está en curso y la entrega exige RED) | §8.3 fila 9 |
