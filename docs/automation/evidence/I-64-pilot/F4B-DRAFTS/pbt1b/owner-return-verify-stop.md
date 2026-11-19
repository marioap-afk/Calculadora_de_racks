# I-64 F4B PB-T1b, intento 2: vuelta al Owner por la parada de la verificación (Authority, AUTOMATION_PLAN 16.3)

**Fecha:** 2026-11-19. **Verificación del Controller:** `R20261119T014757Z-c78a`. Resultado: EXECUTION_BLOCKED / STOP, FailureClass `Authority`, sobre el CurrentSha `a18ba345` (RED `126fd65f`, BaseSha `2f5d3dec`).

## Los hechos

- **12 de las 14 comprobaciones pasan:** Termination, Handoff, Contract, Identity, Remote, CleanTree, Ci, Tests, Trailer, Routing, FreeText y Denials. En particular:
  - **Tests:** cada método con ExpectRed falla en el RED por aserción y pasa en el GREEN; el SHA-256 del análisis coincide con CorrectionOf; los blobs del RED son el esqueleto de `2d9febc6`; el diff de `src/` entre la base y el GREEN está vacío.
  - **Ci:** el GREEN da 4/4. El RED da Core y UI en failure, y `build-plugin` omitido, que ampara la disposición M14.
- **Authority (fail, STOP).** El texto de 16.3 dice que `git diff --name-only AuthorityRevision BaseSha` no puede contener rutas con secciones UNIT_CHANGE. El diff `5e52aaa3..2f5d3dec` contiene ES (`PushBackEditorState.cs`) y RMES (`RackModuleEditSession.cs`), que el contrato declara UNIT_CHANGE.
  - Solo los cambiaron los dos commits del Worker del intento 1 de esta misma cadena (RED `2d9febc6` y GREEN `a0a5f699`), dentro del AllowedWriteScope. Ningún commit de la sesión los toca.
  - **Causa:** para la corrección la sesión no reemitió el contrato. Además, 16.3 no prevé una base de corrección que ya contiene el GREEN anterior sobre rutas UNIT_CHANGE. Por la letra de 16.3, ni reemitiendo se podría elegir una AuthorityRevision posterior sin esas rutas. Por eso se repetiría en PB-T1c, donde W y WC serán UNIT_CHANGE, y en las demás correcciones de F4B.
  - La sesión lo declaró al Controller antes de verificar, como riesgo.
- **Scope (fail): es un error de la sesión, no de la entrega.**
  - La comprobación de 16.9 (`BaseSha..CurrentSha` contra AllowedWriteScope) pasa: 5 rutas y todas permitidas; las 11 del RED, también.
  - Falla un control EXTRA que la sesión añadió al prompt: «`ChainBaseSha..CurrentSha` dentro del AllowedWriteScope». Ese rango incluye los commits de documentación de la sesión, que 16.2 permite («commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad»).
  - En una nueva verificación, ese control se limita a `src/` y `tests/`.

## La pregunta

¿Cómo se trata la parada de Authority?

- **(A) Ratificar como regla de F4B y verificar de nuevo (Recomendado).**
  - **La regla:** en una corrección de F4B, una ruta UNIT_CHANGE que cambia entre la AuthorityRevision y la BaseSha SOLO por commits del Worker de la misma cadena, dentro del AllowedWriteScope, no incumple 16.3. El Controller lo comprueba commit a commit.
  - **Para PB-T1b:** una verificación nueva del intento 2 con esa disposición y con el control de cadena corregido.
  - **Lo que no cambia:** no gasta intento, no se reescribe nada de la entrega y no cambia el producto.
  - **Lo que evita:** la misma parada en PB-T1c y en las demás correcciones de F4B.
- **(B) Ratificar solo para PB-T1b y verificar de nuevo.** Igual que (A), pero solo para este caso. Una corrección futura con el mismo patrón volvería al Owner.
- **(C) Reemitir el contrato y repetir la corrección.** Sería el intento 3, que vuelve siempre al Owner (D-PB-10). No resuelve la regla: la nueva base también tendría ES y RMES cambiados desde la AuthorityRevision. Es la opción más cara.

## Lo que no decide esta pregunta

- No cambia AUTOMATION_PLAN fuera de F4B: es una disposición de la unidad, como F4B-E21.
- No cambia el producto ni las pruebas de PB-T1b.
- No toca el otro hecho declarado al Controller: el RED `2d9febc6` marcado como ChainRedSha sin que lo acreditara una verificación (16.8). La comprobación Tests pasó con él.
