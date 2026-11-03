**Informe del Architect: revisión final de conformidad de F5 sobre el Candidato `bd1fcf8a`**

Rol: Architect de I-65, SAME-SESSION ROLE, solo lectura. HEAD revisado: `a8657ff2`. Candidato: `bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109`.

**pwd (primera acción):** `/d/Documentos/Codex/Calculadora de racks`. Es el directorio de proceso del checkout principal; no leí nada en él. Todo lo leí con `git -C <worktree>` o con rutas absolutas del worktree `%USERPROFILE%\.codex\worktrees\architecture-interop-analisis-estructural`. Este informe no copia rutas con nombre de usuario.

**Operaciones:** no escribí ni borré nada. No hice commit, push, fetch, checkout ni stash. No ejecuté intérpretes, AutoCAD ni RAM. Solo usé `git` de lectura (`show`, `diff`, `diff --no-index`, `log`, `rev-parse`, `ls-tree`, `ls-files --eol`, `cat-file`), `sha256sum`, `grep`, `sed`, `cut`, `wc` y `ls`, y miré las capturas. Desviaciones menores: hice `cd` al worktree para dos `git diff --no-index` de lectura y un `cd /tmp` sin leer ni escribir allí. No leí `D:/r65-smoke` ni `D:/r65-m05c`.

---

## Veredicto: **CONFORMANT WITH OBSERVATIONS**

- **0 REQUIRED** y 8 OPTIONAL nuevos (A65-F5F-01..08).
- No hay ningún FAIL ni INCOMPLETE en los doce puntos.
- AC-F5-01..08 CUMPLEN. AC-F5-09 y AC-F5-10 quedan **pendientes por construcción**.
- OV-I65-02 = **PASS**.
- No declaro GATE PASS: esa determinación es del Coordinator.

---

## Respuestas 1-15

**1. Freeze y ADR-0050 — PASS.**
- Freeze: blob `4e6e1feb…` igual en `bd1fcf8a`, en HEAD, en `1e7d8fc2` y en `34da5c3e`.
- ADR-0050: blob `c18def42…` igual en los mismos cuatro commits.
- No hay ninguna A-n, delta ni EXP-01: no existe ningún archivo de enmienda de I-65, y §85-§96 solo mencionan una «probable A-n» hipotética para la opción 1, que no se autorizó.

**2. AC-F5-01..10:** ver la matriz más abajo.

**3. Integridad del Candidato y de `pkg-OV02` — PASS.**
- **DLL** `81299b87…fef5` con ProductVersion `1.0.0+bd1fcf8a…`:
  - aparece en `identity.txt`, en la identidad de W3 (BASELINE, antes de NETLOAD y POST) y en las dos listas de módulos de W3;
  - en el POST de W4 (`P-09`) sigue igual;
  - Application, Domain y UI en la lista de módulos son iguales a `identity.txt`.
- **Tabla:** la medí yo: `git show bd1fcf8a:assets/…/section-material-map.json` da SHA-256 `fc3d9146…`, 3279 bytes, blob `d088e2a9`, igual en HEAD.
- **Archivo de cargas:** SHA-256 `d675d9f6…`, blob `dcc1a039`, igual en `bd1fcf8a` y en HEAD.
- **Paquete:** `manifest.json` `f6cf8a91…`, `rackcad-ram-build.py` `f7ed0b06…` y `structural-loads.json` `d675d9f6…` son iguales en todas las mediciones:

  | Momento | Archivo de evidencia |
  |---|---|
  | Creación en W3 | `package-and-originals-sha256.txt`, validador V-05/V-06 |
  | Preparación de W4 | `w4-prep-summary.txt` |
  | Preflight original 02:32Z | `w4-pre` S-07 |
  | Preflight nuevo 12:04Z | `w4b-pre` S-07 |
  | POST 12:31Z | P-07 |

  En el POST hay 4 archivos: el único nuevo es `ram-readback.json` `e7481e04…`, de las 12:06:37Z. Los tres originales conservan la fecha de W3 (18:00:05Z). Cada carpeta de volcado tiene solo su script y su `ram-dump.json`.
