# I-62 — Kit de la revisión formal de la enmienda candidata A-2 (R20261008T014941Z-68fe)

```text
LogicalReviewRequestId: L20261008T014941Z-68fe   InvocationId: I20261008T014941Z-68fe   AttemptSeq: 1   RunId: R20261008T014941Z-68fe
Autorización:   disposición del Coordinator (decisiones §55, punto 8): paquete completo y, sin transporte automático elegible, HUMAN_LAUNCH_REQUIRED;
                UNA invocación del Architect (cabecera del paquete), sin reintento automático
Objeto:         commit de publicación 4a059afa85dce5a82f74120cad88c5ebadb61c7a (CI 37714804772, push, 4/4 success)
                commit de A-2 b280f7079e128113666e85a2f437e629d0f0d99e (CI 37714368467, push, success), sobre c9f9419dc4aed32242c59de489a8e78a8e369f9b
                docs/initiatives/I-62-A-2.md                              blob d47f71b66ba86c31f857f0d8c2d2437636947af0
                docs/initiatives/I-62-architect-package-A-2.md            blob f45a2384eb1a3c9ec5654995c5b88f6c51b6742f
                docs/automation/evidence/I-62-A2/a2-guards.py              blob 91c9c2a46a4a9327ecfff44fa52a4d48e0e33935
                docs/automation/evidence/I-62-A2/a2-guards-result.json     blob 33ffbb9deb114972cfd11ade3ab448a2af1f50dd
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a2 (git clone --no-local -c core.autocrlf=false --single-branch; main = el commit; sin remoto; sin enlaces;
                kit/clone-verification.json)
Run:            D:\r62-arch-a2-run (order.txt = el cuerpo exacto pegado de la disposición §55, 6 234 bytes, SHA-256 bfa265cd…;
                prompt.md = kit/prompt.md, 23 737 bytes, SHA-256 22d93ae5…)
Transporte:     claude-desktop-session: sesión nueva y separada que el Owner abre con un clic desde la tarjeta de tarea que crea la sesión principal
                (cwd D:\r62-arch-a2; la app crea el worktree en D:\Documentos\Worktrees\r62-arch-a2\<nombre>); primer mensaje = kit/prompt.md literal
Estado:         kit custodiado ANTES del lanzamiento (commit de custodia: pendiente); HUMAN_LAUNCH_REQUIRED; NO lanzado
Acreditación:   la decide el Coordinator después de la corrida (auditor v4-a2.1 sobre la transcripción)
```

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon):
  - canónicos: la tabla de insumos del paquete §2 en el commit de publicación (12 archivos), cada uno con `Blob`, `Bytes`, `Lines`, `Sha256`,
    codificación y `LineRanges` calculados mecánicamente desde los encabezados y las filas (título exacto y fila única comprobados);
  - transitivos: 14, con la solución de GAP-07 de A-1 (opción A): las lecturas iniciales de `CLAUDE.md` y `AGENTS.md`, el contrato de I-62 que declara
    el Context Pack y los `required_docs` de `documentation-governance`. `docs/AUTOMATION_PLAN.md` señala además §16.8 (600-618, STOP por el tope de
    invocaciones en 612) y §16.11 (674-708, fila que define P-07 en 704);
  - run: `order.txt` y `prompt.md`, los dos con `RunFileHashes` (el script exige que el run y las copias del kit sean idénticos).
- **Acciones** (`AllowedActions`, idénticas a la tabla de `kit/prompt.md`; cada una declara su `Tool`, y `ToolPolicy` fija el reparto):
  - ID-1 e ID-2: identidad; `rev-parse <commit>:<ruta>` y `cat-file -p` solo con `4a059afa` o `HEAD` (`cat-file -t|-s`, también con `c9f9419d` o
    `b280f707`);
  - MD-1: metadatos y hashing enumerados, con `git diff c9f9419d b280f707` (el delta de A-2) o `git diff b280f707 4a059afa` (la publicación;
    `HEAD` vale por `4a059afa`): exactamente dos revisiones, en ese orden, y siempre `-- <rutas del cierre>`;
  - NAV-1: `cd` o `Set-Location` a la raíz o a subdirectorios del clon o del worktree. Navegar no autoriza leer fuera del cierre;
  - RD-1 y RD-2: lecturas y búsquedas solo sobre archivos del cierre en el commit;
  - EX-1: `a2-guards.py self-test --out <salida propia>` (el modo `run` necesita `origin/main`, que el clon no tiene: no disponible ni permitido) y
    python propio sin procesos, listados ni red; EX-2: `dotnet test`, sin exención nueva;
  - OWN-1 y OWN-2: salidas propias. Los scripts propios, incluido un `sed -i` declarado, solo en el scratchpad de la corrida;
  - herramientas: ID-1, ID-2, MD-1, RD-1, RD-2, EX-1 y la redirección, `mkdir -p` y `sed -i` de OWN-2, solo con Bash; PowerShell solo para EX-2,
    `Set-Location` y `Get-Content`.
