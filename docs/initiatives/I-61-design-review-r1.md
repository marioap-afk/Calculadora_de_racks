# I-61 — Revisión de diseño, ronda 1: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v1.md (blob 9dfa957b) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob 1feca318), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D enrutamiento/catálogo, B operación/fallos, A autoridad, C obligaciones/gates). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_0d41af0e-fe9 de la sesión de I-61
Veredicto: CHANGES REQUIRED — 56 REQUIRED (D 11, B 17, A 12, C 16) y 32 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V2 y ADR-0046 revisado
```

Ningún REQUIRED se rebaja. «Aceptado con ajuste» significa que se corrige el defecto descrito con una solución distinta de la sugerida; el motivo figura en la fila. Las secciones
remiten a [I-61-proposal-v2.md](I-61-proposal-v2.md) salvo que se indique ADR.

## REQUIRED

| Hallazgo | Defecto (resumen) | Disposición | Dónde |
|---|---|---|---|
| D-01 | Elegibilidad circular; U-04 sin contingencia; §11/§14 lo daban por medido | Aceptado | §4.2 contingencia, §4.3 celdas, §11, §12.1 paso 0, §14 |
| D-02 | Falta unir nivel/effort del enrutamiento con el catálogo | Aceptado | §5 (nivel y tabla de effort); OBL-05 |
| D-03 | Consumo fuera de la regla estable; `service_tier` sin evaluar | Aceptado con ajuste: consumo cubierto como condición de elegibilidad (UNKNOWN ⇒ no elegible). El `service_tier` no se sobrescribe (el único valor documentado es `fast`, que equivale al heredado): se declara UNKNOWN y pendiente del Owner, no bloqueante, con tope de invocaciones y STOP P-06 | §4.3, §11, §16.2, §17 |
| D-04 | Modelo y effort del Controller sin fijar | Aceptado | §4.3 «Controller», §11 receta |
| D-05 | Catálogo con datos no oficiales | Aceptado | §5 (tipo de fuente; reglas del contenido inicial) |
| D-06 | Sin campo para exigir el enrutamiento; effort efectivo no exigido | Aceptado | §4.3 `RoutingEnforcement`; §8.3 `Routing`; §16.2 G3 |
| D-07 | ADR afirma imposibilidad del worker Codex con escritura | Aceptado | ADR Contexto y Alternativas; §11 |
| D-08 | Sin correspondencia de los 14 criterios | Aceptado | §18 |
| D-09 | Cambio de `config.toml` solo como desviación; «neutralizan» | Aceptado | §3.2, §10.1, §10.3 P-01, §14 («detectan») |
| D-10 | OBL-05 con lista fija | Aceptado | OBL-05 |
| D-11 | Falta `Owner` en el esquema | Aceptado | §8.2 delegación `Owner` |
| B-01 | Orden de commits frente a HEAD = `CurrentSha` | Aceptado | §3.2 orden congelado |
| B-02 | Procesos vivos no ejecutable | Aceptado | §3.3 |
| B-03 | Igual que D-01 más campo de exigencia | Aceptado | Como D-01 y D-06 |
| B-04 | Igual que D-11 | Aceptado | §8.2 |
| B-05 | STOP no representable; sin artefacto de fallo de transporte | Aceptado | §8.2 `Disposition`, `TriggeredStopConditions[]`, `rackcad-relay-record/v1` |
| B-06 | VERIFIED no atado a `Checks[]` | Aceptado con ajuste: `Checks` pasa a objeto de 14 propiedades fijas y requeridas, de modo que omitir una es imposible por esquema; la coherencia la valida el relevo con una regla documentada, con fixtures negativos | §8.3; OBL-11 |
| B-07 | Oráculo ciego de los controles negativos | Aceptado: el oráculo exige la comprobación concreta; las comprobaciones anteriores deben coincidir con la verificación real (no todas, porque un SHA inexistente hace fallar por construcción las posteriores) | §12.1 paso 9 |
| B-08 | Identidad de reejecuciones y colisión de rutas | Aceptado | §3.5 `RunId` y delegación abierta; §11 ruta con `<RunId>` |
| B-09 | Conteo no determinista | Aceptado | §9 |
| B-10 | Bucles BLOCKED sin tope | Aceptado | §9 topes; §10.3 P-04 |
| B-11 | Aceptación del paquete solo por esquema | Aceptado | §3.4 |
| B-12 | `remote-facts` sin campos | Aceptado: los hechos remotos son una sección del registro de relevo con campos congelados | §8.2 `RemoteFacts`; §8.3 `Ci`; §12.1 paso 7 |
| B-13 | RED no ejecutable ni atribuible | Aceptado (esqueleto compilable, commit RED con CI propia, GREEN aparte) | §12.1 paso 6; OBL-P1 |
| B-14 | §8 arrastra el PR draft | Aceptado | §1 no-objetivos; §2.2 |
| B-15 | `config.toml` solo desviación; modo sin `--ephemeral` no medido | Aceptado | §3.2; §11; sonda PR-1 en §16.2 |
| B-16 | Custodia sin destino ni hashes reverificables | Aceptado con ajuste: copia en la evidencia versionada con identidad = blob de Git (reverificable sin `.gitattributes`); el SHA-256 transitorio queda como procedencia y se compara tras el commit | §11 «Custodia duradera» |
| B-17 | `AuthorityRevision` sin semántica | Aceptado | §9 «Identidad»; §8.3 `Authority` |
| A-01 | Paquete no contrastado con el contrato; oráculo circular | Aceptado | §3.4; INV-10; OBL-10; nc4; §3.5 actores del análisis |
| A-02 | GATE PASS sin exigir VERIFIED | Aceptado con ajuste: se congela cuándo **termina** un trabajo delegado (solo con VERIFIED sobre el SHA de la entrega) en vez de añadir una condición a LIFECYCLE §7, que no es dominio de esta unidad | §3.1 |
| A-03 | Sin documento dueño para las reglas nuevas | Aceptado | §2.3 |
| A-04 | «Sin cambio de dueño» incorrecto; M-08 y EXP-07 sin evaluar | Aceptado | §2.1; §17; ADR decisión 1 |
| A-05 | `AuthorityRevision` y §2 «Modo normal» | Aceptado | §9; §2.1 (frase declarativa de F-05) |
| A-06 | Lista STOP frente a la fila OV | Aceptado | §10.3 |
| A-07 | §§8-9 completos; actor del «ejecutor» | Aceptado | §2.2 |
| A-08 | Guarda antiduplicación incompleta | Aceptado | §13 frases testigo; OBL-06 |
| A-09 | ADR restringe Computer Use | Aceptado | ADR decisión 8 y Alternativas |
| A-10 | Igual que D-07 | Aceptado | ADR |
| A-11 | Redacción de evidencia en el ADR | Aceptado | ADR decisión 6; §8.3 |
| A-12 | Sin puntos de extensión ni FOUNDATIONS | Aceptado | §15 |
| C-01 | RED por compilación | Aceptado | Como B-13 |
| C-02 | Oráculo de OBL-P2 | Aceptado | OBL-P2 |
| C-03 | Texto libre ciego | Aceptado | §8.4 actores; nc3; OBL-01 |
| C-04 | Controles negativos sin actor, oráculo ni ubicación | Aceptado | §12.1 paso 9 |
| C-05 | Igual que B-05 | Aceptado | §8.2 |
| C-06 | OBL-08 sin condición de fallo | Aceptado | §9 escenarios; OBL-08 |
| C-07 | Relevo sin obligación | Aceptado | OBL-09; registro de relevo |
| C-08 | RED de OBL-03/04/06 solo por ausencia | Aceptado | §13 mutaciones por aserción |
| C-09 | U-04 presentado como medido | Aceptado | Como D-01; §16.2 (solicitado y efectivo comparados) |
| C-10 | Matriz OV sin la forma de la guía §7.2; OV-03 condicional | Aceptado con ajuste: OV-I61-03a es incondicional; OV-I61-03b depende de un DWG anterior a I-60 y su retirada exige decisión explícita del Owner (guía §7.2), nunca un salto silencioso | §16.3 |
| C-11 | D-3 sin observación | Aceptado | OV-I61-01 |
| C-12 | Decisión antes del cierre | Aceptado | §12.1 pasos 10-11 |
| C-13 | READY-03 y DEV-G1C-01; rechazo del ADR | Aceptado | §16.1; §16.2 READY |
| C-14 | FOUNDATIONS ausente | Aceptado | §15 |
| C-15 | Igual que A-11 | Aceptado | ADR decisión 6 |
| C-16 | Igual que A-09 | Aceptado | ADR decisión 8 |

## OPTIONAL

| Hallazgo | Disposición | Dónde |
|---|---|---|
| O-01 regla de dimensiones | Aceptado | §4.1 |
| O-02 retiros y responsable de la frescura | Aceptado | §4.3; §5 |
| O-03 ToolCalls y effort en métricas | Aceptado | §12.1 métricas |
| O-04 Computer Use y protocolo idéntico | Aceptado | §11; ADR decisión 8 |
| O-05 proveedor fijado del Controller | Aceptado | ADR Alternativas |
| O-06 12 puntos de ARCHITECT REVIEW | Aceptado | §17 |
| O-07 quién evalúa los disparadores de B | Aceptado | §14 |
| O-08 selección del transporte; perfiles en la guía | Aceptado | §4.2 paso 3; §6 |
| B-18 `TestResults[]` | Aceptado | §8.2 |
| B-19 matiz de evidencia en el ADR | Aceptado | ADR decisión 6 |
| B-20 Computer Use por referencia | Aceptado | ADR decisión 8 |
| B-21 compatibilidad con `--output-schema` | Aceptado | §8; sonda PR-1; OBL-02 |
| B-22 tope, terminación y `service_tier` | Aceptado | §3.3; §11 |
| B-23 lista de términos y propiedad `Gate` | Aceptado | §8.4 |
| B-24 artefacto y autor del análisis | Aceptado | §3.5; §9 |
| A-13 posición de la fila de WORKFLOW §10 | Aceptado | §2.1 |
| A-14 referencia a la Proposal; cláusula alineada | Aceptado | ADR cabecera y decisión 1 |
| A-15 ruta ante un rechazo | Aceptado | §16.1 |
| A-16 quién registra la aceptación | Aceptado | §16.1 |
| A-17 actores del texto libre | Aceptado | §8.4 |
| A-18 etiqueta INFERENCE de Q-06 | Aceptado | §12 |
| A-19 P-17 | Aceptado | ADR Alternativas |
| A-20 título de §G y cabecera | Aceptado | §2.1; §7 |
| C-17 comprobación recursiva | Aceptado | §8; OBL-02 |
| C-18 restricciones de las pruebas Core | Aceptado | §13 |
| C-19 negación en `.gitignore`; prosa | Aceptado | OBL-04 |
| C-20 igual que D-10 | Aceptado | OBL-05 |
| C-21 paridad; corrida focal UI | Aceptado: se retira la frase de paridad; corrida UI en la evidencia | OBL-P2, OBL-P3; §16.2 |
| C-22 deberes de la guía §7.2 y §8 | Aceptado | §16.2 |
| C-23 qué se copia | Aceptado | §11 custodia |
| C-24 responsable de la regla de 90 días | Aceptado | §4.2 paso 3; §4.3 |
| C-25 igual que A-14 | Aceptado | ADR cabecera |

## Cambio introducido por el autor, sin hallazgo previo

- **Fila del índice de ADR en el commit de cierre** (WORKFLOW §11.4). V1 la situaba antes, con ventana de I-52. Se somete a la ronda 2.
