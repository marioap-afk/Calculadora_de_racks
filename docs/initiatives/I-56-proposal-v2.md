# I-56 G2.1 — Workflow V2 Proposal V2

> ```text
> PROPOSAL V2 — NOT CONSENSUS
>
> Proposal Version          = V2
> Coordinator               = REVIEW REQUIRED
> Architect                 = NOT OPEN
> Consensus                 = NOT REACHED
> Owner                     = NOT REQUESTED
> Workflow V2               = NOT EFFECTIVE
> WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
>
> I-56 remains governed by Workflow V1.
> ```
>
> Documento de **propuesta**, no de norma. Nada de lo que aqui se escribe con verbos normativos («se exige», «debe») esta en
> vigor: describe la regla que la V2 tendria **si** la aprobaran Coordinator y Architect sobre esta misma version y despues el
> Owner (contrato I-56 §0.2). Mientras tanto rige Workflow V1 para I-56 y para toda iniciativa reclamada antes de
> `WORKFLOW_V2_EFFECTIVE_SHA` (contrato I-56 §0.1). Esta propuesta **no** modifica ningun contrato activo (I-49, I-52, I-55) y
> **no** se aplica a sus reclamos, ramas y contratos existentes (una unidad nueva reclamada despues del SHA sigue P-25 T8).

## Cambios respecto de V1 — reconciliacion cerrada

La [Proposal V1](I-56-proposal-v1.md) (commit `18401da`) recibio del Coordinator el veredicto **CHANGES REQUIRED — PROPOSAL V2**.
Esta V2 es una **reconciliacion**, no un rediseño: conserva todas las decisiones de V1 salvo lo que exigen los ocho hallazgos. V1 no
se modifica. La revision del Architect **no** esta abierta.

| # | Coordinator finding | V1 problem | V2 resolution | Sections changed | Status |
|---|---|---|---|---|---|
| CR-01 | `WORKFLOW_V2_EFFECTIVE_SHA` con dos significados | V1 lo definia como el merge normativo, pero hacia empezar la vigencia al levantarse una pausa posterior y clasificaba como V1 un reclamo hecho durante esa pausa | Un solo significado: el merge normativo de I-56 **es** el punto de vigencia. La pausa de reclamos es una decision temporal del Owner que puede empezar antes del merge y durar hasta la verificacion post-merge, **sin** crear otro momento de vigencia. Un reclamo aceptado despues del SHA es V2 aunque viole la pausa: es DESVIACION DE PROCESO y se detiene hasta resolver la activacion; nunca se reclasifica como V1. Un reclamo aceptado antes del SHA es V1. El orden se decide por ascendencia y por la punta de `origin/main` registrada tras el reclamo; si no se puede decidir, STOP y decide el Owner, nunca V1 por defecto | P-25 (incluida la tabla de verdad T1..T8); P-20 (tag de I-56 por decision local); §4 (Transicion); §6 OWN-E; §10; §11 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-02 | Herencia de V1 entre reclamos | Una unidad de entrega listada antes del SHA efectivo heredaba V1 aunque se reclamara despues | Cada unidad con reclamo atomico, rama y worktree propios se clasifica por **su propio** reclamo. El Freeze conceptual se puede consumir, la version de workflow nunca se hereda. Unidad V2 sobre diseño V1: sin reescribir el Freeze V1, intake y Discovery delta V2 de compatibilidad; si se consume sin contradiccion se referencia; si exige un contrato materialmente distinto, STOP y mecanismo V2 de cambio material / Owner. Una unidad nueva de I-49, I-52 o I-55 reclamada despues del SHA es V2 con archivos propios, sin tocar los de la iniciativa V1 (T8, a confirmar por el Owner) | §3; P-02; P-03 regla 3; P-09 (Freeze V1 consumido); P-25 T7 y T8; §6 OWN-J; §11 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-03 | Freeze de EXTENSION y evidencia en el mismo archivo | El Freeze de EXTENSION vivia en el contrato y se comprobaba inmutable, mientras P-15 escribia la «Evidencia final» operativa en ese mismo contrato | Tres superficies separadas: **contrato** (estado mutable y enlaces, sin hashes), **artefacto de Freeze** inmutable (archivo propio en todos los arquetipos) y **evidencia operativa** (`docs/automation/evidence/<unidad>-evidence.md`, carpeta ya existente) | P-06 regla 2; P-09 (archivo de Freeze, trailer, A-n para todo cambio posterior); P-12 READY-09 y sin commits entre READY-04 y cierre; P-14; P-15; P-18 plantillas; P-19; P-22; P-24; §4; §8 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-04 | U-11 sin resolver | Los hechos posteriores al merge no tenian registro durable | **Tag anotado `integration/<unidad>`** sobre el merge verificado de cada unidad V2, creado tras la verificacion y la limpieza, con mensaje estructurado (CLOSURE_SHA, MERGE_SHA, FINAL_MAIN_SHA si hubo merge de correccion, CI post-merge, cobertura del Candidato, limpieza). Sin commit, luego sin recursion; tag ausente = desviacion detectada. Requiere decision de politica Git del Owner (OWN-H); para I-56, que es V1, tag por decision local del Owner | P-15; P-20; P-18 plantilla E; P-21; P-24; P-25; §6 OWN-H; §10 U-07 y U-11 cerradas | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-05 | Discrepancias de fundacion aplazables | P-06 permitia llevar a ideas-futuras una discrepancia registro/ADR/codigo fuera de alcance aunque afectara lo consumido | Discrepancia que afecta una premisa, autoridad, regla de persistencia, garantia de mutacion, semantica de fallo, punto de extension o prueba protectora **consumida**: STOP; se resuelve por arreglo previo, revision de cambio material o decision del Owner/Architect. Solo lo demostrablemente irrelevante para todo invariante consumido se aplaza. «Fuera de alcance para arreglar» no es «seguro ignorar» | P-05 (EXP-01, criterio de salida); P-06 regla 5 (A/B con confirmacion de B); P-10 cierre de gate; P-17 regla 7; P-19; plantillas A, B y D; §11 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-06 | Regla de agrupacion demasiado amplia | Bastaba compartir autoridad o fundacion y persistencia o mutacion, aunque el problema de usuario fuera distinto | Agrupar exige **ambas**: sustancialmente el mismo problema de usuario u objetivo observable **y** autoridad o fundacion de diseño materialmente compartida, normalmente acoplada por persistencia o mutacion. Infraestructura compartida sola no agrupa: se reutiliza la fundacion y se secuencia o coordina. I-53 e I-50 re-probados | P-02; §4 (Agrupacion); P-26 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-07 | Contrato como almacen de estado vivo de ramas | P-07 pedia «Punta observada» en la tabla del contrato | El contrato registra solo archivos calientes conocidos, dependencias declaradas y estrategia de coordinacion. Las puntas observadas van a la evidencia de preflight: cuerpo del commit e informe de gate o sesion. «Punta observada» retirada de los requisitos durables y de las plantillas | P-07; P-18 plantillas A, B y F; P-21; §4 | RESOLVED IN V2 (pendiente de revision del Coordinator) |
| CR-08 | Evaluacion bloqueada por arquetipos raros | P-22 exigia cinco iniciativas con al menos una de cada arquetipo | Dos puntos: (1) evaluacion general tras aproximadamente cinco iniciativas V2 integradas, con los arquetipos ausentes como UNKNOWN; (2) evaluacion completa por arquetipo cuando los tres tengan muestras reales. Ninguno es compuerta de seguridad para iniciativas ordinarias | P-22; §10 U-12 | RESOLVED IN V2 (pendiente de revision del Coordinator) |

**Nada mas cambia de fondo.** Se conservan sin rediseño: tres arquetipos y M-01..M-08; arquetipos que nunca gobiernan CI, pruebas,
Candidato, cobertura ni evidencia de OV; Discovery Core + Conditional; verificacion en codigo de fundaciones consumidas; modelo
anti-churn; revision del Freeze por el Architect en EXTENSION independiente; re-revision del delta; conformidad final Architect +
Coordinator; gates funcionales; RED→GREEN focal; CI leido antes del siguiente gate; cambio moderado de cadencia del Core; evidencia
Full del Candidato; modelo READY (por CR-03 solo cambian READY-09 y la ventana sin commits documentales entre READY-04 y el cierre); exact-SHA; matriz OV; cobertura sin cambio; reference over
repetition; autoridad por dominio; sin ORCHESTRATION.md; sin T0–T4, R0–R4 ni Quick CI; sin merges automaticos. El autocontrol de V1
(sus SC-01..SC-29) sigue registrado en [V1 §11](I-56-proposal-v1.md) y no se copia; el de V2 esta en §11.

## 0. Lectura, entradas y convenciones

### 0.1 Entradas

| Entrada | Uso | Estado |
|---|---|---|
| [Contrato I-56](I-56-initiative-workflow-v2.md) §0–§4, §12 | Invariantes y alcance vinculantes | vigente |
| [Evidence Audit](I-56-evidence-audit.md) (G1 + G1.1) | Toda la evidencia; categorias H.1/H.2/H.3 | **cerrado y aceptado**; no se modifica |
| [WORKFLOW](../WORKFLOW.md), [AGENTS](../../AGENTS.md), [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) | Linea base V1 | vigentes en `main` `dad4e77` |
| [TEMPLATE](TEMPLATE.md), [README de iniciativas](README.md), [Context Packs](../context-packs/README.md), [documentation-governance](../context-packs/documentation-governance.md) | Estructura documental V1 | vigentes |
| [Guia de validacion manual](../guias/validacion-manual-autocad.md) §6–§8, [adr/README](../adr/README.md) | Candidato, Owner Validation, estados de ADR | vigentes |
| [ADR-0033](../adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) | **Solo como registro historico** (estado `propuesto`) | no es autoridad aceptada |
| [Proposal V1](I-56-proposal-v1.md) + veredicto del Coordinator (CR-01..CR-08) | Base de esta reconciliacion | V1 historica; no se modifica |

La auditoria no presenta ninguna contradiccion factual que haga imposible esta propuesta; no se modifico.

### 0.2 Convenciones

- **Evidencia**: `EA <id>` remite al Evidence Audit (`EA B-12` = TABLE B fila B-12; `EA C-08` = TABLE C; `EA Q7` = respuesta
  Q7; `EA O-2` = incidente de orden; `EA §6` = seccion 6; `EA H.1-7` = restriccion H.1-7; `EA hechos A.12` = seccion A punto 12).
  Las clases (MEASURED, RECONSTRUCTED, INFERENCE, UNKNOWN) son las de la auditoria y no se re-miden aqui.
- **Categorias de cada regla** (las cinco de la orden de G2), marcadas entre corchetes:

  | Marca | Categoria |
  |---|---|
  | `[BIND]` | A. Restriccion vinculante de I-56 (EA H.1) |
  | `[KEEP]` | B. Politica V1 que se propone conservar sin cambio |
  | `[CHANGE]` | C. Politica V1 escrita que se propone cambiar |
  | `[NEW]` | D. Mecanismo nuevo de la V2 (incluye codificar practica no escrita, EA H.2-13) |
  | `[NOT-REOPENED]` | E. Alternativa historica rechazada o diferida que **no** se reabre (EA H.3) |

- **Identificadores estables**: `P-nn` decisiones de la propuesta; `M-nn` disparadores de materialidad; `EXP-nn` disparadores de
  expansion del Discovery; `READY-nn` condiciones de preparacion del Candidato; `OV-nn` escenarios de Owner Validation;
  `OWN-x` decisiones reservadas al Owner; `U-nn` preguntas abiertas; `CR-nn` hallazgos del Coordinator sobre V1; `SC-nn`
  hallazgos del autocontrol. Ningun prefijo se usa
  como nivel, tier ni clase de riesgo.
- Una regla `[CHANGE]` o `[NEW]` sin evidencia del Evidence Audit declara su **racional explicito** en lugar de evidencia.
- **Aprobacion.** Toda regla `[CHANGE]` o `[NEW]` de este documento solo seria norma tras la aprobacion global de la version
  (EA H.1-3). La columna «Owner decision required?» de la tabla §4 distingue ademas las que exigen una **decision especifica**
  del Owner (§6) de las que solo entran en esa aprobacion global.
- **Anclas.** Como el Evidence Audit, este documento cita algunos SHAs y corridas como anclas de evidencia ya auditada; no es
  estado vivo ni contrato. La tension con «hashes solo en HANDOFF» (AGENTS; WORKFLOW §8) esta registrada en EA Q10 y la trata P-15.

---

## 1. Objetivo de diseño

Workflow V2 debe **reducir**: Discovery repetido; re-descripcion de contratos ya establecidos; churn de Proposal/Architect;
micro-gates sin resultado util por si mismos; Full repetido antes de que exista un Candidato real; Candidatos prematuros;
ediciones repetidas de documentos compartidos; duplicacion y re-enunciado contradictorio en las ordenes; ceremonia fija por
unidad Git.

**Sin debilitar**: identidad por SHA exacto; comportamiento fail-closed; pruebas necesarias; validacion Full del Candidato;
Owner Validation; confianza posterior al merge; revision arquitectonica donde añade valor material.

**No optimiza**: menos pruebas como KPI; menos hallazgos de Architect; menos documentos a costa de claridad; ordenes mas cortas a
costa de seguridad. Ninguna metrica de la seccion P-22 se lee como «mejor» por bajar el numero de pruebas ejecutadas.

---

## 2. Restricciones vinculantes que la propuesta hereda `[BIND]`

Proceden de la autorizacion de I-56 (EA H.1). Esta propuesta no las reabre; cada decision que las roza las cita.

| EA | Restriccion | Donde la respeta esta propuesta |
|---|---|---|
| H.1-1 | Solo documentacion y proceso; sin implementacion de producto, pruebas, CI ni scripts | P-21 solo diseña scripts; P-24 solo recomienda archivos; G2 crea un unico documento. Editar `AGENTS.md` (fuera de `docs/**`) en la integracion normativa exige declararlo y una orden que lo autorice (contrato I-56 §12) |
| H.1-2 | Transicion: I-49, I-52, I-55 y toda iniciativa reclamada antes de `WORKFLOW_V2_EFFECTIVE_SHA` grandfathered | P-25 |
| H.1-3 | Vigencia: Coordinator AGREED + Architect AGREED sobre la misma version + Owner APPROVED | Bloque de estado; P-25; §OWN |
| H.1-4 | No modificar contratos activos de I-49, I-52, I-55 | Ninguna regla se aplica a sus reclamos, ramas y contratos existentes; una unidad nueva reclamada despues del SHA usa archivos propios (P-25 T8) |
| H.1-5 | Sin T0–T4 | P-03, P-04, P-11 (arquetipos y materialidad no gobiernan evidencia) |
| H.1-6 | Sin R0–R4 | P-03, P-11, P-13 |
| H.1-7 | Sin Quick CI | P-11, P-13 (la poblacion del CI no cambia) |
| H.1-8 | No eliminar Owner Validation | P-14 |
| H.1-9 | No eliminar la validacion Full del Candidato | P-11, P-12 |
| H.1-10 | No relajar la identidad por SHA exacto | P-12, P-19, P-20 |
| H.1-11 | Mismo arbol no es identidad de evidencia | P-12, P-19, P-20 |
| H.1-12 | Sin merges automaticos | P-20, P-21 |

---

## 3. Ciclo de vida propuesto (P-01) `[NEW]`

El ciclo **conserva intacta la mecanica Git y de integracion V1** (WORKFLOW §1–§4, §4.5, §6) y le añade decisiones de diseño,
revision y preparacion que hoy solo existen como practica (EA H.2-13).

```text
0. INTAKE (Coordinator; planificacion, sin trabajo sustantivo)
   IDs funcionales -> agrupacion provisional (P-02) -> arquetipo provisional (P-03/P-04)
   -> preflight de paralelas derivado de Git (P-07) -> autorizacion del Owner o fila de ROADMAP (V1, WORKFLOW §2)
1. RECLAMO + BOOTSTRAP                      (V1 sin cambio; contrato con campos V2: P-02, P-03, P-06, P-07, P-25)
2. DISCOVERY CORE (+ CONDITIONAL)           (P-05) -> confirma agrupacion y arquetipo (puede elevarlo)
3. DISEÑO por arquetipo                     (P-08, P-09)
     Extension            : archivo de Freeze propio; Coordinator + revision de Freeze del Architect
     Foundation Evolution : Proposal + revision adversarial del Architect
     New Architecture     : Proposal (+ ADR propuesto) iterada Coordinator <-> Architect
4. CONSENSUS FREEZE                         (P-09) + decisiones de politica del Owner cuando apliquen
                                            + matriz OV (P-14)
5. GATES DE IMPLEMENTACION                  (P-10, P-11) unidades de comportamiento; revision del Coordinator por gate
6. PREPARACION DEL CANDIDATO                (P-12 READY-01..09; incluye rebase final V1 y conformidad P-19)
7. FINAL_CANDIDATE_SHA + evidencia V1       (AGENTS «Pruebas» punto 1; guia §7.1) + Owner Validation donde se active
8. INTEGRACION                              (V1 WORKFLOW 4.5.4–4.5.7 + re-fetch antes del merge + cierre concentrado P-15, P-20)
9. LIMPIEZA (V1, tras ambas compuertas)     + tag anotado integration/<unidad> (P-20) + informe final (plantilla E)
```

**El intake es planificacion** (momento 1 de WORKFLOW §2): usa ROADMAP, `ideas-futuras.md`, el registro de fundaciones y Git; **no
lee codigo ni mide** (EA O-9; WORKFLOW §2 «Ningun trabajo sustantivo antes de que el bootstrap este versionado»). Lo que el intake
deja UNKNOWN lo resuelve el Discovery Core despues del bootstrap.

En una **iniciativa conceptual con unidades de entrega** (P-02), los pasos 0, 2, 3 y 4 ocurren **una vez** para la iniciativa
conceptual, y los pasos 1 y 5–9 ocurren **por unidad de entrega**. El Discovery y el diseño conceptuales se hacen en la primera
unidad reclamada, cuyo contrato es el contrato conceptual. Cada unidad se clasifica V1 o V2 por **su propio** reclamo (P-25) y
hace su Discovery delta (P-02).

---

## 4. Tabla normativa de la propuesta

Toda regla V1 **escrita** que la V2 cambia aparece aqui marcada `[CHANGE]` y se repite en §8. «Global (H.1-3)» = sin decision
especifica del Owner, pero sujeta a la aprobacion global de la version. Cada regla tiene **un** destino normativo (P-24).

