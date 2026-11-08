# aa47 — Análisis de la línea WATCH STOP (07:13:05Z)

**Línea, sin cambios, en `process/acad-watch.log`:**
`2026-11-08T07:13:05.2044435Z WATCH STOP: otra instancia 39252@2026-11-08T07:06:56.3271404Z`

**Hechos:**
- La «otra instancia» es la misma PID autorizada 39252, con la misma hora de inicio (ticks 639297184163271404).
- La vigilancia anota cada cambio del conjunto de procesos acad. Solo hay una línea de conjunto (07:07:20Z, `[39252@…]`): nunca apareció otra PID.
- Momento: 11 s después de CHECK PRE-CLOSE PASS (07:12:54Z), durante la salida por `QUIT` (unos 07:12:58Z). CLOSED PASS a las 07:13:12Z.
- CHECK HS-04, HS-06, HS-08 y PRE-CLOSE: los cuatro PASS, con una sola acad.exe.

**Mecanismo:**
- `Test-AcadProcessGuard.ps1` (copia idéntica del kit de 6862) compara la identidad por PID, ticks de inicio y `Path`.
- Mientras un proceso sale, `Get-Process` sigue dando su Id y su StartTime, pero `Path` puede volver vacío. Entonces `SameInstance` es falso y la guarda escribe «otra instancia».
- En 6862 la PID desapareció entre dos sondeos, y la vigilancia anotó la salida tras PRE-CLOSE sin STOP.

**Sonda inocua** (`scratchpad/hs65/teardown_probe.ps1`):
- Montaje: 25 procesos `PING.EXE` de vida corta, sondeados igual que la guarda.
- Resultado, dos ejecuciones:
  - la primera, en la consola: 3281 muestras; en **8**, el Id y el inicio eran legibles pero la ruta salía vacía (por ejemplo, `pid 61680 … path=''` cuando la esperada era `<System32>\PING.EXE`);
  - la segunda, guardada en `logs/teardown-probe-output.txt`: 2945 muestras y otra vez **8**.
- El mecanismo queda reproducido. No se ha medido sobre la propia acad.exe, que ya no existe.

**Lectura propuesta:** falso positivo de la guarda al cerrarse la PID autorizada. No es una segunda instancia ni una PID ambigua. Lo clasifica el Coordinator.

**Consecuencia en el kit:** PG-6 de `owner-post.ps1` se negó a tomar el POST (`console/post-pg6-refusal.txt`). El procedimiento aprobado manda POST y DIFF también tras un STOP. Por eso el POST se tomó con `kit/owner-post-after-watch-stop.ps1`, que tiene una sola sustitución contada, registrada en `kit/generation-record-post.txt`: lista las líneas STOP en vez de parar y conserva el resto de comprobaciones. Desviación declarada.
