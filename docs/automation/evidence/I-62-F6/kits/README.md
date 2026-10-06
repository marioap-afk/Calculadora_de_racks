# I-62 F6 — Tarjeta del Owner (fronteras humanas; nada de esto lo hace la sesión)

Orden preferido del Coordinator ([decisiones](../../../decisions/I-62.md) §47): **A.** GitHub Actions del fixture → **B.** dos sondas de OD-2b-PROBE
(hechas) → **C.** paquete OD-2c (hecho) → **D.** el Owner decide OD-2c → **E.** solo entonces se pide abrir el Principal A. La apertura de A **no se pide
todavía**.

## 1. A — GitHub Actions del fixture (bloqueo principal: sin CI, FX-02 no puede ser PASS y F6 no cierra)

**Diagnóstico de solo lectura** ([CI/actions-diagnostic.json](../CI/actions-diagnostic.json), 2026-10-06T15:13:20Z): `marioap-afk/rackcad-i62-fixture`
(privado) tiene 0 corridas. Actions está habilitado (`allowed_actions` all); el flujo `fixture` está activo y bien formado (`on: push`, sin filtros); el
`PushEvent` de `fx/u1` llegó (07:35:53Z); en cada SHA solo hay check suites de la app `claude`. El repositorio **público** de RackCad, de la misma cuenta,
sí ejecuta Actions. La causa más probable está en la cuenta y afecta solo a repositorios privados (facturación, presupuesto o método de pago de Actions).
La sesión no puede leer la facturación (el token no tiene el alcance `user`) y no amplía credenciales ni busca un bypass.

**Comprobación del Owner en la interfaz de GitHub, en este orden** (anota el texto literal de cualquier aviso):
1. `https://github.com/marioap-afk/rackcad-i62-fixture/actions`: ¿hay un aviso arriba (Actions deshabilitado, cuenta bloqueada, facturación)?
2. `https://github.com/settings/billing`: avisos de pago fallido o de cuenta bloqueada; en el uso del mes, los minutos de Actions usados frente a los
   incluidos.
3. En la página de presupuestos y alertas de la facturación (*Budgets and alerts*): un presupuesto de Actions en 0 con «detener el uso al alcanzar el límite»
   bloquea los repositorios privados cuando no quedan minutos incluidos.
4. En la información de pago (*Payment information*): método de pago caducado o rechazado.
5. `https://github.com/marioap-afk/rackcad-i62-fixture/settings/actions`: debe decir «Allow all actions» (la API ya lo confirma; no hace falta cambiarlo).

**Después:**
- si encuentras y corriges la causa, dile a la sesión «Actions corregido»: la sesión empuja un commit vacío en `ci/smoke` del fixture y registra la corrida
  (id, evento, ref, SHA exacto, jobs `fixture-build` y `fixture-tests`, conclusión), sin heredar CI entre SHAs;
- si no hay ningún aviso, dile a la sesión el texto que veas en el paso 1. Hacer público el fixture sería cambiar OD-7 (decisión tuya, no un arreglo de la
  sesión); un ticket a GitHub Support puede citar el repositorio `1406872390` y los SHAs `fbe25347`, `7cf79ffc` y `54f2a4a8`.

## 2. D — OD-2c: huella exacta de `codex-cli` (paquete listo)

Ver [owner-decision-packets.md](../../I-62-prep/owner-decision-packets.md) §OD-2c. Las sondas de OD-2b-PROBE no cambiaron la huella; el único cambio desde la
línea base aceptada es la entrada de la sonda de OD-4, comprobado byte a byte. Respuesta en una línea:
- **Aprobar:** `OD-2c = A (línea base 9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E)`
- **Rechazar:** `OD-2c = RECHAZAR`

## 3. E — Abrir el Principal A (FX-01; después el arranque, FX-02, FX-06 y el QH de FX-04a) — todavía no

Se pedirá cuando A y D estén resueltos. El procedimiento queda preparado:
1. En la app de Claude, una **sesión nueva** con directorio `D:\r62-fixture\A`, modelo `claude-opus-5-5`, effort **`high`** (insuficiente a propósito: el
   requerido de PRINCIPAL_COORDINATION es Long-horizon = `xhigh`).
