# I-62 F6 — Tarjeta del Owner (fronteras humanas; nada de esto lo hace la sesión)

Orden preferido del Coordinator ([decisiones](../../../decisions/I-62.md) §47): **A.** GitHub Actions del fixture → **B.** dos sondas de OD-2b-PROBE
(hechas) → **C.** paquete OD-2c (hecho) → **D.** OD-2c (**decidida: A**, §48) → **E.** solo entonces se pide abrir el Principal A. La apertura de A
**no se pide todavía**: falta A.

## 1. A — GitHub Actions del fixture (bloqueo principal: sin CI, FX-02 no puede ser PASS y F6 no cierra)

**Estado (2026-10-06T16:04Z;** [diagnóstico](../CI/actions-diagnostic-public.json)**):** el fixture es **público** desde las 15:50:43Z (OD-7 corregida,
§48), tras una auditoría previa limpia ([public-audit.json](../CI/public-audit.json)). El commit vacío `a0a3d483` en `ci/smoke` (15:51:00Z) **no creó
ninguna corrida**: la hipótesis de la facturación de repositorios privados queda refutada. GitHub procesa los pushes (check suite de la app `claude`), el
flujo `fixture` está activo, con bytes limpios y el mismo `on: push:` vacío que el de RackCad, que sí corre. La causa es desconocida y específica de este
repositorio. El diagnóstico anterior, con el repositorio privado, está en [actions-diagnostic.json](../CI/actions-diagnostic.json).

**Comprobación del Owner con su sesión de GitHub** (ve avisos que la API y la vista anónima no muestran; anota el texto literal):
1. `https://github.com/marioap-afk/rackcad-i62-fixture/actions`: ¿hay un aviso arriba (flujos que no se ejecutan, botón para habilitarlos, restricción
   de la cuenta)? ¿Aparece el flujo `fixture` como deshabilitado?
2. `https://github.com/marioap-afk/rackcad-i62-fixture/actions/workflows/fixture.yml`: ¿aviso propio del flujo?
3. `https://github.com/marioap-afk/rackcad-i62-fixture/settings/actions`: debe decir «Allow all actions and reusable workflows» (la API ya lo confirma);
   anota cualquier aviso.

**Acciones posibles sobre el fixture** (cada una necesita una autorización tuya; ninguna toca RackCad ni expone nada):
- **R1 (recomendada):** la sesión deshabilita y vuelve a habilitar el flujo `fixture` (`gh workflow disable` / `enable`; reversible) y empuja otro commit
  vacío en `ci/smoke`. Fuerza un registro nuevo del flujo, que pudo quedar mal desde el primer push al repositorio vacío.
- **R2:** la sesión empuja solo en `ci/smoke` un cambio del archivo del flujo que añade `workflow_dispatch:` (sin tocar `main` ni `fx/u1`) y lo
  dispara a mano; si corre, el registro estaba roto.
- **R3:** recrear el repositorio público con las mismas refs, solo si GitHub lo exige (cambia el id del repositorio y la evidencia de CI anterior).
- Ticket a GitHub Support con el repositorio `1406872390` y los SHAs `7cf79ffc`, `54f2a4a8` y `a0a3d483`.

Si encuentras y corriges la causa, dile a la sesión «Actions corregido»: empuja un commit vacío en `ci/smoke` y registra la corrida (id, evento, ref,
SHA exacto, jobs `fixture-build` y `fixture-tests`, conclusión), sin heredar CI entre SHAs.

## 2. D — OD-2c: decidida (A, decisiones §48)

Línea base aceptada: `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`. `codex-cli` sale del STOP P-01: la sesión revalida la huella y
el binario antes de cada invocación, y un cambio vuelve a ser P-01 / STOP sin aceptar nada. Paquete en
[owner-decision-packets.md](../../I-62-prep/owner-decision-packets.md) §OD-2c.

## 3. E — Abrir el Principal A (FX-01; después el arranque, FX-02, FX-06 y el QH de FX-04a) — todavía no

Se pedirá cuando A (Actions) esté resuelto; D ya está decidida. FX-01 y FX-04a no necesitan la CI del fixture: adelantarlos es decisión del Coordinator.
El procedimiento queda preparado:
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
- **`codex-cli` (OD-2c = A, §48):** antes de cada invocación, huella = `9002E854…` y binario `37762753…`; si cambian, P-01 y STOP sin aceptar nada. Solo la
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
  `fbe25347`; tarea mínima sobre `Fixture.Lib` (p. ej., una operación nueva con su prueba). El Controller y el Architect usan la línea base de OD-2c.
- **FX-04a:** al QH de A, terminación de A acreditada por los metadatos de la app (`isRunning`) o atestada por el Owner; `fx04a_real.py oracle` →
  SHA-256 en la evidencia real antes de crear `D:\r62-fixture\B` (`git clone --no-local` en el QH); `hash` de la respuesta de B antes de entregar el
  oráculo; `compare`; `n11` para B2.