| Area | Current V1 | Proposed V2 | Reason / G1 evidence | Safety mechanism | Owner decision required? | Architect concern? | Normative destination if accepted |
|---|---|---|---|---|---|---|---|
| Apertura / bootstrap | Fila de ROADMAP o autorizacion (d); reclamo atomico; bootstrap = contrato + fila (WORKFLOW §2, §4.1) `[KEEP]` | Git y ROADMAP sin cambio. El contrato (estado mutable, sin hashes) añade: `workflow: V1/V2` (P-25), agrupacion, arquetipo provisional con disparadores, archivos calientes conocidos, dependencias declaradas, estrategia de coordinacion y Consumes/Extends/Introduces. Una unidad de entrega nace con **contrato delta** (P-02) `[NEW]` | EA C-29 (contratos minimos con 18–22% re-enunciado y el error B-31); EA §6; EA C-24 | Reclamo, rama y worktree intactos; el delta cita secciones exactas; las puntas observadas no se guardan en el contrato (CR-07) | Global (H.1-3) | Bajo | TEMPLATE (campos); INITIATIVE_LIFECYCLE (reglas) |
| Agrupacion de IDs | Sin regla; «una iniciativa cabe en 1-3 sesiones; si crece, se parte» (WORKFLOW §2) `[KEEP]` | FUNCTIONAL ID != INITIATIVE; agrupar exige **a la vez** el mismo problema de usuario u objetivo observable **y** autoridad o fundacion de diseño materialmente compartida (normalmente por persistencia o mutacion); infraestructura compartida sola = reutilizar y secuenciar/coordinar; unidades de entrega; fundacion fundida con su primer consumidor; cada unidad se clasifica por su propio reclamo y hace Discovery delta (P-02) `[NEW]` | EA Q13 (I-53 ahorro diseño y triplico ceremonia); EA Q1; EA C-13; EA §6 (fundaciones compartidas por lineas distintas) | Cada unidad conserva reclamo, Candidato, merge y CI post-merge propios; ninguna hereda evidencia ni version de workflow | Si: OWN-J | Medio: fundir fundacion y consumidor agranda la unidad | INITIATIVE_LIFECYCLE |
| Clasificacion / arquetipos | No existe | EXTENSION / FOUNDATION EVOLUTION / NEW ARCHITECTURE por disparadores M-01..M-08; UNKNOWN = activado; solo gobierna alcance del Discovery, rondas de diseño y profundidad de la Proposal (P-03, P-04) `[NEW]` | EA Q4, Q5, Q14; EA B-01..B-05 (Discovery con valor) | No cambia CI, suites, Candidato, READY, conformidad, cobertura, OV ni exact-SHA | Si: OWN-A | **Si**: riesgo de uso como nivel de riesgo (argumento en P-03) | INITIATIVE_LIFECYCLE |
| Discovery | Practica no escrita (EA H.2-13) | DISCOVERY CORE obligatorio con lectura de codigo tras el bootstrap + DISCOVERY CONDITIONAL por EXP-01..EXP-08 autorizado por el Coordinator (P-05) `[NEW]` | EA hechos E.1 (defectos HIGH por Discovery); EA C-28; EA §6; EA Q14 (~20% del Discovery de I-54 fue re-medicion) | Core exige evidencia de codigo aun con registro; expansion documentada; nunca «auditar todo RackCad» por defecto | Si: OWN-A | Si: Core insuficiente en fundaciones fragmentadas | INITIATIVE_LIFECYCLE |
| Reutilizacion de fundaciones | ADR + ARCHITECTURE + Context Packs (obsoletos, EA hechos A.18) `[KEEP]` para ADR y ARCHITECTURE | Registro breve con punteros verificables; los consumidores lo verifican contra codigo; una discrepancia que afecta un invariante **consumido** es STOP y no se aplaza; solo lo demostrablemente irrelevante va al cierre o a ideas-futuras (P-05, P-06) `[NEW]` | EA §6; EA Q11; EA B-35 | Registro = puntero, no verdad; discrepancia = EXP-01 con clasificacion A/B; entrada redactada en el contrato y revisada por la conformidad | Si: OWN-K | **Si**: obsolescencia y propagacion de prosa erronea (Header Mutation) | FOUNDATIONS (reglas en su cabecera) |
| Coordinacion paralela / archivos calientes | Estorbos en ROADMAP; tabla de archivos calientes (WORKFLOW §7); rebase al abrir sesion (§4.2) `[KEEP]` | Preflight ligero derivado de Git; distingue dependencia funcional / archivo compartido / documento compartido de cierre; puntos de re-verificacion definidos; el contrato guarda calientes, dependencias y estrategia, **no** puntas; las puntas observadas van al cuerpo del commit o al informe de gate/sesion (P-07) `[NEW]` | EA C-24; EA B-33 (E3-C BLOCKED); EA O-4 (I-54 G7A); EA Q12 (todos los conflictos medidos fueron documentales) | Sin registro central ni estado vivo en documentos durables; un indice nunca crea dependencia funcional; sin rebases nuevos | Global (H.1-3) | Bajo | WORKFLOW §4 (puntos de re-fetch) |
| Proposal | Practica: un archivo por version; versiones delta o acumulativas | Profundidad por arquetipo; version autocontenida sin bloques de estado arrastrados; «Cambios respecto de Vn-1»; errata confirmada antes del Freeze (P-08, P-09) `[NEW]` | EA §5.1 (I-48 delta en 8 archivos; I-54 90.4% arrastrado); EA C-18 | Freeze autocontenido e identico a lo acordado; ningun hallazgo que toque un elemento congelable se trata como errata | Si: OWN-A | Medio | INITIATIVE_LIFECYCLE |
| Participacion del Architect | Practica: revision de rol dentro de la sesion executor; independencia UNKNOWN (EA 0.4) | EXTENSION independiente: revision del Freeze por el Architect; unidad de entrega bajo un Freeze ya revisado: sin revision de diseño salvo disparador; FOUNDATION EVOLUTION: 1 revision adversarial + re-revision del delta; NEW ARCHITECTURE: rondas con reglas anti-churn; modo de revision registrado (P-08) `[NEW]` | EA Q4 (primera ronda material en todas las unidades cerradas con ronda); EA Q5; EA TABLE A (E2/E3 sin rondas bajo el Freeze de I-53 {C}); EA hechos C.9 | Nunca hay diseño sin revision del Architect salvo bajo un Freeze ya revisado; no se afirma independencia no provista | Si: OWN-B | **Si** | INITIATIVE_LIFECYCLE |
| Consensus Freeze | Practica («congelacion»); acuerdo repartido en varios archivos (I-48 V8) | Elementos congelados y no congelados; **artefacto de Freeze en archivo propio e inmutable** en todos los arquetipos (EXTENSION: `<I>-freeze.md`; FE/NA: version congelada de la Proposal); enmiendas A-n en el registro de decisiones; invalidacion clasificada por quien decide (P-09) `[NEW]` | EA §5.1; EA C-14; EA C-23 (16 menciones de integridad del blob) | La integridad se prueba porque el archivo no recibe commits tras el Freeze; contrato y evidencia viven en otros archivos | Si: OWN-B | Si | INITIATIVE_LIFECYCLE |
| Gates de implementacion | TEMPLATE §8: «cada fase termina con evidencia revisable»; practica de micro-gates por capa | Gates = unidades de comportamiento verificables; varios commits por gate; criterios de division; cierre con RED→GREEN focal, suite Core local, CI leido y revision del Coordinator; sin commits `-CLOSE` obligatorios (P-10) `[NEW]` | EA Q6 (primer gate verificable: I-50 commit 17 de 20; I-54 solo G7); EA hechos E.3, E.4; EA B-12; EA O-6 | RED focal preservado; CI del cierre leido antes de abrir el siguiente gate | Si: OWN-G | Medio | INITIATIVE_LIFECYCLE |
| Pruebas de iteracion | AGENTS «Pruebas» punto 1 (tabla): suite Core en local en la iteracion ordinaria «sin cambio» junto a UI «NO obligatoria antes del push», es decir, Core local antes de cada push; 0 seleccionadas = FALLO | Focales + relevantes durante el gate; **suite Core completa en local obligatoria al cierre de cada gate** sobre su SHA, no antes de cada push interno; LC-UI y 0 seleccionadas = FALLO sin cambio (P-11) `[CHANGE]` | EA C-08..C-11 (Core repetido por orden de commit y habito); EA Q7; EA B-17 y B-12 (plataforma y runner los detecto el CI) | CI push ejecuta Core y UI completos sobre cada punta empujada; suite Core local al cierre de gate y en el Candidato | Si: OWN-C | **Si**: Core Windows vs Ubuntu; pushes agrupados | AGENTS «Pruebas» |
| Suites Full | Candidato: Core + UI Full local + builds + CI exacto; cierre y gates que exigen Full (AGENTS punto 1; guia §7.1) `[KEEP]` | Sin cambio en Candidato ni cierre. Un cierre de gate V2 no es «gate que exige Full». H3 fuerte (Core local solo en el Candidato) **no se adopta** (P-11) | EA Q7, Q14 (valor contrafactual UNKNOWN; AGENTS excluye el Core de LC-UI por clase) | Validacion Full del Candidato intacta (EA H.1-9) | Si: OWN-C (confirmar la no adopcion) | Si | AGENTS «Pruebas» |
| Declaracion del Candidato | Candidato = SHA exacto entregado para validar o integrar; rebase final antes (WORKFLOW 4.5.1–4.5.2; AGENTS; guia §7.1) `[KEEP]` | Umbral READY-01..READY-09 antes de fijar `FINAL_CANDIDATE_SHA`; toda entrega al Owner es un Candidato con evidencia V1 completa; commits parciales no se entregan ni se llaman Candidato (P-12) `[NEW]` | EA Q9 (`acecde6` declarado con CI rojo); EA B-33 (base superada); EA Q8 | Evidencia V1 del Candidato sin reduccion; todo SHA nuevo invalida | Si: OWN-D | Si: orden entre conformidad y evidencia | INITIATIVE_LIFECYCLE (READY); guia §7.1 (bloque) |
| Cobertura | Sin cobertura en push ordinario; con cobertura en push a `main` y dispatch del Candidato (WORKFLOW 4.5.2.bis, 4.5.6, 4.5.7; `ci.yml`) `[KEEP]` | Sin cambio. Aclaracion: nunca se despacha cobertura sobre `MERGE_SHA`. El doble dispatch del Candidato queda en U-03 (P-13) | EA C-06 (orden erronea), EA C-07 | Ninguna retirada de cobertura | Si: OWN-I (confirmar **sin cambio**) | Bajo | WORKFLOW 4.5.7 (aclaracion) |
| Owner Validation | Disparador = cambia dibujo; metadata monotonica; formato de evidencia; reutilizacion por mismo SHA, proposito y alcance (AGENTS punto 5 y «Reutilizacion»; AUTOMATION_PLAN §11; guia §6–§8) `[KEEP]` | Matriz OV-nn en el Freeze, **aditiva** al checklist de la guia; solo se amplia sin Owner; retirar escenarios exige Owner; comprobacion del DLL entregado; separacion POLICY DECISION / PRODUCT VALIDATION (P-14) `[NEW]` | EA B-13 (DLL anterior al Candidato); EA B-38; EA 0.5 (veredictos transcritos fuera de sesion) | No elimina ni reduce (EA H.1-8; EA H.3-6 no reabierta) | Si: OWN-M | Bajo | guia de validacion §7 |
| Cadencia documental | HANDOFF solo al integrar; ROADMAP en 3 momentos; guias «en la misma rama»; ideas-futuras «al detectarlo»; ADR antes de implementar; conteos y hashes solo en HANDOFF §12; registro de la ronda del Candidato en el contrato o en el cuerpo del commit (WORKFLOW §2, §8; AGENTS; README de iniciativas; guia §7.1) | Tres superficies: contrato mutable sin hashes; Freeze inmutable; **evidencia operativa en `docs/automation/evidence/<unidad>-evidence.md`**. Guias y README en el ultimo gate; indices, ideas-futuras, registro y HANDOFF en el commit de cierre; hechos posteriores al merge en el tag `integration/<unidad>`; sin marcadores PENDING (P-15) `[CHANGE]` | EA §5.3 (HANDOFF +43%); EA Q12; EA C-25; EA B-36 (§8-12 inexistente); EA Q10; EA TABLE A (I-48 «MERGE_SHA = PENDING»); EA 0.5 (I-51) | HANDOFF sigue solo al integrar; el archivo ADR sigue antes de implementar; documentacion visible antes del Candidato (AGENTS punto 4); el contrato conserva la regla V1 «sin hashes» | Si: OWN-K | Si | WORKFLOW §8 |
| Prompts del Coordinator | Sin norma; ordenes re-enuncian en parafrasis | REFERENCE OVER REPETITION con referencia precisa y completa; campos obligatorios; STOP ante contradiccion; plantillas A, B y F (P-16, P-18) `[NEW]` | EA §5.4; EA O-1..O-10; EA hechos C.6 (incluida la contraevidencia de la celda ROADMAP) | Una orden no redefine autoridad superior; nombra las reglas que el gate ejerce; condiciones de parada especificas obligatorias | Si: OWN-F | **Si**: ordenes cortas que omiten paradas | PROMPT_TEMPLATES |
| Prompts del Executor | Sin norma | Plantilla B (≈15–40 lineas como guia de la orden de G2, no limite) (P-18) `[NEW]` | EA §5.4 (60–85% especifico; 9–20% politica re-enunciada) | Correccion antes que brevedad; paradas obligatorias listadas en la plantilla | Si: OWN-F | Si | PROMPT_TEMPLATES |
| Prompts del Architect | Sin norma; «aprobar» | Plantilla C con salida estructurada y estado de consenso sin «AGREED WITH CHANGES» ambiguo (P-08, P-18) `[NEW]` | EA Q5 (control I-52: 8 de 8 «AGREED WITH Vn» seguidos de CHANGES REQUIRED) | AGREED solo sin cambios requeridos abiertos | Si: OWN-B | Si | PROMPT_TEMPLATES |
| Conformidad | Practica (I-45 CR1–CR3) | Resultado implementado vs contrato congelado; **Architect + Coordinator en todos los arquetipos**; CONFORMING / NON-CONFORMING con desviaciones clasificadas; completa sobre cada SHA nuevo; no es ronda de rediseño (P-19) `[NEW]` | EA B-26 (4 HIGH tras «NONE» del Coordinator); EA B-27; EA B-30 (invariante sin prueba); EA B-31 | Revisor independiente del arquetipo; desviacion material reabre Architect; reservada, Owner | Si: OWN-B | Si | INITIATIVE_LIFECYCLE |
| Integracion / post-merge | WORKFLOW 4.5.1–4.5.7 y §4 paso 6; nunca commit directo en `main`; sin merge automatico `[KEEP]` | Secuencia V1 intacta + cierre concentrado + **tag anotado `integration/<unidad>`** sobre el merge verificado (`FINAL_MAIN_SHA`), tras la limpieza, como registro durable post-merge + informe final `[NEW]`; re-fetch antes del merge y vuelta a 4.5.1 si `main` avanzo, sin excepciones propuestas `[CHANGE]` (precision; P-20) | EA B-32/O-1 (orden contra 4.5.7); EA O-2 (merge sin rebase citado como precedente); EA 0.5 y hechos A.20 (post-merge de I-51 sin registro) | Exact-SHA, CI post-merge y limpieza tras ambas compuertas sin cambio (EA H.1-10..12); el tag no es commit: sin recursion | Si: OWN-H | Medio: el push de tags dispara CI (medido en `archive/*`); esa corrida no es evidencia | WORKFLOW §4.5 |
| Autoridad y precedencia | WORKFLOW §10 y AUTOMATION_PLAN §2 con ordenes distintos; ordenes de gate sin lugar | Autoridad por dominio con reglas de conflicto; la orden de gate solo estrecha; camino de una decision del Owner a norma durable (P-17) `[CHANGE]` | EA O-1, O-2, O-3; EA hechos B.10 | STOP + citar ambas fuentes; la mas estricta en garantias de seguridad | Si: OWN-L | Si | WORKFLOW §10 (AUTOMATION_PLAN §2 remite) |
| Scripts futuros | No existen (salvo `eng/validation` de I-45) | Solo diseño de 4 scripts; implementacion en otra iniciativa (P-21) `[NEW]` (diseño) | EA C-24; EA hechos A.20; EA B-13 | Fail-closed; UNKNOWN = FALLO en comprobaciones requeridas; decisiones humanas explicitas | Global (H.1-3); su implementacion necesitaria iniciativa propia | Medio | Iniciativa futura |
| Metricas | Solo duracion activa experimental del Owner (guia §8) `[KEEP]` | Fila ligera por unidad en el archivo de evidencia + dos evaluaciones: general tras ~5 iniciativas V2 integradas (arquetipos ausentes = UNKNOWN) y completa por arquetipo cuando los tres tengan muestras (P-22) `[NEW]` | EA hechos D.1 (tiempo activo UNKNOWN); EA Q1 | Dato ausente = UNKNOWN, nunca fallo; nunca KPI de menos pruebas; ninguna evaluacion es compuerta de iniciativas ordinarias | Global (H.1-3); la conclusion de la evaluacion va al Owner | Bajo | INITIATIVE_LIFECYCLE |
| Transicion / vigencia | Contrato I-56 §0.1, §0.2 `[BIND]` | `WORKFLOW_V2_EFFECTIVE_SHA` = merge normativo unico de I-56 = punto de vigencia (un solo significado); pausa temporal de reclamos del Owner sin segundo momento de vigencia; clasificacion por el reclamo formal propio; reclamo tras el SHA = V2 aunque viole la pausa (desviacion + STOP) (P-25) `[NEW]` | Contrato I-56 §0 | Nada entra en vigor antes del merge normativo aprobado; nunca se reclasifica como V1 un reclamo posterior | Si: OWN-E | Si | WORKFLOW (seccion de transicion) |

---

## 5. Decisiones de la propuesta

### P-02 — FUNCTIONAL ID != INITIATIVE; agrupacion y unidades de entrega

**Categoria**: `[NEW]` regla de agrupacion y unidades de entrega; `[KEEP]` toda la mecanica Git por unidad y las filas de
ROADMAP de WORKFLOW §2 (una por unidad Git, como hizo I-53).

**Principio.** Un ID funcional (ID1, ID6, ID15…) es una **necesidad del usuario**; una iniciativa es una **unidad de diseño y de
decision**; una unidad de entrega es una **unidad Git** (rama, worktree, Candidato, merge). No son lo mismo y la V2 no las
confunde.

**Regla cualitativa (sin puntuacion).** En el intake (planificacion, sin leer codigo) el Coordinator evalua cada par de IDs
candidatos sobre siete dimensiones con ROADMAP, ADR y registro de fundaciones, y registra por dimension `igual` / `distinta` /
`UNKNOWN`; el Discovery Core de la primera unidad la **confirma con evidencia de codigo** antes del Freeze:

| Dimension | Pregunta |
|---|---|
| Objetivo de usuario | ¿Resuelven la misma necesidad observable? |
| Autoridad | ¿El mismo dueño de la regla o del valor (simbolo, ADR)? |
| Persistencia | ¿El mismo modelo persistido (DTO, Xrecord, store)? |
| Mutacion | ¿El mismo camino de cambio (reconciliacion, restamp, editor de estado)? |
| Superficie de UI | ¿La misma ventana o comando? |
| Archivos calientes | ¿Los mismos archivos de WORKFLOW §7 o de la tabla de preflight (P-07)? |
| Fundacion reutilizable | ¿Necesitan una misma fundacion que, separados, se diseñaria dos veces? |

Decision (CR-06):

1. **AGRUPAR** en una iniciativa conceptual **solo si se cumplen las dos condiciones a la vez**:
   - (a) **sustancialmente el mismo problema de usuario u objetivo observable**; **y**
   - (b) **autoridad o fundacion de diseño materialmente compartida**, normalmente acoplada por persistencia o mutacion, que
     separados se diseñaria dos veces.
2. **Infraestructura compartida sola no agrupa.** Si dos IDs comparten una fundacion, un store o un archivo caliente pero resuelven
   problemas de usuario distintos: se **reutiliza** la fundacion (Consumes/Extends, P-06) y se **secuencia o coordina** segun P-07;
   no se agrupan automaticamente.
3. **SECUENCIAR, no agrupar**, cuando comparten solo superficie de UI o archivos calientes: es conflicto de archivo, no dependencia
   funcional (P-07).
4. **SEPARAR** cuando falla (a) o (b), o cuando un ID tiene resultado visible por el Owner independiente y no necesita el diseño
   del otro.
5. **UNKNOWN en (a) o (b)** no se resuelve agrupando por conveniencia: el Discovery Core de la iniciativa candidata lo resuelve
   antes del Freeze, y el contrato registra por que agrupa o separa.

Las demas dimensiones de la tabla (superficie de UI, archivos calientes) no deciden la agrupacion: deciden **unidades de entrega** y
coordinacion.

Racional de la conjuncion: en I-53 lo que se diseño una vez fue la autoridad de la cabecera configurable **y** su reconciliacion
(mutacion), al servicio de un mismo objetivo — aplicar una cabecera configurada a varios destinos —, y eso es lo que E2 y E3
reutilizaron sin rondas propias (EA Q13); cuando dos trabajos solo comparten ventana, archivos o infraestructura, lo medido fue
conflicto documental o de archivo, o re-descripcion de la misma fundacion por lineas distintas, no diseño compartido (EA Q12, §6).

**Unidades de entrega.** Dentro de una iniciativa conceptual, se definen unidades de entrega cuando las superficies de UI, los
sistemas, los archivos calientes o los limites de rollback difieren. Cada unidad:

- tiene **resultado verificable por si misma** (prueba observable o hito visible por el Owner);
- conserva **toda** la mecanica V1 de una unidad Git: reclamo atomico, rama, worktree, rebase, Candidato, cierre, merge
  `--no-ff`, CI post-merge, cobertura del Candidato y limpieza `[KEEP]`;
- nace con un **contrato delta** (mutable): encabezado, porcion de alcance, subconjunto de la matriz OV, archivos calientes y puntos
  del Freeze que ejecuta, **citados por seccion** del archivo de Freeze conceptual; no re-enuncia el Freeze.

**La fundacion se funde con su primer consumidor por defecto.** Una unidad que solo entrega fundacion sin salida verificable (I-53
E1) solo existe si aplica un criterio de division de P-10 (limite de rollback, secuenciacion forzada por archivos calientes, u
otra iniciativa que la necesite integrada antes). Si existe, conserva Candidato Full, merge y CI post-merge; la Owner Validation
se decide por su disparador (cambia comportamiento de dibujo) y por la metadata monotonica, como en V1 — no por costumbre.

**Version de workflow por reclamo propio (CR-02).** Cada unidad de entrega con reclamo atomico, rama y worktree propios se clasifica
V1 o V2 **por su propio reclamo formal**, segun la regla vinculante de transicion (P-25). La version de workflow **nunca se hereda**
entre reclamos: ni por figurar la unidad en un contrato o Freeze conceptual anterior, ni por pertenecer a una iniciativa V1. Un
Freeze conceptual si puede **consumirse** por una unidad posterior. No hay opcion de entrar ni de salir: una unidad reclamada antes
de `WORKFLOW_V2_EFFECTIVE_SHA` es V1 aunque prefiriera V2, y una reclamada despues es V2 aunque su iniciativa se diseñara bajo V1.
El arquetipo de una unidad V2 parte del de su Freeze conceptual V2 y solo puede subir por los disparadores de la propia unidad; si
el Freeze consumido es V1 (sin arquetipo), la unidad se clasifica desde cero con M-01..M-08 y UNKNOWN = activado. Una unidad nueva de
una iniciativa grandfathered por nombre (I-49, I-52, I-55) lleva contrato, Freeze delta y enmiendas en **archivos propios** y nunca
escribe en los de la iniciativa V1 (P-25 T8).

**Discovery delta por unidad.** Cada unidad posterior hace, en su propia base, un Discovery Core **delta** de DC-7 (archivos
calientes), DC-8 (fundaciones verificadas contra el codigo actual, con la regla de P-06 regla 5) y DC-9 (disparadores): la base
puede haber cambiado mucho entre unidades (I-54 integro 57 archivos entre E1 y E3, EA C-12, B-33). El Freeze conceptual sigue
siendo autoridad para las unidades que lo consumen hasta que se cierre la ultima, aunque la primera ya este integrada.

**Iniciativa diseñada bajo V1 con una unidad posterior reclamada bajo V2.**

1. **No se reescribe** el Freeze V1 (contrato I-56 §0.1; el archivo congelado es inmutable, P-09).
2. La unidad V2 hace su **intake V2** (agrupacion confirmada, arquetipo, preflight) y su **Discovery delta**, que ademas comprueba
   la **compatibilidad** del Freeze V1 con lo que V2 exige congelar (P-09): autoridad, persistencia, comportamiento observable,
   semantica de fallo, compatibilidad, no-objetivos, puntos de extension, obligaciones de prueba por invariante, matriz OV y plan de
   gates.
3. **Si el Freeze V1 se puede consumir sin contradiccion**, la unidad lo **referencia**. Lo que V2 exige y el Freeze V1 no contiene
   (p. ej. la matriz OV o la obligacion de prueba de un invariante) se añade en un **Freeze delta** propio de la unidad, inmutable
   desde su propio Freeze, que solo **añade** y nunca contradice al V1.
4. **Si V2 exige un contrato materialmente distinto** — un disparador M-01..M-08 sobre un elemento congelado, o una materia
   OWNER-RESERVED — **STOP**: se resuelve por el mecanismo V2 de cambio material (P-09: re-revision del Architect) o por decision
   del Owner. No se continua consumiendo un Freeze que contradice lo que la unidad hara.
5. La revision de diseño del Architect para esa unidad sigue P-08: si el Freeze V1 tuvo revision del Architect registrada, la unidad
   es «unidad bajo Freeze ya revisado» para lo que consume; si no la tuvo, su Freeze delta y la compatibilidad reciben la revision
   del Freeze de una EXTENSION independiente como minimo.
6. La conformidad final de la unidad (P-19) es contra el Freeze V1 referenciado **mas** su Freeze delta **mas** las enmiendas.

**ROADMAP `[KEEP]`.** Una fila por unidad Git (WORKFLOW §2), cada una marcada en su momento. El problema medido de la fila de E1
(«integrada» 11 h antes de que ID6/ID7 fueran visibles, EA Q13) lo resuelve la fusion por defecto, no un cambio de ROADMAP.

**Caso principal: I-53** (EA Q13, EA Q1, EA C-13, EA C-29, EA B-31, EA O-1).

| | Evidencia |
|---|---|
| Lo que la agrupacion ahorro | Un Discovery, dos Proposals, un ADR (ADR-0037) y un registro de decisiones para 2 IDs en 2 sistemas; E2 y E3 sin rondas de diseño ni desviaciones de diseño hacia atras; nucleo compartido sin cambios desde G3; el rebase de E3 no re-valido E2 |
| Ceremonia que introdujo E1/E2/E3 | 3 reclamos, 3 bootstraps, 3 cierres, 3 merges, 7 corridas en `main`, 6 ediciones de ROADMAP, ~490 lineas de HANDOFF; E1 sin salida verificable pero con Candidato completo y smoke en AutoCAD sobre codigo sin llamadores (C-13); fila «integrada» 11 h antes de que ID6/ID7 fueran visibles; la particion tomo 5 formas; desviaciones de proceso concentradas en E1 (O-1); contratos minimos con 18–22% re-enunciado y el error B-31 |
| Como lo haria la V2 | Iniciativa conceptual I-53 con Discovery, Proposal, Freeze y decisiones **una vez**; E1 **fundida** con E2 (primera unidad visible: fundacion + UI Selectivo) salvo que un criterio de division de P-10 lo impida; E3 como segunda unidad; contratos delta sin re-enunciado; filas de ROADMAP segun V1; la ceremonia Git y la evidencia de Candidato se pagan por unidad **sin reduccion**; con E1 fundida desaparecen un reclamo, un bootstrap, un cierre, un merge con su CI post-merge y el smoke sobre codigo sin llamadores |
| Coste aceptado | La primera unidad es mas grande (E1 tardo 17h17 de reloj frente a 5h16 de E2); si otra iniciativa necesitara la fundacion integrada antes, el criterio de division lo permite |

