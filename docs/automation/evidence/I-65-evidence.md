# I-65 — Evidencia de la unidad (Structural Analysis Interop Foundation + RAM Elements Adapter)

Unit / Initiative / Workflow: `I-65` / I-65 / V2. Claim-Id `0b4aad79-068e-4a57-a2c2-dc8f23406778`.
Contrato: [I-65-interop-analisis-estructural.md](../../initiatives/I-65-interop-analisis-estructural.md).
Decisiones: [I-65.md](../decisions/I-65.md).

Estado de esta evidencia: G0 (reclamo y bootstrap). Las secciones son registros de su momento y no se reescriben
([WORKFLOW](../../WORKFLOW.md) §11.4). Los hechos posteriores al commit que publica este archivo (su CI exacta, el RELEASE de la
ventana) se registran en un commit posterior: un archivo no puede citar el SHA del commit que lo contiene.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `bb0d5522e8411f66a51fdfb3f1f0d0514b737453` (recién obtenida antes del reclamo; coincide con el baseline del mandato) | `git fetch` + `git rev-parse origin/main` |
| Commit de reclamo | `32a8475689c9ef8a44837576f3c6f61abd34b07a` (vacío; 2026-10-10T14:58:09-06:00) | `git log` |
| Primer push aceptado (reclamo) | `architecture/interop-analisis-estructural` nueva en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/interop-analisis-estructural` / `%USERPROFILE%\.codex\worktrees\architecture-interop-analisis-estructural` (fuera del árbol versionado; distinto del principal del Owner, que no se tocó) | `git worktree list` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364` (merge que activa V2; el trailer `Workflow-V2-Normative: I-56` está en `afd1077c`); ancestro de la base | `git log --grep`, `git merge-base --is-ancestor` |
| Clasificación | **V2, T4** (unidad nueva posterior, base con el SHA efectivo, sin pausa). T7/T8 evaluados negativos: no consume diseño V1 de I-49/I-52/I-55 ni pertenece a ellas | [WORKFLOW](../../WORKFLOW.md) §11.3 |

## 2. Fuentes recibidas

| Fuente | Bytes | SHA-256 de transporte | Nota |
|---|---:|---|---|
| Mandato del Owner (pegado en el chat de la sesión Coordinator, 2026-10-10T20:39:21Z) | 25 549 | `AE1A04127AA3E9E9E8AA7B26D2329DFD48A67A9DE9B1F285B51EAEB106569E56` | Extraído de la transcripción local (contenido entre las etiquetas de texto pegado, sin las etiquetas), no de una copia transcrita. UTF-8 sin BOM, solo LF, sin LF final. Versionado como [`I-65-owner-mandate.original.txt`](../decisions/I-65-owner-mandate.original.txt) |

**Blob de Git del mandato:** la identidad durable es el blob versionado. Con `core.autocrlf=true`, un checkout en Windows puede convertir
el archivo a CRLF en disco; el SHA-256 del blob (LF) es el que se compara con el de transporte.

## 3. Preflight (2026-10-10, 20:39Z–20:58Z)

- `origin/main` = `bb0d5522…`, igual al último conocido por el Master Orchestrator. `main` local del worktree humano: `95690c28`
  (90 commits detrás, a propósito; no se tocó).
- Ramas remotas: `main`, `architecture/portabilidad-coordinador-principal` (I-62, suspendida), `architecture/workspace-persistente-rackcad`
  (I-64 / ID30) y `feature/rackmirror-espejo-semantico` (I-52). Tags `integration/*` hasta I-63; ningún tag ni rama de I-65.
- `git log --all --grep='I-6[5-9]'`: vacío. Ningún archivo `docs/**/I-65*` en `origin/main` ni en las ramas remotas. Carpetas de paquete
  `D:\IDs`: I-62, I-63, I-64 (sin I-65). → **I-65** (CD-I65-G0-01).
- Sesión Coordinator: `claude-opus-5-5` / `xhigh` (`get_session("self")`). Perfil requerido PRINCIPAL_COORDINATION → Long-horizon →
  Frontera → `claude-opus-5-5` / `xhigh` según [routing.md](../agent-execution/routing.md) §3 y el
  [catálogo](../agent-execution/model-catalog.md) (verificado 2026-09-30; STALE desde 2026-12-29). `CONFIGURATION_STATUS = MATCH`.
- **Entorno de RAM Elements (inspección de solo lectura, sin ejecutar el programa):** registro de Windows → «RAM Elements 2026»,
  `DisplayVersion 26.00.01.015`, instalado 2026-08-28 en `C:\Program Files\Bentley\Engineering\RAM Elements\`; `RAMElements.exe`
  `FileVersion 26.00.01.15`. La caracterización de las superficies de integración es materia de Discovery.

## 4. Ventana de `docs/ROADMAP.md`

Solicitud enviada por el canal entre sesiones a las tres sesiones paralelas (2026-10-10T20:49:14Z, 20:49:18Z y 20:49:22Z): archivo
`docs/ROADMAP.md`, solo en la rama de I-65; alcance = una fila propia al final de la tabla de la Fase 6, tras I-38; intervalo por eventos
desde el acuse hasta el RELEASE de I-65 al publicar el bootstrap o al abortar.

| Sesión | Llegada del acuse (UTC) | Texto literal |
|---|---|---|
| «I - 64» (I-64 / ID30) | 2026-10-10T20:49:44Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» + motivo: sin escrituras de ROADMAP en curso ni planeadas; su fila está en la Fase 6 tras I-63 (punta `d5bdcb7a`, rebasada sobre `bb0d5522`); frontera ID30 anotada |
| «I - 62» (I-62, suspendida) | 2026-10-10T20:50:18Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» + el acuse no levanta la suspensión de I-62 ni concede nada más |
| «I - 52» (I-52) | 2026-10-10T20:57:52Z | «ACUSE I-65 ROADMAP: no escribiré docs/ROADMAP.md hasta tu RELEASE» |

El reclamo se hizo después del tercer acuse. Compatibilidad textual de la fila con las ramas paralelas: comprobada con
`git merge-tree` antes del push del bootstrap (se informa con el commit); no se usa como prueba de exclusividad (C61-G0-03).

## 5. Bootstrap

Archivos nuevos: el contrato `docs/initiatives/I-65-interop-analisis-estructural.md`; `docs/automation/decisions/I-65.md` y
`docs/automation/decisions/I-65-owner-mandate.original.txt`; `docs/automation/state/I-65.yml`; este archivo. Archivo modificado:
`docs/ROADMAP.md` (una fila en la tabla de la Fase 6, tras I-38). **Nada más**: sin cambios en normas globales, HANDOFF, índice de ADR,
FOUNDATIONS, `src/`, `tests/`, `assets/`, `eng/`, CI ni configuración. `automation.enabled: false`.

## 6. Validación de G0

- Comprobación documental: `git diff --name-only` del bootstrap contra la base, todo bajo `docs/`; se informa con el commit.
- CI exigible: la corrida `push` del SHA exacto del bootstrap ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental); es un hecho
  posterior al commit.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto); no se ejecutaron.

## 7. Participantes invocados antes del bootstrap

- Dos subagentes de solo lectura con el modelo heredado de la sesión (CD-I65-G0-08): investigación de fuentes oficiales de Bentley
  (web) y caracterización de la fuente estructural de RackCad sobre `origin/main`. No escribieron en el repositorio. Sus resultados
  alimentan Discovery y se citan allí.
- Ningún Controller, Worker ni Architect se invocó en G0.

## 8. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Arquetipo inicial: NEW ARCHITECTURE (mandato; M-07 previsto). Conformidad: no aplica todavía.
Owner Validation: por determinar; no aplica a G0. Tag de integración: no existe.

## 9. Hechos posteriores al bootstrap (registrados en F0)

- CI `push` del reclamo `32a8475689c9ef8a44837576f3c6f61abd34b07a`: corrida `38085815893`, **success 4/4** (Tests Domain+Application,
  Build UI, UI Tests, Build Plugin without AutoCAD), `head_sha` exacto.
- CI `push` del bootstrap `214c171dfa886acf1dd7aa461283b7e9a1d5158c`: corrida `38086020592`, **success 4/4**, `head_sha` exacto.
- RELEASE de la ventana de `docs/ROADMAP.md` enviado tras el push del bootstrap a «I - 64» (2026-10-10T21:01:28Z), «I - 62» (21:01:30Z)
  e «I - 52» (21:01:31Z). Ventana cerrada; I-65 no vuelve a escribir ROADMAP hasta el cierre.
- `git merge-tree --write-tree` del árbol del bootstrap contra `origin/architecture/workspace-persistente-rackcad` (`d5bdcb7a`),
  `origin/architecture/portabilidad-coordinador-principal` (`362bec12`) y `origin/feature/rackmirror-espejo-semantico` (`fb6b5648`):
  sin conflicto en los tres (no acredita exclusividad).

## 10. F0 — Discovery y Proposal V1

- Discovery: [I-65-discovery.md](../../initiatives/I-65-discovery.md); Proposal: [I-65-proposal-v1.md](../../initiatives/I-65-proposal-v1.md);
  paquete del Architect: [I-65-architect-package-v1.md](../../initiatives/I-65-architect-package-v1.md); sonda P1 (diseño, **no ejecutada**):
  [ram-probe-p1.py](I-65-f0/ram-probe-p1.py).
- Participantes de F0 (CD-I65-G0-08): cuatro subagentes de solo lectura con el modelo heredado (`claude-opus-5-5`): fuentes oficiales
  web de Bentley; fuente estructural de RackCad sobre `bb0d5522`; «RAM Elements Python API User Manual.pdf» instalado; «OpenRE
  Manual.pdf» instalado. Ninguno escribió en el repositorio ni ejecutó RAM Elements.
- RAM Elements **no se ejecutó** (CD-I65-G0-06). Todas las afirmaciones sobre el entorno son de inspección de archivos y registro.

## 11. F0 — Architect, ronda 1, y Proposal V2

- CI `push` del commit de F0 `0f4bc8849ddc0e491231d841cf43b56badba1e98`: corrida `38087065661`, **success 4/4**, `head_sha` exacto.
- Ronda 1 del Architect sobre `0f4bc884`: **CHANGES REQUIRED** (13 REQUIRED, 6 OPTIONAL). Modo `SAME-SESSION ROLE` (subagente,
  contexto independiente, `claude-opus-5-5`). Texto íntegro en [architect-r1-verdict.md](I-65-f0/architect-r1-verdict.md) con su
  SHA-256.
- Respuesta: Proposal V2 completa, borrador de ADR-0050, sonda P1 v2, adenda §13 de Discovery y paquete de la ronda 2.
- Hallazgo propio al corregir P1: un `SystemExit` dentro del intérprete embebido de RAM podría terminar la aplicación anfitriona
  (`PyErr_Print` trata `SystemExit` saliendo del proceso); la puerta de P1 v2 no lo usa.

## 12. F0 — Architect, ronda 2, y Proposal V2.1

- CI `push` de `8f9ac48ce135ef7cff08feb25f0ec19cc4c6aa7a` (Proposal V2): corrida `38088757141`, **success**, `head_sha` exacto.
- Ronda 2 del Architect sobre `8f9ac48c` (mismo revisor): **CHANGES REQUIRED**. Cerrados A65-V1-02..11, 13 y 14..19; A65-V1-01 `OPEN —
  BLOCKED BY P1` (correcto por diseño); A65-V1-12 abierto por un residual; nuevos A65-V2-01..04 REQUIRED y 05..09 OPTIONAL. Texto
  íntegro en [architect-r2-verdict.md](I-65-f0/architect-r2-verdict.md) con su SHA-256.
- Respuesta: Proposal V2.1 completa (disposición en su §14), sonda P1 v3, ADR-0050 ajustado y paquete de la ronda 3.


## 13. F0 — Architect, ronda 3, y sonda P1 v3.1

- Ronda 3 sobre `9bfd8df5`: **CHANGES REQUIRED** con un solo `REQUIRED` nuevo, A65-V21-01 (la sonda no medía el brazo `PositiveY` si
  RAM resultara Z arriba). Cerrados A65-V1-02..19 y A65-V2-01..09; A65-V1-01 `OPEN — BLOCKED BY P1`. A65-V21-02 (OPTIONAL: redacción
  de OV-I65-03 con doble cara / `NegativeY`) se incorpora en la V3, no en la V2.1 ya revisada. Texto íntegro en
  [architect-r3-verdict.md](I-65-f0/architect-r3-verdict.md).
- Corrección: sonda **P1 v3.1** (A1 añade el miembro `(0, 100, 4.375)` adelante y atrás con giros 0°, 90° y 12.3456°; L1 lee además
  una carga `GLOBAL_Z`). El Architect anunció que cierra A65-V21-01 a la vista de este diff, sin ronda completa.
- **Cierre:** el Architect cerró **A65-V21-01** sobre `acaefc1d` y declaró que **P1 v3.1 puede autorizarse como la corrida definitiva con
  licencia**, sujeta solo a Q-O4 ([architect-r3b-closure.md](I-65-f0/architect-r3b-closure.md)). Único `REQUIRED` abierto: A65-V1-01
  (`OPEN — BLOCKED BY P1`). La revisión de la V3 será de confirmación de valores medidos y respuestas del Owner, salvo STOP o resultado
  Z arriba.

## 14. P1 — preflight y STOP por condición del Owner (2026-10-10)

- Decisión del Owner recibida a las 22:48:00Z: [`I-65-owner-decision-p1.original.txt`](../decisions/I-65-owner-decision-p1.original.txt).
- Preflight (CD-I65-P1-01), medido:

| Comprobación | Resultado |
|---|---|
| Rama | `origin/architecture/interop-analisis-estructural` = local = `048a4b07b34e5ad5a4d51df2b5e6b8c60d07b47c`; árbol limpio; `origin/main` sin cambio (`bb0d5522`); CI de `048a4b07`, `acaefc1d` y `9bfd8df5` success |
| Sonda aprobada | blob `671612f6790bb87e1453a07c4c48e201ee8ba763` en `acaefc1d` y en `048a4b07` (idénticos); 16 198 bytes; SHA-256 `743fcf96f756e4deb40349e2d5d6ad7674e30734ce916d6a929fc5fd15e6a6b6`; `acaefc1d..048a4b07` no toca la sonda |
| Seguridad (Architect) | puerta antes de la primera escritura, sin `SystemExit`, escrituras exclusivas en `OUT_DIR`: vigentes. **Conflicto:** el paso S9 (`m.save_as`) guarda el modelo |
| RAM Elements | «RAM Elements 2026» `26.00.01.015` (registro y `HKLM\SOFTWARE\Bentley\RAMElements`); `RAMElements.exe` `26.00.01.15`, SHA-256 `7DCB4855…7970`; `ramelements` `0.1.0`, `__init__.py` SHA-256 `FC5946D5…AACE`; sin cambios respecto de Discovery |
| Modelos del usuario | RAM Elements **no** está en ejecución: una corrida abriría una instancia nueva sin modelos del usuario |
| Autoguardado | sin configuración de autoguardado visible en la configuración de usuario de RAM; `DataRecovery.log` solo registra el rescate de datos de versión previa al primer arranque |
| Computer Use | herramientas disponibles; «RAM Elements 2026» instalada como aplicación; acceso todavía no solicitado |

- **STOP (CD-I65-P1-02):** no se ejecutó P1. Se prepara P1 v3.2 (v3.1 sin S9) para revisión por diff del Architect y autorización del
  Owner. **P1 NO se ha ejecutado**: todo lo anterior es inspección de archivos, registro y Git.
- Hallazgo lateral: la configuración de usuario de RAM (`%APPDATA%\Bentley\Engineering\RAM Elements\26.0.1\RE\Config.ini`,
  `[DlgSectionsDB] frameSeccionesDataBaseName=Montilla`) apunta a una base de secciones personalizada «Montilla» que no se encontró en
  las carpetas de RAM, ProgramData, AppData ni Documentos: UNKNOWN; puede contener secciones de usuario relevantes para Q-O2. Par de
  unidades de visualización por defecto del usuario: `CurrentUnit=4` (`KIP_FT`).

- **Architect sobre P1 v3.2** (`54f17ef6`, blob `2fafe9c9…`, SHA-256 `96ae6ea9…db80`): sin defectos; no retira nada que la V3 deba
  congelar; **puede autorizarse como la corrida definitiva**, sujeta a que el Owner autorice el cambio desde v3.1
  ([architect-p1v32-diff.md](I-65-f0/architect-p1v32-diff.md)). RC-05 (extensión y efecto de `save_as`) queda UNKNOWN. Las unidades de
  **carga distribuida** no se miden desde la v2 (fuera de V1): se informarán como DOCUMENTED ONLY / UNKNOWN; las de **momento** se
  comprueban por las reacciones en `LB_IN` (N2: 1.0e5 lb·in; N1: 6.0e4 lb·in). Procedimiento de ejecución:
  [p1-procedure.md](I-65-f0/p1-procedure.md).

## 15. P1 — preparación de la corrida y acceso denegado a Computer Use (2026-10-11T01:51:11Z)

- Re-preflight: rama `c22e0148` = origin, árbol limpio; blob de la sonda en `54f17ef6` y en la punta = `2fafe9c9…`; RAM Elements no
  estaba en ejecución. Autorización del Owner registrada en `b0098f21` antes de ejecutar.
- Carpeta de evidencias **nueva** fuera del repositorio: `D:\I65-P1\run-20261011T014503Z\`. Copia ejecutable `ram-probe-p1.py`:
  SHA-256 `95378d877a37f297f7e1667b4e763ec891c5df3c07d33e833e34bb1009d85fe0` (16 089 bytes); única diferencia con el blob aprobado
  (SHA-256 `96ae6ea9…db80`, 16 070 bytes): la línea `OUT_DIR = r"D:\I65-P1\run-20261011T014503Z"`. Sintaxis verificada sin ejecutar.
- `request_access` de Computer Use para «RAM Elements 2026»: **denegado por el usuario** (`user_denied`). No se reintentó; RAM Elements
  no se abrió.
- **P1 NO se ha ejecutado.** F0 queda bloqueado por la ejecución de P1; se entrega el procedimiento manual
  ([p1-procedure.md](I-65-f0/p1-procedure.md), secciones B y C) con la carpeta ya preparada.

## 16. Diagnóstico del bloqueo de Computer Use (2026-10-11T02:19:27Z)

Orden del Owner: diagnosticar sin modificar configuraciones de seguridad, sin repetir la solicitud de acceso y sin ejecutar P1. El
Owner confirmó que no rechazó ningún permiso: la denegación apareció sola.

| Pregunta | Resultado | Clase |
|---|---|---|
| ¿Restricciones de Computer Use sobre apps de escritorio? | Documentadas: niveles por categoría (solo ver: navegadores y plataformas de trading; solo clic: terminales e IDE; control total: el resto), una lista de bloqueo de apps sensibles que «se deniegan automáticamente», la lista **«Denied apps»** del usuario (Settings > This computer > System > Computer use) que rechaza «sin preguntar», y no disponible en planes Team/Enterprise. RAM Elements no pertenece a ninguna categoría restringida documentada | DOCUMENTED (code.claude.com/docs/en/desktop; support.claude.com, artículo 14128542) |
| ¿RAM Elements se ejecuta elevado? | **No.** Manifiesto embebido `requestedExecutionLevel level="asInvoker" uiAccess="false"`; sin capa `RUNASADMIN` en `AppCompatFlags\Layers`; el acceso directo del menú Inicio no pide elevación (destino `RamElements.exe /Multiopen`); la sesión corre con integridad media; RAM ni siquiera estaba en ejecución al denegarse | MEASURED |
| ¿Origen: sistema operativo, herramienta o política? | **Capa de acceso de la app de Claude.** La respuesta (`user_denied`) llegó antes de cualquier interacción con RAM o con Windows, así que no es UIPI. En la app: Computer Use habilitado (`chicagoEnabled = true`); trabajo entre apps no deshabilitado por la organización (`orgWorkAcrossAppsDisabled = false`); sin políticas `HKLM/HKCU\SOFTWARE\Policies\{Anthropic,Claude}`; esta sesión no guarda decisiones previas (`cuAllowedApps: []`); «ramelements»/«RAM Elements» no aparece en el almacenamiento local legible de la app. La última concesión de Computer Use en este equipo (sesión I-62, app «Claude», 2026-10-05T02:42:39Z, mismo modo de permisos) es **anterior** a la actualización de la app del 2026-10-09 (`app-2.31226.1`) | MEASURED (hechos); causa exacta UNKNOWN |
| Causa exacta | Los únicos rechazos sin aviso documentados son la lista «Denied apps» y la lista de bloqueo. Cuál aplicó, o si la compilación actual o el modo de la sesión cambian el comportamiento, no se puede ver sin abrir Settings (acción del usuario) y no está documentado | UNKNOWN |
| Alternativas oficiales | (1) El Owner revisa Settings > This computer > System > Computer use > Denied apps y, si RAM Elements figura, la quita: autorización normal del usuario; el Coordinator haría **una** solicitud nueva. (2) Ejecución manual por el Owner con el procedimiento preparado (sin cambios de configuración). El modo guiado («teach») usa la misma lista de apps permitidas y no está documentado oficialmente: no se propone | DOCUMENTED / prepared |

Sin cambios de configuración, sin nuevas solicitudes de acceso, sin abrir RAM Elements. **P1 sigue sin ejecutarse**; F0 sigue bloqueado.

## 17. P1 ejecutada (2026-10-11T02:43:00Z)

- Único intento autorizado de Computer Use: **concedido** («RAM Elements 2026», nivel completo). La validación de licencia de RAM la
  atendió el Owner fuera del alcance de Computer Use.
- **P1 v3.2 se ejecutó** en un modelo nuevo y vacío por «Ejecutar archivo…»: 12 pasos `OK`, «Script ejecutado con éxito». Dos diálogos
  modales «Los resultados del análisis … se perderán» se respondieron «Sí» (solo descartan resultados del modelo desechable). Cierre sin
  guardar («No»); error gRPC benigno al cerrar. Ningún `.retx` guardado; RAM creó su archivo de trabajo `NoName.fem` en `ProgramData`.
- Custodia en [p1-run/](I-65-f0/p1-run/) (informe, copia ejecutada, archivo de prueba y 7 capturas) y análisis en
  [p1-results.md](I-65-f0/p1-results.md). Sin falla de clase STOP.
- Siguen: adenda §14 de Discovery, Proposal V3, ADR-0050 (borrador final) y paquete de confirmación para el mismo Architect.

## 18. F0 — Architect, ronda 4, y Proposal V3.1

- CI `push` de `d077a1b4`: **success**, `head_sha` exacto.
- Ronda 4 (confirmación) sobre `d077a1b4`: **CHANGES REQUIRED** con dos `REQUIRED` de documento (A65-V3-01 contrato de unidades del
  volcado ante un modelo analizado; A65-V3-02 oráculo de material con fuente de ingeniería) y cuatro OPTIONAL. **A65-V1-01 y A65-V21-02
  CLOSED.** El Architect verificó por su cuenta el informe de P1 (regla de ejes, Timoshenko, conversiones, huérfanos, enum). Texto íntegro
  en [architect-r4-verdict.md](I-65-f0/architect-r4-verdict.md).
- Respuesta: Proposal V3.1 (disposición en su §17), `p1-results.md` con los dos hashes del informe y ADR-0050 apuntando a V3.1. El
  Architect anunció que cierra ambos `REQUIRED` a la vista del diff.

## 19. Consenso de diseño (2026-10-11T02:52:44Z)

- Architect, cierre por diff `31aad097..3872fcc0`: **AGREED** sobre la **Proposal V3.1**, `docs/initiatives/I-65-proposal-v3.1.md`,
  blob `11062a915c4c174ede180830c8ab3173ee7b6666`, commit `3872fcc03f6036bbe1c29a045cf4702bc63df715`
  ([architect-r4b-agreed.md](I-65-f0/architect-r4b-agreed.md)). Todos los `REQUIRED` de todas las rondas cerrados (A65-V1-01..13,
  A65-V2-01..04, A65-V21-01, A65-V3-01/02, A65-V31-01); opcionales incorporados.
- Coordinator: **AGREED** sobre el mismo blob (CD-I65-F0-08).
- Commit de Freeze permitido (INITIATIVE_LIFECYCLE §6, según el Architect): solo la línea `Frozen` y las líneas de cabecera `Estado` y
  `Coordinator/Architect`. **No se ejecuta**: el Freeze, la aceptación de ADR-0050 (Q-O6) y la autorización de F1 (Q-O8) quedan con el
  Owner. IMPLEMENTATION AUTHORIZATION = NO.
- Métricas de F0: arquetipo NEW ARCHITECTURE (M-07) sin cambio; 5 revisiones del Architect (rondas 1-4 y cierres por diff), modo
  `SAME-SESSION ROLE` (subagente de contexto independiente); 1 corrida licenciada de RAM Elements (P1 v3.2); 0 cambios de código de
  producción.

## 20. Verificación previa al Freeze (2026-10-11T03:04:39Z)

Orden del Owner «Conditional GO for Freeze and F1» (pegada en el chat; custodia en el commit siguiente). Resultado de las cinco
condiciones:

| Condición | Resultado |
|---|---|
| CI pendientes en PASS | **sí**: las 16 corridas `push` de la rama, de `32a84756` a `dc3afdda`, `completed/success` |
| Blob exacto de la Proposal V3.1 | **sí**: `11062a915c4c174ede180830c8ab3173ee7b6666` en `dc3afdda` (y en `5253b031`) |
| Architect mantiene AGREED sobre esos bytes | **sí** («AGREED MAINTAINED on blob 11062a91 at dc3afdda»); confirma que el commit de Freeze solo puede cambiar `Estado`, `Frozen` y la línea `Coordinator/Architect` |
| Conflictos con iniciativas paralelas | **no**: `origin/main` sin cambio (`bb0d5522`); I-64 avanzó a `db0f9d2b` (solo `Application/Workspace`, `Plugin/Workspace`, `tests/.../Workspace` y documentos); ninguna rama toca unidades, catálogo de secciones ni Cantilever; `git merge-tree` limpio contra I-64, I-62 e I-52; ADR-0050 solo existe en esta rama |
| ADR-0050 corresponde a la arquitectura acordada | **NO en `dc3afdda`**: la Decisión 4 no recogía la lista de permiso de un archivo de comando del Plugin ni la exclusión de pruebas y herramientas de la guarda G1 (Proposal V3.1 D-01/D-13). **STOP de la orden del Owner**: no se acepta el ADR ni se congela |

- Corrección preparada (ADR sigue `propuesto`): Decisión 4 alineada con G1 en `5253b031` (blob del ADR `4258c286…`); la Proposal no cambia.
  Pendiente: confirmación por diff del Architect y confirmación del Owner para aceptar el texto corregido.
- Entorno del Controller para F1 (hechos, sin delegación abierta): Codex CLI en `%LOCALAPPDATA%\OpenAI\Codex\bin\2e5e00daee91c61d\codex.exe`
  (2026-10-09T17:25Z); `pwsh` del runtime presente; `~/.codex/config.toml` SHA-256 `F909F480…8113`, **modificado 2026-10-11T02:54:45Z**
  (distinto de las líneas base de I-63). No es P-01 retroactivo; la línea base se toma en la primera salida de relevo de F1. Riesgo: si el
  archivo cambia durante una cesión, P-01 (le ocurrió a I-64 el 2026-10-10).
- Architect sobre `5253b031`: Decisión 4 **coincide** con V3.1; ADR-0050 blob `4258c286dd48c7e02b07ed7d434d9368d8596bc6` corresponde
  por completo a la arquitectura acordada; AGREED sobre la Proposal V3.1 blob `11062a91` mantenido. Con la aceptación solo deben cambiar
  estado, fecha, decisores y notas posteriores del ADR. Pendiente: confirmación del Owner sobre el texto corregido.

## 21. Freeze y autorización de F1 (2026-10-11T03:10:35Z)

- ADR-0050 **aceptado** por el Owner: commit `ed8154c3` (solo estado, fecha, decisores y nota; texto aceptado = blob
  `4258c286dd48c7e02b07ed7d434d9368d8596bc6`).
- **Freeze**: commit **`a9d7080be3046e53b41d83af1bfece0be29871f3`**. Cambia solo las líneas `Estado`, `Frozen` y `Coordinator/Architect` de
  `docs/initiatives/I-65-proposal-v3.1.md` (3 líneas, comprobado con `git diff -U0`); blob congelado
  `4e6e1feb4d83aa291707c72ad12f89411ff1d37a`; acuerdo sobre el blob `11062a915c4c174ede180830c8ab3173ee7b6666` (commit `3872fcc0`).
- Consenso: Coordinator AGREED (CD-I65-F0-08) y Architect AGREED ([architect-r4b-agreed.md](I-65-f0/architect-r4b-agreed.md)),
  reconfirmado por el Architect en `dc3afdda` y `5253b031`.
- F1 IMPLEMENTATION AUTHORIZATION = **YES**, solo para el alcance de F1 de la Proposal V3.1 (§6 fila F1), con el protocolo I-61; sin F3-F6
  ni interfaz gráfica; termina al concluir F1 (orden del Owner, decisiones §9).

## 22. F1-T1-NEUTRAL-MODEL — ejecución delegada (2026-10-11T04:21:27Z)

Primera tarea de F1 (CD-I65-F1-01), protocolo de AUTOMATION_PLAN §16. Contrato de gate emitido por el Coordinator sobre
`AuthorityRevision` = `BaseSha` = `9d0c6619` (decisiones §10-§11); `MainSha` `bb0d5522` sin cambio durante toda la tarea.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T032206Z-7516` | PLANNING | Codex `gpt-6-luna`/`high` (efectivo) | paquete válido; seis colecciones idénticas al contrato; A1-A8 en `pass`; celda `claude-sonnet-5-5`/`high` (Deep), ROUTINE_IMPLEMENTATION |
| `R20261011T032626Z-7e5a` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual que la real: **oráculo cumplido** |
| `R20261011T032758Z-3556` | WORK | subagente `claude-sonnet-5-5`/`high` (efectivo, 143 mensajes) | `IMPLEMENTATION_COMPLETE`; RED `172d53fc`, GREEN `1692804a`; 29 min 36 s; 87 llamadas |
| `R20261011T040119Z-5751` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `1692804a3e81a607d81d924c9345f02fe7f9a41f`; coherencia README §8 = True |
| `R20261011T040859Z-a7a4` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `Identity` `fail`, `EXECUTION_BLOCKED/STOP`, anteriores iguales: **oráculo cumplido** |
| `R20261011T041233Z-ccd3` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `EXECUTION_VERIFIED`, `Scope` `pass`: **oráculo NO cumplido** (DEV-I65-F1-01) |
| `R20261011T041658Z-4f9c` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `FreeText` `fail`, `EXECUTION_REWORK_REQUIRED/REWORK`, anteriores y posteriores iguales: **oráculo cumplido** |

- **Commits del Worker:** RED `172d53fcde34e6b7468923b974d017bed73729a2` (esqueleto que compila + 9 archivos de prueba) y GREEN
  `1692804a3e81a607d81d924c9345f02fe7f9a41f`; ambos con `Co-Authored-By: Claude Sonnet 5.5`, push sin force. 24 archivos: 12 nuevos en
  `src/RackCad.Application/StructuralAnalysis/`, `Units/MassUnits.cs`, `Units/ForceUnits.cs`, `StructuralSectionUnits.cs` (una línea: el
  literal pasa a `MassUnits.KilogramsPerPound`) y 9 pruebas nuevas en `tests/RackCad.Tests/StructuralAnalysis/`.
- **CI push:** RED `38109425352` = `failure` (job Core en `failure`, Plugin `skipped`); GREEN `38109857453` = `success` con los cuatro jobs.
  TRX Core GREEN 12866/12866 y filtro `FullyQualifiedName~RackCad.Tests.StructuralAnalysis` 271/271; TRX Core RED: 167 fallidas, todas del
  filtro (271 seleccionadas, 104 superadas: autopruebas de guardas y comprobaciones estructurales), sin errores de compilación; TRX UI 1654
  total / 1637 ejecutadas (17 `Skip`, convención del logger).
- **ChainRedFiles** (acreditado por el Controller): los 9 archivos de `tests/RackCad.Tests/StructuralAnalysis/`; `git diff --name-only
  RedSha CurrentSha -- tests/` vacío.
- **Comprobaciones mecánicas de la sesión:** alcance del diff `9d0c6619..1692804a` PASS (24/24 permitidos, 0 prohibidos); pruebas solo
  altas; prompt del Worker = `prompt.md`; trailers; términos de gate ausentes en la entrega y en la verificación (control manual 16.10).
- **Relevos:** `config.toml` `F909F480…8113` sin cambio en las seis invocaciones; ningún participante ajeno. Falsos P-02 releídos por PID
  (lección de I-61): dos `bash.exe` (entrada del Worker, creados tras el fin de la cesión) y dos `git.exe` lanzados por el servicio node del
  escritorio (salida de nc1, antes de invocar); todos desaparecidos a los 2 s.
- **Errores del Worker superados (señal Denials):** dos heredoc de Bash mal cerrados y un bloqueo del arnés a una orden de PowerShell cuya
  cadena XPath se leyó como ruta protegida; sin denegaciones de permisos ni avisos de límite.
- **DEV-I65-F1-01 (nc2 no superado):** el Controller comparó el diff con el `AllowedWriteScope` del **contrato** y no con el de la
  delegación mutada, que excluía `StructuralArea.cs`. Mismo modo de fallo que DEV-G2-01 de I-63. La comprobación mecánica de la sesión da
  `Scope` = `pass` para la delegación real y `fail` para la de nc2 (`session-scope-check.json`). Salida válida: no se repite. No bloquea.
- **Invocaciones de Codex en F1:** 5 de 14 (CD-I65-F1-02). `attempts` = 0.

