> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT1B-LOSSLESS-STATE-TYPE, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**El worktree,** en solo lectura: `<worktree-de-I-64>`, en la rama `architecture/workspace-persistente-rackcad`. Usa siempre `git -C <worktree>`, sin checkout.

**El objeto es todo el cambio de PB-T1b:** `git diff 4b0498e3e05f65813c5e6274b3beb51217587d65 a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3`, con el RED intermedio.
- **BaseSha** = ChainBaseSha `4b0498e3`, el commit de la nueva emisión del contrato con el paquete v2.7.
- **RED** `2d9febc6087280e0ba9f7a6c72e77b283506891d`: el esqueleto inerte del §5.3 y las seis clases de prueba.
- **GREEN** `a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3`: la implementación, solo en `src/` de Application.
- **Qué es PB-T1b:** la segunda subtarea de PB-T1. Crea en Application el estado editable de Push Back sin pérdidas (`PushBackEditableState`), con:
  - las cargas, la salida `BuildInputs`, el copiador y el comparador;
  - el mapa de propiedad y `PushBackEditorOutputGate`;
  - las reglas explícitas de c-02 y de c-09;
  - la excepción D2 del Owner: solo la línea base resuelta y el informe de módulos de cada lado se comparten por referencia, con diez condiciones y su límite.

  La ventana no cambia: es de PB-T1c.
- **La entrega:** `artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/worker-handoff.json`, con sus TRX y logs bajo `tests/` y los de la CI en `ci-core/`, `ci-ui/`, `red-ci-core/` y `red-ci-ui/`.
- **La historia:**
  - una delegación anterior (contrato de la v2.6) tuvo una invocación detenida por una pausa del Owner y otra (`R20261118T193423Z-fb01`) que paró con PBT1B-C15 en pb-15. Su borrador está en `.../1/R20261118T193423Z-fb01/uncommitted-work-at-stop.patch`;
  - el Owner decidió que pb-15 es una normalización conocida (F4B-E20), y el paquete pasó a la v2.7;
  - **esta entrega declara que partió de ese borrador.**

**Las autoridades** (léelas en el GREEN):
- el contrato `docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json`, con sus invariantes INV-PBT1B-*, el AllowedWriteScope (12), los RequiredTests (23) y las paradas, en especial C04, C07, C13, C14, C15 (con su excepción de pb-15) y C17;
- el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.7: el §3 (D-PBT1-02 a D-PBT1-11, D-PBT1-09b y D-PBT1-15, con las diez condiciones de D2 y las reglas de G-L1), el §5.0 y el §5.3 (el esqueleto exacto y CT-PBT1B-01 a 17) y el §6.2;
- las decisiones del Owner, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: `owner-pbt1-pre-decisions.md`, `owner-d1-d2-decisions.md`, `owner-pbt1a-decisions.md` y `owner-pbt1b-c15-decisions.md`.

**QUÉ REVISAR**, con evidencia `ruta:línea`:

1. **Alcance:** las 12 rutas, frente al AllowedWriteScope. El RED es SOLO el esqueleto exacto del §5.3 más las pruebas, sin implementación; el GREEN no toca `tests/`. Se cumplen INV-PBT1B-01, 03 (la API exacta, ningún miembro público de más), 04 (ningún campo de instancia ni estático nuevo en ES, RMES ni los demás tipos del censo), 04b y 05.
2. **El diseño frente al código,** D-PBT1-02 a 11 y 09b, línea a línea:
   - las cargas, con la semántica y el orden de hoy (D-PBT1-03), y su paridad con la ventana;
   - `BuildInputs`;
   - el copiador y el comparador frente a los del piloto;
   - el mapa;
   - c-02 (`ResolvePostCatalogId`) y c-09 (`ResolveWeightUnit`, en la rama que fijó el paso 1);
   - la exclusión de pb-21-cola;
   - `PushBackEditorOutputGate`, frente al criterio de bloqueo de hoy.

   ¿Algún comportamiento cambia sin estar declarado?
3. **D2 y su límite (lo más importante):**
   - ¿la copia comparte por referencia algo más que los no nulos de `workingBaseline` y `LastModuleReconciliation` de cada lado?
   - ¿Alguna línea muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
   - ¿Las guardas CT-14 (G-L1) y CT-15 (los escritores de `Name`) aplican las reglas y las autopruebas EXACTAS de D-PBT1-15, o se dejan algo?
   - ¿CT-07, 08 y 16 prueban de verdad lo que dicen, sin tautologías?
4. **Cada CT** (CT-PBT1B-01 a 17):
   - ¿la clase y el método son los que dice el §5.3, y las líneas esperadas son las del paquete y del paso 1, sin ajustar?
   - ¿Fija lo que promete? ¿Hay aserciones tautológicas o que no pueden fallar?
   - **CT-02:** ¿sus tres pasos son los de la v2.7, y la excepción de pb-15 solo cubre la normalización conocida?
   - **CT-06:** ¿aplica el `Mutator` del piloto y solo las exclusiones ancladas?
   - **CT-01 (UI):** ¿compara de verdad la ventana con el estado?
5. **El borrador heredado:** ¿qué partes vienen del parche de `fb01`? ¿Arrastran algo de la v2.6, por ejemplo la lectura de CT-06 por reflexión, que la v2.7 corrigió? ¿El RED tenía de verdad solo el esqueleto? Compara el RED con el §5.3.
6. **El RED** (TRX en `red-ci-core/` y `red-ci-ui/`): ¿fallan por aserción todos los métodos de los filtros con ExpectRed? ¿Pasan las guardas de D2? ¿Ninguna prueba existente falla?
7. **El GREEN y la estabilidad:** la CI 4/4 con Core 19 086 y UI 2 437 (2 416 y 21). ¿Hay dependencias de orden, de reloj, de cultura, de rutas o de estado estático (por ejemplo, la reflexión sobre singletons)? ¿Alguna prueba deja una ventana o un modal vivo?
8. **Contención:** ninguna escritura fuera de una ubicación temporal ni en el perfil; ni Plugin ni AutoCAD.
9. **Hallazgos para el Owner:** ¿alguna diferencia o pérdida nueva que la entrega no declare, distinta de la normalización conocida de pb-15?

**REGLAS:**
- Solo lectura, con git y lectura de archivos.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el worktree.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\hs65\pbt1b\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla diseño → código;
- la tabla de los CT con su valoración;
- D2;
- el borrador heredado;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
