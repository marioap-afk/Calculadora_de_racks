---
schema: rackcad-initiative/v1
id: I-05
title: Guardia de unidades
type: feature
status: integrated
branch: feature/guardrail-unidades
base_branch: main
priority:
size: S
depends_on: []
conflicts_with: []
context_packs:
  - autocad-plugin
  - delivery-validation
  - documentation-governance
automation_state_path: docs/automation/state/I-05.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-05 — Guardia de unidades

## 1. Objetivo

RackCad genera TODA su geometría en pulgadas (los builders, catálogos y jigs asumen pulgadas). Si el
DWG destino declara otra unidad en `INSUNITS` (milímetros, metros…) o ninguna (unitless), un rack
recién insertado aterriza a escala de pulgadas y queda desproporcionado frente al dibujo (p. ej. 25.4×
sobre un plano en milímetros). Hoy el Plugin **nunca** lee `INSUNITS`, así que ese error de escala es
silencioso (auditoría 2026-07, hallazgo **D4**, severidad ALTA).

Esta iniciativa añade una **guardia visible y NO bloqueante** en el límite de AutoCAD: al insertar un
sistema o una vista nueva, o al ejecutar `RACKLAYOUT`/`RACKRELLENAR`, el Plugin lee `INSUNITS` y, si el
dibujo no está en pulgadas, escribe **una sola advertencia** en la línea de comandos antes de la primera
modificación del DWG. Además documenta la estrategia de unidades a largo plazo en un **ADR** (nuevo,
`docs/adr/0005-estrategia-de-unidades.md`).

Resultado verificable cuando: existe la guardia centralizada en `RackCad.Plugin`; todas las rutas de
inserción autorizadas y `RACKLAYOUT`/`RACKRELLENAR` (con sus alias) la invocan una sola vez por
operación; una actualización en sitio (`RACKEDITAR` «Actualizar») NO advierte y una inserción de vista
nueva SÍ; la decisión pura (pulgadas ⇒ sin aviso; cualquier otro valor, incluido unitless ⇒ aviso) queda
cubierta por pruebas y el cableado por source-guards; los builds Debug de UI y Plugin quedan en 0 errores
propios; CI queda verde. **Ninguna** geometría, coordenada, builder, BOM, GUID, capa, persistencia,
payload/Xrecord, comando, alias ni mensaje ajeno cambia. El ADR queda en estado `propuesto` (la
aceptación es del dueño).

## 2. Problema

