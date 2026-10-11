# FX-02 — Causas del fallo del Controller y reutilización de la sonda 2 (decisiones §70, puntos 3 y 4)

> Supervisión, plano a. Análisis breve; no cambia ningún criterio normativo ni propone convertir resultados parciales en elegibilidad.

## 1. Causas, por categoría, en las dos mediciones

Clasificación mecánica de cada clave fallida de `comparison.json` (script en la evidencia de la supervisión): OMITIDA = valor vacío o «FALTA»;
UTF8 = texto no ASCII deformado o conteo de no ASCII; SANDBOX = `ls-remote` denegado; ERROR_MODELO = valor distinto del de referencia.

| Medición | Claves fallidas | OMITIDA | ERROR_MODELO | UTF8 | SANDBOX | Comandos |
|---|---|---|---|---|---|---|
| A4-1 (`cmd.exe`, identidad `73890CA3`) | 68 | 59 | 3 | 4 | 2 | 11 |
| A2-P2 sonda 1 (`pwsh` nativo, `F909F480`) | 94 | 83 | 6 | 3 | 2 | 4 |

- **Operaciones omitidas voluntariamente (≈ 88 % en las dos):** la celda `gpt-6-luna/high` para tras pocos comandos (11 y 4), con el 80-83 % del tope
  sin usar. No intenta la cadena de AP 16.13 ni los negativos y, en la sonda 1, tampoco el TRX ni la entrega. Ningún error de transporte la detuvo: es
  la conducta de la celda ante una tarea larga. Es la causa dominante, idéntica con las dos shells.
- **Errores del modelo en Git y hashes (3 y 6):** diffs con otra base (`diff-red-head`, `rt`), ascendencia de AR (`anc:ar-base`), blobs en AR
  (`blob-at-ar`) y el conteo de `ForbiddenWriteScope` (13 y 14 frente a 12). Son pocos y de lectura, no de capacidad del binario.
- **Fidelidad UTF-8 del transporte (4 y 3):** el texto no ASCII llega deformado con `type` bajo `cmd.exe` y también con el `pwsh` del runtime (la salida
  de la consola no va en UTF-8). Es una propiedad del transporte del adapter en Windows, no del modelo. **Otra medición no la resuelve por sí sola:** en
  la receta real, el texto del rol está fijado, y fijar la codificación de salida sería un cambio de la receta o del texto del rol.
- **Restricciones reales del sandbox (2 y 2):** `git ls-remote` contra el origen local falla en las dos versiones («Could not read from remote
  repository»). La regresión de la shell (F6-OBS-03) sí desapareció con el binario nuevo.
- **Requisitos que podrían no ser obligatorios o estar agregados** (para disposición del Coordinator; el kit no los cambia):
  - **`ls-remote` por el Controller:** en VERIFY, `Remote` se comprueba «En el registro» con la señal «sesión → Controller» (AUTOMATION_PLAN, tabla de
    VERIFY, fila 6). El Principal registra el `ls-remote` y el Controller lo compara, así que en la ruta normal la restricción del sandbox no bloquea
    VERIFY. (A4-1, regla 2, sí nombraba «las lecturas de Git … de `Remote`» para la ruta `cmd.exe`.)
  - **Agregación:** D-4 (§65) pidió una sola medición para PLAN, VERIFY y los negativos. En la ejecución real, PLAN y VERIFY son invocaciones
    distintas, y cada control negativo es su propia invocación de VERIFY sobre una copia (README de agent-execution §10). La sonda exige en una sola
    corrida más de lo que hace ninguna invocación real: es un criterio de medición de D-4, no un requisito de PLAN ni de VERIFY por separado.

## 2. ¿Puede reutilizarse la sonda 2 para el Controller?

- **Texto de A2-P2 (A-2, §3, regla 3):** el bloque se autoriza «para el par exacto (`BinaryHash`, `AppVersion`)»; «una disposición del Coordinator nombra
  ese par, y el Owner autoriza el consumo». La regla **no ata las sondas a celdas, roles ni modos**, y la línea del Owner `BLOQUE-CODEX-CONSUMO` dice
  «≤ 2 invocaciones read-only de codex-cli para el par exacto», sin celda.
- **Conclusión:** reasignar la sonda 2 al Controller es posible con una **disposición ordinaria** del Coordinator. No exige enmienda ni otra línea del
  Owner, siempre que siga siendo una sola invocación read-only del mismo par, dentro del tope 2 del bloque, comprobada por P-07 y antes de cualquier
  actualización nueva (regla 6: las sondas no usadas caducan). La asignación a NATIVE_ARCHITECT era de la disposición de §69, que puede cambiar otra
  disposición.
- **Límites:**
  - §69.5 y §70.2 prohíben repetir la sonda 1, así que una segunda medición del Controller tendría que ser distinta (otro alcance u otro diseño), no
    una reejecución;
  - **una sola sonda no puede demostrar PLAN y VERIFY por separado**, y la medición conjunta ya falló por omisión. Con una sonda nueva, una de las dos
    acciones quedaría sin demostrar;
  - no resuelve la fidelidad UTF-8 del transporte.
- **Qué podría resolver otra medición:** solo la omisión y, quizá, los errores de lectura, con una tarea más acotada (una acción, con instrucciones
  explícitas de completar). No resuelve UTF-8 (receta) ni MAP_INVALID (fixture), y `ls-remote` no lo necesita VERIFY.
