# B-min — especificación del paquete experimental (I-62, decisiones §75)

> **Experimental, no normativo y fuera de producción.** Autoridad: decisiones §75, puntos 1, 2, 3 y 5. No entra en F_seed ni en F_norm, ni en ningún
> contrato de producción. Durante la preparación no se ejecuta ningún modelo. Normativa de referencia: `docs/AUTOMATION_PLAN.md`, blob `f525cb1e`
> («AP» en adelante; mismo blob en MC_I62 `6f0187cb`, en la punta de I-62 y en el EFF del fixture `fbe25347`).

## 0. Reparto de responsabilidades (§75.3)

| Pieza | Hace | No hace nunca |
|---|---|---|
| **Herramienta** `bmin_facts.py` | calcula **hechos** reproducibles a partir de objetos Git fijados por SHA y de archivos fijados por SHA-256 | emitir PASS, FAIL, VERIFIED, `EXECUTION_*`, MAP_VALID, MAP_INVALID, una `Classification`, una `Disposition` o un `Result` de comprobación |
| **Controller** (fuera de este paquete) | interpreta y contrasta los hechos, cruza al menos la orden Git primaria y emite su `controller-verification/v2` o su `delegation/v2` | copiar un veredicto de la herramienta (no lo hay) |
| **Aceptante** `bmin_accept.py` | reproduce R1-R10; recalcula los controles críticos con una **implementación independiente**; rechaza toda discrepancia | usar la herramienta del Controller como oráculo de C-24 |

**Independencia de los oráculos.** Ninguna de las dos implementaciones se construye leyendo `probe_tools.py`, `mv_check.py`, `clause_map.py`, ni los
`reference.json` o `comparison.json` de las sondas: solo esta especificación y el texto normativo. `probe_tools.py` nunca es instrumento del
Controller. La herramienta y el aceptante los escriben autores distintos sin ver el código del otro.

## 1. Reglas comunes de la herramienta

1. **Lenguaje:** Python 3, solo la biblioteca estándar. Se ejecuta como `python -I bmin_facts.py <orden> ...` o desde el objeto Git
   (`git cat-file blob <blob> | python -I - <orden> ...`). No depende de `__file__` ni del directorio actual.
2. **Git:** solo a través de un único ejecutor con lista cerrada de subórdenes: `rev-parse`, `cat-file`, `ls-tree`, `rev-list`, `diff`, `log`, `show`,
   `merge-base`, `status`, `check-ignore`.
   - `diff` y `show` siempre con `--no-ext-diff --no-textconv`; `status` siempre con `--porcelain`.
   - Prohibidos: `-w`, `--write`, `fetch`, `pull`, `push`, `ls-remote`, `remote`, `config` y cualquier suborden fuera de la lista.
   - Entorno de cada llamada: `GIT_OPTIONAL_LOCKS=0`, `GIT_TERMINAL_PROMPT=0`, `LC_ALL=C`, más `-c core.quotepath=false -c core.fsmonitor=false`.
   - Sin `shell=True`. Una suborden fuera de la lista es `FORBIDDEN_OPERATION`.
3. **Cero escrituras y cero red:**
   - ninguna apertura de archivo en modo escritura;
   - ni `os.remove`, `rename`, `mkdir`, `shutil`, `tempfile`, `socket`, `urllib`, `http` ni `ssl`;
   - ningún proceso distinto de `git`.
4. **Entradas cerradas:**
   - todo commit se pasa como SHA completo de 40 hexadecimales; un SHA que no es un commit existente es `BAD_REVISION`;
   - todo archivo fuera de Git (entrega, hechos del relevo, TRX, delegación) se pasa como `--file NOMBRE=RUTA` con `--sha256 NOMBRE=HEX`; un SHA-256
     distinto es `INPUT_HASH_MISMATCH`;
   - las rutas dentro del repositorio usan `/`.
