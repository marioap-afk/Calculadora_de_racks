---
schema: rackcad-initiative/v1
id: I-06
title: Reestructura documental y Context Packs
type: docs
status: waiting
branch: docs/reestructura
base_branch: main
priority: 10
size: M
depends_on: []
conflicts_with:
  - I-07
context_packs: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# I-06 — Reestructura documental y Context Packs

Este archivo define el trabajo futuro de I-06. Crear este contrato no ejecuta ninguna de sus fases.
El estado operativo se deriva de `origin/docs/reestructura` y de su Pull Request, no solo del campo
`status`.

## Estado del piloto

- El bootstrap del contrato y el reclamo remoto de I-06 ya terminaron.
- La fase 1, auditoria documental, termino. Su evidencia versionada esta en
  [I-06-auditoria-documental.md](I-06-auditoria-documental.md).
- La siguiente fase pendiente es la fase 2: decision del dueno sobre taxonomia de Context Packs y
  destinos ambiguos. No debe continuar sin resolver ese gate.
- El piloto se ejecutara manualmente antes de programar horarios o crear una automatizacion.
- Mientras el sistema documental no este integrado en `main`, el modo bootstrap solo puede reanudar
  I-06 en `docs/reestructura`, su worktree y su Pull Request existentes.

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

- Crear `docs/ARCHITECTURE.md` desde `docs/02-modelo-tecnico-vigente.md` y la seccion 4 de la
  auditoria arquitectonica, actualizada con seguridad, layout y cotas.
- Definir Context Packs y el mecanismo documental para seleccionar el contexto por iniciativa.
- Reducir HANDOFF para que conserve unicamente estado vivo.
- Retirar `docs/00-indice-contexto.md`, `docs/01-estado-actual-mvp.md`,
  `docs/03-guia-desarrollo-y-validacion.md` y `docs/04-roadmap-operativo.md` a `docs/archivo/`.
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
- `docs/02-modelo-tecnico-vigente.md`;
- `docs/03-guia-desarrollo-y-validacion.md`;
- `docs/04-roadmap-operativo.md`;
- `docs/auditoria-arquitectura-2026-07.md`, en particular su seccion 4;
- `docs/adr/README.md` y ADRs aceptados;
- todos los referentes obtenidos con una busqueda global de las rutas que se moveran.

Los Context Packs concretos se diseñan durante la iniciativa y requieren decision del dueno antes
de fijar su taxonomia o contrato estable.

## 6. Dependencias

No tiene dependencias previas. I-07 es incompatible y no puede estar activa mientras I-06 trabaje,
porque ambas modifican la estructura y las referencias documentales. La rama debe partir de la punta
actual de `origin/main` y conservarse como `docs/reestructura`.

Entradas del dueno:

- aprobar la taxonomia y granularidad de los Context Packs;
- resolver cualquier contenido unico cuyo destino no sea evidente;
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

1. **Auditoria documental:** inventariar documentos, propositos, contenido unico y todos los
   referentes mediante busqueda global; proponer el mapa final sin mover archivos.
2. Presentar al dueno las decisiones de taxonomia de Context Packs y destinos ambiguos. Detenerse
   hasta recibirlas.
3. Crear `ARCHITECTURE.md`, Context Packs y glosario; verificar que cada tema tenga una fuente.
4. Reducir HANDOFF, mover guias e historicos y conservar el contenido unico identificado.
5. Corregir en la misma rama todos los enlaces y rutas; repetir la busqueda hasta eliminar
   referentes obsoletos.
6. Renderizar o revisar la navegacion Markdown, ejecutar validaciones, publicar y actualizar el
   Pull Request draft para validacion del dueno.

## 9. Pruebas y builds

- `git diff --check`.
- Busqueda global de cada ruta anterior y comprobacion de que los resultados restantes son
  intencionales.
- Verificacion de enlaces Markdown locales y ausencia de destinos duplicados.
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` como guardia del repositorio.
- CI verde sobre la rama.

No requiere build local de UI o Plugin porque el alcance es exclusivamente documental.

## 10. Validacion manual

No requiere AutoCAD. El dueno debe revisar y confirmar:

- la taxonomia de Context Packs y su utilidad para iniciativas reales;
- que `ARCHITECTURE.md` describe la arquitectura vigente y objetivo sin perder restricciones;
- que HANDOFF conserva el estado necesario y ya no duplica historia o arquitectura;
- que la navegacion desde README, AGENTS y CLAUDE llega a los destinos correctos;
- que ningun documento vigente fue archivado por error.

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

## 13. Entrega del Pull Request

Abrir o actualizar un Pull Request draft hacia `main` con titulo `[I-06] Reestructura documental y
Context Packs`. Debe incluir Claim-Id, mapa antes/despues, documentos creados/movidos, decisiones del
dueno, busquedas de referentes, validaciones, CI, hallazgos fuera de alcance y confirmacion de que no
se hizo merge. `automation.auto_merge` permanece siempre en `false`.

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
