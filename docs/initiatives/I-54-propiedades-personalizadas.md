---
schema: rackcad-initiative/v1
id: I-54
title: "ID24 — Custom Properties Foundation"
type: architecture
status: claimed
branch: architecture/propiedades-personalizadas
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [persistence, autocad-plugin, architecture-kernel, ui-editors]
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

# I-54 — ID24 — Custom Properties Foundation

> **Fase actual: G2A — Proposal V1 publicada, SIN consenso.** Discovery en [I-54-discovery.md](I-54-discovery.md)
> (G1 `195964b`), Proposal en [I-54-proposal-v1.md](I-54-proposal-v1.md) y paquete de revision en
> [I-54-architect-review-package.md](I-54-architect-review-package.md). `Coordinator = NOT REVIEWED`,
> `Architect = NOT REVIEWED`, `Owner = NOT ASKED`. **No hay una sola linea de produccion escrita** y la
> implementacion esta BLOQUEADA (seccion 12). La revision de Arquitecto no se inicia sin orden.

> **Apertura por autorizacion explicita del Owner sin fila previa**, transmitida por el Coordinador de
> I-54 — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la
> preexistencia de la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap
> inmediatamente posterior al reclamo atomico. **No existe** `docs/automation/decisions/I-54.md` y la
> norma **no lo exige** para el caso (d), asi que `decision_paths` queda vacio en vez de apuntar a un
> archivo inventado.

```text
Initiative = I-54
Owner ID   = ID24 — Custom Properties Foundation
Branch     = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
```

## 1. Objetivo

Fundar en RackCad **propiedades personalizadas**: metadatos con nombre y valor definidos por el usuario,
en dos alcances —**Proyecto** (el dibujo) y **Rack**—, persistidos en el DWG con identidad estable. La
fundacion **no** convierte esas propiedades en variables de proyecto ni en expresiones, y deja
**declarados, no implementados**, los puntos de extension hacia el dibujo, las expresiones y las
plantillas.

Esta sesion **no** entrega la fundacion. Entrega la **evidencia** (G1) y la **primera propuesta**
(Proposal V1) sobre la que Coordinador y Arquitecto deben converger antes de escribir codigo.

## 2. Problema

Hoy RackCad persiste en el DWG dos clases de dato del usuario: el **sobre por rack**
(`RackEmbedDocument`, sobre la definicion de bloque; [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md))
y el **registro de variables de proyecto** (I-47,
[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)). Que metadatos de proyecto o de rack
existen ya —nombre, cliente, area, ubicacion, revision, notas, descripcion, codigo o equivalentes—, donde
viven y con que semantica **no se afirma aqui**: es el primer entregable de G1.

El Coordinador entrego hallazgos **preliminares** que G1 debe **confirmar o corregir** contra el codigo, sin
darlos por ciertos:

