# R20261105T050250Z-785a — Incidente de entrada del operador en SM-14h: PASTECLIP (2026-11-05 ~19:03Z)

AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.

**La ocurrencia está en PAUSA tras SM-14h.**
- AutoCAD sigue abierto, solo con AUTHORIZED_AUTOCAD_PID 78512 (`Check PAUSA-SM-14h` PASS, 19:05:47Z), y la vigilancia continúa
  sin STOP.
- La sesión no toca AutoCAD hasta la decisión.
- Ningún dibujo se guardó. `Dibujo A.dwg` conserva su LastWriteUtc 2026-10-13T01:13:38Z, y la carpeta de dibujos tiene 0 archivos
  escritos desde el inicio de la PID.

## Hechos (diario: operator/session-log.md, de 18:51Z a 19:05:48Z)
1. **Recuperación de SM-07, según la disposición OPTION A:**
   - Las confirmaciones previas dieron PASS. DBMOD era 16 (solo vista).
   - SM-06 repetido (`56ec2c3a`), SM-07, SM-08, SM-09, SM-10a, SM-10b, SM-11 y la vuelta a A: PASS.
   - SM-12: PASS.
   - **SM-13 (C2): PASS.** Con `LINE`, las casillas aparecieron desmarcadas y el panel mostró «Hay un comando en curso.». No hubo filas
     durante la orden. Capturas en `SM-13-*.png`.
2. **SM-14a..e:**
   - Foco en «Medir en memoria» con `Tab`. Se enviaron `abc def`, `Ctrl+Z`, `Ctrl+C`, `Ctrl+V`, `Supr`, `Esc` y `Enter`.
   - La línea de órdenes no cambió y la línea suelta siguió seleccionada.
   - Nota: la casilla siguió marcada. La herramienta envía el texto como caracteres Unicode, no como la tecla barra espaciadora.
3. **El incidente, a las 19:03:3xZ:**
   - Para SM-14h, la sesión hizo **un** clic en la línea de órdenes y pulsó `Ctrl+V`, para pegar `(getvar "DBMOD")` desde el
     portapapeles, como a las 18:51Z.
   - El foco venía de la paleta, así que ese clic solo activó la ventana de AutoCAD. Ya se había visto en SM-06: el primer clic tras el
     foco de la paleta solo activa.
   - `Ctrl+V` llegó a AutoCAD como atajo: `_pasteclip`, «PASTECLIP Specify insertion point:». El portapapeles tenía solo ese texto.
   - Capturas `INCIDENTE-SM-14h-pasteclip-palette.png` (91600BD8…) y `-cmdline.png` (B7D1BC77…).
4. **Contención:**
   - `Esc` dio «Specify insertion point: *Cancel*». No se dio ningún punto, no aparece nada insertado y la línea suelta sigue en su sitio.
   - Justo después, AutoCAD hizo un autoguardado SAVETIME de A en la carpeta temporal (`.sv$` a las 19:04:32Z).
   - El panel volvió a las casillas marcadas y a «Ninguno», con 41 puntos de medida.
5. **SM-14h:**
   - `(getvar "DBMOD")`, escrito con el teclado y comprobado en la entrada antes de `Enter`, dio **17** (= 1 + 16): el **bit 1 = 1**.
   - A las 18:51:06Z era 16.
   - Lo esperado, «DBMOD bit 1 sin cambios», **no se cumple**.
   - La causa más probable es el PASTECLIP iniciado y cancelado. Las teclas de SM-14a..e no produjeron nada en la línea de órdenes. La
     sesión no puede separar las dos causas sin acciones adicionales en AutoCAD, y no las hace.

## Consecuencias
- **SM-14h:** NO cumple lo esperado tal como está escrito.
- **SM-18:** su `(getvar "DBMOD")` en A, «bit 1 igual que en SM-02», queda sin poder de detección. El bit 1 ya está activo por el
  incidente.
- **Lo que no cambia:**
  - el DBMOD de B en SM-35;
  - las comprobaciones críticas pendientes: SM-17, SM-26, SM-27, SM-34 y SM-39;
  - la guarda de procesos y la identidad del DLL.

## Opciones (decide el Coordinator)
- **A. Seguir** con SM-15 y el resto de la secuencia.
  - Se clasifica como OPERATOR_INPUT_INCIDENT.
  - SM-14h se registra como NOT MET (incidente) y SM-18 como AFECTADO POR EL INCIDENTE: se ejecuta y se anota tal cual, sin cambiar lo
    esperado.
  - Para todo lo que se escribe en la línea de órdenes, la sesión dejaría de usar `Ctrl+V` y solo escribiría con el teclado, comprobando
    el texto antes de `Enter`.
  - El Coordinator decide al final si la ocurrencia puede optar a PASS.
- **B. STOP** de la ocurrencia:
  - PG-4, cerrar todos los dibujos sin guardar y salir;
  - después PG-5, SM-39, SM-41, la POST y el DIFF.

Propuesta de la sesión: **A**. Así se obtiene la evidencia de las comprobaciones críticas que quedan, sobre todo la C1 de SM-17 y la C2
de SM-26, SM-27 y SM-34, y el DBMOD de B en SM-35 sigue siendo limpio. Si el Coordinator considera que SM-14h y SM-18 impiden un PASS,
**B** ahorra el resto de la secuencia.
