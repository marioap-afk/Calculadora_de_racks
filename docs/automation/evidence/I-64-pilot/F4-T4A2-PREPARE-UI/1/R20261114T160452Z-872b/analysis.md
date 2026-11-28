# I-64 F4-T4A2-PREPARE-UI, intento 1: análisis de las revisiones y disposición de la corrección

**Objeto:**
- la entrega del intento 1: RED `1fe36cf9`, GREEN `9333396d`, sobre BaseSha `4eb7e924`;
- la verificación `R20261114T160452Z-872b`: EXECUTION_VERIFIED 14/14, con el RED acreditado (`ChainRedFiles`, 10 rutas de `tests/`).

## 1. Revisiones

- **La directa de la sesión** (`direct-review.md`): el producto cumple el contrato, sin bloqueantes ni importantes; 3 menores (D-1 a D-3).
- **La adversarial independiente** (`independent-review.md`): **NO ACEPTABLE (0/1/9)**.
  - El producto (VM, XAML, código subyacente y el cambio exacto del host) es correcto frente al paquete v3 y al contrato.
  - Lo único que impide aceptar es **I-1**, en una prueba nueva de esta tarea.

## 2. Clasificación

| Id | Clase | Contenido | Tratamiento en el intento 2 |
|---|---|---|---|
| I-1 | **Importante**, guarda | La prueba positiva de la regla 5 (`WorkspacePilotReconciliationGuardTests.cs:461-462`) fija exactamente una llamada a `_authorize` dentro de `PilotIntentReader` y exactamente 3 en el archivo. Contradice el «sin exigir unicidad» de INV-F4T4A2-12 y congela la lectura nueva de F4-T4B en esa clase | Sin cuentas exactas: al menos una llamada dentro de la clase y al menos una fuera (INV-12) |
| M-1 | Producto | Si la relectura de memoria (`GetField` de `Render`) lanza tras un Confirmed, sale A2-01 calculado con la vista anterior: un falso limpio defensivo, porque el puerto declara que `GetField` no lanza | Un resultado que no se pudo releer no se presenta; queda el Error del campo. También en la comprobación de `TryCheckNow` y en la elección. Con su prueba, que falla contra `9333396d` (INV-04) |
| M-2 | Declaración | Dos desviaciones de producto sin declarar: la rama A2-34 de `CheckNow` y `ReconcileText` vacío con un fallo | Se declaran y la primera gana su prueba (INV-03) |
| M-3 | Declaración y prueba | Tras un descarte del navegador, el `Rematerialize` del navegador retira la elección en silencio; el A2-34 del paquete no llega a verse | Se declara; la prueba fija el `NoticeText` real (INV-05) |
| M-4 | Guarda | La regla 1 cuenta el token y no la invocación: un grupo de métodos dentro del cuerpo correcto la evade | La forma de invocación `_port.TryX(request)` y su autoprueba (INV-02) |
| M-5 | Guarda | Autopruebas de la regla 4 que ejercen otra rama o concatenan en vez de un solo `Replace`; la carpeta de la regla 4 fija un solo `.xaml`; la regla 3 no fija el orden del texto de la elección | Corregidas (INV-04, INV-05) |
| M-6 | Prueba | «No» no se prueba en la elección de Unknown con PostReread (prueba 12 de la orden) | Añadida (INV-05) |
| M-7 | Prueba | Una prueba de retirada que pasaba con el RED inerte; la del corpus, tautológica; un nombre que promete la marca de obsolescencia sin comprobarla; «salir de la caja» sin `LostFocus` | Corregidas (INV-05, INV-15) |
| M-8 | Prueba | `AssertHeld` compara el inicial y el actual solo por un campo | Por referencia (INV-13) |
| M-9 | Guarda | Una cuenta exacta no declarada en el núcleo (`pilot.Read.(IsAdmitted\|Admission)` = 2) | Se retira; M-3 del paquete se mantiene (INV-13) |
| D-1 a D-3 | — | Los menores de la revisión directa: ramas defensivas y un aviso más informativo de lo pedido | Se aceptan como están; D-2 coincide con M-2 y queda declarado |

**Nota para el paquete de F4-T4B** (de M-9): el §8 del paquete de F4-T4A2 asigna a T4B el diagnóstico de admisión en la vía `TryCheckNow`. Con M-3 (una aparición del patrón en todo el núcleo) y con las cuentas en cero de `CheckBody`, T4B tendrá que declarar esos cambios de guarda o factorizar la expresión. Junto con I-1, se prevé en el paquete de T4B.

## 3. Disposición

**Decisión:** corrección en el intento 2 de 3. La decidió de antemano el Owner para la noche (`F4-DRAFTS/coordinator/owner-f4t4a2-overnight-decisions.md`, pregunta 2: «Si el Worker se detiene por un hueco del paquete o la revisión da un importante, corrijo en el intento 2. Nunca gasto el intento 3 sin ti.»).

**El contrato del intento 2** (`gate-contract-attempt2.json`):
- parte del del intento 1;
- conserva el alcance de 15 rutas, la lista prohibida y las paradas;
- añade las correcciones a INV-02, 03, 04, 05, 12, 13 y 15, y un INV-18 sobre la mecánica de la corrección;
- **cambio declarado:** el filtro de Core pasa a `ExpectRed` false, porque en el RED de la corrección solo pueden fallar pruebas de UI (la de M-1); el de UI sigue con `ExpectRed` true.

**La cadena:**
- ChainBaseSha `4eb7e924`;
- ChainRedSha `1fe36cf9` (acreditado por `872b`);
- ChainRedFiles: las 10 rutas de `tests/` de ese RED;
- CorrectionOf = `{ RunId: "R20261114T160452Z-872b", FailureClass: "Tests", AnalysisSha256: <este archivo> }`. La clase Tests registra que una prueba de la tarea contradecía el contrato.
