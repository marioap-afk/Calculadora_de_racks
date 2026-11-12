# F4-T2B2A-INTENT-WIRING, intento 1: análisis de las revisiones y corrección

**Entrega:** Worker `R20261112T143337Z-4331`, RED `db37e30d`, GREEN `2d743e46`; CI `43247864348` 4/4 (RED `43243624408`: Core en rojo con 80 fallos, todos de Workspace).
**Verificación del Controller** `R20261112T155354Z-46e1`: `EXECUTION_VERIFIED`, 14/14. El RED queda acreditado, con ChainRedFiles = las 5 rutas de prueba de `e277dd42..db37e30d`.

## Revisión directa de la sesión

El producto no tiene defectos de corrección:
- El core sigue los pasos 1 a 8 de D-T2B2-06, sin lectura antes del 6. El mapeo de estados coincide con la tabla. La contención da Failed sin degradar la sesión, y un `PilotRead` nulo se trata como fallo.
- El lector anidado sigue el orden de D-T2B2-03 e INV-22 (el id nativo de `UnmanagedObject`, como el precedente de navegación).
- El adaptador cumple D-T2B2-02.
- La proyección guarda el mismo `json` que lee `Traverse`.
- Los cuatro arrastres de T2B1 son correctos.

Observaciones:
- `default(EditCheckOutcome).Differences` vale null.
- En `EditIntentReadResult`, el cero del enum es `Read`; el host lo defiende.
- La suite completa de UI da 2 fallos en Release (`PendingTextFieldTests`, que dependen de `#if DEBUG`): condición previa conocida, ya medida en F4-T2A (`dd72`); en Debug, que es lo que corre la CI, son 0.

## Revisión adversarial independiente

**Veredicto:** NO ACEPTABLE (0 bloqueantes, 1 importante, 8 menores). El producto no tiene defectos de corrección. El informe completo está en `independent-review.md`, junto a este archivo.

| Id | Severidad | Hallazgo | Tipo |
|---|---|---|---|
| I-1 | IMPORTANTE | Varias pruebas y guardas nuevas prohíben para siempre lo que el paquete v5 asigna a F4-T2B2B (detalle abajo). T2B2B tendría que cambiarlas, y no están en su alcance (§3 y §6): su parada se activaría con certeza. La guarda de la obligación 10 no es la contratada: INV-08 dice «Begin, CheckNow o DiscardAndReload». Lo mismo ocurrirá en F4-T3 con «`Try(Begin\|Check)PilotEdit(` solo en el core» | Pruebas |
| M-2 | MENOR | La guarda de la obligación 10 no mira el lado del Plugin (adaptador y host de navegación), que es quien maneja el resultado del piloto | Pruebas |
| M-3 | MENOR | Un testigo con clave nula acaba en `NotSupplied` y uno con clave en blanco, en un fallo tipado. Además, el fallo se detecta tras `LoadCatalog`, el bloqueo, la autorización y `Revalidate`: una petición inválida gasta una lectura | Producto |
| M-4 | MENOR | La comprobación Debug de `TopTransaction` «después» solo corre en el camino de éxito de `Read`, y `ReadPreviewWitness` no tiene ninguna (INV-14) | Producto |
| M-5 | MENOR | El constructor `internal` de `RackSiblingDrawingScan` gana un parámetro opcional. INV-01 y D-T2B2-01 dicen que ninguna firma cambia, y la guarda se llama `NoSignatureOfTheScanChanged…`. El valor por defecto deja en silencio un diccionario vacío | Producto |
| M-6 | MENOR | Hay comentarios que afirman más de lo que comprueban: la cabecera de la guarda nueva («toda familia tiene prueba de inyección»), el comentario «SUBSET» del arrastre N-4 y la premisa no comprobada del par (9,11) | Pruebas |
| M-7 | MENOR | `default(EditCheckOutcome).Differences` vale null; el cero de `EditIntentReadStatus` es `Read` | Producto |
| M-8 | MENOR | El esqueleto del RED no era del todo inerte: los tipos de valor ya lanzaban y varias pruebas pasaban. Ninguna se presentaba como discriminante | Registro |
| M-9 | MENOR | El orden «compuerta antes de AlreadyOpen» de la intención solo lo fija el texto, sin prueba de comportamiento. El comentario de `Rejected` dice «sin lectura», pero la rama `SessionRetired` llega después de la lectura | Pruebas |

**Detalle de I-1.** Estas pruebas y guardas prohíben de forma permanente lo que corresponde a F4-T2B2B:
- `Assert.False(draft.IsPossiblyStale)` después de los drenajes;
- el texto `_drafts` en `OnDrained`;
- exactamente 2 `ReadIntent` en el core;
- `DiscardAndReload`;
- `IWorkspaceLifecycleAdvisor`, `AdviseClose`, `SessionDestroyed` y `EvaluateClose`;
- las palabras `Advisor` y `Draft` en `EventBridgeCore` y `AutoCadEventBridge`.

**Riesgos para Smoke-2 y T3** (no son defectos):
- tres niveles de `LockDocument` anidados;
- `Commit` de la transacción de lectura del barrido: verificar que DBMOD y UNDO no cambian;
- coste de dos recorridos de la BlockTable por operación;
- «Comprobar ahora» depende del catálogo;
- los fallos del adaptador no van a RackLog;
- `ReadPreviewWitness` no tiene compuerta propia;
- el core no se defiende de una llamada desde el drenaje;
- el origen solo cuenta referencias de Model Space.

## Disposición

La verificación 14/14 es necesaria pero no suficiente. La revisión independiente es obligatoria (F4-E02) y su veredicto impide cerrar.

El hallazgo I-1 bloquearía F4-T2B2B. Arreglarlo enmendando el alcance de T2B2B reabriría el paquete v5 ya ratificado, contra la orden del Owner (F4-E29). Por eso la sesión aplica la autorización de correcciones del contrato (`CorrectionsAuthorized`):
- **Intento de corrección 2** de F4-T2B2A (`attempts` 2 de 3), con un contrato del intento 2 que solo añade texto a los invariantes, añade INV-13 y añade la regla de replanificación de F4-E31.
- **RED de la corrección**, sobre el producto del GREEN `2d743e46`:
  - I-1, M-2, M-6 y M-9: pruebas y guardas re-acotadas o nuevas, que pueden pasar ya;
  - M-3, M-4, M-5 y M-7: pruebas o guardas que fallan por aserción contra el producto actual.
- **GREEN de la corrección:** solo producto (M-3, M-4, M-5, M-7 y el comentario de M-9); ningún archivo de `tests/`.
- **Regla de I-1:** ninguna prueba ni guarda nueva prohíbe de forma permanente lo que el paquete asigna a F4-T2B2B (D-T2B2-09 a D-T2B2-14) ni la exposición de las operaciones en F4-T3. La ausencia del ciclo de vida en esta tarea se acredita por el diff, el alcance y la revisión, no con una guarda permanente. Las guardas de esta tarea se limitan a sus propios nombres y a los cuerpos de sus dos operaciones.
- **M-8:** se registra; el RED de la corrección no necesita esqueleto, porque corre sobre el producto existente.
- **Riesgos de Smoke-2 y T3:** se arrastran a Smoke-2 y a F4-T3.
- **CorrectionOf** = esta verificación, con la clase `Tests` (el hallazgo importante está en las pruebas y guardas exigidas por el contrato). Este archivo es el análisis.
