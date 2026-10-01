# Plan del ejecutor nocturno

Este documento define el ejecutor de iniciativas y, en §16, la ejecución delegada bajo orden explícita del Coordinator. El ejecutor prepara trabajo revisable y
publicado; nunca integra cambios. `WORKFLOW.md` gobierna Git, integración, transición y clasificación;
`INITIATIVE_LIFECYCLE.md` gobierna Discovery, diseño, Freeze/A-n, READY y conformidad; `AGENTS.md`
gobierna composición Full, clases de evidencia y SHA exacto. Este plan no compite con esas fuentes,
con ADR aceptados, Freeze+A-n ni decisiones del Owner dentro de su alcance.

El estado operativo legible por el ejecutor se versiona en
`docs/automation/state/<initiative>.yml`. Las decisiones del dueno que resuelven gates pueden
versionarse en `docs/automation/decisions/<initiative>.md`. Un bloque `automation_state` en el Pull
Request es una copia opcional: nunca sustituye el archivo de estado de la rama.

## Estado actual de activacion

La infraestructura documental, los contratos, estados, decisiones y evidencias quedaron preparados
por I-06, pero **no existe actualmente una automatizacion nocturna activa**. I-06 no programa tareas,
horarios, recordatorios ni ejecuciones recurrentes.

Workflow V2 está materializado pero **no es efectivo** hasta que `WORKFLOW_V2_EFFECTIVE_SHA` exista
según `WORKFLOW.md` §11. Este documento no lo activa, no inicia la pausa de activación y no reclasifica
reclamos V1. Los scripts descritos por I-56 P-21 siguen siendo solo diseño; no existe implementación.

La variante Git-only fue un mecanismo acotado de bootstrap y cierre manual de I-06. Demostro que
commit, push y estado versionado no dependen de GitHub CLI, pero no sustituye una integracion capaz
de consultar de forma confiable Pull Requests, comentarios y checks. Esa automatizacion conectada
queda aplazada hasta que exista GitHub CLI o un mecanismo equivalente aprobado por el dueno.

Antes de activar cualquier ejecutor se requiere:

1. elegir y aprobar el mecanismo de integracion con GitHub;
2. ejecutar un nuevo piloto controlado sobre una iniciativa apta;
3. comprobar lectura de gates, checks, reintentos, estado y ausencia de trabajo concurrente;
4. aprobar expresamente el horario y la operacion recurrente.

Hasta entonces, el desarrollo continua manualmente bajo `docs/WORKFLOW.md`. Los contratos,
decisiones, estados y evidencias de `docs/automation/` se conservan como base para retomar el piloto.
Ninguna modalidad presente o futura autoriza merge automatico.

## 1. Proposito

El ejecutor puede leer el plan, elegir como maximo una iniciativa nueva elegible por ejecucion,
reclamarla atomicamente, trabajar en un worktree aislado, validar sus cambios, publicar commits y
abrir o actualizar un Pull Request draft. Debe dejar evidencia suficiente para que el dueno decida
si el trabajo continua, se valida localmente o se integra en una sesion separada.

## 2. Fuentes del plan y modos de ejecucion

### Modo bootstrap

Mientras `docs/AUTOMATION_PLAN.md` y los contratos de `docs/initiatives/` aun no esten integrados
en `origin/main`, la unica ejecucion piloto autorizada lee el sistema documental desde la rama y el
worktree activos de I-06, cuya fuente remota es:

```text
origin/docs/reestructura
```

Antes de continuar debe comprobar que el checkout es `docs/reestructura`, que `HEAD` coincide con
`origin/docs/reestructura`, que el estado versionado identifica el worktree y el Pull Request de I-06
y que el worktree no esta ocupado por otra sesion. Si la plataforma permite consultar el Pull
Request, tambien verifica que siga abierto; la falta de esa consulta no invalida el estado Git
versionado. Este modo solo puede reanudar I-06 en su rama, Pull Request y worktree existentes. No
ejecuta el algoritmo de seleccion, no reclama otra iniciativa y no crea otra rama, worktree o Pull
Request.

El modo bootstrap termina cuando el sistema documental queda integrado en `origin/main`. La
variante Git-only usada para cerrar manualmente I-06 no se activa para otras ramas ni permite que
una rama sustituya las reglas globales del ejecutor. Su existencia tampoco programa ejecuciones.

### Modo normal

Despues de la integracion documental, toda ejecucion nueva lee estos archivos desde la punta actual
de `origin/main`, nunca desde una copia local potencialmente atrasada:

- `AGENTS.md`;
- `docs/WORKFLOW.md`;
- `docs/ROADMAP.md`;
- `docs/AUTOMATION_PLAN.md`;
- `docs/initiatives/*.md`.

Una rama de iniciativa puede contener una version mas nueva de su propio contrato para precisar el
trabajo de esa iniciativa. Esa version no puede cambiar unilateralmente concurrencia, seleccion,
reclamo, reintentos, gates, seguridad ni ninguna otra regla global. Ante una contradiccion se aplican
las reglas de `origin/main` y se detiene el punto conflictivo para revision.

La ejecución delegada de §16 no es una ejecución de este modo: lee en `AuthorityRevision` los documentos de su propia unidad y los cambios normativos que su orden declara por sección,
y todas las demás autoridades en `MainSha`; una rama no puede rebajar una regla global.

### Autoridad y referencias obligatorias

El ejecutor consulta `WORKFLOW.md` §10 para el dueño único de cada dominio. En particular:

- clasificación V1/V2, Claim-Id, transición, snapshots y mecánica Git: `WORKFLOW.md`;
- ciclo de diseño, materialidad, Discovery, Freeze/A-n, READY y conformidad:
  `INITIATIVE_LIFECYCLE.md`;
- composición de pruebas, clases de evidencia e invalidadores: `AGENTS.md`;
- escenarios y ejecución de Owner Validation: `docs/guias/validacion-manual-autocad.md`;
- alcance congelado: Freeze+A-n; decisiones del Owner: su registro versionado.

Este plan decide la selección y operación del ejecutor y, en §16, la ejecución delegada, dentro de esas reglas. El estado real puede
desmentir una afirmación factual, pero no cambia por sí mismo lo que una autoridad exige. Una
contradicción material obliga a detenerse, citar las fuentes y acudir al dueño del dominio; el
ejecutor no elige una interpretación silenciosa. El modo bootstrap solo cambia desde dónde se leen
los archivos.

## 3. Limites de seguridad

- Nunca trabajar directamente sobre `main`, hacer merge, activar auto-merge, cerrar un Pull Request
  como integrado ni borrar una rama remota.
- Nunca usar `push --force`. `--force-with-lease` solo se permite al reanudar una rama ya reclamada
  despues del rebase exigido por `WORKFLOW.md`.
- No iniciar trabajo sin contrato detallado, dependencias satisfechas, conflictos libres y
  `automation.enabled: true`.
- No cambiar alcance para resolver hallazgos laterales. Se registran en el Pull Request y se detiene
  la parte afectada.
- No tomar decisiones reservadas al dueno, aceptar ADRs, inventar bloques DWG, modificar datos
  externos ni afirmar una validacion en AutoCAD que no realizo el dueno.
- Preservar cambios ajenos y detenerse ante un arbol sucio, una operacion Git en curso o una
  divergencia que no pueda explicarse con el historial remoto.
- Respetar la precedencia obligatoria, las dependencias del repositorio y el contrato de la
  iniciativa.
- El ejecutor no depende de GitHub CLI. Commit, push y estado versionado deben poder completarse con
  Git; actualizar metadatos del Pull Request es opcional cuando exista una integracion disponible.

