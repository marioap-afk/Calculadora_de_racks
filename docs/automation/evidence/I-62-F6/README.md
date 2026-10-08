# I-62 — Evidencia de F6 (pilotos en el fixture; plano c)

```text
Autoridad:  Freeze V14 + A-1 AGREED + F4 GATE PASS; decisiones del Owner OD-5/OD-7/OD-2/OD-4 = A, OD-3 = RECHAZAR (§46), OD-2b-PROBE = A (§47), OD-7 corregida a PÚBLICO y OD-2c = A (§48), OD-2d-PROBE = A (§51, sobre la instalación anterior)
Orden:      §46 (nocturna), §47-§50, §51 (C-22/C-23 PASS, F6-OBS-01), §52 (selección de B), §53 (B1 INVALID_TEST_ORACLE; contrato y oráculo v2) y §54 (modo nocturno); I-61 sigue activa; superficies I62 inactivas en RackCad
Estado:     F6 EN CURSO; F6 GATE PASS no se autodeclara
```

## Escenarios (estado de esta ejecución)

| Escenario | Estado | Causa exacta | Próximo paso |
|---|---|---|---|
| F6-A fixture D.1 | **hecho** (pasos 1-4) | — | pasos 5-6 los publica el Principal A (su preflight CUSTODY es suyo, §8.6) |
| CI del fixture | **operativa: clasificación A** (§50) | R2 restableció la ejecución; el push ordinario del BOOTSTRAP a `fx/u1` (sin tocar el flujo) creó la corrida 37538606357 con los dos jobs en `success`; G0, QU, T1 y QH también pasaron ([chain.json](FX-U1-chain/chain.json)) | — |
| FX-05 / C-27 | **PASS** (corrida 2; confirmado por el Coordinator, §47) | la corrida 1 se conserva como inválida | no se repite salvo que su evidencia se invalide |
| OD-4 sondas | **medido**: sonda 1 de ≤ 2 | escritura de archivos y herramientas SUPPORTED; **commit UNSUPPORTED** (`.git/index.lock` denegado por el sandbox); fuera del espacio bloqueado | sin segunda sonda: transporte en STOP |
| OD-2b-PROBE | **hecho** (2 sondas `read-only`, §47) | sonda 1 en `A` (`gpt-6-luna`/`high`) y sonda 2 en `arch` (`gpt-6.1-sol`/`high`): huella `9002E854…` antes y después de cada una; ninguna entrada nueva; `read-only` no crea entradas, `workspace-write` sí ([result.json](OD-2b-PROBE/R20261006T150704Z-od2b/result.json)) | — |
| OD-2 | **P-01 / STOP de `codex-cli`** | instalación actual estable desde 03:29Z (comprobada a las 08:20Z): huella `9EA26634…`, binario `97c57e4e…`, app `26.1002.6548.0` ([result.json](OD-2/R20261007T061915Z-od2d-night-passive/result.json)) | tras clasificar FX-04a: OD-2d-PROBE (tarjeta de mañana, acción 2) y después OD-2d sobre el paquete medido (acción 3) |
| FX-01 / C-23 | **PASS** (Coordinator, §51) | 4 preflights conformes al oráculo; la observación inicial de más en `xhigh` es una desviación no material; se conservan las cuatro ([result.json](FX-01/R20261006T203145Z-fx01/result.json)) | — |
| FX-02 / C-24 | **UNVERIFIED — staging completo hasta sus fronteras** (§54, evidencia §76-§77) | kit en `kits/FX-02/`; bloquean FX-04a abierto (sin escrituras en el origen), su clasificación, OD-2d-PROBE y OD-2d, el grupo (b) del Coordinator (CD-01..CD-06, CD-08..CD-11, CD-13..CD-15, CD-17..CD-25; `kits/coordinator-disposition-request.md`), la apertura de A2 y la CI real del fixture | tarjeta de mañana, acciones 1-4 |
| FX-04a / C-25a | **UNVERIFIED / OPEN** (Coordinator, §55) | B1 INVALID_TEST_ORACLE; B2 INVALID_LAUNCH (FAIL bruto 2/23 conservado); tope de Principal B agotado; B3 no autorizada con el Freeze vigente | A-2 (candidata, material) acordada por Architect + Coordinator; después, disposición que nombre B3 |
| FX-04b / C-25b | **UNSUPPORTED** (medido) | el Worker Codex no puede hacer commit (OD-4, sonda 1); ningún otro adapter lanzable por B tiene escritura acreditada (`claude-cli` rechazado, OD-3) | decisión del Owner sobre la limitación (OV-I62-05 b) |
| FX-06 / C-39 | **UNVERIFIED — staging completo hasta sus fronteras** (§54, evidencia §76-§77) | kit en `kits/FX-06/staging/` (auditor de `OWNER_AS_MESSAGE_BUS` v3.1.0, autoprueba 64/64 con entradas sintéticas); bloquean la clasificación de FX-04a, OD-2d-PROBE (re-medición de la celda del Architect), P1b y OD-2d, el QH de FX-02 con la terminación de A2 acreditada y las precondiciones de FX06-F04, las decisiones de FX06-F05 y la apertura del Principal en `D:\r62-fixture\A6` | tarjeta de mañana, acción 5 |
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
| C-22 | fixture arrancable (D.1 hasta un contrato I62 válido) | **PASS** (Coordinator, §51) | BOOTSTRAP `1746b404`, G0 `5a3a7d69`, QU `1a4fc9c6`, contrato de T1 `d30fb6a9` ([chain.json](FX-U1-chain/chain.json)) |
| C-23 | FX-01 autoverificación | **PASS** (Coordinator, §51) | 4/4 conformes; desviación no material |
| C-24 | FX-02 topología A | UNVERIFIED — staging | bloquean FX-04a (B2 y su clasificación), OD-2d-PROBE y OD-2d, el grupo (b) del Coordinator y la apertura de A2; CI del fixture en clasificación A |
| C-25a | FX-04a portabilidad | UNVERIFIED (§55) | B1 INVALID_TEST_ORACLE; B2 INVALID_LAUNCH; F6 GATE PASS imposible hasta satisfacer C-25a |
| C-25b | FX-04b continuación | UNSUPPORTED (medido) | Worker Codex sin commit en `workspace-write`; ningún otro adapter con escritura acreditada lanzable por B; decide el Owner (OV-I62-05 b) |
| C-26 | FX-03 topología B | UNVERIFIED | OD-3 = RECHAZAR; además el Worker Codex no puede hacer commit; limitación para OV-I62-04; no se retira |
| C-27 | FX-05 | **PASS** (Coordinator, §47) | `FX-05/R20261006T072900Z-fx05/` |
| C-39 | FX-06 autonomía real | UNVERIFIED — staging | bloquean la clasificación de FX-04a, OD-2d-PROBE con la re-medición de la celda del Architect, P1b, OD-2d, FX-02 hasta su QH, FX06-F05 y la apertura del Principal; kit en `kits/FX-06/staging/` |
| C-32 (F6) | auditoría del transporte de FX-06 | UNVERIFIED | FX-06 no ejecutado |
| C-37 (F6) | relevo manual → AUTONOMY_GAP | sin relevos | ninguna ejecución de esta noche tuvo un relevo humano; FX-06 no ejecutado |
| C-42 (F6: 7, 8) | reconstrucción de la fidelidad desde la custodia; cambio de proveedor o de runtime entre intentos | UNVERIFIED | ningún intento real de revisión todavía (necesitan FX-02/FX-06) |

## FX-03 — preparación (OD-3 = RECHAZAR)

| Rol | Binding posible | Estado |
|---|---|---|
| Principal | `codex-desktop-session` (la abre el Owner) | disponible tras OD-5; huella del adapter UNVERIFIED (F2) |
| Controller | `codex-cli` (`gpt-6-luna`/`high`) | STOP P-01 hasta OD-2d (decisiones §49; evidencia §67) |
| Worker | `codex-cli` con escritura | **UNSUPPORTED medido**: commit denegado por el sandbox |
| Reviewer y Architect | `claude-cli` | no autenticado: OD-3 = RECHAZAR (decisión del Owner, no se cambia) |

Resultado esperado si las OD estuvieran concedidas: VERIFIED + Q7 en la topología B. Limitación exacta hoy: **UNVERIFIED por la frontera de OD-3**, con la
limitación medida del Worker que lo haría UNSUPPORTED aun con OD-3. Sigue a OV-I62-04.
