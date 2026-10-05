# I-62 — Paquetes de decisión del Owner (preparados; ninguno se solicita todavía)

> **Preparación.** Órdenes del Coordinator de [decisiones](../../decisions/I-62.md) §33 y §34 y orden de continuación del 2026-10-04. Ninguna decisión se
> pide hasta que el trabajo autorizado llegue a su frontera exacta; la sesión no decide ninguna. Fuente: Proposal V14 §18 y Anexo D (blob `34ad80ea`).
> Revisión: 2026-10-04 (formato de respuesta en una línea; OD-2 con medición nueva; DEP-F4-YAML ya no hace falta).

## Respuesta en una línea

| Decisión | Recomendación de la sesión (no decisión) | Respuesta en una línea | Desbloquea | Si no |
|---|---|---|---|---|
| OD-2 | **B** | `OD-2 = B` (medir justo antes de la primera invocación de `codex-cli` y aceptar ese valor) | sondas y FX-02, FX-03, FX-04b, FX-06 por `codex-cli`; observación completa de C-06 | esos escenarios UNVERIFIED; `codex-cli` sigue UNKNOWN/NOT_ELIGIBLE |
| OD-3 | según el uso deseado de la topología B | `OD-3 = A` o `OD-3 = B` | FX-03; FX-06 por `claude-cli`; un transporte limpio de Architect sin clic (Track H) | FX-03 UNVERIFIED |
| OD-4 | **A** solo en el fixture | `OD-4 = A` | Worker Codex de FX-03 y FX-04b | FX-03/FX-04b UNVERIFIED o UNSUPPORTED |
| OD-5 | **A** | `OD-5 = A` | todo F6, empezando por FX-04a (listo para un comando) | F6 no se ejecuta |
| OD-7 | **A** | `OD-7 = A` (repositorio privado del fixture con CI) | cierre de F6 (FX-02), FX-04b, FX-06 paso 5 | F6 no cierra |
| DEP-F4-YAML | — | **ninguna**: no hace falta (§DEP-F4-YAML) | — | — |
| OD-1 | — | antes de READY-03 (recordatorio) | aceptación de ADR-0048 y deltas OWNER-RESERVED | READY-03 no avanza |

## OD-2 — Línea base de huella de `codex-cli`

**Recomendación: B.** La huella cambia sola con las actualizaciones de la app, así que una línea base aceptada por adelantado envejece antes de usarse (MEASURED,
abajo).

| Opción | Efecto | Riesgo |
|---|---|---|
| A | aceptar ahora `091540ED…` (observada el 2026-10-04) como línea base | puede quedar obsoleta otra vez antes de F6; la primera invocación daría P-01 y habría que volver a decidir |
| **B** | justo antes de la primera invocación afectada, la sesión mide (sin invocar) y el Owner acepta ese valor en una línea | ninguno nuevo; cada cesión compara salida con la línea base y entrada con salida (P-01/P-11) |
| C | no autorizar ahora | FX-02, FX-03, FX-04b y FX-06 por `codex-cli` quedan UNVERIFIED; F6 no cierra por FX-02 |

- **Seguridad:** solo el hash del archivo y los nombres de secciones y claves, saneados; nunca valores ni credenciales. `auth.json` no se lee.
- **Consumo:** suscripción existente con autenticación por ChatGPT; sondas ≤ 2; después, los topes de D.3.
- **Sigue bloqueado con cualquier opción:** la escritura del Worker Codex (OD-4) y el cierre de F6 sin CI (OD-7).
- **Se desbloquea:** la primera invocación medida de `codex-cli` (sondas de F6) y, con ello, la elegibilidad de su celda.

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

**Cronología de la huella:**

| SHA-256 | Momento | Causa del cambio desde la anterior |
|---|---|---|
| `37DD3559…` | registrada por I-61 tras el retiro autorizado | — |
| `42E15A03…` | G0 de I-62 | UNKNOWN |
| `40c27b57…` | revisiones del Architect de I-62, hasta ~18:45Z del 2026-10-02 | UNKNOWN |
| `155933B3…` | línea base aceptada por el Owner para I-63, 21:42Z del 2026-10-02 | KNOWN: actualización y reinicio de la app (21:31Z) |
| `091540ED…` | observada el 2026-10-04 (escrita a las 21:39Z) | UNKNOWN; coincide con una actualización de la app (2377 → 3930), sin demostrar la relación |

