# PB-T4a: las dudas del generador y de la emisión (fase 1, sobre la v4)

Este documento transcribe las dudas del redactor de `make_pbt4a_contract.py` y de `issue_pbt4a.py` (2026-11-20), antes de la v4.1 del paquete de PB-T4. No contiene decisiones de la sesión. Cada duda lleva la lectura del redactor, para que la v4.1 o CA-T4a la confirmen o la corrijan.

## Lo que hay

- **`make_pbt4a_contract.py`** (fase 1; se arma con `_t4a/build_t4a.py`). Es ASCII puro y aborta si encuentra un byte no ASCII en sí mismo; también exige que `BOUNDED_RESULT` sea ASCII. El seco sobre `7f0d9aa3`, con la v4, sale con rc 0 (`dry-pbt4a-fase1.txt`). Comprueba:
  - el anclaje, la CI del cierre, la numeración y el contrato de PB-T3c;
  - nc1 y nc2 v2 con `--task PBT4`, la primera evidencia en `F4B_EXACT_EVIDENCES` y la Snapshot K;
  - los 36 filtros heredados, medidos.

  No escribe ningún contrato: lista 13 pendientes de la v4.1 y 5 AVISOS de la emisión.
- **`issue_pbt4a.py`** (fase 1): `--selftest` y `--dry-objects` dan rc 0, y `--dry` y `--commit` abortan.
  - La numeración: F4B-E48 y la §276 son las últimas en `7f0d9aa3`.
  - Las 8 líneas del estado que dejó el cierre de PB-T3c están en el estado de `7f0d9aa3`.
  - De los 35 textos de la custodia propuesta, 34 salen limpios.
  - La redacción de `<unidad>:\r4<letra>` como `<copia>` funciona.
- **Los barridos sobre `7f0d9aa3`:** `overload-sweep-pbt4a-7f0d9aa3.txt` y `name-check-pbt4a-7f0d9aa3.txt`, con el corte de argumentos corregido (CA-T3c M-03).

## Las dudas

1. **El TaskId tiene 23 caracteres, no 24.** `F4B-PBT4-FULL-READ-PATH` tiene 23. El §0 de la v4 dice «(24)», y el §6.0 de la v1, «de 24 a 26 caracteres (MAX_PATH, R-T1-07)». MAX_PATH solo acota por arriba; R-T1-07 de PB-T1 decía 28 a 31.
   - El generador solo exige ≤ 26, y deja la diferencia como pendiente.
   - Propuesta: la v4.1 corrige el recuento y escribe la regla como «≤ 26». La alternativa es otro nombre de 24.
2. **PBT4A-C11.** El generador la saca de `PBT3C-C11` del contrato de PB-T3c, cambiando `PBT3C` por `PBT4` (el `--task` de nc1 y nc2 es `PBT4` para las tres subtareas, y sus registros, `SATISFIED_UNDER_PBT4_…`) y «PB-T3c» por «PB-T4a». El texto acaba en «no one-off acceptance extends to PB-T4a». La sustitución es mecánica y completa. **Falta** el literal de la v4.1 contra el que compararla.
3. **Lo heredado en los `RequiredTests`.** La v3 (§6.1) dice «`Workspace` (a medir) y las de PB-T1 a PB-T3: false».
   - El generador toma los 36 filtros del contrato de PB-T3c, todos con `ExpectRed` false y su `MinSelected`. Es el precedente de CA-T3c M-01. `PushBackViewEraseGuardTests` llevaba true en PB-T3c.
   - Lo estático no baja de ningún mínimo.
   - **Pero** el `Skip` de CT-PBT3A-12 está en `PushBackDraftWriteGuardTests`, cuyo mínimo es 3, y con el `Skip` quedan 2 ejecutables. El mínimo de ese filtro tiene que bajar a 2, como bajaron los de F4 con los `Skip` de D-1. Lo decide la v4.1.
