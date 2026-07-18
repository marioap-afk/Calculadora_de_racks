---
schema: rackcad-initiative/v1
id: I-06
title: Reestructura documental y Context Packs
type: docs
status: completed
branch: docs/reestructura
base_branch: main
priority: 10
size: M
depends_on: []
conflicts_with:
  - I-07
context_packs:
  - documentation-governance
automation_state_path: docs/automation/state/I-06.yml
decision_paths:
  - docs/automation/decisions/I-06.md
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-06 — Reestructura documental y Context Packs

Este archivo define el trabajo de I-06. El estado operativo se valida contra
`origin/docs/reestructura` y se registra en `docs/automation/state/I-06.yml`; el Pull Request puede
contener una copia opcional, pero no es la fuente legible por el ejecutor.

## Estado del piloto

- El bootstrap del contrato y el reclamo remoto de I-06 ya terminaron.
- La fase 1, auditoria documental, termino. Su evidencia versionada esta en
  [I-06-auditoria-documental.md](I-06-auditoria-documental.md).
- La fase 2 termino: la decision del dueno se verifico desde
  [docs/automation/decisions/I-06.md](../automation/decisions/I-06.md) en la rama remota y resolvio
  CP-01, CP-02 y DOC-01 a DOC-06.
- La fase 3 termino con `ARCHITECTURE.md`, los nueve Context Packs y el glosario aprobados.
- La fase 4 termino: HANDOFF fue reducido, se creo la guia de validacion, las guias vigentes se
  movieron a `docs/guias/` y la transicion/historia se preservo en `docs/archivo/`.
- La fase 5 termino: las rutas y enlaces fueron barridos, la navegacion quedo corregida y los nueve
  Context Packs apuntan a fuentes existentes.
- La fase 6 termino: alcance, fuentes, enlaces, pruebas y builds Debug de UI y Plugin quedaron
  revisados.
- La validacion del dueno fue aceptada para preparar la integracion final. I-06 esta `completed`, sin
  gate operativo; el merge sigue pendiente y la marca de ROADMAP se hace efectiva solo al integrar.
- La automatizacion permanece pausada; cualquier piloto posterior sera manual y requerira nueva
  aprobacion antes de programar horarios.
- Mientras el sistema documental no este integrado en `main`, el modo bootstrap solo puede reanudar
  I-06 en `docs/reestructura`, su worktree y su Pull Request existentes.
- En modo Git-only, la incapacidad de leer o actualizar la descripcion del PR no bloquea commit,
  push y estado versionado. El ejecutor no depende de GitHub CLI.

## 1. Objetivo

Crear `docs/ARCHITECTURE.md` y Context Packs que permitan a cada iniciativa cargar solo el contexto
necesario; reducir HANDOFF a estado vivo; archivar documentacion historica; corregir todas las
referencias afectadas y preparar la seleccion de contexto por iniciativa.

## 2. Problema

La documentacion vigente reparte arquitectura, entorno, validacion e historia entre documentos que
se solapan. El ROADMAP identifica duplicacion, referencias cruzadas fragiles y demasiado contexto de
arranque para las IAs. El estado vivo, las convenciones estables y la historia deben tener fuentes
separadas.

## 3. Alcance

- Crear `docs/ARCHITECTURE.md` desde la fuente de transicion ahora preservada en
  `docs/archivo/transicion-2026-07/02-modelo-tecnico-vigente.md` y la seccion 4 de la auditoria
  arquitectonica, actualizada con seguridad, layout y cotas.
- Definir Context Packs y el mecanismo documental para seleccionar el contexto por iniciativa.
- Reducir HANDOFF para que conserve unicamente estado vivo.
- Retirar las fuentes 00, 01, 03 y 04 a `docs/archivo/transicion-2026-07/`.
- Mapear el contenido unico de la guia 03 a su destino antes de archivarla.
- Conservar el patron "agregar un tipo" de la guia 04 hasta que I-18 entregue la guia definitiva.
- Mover los documentos historicos previstos por la arquitectura documental y crear el glosario.
- Corregir todos los referentes afectados, incluidos CLAUDE, AGENTS, README, HANDOFF y WORKFLOW.

