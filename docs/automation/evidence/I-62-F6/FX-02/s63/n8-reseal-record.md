# FX-02 — re-sellado de N8 (decisiones §63.4)

> Supervisión, plano a. Registro **sin contenido**: no reproduce `negatives.md` ni la mutación de N8, que solo existen en el archivo sellado
> (supervisión; fuera del repositorio público hasta que su corrida termine). Nada se ejecutó contra el fixture, no se invocó ningún runtime y no se
> hizo ningún commit.

## 1. Origen

Decisiones §63, punto 4: «Para N8, preparar una mutación válida, comprobar su semántica y volver a sellarla antes de ejecutar. N8 no se clasifica
NOT_APPLICABLE sin demostrar la ausencia real de su precondición.» La entrada sellada anterior suponía una precondición (candidata sin credencial)
que dejó de existir en el plano real con OD-3 = A (A-4, Q-A4-08).

## 2. Hashes y tamaños

| Archivo | SHA-256 | Bytes |
|---|---|---|
| `negatives.md` anterior (sellado 63e84ba3) | `63e84ba3d7f06371e4c44a7c2cf671757ac34d61caf35bf56fed94524507b0e7` | 12 897 |
| archivado: `D:/r62-fixture/evidence-out/supervision/fx02/history/negatives.63e84ba3.md` | `63e84ba3d7f06371e4c44a7c2cf671757ac34d61caf35bf56fed94524507b0e7` (byte a byte igual al anterior) | 12 897 |
| `negatives.md` nuevo, `D:/r62-fixture/evidence-out/supervision/fx02/negatives.md` | `25a33a4891ecc1e6c3f7aa008920241ed2aac6bd3fdd0c1cfeec2ca5c4780f6a` | 16 424 |
| `supervision-checks.md` (sin cambio) | `729fc3ef34b0704d36fa0313b980554bbd5253c2cd64c96a95c3c4cd9522a0fd` | 22 954 |
| copia actualizada del registro de sellados (`sealed-supervision-files.json`, en esta carpeta) | `22653652eedbcb197b863d3335e1853d0ef40cd08b2676d147c36fc4807ed409` | 2 484 |

## 3. Alcance del cambio

- Solo la entrada N8: sus dos filas de tabla (parte A, procedimiento; parte B, esperado). Comparación línea a línea: mismo número de líneas, dos
  líneas distintas (41 y 62), el resto byte a byte igual; LF y UTF-8 conservados.
- En la fila de la parte A no cambian «Fase» ni «¿Invocación?» (sigue **sin invocación**, 0 en los topes de D.3).
- La columna copiable a la orden O4 («Mutación exacta») pasó la búsqueda literal P-16 de la plantilla de la orden (L15-L19): sin unidades reales,
  RackCad, OD, referencias a decisiones, evidencia o Proposal, identificadores del staging ni ids de sesión; solo cita autoridades copiadas byte a
  byte en el fixture (blobs comprobados iguales a los de RackCad en `d5b454fd`).

## 4. Comprobación de semántica (genérica)

Precondición de credencial ausente recreada sobre una copia; disposición esperada conforme a P-10.

| Cláusula | Resultado |
|---|---|
| V14 D.3 («N8 credencial → P-10», negativo de sesión sin invocación, fase «aceptación y estado») | conforme: sin invocación ni consumo; esperado P-10 |
| AP 16.16 (P-10: «no se vincula ni se invoca; no consume `attempts`») | conforme |
| AP 16.18-16.20 | conforme |
| RAE §12-§14 | conforme |
| esquemas de hechos del adapter y `preflight.v1` | conforme (la copia sigue siendo válida) |
| §63.4, «no NOT_APPLICABLE sin demostrar la ausencia» | conforme: la precondición se recrea; N8 sigue aplicable |
| U-16 (4) (dec. §62: control sobre copias, disposición confinada al control) | conforme |
| otras disposiciones o STOP distintos de P-10 | ninguno aplica (descartes, uno por uno, en la entrada sellada) |

**Simulación mecánica** sobre un preflight base sintético (stand-in, no evidencia): fases 1 y 2 con `Test-Json` (PowerShell 7.6.6) = `True` antes y
después de la mutación; reglas C5-C8 = conformes; resultado de la copia conforme a P-10. Los scripts temporales de la simulación y del re-sellado se
borraron tras usarse, para que la entrada sellada sea la única copia del contenido.

## 5. Pendiente fuera de este re-sellado (no tocado)

- **Colocación de N8-N10** (U-16 (3), «cubierta por el texto congelado»: dentro de la ventana, antes del Q7): la fila N8 conserva su «Fase» y el
  párrafo común de `negatives.md` sigue con la colocación del staging (después del Q7). La entrada nueva no depende de la colocación. Alinear el texto
  común exige otro re-sellado, fuera del mandato «solo N8».
- **Párrafo «Confinamiento de N8 y N9 (pendiente de CD-11)»** de `negatives.md`: U-16 (4) ya lo dispuso (dec. §62); el párrafo no se tocó.
- **Kit de FX-02 (repositorio):** `README.md` L7 cita el sello anterior `63e84ba3…` (actualizar a `25a33a48…`, 16 424 bytes) y L167 resume N8 como
  «candidata sin credencial (`claude-cli`, sin invocar)», que ya no describe la mutación (sustituir por un resumen genérico, sin esperados).
- **A-4 Q-A4-08** (aplicación, no delta) queda atendida por este re-sellado; A-4 no se modifica (objeto en revisión del Architect, §62.4).
- **Registro de sellados:** la copia de esta carpeta sustituye al registro del repositorio (`docs/automation/evidence/I-62-F6/kits/sealed-supervision-files.json`,
  blob `9ee83072…`) cuando el Principal la publique: entrada `Sealed` de `negatives.md` con el sello nuevo y la anterior en `Superseded`.
