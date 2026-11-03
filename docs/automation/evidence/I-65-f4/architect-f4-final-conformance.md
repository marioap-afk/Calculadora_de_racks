# I-65 F4 — revisión final de conformidad del Architect (cierre de F4 sobre el Candidato de hito `5a5ca577`)

- Orden: «F4 Final Conformance Review and Gate Closure» (decisiones §82). Architect = subagente `claude-opus-5-5`, SAME-SESSION ROLE,
  solo lectura; prompt: [architect-f4-final.prompt.md](architect-f4-final.prompt.md); barrido K5:
  [architect-f4-final-k5-transcript-scan.json](architect-f4-final-k5-transcript-scan.json). K1/K2 de los checkouts sin hallazgos.
- Texto del informe, literal:

---

**Veredicto: CONFORMANT WITH OBSERVATIONS** sobre la evidencia final de F4 del Candidato de hito `5a5ca57731ebbbbc41ed4f702698709d5e128a86`. No hay ningún hallazgo REQUIRED y registro 10 observaciones OPTIONAL nuevas (A65-F4F-01..10). La matriz OV-I65-01 y 03..07 no tiene ningún FAIL ni ningún INCOMPLETE. Los criterios de cierre se cumplen, salvo los que solo pueden cumplirse después de este informe (LIFECYCLE §7 puntos 4 y 6, AC-F4-10 y el registro de cierre del Coordinator). No declaro GATE PASS.

Nota para la determinación: el texto del Owner en §82 dice «CONFORMANT sin REQUIRED». F1 y F3 cerraron con «CONFORMANT WITH OBSERVATIONS» (CD-I65-F1-21 y CD-I65-F3-17), así que este veredicto sigue ese precedente. La lectura final corresponde al Coordinator y al Owner.

**pwd informado:** `/d/Documentos/Codex/Calculadora de racks` (checkout principal). No lo leí ni lo usé.

**Método (solo lectura):**
- Usé `git -C <worktree>` con `rev-parse`, `ls-tree`, `log`, `show`, `diff` (también `--no-index` sobre archivos del worktree), `grep`, `merge-base`, `rev-list`, `config --get` y `check-attr`.
- Calculé `sha256sum` sobre blobs y usé `cat`, `sed`, `od` y `wc` sobre rutas absolutas del worktree.
- Miré 9 capturas con el lector de imágenes.
- No ejecuté `gh`, fetch, intérpretes, compilaciones, AutoCAD ni RAM, y no escribí ningún archivo. No leí D:/r65-smoke ni D:/r65-m05c.
- **Transparencia:**
  - mi primer `git status --short` corrió sin `GIT_OPTIONAL_LOCKS=0`: pudo refrescar la caché de stat del índice, sin cambiar contenido;
  - una llamada hizo `cd` a `docs/automation/decisions` dentro del worktree;
  - `diff --no-index` imprimió avisos de LF/CRLF sin escribir nada.

## Respuestas

**1. Freeze y ADR.**
- El Freeze (blob `4e6e1feb…`) y ADR-0050 (blob `c18def42…`) tienen el mismo blob en `5a5ca577` y en HEAD `e14e4b76`, y ningún commit los toca en ese rango.
- No hay ninguna A-n ni ningún `freeze-delta`.
- Los cambios hasta HEAD solo añaden texto: la única eliminación de líneas es la de `state/I-65.yml` (6/5). La custodia de P2 no cambia.

**2. Identidad.**
- `git diff --name-only 5a5ca577 HEAD` devuelve 136 archivos, todos bajo `docs/`. Desde `51fc82f1` hasta HEAD hay 0 archivos fuera de `docs/`.
- Los árboles son idénticos en `51fc82f1`, `5a5ca577` y HEAD: `src` `6cadd526`, `tests` `47fbcaf5`, `tools` `29485f42`, `assets` `d1cc1811`, más `Directory.Build.targets`, `global.json` y `RackCad.sln`.
- Tabla: el blob `d088e2a9` da SHA-256 `fc3d9146…b619`.
- La DLL `c72b759d…ac07` (`1.0.0+5a5ca577…`) y la tabla `fc3d9146` coinciden en:
  - el bloque §7.1 de la evidencia §81;
  - la BASELINE (B-09, que incluye la ProductVersion completa) y el POST, cuyo diff solo cambia B-04, X-02 y X-03;
  - el timeline antes de NETLOAD y antes de «Open»;
  - las listas de módulos de `acad.exe` (PID 26824): Plugin, UI, Application y Domain desde `D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows`, con los SHA-256 del manifiesto de salida `ed2875e4`.