**Re-prueba de la regla endurecida (CR-06).**

| Caso | (a) Mismo problema de usuario | (b) Autoridad/fundacion de diseño compartida | Resultado | ¿Coherente con la historia? |
|---|---|---|---|---|
| I-53: ID6 (reutilizar) + ID7 (distribuir) | Si: aplicar la configuracion de una cabecera a un conjunto de destinos (fila de ROADMAP de I-53) | Si: autoridad de la cabecera configurable y su reconciliacion, diseñadas una vez (ADR-0037; EA Q13) | **Agrupar**; Selectivo y Dinamico como unidades de entrega | Si: el ahorro medido vino de ese diseño unico |
| I-50: ID1 en 3 sistemas | No aplica agrupar IDs: es un solo ID; (a) y (b) confirman que los tres sistemas no son tres iniciativas (misma politica de cotas, ADR-0035, un modelo persistido) | — | **Una iniciativa**; los sistemas son gates o unidades de entrega | Si: I-50 fue una iniciativa; lo que cambia es la forma de los gates (P-10) |
| I-50 (cotas por vista) e I-51 (RACKDUPLICAR multiorigen) | No: problemas distintos | Comparten infraestructura: ambas re-describieron View Identity y el restamp de identidad, que I-51 ademas extendio (EA §6) | **No agrupar**: reutilizar la fundacion y coordinar | Si: fueron separadas; su merge solo conflictuo en documentos (EA 0.6 punto 4) |
| I-47 (Project Variables) e I-54 (Custom Properties) | No: variables de proyecto que gobiernan valores vs metadatos del usuario | Comparten infraestructura de identidad y persistencia del rack (EA §6: Rack Identity extendida por ambas) | **No agrupar**: I-54 consume y extiende la fundacion | Si: fueron separadas, con ADR propio cada una |

**I-55, con prudencia.** Activa y grandfathered: esta propuesta **no se aplica** a su reclamo, rama y contrato existentes (EA H.1-2, H.1-4; una unidad nueva reclamada despues del SHA seria T8, P-25). Solo se registra que la
regla preguntaria si la colocacion (ID17/ID18) y la proyeccion por familia de marco (ID19) comparten autoridad; su resultado es
UNKNOWN (EA Q13).

**Relacion con WORKFLOW §2 «1-3 sesiones».** Se conserva `[KEEP]`; no es criterio de agrupacion (no mide acoplamiento). Su
posible sustitucion por los criterios de division queda en U-05.

**Seguridad.** Agrupar solo reduce diseño y documentacion repetidos; ninguna unidad hereda evidencia de otra (exact-SHA, EA
H.1-10), y el merge y el CI post-merge siguen siendo por unidad.

### P-03 — Arquetipos de iniciativa

**Categoria**: `[NEW]`. **Veredicto sobre la hipotesis inicial: ADOPTADA CON MODIFICACIONES.**

**Que gobierna, y solo esto:** alcance esperado del Discovery (P-05), forma de la revision de diseño del Architect antes del
Freeze (P-08) y profundidad de la Proposal (P-09).

**Que NO gobierna, nunca:** poblacion del CI, cadencia de suites, umbral READY, evidencia del Candidato, conformidad final y su
revisor, cobertura, disparador de Owner Validation, identidad por SHA, revision del Coordinator entre gates. Ningun mecanismo
selecciona ni omite pruebas por arquetipo (no es taxonomia de pruebas, EA H.3-3).

**Por que no es R0–R4 ni T0–T4 bajo otro nombre** (EA H.1-5, H.1-6). El rechazo historico de esos modelos se apoyaba en que un
riesgo autodeclarado, sin mecanismo que lo fuerce, termina reduciendo evidencia en silencio (ADR-0033, Alternativas, como registro).
Los arquetipos se separan de eso en tres puntos comprobables: (1) se deciden por disparadores observables sobre autoridad,
persistencia y contratos, con UNKNOWN = activado, no por riesgo ni tamaño declarados; (2) **todo lo que protege el producto es
identico en los tres** — CI, suites, READY, evidencia del Candidato, conformidad por el Architect, Owner Validation, post-merge —;
(3) lo unico que varia es cuanto diseño se revisa antes de congelar, y hasta la EXTENSION independiente recibe revision del
Architect (P-08). Si una version futura hiciera depender de un arquetipo cualquier elemento del punto (2), dejaria de cumplir esta
propuesta.

**Definicion por caracteristicas observables** (disparadores de P-04):

| Arquetipo | Caracteristicas observables | Disparadores |
|---|---|---|
| **EXTENSION** | Consume fundaciones aceptadas e integradas; sin autoridad nueva; sin modelo de persistencia nuevo ni cambio de semantica persistida; sin cambio de semantica de fallo; sin cambio de contrato de compatibilidad; no modifica un contrato reutilizable ni un punto de extension | Ninguno de M-01..M-08 |
| **FOUNDATION EVOLUTION** | Cambia materialmente un contrato reutilizable existente, una autoridad, la semantica de persistencia o un punto de extension | Alguno de M-01..M-06 u M-08 sobre algo **existente** |
| **NEW ARCHITECTURE** | Introduce una autoridad transversal nueva, un modelo de persistencia nuevo, un framework, un registro o un contrato arquitectonico nuevo | M-07, o M-01/M-02 que **crean** (no modifican) una autoridad o persistencia transversal |

**Modificaciones respecto de la hipotesis:**

1. **Clasificacion fail-closed.** Un disparador cuya respuesta sea UNKNOWN cuenta como activado hasta que el Discovery Core lo
   resuelva con evidencia. Ante duda entre dos arquetipos, el superior.
2. **Direccion.** Antes del Freeze, el arquetipo puede subir o bajar con evidencia registrada (bajar exige Coordinator y, si
   participo, Architect). **Despues del Freeze solo sube**; un disparador activado durante la implementacion es una desviacion
   MATERIAL (P-19) y reabre la revision que corresponda al arquetipo nuevo.
3. **Mezcla.** Una iniciativa conceptual toma el arquetipo del ID o unidad mas alto; cada unidad de entrega re-comprueba sus
   disparadores en su Discovery delta y nunca queda por debajo de lo que su propio diff activa. El arquetipo se consume del Freeze
   conceptual; la **version de workflow no** (P-02, P-25).
4. **Quien decide.** El Executor propone con evidencia en el Discovery Core; el Coordinator decide; el Architect puede elevar.
   Nadie clasifica por «tamaño» ni por «riesgo declarado».

**¿Bastan tres?** Se evaluaron alternativas:

| Candidato a cuarto arquetipo | Evidencia | Resultado |
|---|---|---|
| «Trivial / fix pequeño» con menos proceso | I-51 fue pequeña (3 gates) y su Discovery hallo 3 HIGH, uno de ellos en el restamp compartido (EA B-01..B-03) | **Rechazado**: tamaño no es materialidad; seria un nivel por autodeclaracion |
| «Refactor mecanico» | I-23 (fuera de la muestra de G1; dato de su contrato) movio 176 archivos sin logica | No hace falta: sin disparadores es EXTENSION; su riesgo real (archivos calientes) lo cubre P-07, no el arquetipo |
| «Proceso / gobierno» (I-45, I-56) | I-45 cambio normas, CI y AGENTS (EA TABLE A) | No hace falta: los disparadores se aplican a contratos normativos (cambiar una norma de proceso compartida = M-05/M-07 por analogia); ademas requiere decision de politica del Owner |
| «Experimento» | WORKFLOW §4 ya da a `experiment/*` un cierre propio | Fuera del ciclo; sin cambio |
| «Multi-ID» | I-53, I-50 | Es agrupacion (P-02), no arquetipo |

**Conclusion**: tres arquetipos bastan, siempre que se definan por disparadores y no gobiernen evidencia.

### P-04 — Regla de materialidad

**Categoria**: `[NEW]`. Una misma definicion sirve para clasificar (P-03), invalidar el Freeze (P-09) y clasificar desviaciones
(P-19).

**MATERIAL CHANGE** = cualquier cambio, propuesto o implementado, que active uno de estos disparadores:

| ID | Disparador | Ejemplo en la muestra |
|---|---|---|
| M-01 | **Autoridad**: cambia quien posee un valor o regla, o aparece una segunda autoridad para el mismo dato | I-48 `FindBroken` elegia el primer hermano en vez de la autoridad authored (EA B-40) |
| M-02 | **Persistencia**: cambia esquema, DTO o wire, el significado de un campo persistido, el fallback legacy o la preservacion de campos desconocidos | I-48 `Enum.TryParse` amplio la gramatica persistida (EA B-10); I-54 `Compose` descartaba un campo del sobre (EA B-04) |
| M-03 | **Compatibilidad observable**: cambia el comportamiento visible de un flujo existente (dibujo, BOM, round-trip, comandos) o el trato a documentos existentes, fuera de lo pedido | I-50 V1 leia enteros negativos como legacy (EA B-14) |
| M-04 | **Semantica de fallo**: cambia que falla o como (silencioso/visible, abortar/normalizar, fail-closed) | I-48 `UnlinkAllAndDelete` fabricaba un vinculo roto y reportaba Success (EA B-06) |
| M-05 | **Contrato entre sistemas**: cambia algo que otros sistemas o kinds consumen | I-51 GUID generado dentro del restamp compartido (EA B-01) |
| M-06 | **Punto de extension**: añade, cambia o retira un punto que otras iniciativas consumen o consumiran | I-54 extension del sobre (EA TABLE A) |
| M-07 | **Framework o registro nuevo**: registro, kernel, shell o mecanismo generico transversal | I-48 kernel de propiedades vinculables (EA TABLE A) |
| M-08 | **Decision aceptada**: modifica, reinterpreta o contradice un ADR aceptado o una entrada del registro de fundaciones | I-54: ADR-0039 acoto el alcance de ADR-0035 (DimensionViews; EA §6) |

**Normalmente NO material** (salvo que rompa un invariante congelado): nombres de helpers; partir un metodo privado; mover
archivos mecanicamente; tecnica local de implementacion que preserva los invariantes congelados; añadir pruebas; endurecer una
validacion sin cambiar la semantica de fallo congelada; redaccion.

**Escalada.** Un disparador activado: (a) antes del Freeze, fija el arquetipo; (b) despues del Freeze, es desviacion MATERIAL
que exige la revision de P-09. Un cambio que ademas altera alcance, no-objetivos, una decision del Owner o un escenario OV
retirado es OWNER-RESERVED.

### P-05 — DISCOVERY CORE + DISCOVERY CONDITIONAL

**Categoria**: `[NEW]` (codifica practica no escrita, EA H.2-13). **Evidencia**: el Discovery con lectura de codigo capturo
defectos HIGH antes del diseño (EA B-01..B-05; EA hechos E.1); a la vez, fundaciones re-descritas 4–6 veces (EA §6, C-28) y ~20%
del Discovery de I-54 fue re-medicion de paralelas (EA Q14).

**DISCOVERY CORE — salidas obligatorias** (todas las iniciativas; en EXTENSION suele ser todo el Discovery). Cada afirmacion lleva
clase de evidencia (MEASURED / RECONSTRUCTED / INFERENCE / UNKNOWN), el vocabulario que la muestra re-invento 6 veces (EA §6):

| # | Salida | Contenido minimo |
|---|---|---|
| DC-1 | Comportamiento observable actual | Que hace hoy el flujo afectado, con referencia a codigo y, si existe, a prueba |
| DC-2 | Autoridad | Simbolo(s) dueño(s) de cada regla o valor tocado; ADR o entrada de registro que la gobierna |
| DC-3 | Persistencia | DTO/Xrecord/store, campos, fallback legacy, preservacion de desconocidos |
| DC-4 | Camino de mutacion | Entrada → estado → persistencia → dibujo, con simbolos |
| DC-5 | Consumidores directamente afectados | Llamadores a un salto y consumidores entre sistemas del contrato tocado |
| DC-6 | Pruebas protectoras | Pruebas y guardas existentes que cubren DC-1..DC-5; huecos declarados |
| DC-7 | Archivos calientes activos | Tabla de P-07 |
| DC-8 | Fundaciones consumidas o extendidas | Entrada de registro citada **y verificada contra el codigo** en la base de la sesion (simbolos y pruebas existen; comportamiento descrito coincide) |
| DC-9 | Disparadores M-01..M-08 | Activado / no activado / UNKNOWN, con evidencia; arquetipo propuesto |

**Disparadores de expansion (DISCOVERY CONDITIONAL).** Solo se abre una expansion cuando el Core expone una dependencia material
no resuelta:

| ID | Disparador |
|---|---|
| EXP-01 | El registro de fundaciones, un ADR aceptado, un Freeze o el codigo se contradicen entre si (P-06 regla 5). Se clasifica **A** (afecta algo que la iniciativa consume: STOP) o **B** (demostrablemente irrelevante para todo invariante consumido: aplazable) |
| EXP-02 | Autoridad ambigua: dos candidatas o ninguna |
| EXP-03 | Camino legacy o de persistencia no localizable desde el Core |
| EXP-04 | El contrato tocado tiene consumidores mas alla de un salto o en otro sistema (M-05) |
| EXP-05 | Un invariante candidato a congelarse no tiene prueba protectora ni forma conocida de probarlo |
| EXP-06 | Semantica de fallo no determinable (posible fallo silencioso) |
| EXP-07 | Una rama activa toca la misma autoridad o contrato (dependencia funcional de P-07) |
| EXP-08 | Deuda o defecto preexistente en el camino de mutacion que el cambio haria visible o empeoraria |

Una ambigüedad de **comportamiento deseado** no es expansion: es decision de alcance y va al Owner.

**Autorizacion.** El Executor la pide con: disparador, pregunta concreta, areas o simbolos acotados y criterio de salida. La
autoriza el **Coordinator**; en FOUNDATION EVOLUTION y NEW ARCHITECTURE el Architect puede recomendarla u objetarla. Nunca se
autoriza «auditar todo RackCad».

**Registro del POR QUE.** Seccion «Expansiones» del Discovery:

```text
EXP-id | Disparador | Pregunta | Autorizada por | Area acotada | Resultado | Cerrada: SI / UNKNOWN con dueño de la decision
```

**Criterio de salida del Discovery.** DC-1..DC-9 presentes con clase de evidencia; cada expansion cerrada con respuesta o
convertida en UNKNOWN explicito con quien lo decide; arquetipo confirmado por el Coordinator; y **ninguna discrepancia EXP-01 de
clase A abierta**. Una discrepancia A no se convierte en UNKNOWN para salir: se resuelve antes (P-06 regla 5). Una UNKNOWN sobre si
una discrepancia es A o B cuenta como **A** (fail-closed).

**Salvaguarda.** El registro de fundaciones **no** exime de DC-8: consumir una fundacion exige verificarla contra el codigo en la
base actual. La evidencia de la muestra lo justifica: la prosa de Header Mutation ya era erronea y un registro construido con ella
la habria propagado (EA §6).

### P-06 — Registro de fundaciones (diseño; NO se crea en G2)

**Categoria**: `[NEW]`. **Evidencia**: EA §6, EA Q11 (Rack Identity y View Identity re-descritas en 6 de 9 lineas; restamp 5;
Project Variables 5), EA B-35 (el mismo comentario erroneo registrado 5 veces), EA hechos A.18 (Context Packs sin cambios desde
julio y sin uso como registro).

**Ubicacion normativa recomendada: `docs/FOUNDATIONS.md`.** Verificado en el arbol: `docs/architecture/` **no existe**; los
documentos normativos globales viven en la raiz de `docs/` con nombre en mayusculas (`ARCHITECTURE.md`, `WORKFLOW.md`,
`ROADMAP.md`). Un directorio `docs/architecture/` junto al archivo `docs/ARCHITECTURE.md` confundiria sin aportar. La alternativa
`docs/architecture/foundations.md` queda como U-02. `ARCHITECTURE.md` §4 enlazaria al registro en vez de duplicarlo.

**Formato de entrada** (deliberadamente breve; orientativo ≤ 15 lineas):

```text
Name:               <nombre estable>
Status:             STABLE | SUPERSEDED (-> <entrada>)
Authority:          <simbolos dueños>
Persistence:        <DTO / Xrecord / store; campos clave; legacy>
Mutation contract:  <como se cambia; que preserva>
Extension point:    <como se extiende; que NO se debe hacer>
Decision source:    <ADR aceptado> / <iniciativa integrada y seccion de su archivo de Freeze>
Protecting tests:   <clases de prueba / guardas>
Known limitations:  <deuda conocida; p. ej. comentario RackBlockData.cs:7-11>
Last changed by:    <iniciativa> <fecha>   (sin hashes: WORKFLOW §8)
```

**Reglas:**

1. **Elegibilidad**: integrada en `main`, con ADR aceptado o Freeze de una iniciativa integrada, y con punto de
   extension declarado o al menos un consumidor fuera de la iniciativa que la creo.
2. **Quien añade o cambia**: solo la iniciativa que la **introduce** o la **extiende**. El texto de la entrada se **redacta en el
   contrato (superficie mutable, P-15) antes de READY-06**, de modo que la conformidad del Architect (P-19) lo revise contra el codigo; el commit de cierre
   (P-15) lo copia **literalmente** al registro. Un consumidor que solo la **consume** no edita la entrada. Ninguna rama edita el
   registro fuera de su cierre.
3. **Evolucion**: la entrada se actualiza en el cierre de la iniciativa que la cambia; la historia vive en Git y en la fuente de
   decision, no en la entrada. **«En evolucion» no es un estado escrito**: se deriva de Git en el preflight (P-07) — ramas activas
   cuyo contrato declara `Extends:` esa entrada —, asi que nunca queda obsoleto ni obliga a editar un documento compartido a mitad
   de una iniciativa. Los contratos V1 (grandfathered) no tienen ese campo: para ellos el preflight informa **UNKNOWN**, nunca «no
   en evolucion». La poblacion inicial no marca ninguna iniciativa activa.
4. **Verificacion por consumidores**: DC-8 verifica simbolos, pruebas y comportamiento descrito en la base de la sesion y lo
   registra **en su propio Discovery** («verificada en esta sesion: si/no, discrepancias»), no en el registro.
5. **Registro, ADR, Freeze y codigo en desacuerdo (CR-05)**: el codigo manda sobre **lo que es**; el ADR aceptado o el Freeze
   congelado mandan sobre **lo que debe ser**. Todo desacuerdo es EXP-01 y se registra en el Discovery con su clase:

   - **A — afecta lo consumido.** Si la discrepancia afecta una premisa, autoridad, regla de persistencia, garantia de mutacion,
     semantica de fallo, punto de extension o prueba protectora **que la iniciativa consume**: **STOP**. No se aplaza a
     ideas-futuras ni al cierre. Se resuelve antes de continuar por una de estas vias: **arreglo previo** (dentro del alcance, o
     como iniciativa prerrequisito integrada antes), **revision de cambio material** (P-09; el Architect, y reclasificacion si se
     activa un disparador) o **decision del Owner o del Architect** segun la materia (P-17). Hasta entonces no hay salida del
     Discovery ni cierre de gate que dependa de ello.
   - **B — demostrablemente irrelevante.** Solo si el Discovery **demuestra**, citando DC-2..DC-6, que la discrepancia no toca
     ningun invariante consumido, se puede aplazar: se registra en la lista de hallazgos y pasa a `ideas-futuras.md` en el commit de
     cierre, anotando la entrada del registro si la hay. La clasificacion B **no se autodeclara**: la confirma el Coordinator (y el
     Architect en FOUNDATION EVOLUTION y NEW ARCHITECTURE); sin esa confirmacion cuenta como A.
   - «Fuera de alcance para arreglar» **no** equivale a «seguro ignorar». Una clasificacion dudosa cuenta como A.
   - Nunca se «arregla» la entrada para que coincida con un codigo que contradice un ADR aceptado.
   - La misma regla rige si la discrepancia aparece durante la implementacion: es desviacion MATERIAL (P-19) y detiene el gate.
6. **Deteccion de obsolescencia**: manual y obligatoria hoy — cada DC-8 comprueba que los simbolos y pruebas citados existen y
   que el comportamiento descrito coincide; una discrepancia es EXP-01 —; `Last changed by` solo lo actualiza quien introduce o
   extiende, y significa ultimo cambio, no ultima verificacion. Una comprobacion mecanica futura (`evidence-report.ps1`, P-21) seria ayuda, no requisito.
7. **Declaracion en el contrato**: `Consumes:`, `Extends:`, `Introduces:` con nombres de entradas.
8. **Poblacion inicial**: la hace la integracion normativa de la V2 (P-24), **construida por verificacion de codigo, no copiando
   prosa**. Candidatas por evidencia: Rack Identity; View Identity (tabla View×Section, re-derivada 3 veces); RACKDUPLICAR/restamp;
   Project Variables; Authored vs Effective (desambiguacion); Custom Properties; DimensionViews; Header Mutation/Reconciliation
   (desde el codigo; la prosa tenia 5 discrepancias); inventario de sistemas/kinds; preservacion de campos desconocidos del sobre
   (I-11); Linked Properties.

**Anti-enciclopedia.** Sin narrativa, sin historia, sin conteos, sin hashes, sin copia de ADR. Si una entrada necesita mas de lo
que cabe en los campos, el detalle pertenece al ADR o al contrato citado.

### P-07 — Preflight de paralelas y archivos calientes

**Categoria**: `[NEW]`; `[KEEP]` rebase al abrir sesion (WORKFLOW §4.2), estorbos del ROADMAP y tabla §7.

**Evidencia**: re-medicion de paralelas en cada gate con resultado mayormente «sin impacto» (EA C-24: I-51 ~16; I-54 ×8); a la vez
capturas reales en momentos precisos (EA B-33 E3-C BLOCKED; EA O-4 I-54 G7A); todos los conflictos medidos fueron documentales
(EA Q12).

**Mecanismo ligero (sin registro central).** Derivado de Git cuando se necesita:

1. `git fetch --all --prune`; ramas remotas activas; para cada una, rutas cambiadas `merge-base..punta`.
2. Interseccion con las areas probables de la iniciativa (DC-4/DC-5 o, antes del Discovery, el alcance del ROADMAP) y con WORKFLOW
   §7.
3. Clasificacion de cada interseccion:

   | Tipo | Definicion | Tratamiento |
   |---|---|---|
   | **Dependencia funcional** | Ambas consumen o cambian la misma autoridad o contrato (DC-2, entrada de registro) | Secuenciar, o dividir fronteras con acuerdo explicito; EXP-07 |
   | **Conflicto de archivo compartido** | Mismo archivo de producto sin autoridad comun | Coordinar (quien toca que y en que orden) o secuenciar si es archivo caliente |
   | **Documento compartido de cierre** | `ROADMAP.md`, `HANDOFF.md`, `ideas-futuras.md`, `adr/README.md`, `initiatives/README.md`, `FOUNDATIONS.md` | **Nunca** es dependencia funcional por si mismo; se edita en el commit de cierre (salvo los momentos 1 y 2 de ROADMAP, WORKFLOW §2, y las actualizaciones de la tabla de WORKFLOW §7 al mover archivos calientes) y se resuelve en el rebase de integracion (serializada, WORKFLOW 4.5). Que dos iniciativas **extiendan la misma entrada** de `FOUNDATIONS.md` si es dependencia funcional (fila 1) |

