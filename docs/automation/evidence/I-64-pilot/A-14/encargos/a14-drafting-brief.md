# Encargo: redactar la enmienda I-64 A-14 (v1), replaneación hacia la ventana RackCad sin bloqueo

## Rutas

- W (worktree de I-64, rama `architecture/workspace-persistente-rackcad`): `<worktree-de-I-64>`
- S (carpeta de trabajo): `<carpeta-de-trabajo-de-A-14>`

## Reglas

- **W es SOLO LECTURA.** No escribas, no muevas, no borres nada en W; ninguna orden git que cambie estado (nada de checkout, commit,
  stash, reset, fetch, add). Lectura con Read/Grep/Glob o `git -C <W> show/log/cat-file/ls-tree`. Nunca toques `main` ni el checkout
  principal `<checkout-principal>`.
- Escribe SOLO en S.
- Sin AutoCAD, sin compilar, sin correr pruebas.
- El repo es PÚBLICO: el texto de la enmienda no lleva rutas locales, nombres de personas, correos ni datos de clientes. Cita rutas del
  repo como rutas relativas (`docs/initiatives/...`).
- **Fidelidad:** no inventes campos de cabecera por analogía con otras enmiendas. Si un campo no tiene fuente, déjalo `PENDING` o
  vacío y dilo en las notas. Cada afirmación sobre V5 cita su sección o decisión exacta (y, si puedes, la línea).
- Si Python hace falta: escribe el script con Write y ejecútalo con `python -I script.py`; nunca `python -c`, `python -` ni heredoc.

## Fuentes (léelas)

1. **El Freeze:** `W/docs/initiatives/I-64-proposal-v5.md` (commit `9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311`, blob
   `dc1924ff5a73a4e529fbc5f995621d29351a091b`). Comprueba que el archivo de W tiene ese blob (`git -C <W> rev-parse HEAD:docs/initiatives/I-64-proposal-v5.md`);
   si no, lee el blob exacto con `git cat-file -p` y dilo en las notas. Lee en especial: §1 objetivo, no-objetivos, D-01, D-08, D-09,
   D-15, D-16, D-20, el plan de fases/gates (§11 o donde esté: F1..F4, F5-A, F5-B, F6, F7, Smoke-PERF, READY) y las reglas de cambio
   (enmiendas, autoridad Owner/Architect/Coordinator, qué es material, qué es OWNER-RESERVED).
2. **Enmiendas previas:** `W/docs/initiatives/I-64-A-1.md` … `I-64-A-13.md` (formato de cabecera y de «Lo que A-n no hace»; A-13 es
   el modelo más reciente). A-14 no modifica ninguna.
3. **Reglas del proyecto:** `W/docs/WORKFLOW.md` (ciclo de vida, Freeze, enmiendas, autoridad, integración) y `W/AGENTS.md` si cita
   algo pertinente.
4. **Estado de I-64:** el archivo de estado de I-64 en W (búscalo: `current_phase: F4B`), `W/docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`
   (F4B-E01: cómo se insertó F4B entre F4 y F5-A por disposición del Owner/Coordinator; D-PB-01 = «Editar todo» MODAL) y
   `W/docs/initiatives/I-64-f4b-push-back-completo-package.md` (solo lo necesario).
5. **La replaneación aprobada:** `S/replaneacion-i64.html` (§1-§7; la tabla de §3 es lo que el Owner aprobó; §4 son las fases).
6. **Decisiones del Owner, literales:** `S/owner-a14-decisions.md`.
7. **Prototipo aprobado (solo para describir la ventana):** `S/../proto-rackcad/project/Main.dc.html` y `canvas.json` (HTML con datos
   de ejemplo; no es especificación de código).

## Qué escribir

`S/I-64-A-14-draft-v1.md`, en español (la cabecera en bloque `text`, como A-13). Contenido mínimo:

1. **Cabecera:** Amendment A-14; Initiative I-64; Freeze (commit/path/blob); qué secciones/decisiones de V5 enmienda; relación con
   A-1..A-13 (no modifica ninguna) y con las disposiciones de F4B (D-PB-01 pasa a ser paso intermedio; F4B NO se reabre ni cambia);
   Authority (la que exijan V5/WORKFLOW para cada cambio, sin inventar); Owner (decisiones del 2026-11-23, archivo literal; la firma
   del texto final sigue pendiente); Architect y Coordinator (PENDING); Status: PROPOSED / NOT ACTIVE.
