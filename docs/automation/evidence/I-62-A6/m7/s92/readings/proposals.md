# I-62 — Propuestas para las ocho lecturas indeterminadas y limitación de L-18

```text
Tipo:      preparación para una disposición del Coordinator (decisiones §92, punto 5). No decide, no declara nada y no cambia
           ninguna pieza fijada.
Fecha:     2026-10-17
W:         %USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal, HEAD bb439090, solo lectura
Base:      r12.md (doce filas; custodiado en W, docs/automation/evidence/I-62-A6/m7/s91/r12/)
Aceptante: bmin_accept.py, blob 434ef1fc…, SHA-256 173514aa… (no se usa como fundamento ni como implementación de las
           comprobaciones)
Resultado: las 8 INDETERMINATE → (b), condición de fallo cerrado; L-18 → limitación de escenario; ninguna se dispara en el banco
```

## 0. Resumen

- **Mandato:** «Preparar para cada INDETERMINATE una interpretación normativa motivada o una condición de fallo cerrado.» [C-118]
  (DEC@ea575012:1816), «Sin adoptar interpretaciones por conveniencia de implementación.» [C-120] (DEC@ea575012:1816) y «Limitar
  los escenarios afectados por L-18 sin cambiar el significado de U-19» [C-119] (DEC@ea575012:1816). El registro de las doce
  filas: «COMPATIBLE = L-02, L-11, L-14; INCOMPATIBLE = L-18; INDETERMINATE = L-01, L-04, L-10, L-12, L-13, L-16, L-23, L-24»
  [C-124] (DEC@ea575012:1822).
- **Las ocho INDETERMINATE van por (b).** En ninguna una autoridad decide; por eso son indeterminadas.
  - En cinco de ellas (L-01, L-04, L-12, L-16 y L-23), la lectura mejor fundada en una autoridad **no coincide** con la del
    aceptante. Adoptarla como (a) dejaría al aceptante sin uso tal como está: «Sin esa coincidencia el aceptante no se usa tal como está: cambiarlo exige otro blob, otra corrección de A-6 y otra determinación del Owner.» [C-03] (A6@4bc919dd:709-710).
  - En las otras tres (L-10, L-13 y L-24) no hay lectura mejor fundada.
  - Las ocho dependen de los datos y el caso ambiguo no aparece ni en el banco ni en la parte de B1 que fija A5-1.
- **Siete comprobaciones:** FC-01, FC-04, FC-10, FC-12, FC-13, FC-16 (que cubre L-16 y L-23) y FC-24. Para L-18, FC-18 limita el
  escenario.
- **Banco:** ninguna se dispara (sonda P5).
- **B1:** la parte fijada por A5-1 no las dispara. Las partes que dependen de la ejecución (commits de `main` después de F_eff,
  contrato de FX-U2, delegaciones, entrega) se comprueban en la propia ronda.
- **Lo que no cambia:** el instrumento, el aceptante, A-6, U-19, las reglas del Controller, el oráculo, los presupuestos y el
  cierre de insumos.

## 1. Marco común de las comprobaciones

- **Ámbito:** «una invocación VERIFY con el instrumento es toda invocación VERIFY de FX-02 (la verificación, su reverificación de 16.7 si la hay y cada control negativo con invocación)» [C-113] (A6@4bc919dd:717-718), sobre B1.
  - **No** se aplican a la medición de A6-7: «esa invocación es una medición, no una invocación de rol de FX-02» [C-114]
    (A6@4bc919dd:771), juzgada «y R4 y R10 no aplicables, porque la medición no produce una verificación.» [C-08]
    (A6@4bc919dd:769-770). Aun así, la sonda dice qué darían sobre el banco.
  - También se ejecutan en la prueba previa, cuando «antes de esa primera invocación, la supervisión ejecuta el instrumento y el aceptante sobre los datos del fixture de la ronda, sin Controller» [C-123] (A6@4bc919dd:712-713), como aviso temprano.
- **Momento 1, antes de cada lanzamiento** (también antes del primero). Se hace en la ventana de la comprobación del punto de uso:
  «inmediatamente antes de cada lanzamiento VERIFY con el instrumento, después de la Salida de 16.4 y antes de la cesión, con la sesión retenida» [C-110] (A6@4bc919dd:567-568).
  Si falla, la invocación no se lanza: como en el punto de uso, «deja la invocación sin lanzar y sin contar» [C-111]
  (A6@4bc919dd:577-578). El caso va al Coordinator como ambigüedad material (S-03), con STOP.