- **Scripts de volcado:**
  - Las copias versionadas tienen SHA-256 `4a1be499…` (gen) y `bdd02eee…` (man), los mismos que registra el `pre-run` de la custodia.
  - Mi `git diff --no-index` contra la plantilla de P2 (`manual-rackcad-ram-dump.py.txt`, `af262489…`) da **una sola línea distinta, la 11**: la carga útil con `dumpFolder`.
- **Custodia:** verifiqué 109 entradas de las tablas de evidencia §92-§100 contra los blobs. Hay tres desajustes:
  - el anexo operativo: correcto, son dos versiones registradas en su momento;
  - `host-coordination-w3…md`: A65-F5F-01;
  - `w4-prep-summary.txt`: A65-F5F-02.

  Las copias versionadas de lectura de vuelta, volcados e informe del comparador coinciden byte a byte con lo custodiado.

**4. Resultados de W3 y W4 frente a sus oráculos — PASS.**
- **W3:** `Valid` con 3 archivos escritos (captura 10). Validador `PASS` 16/16 (V-01..V-16 en el JSON). Seis cargas iguales a la tabla (6/6). Q2 = 99.99999999999997 %. La carga útil es igual a la de la comprobación en seco salvo `packageFolder` y la huella. DIFF solo INFORMAR: cambian únicamente `hkcu-profiles` y `hkcu-fixedprofile`. K1/K2 sin hallazgos.
- **W4, lectura de vuelta (`ram-readback.json`):**
  - `READBACK OK`, `differences` vacío, huella `8f36e3ce…` igual al manifiesto, 8 nudos, 7 miembros y 6 cargas;
  - valores idénticos a la carga útil de W3 (por ejemplo Q2 −337.2134146495658 al 99.99999999999997 %).
- **W4, volcados:** los dos dan `DUMP OK` con `errors` vacío.
- **W4, comparador:** `PASS`, salida 0. Las entradas del informe coinciden con la custodia: manifiesto `f6cf8a91`, lectura de vuelta `e7481e04`, volcado generado `e5cf0ead`, volcado manual `12c89479`.

**5. Seis cargas, recalculadas a mano — PASS.**

Reglas usadas: 1/4.4482216152605 = 0.2248089431 lbf/N; distancia = 6.354218840504721 + f × 36 in; L = 42.35421884050472 in = 6.3542… + 36, así que la fracción 1 cae exactamente en la punta.

| Carga | Magnitud recalculada | Posición recalculada | Valor en la carga útil / lectura de vuelta / volcado generado | Volcado manual | Miembro (gen / man) |
|---|---|---|---|---|---|
| Q1 | 2000 N × 0.2248089431 = 449.6178862 lbf | 6.354218840504721 in → 15.00256412 % | −449.617886199421 al 15.002564123383088 % | −449.617886 al 15.002564 % | 1 / 5 |
| Q2 | 1500 N → 337.2134146 lbf | 42.354218840504721 in = L → 100 % | −337.2134146495658 al 99.99999999999997 % | −337.213415 al 100.0 % | 1 / 5 |
| Q3 | 750 lbf | 15.354218840504721 in → 36.2519230925 % | −750 | −750 | 2 / 6 |
| Q5 | 300 kgf × 9.80665 = 2941.995 N → 661.3867866 lbf | 24.354218840504721 in → 57.5012820617 % | −661.3867865546327 | −661.386787 | 3 / 7 |

- Q4 (1000 lbf al 91.5002564 %) y Q6 (4500 N = 1011.6402439 lbf al 78.7506410 %) también coinciden.
- **Dirección:** `-Z` en el archivo pasa a `GLOBAL_Y` con valor negativo (D-04: Y de RAM = Z). Es una sola inversión de signo (anexo §2). La lectura de vuelta y los dos volcados dicen `Y` con valores negativos.
- **Unidades:** carga útil y volcado generado en `LB_IN`. El manual empezó en `KIP_FT`, se leyó en `LB_IN` y se restauró (`unitsRestored` true), conforme a D-11.
- INFORMATIVE: §15.10 del Freeze recomienda construir la referencia manual en lb-in. Se construyó en ft/kip, igual que en P2; el contrato de unidades de D-11 cubre ese cambio porque el modelo no estaba analizado.