## 4. Estado derivado

El estado operativo se recalcula al inicio y antes de publicar. El archivo
`docs/automation/state/<initiative>.yml` es la fuente transitoria canonica legible por el ejecutor,
pero se valida contra Git y resultados verificables; no se controla solo con checkboxes, el campo
`status` del front matter ni el cuerpo del Pull Request.

| Evidencia observada | Estado derivado |
|---|---|
| Los cambios de la iniciativa estan contenidos en `origin/main` y ROADMAP la marca integrada | terminada |
| Existe `origin/<rama>` y estado versionado no integrado | reclamada o en implementacion |
| Existe un Pull Request abierto conocido | en revision, CI o validacion |
| El estado versionado declara gate de AutoCAD, build local o decision | bloqueada para integrar |
| No hay rama, estado ni Pull Request conocido; dependencias integradas y sin conflictos | candidata elegible |
| Falta contrato detallado, una dependencia o una entrada humana previa | no elegible |

Una rama remota cuenta como iniciativa activa aunque el Pull Request no sea consultable. Una
iniciativa cuyo estado espera AutoCAD sigue activa, pero no bloquea por si misma iniciativas
compatibles. Las ramas ya integradas no cuentan como activas aunque aun no se hayan limpiado.

## 5. Reanudacion antes de seleccionar trabajo nuevo

En cada ejecucion el agente:

1. Ejecuta `git fetch origin --tags --prune` y deriva primero las iniciativas activas desde ramas
   remotas, archivos de `docs/automation/state/` y, cuando sea consultable, Pull Requests. Valida el
   estado versionado contra la rama, el Claim-Id y el ultimo commit publicado; una copia del PR nunca
   prevalece sobre esas evidencias.
2. Busca una iniciativa activa que pueda continuar sin intervencion humana. Cualquier `gate`
   distinto de `none` impide reanudarla hasta que exista evidencia de que fue resuelto. El estado
   `ci-failed` con `gate: none` si permite una correccion de CI; `gate: ci` significa que se espera
   un check o una accion externa y no autoriza otro intento.
3. Reutiliza la rama, el Pull Request y el worktree registrados de la iniciativa reanudable. Si el
   worktree no existe, esta ocupado o no coincide con la rama, se detiene: nunca crea un reemplazo
   silencioso para una iniciativa activa.
4. Ejecuta como maximo una fase coherente del contrato o una correccion de CI, actualiza evidencia y
   estado versionado, crea commit, hace push y termina. Si puede actualizar el mismo Pull Request,
   sincroniza alli una copia; si no puede, lo reporta sin bloquear una ejecucion ya publicada.
5. Solo cuando no existe trabajo activo reanudable cuenta la capacidad y evalua trabajo nuevo. El
   limite inicial sigue siendo dos iniciativas activas; una iniciativa detenida por gate cuenta como
   activa, pero puede dejar un lugar para otra compatible.
6. Excluye iniciativas integradas, condicionales no activadas, deshabilitadas, sin plan detallado,
   con dependencias pendientes, conflictos activos, prerrequisitos del dueno pendientes o intentos
   agotados.
7. Si hay capacidad, ordena las elegibles por `priority` numerica ascendente. Un empate se resuelve
   por el orden del ROADMAP y despues por ID. Una prioridad no asignada no se inventa: la iniciativa
   queda no elegible para seleccion automatica.
8. Selecciona y reclama como maximo una iniciativa nueva. Si ninguna cumple, emite el informe y
   termina.

El limite de dos iniciativas activas y el maximo de una nueva por ejecucion son compuertas
independientes. Reanudar trabajo ya reclamado no cuenta como iniciativa nueva, pero consume la unica
unidad de trabajo permitida en esa ejecucion. Antes de abrir un Pull Request se buscan PRs por rama,
Claim-Id e iniciativa mediante la integracion disponible o el estado versionado. Si ya existe uno,
se reutiliza; nunca se abre un segundo PR para la misma iniciativa. Si hay evidencia de que el PR
previo esta cerrado sin integrar, se detiene para decision del dueno.

## 6. Dependencias y conflictos

Una dependencia esta satisfecha solo cuando esta integrada en `origin/main`; una rama terminada o
un Pull Request aprobado no bastan. Un conflicto esta activo cuando la iniciativa conflictiva tiene
rama remota no integrada o Pull Request abierto. Tambien se respetan conflictos textuales del
ROADMAP, como trabajo simultaneo sobre un subsistema o toda una fase.

Antes de reclamar se vuelve a consultar el remoto. Si el estado cambio desde la seleccion, se
recalcula la elegibilidad en lugar de continuar con datos obsoletos.

## 7. Reclamo atomico y worktree

1. Confirmar arbol limpio, ausencia de operaciones Git y que `origin/<rama>` no existe.
2. Crear un worktree fuera del worktree principal desde la punta exacta de `origin/main`, con la
   rama canonica declarada en el contrato.
3. Crear un commit vacio cuyo mensaje incluya `Initiative`, `Branch`, un `Claim-Id` UUID unico y el
   trailer `Co-Authored-By`.
4. Ejecutar el primer `git push -u origin <rama>` sin force.
5. Si el push es rechazado porque la rama ya existe, no sobrescribirla: retirar solo el worktree y
   la rama local del reclamo no publicado, recalcular el plan y terminar sin reclamar otra
   iniciativa en esa ejecucion.

El primer push aceptado es el reclamo. La misma rama y el mismo worktree se reutilizan en sesiones
posteriores, siempre de forma secuencial.

### Reutilizacion del worktree

Antes de operar sobre una iniciativa activa se consulta `git worktree list --porcelain` y el
registro de la ejecucion anterior:

1. Si la rama activa ya esta registrada en un worktree accesible y limpio, se usa ese worktree.
2. No se intenta hacer checkout de la misma rama en otro worktree.
3. Si el worktree esta ocupado por otra sesion, la ejecucion se detiene.
4. Si el worktree registrado no existe fisicamente o quedo obsoleto, la primera version del
   ejecutor no lo elimina ni lo recrea automaticamente: reporta el problema al dueno.
5. Solo se crea un worktree nuevo como parte del reclamo atomico de una iniciativa nueva.

## 8. Implementacion, estado versionado y Pull Requests

El agente implementa solo las fases y archivos autorizados por el contrato. Al reanudar una rama,
primero compara con `origin/main` y hace el rebase requerido por `WORKFLOW.md`. Cada unidad coherente
lleva commit con asunto en espanol, explicacion del por que y trailer `Co-Authored-By`.

Tras el primer cambio sustantivo publica la rama y abre un Pull Request draft contra `main` mediante
la integracion disponible. Si ya existe, lo reutiliza en lugar de abrir otro. La incapacidad de
actualizar su descripcion no bloquea la ejecucion cuando commit, push y estado versionado terminaron.
El ejecutor no instala ni requiere GitHub CLI. Cuando el cuerpo sea actualizable, conserva:

- Initiative, Branch y Claim-Id;
- alcance realizado y pendiente;
- validaciones ejecutadas y sus resultados;
- CI, build local, AutoCAD y decisiones pendientes;
- intentos consumidos;
- hallazgos fuera de alcance;
- confirmacion de que no se hizo ni se hara merge automatico.

Cada iniciativa activa tiene exactamente un archivo canonico
`docs/automation/state/<initiative>.yml` con esta forma minima:

