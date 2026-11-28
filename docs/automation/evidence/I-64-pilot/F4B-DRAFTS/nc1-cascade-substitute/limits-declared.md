# PBT3x-NC1-CASCADE-SIGNAL-01 — la versión revisada y sus límites declarados

**La versión que se usa:**
- el script `nc1_cascade_signal_substitute.py`, SHA-256 `41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40`;
- el dorado `nc1_cascade_golden.json`, SHA-256 `8eb3a991371777ab218e434f4ecf43dc1fd42403c64dbf5c9a949084efa88c24`.

**La revisión:** independiente, en tres rondas.
- Primera: APTO TRAS CORRECCIONES, 0/6/10.
- Segunda: APTO TRAS CORRECCIONES, 0/2/6.
- Tercera, la «Recomprobación 2»: **APTO**, 0/0/2.

**Los límites declarados,** que no se corrigen porque son menores y la sesión decide usar la versión revisada tal cual:
- **M-17:** el ancla del prompt de la verificación real acepta cualquiera de los SHA de prompt que haya en su `relay-record.json` y cualquier clave con `prompt` y `sha`. Explotarlo exige manipular la copia de la verificación real, que la custodia fija por SHA-256.
- **M-18:** la autoprueba no ejerce las comprobaciones de `main()`, que son el ligado de VALIDATION, el repositorio bueno, la creación de la carpeta de salida y el texto libre del SHA del prompt. Las cubren las sondas de la revisión: N01 a N05, X08 a X10 y R1 a R5.
- **I-6, el límite heredado:** una causa ajena redactada con palabras que no están en la lista cerrada (por ejemplo «falta…», «missing…» o «…vacío») no se detecta. Son las sondas P01 a P05.
- **M-3 y M-4:** son falsos FAIL conservadores (F02 a F05). Además, el prefijo de 8 caracteres cuenta como cita del SHA falso en Remote, Scope, Ci y Tests, como exige `ea01`.
- **La copia custodiada del dorado no es ejecutable tal cual.** En el repositorio público, el dorado `nc1_cascade_golden.json` va redactado: sus rutas locales pasan a marcadores. Por eso su SHA-256 ya no es el `GOLDEN_SHA256` que fija el script, y la autoprueba no lo acepta. La copia ejecutable con el SHA original es la de la sesión.
- **`--task`** solo admite PBT3A, PBT3B, PBT3C, PBT4, PBT5 y VALIDATION. Si PB-T4 o PB-T5 se dividieran en subtareas, el sustituto tendría que editarse y revisarse de nuevo.