Custodia (blob = identidad duradera; SHA-256 = procedencia del transitorio):

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/gate-contract.json` | `842D550BBE12276D…` | `9801b35bfcbc` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/prompt.md` | `53C2255D77E95743…` | `8088a618e00d` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/delegation.json` | `8771F69F333ACA41…` | `68ab3d4ce6c7` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL/R20261011T032206Z-7516/relay-record.json` | `BC3D7B09349CE7EF…` | `0105e3117f30` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032758Z-3556/prompt.md` | `BBA8F55FB09C94A1…` | `a1d17dd03c49` |
| `F1-T1-NEUTRAL-MODEL/R20261011T032758Z-3556/worker-handoff.json` | `DF5268CA0CFB5629…` | `08f7f8bbcfc4` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL/R20261011T032758Z-3556/relay-record.json` | `184317239898FD0B…` | `1532d80c925a` |
| `F1-T1-NEUTRAL-MODEL/R20261011T040119Z-5751/prompt.md` | `DD28B2B51F15342C…` | `94dd86227770` |
| `F1-T1-NEUTRAL-MODEL/R20261011T040119Z-5751/controller-verification.json` | `3BD55D929D7949CE…` | `f95821606cab` |
| `F1-T1-NEUTRAL-MODEL/R20261011T040119Z-5751/relay-record.json` | `355FE6246CD0C9FD…` | `7ee108bd85ae` |
| `F1-T1-NEUTRAL-MODEL/R20261011T040119Z-5751/trx-summary.json` | `9D1538663423E2ED…` | `be58c3906db2` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL-nc1/R20261011T040859Z-a7a4/prompt.md` | `B9D5FEE2FF600BEA…` | `32a7aef16a17` |
| `F1-T1-NEUTRAL-MODEL-nc1/R20261011T040859Z-a7a4/mutation.json` | `84E7354BBC662E1D…` | `8ceda9b8e61f` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL-nc1/R20261011T040859Z-a7a4/controller-verification.json` | `58DB1A4BF9F14AED…` | `20e4b967e3fd` |
| `F1-T1-NEUTRAL-MODEL-nc1/R20261011T040859Z-a7a4/relay-record.json` | `F72E7AC6155654CA…` | `3c44323edf7d` |
| `F1-T1-NEUTRAL-MODEL-nc2/R20261011T041233Z-ccd3/prompt.md` | `9D57E48C8CB61E40…` | `be624d3c9039` |
| `F1-T1-NEUTRAL-MODEL-nc2/R20261011T041233Z-ccd3/mutation.json` | `68D9C1D0E7359368…` | `2b29ce69159e` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL-nc2/R20261011T041233Z-ccd3/controller-verification.json` | `CC32BCFAA00982C5…` | `a597c98f8f76` |
| `F1-T1-NEUTRAL-MODEL-nc2/R20261011T041233Z-ccd3/relay-record.json` | `FFA157AF646A569A…` | `55e49f6259ac` |
| `F1-T1-NEUTRAL-MODEL-nc2/R20261011T041233Z-ccd3/session-scope-check.json` | `161935292CE2DEE5…` | `1c1db3aeea81` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL-nc3/R20261011T041658Z-4f9c/prompt.md` | `4789658A8E18530E…` | `7b22654dc7b8` |
| `F1-T1-NEUTRAL-MODEL-nc3/R20261011T041658Z-4f9c/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F1-T1-NEUTRAL-MODEL-nc3/R20261011T041658Z-4f9c/controller-verification.json` | `C727711DD24D408F…` | `b52fce073731` |
| `F1-T1-NEUTRAL-MODEL-nc3/R20261011T041658Z-4f9c/relay-record.json` | `D8E4C36613BC46FB…` | `9da93976c24b` |
| `F1-T1-NEUTRAL-MODEL-nc4/R20261011T032626Z-7e5a/delegation.json` | `0F4B785DAF2BB1B6…` | `e99f338d12d7` |
| `F1-T1-NEUTRAL-MODEL-nc4/R20261011T032626Z-7e5a/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F1-T1-NEUTRAL-MODEL-nc4/R20261011T032626Z-7e5a/relay-record.json` | `BC51513B25CD54B3…` | `338e397c3007` |

## 23. F1-T2-CANTILEVER-SOURCE — ejecución delegada (2026-10-11T05:40:16Z)

Segunda tarea de F1. Golden de `REF-CANT-01` derivado a mano por el Coordinator antes de la delegación
([I-65-f1-ref-cant-01.md](../../initiatives/I-65-f1-ref-cant-01.md); corrida de solo lectura del resolver en
[I-65-f1/](I-65-f1/ref-cant-01-resolver-run.txt)); contrato sobre `AuthorityRevision` = `BaseSha` = `ff0c57c7`; `MainSha` `bb0d5522` sin cambio.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T044358Z-f76f` | PLANNING | Codex `gpt-6-luna`/`high` | paquete válido; seis colecciones idénticas al contrato; A1-A8 en `pass`; celda `claude-sonnet-5-5`/`high` (Deep) |
| `R20261011T044756Z-8645` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual: **oráculo cumplido** |
| `R20261011T044815Z-7eae` | WORK | subagente `claude-sonnet-5-5`/`high` (efectivo, 108 mensajes) | `IMPLEMENTATION_COMPLETE`; RED `3134c2fe`, GREEN `667a3991`; 28 min 46 s; 64 llamadas, 0 errores |
| `R20261011T052025Z-a470` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `667a3991a106afe4e18c88500f65382e69301c69`; `Scope` contra la delegación, archivo por archivo |
| `R20261011T052706Z-20ca` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP` [S-04] con `Identity` `fail`, pero `FailureClass` = `Handoff`: **oráculo NO cumplido en sentido estricto** (DEV-I65-F1-02) |
| `R20261011T053105Z-4217` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `Scope` `fail`, `EXECUTION_BLOCKED/STOP`, anteriores iguales: **oráculo cumplido** (la instrucción de CD-I65-F1-12 corrigió el modo de fallo de DEV-I65-F1-01) |
| `R20261011T053522Z-4b8c` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `FreeText` `fail`, `EXECUTION_REWORK_REQUIRED/REWORK`, anteriores y posteriores iguales: **oráculo cumplido** |

- **Commits del Worker:** RED `3134c2fe295c01c8746ab2c577580bc25e57414f` y GREEN `667a3991a106afe4e18c88500f65382e69301c69`, con
  `Co-Authored-By: Claude Sonnet 5.5`. 8 archivos: `Systems/Cantilever/CantileverStructuralModelSource.cs` (nuevo; el GREEN solo cambia
  este archivo), `StructuralAnalysis/StructuralDiagnostic.cs` (13 códigos de diagnóstico nuevos, aditivos, en el RED), los dos fixtures
  JSON y cuatro archivos de prueba `StructuralAnalysisCantilever*`.
- **CI push:** RED `38114028511` = `failure` (Core); GREEN `38114105131` = `success` con los cuatro jobs. TRX Core GREEN 12946/12946 y filtro
  `FullyQualifiedName~RackCad.Tests.StructuralAnalysisCantilever` 80/80; TRX Core RED: 78 fallidas, todas del filtro (2 superadas:
  comprobación literal de las colocaciones del resolver), sin errores de compilación.
- **Golden:** los 8 nudos y los 7 miembros coinciden con la tabla derivada a mano; diferencia máxima 0 en coordenadas y 1.39e-16 en los
  vectores de peralte; posiciones de carga 24.35421884050472, 15.354218840504721 y 33.35421884050472 in; magnitudes 1000, 2000 y
  1102.311310924388 lbf. Sin C-03.
- **ChainRedFiles:** los 6 archivos nuevos de `tests/RackCad.Tests/StructuralAnalysis/` (2 fixtures y 4 de prueba).
- **Comprobaciones mecánicas de la sesión:** alcance PASS (8/8 permitidos por la delegación, 0 prohibidos); pruebas solo altas; prompt
  del Worker = `prompt.md`; para nc2, la misma comprobación da `fail` (`StructuralDiagnostic.cs` fuera); términos de gate ausentes.
- **Relevos:** `config.toml` `F909F480…8113` sin cambio; ningún participante ajeno. Falso P-02 releído: dos `bash.exe` en la salida de
  nc2, desaparecidos a los 2 s. En la entrada del Worker, solo servidores de compilación, el `conhost` del propio comprobador y los
  servidores MCP de la app Codex del Owner (sin la ruta del worktree).
- **DEV-I65-F1-02 (nc1 de T2):** la mutación (`CurrentSha` inexistente en la copia de la entrega) cambió el SHA-256 de la entrega, que ya
  no coincide con `Outcome.OutputSha256` del registro del trabajo copiado; el Controller lo detectó en `Handoff` (comprobación 2), antes
  que en `Identity` (5, también `fail`), con la disposición esperada `BLOCKED/STOP`. La mutación fue detectada; el oráculo relativo
  (anteriores iguales a la real) no se cumple en sentido estricto. Salida válida: no se repite.
- **Hallazgos abiertos del Worker:** orden de las seis banderas de apoyo Tx, Ty, Tz, Rx, Ry, Rz; prioridad de fusión de nudos de la
  columna (pie, cabeza, J inferior) dentro de 0.001 in; con brazos C o L reales, la colocación centrada por caja sale del plano y da
  `Unsupported` por desviación del plano antes que por familia (mismo resultado).
- **Invocaciones de Codex en F1:** 10 de 14. `attempts` = 0.

Custodia (blob = identidad duradera; SHA-256 = procedencia del transitorio):

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/gate-contract.json` | `6F32D1C5A97AD159…` | `a6602f98d5fa` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/prompt.md` | `169F5D207B08BBF6…` | `980f362fcde5` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/delegation.json` | `89A9DA809C2860D7…` | `252dbde8d85b` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044358Z-f76f/relay-record.json` | `62DAE87CF80A6ACD…` | `052d449828c0` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044815Z-7eae/prompt.md` | `DACA7B0441BABD00…` | `b29593cecfaf` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044815Z-7eae/worker-handoff.json` | `DFA982A7624C6852…` | `f788b3dbc5fd` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T044815Z-7eae/relay-record.json` | `09CBB6FDED1DB6D7…` | `eb953f09afd0` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T052025Z-a470/prompt.md` | `4A4EFC146D016A31…` | `a0e529b01d05` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T052025Z-a470/controller-verification.json` | `28A6F41045956332…` | `69af9eb7ed67` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T052025Z-a470/relay-record.json` | `C264EBC544E9CB33…` | `04209eed3afc` |
| `F1-T2-CANTILEVER-SOURCE/R20261011T052025Z-a470/trx-summary.json` | `C774A3EF84009A9A…` | `a2e03dec1e61` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE-nc1/R20261011T052706Z-20ca/prompt.md` | `A76BDAF33CEE87D6…` | `10d9a3495ab6` |
| `F1-T2-CANTILEVER-SOURCE-nc1/R20261011T052706Z-20ca/mutation.json` | `46FE0ED0C9F2DB05…` | `d285d9839084` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE-nc1/R20261011T052706Z-20ca/controller-verification.json` | `C706677259F068CD…` | `011e129085ea` |
| `F1-T2-CANTILEVER-SOURCE-nc1/R20261011T052706Z-20ca/relay-record.json` | `DB988005BDDE457D…` | `2abc252c2841` |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/prompt.md` | `EDF78289A1ADEBC2…` | `802798184618` |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/mutation.json` | `D8699C4DC6619435…` | `37c2d85fe2a5` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/controller-verification.json` | `B9CD1F79FCF1BCFA…` | `46d3a6a97e6d` |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/relay-record.json` | `DAD5876DC6C0729E…` | `2d2e9e76c023` |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/session-scope-check.json` | `A476550976C63C1E…` | `c392d182b2fc` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE-nc2/R20261011T053105Z-4217/exit-reread.txt` | `5C761396B835DBD3…` | `6c0b6b0b729a` |
| `F1-T2-CANTILEVER-SOURCE-nc3/R20261011T053522Z-4b8c/prompt.md` | `C79C3EE2981F3762…` | `e0c903969084` |
| `F1-T2-CANTILEVER-SOURCE-nc3/R20261011T053522Z-4b8c/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F1-T2-CANTILEVER-SOURCE-nc3/R20261011T053522Z-4b8c/controller-verification.json` | `C01674CB6C5902EC…` | `3740d075f608` |
| `F1-T2-CANTILEVER-SOURCE-nc3/R20261011T053522Z-4b8c/relay-record.json` | `C831E3501055E96C…` | `bfc6ce75fbb4` |
| `F1-T2-CANTILEVER-SOURCE-nc4/R20261011T044756Z-8645/delegation.json` | `33B37C79882F3314…` | `fd351532862b` |
| `F1-T2-CANTILEVER-SOURCE-nc4/R20261011T044756Z-8645/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F1-T2-CANTILEVER-SOURCE-nc4/R20261011T044756Z-8645/relay-record.json` | `9B718EB954E5AB89…` | `0e8c09dc479b` |

## 24. Cierre de F1 — revisión y criterios de LIFECYCLE §7 (2026-10-11T05:50:18Z)

- **Revisión independiente de conformidad del Architect** sobre `9d0c6619..667a3991 -- src tests`:
  **CONFORMANT WITH OBSERVATIONS**, sin REQUIRED; 12 hallazgos OPTIONAL A65-F1-01..12
  ([architect-f1-conformance.md](I-65-f1/architect-f1-conformance.md)). Confirma que CD-I65-F1-14 (lectura de IDL-10) **no es material**.
- **Revisión del Coordinator** del diff contra el Freeze y las decisiones: CD-I65-F1-11 (T1) y CD-I65-F1-17 (T2); disposición de los
  hallazgos del Architect en CD-I65-F1-20.
- **Criterios de LIFECYCLE §7 para F1** (estado en este commit):

| # | Criterio | Estado |
|---|---|---|
| 1 | Resultado observado por pruebas | sí: `REF-CANT-01` + archivo de cargas → `StructuralModel` `Valid`, determinista, igual al golden derivado a mano (pruebas `StructuralAnalysisCantileverGoldenTests` en el TRX de `38114105131`) |
| 2 | RED→GREEN focal por comportamiento nuevo, selección > 0 | sí: T1 271 (RED 167 fallidas) y T2 80 (RED 78 fallidas), acreditados por el Controller |
| 3 | Pruebas y evidencia exigidas en verde | sí en los SHA de código (`1692804a`, `667a3991`); Core Full local sobre el SHA de cierre: tras este commit |
| 4 | CI propio del SHA de cierre leído y verde | tras el push de este commit |
| 5 | Revisión del Coordinator contra Freeze + A-n, sin EXP-01 A abierta | sí (sin A-n; sin EXP-01 A) |
| 6 | Orden commit → árbol limpio → evidencia local → push → CI | en curso sobre este commit |
| 7 | Rebase entre gates | no hubo (`origin/main` = `bb0d5522` durante todo F1) |

## 25. F1 cerrada (2026-10-11T06:00:46Z)

Hechos posteriores al commit de cierre `4514a99a7ac853a707fd14c89823f327e338de4e` (LIFECYCLE §7, orden de AGENTS.md / WORKFLOW):

- **Árbol limpio y evidencia local:** Core Full local sobre `4514a99a` con árbol limpio: **12946/12946** superadas, 0 fallidas, 0 omitidas
  (3 min 47 s; TRX `core-full-4514a99a.trx`, SHA-256 `8e04bab017308720b80ef9e28a85f0cefcca9ccc5b8455d365a3b03231381340`, transitorio).
- **Push y CI del SHA de cierre:** corrida `push` 38116394272 de `refs/heads/architecture/interop-analisis-estructural`, `head_sha`
  `4514a99a`, `completed/success`: UI Tests (WPF controls, net8.0-windows) = `success`; Tests (Domain + Application) = `success`; Build UI (WPF, valida API de Application) = `success`; Build Plugin without AutoCAD = `success`.
- **Criterios de LIFECYCLE §7:** 1-7 cumplidos (ver §24 y este apartado). **GATE PASS de F1** declarado por el Coordinator (CD-I65-F1-21)
  sobre el SHA de cierre `4514a99a` (código de producto = `667a3991`).
- **Resultado verificable de F1 (Proposal V3.1 §6):** de `REF-CANT-01` y del archivo de cargas se obtiene un `StructuralModel` `Valid`,
  determinista, igual al golden derivado a mano; fallo cerrado (`Unsupported`/`Invalid`/`Unavailable` sin modelo); unidades explícitas con
  propietarios únicos y frontera de entrada única.
- **Métricas de F1:** 2 tareas delegadas, 0 correcciones (`attempts` = 0), 10 de 14 invocaciones de Codex, 2 invocaciones del Worker
  (29 min 36 s y 28 min 46 s), 1 revisión independiente del Architect; controles negativos 8 (nc4 ×2, nc1-nc3 ×2): 6 con oráculo cumplido,
  2 no (DEV-I65-F1-01 y DEV-I65-F1-02, no bloqueantes); 0 cambios de `config.toml`; 0 avances de `main`.
- **Autorización:** la F1 IMPLEMENTATION AUTHORIZATION del Owner **termina** con este cierre. Ningún gate posterior (F3 en adelante) está
  autorizado.

## 26. F3 — orden del Owner y preflight (2026-10-11T06:17:42Z)

Orden del Owner «I-65 — Owner Authorization: F3 RAM Elements Adapter» (pegada en el chat; copia byte a byte en
[I-65-owner-authorization-f3.original.txt](../decisions/I-65-owner-authorization-f3.original.txt)): F1 = ACCEPTED; autoriza **solo F3**.

| Comprobación del preflight | Resultado |
|---|---|
| Rama | `HEAD` = `origin/architecture/interop-analisis-estructural` = `237d03eb`; árbol limpio |
| `origin/main` | `bb0d5522`, sin cambio desde el reclamo |
| Cierre de F1 | GATE PASS sobre `4514a99a` (§25); CI `38116394272` y `38116666332` en verde |
| Autoridades congeladas | Proposal V3.1: blob `4e6e1feb` sin cambios desde el Freeze `a9d7080b`; ADR-0050: blob `c18def42`, el mismo que en su aceptación `ed8154c3` |
| Iniciativas paralelas | I-64 `ba52b3b9`, I-62 `efcfcbc5`, I-52 `fb6b5648`: ninguna toca `StructuralAnalysis`, `Units`, `StructuralSections`, `Systems/Cantilever`, `assets/` ni `.csproj`; `git merge-tree` limpio contra I-65; I-64 usa ADR-0047 |
| Rutas de F3 | decisiones §17, CD-I65-F3-03 |
| Presupuesto de Codex | 14 invocaciones para F3 (CD-I65-F3-02); `config.toml` sin tocar (línea base `F909F480…8113`) |
| API verificada | P1 (`p1-results.md`, `ram-probe-p1.executed.py`) y fuente del paquete instalado leída sin ejecutar RAM: `Restraints(dx, dy, dz, rx, ry, rz)`, `members.add(node_i, node_j, section, material="", rotation, description)` (material vacío = material por defecto de la sección: la plantilla siempre lo pasa explícito), `add_point_load(load_case, direction, value, distance, is_percentage, category)` |

## 27. F3-T1-MAP-AND-TRANSLATION — ejecución delegada (2026-10-11T07:20:52Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `bf2d3478`; `MainSha` `bb0d5522` sin cambio.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T061951Z-7db1` | PLANNING | Codex `gpt-6-luna`/`high` | paquete válido; seis colecciones idénticas al contrato; A1-A8 en `pass`; celda `claude-sonnet-5-5`/`high` (Deep) |
| `R20261011T062450Z-f0af` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual: **oráculo cumplido** |
| `R20261011T062529Z-8e09` | WORK | subagente `claude-sonnet-5-5`/`high` (143 mensajes) | `IMPLEMENTATION_COMPLETE`; RED `1c1e8650`, GREEN `16fa9e76`; 31 min 42 s; 84 llamadas, 0 errores |
| `R20261011T070131Z-8903` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `16fa9e7649e139798d41188db5bbe8b3be94e086` |
| `R20261011T070847Z-f93c` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `StopCondition`: **oráculo NO cumplido** |
| `R20261011T071344Z-c40c` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261011T071657Z-9428` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **Cambios:** 18 archivos. Siete nuevos en `Providers/RamElements/` (lector y validador de la tabla, fila y tabla con proyección, ejes
  locales, traducción, plan de RAM, códigos de diagnóstico); `StructuralMaterialProfile.cs` aditivo (fila de proveedor y prefijos de
  longitud); los siete enunciados de supuestos de `CantileverStructuralModelSource.cs` (CD-I65-F3-06); el archivo de datos con dos filas
  `proposed`; siete archivos de prueba nuevos; `StructuralAnalysisSourceGuardTests.cs` con exactamente las ediciones (a)-(c) en el RED.
- **CI push:** RED `38119009179` = `failure` (Core); GREEN `38119516204` = `success`, 4/4. TRX Core GREEN 13242/13242; filtro
  `FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElements` 296/296; TRX Core RED: 269 fallidas, todas del filtro (27 superadas:
  autopruebas de guardas y literales), sin errores de compilación.
- **Golden y P1:** el golden de RAM y la tabla de P1 (36 casos) concuerdan con las reglas; diferencias máximas 1.42e-14 y 3.31e-15.
- **Comprobaciones mecánicas de la sesión:** alcance PASS contra la delegación; guarda limitada a (a)-(c); pruebas solo altas más la guarda
  en el RED; prompt del Worker = `prompt.md`; términos de gate ausentes. Falsos P-02 releídos: procesos efímeros de **otra sesión** de
  Claude del escritorio (`claude.exe` 18312 y 38340) creados tras el fin de las cesiones.
- **Limitaciones declaradas por el Worker** (decididas en CD-I65-F3-10): lector más estricto que el Freeze en tres puntos que este no decide;
  porcentaje recortado a [0, 100] dentro de 1e-6.
- **Invocaciones de Codex en F3:** 5 de 14. `attempts` = 0.

Custodia (blob = identidad duradera; SHA-256 = procedencia del transitorio):

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/gate-contract.json` | `0F94F268AEB69A86…` | `0856035964bf` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/prompt.md` | `70BF4DF8DDF2C0EC…` | `3399565ee6bf` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/delegation.json` | `80E8353DCD39980F…` | `084d38f12a5d` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T061951Z-7db1/relay-record.json` | `ECC0CE8B6D17FE81…` | `5dc199bce763` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T062529Z-8e09/prompt.md` | `A40A30B3386D7820…` | `0feff0055c08` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T062529Z-8e09/worker-handoff.json` | `ABE242EEC485AA25…` | `68d22eec752e` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T062529Z-8e09/relay-record.json` | `AD7EF24BBD4D0E2B…` | `02dda5576718` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T070131Z-8903/prompt.md` | `93EA99700E3D440D…` | `a5e4c52d74fd` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T070131Z-8903/controller-verification.json` | `6462F2843B9C88D9…` | `acbfc084a789` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T070131Z-8903/relay-record.json` | `71A2717F27D2A5A5…` | `484cedbf3455` |
| `F3-T1-MAP-AND-TRANSLATION/R20261011T070131Z-8903/trx-summary.json` | `DC9B784DE3125601…` | `0e09401a989b` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION-nc1/R20261011T070847Z-f93c/prompt.md` | `846D28096754304B…` | `dde94f9dee73` |
| `F3-T1-MAP-AND-TRANSLATION-nc1/R20261011T070847Z-f93c/mutation.json` | `92347CC4797758F6…` | `647e81b03cee` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION-nc1/R20261011T070847Z-f93c/controller-verification.json` | `1FE8C2D2C17C0394…` | `a65916808cb7` |
| `F3-T1-MAP-AND-TRANSLATION-nc1/R20261011T070847Z-f93c/relay-record.json` | `318385F3CEB65B34…` | `a0d682fa4fe8` |
| `F3-T1-MAP-AND-TRANSLATION-nc1/R20261011T070847Z-f93c/exit-reread.txt` | `5C761396B835DBD3…` | `6c0b6b0b729a` |
| `F3-T1-MAP-AND-TRANSLATION-nc2/R20261011T071344Z-c40c/prompt.md` | `39538B05BBE9BA12…` | `90c006730ab4` |
| `F3-T1-MAP-AND-TRANSLATION-nc2/R20261011T071344Z-c40c/mutation.json` | `F88D3B0AFDDCB6FD…` | `b229adac8ae4` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION-nc2/R20261011T071344Z-c40c/controller-verification.json` | `B9B43537F5BF6F1D…` | `739e61d15b59` |
| `F3-T1-MAP-AND-TRANSLATION-nc2/R20261011T071344Z-c40c/relay-record.json` | `AC91BD6640D043F2…` | `f63ed8460d29` |
| `F3-T1-MAP-AND-TRANSLATION-nc2/R20261011T071344Z-c40c/session-scope-check.json` | `1209DD5E23D806E5…` | `381f1c269044` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION-nc3/R20261011T071657Z-9428/prompt.md` | `D7F4D4F1BDAD0DF5…` | `f5e810753203` |
| `F3-T1-MAP-AND-TRANSLATION-nc3/R20261011T071657Z-9428/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F3-T1-MAP-AND-TRANSLATION-nc3/R20261011T071657Z-9428/controller-verification.json` | `A2BE79E75DF61E67…` | `a1343075264b` |
| `F3-T1-MAP-AND-TRANSLATION-nc3/R20261011T071657Z-9428/relay-record.json` | `B66C5D5B5EDA4EA0…` | `57b3b83323ef` |
| `F3-T1-MAP-AND-TRANSLATION-nc4/R20261011T062450Z-f0af/delegation.json` | `A303DC0817834423…` | `eff1d1acb914` |
| `F3-T1-MAP-AND-TRANSLATION-nc4/R20261011T062450Z-f0af/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F3-T1-MAP-AND-TRANSLATION-nc4/R20261011T062450Z-f0af/relay-record.json` | `B5B754234C53F439…` | `44568674e1d6` |

## 28. F3-T2 — planificación rechazada y reemisión (2026-10-11T07:25:21Z)

- **DEV-I65-F3-01 (nc1 de F3-T1):** mismo modo que DEV-I65-F1-02: la mutación de `CurrentSha` cambia el SHA-256 de la entrega frente al
  registro del trabajo copiado; el Controller falla `Handoff` (y también `Identity`) y declara `StopCondition` con `BLOCKED/STOP`. Mutación
  detectada; oráculo estricto no cumplido; salida válida, no se repite.
- **Planificación `R20261011T072116Z-aea0` rechazada antes de invocar al Worker:** A5 `fail` (el invariante INV-10 volvió con la secuencia
  `—` con la barra invertida duplicada) y A8 `fail` (`Executor.Transport` con tilde). Disposición **STOP (P-03)**; causa raíz en
  `analysis.md`; decisión CD-I65-F3-11: reemisión del contrato con el mismo trabajo y sin la trampa de transcripción. Invocación de Codex 6
  de 14 en F3; `attempts` sin cambio (0).

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/gate-contract.json` | `E2FBD82A87804EB6…` | `13418cf38fd4` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/prompt.md` | `2EE5037F02546106…` | `89fb7a45ed8a` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/delegation-rejected.json` | `EFF33178B8D32667…` | `fad347b99991` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/acceptance.json` | `E2D88B56EFE95875…` | `a358e3afca4d` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/relay-record.json` | `DADB6C64777261FF…` | `83d3efefd16f` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072116Z-aea0/analysis.md` | `8E1AD8524B3DC7E5…` | `9f63150c9451` |

## 29. F3-T2-PACKAGE-AND-WRITER — ejecución delegada (2026-10-11T08:37:13Z)

Contrato reemitido (CD-I65-F3-11) sobre `AuthorityRevision` = `BaseSha` = `e0c1120b`; `MainSha` `bb0d5522` sin cambio.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T072637Z-c92e` | PLANNING | Codex `gpt-6-luna`/`high` | paquete válido; seis colecciones idénticas; A1-A8 en `pass`; `claude-sonnet-5-5`/`high` (Deep) |
| `R20261011T073019Z-5e40` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual: **oráculo cumplido** |
| `R20261011T073033Z-b3ec` | WORK | subagente `claude-sonnet-5-5`/`high` (181 mensajes) | `IMPLEMENTATION_COMPLETE`; RED `60014790`, GREEN `e3ae36d0`; 42 min 25 s; 103 llamadas |
| `R20261011T081954Z-40b1` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `e3ae36d0d6cb1f13de685ed0f9825d2b5a71ecf5` |
| `R20261011T082548Z-d437` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Identity`: **oráculo cumplido** |
| `R20261011T082840Z-a841` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261011T083227Z-37d6` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **Cambios:** 12 archivos: `Providers/RamElements/RamBuildPayload.cs`, `RamBuildScriptTemplate.cs`, `RamBuildManifest.cs` y
  `RamBuildPackageFactory.cs`; `StructuralAnalysis/StructuralPackageWriter.cs` (tipo `StructuralPackagePublisher`); siete archivos de prueba
  `StructuralAnalysisRamElementsPackage*`.
- **CI push:** RED `38123723750` = `failure` (Core); GREEN `38124008377` = `success`, 4/4. TRX Core GREEN 13483/13483; filtro 241/241;
  TRX Core RED 230 fallidas, todas del filtro (11 superadas), sin errores de compilación.
- **Golden:** la carga útil de `REF-CANT-01` reproduce el golden de RAM (diferencia máxima 1.4e-14).
- **Disposiciones del Coordinator** previas a la verificación (texto en las entradas de la verificación; registradas en decisiones §20):
  CD-I65-F3-12 (uso puntual de Python por el Worker, DEV-I65-F3-02, C-08 no activado), CD-I65-F3-13 (`StructuralPackagePublisher`),
  CD-I65-F3-14 (lecturas no materiales y limitaciones de la lectura de vuelta).
- **Simulacro del Coordinator** (fuera de la suite, NO es RAM Elements): [I-65-f3/coordinator-smoke/](I-65-f3/coordinator-smoke/README.md):
  `py_compile` correcto; contra un módulo `ramelements` falso que imita P1: `READBACK OK` sin diferencias; sin sobrescritura; puertas sin
  escritura ante modelo no vacío, versión distinta o función ausente; nombre desconocido → `FAILED — DISCARD MODEL`.
- **Relevos:** `config.toml` sin cambio; los procesos vivos creados durante la cesión del Worker eran de **otras sesiones** (planificación
  de I-64 sobre su propio worktree, kit de I-62, shells de otras sesiones), ninguno con la ruta de este worktree.
- **Invocaciones de Codex en F3:** 11 de 14 (planificación rechazada incluida). `attempts` = 0.