4. **En el contrato (durable, CR-07)** solo lo que no envejece con cada push: `Archivos calientes conocidos | Dependencias
   declaradas (iniciativa y autoridad) | Estrategia de coordinacion (independiente / coordinar / secuenciar / dividir)`. **Sin
   puntas ni SHAs de ramas**: el contrato no es almacen de estado vivo.
5. **Evidencia de preflight (operativa)**: las puntas observadas de `origin/main` y de las ramas intersecantes, con la fecha de la
   consulta, van al **cuerpo del commit** de la sesion o gate y al **informe de gate/sesion**. Solo las que son evidencia de un SHA
   exacto relevante — la base del Candidato (READY-04) — pasan al archivo de evidencia (P-15).

**Puntos de re-verificacion (no en cada gate):** apertura de sesion (V1); antes de la primera edicion de un archivo de la
interseccion; cierre de gate (**solo deteccion**: si una punta cambio respecto de la registrada en el ultimo commit o informe y
toca la interseccion, se detiene el gate para reclasificar; no se rebasa por esto); preparacion del Candidato (READY-04); antes del
merge (P-20). Si ninguna punta se movio, no se re-mide.

**No cambia la politica Git**: no añade rebases fuera de los de V1 (apertura de sesion e integracion).

### P-08 — Participacion proporcional del Architect

**Categoria**: `[NEW]` (hoy es practica no escrita, EA H.2-13). La hipotesis inicial se **pone a prueba** contra la evidencia en
vez de copiarse:

| Hipotesis inicial | Evidencia a favor | Evidencia en contra o limites | Resultado |
|---|---|---|---|
| Extension: Coordinator + Executor; Architect solo si el Discovery expone un problema material | E2/E3 (0 rondas propias) sin defectos de producto registrados (EA Q14) | E2/E3 trabajaron **bajo un Freeze que ya tenia 2 rondas** del Architect (I-53 {C}, EA TABLE A); G1 no tiene ninguna iniciativa independiente cuyo diseño no se revisara, y toda primera ronda que ocurrio fue material (EA Q4); la unica ronda de I-51 cambio el diseño (B-29) | **Modificada**: sin revision de diseño **solo** para una unidad de entrega bajo un Freeze ya revisado por el Architect y sin disparadores propios; una EXTENSION independiente recibe una **revision del Freeze** por el Architect |
| Foundation Evolution: revision adversarial antes de implementar; conformidad final; sin revision por gate | Primera ronda material en todas las unidades cerradas con ronda (EA Q4); la conformidad de I-45 hallo 4 HIGH tras «NONE» del Coordinator (EA B-26) | Rondas tardias de I-48 aun produjeron MATERIAL convertidos en codigo (EA Q14, C-14) | **Adoptada con cambio**: re-revision permitida, **acotada al delta**, cuando la anterior deja cambios requeridos |
| New Architecture: Coordinator ↔ Architect hasta el Freeze; conformidad final | I-48 AR2 BLOCKER; I-54 G2B 5 MATERIAL (EA B-08, B-21) | Rondas derivadas de la reconciliacion previa y mecanicas (EA Q5; EA hechos C.2); control activo I-52 con 127 hallazgos en V1..V10, 12 «aspecto nuevo» | **Adoptada con reglas anti-churn** |
| (no estaba) | — | La revision del Coordinator del Discovery G1 de I-48 hallo B-06 (origen de la adenda G1.1), y entre gates internos hallo 4 defectos materiales (EA B-10, B-11, B-40 ×2) | **Añadida**: revision del Coordinator por gate en todos los arquetipos |
| (no estaba) | — | Conformidad solo del Coordinator fallo en I-45 (EA B-26) | **Añadida**: conformidad final por el Architect **en todos los arquetipos** (P-19) |

**Regla propuesta:**

| | Unidad bajo Freeze ya revisado | EXTENSION independiente | FOUNDATION EVOLUTION | NEW ARCHITECTURE |
|---|---|---|---|---|
| Revision de diseño antes del Freeze | Ninguna nueva si su contrato delta no activa disparadores | **Revision del Freeze por el Architect**: clasificacion M-01..M-08, invariantes congelados, obligaciones de prueba, matriz OV | **1 revision adversarial** de la Proposal (obligatoria) | Rondas Coordinator ↔ Architect hasta el Freeze |
| Re-revision | Un disparador propio la saca de esta columna | Si la revision deja cambios requeridos: delta | Solo con cambios requeridos abiertos o si la reconciliacion cambio un elemento congelable; **acotada al delta** | Igual que FE, con reglas anti-churn |
| Durante la implementacion | Coordinator por gate | Coordinator por gate | Coordinator por gate; Architect ante invalidacion del Freeze (P-09) | Igual que FE; el Coordinator puede pedir revision de un gate que implementa un punto de extension congelado (motivo registrado) |
| Conformidad final (P-19) | Architect + Coordinator | Architect + Coordinator | Architect + Coordinator | Architect + Coordinator |

**Reglas anti-churn** (toda revision de diseño del Architect, incluida la revision del Freeze de una EXTENSION independiente;
evidencia EA Q5, C-14..C-17, §5.1):

1. **Clasificar cada hallazgo** al emitirlo: aspecto nuevo / corrige mecanismo previo / consecuencia de la reconciliacion
   anterior / detalle de contrato o prueba / deriva de paralelas / editorial.
2. **Requerido vs opcional por materialidad, no por etiqueta de severidad** (las escalas HIGH/MEDIUM/LOW variaron entre
   iniciativas): un hallazgo es **REQUERIDO** solo si cambia el **significado** de un elemento congelable (P-09): activa un
   disparador M-01..M-08, deja un invariante sin obligacion de prueba, o produce un comportamiento observable distinto — sea cual
   sea su severidad (B-29 fue MEDIUM y cambio el diseño; RR-01 fue MEDIUM y se incorporo al congelar, EA B-20). Precisar la redaccion
   de un elemento sin cambiar su significado, o detallar mas una prueba ya obligada, es **OPCIONAL** (errata o seguimiento): asi
   los 43 hallazgos «detalle de contrato/prueba» del control I-52 (EA B-39) no justificarian por si solos una ronda.
3. **Una ronda nueva exige al menos un hallazgo REQUERIDO abierto.** Los opcionales se acumulan como errata.
4. **Errata confirmada**: antes del Freeze, el revisor confirma un diff que contiene **solo** la errata; asi el texto congelado es
   identico al acordado (evita el «no identico byte a byte» de I-53 {C}, EA TABLE A) sin otra ronda completa.
5. **Re-revision acotada al delta**: entrada = cambios requeridos de la ronda anterior + diff de la Proposal; fuera de alcance, las
   secciones sin cambio salvo interaccion demostrada con el delta.
6. **Punto de convergencia**: cuando todos los REQUERIDOS de una ronda son «consecuencia de la reconciliacion anterior» o
   «detalle», el Coordinator registra si la causa es un contrato que congela lo que P-09 no manda congelar y lo poda antes de pedir
   otra ronda. No es un limite numerico de rondas: la evidencia no justifica un numero.
7. **Estado de consenso sin ambigüedad**: `AGREED` solo sin REQUERIDOS abiertos; `CHANGES REQUIRED` en otro caso; `BLOCKED — OWNER
   DECISION` cuando falta una decision reservada. «AGREED WITH CHANGES» no es estado de consenso. Evidencia: control activo I-52,
   8 de 8 «AGREED WITH Vn» del Coordinator seguidos de CHANGES REQUIRED (EA Q5).
8. **El Architect no «aprueba»**: produce la salida estructurada de la plantilla C.

**Independencia (EA 0.4, EA hechos C.9).** Toda revision de Architect de la muestra fue un cambio de rol dentro de la sesion
executor; su independencia es UNKNOWN. La V2 **no afirma independencia que no provee**. Cada revision registra su modo:

```text
Review mode: SAME-SESSION ROLE | SEPARATE SESSION (sin contexto compartido salvo los archivos citados) | EXTERNAL HUMAN
```

`Architect = AGREED` significa «la revision de rol, en el modo registrado, no deja cambios requeridos», no «revisor independiente
de acuerdo». Si NEW ARCHITECTURE debe exigir `SEPARATE SESSION` queda en U-08 (coste y beneficio no medidos; hipotesis H9 de la
auditoria, que las metricas de P-22 permiten evaluar).

### P-09 — Consensus Freeze

**Categoria**: `[NEW]` (codifica practica). **Evidencia**: acuerdo de I-48 repartido en 8 archivos, V8 no autocontenida (EA §5.1);
rondas derivadas de contratos demasiado detallados (EA Q5); integridad del blob de V8 verificada casi en cada gate (EA C-23).

**Se congela antes de implementacion sustantiva:**

| Elemento | Contenido |
|---|---|
| Autoridad | Dueños de cada regla o valor (DC-2 resuelto) |
| Persistencia | Esquema, significado, fallback legacy, preservacion |
| Comportamiento observable | Lo que el Owner y las pruebas veran |
| Semantica de fallo | Que falla, como, y que nunca se normaliza en silencio |
| Compatibilidad / legacy | Trato de documentos y flujos existentes |
| No-objetivos | Lo que la iniciativa no hara |
| Puntos de extension consumidos por otros | Forma y garantias |
| Obligaciones de prueba | Invariante → prueba o guarda que lo protegera (incluye RED esperado) |
| Escenarios del Owner | Matriz OV (P-14) |
| Plan de gates | Resultado y verificacion de cada gate, no archivos (P-10) |

**Normalmente NO se congela:** nombres de helpers, metodos, archivos exactos, detalles mecanicos, orden interno de commits.

**Artefacto de Freeze: siempre un archivo propio e inmutable (CR-03).**

| Arquetipo | Artefacto | Nota |
|---|---|---|
| EXTENSION | `docs/initiatives/<I>-freeze.md`: solo los elementos congelados de la tabla anterior, breve | Formato inspirado en el contrato vinculante de I-51 en su G2, sin archivos de Proposal (EA TABLE A); ejemplo de formato, no de arquetipo: I-51 seria FOUNDATION EVOLUTION |
| FOUNDATION EVOLUTION / NEW ARCHITECTURE | La version **autocontenida** de la Proposal que se congela | Ya es un archivo propio; no se crea otro |
| Unidad de entrega | Consume el Freeze conceptual por referencia; si añade elementos (P-02), `docs/initiatives/<unidad>-freeze-delta.md`, que solo añade | Inmutable desde el Freeze de la unidad |

**Integridad (estable ante rebase).** Exactamente **un** commit de la historia lleva el trailer `Freeze: <unidad> <ruta del
archivo>`; ese commit **modifica** el archivo (como minimo, fija en el su linea «Frozen») y es el **ultimo** commit que toca esa ruta
(`git log -- <ruta>`); el archivo congelado no se renombra. Los trailers sobreviven al rebase, y no se guarda ningun SHA en el
contrato. Como el estado mutable vive en el contrato, las enmiendas en el registro de decisiones y la evidencia en su propio archivo
(P-15), **ninguna escritura ordinaria toca el artefacto congelado**. La errata confirmada se aplica **antes** del commit de Freeze
(P-08 regla 4); despues, **todo** cambio a un elemento congelado es una enmienda A-n, sea cual sea su clase. El bloque de estado que
una Proposal congelada lleve en cabecera refleja el momento del Freeze y no se actualiza: el estado posterior vive en el contrato o en
el registro de decisiones.

**Freeze V1 consumido por una unidad V2.** Un Freeze V1 puede vivir en un contrato mutable o en una Proposal con bloques de estado,
asi que la regla del trailer no le aplica. La unidad V2 lista en su Discovery delta las secciones del Freeze V1 que consume y
registra en su archivo de evidencia la referencia de commit de esa lectura; READY-09 comprueba que **esas secciones** no cambiaron
desde esa referencia. Si cambiaron: **STOP** y se repite la comprobacion de compatibilidad de P-02 (pasos 2 a 4).

Reglas de version de la Proposal: cada version abre con «Cambios respecto de Vn-1»; no arrastra bloques de estado, preflights ni
tablas del Owner copiados de la anterior (viven una vez en el contrato o en el registro de decisiones; EA C-18); la version
congelada no depende de versiones previas para leerse.

**Enmiendas.** Toda modificacion posterior al Freeze de un elemento congelado — incluidas las de clase «solo Coordinator», como
añadir un escenario OV o re-secuenciar gates — es un delta numerado (`A-n`) que lista exactamente que clausulas cambian, quien lo
decidio y por que. Donde vive: las A-n de un Freeze **conceptual** en `docs/automation/decisions/<I>.md`, y **todas** las unidades que
lo consumen las leen; las A-n de un **Freeze delta** de unidad, o de una unidad T8 (P-25), en `docs/automation/decisions/<unidad>.md`.
READY-08, READY-09 y la conformidad leen **Freeze + todas las A-n aplicables**.

**Invalidacion del Freeze — quien decide:**

| Clase de cambio | Ejemplos | Decide |
|---|---|---|
| Solo Coordinator | Aclarar un detalle no congelado (sin enmienda); re-secuenciar gates sin cambiar resultados, **añadir** escenarios OV u obligaciones de prueba (enmienda A-n); mover o renombrar codigo (sin enmienda) | Coordinator, con A-n cuando toca un elemento congelado |
| Re-revision del Architect | Cualquier disparador M-01..M-08 sobre un elemento congelado; riesgo material nuevo descubierto en implementacion; debilitar o retirar una obligacion de prueba de un invariante congelado | Architect (delta) + Coordinator, **en todos los arquetipos**; un disparador ademas reclasifica hacia arriba |
| Re-decision del Owner | Alcance o no-objetivos; comportamiento visible distinto de una decision del Owner; **retirar o sustituir** escenarios OV; cambiar un ADR aceptado (exige ADR nuevo); dependencias NuGet u otra materia reservada | Owner |

### P-10 — Gates de implementacion como unidades de comportamiento

**Categoria**: `[NEW]`. **Evidencia**: patron dominante de gates internos por capa con un unico verificable al final (EA Q6: I-50
primer verificable en el commit 17 de 20; I-54 solo G7; I-51 G5); a la vez, RED por gate y revision del Coordinator entre gates
capturaron defectos (EA hechos E.3, E.4; B-16, B-18, B-41; B-10, B-11, B-40); gates que terminaban en push sin mirar CI dejaron
pasar el cuelgue (EA B-12, O-6).

**Definicion.** Un gate es una unidad con **resultado util y verificable por si misma**: comportamiento observable por prueba,
o hito visible por el Owner. Ejemplos a evaluar por iniciativa, **no plantilla fija**: «Modelo + persistencia», «Comportamiento de
Application», «UI / integracion», «Regresion / Candidato». La V2 no prescribe numero de gates. Un gate admite varios commits
internos.

**Division justificada** cuando hay: contrato revisable por separado; limite de rollback; subsistema materialmente distinto;
hito real visible por el Owner; secuenciacion forzada por archivos calientes (P-07).

**Division NO justificada**: DTO, helper, resolver o mapper solo porque son clases o archivos distintos.

**Cierre de gate** (todas las condiciones):

1. Resultado del gate verificado (pruebas que lo observan o hito del Owner registrado).
2. RED→GREEN focal demostrado para cada comportamiento nuevo y cada bugfix (AGENTS punto 2; prueba vista fallando), con conteo de
   pruebas seleccionadas > 0 (`0 seleccionadas = FALLO`, AGENTS) `[KEEP]`.
3. Pruebas relevantes verdes y la evidencia de suites que AGENTS «Pruebas» exija al cierre de gate (cadencia propuesta en P-11).
4. **CI push sobre el SHA de cierre en verde y leido** antes de abrir el siguiente gate: `event = push`, `head_sha` exacto, jobs
   requeridos en `success` `[NEW]`. Un rojo se diagnostica leyendo logs, TRX, volcados y artefactos **antes** de formular un gate
   de correccion (EA C-27, O-7).
5. Revision del Coordinator del gate (diff vs Freeze + A-n; hallazgos; desviaciones clasificadas) y **ninguna discrepancia EXP-01
   de clase A abierta** que afecte al gate (P-06 regla 5).
6. Registro en el cuerpo del commit de cierre del gate (WORKFLOW §4.3). **Sin commits documentales `-CLOSE` obligatorios**: una
   precision o decision material va al registro de decisiones; el resto no se re-enuncia (EA TABLE A I-54: 6 commits `-CLOSE`
   mayormente re-enuncian tablas; EA Q14).

### P-11 — Pruebas de iteracion y cadencia Full

**Categoria**: `[CHANGE]` cadencia de la suite Core local; `[KEEP]` LC-UI, Candidato, cierre, 0 seleccionadas = FALLO, CI en todo
push; `[BIND]` sin T0–T4, R0–R4 ni Quick CI.

**Iteracion ordinaria dentro de un gate** (sin cambio de poblacion del CI):

- pruebas focales del comportamiento en curso (RED→GREEN), con conteo;
- pruebas relevantes del sistema o componente tocado;
- builds dirigidos solo cuando añaden evidencia que ninguna prueba ejecutada da;
- el CI corre en todo push como hoy (`ci.yml` sin cambio; EA H.1-7).

**Cambio a la cadencia de la suite Core local:**

| | V1 | V2 propuesta |
|---|---|---|
| Regla V1 que cambia | AGENTS «Pruebas — definicion de terminado», punto 1, tabla: en la iteracion ordinaria la suite Core en local queda «sin cambio: LC-UI no toca el nucleo», junto a la UI «NO obligatoria antes del push». Leidas juntas: **suite Core local antes de cada push** de un cambio de comportamiento | **Suite Core completa en local obligatoria en el cierre de cada gate** (sobre su SHA, tras el commit, arbol limpio), en el Candidato y en el cierre; **no obligatoria antes de cada push interno** de un gate |
| Evidencia G1 | — | EA C-08 (I-54 Core «antes del commit y otra vez sobre el SHA» ×4); EA C-11 (I-50 Core ×4 con entradas de Core sin cambio); EA C-09; EA Q7 (I-54 ~13 corridas antes del Candidato; 0 defectos inesperados registrados) |
| Que sustituye la repeticion temprana | — | Focales + relevantes locales; CI push con **Core y UI completos** sobre cada **punta empujada**; suite Core local al cierre de gate |
| Deteccion de regresion entre sistemas | — | La suite completa del CI detecta regresiones cruzadas en la **punta** de cada push. En un push agrupado `A → B → C` solo `C` recibe corrida (AGENTS «Push agrupado»): la regresion se detecta en ese push, no en el commit que la introdujo |
| Riesgo residual declarado | — | Una regresion que solo aparece en Core **local Windows** (bytes en disco, cultura del hilo; motivo de AGENTS para excluir el Core de LC-UI) se detecta al cierre del gate, no en el push; el coste es retrabajo dentro del gate. Un Candidato nunca carece de suite Core local |
| Candidato Full | Core + UI Full local + builds + CI exacto | **Sin cambio** (EA H.1-9) |

**Evaluacion de la hipotesis H3 de la auditoria** («Full local concentrado principalmente en el Candidato final»):

- **UI**: ya esta concentrada por LC-UI (V1); sin cambio.
- **Core, forma fuerte (solo en el Candidato): NO ADOPTADA.** El Full local previo no registro defectos inesperados (EA Q7), pero
  su valor contrafactual es UNKNOWN (EA hechos D.8) y AGENTS excluye el Core de LC-UI por una diferencia de clase que la muestra no
  refuta. Concentrarlo en el Candidato moveria toda la deteccion Core-Windows al final.
- **Core, forma moderada (cierre de gate): ADOPTADA** como `[CHANGE]` arriba. Con gates de comportamiento (P-10) su numero cae de
  forma natural sin reclasificar cambios.
- **Reapertura de la forma fuerte**: solo con evidencia de P-22 (capturas de la suite Core local de cierre de gate que el CI no
  vio, medidas sobre varias iniciativas V2; U-04).

**Alcance de la obligacion, como en V1.** La definicion de terminado de AGENTS punto 1 rige para cambios de comportamiento. Que un
gate no cambia comportamiento **no se autodeclara**: se demuestra con la guardia mecanica de rutas de WORKFLOW 4.5.4
(`git diff --name-only` no debe listar nada fuera de `docs/`, `README.md` o `CLAUDE.md`; lista cerrada, citada de V1). Solo entonces no
hay obligacion de suite local, igual que hoy; su push sigue ejecutando el CI completo.

**Ninguna exencion por clase de cambio.** No hay carril «solo documentacion», ni «cambio pequeño», ni «bajo riesgo»: la cadencia
depende de **si hay un cambio de comportamiento declarado terminado** (regla V1) y del **punto del ciclo**, nunca de una clase
autodeclarada (EA H.1-5..7; EA H.3-4 no reabierta).

### P-12 — Candidato: umbral de preparacion y evidencia

**Categoria**: `[NEW]` umbral y termino `FINAL_CANDIDATE_SHA`; `[KEEP]` evidencia del Candidato, exact-SHA, rebase final, commit
documental de cierre y roles de SHA; `[BIND]` EA H.1-9..11.

**Evidencia**: `acecde6` declarado Candidato con CI rojo desde G4E (EA Q9, B-12); E3-C BLOCKED por avance de `main` (EA B-33);
2 de 10 Candidatos invalidados (EA Q8). Nota honesta: «Candidato declarado antes de completar la funcionalidad» **no se observo**
en la muestra (EA Q9); el umbral se apoya en los dos casos anteriores y en el racional de no ensamblar evidencia cara sobre un
SHA que se sabe que cambiara.

**Umbral de preparacion.** Antes de fijar `FINAL_CANDIDATE_SHA`, en este orden:

| ID | Condicion |
|---|---|
| READY-01 | Alcance congelado completo: cada elemento del Freeze implementado, o diferido por decision registrada de quien corresponde (P-09) |
| READY-02 | Todos los gates cerrados (P-10) |
| READY-03 | Sin bloqueos materiales: ningun hallazgo REQUERIDO abierto (P-08), desviacion MATERIAL u OWNER-RESERVED sin decidir, ni decision pendiente |
| READY-04 | `git fetch`; si `origin/main` avanzo, **rebase final** (WORKFLOW 4.5.1) antes de seguir; preflight de paralelas registrado (P-07) |
| READY-05 | Sobre el SHA resultante: focales y relevantes verdes; CI push verde y leido (incluido el job `ui-tests`) |
| READY-06 | Conformidad completa sobre ese SHA: CONFORMING (P-19) |
| READY-07 | Arbol limpio; ningun commit pendiente; SHA fijado |
| READY-08 | Matriz OV lista para ese SHA si aplica (P-14) |
| READY-09 | Integridad del Freeze comprobada con Git: el ultimo commit que toca el archivo de Freeze (y el Freeze delta de la unidad) es el que lleva su trailer `Freeze:`; para un Freeze V1 consumido, las secciones citadas no cambiaron desde la referencia registrada; toda modificacion posterior consta como A-n (P-09) |