**3. W1.**

| Caso | Resultado | Evidencia |
|---|---|---|
| OV-I65-01, parte AutoCAD | **PASS** | `Valid`; validador v2 16/16 (V-01..V-16); huella de la consola `f5ce900c` = V-10/V-11 = manifiesto; contraste 117/117 «IGUAL», 0 hallazgos; `rowsSha256` `54ca9f6c` (V-14); cargas `25f48e7f` (V-06/V-07) |
| OV-I65-04, parte AutoCAD | **PASS** | `pkg-OV04` `Valid` 16/16. Frente a `pkg-OV01` solo cambian `packageFolder` y el hash del script (comparé los manifiestos) |
| OV-I65-03 (a)-(e) | **PASS** | En cada caso aparece solo el código esperado, sin `Invalid` y con `exists=False tmp=0` (timeline). Vi la captura 26 del caso (d) |
| «Fila sin verificar» (CD-I65-F4-186) | NOT_EVALUATED en el host | Se presenta como otra clase de evidencia (w1-report:108-109; evidencia §81 y §86) y no como ejecución en AutoCAD. Las tres pruebas de fixture existen en `5a5ca577` (ModelBuilderTests:448, PackageValidationTests:60, TranslationTests:457). Matiz de redacción en A65-F4F-03 |

- **DIFF del host:** sin PROHIBIDO. Solo cambian las MRU y `InitialDirectory` de NETLOAD y `Position`, y se borra `VIEWBACKSTATUS`; los Applications de HKCU y HKLM quedan IGUAL.
- **ACL de `AcadLocation`:** la conciliación es aceptable. Los dos hashes se midieron en el mismo instante (08:49:31Z): 850 → `d1bcd226` y 65001 → `7d64e54b`, que es la referencia. La BASELINE y el POST usan el mismo camino (`d1bcd226` en ambos), así que la comparación es homogénea (ver A65-F4F-08).

**4. W2 (contrasté los JSON versionados con el informe).**

| Caso | Resultado | Evidencia |
|---|---|---|
| OV-I65-01 | **PASS** | Lectura de vuelta `READBACK OK`, `differences` vacío, huella `f5ce900c`, 8 nudos, 7 miembros, 3 cargas, REF1 OTHER sin peso propio. Volcado `DUMP OK`: 7 miembros, 8 nudos, CM (DL) y REF1, `combinations` [], `errors` []. Comparador: PASS, código 0, `manualDumpSha256` `c4a98fb0…`, `readbackSha256` `0848b785`. 10 filas PASS (COMBINATIONS PASS: 0 y 0, conforme a CMB-3, porque el manual también tiene `combinations` []). Las dos filas de propiedades son NOT_EVALUATED (D-09) y MANUAL-CASE-CATEGORY y SELF-WEIGHT son INFORMATIVE |
| OV-I65-04 | **PASS** | `gate: the model has 1 nodes`; lectura de vuelta FAILED con `values` vacíos. Las capturas 10 y 13 son byte a byte iguales («Elementos: 1») |
| OV-I65-05 | **PASS** | `ValueError: Section not found: "HSS_SQR INEXISTENTE-99"` en la consola y en la lectura de vuelta. Elementos parciales 8+1 (capturas 18 y 20), descartados (timeline 16:18:10Z). Control del comparador REJECTED (2), con las entradas fijas de la adenda §11 |
| OV-I65-06 | **PASS** | FAILED por el eje local 2 de `ARM.1.PositiveY`. Los `values` son idénticos a los de OV-01: el modelo se construyó bien y discrimina la expectativa invertida |
| OV-I65-07a | **PASS** | `gate: package version is '0.1.0', expected '999.0.0'`. Según la plantilla, la puerta de capacidades va antes de `rm.model()`. «Elementos: 0» antes y después (capturas iguales a la 02) |
| OV-I65-07b | **PASS** | `gate: core function missing: rackcad_nonexistent_function`; el modelo sigue vacío |

- **Volcado byte a byte igual al de P2 (blob `4593b6c7`, SHA-256 `f5789d1c`): es coherente.**
  - El volcado no contiene marcas de tiempo, rutas, huella ni `rackId` (claves de primer nivel comprobadas).
  - D-09 y D-11 exigen salida canónica sin marcas de tiempo, la carga útil es igual 117/117 y RAM, la API y la plantilla son las mismas.