5. **Salida canónica:** un único objeto JSON por stdout, `json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))` seguido de `\n`.
   Todos los bytes son < 0x80; el texto no ASCII sale como `\uXXXX` (fidelidad por representación ASCII). Sobre:

   ```json
   {"Schema": "rackcad-bmin-facts/0-experimental", "Command": "<orden>", "Args": {...}, "Inputs": [...], "GitCalls": [[...argv sin el binario...]],
    "Facts": {...} | null, "FactsSha256": "<sha256 de la serialización canónica de Facts>" | null, "Error": null | {"Code": "...", "Detail": "..."},
    "Runtime": {"Python": "<platform.python_version()>", "Isolated": true|false}}
   ```

   - `Inputs`: `{"Kind": "commit", "Sha": ...}`, `{"Kind": "git-blob", "Rev": ..., "Path": ..., "Blob": ...}` o
     `{"Kind": "file", "Name": ..., "Sha256": ...}`;
   - `GitCalls`: las llamadas en orden (son las lecturas declaradas, para R9);
   - `Runtime` queda fuera de `FactsSha256`;
   - `Isolated` = `sys.flags.isolated == 1`. Sin `-I`, la herramienta sale con el error `NOT_ISOLATED`.
6. **Fallo cerrado:** un dato obligatorio ausente, ilegible, contradictorio o ambiguo da `Facts` = null, `Error` con código y exit 2. Códigos:
   `BAD_REVISION`, `MISSING_INPUT`, `INPUT_HASH_MISMATCH`, `MALFORMED_JSON`, `MALFORMED_XML`, `AMBIGUOUS`, `CONTRADICTORY`, `UNSUPPORTED_SCHEMA_KEYWORD`,
   `FORBIDDEN_OPERATION`, `GIT_ERROR`, `NOT_ISOLATED`, `USAGE` (exit 64). Nunca se emiten hechos parciales.
   **Excepción definida:** cuando la norma convierte una ausencia en un hecho, esta especificación la nombra como hecho. Ejemplos: un archivo ausente en
   una revisión es `null`; `merge-base --is-ancestor` con rc 1 es `false`; `check-ignore` con rc 1 es un hecho; un mapa ausente en EFF es `MapPresent` =
   false. Cualquier otro código de retorno inesperado es `GIT_ERROR`.
7. **Determinismo:** misma entrada, mismos bytes de `Facts`. Las listas salen en el orden que fija cada orden: el de Git, o el ordenado que se indique.
8. **Vocabulario prohibido en `Facts`** (claves y valores): `pass`, `fail`, `PASS`, `FAIL`, `VERIFIED`, `EXECUTION_`, `MAP_VALID`, `MAP_INVALID`,
   `REWORK`, `BLOCKED`, `STOP`. Los hechos son valores, listas, recuentos y booleanos con nombre explícito.
9. **Normalización de texto** (AP L896; L873): CRLF → LF; para comparar, espacios colapsados (`\s+` → un espacio) y extremos recortados. Una sección va
   desde su encabezado hasta el siguiente con el mismo número de `#` o menos; preámbulo = texto anterior al primer `##`. Las líneas dentro de bloques
   de código (entre líneas que empiezan por tres comillas invertidas) no son encabezados.
10. **Frase** (para MV-7): se parte el texto normalizado en frases por el límite «`.` seguido de espacio en blanco o fin de texto». Un punto seguido de un
   dígito (`16.13`) no es límite. Cada frase conserva su punto final.

## 2. Órdenes y hechos

Todas las órdenes llevan `--repo <ruta del clon>`.

### 2.1 `identity --branch B --base S --current S [--red S]` (AP 16.5 A6; 16.9 #5)

`Facts`:
- `Head`, `AbbrevRef` y `Toplevel` (con `/`);
- `OriginBranch` = `rev-parse refs/remotes/origin/B`, o null si no existe; `OriginMain` = `refs/remotes/origin/main`, o null;
- `Ancestry`: objeto con las claves `base->current`, `base->red` y `red->current` (las de red, solo con `--red`). Valor: el booleano de
  `merge-base --is-ancestor`.

### 2.2 `diff --from A --to B` (16.9 #7, #11)

`Facts`:
- `Paths` = `git diff --name-only A..B`, en el orden de Git;
- `Commits` = `git rev-list --reverse A..B`;
- `CoAuthoredBy` = objeto commit → lista de valores de `Co-Authored-By`, en orden (`git log -1 --format=%(trailers:key=Co-Authored-By,valueonly)`;
  líneas no vacías).