**6. Q2 en el límite de fracción 1.0 — PASS, coherente.**
- 99.99999999999997 % es 100 − 2 ulp: 2.8e-14 % de diferencia, unos 1.2e-14 in. Eso es unas 1.6e12 veces menor que la tolerancia de 0.02 in.
- RAM conserva el valor emitido sin recortarlo ni rechazarlo (lectura de vuelta y volcado generado), y acepta el 100 % en el manual (captura 12 y volcado).
- El algoritmo no cambió (`src`, `tests` y `tools` iguales a `51fc82f1`) y las tolerancias del informe del comparador son las del Freeze §4.
- Todas las lecturas la presentan como diferencia de representación: W3-07, W3 §1, W4 §1, CD-I65-F5-08 y §96.
- Matiz: en `payload-vs-table` el valor aparece como «100.000000000000» a 12 decimales, pero la línea siguiente da el valor exacto (`repr`).

**7. Independencia del modelo manual — PASS (independencia por fuente).**
- **Fuente:** la hoja `ov-i65-02-manual-entry-sheet.json`, blob `6dc0d8da`, igual en `bd1fcf8a` y en HEAD.
- **Firma numérica de la hoja en el volcado manual** (es el indicio más fuerte):
  - coordenadas = cadenas de la hoja en ft con 8 decimales × 12 (por ejemplo 0.02083333 × 12 = 0.24999996; −3.0188415 × 12 = −36.226098);
  - cargas = cadenas en kip con 9 decimales × 1000;
  - porcentajes = cadenas con 6 decimales;
  - numeración propia de la hoja (miembros 1-4 columnas y 5-7 brazos), descripciones vacías y categoría `DL`.

  Si se hubiera copiado del generado se verían dobles de precisión completa, la numeración del generado (brazos 1-3) y su categoría `OTHER`.
- **`manual-dump-vs-sheet.txt`:** recorre los 8 nudos, los 7 miembros y las 6 cargas, y compara contra la hoja, no contra el generado.
  - Coordenadas: Δ máx. 5.69e-8 in, dentro de la cota de redondeo de 6e-8 in.
  - Cargas: Δ relativo máx. 1.04e-9 (la salida dice «1.0e-09»), dentro de la cota de 1.5e-9.
  - El script compara valores con signo, así que también comprueba el signo. Es suficiente.
- **Cronología:** el generado se descartó a las 12:08:17Z, el manual se construyó entre 12:08 y 12:28 y su volcado es de las 12:29:01Z.
- **Límite:** el mismo agente construyó ambos y vio el contraste IGUAL antes de empezar el manual (A65-F5F-04 y A65-F5F-08). No hay indicio de copia.

**8. CMB-3 y peso propio — PASS.**
- Ambos volcados tienen `combinations` vacío.
- El código del comparador (`StructuralReadbackComparison.Combinations()`) marca la fila NOT_EVALUATED y añade una decisión de combinaciones cuando se usa `--*-combinations-unread`. El informe da `COMBINATIONS` PASS y no lleva decisión, así que la ejecución sin esas opciones queda probada por el propio informe.
- Peso propio (0, 0, 0) en el generado y en el manual: INFORMATIVE.
- Las filas clasificadas están en el apartado OV-I65-02.

**9. Trazabilidad de sesión (opción 0) — PASS como evidencia operativa.**
- `session-custody-w4.jsonl` tiene `session-start`, tres pares `pre-run`/`post-run` y `session-end` (`ramCount` 0), todos con el PID 11464.
- En cada `pre-run`, `outputExistsBefore` es false.
- Cadena de tiempos: inicio de RAM 02:32:51Z ≤ lectura de vuelta 12:06:37Z ≤ volcado generado 12:07:30Z ≤ volcado manual 12:29:01Z ≤ cierre 12:30:07Z / 12:30:20Z. Es coherente.
- El contraste de contenido da IGUAL (8/7/6).
- Está declarada como **no criptográfica** en CD-I65-F5-06/11, en la preparación §7 y en W4-10. El informe de W4 no repite esa advertencia.
- Limitaciones que pasan a READY:
  - dos sesiones con la misma geometría producen el mismo contenido;
  - los tiempos de archivo se pueden alterar;
  - el volcado no lleva nonce (A65-F4F-04, decisión pendiente para READY);
  - el registro lo escribe el propio operador;
  - el proceso de RAM venía de la ventana vencida (A65-F5F-05).

