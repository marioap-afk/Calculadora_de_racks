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

## 2. Limites de seguridad

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

## 3. Estado derivado

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

## 4. Seleccion de la siguiente iniciativa

En cada ejecucion el agente:

1. Ejecuta `git fetch origin --tags --prune` y obtiene la punta de `origin/main`, ramas remotas,
   worktrees y Pull Requests abiertos.
2. Lee desde `origin/main` `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, este documento y los
   contratos detallados disponibles.
3. Deriva el estado de cada iniciativa usando `main`, ramas y Pull Requests; el estado remoto gana
   sobre pistas estaticas del front matter.
4. Excluye iniciativas integradas, condicionales no activadas, deshabilitadas, sin plan detallado,
   con dependencias pendientes, con conflictos activos, con prerrequisitos del dueno pendientes o
   con intentos agotados.
5. Cuenta las iniciativas activas. El limite inicial es dos. Si ya hay dos, no reclama otra.
6. Ordena las elegibles por `priority` numerica ascendente. Un empate se resuelve por el orden del
   ROADMAP y despues por ID. Una prioridad no asignada no se inventa: la iniciativa queda no
   elegible para seleccion automatica.
7. Selecciona como maximo una iniciativa nueva. Si ninguna cumple, emite el informe y termina.

El limite de dos iniciativas activas y el maximo de una nueva por ejecucion son compuertas
independientes. El ejecutor puede reanudar trabajo ya reclamado sin convertirlo en una segunda
iniciativa nueva de esa misma ejecucion.

## 5. Dependencias y conflictos

Una dependencia esta satisfecha solo cuando esta integrada en `origin/main`; una rama terminada o
un Pull Request aprobado no bastan. Un conflicto esta activo cuando la iniciativa conflictiva tiene
rama remota no integrada o Pull Request abierto. Tambien se respetan conflictos textuales del
ROADMAP, como trabajo simultaneo sobre un subsistema o toda una fase.

Antes de reclamar se vuelve a consultar el remoto. Si el estado cambio desde la seleccion, se
recalcula la elegibilidad en lugar de continuar con datos obsoletos.

## 6. Reclamo atomico y worktree

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

## 7. Implementacion, commits y Pull Requests

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

## 8. CI fallido y reintentos

Ante CI fallido se inspeccionan el check y sus logs antes de editar. Un intento es una secuencia de
diagnostico, correccion acotada, validacion local, commit y push. No se repite el mismo cambio sin
nueva evidencia. El maximo por defecto es `automation.max_attempts` del contrato, inicialmente tres.

Se detiene antes del limite si el fallo es externo, requiere secretos/permisos, depende de AutoCAD,
exige ampliar alcance o no puede reproducirse con la evidencia disponible. Al agotar intentos, el PR
queda draft con el ultimo fallo, enlaces a checks y una peticion concreta al dueno.

## 9. Build local y AutoCAD

Si `requires_plugin_build: true`, el ejecutor intenta el build Debug local solo cuando la estacion
tiene las referencias necesarias y AutoCAD no bloquea los DLL. Si no es posible, publica el trabajo
con estado `esperando build Plugin local`; no declara exito ni integra.

Si `requires_autocad: true`, completa primero las validaciones automatizables y entrega al dueno la
ruta exacta del DLL del worktree y un checklist basado en el contrato. El PR queda esperando
AutoCAD. Esa espera bloquea la integracion de la iniciativa, no el inicio de otra compatible,
siempre que el total activo permanezca por debajo de dos.

## 10. Decisiones del dueno

Cuando `requires_owner_decision: true`, el contrato debe identificar la decision y el punto en que
se necesita. El agente puede recopilar evidencia y plantear opciones, pero se detiene antes de tomar
la decision o implementar una opcion no autorizada. Los ADR nuevos permanecen `propuesto` hasta que
el dueno los acepte o rechace.

`requires_owner_validation: true` exige una confirmacion explicita del dueno antes de considerar la
iniciativa lista para integracion, incluso si CI esta verde.

## 11. Condiciones obligatorias para detenerse

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

## 12. Prohibicion de merge automatico

El ejecutor nunca ejecuta `merge`, `gh pr merge`, auto-merge, squash, rebase-and-merge ni una API
equivalente. Tampoco actualiza ROADMAP o HANDOFF como si la iniciativa estuviera integrada. La
integracion es una sesion separada, serializada y autorizada por el dueno conforme a `WORKFLOW.md`.

## 13. Informe nocturno

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

## 14. Registro resumido de iniciativas

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
