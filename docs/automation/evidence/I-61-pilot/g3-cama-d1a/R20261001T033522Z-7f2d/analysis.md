# Análisis — STOP P-02 (huérfano) en el relevo de entrada del Worker (g3-cama-d1a, R20261001T033522Z-7f2d)

Redacta: Coordinator de I-61 (SAME-SESSION ROLE). AUTOMATION_PLAN 16.11 / Freeze §10.2. P-02 no es materia OWNER-RESERVED.

## Hecho

Relevo de entrada (2026-10-01T03:52:01Z, `processes-entry.json`) del Worker subagente. Cesión de 03:38:18Z a 03:51:53Z.

- Lo correcto: `HEAD` = `ls-remote` = `b8dfc900`, árbol limpio, sin operación Git en curso, `origin/main` = `95690c28` y hash de `config.toml` sin cambio.
- Descendientes de la sesión (raíz `claude.exe` 664): solo el `conhost.exe` 25652, el mismo de la salida. Ningún descendiente nuevo.
- Un proceso de la clase `orphan`: `bash.exe` PID 35912, creado a las 03:49:14Z (dentro de la cesión), con el padre 30476 ya inexistente y vivo en la entrada. Por la regla de
  huérfanos de AUTOMATION_PLAN 16.4 / Freeze §3.3 es **STOP (P-02)**.

## Causa raíz (medida)

1. Su línea de órdenes es legible: `…\Git\usr\bin\bash.exe ./runinj.sh injections2b.txt`. No contiene la ruta del worktree.
2. `runinj.sh` e `injections2b.txt` no existen en el worktree (búsqueda recursiva, incluidos los archivos ignorados). La ruta relativa `./runinj.sh` descarta que su directorio de
   trabajo sea el worktree.
3. Un segundo antes (03:49:13Z) arrancó una cadena `bash.exe` 32160 → 31912 cuyo padre es `claude.exe` 28012, otra sesión de la app de escritorio, hermana de la sesión 664 bajo el
   proceso principal 25960. Es el patrón de un script lanzado en segundo plano por esa sesión.
4. La transcripción completa del Worker (`agent-ad242c3d2bad732d9.jsonl`, workflow `wf_306d36ac-e20`) no contiene `runinj`. Todos los procesos que lanza el Worker cuelgan de la
   sesión 664, y en la entrada no queda ninguno nuevo.

Conclusión: el proceso pertenece a otra sesión de la misma máquina. No es participante sobre el worktree ni lo dejó el Worker. La regla congelada de huérfanos no distingue por
sesión de origen: atrapa cualquier proceso de la lista cerrada nacido durante la cesión cuyo padre terminó.

## Decisión del Coordinator

1. El STOP queda resuelto sin cambio del trabajo, del contrato ni de las autoridades. No consume `attempts` ni ningún tope. No hubo escritura Git de la sesión desde el push del
   Worker.
2. La salida del Worker sigue siendo válida: la sesión no operó durante la cesión, lo que descarta P-08.
3. Se registra como parte de **DEV-G3-02**, la medición del relevo. Se **propone**, sin bloquear, para la decisión de G3: acotar el criterio de huérfano a procesos sin línea legible,
   o con la ruta del worktree, o cuyo origen aparezca en la transcripción del Worker.
4. Se sigue con el relevo de entrada: entrega, hechos remotos y verificación del Controller.
