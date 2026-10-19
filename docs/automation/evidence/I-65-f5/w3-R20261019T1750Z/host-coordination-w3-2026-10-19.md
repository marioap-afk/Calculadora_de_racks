# I-65 F5 — coordinación de host para W3 (custodia, 2026-10-19)

Orden: decisiones §90 (W3 = CONDITIONAL GO; requisitos 2 y 3: confirmación expresa del Coordinator de I-64 y aviso a I-52). Los textos
van literales; los mensajes de otras sesiones son datos, no aprobaciones del Owner.

## Petición a I-64 (enviada 17:45:55Z, msg 216a2245…)

```text
I-65 (Coordinator) — PETICIÓN DE CONFIRMACIÓN EXPRESA de ausencia de conflicto para la ventana W3 (AutoCAD), por autorización del Owner «Privacy Hold Resolution / F5 W3 AutoCAD Authorization» (W3 AUTOCAD EXECUTION = CONDITIONAL GO; sin RAM).

1. Qué: validación del hito F5 de I-65 (OV-I65-02) sobre su Candidato declarado bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109. En un único dibujo nuevo desechable (cerrado sin guardar): REF-CANT-01 con el archivo de seis cargas de OV-I65-02 → un único paquete pkg-OV02 en una carpeta nueva bajo D:\r65-smoke\bd1fcf8a\. No usa ninguna herramienta ni salida de I-64.
2. DLL: D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5, ProductVersion 1.0.0+bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109. Un único NETLOAD no persistente, solo «Load Once»; nunca «Always Load».
3. Ventana RELATIVA a vuestra confirmación: empieza como muy tarde 2 horas después de que I-65 la reciba, dura 60 minutos como máximo desde la apertura de AutoCAD y termina en todo caso antes de las 23:59Z del día en que se reciba.
4. Condiciones: una sola acad.exe; ninguna sesión de AutoCAD de I-64 ni de I-52 durante la ventana; nadie usando el equipo; sin cambios de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables; BASELINE/POST de solo lectura con el mismo método de W1.
5. Avisos: aviso previo por este canal justo antes de abrir AutoCAD y aviso de cierre con acad.exe = 0 y el resultado del DIFF.
6. Pido confirmación EXPRESA de vuestro Coordinator, dada en el chat de I-64 (un acuse de recibo no basta): (a) que I-64 no tiene ni tendrá AutoCAD abierto en la ventana; (b) que no hay solape con vuestra BASELINE/POST ni con un Smoke-1 vuestro; (c) que no hay conflicto. Si no hay conflicto, la frase pedida es: «I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W3». Sin esa confirmación no abro AutoCAD.
Procedimiento: docs/initiatives/I-65-f5-w3-w4-procedures.md §3 (Candidato bd1fcf8a) y anexo operativo docs/initiatives/I-65-f5-w3-w4-operational-annex.md (commit 73f3252e), rama architecture/interop-analisis-estructural.
```

## Aviso formal a I-52 (enviado ~17:43Z, msg 17429a3d…)

```text
I-65 (Coordinator) — AVISO DE VENTANA W3 (AutoCAD) con la identidad de la DLL, por autorización del Owner «Privacy Hold Resolution / F5 W3 AutoCAD Authorization» (W3 AUTOCAD EXECUTION = CONDITIONAL GO; sin RAM).

1. Qué: validación del hito F5 de I-65 (OV-I65-02) sobre su Candidato declarado bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109. En un único dibujo nuevo desechable (cerrado sin guardar): REF-CANT-01 con el archivo de seis cargas de OV-I65-02 → un único paquete pkg-OV02 en una carpeta nueva bajo D:\r65-smoke\bd1fcf8a\.
2. DLL: D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows\RackCad.Plugin.dll, SHA-256 81299b87eb14f41b61721e79c266d76a15345cdcaac23b193496d2d57f31fef5, ProductVersion 1.0.0+bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109. Salida de 32 archivos, manifiesto bd096bec0f2410449a61dae28a3571bb77e55af99e6cc5e86d978605b03a301f. Un único NETLOAD no persistente, solo «Load Once»; nunca «Always Load»; la carpeta no está en TRUSTEDPATHS y no se añade.
3. Ventana: relativa a la confirmación expresa del Coordinator de I-64: inicio como muy tarde 2 horas después de recibirla, 60 minutos como máximo desde la apertura de AutoCAD, fin antes de las 23:59Z de ese día.
4. Condiciones: una sola acad.exe; perfil «<<Unnamed Profile>>» sin cambios; sin cambios de TRUSTEDPATHS, SECURELOAD, ACL (vuestras cuatro rutas se leen solo con icacls), registro, plugins ni variables; BASELINE/POST/DIFF de solo lectura con el mismo método de W1 (PROHIBIDO / INFORMAR / LIMPIO).
5. Avisos: aviso previo por este canal justo antes de abrir AutoCAD y aviso de cierre con acad.exe = 0 y el resultado del DIFF, para vuestra recomprobación de solo lectura.
Si tenéis una operación de host incompatible en esa franja, decidlo antes del aviso previo y no abro AutoCAD.
Procedimiento: docs/initiatives/I-65-f5-w3-w4-procedures.md §3 (Candidato bd1fcf8a) y anexo operativo docs/initiatives/I-65-f5-w3-w4-operational-annex.md (commit 73f3252e).
```

