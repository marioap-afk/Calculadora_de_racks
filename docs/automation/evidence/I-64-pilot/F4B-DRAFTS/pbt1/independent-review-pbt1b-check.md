> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. El worktree se cita como `<worktree-de-I-64>` y las rutas del repo son relativas.

# I-64 F4B — Comprobación acotada de PB-T1b: el paquete v2.5, los anclajes sobre `5043b49a` y el generador del contrato

**Veredicto:** **EMITIBLE TRAS CORRECCIONES — B/I/M = 0 / 1 / 5. Decisiones materiales nuevas para el Owner: ninguna.**

- **Lo que se sostiene:**
  - los anclajes sobre `5043b49a`: `src` sin cambios, las anclas en su sitio, G-L1 en 0 (también con una implementación mía independiente) y la línea base de la CI medida con `gh`;
  - la regla de c-09 (D-PBT1-09b): reproduce el comportamiento de hoy, sigue el patrón de D-PBT1-09 y deja RACKEDITAR igual;
  - OWNER-3 y OWNER-4, tal cual y sin regla propia;
  - el generador: su JSON coincide con el §6.2 en todo lo que pide el encargo;
  - D2, completa y comprobable.
- **El importante (PBT1BC-I01):** CT-PBT1B-17 fija la regla de c-09 como función pura en sus dos ramas, pero no fija de punta a punta la rama «caja en lb» a través de `AdoptCarriers`.
  - Esa rama es la única razón de que la regla reciba `current`.
  - Ninguna prueba de PB-T1b ni de PB-T1c detectaría un cableado equivocado de `current`, porque en la ventana `ReadInputs` sobrescribe la unidad en cada pasada.
  - **La corrección es solo de texto,** una línea más en CT-PBT1B-17, con lo esperado tomado de la fila (f) del paso 1. No pide ninguna decisión.
- **Con esa corrección** (y, si se quiere, los cinco menores), PB-T1b se emite sin volver al Owner (F4B-E09, pregunta 4; F4B-E11).

## 1. Método y límites

**Las entradas** (en `<carpeta-del-paquete>`):
- la v2.5 (`T25:n`) y su registro de cambios v2.4 → v2.5;
- `pbt1-measurements.md`, apartado P, con `pbt1b-anchors-out.txt` y `measure_pbt1b_anchors.py`;
- `make_pbt1b_contract.py`, `check_v25_skeleton.py` y `pbt1b-instructions.md`;
- para comparar, la v2.4 custodiada en HEAD (`git show HEAD:docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`), el contrato de PB-T1a custodiado y las comprobaciones acotadas anteriores.

**Contrastado en `<worktree-de-I-64>`,** HEAD `5043b49a`, siempre con `git -C`, sin checkout y en solo lectura. `git status --short`, vacío antes y después.

**Las decisiones del Owner, leídas en HEAD:**
- `owner-pbt1a-decisions.md` (F4B-E14) y `owner-pbt1a-nc1-decisions.md` (F4B-E15);
- la vuelta `pbt1a/owner-return-pbt1a.md`;
- las entradas nuevas de `docs/ideas-futuras.md` (c-09, OWNER-3 y OWNER-4).

**El código leído** para c-09:
- W:95-160, W:266-330, W:383-440, W:832-890 y W:3300-3335, y la conexión de `WeightUnitBox` en WX:274;
- DA:165-222 y DA:268-276; DDA:40-70; `DynamicDepthGeometry.Matches`;
- `PalletSpecification`, `PushBackEditorInputs.NewDesign()` y EL:30-60 y :229-232;
- CT-PBT1A-02 (`PushBackStateNetRackControlsTests.cs:133-207`), S0:95-130 y `PushBackStateNetSupport.cs:1-80`;
- `HeaderBatchContractTests.cs:493-563`, para ver el alcance de C-10.

