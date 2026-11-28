# I-64 Smoke-1 R20261104T194513Z-a578 — COORDINATOR_SMOKE1_REVIEW_REQUIRED

Candidato cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6 (producto 0e0d536b10350cc85e4a932a0d576b22fae22fa9). HEAD = remoto; árbol limpio;
sin commits ni recompilación durante la ocurrencia. **Propuesta de la sesión: STOP** (SM-STOP-02, causado por la propia sesión), sin PASS
ni NOT PASS. F2 no empieza. Sin merge ni rebase de main.

## STOP SM-STOP-02 (causa: la sesión)
- A las 22:34:52Z la sesión contó **dos acad.exe**: pid 46292 (inicio 22:14:14.884Z, la del Owner con el Candidato, la operada) y pid
  9640 (inicio 22:20:52.493Z).
- La 9640 la lanzó la sesión: al tomar el control (~22:20:40Z) su primer clic falló porque el escritorio estaba al frente, y llamó a
  `open_application("AutoCAD 2025 - English")`. La herramienta abrió una instancia nueva en vez de enfocar la existente.
- La sesión no contó procesos entre SM-00d1 (22:18:34Z, acad = 1) y las 22:34:52Z.
- La 9640 no tenía RackCad y nunca se operó: RACKPANEL y la paleta solo existían en la 46292.
- T3D §11 SM-STOP-02 («otra `acad.exe` … durante la ventana»): no se siguió la secuencia (SM-27 … SM-35 NOT_EXECUTED). Se cerraron las
  dos sin guardar, y el Owner tomó el POST.

## Desviaciones
1. **Rol (nueva):** el Owner operó SM-00a, SM-00b y SM-00d. Desde SM-00e operó la SESIÓN, por orden literal del Owner: «De ahí continúa
   tú tomando el control, no cuestiones mi orden». Va contra T3D §1 y contra la autorización: «It must NOT operate AutoCAD instead of the
   Owner. If the session operates AutoCAD: STOP and return for Coordinator disposition». No se reutiliza la aceptación de 0a77.
   Registro: disposition/owner-order-session-operator.md.
2. **Segunda acad.exe (sesión):** la causa del STOP.
3. Después del POST el Owner escribió: «Avanza todo lo que puedas, si es necesario utiliza tú AutoCAD sin solicitar permisos ya que no
   estaré». La sesión no abrió AutoCAD, porque nada pendiente lo necesita y el STOP impide seguir la secuencia
   (disposition/owner-message-after-stop.md).
4. **Firma:** el Owner firmó el registro del operador a las 04:47:56Z, literal «Firmo el registro.» (disposition/owner-signature.md).
   La firma no cambia la propuesta ni decide la disposición.

## Integridad del Candidato (SM-00d1)
VERDICT = PASS (22:18:34Z): un solo RackCad.Plugin.dll,
`<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`, SHA-256
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0, más RackCad.Application.dll 12B8E237…27B3 MATCH.
- El Owner no informó las opciones del aviso de seguridad ni la opción elegida (SM-00d).
- Estado final: TRUSTEDPATHS y SECURELOAD iguales, y ningún registro de carga.

## Comprobaciones críticas
| Paso | Resultado | Observado |
|---|---|---|
| SM-03 (C1) | **PASS** | flotante dentro de la pantalla, arriba a la izquierda, visible y usable, sin rescate |
| SM-17 (C1) | **PASS** | acoplada izquierda / derecha, flotante dentro de la pantalla, ocultación automática (se pliega a «RACKCAD» y se despliega con clic); sin errores |
| SM-13 (C2) | **PASS** | con `LINE`: casillas desmarcadas y «Hay un comando en curso.»; ninguna fila drain/selection.read durante la orden (36 → 36 a ~2 s y ~7 s); con `Esc` vuelven las casillas (37 selection.read, 38 drain) |
| SM-26 (C2) | **PASS** | «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa»; casillas desmarcadas; RackLog sin entradas (ninguna NRE de `DocumentActivated`) |
| SM-27 (C2) | **NOT_EXECUTED** | STOP antes del paso; el caso de la pestaña Start con C abierto (2.ª NRE de 0a77) no se ejercitó |
| SM-34 (C2) | **NOT_EXECUTED** | STOP |
| SM-39 | **PASS hasta el STOP** | RackLog sin ninguna entrada nueva (sin rackcad-20261104.log); releído tras el POST: igual |

