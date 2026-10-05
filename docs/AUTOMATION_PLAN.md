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

**Unidades I62 (inactivo hasta `I62_EFFECTIVE_SHA`, 16.14).** El estado de una unidad I62_DELEGATED no usa el formato de esta sección: es
`rackcad-automation-state/v2` ([esquema](automation/agent-execution/schemas/automation-state.v2.schema.json)), se escribe solo en los puntos durables
de 16.25 y conserva los nueve campos de `automation_state` con el mismo significado. Las unidades I61 y DIRECT_ONLY siguen con este formato (16.28).

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

Antes de aplicar esta sección, toda unidad aplica §16.13. La **ejecución delegada** es la ejecución de un trabajo de gate por participantes delegados (un Controller Codex de solo lectura y un Worker) bajo una orden explícita del Coordinator.
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

**Unidades I62** (materializado por I-62 e inactivo hasta su vigencia, 16.14). Para una unidad I62, la tabla anterior se sustituye por cinco roles semánticos
sin proveedor; las unidades I61 siguen con ella. Ningún rol nombra proveedor, modelo ni runtime: un binding los vincula según capacidad acreditada (Proposal
V14 de I-62, §5), y los proveedores viven en los descriptores de adapter y en el catálogo.

| Rol | Semántica | Declara | Nunca declara |
|---|---|---|---|
| **PRINCIPAL_COORDINATOR** | sesión responsable de la unidad: worktree, preflight, relevos, propuesta de bindings, trabajo directo autorizado, hechos y custodia; en una unidad I62_DELEGATED, además, **orquesta** las invocaciones de rol elegibles (Proposal V14 §20.2) sin ganar autoridad de gate | hechos del relevo, del remoto, del rebase y del preflight; `CONFIGURATION_STATUS` propio | `EXECUTION_*`; GATE PASS salvo SAME-SESSION ROLE de Coordinator declarado |
| **ARCHITECT** | revisión de diseño y conformidad (LIFECYCLE §5, §9) | veredicto de LIFECYCLE, en `architect-review-result/v1` (Proposal V14 §20.7) | GATE PASS, `EXECUTION_*` |
| **EXECUTION_CONTROLLER** | planificación y verificación, solo lectura (§16) | `EXECUTION_VERIFIED/REWORK_REQUIRED/BLOCKED` | GATE PASS, Candidato, cierre, integración |
| **WORKER** | escritura dentro del alcance | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS |
| **REVIEWER** | revisión operativa fuera de LIFECYCLE, sin autoridad de Architect ni de Controller | hallazgos y recomendaciones, en `reviewer-result/v1` (Proposal V14 §20.7) | `EXECUTION_*`, GATE PASS, veredictos de LIFECYCLE, ARCHITECT_SATISFIED, cierre o rebaja de hallazgos del ARCHITECT |

