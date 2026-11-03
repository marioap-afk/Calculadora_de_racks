# I-65 F4-T6-COMPARATOR-HARDENING — disposiciones del Coordinator para la verificacion (2026-10-13)

Redactadas por el Coordinator (AUTOMATION_PLAN 16.6) ANTES de la verificacion, con los hechos de la entrega del Worker
`R20261013T214908Z-260e` (RedSha `d655071b`, CurrentSha `6422e5b0`). No amplian el contrato; fijan como se leen dos puntos.

## D1. Ligadura de A65-F4-03 y nudos de los miembros

El invariante A65-F4-03 (ligadura) pide que la lectura del volcado del generado coincida con los `values` de la lectura de vuelta
«mismos miembros con sus nudos, seccion y material». La funcion `read_back` de `RamBuildScriptTemplate.cs` (fuera del alcance de
F4-T6) registra en `values.members` id, ramId, seccion, material, giro, punto cardinal, offsets, factores, comportamiento axial,
resortes, liberaciones y descripcion, pero **no** los nudos I y J. Disposicion del Coordinator: la ligadura compara los campos que la
lectura de vuelta registra; para los miembros, ids, seccion, material y recuentos (y los nudos se ligan por sus ids y coordenadas).
La conectividad del generado se sigue evaluando contra el manual en la fila de conectividad. Ligar los extremos exigiria cambiar la
plantilla de construccion (fuera de esta tarea): es una **limitacion registrada** para la revision del Architect, no un defecto de la
entrega. Evalua Tests con esta lectura.

## D2. Controles operativos del Owner (invariante de rutas absolutas) y DEV-I65-F4-19

El ultimo invariante del contrato es un control de proceso (decisiones §41, CD-I65-F4-32), no una propiedad del codigo: no lo observa
ninguna prueba y lo evalua la sesion con K1-K5 (ver Notes de `relay-record-work.json`). Hechos: la primera orden del Worker fue `pwd`
y la entrega registra pwd, Get-Location y [Environment]::CurrentDirectory; K1/K2 sin cambios en el checkout principal ni en los demas
worktrees; K5 sin interpretes, RAM, AutoCAD, lecturas o escrituras fuera del worktree ni E/S .NET relativa. El Worker declaro que 30
llamadas de Bash hicieron `cd` a una ruta ABSOLUTA del worktree y usaron nombres relativos en la MISMA llamada. Disposicion del
Coordinator: **DEV-I65-F4-19**, desviacion de procedimiento sin efectos (todo resolvio dentro del worktree); C-12 no se activa (no hubo
cambio externo y el directorio se informo). No la conviertas en fallo de Tests ni en parada; citala en Findings.

## D3. Otros puntos declarados por el Worker (admisibles, citalos si procede)

- `values` de una lectura de vuelta con estado de fallo solo se exige objeto: esas lecturas se rechazan antes de cualquier ligadura.
- `--combinations-decision` tambien se acepta sin las opciones de combinaciones no leidas (el texto se registra); el contrato exige la
  decision con esas opciones y no prohibe lo demas.
- `StructuralReadbackLoadCase.WithoutLoadsAndCategory` (anadido en StructuralReadback.cs, solo anadidos) y tres ayudantes de
  `RamDumpReader` pasados a `internal` (archivo del alcance).
- `S.Approved()` sin argumentos devuelve valores coherentes con `D.Valid()` (edicion nombrada de Support, porque una prueba existente
  de RowTests lo llama y no podia editarse).
