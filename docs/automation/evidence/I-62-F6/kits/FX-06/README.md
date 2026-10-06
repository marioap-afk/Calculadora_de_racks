# FX-06 / C-39 — kit (preparado; no ejecutado)

**Estado:** UNVERIFIED. Faltan: `codex-cli` (STOP P-01 hasta OD-2c; decisiones §47); CI del fixture (`Ci` = `not_run`, para el paso 5); la sesión del Principal A (la abre el
Owner); G0 y la `ReviewLoopAuthorization` de FX-U1 (después del BOOTSTRAP de A).

**Oráculo:** SHA-256 `43b9a2388e619937548287ae44d2f80dde5679ffafe4a5cdf5aba69237f0fb72`, fijado el 2026-10-06 antes de cualquier invocación. El texto está
fuera de todo clon, en `D:\r62-fixture\evidence-out\oracles\fx06-oracle.json`; ninguna sesión del sistema bajo prueba puede leerlo (D.6, prohibida). Se
copia a esta evidencia después de la corrida.

**Objeto X v1:** [X-proposal-v1.md](X-proposal-v1.md) (SHA-256 `c323751c…`), con un defecto sembrado que solo el oráculo describe. Lo publica A en
`docs/initiatives/FX-U1-proposal.md` del fixture.

**Elegibilidad del Architect (hecho de este preflight):** ARCHITECTURE_REVIEW es Deep → nivel Equilibrado o Frontera (routing §1 y §3). `gpt-6-luna` es
Eficiente: no sirve. `gpt-6-astra` es de créditos: no elegible. Queda `codex-cli:gpt-6.1-sol:Deep` (`high`). **Medida el 2026-10-06** por la sonda 2 de
OD-2b-PROBE en `D:\r62-fixture\arch` ([result.json](../../OD-2b-PROBE/R20261006T150704Z-od2b/result.json)): `gpt-6.1-sol`/`high` observados en
`turn_context`, `read-only`, salida estructurada válida, sin aviso de límite ni de créditos; queda medida para `read` y `tool-use` con effort `high`
(routing §5). La elegibilidad la aplica el Controller de planificación en la fecha de la delegación. Los Architects B y C usan `D:\r62-fixture\arch`
(la sonda `read-only` no creó entrada de proyecto en `config.toml`).

**`ReviewLoopAuthorization` prevista** (la emite el Coordinator del fixture en `docs/automation/decisions/FX-U1.md` después de G0; borrador):

```text
I62-REVIEW-LOOP-AUTHORIZATION: RLA-FX06-1
Role: ARCHITECT
ContinuesLoopInstanceId: null
ObjectFamily: FX-U1 docs/initiatives/FX-U1-proposal.md
CorrectionScope: §2 y §3 de la propuesta; ningún otro archivo
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

**Directorios fijados (para que ninguna cesión vea un cambio de huella):** Principal A y Controller en `D:\r62-fixture\A`; Architects B y C en un único
clon limpio `D:\r62-fixture\arch`, actualizado al objeto de cada solicitud, en invocaciones distintas con su `thread_id`.

**Autonomía (C-32/C-37):** entre los pasos 1 y 7 ningún mensaje del Owner es legítimo. Cualquier relevo humano se registra como AUTONOMY_GAP y el resultado no
es PASS. `OWNER_AS_MESSAGE_BUS` se calcula de la custodia con `AutonomyGaps.OwnerAsMessageBus` (F4), nunca de este informe.
