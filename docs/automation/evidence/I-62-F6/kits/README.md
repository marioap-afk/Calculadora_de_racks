# I-62 F6 — Tarjeta del Owner (fronteras humanas; nada de esto lo hace la sesión)

## Tarjeta de la mañana del 2026-10-07 (modo nocturno, decisiones §54) — sustituye a las secciones anteriores como orden de acción

Orden por dependencia. Ninguna sesión del fixture recibe valores esperados, oráculos, resultados de otras sesiones ni texto de RackCad o de I-62.

1. **B2 de FX-04a** (independiente; primero): sesión nueva de la app de Claude en `D:\r62-fixture\B2`, `claude-opus-5-5`, `xhigh`; texto inicial = el
   contenido exacto de `D:\r62-fixture\evidence-out\fx04a-b2\B2-kickoff.md` (SHA-256 `233b0582…`); nada más; avisar «B2 terminó».
2. **OD-2d** (una línea; paquete vigente en `../../I-62-prep/owner-decision-packets.md`): aceptación exacta de `9EA26634…` y del binario `97c57e4e…`
   condicionada a OD-2d-PROBE sin cambios.
3. **Disposiciones del Coordinator** que necesita la preparación (FX-02 grupo b: CD-01..CD-21 de `FX-02/frontiers.json`; FX-06: FX06-F05 y OQ-01..OQ-24 de
   `FX-06/staging/README.md`; F7: Q1-Q17 de `../../I-62-prep/f7/`): las trae el Owner desde el Coordinator; la supervisión solo las registra.
4. **A2 de FX-02** (después de 1, 2 y 3): sesión nueva en `D:\r62-fixture\A2` según [FX-02/launch-card-A2.md](FX-02/launch-card-A2.md); después, solo los
   «continúa» que la supervisión indique.
5. **Principal de FX-06** (después del Q7 de FX-02): sesión nueva en la carpeta limpia que prepare la supervisión, según
   [FX-06/staging/README.md](FX-06/staging/README.md) P6-P8.

---

Orden del Coordinator ([decisiones](../../../decisions/I-62.md) §47 y §49): A. GitHub Actions del fixture (R1 sin efecto; R2 con corridas,
clasificación A provisional) → B-C. sondas y paquete OD-2c (hechos) → D. OD-2c (decidida: A, §48) → E. **apertura del Principal A autorizada sin esperar
a Actions (§49) y pedida al Owner** para FX-01 y FX-04a.

## 1. A — GitHub Actions del fixture (recuperado de forma provisional, decisiones §49)

**R1** (flujo deshabilitado y habilitado, commit vacío `4a276c75` en `ci/smoke`): 0 corridas. **R2** (solo en `ci/smoke`, `workflow_dispatch:` añadido
al flujo, commit `7d053246`): corrida `push` 37515854486 y corrida manual 37515901956, las dos con `fixture-build` y `fixture-tests` en `success`
([actions-recovery-r1-r2.json](../CI/actions-recovery-r1-r2.json)). **Clasificación A provisional:** falta un push sin cambio del flujo en una rama con el
archivo original; lo dará el primer push real a `fx/u1` (el BOOTSTRAP del Principal A). Si ese push no crea corrida: clasificación B (push defectuoso) y
nueva decisión del Coordinator; R3 sigue sin autorizar. Historial: con el repositorio privado y después público, 0 corridas
([actions-diagnostic.json](../CI/actions-diagnostic.json), [actions-diagnostic-public.json](../CI/actions-diagnostic-public.json)).

## 2. D — OD-2d: nueva línea base tras la actualización de la app de Codex (pendiente)

La app de Codex se actualizó a las 18:56Z y reescribió `config.toml` (`723A6898…`) y el binario (`3b8f6e33…`): P-01, `codex-cli` en STOP. Paquete en
[owner-decision-packets.md](../../I-62-prep/owner-decision-packets.md) §OD-2d. Recomendación: revisa tú los valores que te importen y después responde
`OD-2d = A (línea base 723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8; binario 3b8f6e33caa75f232558a3cf76ff9b87bb5ef6dbcf4996372f24e55c78b1b91)`.
Solo afecta a FX-02 y FX-06; FX-01 y FX-04a siguen.

### Historial: OD-2c, decidida (A, decisiones §48)

Línea base aceptada: `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`. `codex-cli` sale del STOP P-01: la sesión revalida la huella y
el binario antes de cada invocación, y un cambio vuelve a ser P-01 / STOP sin aceptar nada. Paquete en
[owner-decision-packets.md](../../I-62-prep/owner-decision-packets.md) §OD-2c.

## 3. E — Abrir el Principal A (FX-01; después el arranque, el QH de FX-04a, y FX-02 y FX-06 con la CI) — autorizado (§49), pedido al Owner

El Coordinator autorizó FX-01 y FX-04a sin esperar a Actions (§49). Procedimiento:
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

**Antes de abrir B (preparación de la sesión de supervisión; decisión del Coordinator donde se indica):**
- **QH sin el Q7 de FX-02** (D.3, FX-04a paso 1): tras el QU de A, el Coordinator del fixture emite el contrato de T1 y una orden de QH; A publica QH con
  `task_intent` = T1 (`Attempt` vigente, FIRST, contrato custodiado, Controller de planificación con binding pendiente), `principal.state` = RELEASED y
  `window` CLOSED, y termina. Se registra como «hechos más pobres». FX-02 llegará después con A reabierto (tope de 2 sesiones de A, D.3).
- **Entradas automáticas de B (D.6):** se enumeran con ruta y SHA-256 antes de que B empiece: `~/.codex/AGENTS.md` (hoy 0 bytes), `config.toml` (solo
  hash), `rules/`, `skills/` y las memorias de Codex (`memories_1.sqlite`, `memories/`). La comprobación de que ninguna lleva hechos de FX-U1 o del
  fixture es mecánica (búsqueda de identificadores, sin publicar contenido). Si una los lleva: aislamiento UNVERIFIED y decisión antes de abrir B.
- **Huella (decisión del Coordinator):** abrir B y B2 en directorios nuevos con la app de Codex puede añadir entradas de proyecto a `config.toml` (D.7:
  `codex-desktop-session` comparte el archivo, UNVERIFIED en F2) y dejaría `codex-cli` en P-01 hasta una nueva línea base exacta. Opciones: aceptar ese
  riesgo y medir antes y después de B (paquete exacto si cambia), o decidir otra cosa antes de abrir B.

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
