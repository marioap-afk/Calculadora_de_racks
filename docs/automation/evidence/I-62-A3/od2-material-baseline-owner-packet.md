# I-62 — Paquete de decisión del Owner OD-2-MAT: ¿línea base material de la huella de `codex-cli`? (preparado el 2026-10-08; PENDIENTE)

> **Origen.** Disposición del Coordinator, decisiones §57, punto 4 (texto pegado, SHA-256 `00a13ae5…`; DEC L1115): «Evaluar si OD-2 puede pasar
> de un hash exacto de todo `config.toml` a una línea base material verificable sin una decisión del Owner por cada cambio inocuo; si eso modifica
> autoridad reservada al Owner, preparar la decisión del Owner y no resolverla por disposición ordinaria». Análisis completo en
> [od2-evolution.md](od2-evolution.md).
>
> **Entrega.** La solicitud de recuperación de F6 (REQ) ya está publicada y no se retrasa por este paquete. Su §4 (REQ §4, añadido append-only en
> el commit `76b48e78`) ya publica la suspensión de la comparación por clave y los controles del bloque de Codex sin lectura de valores (sección
> «Lectura de valores (no se pide ahora)»). OD-2-MAT se añade a la misma solicitud en una sección posterior, tras el commit de A-3 (decisiones §57,
> punto 6: las decisiones del Owner, concentradas), junto con las demás decisiones pendientes entonces; nunca como solicitud aislada.
>
> **Siglas de cita** (todas en `b0884216`, salvo REQ §4, en `76b48e78`; «L» = número de línea del blob):
>
> | Sigla | Archivo | Blob |
> |---|---|---|
> | V14 | `docs/initiatives/I-62-proposal-v14.md` | `34ad80ea` |
> | FRZ | `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608` |
> | A2 | `docs/initiatives/I-62-A-2.md` (AGREED, no se modifica) | `f1e1d6f0` |
> | LC | `docs/INITIATIVE_LIFECYCLE.md` | `f19896a8` |
> | AP | `docs/AUTOMATION_PLAN.md` | `f525cb1e` |
> | RD | `docs/automation/agent-execution/README.md` | `592dcfd4` |
> | DESC | `docs/automation/agent-execution/adapters/codex-cli.md` | `155f3469` |
> | DEC | `docs/automation/decisions/I-62.md` | `27591f16` |
> | EV | `docs/automation/evidence/I-62-evidence.md` | `6a2b1569` |
> | PKT | `docs/automation/evidence/I-62-prep/owner-decision-packets.md` | `cdb86114` |
> | REQ | `docs/automation/evidence/I-62-F6/requests/F6-recovery-request-2026-10-08.md` | `8fb8ec55` |
> | REQ §4 | el mismo archivo, §4 (L71-93), añadido append-only en `76b48e78` (padre `b0884216`) | `0d436630` |
> | PAS | `docs/automation/evidence/I-62-F6/OD-2/R20261008T2200Z-codex-passive/result.json` | `bda544f0` |
>
> **Por qué es del Owner.** La materia es OWNER-RESERVED: cambia el objeto de una decisión del Owner, OD-2, que pasaría de aceptar una huella exacta a
> pre-aceptar una clase de huellas futuras (LC L94, L221; DEC L913; A2 L227; V14 L910-913). **No puede resolverse por disposición ordinaria del
> Coordinator.** La sesión prepara el paquete y no decide.
>
> **Mientras no se decida:** OD-2 sigue siendo el SHA-256 **exacto** de todo `~/.codex/config.toml` (DEC L890, L931; A2 L227); P-01 no cambia (AP L698
> para I-61; P-01/P-11 para I62); `codex-cli` sigue en **STOP P-01** con la huella vigente `6518EFAB…` sin aceptar. El silencio no es decisión (V14
> L977).
>
> **No bloquea F6.** La recuperación de F6 sigue su camino propio: `BLOQUE-CODEX-CONSUMO` (REQ L62) y, después de medir, una OD-2 exacta sobre la
> huella resultante (REQ L50). Ninguna opción de este paquete acepta `6518EFAB…` ni ninguna otra huella.

