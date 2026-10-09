# I-62 — Solicitud al Owner: consumo para FX-02 (BORRADOR; decisiones §63.8)

> **BORRADOR de la supervisión. No se pide nada ahora.** Decisiones §62.5: «No pedir autorizaciones nuevas hasta tener un objeto A-4 revisado y sus
> precondiciones acreditadas». Este documento no se entrega al Owner hasta que se cumpla la sección 3. Cuando se entregue, irá como **una sola
> solicitud** con las tres líneas (§63.8).

## 1. Líneas exactas (texto a pegar)

```text
A4-SONDA-CONSUMO = A (1 invocación read-only de codex-cli fuera de A2-P2, celda gpt-6-luna/high del Controller de FX-02 con la shell declarada cmd.exe, para el trío exacto 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68/26.1002.7124.0/6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32, con huella, binario y versión antes y después; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; no acepta su resultado)
A4-CLAUDE-FX02-CONSUMO = A (uso read-only de claude-cli como Architect de la topología A de FX-02, revisión del contrato de T1, con una celda medida elegible para ARCHITECT; 1 lanzamiento y las reejecuciones del pool de D.3, dentro de los mismos topes; Actor/Session/Context REQUIRED, Provider PREFERRED; sin PRINCIPAL_COORDINATOR, EXECUTION_CONTROLLER ni WORKER; sin escritura; sin lectura de credenciales; no acepta su resultado ni amplía OD-3, OD-5 ni CLAUDE-CLI-I62)
A4-PRINCIPAL-A2-CONSUMO = A (una sesión adicional de Principal de la ronda A de F6, por encima del tope de D.3 que cubre OD-5, solo para A2 (claude-desktop-session, claude-opus-5-5 xhigh, abierta por el Owner en D:\r62-fixture\A2 con launch-card-A2.md), contada por P-07; no fungible; no acepta su resultado)
```

| Línea | Fuente | Bytes (UTF-8) | SHA-256 de la línea |
|---|---|---|---|
| `A4-SONDA-CONSUMO` | literal de `docs/initiatives/I-62-A-4.md` §7, L354, en HEAD `d5b454fd` (blob `0d954376`) | 501 | `1baea45864ac5aa63c37df2f662ec135702aad384fd3fbbde0b88b3ac959e72f` |
| `A4-CLAUDE-FX02-CONSUMO` | literal de `docs/initiatives/I-62-A-4.md` §7, L355, en HEAD `d5b454fd` (blob `0d954376`) | 473 | `74f9a5a2b796fab2a6baeb77e8c5dbe83f81796bc01b4211f2cbaacecff8f0c4` |
| `A4-PRINCIPAL-A2-CONSUMO` | redacción de la supervisión por §63.2 y §63.8; **sujeta al texto acordado de A4-6** (corrección 3 de A-4, en preparación en paralelo): el texto final se alineará literalmente con ella y su hash cambiará | 309 | `07bfff0a1800045f44a0ad2c54d1f2710dc3a1dafc5b8285c0c566a739065ac7` (borrador) |

Si la corrección 3 de A-4 cambia el texto de §7, las dos primeras líneas se vuelven a tomar, literales, del blob acordado, y se recalculan sus hashes.

## 2. Qué autoriza cada línea y qué no

- Cada línea autoriza **solo consumo**: no acepta ningún resultado por adelantado ni amplía OD-3, OD-5 ni CLAUDE-CLI-I62.
- `A4-SONDA-CONSUMO`: la invocación de solo lectura de `codex-cli` de A4-1 (F6-OBS-03), fuera de A2-P2 y sin reintento.
- `A4-CLAUDE-FX02-CONSUMO`: `claude-cli` como Architect de la Topología A de FX-02 (A4-2), dentro de los mismos topes de D.3.
- `A4-PRINCIPAL-A2-CONSUMO`: una sesión física adicional de Principal de la ronda A, por encima del tope de D.3, solo para A2. §63.2: R cuenta,
  A + R = 2 sesiones usadas y A2 sería la tercera; R no se reclasifica y ningún presupuesto se reinicia. La sesión no es fungible: no sirve para otro
  rol, escenario ni reapertura. P-07 la cuenta antes de lanzarla. A2 no se lanza sin las dos autoridades de §63.2: la revisión independiente del
  Architect del delta A4-6 (con A-4 acordada) y esta autorización.

## 3. Cuándo tiene efecto

1. **Solo después del acuerdo de A-4** (con A4-6): re-revisión formal acreditada por `claude-cli` y decisión del Coordinator (§63.6: sin AGREED
   anticipado), **y** de la disposición del Coordinator que nombre cada aplicación: para A4-1, celda, shell, trío exacto y `-C`; para A4-2, la
   medición custodiada y la clasificación BELOW_REQUIRED de la celda sustituida; para A4-6, A2. Así lo exigen A-4 §7 («solo tienen efecto con A-4
   AGREED y la disposición del Coordinator que nombre cada aplicación») y §63.8 («Solo entran en efecto tras el acuerdo de la enmienda
   correspondiente»). Una respuesta del Owner antes de eso no surte efecto.
2. **El silencio no es aprobación** (§62.5, §63.8). Sin una respuesta expresa, la línea no está autorizada. Cada línea se responde y se registra por
   separado, con la respuesta literal del Owner en `docs/automation/decisions/I-62.md`.
3. **Hoy no se solicita nada** (§62.5): A-4 sigue PROPUESTA y en revisión.

## 4. Antes de enviar

- [ ] Corrección 3 de A-4 publicada (blob nuevo) con A4-6, guardas PASS, re-revisión formal acreditada y AGREED del Coordinator.
- [ ] Línea 3 alineada literalmente con el texto acordado de A4-6; hash recalculado.
- [ ] Líneas 1 y 2 comprobadas, literales, contra el blob acordado de A-4.
- [ ] Disposiciones del Coordinator que nombran cada aplicación.
- [ ] Una sola solicitud con las tres líneas, sin abreviar ningún texto a pegar.
