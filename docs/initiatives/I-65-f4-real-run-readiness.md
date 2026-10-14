# I-65 — F4: preparación de la primera validación real (DIF, decisiones, condiciones y recomendación)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation Preparation»
([texto literal](../automation/decisions/I-65-owner-disposition-f4-engineering-validation-preparation.original.txt); decisiones
[§47](../automation/decisions/I-65.md)). Cubre los puntos 4-7 del entregable; los puntos 1-3 son:

1. [Hoja de validación de ingeniería](I-65-f4-engineering-validation-sheet.md).
2. [Procedimiento manual `REF-CANT-01`](I-65-f4-ref-cant-01-manual-procedure.md).
3. [Procedimiento de smoke en AutoCAD](I-65-f4-autocad-smoke-procedure.md).

> IMPLEMENTATION AUTHORIZATION = NO. RAM EXECUTION = NO. AUTOCAD HOST EXECUTION = NO. F4 sin GATE PASS; sin Candidato de hito. Nada de
> este documento se ejecutó.

## 1. Estado de DIF-01 a DIF-04

| # | Diferencia | Magnitud | Estado tras la disposición | Siguiente paso |
|---|---|---|---|---|
| DIF-01 | Peso unitario: RAM 486.94 pcf (≈ 7800 kg/m³) frente a AISC 490 pcf, en A992 Gr50 y en A500 GrC rectangular | −0.62 % (fuera de ±0.5 %) | **RECONOCIDA y ACEPTADA como diferencia informativa solo para la primera validación de equivalencia sin peso propio** (disposición §3). La tolerancia no cambia; la fila de material sigue fuera de ±0.5 % y se registra así | ninguno para la primera validación; cualquier análisis futuro con peso propio la reabre |
| DIF-02 | G esperado 11 200 ksi frente al G implícito de RAM 11 153.85 ksi (solo si la fuente del ingeniero da G y no ν) | −0.41 % | **ABIERTA**, sujeta a revisión | el ingeniero fija el oráculo (ν o G) con su cita; con ν = 0.3 desaparece; con G, reconocimiento del Owner |
| DIF-03 | Base de secciones de usuario «Montilla» registrada en la configuración de RAM, no localizada | desconocida | **ABIERTA**, sujeta a revisión | revisarla en la interfaz de RAM (exige autorizar abrir RAM, OD-F4-02) o excluirla expresamente de la corrida (OD-F4-10) |
| DIF-04 | Edición de AISC de la base de RAM no declarada en sus archivos | sin delta en A, Ix, Iy ni J | **ABIERTA**, sujeta a revisión | el ingeniero la registra en la hoja (§6) |

## 2. Decisiones pendientes del Owner

| ID | Decisión | Recomendación del Coordinator |
|---|---|---|
| OD-F4-02 | Designar al ingeniero verificador, cuándo firma y si se autoriza abrir RAM **solo** para verificar las filas (paso B del paquete: sin adaptador, File > New, cerrar sin guardar) | designarlo ya; la hoja puede firmarse contra las fuentes de §4.1 y la pre-lectura, y abrir RAM para B confirmaría además DIF-03 y la interfaz |
| OD-F4-03 | Confirmar como definitiva Q-O2 = (b) (base de RAM como fuente de propiedades, comparación ±0.5 % informativa), hoy provisional | confirmarla cuando la hoja esté firmada |
| OD-F4-04 (resto) | Oráculo de material ν o G y, si es G, reconocer DIF-02 | decidir con la cita del ingeniero; DIF-01 ya quedó resuelta para la primera validación |
| OD-F4-07 | Sonda previa P2 en RAM (giro, descripción, banderas parciales) | no hacerla: lo miden M-01..M-03 dentro de OV-I65-01 |
| OD-F4-08 | Quién opera RAM y atiende la licencia | Owner o ingeniero a mano; la sesión prepara scripts y mide hashes |
| OD-F4-09 | Cómo se obtiene `REF-CANT-01` en un DWG | dibujarlo con `RACKCANTILEVER` con los parámetros del [smoke](I-65-f4-autocad-smoke-procedure.md) §6 en el mismo smoke y conservar ese DWG de prueba solo si el Owner lo pide |
| OD-F4-10 | Base de usuario «Montilla» | excluirla de la corrida si contiene los mismos nombres |
| OD-F4-11 | Costo del Candidato de hito en F4 y repetición de OV-I65-01..07 sobre el Candidato final; cómo se repite el modelo manual | aceptar el costo; reutilizar el volcado manual y la hoja firmada si el hash de RAM y de su base no cambian |
| **OD-F4-12** | Autorizar el **cambio de datos F4-D** (escribir las dos filas `verified` tras la firma), con alcance de rutas y presupuesto propios: hoy `attempts` = 6 de 6 y Codex 41 de 46 | autorizarlo **después** de la firma, como una cadena RED → GREEN con T4-18, con `attempts` nuevos asignados por el Owner y su propio tope de Codex |
| **OD-F4-13** | Autorizar el smoke de AutoCAD: sobre qué build (exploratorio con el SHA-256 del DLL, o solo sobre un Candidato) y en qué ventana de host respecto de I-52 e I-64; **con qué método** se toman la línea base y el POST + diff del host que exige I-52 (SP-07: hoy I-65 no tiene ninguno y las herramientas de I-64 no se usan sin su Coordinator) | autorizarlo sobre un build exploratorio trazable (no es Owner Validation) en una ventana con ACK + RELEASE de I-52 (decidido por su Coordinator, el Owner o el CAD manager) y de I-64; método de host: pedir al Coordinator de I-64 que autorice el uso de sus herramientas en solo lectura, o que I-52 tome su propia línea base y POST, en lugar de crear herramientas nuevas; no depende de las filas, puede ir antes de F4-D |
| **OD-F4-14** | Cómo concluye OV-I65-01 si las combinaciones quedan **NO EVALUADAS** (disposición §4): el comparador da `INCOMPLETE`, nunca PASS, y AC-F4-07 pedía OV-I65-01 PASS | decidirlo **antes** de la corrida: registrar la OV como `INCOMPLETE` con todas las filas evaluadas en PASS, la fila de combinaciones NO EVALUADA con la captura del ingeniero, y que el Owner fije si eso basta para el GATE PASS de F4 o exige medir M-05 primero; sin cambiar tolerancias ni el Freeze sin una A-n |
| — | Autorización específica de la primera corrida en RAM (pasos C-F del paquete y, si se incluyen, OV-I65-03..07) | pedirla solo cuando se cumplan las condiciones de §3 |
| — | Confirmación definitiva de Q-O1 y Q-O7 | tras OV-I65-01 |

