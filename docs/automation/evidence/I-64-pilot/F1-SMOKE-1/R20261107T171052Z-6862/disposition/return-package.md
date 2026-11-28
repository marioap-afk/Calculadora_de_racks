# I-64 — Paquete de vuelta en COORDINATOR_SMOKE1_REVIEW_REQUIRED, RunId R20261107T171052Z-6862

Responde a «I-64 — COORDINATOR FULL SMOKE-1 AUTHORIZATION», literal en `authorization/coordinator-full-smoke1-authorization-6862.md`.

**Propuesta de la sesión: PASS, sujeta a las tres decisiones de la sección 3.** La sesión no declara PASS; la decisión es del Coordinator.

**Resumen:**
- La ocurrencia se ejecutó completa, sin ningún STOP de la guarda, con una sola `acad.exe` de principio a fin.
- Todo lo que 785a dejó pendiente tiene evidencia nueva: S1-06h, S1-08 / OV-20, S1-02 y las capturas autoritativas de SM-17, SM-26, SM-27b y SM-34.
- Hubo **una pausa, en SM-29**: el gesto del Owner fue más allá del arrastre autorizado. Se repitió de forma limpia.
- **La disposición de esa pausa llegó como la letra «A» escrita por el Owner, sin texto literal del Coordinator.** Hay que confirmarla (sección 3, punto 1).

## 1. Identidad

| Punto | Valor |
|---|---|
| Candidato | `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6`, sin recompilar |
| Punta de la rama | `1917bc19…`; delta frente al Candidato: src 0, tests 0, proyecto 0, fuera de `docs/automation` 0 |
| DLL | `3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0`, ProductVersion `1.0.0+cc7503e3…` |
| AUTHORIZED_AUTOCAD_PID | **67260**, inicio 2026-11-07T18:05:46.9023864Z, exe `2A75996F…6422` (`process/authorized-autocad-pid.json`) |
| Módulos | VERDICT PASS a las 18:08:15Z y 18:47:08Z: un solo `RackCad.Plugin.dll`, en el bin del Candidato, con el SHA autorizado |
| Guarda | REGISTER 1, CHECK 22 (incluido PRE-CLOSE), CLOSED 1, todos PASS; 0 líneas FAIL o INVALID. Vigilancia de 1 s de 18:06:12Z a 19:06:25Z, código 0 |
| Fin | salida normal a las 19:05:02Z; PG-5 PASS a las 19:05:16Z; acad = 0 y RAM = 0 |
| SECURELOAD | 1 (preflight, `reg query`, ejecutado por la sesión); la BASELINE lo lee por su cuenta: MATCH |
| BASELINE | Canonical `411b97fd…76c2`, archivo `E5215177…7E51` |
| POST | Canonical `353087a9…c523`, archivo `6485685B…C8AC`, código 0 |
| DIFF de T3C | **CLEAN**, código 0; added 1, changed 2; ALLOWED 3 (R-A-03: `Application Window\Position`, `OpenSaveAnavDialogs\InitialDirectory`, `Variables\VIEWBACKSTATUS`); REPORT, FORBIDDEN e INVALID 0; `B2400A71…C1E8` |
| Dibujos (SM-41) | A, B, C, D y biblioteca byte a byte iguales; 0 archivos escritos; sin `.bak`, `.sv$` ni `.dwl` |
| RackLog (SM-39) | cuatro archivos iguales que BEFORE; ningún archivo nuevo; sin NullReferenceException de `DocumentActivated`, sin fuga de transacción, sin ERROR |
| I-52 | 19:07:21Z, sin deriva en lo que registra; sin operaciones de host en la ventana; literal en `disposition/i52-post-run-recheck.txt` |

## 2. Requisitos de T3D §9

