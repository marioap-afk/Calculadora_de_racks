# PB-T4c: lo que el generador necesita de la v5.0 (estado del seco provisional sobre la v4.9)

**Qué es:** el estado del generador de PB-T4c (`make_pbt4c_contract.py`, fase 1, provisional) tras su seco por objetos sobre el cierre de PB-T4b (`6adff90d`, F4B-E53). Lee la v4.9 (`043f196d…`), donde PB-T4c está en prosa (§4.3, §5.3 y §6.3), y la lista de 4 (`pbt4c-substitution-list-4.tsv`), junto al paquete. Recoge lo que ya comprueba y lo que espera a la v5.0.

## Lo que ya comprueba (verde en el seco)

- **El anclaje:** la revisión desciende de X y BASE de la emisión de PB-T4b (`f5ff00ee` y `8f783df5`), de su RED (`c1e4f458`), de su reparación (`9b2d1893`), de su GREEN (`c065080d`) y de su cierre (`6adff90d`); `src/` y `tests/` son los del GREEN.
- **La numeración:** F4B-E54 para la emisión y la evidencia 283 (X) y 284 (BASE).
- **La línea base C:** la CI del GREEN de PB-T4b (`44903818806`), Core 19 605 = 19 147 + 0 + 458 y UI 2 481 = 2 433 + 0 + 48.
- **El contrato de PB-T4b:** las 17 paradas estándar, la primera `ExpectedEvidence` y PBT4C-C11, que sale de PBT4B-C11.
- **La Snapshot K y los sustitutos de nc1 y nc2,** con `--task PBT4`.
- **La lista de 4:**
  - empieza por las dos filas «PB-T4c» del archivo de todas las listas (PEG:155-163 y CT-PBT1B-15), en su orden;
  - añade exactamente CT-PBT4B-20 (`owner-pbt4c-list-3-decision.md`) y CT-PBT2B-06 (`owner-pbt4c-list-4-decision.md`);
  - cada una es un `[Fact]` sin `Skip`, en su rango;
  - la de 3 (`d83e0a81…`) queda como historia.
- **Los filtros heredados de PB-T4b (77), con los cuatro `Skip`.** Coinciden con lo que midió el autor:
  - n.º 7, `PushBackSharedReferenceGuardTests`: de 2 a 1;
  - **n.º 12, `RackCad.Tests.Workspace`, de 2 718 a 2 716:** selecciona PEG y CT-PBT4B-20. El hallazgo del primer seco no se sostiene: comparaba con punto final, y el filtro es `FullyQualifiedName~`, un prefijo sin punto, como en PB-T4b;
  - n.º 36, `PushBackWorkspaceApplyCloseTests` (UI): de 4 a 3;
  - **n.º 43, `WorkspaceFullCopyGuardTests`, de 1 a 0:** su fila sale (PENDIENTE hasta la v5.0).
- **Los totales,** desde la lista (3 `Skip` en Core y 1 en UI, cada uno con su equivalente) y la aritmética de la v4.9 (11 CT de Core y 4 de UI; 9 y 4 `Failed` en el RED). **Son los que midió el autor:**
  - GREEN: Core 19 619 = 19 158 + 0 + 461, y UI 2 486 = 2 437 + 0 + 49;
  - RED: Core 19 619 = 19 149 + 9 + 461, y UI 2 486 = 2 433 + 4 + 49.

## Lo que falta de la v5.0 (PENDIENTES del seco)

1. **El sha256 de la lista de 4** (`LIST_SHA`; hoy el archivo da `9bd4c17a…`), y que §5.3 lo diga con K_C = 4 y su aritmética.
2. **§6.3.1, los textos literales del contrato,** como §6.2.1 de PB-T4b:
   - `TaskId` y `Objective` ASCII, con la v5.0 y CT-PBT4C-01..15;
   - el `AllowedWriteScope` ruta a ruta, con su número. Hoy hay 14 rutas resueltas con las abreviaturas, entre ellas los dos archivos nuevos de la lista. Faltan las 4 nuevas de `src/` sin ruta completa (D-S9 renombra `IWorkspaceFullEditorHost` a `IWorkspaceFullEditHost`) y las clases nuevas de `tests/`;
   - `UNIT_CHANGE` y la regla de `ForbiddenWriteScope`;
   - los `Invariants` y las paradas PBT4C-C*;
   - la tabla de `RequiredTests`: los heredados con lo que dejan los `Skip`, sin el n.º 43, y los nuevos, con los filtros por clase exacta de los equivalentes (D-S8);
   - la `ExpectedEvidence`, con [13] y [14].
3. **§6.3.2, lo que necesita el generador:**
   - las autoridades: hoy hereda las 72 de PB-T4b y añade, provisionalmente, las dos decisiones del Owner, la de la sesión, la lista y las tres piezas;
   - las piezas por sha256: `pbt4c-skeleton-v50.txt`, `pbt4c-green-v50.txt` y `green_pbt4c_v50.py`;
   - la precondición 6, `pbt4c-precondition6.json`: su esquema y lo que mide;
   - CA-T4c: `BOUNDED_SECTIONS`, la versión sobre la que corre y la línea BOUNDED.

## La custodia (issue_pbt4c.py)

- **371 iguales, 0 cambiados, 42 nuevos y 0 sin clasificar.** Se clasifican así:
  - las decisiones del Owner sobre PB-T4c van a `coordinator/`;
  - la decisión de la sesión, a `pbt4/`;
  - las listas, el esqueleto, el GREEN y las sondas `pbt4c-*`, byte a byte;
  - los guiones `*_v50.py` y `make_list_pbt4c_<n>.py`, y los archivos de `v50/`.
- **Quedan sin clasificar cuatro subcarpetas de `v50/`,** que van por el sha256 de su conjunto hasta que la sesión decida:
  - `eq-tree/`: 4 archivos;
  - `eq-tree4/`: 5;
  - `out-probe-skips-r5a/`: 5;
  - `variant-rename/`: 98. Tiene rutas de más de 260 caracteres en local, así que leerla necesita el prefijo de rutas largas. Si se custodia, hay que comprobar sus rutas en X.
- **El barrido de rutas con unidad:** en lo nuevo hay 3 etiquetas `/<letra>/` de copias exportadas, que van tal cual.

## Lo que se arrastra de PB-T4b

- **Al emitir, todo se lee de la revisión X,** y los pendientes que dependen de la precondición 6 o de CA-T4c se deciden después de leerlas.
- **La carpeta de la precondición 6 va en una ruta corta,** `cc4c/`.
- **En lo que escribe la emisión, cero rutas con unidad.**
