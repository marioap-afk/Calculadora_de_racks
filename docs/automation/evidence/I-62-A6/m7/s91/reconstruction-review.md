# I-62 — Examen de `reconstruct_runs_m7.py` como candidato técnico (decisiones §91, punto 5)

> Análisis estático del diff frente a la referencia de las rondas 3 y 4 y de las pruebas existentes del kit (`selftest-report.json`, grupo B). No se
> ejecutó nada nuevo. La capacidad de reconstruir desde un registro real **no** se declara demostrada: sigue siendo una precondición con fallo cerrado.

## 1. Objetos

| Pieza | SHA-256 | Ubicación |
|---|---|---|
| Referencia (rondas 3 y 4, banco m7) | `5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da` | `I-62-A6/r4/r1probes/reconstruct_runs.py` |
| Candidato v2 | `1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab` | `kits/FX-02/a6m7-measure/reconstruct_runs_m7.py` |

El diff (181 líneas) toca solo: la cabecera; `_abs_shell` (nueva); la separación del analizador interior de pwsh en `_parse_pwsh_inner` (sin cambio de
reglas); `_parse_cmd_inner` (nueva); el despacho de `parse_instrument_command` entre tres representaciones; y el campo `Rendering` en cada ejecución. El
divisor exterior, el analizador de pwsh, la extracción del sobre, la completitud del registro y las reglas de salida son los de la referencia,
textualmente.

## 2. Garantías, una por una

| Garantía (A6-3, regla 6 (a); A6-4, regla 3) | Referencia | v2 | ¿Se reduce? |
|---|---|---|---|
| Identidad de las ejecuciones: toda orden registrada con el blob completo es ejecución del instrumento | `blob in command` | igual | no |
| Completitud del registro (inicio y fin del turno, fin de cada orden, tipos de evento cerrados) | `parse_log` | igual | no |
| Bytes del sobre: un único sobre, LF, ASCII, serialización canónica, sin alterar ningún byte | `extract_envelope` | igual | no |
| Código de salida registrado y coherente con el estado y con `Error` | igual | igual | no |
| `Argv` con la forma exacta de la orden declarada, Git e intérprete por ruta absoluta (`_abs_exe`) y `-I` | gramática pwsh | pwsh igual; cmd con gramática cerrada: cuatro comillas exactas (solo las dos rutas), una sola tubería, palabras desnudas `[A-Za-z0-9_./:=+-]`, prohibidos `&`, `<`, `>`, `^`, `%`, `!`, `(`, `)`, `;`, el acento grave, `$` y la comilla simple, mismo `_abs_exe` para Git e intérprete, `--no-replace-objects`, `cat-file blob <blob>`, `-I`, `-` y orden de la lista cerrada | no en el texto; ver §3 |
| Ruta del shell | absoluta, sin componentes que empiecen por punto | absoluta; admite un componente que **empieza** por punto solo en la posición del shell (`.cache`); siguen rechazados `.`, `..`, punto o espacio final | sí, solo en la posición del shell; necesario para el pwsh del runtime |
| Evidencia custodiada | SHA-256 del registro y de cada sobre | igual, más `Rendering` | no |

Las pruebas existentes lo respaldan: en 62 de 64 registros de referencia (r3, r4, banco y custodiados) v2 da exactamente el mismo resultado; las dos
diferencias son las esperadas (`-NoProfile` ahora reconstruye con los mismos `Argv` y sobres que el caso conforme; `cmd` sigue fallando, con otro
mensaje); las tres representaciones nuevas reconstruyen y 29 variantes mal formadas fallan cerradas.

## 3. Observaciones

1. **La referencia no sirve con el runtime actual.** Rechaza el componente `.cache` de la ruta del pwsh del runtime de Codex, así que no reconstruiría
   ninguna ejecución hecha con ese shell. Nombrar la referencia en la disposición garantizaría una medición NOT_DEMONSTRATED.
2. **Texto registrado frente a orden ejecutada.** r3/r2static mostró que el `command` de Codex no siempre es la línea ejecutada (en 0.162 la real llevaba
   delante `try { [Console]::OutputEncoding=… } catch {}`). Con `cmd /c`, además, la regla de comillas de cmd (más de dos comillas: quita la primera y la
   última) puede cambiar lo que se ejecuta si Codex no envuelve la orden. La reconstrucción no puede detectarlo; lo detecta aguas abajo el aceptante, que
   reejecuta cada `Argv` y compara argumentos, entradas, llamadas y hechos con el sobre registrado (R3): una divergencia da REJECT_FACTS. El fallo
   sigue cerrado, pero la medición se perdería.
3. **El shell no queda fijado.** Ni la referencia ni v2 fijan qué shell es (solo que se llama `pwsh.exe` o `cmd.exe`); el comparador del kit registra si es
   el pwsh del runtime, pero no lo exige, y la comprobación del punto de uso (iii) previa al lanzamiento se hace solo con el pwsh del runtime. Si Codex
   26.1013 usara `cmd.exe`, la comprobación (iii) no habría usado «la tubería del shell de la invocación» (A6-3, regla 3). Cambio mínimo, cuando el
   Coordinator lo disponga: hacer (iii) con cada shell admitido, o exigir el pwsh del runtime y dar NOT_DEMONSTRATED con otro.
4. **Ejecuciones que no nombran el blob** (`-EncodedCommand`, variables, archivos) no se identifican: límite heredado de la referencia.

## 4. Viabilidad

Candidato técnico **viable**: la ampliación no reduce las garantías de identidad, bytes, `Argv` ni custodia; la única relajación (el punto inicial en la
ruta del shell) es imprescindible con el runtime actual. Su suficiencia frente a un registro real de 26.1013 **no está demostrada** (observaciones 2 y
3), y ese es el riesgo principal de la medición única.
