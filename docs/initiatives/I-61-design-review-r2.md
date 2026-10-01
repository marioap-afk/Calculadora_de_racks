# I-61 — Revisión de diseño, ronda 2: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v2.md (blob 229ca694) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob 6d18c8ba), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_465b000c-68c de la sesión de I-61
Ronda 1: de 56 REQUIRED, 52 RESOLVED y 4 PARTIAL (D-03, B-02, B-08, A-05); todos los OPTIONAL RESOLVED
Nuevos: 19 REQUIRED y 18 OPTIONAL. Veredicto de las cuatro lentes: CHANGES REQUIRED
Adjudica: Architect (misma sesión). Resultado: Proposal V3 y ADR-0046 revisado
```

Ningún REQUIRED se rebaja. Las secciones remiten a [I-61-proposal-v3.md](I-61-proposal-v3.md) salvo que se indique ADR.

## PARTIAL de la ronda 1

| Hallazgo | Residuo | Disposición | Dónde |
|---|---|---|---|
| D-03 | `service_tier` «no bloqueante» frente a «UNKNOWN ⇒ no elegible» | Aceptado: el criterio de consumo tiene una sola regla de evidencia; el `service_tier` queda fuera del criterio como riesgo registrado con mitigación | §4.3; §11; §17 |
| B-02 | Procesos con `CommandLine` ilegible; «descendientes» ambiguo | Aceptado. Medido hoy: entre los ilegibles de la lista cerrada solo aparece `codex-windows-sandbox-service.exe`, que queda como exclusión nominal | §3.3 |
| B-08 | `RunId` contradictorio; reverificación tras rebase inejecutable | Aceptado | §3.5; §3.6 |
| A-05 | «Normas globales en `MainSha`» frente a la comprobación 3 | Aceptado: solo los cambios normativos de la unidad se leen en `AuthorityRevision`; las demás autoridades en `MainSha` y sin cambios en la rama | §2.1 frase de §2; §8.3 `Authority`; §9 |

## REQUIRED nuevos

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-R2-01 | Fuente oficial de configuración no versionada | Aceptado: versionada en la evidencia §12 con URL, redirección y la advertencia de que la herramienta puede resumir; ninguna regla congelada depende ya de la equivalencia `priority` = `fast` | §5; §11; evidencia §12 |
| D-R2-02 | Cronología falsa sobre el `service_tier` | Aceptado: es cierto que el valor se registró en `07ef2a85`, el mismo commit que registra la orden de continuidad. Se retiran la frase y la INFERENCE | §4.3; §11 |
| D-R2-03 | «Consumo cubierto» sin criterio de verificación; Controller incoherente | Aceptado: evidencia oficial o medida; regla de sondas; una sola regla para Controller y Worker | §4.3; §5; ADR decisión 4 |
| D-R2-04 | Tope de invocaciones sin sitio para correcciones | Aceptado | §16.2; §10.3 P-07 |
| B-R2-01 | Procesos vivos (detalle de B-02) | Aceptado | §3.3 |
| B-R2-02 | Semántica del `RunId` | Aceptado (`DelegationRunId`, `WorkRunId`, token `{WorkRunId}`) | §3.5; §8.2; §8.3 `Handoff`; A2 |
| B-R2-03 | §8.3 frente a §10.1 (entrega ausente; permisos) | Aceptado | §8.3 `Handoff` y `Denials` |
| B-R2-04 | Igual que D-R2-04 | Aceptado | §16.2 |
| B-R2-05 | Oráculo de nc1..nc3 sin verificación VERIFIED garantizada | Aceptado: solo sobre la verificación VERIFIED final; oráculo relativo | §12.1 paso 9 |
| B-R2-06 | Recuperación tras rebase inejecutable | Aceptado: rebase de la sesión con `RebaseMap` y reverificación por imágenes y `patch-id` | §3.6; §8.3 `Identity`/`Ci` |
| B-R2-07 | OBL-04 frente a las órdenes del README | Aceptado: definición de «ruta operativa» con bloques `text operativo` | §13 |
| A-R2-01 | Frases de alcance de AUTOMATION_PLAN intactas | Aceptado: línea 3 y párrafo de autoridad enumerados; el título se conserva y se dice | §2.1; §16.2 G2 |
| A-R2-02 | Enumeración de §2.2 incoherente; ADR con otra lista | Aceptado: todas las secciones clasificadas, §11 incluido; el ADR remite a la sección de la Proposal congelada | §2.2; ADR decisión 1 |
| C-R2-01 | Bucles de corrección y recuperación; tope; nc sobre la verificación final | Aceptado | §12.1 pasos 8-9; §16.2 |
| C-R2-02 | Oráculo ciego de nc4 | Aceptado: nc4 se evalúa antes de aceptar la real, sin cortocircuito, con A3 como único `fail` | §3.4; §12.1 paso 4; OBL-10 |
| C-R2-03 | RED exigido siempre en `Ci`/`Tests` | Aceptado: RED solo con `RedSha`; `ChainRedSha` en las delegaciones siguientes; A5 compara `ExpectRed` solo en la primera | §8.3; §3.4 A5; §9 |
| C-R2-04 | `attempts` frente a AUTOMATION_PLAN §8-9 | Aceptado: regla acotada a la ejecución delegada; corrección tras STOP consume; fuera de la delegación rige el plan | §9 |
| C-R2-05 | OBL-09 sin disposición por condición | Aceptado (P-08 nuevo) | OBL-09; §10.3 |
| C-R2-06 | U-04 sin oráculo contra Git | Aceptado | §12.1 paso 0 |

## OPTIONAL nuevos

| Hallazgo | Disposición | Dónde |
|---|---|---|
| D-R2-05 retiro con precisión de mes | Aceptado | §4.3 |
| D-R2-06 `Routing` con `advisory` | Aceptado | §4.3 |
| D-R2-07 sondas atadas a las celdas enrutadas; Controller | Aceptado | §4.3 «Controller»; §12.1 paso 0; §16.2 PR-1 |
| D-R2-08 términos de la regla de dimensiones | Aceptado (`tool-use`, horizonte Alto, suelo `Routine`, transporte más preferido) | §4.1; §4.2; §4.3 |
| B-R2-08 instantánea de claves en la salida | Aceptado | §3.2; §8.2 |
| B-R2-09 Worker subagente con tope y finalización notificada | Aceptado con ajuste: la orquestación de subagentes de la sesión notifica la finalización en lugar de bloquear; la sesión no opera hasta la notificación y el tope es de 60 min | §3.2 |
| B-R2-10 espera de la CI | Aceptado (90 min, consulta sin reinvocar) | §12.1 paso 7 |
| B-R2-11 términos de uso corriente | Aceptado (frases completas) | §8.4 |
| B-R2-12 huecos de nc4, A2/A8 y `not_run` | Aceptado | §3.4; §8.2; §8.3; §12.1 paso 4 |
| A-R2-03 referencia a §3.3 | Aceptado | §2.1 |
| A-R2-04 redacción de §15 | Aceptado | §15 |
| A-R2-05 DEV-G1C-01 frente a decisiones §11 | Aceptado | §16.1 |
| A-R2-06 aviso del número 0046 a I-52 | Aceptado | §16.1 |
| C-R2-07 igual que A-R2-05 | Aceptado | §16.1 |
| C-R2-08 fuente de 03b e identidad de los DLL legacy | Aceptado | §16.3 |
| C-R2-09 prompt de los controles | Aceptado | §12.1 paso 9 |
| C-R2-10 campos y coincidencia de `FreeText` | Aceptado | §8.4 |
| C-R2-11 PR-1 negativa | Aceptado | §16.2 |