2. **Motivo:** la visión del Owner (todo RackCad sin bloquear AutoCAD, ventana tipo navegador). Hecho clave a verificar y citar: V5
   ya decía que los editores serían editables en el panel (D-15) y F4B D-PB-01 abrió «Editar todo» como ventana modal.
3. **Decisiones del Owner** (resumen con referencia al archivo literal; el supuesto de RACKPANEL marcado como supuesto).
4. **Cambios, uno por uno:** para cada punto de la tabla §3 de la replaneación: texto vigente de V5 (cita exacta o paráfrasis fiel
   con su referencia) → texto nuevo → autoridad que lo decide → riesgo que V5 quería evitar y cómo lo trata A-14. Obligatorios:
   - §1 objetivo;
   - D-08 (pestañas → columna lateral con buscador, filtro por sistema, «solo con cambios», «seguir la selección de AutoCAD»,
     un rack abierto en el centro, borradores vivos al cambiar);
   - D-09 y D-20 (sin lote → «Actualizar todo»: todo o nada, un solo Deshacer, dice cuál falló y por qué; conflicto con el dibujo:
     volver a leer / conservar mi borrador). Lee POR QUÉ V5 lo excluyó (residual de ID3) y trata ese riesgo de frente;
   - no-objetivo «crear racks desde el panel» → «Nuevo» (intermedio: ventana clásica del sistema);
   - D-16 y no-objetivo de comandos → secciones (Variables, BOM y lista de racks, Biblioteca de diseños, Configuración); RACKCAD abre
     la ventana; la retirada de comandos es una fase propia con lista decidida por el Owner antes; RACKPROYECTAR y RACKEDITAR se quedan;
   - D-15 + D-PB-01 (realineación: el editor vive en el área central; el modal queda como intermedio);
   - no-objetivo «no persistir la posición de la paleta» → la ventana recuerda tamaño y posición;
   - D-01 (PaletteSet y obligación de foco) sigue igual: dilo.
5. **Plan de fases nuevo** (de §4 de la replaneación): 0 cierre de F4B tal cual · 1 ventana (menú RACKCAD en el panel, sin bloquear) ·
   2 racks del dibujo (ex F6, lista lateral 70+) · 3 Push Back en la ventana (sustituye «Editar todo» modal; reutiliza estado y
   escritura de F4B) · 4 Actualizar todo · 5 secciones · D línea de diseño en paralelo (prototipos aprobados por el Owner; orden
   Sel/Din/Cant, luego Cab/Cama, luego PB) · 6 Sel/Din/Cant (ex F5-A) · 7 Cab/Cama (ex F5-B) · 3b Push Back rediseñado · 8 Nuevo ·
   9 retiro de comandos · F7 / Smoke-PERF / READY. Para cada fase: objetivo, dependencias, qué gate de V5 sustituye o conserva,
   y que cada una necesita su propio paquete con revisión independiente y orden del Owner antes de emitirse (como F4B).
   Tabla de correspondencia gates V5 → fases A-14.
6. **Riesgos** (los de §5 de la replaneación, más los que encuentres).
7. **Lo que A-14 no hace:** no reabre F1..F4B; no cambia el contrato de PB-T5 ni el Smoke-PB en curso; no retira comandos; no
   autoriza emitir ninguna fase; no toca `src/` ni `tests/`; no cambia la semántica de escritura ya cerrada (PREPARE/MUTATE/POST).
8. **Camino de aprobación** y **preguntas pendientes** (lista de retiro antes de la fase 9; RACKPANEL; lo que encuentres).

Además, `S/a14-drafting-notes.md`: fuentes leídas con su blob/SHA, cada `PENDING` y por qué, supuestos, contradicciones encontradas
entre V5, F4B y la replaneación, y cómo se invocó al Architect en las enmiendas anteriores de I-64 (celda, transporte, dónde quedan sus
revisiones) si lo encuentras en W.

## Entrega

Tu respuesta final: rutas de los dos archivos, número de líneas, la lista de `PENDING`, las contradicciones y cualquier punto que
creas que debe volver al Owner. Breve.