El Coordinator de gates no es vinculable. La acumulación de roles en un mismo actor compara la identidad observable del actor, nunca el binding, y se aplica al
vincular (Proposal V14 §2 y §11.3): un rebinding o una etiqueta nueva no crea un actor distinto.

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
encabezado `##`. Si algo de esto no es verificable → STOP (S-12). Antes de aplicar esta sección, toda unidad aplica §16.13.

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
`bash.exe` y `dotnet.exe`. Es participante sobre el worktree un proceso evaluado cuya línea de órdenes contiene la ruta del worktree (sin distinguir mayúsculas, con `\` o `/`) o `-C <worktree>`. Exclusión
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

### 16.13 Compatibilidad de protocolos de ejecución delegada

Origen: Freeze de I-62 ([Proposal V14](initiatives/I-62-proposal-v14.md) Anexo E, E.1-E.5 y E.7). **Rige desde `I62_EFFECTIVE_SHA` (16.14, «Punto
efectivo») para toda unidad**, sea cual sea su protocolo: es la excepción declarada de 16.14 y la subsección que nombra el punto de entrada de
[WORKFLOW](WORKFLOW.md) §12. Antes de `I62_EFFECTIVE_SHA` no rige ninguna evaluación: la lectura de autoridades es la de 16.3, sin cambios
(PRE_ACTIVATION). No edita unidades I61, no muta los esquemas `/v1` y no congela archivos enteros. Ningún contrato I61 necesita cambiar.

**Componentes:**

| Componente | Ubicación | Se lee en | Identidad |
|---|---|---|---|
| Punto de entrada | `WORKFLOW.md`, `## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)` | `MainSha_eval`, por toda unidad, como gobierno de proceso | encabezado único; nombra esta subsección por su línea de encabezado exacta; con `I62_EFFECTIVE_SHA` presente y la sección ausente, repetida o ambigua → ENTRY_INVALID |
| Resolver | esta subsección | `MainSha_eval`, por toda unidad | su texto; con `I62_EFFECTIVE_SHA` presente y esta subsección ausente o repetida → ENTRY_INVALID |
| Punteros | primera frase de §16 y última frase de 16.3 | `MainSha` | vía redundante para los contratos que leen §16 en `MainSha`; la cadena no depende de ellos |
| Mapa de cláusulas | `docs/automation/agent-execution/compatibility/I62-clause-map.json` | **`I62_EFFECTIVE_SHA`**, nunca `MainSha` | `git rev-parse <I62_EFFECTIVE_SHA>:<ruta>`; el tag `integration/I-62` repite el blob. Esta subsección no cita el blob: el mapa contiene el de AUTOMATION_PLAN y la cita crearía un ciclo |
| Esquema del mapa | `docs/automation/agent-execution/compatibility/clause-map.schema.json` (`rackcad-clause-map/v1`) | `I62_EFFECTIVE_SHA` | ruta fija; el mapa lo lista como ENTRY con su blob |
| Tabla PRE | cuerpo del commit `I62_EFFECTIVE_SHA` (precedente: `WORKFLOW.md` §11.3) | `git show -s --format=%B <I62_EFFECTIVE_SHA>` | el propio commit |
| Tabla POST | informe posterior al merge y tag | solo para casos DESCONOCIDA | tag |

16.3 conserva literalmente el texto de I-61 más la frase puntero. Las reglas de autoridad propias de I62 viven en las subsecciones I62, no en 16.3.

**Superficies** (lista cerrada): `AGENTS.md`, `CLAUDE.md`, `docs/AUTOMATION_PLAN.md`, `docs/FOUNDATIONS.md`, `docs/INITIATIVE_LIFECYCLE.md`,
`docs/WORKFLOW.md`, `docs/adr/`, `docs/automation/agent-execution/`, `docs/initiatives/PROMPT_TEMPLATES.md`.

**Clasificación** (`Classify`):

```text
Classify(unidad u, MainSha_eval):
 1. EFF := Derive(MainSha_eval)                     -- primer merge first-parent con el trailer único (16.14, «Punto efectivo»)
    ninguno            → PRE_ACTIVATION (esa main no contiene I-62: lectura de I-61 sin resolver)
    duplicado/ambiguo  → ACTIVATION_INVALID → STOP al Owner
 2. cid := claim_id del estado de u en BaseSha (o en la punta de su rama fuera de una tarea)
 3. PRE := tabla del cuerpo de EFF; mal formada o con Claim-Id duplicado → ACTIVATION_INVALID
 4. cid aparece exactamente una vez en PRE                                   → I61 (ANTERIOR_DEMOSTRADA)
 5. si no, estado de u en /v2 con protocol.set = I62, protocol.effective_sha = EFF y basis.claim_id = cid:
      g0_acceptance.state = PENDING                                          → PENDING_G0
      g0_acceptance.state = ACCEPTED y decision válida (blob presente; marcadores
        `I62-CLASSIFICATION: I62` e `I62-DELEGATED-EXECUTION: I62_DELEGATED`, y cid) → I62 (POSTERIOR_DEMOSTRADA; base obsoleta
                                                                                si basis.claim_parent_contains_effective = false)
      g0_acceptance.state = REJECTED, o decision inválida                    → paso 6
 5b. si no, estado de u en /v1 y cid fuera de PRE:
      decisión de G0 con `I62-DELEGATED-EXECUTION: DIRECT_ONLY`               → DIRECT_ONLY (sin protocolo delegado)
      sin ese marcador                                                       → UNKNOWN (solo para la ejecución delegada)
 6. si no: decisión del Coordinator para cid, con marcador `I62-CLASSIFICATION: I61|I62`
    y su evidencia (WORKFLOW §11.3), emitida tras un STOP                     → ese valor
 7. si no                                                                    → UNKNOWN
```

| `g0_acceptance.state` | Resultado | Contratos y delegaciones | Puntos durables admitidos |
|---|---|---|---|
| PENDING (desde BOOTSTRAP hasta el QU de aceptación) | PENDING_G0 | STOP (P-15) | QU, QH, QR (Q0 prohibido) |
| ACCEPTED | I62 | permitidos; con base obsoleta, STOP de delegaciones mientras la rama no contenga `effective_sha` | todos |
| REJECTED | UNKNOWN, salvo una decisión posterior (paso 6) | STOP | QU, QH, QR |
| (estado `/v1`, DIRECT_ONLY) | DIRECT_ONLY | STOP (P-15) hasta una adopción; el trabajo directo no se ve afectado | — (sin puntos `/v2`) |

- **Quién:** el Coordinator, en el G0 de cada unidad nueva (acepta o rechaza la evidencia del BOOTSTRAP) y antes de emitir cualquier contrato posterior a
  EFF. El Coordinator al aceptar y el Controller en `Authority` la vuelven a aplicar sobre las mismas fuentes; una discrepancia es S-04.
- **Durabilidad:** nunca se rederiva por ascendencia actual (`WORKFLOW.md` §11.1). Las fuentes son la tabla PRE, el `protocol` del estado `/v2` (evidencia
  del BOOTSTRAP más la aceptación registrada) o una decisión registrada.
- **Unidades I61:** se clasifican sin escribir en su rama.

**Evaluación** (`Evaluate`; el punto de entrada la impone a toda evaluación de un contrato de ejecución delegada: la emisión, la aceptación A1-A8, la
comprobación `Authority` de la verificación y la decisión del Coordinator sobre un `EXECUTION_VERIFIED`; `Resolve` nunca se invoca directamente):

```text
Evaluate(K, MainSha_eval):
 E1. W := docs/WORKFLOW.md en MainSha_eval                     -- gobierno de proceso
 E2. EFF := Derive(MainSha_eval); X := Match(MainSha_eval, WORKFLOW, <línea de encabezado del punto de entrada>)
     (misma semántica que Match: línea normalizada, nivel ##, fuera de bloques de código)
       EFF ausente y X = ∅           → PRE_ACTIVATION: 16.3 de la revisión que gobierne K, sin cambios (fin)
       EFF ausente y X ≠ ∅           → ACTIVATION_INVALID → STOP al Owner
       EFF duplicado o ambiguo        → ACTIVATION_INVALID → STOP al Owner
       EFF presente y |X| ≠ 1         → ENTRY_INVALID → STOP (S-12; P-15)
 E3. R := Match(MainSha_eval, AUTOMATION_PLAN, <línea de 16.13 que nombra X>), con la misma semántica
       |R| ≠ 1                        → ENTRY_INVALID → STOP (S-12; P-15)
 E4. Resolve(K, MainSha_eval), pasos 1-5, con el algoritmo de R
 E5. Registro de descubrimiento: blobs de WORKFLOW y de AUTOMATION_PLAN en MainSha_eval, EFF, encabezados hallados; se une al registro de Resolve
```

| Estado en `MainSha_eval` | Resultado |
|---|---|
| sin EFF y sin punto de entrada | PRE_ACTIVATION: I-61 sin cambios |
| punto de entrada sin EFF derivable, o trailer duplicado | ACTIVATION_INVALID → STOP al Owner |
| EFF presente y punto de entrada ausente, repetido o con encabezado ambiguo | ENTRY_INVALID → STOP (S-12; P-15) |
| el punto de entrada nombra una 16.13 ausente o repetida | ENTRY_INVALID → STOP |
| mapa inválido | MAP_INVALID → STOP |
| verificación de una unidad I61 tras EFF sin registro de descubrimiento y de resolución en `Authority.Evidence`, o con una resolución distinta de la del Coordinator | el Coordinator no la acepta (16.1); reverificación (BLOCKED, fase VERIFICATION) |

- **Coordinator:** ejecuta `Evaluate` al aceptar (A1-A8) y antes de aceptar un VERIFIED.
- **Sesión responsable:** registra el descubrimiento en `Notes` del relevo. En cada invocación de verificación de una unidad I61 tras EFF, añade a las
  entradas canónicas del prompt la **entrada de compatibilidad**: la instrucción normativa de ejecutar `Evaluate` antes de 16.3 y las líneas de encabezado
  del punto de entrada y de esta subsección. **No** le pasa el resultado esperado ni su propio registro, y no la añade al contrato ni a la delegación.
- **Controller:** ejecuta `Evaluate` de forma independiente (es de solo lectura) y registra en `Authority.Evidence` el descubrimiento y la resolución.
- **Comparación posterior:** terminada la verificación, el Coordinator compara el registro del Controller con el suyo; una diferencia hace que el VERIFIED no
  se acepte (16.1).

**Resolución** (`Resolve`). Entradas: el contrato K (`gate-contract/v1` o `/v2`), tal como se emitió; `AR` = `K.AuthorityRevision`; `MainSha_eval` =
`K.MainSha`, salvo en una reverificación de 16.7 sin conflictos, donde es el `MainSha` nuevo del `RebaseMap` y el contrato no cambia. El contrato no necesita
citar esta subsección ni el mapa, ni campos nuevos, ni reemitirse.

```text
Resolve(K, MainSha_eval):                       -- solo desde Evaluate, E4
 0. Precondición: Evaluate pasó E1-E3; EFF y 16.13 ya están identificados.
 1. P := Classify(K.Unit, MainSha_eval). UNKNOWN o PENDING_G0 → STOP. P = I62 con K /v1, o P = I61 con K /v2 → P-15.
 2. M := mapa en EFF (ruta fija); válido contra el esquema en EFF; Validate(M).
    Cualquier fallo → MAP_INVALID → `Authority` fail; STOP (S-12; P-15).
 3. Para cada cita a = (Path, Section, Class) de K.Authorities, en el orden del contrato:
      Class ∈ {UNIT_DOC, UNIT_CHANGE}            → AR                               (sin cambio)
      EXTERNAL y P = I62                         → MainSha_eval                     (normal)
      EXTERNAL y P = I61                         → R61(Path, Section)
 4. Comprobaciones de 16.3 con el texto de 16.3 resuelto (para I61, el de EFF^1), donde
    «se lee en MainSha» se lee como «se lee en la revisión resuelta en el paso 3».
    MB = merge-base(MainSha_eval, AR) y las demás comprobaciones de 16.3, sin cambio.
 5. Registro: lista ordenada (Path | Section | Class | NORMAL|COMPAT|COMPUESTA|ENTRY | revisión, o revisión por unidad si es compuesta)
    + EFF + blob del mapa + clase. /v1: `Authority.Evidence` de la verificación. /v2: `AuthorityResolution[]`.

R61(Path, Section):
 a. Path = ruta del mapa, o archivo con FileKind ENTRY                  → EFF
 b. Path con FileKind ADDED (no existía en EFF^1)                       → NOT_APPLICABLE → fallo (P-15)
 c. Path con FileKind MODIFIED:
    c1. Section = "documento completo":
          archivo Markdown                                             → LECTURA COMPUESTA (abajo)
          otro archivo                                                 → EFF^1, entero (compromiso declarado)
    c2. (Path, Section) con Kind ENTRY                                 → MainSha_eval          (16.13)
    c3. Match(EFF^1, Path, Section) = un único h:
          (Path, h) con Kind MODIFIED o REMOVED                        → EFF^1                 (compatibilidad)
          si no, Match(MainSha_eval) = uno                             → MainSha_eval          (normal)
          si no                                                        → fallo (S-12)
    c4. Match(EFF^1) = 0:
          Match(EFF) = uno, con Kind ADDED                             → NOT_APPLICABLE → fallo (P-15)
          Match(EFF) = 0 y Match(MainSha_eval) = uno                   → MainSha_eval  (sección posterior a EFF, de otra iniciativa)
          cualquier otro caso                                          → fallo (S-12)
    c5. Match(EFF^1) > 1                                               → fallo (S-12)
 d. Path fuera de M.Files:
      dentro de M.Surfaces                                             → MainSha_eval          (I-62 no lo modificó)
      fuera: blob(EFF^1,Path) = blob(EFF,Path) → MainSha_eval; si difiere → fallo (S-12)
```

**Lectura compuesta de «documento completo»** (archivo Markdown modificado por I-62). La cita se conserva tal cual y se lee como la lista de sus secciones en
el sentido de 16.3, el preámbulo y cada `##`:

| Unidad de lectura | Revisión |
|---|---|
| preámbulo, o `##` de EFF^1 con Kind MODIFIED o REMOVED | EFF^1 (texto de I-61) |
| preámbulo, o `##` de EFF^1 sin entrada en el mapa | `MainSha_eval`; si ya no existe allí, se omite, como en la lectura de I-61 de un documento completo en `MainSha` |
| `##` con Kind ADDED (solo I62) | **omitida**: no forma parte de la lectura I61 |
| `##` con Kind ENTRY (el punto de entrada de WORKFLOW) | `MainSha_eval`, **incluida**: es gobierno de toda unidad |
| `##` presente en `MainSha_eval` y ausente de EFF (posterior a EFF, de otra iniciativa) | `MainSha_eval` |

Orden: el de EFF^1, seguido de las secciones posteriores a EFF en el orden de `MainSha_eval`. El registro (paso 5) enumera cada unidad con su revisión.

**Compromiso declarado:** una sección `##` que I-62 modificó se lee **entera** en EFF^1, incluidas sus subsecciones no tocadas, que dejan de evolucionar
para las unidades I61, y lo mismo vale para la cita individual de esa sección; un archivo modificado que no es Markdown se lee entero en EFF^1; las secciones
solo I62 no se leen; todo lo demás sigue la evolución normal de `MainSha`, exactamente como en I-61.

**`Match(rev, Path, Section)`:** «preámbulo» designa el texto anterior al primer `##`; si no, cuenta los encabezados del archivo en `rev` cuya línea,
normalizada, es igual a `Section` normalizado (normalizar = espacios colapsados y extremos recortados); se ignoran las líneas dentro de bloques de código.

**Significado `/v1` conservado:** `MainSha` sigue siendo `origin/main` al emitir (A6 lo compara con un `origin/main` recién obtenido, la comprobación
`Remote` lo usa y `MB` se calcula con él, o con el nuevo en la reverificación de 16.7); `AuthorityRevision` sigue siendo el commit de los documentos y
cambios propios de la unidad. Lo único que cambia, y solo para unidades I61 y cláusulas del mapa, es la revisión en la que se lee el texto `EXTERNAL`. Una
delegación en curso que cruza EFF no exige nada nuevo al contrato: antes de escribir, el avance de `main` ya obliga a reemitir por 16.7 (S-13), y el contrato
reemitido conserva sus citas; después de escribir, la reverificación de 16.7 usa el `MainSha` nuevo con el contrato intacto, y `Evaluate` se ejecuta con ese
`MainSha_eval`.

**Mapa de cláusulas** (`rackcad-clause-map/v1`):

```json
{
  "Schema": "rackcad-clause-map/v1",
  "Protocol": "rackcad-protocol/I62",
  "LegacyProtocol": "rackcad-protocol/I61",
  "Surfaces": ["<la lista cerrada de esta subsección, en su orden>"],
  "Files":   [ { "Path": "...", "FileKind": "MODIFIED | ADDED | ENTRY", "BaseBlob": "<40 hex> | null", "EffBlob": "<40 hex>" } ],
  "Entries": [ { "Path": "...", "Section": "<línea de encabezado exacta | preámbulo>", "Level": 0, "Kind": "MODIFIED | REMOVED | ADDED | ENTRY" } ]
}
```

`Files` excluye el propio mapa. `BaseBlob` es `null` solo con ADDED o ENTRY. `Level` 0 corresponde al preámbulo. Una sección va desde su encabezado hasta el
siguiente con el mismo número de `#` o menos, e incluye sus subsecciones; el texto se normaliza con CRLF → LF y espacios colapsados.

**`Validate(M)`, en EFF:**

| Id | Regla |
|---|---|
| MV-1 | el mapa existe en EFF en la ruta fija, se interpreta como JSON y valida contra su esquema en EFF |
| MV-2 | `Surfaces` = la lista cerrada de esta subsección |
| MV-3 | **completitud de archivos:** {p bajo `Surfaces` : blob(EFF^1, p) ≠ blob(EFF, p)} ∖ {mapa} = {`Files.Path`}, sin duplicados; un archivo borrado no tiene clase → inválido |
| MV-4 | MODIFIED ⇒ `BaseBlob` = blob(EFF^1, p) ∧ `EffBlob` = blob(EFF, p); ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` = blob(EFF, p); ENTRY de archivo ⊆ {esquema del mapa} |
| MV-5 | `Entries` único por (`Path`, `Section`), con `Path` de un archivo MODIFIED |
| MV-6 | **igualdad con la derivación**, por archivo MODIFIED p, con H1 = secciones en EFF^1 y H2 = secciones en EFF (todos los niveles + preámbulo): MODIFIED = {h ∈ H1 ∩ H2 : texto normalizado distinto}; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1; ENTRY = {(AUTOMATION_PLAN, esta subsección), (WORKFLOW, el `##` del punto de entrada)}, ni más ni menos; encabezados únicos por archivo en ambas revisiones |
| MV-7 | el punto de entrada de WORKFLOW existe una sola vez en EFF y nombra esta subsección por su línea exacta; punteros presentes en §16 y 16.3 en EFF; 16.3 en EFF sin el puntero = 16.3 en EFF^1 (normalizado) |

| Defecto | Efecto |
|---|---|
| mapa ausente, ilegible o inválido contra su esquema | MAP_INVALID |
| entrada o archivo duplicado | MAP_INVALID |
| archivo modificado ausente de `Files`, o listado sin modificarse | MAP_INVALID |
| `BaseBlob` o `EffBlob` distintos de los observados | MAP_INVALID |
| entrada que contradice la derivación (MODIFIED con texto igual; modificada no listada; ADDED que existe en EFF^1; REMOVED que existe en EFF) | MAP_INVALID |
| ENTRY fuera de la lista cerrada; encabezados duplicados | MAP_INVALID |
| punto de entrada de WORKFLOW, 16.13 o punteros ausentes con EFF presente | MAP_INVALID |

**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.** STOP (S-12; P-15) al Coordinator. La corrección es un cambio normativo con su propia
autoridad, nunca un parche local del resolver.

**Encabezados estables:** una sección de I-61 modificada conserva su línea de encabezado exacta, y los encabezados son únicos por archivo en las superficies.
El encabezado del punto de entrada de WORKFLOW y el de esta subsección son fijos: cambiarlos es un cambio normativo con su propia compatibilidad.

| Id | Condición | Comportamiento |
|---|---|---|
| P-15 | `ProtocolSet` o esquema del contrato ≠ protocolo de la unidad; clasificación UNKNOWN, PENDING_G0 o DIRECT_ONLY (sin protocolo delegado adoptado) en un paso que depende de ella; cita de una unidad I61 a una superficie solo I62; mapa de cláusulas inválido | STOP |

### 16.14 Vigencia de las partes I62

Origen: Freeze de I-62 ([Proposal V14](initiatives/I-62-proposal-v14.md) y [Consensus Freeze](initiatives/I-62-consensus-freeze.md)) y
[ADR-0048](adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md), sucesor parcial de ADR-0046, en estado propuesto. Las partes de esta
sección marcadas **I62** (en su encabezado o en el rótulo de su bloque) son texto materializado **inactivo**:

- **Antes de `I62_EFFECTIVE_SHA`** no gobiernan ninguna operación real. Rigen I-61, el resto de este plan y el Workflow vigente. Su presencia no adopta nada
  ni cambia la conducta de ninguna unidad, y no se usan como autoridad para desarrollar I-62, que sigue gobernada por I-61 hasta su integración.
- **Desde `I62_EFFECTIVE_SHA`** rigen solo para las unidades I62, según su aplicabilidad (Proposal V14 §14.0 y §14.1): la ejecución delegada I62 sigue
  siendo opt-in y solo la adopta una unidad con decisión registrada I62_DELEGATED; una unidad DIRECT_ONLY trabaja sin ella. La excepción declarada es el
  resolver de compatibilidad de 16.13 (Proposal V14 Anexo E; destino propuesto hasta su materialización, Proposal V14 §3.1), que desde `I62_EFFECTIVE_SHA`
  rige la lectura de autoridades de toda unidad, sin cambiar la conducta de las unidades I61 más allá de la revisión en la que leen las cláusulas que I-62
  modificó.
- **Unidades I61:** siguen en I61 toda su vida, con la ejecución delegada de I-61 sin cambios. Los esquemas `/v1` no se retiran.

**Punto efectivo:** `I62_EFFECTIVE_SHA` es el primer merge en first-parent de `origin/main` cuyo segundo padre alcanza el **único** commit con el trailer
`Agent-Protocol-Normative: I-62` y cuyo primer padre no lo alcanza. Rige al aceptarse el push de `main`, y su identidad se registra en el tag
`integration/I-62`. Ausencia, duplicidad o derivación contradictoria = activación no válida: ninguna unidad es I62, y STOP al Owner. No hay activación
parcial.

### 16.15 Perfil del Principal y autoverificación (I62)

Clase «Coordinación principal», perfil PRINCIPAL_COORDINATION (Long-horizon, Frontera). **Aplica a las unidades I62_DELEGATED** y a sus ensayos; una unidad
DIRECT_ONLY no lo necesita para su trabajo ordinario. Requisitos **obligatorios**, como capacidades **por acción**:

| Acción del Principal | Obligatorios |
|---|---|
| RESUME_DECISION: reconstruir el estado y proponer la siguiente decisión, sin tomar la custodia | nivel y effort; `remote-facts` (lectura); `introspection` ≥ RUNTIME_OBSERVED |
| CUSTODY: custodia, relevo y orquestación **del protocolo delegado** (puntos durables BOOTSTRAP, Q0, Q7, QU, QH y QR, cesiones, bindings) | lo anterior + `repo-write` |
| evidencia local | lo anterior + `build-test`, solo para la acción que deba producir evidencia local |

El perfil y sus requisitos existen **aunque el binding del Principal no pueda aceptarse**. P-09 y P-10 (16.16) se evalúan **por acción**: un Principal en
BELOW_REQUIRED para CUSTODY no toma la custodia, pero puede producir una respuesta RESUME_DECISION si esa acción está en MATCH o ABOVE_REQUIRED.

**CUSTODY no es un permiso de trabajo directo.** El trabajo directo de una iniciativa (editar, probar, hacer commits, relevos de `WORKFLOW.md` §3) sigue las
reglas de WORKFLOW y de AGENTS, y ninguna acción de este perfil lo condiciona. Una CUSTODY en UNKNOWN o BELOW_REQUIRED bloquea solo la maquinaria delegada
(P-09/P-10 de esa acción), nunca el trabajo directo autorizado.

**Autoverificación.** Al abrir la unidad, el Principal observa su propia sesión y evalúa su `CONFIGURATION_STATUS` por acción (16.16):
- **solo hechos observables.** Niveles de la fuente: REQUESTED; CONFIGURED; RUNTIME_OBSERVED (metadato escrito por el runtime, ligado a sesión, turno o
  invocación e instante); SERVICE_ATTESTED (adicional, no exigida). El mínimo para los obligatorios es RUNTIME_OBSERVED; por debajo, UNKNOWN. Solo
  REQUESTED no se presenta como efectivo;
- **ninguna inferencia.** La autodeclaración del modelo no es fuente, y ni el nombre de un proveedor o de un modelo ni una entrada de catálogo acreditan por
  sí solos un requisito;
- **sin depender del binding.** La autoverificación produce su observación y su disposición aunque no haya binding: el binding consume la observación, y la
  observación no depende del binding;
- **sin valores optimistas.** Un requisito sin acreditar es UNKNOWN, y UNKNOWN nunca se convierte en MATCH.

La forma del registro de la observación, sus invalidadores y las fuentes de cada runtime no forman parte de esta subsección (Proposal V14 §6, §10 y
Anexo B.4).

### 16.16 `CONFIGURATION_STATUS` y disposición (I62)

Estados exactos: **MATCH**, **ABOVE_REQUIRED**, **BELOW_REQUIRED**, **UNKNOWN**. Agregado solo sobre los requisitos **obligatorios** de la acción:
1. BELOW_REQUIRED si alguno es insuficiente;
2. si no, UNKNOWN si falta acreditar alguno;
3. si no, ABOVE_REQUIRED;
4. en otro caso, MATCH.

Las métricas opcionales no alteran el agregado. Se registran **todas** las causas. El estado no es la disposición.

| Situación | Disposición |
|---|---|
| Principal en BELOW_REQUIRED para la acción | STOP de esa acción antes de trabajo sustantivo (P-09) |
| Rol en BELOW_REQUIRED o UNKNOWN | no hay binding ni trabajo dependiente (P-10); la investigación independiente sigue |
| Evidencia material contradictoria (incluidas dos filas del mismo requisito con valores distintos) | STOP S-04, **además** de UNKNOWN en ese requisito |
| MATCH / ABOVE_REQUIRED | habilita solo la configuración; los demás STOP siguen |
| Decisión del Owner | cambia requisitos o acepta riesgos dentro de su autoridad; nunca convierte lo no observado en MATCH |
| Recuperación | nueva observación; nunca reconfiguración automática ni reset |

| Id | Condición | Comportamiento |
|---|---|---|
| P-09 | Principal en BELOW_REQUIRED para la acción al abrir | STOP de esa acción antes de trabajo sustantivo |
| P-10 | requisito obligatorio de un rol o acción en UNKNOWN o BELOW_REQUIRED | no se vincula ni se invoca; no consume `attempts` |

El procedimiento de agregación y los ejemplos con esperado independiente (E1-E13) están en el procedimiento subordinado
([agent-execution/README](automation/agent-execution/README.md) §12).

### 16.17 Frontera de renderizado (I62)

Los contratos y las reglas de las partes I62 son neutrales: no contienen texto de prompt de ningún proveedor ni los ejecutables, las órdenes, los modelos o
el formato de prompt de un runtime. El adapter de cada runtime renderiza la invocación concreta (CLI o sesión) a partir del contrato neutral: es la
operación «renderizar» de su contrato (Proposal V14 §7). El prompt renderizado es representación, nunca autoridad. Para las unidades I62, proveedores,
ejecutables, modelos y recetas viven en los descriptores de adapter y en el catálogo, no en el núcleo.

### 16.18 Observación de capacidad y comprobaciones de relevo (I62)

Se separan dos objetos:

| Objeto | Qué observa | Invalidadores pertinentes | Quién y cuándo |
|---|---|---|---|
| **Observación de capacidad** (`rackcad-preflight/v1`) | un runtime candidato para un rol y una acción: localización y versión del ejecutable, autenticación (sin credenciales), introspección y nivel de garantía, permisos y sandbox, huella de configuración, celda del catálogo, perfil y requisitos | cambio de **instancia de host**, runtime, versión o ruta del binario, estado de autenticación, huella, blob de la entrada de catálogo o versión de `routing.md`. **No** la invalidan el binding (que la consume) ni el SHA de la rama | la sesión, antes del binding y al abrir (Principal) |
| **Comprobaciones de relevo** (16.4, sin cambio de semántica) | Git (`HEAD`, remoto, `origin/main`, árbol limpio, operaciones en curso), procesos, **estado de delegación derivado** (Proposal V14 B.8.2: como máximo una delegación abierta; ninguna aceptación nueva si es desconocido) y huella antes y después | se repiten en cada Exit/Entry | la sesión, en cada invocación |

Una CLI encontrada o un esquema válido no acreditan una invocación. Los esperados de cualquier control del host son **lo observado en el momento del
ensayo**, no una foto anterior.

**Registro.** La observación de capacidad se registra en `rackcad-preflight/v1` (`agent-execution/schemas/preflight.v1.schema.json`), y el relevo y la
verificación de una unidad I62 en `rackcad-relay-record/v2` y `rackcad-controller-verification/v2` (mismo directorio). Los esquemas `/v1` siguen para las
unidades I61. Un `HostInstanceState` UNOBSERVED impide heredar una observación de capacidad entre sesiones.

**Validación de los hechos del adapter**, que hace el productor:
1. fase 1: `Test-Json` del artefacto contra el esquema del núcleo;
2. fase 2: `Test-Json` de `Facts` contra `SchemaRef.Path`, con el blob comprobado en la `AuthorityRevision`;
3. `Facts.SchemaId` = `SchemaRef.SchemaId`;
4. registro de las dos validaciones (versión de PowerShell y resultado); sin él, el preflight no se acepta;
5. contraste independiente: el aceptante compara los hechos que coinciden (estado de autenticación, versión del binario y huella) con los del Exit/Entry
   de 16.4 tomados en la invocación. Una discrepancia es S-04;
6. adapter desconocido, esquema ausente, blob distinto, versión incompatible, propiedad inesperada o requisito omitido → P-14.

El procedimiento, las reglas de coherencia del registro y el saneamiento previo a la custodia están en el procedimiento subordinado
([agent-execution/README](automation/agent-execution/README.md) §13).

### 16.19 Frontera de adapters (I62)

**Contrato del adapter.** Nueve operaciones: 1. describir; 2. observar; 3. renderizar; 4. invocar; 5. observar el resultado; 6. cancelar; 7. confirmar la
terminación; 8. clasificar procesos; 9. declarar la huella.

**Descriptor.** Cada adapter tiene un descriptor `agent-execution/adapters/<AdapterId>.md`, cuya ruta se deriva del id por patrón fijo, y un esquema de hechos
estricto `agent-execution/schemas/adapters/<AdapterId>.facts.v<n>.schema.json`, derivado del id y de la versión que declara el descriptor, sin URLs ni
descargas. El descriptor declara por operación DISPONIBLE, NO APLICA o UNVERIFIED. Una operación UNVERIFIED que el rol necesita hace que la celda no sea
elegible para ese rol. Una sesión existente no acredita por sí misma invocación, cancelación ni terminación. `Facts` es el **único punto abierto** del
esquema del núcleo, y su frontera estricta es el esquema del adapter: un adapter nuevo se añade con su descriptor y su esquema, sin tocar el núcleo.

**Huella.** «Ninguna» solo con la demostración del descriptor y la aceptación registrada del Coordinator (para los adapters de I-62, en su gate F2). Un
adapter cuya huella es un archivo de configuración la declara con el SHA-256 del archivo y los nombres de sus secciones y claves, sin valores; el archivo
lo nombra su descriptor. P-01 se conserva, y P-11 lo generaliza a toda huella declarada.

| Id | Condición | Comportamiento |
|---|---|---|
| P-11 | huella declarada cambiada durante una cesión | STOP (igual que P-01) |
| P-14 | adapter desconocido, esquema ausente, versión incompatible o hechos inválidos | no elegible |

### 16.20 Binding y aceptación (I62)

Origen: Proposal V14 §5, §20.5.1 y §20.7, y Anexo B.2 y B.5. El binding asigna un rol a una celda concreta para una unidad o una tarea. Se registra en
`rackcad-binding/v1` (`agent-execution/schemas/binding.v1.schema.json`) y se referencia con `BindingRef` (B.2).

**Entradas:** los requisitos del rol y de la acción ([routing.md](automation/agent-execution/routing.md) §8), la independencia (16.21), una observación de
capacidad vigente (16.18) y la **elegibilidad** de ADR-0046 #4, que se conserva: invocación **medida** de la celda (MEASURED), consumo cubierto (OFFICIAL o
MEASURED, nunca UNKNOWN) y celda no `Stale`. Una cuota desconocida no equivale a consumo autorizado, y una entrada de catálogo no acredita los hechos.

