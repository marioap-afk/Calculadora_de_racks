---
schema: rackcad-initiative/v1
id: I-50
title: "Cotas independientes por vista"
type: feature
status: claimed
branch: feature/cotas-independientes-por-vista
base_branch: main
priority:
size:
depends_on: []
conflicts_with: [I-49]
context_packs: [ui-editors, persistence, system-selective, architecture-kernel]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-50 — Cotas independientes por vista

> Fase actual: **G0 — reclamo publicado; bootstrap pendiente de completar con la fila durable en ROADMAP y el worktree local**.
>
> ```text
> BASE_SHA  = a4d88f18a1f42263d366c44dc05dd18a6786f152
> CLAIM_SHA = 97cc0ef66bf865175a8976e4e2452aa6a4e0d5e6
> Claim-Id  = 4d9fb22f-3c68-4fa9-b4bf-52ff9d9b8252
> Branch    = feature/cotas-independientes-por-vista
> ```

## 1. Objetivo

Permitir decidir de forma independiente **en qué vistas de un rack se muestran las cotas**, reutilizando el motor de cotas existente y **sin reconstruirlo**.

ID1 no pide crear cotas automáticas. El pendiente es seleccionar la visibilidad de las cotas por vista real soportada por cada sistema.

## 2. Pregunta de contrato que Discovery debe resolver

La hipótesis inicial es una política persistida por `rack + view-kind`, pero **no queda aceptada por este contrato**. Discovery debe demostrar si la autoridad correcta pertenece:

- al **tipo de vista** del rack, de modo que dos instancias `Front` compartan política; o
- a **cada instancia dibujada**, de modo que dos vistas del mismo kind puedan diferir.

Alternativas a comparar: flags explícitos, set de `ViewKind`, policy object y metadata por view instance. Si el producto no queda determinado por el comportamiento y contratos actuales, se escala al Owner antes de G2. Si Discovery exige una decisión arquitectónica transversal material, se pide revisión de Arquitecto antes de implementación.

## 3. Compatibilidad obligatoria

Los DWG existentes deben conservar **exactamente** el comportamiento visual actual. Debe existir un default legacy equivalente a la configuración vigente. Abrir o actualizar un dibujo viejo no puede hacer desaparecer cotas.

La política acordada debe sobrevivir el flujo completo:

`crear -> insertar -> RACKEDITAR -> Actualizar -> save -> reopen -> linked view insertion`.

Un redraw no puede recuperar silenciosamente una política global histórica y perder la selección por vista.

## 4. Alcance de Discovery

Auditar **todos los sistemas/productos con cotas existentes** y concretar por sistema:

- tipos de vista;
- dónde se generan cotas;
- propiedad global vigente;
- quién decide `DrawDimensions` o equivalente;
- persistencia actual;
- metadata/identidad de vista;
- insert/update/redraw;
- save/reopen;
- `RACKEDITAR`;
- library/export cuando aplique.

Entregable mínimo de G1: matriz `Sistema × Vista × cotas existentes × autoridad actual × persistencia × cambio necesario`, inventario completo de call sites de drawing y trazado create/update/reopen.

## 5. Alcance de producto

La fila original dice **Todos**. I-50 no puede declararse completa si solo cubre Selectivo. Discovery debe identificar qué sistemas poseen cotas y qué vistas soportan; una solución shared + adaptadores es válida si cae naturalmente del diseño existente.

No se crean controles para vistas o cotas que un sistema no posee hoy.

## 6. UI

Buscar el punto mínimo y coherente de configuración: creación, editor o una única sección `Cotas` con controles por vista según la taxonomía real. Evitar duplicar configuración en varias ventanas salvo necesidad demostrada.

**No tocar `LinkedPropertyEditor`, Expression Engine ni Project Variables salvo necesidad real demostrada.** I-49 corre en paralelo; cualquier archivo productivo compartido material debe reportarse antes de continuar.

## 7. Fuera de alcance

- rediseñar el motor completo de cotas;
- ID17 first-view freedom;
- ID18 multi-view queue;
- ID19 multi-rack projection;
- ID2 pallet visibility;
- ID13 blank fronts;
- Expression Engine;
- cotas nuevas no pedidas;
- refactor general de UI.

## 8. Tests mínimos de aceptación

- default legacy;
- Front ON / Side OFF / Plan OFF;
- Front OFF / Side ON;
- combinaciones relevantes por sistema;
- insert;
- update;
- RACKEDITAR;
- save/reload;
- linked sibling views;
- nueva vista creada después del cambio;
- cada sistema incluido;
- no cross-write entre vistas;
- no pérdida de geometría ni BOM.

## 9. Owner Validation

AutoCAD 2025, por cada sistema acordado: crear rack, elegir cotas por vista, insertar, verificar solo vistas seleccionadas, `RACKEDITAR`/Actualizar, save/reopen, insertar vista adicional y smoke legacy.

## 10. Gates

| Gate | Entregable | Estado |
|---|---|---|
| G0 | Preflight, claim, worktree, bootstrap | **EN CURSO** — claim y contrato publicados; faltan worktree local + fila ROADMAP |
| G1 | Characterization / matriz completa | pendiente |
| G2 | Contrato de autoridad + persistencia + default legacy | pendiente |
| G3 | UI mínima | pendiente |
| G4 | Builders / draw path | pendiente |
| G5 | Persistence / update | pendiente |
| G6 | Cobertura de sistemas | pendiente |
| G7 | Candidate + Owner AutoCAD 2025 | pendiente |
| G8 | Docs + integration + cleanup | pendiente |

**No implementation before G1/G2.** G3+ queda bloqueado hasta cerrar el contrato y, si aparece una decisión arquitectónica transversal material, hasta la revisión de Arquitecto correspondiente.

## 11. Conflicto con I-49

- no refactorizar `LinkedPropertyEditor`;
- no tocar Project Variables salvo necesidad real;
- commits pequeños;
- reportar archivos productivos compartidos materiales;
- integración solo tras preflight contra `main` actualizado.