## Respuesta en una línea

| Opción | Recomendación de preparación (no es decisión) | Respuesta exacta |
|---|---|---|
| **A** | **sí, ahora** (ver abajo) | `OD-2-MAT = A (mantener OD-2 como aceptación del SHA-256 exacto de todo config.toml; cada cambio sigue siendo P-01 y una OD-2 nueva)` |
| B | no por ahora | `OD-2-MAT = B (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): línea base material de codex-cli por clase cerrada de claves volátiles, neutralizadas en la receta y comparadas por nombre y con digests con clave por clave, que leen valores en el host sin registrarlos; efectiva solo tras medir la neutralización, esa A-n AGREED y una OD-2 exacta de anclaje; P-01 de I-61 y P-01/P-11 en la cesión sin cambio; esa A-n necesita su propia autorización del Owner para leer valores, que esta línea no da)` |
| C | no | `OD-2-MAT = C (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): como B, pero con predicados de valor sobre las claves volátiles evaluados mecánicamente sin registrar valores, en lugar de neutralizarlas en la receta; esa A-n necesita su propia autorización del Owner para leer valores, que esta línea no da)` |
| D | candidata para después de F6, si el Coordinator la ordena | `OD-2-MAT = D (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): CODEX_HOME dedicado para codex-cli con OD-2 exacta sobre su config.toml; antes, una medición read-only autorizada aparte; el Owner autentica la CLI en ese directorio; sin línea base material)` |

Solo cuenta una línea literal del Owner para OD-2-MAT. Una aprobación de otra solicitud (B3, el bloque de Codex u OD-2) no implica ninguna opción
de OD-2-MAT, y ninguna línea de OD-2-MAT autoriza leer valores (sección «Lectura de valores (no se pide ahora)»).

---

## OD-2-MAT — Línea base material frente a huella exacta

**Definición vigente** (sin cambio hasta esta decisión y una A-n): OD-2 es la «línea base de huella por adapter» de V14 §18 (V14 L971). Para
`codex-cli`, la huella es el SHA-256 de `~/.codex/config.toml` y sus nombres de clave saneados (AP L1055-1056; DESC L40). El Owner la ha aceptado
siempre como un valor exacto (`091540ED…`, DEC L890; `9002E854…`, DEC L931). Si cambia: P-01, STOP y una solicitud nueva (PKT L91). El Coordinator
ya resolvió que aceptar un hash futuro por su delta «es demasiado amplio para esa frontera» (DEC L913).

