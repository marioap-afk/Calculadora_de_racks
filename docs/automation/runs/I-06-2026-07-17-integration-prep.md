# I-06 — preparación final de integración

Fecha: 2026-07-17, America/Mexico_City

Rama: `docs/reestructura`

Worktree: `rackcad-docs-reestructura`

Pull Request registrado: #1, no consultado ni modificado

Claim-Id: `6cdf1e76-7943-4bfb-b1fa-b4c8bb88a050`

## Base y rebase

- `origin/main`: `8e52828c5470af7f09b49a0b0cddce2a03ea3bbe`.
- Punta anterior de la rama: `273af5ca9c75efea4728146924c5aa05dc44e24e`.
- Divergencia observada antes de editar: cero commits detrás y doce delante de `origin/main`.
- `origin/main` seguía siendo ancestro de la rama y no había commits nuevos que incorporar.
- No se ejecutó rebase ni se reescribió el historial; tampoco se usó force ni
  `--force-with-lease`.

## Intento de integración descartado

El intento local de merge `878e6c6...` fue descartado sin publicarse. Se detuvo porque el HANDOFF
preparado por `39cd54189457e8737f08cf95dbf948bc2e564dd3` todavía describía I-06 como no integrada,
indicaba integrar I-06 como siguiente acción y presentaba I-04 como baseline actual; ese contenido
habría quedado obsoleto inmediatamente después del merge.

La corrección se realiza en `docs/reestructura` antes de repetir la integración. El descarte no
perdió código ni alteró `origin/main`, que continúa en
`8e52828c5470af7f09b49a0b0cddce2a03ea3bbe`. Debe repetirse CI de la rama sobre el nuevo SHA antes
de crear otro merge.

## Validación local

AutoCAD 2025 estaba cerrado. Las validaciones se ejecutaron con el SDK .NET 8.0.423 instalado por
usuario y el Plugin se compiló contra `C:\Program Files\Autodesk\AutoCAD 2025`.

- `git diff origin/main --check`: limpio.
- Suite `RackCad.Tests` Debug: verde, sin fallos ni pruebas omitidas; el conteo canónico permanece
  en `docs/HANDOFF.md`.
- Build `RackCad.UI` Debug: correcto, 0 errores y 0 advertencias.
- Build `RackCad.Plugin` Debug: correcto, 0 errores y dos advertencias `MSB3277` conocidas por las
  familias `Microsoft.VisualBasic` y `System.Drawing`.
- Enlaces Markdown: 52 documentos y 123 destinos locales revisados; cero destinos rotos.
- Context Packs: nueve manifests, nueve IDs únicos, con documentos, globs, gates y exclusiones
  válidos.
- Cambios prohibidos: cero bajo `assets/`, `deploy/`, `tests/`, solución o proyectos.
- AutoCAD no se ejecutó porque I-06 no cambia comportamiento.
- CI de la rama fue confirmado verde por el dueño para
  `39cd54189457e8737f08cf95dbf948bc2e564dd3`; el nuevo commit administrativo debe volver a pasar CI
  antes del merge.

## Diff completo bajo `src/`

```diff
diff --git a/src/RackCad.UI/RackCommandReference.cs b/src/RackCad.UI/RackCommandReference.cs
@@
-    /// docs/despliegue.md). When a command or its alias changes (see RackFrameCommands.Aliases.cs), update this list.
+    /// docs/guias/despliegue.md). When a command or its alias changes (see RackFrameCommands.Aliases.cs), update this list.
```

Es únicamente la corrección autorizada de una ruta dentro de un comentario XML; no cambia código
ejecutable ni comportamiento.

## Documentos finales preparados

- `docs/ROADMAP.md` lleva la marca de I-06 que se hará efectiva con el merge manual y mantiene I-07
  bloqueada hasta ese momento.
- `docs/HANDOFF.md` registra el cierre, la automatización pausada y la siguiente acción posterior al
  merge.
- `docs/automation/state/I-06.yml` queda en `completed`, sin gate y con verificación de CI de `main`
  y limpieza como siguiente acción post-merge.
- `docs/initiatives/I-06-reestructura-context-packs.md` registra completas las fases 1 a 6, la
  validación del dueño aceptada y el merge pendiente.
- Esta evidencia conserva la base, las validaciones y las restricciones de la sesión.

## Validación del dueño y detención

La validación del dueño está completada y la integración manual está autorizada. Esa autorización
resuelve `owner-validation`, pero no sustituye la repetición de CI para la nueva punta antes del
merge.

No se hizo merge, no se modificó `main`, no se eliminó la rama ni el worktree y no se consultó o
modificó el Pull Request. La automatización sigue pausada y el desarrollo posterior continúa
manualmente bajo WORKFLOW.

`last_evidence_commit` puede continuar en el commit técnico de fase 6
`679aa50d62cfe455530b4ee36fe4a75e89648989`: los commits administrativos posteriores no pueden
autorreferenciar su propio SHA.

Antes de repetir el merge, el dueño debe verificar en GitHub Actions que CI esté verde para el nuevo
SHA final de `docs/reestructura` publicado por el commit que contiene esta corrección.