**Lo que ejecuté:**
- **El generador,** solo con la orden literal del encargo (`… --dry --print`). Su salida es idéntica a `make_pbt1b_contract-dry.txt` (`allowed 12 forbidden 213 (src 117 tests 79 root 17) inv 15 stops 33 required 23 evidence 8 authorities 46 anchors-ok 39 skeleton-in-HEAD v2.4 bytes 53912 sha256 E5062453…3CF86`).
- **Cuatro scripts míos de solo lectura** en `pbt1/rv-b/`, escritos con la herramienta de archivos y ejecutados con `python -I`:
  - `indep_gl1.py`: las reglas de G-L1 de la v2.4, escritas desde el texto de T25:629-667 y no desde `remeasure_gl1_v24.py`, sobre HEAD y sobre las autopruebas;
  - `dump_pbt1b_json.py`: ejecuta el generador en seco y vuelca el JSON que escribiría, sin tocar el worktree;
  - `cmp_contracts.py`: compara ese JSON con el contrato custodiado de PB-T1a;
  - `skel_v25_vs_gen.py`: comprueba el esqueleto, el título y el `Objective` de la v2.5 contra el generador.
- **`gh`, solo de lectura:** `run list`, `run view --json jobs` y `run view --job --log`, filtrado a las líneas de resumen.

**Incidencia de procedimiento, declarada.** Una de mis órdenes de Bash llevaba al final, por error, `python -I -` con un heredoc vacío. No había programa: no leyó ni escribió nada y se quedó esperando la entrada hasta que lo detuve. Incumple la letra de la regla («nunca con un heredoc»). No se repitió, y no hubo ninguna otra ejecución de Python fuera de los scripts de archivo y del `--dry`.

**Límites:**
- Nada se compiló ni se ejecutó. Lo que digo de WPF (`Selector.CoerceSelectedItem`) y del RED sobre el esqueleto es lectura.
- No leí el `config.toml` ni el binario de Codex (precondición 6, de la sesión).
- No volví a contar los 997 y 320 miembros de la heurística de G-L1; mi comprobación usa alcances léxicos.

## 2. Los anclajes (§6.0, precondición 4)

| Qué | Lo que comprobé | Juicio |
|---|---|---|
| **Identidad** (V25-01) | HEAD `5043b49a`; worktree limpio; `git log` y los `diff` de `src` vacíos los reproduce el generador con sus `assert`, que pasaron en mi `--dry` | Se sostiene |
| **Las anclas** (V25-02; 104) | En el código: W:148-149, :392-396, :399, :838-840, :850-853, :868, :3320-3327; DA:169-176, :183, :211-218, :273; DDA:44-61; `ClonePallet` (DA:573-576). Las 39 de ES, RMES, CS, DA y CA, por los `assert` del generador. Las citas de CT-PBT1B-17 que el registro P-1 no lista, también en su sitio: S0:108 (`EditInner`), :118 (`StructureNode`), :122-123 (`Persist`) y `PushBackStateNetSupport.cs:55` (`Leer`) | Se sostienen |
| **G-L1** (V25-03) | Mi implementación independiente: **0 coincidencias** en los 47 archivos de CT-PBT1B-14 y en W y WC; los mismos alias de lectura que el registro (`structure`, `signature`, `baseline` ×2, `rebuildShape`; `outputIsBlocked`, `reconciliation` y el lambda `module`). **Las catorce autopruebas saltan, cada una por la regla que dice D-PBT1-15** (a, f1, f2, d por la 1; i y f3 por la llamada mutadora; b, e, h1 y h2 por la regla 2; c, g1 y g2 por la 1b; j por la 1c). `L1-alias2`, `L2-anidado` y `L3-otro-metodo` no saltan, como declara el límite | **G-L1 da 0 de verdad y sus autopruebas saltan** |
| **Los recuentos** (V25-04) | `PushBackStateNet` 14 = CT-PBT1A-01 a 14; UI `PushBack` 556 = 542 + 14; el filtro `PushBackEditableState` 13 = CT-02 a 11, 13, 16 y 17; 23 filtros en la tabla de T25:1076-1090 | Correctos |
| **La CI** (V25-05) | `gh`: `44361363873` sobre `5043b49a`, `push`, 4 de 4 en `success`; UI `Passed: 2415, Skipped: 21, Total: 2436`; Core `Passed: 19070, Total: 19070`. Es la previsión del cierre de PB-T1a (`44338475965` sobre `73d30982`, `success`) | Correcta. La previsión del GREEN (Core 19 086; UI 2 437 / 21 / 2 416) cuadra con 13 + 1 + 2 en Core y 1 en UI |

