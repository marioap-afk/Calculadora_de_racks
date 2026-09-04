---
schema: rackcad-initiative/v1
id: I-43
title: Selectivo — edicion por alcance y fondos
type: feature
status: integrated
branch: feature/selectivo-scopes-fondos
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [system-selective, ui-editors, persistence, delivery-validation]
automation_state_path:
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

# Selectivo — edicion por alcance y fondos

> **INTEGRADA y CERRADA el 2026-09-04.** Validacion manual del Owner en AutoCAD 2025 **PASS TOTAL** sobre el DLL
> construido exactamente desde `d582deed5bbd93083261399e45b2ecc3e16088d7` (SHA-256
> `f70d89bffad38cf77fd8b5b51e2951512e34f2af5b7050392c590d8ff4a06d87`).
> [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) queda **aceptado**, e I-43 ya
> tiene fila en [ROADMAP.md](../ROADMAP.md).
>
> **Recorrido.** Gate 8 PASS funcional sobre `de100ed`; primera revision arquitectonica (8.5) que produjo el plan
> v1.1 y el contrato escrito ANTES de codificar (8.6A); gates 8.6B-8.6G; **segunda revision independiente (8.9)** con
> dictamen *C - CONDITIONAL*; **Gate 8.6H** correctivo; **Gate 8.10** incorporacion de `main` por merge; Owner PASS;
> Gate 9 integracion.
>
> **Lo que la segunda revision destapo y 8.6H cerro:** R2-01 (BLOCKER - encoger el numero de fondos desde uno que
> desaparecia pisaba la matriz del superviviente), R2-02 (el indice destino del combo se validaba ANTES del commit y
> podia quedar fuera de rango), R2-03 (un gesto estructural podia comprometer una celda sin recalcular), R2-05
> (avisos que se pisaban), R2-06 («poste F3» se leia como un fondo), R2-07 (textos contradictorios), R2-08
> (cobertura ausente) y **R2-10** (la nomenclatura de BOTA de Push Back se habia filtrado al Selectivo: vuelve a
> `Ninguno / Izquierda / Derecha / Ambas`, con los MISMOS ordinales, asi que ninguna seleccion guardada cambia de
> significado).
>
> **Evidencia final.** CI pre-Owner **33916118566** success; `RackCad.Tests` **4643 PASS**;
> `RackCad.UI.Tests` **1216 PASS / 17 skip**; **P0 61/61**; focal I-44 **22/22** (Push Back intacto).
>
> **Diferidos, en [ideas-futuras.md](../ideas-futuras.md):** R2-04, R2-09, ARQ-43-10 (`EffectiveCustomAt` sigue
> imponiendo el Depth in-place, a proposito), ARQ-43-11, ARQ-43-12, ARQ-43-14, ARQ-43-15, ARQ-43-16, ARQ-43-08B,
> **ID12 (SPLIT)** e ID13. Dos piden **decision del dueno**: el vocabulario de BOTA del **DINAMICO** -conservado
> exactamente como estaba porque no hay decision registrada para ese sistema- y una confusion de display de «Fondo de
> tarima»/«Fondo de cabecera» que el dueno reporto una vez y **no se pudo reproducir**.

## 1. Objetivo

Que el editor Selectivo permita editar **cualquier propiedad sobre los fondos que el usuario elija**,
con un alcance interno explicito, sin que un gesto de foco accidental escriba en fondos que el usuario
no eligio y sin que un valor solo tecleado llegue al documento.

Resultado verificable: los doce escenarios de revalidacion manual del §10 pasan en AutoCAD 2025 sobre
el DLL Debug construido desde el SHA final de la rama, con las suites completas verdes y CI verde.

## 2. Problema

El Selectivo editaba un solo fondo a la vez: el visible. Con racks de doble o triple profundidad eso
obliga a repetir cada edicion fondo por fondo, y las propiedades que si eran por fondo (cabeceras,
profundidades) no tenian una autoridad declarada.

La primera entrega resolvio el eje de fondos, pero la revision arquitectonica (Gate 8.5) documento que
el contrato resultante **no esta escrito** y que, en el codigo, la frontera entre "lo que el usuario
tecleo" y "lo que el editor comprometio" no existe:

- las cuatro cajas (`FondosBox`, `BayCountBox`, `FondoBox`, `CabeceraFondoBox`) son a la vez editor y
  autoridad, de modo que un `LostFocus` sin edicion real redimensiona o reprofundiza todos los destinos;