### 2.3 `scope --from A --to B --file DELEG=RUTA --sha256 DELEG=HEX` (16.9 #7; AP L552-L554; U-19)

La delegación aporta `AllowedWriteScope` y `ForbiddenWriteScope`. `Facts`:
- `Paths` (como 2.2);
- `PerPath` = `[{"Path", "AllowedCoveredBy": [...], "ForbiddenCoveredBy": [...]}]`, en el orden de `Paths`;
- `AllowedEntries` y `ForbiddenEntries`, tal cual;
- `MalformedEntries` = entradas absolutas, con `..`, con comodín (`*`, `?`, `[`) distinto del sufijo `/**`, o con `\`;
- `U19Normalized` = entradas que terminan en `/**`, leídas como el prefijo terminado en `/` (decisión U-19).

**Cobertura** (AP L553): R está cubierta por E si R = E, o si E termina en `/` y R empieza por E (con E ya normalizada por U-19). Sin distinguir
mayúsculas: nunca.

### 2.4 `contract --file CONTRACT=RUTA --sha256 CONTRACT=HEX --file DELEG=RUTA --sha256 DELEG=HEX` (16.5 A3-A5; 16.22 A3'-A5')

`Facts`:
- `A3` = `[{"Entry", "ContainedBy": [...]}]`, por cada entrada de `AllowedWriteScope` de la delegación, frente al alcance permitido del contrato. A ⊆ B
  por entradas (AP L553-L554): un prefijo de A solo lo contiene un prefijo de B que lo contenga;
- `A4` = `[{"Entry", "ContainedBy": [...]}]`, por cada entrada prohibida del contrato, frente a `ForbiddenWriteScope` de la delegación;
- `A5Missing` = objeto con las claves `Invariants`, `StopConditions` (por id), `Authorities` (por tripla `Path|Section|Class`), `RequiredTests` (por
  `Filter`) y `RoleRequirements` (por `Role`). Valor: lista de los elementos del contrato ausentes de la delegación;
- `RequiredTestsPairs` = `[{"Filter", "ContractMinSelected", "DelegMinSelected", "ContractExpectRed", "DelegExpectRed"}]`;
- `SupersededEqual` = booleano de igualdad de `SupersededCommits`.

Los nombres de campo del contrato se leen de su esquema `gate-contract/v2`. Si falta un campo obligatorio: `MISSING_INPUT`.

### 2.5 `hashes --rev S --paths p...` (AP 16.24)

`Facts.PerPath` = `[{"Path", "WorktreeRawBlobSha1", "WorktreeSha256", "RevBlob", "RevBlobSha256"}]`.
- `WorktreeRawBlobSha1` = SHA-1 de `blob <len>\0<bytes>` sobre los bytes del archivo en disco;
- `RevBlobSha256` = SHA-256 del contenido del blob en S;
- los valores ausentes son null.

### 2.6 `cleantree --ignore-path P` (16.9 #8)

`Facts`:
- `StatusPorcelain` y `StatusIgnored` (`status --porcelain` y `status --porcelain --ignored`), como listas de líneas;
- `CheckIgnoreOutput` y `CheckIgnoreRc` (`check-ignore -v --no-index P`; rc 0 o 1; otro rc es `GIT_ERROR`).

### 2.7 `read-text --rev S --path P [--lines a-b]` (fidelidad; AP 16.24)

`Facts`:
- `Blob`;
- `Lines` = `[{"N", "Text"}]`, sin fin de línea;
- `NonAsciiCount`;
- `NonAsciiChars` = `[{"CodePoint": "U+XXXX", "Count"}]`, ordenado por punto de código.

Ruta ausente en S: `MISSING_INPUT`.

### 2.8 `read-json (--rev S --path P | --file N=RUTA --sha256 N=HEX) --pointer /a/b ...`

`Facts.Values` = puntero → valor JSON, o `{"Absent": true}`, con punteros RFC 6901. JSON ilegible: `MALFORMED_JSON`.

### 2.9 `trx --file TRX=RUTA --sha256 TRX=HEX [--filter-substring S]` (16.9 #10)

`Facts`:
- `Sha256`;
- `Counters` = atributos de `ResultSummary/Counters`, como cadenas;
- `Tests` = `[{"TestName", "Outcome"}]`, en el orden del documento;
- `FailedTests`;
- `FilterSelected` y `FilterFailed`: `testName` que contienen S, en total y fallidas.

El análisis es independiente del espacio de nombres. XML ilegible: `MALFORMED_XML`.

### 2.10 `handoff --file H=RUTA --sha256 H=HEX --schema-rev S [--allow-absent]` (16.9 #2)

- Con `--allow-absent` y el archivo ausente: `Facts` = `{"Present": false}`.
- Si no: `Present`, `ValidJson`, `MissingRequiredKeys`, `Fields` y `Sha256`.
  - `MissingRequiredKeys` se calcula frente a `required` de `docs/automation/agent-execution/schemas/worker-handoff.schema.json` en S;
  - `Fields` incluye `TaskId`, `Attempt`, `DelegationRunId`, `RunId`, `BaseSha`, `RedSha`, `CurrentSha`, `Branch`, `Worktree` y `Worker`.

### 2.11 `freetext --file H=RUTA --sha256 H=HEX --terms-rev S` (16.10)

`Facts`:
- `Terms`: los términos entre comillas invertidas del párrafo de 16.10 en `docs/AUTOMATION_PLAN.md` en S, en orden. El párrafo va desde la línea
  siguiente a `### 16.10 Términos de gate` hasta la frase «No se aplica …». Si se obtienen 0 términos: `CONTRADICTORY`;
- `Fields`: `WorkCompleted`, `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y `RecommendedNextAction`;
- `Matches` = `[{"Field", "Term", "Count"}]`, solo con recuento > 0, ordenado por (Field, Term).

Coincidencia sin distinguir mayúsculas (`casefold`), por palabra o frase completa: sin carácter alfanumérico ni `_` a los lados.

### 2.12 `clausemap --main S [--state-rev B --state-path P] [--file CONTRACT=RUTA --sha256 CONTRACT=HEX]` (AP 16.13, 16.14)

Constantes normativas (AP L728, L717, L731-L732, L431):
- punto de entrada `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)`;
- encabezado de 16.13 `### 16.13 Compatibilidad de protocolos de ejecución delegada`;
- mapa `docs/automation/agent-execution/compatibility/I62-clause-map.json`;
- esquema `docs/automation/agent-execution/compatibility/clause-map.schema.json`;
- puntero `Antes de aplicar esta sección, toda unidad aplica §16.13.`;
- trailer `Agent-Protocol-Normative: I-62`.

`Facts`:

- **`Derive`** (16.14):
  - `TrailerCommits`: commits alcanzables desde S con una línea de mensaje exactamente igual al trailer;
  - `FirstParentMerges`: `rev-list --first-parent --merges S`, del más antiguo al más reciente;
  - `Candidates`: los merges de esa lista cuyo segundo padre alcanza el único commit con el trailer y cuyo primer padre no lo alcanza. Lista vacía si
    hay 0 o más de un commit con el trailer;
  - `Eff`: el primer candidato, o null; `EffParent1` y `EffParent2`.
- **`Pre`** (16.13, Classify paso 3): cuerpo de `Eff` con CRLF → LF.
  - `HeaderFound` y `HeaderCells`. La cabecera es la primera línea que, sin el `|` inicial ni espacios, empieza por `Initiative |`;
  - `Rows`: las líneas consecutivas siguientes que contienen `|`, hasta una línea en blanco o una sin `|`, partidas por `|` con las celdas
    recortadas, quitando las vacías de los extremos;
  - `MalformedRows`: filas con un número de celdas distinto de la cabecera;
  - `ClaimIdColumn` (índice de la celda `Claim-Id`, o null);
  - `ClaimIdCounts`.
- **`Classify`** (con `--state-rev` y `--state-path`):
  - `ClaimIdLines`: valores de las líneas `^\s*claim_id:\s*(\S+)` del estado en B, en orden;
  - `ClaimIdInPre`: recuento del único `claim_id`. Si hay 0 o más de un `claim_id`, null.
- **`Entry`** (E1-E3, en S):
  - `WorkflowBlob` y `ApBlob`;
  - `EntryMatches`: números de línea de WORKFLOW cuya línea normalizada es igual al punto de entrada, nivel `##`, fuera de código;
  - `R163Matches`, lo mismo en AP para el encabezado de 16.13;
  - `EntryNamesR163`: si la sección del punto de entrada (única, si lo es) contiene literalmente la línea de 16.13; null si no es única.
- **`Validate`** (solo con `Eff` no null; si no, null), en Eff, con Eff^1 = `EffParent1`:
  - `MV1`: `MapPresent`, `MapBlob`, `MapParses`, `SchemaPresent`, `SchemaBlob` y `SchemaViolations` (`[{"Pointer", "Keyword"}]`). El validador es un
    subconjunto de draft 2020-12 con `type`, `enum`, `const`, `required`, `properties`, `additionalProperties`, `items`, `minItems`, `maxItems`,
    `uniqueItems`, `minLength`, `pattern`, `minimum` y `maximum`. Una palabra clave del esquema fuera del subconjunto, salvo las anotaciones `$schema`,
    `title`, `description`, `$id`, `$comment`, `default` y `examples`, es `UNSUPPORTED_SCHEMA_KEYWORD`;
  - `MV2`: `MapSurfaces` y `ClosedList`. La lista cerrada son los valores entre comillas invertidas del párrafo que empieza por
    `**Superficies** (lista cerrada):` en AP en Eff, hasta la primera línea en blanco;
  - `MV3`:
    - `ChangedUnderSurfaces`: rutas bajo `Surfaces` cuyo blob difiere entre Eff^1 y Eff, sin el mapa; incluye las añadidas y las borradas, ordenadas;
    - `DeletedUnderSurfaces` y `FilesPaths`;
    - `FilesDuplicates`;
    - `OnlyInChanged` y `OnlyInFiles`.

    Bajo `Surfaces` significa ruta igual a una superficie, o que empieza por una superficie que termina en `/`;
  - `MV4`:
    - `Rows` = `[{"Path", "FileKind", "MapBaseBlob", "MapEffBlob", "ObservedEff1Blob", "ObservedEffBlob"}]`;
    - y cuatro listas de rutas, cada una con su ancla y ninguna como veredicto:
      - `LiteralViolations` (L905 literal: MODIFIED con `MapBaseBlob` ≠ blob en Eff^1 o `MapEffBlob` ≠ blob en Eff; ADDED o ENTRY con `MapBaseBlob` ≠
        null o `MapEffBlob` ≠ blob en Eff; un blob observado null nunca coincide);
      - `EntryFilesOutsideSchema` (L905, «ENTRY de archivo ⊆ {esquema del mapa}»);
      - `AddedPresentInEff1` (R61 b, L835; tabla de defectos, L916);
      - `EntryFilePresentInEff1` (ninguna regla lo convierte en defecto; se informa).
  - `MV5`: `EntryKeyDuplicates` y `EntriesOnNonModifiedPath`;
  - `MV6`, por cada archivo MODIFIED p:
    - `{"Path", "H1Duplicates", "H2Duplicates", "DerivedModified", "DerivedRemoved", "DerivedAddedOrEntry", "MapModified", "MapRemoved",
      "MapAddedOrEntry"}`;
    - H1 y H2 son los encabezados normalizados de todos los niveles más `preámbulo`, en Eff^1 y en Eff;
    - derivados: MODIFIED = H1 ∩ H2 con texto normalizado distinto; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1;
    - más `EntryKindEntries` y `ExpectedEntryKind`: el par fijo (AP, 16.13) y (WORKFLOW, punto de entrada);
  - `MV7`:
    - `EntryCountEff`, `EntryNamesR163Eff`;
    - `FirstSentenceS16` y `PointerIsFirstSentenceS16`: primera frase del cuerpo de `## 16.`;
    - `LastSentenceS163` y `PointerIsLastSentenceS163`: última frase del cuerpo de `### 16.3`;
    - `S163EffMinusPointer` y `S163Eff1`: textos normalizados del cuerpo. En Eff se quita **solo** la frase puntero, una vez, con el espacio que la
      precede, nunca la línea entera;
    - `S163EqualAfterPointerRemoval`.
- **`Resolve`** (con `--file CONTRACT`; 16.13 pasos 3-4):
  - `Ar`, `MainSha`, `Mb` = `merge-base(S, Ar)`;
  - `Citations` = `[{"Path", "Section", "Class", "MapFileKind", "MapEntryKinds", "MatchEff1", "MatchEff", "MatchMain", "ExtDiffMbAr"}]`, en el orden
    del contrato. `ExtDiffMbAr` solo para EXTERNAL: `git diff --name-only Mb Ar -- Path`. Los `Match` son recuentos de encabezados según AP L872-L873;
  - `ArBaseDiff` = `git diff --name-only Ar <BaseSha>`, con `--base`.

**Resolución expresa de la divergencia MV-4/MV-7** (§75.5; análisis estático, evidencia §143). La herramienta no elige lectura.
- **MV-4:** informa por separado la regla literal (L905) y la inconsistencia de clase frente a Eff^1 (R61 b, L835; tabla de defectos, L916), cada lista con
  su ancla. Un recuento único rotulado «MV-4» ya no existe: el Controller aplica la tabla de defectos (cualquier lista no vacía de defecto es MAP_INVALID
  por L915-L916) y lo registra.
- **MV-7:** cada clave tiene un nombre que dice qué es: booleano de presencia y posición (`PointerIsFirstSentenceS16`, `PointerIsLastSentenceS163`),
  frase hallada (`FirstSentenceS16`, `LastSentenceS163`) e igualdad tras quitar solo el puntero. Ya no se puede confundir «frase» con «sí/no».

Ninguna sonda se reinterpreta: sus resultados brutos se conservan (§74.3, §75.5).

## 3. Aceptante (`bmin_accept.py`) y R1-R10

Lista cerrada de Git más amplia (`for-each-ref`, `cat-file --batch`), con el mismo entorno; cero escrituras; salida canónica ASCII.

| R | Qué reproduce | Implementación independiente |
|---|---|---|
| R1 | identidad de la herramienta: blob del archivo de la herramienta (SHA-1 calculado por el aceptante) = blob fijado; ejecución desde el objeto | sí |
| R2 | intérprete: `sys.flags.isolated`, versión y SHA-256 del ejecutable; igualdad con lo declarado | — |
| R3 | reejecución de la orden con los mismos argumentos: bytes de `Facts` y `FactsSha256` iguales; `FactsSha256` recalculado sobre `Facts` | — |
| R4 | de los hechos a la decisión: dada una salida del Controller (`controller-verification/v2`), recalcula con sus propias reglas los resultados de `Identity`, `Scope`, `CleanTree`, `Trailer` y `FreeText` y la parte de mapa de `Authority`, y los compara con los del Controller | sí |
| R5 | `Scope`: `diff` con `ls-tree -r` de los dos árboles (no `git diff`) y su propia cobertura | sí |
| R6 | `Authority`: Derive con su propio recorrido first-parent y su propia búsqueda del trailer; MV-3, MV-4 (las cuatro listas), MV-6 y MV-7 con su propio analizador; comparación con los hechos y con `AuthorityResolution` en lo que cubre | sí |
| R7 | diversidad en las reglas con lectura normativa (MV-4, R61 b, cobertura, U-19): las del aceptante son independientes | sí |
| R8 | texto libre de la verificación: control manual del Coordinator | MANUAL |
| R9 | lecturas: cada `GitCalls` y cada `Inputs` de la herramienta caben en el cierre declarado (archivo de entrada con revisiones, rutas y SHA-256 permitidos) | sí |
| R10 | señales de la sesión: SHA-256 del archivo de hechos del relevo = el citado | — |

Salida: `{"Schema": "rackcad-bmin-accept/0-experimental", "Checks": [{"Id": "R1", "Status": "MATCH|MISMATCH|NOT_APPLICABLE|MANUAL", "Detail": ...}],
"Decision": "ACCEPT_FACTS|REJECT_FACTS"}`. Un MISMATCH o un dato ausente da REJECT_FACTS, porque el aceptante falla cerrado. El aceptante nunca
declara `EXECUTION_*`.

## 4. Pruebas exigidas (§75.5)

- **Unitarias**, sobre repositorios sintéticos creados por la prueba en un directorio temporal fuera de `D:\r62-fixture`, con commits, merges, trailers,
  textos no ASCII y un mapa con defectos conocidos.
- **Negativas de fallo cerrado:**
  - SHA inexistente;
  - archivo ausente;
  - SHA-256 distinto;
  - JSON o XML ilegibles;
  - trailer duplicado;
  - EFF ambiguo;
  - esquema con palabra clave no soportada;
  - suborden de Git prohibida;
  - ejecución sin `-I`;
  - estado con dos `claim_id`.

  Todas dan `Facts` = null, un código y exit 2.
- **Cero escrituras:**
  - antes y después de cada orden, igualdad del SHA-256 del índice, de `git for-each-ref`, de `status --porcelain --ignored` y de la lista completa de
    archivos y tamaños del clon;
  - análisis estático (AST) sin APIs de escritura ni de red.
- **Determinismo:** dos ejecuciones dan los mismos bytes de `Facts`, y todos los bytes de stdout son < 0x80.
- **Vocabulario:** ninguna palabra del §1.8 en `Facts`.
- **Mutación:** un conjunto fijo de mutantes del código de la herramienta, cada uno muerto por las pruebas, por el aceptante o por los dos. Por ejemplo:
  - la regla de cobertura invertida;
  - el límite de frase;
  - el orden de los argumentos de ascendencia;
  - una ruta omitida;
  - una lista de MV-4 fusionada;
  - el puntero quitado con la línea entera.
- **Salida alterada:** un `Facts` modificado, con o sin `FactsSha256` recalculado, que el aceptante rechaza.

## 5. Fe de erratas v0.2 (integración de las dos implementaciones independientes)

Surgen de la primera integración de extremo a extremo. Ninguna cambia un hecho normativo; fijan formatos y resuelven dos contradicciones internas.

| Id | Punto | Texto que rige |
|---|---|---|
| E1 | §1.8 vocabulario | La prohibición alcanza a lo que la herramienta **produce** como hecho. Lo que reproduce literalmente de una entrada queda fuera: nombres de atributo de `ResultSummary/Counters` (`trx.Counters`), `Tests[].Outcome`, `freetext.Terms` y `Matches[].Term`, `read-text` `Lines[].Text`, `read-json` `Values`, `handoff` `Fields`, y las frases y textos de `clausemap` (`FirstSentenceS16`, `LastSentenceS163`, `S163EffMinusPointer`, `S163Eff1`, celdas de `Pre`). |
| E2 | §2.3 `U19Normalized` | Lista de objetos `{"Entry", "List", "Prefix"}`. `List` es `AllowedWriteScope` o `ForbiddenWriteScope`; el orden es el de la delegación, primero Allowed y después Forbidden. |
| E3 | §1.2 rutas | La herramienta puede pasar las rutas a Git como pathspec literal `:(literal)<ruta>` para evitar la expansión de comodines. Quien clasifique `GitCalls` (R9) normaliza ese prefijo; cualquier otra magia de pathspec es una violación. |
| E4 | §2.12 frente a §4 | Rige §4: dos o más commits con el trailer, o un único trailer sin merge que lo cumpla, dan `AMBIGUOUS` (fallo cerrado). Ningún trailer es un hecho (`Eff` = null). Dos o más valores **distintos** de `claim_id` dan `AMBIGUOUS`; repeticiones del mismo valor no. |
| E5 | §2.11 términos | Los nombres de campo entre comillas invertidas del párrafo de 16.10 no son términos: los términos son la lista que sigue a los dos puntos (16 en `f525cb1e`). |
| E6 | §2.2 renombres | `diff --name-only --no-renames`: un renombre da las dos rutas, la de origen y la de destino. |
| E7 | §3 R9 | El cierre declara `RevPaths` para cada blob leído. Las rutas sueltas no autorizan blobs. |
