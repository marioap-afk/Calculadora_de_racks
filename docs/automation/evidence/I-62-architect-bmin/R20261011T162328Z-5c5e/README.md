# I-62 — Kit del dictamen independiente de materialidad de B-min por `claude-cli` (R20261011T162328Z-5c5e)

```text
LogicalReviewRequestId: L20261011T162328Z-5c5e   InvocationId: I20261011T162328Z-5c5e   AttemptSeq: 1   RunId: R20261011T162328Z-5c5e
Autorización:   disposición del Coordinator §75, punto 4 (cuerpo pegado, 3 288 bytes, SHA-256 21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630,
                el que declara decisiones §75 en 460665ab): revisión independiente y acotada por un Architect elegible, con claude-cli, dentro de la
                autoridad de revisión vigente; no es la revisión formal de una enmienda («No solicitar revisión formal de una enmienda que todavía no
                existe»). Owner CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura). UNA revisión, sin reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A, la misma base que las revisiones de A-2, A-3, A-4 y A-5
Objeto:         docs/automation/evidence/I-62-F6/FX-02/bmin/ entero en PKG_COMMIT (pendiente: el paquete todavía no está custodiado); la lista de
                archivos y sus blobs las calcula make_closure.py desde el commit, sin suponer nombres
Base:           460665abd46e4f9f75c896eceecabe11689a6a40 (decisiones §75 registradas; evidencia §145); bmin/ no existe en ella. Todo archivo normativo del
                cierre queda fijado por su blob en la base; decisiones y evidencia, por prefijo exacto (solo añadido)
Paquete:        ninguno (no es revisión formal de una enmienda; el prompt hace de paquete)
Clon:           D:\r62-arch-bmin (pendiente; prepare_clone.py: git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; HEAD separado
                en CLONE_COMMIT = PKG_COMMIT por defecto; sin ramas locales; sin remoto; sin enlaces)
Run:            D:\r62-arch-bmin-run (pendiente), con exactamente dos archivos antes del lanzamiento: order.txt (= day3\s75_body.txt, 3 288 bytes,
                21435cd1…) y prompt.md (= kit/prompt.md, renderizado desde kit/prompt.template.md con el commit del paquete)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos SIN ejecutar el binario el 2026-10-11T17:08:54Z: prep/binary-measurement-2026-10-11.json);
                claude-opus-5-5 con effort xhigh; herramientas Read, Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id
                cd0a1e5c-605d-4363-9e49-57087fd26a47 (uuid4, nuevo); cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s;
                espera de 120 s entre `auth status` y el lanzamiento; ArgsTemplate idéntica a la de los kits de A-2..A-5
Compuerta D-01: MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char (claude-cli-characterization.json, SHA-256
                20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, blob 541956eb): transport_gate.evaluate sin problemas con el cierre de este
                kit (medido en el entorno provisional); el paso final la fija sobre la copia del clon D:\r62-arch-bmin
Estado:         preparado, NO lanzado. Falta solo el paso que depende de PKG_COMMIT (abajo, «Terminar el kit»)
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-bmin sobre la transcripción, la salida stream-json y launch/)
```

**Pregunta (order.txt, línea 30, literal).** «¿Puede B-min utilizarse como herramienta ACTION_COMPATIBLE con entradas canónicas fijadas por blob bajo
el contrato vigente, o modifica materialmente AP 16.4/16.19/16.24, ADR-0046, D.3 o los contratos consumidos?» Entregables de §75.4: cláusulas
aplicables; materialidad M-01..M-08; superficies afectadas; consecuencia sobre independencia; necesidad o no de A-6; decisiones OWNER-RESERVED. La
sesión principal añade: la clasificación ActionCompatibility (AP 16.24, paso 2), si la herramienta puede ser CanonicalInput (AP L1264) y la
observación sobre una sola invocación de solo lectura del Controller (PLAN + VERIFY). El Architect no redacta enmiendas.

