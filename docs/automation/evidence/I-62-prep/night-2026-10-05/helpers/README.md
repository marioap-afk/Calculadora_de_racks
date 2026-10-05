# I-62 — Helpers deterministas P1/P2/P3/P5/P6 (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION)

```text
Autoridad:   orden nocturna §3.C y §8 (decisiones §41). Dentro del Freeze según el triaje del Coordinator, sujeto a contraste con las cláusulas
Naturaleza:  extraen HECHOS; no son un rol, servicio, daemon ni autoridad MechanicalVerifier; nunca firman EXECUTION_VERIFIED ni GATE PASS
Código:      i62_helpers.py (stdlib + git CLI); test_i62_helpers.py (18 pruebas, OK); make_readings.py → real-readings.json
```

## Reproducción

```text
set I62_REPO=<checkout con main ⊇ bb0d5522>   set I62_TRX_UI=<ui.trx de READY-05 de I-63>   set I62_TRX_FOCAL=<trx focales>
python -X dev -m unittest test_i62_helpers           → Ran 18 tests … OK (sin ResourceWarning)
python make_readings.py <repo> <dir de TRX> real-readings.json
```

Los bytes de los TRX no se versionan (16.12; evidencia de I-63 §16). Viven en `D:\r62-exp-night\trx\`, y sus SHA-256 están en `real-readings.json`.

## Resultados sobre artefactos reales de I-63 (`real-readings.json`)

| Gate | P1 entrega real | P1 nc1 (`CurrentSha` mutado) | P2 delegación real | P2 nc2 (`AllowedWriteScope` mutado) | Controller en I-63 |
|---|---|---|---|---|---|
| G1 | PASS | FAIL | PASS | FAIL (`RackMetricIdsTests.cs`) | nc1 y nc2 cumplidos |
| G2 | PASS | FAIL | PASS (11 rutas) | FAIL (`ProjectPopulation.cs`) | nc2 NO cumplido (DEV-G2-01) |
| G3 | PASS | FAIL | PASS (8 rutas) | FAIL (`RackComputedExpressionContext.cs`) | nc1 y nc2 cumplidos |
| G4 | PASS | FAIL | PASS (12 rutas) | FAIL (`ProjectPopulation.cs`) | nc1 y nc2 NO cumplidos (DEV-I63-G4-02/03) |

- **P1:** el `prompt.md` de nc1 de G4 contiene el GREEN `4f45f446`, la narrativa que confundió al Controller. El helper no tiene parámetro de prompt y
  falla igual.
- **P6:** los cuatro TRX dan PASS.
  - `ui.trx` de READY-05: 1654 en total, 1637 superadas y 17 omitidas.
  - Cada omitida trae su motivo del runner (`ErrorInfo/Message`).
  - Los contadores dicen `notExecuted=0`, que es la convención del logger (I-63 §37.4).
  - Las 17 coinciden una a una con los métodos que declaran `Skip =` en `tests/RackCad.UI.Tests` del Candidato `55a66b3c`. El número no está fijado en
    ningún sitio: se deriva del código.

## Qué hace cada helper y qué huecos deja

| Helper | Hechos | Huecos documentados (no se resuelven con semántica nueva) |
|---|---|---|
| **P1** `p1_identity` | `CurrentSha` parseado del artefacto estructurado; tipo de objeto en Git; `BaseSha` ancestro; `RedSha` entre ambos | no compara `HEAD` con el remoto: esa es la comprobación de `Identity`, y con custodia encima la rige P4. Un SHA en mayúsculas falla, en vez de normalizarse |
| **P2** `p2_scope` | rutas de `base..current` **sin detección de renombres** (borrado + alta); los borrados y los cambios de modo cuentan; prefijo solo si la entrada termina en `/`; si no, igualdad exacta; Forbidden gana | sin comodines (NOT_EVALUATED); comparación exacta con mayúsculas y minúsculas, y una coincidencia solo sin distinguirlas se informa como hueco; enlaces y gitlinks se informan sin resolver; un rango no comparable es NOT_EVALUATED, nunca PASS |
| **P3** `p3_facts`, `assert_no_verdict` | agrupa P1 y P2 en `Kind: FACTS` | rechaza cualquier campo o valor de veredicto (`Classification`, `Disposition`, `VerifiedSha`, `EXECUTION_*`, `GATE PASS`…). La clasificación queda en el Controller |
| **P5** `ConfigGate` | línea base aceptada frente a observación (configuración y binario ficticios); deriva = STOP; un preflight nuevo es solo observación; aceptar exige una referencia de decisión que nombre **esa** observación; reanudar solo con MATCH | no toca `config.toml` real ni ejecuta sondas: una prueba lo comprueba. El formato de la referencia de decisión es un ejemplo, no una política |
| **P6** `p6_trx`, `declared_skip_methods` | lectura normalizada: contadores crudos y resultados por desenlace; omitidas solo con evidencia del runner; Passed nunca incluye omitidas; contradicciones entre contadores y resultados; selección vacía; cruce con las declaraciones `Skip =` del código | atributos de omisión condicionales o en tiempo de ejecución (`Skip` dinámico) no se detectan en el código; un NotExecuted sin motivo es inesperado y falla |

## Límites

- Ninguna salida es evidencia de gate ni sustituye al Controller.
- Ningún comportamiento congelado cambia: los helpers aplican 16.9 #2, #5 y #7 tal como están.
- Lo que P4 cambiaría (`Identity` con custodia) está en el dossier de P4, no aquí.
