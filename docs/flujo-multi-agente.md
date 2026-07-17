# Flujo Git y trabajo multi-agente (Claude + Codex + humanos)

> Creado: 2026-07-16 (auditoría de arquitectura). Define el flujo OBJETIVO de ramas, worktrees e
> integración para que varios agentes IA y el humano trabajen en paralelo sin pisarse.
> La migración inicial (sección 7) requiere aprobación del dueño del repo; el resto aplica desde ya.

## 1. Ramas

| Rama | Rol | Reglas |
|---|---|---|
| `main` | **Trunk único de integración** (tras la migración de la sección 7) | Solo recibe trabajo validado (suite verde + verificación en AutoCAD cuando aplique). Nunca se trabaja directo en ella. |
| `claude/<tema>` | Rama de trabajo de una sesión de Claude | Vida corta (días, no semanas). Nace de la punta ACTUALIZADA del trunk. |
| `codex/<tema>` | Rama de trabajo de una sesión de Codex | Ídem. |
| `wip/<tema>` | Respaldo remoto de trabajo a medias | Push libre; "publicado" ≠ "integrado". Sirve para que el remoto siempre respalde todo. |
| `release/vX.Y.Z` | Corte estable etiquetado (futuro) | Solo cuando haya releases a terceros. `release/*` deja de usarse como trunk. |

Estado transitorio actual: el trunk *de facto* es `release/claude-review` (main está abandonada en el
commit inicial). Hasta ejecutar la migración, toda regla que aquí dice "trunk" se aplica a
`release/claude-review`.

## 2. Ciclo de vida de una rama de agente

1. **Nacer actualizado**: `git fetch` y bifurcar de la punta del trunk. Nunca de una rama vieja ni
   de otra rama de agente.
2. **Declarar el área**: al abrir la sesión, anotar en la tarea/prompt (y al cerrar, en HANDOFF §8)
   QUÉ módulo se toca (p. ej. "sistema dinámico", "seguridad del selectivo", "docs"). Dos sesiones
   paralelas no deben tocar el mismo módulo caliente (sección 4).
3. **Rebase si el trunk avanzó**: antes de continuar una sesión de un día para otro, `git fetch` +
   rebase sobre el trunk. Una rama que se queda atrás acumula conflictos semánticos (ya ocurrió:
   `codex/dinamico-modular` bifurcó justo antes de fixes de persistencia que afectan su área).
4. **Integrar**: la integración al trunk la decide el humano (o una sesión dedicada de integración),
   después de: suite completa verde + build Debug de UI y Plugin + verificación manual en AutoCAD
   para cambios de dibujo (política de AGENTS.md).
5. **Limpiar en el mismo acto**: al integrar, borrar la rama local, la remota y el worktree
   (`git worktree remove` + `git branch -D` + `git push origin --delete`). Las ramas zombie con
   worktree montado invitan a retomar trabajo sobre una base muerta.

## 3. Worktrees

- Un worktree por sesión/tarea, nunca dos sesiones sobre el mismo worktree.
- El worktree principal (`D:\Documentos\Codex\Calculadora de racks`) es del humano: los agentes no
  dejan ahí cambios sin commitear. Cambios de catálogo hechos "en vivo" en el worktree principal se
  commitean (o descartan) el mismo día — un CSV sin commitear es invisible para todos los demás
  worktrees y hace divergir dibujos y tests entre agentes.
- Los worktrees de agentes viven fuera del árbol versionado (`.claude/worktrees/` está excluido
  localmente; los de Codex en `%USERPROFILE%\.codex\worktrees`).

## 4. Archivos calientes (alto riesgo de conflicto)

Estos archivos concentran los merges conflictivos. Regla: **un solo agente a la vez** por archivo
caliente; si tu tarea los toca, decláralo.