Custodia (blob = identidad duradera; SHA-256 = procedencia del transitorio):

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/gate-contract.json` | `3214737F9D590B95…` | `823f0eb98078` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/prompt.md` | `9C279DE7D8F07822…` | `e19159593117` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/delegation.json` | `FF31EC2AA14E0967…` | `28d8f20d1dba` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T072637Z-c92e/relay-record.json` | `95EC6AE6AB9FC8B1…` | `37cb5783d55e` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T073033Z-b3ec/prompt.md` | `3E0F0220634BF010…` | `16c8776ca6f8` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T073033Z-b3ec/worker-handoff.json` | `48A72F2166D43D8B…` | `789829f5885a` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T073033Z-b3ec/relay-record.json` | `B60C6B65FFAEC285…` | `72e843555d98` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T081954Z-40b1/prompt.md` | `9E99061DB47BCC23…` | `d37f26c6b4dc` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T081954Z-40b1/controller-verification.json` | `02707DE26AFA2347…` | `85b5f8d53785` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T081954Z-40b1/relay-record.json` | `785966AA01AE6BAA…` | `4edc71ce1229` |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T081954Z-40b1/trx-summary.json` | `7C00AD6B38B6F041…` | `62c3d1d9498f` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER/R20261011T081954Z-40b1/coordinator-dispositions-f3t2.md` | `DEF87EFA7BF56A0F…` | `8e0fa9a536b9` |
| `F3-T2-PACKAGE-AND-WRITER-nc1/R20261011T082548Z-d437/prompt.md` | `2ED7165D7298C07B…` | `478884f370b2` |
| `F3-T2-PACKAGE-AND-WRITER-nc1/R20261011T082548Z-d437/mutation.json` | `0FC7359FE2713141…` | `904d8945f6b9` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER-nc1/R20261011T082548Z-d437/controller-verification.json` | `EC837B8A01351E25…` | `cf4090e2ce3a` |
| `F3-T2-PACKAGE-AND-WRITER-nc1/R20261011T082548Z-d437/relay-record.json` | `09AA80EC37264DC8…` | `e65010ad0b34` |
| `F3-T2-PACKAGE-AND-WRITER-nc2/R20261011T082840Z-a841/prompt.md` | `2F777A40C212DD6F…` | `62f90d1e63bc` |
| `F3-T2-PACKAGE-AND-WRITER-nc2/R20261011T082840Z-a841/mutation.json` | `0D10D4E6AA4A840D…` | `c00f7f19d42f` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER-nc2/R20261011T082840Z-a841/controller-verification.json` | `42FF299B561A326C…` | `4941bfa9165b` |
| `F3-T2-PACKAGE-AND-WRITER-nc2/R20261011T082840Z-a841/relay-record.json` | `2F8F6801EF245973…` | `e8ce4b2845bd` |
| `F3-T2-PACKAGE-AND-WRITER-nc2/R20261011T082840Z-a841/session-scope-check.json` | `B4407913461D1DA8…` | `efc90d1cb516` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER-nc3/R20261011T083227Z-37d6/prompt.md` | `B91E52D4B81D1B8C…` | `d49ea94dd1ac` |
| `F3-T2-PACKAGE-AND-WRITER-nc3/R20261011T083227Z-37d6/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F3-T2-PACKAGE-AND-WRITER-nc3/R20261011T083227Z-37d6/controller-verification.json` | `D6878BC17BD12522…` | `0a486d7f02e0` |
| `F3-T2-PACKAGE-AND-WRITER-nc3/R20261011T083227Z-37d6/relay-record.json` | `406ED3C5FAF7A731…` | `0e752eb5df58` |
| `F3-T2-PACKAGE-AND-WRITER-nc4/R20261011T073019Z-5e40/delegation.json` | `93B0A7018D782B00…` | `77d7770f4024` |
| `F3-T2-PACKAGE-AND-WRITER-nc4/R20261011T073019Z-5e40/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F3-T2-PACKAGE-AND-WRITER-nc4/R20261011T073019Z-5e40/relay-record.json` | `CFF6DF730158D273…` | `d28d3d5c39a3` |

## 30. Cierre de F3 — revisión y criterios de LIFECYCLE §7 (2026-10-11T08:50:04Z)

- **Revisión independiente del Architect** sobre `bf2d3478..e3ae36d0 -- src tests assets`: **CONFORMANT WITH OBSERVATIONS**, sin
  REQUIRED ni violación del Freeze; 12 hallazgos OPTIONAL A65-F3-01..12
  ([architect-f3-conformance.md](I-65-f3/architect-f3-conformance.md)). Golden de RAM recalculado por el Architect y correcto; las 35
  funciones del núcleo de la puerta, contenidas en las 207 de P1.
- **Revisión del Coordinator** del diff contra el Freeze, la orden del Owner y CD-I65-F3-01..15: CD-I65-F3-10 (T1) y CD-I65-F3-15 (T2);
  disposición de los hallazgos en CD-I65-F3-16.
- **Criterios de LIFECYCLE §7 para F3** (estado en este commit):

| # | Criterio | Estado |
|---|---|---|
| 1 | Resultado observado por pruebas | sí: paquete RAM determinista y atómico de `REF-CANT-01` (con filas de fixture) igual al golden de RAM derivado a mano; guardas G1-G4, de plantilla (INV-10) y del proveedor; contrato de la carga útil (TRX de `38119516204` y `38124008377`) |
| 2 | RED→GREEN focal, selección > 0 | sí: F3-T1 296 (RED 269 fallidas) y F3-T2 241 (RED 230 fallidas), acreditados por el Controller |
| 3 | Pruebas y evidencia exigidas en verde | sí en los SHA de código (`16fa9e76`, `e3ae36d0`); Core Full local sobre el SHA de cierre: tras este commit |
| 4 | CI propio del SHA de cierre leído y verde | tras el push de este commit |
| 5 | Revisión del Coordinator contra Freeze + A-n, sin EXP-01 A abierta | sí (sin A-n; sin EXP-01 A) |
| 6 | Orden commit → árbol limpio → evidencia local → push → CI | en curso sobre este commit |
| 7 | Rebase entre gates | no hubo (`origin/main` = `bb0d5522` durante F3) |

- **Controles negativos de F3:** 8 de 9 con oráculo cumplido (nc4 ×2, nc2 ×2, nc3 ×2, nc1 de F3-T2); nc1 de F3-T1 no en sentido estricto
  (DEV-I65-F3-01). Planificación rechazada y reemitida una vez (CD-I65-F3-11).

## 31. F3 cerrada (2026-10-11T09:00:06Z)

Hechos posteriores al commit de cierre `1d8c713ee494a8cf5f2577e38dfca1aab9e2c236`:

- **Árbol limpio y evidencia local:** Core Full local sobre `1d8c713e`: **13483/13483** superadas, 0 fallidas, 0 omitidas (2 min 54 s; TRX
  `core-full-1d8c713e.trx`, SHA-256 `04adf819e7618a9893dca1563899408e3695877d419343a4e827d30d825e3c6e`, transitorio).
- **Push y CI del SHA de cierre:** corrida `push` 38126497464, `head_sha` `1d8c713e`, `completed/success`: UI Tests (WPF controls, net8.0-windows) = `success`; Tests (Domain + Application) = `success`; Build UI (WPF, valida API de Application) = `success`; Build Plugin without AutoCAD = `success`.
- **Criterios de LIFECYCLE §7:** 1-7 cumplidos (§30 y este apartado). **GATE PASS de F3** declarado por el Coordinator (CD-I65-F3-17) sobre
  `1d8c713e` (código de producto = `e3ae36d0`).
- **Resultado verificable de F3 (Proposal V3.1 §6):** paquete de construcción para RAM Elements determinista y atómico (manifiesto,
  script de plantilla fija y copia del archivo de cargas), con validación estricta previa y diagnósticos tipados; tabla de mapeo de
  proveedor única y verificable (entregada con filas `proposed`: toda exportación real es `Unsupported` hasta la verificación de un
  ingeniero); traducción determinista al sistema de RAM; guardas G1-G4, de plantilla y del proveedor; contrato de la carga útil.
- **Métricas de F3:** 2 tareas delegadas; 0 correcciones (`attempts` = 0); **11 de 14** invocaciones de Codex (con una planificación
  rechazada y reemitida); 2 invocaciones del Worker (31 min 42 s y 42 min 25 s); 1 revisión independiente del Architect; controles
  negativos 9 (8 con oráculo cumplido; DEV-I65-F3-01 no bloqueante); 1 simulacro del Coordinator fuera de la suite; 0 cambios de
  `config.toml`; 0 avances de `main`.
- **Autorización:** la IMPLEMENTATION AUTHORIZATION = F3 ONLY del Owner **termina** con este cierre. F4, F5 y F6 no están autorizadas.

## 32. Preparación de F4 — orden del Owner, preflight y paquete (2026-10-11T09:30:57Z)

Orden del Owner «I-65 — Owner Authorization: F4 Preparation and Engineering Validation» (pegada en el chat; copia byte a byte en
[I-65-owner-authorization-f4-preparation.original.txt](../decisions/I-65-owner-authorization-f4-preparation.original.txt)): F3 = ACCEPTED;
autoriza **solo la preparación de F4**; F4 IMPLEMENTATION AUTHORIZATION = NO.

| Comprobación del preflight | Resultado |
|---|---|
| Rama | `HEAD` = `origin/architecture/interop-analisis-estructural` = `ad5443bd`; árbol limpio; 42 commits sobre `origin/main` |
| `origin/main` | `bb0d5522`, sin cambio; base de merge `bb0d5522` |
| Autoridades congeladas | Proposal V3.1 blob `4e6e1feb` (último cambio `a9d7080b`); ADR-0050 blob `c18def42` (último cambio `ed8154c3`) |
| CI del cierre de F3 | `1d8c713e`: 38126497464 `success`; `ad5443bd`: 38126805055 `success`; 4/4 trabajos en `success` en ambas |
| Producción desde el cierre | `git diff 1d8c713e..HEAD -- src tests assets` vacío |
| Iniciativas paralelas | I-62 `5369b83b`, I-64 `f3a9f40e`, I-52 `fb6b5648`: 0 archivos en las áreas de I-65; común solo `docs/ROADMAP.md`; `merge-tree` limpio contra `ad5443bd`; I-64 toca `src/RackCad.Plugin/Workspace` (riesgo R-12 de F4) |
| Hallazgos y pendientes | A65-F3-01..12 abiertos (CD-I65-F3-16); A65-F1-08, 09, 11 a F4; A65-F1-03, 10, 12 a la primera tarea de idealización o familias; A65-F1-06 sin cambio (CD-I65-F1-20) |
| RAM instalado | `RAMElements.exe` SHA-256 `7dcb4855…7970` (= P1); RAM sin proceso en ejecución |
| Codex | 0 invocaciones; `config.toml` SHA-256 `f909f480…c811` (sin cambio) |

Trabajo de la preparación (sin código de producción, sin RAM):

- Pre-lectura pasiva de la base instalada de RAM (CD-I65-F4P-02): [ram-database-preread.md](I-65-f4-prep/ram-database-preread.md),
  script y salida custodiados. Hallazgos: nombres únicos en todas las bases de país; A, Ix, Iy y J de las dos secciones idénticos al
  catálogo; E y ν de los dos materiales iguales a los candidatos; peso unitario 0.28179435 lb/in³ = 486.94 pcf (−0.62 % frente a 490 pcf);
  base de usuario «Montilla» registrada en la configuración de RAM y no localizada.
- Paquete de validación de ingeniería: [I-65-f4-engineering-validation-package.md](../../initiatives/I-65-f4-engineering-validation-package.md)
  (matriz, especificación manual de `REF-CANT-01`, procedimiento de OV).
- Paquete de preparación: [I-65-f4-preparation.md](../../initiatives/I-65-f4-preparation.md) (preflight, riesgos, plan de pruebas,
  condiciones de aceptación, decisiones del Owner, GO/NO-GO).
- Siguiente: revisión documental independiente del Architect sobre este commit.

## 33. Preparación de F4 — revisión del Architect y correcciones (2026-10-11T09:54:52Z)

- **Revisión documental independiente del Architect** sobre `8992fe6c`: **NOT CONFORMANT** por A65-F4P-01 (REQUIRED, documental) con 12
  OPTIONAL; números recalculados y correctos; ninguna concreción cambia el Freeze
  ([architect-f4-prep-review.md](I-65-f4-prep/architect-f4-prep-review.md)).
- **CI de `8992fe6c`:** corrida `push` 38128785665 `success`.
- **Correcciones por diff** (CD-I65-F4P-05, 06) en [I-65-f4-engineering-validation-package.md](../../initiatives/I-65-f4-engineering-validation-package.md)
  y [I-65-f4-preparation.md](../../initiatives/I-65-f4-preparation.md).
- **Re-comprobación de iniciativas paralelas** (avanzaron durante la preparación): `origin/main` `bb0d5522`; I-62 `9f3e3d2a`, I-64 `e0205b5a`,
  I-52 `fb6b5648`: 0 archivos en las áreas de I-65 (incluido `src/RackCad.Plugin/RackCad.Plugin.csproj`), común solo `docs/ROADMAP.md`,
  `merge-tree` limpio contra la punta de I-65.
- Siguiente: cierre por diff del Architect sobre el commit de estas correcciones.

## 34. Preparación de F4 completa (2026-10-11T09:59:16Z)

- **Cierre por diff del Architect** sobre `d2539be2`: CONFORMANT WITH OBSERVATIONS; A65-F4P-01..13 CLOSED; A65-F4P-N01..N03 (OPTIONAL)
  corregidos por diff en este commit (CD-I65-F4P-07).
- **CI de `d2539be2`:** se lee tras el push de este commit junto con el propio.
- **Métricas de la preparación:** 0 invocaciones de Codex (`config.toml` sin cambio); 2 revisiones del Architect (revisión y cierre por
  diff); 1 pre-lectura pasiva de la base de RAM; 0 aperturas de RAM; 0 cambios en `src/`, `tests/` o `assets/`; `origin/main` sin avance.
- **Estado:** preparación completa (CD-I65-F4P-08); IMPLEMENTATION AUTHORIZATION = NO; espera de las decisiones OD-F4-01..11 del Owner.

## 35. F4 — orden del Owner y preflight (2026-10-11T15:43:46Z)

Orden del Owner «I-65 — Owner Authorization: Conditional F4 Implementation» (pegada en el chat; copia byte a byte en
[I-65-owner-authorization-f4.original.txt](../decisions/I-65-owner-authorization-f4.original.txt)): F4 IMPLEMENTATION AUTHORIZATION = YES,
CONDITIONAL; F4 ONLY; 28 invocaciones de Codex; sin corridas en RAM.

| Comprobación del preflight | Resultado |
|---|---|
| Rama | `HEAD` = `origin/architecture/interop-analisis-estructural` = `5bb1b9ba`; árbol limpio |
| `origin/main` | `bb0d5522`, sin cambio |
| Freeze y ADR-0050 | blobs `4e6e1feb` y `c18def42` |
| CI del cierre documental | `5bb1b9ba`: corrida `push` 38130345780 `success` (4/4) |
| Iniciativas paralelas | I-62 `92510286`, I-64 `e0205b5a`, I-52 `fb6b5648`: 0 archivos en `StructuralAnalysis`, `Units`, `Systems/Cantilever`, catálogos, `assets/structural-analysis`, pruebas de I-65, documentos de I-65, `RackCad.Plugin.csproj` y comandos; `merge-tree` limpio; I-64 toca solo `src/RackCad.Plugin/Workspace` (6 archivos) |
| Contratos de I-61 | `docs/automation/agent-execution/` (README, routing, esquemas) y AUTOMATION_PLAN §16 sin cambio (misma base `bb0d5522`) |
| Rutas | decisiones §26, CD-I65-F4-03; [I-65-f4-implementation.md](../../initiatives/I-65-f4-implementation.md) |
| Codex | 0 de 28 al empezar; `config.toml` `f909f480…c811` |

## 36. F4-T1-PACKAGE-HARDENING — intento 0 (2026-10-11T16:29:40Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `f64014f3`; `MainSha` `bb0d5522` sin cambio.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T154535Z-39b9` | PLANNING | Codex `gpt-6-luna`/`high` | paquete válido; seis colecciones idénticas al contrato; A1-A8 en `pass`; `claude-sonnet-5-5`/`high` (Deep) |
| `R20261011T154936Z-71eb` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual: **oráculo cumplido** |
| `R20261011T155212Z-11d5` | WORK | subagente `claude-sonnet-5-5`/`high` (137 mensajes) | `BLOCKED`; RED `53bfcd94` (CI `38154104233` Core `failure`: 103 de 153 de la selección por aserción), GREEN `a6fa5a35` (CI `38154505343` Core `failure`: 13 635/13 636, una aserción defectuosa del RED); 24 min 25 s; 86 llamadas, 0 errores |
| `R20261011T162337Z-4794` | VERIFICATION | Codex `gpt-6-luna`/`high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` `Ci`; RED acreditado; DEV-I65-F4-01 aceptada |

- **Cambios (intento 0):** `RamBuildScriptTemplate.cs` (tercera puerta `payload_failures`, prefijo `gate: `, `angle_near` módulo 360, sin
  comparación de descripción, `persist_readback` con respaldo) y `RamBuildPackageFactory.cs` (carpeta embebida expuesta, regla de carpeta con
  segmentos terminados en punto o espacio y nombres reservados); cuatro pruebas nuevas `StructuralAnalysisRamElementsHardening*`; ediciones de
  los cinco archivos de prueba nombrados; `StructuralPackageWriter.cs` sin cambios (la carrera del destino ya fallaba sin tocarlo).
- **Simulacro del Coordinator** (fuera de la suite, módulo `ramelements` **falso**, no RAM): S1 `READBACK OK`; SIM-01 giro leído 270 → OK;
  SIM-02 giro 0 → `FAILED` por ejes; SIM-03 descripción truncada → OK; SIM-04 bandera perdida → `FAILED`; SIM-05 fallo de `json.dumps` → lectura
  de vuelta de respaldo con `FAILED`; SIM-06 material vacío y SIM-07 caso `REF1` previo → `gate:` con 0 escrituras; S2-S6 como en F3.
- **Corrección:** decisiones §27 (CD-I65-F4-07, DEV-I65-F4-01 y 02); `attempts` = 1.
- **Invocaciones de Codex en F4:** 2 de 28.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/gate-contract.json` | `CBA1679B8A682326…` | `735b485834b7` |
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/prompt.md` | `D1F42A05B12B7C86…` | `0abe9d596ead` |
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/delegation.json` | `6684FEF24394CC8F…` | `102f2f03a472` |
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING/R20261011T154535Z-39b9/relay-record.json` | `1A9E600C8CCBA8EC…` | `7d26978a4e65` |
| `F4-T1-PACKAGE-HARDENING-nc4/R20261011T154936Z-71eb/delegation.json` | `0969D5441B61D611…` | `546692ce75e6` |
| `F4-T1-PACKAGE-HARDENING-nc4/R20261011T154936Z-71eb/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T1-PACKAGE-HARDENING-nc4/R20261011T154936Z-71eb/relay-record.json` | `9CDEEF97D8117210…` | `56ef3ac5c109` |
| `F4-T1-PACKAGE-HARDENING/R20261011T155212Z-11d5/prompt.md` | `AEB403B616698AF3…` | `bae4c0284acd` |
| `F4-T1-PACKAGE-HARDENING/R20261011T155212Z-11d5/worker-handoff.json` | `C5B9C526D9168482…` | `3181dd2b6da8` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING/R20261011T155212Z-11d5/relay-record.json` | `60E78285CED3206C…` | `2bf1567ec115` |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/prompt.md` | `3E3812EE80B46D3A…` | `2c5e5dc4f6fc` |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/controller-verification.json` | `C0A0C53325DCE7A7…` | `16eac1728049` |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/relay-record.json` | `D34EC75062B86A60…` | `de411801df2b` |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/trx-summary.json` | `CF66FDE6F27AA22D…` | `8b57b5d52c23` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/coordinator-dispositions-f4t1.md` | `3C00996C380E17E0…` | `8c4189d04183` |
| `F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/analysis.md` | `2AEDD7D047A1B786…` | (este commit) |

## 37. F4-T1-PACKAGE-HARDENING — corrección y controles (2026-10-11T17:09:22Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T163137Z-5315` | PLANNING (intento 1) | Codex `gpt-6-luna`/`high` | delegación de corrección válida; seis colecciones idénticas; A1-A8 en `pass`; `CorrectionOf` `R20261011T162337Z-4794`; `claude-sonnet-5-5`/`medium` (Balanced) |
| `R20261011T163420Z-5bee` | WORK (intento 1) | subagente `claude-sonnet-5-5`/`medium` (18 mensajes) | `IMPLEMENTATION_COMPLETE`; un commit `f5c6b885` sin RED (DEV-I65-F4-02): patrón `WholeToken` y autoprueba sintética en un solo archivo; 4 min 28 s; 10 llamadas; sin Python |
| `R20261011T164003Z-6ecb` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `f5c6b885`; DEV-I65-F4-02 admitida |
| `R20261011T164930Z-b2ea` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Handoff`: **oráculo NO cumplido** (DEV-I65-F4-03) |
| `R20261011T165257Z-97b9` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261011T165957Z-92bf` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **CI push:** `f5c6b885` → `38156339767` `success` (4/4); Core 13 637/13 637; filtro 154/154. El commit de sesión `ab2a0fe2` (solo docs) tiene
  su corrida `38155736708` en `failure` por la misma aserción defectuosa, ya corregida.
- **Procesos:** en las entradas de la verificación y de los controles aparecieron `git.exe` creados tras el fin de cada cesión, sin línea de
  órdenes legible; releídos por PID: terminados (uno, 20180, seguía vivo unos minutos y terminó poco después): falsos P-02 de la aplicación de
  escritorio.
- **Invocaciones de Codex en F4:** 7 de 28. `attempts` = 1.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/gate-contract.json` | `CBA1679B8A682326…` | `735b485834b7` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/prompt.md` | `4EB61998C6383A76…` | `f748c2dd41a1` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/delegation.json` | `6A8AD418DEC5C1EA…` | `c8886a0c999b` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING/R20261011T163137Z-5315/relay-record.json` | `B01FF39F71EC2D4B…` | `524934f7a89d` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163420Z-5bee/prompt.md` | `92F9A040192FC13E…` | `ccd808a3f050` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163420Z-5bee/worker-handoff.json` | `D001B1755BD80200…` | `791417926a22` |
| `F4-T1-PACKAGE-HARDENING/R20261011T163420Z-5bee/relay-record.json` | `F87F725A724F5F87…` | `4986e75e5957` |
| `F4-T1-PACKAGE-HARDENING/R20261011T164003Z-6ecb/prompt.md` | `90F7DC2532263333…` | `035782447c8b` |
| `F4-T1-PACKAGE-HARDENING/R20261011T164003Z-6ecb/controller-verification.json` | `956EA25950E1B30B…` | `947a386cd1ae` |
| `F4-T1-PACKAGE-HARDENING/R20261011T164003Z-6ecb/relay-record.json` | `D0DE4D156E10E891…` | `4b77cc13b328` |
| `F4-T1-PACKAGE-HARDENING/R20261011T164003Z-6ecb/trx-summary.json` | `ECA59C63393FBADF…` | `ced8155fa923` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING/R20261011T164003Z-6ecb/coordinator-dispositions-f4t1c1.md` | `D16602092A67E7E4…` | `a60935c85190` |
| `F4-T1-PACKAGE-HARDENING-nc1/R20261011T164930Z-b2ea/prompt.md` | `C356DE032596565B…` | `0488bebb430d` |
| `F4-T1-PACKAGE-HARDENING-nc1/R20261011T164930Z-b2ea/mutation.json` | `626BEA8ED538562F…` | `30e496bcfbbe` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING-nc1/R20261011T164930Z-b2ea/controller-verification.json` | `74F31BF62F8F8128…` | `24ddffdc2945` |
| `F4-T1-PACKAGE-HARDENING-nc1/R20261011T164930Z-b2ea/relay-record.json` | `39A5B3134B0B341C…` | `bb6283551862` |
| `F4-T1-PACKAGE-HARDENING-nc2/R20261011T165257Z-97b9/prompt.md` | `E10DE73C51BF744A…` | `1fa472660935` |
| `F4-T1-PACKAGE-HARDENING-nc2/R20261011T165257Z-97b9/mutation.json` | `EDA0954885BE23F7…` | `3ca01cac2426` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING-nc2/R20261011T165257Z-97b9/controller-verification.json` | `015BA4F24A842525…` | `b59f2ac0afdb` |
| `F4-T1-PACKAGE-HARDENING-nc2/R20261011T165257Z-97b9/relay-record.json` | `0ED8EAF34AAD3019…` | `b73c8608aa55` |
| `F4-T1-PACKAGE-HARDENING-nc3/R20261011T165957Z-92bf/prompt.md` | `3C3E8880C0641136…` | `7de93dc9b97b` |
| `F4-T1-PACKAGE-HARDENING-nc3/R20261011T165957Z-92bf/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T1-PACKAGE-HARDENING-nc3/R20261011T165957Z-92bf/controller-verification.json` | `23DA85D288A4B71F…` | `b31a9c739eee` |
| `F4-T1-PACKAGE-HARDENING-nc3/R20261011T165957Z-92bf/relay-record.json` | `4357D6576083D3A2…` | `396fa8a6721e` |

## 38. F4-T2-SOURCE-ENTRY — intento 1: STOP del Worker (2026-10-11T17:33:42Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `cd3fb69b`; `MainSha` `bb0d5522` sin cambio.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T171258Z-689c` | PLANNING | Codex `gpt-6-luna`/`high` | paquete válido; seis colecciones idénticas al contrato; A1-A8 en `pass`; `claude-sonnet-5-5`/`high` (Deep) |
| `R20261011T171639Z-6ff1` | CONTROL nc4 | sin invocar | `REJECTED_BEFORE_INVOCATION`: A3 `fail`, el resto igual: **oráculo cumplido** |
| `R20261011T171727Z-2e11` | WORK | subagente `claude-sonnet-5-5`/`high` (97 mensajes) | `BLOCKED`/`STOP` (C-09, C-01); **sin commits**; 11 min 31 s; 53 llamadas, 0 errores, sin Python |

- **Causa:** la entrada exigida en `Systems/Cantilever` con el almacén de proyecto choca con la guarda existente
  `CantileverSourceGuardTests.TheStationHasNoPersistenceAndNoView`; el Worker no la debilitó. Defecto del contrato (decisiones §29).
- **Resolución:** CD-I65-F4-11 (decodificador inyectado; contrato reemitido), DEV-I65-F4-04 (STOP aplicado sin Controller), `attempts` = 2.
- **Invocaciones de Codex en F4:** 8 de 28.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/gate-contract.json` | `1789597844C3CBDB…` | `71fc75632e74` |
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/prompt.md` | `D72CBE8E797BB980…` | `208ba4a4b171` |
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/delegation.json` | `643A7021A547BED6…` | `440ffed2f466` |
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY/R20261011T171258Z-689c/relay-record.json` | `0BF028CE47F36FBE…` | `c9e827544659` |
| `F4-T2-SOURCE-ENTRY-nc4/R20261011T171639Z-6ff1/delegation.json` | `0A7D2E5CF613A52C…` | `fba4a54dfb92` |
| `F4-T2-SOURCE-ENTRY-nc4/R20261011T171639Z-6ff1/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T2-SOURCE-ENTRY-nc4/R20261011T171639Z-6ff1/relay-record.json` | `8074229875C0E4B5…` | `2d75264e502f` |
| `F4-T2-SOURCE-ENTRY/R20261011T171727Z-2e11/prompt.md` | `4941E3B5E94236B4…` | `af3224554e3c` |
| `F4-T2-SOURCE-ENTRY/R20261011T171727Z-2e11/worker-handoff.json` | `75924E79819E0121…` | `8387c6b1c3ed` |
| `F4-T2-SOURCE-ENTRY/R20261011T171727Z-2e11/relay-record.json` | `A610515A9BEEA3B4…` | `423c49db9899` |
| `F4-T2-SOURCE-ENTRY/R20261011T171727Z-2e11/analysis.md` | `98D8394B5DCA0F5C…` | (este commit) |

## 39. F4-T2-SOURCE-ENTRY — corrección, verificación y controles (2026-10-11T18:22:59Z)

Contrato reemitido sobre `AuthorityRevision` = `BaseSha` = `dc609362` (CD-I65-F4-11).

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T173529Z-98df` | PLANNING (intento 2) | Codex `gpt-6-luna`/`high` | delegación de corrección válida; seis colecciones idénticas; A1-A8 en `pass`; `CorrectionOf` `R20261011T171727Z-2e11` (`StopCondition`); `claude-sonnet-5-5`/`high` |
| `R20261011T173811Z-0167` | WORK (intento 2) | subagente `claude-sonnet-5-5`/`high` (128 mensajes) | `IMPLEMENTATION_COMPLETE`; RED `2e92a0df` (CI `38161386081` Core `failure`: 47 de 66 por aserción o excepción), GREEN `0070ae40` (CI `38161863583` 4/4 `success`); 24 min 2 s; 78 llamadas; una invocación `python3 --version` (DEV-I65-F4-05) |
| `R20261011T180413Z-2afc` | VERIFICATION | Codex `gpt-6-luna`/`high` | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `0070ae40` |
| `R20261011T180928Z-cb1e` | CONTROL nc1 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Identity`: **oráculo cumplido** |
| `R20261011T181525Z-c59b` | CONTROL nc2 | Codex `gpt-6-luna`/`high` | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261011T181926Z-2487` | CONTROL nc3 | Codex `gpt-6-luna`/`high` | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **Cambios:** `CantileverStructuralExport.cs` (entrada nueva con decodificador inyectado), `CantileverStructuralModelSource.cs`,
  `StructuralModelBuilder.cs`, `StructuralDiagnostic.cs`; dos pruebas nuevas `StructuralAnalysisSourceEntry*`; `StructuralAnalysisCantileverBoundaryTests.cs`
  editada solo para el código de línea bloqueada (A65-F1-11).
- **Procesos:** `git.exe` efímeros de la aplicación de escritorio en salidas y entradas; releídos por PID: terminados (falsos P-02).
- **Invocaciones de Codex en F4:** 13 de 28. `attempts` = 2.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/gate-contract.json` | `DF44A35FFE4DC40F…` | `a199efcf8686` |
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/prompt.md` | `BD4B1CC6CDE24786…` | `6c8d2704a8d3` |
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/delegation.json` | `67B12D876D38B33E…` | `0ad7ea4f9301` |
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY/R20261011T173529Z-98df/relay-record.json` | `7787C6984BBAFBB7…` | `5fa857068ca6` |
| `F4-T2-SOURCE-ENTRY/R20261011T173811Z-0167/prompt.md` | `917831B19ACCAB02…` | `fbcfbff094a9` |
| `F4-T2-SOURCE-ENTRY/R20261011T173811Z-0167/worker-handoff.json` | `56DAAD8064953573…` | `5eb91439f088` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY/R20261011T173811Z-0167/relay-record.json` | `3B0D970E17108713…` | `9724b4dbf273` |
| `F4-T2-SOURCE-ENTRY/R20261011T180413Z-2afc/prompt.md` | `642DFC7D37F247A4…` | `838a41cc8452` |
| `F4-T2-SOURCE-ENTRY/R20261011T180413Z-2afc/controller-verification.json` | `D78400C6681BDA98…` | `e13c120f1c68` |
| `F4-T2-SOURCE-ENTRY/R20261011T180413Z-2afc/relay-record.json` | `0A30FC9CA9F5872E…` | `efe3e8dcd843` |
| `F4-T2-SOURCE-ENTRY/R20261011T180413Z-2afc/trx-summary.json` | `E07A6347386BFD05…` | `a4a63f7e9bae` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY/R20261011T180413Z-2afc/coordinator-dispositions-f4t2c2.md` | `F83739CF1584A271…` | `c7f5615f93db` |
| `F4-T2-SOURCE-ENTRY-nc1/R20261011T180928Z-cb1e/prompt.md` | `745DA4186985F3E6…` | `c6aefc34ec92` |
| `F4-T2-SOURCE-ENTRY-nc1/R20261011T180928Z-cb1e/mutation.json` | `8EB83DDDA73107D8…` | `043e1b3029a8` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY-nc1/R20261011T180928Z-cb1e/controller-verification.json` | `4CA00668728A649A…` | `27d40da20dbd` |
| `F4-T2-SOURCE-ENTRY-nc1/R20261011T180928Z-cb1e/relay-record.json` | `3105CEB6D1A03C27…` | `8955184f9975` |
| `F4-T2-SOURCE-ENTRY-nc2/R20261011T181525Z-c59b/prompt.md` | `25887BA9E3B5DF26…` | `fa420fa69f90` |
| `F4-T2-SOURCE-ENTRY-nc2/R20261011T181525Z-c59b/mutation.json` | `FFA3861B3A47471E…` | `139f63f133b2` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY-nc2/R20261011T181525Z-c59b/controller-verification.json` | `58254696F543C77E…` | `0d2956cb9e0a` |
| `F4-T2-SOURCE-ENTRY-nc2/R20261011T181525Z-c59b/relay-record.json` | `1FFD3E1594C13584…` | `ba718a290bd1` |
| `F4-T2-SOURCE-ENTRY-nc3/R20261011T181926Z-2487/prompt.md` | `6CA0811064FAFF58…` | `739058debeac` |
| `F4-T2-SOURCE-ENTRY-nc3/R20261011T181926Z-2487/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T2-SOURCE-ENTRY-nc3/R20261011T181926Z-2487/controller-verification.json` | `AF3685CE4DDCC6CB…` | `a9ccbadd6d07` |
| `F4-T2-SOURCE-ENTRY-nc3/R20261011T181926Z-2487/relay-record.json` | `72F660BB16953834…` | `e81b87748c1c` |

## 40. F4-T3-DUMP-AND-READERS — trabajo, parada, disposición del Owner y verificación (2026-10-11T20:20:51Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `c73058f8` (CD-I65-F4-13).

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T183102Z-7511` | PLANNING | Codex `gpt-6-luna`/`high` (14) | delegación válida; seis colecciones idénticas; A1-A8 en `pass`; `claude-sonnet-5-5`/`high`, `ROUTINE_IMPLEMENTATION` |
| `R20261011T183516Z-4d94` | CONTROL nc4 | sin invocación | A3 `fail` y las demás `pass`: oráculo cumplido |
| `R20261011T183618Z-05a9` | WORK | subagente `claude-sonnet-5-5`/`high` (211 mensajes) | RED `a283e0cc` (CI `38166596013` Core `failure`), GREEN `76db7642` (CI `38166926118` 4/4 `success`); 40 min 50 s; 124 llamadas; `BLOCKED/STOP` C-08 por `python --version` tras los commits (DEV-I65-F4-06) |
| `R20261011T192137Z-51fd` | VERIFICATION | Codex (15) | `EXECUTION_BLOCKED/STOP` C-08 con 14 `pass`: incoherente con README §8 (INVALID_OUTPUT); histórico sin modificar; STOP al Owner |
| `R20261011T194727Z-d7ea` | VERIFICATION | Codex (16) | inválida por **P-08** (la sesión leyó el worktree durante la cesión; DEV-I65-F4-07); no se usa |
| `R20261011T195706Z-dc0b` | VERIFICATION | Codex (17) | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, coherente, `VerifiedSha` `76db7642` |
| `R20261011T200306Z-807c` | CONTROL nc1 | Codex `gpt-6-luna`/`high` (18) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Identity`: **oráculo cumplido** |
| `R20261011T201200Z-f320` | CONTROL nc2 | Codex `gpt-6-luna`/`high` (19) | `EXECUTION_VERIFIED/NONE`, `FailureClass` = `NONE`: **oráculo NO cumplido** (esperado `EXECUTION_BLOCKED/STOP`, `Scope`): DEV-I65-F4-08 |
| `R20261011T201450Z-b8ab` | CONTROL nc3 | Codex `gpt-6-luna`/`high` (20) | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **Cambios:** siete archivos nuevos del proveedor (`RamDumpScriptTemplate.cs`, `RamDumpScriptFactory.cs`, `RamDumpPayload.cs`, `RamDumpReader.cs`,
  `RamTransferReaders.cs`, `RamStrictJson.cs`, `RamDumpDiagnosticCodes.cs`), `StructuralReadback.cs` (tipos neutrales) y siete archivos de
  pruebas `StructuralAnalysisRamElementsDump*`; ninguna prueba existente cambió; la exención de INV-14 sigue en cuatro rutas.
- **Ida y vuelta de REF-CANT-01:** 8 nudos, 7 miembros y 3 cargas; diferencia máxima 3.55e-15 (tolerancia 1e-9).
- **Procesos:** `git.exe` efímeros de la aplicación de escritorio en salidas y entradas; releídos por PID: terminados (falsos P-02).
- **Invocaciones de Codex en F4:** 20 de 30 (tope del Owner desde su disposición de F4-T3). `attempts` = 2.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/gate-contract.json` | `DDE1B854DC2018F6…` | `2d99d452ed60` |
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/prompt.md` | `72991BAAB1424B2E…` | `29c9579ef50e` |
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/delegation.json` | `7C9BE4C27967DF3D…` | `f4845a2971fd` |
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T183102Z-7511/relay-record.json` | `4080C958463FC1C7…` | `2db72d9e7114` |
| `F4-T3-DUMP-AND-READERS-nc4/R20261011T183516Z-4d94/delegation.json` | `4508D1E104D8A69B…` | `c15e335eb734` |
| `F4-T3-DUMP-AND-READERS-nc4/R20261011T183516Z-4d94/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T3-DUMP-AND-READERS-nc4/R20261011T183516Z-4d94/relay-record.json` | `EF8C91330560FC5A…` | `732e504bbaff` |
| `F4-T3-DUMP-AND-READERS/R20261011T183618Z-05a9/prompt.md` | `F86240C6A33098D5…` | `5bfa6053328d` |
| `F4-T3-DUMP-AND-READERS/R20261011T183618Z-05a9/worker-handoff.json` | `0FDD39EFFF185162…` | `0066972508d7` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T183618Z-05a9/relay-record.json` | `5701FC44E0BFA30F…` | `920396b3ec0a` |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/prompt.md` | `5595557372B499B0…` | `ca742126ab99` |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/controller-verification.json` | `6D05ABFE1A28D5C6…` | `b24c782bf9fb` |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/relay-record.json` | `3399A3929450224D…` | `0092f4a6849f` |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/coherence.txt` | `05C529D22BED87E1…` | `7bdc38331def` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/trx-summary.json` | `F2D3D570FDFF6770…` | `609e4372c418` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/analysis-f4t3.md` | `7FBF6A9786FD3237…` | `ac70657ef52d` |
| `F4-T3-DUMP-AND-READERS/R20261011T192137Z-51fd/coordinator-dispositions-f4t3.md` | `B1E6EB54B843A03D…` | `30b80113be4a` |
| `F4-T3-DUMP-AND-READERS/R20261011T194727Z-d7ea/prompt.md` | `6D33C4DD5FD11EC1…` | `47fd92166d14` |
| `F4-T3-DUMP-AND-READERS/R20261011T194727Z-d7ea/controller-verification.json` | `923B26501FE5D229…` | `6d438e769875` |
| `F4-T3-DUMP-AND-READERS/R20261011T194727Z-d7ea/relay-record.json` | `59AFE0BAE196561A…` | `975e43683518` |
| `F4-T3-DUMP-AND-READERS/R20261011T194727Z-d7ea/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T194727Z-d7ea/analysis-f4t3-p08.md` | `CB203D3B58200293…` | `6f939d897839` |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/prompt.md` | `33A2CE32EA26F50B…` | `5fa0db752e6b` |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/controller-verification.json` | `72EAA5CD9BFB8294…` | `2f637bb3ec23` |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/relay-record.json` | `520D13B6A0223C3C…` | `6f156f551b33` |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/trx-summary.json` | `F2D3D570FDFF6770…` | `609e4372c418` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/owner-disposition-f4t3.original.txt` | `66124E3CCC090F9D…` | `77d4fd1a61e0` |
| `F4-T3-DUMP-AND-READERS/R20261011T195706Z-dc0b/scope-mechanical.json` | `0861836B5EB5CB00…` | `84eed024693b` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS-nc1/R20261011T200306Z-807c/prompt.md` | `D17848F5B2DCA878…` | `e78a409ec2cd` |
| `F4-T3-DUMP-AND-READERS-nc1/R20261011T200306Z-807c/mutation.json` | `0F175497DAE0FBBA…` | `44aaa395b87f` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS-nc1/R20261011T200306Z-807c/controller-verification.json` | `429BDB095A743EF3…` | `65c414885bd1` |
| `F4-T3-DUMP-AND-READERS-nc1/R20261011T200306Z-807c/relay-record.json` | `16E6311C0D75A154…` | `fc96a31ec4dd` |
| `F4-T3-DUMP-AND-READERS-nc2/R20261011T201200Z-f320/prompt.md` | `41F2A2465876D5A0…` | `6b242aa554aa` |
| `F4-T3-DUMP-AND-READERS-nc2/R20261011T201200Z-f320/mutation.json` | `ED72D40D35641F28…` | `1fdee909f870` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS-nc2/R20261011T201200Z-f320/controller-verification.json` | `ECA183E07E0FC5F5…` | `730fcb097877` |
| `F4-T3-DUMP-AND-READERS-nc2/R20261011T201200Z-f320/relay-record.json` | `5E9618BD1578FAF7…` | `0aaf4ddf9f10` |
| `F4-T3-DUMP-AND-READERS-nc2/R20261011T201200Z-f320/scope-mechanical.json` | `0F90202B3039DA33…` | `07e0e99ce39b` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS-nc3/R20261011T201450Z-b8ab/prompt.md` | `5FF23A2ACDCE527E…` | `89ee1c108a4c` |
| `F4-T3-DUMP-AND-READERS-nc3/R20261011T201450Z-b8ab/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T3-DUMP-AND-READERS-nc3/R20261011T201450Z-b8ab/controller-verification.json` | `4E3C3143921E9733…` | `f7abf7b67691` |
| `F4-T3-DUMP-AND-READERS-nc3/R20261011T201450Z-b8ab/relay-record.json` | `55E4F156F772D7AF…` | `331d4b08bda0` |

## 41. F4-T4-COMPARATOR-AND-TOOL — intento 2: STOP del Worker (2026-10-11T20:56:01Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `34831380`.

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T202527Z-1167` | PLANNING | Codex `gpt-6-luna`/`high` (21) | delegación válida; seis colecciones idénticas; A1-A8 en `pass`; `claude-sonnet-5-5`/`medium` (Balanced), `ROUTINE_IMPLEMENTATION` |
| `R20261011T202728Z-e506` | CONTROL nc4 | sin invocación | A3 `fail` y las demás `pass`: oráculo cumplido |
| `R20261011T202738Z-6356` | WORK | subagente `claude-sonnet-5-5`/`medium` (130 mensajes) | RED `b51adf28` (CI `38173107295` Core `failure`: 75 de 81 del filtro), GREEN `0e9bbc29` (CI `38173786847` Core `failure`: 2 de 81 del filtro, las dos aserciones defectuosas); 22 min 24 s; 78 llamadas; ninguna invocación de un intérprete; `BLOCKED/STOP` C-01 |