El resto: SM-00a, SM-00b, SM-00c, SM-00e, SM-01 (ms=172), SM-02, SM-04 … SM-12, SM-14a..e (complementario), SM-14h, SM-15, SM-16, SM-18,
SM-20 … SM-25 = PASS. SM-00d = PASS indirecto (opción no informada). SM-19 = NOT_APPLICABLE (PICKFIRST 1). S1-06a..e =
NOT_APPLICABLE_BY_DESIGN. SM-28 … SM-33 y SM-35 = NOT_EXECUTED. SM-36 … SM-38 = PASS hasta el STOP. SM-40 = cierre normal sin guardar.
SM-41 = PASS.

## Host
| Elemento | Antes | Después |
|---|---|---|
| acad.exe / RAM | 0 / 0 (22:05:31Z, 22:06:37Z; preflight 22:10Z) | 0 / 0 (22:36:54Z; 00:39:14Z; 00:44:10Z) |
| BASELINE T3C | código 0; CanonicalSha256 690d6926055faed403688ff204e8da429fb25d3b84060feecf48985eb82ef017; archivo 2ca0614c…3a9f | — |
| POST T3C | — | código 0 (2026-11-05T00:38:54Z); CanonicalSha256 d651ddf472ddbdcc65d7c45a34b2669fbb1e4698e7773b51586fe79a6a8ac7b2; archivo be629a64…4bff |
| DIFF T3C (PKA = ACCEPTED) | — | **CLEAN**, código 0: ALLOWED 2 (R-A-03: Application Window\Position CHANGED; Variables\VIEWBACKSTATUS REMOVED); REPORT 0; FORBIDDEN 0; INVALID 0; diff 1edd3f56…d7aa |
| SECURELOAD / TRUSTEDPATHS / perfil / acad | 1 / a9ba0fd1…2498 / <<Unnamed Profile>> / 2A75996F… MATCH | sin cambios (DIFF CLEAN) |
| AssemblyMap | 2 valores, RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z | idéntico |
| CoreUser | 57 entradas | idéntico |
| HOST-EXTRAS | BEFORE f15fa6498449fedc3931673062c3b9cdb56d9835c03b8eb740383176e36db34d | AFTER ccc8cd8777242d6e854b103eae1ee2fd992a8c7f086095d6a9cb44de109b33c3; DIFF REPORT_REQUIRED (código 4) |

**HOST-EXTRAS:** el REPORT_REQUIRED sale solo de la clave del producto, en dos LASTWRITE_ONLY con valores idénticos:
- `Profiles\<<Unnamed Profile>>\Dialogs\Select File`: 2026-11-03T20:22:36.958Z → 22:33:00.077Z;
- `Recent File List`: 22:01:55.980Z → 22:36:41.644Z.

No hay ninguna creación ni cambio de RackCad en AssemblyMap, Applications ni Loaded, y el historial de NETLOAD no cambió. Por la regla
de 0a77, un cambio solo de MRU es informativo, así que la sesión lo lee como **INFORMATIVE**.

**Limitación del POST (para la decisión del Coordinator):**
- La última instancia en salir fue la 9640. La 46292 salió entre 22:36:22Z y 22:36:36Z; la 9640, entre 22:36:40Z y 22:36:54Z.
- `Recent File List` se escribió a las 22:36:41.644Z y quedó con los valores de la BEFORE, aunque en la 46292 se abrieron D, C, A y B. En
  0a77, con una sola instancia, esa clave sí cambió.
- Inferencia de la sesión: al salir, la 9640 reescribió el estado de interfaz que leyó al arrancar.
- El DIFF CLEAN prueba el estado final del host. No excluye por sí solo valores que la 46292 escribiera al salir y que la 9640 también
  reescribe.