```yaml
schema: rackcad-automation-state/v1
automation_state:
  initiative:
  branch:
  claim_id:
  current_phase:
  state:
  gate:
  attempts:
  next_action:
  last_evidence_commit:
```

El ejecutor actualiza y publica ese archivo al terminar cada ejecucion, incluso si solo verifico un
gate. Un Pull Request puede contener exactamente un bloque `automation_state` con los mismos campos,
pero es una copia opcional y puede quedar atrasada. Los campos siguen estas reglas:

- `initiative`, `branch` y `claim_id` son inmutables y deben coincidir con el reclamo remoto.
- `current_phase` apunta a la siguiente fase pendiente o a la fase actualmente detenida.
- `state` usa uno de `claimed`, `implementing`, `validating`, `ci-failed`, `waiting`,
  `review-ready`, `integration-ready` o `completed`.
- `gate` usa `none`, `owner-decision`, `owner-validation`, `autocad`, `plugin-build`, `ci`,
  `dependency`, `conflict`, `permissions` o `scope`.
- `attempts` es un entero no negativo y se incrementa unicamente al intentar corregir un fallo; una
  verificacion, una fase normal o la espera de un gate no lo incrementan.
- `next_action` describe una sola accion siguiente, en una linea.
- `last_evidence_commit` es el SHA completo del commit publicado que respalda la ultima fase o
  correccion. Cuando el archivo de estado se publica en un commit posterior de cierre, apunta al
  commit de evidencia, no intenta autorreferenciar su propio SHA.
- `completed` significa que el trabajo de la iniciativa termino en su rama; no significa integrada.
  La integracion sigue siendo manual.

El archivo es estado transitorio canonico para reanudar, no una fuente superior a la realidad. Git y
los resultados verificables prevalecen cuando lo contradicen: `main`, ramas, commits y checks
disponibles se inspeccionan antes de actuar. AutoCAD debe identificar el commit/DLL validado y el
build local debe registrar commit y resultado. En la siguiente reanudacion el agente verifica esa
evidencia antes de cambiar un gate a `none`.

## 9. CI fallido y reintentos

Ante CI fallido se inspeccionan el check y sus logs antes de editar. Un intento es una secuencia de
diagnostico, correccion acotada, validacion local, commit y push. No se repite el mismo cambio sin
nueva evidencia. El maximo por defecto es `automation.max_attempts` del contrato, inicialmente tres.

Se detiene antes del limite si el fallo es externo, requiere secretos/permisos, depende de AutoCAD,
exige ampliar alcance o no puede reproducirse con la evidencia disponible. Al agotar intentos, el
estado versionado conserva el ultimo fallo, enlaces disponibles y una peticion concreta al dueno; el
PR se actualiza solo cuando la integracion lo permita.

## 10. Build local y AutoCAD

Si `requires_plugin_build: true`, el ejecutor intenta el build Debug local solo cuando la estacion
tiene las referencias necesarias y AutoCAD no bloquea los DLL. Si no es posible, publica el trabajo
con estado `esperando build Plugin local`; no declara exito ni integra.

Si `requires_autocad: true`, completa primero las validaciones automatizables y entrega al dueno la
ruta exacta del DLL del worktree y un checklist basado en el contrato. El estado versionado queda
esperando AutoCAD; el PR puede reflejarlo. Esa espera bloquea la integracion de la iniciativa, no el
inicio de otra compatible, siempre que el total activo permanezca por debajo de dos.

## 11. Decisiones del dueno

Cuando `requires_owner_decision: true`, el contrato debe identificar la decision y el punto en que
se necesita. El agente puede recopilar evidencia y plantear opciones, pero se detiene antes de tomar
la decision o implementar una opcion no autorizada. Los ADR nuevos permanecen `propuesto` hasta que
el dueno los acepte o rechace.

Una decision puede recibirse como `docs/automation/decisions/<initiative>.md`. Para resolver el gate,
el ejecutor verifica que el archivo exista en `origin/<rama>`, identifique las decisiones requeridas,
sea atribuible al dueno y autorice el siguiente alcance. Registra la ruta y el commit de evidencia en
el archivo de estado. Un comentario o revision del Pull Request sigue siendo valido si puede
consultarse, pero no es el unico canal admitido.

`requires_owner_validation: true` exige una confirmacion explicita del dueno antes de considerar la
iniciativa lista para integracion, incluso si CI esta verde.

### La metadata de validacion del dueno es MONOTONICA: solo puede anadir, nunca quitar

`requires_owner_validation` y `requires_autocad` son **declaraciones que ANADEN una obligacion**, y esa
es toda su semantica:

```
true   = este contrato anade o confirma explicitamente el gate de validacion del dueno
false  = este contrato no anade un gate adicional POR ESTA METADATA
         NO significa exencion, y no cancela ninguna obligacion que venga de otro sitio
```

Si `AGENTS.md`, `docs/WORKFLOW.md`, la guia de validacion manual o el alcance de la propia iniciativa
exigen la validacion del dueno por la **naturaleza del cambio** —el disparador vigente es «cambio el
comportamiento de dibujo», AGENTS.md punto 5 y WORKFLOW seccion 4.5.3—, entonces
`requires_owner_validation: false` **NO la cancela**.

**En caso de conflicto gana el requisito aplicable mas estricto.** Y un `false` heredado «por analogia»
con otra iniciativa no es un argumento: la obligacion se decide por lo que el cambio hace, no por lo
que declaro un contrato vecino.

Esto no introduce ningun nivel ni modelo de riesgo, y no debe leerse como tal: es la regla de
composicion de un campo que ya existia, escrita para que un dato autodeclarado no pueda retirar en
silencio una garantia que otro documento exige
([ADR-0033](adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) §10, en estado `propuesto`,
llega a la misma conclusion: esa validacion no se reduce por politica general).

## 12. Condiciones obligatorias para detenerse

El ejecutor se detiene y deja informe cuando ocurra cualquiera de estas condiciones:

- no hay iniciativa elegible o ya existen dos activas;
- la rama fue reclamada por otra ejecucion;
- el arbol no esta limpio, hay una operacion Git en curso o el worktree esperado esta activo en
  otra sesion;
- faltan decisiones, validaciones, bloques DWG, secretos, permisos, referencias de AutoCAD o build
  local obligatorio;
- una dependencia o conflicto cambio durante la ejecucion;
- el cambio necesario rebasa el alcance, toca archivos prohibidos o contradice un ADR aceptado;
- CI no se puede diagnosticar con seguridad o se agotaron los intentos;
- se requeriria merge, auto-merge, force-push no permitido o una accion destructiva no autorizada.

No es condicion de detencion la imposibilidad de leer o actualizar la descripcion del Pull Request
si la rama, el commit, el push y `docs/automation/state/<initiative>.yml` pueden completarse y el PR
existente ya esta identificado.

## 13. Prohibicion de merge automatico

El ejecutor nunca ejecuta `merge`, `gh pr merge`, auto-merge, squash, rebase-and-merge ni una API
equivalente. Tampoco actualiza ROADMAP o HANDOFF como si la iniciativa estuviera integrada. La
integracion es una sesion separada, serializada y autorizada por el dueno conforme a `WORKFLOW.md`.

## 14. Informe nocturno

Cada ejecucion entrega, aun cuando no cambie archivos:

1. SHA de `origin/main` observada y hora/zona de la consulta.
2. Iniciativas activas y su estado derivado.
3. Iniciativa seleccionada o motivo de no seleccion.
4. Claim-Id, rama, worktree y SHA del reclamo si hubo uno.
5. Commits y archivos cambiados.
6. Pull Request registrado y estado conocido; indicar si no pudo consultarse o actualizarse.
7. Pruebas, builds y checks de CI disponibles con resultado verificable.
8. Intentos consumidos y fallos pendientes.
9. Validacion AutoCAD, build local o decisiones requeridas del dueno.
10. Hallazgos fuera de alcance y siguiente accion recomendada.
11. Confirmacion de que `main` no fue modificada y no hubo merge automatico.
12. Ruta y contenido final del estado versionado.

## 15. Registro resumido de iniciativas

Este registro copia solo metadatos del ROADMAP vigente al crear el contrato. `Prioridad` queda sin
asignar cuando ROADMAP no proporciona un orden numerico; el ejecutor no debe deducirla. `Build`
indica si el alcance descrito exige validar el Plugin localmente. `Decision` identifica una compuerta
explicita del dueno o un ADR que solo el dueno puede aceptar.

| ID | Rama | Prioridad | Depende de | Conflictos | AutoCAD | Build Plugin local | Decision del dueno | Plan detallado | Estado ROADMAP |
|---|---|---:|---|---|---|---|---|---|---|
| I-03 | `refactor/fallos-silenciosos` | — | — | I-11 | no | si | no | pendiente | pendiente |
| I-05 | `feature/guardrail-unidades` | — | — | — | si | si | si: ADR de unidades | pendiente | pendiente |
| I-06 | `docs/reestructura` | 10 | — | I-07 | no | no | si | disponible | pendiente |
| I-07 | `docs/adr-retroactivos` | — | — | I-06 | no | no | si: aceptar/rechazar ADRs | pendiente | pendiente |
| I-13 | `experiment/refs-autocad-ci` | — | — | — | no | si | si: excepcion NuGet y adopcion | pendiente | pendiente |
| I-26 | `refactor/test-catalog-ids` | — | — | — | no | no | no | pendiente | pendiente |
| I-08 | `architecture/system-registry` | — | I-02 (integrada) | I-10, I-11 | no | no | no | pendiente | pendiente |
| I-09 | `refactor/plugin-commands` | — | I-02 (integrada) | I-10, I-16 | no | si | no | pendiente | pendiente |
| I-10 | `architecture/kind-handlers` | — | I-08, I-09 | I-09, I-16 | no | si | no | pendiente | pendiente |
| I-11 | `architecture/persistencia-uniforme` | — | I-02 (integrada) | I-03, I-08 | no | si | no | pendiente | pendiente |
| I-12 | `refactor/versionado` | — | — | — | no | si | si: ADR de versiones AutoCAD | pendiente | pendiente |
| I-14 | `architecture/ui-controls` | — | I-02 (integrada) | I-15, I-17 | no | no | no | pendiente | pendiente |
| I-15 | `architecture/editor-shell` | — | I-08, I-14 | I-14 | no | no | no | pendiente | pendiente |
| I-16 | `refactor/draw-services` | — | I-09 | I-09, I-10 | no | si | no | pendiente | pendiente |
| I-17 | `refactor/clon-unico-cabecera` | — | I-02 (integrada) | I-14; trabajo en selectivo/configurador | no | no | no | pendiente | pendiente |
| I-18 | `feature/push-back` | — | I-10, I-11, I-15, I-16; bloques DWG del dueno | — | si | si | si: bloques y filas de catalogo | pendiente | pendiente |
| I-19 | `feature/validador-catalogos` | — | — | — | no | no | no | pendiente | pendiente |
| I-20 | `refactor/selective-editor-state` | — | I-15 | I-22 | no | no | no | pendiente | pendiente |
| I-21 | `refactor/dynamic-editor-state` | — | I-15, I-02 (integrada) | I-28 condicional | no | no | no | pendiente | pendiente |
| I-22 | `refactor/safety-placement` | — | I-14, I-20 | I-20 | no | si | no | pendiente | pendiente |
| I-23 | `refactor/namespaces-sistemas` | — | I-08, I-15, I-16, I-20, I-21, I-22 | toda la Fase 5 | no | si | no | pendiente | pendiente |
| I-24 | `refactor/ui-tests-editores` | — | I-15, I-20 | — | no | no | no | pendiente | pendiente |
| I-25 | `feature/guardas-traseras` | — | I-22 | — | si | si | no | pendiente | pendiente |

No forman parte de la cola pendiente: I-00, I-01, I-02, I-04 e I-27 estan integradas. I-28 es una
contingencia condicional y solo entra a la cola si un ADR futuro reemplaza ADR-0002 por la opcion B.
Mientras las demas prioridades y planes detallados sigan pendientes, la primera iniciativa que el
algoritmo puede seleccionar es I-06.

## 16. Ejecución delegada bajo orden del Coordinator

La **ejecución delegada** es la ejecución de un trabajo de gate por participantes delegados (un Controller Codex de solo lectura y un Worker) bajo una orden explícita del Coordinator.
**No es el ejecutor nocturno:** no selecciona iniciativas, no reclama, no exige activación ni `automation.enabled: true` y no abre ni actualiza Pull Requests. Origen: Freeze de I-61
(`docs/initiatives/I-61-proposal-v9.md`). Vigencia: desde la integración de I-61 con ADR-0046 aceptado; antes, solo en el piloto de I-61, bajo la autoridad del Owner y sin rebajar
ninguna regla vigente. Ninguna otra iniciativa lo adopta por estar escrito. El procedimiento, las órdenes y las plantillas viven en `docs/automation/agent-execution/` (subordinados) y
la composición de prompts en `PROMPT_TEMPLATES.md` §G.

### 16.1 Participantes y declaraciones

| Participante | Puede declarar | Nunca declara |
|---|---|---|
| Worker (Claude o Codex) | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS, Candidato, cierre, integración |
| Controller (Codex) | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | GATE PASS, Candidato, cierre, integración |
| Coordinator | GATE PASS conforme a `INITIATIVE_LIFECYCLE.md` §7 | Candidato, cierre o integración fuera del workflow |
| Sesión responsable (la que tiene asignado el worktree) | hechos del relevo, del remoto y del rebase, y la disposición de un fallo de transporte | `EXECUTION_*` |

La verificación (`EXECUTION_*`) es exclusiva del Controller; la sesión responsable solo completa las comprobaciones del relevo y registra hechos. La sesión Claude puede actuar como
Coordinator, Architect y Executor (`SAME-SESSION ROLE`), pero no se presenta como Codex.

