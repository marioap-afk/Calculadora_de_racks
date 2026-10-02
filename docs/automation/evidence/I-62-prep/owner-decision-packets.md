# I-62 — Paquetes de decisión del Owner (preparados; ninguno se solicita todavía)

> **Preparación.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33. Ninguna decisión se pide hasta que el trabajo autorizado llegue a su
> frontera exacta. A 2026-10-02, **ninguna** de estas fronteras bloquea F2. Cada paquete termina con una selección corta. SHA de referencia:
> `1eddbf48dbefd9685e68d63c0560bacf484d2dcb` (F2). Fuente: Proposal V14 §18 y Anexo D (blob `34ad80ea`).

## OD-2 — Línea base de huella de `codex-cli` para invocarlo en una unidad I62

- **Por qué:** V14 §7 y P-01/P-11. Toda invocación de `codex-cli` compara la huella de `~/.codex/config.toml` antes y después de la cesión. Invocarlo puede
  reescribir el archivo (DEV-G1C-01 de I-61), y su huella cambió por actualizaciones de la app. Sin una línea base aceptada para I-62, la primera invocación
  afectada no tiene referencia.
- **Qué bloquea exactamente:** la primera invocación de `codex-cli` en el plano de I-62:
  - las sondas de F6 (D.3, «Sondas previas … con OD-2»);
  - FX-02, FX-03, FX-04b y FX-06 por `codex-cli`;
  - la observación de C-06 que exige invocar: `CliVersion`, `AuthState` y el effort y el nivel efectivos.

  **No** bloquea el cierre de F2 (V14 §17: «OD-2 solo para medir **invocando** `codex-cli`»).
- **Adapter:** `codex-cli`. Descriptor `docs/automation/agent-execution/adapters/codex-cli.md`, blob `155f3469e345e2397fa26347a9cb52d6fdb91122`; esquema de hechos
  blob `3d1b7478b433ebff2f882fb3edae82aff970d151`; preflight de C-06 `P20261002T224500Z-f206`, blob `c48d1b29080e5e07c7c70c3fb854721803bd607f`.
- **Huella observada ahora** (MEASURED, sin ejecutar ningún binario):
  - `~/.codex/config.toml` tiene SHA-256 `155933b32e5178001700b1137a58435075c13aa0179ed9f530791f3f899726d7`, con `LastWriteTimeUtc` 2026-10-02T21:34:25Z;
  - tiene 103 nombres de secciones y claves, 22 de ellos saneados (rutas de proyectos).
- **Huellas de referencia:**

  | Valor | Origen |
  |---|---|
  | `37DD3559E89681B253D8541AD2BFE43B452192E9AB2CD6F66C7503BE186E1E2D` | referencia registrada por I-61 tras el retiro autorizado (decisiones de I-61 §17; Discovery de I-62) |
  | `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5` | observación del host en el G0 de I-62 (Discovery) |
  | `40c27b570b0056bc6d2b5aaf460628922c5ad39a405751e68b9001ffdf15f74f` | la que vieron todas las revisiones del Architect de I-62, sin cambio, hasta ~18:45Z del 2026-10-02 |
  | `155933B3…` | **línea base aceptada por el Owner para I-63** a las 21:42Z, tras la actualización y el reinicio de la app Codex (evidencia de I-63 §45; paquete `OpenAI.Codex_26.930.2377.0`) |
- **Causa de las discrepancias:**
  - de `40c27b57` a `155933b3`: KNOWN. La app Codex se actualizó y se reinició a las 21:31Z (I-63 §45);
  - de `37DD3559` y `42E15A03` a `40c27b57`: **UNKNOWN**. No hay diff de valores (solo se registran nombres), y la app escribe el archivo al actualizarse.
- **Binarios:**
  - la ruta verificada `bin/a51e250fa15c740a` tiene SHA-256 `fcd5eafe…` (`codex-cli 0.159.2`);
  - la actualización instaló `bin/be3fd7e5c1969ff6`, SHA-256 `1722907a…` (`0.159.0-alpha.12.1` según I-63), que no está medida;
  - la receta elige el binario por ruta verificada, nunca el más reciente.
- **Invocación propuesta** (la de la receta del descriptor, sin cambios):
  - binario `…/bin/a51e250fa15c740a/codex.exe` por ruta verificada, con `exec -C <clon o worktree>` y `-s read-only`;
  - `-m <modelo de una celda elegible> -c model_reasoning_effort=<effort>`;
  - `--output-schema`, `-o <dir del RunId>` y `--json`, sin `--ephemeral`;
  - stdin cerrado y el `pwsh` del runtime primero en el `PATH`.