**Nunca se infiere.** Ni el nombre de un proveedor o de un modelo, ni una fila del catálogo, ni el runtime solicitado, ni la autodeclaración de un runtime
acreditan un requisito ni producen un binding. Un requisito obligatorio sin acreditar es UNKNOWN, y UNKNOWN nunca se convierte en MATCH: no hay valor por
defecto optimista.

**Orden, sin ciclos.** El binding consume la observación; la observación no depende del binding:
1. candidatas del catálogo;
2. observación de capacidad (preflight);
3. invocación medida previa, de una medición autorizada que no es un binding;
4. registro del binding, transitorio hasta su custodia;
5. aceptación: individual (A7', por el Coordinator) o **materialización autorizada** bajo una autorización vigente, transitoria hasta la custodia;
6. cesión al rol.

**Algoritmo:**
1. requisitos del rol y de la acción;
2. candidatas con descriptor de adapter (16.19);
3. filtro: todos los obligatorios en MATCH o ABOVE_REQUIRED (16.16) y la elegibilidad completa;
4. independencia sobre `ActorRef`, `SessionRef`, entradas y proveedor (16.21);
5. el nivel más bajo adecuado, y el transporte;
6. registro `rackcad-binding/v1`;
7. aceptación: individual, o materialización autorizada con **todos** los criterios en SATISFIED.

Una celda sin medición previa no se vincula. **Sin celda elegible no se invoca** (P-10). **Rebinding:** un registro nuevo con el mismo ámbito y la misma
`TaskId`, sin tocar los contadores; el binding anterior del mismo rol y ámbito queda **obsoleto**.

**Cuatro estados distintos.** Ninguno implica al siguiente:

| Estado | Qué es | Qué permite |
|---|---|---|
| Candidato | binding registrado con `Acceptance.State` = PENDING, o candidata observada sin registro | nada: no se invoca |
| Aceptación individual | `ACCEPTED` con `Basis` = INDIVIDUAL_DECISION: decisión del Coordinator en `decisions/<unit>.md` (`DecisionRef`) | invocar el rol |
| Materialización autorizada | `ACCEPTED` con `Basis` = AUTHORIZED_MATERIALIZATION, `DecisionRef` = `null`, `AuthorizationRef` y `MaterializationCheck` con todos los criterios en SATISFIED | invocar el rol, dentro de la autorización |
| Binding aceptado | cualquiera de los dos `ACCEPTED`, custodiado a más tardar en el punto durable que reserva la invocación | lo que su `Role` y su acción permiten (16.23) |

`Acceptance` admite una sola transición, PENDING → ACCEPTED o REJECTED; el binding aceptado es una versión nueva del mismo `BindingId` con el resto del
contenido idéntico. Un contenido distinto con el mismo `BindingId` es S-04.

**Materialización autorizada.** Solo para ARCHITECT, bajo una `ReviewLoopAuthorization` del Coordinator (marcador `I62-REVIEW-LOOP-AUTHORIZATION:
<AuthorizationId>` en `decisions/<unit>.md`), y para REVIEWER, bajo la autorización que el Coordinator incluya en `RoleRequirements[].Materialization` del
contrato de gate (`rackcad-gate-contract/v2`). La autorización fija, como mínimo: `Role`, `AuthorizedActions`, `MinimumCapabilities` (cada una en MATCH o
ABOVE_REQUIRED), `RequiredIndependence`, `EligibleCells` (lista cerrada o el criterio cerrado de ADR-0046 #4), `ModelEffortBounds`, `Permissions` =
READ_ONLY, `Budget`, `ObjectFamily` y `Validity`; la `ReviewLoopAuthorization` fija además `ContinuesLoopInstanceId` (16.29). El Principal, **solo si se
cumple exactamente**:
1. observa un candidato (16.18);
2. comprueba cada criterio y registra `{CriterionId, Required, Observed, Result, Evidence}`;
3. materializa el binding con `DecisionRef` = `null` y `AuthorizationRef` = `{Path, Marker, AuthorizationId, Commit, Blob}`; `Commit` es resoluble por
   ResolveBranchRef (16.25) desde el punto que custodia el binding, en el que la autorización ya figura, y su imagen nunca es la de ese mismo punto;
4. lo custodia con su preflight y su comprobación;
5. lo invoca.

No relaja ningún criterio: **UNKNOWN cuenta como NOT_SATISFIED**. Si ningún candidato satisface la autorización, no hay invocación: STOP y
COORDINATOR_DECISION (autorización nueva o enmendada), o ESCALATION_OWNER si lo que falta es materia del Owner. **Sin aceptación fingida:** un binding
materializado nunca lleva un `DecisionRef` que simule una decisión individual que no ocurrió. Quien lo acepta o lo valida **reproduce** la comprobación con
el preflight custodiado y con la autorización tal como figura en `AuthorizationRef.Commit`/`Blob`; si no la reproduce, el binding es inválido (P-20).

**Vigencia de acción y acreditación histórica.** Una materialización nueva exige la vigencia de acción de la autorización: termina en el punto que registra
ARCHITECT_SATISFIED (en una materialización del REVIEWER, en el que registra REVIEWER_SATISFIED), en el agotamiento, con la revocación, la sustitución o la enmienda custodiadas por el Coordinator, o al pasar `Validity.Until`. Una
autorización terminada nunca se reutiliza para un binding nuevo ni vuelve a abrir un bucle (16.29). Un binding ya materializado y usado durante la vigencia conserva su acreditación
histórica: se valida contra la autorización de su `AuthorizationRef`, no contra la vigente. El destino de los intentos en curso al terminar la vigencia es
de la orquestación (Proposal V14 §20.5.1 y §20.6).

**Referencias (A2').** Un `BindingRef` es válido solo si es de la misma unidad, la misma tarea (o `Scope` = UNIT con `TaskId` nulo) y la revisión
custodiada, no está obsoleto por un rebinding y, si se presenta como custodiado, es CUSTODIED con su commit y su blob. TRANSIENT nunca es evidencia
custodiada.

| Id | Condición | Comportamiento |
|---|---|---|
| P-10 | requisito obligatorio de un rol o acción en UNKNOWN o BELOW_REQUIRED (16.16) | no se vincula ni se invoca; no consume `attempts` |
| P-20 | el Principal intenta un acto de autoridad que no tiene: declarar AGREED, cerrar o rebajar un REQUIRED, elegir una decisión del Owner, declarar Freeze o PASS, materializar un binding fuera de los criterios de la autorización, usar un resultado de REVIEWER para satisfacer al ARCHITECT | rechazo y STOP |

Procedimiento, reglas de coherencia del registro y casos: [agent-execution/README](automation/agent-execution/README.md) §14; algoritmo de selección de
celda: [routing.md](automation/agent-execution/routing.md) §9.

### 16.21 Independencia por riesgo (I62)

Origen: Proposal V14 §11 y Anexo B.2. La independencia se evalúa sobre identidades **observadas**, nunca sobre identificadores de binding ni sobre
autodeclaraciones:

| Dimensión | Se satisface cuando … | Evidencia |
|---|---|---|
| **Actor** | `ActorRef` distinto (instancia de runtime observada). **No** basta un `BindingId` distinto: dos bindings con el mismo `ActorRef` son el mismo actor | `ActorRef` con `Assurance` RUNTIME_OBSERVED |
| **Sesión** | `SessionRef` distinta (sesión de runtime de nivel superior; un subagente comparte la de su padre) | `SessionRef` RUNTIME_OBSERVED |
| **Contexto** | las entradas son solo el cierre efectivo de insumos (16.24), sin transcripción, memoria ni razonamiento de la referencia, y con las entradas automáticas del runtime enumeradas | invocación custodiada + `EffectiveInputClosure` + entradas automáticas + auditoría de lecturas cuando exista |
| **Proveedor** | proveedor distinto según los descriptores de adapter | descriptores |

Valores: REQUIRED, PREFERRED, NOT_REQUIRED. UNKNOWN en una dimensión REQUIRED no satisface; `Assurance` NONE deja la dimensión en UNKNOWN. Un proveedor
distinto con el contexto de la referencia **no** satisface Contexto. Una marca no se prohíbe si la independencia se acredita.

**Combinación.** Por (referencia, dimensión) rige el **máximo** (REQUIRED > PREFERRED > NOT_REQUIRED); las referencias distintas se evalúan por separado. Si
falta un REQUIRED, el binding del revisor o del verificador **no se acepta**, la revisión sigue pendiente y la operación dependiente se bloquea. Un PREFERRED
no satisfecho se registra.

**Referencia.** Es el conjunto de actores que escribieron el rango evaluado (`BaseSha..CurrentSha` y, con `SupersededCommits`, también los de esos commits).
La independencia se comprueba contra **cada** uno.

| Disparador | Rol o revisión | Referencia | Actor | Sesión | Contexto | Proveedor | Si falta un REQUIRED |
|---|---|---|---|---|---|---|---|
| Toda delegación | EXECUTION_CONTROLLER (verificación) | WORKER | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no es posible VERIFIED: BLOCKED de planificación |
| Coordinator y Worker en la misma sesión | EXECUTION_CONTROLLER | sesión Coordinator/Worker | REQUIRED | REQUIRED | REQUIRED | PREFERRED | ídem |
| Cambio de autoridad compartida | EXECUTION_CONTROLLER + REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | revisión pendiente; la tarea no se cierra |
| Operación destructiva o irreversible | REVIEWER de la autorización (además de S-05) | quien la propone | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no se ejecuta |
| Cambio sensible a la seguridad | REVIEWER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | revisión pendiente |
| Evidencia ambigua de alto coste | REVIEWER | autor de la evidencia | REQUIRED | PREFERRED | REQUIRED | PREFERRED | la evidencia no se usa para decidir |
| Alto coste de fallo (dimensión `High`) | EXECUTION_CONTROLLER | WORKER | REQUIRED | REQUIRED | REQUIRED | PREFERRED | como «toda delegación», con el proveedor registrado |
| Acumulación de roles | según 16.1 | — | — | — | — | — | combinación prohibida → binding rechazado |
| Cableado rutinario acotado | REVIEWER (si se pide) | WORKER | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | NOT_REQUIRED | — |

**Revisiones mayores de LIFECYCLE.** La revisión de diseño de NEW ARCHITECTURE y de FOUNDATION EVOLUTION antes del Freeze, y la conformidad de READY-06,
de una unidad I62_DELEGATED siguen el predicado de independencia de [INITIATIVE_LIFECYCLE](INITIATIVE_LIFECYCLE.md) §5 para unidades I62, que se evalúa
frente a `ReviewSubject` (B.2). Los tres modos de revisión se conservan.

### 16.22 Aceptación del paquete y comprobaciones de la verificación (I62)

Origen: Proposal V14 Anexo G.1. Para una unidad I62, la aceptación del paquete (16.5, A1-A8) y las comprobaciones de la verificación (16.9) se aplican con
estos deltas. Lo que no figura aquí no cambia: la precedencia de 16.9, el primer fallo, y `Identity`, `Remote`, `CleanTree`, `FreeText`, `Denials` y
`Handoff`.

| Comprobación | Tratamiento I62 | Tipo |
|---|---|---|
| A1' | + validación contra los esquemas del conjunto I62 (`delegation/v2`, `gate-contract/v2`, `binding/v1`) y de los hechos del adapter (16.18) | ampliado |
| A2' | + `BindingRef` válido según 16.20: misma unidad, misma tarea, revisión custodiada y no obsoleto | ampliado |
| A3'-A5' | + `RoleRequirements` de la delegación ⊇ los del contrato por rol (`Mandatory` ⊇ y cada dimensión de `Independence` ≥ la del contrato, con REQUIRED > PREFERRED > NOT_REQUIRED) + `SupersededCommits` igual al del contrato | ampliado |
| A6' | + `ProtocolSet` = protocolo de la unidad; distinto → P-15 | ampliado |
| A7' | elegibilidad mediante un binding **aceptado** (individual o materializado bajo autorización, con acreditación histórica; 16.20) + observación de capacidad vigente | sustituido (misma regla de ADR-0046 #4) |
| A8' | + `CountersSnapshot` = autoridad | ampliado |
| `Routing` (16.9 #12) | `required`: efectivo ≠ solicitado → fail (BLOCKED); `advisory`: pass con la discrepancia en `Evidence`, `Findings` y `Deviations`. **Caso añadido:** efectivo no observado al nivel mínimo → `required`: fail (BLOCKED); `advisory`: pass con la limitación anotada | ampliado |
| `Termination` (16.9 #1) | `Outcome` COMPLETED **y**, si es un proceso o una sesión, la operación 7 del adapter acreditada (16.19). Un proceso muerto con FAILED_TURN → fail (BLOCKED de transporte) | ampliado |
| `Ci` (16.9 #9) | los jobs que exige el AGENTS del repositorio de la unidad | sustituido (mismo significado en RackCad) |
| `Tests` (16.9 #10) | igual + `Skipped` coherente | ampliado |
| `Trailer` (16.9 #11) | coherente con el proveedor y el modelo del binding; con `SupersededCommits`, además presencia y celda de catálogo de los commits sustituidos | ampliado |
| `Scope` (16.9 #7) | sin cambio; con `SupersededCommits`, además el alcance acumulado | ampliado (solo con sustitución) |
| `Contract` (16.9 #4) | + `RoleRequirements` y `SupersededCommits`, como A3'-A5' | ampliado |

**Comparación no establecida.** Una comprobación cuya comparación no se pudo ejecutar o no terminó es `not_run`, y `not_run` cuenta como fail; nunca es
pass (ADR-0046 #6, conservado por ADR-0048). En particular, `Scope` = pass exige en su `Evidence` la salida reproducible de
`git diff --name-only BaseSha..CurrentSha` y la entrada de `AllowedWriteScope` que contiene cada ruta. Sin esa pertenencia por ruta, o si la comparación
auxiliar falló, `Scope` es `not_run`, y el Coordinator, que hace las comprobaciones entre artefactos de la verificación, rechaza un VERIFIED que la omita.

El procedimiento y los casos de cierre con su esperado exacto están en [agent-execution/README](automation/agent-execution/README.md) §14.

### 16.23 Orquestación de roles (I62): invocación de rol y contratos de salida

Origen: Proposal V14 §20.3, §20.5 y §20.7, y Anexo B.9 y B.10. `rackcad-role-invocation/v1` (`agent-execution/schemas/role-invocation.v1.schema.json`) es
el contrato semántico de toda invocación de rol en una unidad I62_DELEGATED. En una unidad I61, el Controller sigue con sus contratos de I-61 sin cambio.

**Propiedades:**
- sin texto de prompt de ningún proveedor: el adapter renderiza la invocación concreta (16.17);
- **objeto exacto obligatorio** cuando se revisa o se verifica algo: `Target` = commit, ruta y blob; solo PLAN puede no tenerlo;
- la salida del rol identifica el objeto exacto revisado o ejecutado;
- tres identidades distintas: la solicitud lógica (`LogicalReviewRequestId`), el contrato de cada intento (`InvocationId`) y el lanzamiento (`RunId`);
- permisos mínimos: ARCHITECT, EXECUTION_CONTROLLER y REVIEWER son de solo lectura (`Permissions` = READ_ONLY);
- contrato de salida fijado por el rol y la acción.

**Invocación limpia de una revisión.** Una sesión o un proceso elegible y distinto; el commit, la ruta y el blob exactos; solo el cierre efectivo de insumos
(16.24), sin transcripción ni memoria privada del autor; el contexto inyectado automáticamente, declarado en el resultado y contrastado con el cierre; la
fidelidad de los insumos acreditada antes de lanzar y comprobada después (16.24); un resultado estructurado; la terminación acreditada, y la identidad del
runtime observada por el invocador. En las revisiones mayores de LIFECYCLE se aplica además su predicado (16.21). Sin binding elegible no hay invocación
(P-10).

**Correspondencia cerrada entre rol, acción y contrato de salida:**

| `RequestedRole` / `Action` | `OutputContract` | Puede satisfacer | Nunca |
|---|---|---|---|
| ARCHITECT / REVIEW_DESIGN | `rackcad-architect-review-result/v1` (AGREED \| CHANGES REQUIRED \| BLOCKED — OWNER DECISION) | ARCHITECT_SATISFIED sobre el blob exacto; cierre o sustitución de hallazgos del ARCHITECT | GATE PASS, `EXECUTION_*` |
| REVIEWER / REVIEW_CHANGE | `rackcad-reviewer-result/v1` (hallazgos, recomendaciones y `Disposition` NO_FINDINGS \| FINDINGS) | el requisito de revisión operativa de 16.21 que lo pidió; el cierre de sus propios hallazgos | AGREED u otro veredicto de LIFECYCLE, ARCHITECT_SATISFIED, cierre o rebaja de hallazgos del ARCHITECT |
| EXECUTION_CONTROLLER / PLAN | `rackcad-delegation/v2` | la planificación de 16.5 | GATE PASS, Candidato; nunca `controller-verification/v2` |
| EXECUTION_CONTROLLER / VERIFY | `rackcad-controller-verification/v2` | la verificación de 16.9 | GATE PASS, Candidato; nunca `delegation/v2` |
| WORKER / IMPLEMENT | `rackcad-worker-handoff/v1` | — | verificación, GATE PASS |

PLAN y VERIFY no son intercambiables. El triple `RequestedRole` + `Action` + `OutputContract` debe coincidir con el binding aceptado y con la invocación: un
resultado cuyo esquema no es el `OutputContract` de su invocación es INVALID (P-19). Usar un resultado de REVIEWER para satisfacer al ARCHITECT, o para cerrar
un hallazgo del ARCHITECT, es P-20.

**Resultado inválido o incompleto** (no avanza el estado, P-19): falla el esquema o no es el `OutputContract`; el commit o el blob no coinciden con la
invocación; falta la declaración de contexto inyectado o la evidencia de independencia; un REQUIRED no tiene id, sección, evidencia o corrección; AGREED con un
REQUIRED abierto sin disposición; o hay una lectura fuera del cierre (P-22), una contradicción de identidad (P-23) o una representación no fiel (P-25), para lo
afectado.

| Id | Condición | Comportamiento |
|---|---|---|
| P-19 | salida de rol inválida o incompleta | no avanza; reejecución de transporte dentro del tope; agotado → STOP |

El ciclo de la revisión (estados, intentos, presupuestos y linaje) es de la orquestación (Proposal V14 §20.5 y §20.6). Procedimiento de construcción y de
validación: [agent-execution/README](automation/agent-execution/README.md) §15 y §16.

### 16.24 Orquestación de roles (I62): cierre de insumos, identidad del revisor y fidelidad

Origen: Proposal V14 §20.3.1, §20.3.2 y §20.3.3, y Anexo B.9 y B.11.

**Cierre efectivo de insumos** (`rackcad-input-closure/v1`). Una invocación distingue:

| Clase | Contenido | Quién la fija |
|---|---|---|
| `CanonicalInputs[]` | los artefactos versionados que la acción necesita: el objeto exacto, sus registros de revisión y sus autoridades | el Principal |
| `AllowedTransitiveInputs[]` | los archivos que un insumo canónico, o una instrucción que el runtime inyecta automáticamente, **obliga normativamente** a leer, cada uno con la obligación que lo exige (ruta, sección y blob) | el cálculo del cierre, antes de lanzar |
| `DeclaredRuntimeContext[]` | instrucciones del sistema y del runtime, mensajes de herramientas y catálogos que inyecta el adapter, con su tamaño o su hash cuando el adapter los expone | el adapter (operación 1) |
| `ForbiddenInputs[]` | transcripción y memoria del autor, sesiones ajenas, worktrees reales de otras unidades, artefactos transitorios | el contrato, siempre |

Cálculo, antes de cualquier lanzamiento:
1. se parte de `CanonicalInputs` y de las instrucciones automáticas que el descriptor del adapter declara para el directorio de trabajo;
2. cada obligación se clasifica: **READ** → transitivo permitido (opción A, por defecto); **ACTION_COMPATIBLE** → acción permitida;
   **ACTION_INCOMPATIBLE** → solo se omite con una **exención explícita y acotada** a esa invocación y esa acción, de una autoridad aplicable (opción B); sin
   exención no hay lanzamiento (STOP, COORDINATOR_DECISION); **CONDITIONAL_NOT_TRIGGERED** → se registra con su motivo; una condición ambigua se trata como
   READ;
3. se repite hasta el punto fijo; un ciclo no añade nada;
4. el cierre fija la `AuthorityRevision` y los blobs; si cambia uno, se recalcula antes de lanzar.

Una exención **omite** una acción; no la sustituye por otra evidencia. Una señal de salud de publicación (`HealthSignals`) no es evidencia equivalente a
ninguna prueba local, no satisface ninguna clase de prueba de AGENTS y no se propaga a gates, Candidato, cierre ni implementación. Tras la terminación, una
lectura registrada fuera del cierre es INVALID_REVIEW_CONTEXT (P-22), y un registro de lecturas sin lecturas o incompleto deja Contexto en UNKNOWN.

**Identidad del revisor.** `ReviewerDeclaredIdentity` es informativa: nunca acredita modelo, effort, sesión ni hilo, y nunca declara MATCH.
`InvokerObservedRuntimeIdentity`, que el invocador toma de los hechos del adapter, es la única fuente de la identidad efectiva. Una contradicción entre una
declaración concreta y la observada es S-04 (P-23); una declaración UNKNOWN no es contradicción. Una compactación del contexto del revisor se registra, no
invalida la revisión por sí sola y no aporta evidencia propia.

**Fidelidad de los insumos** (`rackcad-input-fidelity/v1`). Un registro `CanonicalInputFidelity` por cada insumo del cierre y por la invocación
renderizada, con los bytes canónicos (blob y SHA-256), su codificación (UTF-8 según los bytes reales, BOM y fin de línea), la representación de transporte,
su hash o el método de comparación, cada carácter no ASCII distinto del corpus contado en ambos lados, el estado (FAITHFUL \| FAITHFUL_NORMALIZED \|
DEGRADED_BOUNDED \| DEGRADED_UNBOUNDED \| UNVERIFIED) y los tramos degradados. La única normalización fiel es el fin de línea y el salto final
(FAITHFUL_NORMALIZED); cualquier otra diferencia es degradación semántica.
- **Antes de lanzar:** bytes canónicos, transporte en UTF-8 y comprobación por el **mismo camino de lectura** que usará el revisor. Sin FAITHFUL o
  FAITHFUL_NORMALIZED no hay lanzamiento (P-24).
- **Después de la terminación:** se compara la **representación entregada al revisor**, con sus truncamientos y omisiones; sin una fuente que la represente,
  UNVERIFIED.
- **Fallo cerrado:** con DEGRADED_BOUNDED, solo se ingieren los hallazgos y las disposiciones cuyas premisas son independientes de la degradación; los demás
  son INVALID_PREMISE y no cambian linajes; con DEGRADED_UNBOUNDED o UNVERIFIED no se ingiere nada (P-25).

**Independencia de una premisa.** Cada premisa identifica la proposición normativa completa (`PremiseRefs`). Con algún insumo en DEGRADED_BOUNDED, es
independiente solo si su envoltorio directo es fiel (unidad estructural completa, expresión completa y cadena de títulos), si **todas** las unidades de su
clausura de dependencias normativas llegaron fieles al revisor, si todas sus dependencias se resolvieron de forma determinista y si su contexto de control es
fiel. La clausura recorre el **manifiesto de dependencias normativas** (`rackcad-normative-dependency-manifest/v1`,
`agent-execution/schemas/normative-dependency-manifest.v1.schema.json`), nunca la prosa:
- la resolución sigue este orden: referencia calificada → sección del mismo documento → destino propuesto de la Proposal → identificador definido →
  AMBIGUOUS_REFERENCE (sin calificador y con algún destino posible en otro documento, aunque sea uno solo) → UNRESOLVED_REFERENCE;
- un documento entero entra solo por un conjunto de entrada acotado o por una regla compuesta; si no, WHOLE_DOCUMENT_UNBOUNDED;
- solo se siguen aristas de control declaradas; un enlace informativo, una cita histórica, un ejemplo o una procedencia nunca amplían la clausura;
- una unidad sin entrada, o con `Complete` = false, tiene metadatos incompletos;
- los ciclos terminan por los visitados y los duplicados se funden por su identidad canónica; un nodo es terminal solo con cero dependencias de control sin
  resolver.

AMBIGUOUS_REFERENCE, UNRESOLVED_REFERENCE, WHOLE_DOCUMENT_UNBOUNDED y los metadatos incompletos dejan la independencia en **UNKNOWN**, que no acredita. El
manifiesto de las superficies congeladas de I-62 es [I-62-normative-dependency-manifest.json](initiatives/I-62-normative-dependency-manifest.json); lo
revisa el Architect como parte del objeto. La evidencia de fidelidad y de independencia la observa y la custodia el **invocador**, no el revisor.

| Id | Condición | Comportamiento |
|---|---|---|
| P-22 | lectura registrada fuera del `EffectiveInputClosure` | INVALID_REVIEW_CONTEXT: el resultado no se ingiere como dictamen; el intento cuenta; reejecución de transporte dentro del tope; agotado → STOP |
| P-23 | contradicción entre `ReviewerDeclaredIdentity` e `InvokerObservedRuntimeIdentity` | S-04: el resultado no se ingiere y la orquestación se detiene hasta la decisión |
| P-24 | preflight de fidelidad no acreditado: transporte UTF-8 no establecido o camino de lectura que degrada algún carácter del corpus | no se lanza; no consume presupuesto si no se llegó a reservar |
| P-25 | degradación semántica observada en la representación entregada al revisor | INPUT_FIDELITY_INVALID: los hallazgos y las disposiciones cuyo envoltorio o cuya clausura la contienen, o cuya independencia queda UNKNOWN, son INVALID_PREMISE y no cambian linajes; si no se puede acotar, no se ingiere nada; el intento cuenta |

Procedimiento del cierre, del preflight de fidelidad y del validador del manifiesto: [agent-execution/README](automation/agent-execution/README.md) §15.

### 16.25 Custodia por unidad (I62): puntos durables, ventana, CAS y rebase

Origen: Proposal V14 §8.1-§8.5, §8.8, §8.9 y Anexo B.8, con la [A-1](initiatives/I-62-A-1.md) acordada (D2-1..D2-12). El estado canónico de una unidad
I62_DELEGATED es `docs/automation/state/<unit>.yml` con `rackcad-automation-state/v2`
([esquema](automation/agent-execution/schemas/automation-state.v2.schema.json); contrato semántico, invariantes y reconstrucción: Proposal V14 Anexo B.8 con
A-1). Cada escritura de ese archivo es un **punto durable**: un commit de la sesión en la rama de la unidad, publicado por CAS, con `record_version` + 1. Solo
hay puntos durables donde 16.4 permite escrituras Git de la sesión. Procedimiento: [agent-execution/README](automation/agent-execution/README.md) §17.

**Puntos durables:**

| Punto | Cuándo | `window.state` | `principal.state` | Fija |
|---|---|---|---|---|
| **BOOTSTRAP** | arranque de la unidad, antes de la revisión de G0, o BOOTSTRAP de adopción (16.28) | CLOSED | HELD | `protocol` con la evidencia de clasificación y `g0_acceptance` PENDING; titular con su preflight y su binding propuesto (`principal.acceptance` PENDING), custodiados en el mismo commit; contadores vacíos |
| **Q0** | paso (1) de 16.4, obligatorio antes de cada ventana | OPENABLE | HELD | `task_intent` con el contrato custodiado; si es una corrección, `attempts` +1 y su `CorrectionLaunch` (16.27) en el mismo commit |
| **Q7** | paso (7) de 16.4: tras la verificación y los controles, o tras el cierre declarado de la ventana | CLOSED | HELD | `last_window`, cadenas, contadores desde el diario, manifiesto de custodia |
| **QU** | actualización del titular sin ventana | CLOSED | HELD | fase, `state`, `gate`, `next_action`, `last_evidence_commit`, la intención siguiente u `orchestration` (16.29); no toca `window`, `last_window` ni los contadores de ventana. `custody.point_kind` = ORDINARY, o REBASE_RECONCILIATION tras un rebase fuera de ventana |
| **QH** | el titular libera la unidad | CLOSED | RELEASED | la intención siguiente, si existe, y la liberación; el titular liberado no opera después |
| **QR** | recuperación o transferencia del titular | CLOSED | HELD (nuevo titular) | la designación del Coordinator; si había una ventana posiblemente abierta, su cierre ABANDONED (16.26). Con avance de `main`, es el QR REBASE_RECONCILIATION de una toma con rebase |

Todo commit que modifica el archivo de estado es exactamente uno de estos puntos. Los demás commits de la sesión fuera de una ventana (documentos, evidencia)
no son puntos durables. Tras un Q0, cualquier commit de la sesión impide abrir la ventana y obliga a cerrarla (T18) y a emitir un Q0 nuevo.

**Ventana de cesión.** Se abre con la primera cesión posterior a un Q0 y se cierra con el Q7 o el QR siguiente:
- **W-1:** solo se abre con `HEAD` = `origin/<rama>` = un commit Q0;
- **W-2:** entre el Q0 y el cierre no hay escrituras Git de la sesión; la única excepción es el rebase de 16.7, que no escribe el archivo de estado;
- **W-3:** el Worker nunca escribe el archivo de estado.

El último punto durable de la rama dice, por sí solo, si puede existir una ventana: **solo si es un Q0**.

**Estado de delegación derivado** (no se almacena; Proposal V14 B.8.2): `DS(L, J)` con `L` = el último punto durable en `origin/<rama>` y `J` = el diario de
la ventana `L.window.seq` (los `relay-record/v2` con ese `WindowSeq`, encadenados por `PrevRelaySha256`):

| Condición | `DS` | En I-61 (16.6) |
|---|---|---|
| CLOSED y `task_intent` = `null` | NONE | ninguna delegación abierta |
| CLOSED y `task_intent` ≠ `null` | PLANNED | ninguna delegación abierta (solo intención) |
| OPENABLE; `J` íntegro; sin aceptación | PLANNED | ninguna delegación abierta |
| OPENABLE; `J` íntegro; aceptación de `d` sin verificación válida ni cierre declarado | **ACCEPTED_OPEN** | **delegación abierta** |
| OPENABLE; `J` íntegro; aceptación + verificación válida o cierre declarado | CLOSED_PENDING_CUSTODY | ninguna delegación abierta; falta el Q7 |
| OPENABLE; `J` no disponible o con la cadena rota | UNKNOWN | posiblemente abierta: ninguna aceptación nueva (P-02); reconstrucción (16.26) |

`Exit.OpenDelegations` = 1 con ACCEPTED_OPEN y 0 con NONE, PLANNED o CLOSED_PENDING_CUSTODY. Con UNKNOWN no hay Exit válido (STOP).

**Durable y transitorio.** Es durable en Q0 la intención (`TaskId`, `Attempt`, clase, contrato custodiado, roles planificados con los bindings ya aceptados),
`attempts` y el `CorrectionLaunch` de una corrección. Entre Q0 y Q7 es transitorio el diario encadenado: aceptación de bindings nuevos, delegación y su
aceptación, cesiones, terminaciones, handoff, verificación, controles negativos, lanzamientos (también los inciertos), reejecuciones, `RebaseMap` y, en su
caso, el TRANSFER de T12a. Es durable en Q7 el cierre de la ventana, la custodia del diario (manifiesto TRANSIENT → CUSTODIED en
`docs/automation/evidence/<unit>-agent/<task>/…`), las cadenas y los contadores. Una referencia transitoria nunca se presenta como evidencia custodiada.
Ningún SHA se escribe dentro de su propio commit: `ChainBaseSha` (el Q0 de la primera ventana de la tarea) y `VerifiedSha` se fijan en el Q7 siguiente.

**SHAs y CAS.** `BaseSha` = `HEAD` al abrir la delegación = el Q0 de la ventana; `RedSha` y `CurrentSha` son commits del Worker; `Identity` (16.9) no cambia.
En cada punto durable se lee `record_version` n en `HEAD` = `origin/<rama>`, se escribe n+1 y se publica sin force. Un push rechazado es una transición **no
acreditada en el remoto**: el commit local se conserva y se inspecciona la causa antes de repetir.

| Causa observada | Tratamiento |
|---|---|
| no fast-forward (el remoto avanzó) | `fetch` y clasificación según T10 (16.26) |
| permisos o autenticación | S-06 |
| transporte | reintento acotado tras la inspección |
| estado remoto desconocido | STOP |

Sin servicio de locks ni registro global: Git prueba identidad y remoto, no quién opera en otra máquina.

**Rebase dentro de una ventana activa** (último punto Q0). Rige la reverificación de 16.7 y **no** se inserta ningún QU en la ventana. El `RebaseMap` del
diario incluye en `StateFields[]` todo SHA persistido del estado que la reescritura afecta (las cadenas de todas las tareas, `chain_red_sha`,
`last_window.verified_sha`, `unverified_commits[].sha`, `last_evidence_commit` y los campos vivos de la orquestación de abajo). El Q7 o QR que cierra la
ventana hace la reconciliación durable: fija `custody.last_rebase`, añade a `custody.rebase_history[]` todos los mapas de los rebases de la ventana en el orden
en que ocurrieron (nunca una composición que pierda un paso) y escribe cada campo afectado con su imagen. Los resultados propios de la ventana se registran ya
sobre la rama rebasada, y la corrida de CI del RED se sigue citando por el SHA original (`CiRuns`).

**Rebase fuera de una ventana** (titular vigente; T19):
1. `git fetch`; se registran `branch_before` (= `origin/<rama>`), `main_before` y `main_after`;
2. rebase; con conflictos, `git rebase --abort` y STOP (16.7);
3. construcción del `RebaseMap`: cada commit reescrito, del original a su imagen, con `git patch-id` igual, y cada campo SHA del estado, del original a su
   imagen;
4. si una imagen o una identidad de parche no se acredita, la rama local vuelve a `branch_before`, no se publica nada y STOP (16.7);
5. publicación con `git push --force-with-lease=<rama>:<branch_before>`; un rechazo es STOP (T10), sin reintento a ciegas. Este force-push no es un punto
   durable;
6. **QU REBASE_RECONCILIATION**, obligatorio antes de cualquier Q0 o acción delegada: custodia el mapa en
   `docs/automation/evidence/<unit>-agent/rebase/<RunId>/rebase-map.json`, fija `custody.last_rebase`, lo añade a `custody.rebase_history[]`, reescribe cada
   campo según la tabla siguiente y deja intactos los contadores y las `TaskId` (sin cadena en curso, `TaskId` = SESSION). Se publica por CAS normal, en
   fast-forward sobre `branch_after`.

Entre los pasos 5 y 6, `HEAD` contiene la imagen del último punto durable con SHAs sin reconciliar. Un evaluador lo detecta (I-H02: un SHA de rama vivo del
estado que no es ancestro de `HEAD`) y no admite Q0 ni acción delegada hasta el QU.

| Campo | En el QU (o QR) de reconciliación |
|---|---|
| `chains[].chain_base_sha`, `chains[].chain_red_sha`, `last_window.verified_sha`, `unverified_commits[].sha` | imagen (`window_seq` y `superseded_by` iguales) |
| `automation_state.last_evidence_commit` | imagen si es un commit reescrito de la rama; sin cambio si es ancestro de `main_before` |
| `orchestration.loop.object.commit` | imagen; `path`, `blob`, fase, contadores y linajes iguales |
| `orchestration.review_requests[].object.commit` (solicitud OPEN) | imagen; mismo `path` y `blob` |
| intento en INVOCATION_PLANNED o BUDGET_RESERVED | **replanificado**: invocación nueva (`InvocationId` nuevo) reconstruida entera sobre las imágenes (el `Target` y cada referencia de rama de la invocación, resueltos con ResolveBranchRef a su imagen con el mismo `path` y `blob`), con la misma reserva (`reserved_at`, contadores y `BudgetSnapshot` iguales); una referencia que no resuelve es STOP sin publicar |
| intento en LAUNCHING | **sin cambio**: conserva su invocación y su `Target` como intención histórica; su `Target` debe resolver por ResolveBranchRef con la historia completa de n (`n.custody.rebase_history[]`, que ya termina en el mapa nuevo) y la punta rebasada; si no resuelve, STOP sin publicar. Su destino lo decide después la evidencia de arranque (16.29) |
| intento en LAUNCHED o posterior; solicitud terminal | sin cambio: histórico, acreditado por su `blob` y resuelto con ResolveBranchRef y EquivalentReviewedObject cuando hace falta |
| `next_action.target.commit` | imagen del objeto que copia, con el mismo `path` y `blob` |
| `task_intent.contract` | el contrato custodiado es inmutable; si contiene SHAs reescritos o un `MainSha` obsoleto se reemite (16.7) y el QU lleva la intención con `kind` = REISSUE, o `task_intent` = `null` hasta la reemisión |
| `counters.rebase_recoveries[].last_rebase_map` | la `StateRef` nueva si el rebase pertenece a esa tarea; `count` según 16.7 |
| `protocol.effective_sha`, `protocol.basis.*` | sin cambio |
| `StateRef` (`{path, blob}`) | sin cambio; cada ruta existe con su blob en el árbol del propio punto |
| `custody.rebase_history[]` | recibe el mapa nuevo (append-only) |

**Ningún SHA se descarta en silencio:** un campo con un SHA de la rama reescrita sin imagen acreditada es STOP, nunca `null` ni borrado. `chain_red_files`
es durable y no se recalcula. **Otra máquina:** la reconciliación y la Q0 siguiente solo usan lo que hay en el remoto (las imágenes, los mapas custodiados y
sus `patch-id`); los commits originales no hacen falta.

**`RebaseMap`** (Proposal V14 B.8.7 con A-1 D2-1): `RunId`, `TaskId`, `MainBeforeSha`, `MainAfterSha`, `BranchBeforeSha`, `BranchAfterSha`;
`Commits[]` = `{OriginalSha, ImageSha, PatchId, PatchIdEqual}` por cada commit que **ese** rebase reescribió (`MainBefore..BranchBefore`), en orden;
`StateFields[]` = `{Field, OriginalSha, ImageSha}` por cada campo SHA del estado, incluidos los que no cambian y los vivos de la orquestación
(`orchestration.loop.object.commit`, `orchestration.review_requests[<id>].object.commit` de cada solicitud OPEN y
`orchestration.review_requests[<id>].attempts[<seq>].Target.commit` de cada intento en INVOCATION_PLANNED o BUDGET_RESERVED); `CiRuns[]`; `Unmapped[]`
vacío. Un campo sin entrada, una entrada duplicada o una imagen fuera de la rama publicada invalidan el mapa: no se publica y es STOP. **Una entrada solo
acredita un commit que ese rebase reescribió de verdad:** una entrada compuesta X → X'' de un rebase que no reescribió X nunca sustituye al paso que falta.

**ResolveBranchRef(ref, H, HEAD)**, con `ref` = `{Commit, Path, Blob}` y `H` = `custody.rebase_history[]` del punto que consume la referencia: (1) si
`Commit` es ancestro de `HEAD` y el blob de `Path` en `Commit` es `Blob`, resuelve a `Commit`; (2) si no, se toma el primer mapa de `H` con `Commit` como
`OriginalSha` (ninguno → UNRESOLVED) y su `ImageSha` (con `PatchIdEqual`; si no, UNRESOLVED); para cada mapa posterior, en orden, la imagen sigue si figura
como `OriginalSha`, o debe ser ancestro de su `MainBeforeSha` (si no, falta un paso → UNRESOLVED); (3) resuelve a la imagen solo si es ancestro de `HEAD` y
el blob de `Path` en ella es `Blob`, con el `PatchId` recalculado igual al del mapa. Nunca se adivina una correspondencia (ni por parecido de parches fuera de
los mapas, ni por ruta, ni por árbol). UNRESOLVED tiene el efecto que el texto da a una referencia sin ancestro válido (binding inválido, P-20; rechazo de
A2'; I-S18 en fallo).

**Lectura de la ancestría de las referencias de rama.** Donde un texto exige que el `Commit` de una referencia de rama custodiada (`BindingRef.Location.Commit`
CUSTODIED, `AuthorizationRef.Commit`, la `AuthorityRevision` de una invocación, los commits de `IndependenceRequirements.ReviewSubject`) sea ancestro del
punto, se lee: **resoluble por ResolveBranchRef desde el punto que la consume**. La condición sin ciclos se conserva sobre las imágenes. Los artefactos
históricos nunca se reescriben; una invocación nueva o replanificada reconstruye sus referencias vivas sobre las imágenes.

**EquivalentReviewedObject(A, B, H)** es verdadero solo si `A.path` = `B.path`, `A.blob` = `B.blob` y, además, `A.commit` = `B.commit` o la cadena de mapas de
`H` prueba que uno es la imagen del otro. Rige toda comparación de `Target`, `EvaluatedObject`, objeto de la solicitud o `loop.object` tras un rebase probado:
mismo `path` con otro `blob`, o una imagen no probada, no son equivalentes.

**Toma de custodia con avance de `main`** (REBASE_TAKEOVER, T22). Un Principal entrante tras un QH o tras T12b, con `main` avanzado, sigue un solo orden:
1. **designación acotada** del Coordinator con el marcador `I62-REBASE-TAKEOVER: <BindingId>`, la aceptación de su binding (16.28) y, para T12b, la constancia
   de la terminación acreditada del titular anterior;
2. **autoridad limitada a cinco acciones:** `git fetch`; el rebase de WORKFLOW §4; la publicación con `--force-with-lease=<rama>:<branch_before>`; la
   construcción y custodia del `RebaseMap`; y la publicación de **un** QR combinado. Nada más: ni trabajo ordinario, ni Q0, ni relevos, ni delegaciones;
3. **QR combinado** (`point_kind` = REBASE_RECONCILIATION) que registra al nuevo titular y su designación, reconcilia todos los SHA con la tabla anterior y, en
   T12b con último punto Q0, cierra la ventana como ABANDONED con la reconstrucción de 16.26;
4. solo después del QR el nuevo titular hace trabajo ordinario o publica Q0.

Una imagen o un `patch-id` no acreditables devuelven la rama local a `branch_before`, sin publicar, y STOP. Un `--force-with-lease` rechazado es STOP (T10).
Una caída entre el force-push y el QR deja el estado sin reconciliar (I-H02): solo el mismo designado, o uno nuevo por decisión del Coordinator, completa el
QR. Sin avance de `main`, T16 y T12b siguen con un QR ORDINARY sin rebase.

### 16.26 Recuperación y transiciones (I62)

Origen: Proposal V14 §9.1, §9.2 (análisis con SHAs simbólicos en el Anexo F) y Anexo B.8.5-B.8.6.

**Evidencia de fin de un escritor:**

| Evidencia | Definición | Basta para |
|---|---|---|
| **TERMINATION_ACCREDITED** | operación 7 del adapter ligada al escritor exacto (`ActorRef`/`RunId`, PID + `CreationDateUtc`) y, para invocaciones de trabajo, `Outcome` registrado. Para sesiones de Principal, la observa otra sesión autorizada o la atesta el Owner, nunca la propia sesión observada | cerrar la cesión o transferir la custodia, con las demás comprobaciones |
| **ISOLATION_ACCREDITED** | no se admite (no hay mecanismo medido) | — |
| **NO_OBSERVATION** | ausencia de observación | nada |

Estados del escritor: WRITER_ALIVE, TERMINATION_UNACCREDITED y ORPHAN_CONFIRMED (terminación acreditada sin entrega ni cesión resuelta). Los dos últimos no
autorizan por sí mismos tomar posesión.

**Transiciones** (P = titular; N y B = titulares nuevos; cada registro durable por CAS):

| # | Desde | Evento y evidencia | Decide | Registro | Límite |
|---|---|---|---|---|---|
| T0 | BOOTSTRAP (aceptaciones PENDING) | decisión de G0 con los marcadores (16.28) | Coordinator / P | QU con las aceptaciones y el binding aceptado | Q0 solo tras ACCEPTED en ambas |
| T1-T7 | Q0 (ventana k) | cesiones al Controller y al Worker, aceptación A1'-A8', entrega (`HEAD` = remoto = G), verificación y controles | P / Coordinator | diario con `WindowSeq` k; **Q7** con `last_window` | sin escritura Git dentro de la ventana |
| T3' | tras planificar | rechazo en la aceptación | Coordinator | diario; Q7 NOT_ACCEPTED | ningún Worker |
| T8 | cesión activa | tope vencido: operaciones 6 + 7 | P | diario | sin terminación acreditada → T11 |
| T9 | cesión al Worker | Worker caído con commits sin handoff, terminación acreditada | P | diario | `Handoff` → BLOCKED; se verifican los commits verificables |
| T10 | cualquiera | `HEAD`/remoto ≠ lo esperado: (a) avance del Worker en su alcance → normal; (b) commit local de la sesión sin publicar → publicarlo en un punto permitido o P-08 si estaba en cesión; (c) escritura ajena → STOP P-02/S-07; (d) `main` cambió → S-13 / 16.7 | — | según el caso | no todo desajuste va a rebase |
| T11 | titular ausente | NO_OBSERVATION | Coordinator | ninguno; STOP P-12 | prohibido tomar posesión, reset, borrado o matar procesos |
| T12a | ORPHAN_CONFIRMED(P), último punto Q0, diario íntegro | designación de N + terminación de P acreditada | Coordinator / N | TRANSFER encadenado en el diario; el Q7 fija el titular | N opera solo tras el TRANSFER |
| T12b | ORPHAN_CONFIRMED(P), último punto ≠ Q0, o Q0 sin diario o roto | designación + inspección que preserva el trabajo + reconstrucción | Coordinator / N | **QR**; con Q0 previo, cierre ABANDONED | con avance de `main`: T22 |
| T13 | cualquiera | dos recuperaciones concurrentes | — | gana el primer CAS o la única designación | el perdedor no opera |
| T14 | cualquiera | cambio de máquina sin acreditar | Coordinator | ninguno; STOP P-13 | no se reconstruye trabajo inaccesible |
| T15 | cualquiera | registro que contradice el hecho físico | Coordinator | ninguno; S-04/P-02 | prevalecen los hechos (WORKFLOW §10) |
| T16 | QH, o Q7, QU o QR con titular P, sin avance de `main` | transferencia: terminación de P acreditada + designación con la aceptación del binding de B | Coordinator / B | **QR** ORDINARY con el binding aceptado custodiado | con avance de `main`: T22 |
| T17 | BOOTSTRAP, Q7, QU o QR | liberación (último punto ≠ Q0) | P | **QH** | P no opera después |
| T18 | Q0 sin cesión registrada | retirada de la intención (decisión del Coordinator) | P | **Q7** WITHDRAWN | — |
| T19 | BOOTSTRAP, Q7, QU o QR | rebase de 16.7 (16.25) | P | force-push y **QU** REBASE_RECONCILIATION | ni Q0 ni acción delegada entre ambos |
| T20 | `/v1` DIRECT_ONLY, unidad posterior, sin delegación | adopción (16.28) | P / Coordinator | BOOTSTRAP de adopción → decisión → **QU** | Q0 solo tras ACCEPTED en ambas |
| T21 | BOOTSTRAP `/v2` con aceptaciones PENDING y `window.seq` = 0 | decisión DIRECT_ONLY | Coordinator / P | el commit que la registra vuelve a `/v1` | sin pérdida de custodia |
| T22 | QH, T12b o T16, con avance de `main` | toma con rebase (16.25) | Coordinator / designado | force-push y **un** QR REBASE_RECONCILIATION | antes del QR, solo las cinco acciones |

**Fallos del mandato:** Principal ausente → T11, T12a, T12b o T16; Worker caído → T9; Controller sin contexto → S-12 (16.11); cuota agotada → P-06;
autenticación perdida → S-06; cambio de máquina → T14; handoff ausente → T9 / `Handoff`; diario perdido → reconstrucción (nunca cero lanzamientos; commits sin
verificar declarados); artefactos transitorios y trabajo sin commit en el host → se conservan; SHA distinto → T10.

**Reconstrucción sin diario** (Proposal V14 B.8.5). Fuentes admitidas: la historia de `origin/<rama>`, el último punto durable `L` y lo custodiado hasta él,
las corridas de CI y los procesos de los hosts accesibles; nunca transcripciones ni registros de otras sesiones. Con `L` ≠ Q0, la reconstrucción es completa
desde Git (`DS` = NONE o PLANNED, contadores durables). Con `L` = Q0 y el diario no disponible o roto:
1. **terminación:** sin terminación acreditada del titular → STOP P-12; host inaccesible → STOP P-13; si no, los procesos de la lista cerrada de 16.4 en ese
   host, sin participantes vivos;
2. **commits posteriores a `L` en el remoto:** R-1 ninguno; R-2 solo commits dentro del `AllowedWriteScope` del contrato de la intención y con el trailer de
   una celda del catálogo (hubo un Worker y una delegación aceptada); R-3 cualquier otro → T10(c), STOP;
3. **cierre:** la decisión del Coordinator designa a N, que publica un QR con `closure` ABANDONED, `closure_source` COORDINATOR, `delegation_run_id` =
   `"UNKNOWN"` (o el probado) y `reconstruction` = el registro; en R-2 los commits del Worker van a `unverified_commits` y la cadena sigue IN_COURSE;
4. **contadores, por fase en orden** (PLANNING, WORK, VERIFICATION, NEGATIVE): `launched_p` = los lanzamientos que prueba una fuente admitida (R-2 prueba
   PLANNING = 1 y WORK = 1); `uncertain_p` = 1 si la fase pudo lanzarse y ninguna fuente la prueba ni la excluye; `reconstructed` = true; el Coordinator
   puede fijar valores mayores, **nunca menores**; `attempts` no cambia;
5. **reejecución:** la ventana siguiente de la misma tarea cuenta como reejecución de la fase perdida (R-1: PLANNING; R-2: WORK); la tercera → P-04.

Un diario íntegro hasta un registro y roto después, o con un registro que contradice un hecho físico, es S-04 y se trata como no disponible desde el primer
registro inválido.

**Commits sin verificar.** Mientras `unverified_commits` tenga entradas de una tarea sin `superseded_by`: ninguna verificación de esa tarea cuenta como
trabajo delegado terminado; la delegación siguiente exige un contrato con `SupersededCommits` = esas entradas y `SupersededBaseSha` = el Q0 de la ventana
abandonada (si no, A-n); y su verificación evalúa además el `Scope` acumulado de `SupersededBaseSha..CurrentSha` (sin los commits de la sesión), el `Trailer`
de los sustituidos y el RED con `chain_base_sha`. Con VERIFIED, el Q7 fija `superseded_by`.

| Id | Condición | Comportamiento |
|---|---|---|
| P-12 | TERMINATION_UNACCREDITED u ORPHAN_CONFIRMED sin decisión | STOP; sin toma de posesión |
| P-13 | cambio de máquina sin acreditación | STOP |

### 16.27 Conteo y presupuestos de la ejecución delegada (I62)

Origen: Proposal V14 §9.3 y Anexo B.8.5. Los contadores del bucle de revisión están en 16.29.

| Contador | Evento que cuenta | Autoridad durable | Entre Q0 y Q7 | Sin diario |
|---|---|---|---|---|
| `attempts` | **corrección lanzada** (16.8) | `automation_state.attempts`, +1 en el Q0 de la corrección | — | durable |
| por clase (`TaskId`, `FailureClass`) | corrección lanzada de esa clase, no verificaciones | `counters.correction_launches`, entrada en el mismo Q0 | — | durable; dos verificaciones de la misma entrega no son dos correcciones; una corrección lanzada y caída antes de verificar sí cuenta |
| reejecuciones BLOCKED (`TaskId`, fase) | reejecución lanzada | `counters.blocked_reruns`, en Q7 | diario | la ventana abandonada cuenta como reejecución de la fase en que se perdió; nunca cero |
| recuperaciones de 16.7 | recuperación | `counters.rebase_recoveries`, en Q7 | diario | las que prueben los commits reescritos observables |
| invocaciones | **lanzamiento**, cada uno con `RunId`; un lanzamiento incierto cuenta como lanzado (`LAUNCH_UNCERTAIN`) | `counters.invocations`, en Q7 | diario | lanzados probados + inciertos por fase (16.26); P-07 compara lanzados + inciertos con el tope **antes** de lanzar |

Un rebinding conserva `TaskId` y los contadores. Una `TaskId` nueva solo por decisión del Coordinator; si continúa el mismo trabajo, declara
`ContinuesTaskId` y **hereda** los contadores. No se puede reiniciar la misma cadena ni la misma clase de fallo. Un contador ausente o contradictorio con
su evidencia es S-04, y una reconstrucción con valores menores que los probados es inválida.

### 16.28 Arranque, adopción y marcadores (I62)

Origen: Proposal V14 §8.6, §8.7 y §14.0, con A-1 (D1-12, D1-18, D1-20). Este es el texto literal de los marcadores que exige §8.6: la decisión del
Coordinator los copia literalmente y la entrada de `docs/automation/decisions/<unit>.md` los conserva.

**Aplicabilidad.** La ejecución delegada I62 sigue siendo opt-in (Proposal V14 §14.0). Una unidad **DIRECT_ONLY** usa `rackcad-automation-state/v1` (§8),
sin `protocol`, `custody`, `counters` ni `orchestration`, y trabaja sin la maquinaria delegada: ni contrato de gate, ni delegación, ni bindings, ni preflight
del Principal; su trabajo directo autorizado por WORKFLOW, LIFECYCLE y AGENTS no se ve afectado. Una unidad **I62_DELEGATED** usa
`rackcad-automation-state/v2` con el arranque siguiente. No hay transición inversa: una unidad I62_DELEGATED que deja de delegar no abre ventanas y sigue en
`/v2` con QU.

**Arranque (Modelo A: observación → propuesta → aceptación → referencia durable):**

| Paso | Quién | Artefacto | Aceptación |
|---|---|---|---|
| 1. Observación | la sesión que reclama | su `preflight/v1` con `Action` CUSTODY (TRANSIENT) | — |
| 2. Propuesta | la sesión | `binding/v1` del Principal: `Scope` UNIT, `Role` PRINCIPAL_COORDINATOR, `Acceptance.State` PENDING | PENDING |
| 3. **BOOTSTRAP** + push | la sesión | custodia de 1 y 2 en `docs/automation/evidence/<unit>-agent/bootstrap/` y estado `/v2` con `record_version` 1: `protocol.basis` con la evidencia, `g0_acceptance` PENDING, `principal.preflight` y `principal.binding` a esos blobs, `principal.acceptance` PENDING | PENDING / PENDING |
| 4. Revisión de G0 | Coordinator | decisión con los tres marcadores (abajo) | decidida |
| 5. Registro + **QU** + push | la sesión | en **un solo commit**: la entrada de decisiones con los marcadores, el `Claim-Id` y la `record_version` del BOOTSTRAP; la versión del binding con `Acceptance` decidida, `DecisionRef` = {ruta de decisiones, marcador} y `Utc`; y un QU con `g0_acceptance` y `principal.acceptance` transicionados y sus `StateRef` a esos blobs | ACCEPTED o REJECTED |

**Reglas:** sin referencias futuras ni propias (los `StateRef` apuntan a blobs del mismo commit; el estado nunca cita su propio blob ni su commit);
`protocol.basis.claim_commit` = el commit de reclamo si es anterior al BOOTSTRAP, o `null` cuando el reclamo es el BOOTSTRAP. Antes del paso 5, la
clasificación es PENDING_G0 (STOP P-15 de contratos y delegaciones), Q0 está prohibido (solo QU, QH y QR) y el trabajo no delegado de G0 sigue según
WORKFLOW. **La aceptación nunca se infiere:** un G0 GATE PASS sin los marcadores no transiciona nada. Clasificación REJECTED → UNKNOWN → STOP, remediable solo
por una decisión posterior del Coordinator (16.13). Binding del Principal REJECTED → sin Principal aceptado, Q0 sigue prohibido; se remedia con observación y
propuesta nuevas aceptadas en un QU, o con una transferencia (QR). `protocol.g0_acceptance` cambia una sola vez y `principal.acceptance` una vez por
titular; `protocol.set`, `protocol.effective_sha` y `protocol.basis` son inmutables desde el BOOTSTRAP.

**Adopción.** En G0, la decisión lleva `I62-DELEGATED-EXECUTION: I62_DELEGATED` con los otros dos marcadores (`protocol.basis.adoption_at` = G0); si es
DIRECT_ONLY, el commit que la registra vuelve a un `/v1` con los mismos nueve campos de `automation_state` (T21). **Posterior** (de DIRECT_ONLY a
I62_DELEGATED, T20), con fallo cerrado: (1) unidad posterior a `I62_EFFECTIVE_SHA` (una unidad ANTERIOR sigue en I61 toda su vida), estado `/v1` y ninguna
delegación abierta; (2) observación y propuesta como en los pasos 1-2; (3) BOOTSTRAP de adopción: `/v2` con `record_version` 1, `adoption_at` =
MID_INITIATIVE, las dos aceptaciones PENDING, los nueve campos copiados (incluidos `attempts` y `claim_id`) y los contadores de ventana vacíos (los commits
directos anteriores no son trabajo delegado); (4) decisión de adopción con los tres marcadores, que `g0_acceptance` registra aunque no sea la de G0; (5) QU
con las transiciones; solo entonces Q0; (6) rechazo DIRECT_ONLY → vuelta a `/v1` (T21).

**Marcadores.** Cada decisión va en un bloque cercado de `docs/automation/decisions/<unit>.md`. La línea del marcador es exacta y los demás campos son líneas
`Clave: valor`:

```text
I62-DELEGATED-EXECUTION: I62_DELEGATED | DIRECT_ONLY
I62-CLASSIFICATION: I62 | REJECTED
I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED | REJECTED
Claim-Id: <Claim-Id de la unidad>
BootstrapRecordVersion: <record_version del BOOTSTRAP>
```

- **Titular nuevo** (QR, T12a): `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED` con `Claim-Id`, en la designación del Coordinator.
- **Toma con rebase** (16.25): `I62-REBASE-TAKEOVER: <BindingId>` junto con `I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED` y, en T12b, la referencia a la
  terminación acreditada del titular anterior.
- **Autorización del bucle del Architect** (16.29; materialización: 16.20):

```text
I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>
Role: ARCHITECT
ContinuesLoopInstanceId: null | <loop.instance_id del bucle que sustituye, enmienda o continúa>
ObjectFamily: <unidad y patrón de rutas>
CorrectionScope: <alcance de corrección permitido y cierres que no se reabren>
Budget.<tope>: <entero no mayor que el congelado; un tope ausente no rebaja el congelado>
Validity.Until: <instante>
Claim-Id: <Claim-Id de la unidad>
```

  Los criterios de materialización (`AuthorizedActions`, `MinimumCapabilities`, `RequiredIndependence`, `EligibleCells`, `ModelEffortBounds`, `Permissions`)
  y las exenciones de opción B van en el mismo bloque, con las claves de 16.20. Los nombres de `Budget.<tope>` son los de `architect_budgets[].caps`:
  `review_rounds`, `logical_requests`, `transport_reruns_per_request`, `corrections_per_lineage`, `correction_rounds` y `architect_launches`.
- **Cierre de un bucle del Architect** salvo desde ARCHITECT_SATISFIED: `I62-REVIEW-LOOP-CLOSE: <LoopInstanceId>`; si la vigencia seguía abierta, la misma
  decisión lleva `I62-REVIEW-LOOP-REVOCATION: <AuthorizationId>`.
- **Cierre de un bucle REVIEWER por vigencia terminada:** `I62-REVIEWER-LOOP-CLOSE: <LogicalReviewRequestId de la última solicitud del bucle>`; con la vigencia
  abierta, la misma decisión lleva `I62-REVIEW-LOOP-REVOCATION: <authorization_id>` con la identidad `<path>@<blob>#REVIEWER` (16.29).
- **Sustitución de una autoridad REVIEWER:** `I62-REVIEWER-AUTHORITY-SUPERSEDED: <authorization_id>`.

### 16.29 Orquestación de roles (I62): bucles de revisión, intentos y presupuestos

Origen: Proposal V14 §20.1, §20.2, §20.4-§20.6, §20.8-§20.11 y Anexo B.8.8, con A-1 (D1-1..D1-21, D2-2, D2-5, D2-6, D2-12). La invocación, los contratos de
salida, el cierre de insumos y la fidelidad están en 16.23 y 16.24; la materialización autorizada, en 16.20. Solo para unidades I62_DELEGATED.
Procedimiento: [agent-execution/README](automation/agent-execution/README.md) §18.

**Relevo frente a escalada.** Un RELAY (mover un artefacto de un rol a otro dentro de la autoridad vigente) lo hace el Principal sin intervención del Owner;
una ESCALATION (decisión del Owner o del Coordinator) se registra con `orchestration.escalation` = OWNER o COORDINATOR y la decisión exacta requerida. **El
Principal orquesta; la autoridad no cambia:** no declara AGREED, no cierra ni rebaja un REQUIRED, no elige una decisión del Owner, no declara Freeze ni PASS,
no materializa fuera de los criterios de la autorización y no usa un resultado de REVIEWER para satisfacer al ARCHITECT (P-20).

**Siguiente acción.** El estado `/v2` lleva `orchestration.next_action` estructurado (rol, acción, `target`, unidad, gate, tarea, insumos, capacidades,
independencia, `invocation_permission`, `budget_remaining`, `expected_output`, condiciones). **Es una función del estado y de los artefactos custodiados y da
una sola acción:** la escalada nombra el rol que decide; CORRECTING es PRINCIPAL / CORRECT_AND_REREVIEW sobre `loop.object`; una fase pendiente cuya
solicitud OPEN tiene un intento en INVOCATION_PLANNED o BUDGET_RESERVED, con la vigencia abierta, invoca al revisor del bucle (ARCHITECT o REVIEWER) sobre
el objeto de esa solicitud, con la autorización vigente como `invocation_permission` y el contrato de salida de su rol (16.23). Si el estado no la determina
de forma única: STOP (P-17). Ningún hecho de continuación vive solo en un chat, una memoria privada o un prompt copiado a mano.

**Bucle del Architect.** Se ejecuta bajo una `ReviewLoopAuthorization` del Coordinator (16.28), custodiada como `StateRef`. Fases (`loop.phase`):

```text
REVIEW_PENDING → ARCHITECT_INVOKED → RESULT_INGESTED → { AGREED            → ARCHITECT_SATISFIED(objeto exacto) → siguiente gate / decisión del Owner
                                                        { CHANGES_REQUIRED  → CORRECTING → PUBLISHED → CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED …
                                                        { BLOCKED_OWNER     → ESCALATE_OWNER (decisión exacta requerida)
                                                        { INVALID           → reejecución de transporte dentro del tope; agotado → STOP (P-19)
```

Cada fase se publica en un QU ORDINARY; la ingestión publica directamente la fase siguiente según el veredicto. **`loop.object` cambia solo en CORRECTING →
PUBLISHED** (y su `commit` pasa a la imagen en una reconciliación, 16.25); se fija al abrir el bucle (NONE → REVIEW_PENDING) y pasa a `null` solo en
LOOP_CLOSED. La versión corregida se publica en un commit propio para que su CI corra sobre él. ARCHITECT_SATISFIED exige el último intento ingerido VALID
con un `architect-review-result/v1` AGREED sobre el blob de `loop.object` y ningún linaje REQUIRED en OPEN o STILL_OPEN.

**Identidad del bucle y presupuestos por instancia.** `loop.instance_id` = `ARL-<record_version del QU que abre el bucle>`, inmutable con el bucle abierto.
`orchestration.architect_budgets[]` lleva una entrada append-only por instancia (`authorizations[]`, contadores, `caps` y cierre): toda solicitud del bucle
lleva su `loop_instance_id` y cuenta en su entrada, nunca en `budgets`. Los topes efectivos son el mínimo componente a componente de los congelados y del
`Budget.*` de cada autorización que ha gobernado el bucle; nunca suben sin A-n. Una sustitución, enmienda o continuación (`ContinuesLoopInstanceId` = el
`loop.instance_id`) no crea entrada ni reinicia contadores; el registro anterior queda ENDED con SUPERSEDED. **LOOP_CLOSED** (cualquier fase → NONE, en un
QU ORDINARY) exige ninguna solicitud OPEN, ningún intento no terminal, la escalada resuelta y la vigencia terminada (ARCHITECT_SATISFIED, EXHAUSTED, EXPIRED o
REVOKED), o revocada por la propia decisión de cierre con `I62-REVIEW-LOOP-CLOSE`. Un bucle nuevo exige una autorización nueva con `ContinuesLoopInstanceId`
= `null` y empieza su entrada en cero; lo consumido se conserva.

| Tope congelado | Valor |
|---|---|
| `review_rounds` (versiones revisadas por bucle) | 3 |
| `logical_requests` | 3 |
| `transport_reruns_per_request` | 2 |
| `corrections_per_lineage` | 2 |
| `correction_rounds` | 2 |
| `architect_launches` | 9 |

**Intentos.** Una solicitud lógica (`LogicalReviewRequestId`) tiene intentos (`attempt_seq`, cada uno con su `InvocationId` y, desde el lanzamiento, su
`RunId`). Estados: INVOCATION_PLANNED → BUDGET_RESERVED → LAUNCHING → LAUNCHED → RESULT_RECEIVED → RESULT_INGESTED, más LAUNCH_UNCERTAIN y
CANCELLED_BEFORE_LAUNCH. La reserva se publica **antes** de lanzar, con `BudgetSnapshot` (los contadores de su entrada de `architect_budgets[]`, o de `budgets`
para el REVIEWER) y `OpenFindings` (para el ARCHITECT, todo linaje de la unidad con `issuer` ARCHITECT o COORDINATOR en OPEN o STILL_OPEN; para el REVIEWER,
los de `issuer` REVIEWER; incluidos los heredados de bucles anteriores). Una caída tras BUDGET_RESERVED se reanuda sin lanzamiento nuevo; tras LAUNCHING
decide la evidencia del invocador (operación 7 ligada al `RunId`): arrancó → LAUNCHED o RESULT_RECEIVED con su invocación original; no arrancó, con la
vigencia abierta → BUDGET_RESERVED con una invocación nueva reconstruida sobre las imágenes, sin consumir otro lanzamiento; indeterminado → LAUNCH_UNCERTAIN
con conteo conservador. **Un intento en LAUNCHING o posterior nunca cambia su invocación en sitio.** Un intento nuevo que superaría un tope es P-18 antes de
reservar. Cambiar de proveedor, modelo, sesión, binding, Principal o etiqueta no reinicia ningún contador.

**Ingestión y linaje.** Cada hallazgo REQUIRED del Architect abre un linaje (`orchestration.findings[]`) que solo cierra o rebaja un resultado con
autoridad para él: un `architect-review-result/v1` de un binding ARCHITECT del bucle cuyo `EvaluatedObject` es **equivalente** (EquivalentReviewedObject,
16.25) al objeto de su solicitud, o una decisión del Coordinator para un linaje COORDINATOR; nunca un `reviewer-result/v1`. Una omisión no cierra; AGREED con
un REQUIRED abierto omitido es INVALID; un resultado cuyo esquema no es el `OutputContract` de su invocación es INVALID (P-19).

**Bucle REVIEWER.** Se abre bajo la autoridad del contrato de gate (`RoleRequirements[].Materialization`), con la identidad
`<path>@<blob>#REVIEWER`, sin `loop.instance_id`; sus solicitudes llevan `loop_instance_id` = `null` y cuentan en `budgets`. Su `loop.object` cambia en
CORRECTING → PUBLISHED y la re-revisión se abre sobre el objeto corregido. **REVIEWER_SATISFIED** se publica en el QU que ingiere el resultado, solo si la
última solicitud del bucle tiene un `reviewer-result/v1` ingerido VALID, ningún intento del bucle es no terminal, ningún linaje BLOCKING con `issuer`
REVIEWER de la unidad queda en OPEN o STILL_OPEN y el requisito operativo exacto del contrato está satisfecho; la vigencia termina en ese punto.
ARCHITECT_SATISFIED es solo del bucle del Architect y REVIEWER_SATISFIED solo del REVIEWER; NO_FINDINGS no es ARCHITECT_SATISFIED. **LOOP_CLOSED de
REVIEWER** (→ NONE en un QU ORDINARY): (S) desde REVIEWER_SATISFIED, sin decisión; o (E) con la vigencia terminada por EXHAUSTED, EXPIRED o REVOKED y la
decisión `I62-REVIEWER-LOOP-CLOSE` (con la vigencia abierta, la misma decisión la revoca). Cada cierre añade un registro a
`orchestration.reviewer_closures[]` (append-only), del que se lee el fin histórico. Una autoridad REVIEWER terminada o sustituida
(`I62-REVIEWER-AUTHORITY-SUPERSEDED`) nunca vuelve a abrir un bucle.

**Portabilidad.** Otro Principal, solo con el estado custodiado y los artefactos de la rama, reconstruye el bucle: el objeto, el resultado, los linajes
abiertos, el presupuesto y la siguiente acción. **Respaldo manual:** un relevo hecho a mano (el Owner como transporte) se registra como AUTONOMY_GAP en
`orchestration.autonomy_gaps[]`; sin ese registro, la evidencia de orquestación no vale para el criterio 15 (P-21). **Nivel A:** todo esto son textos,
esquemas, validadores deterministas y controles reproducibles; ningún servicio. I-62 no se usa como autoridad para desarrollarse a sí misma.

| Id | Condición | Comportamiento |
|---|---|---|
| P-17 | `NextAction` no derivable de forma única desde el estado canónico | STOP por ambigüedad material |
| P-18 | presupuesto del bucle de revisión agotado, o un intento nuevo lo superaría | STOP y escalada, antes de reservar o lanzar |
| P-21 | relevo manual sin registro AUTONOMY_GAP | la evidencia de orquestación no vale para el criterio 15 |

### 16.30 Planos y MaterializationClose (I62)

Origen: Proposal V14 §15.

| Plano | Qué incluye | Autoridad | Nunca puede |
|---|---|---|---|
| (a) real | la sesión de I-62 y la gobernanza | I-61 y el Workflow vigentes | usar reglas de I-62 sin integrar como autoridad |
| (b) materializado inactivo | las partes I62 de este plan y de los procedimientos subordinados, con la cláusula de vigencia de 16.14 | ninguna hasta la vigencia | gobernar operaciones reales |
| (c) sistema bajo prueba | un repositorio fixture con su propia activación de prueba (merge marcado `TEST-ACTIVATION`) | las reglas de (b) activadas dentro del fixture | acreditar GATE PASS, READY, aprobación del Owner o integración reales; cambiar `main`, contadores, decisiones o custodia reales (P-16) |

**MaterializationClose** (generaliza el cierre de G2 de 16.3 para las unidades I62): para una unidad con cambios normativos propios, el commit que cierra el
gate con el **último** cambio a superficies normativas o de plantilla, revisado por el Coordinator contra el Freeze; para una unidad sin cambios normativos,
el commit de bootstrap o el último de `UNIT_DOC` aceptado por el Coordinator. `AuthorityRevision` sigue 16.3 con ese hito. Un cambio posterior a esas
superficies invalida el cierre: se cierra de nuevo y se repiten las pruebas afectadas.

| Id | Condición | Comportamiento |
|---|---|---|
| P-16 | un resultado del plano (c) intenta actuar sobre el plano (a) | rechazo, sin efecto real |