**Trabajo delegado terminado:** solo con `EXECUTION_VERIFIED` cuyo `VerifiedSha` es el `CurrentSha` de la entrega o, tras un rebase registrado (16.7), su imagen; y con el SHA evaluado
del gate igual a ese SHA más commits de la sesión limitados a `docs/automation/` y a los documentos de la unidad, sin rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE` (16.3). El
Coordinator lo comprueba con `git diff --name-only`; tocar esas rutas exige un contrato nuevo. Con REWORK, BLOCKED o STOP no hay trabajo delegado que revisar. El Coordinator puede
rechazar un VERIFIED. Esta regla define cuándo termina la delegación y no añade condiciones de cierre de gate.

### 16.2 Aplicación de este plan a la ejecución delegada

«Ejecutor» se lee así: la sesión responsable para el estado, el informe, el relevo, el build local, el paquete de OV, las decisiones del Owner y la custodia; el Worker para sus commits,
su trailer y su alcance; el Controller no escribe en Git.

| Sección | Aplica | Exclusión o lectura |
|---|---|---|
| Preámbulo (antes de «Estado actual de activacion») | sí | Subordinación a las demás fuentes y uso de los archivos de estado y de decisiones |
| «Estado actual de activacion», §1, §4, §5, §6, §14 y §15 | no | Selección, reclamo e informe nocturno |
| §2 «Fuentes del plan y modos de ejecucion» | solo «Autoridad y referencias obligatorias» y la frase de ejecución delegada de «Modo normal» | Los modos bootstrap y normal rigen al ejecutor nocturno |
| §3 «Limites de seguridad» | viñetas 1, 2, 3 (sin la exigencia de `automation.enabled: true`), 4 (hallazgos laterales a `docs/ideas-futuras.md` o a la evidencia de la unidad, no a un Pull Request), 5, 6, 7 y 8 | En la viñeta 2, la recuperación de 16.7 se lee como reanudar tras el rebase exigido por `WORKFLOW.md` §4, que admite `--force-with-lease` |
| §7 «Reclamo atomico y worktree» | no | Solo como precedente de las comprobaciones de 16.4 |
| §8 «Implementacion, estado versionado y Pull Requests» | primer párrafo y la forma y las reglas de campos del archivo de estado | No aplica abrir, reutilizar ni actualizar un Pull Request. «Publicar al terminar cada ejecución» se lee como los commits de estado de 16.4. `attempts` en tareas delegadas: 16.8 |
| §9 «CI fallido y reintentos» | definición de intento, máximo `automation.max_attempts` y detención antes del límite | No aplica la actualización del PR |
| §10 «Build local y AutoCAD» | completa, para la sesión responsable | El Worker nunca usa AutoCAD |
| §11 «Decisiones del dueno» | completa, para la sesión responsable, incluida la metadata monotónica | — |
| §12 «Condiciones obligatorias para detenerse» | viñetas 2-8, según 16.11 | No aplica la viñeta 1 |
| §13 «Prohibicion de merge automatico» | completa | — |

### 16.3 Lectura de autoridades

`AuthorityRevision` es el SHA del commit cuyo árbol contiene los documentos y los cambios normativos de la propia unidad que obligan a la delegación. Para I-61 es el SHA de cierre de su
gate G2, revisado por el Coordinator contra el Freeze; o un commit posterior X de la rama, elegido por el Coordinator, tal que `git diff --name-only <B> X` no contiene rutas `EXTERNAL`
ni rutas con secciones `UNIT_CHANGE`, donde `B` es el cierre de G2 o la `AuthorityRevision` vigente o, tras un rebase, su imagen en el último `RebaseMap`; o la imagen de cualquiera de
ellos registrada en un `RebaseMap`. Es ancestro de `BaseSha`. Al emitir o reemitir un contrato tras una A-n o decisión de la unidad, el Coordinator elige un X que la contenga.

Clases, declaradas en el contrato por ruta y sección:

- `UNIT_DOC`: documentos de la propia unidad (contrato `docs/initiatives/<I>-*.md`, Freeze y A-n, `decisions/<I>.md`, evidencia y mandato registrado). Se leen en `AuthorityRevision`.
- `UNIT_CHANGE`: cambios normativos de la unidad bajo su Freeze, declarados por sección (en I-61, los que enumera su Freeze §2.1). Se leen en `AuthorityRevision`.
- `EXTERNAL`: toda otra autoridad. Se lee en `MainSha`. Con `MB` = `git merge-base MainSha AuthorityRevision`: si su archivo no contiene secciones `UNIT_CHANGE`,
  `git diff --name-only MB AuthorityRevision -- <ruta>` es vacío; si las contiene, el texto de cada sección `EXTERNAL` es idéntico en `MB` y en `AuthorityRevision` (con espacios
  normalizados) y toda diferencia del archivo cae dentro de secciones `UNIT_CHANGE`. Además, `git diff --name-only AuthorityRevision BaseSha` no contiene rutas `EXTERNAL` ni rutas con
  secciones `UNIT_CHANGE`.

Una **sección** va desde su encabezado hasta el siguiente encabezado con el mismo número de `#` o menos (incluye sus subsecciones); el **preámbulo** es el texto anterior al primer
encabezado `##`. Si algo de esto no es verificable → STOP (S-12).

### 16.4 Transporte, relevo, cesión y orden de commits

**Transporte:** la jerarquía de transportes y los usos de Computer Use son los del mandato de I-61, por referencia y sin cambios; Computer Use nunca es el bus normativo. Esquemas, relevo
y verificación son idénticos en todo nivel de transporte.

Cada invocación de un participante externo (Codex) es un relevo de `WORKFLOW.md` §3. Un Worker subagente de la sesión es alternancia interna de una sola sesión responsable, no un relevo
entre sesiones, y se le aplican las mismas comprobaciones de salida, la cesión, la entrada y el registro. Se invoca con una llamada cuya finalización se notifica, con tope de 60 min: la
sesión no opera hasta recibir esa notificación con el resultado final, y al vencer el tope detiene la tarea y confirma su estado terminal antes de volver a operar. No se usan subagentes
que sobrevivan a su llamada.

1. **Salida:** `HEAD` = `origin/<rama>` comprobado con `git ls-remote`, y el último commit lleva en su cuerpo el resumen de estado (si ya se cumple, no hace falta commit nuevo); árbol
   limpio y ninguna operación Git en curso; `git fetch` y registro de `origin/main`; comprobación de procesos; una sola delegación abierta; SHA-256 de `~/.codex/config.toml` y la
   lista ordenada de sus nombres de secciones y claves, sin valores.
2. **Cesión:** durante la invocación, la sesión no lee, no escribe ni ejecuta nada sobre el worktree, tampoco con sus subagentes; registra inicio y fin en UTC. No se reinterpreta
   «sesión activa»: una sesión que completó el relevo y no opera ha cedido el worktree; la etiqueta «esperando» no basta.
3. **Entrada:** el participante y sus descendientes terminaron; las comprobaciones de `WORKFLOW.md` §3; y la comparación de `config.toml`, cuyo cambio es STOP (P-01): ninguna invocación
   de Codex más hasta que decida el Owner, y se registra el diff de nombres de secciones y claves.