- **Momento 2, tras cada Entrada.** Se hace en la ventana del aceptante, «después de la Entrada de 16.4 de cada invocación VERIFY con el instrumento y antes de cualquier escritura de la sesión en ese directorio» [C-112] (A6@4bc919dd:585), y antes de ejecutar el aceptante.
  Cubre las entradas que usará la solicitud: «`Verification` = la salida `controller-verification/v2` de la invocación, con la delegación y la entrega que cita, la ruta ignorada del paquete, la revisión de los términos de 16.10 y el `MainSha` de la invocación» [C-09] (A6@4bc919dd:618-619).
  Si falla, la salida no se ingiere y el caso va al Coordinator por la vía de «Si durante la ronda el análisis de una salida no aceptada muestra una divergencia de lectura no cubierta, se devuelve al Coordinator como ambigüedad material (S-03), con STOP» [C-04] (A6@4bc919dd:710-711), antes de cualquier otro efecto y sin reejecución.
- **Quién y cómo:**
  - la supervisión (plano a), en el `-C`, solo con lectura, con el Git declarado y sin variables `GIT_*`;
  - con un guion propio, fijado por SHA-256 en el kit de la ronda;
  - las reglas salen del texto de AP, nunca del código del aceptante ni del instrumento;
  - cada resultado (entradas, valores, PASS/FAIL) se custodia con la evidencia de la invocación.
- **Orden:** FC-01 primero (fija el EFF para todas las lecturas); después FC-04 y FC-13, y luego FC-12 (usa el lector que fija
  FC-13) y FC-10. FC-16, FC-18 y FC-24 no dependen de las demás. El primer fallo detiene.
- **Efecto:**
  - con PASS, las lecturas admisibles dan el mismo resultado en esa invocación, así que la disposición puede declarar la
    coincidencia del aceptante en ese dominio (A6-5, regla 6);
  - con FAIL, ninguna lectura decide nada.
- **Límites:**
  - el Controller no recibe ni la disposición ni las condiciones: «los insumos que esta regla añade a una invocación VERIFY son solo tres» [C-17] (A6@4bc919dd:513) y «ni el oráculo del Coordinator entran en ninguna invocación del Controller» [C-13] (A6@4bc919dd:515);
  - el Controller sigue aplicando «el Controller aplica a los hechos del instrumento las reglas de AUTOMATION_PLAN 16.9 y 16.13, incluida la tabla de defectos de `Validate(M)`, tal como están.» [C-05] (A6@4bc919dd:829-830);
  - las condiciones no añaden insumos al cierre ni tocan el contrato de salida.
- **Relación con §92, punto 4:** «Preparar una disposición limitada que excluya la orden `contract` del instrumento B-min durante las VERIFY de FX-02.» [C-121] (DEC@ea575012:1815). Es independiente de estas condiciones: FC-16 y FC-18 actúan sobre R4 `Scope` con o sin la orden `contract`.

## 2. Las ocho filas INDETERMINATE

### L-01 — qué cuenta como «commit con el trailer»

- **Opción: (b) FC-01.**
- **Por qué no (a):** el precedente al que V14 ancla el punto efectivo, «**Punto efectivo** (anclado como WORKFLOW §11.2)» [C-80]
  (V14@34ad80ea:831), dice «El trailer debe ser único en esa historia; ausencia, duplicidad o forma estructural distinta detiene la identificación.» [C-85] (WF@446c32ba:425-426).
  Una lectura (a) fundada en él detendría toda forma distinta, y no coincide con el aceptante, que ignora las variantes y cuenta
  las líneas del cuerpo [C-03]. FC-01 logra el efecto del precedente (detener ante cualquier forma distinta) sin fijar cuál de las
  lecturas es la normativa.
- **Entradas:**
  - el `MainSha_eval` de la invocación: antes del lanzamiento, el de la delegación o, en una reverificación, «`MainSha_eval` = `K.MainSha`, salvo en una reverificación de 16.7 sin conflictos, donde es el `MainSha` nuevo del `RebaseMap`» [C-37] (AP@f525cb1e:813-814); tras la Entrada, el que cita la salida [C-09];
  - los commits de `RevList(MainSha_eval)` con sus mensajes crudos.