**Las diferencias «no materiales»:** las revisé una a una (V25-01 a 05, 07, 08 y 10 a 17). Ninguna es material:
- **V25-13 es cierta:** H-12 ya cuenta las tres clases de Core (registro H, `:155`), y el generador obtiene 15 + 3 = 18 del contrato de PB-T0.
- **V25-16 es cierta:** la copia de trabajo de la v2.4 difería de `95c8126b` solo en la línea «Emitida» y en la ruta del anexo L1-C.
- **V25-07 (la rama de la caja en «lb») no es una decisión:** es medición (§3).

## 3. c-09 (V25-06, F4B-E14, pregunta 2)

**El mecanismo de hoy, leído en el código, coincide con HT1-15 corregido (T25:241-249):**
- `WeightUnitBox` solo ofrece «kg» y «lb» (W:148-149).
- `generalPallet.WeightUnit` guarda la unidad cargada (W:393) y solo la lee su propia sustitución (W:840).
- Los inputs llevan la unidad de la caja (W:852).
- `SamePallet` no mira la unidad (DDA:57-60).
- Sin reconstrucción, DA copia la línea base (DA:273). Al reconstruir, construye con la unidad de los inputs (DA:176, DA:218).

**`ResolveWeightUnit` reproduce la caja como regla explícita, igual que D-PBT1-09 con `PostBox`:**
- **Las dos primeras filas son la siembra de W:395:** en blanco → «kg»; «kg» o «lb» exactos → esa unidad. `Items.Contains` compara cadenas con `Equals` ordinal, así que «KG» no entra.
- **La tercera reproduce el rechazo de la caja.**
- **El valor de `current` reproduce lo que tiene la caja:** en la ventana, `current` (la unidad del estado antes de sustituirlo) es siempre la selección de la caja o «kg». El estado nace con «kg» (`PushBackEditorInputs.cs:19`, igual que W:149), y en PB-T1c `ReadInputs` escribe la de la caja en cada pasada (D-PBT1-13 (5)).
- **La salida coincide en los dos casos:** el ensamblador no cambia, y `BuildInputs` lleva la unidad del estado como W:852.
- **El caso `current` ∉ {kg, lb} → «kg»** no se alcanza en la ventana. Es el cierre natural de la función (W:149, y W:852 con caja vacía) y no tiene efecto en RACKEDITAR. No es una decisión nueva.
- **El orden de la carga no cambia:** es una expresión dentro de `AdoptCarriers`.

**RACKEDITAR no cambia:**
- PB-T1b no toca W, WC, WX ni el Plugin (INV-PBT1B-01).
- CT-PB0-17 y CT-PBT1A-02 siguen en las redes (INV-PBT1B-07).

**CT-PBT1B-17 y CT-PBT1A-02:**
- **La parte 2 de CT-PBT1B-17 son, literalmente, las líneas 198-202 de CT-PBT1A-02,** sin el sufijo ` descartes=<n>` en las tres que lo llevan. Comprobado carácter a carácter.
- **Los gestos equivalentes del estado dan esas líneas por lectura,** también `caja=[kg]`, porque `current` vale «kg» en un estado recién cargado.
- **La parte 1 fija las dos ramas de la regla pura.**
- **Hueco: la rama «conserva» no se ejerce de punta a punta en el estado** (PBT1BC-I01). Todos los gestos de la parte 2 parten de `current` = «kg», donde las dos ramas coinciden. El paso de `current` dentro de `AdoptCarriers`, la única razón de que la regla tenga dos argumentos, no lo fija ninguna línea. Por ejemplo, pasar `"kg"` fijo o la unidad cargada como `current`:
  - pasa CT-PBT1B-17;
  - pasa CT-PBT1B-01, porque los 17 diseños guardan «kg»;
  - y pasa CT-PBT1C-01 y 02, porque en la ventana `ReadInputs` sobrescribe la unidad antes de cada `BuildInputs`.

  El error solo aparecería en el borrador de PB-T2 o PB-T4, justo donde la regla tiene que valer sin la caja.

