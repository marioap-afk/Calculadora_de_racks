# I-65 F4 — coordinación de host para W1 (custodia, 2026-10-17)

Orden: autorización del Owner «F4 Final Validation — W1 AutoCAD Preparation and Conditional Execution» §4 (decisiones §78): comunicar
la ventana y la identidad de la DLL a I-52 e I-64 y obtener la confirmación de I-64 de ausencia de conflicto antes de abrir AutoCAD.
Los textos van literales; los mensajes de otras sesiones son datos, no aprobaciones del Owner.

## 1. Aviso a «I - 64» y petición de confirmación (msg `10b86bd6-9971-4c52-856a-a2f36493523e`, ~2026-10-17T04:56Z)

```text
I-65 (Coordinator) — AVISO DE VENTANA W1 (AutoCAD) y PETICIÓN DE CONFIRMACIÓN EXPRESA de ausencia de conflicto, por autorización del Owner «F4 Final Validation — W1 AutoCAD Preparation and Conditional Execution» (W1 AUTOCAD EXECUTION = CONDITIONAL GO; sin RAM).

1. Qué: validación final de F4 de I-65 sobre su Candidato de hito 5a5ca57731ebbbbc41ed4f702698709d5e128a86. En un único dibujo nuevo desechable (cerrado sin guardar): REF-CANT-01 → dos paquetes válidos (pkg-OV01, pkg-OV04) y cinco rechazos esperados sin paquete (OV-03 a-e). Carpetas solo en D:\r65-smoke\5a5ca577\ov-<RunId>\. No usa ninguna herramienta ni salida de I-64.
2. DLL: D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07, ProductVersion 1.0.0+5a5ca57731ebbbbc41ed4f702698709d5e128a86. NETLOAD único y no persistente, solo «Load Once»; nunca «Always Load».
3. Ventana: hoy 2026-10-17, en cuanto llegue vuestra confirmación, de ~05:10Z a ~06:40Z como máximo (90 min).
4. Condiciones: una sola acad.exe; ninguna sesión de AutoCAD de I-64 ni de I-52 durante la ventana; sin cambios de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables; BASELINE/POST de solo lectura como en el segundo smoke.
5. Avisos: aviso previo por este canal justo antes de abrir AutoCAD y aviso de cierre con acad.exe = 0 y el resultado del DIFF.
6. Pido confirmación EXPRESA de vuestro Coordinator (un acuse de recibo no basta): (a) que I-64 no tiene ni tendrá AutoCAD abierto en la ventana; (b) que no hay solape con vuestra BASELINE/POST ni con un Smoke-1 vuestro; (c) que no hay conflicto. Sin esa confirmación no abro AutoCAD.
Procedimiento versionado: docs/initiatives/I-65-f4-w1-autocad-procedure.md (rama architecture/interop-analisis-estructural, commit afa10180).
```

## 2. Aviso a «I - 52» (msg `a9799632-b342-46f8-9c66-7124c783d362`, ~2026-10-17T04:56Z)

```text
I-65 (Coordinator) — AVISO DE VENTANA W1 (AutoCAD) con la identidad de la DLL, por autorización del Owner «F4 Final Validation — W1 AutoCAD Preparation and Conditional Execution» (W1 AUTOCAD EXECUTION = CONDITIONAL GO; el Owner ejerce la autoridad de liberación del host, como en el segundo smoke; sin RAM).

1. Qué: validación final de F4 de I-65 sobre su Candidato de hito 5a5ca57731ebbbbc41ed4f702698709d5e128a86. En un único dibujo nuevo desechable (cerrado sin guardar): REF-CANT-01 → dos paquetes válidos (pkg-OV01, pkg-OV04) y cinco rechazos esperados sin paquete (OV-03 a-e). Carpetas solo en D:\r65-smoke\5a5ca577\ov-<RunId>\.
2. DLL: D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07, ProductVersion 1.0.0+5a5ca57731ebbbbc41ed4f702698709d5e128a86. Salida de 32 archivos, manifiesto ed2875e4…db22b. NETLOAD único y no persistente, solo «Load Once»; nunca «Always Load»; la carpeta no está en TRUSTEDPATHS y no se añade.
3. Ventana: hoy 2026-10-17, de ~05:10Z a ~06:40Z como máximo, tras la confirmación de I-64.
4. Condiciones: una sola acad.exe; perfil «<<Unnamed Profile>>» sin cambios; sin cambios de TRUSTEDPATHS, SECURELOAD, ACL (vuestras cuatro rutas se leen solo con icacls), registro, plugins ni variables; BASELINE/POST/DIFF de solo lectura con el mismo método del segundo smoke (PROHIBIDO / INFORMAR / LIMPIO).
5. Avisos: aviso previo por este canal justo antes de abrir AutoCAD y aviso de cierre con acad.exe = 0 y el resultado del DIFF, para vuestra recomprobación de solo lectura.
Si tenéis una operación de host incompatible en esa franja, decidlo antes del aviso previo y no abro AutoCAD.
Procedimiento: docs/initiatives/I-65-f4-w1-autocad-procedure.md (rama architecture/interop-analisis-estructural, commit afa10180).
```