| Archivo | Por qué es caliente |
|---|---|
| `docs/HANDOFF.md` | Único documento de estado compartido; se actualiza SOLO al cerrar sesión y de preferencia en la sesión de integración, no en paralelo |
| `assets/catalogs/*.csv` | Datos compartidos por todos los sistemas y 20+ archivos de tests. Convención: **append-only** (agregar filas al final); nunca reordenar ni re-guardar con otro encoding |
| `src/RackCad.UI/RackSelectiveWindow.xaml.cs` (~2,460 líneas) | Editor más grande; cualquier feature del selectivo pasa por aquí |
| `src/RackCad.Plugin/RackFrameCommands.cs` + partials | Una sola clase en 12 archivos con helpers estáticos cruzados |
| `src/RackCad.UI/RackFrameConfiguratorViewModel.cs` (~2,550 líneas) | God-ViewModel del configurador |
| `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `SelectiveSafetySelection.DeepCopy` + DTO: cada familia nueva lo toca |

(El roadmap de la auditoría reduce esta lista: registro de sistemas, editor shell y división de
comandos existen precisamente para que los agentes dejen de chocar en los mismos archivos.)

## 5. Commits, procedencia y push

- Mensajes: convención existente de AGENTS.md (`Area: qué cambió`, cuerpo con el porqué, español).
- **Procedencia de agente**: TODO commit generado por una IA lleva trailer identificándola. Claude ya
  lo hace (`Co-Authored-By: Claude ...`); Codex debe añadir el suyo (`Co-Authored-By: Codex <noreply@openai.com>`
  o un trailer `Agent: codex`). Sin esto es imposible auditar qué herramienta produjo qué código.
- Push: los commits locales son libres; el push del trunk sigue la política de AGENTS.md (tras
  confirmación del usuario). Para no dejar trabajo sin respaldo remoto, cualquier rama puede empujarse
  como `wip/*` sin que eso signifique integrada.
- No copiar conteos de tests ni hashes de commit en documentos que no sean `docs/HANDOFF.md` §12
  (los números copiados divergen; la auditoría encontró 4 conteos distintos conviviendo).

## 6. Integración, CI y releases

- **CI** (`.github/workflows/ci.yml`): en cada push corre la suite (ubuntu) y el build de la UI
  (windows). El Plugin queda fuera (necesita las DLL de AutoCAD). El CI es el árbitro neutral entre
  agentes: si el trunk está rojo, nadie integra encima.
- Un cambio de API en `RackCad.Application` obliga a compilar UI y Plugin localmente antes de dar por
  terminado (la suite verde NO garantiza que compilen; están fuera del grafo de tests).
- **Releases** (cuando se comparta el bundle a terceros): etiquetar `vX.Y.Z` en el commit del corte,
  subir `AppVersion` de `PackageContents.xml` en ese mismo commit, y anotar en HANDOFF qué se entregó
  a quién. Sin tag no hay forma de mapear un bundle instalado a un commit al diagnosticar un bug.

## 7. Migración inicial (una sola vez — requiere aprobación del dueño)

Checklist para pasar del estado actual al flujo objetivo:

1. En el worktree principal: commitear o descartar los 6 CSVs de catálogo modificados sin commitear.
2. Push de `release/claude-review` (el remoto está 2 commits atrás; los fixes verificados de
   `eaede44` solo existen en el disco local).
3. Decidir la rama `codex/dinamico-modular` (+12,600 líneas): rebase sobre el trunk e integrarla, o
   descartarla formalmente. Es LA decisión de secuencia previa a cualquier refactor (ver auditoría §5).
4. Fast-forward de `main` al trunk (`git checkout main && git merge --ff-only release/claude-review
   && git push`), confirmar `main` como default branch en GitHub y proteger la rama con el check de CI.
5. Borrar ramas integradas/muertas y sus worktrees: `codex/seguridad-variantes-topes-botas` (zombie:
   su contenido ya vive en el trunk con otros hashes) y decidir `codex/app-tooling-catalogs-logging`
   (huérfana desde el commit inicial: rescatar por cherry-pick o archivar con tag y borrar).
6. A partir de aquí: `release/claude-review` se retira y `main` es el trunk.

## 8. Precedencia de documentos

Cuando dos documentos se contradigan: `AGENTS.md` > `docs/HANDOFF.md` > `README.md` >
`docs/0N-*.md` > documentos históricos. Y sobre todos ellos: el estado real del repo
(`git log`, `dotnet test`).