Hallazgo **D4** de la [auditoría 2026-07](../auditoria-arquitectura-2026-07.md): «Mono-unidad implícita en
pulgadas: columna `units` decorativa, cero conversiones, el Plugin jamás lee `INSUNITS` → escala errónea
silenciosa sobre planos métricos». La recomendación priorizada #5 de esa auditoría y la sección de
Unidades («ADR explícito. Corto plazo: validación dura (leer `INSUNITS`, avisar si el DWG no está en
pulgadas). Largo plazo: decidir si la columna `units` se honra con conversión real») se adelantaron a la
Fase 1 como I-05 (ROADMAP, fila I-05 y tabla de revalidación #5).

El usuario que dibuja sobre un plano en milímetros no recibe ninguna señal: el rack se inserta 25.4×
más pequeño y solo se descubre por inspección visual. Una advertencia temprana y barata evita horas de
retrabajo. Se elige **advertir**, no **abortar**: el flujo de trabajo del dueño y los casos legítimos (un
dibujo unitless que el usuario sabe interpretar) no deben bloquearse; la conversión real es una decisión
mayor que este ADR deja para una iniciativa futura.

## 3. Alcance

Autorizado por el ROADMAP (fila I-05) y esta instrucción:

1. **ADR de estrategia de unidades** (`docs/adr/0005-estrategia-de-unidades.md`, estado `propuesto`):
   pulgadas como unidad interna canónica vigente; guardia visible en el límite de AutoCAD como solución
   inmediata; ausencia de conversión/reescalado/reinterpretación automática en I-05; estrategia futura
   basada en una frontera explícita entre unidades del DWG y unidades internas (sin implementarla ahora);
   consecuencias, alternativas y condiciones para una futura iniciativa de conversión real. Actualizar el
   índice `docs/adr/README.md`.
2. **Guardia mínima en `RackCad.Plugin`** (único proyecto que toca la API de AutoCAD): un helper interno
   que lee `INSUNITS` del `Database` activo y, si no es pulgadas, escribe **una** advertencia no
   bloqueante. La decisión pura (¿advierte esta unidad?) vive en una clase pura sin dependencia de
   AutoCAD en `RackCad.Application` (testeable); el mapeo del valor de unidades de AutoCAD a esa categoría
   neutral se queda en el Plugin.
3. **Cableado en las rutas de inserción autorizadas**, antes de la primera modificación del DWG y una sola
   vez por operación (sin repetir por alias ni por vista/bloque):
   - menú `RACKCAD` (las cuatro inserciones nuevas), `RACKSELECTIVO`, `RACKSISTEMADINAMICO`, `QUICKCAMA`,
     `RACKCABECERA`, `QUICKCABECERA` — inserción nueva;
   - `RACKEDITAR` (`EditSelective`/`EditDynamic`/`EditCabecera`) SOLO cuando inserta una vista nueva
     (`!UpdateOnly`); una actualización pura NO advierte; la edición de cama nunca inserta vista nueva y
     por tanto nunca advierte;
   - `RACKLAYOUT`/`RLY` y `RACKRELLENAR`/`RR`: pasan por la guardia antes de sus prompts funcionales.
4. **Pruebas**: decisión pura (pulgadas/unitless/métrica) en `RackCad.Tests`; cableado por source-guards
   que leen el `.cs` del Plugin como texto (mismo patrón que `KindHandlerGuardSourceTests`).

## 4. Fuera de alcance

- **Conversión automática, reescalado o reinterpretación** de dibujos, bloques o coordenadas. La guardia
  solo advierte; jamás toca geometría.
- Cambiar catálogos o su columna `units` (sigue decorativa por ahora).
- Rediseñar UI, resolvers, builders, BOM, DrawServices, jigs, GUID, nombres, capas, persistencia,
  payload/Xrecords o cualquier mensaje ajeno.
- Almacenar unidades en diseños o DTO; construir un framework general de unidades; mover conocimiento de
  la API de AutoCAD a Domain/Application.
- `RACKDUPLICAR` (clona geometría ya dibujada a la misma escala del original, no reconstruye desde los
  builders en pulgadas: no introduce una nueva discrepancia de unidades) y los comandos de solo lectura
  (`RACKLISTA`, `RACKBOMTOTAL`, ayuda): fuera de la guardia por diseño.
- Aceptar el ADR (es decisión del dueño): queda `propuesto`.

## 5. Contexto requerido

- `AGENTS.md` (Plugin = único que toca AutoCAD; dirección de dependencias Domain←Application←UI←Plugin;
  mensajes de línea de comandos sin acentos; definición de terminado).
- `docs/WORKFLOW.md` (ciclo de iniciativa, reclamo atómico §4.1, cierre §5) y `docs/ROADMAP.md` (fila
  I-05, revalidación #5).
- `docs/ARCHITECTURE.md` y `docs/auditoria-arquitectura-2026-07.md` (hallazgo D4 y sección Unidades).
- Context Packs: `autocad-plugin`, `delivery-validation`, `documentation-governance`.
- Precedente de arquitectura testeable: I-10/`fix/kind-handler-missing-errors` (`KindDispatch<T>` puro en
  Application + `KindHandlerGuardSourceTests` que leen el `.cs` del Plugin como texto, sin cargar el
  ensamblado — ADR-0003).
- Código: `src/RackCad.Plugin/RackMenuCommands.cs`, `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`,
  `RackCamaCommands.cs`, `RackCabeceraCommands.cs`, `RackLayoutCommands.cs`, `RackLayoutCommands.Fill.cs`;
  los seams de colocación (`Systems/ViewBlockDraw.cs` `DrawAndPlace` vs `RedrawInPlace`).

## 6. Dependencias

- **Integradas requeridas:** ninguna (I-05 no depende de otra iniciativa; ROADMAP «Depende de: —»).
- **Conflictos que deben permanecer inactivos:** ninguno (ROADMAP «Se estorba con: —»). I-22
  (`refactor/safety-placement`) e I-24 (`refactor/ui-tests-editores`) están en curso en sus propios
  worktrees pero NO comparten archivo con I-05 (I-22 toca la colocación de seguridad del selectivo; I-24
  añade pruebas a `RackCad.UI.Tests`; I-05 toca la superficie de comandos del Plugin, un tipo puro nuevo
  de Application y pruebas de `RackCad.Tests`). Las integraciones son serializadas.
- **Entradas del dueño:** aceptación del ADR-0005 (gate `owner-decision`) y validación en AutoCAD +
  owner-validation al cierre (gates abiertos, §10). No se requiere ninguna decisión para arrancar la
  implementación.

## 7. Archivos esperados

Crear (producto):

- `src/RackCad.Application/Drawing/DrawingUnitsAdvisory.cs` — política pura: enum neutral `DrawingUnits`
  (`Inches`/`Unitless`/`Other`, sin tipos de AutoCAD) + `RequiresInsertionAdvisory(DrawingUnits)`.
- `src/RackCad.Plugin/RackUnitsGuard.cs` — límite AutoCAD: lee `INSUNITS`, clasifica el `UnitsValue`,
  escribe una advertencia; texto del mensaje (línea de comandos, sin acentos).

Modificar (producto, cableado mínimo, sin cambio de comportamiento salvo la advertencia nueva):

- `src/RackCad.Plugin/RackMenuCommands.cs`, `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`,
  `RackCamaCommands.cs`, `RackCabeceraCommands.cs`, `RackLayoutCommands.cs`, `RackLayoutCommands.Fill.cs`.

Crear (pruebas, `tests/RackCad.Tests/`):

- `DrawingUnitsAdvisoryTests.cs` (decisión pura: pulgadas/unitless/métrica).
- `RackUnitsGuardSourceTests.cs` (source-guards: rutas conectadas, gating por `UpdateOnly`, `EditCama`
  sin guardia, layout antes de prompts, lee-no-asigna `INSUNITS`, sin conversión).

Crear/actualizar (docs):

- `docs/adr/0005-estrategia-de-unidades.md` (nuevo, `propuesto`) y `docs/adr/README.md` (índice).
- `docs/initiatives/I-05-guardrail-unidades.md` (este contrato), `docs/initiatives/README.md` (índice),
  `docs/automation/state/I-05.yml` (estado versionado).

No se espera modificar geometría, resolvers, builders, BOM, DrawServices, jigs, catálogos, persistencia/
DTO/envelope, `deploy/` ni `.github/workflows`. Una desviación material obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push; ADR-0005
   (`propuesto`) e índice; contrato, índice de iniciativas y estado versionado. (Evidencia: rama remota
   aceptada, este archivo, ADR.)
2. **Política pura + pruebas.** `DrawingUnitsAdvisory` en Application y `DrawingUnitsAdvisoryTests`.
   (Evidencia: `RackCad.Tests` verde con las nuevas pruebas.)
3. **Guardia del Plugin + cableado.** `RackUnitsGuard` y su invocación en cada ruta autorizada; source-
   guards que demuestran rojo contra la baseline sin cablear y verde tras cablear. (Evidencia: source-
   guards verdes; builds Debug de UI y Plugin en 0 errores propios.)
4. **Gates automatizados.** Suite completa + suite UI + build de la solución + CI de la rama sobre el SHA
   publicado. (Evidencia: §14.)
5. **Cierre de sesión.** Revisión de diff, commits lógicos, push de la rama, estado versionado y gates
   manuales documentados como abiertos. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` — suite completa verde + decisión pura
  y source-guards nuevos.
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug` — suite UI verde (sin regresión;
  I-05 no toca UI).
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug -v:minimal` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -v:minimal` — 0 errores propios (solo
  los `MSB3277` conocidos de AutoCAD; requiere AutoCAD 2025 cerrado para el build completo).
- `dotnet build RackCad.sln -c Debug -v:minimal` — build completo de la solución.
- CI: los cuatro jobs (Tests, Build UI, UI Tests, Build Plugin sin AutoCAD) en verde sobre la punta
  publicada de la rama.

Solo se toleran los `MSB3277` ya conocidos; sin errores ni advertencias propias nuevas.

## 10. Validacion manual

Gates **APROBADOS por el dueño (Mario Pérez) el 2026-07-22**, sin observaciones, sobre el DLL Debug del
worktree (`…-I-05-guardrail-unidades\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`),
código validado `f78baaf`, en AutoCAD 2025. AutoCAD **requerido** (`requires_autocad: true`; ROADMAP marca
I-05 con ✋). Confirmaciones: «Ok, todo funciona»; durante la prueba sobre un DWG no configurado en
pulgadas el dueño confirmó que apareció correctamente la advertencia completa de RackCad. Evidencia
versionada en [`docs/automation/evidence/I-05-autocad-validation.md`](../automation/evidence/I-05-autocad-validation.md).

- [x] Dibujo en **pulgadas** (`INSUNITS`=Inches): insertar selectivo/dinámico/cama/cabecera desde menú y
      comandos directos ⇒ **sin** aviso de unidades; geometría idéntica a lo vigente.
- [x] Dibujo en **milímetros / no-pulgadas**: cada inserción ⇒ **una** advertencia de unidades
      antes de colocar; el rack se dibuja igual que hoy (sin conversión), solo aparece el aviso.
- [x] Dibujo **unitless** (`INSUNITS`=0): cada inserción ⇒ advertencia (unitless también advierte).
- [x] `RACKEDITAR` «Actualizar» (en sitio) sobre un dibujo no-pulgadas ⇒ **sin** aviso.
- [x] `RACKEDITAR` «Insertar» (vista nueva lateral/planta) sobre un dibujo no-pulgadas ⇒ advertencia.
- [x] `RACKLAYOUT`/`RLY` y `RACKRELLENAR`/`RR` sobre un dibujo no-pulgadas ⇒ advertencia antes de los
      prompts; el layout/relleno se coloca igual que hoy.
- [x] Alias (`RS`, `RSD`, `QCM`, `RCB`, `QCB`, `RK`, `RED`, `RLY`, `RR`): **un solo** aviso por operación.
- [x] Geometría, BOM, GUID, capas, persistencia y round-trip **idénticos** a lo vigente en todos los casos.
- [x] **owner-validation**: la advertencia es clara, no bloquea, y no altera el dibujo.

## 11. Criterios de aceptacion

- Existe `RackUnitsGuard` en `RackCad.Plugin` que lee `INSUNITS` del `Database` activo y escribe **una**
  advertencia no bloqueante cuando la unidad no es pulgadas; es el único sitio que lee `INSUNITS`.
- La decisión pura vive en `RackCad.Application` (`DrawingUnitsAdvisory`), sin dependencia de AutoCAD,
  sin conversión y sin almacenar unidades; cubierta por pruebas (pulgadas ⇒ no; unitless ⇒ sí; otra ⇒ sí).
- Todas las rutas de inserción autorizadas y `RACKLAYOUT`/`RACKRELLENAR` (con alias) invocan la guardia
  una sola vez por operación, antes de la primera modificación del DWG; una actualización pura con
  `RACKEDITAR` no advierte y una inserción de vista nueva sí; `EditCama` no advierte.
- Los source-guards pinan el cableado y demuestran rojo contra la baseline sin cablear.
- **Sin** cambios de geometría, coordenadas, builders, BOM, GUID, nombres, capas, persistencia, payload/
  Xrecord, comandos, alias ni mensajes ajenos; sin dependencias NuGet nuevas; dirección de dependencias
  intacta (Application no referencia AutoCAD).
- Builds Debug de UI, Plugin y solución en 0 errores propios; CI verde en la rama.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija tocar geometría, builders, resolvers, BOM, DrawServices,
  persistencia/DTO/envelope o coordenadas: detenerse antes de ampliar alcance.
- Que la guardia obligue a cambiar la superficie pública de comandos, alias o el XAML/controles.
- Cualquier deriva hacia conversión, reescalado, columna `units`, almacenamiento de unidades en DTO o un
  framework general de unidades.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test) o de mover conocimiento de AutoCAD a
  Domain/Application.
- Que aparezca en `origin` otra rama tocando las mismas superficies de comando de forma incompatible: no
  sobrescribir; entregar evidencia.

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-05.yml`. La automatización está pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesión. No se
abre un segundo Pull Request ni se activa auto-merge. Los gates `owner-decision` (aceptación de
ADR-0005), `autocad` y `owner-validation` quedan **abiertos** (pendientes del dueño); la integración a
`main` (`git merge --no-ff`, WORKFLOW §4.5) se realiza en la sesión de integración, no en esta rama.

## 14. Evidencia final

Implementación **completa y validada**; **integrada en `main`** en la sesión de integración serializada del
**2026-07-22** (`git merge --no-ff`; este documento **no inventa** el SHA del merge, que vive en
`git log --first-parent main`). Resumen verificable:

- **SHA base:** `9a895e417b485749a6bc62edd643486a43495f0d` (`origin/main`, sin avanzar → sin rebase).
- **Implementación validada:** `f78baaf209c118d168c68620e236341996f9d93e` (punta de código; commits
  posteriores son **solo documentales** y no cambian código).
- **Pruebas:** `RackCad.Tests` 936/936; `RackCad.UI.Tests` 139/139. Builds Debug de UI/Plugin/solución en
  0 errores propios (solo los `MSB3277` conocidos de AutoCAD). Decisión pura + source-guards con
  demostración rojo→verde contra la baseline sin cablear.
- **CI:** run `29932135203` sobre `f78baaf`, **cuatro jobs en `success`** (Tests Domain+Application, Build
  UI, UI Tests, Build Plugin without AutoCAD).
- **Gates del dueño — APROBADOS el 2026-07-22:**
  - `owner-decision`: ADR-0005 **aceptado** por Mario Pérez («Sí, estoy de acuerdo»); decisión versionada
    en [`docs/automation/decisions/I-05.md`](../automation/decisions/I-05.md).
  - `autocad` + `owner-validation`: **aprobados** sin observaciones («Ok, todo funciona»); evidencia en
    [`docs/automation/evidence/I-05-autocad-validation.md`](../automation/evidence/I-05-autocad-validation.md).
- **Invariantes:** diff exclusivamente aditivo; **sin** cambio de geometría, coordenadas, builders, BOM,
  GUID, capas, persistencia/DTO, catálogos, `deploy/` ni workflows; sin dependencias NuGet nuevas.
- **`main` no fue modificada.** El estado versionado vive en
  [`docs/automation/state/I-05.yml`](../automation/state/I-05.yml) (`state: integration-ready`,
  `gate: none`).

`docs/HANDOFF.md` §1-5 y el estado en `docs/ROADMAP.md` (I-05 → `integrada (2026-07-22)`) **se actualizan en
este cierre de integración** como último commit de la rama (WORKFLOW §4.5.4), antes del `git merge --no-ff`.