**Procesos:** se evalúan los de línea de órdenes legible y, entre los ilegibles, los de la lista cerrada `codex*.exe`, `claude.exe`, `node.exe`, `git.exe`, `pwsh.exe`, `powershell.exe`,
`bash.exe` y `dotnet.exe`. Es participante sobre el worktree un proceso evaluado cuya línea de órdenes contiene la ruta del worktree (con `\` o `/`) o `-C <worktree>`. Exclusión
estricta: solo el proceso que comprueba y sus ancestros por `ParentProcessId`, nunca sus hermanos ni otros descendientes de la sesión; y, por nombre,
`codex-windows-sandbox-service.exe`. Un `codex*.exe` del Owner con línea de órdenes legible que no contiene la ruta del worktree no es participante; si la contiene, STOP al Owner. El
PID del participante lanzado y todo su árbol deben estar muertos en la entrada. Para un Worker subagente, ningún descendiente nuevo de la sesión ni ningún huérfano (proceso de la lista
creado en la ventana de la cesión cuyo padre no existe en la entrada, o existe con una `CreationDate` posterior a la del hijo por reutilización del PID) puede seguir vivo, salvo la cadena
del comprobador y los servidores de compilación (`VBCSCompiler.exe`; `MSBuild.exe` o `dotnet.exe` con `/nodemode`, `build-server` o `VBCSCompiler.dll`). `CreationDate` se compara
en UTC. Un participante ajeno o un proceso no atribuible es STOP (P-02). Tras un tope de tiempo se mata el árbol del proceso lanzado y se confirma su muerte antes de lanzar otro
escritor.

**Orden de commits en una tarea delegada:** (1) commit de estado de la sesión, si hace falta, y push; (2) Controller de planificación, sin commit; (3) nc4 y aceptación, sin commit;
(4) Worker: commit RED y push si la entrega va a exigir RED (16.8), y commit GREEN y push con el resumen de estado; (5) ninguna escritura Git de la sesión hasta terminar la
verificación, la reverificación de 16.7 si la hay y los controles negativos, salvo el rebase de 16.7; (6) verificación del Controller; (7) después, el commit de la sesión con
custodia, estado, `attempts` y evidencia. Tras un REWORK se vuelve a (1) con el commit de `attempts`; tras un STOP por avance de `main` se aplica 16.7 sin el commit (7) hasta terminar la
reverificación.

**Receta de Codex:** binario por ruta verificada; `-C` con el worktree de la unidad únicamente; `-s read-only`; `-m` y `-c model_reasoning_effort=…` de una celda elegible;
`--output-schema`; `-o` al directorio del `RunId`; `--json`; sin `--ephemeral`, para leer modelo y effort efectivos en el registro de sesión; stdin cerrado; el `pwsh` del runtime de
Codex delante en el `PATH` del proceso hijo; tope de 600 s; el `service_tier` de la configuración del Owner se hereda sin modificarlo.

### 16.5 Contrato de gate y aceptación del paquete

El Coordinator redacta `gate-contract.json` con, como mínimo: objetivo; alcance permitido y prohibido (incluido el no-touch); invariantes del Freeze por id; pruebas requeridas (filtro,
mínimo seleccionado y si se espera RED); condiciones STOP adicionales; autoridades por ruta, sección y clase, y `AuthorityRevision`; celdas elegibles con sus efforts probados;
`RoutingEnforcement`; y autorización de correcciones. Lo reemite tras todo rebase posterior al cierre de G2, antes de la siguiente delegación; en el caso sin conflictos de 16.7, el
`RebaseMap` solo sustituye a la reemisión para la reverificación de la misma entrega. La reemisión cierra las delegaciones abiertas del contrato anterior.

**Sintaxis de alcance:** ruta relativa a la raíz con `/` y la capitalización exacta, archivo exacto o prefijo de directorio terminado en `/`, sin comodines, `..` ni rutas absolutas.
R está cubierta por E si R = E, o si E termina en `/` y R empieza por E. A ⊆ B si cada entrada de A está cubierta por alguna de B (un prefijo de A solo lo cubre un prefijo de B que lo
contenga).

Antes de invocar al Worker, el Coordinator evalúa todas las comprobaciones, sin cortocircuito; la disposición es la más grave de las fallidas, y ningún fallo invoca al Worker:

| Id | Comprobación | Si falla |
|---|---|---|
| A1 | Válido contra `rackcad-delegation/v1` | BLOCKED (planificación) |
| A2 | `Unit`, `Gate` y `TaskId` del contrato; `RunId` de un registro de planificación `COMPLETED` y no usado por otra delegación aceptada | BLOCKED |
| A3 | `AllowedWriteScope` ⊆ alcance permitido del contrato | STOP (P-03) |
| A4 | `ForbiddenWriteScope` ⊇ prohibido del contrato | STOP (P-03) |
| A5 | `Invariants`, `StopConditions` (por id), `Authorities` y `RequiredTests` ⊇ contrato, con `MinSelected` ≥ y el mismo `ExpectRed` | STOP (P-03) |
| A6 | `AuthorityRevision` del contrato; `MainSha` = `origin/main` recién obtenido; `BaseSha` = `HEAD` = `origin/<rama>`; rama y worktree | `MainSha` distinto → S-13, caso «antes de escribir» de 16.7 (no consume el tope de BLOCKED); lo demás → BLOCKED |
| A7 | Celda y modelo elegibles del contrato, no `STALE`, y `Effort` entre los efforts probados de esa celda | BLOCKED |
| A8 | `Attempt`, `AttemptsRemaining`, `MaxReworkLoops`, `RoutingEnforcement`, `ChainBaseSha`, `ChainRedSha` y `ChainRedFiles` (el custodiado) según 16.8; `ExpectedHandoffPath` en el directorio del intento; `Owner.Kind` coherente con `Executor.Transport` | BLOCKED |

### 16.6 Identidad de las invocaciones y propiedad exclusiva

- `RunId` identifica una invocación de participante; lo asigna el relevo con la forma `R<yyyyMMddTHHmmssZ>-<4 hex>` y el participante lo repite. Las reejecuciones reciben uno nuevo y
  nunca sobrescriben.
- La delegación lleva como `RunId` el de su invocación de planificación (`DelegationRunId`); `Owner.Id` = `<Kind>:<DelegationRunId>`. La entrega lleva el `RunId` de su invocación de
  trabajo y `DelegationRunId`; `ExpectedHandoffPath` lleva el token literal `{WorkRunId}`, que el relevo sustituye al componer el prompt. La verificación lleva su `RunId`,
  `DelegationRunId` y `WorkRunId`. Un registro de rebase recibe un `RunId` propio y vive en el directorio del intento.
- **Propiedad exclusiva:** `Owner` identifica el único puesto de escritor; nunca hay dos. Si no se sabe si un participante sigue vivo, no se lanza otro.
- **Delegación abierta:** aceptada y sin verificación válida registrada ni cierre declarado por la sesión. Como máximo una por unidad.
- **Cadena en curso:** tarea con al menos una delegación aceptada que no terminó en VERIFIED ni en abandono declarado, incluida una corrección autorizada aún no emitida.
- G.7 («invoke worker») y G.11 («route corrections») los ejecuta la sesión exactamente según el paquete aceptado; cualquier divergencia es STOP (P-05). Las correcciones solo las
  autoriza el contrato. El análisis de un defecto nuevo y la causa raíz tras un STOP los redacta el Coordinator en `analysis.md`; la delegación de corrección lo cita en `CorrectionOf`.

### 16.7 Recuperación tras un avance de `main`

Todo rebase de la rama de la unidad posterior al cierre de G2, incluido el que `WORKFLOW.md` §4 exige al abrir una sesión, lo hace la sesión con `--force-with-lease` y lo registra con
`Phase: REBASE` y un `RebaseMap`: `MainSha` anterior y nuevo; el cierre de G2 y su imagen; `AuthorityRevision` (la del último contrato o, sin contrato, el cierre de G2) y su imagen,
con igualdad de los blobs `UNIT_DOC` y del texto de cada sección `UNIT_CHANGE` (con espacios normalizados, como en 16.3); `BaseSha` y `ChainBaseSha` con sus imágenes, o `null` fuera de
una tarea; cada commit del Worker con su imagen e igualdad de `git patch-id`; las rutas en conflicto, si las hay; y la decisión del Coordinator citada, si la hay. Si alguna de esas
igualdades falla, STOP al Coordinator, salvo en el rebase que ejecuta una decisión registrada con A-n, cuyo `RebaseMap` cita esa decisión en lugar de la igualdad de `patch-id`. El rebase
de apertura sin cadena en curso se registra en `artifacts/orchestration/<unit>/session-rebase/<RunId>/` con `TaskId` = `SESSION` y el `Attempt` vigente del estado; con cadena en curso
pertenece a esa tarea, no consume el máximo de recuperaciones y suma al tope de invocaciones lo mismo que una recuperación.

- **Antes de escribir:** rebase, reemisión del contrato con la imagen de `AuthorityRevision` y el `MainSha` nuevo, y delegación nueva del mismo `TaskId`.
- **Después de escribir, sin conflictos:** reverificación del Controller sobre la misma delegación y la misma entrega con el `RebaseMap` como entrada; las comprobaciones usan las imágenes
  y el `MainSha` nuevo, `Identity` exige la igualdad de `patch-id`, `Ci` usa la corrida `push` de `CurrentSha'`, y la parte RED usa la corrida del `RedSha` original (registrada en el
  relevo del trabajo y custodiada después); las diferencias de la parte RED y el cálculo de `ChainRedFiles` usan siempre los SHA originales. No hay Worker nuevo ni consumo de `attempts`.