- `BuildDesign` lee texto de esas cajas, asi que un valor no comprometido puede llegar al documento o
  estampar el slot de un fondo que no es destino;
- la elevacion del larguero a piso tiene tres redacciones incompatibles y su materializacion salta el
  slot del fondo seleccionado;
- el frontal de un fondo `k > 0` no muestra su propia cabecera custom;
- la semilla y los avisos de "Personalizar" se calculan sobre el fondo 0;
- la suite de UI lee y escribe el `settings.json` real del usuario;
- varios comentarios y tooltips afirman contratos que ya no son ciertos.

## 3. Alcance

Autorizado por el plan de correccion v1.1, **un gate por vez** y solo los archivos permitidos de cada
gate:

| Gate | Contenido | AutoCAD |
|---|---|---|
| 8.6A | Contrato documental propuesto: ADR-0032 `propuesto`, este contrato, ideas-futuras | No |
| 8.6B | Aislamiento de settings en tests: factory unica, guard, reescritura del test contaminado | No |
| 8.6C | Frontera pendiente/comprometido, commit atomico y ordenado, fronteras transaccionales | No |
| 8.6D | Materializacion de la elevacion por frente y siembra en los frentes nuevos | No |
| 8.6F | Lector puro de cabecera custom y vista por fondo (frontal `Fk` con su cabecera) | No |
| 8.6E | Semilla por fondo visible, validacion de altura por destino, ancho del medio frente | No |
| 8.6G | Contratos falsos en comentarios y tooltips | No |
| 8.7 | Regresion automatica completa, builds Debug UI y Plugin, CI verde | No |
| 8.8 | Revalidacion manual del Owner | **Si** |

Orden obligatorio: 8.6A → 8.6B → 8.6C → 8.6D → 8.6F → 8.6E → 8.6G → 8.7 → 8.8. **8.6F va antes que
8.6E** (F es Application puro y no depende de C; E depende de C y de F).

El contrato tecnico que estos gates implementan es
[ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md): decisiones del
Owner O-43-01/02/03, separacion pendiente/comprometido con commit atomico y ordenado, tabla de
autoridades, elevacion directa por `(fondo, frente)`, cabecera por `(fondo, poste)` con `Height` en la
receta y `Depth` del fondo destino, persistencia aditiva sin cambio de esquema y preferencia de
destinos en `UserSettings`.

## 4. Fuera de alcance

Prohibido en toda la iniciativa, incluso "de paso" (tocar cualquiera de estos rechaza el gate):

- ViewModel, estado de frente dedicado, aggregate nuevo, framework de comandos o refactor transversal
  de `RackSelectiveWindow`;
- migracion de datos y cambio de `SchemaVersion` o de DTO;
- cambios fuera del sistema Selectivo (Dinamico, Push Back, Cantilever, cama);
- los follow-ups registrados en [`docs/ideas-futuras.md`](../ideas-futuras.md): purificacion del lector
  efectivo, retirada del global de Domain, extraccion de la carga fuera de la ventana, unificacion de
  resultados y resolvers, limpieza de API legacy, seams de dialogo, politica de esquema, estado de
  frente dedicado, *pallet stop* por lado (SPLIT), frentes en blanco, aviso "tramos no caben" y el
  resumen que describe el fondo 0;
- HANDOFF, ROADMAP y guias de usuario hasta despues de 8.8;
- cleanup, renombres y "mejoras de paso";
- Gate 9 (integracion) en cualquiera de sus partes.

En 8.6A ademas: **no se toca `src/`, `tests/`, DTO, esquema, `UserSettings`, UI ni CI.**

## 5. Contexto requerido

- [AGENTS.md](../../AGENTS.md) — convenciones obligatorias (direccion de dependencias, regla en un solo
  sitio, persistencia versionada, formato de commits).
- [docs/WORKFLOW.md](../WORKFLOW.md) — ramas, worktrees e integracion. **No existe `ORCHESTRATION.md`
  en el arbol**; WORKFLOW manda.
- [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) — el contrato de
  esta iniciativa.
- [ADR-0029](../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md) — contrato funcional comun de
  ventanas WPF, del que ADR-0032 es la especializacion del Selectivo.
