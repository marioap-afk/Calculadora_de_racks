# Encargo de la comprobación acotada de PB-T4a (CA-T4a) (enviado por la sesión al revisor del paquete de PB-T4 como mensaje; texto exacto)

## 1. CA-T4a, sobre el contrato en seco y la v4.2 (2026-11-20T18:18:01.183Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T4a (CA-T4a)** antes de emitirla.

**La autorización:** las decisiones del Owner sobre PB-T4, pregunta 4: «Sí, con condiciones (Recomendado)». La condición:
- 0 bloqueantes y 0 importantes sin resolver;
- la remedición sobre el cierre de PB-T3c no cambia nada material;
- la lista emitida es la que vio el Owner o una parte de ella. Para PB-T4a, la lista es 1: CT-PBT3A-12.

Las dos tandas de decisiones están, literales, en `hs65\pbt4\owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`.

**Las reglas de siempre:**
- lee por objetos git en `7f0d9aa3`;
- nada en el worktree;
- sin compilar;
- Python con `-I`, en guiones con el prefijo `cat4a_`;
- escribe solo en `hs65\pbt4\`.

**Las entradas** (en `hs65\pbt4\`):
- **el paquete v4.2:** `I-64-f4b-pbt4-gate-contract-package-v42.md` (sha256 `f3e5ae4d…`), con su tabla V42-01 a V42-11. La v4.1 ya la viste en tu §16. Ahora tocan el delta y todo lo de PB-T4a: el §6.1, el bloque ASCII §6.1.1 y el §6.1.2;
- **el generador:** `make_pbt4a_contract.py` (ASCII, `C8D11E8C…`), con `issue_pbt4a.py`, `issue_pbt4a-selftest.txt` e `issue_pbt4a-dry-objects.txt`;
- **el seco:** `dry-pbt4a.txt` y `contract-dry-pbt4a.json` (sha256 `3A3A1687…`), con `neg-pbt4a.txt` (33 de 33);
- **el esqueleto:** `pbt4a-skeleton.patch` (`25771775…`), con `make_pbt4a_skeleton.py`, su salida y `neg-skeleton-pbt4a.txt` (8 de 8);
- **los barridos:** `overload-sweep-pbt4a-7f0d9aa3.txt` y `name-check-pbt4a-7f0d9aa3.txt`. El generador pide que leas uno a uno los 15 literales de `ReadAt`, que casan por el prefijo de `Read`;
- **el recorrido de las guardas** (V42-04): `pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt`;
- **las dudas del generador:** `generator-doubts-pbt4a.md`. Mira sobre todo las 18 a 26;
- **los borradores:** `f4b-e49.md`, `f4b-e50.md`, `evidence-277.md` y `evidence-278.md`, y la entrada de PB-T4a del registro declarado, que es la sustitución de CT-PBT3A-12.

**Mientras revisas,** la sesión hace la compilación de comprobación (precondición 6) sobre una copia exportada nueva de `7f0d9aa3`, en `hs65\pbt4acc\compile\`. Primero aplica el parche y compila Core, UI y el Plugin, con el control positivo de los tipos nuevos, y corre la suite completa de Core: lo esperado es 19 129 = 19 122 + 7. Después viene un control negativo: sin el `Skip`, CT-PBT3A-12 tiene que fallar. Te mandaré el resultado como complemento.

**Qué comprobar:**
1. **La fidelidad del contrato en seco frente a la v4.2 para PB-T4a:**
   - el alcance: 10 rutas, y WEI, WHC, RIA y NH como UNIT_CHANGE;
   - el ForbiddenWriteScope (316);
   - las 14 invariantes;
   - las 34 paradas, con C11 literal y C13 con `Choose`, `AtKey` y `ReadAt`;
   - los 38 RequiredTests, con `PushBackDraftWriteGuardTests` en 2;
   - las 15 entradas de evidencia, con los totales exactos de la rama única. La primera tiene que estar en `F4B_EXACT_EVIDENCES`;
   - las 47 autoridades, con los dos registros del Owner;
   - los marcadores.
2. **B-08 resuelto:**
   - `PushBackOriginByText.ReadAt` en `Draft/`, y `AtOrigin` en RIA que solo lo llama;
   - EIG:350-364 en verde;
   - CT-PBT4A-04, 07 y 08, con sus líneas nuevas y la autoprueba.
3. **I-25:** el recorrido de 46 clases y 1 026 pruebas. ¿Es completo y correcto? ¿Lo que no ve está declarado bajo PBT4A-C16?
4. **El esqueleto:**
   - `ReadAt` inerte;
   - el `Skip` de CT-PBT3A-12 con su texto ASCII;
   - nada que lean las guardas de texto;
   - la suite esperada sobre la copia.
5. **La lista de PB-T4a, 1:** ¿es la única sustitución y está dentro de la cota del Owner? ¿La entrada del registro declarado es correcta, con su tabla «(al cerrar)»?
6. **Los textos de la emisión,** E49, E50, 277 y 278: ¿son fieles a las dos tandas literales del Owner?
7. **Las decisiones nuevas para el Owner,** si las hay.

**Formato:**
- El informe va en `hs65\pbt4\independent-review-pbt4a-check.md`, con secciones numeradas (§1, §2, …). Es el nombre que espera `issue_pbt4a.py`.
- El veredicto va en una línea propia, con la forma «**Veredicto: EMITIBLE.** B/I/M = **n / n / n**» o «**Veredicto: EMITIBLE TRAS CORRECCIONES.** B/I/M = …».
- Propón el literal ASCII de `BOUNDED_RESULT`, que debe empezar por EMITIBLE. Compruébalo con `bounded_check` de `make_pbt4a_contract.py`.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal.

### 1.1. Complemento enviado durante esta ronda (2026-11-20T18:30:40.548Z)

Mensaje de la sesión, para tu § del esqueleto (el punto 4 de CA-T4a): la compilación de comprobación de PB-T4a ya está hecha. Inclúyela en tu informe.

**Dónde:** `hs65\pbt4acc\compile\`. El informe es `compile-check.md`. Los guiones son `compile_check.sh`, `compile_check_ctrl.sh` y `compile_check_ctrl2.sh`. Las salidas están en `out-run1.txt`, `out-run-ctrl.txt`, `out-run-ctrl2.txt` y `trx-outcomes.txt`, y los registros en `out\`, `out-ctrl\` y `out-ctrl2\`.

**El resultado:**
- **La copia y el esqueleto:** la copia sale de `git archive 7f0d9aa3`. El parche `25771775…` aplica limpio: 4 archivos, 2 nuevos que no existían.
- **La compilación:** Core-tests Debug, UI-tests Debug y el Plugin Release (`-o`) dan rc 0 y 0 errores.
- **El control positivo:** `IWorkspaceEditIntentOriginReader` y `PushBackOriginByText` están en `RackCad.Application.dll`.
- **La suite completa de Core: 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas.** Las omitidas son las 6 de D-1 y CT-PBT3A-12, con el motivo literal del parche. Es lo esperado en la v4.2.
- **El control negativo:**
  - el primero partía de una premisa mía falsa: sin el `Skip`, CT-PBT3A-12 PASA sobre el esqueleto, como ya preveía el paquete («sin el `Skip`, 19 123 + 6»). Está declarado;
  - el corregido sí discrimina. Sin el `Skip` y con `PushBackFullDraftReader` nombrado en RIA de la copia, como hará el GREEN, CT-PBT3A-12 falla 1 de 1. Con RIA restaurado byte a byte, pasa. Con el `Skip` del parche, queda omitida.
- **La limpieza:** `build-server shutdown` hecho, y el checkout principal sin cambios.

**Qué te pido:**
- que digas si esto cumple la precondición 6 de la v4.2;
- si el control corregido es suficiente;
- si `compile-check.md` dice algo que no cuadre con los registros.

**El literal que pondré en `COMPILE_RESULT`,** salvo que lo corrijas:

«Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores; los tipos de los dos archivos nuevos del esqueleto en el ensamblado de Application; la suite completa de Core sobre la copia exportada de 7f0d9aa3, 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas (S-1 a S-6 de D-1 y CT-PBT3A-12); control negativo: sin el Skip y con un tipo del modelo nombrado en RIA, CT-PBT3A-12 falla»

## 2. Recomprobación 1 (2026-11-20T19:21:35.672Z)

Recomprobación de CA-T4a, sobre la v4.3: comprueba que I-01 y M-01 a M-04 están corregidos, y la compilación de comprobación repetida.

**Las reglas de siempre:** objetos git en `7f0d9aa3`, nada en el worktree, sin compilar, guiones `cat4a_`.

**Las entradas:**
- **el paquete v4.3:** `hs65\pbt4\I-64-f4b-pbt4-gate-contract-package-v43.md` (sha256 `4f482279…`), con V43-01 a V43-05. V43-01 lleva «CA-T4a I-01». La regla nueva del §5.0 dice: ningún `?` de referencia sin `#nullable enable`, y nunca en `Draft/`. M-01 está en el §0, §4.1, §5.0, §5.1, §9 y PBT4A-C16. Con M-03, la precondición 6 pide los filtros de UI y 0 `warning CS`;
- **el generador:** `make_pbt4a_contract.py` (`98795D91…`), con M-02: el rango R sale del registro (R-0 a R-43);
- **el seco:** `contract-dry-pbt4a.json` (`7EA2E372…`) y `dry-pbt4a.txt`, todavía sin `BOUNDED_RESULT`;
- **las negativas:** `neg-pbt4a.txt` (34 de 34) y `neg-skeleton-pbt4a.txt` (10 de 10);
- **el parche:** `pbt4a-skeleton.patch` (`e99c87c6…`), con `public static string Choose(…) => null;`;
- **los textos:** `f4b-e50.md`, que lleva M-04, M-01 y una fe de erratas de la precondición 8 de la v4.3, y `evidence-277.md`;
- **la compilación de comprobación repetida:** `hs65\pbt4acc\compile\compile-check.md`, reescrito. Las dos fases sobre la v4.3 dan **0 errores y 0 avisos CS** en Core-tests, UI-tests y el Plugin; Core **19 129 = 19 122 + 7** (D-1 y CT-PBT3A-12); y **los 12 filtros de UI en verde**:
  - el esqueleto: `out-run-skel.txt` y `out-skel\`;
  - el GREEN literal de la v4.3: `apply_green_pbt4a.py`, conjunto `7d29d396…`, en `out-run-green.txt` y `out-green\`.
  
  La primera compilación, sobre la v4.2, queda declarada como sustituida en el §2 de `compile-check.md`, con la lección de los avisos CS. Al custodiar, `issue_pbt4a.py` normaliza los CR sueltos de `out-run-green.txt` con nota en el manifiesto.

**Qué comprobar:**
1. **I-01 está resuelto,** en el código literal de PB-T4a, en el esqueleto y en el GREEN, y la regla del §5.0 está bien medida (`Directory.Build.props:28`).
2. **M-01 a M-04 están corregidos.** La fe de erratas de la precondición 8 en F4B-E50: ¿basta así, o pide una v4.4?
3. **La compilación cumple la precondición 6 de la v4.3,** también con el GREEN literal, y `compile-check.md` cuadra con los registros.
4. **El seco `7EA2E372…`** es fiel a la v4.3, y no cambia nada frente al de la v4.2 aparte de lo previsto: el `Objective`, la autoridad de las mediciones y el parche.
5. **Hay decisiones nuevas para el Owner, o no.**

**El informe:**
- una sección nueva en `independent-review-pbt4a-check.md`, con el encabezado exacto `## §N. Recomprobación de la v4.3`;
- tu veredicto en una línea propia, con la forma «**Veredicto: EMITIBLE.** B/I/M = **n / n / n**»;
- el `BOUNDED_RESULT` final: tu literal de la §10 con el sufijo `; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M <n>`, comprobado con `bounded_check` de `make_pbt4a_contract.py`;
- confirma también el literal de `COMPILE_RESULT`, o corrígelo. El mío sigue tu propuesta: «Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores y ningun aviso CS (solo los MSB3277 locales de las referencias de AutoCAD); los tipos de los dos archivos nuevos del esqueleto en el ensamblado de Application; la suite completa de Core sobre la copia exportada de 7f0d9aa3, 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas (S-1 a S-6 de D-1 y CT-PBT3A-12), tambien con el GREEN literal del paquete; los filtros de UI del contrato, en verde; control negativo: sin el Skip y con un tipo del modelo nombrado en RIA, CT-PBT3A-12 falla».

**Responde con:**
- el veredicto;
- los hallazgos;
- el `BOUNDED_RESULT` final;
- el `COMPILE_RESULT` confirmado.
