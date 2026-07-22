---
schema: rackcad-initiative/v1
id: I-17
title: Clon unico de cabecera (deep-clone via store de serializacion)
type: refactor
status: review-ready
branch: refactor/clon-unico-cabecera
base_branch: main
priority:
size: S
depends_on: [I-02]
conflicts_with: [I-14]
context_packs:
  - architecture-kernel
  - persistence
  - ui-editors
  - system-selective
  - system-dynamic-flowbed
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-17.yml
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-17 — Clon unico de cabecera

## 1. Objetivo

Que exista **un solo** deep-clone de `RackFrameConfiguration`, hecho **a traves del store de
serializacion** (`RackFrameProjectStore`), y que las **tres** copias hoy dispersas en la UI
(configurador de cabecera, ventana selectiva, ventana dinamica) lo consuman. El clon preserva el
diseno completo (metadatos, postes, placas, horizontales, paneles) y su modelo derivado se
reconstruye igual que al abrir un proyecto de disco, **sin cambio de comportamiento observable**.

Resultado verificable cuando: `RackFrameProjectStore` expone un `DeepCopy` unico; el configurador ya
no mantiene un clon manual campo-por-campo; el selectivo y el dinamico clonan por ese mismo `DeepCopy`;
pruebas de equivalencia demuestran la conservacion del estado y la equivalencia con las dos rutas de
serializacion previas; la suite pura y la de UI quedan verdes; los builds Debug de UI (y de
Plugin/solucion) cierran en 0 errores propios.

## 2. Problema

La auditoria 2026-07 (hallazgo **U4**, severidad ALTA, esfuerzo pequeno) documenta **tres**
implementaciones de deep-clone de `RackFrameConfiguration` en la UI: una **manual** y dos **por
serializacion**. La manual **se desincroniza con cada campo nuevo**: cada vez que
`RackFrameConfiguration` gana un campo hay que recordar anadirlo a mano en dos metodos del
configurador (`CloneConfiguration` y `CopyConfiguration`) y en siete ayudantes, ademas de la
persistencia; olvidarlo hace que un proyecto reabierto pierda estado en silencio (el mismo tipo de
regresion que ya obligo a documentar por que la celosia y los parametros de rejilla se copian). Las
otras dos ya usan el round-trip de un store, pero **por stores distintos** (el dinamico por
`RackFrameProjectStore`, el selectivo por `RackProjectStore` + `RackProject.ForSelective`), asi que la
regla de "un solo clon" no existe.

La direccion correcta es la que ya siguen las dos copias por serializacion: **el documento de
persistencia es la fuente unica de que campos componen el diseno**, y el clon debe ser su round-trip.
Asi, un campo nuevo se preserva en el clon **en el mismo momento** en que se anade a
`RackFrameProjectDocument` (que hay que tocar de todos modos para persistir), sin copia manual paralela.

## 3. Alcance

