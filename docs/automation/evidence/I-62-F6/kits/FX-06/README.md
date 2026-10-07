# FX-06 / C-39 — kit (preparado; no ejecutado)

> **SUSTITUIDO por [staging/](staging/README.md)** (kit de staging de la supervisión, revisiones R1 y R2). Este archivo se conserva como historial: no
> es una orden de acción, y sus valores no se reutilizan sin pasar por `staging/`. Rigen la secuencia, las fronteras y las preguntas abiertas de
> [staging/README.md](staging/README.md) y [staging/frontiers.json](staging/frontiers.json).

**Estado:** UNVERIFIED. Hechos vigentes (decisiones §50-§54; evidencia §75): `codex-cli` en P-01 / STOP, pendiente de OD-2d-PROBE y de OD-2d
(ningún OD-2d-PROBE hasta clasificar FX-04a con B2, decisiones §53); CI del fixture con clasificación A (push ordinario a `fx/u1` operativo; no vuelve a
`not_run`, decisiones §51 y §54); la sesión del Principal la abre el Owner (staging P7); G0 de FX-U1 ya está aceptado (C-22 PASS, decisiones §51); la
`ReviewLoopAuthorization` la publica el Coordinator del fixture en el paso P5 de staging.

**Oráculo:** SHA-256 `43b9a2388e619937548287ae44d2f80dde5679ffafe4a5cdf5aba69237f0fb72`, fijado el 2026-10-06 antes de cualquier invocación. El texto está
fuera de todo clon, en `D:\r62-fixture\evidence-out\oracles\fx06-oracle.json`; ninguna sesión del sistema bajo prueba puede leerlo (D.6, prohibida). Se
copia a esta evidencia después de la corrida.

**Objeto X v1:** [X-proposal-v1.md](X-proposal-v1.md) (SHA-256 `c323751c…`), con un defecto sembrado que solo el oráculo describe. Lo publica A en
`docs/initiatives/FX-U1-proposal.md` del fixture.

**Elegibilidad del Architect (hecho de este preflight):** ARCHITECTURE_REVIEW es Deep → nivel Equilibrado o Frontera (routing §1 y §3). `gpt-6-luna` es
Eficiente: no sirve. `gpt-6-astra` es de créditos: no elegible. Queda `codex-cli:gpt-6.1-sol:Deep` (`high`). **Medición OBSOLETA** desde la actualización de la app de Codex (binario nuevo, decisiones §50): hay que volver a medirla antes de usarla. Medida el 2026-10-06 con el binario anterior por la sonda 2 de
OD-2b-PROBE en `D:\r62-fixture\arch` ([result.json](../../OD-2b-PROBE/R20261006T150704Z-od2b/result.json)): `gpt-6.1-sol`/`high` observados en
`turn_context`, `read-only`, salida estructurada válida, sin aviso de límite ni de créditos; queda medida para `read` y `tool-use` con effort `high`
(routing §5). La elegibilidad la aplica el Controller de planificación en la fecha de la delegación. Este kit fijó `D:\r62-fixture\arch` para los
Architects B y C (la sonda `read-only` no creó entrada de proyecto en `config.toml`); hoy el directorio depende de OQ-13 de staging.

**`ReviewLoopAuthorization` prevista — NO REUTILIZAR (R-09).** El borrador siguiente queda solo como historial: su `CorrectionScope` nombraba secciones
concretas del objeto y no era independiente del oráculo (valor retirado en R2), y sus demás líneas no siguen los formatos de 16.28 y §20.5.1 (staging R-09;
[staging/rla.template.md](staging/rla.template.md) §3). La plantilla vigente es [staging/rla.template.md](staging/rla.template.md).

```text
NO REUTILIZAR (R-09) — borrador histórico; ver staging/rla.template.md
I62-REVIEW-LOOP-AUTHORIZATION: RLA-FX06-1
Role: ARCHITECT
ContinuesLoopInstanceId: null
ObjectFamily: FX-U1 docs/initiatives/FX-U1-proposal.md
CorrectionScope: <retirado en R2: no independiente del oráculo (R-09)>
Budget.review_rounds: 2
Budget.logical_requests: 2
Budget.transport_reruns_per_request: 1
Budget.architect_launches: 4
AuthorizedActions: REVIEW_DESIGN
MinimumCapabilities: read, structured-output
RequiredIndependence: proveedor distinto del Principal (PREFERRED); sesión distinta en cada solicitud
EligibleCells: codex-cli:gpt-6.1-sol:Deep
ModelEffortBounds: high
Permissions: READ_ONLY
Validity.Until: <instante del día de la corrida + 1 día>
Claim-Id: fc62f1c7-0000-4000-8000-000000000001
```

**Directorios fijados (historial; sustituido por staging P6 y OQ-13):** Principal A y Controller en `D:\r62-fixture\A`; Architects B y C en un único
clon limpio `D:\r62-fixture\arch`, actualizado al objeto de cada solicitud, en invocaciones distintas con su `thread_id`. Hoy A está terminado y no vuelve
a operar (decisiones §51); la carpeta del Principal de FX-06 es `D:\r62-fixture\A6` (staging P6), y el directorio `-C` del Architect depende de OQ-13.

**Autonomía (C-32/C-37):** entre los pasos 1 y 7 ningún mensaje del Owner es legítimo. Cualquier relevo humano se registra como AUTONOMY_GAP y el resultado no
es PASS. `OWNER_AS_MESSAGE_BUS` se calcula de la custodia con `AutonomyGaps.OwnerAsMessageBus` (F4), nunca de este informe.