Solo entonces `FINAL_CANDIDATE_SHA := <ese SHA>` y se ensambla **la evidencia V1 completa** sobre el: Core Full local, UI Full
local, build Debug UI, build Debug Plugin, CI verde exacto (AGENTS punto 1; guia §7.1), y Owner Validation donde se active.

**Sin commits documentales entre READY-04 y el cierre (CR-03).** Desde el rebase de READY-04 hasta el commit documental de cierre no
se hace ningun commit **de evidencia ni de documentacion**: escribirlos crearia un SHA nuevo e invalidaria al propio Candidato. La
base (READY-04), el CI (READY-05), la conformidad (READY-06), el bloque de §7.1 y la Owner Validation se registran en el informe de la
sesion y se escriben en el archivo de evidencia **solo en el commit de cierre** (WORKFLOW 4.5.4). Excepciones, que no reutilizan nada:

- **Commit de correccion** (READY-05 rojo, READY-06 NON-CONFORMING, Owner Validation fallida): permitido; es un SHA nuevo y la
  preparacion se reinicia en READY-02 (y READY-04 si `main` avanzo).
- **Ronda invalidada**: antes de reiniciar READY-04 se permite **un** commit documental que registre la ronda fallida en el archivo de
  evidencia (dato de «Candidate attempts», P-22).
- **Vuelta a 4.5.1 despues del cierre** (re-fetch antes del merge, P-20): el rebase reescribe tambien el commit de cierre; tras el
  nuevo Candidato, el commit de cierre se rehace y su archivo de evidencia describe el Candidato nuevo, conservando la ronda anterior
  como intento invalidado.

**Commits exploratorios o parciales nunca se llaman Candidato ni se entregan para validar.** Se conserva la definicion V1: todo SHA
que se entrega para validar o integrar **es un Candidato** y exige la evidencia completa (AGENTS punto 1; guia §7.1). Si la matriz
OV planifica un hito intermedio del Owner, ese SHA es un **Candidato intermedio** con evidencia V1 completa y READY-02, -03, -05,
-07 y -08 para los gates que cubre; no sustituye al `FINAL_CANDIDATE_SHA` salvo que sea el mismo SHA y la validacion tenga el mismo
proposito y alcance (AGENTS «Reutilizacion de evidencia»).

**Invalidacion** (V1 sin cambio, AGENTS «Reutilizacion de evidencia»):

- **Cualquier SHA nuevo** invalida la identidad del Candidato: correccion, rebase o commit documental.
- **Rebase**: la evidencia previa no valida el SHA rebasado; READY-04..READY-09 y la evidencia completa se repiten.
- **Commits documentales despues del Candidato**: solo el commit documental de cierre (WORKFLOW 4.5.4) — guardia de rutas + CI
  verde sobre su SHA; sin Owner Validation porque no cambia dibujo, **no** por reutilizacion. Cualquier otro commit que toque
  producto exige un Candidato nuevo.
- **Invalidadores de clase**: arbol sucio, SDK resuelto distinto, version de AutoCAD o biblioteca de bloques distinta (AGENTS).

**Roles de SHA** (WORKFLOW 4.5.2, sin cambio):

```text
FINAL_CANDIDATE_SHA = SHA validado que porta el producto
CLOSURE_SHA         = commit documental de cierre; SHA nuevo con evidencia propia (4.5.4)
MERGE_SHA           = merge --no-ff en main; SHA nuevo; exige su CI (4.5.6)
```

### P-13 — Cobertura

**Categoria**: `[KEEP]`; propuesta: **sin cambio de politica de cobertura** (confirmacion del Owner en OWN-I).

- Cadencia V1 vigente: sin cobertura en push ordinario; con cobertura en push a `main` y en el `workflow_dispatch` del Candidato
  (WORKFLOW 4.5.2.bis, 4.5.6, 4.5.7; `ci.yml`). ADR-0033 §9 es su origen historico, no su autoridad.
- Aclaracion editorial (no cambio): **nunca** se despacha cobertura sobre `MERGE_SHA`; la trae su push a `main`. La orden E1-I que
  lo hizo contradecia WORKFLOW (EA C-06, O-1).
- El doble dispatch del mismo Candidato (EA C-07: rama antes del merge + `main` despues) no se resuelve aqui: exigiria decidir si
  dos corridas de dos definiciones de workflow sobre el mismo SHA medido son la misma evidencia; queda en U-03.
- Ninguna cobertura se retira (EA H.3-5 no reabierta).

### P-14 — Owner Validation

**Categoria**: `[KEEP]` disparador, metadata monotonica, formato de evidencia, reutilizacion por SHA exacto, metrica experimental
de duracion; `[NEW]` matriz OV y comprobacion del DLL entregado; `[BIND]` EA H.1-8; `[NOT-REOPENED]` EA H.3-6.

**Evidencia**: al Owner se le indico un DLL con marca de tiempo anterior al Candidato y el DLL cargado es UNKNOWN (EA B-13); 7
rondas PASS sin hallazgos (EA B-38), que **no** prueban falta de valor (EA Q2) ni justifican reducirla; veredictos del Owner que
solo constan transcritos en ordenes o commits de cierre (EA 0.5) y detalle por escenario UNKNOWN en I-48 e I-53D (EA hechos D.4).

**Matriz OV en el Freeze.** Antes de implementar, el artefacto de Freeze (P-09) incluye:

```text
OV-id | Escenario | Por que aplica (disparador) | Sistema(s) | Datos (DWG nuevo / legacy) | Resultado esperado | Momento (Candidato intermedio planificado / FINAL)
```

La matriz es **aditiva**: especifica los escenarios de la iniciativa **ademas** del checklist y los criterios de aprobacion de la
guia (§6, §7: round-trip, legacy, persistencia y demas). Nunca los sustituye ni los estrecha. Ejemplos de forma, sin forzarlos en
cada iniciativa: OV-01 camino feliz; OV-02 legacy; OV-03 editar/actualizar; OV-04 guardar/reabrir; OV-05 comportamiento ante fallo.

**Reglas:**

1. **Aplicabilidad por disparador** (V1): cambia comportamiento de dibujo → Owner Validation. «No aplica» se justifica con el
   analisis del diff, no con la metadata; `requires_owner_validation: false` no exime (AUTOMATION_PLAN §11).
2. **Refinamiento**: añadir escenarios en cualquier momento (Coordinator), como enmienda A-n del Freeze (P-09); nunca editando el
   archivo congelado. Retirar o sustituir un escenario: **Owner**.
3. **Sin sorpresas al final**: un escenario mayor descubierto en implementacion es desviacion de comportamiento (P-19) y el
   Coordinator decide si cambia alcance visible (entonces OWNER-RESERVED).
4. **Hitos intermedios**: solo sobre un Candidato intermedio con evidencia V1 completa (P-12); su reutilizacion exige mismo SHA,
   mismo proposito y alcance, misma version de AutoCAD y misma biblioteca (AGENTS). No existe validacion del Owner sobre un SHA
   sin evidencia de Candidato.
5. **Identidad del DLL entregado**: antes de entregar, el Executor comprueba que el DLL del worktree fue construido sobre el
   Candidato (`InformationalVersion` con ese sufijo) y registra su SHA-256 (guia §7 ya pide commit y SHA-256).
6. **Proporcionalidad**: la propuesta analiza el alcance de los escenarios, pero **no** elimina ni reduce la Owner Validation
   por politica general.

**POLICY DECISION vs PRODUCT VALIDATION.**

| | Owner POLICY DECISION | Owner PRODUCT VALIDATION |
|---|---|---|
| Que es | Decidir alcance, opcion de diseño, aceptar/rechazar ADR, aprobar politica (p. ej. la V2) | Ejecutar en AutoCAD los escenarios OV sobre un SHA exacto y declarar resultado |
| Metadata | `requires_owner_decision` | `requires_owner_validation` / `requires_autocad` (monotonicas) |
| Registro | `docs/automation/decisions/<I>.md` | Bloque de evidencia de la guia §7 y §7.1 |
| Reutilizacion | Rige su alcance escrito; para ser politica general necesita P-17 | Solo mismo SHA + mismo proposito y alcance + AutoCAD + biblioteca |

### P-15 — Cadencia documental, superficies separadas y cierre concentrado

**Categoria**: `[CHANGE]` WORKFLOW §8 (momento de ideas-futuras; ubicacion de hashes y conteos), AGENTS «Flujo Git multi-agente»
(regla de hashes) y guia de validacion §7.1 (donde se escribe el registro de la ronda del Candidato); `[NEW]` separacion de
superficies, archivo de evidencia por unidad, momento del indice de ADR y demas indices, tag de integracion (P-20); `[KEEP]` HANDOFF
solo al integrar, ROADMAP en tres momentos, archivo de ADR antes de implementar, guias antes de integrar, y la regla del README de
iniciativas «los contratos no copian conteos ni hashes».

**Evidencia**: HANDOFF +43% en 5 dias por anexion (EA §5.3); todos los conflictos de rebase/merge fueron documentales en HANDOFF,
ROADMAP, ideas-futuras, guia de validacion e indice de ADR (EA Q12); evidencia del Candidato copiada 3–7 veces (EA C-25);
referencias a un «HANDOFF §8-12» inexistente (EA B-36); tension entre «hashes solo en HANDOFF §12» y contratos y filas que los
contienen (EA Q10); `MERGE_SHA = PENDING` nunca actualizado en I-48 (EA TABLE A) e I-51 (EA 0.5; EA hechos A.20).

**Tres superficies separadas (CR-03)**, el modelo mas pequeño que no mezcla estado mutable con lo congelado:

| Superficie | Archivo | Contiene | Mutabilidad |
|---|---|---|---|
| **Contrato** (estado y enlaces) | `docs/initiatives/<unidad>-<slug>.md` (V1) | Frontmatter y `workflow` con valor V1 o V2 (solo el valor); alcance resumido y enlaces a Freeze, Discovery, Proposal, decisiones y evidencia; agrupacion; arquetipo; archivos calientes, dependencias y estrategia de coordinacion (P-07); Consumes/Extends/Introduces; borrador de entradas del registro (P-06); lista de hallazgos fuera de alcance. **Sin hashes, corridas ni conteos** (regla V1 conservada) | Mutable |
| **Freeze** (contrato congelado) | `<I>-freeze.md`, la Proposal congelada o `<unidad>-freeze-delta.md` (P-09) | Solo los elementos congelados, incluida la matriz OV | **Inmutable** tras el commit de Freeze; enmiendas A-n fuera de el |
| **Evidencia operativa** | `docs/automation/evidence/<unidad>-evidence.md` (carpeta V1 existente) | Base del reclamo y evidencia de orden respecto de `WORKFLOW_V2_EFFECTIVE_SHA` (P-25); referencia de un Freeze V1 consumido (P-09); bloque de Candidato (guia §7.1), base del Candidato (READY-04), conformidad (resultado y SHA), registros de Owner Validation, fila de metricas (P-22), nombre del tag de integracion. No contiene el SHA del propio commit de cierre (va al tag) | Antes de READY-04, en commits documentales de la rama (p. ej. registros de Candidatos intermedios); desde READY-04, **solo** en el commit de cierre (P-12) |

Los hechos **posteriores al merge** no caben en ninguna de las tres (aun no existen al cerrar la rama): van al **tag anotado
`integration/<unidad>`** de P-20, que no es un commit.

**Durante diseño e implementacion**, solo documentos locales de la iniciativa: contrato; Discovery; Proposal vigente o plan;
Freeze; registro de decisiones `docs/automation/decisions/<I>.md` **solo cuando hay decision material** (Owner, enmienda A-n,
resultado de revision de Architect); archivo de evidencia cuando hay evidencia que registrar. Las puntas observadas de ramas y los
resultados de cada gate van al cuerpo del commit y al informe de gate/sesion (P-07, P-10), no a documentos durables.

Excepciones que siguen su momento V1: el **archivo** de un ADR nace antes de implementar la decision (WORKFLOW §8); la propia fila
de ROADMAP en sus tres momentos (WORKFLOW §2).

**Guias y README** cuando cambia comportamiento visible: en el **ultimo gate de implementacion**, antes del Candidato, para que
la documentacion forme parte de lo terminado (AGENTS punto 4; WORKFLOW §8 «en la misma rama, antes de integrar») `[KEEP]`.

**Cierre concentrado.** El commit documental de cierre (WORKFLOW 4.5.4) lleva, de una vez, las demas ediciones compartidas y la
evidencia final de la rama: bloque de HANDOFF; marca de ROADMAP; traspaso de hallazgos fuera de alcance a `ideas-futuras.md`
`[CHANGE]`; filas del indice `adr/README.md` (hoy practica sin momento escrito) `[NEW]`; entradas del registro de fundaciones copiadas
del borrador revisado (P-06); linea del indice `initiatives/README.md`; archivo de evidencia completado.

**Mapa de autoridad por tipo de informacion** (una sola fuente durable; los demas enlazan):

| Informacion | Autoridad unica | Los demas |
|---|---|---|
| Estado de la iniciativa, enlaces, coordinacion declarada | Contrato | — |
| Alcance congelado, no-objetivos, invariantes, matriz OV | Artefacto de Freeze (+ enmiendas A-n) | Contrato, ordenes y HANDOFF enlazan |
| Evidencia de codigo «como es» | Discovery | Proposal cita DC-n |
| Alternativas y racional de diseño | Proposal (historica tras el Freeze) | — |
| Decisiones del Owner, enmiendas, resultado de revisiones | Registro de decisiones | Contrato enlaza |
| SHAs, corridas y conteos previos al merge (Candidato, conformidad, OV, cierre) | **Archivo de evidencia** `[CHANGE]` y cuerpos de commit | HANDOFF enlaza sin copiar; contrato, ROADMAP y documentos normativos nunca |
| Puntas observadas de ramas en un momento | Cuerpo del commit e informe de gate/sesion | Ningun documento durable |
| Hechos posteriores al merge (`CLOSURE_SHA`, `MERGE_SHA`, `FINAL_MAIN_SHA` si hubo merge de correccion, CI post-merge, cobertura del Candidato, limpieza) | **Tag anotado `integration/<unidad>`** (P-20) | Archivo de evidencia apunta al nombre del tag; ningun documento escribe PENDING |
| Estado vivo del proyecto | HANDOFF | — |
| Plan y registro de cierre | ROADMAP | — |
| Fundaciones reutilizables | Registro de fundaciones | ARCHITECTURE enlaza |

**Resolucion de la duplicacion actual** sin borrar la unica autoridad durable: la Proposal deja de re-enunciar estado (P-09); las
salidas de Architect no se re-transcriben en la orden siguiente, se citan por ID de hallazgo; el contrato de una unidad de entrega
es delta (P-02); el bloque de HANDOFF es corto y enlaza al archivo de evidencia; las referencias a «HANDOFF §8-12» se corrigen a la
seccion real en la integracion normativa (U-13). **No se reescriben registros historicos** (AGENTS «Registro historico, no
precedente»); la poda de los bloques historicos actuales de HANDOFF queda en U-14. No se crea ningun documento grande nuevo: el
archivo de evidencia sustituye la «Evidencia final» del contrato y las copias de la evidencia del Candidato en varios sitios.

**Cambio de la regla de hashes** `[CHANGE]`: AGENTS («No copiar conteos de tests ni hashes de commit fuera de `docs/HANDOFF.md`
(seccion 12)») y WORKFLOW §8 pasarian a: hashes, corridas y conteos de una unidad viven en los cuerpos de commit, en su archivo de
evidencia y en su tag de integracion; HANDOFF enlaza; contrato, ROADMAP, indices y documentos normativos no los contienen. La guia
§7.1 pasaria a situar el bloque del Candidato en el archivo de evidencia o en el cuerpo del commit (hoy: contrato o commit).
Racional: la regla V1 apunta a una seccion que no existe (EA B-36) y la practica ya los dispersaba (EA Q10); un unico lugar por
unidad y fase evita las copias que divergen, que es el objetivo original de la regla.

### P-16 — REFERENCE OVER REPETITION (con referencia precisa y completa)

**Categoria**: `[NEW]`. **Evidencia**: ordenes de Candidato ~55% politica general re-enunciada y de integracion 40–55% (EA §5.4);
omitir reglas documentadas no produjo incidentes en guardia docs-only, jobs post-merge, dispatch y limpieza, pero re-enunciarlas
mal produjo O-1 y O-2, y omitir restricciones **no documentadas** coincidio con desviaciones (EA hechos C.6; EA Q14).
**Contraevidencia** (EA hechos C.6; EA §5.4): omitir en la orden la regla **documentada** de la celda de estado de ROADMAP si
coincidio con 2 desviaciones (I-48 G0; bootstrap de I-45). Por eso «referenciar» no es «callar»: la orden **nombra** cada regla
documentada que el gate ejerce.

**Una orden referencia la autoridad versionada para las reglas generales** en lugar de parafrasearla. La referencia es **precisa
y completa**: ruta + seccion + clausula concreta (p. ej. «AGENTS «Pruebas — definicion de terminado» punto 1 y guia §7.1»), no
«sigue el workflow»; si la clausula depende de un contexto (Candidato vs cierre), la orden nombra cual; y la orden lista **todas**
las reglas documentadas que el gate ejercita (p. ej. un bootstrap lista «ROADMAP: WORKFLOW §2, sin estado en curso»).

**Una orden DEBE declarar siempre:**

1. objetivo del gate;
2. alcance especifico del gate;
3. invariantes especificos de la iniciativa que el gate toca (por ID del Freeze);
4. rutas y archivos calientes relevantes;
5. evidencia requerida (con referencia a la regla que la define);
6. no-touch especifico del gate;
7. condiciones de parada **no cubiertas** adecuadamente por los documentos referenciados;
8. informe esperado.

**Una orden NO copia rutinariamente:** historia del repo; WORKFLOW completo; resumenes completos de ADR; politica general de
integracion; politica general del Owner; politica de limpieza ya gobernada en otro documento; revisiones del Architect a su propio
autor (se citan por ID).

**Contradiccion**: `STOP` + citar ambas autoridades (ruta y seccion o linea). Ninguna eleccion silenciosa. Resuelve el Coordinator
si es editorial o de interpretacion dentro de su competencia; el Owner si es de politica (P-17).

**Reglas recurrentes hoy no documentadas** que la integracion normativa debe **escribir** para que puedan referenciarse (EA §5.4
tabla de bloques): preflight de sesion completo (fetch, arbol limpio, stash, operaciones Git en curso, worktree ocupado, AutoCAD
cerrado antes de compilar); puntos de re-fetch (P-07); CI leido por gate (P-10); diagnostico antes de gate de correccion (P-10).
Mientras no esten escritas, la orden las declara.

### P-17 — Modelo de autoridad

**Categoria**: `[CHANGE]` WORKFLOW §10 y AUTOMATION_PLAN §2 «Precedencia obligatoria».

**Desajuste actual** (verificado en `main`):

| Fuente | Orden que declara |
|---|---|
| WORKFLOW §10 | Estado real del repo > {AGENTS > WORKFLOW} > HANDOFF > ROADMAP > README > guias > historicos. No menciona ADR, contratos, decisiones del Owner ni ordenes |
| AUTOMATION_PLAN §2 | Git real > AGENTS > WORKFLOW > ADR aceptados > AUTOMATION_PLAN > contrato > decisiones del Owner > estado > PR |
| adr/README | ADR aceptado inmutable; solo el Owner acepta |
| AGENTS punto 1 | Un ADR `propuesto` es origen, no autoridad |
| Ordenes de gate | Sin lugar; contradijeron documentos vinculantes en EA O-1, O-2, O-3 |

Problemas: un ADR aceptado queda por debajo de WORKFLOW en una fuente y ausente en la otra; una decision explicita del Owner queda
por debajo del contrato; HANDOFF (estado) figura como si fuera norma; la orden de gate no tiene rango.

**Propuesta: autoridad por dominio con reglas de conflicto** (no un orden total unico, porque las fuentes gobiernan cosas
distintas):

| Dominio | Autoridad (de mayor a menor dentro del dominio) |
|---|---|
| **Hechos** (que ocurrio, que corre, que paso) | Git, resultados de CI, salidas de pruebas y builds. Prevalecen sobre **afirmaciones** documentales; no reescriben normas |
| **Decisiones de arquitectura** | Convenciones arquitectonicas de AGENTS y ADR aceptados (conflicto: regla 4) > entrada del registro de fundaciones (descriptiva) > contrato congelado (dentro de su alcance; no puede contradecir un ADR aceptado sin ADR nuevo aceptado) |
| **Evidencia y pruebas** | AGENTS «Pruebas» y «Reutilizacion de evidencia» (con ADR aceptado de este dominio segun la regla 4; la mas estricta mientras haya conflicto) > guia de validacion (procedimiento) > contrato (obligaciones adicionales; solo añade) |
| **Git, integracion y cadencia documental** | WORKFLOW y ADR aceptados de proceso, p. ej. ADR-0001 (conflicto: regla 4) > AUTOMATION_PLAN (solo el ejecutor automatizado; nunca relaja WORKFLOW) > contrato (precisa, no cambia reglas globales, AUTOMATION_PLAN §2 «Modo normal») |
| **Diseño, revision y preparacion de iniciativas** | INITIATIVE_LIFECYCLE (P-24) > contrato congelado > Proposal vigente |
| **Alcance de una iniciativa** | Decision explicita del Owner registrada > contrato congelado + enmiendas > Proposal vigente > Discovery |
| **Estado y plan** | HANDOFF (estado vivo) y ROADMAP (plan): no normativos |
| **Instrucciones de una sesion** | Orden de gate: la mas baja; puede **estrechar** (añadir paradas, reducir alcance), nunca ampliar ni redefinir una autoridad superior |
| **Registros historicos y ADR propuestos** | Sin autoridad; no son precedente (AGENTS) |

**Reglas de conflicto:**

1. Entre dominios, cada fuente manda solo en el suyo; un conflicto real entre dominios es `STOP` + citar ambas.
2. WORKFLOW o INITIATIVE_LIFECYCLE vs AGENTS sobre evidencia o pruebas: gana AGENTS y se corrige el otro (regla V1 de WORKFLOW
   §10, conservada y extendida).
3. INITIATIVE_LIFECYCLE vs WORKFLOW: WORKFLOW manda en Git, integracion y documentos compartidos; el ciclo de vida, en diseño,
   revision y preparacion; un solapamiento es un defecto de redaccion: `STOP` + citar ambas y corregir uno de los dos.
4. ADR aceptado vs AGENTS o WORKFLOW, en cualquier dominio: si el ADR **declara** ser excepcion a esa convencion (criterio 4 de
   «Cuando crear un ADR» en adr/README), manda el ADR dentro de su alcance escrito; si el conflicto **no** esta declarado, los
   documentos son inconsistentes: `STOP` y decide el Owner. En evidencia y pruebas, mientras tanto, rige la regla mas estricta
   (AGENTS punto 1 ya fija «donde ambos difieran, manda este punto» frente a ADR-0033). Esto sustituye el orden fijo de
   AUTOMATION_PLAN §2 (§8).
