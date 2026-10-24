# I-62 — Kit de la revisión formal única de la RONDA 3 de la enmienda A-7, acotada a A62-A7R2-R1 y a sus consecuencias (R20261023T180846Z-62bd)

```text
LogicalReviewRequestId: L20261023T180846Z-62bd   InvocationId: I20261023T180846Z-62bd   AttemptSeq: 1   RunId: R20261023T180846Z-62bd
SessionId:      a720544e-1ac0-43d9-96bd-ae39e1090dfb (uuid4, generado una vez el 2026-10-23T18:08:46Z con los demás ids; prep/ids.txt)
Orden:          decisiones §96 (línea 1880 en b9bfae50; cuerpo pegado 3 198 bytes, SHA-256 adb11535f24cac2874ddef13af712cc56eebece8905fe948db2f0077b3dd87b2
                = kit/order.txt = run/order.txt), puntos 6 (kit con referencias comprobadas, componentes congelados reutilizados, validación
                incremental según §88 y dictamen expreso sobre A-5 además de A-6) y 7 («Preparar el kit acotado, pero no lanzar una nueva revisión
                formal sin disposición posterior»). §96 es la última sección de decisiones en la HEAD b9bfae50 del worktree
Autorización:   PENDIENTE: owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None; la compuerta 11 bloquea con el único motivo
                «authorization pending» hasta que una disposición del Coordinator POSTERIOR a §96 que nombre esta revisión se fije y el kit se
                vuelva a congelar (abajo, «Lanzamiento»)
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que las rondas 1 y 2
Objeto:         docs/initiatives/I-62-A-7.md, ronda 3, PROPUESTA, blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff (45 668 bytes, 393 líneas), publicado en
                37ac6ca4a6f0a04268ac357764bb74f843eb3a08 (padre 5da2007f, con la ronda 2 8a528133 y §96) y registrado en b9bfae50 (resultado `run` de
                las guardas de r3/, PASS en G1-G7)
Ronda 2:        blob 8a528133c7cccd83356e31f8fd79cd2ebb0cec15 (commit e90b444d) = run/a7-r2.md (premisa); su revisión R20261019T201030Z-d1bf
                (output.json con A62-A7R2-R1 y O1-O4, audit.json NOT_ACCREDITED, README) está en el cierre, sin cambio (árbol a07efc37 = 3f912c84)
Commit del clon: b9bfae50a82ba59c834658e54d7699662ee298bb (HEAD de la rama; contiene la publicación, el registro de r3 y §96, su última sección)
Clon / run:     D:\r62-arch-a7r3 (HEAD separado en b9bfae50, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a7r3-run
                (order.txt = cuerpo §96, a7-r2.md = ronda 2, prompt.md = kit/prompt.md y 42 copias fieles fiel__*.txt). Los clones y runs de las
                rondas 1 y 2 no se tocaron
Transporte:     el de las rondas 1 y 2 sin cambio: claude-cli 2.1.299, %APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe, SHA-256
                295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b (presente y con ese SHA-256 el 2026-10-23); claude-opus-5-5, effort
                xhigh; Read, Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema;
                tope de 3 600 s; espera de 120 s tras `auth status`. La app instaló además claude-code 2.1.308 (riesgo 1)
Orden de lanzamiento: EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos
Compuerta D-01: MEASURED sobre kit/claude-cli-characterization.json (SHA-256 4ba6ba58…, byte a byte la de las rondas 1 y 2); transport_gate.py byte a byte
Compuerta 11:   BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-7 en la HEAD = 8bcccb2f y A-6 = 4bc919dd (acordada);
                decisiones solo por añadido desde el clon; la HEAD desciende del clon; una disposición posterior a §96 fijada (encabezado, blob
                completo, SHA-256 del texto, una sola sección) y ninguna sección posterior. En el kit entregado: PENDIENTE
Estado:         kit CONGELADO con la autorización PENDIENTE (kit/kit-manifest.json, SHA-256 84a9883def489347d2a5a4d5bf2aff8bfc2dac6bb77ec3e4be6107a0c482765f, 71 archivos más el manifiesto); sonda de registro en Ok;
                prueba en seco bloqueada SOLO por la compuerta 11 con el motivo «authorization pending» (4 y 5 no se ejecutan en seco). NO lanzado
```