4. **El RED previsto: 19 136 = 19 124 + 5 `Failed` + 7 `Skipped`** (v4 §5.1). Supone 7 CT, cada una un `[Fact]`, y un `Skip`.
   - La CT de I-24 (G-L1 y la pureza de los archivos nuevos de `Draft/`) cambia la cuenta. Si es una CT más que pasa en el RED, da 19 137 = 19 125 + 5 + 7. Si falla en el RED, da 19 137 = 19 124 + 6 + 7.
   - La v4.1 tiene que dar las dos ramas con el total exacto de cada una (F4B-E46). El generador comprobará la línea contra la cuenta de CT, de `[Fact]` y de `Skip`.
5. **`RackPilotIntentAdapter.Read` se llama desde fuera de su archivo** (NH:242; T4A-2 del barrido). Si gana `originText` como parámetro obligatorio, NH deja de compilar hasta que se cambie, y NH está en el alcance.
   - El esqueleto del RED, según la v3 («RIA y NH, sin cambio»), no puede cambiar la firma.
   - Lo puede hacer el GREEN, con los dos archivos a la vez, o con `string? originText = null`.
   - La v4.1 lo fija.
6. **Las guardas que cuentan `ReadIntent` o `_editIntentReader` sin anclar** (el barrido de nombres):
   - EIG:579, 604-617, 636, 925 y 1124;
   - `WorkspaceLifecycleGuardTests:369, 442, 623, 634 y 687`;
   - PEG:288 y 297.

   La sentencia en línea de D-PBT4-03 añade `_editIntentReader is …` y `ReadIntentAtOrigin(`, que contiene `ReadIntent`. La v3 y la §14.1 de la revisión dan esas guardas por verdes.
   - El esqueleto no cambia WHC, así que la compilación de la precondición 6 no lo prueba.
   - **Propuesta:** CA-T4a las lee una a una. Si la sesión quiere medirlo, aplica la sentencia a una copia solo para las guardas de texto, como dice la v3 §6.0.
7. **`WorkspaceCommandAdvisorGuardTests`:** su `ReasonTable` fija 5 motivos para el cuerpo `Read` de RIA y 3 para `ReadOnActiveDocument` de NH (:410-451). La rama del origen no puede añadir ningún motivo. Es una restricción para el Worker que la v4.1 debería escribir en la CT o en la nota de D-PBT4-03.
8. **La reflexión sobre `WorkspaceHostCore`** (EIH:993-1104, `WorkspaceLifecycleGuardTests:357-358`, PEG:383-385, `WorkspacePilotUpdateHostTests:503` e IG:427-428) cuenta campos, métodos, propiedades y tipos anidados. PB-T4a no declara nada nuevo en WHC: la variable de patrón `originReader` no es un miembro. Sigue verde mientras se respete «WHC solo cambia en las cinco sentencias de lectura».
9. **La redacción de `independent-review-v1.md`** (resuelta; la sesión dijo sí). Nombraba la carpeta de worktrees de Codex, que FORBIDDEN rechaza.
   - `issue_pbt4a.py` lo sustituye por `<worktrees-de-Codex>` (`LOCAL_FIX`, una vez exacta, con su nota de redacción).
   - El encargo de las rondas §13 a §15 sale de la transcripción con `_write_ir4_prompt.py`, en `independent-review-v1-rechecks-prompt.md`. El de la v1 es `independent-review-v1-prompt.md`.
   - Los 37 textos de la custodia salen limpios.
10. **El esqueleto como parche.** La v3 (§6.1) lo describe: `FromReads` deja los campos en null, `Choose` devuelve null, RIA, NH y WHC no cambian, y el `Skip` está puesto.
    - El `Skip` es un archivo de `tests/`. El parche de PB-T3c era solo de `src/`.
    - Con el `Skip` en el parche, la suite de Core sobre la copia daría 19 129 = 19 122 + 7; sin él, 19 129 = 19 123 + 6.
    - **Propuesta:** el parche lleva `src/` y el `Skip` con su motivo literal, porque así lo prueba la suite completa. Espera el texto exacto de la v4.1 (`make_pbt4a_skeleton.py`).
