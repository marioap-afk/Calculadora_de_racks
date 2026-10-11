# I-62 — Kit de la primera revisión formal de la enmienda A-5 por `claude-cli` (R20261011T065226Z-dbb0)

```text
LogicalReviewRequestId: L20261011T065226Z-dbb0   InvocationId: I20261011T065226Z-dbb0   AttemptSeq: 1   RunId: R20261011T065226Z-dbb0
Autorización:   disposición del Coordinator §72, punto 2 (cuerpo pegado, 2 556 bytes, SHA-256 0baac461…, el que declara decisiones §72): revisión
                formal por claude-cli si la celda y la autorización siguen vigentes (vigencia comprobada en decisiones §72: binario 8693c4a0… y
                CLAUDE-CLI-I62 = A); la revisión examina expresamente ocho temas; sin AGREED antes del dictamen acreditado y del veredicto del
                Coordinator. Owner CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura). UNA revisión, sin reintento automático
Consumo:        cubierto por CLAUDE-CLI-I62 = A, la misma base que las revisiones de A-2, A-3 y A-4
Objeto:         docs/initiatives/I-62-A-5.md   blob ad94ceedacbf3dbf70f0217a583978d0e32592fc (primera revisión; sin hallazgos anteriores)
                publicada en bf2cf857 (padre efcfcbc5, sin A-5); guardas `run` PASS custodiadas en 1a0db7b0; decisiones §72 en b631c46f
                docs/automation/evidence/I-62-A5/a5-guards.py                blob 98d264056fe800477cf37fbfd5bde34877857c68
                docs/automation/evidence/I-62-A5/a5-selftest.json            blob 8d115759c2a5ef86a417da06a84cec81076ca25e   (A5TextBlob ad94ceed)
                docs/automation/evidence/I-62-A5/a5-guards-result-run.json   blob 2a383d79aefb773363ba84bdca2d336d34f14159   (RUN, Head bf2cf857, PASS)
Paquete:        ninguno (A-5 §6 lo prevé solo cuando una disposición autorice la revisión; el prompt hace de paquete)
Freeze:         b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43; V14 4c617e82b32b6c810b68d75fc19472efed22b393, blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
                (los dos commits son anteriores al rebase y no están en el clon; V14 se identifica por su blob en el commit)
Clon:           D:\r62-arch-a5 (git clone --no-local -c core.autocrlf=false --single-branch desde el worktree; HEAD separado en b631c46f; sin ramas
                locales; sin remoto; sin enlaces; kit/clone-verification.json, AllChecks)
Run:            D:\r62-arch-a5-run, con tres archivos y nada más antes del lanzamiento:
                order.txt (cuerpo §72, 2 556 bytes, 0baac461…), delta.diff (git diff efcfcbc5 bf2cf857 -- docs/initiatives/I-62-A-5.md; archivo
                nuevo, 37 900 bytes, 3e5d39d5…) y prompt.md (= kit/prompt.md, 31 545 bytes, 1454c917…)
Transporte:     claude-cli 2.1.293: %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa,
                Authenticode válido de «Anthropic, PBC» (re-medidos sin ejecutar el binario); claude-opus-5-5 con effort xhigh; herramientas Read,
                Grep y Glob (+ StructuredOutput); --safe-mode, sin MCP; --add-dir del run; session id dcb1b798-b53a-4a31-9693-61cd4a2dfe51 (uuid4,
                nuevo); cwd = el clon; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s entre `auth status` y el lanzamiento
Compuerta D-01: kit/transport-gate.json en MEASURED (vía a) sobre el registro de caracterización existente R20261008T183600Z-char
                (claude-cli-characterization.json, SHA-256 20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c, fijado; la copia del clon
                D:\r62-arch-a5, blob 541956eb): Probes.C3 PASS sobre la plantilla exacta, STALE = false, transport_gate.evaluate sin problemas
Estado:         kit preparado ANTES del lanzamiento (commit de custodia: pendiente); NO lanzado. Lo lanza la sesión principal con kit/launch.py, una vez
Acreditación:   la decide el Coordinator después de la corrida (auditor v5.1-a5 sobre la transcripción, la salida stream-json y launch/)
```