## Qué pide §96 y dónde se cubre

| Exigencia | Dónde |
|---|---|
| §96.6: corregir el mecanismo que permitió la referencia defectuosa; referencias relativas, válidas, presentes en el cierre y comprobadas contra el archivo real | tres capas: (1) el patrón de referencias de `result.schema.json` solo admite las familias de rutas del cierre (documentos por su nombre exacto y subárboles de evidencia) y `order.txt`/`a7-r2.md`, relativas, con `/` y sin segmentos que empiecen por punto; StructuredOutput lo valida al generar (X06: las 262 rutas del cierre y las dos premisas pasan; 13 formas negativas, incluida la ruta de la ronda 2, no); (2) el auditor v7.0-a7r3 comprueba cada referencia contra CanonicalInputs y las líneas del archivo real en el commit, exige además que la ruta ESCRITA sea la relativa exacta (regla nueva de exactitud, `prep/patch_auditor_r3b.py`) e informa la referencia tal como se escribió y el motivo; (3) el prompt muestra literalmente las 262 rutas, manda copiarlas y pide una autocomprobación de cada referencia antes de entregar. Casos 15, 93-93h, 109, F09 y X06 |
| §96.6: sin alterar el resultado de la revisión anterior | la custodia de la ronda 2 está en el cierre como evidencia de solo lectura, con su árbol comprobado (`a07efc37` = `3f912c84`); nada se reescribe |
| §96.6: reutilizar los componentes congelados compatibles y validar por incrementos (§88) | siete componentes byte a byte (`transport_gate.py`, `c3_analysis.py`, `pin_authorization.py`, `fidelity.py`, `read_fidelity.py`, `selftest-fidelity.py` y la caracterización); diffs del resto en `prep/diffs/`; autoprueba 321/321, autoprueba de fidelidad 53/53, negativa de `verify_clone.py` y mutación COMPLETA (§88.4: cambiaron el auditor y el esquema) |
| §96.6: dictamen explícito sobre la compatibilidad con A-5, además de A-6 | `CompatibilityVerdicts` {A5, A6} en el esquema (COMPATIBLE, INCOMPATIBLE, NOT_DETERMINED, con Rationale, ClauseRefs y FindingIds); el auditor exige un REQUIRED vinculado a INCOMPATIBLE, ninguno a COMPATIBLE, ninguno NOT_DETERMINED con un veredicto, ClauseRefs en los dos y A-5 y A-6 COMPATIBLE para AGREED; A-5 entera en el cierre y sus reglas A5-1 y A5-2 señaladas en el prompt. Casos 69-73, 81, 88, 89, 100b, 102, 111, 114h, 114i y 121i |
| §96.7: preparar el kit acotado sin lanzar | autorización PENDIENTE; la prueba en seco queda bloqueada SOLO por la compuerta 11 con el motivo pendiente |
| §96.8: sin Codex, sin consumir A6-7, sin B1 ni FX-U2, sin tocar producción, I-65 ni D.5 | ninguna invocación de modelos (claude.exe solo en `--record-probe`, con un hijo sustituto, y `--dry-run`, que no lo ejecuta); el preflight de A-7 no se vuelve a ejecutar |

## Alcance del cierre (262 archivos, 5 787 069 bytes)

| Grupo | Archivos | Bytes |
|---|---|---|
| A-7 ronda 3 (objeto) y base normativa, fuentes literales L1-L22 (con el esquema `relay-record/v2`), A-1..A-6, decisiones §90-§96, evidencia §192-§197 | 22 | 2 094 775 |
| preflight de la ronda 3 (`I-62-A7/r3/`, entero, con su resultado `run`) | 58 | 699 595 |
| preflight de la ronda 2 (`I-62-A7/r2/`, entero, sin cambio) | 50 | 550 784 |
| revisión de la ronda 2 (`output.json`, `audit.json`, `README.md`) | 3 | 288 998 |
| m7/ (s91, s92, banco: README, designaciones, p1, N4, nc1; lecturas) | 55 | 1 027 837 |
| kit `a6m7-measure` entero | 44 | 583 283 |
| paquete B-min entero | 30 | 541 797 |