- **No se reutiliza la lectura de vuelta de P2.**
  - La de W2 (`0848b785`) difiere de la de P2 (`e622f429`) en la huella: `f5ce900c` frente a `505941e2`.
  - El informe del comparador cita el manifiesto `6b013b62` y la lectura `0848b785`.
  - La procedencia del volcado nuevo se apoya en tres hechos:
    - timeline: `dump_exists=False` a las 16:13:34Z;
    - capturas: la 06 muestra la carpeta con solo el script; la 07 muestra `DUMP OK`;
    - POST: aparece el `ram-dump.json`.

  El volcado no lo prueba por sí mismo (A65-F4F-04).

**5. Integridad.**
- Los tres archivos de cada paquete (`pkg-OV01`, `pkg-OV04`, `inj-OV05`..`07b`) tienen los mismos SHA-256 en la creación (evidencia §84), en el PRE y en el POST.
- Solo se añaden `ram-readback.json` (los seis, iguales a los blobs versionados) y `ram-dump.json`. El volcado manual `c4a98fb0` no cambia.
- Las inyecciones nacen de `pkg-OV01` sin alterarlo:
  - el diff de manifiestos solo muestra `packageFolder`, el hash del script y `rackcadFaultInjection` con `sourcePackageSha256` = `6b013b62`;
  - `after-w1-prep` registra `pkg-OV01` igual antes y después.
- El script de volcado `56faa355` (blob `e28cc774`) difiere de `manual-rackcad-ram-dump.py.txt` (`af262489`) solo en `dumpFolder` de la línea 11. Lo comprobé con diff de los blobs.
- Mi verificación de la custodia: 127 entradas cuadran en los blobs y 4 no cuadran, todas explicadas (A65-F4F-01 y A65-F4F-02).

**6. Estrategia A: cumplida.**
- Condiciones (a)-(d) de A65-F4C-05:
  - (a) y (b) verificadas;
  - (c) 117/117, `54ca9f6c` y `25f48e7f`;
  - (d) resuelta por el Owner: solo F4.
- Las seis del Owner (CD-I65-F4-185):
  1. Identidades: RAM `7dcb4855` y `ramelements` `f71a017f` iguales en el PF-03 de P2 y en el PRE y el POST de W2. Bases `dffd9e00`, `992ddac9` y `2a8b4239` iguales en la prelectura, en M-05c y en W2; en el momento de P2 no se midieron (A65-F4F-09).
  2. Especificación sin cambios: `f39918ad`, `11853341` y `45cde765` iguales en `5a5ca577` y en HEAD.
  3. Plantilla de volcado idéntica.
  4. Contraste del paquete con REF-CANT-01 hecho.
  5. El comparador cita `c4a98fb0`.
  6. No se reutiliza la lectura de vuelta de P2.

**7. Limitaciones (CD-I65-F4-214).** No encuentro ninguna frase que las presente como validadas.
- Propiedades: NOT_EVALUATED (w2-report:73).
- DIF-01 y peso propio: informativos (:79).
- El manifiesto lleva la advertencia de que solo es equivalencia de transferencia (V-16).
- La edición del catálogo, la estrategia A solo para F4 y los censos de I-64 no se mencionan en los informes: no se afirma nada sobre ellas.
- «Validación final» es el término de Freeze §5 para la OV.
- Recomiendo repetir la lista de limitaciones en el registro de cierre.

**8. Criterios de cierre.** La referencia de la orden («AC-F4-01..09, §8») no coincide con el documento: los criterios están en **§7** y van de **AC-F4-01 a AC-F4-11** (A65-F4F-10). Los evalúo todos.

| Criterio | Estado | Evidencia |
|---|---|---|
| AC-F4-01 | CUMPLE | §26 |
| AC-F4-02 | CUMPLE | Blobs del Freeze y del ADR (pregunta 1) |
| AC-F4-03 | CUMPLE | Por revisiones previas; Core 14390/14390 y CI 38977069487 4/4 (JSON custodiado: `push`, rama y `head_sha` exactos, 4 jobs `success`) |
| AC-F4-04 | CUMPLE | CD-I65-F4-166 y §75-§76 |
| AC-F4-05 | CUMPLE | F4-D §62-§66; DIF-01 CD-I65-F4-47; DIF-03 CD-I65-F4-74; DIF-04 CD-I65-F4-164/174; OD-F4-03 |
| AC-F4-06 | CUMPLE | §77 y §81 |
| AC-F4-07 | CUMPLE | Matriz final; paquete §3 Fc: comparador más hoja firmada, con deltas reconocidos |
| AC-F4-08 | CUMPLE | Producto sin cambios desde `51fc82f1`; W2 sin análisis ni guardado |
| AC-F4-09 | Architect: CUMPLE (este informe, 0 REQUIRED). Coordinator: CUMPLE sobre el Candidato (CD-I65-F4-180); **PENDIENTE** su registro de cierre (§82, paso 5) | |
| AC-F4-10 | **NO CUMPLE TODAVÍA** | Pendiente por construcción |
| AC-F4-11 | CUMPLE | No se activó; OV-01 PASS |