- **Con conflictos:** `git rebase --abort`, registro con las rutas en conflicto e imágenes `null`, y STOP al Coordinator; la reemisión y el trabajo nuevo solo siguen a una decisión
  registrada sobre el estado de la rama, con A-n si exige reescribir commits publicados del Worker.
- Como máximo **dos recuperaciones** por `TaskId`; la tercera → STOP.

### 16.8 Conteo, RED de la cadena y topes

- `attempts` (estado canónico, por unidad), en una tarea delegada, sube en 1 cuando la sesión lanza una **corrección**: tras `EXECUTION_REWORK_REQUIRED`, o tras un STOP cuya resolución
  cambia el trabajo. La recuperación de 16.7 y las reejecuciones tras BLOCKED no lo incrementan. Se incrementa en un commit antes de emitir la delegación de corrección. Fuera de la
  ejecución delegada rigen §8 y §9 sin cambios.
- `Attempt` = `attempts` en el estado de `HEAD` al emitir la delegación; `AttemptsRemaining` = `max_attempts` − `attempts`; `MaxReworkLoops` = 3.
- Cadena = `TaskId`. Contador por clase = correcciones lanzadas en la cadena para una misma `FailureClass` (`NONE`, el id de una comprobación de 16.9 o `StopCondition`; misma clase =
  mismo valor). Si la `FailureClass` difiere de la del REWORK anterior, el Coordinator registra `analysis.md` antes de la corrección, y la corrección consume `attempts`.
- **STOP** si, ante un nuevo REWORK de clase X, el contador de X ≥ 3, o si `attempts` ≥ `max_attempts` (S-11).
- **Sin reinicios:** cambiar de modelo, rol o sesión, o renombrar el error, no reinicia `attempts`, los contadores por clase ni los topes.
- **BLOCKED y fallos de transporte:** como máximo dos reejecuciones por (`TaskId`, fase), y por control negativo; la tercera → STOP (P-04). No se reescribe un prompt sin `analysis.md`
  versionado.
- **Invocaciones:** el plan de gates de la unidad fija el tope (en I-61, su Freeze); una invocación que lo excedería no se lanza: STOP (P-07).
- **RED de la cadena:** `ChainBaseSha` = `BaseSha` de la primera delegación de la tarea (o su imagen). `RT` = `git diff --name-only <ChainBaseSha> <RedSha> -- tests/`. Un RED se
  acredita con `RedPart` = `pass` en `Ci` y en `Tests`; entonces `ChainRedFiles` = `ChainRedFiles` anterior ∪ `RT`, que nunca decrece y queda registrado en el `Evidence` de `Tests` de
  esa verificación y custodiado. El **RED vigente** (`ChainRedSha` de la delegación siguiente) es el último acreditado, salvo que después una entrega que exigía RED haya terminado con
  `RedPart` = `fail`; entonces es `null` hasta otro. `ChainRedFiles` solo está vacío si la cadena nunca acreditó un RED. Una entrega **exige RED** si `ChainRedSha` es `null` o si
  `git diff --name-only BaseSha..CurrentSha` toca `ChainRedFiles`. El commit RED es el esqueleto (primera delegación) o la corrección desactivada, e incluye todo cambio de las pruebas de
  la cadena; el GREEN no toca `RT` ∪ `ChainRedFiles`. `ExpectRed` es siempre el del contrato.

### 16.9 Verificación: comprobaciones obligatorias y modos de fallo

Orden fijo; `FailureClass` es la primera en `fail` o `not_run`. `EXECUTION_VERIFIED` solo si las catorce están en `pass`. Un `not_run` cuenta como `fail` con la disposición de su
fila, salvo el causado porque una comprobación anterior en `fail` dejó sin entrada a la comprobación, que no aporta disposición (con la entrega ausente, la disposición es BLOCKED). Con
varias en `fail`: STOP > BLOCKED > REWORK. Pares válidos de `Classification` y `Disposition`: `VERIFIED/NONE`, `REWORK_REQUIRED/REWORK`, `BLOCKED/BLOCKED` y `BLOCKED/STOP`;
STOP = `EXECUTION_BLOCKED` + `Disposition: STOP`, que el Controller recomienda y la sesión aplica y devuelve al Coordinator. La coherencia entre `Classification`, `Disposition` y
`Checks` la valida el relevo, además del esquema.

| # | Id | Comprueba | Actor de la señal | Si falla |
|---|---|---|---|---|
| 1 | `Termination` | Registro del trabajo `COMPLETED`; si fue un proceso, muerte confirmada | sesión → Controller | BLOCKED |
| 2 | `Handoff` | Entrega presente y válida, con `TaskId`, `Attempt` y `DelegationRunId` de la delegación, `RunId` asignado a esa invocación y fecha posterior al inicio | Controller | ausente → BLOCKED; inválida u otra corrida → REWORK si `Identity` pasa, si no STOP |
| 3 | `Authority` | Lectura de autoridades de 16.3 | Controller | STOP |
| 4 | `Contract` | A3-A5 entre delegación y contrato | Controller | STOP |
| 5 | `Identity` | Rama y worktree; `BaseSha` ancestro; `RedSha` (si no es `null`) entre `BaseSha` y `CurrentSha`; `HEAD` = `refs/remotes/origin/<rama>` = `CurrentSha`; tras un rebase, sobre los originales del `RebaseMap` y sus imágenes, con `patch-id` igual | Controller (Git local) | STOP |
| 6 | `Remote` | En el registro: `ls-remote` = `CurrentSha` y `origin/main` = `MainSha` | sesión → Controller | avance de `main` → STOP (S-13); hechos ausentes → BLOCKED |
| 7 | `Scope` | `git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope` y sin intersección con `ForbiddenWriteScope` | Controller | STOP |
| 8 | `CleanTree` | Árbol limpio y ruta del paquete ignorada (`git check-ignore`) | Controller | REWORK |
| 9 | `Ci` | Corrida `push` de `CurrentSha` con `ref` exacta y los cuatro jobs requeridos de `AGENTS.md` en `success`. `RedPart`: si la entrega exige RED, `pass` con `RedSha` no `null` y su corrida terminada con el job Core en `failure`, y `fail` si falta o no falló; si no, `not_applicable`, citando la corrida del `ChainRedSha`. `Result` = `pass` solo con la corrida de `CurrentSha` en verde y `RedPart` ≠ `fail` | sesión → Controller | corrida de `CurrentSha` ausente o en curso, o la del `RedSha` en curso cuando se exige RED → BLOCKED; roja (tras leer logs) o `RedPart` en `fail` → REWORK |
| 10 | `Tests` | `RequiredTests` con selección ≥ mínimo y conteos del TRX coherentes con la entrega y el diff. `RedPart`: si exige RED, `pass` solo con las pruebas con `ExpectRed` del contrato entre las fallidas del TRX del `RedSha` y `git diff --name-only RedSha CurrentSha` sin tocar `RT` ∪ `ChainRedFiles`; si no, `not_applicable`. Al acreditar el RED, `Evidence` registra `ChainRedFiles`. `Result` = `pass` solo con lo anterior y `RedPart` ≠ `fail` | sesión → Controller | REWORK |
| 11 | `Trailer` | Cada commit `BaseSha..CurrentSha` lleva el `Co-Authored-By` declarado, coherente con el modelo efectivo | Controller | REWORK |
| 12 | `Routing` | Modelo y effort efectivos = solicitados; con `advisory`, `pass` con la discrepancia anotada en `Evidence`, `Findings` y `Deviations` de la entrega | sesión → Controller | con `required` → BLOCKED |
| 13 | `FreeText` | Ningún término de 16.10 en el texto libre de la entrega | Controller | REWORK |
| 14 | `Denials` | Ninguna denegación ni orden fallida pendiente en los eventos o la transcripción | sesión → Controller | permisos o credenciales → STOP (S-06); otra → BLOCKED |