- **Regla:** sea M el conjunto de los commits cuyo mensaje contiene `agent-protocol-normative` sin distinguir mayúsculas. PASS si
  y solo si |M| = 1 y su commit c cumple:
  1. el mensaje no tiene CR;
  2. una sola línea es igual, byte a byte, a `Agent-Protocol-Normative: I-62`;
  3. ninguna otra línea menciona la clave en ninguna forma;
  4. `git log -1 --format=%(trailers:key=Agent-Protocol-Normative,valueonly,unfold) c` con el Git declarado da exactamente `I-62`.

  Con PASS, todas las lecturas (línea exacta, tráiler de Git, precedente §11.2, subcadena) ven el mismo commit único, conforme a
  «es el primer merge en first-parent de `origin/main` cuyo segundo padre alcanza el **único** commit con el trailer» [C-30]
  (AP@f525cb1e:945), y derivan el mismo EFF.
- **Bloquea:** el lanzamiento (momento 1) o la ingestión (momento 2); S-03, con STOP.
- **Banco:** sin disparo [P-01]. La historia tiene 3 commits [P-20]; la clave aparece solo en `a9f6c929` [P-02], con el valor de
  tráiler de Git `I-62` [P-03]; la derivación da un único EFF [P-04].
- **B1:** en F_eff pasa por construcción: «Es el único commit del repositorio con el trailer» [C-91] (A5@ad94ceed:185), con la precondición «`Derive` da F_eff con un único commit con el trailer, el punto de entrada de WORKFLOW y 16.13 aparecen una sola vez, la tabla PRE de F_eff está bien formada y `Validate(M)` pasa MV-1..MV-7 (MAP_VALID)» [C-93] (A5@ad94ceed:189-191). Los commits de `main` posteriores a F_eff se comprueban en cada invocación; solo la disparan si mencionan la clave.
- **Riesgo residual:**
  - un STOP falso, conservador, si un commit del fixture cita la clave;
  - depende del analizador de tráileres del Git declarado, cuya identidad se comprueba en el punto de uso;
  - no examina commits fuera de `RevList(MainSha_eval)`: el universo ya quedó fijado en r12.

### L-04 — rigidez del JSON y semántica del validador (MV-1)

- **Opción: (b) FC-04.**
- **Por qué no (a):** el esquema declara su dialecto, «https://json-schema.org/draft/2020-12/schema» [C-104] (SCH@6b54dda9:2). La
  lectura (a) mejor fundada, validar según ese dialecto, no coincide con el aceptante en los casos límite: palabras desconocidas,
  igualdad numérica, `uniqueItems` y clases de `pattern` [C-03]. Los blobs fijados evitan esos casos.
- **Entradas:** el EFF de la invocación, derivado tras el PASS de FC-01; los blobs en EFF de
  `docs/automation/agent-execution/compatibility/I62-clause-map.json` y de `clause-map.schema.json`.
- **Regla:** PASS si y solo si son exactamente `4d49d3e1a63d2b6128c9d9eeac473156a421571a` y
  `6b54dda9579d694200d8fc9ec26a4a846eef529b`. Sobre ellos, el JSON estricto con el subconjunto de palabras clave y el validador
  completo de 2020-12 dan el mismo veredicto: válido, sin palabras fuera del subconjunto ni violaciones (r12, P1).
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** sin disparo [P-05].
- **B1:** A5-1 copia esos blobs, «el mapa de cláusulas, su esquema, AUTOMATION_PLAN 16.13 y 16.14 y las demás autoridades normativas no cambian, y ningún texto se adapta al fixture.» [C-95] (A5@ad94ceed:195-196): sin disparo.
- **Riesgo residual:** aproximación por exceso (cualquier otro blob detiene, aunque fuera inocuo). Con A5-1 solo ocurre en un
  fixture no conforme.

### L-10 — ENTRY de archivo presente en EFF^1 (MV-4)