Fuera del cierre, frente al de la ronda 2: el preflight de la ronda 1 (`I-62-A7/` fuera de `r2/` y `r3/`), `r2-dispositions/`, la revisión de la
ronda 1 y `I-62-A6/r4/` (solo los citaba el preflight de la ronda 1). Ni A-7 ni los preflights de las rondas 2 y 3 los citan; el prompt y
`NotTriggered` del cierre lo declaran. Lectura directa del Paso 0: AUTOMATION_PLAN 16.7 L593-L595 (la reverificación «después de escribir, sin
conflictos», con el `RebaseMap` como entrada).

## Fidelidad de lectura (componentes de §94, punto 5, reutilizados)

42 archivos marcados por `fidelity.py` (sin cambio) se leen SOLO por su copia fiel RACKCAD-COPIA-FIEL/1 en el run: 16 con `UNMEASURED_CHAR` (13 solo
por ella), 16 con `LONG_LINE` (13 solo por ella), 8 con `BOM` y 5 con `ZWNBSP`; 2 356 398 bytes en total, la mayor `I-62-evidence.md` (447 407).
El objeto (A-7 ronda 3) no está marcado. Las copias no se custodian como archivos (sus rutas pasarían de MAX_PATH en el worktree): se reproducen byte a
byte desde `b9bfae50` con `kit/fidelity.py` (`prep/regen_faithful.py` → `prep/faithful-regeneration.json`, AllReproduced: 42/42 copias, 2 356 398 bytes).

## Qué cambió frente al kit de la ronda 2

Base: `kit-review2/kit/` = el kit de la ronda 2 tal como se fijó en §95 y se lanzó (custodia `e42d23d1`/`3f912c84`). Diffs unificados en
`prep/diffs/`, tabla en `prep/diffs/diff-table.json` (`prep/make_diffs_r3.py`). Cada componente cambiado tiene su script de derivación en `prep/`.

| Componente | Ronda 2 → ronda 3 | Qué cambia |
|---|---|---|
| `transport_gate.py`, `c3_analysis.py`, `pin_authorization.py`, `fidelity.py`, `read_fidelity.py`, `selftest-fidelity.py`, `claude-cli-characterization.json` | IDÉNTICOS | — |
| `result.schema.json` | `9e533317` → `4b003339` | `rackcad-a7r3-architect-review/0` (`prep/gen_schema_r3.py`, reproducible): PriorFindingResolution de A62-A7R2-R1; DeltaPurity con los ocho cambios de §3.7, WITHIN_SCOPE/OUTSIDE_SCOPE; RegressionCheck de ocho temas; CompatibilityVerdicts A5/A6; Q-A7R3-01..03; Step0 de la ronda 3; sin OptionalDispositionConformance ni RelatedImpacts; patrón de referencias por familias del cierre; 18 367 caracteres compactos ASCII, las mismas palabras clave medidas |
| `post-review.py` | `f5cdef1f` → `e1d6b1b2` | v7.0-a7r3 (`prep/patch_auditor_r3.py` y `prep/patch_auditor_r3b.py`): esquema y coherencia de la ronda 3, compatibilidad, compuerta 11 tras §96, mensaje de referencia no canónica con la referencia escrita y su motivo, y la regla de exactitud de la ruta escrita |
| `owner_gate.py` | `0064b35e` → `7e0cf27a` | objeto 8bcccb2f, base §96, AUTH_* = None (PENDIENTE), RunId; funciones byte a byte (`prep/patch_owner_gate_r3.py`) |
| `make_closure.py` | `756459c0` → `2fbbd6a8` | cierre de la ronda 3, comprobaciones de identidad de la ronda 3, lectura directa AP 16.7 L593-L595, prompt con las rutas del cierre (falla si el prompt no muestra alguna) |
| `prompt.template.md` | `24b93624` → `05250607` | alcance A62-A7R2-R1, Paso 0 de la ronda 3, compatibilidad A-5/A-6, autocomprobación de referencias |
| `launch.py`, `prepare_clone.py`, `verify_clone.py`, `finish_kit.py`, `make_manifest.py`, `mutation-post-review.py` | cambiados | constantes de la ronda 3 (clon, run, sesión, premisa a7-r2.md, patrón de proyecto `D--r62-arch-a7r3*`, §96); `verify_clone.py` reproduce las guardas de r3/; la lógica de lanzamiento, registro y mutación no cambia |
| `selftest-post-review.py` | `435f76f4` → `318ef540` | casos de la ronda 3 (abajo); los de transporte, lectura, custodia, lanzador, compuertas y fidelidad, sin cambio salvo nombres |