| Punto | Contenido |
|---|---|
| Hechos medidos | Tres actualizaciones automáticas de la app de Codex en ≈ 17 h (binarios escritos el 2026-10-06T18:56Z, el 2026-10-07T02:14Z y el 2026-10-07T11:57Z; REQ L43). Cada una cambió la huella sin ninguna invocación de la sesión. En las dos últimas, la comparación por clave localizó **las mismas 9 claves**, con cambio solo de valores: `mcp_servers.node_repl.command`, siete variables de `mcp_servers.node_repl.env.*` y `notify` (EV L2568-2570, L2879-2882; PAS). En la primera solo se probó la misma estructura (EV L2486-2488). `config.toml` se reescribió entre 3 min y 8 h después del binario (§2.1 del análisis). Vigente: binario `3553cd6e…`, app `26.1002.7124.0`, huella `6518EFAB…` (107 nombres, 39 secciones), estable desde el 2026-10-07T20:05Z |
| Qué son esas 9 claves | Según la documentación del proveedor: `command` es «Launcher command for an MCP stdio server», `env` son «Environment variables forwarded to the MCP stdio server» y `notify` es «Command invoked for notifications». Son claves que **designan ejecutables que Codex lanza y su entorno**, con nombres como `NODE_REPL_TRUSTED_CODE_PATHS` y `NODE_REPL_TRUSTED_SERVICES`. La receta de solo lectura no las fija. Según el código del proveedor en `rust-v0.160.1` (la última CLI medida), `codex exec` arranca el servidor MCP configurado, expone sus herramientas al modelo y lanza `notify` tras cada turno (VERIFIED-SRC; anexo de A-3, §2); en el binario vigente, sin versión observada, UNVERIFIED. Las únicas claves plausiblemente inocuas por nombre (`desktop.*`, `tui.*`) no han cambiado nunca. Según la investigación (anexo de A-3, §5; no normativo), 8 de las 9 designan código lanzado o sus fronteras de confianza y son materiales, y con los hechos de hoy no se demuestra ninguna clase de cambio irrelevante |
| Qué se decide | si OD-2 sigue aceptando solo huellas exactas (A) o si se prepara la enmienda siguiente (previsiblemente A-4; en adelante, «A-4» es solo esa etiqueta prevista) para que una regla acepte, sin otra decisión, las huellas futuras de una clase (B o C), o para aislar la configuración del transporte manteniendo la aceptación exacta (D) |
| Por qué es del Owner | (1) cambia el objeto de una decisión suya: de un valor exacto a una clase (LC L94); (2) la aceptación de cada instancia pasaría a una regla que evalúa la sesión (M-01, LC L83); (3) reabre la frontera de DEC L913; (4) contradice A-2 regla 4, acordada (A2 L227), que solo otra A-n puede sustituir (LC L216); (5) cambia las salvaguardas del host que el Owner autorizó con OD-5 (V14 L910-913); (6) es una decisión de seguridad sobre su host: con B y con C la sesión leería valores en el host, aunque no los registre (digests con clave por clave), contra los términos con los que aprobó OD-2 («nunca valores», PKT L93; RD L366); con B aceptaría además sin verlos cambios de valor en claves de ejecución neutralizadas en la receta (una A-n que cumpla la regla 11, c) de A-3, solo tras medir la neutralización y verificar su función para la versión observada), y con C evaluaría además predicados sobre esos valores. D no cambia OD-2, pero exige una autenticación nueva en su host |
| Qué desbloquea | **A:** nada nuevo; todo sigue igual. **B/C/D:** el encargo de preparar la A-n siguiente, previsiblemente A-4 (su número lo fija LIFECYCLE §6 al publicarse; MATERIAL con consecuencia OWNER-RESERVED; Owner + Architect independiente + Coordinator, LC L221; FRZ L97-98), que tendría que cumplir los requisitos de la regla 11, c) de A-3 (fila siguiente). Ninguna opción tiene efecto operativo antes de que esa A-n esté AGREED y materializada |
| Requisitos de A-3, regla 11, c) | la A-n la ordena una decisión expresa del Owner (OD-2-MAT), se acuerda con la autoridad de LC §6 para una materia OWNER-RESERVED y fija cómo compone con las reglas 1, 3 y 9 de A-3. Como mínimo: (1) conserva la evidencia del cambio de binario y versión, la clasificación de las claves cambiadas, la comparación con las propiedades materiales ya acreditadas, la sonda rápida con presupuesto autorizado, el fallo cerrado ante cambios desconocidos y la ausencia de ampliaciones implícitas de permisos o de consumo; (2) sin aceptación implícita de versiones nuevas, sin presumir mejor a un sucesor y sin desactivar P-01 en producción bajo la autoridad actual; (3) las claves que fijan el sandbox, la política de aprobación o de permisos, la confianza de los espacios de trabajo, las extensiones habilitadas o su origen, o el modo de consumo, y las que designan código que el runtime ejecuta o lanza y las fronteras de confianza de ese código, nunca son acopladas a la versión ni no materiales, por su función y no por la etiqueta de un mapa, y sin función verificada para la versión observada cuentan como tales; (4) tokens de versión observados con independencia de la propia huella, nunca leídos de la configuración que explican; (5) sus reglas se fijan y se cambian solo con la autoridad que esa A-n declare, y se custodian por blob; (6) leer valores, solo con una autorización expresa del Owner, registrada de forma explícita, que nombre el blob del programa ejecutable, las superficies que lee y su caducidad, que ni una disposición del Coordinator, ni una autorización de medición o de consumo, ni la línea de OD-2-MAT sustituyen; (7) publicación por una lista cerrada, sin valores ni hashes sin clave de valores, y causas solo por motivo y nombre; (8) nombres saneados por una lista de permitidos; (9) una línea base derivada solo de una huella aceptada con OD-2 exacta, y un resultado que el aceptante reproduce; (10) un cambio sin cambio de identidad del runtime, una observación inestable, una clave desconocida, añadida o eliminada, un cambio observado durante una cesión o un intento en curso (P-11 y S-04 sin cambio) o una versión no observada nunca cuentan como explicados |
| Qué NO autoriza ninguna opción | aceptar `6518EFAB…` u otra huella; invocar `codex-cli`; leer o editar `config.toml` (ninguna opción autoriza leer sus valores: sección «Lectura de valores (no se pide ahora)»), `auth.json` o credenciales; cambiar `trust_level`, `windows.sandbox` o la configuración de la app; desactivar P-01 de I-61 (AP L698) o P-01/P-11 en la cesión; aceptar huellas añadidas o eliminadas, cambios fuera de la clase o cambios durante una cesión; pre-autorizar el consumo de bloques futuros (A2 L225: «Nunca es permanente ni anticipada»); editar A-2 o A-3 |
| Consumo, coste y tiempo | **A:** ninguno. **B/C/D:** redactar A-4 y su revisión independiente (consumo de revisión por autorizar aparte). Su materialización invalida `MC_I62`: se cierra de nuevo, se resiembra el fixture y se repiten los pilotos afectados (V14 L898-899). B y D necesitan antes una medición read-only autorizada aparte. B y C necesitan una clave durable por host para los digests con clave por clave (estado persistente nuevo en el host) y una autorización de lectura de valores dentro de su A-n. **Beneficio en F6:** con A-2, cada actualización sigue pidiendo una autorización de consumo por bloque (A2 L224-225); D quitaría la OD-2 posterior (de dos actos del Owner por actualización a uno) si la app no reescribe el archivo dedicado; B y C solo para cambios que una A-n que cumpla la regla 11, c) de A-3 pudiera explicar: con los hechos de hoy ninguno de los observados (8 de las 9 claves son de lanzamiento o de confianza y la función de la novena no está verificada para la versión observada); con B, solo tras medir la neutralización y verificar la función para la versión observada |
| Seguridad | **A:** la solicitud de cada OD-2 lleva el hash exacto y los nombres saneados, con la evidencia por nombre de la regla 11, b) de A-3 (hash antes y después con su estabilidad, binario y versión antes y después, nombres añadidos o eliminados y estructura); no se lee ningún valor. **B:** leería valores en el host para los digests con clave (sin registrarlos). Una A-n que cumpla la regla 11, c) no podría aceptar cambios de valor no vistos en claves de ejecución o de confianza: solo tras medir que la receta las neutraliza (`mcp_servers.<id>.enabled`, «Disable an MCP server without removing its configuration», y `notify` vacío; sin medir) y verificar su función para la versión observada; B solo tendría ese efecto sin ellas si la A-n se apartara de ese requisito de forma declarada y con la autoridad del Owner. **C:** la sesión leería valores de configuración de forma mecánica; publicar digests sin clave de valores de baja entropía equivale a publicarlos. Una A-n que cumpla la regla 11, c) no podría aceptar con C ningún cambio de las claves de lanzamiento o de confianza, que sin neutralizar siguen designando código lanzado; C solo tendría ese efecto si la A-n se apartara de ese requisito de forma declarada y con la autoridad del Owner. **D:** una credencial nueva de la CLI en un directorio dedicado (la sesión nunca la lee) y un `config.toml` mínimo que fija el Owner; que la app no lo reescriba es una hipótesis sin medir |

