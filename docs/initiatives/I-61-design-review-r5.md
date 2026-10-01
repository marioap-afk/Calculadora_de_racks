# I-61 — Revisión de diseño, ronda 5: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v5.md (blob 861e7cbb) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob cda4491b), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_ebced569-226 de la sesión de I-61
Veredictos: las cuatro lentes CHANGES REQUIRED
Ronda 4: todo RESOLVED salvo C-R4-02 y B-R4-05 (PARTIAL; sus residuos son C-R5-01 y B-R5-02)
Nuevos: 6 REQUIRED (D-R5-01, C-R5-01 y B-R5-01 describen el mismo defecto; A-R5-01; B-R5-02; B-R5-03) y 8 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V6 y ADR-0046 (solo la referencia a la versión)
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v6.md](I-61-proposal-v6.md).

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R5-01 / C-R5-01 / B-R5-01 | Caducidad del RED circular: `ExpectRed` de la delegación y `Files[]` sin anclar | Aceptado con cambio de diseño: se retiran `Files[]` y la caducidad en la aceptación. `ExpectRed` es siempre el del contrato (A5). `ChainRedFiles` = archivos de prueba del commit RED acreditado, salidos de Git y custodiados; la **verificación** exige RED nuevo si el diff toca `ChainRedFiles`. Así el escenario «toca las pruebas sin RED nuevo → REWORK» es alcanzable y no choca con `Scope` (resuelve también C-R5-02 y B-R5-04) | §3.4 A5/A8; §8.2; §8.3 filas 9-10; §9; §12.1 paso 6.1 |
| A-R5-01 | `AuthorityRevision` rígida frente a A-n y decisiones posteriores a G2 | Aceptado: cierre de G2 o commit posterior con diff limitado a `UNIT_DOC`, o su imagen | §9 |
| B-R5-02 | Barrido de huérfanos sin filtro ni consecuencia | Aceptado: definición operativa de huérfano | §3.3 |
| B-R5-03 | Rebase al abrir sesión sin `RebaseMap` | Aceptado: todo rebase posterior a G2 se registra y el contrato se reemite | §3.6 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R5-02 celda, no modelo, sin control de effort | Aceptado | §4.3; §12.1 paso 0 |
| D-R5-03 effort heredado en catálogo y criterio 2 | Aceptado | §5; §18 |
| C-R5-02 rama de REWORK inalcanzable | Resuelto por el cambio de diseño de D-R5-01 | §9 escenarios |
| C-R5-03 `RedPart` con corrida ausente | Aceptado (BLOCKED) | §8.3 fila 9 |
| A-R5-02 tramo `AuthorityRevision..BaseSha` | Aceptado | §8.3 fila 3 |
| A-R5-03 destino del orden de commits y de la recuperación | Aceptado | §2.3 |
| B-R5-04 igual que C-R5-02 | Resuelto por el cambio de diseño | §9 |
| B-R5-05 inciso de conflictos; `Files[]` | Aceptado; `Files[]` retirado | §3.6 |
