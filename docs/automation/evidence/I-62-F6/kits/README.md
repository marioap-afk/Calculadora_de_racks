# I-62 F6 — Tarjeta del Owner para la mañana (fronteras humanas; nada de esto lo hace la sesión)

Orden recomendado. Cada paso es independiente salvo donde se indica.

## 1. CI del fixture (desbloquea el cierre de F6, FX-02, FX-04b y el paso 5 de FX-06)

GitHub Actions no crea ninguna corrida en `marioap-afk/rackcad-i62-fixture` (privado): tres pushes, ninguna check suite de Actions, Actions habilitado en el
repositorio, flujo `fixture` activo, estado de GitHub operativo. La sesión no tiene un alcance de facturación y no busca un bypass.
- **Acción del Owner:** revisar en GitHub la configuración de Actions y la facturación de la cuenta para repositorios privados (cuota, límite de gasto,
  política de Actions de la cuenta) y, si se corrige, decírselo a la sesión. La sesión empuja entonces un commit vacío en `ci/smoke` y verifica la corrida.

## 2. OD-2b — nueva línea base de `codex-cli` (desbloquea FX-02 y FX-06)

Ver `../../I-62-prep/owner-decision-packets.md` §OD-2b. Respuesta en una línea, por ejemplo:
`OD-2b = A2 (sondas en D:\r62-fixture\A y D:\r62-fixture\arch; aceptar el hash resultante si el delta saneado es exactamente +2 secciones [projects.<redactado>] de una clave)`

## 3. Abrir el Principal A (FX-01; después el arranque, FX-02, FX-06 y el QH de FX-04a)

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

- **CI:** tras el aviso del Owner, un commit vacío en `ci/smoke` del fixture (`supervisor`), push a `github`, y registro de la corrida (id, evento, ref, SHA
  exacto, jobs `fixture-build` y `fixture-tests`, conclusión). Sin CI heredada entre SHAs.
- **OD-2b = A2:** antes de cada sonda, huella = `9002E854…`; sonda de solo lectura en `D:\r62-fixture\A` (Controller, `gpt-6-luna`/`high`) y en un clon limpio
  `D:\r62-fixture\arch` (Architect, `gpt-6.1-sol`/`high`); después, el delta saneado debe ser exactamente +2 secciones `[projects.<redactado>]` de una clave.
  Si es así, la huella medida es la línea base; si no, P-01 y STOP.
- **G0 del Coordinator del fixture**, después del BOOTSTRAP de A (16.28; se añade a `docs/automation/decisions/FX-U1.md` del fixture en `fx/u1`):

```text
I62-DELEGATED-EXECUTION: I62_DELEGATED
I62-CLASSIFICATION: I62
I62-PRINCIPAL-BINDING: <BindingId del binding propuesto por A> ACCEPTED
Claim-Id: fc62f1c7-0000-4000-8000-000000000001
BootstrapRecordVersion: 1
```

- **Contrato de T1** (`gate-contract/v2`), después del QU de A: `ProtocolSet` I62, `AuthorityRevision` = commit del BOOTSTRAP de FX-U1, `EXTERNAL` = F_eff
  `fbe25347`; tarea mínima sobre `Fixture.Lib` (p. ej., una operación nueva con su prueba). El Controller y el Architect esperan a OD-2b.
- **FX-04a:** al QH de A, terminación de A acreditada por los metadatos de la app (`isRunning`) o atestada por el Owner; `fx04a_real.py oracle` →
  SHA-256 en la evidencia real antes de crear `D:\r62-fixture\B` (`git clone --no-local` en el QH); `hash` de la respuesta de B antes de entregar el
  oráculo; `compare`; `n11` para B2.