- **Consumo:** suscripción existente con autenticación por ChatGPT; sin claves de API de pago. Lanzamientos: sondas ≤ 2, y luego los topes de D.3 por
  ronda.
- **Sandbox:** solo lectura. La escritura es OD-4.
- **Impacto de credenciales:** ninguno; no se leen ni se escriben credenciales.
- **Impacto de configuración:** la invocación puede reescribir `config.toml`, y un cambio durante la cesión es STOP P-01.
- **Seguridad:** ninguna clave en el repositorio; la huella solo registra el hash y los nombres saneados.
- **Opciones:**
  - **A:** aceptar `155933B3…` (la misma que I-63) como línea base de `codex-cli` para I-62. Cada cesión compara la salida con ella y la entrada con la
    salida; cualquier diferencia es STOP P-01;
  - **B:** pedir una medición nueva justo antes de la primera invocación y aceptar el valor observado entonces;
  - **C:** no autorizar ahora.
- **Si NO:** FX-02, FX-03, FX-04b y FX-06 por `codex-cli` quedan UNVERIFIED; F6 no puede cerrarse por FX-02. FX-01, FX-04a y FX-05 no dependen de OD-2.
- **Si se aplaza:** nada cambia hasta F6; la huella puede volver a cambiar (la app se actualiza sola), así que la opción A podría quedar obsoleta antes de
  usarse, y la B no.
- **Respaldo UNVERIFIED:** sin OD-2, la observación de `codex-cli` sigue UNKNOWN/NOT_ELIGIBLE (P-10), como en C-06, y nada se invoca.

**Selección:** `OD-2 = A` | `OD-2 = B` | `OD-2 = C`

## OD-3 — Autenticar la CLI de Claude

- **Por qué:** `claude-cli` está NOT_AUTHENTICATED (Discovery) y no está en el `PATH` (MEASURED en F2). Es el Reviewer y el Architect de la topología B.
- **Qué bloquea:** FX-03 (topología B); la alternativa de FX-06 con `claude-cli`.
- **Rutas:** `docs/automation/agent-execution/adapters/claude-cli.md` (blob `ae570380509cacabf11f757a27573bf6fe616a36`); preflight de C-06 `…-f205` (blob
  `9cfb6fe70d95c8e4f6e6c6fdd172095fc28e60d2`).
- **Seguridad, credenciales y configuración:** la instalación y la autenticación las hace el Owner, y la sesión nunca ve credenciales. Añade un archivo de
  credenciales en el perfil del usuario, que el protocolo nunca lee; la huella de configuración del adapter se demostraría después.
- **Consumo:** el del plan con el que se autentique; sondas ≤ 2.
- **Opciones:** **A** autenticar antes de F6 B; **B** no autenticar: la topología B queda UNVERIFIED, y el Owner decide la limitación (OV-I62-04).
- **Si NO:** FX-03 UNVERIFIED; no afecta a FX-01, FX-02, FX-04a, FX-05 ni a FX-06 por `codex-cli`.
- **Si se aplaza:** sin efecto hasta F6.

**Selección:** `OD-3 = A` | `OD-3 = B`

## OD-4 — Sandbox de `codex-cli` con escritura (Worker Codex)

- **Por qué:** un Worker Codex con escritura, commit y push no está demostrado (UNKNOWN; I-61), y en una réplica el sandbox `workspace-write` rechazó lanzar
  procesos.
- **Qué bloquea:** los Workers Codex de FX-03 y FX-04b.
- **Rutas:** descriptor de `codex-cli` (operación 4: «Escritura (WORKER) no demostrada: OD-4»).
- **Seguridad:** escritura en el worktree del **fixture**, nunca en el de RackCad; los alcances de F3 (`AllowedWriteScope`) limitan la escritura.
- **Configuración:** puede requerir cambiar el modo de sandbox en la invocación, sin cambiar `config.toml`; si lo cambiara, sería P-01.
- **Credenciales:** el push al remoto del fixture usa la autenticación de Git del Owner (OD-7).
- **Consumo:** Worker ≤ 2 por escenario.
- **Opciones:** **A** autorizar `workspace-write` para el Worker Codex solo en el fixture; **B** no autorizar: FX-04b y FX-03 quedan UNVERIFIED o UNSUPPORTED
  y el Owner decide la limitación.
