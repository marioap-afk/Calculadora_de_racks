TURNO NOCTURNO AUTÓNOMO — RACKCAD I-64
Vas a trabajar de forma autónoma durante toda esta sesión.
El Owner estará ausente y no responderá hasta mañana.
Esta instrucción constituye AUTORIZACIÓN PERMANENTE para que tomes las decisiones necesarias, ejecutes el trabajo, resuelvas gates, corrijas problemas, hagas commits, pushes, CI, verificaciones, controles y cierres, sin pedir autorización intermedia al usuario.
1. OBJETIVO
Avanza I-64 todo lo que sea razonablemente posible durante esta sesión.
Objetivo máximo:

* reconciliar el estado actual de I-64 con `main`;
* ejecutar y cerrar F2-T2B;
* ejecutar Smoke-NAV cuando sea técnicamente posible;
* continuar con las siguientes fases restantes de I-64;
* completar toda la iniciativa I-64 si el entorno y la evidencia lo permiten;
* dejar el repositorio en el estado más avanzado, limpio, verificable y recuperable posible.

No te detengas simplemente porque aparezca un gate que normalmente pediría confirmación del Owner o Coordinator.
Resuélvelo tú usando la evidencia disponible y continúa.
2. ESTADO DE PARTIDA
Repositorio:
`marioap-afk/Calculadora_de_racks`
Rama I-64:
`architecture/workspace-persistente-rackcad`
Tip confirmado al inicio:
`a991ae04eb511aa21dbcc6585647656908841cb4`
`origin/main` confirmado:
`812baf879612320653094a8aaf46aca0bac81383`
Estado conocido:

* F0: CLOSED / PASS
* F1: CLOSED / PASS
* F2-T1-NAVIGATION-CORE: CLOSED / PASS
* F2-T2A-NAVIGATION-HOST: CLOSED / PASS
* F2-T2B-NAVIGATION-UI: NOT STARTED
* Smoke-NAV: PENDING

Producto verificado de F2-T2A:
`44a692a4ec386e559cc6d330b3d78b87dbad6581`
La rama avanzó posteriormente por documentación/evidencia hasta:
`a991ae04...`
Ese avance no sustituye la identidad del producto verificado de T2A.
3. AUTORIDAD PARA ESTA SESIÓN
Trata este prompt como una autorización explícita del Owner para tomar decisiones durante toda la noche.
No vuelvas a detenerte con mensajes del tipo:

* OWNER_DECISION_REQUIRED
* COORDINATOR_DECISION_REQUIRED
* ACCEPTANCE_REQUIRED
* CLOSE_REQUIRED
* AUTHORIZATION_REQUIRED

si el asunto puede resolverse razonablemente con la evidencia del repositorio, las reglas existentes y juicio técnico conservador.
En esos casos:

1. analiza;
2. registra la decisión;
3. custodia la evidencia cuando corresponda;
4. continúa.

No necesito aprobar cada transición.
4. NO DIVIDIR EL TRABAJO EN GATES PARA EL USUARIO
Los gates técnicos siguen existiendo internamente.
Debes seguir haciendo:

* CI;
* tests;
* A1-A8;
* verificaciones;
* controles negativos;
* source guards;
* revisiones independientes;
* Smoke;
* comprobaciones de identidad;
* comprobaciones de scope;
* pruebas de ramas;
* custodia.

Pero NO debes convertir cada uno en una parada esperando al usuario.
Ejecuta:
gate -> resultado -> decisión -> siguiente paso
de forma autónoma.
Solo detente definitivamente cuando exista un bloqueo real que no pueda resolverse de manera segura o verificable.
5. PRIMER PASO: RECONCILIAR MAIN
`main` avanzó después del cierre de F2-T2A.
Antes de contratar o implementar F2-T2B:

1. fetch / remeasure;
2. mide otra vez `origin/main`;
3. revisa qué commits entraron desde el MainSha anterior;
4. compara contra:
   * autoridades;
   * Freeze;
   * contratos;
   * candidate files;
   * tests;
   * source guards;
   * UI;
   * Workspace;
   * AutoCAD Plugin;
5. decide la ruta correcta conforme a WORKFLOW.

