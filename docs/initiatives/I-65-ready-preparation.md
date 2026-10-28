# I-65 — Preparación de READY final (READY-01..09, decisiones y Candidato final)

- **Estado:** PREPARACIÓN, sin ejecución (orden del Owner «F7 Acceptance / READY Final Preparation», [decisiones](../automation/decisions/I-65.md)
  §104). No declara READY, no fija `FINAL_CANDIDATE_SHA`, no ejecuta OV-I65-08 y no integra.
- **Sustituye para READY** a la propuesta de F7 ([I-65-ready-final-plan.md](I-65-ready-final-plan.md)), que se conserva como entregable
  cerrado de F7.
- **Autoridades:** [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §6, §8 y §9; [WORKFLOW](../WORKFLOW.md) §4.5 y §11.5;
  [AGENTS.md](../../AGENTS.md) («Pruebas — definición de terminado», «Reutilización de evidencia»);
  [guía manual](../guias/validacion-manual-autocad.md) §5-§7; Freeze [Proposal V3.1](I-65-proposal-v3.1.md) (blob `4e6e1feb…`, sin A-n).
- **Base:** F1, F3, F4, F5 y F7 con GATE PASS (F7 sobre `e8e6893c`, aceptado en §104); producto = árbol de `51fc82f1`; `origin/main` =
  `bb0d5522` (fetch del 2026-10-28T21:30:41Z).

## 1. Matriz READY-01..09

Las condiciones se satisfacen **en orden** (LIFECYCLE §8). Estado al preparar: CUMPLE, PENDIENTE (lo que falta) o NO INICIADO (exige
ejecución, que no está autorizada).

| ID | Criterio exacto (LIFECYCLE §8) | Lectura para I-65 | Estado |
|---|---|---|---|
| READY-01 | «Alcance congelado completo; diferimientos decididos por autoridad competente y A-n versionada cuando cambie Freeze» | Freeze `4e6e1feb…` sin A-n; F6 fuera (Q-O5). Diferimientos a decidir: A65-F4R-07 (limitación documentada, CD-I65-F4-168), DIF-01, edición del catálogo de RAM (DIF-04), tabla fuera del Autoloader (CD-I65-F4-10 → R-D11), ayuda integrada (F7-OBS-01 → R-D8). Si se autoriza R-D8 opción B, antes hace falta una A-n (§5) | **PENDIENTE**: decisiones R-D8 y R-D11 y confirmación del Owner de los demás diferimientos |
| READY-02 | «Gates cerrados; decisiones, A-n y documentos de producto listos y versionados» | F1, F3, F4, F5 y F7 cerrados; guía, README y entradas de FOUNDATIONS redactadas versionados. Faltan las decisiones R-D1..R-D11 versionadas y, si se autoriza código (R-D8 B o R-D11 B), su gate cerrado | **PENDIENTE** |
| READY-03 | «Sin REQUIRED abierto, discrepancia A ni decision material/Owner pendiente» | 0 REQUIRED; 0 EXP-01 A. **Decisiones del Owner abiertas:** R-D2 (forma final), R-D5..R-D8, R-D11 y **C-5** (valorar la notificación al tercero afectado y a la organización): mientras sigan abiertas, READY-03 no se cumple | **PENDIENTE** |
| READY-04 | «Fetch, preflight y rebase final completados bajo `docs/WORKFLOW.md`; si existe cierre previo se ejecuta su ruta de retorno» | Fetch fresco y preflight; hoy `origin/main` = merge-base (0 detrás): sin rebase si no cambia. Si `main` avanza (p. ej., integra I-64), rebase con evidencia nueva y conciliación de censos (R-D1). Sin cierre previo | **NO INICIADO** |
| READY-05 | «Focales/relevantes y CI propio del SHA resultante satisfacen la identidad y todos los jobs requeridos» | Focales y relevantes de I-65 sobre el SHA resultante; CI `push` con `head_sha` exacto y los 4 jobs | **NO INICIADO** |
| READY-06 | «Architect + Coordinator emiten conformidad completa `CONFORMING` de ese SHA contra Freeze/delta/A-n versionados» | Conformidad completa (LIFECYCLE §9) sobre el SHA ya rebasado, incluidas las entradas de FOUNDATIONS redactadas; la conformidad de F7 es insumo, no sustituto | **NO INICIADO** |
| READY-07 | «Arbol limpio, sin cambio ni commit pendiente para el producto; HEAD identificado» | Depende de R-D8 y R-D11 (código o no) | **NO INICIADO** |
| READY-08 | «Matriz/asignacion OV completa: todo escenario conceptual tiene unidad, toda obligacion aplicable a esta unidad esta preparada para el SHA final y ningun escenario o ultima asignacion fue retirado sin Owner» | OV-I65-01..07 y OV-I65-08 asignados a I-65 (Freeze §5) y preparados en §7 y en el [procedimiento de OV-I65-08](I-65-ov08-procedure.md). Reutilizar los volcados manuales (R-D2) reduce OV-01/02: exige la autorización formal del Owner | **PREPARADO**; pendiente la autorización formal de R-D2 |
| READY-09 | «Acuerdo exacto, diff permitido, identidad/unicidad/inmutabilidad de Freeze, compatibilidad de fuente V1 y secuencia append-only, alcance y visibilidad de todas las A-n comprobados conforme a §6» | Hoy: Freeze único e intacto (blob `4e6e1feb…`), sin A-n, diff desde el Candidato F5 solo en `docs/` y `README.md`. Se repite sobre el SHA final, con la A-n de R-D8 si existiera | **NO INICIADO** (comprobación preliminar sin hallazgos) |

**Después de READY-09:** se fija `FINAL_CANDIDATE_SHA` y sobre ese SHA exacto se produce la evidencia final y la Owner Validation (§7).
Cualquier corrección que cambie el SHA reinicia READY-02; tras READY-04, cualquier A-n o decisión nueva crea SHA nuevo (LIFECYCLE §6).

## 2. Texto y opciones exactas de R-D5 a R-D8

No se cierran sin decisión expresa del Owner.

### R-D5 — Reglas operativas para las ventanas de READY (A65-F5F-03, A65-F5F-04, A65-F5F-05)

> **Texto propuesto:** «Para las ventanas de READY se adoptan las reglas siguientes: (a) identidad de las bases de RAM: [opción];
> (b) contraste lectura de vuelta ↔ volcado: [opción]; (c) proceso de RAM por ventana y tras un STOP de licencia: [opción].»

| Parte | Opción 1 (recomendada) | Opción 2 |
|---|---|---|
| (a) A65-F5F-03 | Un solo método para los árboles de bases de RAM: el de PowerShell de W4 (`w2_hostsnap.ps1`), en PRE y POST de la misma ventana; el valor del anexo de F5 (método Python) queda histórico | Conservar ambos métodos con una tabla de conciliación por ventana |
| (b) A65-F5F-04 | El contraste se ejecuta **después** de cerrar RAM (como el anexo operativo §3) | Se permite con RAM abierto y antes del modelo manual, porque su salida es solo veredicto y conteos (como en W4) |
| (c) A65-F5F-05 | Proceso de RAM **nuevo** por ventana; tras un STOP de licencia se cierra RAM antes de terminar la ventana (`ram=0` comprobado) | Se permite reutilizar el proceso abierto si el Owner autoriza la reanudación (precedente de W4) |

### R-D6 — Codificación del script de captura del host (A65-F4F-08)

> **Texto propuesto:** «Antes de la próxima ventana de AutoCAD, la captura de la ACL de `AcadLocation`: [opción].»

| Opción | Contenido | Coste |
|---|---|---|
| 1 (recomendada) | Corregir la herramienta del Coordinator (fuera del producto): ACL por `Get-Acl … .Sddl`, independiente de la página de códigos; versionar su nuevo SHA-256 y medir BASELINE y POST con esa versión | sin Codex; sin cambio de producto |
| 2 | Mantener el script; BASELINE y POST por el mismo canal y la misma página de códigos, con la conciliación documentada (como W1 y W3) | ninguno; conserva la fragilidad |

### R-D7 — C-5: notificación por la exposición de información de terceros en capturas históricas

> **Texto del punto abierto:** «Valorar la notificación al tercero afectado y a la organización según la política interna»
> ([I-65-f5-scope-and-ready-review.md](I-65-f5-scope-and-ready-review.md) §5; decisiones §85, CD-I65-F4-225: de 30 capturas de la rama con
> datos identificables, 12 llevan el nombre de una tercera persona asociado a carpetas comerciales, además de datos de cuenta y organización;
> repositorio público aceptado por el Owner en CD-I65-F5-31, sin reescritura de historial).

| Opción | Contenido | Quién actúa |
|---|---|---|
| A | El Owner valora y registra que **no procede notificar** (riesgo residual aceptado) | Owner |
| B | El Owner **notifica** al tercero afectado y a la organización según la política interna; el Coordinator solo registra el hecho, sin contactar a nadie | Owner |
| C | B y, además, **redacción hacia delante** (plan C-2): copias redactadas en un commit nuevo, sin borrar ni reescribir los originales históricos | Owner autoriza; Coordinator ejecuta solo documentación |

El historial no se reescribe en ninguna opción (C-3 = NO). **Recomendación:** decisión reservada al Owner; READY-03 exige que esté tomada.

### R-D8 — Ayuda integrada `RACKAYUDA` sin `RACKRAMPAQUETE` (F7-OBS-01)

> **Texto propuesto:** «La ayuda integrada: [opción].»

| Opción | Contenido | Freeze | Coste |
|---|---|---|---|
| A (recomendada) | Seguimiento en `docs/ideas-futuras.md` para una unidad posterior; el README y la guía ya documentan el comando | sin cambio | 0 |
| B | Entrada **neutra y sin alias** antes del Candidato final (§5) | A-n (comportamiento observable de otro comando, fuera del Freeze) | 1 Worker, +1 Codex, +1 `attempts`, gate previo a READY y SHA nuevo |
| C | Entrada que nombre «RAM Elements» o con alias | A-n que además cambia la lista de permiso de G1 (ADR-0050 Decisión 4) o el comando mínimo de D-13, la guarda C02 y el censo de comandos (más conflicto con I-64) | como B, más riesgo; no recomendada |

## 3. Estado de R-D1 a R-D11

| ID | Estado | Detalle |
|---|---|---|
| R-D1 | **PREFERENCIA DEL OWNER: I-65 antes de I-64**, condicionada | I-64 (`architecture/workspace-persistente-rackcad`, `09f2f1c3`, último commit 2026-10-13; reintento de Smoke-1 pendiente) no está integrada y comparte con I-65 solo `docs/ROADMAP.md` y dos pruebas de censo. Con las reglas de integración (manual y serializada, WORKFLOW §4.5 y §11.5), integrar I-65 primero no exige cambios en I-65 y deja la conciliación de censos (38→39) a I-64. **Condición que hay que comprobar en READY-04:** que I-64 no haya integrado ni haya pedido integrar antes; si lo hubiera hecho, rebase y conciliación (código: Worker, Codex y `attempts`) |
| R-D2 | **PREFERENCIA DEL OWNER: reutilizar los volcados manuales custodiados** si identidades y especificaciones coinciden | Condiciones de reutilización en §7.3. No se reutilizan lecturas de vuelta ni volcados del generado: esos se producen de nuevo con el DLL final. Es una reducción de OV-01/02 que el Freeze §5 reserva al Owner: se registra como autorización formal antes de READY-08 |
| R-D3 | **DECIDIDO (preliminar): opción 0** | Trazabilidad externa de sesión (registro de PID y tiempos más contraste), declarada no criptográfica |
| R-D4 | **DECIDIDO (preliminar)** | La construcción manual con Computer Use cuenta como evidencia de equivalencia entre mecanismos de entrada, no como validación independiente de ingeniería |
| R-D5 | **ABIERTO** | Opciones en §2 |
| R-D6 | **ABIERTO** | Opciones en §2 |
| R-D7 | **ABIERTO** (C-5) | Opciones en §2 |
| R-D8 | **ABIERTO** | Opciones en §2; evaluación en §5 |
| R-D9 | **PROCEDIMIENTO PREPARADO, ejecución NO autorizada** | [I-65-ov08-procedure.md](I-65-ov08-procedure.md) |
| R-D10 | **DECIDIDO (por ahora):** Codex 50/52, `attempts` 7/9 | Suficiente solo si R-D8 = A y R-D11 = A (§8) |
| R-D11 | **EVALUADO, implementación NO autorizada** | §4 |

## 4. Evaluación de R-D11 — la tabla de secciones y materiales y el Autoloader

1. **¿Es el Autoloader un mecanismo de distribución soportado?** **Sí.**
   - [despliegue.md](../guias/despliegue.md) §4 lo presenta como «Carga automática (Autoloader bundle — **recomendado**)».
   - Tiene herramientas de producto: `deploy/build-bundle.ps1`, `verify-bundle.ps1`, `install-bundle.ps1` y sus pruebas.
   - El CI lo verifica: `eng/ci/verify-autocad-references.ps1` publica y ejecuta `deploy/verify-bundle.ps1` y su arnés
     `deploy/test-verify-bundle.ps1` en el job «Build Plugin without AutoCAD».
   - Hoy, un usuario que instala RackCad por la vía recomendada obtiene `Unavailable` en `RACKRAMPAQUETE`.
2. **¿Exige el Freeze que `RACKRAMPAQUETE` funcione desde el Autoloader?** **No.**
   - El Freeze y ADR-0050 no regulan la distribución. D-07/D-09 piden una tabla de datos versionada y verificada.
   - D-08/D-13 piden que el comando falle cerrado y de forma visible, y eso se cumple: `Unavailable` con diagnóstico.
   - La validación manual usa el DLL Debug de la salida de compilación (guía manual §2), donde la tabla sí está.
   - CD-I65-F4-10 difirió el Autoloader como riesgo residual porque la ruta no estaba autorizada.
   - La carencia es de **completitud de producto para el usuario final**, no de conformidad con el Freeze.
3. **Cambio mínimo:**
   - (a) target `AssembleAutoloaderBundle` de `src/RackCad.Plugin/RackCad.Plugin.csproj`: copiar
     `assets/structural-analysis/ram-elements/section-material-map.json` a `Contents/structural-analysis/ram-elements/`, la ruta que el
     comando busca junto al DLL;
   - (b) `deploy/verify-bundle.ps1`: admitir **exactamente** esa ruta en la lista de permiso fail-closed y exigir que su contenido sea
     idéntico byte a byte al de `assets/` (como `Assert-CatalogsMatchSource`);
   - (c) `deploy/test-verify-bundle.ps1`: casos nuevos (bundle válido con la tabla; tabla ausente; tabla modificada; archivo extra bajo
     `structural-analysis/`);
   - (d) sustituir la prueba `LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca` por su positiva (el target copia la tabla
     y el verificador la exige);
   - (e) documentación: despliegue §2 y §4, guía de I-65 §2 y §11, README y entrada «Provider Boundary» (desaparece la limitación).
   - Sin cambios en `src/RackCad.Application`, en el comando ni en el script de RAM.
4. **Pruebas y regresiones:**
   - RED→GREEN de la prueba sustituida y de los casos nuevos de `test-verify-bundle`;
   - Core Full y UI Full locales; builds Debug;
   - `dotnet publish` y `deploy/build-bundle.ps1` en local con AutoCAD cerrado, más `deploy/test-verify-bundle.ps1` y
     `deploy/test-install-bundle.ps1`;
   - CI con el job «Build Plugin without AutoCAD», que vuelve a ejecutar la verificación del bundle;
   - en la Owner Validation, opcionalmente, un smoke de `RACKRAMPAQUETE` cargado desde el bundle. Eso instala en `ApplicationPlugins` del
     usuario y exige una autorización aparte.
5. **¿A-n?** **No es necesaria por LIFECYCLE §6:**
   - la distribución no es un elemento congelado;
   - el comportamiento del comando, su semántica de fallo, la tabla y los oráculos no cambian;
   - el contrato de ADR-0003 (cero DLL de Autodesk y los cuatro DLL de RackCad) se conserva, porque se añade un archivo de datos;
   - la guarda G1 no se ve afectada (examina `.cs` bajo `src/`).

   Sí exige: (i) **decisión del Owner** que revierta el diferimiento de CD-I65-F4-10, porque toca el contrato compartido del bundle (I-12)
   y su guía; (ii) un registro versionado; (iii) un **gate previo a READY** con contrato de Worker, RED→GREEN, revisión del Architect y
   cierre con Core Full y CI.
6. **Impacto en SHA y presupuesto:**
   - SHA de producto nuevo antes del Candidato final (el producto deja de ser el árbol de `51fc82f1`): todo el Candidato final y OV-01..08
     se ejecutan sobre ese SHA, que es lo que ya exige el Freeze;
   - **+1 Codex, +1 `attempts`, 1 Worker**, con lo que quedarían 51/52 y 8/9, sin margen para un REWORK si se combina con R-D8 B;
   - sin conflicto con I-64: I-64 no toca `deploy/`, el `.csproj` del Plugin ni la prueba sustituida.

**Opciones para el Owner:** **A** mantener el diferimiento como limitación de V1, declarada en la guía, el README y FOUNDATIONS (coste 0).
**B** autorizar el cambio mínimo en un gate previo a READY (coste del punto 6). **Recomendación del Coordinator:** B si RackCad se va a
distribuir por el Autoloader a usuarios que necesiten el comando (es la vía recomendada); A si el uso previsto en V1 es solo de ingeniería
con el DLL de compilación.

## 5. Evaluación de `RACKAYUDA`

- **Cambio mínimo** (opción B de R-D8):
  - una línea en `src/RackCad.UI/RackCommandReference.cs`: `new RackCommandInfo(<grupo>, "RACKRAMPAQUETE", "", <resumen neutro>)`, en el
    grupo «Reportes» o uno nuevo;
  - la fila espejo en [despliegue.md](../guias/despliegue.md) §6.
- **Restricciones que lo definen:**
  - **G1** (guarda de proveedor) examina `src/RackCad.UI/` y **no** excluye cadenas: el resumen no puede contener «RAM Elements»,
    «RamElements», «ramelements» ni `Ram[A-Z]…`. Sirve un texto neutro: «Exportar una estación Cantilever como paquete para un programa de
    análisis estructural externo (sin análisis ni resultados)». El nombre `RACKRAMPAQUETE` no activa G1.
  - **Sin alias:** el comando se registró **sin alias** (D-13; censo de 38 comandos en `SelectiveEditorOpenTests`). La guarda C02 exige
    exactamente un `[CommandMethod(` en el archivo del comando. Añadir un alias cambiaría el censo (más conflicto con I-64) y la guarda C02,
    o pondría `RackRamElementsCommands` en otro archivo, lo que viola G1.
  - El censo de la ayuda (`CustomPropertiesHelpCensusTests`) exige que **cada comando y alias** esté registrado en el Plugin. Una entrada
    sin alias obliga a adaptar esa prueba para aceptar un alias vacío, y la ventana (`RackCommandHelpWindow`) debe mostrarlo vacío o «—».
- **Pruebas:**
  - RED→GREEN en `RackCad.UI.Tests`: censo de la ayuda (14 de apertura + G7 + G15 + I-65), mutaciones (falta la entrada, alias inesperado,
    entrada duplicada), «cada comando registrado» con alias opcional y resumen sin tokens de G1;
  - G1 del Core sigue en verde; UI Full; Core Full; builds; CI.
- **Encaje con el Freeze:** la ayuda es el comportamiento observable de **otro** comando, que el Freeze de I-65 no cubre, así que
  incorporarla a I-65 exige una **A-n** (LIFECYCLE §6: no es una A-n Coordinator-only, porque añade comportamiento; requiere Architect y
  Coordinator y la aceptación del Owner). Con un texto neutro y sin alias no toca ADR-0050 ni D-13. Con «RAM Elements» o con alias,
  tocaría la lista de permiso de G1 (ADR-0050 Decisión 4) o el comando mínimo de D-13: no recomendado.
- **Coste:** igual que R-D11 (+1 Codex, +1 `attempts`, 1 Worker) y SHA nuevo. Combinado con R-D11 en un solo contrato de Worker, el
  consumo es el mismo si el primer intento pasa, pero queda sin margen.

## 6. Procedimiento propuesto de OV-I65-08

En [I-65-ov08-procedure.md](I-65-ov08-procedure.md): checklist de la guía manual (§5) acotado a I-65, con guardado, cierre y reapertura de
un DWG desechable, persistencia, `RACKEDITAR`, vistas, BOM y una segunda exportación que debe reproducir la misma huella de entrada. **No
autorizado para ejecutar.**

## 7. Plan del Candidato final

### 7.1 Estrategia

1. **Decisiones antes del Candidato:** el Owner decide R-D1..R-D11 y C-5. Si R-D8 B, primero su A-n (Architect + Coordinator, aceptación
   del Owner). Todas se versionan **antes** de READY-04.
2. **Código, solo si se autoriza** (R-D8 B o R-D11 B): un **gate previo a READY** con contrato de Worker, RED→GREEN, Core Full, CI, revisión
   del Architect y GATE PASS. Sin código: no hay gate.
3. **READY-01..09 en orden** (§1). READY-06 es la conformidad completa de Architect y Coordinator sobre el SHA ya rebasado.
4. **`FINAL_CANDIDATE_SHA` propio:** es un commit que **contiene** todas las decisiones, la A-n si existe, los documentos de producto y el
   registro de READY-01..09. **No se reutilizan** la excepción documental de F4 (CD-I65-F4-178/184, lectura de commits hijos) ni la
   disposición A65-F5C-01 de F5. Después de ese SHA solo se versiona evidencia de ese SHA exacto (AGENTS.md). Una decisión nueva después
   de READY-04 crea un SHA nuevo y reinicia READY-02.
5. **Evidencia y Owner Validation** sobre ese SHA (§7.2 y §7.3). Integración: orden aparte del Owner.

### 7.2 Pruebas que se repiten sobre `FINAL_CANDIDATE_SHA`

| Clase | Contenido |
|---|---|
| Local, después del commit y con el árbol limpio | Core Full; UI Full (omitidas declaradas); builds Debug de UI y Plugin (DLL en una carpeta nueva `D:\r65-smoke\<sha>\`, ProductVersion `1.0.0+<sha>`); build de la herramienta; SDK resuelto. Con R-D11 B, además `deploy/build-bundle.ps1` y `deploy/test-verify-bundle.ps1` |
| CI | `push`, `ref` y `head_sha` exactos, 4/4 jobs (con `ui-tests`), leído |
| Bloque §7.1 de la guía manual | SHA, árbol limpio, SDK, Core, UI, builds, CI, Owner validation, biblioteca de bloques (ruta resuelta y SHA-256) y versión de AutoCAD |
| OV-I65-01..07 | Todas de nuevo con paquetes generados por el DLL final (Freeze §5; la guía manual exige el mismo SHA para reutilizar una validación) |
| OV-I65-08 | Checklist completo ([procedimiento](I-65-ov08-procedure.md)) |
| Controles | K1/K2 por ventana, custodia de la opción 0 (R-D3), privacidad de capturas (§90), comparación de hashes con los blobs antes de cada commit de evidencia |

### 7.3 Ventanas propuestas (sin ejecutar)

| Ventana | Escenarios | Duración estimada |
|---|---|---|
| W-R1 AutoCAD | OV-01 (paquete de REF-CANT-01), OV-02 (paquete con las cargas `d675d9f6…`), OV-03 (a)-(e), OV-04 (paquete), generación de inyecciones para OV-05..07 con la herramienta, **OV-08** | 2.5-3 h |
| W-R2 RAM | OV-01 y OV-02 (construcción, `READBACK OK`, volcado del generado y comparador contra el volcado manual reutilizado), OV-04 (negativa sobre modelo no vacío), OV-05..07 (inyecciones) | 1.5-2 h (con R-D2) |

**Condiciones de reutilización de los volcados manuales (R-D2):**

- **Identidades sin cambios:** RAM `7dcb4855…`, API `ramelements` 0.1.0 (árbol `f71a017f…`), bases de RAM (con el método de R-D5 (a)) y
  plantilla de volcado.
- **Especificaciones sin cambios:** REF-CANT-01 (Freeze §15), hoja de OV-02 (blob `6dc0d8da…`) y archivo de cargas `d675d9f6…`.
- **Integridad:** los volcados conservan su SHA-256 custodiado (`c4a98fb0…` para OV-01, `12c89479…` para OV-02).
- **Si cualquier identidad cambia:** construcción manual nueva (opción C de R-D2) con la hoja aprobada y Computer Use (R-D4).
- **Nunca** se reutilizan lecturas de vuelta, volcados del generado ni paquetes.

## 8. Presupuesto y riesgos

| Escenario | Codex (hoy 50/52) | `attempts` (hoy 7/9) | Workers | Architect | Host |
|---|---|---|---|---|---|
| R-D8 A + R-D11 A (recomendado si no se distribuye por el Autoloader) | 0 | 0 | 0 | READY-06 (+1 relectura) | W-R1 2.5-3 h; W-R2 1.5-2 h |
| R-D8 B o R-D11 B (uno) | +1 → 51/52 | +1 → 8/9 | 1 | + revisión del gate previo | igual |
| R-D8 B y R-D11 B (un contrato) | +1 (sin margen para REWORK) | +1 (sin margen) | 1 | igual | igual; con R-D11 B, opcionalmente un smoke del bundle |
| R-D1 si I-64 integra antes | +1 | +1 | 1 | + relectura | igual |

**Recomendación de presupuesto:** con cualquier código autorizado, ampliar los topes (p. ej., Codex 54 y `max_attempts` 11) para conservar
un REWORK de margen. Sin código, R-D10 basta.

**Riesgos:**

- la licencia de RAM depende del inicio de sesión del Owner en Bentley CONNECTION;
- un cambio de RAM, de su API o de sus bases invalida R-D2 y obliga a la construcción manual;
- si `main` avanza antes de READY-04 (p. ej., I-64), habrá rebase, evidencia nueva y quizá conciliación de censos con código;
- C-5 abierto bloquea READY-03;
- OV-08 guarda y reabre un DWG (autorización expresa; DWG desechable fuera del repositorio);
- las limitaciones de V1 siguen: equivalencia de transferencia, no validación estructural; propiedades NOT_EVALUATED; peso propio
  excluido; DIF-01; edición del catálogo de RAM UNKNOWN; A65-F4R-07.

## 9. Decisiones que requieren autorización del Owner

1. **R-D5** (a), (b) y (c), **R-D6**, **R-D7 (C-5)** y **R-D8** con las opciones de §2.
2. **R-D11**: A (diferir) o B (gate previo con el cambio mínimo de §4).
3. **Autorización formal de R-D2** (reducción de OV-01/02 con las condiciones de §7.3) y confirmación de R-D1, R-D3 y R-D4.
4. **Presupuesto** (R-D10) si se autoriza código.
5. **Inicio de READY-01..09** (ejecución), y después la declaración de `FINAL_CANDIDATE_SHA`.
6. **Ventanas W-R1 y W-R2**, incluido **OV-I65-08** con el guardado y la reapertura del DWG desechable (R-D9).
7. **Integración** (orden aparte, después de la Owner Validation).