**10. Incidencias, restricciones y STOP — PASS. Ninguna afecta a la validez.**
- **Licencia:** el STOP del operador se respetó (sin modelo ni scripts). El operador no tocó el licenciamiento: el acceso a CONNECTION Client le fue denegado. Se reanudó por orden del Owner en una ventana nueva con preflight propio. La aceptó el Owner como incidencia externa (CD-I65-F5-52). Matiz en A65-F5F-05.
- **Lote de materiales rehecho:** la captura 08 y el volcado muestran los materiales correctos.
- **Escape:** solo quitó la selección. Al reseleccionar con «Todo» salen 15 elementos (8 nudos + 7 miembros); no cambió ningún dato.
- **Interrupción del usuario (~10 min):** sin acciones del operador.
- **Diálogo «Ejecutar archivo…» en la carpeta del generado:** el `pre-run` del manual registra `bdd02eee`, y P-07 muestra 2 archivos por carpeta de volcado. No se ejecutó nada desde ahí.
- **Fecha de la carpeta de la DLL (02:30:01Z):** la causa no está determinada. La DLL sigue íntegra y W4 no la usa. INFORMATIVE.
- **`Config.ini` `505a3592…` → `e51d5ae3…`:** el diff solo muestra contadores, `accRequireSignIn=2` (inicio de sesión del Owner) y `PythonScripts`. Es estado de interfaz, conforme a CD-I65-F4-73.
- **K1/K2:** W3 sin hallazgos. En W4, OWN: dos originales sin versionar, que se versionaron después en `2824dcef`. SIGNAL: el HEAD de I-62 avanzó, limpio. Sin STOP.
- **Presupuesto:**
  - W3: unos 10 min, 1 NETLOAD, 1 generación, 0 reintentos.
  - W4: 1 construcción y 2 volcados, como mucho 1 corrección controlada, 0 reintentos, unos 27 min de las 2.5 h, sin modelos guardados y sin análisis.
  - Codex 50/52, `attempts` 7/9.
- **Doble arranque de AutoCAD en W3:** la segunda instancia se cerró antes de cualquier NETLOAD (CD-I65-F5-37) y el Owner aceptó la incidencia.
- Además, W4-PR-04 no se cumplió literalmente (ram=1) y el contraste se ejecutó antes del cierre: A65-F5F-04 y A65-F5F-05.

**11. Hallazgos previos:** ver la tabla de estado más abajo. No queda ningún REQUIRED abierto.

**12. Ausencia de cambios funcionales — PASS.**
- `git diff --name-only bd1fcf8a HEAD` fuera de `docs/` está vacío.
- `src` `6cadd526`, `tests` `47fbcaf5`, `tools` `29485f42`, `assets` `d1cc1811`, `Directory.Build.props` `69223076`, `Directory.Build.targets` `58d1c335`, `global.json` `eaa68c65` y `RackCad.sln` `6483b50d` son iguales en `51fc82f1`, en `bd1fcf8a` y en HEAD.
- Procedimientos, preparación, alcance, hoja, archivo de cargas y tabla siguen sin cambios desde el Candidato. Lo único nuevo en `docs/initiatives` es el anexo operativo, permitido por CD-I65-F5-21.

**13. LIFECYCLE §7 para el cierre de F5:**

| Punto | Estado | Evidencia |
|---|---|---|
| 1. Resultado observado por pruebas o por un hito del Owner | CUMPLE | W3 ACCEPTED (CD-I65-F5-43), W4 ACCEPTED (CD-I65-F5-52) |
| 2. RED→GREEN focal | NO APLICA | 0 cambios de comportamiento o bugfix (punto 12) |
| 3. Pruebas y evidencia en verde | CUMPLE para el Candidato; PENDIENTE POR CONSTRUCCIÓN para el SHA de cierre | Core 14390/14390, UI 1637 + 17 omitidas, builds y CI `39187567306` 4/4 con `head_sha` exacto (`ci-run.txt`); W3 y W4 PASS |
| 4. CI del SHA de cierre leído y en verde | PENDIENTE POR CONSTRUCCIÓN | — |
| 5. Revisión del Coordinator (diff contra Freeze y A-n, hallazgos, desviaciones, sin EXP-01 A) | PENDIENTE (acto del Coordinator) | Insumos verificados aquí: Freeze y ADR intactos, sin A-n ni EXP-01, 0 REQUIRED |
| 6. Orden commit → árbol limpio → evidencia local → push → CI | PENDIENTE POR CONSTRUCCIÓN | — |
| 7. Rebase entre gates | CUMPLE con reserva | `origin/main` local = `bb0d5522` = merge-base, 0 detrás; no lo verifiqué con fetch (prohibido). El Coordinator debe hacer un fetch fresco; si `main` avanzó, aplicar WORKFLOW |