| LIFECYCLE §7 | Estado | Evidencia |
|---|---|---|
| Punto 1 | CUMPLE | Hito del Owner, §80 y §82 |
| Punto 2 | CUMPLE | Tomado de las revisiones de F4-T1..T6 y F4-D; no lo repetí |
| Punto 3 | CUMPLE | Sobre `5a5ca577` |
| Punto 4 | **PENDIENTE** | |
| Punto 5 | CUMPLE sobre el Candidato; falta el registro para el cierre | |
| Punto 6 | **PENDIENTE** | |
| Punto 7 | CUMPLE hasta hoy | Sin rebase; en mi copia `origin/main` = `bb0d5522` = merge-base, sin fetch. Repetir con fetch fresco |

**SHA de cierre según F1 (CD-I65-F1-21: `4514a99a`, código `667a3991`) y F3 (CD-I65-F3-17: `1d8c713e`, código `e3ae36d0`):**
- **Cuál es:** el **commit documental nuevo** que versione esta revisión final y el registro de cierre del Coordinator, creado después de este informe sobre la punta (hijo de `e14e4b76` si no hay otros commits).
- **Cuál no es:**
  - no es `5a5ca577`: su CI 38977069487 acredita solo a `5a5ca577`;
  - no es `e14e4b76`.
- **Evidencia que exige, en este orden:**
  1. árbol limpio tras el commit;
  2. **Core Full local sobre ese HEAD exacto**, con conteo, duración, SHA-256 del TRX y HEAD/dirty antes y después (los precedentes solo exigieron Core Full);
  3. push;
  4. **CI `push` de `refs/heads/architecture/interop-analisis-estructural`** con `head_sha` exacto y los 4 jobs en `success` (Tests, UI Tests, Build UI, Build Plugin), leída antes del siguiente gate;
  5. GATE PASS registrado después como hecho posterior, como en la evidencia §25 y §31: «SHA de cierre `<x>`, Candidato de hito `5a5ca577`, código = árbol de `51fc82f1`».
- **Comprobar además:** 0 archivos fuera de `docs/` entre `5a5ca577` y el cierre.
- **Alcance del CI de cierre:** no da evidencia nueva a `5a5ca577` (AGENTS, «Commit documental»).

**9. Incidencias: ninguna afecta a la validez.**
- **W1:**
  - el caso (c) se corrigió de 0.3 a 3 antes de exportar, y `ARM-SLOPE-UNSUPPORTED` ×3 solo es posible con más de 10°;
  - el clic de maximizar y el primer clic del caso (e) no tuvieron efecto;
  - lo de la página de códigos de la ACL ya está explicado.
- **W2:** el nudo de prueba se creó con Enter (preparación, no intento); «Limpiar» y abrir la pestaña Miembros solo afectan a la interfaz; OV-07 sin aviso de guardar confirma que no hubo escritura; el volcado idéntico ya está tratado en la pregunta 4.
- **Elementos parciales de OV-05: estoy de acuerdo con el Owner.**
  - Freeze D-08 dice literalmente que un nombre rechazado lanza `ValueError` y deja un miembro huérfano, y que hay que descartar sin guardar.
  - En la plantilla, las puertas solo cubren lo previo a escribir y `write_model` no es transaccional.
  - El descarte quedó registrado (captura 21 y timeline).