11. **El registro declarado de CT-PBT3A-12.** La entrada «## PB-T4a» de `superseding-characterization.md` lleva el `Skip`, su motivo literal y CT-PBT4A-06, que la sustituye, y su tabla de ejecución se completa al cerrar (la lección de PB-T3b). Espera el motivo literal de la v4.1.
12. **La custodia propuesta** (`HIST` de `issue_pbt4a.py`, 30 textos en `F4B-DRAFTS/pbt4/`):
    - las v1 a v4 como `package-v1.md` … `package-v4.md`;
    - la revisión y su encargo;
    - los guiones de medición y sus salidas;
    - las instrucciones de cada versión y la lista de sustituciones;
    - los dos barridos.

    Fuera de esa carpeta van tres más:
    - el registro de mediciones, en `docs/initiatives/I-64-f4b-pbt4-measurements.md`;
    - el registro del Owner, en `F4B-DRAFTS/coordinator/owner-pbt4-decisions.md`;
    - la v4.1, en `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`.

    La sesión dice si falta o sobra algo. Por ejemplo, sus mensajes al revisor como parte del encargo.
13. **La v4 remite a versiones anteriores** («como en la v3», «lo de la v2 (§6.0)», «las de la v1 (§6.0)»). El generador compara textos literales con el paquete, así que la v4.1 debería llevar enteros:
    - el §5.1;
    - el §6.0, con la identidad, las paradas comunes con su texto, RR y C11;
    - el §6.1.

    Si no, el generador tendría que leer la v1, la v2 y la v3 a la vez.

## Las dudas de la fase 2 (con la v4.1)

14. **La indentación del `Skip`.** En el §6.1 la línea va dentro de un bloque de código que la lista sangra con 4 espacios, y la línea lleva 8. La línea 60 del archivo lleva 8, por la clase. El redactor lee como literal el texto sin la sangría, y conserva los 8 espacios del archivo. Así la única línea que cambia es la del atributo, como pide el §6.1.
15. **El tamaño del contrato:** son 63 507 bytes, frente a los 54 162 de PB-T3c.
    - El `ForbiddenWriteScope` de `src/` tiene 190 entradas. Application ya no va entera: PB-T4a escribe en ella, así que [PKC-C] la parte en prefijos.
    - El §6.0 de la v4.1 no fija un límite para PB-T4a. Solo saca la lista de PB-T4b a un archivo custodiado.
16. **La fila del kit en la tabla de RR-3** («`PushBackFullReadSupport` (el kit y el doble)»). `rr3_table_check` admite `(CT-`, `(el kit` y `(el doble ` tras el nombre de la clase. El bloque sigue marcado `# ---- rr3_table_check (inicio)` y `(fin)` para `sync_rr3_reading.py`.
17. **El sha256 del paquete se comprueba al final del paquete leído,** tras todas las comprobaciones de contenido. Así una negativa sobre el paquete falla por su motivo, y no siempre por el sha. El resultado es el mismo: ninguna copia alterada pasa.

## B-08 y la segunda tanda del Owner (la v4.2)

18. **Lo que cambia con la v4.2 queda como parámetro.**
    - **El generador:** `PKG_VERSION`, `CUSTODY_SHA256` y `ALLOWED_EXPECTED` (c1_head.py). `NEW_FILES` sale del alcance.
    - **El generador comprueba B-08:** el bloque del GREEN de `AtOrigin` no llama a `PushBackPilotIntentReader.Read(` y sí a `PushBackOriginByText.ReadAt(`. Tiene las anclas EIG:357 y RIA:121. Con la v4.1 es un AVISO del seco; desde la v4.2, un error.
    - **El esqueleto:** `PKG_NAME`, `PKG_VERSION`, `PKG_SHA` y `NEW_SRC_RULES` (`make_pbt4a_skeleton.py`). El cuerpo inerte de cada método público sale de la viñeta del esqueleto de §6.1, con la forma «`X` que devuelve `Y` (`=> Y;`)». En la v4.1, `Choose` y `AtKey`; si la v4.2 añade `ReadAt`, su viñeta tiene que darle el suyo. Un archivo nuevo de `src/` sin regla aborta.
    - **La emisión:** `issue_pbt4a.py` toma la versión y su sha256 del generador. Las anteriores van a la historia (`package-v41.md`). Los recuentos salen del seco custodiado, y el parche se comprueba contra la salida del guion del esqueleto. Sus textos llevan la marca `B08_TEXTS_REVIEWED`: si el generador fija otra versión, aborta hasta que el redactor los relea. Por ejemplo, la frase de `AtOrigin` de F4B-E50 y los archivos nuevos de la 278.
