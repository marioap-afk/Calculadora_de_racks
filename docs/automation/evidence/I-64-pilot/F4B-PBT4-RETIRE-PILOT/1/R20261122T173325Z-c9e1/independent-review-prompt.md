Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT4-RETIRE-PILOT (PB-T4d, la retirada del código del piloto de Push Back), intento 1. Es RackCad, un plugin de AutoCAD 2025 en C#/.NET 8 con WPF. No participaste en el trabajo. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- **El worktree, PROHIBIDO:** no leas, listes ni ejecutes nada en `<worktree-de-I-64>`, y no escribas esa ruta en ninguna línea de órdenes.
- **El código y los documentos, SOLO por objetos git** desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4dcc\rv\`:
  - `worker-handoff.json`;
  - `relay-record-work.json`, el registro del trabajo con lo que la sesión midió en sus Notes;
  - `relay-subst-check.json`: el detalle de lo que el relevo comprobó (en `Retirada`, el papel de cada ruta en el RED y en el GREEN y C02 de la retirada archivo a archivo, `C02PorArchivo`; las tres clases nuevas que no son CT con su `NewFileSha`, en `Equivalentes`; el GREEN literal y los `warning CS`);
  - `tests/`, `inv/` y `gen/`, con las TRX, los logs y las salidas de sus guiones y de las herramientas del paquete (`apply-red.txt` y `apply-green.txt`), y `clean/`, la prueba de limpieza, si la hay;
  - `ci-core/` y `ci-ui/` (la CI del GREEN) y `red-ci-core/` y `red-ci-ui/` (la del RED).

**Las abreviaturas:**
- Pilot/: `src/RackCad.Application/Systems/PushBack/Pilot/`;
- WPUR: `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`;
- WPPG: `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`;
- la lista: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/pbt4d-retire-list-271.tsv` (284 filas, 271 con acción; sha256 `05a4d3d1…`);
- la precondición 6: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/pbt4d-precondition6.json` (la lista, las piezas, `RedFiles` y `RedDeleted`, `GreenFiles` y `GreenDeleted`, los totales, la cobertura y el censo), con su registro `cc4d/compile-check.md`.

**El objeto.** El diff es `git diff 399c9bec6d4778aa50096ab6204859c0bd015419 7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`, con dos commits:

1. **El RED, `aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a`.** Es la pieza literal de `tests/` del paquete (INV-PBT4D-02; sin esqueleto: D-S11): `pbt4d-red-v54.txt` (sha256 `80f8cc0c…`), que da `red_pbt4d_v53.red_full`, el generador custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/red_pbt4d_v53.py` (sha256 `7a67652e…`), y que escribe la herramienta del paquete `apply_red_pbt4d_v53.py`. Cada archivo, con el sha256 de `RedFiles` de la precondición 6. **No toca `src/`.** Son 31 rutas de `tests/`:
   - **Los 26 archivos existentes del alcance** (23 que el RED edita y 3 que borra), cada fila de la lista como dice su acción (borrar, reescribir, sustituir o acotar con registro, o se queda; INV-PBT4D-07). Los tres que borra son de UI y se quedan sin pruebas: `PushBackDeclaredCorrectionPilotParityTests.cs`, `PushBackFieldValidationConcordanceTests.cs` y `Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs`.
   - **Los tres `Skip` con su equivalente** (D-S12), cada uno en la línea de atributo de su prueba y con su motivo literal, que nombra su equivalente: uno en `PushBackEditAllFullCopyGuardTests.cs` y dos en WPPG.
   - **Las tres clases nuevas que no son CT** (INV-PBT4D-05), byte a byte las de la pieza (su `NewFileSha`): `PushBackRetirePilotRunTests` (las 29 sustitutas de WPUR sobre el modelo completo; `3554df0d…`), `PushBackRetirePilotFullCopyGuardTests` (el equivalente de CT-PBT4B-20; `ee9b6f15…`) y `PushBackRetirePilotPlanGuardTests` (las dos guardas de WPPG; `48c2f4a9…`).
   - **Las dos clases de las CT** de la lista cerrada CT-PBT4D-01 y 02 (§5.6 del paquete), un `[Fact]` por CT: `PushBackRetirePilotGuardTests` (CT-01, `ExpectRed` true) y `PushBackRetirePilotCensusGuardTests` (CT-02).
2. **El GREEN, `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`.** El texto literal del paquete (`pbt4d-green-v54.txt`, sha256 `aaf6cab8…`, que da `green_pbt4d_v53.green`, sha256 `ea7b2086…`, y que escribe `apply_green_pbt4d_v53.py`; su sha256, en `GreenFiles` y `GreenDeleted` de la precondición 6): solo los seis archivos de Pilot/ del alcance. 4 cambian (`PushBackPilotMaterializer.cs`, `PushBackPilotUpdatePlan.cs`, `PushBackPilotUpdateRun.cs` y `PushBackPilotDraftState.cs`) y 2 se borran (`PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`). Ninguno de `tests/`.