## Hallazgo al reproducir las guardas de la ronda 3

`verify_clone.py` reproduce el resultado `run` de las guardas (entero e igual al custodiado) y su autoprueba (38/38). La autoprueba custodiada,
`r3/q7-guards-selftest.json`, se ejecutó con un archivo más en el área del preflight: `preflight_text_r2.py`, una copia byte a byte de
`r2/preflight_text_r2.py` (blob `a773a873`), que se borró después (2026-10-23T18:00:03Z), antes de regenerar `MANIFEST.sha256` y publicar. Con los 51
archivos publicados que existían al ejecutarla más esa copia, la reproducción da el mismo commit hipotético (`fbb0d5a3`, Added 53) y el resultado
entero custodiado; sin ella, no (`prep/find_selftest_evid.py`). El resultado `run` de las guardas sobre la publicación real (PASS) no depende de ello.
`verify_clone.py` lo declara (`EVID_REMOVED_BEFORE_PUBLICATION`) y comprueba el blob de la copia.

## Corrección de un dato del registro

La referencia que dejó sin acreditar la revisión de la ronda 2 estaba escrita **relativa**, `docs/initiatives/I-62-A7.md:167-168` (`output.json`,
línea 560; sin el guion de `A-7`), no absoluta: el mensaje del auditor v6.0-a7r2 mostraba su resolución absoluta en el clon (`audit.json`, línea
5960). La evidencia §195 y el texto de §96, punto 6, hablan de una ruta absoluta; el mecanismo corregido cubre los dos casos (las rutas absolutas ya
las rechazaba el esquema de la ronda 2; la mal escrita, no).

## Preparación ejecutada (2026-10-23)

1. **Identificadores** generados una vez (`prep/ids.txt`): RunId, InvocationId, LogicalReviewRequestId y SessionId (uuid4).
2. **Esquema** `prep/gen_schema_r3.py` → `kit/result.schema.json` (SHA-256 `4b0033392d7aee27cbeae03db2d3e756d668a2605bf281de3dc7e6e786404d08`; reproducible).
3. **Componentes derivados** del kit de la ronda 2 por sustituciones exactas comprobadas (`prep/patch_*.py`, cada uno exige el SHA-256 de partida):
   `owner_gate.py`, `post-review.py` (dos pasos), `launch.py`, `verify_clone.py`, `finish_kit.py`, `make_manifest.py`, `mutation-post-review.py`,
   `selftest-post-review.py` y `make_closure.py` (docstring, identidad y renderizado por script; constantes y rangos con ediciones exactas: el diff
   completo está en `prep/diffs/make_closure.py.diff`); `prepare_clone.py` y `prompt.template.md` reescritos para la ronda 3 (diffs en `prep/diffs/`).
4. **Clon y run** (`kit/prepare_clone.py`, 2026-10-23T18:34:01Z): las quince precondiciones en true (`kit/kit-params.json`).
5. **Copias fieles y prompt** (`make_closure.py --faithful-copies` y `--render-prompt`): 42 copias; `prompt.md` de 110 404 bytes y 731 líneas
   (SHA-256 `085776ac9c20f3d28bb84ea0f690f76144e6ee4fa70ec51597c7b21be14bb157`), que muestra literalmente las 262 rutas del cierre (el renderizado falla si falta alguna).
6. **Cierre** (`make_closure.py`): `kit/closure.json` (SHA-256 `e399b04a6a6231e81fdd9700dd1eece01e3517fd774aea93d7900ab4992e254a`), 262 archivos, comprobaciones de identidad de la ronda 3 en true.
7. **Compuerta D-01** (`transport_gate.py init` y `set MEASURED`): `kit/transport-gate.json` MEASURED sobre la caracterización `4ba6ba58…`.
8. **verify_clone.py**: AllChecks; guardas de r3/ reproducidas: `run` PASS, entero igual al custodiado; autoprueba PASS, 38/38 mutantes muertos, Added 53
   con el conjunto que existía al ejecutarla (más `preflight_text_r2.py`; arriba, «Hallazgo»), entera igual a la custodiada.
