# Plan del ejecutor nocturno

Este documento es la fuente principal para las ejecuciones automatizadas de iniciativas de
RackCad. El ejecutor prepara trabajo revisable y publicado; nunca integra cambios. `ROADMAP.md`
continua siendo el indice global, `WORKFLOW.md` define el proceso Git y cada archivo en
`docs/initiatives/` define el contrato detallado de una iniciativa.

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
`origin/docs/reestructura`, que el Pull Request de I-06 sigue abierto y que el worktree registrado no
esta ocupado por otra sesion. Este modo solo puede reanudar I-06 en su rama, Pull Request y worktree
existentes. No ejecuta el algoritmo de seleccion, no reclama otra iniciativa y no crea otra rama,
worktree o Pull Request.

El modo bootstrap termina cuando el sistema documental queda integrado en `origin/main`. No se
activa para otras ramas ni permite que una rama sustituya las reglas globales del ejecutor.

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
- Respetar `AGENTS.md`, `WORKFLOW.md`, las dependencias del repositorio y el contrato de la
  iniciativa. Si se contradicen, se aplica la precedencia de `WORKFLOW.md`.

## 4. Estado derivado

El estado operativo se recalcula al inicio y antes de publicar. No se controla solo con checkboxes
ni con el campo `status` del front matter.

| Evidencia observada | Estado derivado |
|---|---|
| Los cambios de la iniciativa estan contenidos en `origin/main` y ROADMAP la marca integrada | terminada |
| Existe `origin/<rama>` y no hay Pull Request abierto | reclamada o en implementacion |
| Existe un Pull Request abierto | en revision, CI o validacion |
| El Pull Request declara que espera AutoCAD, build local o una decision | bloqueada para integrar |
| No hay rama ni Pull Request, las dependencias estan integradas y no hay conflictos activos | candidata elegible |
| Falta contrato detallado, una dependencia o una entrada humana previa | no elegible |

Una rama remota cuenta como iniciativa activa aunque no tenga Pull Request. Un Pull Request que
espera AutoCAD sigue activo, pero no bloquea por si mismo iniciativas compatibles. Las ramas ya
integradas no cuentan como activas aunque aun no se hayan limpiado.

## 5. Reanudacion antes de seleccionar trabajo nuevo

En cada ejecucion el agente:

1. Ejecuta `git fetch origin --tags --prune` y deriva primero las iniciativas activas desde ramas
   remotas y Pull Requests. Valida su bloque `automation_state` contra la rama, el Claim-Id y el
   ultimo commit publicado.
2. Busca una iniciativa activa que pueda continuar sin intervencion humana. Una iniciativa con
   `gate: owner-decision`, `owner-validation`, `autocad` o `plugin-build-unavailable` no es
   reanudable hasta que exista evidencia posterior a `last_evidence_commit` de que el gate fue
   resuelto.
3. Reutiliza la rama, el Pull Request y el worktree registrados de la iniciativa reanudable. Si el
   worktree no existe, esta ocupado o no coincide con la rama, se detiene: nunca crea un reemplazo
   silencioso para una iniciativa activa.
4. Ejecuta como maximo una fase coherente del contrato o una correccion de CI, actualiza evidencia,
   commit, push y el mismo Pull Request, y termina la ejecucion.
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
Claim-Id e iniciativa. Si ya existe uno, se actualiza; nunca se abre un segundo PR para la misma
iniciativa. Si el PR previo esta cerrado sin integrar, se detiene para decision del dueno.

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

## 8. Implementacion, commits y Pull Requests

El agente implementa solo las fases y archivos autorizados por el contrato. Al reanudar una rama,
primero compara con `origin/main` y hace el rebase requerido por `WORKFLOW.md`. Cada unidad coherente
lleva commit con asunto en espanol, explicacion del por que y trailer `Co-Authored-By`.

Tras el primer cambio sustantivo publica la rama y abre un Pull Request draft contra `main`. Si ya
existe, lo actualiza en lugar de abrir otro. El cuerpo conserva:

