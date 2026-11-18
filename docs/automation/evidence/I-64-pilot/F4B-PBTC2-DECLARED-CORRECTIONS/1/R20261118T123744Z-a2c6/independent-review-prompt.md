Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBTC2-DECLARED-CORRECTIONS, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**El worktree,** en solo lectura: `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`. Usa siempre `git -C <worktree>`, sin checkout.

**El objeto es todo el cambio de PB-TC2:** `git diff eedb069ce8ae79cda684f2620694949fd402051d b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4`.
- BaseSha = ChainBaseSha `eedb069c`, el commit de emisión del contrato.
- RED `3677847c59e92b34567fa42c600eb2c01e65576c`: solo los dos archivos nuevos de prueba.
- GREEN `b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4`: solo W y WC de la ventana clásica de Push Back.
- **Lo que corrige PB-TC2,** como cambio declarado de RACKEDITAR (F4B-E09):
  - **H-31:** el hueco entre lados, la longitud del módulo y las cuatro cajas avanzadas anulables, con la regla `Keep`;
  - **OWNER-1:** en «Ambos lados» con el lado B presente, una caja mixta que recibe un valor lo aplica a los dos lados.
- **La historia de la tarea:**
  - una primera invocación del Worker (`R20261118T055535Z-38d3`) paró, sin commits, en la medición previa por PBTC2-C14: el BOM de postes se partía en dos líneas con una altura manual de cabecera exacta;
  - el Owner ratificó ese efecto y ordenó reanudar sin gastar intento (F4B-E11, D1);
  - la segunda invocación (`R20261118T113927Z-037c`) es la que entrega el RED y el GREEN.

**Las autoridades** (léelas en el GREEN):
- el contrato `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json`, con sus invariantes INV-PBTC2-*, el AllowedWriteScope (4), el ForbiddenWriteScope (129), los RequiredTests (18) y las paradas, en especial PBTC2-C04, C07, C09, C10 y C14;
- el paquete `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md` v2: el §2 (hechos), el §3 (el diseño POR LÍNEA, D-PBTC2-01 a 08) y el §4 (CT-PBTC2-01 a 04 con sus líneas esperadas);
- las decisiones del Owner:
  - `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md` (F4B-E09);
  - D1 y D2 (F4B-E11), cuyo texto literal está en `artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T113927Z-037c/owner-d1-d2-decisions.md`;
- las dos entregas: `…/1/R20261118T055535Z-38d3/worker-handoff.json` y `…/1/R20261118T113927Z-037c/worker-handoff.json`, con sus TRX y logs bajo `tests/` y los de la CI en `ci-core/`, `ci-ui/`, `red-ci-core/` y `red-ci-ui/`.

**QUÉ REVISAR**, con evidencia `ruta:línea`:

1. **Alcance:** compara tú las listas, frente al AllowedWriteScope y el ForbiddenWriteScope. El RED no toca `src/`, el GREEN no toca `tests/` y WX no cambia.
2. **El diseño frente al código del GREEN** (D-PBTC2-01 a 05), línea a línea. Para cada uno, ¿hace exactamente lo que fija el paquete? Revisa en particular:
   - **`MixedBoth`:** ¿puede escribir a los dos lados un valor que el usuario no escribió? Piensa en un indicador obsoleto, una caja sembrada o el cambio de celda.
   - **`KeepOptional`:** vacía da nulo, el cálculo.
   - **La rama vacía de `OptionalVal`:** ¿queda literal?
   - **`ModuleLengthBox`:** ¿un módulo calculado sigue calculado al pasar por la caja?
   - **`Val`:** ¿se ha retirado sin dejar ninguna referencia?
   - ¿Algún lector de esas cajas queda fuera, o algún comportamiento cambia sin estar declarado?
3. **Cada CT** (CT-PBTC2-01 a 04, en `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2*.cs`):
   - ¿la clase y el método son los del paquete, y las líneas esperadas son las del §4, sin ajustar?
   - ¿fija lo que promete, o hay alguna aserción tautológica?
   - ¿hay casos negativos?
   - ¿se cumplen las reglas de las ventanas: `Ventana()`, la carga dentro del `try`, `Cerrar` en el `finally`, `EditorDiscardPrompt.Substitute` con `descartes=<n>` leído después de cerrar, y ninguna ventana mostrada?
4. **El RED** (TRX en `red-ci-ui/`): ¿fallan los 4 métodos nuevos, por aserción, y ninguna prueba existente?
5. **La medición previa y la orden D1:**
   - ¿la corrida `m8b` reproduce la de `38d3`?
   - ¿la comparación «idéntica» se sostiene con los artefactos?
   - ¿es correcto continuar por D1 sin otra vuelta al Owner?
   - ¿el código temporal se retiró por completo?
6. **El GREEN y la estabilidad:** CI 4/4. ¿Hay dependencias de orden, de reloj, de cultura, de rutas, de foco o de ventanas? ¿Alguna prueba deja una ventana, un modal o un dispatcher vivo?
7. **Contención:** ninguna escritura fuera de una ubicación temporal ni en el perfil del usuario, ni carga del Plugin ni de AutoCAD.
8. **El producto:** ¿la corrección introduce algún defecto nuevo en RACKEDITAR que el Owner deba conocer? No propongas corregir nada fuera del alcance.

**REGLAS:**
- Solo lectura, con git y lectura de archivos.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c`.
- No ejecutes dotnet ni pruebas.
- No escribas en el worktree.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbtc2\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla diseño → código;
- la tabla de los CT con su valoración;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
