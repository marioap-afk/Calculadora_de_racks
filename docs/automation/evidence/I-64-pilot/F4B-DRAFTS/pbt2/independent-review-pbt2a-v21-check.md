# I-64 F4B — Comprobación acotada de PB-T2a (`F4B-PBT2A-DRAFT-STATE-PORT`) sobre el paquete v2.1

**Veredicto: EMITIBLE TRAS CORRECCIONES — 0 BLOQUEANTES / 0 IMPORTANTES / 6 MENORES.**

**Fidelidad.** El contrato en seco reproduce fielmente el §6.1 y lo común del §6.0 de la v2.1:
- el `Objective`, el alcance y los 18 `RequiredTests`;
- los 13 invariantes, sin la nota, y las 32 paradas;
- la `ExpectedEvidence` (9);
- las 39 `Authorities`, ninguna `UNIT_CHANGE`.

**F4B-E28 está bien aplicada:**
- es la autoridad de emisión, por la pregunta 4;
- la pregunta 3 deja la validación de la escala fuera de PB-T2a;
- las preguntas 1 y 2 no tocan PB-T2a.

**Las lecciones de PB-T1b y PB-T1c están aplicadas:**
- C13 por nombre;
- las notas, fuera del contrato;
- el `Trailer` real en la evidencia;
- `build-plugin` omitido con Core en `failure` en el RED;
- P-01 con la Snapshot J;
- F4B-E25 para las correcciones.

**Las seis correcciones** son menores. Cuatro las aplica el generador antes de escribir (M-1 a M-4). Las otras dos son una limpieza de texto (M-5) y dos hechos que el paquete tiene que recoger para PB-T2b y PB-T2c (M-6).

**PB-T2a se puede emitir en cuanto cierre PB-T1c,** con las comprobaciones del §3. Hoy PB-T1c está en su corrección, en el intento 2 (F4B-E30).

**No hay ninguna decisión material nueva para el Owner.**

## §1. Método y límites

**Qué leí:**
- el paquete v2.1 (§6.0 y §6.1 enteros, y lo que citan) y `v21-delta.md`;
- `generator-notes-pbt2a.md`;
- `make_pbt2a_contract.py`: el anclaje, `window_checks`, la línea base de la CI, las paradas y la escritura;
- `dry-pbt2a.txt` y `contract-dry-pbt2a.json`, entero;
- `owner-pbt2-decisions.md`.

**Cómo lo comprobé, en `d8ccbcb8`:**
- por objetos git desde el checkout principal;
- **el contrato en seco:** su sha256 (`71B0918C…E341A`) coincide con el que imprime el seco;
- **F4B-E28:** la copia de trabajo es idéntica, byte a byte, a la custodiada (`git show d8ccbcb8:…/owner-pbt2-decisions.md` sin diferencias), y `f4b-dispositions.md` llega hasta F4B-E30;
- **el alcance prohibido:** las 87 entradas de `src/` coinciden con `_work/forbidden-v2-out.txt`, y hay 25 de Core, 63 de UI y 17 de la raíz;
- **las autoridades:** de las 39 rutas, existen todas salvo las siete que se custodian con la emisión o al cerrar PB-T1c;
- **las líneas de los mutantes** de PB-T2c existen (W:3683-3684 y WC:95);
- **las clases de los filtros de UI de PB-T2a no abren el configurador real.** Lo comprobé con grep: solo aparece `HeaderConfiguratorDialog` en `…SubdialogTests` y una lista de nombres en `…KeyboardTests`.

**Límites:**
- no ejecuté el generador ni Python, y no leí el worktree;
- el texto de la ventana de PB-T1c, la de su GREEN, todavía no existe.

## §2. Las 14 DUDAS del generador