9. **Autoprueba** `selftest-post-review.py` (2026-10-23, 19:03Z-20:05Z): **321/321** (auditoría 214, compuerta de transporte 61, compuerta 11 40 con la sonda
   viva L00 en estado PENDIENTE, consistencia 6 con X06); clon limpio y run igual al kit al terminar. Pasó a la primera.
10. **Cadena** `prep/chain_r3.sh` (registro `prep/chain.log`): autoprueba de fidelidad 53/53 (2026-10-23T20:05:48Z → 2026-10-23T20:05:57Z, código 0); negativa de verify_clone (2026-10-23T20:05:58Z → 2026-10-23T20:07:46Z, código 0; la copia alterada
    no verifica, las otras 41 sí, clon limpio: `prep/neg-verify-result-r3.json`); manifiesto, sonda y prueba en seco previa (bloqueada solo por la
    compuerta 11: `launch-dry-run-before-full-mutation.json`); **mutación COMPLETA 209/209** (`post-review.py` 131/131, `transport_gate.py` 40/40, `owner_gate.py` 19/19, `fidelity.py` 19/19; 3 muertos por excepción; línea base 320 casos; 2026-10-23T20:08:09Z → 2026-10-24T00:48:23Z, código 0);
    manifiesto final, sonda (`record-probe.json`, RecordProbe y FrozenCommandGuard en Ok) y prueba en seco final (`launch-dry-run.json`): código 2,
    `BlockingChecks` = [11-OwnerAndAuthorizationGate] con el único motivo pendiente; 1-3 y 6-10 en Ok; 4 y 5 no ejecutadas en seco.
11. **Copias fieles reproducidas** desde `b9bfae50` (`prep/regen_faithful.py` → `prep/faithful-regeneration.json`): AllReproduced: 42/42 copias, 2 356 398 bytes.
12. **Privacidad** (`prep/privacy_scan.py` → `prep/privacy-final.json`): 150 archivos, 0 coincidencias fuera de las copias fieles exentas (texto del
    repositorio, que no se custodia). Las líneas de órdenes de los procesos no se registran (solo el ejecutable): podrían nombrar archivos de terceros.

## Supuestos no medidos y riesgos residuales

1. **Binario:** la app instaló claude-code 2.1.308 junto al 2.1.299 medido (presente con su SHA-256 el 2026-10-23). Si una actualización borra el
   2.1.299 (como pasó con 2.1.293 el 2026-10-16), la comprobación 3 bloquea y hace falta una caracterización nueva.
2. **Duración frente al tope de 3 600 s:** la ronda 2 duró 821 s (55 turnos). La ronda 3 tiene un alcance más estrecho, un cierre de tamaño parecido
   (262 archivos) y un esquema con ocho cambios, ocho temas y dos dictámenes de compatibilidad. El tope no se cambió.
3. **Esquema de 18 367 caracteres compactos** (11 063 en la ronda 2), por el patrón de referencias (620 caracteres, con alternancias y clases de
   caracteres, sin anticipaciones); las palabras clave son las medidas, pero su comportamiento con claude-cli a este tamaño no está medido.
4. **Compactación automática del contexto:** el prompt avisa; contaría como un segundo mensaje de usuario (NOT_ACCREDITED).
5. **Que el revisor lea un original marcado:** si la entrega se altera es DEGRADED y motivo; si no, informativo. El prompt da las copias.
6. **El hijo sustituto de la sonda no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro, no la salida real.
7. **Anclaje:** la evidencia de I-62, los `preflight.md` y las notas son de la sesión que preparó A-7; el prompt los presenta como evidencia
   contrastable, nunca como autoridad.
8. **Preparador no independiente:** la sesión principal, que preparó A-7 ronda 3, preparó este kit; el prompt no contiene veredicto esperado ni
   valoraciones, y el revisor es un proceso nuevo de claude-cli con su propio clon, run y proyecto.
9. **`pin_authorization.py`** conserva su docstring histórica (ronda 4 de A-6) para no cambiar su SHA-256; su lógica usa `owner_gate.BASE_SECTION`
   (ahora 96).

## Host: I-64 y checkout principal (solo lectura)

