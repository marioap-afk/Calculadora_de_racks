# Encargo: revisión adversarial independiente del código de I-64 F4-T1B-PILOT-CHARACTERIZATION (intento 1)

Eres un revisor independiente y adversarial. Tu trabajo es encontrar defectos reales en una entrega que dice cumplir su contrato: intenta refutar cada afirmación de la entrega contra el código y los artefactos. No es tu trabajo confirmar.

## Dónde está todo

- Worktree (solo lectura): `<worktree-de-I-64>`. Rama `architecture/workspace-persistente-rackcad`.
- Commits:
  - base `0a6f84c298d0f542bbfdedabd82946490196e23f` (ChainBaseSha = BaseSha);
  - RED `92f5f5dbed597bc0e5a9ab57e83ff8a59459933e`, que lleva las siete clases nuevas de prueba y los mutantes deliberados del producto;
  - GREEN `1d36d0065ed3a469004f571ad8e3bc0fdc0a0d02`, que revierte los mutantes.
- Contrato que manda: `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json`. Tiene 16 invariantes `INV-F4T1B-01..16` y 27 paradas; la tabla CERRADA de mutantes MU-01..MU-28 está en INV-F4T1B-14.
- Paquete del contrato, que es la fuente del diseño: `docs/initiatives/I-64-f4-t1b-gate-contract-package.md`. Lo esencial está en el §2 (hechos medidos), el §3 y en D-T1B-01..07: reglas (a), (b) y (c) de los mutantes, tabla prevista, aserciones ancla y CT-01..CT-28.
- Autorización del Owner, con sus órdenes de contención: `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1b-authorization.md`.
- Entrega del Worker: `artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214644Z-c077/worker-handoff.json`. Evidence[0..11] contiene el mapa CT→método, la tabla de mutantes, las anclas, la unión, la tabla de bytes, las mediciones, la limpieza y los builds; mira también UnexpectedFindings, Deviations y KnownLimitations.
- Artefactos locales de esa carpeta (ignorados por Git): `tests/<RunRef>/` con los TRX de cada corrida y el `mutant.patch` de cada mutante aislado (`m01`, `m02`…); `ci-ui/ui.trx` y `ci-core/core.trx` (CI del GREEN); `red-ci-ui/ui.trx` y `red-ci-core/core.trx` (CI del RED).
- Archivos nuevos (los únicos de `BaseSha..CurrentSha`): `tests/RackCad.UI.Tests/PushBackClassicCharacterization{Keyboard,Close,Intent,Owner,Resource,SourceGuard}Tests.cs` y `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`.
- Ventana caracterizada: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` y `.xaml.cs`, más los demás archivos de MutantScope que lista el contrato.

## Reglas de esta revisión

- **Solo lectura:**
  - no compiles;
  - no ejecutes pruebas: la suite de UI existente escribe en el perfil del usuario;
  - no modifiques ni crees nada en el worktree;
  - no hagas ninguna operación de Git que escriba (commit, checkout, stash, restore, reset, fetch, push…).
- **Se permite** leer archivos y usar `git show`, `git diff`, `git log`, `git cat-file` y `git rev-parse` con `git -C <worktree>`.
- **No escribas en el informe** ninguna ruta absoluta local, nombre de usuario ni nombre de máquina: solo rutas relativas al repositorio. El repositorio es público.
- **Escribe el informe** en español, en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\t1b\independent-review-code.md`. Es el único archivo que puedes crear.

## Qué tienes que intentar refutar (como mínimo)

1. **Tabla cerrada (INV-14, F4T1B-C04, C10).** Comprueba cada cambio de `git diff 0a6f84c2 92f5f5db -- src`:
   - que instancia una fila de MU-01..MU-28, o una alternativa que la propia fila enumera;
   - que respeta la clase que su archivo permite (de comportamiento o textual con comportamiento idéntico);
   - que cumple las reglas (a) y (b) del paquete (D-T1B-03).
   - Para los textuales: ¿alguno cambia de verdad el comportamiento o genera un aviso del compilador (CS0162, CS0168, CS0169, CS0219, CS0414, CS8321)?
   - Para los de comportamiento: ¿alguno toca `Owner`, un `ShowDialog`, un `MessageBox`, `EditorDiscardPrompt`, una comprobación de nulo de una costura o el constructor? ¿Desvía algún gesto hacia un botón que abre un diálogo o escribe? ¿Debilita una guarda de validez, de bloqueo o `canInsertInAutoCad`? ¿Puede dejar un modal abierto?
   - Juzga si es legítimo instanciar una fila con varias variantes o en varios sitios (Deviations[1]).
