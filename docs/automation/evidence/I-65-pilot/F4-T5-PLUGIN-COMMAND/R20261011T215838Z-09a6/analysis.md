# I-65 F4-T5-PLUGIN-COMMAND — analysis.md: STOP del Worker y escritura en el checkout principal

Redactado por el Coordinator según AUTOMATION_PLAN §16.11.

## Hechos (comprobados mecánicamente)

- Delegación `R20261011T215537Z-f944` (Attempt 3, AttemptsRemaining 0), Worker `R20261011T215838Z-09a6` (`claude-sonnet-5-5`/`high`). Entrega
  válida: `BLOCKED/STOP`, C-08 y C-05; ningún commit ni push; `CurrentSha` = `BaseSha` = `283ed613`.
- **C-08:** `python3 --version` con la salida descartada (llamada 52 de 55, 22:08:56Z). Quinta invocación de un intérprete por un Worker en la
  unidad (CD-I65-F3-12, DEV-I65-F4-01, DEV-I65-F4-05, DEV-I65-F4-06 y esta), con C-08 ampliada y la prohibición expresa en el prompt.
- **C-05:** la herramienta de escritura creó `D:\Documentos\Codex\Calculadora de racks\placeholder.txt` (checkout principal, zona No-touch) a las
  22:05:31Z; el Worker lo borró a las 22:05:34Z y la sesión comprobó que no existe.
- Worktree sucio a la entrada (borradores sin commit): preservados en `preserved/` del directorio del trabajo; la sesión devolvió el worktree a
  `HEAD` limpio.
- **Hallazgo nuevo (intento 2 de F4-T4):** el `RackCad.sln` del checkout principal está modificado sin commit desde 20:32:56Z (entrada del
  proyecto `RackCad.StructuralAnalysis.RamTool` con el GUID `5C7B2D1E-…` de F4-T4 y BOM eliminado). Lo escribió el Worker de
  `R20261011T202738Z-6356` en su llamada 25: `Set-Location` al worktree y después `[IO.File]::ReadAllText/WriteAllText("RackCad.sln")`;
  .NET resuelve la ruta relativa contra el directorio del proceso, que es el checkout principal. El Worker no lo declaró; la verificación de
  F4-T4 no lo vio porque los hechos del relevo solo miran el worktree. Es una **violación con consecuencias reales**: el checkout de trabajo del
  Owner tiene un cambio no suyo que, si se confirmara en `main`, referenciaría un proyecto inexistente. Diff preservado
  (`preserved/main-checkout-RackCad.sln.diff`).

## Clasificación

- C-08 de T5: infracción de procedimiento sin efectos observados (antes de cualquier commit; nada se escribió con ella).
- C-05 de T5: violación de alcance con efecto transitorio (archivo creado y borrado en 3 s; sin residuo comprobado).
- Escritura de F4-T4 en el checkout principal: violación con consecuencias reales, todavía presente.

## Situación del protocolo

- `attempts` = 3 = `max_attempts`. Una nueva delegación de T5 sería una corrección (`attempts` 4): **S-11**. Volver a ejecutar el Worker con la
  misma delegación no está previsto por §16.8 para un STOP; la decisión es del Owner, igual que en F4-T3.
- Presupuesto de Codex: 28 de 30 usadas; T5 necesita al menos verificación y tres controles (4) más lo que requiera la vía que se elija.

## Decisión del Coordinator (CD-I65-F4-22)

STOP al Owner. No se relanza nada. No se restaura el checkout principal sin su decisión (el diff está preservado). La propuesta para I-62 se
amplía con las escrituras fuera del worktree.
