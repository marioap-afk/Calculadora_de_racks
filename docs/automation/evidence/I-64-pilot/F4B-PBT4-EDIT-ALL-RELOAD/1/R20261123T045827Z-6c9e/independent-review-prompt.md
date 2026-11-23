Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT4-EDIT-ALL-RELOAD (PB-T4e, la recarga de «Editar todo»), intento 1. Es RackCad, un plugin de AutoCAD 2025 en C#/.NET 8 con WPF. No participaste en el trabajo. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- **El worktree, PROHIBIDO:** no leas, listes ni ejecutes nada en `<worktree-de-I-64>`, y no escribas esa ruta en ninguna línea de órdenes.
- **El código y los documentos, SOLO por objetos git** desde el checkout principal, que comparte el almacén: `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4ecc\rv\`:
  - `worker-handoff.json`;
  - `relay-record-work.json`, el registro del trabajo con lo que la sesión midió en sus Notes;
  - `relay-subst-check.json`: el detalle de lo que el relevo comprobó (en `GreenLiteral`, el GREEN literal; en `WarningsCS`, los registros de build; la lista vacía, sin ningún archivo de `Skip`);
  - `tests/`, `inv/` y `gen/`, con las TRX, los logs y las salidas de sus guiones y de las herramientas del paquete (`apply-red.txt` y `apply-green.txt`), y `clean/`, la prueba de limpieza, si la hay;
  - `ci-core/` y `ci-ui/` (la CI del GREEN) y `red-ci-core/` y `red-ci-ui/` (la del RED).

**Las abreviaturas:**
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- el puerto: `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- la precondición 6: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/pbt4e-precondition6.json` (las piezas, la lista vacía, `RedFiles` y `GreenFiles`, los totales, la cobertura y los RequiredTests), con su registro `cc4e/compile-check.md`.

**El objeto.** El diff es `git diff 7da361ebe8919bf31e2874c42dc946b98c62635a 0eac7911a025a59628b446db8b768035175f96de`, con dos commits:

1. **El RED, `265f9e31cb33ce312c04d2d7959c999aee763b3c`.** Es la pieza literal de `tests/` del paquete (INV-PBT4E-02; sin esqueleto): `pbt4e-red-v56.txt` (sha256 `d052f73d…`), que da `red_pbt4e_v56.red`, la función del generador custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/red_pbt4e_v56.py` (sha256 `8c38e532…`), y que escribe la herramienta del paquete `apply_red_pbt4e_v56.py`. **No toca `src/`.** Son 2 archivos nuevos de `tests/`, las dos clases de las CT (la lista cerrada CT-PBT4E-01 a 04, §5.7 del paquete), cada uno con su `NewFileSha` (`RedFiles` de la precondición 6):
   - `PushBackEditAllReloadTests` (Core; CT-01, 02 y 04; `9e849864…`);
   - `PushBackEditAllReloadFieldTests` (UI; CT-03; `a29d7a36…`).
   Ninguna prueba existente se edita, se salta ni se borra: la lista cerrada de PB-T4e está vacía (INV-PBT4E-07).