**Recibo de publicación.** A-5 se publicó en `bf2cf857` (padre `efcfcbc5`, donde no existía) y conserva el blob `ad94ceed…` en `1a0db7b0` (custodia
del resultado `run` de las guardas) y en `b631c46f` (decisiones §72), el commit del clon. `a5-guards.py`, `a5-selftest.json` y `preflight.md` no
cambian después de `bf2cf857`; `1a0db7b0` añade `a5-guards-result-run.json`, `a5-guards-result-preview.json` y `out/candidate-commit.json`;
decisiones y evidencia solo crecen por añadido después de `bf2cf857` (prefijo exacto, comprobado). Todos los blobs que A-5 fija en su cabecera,
§2, §6 y §9 para los archivos del cierre (V14, Freeze, A-1..A-4, AUTOMATION_PLAN, LIFECYCLE, mapa, esquema, `clause_map.py`, `mv_check.py`,
`characterization.md`, `reconstruction-plan.md`, `effects.md`, conciliación de U-14 y borrador de B1) y los árboles `MAP-INVALID/` (`b0b3b258`) y
`agent-execution/` (`809fd58f`) coinciden con los del commit (`make_closure.py` lo comprueba). A-5 no se edita.

**Delta.** Es la primera revisión: no hay versión anterior. `delta.diff` es la salida literal de `git diff efcfcbc5 bf2cf857 -- <objeto>` (archivo
nuevo, línea `index 00000000..ad94ceed`); sirve para la comprobación de identidad del Paso 0, y sus líneas `+` son el objeto, línea a línea
(`make_closure.py` y `verify_clone.py` lo comprueban). No se usa una vista previa de V14 con A5-1 y A5-2 aplicadas porque no sería salida literal de
git; la aplicación sobre V14 + A-2 + A-4 la comprueba G5 y el revisor la puede contrastar con V14 D.1/D.3, A-2 y A-4 §3.8, que están en el cierre.

## Contrato y auditor (fijados antes de invocar)

- **Cierre** (`kit/closure.json`, generado por `kit/make_closure.py` desde el clon; SHA-256 `6f9c092f…`): 54 archivos canónicos
  (1 927 171 bytes) y tres archivos del run:
  - A-5 entera, con rangos calculados (cabecera, secciones, párrafos propuestos de A5-1 y A5-2 y cada una de sus reglas, filas M-01..M-08, líneas del
    Owner y Q-A5-01..Q-A5-12);
  - V14 (Anexo C con C-22, C-25a y C-39; D.1, D.2, D.3, D.4, D.5, D.6, D.8, §17 y §18), con las líneas que cita A-5 comprobadas por su primera línea
    (2361, 2388-2397, 2391, 2394, 2395, 2398-2406, 2437, 2442-2445, 2523, 2528, 2544, 2558, 970-975, 974 y 975);
  - A-2 y A-4 enteras (líneas citadas 148, 156, 159, 170, 233; 203, 351, 355, 361 y §7), cabeceras de A-1 y A-3, el Freeze, LIFECYCLE §3, §5, §6 y §8;
  - AUTOMATION_PLAN 16.13 y 16.14 (731, 822, 902-908, 920-921, 945-946 comprobadas); el mapa de cláusulas y su esquema; `clause_map.py` de F4;
  - todo `docs/automation/evidence/I-62-A5/` (31 archivos: preflight, guardas, autoprueba, resultados `run` y `preview`, constructor, negativos,
    conformidad y los 21 de `out/`);
  - de `I-62-F6/MAP-INVALID/`: `characterization.md`, `effects.md`, `reconstruction-plan.md` y `mv_check.py`; la conciliación de U-14 y el borrador
    de B1;
  - decisiones §46, §48, §51 y §63-§72 (1265, 1454, 1456, 1458 y 1469-1470 comprobadas); evidencia §115-§133; el README de F6;
  - solo como referencia de formato, el registro y la salida de la segunda re-revisión de A-4 (`I-62-architect-review-A-4-r3.md` y su `output.json`);
  - `PriorFindingIds` = [] (primera revisión); run: los tres archivos, clase RUN, con `RunFileHashes`; `order.txt` y `delta.diff` pueden ser premisa;
  - transporte: flags medidos en C1, `AddDir`, `ArgsTemplate` (idéntica a la de los kits de A-2, A-3 y A-4), `TimeoutSeconds`, `AuthSettleSeconds` y
    `Gate`.