No reutilices el frame histórico de controles de T2A.
El espejo histórico de T2A quedó cerrado y NO es una herramienta normal para T2B.
Si procede rebase/merge/sincronización según las reglas vigentes, ejecútalo tú.
No me preguntes cuál prefiero.
Elige la ruta que preserve:

* historia recuperable;
* evidencia;
* CI;
* Freeze;
* contratos;
* mínimo riesgo.

6. F2-T2B — OBJETIVO DE PRODUCTO
F2-T2B es la capa UI de navegación.
No diseñes la UI suponiendo únicamente una barra lateral angosta.
Objetivo final de RackCad:
una única Workspace modeless integrada con AutoCAD que pueda:

* estar docked;
* flotar;
* redimensionarse;
* ocupar una parte grande o prácticamente toda una segunda pantalla;
* coexistir simultáneamente con AutoCAD;
* servir como superficie primaria de RackCad.

La experiencia debe poder evolucionar desde una vista estrecha hasta una superficie amplia.
No conviertas las ventanas legacy en una segunda UX permanente.
Son compatibilidad/migración, no el destino final.
Para T2B:

* implementa solamente el alcance contractual de UI necesario;
* no hagas todavía un rediseño visual completo si no corresponde;
* no introduzcas deuda que impida luego la UI responsiva/wide;
* usa la autoridad `NavigationTargets` existente;
* no dupliques inventario;
* no inventes otra autoridad de navegación.

7. D-05 / D-07 SIGUEN VIGENTES
Mantén las decisiones arquitectónicas ya aceptadas.
Entre otras:

* `NavigationTargets` es autoridad única por `(session, RackId)`;
* solo miembros realmente atribuibles son navegables;
* probe-only Id nunca vuelve navegable un miembro;
* cache on-demand;
* invalidación por hints;
* Select / View / Center / Locate explícitos;
* command context de AutoCAD;
* no authored writes por navegación;
* no cambio automático de layout;
* View con margen;
* Center conserva escala;
* Locate usa referencia de la vista elegida;
* content-based echo;
* focus vuelve al dibujo;
* INV-40 revalida inmediatamente antes de la acción.

`ProbedId = null` sigue siendo aceptable únicamente como limitación diagnóstica.
Nunca debe convertir un miembro ilegible en navegable.
8. PROTOCOLO Y DECISIONES AUTÓNOMAS
Si una delegación pasa objetivamente sus gates:
acéptala y continúa.
Si un Worker termina correctamente:
verifica y continúa.
Si un intento falla por un defecto claro del producto:
corrige dentro del presupuesto permitido.
Si falla por un defecto del Controller/oráculo/protocolo y no del producto:
no gastes intentos del producto inútilmente.
Puedes crear una solución initiative-local equivalente a los bridges anteriores SIEMPRE que:

* sea necesaria;
* sea mínima;
* sea fail-closed;
* quede versionada;
* tenga pruebas adversariales;
* tenga revisión independiente cuando corresponda;
* no modifique globalmente el protocolo;
* no esconda evidencia real;
* no convierta un fallo en PASS por decreto.

No te autoriza a simplemente saltarte una comprobación.
Te autoriza a resolver correctamente el defecto y continuar.
9. OWNER / COORDINATOR DURANTE ESTA NOCHE
Para decisiones exclusivamente de I-64 que normalmente requieran Owner o Coordinator:
tú puedes tomar la decisión usando juicio técnico conservador y registrarla como:
`OVERNIGHT_OWNER_COORDINATOR_DISPOSITION`
con:

* problema;
* opciones;
* decisión;
* fundamento;
* evidencia;
* riesgos;
* alcance;
* por qué no cambia semántica global.

No necesitas pedirme texto literal.
No inventes aprobación humana para cambios globales fuera de I-64.
10. INTENTOS
No reinicies presupuestos silenciosamente.
Mantén attempts y budgets reales.
Puedes usar los intentos disponibles cuando corresponda.
Si un presupuesto se agota:

* determina primero si el problema es producto o infraestructura/protocolo;
* si es producto y ya no existe camino autorizado, detente;
* si es protocolo/control y existe una solución initiative-local legítima, puedes diseñarla y validarla sin esperar al usuario.

