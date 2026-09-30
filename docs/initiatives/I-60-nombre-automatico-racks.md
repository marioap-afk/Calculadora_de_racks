# I-60 — Nombre lógico automático de los racks nuevos

Status: Freeze `30c7ba0b`; RED `51711c84`; GREEN `f5104016`; Candidato declarado (decisiones §4); OWNER VALIDATION REQUIRED; NOT INTEGRATED

Workflow: V2 (unidad nueva posterior a `WORKFLOW_V2_EFFECTIVE_SHA`; base con el SHA efectivo; sin pausa: T4)

Initiative: I-60. Unit: `I-60`. Archetype: EXTENSION de producto (sin Foundation, sin esquema).

Branch: `feature/nombre-automatico-racks` (worktree `~/.codex/worktrees/feature-nombre-automatico-racks`)

Decisiones: [`docs/automation/decisions/I-60.md`](../automation/decisions/I-60.md). Freeze: [`docs/initiatives/I-60-freeze.md`](I-60-freeze.md).

## Proposito (autorizacion del Owner)

Todo rack **lógico nuevo** creado por RackCad recibe un nombre lógico automático, **no vacío y editable**, en `RackEmbedDocument.Name`
(no en el nombre de la definición de bloque, no en el BaseName de AUTH-11, no en el RackId). Política inicial: «Selectivo N», «Dinámico N»,
«Push Back N», «Cantilever N», «Cabecera N»; la cama de rodamiento con la terminología que el producto ya usa (se caracteriza antes de congelar).
Siguiente `N` = el mayor `N` de los nombres del dibujo que siguen exactamente el patrón de su familia + 1 (no se rellenan huecos; sin distinguir
mayúsculas; los nombres personalizados no consumen número salvo que sigan exactamente el patrón; secuencias independientes por familia). Un rack
lógico nuevo recibe UN nombre y todas sus vistas iniciales lo comparten. Los racks heredados sin nombre siguen siendo válidos, se muestran
«(sin nombre)» y **no** se renombran automáticamente (ni al abrir, `RACKLISTA`, `RACKBOMTOTAL`, `RACKEDITAR`, `RACKPROYECTAR`, guardar, insertar hermana
o proyectar), salvo que el usuario edite su nombre. `RACKDUPLICAR` se caracteriza y, si la ambigüedad persiste, queda fuera de alcance y sin cambios.

## Coordinacion

- **I-55** (`feature/creacion-de-vistas`) espera su Owner Validation: esta unidad **no se integra** mientras tanto (avanzaria `main` y dejaria obsoleto su
  Candidato). Las rutas ID17 (primera vista libre) e ID18 de I-55 no existen en `main`; se reconcilian cuando I-55 se integre.
- **AUTH-15 / I-52-AUTH15-C1** no se tocan: el `Name` en blanco sigue siendo valido.
- Sin cambios de Foundation ni de esquema.

## Fuera de alcance

Migracion masiva; renombrar racks heredados; cambiar la politica de nombres de bloque (AUTH-11); `RACKDUPLICAR` (autorizacion del Owner: sin cambios
mientras la ambiguedad persista) y `RACKLAYOUT` independiente (duplicacion en lote bajo el mismo contrato de reestampado, por analogia); C16-06 y cualquier
parte de I-55.