- **Cambios del intento:** `StructuralReadbackComparison.cs`, `tools/RackCad.StructuralAnalysis.RamTool/` (proyecto, `Program.cs`, `RamToolCommands.cs`,
  `RamToolCompare.cs`, `RamToolDumpScript.cs`, `RamToolInject.cs`), `RackCad.sln`, `tests/RackCad.Tests/RackCad.Tests.csproj` y cuatro archivos
  `StructuralAnalysisComparison*`.
- **Decisión:** corrección (CD-I65-F4-18, DEV-I65-F4-09); `attempts` = 3. **Invocaciones de Codex en F4:** 21 de 30.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/gate-contract.json` | `25B05C775AA5D076…` | `c5613e7c4253` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/prompt.md` | `5C0E9E881234BC91…` | `d53057752125` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/delegation.json` | `DC6E51F5DA177815…` | `8deee85b35a7` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202527Z-1167/relay-record.json` | `66FB037D9AD82CC8…` | `52207c49e4a6` |
| `F4-T4-COMPARATOR-AND-TOOL-nc4/R20261011T202728Z-e506/delegation.json` | `922657D5E64C6F0F…` | `244ccb9b886b` |
| `F4-T4-COMPARATOR-AND-TOOL-nc4/R20261011T202728Z-e506/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T4-COMPARATOR-AND-TOOL-nc4/R20261011T202728Z-e506/relay-record.json` | `2ACA74C9996B3213…` | `246ceef4486a` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/prompt.md` | `527AA67BFC18C33F…` | `e64c39aef3e6` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/worker-handoff.json` | `A0E7AABF173FEF3C…` | `4831d792a809` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/relay-record.json` | `7D0A8AE83F3B11C7…` | `bfa52572f10d` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/trx-summary.json` | `DD2002E46EE73948…` | `86a227e636f7` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/analysis.md` | `5CAFA306B7430F69…` | `4018740f7b4e` |

## 42. F4-T4 — planificación de la corrección rechazada (2026-10-11T21:02:05Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T205717Z-064f` | PLANNING (intento 3) | Codex `gpt-6-luna`/`high` (22) | **rechazada**: A5 `fail` (`Authorities[0].Section` corrompida), `AnalysisSha256` erróneo; A2-A4 y A6-A8 `pass`; STOP P-03; sin Worker |

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/gate-contract.json` | `25B05C775AA5D076…` | `c5613e7c4253` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/prompt.md` | `457E787D8A8F0ABF…` | `438a7ea6bc40` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/delegation-rejected.json` | `CBC4D1EA4AC80097…` | `86a82ddbf8a6` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/acceptance.json` | `7FDFD7EFE56730E4…` | `e8d9a9549ec9` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/exactness.json` | `5F9D38644FF5683D…` | `976cf8edf012` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/relay-record.json` | `AA526480173C5D75…` | `e546cc25fe15` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T205717Z-064f/analysis-p03.md` | `EF3F21EA61344A8A…` | `f435beb28b18` |

## 43. F4-T4-COMPARATOR-AND-TOOL — corrección, verificación y controles (2026-10-11T21:32:42Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T210328Z-2ee6` | PLANNING (intento 3, reemitida) | Codex `gpt-6-luna`/`high` (23) | A1-A8 `pass`; C-07 abreviada por el Controller (DEV-I65-F4-11); `CorrectionOf` con el SHA-256 correcto; `claude-sonnet-5-5`/`medium` |
| `R20261011T210642Z-ca39` | WORK (intento 3) | subagente `claude-sonnet-5-5`/`medium` (38 mensajes) | RED `c4660703`, GREEN `7efacd78`; 5 min 58 s; 20 llamadas; ninguna invocación de un intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261011T211738Z-74b4` | VERIFICATION | Codex (24) | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `7efacd78` |
| `R20261011T212230Z-4f4f` | CONTROL nc1 | Codex `gpt-6-luna`/`high` (25) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Handoff`: **oráculo NO cumplido** (esperado `EXECUTION_BLOCKED/STOP`, `Identity`) |
| `R20261011T212448Z-fcb8` | CONTROL nc2 | Codex `gpt-6-luna`/`high` (26) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261011T212904Z-78f0` | CONTROL nc3 | Codex `gpt-6-luna`/`high` (27) | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **Diff de la corrección:** cuatro líneas en `StructuralAnalysisComparisonToolTests.cs` y `StructuralAnalysisComparisonSupport.cs` (dos aserciones
  corregidas, `CS0649` y `xUnit2013`); `git diff 0e9bbc29 7efacd78 -- src tools RackCad.sln` vacío.
- **Entregable de T4:** `StructuralReadbackComparison.cs` (comparador neutral puro) y la herramienta `tools/RackCad.StructuralAnalysis.RamTool/`
  (`dump-script`, `compare`, `inject`), con la entrada en `RackCad.sln` y la referencia del proyecto de pruebas.
- **Invocaciones de Codex en F4:** 27 de 30. `attempts` = 3 (= máximo).

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/gate-contract.json` | `25B05C775AA5D076…` | `c5613e7c4253` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/prompt.md` | `E4C6121123FCC4BD…` | `08f8be9c3281` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/delegation.json` | `CA5642BB4142239C…` | `6f3dc55fceba` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/acceptance.json` | `E12AFB35CBDE44C0…` | `c968ab8fdf67` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/exactness.json` | `CE1243A241341441…` | `0fece11d091a` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210328Z-2ee6/relay-record.json` | `FBFD6420B0D03638…` | `8d768954e2b8` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210642Z-ca39/prompt.md` | `61914EB1BAB6BF46…` | `81b460543cc1` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210642Z-ca39/worker-handoff.json` | `677B0B78281608F5…` | `6cbc037ac1be` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T210642Z-ca39/relay-record.json` | `57E069229F1F0E90…` | `35fc31eb248b` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/prompt.md` | `D487181F9DF48DB2…` | `22fe8434fcd8` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/controller-verification.json` | `34D52727B3EDBB2B…` | `edc58eed6f87` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/relay-record.json` | `849F0165243D1FAC…` | `9865027aa3e0` |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/trx-summary.json` | `1CE3B9E9FED44CF4…` | `a25f5c853f52` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/scope-mechanical.json` | `44AD85097CE259A7…` | `f2b787f4e0ef` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL/R20261011T211738Z-74b4/coordinator-dispositions-f4t4c.md` | `42A52DB8676CDF79…` | `ee6018f2cb72` |
| `F4-T4-COMPARATOR-AND-TOOL-nc1/R20261011T212230Z-4f4f/prompt.md` | `92183727E6C32D54…` | `8d4eb7a91dcc` |
| `F4-T4-COMPARATOR-AND-TOOL-nc1/R20261011T212230Z-4f4f/mutation.json` | `454115A35845BC30…` | `0f6eaff4ddde` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL-nc1/R20261011T212230Z-4f4f/controller-verification.json` | `D38424706C2F60DE…` | `0c2d3028f0bc` |
| `F4-T4-COMPARATOR-AND-TOOL-nc1/R20261011T212230Z-4f4f/relay-record.json` | `3D1BB4914200D7A9…` | `3361ace50b3c` |
| `F4-T4-COMPARATOR-AND-TOOL-nc2/R20261011T212448Z-fcb8/prompt.md` | `0FE4507F1B984FF0…` | `76d462113ca4` |
| `F4-T4-COMPARATOR-AND-TOOL-nc2/R20261011T212448Z-fcb8/mutation.json` | `826720F5C39932E8…` | `5d6819bae8ab` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL-nc2/R20261011T212448Z-fcb8/controller-verification.json` | `E3389C75DD4C2673…` | `c6ffdb7131d5` |
| `F4-T4-COMPARATOR-AND-TOOL-nc2/R20261011T212448Z-fcb8/relay-record.json` | `4D779D39B2BC9351…` | `ca933799dbf6` |
| `F4-T4-COMPARATOR-AND-TOOL-nc2/R20261011T212448Z-fcb8/scope-mechanical.json` | `3C708C9B874F0642…` | `61352aa73253` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL-nc3/R20261011T212904Z-78f0/prompt.md` | `E27A4EB98DBC61F1…` | `578511ebb564` |
| `F4-T4-COMPARATOR-AND-TOOL-nc3/R20261011T212904Z-78f0/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T4-COMPARATOR-AND-TOOL-nc3/R20261011T212904Z-78f0/controller-verification.json` | `632257C411BF78A3…` | `d22ead39b94c` |
| `F4-T4-COMPARATOR-AND-TOOL-nc3/R20261011T212904Z-78f0/relay-record.json` | `227FB83391CDDE5E…` | `37ab5a001ffc` |

## 44. F4-T5-PLUGIN-COMMAND — STOP del Worker (2026-10-11T22:13:27Z)

Contrato sobre `AuthorityRevision` = `BaseSha` = `283ed613` (incluye la revisión del Architect de T1-T4, CD-I65-F4-21).

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261011T215537Z-f944` | PLANNING (Attempt 3) | Codex `gpt-6-luna`/`high` (28) | delegación válida; seis colecciones idénticas; A1-A8 en `pass`; `claude-sonnet-5-5`/`high` |
| `R20261011T215832Z-a127` | CONTROL nc4 | sin invocación | A3 `fail` y las demás `pass`: oráculo cumplido |
| `R20261011T215838Z-09a6` | WORK | subagente `claude-sonnet-5-5`/`high` (93 mensajes) | **BLOCKED/STOP** C-08 (`python3 --version`) y C-05 (`placeholder.txt` en el checkout principal, borrado); sin commits; worktree sucio, preservado y restaurado por la sesión |