**Nota para el Coordinator (otra unidad):** la línea base de I-63 (`155933B3…`) ya no es la huella actual. No se actúa sobre I-63; se informa.

## OD-3 — Autenticar la CLI de Claude

**Recomendación:** A si el Owner quiere la topología B y un transporte de Architect que no exija un clic (Track H); si no, B.

| Opción | Efecto | Riesgo |
|---|---|---|
| A | el Owner instala `claude` en el `PATH` y lo autentica antes de F6 B | un archivo de credenciales en el perfil del usuario, que el protocolo nunca lee; la huella del adapter se demuestra después |
| B | no autenticar | FX-03 UNVERIFIED; el Owner decide la limitación (OV-I62-04); FX-06 solo por `codex-cli` |

- **Seguridad:** la sesión nunca ve credenciales. **Consumo:** el del plan autenticado; sondas ≤ 2.
- **Desbloquea:** FX-03; FX-06 por `claude-cli`; la revisión limpia de un Architect lanzada por un proceso, sin clic. **Sigue bloqueado:** FX-03 necesita además OD-4.

## OD-4 — Sandbox de `codex-cli` con escritura (Worker Codex)

**Recomendación: A**, limitada al worktree del fixture.

| Opción | Efecto | Riesgo |
|---|---|---|
| **A** | `workspace-write` para el Worker Codex solo en el fixture | escritura acotada por `AllowedWriteScope`; un cambio de `config.toml` sería P-01 |
| B | no autorizar | FX-04b y FX-03 UNVERIFIED o UNSUPPORTED; FX-04a no cambia |

- **Seguridad:** nunca en el worktree de RackCad. **Consumo:** Worker ≤ 2 por escenario. **Credenciales:** el push al fixture usa la autenticación de Git del Owner (OD-7).

## OD-5 — Permiso de ensayo y sesiones del sistema bajo prueba

**Recomendación: A.**

| Opción | Efecto | Riesgo |
|---|---|---|
| **A** | aceptar la semántica única de §15 y autorizar F6 con los topes de D.3; el Owner abre las sesiones A, B y B2 cuando la receta lo pide | consumo dentro de los topes; P-16 y FX-05 protegen el plano real |
| B | no aceptar | F6 no procede; otra vía exige una A-n (§15) |

- **Desbloquea:** FX-01 y **FX-04a**, que tiene receta de un comando (`f6/recipes.md`; prototipo `f6/fx04a/fx04a_proto.py` PASS); FX-05. **Sigue bloqueado:** FX-02/FX-04b/FX-06 sin OD-7 y OD-2.

## OD-7 — Remoto del fixture con CI

**Recomendación: A** (repositorio privado del Owner, sin secretos, solo FX-U1).

| Opción | Efecto | Riesgo |
|---|---|---|
| **A** | repositorio del fixture con un flujo `fixture-build` + `fixture-tests` | minutos de CI; ningún dato real |
| B | no crearlo | `Ci` = `not_run`: F6 queda pendiente con UNVERIFIED; aceptar la limitación no convierte en PASS una verificación incompleta |

## DEP-F4-YAML — ya no hace falta (sin decisión)

Prototipo medido en `f4/yaml-subset/`: lector con fallo cerrado del subconjunto exacto de `state/v2` y su escritor canónico. 27/27 casos (incluidos dos puntos
con comillas, sangría inválida, clave duplicada, ancla, alias, escalar multilínea y lista mal formada), ida y vuelta exacta de un `state/v2` completo de 217
líneas, 4/4 mutaciones del lector detectadas. Sobre los 39 archivos de estado reales: el modo HEADER (lo que lee el clasificador E.2) extrae `claim_id` de 38;
el que falta no es un estado `rackcad-automation-state`. **Resultado: DEP-F4-YAML = NOT NEEDED**, a condición de que F4 fije el escritor canónico en el
procedimiento y que el validador rechace todo lo demás (22 de los 39 `/v1` reales usan construcciones fuera del subconjunto, como escalares plegados `>-`).
No se solicita YamlDotNet.

## OD-1 — ADR sucesor y deltas reservados (recordatorio)

Aceptación de ADR-0048 y de los deltas OWNER-RESERVED. **Momento:** antes de READY-03 (V14 §18). Hoy no bloquea ningún gate de implementación.