| Requisito | Paso | Resultado | Evidencia clave |
|---|---|---|---|
| S1-01, INV-02 | SM-03, SM-04, SM-28 | PASS | Stage B limpio: SM-03-palette `60D5AC99`, cmdline `1C44A77E` |
| **S1-02, OV-02** | SM-29 | **PASS propuesto** | ver la sección 3, punto 1 |
| S1-03, OV-03 | SM-30 | PASS | `UNDO` 3 → «ERASE COPY MOVE» |
| S1-04, OV-04, OV-14 | SM-06..SM-11 | PASS | incluye Sin identidad (SM-10b) y Diagnóstico (SM-11), observados en el host |
| S1-05, OV-15 | SM-20..SM-27 | PASS | sesiones 1, 3, 4, 5, 7 y 8 sin mezclarse |
| S1-06a..e | — | NOT_APPLICABLE_BY_DESIGN | I-64-A-7 |
| **S1-06f** | SM-13 | PASS | casillas desmarcadas; 28 puntos durante la orden; `C2F1E2F6` |
| **S1-06g** | SM-15 | PASS | teclas reales con foco en la paleta: nada en la línea de órdenes |
| **S1-06h** | SM-14h, SM-18 | **PASS** | bit 1 = 0 en las 13 lecturas de A (DBMOD 0 en SM-02; 16, solo vista, en las demás) |
| Complementarias | SM-14a..e | PASS | foco visible y capturado antes de `Ctrl+V`; `Ctrl+V` una vez, sin PASTECLIP |
| **S1-07, OV-17** | SM-34, SM-35 | PASS | captura autoritativa `BBA9F621`; B: DBMOD 0 antes y después |
| **S1-08, OV-20** | SM-02, SM-18 | **PASS** | A: bit 1 = 0 en SM-02 y en SM-18; `DE705695` |
| S1-09 | SM-12 | PASS | 0 filas entre la espera de unos 62 s y `panel.hide` |
| S1-10 | SM-01, SM-31, SM-32 | PASS | 188, 234 y 234 ms, sin umbrales |
| S1-11, INV-21 | POST, DIFF | CLEAN | sección 1 |
| S1-12 | SM-19 | NOT_APPLICABLE | PICKFIRST = 1 |
| SM-36..SM-39 | | sin hallazgos | |
| SM-41 | | sin cambios | `logs/sm41.txt` |

**Comprobaciones críticas de la autorización:**
- **SM-17:** izquierda, derecha, flotante, plegada, desplegada y derecha final, todas capturadas y limpias.
- **SM-26:** `22109588`.
- **SM-27b:** `FEED48C2`, sin la sesión anterior de C.
- **SM-34:** `BBA9F621`.

## 3. Lo que debe decidir el Coordinator

1. **SM-29 y S1-02 (OWNER_GESTURE_INCIDENT propuesto).** Detalle en `disposition/deviation-request-sm29.md` y `disposition/disposition-sm29-option-a.md`.
   - **El primer gesto** no cumplió «only the requested drag»: el Owner marcó «Clic izquierdo», «Tecla Esc» y «Giré la rueda». V0 (3344.54 1493.04) → V1 (3079.37 1205.26), mezclado con zoom.
   - **Durante la pausa** el Owner hizo otro arrastre sin que se le pidiera (V1 → V2). Se conserva, pero no se usa como prueba.
   - **Procedencia:** a la petición de opciones, el Owner respondió «A». **La sesión siguió la opción A sin texto literal del Coordinator.** Se pide confirmarla o rechazarla.
   - **Repetición limpia:** V2 (3596.49 1229.31 0.0), VIEWSIZE 1649.97 → V3 (4467.18 1450.59 0.0), VIEWSIZE 1649.97, sin líneas nuevas en el historial y sin filas nuevas. Owner: «La vista siguió al ratón», «Paleta sin parpadeo». Capturas `SM-29-repeticion-*`. Se cumple el criterio de la opción A.
   - **Límite:** antes de leer V3 la sesión hizo clic en la línea de órdenes, porque el Owner había usado el chat. Que la paleta no roba el foco durante el arrastre se apoya en la observación del Owner y en el historial sin líneas nuevas, no en una lectura sin clic.
   - **Sin efecto sobre las aserciones de escritura:** SM-14h y SM-18 ya estaban hechas en A; el gesto fue en C, descartable y sin aserción; B seguía limpio.
   - **Una repetición de un gesto, no de la ocurrencia.** T3D §11 prohíbe repetir una corrida para buscar otro resultado; la sesión lo lee como no aplicable aquí, y lo decide el Coordinator.