5. Una garantia de seguridad (exact-SHA, Owner Validation, post-merge, no merge automatico) en conflicto con cualquier otra fuente:
   se aplica **la mas estricta** hasta resolver (precedente V1: AUTOMATION_PLAN §11).
6. Una orden de gate que parezca ampliar o redefinir: `STOP`; nunca se ejecuta la lectura amplia.
7. Un hecho que contradice una afirmacion documental: manda el hecho; la afirmacion se corrige en el siguiente cierre que la toque,
   **salvo** que la discrepancia afecte algo que la iniciativa consume: entonces rige P-06 regla 5 (clase A = STOP).

**Como una decision explicita del Owner se vuelve norma durable** (evidencia: la excepcion «sin rebase» de I-50 se cito luego como
precedente en E3-C, EA O-2):

1. Se registra en `docs/automation/decisions/<I>.md` con su **alcance**: `ESTA INICIATIVA / ESTE GATE` o `POLITICA`.
2. De alcance local: aplica solo donde dice y **no es citable como precedente** por otra iniciativa.
3. De alcance politica: no vale para otras iniciativas hasta que se **materializa** en el documento normativo dueño de ese dominio
   (WORKFLOW, AGENTS, ADR aceptado) mediante un commit integrado; hasta entonces es excepcion registrada, no norma.
4. Una excepcion local repetida en varias iniciativas es señal para proponer su materializacion, no autorizacion para seguir
   repitiendola.

### P-18 — Plantillas (propuesta compacta)

**Categoria**: `[NEW]`. Los objetivos de longitud son guia, no limite; la correccion manda. Todas usan referencias precisas (P-16).

**A. Coordinator Bootstrap Template**

```text
I-NN — BOOTSTRAP (Workflow V2)
IDs funcionales: ...            Agrupacion provisional (P-02): (a) mismo problema de usuario + (b) autoridad/fundacion
                                compartida, sin leer codigo; decision + por que
Arquetipo provisional (P-03): ...  Disparadores M-01..M-08: activado / no / UNKNOWN (UNKNOWN = activado)
Coordinacion declarada (P-07): archivos calientes conocidos; dependencias declaradas; estrategia
Autorizacion: fila ROADMAP | autorizacion del Owner (WORKFLOW §2 caso d)
Reglas que ejerce: reclamo WORKFLOW §4.1; bootstrap y fila WORKFLOW §2 (sin estado en curso); preflight de sesion <ref>;
                   clasificacion V1/V2 por el propio reclamo (P-25)
Pedido: contrato desde TEMPLATE (workflow, Consumes/Extends/Introduces, coordinacion declarada, sin hashes); luego DISCOVERY
        CORE (P-05 DC-1..DC-9)
No-touch: ramas activas <lista>; archivos de la interseccion <lista>
STOP si: rama remota ya existe; interseccion funcional no listada; un disparador pasa a activado o UNKNOWN respecto del
         provisional; discrepancia EXP-01 de clase A; hace falta una expansion EXP (pedir autorizacion); reclamo durante una
         pausa del Owner; contradiccion entre fuentes -> citar ambas
Informe: BASE_SHA, CLAIM_SHA, puntas observadas en el preflight (solo en el informe y el cuerpo del commit), archivos, tabla DC,
         disparadores, arquetipo propuesto, expansiones pedidas
```

**B. Executor Gate Prompt Template** (≈15–40 lineas como guia)

```text
I-NN — G<n> <nombre>                      Arquetipo: ...   Freeze: <ruta §>
Objetivo: <resultado verificable del gate>
Alcance: <puntos del Freeze F-x..F-y>     Fuera de alcance de este gate: ...
Invariantes tocados: <IDs del Freeze>
Rutas / calientes: ...                    Comparar puntas con: <ultimo commit o informe que las registro>
Reglas que ejerce: preflight de sesion <ref>; rebase al abrir WORKFLOW §4.2; RED->GREEN AGENTS punto 2;
                   0 seleccionadas = FALLO (AGENTS); suite Core local al cierre (P-11); CI leido (P-10)
Evidencia requerida: SHA de cierre; pruebas con conteo; suite Core local; CI push del SHA de cierre (event, head_sha, jobs)
No-touch: ...; el archivo de Freeze
STOP si:
  - el CI del gate anterior no esta verde y leido;
  - una punta cambio respecto de la registrada y toca la interseccion; o origin/main avanzo sobre un archivo caliente del gate;
  - se activa un disparador M-01..M-08, una discrepancia EXP-01 de clase A o una desviacion MATERIAL / OWNER-RESERVED;
  - hace falta compilar el Plugin con AutoCAD abierto;
  - un CI rojo: leer logs/TRX/volcados antes de proponer correccion;
  - <condiciones especificas del gate>; contradiccion entre fuentes -> citar ambas
Informe: SHA(s), puntas observadas, pruebas con conteos, CI run, desviaciones clasificadas (P-19), hallazgos fuera de alcance
```

**C. Architect Review Template**

```text
I-NN — ARCHITECT REVIEW <ronda>           Review mode: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN
Entrada: <Proposal vN o archivo de Freeze, ruta>; delta desde la ronda anterior: <cambios requeridos + diff>
Alcance: <completo (primera ronda) | delta + interacciones demostradas | compatibilidad de un Freeze V1 consumido por una unidad V2>
Evaluar contra: materialidad P-04, elementos congelables P-09, fundaciones consumidas (DC-8, clase A/B de P-06)
Salida:
  AGREED POINTS
  DISAGREEMENTS
  MATERIAL RISKS
  REQUIRED CHANGES        (cada uno: id, clase de hallazgo, elemento afectado)
  OPTIONAL IMPROVEMENTS   (errata / seguimiento; no generan ronda)
  CONSENSUS STATUS        AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
No "aprobar": sin REQUIRED CHANGES abiertos -> AGREED
```

**D. Conformance Review Template**

```text
I-NN — CONFORMANCE sobre <SHA>            Revisor: Architect + Coordinator   Review mode: ...
Contra: archivo de Freeze <ruta> (+ Freeze V1 consumido y Freeze delta, si aplica) + enmiendas A-n
Integridad: READY-09 (trailer `Freeze:` unico y ultimo commit de la ruta; secciones V1 citadas sin cambio)
Traza: invariante congelado -> ubicacion en codigo -> prueba/guarda (sin prueba = desviacion)
No-objetivos respetados (diff) | Puntos de extension conformes | Matriz OV lista | Borrador de entradas del registro (P-06)
Discrepancias EXP-01 abiertas de clase A: ninguna
RESULT: CONFORMING | NON-CONFORMING
Deviations: id | descripcion | clase (EDITORIAL | NON-MATERIAL | BEHAVIORAL-WITHIN-FREEZE | MATERIAL | OWNER-RESERVED) | decide
Seguimientos (no requeridos): mejoras de diseño -> ideas-futuras
Registro: resultado y SHA en el informe de la sesion; al archivo de evidencia solo en el commit de cierre (P-12)
```

**E. Final Handoff Template** (el contenido durable es el mensaje del tag de integracion, P-20)

```text
I-NN (unidad Ex) — FINAL HANDOFF
Workflow: V2                          Evidencia previa al merge: docs/automation/evidence/<unidad>-evidence.md
Tag: integration/<unidad>  (anotado, sobre el merge verificado; creado tras la limpieza; correcciones -corr<N>)
Mensaje del tag (bloque registrado, P-20 regla 4):
  FINAL_CANDIDATE_SHA / CLOSURE_SHA / MERGE_SHA (primer merge) / FINAL_MAIN_SHA (merge verificado = destino) / merges no verificados
  Post-merge CI: run, event=push, ref=refs/heads/main, head_sha=FINAL_MAIN_SHA, jobs, artefacto de cobertura
  Candidate coverage: run, candidate_sha, measured-sha, artefacto
  Cleanup: rama local, rama remota, worktree (fecha)
Owner: POLICY DECISIONS <refs>  |  PRODUCT VALIDATION <OV ids, SHA, DLL SHA-256> (en el archivo de evidencia)
Registro de fundaciones: entradas añadidas/cambiadas
Desviaciones de proceso: ...          Metricas (P-22): en el archivo de evidencia
No es evidencia: la corrida de CI que dispara el propio push del tag (ref refs/tags/...)
```

**F. Candidate & Integration Prompt Template** (añadida: los incidentes O-1, O-2 y O-3 ocurrieron en ordenes de Candidato e
integracion, EA §5.4)

```text
I-NN (unidad Ex) — CANDIDATE READINESS + INTEGRATION
Reglas que ejerce: READY-01..09 (P-12); Candidato AGENTS «Pruebas» punto 1 + guia §7.1; WORKFLOW 4.5.1–4.5.7 y §4 paso 6
                   (sin copiar); Owner Validation AGENTS punto 5 + matriz OV <ruta>; cierre concentrado (P-15); tag P-20
Especifico de esta unidad: escenarios OV <ids>; documentos compartidos del cierre <lista>; comparar puntas con <informe previo>
STOP si:
  - cualquier READY no se cumple o queda UNKNOWN;
  - origin/main avanzo antes del merge (volver a 4.5.1; sin excepciones);
  - arbol sucio al producir evidencia local; AutoCAD abierto antes de compilar el Plugin;
  - el DLL a entregar no esta construido sobre el Candidato (InformationalVersion y SHA-256, P-14 regla 5);
  - la orden parece contradecir WORKFLOW 4.5 o AGENTS (p. ej. cobertura sobre MERGE_SHA, limpieza antes de 4.5.7) -> citar ambas;
  - el CI de MERGE_SHA no esta verde o falta el artefacto: no limpiar ni crear el tag; corregir en la rama
Informe: plantilla E; el tag se crea solo tras 4.5.6, 4.5.7 y la limpieza
```

### P-19 — Conformidad final

**Categoria**: `[NEW]` (codifica practica de I-45 CR1–CR3). **Evidencia**: la conformidad de I-45 hallo 4 HIGH tras «NONE» del
Coordinator (EA B-26) y dos remedios incompatibles creados por un gate (EA B-27); INV-09 de I-51 quedo sin prueba de comportamiento
y lo hallo I-54 despues del merge (EA B-30); contrato y fila de I-53D con «aviso y confirmacion» que V2 §7.8 no tenia (EA B-31).

**Definicion**: resultado implementado **vs** contrato congelado (Freeze + enmiendas). **No es una ronda de rediseño**: una mejora
de diseño se registra como seguimiento; solo es cambio requerido si revela un invariante congelado violado o un riesgo material no
cubierto por el Freeze (entonces es desviacion MATERIAL).

**Momento**: READY-06, sobre el SHA ya rebasado. Si un rebase posterior crea otro SHA, la conformidad se repite **completa sobre
el SHA nuevo**; ningun resultado de conformidad se traslada entre SHAs por range-diff, patch-id ni igualdad de arbol (EA H.1-10,
H.1-11). Si una conformidad acotada podria bastar queda como pregunta (U-09), no como regla.

**Revisor**: Architect + Coordinator **en todos los arquetipos**, con `Review mode` registrado. La conformidad es siempre completa
respecto de lo congelado; su tamaño lo da cuanto se congelo, no la etiqueta del arquetipo.

**Clases de desviacion y quien decide:**

| Clase | Ejemplo | Acepta |
|---|---|---|
| EDITORIAL | Redaccion del contrato que no coincide con el comportamiento congelado (B-31) | Coordinator (se corrige en el cierre) |
| NON-MATERIAL | Helper, archivo, tecnica local que preserva invariantes | Coordinator |
| BEHAVIORAL-WITHIN-FREEZE | Detalle visible no congelado (texto de un mensaje no validado por OV) | Coordinator; si toca un escenario OV, Owner |
| MATERIAL | Disparador M-01..M-08 sobre un elemento congelado; invariante sin prueba (B-30) | Architect + Coordinator (en EXTENSION reclasifica) |
| OWNER-RESERVED | Alcance, no-objetivos, decision del Owner, escenario OV retirado, ADR aceptado | Owner |

`NON-CONFORMING` impide READY-06; se corrige (SHA nuevo) o se acepta la desviacion por quien corresponde con registro. Una
discrepancia EXP-01 de clase A abierta (P-06 regla 5) hace el resultado **NON-CONFORMING** y no se acepta como desviacion: se
resuelve por las vias de esa regla. La conformidad se hace contra **Freeze + A-n** (P-09).

### P-20 — Integracion, post-merge y registro durable

**Categoria**: `[KEEP]` WORKFLOW 4.5.1–4.5.7 y §4 paso 6, nunca commit directo en `main`, sin merge automatico (EA H.1-12);
`[NEW]` tag anotado de integracion como registro durable posterior al merge (CR-04); `[CHANGE]` **precision** de politica Git:
re-fetch antes del merge y vuelta al rebase si `main` avanzo (hoy WORKFLOW no dice nada del intervalo entre el Candidato y el
merge). OWN-H: el Owner decide ambas cosas; no cambia ninguna otra mecanica Git.

**Analisis de la secuencia V1 y cambios explicitos:**

| Paso V1 | V2 |
|---|---|
| 4.5.1 Rebase final + `--force-with-lease` | Sin cambio; ocurre dentro de READY-04 |
| 4.5.2 CI verde sobre el tip rebasado + builds + Candidato Full | Sin cambio; precedido por READY-05..09 (P-12) |
| 4.5.2.bis Cobertura del Candidato (opcional) | Sin cambio (P-13) |
| 4.5.3 Owner Validation sobre el SHA rebasado | Sin cambio; ejecuta la matriz OV (P-14) |
| 4.5.4 Commit documental de cierre | Sin cambio en guardia y evidencia; contenido concentrado y archivo de evidencia completado (P-15) |
| (nuevo) antes de 4.5.5 | `git fetch`: si `origin/main` avanzo desde la base del Candidato, volver a 4.5.1 (rebase, Candidato nuevo). La propuesta **no** contiene ninguna via de merge sin rebase, ni guardia de patch-id o de igualdad de arbol |
| 4.5.5 `merge --no-ff` | Sin cambio; manual |
| 4.5.6 CI sobre `MERGE_SHA` + artefacto de cobertura | Sin cambio; nunca se sustituye por el CI del cierre aunque los arboles coincidan (EA C-05) |
| 4.5.7 Cobertura del Candidato en `main` | Sin cambio (P-13) |
| §4 paso 6 Limpieza tras 4.5.6 y 4.5.7 | Sin cambio |
| (nuevo) tras la limpieza | **Tag anotado `integration/<unidad>`** sobre el merge verificado (`FINAL_MAIN_SHA`) (abajo) |

**Registro durable de los hechos posteriores al merge (CR-04; cierra U-11).**

Alternativas evaluadas:

| Mecanismo | Durable | Sin commit (sin recursion) | Visible y verificable | Coste o riesgo | Resultado |
|---|---|---|---|---|---|
| Siguiente commit de cierre que toque HANDOFF | Si, tarde | No: depende de otra integracion; la ultima queda sin registrar | Si | Edicion de una iniciativa ajena; retraso indefinido | Rechazado |
| Commit posterior sobre la rama y segundo merge | Si | **No**: el segundo merge necesita a su vez su registro (recursion) | Si | Ceremonia infinita | Rechazado |
| Notas de Git (`refs/notes/*`) sobre el merge | Si | Si | Pobre: GitHub no las muestra; exigen configurar el fetch | Ref mutable y poco conocida | Rechazado |
| Solo GitHub Actions + informe final | No: logs y artefactos con retencion limitada; el informe vive fuera del repo | Si | Parcial | Repite el defecto de I-51 (EA 0.5) | Rechazado |
| **Tag anotado por integracion** | **Si**: objeto Git con autor (tagger), fecha y mensaje, en el remoto | **Si**: un tag no es un commit | **Si**: aparece en GitHub y en `git for-each-ref refs/tags/integration` | El push de un tag dispara CI (medido: los tags `archive/*` generaron corridas `push` sobre su commit); el repo ya usa tags anotados `archive/*` | **Elegido** |

**Regla propuesta** (unidades V2; una iniciativa V1 no la sigue por norma, contrato I-56 §0.1):

1. **Cuando**: despues de 4.5.6 verde, 4.5.7 verificada y la limpieza de §4 paso 6. La limpieza sigue siendo lo que declara terminada
   la integracion (WORKFLOW §4 paso 6); el tag solo **registra** los hechos, verificados, de forma durable.
2. **Destino**: el merge **verificado** de la unidad en `main` (exact-SHA). Si un CI post-merge rojo obligo a corregir en la rama y a
   un merge nuevo, el tag apunta a ese merge nuevo y su mensaje conserva el primer `MERGE_SHA`.
3. **Nombre**: `integration/<unidad>` (p. ej. `integration/I-53S`). Unico por unidad; **no se mueve ni se fuerza**. Un error en el
   mensaje se corrige con un tag nuevo `integration/<unidad>-corr<N>` que cita al anterior. Los consumidores buscan **exactamente**
   `integration/<unidad>` e `integration/<unidad>-corr<N>` (nunca un prefijo: `integration/I-53` no debe coincidir con
   `integration/I-53S`) y toman el de mayor N en orden numerico (`git tag --list --sort=v:refname`).
4. **Mensaje** (bloque estructurado; plantilla E):

   ```text
   Initiative: <I> (unit <unidad>)   Workflow: V2   Claim-Id: <uuid>
   FINAL_CANDIDATE_SHA: <40 hex>
   CLOSURE_SHA: <40 hex>
   MERGE_SHA: <40 hex>          (primer merge de la unidad en main)
   FINAL_MAIN_SHA: <40 hex>     (merge verificado = destino del tag; igual a MERGE_SHA si no hubo merge de correccion)
   Unverified merges: none | <sha> (<motivo>)
   Post-merge CI: run <id> event=push ref=refs/heads/main head_sha=FINAL_MAIN_SHA jobs=<4/4 success> coverage-artifact=present
   Candidate coverage: run <id> workflow_dispatch candidate_sha=<sha> measured-sha=<sha> artifact=present
   Cleanup: local-branch=deleted remote-branch=deleted worktree=removed (<fecha>)
   Evidence: docs/automation/evidence/<unidad>-evidence.md
   ```

5. **Evidencia que no cuenta**: la corrida de CI que dispara el push del propio tag (`ref=refs/tags/integration/...`) **no** es
   evidencia de nada, aunque aparezca en el estado del commit y aunque saliera roja; la evidencia post-merge es la corrida
   `event=push`, `ref=refs/heads/main`, `head_sha=FINAL_MAIN_SHA` que el mensaje nombra. Las herramientas de evidencia (P-21)
   excluyen `refs/tags/*`. Evitar esa corrida exigiria filtrar tags en `ci.yml`, cambio de CI fuera de I-56 (EA H.1-1; U-15).
6. **Quien**: la sesion de integracion (humano o sesion dedicada), sin automatizacion; ningun merge automatico (EA H.1-12).
7. **Consumo**: el Orchestrator o una iniciativa futura leen `git fetch --tags` y `git tag -n99` sobre los nombres exactos de la regla 3; los hechos
   quedan aunque caduquen los logs de Actions (el veredicto y los IDs quedan en el tag).
8. **Tag ausente**: `post-merge-check` y el preflight de la siguiente integracion comprueban que toda unidad V2 integrada tenga su
   tag con nombre exacto (regla 3); si falta, es DESVIACION DE PROCESO y se crea con los hechos recuperables (lo no recuperable, UNKNOWN).
9. **Precondicion de durabilidad**: los tags `integration/*` quedan protegidos contra borrado y reescritura en GitHub **antes** de crear
   el primero, incluido `integration/I-56` (P-25). Sin esa proteccion no se crean tags y la integracion de unidades V2 se detiene hasta
   que exista (OWN-H).

**Decisiones que requiere**: politica Git nueva de namespace de tags `integration/*`, su inmutabilidad y su proteccion en GitHub
(OWN-H).

### P-21 — Scripts futuros (solo diseño; no se implementan en I-56)

**Categoria**: `[NEW]` (diseño). `eng/**` no se toca en I-56 (EA H.1-1). Se implementarian en una iniciativa propia, con reclamo y
Candidato. Regla comun: **fail-closed**; una comprobacion requerida que no puede determinarse devuelve UNKNOWN y sale con codigo de
fallo; una seleccion que no selecciona nada es FALLO (AGENTS).

| Script | Entradas | Salidas | Semantica de fallo | Queda como decision humana |
|---|---|---|---|---|
| `initiative-preflight.ps1` | Worktree; rama; lista de areas probables; tabla WORKFLOW §7 | Estado Git (arbol, stash, operaciones en curso, divergencia con `origin/main`); ramas activas con rutas `merge-base..punta`; intersecciones clasificadas como candidato funcional / archivo / documento compartido de cierre; entradas de FOUNDATIONS extendidas por contratos de ramas activas (UNKNOWN para contratos V1); puntas observadas para el informe y el cuerpo del commit, nunca para el contrato | Error de fetch, arbol sucio, operacion en curso o worktree ocupado = FALLO; clasificacion funcional no determinable = UNKNOWN visible (nunca «independiente») | Si una interseccion es dependencia funcional; secuenciar o dividir |
| `candidate-check.ps1` | SHA propuesto; ruta del Freeze; ruta del contrato | READY-04/05/07/09 comprobables mecanicamente: base al dia, CI push del SHA (event, head_sha, jobs), arbol limpio, archivo congelado sin commits posteriores; plantilla del bloque de Candidato de la guia §7.1 | Cualquier condicion no verde o UNKNOWN = FALLO; nunca rellena lineas por analogia | READY-01/02/03/06/08 (alcance completo, conformidad, OV); declarar el Candidato |
| `post-merge-check.ps1` | `MERGE_SHA`; SHA del Candidato | Corrida de CI de `MERGE_SHA` con los cuatro jobs; artefacto de cobertura; dispatch del Candidato con `measured-sha` == Candidato; veredicto de si la limpieza procede; borrador del mensaje del tag `integration/<unidad>` (P-20); unidades V2 integradas antes sin su tag | Corrida ausente, job no `success`, artefacto ausente, `measured-sha` distinto o corrida de `ref` distinto de `refs/heads/main` = FALLO; no borra nada ni crea el tag | Ejecutar la limpieza; crear y publicar el tag; que hacer ante un rojo (siempre en la rama) |
| `evidence-report.ps1` | Iniciativa; rango de commits | Tabla de corridas por SHA y clase (excluye corridas de `refs/tags/*`); unidades V2 integradas sin tag de nombre exacto (P-20 regla 3); Full afirmados en cuerpos de commit vs corridas medidas; fila de metricas P-22 con UNKNOWN explicitos; entradas del registro de fundaciones con simbolos o pruebas citados inexistentes | Datos no recuperables = UNKNOWN (nunca 0); no clasifica valor | Interpretar metricas; marcar entradas obsoletas |

### P-22 — Metricas y evaluacion

**Categoria**: `[NEW]`. Registro ligero en el archivo de evidencia de la unidad (P-15), una fila por unidad:

