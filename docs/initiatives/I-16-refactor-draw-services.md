---
schema: rackcad-initiative/v1
id: I-16
title: Refactor de Draw Services del Plugin
type: refactor
status: integrated
branch: refactor/draw-services
base_branch: main
priority:
size: M
depends_on: [I-09]
conflicts_with: [I-09, I-10]
context_packs:
  - autocad-plugin
  - delivery-validation
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-16 — Refactor de Draw Services del Plugin

> Estado: **F0-F5 completadas; INTEGRADA en `main` el 2026-07-21** (merge `--no-ff`). CI de rama verde;
> build y tests locales verdes; **validación manual en AutoCAD 2025 aprobada** (registro:
> [I-16-autocad-validation.md](I-16-autocad-validation.md)). El detalle de la integración vive en `docs/HANDOFF.md` §5;
> la integración es manual del dueño (WORKFLOW §4.5). Los conteos de pruebas y hashes canónicos viven en
> `docs/HANDOFF.md` §12, no aquí.

## 1. Objetivo

Reducir la duplicación de los `*DrawService` del Plugin **sin cambiar comportamiento observable**. Hoy
siete servicios de dibujo (`SelectiveFrontalDrawService`, `SelectivePlantaDrawService`,
`DynamicSystemDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`, `FlowBedDrawService`,
`PlantaHeaderDrawService`) son cascarones de orquestación casi idénticos que embudan todo a
`DynamicSystemPlan → SystemBlockWriter`/`LateralHeaderDrawer` y difieren solo por especializaciones por
vista. `LateralHeaderDrawService` es la clase «rica» de la que se tallaron las demás: hospeda primitivas
compartidas (colocación con jig, limpieza de inserción cancelada, carga de catálogo, `DescribeMissing`)
además de su lógica propia.

El resultado esperado es: (a) **colapsar la boilerplate** de los siete servicios detrás de un servicio de
vista compartido; (b) **extraer, interno a `RackCad.Plugin`**, la superficie de colocación y la carga
compartida de catálogo hoy alojadas en `LateralHeaderDrawService`; (c) **uniformar el manejo de `regen`**
conservando el único `Regen` final por operación multivista. La forma concreta del servicio compartido
(nombre, genericidad y firma) es una **dirección de diseño**, no una decisión de este contrato: la fija la
línea base. La equivalencia se sostiene con **tests golden** sobre las estructuras que el harness prueba
sin AutoCAD (en particular `DynamicSystemPlan`) más **inventario mecánico y revisión de equivalencia**
para nombres de bloque, sufijos, mensajes y firmas. La validación humana en AutoCAD queda en **muestreo**
al integrar (ROADMAP principio 4; I-16 no está marcada con ✋).

## 2. Problema

La duplicación es el hallazgo E4/P3 de la auditoría 2026-07 y la razón de que la pista B del Plugin se
serialice `I-09 → I-16 → I-10`. I-09 ya extrajo helpers de comandos y dejó los `*DrawService` intactos y
**reservados para I-16** (contrato de I-09, §4). El esqueleto `null-guard → cargar catálogo → construir
DynamicSystemPlan → crear bloque → colocar` (dibujar) y su equivalente de redibujo se repiten en los siete
servicios; `LateralHeaderDrawService` concentra primitivas que los siete consumen. Esa concentración
impide que la lista de archivos calientes del Plugin encoja y obliga a I-10 a declarar estorbo con esta
área.

## 3. Alcance

Estrictamente el alcance de I-16 en ROADMAP, sin ampliaciones laterales:

- **Colapsar la boilerplate de los siete servicios** en un servicio de vista compartido, dejando
  **aisladas** las especializaciones por vista/servicio (la construcción del plan, el nombre de bloque y
  los mensajes) mediante una descomposición equivalente que la línea base valide. La genericidad, el
  nombre y la firma del servicio compartido **no se fijan aquí**.
