# I-65 F4-T4-COMPARATOR-AND-TOOL — disposiciones del Coordinator previas a la verificación (intento 3, WorkRunId R20261011T210642Z-ca39)

Texto literal entregado al Controller como entrada de la verificación. Ya registradas en `docs/automation/decisions/I-65.md` §33 y §34
(commits `0b8e3b74` y `c8f3aa88`), salvo la última viñeta, que se registrará con la custodia después de la verificación.

- **CD-I65-F4-18 (corrección por RED desactivado).** El intento 2 (`R20261011T202738Z-6356`, RED `b51adf28`, GREEN `0e9bbc29`) se detuvo
  por dos aserciones defectuosas de su propio RED y dos advertencias en archivos del RED, sin verificación (DEV-I65-F4-09). La cadena no
  acreditó ningún RED (`ChainRedSha` null), así que esta corrección **exige RED** y lo da con la «corrección desactivada» de AUTOMATION_PLAN
  §16.8: el commit RED `c4660703` lleva las pruebas de la cadena corregidas y la implementación devuelta al esqueleto de `b51adf28`; el
  GREEN `7efacd78` restaura la implementación de `0e9bbc29` sin tocar `tests/`. Por eso `git diff --name-only BaseSha..CurrentSha` (con
  `BaseSha` = `c8f3aa88`, que ya contenía la implementación de `0e9bbc29`) son solo los dos archivos de prueba corregidos, y el diff
  `RedSha..CurrentSha` restaura `src/` y `tools/` sin tocar `RT` ni `ChainRedFiles`. `RT` = `git diff --name-only <ChainBaseSha> <RedSha> -- tests/`
  con `ChainBaseSha` = `34831380`.
- **CD-I65-F4-19 y DEV-I65-F4-11.** La primera planificación de esta corrección (`R20261011T205717Z-064f`) se rechazó por A5 (P-03) y se
  reemitió. La delegación aceptada (`R20261011T210328Z-2ee6`) pasó A1-A8; su `StopConditions` tiene los mismos ids y orden que el contrato,
  pero el texto de C-07 omite «o al reves» (A5 compara por id). El prompt del Worker le dio el texto completo de C-07 del contrato. Evalúa
  la comprobación `Contract` contra el contrato y declara esa diferencia en la Evidence.
- `CorrectionOf.AnalysisSha256` debe coincidir con el SHA-256 de
  `docs/automation/evidence/I-65-pilot/F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/analysis.md` (versionado en `0b8e3b74`).
- Esta es la **última** corrección de la unidad (`attempts` = 3 = `max_attempts`): evalúa las 14 comprobaciones sobre la evidencia, sin
  ninguna consideración por esa circunstancia.