Además: WorkRunId `R20261122T173325Z-c9e1`, en `artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/`. BaseSha = ChainBaseSha = `399c9bec6d4778aa50096ab6204859c0bd015419`, el commit de emisión del contrato (F4B-E56) y primera delegación. Su `src/` y sus `tests/` son los del cierre de PB-T4c (F4B-E55, `6c2820ba`).

**Un dato de la sesión sobre la CI de la base:** la CI del commit de BASE `399c9bec` (run `45043090710`) falló por un cuelgue de 5 minutos en `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_…`, la prueba lenta que PB-T4d acota (D-S14). Ese commit solo lleva documentos, y la CI de `6c2820ba` (la línea base) pasó. No es de la subtarea, pero tenlo en cuenta al leer la CI y al juzgar la prueba lenta acotada.

**Qué es PB-T4d** (D-PBT4-10; el `Objective` del contrato, con el alcance B que eligió el Owner):
- **Se retira** el código del piloto que el panel ya no usa desde PB-T4b: el comparador del piloto y el adaptador de su campo, enteros; `ComposeForWrite`, `KeepPilotEditsOn` y `PushBackPilotComposition`; `PushBackPilotPrepareInput`, su `Prepare` y su `Build`; el `Precheck`, el `Execute` y el `ComparePost` del piloto; y, en `BuildCore`, el modo que no borra vistas (`allowErase`, la rama de la vista no disponible y sus dos constantes, `ViewUnavailablePrefix` y `ViewUnavailableSuffix`).
- **Se limpian** los cuatro `cref` y el comentario que nombran lo retirado (D-S15) y los comentarios que describían como presentes la corrida, el PREPARE y la composición para escribir del piloto (CA-T4d M-05).
- **Se quedan** el lector del piloto, su materialización, `PushBackPilotDraftState`, las constantes de `PushBackPilotMaterializer`, `PrepareFull`, `ExecuteFull`, el `Precheck` del borrador completo, `ComparePostFull`, `BuildFull` y `BuildCore`.
- **Ningún cambio de comportamiento:** ni el panel, ni la ventana, ni RACKEDITAR, ni el Plugin; cero escrituras en el DWG; no se ejecuta AutoCAD. La recarga de «Editar todo» sobre un borrador limpio va a una tarea propia (`owner-pbt4d-list-decisions.md`, pregunta 2).

**Las autoridades** (en `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`):
- **El contrato,** `docs/automation/evidence/I-64-pilot/F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`:
  - sus invariantes (INV-PBT4D-01 a 14), el AllowedWriteScope (37 rutas), las autoridades UNIT_CHANGE (los seis de Pilot/) y los RequiredTests (88 filtros, 1 con `ExpectRed`);
  - sus paradas, en especial C02 (ninguna excepción fuera de las 271 filas con acción de la lista), C07 (una sustituta o un equivalente de las tres clases nuevas que falla), C10 (lo que el alcance excluye), C13 (la lista cerrada de 57 declaraciones quitadas, y ninguna nueva), C16 (una prueba que falle fuera de la lista, o una línea o una rama de `src/` que se queda y deja de cubrirse, vuelve al Owner), C17 (D2) y C18 (la reparación del RED);
  - los totales exactos de cada rama (F4B-E46) y la línea base (la CI del cierre de PB-T4c: Core 19 619 con 461 Skipped y UI 2 486 con 49 Skipped).
- **El paquete,** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.4): D-PBT4-10, §4.4, §5.6 (las CT, línea a línea), §6.0 (RR-3), §6.4 y los anexos V52-A y V54-A (el texto literal del GREEN, las ediciones con registro, los ayudantes sin uso, los `Skip` y los archivos nuevos).
- **Las decisiones de diseño de la sesión,** D-S11 a D-S16: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/session-pbt4d-design-decisions.md`.
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md`, `owner-pbt4d-cat4d-decisions.md` y `owner-pbt4b-code-decisions.md` (F4B-E53, los vacíos de M-04).
- **Las decisiones anteriores:** F4B-E35, F4B-E46 y D2 (`f4b-dispositions.md` y `owner-d1-d2-decisions.md`).
- **El registro de la sustitución,** la entrada «## PB-T4d» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`.

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es solo `tests/`, archivo a archivo el texto de la pieza (su sha256 de `RedFiles`; los tres que borra, ausentes), con `src/` sin cambio?
   - ¿El GREEN es el texto literal del paquete, solo los seis de Pilot/ (4 cambian y 2 se borran), sin nada de `tests/`?
   - ¿Las declaraciones quitadas son exactamente la lista cerrada de C13, por nombre y con su cuenta (una sobrecarga y un tipo anidado cuentan), ninguna nueva, y `allowErase` no aparece en `src/`?