- **Extraer, interno a `RackCad.Plugin`, la superficie de colocación** hoy en `LateralHeaderDrawService`
  (colocación con jig, colocación en punto fijo, informe de colocación y limpieza de inserción cancelada)
  a una superficie compartida que consuman el servicio de vista y `LateralHeaderDrawService`.
- **Extraer, interno a `RackCad.Plugin`, la carga compartida de catálogo** hoy en
  `LateralHeaderDrawService.LoadCatalog()`, sin cambiar su comportamiento (misma fuente, mismo fallback a
  catálogo vacío, misma caché por firma).
- **Uniformar el manejo de `regen`** en un punto guardado por el flag, conservando el contrato del único
  `Regen` final por operación multivista (§11).
- **Preservar el comportamiento observable y las fachadas públicas** (§11, §7).

## 4. Fuera de alcance

- **`RackCad.UI`**: no se modifica. **No se unifica `UiSupport.LoadCatalogSafe()`** con la carga de
  catálogo del Plugin; la extracción es **interna a `RackCad.Plugin`**. Si procede, se anota el duplicado
  en `docs/ideas-futuras.md`.
- **«Catálogo» = carga de datos de catálogo** (`LoadCatalog`), **no** «familia de DrawServices».
- **Geometría** (`LateralHeaderDrawer`): no se toca (creación/redefinición de bloques, parámetros
  dinámicos, cotas, mirror, extents).
- **Persistencia / payload / GUID** (`RackBlockData`, `RackEmbedDocument`/`RackEmbedStore`): no se toca.
- **BOM**: sin cambios de conteo ni de recetas.
- **I-08 (`architecture/system-registry`)**: no se toca `SystemRegistry`, `RackProjectStore`,
  `RackDesignLibrary` ni la validación de Application.
- **I-10 (`architecture/kind-handlers`)**: no se introduce `IRackKindHandler`/registro; los switches y el
  despacho por `Kind` (incluido `RACKEDITAR`) se conservan.
- **Mover lógica a `RackCad.Domain`/`RackCad.Application`** para hacerla testeable, o **crear un proyecto
  de tests del Plugin**: fuera de alcance (§9).
- **Cualquier cambio funcional** (geometría nueva, comando nuevo, BOM, persistencia/schema, catálogos,
  UX); ningún ajuste «de paso».
- **`src/RackCad.Domain`, `src/RackCad.Application`** (más allá de leer), `assets/catalogs`, `deploy`.
- **`docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/initiatives/README.md`** y cualquier documentación
  distinta de este contrato (solo se tocan al integrar, WORKFLOW §4.5).
- Merge, auto-merge, integración o limpieza de rama/worktree.

## 5. Contexto requerido

