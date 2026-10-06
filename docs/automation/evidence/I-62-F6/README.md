# I-62 — Evidencia de F6 (pilotos en el fixture; plano c)

```text
Autoridad:  Freeze V14 + A-1 AGREED + F4 GATE PASS; decisiones del Owner OD-5/OD-7/OD-2/OD-4 = A, OD-3 = RECHAZAR (§46), OD-2b-PROBE = A (§47), OD-7 corregida a PÚBLICO y OD-2c = A (§48)
Orden:      orden nocturna (§46), disposición tras la primera ejecución (§47), corrección de OD-7 (§48) y orden de continuación (§49); I-61 sigue activa; superficies I62 inactivas en RackCad
Estado:     F6 EN CURSO; F6 GATE PASS no se autodeclara
```

## Escenarios (estado de esta ejecución)

| Escenario | Estado | Causa exacta | Próximo paso |
|---|---|---|---|
| F6-A fixture D.1 | **hecho** (pasos 1-4) | — | pasos 5-6 los publica el Principal A (su preflight CUSTODY es suyo, §8.6) |
| CI del fixture | **recuperada de forma provisional (clasificación A, §49)** | público desde 15:50:43Z (§48); R1 (deshabilitar y habilitar el flujo, commit vacío `4a276c75`): 0 corridas; R2 (`workflow_dispatch:` solo en `ci/smoke`, commit `7d053246`): corrida `push` 37515854486 y manual 37515901956, ambas con `fixture-build` y `fixture-tests` en `success` ([actions-recovery-r1-r2.json](CI/actions-recovery-r1-r2.json)) | confirmar push sin cambio del flujo en `fx/u1` (BOOTSTRAP de A); si no corre, clasificación B |
| FX-05 / C-27 | **PASS** (corrida 2; confirmado por el Coordinator, §47) | la corrida 1 se conserva como inválida | no se repite salvo que su evidencia se invalide |
| OD-4 sondas | **medido**: sonda 1 de ≤ 2 | escritura de archivos y herramientas SUPPORTED; **commit UNSUPPORTED** (`.git/index.lock` denegado por el sandbox); fuera del espacio bloqueado | sin segunda sonda: transporte en STOP |
| OD-2b-PROBE | **hecho** (2 sondas `read-only`, §47) | sonda 1 en `A` (`gpt-6-luna`/`high`) y sonda 2 en `arch` (`gpt-6.1-sol`/`high`): huella `9002E854…` antes y después de cada una; ninguna entrada nueva; `read-only` no crea entradas, `workspace-write` sí ([result.json](OD-2b-PROBE/R20261006T150704Z-od2b/result.json)) | — |
| OD-2 | **OD-2c = A** (§48): línea base `9002E854…` | la sonda de OD-4 había añadido `[projects.'d:\r62-fixture\probe-od4-1']` (`trust_level`): `091540ED…` → `9002E854…` | `codex-cli` disponible con la huella y el binario revalidados antes de cada invocación; un cambio es P-01 / STOP |
| FX-01 / C-23 | **HUMAN_LAUNCH_REQUIRED** (pedida al Owner, §49) | la sesión A la abre el Owner y cambia el effort entre los tres preflights; comprobaciones previas hechas (evidencia §66) | tarjeta `kits/README.md` §3 |
| FX-02 / C-24 | **UNVERIFIED** (bloqueado) | sin sesión A; `Ci` = `not_run` (`codex-cli` ya disponible: OD-2c = A) | Actions + sesión A |
| FX-04a / C-25a | **HUMAN_LAUNCH_REQUIRED** | QH de A tras el BOOTSTRAP con T1 planificada (hechos más pobres), terminación de A acreditada, entradas automáticas de B enumeradas y sesión B abierta por el Owner; decisión previa sobre la huella al abrir B | tarjeta §4 |
| FX-04b / C-25b | **UNSUPPORTED** (medido) | el Worker Codex no puede hacer commit (OD-4, sonda 1); ningún otro adapter lanzable por B tiene escritura acreditada (`claude-cli` rechazado, OD-3) | decisión del Owner sobre la limitación (OV-I62-05 b) |
| FX-06 / C-39 | **UNVERIFIED** | sin sesión A; sin CI para el paso 5 (`codex-cli` disponible: OD-2c = A; celda del Architect `gpt-6.1-sol`/`high` medida) | Actions + sesión A |
| FX-03 / C-26 | **UNVERIFIED** (OD-3 = RECHAZAR) | además, el Worker Codex no puede hacer commit: con OD-3 sería UNSUPPORTED | limitación para OV-I62-04; no se retira |

## Identidad del fixture