- **Auditor v4-a2.1** (`kit/post-review.py`): la lógica v4 del kit de A-1 (rutas efectivas con `realpath`; bucles `for`; escrituras solo sobre
  salidas propias; rutas de objetos Git aparte de las de disco; una lectura fallida nunca se acredita), adaptada a A-2 y con las correcciones del
  verificador:
  - el cierre se lee del kit custodiado, separado del run;
  - EX-1 solo admite `self-test --out <salida propia>`;
  - PowerShell, solo EX-2, `Set-Location` y `Get-Content` (git y python en PowerShell quedan fuera; las comprobaciones de HEAD del Paso 0 leen Bash);
  - `git diff` solo con un par de `DiffPairs` en su orden; `rev-parse <rev>:<ruta>` y `cat-file -p` solo con el commit o `HEAD`; `cat-file -p` de un
    árbol, de otro objeto o con `--batch`, rechazado;
  - premisas: archivos del cierre u `order.txt` (líneas entregadas fielmente por Read); `prompt.md` nunca;
  - custodia del run: el SHA-256 de `order.txt` y `prompt.md` del run debe coincidir con la copia del kit y con `RunFileHashes`;
  - coherencia del resultado de A-2: Q-A2-01..Q-A2-07 con sus hallazgos, `Focus` 1-4, `NoChangeConfirmation`, `IfAgreed` y la vía NOT_ACCREDITED
    por identidad (sin exigir cobertura, con el motivo obligatorio); con un veredicto, sin `NOT_ASSESSED` ni `null` en materialidad, identidad,
    `LegacyApplicability` ni `OwnerDecisionRequired`; premisas comprobadas también en `QuestionDispositions`;
  - uso: `python kit/post-review.py <transcripción.jsonl> D:\r62-arch-a2 D:\r62-arch-a2-run kit kit/result.schema.json <audit.json>`.
- **Esquema** (`kit/result.schema.json`): el veredicto del paquete §1 (AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION, y NOT_ACCREDITED como en
  A-1); REQUIRED `A62-A2-NN` con premisa y autoridad o contraejemplo; OPTIONAL `A62-A2-ON`; disposiciones de Q-A2-01..Q-A2-07; decisión del Owner;
  confirmación sobre B.1-B.11, `state/v2`, §20, A-1, AUTOMATION_PLAN 16.x y F4; modo; identidad; limitaciones. Para la parada NOT_ACCREDITED admite
  `NOT_ASSESSED` en la materialidad y `null` en `ReviewedCommit`, `ReviewedBlob`, `IdentityCheck`, `LegacyApplicability` y `OwnerDecisionRequired`.
- **Selftest previo** (`kit/selftest-result.json`): 23/23 PASS sobre el clon real, que queda limpio y en el commit:
  - los casos de A-1: la base acreditada; los dos bucles `for` y un `for` con una ruta fuera; `cd` a subdirectorios, también `Set-Location`; `cd` no
    autoriza leer fuera; un script propio editado con Write, `sed -i`, Edit y `mkdir -p`; el rechazo de ediciones canónicas (`sed -i`, Edit, Write y
    redirección sobre el arnés custodiado); el rechazo de lecturas fuera del cierre (absoluta, escape por `..` y Grep sobre un directorio) y el escape
    por unión (junction) en el clon y en las salidas propias; lecturas fallidas sin crédito (el `git show` con `..` que Git rechaza, un Read fallido y
    un `git show | sed` vacío), cada una con su premisa rechazada; metadatos enumerados frente a prohibidos;
  - los casos de A-2: EX-1 `self-test` aceptado (ruta del clon, relativa, `--out=` y ruta del worktree); `run`, salida fuera de las salidas propias y
    falta de `--out`, rechazados; la vía NOT_ACCREDITED por identidad, ahora con `NOT_ASSESSED`, `null` y `NOT_DETERMINED`; la coherencia del
    resultado (AGREED incoherente y NOT_ACCREDITED sin motivo);
  - los casos de las correcciones (16-22): git y python en PowerShell rechazados, y `Get-Content`, `Set-Location` y `dotnet test` en PowerShell
    aceptados; premisa sobre `order.txt` aceptada (ruta absoluta y nombre suelto); premisa sobre `prompt.md` o con una cita falsa, rechazada; pares de
    diff cruzado, invertido, de una sola revisión y `a..b`, `cat-file -p` y `rev-parse` de otra revisión, `cat-file -p` de otro objeto o de un árbol y
    `cat-file --batch`, rechazados, y las formas permitidas aceptadas; custodia del run (discordancia con `RunFileHashes` simulada en memoria, sin
    tocar el run); veredicto con campos sin evaluar, rechazado.
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 3/3 lecturas fieles en el clon: A-2 §2.3 (130-156), A-2 §3.3 (198-224) y V14 D.3,
  totales y ancla (2519-2528). Orden registrada en el propio archivo (`Command`, `Filter` = null, `TranscriptSha256`): sin filtro, porque
  `read_fidelity.py` ya exige el separador tras la ruta del clon (antes, `D:\r62-arch-a2-run\order.txt` contaba como archivo del clon y la
  ejecución sin filtro fallaba). El resultado coincide con el registrado antes con el filtro `docs/`.