- Fuentes globales: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`,
  `docs/ARCHITECTURE.md` (§§2-5; AutoCAD §5; identidad/round-trip §4.1; BOM §4.3).
- Context Packs `autocad-plugin` y `delivery-validation`, con sus guías
  (`docs/guias/generacion-cabecera-lateral.md`, `docs/guias/validacion-manual-autocad.md`).
- Colaboradores de solo lectura: `LateralHeaderDrawer.cs` (geometría), `RackBlockData.cs` (persistencia),
  las clases de comando que consumen los servicios o `LoadCatalog` (`RackSelectivoCommands`,
  `RackCabeceraCommands`, `RackDinamicoCommands`, `RackCamaCommands`, `RackInventarioCommands.BomTotal`) y
  los builders de plan de Application que producen el `DynamicSystemPlan`.
- La suite `tests/RackCad.Tests` como red de regresión (Domain + Application, sin AutoCAD).

## 6. Dependencias

- **Depende de I-09** (`refactor/plugin-commands`), integrada el 2026-07-20; sus helpers de comando ya
  están en `origin/main`.
- **Estorbos declarados en ROADMAP** («se estorba con»): I-09 e I-10. I-09 está integrada, así que el
  único estorbo vivo es **I-10 (`architecture/kind-handlers`)**, ausente en `origin` al reclamar. Si
  aparece durante la vida de I-16, se aplica §12.
- **Convivencia con I-08 (`architecture/system-registry`)**: reclamada en `origin` y corre en paralelo.
  **No es estorbo**: capas distintas (I-08 = Application; I-16 = Plugin) y ROADMAP no las lista mutuamente
  en «se estorba con». Al reclamar I-16 se verificó que la rama de I-08 no toca ningún archivo propiedad de
  I-16. I-16 no consume `SystemRegistry` ni contratos de I-08.
- Convive con I-07 (`docs/adr-retroactivos`): distinta capa (docs), sin archivos calientes compartidos.
- Sin entrada del dueño requerida para abrir la iniciativa.

## 7. Archivos esperados y fachadas

**Propiedad de I-16 — capa `src/RackCad.Plugin` (Plugin), más el contrato y sus tests.**

Nuevos (en `src/RackCad.Plugin`, salvo el contrato y los tests):

- El servicio de vista compartido que colapsa la boilerplate (forma y nombre por definir en la línea base).
- La superficie interna de colocación y la carga compartida de catálogo, extraídas de
  `LateralHeaderDrawService` **dentro del Plugin**.
- Este contrato: `docs/initiatives/I-16-refactor-draw-services.md`.
- Tests golden en `tests/RackCad.Tests`.

Modificados:

- Los siete `*DrawService` colapsables (redistribución mecánica hacia el servicio compartido).
- `LateralHeaderDrawService.cs` (delega colocación y carga de catálogo a las superficies extraídas;
  conserva su lógica propia).
- `SystemBlockWriter.cs` (punto único de `regen`).

**Fachadas públicas — la regla es preservarlas.** Las fachadas existentes que consumen las clases de
comando no cambian. Un cambio de firma solo se acepta si es **estrictamente necesario**, la **línea base
lo demuestra** y **no altera el comportamiento observable**; en ese caso se actualiza su sitio de llamada
dentro del Plugin. Los call-sites de comando **solo** se tocan si la extracción obliga a adaptar una
firma; cuando sea razonable se preservan mediante fachadas (por ejemplo, conservando el punto de entrada
estático de la carga de catálogo), de modo que ningún call-site cambie.

No se esperan cambios bajo `src/RackCad.Domain`, `src/RackCad.Application` (más allá de su lectura),
`src/RackCad.UI`, `assets/catalogs`, `deploy`, `docs/HANDOFF.md`, `docs/ROADMAP.md` ni
`docs/initiatives/README.md`. Una desviación material obliga a detenerse.

## 8. Fases

- [x] **F0. Reclamo atómico**: rama + worktree desde `origin/main`, commit vacío de reclamo (con
  `Claim-Id` nuevo) y primer push aceptado sin force; base verificada al reclamar (`origin/main` verde);
  sin estorbo activo (I-10 ausente en `origin`); I-08 verificada sin solape de archivos.
- [x] **F1. Contrato + línea base golden**: contrato publicado y línea base establecida (golden
  `DrawServicePlanBaselineTests` del `DynamicSystemPlan` por vista/payload + inventario mecánico de nombres,
  sufijos, mensajes y firmas).
- [x] **F2. Extraer colocación y carga de catálogo** (interno al Plugin) sin cambiar comportamiento:
  `RackCatalogLoader` y `BlockPlacement` extraídos con fachadas; los siete servicios y
  `LateralHeaderDrawService` los consumen.
- [x] **F3. Colapsar la boilerplate** de los siete servicios en `ViewBlockDraw` (colaborador no genérico con
  delegados), aislando las especializaciones por vista y preservando las invariantes de §11.
- [x] **F4. Uniformar `regen`** en `SystemBlockWriter.ApplyRegen`, preservando el contrato de §11.
- [x] **F5. Verificar equivalencia**: golden verdes; inventario antes/después de nombres, mensajes y
  `Regen`; suite completa; builds Debug UI y Plugin; CI de rama verde. Muestreo AutoCAD **ejecutado y
  aprobado** por el dueño (§10; registro en `I-16-autocad-validation.md`).

Cada fase cierra con commit + push de la rama (respaldo, WORKFLOW §4.3) y evidencia revisable.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
git diff origin/main --check
```