[fixture/fixture-identity.json](fixture/fixture-identity.json): `D:\r62-fixture\`, origen `fixture-origin.git`, F_seed `930288c5` (37 autoridades copiadas byte
a byte desde `MC_I62` = `6f0187cb`, comprobado blob a blob), F_norm `a9f6c929` (único commit con el trailer), F_eff `fbe25347` (tag `test-activation/I-62`),
reclamo de FX-U1 `eea0114a` (`Claim-Id` `fc62f1c7-0000-4000-8000-000000000001`), orden FX-U1-O1 del Coordinator del fixture `54f2a4a8`. Remoto
`marioap-afk/rackcad-i62-fixture` (creado privado 2026-10-06T07:19:09Z; **público** desde 15:50:43Z por OD-7 corregida, §48) como segundo remoto **del fixture**; los remotos de RackCad no cambian.

## Medición de la sonda de OD-4

[OD-4/R20261006T072306Z-od41/result.json](OD-4/R20261006T072306Z-od41/result.json): binario `8aaf1547b825b104` (`37762753…`, `codex-cli 0.160.0`,
nuevo frente a 0.159.2), `gpt-6-luna`/`high` observados en el registro de sesión (RUNTIME_OBSERVED), sandbox `workspace-write` sin red, 147 250 tokens de
entrada (123 136 en caché) y 2 229 de salida. Antes y después de la huella, saneados, en el mismo directorio.

## FX-05

[FX-05/R20261006T072900Z-fx05/fx05-result.json](FX-05/R20261006T072900Z-fx05/fx05-result.json). La corrida 1 no vale como prueba: el detector P-16 tomaba
los identificadores reales solo de `main` y no vio I-62, cuyo estado vive en su rama. Se conserva en `fx05-run1-invalid-detector.json`. La corrida 2 toma
los identificadores de todas las puntas remotas: gate legítimo ACCEPT; artefacto que nombra I-62 REJECT; remoto de RackCad configurado REJECT; el commit
rechazado no llegó al origen del fixture; **0 diferencias** en el estado real (refs remotas, refs locales, worktrees, remotos, blobs de estado y decisiones
de I-62, árbol de estados de `main`). El rechazo lo aplica la sesión de supervisión del plano (a) a los resultados del plano (c).

## Obligaciones de F6 (Anexo C; estado de esta ejecución)

| C | Escenario | Estado | Evidencia o causa |
|---|---|---|---|
| C-22 | fixture arrancable (D.1 hasta un contrato I62 válido) | UNVERIFIED | pasos 1-4 hechos; 5-6 (BOOTSTRAP, G0, QU, contrato de T1) esperan al Principal A, cuyo preflight CUSTODY solo puede producir él (§8.6) |
| C-23 | FX-01 autoverificación | UNVERIFIED (HUMAN_LAUNCH_REQUIRED) | la sesión A la abre el Owner; secuencia de effort en `kits/README.md` |
| C-24 | FX-02 topología A | UNVERIFIED | `Ci` = `not_run`, sin sesión A (`codex-cli` disponible tras OD-2c) |
| C-25a | FX-04a portabilidad | UNVERIFIED (HUMAN_LAUNCH_REQUIRED) | QH de A, terminación acreditada y sesión B; herramienta del oráculo lista |
| C-25b | FX-04b continuación | UNSUPPORTED (medido) | Worker Codex sin commit en `workspace-write`; ningún otro adapter con escritura acreditada lanzable por B; decide el Owner (OV-I62-05 b) |
| C-26 | FX-03 topología B | UNVERIFIED | OD-3 = RECHAZAR; además el Worker Codex no puede hacer commit; limitación para OV-I62-04; no se retira |
| C-27 | FX-05 | **PASS** (Coordinator, §47) | `FX-05/R20261006T072900Z-fx05/` |
| C-39 | FX-06 autonomía real | UNVERIFIED | `Ci` = `not_run`, sin sesión A; `codex-cli` disponible y celda del Architect medida; kit en `kits/FX-06/` |
| C-32 (F6) | auditoría del transporte de FX-06 | UNVERIFIED | FX-06 no ejecutado |
| C-37 (F6) | relevo manual → AUTONOMY_GAP | sin relevos | ninguna ejecución de esta noche tuvo un relevo humano; FX-06 no ejecutado |
| C-42 (F6: 7, 8) | reconstrucción de la fidelidad desde la custodia; cambio de proveedor o de runtime entre intentos | UNVERIFIED | ningún intento real de revisión todavía (necesitan FX-02/FX-06) |

## FX-03 — preparación (OD-3 = RECHAZAR)

| Rol | Binding posible | Estado |
|---|---|---|
| Principal | `codex-desktop-session` (la abre el Owner) | disponible tras OD-5; huella del adapter UNVERIFIED (F2) |
| Controller | `codex-cli` (`gpt-6-luna`/`high`) | disponible (OD-2c = A, §48) |
| Worker | `codex-cli` con escritura | **UNSUPPORTED medido**: commit denegado por el sandbox |
| Reviewer y Architect | `claude-cli` | no autenticado: OD-3 = RECHAZAR (decisión del Owner, no se cambia) |

Resultado esperado si las OD estuvieran concedidas: VERIFIED + Q7 en la topología B. Limitación exacta hoy: **UNVERIFIED por la frontera de OD-3**, con la
limitación medida del Worker que lo haría UNSUPPORTED aun con OD-3. Sigue a OV-I62-04.