- **SHA documental de cierre de F5**, según el precedente de F4 (CD-I65-F4-219/221): el **commit futuro** que versione este informe, la conformidad de cierre del Coordinator, la evaluación de LIFECYCLE §7 y de AC-F5 y el estado.
  - No es `bd1fcf8a` (Candidato) ni `a8657ff2`.
  - El GATE PASS se registra en un commit **posterior**, como hizo `34da5c3e` en F4.
- **Evidencia que exige ese SHA**, en este orden:
  1. árbol limpio (HEAD = ese SHA y `status --porcelain` vacío, antes y después);
  2. Core Full local sobre ese HEAD exacto, con SDK 8.0.423 del dotnet del usuario y sin versionar el TRX;
  3. push de la punta;
  4. CI con `event=push`, `ref=refs/heads/architecture/interop-analisis-estructural`, `head_sha` exacto y 4/4 jobs en `success` (Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI, Build Plugin without AutoCAD), leído antes del siguiente gate.
- UI Full local no es obligatoria en un cierre de gate (LC-UI), como en F4.

**14. Privacidad (§90) — PASS.**
- Las capturas de W3 (11) y de W4 (19) están recortadas a la zona técnica. Las revisé visualmente: no hay barras de título con cuenta, ni barra de tareas, ni nombres de personas.
- En 01a, 02a y 13a el panel de navegación está tapado. Se ve parcialmente un **rótulo genérico de carpeta** bajo el recuadro; no es identificable.
- `manifest-pkg-OV02.redacted.json` y `validator-v2-pkg-OV02.redacted.json` tienen redactado el nombre del verificador, también dentro de `canonicalText`. Los originales quedan en local con su SHA-256.
- `k1-k2-w4-compare.redacted.json` usa `<usuario>` en las rutas.
- Barridos: W3 29 archivos y W4 38 archivos, 0 coincidencias. Mi propio barrido de nombres, correos, rutas de usuario, OneDrive y tokens tampoco encontró nada.
- No hay secretos. Bentley y CONNECTION Client son nombres de producto, no datos personales.

**15. F7 y READY (no se inicia nada).**
- **Fila F7 del Freeze:** conformidad, regresión (Full), entrada de FOUNDATIONS redactada (Freeze §7: Consumes/Extends/Introduces), guía de uso (`RACKRAMPAQUETE`, volcado, comparador, hoja manual) y ADR-0050 (sin cambios salvo A-n).
- **Requisitos:**
  - autorización del Owner (F7 = NO);
  - sin código nuevo;
  - después, READY-01..09 y `FINAL_CANDIDATE_SHA` con Core y UI Full local, builds y CI;
  - **OV-I65-01..07 repetidas sobre el Candidato final, incluida OV-I65-02, más OV-I65-08** (Freeze §5, LIFECYCLE §8).
- **Riesgos y limitaciones que pasan a READY:**
  - equivalencia de transferencia, no validación estructural (Q-O3);
  - propiedades NOT_EVALUATED (D-09);
  - peso propio excluido; DIF-01 −0.62 %;
  - edición formal del catálogo de RAM UNKNOWN;
  - A65-F4R-07 (`Valid` sin tabla);
  - A65-F4F-04 (opción 0 o 1 + A-n) y A65-F4F-08 (script de ACL) abiertas;
  - conflicto de censos con I-64;
  - C-5 pendiente;
  - dependencia de inicio de sesión en Bentley para las ventanas de RAM;
  - regla de reutilización del proceso de RAM;
  - operador manual no independiente (A65-F5F-08);
  - A65-F5C-01 no sirve de precedente para el Candidato final.

---

## Matriz AC-F5-01..10