**Rutas:** `<carpeta-de-I-52>` y `<carpeta-local-de-I-65>` = UNKNOWN_EXTERNAL_HOST_CHANGE, Attribution NONE, NON-BLOCKING. No se tocaron.

**Privacidad:** los JSON de HOST-EXTRAS guardan los valores crudos del registro. Son locales: artifacts/ está en .gitignore. Este
paquete no cita rutas locales salvo `<worktree>`.

## I-52
- Aviso antes de la BASELINE: 22:07:53Z (msg 040e0ac2).
- Aviso tras el POST y petición de la recomprobación posterior en solo lectura: 2026-11-05T00:43:04Z (msg 5112203c), en
  disposition/i52-notice-after-post.txt.
- **Recomprobación recibida** a las 00:43:29Z (disposition/i52-post-run-recheck.txt, literal): «SIN DERIVA de lo que I-52 registró. No
  concedo ACK/RELEASE y esto no autoriza P2 H10.»
  - acad = 0 y RAM = 0.
  - acad 2A75996F… igual.
  - TRUSTEDPATHS: 19 entradas, a9ba0fd1… igual. SECURELOAD 1. Perfil <<Unnamed Profile>>.
  - Sin RackCad en Applications, InstalledApplications, Loaded, Applications del perfil, HKLM, AssemblyMap ni ApplicationPlugins.
  - CoreUser RackCad sin cambio.
  - P2: tip fb6b5648, 0 discrepancias.
  - `<carpeta-de-I-52>` y `<carpeta-local-de-I-65>` siguen sin existir, así que no se pueden comparar las ACL de sus rutas ni H10.
  - H10 sigue NOT_AUTHORIZED por motivos propios de I-52.
- **Dato nuevo de I-52: HKCU `Applications` con LastWrite 22:36:25.592Z, dentro de la ventana.**
  - Lectura de la sesión, solo lectura, en host-extras/applications-lastwrite-session.txt: lo que se reescribió es
    `Applications\AcMr` («AutoCAD Action Commands», LOADER en la carpeta de AutoCAD 2025), con Commands, Groups y MRU a las
    22:36:25.596–.598Z, durante la salida de la pid 46292.
  - El contenido es idéntico: el libro T3C registra esas claves y valores, y el DIFF no tiene ninguna entrada en Applications
    (R-F-05 no se dispara). Loaded e InstalledApplications no cambian.
  - Propuesta de la sesión: INFORMATIVE (reescritura de un módulo de AutoCAD al salir, sin cambio de contenido y sin RackCad).
  - I-52 escribe que I-64 «ya clasificó (ALLOWED)» esa escritura. **No es así:** la sesión no la había clasificado. No hay medida de
    esta fecha en 0a77. Lo decide el Coordinator: regla de 0a77, «an unexplained load-state change = STOP».

## Evidencia (transitoria, sin custodia hasta orden del Coordinator)
authorization/, kit/ (generado del de 0a77 con sustituciones contadas), preflight/, host-extras/, profile/, console/{baseline,post,diff}.txt,
process/inspect-after-netload.txt, logs/racklog-BEFORE.txt, diff/{profile-diff.json(.sha256),host-extras-diff.json},
operator/{operator-guide.md, session-log.md, operator-record.md (firmado por el Owner)}, disposition/* (incluye owner-signature.md).

## Decisiones para el Coordinator
1. Disposición de la ocurrencia: STOP propuesto (SM-STOP-02 causado por la sesión) frente a cualquier otra lectura.
2. Desviación de rol nueva (la sesión operó por orden del Owner).
3. Valor probatorio de los resultados observados hasta el STOP: C1 (SM-03, SM-17) y C2 (SM-13, SM-26) PASS con dos acad.exe vivas.
4. Lectura de la limitación del POST, del HOST-EXTRAS REPORT_REQUIRED (solo LASTWRITE_ONLY de MRU) y de la LastWrite de HKCU
   `Applications`/`AcMr` (22:36:25Z, contenido idéntico).
5. Para una repetición, que necesita RunId nuevo y autorización: rol del operador; medida contra una segunda instancia (contar procesos
   tras cada cambio de foco y no usar `open_application` con AutoCAD abierto).