| Opción | Efecto | Freeze / A-n | Riesgo |
|---|---|---|---|
| **A** | sin cambio: cada actualización = P-01 + `BLOQUE-CODEX-CONSUMO` + OD-2 exacta tras medir | ninguna | churn: hasta un ciclo de decisión por actualización (tres en ≈ 17 h) |
| **B** | *R* acepta *H₁* si: misma estructura; solo cambian claves de una lista cerrada por adapter; digests por clave iguales fuera de ella; coincide con un cambio observado del par y la huella resultante es estable (A2 L223); fuera de una cesión; claves de la lista neutralizadas en la receta y medidas inertes. Cualquier otra cosa → P-01 y OD-2 exacta. Cada aceptación por regla se custodia y es reproducible; el Owner puede revocar *R* | A-4: V14 §7, §18 (L971, L977), OD-5 (L910-913), D.3/D.7, A-2 regla 4; AP 16.19, README §13, descriptor y esquemas (`MC_I62` nuevo). M-01..M-06 sí; M-07 y M-08 por duda | depende de una neutralización sin medir; pre-acepta una clase; lee valores en el host (digests con clave); ventana de coincidencia de horas |
| **C** | como B, pero R7 por predicados de valor sobre la lista (p. ej., rutas dentro de la instalación verificada del par; versión = `AppVersion` observada), evaluados sin registrar valores; sin tocar la receta | A-4 como B, y además RD L366 («Nunca se leen…») | lectura de valores contra los términos de OD-2; clave durable por host |
| **D** | `CODEX_HOME` dedicado para `codex-cli`: la app sigue escribiendo su `~/.codex`; OD-2 exacta sobre el `config.toml` dedicado; una actualización solo cambia la identidad del binario (A2-P2 y, si se acuerda, A-3, sin cambiar su regla 1, con la misma limitación de la sonda: anexo de A-3, §5 y §6) | A-4 menor: descriptor (ruta de la huella y entorno de la receta) y su materialización (`MC_I62` nuevo); V14 §18 sin cambio. M-03, M-05 y M-06 sí | hipótesis sin medir; credencial nueva; un `config.toml` mínimo que mantiene el Owner |