- **Si NO:** FX-04b y FX-03 sin continuación; FX-04a no cambia (D.4).

**Selección:** `OD-4 = A` | `OD-4 = B`

## OD-5 — Permiso de ensayo (semántica única de §15) y apertura de sesiones del sistema bajo prueba

- **Por qué:** los ensayos de F6 son pruebas del plano (c) en otro repositorio con sus propias autoridades. El Owner autoriza el consumo, la apertura de
  sesiones (Principal A, Principal B, B2) y las salvaguardas del host: las comprobaciones de 16.4 sobre el worktree del fixture.
- **Qué bloquea:** **todo F6**: FX-01..FX-06, incluido FX-04a.
- **Rutas:** ninguna de RackCad. El fixture vive en otro repositorio (D.1).
- **Seguridad:** P-16 impide que el plano (c) actúe sobre el plano (a), y FX-05 lo comprueba.
- **Configuración y credenciales:** las sesiones abiertas por el Owner usan su autenticación; la memoria de proyecto de un directorio nuevo se comprueba vacía
  (D.6).
- **Consumo:** los topes de D.3 (§4 de [f6-dossier.md](f6-dossier.md)).
- **Opciones:** **A** aceptar la semántica única de §15 y autorizar F6 con los topes de D.3; **B** no aceptarla: F6 no procede, y cualquier otra vía exige
  una A-n (§15: «No se congelan dos semánticas»).
- **Si NO:** F6 no se ejecuta; los criterios 9, 12 y 15 del mandato quedan sin demostrar, y el Owner decide sobre OV-I62-03..06.
- **Si se aplaza:** F6 espera; F3 y F4 no dependen de OD-5.

**Selección:** `OD-5 = A` | `OD-5 = B`

## OD-7 — Remoto del fixture con CI

- **Por qué:** sin CI del fixture, `Ci` = `not_run` y ninguna verificación puede ser VERIFIED (D.2).
- **Qué bloquea:** **el cierre de F6** (FX-02), FX-04b y FX-06 (paso 5).
- **Rutas:** un repositorio nuevo del Owner en GitHub con CI (`fixture-build`, `fixture-tests`); ninguna ruta de RackCad.
- **Seguridad:** repositorio aparte, sin secretos; el fixture nombra solo FX-U1 (FX-05).
- **Credenciales:** la autenticación de GitHub del Owner.
- **Consumo:** minutos de CI del repositorio del fixture.
- **Opciones:** **A** crear el remoto con CI antes de F6; **B** no crearlo: F6 queda pendiente con UNVERIFIED y vuelve al Owner. Aceptar la limitación no
  convierte en PASS una verificación incompleta.
- **Si NO:** F6 no cierra; FX-04a, FX-01 y FX-05 no dependen de OD-7.

**Selección:** `OD-7 = A` | `OD-7 = B`

## DEP-F4-YAML — Dependencia para leer `state/v2` en las pruebas (decisión nueva, no congelada)

- **Por qué:** el validador de `state/v2` (C-18, F4) lee YAML (B.8) y el proyecto de pruebas no tiene lector. AGENTS («Dependencias»): «No agregar dependencias
  sin acuerdo explícito del usuario».
- **Qué bloquea:** la elección de implementación de C-18 en F4. No bloquea F2 ni F3.
- **Rutas:** `tests/RackCad.Tests/RackCad.Tests.csproj` (solo en la opción B).
- **Seguridad y configuración:** opción B: paquete NuGet en el proyecto de pruebas, sin efecto en el producto; opción A: ninguna.
- **Opciones:** **A (recomendada)** lector mínimo del subconjunto YAML que escribe el protocolo, en el código de prueba, sin dependencia nueva; **B** añadir
  YamlDotNet al proyecto de pruebas.
- **Si NO (B rechazada):** se aplica A.

**Selección:** `DEP-F4-YAML = A` | `DEP-F4-YAML = B`

## OD-1 — ADR sucesor y deltas reservados (recordatorio; no se prepara su contenido aquí)

Aceptación de ADR-0048 y de los deltas OWNER-RESERVED (ampliación de §16, §16.13, fila y sección de WORKFLOW, aplicabilidad, rebase y toma, orquestación
autónoma y LIFECYCLE con OD-6 alternativa 1). **Momento:** antes de READY-03 (V14 §18). Hoy no bloquea ningún gate de implementación.