**La fila (f) del paso 1 es legítima, como N-M-03 con c-02:**
- Mide un comportamiento de hoy que CT-PBT1A-02 no distingue: desde «kg», «conserva» y «vuelve a kg» dan lo mismo.
- Las dos ramas están fijadas de antemano, y cada una reproduce «como hoy».
- Cualquier otra línea es PBT1B-C14, que para antes del RED y vuelve a la sesión.
- La variante `<null>` de la rama «kg» es inocua: la salida es la misma (W:852). Además, CT-PBT1A-02 ya muestra que rechazar desde «kg» no deja la caja vacía.
- **El guion es el correcto:** cargar «ton», elegir «lb», «Restaurar» y `DepthBox=54`. Elegir «lb» no reconstruye, así que la última computación válida conserva «ton». «Restaurar» recarga entonces «ton» con la caja en «lb», y `DepthBox=54` reconstruye con la unidad de la caja.
- **Falta escribir la carga** (PBT1BC-M01). Si un Worker no la hiciera, la línea `f WeightUnitBox.SelectedItem=kg salida=kg` saldría igual y fijaría en silencio la rama «kg».

**HT1-15 y el §5.5 están bien corregidos:**
- T25:241-249 y T25:890-891, :908.
- La fila R-6 del §8.4.
- Queda una frase de §8.5 (T25:1332) que hoy se lee como contradicción (PBT1BC-M05).

**PB-T1c no cambia más de lo que dice V25-10:**
- D-PBT1-13 (5) ya escribía la unidad de la caja en el estado desde la v2.4.
- `generalPallet.WeightUnit` no tiene más lectores que W:840, así que retirar W:393 no cambia nada observable.
- El `MinSelected` de §6.3 pasa a 13, y CT-PBT1C-02 no cambia de texto.

## 4. OWNER-3 y OWNER-4 (F4B-E14, pregunta 3)

**Se reproducen tal cual, sin regla propia, como dice V25-09:**
- **«lb» elegida:** el estado la lleva a `BuildInputs` como W:852, y el ensamblador la ignora hasta que algo reconstruye. Es la línea 199 de CT-PBT1A-02, que CT-PBT1B-17 también fija en el estado.
- **`PalletsDeepBox`:** `AdoptCarriers` adopta `Math.Max(2, …)`, que es la siembra de W:396 y lo que relee W:853. El ensamblador lo usa igual (DA:183).
- **El portador de botas:** lo añade `Safety_Click` (W:2682), que PB-T1b no toca. El estado solo transporta `SafetySelections` y el `Editor`, donde vive `DefensePieceId`.

**Los fija la red de PB-T1a** (CT-PBT1A-02 y 11), y están en `docs/ideas-futuras.md`.

## 5. El generador

