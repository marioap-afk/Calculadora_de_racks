# R20261107T171052Z-6862 — Pausa en SM-29: el gesto del Owner fue más allá del arrastre autorizado (2026-11-07 ~18:45Z)

AutoCAD operated by implementation session under Coordinator exception for this occurrence, except the SM-29 middle-button drag (OWNER_MANUAL_GESTURE_EXCEPTION — SM-29 only).

**La ocurrencia está en PAUSA antes de terminar SM-29.**
- AutoCAD sigue abierto, solo con AUTHORIZED_AUTOCAD_PID 67260 (`Check PAUSA-SM-29` PASS, 18:45:21Z), y la vigilancia de 1 s sigue viva y sin STOP.
- La sesión no toca AutoCAD hasta la decisión.
- Dibujos abiertos: C (activo) y B. A y D ya se cerraron sin guardar en SM-26.

## Hechos (diario: `operator/session-log.md`, de 18:35:43Z a 18:45:22Z)

1. **Antes del gesto:** rueda arriba y abajo por la sesión, PASS. V0 = `(getvar "VIEWCTR")` = **(3344.54 1493.04 0.0)**. Capturas `SM-29-antes-*`.
2. **Primera petición (~18:36Z):** la sesión pidió el arrastre en el chat y esperó 130 s sin enviar entradas. La vista no cambió. Probablemente el mensaje no era legible mientras la herramienta tenía el control; la sesión no lo puede demostrar.
3. **Segunda petición (18:38:45Z):** la sesión terminó el turno para pedirlo. Instrucción dada: un arrastre, una vez, sin clics izquierdos ni teclado; traer AutoCAD al frente solo desde la barra de tareas.
4. **ChatGPT en el monitor principal.** El Owner informó, literal: «Se abrio ChatGpt en el monitor principal». A las 18:40:53Z figuraba minimizada. Probablemente, al terminar el turno, la herramienta devolvió las ventanas que oculta; es una interpretación, no una medición. No se tomó ninguna captura en ese estado.
5. **Respuesta del Owner, literal:** «hecho, la vista siguió al ratón y la paleta no parpadeó».
6. **Lo que apareció en AutoCAD entre V0 y V1**, que la sesión no envió (en ese intervalo solo tomó capturas):
   ```text
   Command:
   Command: *Cancel*
   Command: Specify opposite corner or [Fence/WPolygon/CPolygon]:
   Command: Specify opposite corner or [Fence/WPolygon/CPolygon]:
   Command: *Cancel*
   ```
   Además cambió la escala de la vista: un rack mide unos 64 px en el marco, frente a unos 40 antes.
7. **Respuesta del Owner a la pregunta de la sesión** (opciones marcadas): **«Clic izquierdo», «Tecla Esc», «Giré la rueda».** No marcó «Solo el arrastre».
8. **Después:** clic de la sesión en la línea de órdenes para recuperar el foco; V1 = **(3079.37 1205.26 0.0)**. Capturas `SM-29-despues-palette.png` (9D5CF378) y `SM-29-despues-cmdline.png` (84B38502), abiertas y revisadas: limpias.

## Consecuencias, según la lectura de la sesión

- **La autorización dice:** «The Owner must perform only the requested drag and no additional AutoCAD action.» Eso **no se cumplió**: hubo además clics izquierdos, Esc y rueda.
- **S1-02:**
  - A favor: el arrastre se hizo físicamente y el Owner observó, literal, que la vista siguió al ratón y que la paleta no parpadeó.
  - En contra: **V1 ≠ V0 no aísla el encuadre**, porque la rueda también mueve el centro de la vista.
  - **Foco:** el Owner usó el chat antes de V1, así que la lectura sin clic previo no se pudo hacer; la prueba de «no roba el foco» durante el gesto es solo su observación.
  - La sesión **no marca S1-02 como PASS** por su cuenta.
- **Lo que no cambia:**
  - **Dibujo A:** SM-14h y SM-18 ya están hechas, limpias, antes de esto; A está cerrado sin guardar.
  - **Dibujo C:** es la copia descartable y no tiene ninguna aserción de escritura. Ya estaba modificado por SM-01 y SM-28.
  - **Dibujo B:** sin tocar; su aserción de SM-35 sigue pendiente y limpia.
  - **Selección:** el panel sigue en «Sesión 7», «Ninguno», sin error.
- **Comprobaciones de solo lectura hechas en la pausa:**
  - `disposition/no-save-check-sm29.txt`: A, B, C y D sin escritura desde el inicio de la PID; solo un autoguardado temporal `.sv$` de C.
  - `process/inspect-after-netload.txt` y `process/inspect-at-pause-sm29.txt`: VERDICT PASS (Plugin `3C261B8F…B6A0`, Application y UI MATCH).
  - `logs/racklog-checks.txt`: RackLog igual que BEFORE, sin archivo nuevo.
- **No se ve en esto un fallo del producto.** Es una desviación de la operación, de la instrucción de la sesión y del gesto.

## Causa probable en la instrucción (autocrítica de la sesión)

El método no previó dos cosas:
- que el Owner tendría que activar AutoCAD después de usar el chat;
- que un botón central con rueda gira con facilidad al pulsarlo.

Además, el relevo de ventanas al terminar el turno puso ChatGPT encima de AutoCAD justo antes del gesto.

## Opciones (decide el Coordinator)

- **A. Repetición limpia del arrastre, una vez.** Recomendada por la sesión.
  - Clasificación: OWNER_GESTURE_INCIDENT. Los hechos 1 a 8 se conservan tal cual.
  - Secuencia: la sesión deja AutoCAD al frente; lee V2 con `VIEWCTR` y **`(getvar "VIEWSIZE")`**, lectura añadida que requiere esta autorización y sirve para demostrar que no hubo zoom; termina el turno; el Owner, sin clic izquierdo, sin teclado y sin rueda, hace un solo arrastre; la sesión lee V3 y `VIEWSIZE`.
  - Criterio: V3 ≠ V2 con `VIEWSIZE` igual. Después, capturas, `ZOOM` `E` y `PAN`, y sigue SM-30.
  - Si `VIEWSIZE` cambia otra vez o el arrastre no se puede aislar: vuelve al Coordinator.
- **B. Aceptar la observación actual** como S1-02, con nota, y seguir. La sesión no la recomienda: la evidencia está mezclada, y la autorización pide observar el arrastre de verdad.
- **C. STOP** por SM-STOP-06: cerrar sin guardar, POST y resultado sin PASS posible. La sesión no lo recomienda: el resto de la ocurrencia está limpio y SM-30..SM-41 siguen ejecutables.

La sesión no declara PASS ni continúa hasta la disposición.
