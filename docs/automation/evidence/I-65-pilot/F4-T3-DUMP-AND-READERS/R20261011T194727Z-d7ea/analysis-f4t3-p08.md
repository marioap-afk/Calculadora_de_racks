# I-65 F4-T3-DUMP-AND-READERS — analysis.md: P-08 en la verificación R20261011T194727Z-d7ea

Redactado por el Coordinator según AUTOMATION_PLAN §16.11 (P-08 es STOP; causa raíz y decisión del Coordinator).

## Hechos

- Verificación autorizada por el Owner («Owner Disposition: F4-T3 C-08 and Codex Budget»), invocación de Codex 16 de 30, `RunId`
  `R20261011T194727Z-d7ea`; cesión `2026-10-11T19:48:26Z`–`19:54:39Z`.
- A las `2026-10-11T19:48:34.767Z` (transcripción de la sesión principal), la sesión ejecutó en el worktree una orden de solo lectura:
  `grep` sobre `docs/automation/decisions/I-65.md` y `docs/automation/evidence/I-65-evidence.md`, y `ls docs/automation/evidence/`, para
  buscar el precedente DEV-I65-F3-02 mientras redactaba la propuesta para I-62. No escribió nada; el árbol siguió limpio y `HEAD` en
  `76db7642`. Las consultas posteriores de la sesión durante la cesión fueron al checkout principal (`D:\Documentos\Codex\Calculadora de racks`)
  y al scratchpad, no al worktree.
- AUTOMATION_PLAN §16.4 paso 2: durante la invocación «la sesión no lee, no escribe ni ejecuta nada sobre el worktree». Tabla de §16.11:
  P-08 «La sesión operó durante una cesión: la salida de esa invocación es inválida» → STOP.
- La salida del Controller (`EXECUTION_VERIFIED/NONE`, 14 en `pass`, coherente con README §8) es, por P-08, **inválida**: no se usa como
  verificación. Se custodia como invocación inválida.

## Causa raíz

Error de la sesión: consultó un precedente en el worktree en vez de en el checkout principal o en los objetos de Git, durante una cesión.
No fue una operación necesaria para el relevo.

## Decisión del Coordinator (CD-I65-F4-15)

- Se reejecuta la verificación con un `RunId` nuevo, las **mismas entradas** y el **mismo prompt** (solo cambian el `RunId` y la ruta de
  entrada); la salida inválida no se entrega como entrada. No hay cambio del trabajo: `attempts` sigue en 2.
- Invocación de Codex 17 de 30. Con ella, el plan restante (controles nc1-nc3 de T3; T4 y T5 con 5 cada una) llega exactamente a 30, sin
  reserva: cualquier invocación adicional exige pedir autorización al Owner antes de lanzarla (su disposición, punto 3).
- Medida: durante toda cesión la sesión solo trabaja en el scratchpad y en el checkout principal; ninguna orden con la ruta del worktree.
- Esto no es materia OWNER-RESERVED y cabe en el tope del Owner; se informará como desviación del protocolo en la entrega final.