- [ADR-0030](../adr/0030-fondo-por-celda-push-back-y-envolvente-derivada.md) — precedente de "la
  propiedad pertenece a la celda; lo del frente es derivado".
- Plan de correccion arquitectonica post-Gate 8 de I-43, version 1.1 (artifact aprobado): unica fuente
  de verdad de los gates, los tests P0 y los mutantes.
- Context Packs: `system-selective` (el sistema que se edita), `ui-editors` (la ventana WPF y el shell),
  `persistence` (DTO, round-trip y compatibilidad legacy), `delivery-validation` (builds, CI y
  validacion manual en AutoCAD). Se declaran por el alcance real de los archivos de los gates.

## 6. Dependencias

- **Integradas** y presentes en `main` (`085ca2f`): el estado del editor selectivo en Application, el
  Editor Shell y la migracion del Selectivo al shell, y la doble profundidad por fondo. La rama sale
  de `main` sin divergencia (`merge-base` = `085ca2f`).
- **Decision del dueno requerida:** aceptacion de ADR-0032. Las decisiones O-43-01/02/03 ya estan
  tomadas y **no se reabren**.
- **Validacion del dueno requerida:** Gate 8.8 en AutoCAD 2025.
- **Conflictos:** ninguno declarado. La rama toca exclusivamente el Selectivo.

## 7. Archivos esperados

**Gate 8.6A (este gate):**

- `docs/adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md` (nuevo, `propuesto`);
- `docs/adr/README.md` (una fila de indice);
- `docs/initiatives/I-43-selectivo-scopes-fondos.md` (nuevo, este archivo);
- `docs/ideas-futuras.md` (una seccion de follow-ups).

**Gates posteriores** (segun el plan; una desviacion material obliga a detenerse):

- 8.6B: `tests/RackCad.UI.Tests/` — factory unica de construccion de la ventana, guard de construccion
  y reescritura del test contaminado; los 13 archivos que construyen la ventana.
- 8.6C: `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml(.cs)`, un helper de campo pendiente
  en `src/RackCad.UI/`, y una clase de tests nueva. **Application sin cambios en este gate.**
- 8.6D: `src/RackCad.Application/Systems/Selective/SelectiveEditorState.cs` y la carga de la ventana.
- 8.6F: `SelectiveCabeceraAuthority.cs` y `SelectiveDepthLayout.cs`.
- 8.6E: `SelectivePostGeometry.cs` (helper puro) y `RackSelectiveWindow.xaml.cs`.
- 8.6G: comentarios y tooltips de la lista cerrada del plan.

## 8. Fases

Cada gate es un commit (o un par: tests y fix) independiente y reversible con `git revert`, con push de
la rama al cerrarlo y **parada obligatoria** para que el Coordinador apruebe antes del siguiente.

1. **8.6A** — contrato documental propuesto. Sin tests, sin build.
2. **8.6B** — aislamiento de settings. RED: el guard falla listando los sitios; el test contaminado
   falla con un gateway limpio.
3. **8.6C** — frontera pendiente/comprometido. RED: los P0 A, C, D, F, G, H, S, W, X fallan sobre la
   punta de 8.6B; B, E, T, U se documentan como caracterizacion.
4. **8.6D** — materializacion de la elevacion. RED: P0 I y J.
5. **8.6F** — lector puro y frontal por fondo. RED: P0 N y O; P protege compatibilidad.
6. **8.6E** — semilla, validacion de altura y ancho del medio frente. RED: P0 K, L, M, Q.
7. **8.6G** — contratos falsos. Sin comportamiento; solo build.
8. **8.7** — regresion completa, builds y CI; entrega del DLL Debug para 8.8.
9. **8.8** — revalidacion manual del Owner.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

- **Test rojo antes del fix** en cada gate con comportamiento: se escriben los P0 del gate, se ejecutan
  sobre la punta del gate anterior y se registran los fallos (nombre + asercion) en el cuerpo del
  commit.
- **Mutantes** indicados por gate: se aplican localmente, se comprueba el rojo y se revierten.
- CI verde sobre el SHA de cada gate. Builds con **0 errores** y **AutoCAD cerrado**.
- Hasta cerrar 8.6B, **respaldar y restaurar `%APPDATA%\RackCad\settings.json`** antes de correr la
  suite de UI en la estacion del dueno: la suite actual lo escribe.
- **Gate 8.6A no requiere pruebas ni build**: es documental.