**Independencia de los implementadores.** Quien preparó este kit no leyó `day3\s75\bmin\tool\` ni `\accept\` (solo el borrador de `SPEC.md`). El
entorno provisional usa un paquete de relleno (SPEC = el borrador; README y cuatro `.py` con una línea de aviso). El kit no ejecuta el código del
paquete.

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon; pendiente): todos los archivos de `bmin/` en el commit del paquete
  (objeto, enteros) y 19 archivos normativos, 1 424 647 bytes, con rangos calculados y las líneas citadas comprobadas por su primera línea:
  - AUTOMATION_PLAN 16.4 (507-543; transporte 509; receta de Codex 541), 16.9 (620-665; tabla 628-643), 16.10 (667-672), 16.13 (717-928; 731;
    902-908), 16.14 (930-948; 945-946) y 16.18-16.24 (1015-1323; 16.19 contrato 1045-1046 y descriptor 1048-1052; 16.24 `CanonicalInputs[]` 1264,
    demás clases 1265-1267, paso 2 de la clasificación 1271-1274, identidad del revisor 1282, fidelidad 1287, P-22..P-25 1318-1321);
  - ADR-0046 y ADR-0048 enteros (puntos 1-9 de cada Decisión);
  - LIFECYCLE §2 (34-64), §3 (66-94; M-01..M-08 83-90; «UNKNOWN cuenta como activado» 78-79; OWNER-RESERVED 94) y §6 (193-222);
  - V14 §20.2 (AUT-I1 1039-1047), §20.3 (1049-1071) y §20.3.1 (1073-1130; `CanonicalInputs[]` 1079), Anexo C (2330-2382; 2333-2334; C-24 2363) y
    D.1-D.6 (2386-2587; tabla de la Topología A de D.3 2433-2447; D.6 2572-2573);
  - A-4 y A-5 enteras (A5-1 175-199; reglas 1-2 178-186);
  - agent-execution README §5, §8, §10, §14 y §15; `routing.md` y `adapters/codex-cli.md` enteros;
  - los esquemas role-invocation.v1, input-closure.v1, controller-verification.v2, delegation.v2, worker-handoff (v1) y gate-contract.v2;
  - `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` (los contratos de las invocaciones del Controller);
  - decisiones §73-§75 (1479-1529; §75 1515-1529, cuerpo pegado 1518, punto 4 1525) y evidencia §140-§145 (3780-3945);
  - run: `order.txt` (clase RUN; único archivo del run que puede ser referencia) y `prompt.md`, con `RunFileHashes`;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate`, `TimeoutSeconds`, `AuthSettleSeconds` y `Gate`.
  Excluidos: transcripciones y memoria, otros worktrees y clones, el fixture, `probe_tools.py` y los `reference.json`/`comparison.json` de las sondas,
  los análisis `controller-strategy.md` y `mv-static-analysis.md`, el resto de `kits/FX-02/`, WORKFLOW, el mapa de cláusulas, `state/I-62.yml`.
