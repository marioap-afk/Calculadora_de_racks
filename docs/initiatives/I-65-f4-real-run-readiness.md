# I-65 — F4: preparación de la primera validación real (DIF, decisiones, condiciones y secuencia)

Documento de la unidad (Coordinator). Versión 2 (2026-10-14), tras la disposición del Owner «F4 Engineering Validation and Host
Readiness» ([texto literal](../automation/decisions/I-65-owner-disposition-f4-validation-host-readiness.original.txt); decisiones
[§49](../automation/decisions/I-65.md)). La versión 1 está en `71baf43b` (decisiones §47).

Documentos de la preparación:

1. [Hoja de validación de ingeniería](I-65-f4-engineering-validation-sheet.md) (lista para la validación del Owner).
2. [Procedimiento manual `REF-CANT-01`](I-65-f4-ref-cant-01-manual-procedure.md).
3. [Procedimiento de smoke en AutoCAD](I-65-f4-autocad-smoke-procedure.md) y
   [solicitud de liberación de host y preflight/POST](I-65-f4-host-release-and-preflight.md).
4. [Resolución propuesta de OD-F4-14](I-65-f4-od-f4-14-combinations.md).
5. [Contrato propuesto de la actualización a `verified`](I-65-f4-d-mapping-verified-contract.md).

> ENGINEERING PREPARATION = APPROVED. MAPPING UPDATE = PENDING ENGINEERING APPROVAL. AUTOCAD EXECUTION = NOT AUTHORIZED. RAM
> EXECUTION = NOT AUTHORIZED. F4 GATE PASS = NOT AUTHORIZED. Nada de este documento se ejecutó.

## 1. Estado de DIF-01 a DIF-04

| # | Diferencia | Estado |
|---|---|---|
| DIF-01 | Peso unitario de RAM 486.94 pcf frente a 490 pcf (−0.62 %) | **Aceptada** solo como diferencia informativa del piloto sin peso propio; la tolerancia no cambia |
| DIF-02 | ν o G | **Pendiente**: el Owner elige el oráculo y su fuente (hoja §5.1; recomendado ν = 0.3, que coincide exacto con RAM) |
| DIF-03 | Base de usuario «Montilla» | **Dispuesta**: no es fuente de autoridad del piloto mientras no se identifique. En la corrida, la sección se toma de la base instalada (`United States`); si la interfaz muestra «Montilla» como base activa con los mismos nombres → STOP |
| DIF-04 | Edición del catálogo de RAM | **Evidencia reunida, confirmación pendiente** (hoja §4): las 289 W y 126 HSS cuadradas de v16.0 están en RAM con valores idénticos; los archivos no declaran la edición; el Owner decide si basta (V-01) **antes** de verificar las filas |

## 2. Decisiones del Owner

| ID | Estado |
|---|---|
| OD-F4-02 | Owner verificador **provisional**; sin firma ni fecha atribuidas hasta su confirmación |
| OD-F4-03 | **Aceptada** (base de RAM como representación, contrastada con fuentes externas) |
| OD-F4-04 (resto) | pendiente: DIF-02 |
| OD-F4-07, 08, 09, 10, 11 | abiertas (10 queda dispuesta por DIF-03) |
| OD-F4-12 | **CONDITIONAL APPROVAL**: contrato preparado, no emitido ni ejecutado; reserva provisional Codex 48 y `max_attempts` 9 **no habilitada** (decisiones §50) |
| OD-F4-13 | HOST COORDINATION = AUTHORIZED: solicitud formal **enviada** a I-52 e I-64 (decisiones §50); sin ACK ni RELEASE todavía; preflight/POST definitivo y validado en seco |
| OD-F4-14 | **M-05c ejecutada (2026-10-14): lectura de combinaciones VERIFICADA** ([custodia](../automation/evidence/I-65-f4/m05c/m05c-report.md)); resolución propuesta: CMB-3 en la primera corrida, con su condición de validez ([documento](I-65-f4-od-f4-14-combinations.md) §5), pendiente de decisión del Owner |
| — | autorización de la corrida en RAM; Q-O1 y Q-O7 tras OV-I65-01 |