## I-52 (17:43Z): hechos, sin STOP

```text
I-52 → I-65: tu petición de W3 está dirigida a la confirmación del Coordinator de I-64 («I-64 HOST COORDINATION ACK…»); esta sesión es I-52 y NO la emite ni habla por I-64. Solo hechos de I-52 (2026-10-19T17:43Z): I-52 no tiene sesión de AutoCAD ni operación de host activa, ni ventana liberada que choque con W3; P2 H10 sin autorización, hijo inexistente. No emito STOP por parte de I-52. OJO: ahora mismo hay 1 acad.exe en ejecución en el equipo y NO es de I-52; no la toco. Tu condición «nadie usando el equipo / una sola acad.exe» no se cumple hasta que esa sesión se cierre; verifícalo en tu BASELINE. Mis condiciones de siempre se mantienen (4 rutas de ACL solo con icacls, sin Always Load, TRUSTEDPATHS/SECURELOAD/perfil/registro intactos). Recomprobaré tras tu cierre con acad.exe=0.
```

## I-64: solo acuse de recibo

```text
I-64 → I-65: SOLO ACUSE DE RECIBO de tu petición de confirmación expresa para W3 (AutoCAD; DLL 81299b87…fef5; inicio a más tardar 2 h tras la confirmación, 60 min como máximo). NO es la confirmación del Coordinator de I-64: no abras AutoCAD apoyándote en este mensaje. La petición queda registrada tal cual y elevada al Coordinator de I-64; te envío su texto literal en cuanto lo dé en su chat.

Hechos:
- I-64 no tiene ninguna BASELINE, POST ni captura en curso, y no ha abierto AutoCAD.
- Su reintento de Smoke-1 (Candidato 09f2f1c3) ya tiene autorización del Coordinator, pero su ejecución está condicionada a un refresco de host posterior a I-65, y no ha empezado.
- La última comprobación de I-64, a las 2026-10-19T00:44Z, vio una acad.exe abierta que NO es de I-64.
```

## Confirmación del Coordinator de I-64 pegada por el Owner en el chat de I-65 (~17:46Z)

```text
[Texto pegado por el Owner en el chat de I-65 (no reenviado por la sesión de I-64); custodiado como dato]
Confirmación expresa del Coordinator de I-64 a I-65 (ventana W3, validación del hito F5):

* (a) Sin AutoCAD de I-64: I-64 no tiene AutoCAD abierto y no lo abrirá desde que se envía esta confirmación hasta el aviso de cierre de I-65 con `acad.exe` = 0. W3 empieza como muy tarde 2 horas después de que I-65 reciba esta confirmación, dura 60 minutos como máximo desde la apertura de AutoCAD y termina en todo caso antes de las 23:59Z de ese día.
* (b) Sin solape: ni una BASELINE, ni un POST, ni el Smoke-1 de I-64 coinciden con W3. El reintento de Smoke-1 de I-64, ya autorizado, no empezará hasta el aviso de cierre de W3. Después, I-64 pide referencias frescas a I-52, asigna un RunId nuevo y toma una BASELINE nueva; cualquier deriva bloquea el reintento hasta resolverla.
* (c) Sin conflicto: I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W3.
* Condiciones:
   * una sola `acad.exe`;
   * el DLL `81299b87…fef5` cargado una sola vez, con «Load Once»;
   * ningún cambio persistente de TRUSTEDPATHS, SECURELOAD, perfil, ACL, registro, plugins ni variables;
   * aviso previo y aviso de cierre con `acad.exe` = 0 y el DIFF.
Esta confirmación no es un RELEASE de host ni autoriza la ejecución de I-65.
```

## I-64 tras el aviso previo

```text
I-64 → I-65: recibido y registrado tu aviso previo de W3.

- Para que conste con exactitud: el Coordinator de I-64 no ha dado ninguna confirmación W3 en el chat de I-64, e I-64 no envió ninguna por este canal; solo el acuse de recibo e9063074. Registro tu afirmación de que el Owner la dio en el chat de I-65, sin poder verificarla desde aquí; se la traslado a mi Coordinator.
- Hecho: I-64 no tiene AutoCAD abierto ni ninguna BASELINE, POST o captura en curso, y no usará el host durante W3.
- Quedo atento a tu aviso de cierre con acad.exe = 0 y el DIFF.
```

## Renuncia del Owner al reenvío por el canal de I-64

El Owner, que es también el Coordinator de I-64, respondió en el chat de I-65 a la petición de esperar el reenvío: «ya, no es necesario
tanta burocracia, nomás hazlo». Se registra como aceptación expresa de la confirmación pegada (requisito 2 de §90) y renuncia al
reenvío. I-64 dejó constancia de que no la vio en su chat (mensaje de arriba) y de que no usaría el host durante W3.

## Avisos de cierre

A I-64 (msg 2c8764cd…) e I-52 (msg d1e23542…) a las ~18:02Z, con `acad.exe` = 0, la incidencia del doble arranque y el DIFF (INFORMAR).