- **Herramientas y lectura** (como en A-5). Solo Read, Grep y Glob, y StructuredOutput. Es una violación: cualquier Read o Grep fuera del cierre y
  del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas contengan un archivo fuera del
  cierre (el único Glob previsto es el del Paso 0 sobre `bmin/`, que está entero en el cierre); otra herramienta; una denegación de permiso. Archivos
  de más de 60 000 bytes por tramos de 300 líneas como máximo.
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md` = `D:\r62-arch-bmin-run\prompt.md`, sin salto final; vista previa renderizada con el
  paquete de relleno: `provisional/prompt-provisional.md`, ≈ 25,5 KB, 235 líneas). Neutral: sin veredicto esperado ni contexto privado. Pide:
  - el Paso 0: `order.txt` entera; decisiones §75 (bytes y SHA-256 del cuerpo pegado; el punto 4 = la pregunta de la línea 30); un Glob de `bmin/`
    cuyo resultado debe ser exactamente la tabla del objeto; la cabecera de `SPEC.md` (nombra §75); si algo no coincide, NOT_ACCREDITED;
  - la pregunta de §75.4 y sus seis entregables, más ActionCompatibility, CanonicalInputAdmissible y SingleInvocationMeasurement;
  - referencias exactas `ruta:línea` / `ruta:inicio-fin`, solo de líneas leídas con Read; `Excerpt` literal de 200 caracteres como máximo;
  - el modo de revisión, la relación con el autor y el contexto inyectado (los identificadores de cuenta, solo por su tipo).
- **Esquema** (`kit/result.schema.json`, `rackcad-bmin-materiality-ruling/0`; 7 048 caracteres ASCII en forma compacta; generado por
  `prep/gen_schema.py`): solo las palabras clave que ya midió el esquema de A-5 con claude-cli 2.1.293 (type, additionalProperties, required,
  properties, enum, pattern, minLength, minItems, items); los límites de 200 caracteres van por `pattern` `^[\s\S]{1,200}$`; la unicidad la comprueba
  el auditor. Campos: `Schema`, `RunId`, `InvocationId`, `LogicalReviewRequestId`, `AttemptSeq`, `RequestedRole` (ARCHITECT), `Action` (REVIEW_DESIGN),
  `ReviewedUnit`, `Object` {`Commit`, `Files`[{`Path`, `Blob`}]}, `ReviewerMode`, `SamePersonAsAuthor`, `ReviewerDeclaredIdentity`, `ContextDeclaration`
  {`State`, `Items`}, `Step0` (cinco booleanas + `Basis` INVOKER_VERIFIED_CLONE + `Note`), `Reads`, `Verdict` (NOT_MATERIAL | MATERIAL |
  BLOCKED_OWNER_DECISION | NOT_ACCREDITED), `ApplicableClauses`[{`Ref`, `Excerpt`, `Relevance`}], `ActionCompatibility` {`Classification` READ |
  ACTION_COMPATIBLE | ACTION_INCOMPATIBLE | CONDITIONAL_NOT_TRIGGERED | NOT_DETERMINABLE, `Rationale`, `ClauseRefs`}, `CanonicalInputAdmissible`
  {`Admissible` yes | no | needs-change | NOT_ASSESSED, …}, `Materiality`[{`Id` M-01..M-08, `Activated` yes | no | UNKNOWN, …}] (mínimo 8),
  `AffectedSurfaces`[{`Surface` AP_16_4 | AP_16_19 | AP_16_24 | ADR_0046 | V14_D3 | CONSUMED_CONTRACTS | OTHER, `Name`, `Effect` NOT_AFFECTED |
  WITHIN_CURRENT_TEXT | REQUIRES_NON_MATERIAL_CHANGE | REQUIRES_MATERIAL_CHANGE | UNKNOWN, …}], `Independence` {`Dimensions`[{`Dimension` ACTOR |
  SESSION | CONTEXT | PROVIDER | C24_ORACLE | TOOL_ACCEPTOR_DIVERSITY, `Effect` NO_EFFECT | PRESERVED_WITH_CONDITIONS | STRENGTHENED | WEAKENED |
  UNKNOWN, …}], `Consequence`}, `A6` {`Needed` yes | no | NOT_ASSESSED, `MinimalDeltaScope`[≤ 200 caracteres], …}, `OwnerReserved`[{`Decision`,
  `Why`, `ClauseRefs`}], `SingleInvocationMeasurement` {`Possible` yes | no | UNKNOWN, `Conditions`, …}, `Observations`, `KnownLimitations`.
- **Compuerta de transporte D-01** (`kit/transport_gate.py` y `kit/c3_analysis.py`, byte a byte los de los kits de A-4 y A-5; predicados sin cambio).
- **Auditor v5.1-bmin** (`kit/post-review.py`, derivado de v5.1-a5 por sustituciones exactas de bloques con `prep/adapt_post_review.py`: las reglas
  de lanzamiento, compuerta, stream-json, transcripción, herramientas, lectura, fidelidad, custodia del run y clon quedan byte a byte). Cada fallo es
  un motivo y ACCREDITED exige cero motivos. Nuevas:
  - referencias: cada `Ref` y cada `ClauseRefs` resuelve a líneas de un archivo del cierre (ruta relativa al clon) o de `order.txt`, todas entregadas
    fielmente por Read; un `Excerpt` se halla, con espacios colapsados, dentro de sus líneas; `prompt.md` nunca;
  - siempre: M-01..M-08 una vez cada uno; `A6.Needed` = yes ⇔ `MinimalDeltaScope` no vacío; ids de la invocación = los del cierre;
  - con un veredicto: las seis superficies nombradas y las seis dimensiones una vez cada una; las cinco booleanas de `Step0` en true; `Object` = el
    commit del paquete y exactamente sus archivos con sus blobs; `ApplicableClauses` no vacía; `ClauseRefs` en ActionCompatibility,
    CanonicalInputAdmissible, A6, cada fila de Materiality y cada OwnerReserved; sin NOT_ASSESSED;
  - NOT_MATERIAL ⇒ las ocho filas en `no`, `A6.Needed` = no y ninguna superficie en REQUIRES_MATERIAL_CHANGE ni UNKNOWN; MATERIAL ⇒ al menos una fila
    en `yes` o `UNKNOWN` y `A6.Needed` = yes; BLOCKED_OWNER_DECISION ⇒ OwnerReserved no vacía; NOT_ACCREDITED ⇒ KnownLimitations no vacía.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-bmin-run\launch\stdout.jsonl D:\r62-arch-bmin D:\r62-arch-bmin-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`, derivado del de A-5 con `prep/adapt_launch.py`: solo constantes, kit-params.json y dos archivos del run). Las diez
  comprobaciones de A-5 sin cambio de predicados (integridad del kit, transporte del cierre, binario, `--version`, `auth status`, clon, run,
  sesión nueva, línea de órdenes, compuerta); además exige `kit-params.json` con Provisional = false y `closure.PkgCommit` = el del kit-params.
  No reintenta. `--dry-run` no ejecuta el binario.
