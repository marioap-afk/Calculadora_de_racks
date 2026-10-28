# I-62 — Borrador de la disposición única previa a la medición de A6-7 (decisiones §100, punto 3)

```text
Tipo:        borrador preparado por la supervisión; NO decide, NO se aplica y NO autoriza nada. Lo emite, si quiere, el Coordinator
             como UNA sección de decisiones posterior a §100 (la puerta del kit busca exactamente una).
Autoridad:   A-6 AGREED (blob 4bc919dd, §90), A-7 AGREED (blob ebb3a87b, §100), A-5 AGREED (§73); decisiones §100, puntos 2-5.
Kit:         kits/FX-02/a6m7-measure-v2/ (FREEZE fc355540…; actualización incremental del kit 459a3440, que no cambia).
Fuentes:     s92/readings/proposals.md (lecturas y FC), s92/contract/disposition-draft.md (D-1), s91/risk-matrix.md, I-62-A7/r4/modalities.md.
```

## 0. Resumen

- Una sola sección consolida lo que §100, punto 3, enumera. Lleva los marcadores literales que la puerta del kit v2 exige (§1, punto 13) y se
  nombra a sí misma en `M7-LECTURAS`, porque contiene la disposición de A6-5, regla 6 (punto 5).
- No interpreta nada contra A-5, A-6 o A-7. Cinco cuestiones materiales se presentan aisladas (§2), sin enmienda.
- Aunque se emita, no se puede medir sin las dos líneas del Owner (§3), y sin que la app de escritorio de Codex esté cerrada.

## 1. Texto propuesto

