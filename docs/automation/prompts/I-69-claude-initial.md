# I-69 — Prompt inicial para Claude

Actua como implementador Claude de I-69 — Selectivo Component Catalog Expansion.
ChatGPT conserva el rol de Coordinator; el humano es el Owner.

## 1. Mandato actualizado

El Owner agregara los largueros, renombres y datos de catalogo mediante Excel/CSV.
Tu responsabilidad es el codigo y las pruebas que resulten necesarios para el
alcance acordado. Los datos pendientes NO bloquean recibir el worktree ni iniciar
la preparacion tecnica. No vuelvas a pedir la lista completa de Excel como
condicion para arrancar; no inventes ni completes registros por el Owner.

Objetivo de producto: opciones de larguero compatibles con IDs estables y
disponibilidad de Guarda trasera y Entrepaño caida de agua en Selectivo, con
persistencia, geometria y BOM segun el contrato que se acuerde.
La decision posterior del Owner esta en docs/automation/decisions/I-69.md, D-09.

## 2. Iniciativa y worktree: recibe el reclamo existente

- Repositorio: https://github.com/marioap-afk/Calculadora_de_racks
- Iniciativa: I-69. Workflow V2.
- Rama: feature/selectivo-catalogo-componentes.
- Claim-Id: ee21eac2-5e9c-4761-aa20-71c18fe3ac23.
- Worktree verificado en el entorno del Coordinator:
  /workspace/scratch/e1f4ec6199fc/feature-selectivo-catalogo-componentes
- Base tecnica del Discovery publicado:
  e350f8d79023b33c70ec3240e4af78c3b872b672.
- Main observado en el relevo:
  812baf879612320653094a8aaf46aca0bac81383; releer al iniciar.

Usa la punta remota actual de la rama, que contiene este prompt y D-09.
El SHA del Discovery es una referencia historica, no una orden de reset.
No abras otra iniciativa, rama equivalente, claim ni fila de ROADMAP.

Si estas en el mismo entorno, reutiliza ese worktree. Si estas en otra maquina,
no asumas que existe la ruta Linux: localiza el clon del Owner y consulta primero
git worktree list --porcelain. Reutiliza el worktree local de esta misma rama.
Solo si no existe, prepara un checkout local de la rama remota ya reclamada,
sin crear otra rama de iniciativa ni forzar un checkout existente.

Antes de escribir: git fetch origin; verifica branch, upstream/divergencia,
status, log y worktrees. Registra tu ruta real y los SHA actuales. Aplica
WORKFLOW §4 si main avanzo, despues de comprobar limpieza y exclusividad.
No hagas reset, clean, stash, descarte ni commit indiscriminado de cambios
ajenos. Si encuentras cambios del Owner u otro escritor, informa el estado
concreto y coordina el relevo antes de una operacion que pueda afectarlos.

La asignacion Owner=datos / Claude=codigo no permite dos escritores simultaneos
en el mismo worktree. Serializa tambien las operaciones Git y las entradas de
datos del Owner. El Coordinator devuelve su turno al entregar este relevo y
revisa cuando tu devuelvas la entrega; no mantengas escritores paralelos.

## 3. Lectura de entrada

Lee AGENTS.md y las autoridades vigentes WORKFLOW, INITIATIVE_LIFECYCLE y
AUTOMATION_PLAN §16. Consulta HANDOFF como contexto, sin editarlo.
Lee los context packs declarados por la iniciativa y estos documentos:

- docs/initiatives/I-69-selectivo-catalogo-componentes.md
- docs/automation/decisions/I-69.md, incluida D-09
- docs/automation/evidence/I-69-evidence.md
- docs/initiatives/I-69-proposal-v1.md (Frozen: NO)
- docs/automation/evidence/I-69/review-v1.md

No repitas el Discovery completo ni sus 30 puntos. Contrasta la base actual y
revisa solo el delta que afecte esta entrega. Proposal V1 antecede a D-09:
O-01/O-02 quedan a cargo del Owner y no son requisito para aceptar este relevo;
los datos reales si deben integrarse y validarse antes de la entrega funcional.

## 4. Primer encargo autorizado

Recibe el worktree y prepara el siguiente paquete tecnico para el Coordinator:

1. Preflight actual y confirmacion del worktree/rama/upstream/exclusividad.
2. Reparto concreto entre datos que completa el Owner y trabajo de software.
   Si el comportamiento acordado ya lo cubre el catalogo, no generes codigo
   artificial. Identifica solo los cambios realmente necesarios.