Autorizado por el ROADMAP (Fase 3, I-17: "Un solo deep-clone de `RackFrameConfiguration` via store de
serializacion; borrar las 3 copias de la UI (VM del configurador, selectivo, dinamico) + test de
equivalencia (U4)"). Sin ampliaciones laterales.

1. **Mecanismo canonico**: anadir `RackFrameProjectStore.DeepCopy(RackFrameConfiguration)` que devuelve
   `null` para entrada nula y en otro caso hace `Deserialize(Serialize(configuration))` — exactamente el
   round-trip que el dinamico ya usaba. La copia de cada campo la posee el esquema de persistencia; el
   modelo derivado (`Members`, elevaciones de panel) se reconstruye en la carga
   (`BracingPanelMemberBuilder.RefreshPhysicalModel`), igual que un proyecto abierto de disco.
2. **Dinamico** (`RackDynamicSystemWindow.Clone`): reexpresar como `DeepCopy` (comportamiento
   identico; ya era ese round-trip inline).
3. **Selectivo** (`RackSelectiveWindow.CloneCabecera`): pasar del round-trip por el **wrapper**
   (`RackProjectStore` + `RackProject.ForSelective(cfg).Header`) al `DeepCopy` de cabecera desnuda. Es
   equivalente: ambas rutas serializan el **mismo** `RackFrameProjectDocument`, reconstruyen el modelo
   derivado y rechazan una cabecera inusable.
4. **Configurador** (`RackFrameConfiguratorViewModel`): `CloneConfiguration` pasa a **delegar** en
   `DeepCopy`; se eliminan `CopyConfiguration` (copia en sitio) y los siete ayudantes manuales
   (`ClonePost`/`CloneBasePlate`/`CloneHorizontal`/`CloneBracingPanel`/`CloneFrameMember`/
   `CloneFrameMemberEnd`/`CloneException`, usados **solo** por esos dos metodos). Las dos rutas que
   usaban `CopyConfiguration` (`RestoreStandardConfiguration`, `ReplaceConfigurationAndReload`) pasan a
   **reasignar** `Configuration` con un clon fresco; la secuencia de recarga que sigue (LoadRows +
   Normalize + RefreshPhysicalMembers) reconstruye filas y modelo, de modo que el swap de referencia es
   equivalente a la copia en sitio previa.
5. **Pruebas de equivalencia (U4)**: en `RackCad.Tests`, sobre una configuracion rica (cada campo
   persistido con valor no-default), fijar que `DeepCopy` preserva toda la fuente de verdad (igualdad de
   forma de alambre), devuelve una instancia **independiente**, reconstruye el modelo derivado, es
   idempotente, tolera `null`, iguala el round-trip inline del dinamico e iguala la ruta wrapper previa
   del selectivo. En `RackCad.UI.Tests`, una prueba de `RestoreStandardConfiguration` que fija el swap de
   referencia del configurador.

## 4. Fuera de alcance

- **UI/XAML, geometria, BOM, GUID, persistencia fisica** (formato en disco/Xrecord) y **cualquier
  comportamiento visible**: no cambian. Si preservar el comportamiento exigiera tocarlos, **detenerse**.
- **Modificaciones internas de los stores de I-03** (`refactor/fallos-silenciosos`, en curso): I-17
  solo **anade** `DeepCopy` a `RackFrameProjectStore`; no toca escritura atomica, logging ni el resto de
  los stores.
- **Rediseno de los configuradores** o de la ventana selectiva/dinamica; **cambios de DTO**
  (`RackFrameProjectDocument` y familia se conservan intactos); **migraciones adicionales** de
  selectivo/dinamico (su estado propio ya vive en Application por I-20/I-21).
- Dependencias NuGet nuevas (politica cero-NuGet, ADR-0003).
- Cambios en `docs/HANDOFF.md` y `docs/ROADMAP.md`: se actualizan **solo** en la sesion de integracion
  (WORKFLOW seccion 8), no desde esta rama.

## 5. Contexto requerido

- `AGENTS.md` (direccion de dependencias Domain<-Application<-UI<-Plugin; regla en un solo sitio;
  definicion de terminado; archivos calientes) y `docs/WORKFLOW.md` (ciclo de iniciativa, cierre).
- `docs/ROADMAP.md` (I-17: Fase 3; depende de I-02; se estorba con I-14 y con trabajo en
  selectivo/configurador) y `docs/auditoria-arquitectura-2026-07.md` (hallazgo U4).
- Context Packs: `architecture-kernel`, `persistence`, `ui-editors`, `system-selective`,
  `system-dynamic-flowbed`, `delivery-validation`, `documentation-governance`.
- Codigo: `src/RackCad.Application/Persistence/RackFrameProjectStore.cs`,
  `RackFrameProjectDocument.cs`, `RackProjectStore.cs`, `RackProject.cs`;
  `src/RackCad.Application/RackFrames/BracingPanelMemberBuilder.cs`;
  `src/RackCad.UI/RackFrameConfiguratorViewModel.cs`, `RackDynamicSystemWindow.xaml.cs`,
  `RackSelectiveWindow.xaml.cs`; pruebas `tests/RackCad.Tests/RackFrameProjectStoreTests.cs`,
  `tests/RackCad.UI.Tests/RackFrameConfiguratorViewModelTests.cs`.

## 6. Dependencias

- **Integradas requeridas:** I-02 (`feature/dinamico-modular`), en `main` desde 2026-07-17 (ROADMAP).
- **Conflictos declarados:** I-14 (`architecture/ui-controls`), ya **integrada** (2026-07-21): no hay
  estorbo activo. La condicion "no en paralelo con trabajo en selectivo/configurador" se cumple: al
  reclamar, `origin` solo tenia `docs/adr-retroactivos` (I-07, docs) y `refactor/fallos-silenciosos`
  (I-03, logging/stores) activas; ninguna edita los editores selectivo/configurador.
- **Trabajo paralelo observado (no conflictivo):** I-03 (`refactor/fallos-silenciosos`) puede tocar
  `RackFrameProjectStore.cs` para escritura atomica; I-17 solo **anade** un metodo `DeepCopy` a ese
  archivo (region distinta), conciliable mecanicamente en el rebase de integracion. No es conflicto
  declarado de I-17.
- **Entradas del dueno:** ninguna. No hay `docs/automation/decisions/I-17.md`. Por ser un refactor sin
  comportamiento observable, `requires_autocad: false` y `requires_owner_validation: false` (seccion 10).

## 7. Archivos esperados

Modificar (produccion):

- `src/RackCad.Application/Persistence/RackFrameProjectStore.cs` — anadir `DeepCopy` (+15 lineas).
- `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` — `Clone` delega en `DeepCopy`.
- `src/RackCad.UI/RackSelectiveWindow.xaml.cs` — `CloneCabecera` delega en `DeepCopy`.
- `src/RackCad.UI/RackFrameConfiguratorViewModel.cs` — `CloneConfiguration` delega; se eliminan
  `CopyConfiguration` y los 7 ayudantes; dos rutas reasignan `Configuration`.

Crear (pruebas):

- `tests/RackCad.Tests/RackFrameConfigurationDeepCopyTests.cs` — equivalencia/independencia/idempotencia
  del `DeepCopy` y equivalencia con las dos rutas previas (dinamica y selectiva).

Modificar (pruebas):

- `tests/RackCad.UI.Tests/RackFrameConfiguratorViewModelTests.cs` — prueba de
  `RestoreStandardConfiguration` (swap de referencia).

Modificar (documentacion):

- `docs/initiatives/I-17-clon-unico-cabecera.md` (este contrato), `docs/initiatives/README.md`
  (indice, solo anadir la entrada de I-17), `docs/automation/state/I-17.yml` (estado versionado).

No se espera tocar XAML, DTO de persistencia, geometria, BOM, catalogos, DrawServices, Plugin,
deploy ni `.github/workflows`. Una desviacion material obliga a detenerse (seccion 12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato desde
   `TEMPLATE.md`, indice y estado versionado. (Evidencia: rama remota aceptada, este archivo.)
2. **Mecanismo y consumidores.** `DeepCopy` en el store; dinamico, selectivo y configurador delegan; se
   elimina el clon manual. (Evidencia: build UI verde, diff.)
3. **Pruebas de equivalencia.** Suite pura de `DeepCopy` + prueba de restore del configurador.
   (Evidencia: suites verdes.)
4. **Gates.** `RackCad.Tests`, `RackCad.UI.Tests`, builds Debug de UI/Plugin/solucion, CI de la rama.
   (Evidencia: seccion 14.)
5. **Cierre de sesion.** Revision de diff, commits logicos, push de la rama, estado versionado.
   (Evidencia: seccion 14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` — suite completa verde con las nuevas
  de equivalencia, **sin regresion**.
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug` — suite UI verde con la nueva de
  restore, en el runner STA existente.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build RackCad.sln -c Debug` — build completo (Plugin incluido) sin errores; solo los
  `MSB3277` conocidos de las referencias de AutoCAD.
- CI: los cuatro jobs (Tests, Build UI, UI Tests, Build Plugin sin AutoCAD) verdes sobre la punta
  publicada de la rama.

## 10. Validacion manual

**No aplica.** I-17 es un refactor de la mecanica de clonado **sin cambio de comportamiento
observable**: el diseno clonado es identico (fijado por las pruebas de equivalencia de forma de
alambre y por la equivalencia con las dos rutas de serializacion previas), y no cambia dibujo,
geometria, BOM, GUID, persistencia fisica ni la UI. Por eso `requires_autocad: false` y
`requires_owner_validation: false` (analogo a I-09/I-10/I-24: equivalencia mecanica + suites + CI). Si
el diff real invalidara esa premisa, **no** se declaran los gates cerrados: se detiene y se explica la
contradiccion (seccion 12).

## 11. Criterios de aceptacion

- Existe **un** `RackFrameProjectStore.DeepCopy` y las tres copias de la UI lo consumen; **no** queda
  clon manual campo-por-campo en el configurador (0 referencias a los 7 ayudantes ni a
  `CopyConfiguration`).
- Las pruebas de equivalencia (U4) pasan y son **significativas**: fijan preservacion total de la fuente
  de verdad, independencia de la instancia, reconstruccion del modelo derivado, idempotencia,
  tolerancia a `null`, equivalencia con el round-trip del dinamico y con la ruta wrapper del selectivo;
  la prueba de UI fija el swap de referencia del configurador.
- `RackCad.Tests` y `RackCad.UI.Tests` verdes sin regresion; builds Debug de UI (y Plugin/solucion donde
  el entorno lo permite) en 0 errores propios; CI verde en los cuatro jobs.
- Se conserva la direccion de dependencias (el `DeepCopy` vive en Application; la UI solo lo consume) y
  no hay dependencias NuGet nuevas ni cambios de DTO/persistencia fisica.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija tocar UI/XAML, geometria, BOM, GUID, persistencia fisica, DTO o
  reglas de producto: **detenerse con gate de alcance**.
- Que el `DeepCopy` no pueda ser el round-trip del store (p. ej. una ruta cliente clonara una
  configuracion **inusable** que el store rechaza), obligando a una semantica distinta por consumidor.
- Que aparezca en `origin` otra rama tocando de forma incompatible los editores selectivo/configurador
  o `RackFrameProjectStore`: no sobrescribir; entregar evidencia.
- Cualquier necesidad de un paquete NuGet nuevo o de un cambio de DTO de persistencia.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en `docs/automation/state/I-17.yml`. La automatizacion esta pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesion. No
se abre un segundo Pull Request ni se activa auto-merge. Al ser un refactor sin gates de AutoCAD ni
owner-validation, tras CI verde el estado queda `review-ready` con `gate: none`; la integracion a
`main` (`git merge --no-ff`, WORKFLOW seccion 4.5) se realiza en la sesion de integracion, no en esta
rama. `docs/HANDOFF.md` y el estado en `docs/ROADMAP.md` se actualizan **solo** en la sesion de
integracion (ultimo commit de la rama), nunca desde esta rama.

## 14. Evidencia final

Se completa al cierre de la sesion: commits logicos con trailer de procedencia, archivos
creados/modificados, resultados de `dotnet test` (suite pura y UI) y de los builds, evidencia de CI
sobre el SHA publicado, SHA base y punta de la rama, confirmacion del push y confirmacion de que `main`
no fue modificada. Riesgos y validaciones pendientes se listan al entregar.