- Initiative, Branch y Claim-Id;
- alcance realizado y pendiente;
- validaciones ejecutadas y sus resultados;
- CI, build local, AutoCAD y decisiones pendientes;
- intentos consumidos;
- hallazgos fuera de alcance;
- confirmacion de que no se hizo ni se hara merge automatico.

Ademas, todo Pull Request gestionado por el ejecutor contiene exactamente un bloque YAML
`automation_state` con esta forma:

```yaml
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

El bloque se actualiza despues de cada fase, correccion de CI o cambio de gate. Sus campos siguen
estas reglas:

- `initiative`, `branch` y `claim_id` son inmutables y deben coincidir con el reclamo remoto.
- `current_phase` identifica una fase del contrato o una correccion acotada, sin texto multilinea.
- `state` usa uno de `claimed`, `in-progress`, `ci-failed`, `waiting`, `ready-for-owner` o
  `ready-for-integration`.
- `gate` usa `none`, `owner-decision`, `owner-validation`, `autocad` o
  `plugin-build-unavailable`.
- `attempts` es un entero no negativo para la fase o fallo actual; aumenta con cada reintento y
  vuelve a cero solo al avanzar de fase o resolver el fallo.
- `next_action` describe una sola accion siguiente, en una linea.
- `last_evidence_commit` es el SHA completo del commit publicado que respalda el estado.

El bloque es estado operativo estructurado, no reemplaza la evidencia real de `origin/main`, ramas,
commits, checks y revisiones. Un gate del dueno solo se considera resuelto con comentario o revision
explicita posterior a `last_evidence_commit`; AutoCAD debe identificar el commit/DLL validado y el
build local debe registrar el commit y resultado. En la siguiente reanudacion el agente verifica esa
evidencia antes de cambiar `gate` a `none`.

## 9. CI fallido y reintentos

Ante CI fallido se inspeccionan el check y sus logs antes de editar. Un intento es una secuencia de
diagnostico, correccion acotada, validacion local, commit y push. No se repite el mismo cambio sin
nueva evidencia. El maximo por defecto es `automation.max_attempts` del contrato, inicialmente tres.

Se detiene antes del limite si el fallo es externo, requiere secretos/permisos, depende de AutoCAD,
exige ampliar alcance o no puede reproducirse con la evidencia disponible. Al agotar intentos, el PR
queda draft con el ultimo fallo, enlaces a checks y una peticion concreta al dueno.

## 10. Build local y AutoCAD

Si `requires_plugin_build: true`, el ejecutor intenta el build Debug local solo cuando la estacion
tiene las referencias necesarias y AutoCAD no bloquea los DLL. Si no es posible, publica el trabajo
con estado `esperando build Plugin local`; no declara exito ni integra.

Si `requires_autocad: true`, completa primero las validaciones automatizables y entrega al dueno la
ruta exacta del DLL del worktree y un checklist basado en el contrato. El PR queda esperando
AutoCAD. Esa espera bloquea la integracion de la iniciativa, no el inicio de otra compatible,
siempre que el total activo permanezca por debajo de dos.

## 11. Decisiones del dueno

Cuando `requires_owner_decision: true`, el contrato debe identificar la decision y el punto en que
se necesita. El agente puede recopilar evidencia y plantear opciones, pero se detiene antes de tomar
la decision o implementar una opcion no autorizada. Los ADR nuevos permanecen `propuesto` hasta que
el dueno los acepte o rechace.

`requires_owner_validation: true` exige una confirmacion explicita del dueno antes de considerar la
iniciativa lista para integracion, incluso si CI esta verde.

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
6. Pull Request, estado draft y enlace.
7. Pruebas, builds y checks de CI con resultado verificable.
8. Intentos consumidos y fallos pendientes.
9. Validacion AutoCAD, build local o decisiones requeridas del dueno.
10. Hallazgos fuera de alcance y siguiente accion recomendada.
11. Confirmacion de que `main` no fue modificada y no hubo merge automatico.

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
