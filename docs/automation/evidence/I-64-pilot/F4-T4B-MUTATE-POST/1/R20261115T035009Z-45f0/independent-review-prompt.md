# Encargo: revisión adversarial independiente del código de I-64 F4-T4B-MUTATE-POST (intento 1)

Eres un revisor independiente y adversarial. Tu trabajo es encontrar defectos reales en una entrega que dice cumplir su contrato: intenta refutar cada afirmación de la entrega contra el código y los artefactos. No es tu trabajo confirmar.

## Dónde está todo

- **Worktree** (solo lectura): `<worktree-de-I-64>`, en la rama `architecture/workspace-persistente-rackcad`.
- **Commits:**
  - base `3633f08d1082a8a049bca1ac24432900919253d3` (ChainBaseSha = BaseSha). Su producto es el de `c3ae1c97`, que es la base medida del paquete;
  - RED `f4476bc4c38f32331d963ad74fc238434b9bd63c`: el esqueleto inerte de los archivos nuevos y las pruebas nuevas y cambiadas;
  - GREEN `4314c1803b7a047f0521c34d631950a475e7cd32`: la implementación.
- **El contrato que manda:** `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json`. Tiene 18 invariantes `INV-F4T4B-01..18`, 26 paradas y 24 RequiredTests.
- **El paquete del contrato**, que es la fuente del diseño: `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`. Lo esencial es el §2 (hechos medidos y trampas), D-T4B-02 (el puerto, la corrida y la tabla de frontera con su precedencia), D-T4B-03 (el escritor y el orden de Post), D-T4B-04 (el movimiento de `Traverse` y sus guardas (1) a (8)), D-T4B-05, D-T4B-06 (`CompleteApply` por rama), D-T4B-07 (`TryUpdatePilot`), D-T4B-08 y D-T4B-09 (las guardas).
- **La autorización del Owner**, con sus órdenes de contención: `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4b-emission-authorization.md`. Las disposiciones previas (F4-E59 y F4-E60) están en `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md`.
- **La entrega del Worker:** `artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012820Z-7a4a/worker-handoff.json`. Mira Evidence, WorkCompleted (incluye las «decisiones de implementación conservadoras»), UnexpectedFindings, KnownLimitations y Deviations.
- **Los artefactos locales** de esa carpeta (ignorados por Git):
  - `tests/<RunRef>/` con los TRX de cada corrida local;
  - `ci-core/core.trx` y `ci-ui/ui.trx` (la CI del GREEN, run `43668817337`);
  - `red-ci-core/core.trx` y `red-ci-ui/ui.trx` (la CI del RED, run `43667312134`);
  - `green-src.patch`.
- **Los archivos cambiados:** `git diff --name-status 3633f08d 4314c180`, 21 archivos.

## Reglas de esta revisión

- **Solo lectura:**
  - no compiles;
  - no ejecutes pruebas: la suite de UI existente escribe en el perfil del usuario;
  - no modifiques ni crees nada en el worktree;
  - no hagas ninguna operación de Git que escriba (commit, checkout, stash, restore, reset, fetch, push, add…).
- **Se permite:**
  - leer archivos;
  - usar `git show`, `git diff`, `git log`, `git cat-file`, `git rev-parse` y `git grep` con `git -C <worktree>`;
  - escribir scripts de comprobación de solo lectura en la carpeta del informe (abajo) y ejecutarlos con Python.