- **Recomendación de preparación (etiquetada; la sesión no decide): A ahora; D como siguiente paso a estudiar después de F6, si el Coordinator lo
  ordena.** No recomienda ninguna autorización de lectura de valores. Motivos medidos:
  1. las claves que cambian son de ejecución y de confianza, no inocuas: una A-n que cumpla la regla 11, c) de A-3 no podría aceptar esos cambios
     (con B, solo tras medir la neutralización y verificar la función para la versión observada); B o C solo tendrían ese efecto si la A-n se
     apartara de ese requisito de forma declarada y con la autoridad del Owner, y las dos leerían valores;
  2. en F6, D solo ahorraría la OD-2 posterior, y B o C solo para los cambios que esa A-n pudiera explicar (con los hechos de hoy, ninguno de los
     observados), porque el consumo por bloque de A-2 sigue siendo por actualización, y su materialización obligaría a resembrar el fixture y repetir
     pilotos (V14 L898-899);
  3. D conserva la aceptación exacta del Owner y ataca la causa (la app reescribe el archivo compartido), pero antes hay que medirla.
- **Aprobar (una de):** las cuatro líneas de la tabla «Respuesta en una línea», literales.
- **Rechazar todo cambio:** equivale a `OD-2-MAT = A (…)`. El silencio no es decisión (V14 L977).

## Lectura de valores (no se pide ahora)

- **Ninguna opción la autoriza.** Ni A, ni B, ni C, ni D autorizan leer valores de configuración. A-3 tampoco: su regla 11 no tiene verificador y
  no lee valores ni autoriza leerlos (regla 11, a y b).
- **Qué haría falta.** Toda lectura de valores, incluida la localización por clave de los cambios de valor, exige una autorización expresa del
  Owner que nombre el programa que lee y su blob, las superficies que lee y su caducidad. Este paquete no la pide. Si el Owner eligiera B o C,
  la A-n que encargan necesitaría esa autorización dentro de ella (lo dicen sus líneas literales), también para la instantánea por clave de la OD-2
  de anclaje (*H₀*).
- **Suspensión.** Desde la decisión S-01 (A-3, §12), posterior al commit `b0884216` que custodia la última localización por clave (EV
  L2879-2881; REQ L45), la sesión suspende la localización por clave (comparación por clave de valores) en toda medición. Ya consta, publicado
  antes de cualquier disposición o autorización del bloque, en el §4 de la solicitud de recuperación (REQ §4; commit `76b48e78`).
