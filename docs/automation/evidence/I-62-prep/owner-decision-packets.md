# I-62 — Paquetes de decisión del Owner para F6 (DECIDIDOS el 2026-10-06; decisiones §46)

> **Preflight de F6.** Orden del Coordinator «I-62 — F6 OWNER-DECISION PREFLIGHT» ([decisiones](../../decisions/I-62.md) §45), con la autoridad del
> Freeze V14 + A-1 AGREED + F4 GATE PASS (§44). La sesión evalúa y recomienda; **no decide** ninguna OD. Cada decisión es independiente: el Owner puede
> aprobar unas y rechazar otras. Fuente congelada: Proposal V14 §15, §17 (fila F6), §18 y Anexo D (blob `34ad80ea`).
> **Revisión: 2026-10-06** (medición pasiva 2026-10-06T02:26:14Z). La revisión del 2026-10-04 se conserva como historial al final.

> **Decisiones del Owner (2026-10-06T07:14Z, decisiones §46):** OD-5 = A, OD-7 = A (privado `marioap-afk/rackcad-i62-fixture`), OD-2 = A (huella
> `091540ED…`), OD-4 = A (solo `D:\r62-fixture`), **OD-3 = RECHAZAR**. No se vuelven a pedir mientras sus condiciones sigan válidas. Los paquetes de
> abajo quedan como el registro de lo que se decidió.

> **Corrección del Owner (decisiones §48):** **OD-7 = A, repositorio PÚBLICO** (sustituye «privado»); regla general: todo repositorio de CI temporal,
> de fixture, de validación o piloto de I-62 es público por defecto y uno privado exige autorización explícita. **OD-2c = A** (línea base exacta
> `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`).
>
> **P-01 por la actualización de la app de Codex (2026-10-06T18:58Z):** huella `723A6898…` y binario nuevo; `codex-cli` en STOP; **pendiente del
> Owner: OD-2d** (paquete abajo).
>
> **Disposición del Coordinator (decisiones §47):** OD-2b A2 no se adopta; OD-2b-PROBE = A (autorizada por el Owner; ejecutada). OD-2c (huella exacta
> `9002E854…`; paquete abajo), decidida en §48. Orden preferido del Coordinator: A. GitHub Actions del fixture; B-C. sondas y paquete (hechos); D. OD-2c; E. apertura del
> Principal A.

## Respuesta en una línea

| Decisión | Recomendación de la sesión (no decisión) | Aprobar | Rechazar |
|---|---|---|---|
| OD-5 | **APROBAR (A)** | `OD-5 = A` | `OD-5 = RECHAZAR` |
| OD-7 | **APROBAR (A, repositorio privado)** | `OD-7 = A (privado marioap-afk/rackcad-i62-fixture)` | `OD-7 = RECHAZAR` |
| OD-2 | **APROBAR (A, línea base actual)** — antes B | `OD-2 = A (línea base 091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66)` | `OD-2 = RECHAZAR` |
| OD-4 | **APROBAR (A, solo en el fixture)** | `OD-4 = A (workspace-write solo en D:\r62-fixture)` | `OD-4 = RECHAZAR` |
| OD-3 | **RECHAZAR por ahora** — antes condicional | `OD-3 = A (el Owner autentica claude.exe 2.1.270)` | `OD-3 = RECHAZAR` |

**Matriz de dependencias, comprobada contra el Freeze** (§17 fila F6; D.2; D.4): OD-5 antes de todo F6. OD-7 es precondición del cierre de F6 (FX-02 no
llega a VERIFIED sin CI) y la usa FX-04b. OD-2 antes de toda invocación afectada de `codex-cli` (topología A, B, FX-04b; FX-06 si usa `codex-cli`).
OD-3 antes de `claude-cli` en la topología B. OD-4 antes de los Workers Codex con escritura (B y FX-04b). **FX-04a no depende de OD-3, OD-4 ni OD-7**,
pero es un escenario real de F6 y no corre antes de OD-5. La matriz de la orden coincide con el Freeze; sin diferencias.

**Hechos comunes** (medidos el 2026-10-06, sin ejecutar ningún binario ni leer credenciales; [od-passive.json](f6/od-preflight/od-passive.json), script
[od-passive.ps1](f6/od-preflight/od-passive.ps1)):
- `MC_I62` = cierre de F4 = `6f0187cb` (último cambio a superficies normativas; `a8c6af16` y `c1448687` no las tocan). El fixture se siembra desde ahí.
- El fixture no existe: `D:\r62-fixture` ausente; `marioap-afk/rackcad-i62-fixture` no existe en GitHub.
- Procesos vivos por nombre: `claude`, `codex`, `codex-code-mode-host`, `codex-computer-use-swift`, `codex-windows-sandbox-service`.

---

## OD-5 — Permiso de ensayo y sesiones del sistema bajo prueba

**Definición congelada** (§18; §15 «OD-5, una sola semántica»): permiso de ensayo con la semántica única de §15 y apertura de sesiones del sistema bajo
prueba, incluidas las invocaciones de Architect de FX-06 (autorización previa de apertura y consumo, no de transporte). **Bloquea:** todo F6.

