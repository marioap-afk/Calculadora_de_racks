# I-64 — Paquete de vuelta de Smoke-1 R20261105T050250Z-785a → COORDINATOR_SMOKE1_REVIEW_REQUIRED

AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.

**Estado de la ocurrencia:** EJECUTADA COMPLETA. Termina entre SM-00a y SM-41 con cierre normal.
- Firma del Owner del registro del operador: FIRMADO, «Firmo el registro.» (20:41:06Z; `disposition/owner-signature.md`). Confirma el registro, no un PASS.
- Recheck de I-52: RECIBIDO, SIN DERIVA (19:51:33Z).
- Sin merge ni rebase de main y sin F2.
- Nada custodiado todavía: custodia solo por orden del Coordinator.

## 1. Propuesta de la sesión: **NOT PASS** (la sesión no declara PASS)
- Es el máximo que fijó la disposición de SM-14h: SM-14h = NOT MET (OPERATOR_INPUT_INCIDENT) y SM-18 = COMPROMISED_BY_PRIOR_OPERATOR_INCIDENT, con la aserción de escritura INCONCLUSIVE.
- No hubo ninguna condición de STOP: una sola acad.exe todo el tiempo, el DLL autorizado, ningún dibujo guardado y ningún error del producto.
- **Las comprobaciones críticas de regresión dan PASS:**
  - SM-03 (C1): la paleta aparece flotante dentro de la pantalla.
  - SM-13 (C2): presentación de orden en curso con `LINE`.
  - SM-17 (C1): acoplada a la izquierda y a la derecha, flotante dentro de la pantalla, y la ocultación automática funciona.
  - SM-26 (C2): «Sin documento» sin el error de DocumentActivated.
  - SM-27 (C2): Start sin sesión anterior; sesiones nuevas y la misma sesión de C al volver.
  - SM-34 (C2): RACKEDITAR con presentación de orden en curso, sin filas, y el editor sigue usable.
  - SM-35: **autoritativo en B** (DBMOD 0 antes y 0 después).
  - SM-39: RackLog sin entradas, sin NullReferenceException ni fuga de transacción.

## 2. Resultados de SM (detalle en operator/operator-record.md; diario en operator/session-log.md)
| Paso | Resultado |
|---|---|
| SM-00a, 00b, 00d, 00d1, 00e, 00c | PASS (CPROFILE <<Unnamed Profile>>, SECURELOAD 1, PICKFIRST 1; «Load Once»; inspección ligada a la PID PASS; C = 17 entidades) |
| SM-01 / SM-02 | PASS (ms=297 nunca abierto; DBMOD 0) |
| **SM-03 (C1)** | **PASS** |
| SM-04, SM-05 | PASS |
| SM-06, primera ejecución | PASS tal como se observó (54ef4061); OPERATOR_SELECTION_INCIDENT |
| SM-06, repetido | PASS — RECOVERY REPEAT AUTHORIZED BY COORDINATOR (56ec2c3a) |
| SM-07 … SM-12 | PASS |
| **SM-13 (C2)** | **PASS** |
| S1-06a..e | NOT_APPLICABLE_BY_DESIGN (A-7) |
| SM-14a..e | PASS (complementario). Nota: la casilla no se conmutó, por cómo envía el texto la herramienta |
| **SM-14h** | **NOT MET / OPERATOR_INPUT_INCIDENT** (DBMOD 16 → 17 por el PASTECLIP cancelado) |
| SM-15, SM-16 | PASS |
| **SM-17 (C1)** | **PASS** |
| **SM-18** | **COMPROMISED_BY_PRIOR_OPERATOR_INCIDENT**; DBMOD 17 = INCONCLUSIVE FOR DRAWING-WRITE ASSERTION |
| SM-19 | NOT_APPLICABLE (PICKFIRST 1) |
| SM-20 … SM-25 | PASS (sesiones 3, 1, 3, 4; la copia de solo lectura se cierra sin pregunta) |
| **SM-26 (C2)** | **PASS** |
| **SM-27 (C2)** | **PASS** (sesiones 7, Sin documento, 8 y 7) |
| SM-28, SM-29, SM-30 | PASS; en SM-29 el arrastre con el botón central no es ejecutable con la herramienta |
| SM-31 / SM-32 | PASS; ms=375 (oculto) y ms=406 (visible) |
| SM-33 | PASS |
| **SM-34 (C2)** | **PASS** |
| SM-35 | PASS, autoritativo en B (requisito: B con DBMOD 0 antes de SM-34) |
| SM-35n | nota de T3D registrada |
| SM-36, SM-37, SM-38 | PASS |
| **SM-39** | **PASS** |
| SM-40 | PASS |
| SM-41 | PASS: los cinco SHA sin cambios |