- **Opción: (b) FC-10.**
- **Por qué no (a):** «| `BaseBlob` o `EffBlob` distintos de los observados | MAP_INVALID |» [C-50] (AP@f525cb1e:915) admite B-ext;
  la regla de MV-4 dice «ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` = blob(EFF, p)» [C-48] (AP@f525cb1e:905), y el análisis
  de s74 registra «Ningún texto exige que la ENTRY no exista en EFF^1.» [C-102] (S74@8c8dab0f:48). Ninguna autoridad decide.
- **Entradas:** `Files` del mapa en EFF y los árboles de EFF^1 y EFF.
- **Regla:** sea E el conjunto de rutas p con `FileKind` ENTRY y p ∈ árbol(EFF^1). Se dispara si y solo si E ≠ ∅ y no hay ningún
  defecto independiente de la lectura abierta, es decir:
  - MV-3 se cumple, «{p bajo `Surfaces` : blob(EFF^1, p) ≠ blob(EFF, p)} ∖ {mapa} = {`Files.Path`}, sin duplicados» [C-117]
    (AP@f525cb1e:904); y
  - MV-4 literal no tiene violaciones, como en la lectura A de s74, «| A: literal | L905 | **7** |» [C-115] (S74@8c8dab0f:107).

  Si hay uno de esos defectos, `Authority` no puede ser pass con ninguna lectura y L-10 no decide nada.
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** E contiene el esquema [P-06], pero MV-3 no se cumple [P-17] y MV-4 literal tiene 7 violaciones [P-07]. Coincide con
  A-5: «`Validate(M)` da MAP_INVALID (MV-3 con 31 rutas, MV-4 con 7, MV-6 con 69 y MV-7 con 2)» [C-116] (A5@ad94ceed:156-157).
  Sin disparo [P-08].
- **B1:** «ningún archivo que el mapa lista como ADDED o ENTRY, ni el propio mapa» [C-90] (A5@ad94ceed:179-180) en F_seed = EFF^1;
  E = ∅, sin disparo.
- **Riesgo residual:**
  - depende de MV-3 y de MV-4 literal, filas de una sola lectura (COINCIDE en readings.md: L-06 y L-07; no reexaminadas en r12);
  - la forma simple (E ≠ ∅ → STOP) evita esa dependencia; se dispararía con los datos del banco, pero el banco solo lo usa la
    medición, que está fuera del ámbito [C-114].

### L-12 — si «espacios colapsados» incluye los saltos de línea (MV-6, MV-7)

- **Opción: (b) FC-12.**
- **Por qué no (a):** la definición, «Una sección va desde su encabezado hasta el siguiente con el mismo número de `#` o menos, e incluye sus subsecciones; el texto se normaliza con CRLF → LF y espacios colapsados.» [C-61] (AP@f525cb1e:895-896), contiene un paso «CRLF → LF» que sería redundante si se colapsaran los saltos. Eso favorece conservar las líneas, que no coincide con el aceptante [C-03]. La lectura contraria tiene a su favor el precedente de `Match`, pero ese se aplica a una sola línea. No queda decidido.
- **Entradas:** para cada ruta MODIFIED del mapa en EFF presente en EFF^1 y en EFF, sus dos textos; además, el 16.3 de AP en EFF^1
  y en EFF.
- **Regla:**
  - secciones según AP 16.3 y 16.13, con el lector que deja fijado FC-13 (con PASS en FC-13, los lectores coinciden);
  - para cada archivo, el conjunto MODIFIED derivado tiene que ser igual con tres normalizaciones: N1 (todo blanco colapsado y
    extremos recortados), N2 (CRLF → LF, espacios y tabuladores colapsados dentro de cada línea, líneas conservadas) y N3 (N2 con
    cada línea recortada);
  - la igualdad de MV-7 (16.3 en EFF sin el puntero frente a 16.3 en EFF^1) tiene que dar el mismo valor con N1, N2 y N3;
  - N1 colapsa los blancos Unicode y N2/N3 solo los ASCII, así que también cubre la duda entre «espacios» ASCII y Unicode.
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** sin disparo [P-09]. Los textos de EFF^1 y EFF son idénticos [P-24], y la igualdad de MV-7 da falso con las tres
  normalizaciones [P-21].
- **B1:** sin disparo [P-14]. Las tres normalizaciones coinciden en los seis archivos [P-22][P-23] y en MV-7 [P-15].
- **Riesgo residual:** cubre dos variantes que conservan las líneas y la diferencia entre blancos ASCII y Unicode, pero no otras
  normalizaciones concebibles, que el texto no sugiere. Depende de FC-13.

### L-13 — encabezados y bloques de código (lector Markdown)

- **Opción: (b) FC-13.**
- **Por qué no (a):** `Match` habla de «encabezados» [C-57] y de «bloques de código» [C-57], pero ningún texto los define:
  «cuenta los encabezados del archivo en `rev` cuya línea, normalizada, es igual a `Section` normalizado (normalizar = espacios colapsados y extremos recortados); se ignoran las líneas dentro de bloques de código.» [C-57] (AP@f525cb1e:872-873). Solo la sección por número de `#` [C-61] excluye los setext. No hay autoridad que decida lo demás.