| Qué | JSON en seco frente al §6.2 | Juicio |
|---|---|---|
| Identidad y forma | Las mismas 19 claves que el contrato de PB-T1a, en el mismo orden; `TaskId`, `Schema`, `Unit`, `Gate`, `RoutingEnforcement` y `CorrectionsAuthorized` correctos | Coincide |
| `Objective` | Idéntico al de T25:1071 (comprobado por script) | Coincide |
| Autoridades (46) | Las 39 de PB-T1a, salvo W y WC (que eran `UNIT_CHANGE` de PB-T1a), más F4B-E14, F4B-E15, la vuelta de PB-T1a, su cierre, ES y RMES (`UNIT_CHANGE`), las dos rutas nuevas de `CUSTODY` (la salida de los anclajes y este informe; el paquete y el registro ya estaban) y este contrato | Coincide. Faltan dos autoridades del §1 (PBT1BC-M03) |
| `AllowedWriteScope` (12) | ES, RMES, los cuatro nuevos de Application, los cinco de Core y la clase de paridad | Coincide |
| `ForbiddenWriteScope` (213) | `src/`: 117 por `cover()`, con CS, DA, CA, EL, `Pilot/`, `Workspace/`, Domain, Plugin y UI, y sin ninguna permitida debajo. `tests/RackCad.Tests/`: las 15 prohibidas y las 3 clases de PB-T0 (18). UI: 53 + 7 + `Workspace/` (61). Raíz: 17. Todas existen | Es el método de [PKC-C] y da 213 |
| Invariantes (15) | Los propios (01, 02, 03, 04, 04b, 05, 06 y 07c) y los comunes (07 a 12 y 14). INV-PBT1B-02 lleva el esqueleto exacto; INV-03, `ResolveWeightUnit`; INV-06, las filas (a) a (f); INV-12, las tres comprobaciones de F4B-E15 | Coincide |
| `RequiredTests` (23) | Los de T25:1076-1090. Los 12 heredados son iguales a los de PB-T1a (`assert`), y los prefijos nuevos están libres | Coincide |
| Paradas (33) | **Las 17 estándar, idénticas a las de PB-T1a. C08, C11 y C12, idénticas a las de PB-T1a con el prefijo cambiado** (comprobado por script). C01 a C03, C05 a C07, C09 y C10 (con c-09, OWNER-3 y OWNER-4); C04 del esqueleto; C13; C14 con las dos ramas; C15; y C17 con el texto literal del Owner | Coincide |
| `ExpectedEvidence` (8) | El §6.1 adaptado: el paso 1 con (a) a (f) y las dos ramas; el RED con `tests` y `ui-tests` en `failure`; la tabla CT → aserción; las autopruebas de las guardas; el GREEN con 2 437 / 21 / 2 416 y 19 086; la verificación con F4B-E15 | Coincide |
| Celdas e `IssuedBy` | Las celdas, iguales a las de PB-T1a y PB-TC2 (vencen el 2026-12-29). `IssuedBy` cita F4B-E09, F4B-E11, F4B-E14, F4B-E15, F4B-E16 y F4B-E03 | Coincide |
| El `assert` del esqueleto | El esqueleto de la v2.5 local es `SKELETON`, y el de la v2.4 de HEAD es `SKELETON` sin el fragmento de c-09 (comprobado por script). La expresión del título reconoce «v2.5» | Correcto |
| `CUSTODY` | El paquete, el registro, `pbt1b-anchors-out.txt` y este informe. Al escribir, exige los cuatro en HEAD, el esqueleto y el título de la v2.5 y el apartado P | Correcto. `check_v25_skeleton.py` y la salida del `--dry` no figuran en la custodia (PBT1BC-M04) |

**Una advertencia práctica.** El generador ata la versión: el `assert` de `make_pbt1b_contract.py:505` exige el título «v2.5», y los textos citan «v2.5» en `Objective`, `CUSTODY` e `IssuedBy`. Si las correcciones de este informe se aplican como v2.6, hay que cambiar a la vez el generador y el `Objective` del §6.2. Si se aplican dentro de la v2.5, con su registro de cambios, nada más cambia: ninguna corrección toca el esqueleto, los recuentos ni la CI.

## 6. D2

**Lo de D2 que toca a PB-T1b sigue completo y comprobable tras la v2.5:**
- CT-PBT1B-06, 07, 08, 14, 15 y 16;
- G-L1 con las reglas de la v2.4;
- INV-PBT1B-07c y C17.

**El diff v2.4 → v2.5 no toca D-PBT1-15** ni el texto de esos CT, de INV-07c ni de C17. La C17 del generador lleva el texto literal del Owner, idéntico al de PB-T1a.

**c-09 no roza D2:**
- `ResolveWeightUnit` es pura y trabaja con cadenas.
- `AdoptCarriers` solo lee el `Pallet` anterior del estado.
- Ningún camino nuevo lee ni escribe la línea base ni el informe.

**G-L1:** las reglas dan 0 y sus autopruebas saltan (§2), medido de forma independiente.

**CT-PBT1B-15:** el Plugin no cambió desde `b5c09f0b`, así que sus 14 archivos y sus siete asignaciones siguen (N-11).

## 7. PB-T1b por sí misma

**El RED con el esqueleto inerte:**
- El esqueleto declara toda la API, con `ResolveWeightUnit`, así que las pruebas compilan.
- Un `PushBackEditorState` nuevo tiene un frente por defecto, así que `Pasada` sobre el esqueleto calcula sin lanzar (IRT1-1 ya lo vio).
- CT-PBT1B-17 falla por aserción: por ejemplo, `regla current=[kg] requested=[ton] -> [ton]` frente a `[kg]`.
- **Una salvedad, anterior a la v2.5 (PBT1BC-M02):** las líneas esperadas de CT-PBT1B-16 son invariantes (`huellas-iguales=True`, `comparar-invariante=True`) que el esqueleto también cumple. Falla en el RED solo porque la receta F-01 no encuentra un separador `B:` en el rack por defecto, y la regla «EXCEPCION» lo convierte en una línea. No falla porque «`Copy` da un estado vacío», como dice su columna. Si la receta se escribe tolerante, CT-16 pasa sobre el esqueleto (C04).

