# ADR-0001: Ramas por iniciativa técnica, no por herramienta

- **Estado:** aceptado
- **Fecha:** 2026-07-16
- **Decisores:** dueño del repo (decisión directa); redactado por Claude
- **Iniciativa relacionada:** fase de planificación post-auditoría

## Contexto

Hasta ahora las ramas se nombraban por la IA que las creaba (`claude/*`, `codex/*`). La auditoría
2026-07 mostró que ese esquema no dice NADA útil: no comunica qué se está construyendo, dos
herramientas pueden trabajar la misma iniciativa en sesiones distintas, y el inventario de ramas no
funciona como registro del trabajo en curso. Además fomentó los incidentes observados (rama zombie,
rama huérfana, bifurcación desactualizada): la rama pertenecía "a la herramienta", no a un objetivo
con criterio de cierre.

## Decisión

Las ramas se nombran por la INICIATIVA técnica que contienen, con prefijo por tipo:
`architecture/` (estructural), `feature/` (funcionalidad), `refactor/` (preserva comportamiento),
`fix/` (bug), `docs/` (documentación), `experiment/` (spike descartable), `release/vX.Y.Z` (cortes)
y `hotfix/` (reservado post-release). Slug en kebab-case y español. **1 iniciativa = 1 rama = 1
worktree.** La procedencia de la herramienta se registra en los trailers de commit
(`Co-Authored-By`), no en el nombre de la rama. Detalle operativo: [WORKFLOW.md](../WORKFLOW.md) §1-2.

## Alternativas consideradas

- **Mantener `claude/*`/`codex/*`** — descartada: nombra al autor, no al trabajo; impide que el
  listado de ramas sea el registro de iniciativas en curso.
- **Prefijos por herramienta + sufijo de tema** (`codex/dinamico-modular`) — descartada: duplica
  información que ya dan los trailers y fragmenta una iniciativa entre herramientas.
- **Sin prefijos, solo slug** — descartada: el prefijo comunica el tipo de riesgo/revisión que la
  integración exige (un `refactor/` promete comportamiento idéntico; un `architecture/` pide más ojo).

## Consecuencias

- Positivas: `git branch -a` se vuelve el tablero de trabajo en curso; las iniciativas tienen dueño
  conceptual y criterio de cierre; cualquier IA puede continuar la rama de otra.
- Negativas / costos aceptados: las ramas existentes con nombre viejo se resuelven una a una en la
  migración (WORKFLOW §8), no en masa; hay que mantener la disciplina de trailers para conservar la
  procedencia por herramienta.

## Referencias

Auditoría 2026-07 (hallazgos G1/G2, sección Git); WORKFLOW.md; directiva del dueño del repo (2026-07-16).
