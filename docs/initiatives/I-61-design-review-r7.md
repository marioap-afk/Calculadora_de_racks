# I-61 — Revisión de diseño, ronda 7: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v7.md (blob 2bbb07fa) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob c7a6f22c), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_e913851b-59e de la sesión de I-61
Veredictos: las cuatro lentes CHANGES REQUIRED
Ronda 6: RESOLVED salvo D-R6-02, C-R6-02, A-R6-03, B-R6-02 y B-R6-04 (PARTIAL; residuos en los hallazgos de abajo)
Nuevos: 7 REQUIRED (D-R7-01; D-R7-02; C-R7-01 = B-R7-02; A-R7-01 = B-R7-03; B-R7-01) y 7 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V8 y ADR-0046 (solo la referencia a la versión)
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v8.md](I-61-proposal-v8.md).

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R7-01 | Tras un `RedPart` en `fail`, la entrega siguiente seguía con el RED acreditado anterior y eludía la regla | Aceptado: **RED vigente** = `null` tras una entrega con `RedPart` = `fail`; `ChainRedFiles` se conserva y solo está vacío si la cadena nunca acreditó RED | §9; §3.4 A8; §8.2 |
| D-R7-02 | El `RebaseMap` no registraba la imagen del cierre de G2 | Aceptado: la registra en todo rebase posterior a G2; sin contrato, `AuthorityRevision` = cierre de G2; `B` admite la `AuthorityRevision` vigente | §3.6; §9 |
| C-R7-01 / B-R7-02 | `RT` y `ChainRedFiles` vacíos en una corrección tras RED no acreditado | Aceptado: `RT` = `git diff --name-only <ChainBaseSha> <RedSha> -- tests/` | §9; escenarios OBL-08 |
| A-R7-01 / B-R7-03 | Esquema del registro de relevo desalineado con §3.6 | Aceptado | §8.2 |
| B-R7-01 | Paso 7 contradecía la fila 9 | Aceptado | §12.1 paso 7 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R7-03 redacción del STOP del `RebaseMap` | Aceptado (solo igualdades; excepción del rebase que ejecuta una decisión con A-n) | §3.6 |
| C-R7-02 igual que A-R7-01 | Aceptado | §8.2 |
| A-R7-02 `B` del predicado de X | Aceptado | §9 |
| B-R7-04 `CreationDate` en la orden | Aceptado | §3.3 |
| B-R7-05 rebase de apertura con cadena en curso | Aceptado | §3.5 |
| B-R7-06 SHA originales en la parte RED de la reverificación | Aceptado | §3.6 |