- **Entradas:**
  - los archivos MODIFIED del mapa en EFF^1 y en EFF;
  - WORKFLOW y AP en `MainSha_eval`, porque E2-E3 se leen ahí: «X := Match(MainSha_eval, WORKFLOW, <línea de encabezado del punto de entrada>)» [C-122] (AP@f525cb1e:784);
  - AP en EFF^1, para MV-7.
- **Regla:** PASS si ninguna línea presenta uno de estos casos:
  - (i) una línea de aspecto de encabezado tras blancos iniciales;
  - (ii) una valla de tildes;
  - (iii) una valla de comillas invertidas sangrada de 1 a 3 espacios;
  - (iv) un subrayado setext justo después de una línea de párrafo;
  - (v) una línea de aspecto de encabezado dentro de un bloque HTML (desde una línea que empieza por `<` hasta la siguiente en
    blanco);
  - (vi) un marcador de contenedor (`>` o de lista) seguido de una línea de aspecto de encabezado;
  - (vii) una valla de comillas invertidas que se cierra con otra longitud o con cadena de información, o que no se cierra;
  - (viii) una línea de aspecto de encabezado con tabulador o con blanco no ASCII.

  Son los casos en los que difieren una lectura CommonMark y la lectura "vallas de la columna 0 y encabezados con cualquier
  sangría".
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** sin disparo [P-10]. **B1:** sin disparo [P-16]; además, r12 (P1) no encontró ninguna diferencia de nivel de
  encabezado.
- **Riesgo residual:**
  - son condiciones suficientes y sintácticas, sacadas de las diferencias conocidas; queda el riesgo, bajo, de una construcción
    no listada;
  - es conservadora: puede detener por una construcción inocua;
  - cubre el caso de que `main` cambie WORKFLOW o AP después de F_eff.

### L-16 y L-23 — entradas de alcance mal formadas o vacías

- **Opción: (b) FC-16, compartida.**
- **Por qué no (a), L-16:** la lectura mejor fundada es la sintaxis positiva de «ruta relativa a la raíz con `/` y la capitalización exacta, archivo exacto o prefijo de directorio terminado en `/`, sin comodines, `..` ni rutas absolutas.» [C-71] (AP@f525cb1e:552). Excluye más entradas que el aceptante (vacías, segmentos `.` o vacíos, blancos extremos) y no coincide con él [C-03].
- **Por qué no (a), L-23:**
  - el literal de «| 7 | `Scope` | `git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope` y sin intersección con `ForbiddenWriteScope` | Controller | STOP |» [C-73] (AP@f525cb1e:636), con «R está cubierta por E si R = E, o si E termina en `/` y R empieza por E.» [C-72] (AP@f525cb1e:553), compara cadenas;
  - el aceptante, en cambio, falla `Scope` con una entrada mal formada, con el apoyo de «Una comprobación cuya comparación no se pudo ejecutar o no terminó es `not_run`, y `not_run` cuenta como fail; nunca es pass» [C-47] (AP@f525cb1e:1204-1205);
  - ninguna de las dos decide, y la lectura literal no coincide con el aceptante [C-03];
  - el subpunto del diff vacío está fijado (∅ ⊆ A) y ahí el aceptante sí coincide.
- **Entradas:** `AllowedWriteScope` y `ForbiddenWriteScope` del contrato K y de la delegación D de la invocación (real o copia de
  control), con los bytes fijados por blob en el cierre de la invocación.
- **Regla:** toda entrada cumple las siguientes condiciones:
  - no está vacía;
  - no tiene blancos en los extremos ni caracteres de control;
  - no es absoluta (ni `/` inicial ni letra de unidad);
  - no tiene barra invertida;
  - no tiene `*`, `?` ni `[` salvo en un `/**` final (esta cláusula es FC-18);
  - no tiene segmentos `.`, `..` ni vacíos (se admite una sola `/` final para los prefijos).

  Con PASS, el conjunto de entradas mal formadas está vacío con las dos lecturas: la regla del aceptante sobre entradas mal
  formadas no actúa en R4 `Scope`, la comparación literal da lo mismo y `MalformedEntries` está vacía en R7 para las dos piezas.
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** sin disparo [P-11], sobre el contrato T1, la delegación del kit y las doce delegaciones del banco, incluida la copia
  mutada de nc2.
- **B1:** el contrato de FX-U2 lo redacta el Coordinator del fixture. La delegación la emite el Controller: A3 limita las
  permitidas, «| A3 | `AllowedWriteScope` ⊆ alcance permitido del contrato | STOP (P-03) |» [C-76] (AP@f525cb1e:562), y A4 admite añadidos en las prohibidas, «| A4 | `ForbiddenWriteScope` ⊇ prohibido del contrato | STOP (P-03) |» [C-79] (AP@f525cb1e:563). Solo se dispara si aparece una entrada así.