## 10. Validacion manual

Gate 8.8, en AutoCAD 2025, con el DLL Debug del worktree construido desde el SHA final de 8.7. Doce
escenarios: (1) tabular por Frentes, tarima y cabecera sin editar no cambia nada; (2) retipear el mismo
valor si aplica a los destinos; (3) con el fondo visible fuera de los destinos solo cambian los
destinos; (4) teclear sin salir del campo y pulsar Guardar/Actualizar compromete ambos valores, y con
un valor invalido la accion se rechaza sin cambiar nada; (5) personalizar cabecera en un fondo mayor
que 1; (6) el frontal `Fk` muestra esa cabecera; (7) la preview del fondo `k` coincide; (8) custom
multi-destino con alturas distintas produce **un solo** aviso consolidado que nombra los fondos;
(9) "Aplicar de todos modos" lleva esa altura a lateral, planta y BOM; (10) "Cancelar" deja mutacion
cero; (11) guardar en biblioteca, actualizar y reabrir con `RACKEDITAR` conserva elevaciones y
cabeceras, y un documento legacy reabierto dibuja igual; (12) smoke de "Seleccionadas" con topologias
divergentes y de "Medio frente" en un frente que solo tiene el fondo mas largo.

## 11. Criterios de aceptacion

1. Gates 8.6A–8.6G aprobados por el Coordinador con su evidencia.
2. Suites completas verdes y builds Debug de UI y Plugin con 0 errores; CI verde sobre el SHA final.
3. `settings.json` con hash identico antes y despues de la suite de UI.
4. Los doce escenarios del §10 con veredicto PASS del Owner.
5. ADR-0032 aceptado por el dueno.
6. Ningun archivo fuera de los previstos por gate (`git diff --stat` y `git range-diff` respecto a
   `de100ed`).

Cumplir estos criterios significa **implementada**, no integrada: el merge es Gate 9 y es manual.

## 12. Condiciones para detenerse

- Contradiccion entre el plan v1.1, el codigo real, AGENTS.md o WORKFLOW.md: se reporta, no se resuelve
  por cuenta propia.
- Necesidad aparente de cambiar el DTO, `SchemaVersion` o cualquier store: contradiccion critica.
- Cualquier cambio que exija tocar un archivo fuera de la lista del gate, o un follow-up del §4.
- HEAD distinto del punto de partida esperado, worktree sucio o trabajo ajeno en la rama.
- Un gate cerrado: se entrega la evidencia y **se espera aprobacion explicita del Coordinador** antes
  de continuar.
- Falta de la decision del dueno sobre ADR-0032 antes de Gate 9.

## 13. Estado versionado y entrega del Pull Request

No hay `docs/automation/state/I-43.yml` ni Pull Request abierto: la iniciativa avanza por gates
coordinados con push directo a `feature/selectivo-scopes-fondos`, sin `--force` salvo rebase
autorizado. La automatizacion esta **deshabilitada** para esta iniciativa (`automation.enabled: false`)
y el merge automatico esta prohibido. Si el Coordinador decide publicar estado versionado o abrir un
Pull Request, se hace con el bloque canonico del TEMPLATE y nunca se abre un segundo Pull Request.

Las decisiones del dueno, si llegan por archivo, iran en `docs/automation/decisions/I-43.md`; hasta
entonces la instruccion del Coordinador es la fuente.

## 14. Evidencia final

Por gate se entrega: SHA de la punta tras el gate, salida completa de `dotnet test` de ambos proyectos
con sus conteos, lista de tests RED con su asercion antes del fix, resultado de los mutantes,
`git diff --stat` del gate, builds Debug de UI y Plugin y el id de la ejecucion de CI verde. En 8.6B
se anade el hash de `settings.json` antes y despues; en 8.6F el censo de callers repetido; en 8.7 el
SHA-256 del `RackCad.Plugin.dll` Debug del worktree.

`main` no se modifica en ningun gate: permanece en `085ca2f5b33541cfb93c8cdec8cbc8f0368c899f` hasta el
merge `--no-ff` de Gate 9, que no ejecuta el implementador.

**Gate 8.6A** — documental: ADR-0032 creado en estado `propuesto` e indexado, este contrato creado
desde el TEMPLATE y la seccion de follow-ups en `docs/ideas-futuras.md`. Sin pruebas y sin build por
contrato; `src/` y `tests/` intactos.