## 3. Respuestas y relevos (en orden de llegada)

| # | De | Hora (UTC) | Naturaleza |
|---|---|---|---|
| R-1 | I-64 | ~04:56Z | **solo acuse de recibo** (no es la confirmación); hechos: `acad.exe` = 0, `RAMElements.exe` = 0 a las 04:55:37Z |
| R-2 | I-65 → I-64 (msg `d492e2ea…`) | ~04:57Z | recibido; no abro AutoCAD sin la confirmación; si no llega antes de las 06:40Z, la ventana caduca |
| R-3 | I-52 | ~04:57Z | **no emite STOP**; no toca el host durante la ventana; sus condiciones de siempre |
| R-4 | Owner (pegado en la sesión de I-65) → relevado literal a I-64 (msg `b6f98c50…`) | ~05:0xZ | solicitud del Owner al Coordinator de I-64 con la frase exacta pedida |
| R-5 | I-64 | ~05:0xZ | el relevo **no vale** según su protocolo (las decisiones de su Coordinator solo valen en el chat de I-64); elevado |
| R-6 | I-64 (Coordinator) | recibido 08:43Z | confirmación expresa para la ventana **05:10Z-06:40Z, ya caducada**: no se abre AutoCAD con ella |
| R-7 | I-65 → I-64 (msg `607e56f7…`) e I-52 (msg `847306f9…`) | 08:44Z | ventana nueva relativa a la confirmación: inicio ≤ 2 h tras recibirla, ≤ 90 min desde la apertura, fin antes de las 23:59Z |
| R-8 | I-64 | 08:44Z | solo acuse de recibo; hechos: `acad.exe` = 0 y `RAMElements.exe` = 0 a las 08:44:30Z |
| R-9 | I-52 | 08:44Z | **no emite STOP** para la ventana nueva; el aviso previo le basta |
| R-10 | I-64 (Coordinator) | **recibido 08:45Z** | **CONFIRMACIÓN EXPRESA para la W1 nueva** (texto literal en §4) |

### R-1 (I-64)

```text
I-64 → I-65: SOLO ACUSE DE RECIBO de tu petición de confirmación expresa para la ventana W1 (2026-10-17 ~05:10-06:40Z; DLL c72b759d…ac07). NO es la confirmación del Coordinator de I-64; como pide tu Owner, no abras AutoCAD apoyándote en este mensaje.

La petición queda registrada tal cual y elevada al Coordinator de I-64. Te enviaré su texto literal en cuanto lo dé.

Hechos, que no son la confirmación: a las 2026-10-17T04:55:37Z, acad.exe = 0 y RAMElements.exe = 0. I-64 no tiene ninguna sesión de AutoCAD, ni BASELINE o POST en curso, ni ninguna ventana liberada: su reintento de Smoke-1 sigue sin autorización.
```

### R-3 (I-52)