19. **La segunda tanda del Owner va en F4B-E49,** con la primera, en el mismo commit X. E50 sigue siendo la emisión, y la 277 y la 278 no cambian de número.
    - F4B-E49 se asigna al custodiar.
    - El registro dice que «la numeración F4B-E se asigna al custodiarlo».
    - Las dos tandas son decisiones del Owner sobre PB-T4, previas a esta emisión.
20. **La custodia estaba incompleta frente a la precondición 8 de la v4.1.** Le faltaban los guiones de la v4.1 que su cabecera lista: `lists_core.py`, `copy_map_*`, `retire_compile_*`, `measure_pbt4_v41*`, `read_pbt3c_contract.py` con su volcado, las sondas, `make_v41.py` y los fragmentos de `v41/`.
    - Se añaden, y ahora son 79 textos, frente a 45.
    - Además, todo archivo de `hs65/pbt4` tiene que estar en la custodia o en `CUSTODY_SKIP`, con su motivo. Lo que no lo está queda pendiente, y aborta con `--dry` y `--commit`.
    - Hoy quedan sin clasificar 24 archivos de la v4.2 en curso, entre ellos `green_pbt4a.py`, `walk_guards_pbt4a.py`, `measure_pbt4_v42*`, `split_lists_pbt4*`, `pbt4b-substitution-list.tsv`, `pbt4d-retire-list.tsv`, `probe_tests_text.py` y `v42/`. La v4.2 dice cuáles se custodian.
21. **El estado con la segunda tanda:**
    - **`f4b_status`:** PB-T4b, con su lista exacta, 470, la que vio el Owner; el código del piloto se retira en PB-T4d, después de PB-T4c, que vuelve al Owner antes de emitirse.
    - **`NEXT_ACTION`:** PB-T4b y PB-T4c, por la emisión condicional, cada una tras el cierre de la anterior; PB-T4d vuelve al Owner.
    - **`REMAINING_BUDGET`:** PB-T4b, PB-T4c y PB-T4d empiezan en 1 de 3.

    Es la lectura del redactor del registro. La precondición 7 de la v4.1 aún dice «PB-T4b: no»; la v4.2 lo actualiza.
22. **En F4B-E49, la pregunta 1 de la primera tanda** ya no dice «en PB-T4b». Dice: «Cuándo se retira lo fija la segunda tanda: en PB-T4d». La sesión confirma la frase.

## Con la v4.2

23. **La v4.2 leída:** sha256 `f3e5ae4d3ac321340f0616014fd962c0af32bf19791c1060617cfca9ea8b007b`, 192 400 bytes. El generador la lee cambiando solo la versión y el sha256.
    - **Lo que cambia en el contrato** frente a la v4.1:
      - el `Objective`;
      - INV-PBT4A-04: `AtOrigin` solo llama a `ReadAt`, y RIA conserva una lectura de cada lector y un escaneo, en `Read`;
      - INV-PBT4A-13 y PBT4A-C13: `ReadAt` entra en la lista cerrada;
      - las dos autoridades que nombran la versión.
    - **Los recuentos no cambian.** La comprobación de B-08 pasa: ya no es un AVISO. Si la v4.2 que se emite no es esta, cambian el sha256 del generador, el del esqueleto y su salida.
24. **El esqueleto de la v4.2:** `ReadAt` lleva un tipo de retorno en tupla. El nombre del método ahora se lee también con esa forma (S07 de las negativas del esqueleto, sobre los fragmentos de `v42/`).
    - El parche cambia: 6 441 bytes, sha256 `25771775bf8fe8d4844cc0b2fb8ee26c5044b321f3cc0f5b0d4f73804b379515`.
    - `git apply --check` sobre los dos archivos de la base exportados da limpio.
    - Lo previsto sobre la copia sigue igual: Core 19 129 = 19 122 + 7.
