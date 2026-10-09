# I-62 — Solicitud única al Owner: consumo para FX-02 (decisiones §63, punto 8)

> Supervisión, plano a. Sustituye al [borrador](../FX-02/s63/owner-request-draft.md), que se conserva sin cambios. Las tres líneas se toman,
> literales, de A-4 §7 en el blob `5e2ba68efdc1b902aa1c28aff4453b4b6973be7f`, el objeto de la segunda re-revisión formal
> (R20261009T195811Z-2df6: AGREED; auditor ACCREDITED). **El veredicto del Coordinator sobre A-4 sigue PENDING**: A-4 sigue PROPUESTA.

## 1. Texto a pegar (tres líneas exactas)

```text
A4-SONDA-CONSUMO = A (1 invocación read-only de codex-cli fuera de A2-P2, celda gpt-6-luna/high del Controller de FX-02 con la shell declarada cmd.exe, para el trío exacto 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68/26.1002.7124.0/6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32, con huella, binario y versión antes y después; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; no acepta su resultado)
A4-CLAUDE-FX02-CONSUMO = A (uso read-only de claude-cli como Architect de la topología A de FX-02, revisión del contrato de T1, con una celda medida elegible para ARCHITECT; 1 lanzamiento y las reejecuciones del pool de D.3, dentro de los mismos topes; Actor/Session/Context REQUIRED, Provider PREFERRED; sin PRINCIPAL_COORDINATOR, EXECUTION_CONTROLLER ni WORKER; sin escritura; sin lectura de credenciales; no acepta su resultado ni amplía OD-3, OD-5 ni CLAUDE-CLI-I62)
A4-PRINCIPAL-A2-CONSUMO = A (una sesión adicional de Principal, claude-desktop-session, abierta por el Owner para el titular A2 de FX-02 designado por T16, por encima del límite de la ronda A de D.3 que cubre OD-5; solo con A-4 AGREED, A4-6 incluida, y la disposición del Coordinator que nombre la apertura; ninguna otra sesión ni reapertura; no acepta su resultado ni amplía OD-5)
```

| Línea | Fuente | Bytes (UTF-8) | SHA-256 de la línea |
|---|---|---|---|
| `A4-SONDA-CONSUMO` | `docs/initiatives/I-62-A-4.md` §7, L432, blob `5e2ba68e` (commit `c2dbc225`) | 501 | `1baea45864ac5aa63c37df2f662ec135702aad384fd3fbbde0b88b3ac959e72f` |
| `A4-CLAUDE-FX02-CONSUMO` | `docs/initiatives/I-62-A-4.md` §7, L433, blob `5e2ba68e` (commit `c2dbc225`) | 473 | `74f9a5a2b796fab2a6baeb77e8c5dbe83f81796bc01b4211f2cbaacecff8f0c4` |
| `A4-PRINCIPAL-A2-CONSUMO` | `docs/initiatives/I-62-A-4.md` §7, L434, blob `5e2ba68e` (commit `c2dbc225`) | 386 | `a13204bf2af38eec8a27440f949555d104dfbd8ae66fe70d8270bc1376a647ba` |

SHA-256 del bloque de tres líneas (unidas con LF, con LF final): `08ac7dfb78cda4f927b3f41e0244c4df7f3a8619c2e31260bdaea554cd298a06` (1363 bytes).
Las líneas 1 y 2 son byte a byte iguales a las del borrador (tomadas de `0d954376`). La línea 3 es ahora la de A4-6 en el objeto revisado; la del
borrador era una redacción provisional de la supervisión y no vale.

## 2. Qué autoriza cada línea y qué no

- Cada línea autoriza **solo consumo**: no acepta ningún resultado por adelantado ni amplía OD-3, OD-5 ni CLAUDE-CLI-I62 (A-4 §7; la re-revisión
  lo confirma: `OwnerConsumptionLines` = ONLY_CONSUMPTION, `EffectiveOnlyAfterAgreement` = YES).
- `A4-SONDA-CONSUMO`: la invocación de solo lectura de `codex-cli` de A4-1 (F6-OBS-03), fuera de A2-P2, tope uno y sin reintento.
- `A4-CLAUDE-FX02-CONSUMO`: `claude-cli` como Architect de la topología A de FX-02 (A4-2), dentro de los mismos topes de D.3.
- `A4-PRINCIPAL-A2-CONSUMO`: una sesión de Principal más en la ronda A, solo para el titular A2 (A4-6). R cuenta (A + R = 2) y no se reclasifica;
  ningún presupuesto se reinicia; la sesión no es fungible; P-07 la cuenta antes de abrirla.

## 3. Cuándo tiene efecto

1. Cada línea necesita una respuesta expresa del Owner, que se registra literalmente en `docs/automation/decisions/I-62.md`. **El silencio no es
   aprobación** (decisiones §62.5 y §63.8).
2. Una línea respondida solo tiene efecto cuando se cumplen las dos condiciones de A-4 §7: (a) A-4 AGREED por el Coordinator, con la acreditación de
   la re-revisión R20261009T195811Z-2df6 (para la línea 3, con A4-6 incluida); (b) la disposición del Coordinator que nombre cada aplicación (A4-1:
   celda, shell, trío exacto y `-C`; A4-2: medición custodiada y clasificación BELOW_REQUIRED de la celda sustituida; A4-6: la apertura de A2). Una
   respuesta anterior queda registrada y sin efecto hasta entonces.
3. Si el Coordinator no acuerda A-4, excluye A4-6 o acuerda otro blob, la línea afectada caduca y la solicitud se rehace con el texto acordado.