2. **El GREEN, `0eac7911a025a59628b446db8b768035175f96de`.** El texto literal del paquete (`pbt4e-green-v56.txt`, sha256 `a745c273…`, que da `green_pbt4e_v56.green`, la función del generador custodiado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/green_pbt4e_v56.py`, sha256 `b3aba680…`, y que escribe `apply_green_pbt4e_v56.py`; el sha256 de cada archivo, en `GreenFiles` de la precondición 6): solo los 4 archivos de `src/` del alcance, que cambian: `WorkspaceHostCore.cs` (`6eb9bf7a…`), `WorkspaceEditIntent.cs` (`bbc68822…`, solo comentarios), `IWorkspacePilotEditPort.cs` (`49b5245c…`, solo comentarios) y `WorkspacePilotFieldViewModel.cs` (`c4b5c699…`). Ninguno de `tests/`.

Además: WorkRunId `R20261123T045827Z-6c9e`, en `artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/`. BaseSha = ChainBaseSha = `7da361ebe8919bf31e2874c42dc946b98c62635a`, el commit de emisión del contrato (F4B-E60) y primera delegación. Su `src/` y sus `tests/` son los del cierre de PB-T4d (F4B-E59, `160e019d`).

**Qué es PB-T4e** (D-PBT4-22; el `Objective` del contrato, con lo que eligió el Owner en `owner-pbt4e-list-decisions.md`: «Solo la recarga» y «No abrir y avisar»):
- **«Editar todo» recarga desde el dibujo**, antes de abrir la ventana modal, un borrador Clean marcado como posiblemente obsoleto, con la misma recarga que «Editar» (`TryReloadPilotEdit`) y sin aviso.
- **Si la recarga no recarga, la ventana no se abre**, y el campo muestra lo mismo que «Editar»: el motivo, la pista de RACKEDITAR y, para una recarga no admitida, «No se recargó: el borrador se conserva tal como estaba.».
- **Las tres decisiones de diseño de la sesión:**
  - D-S17: la recarga va en el anfitrión;
  - D-S18: se reutiliza `EditAllStatus.NotBegun`;
  - D-S19, tras CA-T4e M-01: la recarga va detrás de las tres comprobaciones de la vista, sobre un borrador que el gesto no acaba de empezar.
- **Ningún otro cambio visible:** ni «Editar», ni el botón, ni la ventana, ni RACKEDITAR, ni el Plugin; ningún texto ni estado nuevo; cero escrituras en el DWG; no se ejecuta AutoCAD.

**Las autoridades** (en `0eac7911a025a59628b446db8b768035175f96de`):
- **El contrato,** `docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json`:
  - sus invariantes (INV-PBT4E-01 a 14), el AllowedWriteScope (6 rutas), las autoridades UNIT_CHANGE (los 4 de `src/`) y los RequiredTests (90 filtros, 2 con `ExpectRed`);
  - sus paradas, en especial:
    - C02: la lista vacía, sin ninguna excepción;
    - C03: un cambio observable fuera de D-PBT4-22;
    - C07: un hecho del §2 o de D-PBT4-22 que no se cumple, o una línea esperada que el GREEN no cumple (el Worker no ajusta lo esperado);
    - C10: lo que el alcance excluye;
    - C13: la lista cerrada de 1 declaración nueva, `NotReloadedText`, y ninguna quitada;
    - C16: una prueba existente que falle, o una línea o una rama de `src/` que se queda y deja de cubrirse, vuelve al Owner;
    - C17 (D2) y C18 (la reparación del RED);
  - los totales exactos de cada rama (F4B-E46) y la línea base (la CI del cierre de PB-T4d, run `45057868915` sobre `7db6fd0c`: Core 19 418 con 403 Skipped y UI 2 464 con 48 Skipped).
- **El paquete,** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.6): D-PBT4-22, §4.5, §5.7 (las CT, línea a línea), §6.0 (RR-3), §6.5 y el anexo V56-A (el texto literal del GREEN y de los dos archivos nuevos del RED).
- **Las decisiones de diseño de la sesión,** D-S17 a D-S19: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/session-pbt4e-design-decisions.md`.
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`:
  - `owner-pbt4e-list-decisions.md`, la 15.ª tanda;
  - `owner-pbt4c-code-decisions.md`: la pregunta 2, OWNER-02, el origen de PB-T4e.
- **Las decisiones anteriores:** F4B-E35, F4B-E46 y D2 (`f4b-dispositions.md` y `owner-d1-d2-decisions.md`).
- **El registro de la sustitución,** la entrada «## PB-T4e» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`: la lista vacía.

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - ¿El RED es solo `tests/`, los dos archivos nuevos, cada uno con el texto de la pieza (su `NewFileSha`), con `src/` sin cambio? Compruébalo tú con `git show` y sha256, no con el resumen.
   - ¿El GREEN es el texto literal del paquete, solo los 4 archivos de `src/` del alcance, sin nada de `tests/`, y con WEI y el puerto cambiados solo en comentarios?
   - ¿La única declaración nueva es la de la lista cerrada de C13 (`NotReloadedText`, privada, en FVM), por nombre y con su cuenta (una sobrecarga y un tipo anidado cuentan), y no se quita ninguna?
2. **La lista vacía y C02.**
   - ¿`git diff 7da361ebe8919bf31e2874c42dc946b98c62635a 0eac7911a025a59628b446db8b768035175f96de -- tests/` solo añade los dos archivos nuevos?
   - ¿Ninguna prueba ni ningún ayudante existente se edita, se salta, se reactiva ni se borra?
3. **El diseño frente al código,** D-PBT4-22 punto a punto:
   - **La recarga en el anfitrión** (`EditAllOfPilot`, D-S17 y D-S19). ¿Va después de la compuerta, de la actualización en vuelo, del anfitrión del editor y de las tres comprobaciones de la vista (`Applying` o no registrado, `CanEdit` falso y el texto pendiente), y antes de `ShowModal`? ¿Solo sobre un borrador que el gesto no acaba de empezar (`begunHere` falso) y con `NeedsLazyReload`?
   - **La llamada.** ¿Llama a `TryReloadPilotEdit(request)`, y no a una copia, guardada en la variable `reload`?
   - **Los resultados.** ¿Con `Reloaded` abre, sobre el mismo borrador? ¿Con cualquier otro resultado devuelve `EditAllOutcome.NotBegun` con el resultado de la recarga y no abre?
   - **Lo que va antes.** ¿Nada evalúa `NeedsLazyReload` antes de esas tres comprobaciones? ¿Hay como mucho una lectura por gesto, y ninguna durante `ShowModal`?
   - **El campo** (FVM, D-S18). ¿Con `NotBegun` y `NotAdmitted` sobre un borrador que ya existía (`view.Exists`) pone el diagnóstico, la pista y `NotReloadedText`? ¿`BeginEdit` usa la misma constante sin cambiar lo que muestra? ¿Los demás resultados salen como en «Editar» (`Describe`)?
   - **Los comentarios.** ¿Son solo comentarios (WEI, el puerto y el resumen de `EditAll`)? ¿El del puerto conserva las dos frases que fija `WorkspacePilotEditGuardTests` y no lleva las dos que prohíbe?
   - **Lo que no hace.** ¿Hay algún aviso cuando recarga? ¿Dirty, Conflict, un borrador sin la marca, «Sin decidir», Unknown, Error, Applying y con texto pendiente responden como en BaseSha? ¿Hay algún miembro nuevo del puerto, o algún valor nuevo de `EditAllStatus` o de `PilotBoundaryKind`?
4. **Lo que la entrega declara, y que tienes que juzgar tú:**
   - **(a) Sus desviaciones y sus hallazgos inesperados:** ¿alguno cambia lo que fija el contrato o el paquete?
5. **D2 y su límite (lo más importante).**
   - ¿Alguna copia comparte un objeto mutable fuera de las dos referencias declaradas de cada lado?
   - ¿Alguna línea añadida muta un `PushBackSystem` o un `RackModuleReconciliationResult` adoptado?
6. **Las CT** (CT-PBT4E-01 a 04; §5.7, las 16 líneas de CT-01, 02 y 04 y las 9 de CT-03):
   - ¿fijan lo que dice el §5.7, línea a línea, con la línea `marcado-sin-decidir` de CT-01 (D-S19)?
   - ¿hay un `[Fact]` por CT?
   - ¿sus dobles van dentro de su clase, sin referenciar las clases de PB-T4c?
   - ¿la CT de UI fija que el campo solo pide `TryEditAll`?
   - ¿cae cada una en el RED por su `Assert.Equal` y no por una excepción?
7. **El RED y el GREEN** (las TRX de `rv/`):
   - ¿fallan por aserción (la primera línea del StackTrace bajo `tests/`) los 2 filtros con `ExpectRed` (las 3 CT de Core y la de UI), y pasan todos los demás?
   - ¿las 7 filas `Skipped` de la base (D-1 y CT-PBT3A-12) siguen saltadas, y lo demás cuadra con los totales?
   - ¿alguna prueba existente falla (C16)?
   - ¿cuadran los totales exactos de cada rama?
     - RED: Core 19 421 = 19 015 + 3 + 403, y UI 2 465 = 2 416 + 1 + 48.
     - GREEN: Core 19 421 = 19 018 + 0 + 403, y UI 2 465 = 2 417 + 0 + 48.
   - ¿ningún `warning CS`?
   - ¿la CI del RED da tests en failure, ui-tests en failure, build-ui en success y build-plugin omitido, y la del GREEN 4/4, con el Plugin?
   - ¿la cobertura de lo que se queda (INV-PBT4E-06: la suite completa de Core y los 19 filtros de UI del contrato, medida en la precondición 6) sigue siendo la de BaseSha? Lo que no puedas medir sin ejecutar, dilo.
8. **Contención:**
   - ¿algún cambio observable de «Editar», del botón, de la ventana, de RACKEDITAR, del camino clásico, del Plugin o de la escritura del dibujo (C03)?
   - ¿alguna escritura en el DWG, en disco o en el perfil?
   - ¿corrió el Worker algo de la UI fuera de los filtros del contrato?
9. **Hallazgos para el Owner:**
   - ¿alguna decisión que el contrato no le dejaba al Worker?
   - ¿algún riesgo de la recarga que el Smoke-PB tenga que mirar y el paquete no declare?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt4ecc\independent-review-code.md`, **sin rutas locales absolutas**. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta final. El informe lleva:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- la lista vacía y C02;
- el punto 4 (a);
- D2;
- las CT;
- los hallazgos OWNER;
- lo no verificado.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.