| # | Hallazgo preliminar del Coordinador |
|---|---|
| H-1 | `origin/main` observado antes de la ejecucion: `46fcac2b071929d2bd5b07aa28373941417f74a8` (no asumido: verificado en el preflight del reclamo) |
| H-2 | `RACKCAD_PROJECT` parece ser hoy un **Xrecord directo** del NOD, no un sub-`DBDictionary` |
| H-3 | `RackEmbedDocument` parece la **costura comun natural** para metadatos de nivel rack |
| H-4 | I-51 parece **conservar los campos adicionales del sobre exterior** al re-estampar; debe probarse contractualmente |
| H-5 | La exportacion a biblioteca del Selectivo parece derivarse del **diseno interior**, y por tanto no exportaria metadatos situados solo en el sobre |
| H-6 | I-49 **no** debe convertirse en dependencia |
| H-7 | I-50 puede tocar DTOs del Selectivo; hay que **medir** el cruce actual |

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Auditar por archivo y simbolo:
   - metadatos existentes: `ProjectName`, `Client`/`Cliente`, `Area`, `Location`/`Ubicacion`, `Revision`,
     `Notes`, `Description`, `Code` y equivalentes;
   - `RACKCAD_PROJECT`, `ProjectVariablesData`/`Registry`/`Document`/`Store` y la semantica
     ABSENT / PRESENT-BUT-UNREADABLE;
   - `RackEmbedDocument`, `RackEmbedStore`, `RackBlockData`, `RackId` y definicion frente a referencia;
   - autoridad entre vistas hermanas;
   - `RACKEDITAR` y los handlers por kind: como redibuja y como crea vistas cada sistema;
   - I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp`, `RackDuplicarCommands`, `RackCloner`;
   - biblioteca, `.rackcad.json`, exportacion/importacion y el cruce real entre dibujos;
   - politicas de schema y version, y `JsonExtensionData`;
   - patrones reutilizables de UI y CRUD, en especial `RACKVARIABLES`;
   - serializadores y ayudantes reutilizables;
   - confirmar o corregir H-1..H-7.
3. **G2 — Proposal.** En esta sesion, solo la **V1**. Compara explicitamente:
   **A** `Dictionary<string,string>`, **B** registros con id estable y **C** documentos de propiedad tipados;
   y decide o propone: identidad; alcances Proyecto/Rack; solo texto frente a Texto/Numero/Booleano;
   persistencia de proyecto; persistencia de rack; autoridad entre hermanas; renombrar y borrar;
   semantica de duplicacion e independencia; biblioteca/exportacion; schema malformado o futuro; punto
   de extension futuro hacia el dibujo; punto de extension futuro hacia expresiones **sin implementar
   expresiones**; punto de extension futuro hacia plantillas; y la UI minima reutilizable.

   El Coordinador pidio **evaluar especialmente** —y **no aceptar por instruccion**— esta opcion:
   `CustomPropertyId + Name + string Value`; Proyecto = documento versionado **independiente** en el NOD,
   **sin** migrar `RACKCAD_PROJECT` salvo evidencia fuerte; Rack = campo **aditivo comun** en
   `RackEmbedDocument`, no en DTOs de producto; los mismos metadatos en **todas** las vistas hermanas del
   `RackId`; renombrar conserva el id; duplicar copia los valores en **colecciones independientes**; ni
   ProjectVariables, ni parser, ni formulas.
4. **Publicacion** en la rama: informe Discovery; Proposal V1; mapa de archivos y simbolos con los cruces
   con las ramas activas; decisiones materiales; riesgos; recomendacion de ADR si/no; y un **paquete
   autonomo** para la revision de Arquitecto.
5. **Implementacion**: sus gates se definen tras el consenso de G2. **No esta autorizada.**

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0, G1 y G2**: nada en `src/`, `tests/`, `assets/`, `eng/`,
  `deploy/` ni `.github/`. **G3+ no se inicia.**
- `docs/HANDOFF.md`: prohibido por la orden y por [WORKFLOW](../WORKFLOW.md) seccion 2.
- **La semantica de Project Variables** (I-47, ADR-0034) y de la **edicion vinculable** (I-48): una
  propiedad personalizada no es una variable de proyecto, y esta iniciativa no reabre esa semantica.
- **Parser, formulas y expresiones** (ID22B, territorio de I-49). **I-49 no es dependencia** de I-54.
- **Cotas por vista** (I-50) y **RACKMIRROR** (I-52).
- **Migrar `RACKCAD_PROJECT`** sin evidencia fuerte; y aun con ella, eso seria decision, no implementacion.
- **Dibujar** propiedades en el DWG (atributos, textos, cuadros de rotulacion) y **plantillas**: solo se
  declaran sus puntos de extension.
- Los hallazgos laterales se registran —en el Discovery y, al fijar el contrato, en
  [ideas-futuras.md](../ideas-futuras.md)—; **no se arreglan aqui**.

## 5. Contexto requerido

- [ARCHITECTURE.md](../ARCHITECTURE.md): identidad del rack y persistencia embebida.
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)
  y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md).
- Contrato de [I-11](I-11-persistencia-uniforme.md) (campos desconocidos y version no degradada).
- Decisiones de [I-47](../automation/decisions/I-47.md), Proposal V8 de [I-48](I-48-proposal-v8.md) y
  contrato y decisiones de [I-51](I-51-rackduplicar-multiples-origenes.md).
- Contratos de I-49, I-50 e I-52 **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.
- [AGENTS.md](../../AGENTS.md): convenciones de persistencia versionada, pruebas y evidencia.
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **Sin dependencias pendientes**: I-47, I-48 e I-51 estan integradas.
- **I-49 no es dependencia**, por instruccion expresa del Coordinador.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main`, **I-49**
  (`architecture/motor-expresiones-parametricas`, solo documentacion), **I-50**
  (`feature/cotas-independientes-por-vista`, con produccion en DTOs y Domain de Selectivo y Dinamico) e
  **I-52** (`feature/rackmirror-espejo-semantico`, reclamada durante el preflight). `conflicts_with` queda
  **vacio** hasta que G1 mida el cruce real de archivos: no se declara un estorbo sin evidencia, ni se
  omite uno que la tenga.
- **Entrada del Owner**: la Proposal V1 lista nueve preguntas de producto (OQ-01..OQ-09: biblioteca, limites,
  comando, politica de copia, builds desplegados, orden, relacion con ID20, texto de ID24 y edicion desde los
  editores de sistema). Con esa evidencia `requires_owner_decision` pasa a `true`; la metadata solo añade.
- **Cruce medido en G1** ([Discovery](I-54-discovery.md) §2 y §13): cruce **productivo** actual con I-49,
  I-50, I-52 e I-53 = **cero archivos** mientras I-54 no toque DTO ni Domain de sistema, editores de sistema,
  `RackEnvelopeRestamp` ni `RackCloner`. Cruce **documental** previsto con las cuatro (fila de ROADMAP tras I-51,
  final de `ideas-futuras.md`, numeracion de ADR) y **textual** con I-52 si ambas añaden comandos (censo de
  `[CommandMethod]` y ayuda). Cruce **semantico** con I-49: `Rack`/`Project` reservados para ID20. Por eso
  `conflicts_with` sigue vacio.

## 7. Archivos esperados

**Este contrato no fija la lista de archivos de produccion, y no debe fingir que si.** El mapa por archivo
y simbolo **es el entregable de G1**, y los archivos de produccion los fija el consenso de G2.