> «**Disposición del Coordinator: medición de A6-7 del Controller de FX-02 con el instrumento B-min (disposición única previa a la medición).**
> Solo para la invocación de medición de A6-7 (A-6, A6-7, reglas 1-7) del EXECUTION_CONTROLLER de la Topología A (FX-02) en F6, y para lo que
> esta sección fija expresamente para las invocaciones VERIFY posteriores. Sin cambiar ningún texto de A-5, A-6, A-7, AUTOMATION_PLAN ni RAE:
>
> 1. **Autoridad.** A-6 AGREED (§90) y A-7 AGREED (§100): el cierre efectivo de insumos de la medición es el de A7-1 con `Invocation.Kind`
>    MEASUREMENT (sin hechos del relevo y sin registro de rebase; `MainSha_eval` = `K.MainSha`). La medición no es una invocación de rol: no
>    produce ninguna verificación, ni VERIFIED, ni C-24 PASS (A6-7, regla 2). Su tope es uno para FX-02 en F6, sin reintento (reglas 3 y 4).
> 2. **Celda, trío, instrumento, intérprete, Git y `-C`** (A6-7, regla 3): las líneas `M7-CELDA`, `M7-TRIO`, `M7-INSTRUMENTO`,
>    `M7-INTERPRETE`, `M7-GIT` y `M7-C` del punto 13. El trío es el vigente, observado pasivamente el 2026-10-28 (§100, punto 5); el del
>    2026-10-16 ya no rige.
> 3. **Cierre efectivo y cobertura de las lecturas legítimas** (A6-7, regla 7; A7-1). El cierre es el de `M7-CIERRE`: designación
>    `a7-designation/4` MEASUREMENT, E de A7-1 (16 entradas), cierre derivado por la tabla de A6-9 (A6-3, regla 6 (b)), con la única omisión
>    `C1.D.ChainRedSha FIELD_NULL`, y aceptado por el validador de A7-1. Cubre todas las lecturas que hacen las 13 ejecuciones del instrumento:
>    - `identity`, `diff`, `scope` y `hashes`, en `BaseSha`, `RedSha` y `CurrentSha`;
>    - `clausemap`, con `--main` = `K.MainSha`, que está en C1;
>    - las lecturas `--file` de CONTRACT, DELEG, H y el TRX de `CurrentSha`;
>    - `read-text` y `read-json`, en `CurrentSha`.
>
>    La comprobación de cobertura de A-7 §3.5 sobre ese cierre da LAUNCH_MAY_PROCEED, sin ninguna lectura fuera de E. La misma regla rige la
>    disposición que autorice cada invocación VERIFY posterior, cada una con su propio cierre de A7-1.
> 4. **Origen e identidad de las entradas** (A7-1, regla 2 (c)):
>    - **CONTRACT:** `docs/automation/decisions/FX-U1-T1.gate-contract.json`, blob `628d89af…`, en el árbol de `CurrentSha`.
>    - **DELEG:** la copia custodiada de la delegación, con `ExpectedWorktree` = el `-C`: blob `8358efbf0fa00b6deb0edd03997b8f801c29b1e2`, en su ruta KPR-2 bajo
>      `R20261012T090000Z-d001`.
>    - **H:** la entrega, con `Worktree` = el `-C`: blob `b56992662d942b122cc365e5176b7a7daa1fd6d1`, bajo `R20261012T091000Z-a001`.
>    - **TRX de `CurrentSha`:** el de la corrida de CI R37949808974, blob `40295387…` (con BOM). La designación lo identifica por esa ruta, y
>      su copia está en la ruta ignorada `<RunPath>ci-current/`.
>    - **Hechos del relevo:** ninguno; la medición no los tiene (A7-1, punto 1 (c); A6-7, regla 1).
>    - **TRX de `RedSha`:** no se designa. La entrega exige RED por 16.8 (`ChainRedSha` nulo), pero no existe un TRX real de una corrida de
>      `RedSha` de FX-U1. El constructor registra la ausencia `NOT_DESIGNATED`, sin sustituir nada y sin ningún archivo sintetizado (§2, A).
>    - **RunId de la medición:** `R20261028T024440Z-27a9`, con la forma de AP 16.6.
> 5. **Lecturas normativas del aceptante (A6-5, regla 6).** Aceptante `bmin_accept.py`, blob `434ef1fc…`. Se adoptan el texto y la tabla de
>    `s92/readings/proposals.md` §5, puntos 1-7:
>    - **lecturas compatibles:** L-02, L-11 y L-14, adoptadas;
>    - **lecturas coincidentes de `readings.md`:** las doce, adoptadas;
>    - **las ocho indeterminadas:** fallo cerrado FC-01, FC-04, FC-10, FC-12, FC-13, FC-16 y FC-24;
>    - **L-18:** limitación de escenario FC-18, con U-19 intacta.
>
>    Las comprobaciones FC se ejecutan sobre cada invocación VERIFY de FX-02 con el instrumento, con el Git declarado y solo en lectura: antes
>    del lanzamiento (si fallan, no se lanza y no cuenta) y tras la Entrada, antes del aceptante (si fallan, la salida no se ingiere). En los
>    dos casos va S-03, con STOP, al Coordinator. El guion de las FC se fija por SHA-256 en el kit de la ronda de B1 antes del primer
>    lanzamiento. **No se aplican a la medición**, en la que R4 y R10 no son aplicables (A6-7, regla 1); las lecturas adoptadas sí rigen para
>    lo que la medición compara en R5-R7. Ni esta sección ni las FC se entregan al Controller (A6-9, regla 2).
> 6. **Exclusión de la orden `contract` (D-1)**, para la medición y para toda invocación VERIFY con el instrumento:
>    - **Orden declarada:** `<git> --no-replace-objects cat-file blob 9a829b0e… | <intérprete> -I - <orden> …`, con `<orden>` exactamente una
>      de las once de `M7-INSTRUMENTO`. Una ejecución con `<orden>` = `contract` es «otra orden» (A6-1, regla 1): no se reconstruye (regla v3),
>      no hay solicitud y la salida queda sin aceptar (A6-3, regla 6 (a); A6-5, reglas 4 y 7 (c)). Nunca es un FAIL del producto (A6-5, regla
>      5). En la medición, la regla 1 no se cumple y no hay reintento.
>    - **`Contract` sigue siendo obligación del Controller** (16.9 #4; A3-A5 de 16.5 y A3'-A5' de 16.22), con U-19 tal como está registrada
>      (§62, punto 3). La verifica leyendo CONTRACT y DELEG con `read-json --file … --sha256 …`, con los ocho punteros (`/AllowedWriteScope`,
>      `/ForbiddenWriteScope`, `/Invariants`, `/StopConditions`, `/Authorities`, `/RequiredTests`, `/RoleRequirements` y
>      `/SupersededCommits`), y cita esas órdenes y su contraste primario (el SHA-256 de los archivos, A6-4, regla 2).
>    - **Controles del aceptante:** R3 y R9 cubren esas lecturas. R7 sigue con `scope` y `clausemap` y deja de comparar A3/A4. La tabla de
>      A6-5, regla 2, no cambia.
>    - **Demostración de que `Contract` sigue verificándose conforme a U-19** (§100, punto 3):
>      - estática, en el banco de s92: con nc2 S1 sin `contract`, la salida se acepta y el oráculo relativo queda intacto; la lectura fiel de
>        los ocho campos dio `ACCEPT_FACTS`; y el recálculo independiente coincide con el Controller en 10 de 11 salidas (la única diferencia
>        es el falso fail de D-1);
>      - en la medición: dos de las 13 ejecuciones son esas lecturas, y el comparador exige sus valores iguales a los bytes custodiados (OP6).
>
>    El recálculo de `Contract` por la supervisión (punto 7 del borrador de s92) no se incluye: es un control nuevo (§2, B).
> 7. **Condiciones de fallo cerrado FC-01, FC-04, FC-10, FC-12, FC-13, FC-16, FC-18 y FC-24**, con el ámbito, los momentos y el efecto del
>    punto 5 y de `s92/readings/proposals.md` §1-§4.
> 8. **Reconstrucción de `Runs`** (A6-3, regla 6 (a); A6-7, regla 7): la regla de `M7-RECONSTRUCCION` (v3) es la v2 del kit anterior más el
>    conjunto declarado de órdenes. La regla de referencia queda en el kit. La disposición que autorice cada invocación VERIFY posterior nombra
>    la misma.
> 9. **Shell, Python y Git efectivos:**
>    - **shell de la invocación:** el pwsh del runtime de Codex (`%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\…\pwsh.exe`,
>      SHA-256 `362a356c…`, bundle 26.1020.11618). Se identifica en la comprobación del punto de uso, (iii) con ese shell y su modo de
>      lenguaje registrado, y en OP1, donde la entrega (C2) y las 13 ejecuciones tienen que tener un único shell, registrado si es el del
>      runtime;
>    - **intérprete:** el de `M7-INTERPRETE`;
>    - **Git:** el de `M7-GIT`; `git` por nombre en el entorno de la invocación tiene que ser el Git declarado (C1; parte RA del registro).
>
>    Cualquier diferencia anula la medición (A6-7, regla 5).
> 10. **Mitigación de O2** (A62-A7R4-O2), sin reducir ninguna comprobación obligatoria:
>     - **en la medición:** `MainSha_eval` = `K.MainSha`, que está en C1, y ninguna orden lee `MainAfterSha` ni imágenes de un rebase, porque
>       no hay reverificación. La comprobación de cobertura del punto 3 lo demuestra antes de medir;
>     - **en las VERIFY posteriores:** antes de cada lanzamiento se repite esa comprobación con el cierre de la invocación (A-7 §3.5). Una lectura
>       fuera del cierre deja la invocación sin lanzar (S-03). Lo que queda para la reverificación y sus controles se trata en §2, C.
> 11. **Presupuesto de D.3:**
>     - **la medición:** se cuenta aparte, con tope 1 y sin reintento (A6-7, reglas 3 y 4);
>     - **la ronda de FX-02 (V14 D.3):** 11 lanzamientos base (planificación, verificación, Architect y los 8 controles con invocación), un
>       fondo de 4 reejecuciones de transporte (como máximo 2 por fase o por control) y un tope de 15, sin reserva para la reverificación;
>     - **la reverificación de 16.7:** suma una invocación («suma al tope de invocaciones lo mismo que una recuperación»). Los 8 controles son
>       los mismos, ejecutados sobre sus entradas (RAE §10, A7-1), no adicionales;
>     - **los rechazos falsos de O2** en la reverificación o en sus controles consumen reejecuciones del fondo (A6-5, regla 4); a la tercera,
>       STOP P-04 o control no superado;
>     - **control:** P-07 comprueba el tope antes de cada lanzamiento. Si el siguiente lo superara, no se lanza y el caso vuelve al
>       Coordinator. Ningún contador se amplía.
> 12. **Escritura de preparación** (A6-3, regla 1): la de `M7-ESCRITURA`, con la DecisionRef `I62-A6-7-M7`, que esta sección adopta como su
>     identificador.
> 13. **Marcadores literales** (la puerta del kit busca cada línea con `grep -F`):
>
> >     - M7-CELDA: codex-cli / gpt-6-luna / high; EXECUTION_CONTROLLER de FX-02 en F6
>     - M7-TRIO: BinaryHash 5c1b55966bf9f757dcbf00065c9a6cb862ead55d34b015f1be1939ff18e43943, AppVersion 26.1023.6123.0, huella 0A9F6A344C965BE17650A2559601DB8768AE8DCFAC6BEB7DEEFC615AE4E7F550
>     - M7-INSTRUMENTO: blob 9a829b0eeb1210238052b1feac06cfa8f7964aed; orden declarada <git> --no-replace-objects cat-file blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | <interprete> -I - <orden> ...; <orden> una de: identity, diff, scope, hashes, cleantree, handoff, freetext, clausemap, trx, read-text, read-json; sin contract
>     - M7-INTERPRETE: %LOCALAPPDATA%\Microsoft\WindowsApps\python.exe; Python 3.13.14; SHA-256 ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec
>     - M7-GIT: %USERPROFILE%\AppData\Local\Programs\Git\cmd\git.exe; git version 2.54.0.windows.1; SHA-256 81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5
>     - M7-C: D:\r62-a6m7\m7-run2; instantanea SHA-256 1d2d4d1d92a3c42b4fc07c14c13e963a30624a90c7ee6f962318348368c104ed
>     - M7-CIERRE: A7-1 (a7-designation/4, Kind MEASUREMENT); designacion SHA-256 a5d45905768febd2bca72fa70436562b52ff69306b15ea67d331f7bbf895afad; E SHA-256 0488409ceec42e54edcbb0b9f68b91eb0128a71b5f8edb99b3cb201d853b1cd5; cierre derivado SHA-256 2c5cedf14b9390eae13102084990063951ec00fc8a2bc70b51f039e668049674; registro de derivacion SHA-256 716463df3ba80e1712d1d879260c3db32932c151d1e62a5122a46c8cdc7d50b6
>     - M7-RECONSTRUCCION: reconstruct_runs_m7.py SHA-256 f5f8f269efdb881bc395a0a7a244d7d9fe00bbce23f56306ae6efc0d2a6af834
>     - M7-ESCRITURA: autorizada la escritura de preparacion de A6-3, regla 1, en D:\r62-a6m7\m7-run2; DecisionRef I62-A6-7-M7
>     - M7-KIT: FREEZE.json SHA-256 fc355540bf66cb10cdcd2783cec00f37778639b9339e7b34d5f698aee7fc2aad
>     - M7-LECTURAS: disposicion de A6-5, regla 6: §<número de esta sección>
> 14. **Pasos dentro de la invocación que esta sección acepta:**
>     - la prueba de entrega exacta (C2) usa el nombre abreviado del objeto (`9a829b0eeb12`), porque toda orden con el identificador completo es
>       una ejecución (A6-3, regla 6 (a));
>     - para `AuthorityResolution`, el comparador acepta una de las dos lecturas documentadas de 16.13 (V5 o R3) y la nombra; la disposición de
>       elegibilidad confirma cuál (§2, E).
> 15. **Límites:**
>     - esta sección no autoriza ningún consumo ni ninguna invocación de Codex: la medición exige además la línea OD-2 y la línea (b) del Owner
>       para el trío vigente, como líneas completas posteriores a esta sección, y que la app de escritorio de Codex esté cerrada;
>     - no declara la celda ELIGIBLE, ni FX-02 PASS, ni F6 GATE PASS;
>     - no cambia el instrumento, el aceptante, U-19, el oráculo de C-24, los presupuestos ni ningún blob fijado.»

## 2. Cuestiones materiales aisladas (no se inicia ninguna enmienda)

| Id | Cuestión | Por qué es material | Opciones (decide el Coordinator) |
|---|---|---|---|
| A | No existe un TRX real de la corrida de `RedSha` de FX-U1, y la entrega exige RED por 16.8 (`ChainRedSha` nulo). | A7-1 admite el TRX de `RedSha` solo como artefacto custodiado; sintetizarlo sería inventar una entrada. El preflight de A-7 usó uno sintético solo como prueba estática de la regla. | (1) medir con la ausencia registrada (`NOT_DESIGNATED`), que es lo que prepara el kit: la medición no clasifica `Ci`/`Tests` porque no produce verificación; (2) obtener un TRX real ejecutando las pruebas del fixture en `RedSha` (corrida nueva de CI o local), con su custodia; (3) otra delegación del banco sin RED exigible (cambia la designación y el escenario). |
| B | Recálculo de `Contract` por la supervisión (punto 7 del borrador de s92). | Ninguna cláusula define ese control. Como motivo para rechazar un VERIFIED, cambia qué falla. | Mantenerlo fuera, como hace este borrador. Si se quiere, es una enmienda, no una disposición. |
| C | O2 en la reverificación de 16.7 y en sus controles: `clausemap --main` lee en `MainAfterSha`, fuera de C1 de A6-9, y el aceptante da R9 MISMATCH. | Restringir esa orden dejaría al Controller sin los hechos de `Authority` (16.13), que es una comprobación obligatoria. Aceptar el residuo consume reejecuciones de D.3 (punto 11). | (1) aceptar el residuo con el control de D.3 y STOP P-04, como hoy; (2) decidir antes de la ronda de B1 si una reverificación con instrumento se lanza solo cuando el fondo lo permite; (3) cualquier ampliación de C1 es una A-n. No afecta a la medición. |
| D | Codex se actualizó otra vez (26.1023.6123.0, binario `5c1b5596…`, huella `0A9F6A34…`). | El trío del kit v1 ya no rige (A6-7, regla 5). OD-2 y la línea (b) son nuevas. La versión no está medida: siguen vigentes las incertidumbres U1-U6 y U8 de la matriz de riesgos de s91. | (1) medición directa; (2) antes, una sonda de caracterización del bloque A2-P2 de esta actualización (s91, opción B: otra disposición y otra línea del Owner). §100 no autoriza ninguna de las dos. |
| E | Lectura de `AuthorityResolution` (Mode/RevisionSha) en las filas UNIT_*: V5 frente a R3. | El comparador acepta una de las dos y la nombra. | Confirmarla en la disposición de elegibilidad, o fijarla ahora. |

## 3. Líneas del Owner imprescindibles (texto exacto; preparadas, NO autorizadas)

Las dos, como líneas completas del archivo de decisiones, después de la disposición. La puerta exige que la línea (b) nombre el trío en orden, el
blob y la celda.

```text
OD-2 = A (línea base 0A9F6A344C965BE17650A2559601DB8768AE8DCFAC6BEB7DEEFC615AE4E7F550; binario 5c1b55966bf9f757dcbf00065c9a6cb862ead55d34b015f1be1939ff18e43943)
```

```text
A6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación de medición read-only de codex-cli, celda gpt-6-luna/high del EXECUTION_CONTROLLER de FX-02 en F6, con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete que nombre la disposición, para el trío exacto 5c1b55966bf9f757dcbf00065c9a6cb862ead55d34b015f1be1939ff18e43943/26.1023.6123.0/0A9F6A344C965BE17650A2559601DB8768AE8DCFAC6BEB7DEEFC615AE4E7F550, con huella, binario y versión antes y después; fuera de A2-P2, sin reutilizar su bloque agotado ni la invocación consumida de A4-1; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; solo con A-6 AGREED, con A6-ADR0046-9-FX02 = A y con la disposición del Coordinator que la nombre; distinta de A6-ADR0046-9-FX02 y no implícita en ella; no acepta su resultado ni amplía OD-2 ni OD-5)
```

## 4. Mitigación comprobable de D-1 y de O2

- **D-1:**
  - la regla v3 rechaza cualquier registro con una ejecución de `contract` (caso `V3-CONTRACT-NOT-RECONSTRUCTIBLE` del autotest);
  - el texto armado no contiene `contract` y tiene 13 ejecuciones del instrumento;
  - el comparador exige las dos lecturas `read-json --file` iguales a los bytes custodiados;
  - el esquema ya no tiene los campos de `contract`.
- **O2:** comprobación de cobertura de A-7 §3.5 sobre el cierre de la medición: LAUNCH_MAY_PROCEED (`s100/kit-v2/coverage.json`).

## 5. Cobertura y riesgos del cierre de VERIFY

- **Medición:**
  - cierre A7-1 aceptado por el validador, sin entradas ajenas ni faltantes;
  - ausencias registradas: `TRX_RED` (`NOT_DESIGNATED`) y `PROMPT_TEMPLATES.md` en EFF y EFF^1 (16.13 MV-6, propia del fixture, igual que
    en el kit v1);
  - E idéntica en sus entradas canónicas a la E R7-A del kit v1 (`137c7c3b…` con el `-C` anterior).
- **VERIFY posteriores:** las modalidades de `I-62-A7/r4/modalities.md` (M1-M10). El residuo O1/O2 (§2, C) y la segunda recuperación (sin
  cierre, M7) siguen como se declararon.
- **Riesgos sin demostrar:**
  - la forma en que 26.1023 registra las órdenes;
  - el shell real y su modo de lenguaje;
  - el arranque del alias de Python dentro del sandbox;
  - `git` por nombre en el sandbox;
  - el truncado de sobres (clausemap ≈ 44 KB);
  - el tope de 600 s;
  - la fidelidad del modelo al texto literal;
  - el modelo y el effort efectivos.