```text
Archetype (inicial -> final; reclasificaciones y disparador) | Discovery rounds (Core + expansiones EXP) |
Architect rounds (y Review mode) | Implementation gates (total / verificables) | Full-suite runs (Core local, UI local; por punto:
cierre de gate / Candidato) | Candidate attempts (invalidaciones y causa) | CI attempts (corridas de rama; rojas; rojas no leidas
antes del siguiente gate) | Owner rounds (POLICY DECISIONS / PRODUCT VALIDATIONS; hallazgos) | Process deviations (por clase P-19)
| Freeze invalidations (por clase P-09) | Escaped findings (hallados despues del merge, p. ej. B-30)
```

Reloj entre eventos solo cuando es MEASURED desde Git o Actions; duracion activa solo declarada (guia §8). **Un dato ausente es
UNKNOWN, nunca un fallo** ni un bloqueo.

**Evaluacion de la V2: dos puntos (CR-08).**

| Punto | Cuando | Que evalua | Si falta un arquetipo |
|---|---|---|---|
| **1. Evaluacion general** | Tras **aproximadamente cinco** iniciativas V2 integradas, sean del arquetipo que sean (escala comparable a las seis iniciativas cerradas de la muestra de G1; eleccion ajustable por consenso, no umbral medido) | Hallazgos escapados (post-merge o por iniciativas posteriores); Candidatos invalidados; invalidaciones del Freeze; reclasificaciones hacia arriba; rondas de Architect vs hallazgos materiales por modo de revision; suites locales por punto y sus capturas; criterios de refutacion de H1–H9 que la muestra permita | Sus conclusiones quedan **UNKNOWN** para ese arquetipo; no bloquea la evaluacion |
| **2. Evaluacion completa por arquetipo** | Cuando los **tres** arquetipos tengan al menos una muestra real integrada | Lo mismo, desglosado por arquetipo, y las reglas que dependen de el (P-05 alcance, P-08 revision de diseño, P-09 profundidad) | — |

**Ninguno de los dos puntos es compuerta de seguridad** para iniciativas ordinarias: ninguna iniciativa espera ni se detiene por
ellos. El Coordinator propone la orden cuando se alcanza el punto; la hace una iniciativa documental con orden propia, y su
conclusion va al Owner. Una iniciativa conceptual con varias unidades cuenta **una vez**, cuando se integra su ultima unidad; sus
metricas se suman desde los archivos de evidencia de sus unidades. **Guardarrail**:
ninguna conclusion se apoya en «menos pruebas»; un aumento de hallazgos escapados obliga a revisar primero el mecanismo relajado mas
cercano (P-11 cadencia del Core; P-08 unidades sin revision de diseño propia).

**Traza de las hipotesis H1–H9 de la auditoria (EA seccion G):**

| Hipotesis | Decision que la aplica o pone a prueba | Refutacion que la evaluacion comprueba |
|---|---|---|
| H1 Rondas de diseño proporcionales | P-08 (anti-churn: ronda solo con REQUERIDO abierto) | Defectos materiales tras el Freeze en iniciativas con una sola ronda |
| H2 Chequeo de CI por gate | P-10 condicion 4 | Fallos de CI que igual solo se detectan al declarar Candidato |
| H3 Full local concentrado en el Candidato | P-11 (forma moderada; forma fuerte no adoptada) | Un defecto que solo una suite local intermedia habria capturado |
| H4 Re-fetch en momentos definidos | P-07, P-12 READY-04, P-20 | Trabajo sobre base superada no detectado por esos puntos |
| H5 Cierre concentrado | P-15 | Informacion de cierre que solo existia en una copia secundaria |
| H6 Ordenes por referencia | P-16 (con la contraevidencia de la celda de ROADMAP) | Desviaciones en reglas vinculantes que la orden no nombro |
| H7 Registro de fundaciones | P-06 | Entradas contradichas por el codigo o por un ADR posterior en menos de dos iniciativas |
| H8 Unidades de entrega funcionales | P-02 | Re-validaciones cruzadas entre unidades o fundaciones integradas sin prueba de uso |
| H9 Revision independiente vs de rol | P-08 `Review mode` (sin exigir independencia; U-08) | Tasas de hallazgos equivalentes entre modos en una comparacion controlada |

### P-23 — Dry-run obligatorio futuro (metodo y casos)

**Categoria**: `[NEW]`. G2 **no** ejecuta el dry-run normativo; el autocontrol (§11) no hallo una contradiccion interna que
exigiera ejecutarlo antes.

**Cuando**: despues de que Coordinator y Architect revisen esta Proposal y **antes** del paquete del Owner, sobre la version que
se pretende congelar. **Donde**: un documento de I-56 con orden propia (p. ej. `docs/initiatives/I-56-dry-run.md`), solo
documentacion.

**Metodo, por caso:**

1. **Estado inicial reconstruido** en el momento historico de apertura: fila u orden, `origin/main`, ramas activas, fundaciones
   integradas entonces (sin usar conocimiento posterior salvo para la comparacion del paso 4).
2. **Aplicacion paso a paso** de la V2 con traza:
   `paso | actor (Coordinator / Architect / Executor / Owner) | regla P-nn | entrada | artefacto producido | evidencia requerida |
   compuerta del Owner | camino hacia el Candidato`.
3. **Respuesta obligatoria**: «Si esta iniciativa hubiera nacido bajo V2, ¿que proceso EXACTO habria seguido?», con acciones del
   Coordinator, del Architect y del Executor, evidencia requerida, compuertas del Owner y camino del Candidato.
4. **Comprobacion de preservacion de capturas**: para cada fila de TABLE B del caso, en que etapa V2 se habria detectado; **una
   captura que la V2 habria perdido o retrasado es una contradiccion** a resolver antes del paquete del Owner.
5. **Comparacion de ceremonia**: que operaciones de TABLE C desaparecen, cuales se conservan y cuales añade la V2.
6. **Revision**: por el Architect, registrando `Review mode`.
7. **Efecto sobre la version**: si el dry-run obliga a cambiar el texto de la Proposal, el resultado es una **version nueva**, y
   Coordinator y Architect deben volver a acordar esa misma version antes del paquete del Owner (contrato I-56 §0.2).

**Casos, elegidos por evidencia:**

| | Caso | Por que este y no otro | Lo que el dry-run debe responder |
|---|---|---|---|
| **A. Small / Extension** | **I-53S (E2 de I-53)**: UI del Selectivo sobre una fundacion ya integrada; 0 rondas propias; 1 gate verificable; Owner PASS con 14 escenarios; suites pre-commit repetidas por el sello del padre (EA TABLE A; EA C-09) | Es la unica unidad de la muestra que consumio una fundacion congelada e integrada sin cambiarla; I-51 (3 gates) activa M-05 (EA B-01) y no representa EXTENSION | ¿Seria iniciativa propia o unidad de entrega (P-02), y habria absorbido E1? ¿Discovery Core basta (DC-8 sobre la fundacion de E1)? Sin revision de diseño propia bajo el Freeze de I-53 {C}, ¿la conformidad del Architect habria capturado la discrepancia editorial B-31 de E3? ¿Cuantas suites Core locales (P-11) frente a las registradas? ¿Matriz OV antes de implementar con los 14 escenarios? ¿Que ceremonia de cierre desaparece? |
| **B. Foundation Evolution** | **I-51**: RACKDUPLICAR multi-origen; extiende restamp e identidad (ADR-0009, sin ADR nuevo); Discovery con 3 HIGH (EA B-01..B-03); 1 revision con 8 cambios (B-29); INV-09 sin prueba hallado despues del merge (B-30); 3 gates, primer verificable el ultimo (EA Q6) | Pequeña en codigo pero activa M-05 sobre un contrato compartido: pone a prueba que la clasificacion sea por disparador y no por tamaño, y que la conformidad capture un invariante sin prueba | ¿Que disparadores activa y cuando (Core o expansion)? ¿Una revision adversarial + conformidad del Architect habria capturado B-29 y B-30? ¿Como serian los gates de comportamiento? ¿Donde quedan registrados `MERGE_SHA` y el CI post-merge (EA 0.5)? |
| **C. New Architecture / large** | **I-48**: kernel generico de propiedades vinculables (M-07); 8 rondas con 2 BLOCKER y 18 MATERIAL; AR4–AR7 derivadas de la reconciliacion; 4 defectos materiales hallados por el Coordinator entre gates; CI rojo sin leer de G4E a G4G; Candidato `acecde6` invalidado; DLL entregado dudoso (EA TABLE A; B-06..B-13, B-40; C-14) | Concentra los mecanismos mas discutibles (anti-churn, revision del Coordinator por gate, CI leido por gate, umbral del Candidato, identidad del DLL) y las capturas tardias de mayor valor (B-08, B-09): si la V2 las pierde, el dry-run lo expone. I-54 seria alternativa valida si Coordinator y Architect juzgan que I-48 era FOUNDATION EVOLUTION por extender I-47 | ¿Las reglas anti-churn habrian conservado B-08 (AR2) y B-09 (AR7)? ¿Que se habria congelado y que no? ¿Los gates de comportamiento y el CI leido habrian detectado B-12 antes? ¿READY-05 habria evitado `acecde6`? ¿P-14 regla 5 habria evitado B-13? |

### P-24 — Conjunto minimo de archivos normativos

**Categoria**: `[NEW]` (recomendacion). **No se crea ni edita ningun archivo en G2.**

**Evaluacion de la hipotesis inicial:**

| Hipotesis | Evaluacion | Recomendacion |
|---|---|---|
| `docs/process/initiative-workflow-v2.md` | Un segundo documento de «workflow» junto a WORKFLOW repetiria autoridad de proceso; «v2» en el nombre envejece | **Modificada**: `docs/INITIATIVE_LIFECYCLE.md` con dominio separado — diseño, revision y preparacion: P-01, P-02, P-03, P-04, P-05, P-08, P-09, P-10, P-12 (READY), P-19, P-22 —; WORKFLOW conserva Git, integracion, cadencia documental, precedencia y transicion (P-07 puntos de re-fetch, P-15, P-17, P-20, P-25). Alternativa: fundirlo en WORKFLOW (U-01) |
| `docs/process/prompt-architecture.md` | Sus reglas (P-16) solo se aplican a traves de las plantillas; dos archivos repetirian | **Consolidada** con las plantillas |
| `docs/architecture/foundations.md` | `docs/architecture/` no existe; convencion de la raiz de `docs/` en mayusculas | **Modificada**: `docs/FOUNDATIONS.md` (U-02) |
| `docs/process/initiative-templates.md` | Las plantillas de ordenes viven mejor junto al contrato-plantilla | **Modificada**: `docs/initiatives/PROMPT_TEMPLATES.md` (P-16 + P-18) |
| `docs/ORCHESTRATION.md` | Sus contenidos posibles (roles, ordenes, consenso) ya tienen dueño en el ciclo de vida y las plantillas; no hay autoridad distinta y duradera | **No se crea** |

**Resultado: tres archivos nuevos** — `docs/INITIATIVE_LIFECYCLE.md`, `docs/FOUNDATIONS.md`, `docs/initiatives/PROMPT_TEMPLATES.md`
— sin directorios nuevos.

**Archivos existentes que la integracion normativa actualizaria** (no en G2):

| Archivo | Cambio |
|---|---|
| `docs/WORKFLOW.md` | §4 enlace al ciclo de vida, puntos de re-fetch (P-07), re-fetch antes del merge y tag `integration/<unidad>` tras la limpieza (P-20); preflight de sesion escrito (P-16); §5 checklist; §8 superficies documentales y ubicacion de hashes (P-15); §10 modelo de autoridad (P-17); seccion de transicion con la tabla de verdad (P-25); referencias «§8-12» corregidas |
| `AGENTS.md` (fuera de `docs/**`: requiere declararlo y orden, contrato I-56 §12) | «Pruebas» punto 1: cadencia de la suite Core al cierre de gate (P-11); la regla de hashes pasa a remitir a WORKFLOW §8; referencia «§8-12» corregida |
| `docs/AUTOMATION_PLAN.md` | §2 remite al modelo de autoridad de WORKFLOW §10 (ejecutor inactivo; sin cambio de limites) |
| `docs/initiatives/TEMPLATE.md` | Solo campos del contrato mutable: `workflow` (solo V1 o V2; la base y la evidencia de orden van al archivo de evidencia), arquetipo y disparadores, agrupacion, Consumes/Extends/Introduces, archivos calientes, dependencias y estrategia de coordinacion (sin puntas), salidas DC enlazadas, enlaces al archivo de Freeze y al archivo de evidencia; §14 «Evidencia final» pasa a ser un enlace al archivo de evidencia (sin hashes en el contrato) |
| Plantillas nuevas (en `docs/initiatives/PROMPT_TEMPLATES.md`) | Esqueleto del archivo de Freeze de EXTENSION y del Freeze delta de unidad (P-09); esqueleto del archivo de evidencia `docs/automation/evidence/<unidad>-evidence.md` (P-15); formato del mensaje del tag `integration/<unidad>` (P-20) |
| `docs/initiatives/README.md` | Politica de indice de una linea por contrato (hacia delante) |
| `docs/context-packs/README.md`, `documentation-governance.md` | Enlazar ciclo de vida y registro; los packs no son registro |
| `docs/guias/validacion-manual-autocad.md` §7, §7.1 | Matriz OV aditiva y comprobacion del DLL (P-14); termino `FINAL_CANDIDATE_SHA` enlazando a READY del ciclo de vida; el bloque del Candidato se escribe en el archivo de evidencia o en el cuerpo del commit (P-15) |
| `docs/ARCHITECTURE.md` §4 | Enlace a FOUNDATIONS en lugar de duplicar |
| `docs/adr/` | ADR de Workflow V2 si el Owner lo decide (U-06; WORKFLOW §8 «ADR si es decision de fondo») |
| `docs/ROADMAP.md`, `docs/HANDOFF.md` | Solo en el momento de cierre de I-56 |

### P-25 — Transicion y `WORKFLOW_V2_EFFECTIVE_SHA`

**Categoria**: `[NEW]` sobre `[BIND]` contrato I-56 §0.1 y §0.2. **Ningun SHA se asigna en esta propuesta.** Todo este punto es
materia de OWN-E.