**10. Riesgos para F5 y fases posteriores, y recomendaciones (sin iniciar nada).**
- **Riesgos:**
  1. La estrategia A no vale para READY: hay que decidir pronto entre A y B, porque guardar un modelo de RAM requiere autorización.
  2. La lectura de los hijos documentales tampoco vale para READY: las decisiones deben versionarse antes de READY-04 (LIFECYCLE §8), con fetch o rebase y conflicto de censos con I-64 (37→38→39).
  3. Freeze §5: OV-01..07 se repiten sobre el `FINAL_CANDIDATE_SHA`, más OV-08. El margen es corto: `attempts` 7 de 9 y Codex 50 de 52.
  4. OV-I65-02 (F5) necesita su propio archivo de cargas (OD-F4-06). No debe inferirse de las filas de cargas de OV-01.
  5. Limitaciones vigentes:
     - DIF-01 se vuelve bloqueante si entra el peso propio;
     - las propiedades solo las cubre D-09;
     - la edición del catálogo es UNKNOWN;
     - la versión de RAM figura como «declared, not verified»;
     - A65-F4R-07 sigue en el producto.
  6. Host y Computer Use: portapapeles, celdas, NETLOAD, `PythonScripts` (ahora apunta a `inj-OV07b`), avisos a I-52 e I-64, privacidad de las capturas.
- **Recomendaciones:**
  - cerrar F4 con el orden del punto 8 y restituir el estado de I-65;
  - antes de F5: alcance y archivo de cargas de OV-02, presupuesto explícito y estrategia del modelo manual para READY;
  - valorar, con A-n si hace falta, la procedencia mecánica del volcado (A65-F4F-04).

## Matriz final

| OV | Ventana | Resultado | Filas del comparador o componentes no evaluados |
|---|---|---|---|
| OV-I65-01 | W1 + W2 | **PASS** | SECTION-PROPERTIES y MATERIAL-PROPERTIES = NOT_EVALUATED. Las cubre la hoja firmada D-09 (F4-D, otra clase de evidencia, ligada al blob `d088e2a9` y a las bases sin cambios). Sección: informativa (Q-O2 = b). Peso unitario: DIF-01 −0.62 %, fuera de ±0.5 %, reconocido como informativo solo sin peso propio. MANUAL-CASE-CATEGORY y SELF-WEIGHT = INFORMATIVE |
| OV-I65-03 (a) doble cara | W1 | **PASS** | — |
| OV-I65-03 (b) brazo `NegativeY` | W1 | **PASS** | — |
| OV-I65-03 (c) más de 10° | W1 | **PASS** | — |
| OV-I65-03 (d) canales pareados | W1 | **PASS** | La variante C/L es NOT_EVALUATED en el host: la autorización (§78) daba «canales pareados o C/L» como alternativa. Hay pruebas de Core para `SECTION-FAMILY-UNSUPPORTED` |
| OV-I65-03 (e) mapeo ausente | W1 | **PASS** | — |
| OV-I65-03 «fila sin verificar» | sin host | NOT_EVALUATED en el host | Cubierta por 3 pruebas de fixture en verde en Core Full. El primer smoke `745653e1` es solo histórico |
| OV-I65-04 | W1 + W2 | **PASS** | — |
| OV-I65-05 | inyección + W2 | **PASS** | El REJECTED del control no discrimina por diseño; decide la consola más la lectura de vuelta |
| OV-I65-06 | inyección + W2 | **PASS** | — |
| OV-I65-07a | inyección + W2 | **PASS** | — |
| OV-I65-07b | inyección + W2 | **PASS** | — |