| Duda | Qué hacer |
|---|---|
| **D-1.** Las clases de Core en el `ForbiddenWriteScope` | **Aceptar la lectura del generador:** 25 = las 18 de PB-T0, las 5 de PB-T1b y las 2 de PB-T1c. Es la del contrato emitido de PB-T1c (23 = 18 + 5). Solo estrecha, e INV-PBT2A-07 ya prohíbe editar esas clases. La lectura literal las dejaría sin proteger. No es material. Basta con anotarlo en la salida de los anclajes |
| **D-2.** Los nombres de custodia | **Correctos.** El nombre de esta comprobación es el que dio la sesión. El registro, `docs/initiatives/I-64-f4b-pbt2-measurements.md`, sigue el patrón de `I-64-f4b-pbt1-measurements.md`, que existe en `833ea6bf`. La carpeta `F4B-DRAFTS/pbt2/` es la que fija el §6.0 |
| **D-3.** `v21-delta.md` no es autoridad | **Dejarlo como está,** en `CUSTODY_EXTRA`: la lista `UNIT_DOC` literal del §6.0 no lo nombra, y el registro V21 está dentro del propio paquete. Subirlo también sería válido, pero no hace falta |
| **D-4.** La salida de los anclajes no es autoridad | **Dejarla en `CUSTODY_EXTRA`.** Es evidencia de la emisión, y la autoridad es esta comprobación, que la cita |
| **D-5.** F4B-E28 como autoridad | **Sí, obligatoria:** es la autoridad de emisión que cita `IssuedBy`, y su pregunta 3 fija el alcance. No hace falta editar el paquete para PB-T2a, pero la entrada de emisión de `f4b-dispositions.md` tiene que decir que se cumplen las condiciones de la pregunta 4 |
| **D-6.** Las autoridades del §1 que no están en el §6.0 | **Correcto,** tanto lo incluido como lo excluido |
| **D-7.** La redacción propia | **Aceptable.** Las adaptaciones al inglés de C01, C03, C05 y C10, y las de C02 y C07, no cambian el sentido. La cola de C17 es la del §6.0. El orden de la `ExpectedEvidence` es coherente |
| **D-8.** C11 conserva «section 6.0 of the package» | **Hay que corregirlo** (M-4) |
| **D-9.** INV-PBT2A-07 lleva la frase de PB-T2c | **Dejarla literal:** en PB-T2a está vacía y no hace daño. Lo mismo vale para las subviñetas de UI de INV-PBT2A-08 |
| **D-10.** El anclaje de la `BaseSha` no va en un invariante | **Correcto.** Lo comprueban la sesión antes de emitir (§6.0) y el generador, con `git diff PT1C_CLOSE REV -- src` vacío |
| **D-11.** Las anclas [T1c] de W, WC y WX no se comprueban | **Correcto:** son de los anclajes de PB-T2b. En PB-T2a, la ventana solo cuenta para V2-05 |
| **D-12.** «Ni ninguna otra guarda», en V2-05 | **Suficiente,** con un añadido (M-3). Las guardas de T1B y de texto se analizaron en el §4 (IR1 §3) y ninguna mira `editable` |
| **D-13.** La declaración de `editable`, con su texto exacto | **Comparar con los espacios colapsados.** Si solo cambia la forma (por ejemplo, `= new();`, con el mismo tipo, `private readonly` y una sola declaración), se anota como hecho no material y se sigue. Si no es un campo `private readonly PushBackEditableState` con inicializador, o hay más de una declaración, se aplica el §6.0: STOP y v3 (M-2) |
| **D-14.** `BOUNDED_RESULT` | **Correcto.** Vale: «EMITIBLE TRAS CORRECCIONES, 0/0/6», y «EMITIBLE» cuando estén aplicadas las correcciones M-1 a M-5 (M-6 se lleva al paquete antes de PB-T2b) |

## §3. La emisión tras el cierre de PB-T1c

PB-T2a se puede emitir en cuanto cierre PB-T1c. Antes, el generador y la sesión comprueban esto al escribir, sin `--objects-rev`, y con todo `soft()` convertido en error:

1. **El cierre** (`PT1C_CLOSE`).
   - Desciende de `833ea6bf`, y la revisión desciende de él.
   - `git diff PT1C_CLOSE <BaseSha> -- src` sale vacío.
   - Tiene su entrada de cierre en `f4b-dispositions.md` y su `task-close-package.md`.
   - Ya lo hace el generador.
2. **V2-05, sobre W y WC del cierre:**
   - la declaración de `editable`, con la regla de D-13;
   - `composite => editable.Editor`, y `state` y `moduleOwner` calculados;
   - `EditableStateForTest => editable;`;
   - ninguna otra asignación ni suscripción;
   - **además (M-3):**
     - ningún campo, ni ninguna asignación de constructor, guarda una referencia que salga de `editable`, `composite`, `state` o `moduleOwner`;
     - ningún miembro de W o WC se llama como uno de los 26 nombres de PBT2B-C13. Hoy solo aparece `mode`, en un comentario de W:3306;
   - las listas `Forwarded`, `Tokens` y `Rebind` y los literales de las tres clases de PB-T1c, incluida la CT-02 corregida, siguen sin nombrar `editable`.
3. **La línea base de la CI** (`CI_BASE_T1C`, M-1).
   - Es la corrida `push` de la punta que cierra PB-T1c, 4/4 en `success`.
   - Su SHA tiene el mismo `src` y los mismos `tests` que `PT1C_CLOSE`.
   - Sus cifras se comparan con las previstas: Core 19 088, y UI 2 439 con 21 `Skipped`. Si difieren, para hasta que la sesión lo explique por el diff de PB-T1c. No sería material, pero cambia la `ExpectedEvidence` y el GREEN previsto (19 092 / 2 439).
4. **El resto del producto:**
   - Application, Domain y Plugin siguen iguales que en `a18ba345`;
   - G-L1c y sus quince autopruebas siguen como en `95373af6`;
   - el recuento de los `RequiredTests` sobre la revisión no cambia.
5. **La custodia** (los 19 AVISOS en cero), en el commit que será la `AuthorityRevision`:
   - el paquete v2.1 y el registro con su apartado Q-7;
   - `v2-delta.md`, IR1 con su §10 y esta comprobación;
   - los guiones y lo demás de `CUSTODY_EXTRA`.

   `IssuedBy` y la autoridad de `f4b-dispositions.md` (hasta la entrada de emisión) se ponen al día.