2. Primer mensaje (sin hechos de la unidad):
   `Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden FX-U1-O1 de docs/automation/decisions/FX-U1.md.`
3. Cuando A registre su primer preflight (esperado: BELOW_REQUIRED → STOP P-09 de CUSTODY), sube el effort a **`xhigh`** y dile `continúa`.
4. Tras el segundo preflight (esperado: MATCH), baja el effort a **`medium`** y dile `continúa`; tras el tercero (esperado: BELOW_REQUIRED) vuelve a
   `xhigh` y dile `continúa` para que publique el BOOTSTRAP.
5. La decisión G0 la escribe la sesión de supervisión como Coordinator del fixture en `docs/automation/decisions/FX-U1.md`; avísale cuando A haya
   publicado el BOOTSTRAP.

Los mensajes `continúa` son estímulos del ensayo FX-01, no relevos de un rol: FX-01 no evalúa la autonomía (eso es FX-06).

## 4. FX-04a (después del QH de A)

La sesión de supervisión acredita la terminación de A, calcula el oráculo (`FX-04a/fx04a_real.py oracle`) y publica solo su SHA-256 antes de que B
empiece; prepara el clon limpio `D:\r62-fixture\B` en el QH. El Owner abre una sesión de la **app de Codex** en `D:\r62-fixture\B` y le pega el
contenido de `FX-04a/B-prompt.md` con `FX-04a/response.schema.json`. Después, B2 para N11 igual, en `D:\r62-fixture\B2`.

## 5. Límites ya medidos (para decidir, no para ejecutar)

- **FX-04b:** UNSUPPORTED medido (el Worker Codex no puede hacer commit en `workspace-write`; ningún otro adapter con escritura acreditada que B pueda
  lanzar, porque OD-3 = RECHAZAR). Decisión del Owner sobre la limitación: OV-I62-05 (b).
- **FX-03:** UNVERIFIED por OD-3 = RECHAZAR; con OD-3 seguiría limitado por el mismo commit del Worker Codex. Limitación: OV-I62-04.

## 6. Acciones de la sesión de supervisión (no del Owner) cuando se abran las fronteras

- **CI:** tras el aviso del Owner, un commit vacío en `ci/smoke` del fixture (`supervisor`), push a `github`, y registro de la corrida. Sin CI heredada
  entre SHAs.
- **`codex-cli` tras OD-2c = A:** antes de cada invocación, huella = `9002E854…` y binario `37762753…`; si cambian, P-01 y STOP sin aceptar nada. Solo la
  receta 16.4 (`read-only`): Controller en `D:\r62-fixture\A` (`gpt-6-luna`/`high`, medida); Architects B y C de FX-06 en `D:\r62-fixture\arch`
  (`gpt-6.1-sol`/`high`, invocación medida en la sonda 2), cada uno con su `thread_id`. Ningún `workspace-write` (crearía una entrada nueva: P-01).
- **G0 del Coordinator del fixture**, después del BOOTSTRAP de A (16.28; se añade a `docs/automation/decisions/FX-U1.md` del fixture en `fx/u1`):

```text
I62-DELEGATED-EXECUTION: I62_DELEGATED
I62-CLASSIFICATION: I62
I62-PRINCIPAL-BINDING: <BindingId del binding propuesto por A> ACCEPTED
Claim-Id: fc62f1c7-0000-4000-8000-000000000001
BootstrapRecordVersion: 1
```

- **Contrato de T1** (`gate-contract/v2`), después del QU de A: `ProtocolSet` I62, `AuthorityRevision` = commit del BOOTSTRAP de FX-U1, `EXTERNAL` = F_eff
  `fbe25347`; tarea mínima sobre `Fixture.Lib` (p. ej., una operación nueva con su prueba). El Controller y el Architect esperan a OD-2c.
- **FX-04a:** al QH de A, terminación de A acreditada por los metadatos de la app (`isRunning`) o atestada por el Owner; `fx04a_real.py oracle` →
  SHA-256 en la evidencia real antes de crear `D:\r62-fixture\B` (`git clone --no-local` en el QH); `hash` de la respuesta de B antes de entregar el
  oráculo; `compare`; `n11` para B2.
