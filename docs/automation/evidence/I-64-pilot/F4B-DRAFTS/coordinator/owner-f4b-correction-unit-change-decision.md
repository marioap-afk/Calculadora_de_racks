# I-64 F4B — Decisión del Owner: rutas UNIT_CHANGE en la base de una corrección (AUTOMATION_PLAN 16.3), regla de F4B

**Fecha:** 2026-11-19. **Canal:** chat de la sesión principal, pregunta de opciones; respuesta literal. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md`. **Origen:** la verificación del Controller `R20261119T014757Z-c78a` del intento 2 de PB-T1b. Dio EXECUTION_BLOCKED / STOP, con FailureClass Authority; las otras 12 comprobaciones pasaron y Scope falló por un control extra mal planteado por la sesión.

**Texto, literal:** «PB-T1b, intento 2: la verificación del Controller pasa 12 de 14 comprobaciones (pruebas, CI, alcance de la entrega, trailer…), pero PARA en Authority. Su regla (AUTOMATION_PLAN 16.3) prohíbe que entre la revisión de autoridad y la base haya cambios en rutas declaradas UNIT_CHANGE. Aquí los hay en dos archivos de producto, ES y RMES. Solo los cambió el GREEN del intento 1 de esta misma cadena, porque al corregir no reemití el contrato. La regla no prevé este caso, y se repetiría en PB-T1c y en las demás correcciones. (El otro fallo, Scope, es un control extra mío mal planteado; lo corrijo.) ¿Cómo se trata?»

**Opciones ofrecidas:**
- «Regla F4B y reverificar (Recomendado)»: en una corrección de F4B, una ruta UNIT_CHANGE cambiada solo por commits del Worker de la misma cadena, dentro de su alcance, no incumple 16.3; el Controller lo comprueba commit a commit. PB-T1b se verifica de nuevo, sin gastar intento ni cambiar nada.
- «Solo PB-T1b y reverificar».
- «Reemitir y repetir (intento 3)».

**Respuesta del Owner:** «Regla F4B y reverificar (Recomendado)».

## Lo que implica

**La regla, para todas las correcciones de F4B (disposición de la unidad, como F4B-E21).** Una ruta con secciones UNIT_CHANGE que aparece en `git diff --name-only AuthorityRevision BaseSha` de una delegación de corrección **no incumple 16.3** si se cumplen tres condiciones, y el Controller comprueba cada una con git:
1. todo commit de `AuthorityRevision..BaseSha` que la toca es un RedSha o un CurrentSha de una entrega de un Worker de la misma cadena (el mismo TaskId), según su `worker-handoff.json`;
2. esa ruta está en el AllowedWriteScope de la delegación de esa entrega;
3. ningún commit de la sesión de `AuthorityRevision..BaseSha` toca esa ruta.

Las rutas EXTERNAL siguen prohibidas en ese diff sin excepción. Todo lo demás de 16.3 sigue igual.

**Para PB-T1b:**
- una verificación nueva del intento 2 (`a18ba345`) con esta disposición, sin gastar intento ni cambiar la entrega;
- el control de cadena del prompt de la sesión se corrige: `ChainBaseSha..CurrentSha` solo se mira en `src/` y `tests/`, porque en ese rango están también los commits de documentación de la sesión, que 16.2 permite;
- la verificación parada `c78a` se custodia con las demás unidades.

**Para PB-T1c y las tareas siguientes:** la v2.8 del paquete de PB-T1 recoge la regla en su §6.0 (V28-04, los hechos de procedimiento) y sus contratos la citan como autoridad.
