# Análisis — STOP P-02 en el relevo de salida de la planificación (g3-cama-d1a, R20261001T032333Z-4a2d)

Redacta: Coordinator de I-61 (SAME-SESSION ROLE). AUTOMATION_PLAN 16.11 / Freeze §10.2: un STOP exige causa raíz y decisión del Coordinator, o del Owner si la materia
es OWNER-RESERVED. P-02 no es OWNER-RESERVED (a diferencia de P-01).

## Hecho

Primer relevo de salida (2026-10-01T03:27:00Z, `processes-exit-1.json`, SHA-256 `6AC48534…84129F2`): siete `git.exe` con la línea de órdenes ilegible, clase `unattributable`.
Por AUTOMATION_PLAN 16.4 / Freeze §3.3 es **STOP (P-02)**. **No se lanzó ninguna invocación.** El resto del relevo de salida estaba bien: árbol limpio, `HEAD` = `ls-remote` =
`b677cb57`, `origin/main` = `95690c28`, hash de `config.toml` sin cambio.

## Causa raíz (medida)

- Los padres de seis de los siete eran `git.exe` cuya raíz es `claude.exe` PID 27584, un proceso de la app de escritorio de Claude (hijo del proceso principal 25960). Esta rama
  es hermana de la cadena del comprobador, no ancestro suyo. El séptimo tenía un padre ya terminado (22740).
- Un muestreo de 40 s (03:28:30-03:29:10Z, `git-sampler.ps1` en el scratchpad) mostró dos apps del host que lanzan `git.exe` de corta vida aproximadamente cada 2 s:
  - la app del Owner `ChatGPT.exe` PID 3892, padre también de los `codex.exe` del Owner, con `status --no-renames --ignored`, `ls-files --others` y `config --null`;
  - la app de escritorio de Claude (`claude.exe` 27584), con `-c core.quotepath=false -c safe.directory=*`.
- En ningún caso la línea legible contiene la ruta del worktree. Las ilegibles lo son solo en el instante del muestreo: la siguiente lectura las encuentra legibles o ya terminadas.
- Conclusión: no hay participante ni proceso vivo no atribuible. Una foto única de CIM capta, con cierta frecuencia, procesos en creación o terminación cuya línea de órdenes todavía no
  es legible. La regla de §3.3 no fija el número de lecturas.

## Decisión del Coordinator

1. El STOP queda resuelto sin cambio del trabajo, del contrato ni de las autoridades. No consume `attempts` (Freeze §9) ni el tope de invocaciones (no hubo invocación).
2. Medición, desde ahora y en todos los relevos de I-61:
   - un proceso de la lista cerrada con línea ilegible se **vuelve a leer por PID a los 2 s**;
   - si ya no existe, o el PID tiene otra `CreationDate`, no está vivo;
   - si es legible, se clasifica con la regla general;
   - si sigue vivo e ilegible, es `unattributable` y STOP (P-02).

   Los descartados quedan en `processes-<lado>.json.transient.json`. La regla y sus consecuencias no cambian; cambia solo la medición de «ilegible». Se registra como **DEV-G3-02** y
   se **propone**, sin bloquear, aclarar el README §3.2 en la decisión de G3.
3. Se repite el relevo de salida completo antes de lanzar al Controller, con el mismo `RunId`: no hubo invocación que reejecutar.
