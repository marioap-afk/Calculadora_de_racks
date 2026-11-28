# Traspaso del autor: la v2.3 del paquete, la guía del Smoke-PB contratado y el guion de la emisión

**Encargo:** aplicar las siete decisiones del Owner (`owner-pbt5-decisions.md`, sha256 `2d63b10d2c7ab15ef6e573df0ee69e895ea678946937a6f09b89101611f12a45`, leído entero) y rellenar lo [T4e] con el cierre de PB-T4e, leído por objetos git en `ebd304a3773495fa9071de6611936c4c01a7b06c` (F4B-E61, §291). Reglas: solo en `S\pbt5\`, el repositorio por objetos git, sin AutoCAD, sin compilar y sin commits.

## Lo entregado
- **`I-64-f4b-pbt5-package-v2.3.md`:**
  - lo genera `_tools/make_pkg_v23.py` desde la v2.2, con la cabecera y la cola nuevas (`_tools/pkg_v23_head.md` y `pkg_v23_tail.md`) y 10 filas y 5 textos de §4;
  - lo rellena `_tools/fill_t4e_pkg_v23.py`, con el cierre de PB-T4e y OWNER-01 de PB-T4e en SPB-31, SPB-36 y la trazabilidad.
- **`smoke-run-v1.md`:** lo genera `_tools/make_smoke_run_v1.py` desde `pre-run-tonight-v2.4.md` y los fragmentos `_tools/sr_head.md`, `sr_new4.md` y `sr_56.md`. Dos rótulos se corrigieron después con `sed` (la cabecera de la tabla de §3 y un comentario «33 copias» que pasa a 44).
- **`_tools/emit_pbt5.py`:** el guion de la emisión, con `--dry` y `--commit`. **No se ha ejecutado:** solo se compiló en memoria (`_tools/syntax_check.py`).
- **Comprobaciones,** sin ejecutar nada del Smoke-PB:
  - los 20 bloques de PowerShell de `smoke-run-v1.md` pasan el analizador (`_tools/ps_parse_any.ps1`): 0 errores, con `StrictMode`, `Stop` y `exit 0`, y sin variables libres;
  - las 22 líneas LISP miden menos de 1024 caracteres y tienen los paréntesis cerrados;
  - la privacidad da 0 coincidencias no admitidas (`_tools/check_pbt5.py`; `sha256-v3.txt`).

## Lo leído en `ebd304a3`
- **F4B-E61:**
  - PB-T4e CLOSED / PASS WITH LIMITATIONS sobre el GREEN `0eac7911`, intento 1 de 3;
  - CI del GREEN `45123886125`, 4/4: Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48;
  - OWNER-01 de PB-T4e (informativo): la paleta no se repinta con la ventana de «Editar todo» abierta;
  - el registro declarado de PB-T4e, en EJECUTADA.
- **La numeración:** la última disposición es F4B-E61 y la última sección, la 291. La emisión de PB-T5 es F4B-E62 con la §292.
- **Los 19 filtros de UI,** de `RequiredTests` del contrato de PB-T4e (`_tools/list_ui_filters.py`, por objetos git).
- **La CI de push del Candidato:** `45147867500`, que estaba `in_progress` cuando la leí con `gh run list`, solo en lectura y desde el checkout principal.

## Lo que no se aplicó literal, con su razón
1. **Q-PBT5-2.** Si ningún dibujo guarda un estilo, SPB-07 **vuelve al Owner**: no queda como NOT AVAILABLE. Es la letra de la opción elegida («si ninguno, te vuelve»), que corrige la forma de la v2.2.
2. **Q-PBT5-8 y nc2.** Con la forma B no hay nc2, así que la v3 de su sustituto, elegida «para PB-T5» en F4B-E58, no se usa. Se declara en §6.1.
3. **El Candidato es `ebd304a3`, y la emisión es un commit encima.**
   - El build va antes del commit de la emisión, porque la `InformationalVersion` tiene que identificar el Candidato.
   - Por eso el bloque 10 (SPB-51) ya no exige que HEAD sea el Candidato: exige que el Candidato sea antepasado de HEAD, sin cambios en `src/`, `tests/` ni `assets/`.
   - El bloque 1 sí sigue exigiendo HEAD = Candidato, porque corre antes de la emisión.
4. **La forma de la emisión.** Smoke-2 no tuvo emisión que imitar, y la de PB-T4e eran dos commits (X y BASE) porque generaba un `gate-contract.json`. Propongo la forma mínima: **un solo commit documental**, F4B-E62 y §292, con:
   - el paquete en `docs/initiatives/I-64-f4b-pbt5-package.md`;
   - el registro del Owner en `F4B-DRAFTS/coordinator/`;
   - el contrato (el §6 del paquete, sacado tal cual) en `F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md`;
   - la historia y las herramientas en `F4B-DRAFTS/pbt5/`;
   - y los cambios de `I-64.yml`.

   Es el supuesto S-12 del paquete.
5. **Lo añadido por la sesión, declarado:**
   - el `TaskId`, `F4B-PBT5-SMOKE-PB-GATE-CLOSE`;
   - el nombre del paquete en el repositorio, sin «gate-contract», porque no hay contrato de gate JSON;
   - INV-PBT5-17;
   - PBT5-C16 (la revisión de la evidencia encuentra discrepancias) y PBT5-C17 (no se cumple una condición de la emisión);
   - la presentación elegida para SPB-45, la primera de `(layoutlist)`;
   - y el orden de la corrida, que deja las inyecciones al final, cada una en su AutoCAD.
6. **La extracción de `dimkeys.py`** (bloque 14) usa `git show … > archivo`. En PowerShell 7.4 y posteriores esa redirección conserva los bytes; aquí corre la 7.7. Después se comprueba con `git hash-object` frente al blob, y si no casa, el bloque para.
7. **`--commit` no es atómico.** Hace todas sus comprobaciones antes de escribir nada, pero si fallara a mitad de la escritura (un disco lleno, por ejemplo), quedaría un estado parcial en el worktree, sin commit. La sesión lo vería en `git status`.

## Lo que queda
- **La CI de push del Candidato, `45147867500`,** tiene que terminar 4/4. El `--dry` para si no está en `success`.
- **La comprobación acotada de la v2.3,** con una línea `BOUNDED_RESULT:` que dé EMITIBLE y B 0 / I 0. El guion la exige.
- **Sin verificar en AutoCAD:**
  - las órdenes nativas de los contextos construidos y sus respuestas;
  - `QUIT` en cada inyección;
  - `rcdyn` y `rcdynchk`;
  - `vla-item` por nombre;
  - `WaitForInputIdle`.
- **La lista local de los originales de la selección de Smoke-2 para P-09** (`<lista-local-de-la-seleccion>`) la prepara la sesión. No está en el repositorio.