- **Reconciliación con la solicitud de recuperación pendiente** (REQ, blob `8fb8ec55`). Su fila «Controles» (REQ L48) incluye una «comparación por
  clave antes y después de cada operación». `BLOQUE-CODEX-CONSUMO` (REQ L62) no cubre leer valores, así que la comparación antes y después de cada
  operación del bloque se limita a SHA-256, nombres y estructura saneados, binario y versión, sin comparación por clave; los demás controles de REQ
  L48 (sin `workspace-write`, sin cambios de `config.toml`, `trust_level` ni sandbox, y la invalidación del bloque por un cambio de huella) no
  cambian, ni la línea literal del Owner (REQ L62: «con huella, binario y versión antes y después de cada una»). REQ §4 ya lo dice, y fija que
  `BLOQUE-CODEX-CONSUMO = A` no cubre leer valores y que la disposición `BLOQUE-CODEX` del Coordinator que nombre el bloque se entiende con estos
  controles. La solicitud de recuperación no se retrasa.
- **Secuencia.** La solicitud de recuperación sigue su curso. La suspensión y la reconciliación anteriores ya están publicadas antes de cualquier
  disposición o autorización del bloque, en el §4 de la solicitud de recuperación (commit `76b48e78`), así que ninguna operación del bloque se
  ejecuta con controles distintos de los publicados. OD-2-MAT se añade a la misma solicitud en una sección posterior, tras el commit de A-3, con
  las demás decisiones pendientes entonces.
- **Lecturas anteriores** (registro de hechos, todos en `b0884216`; este registro no afirma ni niega si esas aprobaciones cubrían leer valores):
  1. EV L2487 (§67): para la primera actualización, se revirtió un valor (la etiqueta del binario en `CODEX_CLI_PATH`) y se recalculó el SHA-256 del
     archivo entero; lee un valor, sin digest con clave;
  2. EV L2490 (§67): instantánea propia de la sesión, con digests HMAC por clave (clave solo en el scratchpad), comunicada al Owner en PKT L249;
  3. EV L2497 (§68), EV L2529 (§69) y EV L2569 (§71): comparación por clave dentro de la medición pasiva que ordenó decisiones §50 (DEC L967) y que
     continuó decisiones §54 (DEC L1043);
  4. EV L2551 (§70): comparación por clave dentro de OD-2d-PROBE, cuya aprobación del Owner (DEC L989) nombra «comparación por clave»;
  5. EV L2627-2629 (§75): medición pasiva nocturna de decisiones §54 (DEC L1043), con instantánea por clave nueva;
  6. EV L2879-2881 (§87): localización por clave de las 9 claves de la tercera actualización, citada en REQ L45.

### Relación con las solicitudes pendientes

| Solicitud | Relación con OD-2-MAT |
|---|---|
| `B3-CONSUMO` (REQ L36) | ninguna |
| `BLOQUE-CODEX-CONSUMO` (REQ L62) | independiente: autoriza medir el par `3553cd6e…`/`26.1002.7124.0`; no acepta la huella y no cubre leer valores, así que la comparación antes y después de sus controles (REQ L48) se limita a SHA-256, nombres y estructura saneados, binario y versión, sin comparación por clave; sus demás controles no cambian (sección anterior; REQ §4) |
| OD-2 exacta tras el bloque (REQ L50) | se pide igual con cualquier respuesta a OD-2-MAT, con la evidencia por nombre de la regla 11, b) de A-3. Con B o C, sería además la OD-2 de anclaje (*H₀*); su instantánea por clave leería valores y solo con la autorización de lectura que esa A-n necesite |
| A-3 (candidata) | no define ningún verificador ni ninguna clase de cambios aceptables (regla 11, a y c); conserva la exclusión de todo cambio de huella (Q-A3-10). Con B o C, la A-4 fija cómo compone con la regla 1, c de A-3 y cumple los requisitos de su regla 11, c). Con D, A-3 pasaría a aplicarse a `codex-cli` sin cambiar su texto |
