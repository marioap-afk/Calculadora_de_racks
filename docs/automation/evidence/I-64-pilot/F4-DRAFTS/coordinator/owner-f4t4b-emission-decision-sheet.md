# I-64 F4-T4B v7: hoja de decisión para la emisión

Ya cumplido: T4A2 y T1B cerradas; la v4 revalidada contra `7c29a5e5`; las comprobaciones independientes de la v4 (1/4/13), la v5 (0/2/6) y la v6 (EMITIBLE, 0/0/3), todas aplicadas; y la validación determinista de la v7, PASS 99/99.

**Ya decidido por ti:** I-1 (T4B = escritura, T4C = contexto); I-4 (la vía propia del piloto, con Smoke-2 como primera validación física); I-5 en la purga (solo registro, categoría (b) probada); e I-6 (el movimiento de `Traverse` con guardas exactas en T4B).

**Queda por decidir:** las 12 interpretaciones de la sesión de esta tabla. La columna central es la propuesta del paquete, y la de la derecha, la alternativa.

| # | Propuesta del paquete | Alternativa |
|---|---|---|
| I-2 | `TryUpdatePilot` es **un método del núcleo**, sin llamador de producto en T4B, para que la ranura sea oráculo de T4B. La operación del puerto va en T4C. **T4C necesitará un cambio de guarda declarado para cablear la instancia del puerto** (§8, RR R-M-12) | La operación en el puerto ya en T4B, con los cinco implementadores y UI.Tests en el alcance |
| I-3 | El diagnóstico de `TryCheckNow` (`A2P:657`, F4-E55) **pasa a T4C**. Toca el VM y rompe M-3 o M-2. Esto **añade «Comprobar ahora» a T4C**, más allá de su fila (`T4P:173`) (IR M-13) | Mantenerlo en T4B, añadiendo la UI al alcance |
| I-5 | **«Aplicado con avisos»:** las piezas faltantes (por unidad), un nombre distinto tras `SyncName` y un `Regen` que lanza dan avisos visibles, y los decide la corrida a partir de los hechos de `Post`. **Los fallos de la purga solo van al registro**, porque la purga es categoría (b) con su orden (§2.4, INV-16) | Tocar `LateralHeaderDrawer` para que informe, fuera del alcance |
| I-7 | **`TopTransaction`:** se rechaza una ajena al entrar (`TOP_TRANSACTION_OPEN`) y, al salir, se comprueba registrando en el log, sin `Debug.Assert` | `Debug.Assert`, que terminaría AutoCAD |
| I-8 | **El `PushBackSystem`** se reconstruye con una ayuda pura de `Pilot/` a partir del texto de diseño del plan; el corte, con `RackViewCodec`. **Hay dos catálogos** (el de la lectura previa y el de `PrepareRedraw`), y se declaran | Ampliar el plan |
| I-9 | **El origen de MUTATE** lo elige **la corrida, en puro**, por el texto exacto del origen del borrador (la regla Build (b)). Cero da `NoOriginReason` y varios `SeveralOriginsReason`, los dos como Preflight. El documento se elige por `AuthorizedDocumentNativeId`. **El origen de POST es `plan.OriginKey`.** Es distinto de la elección de «Comprobar ahora», y R-T4-01 exige solo la equivalencia de la StaleBase. **Consecuencia** (RR R-M-11): la promesa de D-T4A2-06 (`A2P:349`) solo se cumple para las causas que no dependen del origen. Las reglas 2, 5 y 6 del lector pueden dejar un diagnóstico persistente en «Comprobar ahora» mientras Actualizar admite | Leer por el lector del host (D-1 6 → 7, `PilotForms` y el catálogo, que `EditIntentReadResult` no lleva) |
| I-10 | **El respaldo de `CompleteApply`:** si el cálculo del estado lanza en una rama NotApplied, el borrador queda en **Error** con su causa (`ClosedInError`), nunca en «aplicado» | Dejar la rama tal cual y que la excepción escape, que es lo de hoy y lo que F4-E25 (4) pide corregir |
| I-11 | **La puerta de `TryUpdatePilot`** (RR R-I-1, opción a): la propia, **sin reposo ni lector** (el documento activo, el modal y `Authorize`). **La puerta de reposo pasa a la operación del puerto de T4C**, antes del contexto de comando. Así T4C llama a `TryUpdatePilot` dentro del callback sin reescribir el oráculo de la ranura | (b) `PassGate` en T4B, con un cambio en T4C que reescribe el método |
| I-12 | **«`Seal` volvió y su `Dispose` lanzó»** va a POST, como la fila «`Commit()` retornó» de D-13 (`FRZ:527`), y el `Dispose` se convierte en un aviso. No va a CommitThrew, que daría un Conflicto falso con la escritura ya hecha (RR R-I-3) | CommitThrew |
| I-13 | **Las capas bloqueadas, en T4B**: antes de importar y en MUTATE, con `LOCKED_LAYER`, como pide el paso 3 de D-13 (RR R-I-4) | Pasarlas a T4C, que deja un residuo OM-5 evitable |
| I-14 | **«Varios orígenes»** da Preflight con `SeveralOriginsReason`, y no el NotAdmitted u OriginMissing engañoso del lector (RR R-I-5) | NotAdmitted |
| I-15 | **El orden de `Post`** es purga → `SyncName`, el del clásico y el de I-55. R-T4-04 (`T4P:540`) y `FRZ:519` lo enumeran al revés, pero `FRZ:519` remite a «la autoridad existente» y `FRZ:506` no congela la mecánica | `SyncName` → purga, como la letra de R-T4-04 |

Si se emite: contrato `F4-T4B-MUTATE-POST` 1/3, con la cadena completa de T1B (planificación, A1-A8, nc4, Worker, CI exacta, revisiones directa e independiente, verificación, nc1-nc3 y custodia).