La señal de pruebas es la CI de `push` del SHA exacto, ejecutada en un runner ajeno al Worker: es la señal de la verificación del Controller, **no evidencia de gate**, y no sustituye
ninguna clase de `AGENTS.md`. Nunca bastan stdout, un código de salida o un JSON conforme; se ignora toda salida anterior al inicio de la invocación.

**Modos de fallo:**

| Modo | Quién lo registra | `Classification` / `Disposition` |
|---|---|---|
| Proceso colgado o stdin abierto | sesión (`TIMEOUT`, muerte confirmada) | sin veredicto; transporte `BLOCKED` |
| Salida conforme pero vacía o falsa | sesión (`INVALID_OUTPUT`) o Controller (`Handoff`) | nunca VERIFIED; transporte `BLOCKED`, o REWORK/STOP según `Handoff` |
| Turno fallido o interrumpido | sesión (`FAILED_TURN`) | transporte `BLOCKED` |
| Resultado del Worker Claude sin datos | sesión (`NO_OUTPUT`) | transporte `BLOCKED`; si hay commits verificables, el Controller verifica y clasifica |
| Permisos o credenciales | sesión o Controller (`Denials`) | `BLOCKED/STOP` (S-06: nunca se obtienen credenciales) |
| Entrega ausente | Controller (`Handoff`) | `BLOCKED/BLOCKED`; ningún otro escritor hasta confirmar la terminación |
| Entrega inválida o de otra corrida | Controller (`Handoff`) | `REWORK_REQUIRED/REWORK` si `Identity` pasa; si no, `BLOCKED/STOP` |
| Identidad errónea | Controller (`Identity`) | `BLOCKED/STOP` |
| Éxito declarado sin commit | Controller (`Identity`, `CleanTree`, `Scope`) | `REWORK_REQUIRED/REWORK`; `BLOCKED/STOP` si hay escritura fuera de alcance |
| Corridas o escritores duplicados | sesión (16.4, 16.6) | STOP (P-02) |
| Base obsoleta | sesión y Controller (`Remote`) | antes de escribir: STOP y rebase con delegación nueva; después: `BLOCKED/STOP` (S-13) y 16.7 |
| Owner Validation o AutoCAD | delegación | frontera, no fallo (16.11) |
| Modelo o effort distinto | sesión → Controller (`Routing`) | con `required`: `BLOCKED/BLOCKED`; con `advisory`: desviación |
| Cambio de `config.toml` | sesión | STOP (P-01) |

### 16.10 Términos de gate

Coincidencia por palabra o frase completa, sin distinguir mayúsculas, en `WorkCompleted`, `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y
`RecommendedNextAction`: `GATE PASS`, `GATE_PASS`, `Candidato`, `Candidate`, `FINAL_CANDIDATE_SHA`, `gate cerrado`, `gate closed`, `cierre del gate`, `rama integrada`,
`integrada en main`, `integrated into main`, `merged into main`, `lista para integrar`, `ready to merge`, `Owner Validation APROBADA` y `Owner Validation APPROVED`. No se aplica al
nombre de propiedad `Gate` ni a su valor. El Controller los comprueba en la entrega; el Coordinator, en el texto libre de la verificación, como control manual registrado.

### 16.11 Recuperación y condiciones STOP

**Recuperación:** REWORK → corrección según 16.8, si el contrato la autoriza, volviendo al paso (1) de 16.4; BLOCKED → el Coordinator resuelve la precondición y se reejecuta dentro del
tope de 16.8; STOP → causa raíz en `analysis.md` y decisión del Coordinator, o del Owner si la materia es OWNER-RESERVED. No hay continuación especulativa.

«Frontera»: el participante no la cruza, entrega lo automatizable, no es fallo, no consume `attempts` y el bloqueo vuelve al Coordinator en `KnownLimitations` y `Findings`; la
verificación puede ser VERIFIED sobre lo automatizable con «OV pendiente». Ningún STOP consume `attempts` por sí mismo.

| Id | Condición | Comportamiento |
|---|---|---|
| S-01 | Owner Validation required | frontera |
| S-02 | architecture outside Freeze | STOP |
| S-03 | material ambiguity | STOP |
| S-04 | contradictory evidence | STOP |
| S-05 | destructive/irreversible action not authorized | STOP |
| S-06 | missing credentials/permissions | STOP |
| S-07 | conflict with another active owner | STOP |
| S-08 | branch/worktree ownership conflict | STOP |
| S-09 | dependency not integrated | STOP |
| S-10 | required AutoCAD/user interaction | frontera |
| S-11 | repeated rework limit reached | STOP |
| S-12 | Worker loses context or cannot verify authority revision | STOP |
| S-13 | main movement invalidates exact-SHA assumptions | STOP; 16.7 |
| S-14 | initiative contract explicitly says STOP | STOP |
| P-01 | Cambio del hash de `config.toml` | STOP |
| P-02 | Participante vivo ajeno, proceso no atribuible o más de una delegación abierta | STOP |
| P-03 | Delegación fuera del contrato | STOP |
| P-04 | Tope de BLOCKED alcanzado | STOP |
| P-05 | Invocación distinta del paquete aceptado | STOP |
| P-06 | Aviso de límite de uso o de créditos en los eventos o la transcripción de una invocación | STOP |
| P-07 | Una invocación excedería el tope de invocaciones | STOP (no se lanza) |
| P-08 | La sesión operó durante una cesión: la salida de esa invocación es inválida | STOP |

§12 de este plan: la viñeta 1 no aplica; la 2 equivale a S-08; la 3, STOP; en la 4, faltan decisiones, bloques DWG, secretos o permisos → STOP (S-03, S-06), y faltan validaciones,
referencias de AutoCAD o build local → frontera, que la sesión gestiona según §10; la 5 equivale a S-07/S-09; la 6, a S-02 o alcance; la 7, STOP; la 8, S-05.

### 16.12 Custodia

Antes de que el Coordinator registre una decisión, la sesión copia los JSON y MD que la respaldan a `docs/automation/evidence/<unit>-pilot/<task>/<RunId>/`. La identidad duradera es el
blob de Git del commit de custodia; el SHA-256 del archivo transitorio es procedencia, y si difiere del de `git cat-file -p <blob>` por fin de línea se declara. Eventos, registros de
sesión y transcripciones no se versionan: quedan su SHA-256 y los campos extraídos, como procedencia no reverificable tras la limpieza. Una decisión solo cita artefactos versionados.
Esta custodia aplica la evidencia por unidad de `WORKFLOW.md` §11.4 y no añade condiciones de cierre de gate.