- **Clon y EX-1** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`): HEAD = el commit; solo `main`; sin remotos; árbol limpio
  (también sin ignorados); los cinco blobs; `b280f707` y `c9f9419d` presentes, `4c617e82` y `b64a3b64` ausentes; 0 enlaces o uniones y 0 entradas
  120000; `core.autocrlf` = false; run con los dos archivos, idénticos a las copias del kit; `a2-guards.py self-test` en el clon: PASS, 43/43
  vectores y 7/7 mutantes, clon limpio después.
- **Manifiesto:** `kit/kit-manifest.json` (SHA-256 de cada archivo del kit, incluidas las copias de `order.txt` y `prompt.md`).

El cierre y las acciones no se amplían después de ejecutar. Este kit no cambia el contrato de ninguna revisión anterior.

## Desviaciones frente al kit v4 de A-1

- **Kit separado del run.** En A-1 los archivos del kit vivían en el directorio del run. Aquí el run contiene solo `order.txt` y `prompt.md`, y el
  auditor recibe el directorio del kit como argumento (y comprueba el `RunDir` del cierre y la custodia de los dos archivos del run).
- **EX-1.** El arnés es `a2-guards.py`, con argumentos `self-test --out <salida propia>`; en A-1, `a1-counterexamples.py <salida propia>`.
- **MD-1.** Dos pares de diff (el delta de A-2 y la publicación) en lugar de uno, cada uno en su orden; la regla `-- <rutas del cierre>` no cambia.
- **Transitivos.** Se añaden `docs/AUTOMATION_PLAN.md` (`required_docs` del Context Pack) y el contrato de I-62 (declara el Context Pack), que en A-1
  eran canónicos: así la opción A de GAP-07 sigue completa con el cierre más estrecho del paquete §2.
- **Cierre.** Cada registro lleva `LineRanges` y `Scope`, y el cierre declara `QuestionIds`, `FocusItems`, `DiffPairs`, `Harness`, `RunDir`,
  `RunFileHashes` y `ToolPolicy`, derivados del texto.
- **Resultado.** `QuestionDispositions`, `NoChangeConfirmation` y `GuardsVerification` sustituyen a `FindingDispositions`, `OptionalCorrections`,
  `F3ContractCrossCheck` y `CounterexampleVerification`; `IdentityCheck` añade las guardas, su resultado y V14; un REQUIRED exige la autoridad violada
  o un contraejemplo (paquete §1); la plantilla del prompt no trae valores de materialidad ni de modo prellenados.
- **Coherencia.** Con NOT_ACCREDITED el auditor no exige cobertura ni el objeto designado (el v4 de A-1 los exigía siempre, y una parada por identidad
  sumaba motivos de ruido), y el esquema admite `NOT_ASSESSED` y `null` para lo no evaluado.
- **Paso 0.** El prompt pide leer entero `prompt.md` con Read (el auditor v4 ya lo exigía, `PromptReadFaithful`, pero el prompt de A-1 no lo pedía) y
  ejecutar sus comandos con Bash.
- **Herramientas.** El prompt de A-1 decía «Bash o PowerShell: solo para las acciones de la tabla», aunque su auditor solo admitía PowerShell para
  `dotnet test`, `Get-Content` y `Set-Location`. Aquí el prompt y `AllowedActions` dicen lo mismo que el auditor.
- **Premisas.** Pueden citar `order.txt`, que el prompt presenta como la orden vigente; en A-1 solo archivos del cierre.
- **Discordancias del paquete, registradas sin resolver.**
  - El paquete §1 y §4 nombran Q-A2-01..Q-A2-05, y A-2 §8 contiene siete: el resultado dispone las siete.
  - El paquete §2 (línea 44) pide «la fila de P-07 en §8». El cierre contiene dos filas que pueden corresponder, y el kit no elige entre ellas:
    V14 657 (tabla de §9.3), que nombra P-07, y AUTOMATION_PLAN §16.11 línea 704, que la define (con §16.8, línea 612).
- **Selftest.** Todos los temporales (transcripciones, esquemas temporales del auditor, el directorio de enlaces de las salidas propias) viven en
  `../.st`, que se borra con una guarda contra enlaces. La unión de prueba del clon apunta a `D:\r62-arch-a1t01`. Hay doce casos nuevos, y el
  resultado escribe `%USERPROFILE%` en lugar del directorio del usuario.

## Al terminar la corrida (procedimiento)

- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json`, y `D:\r62-arch-a2-run\order.txt` y `D:\r62-arch-a2-run\prompt.md` frente a
  las entradas `order.txt` y `prompt.md` del manifiesto (el auditor repite la comprobación contra la copia del kit y `RunFileHashes`, en
  `RunCustody`).