- **Herramientas y lectura.** Solo Read, Grep y Glob, y StructuredOutput para el resultado. Los rangos orientan, no limitan. Es una violación: cualquier
  Read o Grep fuera del cierre y del run; un Grep sin `path`, sobre un directorio o con `glob` o `type`; un Glob cuyo alcance o cuyas rutas devueltas
  contengan un archivo fuera del cierre; cualquier otra herramienta; toda denegación de permiso. Archivos de más de 60 000 bytes por tramos de 300
  líneas como máximo.
- **Prompt** (`kit/prompt.md` = `D:\r62-arch-a5-run\prompt.md`, byte a byte; 31 545 bytes, 308 líneas, SHA-256
  `1454c9176db58e71fd17267bc391333fa2f52abd55feb6a0541d4ebba3495f0b`; sin salto de línea final). Neutral: sin veredicto esperado ni contexto privado.
  Pide:
  - el Paso 0: `order.txt` entera (puntos 1 y 2, líneas 3-21), `delta.diff` 1-6 (línea `index 00000000..ad94ceed` y rutas), decisiones 1462-1477
    (§72), `a5-guards-result-run.json` 1-5 y 111-116 (Mode, Base, Head, Result) y `a5-selftest.json` 510-528 (Summary, Result, Repo, ToolBlob y
    A5TextBlob);
  - disposiciones explícitas de los ocho temas de §72, punto 2, en `Focus`: `A5-1_CONFORMS_AP_16_13_16_14` (16.13 con Evaluate/Resolve/Validate y
    MV-1..MV-7, y 16.14), `ORIGINAL_FIXTURE_HISTORY_INTEGRITY`, `IMPACT_C-22_C-25a`, `BUDGET_SEPARATION_A4-6_A5-2`, `NO_RESETS_NO_QUOTA_TRANSFERS`,
    `D5_AND_OWNER_VALIDATION`, `OWNER_RESERVED_DECISIONS` y `GUARDS_AND_NEGATIVES_SUFFICIENCY`; cada tema contesta además las preguntas Q-A5-* que le
    corresponden;
  - `A51Verdict` y `A52Verdict` separados (separabilidad, Q-A5-08); `D5Disposition` (la decisión sobre D.5 y la Owner Validation que A-5 §3.4 deja
    abierta); Q-A5-01..Q-A5-12; `ScopeConfirmation` (§72, punto 1: añadidos puros al final de D.1 y de D.3, ningún texto congelado sustituido, la cita
    interna de A5-1 = V14 2391 y las cifras de A5-2 = V14 2442); `OwnerAuthorizationLines` para las dos líneas del Owner (una acción o un consumo cada
    una y solo con efecto tras el acuerdo); `NoChangeConfirmation` (12); `GuardsVerification` (G1-G5, G5b, negativos N-1..N-11 y conformidad
    K-1..K-11); materialidad; modo e identidad;
  - **números de línea exactos** en cada premisa;
  - los identificadores de cuenta inyectados, solo por su tipo.