FAIL: 0. INCOMPLETE: 0. OV-I65-02 (F5) y OV-I65-08 (READY) no aplican a F4.

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-F4F-01 | evidencia §79-§81; procedimiento W1 §1; validador | `expected-inputs.json` (`b3d101c1`) y el barrido K5 (`46a610df`) son hashes de bytes CRLF (copia en disco o checkout con `autocrlf`). Los blobs son LF (`1c70e490`, `1011b2a0`). El contenido es igual | OPTIONAL | Anotarlo en el cierre y, para el futuro, `-text` en la evidencia cuyos bytes se citan, sin reescribir la historia |
| A65-F4F-02 | evidencia §82 y §85 | Los hashes de los procedimientos (`a7f49707`, `abdd4c74`) son los de las versiones autorizadas (`afa10180`, `f0f0aabf`). En HEAD solo se añadió una cabecera «EJECUTADO» (diff verificado) | OPTIONAL | Registrarlo en el cierre |
| A65-F4F-03 | CD-I65-F4-198; w2-report §7 | «OV-I65-03 PASS (… fila sin verificar por fixture)» mezcla esa clase de evidencia en la línea PASS, cuando el Owner pidió «evidencia separada». No se menciona C/L | OPTIONAL | Usar en el cierre filas separadas, como en la matriz de arriba |
| A65-F4F-04 | volcado, comparador | El volcado no lleva huella ni nonce, así que la procedencia de W2 es procedimental y el comparador no enlaza el volcado con la lectura de vuelta | OPTIONAL | Para READY, decidir (Owner y A-n si cambia la plantilla) cómo ligar el volcado a la sesión |
| A65-F4F-05 | `state/I-65.yml` en HEAD | `owner_validation: required (W1 y W2 pendientes)` está desactualizado; dice «W2 EJECUTADA» en vez de ACCEPTED/PASS | OPTIONAL | Corregirlo en el commit de cierre |
| A65-F4F-06 | `payload-vs-manual-pkg-OV01.json`; w1-report §9 | La salida no registra el script (`a10d5b46`) ni el blob de la especificación. Además, «213 líneas» frente a 212 en el blob (artefacto de conteo) | OPTIONAL | Registrarlos en el cierre y en las salidas futuras |
| A65-F4F-07 | capturas W2 06 y 16 (y probablemente las demás de «Abrir») | Muestran el nombre de la cuenta y de la organización, y carpetas ancladas con el nombre de un tercero (el procedimiento W1 §6 dice que no se versionan datos personales) | OPTIONAL | No reescribir; recortar las capturas futuras a la barra de ruta y la lista de archivos |
| A65-F4F-08 | `w1_hostsnap` | El hash de la ACL depende de la página de códigos de la consola | OPTIONAL | Fijar la codificación en el script o usar el SDDL de `Get-Acl` |
| A65-F4F-09 | preflight de P2 | Las bases de RAM no se midieron en P2; su identidad se infiere de antes (prelectura y M-05c) y de después (W2) | OPTIONAL | En READY, medirlas en cada volcado de referencia |
| A65-F4F-10 | orden de la revisión; registro de cierre | Los criterios están en §7 y son AC-F4-01..11 (no §8 ni 01..09) | OPTIONAL | Citar §7 y los 11 en la determinación del gate |

## Estado de mis hallazgos previos

- **A65-F4C-01..08: CERRADOS para F4.**
  - 01: sustitución nombrada en §77 y §78; queda un matiz de redacción en F4F-03.
  - 02: se cumplen las tres condiciones (CD-I65-F4-184; READY-04 a las 02:30Z; 0 cambios fuera de `docs/`). Pasa a ser una limitación para READY.
  - 03: entradas fijas; consola más lectura de vuelta.
  - 04 y 05: verificados.
  - 06: CI custodiado con su `ref` y verificado en el JSON; la cobertura sigue sin medirse.
  - 07: paradas W1-ST y W2-ST, y comprobación de rutas y hashes en los timelines.
  - 08: lo sustituye F4F-05.
- **Anteriores:**
  - A65-F4R-06 y 08: cerrados.
  - A65-F4R-07: limitación documentada.
  - A65-F4R-02: limitación aceptada; se cumplió el volcado en la misma sesión.
  - A65-F4R-04: atendido por el procedimiento.
  - A65-F4R-10: resuelto en F4-D.
  - A65-F4R-01, 03, 05 y 09: seguimiento OPTIONAL sin efecto en W1 ni W2.
  - A65-F4D-02, 03, 04, 05 y 09: diferidos (§75).
  - A65-F4D-01: sin efecto, porque la tabla junto a la DLL es el blob LF.
  - A65-F4D-06, 07 y 08: superados por los hechos.
  - A65-F4D-10: cubierto por V-15 PASS.
  - A65-F4D-11: limitación mantenida.
  - A65-F1-06: NO CHANGE confirmado.
- Ningún REQUIRED abierto.

## No comprobado sin ejecutar

- Core y UI Full, y los CI 38977069487, 39004356219 y 39047302707: solo por la custodia, sin consultar GitHub.
- El CI de HEAD y de `bbe113f5`.
- Los archivos de D:\r65-smoke: DLL, salida, scripts de los paquetes y de las inyecciones (solo sus hashes), copia en disco del volcado manual y `Config.ini`.
- El contraste 117/117 y el validador v2, que no volví a ejecutar.
- 55 de las 64 capturas.
- El contenido de la hoja D-09.
- El RED→GREEN de F4-T1..T6, que tomé de las revisiones previas.
- Si `main` avanzó: no hice fetch.
- K1/K2 en el checkout principal.
- La cobertura del Candidato.
- La conformidad de cierre del Coordinator.
- El comportamiento real de RAM y AutoCAD fuera de lo registrado.

**Comprobación final.** Volví a leer las 10 preguntas y el formato pedido. Verifiqué cada identidad con git y `sha256sum` sobre los blobs. No escribí ningún archivo.