- **Riesgo residual:** es conservadora (detiene por entradas inocuas con las dos lecturas, como un `./x` añadido a las
  prohibidas) y no comprueba ni mayúsculas ni existencia, que no son sintácticas.

### L-24 — conjunto de rutas del diff (renombres)

- **Opción: (b) FC-24.**
- **Por qué no (a):** el literal es una orden. Lo fijan [C-73], «`Scope` = pass exige en su `Evidence` la salida reproducible de `git diff --name-only BaseSha..CurrentSha` y la entrada de `AllowedWriteScope` que contiene cada ruta.» [C-74] (AP@f525cb1e:1205-1206), y en A6-7 «`git diff --name-only <base>..<head>` reproducible y la pertenencia por ruta de `Scope`» [C-14] (A6@4bc919dd:762). Su salida depende de una configuración de Git que nadie fija (r12, P4: por defecto lista solo el destino). Ninguna lectura queda decidida, y la de la configuración por defecto no coincide con el aceptante [C-03].
- **Entradas:** `D.BaseSha` y `H.CurrentSha` de la invocación (real o copia).
- **Regla:**
  - si alguno no es un commit existente, no se aplica: no hay conjunto de rutas con ninguna lectura (caso nc1);
  - si no, con el Git declarado y sin `GIT_*`: el conjunto de `git -c diff.renames=true diff --name-only B..C` tiene que ser
    igual al de `git diff --name-only --no-renames B..C`;
  - toda ruta tiene que ser ASCII imprimible, sin comillas dobles ni barra invertida, para que `core.quotepath` no altere la
    salida literal.
- **Bloquea:** el lanzamiento o la ingestión; S-03.
- **Banco:** sin disparo [P-13], en diez casos aplicables (n7b, con 31 rutas [P-19]); nc1 no es aplicable [P-18].
- **B1:** depende de la entrega. Solo se dispara si el Worker renombra un archivo o crea una ruta no ASCII.
- **Riesgo residual:** es conservadora (cualquier renombre detiene, aunque fuera inocuo). Además, se adelanta a lo que provocaría
  un renombre entre el instrumento (`--no-renames`) y una orden primaria por defecto: «Una discrepancia entre un hecho del instrumento y la orden Git primaria es evidencia contradictoria» [C-10] (A6@4bc919dd:669-670).

## 3. L-18 (INCOMPATIBLE): limitación de escenario FC-18

- **U-19 sigue tal como está registrada.** §62, punto 3: «Se registran las disposiciones ordinarias que indica el Coordinator sobre» [C-20] (DEC@ea575012:1215).
  El texto: «comodín no final = lista exacta de rutas en el árbol de BaseSha, enumerada en la orden» [C-21] (DEC@ea575012:1234),
  dispuesto entero: «| U-19 | entera | — |» [C-22] (DEC@ea575012:1250). La propuesta de la que procede: «Una entrada con comodín en otra posición se lee como la lista exacta de rutas que casan en el árbol de `BaseSha`, enumerada en la orden; un archivo nuevo fuera de `AllowedWriteScope` falla `Scope` igualmente.» [C-99] (UNB@203d2887:89). La orden de FX-U2 conserva el marcador «`{SCOPE_READING}`» [C-100] (UNB@203d2887:89) con U-19 completa.
- **Comprobación FC-18:**
  - *Entradas:* las listas de alcance del contrato y de cada delegación (verificación, reverificación y cada copia de control).
  - *Regla:* ninguna entrada contiene `*`, `?` ni `[` fuera de un `/**` final.
  - *Momentos:* el 1 y el 2 del §1. Se recomienda además en la emisión del contrato y en la aceptación A1-A8 de cada delegación,
    para detectarlo antes de cualquier trabajo.
  - *Bloquea:* el lanzamiento o la ingestión; S-03, con STOP, al Coordinator.
- **Por qué es una limitación y no una reinterpretación:**
  - U-19 (b) dice cómo se lee una entrada así *si existe*. FC-18 solo decide que FX-02 se ejecuta con entradas que no la tienen:
    no le da otro significado y no la declara mal formada.
  - Lo que trata como mal formada a esa entrada es el aceptante, y FC-18 hace que esa regla nunca se ejerza.
  - Si una entrada así aparece, no decide ni U-19 (b) ni el aceptante: decide el Coordinator (S-03).
  - No hay ninguna lectura contraria al texto congelado: «no se declara conformidad con una interpretación informal contraria al texto congelado» [C-23] (DEC@ea575012:1456). La sintaxis de AP ya prescribe «sin comodines» [C-71].