## 4. Fuera de alcance

- Cambiar codigo, catalogos, bloques DWG, comportamiento de dibujo, BOM o persistencia.
- Ejecutar I-07 o redactar sus ADRs retroactivos.
- Crear la guia definitiva `guias/agregar-un-sistema.md`, que corresponde a I-18.
- Reescribir el backlog de `ideas-futuras.md` o implementar hallazgos de la auditoria.
- Modificar el proceso de integracion, hacer merge o activar auto-merge.

## 5. Contexto requerido

Leer completamente antes de editar:

- `AGENTS.md`;
- `docs/WORKFLOW.md`;
- `docs/ROADMAP.md`, especialmente la arquitectura documental objetivo;
- `docs/HANDOFF.md` solo para separar estado vivo de historia;
- `docs/archivo/transicion-2026-07/02-modelo-tecnico-vigente.md`;
- `docs/archivo/transicion-2026-07/03-guia-desarrollo-y-validacion.md`;
- `docs/archivo/transicion-2026-07/04-roadmap-operativo.md`;
- `docs/guias/validacion-manual-autocad.md`;
- `docs/auditoria-arquitectura-2026-07.md`, en particular su seccion 4;
- `docs/adr/README.md` y ADRs aceptados;
- `docs/automation/state/I-06.yml`;
- `docs/automation/decisions/I-06.md`;
- todos los referentes obtenidos con una busqueda global de las rutas que se moveran.

La taxonomia y el contrato ligero de Context Packs fueron aprobados por el dueno. Una iniciativa
puede usar multiples packs; cada pack puede resumir invariantes esenciales sin copiar capitulos.

## 6. Dependencias

No tiene dependencias previas. I-07 es incompatible y no puede estar activa mientras I-06 trabaje,
porque ambas modifican la estructura y las referencias documentales. La rama debe partir de la punta
actual de `origin/main` y conservarse como `docs/reestructura`.

Entradas del dueno:

- taxonomia, granularidad y destinos ambiguos: resueltos en
  `docs/automation/decisions/I-06.md`;
- validar que HANDOFF reducido siga siendo suficiente para continuidad real.

## 7. Archivos esperados

Se esperan cambios documentales en:

- `docs/ARCHITECTURE.md` y los Context Packs que se aprueben;
- `docs/HANDOFF.md`, `docs/WORKFLOW.md`, `README.md`, `AGENTS.md` y `CLAUDE.md` para reducir o
  corregir referencias;
- `docs/guias/` para las guias vigentes y el glosario;
- `docs/archivo/` para documentos historicos y los indices/guias retirados;
- documentos que contengan enlaces a cualquiera de las rutas movidas.

No se esperan cambios bajo `src/`, `tests/`, `assets/` o `deploy/`.

## 8. Fases

1. **Completada — auditoria documental:** inventariar documentos, propositos, contenido unico y todos
   los referentes mediante busqueda global; proponer el mapa final sin mover archivos.
2. **Completada — decision del dueno:** taxonomia de Context Packs y destinos ambiguos aprobados en
   la evidencia versionada.
3. **Completada — fuentes nuevas:** crear `ARCHITECTURE.md`, Context Packs y glosario; verificar que
   cada tema tenga una fuente.
4. **Completada — migracion:** reducir HANDOFF, mover guias e historicos y conservar el contenido
   unico identificado.
5. **Completada — referencias:** corregir enlaces y rutas; repetir la busqueda y justificar las
   referencias historicas conservadas.
6. **Completada — cierre:** navegacion Markdown, alcance, pruebas, builds Debug de UI y Plugin y
   evidencia final revisados. La validacion del dueno fue aceptada para preparar la integracion; el
   merge manual sigue pendiente y el PR no se manipula directamente en modo Git-only.