6. **El entorno:**
   - `MainSha` = `origin/main`, recién leído (hoy `812baf87`);
   - `config.toml` = Snapshot J;
   - el binario acreditado;
   - las celdas vigentes, hasta el 2026-12-29;
   - `Test-Json` en `True`;
   - una sola delegación abierta (la de PB-T1c, cerrada).
7. **La autorización (F4B-E28, pregunta 4):** esta comprobación, sin bloqueantes ni importantes, y la remedición de los pasos 1 a 4 sin ningún cambio material. Si PB-T1c llegara a su intento 3, vuelve al Owner (PBT1C-C12), y PB-T2a espera.

## §4. Decisiones materiales nuevas

**Ninguna.** Las miré con rigor:
- la lectura de D-1 solo estrecha;
- la regla de D-13 sigue el §6.0;
- una línea base distinta, explicada por el diff de PB-T1c, no es material;
- la corrección de C11 solo aclara una referencia;
- los dos hechos de F4B-E30, la escritura en el perfil y el desfase de una pasada, no tocan PB-T2a. Para PB-T2b y PB-T2c son M-6. Informar al Owner del primero es cosa de la sesión, por F4B-E30, no de esta tarea.

## §5. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| M-1 | MENOR | Con `CI_BASE_T1C`, el generador solo comprueba el formato. No ata el SHA de la corrida al cierre de PB-T1c, y no compara las cifras con las previstas, que fijan el §6.1 y su GREEN (19 092 / 2 439) | `make_pbt2a_contract.py:1040-1045` | Comprobar que `git diff <sha> PT1C_CLOSE -- src tests` sale vacío, y que `(Core, UI, Skipped)` vale `(19088, 2439, 21)`. Si no, error hasta que la sesión lo explique |
| M-2 | MENOR | V2-05 compara la declaración de `editable` byte a byte. Si el GREEN de PB-T1c solo cambia la forma, se dispararía STOP y v3 sin necesidad | `make_pbt2a_contract.py:429`, `:442`; paquete §6.0 | La regla de D-13 (§2) |
| M-3 | MENOR | `window_checks` mira `composite`, `state`, `moduleOwner`, la costura, las asignaciones y los `+=`. No mira si hay campos o asignaciones de constructor que capturen una referencia sacada del estado, ni choques de nombres con PBT2B-C13. «Todo lo que cuelga de él es calculado» (§6.0) queda comprobado a medias | `make_pbt2a_contract.py:438-450` | Añadir las dos comprobaciones del §3, paso 2, con sus autopruebas sintéticas |
| M-4 | MENOR | C11, copiado con «cambiando solo el prefijo», dice «the only admitted differences are those listed for each signature in section 6.0 of the package». En el contrato de PB-T2a, «the package» es el de PB-T2, y su §6.0 no lista esas diferencias: están en [PT1] §6.0 | `contract-dry-pbt2a.json`, PBT2A-C11; paquete §6.0 («con el texto de [PT1] §6.0»); D-8 | Precisar «section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)». Aclara la referencia sin cambiar la regla |
| M-5 | MENOR | Limpieza de texto. La sección de la autoridad de F4B-E25 encadena la celda del §1, las líneas del §6.0 y «PB-T2a: ninguna…», con frases repetidas. `ExpectedEvidence[1]` dice «en los tres RED» en un contrato que solo tiene uno | `contract-dry-pbt2a.json:117`, `:533` | Quitar lo repetido de F4B-E25 y, si se quiere, empezar `ExpectedEvidence[1]` por «En el RED de PB-T2a» (opcional: es literal del §6.0) |
| M-6 | MENOR | Son hechos para PB-T2b y PB-T2c, no para PB-T2a. F4B-E30 deja dos que el paquete todavía no recoge. **(a)** Los filtros `RichEditor` y `WindowCensusGuardTests`, que están en los `RequiredTests` de PB-T2b, escriben `RackFrameConfigurator.layout` en el perfil. Chocan con INV-PBT2B-10 («el perfil no cambia») y con C09. **(b)** Tras `GapBox`, el separador central o la estructura propia, el diseño de `LastComputation` del compuesto va una pasada por detrás. Hay que mirar las comparaciones `diseno-igual` de CT-PBT2B-02 y CT-PBT2C-03, y «Restaurar» | `F4B-PBT1C-…/1/R20261119T025420Z-ee6c/analysis.md` §1 y §4; F4B-E30 | Recogerlos en el paquete antes de emitir PB-T2b: la regla de atribución del archivo de disposición, como en la corrección de PB-T1c, y las comparaciones tomadas tras un recálculo |

## §6. Veredicto

**EMITIBLE TRAS CORRECCIONES — 0 / 0 / 6.**

**Qué hace falta:**
- M-1 a M-4, en el generador y en el texto de C11, antes de escribir;
- M-5, opcional;
- M-6, en el paquete antes de PB-T2b.

**Después:** la emisión se hace en cuanto cierre PB-T1c, con las comprobaciones del §3, sin AVISOS y con `BOUNDED_RESULT` puesto.

**Decisiones para el Owner:** ninguna nueva. PB-T2a se emite con la autorización condicional de F4B-E28, pregunta 4.