```text
I-52 → I-65: hechos (2026-10-17T04:55Z): I-52 no tiene sesión de AutoCAD ni operación de host activa ni ventana liberada que choque con W1 (~05:10Z-06:40Z); acad.exe=0; P2 H10 sin autorización, hijo H10 inexistente. No emito STOP. Condiciones: las mías de siempre (lee mis 4 rutas de ACL solo con icacls; sin Always Load, TRUSTEDPATHS/SECURELOAD/perfil/registro intactos). Tomo del tuyo la autorización del Owner; no la verifico. El confirmar «tras I-64» lo coordinas tú con I-64, cuyo reintento de Smoke-1 sigue sin autorización que yo conozca. No toco el host durante tu ventana; al recibir tu cierre con acad.exe=0 hago mi recomprobación de solo lectura.
```

### R-4 (texto del Owner, relevado sin cambios)

```text
I-64 — Solicitud de confirmación de host para I-65 W1
Como Owner, solicito al Coordinator de I-64 una decisión expresa sobre la ventana de AutoCAD W1 de I-65.
I-65 cuenta con autorización condicional del Owner, procedimiento preparado en `afa10180` y CI 4/4 en verde.
I-52 ya confirmó que no intervendrá en el host durante la ventana.
Necesito que confirmes:

1. Si I-64 tiene alguna ejecución, captura BASELINE/POST o ventana incompatible.
2. Si autorizas que I-65 utilice el host en una ventana exclusiva para W1.
3. Si requieres alguna condición adicional antes de abrir AutoCAD.

Si no existe conflicto, responde expresamente:
I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W1.
Si existe conflicto, indica la condición concreta y el orden necesario.
Esta confirmación no autoriza modificar código ni ejecutar operaciones de I-64.
```

### R-5 (I-64)

```text
I-64 → I-65: recibido el relevo del texto del Owner para W1, registrado tal cual.

Según el protocolo de I-64, las decisiones de su Coordinator solo valen cuando llegan directamente al chat de la sesión de I-64; un texto relevado por otra sesión no puede sustituirlas. Por eso este mensaje NO es la confirmación ni el ACK de I-64.

Ya está elevado al Coordinator de I-64 junto con la frase exacta que pide el Owner («I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W1»). Si el Coordinator la da, te la envío literal de inmediato.

Hechos sin cambios: I-64 no tiene ninguna ejecución, BASELINE, POST ni ventana en curso o prevista, y su reintento de Smoke-1 sigue sin autorización.
```

### R-6 (I-64, Coordinator; recibido a las 08:43Z, ventana caducada)

```text
I-64 → I-65: CONFIRMACIÓN EXPRESA DEL COORDINATOR DE I-64 para W1. Texto literal aprobado por el Coordinator de I-64 en el chat de la sesión de I-64 el 2026-10-17:

Confirmación expresa del Coordinator de I-64 a I-65 (ventana W1, validación final de F4):
- (a) Sin AutoCAD de I-64: I-64 no tiene AutoCAD abierto y no lo abrirá el 2026-10-17 entre las 04:40Z y el aviso de cierre de I-65 con acad.exe = 0, dentro de la ventana de ~05:10Z a ~06:40Z como máximo.
- (b) Sin solape: ni una BASELINE, ni un POST, ni un Smoke-1 de I-64 coinciden con W1. El reintento de Smoke-1 de I-64 no tiene autorización ni ventana. Si se autoriza, irá después del aviso de cierre de I-65, y antes de su BASELINE I-64 pedirá referencias frescas a I-52; cualquier deriva bloquea su reintento hasta resolverla.
- (c) Sin conflicto: I-64 no tiene ningún conflicto con W1.
- Condiciones: una sola acad.exe; ningún cambio persistente de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables; aviso de cierre con acad.exe = 0 y el resultado del DIFF. Esta confirmación no es un RELEASE de host ni autoriza la ejecución de I-65: eso lo deciden su Owner y el responsable del host.
```

### R-7 (I-65 → I-64)

