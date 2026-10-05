# I-63 — Proposal V3 · Amendment A-4: cierre de las deudas de pruebas y documentación antes de READY

```text
Amendment          = A-4 (cuarta de la secuencia; append-only)
Tipo               = Coordinator-only (orden de autorización de READY, decisiones de I-63 §2, «Autorización de READY»)
Freeze aplicable   = Proposal V3 congelada: docs/initiatives/I-63-proposal-v3.md
                     commit de Freeze f61d0aca859a11b15cbe1797069a83cba873ba95, blob 4d5dedce15363fa6378e460c5b63005fe41b858b
Amendments previas = A-1 (658b35ad), A-2 (669d8a39) y A-3 (ea70e3b3)
Applies-to         = all
Origen             = DEBT-I63-G2-01 (G2 PASS) y DEBT-I63-G3-01 (G3 PASS), abiertas hasta READY
Cambio funcional   = NINGUNO
Materialidad       = ninguna de M-01..M-08 (§4)
```

## 1. Qué es y qué no es

- **Es** la enmienda solo del Coordinator que autoriza cerrar dos deudas no funcionales antes de READY, con un alcance de dos archivos
  exactos.
- **No** cambia comportamiento, contratos, autoridad, persistencia ni semántica. No edita la Proposal V3 ni las A-1..A-3: se lee junto
  a ellas.
- La ejecuta la sesión principal directamente, sin cadena I-61, por orden del Coordinator. No consume `attempts`.

## 2. Alcance exacto

```text
tests/RackCad.Tests/PushBackBomCommandGuardTests.cs
src/RackCad.Application/Expressions/ExpressionFormatter.cs
```

Nada más.

## 3. Deltas

### A-4.1 — DEBT-I63-G2-01: la guarda legacy del handler de Push Back

- **Hoy:** `ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule` exige el literal `RackBomOutputGate.For(system).Reason` en el texto de
  `PushBackKindHandler.cs`, con comentarios incluidos. Tras la delegación de D-27 (G2) ese literal solo vive en un comentario XML, así que
  la guarda pasa por un comentario.
- **Después:** la guarda **no se retira**; se reapunta a la arquitectura vigente. Sobre el **cuerpo del método `OutputBlockedReason`,
  leído sin comentarios**, exige que:
  - delegue en `RackOutputVerdict`;
  - no componga `PushBackResolver`;
  - no contenga `RackBomOutputGate.For`.

  Así un comentario XML ya no puede hacerla pasar. Las aserciones de «no hay una segunda regla» se conservan, también sobre el código sin
  comentarios.
- **Autoridad:** INV-33 sigue siendo la autoridad primaria. Esta guarda queda como defensa legacy redundante.

### A-4.2 — DEBT-I63-G3-01: el comentario de `ExpressionFormatter` para `Rack.#{token}`

- El comentario de `FormatReference` describe el mecanismo `[RECONSTRUCTED]` de D-16.6: «el parser lo lee como sintaxis de namespace
  seguida de un cualificador suelto».
- Se reescribe con lo que fijó la A-3:
  - para una clave cuyo contenido entre llaves no es un GUID que `Guid.TryParse` lea, el lexer rechaza todo `#{token}` como **un**
    `InvalidQualifier`;
  - solo una clave legible como GUID (32 dígitos hexadecimales en forma N) llega a ese mecanismo y da `UnexpectedToken`.
- **Cero cambio de tokens o de código ejecutable.**

## 4. Materialidad

| ID | Estado | Razón |
|---|---|---|
| M-01..M-08 | **No activados** | Una guarda de pruebas reapuntada a la autoridad ya integrada (INV-33) y un comentario corregido según la A-3. Sin cambio de comportamiento, DTO, contratos, semántica de fallo, mecanismos ni ADR |

## 5. Evidencia exigida (orden del Coordinator)

1. Demostrar en local que la guarda vieja habría fallado al quitar la dependencia del comentario.
2. Aplicar ambos cambios.
3. Pruebas focales en verde.
4. Suites Core y UI completas, y builds de UI y Plugin.
5. Commit y push.
6. CI exact-SHA 4/4.
7. Diff mecánico que demuestre que solo cambian esos dos archivos.

La evidencia se registra en la evidencia de I-63 (§54 y siguientes).