- `output.json`: el bloque JSON literal del mensaje final, extraído de la transcripción.
- `audit.json`: el auditor v4-a2.1 literal.
- `runtime-evidence.json`: la identidad observada de la sesión, la transcripción (no versionada; ruta y SHA-256), el clon y el worktree después de
  la corrida, la integridad del kit frente a `kit/kit-manifest.json` y la de los dos archivos del run.

## Mensaje de la tarea (fijado antes de crear la tarjeta)

La tarjeta de tarea de la app lleva este mensaje fijo, igual que en A-1 r5 (task_44fd8237). Remite a `prompt.md` del directorio del run, cuyo SHA-256 la
sesión revisora comprueba en su Paso 0 y que el auditor exige leído entero de forma fiel. Tiene 659 bytes en UTF-8 y SHA-256 `4daa606c7fef632132257e867f6413a71b17aff5e475fb03071c8ebb6d33182e`. La app
puede anteponerle un aviso de worktree: la supervisión comprueba que el primer mensaje contiene este texto exacto.

```text
Eres el ARCHITECT de una revisión formal en solo lectura (I-62, RunId R20261008T014941Z-68fe). Tu única instrucción es el archivo D:\r62-arch-a2-run\prompt.md.

Primero, léelo ENTERO con la herramienta Read (ruta absoluta D:\r62-arch-a2-run\prompt.md) y síguelo exactamente: identidad (Paso 0) antes de cualquier lectura sustantiva, el cierre de insumos, el contrato de acciones cerrado, la forma de lectura y el formato del resultado. No leas ni busques nada fuera de lo que ese prompt permite, no edites el repositorio, no hagas commit ni push y no invoques a otros agentes. Tu último mensaje debe contener solo el bloque ```json que el prompt define.
```

## Resultado custodiado (después de la corrida)

- Lanzada por el Owner desde la tarea `task_241699b1`; sesión `local_b7dce5ad…`, de 03:07:27Z a 03:22:01Z, con `claude-opus-5-5` y `xhigh`. El primer
  mensaje contiene el texto fijo de la tarea (`4daa606c…`) y es el único mensaje humano.
- `output.json`: el bloque JSON literal del mensaje final (SHA-256 `aa0246a94a25de81bff75ae16cb4db2f16baa04f69b8351339f7e416bd5d865e`). **Veredicto: CHANGES REQUIRED.**
  - REQUIRED: A62-A2-01. La regla 2 de A2-P1 solo conserva el FAIL de aislamiento de D.6; tiene que conservar todo FAIL por violación observada
    (D.4, FX-04b, D.8).
  - OPTIONAL: A62-A2-O1 (modelo G5: INVALID_TEST_ORACLE), A62-A2-O2 (bloque a mitad de una actualización) y A62-A2-O3 (presupuesto restante de la
    reejecución).
  - Materialidad M-03 y M-04 sí; ninguna decisión del Owner; sin cambio en las superficies protegidas.
- `audit.json`: el auditor v4-a2.1 literal, sin cambios desde su custodia. **NOT_ACCREDITED**, con 3 motivos en 36 llamadas:
  - llamada 32: `python - <salida propia>` con heredoc;
  - llamadas 33 y 34: `python -X utf8 -` con heredoc, scripts en línea que leen A-2, V14 y las guardas del clon.

  Pasan las comprobaciones de identidad y de orden, la custodia del run, el clon limpio y todas las premisas (encontradas y entregadas fielmente). La
  acreditación la decide el Coordinator.
- `runtime-evidence.json`: identidad observada, transcripción (no versionada; ruta y SHA-256), clon y worktree después de la corrida, integridad
  del kit.