| AC | Estado | Evidencia |
|---|---|---|
| 01 | CUMPLE | Decisiones §85-§87 (CD-I65-F4-224, CD-I65-F5-01/07/09/14); Codex 50/52, `attempts` 7/9, 0 Workers (CD-I65-F5-51, yml) |
| 02 | CUMPLE | Blobs `4e6e1feb` y `c18def42` en `bd1fcf8a` y HEAD; sin A-n |
| 03 | CUMPLE | Evidencia §92-§94; declaración CD-I65-F5-27 y aceptación CD-I65-F5-30; DLL con `-o` registrada en el bloque §7.1, por la lectura de A65-F5C-08(c) ratificada |
| 04 | CUMPLE | `Valid`, 16/16, 6/6 (también recalculado aquí); aceptación del Owner CD-I65-F5-43; integridad del host W3-08 (DIFF INFORMAR, K1/K2) |
| 05 | CUMPLE | `READBACK OK` con huella; `DUMP OK` ×2; comparador PASS con `LOADS` PASS; `manualDumpSha256` `12c89479`; Q2 reproducida; aceptación CD-I65-F5-52; W4-11 |
| 06 | CUMPLE | Custodia completa (mismo PID), contraste IGUAL; no criptográfica |
| 07 | CUMPLE | Punto 14 |
| 08 | CUMPLE | Producto sin cambios; descartes «No» en las capturas 03 y 14, modelo vacío en la 15; sin análisis según la cronología (evidencia operativa) |
| 09 | PENDIENTE POR CONSTRUCCIÓN | El Architect aporta este informe, sin REQUIRED. Falta la conformidad de cierre del Coordinator y versionar ambos en el commit de cierre |
| 10 | PENDIENTE POR CONSTRUCCIÓN | Falta: commit de cierre → árbol limpio → Core Full local → push → CI exacto 4/4 → GATE PASS registrado después |

## OV-I65-02: **PASS**

| Clasificación | Filas del comparador (14 en total) |
|---|---|
| **PASS** (10) | NODE-COORDINATES, CONNECTIVITY, LOCAL-AXES, SECTION-NAME, MATERIAL-NAME, MEMBER-DEFAULTS, RELEASES-SUPPORTS, LOADS, COMBINATIONS, CASE-CATEGORY |
| **NOT_EVALUATED** (2) | SECTION-PROPERTIES, MATERIAL-PROPERTIES (D-09, hoja firmada); no se convierten en PASS |
| **INFORMATIVE** (2) | MANUAL-CASE-CATEGORY (`DL`), SELF-WEIGHT (0, 0, 0 en ambos) |

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-F5F-01 | Evidencia §96; `host-coordination-w3-2026-10-19.md` | El hash custodiado `8CAC1648…` es el de `202ae3ba`. `d4942f30` añadió 6 líneas (recomprobación de I-52) y el blob en HEAD tiene SHA-256 `2B8E41F6…`, sin registro nuevo | OPTIONAL | Registrar el hash nuevo en el cierre, sin reescribir §96 |
| A65-F5F-02 | Evidencia §97; `w4-prep-summary.txt` | `8F2DACD0…` son los bytes de trabajo con fin de línea mixto; el blob LF tiene SHA-256 `29ADF14C…`. Repite la clase de A65-F4F-01 | OPTIONAL | Registrar el hash LF; calcular la custodia desde `git show`. Devolver al Coordinator como fallo repetido |
| A65-F5F-03 | Anexo §1.2/§1.3; evidencia de W3/W4 | No hay medición versionada antes del uso de `w1_hostsnap.ps1`, `w1_idle.ps1`, el validador, `f5_dryrun_compare.py`, `w2_readiness.ps1` ni `w2_hostsnap.ps1`. Los árboles de bases medidos (`e70bfff0`/`b8e46d28`, método PS) no son los del anexo (`ccdc2346`/`4cb742a5`, método Python): la identidad es transitiva vía W2, no literal, y solo `timeline-w4.txt` lo explica (lo atribuye a DIF-04) | OPTIONAL | Citar los hashes locales en el cierre o registrarlo como limitación; para READY, un solo método |
| A65-F5F-04 | Anexo §3; `timeline-w4` 12:07:50Z | El contraste W4-09 se ejecutó con RAM abierto y antes del manual; el anexo lo sitúa después del cierre. Sin efecto en los datos (la salida es solo veredicto y conteos) | OPTIONAL | Registrarlo como desviación; aclararlo en el procedimiento de READY |
| A65-F5F-05 | W4-PR-04; anexo §3; custodia | RAM no se cerró al límite de la ventana original (05:02:51Z) y la ventana nueva empezó con ram=1 (proceso iniciado 9.5 h antes del `session-start`). Lo cubren la disposición «W4 Resume» y §96 | OPTIONAL | Mencionarlo expresamente en el cierre; para READY, fijar la regla tras un STOP de licencia y un proceso de RAM nuevo por ventana |
| A65-F5F-06 | Informe W4 §1; comparador | «Δ máx. 0 % y 0 in» es el redondeo a 6 decimales (`Math.Round`, línea 715). Los valores reales son unos 7e-8 in en coordenadas, ~1.0e-7 % en valores y ≲3e-7 in en posiciones | OPTIONAL | Redactarlo como «0 al redondeo de 6 decimales» |
| A65-F5F-07 | Capturas 11-12 de W4 | La columna «Valor» muestra Q1, Q2 y Q5 sin signo (0.449618, 0.337213, 0.661387); probablemente es el ancho de la columna. El volcado (API) los lee negativos | OPTIONAL | Anotar que la autoridad del signo es el volcado y `manual-dump-vs-sheet` |
| A65-F5F-08 | CD-I65-F5-10; W4 | La independencia del manual es por fuente y por firma numérica, no por persona (el mismo agente). Aceptada para F5 | OPTIONAL (READY) | Para el Candidato final: ingeniero u otro operador, o conservar la comprobación de firma contra la hoja |

