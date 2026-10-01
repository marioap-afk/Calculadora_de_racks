# I-61 — Revisión de diseño, ronda 4: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v4.md (blob c0359f8f) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob e9a70b32), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_69f6c12a-18f de la sesión de I-61
Veredictos: A = AGREED; B = AGREED; D = CHANGES REQUIRED; C = CHANGES REQUIRED
Ronda 3: todo RESOLVED salvo C-R3-01 (PARTIAL; su residuo es C-R4-01)
Nuevos: 3 REQUIRED (D-R4-01, C-R4-01, C-R4-02) y 14 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V5 y ADR-0046 revisado
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v5.md](I-61-proposal-v5.md) salvo que se indique ADR.

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R4-01 | Celda sin control de effort («no aplica») frente al effort heredado medido de los subagentes | Aceptado: se registra el effort **heredado medido**, la delegación lo pide y `Routing` lo compara; no es escalado, es una limitación anotada en `RoutingReason`. Igual en el oráculo de U-04 | §4.3; §12.1 paso 0 |
| C-R4-01 | Partes RED decididas por el `RedSha` que declara el Worker | Aceptado: `RedPart` en `Ci` y `Tests`, decidido por el `ChainRedSha` de la **delegación**; una entrega sin `RedSha` cuando se exige RED da REWORK | §8.2; §8.3 filas 9-10; §9 escenarios |
| C-R4-02 | El RED acreditado no caduca aunque se modifiquen las pruebas RED | Aceptado: caduca para la delegación cuyo `AllowedWriteScope` cubre los archivos `Files[]` de pruebas con `ExpectRed` (A8 fija entonces `ChainRedSha` = `null`); si una delegación sin RED exigido los toca, `Tests` da REWORK | §8.2 `RequiredTests[].Files[]`; §3.4 A8; §8.3 `Tests`; §9 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R4-02 conteo de PR-1 | Aceptado (texto fijo `PR1: ls-files=<n>` en `RoutingReason`) | §16.2 |
| D-R4-03 un solo effort sondeado | Aceptado (escalón superior; ejecución por valor de effort) | §12.1 paso 0 |
| A-R4-01 granularidad de `UNIT_CHANGE` | Aceptado por la vía (b): `AuthorityRevision` = SHA de cierre de G2 revisado contra el Freeze, o su imagen | §9 |
| A-R4-02 referencias de la tabla copiada a §16 | Aceptado | §2.1 (1); §2.2; ADR decisión 1 |
| A-R4-03 definición de sección; «contrato» | Aceptado | §9 |
| C-R4-03 «partes RED» frente a un único `Result` | Aceptado (campo `RedPart`) | §8.2; §8.3; §9 |
| C-R4-04 longitud de ruta de los DLL legacy | Aceptado (raíz corta `%TEMP%\i61l-<sha8>`) | §16.3 |
| B-R4-01 rebase con conflictos | Aceptado (`--abort`, registro y STOP; reemisión tras decisión del Coordinator, con A-n si reescribe commits publicados) | §3.6 |
| B-R4-02 reemisión en el caso sin conflictos; nombres de clases | Aceptado | §3.4; §3.6 |
| B-R4-03 `Identity` tras rebase contra los SHA originales | Aceptado | §8.3 fila 5 |
| B-R4-04 oráculo de nc4 frente a A6 | Aceptado (oráculo relativo; repetición si la real no pasa A6) | §12.1 paso 4 |
| B-R4-05 servidor de Roslyn como `dotnet.exe`; huérfanos | Aceptado | §3.3 |
| B-R4-06 tope de los controles | Aceptado (2 por control; agotado → P-04 y control no superado) | §12.1 paso 9 |
| B-R4-07 `--force-with-lease` frente a AUTOMATION_PLAN §3 | Aceptado | §2.2 |
