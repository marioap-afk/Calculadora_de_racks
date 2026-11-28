# P-05: el entorno antes de la corrida (bloques 2, 3 y 4 de `smoke-run-v2.md`)

- **Hora:** 2026-11-23 12:43 UTC, antes de abrir AutoCAD.
- **De dónde sale:** es lo registrado entonces en `run-log.md` (fila P-05). Las salidas de los bloques no se guardaron aparte.

## Lo registrado

- **Procesos:** ningún `acad` ni `accoreconsole` abierto.
- **Variables:** ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni la consola.
- **Bundles:** ningún bundle de RackCad.
- **Perfil de AutoCAD:**
  - 0 entradas de RackCad en `Applications`, `Loaded` y `AssemblyMap`.
  - `Dialogs\RackCad.*` contadas: 20.
  - `PICKFIRST` no está en el registro. Se lee en AutoCAD con `(getvar "PICKFIRST")` antes de los pasos con selección por expresión, sin cambiarla.
- **Datos del usuario:**
  - La carpeta de diseños existe.
  - La disposición `RackFrameConfigurator.layout` es `7AAB8FF7A3E52CE52399DE3B2C8A8AFEEC0C01720D90DAD0CA49D108BFF82621`, escrita por última vez el 2026-11-19T21:41:29Z.
  - `settings.json` es `A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873`.
  - La carpeta de registros de RackCad existe y no lista ningún `rackcad-*.log`. El valor de la carpeta que se usó no quedó anotado; ver `environment-post.md`.

La comparación con el final de la corrida está en `environment-post.md` (SPB-52).
