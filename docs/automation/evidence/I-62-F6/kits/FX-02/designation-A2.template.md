# Plantilla — designación del titular nuevo A2 de FX-U1 (Coordinator del fixture)

> **Preparación; no publicada.** Se publica en S08 (README §2), en un commit del Coordinator del fixture que añade al final de
> `docs/automation/decisions/FX-U1.md` el bloque entre `<<<BEGIN>>>` y `<<<END>>>` ya relleno (push a `origin` y a `github`). Antes: validación S07 en
> pass. Sin esperados y sin identificadores reales (P-16): antes de publicar, la búsqueda literal de las notas de `order-FX-U1-O4.template.md` sobre el
> bloque relleno (las fuentes de los valores de la tabla, como «ev. §71» o «dec. §52», no entran en el bloque).

**Fuente literal de los marcadores** — AP 16.28, «Marcadores» (copia byte a byte en el fixture):
- «Cada decisión va en un bloque cercado de `docs/automation/decisions/<unit>.md`. La línea del marcador es exacta y los demás campos son líneas
  `Clave: valor`»;
- «**Titular nuevo** (QR, T12a): `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED` con `Claim-Id`, en la designación del Coordinator.»;
- «**Toma con rebase** (16.25): `I62-REBASE-TAKEOVER: <BindingId>` junto con `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED` y, en T12b, la referencia a la
  terminación acreditada del titular anterior.»

Precedente en el fixture: la designación de R (`1d2f14bf`, con fe de erratas `f4f5929f`): prosa con la terminación acreditada del titular anterior,
la observación y la propuesta validadas con sus commits, el alcance de la designación, y el bloque de dos líneas. Lección de esa fe de erratas: copiar
el SHA de 40 caracteres del commit de la propuesta, no uno abreviado.

| Marcador de posición | Valor |
|---|---|
| `{PREFLIGHT_ID}` / `{PREFLIGHT_COMMIT}` | id del preflight CUSTODY de A2 y SHA completo del commit que lo publicó |
| `{BINDING_ID}` / `{PROPOSAL_COMMIT}` | `BindingId` del binding PENDING de A2 y SHA completo de su commit |
| `{EVIDENCE_DIR}` | el de la orden O4 |
| `{R_TERMINATION}` | «metadatos `isRunning` = false en las observaciones de 2026-10-07T03:38:55Z y de hacia las 03:40Z; última actividad 03:38:41Z, tras el push del QH» (ev. §71; acreditada en dec. §52), observado por la sesión de supervisión; la supervisión vuelve a observarlo justo antes de publicar |
| `{MAIN_CHECK}` | resultado de S00: `main` del fixture = `fbe25347…` = `protocol.effective_sha` (sin avance) |

## Variante T16 (sin avance de `main`; la prevista)

<<<BEGIN>>>
## Designación del titular nuevo de FX-U1 (Coordinator del fixture; orden FX-U1-O4)

Terminación del titular anterior (liberado en el QH `cabed54738f56e846bd77be321cace10d053094d`, `record_version` 5) acreditada por la sesión de
supervisión: {R_TERMINATION}. `main` del fixture sin avance respecto de `protocol.effective_sha`: {MAIN_CHECK}. Observación del titular nuevo
`{PREFLIGHT_ID}` (CUSTODY; commit `{PREFLIGHT_COMMIT}`) y propuesta `{BINDING_ID}` (`rackcad-binding/v1`, PRINCIPAL_COORDINATOR, `Scope` UNIT; commit
`{PROPOSAL_COMMIT}`), en `{EVIDENCE_DIR}`, validadas contra los esquemas del fixture. Designación para la transferencia T16 (QR ORDINARY) y, después,
para la ventana 1 de T1 según la orden FX-U1-O4 y el contrato `docs/automation/decisions/FX-U1-T1.gate-contract.json` (blob
`628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0`).

```text
I62-PRINCIPAL-BINDING: {BINDING_ID} ACCEPTED
Claim-Id: fc62f1c7-0000-4000-8000-000000000001
```
<<<END>>>

## Variante T22 (solo si S00 observa avance de `main` del fixture; AP 16.25 «Toma de custodia con avance de `main`» y AP 16.26 T22)

Misma prosa, sustituyendo la frase de `main` por el avance observado (`main_before`, `main_after`), y limitando la designación a las cinco acciones de
16.25 y a **un** QR REBASE_RECONCILIATION antes de cualquier trabajo ordinario. Bloque:

```text
I62-REBASE-TAKEOVER: {BINDING_ID}
I62-PRINCIPAL-BINDING: {BINDING_ID} ACCEPTED
Claim-Id: fc62f1c7-0000-4000-8000-000000000001
```

La variante T22 invalida además la orden O4 tal como está escrita (el contrato de T1 tiene `MainSha` = `fbe25347…` y habría que reemitirlo, AP 16.7
«Antes de escribir»): con avance de `main`, la supervisión se detiene y vuelve al Coordinator antes de publicar nada.
