# I-61 — Revisión de diseño, ronda 3: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v3.md (blob 56972b06) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob 8403bf98), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_a0761336-ce0 de la sesión de I-61
Ronda 2: todo RESOLVED salvo B-08, B-R2-03, B-R2-06 y C-R2-08 (PARTIAL; sus residuos son B-R3-01, B-R3-02 y C-R3-02)
Nuevos: 9 REQUIRED y 14 OPTIONAL. Veredicto de las cuatro lentes: CHANGES REQUIRED
Adjudica: Architect (misma sesión). Resultado: Proposal V4 y ADR-0046 revisado
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v4.md](I-61-proposal-v4.md) salvo que se indique ADR o evidencia.

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R3-01 | Autenticación de los subagentes rotulada MEASURED sin medición | Aceptado: **medida y versionada** (OAuth de cuenta de claude.ai, sin `ANTHROPIC_API_KEY`, `ANTHROPIC_AUTH_TOKEN` ni proveedor de nube; fuente oficial sobre Desktop y precedencia de credenciales). El tipo de plan queda UNKNOWN y no lo necesita la regla | Evidencia §12; §4.3 «Sondas» |
| C-R3-01 | RED ligado a la «primera delegación» y no a un RED acreditado | Aceptado: definición de RED acreditado; `ChainRedSha` solo con él; la corrección sin RED acreditado exige RED con la corrección desactivada; disposición REWORK para una corrida del `RedSha` sin fallo | §9; §3.2 punto 4; §3.4 A5; §8.3 `Ci`; §12.1 paso 6.1 |
| C-R3-02 | DLL legacy sin SHA en la `ProductVersion` | Aceptado (comprobado: `Directory.Build.targets` admite `SourceRevisionId` explícito) | §16.3 |
| A-R3-01 | Lectura de autoridades binaria y por archivo | Aceptado: tres clases (`UNIT_DOC`, `UNIT_CHANGE`, `EXTERNAL`) con comprobación por sección y definición de sección y preámbulo | §9; §8.3 `Authority`; §8.2; §2.1 frase; ADR decisión 1 |
| A-R3-02 | Rebase rompe `AuthorityRevision` | Aceptado: imagen en el `RebaseMap` con igualdad de blobs de clases (a) y (b), reemisión del contrato y `MainSha` nuevo | §3.6; §3.4; §8.2 |
| B-R3-01 | Reverificación tras rebase con un commit de custodia intermedio | Aceptado por la vía «ninguna escritura de la sesión entre la verificación S-13 y la reverificación, salvo el rebase»; la corrida RED se cita desde el registro de relevo y se custodia después | §3.2 punto 5 y párrafo final; §3.6 |
| B-R3-02 | `not_run` reintroduce STOP con la entrega ausente | Aceptado | §8.3 introducción |
| B-R3-03 | Comprobación por archivo frente a declaración por sección | Aceptado, junto con A-R3-01 | §8.3 `Authority` |
| B-R3-04 | nc2 ciego tras una corrección | Aceptado: la mutación excluye un archivo del diff de la delegación verificada | §12.1 paso 9 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R3-02 oráculo contra Git en PR-1 | Aceptado | §16.2 |
| D-R3-03 selección de celdas de U-04 | Aceptado | §12.1 paso 0 |
| D-R3-04 efforts probados por celda | Aceptado | §4.3; §3.4 A7; §8.2 |
| C-R3-03 fallo de transporte en los controles | Aceptado | §12.1 paso 9 |
| C-R3-04 PR-1 tras ajustar el esquema | Aceptado | §16.2 |
| C-R3-05 réplicas de U-04 y «ruta operativa» | Aceptado | §13 |
| A-R3-03 tabla de aplicación sin hogar normativo | Aceptado: pasa a AUTOMATION_PLAN §16 | §2.1; §2.3 |
| A-R3-04 imagen tras rebase en el ADR | Aceptado | ADR decisión 2 |
| A-R3-05 forma de registrar la sustitución de DEV-G1C-01 | Aceptado | §16.1 |
| A-R3-06 nombre de la frase y preámbulo | Aceptado | §2.1; §2.2 |
| B-R3-05 lectura de P-07 y recuperaciones sin cota | Aceptado | §16.2; §3.6; §10.3 |
| B-R3-06 A6 por avance de `main` | Aceptado | §3.4 A6 |
| B-R3-07 descendientes vivos del Worker subagente | Aceptado | §3.3 |
| B-R3-08 imagen en el ADR; `RunId` del rebase | Aceptado | ADR decisión 2; §3.5 |