- **Selftest previo, PROVISIONAL** (`provisional/selftest-result-provisional.json`, SHA-256 `b00ec39a…`): **127/127 PASS** (86 casos de
  auditoría y 41 de la compuerta), con el clon y el run provisionales limpios e idénticos al terminar; 560 s. Casos 00-55: los del kit de A-5
  adaptados (dos archivos del run, `order.txt` alterado en el 12, referencias en lugar de premisas en el 14, 15, 25, 26 y 48, el Glob del Paso 0 en
  la base de todos); 56-83, nuevos: cobertura de M-01..M-08 (repetido y siete filas), MATERIAL sin disparador, BLOCKED sin OwnerReserved,
  superficies ausentes o repetidas, dimensión ausente, NOT_MATERIAL con superficie material o con un UNKNOWN, objeto con otro commit o sin un
  archivo, ids, `Step0` nulo o falso, NOT_ASSESSED con veredicto, A6 sin alcance, extracto no hallado, referencias fuera del cierre o de rango,
  referencias exigidas ausentes (cuatro casos), `Excerpt` de más de 200 caracteres, referencia absoluta y mal formada; y tres positivos (MATERIAL
  coherente, BLOCKED coherente y MATERIAL por UNKNOWN).
- **Mutación, PROVISIONAL** (`provisional/mutation-result-provisional.json`, SHA-256 `affb69fd…`): cada sitio de regla de `post-review.py` (96) y de
  `transport_gate.py` (36) desactivado por turno y los 127 casos contra cada mutante: **132/132 mutantes muertos**, base 127/127; 2 904 s. Sobre los
  archivos finales (`post-review.py` `3910e024…`, `transport_gate.py` `b05fedd1…`).
- **Entorno provisional** (`prep/provisional_env.py`, ya borrado): clon disperso del worktree en la base 460665ab (los archivos del cierre,
  `docs/adr/` y el registro de caracterización) más un commit sintético con el paquete de relleno, bajo la ruta corta 8.3 del scratchpad por
  MAX_PATH; kit copiado con `Provisional` = true; `make_closure.py --provisional` y `transport_gate.py set MEASURED` sobre el registro real
  (`Problems` = []). Vista previa del cierre y del prompt en `provisional/`.