| Punto | Contenido |
|---|---|
| Hechos medidos | fixture inexistente; `MC_I62` = `6f0187cb`; app Codex `26.930.3930.0` instalada (Principal B); la app de Claude de esta sesión está activa (Principal A); ninguna OD concedida |
| Qué se autoriza | (1) aceptar que los ensayos son **pruebas del plano (c) en otro repositorio con sus propias autoridades**, no ejecución delegada de §16 de una unidad de RackCad; (2) que la sesión de supervisión cree el fixture local `D:\r62-fixture\` (D.1: remoto bare local, siembra byte a byte desde `MC_I62`, F_norm/F_eff marcado `TEST-ACTIVATION`, FX-U1); (3) que el Owner abra, cuando la receta lo pida, las sesiones A, B y B2; (4) el consumo dentro de los topes de D.3; (5) las salvaguardas del host de 16.4 aplicadas al worktree del fixture (procesos, huella antes y después, cesión) |
| Qué desbloquea | **FX-01, FX-04a y FX-05** con OD-5 sola. Es la precondición de FX-02, FX-03, FX-04b y FX-06, que además necesitan sus otras OD |
| Qué NO autoriza | invocar `codex-cli` (OD-2), autenticar o usar `claude-cli` (OD-3), escritura de Codex (OD-4), crear un remoto en GitHub (OD-7); superar un tope de D.3; acreditar GATE PASS, READY, aprobación del Owner o integración reales; tocar `main`, contadores, decisiones o custodia de RackCad (P-16, FX-05); leer entradas privadas (D.6) |
| Consumo, coste y tiempo | Topes de D.3: ronda A ≤ 2 sesiones de Principal y ≤ 4 subagentes de Claude (las invocaciones de Codex cuentan solo con OD-2); FX-04a ≤ 2 sesiones de B + 1 de B2, **0 invocaciones de modelo**; FX-06 ≤ 2 sesiones de A. Acciones del Owner: abrir A (FX-01, cambio de effort a mitad de sesión), atestar la terminación de A (FX-04a), abrir B y B2 en clones limpios. Sin gasto externo nuevo |
| Seguridad | todo ocurre en `D:\r62-fixture\`, fuera de RackCad; las sesiones usan los ajustes de permisos que el Owner ya tiene (no se cambia ninguno); el oráculo de FX-04a se publica solo como SHA-256 antes de la respuesta de B; FX-05 compara el estado real antes y después |
| Recomendación | **APROBAR (A).** Motivo: es la precondición de cualquier escenario real; con ella sola ya corren FX-01, FX-04a (la prueba de portabilidad, sin invocaciones de modelo) y FX-05; no añade credenciales, infraestructura ni configuración |
| Aprobar | `OD-5 = A` |
| Rechazar | `OD-5 = RECHAZAR` — F6 no procede; otra vía exigiría una A-n (§15: no se congelan dos semánticas) |

## OD-7 — Remoto del fixture con CI

**Definición congelada** (§18; §15; D.1; D.2): remoto del fixture con CI; permiso de infraestructura, no excepción normativa. **Bloquea:** el **cierre de
F6** (sin CI, `Ci` = `not_run`: FX-02 y FX-04b no pueden ser PASS) y FX-04b; FX-06 lo usa en su paso 5 (CI_VERIFIED).

| Punto | Contenido |
|---|---|
| Hechos medidos | `gh` autenticado como `marioap-afk` (almacén del sistema), protocolo Git `https`, alcances del token `gist`, `read:org`, `repo`, `workflow` (bastan para crear un repositorio y publicar un flujo; el token no se lee); `marioap-afk/rackcad-i62-fixture` no existe; RackCad es **público** y tiene Actions habilitadas |
| Qué se autoriza | que la sesión de supervisión, con la autenticación de `gh` ya existente, cree **un** repositorio **privado** `marioap-afk/rackcad-i62-fixture`, lo añada como segundo remoto **del fixture** (nunca de RackCad) y publique un flujo con los jobs `fixture-build` y `fixture-tests` (`ubuntu-latest`, `dotnet test`, `permissions: contents: read`, solo `actions/checkout` y `actions/setup-dotnet`, sin secretos), y que empuje allí los refs del fixture |
| Qué desbloquea | el cierre de F6 (FX-02 VERIFIED con `Ci` pass); FX-04b (VERIFIED sobre G'); FX-06 paso 5 |
| Qué NO autoriza | secretos de Actions, despliegues o permisos de escritura del flujo; un repositorio público; tocar los remotos o los ajustes de RackCad; borrar el repositorio (lo archiva o borra el Owner al terminar); cualquier otro repositorio |
| Consumo, coste y tiempo | estimación: ≈ 20 pushes con CI en F6 (puntos durables, RED/GREEN de FX-02 y FX-04b, X2 de FX-06) × 2 jobs × ≈ 1-2 min ≈ **≤ 80 minutos de runner Linux** (referencia: los jobs ligeros de RackCad, Build UI y Build Plugin, tardaron ≈ 1 min en la corrida 37391012002). Al ser privado, consume la cuota de Actions de la cuenta; la cuota restante **no se midió** (exige un alcance de facturación). Si se agotara, `Ci` = `not_run` y F6 queda pendiente |
| Seguridad | el repositorio privado contiene copias de documentos ya públicos de RackCad, una biblioteca mínima y la evidencia del ensayo (preflights saneados, registros de relevo, identificadores de sesión); privado porque esa evidencia describe el host aunque esté saneada. El token sigue en el almacén del sistema; nada se escribe en el repositorio. FX-05 comprueba que ningún ref del fixture llega a RackCad |
| Recomendación | **APROBAR (A, privado).** Motivo: sin CI F6 no puede cerrarse por definición (D.2), y los alcances `repo` y `workflow` ya existen, así que no hace falta cambiar credenciales. Una variante pública evitaría la cuota pero publicaría la evidencia del host; no se recomienda |
| Aprobar | `OD-7 = A (privado marioap-afk/rackcad-i62-fixture)` |
| Rechazar | `OD-7 = RECHAZAR` — `Ci` = `not_run`; FX-02 y FX-04b UNVERIFIED; F6 no cierra (aceptar la limitación no convierte en PASS una verificación incompleta) |

## OD-2 — Línea base de la huella de `codex-cli`

**Definición congelada** (§18): línea base de huella por adapter (`config.toml`; el Freeze cita `37DD3559…` registrada frente a `42E15A03…` observada).
**Bloquea:** toda invocación afectada: `codex-cli` en A, B y FX-04b (y FX-06 si sus Architects son `codex-cli`); `codex-desktop-session` solo si comparte
`config.toml` (en F2 sigue UNVERIFIED). Momento: antes de la primera invocación afectada.

| Punto | Contenido |
|---|---|
| Hechos medidos | `~/.codex/config.toml`: SHA-256 **`091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66`**, 4 710 bytes, escrito 2026-10-04T21:39:14Z: **los mismos bytes que el 2026-10-04**, sin cambios desde hace ≈ 29 h. App Codex `26.930.3930.0`, igual que el 2026-10-04. Binarios sin ejecutar: `…\app\resources\codex.exe` y `~/.codex/plugins/.plugin-appserver/codex.exe`, SHA-256 `37762753…` (iguales entre sí); `~/.codex/.sandbox-bin/codex.exe`, SHA-256 `081E4DE4…` (2026-09-12). `codex` no está en el `PATH`. Nombres de secciones y claves, saneados: 105 con este escáner (24 sustituidos por `<redactado>`); el 2026-10-04 se contaron 110 con otro escáner sobre el mismo archivo (mismo SHA-256): la diferencia es del método, no del archivo |
| Qué se autoriza | fijar como línea base de la huella del adapter `codex-cli` el SHA-256 `091540ED…` de `~/.codex/config.toml`, con sus nombres saneados, para las invocaciones del fixture dentro de los topes de D.3, incluidas las sondas previas (≤ 2) |
| Qué desbloquea | las sondas y FX-02 (con OD-5 y OD-7); FX-04b (con OD-4 y OD-7); FX-06 con Architects `codex-cli` (con OD-5 y OD-7); FX-03 (con OD-3 y OD-4) |
| Qué NO autoriza | cambiar `config.toml`, la autenticación o el sandbox; aceptar en silencio otra huella: si el archivo difiere en la invocación (o lo reescribe una invocación, DEV-G1C-01 de I-61), es **P-01 → STOP** y una solicitud nueva de OD-2 con el valor nuevo; invocar `codex-cli` fuera del fixture |
| Consumo, coste y tiempo | invocaciones de `codex-cli` dentro de los topes: ronda A ≤ 15 + 2 sondas; FX-04b ≤ 6; FX-06 ≤ 4; FX-03 ≤ 12 si se desbloquea. Cuota de la suscripción ChatGPT existente; en I-61 cada invocación usó ≈ 45 000-115 000 tokens de entrada; coste monetario UNKNOWN |
| Seguridad | solo el hash del archivo y nombres saneados; nunca valores; `auth.json` no se lee. P-01/P-11 comparan la huella en cada cesión |
| Recomendación | **APROBAR (A, línea base actual)**; antes era B. Motivo medido: la huella lleva ≈ 29 h sin cambiar y la app no se ha actualizado, así que aceptarla ahora deja F6 sin otra pregunta. Si cambia antes del uso, P-01 la detecta y se vuelve a preguntar, que es exactamente lo que B haría siempre |
| Aprobar | `OD-2 = A (línea base 091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66)` |
| Rechazar | `OD-2 = RECHAZAR` — ninguna invocación de `codex-cli`: FX-02, FX-04b y FX-03 UNVERIFIED; FX-06 solo con `claude-cli` (OD-3); F6 no cierra por FX-02 |

## OD-4 — Sandbox de `codex-cli` con escritura (Worker Codex)

**Definición congelada** (§18; D.3; D.4): sandbox de Codex para escritura. **Bloquea:** los Workers Codex con escritura de la topología B (FX-03) y de
FX-04b (rebinding a `codex-cli` con escritura porque B no puede lanzar subagentes de Claude).

| Punto | Contenido |
|---|---|
| Hechos medidos | la clave `windows.sandbox` existe en `config.toml` (valor no leído; I-61 registró `unelevated` el 2026-09-30); servicio `codex-windows-sandbox-service` vivo; `~/.codex/.sandbox-bin/codex.exe` de 2026-09-12. **Medición previa de I-61 (sonda P2, 2026-09-30, `workspace-write` en una réplica de `%TEMP%`):** 4 rechazos del sandbox al crear procesos (incluido `dotnet --version`) y un commit fallido; además, la propia CLI añadió a `config.toml` una entrada `[projects.<ruta>] trust_level` (DEV-G1C-01). Desde entonces la app se actualizó (26.930.x); la capacidad no se ha vuelto a medir |
| Qué se autoriza | invocar `codex-cli` con `-s workspace-write` **solo** con `-C` dentro de `D:\r62-fixture\` y `AllowedWriteScope` del contrato: ≤ 2 sondas de escritura y el Worker de FX-04b (y de FX-03 si se desbloquea), ≤ 2 por escenario |
| Qué desbloquea | medir la escritura de Codex: FX-04b pasa de UNVERIFIED a PASS o a **UNSUPPORTED con causa medida** (resultado legítimo para la decisión del Owner, OV-I62-05 b); el Worker de FX-03 |
| Qué NO autoriza | cambiar `windows.sandbox`, activar el sandbox elevado o crear cualquier permiso del sandbox o del sistema; escribir fuera del fixture o en RackCad; aceptar un cambio de huella: si la CLI añade otra vez una entrada de confianza a `config.toml`, es **P-01 → STOP** y una solicitud nueva de OD-2 |
| Consumo, coste y tiempo | dentro de los topes de Codex de OD-2 (sondas de escritura ≤ 2; Worker ≤ 2 por escenario); misma cuota de suscripción |
| Seguridad | escritura acotada al worktree del fixture por el sandbox de Codex y por `AllowedWriteScope`; el push al fixture usa la autenticación de Git del Owner (con OD-7); riesgo conocido: reescritura de `config.toml` por la CLI, contenida por P-01 |
| Recomendación | **APROBAR (A, solo en el fixture).** Motivo: con coste acotado convierte la incógnita de FX-04b en un resultado medido. La medición de I-61 hace probable un UNSUPPORTED con el sandbox no elevado, y ese UNSUPPORTED con causa ya es la evidencia que el Freeze pide para la decisión del Owner. Sin OD-4 solo queda UNVERIFIED |
| Aprobar | `OD-4 = A (workspace-write solo en D:\r62-fixture)` |
| Rechazar | `OD-4 = RECHAZAR` — FX-04b y FX-03 UNVERIFIED por falta de OD; FX-04a no cambia |

## OD-3 — Autenticar la CLI de Claude

**Definición congelada** (§18): autenticar Claude CLI. **Bloquea:** `claude-cli` en la topología B (Reviewer y Architect de FX-03). Momento: antes de F6 B.
FX-06 puede usar `claude-cli` como alternativa a `codex-cli` para sus Architects.

| Punto | Contenido |
|---|---|
| Hechos medidos | `~/.local/bin/claude.exe` versión de archivo **2.1.270.0**, SHA-256 `FD7F35EC7761195A…`, de 2026-09-12: los mismos que en la preparación anterior; **no** está en el `PATH`, pero puede invocarse por ruta absoluta (no hace falta tocar el `PATH`). Estado de autenticación: **UNKNOWN**; observarlo exige ejecutar la CLI (no se hizo) y nunca se leen credenciales; el Discovery de I-62 la registró NOT_AUTHENTICATED |
| Qué se autoriza | que **el Owner** (nunca la sesión) autentique `claude.exe` 2.1.270 en su perfil; después, ≤ 2 sondas de medición (operaciones 2-9 del descriptor) y el Reviewer y el Architect de FX-03 (≤ 4 invocaciones) dentro del fixture |
| Qué desbloquea | `claude-cli` en FX-03 (que además necesita OD-2, OD-4 y la escritura de Codex); un Architect alternativo para FX-06 |
| Qué NO autoriza | que la sesión vea o lea credenciales; cambiar el `PATH`; usar `claude-cli` fuera del fixture o en un rol real de RackCad; superar los topes |
| Consumo, coste y tiempo | `claude-cli` ≤ 4 + 2 sondas en FX-03 (+ hasta 4 si FX-06 lo usara); cuota del plan de Claude del Owner; una autenticación interactiva hecha por el Owner (minutos) |
| Seguridad | la CLI guarda una credencial nueva en el perfil del usuario, que el protocolo nunca lee; corre sin sandbox del SO, con las herramientas que permita la receta (solo lectura para Reviewer y Architect) |
| Recomendación | **RECHAZAR por ahora**; antes era condicional. Motivo medido: FX-03 solo puede ser PASS si el Worker Codex escribe, y la medición de I-61 (P2) indica que esa escritura falla con el sandbox no elevado. Así, autenticar ahora añadiría una credencial con poco valor esperado. FX-06 no depende de OD-3 si se aprueba OD-2 (Architects `codex-cli`, proveedor distinto del de A, PREFERRED). FX-03 no se retira: queda UNVERIFIED y el Owner decide su limitación (OV-I62-04). **Revisión prevista:** si la sonda de OD-4 demuestra la escritura de Codex, la sesión volverá a pedir OD-3 con ese hecho |
| Aprobar | `OD-3 = A (el Owner autentica claude.exe 2.1.270)` |
| Rechazar | `OD-3 = RECHAZAR` — FX-03 UNVERIFIED (falta OD-3); FX-06 solo con `codex-cli` |

## OD-2b — Línea base nueva de `codex-cli` tras P-01 (2026-10-06T07:24Z; SUSTITUIDO por la disposición del Coordinator, decisiones §47)

> **Disposición del Coordinator (§47):** la opción A2 **no se adopta**: aceptar automáticamente un hash futuro según su delta es demasiado amplio para
> la frontera de OD-2. Se divide en **OD-2b-PROBE** (A, autorizada por el Owner; ejecutada:
> [`R20261006T150704Z-od2b`](../I-62-F6/OD-2b-PROBE/R20261006T150704Z-od2b/result.json)) y **OD-2c** (abajo). Esta sección se conserva como historial
> con dos correcciones medidas: su predicción de +2 secciones era falsa (las sondas `read-only` no crearon ninguna entrada) y, en la topología A, el
> Worker es `claude-subagent`, no `codex-cli` (receta de FX-02).

**Hecho nuevo medido** (sonda 1 de OD-4, [result.json](../I-62-F6/OD-4/R20261006T072306Z-od41/result.json)): la primera invocación de `codex-cli` en un
directorio nuevo **reescribe `~/.codex/config.toml`**: un segundo después de arrancar añadió una sección `[projects.<redactado>]` con una clave (+65 bytes;
valores no leídos), y la huella pasó de `091540ED…` a **`9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`**. Es la conducta que I-61
registró como DEV-G1C-01 y la causa, antes UNKNOWN, de los cambios de huella de la cronología. P-01 es literal (cualquier cambio del hash es STOP; el
descriptor fija la línea base solo por OD-2), así que: **STOP del transporte `codex-cli`**, la huella nueva no se acepta y no hay más invocaciones hasta
esta decisión. También cambia la huella un reinicio o una actualización de la app de Codex (proceso iniciado 2026-10-04T21:39:15Z, archivo escrito 21:39:14Z).

**Consecuencia para F6:** cada directorio nuevo en el que corra `codex-cli` añade su entrada. Para que ninguna cesión vea un cambio, los directorios de
trabajo de `codex-cli` deben fijarse antes y medirse fuera de las cesiones: `D:\r62-fixture\A` (Controller y Worker de la topología A; directorio de
trabajo del Principal A) y `D:\r62-fixture\arch` (clon limpio único de los Architects B y C de FX-06, en invocaciones distintas con su `thread_id`).

| Opción | Efecto | Riesgo |
|---|---|---|
| **A2** | autorizar ≤ 2 sondas previas de solo lectura (D.3, «sondas previas 1 +1») en `D:\r62-fixture\A` y `D:\r62-fixture\arch`, y aceptar como línea base el hash medido después **si el delta saneado frente a `9002E854…` es exactamente +2 secciones `[projects.<redactado>]` con una clave cada una y nada más**; si no, P-01 y otra decisión | si la app de Codex se reinicia o se actualiza antes de las cesiones, P-01 de nuevo |
| A | aceptar ahora `9002E854…` | la primera invocación en `A` o en `arch` lo cambiará: P-01 y otra decisión |
| C | no autorizar | `codex-cli` sigue en STOP: FX-02 y FX-06 UNVERIFIED |

- **Qué NO autoriza:** escribir `config.toml`, crear entradas de confianza a mano, cambiar `trust_level`, `windows.sandbox` o permisos; aceptar otro delta.
- **Consumo:** 2 sondas de solo lectura (dentro de las «sondas previas» de D.3); después, los topes de D.3.
- **Celdas de las sondas:** en `A`, la celda del Controller medida (`gpt-6-luna`/`high`); en `arch`, la del Architect `gpt-6.1-sol`/`high` (Deep), que
  no tiene invocación medida: esa sonda es también su medición de elegibilidad (routing §5). `gpt-6-luna` no sirve de Architect (Eficiente; Deep exige
  Equilibrado o Frontera).
- **Seguridad:** solo el hash y nombres saneados; nunca valores; la comprobación del delta se hace sobre nombres saneados.
- **Recomendación: A2.** Motivo: es la única opción conforme a P-01 que deja FX-02 y FX-06 sin una segunda pregunta, porque fija los directorios antes de
  las cesiones y acepta solo el delta exacto que la conducta medida produce.
- **Aprobar:** `OD-2b = A2 (sondas en D:\r62-fixture\A y D:\r62-fixture\arch; aceptar el hash resultante si el delta saneado es exactamente +2 secciones [projects.<redactado>] de una clave)`
- **Rechazar:** `OD-2b = RECHAZAR`

## OD-2c — Aceptación de la huella exacta de `codex-cli` (2026-10-06T15:10Z; DECIDIDA: A, decisiones §48)

Paquete pedido por el Coordinator (decisiones §47) tras las dos sondas de OD-2b-PROBE
([result.json](../I-62-F6/OD-2b-PROBE/R20261006T150704Z-od2b/result.json)). `codex-cli` sigue en **STOP P-01** hasta esta decisión: ninguna invocación más.

| Campo | Valor medido |
|---|---|
| Huella final de `~/.codex/config.toml` (SHA-256) | **`9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`**: 4 775 bytes; `LastWriteTimeUtc` 2026-10-06T07:23:07.616Z, sin cambios desde la sonda de OD-4 |
| Huella anterior aceptada (OD-2 = A) | `091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66` (4 710 bytes) |
| Delta estructural saneado desde la aceptada | +1 sección `[projects.<ruta>]` con 1 clave; nada eliminado (105 → 107 nombres saneados) |
| Secciones de proyecto añadidas (exactas) | `[projects.'d:\r62-fixture\probe-od4-1']` con la clave `trust_level` (valor no leído). La escribió la sonda 1 de OD-4 (`workspace-write`, 2026-10-06T07:23:07Z). Las sondas de OD-2b no añadieron ninguna |
| ¿Cambió algo más? | **No, byte a byte.** El archivo final, sin esa sección y su línea en blanco, tiene exactamente el SHA-256 `091540ED…` (`cfg_projects.py`, sin publicar valores) |
| Huella antes y después de cada paso | `--version` y `login status`: `9002E854…` → `9002E854…`; sonda 1 (`A`): `9002E854…` → `9002E854…`; sonda 2 (`arch`): `9002E854…` → `9002E854…`; deltas saneados vacíos |
| Binario | `%LOCALAPPDATA%\OpenAI\Codex\bin\8aaf1547b825b104\codex.exe`, SHA-256 `37762753b554982eef1c109303d1be652b6397f1479e844794353a85650199c6`, `codex-cli 0.160.0` (igual que el `cli_version` del registro de sesión); único `codex.exe` bajo `bin\`, escrito el 2026-10-03 |
| Runtime observado (RUNTIME_OBSERVED, `turn_context`) | sonda 1: `gpt-6-luna`/`high`, `read-only`, `approval_policy` never, proveedor `openai`, cwd `D:\r62-fixture\A`; sonda 2: `gpt-6.1-sol`/`high`, `read-only`, cwd `D:\r62-fixture\arch`, primera invocación medida de la celda del Architect (ejecuta y devuelve salida estructurada válida) |
| Autenticación | `Logged in using ChatGPT` (`codex login status`; la sesión no lee `auth.json`) |
| Estabilidad medida | `read-only` no crea entradas de proyecto (2 de 2); `workspace-write` sí (2 de 2: DEV-G1C-01 de I-61 y la sonda de OD-4). La receta 16.4 de `codex-cli` es `read-only` (Controller y Architects de FX-02 y FX-06): esta huella es estable para esas cesiones. La cambiarían un reinicio o una actualización de la app de Codex, o un Worker `codex-cli` en `workspace-write` en un directorio nuevo (FX-03 y FX-04b, hoy UNVERIFIED y UNSUPPORTED) |
| Qué NO autoriza | editar `config.toml`; cambiar `trust_level`, `windows.sandbox`, credenciales o permisos; aceptar otro hash; ejecutar `codex-cli` en `workspace-write` |
| Recomendación | **ACEPTAR `9002E854…` exacto.** Motivo: el único cambio desde la línea base aceptada es la entrada de directorio medida de la sonda de OD-4, demostrado byte a byte |
| Aprobar | `OD-2c = A (línea base 9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E)` |
| Rechazar | `OD-2c = RECHAZAR` — `codex-cli` sigue en STOP: FX-02 y FX-06 UNVERIFIED |

## OD-2d (vigente) — Línea base exacta de `codex-cli` sobre la instalación actual de Codex (2026-10-07T06:19Z; pendiente del Owner)

Medición pasiva ([result.json](../I-62-F6/OD-2/R20261007T061915Z-od2d-night-passive/result.json)); `codex-cli` sigue en **P-01 / STOP**.

| Campo | Valor |
|---|---|
| `config.toml` (SHA-256) | **`9EA26634078B314A72A815B37CDD17657D7D0406E4911FC4CC4FEC42845153C3`** (4 777 bytes; escrito 03:29:05Z; estable a las 03:40Z y 06:19Z) |
| Binario | `%LOCALAPPDATA%\OpenAI\Codex\bin\979a96ce184041d1\codex.exe`, SHA-256 **`97c57e4eb64257bcd7a470757950886f2c59eec4aa8537908979c6475d41cc08`** (único) |
| App | `OpenAI.Codex 26.1002.6548.0` |
| Claves cambiadas (frente a `723A6898…`, comparación por clave) | `mcp_servers.node_repl.command`, `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION`, `.CODEX_CLI_PATH`, `.NODE_REPL_NODE_MODULE_DIRS`, `.NODE_REPL_NODE_PATH`, `.NODE_REPL_TRUSTED_CODE_PATHS`, `.NODE_REPL_TRUSTED_SERVICES`, `.SKY_CUA_NATIVE_PIPE_DIRECTORY`, `notify`; estructura (107 nombres, 39 secciones) sin cambios |
| Impacto de seguridad | claves de integración de la app (servidor MCP `node_repl` y comando `notify`); ninguna de modelo, effort, sandbox, aprobación, `service_tier` ni entradas de proyecto o `trust_level`; la receta de solo lectura no depende de ellas |
| Sin medir | versión de la CLI, autenticación, runtime observado y estabilidad en sondas (exigen ejecutar este binario) |

- **Recomendación:** autorizar una medición controlada de esta instalación y aceptar la huella exacta solo si no cambia durante ella. La app se ha
  actualizado sola dos veces en unas 9 horas; si el Owner quiere evitar más P-01 durante los pilotos, puede pausar las actualizaciones de la app (decisión
  suya; la sesión no toca esa configuración).
- **Secuencia (corrección de la ronda 2, decisiones §53 y §54):** dos actos separados y en este orden; ninguno se pide antes de que el Coordinator
  clasifique FX-04a con B2.
  1. **OD-2d-PROBE** (en la ronda de disposiciones del Coordinator, tarjeta de la mañana, acción 2):
     `OD-2d-PROBE = A (--version, login status y ≤ 2 sondas read-only en <directorios que fije la disposición; por defecto D:\r62-fixture\A y D:\r62-fixture\arch>; huella, binario, versión de la app y comparación por clave antes y después de cada operación; sin workspace-write ni cambios de config.toml, trust_level o sandbox)`.
     El tope de sondas (FX-06 OQ-21, FX-02 OQ-08) y la medición `codex sandbox` (P1b, FX-06 OQ-22) se deciden en la misma ronda.
  2. **OD-2d** (tarjeta, acción 3), sobre el paquete que la supervisión publique tras la medición:
     `OD-2d = A (línea base <huella medida>; binario <SHA-256 medido>)`. Si la medición no cambia nada, los valores son los de esta tabla (`9EA26634…`,
     `97c57e4e…`).
- ~~Aprobar (una línea, condicionada): `OD-2d = A (línea base 9EA26634…; binario 97c57e4e…; efectiva solo tras OD-2d-PROBE …)`~~ **[retirada en la ronda
  2: autorizaba la sonda y aceptaba el resultado de antemano]**
- **Rechazar:** `OD-2d = RECHAZAR` — FX-02 y FX-06 siguen UNVERIFIED.

## OD-2d — Línea base exacta de `codex-cli` tras la actualización de la app de Codex (2026-10-07; OBSOLETA antes de decidirse: segunda actualización de la app)

> **Obsoleta (2026-10-07T03:40Z, evidencia §71):** la app de Codex se actualizó otra vez (`26.1002.6548.0`; binario `979a96ce184041d1`,
> `97c57e4eb64257bcd7a470757950886f2c59eec4aa8537908979c6475d41cc08`) y `config.toml` pasó a `9EA26634078B314A72A815B37CDD17657D7D0406E4911FC4CC4FEC42845153C3`
> (9 claves de `mcp_servers.node_repl` y `notify`, localizadas por la comparación por clave). No responder a la propuesta de abajo: hará falta una medición
> nueva (con autorización) y un paquete con la huella vigente, mejor después de abrir B, que puede volver a cambiarla.

> **Medición completa (OD-2d-PROBE = A, §51;** [result.json](../I-62-F6/OD-2/R20261007T013200Z-od2d-probe/result.json)**):** huella
> **`723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8`** estable en las seis mediciones (antes y después de `--version`, `login status` y
> las dos sondas de solo lectura), con la comparación por clave sin cambios; binario **`%LOCALAPPDATA%\OpenAI\Codex\bin\5ea220ae823df3d7\codex.exe`**, SHA-256
> **`3b8f6e33caa75f232558a3cf76ff9b87bb5ef6dbcf4996372f24e55c78b1b91`**, `codex-cli 0.160.1`; `Logged in using ChatGPT`; app `26.930.7945.0`; sondas con
> `gpt-6-luna`/`high` y `gpt-6.1-sol`/`high`, `read-only`, sin aviso de límite. Lo que sigue sin poder probarse sin valores: qué valores cambió la
> actualización respecto de `9002E854…`. **Recomendación: ACEPTAR esa huella y ese binario exactos**; el Owner puede revisar antes los valores que quiera en
> su archivo. Respuesta: ~~`OD-2d = A (línea base 723A6898…; binario 3b8f6e33…)`~~ **[retirada: obsoleta]**.

> **Disposición del Coordinator (§50):** no se pide OD-2d hasta medir de forma controlada el binario y el runtime nuevos. Medición pasiva hecha
> ([result.json](../I-62-F6/OD-2/R20261006T200612Z-od2d-passive/result.json)): huella estable desde las 19:18Z, app `26.930.7945.0`, binario `3b8f6e33…`.
> Falta lo que exige ejecutar el binario nuevo (versión, autenticación, modelo, effort y sandbox observados, estabilidad en sondas de solo lectura):
> **OD-2d-PROBE** (ver al final de esta sección). Las opciones de abajo quedan en suspenso hasta esa medición.

**Hecho (P-01, [result.json](../I-62-F6/OD-2/R20261006T191621Z-p01/result.json)):** sin ninguna invocación de Codex de la sesión, la huella pasó de la
línea base de OD-2c `9002E854…` a **`723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8`** (escrita a las 18:58:51Z). Coincide con una
actualización de la app (paquete `26.930.3930.0` → `26.930.7945.0`) que sustituyó el binario: el aceptado (`bin\8aaf1547b825b104`, `37762753…`)
ya no existe. **STOP del transporte `codex-cli`**; FX-01 y FX-04a no lo usan y siguen.

| Campo | Valor medido |
|---|---|
| Huella nueva (SHA-256) | `723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8` (4 775 bytes) |
| Huella anterior aceptada (OD-2c) | `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E` |
| Delta estructural saneado | ninguno: los mismos 107 nombres de clave y 39 secciones, misma longitud; cambian **valores** |
| Dónde cambian (solo nombres de clave) | `mcp_servers.node_repl.env.CODEX_CLI_PATH` lleva la etiqueta del binario nuevo; la versión de la app está en `mcp_servers.node_repl.env.BROWSER_USE_CODEX_APP_VERSION` y `mcp_servers.node_repl.env.NODE_REPL_TRUSTED_SERVICES`. Revertir solo la etiqueta del binario no reproduce `9002E854…`: cambió al menos otro valor |
| ¿Solo cambios de la actualización? | **No demostrable sin valores:** el archivo anterior nunca se guardó (solo su hash y nombres saneados). Desde ahora, una instantánea local con digests por clave localiza cambios futuros por nombre de clave sin guardar valores |
| Binario nuevo | `%LOCALAPPDATA%\OpenAI\Codex\bin\5ea220ae823df3d7\codex.exe`, SHA-256 `3b8f6e33caa75f232558a3cf76ff9b87bb5ef6dbcf4996372f24e55c78b1b91`; versión y autenticación UNKNOWN (no se ejecuta en STOP; el ejecutable no tiene metadatos de versión) |
| Qué NO autoriza | editar `config.toml`; cambiar `trust_level`, `windows.sandbox`, credenciales o permisos; aceptar otro hash |

| Opción | Efecto |
|---|---|
| **A** | aceptar `723A6898…` exacto y el binario `3b8f6e33…`; antes de la primera invocación de modelo, la sesión mide `codex --version` y `codex login status` (sin modelo) con la huella antes y después |
| B | antes de decidir, el Owner revisa él mismo en su archivo los valores que le importen (p. ej., `windows.sandbox`, `model`, `model_reasoning_effort`, `service_tier`); la sesión no los lee ni los publica |
| C | no aceptar: `codex-cli` sigue en STOP; FX-02 y FX-06 UNVERIFIED |

- **Recomendación: B y después A.** La estructura no cambia y el cambio coincide con una actualización de la app, pero no se puede probar, sin valores, que
  solo cambiaran valores ligados a ella. FX-02 y FX-06 necesitan `codex-cli`; FX-01 y FX-04a no.
- **Aprobar:** ~~`OD-2d = A (línea base 723A6898…; binario 3b8f6e33…)`~~ **[retirada: obsoleta]**
- **Rechazar:** `OD-2d = RECHAZAR`

**OD-2d-PROBE (autorización pedida; necesaria para completar la medición del §50):** `codex --version` y `codex login status` del binario nuevo, y
≤ 2 sondas de solo lectura (en `D:\r62-fixture\A` con la celda del Controller y en `D:\r62-fixture\arch` con la del Architect, que así se vuelve a medir), con la huella y
la comparación por clave antes y después de cada paso. No acepta ninguna huella ni permite editar `config.toml`, `trust_level`, `windows.sandbox` o
credenciales, ni empezar FX-02 o FX-06. Línea: ~~`OD-2d-PROBE = A (…)`~~ **[concedida y ejecutada sobre el binario `3b8f6e33…` (§51, evidencia §70); no reutilizable]**

## OD-3 (reconsideración, 2026-10-08) — el Owner informa que autenticó `claude-cli`

**Hecho declarado por el Owner** (en la conversación, 2026-10-08): «Ya autentifiqué Claude CLI». La sesión no lo ha medido: no ejecuta `claude-cli`
mientras OD-3 conste RECHAZADA (decisiones §46; orden del Coordinator «no usar Claude CLI con OD-3 rechazada») y nunca lee credenciales.

| Punto | Contenido |
|---|---|
| Estado formal | OD-3 = RECHAZAR (decisiones §46) hasta que el Owner decida otra cosa con una línea explícita. Autenticar la CLI no cambia por sí solo la decisión registrada |
| Qué cubre OD-3 congelada (V14 §18 y D.3) | `claude-cli` **solo en el fixture**: ≤ 2 sondas de medición de la fila «Sondas previas» de la Topología B (`claude-cli` autenticado 1 (+1), sin usar); Reviewer y Architect de FX-03 (≤ 4); Architects de FX-06 como alternativa a `codex-cli` (D.8, ≤ 4) si el Coordinator lo dispone. La huella de `claude-cli` la acepta el Owner por OD-2 tras la medición (D.7: «según su huella») |
| Qué desbloquearía hoy | la medición de `claude-cli` (versión, autenticación, modelo y effort observados, receta de solo lectura); la vía de FX-06 con Architects `claude-cli`, sin depender de `codex-cli` ni de A-2 para el Architect (FX-06 opción A sigue esperando al QH de FX-02, que necesita el Controller `codex-cli`) |
| Qué no desbloquea | FX-03 sigue UNSUPPORTED (el Worker Codex no puede hacer commit); FX-02 sigue necesitando `codex-cli` (Controller); la re-revisión de A-2 es del plano real de I-62 y **no** la cubre OD-3: usar `claude-cli` como transporte del Architect de A-2 exige una autorización aparte del Owner y del Coordinator, y que el Coordinator confirme la independencia (OD-6 alternativa 1: Actor, Session y Context; Provider PREFERRED) |
| Qué NO autoriza | que la sesión vea o lea credenciales; cambiar el `PATH`; usar `claude-cli` en un rol real de RackCad sin la autorización aparte; superar los topes |
| Aprobar (una línea) | `OD-3 = A (el Owner autenticó claude-cli; alcance: solo el fixture — ≤ 2 sondas de la fila «Sondas previas» de la Topología B, Reviewer y Architect de FX-03 y Architects de FX-06 si el Coordinator lo dispone; la huella de claude-cli se acepta por OD-2 tras la medición)` |
| Mantener | `OD-3 = RECHAZAR` — la sesión no usa `claude-cli`; el Owner puede cerrar la sesión de la CLI cuando quiera |

## OD-3 — hecho nuevo de OD-4 (2026-10-06T07:24Z; OD-3 sigue RECHAZADA)

La sonda de OD-4 midió que el Worker `codex-cli` en `workspace-write` **no puede hacer commit** (el sandbox deniega `.git/index.lock`). FX-03 necesita ese
Worker, así que con OD-3 aprobada FX-03 quedaría UNSUPPORTED por la misma causa. El hecho nuevo **no** hace útil reconsiderar OD-3: refuerza el rechazo.
No se autentica `claude-cli` ni se decide nada.

## Recordatorios (sin solicitud)

- **OD-6:** decidida (alternativa 1, decisiones §30); no se revisa.
- **OD-1:** antes de READY-03 (aceptación de ADR-0048 y deltas OWNER-RESERVED); no bloquea F6 y no se pide todavía. **Preparación (2026-10-06; no
  aceptada, no solicitada):** ADR-0048 sigue «propuesto», blob `e1bd8d91` en `MC_I62` (`6f0187cb`). Enuncia la orquestación a nivel de principio (V14
  §20) y no cita A-1; A-1 §6 declara que no cambia ninguna decisión del Owner (OD-1..OD-7), así que el ADR puede presentarse tal cual. **Aviso de
  momento:** `docs/adr/` está en la lista cerrada de superficies de 16.13; cualquier edición de ADR-0048 antes de la integración (p. ej., para citar A-1)
  invalida `MC_I62` (C-21), obliga a resembrar el fixture y a repetir los pilotos afectados. Si el Coordinator quiere esa edición, el momento más barato
  es antes de ejecutar los pilotos de F6. **Disposición del Coordinator (decisiones §47): ADR-0048 = NO CHANGE BEFORE F6 PILOTS**; la relación factual
  con A-1 puede documentarse más tarde en el material de cierre o de historial, sin reescribir el `MC_I62` vigente.
- **DEP-F4-YAML:** no hizo falta (F4 cerrado sin dependencia nueva).

---

## Historial — revisión del 2026-10-04 (conservada; sustituida por la de arriba)

| Decisión | Recomendación de entonces | Respuesta en una línea | Desbloquea | Si no |
|---|---|---|---|---|
| OD-2 | **B** | `OD-2 = B` (medir justo antes de la primera invocación de `codex-cli` y aceptar ese valor) | sondas y FX-02, FX-03, FX-04b, FX-06 por `codex-cli`; observación completa de C-06 | esos escenarios UNVERIFIED; `codex-cli` sigue UNKNOWN/NOT_ELIGIBLE |
| OD-3 | según el uso deseado de la topología B | `OD-3 = A` o `OD-3 = B` | FX-03; FX-06 por `claude-cli`; un transporte limpio de Architect sin clic (Track H) | FX-03 UNVERIFIED |
| OD-4 | **A** solo en el fixture | `OD-4 = A` | Worker Codex de FX-03 y FX-04b | FX-03/FX-04b UNVERIFIED o UNSUPPORTED |
| OD-5 | **A** | `OD-5 = A` | todo F6, empezando por FX-04a (listo para un comando) | F6 no se ejecuta |
| OD-7 | **A** | `OD-7 = A` (repositorio privado del fixture con CI) | cierre de F6 (FX-02), FX-04b, FX-06 paso 5 | F6 no cierra |

**Medición pasiva del 2026-10-04 (MEASURED, sin ejecutar ningún binario):**

| Dato | Valor |
|---|---|
| `~/.codex/config.toml`, SHA-256 | `091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66` |
| `LastWriteTimeUtc` / tamaño | 2026-10-04T21:39:14Z / 4 710 bytes |
| nombres de secciones y claves | 110, de ellos 20 saneados (`[projects.<redactado>]`); antes, 103 y 22: la estructura cambió |
| secciones de primer nivel | `desktop`, `features`, `marketplaces`, `mcp_servers`, `model`, `model_reasoning_effort`, `notify`, `plugins`, `projects`, `service_tier`, `shell_environment_policy`, `tui`, `windows` |
| paquete de la app | `OpenAI.Codex_26.930.3930.0` (antes `26.930.2377.0`) |
| binarios `codex.exe` (sin ejecutar) | `…\app\resources\codex.exe` (paquete actual), SHA-256 `37762753B554982E…`, idéntico a `~/.codex/plugins/.plugin-appserver/codex.exe`; `~/.codex/.sandbox-bin/codex.exe`, SHA-256 `081E4DE4BE8E38FA…`, de 2026-09-12. La ruta antes verificada (`bin/a51e250fa15c740a`, `0.159.2`) ya no aparece |
| `codex` en el `PATH` | no |

**Cronología de la huella (con la medición del 2026-10-06):**

| SHA-256 | Momento | Causa del cambio desde la anterior |
|---|---|---|
| `37DD3559…` | registrada por I-61 tras el retiro autorizado | — |
| `42E15A03…` | G0 de I-62 | UNKNOWN |
| `40c27b57…` | revisiones del Architect de I-62, hasta ~18:45Z del 2026-10-02 | UNKNOWN |
| `155933B3…` | línea base aceptada por el Owner para I-63, 21:42Z del 2026-10-02 | KNOWN: actualización y reinicio de la app (21:31Z) |
| `091540ED…` | observada el 2026-10-04 (escrita a las 21:39Z); **sin cambios el 2026-10-06T02:26Z** | UNKNOWN; coincide con una actualización de la app (2377 → 3930), sin demostrar la relación |

**OD-3 en 2026-10-04:** A si el Owner quería la topología B y un transporte de Architect sin clic (Track H); si no, B. Opción A de entonces: «el Owner
instala `claude` en el `PATH` y lo autentica» (sustituida: no hace falta tocar el `PATH`). **OD-4 en 2026-10-04:** A, `workspace-write` solo en el
fixture. **OD-5 en 2026-10-04:** A. **OD-7 en 2026-10-04:** A, repositorio privado del Owner, sin secretos, solo FX-U1. **DEP-F4-YAML:** no hacía falta
(prototipo `f4/yaml-subset/`, 27/27 casos).

## OD-2-MAT (2026-10-09; PENDIENTE) — ¿línea base material de la huella de `codex-cli`?

Paquete completo, con las cuatro respuestas literales y la sección «Lectura de valores (no se pide ahora)»:
[I-62-A3/od2-material-baseline-owner-packet.md](../I-62-A3/od2-material-baseline-owner-packet.md) (blob `b2c8ec8a`, publicado con la candidata
A-3 en `91e29886`). Se pide dentro de la solicitud única de recuperación (§5). Es materia reservada al Owner: no se resuelve por disposición del
Coordinator. Mientras no se decida, OD-2 sigue siendo el SHA-256 exacto y P-01 no cambia.

## OD-2e — Línea base exacta de `codex-cli` tras el bloque A2-P2 (2026-10-09; DECIDIDA: A, decisiones §59)

Lo pide la disposición del Coordinator, decisiones §58, punto 2: «Tras las sondas, solicitar OD-2 exacta al Owner antes de trabajo ordinario de
codex-cli». Sustituye a las solicitudes OD-2d, que quedaron obsoletas por las actualizaciones automáticas de la app. Medición:
[result.json](../I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json).

| Punto | Contenido |
|---|---|
| Hechos medidos | **Binario** `3553cd6e…`, el único `codex.exe`, CLI `codex-cli 0.162.0-alpha.2`. **App** `26.1002.7124.0`. **Autenticación:** «Logged in using ChatGPT». **`config.toml`:** SHA-256 **`6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32`** con los mismos 107 nombres saneados. Es estable desde el 2026-10-07T20:05:22Z y no cambió en ninguna de las seis mediciones del bloque (antes y después de cada operación). No se leyeron valores ni se calcularon digests por clave |
| Sondas (A2-P2; 2 de 2) | **Sonda 1:** Controller `gpt-6-luna/high`, read-only, en `D:\r62-fixture\A`. Completó 8/8 pasos con resultados correctos, pero solo recurriendo a `cmd.exe`. **Sonda 2:** Architect `gpt-6.1-sol/high`, read-only, en `D:\r62-fixture\arch`. Completó 0/6 pasos: ningún comando llegó a ejecutarse. En las dos sondas, los clones quedaron sin cambios |
| Hallazgo (F6-OBS-03, propuesto) | La CLI 0.162.0-alpha.2 lanza los comandos con el alias de aplicación `WindowsApps\pwsh.exe`, aunque la receta 16.4 pone primero en el `PATH` el `pwsh` del runtime. Con `-s read-only`, `CreateProcessAsUserW` falla con «Acceso denegado». Con la CLI 0.160.1, la misma receta ejecutaba los comandos (OD-2d-PROBE). Es un cambio de comportamiento del runtime en una propiedad material. **La huella no cambió** |
| Qué se autoriza | Fijar como línea base de la huella de `codex-cli` el SHA-256 exacto `6518EFAB…`, con sus nombres saneados, para el binario `3553cd6e…` |
| Qué desbloquea | Solo levanta el STOP P-01 por la huella. **No hace operativo `codex-cli`:** con la regresión de shell, la celda del Architect no ejecuta comandos, y la del Controller depende de un rodeo que no está medido. El trabajo ordinario de `codex-cli` necesita además la disposición del Coordinator sobre F6-OBS-03 |
| Qué NO autoriza | Aceptar otra huella o un binario distinto. Leer valores o cambiar `config.toml`, `trust_level` o el sandbox. Invocar `codex-cli` fuera de lo que disponga el Coordinator. Si la huella cambia, vuelve P-01 con una OD-2 nueva (OD-2-MAT = A) |
| Consumo | La respuesta en sí no consume nada. El bloque ya consumió sus 2 sondas |
| Recomendación de preparación (etiquetada; no es decisión) | **A.** La huella es exacta, lleva unas 32 h estable y no cambió durante el bloque. Aceptarla no amplía permisos. Su efecto operativo depende de la disposición sobre F6-OBS-03 |
| Aprobar | `OD-2 = A (línea base 6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32; binario 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68)` |
| Rechazar | `OD-2 = RECHAZAR`. `codex-cli` sigue en STOP P-01. FX-02 y FX-06 tendrían que resolverse con otros transportes o quedar UNVERIFIED con causa |