25. **El barrido de nombres con `ReadAt`** da 15 literales específicos, los mismos que da el nombre viejo `Read`: todos casan por el prefijo.
    - **CustomPropertiesEdgeGuardTests:566** selecciona por nombre exacto (`member.Name == "Read"`) en el acceso físico de las propiedades, que PB-T4a no toca.
    - **Los otros 14** están en clases que recorre la v4.2 (WBOSG, WBST, CAG, WISG e IG).
    - **Los dos patrones `\.Read\w*\s*\(`** (WBOSG:95 e IG:1045) miran el clasificador y el manejador de `CommandWillStart` del puente, no RIA ni `Draft/`.
    - **El barrido de sobrecargas:** `ReadAt`, 0 en sus archivos y 0 en ninguna parte.
26. **La custodia con la v4.2:** los 24 archivos, más `make_v42.py`, `pbt4a-guard-walk-flagged.txt` y los 13 de `v42/`, van en la historia según la cabecera y la precondición 8 de la v4.2. Son 107 textos y no queda nada sin clasificar.
    - `pbt4b-substitution-list.tsv` va byte a byte y se comprueba con su sha256 `44393b19…` (precondición 7).
    - La v4.1 pasa a `package-v41.md`.

## Con la v4.3 (CA-T4a)

27. **CA-T4a M-02:** el rango del registro de mediciones ya no va fijo en el generador.
    - Se lee del registro: en la revisión, o, en seco, del `pbt4-measurements.md` de la carpeta de `--package-file`.
    - Exige que los R-n sean consecutivos desde R-0, y que la cabecera del paquete cite «R-0 a R-n» con el último.
    - Con la v4.3, `Authorities[1]` dice «R-0 a R-43».
28. **CA-T4a M-04 y M-01, en los textos de la emisión:**
    - **F4B-E50** dice «la del contrato de PB-T3c (`F4B_EXACT_EVIDENCES` del sustituto v2)».
    - **El recorrido** va con la redacción de la precondición 6 de la v4.3: «por marcas», con la ampliación de CA-T4a (97 archivos) y las aserciones traducidas, 36 de 36. La compilación de comprobación nombra también el `src/` del GREEN literal (`apply_green_pbt4a.py`).
    - **La 277** custodia `v43/`, `apply_green_pbt4a.py` con su autoprueba y `pbt4a-guard-walk-flagged-v43.txt`.
29. **CA-T4a I-01 en el esqueleto:** `make_pbt4a_skeleton.py` comprueba la regla nueva de §5.0. En un archivo nuevo sin `#nullable enable`, ningún `string?` ni `object?`, tampoco en un molde: lo mira en el bloque del GREEN entero, no solo en el esqueleto. Las negativas S08 y S09 devuelven el `string?` de `Choose` y el molde de `AtKey`, y abortan.
30. **El encargo de CA-T4a:** `_write_cat4a_prompt.py` lo saca de la transcripción de la sesión, con la ronda, sus complementos y las recomprobaciones.
    - Cuenta los mensajes al revisor `ac44e95c3001c60e3` desde el primero de CA-T4a; las rondas del paquete no entran.
    - Si aparece un mensaje que no es ronda ni complemento, aborta.
31. **Los archivos del revisor:** se custodian todos los `cat4a_*` (`.py`, `.txt` y `.json`), salvo los dos listados de `git ls-tree` de `7f0d9aa3`. Esos se regeneran con git, y el mayor pesa 1,1 MB. La sesión decide si van.
32. **Una errata de la v4.3,** para el autor: la precondición 8 sigue diciendo «las versiones v1 a v4.1» y «los fragmentos de `v41/` y `v42/`». La custodia lleva v1 a v4.2 y `v41/` a `v43/`.

## Lo que queda para la fase 2 (con la v4.1)

- **El generador:** el `Objective`, el alcance y su `ForbiddenWriteScope`, `UNIT_CHANGE`, los invariantes y las paradas, la lista cerrada de declaraciones, los `RequiredTests` propios, la evidencia y la línea de la CI, RR y la tabla de RR-3, y las negativas.
- **`issue_pbt4a.py`:**
  - el armado de X y de BASE;
  - las plantillas `f4b-e49.md`, `f4b-e50.md`, `evidence-277.md`, `evidence-278.md` y `superseding-pbt4a.md`;
  - las negativas del registro declarado.
- **`make_pbt4a_skeleton.py` y `pbt4a-skeleton.patch`.**