- **Selftest y mutación definitivos, clon y run, manifiesto, preflight en seco**: pendientes del paso final (los rehace `finish_kit.py` sobre
  D:\r62-arch-bmin y quedan en `kit/selftest-result.json`, `kit/mutation-result.json`, `kit/clone-verification.json`, `kit/kit-manifest.json` y
  `launch-dry-run.json`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Terminar el kit (cuando exista PKG_COMMIT)

Un solo comando, desde cualquier directorio (≈ 60 min: selftest ≈ 10 min y mutación ≈ 50 min, medidos en provisional; sin red; no ejecuta el
binario):

```text
python -B "%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\kit-bmin-mat\kit\finish_kit.py" <PKG_COMMIT>
```

- `<PKG_COMMIT>`: SHA completo del commit que custodia `bmin/` en la rama de I-62. Opcional, un segundo SHA si el clon debe quedar en un commit
  posterior que conserve el mismo árbol de `bmin/` (por defecto, el mismo). `--no-mutation` aplaza la mutación (el manifiesto lo declara).
- Pasos: `prepare_clone.py` (clon y run; `kit-params.json`) → `make_closure.py --render-prompt` → copia de `prompt.md` al run → `make_closure.py` →
  `transport_gate.py init` y `set MEASURED D:\r62-arch-bmin\docs\automation\evidence\I-62-claude-cli\R20261008T183600Z-char\claude-cli-characterization.json`
  → `verify_clone.py` → `selftest-post-review.py kit/selftest-result.json` → `mutation-post-review.py kit/mutation-result.json` → `make_manifest.py`
  → `launch.py --dry-run` (`launch-dry-run.json`). Se detiene en el primer fallo.
- Se detiene también, a propósito, si el commit del paquete cambió un archivo normativo del cierre (blob distinto del de la base), si decisiones o
  evidencia no crecen solo por añadido, si un archivo de `bmin/` no es UTF-8 o tiene caracteres fuera de `[A-Za-z0-9_./-]` en la ruta, si falta
  `SPEC.md` o su cabecera no nombra §75, o si el clon o el run ya existen.

## Desviaciones frente al kit de A-5 (R20261011T065226Z-dbb0) y a la orden, con su motivo

- **Objeto = directorio, en un commit futuro.** El cierre, el prompt, el clon y el manifiesto se generan después, parametrizados por
  `kit-params.json`; `prompt.md` sale de una plantilla con marcadores que `make_closure.py` sustituye y comprueba.
- **Dos archivos del run** (sin `delta.diff`): no hay versión anterior ni diff; la identidad del paquete la verifica el invocador (blobs) y el revisor
  contrasta el conjunto de archivos con un Glob.
- **Sin preflight de guardas** en `verify_clone.py`: el objeto no trae guardas custodiadas como A-5; el kit no ejecuta las pruebas del paquete.
- **Verdict con NOT_ACCREDITED** además de los tres valores de la orden, y NOT_ASSESSED en `CanonicalInputAdmissible` y `A6` solo con NOT_ACCREDITED:
  es la vía de parada del Paso 0 (como en A-5), para no fabricar un veredicto.
- **Reglas de coherencia añadidas** a las de la sesión principal, todas declaradas en el prompt: coherencia de M-01..M-08 con el veredicto (LIFECYCLE
  §3, L78-L79: UNKNOWN cuenta como activado), `A6.Needed` ⇔ `MinimalDeltaScope`, superficies con NOT_MATERIAL, cobertura de superficies y dimensiones,
  `Step0` en true con un veredicto, referencias exigidas.
- **Insumos añadidos a la lista**: `gate-contract.v2.schema.json` (la SPEC de B-min, §2.4, lee sus nombres de campo: es un contrato consumido).
  El «documento de contratos del Controller» no está en agent-execution: es `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`
  (declara no contener valores esperados). README de agent-execution: además de §8, §10 y §14, §5 (invocación del Controller) y §15 (cierre y
  fidelidad), como orientación.
- **Rangos sin la línea en blanco final**: 16.4 507-543 (no 544), 16.9 620-665, 16.13-16.14 717-948, 16.18-16.24 1015-1323; mismo contenido.
- **Clon por defecto en PKG_COMMIT** (el patrón de A-5: el clon en el commit que contiene el objeto y la orden; la custodia del kit va después).
- **Sin `preflight-read-fidelity.json`**: en A-5 salió de la transcripción del subagente que preparó el kit; aquí no se leen transcripciones. La
  fidelidad la mide después el auditor sobre cada Read del revisor (punto abierto 2).

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (el cierre pesa ≈ 1,45 MB más el paquete; las revisiones de A-4 y A-5 duraron unos 11-21 minutos).
- La compactación automática del contexto (no observada en las corridas anteriores); el prompt avisa.
- Que el esquema, con `pattern` para los límites de longitud y `minItems` 8, se comporte con claude-cli como el de A-5 (mismas palabras clave).
- Que la espera de 120 s evite el conflicto de renovación del token.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Autoridad: decisiones §75, punto 4; consumo cubierto por CLAUDE-CLI-I62 = A. La decisión de lanzar es de la sesión principal; el kit no la presume.
   §75, punto 7: I-64 tiene prioridad de recursos.
2. Terminar el kit (arriba) y revisar `launch-dry-run.json` (todo en Ok salvo la 4 y la 5, que en seco no se ejecutan).
3. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`. Si cambia un archivo del kit, `python -B kit/make_manifest.py`
   y volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los dos archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-bmin\cd0a1e5c-605d-4363-9e49-57087fd26a47.jsonl" D:\r62-arch-bmin-run\launch\stdout.jsonl D:\r62-arch-bmin D:\r62-arch-bmin-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-bmin.
- El dictamen no es un AGREED de ninguna enmienda; con MATERIAL, §75.7 pide preparar después solo el delta mínimo de A-6, condicionado a una
  disposición nueva.

## Puntos abiertos

1. **PKG_COMMIT.** Todo lo que depende del commit del paquete está pendiente: `kit-params.json`, `prompt.md` renderizado, `closure.json`, el clon y
   el run, `transport-gate.json` fijado sobre la copia del clon, `clone-verification.json`, el selftest y la mutación definitivos, el manifiesto y la
   preflight en seco. Lo hace `finish_kit.py` (arriba). Nada se ha creado en `D:\`.
2. **Fidelidad previa al lanzamiento** (AP 16.24, «Antes de lanzar»; P-24). En A-5 la acreditó `preflight-read-fidelity.json` (tres lecturas del
   subagente que preparó el kit, comparadas con `read_fidelity.py` sobre su transcripción). Aquí no se leen transcripciones, así que no existe. La
   sesión principal decide si la hace (el kit trae `read_fidelity.py`: tres Read en D:\r62-arch-bmin y el script sobre su transcripción, solo para
   esas lecturas) o si basta la comprobación posterior del auditor, que compara cada Read del revisor con el blob.
3. **Evidencia §144 en el cierre.** La lista de la sesión principal incluye §140-§145 enteras; §144 contiene la valoración previa de la sesión que
   preparó B-min («es dudosa (M-03/M-04 probable) y pide un dictamen») y su recomendación. El prompt no añade veredicto esperado, pero el revisor
   puede leer esa valoración. El cierre es por archivo: aislar al revisor de ella exigiría sacar del cierre el archivo de evidencia entero (cambio
   del kit antes de custodiarlo y volver a autoprobar).
4. **Insumos que la orden no nombra.** `gate-contract.v2.schema.json` añadido (contrato que la SPEC de B-min lee). El documento de contratos del
   Controller está en `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md`, no en agent-execution.
5. **El kit no ejecuta el paquete.** No reproduce sus pruebas ni R1-R10 (no hay guardas custodiadas que reproducir como en A-5); el revisor solo lee
   el README y la SPEC del paquete. Los resultados R1-R10 son del aceptante (§75.5), fuera de este kit.
6. **Fijación por blob.** Si el commit del paquete cambia cualquier archivo normativo del cierre, o decisiones/evidencia no solo por añadido,
   `make_closure.py` se detiene: decidir antes de lanzar (y, si procede, actualizar `PINNED`/`APPEND_ONLY_SINCE_BASE` y volver a autoprobar).
7. **Autopruebas provisionales.** El selftest y la mutación de este informe corrieron sobre un clon disperso de la base con un commit sintético
   (`prep/provisional_env.py`), no sobre D:\r62-arch-bmin; los definitivos los rehace el paso final.
8. **Etiquetas.** Si en el momento del clon existe una etiqueta sobre un commit posterior al del clon, `verify_clone.py` falla
   (`TagsNotMergedIntoHead`): quitarla del clon o elegir el commit del clon en consecuencia.

## Archivos

Archivos del kit fijados en la preparación (el manifiesto definitivo, `kit/kit-manifest.json`, lo escribe el paso final y es el que manda):

```text
SHA-256                                                           bytes  archivo
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7   6070  kit/c3_analysis.py           (= kit de A-5)
3b6c2735f0ce24eba02c33b7601511d6307346749fa6e839a74b2c616aa8fe60   4495  kit/finish_kit.py
3c43a2b4cfab55bc42d331369e0bbd64178acb26f95e75b5bba4a11594849f36  18030  kit/launch.py
ff6716439bb8b9c9589a09e3bd7308bb028f2705afbb29d9abfc98d6d3882466  42177  kit/make_closure.py
cc2b6ec107222d713f9a291fb178e3b5f4bc2cfda02f5f57f1430c3cdae9d056   5583  kit/make_manifest.py
01cc19953ebcacb5aeeca83329b32bf1a84f0e4a87468df968531b0722c383ac   5968  kit/mutation-post-review.py
21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630   3288  kit/order.txt                (= day3\s75_body.txt)
3910e024cc0ca6f34f4233fe6b4240f71eea0e66c0cf1222dcd5120b3d47b7c0  49468  kit/post-review.py
ff46a4549592c8115a1f4316721be6520326a662d42e53d3af31e034a7167647   5831  kit/prepare_clone.py
285bed872e829948a12091fbc8ea9bdb4048832fd29e3ee1e02f4f7fa3ffda04  18633  kit/prompt.template.md
108d799691a8b339271f454dee59807189c4d1de46baf6654a13ac18b85da0a2   4927  kit/read_fidelity.py
b8d29e4a7b7100c1e90b882b88dc6fd3b49ce319a23180ab288a61e1dab1e9b0  10573  kit/result.schema.json
26655729d96ae11111d447955769d79fa84afe1fc39794a6b0af5fa9a8b76036  64952  kit/selftest-post-review.py
b05fedd17b9b6856d618d2ffcde88efc1ceafaa3e6c2d5373667919a718129c6  18828  kit/transport_gate.py        (= kits de A-4 y A-5)
f7d8296c8ee5b83cb4244ea9632e3b7aced000f621272146886bb3af20c7605b  10215  kit/verify_clone.py
```

Pendientes del paso final, en `kit/`: `kit-params.json`, `prompt.md`, `closure.json`, `transport-gate.json`, `clone-verification.json`,
`selftest-result.json`, `mutation-result.json`, `kit-manifest.json`; junto a este README, `launch-dry-run.json`.

Preparación (fuera de `kit/`, no se custodian como kit): `prep/gen_schema.py` (genera el esquema), `prep/adapt_post_review.py`,
`prep/adapt_launch.py` y `prep/adapt_small.py` (derivan el auditor, el lanzador y `read_fidelity.py` del kit de A-5 por sustituciones exactas),
`prep/provisional_env.py` y `prep/remove_provisional.py` (entorno provisional de autoprueba), `prep/binary-measurement-2026-10-11.json`;
`provisional/` (cierre, prompt, compuerta, parámetros y resultados del selftest y de la mutación provisionales).

## Custodia antes del lanzamiento (sesión principal)

- **Kit terminado** sobre `PKG_COMMIT` = `9356edf141c795ea718a360e266334ded10d6a76` con `finish_kit.py` (`finish-kit.log`):
  - clon `D:\r62-arch-bmin` en `9356edf1`, sin ramas ni remoto; run con `order.txt` y `prompt.md`;
  - cierre de 48 insumos canónicos;
  - compuerta MEASURED;
  - auditor: autoprueba con todos los casos PASS y todas las mutaciones muertas (`selftest-result.json`, `mutation-result.json`);
  - `launch --dry-run` sin bloqueos (sale con 2 por diseño: no ejecuta el binario).

  `kit-manifest.json`: `b7ce5fda637b9586…`.
- **Exclusión decidida por la supervisión:** la evidencia §140-§145 queda fuera del cierre. §144 recoge la valoración previa de la sesión que prepara
  («dudosa (M-03/M-04 probable)») y su recomendación, y el revisor independiente decide sin ese anclaje. Las decisiones §73-§75, la SPEC y el texto
  normativo sí entran. El cambio está en `make_closure.py`.
- **Fidelidad de lectura previa:** no hay un archivo previo como el de A-5, que salía de la transcripción de su preparador. La fidelidad de cada Read del
  revisor la comprueba el auditor después de la corrida.
- **Lanzamiento:** una vez, `python -B kit/launch.py`, por la sesión principal, después de este commit. Sin reintento.

## Resultado (añadido tras la corrida, sesión principal)

```text
Sesión:          cd0a1e5c-605d-4363-9e49-57087fd26a47, de 2026-10-11T20:17:12Z a 2026-10-11T20:31:53Z, salida 0, sin fallo de transporte
Uso:             37 turnos; herramientas {"Read": 30, "Glob": 1, "Grep": 4, "StructuredOutput": 1}
Veredicto:       MATERIAL
Materialidad:    M-01 no, M-02 yes, M-03 yes, M-04 yes, M-05 yes, M-06 no, M-07 no, M-08 yes
ActionCompat.:   ACTION_COMPATIBLE
CanonicalInput:  needs-change
A-6:             yes (8 puntos de alcance mínimo)
OWNER-RESERVED:  2
Una invocación:  no
Acreditación:    ACCREDITED por el auditor v5.1-bmin (0 motivos); la acreditación final es del Coordinator
Clon tras corrida: HEAD 9356edf1, estado limpio
```

**Alcance mínimo de A-6 que señala el dictamen** (sin texto de enmienda; §75.7):
1. V14 D.3, Topología A, Controller de FX-02 en F6: ruta de ejecución con el instrumento fijado por blob (análoga a A4-1), sin tocar la receta 16.4, topes ni totales
2. V14 §20.3.1 (AP 16.24 L1264) y RAE §15.2 I6: admisión, solo para esa invocación, del archivo de la herramienta en CanonicalInputs y de su ejecución en AllowedActions con su fuente
3. Vía del blob hasta el -C del Controller sin F_seed ni F_norm y sin leer el worktree real de I-62 (V14 D.1 y D.6; AP L1267; orden §75.2)
4. AP 16.24: registro de fidelidad del archivo de la herramienta y cobertura de sus GitCalls e Inputs en la auditoría de lecturas frente al EffectiveInputClosure (P-22; Contexto)
5. AP 16.22 L1204-L1207 y V14 D.4: efecto de los códigos de fallo cerrado de la herramienta y del REJECT_FACTS del aceptante sobre Checks, Classification y el resultado de FX-02
6. V14 Anexo C, C-24 (L2333-L2334): oráculo del Coordinator calculado sin la herramienta ni el aceptante, sellado antes y no entregado al Controller
7. Elegibilidad de la celda del Controller con el instrumento: invocación medida que demuestre la ruta, bajo una autorización y un consumo del Owner (patrón A4-1, reglas 2-4)
8. ADR-0046 #9 (Level A, conservado por ADR-0048): determinación OWNER-RESERVED acotada a FX-02 en F6; exclusión expresa de producción, de otras unidades y de D.5

**OWNER-RESERVED:**
- Si la evidencia de F6 justifica usar un script operativo (la herramienta B-min y su aceptante) en la verificación del Controller de FX-02, frente a ADR-0046 #9 (Level A: «Sin plataforma ni scripts operativos hasta que la evidencia lo justifique»), conservado por ADR-0048, y con qué alcance (solo F6 o más).
- Autorización de consumo de cualquier invocación medida nueva de codex-cli que demuestre la ruta del Controller con el instrumento: el bloque A2-P2 de la identidad vigente está agotado y la medición de A4-1 está consumida.

Archivos: `output.json`, `audit.json`, `runtime-evidence.json` y `launch/` (saneado). La transcripción y `stdout.jsonl` no se versionan; sus SHA-256
están en `runtime-evidence.json`.
