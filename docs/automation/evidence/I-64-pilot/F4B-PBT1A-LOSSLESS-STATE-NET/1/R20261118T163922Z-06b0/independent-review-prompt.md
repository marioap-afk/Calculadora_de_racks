Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT1A-LOSSLESS-STATE-NET, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**El worktree,** en solo lectura: `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`. Usa siempre `git -C <worktree>`, sin checkout.

**El objeto es todo el cambio de PB-T1a:** `git diff a3b63ccc8c8c7ec758960c10f872c371338ce104 73d30982b0c0ce2b8c9213e626df0ab0d50b0475`, más el RED intermedio.
- BaseSha = ChainBaseSha `a3b63ccc`, el commit de emisión del contrato. Su producto es el del GREEN de PB-TC2 (`b5c09f0b`).
- RED `15f1f225b8981d2b78746b0daed4e27da2bcd901`: los siete archivos nuevos de prueba y los quince mutantes de la tabla cerrada en W y WC.
- GREEN `73d30982b0c0ce2b8c9213e626df0ab0d50b0475`: la reversión de los mutantes con `git restore --source=BaseSha`. Al final, `git diff BaseSha GREEN -- src` tiene que estar vacío.
- **Qué es PB-T1a:** la primera de las tres subtareas de PB-T1 (sacar de la ventana WPF el estado editable de Push Back sin pérdidas). Es SOLO la red de caracterización de la ventana clásica: catorce CT nuevos (CT-PBT1A-01 a 14) con un RED por quince mutantes revertidos. El producto no cambia. PB-T1b (el estado en Application) y PB-T1c (la ventana sobre el estado) se van a medir contra esta red. Por eso la pregunta central es si la red **discrimina de verdad** lo que dice fijar.
- La entrega del Worker: `artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T142230Z-d31d/worker-handoff.json`, con sus TRX y logs bajo `tests/` y los de la CI en `ci-core/`, `ci-ui/`, `red-ci-core/` y `red-ci-ui/`, y los parches de los mutantes.

**Las autoridades** (léelas en el GREEN):
- el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/gate-contract.json`, con sus invariantes INV-PBT1A-*, el AllowedWriteScope (9), el ForbiddenWriteScope (131), los RequiredTests (19) y las paradas, en especial PBT1A-C04, C07, C09, C10 y C17;
- el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.4: §2 (hechos), §3 (D-PBT1-01, D-PBT1-12, D-PBT1-14, D-PBT1-15), §5.0 (las recetas X-01, F-01, F-02 y F-03), §5.2 (el régimen, las clases, la tabla de CT y la tabla cerrada de mutantes) y §6.0-6.1 (el contrato);
- las decisiones del Owner: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md` (F4B-E09) y `owner-d1-d2-decisions.md` (F4B-E11, D1 y D2).

**QUÉ REVISAR**, con evidencia `ruta:línea`:

1. **Alcance:** compara tú las listas con el AllowedWriteScope y el ForbiddenWriteScope. `git diff --name-only BaseSha GREEN` tiene que dar los siete archivos nuevos; el RED solo toca `src/` en W y WC; el GREEN no toca `tests/`; el blob de W y WC en el GREEN es el de la BaseSha.
2. **Los mutantes:** cada cambio de `src/` del RED es exactamente una fila de la tabla cerrada (§5.2), la sustitución exacta de una línea con su sangría, sin nada más. MU-T1A-13 está en `LoadExisting` (W:313) y no en W:296. ¿Hay algún mutante fuera de la tabla, alguno que falte o alguno con otra forma?
3. **Cada CT** (CT-PBT1A-01 a 14, en `tests/RackCad.UI.Tests/PushBackStateNet*.cs`):
   - ¿la clase y el método son los que asigna el §5.2? ¿Cada `[Fact]` acaba en un solo `Assert.Equal`?
   - ¿fija lo que promete la fila del §5.2, sobre la salida persistida y no sobre el estado privado?
   - ¿Hay alguna aserción tautológica, alguna línea esperada que no puede fallar, o algún CT que solo pasa porque no mira?
   - ¿Las líneas esperadas son las que midió el paso 1 de la entrega, sin ajustar? ¿Lo que contradice el diseño lleva «INCUMPLIMIENTO CARACTERIZADO», como CT-10?
   - **CT-14:** ¿el corpus (los 17 de `Fixtures/I64`, X-01, F-01, F-02 y F-03), las recetas y los guiones G-1 a G-6 van en el orden fijado? ¿F-03 se comprueba a sí misma, con `pasadas-restaurar=2` y `frontal=A`?
   - **Las recetas:** ¿son copias fieles del §5.0? ¿`BuildX01` se copió sin editar el original?
   - **Las ventanas:** `Ventana()`, la carga dentro del `try`, `Cerrar` en el `finally`, `EditorDiscardPrompt.Substitute` con `descartes=<n>` leído después de cerrar; ninguna ventana mostrada salvo la de CT-11, fuera de pantalla como dice el paquete; sin teclado ni foco real; sin `MessageBox` real, BOM, «Guardar en biblioteca», configurador real ni «Insertar».
4. **La discriminación (INV-PBT1A-03):** para cada mutante, ¿la corrida aislada de la entrega hace fallar por aserción el CT de su fila? Cótejalo con sus TRX. ¿Algún CT de la red no está cubierto por un mutante, o algún mutante discrimina por una razón distinta de la que el paquete le atribuye?
5. **El RED** (TRX en `red-ci-ui/`): ¿fallan los métodos del filtro `PushBackStateNet` por aserción, todos, con el criterio mecánico de INV-PBT1A-02? ¿La tabla T-8 atribuye cada prueba existente que falla a un mutante de forma creíble? ¿Core sigue en `success`?
6. **El GREEN y la estabilidad:** CI 4/4 con UI 2 436 / 21 / 2 415 y Core 19 070, o la diferencia explicada. ¿Hay dependencias de orden, de reloj, de cultura, de rutas, de foco o de ventanas? ¿Alguna prueba deja una ventana, un modal o un dispatcher vivo?
7. **Contención:** ninguna escritura fuera de una ubicación temporal ni en el perfil del usuario; ni carga del Plugin ni de AutoCAD. El código temporal del paso 1 no entró en ningún commit.
8. **D2 y su límite (PBT1A-C17):** ¿alguna receta o prueba muta el rack resuelto (`PushBackSystem`) o el informe de módulos (`RackModuleReconciliationResult`) de una ventana viva, o comparte por referencia otro objeto mutable?
9. **INV-PBT1A-13:** ¿la lista de `UnexpectedFindings` está completa? ¿Hay alguna línea fijada que muestre una pérdida o un control releído que cambia el estado o la salida sin edición, distinta de c-01, c-02 y M-07, que la entrega no declare? Esto va al Owner antes de PB-T1b.

10. **Las desviaciones que declara la entrega** (`Deviations`): evalúalas tú frente al contrato (en especial PBT1A-C04 y PBT1A-C07), frente a las filas de CT-02 y CT-03 del §5.2 y frente a la regla «SIN LATITUD» del prompt del Worker (`artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T142230Z-d31d/prompt.md`). Mira las corridas de medición del paso 1 (p1 a p4) y dime qué cambió entre ellas, por qué, y si lo que fija cada CT sigue siendo lo que describe su fila.

**REGLAS:**
- Solo lectura, con git y lectura de archivos.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el worktree.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt1a\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de mutantes (fila → cambio → CT que discrimina → valoración);
- la tabla de los CT con su valoración;
- los hallazgos OWNER, incluidos los de INV-PBT1A-13;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
