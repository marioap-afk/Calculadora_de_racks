# ADR-0002: Secuencia de integración de la rama del dinámico modular

- **Estado:** aceptado
- **Fecha:** 2026-07-16 (propuesta); **decisión: 2026-07-17**
- **Decisores:** dueño del repo (decisión directa, 2026-07-17); redactado por Claude
- **Iniciativa relacionada:** I-01 (`docs/decision-dinamico-modular`, Paso 0 y decisión) y
  I-02 (`feature/dinamico-modular`, ejecución)

## Contexto

`codex/dinamico-modular` contiene la reescritura del sistema dinámico: +12,621 líneas en 85
archivos (3 commits), bifurcada de `cd20200` — es decir, SIN los 6 arreglos verificados de
`eaede44`, que tocan la persistencia de cabecera que el dinámico consume (`RackFrameProjectDocument`
perdía 4 campos; `RackFrameProjectStore` sin SchemaGuard). La rama además modifica archivos
compartidos (`SystemBomBuilder`, stores, 6 CSV de catálogo) e infla
`RackDynamicSystemWindow.xaml.cs` de 1,332 a 3,318 líneas con los mismos patrones (code-behind,
tubería clonada) que el roadmap quiere erradicar.

Todo refactor estructural (registro de sistemas, editor shell, persistencia uniforme) toca esos
mismos archivos: arrancarlos ANTES de resolver esta rama garantiza un conflicto de 12k líneas
después. Es la decisión de secuencia que bloquea la Fase 2 del ROADMAP.

## Paso 0 — evidencia ejecutada (2026-07-17)

El Paso 0 propuesto (compilar la rama en su worktree, cargar con NETLOAD el DLL de ESE worktree y
recorrer el checklist funcional) se ejecutó el 2026-07-17 **sobre `9f19a8c`, ANTES del rebase sobre
`main`**. Resultado completo en [0002-paso0-evidencia.md](0002-paso0-evidencia.md):

- Automatizado: 627/627 tests de la suite completa, 138/138 del subconjunto dinámico, build UI con
  0 errores/0 advertencias, build Plugin con 0 errores (solo las MSB3277 conocidas).
- Manual (dueño, AutoCAD 2025): 17/17 pruebas OK — editor multi-frente, geometría, camas, IN/OUT e
  intermedios, las cuatro vistas, seguridad, BOM, persistencia, RACKEDITAR, actualización en sitio,
  legacy y el desviador de la frontal de entrada (el único pendiente que la rama declaraba).
- Sin fallos bloqueantes ni pérdida de información. Conflicto textual del futuro rebase medido:
  solo 7 archivos de documentación (los arreglos de `eaede44` viven en archivos que la rama no tocó).

## Decisión

**Opción A — integrar primero la rama validada, mediante I-02 (`feature/dinamico-modular`).**
Aceptada por el dueño del repositorio el 2026-07-17 con base en la evidencia anterior.

I-02 debe:

1. Preservar `9f19a8c` mediante tag de resguardo antes de tocar nada.
2. Renombrar la rama a `feature/dinamico-modular` (ADR-0001).
3. Rebasar sobre `origin/main` (publicar con `--force-with-lease`, nunca `--force`).
4. Conservar íntegras las correcciones actuales de `main` (en particular `eaede44`).
5. Resolver los conflictos documentales con la estructura vigente de `main` (post I-00).
6. Conservar los catálogos de la rama (las +38 filas; append-only).
7. Ejecutar la suite completa y los builds Debug de UI y Plugin.
8. Re-validar en AutoCAD **sobre el árbol ya rebasado** (WORKFLOW §4.5.3).
9. Integrarse mediante `merge --no-ff` a `main` (HANDOFF §8-12 y ROADMAP como último commit).
10. Limpiar rama y worktree al terminar, con el borrado seguro de WORKFLOW §3.

Los refactors de Fase 2 parten del árbol resultante y el editor dinámico nuevo se convierte en
candidato principal de la migración al editor shell (Fase 5, I-21).

**Contingencia:** si I-02 no se estabiliza dentro de tres sesiones, detener la iniciativa y
redactar un ADR nuevo que proponga reemplazar ADR-0002 por la opción B. ADR-0002 no se modifica
retroactivamente (ADR aceptado = ADR inmutable).

## Alternativas consideradas

- **Opción B — descartarla formalmente** (tag `archive/dinamico-modular`, borrar rama y worktree,
  re-implementar el dinámico DESPUÉS del registro de sistemas y el editor shell usando la rama
  archivada como referencia): **descartada con la evidencia actual**. Sus criterios no se
  cumplen — no fallan flujos fundamentales, BOM y persistencia son confiables, no hay pérdida de
  datos y la funcionalidad está validada de punta a punta; re-implementar descartaría ~12,600
  líneas funcionando para pagar después el costo que B pretendía evitar, cuando el rebase medido
  resultó ser esencialmente documental. Queda como alternativa de contingencia vía ADR nuevo.
- **Integrar "cuando toque", sin secuencia explícita** — descartada: cada semana de espera agranda
  el conflicto; es exactamente el patrón que produjo la rama zombie y la huérfana.
- **Refactorizar primero y rebasar la rama encima** — descartada: obliga a rebasar 12.6k líneas
  sobre una base reorganizada; el costo del rebase se multiplica en vez de reducirse.

## Consecuencias

- El trunk absorbe pronto una pieza grande ya escrita y validada; el costo es una sesión de
  integración/validación intensa (I-02), con la suite combinada y la re-validación en AutoCAD
  post-rebase como red.
- Mientras I-02 no se integre: congelado todo trabajo nuevo sobre el subsistema dinámico y no se
  abren I-08/I-09/I-11/I-14/I-15/I-16/I-17 (sus archivos son los que I-02 reescribe).
- La deuda de arquitectura de la ventana dinámica (3,318 líneas de code-behind) se hereda a
  sabiendas y se paga en I-21, igual que la del selectivo.

## Referencias

Auditoría 2026-07 §5 y hallazgo G4; crítico de completitud (hallazgo "rama subestimada");
HANDOFF §11.1; WORKFLOW.md §8; evidencia del Paso 0:
[0002-paso0-evidencia.md](0002-paso0-evidencia.md); decisión del dueño del repo (2026-07-17).

## Notas posteriores

**2026-07-17** — I-02 ejecutó la decisión A: completó el rebase sobre `main`, la suite combinada
quedó verde y la validación manual post-rebase del dueño en AutoCAD quedó OK. La opción A se
ejecutó correctamente en una sola sesión de estabilización; la contingencia (opción B) no se
activó. El detalle vive en HANDOFF §8-12.