- **Coincidencia para A6-5, regla 6:** en el dominio que admite FC-18, U-19 (b) es vacía y la regla del aceptante sobre comodines
  no actúa, así que las lecturas coinciden. Fuera de él, S-03. U-19 (a) (`/**` final) sigue cubierta por L-17 de readings.md.
- **Banco:** sin disparo [P-12]. **B1:** solo si un comodín no final llega al contrato de FX-U2 o a una delegación.
- **Riesgo residual:** si el contrato de FX-U2 necesitara un comodín, el Coordinator tendría que enumerar las rutas exactas en las
  propias listas. Eso es compatible con que la enumeración esté «en la orden», que dice U-19.

## 4. Tabla consolidada

| Fila | Opción | Comprobación | Bloquea | Banco | B1 (A5-1) |
|---|---|---|---|---|---|
| L-01 | (b) | FC-01 | lanzamiento o ingestión; S-03 | no se dispara | en F_eff, no por construcción; los commits posteriores se comprueban en ejecución |
| L-04 | (b) | FC-04 | lanzamiento o ingestión; S-03 | no se dispara | no (blobs copiados sin adaptar) |
| L-10 | (b) | FC-10 | lanzamiento o ingestión; S-03 | no se dispara (E ≠ ∅, pero MV-3 y MV-4 literal ya fallan) | no (F_seed sin ENTRY) |
| L-12 | (b) | FC-12 | lanzamiento o ingestión; S-03 | no se dispara | no (tres normalizaciones coinciden) |
| L-13 | (b) | FC-13 | lanzamiento o ingestión; S-03 | no se dispara | no (ninguna construcción; WORKFLOW y AP en `MainSha_eval`, en ejecución) |
| L-16 | (b) | FC-16 | lanzamiento o ingestión; S-03 | no se dispara | solo si una entrada así llega al contrato o a la delegación |
| L-23 | (b) | FC-16 | lanzamiento o ingestión; S-03 | no se dispara | ídem |
| L-24 | (b) | FC-24 | lanzamiento o ingestión; S-03 | no se dispara (nc1 no aplicable) | solo si la entrega renombra un archivo o crea una ruta no ASCII |
| L-18 | (lim) | FC-18 | lanzamiento o ingestión; S-03 | no se dispara | solo si un comodín no final llega al contrato o a la delegación |

## 5. Borrador de la disposición de lecturas (A6-5, regla 6)

Texto que el Coordinator podría emitir; no lo emite este análisis. Contiene la cadena `A6-5, regla 6`, que exige la puerta de
decisiones del kit de la medición para la línea `M7-LECTURAS`.