**El criterio de INV-PBT1B-02** es mecánico y completo: todos `Failed` por aserción, `Failed == Selected`, con 13, 1 y 1. Coincide entre el paquete y el generador.

**Las dos guardas de D2 pasan en el RED:**
- G-L1 no ve ninguna ficha en los cuatro archivos del esqueleto.
- El Plugin no cambia.
- Discriminan por sus autopruebas, como pide INV-F4T3A-13.

**La previsión de la CI:**
- **En el RED:** `tests` y `ui-tests` en `failure`, y los dos builds en `success`.
- **En el GREEN:** Core 19 086 y UI 2 437 / 21 / 2 416. Es coherente con la línea base medida.

**Ningún obstáculo para emitir ni para cerrar,** una vez corregido PBT1BC-I01.

## 8. Los hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **PBT1BC-I01** | **IMPORTANTE** | **CT-PBT1B-17 no fija de punta a punta la rama de c-09 con la caja en «lb».** La parte 1 prueba la función pura. La parte 2 solo parte de estados con `current` = «kg», donde las dos ramas coinciden. El cableado de `current` en `AdoptCarriers` («la unidad del `Pallet` del estado justo antes de sustituirlo») no lo fija ninguna prueba de PB-T1b ni de PB-T1c, porque en la ventana `ReadInputs` sobrescribe la unidad. Un error ahí cerraría PB-T1b y PB-T1c en verde y rompería «como hoy» en el borrador de PB-T2 y PB-T4 | T25:481, :872; D-PBT1-13 (5) (T25:534); W:852; los 17 diseños en «kg» (P-2) | **Añadir a la parte 2 de CT-PBT1B-17 una sexta línea, el equivalente en el estado de la fila (f):** otro estado con la carga «ton» y su `Pasada`; `Pallet` con la unidad «lb» y `Pasada`; `Restore(<diseño de esa computación>, resolver)` y `Pasada`; `caja=` es `Pallet.WeightUnit`; después `Pallet` con `Depth` 54 y `Pasada`. La línea es `restaurar ton lb caja=[<u>] [PalletDepth=54 PalletWeightUnit="<u>" PalletWeight=1000]`, con `<u>` = `lb` en la rama «conserva» y `kg` en la rama «kg», fijada por la fila (f). En el generador, nada: el CT vive en el paquete. No cambia el esqueleto, ni los recuentos, ni la CI, ni la clase. **No es una decisión:** es la misma regla, con lo esperado tomado del paso 1 |
| **PBT1BC-M01** | MENOR | **La fila (f) del paso 1 no nombra la carga.** Dice «con `pb-01-base` y su `PalletWeightUnit` cambiado a `ton`, en una ventana nueva, `WeightUnitBox` = lb, «Restaurar»…». Sin `Cargar` antes de elegir «lb», «Restaurar» recarga el diseño por defecto («kg», que la caja sí ofrece). Saldría `f WeightUnitBox.SelectedItem=kg salida=kg`, una línea válida de la rama «kg», y se fijaría la rama equivocada sin pasar por PBT1B-C14 | T25:1112; `make_pbt1b_contract.py:255-258` | En el paquete y en INV-PBT1B-06 del generador: «`Cargar(w, <ese sobre>)` en una ventana nueva; después `WeightUnitBox.SelectedItem = "lb"`…». Mejor aún, que la línea se compruebe a sí misma, como F-03 (N-M-08): añadir `carga=[<caja tras cargar>]` y `antes=<unidad de la salida tras «Restaurar»>`, con `carga=[kg]` y `antes=ton` esperados en las dos ramas |
| **PBT1BC-M02** | MENOR | **La razón del RED de CT-PBT1B-16 no es la que dice su columna.** Sus líneas esperadas son invariantes que el esqueleto también cumple. Falla en el RED solo por la línea «EXCEPCION» de la receta F-01 (no hay separador `B:` en el rack por defecto). Es anterior a la v2.5, pero la tabla «CT → la aserción que falla en el RED» de la evidencia la contradiría | T25:871 (columna «Falla en el RED porque»); §5.0 (F-01, la regla «EXCEPCION») | Cambiar la columna de CT-16 a «la receta F-01 no se completa sobre el esqueleto (línea EXCEPCION)», o añadir a CT-16 una línea que el esqueleto no puede dar, por ejemplo `copia-comparte-linea-base=True` en el bloque «comparar», literal por D2 |
| **PBT1BC-M03** | MENOR | **El contrato cita INV-F4T3A-13 y el régimen del esqueleto inerte sin su autoridad.** El §1 lista los contratos de F4 con API nueva (`I-64-f4-t2-gate-contract-package.md:282`, `I-64-f4-t3-gate-contract-package.md`), que son el patrón del RED de PB-T1b. No están en las autoridades del generador. Existen en HEAD | T25:192; `make_pbt1b_contract.py:396-431`; INV-PBT1B-02 | Añadir los dos (y el de F4-T3C, si se quiere el §1 completo) a `EXISTING`. Pasan a 48 o 49 autoridades, y hay que ajustar el `assert` de `:477` |
| **PBT1BC-M04** | MENOR | **La custodia no recoge todo lo que sostiene el registro P-7.** `check_v25_skeleton.py` y la salida `make_pbt1b_contract-dry.txt` no figuran en la custodia del §6.0. PB-T1a sí custodió sus salidas `--dry` | T25:959; P-7 | Añadirlos a la lista de lo que se custodia en `F4B-DRAFTS/pbt1/` con la emisión de PB-T1b. No son autoridades del contrato |
| **PBT1BC-M05** | MENOR | **Precisión del texto.** (a) D-PBT1-09b enumera los disparadores de la reconstrucción como «un cambio de fondo, frente, alto o peso del pallet, o del número de fondos…». `MustRebuild` incluye además el `Matches` del reparto de fondos (DDA:48), que depende de la envolvente de cada frente (`DepthStartBox`, el fondo de la celda). Y «número de fondos» puede leerse como `PalletsDeepBox`, que según OWNER-3 no mueve la salida. (b) §8.5 dice que HT1-15, el §5.5 y CT-PBT1C-02 «no ganan ninguna excepción»: es cierto para `RackHeaderHeightBox`, pero hoy choca con c-09 | T25:484, :1332; DA:211-213; DDA:44-48; `DynamicDepthGeometry.Matches` | (a) «una pasada que reconstruye (DA:211-213: un «Restaurar estándar» o un «Restaurar módulo» confirmados, o `MustRebuild`: otro pallet sin contar la unidad, u otro reparto de fondos)». (b) «…no ganan ninguna excepción por C14» |

## 9. El veredicto

**EMITIBLE TRAS CORRECCIONES — B/I/M = 0 / 1 / 5.**

**PBT1BC-I01 se resuelve con texto**, una línea más en CT-PBT1B-17 con su receta, sin volver al Owner. Los cinco menores pueden ir en el mismo cambio. Después, PB-T1b se emite por F4B-E09 (pregunta 4) y F4B-E11, con F4B-E14 aplicada.

**Si la corrección se hace como v2.6,** hay que cambiar a la vez el generador (§5) y el `Objective`.

**Decisiones materiales nuevas para el Owner: ninguna.**
- **c-09** ya la decidió el Owner (F4B-E14), y la v2.5 la aplica con el patrón de D-PBT1-09 sin ir más allá:
  - la regla reproduce la caja;
  - la fila (f) es medición con dos ramas fijadas y PBT1B-C14;
  - el caso `current` fuera de {kg, lb} es el cierre de una entrada que la ventana no alcanza;
  - RACKEDITAR no cambia.
- **OWNER-3 y OWNER-4** se reproducen tal cual.
- **F4B-E15** se aplica tal como la dio el Owner.
- **Ninguna corrección de este informe** cambia el alcance, el orden de la carga, D2 ni RACKEDITAR.
