# FX-02 — N9 y N10: verificación de las mutaciones selladas y borrador de disposición (decisiones §66, punto 8)

> Supervisión, plano a. **Borrador; no es disposición.** No se modifica el archivo sellado (`negatives.md`, SHA-256 `25a33a4891ecc1e6c3f7aa008920241ed2aac6bd3fdd0c1cfeec2ca5c4780f6a`) y no se ejecuta nada.
> Ninguna disposición aprobó todavía las mutaciones de N9 y N10 (§63.4 solo aceptó N4, N5, N7a y N7b y mandó resellar N8). La orden O4 (punto 15)
> las deja para un commit del Coordinator del fixture antes del Q0. Los esperados (parte B) siguen solo en el archivo sellado y no se copian aquí.

## 1. Fuente y semántica

| Control | Etiqueta de D.3 (V14 L2446) | Regla que ejerce | Fase |
|---|---|---|---|
| N9 | «N9 TERMINATION_UNACCREDITED → P-12» | V14 §9.1 (L598-L605): la terminación del titular de una sesión de Principal la observa otra sesión autorizada (`isRunning`) o la atesta el Owner; TERMINATION_UNACCREDITED «no autoriza tomar posesión»; P-12 (L778): STOP, sin toma de posesión | sin invocación; dentro de la ventana, antes del Q7, sobre copias (U-16 (3), texto congelado) |
| N10 | «N10 contadores tras cambio de proveedor → sin reinicio» | V14 I-P13 (L2198): los contadores «no decrecen y no se reinician por un cambio de proveedor, modelo, sesión, binding, Principal o etiqueta» | ídem |

## 2. Verificación de las mutaciones selladas (parte A)

- **N9** (fila N9 de la parte A del archivo sellado; no se reproduce aquí, como en el registro de N8):
  - Los objetos existen en el camino de FX-02: la designación del titular nuevo por T16 cita la terminación acreditada del titular anterior (U-28,
    decisiones §52), y el registro de relevo del Worker tiene la operación 7 (relay-record/v2).
  - **Falta exactitud:** el texto ofrece dos variantes («Alternativa: …»). Una mutación exacta necesita una sola.
- **N10** (fila N10 de la parte A del archivo sellado; no se reproduce aquí):
  - Los campos existen: `Executor.BindingRef` (`UnitId`, `Scope`, `TaskId`, `Role`, `BindingId`, `Sha256`, `Location`) en `delegation.v2`, y
    `CountersSnapshot` (`Attempts`, `CorrectionLaunches`, `BlockedReruns`, `RebaseRecoveries`, `Invocations`) en `binding.v1`.
  - **Falta exactitud** en dos parámetros: «una celda de **otro proveedor**» no nombra la celda, y la variante (ii) usa «p. ej.» para los
    contadores.

## 3. Propuesta de disposición (texto para el Coordinator)

- **N9: variante principal**, que ejerce P-12 en la toma de posesión del titular (T16), la regla exacta de D.3. La alternativa del Worker se descarta.
  Texto propuesto para la columna «Mutación exacta»: «copia de las entradas de la precondición de T16 de la designación del titular de la unidad
  (la designación de este Coordinator con `I62-PRINCIPAL-BINDING: … ACCEPTED`) **sin** la acreditación de la terminación del titular anterior: de la
  copia se retiran la observación `isRunning` y toda atestación del registro de terminación; después, la evaluación mecánica de esa precondición
  sobre la copia, registrada en la ruta del control».
- **N10: los dos parámetros fijados.** Texto propuesto: «copia de la aceptación de la delegación verificada cuyo `Executor.BindingRef` apunta a un
  **rebinding sintético del WORKER** (registro copia, nunca aceptado ni invocado) a la celda `codex-cli:gpt-6-luna` (effort high; proveedor OpenAI),
  con el mismo `Scope` y la misma `TaskId`, evaluada con A1'-A8' sin cortocircuito en dos variantes: (i) `CountersSnapshot` del rebinding = los
  contadores de la autoridad; (ii) `CountersSnapshot` con todos los contadores de la autoridad que no son cero (`Attempts`, `CorrectionLaunches`,
  `BlockedReruns`, `RebaseRecoveries` y `Invocations[].Launched`) puestos a cero».
- **Procedimiento tras la disposición** (como N8, §63.4): sustituir solo esas dos celdas de la parte A en `negatives.md`, comprobar la parte B frente
  a los textos nuevos, volver a sellar (registro de sellados) y publicar las dos mutaciones en el fixture en un commit del Coordinator antes del Q0,
  tras la búsqueda P-16.

## 4. Precondiciones (sin ejecución antes de acreditarlas)

| Control | Precondición | ¿Acreditable hoy? |
|---|---|---|
| N8 | binding aceptado del Controller de planificación (S16) | **no**: con A4-1 NOT_DEMONSTRATED, la celda del Controller no es elegible y ese binding no puede aceptarse |
| N9 | designación del titular por T16 con la terminación acreditada del anterior (S08) | no: A2 no se ha abierto (y no se pide) |
| N10 | delegación de la ventana VERIFIED (los negativos van sobre la verificación `EXECUTION_VERIFIED`) | **no**: sin Controller elegible no hay planificación ni verificación |

Mientras FX-02 siga UNVERIFIED por A4-1, ninguno de los tres puede ejecutarse. La disposición de N9 y N10 solo fija su texto exacto para cuando el
camino se reabra.