2. **Clasificaciones de host (propuestas, por el precedente de 785a).**
   - **HOST-EXTRAS:** REPORT_REQUIRED mecánico; INFORMATIVE propuesto. Solo `Select File` LASTWRITE_ONLY (18:30:54.241Z, diálogo de SM-24) y `Recent File List` VALUES_CHANGED (19:04:59.266Z, al salir). AssemblyMap y CoreUser idénticos.
   - **HKCU Applications:** LW 19:04:59.935Z, durante la salida; mismas 7 claves, sin RackCad. INFORMATIVE propuesto (WRITE_ACTIVITY YES, SEMANTIC_DRIFT NO).
   - **Autoguardados `.sv$`:** de A durante SM-15 y de C más tarde. Ningún DWG guardado; los archivos fuente son idénticos; ningún resultado se apoya en ellos; ninguno de esta PID sigue en TEMP.

3. **PaletteKeyAcceptance.** La propuesta pedía autorizarlo y la autorización no lo nombra. La sesión pasó `NOT_RECORDED`, el valor que T3D §8 prevé. **Sin efecto en el resultado:** el DIFF es CLEAN y no tiene entradas de la clave de la paleta.

## 4. Otras notas que el Coordinator debe conocer

- **Stage A** se hizo dos veces. La primera encontró ChatGPT maximizado en el monitor principal; AutoCAD no se abrió hasta que el Owner lo quitó. Entre la BASELINE (17:24Z) y la apertura (18:05Z) pasaron unos 40 min, sin otra apertura de AutoCAD; el DIFF no muestra deriva ajena.
- **Ventanas ocultas por la herramienta:** se ocultan mientras tiene el control y vuelven al terminar el turno. Así reapareció ChatGPT en SM-29, lo que es una interpretación de la sesión.
- **Capa transparente de la app de Claude** a pantalla completa en el monitor principal, mientras la herramienta tiene el control. Deja un tinte rosado en los bordes de las capturas y no tapa texto. Es otra interpretación de la sesión, que ajustó la comprobación previa para bloquear solo ventanas opacas.
- **Capturas:** 41 PNG. Las críticas se abrieron al tomarlas. Cinco de apoyo se abrieron después, antes de este paquete (registro, incidente 5). `SM-34-cmdline` queda tapada en parte por el editor de RackCad, con el prompt legible; la autoritativa de SM-34 es la de la paleta.
- **Comprobación antes de `Enter`:** cumplida en las expresiones, salvo una imagen revisada justo después en A-2. Órdenes cortas sin expresión se enviaron en lote con su `Enter` (registro, incidente 6). Ningún resultado inesperado.
- **`_ALL N found`** de SM-31 y SM-32 queda cortado en la línea de órdenes; no se afirma el número.
- **`MBUTTONPAN`** no se leyó: la autorización no lo nombra.
- **`host-extras/after-read-session.txt`** dice «lineas STOP = 1» porque su búsqueda coincide con el texto `watch-stop.flag`. No hubo ningún STOP.
- **I-52** señala una diferencia con sus notas en BlockPreviewUser. Los HOST-EXTRAS muestran dos claves distintas, sin cambio y anteriores a la ventana (nota en `disposition/i52-post-run-recheck.txt`).

## 5. Lo que no se hizo

- Sin commit: la custodia espera la revisión. Sin merge ni rebase de main, sin F2 y sin cambios de producto.
- Sin escrituras en el registro, el perfil, TRUSTEDPATHS, SECURELOAD, AssemblyMap ni ACL por parte de la sesión.
- Ningún dibujo guardado; los `.sv$` previos no se tocaron.

**Registro del operador:** `operator/operator-record.md`. Firma del Owner: «Firmo el registro.», hora medida 2026-11-07T19:16:29Z (`disposition/owner-signature.md`); confirma el registro, no un PASS. **Manifiesto:** `disposition/evidence-hashes.txt`.