Lo que si se declara para esta sesion: solo `docs/initiatives/I-54-*.md` y la fila de I-54 en
`docs/ROADMAP.md`. **Una desviacion material frente a esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** — bootstrap `f908b2f` |
| G1 | Discovery | [I-54-discovery.md](I-54-discovery.md): informe por archivo/simbolo, H-1..H-8, mapa de cruces, riesgos y hallazgos fuera de alcance | **HECHA** — `195964b` |
| G2 | Proposal y consenso | **G2A**: [Proposal V1](I-54-proposal-v1.md) + [paquete de Arquitecto](I-54-architect-review-package.md) (su SHA se registra en el commit siguiente). Revision de Arquitecto, reconciliacion y Consensus Freeze **despues**, cada una con orden propia | **G2A HECHA**; revision PENDIENTE, sin consenso |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Por definir **tras el consenso de G2**, no aqui | bloqueada |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **tras el consenso de G2**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no
depende de G2: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y builds
Debug de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0, G1 y G2 no producen codigo:
su evidencia es documental.

## 10. Validacion manual

**Requerida** en la implementacion: persistir datos del usuario en el DWG y hacerlos sobrevivir a
`RACKEDITAR`, a la duplicacion y a guardar/reabrir cambia el comportamiento del dibujo (AGENTS.md,
punto 5). El **checklist concreto se fija tras el consenso de G2**. G0, G1 y G2 **no** la requieren: no
tocan producto.

## 11. Criterios de aceptacion

Preliminares; **los fija el consenso de G2**. Solo recogen lo que ya es doctrina del repositorio o limite
expreso de la orden:

1. Las propiedades de ambos alcances sobreviven a guardar, cerrar y reabrir el mismo DWG.
2. Un contenedor presente pero ilegible, o de un schema futuro, **no** se trata como vacio
   (`UNKNOWN/UNREADABLE != ABSENT`, ADR-0034).
3. Ninguna propiedad personalizada se lee, escribe ni resuelve como variable de proyecto, expresion o
   formula.
4. El formato persistido existente sigue abriendose, y un binario anterior no destruye en silencio lo que
   no entiende, en la medida que fije la Proposal.

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0, G1 y Proposal V1, solo documentacion.** Ninguna linea de produccion;
  G3+ no se inicia.
- **Implementacion bloqueada** hasta `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma**
  Proposal, y la aprobacion del Owner.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49, I-50 o I-52**: reportarlo antes
  de continuar.
- Si la Proposal exige cambiar el **formato persistido existente** (major de `RackEmbedDocument`, forma de
  `RACKCAD_PROJECT`) o la semantica de identidad del rack: **ADR y decision del Owner antes de implementar**.
- Si el alcance deriva hacia Project Variables, expresiones, dibujo de propiedades o plantillas:
  **detenerse** (seccion 4).
- `docs/HANDOFF.md` **no se toca**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-54.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y el caso (d) no lo exige. [WORKFLOW](../WORKFLOW.md) seccion 8 pide para el bootstrap del caso (d)
exactamente **fila en ROADMAP mas contrato**. El estado vivo se deriva, como manda la seccion 2, de la
existencia de `origin/architecture/propiedades-personalizadas`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-54 - ID24 - Custom Properties Foundation
Rama       = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784  (commit vacio)
Claim-Id   = d4b871e9-8a5d-4e67-bc11-a8f3c023788e
```

Primer `git push -u origin architecture/propiedades-personalizadas` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna referencia a I-54 ni a ID24 en el preflight. `main` **no fue
modificada**.

**G1 — Discovery.** Solo documentacion, sobre el codigo de `BASE_SHA` (la rama no difiere en `src/`,
`tests/` ni `assets/`). Preflight de G1: `origin/main` sin mover; I-49 y I-52/I-53 solo documentacion; I-50
con produccion que **no** toca sobre, compositor, restamp, cloner ni autoridad. Sin compilacion ni pruebas:
G1 no produce codigo. Los hallazgos fuera de alcance (F-01..F-12 en G1; F-13 añadido en G2A) quedan en el Discovery y pasan a
`ideas-futuras.md` cuando se fije el contrato.

**G2A — Proposal V1.** Solo documentacion. Compara A/B/C, evalua componente a componente la opcion del
Coordinador sin aceptarla por instruccion, propone D-01..D-20, INV-01..INV-16, gates G2B..G10 no autorizados,
pruebas T/U, validacion OV-01..OV-12 y preguntas OQ-01..OQ-09, y recomienda ADR antes de implementar. El
paquete autonomo de Arquitecto lista RD-01..RD-20 con evidencia verificable. Addendum al Discovery (§2.5:
re-medicion de paralelas antes de publicar —I-50 @ `8ceb3a7`, I-52 @ `339b3ab`, I-53 @ `c8476cc`, sin cambio de
cruce—; §11.5: lista verificada de comandos; §15: F-13, troceado de Xrecord duplicado). Sin compilacion ni pruebas.

El resto de la evidencia se acumula al cerrar cada fase.