- **Tests golden (automatizados)**: cubren **solo** estructuras que `tests/RackCad.Tests` prueba **sin
  AutoCAD** — en particular el `DynamicSystemPlan` resuelto por vista/payload (patrón «ARRAY == plano» ya
  existente). Cualquier diferencia en el plan detiene la fase.
- **Inventario mecánico y revisión de equivalencia**: nombres de bloque, sufijos, mensajes
  (prompts/keywords/`Failure`) y firmas se comprueban por inventario antes/después, **no** por golden
  automatizado (viven en el Plugin, que referencia AutoCAD; el harness actual no los ejecuta). **No** se
  crea un proyecto de tests del Plugin ni se mueve lógica a Domain/Application para testearla.
- La suite existente completa permanece verde en cada fase.
- Build Debug de UI y Plugin con 0 errores; los `MSB3277` conocidos de las referencias de AutoCAD no
  cuentan.
- CI de rama verde (Tests, Build UI, Build Plugin without AutoCAD) al cerrar cada sesión.

## 10. Validación manual

I-16 **no está marcada con ✋** en ROADMAP. La equivalencia se sostiene con los golden de
`DynamicSystemPlan`, el inventario mecánico y la suite existente; la validación humana en AutoCAD queda en
**muestreo** al integrar (ROADMAP principio 4). Si un cambio dejara de ser equivalente (geometría, nombre
de bloque, mensaje, BOM, payload/GUID o el patrón de `Regen`), se convierte en cambio de comportamiento y
sale del alcance: detenerse, no «arreglar de paso».

## 11. Criterios de aceptación

- Los siete servicios comparten la boilerplate colapsada, con sus especializaciones por vista **aisladas**
  mediante una descomposición equivalente validada por la línea base. La genericidad o la firma del
  servicio compartido no son en sí un criterio de aceptación.
- **Invariantes de comportamiento observable preservadas** (verificadas por golden de plan e inventario
  mecánico):
  - **`Regen`**: el flag conserva su default; los editores multivista siguen suprimiendo el regen
    intermedio y disparando un **único `Regen` final** gateado por su contador de cambios (incluidas las
    vistas borradas); la cama regenera una vez internamente; layout y fill conservan su `Regen`. Sin regen
    nuevo por bloque.
  - **Nombres de bloque**: se preservan por vista, incluidos los sufijos por vista, sin colisiones nuevas
    ni impacto en la identidad round-trip por vista.
  - **Mensajes**: verbatim, incluida la asimetría dibujar/actualizar dentro de un servicio.
  - **Cancelación del jig**: se preserva la limpieza de la definición fantasma con payload (y sus defs
    anidadas) para que el escaneo por GUID de `RACKEDITAR` no la redibuje.
  - **Payload/GUID, BOM, geometría y persistencia**: intactos; en particular se preserva la reconstrucción
    de conteos de la cabecera lateral.
  - **Colocación y carga de catálogo**: extraídas dentro de `RackCad.Plugin`, sin tocar `RackCad.UI`; la
    carga conserva fuente, fallback y caché.
- **Fachadas públicas preservadas** (§7); un cambio de firma solo procede si es estrictamente necesario,
  demostrado por la línea base y sin alterar comportamiento observable.
- Suite completa y builds Debug de UI y Plugin sin fallos ni errores/advertencias propias; golden verdes.
- Sin cambios funcionales ni cambios bajo Domain, Application, UI, `assets/catalogs`, `deploy`, ni en
  `docs/HANDOFF.md`/`docs/ROADMAP.md`/`docs/initiatives/README.md` fuera de la sesión de integración.