## 3. Proceso, DLL, BASELINE, POST, DIFF y HOST-EXTRAS
- **AUTHORIZED_AUTOCAD_PID:**
  - PID 78512, inicio 2026-11-05T17:25:16.8462970Z, acad.exe 2A75996F…6422.
  - Fichero `process/authorized-autocad-pid.json` (DE520754…3038).
  - Guarda: REGISTER, 20 CHECK, PRE-CLOSE y CLOSED, todo PASS (`process/guard-log.txt`, D74C0944…9984).
  - Vigilancia de 1 s de 17:25:43Z a 19:49:30Z: una sola acad.exe y salida normal de la PID tras PRE-CLOSE a las 19:47:20Z; código 0 (`process/acad-watch.log`, D44AAC94…3295).
- **DLL cargado (en la PID 78512):**
  - un solo `RackCad.Plugin.dll`, en `<worktree-de-I-64>\src\RackCad.Plugin\bin\Debug\net8.0-windows\`;
  - SHA-256 3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0;
  - con RackCad.Application 12B8E237 y RackCad.UI 363AE9FE, MATCH;
  - inspecciones a las 17:28:21Z, 18:48:38Z y 19:17:00Z, todas VERDICT PASS.
- **BASELINE:**
  - CanonicalSha256 980ce80f208e87ebc2da5fce50249ce88391d5fd2823d97c17fe105d202123dd;
  - archivo 17e81015…1e92;
  - companion local 315d6e78… MATCH.
- **POST:**
  - código 0; CanonicalSha256 310f6ac94bd2e1c7599f9b4526e8896b1c51b301226a2a30f7d063786ab08916;
  - archivo 97737343d55b8d206e377f97db1487923aaae6c9e93ac4978c40d57e20b72e35;
  - 2241 líneas, 399 claves, 1772 valores, 21 archivos, 3 plugins, env 0, ilegibles 0.
- **DIFF de T3C:**
  - herramientas 750A9097, DE642515 y D0BB54BE; PaletteKeyAcceptance = ACCEPTED;
  - **CLEAN, código 0**: added 0, removed 0, changed 1;
  - ALLOWED 1: R-A-03, `Dialogs\OpenSaveAnavDialogs\InitialDirectory`;
  - REPORT 0, FORBIDDEN 0, INVALID 0;
  - `diff/profile-diff.json` BB5DC356…AFDD.
  - Con una sola instancia de principio a fin, esta vez el POST refleja también la salida de la PID operada: la limitación de a578 no se repite.
- **HOST-EXTRAS:**
  - BEFORE f9499f0f…0c79 y AFTER E8C8D14C…3606, los dos con código 0. Comparación (`kit/compare_host_extras.py` FF09B93C): **REPORT_REQUIRED, código 4**.
  - AssemblyMap idéntico: 2 valores, RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z. CoreUser idéntico (29 y 28 entradas).
  - En la clave del producto:
    - `Dialogs\Select File` LASTWRITE_ONLY, a las 19:27:26.148Z (el diálogo OPEN de SM-24);
    - `Recent File List` VALUES_CHANGED, a las 19:47:15.455Z, al salir: lista de recientes por los dibujos abiertos, como en 0a77 con una sola instancia.
  - **Propuesta: INFORMATIVE (regla de 0a77).** Decide el Coordinator.
- **HKCU Applications:**
  - Lectura adicional de solo lectura: LastWrite de Applications 19:47:16.440Z.
  - Lo reescribe AcMr («AutoCAD Action Commands», de AutoCAD) con Commands, Groups y MRU durante la salida de la PID 78512; mismo patrón que a578.
  - Loaded e InstalledApplications sin cambio. Ninguna entrada RackCad. El DIFF de T3C no tiene entradas en Applications.
  - **Propuesta: INFORMATIVE.** No se limpió nada (`host-extras/applications-lastwrite-session.txt`).
- **Estado final:** acad.exe = 0 y RAMElements.exe = 0 (PG-5 a las 19:47:32Z; POST a las 19:48Z).
- **I-52:**
  - Aviso de fin y petición de recheck a las ~19:51Z (msg 41693629); I-62 avisado (msg 3e74c602).
  - Respuesta a las 19:51:33Z: **«SIN DERIVA de lo que I-52 registró. Solo cambió la última escritura de HKCU Applications.»**
    - acad = 0; TRUSTEDPATHS, SECURELOAD, perfil, AssemblyMap, Loaded y CoreUser iguales; sin RackCad;
    - Applications con el mismo conjunto de claves y LW 19:47:16.440Z, el mismo dato que la lectura de la sesión;
    - I-52 no concede ACK/RELEASE y no cubre RackLog, el DIFF/HOST-EXTRAS ni la clave del producto.
  - Literal en `disposition/i52-post-run-recheck.txt`, con dos rutas locales redactadas.

## 4. Puntos que clasifica el Coordinator
1. **SM-31 y SM-32, el portapapeles interno de la herramienta.**
   - Para la línea larga de medición, la acción de escribir usó por su cuenta su vía rápida del portapapeles («Typed (via clipboard)»).
   - Va contra la letra de la restricción «sin Ctrl+V».
   - El texto quedó en la entrada sin ejecutar. Se vació con `Esc` («…*Cancel*») y se volvió a escribir en trozos cortos enviados como teclas, comprobando el texto antes de `Enter`.
   - No hubo PASTECLIP ni cambio en el dibujo. La medición válida es la escrita con teclas.
2. **HOST-EXTRAS REPORT_REQUIRED** (código 4) y **LastWrite de Applications**: propuesta INFORMATIVE en los dos casos (sección 3).
3. **Autoguardados SAVETIME de A y C en la carpeta temporal:**
   - A y C durante la pausa de SM-07; A tras cancelar PASTECLIP; C en la fase D.
   - Es comportamiento de AutoCAD, como en 0a77 y a578. Los dibujos conservan su SHA y los `.sv$` desaparecieron con el cierre.
4. **Diferencias observadas frente a a578 que no cambian lo esperado:**
   - en SM-13, después de la orden aparecen `cache.invalidate` y `drain`, no `selection.read` y `drain`;
   - en SM-14a, la casilla no se conmutó;
   - en SM-29, el arrastre con el botón central no es ejecutable con la herramienta.
5. **Pasos añadidos que no están en la guía:**
   - DBMOD de B antes de SM-34, como requisito pedido por la disposición de SM-14h;
   - acoplar la paleta a la derecha al final de SM-17, como en a578.

## 5. Evidencia
`artifacts/orchestration/I-64/F1-SMOKE-1/R20261105T050250Z-785a/` (gitignored):
- authorization;
- kit, con su generation-record;
- preflight;
- host-extras;
- profile;
- console: baseline, post, diff y host-extras-compare;
- diff;
- process;
- logs: racklog-BEFORE, racklog-checks y sm41;
- operator: operator-guide, session-log, operator-record y screens (14 PNG con su .sha256; solo la paleta y la línea de órdenes, sin datos ajenos);
- disposition:
  - deviation-request-sm07 y las dos disposiciones literales;
  - incident-sm14h-pasteclip;
  - los dos no-save-check;
  - i52-post-run-recheck.txt y owner-signature.md;
  - evidence-hashes.txt, regenerado después de la firma (incluye este paquete en su versión anterior a la regeneración; el SHA final de este paquete va en el mensaje de entrega);
  - este paquete.