3. Delta acotado de seguridad: mecanismo para Guarda trasera y Entrepaño,
   superficies afectadas y decisiones pendientes de comportamiento.
4. Propuesta actualizada de implementacion minima, materialidad y gates,
   con T1–T15 y smoke; conserva trazabilidad respecto de Proposal V1.
5. Pruebas existentes relevantes y entorno de ejecucion realmente disponible.
   Puedes ejecutar baseline; informa ejecuciones, resultados y limites reales.

En este primer encargo puedes escribir solo documentos propios de I-69 bajo
docs/initiatives/ y docs/automation/. No se autoriza aun modificar codigo,
pruebas ni catalogos de producto.

Las decisiones de seguridad que siguen abiertas son A/B/C por elemento,
relacion Entrepaño/Parrilla y exposicion en consumidores compartidos.
Lleva al Coordinator una recomendacion concreta y solo las preguntas que aun
exijan decision Owner; no elijas una alternativa por conveniencia tecnica.

## 5. Hechos tecnicos que debes conservar

- El dropdown usa secciones.csv → BeamProfiles → CatalogOption; Id identifica
  y displayName presenta. Persistencia usa BeamId; geometria PieceId; BOM ProfileId.
  Conserva los IDs existentes, incluido casing. Un renombre puede afectar orden/default.
- Seguridad usa seguridad.csv y SelectiveSafetySelection.ElementId/Quantity,
  copia central DeepCopy y DTOs compartidos.
- El camino generico existente es B: UI + persistencia + BOM de cantidad manual.
  A necesita separar seleccion de BOM; C requiere reglas fisicas y bloques reales.
- Una segunda PARRILLA no es automaticamente segura: UI admite varias, pero
  dibujo/BOM toman la primera. No asignes esa familia sin contrato de coexistencia.
- Los catalogos tienen otros consumidores; una familia nueva puede aparecer en
  PushBack. No conviertas ese efecto en una decision implicita del Owner.
- No existe fail-closed general por catalogo faltante. Conserva la politica
  caracterizada por frontera; no la endurezcas incidentalmente.
- La biblioteca DWG del Owner no esta versionada. Bundle/CSV correctos no prueban
  presencia ni parametros de sus bloques.

Los registros, etiquetas, dimensiones y mappings CSV los completa el Owner.
Lee esos datos, identifica faltantes y valida los aportados; no edites por tu
cuenta secciones.csv, seguridad.csv, mensulas.csv, blocks.csv, connection-layout.csv
ni otras tablas del catalogo. No sobrescribas sus cambios al recibirlos.

## 6. Limites, autoridad y salida

Fuera de alcance: I-68/XlsxWriter, consolidacion BOM, Workspace, RAM, expresiones,
rediseño del editor, larguero por lado y plataforma general de catalogos.
No modifiques HANDOFF, ROADMAP, FOUNDATIONS ni iniciativas ajenas en este encargo.
Refresca la interseccion I-64/SelectiveEditorOpenTests antes de proponer tocarla.

F0 sigue abierto. FOUNDATION EVOLUTION es provisional por materialidad pendiente;
no afirma que haga falta schema/migracion. Propone la clasificacion con evidencia.
No declares Freeze, Gate PASS, Candidato, Owner Validation ni merge.
Este prompt inicia un relevo de sesion; no sustituye un paquete formal I-61 ni
acredita EXECUTION_VERIFIED. Si se usa ejecucion delegada, aplicar el protocolo
integrado con contrato, planificacion, elegibilidad y verificacion reales.

Antes de producto se requieren alcance concreto, Freeze/revision y autorizacion.
Conserva el orden: gates, F4 solo preparacion de OV, READY/Candidato, OV y cierre.
No heredes el CI del Discovery a tus commits ni declares AutoCAD disponible sin medirlo.

Devuelve al Coordinator: ruta/branch/HEAD/main/upstream, cambios de base, archivos
tocados, reparto Owner/Claude, propuesta concreta, pendientes minimos, pruebas
ejecutadas o NOT RUN y siguiente paso. Publica tus documentos y entrega el worktree
limpio conforme al Workflow. No mezcles cambios del Owner en tu commit sin identificarlos.

Empieza por recibir y verificar el worktree; no esperes la lista de Excel.

IMPLEMENTATION AUTHORIZATION = NO (producto).
Autorizado ahora: relevo, preflight, lectura/delta y preparacion documental de I-69.