## 12. Condiciones para detenerse

- Un paso exige un cambio de comportamiento observable (§11).
- La extracción o el colapso obligarían a introducir `IRackKindHandler`/registro (I-10), a tocar geometría
  (`LateralHeaderDrawer`), persistencia (`RackBlockData`), BOM o `RackCad.UI`.
- Una especialización por vista no puede aislarse sin unificar/reagrupar el plan (rompe el caso all-loose
  o duplica el agrupamiento) o sin cambiar una firma que altere comportamiento.
- La carga de catálogo no puede extraerse dentro del Plugin sin cambiar su comportamiento o sin cruzar a
  `RackCad.UI`.
- Aparece en `origin` la rama de I-10 (`architecture/kind-handlers`): estorbo activo.
- I-08 empieza a tocar un archivo propiedad de I-16, o aparece otra sesión activa sobre este worktree o
  esta rama.
- `origin/main` avanza con conflictos semánticos.
- El inventario mecánico o los golden detectan una divergencia observable no explicable por un cambio
  puramente mecánico.
- Un hallazgo fuera de alcance se anota en `docs/ideas-futuras.md` y se detiene; no se «arregla de paso».

## 13. Estado versionado y entrega del Pull Request

Iniciativa **manual**: la automatización permanece **deshabilitada** (HANDOFF §4: sin ejecutor nocturno ni
horarios). No se crea `docs/automation/state/I-16.yml` ni Pull Request en esta fase. El reclamo y el
respaldo de la iniciativa son la rama remota `refactor/draw-services` (WORKFLOW §2 y §4.1); cada sesión
cierra con commit + push de la rama, sin esperar aprobación. La implementación podrá quedar `completed`,
pero `completed` no significa integrada: el rebase final, el merge `--no-ff`, el CI posterior de `main`, el
muestreo de validación en AutoCAD y la limpieza segura de rama y worktree son operaciones manuales del
dueño, externas a este contrato. Nunca se abre un segundo Pull Request para la iniciativa. Git y los
resultados verificables prevalecen sobre cualquier campo `status` del front matter.

## 14. Evidencia final

**F0-F5 completadas; iniciativa INTEGRADA en `main` el 2026-07-21** (merge `--no-ff`; rebaseada sobre el `main`
con I-08 antes de integrar). El detalle de la integración (SHAs, tests, validación) vive en `docs/HANDOFF.md` §5.

- **F0**: reclamo atómico (`Claim-Id: 5838ddaa-af47-4879-8d34-1d2f0c768005`); sin estorbo de I-10; sin solape
  con I-08.
- **F1**: contrato + línea base golden (`DrawServicePlanBaselineTests`) + inventario mecánico (baseline).
- **F2**: `RackCatalogLoader` y `BlockPlacement` extraídos (interno al Plugin) con fachadas; los siete
  servicios los consumen.
- **F3**: orquestación de los siete colapsada en `ViewBlockDraw`; firmas y especializaciones (`postIndex`,
  `DynamicRackEnd`, all-loose) preservadas.
- **F4**: regen uniformado en `SystemBlockWriter.ApplyRegen`; 7 ubicaciones efectivas de `Regen` preservadas.
- **F5**: equivalencia verificada; **CI de rama verde**; **build y tests locales verdes** (suite completa, UI
  y Plugin Debug, solo `MSB3277`); **validación manual en AutoCAD 2025 aprobada** por el dueño (registro:
  [I-16-autocad-validation.md](I-16-autocad-validation.md)).

Integración ejecutada: rebase final sobre `main` (con I-08 integrada), actualización de
`docs/HANDOFF.md`/`docs/ROADMAP.md`/índice y merge `--no-ff`; solo resta la limpieza segura de rama y worktree
(WORKFLOW §3/§6). Los conteos de
pruebas y hashes canónicos viven en `docs/HANDOFF.md` §12, no aquí.