- **Esquema** (`kit/result.schema.json`; compacto, 14 348 caracteres ASCII; SHA-256 `909f1cda…`): la forma del de A-4 sin
  `PriorFindingDispositions` ni `DeltaConfirmation` (no hay versión anterior), con ids `A62-A5-NN` / `A62-A5-ON`, `Focus` (8 temas de §72),
  Q-A5-01..12, `A51Verdict` y `A52Verdict` (misma forma que `A45Verdict`), `D5Disposition` (`Disposition`: DEFER_UNTIL_OV_PREPARATION |
  NO_D5_IMPACT | COVER_IN_A5 | OWNER_DECISION_REQUIRED | NOT_DETERMINED; `OwnerValidationAffected`: YES | NO | NOT_DETERMINED; `FindingIds`;
  `Rationale`; `PremiseRefs`), `ScopeConfirmation` (5 claves), `NoChangeConfirmation` (12 claves: A1, A2, A3, A4, ClauseMapAndSchema,
  AutomationPlan1613to1614, V14OutsideD1D3Appends, D3RowsAndTotalsBeyondA52, A46Quota, OD1toOD7, OVMatrix, Production),
  `OwnerAuthorizationLines` (ONLY_ACTION_OR_CONSUMPTION | EXCEEDS_ACTION_OR_CONSUMPTION | NOT_DETERMINED, con `EffectiveOnlyAfterAgreement`),
  `GuardsVerification` (G1, G2, G3, G4, G5, G5b, MV_NEGATIVES_N1_N11, CONFORMANCE_K1_K11), `Materiality` (M01..M08), `OwnerAuthorityCheck` e `IfAgreed`
  (7 booleanas: ScopeLimitedToD1AndD3Appends, MapAndAuthoritiesUnchanged, OriginalFixtureHistoryPreserved, NoResetsOrQuotaTransfers,
  NoOwnerDecisionRequired, NoChangeToProtectedSurfaces, SuitableForCoordinatorAgreement).
- **Compuerta de transporte D-01** (`kit/transport_gate.py`, byte a byte igual al de los kits de A-4, predicados sin cambio): entregada en MEASURED
  (vía a) sobre el registro existente `20d0de2c…` (copia del clon). Sin ejecutar el binario: SHA-256 `8693c4a0…` y firma válida
  (Get-FileHash y Get-AuthenticodeSignature, en la preflight en seco).
- **Auditor v5.1-a5** (`kit/post-review.py`; SHA-256 `40575d05…`): las reglas de transporte, lectura, fidelidad, premisas y custodia de v5.1-a4r2,
  sin cambio, adaptadas al objeto y al cierre de A-5. Cada fallo es un motivo y ACCREDITED exige cero motivos. Coherencia:
  - sin reglas de hallazgos anteriores (no los hay); cobertura de Q-A5-01..12 y de los ocho temas de `Focus`; `ScopeConfirmation` en CONFIRMED o
    NOT_CONFIRMED con un veredicto;
  - `A51Verdict` y `A52Verdict` con las reglas de `A45Verdict`/`A46Verdict` (NOT_ASSESSED solo con NOT_ACCREDITED; CHANGES REQUIRED con un REQUIRED;
    BLOCKED con `OwnerDecisionRequired`; ids existentes);
  - `D5Disposition`: con un veredicto, determinada en sus dos campos y con al menos una premisa (que se audita como las demás); COVER_IN_A5 con un
    REQUIRED vinculado; OWNER_DECISION_REQUIRED solo con `OwnerDecisionRequired`; ids existentes;
  - AGREED: cero REQUIRED (también en `Focus` y en las preguntas), `A51Verdict` y `A52Verdict` en AGREED o EXCLUDE con al menos uno AGREED,
    `D5Disposition` en DEFER_UNTIL_OV_PREPARATION o NO_D5_IMPACT, sin decisión del Owner, `OwnerAuthorizationLines` = ONLY_ACTION_OR_CONSUMPTION con
    `EffectiveOnlyAfterAgreement` = YES, `NoChangeConfirmation` todo NO_CHANGE, `ScopeConfirmation` todo CONFIRMED e `IfAgreed` todo en true;
  - CHANGES REQUIRED, BLOCKED y NOT_ACCREDITED como antes; identidad revisada (commit, ruta, blob) e ids de la invocación;
  - clon después de la corrida: HEAD = el commit, separado, sin ramas locales ni remotos, limpio con ignorados, blobs designados, sin enlaces.
  Uso: `python -B kit/post-review.py <transcripción> D:\r62-arch-a5-run\launch\stdout.jsonl D:\r62-arch-a5 D:\r62-arch-a5-run kit
  kit/result.schema.json <audit.json> [<output.json>]`.