## 3. Condiciones exactas para autorizar la primera corrida real

| # | Condición |
|---|---|
| CR-01 | Tarjeta de validación del Owner confirmada para las dos filas (hoja §6; V-01..V-09) |
| CR-02 | DIF-02 resuelta, DIF-04 aceptada, DIF-03 dispuesta; OD-F4-03 aceptada (cumplida) |
| CR-03 | F4-D autorizado y terminado: tabla `verified`, RED → GREEN, verificación, CI verde del SHA exacto |
| CR-04 | Smoke de AutoCAD PASS **con las filas `proposed`**, en una ventana con ACK + RELEASE de I-52 e I-64 y con BASELINE/POST/DIFF `LIMPIO` o `INFORMAR` aceptado por I-52 |
| CR-05 | Candidato de hito declarado por orden del Owner (suites Core y UI completas en local, CI del SHA exacto, SHA-256 del DLL) con la tabla `verified` |
| CR-06 | Paquete válido de `REF-CANT-01` generado con `RACKRAMPAQUETE` desde el Candidato, en una ventana de host liberada, con su DWG (OD-F4-09) |
| CR-07 | Operador y licencia de RAM asignados (OD-F4-08); RAM 26.00.01.015 con el hash de P1 |
| CR-08 | OD-F4-14 decidida: M-05c ya verificó la lectura de combinaciones; falta que el Owner adopte la resolución propuesta (CMB-3 con condición de validez) |
| CR-09 | Plan y paradas confirmados: volcados en la misma sesión sin guardar ni reabrir (A65-F4R-02), un paquete nuevo por intento, todo `FAILED` descartado sin guardar |
| CR-10 | Presupuesto para atender lo que revele la corrida (hoy 0 `attempts`; Codex sujeto al tope que fije el Owner) |
| CR-11 | Autorización específica: RAM EXECUTION = YES, con alcance (pasos C-F del paquete, M-05c y, si se incluyen, OV-I65-03..07) |

## 4. Secuencia mínima hasta la primera corrida real

| Paso | Qué | Quién | Depende de |
|---|---|---|---|
| 1 | Validar la hoja: edición del catálogo de RAM (V-01), oráculo ν o G (V-02), citas, geometría externa y datos de la fila (V-03..V-09) | Owner | — |
| 2 | Autorizar el envío de la solicitud de host (§1 del documento de host) | Owner | — |
| 3 | ACK + RELEASE de I-52 (su Coordinator, el Owner o el CAD manager) y de I-64 | I-52, I-64 | 2 |
| 4 | Smoke con las filas `proposed`: build del commit identificado, BASELINE, smoke, POST y DIFF | sesión y operador | 3 y autorización de AUTOCAD EXECUTION |
| 5 | Emitir y ejecutar F4-D con presupuesto propio (`verified`) | Owner autoriza; Worker y Controller | 1 y 4 |
| 6 | Declarar el Candidato de hito con la tabla `verified` | Owner ordena; sesión prepara | 5 |
| 7 | Decidir OD-F4-14 (y M-05c) y OD-F4-08 / 09 / 11 | Owner | — (antes de 9) |
| 8 | Ventana de host para generar el paquete de `REF-CANT-01` desde el Candidato (puede ser el mismo día que 9) | I-52, I-64 | 6 |
| 9 | Corrida en RAM: script generado y su volcado, modelo manual y su volcado, M-05c si se autoriza, comparación y matriz | operador y sesión | 7, 8 y autorización de RAM EXECUTION |

Los pasos 1-3 y 7 pueden ir en paralelo. Hacen falta **dos** ventanas de AutoCAD (4 y 8) porque el smoke exige las filas `proposed` y el
paquete válido exige las filas `verified`.

## 5. Recomendación

- **Primera corrida real en RAM: NO-GO** hasta cumplir CR-01..CR-11.
- **Siguiente paso: GO** para los pasos 1, 2 y 7, que no ejecutan nada sobre el host.