2. **Prueba por mutante (INV-15).**
   - Para cada mutante, ¿el `mutant.patch` de su corrida aislada coincide con lo que de verdad hay en el RED?
   - ¿Fallan en su TRX aislado al menos las pruebas que INV-14 le asigna?
   - ¿Hay algún mutante del RED sin corrida aislada, o alguna fila obligatoria sin mutante?
   - Comprueba también las corridas repetidas sobre el GREEN tras retocar pruebas (`m01b`, `m15vb`, `m15db`).
3. **RED conjunto y criterio mecánico (INV-02).** Sobre los TRX de la CI del RED (`red-ci-*`), para cada filtro:
   - todos los resultados en Failed;
   - ningún Message con el prefijo `Tipo.Cualificado : `;
   - la primera línea de StackTrace bajo `tests/`;
   - ningún NotExecuted ni Skipped;
   - `Failed == Selected >= MinSelected` (UI 22, Core 4).
   - Además: ¿la unión de pruebas existentes rotas que declara Evidence[3] está bien atribuida?
4. **GREEN = inverso exacto (INV-01):**
   - `git diff ChainBaseSha CurrentSha -- src` vacío;
   - `--name-only` igual a los siete archivos;
   - el blob de cada archivo mutado en CurrentSha igual al de ChainBaseSha;
   - `RedSha..CurrentSha` no toca `tests/`.
5. **Calidad de la caracterización** (D-T1B-04, CT-01..CT-28 salvo CT-18 opcional).
   - ¿Cada CT obligatorio tiene un método propio que fija de verdad lo que el paquete pide?
   - ¿Hay aserciones débiles, tautológicas o que pasarían con otro comportamiento?
   - ¿Los textos son byte a byte los del producto (INV-09)?
   - ¿Las aserciones sin mutante propio están declaradas como ancla y van en un método detectado?
   - ¿Hay ramas condicionales prohibidas (INV-04: «sin ramas del tipo si falla el control, tomar el respaldo»)?
   - ¿Hay algún `Assert` dentro de un delegado que ejecuta el producto?
   - ¿Las guardas de fuente normalizan CRLF→LF, quitan comentarios y buscan por las raíces de la clausura (INV-08)?
   - ¿Serían frágiles o falsos positivos ante un cambio trivial del archivo?
6. **Contención (INV-05, INV-06, INV-07, INV-16, F4T1B-C09 y la autorización del Owner).**
   - ¿Alguna prueba nueva puede, en algún camino, incluido cuando falla (RED), quedarse esperando un diálogo real, dejar vivo un dispatcher, una ventana o un modal, escribir en el perfil o en disco, instanciar `RackFrameConfiguratorWindow` o ejercer PromptSaveToLibrary, BOM, `RackProjectStore.Save`, `SaveLibrary_Click` o `InsertBatch_Click` con entradas válidas?
   - Revisa en detalle CT-04, que usa un `ShowDialog` real con respaldo, y CT-23 (ventana sin mostrar).
   - Revisa la desviación de UnexpectedFindings[1] (una sonda temporal de medición que disparó `InsertBatch_Click` con entradas válidas) y la limitación sobre las suites existentes que reescriben el archivo de disposición del perfil real. ¿Qué gravedad tienen respecto a la orden del Owner?
7. **Determinismo y entorno:**
   - el GUID sale de `newIdFactory`;
   - la cultura (INV-11);
   - ¿hay dependencias de orden entre pruebas, del foco o de la activación de la ventana (CT-04 promovido: ¿es de verdad independiente de la activación?), de rutas de máquina o de esperas fijas?
8. **Honestidad de la entrega.** ¿Alguna afirmación de la entrega es falsa o no se puede verificar con los artefactos (conteos, tablas, mapa CT→método)?

## Formato del informe

1. Objeto y método: qué leíste y qué no pudiste verificar.
2. Un apartado por cada uno de los puntos 1 a 8, con evidencia concreta: archivo:línea y SHA.
3. La tabla de hallazgos con Id, gravedad (BLOQUEANTE, IMPORTANTE o MENOR), contenido, evidencia y una disposición propuesta.
4. Un veredicto final: **ACEPTABLE**, **ACEPTABLE CON MENORES** o **NO ACEPTABLE**, con el recuento bloqueantes/importantes/menores.

Tu respuesta final, que vuelve a la sesión, es solo una línea: el veredicto, el recuento y la ruta del informe.