> **Disposición del Coordinator: lecturas normativas del aceptante fijado (A-6, A6-5, regla 6).**
>
> 1. *Objeto.* Aceptante `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, blob
>    `434ef1fccb0d15762b30244b4a60928120698411`, SHA-256 `173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0`.
>    Fuentes: `r12.md` (registro de §92, punto 5) y `proposals.md`, con los SHA-256 que se registran con esta disposición.
> 2. *Lecturas compatibles, adoptadas.* El Coordinator fija como suya la lectura del aceptante y declara que coincide:
>    - **L-02**: el punto de entrada "nombra" 16.13 si su sección contiene literalmente la línea exacta de 16.13 (AP L728). Con la
>      lectura de línea propia, el WORKFLOW de MC_I62 no pasaría MV-7 y B1 no cumpliría A5-1, regla 4.
>    - **L-11**: MV-5 compara `Section` normalizada, porque `Match` (AP L872-L873) fija la identidad de una designación.
>    - **L-14**: los punteros se comprueban en sus posiciones (AP L730); "sin el puntero" quita la frase; 16.13 repetida ya es
>      ENTRY_INVALID (AP L729, E3).
> 3. *Lecturas coincidentes de readings.md* (L-03, L-05, L-06, L-07, L-08, L-09, L-15, L-17, L-19, L-20, L-21, L-22; blob
>    `0991419a`): se adoptan tal como las redacta readings.md, sin reexamen en r12, con las salvedades del punto 7.
> 4. *Lecturas indeterminadas* (L-01, L-04, L-10, L-12, L-13, L-16, L-23 y L-24). El Coordinator no elige entre las lecturas
>    admisibles. Fija las condiciones de fallo cerrado FC-01, FC-04, FC-10, FC-12, FC-13, FC-16 y FC-24 de `proposals.md` §2:
>    - la supervisión las comprueba, con el Git declarado y solo en lectura, sobre los datos de cada invocación VERIFY de FX-02
>      con el instrumento, antes de su lanzamiento y otra vez tras su Entrada, antes del aceptante;
>    - mientras todas pasan, las lecturas admisibles dan el mismo resultado, y en ese dominio el Coordinator declara que su
>      lectura coincide con la del aceptante;
>    - si una falla, la invocación no se lanza (sin contar) o su salida no se ingiere, y el caso vuelve al Coordinator como
>      ambigüedad material (S-03), con STOP, sin aplicar ninguna lectura.
> 5. *L-18, limitación de escenario.* FX-02 se ejecuta sin entradas con comodín no final en el contrato ni en ninguna delegación
>    o copia de control (FC-18). Si aparece una, S-03, con STOP. U-19 conserva su texto y su significado tal como se registraron
>    (§62, punto 3), y la orden lleva U-19 completa. En el dominio de FC-18, el Coordinator declara la coincidencia.
> 6. *Momento y custodia.* Las condiciones se ejecutan también en la prueba previa de A6-5, regla 6, sobre los datos de la ronda.
>    Cada resultado se custodia con la evidencia de su invocación. El guion de las condiciones se fija por SHA-256 en el kit de la
>    ronda antes del primer lanzamiento.
> 7. *Excepciones y salvedades:*
>    - (a) las lecturas propias del aceptante en R4 `Identity`, `Trailer`, `FreeText` y `CleanTree` quedan fuera: una divergencia
>      sobre ellas sigue la regla 6 (S-03, STOP);
>    - (b) U-19 (c) no la aplica el aceptante (coincidencia vacía, L-19);
>    - (c) A3 y A4 con U-19 (L-20 a L-22) y la divergencia D-1 quedan sujetas a la disposición de §92, punto 4 (exclusión de la
>      orden `contract`), todavía no emitida;
>    - (d) en L-09, la regla de archivo ADDED descansa en R61 b (AP L835), no en AP L916, que trata de entradas;
>    - (e) MV-7 y E2-E3 no figuran en el paréntesis de la regla 6, pero quedan cubiertos por L-01, L-02, L-14, FC-01 y FC-13;
>    - (f) esta disposición y sus condiciones no se entregan al Controller, que aplica 16.9 y 16.13 tal como están (A6-9, regla 2);
>    - (g) no cambia el instrumento, el aceptante, A-6, U-19, el oráculo, los presupuestos ni el cierre de insumos;
>    - (h) las condiciones FC no se aplican a la medición de A6-7, donde R4 y R10 no son aplicables; las lecturas adoptadas en
>      los puntos 2 y 3 sí rigen para lo que la medición compara en R5-R7.

Citas de apoyo para el borrador: R61 b, «b. Path con FileKind ADDED (no existía en EFF^1)» [C-125] (AP@f525cb1e:835); el recuento
de readings.md, «COINCIDE 12 · AMBIGUOUS-ACCEPTOR-CHOICE 12 · DIVERGE 0 (24 filas)» [R-01] (RD@0991419a:13); y el cambio de pieza
que evitan las condiciones, «un instrumento o un aceptante con otro blob no está cubierto por la determinación: su uso exige otra corrección de A-6 y otra determinación del Owner.» [C-07] (A6@4bc919dd:815-816).

## 6. Verificación y sonda

- `python -I verify_proposals.py`: comprueba las citas de `proposals-citations.json` contra los blobs de W, los hechos de la sonda,
  la tabla del §4 frente a `proposals.json` y la higiene. Escribe `proposals-verify-result.json`.
- `python -I probes/probe_proposals.py` (P5): evalúa FC-01, FC-04, FC-10, FC-12, FC-13, FC-16/FC-18 y FC-24 sobre el banco y sobre
  la parte de B1 que fija A5-1.
  - Es una implementación propia desde el texto de AP: no usa el código del aceptante ni el del instrumento.
  - Solo lee W, el banco `D:\r62-a6r2\r3\deliv` (`--no-optional-locks`, sin configuración de sistema ni global) y las
    delegaciones y entregas del banco.
  - Solo escribe `probes/probe-proposals-result.json`.