- `prep/host_checks_r3.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process, solo el ejecutable) a las 2026-10-24T00:50:13Z (`prep/host-checks-final.json`):
  checkout principal en `main`, HEAD `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`, 0 líneas de estado, origin/main local `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`; worktree de I-64 en `09f2f1c3` con 0 líneas de estado;
  worktree de I-62 en `b9bfae50` con 0 líneas; clon de la ronda 3 en `b9bfae50` con 0 líneas (con ignorados); clon de la ronda 2 en `b432c40f` con 0 líneas;
  run de la ronda 2 con 47 entradas y la última modificación el 2026-10-22T20:30:54Z (no se tocó); claude-code instalados: 2.1.299, 2.1.308; procesos vigilados: acad.exe (iniciado 2026-10-23T11:01:44; sin mención de I-64 ni de un clon r62-arch-a7);
  memoria libre 22.8 GB, CPU 9 %.
- La preparación no tocó el worktree de I-64 ni el checkout principal (solo `rev-parse` y `status` con GIT_OPTIONAL_LOCKS=0). AutoCAD estaba abierto
  por el Owner; esta preparación no lo lanzó ni lo usó.
- **Energía:** el equipo estuvo conectado a la corriente; la sesión retuvo la suspensión por inactividad y la pantalla mientras corrían la autoprueba
  y la cadena (peticiones de energía por proceso, `prep/hold_awake.py`, sin cambiar ajustes, liberadas al terminar). Sin suspensiones.

## Lanzamiento (no ejecutado; exige una disposición nueva)

1. Una disposición del Coordinator POSTERIOR a §96 que nombre esta revisión (RunId R20261023T180846Z-62bd, objeto 8bcccb2f) en las decisiones de la
   rama; ninguna sección posterior a ella hasta lanzar y auditar.
2. Fijarla: `python -B kit/pin_authorization.py <texto de decisiones de la HEAD> <número>` (Ready: número posterior a §96, una sola sección, blob
   completo como token, ninguna sección posterior) y sustituir SOLO las tres líneas AUTH_* de `kit/owner_gate.py`.
3. Volver a congelar: `python -B kit/make_manifest.py`; repetir la autoprueba de la compuerta 11 y la sonda L00 en estado FIJADO y la mutación dirigida
   de `owner_gate.py`, como en la ronda 2.
4. Desde esta carpeta: `python -B kit/launch.py --record-probe` (código 0) y `python -B kit/launch.py --dry-run` (código 2, `BlockingChecks` = []);
   después, UNA vez, `python -B kit/launch.py` (tope 3 600 s, sin reintento) y el auditor sobre la corrida completa:

```text
python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a7r3\a720544e-1ac0-43d9-96bd-ae39e1090dfb.jsonl" D:\r62-arch-a7r3-run\launch\stdout.jsonl D:\r62-arch-a7r3 D:\r62-arch-a7r3-run kit kit/result.schema.json audit.json output.json
```

El auditor informa por separado `Categories.Execution` y `Categories.ArchitectVerdict`; el veredicto no es el acuerdo. Ni A-7 AGREED, ni Controller
ELIGIBLE, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Archivos y privacidad

Los SHA-256 de todos los archivos de esta carpeta están en `MANIFEST.sha256` (la regla `-text` de `.gitattributes` conserva los bytes
exactos). Las 42 copias fieles `kit/fiel__*.txt` se custodian por reproducción (`prep/faithful-regeneration.json`); sus SHA-256 están en
`kit/kit-manifest.json` y en `FaithfulCopies` del cierre. Componentes del kit (sin las copias fieles):

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `kit/a7-r2.md` | 37 205 | `dbb7db53c5668afdab8bf8cdf0f4b005b4ecdcd7747606015d1d8f7814426d9a` |
| `kit/c3_analysis.py` | 6 070 | `4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7` |
| `kit/claude-cli-characterization.json` | 17 086 | `4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c` |
| `kit/clone-verification.json` | 89 362 | `70e7c6b6097a03f1e667e78ce520625efb94f36b1d9b1a11027944eea67347c4` |
| `kit/closure.json` | 412 292 | `e399b04a6a6231e81fdd9700dd1eece01e3517fd774aea93d7900ab4992e254a` |
| `kit/fidelity.py` | 21 072 | `a84825e00112965f33c7530ffac6ff84353ff036f2773ee525ebd178e872c68f` |
| `kit/finish_kit.py` | 11 391 | `f82cbc317ae1dabf4cbf97d29a503e4bd358185fef2253158e9e8117935e9b61` |
| `kit/kit-params.json` | 2 724 | `b4c1506f724f1e036eea5737d3fb7a96ca5d6d88f8a3e0e7006bda19ce8f08e4` |
| `kit/launch.py` | 48 259 | `87ff62ffeaeeefdffd9849181f995110e5ee340b85c9c6697a06c4f612220ea1` |
| `kit/make_closure.py` | 108 215 | `2fbbd6a8d772a692d4184f954a14d12ac2cddf707cc06ec37a08a912b3e624db` |
| `kit/make_manifest.py` | 10 728 | `575e04faffbb88652d252e4ac6bffad42d6a7ac1003a4c36a322c4eac13ed4ed` |
| `kit/mutation-post-review.py` | 11 182 | `cf120dd8647249e0279ca9e029830bc08130810d3e02d667c692ce05928cc35f` |
| `kit/mutation-result.json` | 68 089 | `f2f0039573631f2181ebe4af6b09601a24f8e9f149d6eb41402d869e3cd2ae43` |
| `kit/order.txt` | 3 198 | `adb11535f24cac2874ddef13af712cc56eebece8905fe948db2f0077b3dd87b2` |
| `kit/owner_gate.py` | 17 967 | `7e0cf27aa84f433ebb0e046bbb0147dc76e273bbcb78548156c780fd633cac74` |
| `kit/pin_authorization.py` | 5 588 | `a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc` |
| `kit/post-review.py` | 76 064 | `e1d6b1b26da7f61f036d820ab94a944fb357810d3945146dcfbecda9d66c8ca0` |
| `kit/prepare_clone.py` | 11 550 | `82082790f250bbcd419e51abf9d36160b75a9a0749cdc8466e1df715fb9ce38d` |
| `kit/prompt.md` | 110 404 | `085776ac9c20f3d28bb84ea0f690f76144e6ee4fa70ec51597c7b21be14bb157` |
| `kit/prompt.template.md` | 40 948 | `05250607fd464c3b98b458a4d0712b6efbc8bb1d77435f815d1c2b31109caa0a` |
| `kit/read_fidelity.py` | 6 143 | `da1457a1a035221ba41710f1bea6bcd28312c95d66199885accccf3a21f11cfa` |
| `kit/result.schema.json` | 23 896 | `4b0033392d7aee27cbeae03db2d3e756d668a2605bf281de3dc7e6e786404d08` |
| `kit/selftest-fidelity-result.json` | 9 062 | `df0506164f5019276479cd0d9039a5975974faf2a592b233f477de29c544b890` |
| `kit/selftest-fidelity.py` | 18 240 | `0ad71f0d3f3bd4be6986d0851910a095ff573e48193e9c106d7c53fefffd0da3` |
| `kit/selftest-post-review.py` | 153 622 | `318ef54023219db4ef0f0cbccbc583b36387703d51e653667be106af50bb34ec` |
| `kit/selftest-result.json` | 179 917 | `23796b9087725320f85a2ab5d80aedf98e4720e7b6a4acb952e1049d94582e40` |
| `kit/transport-gate.json` | 1 544 | `4cbd5e3518597ebe20ecd311d2090c3c00b10039086d32f3d27dce3fbf555710` |
| `kit/transport_gate.py` | 22 485 | `e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf` |
| `kit/verify_clone.py` | 29 879 | `6ccdcb3267c108da8bcfb7275719f8e3ae2f30dc7418bd869987907ddcbe55b2` |
| `kit/kit-manifest.json` | 30 101 | `84a9883def489347d2a5a4d5bf2aff8bfc2dac6bb77ec3e4be6107a0c482765f` |
| `record-probe.json` | 2 919 | `332ec560e148229a66437ae4eb0d61eafdc4269b02fa80ba41d5942aff60d6fe` |
| `launch-dry-run.json` | 15 460 | `9cd2ca22a6d906d59d473f3d57c4edafd0892c5818ee8da9faaa915d8ec274da` |
| `launch-dry-run-before-full-mutation.json` | 15 460 | `94f7d8463ecc84b1dfbf23c31a6b0e4ec739fe98b771b4c5442baa8bf84b65e9` |

`prep/` contiene los scripts de derivación y de preparación, los diffs frente al kit de la ronda 2, las salidas crudas de la cadena y
los resultados de la negativa, la reproducción, el host y la privacidad. Ninguna transcripción, credencial ni archivo de autenticación.