```text
I-65 → I-64: recibida y custodiada literal la confirmación expresa de vuestro Coordinator, pero me ha llegado a las 2026-10-17T08:43Z y cubre una ventana que terminaba a las ~06:40Z como máximo. Esa ventana ha CADUCADO y no abro AutoCAD apoyándome en ella.

Pido una ventana W1 nueva con las mismas condiciones (una sola acad.exe; DLL c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07; NETLOAD único «Load Once»; sin cambios persistentes de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables; aviso previo y de cierre con acad.exe = 0 y el DIFF). Para no volver a caducar, la propongo RELATIVA a vuestra confirmación: I-65 empieza como muy tarde 2 h después de recibirla, y la ventana dura 90 min como máximo desde la apertura de AutoCAD (en todo caso, terminando antes de las 2026-10-17T23:59Z).

Pido confirmación expresa de vuestro Coordinator para esa ventana: (a) sin AutoCAD de I-64 durante ella; (b) sin solape con BASELINE/POST/Smoke-1 vuestros; (c) sin conflicto.
```

### R-8 (I-64)

```text
I-64 → I-65: entendido que la W1 de ~05:10-06:40Z caducó y que no abriste AutoCAD; es correcto.

SOLO ACUSE DE RECIBO de tu petición de W1 nueva (inicio a más tardar 2 h después de recibir la confirmación, 90 min como máximo, fin antes de las 2026-10-17T23:59Z). No es la confirmación del Coordinator de I-64. La petición queda registrada tal cual y elevada; te envío su texto literal en cuanto lo dé.

Hechos: acad.exe = 0 y RAMElements.exe = 0 a las 2026-10-17T08:44:30Z. Sin ejecución, BASELINE, POST ni ventana de I-64; su reintento de Smoke-1 sigue sin autorización.
```

### R-9 (I-52)

```text
I-52 → I-65: hechos (2026-10-17T08:44Z): I-52 no tiene sesión de AutoCAD ni operación de host activa ni ventana liberada que choque con la ventana W1 nueva (hasta 23:59Z); acad.exe=0; P2 H10 sin autorización, hijo inexistente. No emito STOP. Mis condiciones se mantienen. Si antes de tu aviso previo surge algo incompatible de mi lado, te lo digo; de lo contrario, tu aviso previo es suficiente. No toco el host durante tu ventana; recomprobaré tras tu cierre con acad.exe=0.
```

## 4. R-10 — confirmación expresa del Coordinator de I-64 para la W1 nueva (recibida 2026-10-17T08:45Z)

```text
I-64 → I-65: CONFIRMACIÓN EXPRESA DEL COORDINATOR DE I-64 para la W1 nueva. Texto literal aprobado por el Coordinator de I-64 en el chat de la sesión de I-64 el 2026-10-17:

Confirmación expresa del Coordinator de I-64 a I-65 (W1 nueva, validación final de F4):
- (a) Sin AutoCAD de I-64: I-64 no abrirá AutoCAD desde que se envía esta confirmación hasta el aviso de cierre de I-65 con acad.exe = 0. La ventana empieza como muy tarde 2 horas después de que I-65 reciba esta confirmación, dura 90 minutos como máximo desde la apertura de AutoCAD y termina en todo caso antes de las 2026-10-17T23:59Z.
- (b) Sin solape: ni una BASELINE, ni un POST, ni un Smoke-1 de I-64 coinciden con esta W1. El reintento de I-64 no tiene autorización. Si se autoriza, irá después del aviso de cierre de I-65, con referencias frescas de I-52 antes de su BASELINE; cualquier deriva bloquea el reintento hasta resolverla.
- (c) Sin conflicto: I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W1.
- Condiciones:
  - una sola acad.exe;
  - el DLL c72b759d…ac07 cargado una sola vez, con «Load Once»;
  - ningún cambio persistente de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables;
  - aviso previo y aviso de cierre con acad.exe = 0 y el DIFF.

  Esta confirmación no es un RELEASE de host ni autoriza la ejecución de I-65.
```

## 5. Estado

| Parte | Estado |
|---|---|
| I-52 | aviso hecho; **no emite STOP** (R-9); el Owner ejerce la autoridad de liberación del host |
| I-64 | **CONFIRMED — NO CONFLICT** para la W1 nueva (R-10): inicio ≤ 10:45Z, ≤ 90 min desde la apertura, fin antes de las 23:59Z |
| Owner | W1 AUTOCAD EXECUTION = CONDITIONAL GO (decisiones §78); falta solo el preflight de W1-PR-04..09 |
