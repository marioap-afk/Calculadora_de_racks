# Encargo de la comprobación acotada de PB-T4e (CA-T4e) (enviado por la sesión a un revisor independiente nuevo, y sus recomprobaciones como mensajes al mismo revisor; texto exacto)

**Rutas locales:** el repositorio es público. Las tres rutas locales del primer encargo se sustituyen por `<worktree>`, `<scratchpad de la sesión>` y `<checkout principal>`. Es la única diferencia con el texto enviado.

## 1. CA-T4e, sobre la v5.5 (2026-11-22T23:38:00.996Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T4e (CA-T4e)** sobre la v5.5 del paquete del contrato de PB-T4 (I-64 F4B, RackCad: plugin de AutoCAD 2025 en C#/.NET 8 con WPF; rama `architecture/workspace-persistente-rackcad`). Eres un revisor independiente y adversarial: no has participado en el paquete. Busca defectos reales y no te fíes de los resúmenes.

**Qué es PB-T4e:** la recarga de «Editar todo» en el Workspace de Push Back. Hoy «Editar» recarga sin aviso un borrador Clean marcado como posiblemente obsoleto antes de editar, y «Editar todo» no mira la marca: abre el editor completo sobre la base vieja. El Owner eligió «Solo la recarga», «No abrir y avisar» si la recarga falla, «Antes de PB-T5» y la emisión condicional: se emite sin volver a él si esta comprobación da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material y la lista es la elegida (vacía: ninguna prueba existente sustituida).

