# ADR-0002: Secuencia de integración de la rama del dinámico modular

- **Estado:** propuesto
- **Fecha:** 2026-07-16
- **Decisores:** dueño del repo (pendiente); redactado por Claude
- **Iniciativa relacionada:** `feature/dinamico-modular` (si se acepta la opción A)

## Contexto

`codex/dinamico-modular` contiene la reescritura del sistema dinámico: +12,621 líneas en 85
archivos (3 commits), bifurcada de `cd20200` — es decir, SIN los 6 arreglos verificados de
`eaede44`, que tocan justamente la persistencia que el dinámico usa (`RackFrameProjectDocument`
perdía 4 campos; `RackFrameProjectStore` sin SchemaGuard). La rama además modifica archivos
compartidos que el trunk evolucionó (`SystemBomBuilder`, stores, `seguridad.csv`/`secciones.csv`/
`blocks.csv`) e infla `RackDynamicSystemWindow.xaml.cs` de 1,332 a ~3,318 líneas con los mismos
patrones (code-behind, tubería clonada) que el roadmap quiere erradicar.

Todo refactor estructural (registro de sistemas, editor shell, persistencia uniforme) toca esos
mismos archivos: arrancarlos ANTES de resolver esta rama garantiza un conflicto de 12k líneas
después. Es la decisión de secuencia que bloquea la Fase 2 del ROADMAP.

## Decisión (propuesta)

**Paso 0 — obtener la evidencia que decide (media sesión, ANTES de elegir):** hoy NO está
registrado en ningún documento si `codex/dinamico-modular` fue validada alguna vez en AutoCAD
(HANDOFF §9 solo valida el trunk). Compilar la rama en su worktree, cargar con NETLOAD el DLL de ESE
worktree, recorrer el checklist funcional del dinámico (editor, vistas, BOM, round-trip con
RACKEDITAR) y anexar el resultado aquí. Esa evidencia — no una corazonada — es la que separa A de B.

**Opción A (recomendada si el Paso 0 muestra la funcionalidad validada o cerca de estarlo):**
integrar primero. Push del trunk → rebase de la rama sobre su punta (renombrándola
`feature/dinamico-modular`, ADR-0001) → resolver conflictos con la suite como red → re-validar en
AutoCAD → merge → borrar rama y worktree. Los refactors de Fase 2 parten del árbol resultante y el
editor dinámico nuevo se convierte en candidato principal de la migración al editor shell (Fase 5).

**Opción B:** descartarla formalmente. Tag `archive/dinamico-modular`, borrar rama y worktree, y
re-implementar el dinámico modular DESPUÉS del registro de sistemas y el editor shell, usando la
rama archivada como referencia de requisitos. Elegible si la validación funcional de la rama está
lejos o el rebase resulta más caro que re-implementar.

Mientras no se decida: **congelado todo trabajo nuevo sobre el subsistema dinámico** (la "siguiente
tarea" de HANDOFF §11 empieza por esta decisión).

## Alternativas consideradas

- **Integrar "cuando toque", sin secuencia explícita** — descartada: cada semana de espera agranda
  el conflicto; es exactamente el patrón que produjo la rama zombie y la huérfana.
- **Refactorizar primero y rebasar la rama encima** — descartada: obliga a Codex a rebasar 12.6k
  líneas sobre una base reorganizada; el costo del rebase se multiplica en vez de reducirse.

## Consecuencias

- A: el trunk absorbe pronto una pieza grande ya escrita; el costo es una sesión de
  integración/validación intensa ahora, con los 554+ tests y la validación en AutoCAD como red.
- B: se pierde trabajo hecho (mitigado por el tag de archivo), pero el dinámico renace directamente
  sobre la arquitectura objetivo, sin arrastrar otra ventana de 3,300 líneas a migrar en Fase 5.

## Referencias

Auditoría 2026-07 §5 y hallazgo G4; crítico de completitud (hallazgo "rama subestimada");
HANDOFF §11.1; WORKFLOW.md §8.
