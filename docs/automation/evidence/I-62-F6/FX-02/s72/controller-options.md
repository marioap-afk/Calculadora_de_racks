# FX-02 — Opciones reales para un Controller elegible (decisiones §72, punto 3)

> Supervisión, plano a. Solo análisis de la evidencia ya custodiada; ninguna invocación. No rebaja ningún criterio normativo: lo que se exige al
> Controller (las operaciones de PLAN y VERIFY y la fidelidad de los insumos) sigue igual. La sonda 2 del bloque A2-P2 sigue HELD.

## 1. Evidencia nueva sacada de los eventos custodiados

Fuentes: `probe-events.jsonl` y `probe-last.json` de las dos mediciones (`I-62-F6/OD-2/R20261010T000542Z-a4-1-probe/` y
`I-62-F6/OD-2/R20261011T050156Z-a2p2-v4-controller/`).

| Hecho | A4-1 (`cmd.exe`) | Sonda 1 (`pwsh` nativo) |
|---|---|---|
| Comandos / tiempo usado | 11 / ~100 s de 600 | 4 / ~105 s de 600 |
| Tokens de salida (de ellos, razonamiento) | 13 172 (2 478) | 12 443 (2 691) |
| Tamaño de la respuesta final | 24 844 caracteres (32 pasos, más los objetos canónicos de PLAN y VERIFY) | 24 651 caracteres (ídem) |
| El texto de la sonda empieza con | «Tiempo limitado. …» | «Tiempo limitado. …» |
| `Objective` del contrato en la **salida cruda del comando** | `"Añadir a Fixture.Lib…"`: correcto (byte `0xf1`) | `"A�adir a Fixture.Lib…"`: **ya deformado** (U+FFFD) |
| `Objective` en la **respuesta de la celda** | escapes mal formados (`Versi\u0000f3n`, `Designaci\u0000f3n`) | el U+FFFD heredado del transporte |

## 2. Causas, con su mecanismo

1. **Operaciones omitidas.** En las dos corridas la celda dedica casi toda su salida a la respuesta final (≈ 12-13 k tokens, con solo ≈ 2,5 k de
   razonamiento). Para tras 4-11 comandos, con el 80-83 % del tope sin usar. El texto empezaba con «Tiempo limitado» y pedía, en una sola respuesta, 32
   pasos con 175 claves y los dos objetos canónicos. La evidencia apunta a un efecto del **diseño de la medición** (presión de tiempo y una respuesta
   enorme), no a un límite del runtime. No demuestra que la celda sea capaz: solo identifica qué cambiar para medirla de forma representativa.
2. **Fidelidad UTF-8: dos mecanismos distintos.**
   - Con `cmd.exe`, el **transporte es correcto** (`type` entrega los bytes UTF-8 bien). El fallo está en la **salida del modelo**, que escribe escapes
     JSON mal formados (`\u0000f3` en lugar de `ó`).
   - Con el `pwsh` nativo, el **transporte falla**: la salida de la consola de `pwsh` no va en UTF-8 y Codex la decodifica con U+FFFD. Ni el modelo
     ni la verificación pueden recuperar esos bytes después. No consta si el `pwsh` del sandbox (en modo ConstrainedLanguage) permite fijar
     `[Console]::OutputEncoding`: nunca se ha medido.
3. **Errores de lectura de Git (3 y 6)** y **`ls-remote` bloqueado por el sandbox (2 y 2):** el primero es menor; el segundo no lo necesita VERIFY, que
   comprueba `Remote` en el registro (análisis de §70).

## 3. Opciones

| Opción | Autoridad | Medición nueva | Qué resolvería (con evidencia) | Qué no resolvería | Valoración |
|---|---|---|---|---|---|
| **A. Celda alternativa ya autorizada con evidencia medida** | — | — | — | — | **No existe.** `claude-cli` está autorizado solo para ARCHITECT/REVIEWER (CLAUDE-CLI-I62; `A4-CLAUDE-FX02-CONSUMO` excluye EXECUTION_CONTROLLER), y con Read/Grep/Glob no ejecuta Git. No hay otra celda de `codex-cli` medida para el Controller; la del Architect (`gpt-6.1-sol/high`) solo tiene la medición 0/6 de la identidad anterior, ya STALE. Un subagente de A2 comparte la sesión del Worker, y la independencia frente a WORKER es REQUIRED |
| **B. Sonda 2 retenida, para una medición distinta y acotada** | Disposición ordinaria del Coordinator que cambie su destino (A2-P2 ata el bloque al par; `BLOQUE-CODEX-CONSUMO` no nombra celda; §71, punto 1). Sin línea nueva del Owner | 1 invocación read-only del par `d13914ce…/26.1007.2314.0`; caduca si llega otra actualización | **Omisiones:** quitar «Tiempo limitado»; exigir ejecutar todos los pasos antes de responder; reducir la respuesta a lo representativo (sin agrupar los 8 controles negativos, que en la ejecución real son invocaciones aparte). **UTF-8 nativo:** medir si la celda puede fijar la salida UTF-8 del `pwsh` del sandbox; el resultado diría si la receta necesita un cambio | No puede demostrar PLAN y VERIFY por separado: si se acota a una acción, la otra queda sin medir. Una medición conjunta y aligerada mediría las dos a la vez. No resuelve MAP_INVALID | **La única opción ejecutable sin enmienda.** Medir PLAN y VERIFY juntas, sin los negativos agregados, sin presión de tiempo y con la comprobación de UTF-8 como paso explícito |
| **C. Cambio limitado de transporte, modelo o medición** | Según lo que cambie: el diseño de la medición, por disposición ordinaria (es B); la receta del rol (por ejemplo, fijar la salida UTF-8 o declarar `cmd.exe` en la vía normal), por una **A-n material**, porque A4-1 solo abre `cmd.exe` con la firma de F6-OBS-03, ya resuelta; otra celda, solo si `routing.md` la hace elegible y con su propia medición | Sí | UTF-8: con `cmd.exe` el transporte es correcto, así que una A-n que admitiera `cmd.exe` en la vía normal quitaría la deformación del transporte; o una A-n que fije la salida UTF-8 en el texto del rol | Los escapes mal formados del modelo con `cmd.exe` no los resuelve el transporte. Del cambio de celda no hay evidencia | Solo si B demuestra que el modelo no puede fijar UTF-8 dentro de la receta actual. Hoy no hay evidencia suficiente para proponer una A-6 |

## 4. Necesidad real de una enmienda nueva

- **Para las omisiones:** no. Es un asunto del diseño de la medición, que se dispone para la sonda 2.
- **Para UTF-8 en la vía nativa:** solo si una medición muestra que la celda no puede fijar la salida UTF-8 dentro de la receta actual. Entonces haría
  falta cambiar la receta o el texto del rol, que es material. Hoy no consta.
- **Para el fixture:** A-5, ya en revisión.