- **Hallazgo:** `RackCad.sln` del checkout principal modificado por el Worker de F4-T4 intento 2 (DEV-I65-F4-16); diff en la custodia.
- **Invocaciones de Codex en F4:** 28 de 30. `attempts` = 3 (= máximo): S-11.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/gate-contract.json` | `0974B91C8D5D80BB…` | `c46f65e2f379` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/prompt.md` | `DBAEC1C5CC5BADF0…` | `2c725e74e3d6` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/delegation.json` | `F357CD928321CFFC…` | `6787d8d001e0` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261011T215537Z-f944/relay-record.json` | `A3D9355D88E4A259…` | `61d7e7c9a8eb` |
| `F4-T5-PLUGIN-COMMAND-nc4/R20261011T215832Z-a127/delegation.json` | `6885F476C9764DDA…` | `2800d01bcd58` |
| `F4-T5-PLUGIN-COMMAND-nc4/R20261011T215832Z-a127/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T5-PLUGIN-COMMAND-nc4/R20261011T215832Z-a127/relay-record.json` | `6C40F2746CB8F34E…` | `ac0be394f4e2` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/prompt.md` | `58A24ADD23A758D2…` | `0fe074dc86e9` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/worker-handoff.json` | `E2D3E9DAF3B0BF33…` | `74f209e9a6ea` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/relay-record.json` | `6083A1E19B870554…` | `15aa76153057` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/analysis.md` | `1941BEA23566709B…` | `43a21c0570fa` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/main-checkout-RackCad.sln.diff` | `4BA20015ECA06AED…` | `af1499804563` |
| `F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/worktree-dirty.diff` | `CF2E4B632AFFDA3C…` | `428552020567` |

## 45. F4 detenida (Option C): incidente del checkout principal, informe parcial y escalamiento (2026-10-12T07:21:46Z)

- **Orden del Owner** en custodia: `docs/automation/decisions/I-65-owner-decision-f4-option-c.original.txt` (SHA-256 `06046F38DF1DD7B0E0B4301342CD2432B69356C95EF2497A0C33B5C14BD70A5F`).
- **Inspección del checkout principal** (solo lectura): `RackCad.sln` actual SHA-256 `4F38D61B8C09C84404D6E58C5E8F3FBB1D62E504699E2C078AD44F9EEF579C4C`
  (sin BOM, CRLF, 144 líneas, escrito `2026-10-11T20:32:56.8408396Z`); blob de `HEAD` `5317eece…` SHA-256 `DEB08FB0DFDEAFBC4DC4A262A285FAB2F964FF3844A6AF449E6621B1DB57FC14`;
  estado previo reconstruido SHA-256 `A06892EAE7EA841CED3612961C77FE83815794C1D9110B8FEE184B443086E399`; diff SHA-256
  `4BA20015ECA06AED38D4351FADAD7E11D40888ED8BA4832A536412843AF6E8F6`; reproducción de la llamada 25 idéntica byte a byte. Otras modificaciones:
  ninguna (Git y barrido de fechas desde el 2026-10-09). Restauración: preparada, no ejecutada.
- **Barrido de ejecuciones:** 26 transcripciones de subagentes y 28 registros de eventos de Codex de F4: ninguna ejecución del programa de
  análisis ni de los scripts generados; siete Workers con invocaciones de intérprete (dos de F1 sin registro previo, DEV-I65-F4-17).
- **Documentos:** [informe parcial de F4](../../initiatives/I-65-f4-partial-closure.md) (F4 INCOMPLETE / BLOCKED) y
  [paquete de escalamiento a I-62](I-65-f4/i62-escalation-package.md).
- **Sin Workers, sin Codex** (28 de 30 sin cambio), sin cambios de producto, sin RAM.

Hashes de la evidencia del incidente (copias de la sesión; Git puede normalizar el fin de línea de las copias versionadas):

| Archivo (bajo `I-65-f4/incident-main-checkout/`) | SHA-256 |
|---|---|
| `README.md` | `4A659304BEFEE6A8FE4E8FD100611C965E7B7E6FF00AEFE43B0B162E3E11C182` |
| `main-checkout-facts.json` | `40C27825F5EA42AEC3C96F4DE5D3BFD7A2B30942EA65727320356D1EA384CD38` |
| `main-checkout-RackCad.sln.diff` | `4BA20015ECA06AED38D4351FADAD7E11D40888ED8BA4832A536412843AF6E8F6` |
| `main-checkout-RackCad.sln.modified.txt` | `4F38D61B8C09C84404D6E58C5E8F3FBB1D62E504699E2C078AD44F9EEF579C4C` |
| `worker-call-25.ps1.txt` | `2DD27A2CDC62A3D6BB3FFA9551E545689339EBC0BDBDA5D913F1CA714FE9DE76` |
| `simulate-worker-call-25.ps1` | `86C0E0894A9F2FB04B7B8A20F227D7A5F97260F2B6D5CA93E929E1959D7FC427` |
| `simulation.json` | `F319A03456F338D515089064E004C90FB2DF9F495204BA52CD749949846FA825` |
| `main-checkout-mtime-scan.json` | `2C58CD0A710B6E0B2769D2651B2EFEB106216163D14BE4284848EF6519D05A12` |
| `main-checkout-mtime-scan-wide.json` | `E135256F468ED3DC15AA0B24A63FAE86A63A042025BF99E743B4F8BD945CACAB` |
| `ram-execution-scan.json` | `6934545FF73755E114AF9ACC075710BAA4EFDD8C44CEDB6D9FC3859EFCE24793` |

## 46. Restauración controlada del checkout principal (2026-10-12T07:35:12Z)

- **Orden del Owner** en custodia: `docs/automation/decisions/I-65-owner-authorization-checkout-restoration.original.txt` (SHA-256 `FDB3CDA902932DEE31CFF4A1138DF35FD1FA5DC1688835FF711F9158AA7B2796`).
- **Previas** (`restore-precheck.json`, 2026-10-12T07:33:50Z): 1 hash `4F38D61B…` pass; 2 solo ` M RackCad.sln`, índice y `stash` vacíos, pass;
  3 evidencia en `3b4bf6a0` y cuatro copias = `4F38D61B…`, pass; 4 `HEAD` `95690c28`, blob `5317eece`, materializado `A06892EA…`, pass.
- **Restauración:** `git restore --worktree --source=HEAD -- RackCad.sln` en el checkout principal, salida 0 (07:34:04Z).
- **Posteriores** (`restore-postcheck.json`, 07:34:24Z): hash final `A06892EAE7EA841CED3612961C77FE83815794C1D9110B8FEE184B443086E399`; estado limpio;
  BOM presente, CRLF, sin `RamTool`, 7 proyectos existentes, sin configuraciones huérfanas; único archivo escrito desde 07:12Z: `RackCad.sln`.
- **Sin Workers, sin Codex** (28 de 30), sin cambios de producto, sin RAM. F4 sigue INCOMPLETE / BLOCKED.

## 47. Recuperación del Coordinator y reanudación controlada de F4 (2026-10-13)

- **Textos del Owner** en custodia, extraídos byte a byte de la transcripción de la sesión del Coordinator (cuerpo pegado, UTF-8, sin el
  envoltorio); el archivo añade un salto de línea final, como los anteriores. Los seis primeros coinciden con los SHA-256 comunicados al
  Owner antes del registro.

| # | Archivo (`docs/automation/decisions/`) | ID | Bytes del cuerpo | SHA-256 del cuerpo | SHA-256 del archivo |
|---|---|---|---|---|---|
| 1 | `I-65-owner-order-coordinator-recovery.original.txt` | OD-REC | 7567 | `c7877f4caaca22e1234bbc4d05de92bfbfa8d849ca83ac8485834b28ecc738e2` | `D30F81F1781AEEF1011256362752E9637FA57A66EB1F1D8971F0EBBD49FB3A16` |
| 2 | `I-65-owner-disposition-environment-b.original.txt` | OD-ENVB | 3782 | `22c6d962e1d3a53a827d997d60031320ea2d17e7ccd5fec6a6a2366523a5268b` | `985E71AD1AD490F609F2C6059CA0F1D5EEE9074AA1E28C9B26BD49F5A8F60CCA` |
| 3 | `I-65-owner-disposition-environment-b-architecture.original.txt` | OD-ENVB-ARQ | 3446 | `701b8ced6789656aab11cb27e51d6e3304f1c06c4984d95f4724b08405dbd758` | `A2F76BFCB41401E52111B94452F944BB572D42CA2A6110E7BD5CC91150DFB832` |
| 4 | `I-65-owner-decision-publication-simplification.original.txt` | OD-PUB | 2634 | `974d7c4628e8326714f867f620635a64c4a0e355557c69490327ea99e651cfe5` | `0B65E53BAE4B2624FEB74445305F9C7A6D1ED3AB66AF2D352F067C0C10C67D99` |
| 5 | `I-65-owner-disposition-model-s1.original.txt` | OD-S1 | 4122 | `12ba5e90a7412d2bdc8f05cd612a85343196d2c92fcbe634a13cb839a407324d` | `25151A6145A003BF37D97244DD9920011D60F48C1BFEB1616F81E61E05937F99` |
| 6 | `I-65-owner-disposition-adr0046-exception.original.txt` | OD-ADR0046-3 | 1735 | `b2a7d0d5cb7387ef2a90e7c26036121cb952ae7e7b8477d452121de95d8fb398` | `9BDEB2806A9F48DC417E3EFA28F8FFA5AF1C6C159CFE41A80A11991DE2F2DA9C` |
| 7 | `I-65-owner-disposition-return-original-environment.original.txt` | OD-RET | 3268 | `3c3a40d9936971cddadc85fd693102f7b6fec790049b3c19d482a3d24d176705` | `C515C9C6C6C374476A0D9BE09D147B2882927A1C0F7E7A6B1CF057240A2B47DB` |
| 8 | `I-65-owner-authorization-f4-resumption.original.txt` | OD-GO | 5230 | `2e53a0b8825a6acefef95f303983de5c37c8afaaa1fb23cd858ecbed81bd28ea` | `2297DF2CD296C66C65CAC950747C908AF23C9E79E1D4A07CDCE9B8768676EBF6` |

- **Preflight de recuperación** (solo lectura, CD-I65-F4-27): rama y `origin` en `2c9b4ca6`; CI `38218663573` 4/4 `success` (Core 14 078/14 078;
  UI 1 637 superadas de 1 654); `origin/main` = `bb0d5522`; checkout principal limpio, `RackCad.sln` SHA-256
  `A06892EAE7EA841CED3612961C77FE83815794C1D9110B8FEE184B443086E399`; Freeze blob `4e6e1feb`, ADR-0050 blob `c18def42`; `git diff 7efacd78 2c9b4ca6`
  vacío en `src`, `tools`, `tests`, `RackCad.sln`, `assets` y `deploy`; repositorio público y `main` sin protección de rama (solo la regla de tags
  `integration/*`).
- **Preflight de reanudación** (CD-I65-F4-29): I-65 sin cambios; `origin/main` = `bb0d5522`; I-64 `09f2f1c3` (no integrada; censo 38 con
  `RACKPANEL`; Smoke-1 en curso en el host); I-62 `9b8b66a2`; I-52 `fb6b5648`; ningún proceso con la ruta del worktree de I-65 ni AutoCAD,
  RAM Elements o Python en ejecución.
- **Contadores:** Codex 28 de 46; `attempts` 3, `max_attempts` 6. Sin Workers ni Codex en la recuperación.

## 48. F4-T6-COMPARATOR-HARDENING — planificación, trabajo, verificación y controles (2026-10-13T22:49:13Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261013T213942Z-4bec` | PLANNING (intento 3) | Codex `gpt-6-luna`/`high` (29) | A1-A8 `pass`; seis colecciones exactas; `claude-sonnet-5-5`/`high`, Deep |
| `R20261013T214329Z-702f` | CONTROL nc4 | sin invocación | A3 `fail`, resto `pass` |
| `R20261013T214406Z-81c3` | WORK (no lanzado) | — | AutoCAD abierto con el DLL de I-64: pospuesto (CD-I65-F4-37) |
| `R20261013T214908Z-260e` | WORK (intento 3) | subagente `claude-sonnet-5-5`/`high` (230 mensajes) | RED `d655071b`, GREEN `6422e5b0`; 37 min; 143 llamadas; 0 errores; 0 denegaciones; ninguna invocación de un intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261013T222951Z-ece3` | VERIFICATION | Codex (30) | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `6422e5b0` |
| `R20261013T223652Z-da68` | CONTROL nc1 | Codex `gpt-6-luna`/`high` (31) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Identity`: **oráculo cumplido** |
| `R20261013T224014Z-cba7` | CONTROL nc2 | Codex `gpt-6-luna`/`high` (32) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261013T224435Z-bccb` | CONTROL nc3 | Codex `gpt-6-luna`/`high` (33) | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` = `FreeText`: **oráculo cumplido** |

- **CI:** RED `38476630564` (Core `failure`: 131 fallidas, todas en el filtro; sin errores de compilación); GREEN `38477281815` (4/4 `success`; Core
  14269/14269; UI 1 637 de 1 654).
- **Controles del Owner (CD-I65-F4-32):** K1/K2 antes y después de cada cesión (planificación, trabajo, verificación, nc1-nc3) sin cambios en el
  checkout principal (2 151 rastreados, mismo resumen SHA-256) ni en los otros worktrees; K3 informado por el Worker; K4 alcance mecánico `pass`
  (20 archivos); K5 sin intérpretes, RAM, AutoCAD ni accesos fuera del worktree (`k5-transcript-scan.json`).
- **Procesos:** en la entrada de nc1, un `git.exe` ilegible creado a las 22:40:03Z, cuatro segundos después del fin de la cesión, ya terminado al
  comprobarlo: falso P-02 (precedente de F4-T4). En la salida de la planificación, del trabajo, de la verificación y de nc3, `git.exe` ilegibles
  releídos a los 2 s y terminados.
- **Entregable:** `StructuralReadbackComparison.cs`, `StructuralReadback.cs` (añadidos), `RamTransferReaders.cs`, `RamDumpReader.cs` y la herramienta
  (`compare` con `--combinations-decision`, entradas iguales, `inputs` e inyección); 14 archivos de prueba nuevos o editados y fixtures de ensayo.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/gate-contract.json` | `FBB75886AFB875C0…` | `aec9de59c6f2` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/prompt.md` | `C938D28607A03A33…` | `fb6edd28e399` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/delegation.json` | `0B4B7E30C6AB1A1E…` | `bf65ce9988b9` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T213942Z-4bec/relay-record.json` | `B7F00404FE2A3756…` | `fe6600967b56` |
| `F4-T6-COMPARATOR-HARDENING-nc4/R20261013T214329Z-702f/delegation.json` | `57D8B8EAE3A1B783…` | `c99707b4c944` |
| `F4-T6-COMPARATOR-HARDENING-nc4/R20261013T214329Z-702f/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-T6-COMPARATOR-HARDENING-nc4/R20261013T214329Z-702f/relay-record.json` | `27B1640E68E2CD61…` | `43197175e9fa` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214406Z-81c3/prompt.md` | `992B82CAB5E38147…` | `c32195b8fb15` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214406Z-81c3/not-launched.txt` | `FF023513A0777859…` | `8f76a4b24eb7` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214908Z-260e/prompt.md` | `926BFEC89C2569F4…` | `e6ab95355589` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214908Z-260e/worker-handoff.json` | `1A0280A0B443BB62…` | `de8aec7568fb` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214908Z-260e/relay-record.json` | `D8D9F5E8E97B9E98…` | `1a010019946e` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214908Z-260e/k5-transcript-scan.json` | `C7A6B190BA29A176…` | `3e0fe82d984e` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T214908Z-260e/transcript-facts.json` | `8EB504A5BEE6920F…` | `c9d67584de4a` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/prompt.md` | `F052D51347217016…` | `85e0a4f5b851` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/controller-verification.json` | `B57E534A5B67F208…` | `8c38ce2d176a` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/relay-record.json` | `81F8722BA68BE8DE…` | `0255aaa2dd8a` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/trx-summary.json` | `0A923E1A6C329C7D…` | `46c30a408a57` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/scope-mechanical.json` | `46EBE41401C34C99…` | `7e50c2eb3f78` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/coordinator-dispositions-f4t6.md` | `ECE16FAEB0AF04F5…` | `ec4f201bdda9` |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-plan-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-work-compare.json` | `37434A9887D381C9…` | `b4f3713d04cf` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-ver-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-nc1-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-nc2-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING/R20261013T222951Z-ece3/k-f4t6-nc3-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING-nc1/R20261013T223652Z-da68/prompt.md` | `BD8F6DB3A62EB680…` | `dd63770749e7` |
| `F4-T6-COMPARATOR-HARDENING-nc1/R20261013T223652Z-da68/mutation.json` | `035458857D0E54AC…` | `aeaaf7ab1b41` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING-nc1/R20261013T223652Z-da68/controller-verification.json` | `60C03A2D8039CD7C…` | `b5e9948af569` |
| `F4-T6-COMPARATOR-HARDENING-nc1/R20261013T223652Z-da68/relay-record.json` | `116CFF9846F4844C…` | `bde3f0d95b9f` |
| `F4-T6-COMPARATOR-HARDENING-nc2/R20261013T224014Z-cba7/prompt.md` | `7B8456174AFAD227…` | `bebe7b01d97c` |
| `F4-T6-COMPARATOR-HARDENING-nc2/R20261013T224014Z-cba7/mutation.json` | `C58BA31AA2A3479E…` | `1b24c86c38ac` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING-nc2/R20261013T224014Z-cba7/controller-verification.json` | `E9255794F8F44564…` | `8f0ce84876ab` |
| `F4-T6-COMPARATOR-HARDENING-nc2/R20261013T224014Z-cba7/relay-record.json` | `DD3C26323AB640F8…` | `9cf631245647` |
| `F4-T6-COMPARATOR-HARDENING-nc2/R20261013T224014Z-cba7/scope-mechanical.json` | `59201A8F17915797…` | `be790c1012cd` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING-nc3/R20261013T224435Z-bccb/prompt.md` | `236EBF9F1797BE69…` | `12df5fb1b274` |
| `F4-T6-COMPARATOR-HARDENING-nc3/R20261013T224435Z-bccb/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T6-COMPARATOR-HARDENING-nc3/R20261013T224435Z-bccb/controller-verification.json` | `E7CD35D2E3A169DE…` | `678bf9925206` |
| `F4-T6-COMPARATOR-HARDENING-nc3/R20261013T224435Z-bccb/relay-record.json` | `F9C3195A982EA853…` | `ae7c8c7a0fff` |

## 49. F4-T5-PLUGIN-COMMAND — corrección (intento 4): STOP C-09 (2026-10-13T23:16:36Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261013T224954Z-16f8` | PLANNING (intento 4, corrección) | Codex `gpt-6-luna`/`high` (34) | A1-A8 `pass`; seis colecciones exactas; `CorrectionOf` `R20261011T215838Z-09a6`; `claude-sonnet-5-5`/`medium` (Balanced) |
| `R20261013T225238Z-960c` | WORK (intento 4) | subagente `claude-sonnet-5-5`/`medium` (120 mensajes) | **BLOCKED/STOP C-09**; RED `3e1ef603` (CI `38482239303` Core `failure`); sin GREEN; 19 min; 67 llamadas; ninguna invocación de un intérprete |

- **Controles del Owner:** K1/K2 sin cambios en el checkout principal ni en los demás worktrees (planificación y trabajo); K5 sin intérpretes, RAM,
  AutoCAD ni escrituras fuera del worktree; una lectura del checkout principal con `git status` (DEV-I65-F4-20).
- **Archivos del GREEN retenidos** (no verificados) en custodia como `hold-*.txt` para trazabilidad.
- **Contadores:** Codex 34 de 46; `attempts` 5 (máximo 6).

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/gate-contract.json` | `6A4685883AE86B7A…` | `3ccf8f01b77b` |
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/prompt.md` | `0D7B37D4151F00BF…` | `427dfbcf6325` |
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/delegation.json` | `2C7D93724A05F5FE…` | `a183a78ea512` |
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T224954Z-16f8/relay-record.json` | `51A06A0924491939…` | `28fddd636f10` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/prompt.md` | `291B68B4EE5AC182…` | `bf44ae29a1f3` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/worker-handoff.json` | `AED0224AC1C84047…` | `c7dd756e8df7` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/relay-record.json` | `48ED8FFBE8A7E346…` | `5970fa2d1a98` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/analysis.md` | `BB18FEDE5A6FE6DC…` | `3ce57ea8402c` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/k5-transcript-scan.json` | `0034FC1F1D48B8F2…` | `1c5296787f96` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/transcript-facts.json` | `5189452342001C4F…` | `ee51beb5fb97` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/k-f4t5-plan-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/k-f4t5-work-compare.json` | `A13C15824CF431EA…` | `409ee963e06b` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/hold-RackRamElementsCommands.cs.txt` | `8ACBBAC32DFB9E83…` | `5651cc348cf2` |
| `F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/hold-RackCad.Plugin.csproj.txt` | `6BD4DB8580E3D165…` | `81924bb0d61c` (fin de línea normalizado por Git) |

## 50. F4-T5-PLUGIN-COMMAND — corrección (intento 5): trabajo y verificación con REWORK (2026-10-13T23:43:06Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261013T231726Z-7d47` | PLANNING (intento 5, corrección) | Codex `gpt-6-luna`/`high` (35) | A1-A8 `pass`; seis colecciones exactas; `CorrectionOf` `R20261013T225238Z-960c` |
| `R20261013T232041Z-8f39` | WORK (intento 5) | subagente `claude-sonnet-5-5`/`medium` (35 mensajes) | RED `a2a8b868`, GREEN `f1d9a51d`; 9 min; 19 llamadas; 0 errores; ninguna invocación de un intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261013T233523Z-b1dd` | VERIFICATION | Codex (36) | **`EXECUTION_REWORK_REQUIRED`/`REWORK`**, `FailureClass` `Tests`; RED acreditado (`RedPart` `pass` en `Ci` y `Tests`) |

- **CI:** RED `38484201964` (Core `failure`: 21 fallidas: las del filtro y las tres ediciones que dependen del comando); GREEN `38484644494`
  (4/4 `success`; Core 14368/14368; UI 1 637 de 1 654). La CI de `af04ddb5` terminó en `failure` porque contiene el RED de `3e1ef603`.
- **Controles del Owner:** K1/K2 sin cambios externos en planificación, trabajo y verificación; K5 sin intérpretes, RAM, AutoCAD ni accesos fuera
  del worktree; alcance mecánico `pass` (3 archivos; y el RED del intento 4, también `pass`).
- **Contadores:** Codex 36 de 46; `attempts` 6 (máximo 6).

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/gate-contract.json` | `6A4685883AE86B7A…` | `3ccf8f01b77b` |
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/prompt.md` | `EE0A4F1EA3C7B5CD…` | `8a0b5e85a3db` |
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/delegation.json` | `CA2E22EB516C408A…` | `4668317fef95` |
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T231726Z-7d47/relay-record.json` | `FFFDB3FFFDAA21F3…` | `36b91dcc1740` |
| `F4-T5-PLUGIN-COMMAND/R20261013T232041Z-8f39/prompt.md` | `BADD1C239DE3075E…` | `37540700e3df` |
| `F4-T5-PLUGIN-COMMAND/R20261013T232041Z-8f39/worker-handoff.json` | `6CECEBF49FE07444…` | `e75f1fca0799` |
| `F4-T5-PLUGIN-COMMAND/R20261013T232041Z-8f39/relay-record.json` | `318A5358D27A7F5D…` | `c903363b54d8` |
| `F4-T5-PLUGIN-COMMAND/R20261013T232041Z-8f39/k5-transcript-scan.json` | `BA1D1442E362BFE2…` | `b7165b3e07d4` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T232041Z-8f39/transcript-facts.json` | `A2547AFE7F5E67B7…` | `7255c0792cbc` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/prompt.md` | `A261BCECFC4CB5BF…` | `5741d62835be` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/controller-verification.json` | `9BCF6FD3E3D85055…` | `3306fca88e52` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/relay-record.json` | `CB3CA239BCC88494…` | `f104913038bf` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/coherence.txt` | `549CF2A09391A1D4…` | `28672c6afdc6` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/trx-summary.json` | `559BF56C2CF052B5…` | `9d9cf6f115b2` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/scope-mechanical.json` | `EA9243D08F85F96D…` | `5cecf52cc960` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/scope-mechanical-attempt4.json` | `E080A0992EAEB0AC…` | `b73fbd4f8a33` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/analysis.md` | `2CE89899EFAD580E…` | `8e020683ff40` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/coordinator-dispositions-f4t5c2.md` | `F16095099F20005D…` | `bee7c1e954f3` |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/k-f4t5c2-plan-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/k-f4t5c2-work-compare.json` | `DC951BCC96218DC8…` | `be6799734eeb` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T233523Z-b1dd/k-f4t5c2-ver-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |

## 51. F4-T5-PLUGIN-COMMAND — corrección final (intento 6), verificación y controles (2026-10-14T00:24:04Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261013T234354Z-7203` | PLANNING (intento 6, corrección final) | Codex `gpt-6-luna`/`high` (37) | A1-A8 `pass`; seis colecciones exactas; `CorrectionOf` `R20261013T233523Z-b1dd` (`Tests`); `ChainRedSha` `a2a8b868`, 21 `ChainRedFiles` |
| `R20261013T234619Z-b5ef` | WORK (intento 6) | subagente `claude-sonnet-5-5`/`medium` (51 mensajes) | RED `28fe112e` + `e413b9a2`, GREEN `e610ea49`; 13 min; 31 llamadas; ninguna invocación de un intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261014T000444Z-1eac` | VERIFICATION | Codex (38) | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `e610ea49` |
| `R20261014T000903Z-310b` | CONTROL nc1 | Codex `gpt-6-luna`/`high` (39) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Identity`: **oráculo cumplido** |
| `R20261014T001323Z-b951` | CONTROL nc2 | Codex `gpt-6-luna`/`high` (40) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `Scope`: **oráculo cumplido** |
| `R20261014T001948Z-ab02` | CONTROL nc3 | Codex `gpt-6-luna`/`high` (41) | `EXECUTION_BLOCKED/STOP`, `FailureClass` = `StopCondition`: **oráculo NO cumplido** (esperado `EXECUTION_REWORK_REQUIRED/REWORK`, `FreeText`) |

- **CI:** RED `38487208404` (Core `failure`: 25 fallidas) y `28fe112e` (`38487185238`, Core `failure`); GREEN `38487578349` (4/4 `success`; Core
  14384/14384; UI 1 637 de 1 654).
- **Controles del Owner:** K1/K2 sin cambios externos en planificación, trabajo, verificación y nc1-nc3; K3 informado; K4 alcance mecánico `pass`
  (diff neto de 2 archivos); K5 sin intérpretes, RAM, AutoCAD ni accesos fuera del worktree.
- **Entregable de T5:** `src/RackCad.Plugin/RackRamElementsCommands.cs` (comando `RACKRAMPAQUETE`), el `Content` de la tabla en `RackCad.Plugin.csproj`,
  la guarda del comando con las reglas C01-C18 y sus autopruebas, la prueba de la tabla del producto y las cuatro ediciones exactas de pruebas existentes.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/gate-contract.json` | `6A4685883AE86B7A…` | `3ccf8f01b77b` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/prompt.md` | `3B967C310B04FD9B…` | `653e7b5677aa` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/delegation.json` | `CAA0F86FD45DC59D…` | `19f6fa12f22a` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T234354Z-7203/relay-record.json` | `9289AFB631B335C4…` | `a436eb5596f5` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234619Z-b5ef/prompt.md` | `0D5ACE7DDD494866…` | `0b830ba14f50` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234619Z-b5ef/worker-handoff.json` | `09760FD55654971D…` | `de2160b1545c` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234619Z-b5ef/relay-record.json` | `8AF57DE1CBDEA8F9…` | `e66ec8dbabeb` |
| `F4-T5-PLUGIN-COMMAND/R20261013T234619Z-b5ef/k5-transcript-scan.json` | `352B6D68CF46E198…` | `b75421877e22` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261013T234619Z-b5ef/transcript-facts.json` | `C7C03E36D83028CC…` | `afc281fe9ce9` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/prompt.md` | `326022D2D26F08D4…` | `002ca1ccc689` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/controller-verification.json` | `F3B3B874EAF67F6D…` | `59b7b2f99031` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/relay-record.json` | `A93F4CB9C41181D7…` | `02d31365cbbb` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/trx-summary.json` | `134FE7E7C82BA0B0…` | `2bf7bceaae9d` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/scope-mechanical.json` | `09E037A7222C0F7E…` | `f2b787f4e0ef` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/scope-mechanical-red1.json` | `09E037A7222C0F7E…` | `f2b787f4e0ef` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/coordinator-dispositions-f4t5c3.md` | `36CBC96B1F41948F…` | `4bdb240b239f` |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-plan-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-work-compare.json` | `76CCC5A90B4AF9B2…` | `7b023e0082cf` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-ver-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-nc1-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-nc2-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND/R20261014T000444Z-1eac/k-f4t5c3-nc3-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND-nc1/R20261014T000903Z-310b/prompt.md` | `1CD13D7BA6FA4762…` | `866f3ff95a95` |
| `F4-T5-PLUGIN-COMMAND-nc1/R20261014T000903Z-310b/mutation.json` | `F4D7D4F569DDF3B9…` | `24e73c62ff9f` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND-nc1/R20261014T000903Z-310b/controller-verification.json` | `C42E887F5DC7C83A…` | `ca7b7075eeff` |
| `F4-T5-PLUGIN-COMMAND-nc1/R20261014T000903Z-310b/relay-record.json` | `5EE88376ACFF6276…` | `f9e9984f52fc` |
| `F4-T5-PLUGIN-COMMAND-nc2/R20261014T001323Z-b951/prompt.md` | `3779792CDC445EAA…` | `a3cdc98350f0` |
| `F4-T5-PLUGIN-COMMAND-nc2/R20261014T001323Z-b951/mutation.json` | `BD5E6CEBD13E75D2…` | `d04b3f88c30e` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND-nc2/R20261014T001323Z-b951/controller-verification.json` | `2DECABFF09124404…` | `dc0c8957bbf0` |
| `F4-T5-PLUGIN-COMMAND-nc2/R20261014T001323Z-b951/relay-record.json` | `396CD86F42761359…` | `0a823355fe5d` |
| `F4-T5-PLUGIN-COMMAND-nc2/R20261014T001323Z-b951/scope-mechanical.json` | `8D179AD01CE40AAF…` | `e2f23d97152a` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND-nc3/R20261014T001948Z-ab02/prompt.md` | `8C9B39C43B44736F…` | `09a734375e3f` |
| `F4-T5-PLUGIN-COMMAND-nc3/R20261014T001948Z-ab02/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-T5-PLUGIN-COMMAND-nc3/R20261014T001948Z-ab02/controller-verification.json` | `96EC36BBB3E5C8E0…` | `b7e479faaf61` |
| `F4-T5-PLUGIN-COMMAND-nc3/R20261014T001948Z-ab02/relay-record.json` | `5D888716FC38C4E4…` | `5c5c40c6145c` |

## 52. Revisión del Architect de F4-T6 y F4-T5 (2026-10-14T00:34:30Z)

- **Participante:** subagente `claude-opus-5-5`/`high` (Architect, `SAME-SESSION ROLE`), solo lectura; cesión 2026-10-14T00:24:41Z - 2026-10-14T00:33:24Z.
- **Resultado:** CONFORMANT WITH OBSERVATIONS; 0 REQUIRED; 10 OPTIONAL (A65-F4R-01..10); custodia en
  `docs/automation/evidence/I-65-f4/architect-f4-t6-t5-conformance.md` (SHA-256 `F5C7876871E8282F88F69140258FC0EFD2B2ED7D374F0CDDE41AECF120D52D7B`).
- **Controles del Owner:** K1/K2 sin cambios en el checkout principal ni en los demás worktrees; K5 sin intérpretes, RAM, AutoCAD ni accesos fuera
  del worktree (`architect-f4-t6-t5-k5-transcript-scan.json`).
- **Contadores finales de la unidad de cierre:** Codex 41 de 46 (29-41: planificaciones, verificaciones y controles); `attempts` 6 de 6; Workers 4
  (T6; T5 intentos 4, 5 y 6) más 1 preparado y no lanzado; Architect 1. Sin RAM.

## 53. Preparación de la primera validación real de F4 (2026-10-14T00:58:49Z)

- **Orden:** disposición del Owner «F4 Engineering Validation Preparation» (decisiones §47), sobre `191344ff` (CI 38490960626 push 4/4
  `success`).
- **Ejecutor:** Coordinator, sin cesiones (sin Codex, Workers ni subagentes). Lecturas del worktree y de las ramas de I-52 e I-64 con
  `git --no-optional-locks` (referencias locales = `ls-remote`, sin `fetch`).
- **Documentos publicados (SHA-256 al registrar):**

| Archivo | SHA-256 |
|---|---|
| `docs/initiatives/I-65-f4-engineering-validation-sheet.md` | `570FE3269ADD338E9AE0FAC67B283EF826E678122131F7978A00650FF0DE2878` |
| `docs/initiatives/I-65-f4-ref-cant-01-manual-procedure.md` | `6FC226B8FFA97D7E871C9C78C7F8234781BC354A7AA32FD77072F9F17E4FFFF3` |
| `docs/initiatives/I-65-f4-autocad-smoke-procedure.md` | `191521A72B3C6297EDC640E8FD0E14939D67D448838C9609FA4538B31401F7A1` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `25509D7A0D2F1D1B1B627597691AA24EA92187B2F80C465F81A5E45E9E17B6B3` |
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `1810E284A84FCB523B8949C9FAFA81EBB3554845755CE111E55BC55938A27D89` |

- **Coordinación de host:** mensajes `207da1c2-dc67-4198-af8e-9d96848f7b66` (a I-52) y `9795f3c5-6f2c-4eb9-8a32-ef7fa87948ce` (a I-64);
  respuesta de I-64 custodiada literalmente; I-52 sin respuesta al registrar.
- **Corroboración pública de la hoja:** tres búsquedas web (W12X26, HSS4X4X1/4 y A500 Gr C) y dos lecturas: el PDF de Atlas Tube no fue
  legible (flujos comprimidos; la herramienta lo guardó en la carpeta de resultados de la sesión, no se abrió ni se usó) y la página de
  terceros de la HSS respondió 403. Nada de eso es fuente autoritativa.
- **Sin ejecución:** no se abrió AutoCAD ni RAM, no se ejecutó `NETLOAD`, no se compiló ni se cambió código, la tabla de secciones y
  materiales no cambió (SHA-256 `5229231E0D924E48A839EF77BF0B63A2B631214A670783E01D148B9AC6CB4048`).
- **Controles del Owner:** K1/K2 antes del commit sin cambios en el checkout principal ni en los demás worktrees; en el propio, solo los
  seis archivos nuevos de esta preparación.
- **Contadores:** Codex 41 de 46; `attempts` 6 de 6 (sin cambio).

## 54. Respuesta de I-52 a la coordinación de host (2026-10-14T01:01:19Z)

- Mensaje de «I - 52» (sesión `local_7a6025bd-f5c5-4bea-99a4-a1a821849a73`), recibido después de `71baf43b`; texto literal en la custodia
  §4. Acuse de I-65 a I-64: msg `3b93b0f8-3a92-4469-b623-54139e1172db` (custodia §5).
- Documentos actualizados (SHA-256 al registrar):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `95BADE2A28A24E63A0D59E85161ED122B3E2AE8463958BEC50C60E5360588536` |
| `docs/initiatives/I-65-f4-autocad-smoke-procedure.md` | `DEEC3DF8E057F53C49DA0C913A3398D54B4F7302FAFA2C8B8BA3F8AFA962311C` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `B1C476B66BFD29EF8ABCECE957EB8CE17849303EFFAC27387B47E49F42BC479D` |

- Sin ejecución: sin AutoCAD, sin RAM, sin código. Codex 41 de 46; `attempts` 6 de 6.

## 55. Preparación de validación de ingeniería y de host (2026-10-14T01:21:24Z)

- **Orden:** disposición del Owner «F4 Engineering Validation and Host Readiness» (decisiones §49), sobre `27dd4778` (CI 38493431339
  push 4/4 `success`; `71baf43b`: CI 38493208665 push 4/4 `success`).
- **Ejecutor:** Coordinator, sin cesiones (sin Codex, Workers ni subagentes). Lecturas del worktree y de la rama de I-64 (su procedimiento
  T3C, como documento) con `git --no-optional-locks`, sin `fetch`.
- **Lectura pasiva de RAM (DIF-04):** script de solo lectura en la carpeta temporal de la sesión, `python -I`, sin proceso
  `RAMElements.exe` antes ni durante; custodia en `docs/automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.*`. Una primera pasada
  recorrió también la documentación de la instalación y terminó con un error de codificación al imprimir; se repitió dos veces solo sobre
  `Database` (normalización de los nombres y después sin distinguir mayúsculas); solo la salida final se custodia.
- **Búsquedas web:** notas de versión de RAM Elements y tablas AISC (sin mención de la edición) y propiedades del acero (ν, G, peso
  unitario): solo orientan las fuentes candidatas de la hoja; nada es autoritativo.
- **Documentos (SHA-256 al registrar):**

| Archivo | SHA-256 |
|---|---|
| `docs/initiatives/I-65-f4-engineering-validation-sheet.md` | `66DC6E63970DE579A4D350E92FA49249ADCD510A6E0262649E754F16026B90A7` |
| `docs/initiatives/I-65-f4-host-release-and-preflight.md` | `97F1471D017EE81A5E818809FE8AD9841464DE5BEC9FE43FDDC491B0F2AEC6F9` |
| `docs/initiatives/I-65-f4-autocad-smoke-procedure.md` | `C060498916DC80DD9016C65A37B78295B04B68E2342AC3F6BAA1CBEFF9F4001A` |
| `docs/initiatives/I-65-f4-od-f4-14-combinations.md` | `0F23E86B643735E0F6991C47FD3AB9AA8D50FECB418D816043EAD13F5206B2B0` |
| `docs/initiatives/I-65-f4-d-mapping-verified-contract.md` | `C94CCB010E9FABFED8A6A15D7C8010093A3D0AAC67A5CAFE8C237A9A0E23857F` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `6132B51130AFC59ED8F8A8C41F89774249133CF593AE7C01A171E3C91C7BF217` |
| `docs/automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.md` | `720382CAD560A28EA584D1289560D59B9BC516BD767CAC829B723AEDB8F4C6F6` |
| `docs/automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.py.txt` | `5CEB2B2B40E2260B5FDD6C7CC5A79ABE957E47989E8F0D72A359413730155C15` |
| `docs/automation/evidence/I-65-f4/ram-section-tables-vs-aisc-v16.out.txt` | `022C3B5D6B17B2F4EB64E60875CE8376E1936F5ECD92C9E58C1FB9A6091EBD42` |

- **Sin ejecución sobre el host:** sin AutoCAD, sin `NETLOAD`, sin RAM, sin build, sin comandos de registro; la solicitud de host **no se
  envió**. La tabla de secciones y materiales no cambió (SHA-256 `5229231E0D924E48A839EF77BF0B63A2B631214A670783E01D148B9AC6CB4048`).
- **Contadores:** Codex 41 de 46; `attempts` 6 de 6 (sin cambio).

## 56. Coordinación de host, plan M-05c y revisión de propiedades (2026-10-14T01:40:39Z)

- **Orden:** disposición del Owner «F4 Host Coordination and Combination Validation» (decisiones §50), sobre `da5f8cfe` (CI 38495297376
  push 4/4 `success`).
- **Ejecutor:** Coordinator, sin cesiones (sin Codex, Workers ni subagentes).
- **Lecturas del host (solo lectura; `acad.exe` = 0 y `RAMElements.exe` = 0 antes de empezar):** perfil, `TRUSTEDPATHS` (solo número de
  entradas y SHA-256), `SECURELOAD` y `acad.exe` (versión y SHA-256); dos capturas en seco de B-01, B-03..B-08 en una carpeta local de la
  sesión, no versionada (resultado LIMPIO). Ninguna escritura en el registro, el perfil, las ACL ni las carpetas de AutoCAD.
- **Lecturas pasivas de RAM:** `Materials/United States/Steel.mat` (SHA-256 `2A8B423964D2BA3580273DE2D8A29D5E70C9E6DCEEFA1F892A40BB8CD1139CE1`,
  sin cambio desde la pre-lectura); sin abrir RAM.
- **Cálculo independiente:** `section-props-analytic-check.py.txt` con `python -I` en la carpeta temporal (solo dimensiones; no lee el
  catálogo ni RAM).
- **Canal entre sesiones:** solicitudes formales a I-52 (`9f80bc9e-…`) y a I-64 (`01e1f8d4-…`); recibo de I-64 (no es ACK); I-52 sin
  respuesta al registrar.
- **Documentos (SHA-256 al registrar):**

| Archivo | SHA-256 |
|---|---|
| `docs/initiatives/I-65-f4-host-release-and-preflight.md` | `F04EC29BC7ACD0207E94E0976A7F8142BC46F1B4582D507156F14D18337EA033` |
| `docs/initiatives/I-65-f4-m05c-plan.md` | `A75629E84305AD77C5BBA21A67FBE524E9828E1F37F4833193511F018394864C` |
| `docs/initiatives/I-65-f4-properties-technical-review.md` | `CF446269A3C8A4BE9F51CEF3E42079F1BD367DB142B16597BF8A156C9EA6B27B` |
| `docs/initiatives/I-65-f4-engineering-validation-sheet.md` | `9027F616C656B6733B70B7007D499F1C1E7CCB7DDB3E6E80DAE9B072605C12D4` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `683ED86F051824E74ED8918681517F7293036B5B31318D740446636BAAF00E29` |
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `F94747D1DCF1B2A99782881F22717BA3DF9061C2CC63F0A68E8CCABAE28C7BA3` |
| `docs/automation/evidence/I-65-f4/section-props-analytic-check.py.txt` | `2675F7CB0B2EF3D076F6B1961AE6ED951C525E78E23E6583243D9108EF2F7927` |
| `docs/automation/evidence/I-65-f4/section-props-analytic-check.out.txt` | `80E03E48A467F1D8D886E9928809EB77BECE0F493E02EA66BDAA38E657A6C835` |

- **Sin ejecución:** sin AutoCAD, `NETLOAD`, RAM ni build; la tabla de secciones y materiales no cambió. Codex 41 de 46; `attempts` 6 de 6;
  reserva provisional (48; 9) no habilitada.

## 57. Respuesta de I-52 a la solicitud formal (2026-10-14T01:42:31Z)

- Mensaje de «I - 52» recibido después de `54ee711a`; texto literal en la custodia §9. Sin ACK ni RELEASE de I-52 ni de I-64.
- Documentos actualizados (SHA-256 al registrar):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `F7580C34E65555C9C2CA570996415C0897EFEE2BA607DC57DCF346D0650297E2` |
| `docs/initiatives/I-65-f4-host-release-and-preflight.md` | `2517131C24B619C5FA01C130611598D4B1DA4215AD132239CBF8E25DBD660E09` |

- Sin ejecución sobre el host. Codex 41 de 46; `attempts` 6 de 6.

## 58. M-05c en RAM Elements (2026-10-14T02:07:29Z)

- **Orden:** autorización del Owner «RAM M-05c Authorization / Engineering Validation» (decisiones §52), sobre `93606ea0` (CI
  38497191240 push 4/4 `success`; `54ee711a`: CI 38497053907 push 4/4 `success`).
- **Ejecutor:** Coordinator con Computer Use (acceso concedido solo a «RAM Elements 2026»). Sin Codex, Workers ni subagentes.
- **Preparación:** herramienta de F4 compilada con el `dotnet.exe` del usuario (`-c Debug`, salida en `bin/` y `obj/` ignorados);
  `dump-script` en dos carpetas nuevas bajo `D:\r65-m05c\R20261014T015051Z\`. K1/K2 antes (`m05c-before`) y después (`m05c-after`):
  sin cambios. Avisos de cortesía a I-52 e I-64 al empezar y al terminar.
- **Custodia (SHA-256):**

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/m05c/01-ram-modelo-vacio-inicial.jpg` | `CD144B5B0C7638FA467FDE04122FCEC4EFD00473B6EDEB36FA27483CB3064A4F` |
| `docs/automation/evidence/I-65-f4/m05c/02-estados-de-carga-inicial.jpg` | `EBC92F9B2AAEB01CB9B276F4395E8D21F6932DD490461A7FF23A6FCD4384F9CA` |
| `docs/automation/evidence/I-65-f4/m05c/03-mz-casos-a-b.jpg` | `76730000490B0A96F3200A40DF5067772FEE13980509CB51D228D787186DAE89` |
| `docs/automation/evidence/I-65-f4/m05c/04-mz-aviso-descripcion-b.jpg` | `560E1DC5760C7C340FFF5015D53049C737CECFA70BCE083CB0CD07494327293C` |
| `docs/automation/evidence/I-65-f4/m05c/05-mz-dump-ok.jpg` | `F2426EACCD2F40F447C89A7DF491BAC685E8E66795E5FA528C8E0E9056CF8802` |
| `docs/automation/evidence/I-65-f4/m05c/06-mz-guardar-no.jpg` | `CC1A0E569E70FEFF32E066E2A4EDE47DE7DFEA1318A672CD16CEF451A5D64258` |
| `docs/automation/evidence/I-65-f4/m05c/07-mp-combinacion-c1.jpg` | `EAE4EE45A906D46FEE0F6EB5BD4E054178CB6C8192F8ED9CF4BD7BA27234832B` |
| `docs/automation/evidence/I-65-f4/m05c/08-mp-estado-c1.jpg` | `D073D01D74B87CB038C9F9DAA61E19E8D0BF636D2835DDA77DF678E4400B3C6F` |
| `docs/automation/evidence/I-65-f4/m05c/09-mp-dump-ok.jpg` | `19C315A49C657A5DDF871B483F3632C12409BECF94FF099CC1BD7CC2DB61B4E3` |
| `docs/automation/evidence/I-65-f4/m05c/10-secciones-grupos.jpg` | `77E22DA0026A1C74404E59EF5217A10355ADF5DCB2F1FCB64604E16A4A2E8CD8` |
| `docs/automation/evidence/I-65-f4/m05c/11-secciones-grupos-final-lista.jpg` | `D37C2C6B46EE4040ADE4B908E66653D1C6A501ECD84F68D33A186D1226B374F1` |
| `docs/automation/evidence/I-65-f4/m05c/12-hss-sqr-4x4x1_4.jpg` | `6285E447C4DBFF6FC41FBAC607DA87BCF95111B7940122026926D25F7D509A2B` |
| `docs/automation/evidence/I-65-f4/m05c/13-w-12x26.jpg` | `0C22FDB069B5EEFEDE51C03F00243C0805B103F0CE843D0E6DDEFD5437F2A01A` |
| `docs/automation/evidence/I-65-f4/m05c/14-steel-a500.jpg` | `E4FC6672B11CB55DC44BB6E93676CD472A49A783F38122B0FCE2BB6874A738BC` |
| `docs/automation/evidence/I-65-f4/m05c/15-steel-a992-gr50.jpg` | `15DFB6B741F1F562FE1D3AA8B062435CBB9C74CAC5B0BDB0C58DE6B12097932F` |
| `docs/automation/evidence/I-65-f4/m05c/16-cierre-guardar-no.jpg` | `A6CF648538D8571A8F57C34AB421D0F28805164F3B0FBCA50EE38651077E85A2` |
| `docs/automation/evidence/I-65-f4/m05c/m05c-report.md` | `E45CF855586D40E3A99EF9DD573E3FD1632CF554FACEA06C472CD875192FB866` |
| `docs/automation/evidence/I-65-f4/m05c/mp-rackcad-ram-dump.py.txt` | `E53C13452FA2B09E97615B82D99793A401FC1B7C3FBE1D5567A34C179065E048` |
| `docs/automation/evidence/I-65-f4/m05c/mp-ram-dump.json` | `27162B052C1793461054DED13160D4838021D1839EC7DB3D81F0B4272520797C` |
| `docs/automation/evidence/I-65-f4/m05c/mz-rackcad-ram-dump.py.txt` | `F18A3ECF79B345B3E06B6F88B0D57ED4E7DD961F0177797FC3BAF7593A988B69` |
| `docs/automation/evidence/I-65-f4/m05c/mz-ram-dump.json` | `ED7F3CA77D4492624C3CF02478363443BBC767454275634069FD592A0B7DCDEC` |

- **Documentos actualizados:**

| Archivo | SHA-256 |
|---|---|
| `docs/initiatives/I-65-f4-od-f4-14-combinations.md` | `1F95D7B4EEB402F35B8086870D57916A035C43B9135BB107A8EFC95E125F24F3` |
| `docs/initiatives/I-65-f4-ref-cant-01-manual-procedure.md` | `7FCC1B9C4F1F47A76AD0B19A695D81813F17340B11BB0DCF5C0F14075381DD99` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `E5E04F8A036F6C0A5FDAD9CC7895FDE39E79F8482D5770A6DCC6BD4C79EAFDDE` |

- **Contadores:** Codex 41 de 46; `attempts` 6 de 6; reserva 48/9 no habilitada.

## 59. Aceptación de M-05c y siguientes pasos (2026-10-14T02:19:19Z)

- **Orden:** disposición del Owner (decisiones §53), sobre `e88af4ed` (CI 38499458179 push 4/4 `success`).
- **Ejecutor:** Coordinator, sin cesiones; sin RAM, AutoCAD, Codex ni código.
- **Canal entre sesiones:** seguimientos a I-52 (`60ac3038-…`) y a I-64 (`50ea570d-…`); respuesta de I-64 (sin ACK todavía) custodiada
  literalmente; I-52 sin respuesta al registrar.
- **Documentos (SHA-256 al registrar):**

| Archivo | SHA-256 |
|---|---|
| `docs/initiatives/I-65-f4-final-properties-for-approval.md` | `91C5C4FC4B72A144A3F804CBBB8F8109A37A7DA7F29FCF41045EA064B2BE14CC` |
| `docs/initiatives/I-65-f4-real-run-readiness.md` | `F57FC477A01F333BE660A6F0D39445F698D6BEF34A7D2AE220D069FB22D9A4A5` |
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `33462201F7558014C34263C329359E5401C35F983BE5DEF81C14275468E74360` |

- **Contadores:** Codex 41 de 46; `attempts` 6 de 6; reserva 48/9 no habilitada.

## 60. Compilación del DLL del smoke, referencias y fuentes (2026-10-14T02:37:50Z)

- **Orden:** disposición del Owner (decisiones §54), sobre `f9c6bd9c` (CI 38500450515 push 4/4 `success`); texto literal en `745653e1`.
- **Ejecutor:** Coordinator, sin cesiones; sin Codex, Workers ni RAM; AutoCAD cerrado (`acad.exe` = 0) en todo momento.
- **Compilación:** `745653e1` → `RackCad.Plugin.dll` `2e04a4bb…e41d` (detalle en la custodia). Las salidas `bin/` y `obj/` están
  ignoradas por Git; la copia congelada y la carpeta de evidencia del host están fuera del repositorio.
- **Canal entre sesiones:** solicitudes de ventana concreta a I-52 (`db3893c0-…`) y a I-64 (`00b36eca-…`); recibo de I-64 custodiado
  literalmente (§14 de la custodia de coordinación).
- **Búsquedas web** (fuentes secundarias de R-3) listadas en el documento de trazabilidad §5. La Design Guide 9 de AISC no se pudo leer
  (PDF cifrado; la herramienta lo guardó en la carpeta de resultados de la sesión, sin abrirlo).
- **Documentos (SHA-256 al registrar):**

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/smoke-dll-build-745653e1.md` | `9BC602502494CDC3A6A65006C9DD5AAAEA8FF997EC6DD250857A26A80A1C5458` |
| `docs/automation/evidence/I-65-f4/smoke-dll-745653e1-output-manifest.txt` | `14E860D1D4D4B0EA46F1567D2FC128D5CDA8772817227BBF86D3C4D2599DC5B9` |
| `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` | `59415ACA87109D67FAAA2C65A8EAF757F72BE1A4381B89468E07328340B69973` |
| `docs/automation/evidence/I-65-f4/section-props-analytic-check.py.txt` | `24E2D7392A426B72771E4E18A16544674DA14398D9664DD748101F570F8FBA12` |
| `docs/automation/evidence/I-65-f4/section-props-analytic-check.out.txt` | `A31712C8B8A0B79D0C49B627E2DF7F8C2E6014ED8F856AF7B8EB1BC368D280D1` |
| `docs/initiatives/I-65-f4-structural-sources-traceability.md` | `3336A50EFD8C53FDA63D13644BF69E784C3F39D6E50FE7330F1A5E1CB64746E3` |
| `docs/initiatives/I-65-f4-engineering-validation-sheet.md` | `E6961E27BDFBD7F1EC798087A375A1F78AA64E3078BC955BC90E4025F8FF3C85` |
| `docs/initiatives/I-65-f4-final-properties-for-approval.md` | `193EF91E940649558E616459A029FC8344D6E02ED9082FBA00AEF6DE3FC6331A` |

- **Contadores:** Codex 41 de 46; `attempts` 6 de 6; reserva 48/9 no habilitada.

## 61. Respuesta de I-52 a la solicitud concreta (2026-10-14T02:39:59Z)

- Custodia literal en `docs/automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md` §15 (SHA-256 `5160F340E8560CA18A385A8BEFE8BA0AB7256A9366C59E7709D7B354C3FD4C8D`). Sin ACK ni RELEASE.
- Sin ejecución sobre el host. Codex 41 de 46; `attempts` 6 de 6.

## 62. Peticiones de confirmación expresa para la ventana del smoke (2026-10-14T02:48:41Z)

- Disposición del Owner (decisiones §56). Peticiones a I-52 y a I-64 custodiadas literalmente (custodia de coordinación, SHA-256 `49D6E8A5DDB496C673E07CE1E0BC1629E3453C286407CE6BA41B38D491C0F90E`).
- `acad.exe` = 0; sin ejecución sobre el host. Codex 41 de 46; `attempts` 6 de 6.

## 63. Bloqueo del smoke (2026-10-14T02:49:16Z)

- Recibo de I-64 custodiado (§18; custodia SHA-256 `DCED36024618143CEC508C644F4B285EE01B27A5D60FA24B116576BB8E988EFB`); I-52 sin respuesta. Condiciones 1 y 2 no satisfechas; smoke no ejecutado; `acad.exe` = 0.
- Codex 41 de 46; `attempts` 6 de 6.
- Respuesta posterior de I-52 (custodia §19; SHA-256 `91C475C85544D952F102FB3C821C36BC223BA0EAAD237B897F6B174086B97623`): hechos sin conflicto conocido (`acad.exe` = 0, P2 H10 sin autorización ni ventana, sin otras operaciones reservadas), pero **no** es la confirmación expresa de su Coordinator; el bloqueo se mantiene.

## 64. Smoke de `RACKRAMPAQUETE` en AutoCAD (2026-10-14T03:25:13Z)

- Orden: RELEASE del Owner (decisiones §58). Ejecutor: Coordinator con Computer Use (solo «AutoCAD 2025 - English»); sin Codex ni Workers; sin RAM.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/smoke-745653e1/01-dibujo-desechable.jpg` | `836908CDC6CC9B18727DD5148D59DC1DC9736A499C3BF9EB81F15A7A61871D29` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/02-netload-dll-i65-seleccionado.jpg` | `8E772FEF583ED4CFF4FBD3B67E37E45A83D3D924510B88BD038B1D97AB814E65` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/03-aviso-seguridad-load-once.jpg` | `4F93DD61327B75E793EA9869FA6631B22026A4421498C30E082A8039F5EE13FF` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/04-dll-cargado.jpg` | `397D8F3EAE763F880EF38A32A97C242B7B5DED2175F529C5CE7B29D97DCB6571` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/05-columna-base-w12x26.jpg` | `CE0FAA6D29A0AADCC886507FE51F8274A7778A5BFFCBAA1FD74D92A70FCD51D8` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/06-brazo-hss4x4x1_4.jpg` | `EADDF188DF67D8732DB48ACCFE16817D14E3544E1C22EFF6844DEC45BC5D8BBA` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/07-linea-ref-cant-01-resuelta.jpg` | `DA08B0844CE475443912D6971AF5A08F394B352637CA075ED247455D8A0CEFB6` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/08-vista-frontal-insertada.jpg` | `25EC85A24A659FC13616D4E889A8222638D61140FD2566BB1224DF74FB128B66` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/09-rackrampaquete-pide-rack.jpg` | `B6B388B4D5320ED2C5918E12F4E6E62B1182BD407BEAFB8B0DE576F25EB195FB` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/10-incidente-pasteclip.jpg` | `1E0EC0EC9E6B217B545EC8C8497BEF73C74AB1E05FA9E1CA9A839DE1D8E26978` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/11-pasteclip-cancelado.jpg` | `2052DD476B5BEB882EE64A70AA173CCB83B2FB8E145A11876B9F9A6B1B7D7126` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/12-ruta-cargas.jpg` | `5C7311CE5CB9D44EDE60402E6B0D235D61D03ED2D5D76BA5411C6ECD2C6D4622` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/13-resultado-unsupported.jpg` | `AA5EA6DC98538C2D01A8544A701F87F5B05F3D6A30258F6B985965C1F74FD009` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/14-resultado-unsupported-detalle.jpg` | `54B99A5022E4A3948068CA1205981A38285B1E7FC386D504624F649F19C9200B` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/15-cierre-sin-guardar.jpg` | `B267F0F7B788FF014A3D4DB38B2B5704FC1EA458D84A51BFFF281AFCF92C9D3F` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/baseline-summary.txt` | `240D088015E0482F4B82E030411A36EC0C3DE812546A1D5CFAB24AA54CF4709D` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/diff-registry-keys.txt` | `0892338C09123EF046DD481DC75954F359A71A855DE2FC9C04EBDE174EDA294B` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/post-summary.txt` | `24DA51D1B559122CD14D0C607FAB81D81FD7E13F35C0308A02E3D54614531B6E` |
| `docs/automation/evidence/I-65-f4/smoke-745653e1/smoke-report.md` | `2CEF4C4ADDE6DE9E7BB4BD9E2411EFC62297E8CBFA644F9D7552DD19A7B429F5` |

- Exportaciones del registro, capturas completas y resúmenes en `D:\r65-smoke\745653e1\run-R20261014T0312Z\` (no versionado: datos personales).
- Codex 41 de 46; `attempts` 6 de 6.

## 65. Recomprobación de I-52 tras el smoke (2026-10-14T03:27:51Z)

- Respuestas de I-52 custodiadas literalmente (§23-§24; custodia SHA-256 `6474E8FE355C68B52ADC71F5C2E4FF14DF7D086302A4F926FFCC9CFA15979F0C`). Sin deriva; INFORMAR sin efecto en su tupla.

## 66. Paquete de aprobación de ingeniería R-3/R-4/R-5 (2026-10-14T04:00:29Z)

- Orden: disposición «AutoCAD Smoke Acceptance / Final Engineering Mapping Review» (decisiones §60). Ejecutor: Coordinator; sin Codex, Workers, AutoCAD ni RAM; sin descargas.
- Fuente primaria leída en solo lectura: `tools/sources/aisc-shapes-database-v16.0.xlsx` del checkout principal (ignorado por Git), SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496` (igual al del manifiesto del catálogo). Checkout principal limpio antes y después.
- Fichas oficiales de ASTM leídas (solo metadatos bibliográficos): A992/A992M-22 activa (2022-09-21); A500/A500M-23 activa (2023-11-21). Páginas HTML de AISC: 403 a la herramienta de lectura; AISC 360-22 no descargada.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-smoke-acceptance-final-mapping-review.original.txt` | `6E91166B16D28B25E19FD27A0BC7C85FC64F0B36A2EDDC1B9B94CCA9C186AE81` |
| `docs/initiatives/I-65-f4-engineering-approval-package.md` | `B55359523620BC6FD2CFF8BCDF0E118648F2739E01243938B3FC79D748FAB883` |
| `docs/automation/evidence/I-65-f4/aisc-v16-convention-check.py.txt` | `672E36ED9EB779B80EF2D26F05D5C9BC39D7A578F5920D76E8E1132C8B567ACB` |
| `docs/automation/evidence/I-65-f4/aisc-v16-convention-check.out.txt` | `1879FE7457EA1FAE1E5E6C8CA156A881B6C180A41E543B5706775975FA1E9DE4` |

- Tabla `assets/structural-analysis/ram-elements/section-material-map.json` sin cambios: `5229231E0D924E48A839EF77BF0B63A2B631214A670783E01D148B9AC6CB4048` (dos filas `proposed`).
- Codex 41 de 46; `attempts` 6 de 6.

## 67. Comprobación mecánica de D-08 y filas finales (2026-10-14T04:17:07Z)

- Orden: disposición «Engineering Approval Package — D-01 to D-08» (decisiones §61). Ejecutor: Coordinator; sin Codex, Workers, AutoCAD ni RAM.
- Lector del producto ejecutado en solo lectura desde `D:\r65-smoke\745653e1\bin-Debug-net8.0-windows\` (`RackCad.Application.dll` `F9A97451…7F0F` y `RackCad.Domain.dll` `04643D3E…3A2C`, iguales al manifiesto del smoke); `git diff 745653e1 HEAD -- src tests assets tools` vacío.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-engineering-approval-d01-d08.original.txt` | `C2F51A3912A23A6A3BCE836DCC0B6A108DAFADF4D10ABA615AD71D673853981A` |
| `docs/initiatives/I-65-f4-d09-signoff-f4d-request.md` | `00AAF7580865E98390F40E2F6E8D50C5EA20965B0EBB335572CDFEC66B26AA29` |
| `docs/automation/evidence/I-65-f4/d08-variant-b-check.py.txt` | `77B0DA353F380E559F75F9D94E61E5A37F84150B15624406E88F7A30A44A1180` |
| `docs/automation/evidence/I-65-f4/d08-variant-b-check.out.txt` | `9F575BB70AE7B17B07832DE329DE7F3EB4F8624E6C98246F8F9AB5A541D61587` |
| `docs/automation/evidence/I-65-f4/d08-reader-check.ps1.txt` | `A6DBD6FCF454F6A73D81792516971522183C9634859B7D21055B52C80CC67884` |
| `docs/automation/evidence/I-65-f4/d08-reader-check.out.txt` | `5C47F0EFE07B2433C578319C7A1C36680C5F9AC336BC6CEBBCC57429F15CA007` |
| `docs/automation/evidence/I-65-f4/d08-rows-final.json` | `B4CEC50140C434001E09DD8A5812C83CCB2A86759F1B576CB5AEA275311BF993` |

- Tabla del producto sin cambios: `5229231E0D924E48A839EF77BF0B63A2B631214A670783E01D148B9AC6CB4048`. Checkout principal limpio.
- Codex 41 de 46; `attempts` 6 de 6.

## 68. Autorización de F4-D: registro, preflight y tabla objetivo (2026-10-16T14:41:15Z)

- Orden: «D-09 Engineering Sign-Off / D-10 F4-D Implementation» (decisiones §62). Autoverificación de la sesión principal: `get_session("self")` = `claude-opus-5-5`/`xhigh` (MATCH).
- Preflight: `HEAD` = `ls-remote` = `9f6fff6e`; `origin/main` = `bb0d5522` = `merge-base`; 0 commits de `main` por delante; Freeze y ADR-0050 sin commits desde `a9d7080b`; `git diff 745653e1 HEAD -- src tests assets tools` vacío.
- Selección del filtro del contrato en `9f6fff6e` (`dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~RackCad.Tests.StructuralAnalysis"`, TRX en el scratchpad): 1789 superadas, 0 fallidas.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-authorization-d09-d10-f4d.original.txt` | `130D0ACCBF3FBE86B8274CAFE4858F7852007B4AEC2BCF1133B4B6D16189A50E` |
| `docs/automation/evidence/I-65-f4/f4d-table-target.json` | `FC3D91464AC48A15730661EFCD50ECB770114559C68BDEFAE67E0816972DB619` |
| `docs/automation/evidence/I-65-f4/f4d-make-target.py.txt` | `151146599BC272CD828CB3D2038878E9142BEE711EE41B5164411E123B438061` |
| `docs/automation/evidence/I-65-f4/f4d-reader-check-target.ps1.txt` | `FC9901DD2BF3CA6B13BCC0616B2514D4470ECB0442BFE2999039781CB6447012` |
| `docs/automation/evidence/I-65-f4/f4d-reader-check-target.out.txt` | `1400F80A75B1D63D370EB264AE1D7DE4E0FE9A4BB9B047A3FC4721DF3EE7432A` |

- Codex 41 de 48; `attempts` 6 de 9.

## 69. F4-D-SECTION-MAP-VERIFIED — intento 6: planificación, trabajo y verificación con REWORK (2026-10-16T15:30:26Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261016T144346Z-4eaf` | PLANNING (intento 6) | Codex `gpt-6-luna`/`high` (42) | A1-A8 `pass`; seis colecciones exactas; `claude-sonnet-5-5`/`medium`, Balanced |
| `R20261016T145133Z-0214` | CONTROL nc4 | sin invocación | A3 `fail`, resto `pass` |
| `R20261016T145459Z-8301` | WORK (intento 6) | subagente `claude-sonnet-5-5`/`medium` (61 mensajes) | RED `fe9b6720`, GREEN `d548fb0b`; 23 min; 37 llamadas; 0 errores; 0 denegaciones; ninguna invocación de un intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261016T152242Z-2e6f` | VERIFICATION | Codex (43) | **`EXECUTION_REWORK_REQUIRED`/`REWORK`**, `FailureClass` `Ci`; RED acreditado (`RedPart` `pass` en `Ci` y `Tests`) |

- **CI:** RED `38921140743` (Core `failure`: 13 fallidas de 14390, todas en el filtro, por aserción); GREEN `38922575054` (Core `failure`:
  3 fallidas de 14390, carpeta `/tmp/…` rechazada por la fábrica; UI 1 637 de 1 654 `success`; Build UI `success`; Build Plugin `skipped`).
- **Controles del Owner:** K1/K2 sin cambios externos en planificación, trabajo y verificación; K3 primera orden `pwd`; K5 sin intérpretes, RAM,
  AutoCAD ni accesos fuera del worktree; alcance mecánico `pass` (6 archivos).
- **Procesos:** `git.exe`/`bash.exe` ilegibles en las salidas y entradas, creados por las apps del host fuera de las cesiones y ya terminados al
  releerlos a los 2 s (falso P-02 conocido).
- **Contadores:** Codex 43 de 48; `attempts` 7 (máximo 9).

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/gate-contract.json` | `1D30597A8C1997E8…` | `6d3f5a5f3ea8` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/prompt.md` | `7455C0EAD837416D…` | `c0e71ebc9a10` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/delegation.json` | `641A72C312290138…` | `c705fccca877` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T144346Z-4eaf/relay-record.json` | `B8397E244555A238…` | `8bef8e68dfb8` |
| `F4-D-SECTION-MAP-VERIFIED-nc4/R20261016T145133Z-0214/delegation.json` | `94781BC88FCBED6B…` | `f82f920ebf3b` |
| `F4-D-SECTION-MAP-VERIFIED-nc4/R20261016T145133Z-0214/acceptance.json` | `3E2CF552A67987E0…` | `2df92c858dd3` |
| `F4-D-SECTION-MAP-VERIFIED-nc4/R20261016T145133Z-0214/relay-record.json` | `EB242048C60EE0B5…` | `5642381b0f07` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T145459Z-8301/prompt.md` | `BAAA069B178F2A62…` | `d32cd7f5877e` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T145459Z-8301/worker-handoff.json` | `828A0C08F9E807C9…` | `91cd5c54fcac` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T145459Z-8301/relay-record.json` | `531AD7F91C48BDA4…` | `4a03a645f0ad` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T145459Z-8301/k5-transcript-scan.json` | `BD9F3D243AB2CD99…` | `eb61e9863246` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T145459Z-8301/transcript-facts.json` | `D1AD8EACBC96E9AA…` | `c1ac66e3dd09` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/prompt.md` | `E13BA678618EEA84…` | `2b0f95ac26bd` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/controller-verification.json` | `9EE078E1ABE599EF…` | `795c84be6514` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/relay-record.json` | `62EDEDA053B00428…` | `50238b6b7507` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/coherence.txt` | `1CFE9B0EB90C8340…` | `59f654fdcafb` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/trx-summary.json` | `A53FCB11EE6AB6F6…` | `5927adbdbe37` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/scope-mechanical.json` | `EAB463F0847F6452…` | `20777534e692` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/analysis.md` | `15C626B405FB279D…` | `bcf2a6ff8e1f` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/coordinator-dispositions-f4d.md` | `46BB24593504EB0A…` | `ce730620dfe2` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/k-f4d-plan-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/k-f4d-work-compare.json` | `E7BD970DF664ACB8…` | `9edd8c809a65` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/k-f4d-ver-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |

## 70. F4-D — planificación de la corrección rechazada (2026-10-16T15:37:20Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261016T153224Z-5bb3` | PLANNING (intento 7) | Codex `gpt-6-luna`/`high` (44) | **rechazada**: A4 `fail` (`" .github/"`), A5 `fail` (`…f4-d.original.txt`); A2, A3, A6-A8 `pass`; STOP P-03; sin Worker |

- Controles K1/K2 sin cambios; `config.toml` sin cambios; procesos ilegibles de las apps del host terminados al releerlos.
- Codex 44 de 48; `attempts` 7 (máximo 9).

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/gate-contract.json` | `1D30597A8C1997E8…` | `6d3f5a5f3ea8` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/prompt.md` | `5AEE82A7BBADD679…` | `9ce1a98345b9` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/delegation-rejected.json` | `638AD9AD28CE3D4A…` | `522ead82ed71` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/acceptance.json` | `2642FE560F378427…` | `2c75b4923a83` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/exactness.json` | `36A30E4DF57987C5…` | `710c92d2e431` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/relay-record.json` | `F67C3B62F88DD0DE…` | `a5054dbe37cc` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/analysis-p03.md` | `912BAD530BEAC363…` | `70f53517b4ac` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153224Z-5bb3/k-f4d-cplan-compare.json` | `D5EE2EED23590569…` | `f7bf6c77e753` |

## 71. F4-D-SECTION-MAP-VERIFIED — corrección, verificación, controles, Architect y cierre (2026-10-16T16:43:54Z)

| RunId | Fase | Participante | Resultado |
|---|---|---|---|
| `R20261016T153804Z-5fdd` | PLANNING (intento 7, reemisión) | Codex `gpt-6-luna`/`high` (45) | A1-A8 `pass`; seis colecciones exactas; `claude-sonnet-5-5`/`medium`, DEBUGGING |
| `R20261016T154337Z-8aed` | WORK (intento 7) | subagente `claude-sonnet-5-5`/`medium` (34 mensajes) | RED `2188a1d7`, GREEN `51fc82f1`; 17 min; 22 llamadas; 0 errores; 0 denegaciones; ningún intérprete; `IMPLEMENTATION_COMPLETE` |
| `R20261016T160510Z-9e5c` | VERIFICATION | Codex (46) | `EXECUTION_BLOCKED`/STOP (P-02, `Termination`); 13/14 `pass`; resuelto por el Coordinator (falso positivo) |
| `R20261016T161137Z-e795` | VERIFICATION (reverificación) | Codex (47) | **`EXECUTION_VERIFIED`/`NONE`**, 14/14 `pass`, `VerifiedSha` `51fc82f1` |
| `R20261016T161516Z-fbf3` | CONTROL nc1 | Codex `gpt-6-luna`/`high` (48) | `EXECUTION_BLOCKED/STOP`, `FailureClass` `Identity`: **oráculo cumplido** |
| `R20261016T163345Z-e1cd` | CONTROL nc2 | Codex `gpt-6-luna`/`high` (49) | `EXECUTION_BLOCKED/STOP`, `FailureClass` `Contract`: oráculo no cumplido en sentido estricto (DEV-I65-F4-24) |
| `R20261016T163750Z-f190` | CONTROL nc3 | Codex `gpt-6-luna`/`high` (50) | `EXECUTION_BLOCKED/STOP`, `FailureClass` `FreeText`: oráculo no cumplido en sentido estricto (DEV-I65-F4-25) |
| — | Architect | subagente `claude-opus-5-5`/`high` (103 mensajes) | CONFORMANT WITH OBSERVATIONS; 0 REQUIRED; 11 OPTIONAL; cesión 2026-10-16T16:19:36Z - 2026-10-16T16:27:58Z |

- **CI:** RED `38927278590` (Core `failure`: 13 de 14390, por aserción); GREEN `38928508008` (4/4 `success`; Core 14390/14390; filtro
  `FullyQualifiedName~RackCad.Tests.StructuralAnalysis` 1795/1795; UI 1 637 de 1 654). Las CI de los commits de sesión `2a1ed99a` y `c487262f` terminaron en `failure` porque contenían las tres pruebas del intento 6.
- **Tabla:** blob `d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2` en `51fc82f1` = `f4d-table-target.json` en `e886b557`; diff frente a `d08-rows-final.json`: solo las 4 líneas de `verifiedBy`/`verifiedOn`.
- **Controles del Owner:** K1/K2 antes y después de cada cesión (planificación, trabajo, dos verificaciones, nc1-nc3 y Architect) sin cambios en el checkout principal (2 151 rastreados, mismo resumen SHA-256) ni en los demás worktrees; K3 primera orden `pwd`; K4 alcance mecánico `pass`; K5 sin intérpretes, RAM, AutoCAD ni accesos fuera del worktree en el Worker y en el Architect.
- **Procesos:** `git.exe` ilegibles en salidas y entradas, todos terminados al releerlos a los 2 s; su padre era un `python3.13.exe` descendiente de la sesión desde las 14:28Z (ya terminado), no un participante.
- **Contadores:** Codex 50 de 52; `attempts` 7 de 9.

Custodia:

| Archivo (bajo `I-65-pilot/`) | SHA-256 transitorio | Blob |
|---|---|---|
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/gate-contract.json` | `1D30597A8C1997E8…` | `6d3f5a5f3ea8` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/prompt.md` | `74FDA922E2BA1489…` | `411645fbc9fc` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/delegation.json` | `39DA9FDB2D4C8284…` | `5458ce52ff97` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/acceptance.json` | `F63ED8E87F2A0609…` | `73d1783fc942` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/exactness.json` | `D0CBC2F19D9B1FBA…` | `de0f1687cb6b` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T153804Z-5fdd/relay-record.json` | `F9464614B572E0B4…` | `f025c14e10a6` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/prompt.md` | `2A2643F56221DD29…` | `bab2a05a3ff9` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/worker-handoff.json` | `139B23C35234BE16…` | `cba3fc27501b` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/relay-record.json` | `C8C5BCFE58CE7FB1…` | `a810e69f45f1` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/exit-reread.txt` | `E8BCF26FC237E178…` | `c3ec10768635` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/k5-transcript-scan.json` | `D9D167B80A071688…` | `6bdd4719021e` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T154337Z-8aed/transcript-facts.json` | `AF3517E5FA951565…` | `c1b0a2a6fb31` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/prompt.md` | `CA575CF0E6C0D893…` | `d7cc46ce5400` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/controller-verification.json` | `E6582534FA9D0FFC…` | `c8c5a3c373d2` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/relay-record.json` | `E23D5634D0E8D665…` | `8980c4a7771d` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/coherence.txt` | `549CF2A09391A1D4…` | `28672c6afdc6` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/trx-summary.json` | `A2B68DC6B9A3BA51…` | `a4f1c0adedce` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/scope-mechanical.json` | `09E037A7222C0F7E…` | `f2b787f4e0ef` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/scope-mechanical-red.json` | `EA9243D08F85F96D…` | `5cecf52cc960` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T160510Z-9e5c/coordinator-dispositions-f4d-c.md` | `6407C6ABC96C91E3…` | `87cd3d1773e6` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/prompt.md` | `8E42CA65577E7878…` | `683b2ee1de93` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/controller-verification.json` | `C3732CA284E0873A…` | `f7f515ed3b1f` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/relay-record.json` | `C2461374DD92EC37…` | `3e7c84bd2f57` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/coherence.txt` | `B6BEA444F6DA957C…` | `2969a6fa566f` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/trx-summary.json` | `A2B68DC6B9A3BA51…` | `a4f1c0adedce` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/scope-mechanical.json` | `09E037A7222C0F7E…` | `f2b787f4e0ef` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/scope-mechanical-red.json` | `EA9243D08F85F96D…` | `5cecf52cc960` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/coordinator-dispositions-f4d-c2.md` | `EDB4298251F72F6F…` | `88ff4e7a03d7` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/analysis-p02.md` | `C1F0B8AB5621ED65…` | `cc38c2b1e267` |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/exit-reread-work.txt` | `E8BCF26FC237E178…` | `c3ec10768635` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-cplan2-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-cwork-compare.json` | `407468C0FEF5600D…` | `8dea6a51096e` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-cver-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-cver2-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-nc1-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-nc2-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-nc3-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED/R20261016T161137Z-e795/k-f4d-arch-compare.json` | `C088C85BFA563FB0…` | `f7bf6c77e753` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED-nc1/R20261016T161516Z-fbf3/prompt.md` | `1AB1DA569CF63BC2…` | `30bf7b20bdcb` |
| `F4-D-SECTION-MAP-VERIFIED-nc1/R20261016T161516Z-fbf3/mutation.json` | `BAD195C5FC768A53…` | `a81c73469bf1` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED-nc1/R20261016T161516Z-fbf3/controller-verification.json` | `E45ED362CEC34E29…` | `aefa8981454e` |
| `F4-D-SECTION-MAP-VERIFIED-nc1/R20261016T161516Z-fbf3/relay-record.json` | `97D64165DBE214A8…` | `96f253c3bae5` |
| `F4-D-SECTION-MAP-VERIFIED-nc2/R20261016T163345Z-e1cd/prompt.md` | `347A9F307935C130…` | `bf9d6a5c6f8f` |
| `F4-D-SECTION-MAP-VERIFIED-nc2/R20261016T163345Z-e1cd/mutation.json` | `097F965B71EEF33E…` | `e0b2c57a97e1` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED-nc2/R20261016T163345Z-e1cd/controller-verification.json` | `FC0DFA2497E70525…` | `30c72fb49a8a` |
| `F4-D-SECTION-MAP-VERIFIED-nc2/R20261016T163345Z-e1cd/relay-record.json` | `3ED827487B0CDD94…` | `101cf8824657` |
| `F4-D-SECTION-MAP-VERIFIED-nc2/R20261016T163345Z-e1cd/scope-mechanical.json` | `4EE52354A8CBECDF…` | `e3db9383041e` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED-nc3/R20261016T163750Z-f190/prompt.md` | `5DCFC8B84995CE83…` | `89727cc35362` |
| `F4-D-SECTION-MAP-VERIFIED-nc3/R20261016T163750Z-f190/mutation.json` | `DA1F3B8B5E00512B…` | `511067defe3d` (fin de línea normalizado por Git) |
| `F4-D-SECTION-MAP-VERIFIED-nc3/R20261016T163750Z-f190/controller-verification.json` | `479FE29E0100A4C0…` | `586a9787d6e9` |
| `F4-D-SECTION-MAP-VERIFIED-nc3/R20261016T163750Z-f190/relay-record.json` | `6D520B4E2C61CAF4…` | `902e4363a8b3` |

## 72. Preparación del segundo smoke: compilación, procedimiento y coordinación de host (2026-10-16T19:03:46Z)

- Orden: «F4-D Acceptance / Second AutoCAD Smoke Preparation» (decisiones §67). Ejecutor: Coordinator; sin AutoCAD, RAM, Codex ni Workers.
- Compilación 2026-10-16T18:54:16Z-18:54:49Z desde `51fc82f1` hacia `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows` (no versionado); DLL `fa334b14…aedb`; `acad.exe` = 0 y `RAMElements.exe` = 0 antes y después; DLL del primer smoke intacto.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-f4d-acceptance-second-smoke-prep.original.txt` | `C644936498ADD005C44D3F92F003CD4A072E505D253C913A0147249B7517850E` |
| `docs/initiatives/I-65-f4-second-smoke-procedure.md` | `F59DBBE8E1079247646DAF2E4891056C8D5BE612F365D28B0D55529B9E053952` |
| `docs/automation/evidence/I-65-f4/smoke2-dll-build-51fc82f1.md` | `703FCCF2A27662D47C36C5A31C53199C33474F8F8D3E2A136DCFD639AFA430C4` |
| `docs/automation/evidence/I-65-f4/smoke2-dll-51fc82f1-output-manifest.txt` | `5C9A6D62E84D23317D19A6D67A4883E8F7EDB9DAC33A94511761220DBDF265D2` |
| `docs/automation/evidence/I-65-f4/smoke2-expected-inputs.json` | `8B264A05171616070893ECCE7808E36E31048322545AC99185D42572A3682148` |
| `docs/automation/evidence/I-65-f4/smoke2-expected-inputs.ps1.txt` | `0C84A4D0C4145B82505BB5C759CF03489C598DA79794BFABA164BBFC0333F379` |
| `docs/automation/evidence/I-65-f4/validate-smoke2-package.py.txt` | `8B19A9C725C66A629B03B6DDEA7BBA70149788508E38E92E673C9A6852812C17` |
| `docs/automation/evidence/I-65-f4/selftest-smoke2-validator.py.txt` | `93E38ED3BE645297F653737BB0FC1560FB071ABD19FD52F8823282F63CB49B1B` |
| `docs/automation/evidence/I-65-f4/selftest-smoke2-validator.out.txt` | `EF76EB76F7DA981AE314340786923CDDC29FF3F3D9F6FA3D633DBA736F78EDA0` |
| `docs/automation/evidence/I-65-f4/host-coordination-smoke2-2026-10-16.md` | `42DA307C052AE83F97689BA8179BC3391C4A6AC53CC0F81C3A46566DA768E56B` |

- Codex 50 de 52; `attempts` 7 de 9.
- Respuesta de I-52 custodiada literalmente (custodia §6; SHA-256 del archivo `3C2ED03B1B5942D65BA1D970ECD34795050523025ED05A0009C5FA65B2D25EF4`): acuse de recibo con datos; sin RELEASE.

## 73. OWNER HOST RELEASE del segundo smoke y comunicación previa (2026-10-16T19:17:28Z)

- Orden: «OWNER HOST RELEASE / Second AutoCAD Smoke — Verified Mapping Package» (decisiones §68); original SHA-256 `32C25B13F00FC685B77C4C5B83C73D96F21B4A8263EF6D4D597ED50F4F7EDF2E`.
- `acad.exe` = 0 y `RAMElements.exe` = 0 a las 2026-10-16T19:14:43Z; DLL re-leído `fa334b14…aedb`.
- Custodia de la coordinación (§7-§8): `docs/automation/evidence/I-65-f4/host-coordination-smoke2-2026-10-16.md` SHA-256 `0EDEBF85C83420448702CB0131FA342F9BDDF1868E31B3A5D98525348B723B57`.
- Codex 50 de 52 (sin uso); `attempts` 7 de 9.

## 74. Segundo smoke ejecutado (2026-10-16T19:32:25Z)

- Orden: OWNER HOST RELEASE (decisiones §68-§69). Ejecutor: Coordinator con Computer Use (solo «AutoCAD 2025 - English»); sin RAM, Codex ni Workers.
- Corrida local (no versionada): `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\`; exportaciones `.reg` y paquete completo allí.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/host-coordination-smoke2-2026-10-16.md` | `978EF46C87BDFC20E360DED02C954BC34C07123A4857927825B8691B92EE751E` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/01-dibujo-desechable.jpg` | `0308974FC3A6A227D7B2940947BD4B1E3191DF49581CAC1C5F8B379909366369` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/02-incidente-carpeta-test-tmp.jpg` | `867D7CF462F9D6B042D59822A6FDFD01649B76CDCF035548F4065204709C539D` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/03-netload-dll-autorizado-seleccionado.jpg` | `50DE8088B16377C5C922A3AC1491198987C86CAF823C0ED3D7023CDEBDBC1536` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/04-aviso-seguridad-load-once.jpg` | `EA94E57350017DF9B23B611A096348B4C765C0F560EFF9AC3DCC0533C2C6A074` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/05-dll-cargado.jpg` | `957C277C6D5F42A085933C26E06D732442CE091DC43B0A5E9F5FE8D5DFC46253` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/06-columna-base-w12x26.jpg` | `D18DDA4C44A84DD96ABED001901F0C5E87E1B455775C658395B3E93DD409A3B5` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/07-brazo-hss4x4x1_4.jpg` | `5BBB86C2E2DAE2AA084661DA6557763105CC92CFAA734A05A519FEF4F90DE503` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/08-linea-ref-cant-01-resuelta.jpg` | `C2EB82B1410547833E2888778265B414CA363871C27171B69336AB75A557CFF5` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/09-vista-frontal-insertada.jpg` | `D65048CE1E090CBB093F52226EFD6653221EC5D0D4D8B5DBB2B45B884619F24F` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/10-ruta-cargas.jpg` | `A8CBD596D22C532034C54C4C3DF9B79D33020FAC1B3D78441861ACAE20F34C95` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/11-carpeta-tecleada-barras-invertidas.jpg` | `8BA622C43F39EAF7632558AF08A9B2CA916B8DF1B21E1BA648C9B2A1F886E00C` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/12-resultado-valid.jpg` | `FE2F6C21CFA26DD1A700A38968C882B0605F5734249C2685E764B0E3F9F98593` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/13-consola-completa.jpg` | `F6AC30C68D8CF5BACDDA834EC806691B8D3BA1BD18D19A9132D483E3BBA9B9D9` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/14-cierre-sin-guardar.jpg` | `32125F4C1A55A69253D1A64D89D1B5A3A69E8B7A316AA70F9104D1414772D406` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/baseline-summary.txt` | `FCBAF7447B69A321A1D1684F6D8521B51E2BD2DC28319392427DFC3CD7612DDD` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/diag-v11-smoke2.py.txt` | `88AC867FA8FE01FCCC96A8D672DB31C67CA790B8A6396D5813EB22D3F68A1076` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/diff-registry-keys.txt` | `D9846AAAE35E712ECBF68155BEFF6CC081CA9AB80BBF8890C955C099F840AD55` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/manifest.json` | `19FBA962297BADC95DD008BCF8D1477993C55A7FF8B5347D68E311E246F53A78` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/post-summary.txt` | `5AE14DA6FD4F2EE661D2B8CFFABF11F5DEAFFAC4F9164344B1B705DFCE21A2EE` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/smoke-report.md` | `3005492600F74CF406644DF4253FAF0AA0CD2DFFE6FD3D392F18725A8C9FD990` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/smoke2-hostsnap.ps1.txt` | `C04B72B74E79DE020C608FD73B646883EB31D07187A0BF32292F936CED7DCF12` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/smoke2-regdiff.ps1.txt` | `353ECC74D023C4E35D2FE9089014B62185B956938D8AFEB661EBF89872891AA5` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-diagnostic.json` | `C22F22222D15F5795172CDFEAF364A77EDCC7DAD2C17BFDFDE873D84DCF4E1D2` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/validator-report.json` | `0D415A3F79F90B8B44114C478848FECA954C441510055717827B7EEB52D269C0` |

- Script y cargas del paquete por SHA-256: `rackcad-ram-build.py` `D9B41AAB69643C01971E756FB9323A8F35DAD6B33008598655C753FE2F92D719`; `structural-loads.json` `25F48E7F75D879C4668C3D9B37F69FDA0CD5442A407C03FF76362754623CAE05`.
- Codex 50 de 52; `attempts` 7 de 9.
- Recomprobación de I-52 custodiada (custodia §13, decisiones CD-I65-F4-140; commit `46e5e789`): SHA-256 nuevos `docs/automation/evidence/I-65-f4/host-coordination-smoke2-2026-10-16.md` `188982088F8FAB2087F3389A5A52FAC1AAABCC1C6C8196A02DC64CA33FEF9CEF` y `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/smoke-report.md` `2F10E11E04051B96289EBB4207A66181BF905EC638B7C8CDD1A689B64E006DBA`.

## 75. Corrección de V-11 y revalidación de solo lectura (2026-10-16T20:42:21Z)

- Orden: «Second Smoke V-11 Correction and Read-Only Revalidation» (decisiones §70). Ejecutor: Coordinator; `acad.exe` = 0 y `RAMElements.exe` = 0; sin Codex ni Workers.
- Corrida local: `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\v11-correction\` (copias de prueba en `selftest\`, informes por caso en `selftest\reports\`).
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-v11-correction-revalidation.original.txt` | `C615D602402B5F9B7BBB2BADC9F3AD9B083D2E7C300DCB79989B26A0AB4C6316` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/pkg-hashes-after.txt` | `7C8D7C24BE5F26F4EE2BE71C7CF035835C990D83A6F5A2B74ECE5332AE606A1A` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/pkg-hashes-before.txt` | `454D23DA774AE874CC2D29A9070BCBCC7625FBEDA6FCEC0F19D5104B6B3C527C` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/revalidation-report.md` | `547ED93E0E9E875F1763996A99AA37F95A9C5A49E8B8A068308ECE9AD6D475D5` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/test-v11-validator.json` | `BB8DF79AE803F44DA622B033DB50419E618F18BC559DC4D21D0BD5EE9AA1FD80` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/test-v11-validator.out.txt` | `AB2668C9F4F01F1F332768D581A4BB47D87C7D903DF7EE04245106FFC2D97463` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/test-v11-validator.py.txt` | `AA09C4C48AF7687B3346325B5068069BAAA3790CFD49C548B3D8F128BFB80AC0` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/validate-smoke2-package-v1-to-v2.diff.txt` | `81F5321FDB29ECBECA5C9EC4B4352A1925DF53B1CB9AF1740DD1A455E324EBE8` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/validate-smoke2-package-v2.py.txt` | `1F56B8CBA1A64F908C46EB5691930891C3DC0EF1D9BF8ED9C45F8E5F8BDD297A` |
| `docs/automation/evidence/I-65-f4/smoke2-51fc82f1/v11-correction/validator-report-v2.json` | `87E2230BD8ABBD6EA87D46BA07C5056C0BDE1DC77A6109A820F53D169E81324D` |

- Registros históricos sin cambios: `validate-smoke2-package.py.txt` `8B19A9C725C66A629B03B6DDEA7BBA70149788508E38E92E673C9A6852812C17`; `smoke2-51fc82f1/validator-report.json` `0D415A3F79F90B8B44114C478848FECA954C441510055717827B7EEB52D269C0`; `smoke2-51fc82f1/v11-diagnostic.json` `C22F22222D15F5795172CDFEAF364A77EDCC7DAD2C17BFDFDE873D84DCF4E1D2`.
- Codex 50 de 52; `attempts` 7 de 9.

## 76. Preparación de P2 (2026-10-16T21:22:39Z)

- Orden: «Second Smoke Acceptance / RAM First Run Readiness» (decisiones §71). Ejecutor: Coordinator; solo lectura de la instalación de RAM, `Config.ini`, servicios y paquete; `RAMElements.exe` = 0; sin Codex ni Workers.
- Escrituras fuera del repositorio: `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-prep\payload-vs-manual.json`.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-second-smoke-acceptance-ram-readiness.original.txt` | `FFD94EE7414577B11534FA286BF53EE9631BA73B7213A3F1F8F2EB1B4D9C628A` |
| `docs/initiatives/I-65-f4-p2-ram-first-run-procedure.md` | `7868D5373CBA297277216FB79852CB9789363FE4FF1476BE88AA4D1B61B448E8` |
| `docs/automation/evidence/I-65-f4/p2-prep/host-coordination-p2.md` | `676B6F608CA017F615B316ECEF3592A5ADC7E4BC6DB2918D9022D700AFB2E38C` |
| `docs/automation/evidence/I-65-f4/p2-prep/p2-payload-vs-manual.py.txt` | `A10D5B46938CC88CE4700E491C00A4BEB6A90167C21C684EC2C57D8C45F959FA` |
| `docs/automation/evidence/I-65-f4/p2-prep/payload-vs-manual.json` | `B28C39215392A7EA0EFEA808B777730D049B1E709DEFA17C255AFD2E89EE98B5` |

- Paquete sin cambios (21:12Z): `19fba962…`, `d9b41aab…`, `25f48e7f…`; `ram-readback.json` inexistente.
- Codex 50 de 52; `attempts` 7 de 9.

## 77. P2 ejecutada (2026-10-16T22:20:55Z)

- Orden: «P2 — First Real RAM Elements Transfer and Equivalence Test» (decisiones §72-§73). Ejecutor: Coordinator con Computer Use; sin AutoCAD, Codex ni Workers.
- Corrida local: `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-R20261016T2147Z\` (Config.ini antes/después, herramienta congelada, volcados, capturas).
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/p2-prep/host-coordination-p2.md` | `875502206EEF54BE46322817EF64BCCFCA69D09F53DB01793FC98D9B8A2D15B9` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/comparison-console.txt` | `EC1C4A79915590072DDF49A0ACFE7D09B8F83B6D9B52128409B33A0799FD4D6A` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/comparison-report.json` | `D3C669AE875374401FC5D8B2BA7818E457A3EB58C23F65178B86A8BB22D207ED` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/config-ini-diff.txt` | `D68E6CDD8001877EF4A03CED7D1BFD553DE7B2CD6E6863668B432CD38930418A` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/generated-rackcad-ram-dump.py.txt` | `D30C9802589AA50FEE79C72EA38B013B17F5F854E1D288A1C4C8E72028B1ED0E` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/generated-ram-dump.json` | `F5789D1CE923A648B77B192571C7271AC40901043AE92301E443624E4F664BE0` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/manual-entry-sheet.json` | `BEEFA05491491A18D6B944BF72B7E5D80C9FDCCCFA2196B174EA3C44F1DDAE77` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/manual-rackcad-ram-dump.py.txt` | `AF26248953F033CCAF82D9435B85C282B7BD647E532D2642D86D9E3322D47F29` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/manual-ram-dump.json` | `C4A98FB0EA798031DE2B3052D81EAE89F42B731A92F06DBFA97F148ADE9BA23B` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/p2-manual-entry-sheet.py.txt` | `2E518C0073B88C5BEA706F407DC0F8FCAB0F412D114DAA42C9237038DEECA307` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/p2-preflight.ps1.txt` | `F6446C485B95B4FCF901702B9379572AF9294C04F697090F3C0BC7B8E78DCC59` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/p2-report.md` | `B2F368A98CEB8997D0E017124AD2C54048694FB8AD9E1744B7281397E125C148` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/post.txt` | `573804D62128B40FAEBEF2199D4F01DE82B1274605BBC7610A19A7ABFA4C19C1` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/preflight.txt` | `3205DDEF62368B071C592C2C0E633DD0218AA48CD466CC402BD33E4061D5621F` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/ram-readback.json` | `E622F429CAC805BB7CD5F07B4E377833AE79AB26297AE20E2C400AF2B5E91511` |

- Capturas: 30 archivos en `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/captures/` (resumen SHA-256 de nombre+hash `8524D0C35200750A5FBC899D330E68E95269DC864F900F7340F917BEADAB0001`).
- Paquete después: `manifest.json` `19FBA962…`, `rackcad-ram-build.py` `D9B41AAB…`, `structural-loads.json` `25F48E7F…` (sin cambios) + `ram-readback.json` `E622F429…`.
- Codex 50 de 52; `attempts` 7 de 9.

## 78. Preparación del Candidato de hito de F4 (2026-10-16T22:39:32Z)

- Orden: «P2 Acceptance / F4 Final Candidate Preparation» (decisiones §74). Ejecutor: Coordinator; solo lectura de la rama, de `origin`, del host y de la instalación de RAM; ensayo de `inject` en el scratchpad de la sesión (los cuatro tipos con código 0 y el origen `pkg-01` intacto; no vale para la OV); un subagente Explore de solo lectura compiló el estado de los hallazgos; sin AutoCAD, RAM ni Codex.
- Custodia (SHA-256): `docs/automation/decisions/I-65-owner-disposition-p2-acceptance-candidate-prep.original.txt` `F0AC3F03D63D54334DE10DB78A4EE7F8C855CE25E53E600895069CD6D4CBA5FC`; `docs/initiatives/I-65-f4-final-candidate-preparation.md` `76F0FA6E4B4123915DBDD69A2E1B9172C78EB9A963B3C10C700538FD6951D048`.
- La evidencia local y de CI del SHA propuesto (este commit) se registra en §79, después de crearlo y con el árbol limpio.
- Codex 50 de 52; `attempts` 7 de 9.

## 79. Evidencia del SHA propuesto para el Candidato de hito `5a5ca577` (2026-10-16T22:57:56Z; hecho posterior al commit)

Producida **después** de crear `5a5ca57731ebbbbc41ed4f702698709d5e128a86`, con el árbol limpio antes y después, sobre ese HEAD exacto
(AGENTS.md, «Orden para la evidencia local de un Candidato»). Bloque de la [guía §7.1](../../guias/validacion-manual-autocad.md),
**PROPUESTO — NO DECLARADO** (READY-03 y READY-06 pendientes; decisiones §74):

```text
Candidate SHA:        5a5ca57731ebbbbc41ed4f702698709d5e128a86   (PROPUESTO, no declarado)
Arbol limpio:         SI            <- antes y despues de producir la evidencia local
SDK resuelto:         8.0.423
Core Full local:      PASS          (14390/14390, 0 omitidas, 3 m 8 s)
UI Full local:        PASS          (1637 superadas, 0 con error, 17 omitidas = las 17 `Skip =` declaradas en tests/RackCad.UI.Tests)
Debug UI build:       PASS          (0 advertencias, 0 errores)
Debug Plugin build:   PASS          (0 errores; 2 MSB3277 conocidas de las referencias de AutoCAD)
CI exact SHA:         GREEN (run 38977069487, push, head_sha 5a5ca577, job ui-tests "UI Tests (WPF controls, net8.0-windows)" success; 4/4)
Owner validation:     required (OV-I65-01, OV-I65-03..07 pendientes; no autorizadas)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg (override de %APPDATA%\RackCad\settings.json)  SHA-256 b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8
```

- **AutoCAD** de referencia: R25.0.171.0.0, `acad.exe` `2a75996f…6422` (segundo smoke); no se abrió.
- **DLL del Candidato** (compilada desde `5a5ca577` con `-o` a una carpeta **nueva**; `acad.exe` = 0): `D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows\RackCad.Plugin.dll`,
  SHA-256 **`c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07`**, 263 680 bytes, ProductVersion `1.0.0+5a5ca577…`;
  `RackCad.Application.dll` `7f1f3cd2…8f5b`; 32 archivos ([manifiesto de salida](I-65-f4/candidate-5a5ca577/output-manifest.txt));
  tabla junto a la DLL `fc3d9146…b619` = blob `d088e2a9…` (las dos filas `verified`). Las DLL anteriores (`2e04a4bb…`, `fa334b14…`) siguen
  intactas y **no** son la del Candidato.
- **Herramienta de F4 del Candidato:** `D:\r65-smoke\5a5ca577\ramtool\`, `.exe` `0b56e198…8720`, `.dll` `d421c82b…9538`, `1.0.0+5a5ca577…`.
- **Entradas efectivas del validador v2:** cargas congeladas `D:\r65-smoke\5a5ca577\inputs\ref-cant-01.loads.json` `25f48e7f…ae05`;
  [valores esperados](I-65-f4/candidate-5a5ca577/expected-inputs.json) calculados con el `RackCad.Application.dll` del Candidato
  (`rowsSha256` `54ca9f6c…c98c`, igual que antes).
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/expected-inputs.json` | `B3D101C1331632BB3F6CCD19C9D28F5C0A713BAFF12D628BB621E2F10C409911` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/local-evidence.txt` | `FCF506CC0264D6CF49B2061C9097F7AF0EB1A69607786644C983EE5E917F8556` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/output-manifest.txt` | `ED2875E42FB467F0294DF15F8E688D968FF058942E7E98B5BB0C8CBF7B5DB22B` |

- Este registro está en un commit hijo y **no** extiende la evidencia a ese hijo (AGENTS.md, «Commit documental»).
- Codex 50 de 52; `attempts` 7 de 9.

## 80. Revisión del Architect y propuesta de declaración del Candidato de hito `5a5ca577` (2026-10-17T00:14:36Z)

- Orden: decisiones §75-§76. Architect por subagente de solo lectura; Coordinator sin AutoCAD, RAM, Codex ni Workers.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/architect-f4c-conformance.md` | `33022B6FACD725E2CF1103D2B7D8C2FACDC6107468D809C6D360758935003B12` |
| `docs/automation/evidence/I-65-f4/architect-f4c.prompt.md` | `5B95A1060ECBF7211CE86D98DEE904A999591DC2B910787566D3F0AFD664FADB` |
| `docs/automation/evidence/I-65-f4/architect-f4c-k5-transcript-scan.json` | `46A610DFD274BC8D15808EFE64E63DAB4F6EAC9D2B217C695F4839A82BBD23EC` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/ci-run-38977069487.json` | `62EDED2C094E7D1A654F0AC8CCA873D30ECE453C551147B5DD460D00117E2D4F` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/ci-run-38977069487-jobs.json` | `B39EADD5A3AE23BEC118B6D76D163F4C98CF073466BEFDBB45FCD2550E0DEAB3` |
| `docs/initiatives/I-65-f4-final-candidate-preparation.md` | `5AF6E1CE1E31B9ECF775DDC1CA8ED1A82FCAC4BA3F11C6BCA84D88182C3F0161` |

- **Propuesta de bloque de declaración (guía §7.1), pendiente del acto del Owner:**

```text
Candidate SHA:        5a5ca57731ebbbbc41ed4f702698709d5e128a86   (Candidato de hito de F4; PROPUESTO, pendiente de la declaración del Owner)
Arbol limpio:         SI            <- al producir la evidencia local (§79)
SDK resuelto:         8.0.423
Core Full local:      PASS          (14390/14390)
UI Full local:        PASS          (1637 superadas, 0 con error, 17 omitidas declaradas)
Debug UI build:       PASS
Debug Plugin build:   PASS          (DLL c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07, 1.0.0+5a5ca577; -o D:\r65-smoke\5a5ca577\, desviación registrada de la guía §2)
CI exact SHA:         GREEN (run 38977069487, event=push, ref=refs/heads/architecture/interop-analisis-estructural, head_sha exacto, job ui-tests success; 4/4)
Owner validation:     required (OV-I65-01, OV-I65-03..07; W1 AutoCAD + W2 RAM; no autorizadas)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8   <- se vuelve a medir en W1
Conformidad:          Architect CONFORMANT WITH OBSERVATIONS (0 REQUIRED, A65-F4C-01..08); Coordinator CONFORMING
Registro posterior:   decisiones §74-§76 y evidencia §78-§80 en hijos documentales (e8c7f73a, 9a6f69d6 y este commit), sin cambio de producto
Cobertura:            no es requisito previo (WORKFLOW §2.bis/§7); se mide después, cuando ci.yml esté en main
```

- Codex 50 de 52; `attempts` 7 de 9.

## 81. Declaración del Candidato de hito F4 `5a5ca577` (2026-10-17T02:36:27Z)

- Orden: decisiones §77 (Owner: F4 MILESTONE CANDIDATE DECLARATION = AUTHORIZED). Coordinator sin AutoCAD, RAM, Codex ni Workers; sin
  cambios de producto.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-f4-milestone-candidate-declaration.original.txt` | `E9E11923E69C54F1A302D8463CD88500434B7894258B4283B81812B645F9D8DF` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/predeclaration-recheck.txt` | `6C906864DCE788773F6FFAF8B2A00B968923F4D1D80EE5386BB0894CB0C1994B` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/local-evidence.txt` | `FCF506CC0264D6CF49B2061C9097F7AF0EB1A69607786644C983EE5E917F8556` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/output-manifest.txt` | `ED2875E42FB467F0294DF15F8E688D968FF058942E7E98B5BB0C8CBF7B5DB22B` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/expected-inputs.json` | `B3D101C1331632BB3F6CCD19C9D28F5C0A713BAFF12D628BB621E2F10C409911` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/ci-run-38977069487.json` | `62EDED2C094E7D1A654F0AC8CCA873D30ECE453C551147B5DD460D00117E2D4F` |
| `docs/automation/evidence/I-65-f4/candidate-5a5ca577/ci-run-38977069487-jobs.json` | `B39EADD5A3AE23BEC118B6D76D163F4C98CF073466BEFDBB45FCD2550E0DEAB3` |
| `docs/automation/evidence/I-65-f4/architect-f4c-conformance.md` | `33022B6FACD725E2CF1103D2B7D8C2FACDC6107468D809C6D360758935003B12` |
| `docs/automation/evidence/I-65-f4/f4d-table-target.json` | `FC3D91464AC48A15730661EFCD50ECB770114559C68BDEFAE67E0816972DB619` |
| `docs/automation/evidence/I-65-f4/p2-R20261016T2147Z/manual-ram-dump.json` | `C4A98FB0EA798031DE2B3052D81EAE89F42B731A92F06DBFA97F148ADE9BA23B` |
| `docs/initiatives/I-65-f4-final-candidate-preparation.md` | `5AF6E1CE1E31B9ECF775DDC1CA8ED1A82FCAC4BA3F11C6BCA84D88182C3F0161` |

- **Verificación previa (02:30Z):** las siete comprobaciones del Owner pasan (decisiones CD-I65-F4-188, registro literal arriba).
- **Bloque de declaración (guía §7.1):**

```text
Candidate SHA:        5a5ca57731ebbbbc41ed4f702698709d5e128a86   (Candidato de HITO de F4; DECLARADO por autorizacion del Owner, decisiones §77; no es FINAL_CANDIDATE_SHA)
Arbol limpio:         SI            <- al producir la evidencia local (§79)
SDK resuelto:         8.0.423
Core Full local:      PASS          (14390/14390)
UI Full local:        PASS          (1637 superadas, 0 con error, 17 omitidas declaradas)
Debug UI build:       PASS
Debug Plugin build:   PASS          (DLL c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07, 1.0.0+5a5ca577; -o D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows, desviacion registrada de la guia §2)
CI exact SHA:         GREEN (run 38977069487, event=push, ref=refs/heads/architecture/interop-analisis-estructural, head_sha exacto, job ui-tests success; 4/4)
Owner validation:     required (OV-I65-01 y OV-I65-03..07 en W1 AutoCAD + W2 RAM; no autorizadas todavia)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8   <- medida de nuevo a las 02:30Z; se vuelve a medir en W1
```

- **Custodia adicional del Candidato:**

| Elemento | Valor |
|---|---|
| Tabla verificada | `assets/structural-analysis/ram-elements/section-material-map.json`, blob `d088e2a9…` = `f4d-table-target.json`; junto a la DLL, SHA-256 `fc3d9146…b619`; las dos filas `verified` (D-09, decisiones §62-§66) |
| Herramienta de F4 | `D:\r65-smoke\5a5ca577\ramtool\` (`RackCad.StructuralAnalysis.RamTool.exe` `0b56e198…`, `.dll` `d421c82b…`) |
| Entradas congeladas | cargas `25f48e7f…`; valores esperados del validador v2 `expected-inputs.json` (`b3d101c1…`) |
| Architect | CONFORMANT WITH OBSERVATIONS, 0 REQUIRED, A65-F4C-01..08 aceptadas como OPTIONAL (decisiones §76-§77) |
| Coordinator | CONFORMING (CD-I65-F4-180) |
| Disposiciones del Owner | §74 (P2 aceptada, Q-O1/Q-O7, DIF-04), §75 (hallazgos), §77 (declaración, lectura de LIFECYCLE, estrategia A solo F4, OV-03, observaciones) |
| Registro posterior | decisiones §74-§77 y evidencia §78-§81 en hijos documentales (`e8c7f73a`, `9a6f69d6`, `55a228ba` y este commit), sin cambio de producto; lectura aceptada por el Owner solo para el hito |
| Cobertura | no es requisito previo (WORKFLOW §2.bis/§7); se mide después, cuando `ci.yml` esté en `main` |

- **Matriz de la validación final del hito** (preparación §6 con la adenda §11 y las disposiciones de §77; todo sobre `5a5ca577`):

| OV | Ventana | Evidencia que decide | Estado |
|---|---|---|---|
| OV-I65-01 | W1 (`pkg-OV01`, validador v2 16/16, carga útil 117/117 frente a REF-CANT-01) + W2 (construcción, lectura de vuelta, volcado en la misma sesión) | comparador sin `--*-unread` PASS (0) con `manualDumpSha256` = `c4a98fb0…` (estrategia A, solo F4); propiedades por la hoja D-09 | PENDIENTE |
| OV-I65-03 (a)-(e) | W1 | código esperado en la consola (`STATION-FACE-UNSUPPORTED`, `STATION-SIDE-UNSUPPORTED`, `ARM-SLOPE-UNSUPPORTED`, `ARM-ARRANGEMENT-UNSUPPORTED` o `SECTION-FAMILY-UNSUPPORTED`, `MATERIAL-ROW-MISSING`) y carpeta sin crear | PENDIENTE |
| OV-I65-03 «fila sin verificar» | ninguna (sin host) | tres pruebas de fixture en verde en la Core Full del SHA exacto; clase de evidencia sustituida por el Owner (§77); **no** es una ejecución en AutoCAD. Primer smoke (`745653e1`) solo informativo | CUBIERTO POR PRUEBAS AUTOMATIZADAS |
| OV-I65-04 | W1 (`pkg-OV04`, v2 16/16) + W2 | rechazo `gate:` antes de escribir; lectura de vuelta `FAILED`; conteos sin cambio | PENDIENTE |
| OV-I65-05 | entre ventanas (`inject unknown-section`) + W2 | consola y lectura de vuelta `FAILED — DISCARD MODEL`; comparador solo como control | PENDIENTE |
| OV-I65-06 | entre ventanas (`inject inverted-axes`) + W2 | `FAILED` con la diferencia de ejes locales | PENDIENTE |
| OV-I65-07 | entre ventanas (`wrong-version`, `missing-core-function`) + W2 | rechazo antes de escribir; Elementos: 0; lectura de vuelta `FAILED` con `gate:` | PENDIENTE |

  Aceptación de F4 (AC-F4-07) sin cambios: OV-I65-01 PASS, salvo las filas informativas, y OV-I65-03..07 PASS, con custodia. Después:
  revisión del Architect, cierre y GATE PASS solo con orden del Owner.
- FINAL READY DECLARATION = NO; W1 = NO; W2 = NO; F4 GATE PASS = NO. Codex 50 de 52; `attempts` 7 de 9.

## 82. Procedimiento de W1 versionado (2026-10-17T04:54:58Z)

- Orden: decisiones §78 (W1 PROCEDURE PREPARATION = AUTHORIZED). Sin AutoCAD, RAM, Codex ni Workers; sin cambios de producto.
- Custodia (SHA-256; los archivos de cargas, por sus bytes con LF, iguales a las copias de `D:\r65-smoke\5a5ca577\inputs\`):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-authorization-w1-autocad-final-validation.original.txt` | `3DEFC38B2EF38D19164DC0FD6298AC8BFE743F0B27EB71A57B996D49AACC0DF6` |
| `docs/initiatives/I-65-f4-w1-autocad-procedure.md` | `A7F49707D861ABE5502EAB1F942EB19275E132614A1AD892888F66535AA38E77` |
| `docs/automation/evidence/I-65-f4/w1-prep/w1-dryrun.ps1.txt` | `F5725F602CE16F051A996F86C36D854F6472556F0DD8DE8660F8AD603627BFB1` |
| `docs/automation/evidence/I-65-f4/w1-prep/w1-dryrun-output.txt` | `84560544CD6224D902C65EB2760D4EE81848A31D337D76A1BFC21E5862D940CF` |
| `docs/automation/evidence/I-65-f4/w1-prep/w1-hostsnap.ps1.txt` | `5C33A842FA1A39D4DD9CE9EC1DD4B9143B3271FCEECA0D59D570F3B551B35BA4` |
| `docs/automation/evidence/I-65-f4/w1-prep/loads/ov03a-double-face.loads.json` | `8E05FAADBFC2304B0CEBEA1BF4F1187C3734391CCAAFF0792C5F661444EBE87B` |
| `docs/automation/evidence/I-65-f4/w1-prep/loads/ov03b-negative-y.loads.json` | `87E642305C6AA758EC7E6B07D7940272C91557CA23F03737C137959CBA80DE90` |
| `docs/automation/evidence/I-65-f4/w1-prep/loads/ov03c-slope-over-10deg.loads.json` | `25F48E7F75D879C4668C3D9B37F69FDA0CD5442A407C03FF76362754623CAE05` |
| `docs/automation/evidence/I-65-f4/w1-prep/loads/ov03d-paired-channels.loads.json` | `25F48E7F75D879C4668C3D9B37F69FDA0CD5442A407C03FF76362754623CAE05` |
| `docs/automation/evidence/I-65-f4/w1-prep/loads/ov03e-section-without-row.loads.json` | `25F48E7F75D879C4668C3D9B37F69FDA0CD5442A407C03FF76362754623CAE05` |

- Comprobación en seco con las DLL congeladas del Candidato (`RackCad.Application.dll` `1.0.0+5a5ca577…`): REF-CANT-01 `Valid`; (a)-(e)
  con su código esperado y nada más; controles registrados. Es preparación, no evidencia de OV.
- Copias en `D:\r65-smoke\5a5ca577\inputs\` hechas sin sobrescribir nada (el `ref-cant-01.loads.json` congelado no se tocó).
- W1 aún no ejecutada. Codex 50 de 52; `attempts` 7 de 9.

## 83. Coordinación de host de W1 (2026-10-17T08:47:16Z)

- Custodia literal: `docs/automation/evidence/I-65-f4/host-coordination-w1-2026-10-17.md`, SHA-256 `CFB37D63BA263289780C7C2811E1EF5DB32724069CB3FBFED804BDBB0C3EAE79`.
- W1-PR-01..03 cumplidas (procedimiento en `origin` `afa10180`, CI 39004356219 4/4; avisos hechos; confirmación expresa de I-64 a las 08:45Z). Siguiente: preflight W1-PR-04..09.

## 84. W1 ejecutada sobre el Candidato `5a5ca577` (2026-10-17T09:15:22Z)

- Orden: decisiones §78 (CONDITIONAL GO) y §79. Ejecutor: Coordinator con Computer Use (solo «AutoCAD 2025 - English»); sin Codex,
  Workers ni RAM; sin cambios de producto. Informe: `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/w1-report.md`.
- Custodia versionada (SHA-256 de los bytes versionados; textos con LF):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/after-w1-prep.txt` | `8F54CB089539E774140A74F26408DD329C53B30942199FF6C8E6036975E4FA5E` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/baseline-acl-codepage-reconciliation.txt` | `041424FDC2EDA24BB9ACEB46632CAB7BDF391704B571A227E863BA70323B4654` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/baseline-summary.txt` | `F6E16C2B9564551A435BDEEB69473A2E1049DD00B3744572B7E35A096FAB1F7F` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/diff-registry-keys.txt` | `192CBF0E719C911A9D3890AB86807F0B638316FE1BB7033B20949879EF9D007A` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/dump-OV01-gen-rackcad-ram-dump.py.txt` | `56FAA3550934A663889FB78B0B7787350839ED6B3F0747CF9064FC2A5AE1A2D5` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/inj-OV05-manifest.json` | `7335A76992C1D44C3D02062E70F0D5148F86D84259DB84BFA5748A722D179179` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/inj-OV06-manifest.json` | `8C6E1C4D9CB34001CB424F4C39CEE076D0AD2FC40784B8DBF0E2B915F73635CC` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/inj-OV07a-manifest.json` | `72A6E289EA5C7AD6D2A66696BC15E61D8092BF76B0221379865B792525976567` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/inj-OV07b-manifest.json` | `CB4972AB776304D8B0511F7ACC4E5A92CCC291E818FB20B6B0519403E2494C6C` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/k1-k2-compare.json` | `27FE382E5E2E1AB7A55142B30C8C7229DD5E476ADF45EA35493E391B01E8F032` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/module-list-after-netload.txt` | `280619700D16660F67B33DE9BFB569A139CE53A0AFC8F6F5875E34ECE4C1BE14` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/module-list-before-close.txt` | `4D198DFA4F004C966FD6465D9C054759FDC12C34742134577089E897F995BC66` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/payload-vs-manual-pkg-OV01.json` | `BF0D8075EF4B0DEC53A23814425F3F7335F60A2EB13778CE92BF8AC0B3F354C7` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/pkg-OV01-manifest.json` | `6B013B62423E6E1590849A9BACDD5E4242D6930DB6DE040CC3F62507444A4EDC` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/pkg-OV04-manifest.json` | `8506FDCDD2F6FC45B2F9D22D53ABB71BFE948E1091DD0F33743D2D414D1FA326` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/post-summary.txt` | `791EE1FE313CC4C1F58CBCD00921C6E2E88264BDC7A9F0A99CF989DE1B7CC622` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/timeline.txt` | `AEF4D74C8282277CF23C1AE82113AE27D3198A8D6C6787D4DD652026C9D95775` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/validator-v2-pkg-OV01.json` | `CC23450C2A9DD7EB4073D728FC9D359E26694E3E3E961EF8B21C5360F2512CAB` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/validator-v2-pkg-OV04.json` | `4FFBA937784F13B301042FDD82D644CB0649232A6E68C1F901D68AFECE4898FF` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/w1-idle.ps1.txt` | `AC61C01BAE9975E335B5632BB0ED6066F341F1005CB44AF3FD30C06E690FA760` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/w1-report.md` | `D20FEAA5DCB3FD73FFF6BBC8CE13734445EE8132A826A3DD3F625D3591CDB091` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/w2-readiness-readonly.txt` | `0CE8832A4C841DAFBC10E1699024E0FC424820F5CB741814DAFA4F6B99C45EC3` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/w2-readiness.ps1.txt` | `466F7A933042B34E423EB021CF99DD942C57A45D62391DB306935D0297BAB827` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/01-dibujo-nuevo-drawing1.jpg` | `5586C94972AADA77545C79C9FF377123CA56D18317477A77F6BF911523F7FBCC` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/02-netload-dll-candidato-seleccionado.jpg` | `11B80B7E1C723604933F94E705D41822446ADEE818852963CA8DD1AAD1534D50` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/03-aviso-seguridad-load-once.jpg` | `06A5D17E736347686FEC62E45E597867FB80769D0F1BB1C779C1E25F174A16D4` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/04-ref-columna-base-w12x26.jpg` | `8F83343B71E5A54F8E545BC85A8A6763D3C3AAA142E612C7A9A59B01931E0186` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/05-ref-brazo-hss4x4x1_4.jpg` | `3D0C8662C3831F0449FAF50AF503B866D5C82D68087C61F04B335A4605C834E1` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/06-ref-linea-resuelta.jpg` | `F7E643C18FCF667A5BCCFCBFE87B56563DE1375C6C2C3568C392D66C6C46E511` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/07-ref-vista-frontal-insertada.jpg` | `6D0A04BC90B630BDEEF53BC19722CDB362193E44C3E5BD7E1F7AD56440D5F0F9` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/08-ov01-cargas-tecleadas.jpg` | `7ADD1FC27F3E458646BE325AD36E4204D18440F2DEC4CD3F797B52A24EE05962` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/09-ov01-carpeta-tecleada.jpg` | `B5D3F04E7D8A3CB89C32C6C0CB6701453ED3B01699AEE13A88D2FC75E86D707F` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/10-ov01-consola-valid.jpg` | `F2D08EB6802A5803C327EFF41CCAA57EBF89E00BA430177855D049269AF6E3C7` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/11-ov04-carpeta-tecleada.jpg` | `F9A3F681C2994F3C978078FEFF2E746C1A728B7636928C27EB739352F512B915` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/12-ov04-consola-valid.jpg` | `356C446AD5B5A7B072D51C30641552E050110E3851643563D96D4FFF1345BDE9` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/13-ov03a-linea-doble-resuelta.jpg` | `635E8DD309B94791834406D998DD24DF0E1BF5F4CDDBA8BC8C41CF96A33BFA2A` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/14-ov03a-carpeta-tecleada.jpg` | `B7A179DC410C27AF5F9405508AB92565FEA8C21415DF9B43C242FBDFA7E4F4D3` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/15-ov03a-consola-unsupported.jpg` | `84A7599859DCE82616596C4DD01BB048B610B3EE4A753A0A255A894C39D79FEB` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/16-ov03b-linea-negy-resuelta.jpg` | `FB80105E2066C2922FD52E79EE88A0626C80DEB953C313DC6CFC2E7A8900D31F` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/17-ov03b-carpeta-tecleada.jpg` | `1AA8120377861553FED9E4DF859F72F90F4FE41845BB2CBCD48569A4A4CAC7FF` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/18-ov03b-consola-unsupported.jpg` | `21F7CC7708B144DAEBB7969F3514E1C9562A1AA881ADFC913AE5414D4DE5F02B` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/19-ov03c-brazo-pendiente-3.jpg` | `A6DC59FFEBCC0A1CF544C5A5CABC6C6623832DF66922FD34C57B546F41B39DD3` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/20-ov03c-linea-resuelta.jpg` | `66EE2C4610F54EAB3124E4E376C01F8D170BF6C08F5976DCCBF060085E9875C2` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/21-ov03c-carpeta-tecleada.jpg` | `9F351DD5953BED4BAB1E0CD795FC4389A50AA5B1424CC38B711D95F81D8CFC9D` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/22-ov03c-consola-unsupported.jpg` | `E341E3C10BCDC6E40B3090D450B02F8751E3477027369BC15A4C6A106D16CCB6` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/23-ov03d-brazo-canal-doble-c4x5_4.jpg` | `A2C87D036285A347CE8CE0C9D397108AA654F07D49BD4F3FEB4E93FFFEC83043` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/24-ov03d-linea-resuelta.jpg` | `7A355FE5C7A643F8B0523C1CD48F553CFEAB8160530C0C395968041A4AD73B6F` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/25-ov03d-carpeta-tecleada.jpg` | `69DCF53A8601BC1DC68D85071A0292D16F3011F76A6D1FEFF9A73081357253EE` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/26-ov03d-consola-unsupported.jpg` | `021A93AD2A281FE3ACA503EC6E3B9F5AEDAFD21960B9FD93126505F5244DB529` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/27-ov03e-brazo-hss4x4x5_16.jpg` | `B65446F501F7CD1FC55D6980A854B4E14C596A34E65133D938AE93C4628A7A12` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/28-ov03e-linea-resuelta.jpg` | `6C6A45EFF42A46245305FB89E093CED71673640AF2B57F670F090800086A4865` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/29-ov03e-carpeta-tecleada.jpg` | `838B24029FB74F02808C19F7BF8D866E639F97623E5CF9F7D631205B41860608` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/30-ov03e-consola-unsupported.jpg` | `533426622FECB436C7A887F61C5DDD8E0D264C33726958151743318D5DB6C451` |
| `docs/automation/evidence/I-65-f4/w1-R20261017T0847Z/captures/31-cierre-sin-guardar.jpg` | `32FF9271A58D50036F55993C7C70888B7D92C12C78FC09E721A2976452489301` |

- Archivos locales (no versionados enteros; `D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\`, SHA-256 de los bytes):

| Archivo | SHA-256 |
|---|---|
| `pkg-OV01/manifest.json` | `6b013b62423e6e1590849a9bacdd5e4242d6930db6de040cc3f62507444a4edc` |
| `pkg-OV01/rackcad-ram-build.py` | `d01551f7cfa78a80e787471b763e9af8b1a5944681f64190d918a14e5d3c2f3f` |
| `pkg-OV01/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `pkg-OV04/manifest.json` | `8506fdcdd2f6fc45b2f9d22d53abb71bfe948e1091dd0f33743d2d414d1fa326` |
| `pkg-OV04/rackcad-ram-build.py` | `623cbba5e2302e9495ff781b5341c50421a66025ccf0ca8fadc84ca22ea33cde` |
| `pkg-OV04/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `inj-OV05/manifest.json` | `7335a76992c1d44c3d02062e70f0d5148f86d84259db84bfa5748a722d179179` |
| `inj-OV05/rackcad-ram-build.py` | `c995ccb006a113ac3889e2d3b88dc381a0cb32cdcf09cd482416641f17943e39` |
| `inj-OV05/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `inj-OV06/manifest.json` | `8c6e1c4d9cb34001cb424f4c39cee076d0ad2fc40784b8dbf0e2b915f73635cc` |
| `inj-OV06/rackcad-ram-build.py` | `1d36513d3822c4fd53bd1c1a6f3648445606765db05132ac3673c93683cc407c` |
| `inj-OV06/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `inj-OV07a/manifest.json` | `72a6e289ea5c7ad6d2a66696bc15e61d8092bf76b0221379865b792525976567` |
| `inj-OV07a/rackcad-ram-build.py` | `b6016021012a6c523fa03940d0d674c152f703304a1601a7d9fd9bf7bc595486` |
| `inj-OV07a/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `inj-OV07b/manifest.json` | `cb4972ab776304d8b0511f7acc4e5a92ccc291e818fb20b6b0519403e2494c6c` |
| `inj-OV07b/rackcad-ram-build.py` | `6ad0d8916aac8287596197135fe0086727efcbf6a6cc2d916fae1b62990d734c` |
| `inj-OV07b/structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |
| `dump-OV01-gen/rackcad-ram-dump.py` | `56faa3550934a663889fb78b0b7787350839ed6b3f0747cf9064fc2a5ae1a2d5` |

- W1 = SATISFACTORIA; W2 no autorizada. Codex 50 de 52; `attempts` 7 de 9.
- Recomprobación de I-52 tras W1: sin deriva; INFORMAR registrado sin acción; no autoriza ventanas posteriores. Custodia actualizada: `docs/automation/evidence/I-65-f4/host-coordination-w1-2026-10-17.md`, SHA-256 `7C5CC0DF416D2B301C72F2118793FBDA0F86B5BE533129EA82E4A52F5E46CABC`.

## 85. Procedimiento de W2 versionado (2026-10-17T15:57:13Z)

- Orden: decisiones §80. Sin RAM, AutoCAD, Codex ni Workers; sin cambios de producto.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-authorization-w2-ram-final-validation.original.txt` | `786153FF65A61F8C2BC5CEBD97DCC07E8FBAE7E9ACE01A667AC67455B487973A` |
| `docs/initiatives/I-65-f4-w2-ram-procedure.md` | `ABDD4C74E04E0260B6DA9BDF462F2ED9EA8F5840CE2FFE9D49D1C2DC5627B18C` |
| `docs/automation/evidence/I-65-f4/w2-prep/w2-hostsnap.ps1.txt` | `6904A43965DE69B33B78C304CD57D1958B31B2018121906CF22D034724FF5485` |

- W2 aún no ejecutada; entra en vigor con este commit en `origin`, su CI 4/4 y las condiciones W2-PR-01..10.

## 86. W2 ejecutada sobre el Candidato `5a5ca577` (2026-10-17T16:23:54Z)

- Orden: decisiones §80-§81. Ejecutor: Coordinator con Computer Use (solo «RAM Elements 2026»); sin Codex, Workers ni AutoCAD; sin
  cambios de producto. Informe: `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-report.md`.
- Custodia versionada (SHA-256 de los bytes versionados; lecturas de vuelta, volcado e informes del comparador con sus bytes exactos):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/db-tree-python-method.txt` | `C732B054FA3CB7A1347DA8A35401C4C327A3309A4F4960CDB6644638988454BA` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/dump-OV01-gen-ram-dump.json` | `F5789D1CE923A648B77B192571C7271AC40901043AE92301E443624E4F664BE0` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/inj-OV05-ram-readback.json` | `0957AFAF34F7134400BD161E2AF8C3351C14AFFA7F8729B4EE735FAD13A9E8B9` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/inj-OV06-ram-readback.json` | `B0DF7CEA6329D61FE473A354677B5E43F3BD9898BCEB14E24F22746D28D725CA` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/inj-OV07a-ram-readback.json` | `E92F6B38205C9EFDA83EEEDD87DF6BCFCC364033D4C47D0565450BB48A22C018` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/inj-OV07b-ram-readback.json` | `0EBF9D007FA6A342D9DCE456C6E09776BD193DE84EFD0E4D965B806C33322F52` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/k1-k2-w2-compare.json` | `D5EE2EED235905698403496486182D7D2DAAE04B62D3756F98020C9E5926FC04` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/pkg-OV01-ram-readback.json` | `0848B78528518A0D9B26C48188EF6FEB2EBC6E8C0640D3CBE2836394F607116E` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/pkg-OV04-ram-readback.json` | `0D011958F41AF106797B1B71D9F7CA73BB79A29528D9FB0CB5D10B29B1A4E037` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/timeline-w2.txt` | `D4A68D18C748025BEDC5A501FD603CE3C5BD1EF139986290AF7A6828DAE563FA` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-OV01-comparison-console.txt` | `80F674264337B3308DE755221160AD82A1B79240462CB996E145D7D33701CB4A` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-OV01-comparison.json` | `3705AFE9DE9AD6839BB765346C4C76EEAFCBE83B953BA9164F661FB5CA151A0A` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-OV05-control-console.txt` | `CAC225ED66B6DB0505FF5CC7AD05A8F69D82CFEC4AC027ACF6AC9996165FC0F6` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-OV05-control.json` | `1D6FC23BDB97A3A661A8CE84E68D07927CB681AA9B42CB2E1FC22927E0119894` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-config-ini-diff.txt` | `48AF6A9EBA3C9615C597E091D6E46442813372EE0A0FBC250D2F62EF5C166647` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-post-summary.txt` | `F065A4E94B388A74B1EA7DAC140E7B10C212C9A758CB7EFA9BD440787ED170E9` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-pre-summary.txt` | `A0251D29DA2FAF58BF1D125F446B2B4F9A07E7EEB144A7636A942B8CFC91F80A` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/w2-report.md` | `E6F07D1D25606859A5F52BB0ED6E4ED60E5695F619AB6426EC0BF19B9EB5EC78` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/01-ram-modelo-nuevo-vacio.jpg` | `AB590971D829BDF118CC77D6E8955E709D432BB03C8B8BFF03B1978748FC1690` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/02-barra-estado-licencia-elementos-0.jpg` | `5194F612116B65A6A614AF5FC1C9B67136B548072CFC4B5CA55C2B02EFFDB49B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/03-ov01-script-del-paquete-seleccionado.jpg` | `465E9FFB1CB7B03BDA978973A7EED25AD7542A0E447D3F0D1248CD91F9C7BF23` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/04-ov01-consola-readback-ok-huella.jpg` | `6F94AEAE950721C6B7E5784CB3E3C86135BBB08FC32C5FC8DA107F5D505A44DC` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/05-ov01-barra-estado-elementos-15.jpg` | `7AD05F55B8C4A1DE538ABD22333157D8D701A591BE1EE66BC7A7916051AEE59F` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/06-ov01-volcado-seleccionado.jpg` | `0718A22BD492CC31813E9B24AB155698B34A24C56511412DE621FE727DDF1546` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/07-ov01-consola-dump-ok.jpg` | `878D11B5665F37071BC9FFFDAA1EEB8849EC0ACB26E58A8F934B7A56481DE27E` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/08-ov01-descartar-no.jpg` | `5CB46449E27FE6560E61903359A51D7D23C673027437D0D84B111F49DA4306C1` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/09-ov04-nudo-de-prueba-0-0-0.jpg` | `6120C1AE415F068F15CC51ACCF5903D12EBDD6140CC49AC050FEE2239A7699B2` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/10-ov04-barra-estado-elementos-1-antes.jpg` | `22612E9DBA6F6AF111ABA3AE650D2CC165DA42E1CFB1E9187A72DD4E51C8E20B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/11-ov04-script-seleccionado.jpg` | `76CAC8E03B0E4D7965D2B6CE782017B207377C4476DA8722FC67050B827B47CD` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/12-ov04-consola-gate-1-nodo.jpg` | `8E6CE09F99A390A5005633287AFF29028BA99C12E9EF00EC0269C0FEB16377B2` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/13-ov04-barra-estado-elementos-1-despues.jpg` | `22612E9DBA6F6AF111ABA3AE650D2CC165DA42E1CFB1E9187A72DD4E51C8E20B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/14-ov04-rejilla-nudos-despues.jpg` | `C6F7BF396F4B48311762FCB3465765226717EC0F7E06C984CE102D34E527D799` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/15-ov04-descartar-no.jpg` | `5CB46449E27FE6560E61903359A51D7D23C673027437D0D84B111F49DA4306C1` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/16-ov05-script-seleccionado.jpg` | `0D0740BBD90DFDA8685F2F888AB5D45001941B49210AFCB2F8129C02723D9313` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/17-ov05-consola-seccion-rechazada.jpg` | `7382D4F5F883D48FEE7688EB294610DFCE61A94637C081DBCAAAAF811F171A03` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/18-ov05-barra-estado-elementos-9-parcial.jpg` | `FE67B62DAC51BE21268A07EB30E5022F5EF87810B63C819360F6F4BA59C43EEF` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/19-ov05-parcial-nudos.jpg` | `B08952754D8A6C7F79A6169C1A5EF6E07284ADAF6664B7ACCC01C21E905119E7` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/20-ov05-parcial-miembros.jpg` | `ADE7CC4F45D75E66FA7079B794A21AD0023DEC5E1E334EFA80659EA852551FF8` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/21-ov05-descartar-no.jpg` | `5CB46449E27FE6560E61903359A51D7D23C673027437D0D84B111F49DA4306C1` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/22-ov06-script-seleccionado.jpg` | `2EE84544BF151B7D311B7DA2243D5C17DC22460C7DADE6F3CC35AB2A65307ACF` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/23-ov06-consola-eje-local-2.jpg` | `7E8F4058378AF1A6C99E7E98A6521ABD708A5AAA561C7EEE2325C877BF359680` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/24-ov06-barra-estado-elementos-15.jpg` | `7AD05F55B8C4A1DE538ABD22333157D8D701A591BE1EE66BC7A7916051AEE59F` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/25-ov06-descartar-no.jpg` | `5CB46449E27FE6560E61903359A51D7D23C673027437D0D84B111F49DA4306C1` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/26-ov07a-barra-estado-vacio-antes.jpg` | `5194F612116B65A6A614AF5FC1C9B67136B548072CFC4B5CA55C2B02EFFDB49B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/27-ov07a-script-seleccionado.jpg` | `189BA366906856417F9CD0E55B1A0486564BBE4EC05B87D0889C61231C528E72` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/28-ov07a-consola-gate-version.jpg` | `603E3148B1C7C445BC61C140B621A006B5CF4396F90F43406FEE2C43747A8312` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/29-ov07a-barra-estado-vacio-despues.jpg` | `5194F612116B65A6A614AF5FC1C9B67136B548072CFC4B5CA55C2B02EFFDB49B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/30-ov07b-barra-estado-vacio-antes.jpg` | `5194F612116B65A6A614AF5FC1C9B67136B548072CFC4B5CA55C2B02EFFDB49B` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/31-ov07b-script-seleccionado.jpg` | `E1818EFE19F12D5E10B562D77C2B7B0D6A571509FA60ADF5A79B178C95AEC0BC` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/32-ov07b-consola-gate-funcion.jpg` | `AED8DDCA50463D3AD3E6164AC68A8F2BA76F513F2FC5AF75EE62F51E3C763F19` |
| `docs/automation/evidence/I-65-f4/w2-R20261017T0847Z/captures/33-ov07b-barra-estado-vacio-despues.jpg` | `5194F612116B65A6A614AF5FC1C9B67136B548072CFC4B5CA55C2B02EFFDB49B` |

- **Matriz de la validación final del hito (evidencia §81), resultado:**

| OV | Ventana | Resultado |
|---|---|---|
| OV-I65-01 | W1 + W2 | **PASS** |
| OV-I65-03 (a)-(e) | W1 | **PASS** |
| OV-I65-03 «fila sin verificar» | sin host | cubierto por las pruebas de fixture (CD-I65-F4-186) |
| OV-I65-04 | W1 + W2 | **PASS** |
| OV-I65-05 | entre ventanas + W2 | **PASS** |
| OV-I65-06 | entre ventanas + W2 | **PASS** |
| OV-I65-07 (a, b) | entre ventanas + W2 | **PASS** |

- W2 = SATISFACTORIA. F4 GATE PASS = NOT YET AUTHORIZED. Codex 50 de 52; `attempts` 7 de 9.

## 87. Revisión final del Architect y criterios de cierre de F4 (2026-10-17T17:00:40Z)

- Orden: decisiones §82-§83. Architect por subagente de solo lectura; Coordinator sin AutoCAD, RAM, Codex ni Workers.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/architect-f4-final-conformance.md` | `32E4876DA6EAA808D686C1D1D427AF904E276FC20CA34EED0D2521E6A2E1F020` |
| `docs/automation/evidence/I-65-f4/architect-f4-final.prompt.md` | `561F531DD511ABB190BAE49F89F92D0FBC7B9128965D9139D6E4E4BD4319B620` |
| `docs/automation/evidence/I-65-f4/architect-f4-final-k5-transcript-scan.json` | `6C060294FB3B954B730AB10A5F45A418897037F87175712A364877624582433F` |

- **Matriz final de F4 (Candidato de hito `5a5ca577`; filas separadas por clase de evidencia, A65-F4F-03):**

| OV | Ventana / clase | Resultado | No evaluado o informativo |
|---|---|---|---|
| OV-I65-01 | W1 (AutoCAD) + W2 (RAM) + comparador | **PASS** | propiedades de sección y material NOT_EVALUATED (D-09); categoría del caso manual y peso propio INFORMATIVE; DIF-01 informativa |
| OV-I65-03 (a) doble cara | W1 | **PASS** | — |
| OV-I65-03 (b) brazo `NegativeY` | W1 | **PASS** | — |
| OV-I65-03 (c) más de 10° | W1 | **PASS** | — |
| OV-I65-03 (d) canales pareados | W1 | **PASS** | variante C/L: NOT_EVALUATED en el host (alternativa de la orden; pruebas de Core de `SECTION-FAMILY-UNSUPPORTED`) |
| OV-I65-03 (e) mapeo ausente | W1 | **PASS** | — |
| OV-I65-03 «fila sin verificar» | pruebas de fixture (sin host) | cubierta por pruebas automatizadas; NOT_EVALUATED en el host | primer smoke `745653e1` solo histórico |
| OV-I65-04 | W1 + W2 | **PASS** | — |
| OV-I65-05 | inyección + W2 | **PASS** | elementos parciales documentados y descartados; el REJECTED del control no discrimina por diseño |
| OV-I65-06 | inyección + W2 | **PASS** | — |
| OV-I65-07a / 07b | inyección + W2 | **PASS** / **PASS** | — |

  FAIL: 0. INCOMPLETE: 0. OV-I65-02 (F5) y OV-I65-08 (READY) no aplican a F4.

- **Criterios de cierre (AC-F4-01..11, `I-65-f4-preparation.md` §7, y LIFECYCLE §7):**

| Criterio | Estado | Evidencia |
|---|---|---|
| AC-F4-01 | CUMPLE | orden del Owner y contrato de gate (§26) |
| AC-F4-02 | CUMPLE | Freeze `4e6e1feb…` y ADR-0050 `c18def42…` sin cambios; sin A-n |
| AC-F4-03 | CUMPLE | F4-T1..T6 y F4-D con RED→GREEN verificados; Core 14390/14390 y CI 38977069487 4/4 sobre el Candidato |
| AC-F4-04 | CUMPLE | CD-I65-F4-166, §75-§76 |
| AC-F4-05 | CUMPLE | F4-D §62-§66 (filas `verified`, hoja D-09); DIF-01..04 dispuestas; OD-F4-03 |
| AC-F4-06 | CUMPLE | Candidato de hito declarado (§77, evidencia §81) |
| AC-F4-07 | CUMPLE | matriz de arriba (W1 §79, W2 §81; aceptadas por el Owner §80 y §82) |
| AC-F4-08 | CUMPLE | producto igual a `51fc82f1`; sin análisis, guardado ni sobrescritura de modelos de RAM |
| AC-F4-09 | CUMPLE | Architect CONFORMANT WITH OBSERVATIONS sin REQUIRED (CD-I65-F4-216); Coordinator CONFORMING (CD-I65-F4-218) |
| AC-F4-10 | **PENDIENTE** | commit de cierre (este) → árbol limpio → Core Full local → push → CI del SHA exacto → GATE PASS registrado después |
| AC-F4-11 | CUMPLE | no se activó: OV-I65-01 PASS sin hipótesis incorrectas |
| LIFECYCLE §7.1 resultado observado | CUMPLE | hito del Owner (§80, §82) y pruebas |
| §7.2 RED→GREEN | CUMPLE | revisiones de F4-T1..T6 y F4-D |
| §7.3 pruebas y evidencia | CUMPLE | evidencia del Candidato (§79, §81) y validación final (§84, §86) |
| §7.4 CI del SHA de cierre | **PENDIENTE** | CI de este commit |
| §7.5 revisión del Coordinator | CUMPLE | CD-I65-F4-218 |
| §7.6 orden commit → árbol limpio → evidencia local → push → CI | **PENDIENTE** | Core Full local sobre este HEAD y CI |
| §7.7 rebase entre gates | CUMPLE | sin rebase; `origin/main` = `bb0d5522` (fetch fresco) |

- **SHA de cierre de F4 = este commit.** Candidato de hito `5a5ca577`; código = árbol de `51fc82f1`. El CI de este commit no da evidencia
  nueva a `5a5ca577` (AGENTS, «Commit documental»).

## 88. Cierre de F4: evidencia del SHA de cierre `1e7d8fc2` (2026-10-17T17:11:14Z)

- Orden: decisiones §82-§84. Sin AutoCAD, RAM, Codex ni Workers; sin cambios de producto.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f4/close-1e7d8fc2/ci-run-39052279094-jobs.json` | `577F011F4880C4BA9504BFFD60AA427AB5973F37F63848A297F9956B44710CC2` |
| `docs/automation/evidence/I-65-f4/close-1e7d8fc2/ci-run-39052279094.json` | `2AD22DE2E4AD5511FA55249B08105F8350A9EDA4AF2C32C3A645A53495792882` |
| `docs/automation/evidence/I-65-f4/close-1e7d8fc2/local-evidence.txt` | `EF6529C88C2EF4CF47F93087A4087BE16F16DA2DFE22F696DC2278F022D1C76C` |

- Core Full local sobre `1e7d8fc2` (árbol limpio antes y después): **14390/14390**, 0 con error, 291 s; TRX `bc3b28747893fde0512a30805ef44531b47bf86fb7457c7b1afd025ca818fc4c` (no versionado).
- CI 39052279094: `push`, rama exacta, `head_sha` `1e7d8fc23e089619f61d40194969cecb09762336`, 4/4 jobs `success` (Tests, UI Tests, Build UI,
  Build Plugin).
- **GATE PASS de F4** registrado en decisiones §84 (CD-I65-F4-221). Candidato de hito `5a5ca577`; no es READY.

## 89. Revisión del alcance de F5 y de privacidad (2026-10-17T18:00:45Z)

- Orden: decisiones §85. Solo lectura del Freeze, del código y de la evidencia; inventario de las 146 capturas de I-65 por un subagente de solo lectura (`sonnet`); comprobación de la visibilidad del repositorio con la API de GitHub (`private: false`, `visibility: public`). Sin AutoCAD, RAM, Codex ni Workers.
- Custodia: `docs/initiatives/I-65-f5-scope-and-ready-review.md` SHA-256 `F4F6EFF3DC8886E2603E002C48BA7254FE41A32B715B396E277B2A679EC32424`.
- El subagente del inventario creó tres archivos temporales con sus búsquedas en `%TEMP%` (fuera de los checkouts); no tocó el worktree.

## 90. Preparación de F5 y contención de privacidad (2026-10-19T00:53:18Z)

- Orden: decisiones §86. Solo lectura del Freeze, del código y de la evidencia; comprobaciones sin AutoCAD ni RAM; sin Codex ni Workers; sin
  capturas nuevas. Los archivos de esta preparación no contienen datos personales (comprobado mecánicamente).
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-privacy-containment-f5-preparation.original.txt` | `A8EADA6C6FE3019A840675FEE4F4A0F9F1C4DDD46217F8443BCB3835454B0F1D` |
| `docs/initiatives/I-65-f5-preparation.md` | `2511170EA4F1B8A7C0E47F89D9C2DE99AD3EA1F2DD83E09E4CA8E8DF9F8E7396` |
| `docs/automation/evidence/I-65-f5-prep/contrast-test-run.txt` | `2F496D9C63595261A349BD08FFAC22B1FD4A544BD0FC6EA0B3DF330FFF95018B` |
| `docs/automation/evidence/I-65-f5-prep/f5-dryrun-output.txt` | `FACB315F979532FCEC8AD06CDFAE6CB44AA9778FB528C7413D87DF34A354DFB6` |
| `docs/automation/evidence/I-65-f5-prep/f5-dryrun.ps1.txt` | `7DC01426047E7BBED619DBD347E81E8DF908CC86A14B6877FA8C9F8B8641AA3D` |
| `docs/automation/evidence/I-65-f5-prep/f5-manual-entry-sheet.py.txt` | `66D099C356F981FD1DC104768733959A5A715BE60C1667E5391F84F255E65C56` |
| `docs/automation/evidence/I-65-f5-prep/ov-i65-02-manual-entry-sheet.json` | `B15B9F9E28BE3DB82AF3C1C65DA5E07ADCE877B28BF9438960848B7B40394ECA` |
| `docs/automation/evidence/I-65-f5-prep/ov-i65-02.loads.json` | `D675D9F6BB537083A63571F08FB9884C1AB085A0ECA4AD1555EE98BEA4F8BA4C` |
| `docs/automation/evidence/I-65-f5-prep/readback-dump-contrast.py.txt` | `0790930297D221DDEFEADAEF8E04611858B2E81446717155E754D34155114BB1` |
| `docs/automation/evidence/I-65-f5-prep/redaction-candidates.txt` | `16B9413670E32EE0D12C139819CB66C9777B3D1B7BC5D875648C203708AF1F6A` |
| `docs/automation/evidence/I-65-f5-prep/session-custody-test.jsonl` | `B8218171267B2A219295AC15BEC36D3A9F491F7C68365FBCE296D2BF0CCF2810` |
| `docs/automation/evidence/I-65-f5-prep/session-custody.ps1.txt` | `5487544AEC2685C8554145B46D68BD4FE8FE9545E0B1628DEEE84D7FC080491B` |

- Archivo de cargas de OV-I65-02: SHA-256 de los bytes LF `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c`.
- Visibilidad del repositorio a las 00:46Z: `public` (pendiente de la acción del Owner, C-1).

## 91. Disposición de aceptación técnica de F5 y preparación del Candidato de hito de F5 (2026-10-19T01:36:25Z)

- Orden: decisiones §87. Solo documentación en este commit (SHA propuesto del Candidato de hito de F5); producto igual al árbol de
  `51fc82f1` (`src`, `tests`, `tools`, `Directory.Build.*` y `global.json` con los mismos árboles y blobs). Freeze (`4e6e1feb…`) y
  ADR-0050 (`c18def42…`) sin cambios. Sin AutoCAD, RAM, NETLOAD, Codex ni Workers.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-f5-acceptance-candidate-privacy-gate.original.txt` | `85E1F40C9D40CE7071D4BF4292F7D6214B182D37BC29DB483D8D43204C7852EC` |
| `docs/initiatives/I-65-f5-w3-w4-procedures.md` | `88625E4E4A85D9F08105B3BAC13C2912C0928C0E3C6DEF865E749654B2698519` |

- La evidencia del Candidato (Core Full y UI Full locales, builds Debug, manifiesto de salida, identidad de la DLL, valores esperados del
  validador v2 y CI del SHA exacto), la revisión del Architect, la conformidad del Coordinator y el bloque de declaración propuesto se
  versionan en commits posteriores, como evidencia de ese SHA exacto.

## 92. Evidencia del SHA propuesto del Candidato de hito de F5 `bd1fcf8a` (2026-10-19T02:00:06Z)

- Orden: decisiones §87 (CD-I65-F5-09). Evidencia del SHA exacto `bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109`, producida **después** del commit y con el árbol
  limpio (HEAD antes y después = ese SHA; `status --porcelain` vacío antes y después; 01:38:32Z-01:54:08Z). SDK 8.0.423 (dotnet del usuario).
  Base de `main` `bb0d5522`, 0 commits detrás. Producto = árbol de `51fc82f1`. Sin AutoCAD, RAM, NETLOAD, Codex ni Workers.

| Comprobación | Resultado |
|---|---|
| Core Full | **14390/14390** superadas, 0 con error (253 s) |
| UI Full | **1637 superadas, 17 omitidas** (las mismas omisiones declaradas que en F4), 0 con error; total 1654 (627 s) |
| Build Debug de UI | 0 errores, 0 advertencias |
| Build Debug del Plugin (`-o D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows`) | 0 errores; 2 advertencias MSB3277 (conflicto de versiones de `Microsoft.VisualBasic`, igual que en F4) |
| Build Debug de la herramienta (`-o D:\r65-smoke\bd1fcf8a\ramtool`) | 0 errores, 0 advertencias |
| Salida del Plugin | 32 archivos; manifiesto `bd096bec0f2410449a61dae28a3571bb77e55af99e6cc5e86d978605b03a301f`; frente a la salida de F4 solo cambian los 8 binarios `RackCad.*` (dll/pdb, por el SHA estampado); catálogos, `deps.json` y tabla iguales |
| **DLL del Candidato** | `RackCad.Plugin.dll` SHA-256 **`81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5`**, ProductVersion `1.0.0+bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109` (Application, Domain y UI con la misma versión; `identity.txt`) |
| Tabla junto a la DLL | `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619`, 3279 bytes, sin CR, blob `d088e2a9…` = blob en el SHA |
| Herramienta de F4 | 11 archivos; `RackCad.StructuralAnalysis.RamTool.exe` `21290785e13f2daee51b10b9d0b8911f86e73798e2cf49e668ce791a78e9ce7c` (1.0.0+bd1fcf8a…) |
| Archivo de cargas de OV-I65-02 | `D:\r65-smoke\bd1fcf8a\inputs\ov-i65-02.loads.json`, copiado del blob del SHA: `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c`, 1112 bytes, LF |
| Valores esperados del validador v2 | `D:\r65-smoke\bd1fcf8a\expected-inputs.json`, SHA-256 de los bytes en disco `c61a6e1255783f3c59f3e236c0057bbd88b3b1eabbbe424ba7dced71f22ddaf8` (CRLF; cf. A65-F4F-01); filas `54ca9f6c…` = F4; **no versionado** (PRIVACY HOLD: incluye el nombre del verificador); resumen en `expected-inputs-summary.txt` |
| Comprobación en seco con las DLL del Candidato | modelo `Valid` (0 diagnósticos), paquete `Valid` (3 archivos) en memoria, nada escrito en disco; **6/6 IGUAL** a la tabla de la preparación §4; huella `3dd3f8bc…3c7c`; línea `PAYLOAD` idéntica byte a byte a la de las DLL de `5a5ca577` (`939dc7e6…`); Q2 = 99.99999999999997 % (CD-I65-F5-08) |
| CI del SHA exacto | run **`39187567306`** (`push`, rama de I-65, `head_sha` = el SHA): **4/4 success**, con `UI Tests (WPF controls, net8.0-windows)` |

- **Host:** `RAMElements.exe` = 0. Hay un `acad.exe` del usuario abierto desde las 00:38:19Z con un dibujo propio y con la DLL del
  Candidato **de F4** cargada desde `D:\r65-smoke\5a5ca577\…` mediante NETLOAD manual (primera entrada del MRU del diálogo; ningún
  registro de carga automática de RackCad). No se interactuó con él; los builds escriben en la carpeta nueva `D:\r65-smoke\bd1fcf8a\`.
  Para W3 se exige `acad.exe` = 0 (procedimientos W3-PR-04).
- **Privacidad:** los registros completos de pruebas y builds y los `.trx` no se versionan (rutas con el usuario y el equipo); del CI solo
  se versiona un resumen de campos técnicos (sin el JSON crudo de la API). Barrido de los archivos nuevos (`privacy-secrets-scan.txt`):
  **0 secretos y 0 datos personales**. Visibilidad del repositorio: `public` (comprobado a las 01:57:19Z; API autenticada `private: false`, consulta anónima 200): PRIVACY HOLD vigente, C-1 pendiente del Owner.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/ci-run.txt` | `998B0AC830E094410CB3F84E669DBCF4B2C922F68F72D98E83CB786FD368E8F0` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/expected-inputs-summary.txt` | `812E73D7C68E8F1AC801C6C7E2D56B9796242DC7E7EEFC682FE9ADFA9D2F433D` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/f5-dryrun-candidate-output.txt` | `7845A74E31E407FB575E5DC9357DA9E659A5A6E095B4DAAFBE221DB451B4DF45` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/identity.txt` | `C0C0B2DA6C0614112634F04843C2EBDE327C786CE5CB016605ADD29A90E69B27` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/local-evidence.txt` | `64EFD31E7B3FB48E5C3A1207F7566F24DE3653C75D160EC8FEF16C36F742B4DE` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/output-manifest.txt` | `BD096BEC0F2410449A61DAE28A3571BB77E55AF99E6CC5E86D978605B03A301F` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/privacy-secrets-scan.txt` | `3148AB7774D82F3D5E2CCD86C1211BF1562A23C1EDE710490574713679CC4964` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/ramtool-manifest.txt` | `3E6EC61F389A3F88E231665550D44E6A55509DC08D5D195398EB0F8F58C8780D` |

## 93. Revisión del Architect, conformidad del Coordinator y comprobación previa a la declaración del Candidato de F5 (2026-10-19T02:27:45Z)

- Orden: decisiones §87-§88. Architect (opus, solo lectura) de 02:01Z a 02:21Z con K1/K2/K5; comprobación previa con fetch fresco a las
  02:22:50Z; conformidad del Coordinator. Sin AutoCAD, RAM, NETLOAD, Codex ni Workers; producto sin cambios.
- Resultado: Architect **CONFORMANT WITH OBSERVATIONS** (0 REQUIRED, A65-F5C-01..09 OPTIONAL); Coordinator **CONFORMING**; READY-01..09
  CUMPLE en la lectura de hito. **Candidato NO declarado** (acto del Owner; bloque propuesto en `coordinator-f5c-conformity.md` §6).
- A65-F5C-07: `f5-dryrun-candidate-identity.txt` (DLL cargadas y qué se escribió), `tools/` (copias saneadas; la lista local de nombres del
  escáner no se versiona), `expected-inputs.redacted.json` (nombre del verificador redactado; el original `c61a6e12…` sigue en disco).
- Barrido de secretos y datos personales de §92, §93, el estado y los archivos nuevos: 0 coincidencias. Visibilidad: `public` (02:22:50Z).
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/evidence/I-65-f5/architect-f5c-conformance.md` | `630648A36AB58923B9CA533150559BBF414C1A8EDE39E8A70FFD39DAEAC7C59B` |
| `docs/automation/evidence/I-65-f5/architect-f5c-k5.json` | `5226C3013237C0AF26292162C5473187FDF5BDADFA3E1AAE8B7F80B37E56248B` |
| `docs/automation/evidence/I-65-f5/architect-f5c.prompt.md` | `492EA0A3A7F6E790A9C0B0F0626098751CFEDA4E3CD6111CAF934B57323C8EE6` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/expected-inputs.redacted.json` | `FC23C447A344432F32BA5495B6CCEEF9D8D60DC4DCE0B9D04EF0F847D574EFC4` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/f5-dryrun-candidate-identity.txt` | `7A9094441C5FE6B9FA7327EEB571812E8C042D3F870E6194948E7FE62C220C0C` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/predeclaration-recheck.txt` | `A544F7F521988CC941131554FCDF00AEA92CE51F7DF2BC7166B67F263B32F657` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/README.txt` | `366728D6E2BA93C2CED7C9B319B4E1598A1D60409879092CF0311A396E27236D` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/cand-evidence.sh.txt` | `B1B151619DAE33544960043391DF4200EFFEE0343F1C39C3F1A2B51165532D52` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/cand-identity.ps1.txt` | `BB73129D18BEEDEE7C300F0DFE9E2699225531E7C4F17EFDEAE858FF98284F4E` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/ci-summary.sh.txt` | `0D4240002C7C3A694EFAB2E88EF4FB3A74AC7D3639FE6FB9E51B7DD4FDEEBF7D` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/expected-inputs-summary.py.txt` | `7734C97C05B054A3C841755201AE24DE193F4E7CDA021A10751371E3AAB09C68` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/f5-dryrun-compare.py.txt` | `39AC443A5A20368AFBAB217309C9E8AFA27AF77E7E7DCF97CDFD0AA950F85265` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/redact-expected-inputs.py.txt` | `9C67B98A1AC080A734C484D1D523435C66C0C291343B6677994C0DA2489460D2` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/tools/secrets-scan.py.txt` | `DAE25FA9FE595CC90D37A357FEB5CF668375D29314D54253EB4EE881E1058DFB` |
| `docs/automation/evidence/I-65-f5/coordinator-f5c-conformity.md` | `FD319001FC041CCBC30B9C08E4791DEEDF1F75AF0EAD7743CAEB9A8BEC324237` |

## 94. Declaración del Candidato de hito de F5 `bd1fcf8a` (2026-10-19T02:50:54Z)

- Orden: decisiones §89 (Owner: F5 MILESTONE CANDIDATE DECLARATION = AUTHORIZED, CONDITIONAL). Coordinator sin AutoCAD, RAM, NETLOAD,
  Codex ni Workers; sin cambios de producto.
- **Comprobaciones inmediatas (02:47:49Z): las ocho pasan** (`candidate-bd1fcf8a/declaration-recheck.txt`).
- **Bloque de declaración (guía §7.1):**

```text
Candidate SHA:        bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109   (Candidato de HITO de F5, OV-I65-02; DECLARADO por autorizacion del Owner, decisiones §89; no es FINAL_CANDIDATE_SHA)
Arbol limpio:         SI            <- al producir la evidencia local (§92; HEAD antes y despues = el SHA, status vacio)
SDK resuelto:         8.0.423
Core Full local:      PASS          (14390/14390)
UI Full local:        PASS          (1637 superadas, 0 con error, 17 omitidas declaradas)
Debug UI build:       PASS
Debug Plugin build:   PASS          (DLL 81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5, 1.0.0+bd1fcf8a; -o D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows, desviacion registrada de la guia §2, como en F4)
CI exact SHA:         GREEN (run 39187567306, event=push, ref=refs/heads/architecture/interop-analisis-estructural, head_sha exacto, job ui-tests success; 4/4)
Owner validation:     required (OV-I65-02 en W3 AutoCAD + W4 RAM; no autorizadas)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8   <- medida a las 02:47:49Z; se vuelve a medir en W3
```

- **Custodia adicional del Candidato:**

| Elemento | Valor |
|---|---|
| Tabla verificada | blob `d088e2a9…`; junto a la DLL `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` |
| Archivo de cargas de OV-I65-02 | blob `dcc1a039…`; `D:\r65-smoke\bd1fcf8a\inputs\ov-i65-02.loads.json` `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c` |
| Valores esperados del validador v2 | `c61a6e1255783f3c59f3e236c0057bbd88b3b1eabbbe424ba7dced71f22ddaf8` (copia redactada versionada) |
| Herramienta de F4 | `D:\r65-smoke\bd1fcf8a\ramtool\` (`.exe` `21290785…`, `.dll` `86298d2c…`) |
| Architect | CONFORMANT WITH OBSERVATIONS, 0 REQUIRED, A65-F5C-01..09 OPTIONAL (decisiones §88-§89) |
| Coordinator | CONFORMING (CD-I65-F5-16) |
| Disposiciones del Owner | §86 (preparación), §87 (especificación y preparación del Candidato), §89 (declaración, A65-F5C-01, observaciones) |
| Registro posterior | evidencia y conformidades (`daa6fb77`, `dad3b40e`), esta declaración y el anexo operativo: actos posteriores admitidos solo para este hito (CD-I65-F5-21) |
| Anexo operativo de W3/W4 | `docs/initiatives/I-65-f5-w3-w4-operational-annex.md` (A65-F5C-03..06) |

- **Matriz de la validación del hito F5** (todo sobre `bd1fcf8a`): OV-I65-02 en W3 (`pkg-OV02` `Valid`, 16/16, carga útil 6/6) y W4
  (`READBACK OK`, volcados `DUMP OK`, comparador PASS con `LOADS` en PASS, cadena de custodia y contraste IGUAL): **PENDIENTE**.
  AC-F5-01, 02 y 08 (Candidato) CUMPLEN; AC-F5-03 CUMPLE con esta declaración; AC-F5-04..07, 09 (cierre) y 10 PENDIENTES.
- Visibilidad: `public` a las 02:47:49Z (PRIVACY HOLD). W3 = NO; W4 = NO; F5 GATE PASS = NO; READY FINAL = NO. Codex 50/52; `attempts` 7/9.
- Custodia (SHA-256):

| Archivo | SHA-256 |
|---|---|
| `docs/automation/decisions/I-65-owner-disposition-f5-milestone-candidate-declaration.original.txt` | `C7B3741BEDA98EE59C830AB342BC709A607BE4608319E138B47131D616BB6FDA` |
| `docs/initiatives/I-65-f5-w3-w4-operational-annex.md` | `FA1955083D2B35C65C1BCF5BB3DC9723CD5A798A4BCC7148D6ADB5FC9CB5CE21` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/declaration-recheck.txt` | `491F4DBEC3A57CC61F358D068BB25155E597653D9E386952B476B76D6CE16FEC` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/predeclaration-recheck.txt` | `A544F7F521988CC941131554FCDF00AEA92CE51F7DF2BC7166B67F263B32F657` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/local-evidence.txt` | `64EFD31E7B3FB48E5C3A1207F7566F24DE3653C75D160EC8FEF16C36F742B4DE` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/identity.txt` | `C0C0B2DA6C0614112634F04843C2EBDE327C786CE5CB016605ADD29A90E69B27` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/output-manifest.txt` | `BD096BEC0F2410449A61DAE28A3571BB77E55AF99E6CC5E86D978605B03A301F` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/ramtool-manifest.txt` | `3E6EC61F389A3F88E231665550D44E6A55509DC08D5D195398EB0F8F58C8780D` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/ci-run.txt` | `998B0AC830E094410CB3F84E669DBCF4B2C922F68F72D98E83CB786FD368E8F0` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/expected-inputs-summary.txt` | `812E73D7C68E8F1AC801C6C7E2D56B9796242DC7E7EEFC682FE9ADFA9D2F433D` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/f5-dryrun-candidate-output.txt` | `7845A74E31E407FB575E5DC9357DA9E659A5A6E095B4DAAFBE221DB451B4DF45` |
| `docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/f5-dryrun-candidate-identity.txt` | `7A9094441C5FE6B9FA7327EEB571812E8C042D3F870E6194948E7FE62C220C0C` |
| `docs/automation/evidence/I-65-f5/architect-f5c-conformance.md` | `630648A36AB58923B9CA533150559BBF414C1A8EDE39E8A70FFD39DAEAC7C59B` |
| `docs/automation/evidence/I-65-f5/coordinator-f5c-conformity.md` | `FD319001FC041CCBC30B9C08E4791DEEDF1F75AF0EAD7743CAEB9A8BEC324237` |
| `docs/initiatives/I-65-f5-w3-w4-procedures.md` | `88625E4E4A85D9F08105B3BAC13C2912C0928C0E3C6DEF865E749654B2698519` |
| `docs/initiatives/I-65-f5-preparation.md` | `2511170EA4F1B8A7C0E47F89D9C2DE99AD3EA1F2DD83E09E4CA8E8DF9F8E7396` |
| `docs/automation/evidence/I-65-f5-prep/ov-i65-02.loads.json` | `D675D9F6BB537083A63571F08FB9884C1AB085A0ECA4AD1555EE98BEA4F8BA4C` |
| `docs/automation/evidence/I-65-f5-prep/ov-i65-02-manual-entry-sheet.json` | `B15B9F9E28BE3DB82AF3C1C65DA5E07ADCE877B28BF9438960848B7B40394ECA` |
