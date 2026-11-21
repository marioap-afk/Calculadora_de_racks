# Disposición para la verificación de F4B-PBT4-FULL-DRAFT-PANEL: CT-PBT4B-20, comprobación (5)

**Autoridad:** la decisión del Owner del 2026-11-21, «Ratificar como errata (Recomendado)». Es la pregunta 1, con su respuesta literal, del registro `owner-pbt4b-code-decisions.md`, que la sesión custodia con el cierre de PB-T4b. La pregunta se hizo después de la revisión independiente del código (I-01) y antes de esta verificación.

**El punto declarado:** la entrega del Worker `R20261121T085458Z-5ef5` lo declara en `Deviations[1]` y en `UnexpectedFindings[4]`.
- El §5.2 del paquete v4.9, fila 20, comprobación (5), dice: «Ninguna copia nombra `EditIntentFakeReader`, `EditIntentKit`, `PrepareKit`, `PushBackPilotDraftState`, `PushBackPilotStateComparer`, `PushBackPilotFieldAdapter` ni `PushBackFullReadRig` como palabra completa».
- `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs`, en :630-632 de `c065080d`, evalúa esa comprobación solo sobre las nueve clases de C-0.
- La línea esperada `sin-nombres-del-kit-del-piloto=True` no cambia.

**Lo que ratifica el Owner, y nada más:**
1. La comprobación (5) acotada a las nueve clases de C-0 se acepta como errata del paquete, solo para este intento.
2. **El motivo:** tal cual, (5) da `False`. Hay cinco apariciones en literales de cadena de dos copias de guarda cuyo texto fija el contrato por `CopySha`: `PushBackWritePathGuardFull.cs` y `WorkspacePilotEditGuardFull.cs`.
3. **Lo que no se cambia:** ningún archivo de `src/` ni de `tests/`.

**Lo que no cambia:**
- Lo demás de CT-PBT4B-20 lo compruebas tú mismo, como siempre: sus comprobaciones (1) a (4), (6) y (7), sus líneas esperadas y su resultado en el RED y en el CurrentSha.
- **Que la acotación sea solo esa:** la expresión de (5) es la del §5.2, con sus siete nombres, y se aplica a las nueve clases de C-0.
- **Cualquier otra desviación de la entrega respecto del contrato** se juzga con su letra. Esta disposición no la cubre.
- Cita en tu Evidence lo que midas, y nombra esta disposición en la comprobación a la que afecte.
- Esta disposición no relaja ninguna otra comprobación.