- **Lanzador** (`kit/launch.py`; SHA-256 `5ae8506f…`; no lo ejecuta quien prepara el kit, salvo `--dry-run`). Se niega a lanzar (código 2, sin
  escribir nada) si falla: 1. integridad del kit frente a `kit-manifest.json`; 2. transporte del cierre; 3. binario (ruta, SHA-256, Authenticode);
  4. `--version` = 2.1.293; 5. `auth status` con `loggedIn`; 6. clon (HEAD, separado y sin ramas locales, limpio, blobs, sin enlaces); 7. run con
  exactamente los tres archivos y sus hashes; 8. sin transcripción previa con el session id ni directorio de proyecto `D--r62-arch-a5*`; 9. línea de
  órdenes de menos de 32 000 caracteres (mide 16 968); 10. compuerta en MEASURED o ACCEPTED.
  Después escribe `preflight.json` y la copia del registro en `D:\r62-arch-a5-run\launch\`, **espera 120 s desde el final de `auth status`**,
  vuelve a medir el binario y lanza la ruta resuelta con la lista exacta, stdin = `prompt.md`, cwd = el clon y un tope de 3 600 s con terminación.
  Escribe `stdout.jsonl`, `stderr.txt` y `run.json` (con `AuthSettle` y `TransportFailure`: TRANSPORT_AUTH_REFRESH_CONFLICT si el `result` contiene
  «Failed to refresh OAuth token» con 0 tokens). No reintenta en ningún caso. `--dry-run` no ejecuta el binario (4 y 5 quedan sin ejecutar).
  La lista que lanza es, literalmente: `<%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe resuelto> -p --model claude-opus-5-5 --effort
  xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk
  --permission-prompts none --add-dir D:\r62-arch-a5-run --session-id dcb1b798-b53a-4a31-9693-61cd4a2dfe51 --json-schema <kit/result.schema.json
  compacto, ASCII>`, con stdin = `D:\r62-arch-a5-run\prompt.md` y cwd = `D:\r62-arch-a5`.
- **Selftest previo** (`kit/selftest-result.json`): **129/129 PASS** sobre el clon y el run reales, que quedan limpios e idénticos: 88 casos de
  auditoría (los de la re-revisión de A-4 adaptados al objeto, al cierre, a los tres archivos del run y al clon separado, sin los de
  hallazgos anteriores, y los nuevos 70-85: `D5Disposition` sin determinar en cada campo, sin premisa, COVER_IN_A5 sin REQUIRED,
  OWNER_DECISION_REQUIRED sin decisión del Owner, ids inexistentes, AGREED con COVER_IN_A5 y un CHANGES REQUIRED coherente con COVER_IN_A5;
  `A52Verdict` NOT_ASSESSED, CHANGES REQUIRED sin REQUIRED y BLOCKED sin decisión; `ScopeConfirmation` sin determinar y sin confirmar;
  líneas del Owner antes del acuerdo; AGREED con las dos excluidas; y tres positivos: AGREED con A5-2 excluida, AGREED con A5-1
  excluida y un BLOCKED coherente) y 41 de la compuerta (los de A-4, sin cambio). El caso 25 acepta premisas de `order.txt` y
  `delta.diff` (ruta absoluta y nombre suelto); el 15 rechaza una premisa con la línea desplazada en uno; el 03 crea y retira una unión
  temporal en el clon. Corrido con prioridad por debajo de lo normal.
- **Mutación** (`kit/mutation-post-review.py` → `kit/mutation-result.json`): cada sitio de regla de `post-review.py` (101) y de
  `transport_gate.py` (36) se desactiva por turno en una copia y se ejecutan los 129 casos: **137/137 mutantes muertos**, con la base
  sin mutar en 129/129. Corrida sobre los archivos finales del kit (`post-review.py` 40575d05…, `transport_gate.py` b05fedd1…), con
  prioridad por debajo de lo normal, 07:47:42Z-08:44:38Z (unos 57 minutos).
- **Fidelidad previa** (`kit/preflight-read-fidelity.json`): 3 lecturas en el clon, sin filtro, las tres fieles: A-5 entera, `preflight.md` entero y
  decisiones 1257-1477 (§63-§72). Salen de la transcripción del subagente que preparó el kit; su SHA-256 es el de la instantánea medida.
- **Clon y run** (`kit/clone-verification.json`, generado por `kit/verify_clone.py`), todo comprobado: HEAD = `b631c46f`, separado, sin ramas locales
  ni remotos, limpio con ignorados, ninguna etiqueta ni commit fuera de la historia de HEAD (1 877); los 54 blobs del cierre y el del registro de
  caracterización; la historia del objeto (ausente en `efcfcbc5`; `ad94ceed` en `bf2cf857`, `1a0db7b0` y `b631c46f`; guardas, autoprueba y preflight
  iguales en `bf2cf857` y en el commit; resultado `run` en `1a0db7b0`); `b631c46f`, `1a0db7b0`, `bf2cf857`, `efcfcbc5`, `bb0d5522` y `d2d74c14`
  presentes, `4c617e82` y `b64a3b64` ausentes; 0 enlaces y 0 entradas 120000; `core.autocrlf` = false; run con los tres archivos, idénticos al kit y
  al cierre, `order.txt` con el SHA-256 y los bytes que declara decisiones §72 y `delta.diff` igual a la salida de git; el registro de caracterización
  del clon con el SHA-256 de la compuerta; sin directorio de proyecto `D--r62-arch-a5*` ni transcripción previa. Preflight de las guardas, con
  `--repo` = el worktree (solo lectura de git; G1 necesita el commit del Freeze, que el clon no tiene): `a5-guards.py run --base efcfcbc5 --head
  bf2cf857` = PASS con una salida idéntica entera a `a5-guards-result-run.json` custodiado; `a5-guards.py self-test` = PASS (48/48 mutantes), igual a
  `a5-selftest.json` custodiado salvo la clave `Repo` (la HEAD registrada).
- **Preflight en seco** (`launch-dry-run.json`, junto a este README): `launch.py --dry-run` sobre el kit entregado: ninguna comprobación bloqueante;
  1-3 y 6-10 en Ok; la 4 (`--version`) y la 5 (`auth status`) no se ejecutan en seco, porque quien prepara el kit no ejecuta el binario.
  La línea de órdenes mide 16 968 caracteres. Código 2, como siempre en seco.
- **Manifiesto:** `kit/kit-manifest.json` (20 archivos del kit y los tres del run; SHA-256 `60a9c2a44ce902781485fe37a33e2278c11c9b2e8a3edddc01678e8ff5d8b107`).

El cierre, las herramientas y el auditor no se amplían ni se corrigen después de ejecutar.

## Desviaciones frente al kit de la segunda re-revisión de A-4 (R20261009T195811Z-2df6) y a la orden, con su motivo

- **Clon con HEAD separado**, por instrucción de la sesión principal (el de A-4 tenía `main` en el recibo): `launch.py` (comprobación 6),
  `verify_clone.py` y el auditor exigen HEAD = el commit, separado, sin ramas locales ni remotos. El commit es el de decisiones §72 (`b631c46f`); el
  objeto tiene el mismo blob desde su publicación.
- **Sin paquete del Architect ni hallazgos anteriores.** A-5 no tiene paquete; el cierre lo fijan el objeto (lo que cita con blob o con número de
  línea), la orden §72 y la lista de la sesión principal. Se retiran `PriorFindingDispositions`, `DeltaConfirmation` y sus reglas; se añaden
  `D5Disposition` (punto 6 de §72: «decisión sobre D.5») y `ScopeConfirmation` (punto 1 de §72).
- **Tres archivos del run** (`order.txt`, `delta.diff` y `prompt.md`): no hay órdenes de revisiones anteriores ni versiones anteriores del objeto.
- **16.14 en el primer tema.** La orden dice «conformidad de A5-1 con AP 16.13»; la sesión principal pide 16.13 (Evaluate/Resolve/Validate,
  MV-1..MV-7) y 16.14 (punto efectivo, único trailer), que A5-1 también invoca (reglas 2-4). El tema lleva las dos.
- **Insumos añadidos a la lista mínima:** LIFECYCLE §3/§5/§6/§8 (materialidad, OWNER-RESERVED y veredicto), A-1 y A-3 (cabeceras, para
  `NoChangeConfirmation`), el Freeze, `clause_map.py` y `mv_check.py` (los comprobadores que sostienen G4 y MV-1..MV-7), la conciliación de U-14 y el
  borrador de B1 (citados por A-5 como origen), decisiones §46, §48 y §51 (OD-5/OD-7, repositorio público y C-22 PASS, citados por A-5 y el
  preflight) y todo `I-62-A5/` (incluidos los scripts y las salidas `.txt`, para que un Glob sobre el directorio no salga del cierre).
- **Guardas preflight sobre el worktree, solo lectura**, como en el kit de A-4: G1 compara blobs en el commit del Freeze, ausente del clon.
- **`transport_gate.py` y `c3_analysis.py` sin tocar** (byte a byte los de los kits de A-4): se conservan para que sus predicados sean los ya
  probados.
- **Unión del caso 03 del selftest** hacia el clon de la segunda re-revisión de A-4 (`D:\r62-arch-a4r2`, solo como destino de lectura).
- **Tope de 3 600 s**, igual que en los kits de A-4 (las revisiones de A-4 duraron unos 11-21 minutos con cierres de 2,2 MB; este pesa 1,9 MB). La
  espera de 120 s queda fuera del tope.
- **Sin cambios** frente al kit de A-4: la compuerta y sus predicados, la plantilla de argumentos, el binario, la mitigación de 120 s y la
  clasificación TRANSPORT_AUTH_REFRESH_CONFLICT, el analizador C3, el lector de fidelidad y las reglas de transporte, lectura y custodia del auditor.

## Supuestos no medidos (declarados antes del lanzamiento)

- La duración frente al tope de 3 600 s (estimada por las revisiones de A-4).
- La compactación automática del contexto (no observada en C1, C2 ni en las corridas anteriores); el prompt avisa.
- Que la espera de 120 s evite el conflicto de renovación del token del intento 1 de A-3.

## Antes del lanzamiento (procedimiento de la sesión principal)

1. Autoridad: decisiones §72, punto 2 (revisión formal por `claude-cli` si la celda y la autorización siguen vigentes); consumo cubierto por
   CLAUDE-CLI-I62 = A. La decisión es de la sesión principal; el kit no la presume. §72, punto 5: la revisión no debe consumir recursos que impidan
   tareas críticas de I-64.
2. Opcional: `python -B kit/transport_gate.py check` (código 0) y `python -B kit/launch.py --dry-run` (todo en Ok salvo la 4 y la 5).
3. Custodiar el kit (commit de custodia) y lanzar una vez: `python -B kit/launch.py`. Si cambia un archivo del kit, `python -B kit/make_manifest.py` y
   volver a custodiar.

## Al terminar la corrida (procedimiento)

- No hay reintento. Si `launch.py` se niega a lanzar, no se ha consumido nada.
- Integridad antes de auditar: `kit/` frente a `kit/kit-manifest.json` y los tres archivos del run frente a `RunFiles`.
- Auditor:
  `python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a5\dcb1b798-b53a-4a31-9693-61cd4a2dfe51.jsonl" D:\r62-arch-a5-run\launch\stdout.jsonl D:\r62-arch-a5 D:\r62-arch-a5-run kit kit/result.schema.json audit.json output.json`.
- `output.json`: el `structured_output` del `result`, en UTF-8 con sangría 1. `audit.json`: la salida literal del auditor v5.1-a5.
- `runtime-evidence.json`: de `launch/`, `run.json`, `preflight.json`, la copia de la compuerta y los SHA-256 de `stdout.jsonl` y `stderr.txt`; la
  transcripción (ruta y SHA-256, no versionada; sanear antes de versionar cualquier copia); el clon después; la integridad del kit y del run.
- A-5 no se modifica después de lanzar la revisión y no se declara AGREED sin la revisión acreditada y el veredicto posterior del Coordinator.