2. **La lista y C02.**
   - ¿Cada fila se aplica como dice su acción, y nada más cambia en los 26 archivos existentes (`relay-subst-check.json`, `C02PorArchivo`)?
   - ¿Los ayudantes que se borran son los que la retirada deja sin uso (CA-T4d M-03) y ninguno que siga en uso?
   - ¿Los tres `Skip` llevan su motivo literal, en su línea de atributo, y nombran su equivalente?
3. **Las tres clases nuevas que no son CT.**
   - ¿Cada una es, byte a byte, la de la pieza (su `NewFileSha`)? Compruébalo tú con `git show` y sha256, no con el resumen.
   - ¿Conservan las 29 sustitutas lo que protegían las de WPUR (27 de la corrida genérica, sobre `ExecuteFull`, y 2 de `PushBackPilotSystemRebuild`), sobre el modelo completo y no sobre lo retirado?
   - ¿Pasan las tres en las dos ramas?
4. **Lo que la entrega declara, y que tienes que juzgar tú:**
   - **(a) Sus desviaciones y sus hallazgos inesperados:** ¿alguno cambia lo que fija el contrato o el paquete?
5. **El diseño frente al código,** D-PBT4-10, punto a punto:
   - lo retirado, entero y sin restos (el comparador y el adaptador del piloto, la composición para escribir, la instantánea del piloto, `Prepare`, `Build`, el `Precheck`, el `Execute` y el `ComparePost` del piloto);
   - `BuildCore` sin `allowErase` (sus cuatro condiciones, la rama de la vista no disponible y el nombre de cada sobre releído) y `BuildFull` sin el argumento `true`;
   - los cuatro `cref` y el comentario de D-S15, y los comentarios de CA-T4d M-05 (solo comentarios);
   - lo que se queda, una vez y sin cambio de comportamiento (INV-PBT4D-03 e INV-PBT4D-04).
6. **D2 y su límite (lo más importante).**
   - ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas de cada lado?
   - ¿Alguna línea añadida muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
7. **Las dos CT** (CT-PBT4D-01 y 02):
   - ¿fijan lo que dice el §5.6, línea a línea?
   - ¿sus autopruebas, sobre texto inyectado en memoria, discriminan de verdad?
   - ¿hay un `[Fact]` por CT?
8. **El RED y el GREEN** (las TRX de `rv/`):
   - ¿falla por aserción (la primera línea del StackTrace bajo `tests/`) el filtro con `ExpectRed`, el de CT-PBT4D-01 (1 en Core), y pasan todos los demás?
   - ¿las 7 filas `Skipped` de la base (D-1 y CT-PBT3A-12) siguen saltadas, y lo demás cuadra con los totales?
   - ¿alguna prueba falla fuera de la lista (C16), o alguna sustituta o equivalente de las tres clases nuevas (C07)?
   - ¿cuadran los totales exactos de cada rama?
     - RED: Core 19 418 = 19 014 + 1 + 403, y UI 2 464 = 2 416 + 0 + 48.
     - GREEN: Core 19 418 = 19 015 + 0 + 403, y UI 2 464 = 2 416 + 0 + 48.
   - ¿ningún `warning CS`?
   - ¿la CI del RED da tests en failure, ui-tests en success, build-ui en success y build-plugin omitido, y la del GREEN 4/4, con el Plugin?
   - ¿la cobertura de lo que se queda (INV-PBT4D-06, medida en la precondición 6) sigue siendo la de BaseSha? Lo que no puedas medir sin ejecutar, dilo.
9. **Contención:**
   - ¿algún cambio observable del panel, de la ventana, de RACKEDITAR, del Plugin o de la escritura del dibujo (C03)?
   - ¿alguna escritura en el DWG, en disco o en el perfil?
   - ¿corrió el Worker algo de la UI fuera de los filtros del contrato?
10. **Hallazgos para el Owner:**
    - ¿alguna decisión que el contrato no le dejaba al Worker?
    - ¿algún riesgo de la retirada que el Smoke-PB tenga que mirar y el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4dcc\independent-review-code.md`, **sin rutas locales absolutas**. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta final. El informe lleva:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- la lista, C02 y las tres clases nuevas que no son CT;
- el punto 4 (a);
- D2;
- las CT;
- los hallazgos OWNER;
- lo no verificado.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