- **No escribas en el informe** ninguna ruta absoluta local, nombre de usuario ni nombre de máquina: solo rutas relativas al repositorio. El repositorio es público.
- **Escribe el informe** en español, en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\t4b\independent-review-code.md`. Fuera de esa carpeta no puedes crear nada.

## Qué tienes que intentar refutar (como mínimo)

1. **La corrida y la tabla de frontera (INV-01..06, D-T4B-02, D-T4B-05).** Recorre `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs` fila por fila contra la tabla del punto 4 de D-T4B-02, su precedencia (6) y sus dos salvedades.
   - ¿Hay algún camino en el que una excepción, un valor nulo o un `Dispose` que lanza acabe en una clasificación distinta de la del paquete?
   - En particular: un resultado que diga «no aplicado» cuando ya pudo haber escritura, o «aplicado» sin la relectura verificada.
   - ¿La corrida puede lanzar?
   - ¿La relectura de MUTATE es de verdad la autoridad: elige otra vez el origen, compara la StaleBase y el destino, y da cero `WriteUnit` ante una diferencia?
   - ¿POST lee con `plan.OriginKey` y compara sin normalizar?
   - Pondera las decisiones de implementación que declara WorkCompleted y que la tabla no fija:
     - una importación con `Ok = false` da NotAppliedPreflight;
     - un fallo del cálculo de los nombres deseados es un aviso y POST sigue.
2. **El escritor (D-T4B-03, INV-04, INV-07, INV-12, INV-14, INV-16)**, en `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`.
   - ¿Solo hace operaciones primitivas y devuelve hechos?
   - ¿El orden de Post es el del paquete?
   - ¿La purga corre una vez, con la unión, fuera del ámbito de mutación y con TopTransaction nula?
   - ¿La lectura de nombres es de verdad de solo lectura?
   - ¿Hay algún objeto de AutoCAD fuera de los ámbitos?
   - ¿`WriteUnit` comprueba el texto del plan?
   - ¿El mapeo de vista, extremo, lado y poste de `PrepareRedraw` y `RedrawInTransaction` es el del camino clásico (`src/RackCad.Plugin/RackPushBackCommands.cs`)?
   - ¿Hay algún camino en el que la transacción poseída quede abierta, se confirme dos veces o se confirme sin `Seal`?
3. **El movimiento de `Traverse` (INV-17, D-T4B-04)**, en `src/RackCad.Plugin/Views/RackSiblingScan.cs`.
   - ¿Es una refactorización sin cambio de comportamiento?
   - ¿El literal embebido en `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs` (`TraverseBaseline`) es de verdad `c3ae1c97:src/RackCad.Plugin/Views/RackSiblingScan.cs:103-110`, `:115-161` y `:166-169`, con la única sustitución, y `RestLiteral` el resto sin `:98-170`? Compruébalo tú contra `git show c3ae1c97:…`.
   - ¿La normalización de la guarda es la declarada?
   - ¿Hay cambios que la guarda no vería: un cambio dentro de un literal de cadena, o un cambio de orden que la normalización oculte?
   - ¿Las autopruebas negativas son reales?
4. **`CompleteApply` por rama y `TryUpdatePilot` (INV-09, INV-10, D-T4B-06, D-T4B-07)**, en `src/RackCad.Application/Workspace/Drafts/DraftSession.cs` y `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`.
   - Fila por fila de la tabla de D-T4B-06: ¿la ranura se libera exactamente una vez y la excepción nunca escapa?
   - ¿Un NotApplied* puede acabar en «aplicado»?
   - ¿El bloque del error diferido es idéntico byte a byte al de la base?
   - ¿La puerta propia es la del paquete: sin reposo ni lector, y con `Authorize` en la forma exacta?
   - ¿El orden de los pasos 1 a 7 es el del paquete?
   - ¿`Failed` solo antes de `TryBeginApply`?
   - ¿El fin de la marca de limpio va en los resultados correctos?
   - ¿Los textos cumplen el apartado «Textos» de D-T4B-07?
5. **El RED (INV-15).**
   - ¿Los cuerpos del RED son inertes también en las fábricas, igualdades y reenvíos?
   - Sobre `red-ci-core/core.trx`: ¿todos los fallos son por aserción (Message sin el prefijo de tipo cualificado seguido de « : », y la primera línea de StackTrace bajo `tests/`) y de pruebas nuevas o cambiadas?
   - ¿Pasan en el RED las 21 clases de regresión de Core y los dos filtros de UI?
   - ¿Las guardas que pasan ya en el RED son las que declara D-T4B-04 (8)? ¿Hay alguna guarda positiva que pase en el RED cuando debería fallar, y que por tanto sea tautológica?
6. **La calidad de las pruebas.**
   - ¿El doble del ámbito (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs`) permite probar de verdad cada fila de la tabla?
   - ¿Hay aserciones débiles, tautológicas o que pasarían con otro comportamiento?
   - ¿Las guardas de fuente son frágiles, o dan falsos positivos ante un cambio trivial?
   - ¿Los cambios de guarda existentes son solo los declarados en D-T4B-07 y D-T4B-09 (INV-14), y cada uno con su autoprueba en negativo?
7. **Contención (INV-18, la autorización del Owner y F4-E59).** ¿Alguna prueba nueva o cambiada puede, en algún camino (incluido el RED), escribir fuera de memoria, usar la configuración real, dejar vivo un modal, un dispatcher o estado persistente, o ejercer con entradas válidas un camino con diálogo o escritura?
8. **Honestidad de la entrega.** ¿Alguna afirmación de la entrega es falsa o no se puede verificar con los artefactos (conteos, la lista de guardas que pasan en el RED, las decisiones declaradas o los builds)?

## Formato del informe

1. Objeto y método: qué leíste y qué no pudiste verificar.
2. Un apartado por cada uno de los puntos 1 a 8, con evidencia concreta (archivo:línea y SHA).
3. La tabla de hallazgos, con Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y una disposición propuesta.
4. Un veredicto final: **ACEPTABLE**, **ACEPTABLE CON MENORES** o **NO ACEPTABLE**, con el recuento de bloqueantes, importantes y menores.

Tu respuesta final, que vuelve a la sesión, es solo una línea: el veredicto, el recuento y la ruta del informe.
