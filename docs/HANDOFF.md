# Project Handoff

> Estado vivo de RackCad para continuidad entre sesiones. Actualizado: **2026-07-17**.
> La arquitectura se consulta en [ARCHITECTURE.md](ARCHITECTURE.md), el proceso en
> [WORKFLOW.md](WORKFLOW.md), el plan en [ROADMAP.md](ROADMAP.md), los procedimientos en
> [guias/](guias/) y la historia anterior en
> [archivo/transicion-2026-07/handoff-historial-2026-07.md](archivo/transicion-2026-07/handoff-historial-2026-07.md).

## 1. Resumen y estado actual

RackCad es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para diseñar y dibujar racks industriales
con BOM. El trunk único es `main`; Domain y Application son puros, UI usa WPF sin AutoCAD y Plugin
es el único adaptador de la API de AutoCAD.

El producto mantiene cuatro familias operativas: cabecera, selectivo, dinámico modular y cama de
rodamiento. Comparten identidad por GUID embebida en DWG, edición round-trip y vistas ligadas. El
dinámico modular de I-02 y la instalación segura de I-04 están integrados.

I-06 (`docs/reestructura`) está en revisión documental. Su rama reorganiza las fuentes y no cambia
comportamiento de producto; no está integrada hasta que el dueño valide el resultado y ejecute la
sesión manual de integración descrita en WORKFLOW.

## 2. Última validación real

La última validación manual de comportamiento sigue siendo I-02 sobre `b0de31d`, después del rebase
sobre `main`: el dueño cargó el DLL Debug del worktree en AutoCAD 2025 y confirmó el checklist
completo del dinámico modular, incluidos vistas, seguridad, BOM, persistencia, round-trip, escenario
legacy y rendimiento. No se realizó ni se requiere una validación nueva en AutoCAD para I-06 porque
su alcance es documental.

La guía vigente para futuras validaciones está en
[guias/validacion-manual-autocad.md](guias/validacion-manual-autocad.md).

## 3. Problemas y riesgos activos

- `ParrillaFrente` y `ParrillaCantidad` siguen siendo globales al rack; una configuración
  heterogénea puede requerir overrides por frente o nivel en una iniciativa futura.
- En medio frente, la cantidad de parrilla es por tramo; el comportamiento es intencional, pero
  debe comprobarse contra el uso real.
- El build del Plugin puede emitir los `MSB3277` conocidos de las referencias de AutoCAD y falla al
  copiar DLL si AutoCAD los mantiene cargados.
- `RackDynamicSystemWindow.xaml.cs` conserva deuda de code-behind; I-21 define su migración futura.
- El fallback legacy del dinámico conserva cabeceras sin procedencia como personalizadas para evitar
  pérdida de datos.
- Los catálogos de producto y los overrides del usuario aún comparten ubicación; I-04 preserva el
  DWG de bloques, pero la separación de capas de datos sigue diferida.

## 4. Siguiente acción

El dueño debe revisar I-06 en el PR draft #1: suficiencia de este HANDOFF reducido, exactitud de
ARCHITECTURE, utilidad de los nueve Context Packs, navegación y clasificación de documentos. Si la
validación es satisfactoria, la integración a `main` se realiza manualmente conforme a WORKFLOW; no
se debe ejecutar I-07 antes de esa integración.

## 5. Última verificación vigente

La baseline integrada de `main` corresponde a I-04 (`8e52828` como punta de integración):

- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug y Release: **0 errores**, únicamente las familias `MSB3277` conocidas;
- harness del instalador: **25/25 verificaciones** en rutas temporales;
- CI de I-04: Success sobre `f82a49f`.

La evidencia técnica de la rama I-06 se conserva bajo [automation/runs/](automation/runs/). Al no
usar GitHub CLI ni API, la sesión de cierre no declara estado de CI remoto.

## 6. Preguntas abiertas

1. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global según el
   uso real?

## 7. Decisiones vigentes pendientes de I-07

> **Conservación temporal obligatoria:** estas decisiones proceden de la antigua sección 7 y aún
> gobiernan el proyecto. Solo podrán retirarse de HANDOFF cuando I-07 integre los ADRs retroactivos
> correspondientes. I-06 no las convierte en ADR ni reabre su contenido.

| Decisión | Motivo / alcance vigente |
|---|---|
| Solo `RackCad.Plugin` toca AutoCAD | Geometría y BOM permanecen puros y testeables en Application. |
| Catálogos CSV Excel-first, sin base de datos | El usuario los edita en Excel; se conserva fallback Windows-1252 y caché por firma. |
| Un solo `secciones.csv` con columna `rol` | Postes, celosía, largueros y separadores comparten una hoja y FKs explícitas. |
| Identidad por GUID embebido en DWG | El nombre visible no es una identidad estable. |
| `Actualizar` redibuja; `Insertar` agrega una vista ligada | Convención permanente de los cuatro editores. |
| Parámetros dinámicos mediante patrón ARRAY | Evita fijar parámetros repetidamente por referencia. |
| Cero dependencias NuGet en producto | Mantiene simple el despliegue; toda excepción requiere decisión explícita. |
| Parrilla: una por tarima y regla en `SelectiveFrontalBuilder.ParrillaRow` | Dibujo, BOM y UI concuerdan por construcción. |
| Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | El DTO sigue explícito para compatibilidad y round-trip. |
| Entrada numérica localizada sin agrupadores | Acepta punto o coma decimal sin transformar valores ambiguos. |
| Cantidad de parrilla: UI rechaza si no cabe y builder acota | Evita dibujo fuera del marco y degrada de forma segura tras cambios. |
| Validación de cargas diferida a RAM Elements | No debe re-proponerse dentro del alcance actual. |
| Optimizador IA de layout diferido | `RACKLAYOUT` es el motor determinista vigente, no el optimizador futuro. |