**Reglas de lectura (obligatorias):**
- **El worktree, PROHIBIDO.** Nada en `<worktree>`, ni para leer.
- **El código y los documentos, solo por objetos git** desde el checkout principal: `git -C "<checkout principal>" show <sha>:<ruta>`, `diff`, `log`, `grep` y `ls-tree`. La base es el cierre de PB-T4d, `160e019d773ba9095e2d5858cd593a954b8eec67`. No leas los archivos en disco del checkout principal: solo sus objetos git.
- **Nada que compile o ejecute pruebas.** Python solo de lectura, escrito con la herramienta Write y ejecutado con `python -I <archivo>`; nunca `-c`, `python -` ni heredoc.
- **Ninguna escritura en el repositorio,** ni en el checkout principal ni en el worktree. Escribe solo en `S` (abajo), y tus guiones en una carpeta tuya nueva, `S\pbt4\_review_t4e\`.
- **Lo que no puedas verificar, dilo.**

**Las entradas.** `S` = `<scratchpad de la sesión>\hs65`.
- **El paquete:** `S\pbt4\I-64-f4b-pbt4-gate-contract-package-v55.md` (sha256 `e597790f…ae83`): D-PBT4-22, §4.5, §5.7, §6.5 con §6.5.1 y §6.5.2, el anexo V55-A, y lo que cambia en §0, §1, D-PBT4-01, D-PBT4-19, D-PBT4-20, §5.4, §5.5, §6.0, §7, §8.3, §8.5, §9, §10 y §11. La v5.4 anterior es `S\pbt4\I-64-f4b-pbt4-gate-contract-package-v54.md` (`e37843fc…fc63`). El paquete afirma que las demás 99 secciones, PB-T4a a PB-T4d enteras, no cambian: compruébalo.
- **Las decisiones del Owner,** literales:
  - `S\pbt4e\owner-pbt4e-list-decisions.md` (`d889d56b…c973`), la 15.ª tanda, con las cuatro preguntas;
  - su origen: `S\pbt4ccc\owner-pbt4c-code-decisions.md` (pregunta 2) y `S\pbt4\owner-pbt4d-list-decisions.md` (pregunta 2).
- **La propuesta de lista:** `S\pbt4e\list-proposal.md` (`bf2b5804…a7bf`).
- **Las decisiones de la sesión:** `S\pbt4\session-pbt4e-design-decisions.md` (`0c3bfc7b…0da2`). Confirman D-S17 (la recarga en el anfitrión), D-S18 (`EditAllStatus.NotBegun` reutilizado) y el `TaskId`, y evalúan R-T4-39. Ponlas a prueba, no las des por buenas.
- **Las piezas, en `S\pbt4\`:**
  - el GREEN: `pbt4e-green-v55.txt` (`5d26dc48…5407`, solo `src/`, 8 bloques EDIT en 4 archivos), `green_pbt4e_v55.py` y `apply_green_pbt4e_v55.py`;
  - el RED: `pbt4e-red-v55.txt` (`647e1853…033b`, solo `tests/`, 2 NEWFILE), `red_pbt4e_v55.py` y `apply_red_pbt4e_v55.py`.
- **Lo que hay en `S\pbt4\v55\`:**
  - las clases de las CT, en `ct\`;
  - las sondas, en `probe\`;
  - el generador de las piezas, `make_pieces_pbt4e_v55.py`, y el de la v5.5, `make_v55.py`, con sus salidas;
  - el traspaso `handoff-pbt4e.md`.
- **La precondición 6,** en `S\pbt4ecc\compile\`: `pbt4e-precondition6.json` (`ef33dca0…6d21`), `compile-check.md` (`1fd30380…505e`), `cov-p6-v55.txt`, `required-pbt4e.tsv`, `ui_filters_pbt4e_v55.txt`, los guiones y los registros de las corridas (`driver-r7*.txt` y las carpetas `r71-tr` a `r75-ct`).

**Qué comprobar,** con evidencia `ruta:línea` y `sha`:
1. **El GREEN frente a las decisiones del Owner.**
   - ¿Hace solo la recarga, sin aviso cuando recarga, igual que «Editar»? ¿Dirty, Conflict, sin marca, etc. siguen como hoy?
   - Si la recarga falla, ¿la ventana no se abre y el campo muestra lo mismo que «Editar», en todos los resultados que se alcanzan?
   - ¿Hay como mucho una lectura por gesto, y ninguna con el editor abierto?
   - ¿Hay algún cambio visible más (texto, estado, miembro)?
2. **D-S17 y D-S18.**
   - ¿El razonamiento se sostiene sobre el código de `160e019d`, en `EditAllOfPilot`, `TryReloadPilotEdit`, la marca de lectura, `BeginEdit`, `EditAll`, `Describe` y `EditAllNoticeOf`?
   - ¿Es seguro inferir la recarga de `view.Exists` (la vista se toma antes de `CommitField`), incluido el caso de error de `GetField`?
   - ¿Queda algún resultado inalcanzable mal tratado, como `NotNeeded` o `NotFound`?
   - ¿Y el caso del borde del texto pendiente?
3. **Las CT y el RED.**
   - ¿CT-PBT4E-01 a 04 fijan lo que dice el paquete, con sus 24 líneas?
   - ¿Caen en el RED por aserción (C04) y pasan en el GREEN?
   - ¿Pillarían un GREEN equivocado? Por ejemplo: recargar siempre, recargar un Dirty, abrir tras una recarga fallida, leer dos veces, o no mostrar el aviso.
   - ¿El control (solo el campo / solo el anfitrión) prueba lo que dice?
   - ¿Las clases nuevas tocan algo existente?
4. **La lista vacía.**
   - ¿De verdad ninguna prueba existente cambia de resultado o necesita sustituirse? Mira en particular las guardas de texto sobre `src/`, como `WorkspacePilotSurfaceGuardTests` y `WorkspacePilotEditGuardTests` (:574 y :1083-1086), y la línea `cola estado=NotBegun` de CT-PBT4C-03.
   - ¿Algo puede caer fuera de la lista y no se ha corrido? Por ejemplo, la UI fuera de los 19 filtros.
5. **La cobertura.** ¿`cov-p6-v55.txt` sostiene, línea a línea y rama por rama, que no baja nada y que lo nuevo queda cubierto? ¿El método es sólido?
6. **El contrato literal de §6.5:**
   - el alcance (`AllowedWriteScope` y `ForbiddenWriteScope`);
   - los invariantes y las paradas;
   - los `RequiredTests` (90 = 88 + 2) y sus `MinSelected`;
   - los totales de cada rama: Core base 19418 = 19015 + 0 + 403, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403; UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48;
   - la evidencia.

   ¿La base local casa con la CI del cierre de PB-T4d?
7. **R-T4-39** (`NotAppliedPreflight` en vez del conflicto): ¿la evaluación de la sesión, que no vuelve al Owner, es correcta? ¿Cambia alguna premisa de lo que el Owner eligió?
8. **¿Hace falta alguna decisión nueva del Owner?**

**SALIDA:** el informe en español en `S\pbt4\independent-review-pbt4e-check.md`, sin rutas locales absolutas. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta. Lleva:
- un veredicto: EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE;
- el recuento B/I/M;
- la tabla de hallazgos, con id, gravedad, dónde, qué y arreglo;
- lo que comprobaste y está bien;
- lo no verificado;
- **una línea BOUNDED en ASCII, en una sola línea que empiece por `> `**. Su forma, del precedente:

  > EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete vX (I-01, ...); sin decisiones nuevas del Owner; precondicion 6 sobre <sha8>: ...

  Si el veredicto es EMITIBLE sin importantes, la línea empieza por «EMITIBLE; B 0 / I 0 / M n; ...».

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes o importantes.

## 2. Recomprobación 1 (2026-11-23T03:18:26.230Z)

Recomprobación de CA-T4e (1, la v5.6, solo el diff). La sesión aceptó tu informe (EMITIBLE, B 0 / I 0 / M 5) y decidió, por M-01, tu opción (b): **D-S19**, la recarga detrás de las comprobaciones de la vista, con una línea más en las CT. Está en `S\pbt4\session-pbt4e-design-decisions.md` (sha256 `b21f2af4…e6d`), que también corrige M-03 y dice qué se hace con cada menor. El autor escribió la v5.6 y repitió la precondición 6 en copias nuevas. Además, el generador pidió que la v5.6 escriba literales dos textos heredados: PBT4E-C11 con la v3 de nc2 y P-01 con la Snapshot L.

**Las reglas** son las de tu primer encargo: el worktree, prohibido; el código, solo por objetos git en `160e019d`; nada que compile ni ejecute pruebas; Python con `python -I <archivo>`; y escrituras solo en `S`, con tus guiones en `S\pbt4\_review_t4e\`.

**Las entradas** (en `S`):
- **El paquete v5.6:** `pbt4\I-64-f4b-pbt4-gate-contract-package-v56.md` (`6b8939ae…f66a`).
- **Las piezas:**
  - el GREEN: `pbt4\pbt4e-green-v56.txt` (`a745c273…8158`);
  - el RED: `pbt4\pbt4e-red-v56.txt` (`d052f73d…c9e1`). La CT de Core tiene un NewFileSha nuevo, `9e849864…6b017`; la de UI no cambia.
  - Las herramientas y los fragmentos están en `pbt4\v56\`, y la v5.6 descartada en `pbt4\v56\historia\`.
- **La precondición 6,** en `pbt4ecc\compile\`: `pbt4e-precondition6.json` (`daea5984…c169`), `compile-check.md` (`549766eb…d426`) y `post-p6-v56-out.txt` (`36f88c52…b485`), con los registros de `/d/r76` a `/d/r83`.
- **El traspaso:** `pbt4\v56\handoff-pbt4e-v56.md` (`dc7e6775…45e0`), con sus desviaciones 1 a 6.

**Qué comprobar,** con evidencia `ruta:línea` y `sha`:
1. **M-01 → D-S19:**
   - ¿Nada evalúa `NeedsLazyReload` antes de las tres comprobaciones de la vista?
   - ¿Todos los estados distintos de «Clean y marcado» responden exactamente como en `160e019d`?
   - ¿Sigue habiendo como mucho una lectura por gesto, sin ninguna con el editor abierto?
   - ¿La línea nueva `marcado-sin-decidir` fija lo que dice? ¿Está medido que cae con el GREEN de la v5.5 (`/d/r76`) y pasa con el de la v5.6?
   - ¿INV-PBT4E-03, D-PBT4-22, D-S17 y R-T4-41 dicen lo mismo que el código?
   - Las 24 líneas que comprobaste, ¿siguen iguales?
2. **M-02:**
   - ¿Los dos límites están declarados en §4.5 y en la cabecera de `cov_p6_pbt4e_v56.py`?
   - ¿La corrida opcional está bien hecha y sostiene que las pruebas existentes conservan sus caminos? Core da 19418 = 19015 + 0 + 403, y la unión de los 18 filtros, 899 = 869 + 0 + 30 tres veces.
   - **Las 2 líneas que «bajan» en VARIA** (`RackViewBatchDialogPresenter.cs:37-38`): ¿es solo variación entre corridas, sin relación con el cambio? ¿La regla de la cobertura las trata bien?
3. **M-04:**
   - ¿El registro de `post-p6` nombra la `6b8939ae`?
   - El doble armado (`38efafdb` → `6b8939ae`, la desviación 3): ¿la única diferencia es la línea 9? ¿Todo lo demás de `post-p6` es igual byte a byte?
   - ¿Basta como «un solo armado» en su sentido, o hace falta algo más?
4. **Los literales nuevos:**
   - **PBT4E-C11:** ¿es el texto de PBT4D-C11 con exactamente los reemplazos declarados, con la v3 (ruta y SHA-256 comprobados en `160e019d`)? Revisa la lectura del autor de la aceptación puntual de PB-T4d (desviación 6).
   - **P-01:** ¿está con la Snapshot L solo en lo de PB-T4e, con PB-T4a a PB-T4d sin tocar?
   - **§6.5.2:** ¿están las autoridades de la v3 y de la L, con sus rutas en `160e019d`?
5. **«Lo que cambia» de la v5.6:**
   - ¿Sale del diff de secciones?
   - ¿Son ciertas las 109 secciones sin cambio, las 99 de PB-T4a a PB-T4d y las 10 más?
   - ¿Cambia algo que no se declare?
6. **Los totales, los `RequiredTests` (90) y los filtros:** ¿siguen iguales a la v5.5, salvo lo declarado? ¿La base sigue casando con la CI del cierre de PB-T4d?
7. **Las desviaciones 1 a 6 del traspaso:** ¿alguna afecta a la medida?
8. **¿Hace falta alguna decisión nueva del Owner?**

**SALIDA:** añade al MISMO informe, `S\pbt4\independent-review-pbt4e-check.md`, una sección al final, sin tocar lo de antes, con el encabezado exacto `## Recomprobación 1 (v5.6, solo el diff)`. Lleva:
- una línea `**Veredicto:** EMITIBLE, B n / I n / M n (…)`;
- la tabla de hallazgos nuevos (de M-06 en adelante);
- lo que comprobaste de M-01 a M-05;
- lo no verificado;
- y, como última línea del archivo, **la cola BOUNDED en ASCII, en una sola línea que empiece por `> ; recomprobacion v5.6: `**, con la forma de las colas de `independent-review-pbt4d-check.md` (:364 y :527).

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes o importantes.