**Precondiciones, en orden:** `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** version de la Proposal → `Owner =
APPROVED` sobre esa version (decision registrada en `docs/automation/decisions/I-56.md`) → integracion normativa: gates de I-56 que
materializan la politica aprobada en los archivos de P-24 (con orden para `AGENTS.md`, contrato §12), conformidad contra la version
aprobada, y merge por la integracion V1 de I-56.

**Una sola integracion normativa, sin ediciones parciales.** Todas las ediciones normativas (archivos nuevos y cambios en WORKFLOW,
AGENTS, AUTOMATION_PLAN, TEMPLATE, guia y demas de P-24) entran en `main` en **un unico merge** de I-56. I-56 no integra antes de
ese merge ninguna edicion normativa de la V2 (las demas iniciativas siguen editando segun V1, p. ej. la tabla de WORKFLOW §7). Si la materializacion no cupiera en una integracion, **STOP**: la particion vuelve
a consenso y Owner; esta propuesta no define efectividad parcial.

**Que commit es, y que significa (CR-01).** `WORKFLOW_V2_EFFECTIVE_SHA` es **el commit de merge `--no-ff` normativo de I-56 en
`main`**, y tiene **un unico significado**: es el punto en que la V2 entra en vigor. Se deriva de Git (`git log --first-parent main`
y la rama de I-56); no se escribe dentro de si mismo. Ningun otro evento — pausa, verificacion post-merge, tag, merge de correccion —
crea un segundo momento de vigencia. Esta propuesta **no asigna** ese SHA.

**Registro durable del SHA.** I-56 es V1, asi que el tag de P-20 no le aplica por norma. Se propone que el Owner decida, con alcance
local (P-17), registrar la activacion en un tag anotado `integration/I-56` creado tras la verificacion y la limpieza de I-56, con el
mismo formato de P-20 y dos lineas propias: `WORKFLOW_V2_EFFECTIVE_SHA: <merge normativo>` (que puede diferir del destino del tag si
hubo merge de correccion) y `Claim pause lifted: <fecha>`.

**Pausa temporal de reclamos (decision del Owner, no regla de la V2).** Para que ningun reclamo arranque mientras la activacion no
esta verificada, el Owner puede decidir una pausa **de alcance local** (P-17), registrada en `docs/automation/decisions/I-56.md` en la
rama de I-56 **antes** del merge, publicada en el remoto con ella y comunicada por el Coordinator. Antes del SHA la pausa llega a
quien vaya a reclamar por esa comunicacion (sin imponer un preflight V2 a reclamos V1); despues del SHA, el preflight de P-07 lee ese
archivo mientras la rama de I-56 exista:

- **empieza** en el momento que fije esa decision, que puede ser **anterior** al merge normativo (p. ej. el inicio de la sesion de
  integracion de I-56);
- **termina** cuando la verificacion post-merge requerida esta verde (WORKFLOW 4.5.6 y 4.5.7); el Coordinator lo comunica en ese
  momento y el tag `integration/I-56` lo deja registrado despues;
- **no** cambia la vigencia ni la clasificacion: solo convierte en DESVIACION DE PROCESO un reclamo hecho mientras dura.

**Reclamo formal y orden respecto del SHA.** Un reclamo es formal cuando el remoto **acepta** el primer push de su commit de reclamo
(WORKFLOW §2, §4.1). «Antes» o «despues» de `WORKFLOW_V2_EFFECTIVE_SHA` se refiere a si ese push fue aceptado antes o despues del push
de `main` que contiene el merge normativo. Evidencia, por orden de fuerza:

1. **Ascendencia**: si la base del reclamo contiene el SHA, el reclamo es necesariamente posterior.
2. **Registro en el momento**: justo despues de que el remoto acepta el reclamo, la sesion hace `git fetch` y registra en el cuerpo del
   bootstrap la punta observada de `origin/main`. Esta evidencia solo prueba en un sentido: si esa punta **no** contiene el SHA, el
   merge normativo aun no estaba en `main` cuando se miro, luego el reclamo es **anterior** (V1). Si la punta **si** lo contiene, no
   prueba nada sobre el orden.
3. **Marcas de tiempo de Actions** de las corridas `push` del reclamo y del merge: solo apoyo, no durables ni exactas (la hora de
   creacion de una corrida no es la de aceptacion del push).

Decision: base con el SHA ⇒ posterior (T3/T4). Base sin el SHA y punta registrada sin el SHA ⇒ anterior (T1/T2). Base sin el SHA y
punta registrada con el SHA, o sin punta registrada ⇒ **T6**: el Owner decide con la evidencia disponible, incluidas las marcas de
tiempo; solo esa decision puede llevar a T5. La clasificacion se registra en el contrato (solo `workflow: V1` o `V2`) en el primer
commit posterior al reclamo; la base y la evidencia de orden van al cuerpo de ese commit y al archivo de evidencia (P-15). Un rebase
posterior **no reclasifica**.

**Tabla de verdad de la transicion:**

| # | Push del reclamo aceptado | Base del reclamo contiene el SHA | Pausa del Owner activa | Workflow | Consecuencia |
|---|---|---|---|---|---|
| T1 | Antes del SHA | No | No | **V1** | Normal; lee V1 en `WORKFLOW_V2_EFFECTIVE_SHA^1` |
| T2 | Antes del SHA | No | Si (la pausa empezo antes del merge) | **V1** | Sigue siendo V1 (contrato I-56 §0.1); DESVIACION DE PROCESO registrada; el Coordinator solo decide si se suspende hasta que termine la pausa |
| T3 | Despues del SHA | Si | Si | **V2** | DESVIACION DE PROCESO; **STOP** hasta que termine la pausa (verificacion verde); nunca se reclasifica V1 |
| T4 | Despues del SHA | Si | No | **V2** | Normal |
| T5 | Despues del SHA (decidido por el Owner desde T6) | No (base obsoleta) | Si o No | **V2** | DESVIACION DE PROCESO (el reclamo no partio de `origin/main` actual, WORKFLOW §4.1); **STOP**; rebase sobre un `main` que contenga el SHA antes de trabajo sustantivo; si la pausa sigue activa, tambien T3 |
| T6 | Orden no determinable con la evidencia | — | — | **Sin clasificar** | **STOP**; el Coordinator lo escala y el Owner decide con la evidencia disponible; nunca se asume V1 por defecto |
| T7 | Unidad de entrega nueva de una iniciativa diseñada bajo V1, reclamada despues del SHA | Si | No | **V2** | Consume el Freeze V1 solo tras la comprobacion de compatibilidad de P-02; STOP si exige un contrato materialmente distinto |
| T8 | Unidad nueva, reclamada despues del SHA, de una iniciativa grandfathered por nombre (I-49, I-52, I-55) | Si | No | **V2** | Como T7. El grandfathering por nombre cubre sus reclamos, ramas y contratos **existentes**; la unidad nueva lleva su contrato, su Freeze delta y sus A-n en **archivos propios** y nunca escribe en el contrato ni en el registro de decisiones de la iniciativa V1 (EA H.1-4). Interpretacion que confirma el Owner (OWN-E) |

Consecuencias fijas: **no hay opt-in ni opt-out**; la version de workflow nunca se hereda entre reclamos (P-02); una iniciativa V1
lee sus normas en `WORKFLOW_V2_EFFECTIVE_SHA^1`; I-56 sigue siendo V1 tambien despues de su propio merge.

**Verificacion roja del merge normativo.** La V2 ya es vigente desde el merge; la pausa sigue activa. Se corrige en la rama de I-56 por
la integracion V1 (nuevo merge con su propio CI). Si la correccion exige cambiar texto normativo aprobado, **STOP**: vuelve a consenso
y Owner. El merge de correccion **no** es un nuevo `WORKFLOW_V2_EFFECTIVE_SHA`; el tag `integration/I-56`, si el Owner lo decide,
apunta al merge verificado y registra el efectivo en su linea propia.

**Sin opt-in retroactivo.** Ninguna iniciativa grandfathered «adopta» la V2 (I-56 no reescribe contratos activos, contrato §0.1). Una
migracion caso por caso seria decision del Owner fuera de I-56 (U-10).

### P-26 — No-objetivos

**Vinculantes** `[BIND]` (EA H.1): sin cambio de producto; sin cambio de pruebas de producto; sin implementacion de CI; sin
implementacion de scripts; sin V2 retroactiva; sin T0–T4; sin R0–R4; sin Quick CI; sin eliminar la validacion Full del Candidato;
sin eliminar la Owner Validation; sin relajar exact-SHA; sin identidad de evidencia por mismo arbol; sin merges automaticos.

**Especificos de esta propuesta:**

- Sin puntuaciones numericas de agrupacion, materialidad o riesgo.
- Sin clasificacion automatica de arquetipos; la herramienta futura solo aporta datos.
- Sin exigir revision de Architect independiente (U-08) ni afirmar independencia no provista.
- Sin poblar el registro de fundaciones copiando prosa existente.
- Sin reescribir registros historicos, HANDOFF pasado ni contratos cerrados.
- Sin cambiar la poblacion ni los disparadores de `ci.yml`; sin cambiar la cadencia de cobertura.
- Sin convertir la duracion activa del Owner en campo obligatorio.
- Sin aceptar, rechazar ni reabrir ADR-0033 (sigue `propuesto`).
- Sin guardias de patch-id ni igualdad de arbol como sustituto de rebase o de evidencia.
- Sin crear `docs/ORCHESTRATION.md`.
- Sin mover ni forzar tags `integration/*`, sin notas de Git y sin commits de registro sobre `main` para los hechos posteriores al merge.
- Sin heredar la version de workflow entre reclamos ni reescribir un Freeze V1 consumido por una unidad V2.

---

## 6. Decisiones reservadas al Owner (paquete futuro; NO se solicitan ahora)

Se presentarian **despues** de `Coordinator = AGREED` y `Architect = AGREED` sobre la misma version, y del dry-run (P-23).

| ID | Decision | Decisiones P | Por que es del Owner |
|---|---|---|---|
| OWN-A | Arquetipos finales y su efecto en Discovery, revision de diseño y Proposal | P-03, P-04, P-05, P-09 | Politica nueva que decide cuanta revision recibe cada iniciativa |
| OWN-B | Regla de participacion del Architect, reglas anti-churn, conformidad y Freeze | P-08, P-09, P-19 | Hace condicional una practica de revision que G1 mostro valiosa |
| OWN-C | Cadencia de la suite Core local: al cierre de gate en lugar de antes de cada push; confirmar la no adopcion de H3 fuerte | P-11 | Cambia AGENTS «Pruebas» punto 1 |
| OWN-D | Umbral de preparacion del Candidato y Candidatos intermedios | P-12 | Nueva compuerta antes de la evidencia |
| OWN-E | Transicion: merge normativo unico = `WORKFLOW_V2_EFFECTIVE_SHA` = punto de vigencia; pausa temporal de reclamos y tag `integration/I-56` como decisiones locales; clasificacion por el reclamo formal propio con la evidencia de orden de P-25; tabla de verdad T1..T8, incluida la lectura de que el grandfathering por nombre de I-49, I-52 e I-55 cubre sus reclamos existentes y no unidades nuevas | P-20, P-25 | Aplica e interpreta la invariante vinculante de transicion |
| OWN-F | REFERENCE OVER REPETITION y plantillas como norma | P-16, P-18 | Cambia como se ordena el trabajo |
| OWN-G | Practicas retiradas o condicionales: commits `-CLOSE` por gate; revision de diseño propia en unidades bajo un Freeze ya revisado; Discovery completo cuando basta el Core; suite Core local antes de cada push interno | P-05, P-08, P-10, P-11 | Retira o condiciona pasos |
| OWN-H | Politica Git: re-fetch antes del merge y vuelta al rebase; namespace de tags anotados `integration/*` como registro durable post-merge, inmutabilidad y proteccion de tags | P-15, P-20, P-25 | Precision y ampliacion de politica Git |
| OWN-I | Politica de cobertura: **sin cambio** (confirmacion) | P-13 | Asegura que no hay cambio implicito |
| OWN-J | Agrupacion de IDs con las dos condiciones (mismo problema de usuario y fundacion de diseño compartida), unidades de entrega, fusion de la fundacion con su primer consumidor, clasificacion por reclamo propio y consumo de un Freeze V1 por una unidad V2 | P-02 | Cambia como se abren iniciativas |
| OWN-K | Superficies documentales (contrato mutable, Freeze inmutable, archivo de evidencia), ubicacion de hashes y conteos, registro de fundaciones con la regla A/B de discrepancias y su ubicacion | P-06, P-09, P-15 | Cambia WORKFLOW §8, AGENTS, la guia §7.1 y crea obligaciones de verificacion |
| OWN-L | Modelo de autoridad (incluido ADR aceptado sobre convencion de AGENTS) y camino de una decision del Owner a norma durable | P-17 | Cambia WORKFLOW §10 y AUTOMATION_PLAN §2 |
| OWN-M | Matriz OV aditiva y comprobacion del DLL entregado | P-14 | Toca la forma de la validacion del Owner (sin reducirla) |

**Aprobacion global.** Lo que no figura como decision especifica (P-01, P-07, P-21 como diseño, P-22, P-23, P-24 en lo no cubierto
por OWN-K, P-26) no queda aprobado por omision: entra en la aprobacion global de la version (EA H.1-3).

**Decidibles por Coordinator + Architect** (tecnicas o editoriales, dentro de lo aprobado): redaccion de plantillas; campos exactos
de una entrada del registro; redaccion de disparadores M/EXP sin cambiar su sentido; nombres de clases de desviacion; formato del
dry-run; nombres de campos de metricas; consolidacion final de archivos dentro de OWN-K.

---

## 7. Politicas V1 que se proponen conservar `[KEEP]`

1. Git y worktrees: 1 iniciativa Git = 1 rama = 1 worktree = 1 fila de ROADMAP; prefijos; reclamo atomico; rebase al abrir sesion;
   push al cerrar; `--force-with-lease`; borrado seguro (WORKFLOW §1, §2, §3, §4).
2. Nunca un commit directo sobre `main`; un CI post-merge rojo se corrige en la rama (WORKFLOW 4.5.6).
3. Integracion 4.5.1–4.5.7 y limpieza tras ambas compuertas (WORKFLOW §4).
4. Definicion de Candidato (todo SHA entregado para validar o integrar) y composicion de su evidencia: Core y UI Full local, builds
   Debug UI y Plugin, CI verde exacto; LC-UI; el Core del CI no sustituye al local (AGENTS punto 1; guia §7.1).
5. Reutilizacion de evidencia solo por SHA exacto, mismo proposito y alcance; clases que no se sustituyen; tres invalidadores;
   commit documental no hereda (AGENTS).
6. Una seleccion que no selecciona nada es FALLO (AGENTS).
7. Owner Validation por disparador de cambio de dibujo; metadata monotonica; checklist y criterios de la guia; duracion activa
   experimental (AGENTS punto 5; AUTOMATION_PLAN §11; guia §6–§8).
8. Cadencia de cobertura (WORKFLOW 4.5.2.bis, 4.5.6, 4.5.7; `ci.yml`).
9. HANDOFF solo al integrar; ROADMAP en tres momentos; guias y README antes de integrar (WORKFLOW §2, §8; AGENTS punto 4).
10. ADR antes de implementar una decision de arquitectura; solo el Owner acepta o rechaza (WORKFLOW §8; adr/README).
11. Sin merge automatico; ejecutor automatizado inactivo (AUTOMATION_PLAN §13 y estado de activacion).
12. Registro historico no es precedente ni se corrige hacia atras (AGENTS).
13. «Una iniciativa cabe en 1-3 sesiones» (WORKFLOW §2), pendiente de U-05.
14. Tabla de archivos calientes y regla de actualizarla al mover archivos (WORKFLOW §7).
15. Ningun trabajo sustantivo antes del bootstrap (WORKFLOW §2): el intake es planificacion (§3).
16. Los contratos no copian conteos de pruebas ni hashes (README de iniciativas): la evidencia va a su archivo propio (P-15).
17. Tags anotados como practica Git existente (`archive/*`, WORKFLOW §3); el namespace `integration/*` es nuevo (P-20, OWN-H).

## 8. Politicas V1 escritas que se proponen cambiar `[CHANGE]`

| # | Regla V1 | Fuente | Cambio | Decision |
|---|---|---|---|---|
| 1 | Suite Core en local en la iteracion ordinaria «sin cambio», es decir, antes de cada push | AGENTS «Pruebas» punto 1 (tabla) | Suite Core local al cierre de cada gate, en el Candidato y en el cierre; no antes de cada push interno | P-11, OWN-C |
| 2 | Hallazgo fuera de alcance → ideas-futuras «al detectarlo» | WORKFLOW §8 | Lista local durante el trabajo; traspaso en el commit de cierre | P-15, OWN-K |
| 3 | Conteos de pruebas y hashes solo en HANDOFF §12; el registro de la ronda del Candidato va en el contrato o en el cuerpo del commit | WORKFLOW §8; AGENTS «Flujo Git multi-agente»; guia §7.1 | Viven en cuerpos de commit, en el archivo de evidencia de la unidad y en su tag de integracion; HANDOFF enlaza; el contrato sigue sin hashes (README de iniciativas conservado) | P-15, P-20, OWN-K |
| 4 | Precedencia en dos ordenes distintos (AUTOMATION_PLAN: AGENTS y WORKFLOW por encima de ADR aceptados) | WORKFLOW §10; AUTOMATION_PLAN §2 | Autoridad por dominio con reglas de conflicto; ADR aceptado sobre convencion general en su alcance; la orden de gate solo estrecha | P-17, OWN-L |
| 5 | Silencio entre Candidato y merge | WORKFLOW 4.5 | Re-fetch antes del merge; si `main` avanzo, volver a 4.5.1 | P-20, OWN-H |

Todo lo demas que la V2 introduce es `[NEW]` (tabla §4) y codifica o sustituye practica no escrita (EA H.2-13) — por ejemplo el
momento del indice de ADR (P-15) —; tambien requiere la aprobacion de la version antes de ser norma.

## 9. Alternativas historicas no reabiertas `[NOT-REOPENED]`

| EA | Alternativa | Por que no se reabre aqui |
|---|---|---|
| H.3-1 | Seleccion por impacto como compuerta | Sin instrumento de capacidad de deteccion nuevo |
| H.3-2 | Seleccion por FQN, rutas o grafo; FQN como frontera | Sin evidencia nueva que refute el racional registrado |
| H.3-3 | Taxonomia de pruebas / etiquetas masivas | Sin consumidor operacional; los arquetipos no seleccionan pruebas |
| H.3-4 | Clave de estado de validacion por contenido | Sin evidencia nueva; ademas cualquier forma de identidad de evidencia por contenido choca con H.1-10/H.1-11 |
| H.3-5 | Retirar la cobertura por completo | P-13 conserva la cadencia |
| H.3-6 | Reducir la Owner Validation por politica general | 0 hallazgos en 7 rondas no prueban falta de valor (EA Q2) |
| H.3-7 | Multiples hilos STA | Fuera del objeto de proceso |
| H.3-8 | Golden DWG | Sin infraestructura nueva |
| H.3-9 | Mapas de pruebas o de riesgo mantenidos a mano | La tabla de preflight (P-07) se deriva de Git y no clasifica riesgo |

Casos especiales (EA H.3): T0–T4, R0–R4, igualdad de arbol como identidad y Quick CI **no son alternativas reabribles**: estan
prohibidos por EA H.1 y esta propuesta no los contiene bajo ningun nombre (autocontrol de V1 §11 SC-01..SC-03 y de V2 §11).

## 10. Preguntas abiertas

| ID | Pregunta | Quien la cierra |
|---|---|---|
| U-01 | ¿`INITIATIVE_LIFECYCLE.md` separado por dominio o fundido en WORKFLOW? | Coordinator + Architect |
| U-02 | ¿`docs/FOUNDATIONS.md` o `docs/architecture/foundations.md`? | Coordinator + Architect (dentro de OWN-K) |
| U-03 | ¿El dispatch de cobertura del Candidato en la rama (4.5.2.bis) y el de `main` (4.5.7) sobre el mismo SHA medido son la misma evidencia, dado que la definicion del workflow puede diferir? | Architect; si cambia politica, Owner (OWN-I) |
| U-04 | ¿Basta la suite Core local al cierre de gate, o la clase Core-Windows exige mas? Se mide con P-22 | Evaluacion de la V2 |
| U-05 | ¿«1-3 sesiones» (WORKFLOW §2) debe sustituirse por los criterios de division de P-10? | Coordinator + Architect; Owner si cambia |
| U-06 | ¿La V2 se registra ademas como ADR (WORKFLOW §8: «ADR si es decision de fondo»)? | Owner |
| U-07 | ¿Tag anotado para el commit efectivo? | **CERRADA en V2**: se propone como decision local del Owner un tag `integration/I-56` con linea propia `WORKFLOW_V2_EFFECTIVE_SHA` (P-25); la politica de tags queda en OWN-H |
| U-08 | ¿NEW ARCHITECTURE debe exigir revision en sesion separada? | Owner, con evidencia de P-22 |
| U-09 | ¿Podria una conformidad acotada al delta de un rebase sustituir a la completa en algun caso? Hoy P-19 exige la completa y cada rebase repite conformidad y evidencia del Candidato (coste sin cota; se mide en P-22 como Candidate attempts) | Architect; Owner si relaja |
| U-10 | ¿Migracion caso por caso de una iniciativa grandfathered a V2? | Owner, fuera de I-56 |
| U-11 | ¿Donde se registran de forma durable `MERGE_SHA`, CI post-merge y cobertura del Candidato? | **CERRADA en V2**: tag anotado `integration/<unidad>` (P-20); requiere OWN-H |
| U-12 | ¿«Aproximadamente cinco» iniciativas para la evaluacion general es la escala adecuada? | Coordinator + Architect (P-22; ya no depende de cubrir los tres arquetipos) |
| U-13 | ¿Como se nombra en HANDOFF la seccion real que sustituye a «§8-12»? | Coordinator (editorial) |
| U-14 | ¿Se poda la narrativa historica acumulada en HANDOFF, y con que regla compatible con «registro historico»? | Owner |
| U-15 | **Nueva en V2.** El push de un tag dispara una corrida de CI sobre el commit etiquetado (medido en `archive/*`): coste extra por integracion y posible confusion con la evidencia post-merge. P-20 la declara no-evidencia; evitarla exigiria filtrar tags en `ci.yml`, cambio de CI fuera de I-56 | Owner (en otra iniciativa, si se quiere) |
| U-16 | **Nueva en V2.** Proteccion de los tags `integration/*` en GitHub | **CERRADA en V2**: es precondicion (P-20 regla 9); su configuracion es decision del Owner dentro de OWN-H |
| U-17 | **Nueva en V2.** El orden entre el reclamo y el merge normativo se decide por ascendencia y por la punta de `origin/main` registrada justo tras el reclamo (P-25); las marcas de tiempo de Actions solo apoyan. ¿Basta el registro manual en el cuerpo del bootstrap, o conviene automatizarlo en `initiative-preflight.ps1`? | Coordinator + Architect |
| U-18 | **Nueva en V2.** Una unidad V2 que consume un Freeze V1 sin revision del Architect registrada recibe como minimo la revision del Freeze de una EXTENSION independiente (P-02 punto 5); ¿basta para Freezes V1 de FOUNDATION EVOLUTION o NEW ARCHITECTURE? | Architect |

## 11. Autocontrol de V2 antes del commit

El autocontrol de V1 (SC-01..SC-29) sigue registrado en [V1 §11](I-56-proposal-v1.md) y no se repite. Este apartado cubre solo la
reconciliacion CR-01..CR-08: autorrevision del redactor y una pasada adversarial de solo lectura por un subagente con contexto propio
lanzado desde esta sesion. **No es la revision del Coordinator ni del Architect** (Architect = NOT OPEN).

**Comprobaciones pedidas por el Coordinator:**

| # | Comprobacion | Resultado | Donde |
|---|---|---|---|
| 1 | `WORKFLOW_V2_EFFECTIVE_SHA` tiene un solo significado | **PASS**. Definido una vez en P-25 como el merge normativo y punto de vigencia; ni la pausa, ni la verificacion, ni el tag, ni un merge de correccion crean otro momento | P-25 |
| 2 | Ninguna unidad reclamada despues del SHA hereda V1 | **PASS**. P-02 y P-25 (T7, T8): cada unidad se clasifica por su propio reclamo; se retiro la herencia por figurar en un Freeze anterior | P-02, P-25 T7 |
| 3 | Ninguna escritura de evidencia mutable invalida la regla de integridad del Freeze | **PASS tras SC-V2-01 y SC-V2-02**. Freeze en archivo propio con trailer; todo cambio posterior es A-n en el registro de decisiones; sin commits de evidencia entre READY-04 y el cierre | P-09, P-15, READY-09 |
| 4 | La evidencia post-merge tiene un hogar durable sin merge recursivo | **PASS**. Tag anotado `integration/<unidad>`: no es commit, sin recursion; tag ausente detectado; corrida del push del tag excluida como evidencia | P-20 |
| 5 | Una fundacion obsoleta o contradictoria no se consume en silencio | **PASS tras SC-V2-05**. EXP-01 A = STOP en Discovery, cierre de gate y conformidad; B solo confirmada por Coordinator (+ Architect en FE/NA); P-17 regla 7 ya no aplaza | P-05, P-06 regla 5 |
| 6 | IDs tecnicamente parecidos pero funcionalmente ajenos no se agrupan | **PASS**. Regla conjuntiva (a)+(b); re-prueba I-53, I-50, I-50×I-51 e I-47×I-54 coherente con la historia | P-02 |
| 7 | Ningun metadato vivo de puntas remotas se vuelve durable en contratos | **PASS tras SC-V2-03**. Contrato sin puntas ni base; puntas al cuerpo del commit e informe; base del reclamo y del Candidato al archivo de evidencia | P-07, P-15, plantillas |
| 8 | La V2 se puede evaluar aunque NEW ARCHITECTURE sea rara | **PASS**. Evaluacion general tras ~5 iniciativas con arquetipos ausentes como UNKNOWN; completa por arquetipo cuando haya muestras; ninguna es compuerta | P-22 |
| 9 | Ningun hallazgo introdujo T0–T4, R0–R4 ni Quick CI | **PASS**. Solo aparecen como prohibiciones; la poblacion de `ci.yml` no cambia | todo |
| 10 | La Proposal V1 queda sin cambios | **PASS**. `git diff --quiet HEAD -- docs/initiatives/I-56-proposal-v1.md` sin cambios; blob `b296e26` igual al de `HEAD` | Git |

**Pasada adversarial de solo lectura** sobre el primer borrador de V2: 3 HIGH, 11 MEDIUM, 8 LOW; comprobaciones 3, 5 y 7 en FALLO.
Se corrigio antes de una segunda pasada:

| ID | Hallazgo | Correccion |
|---|---|---|
| SC-V2-01 | HIGH: cambios posteriores al Freeze de clase «solo Coordinator» (añadir escenarios OV, re-secuenciar gates) solo podian hacerse editando el archivo congelado | Todo cambio posterior a un elemento congelado es enmienda A-n en `decisions/<unidad>.md`; READY-08, READY-09 y la conformidad leen Freeze + A-n (P-09, P-14) |
| SC-V2-02 | HIGH: escribir la evidencia de READY en el archivo de evidencia creaba un SHA nuevo que invalidaba al Candidato | Sin commits entre READY-04 y el commit de cierre; esa evidencia va al informe y se escribe solo en el cierre (P-12, P-15, plantilla D) |
| SC-V2-03 | HIGH: unidades nuevas de las iniciativas grandfathered por nombre sin resolver | T8: V2 por reclamo propio, con archivos propios y sin escribir en los de la iniciativa V1; interpretacion en OWN-E |
| SC-V2-04 | MEDIUM: base del reclamo y evidencia de orden en el contrato | El contrato solo guarda `workflow`; base y orden van al cuerpo del commit y al archivo de evidencia |
| SC-V2-05 | MEDIUM: evidencia de orden basada en marcas de tiempo de Actions; P-17 regla 7 abria un aplazamiento; B autodeclarable; cierre de gate sin la condicion A; P-19 no cambiaba | Orden por ascendencia y punta registrada tras el reclamo (Actions solo apoyo; empate = T6); excepcion en P-17 regla 7; B confirmada por Coordinator (+ Architect); condicion añadida a P-10; P-19 hace NON-CONFORMING toda A abierta |
| SC-V2-06 | MEDIUM: el tag se aplicaba por norma a iniciativas V1 e I-56; `FINAL_MAIN_SHA` como punta viva; tags de correccion invisibles; tag ausente no detectado | Tags por norma solo para unidades V2; `integration/I-56` como decision local con linea `WORKFLOW_V2_EFFECTIVE_SHA`; `FINAL_MAIN_SHA` = merge verificado; consumidores leen `integration/<unidad>*`; deteccion de tag ausente; proteccion de tags como precondicion |
| SC-V2-07 | MEDIUM: identidad del commit de Freeze no estable ante rebase; integridad indefinida para un Freeze V1 consumido; T2 parecia una salida a V1 | Trailer `Freeze:` estable ante rebase; secciones V1 citadas comprobadas desde una referencia registrada; T2 reescrita (sigue V1, el Coordinator solo decide suspender) |
| SC-V2-08 | LOW: arquetipo de un Freeze V1; corrida roja del push del tag; sobreafirmacion sobre I-50 y el restamp; bloques de estado en una Proposal congelada; «declara» vs «registra»; estados de la tabla de reconciliacion; conteo y disparo de la evaluacion | Clasificacion desde cero para Freeze V1; exclusion de `refs/tags/*`; redaccion de EA §6 corregida; estado posterior fuera del Freeze; la limpieza sigue declarando terminada la integracion; «RESOLVED IN V2 (pendiente de revision del Coordinator)»; iniciativa conceptual cuenta al integrar su ultima unidad y el Coordinator propone la orden |

**Segunda pasada** sobre las correcciones: todas las comprobaciones 1–10 en PASS; 16 de 22 hallazgos resueltos y 6 parciales; 1 HIGH,
4 MEDIUM y 5 LOW nuevos introducidos por las correcciones. Todo se corrigio antes del commit:

| ID | Hallazgo | Correccion |
|---|---|---|
| SC-V2-09 | HIGH: la evidencia de orden «punta registrada contiene el SHA ⇒ reclamo posterior» estaba invertida y podia volver V2 un reclamo anterior | Esa evidencia solo prueba «anterior» (punta sin el SHA); base sin el SHA con punta que lo contiene = T6; T5 solo por decision del Owner |
| SC-V2-10 | MEDIUM: la ventana sin commits prohibia correcciones y el registro de rondas fallidas; una vuelta a 4.5.1 tras el cierre dejaba evidencia obsoleta | Solo se prohiben commits documentales; correccion reinicia READY-02/READY-04; un commit documental por ronda invalidada; el cierre se rehace con el Candidato nuevo |
| SC-V2-11 | MEDIUM: A-n en `decisions/<unidad>.md` invisibles para unidades hermanas | A-n de un Freeze conceptual en `decisions/<I>.md`, leidas por todas sus unidades; por unidad solo para Freeze delta y T8 |
| SC-V2-12 | MEDIUM: comprobacion del trailer engañable o insatisfacible | Un unico commit con el trailer, que modifica el archivo y es el ultimo en tocar la ruta; sin renombres |
| SC-V2-13 | MEDIUM: el prefijo `integration/<unidad>*` coincidia con unidades hermanas y ordenaba N lexicamente | Nombres exactos y orden numerico (`--sort=v:refname`) |
| SC-V2-14 | LOW y parciales: plantilla D; `MERGE_SHA verificado`; proteccion antes del tag de I-56 y efecto de su ausencia; frases de grandfathering sin matiz; preflight V2 impuesto a reclamos V1; cambio de secciones V1 consumidas sin consecuencia; §0 y §11 inexactos | Plantilla D alineada; `FINAL_MAIN_SHA`; proteccion como precondicion de todo tag y U-16 cerrada; «reclamos existentes» y T8; pausa comunicada por el Coordinator antes del SHA; cambio de seccion V1 = STOP y compatibilidad de nuevo; textos corregidos |

**Limites**: autocontrol desde la misma sesion que redacto. La consistencia del mecanismo de tags y de la tabla de verdad frente a
casos reales la pondran a prueba la revision del Coordinator, la revision del Architect cuando se abra y el dry-run (P-23).
