# I-66 — PlacedClone copia los campos de texto y de cota

Status: G0 PASS; Discovery R2 ([I-66-discovery.md](I-66-discovery.md)); Freeze R2 en re-revisión; NOT INTEGRATED

Workflow: V2 (unidad nueva; la base `bb0d5522` contiene el merge normativo de V2 y la pausa de activación terminó: `integration/I-56`)

Initiative: I-66. Unit: `I-66`. Arquetipo propuesto: EXTENSION (sin disparadores M-01..M-08 conocidos; lo decide el Coordinator tras el Discovery).

Branch: `fix/placedclone-copia-anotaciones` (worktree `~/.codex/worktrees/fix-placedclone-copia-anotaciones`)

Decisiones: [`docs/automation/decisions/I-66.md`](../automation/decisions/I-66.md).

Consumes: ninguna fundación (el contrato de `Flatten` lo gobierna ADR-0011, aceptado). Extends: ninguna. Introduces: ninguna.

## Propósito

Corregir el hallazgo **I-50 H4** de [ideas-futuras](../ideas-futuras.md): `HeaderRunPlan.PlacedClone`
(`src/RackCad.Application/Drawing/HeaderRunPlan.cs`) copia rol, pieza, bloque, vista, rotación, espejos, puntos y parámetros dinámicos, pero no
`Text`, `TextHeight`, `DimensionOffset` ni `DimensionStyleName`. Hoy es inocuo, porque cotas y etiquetas quedan siempre sueltas y `Flatten` las
añade sin clonar; sería un defecto el día que una anotación entrara en un grupo ARRAY. La unidad hace que el clon conserve esos cuatro campos, con
una prueba Core RED→GREEN, sin AutoCAD.

## Piloto en sombra R1a de I-62

Es la unidad piloto de R1 «Principal portable» (decisiones de I-62 §105 y §106). Los registros en sombra viven en
`docs/automation/evidence/I-66-R1/` (preflight, binding y estado `state/v2`): solo añaden comprobaciones de STOP y **nunca autorizan nada**.
I-61 y WORKFLOW siguen siendo la única autoridad de esta unidad. Sin delegación Controller/Worker y sin Codex.

## Fuera de alcance

Meter anotaciones en grupos ARRAY; cambiar `HeaderInstanceGrouper`, el cuerpo de `Flatten` (solo cambia su clon por colocación), el dibujante del Plugin, la UI, el esquema o la persistencia; los
demás hallazgos de I-50; cualquier cambio en I-64 o I-65.

## Coordinación

Al reclamar, ninguna rama remota activa tenía commits sobre `HeaderRunPlan.cs` fuera de `main`. `docs/ROADMAP.md` es archivo caliente: I-65 añade
su fila en su propia rama.