## Estado de los hallazgos previos

- **A65-F5C-01** ATENDIDA (CD-I65-F5-21; respetada: solo `docs/` y sin cambiar oráculos).
- **A65-F5C-02** ATENDIDA.
- **A65-F5C-03** ATENDIDA en el anexo §1; la ejecución es parcial (A65-F5F-03).
- **A65-F5C-04** ATENDIDA: una sola inversión, valores negativos en el volcado manual.
- **A65-F5C-05** ATENDIDA: 2.5 h con STOP, integridad antes y después, bases medidas; con el matiz de A65-F5F-05.
- **A65-F5C-06** ATENDIDA: capturas recortadas, sin rejillas del generado. Esta revisión actúa como segundo revisor.
- **A65-F5C-07** ATENDIDA (a-e); (f) registrada sin reescritura.
- **A65-F5C-08** DISPUESTA y ratificada (CD-I65-F5-30); aplicada en la matriz.
- **A65-F5C-09** errata REGISTRADA.
- **A65-F4F-01** REGISTRADA, con recurrencia en A65-F5F-02.
- **A65-F4F-02, 06** REGISTRADAS.
- **A65-F4F-03, 05, 10** CORREGIDAS (el yml en HEAD está al día).
- **A65-F4F-04** opción 0 aplicada en F5; la decisión **sigue ABIERTA para READY**.
- **A65-F4F-07** RESUELTA por el Owner (CD-I65-F5-31); C-5 sigue pendiente.
- **A65-F4F-08** atendida en la operación (mismo camino y misma página de códigos; ACL sin cambios en W3); la corrección del script **queda ABIERTA para READY**.
- **A65-F4F-09** ATENDIDA en F5 (bases medidas en el preflight y el POST de W4).

## No comprobado sin ejecutar

- `origin/main` con un fetch fresco. La referencia local es `bb0d5522`; el último fetch registrado es del 2026-10-19 a las 02:47:49Z.
- Las corridas de CI `39187567306`, `40689115438` y las de la custodia de W3, en la API.
- Los archivos locales en `D:\r65-smoke`: originales sin redactar, `expected-inputs.json`, el script `f7ed0b06` y los hashes de las herramientas del Coordinator.
- La ejecución del validador, del comparador, del contraste y de la custodia.
- Que no se guardó ningún modelo ni se ejecutó ningún análisis; solo hay evidencia operativa.
- La causa de la fecha de la carpeta de la DLL.
- Que el signo de las capturas 11-12 sea un artefacto de visualización; es una inferencia.
- El estado del host, de RAM y de AutoCAD.
- Core Full y CI del SHA de cierre, que todavía no existe.
