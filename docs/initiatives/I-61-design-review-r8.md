# I-61 — Revisión de diseño, ronda 8: adjudicación

```text
Objeto revisado: docs/initiatives/I-61-proposal-v8.md (blob 8199f211) y docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md (blob 3c7a4ead), sin commit
Modo: SAME-SESSION ROLE (Architect en cuatro lentes: D, B, A, C). Ayuda de revisión, NO revisión independiente
Ejecución: workflow wf_972c4bea-38d de la sesión de I-61
Veredictos: D = AGREED; B = AGREED; A = AGREED; C = AGREED
Ronda 7: todos los hallazgos RESOLVED
Nuevos: 0 REQUIRED y 11 OPTIONAL
Adjudica: Architect (misma sesión). Resultado: Proposal V9 (aplica los 11 OPTIONAL) y ADR-0046 (solo la referencia a la versión)
```

Con cero REQUIRED la ronda cierra en **AGREED**. Los OPTIONAL se aplican porque son de redacción o alineación con reglas normativas ya acordadas, y uno (B-R8-03) evita una
comparación que fallaría abierta. La V9 se somete a una **confirmación acotada** del delta V8 → V9 (adjudicación al final).

| Hallazgo | Disposición | Dónde en V9 |
|---|---|---|
| C-R8-01 / A-R8-01 / D-R8-01 descripción de `ChainRedSha` en el esquema | Aplicado: «el RED vigente de §9, o `null`» | §8.2 |
| C-R8-02 / D-R8-02 pasos 6.1-6.2 frente a `RT` desde `ChainBaseSha` | Aplicado | §12.1 paso 6 |
| B-R8-01 paso 7 sin el calificativo «exige RED» | Aplicado: el paso 7 solo registra hechos; la disposición es la de la fila 9 | §12.1 paso 7 |
| B-R8-02 / D-R8-03 «pendiente» y tope de recuperaciones | Aplicado: definición de «cadena en curso»; el rebase de apertura no consume el tope | §3.5 |
| B-R8-03 `CreationDate` local frente a ventana UTC | Aplicado: registro y comparación en UTC | §3.3 |
| A-R8-02 rutas en conflicto en el `RebaseMap` | Aplicado: rutas en conflicto e imágenes `null` | §3.6; §8.2 |
| A-R8-03 inciso de `B` y delta | Aplicado | §9; §0 |

## Confirmación del delta V8 → V9

| Paso | Ejecución | Objeto | Resultado |
|---|---|---|---|
| Confirmación 1 | workflow `wf_6fa0ed72-d8c` (un agente Architect, SAME-SESSION ROLE) | V9, blob `1cc9bf12`, contra V8 | Los 11 OPTIONAL APPLIED, sin cambios no explicados. **N-R9-01 [REQUIRED]:** el rebase de apertura con cadena en curso no decía cómo cuenta en el tope de invocaciones de §16.2. N-R9-02 y N-R9-03 [OPTIONAL]: etiqueta del delta y «las pruebas con `ExpectRed`» en el paso 6.1 |
| Corrección | edición en sitio de la V9 (no publicada) | §3.5, §16.2, §0, §12.1 paso 6.1 | El rebase de apertura con cadena en curso no consume el máximo de 2 recuperaciones, pero añade al tope lo mismo que una recuperación; N-R9-02 y N-R9-03 aplicados |
| Confirmación 2 | workflow `wf_d2995aa2-fa9` | V9, blob **`fccae56d`** | **AGREED**: N-R9-01..03 APPLIED, cero REQUIRED nuevos |

**OPTIONAL de la confirmación 2 no aplicados**, para conservar el blob acordado; ninguno cambia un elemento congelado:

- N-R10-01: la etiqueta del delta V8 → V9 admite contar 11 o 12 cambios (nota de historial).
- N-R10-02: no se dice si un rebase de apertura **sin efecto** (sin avance de `origin/main`) suma al tope. Sin cambio de punta no hay reverificación ni reemisión, así que no produce
  invocaciones de más. Se aplica la lectura conservadora: solo cuenta el rebase que emite un registro `REBASE` con `MainSha` nuevo.

**Versión acordada:** `docs/initiatives/I-61-proposal-v9.md`, blob `fccae56dfbea82fbe092555d51a0fb57baff5651`, con ADR-0046 `propuesto` (blob
`154e067d1fd7920e04801ffb93afcf39c3af55be`).
