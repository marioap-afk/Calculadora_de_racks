# FX-02 — Orden FX-U1-O4 publicada (S02; decisiones §65, punto 7)

> Supervisión, plano a, como Coordinator del fixture (plano c). El texto publicado está en el fixture (repositorio público): `fx/u1`, commit
> `95bdc29d428b35d2188a10e1c5462d6caf2be4df` (padre `c785def9`), `docs/automation/decisions/FX-U1.md` blob `3fcfffc1aa7ad442422d4da2e5edaf89c48239ea`. Bloque añadido: 19126 bytes,
> SHA-256 `faa96146e3779b28d7584873bb1880a9f4a08b0fedddd05a7fe2a84f9cacbbf1`. Búsqueda literal P-16 (lista de la plantilla, L15-L19, ampliada): 0 aciertos. CI del fixture: corrida 38002393525, `push`,
> **success**. Push a `origin` y a `github` (las dos puntas = `95bdc29d`).

## Trazabilidad por punto (de qué disposición sale cada texto)

| Punto | Fuente | Nota |
|---|---|---|
| 1-3 | plantilla `order-FX-U1-O4.template.md` (G1) | sin cambio frente al borrador |
| 4 | plantilla + A-4, A4-1 regla 1 (decisiones §64, punto 2; §65, punto 5) | la shell declarada es la de la línea `Shell` del bloque de transporte posterior; ningún otro elemento de la receta cambia |
| 5 | U-09 (a) y U-10 (b) (decisiones §62, punto 3); A-4, A4-4 regla 2 (conjunto de referencia vacío en la planificación) | marcador `I62-ROLE-BINDING: <BindingId> ACCEPTED` |
| 6 | A-4, A4-3 reglas 1-5 y A4-4 reglas 1, 3, 4 y 5 (decisiones §64, punto 1); U-23 (a) (decisiones §62, punto 3) | dos marcadores de preautorización. Celdas del Worker: la lista cerrada de celdas `claude-subagent` medidas para `write-commit-push` en el catálogo (`claude-sonnet-5-5`, effort `medium` y `high`; sonda U-04). U-22 (G5⚑) sin disponer: se retira su marcador con el mismo efecto (README del kit, §4, punto 6), que la orden hace explícito («Ninguna reejecución BLOCKED del Worker») |
| 7 | U-09 (e) (decisiones §63, punto 3) | T3 solo con A1'-A8' íntegras y evidencia real; si no, T3' |
| 8 | U-13 (a) (G1, relleno del Coordinator del fixture) | la exención del Architect (U-13 (a'), G5⚑) va al bloque de autorización de su revisión (punto 17) |
| 9 | U-08 (decisiones §62, punto 3) | `.gitignore` publicado en `c785def9` |
| 10 | U-07 (decisiones §62, punto 3) | artefacto `fixture-tests-trx` del job `fixture-tests` (commit `b6d294e`) |
| 11 | U-17 (decisiones §62, punto 3) | — |
| 12 | U-19 (decisiones §62, punto 3) | el contrato de T1 solo tiene comodines finales `/**` |
| 13 | plantilla | — |
| 14 | U-24 (decisiones §62, punto 3) | — |
| 15 | U-16 (1), (2) y (4) (decisiones §62, punto 3; §63, punto 4); U-16 (3) (texto congelado) | mutaciones copiadas literalmente de la parte A sellada (`negatives.md` `25a33a48…`): N4, N5, N7a, N7b y N8 (resellada por §63.4). N5: la orden selecciona la alternativa por la fuente de conteos del punto 10 (U-07). **N9 y N10: ninguna disposición aprueba sus mutaciones** (§63.4 solo aceptó N4, N5, N7a y N7b y mandó resellar N8): quedan para un commit posterior del Coordinator del fixture antes del Q0, si el Coordinator de I-62 las aprueba; sin ellas no se ejecutan |
| 16 | U-18 (b) (decisiones §62, punto 3); A-4, A4-5 (decisiones §64, punto 1) | la regla 4 de A4-5 se transcribe sin nombrar el escenario |
| 17 | D-0 (decisiones §65, punto 4): la revisión del Architect es una obligación posterior al PASS del piloto, sin omitirla | la colocación tras el Q7 (U-11 (a)) se deduce de D-0. El bloque de autorización (adapter, celda, directorio, RLA, AUTHOR, exención) se publica después del Q7: depende de la elegibilidad de A4-2 (D-3) y de la reconciliación de AUTHOR (D-6) |
| 18 | U-12 (G5⚑) sin disponer | se retira con su efecto: ningún Reviewer en esta orden |
| 19 | U-18 (c) (G5⚑) sin disponer | se retira con su efecto: espera tras el Q7 |
| 20, 22 | plantilla | — |
| 21 | U-06 (a) y (b) (decisiones §62, punto 3); U-06 (c) (G2) | — |
| Topes | V14 D.3 sin cambio; A-4, A4-2 regla 3 y A4-5 regla 1 | sin fila del Reviewer (U-12); la corrección de A4-5 cuenta en `T1/codex-cli/pool` |

El bloque posterior «Autorización de transporte `codex-cli`» (S13, tras S10) **no** se ha publicado: su `ConfigSha256` será `73890CA3…` (OD-2f = A,
decisiones §65) y sus `Shell`, `Cells` y `MeasurementRecord` dependen del resultado de la sonda A4-1.