## 9. Pruebas y builds

- `git diff --check`.
- Busqueda global de cada ruta anterior y comprobacion de que los resultados restantes son
  intencionales.
- Verificacion de enlaces Markdown locales y ausencia de destinos duplicados.
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` como guardia del repositorio.
- CI verde sobre la rama.

En una ejecucion Git-only que no pueda consultar checks remotos, registrar esa limitacion y ejecutar
localmente la suite y el build Debug de UI equivalentes a los dos jobs del workflow. La falta de
lectura remota no impide publicar el commit y el estado versionado; nunca se inventa un resultado de
CI.

No requiere build local del Plugin ni AutoCAD. El build de UI solo se usa como equivalente local del
job de CI cuando los checks remotos no son consultables.

## 10. Validacion manual

No requiere AutoCAD. El dueno debe revisar y confirmar:

- la taxonomia de Context Packs y su utilidad para iniciativas reales;
- que `ARCHITECTURE.md` describe la arquitectura vigente y objetivo sin perder restricciones;
- que HANDOFF conserva el estado necesario y ya no duplica historia o arquitectura;
- que la navegacion desde README, AGENTS y CLAUDE llega a los destinos correctos;
- que ningun documento vigente fue archivado por error.

Esta revision del dueno fue aceptada para la preparacion final. La iniciativa completada no debe
confundirse con integracion: `main` solo cambia cuando el dueño ejecuta el merge manual posterior.

## 11. Criterios de aceptacion

- Existe una sola fuente vigente por tema documental y los demas documentos enlazan a ella.
- `ARCHITECTURE.md`, Context Packs, glosario, `guias/` y `archivo/` cumplen la arquitectura objetivo
  del ROADMAP.
- El contenido unico de las guias retiradas se preserva en un destino explicito.
- No quedan enlaces rotos ni referentes obsoletos a rutas movidas.
- No hay cambios fuera de documentacion.
- Pruebas y CI estan verdes.
- El dueno completo la validacion manual y las decisiones requeridas.
- El Pull Request sigue draft y no se hizo merge.

## 12. Condiciones para detenerse

- I-07 u otra iniciativa activa toca los mismos documentos.
- El destino de contenido unico, la taxonomia de Context Packs o el nivel de reduccion de HANDOFF
  requiere una decision no recibida.
- Aparece una necesidad de cambiar codigo, catalogos, workflows de CI o alcance del ROADMAP.
- Una referencia no puede corregirse sin romper compatibilidad externa conocida.
- CI falla por una causa no documental o se consumen tres intentos de correccion con evidencia.
- La rama remota, el Pull Request o `origin/main` muestran trabajo concurrente incompatible.

## 13. Estado versionado y entrega del Pull Request

Reutilizar el Pull Request draft #1 hacia `main`; nunca abrir otro. El estado canonico se publica en
`docs/automation/state/I-06.yml`. Si existe una integracion capaz de actualizar la descripcion del PR,
puede sincronizar una copia, pero esa operacion es opcional y no requiere GitHub CLI. El estado y el
informe incluyen Claim-Id, mapa antes/despues, documentos creados/movidos, decisiones del dueno,
validaciones, hallazgos fuera de alcance y confirmacion de que no se hizo merge.
`automation.auto_merge` permanece siempre en `false`.

## 14. Evidencia final

Entregar:

1. SHA de `origin/main` usada y Claim-Id.
2. Commits publicados y enlace del Pull Request draft.
3. Inventario de archivos creados, movidos y modificados.
4. Mapa de fuentes vigentes y archivo de destinos historicos.
5. Resultado de busqueda de referentes y validacion de enlaces.
6. Resultado de pruebas y CI.
7. Decisiones y validacion manual del dueno.
8. Intentos consumidos y cualquier bloqueo restante.
9. Confirmacion de que no se modifico `main`, no se implemento codigo y no hubo merge automatico.
10. Contenido final de `docs/automation/state/I-06.yml`.