## 3. Condiciones exactas para autorizar la primera corrida real

La corrida real (paquete generado → script en RAM → modelo manual → volcados → comparación) puede autorizarse cuando **todas** se cumplan:

| # | Condición | Evidencia que la cierra |
|---|---|---|
| CR-01 | Las dos tarjetas de la [hoja](I-65-f4-engineering-validation-sheet.md) §6 firmadas por el ingeniero designado (OD-F4-02), con fuentes citadas, resultado CONFORME o CONFORME CON DIFERENCIAS RECONOCIDAS | hoja firmada y capturas custodiadas |
| CR-02 | DIF-02 resuelta (oráculo con cita; reconocimiento si es G), DIF-03 revisada o excluida (OD-F4-10), DIF-04 registrada; OD-F4-03 confirmada | decisiones del Owner registradas literalmente |
| CR-03 | F4-D autorizado (OD-F4-12) y terminado: tabla con las dos filas `verified`, RED → GREEN con T4-18, verificación y CI verde del SHA exacto | contrato, verificación y CI |
| CR-04 | Smoke de AutoCAD PASS (A65-F4R-08) en una ventana con ACK + RELEASE de I-52 e I-64, línea base y POST + diff del host con el método de SP-07 y sin deriva | registro del smoke (§10 del procedimiento) |
| CR-05 | Candidato de hito declarado (LIFECYCLE §8: suite Core y UI completas en local, CI del SHA exacto, SHA-256 del DLL) con la tabla verificada, por orden del Owner | declaración del Candidato |
| CR-06 | Paquete válido de `REF-CANT-01` (estación 0, archivo de cargas `P1`-`P3`) generado con `RACKRAMPAQUETE` desde el Candidato: diagnósticos vacíos, tres archivos, SHA-256 de cada uno y `AnalysisInputFingerprint` registrados; DWG obtenido según OD-F4-09; ventana de host con I-52 e I-64 | paquete custodiado |
| CR-07 | Procedimiento manual aceptado por el ingeniero; operador y licencia de RAM asignados (OD-F4-08); RAM 26.00.01.015 con el hash de P1 | registro de precondiciones PM-01..PM-05 |
| CR-08 | Regla de combinaciones fijada: comparador con `--generated-combinations-unread`, `--manual-combinations-unread` y `--combinations-decision` del ingeniero salvo M-05 medido; forma de concluir OV-I65-01 decidida (OD-F4-14) | decisión del Owner |
| CR-09 | Plan de ejecución y paradas confirmados: paquete §3 C-H, volcado del generado y del manual **en la misma sesión, sin guardar ni reabrir** (A65-F4R-02), un paquete nuevo por intento, todo `FAILED` descartado sin guardar | plan aprobado |
| CR-10 | Presupuesto para atender lo que la corrida revele (hoy 0 `attempts` y 5 invocaciones de Codex bajo el tope de 46) | asignación del Owner |
| CR-11 | Autorización específica del Owner: RAM EXECUTION = YES, con alcance (pasos C-F; OV-I65-03..07 si se incluyen) | orden del Owner |

## 4. Recomendación GO / NO-GO

- **Primera corrida real en RAM: NO-GO.** Hoy no puede producirse un paquete válido: las filas siguen `proposed` (el comando responde
  `Unsupported` con `MATERIAL-ROW-UNVERIFIED`), el comando nunca se ejecutó en AutoCAD, no hay Candidato y no queda presupuesto de
  `attempts` para F4-D. Faltan CR-01..CR-11.
- **Siguientes pasos de preparación: GO**, en este orden:
  1. OD-F4-02: designar al ingeniero y firmar la hoja (puede hacerse sin RAM; abrir RAM para el paso B requiere tu autorización).
  2. OD-F4-13: autorizar el smoke de AutoCAD cuando I-52 e I-64 liberen el host; no depende de las filas.
  3. OD-F4-12: tras la firma, autorizar F4-D con presupuesto propio.
  4. CR-05 y CR-06: Candidato de hito y paquete válido.
  5. OD-F4-14 y CR-11: decidir la conclusión con combinaciones NO EVALUADAS y autorizar la corrida en RAM.