No marques nada PASS solo para seguir avanzando.
11. CONFIG / CODEX
Snapshot activo aceptado:
Snapshot E
SHA-256:
`2EEDDAAB28C1147A5D927B4EF1E4C69229197E04CB6B114161FFFF04A16588C8`
Binario Controller acreditado:
SHA-256:
`4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17`
Celda acreditada:
`gpt-6-luna / high / read-only`
Si config o binario vuelven a cambiar:
NO continúes como si nada.
Haz una reacreditación equivalente y continúa si la evidencia efectiva demuestra compatibilidad.
No necesito aprobarla manualmente esta noche si:

* no hay reducción de sandbox;
* no hay ampliación inesperada de permisos;
* modelo/effort/routing son correctos;
* no hay secretos expuestos;
* las sondas son satisfactorias.

Registra claramente cualquier cambio.
12. COMMITS Y GIT
Estás autorizado a:

* crear ramas de trabajo necesarias;
* crear commits;
* push;
* fast-forward;
* merge cuando WORKFLOW lo permita;
* rebase cuando WORKFLOW lo requiera y sea seguro;
* borrar ramas temporales una vez contenidas y verificadas;
* custodiar evidencia;
* versionar análisis;
* actualizar estado.

No estás autorizado a:

* force-push;
* reescribir historia publicada sin necesidad excepcional demostrable;
* borrar trabajo ajeno;
* resetear destructivamente ramas compartidas;
* ocultar movimientos reales de refs;
* alterar evidencia histórica;
* cambiar `main` directamente salvo que el workflow de integración de la iniciativa lo requiera expresamente y todas sus condiciones estén satisfechas.

Mantén la rama recuperable en todo momento.
13. OTRAS SESIONES
Hay otras iniciativas activas.
No interfieras con:

* ramas ajenas;
* refs ajenas;
* worktrees ajenos;
* procesos claramente pertenecientes a otra sesión.

Cambios de otras ramas no son automáticamente participantes de I-64.
Si otra iniciativa mueve `main`:
remeasure y decide conforme al workflow.
No simules un estado viejo modificando refs reales.
14. SMOKE-NAV
Cuando T2B esté técnicamente lista, prepara y ejecuta Smoke-NAV si el entorno permite interacción real con AutoCAD.
Debe cubrir al menos el conjunto ya establecido:

* SN-01..SN-10;
* selección;
* supervivencia/consistencia de selección;
* layout no disponible sin alterar TILEMODE;
* View;
* Center;
* Locate;
* echo;
* focus al dibujo;
* seguridad al cambiar de documento;
* coste/cache;
* mismos NavigationTargets para contexto/tabs cuando aplique.

No declares Smoke PASS basándote únicamente en unit tests.
Si AutoCAD real o interacción UI no están disponibles:
NO inventes el resultado.
En ese caso:

* completa absolutamente todo lo automatizable;
* deja Smoke-NAV listo;
* genera procedimiento exacto;
* deja builds y binarios preparados;
* identifica el único bloqueo humano/GUI pendiente.

15. DESPUÉS DE F2
No te detengas automáticamente al terminar F2.
Lee el Freeze, proposal, lifecycle, state y documentación vigente de I-64 y determina qué fases siguen.
Continúa por las siguientes fases en orden.
Para cada fase:

* contrato;
* planificación;
* implementación;
* tests;
* verificación;
* controles;
* Smoke si corresponde;
* custodia;
* cierre.

Hazlo autónomamente.
Objetivo: terminar I-64 si es técnicamente posible durante esta sesión.
16. PRODUCTO SOBRE PROTOCOLO
No dejes que el protocolo se convierta en trabajo circular sin valor.
El protocolo existe para demostrar que el producto es correcto.
Si detectas ciclos de:

* Controller defectuoso;
* oráculo imposible;
* identidad histórica;
* infraestructura;
* ruido de procesos;
* cambios externos de `main`;

resuélvelos de la forma más simple que preserve evidencia y seguridad.
Evita crear enmiendas sobre enmiendas si existe una solución técnica más directa y verificable.
Pero nunca sacrifiques:

* trazabilidad;
* reproducibilidad;
* seguridad;
* evidencia del producto.

17. CALIDAD DE IMPLEMENTACIÓN
No optimices por “terminar esta noche”.
Optimiza por dejar código que mañana siga siendo correcto.
Evita:

* hacks temporales;
* TODOs evitables;
* duplicación de autoridad;
* dependencias innecesarias;
* cambios de alcance encubiertos;
* source guards debilitados;
* pruebas que solo validan la implementación concreta;
* sobreajuste al Controller.

Prefiere:

* contratos claros;
* tests comportamentales;
* código pequeño;
* invariantes;
* fail-closed;
* determinismo;
* separación Application / Plugin / UI.

18. CUÁNDO SÍ DEBES DETENERTE
Solo detente antes de terminar la noche si aparece uno de estos casos:

1. una acción destructiva/irreversible que pueda perder trabajo;
2. necesidad de credenciales o autorización externa que no posees;
3. conflicto real con trabajo ajeno que no puedas reconciliar sin sobrescribirlo;
4. requisito de interacción humana/visual que no pueda automatizarse;
5. evidencia contradictoria que haga imposible saber si el producto es correcto;
6. agotamiento real de presupuesto de producto sin una ruta legítima de corrección;
7. fallo de infraestructura externo que impida continuar;
8. una decisión que alteraría globalmente el protocolo para otras iniciativas;
9. una decisión de producto grande que cambie el objetivo funcional acordado de RackCad.

En esos casos:
no preguntes repetidamente.
Haz todo lo demás que sí sea posible y deja ese único bloqueo claramente documentado.
19. NO ESPERAR RESPUESTAS
No envíes mensajes del tipo:

* “¿quieres que continúe?”
* “¿autorizas?”
* “¿opción A, B o C?”
* “pega esto al Coordinator”
* “necesito tu decisión”

durante esta sesión.
Toma la decisión tú.
Si existe más de una opción razonable:
elige la más conservadora que permita seguir avanzando.
Documenta las alternativas descartadas.
20. USO DEL TIEMPO
Si una CI, suite o revisión tarda:
aprovecha el tiempo para trabajo independiente que no modifique el mismo estado en conflicto.
Puedes preparar:

* siguiente contrato;
* análisis;
* documentación;
* scripts;
* tests;
* Smoke procedures;
* revisión de siguiente fase.

Pero no crees carreras de escritura ni cambies una base mientras una verificación depende de ella.
21. AL TERMINAR
Antes de terminar la sesión nocturna:

1. deja todos los worktrees limpios o explícitamente documentados;
2. push de todo trabajo que deba persistir;
3. confirma ramas y SHAs;
4. registra CI;
5. registra tests;
6. registra fases cerradas;
7. registra fases abiertas;
8. registra cualquier deuda nueva;
9. registra decisiones autónomas tomadas;
10. registra cualquier desviación del plan;
11. registra bloqueos reales;
12. elimina recursos temporales seguros de eliminar;
13. no borres evidencia necesaria.

22. INFORME PARA MAÑANA
Cuando ya no puedas avanzar más o I-64 termine, entrega UN SOLO informe final.
No una secuencia de peticiones de autorización.
Quiero que incluya:
Estado final

* branch / SHA;
* main / SHA;
* árbol limpio o no;
* CI final.

Qué completaste

* fases;
* features;
* tests;
* Smoke;
* integraciones.

Decisiones que tomaste
Para cada decisión importante:

* problema;
* decisión;
* por qué;
* evidencia.

Qué cambió en RackCad
Explicación funcional, no solo commits.
Calidad

* tests Core;
* tests UI;
* builds;
* AutoCAD/Smoke;
* source guards;
* controles.

Qué queda
Solo pendientes reales.
Riesgos
Cualquier riesgo que deba revisar mañana.
Recomendaciones
Siguiente paso más importante.
Historial
Lista compacta de:

* commits principales;
* verificaciones;
* bridges/enmiendas si aparecieron;
* CI.

23. REGLA FINAL
Tu prioridad esta noche es:
AVANZAR.
No esperar al usuario.
No convertir cada decisión en una consulta.
No rebajar los estándares para poder avanzar.
Usa toda la información del repositorio y del historial disponible, toma las decisiones necesarias y continúa hasta:

* terminar I-64;

o, si eso no es posible,

* llegar al máximo punto técnicamente demostrable y dejar mañana una única revisión final clara.

Comienza ahora re-midiendo el estado real de Git y reconciliando I-64 con el `main` actual antes de F2-T2B.
