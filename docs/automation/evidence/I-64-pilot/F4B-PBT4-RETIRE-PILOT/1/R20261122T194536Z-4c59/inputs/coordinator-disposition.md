# Disposición para la verificación de F4B-PBT4-RETIRE-PILOT: las autopruebas de archivo de CT-PBT4D-01 en el RED

**Autoridad:** la decisión del Owner del 2026-11-22, «Ratificar como límite del diseño (Recomendado)». Es la pregunta 1, con su respuesta literal, del registro `owner-pbt4d-code-decisions.md`, que la sesión custodia con el cierre de PB-T4d. La pregunta se hizo después de la revisión independiente del código (M-02) y antes de esta verificación.

**El punto:**
- **Lo que dice el contrato:** INV-PBT4D-02, en el RED: «las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado».
- **Lo que hace la prueba:** `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs`, en :111-115 de `aa65ecfd` y de `7db6fd0c`, es el texto literal de la pieza del RED del paquete. Sus dos autopruebas de archivo inyectan el texto de un archivo retirado; en el RED ese archivo todavía existe, así que la inyección no cambia el resultado y esas dos autopruebas no discriminan allí. Las otras 12 autopruebas de CT-PBT4D-01 y todas las de CT-PBT4D-02 discriminan en el RED; en el CurrentSha discriminan las 14 de CT-PBT4D-01.

**Lo que ratifica el Owner, y nada más:**
1. **El límite del diseño:** que esas dos autopruebas de archivo de CT-PBT4D-01 no discriminen en el RED se acepta como límite del diseño literal de la CT y errata de la letra de INV-PBT4D-02, solo para este intento.
2. **Sin cambios de código:** no se cambia ningún archivo de `src/` ni de `tests/`.

**Lo que no cambia:**
- **El criterio del RED de INV-PBT4D-02** lo compruebas tú mismo, como siempre: el filtro con ExpectRed (CT-PBT4D-01) da Failed por aserción, con la primera línea del StackTrace bajo `tests/` y Failed == Selected; los filtros con ExpectRed false pasan; las demás autopruebas de las dos CT discriminan; y en el CurrentSha discriminan todas.
- **Cualquier otra desviación** de la entrega respecto del contrato se juzga con su letra. Esta disposición no la cubre.
- **La evidencia:** cita en tu Evidence lo que midas, y nombra esta disposición en la comprobación a la que afecte.
- **El alcance:** esta disposición no relaja ninguna otra comprobación.

**Un recordatorio de la emisión, que no es una autoridad nueva** (F4B-E56, erratas, M-16, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` de BaseSha): en `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`, `RedFiles` de la precondición 6 es el RED de la v5.3 (`62c6ff80…`); el de la pieza v5.4 da `7ea6170422f48334f18ff2cd3b1cb1b32c79264861dd52a6e6b56570daf7ba13`, que solo difiere en comentarios. El contrato manda la pieza (INV-PBT4D-02), y `red-literal.json` lleva ese sha256: compara ese archivo contra la pieza, no contra `RedFiles`.
