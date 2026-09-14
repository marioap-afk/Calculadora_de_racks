# I-56 G2 — Workflow V2 Proposal V1

> ```text
> PROPOSAL V1 — NOT CONSENSUS
>
> Proposal Version          = V1
> Coordinator               = REVIEW REQUIRED
> Architect                 = REVIEW REQUIRED
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
> **no** se aplica a ellos.

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
  `OWN-x` decisiones reservadas al Owner; `U-nn` preguntas abiertas; `SC-nn` hallazgos del autocontrol. Ningun prefijo se usa
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
| H.1-4 | No modificar contratos activos de I-49, I-52, I-55 | Ninguna regla se les aplica; P-02 cita I-55 solo como lectura prudente |
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
     Extension            : seccion Freeze en el contrato; Coordinator + revision de Freeze del Architect
     Foundation Evolution : Proposal + revision adversarial del Architect
     New Architecture     : Proposal (+ ADR propuesto) iterada Coordinator <-> Architect
4. CONSENSUS FREEZE                         (P-09) + decisiones de politica del Owner cuando apliquen
                                            + matriz OV (P-14)
5. GATES DE IMPLEMENTACION                  (P-10, P-11) unidades de comportamiento; revision del Coordinator por gate
6. PREPARACION DEL CANDIDATO                (P-12 READY-01..09; incluye rebase final V1 y conformidad P-19)
7. FINAL_CANDIDATE_SHA + evidencia V1       (AGENTS «Pruebas» punto 1; guia §7.1) + Owner Validation donde se active
8. INTEGRACION                              (V1 WORKFLOW 4.5.4–4.5.7 + re-fetch antes del merge + cierre concentrado P-15, P-20)
9. LIMPIEZA (V1, tras ambas compuertas)     + informe final (plantilla E) + fila de metricas (P-22)
```

**El intake es planificacion** (momento 1 de WORKFLOW §2): usa ROADMAP, `ideas-futuras.md`, el registro de fundaciones y Git; **no
lee codigo ni mide** (EA O-9; WORKFLOW §2 «Ningun trabajo sustantivo antes de que el bootstrap este versionado»). Lo que el intake
deja UNKNOWN lo resuelve el Discovery Core despues del bootstrap.

En una **iniciativa conceptual con unidades de entrega** (P-02), los pasos 0, 2, 3 y 4 ocurren **una vez** para la iniciativa
conceptual, y los pasos 1 y 5–9 ocurren **por unidad de entrega**. El Discovery y el diseño conceptuales se hacen en la primera
unidad reclamada, cuyo contrato es el contrato conceptual.

---

## 4. Tabla normativa de la propuesta

Toda regla V1 **escrita** que la V2 cambia aparece aqui marcada `[CHANGE]` y se repite en §8. «Global (H.1-3)» = sin decision
especifica del Owner, pero sujeta a la aprobacion global de la version. Cada regla tiene **un** destino normativo (P-24).

| Area | Current V1 | Proposed V2 | Reason / G1 evidence | Safety mechanism | Owner decision required? | Architect concern? | Normative destination if accepted |
|---|---|---|---|---|---|---|---|
| Apertura / bootstrap | Fila de ROADMAP o autorizacion (d); reclamo atomico; bootstrap = contrato + fila (WORKFLOW §2, §4.1) `[KEEP]` | Git y ROADMAP sin cambio. El contrato añade: `workflow: V1/V2` (P-25), agrupacion, arquetipo provisional con disparadores, tabla de preflight y Consumes/Extends/Introduces. Una unidad de entrega nace con **contrato delta** (P-02) `[NEW]` | EA C-29 (contratos minimos con 18–22% re-enunciado y el error B-31); EA §6; EA C-24 | Reclamo, rama y worktree intactos; el delta cita secciones exactas del contrato conceptual | Global (H.1-3) | Bajo | TEMPLATE (campos); INITIATIVE_LIFECYCLE (reglas) |
| Agrupacion de IDs | Sin regla; «una iniciativa cabe en 1-3 sesiones; si crece, se parte» (WORKFLOW §2) `[KEEP]` | FUNCTIONAL ID != INITIATIVE; regla cualitativa agrupar / secuenciar / separar; unidades de entrega; la fundacion se funde con su primer consumidor salvo criterio de division; clasificacion heredada de la primera unidad (P-02) `[NEW]` | EA Q13 (I-53 ahorro diseño y triplico ceremonia); EA Q1; EA C-13 | Cada unidad conserva reclamo, Candidato, merge y CI post-merge propios; ninguna hereda evidencia | Si: OWN-J | Medio: fundir fundacion y consumidor agranda la unidad | INITIATIVE_LIFECYCLE |
| Clasificacion / arquetipos | No existe | EXTENSION / FOUNDATION EVOLUTION / NEW ARCHITECTURE por disparadores M-01..M-08; UNKNOWN = activado; solo gobierna alcance del Discovery, rondas de diseño y profundidad de la Proposal (P-03, P-04) `[NEW]` | EA Q4, Q5, Q14; EA B-01..B-05 (Discovery con valor) | No cambia CI, suites, Candidato, READY, conformidad, cobertura, OV ni exact-SHA | Si: OWN-A | **Si**: riesgo de uso como nivel de riesgo (argumento en P-03) | INITIATIVE_LIFECYCLE |
| Discovery | Practica no escrita (EA H.2-13) | DISCOVERY CORE obligatorio con lectura de codigo tras el bootstrap + DISCOVERY CONDITIONAL por EXP-01..EXP-08 autorizado por el Coordinator (P-05) `[NEW]` | EA hechos E.1 (defectos HIGH por Discovery); EA C-28; EA §6; EA Q14 (~20% del Discovery de I-54 fue re-medicion) | Core exige evidencia de codigo aun con registro; expansion documentada; nunca «auditar todo RackCad» por defecto | Si: OWN-A | Si: Core insuficiente en fundaciones fragmentadas | INITIATIVE_LIFECYCLE |
| Reutilizacion de fundaciones | ADR + ARCHITECTURE + Context Packs (obsoletos, EA hechos A.18) `[KEEP]` para ADR y ARCHITECTURE | Registro breve con punteros verificables; los consumidores lo verifican contra codigo; codigo y ADR aceptado prevalecen (P-06) `[NEW]` | EA §6; EA Q11; EA B-35 | Registro = puntero, no verdad; discrepancia = EXP-01; solo lo editan quienes extienden o introducen, en su cierre | Si: OWN-K | **Si**: obsolescencia y propagacion de prosa erronea (Header Mutation) | FOUNDATIONS (reglas en su cabecera) |
| Coordinacion paralela / archivos calientes | Estorbos en ROADMAP; tabla de archivos calientes (WORKFLOW §7); rebase al abrir sesion (§4.2) `[KEEP]` | Preflight ligero derivado de Git; distingue dependencia funcional / archivo compartido / documento compartido de cierre; puntos de re-verificacion definidos (P-07) `[NEW]` | EA C-24; EA B-33 (E3-C BLOCKED); EA O-4 (I-54 G7A); EA Q12 (todos los conflictos medidos fueron documentales) | Sin registro central; un indice nunca crea dependencia funcional; sin rebases nuevos | Global (H.1-3) | Bajo | WORKFLOW §4 (puntos de re-fetch) |
| Proposal | Practica: un archivo por version; versiones delta o acumulativas | Profundidad por arquetipo; version autocontenida sin bloques de estado arrastrados; «Cambios respecto de Vn-1»; errata confirmada antes del Freeze (P-08, P-09) `[NEW]` | EA §5.1 (I-48 delta en 8 archivos; I-54 90.4% arrastrado); EA C-18 | Freeze autocontenido e identico a lo acordado; ningun hallazgo que toque un elemento congelable se trata como errata | Si: OWN-A | Medio | INITIATIVE_LIFECYCLE |
| Participacion del Architect | Practica: revision de rol dentro de la sesion executor; independencia UNKNOWN (EA 0.4) | EXTENSION independiente: revision del Freeze por el Architect; unidad de entrega bajo un Freeze ya revisado: sin revision de diseño salvo disparador; FOUNDATION EVOLUTION: 1 revision adversarial + re-revision del delta; NEW ARCHITECTURE: rondas con reglas anti-churn; modo de revision registrado (P-08) `[NEW]` | EA Q4 (primera ronda material en todas las unidades cerradas con ronda); EA Q5; EA TABLE A (E2/E3 sin rondas bajo el Freeze de I-53 {C}); EA hechos C.9 | Nunca hay diseño sin revision del Architect salvo bajo un Freeze ya revisado; no se afirma independencia no provista | Si: OWN-B | **Si** | INITIATIVE_LIFECYCLE |
| Consensus Freeze | Practica («congelacion»); acuerdo repartido en varios archivos (I-48 V8) | Elementos congelados y no congelados; artefacto autocontenido; enmiendas A-n; invalidacion clasificada por quien decide (P-09) `[NEW]` | EA §5.1; EA C-14; EA C-23 (16 menciones de integridad del blob) | Cambio material o debilitar una obligacion de prueba reabre al Architect; alcance reabre al Owner | Si: OWN-B | Si | INITIATIVE_LIFECYCLE |
| Gates de implementacion | TEMPLATE §8: «cada fase termina con evidencia revisable»; practica de micro-gates por capa | Gates = unidades de comportamiento verificables; varios commits por gate; criterios de division; cierre con RED→GREEN focal, suite Core local, CI leido y revision del Coordinator; sin commits `-CLOSE` obligatorios (P-10) `[NEW]` | EA Q6 (primer gate verificable: I-50 commit 17 de 20; I-54 solo G7); EA hechos E.3, E.4; EA B-12; EA O-6 | RED focal preservado; CI del cierre leido antes de abrir el siguiente gate | Si: OWN-G | Medio | INITIATIVE_LIFECYCLE |
| Pruebas de iteracion | AGENTS «Pruebas» punto 1 (tabla): suite Core en local en la iteracion ordinaria «sin cambio» junto a UI «NO obligatoria antes del push», es decir, Core local antes de cada push; 0 seleccionadas = FALLO | Focales + relevantes durante el gate; **suite Core completa en local obligatoria al cierre de cada gate** sobre su SHA, no antes de cada push interno; LC-UI y 0 seleccionadas = FALLO sin cambio (P-11) `[CHANGE]` | EA C-08..C-11 (Core repetido por orden de commit y habito); EA Q7; EA B-17 y B-12 (plataforma y runner los detecto el CI) | CI push ejecuta Core y UI completos sobre cada punta empujada; suite Core local al cierre de gate y en el Candidato | Si: OWN-C | **Si**: Core Windows vs Ubuntu; pushes agrupados | AGENTS «Pruebas» |
| Suites Full | Candidato: Core + UI Full local + builds + CI exacto; cierre y gates que exigen Full (AGENTS punto 1; guia §7.1) `[KEEP]` | Sin cambio en Candidato ni cierre. Un cierre de gate V2 no es «gate que exige Full». H3 fuerte (Core local solo en el Candidato) **no se adopta** (P-11) | EA Q7, Q14 (valor contrafactual UNKNOWN; AGENTS excluye el Core de LC-UI por clase) | Validacion Full del Candidato intacta (EA H.1-9) | Si: OWN-C (confirmar la no adopcion) | Si | AGENTS «Pruebas» |
| Declaracion del Candidato | Candidato = SHA exacto entregado para validar o integrar; rebase final antes (WORKFLOW 4.5.1–4.5.2; AGENTS; guia §7.1) `[KEEP]` | Umbral READY-01..READY-09 antes de fijar `FINAL_CANDIDATE_SHA`; toda entrega al Owner es un Candidato con evidencia V1 completa; commits parciales no se entregan ni se llaman Candidato (P-12) `[NEW]` | EA Q9 (`acecde6` declarado con CI rojo); EA B-33 (base superada); EA Q8 | Evidencia V1 del Candidato sin reduccion; todo SHA nuevo invalida | Si: OWN-D | Si: orden entre conformidad y evidencia | INITIATIVE_LIFECYCLE (READY); guia §7.1 (bloque) |
| Cobertura | Sin cobertura en push ordinario; con cobertura en push a `main` y dispatch del Candidato (WORKFLOW 4.5.2.bis, 4.5.6, 4.5.7; `ci.yml`) `[KEEP]` | Sin cambio. Aclaracion: nunca se despacha cobertura sobre `MERGE_SHA`. El doble dispatch del Candidato queda en U-03 (P-13) | EA C-06 (orden erronea), EA C-07 | Ninguna retirada de cobertura | Si: OWN-I (confirmar **sin cambio**) | Bajo | WORKFLOW 4.5.7 (aclaracion) |
| Owner Validation | Disparador = cambia dibujo; metadata monotonica; formato de evidencia; reutilizacion por mismo SHA, proposito y alcance (AGENTS punto 5 y «Reutilizacion»; AUTOMATION_PLAN §11; guia §6–§8) `[KEEP]` | Matriz OV-nn en el Freeze, **aditiva** al checklist de la guia; solo se amplia sin Owner; retirar escenarios exige Owner; comprobacion del DLL entregado; separacion POLICY DECISION / PRODUCT VALIDATION (P-14) `[NEW]` | EA B-13 (DLL anterior al Candidato); EA B-38; EA 0.5 (veredictos transcritos fuera de sesion) | No elimina ni reduce (EA H.1-8; EA H.3-6 no reabierta) | Si: OWN-M | Bajo | guia de validacion §7 |
| Cadencia documental | HANDOFF solo al integrar; ROADMAP en 3 momentos; guias «en la misma rama»; ideas-futuras «al detectarlo»; ADR antes de implementar; conteos y hashes solo en HANDOFF §12 (WORKFLOW §2, §8; AGENTS; README de iniciativas) | Documentos locales durante el trabajo; guias y README en el ultimo gate; indices, ideas-futuras, registro y HANDOFF en el commit de cierre; SHAs y conteos en la «Evidencia final» del contrato; sin marcadores PENDING (P-15) `[CHANGE]` | EA §5.3 (HANDOFF +43%); EA Q12; EA C-25; EA B-36 (§8-12 inexistente); EA Q10; EA TABLE A (I-48 «MERGE_SHA = PENDING»); EA 0.5 (I-51) | HANDOFF sigue solo al integrar; el archivo ADR sigue antes de implementar; documentacion visible antes del Candidato (AGENTS punto 4) | Si: OWN-K | Si: durabilidad de hechos posteriores al merge (U-11) | WORKFLOW §8 |
| Prompts del Coordinator | Sin norma; ordenes re-enuncian en parafrasis | REFERENCE OVER REPETITION con referencia precisa y completa; campos obligatorios; STOP ante contradiccion; plantillas A, B y F (P-16, P-18) `[NEW]` | EA §5.4; EA O-1..O-10; EA hechos C.6 (incluida la contraevidencia de la celda ROADMAP) | Una orden no redefine autoridad superior; nombra las reglas que el gate ejerce; condiciones de parada especificas obligatorias | Si: OWN-F | **Si**: ordenes cortas que omiten paradas | PROMPT_TEMPLATES |
| Prompts del Executor | Sin norma | Plantilla B (≈15–40 lineas como guia de la orden de G2, no limite) (P-18) `[NEW]` | EA §5.4 (60–85% especifico; 9–20% politica re-enunciada) | Correccion antes que brevedad; paradas obligatorias listadas en la plantilla | Si: OWN-F | Si | PROMPT_TEMPLATES |
| Prompts del Architect | Sin norma; «aprobar» | Plantilla C con salida estructurada y estado de consenso sin «AGREED WITH CHANGES» ambiguo (P-08, P-18) `[NEW]` | EA Q5 (control I-52: 8 de 8 «AGREED WITH Vn» seguidos de CHANGES REQUIRED) | AGREED solo sin cambios requeridos abiertos | Si: OWN-B | Si | PROMPT_TEMPLATES |
| Conformidad | Practica (I-45 CR1–CR3) | Resultado implementado vs contrato congelado; **Architect + Coordinator en todos los arquetipos**; CONFORMING / NON-CONFORMING con desviaciones clasificadas; completa sobre cada SHA nuevo; no es ronda de rediseño (P-19) `[NEW]` | EA B-26 (4 HIGH tras «NONE» del Coordinator); EA B-27; EA B-30 (invariante sin prueba); EA B-31 | Revisor independiente del arquetipo; desviacion material reabre Architect; reservada, Owner | Si: OWN-B | Si | INITIATIVE_LIFECYCLE |
| Integracion / post-merge | WORKFLOW 4.5.1–4.5.7 y §4 paso 6; nunca commit directo en `main`; sin merge automatico `[KEEP]` | Secuencia V1 intacta + cierre concentrado + informe final `[NEW]`; re-fetch antes del merge y vuelta a 4.5.1 si `main` avanzo, sin excepciones propuestas `[CHANGE]` (precision; P-20) | EA B-32/O-1 (orden contra 4.5.7); EA O-2 (merge sin rebase citado como precedente) | Exact-SHA, CI post-merge y limpieza tras ambas compuertas sin cambio (EA H.1-10..12) | Si: OWN-H | Bajo | WORKFLOW §4.5 |
| Autoridad y precedencia | WORKFLOW §10 y AUTOMATION_PLAN §2 con ordenes distintos; ordenes de gate sin lugar | Autoridad por dominio con reglas de conflicto; la orden de gate solo estrecha; camino de una decision del Owner a norma durable (P-17) `[CHANGE]` | EA O-1, O-2, O-3; EA hechos B.10 | STOP + citar ambas fuentes; la mas estricta en garantias de seguridad | Si: OWN-L | Si | WORKFLOW §10 (AUTOMATION_PLAN §2 remite) |
| Scripts futuros | No existen (salvo `eng/validation` de I-45) | Solo diseño de 4 scripts; implementacion en otra iniciativa (P-21) `[NEW]` (diseño) | EA C-24; EA hechos A.20; EA B-13 | Fail-closed; UNKNOWN = FALLO en comprobaciones requeridas; decisiones humanas explicitas | Global (H.1-3); su implementacion necesitaria iniciativa propia | Medio | Iniciativa futura |
| Metricas | Solo duracion activa experimental del Owner (guia §8) `[KEEP]` | Fila ligera por iniciativa + evaluacion de la V2 tras varias iniciativas (P-22) `[NEW]` | EA hechos D.1 (tiempo activo UNKNOWN); EA Q1 | Dato ausente = UNKNOWN, nunca fallo; nunca KPI de menos pruebas | Global (H.1-3); la conclusion de la evaluacion va al Owner | Bajo | INITIATIVE_LIFECYCLE |
| Transicion / vigencia | Contrato I-56 §0.1, §0.2 `[BIND]` | Merge normativo unico, pausa de reclamos hasta su verificacion, clasificacion en el reclamo por ascendencia de la base (P-25) `[NEW]` | Contrato I-56 §0 | Nada entra en vigor antes del merge normativo aprobado y verificado | Si: OWN-E | Si | WORKFLOW (seccion de transicion) |

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

Decision:

1. **AGRUPAR** en una iniciativa conceptual cuando comparten **autoridad** o una **fundacion reutilizable** que se diseñaria dos
   veces, **y ademas** comparten persistencia o mutacion. Compartir solo objetivo de usuario no basta.
2. **SECUENCIAR, no agrupar**, cuando comparten solo superficie de UI o archivos calientes: eso es conflicto de archivo, no
   dependencia funcional (P-07).
3. **SEPARAR** cuando las autoridades difieren y no hay fundacion comun, o cuando un ID tiene resultado visible por el Owner
   independiente y no necesita el diseño del otro.
4. **UNKNOWN en autoridad o fundacion** no se resuelve agrupando por conveniencia: el Discovery Core de la iniciativa candidata
   lo resuelve antes del Freeze; si agrupa o separa, el contrato registra por que.

Racional de la conjuncion de la regla 1: en I-53 lo que se diseño una vez fue la autoridad de la cabecera configurable **y** su
reconciliacion (mutacion), y eso es lo que E2 y E3 reutilizaron sin rondas propias (EA Q13); cuando dos trabajos solo comparten
ventana o archivos, lo medido fue conflicto documental o de archivo, no diseño compartido (EA Q12).

**Unidades de entrega.** Dentro de una iniciativa conceptual, se definen unidades de entrega cuando las superficies de UI, los
sistemas, los archivos calientes o los limites de rollback difieren. Cada unidad:

- tiene **resultado verificable por si misma** (prueba observable o hito visible por el Owner);
- conserva **toda** la mecanica V1 de una unidad Git: reclamo atomico, rama, worktree, rebase, Candidato, cierre, merge
  `--no-ff`, CI post-merge, cobertura del Candidato y limpieza `[KEEP]`;
- nace con un **contrato delta**: encabezado, porcion de alcance, subconjunto de la matriz OV, archivos calientes y puntos del
  Freeze que ejecuta, **citados por seccion** del contrato conceptual; no re-enuncia el contrato congelado.

**La fundacion se funde con su primer consumidor por defecto.** Una unidad que solo entrega fundacion sin salida verificable (I-53
E1) solo existe si aplica un criterio de division de P-10 (limite de rollback, secuenciacion forzada por archivos calientes, u
otra iniciativa que la necesite integrada antes). Si existe, conserva Candidato Full, merge y CI post-merge; la Owner Validation
se decide por su disparador (cambia comportamiento de dibujo) y por la metadata monotonica, como en V1 — no por costumbre.

**Clasificacion heredada.** Una iniciativa conceptual se clasifica **una vez**, con el reclamo de su primera unidad: su
`workflow` (V1 o V2, P-25) y su arquetipo valen para las unidades posteriores, aunque alguna se reclame despues de
`WORKFLOW_V2_EFFECTIVE_SHA`, **siempre que esas unidades figuren en el contrato o el Freeze conceptual antes de ese SHA**. Una unidad
añadida despues es trabajo nuevo y se clasifica por su propio reclamo. Una unidad posterior nunca aplica V2 sobre un diseño
congelado bajo V1 (contrato I-56 §0.1). El arquetipo solo puede subir por los disparadores de la propia unidad.

**Discovery delta por unidad.** Cada unidad posterior hace, en su propia base, un Discovery Core **delta** de DC-7 (archivos
calientes), DC-8 (fundaciones verificadas contra el codigo actual) y DC-9 (disparadores): la base puede haber cambiado mucho entre
unidades (I-54 integro 57 archivos entre E1 y E3, EA C-12, B-33). El Freeze conceptual sigue siendo autoridad para todas las
unidades listadas hasta que se cierre la ultima, aunque la primera unidad ya este integrada.

**ROADMAP `[KEEP]`.** Una fila por unidad Git (WORKFLOW §2), cada una marcada en su momento. El problema medido de la fila de E1
(«integrada» 11 h antes de que ID6/ID7 fueran visibles, EA Q13) lo resuelve la fusion por defecto, no un cambio de ROADMAP.

**Caso principal: I-53** (EA Q13, EA Q1, EA C-13, EA C-29, EA B-31, EA O-1).

| | Evidencia |
|---|---|
| Lo que la agrupacion ahorro | Un Discovery, dos Proposals, un ADR (ADR-0037) y un registro de decisiones para 2 IDs en 2 sistemas; E2 y E3 sin rondas de diseño ni desviaciones de diseño hacia atras; nucleo compartido sin cambios desde G3; el rebase de E3 no re-valido E2 |
| Ceremonia que introdujo E1/E2/E3 | 3 reclamos, 3 bootstraps, 3 cierres, 3 merges, 7 corridas en `main`, 6 ediciones de ROADMAP, ~490 lineas de HANDOFF; E1 sin salida verificable pero con Candidato completo y smoke en AutoCAD sobre codigo sin llamadores (C-13); fila «integrada» 11 h antes de que ID6/ID7 fueran visibles; la particion tomo 5 formas; desviaciones de proceso concentradas en E1 (O-1); contratos minimos con 18–22% re-enunciado y el error B-31 |
| Como lo haria la V2 | Iniciativa conceptual I-53 con Discovery, Proposal, Freeze y decisiones **una vez**; E1 **fundida** con E2 (primera unidad visible: fundacion + UI Selectivo) salvo que un criterio de division de P-10 lo impida; E3 como segunda unidad; contratos delta sin re-enunciado; filas de ROADMAP segun V1; la ceremonia Git y la evidencia de Candidato se pagan por unidad **sin reduccion**; con E1 fundida desaparecen un reclamo, un bootstrap, un cierre, un merge con su CI post-merge y el smoke sobre codigo sin llamadores |
| Coste aceptado | La primera unidad es mas grande (E1 tardo 17h17 de reloj frente a 5h16 de E2); si otra iniciativa necesitara la fundacion integrada antes, el criterio de division lo permite |

**Comparacion con I-50** (EA Q13, EA Q6). Un ID en 3 sistemas, una iniciativa, gates por sistema, una validacion final. La regla
la conserva como **una** iniciativa (una autoridad, ADR-0035; un modelo persistido). Lo que la V2 cambiaria no es la agrupacion
sino la forma de los gates (P-10): el primer resultado verificable llego en el commit 17 de 20.

**I-55, con prudencia.** Activa y grandfathered: esta propuesta **no se le aplica** (EA H.1-2, H.1-4). Solo se registra que la
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
   disparadores y nunca queda por debajo de lo que su propio diff activa.
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
| EXP-01 | El registro de fundaciones o un ADR contradice el codigo (P-06) |
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
convertida en UNKNOWN explicito con quien lo decide; arquetipo confirmado por el Coordinator.

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
Decision source:    <ADR aceptado> / <iniciativa integrada y seccion de su contrato>
Protecting tests:   <clases de prueba / guardas>
Known limitations:  <deuda conocida; p. ej. comentario RackBlockData.cs:7-11>
Last changed by:    <iniciativa> <fecha>   (sin hashes: WORKFLOW §8)
```

**Reglas:**

1. **Elegibilidad**: integrada en `main`, con ADR aceptado o contrato congelado de una iniciativa integrada, y con punto de
   extension declarado o al menos un consumidor fuera de la iniciativa que la creo.
2. **Quien añade o cambia**: solo la iniciativa que la **introduce** o la **extiende**. El texto de la entrada se **redacta en el
   contrato antes de READY-06**, de modo que la conformidad del Architect (P-19) lo revise contra el codigo; el commit de cierre
   (P-15) lo copia **literalmente** al registro. Un consumidor que solo la **consume** no edita la entrada. Ninguna rama edita el
   registro fuera de su cierre.
3. **Evolucion**: la entrada se actualiza en el cierre de la iniciativa que la cambia; la historia vive en Git y en la fuente de
   decision, no en la entrada. **«En evolucion» no es un estado escrito**: se deriva de Git en el preflight (P-07) — ramas activas
   cuyo contrato declara `Extends:` esa entrada —, asi que nunca queda obsoleto ni obliga a editar un documento compartido a mitad
   de una iniciativa. Los contratos V1 (grandfathered) no tienen ese campo: para ellos el preflight informa **UNKNOWN**, nunca «no
   en evolucion». La poblacion inicial no marca ninguna iniciativa activa.
4. **Verificacion por consumidores**: DC-8 verifica simbolos, pruebas y comportamiento descrito en la base de la sesion y lo
   registra **en su propio Discovery** («verificada en esta sesion: si/no, discrepancias»), no en el registro.
5. **Registro y codigo en desacuerdo**: el codigo manda sobre **lo que es**; el ADR aceptado o el contrato congelado mandan sobre
   **lo que debe ser**. El desacuerdo es EXP-01: se registra en el Discovery; si cae en el alcance se corrige; si no, va a
   ideas-futuras y la entrada se anota en el cierre de esa iniciativa. Nunca se «arregla» la entrada para que coincida con un
   codigo que contradice un ADR aceptado.
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

4. Resultado en el contrato (tabla corta): `Rama | Iniciativa | Interseccion | Tipo | Accion (independiente / coordinar /
   secuenciar / dividir) | Punta observada`.

**Puntos de re-verificacion (no en cada gate):** apertura de sesion (V1); antes de la primera edicion de un archivo de la
interseccion; cierre de gate (**solo deteccion**: si una punta observada cambio y toca la interseccion, se detiene el gate para
reclasificar; no se rebasa por esto); preparacion del Candidato (READY-04); antes del merge (P-20). Si ninguna punta observada
se movio, no se re-mide.

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

**Artefacto.** EXTENSION: seccion «Freeze» del contrato (formato como el contrato vinculante de I-51 en su G2, sin archivos de
Proposal, EA TABLE A; es un ejemplo de formato, no de arquetipo: I-51 seria FOUNDATION EVOLUTION). FOUNDATION EVOLUTION / NEW ARCHITECTURE: una version de Proposal **autocontenida** congelada; el contrato la cita por
ruta y seccion. Reglas de version: cada version abre con «Cambios respecto de Vn-1»; no arrastra bloques de estado, preflights ni
tablas del Owner copiados de la anterior (viven una vez en el contrato o en el registro de decisiones; EA C-18); la version
congelada no depende de versiones previas para leerse. La integridad del archivo congelado se comprueba con Git
(`git log -- <archivo>` sin commits posteriores al Freeze) en la preparacion del Candidato, no en cada gate.

**Enmiendas.** Delta numerado (`A-n`) en el registro de decisiones que lista exactamente que clausulas congeladas cambian.

**Invalidacion del Freeze — quien decide:**

| Clase de cambio | Ejemplos | Decide |
|---|---|---|
| Solo Coordinator | Aclarar un detalle no congelado; re-secuenciar gates sin cambiar resultados; añadir pruebas; mover o renombrar codigo; **añadir** escenarios OV | Coordinator (registro breve) |
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
5. Revision del Coordinator del gate (diff vs Freeze; hallazgos; desviaciones clasificadas).
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
| READY-09 | Integridad del Freeze comprobada con Git (P-09) |

Solo entonces `FINAL_CANDIDATE_SHA := <ese SHA>` y se ensambla **la evidencia V1 completa** sobre el: Core Full local, UI Full
local, build Debug UI, build Debug Plugin, CI verde exacto (AGENTS punto 1; guia §7.1), y Owner Validation donde se active.

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

**Matriz OV en el Freeze.** Antes de implementar, el contrato (o la Proposal congelada) incluye:

```text
OV-id | Escenario | Por que aplica (disparador) | Sistema(s) | Datos (DWG nuevo / legacy) | Resultado esperado | Momento (Candidato intermedio planificado / FINAL)
```

La matriz es **aditiva**: especifica los escenarios de la iniciativa **ademas** del checklist y los criterios de aprobacion de la
guia (§6, §7: round-trip, legacy, persistencia y demas). Nunca los sustituye ni los estrecha. Ejemplos de forma, sin forzarlos en
cada iniciativa: OV-01 camino feliz; OV-02 legacy; OV-03 editar/actualizar; OV-04 guardar/reabrir; OV-05 comportamiento ante fallo.

**Reglas:**

1. **Aplicabilidad por disparador** (V1): cambia comportamiento de dibujo → Owner Validation. «No aplica» se justifica con el
   analisis del diff, no con la metadata; `requires_owner_validation: false` no exime (AUTOMATION_PLAN §11).
2. **Refinamiento**: añadir escenarios en cualquier momento (Coordinator). Retirar o sustituir un escenario: **Owner**.
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

### P-15 — Cadencia documental y cierre concentrado

**Categoria**: `[CHANGE]` WORKFLOW §8 (momento de ideas-futuras; ubicacion de hashes y conteos), AGENTS «Flujo Git multi-agente»
(regla de hashes) y README de iniciativas («los contratos no copian conteos ni hashes»); `[NEW]` momento del indice de ADR y demas
indices; `[KEEP]` HANDOFF solo al integrar, ROADMAP en tres momentos, archivo de ADR antes de implementar, guias antes de integrar.

**Evidencia**: HANDOFF +43% en 5 dias por anexion (EA §5.3); todos los conflictos de rebase/merge fueron documentales en HANDOFF,
ROADMAP, ideas-futuras, guia de validacion e indice de ADR (EA Q12); evidencia del Candidato copiada 3–7 veces (EA C-25);
referencias a un «HANDOFF §8-12» inexistente (EA B-36); tension entre «hashes solo en HANDOFF §12» y contratos y filas que los
contienen (EA Q10); `MERGE_SHA = PENDING` nunca actualizado en I-48 (EA TABLE A) e I-51 (EA 0.5; EA hechos A.20).

**Durante diseño e implementacion**, solo documentos locales de la iniciativa:

- contrato (con Freeze en EXTENSION);
- Discovery;
- Proposal vigente o plan;
- registro de decisiones `docs/automation/decisions/<I>.md` **solo cuando hay decision material** (Owner, enmienda A-n, resultado
  de revision de Architect).

Excepciones que siguen su momento V1: el **archivo** de un ADR nace antes de implementar la decision (WORKFLOW §8); la propia fila
de ROADMAP en sus tres momentos (WORKFLOW §2).

**Guias y README** cuando cambia comportamiento visible: en el **ultimo gate de implementacion**, antes del Candidato, para que
la documentacion forme parte de lo terminado (AGENTS punto 4; WORKFLOW §8 «en la misma rama, antes de integrar») `[KEEP]`.

**Cierre concentrado.** El commit documental de cierre (WORKFLOW 4.5.4) lleva, de una vez, las demas ediciones compartidas:
bloque de HANDOFF; marca de ROADMAP; traspaso de hallazgos fuera de alcance a `ideas-futuras.md` (hasta entonces viven en una
lista del Discovery o del contrato) `[CHANGE]`; filas del indice `adr/README.md` (hoy practica sin momento escrito) `[NEW]`;
entradas del registro de fundaciones (P-06); linea del indice `initiatives/README.md`.

**Mapa de autoridad por tipo de informacion** (una sola fuente durable; los demas enlazan):

| Informacion | Autoridad unica | Los demas |
|---|---|---|
| Alcance, no-objetivos, Freeze | Contrato (EXTENSION) o Proposal congelada citada por el contrato | Ordenes y HANDOFF enlazan |
| Evidencia de codigo «como es» | Discovery | Proposal cita DC-n |
| Alternativas y racional de diseño | Proposal (historica tras el Freeze) | — |
| Decisiones del Owner, enmiendas, resultado de revisiones | Registro de decisiones | Contrato enlaza |
| SHAs, corridas y conteos de la iniciativa (Candidato, cierre, OV) | **Seccion «Evidencia final» del contrato** (TEMPLATE §14) `[CHANGE]` | HANDOFF enlaza sin copiar; ROADMAP y documentos normativos nunca |
| Hechos posteriores al merge (`MERGE_SHA`, CI post-merge, cobertura del Candidato) | **Sin resolver** (U-11): hoy solo Git (durable), GitHub Actions (retencion limitada) y el informe final (plantilla E, fuera del repo) | Ningun documento escribe marcadores PENDING |
| Estado vivo del proyecto | HANDOFF | — |
| Plan y registro de cierre | ROADMAP | — |
| Fundaciones reutilizables | Registro de fundaciones | ARCHITECTURE enlaza |

**Resolucion de la duplicacion actual** sin borrar la unica autoridad durable: la Proposal deja de re-enunciar estado (P-09); las
salidas de Architect no se re-transcriben en la orden siguiente, se citan por ID de hallazgo; el contrato de una unidad de entrega
es delta (P-02); el bloque de HANDOFF es corto y enlaza a la «Evidencia final» del contrato; las referencias a «HANDOFF §8-12» se
corrigen a la seccion real en la integracion normativa (U-13). **No se reescriben registros historicos** (AGENTS «Registro historico, no
precedente»); la poda de los bloques historicos actuales de HANDOFF queda en U-14.

**Cambio de la regla de hashes** `[CHANGE]`: AGENTS («No copiar conteos de tests ni hashes de commit fuera de `docs/HANDOFF.md`
(seccion 12)») y WORKFLOW §8 pasarian a: hashes, corridas y conteos de una iniciativa viven en los cuerpos de commit y en la
«Evidencia final» de su contrato; HANDOFF enlaza; ROADMAP, indices y documentos normativos no los contienen. Racional: la regla V1
apunta a una seccion que no existe (EA B-36) y la practica ya los ponia en contratos (EA Q10); un unico lugar por iniciativa evita
las copias que divergen, que es el objetivo original de la regla.

**Limite declarado.** Los hechos posteriores al merge no pueden escribirse en el commit de cierre (aun no existen) ni en `main`
directamente (WORKFLOW 4.5.6). Derivarlos de GitHub Actions no es durable: las corridas y artefactos tienen retencion limitada, y
EA 0.5 registra como defecto que el `MERGE_SHA` y la corrida post-merge de I-51 no constaran en el repo. Esta propuesta **no**
declara resuelto ese registro; opciones para U-11: registrarlos en el siguiente commit de cierre que toque HANDOFF, notas de Git
sobre el merge, o un tag anotado (las dos ultimas son politica Git, OWN-H).

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
7. Un hecho que contradice una afirmacion documental: manda el hecho; la afirmacion se corrige en el siguiente cierre que la toque.

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
IDs funcionales: ...            Agrupacion provisional (P-02): 7 dimensiones (sin leer codigo) + decision + por que
Arquetipo provisional (P-03): ...  Disparadores M-01..M-08: activado / no / UNKNOWN (UNKNOWN = activado)
Preflight de paralelas (P-07): tabla con puntas observadas
Autorizacion: fila ROADMAP | autorizacion del Owner (WORKFLOW §2 caso d)
Reglas que ejerce: reclamo WORKFLOW §4.1; bootstrap y fila WORKFLOW §2 (sin estado en curso); preflight de sesion <ref>
Pedido: contrato desde TEMPLATE con workflow V1/V2 (P-25), Consumes/Extends/Introduces; luego DISCOVERY CORE (P-05 DC-1..DC-9)
No-touch: ramas activas <lista>; archivos de la interseccion <lista>
STOP si: rama remota ya existe; interseccion funcional no listada; un disparador pasa a activado o UNKNOWN respecto del
         provisional; hace falta una expansion EXP (pedir autorizacion); contradiccion entre fuentes -> citar ambas
Informe: BASE_SHA, CLAIM_SHA, archivos, tabla DC, disparadores, arquetipo propuesto, expansiones pedidas
```

**B. Executor Gate Prompt Template** (≈15–40 lineas como guia)

```text
I-NN — G<n> <nombre>                      Arquetipo: ...   Freeze: <ruta §>
Objetivo: <resultado verificable del gate>
Alcance: <puntos del Freeze F-x..F-y>     Fuera de alcance de este gate: ...
Invariantes tocados: <IDs del Freeze>
Rutas / calientes: ...                    Puntas observadas: <rama=punta>
Reglas que ejerce: preflight de sesion <ref>; rebase al abrir WORKFLOW §4.2; RED->GREEN AGENTS punto 2;
                   0 seleccionadas = FALLO (AGENTS); suite Core local al cierre (P-11); CI leido (P-10)
Evidencia requerida: SHA de cierre; pruebas con conteo; suite Core local; CI push del SHA de cierre (event, head_sha, jobs)
No-touch: ...
STOP si:
  - el CI del gate anterior no esta verde y leido;
  - una punta observada se movio y toca la interseccion; o origin/main avanzo sobre un archivo caliente del gate;
  - se activa un disparador M-01..M-08 o una desviacion MATERIAL / OWNER-RESERVED;
  - hace falta compilar el Plugin con AutoCAD abierto;
  - un CI rojo: leer logs/TRX/volcados antes de proponer correccion;
  - <condiciones especificas del gate>; contradiccion entre fuentes -> citar ambas
Informe: SHA(s), pruebas con conteos, CI run, desviaciones clasificadas (P-19), hallazgos fuera de alcance
```

**C. Architect Review Template**

```text
I-NN — ARCHITECT REVIEW <ronda>           Review mode: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN
Entrada: <Proposal vN, ruta>; delta desde la ronda anterior: <cambios requeridos + diff>
Alcance: <completo (primera ronda) | delta + interacciones demostradas>
Evaluar contra: materialidad P-04, elementos congelables P-09, fundaciones consumidas (DC-8)
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
Contra: Freeze <ruta> + enmiendas A-n
Traza: invariante congelado -> ubicacion en codigo -> prueba/guarda (sin prueba = desviacion)
No-objetivos respetados (diff) | Puntos de extension conformes | Matriz OV lista
RESULT: CONFORMING | NON-CONFORMING
Deviations: id | descripcion | clase (EDITORIAL | NON-MATERIAL | BEHAVIORAL-WITHIN-FREEZE | MATERIAL | OWNER-RESERVED) | decide
Seguimientos (no requeridos): mejoras de diseño -> ideas-futuras
```

**E. Final Handoff Template**

```text
I-NN (unidad Ex) — FINAL HANDOFF
FINAL_CANDIDATE_SHA: ...  evidencia: <contrato «Evidencia final»>
CLOSURE_SHA: ...  CI: <run>           MERGE_SHA: ...  CI post-merge: <run, jobs>  cobertura main: <artifact>
Cobertura del Candidato (4.5.7): <run, measured-sha>
Owner: POLICY DECISIONS <refs>  |  PRODUCT VALIDATION <OV ids, SHA, DLL SHA-256>
Limpieza: rama local/remota, worktree — solo tras 4.5.6 y 4.5.7
Registro de fundaciones: entradas añadidas/cambiadas
Desviaciones de proceso: ...          Metricas (P-22): fila
```

**F. Candidate & Integration Prompt Template** (añadida: los incidentes O-1, O-2 y O-3 ocurrieron en ordenes de Candidato e
integracion, EA §5.4)

```text
I-NN (unidad Ex) — CANDIDATE READINESS + INTEGRATION
Reglas que ejerce: READY-01..09 (P-12); Candidato AGENTS «Pruebas» punto 1 + guia §7.1; WORKFLOW 4.5.1–4.5.7 y §4 paso 6
                   (sin copiar); Owner Validation AGENTS punto 5 + matriz OV <ruta>; cierre concentrado (P-15)
Especifico de esta unidad: escenarios OV <ids>; documentos compartidos del cierre <lista>; puntas observadas <rama=punta>
STOP si:
  - cualquier READY no se cumple o queda UNKNOWN;
  - origin/main avanzo antes del merge (volver a 4.5.1; sin excepciones);
  - arbol sucio al producir evidencia local; AutoCAD abierto antes de compilar el Plugin;
  - el DLL a entregar no esta construido sobre el Candidato (InformationalVersion y SHA-256, P-14 regla 5);
  - la orden parece contradecir WORKFLOW 4.5 o AGENTS (p. ej. cobertura sobre MERGE_SHA, limpieza antes de 4.5.7) -> citar ambas;
  - el CI de MERGE_SHA no esta verde o falta el artefacto: no limpiar; corregir en la rama
Informe: plantilla E
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

`NON-CONFORMING` impide READY-06; se corrige (SHA nuevo) o se acepta la desviacion por quien corresponde con registro.

### P-20 — Integracion y post-merge

**Categoria**: `[KEEP]` WORKFLOW 4.5.1–4.5.7 y §4 paso 6, nunca commit directo en `main`, sin merge automatico (EA H.1-12);
`[NEW]` registro de hechos posteriores; `[CHANGE]` **precision** de politica Git: re-fetch antes del merge y vuelta al rebase si
`main` avanzo (hoy WORKFLOW no dice nada del intervalo entre el Candidato y el merge). OWN-H: el Owner confirma esta precision; no
cambia ninguna otra mecanica Git.

**Analisis de la secuencia V1 y cambios explicitos:**

| Paso V1 | V2 |
|---|---|
| 4.5.1 Rebase final + `--force-with-lease` | Sin cambio; ocurre dentro de READY-04 |
| 4.5.2 CI verde sobre el tip rebasado + builds + Candidato Full | Sin cambio; precedido por READY-05..09 (P-12) |
| 4.5.2.bis Cobertura del Candidato (opcional) | Sin cambio (P-13) |
| 4.5.3 Owner Validation sobre el SHA rebasado | Sin cambio; ejecuta la matriz OV (P-14) |
| 4.5.4 Commit documental de cierre | Sin cambio en guardia y evidencia; contenido concentrado (P-15) |
| (nuevo) antes de 4.5.5 | `git fetch`: si `origin/main` avanzo desde la base del Candidato, volver a 4.5.1 (rebase, Candidato nuevo). La propuesta **no** contiene ninguna via de merge sin rebase, ni guardia de patch-id o de igualdad de arbol |
| 4.5.5 `merge --no-ff` | Sin cambio; manual |
| 4.5.6 CI sobre `MERGE_SHA` + artefacto de cobertura | Sin cambio; nunca se sustituye por el CI del cierre aunque los arboles coincidan (EA C-05) |
| 4.5.7 Cobertura del Candidato en `main` | Sin cambio (P-13) |
| §4 paso 6 Limpieza tras 4.5.6 y 4.5.7 | Sin cambio |
| Registro posterior | Informe final (plantilla E); ningun documento escribe PENDING; el registro durable en el repo queda sin resolver (P-15, U-11) |

### P-21 — Scripts futuros (solo diseño; no se implementan en I-56)

**Categoria**: `[NEW]` (diseño). `eng/**` no se toca en I-56 (EA H.1-1). Se implementarian en una iniciativa propia, con reclamo y
Candidato. Regla comun: **fail-closed**; una comprobacion requerida que no puede determinarse devuelve UNKNOWN y sale con codigo de
fallo; una seleccion que no selecciona nada es FALLO (AGENTS).

| Script | Entradas | Salidas | Semantica de fallo | Queda como decision humana |
|---|---|---|---|---|
| `initiative-preflight.ps1` | Worktree; rama; lista de areas probables; tabla WORKFLOW §7 | Estado Git (arbol, stash, operaciones en curso, divergencia con `origin/main`); ramas activas con rutas `merge-base..punta`; intersecciones clasificadas como candidato funcional / archivo / documento compartido de cierre; entradas de FOUNDATIONS extendidas por contratos de ramas activas; puntas observadas | Error de fetch, arbol sucio, operacion en curso o worktree ocupado = FALLO; clasificacion funcional no determinable = UNKNOWN visible (nunca «independiente») | Si una interseccion es dependencia funcional; secuenciar o dividir |
| `candidate-check.ps1` | SHA propuesto; ruta del Freeze; ruta del contrato | READY-04/05/07/09 comprobables mecanicamente: base al dia, CI push del SHA (event, head_sha, jobs), arbol limpio, archivo congelado sin commits posteriores; plantilla del bloque de Candidato de la guia §7.1 | Cualquier condicion no verde o UNKNOWN = FALLO; nunca rellena lineas por analogia | READY-01/02/03/06/08 (alcance completo, conformidad, OV); declarar el Candidato |
| `post-merge-check.ps1` | `MERGE_SHA`; SHA del Candidato | Corrida de CI de `MERGE_SHA` con los cuatro jobs; artefacto de cobertura; dispatch del Candidato con `measured-sha` == Candidato; veredicto de si la limpieza procede | Corrida ausente, job no `success`, artefacto ausente o `measured-sha` distinto = FALLO; no borra nada | Ejecutar la limpieza; que hacer ante un rojo (siempre en la rama) |
| `evidence-report.ps1` | Iniciativa; rango de commits | Tabla de corridas por SHA y clase; Full afirmados en cuerpos de commit vs corridas medidas; fila de metricas P-22 con UNKNOWN explicitos; entradas del registro de fundaciones con simbolos o pruebas citados inexistentes | Datos no recuperables = UNKNOWN (nunca 0); no clasifica valor | Interpretar metricas; marcar entradas obsoletas |

### P-22 — Metricas y evaluacion

**Categoria**: `[NEW]`. Registro ligero en la «Evidencia final» del contrato (P-15), una fila por iniciativa o unidad:

```text
Archetype (inicial -> final; reclasificaciones y disparador) | Discovery rounds (Core + expansiones EXP) |
Architect rounds (y Review mode) | Implementation gates (total / verificables) | Full-suite runs (Core local, UI local; por punto:
cierre de gate / Candidato) | Candidate attempts (invalidaciones y causa) | CI attempts (corridas de rama; rojas; rojas no leidas
antes del siguiente gate) | Owner rounds (POLICY DECISIONS / PRODUCT VALIDATIONS; hallazgos) | Process deviations (por clase P-19)
| Freeze invalidations (por clase P-09) | Escaped findings (hallados despues del merge, p. ej. B-30)
```

Reloj entre eventos solo cuando es MEASURED desde Git o Actions; duracion activa solo declarada (guia §8). **Un dato ausente es
UNKNOWN, nunca un fallo** ni un bloqueo.

**Evaluacion de la V2.** Cuando existan al menos **cinco iniciativas V2 integradas con al menos una de cada arquetipo** — escala
comparable a las seis iniciativas cerradas de la muestra de G1 y eleccion ajustable por consenso, no derivada de un umbral medido
(U-12) —, una iniciativa documental evalua: hallazgos escapados (post-merge o por iniciativas posteriores); Candidatos invalidados;
invalidaciones del Freeze; reclasificaciones hacia arriba; rondas de Architect vs hallazgos materiales por modo de revision; suites
locales por punto y sus capturas; y los criterios de refutacion de las hipotesis de la auditoria (tabla siguiente). **Guardarrail**:
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
| `docs/WORKFLOW.md` | §4 enlace al ciclo de vida, puntos de re-fetch (P-07) y re-fetch antes del merge (P-20); preflight de sesion escrito (P-16); §5 checklist; §8 cadencia documental y ubicacion de hashes (P-15); §10 modelo de autoridad (P-17); seccion de transicion (P-25); referencias «§8-12» corregidas |
| `AGENTS.md` (fuera de `docs/**`: requiere declararlo y orden, contrato I-56 §12) | «Pruebas» punto 1: cadencia de la suite Core al cierre de gate (P-11); la regla de hashes pasa a remitir a WORKFLOW §8; referencia «§8-12» corregida |
| `docs/AUTOMATION_PLAN.md` | §2 remite al modelo de autoridad de WORKFLOW §10 (ejecutor inactivo; sin cambio de limites) |
| `docs/initiatives/TEMPLATE.md` | Solo campos: `workflow`, arquetipo y disparadores, agrupacion, Consumes/Extends/Introduces, tabla de preflight, salidas DC, seccion Freeze, matriz OV, plan de gates, «Evidencia final» con fila de metricas |
| `docs/initiatives/README.md` | Politica de indice de una linea por contrato (hacia delante) |
| `docs/context-packs/README.md`, `documentation-governance.md` | Enlazar ciclo de vida y registro; los packs no son registro |
| `docs/guias/validacion-manual-autocad.md` §7, §7.1 | Matriz OV aditiva y comprobacion del DLL (P-14); termino `FINAL_CANDIDATE_SHA` enlazando a READY del ciclo de vida |
| `docs/ARCHITECTURE.md` §4 | Enlace a FOUNDATIONS en lugar de duplicar |
| `docs/adr/` | ADR de Workflow V2 si el Owner lo decide (U-06; WORKFLOW §8 «ADR si es decision de fondo») |
| `docs/ROADMAP.md`, `docs/HANDOFF.md` | Solo en el momento de cierre de I-56 |

### P-25 — Transicion y `WORKFLOW_V2_EFFECTIVE_SHA`

**Categoria**: `[NEW]` sobre `[BIND]` contrato I-56 §0.1 y §0.2. **Ningun SHA se asigna en V1.** Todo este punto es materia de
OWN-E.

**Precondiciones, en orden:** `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** version de la Proposal → `Owner =
APPROVED` sobre esa version (decision registrada en `docs/automation/decisions/I-56.md`) → integracion normativa: gates de I-56 que
materializan la politica aprobada en los archivos de P-24 (con orden para `AGENTS.md`, contrato §12), conformidad contra la version
aprobada, y merge por la integracion V1 de I-56.

**Una sola integracion normativa, sin ediciones parciales.** Todas las ediciones normativas (archivos nuevos y cambios en WORKFLOW,
AGENTS, AUTOMATION_PLAN, TEMPLATE, guia y demas de P-24) entran en `main` en **un unico merge** de I-56. I-56 no integra antes de
ese merge ninguna edicion normativa de la V2 (las demas iniciativas siguen editando segun V1, p. ej. la tabla de WORKFLOW §7). Si la materializacion no cupiera en una integracion, **STOP**: la particion vuelve
a consenso y Owner; esta propuesta no define efectividad parcial.

**Que commit es.** `WORKFLOW_V2_EFFECTIVE_SHA` = ese commit de merge `--no-ff` en `main` (derivable de `git log --first-parent
main` y de la rama de I-56; no se escribe dentro de si mismo). Un tag anotado que lo señale seria un puntero de conveniencia creado
tras la verificacion; crear tags es politica Git (U-07, OWN-H).

**Pausa de reclamos hasta la verificacion.** No puede ser una regla de la V2, porque la V2 aun no es efectiva mientras dura. Por
eso se propone como **decision del Owner de alcance local** (P-17), registrada en `docs/automation/decisions/I-56.md` antes del
merge normativo y comunicada por el Coordinator: desde ese merge hasta que su CI post-merge (WORKFLOW 4.5.6) este verde con su
artefacto, no se reclaman iniciativas nuevas. El Owner registra en el mismo archivo, por un commit de I-56 que entra por la
integracion V1, cuando se levanta. Si la verificacion sale roja, se corrige en la rama de I-56 por la integracion V1 (nuevo merge
con su propio CI) y la pausa sigue; si la correccion exigiera cambiar texto normativo aprobado, **STOP**: vuelve a consenso y
Owner. `WORKFLOW_V2_EFFECTIVE_SHA` sigue siendo el merge normativo (el que introdujo la politica); la vigencia empieza al levantarse
la pausa, y ningun documento cita el SHA como efectivo antes. **Un reclamo hecho durante la pausa** (la pausa no se puede forzar
tecnicamente) es **V1**, porque la V2 aun no era efectiva: su contrato lo registra y lee V1 en `WORKFLOW_V2_EFFECTIVE_SHA^1`. Es la
unica excepcion a la regla de ascendencia de abajo, y el Owner la confirma dentro de OWN-E.

**Clasificacion de iniciativas** (`workflow: V1 | V2`, registrado en el contrato en el primer commit posterior al reclamo, P-02):

```text
Sea P = el padre del commit de reclamo ORIGINAL (el del primer push aceptado, identificado por su Claim-Id),
        es decir, la punta de origin/main desde la que se reclamo (WORKFLOW §4.1: fetch y rama desde origin/main).
Si WORKFLOW_V2_EFFECTIVE_SHA es ancestro de P (o igual a P)  ->  V2
En otro caso                                                 ->  V1 (grandfathered)
```

- **Se evalua una sola vez, en el reclamo**, y se registra en el contrato (campo `workflow`) junto a la base, en el primer commit
  posterior al reclamo. El contrato la conserva aunque el commit de reclamo se elimine en un rebase final (WORKFLOW §4.1 lo
  permite). Un rebase posterior da al commit de reclamo un padre nuevo: **no reclasifica**.
- Una iniciativa conceptual se clasifica con su primera unidad, y solo heredan esa clasificacion las unidades **listadas en su
  contrato o Freeze antes de `WORKFLOW_V2_EFFECTIVE_SHA`** (P-02).
- Una iniciativa V1 lee sus normas en `WORKFLOW_V2_EFFECTIVE_SHA^1` (primer padre del merge normativo).
- I-56 sigue siendo V1 tambien despues de su propio merge.

**Interpretacion que el Owner debe confirmar (OWN-E).** El contrato I-56 §0.1 dice «formally claimed before
`WORKFLOW_V2_EFFECTIVE_SHA`». Esta propuesta lo **operacionaliza** por ascendencia de la base del reclamo, no por hora del push. Con
la pausa de reclamos, la unica diferencia entre ambas lecturas es un reclamo preparado sobre una base anterior y empujado despues
de levantarse la pausa: por ascendencia es V1 (leyo normas V1), por hora seria V2. La propuesta recomienda ascendencia por ser derivable de Git y
coincidir con lo que la sesion leyo, pero **no la da por decidida**: si el Owner prefiere la lectura temporal, la regla cambia antes
del Freeze de la V2.

**Sin opt-in retroactivo.** Esta propuesta no permite que una iniciativa grandfathered «adopte» la V2 (I-56 no reescribe contratos
activos, contrato §0.1). Una migracion caso por caso seria decision del Owner fuera de I-56 (U-10).

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

---

## 6. Decisiones reservadas al Owner (paquete futuro; NO se solicitan ahora)

Se presentarian **despues** de `Coordinator = AGREED` y `Architect = AGREED` sobre la misma version, y del dry-run (P-23).

| ID | Decision | Decisiones P | Por que es del Owner |
|---|---|---|---|
| OWN-A | Arquetipos finales y su efecto en Discovery, revision de diseño y Proposal | P-03, P-04, P-05, P-09 | Politica nueva que decide cuanta revision recibe cada iniciativa |
| OWN-B | Regla de participacion del Architect, reglas anti-churn, conformidad y Freeze | P-08, P-09, P-19 | Hace condicional una practica de revision que G1 mostro valiosa |
| OWN-C | Cadencia de la suite Core local: al cierre de gate en lugar de antes de cada push; confirmar la no adopcion de H3 fuerte | P-11 | Cambia AGENTS «Pruebas» punto 1 |
| OWN-D | Umbral de preparacion del Candidato y Candidatos intermedios | P-12 | Nueva compuerta antes de la evidencia |
| OWN-E | Transicion: merge normativo unico, pausa de reclamos, clasificacion por ascendencia (interpretacion de «claimed before») | P-25 | Aplica e interpreta la invariante vinculante de transicion |
| OWN-F | REFERENCE OVER REPETITION y plantillas como norma | P-16, P-18 | Cambia como se ordena el trabajo |
| OWN-G | Practicas retiradas o condicionales: commits `-CLOSE` por gate; revision de diseño propia en unidades bajo un Freeze ya revisado; Discovery completo cuando basta el Core; suite Core local antes de cada push interno | P-05, P-08, P-10, P-11 | Retira o condiciona pasos |
| OWN-H | Politica Git: re-fetch antes del merge y vuelta al rebase; tag del commit efectivo o notas de Git (si se adoptan) | P-15, P-20, P-25 | Precision o ampliacion de politica Git |
| OWN-I | Politica de cobertura: **sin cambio** (confirmacion) | P-13 | Asegura que no hay cambio implicito |
| OWN-J | Agrupacion de IDs, unidades de entrega, fusion de la fundacion con su primer consumidor y clasificacion heredada | P-02 | Cambia como se abren iniciativas |
| OWN-K | Cadencia documental, ubicacion de hashes y conteos, registro de fundaciones y su ubicacion; registro durable de hechos posteriores al merge (U-11) | P-06, P-15 | Cambia WORKFLOW §8, AGENTS y crea una obligacion de verificacion |
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

## 8. Politicas V1 escritas que se proponen cambiar `[CHANGE]`

| # | Regla V1 | Fuente | Cambio | Decision |
|---|---|---|---|---|
| 1 | Suite Core en local en la iteracion ordinaria «sin cambio», es decir, antes de cada push | AGENTS «Pruebas» punto 1 (tabla) | Suite Core local al cierre de cada gate, en el Candidato y en el cierre; no antes de cada push interno | P-11, OWN-C |
| 2 | Hallazgo fuera de alcance → ideas-futuras «al detectarlo» | WORKFLOW §8 | Lista local durante el trabajo; traspaso en el commit de cierre | P-15, OWN-K |
| 3 | Conteos de pruebas y hashes solo en HANDOFF §12; los contratos no copian conteos ni hashes | WORKFLOW §8; AGENTS «Flujo Git multi-agente»; README de iniciativas | Viven en cuerpos de commit y en la «Evidencia final» del contrato; HANDOFF enlaza | P-15, OWN-K |
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
prohibidos por EA H.1 y esta propuesta no los contiene bajo ningun nombre (autocontrol en §11, SC-01..SC-03).

## 10. Preguntas abiertas

| ID | Pregunta | Quien la cierra |
|---|---|---|
| U-01 | ¿`INITIATIVE_LIFECYCLE.md` separado por dominio o fundido en WORKFLOW? | Coordinator + Architect |
| U-02 | ¿`docs/FOUNDATIONS.md` o `docs/architecture/foundations.md`? | Coordinator + Architect (dentro de OWN-K) |
| U-03 | ¿El dispatch de cobertura del Candidato en la rama (4.5.2.bis) y el de `main` (4.5.7) sobre el mismo SHA medido son la misma evidencia, dado que la definicion del workflow puede diferir? | Architect; si cambia politica, Owner (OWN-I) |
| U-04 | ¿Basta la suite Core local al cierre de gate, o la clase Core-Windows exige mas? Se mide con P-22 | Evaluacion de la V2 |
| U-05 | ¿«1-3 sesiones» (WORKFLOW §2) debe sustituirse por los criterios de division de P-10? | Coordinator + Architect; Owner si cambia |
| U-06 | ¿La V2 se registra ademas como ADR (WORKFLOW §8: «ADR si es decision de fondo»)? | Owner |
| U-07 | ¿Tag anotado para el commit efectivo? | Owner (OWN-H) |
| U-08 | ¿NEW ARCHITECTURE debe exigir revision en sesion separada? | Owner, con evidencia de P-22 |
| U-09 | ¿Podria una conformidad acotada al delta de un rebase sustituir a la completa en algun caso? Hoy P-19 exige la completa y cada rebase repite conformidad y evidencia del Candidato (coste sin cota; se mide en P-22 como Candidate attempts) | Architect; Owner si relaja |
| U-10 | ¿Migracion caso por caso de una iniciativa grandfathered a V2? | Owner, fuera de I-56 |
| U-11 | ¿Donde se registran de forma durable `MERGE_SHA`, CI post-merge y cobertura del Candidato (siguiente cierre, notas de Git, tag)? | Coordinator + Architect; Owner si es politica Git (OWN-H, OWN-K) |
| U-12 | ¿Umbral de evaluacion de la V2 (cinco iniciativas, una por arquetipo)? | Coordinator + Architect |
| U-13 | ¿Como se nombra en HANDOFF la seccion real que sustituye a «§8-12»? | Coordinator (editorial) |
| U-14 | ¿Se poda la narrativa historica acumulada en HANDOFF, y con que regla compatible con «registro historico»? | Owner |

## 11. Autocontrol adversarial aplicado antes del commit

Autorrevision del redactor y una verificacion de solo lectura por un subagente con contexto propio lanzado desde esta misma sesion,
contra el contrato, el Evidence Audit, AGENTS, WORKFLOW, AUTOMATION_PLAN, TEMPLATE, adr/README, la guia de validacion, ADR-0033 y
`ci.yml`. **No es la revision del Architect** ni revision independiente: `Architect = REVIEW REQUIRED` sigue pendiente. La
verificacion devolvio 7 hallazgos HIGH, 13 MEDIUM y varios LOW; todos se trataron antes del commit. Una segunda pasada verifico la resolucion y hallo 6 defectos MEDIUM
nuevos introducidos por las correcciones (SC-23..SC-28), tambien corregidos. Tras corregir SC-21, la verificacion comprobo unas 35
citas del Evidence Audit y de V1 (entre ellas I-50 17 de 20, conteos de ceremonia de I-53, HANDOFF +43%, I-52 127/12, 8 de 8,
I-54 ~13, C-06/C-07, B-13, B-30, B-31, O-2, E1 17h17 / E2 5h16, WORKFLOW §10, AUTOMATION_PLAN §2 y la regla de hashes de AGENTS).

| ID | Riesgo comprobado | Resultado y correccion |
|---|---|---|
| SC-01 | T0–T4 o R0–R4 disfrazados | El borrador hacia depender del arquetipo el revisor de la conformidad (READY-06), acercandolo a un nivel. **Corregido**: conformidad por Architect + Coordinator en todos los arquetipos; P-03 argumenta por que no es R0–R4 y fija que ningun elemento protector depende del arquetipo |
| SC-02 | Quick CI o carril «solo documentacion» | `ci.yml` y su poblacion sin cambio. El borrador eximia de suite local a un gate «sin cambio de comportamiento» por autodeclaracion. **Corregido**: solo con la guardia mecanica de rutas de WORKFLOW 4.5.4 |
| SC-03 | Mismo arbol, patch-id o range-diff como evidencia | El borrador acotaba la conformidad tras un rebase al range-diff y P-20 dejaba una via de merge sin rebase. **Corregido**: conformidad completa sobre cada SHA nuevo; ninguna via de merge sin rebase; U-09 queda como pregunta |
| SC-04 | Debilitar la evidencia del Candidato | El borrador creaba un «checkpoint del Owner» sin evidencia de Candidato y omitia «mismo proposito y alcance». **Corregido**: se conserva la definicion V1 (todo SHA entregado para validar es Candidato); Candidatos intermedios con evidencia completa |
| SC-05 | Debilitar la Owner Validation | La matriz OV podia leerse como sustituta del checklist de la guia. **Corregido**: matriz aditiva a §6 y §7 de la guia |
| SC-06 | Cambios de politica Git ocultos | Re-fetch antes del merge declarado `[CHANGE]` (OWN-H); tag y notas de Git solo como opciones de OWN-H; la fila de ROADMAP por iniciativa conceptual se **retiro** (V1 se conserva) |
| SC-07 | Reglas sin evidencia ni racional | Añadidos racionales de la conjuncion de agrupacion, de ADR aceptado sobre convencion de AGENTS y del umbral de evaluacion (declarado como eleccion, U-12); el «≈15–40 lineas» se atribuye a la orden de G2 |
| SC-08 | Arquetipos como niveles de riesgo | Ver SC-01; ademas la clasificacion es por disparadores, UNKNOWN = activado, y solo sube tras el Freeze |
| SC-09 | Registro de fundaciones obsoleto | El borrador tenia un estado `EVOLVING` escrito, marcaba a I-55 (activa) y hacia editar `Last verified` a cada consumidor. **Corregido**: «en evolucion» se deriva de Git; nada marca iniciativas activas; solo editan quienes introducen o extienden; verificacion manual obligatoria en DC-8; `FOUNDATIONS.md` figura como documento compartido de cierre en P-07 |
| SC-10 | Architect opcional donde G1 mostro valor | El borrador dejaba la EXTENSION sin revision del Architect con evidencia de E2/E3, que trabajaron bajo un Freeze ya revisado. **Corregido**: sin revision de diseño solo en unidades bajo un Freeze revisado; la EXTENSION independiente recibe revision del Freeze; conformidad del Architect en todos |
| SC-11 | Bucles de Architect que recreen el churn de I-48/I-52 | Reglas anti-churn; el borrador no clasificaba los MEDIUM y aplicaba errata sin confirmar. **Corregido**: requerido vs opcional por materialidad; errata confirmada por diff; sin limite numerico inventado |
| SC-12 | Gates por clase o archivo | Division no justificada por DTO/helper/resolver/mapper; gates por resultado verificable |
| SC-13 | Congelar implementacion mecanica | P-09 lista lo que no se congela; el punto de convergencia poda contratos sobredetallados |
| SC-14 | Ordenes tan cortas que omiten paradas | El borrador de la plantilla B no tenia paradas por CI previo rojo, puntas movidas, disparadores, AutoCAD abierto ni lectura de logs; no habia plantilla de Candidato/integracion. **Corregido**: paradas añadidas, plantilla F, y la regla de nombrar cada regla documentada que el gate ejerce (contraevidencia de la celda de ROADMAP) |
| SC-15 | Autoridad normativa duplicada | El borrador asignaba P-11, P-12, P-14 y P-25 a dos destinos. **Corregido**: un destino por regla (P-24) y regla de conflicto WORKFLOW vs INITIATIVE_LIFECYCLE (P-17) |
| SC-16 | Cambios a iniciativas grandfathered | Ninguna regla se aplica a I-49, I-52, I-55; ejemplo de I-55 retirado de M-08; clasificacion evaluada una vez en el reclamo y heredada por unidades (P-02, P-25); sin opt-in retroactivo |
| SC-17 | Decisiones del Owner tomadas en silencio | Tabla §6 ampliada; la interpretacion por ascendencia de «claimed before» se presenta como decision del Owner (OWN-E), no como hecho; «Global (H.1-3)» en lugar de «No» |
| SC-18 | Mecanismo de `WORKFLOW_V2_EFFECTIVE_SHA` | El borrador permitia integraciones parciales, clasificaba como V2 reclamos sobre un merge aun no verificado y dependia del commit de reclamo, que puede eliminarse. **Corregido**: merge normativo unico, pausa de reclamos hasta la verificacion, clasificacion registrada en el contrato |
| SC-19 | Intake como trabajo sustantivo antes del reclamo | **Corregido**: el intake no lee codigo; el Discovery Core confirma despues del bootstrap (EA O-9) |
| SC-20 | Durabilidad de hechos posteriores al merge | El borrador los daba por derivables. **Corregido**: declarado sin resolver (retencion de Actions; EA 0.5) en P-15 y U-11 |
| SC-21 | Citas | Corregidas: B-06 fue hallado en la revision del Discovery, no entre gates; C-23 son 16 menciones; solo B-01 es del restamp; EA 0.5 trata veredictos transcritos; «PENDING» de I-48 en TABLE A; «todas» → «todas las unidades cerradas con ronda»; seis iniciativas cerradas de la muestra |
| SC-22 | Markdown | Tablas con columnas consistentes y bloques de codigo equilibrados (comprobado mecanicamente) |
| SC-23 | Entrada del registro sin revision | Se escribia en el cierre, despues de la conformidad. **Corregido**: se redacta en el contrato antes de READY-06 y se copia literal |
| SC-24 | Pausa de reclamos sin base | Era una regla V2 aplicada antes de la vigencia. **Corregido**: decision local del Owner registrada en `decisions/I-56.md`; un reclamo durante la pausa es V1; «ninguna edicion normativa» acotada a I-56 |
| SC-25 | Guardia documental parafraseada | La lista negativa dejaba pasar `.gitattributes` o `global.json`. **Corregido**: lista cerrada de WORKFLOW 4.5.4 citada |
| SC-26 | Regla ADR vs AGENTS | Racional invertido (I-07 escribio ADR desde HANDOFF §7) y dos desenlaces. **Corregido**: excepcion declarada (criterio 4 de adr/README) manda en su alcance; conflicto no declarado = STOP y Owner; ADR aceptados tambien en los dominios de Git y evidencia |
| SC-27 | Herencia de clasificacion | Permitia escapar de V2 añadiendo unidades a una iniciativa V1 y dejaba unidades sin Discovery en su base. **Corregido**: solo heredan unidades listadas antes del SHA efectivo; Discovery Core delta por unidad; Freeze conceptual vigente hasta la ultima unidad |
| SC-28 | «REQUERIDO» demasiado amplio | Cualquier toque a un elemento congelable abria ronda. **Corregido**: solo si cambia su significado; precisiones de redaccion son errata |
| SC-29 | Menores de la segunda pasada | Anti-churn para toda revision de diseño; UNKNOWN para contratos V1 en «en evolucion»; `Last changed by`; excepciones de ROADMAP y WORKFLOW §7 en documentos de cierre; paradas añadidas a la plantilla F; P-10 remite a AGENTS sin re-enunciar la cadencia; coste de repetir conformidad y Candidato por rebase registrado en U-09 |

**Limites de este autocontrol**: fue ejecutado desde la misma sesion que redacto; la adecuacion de la regla de agrupacion, de los
disparadores y de la cadencia del Core frente a casos reales la pondra a prueba el dry-run (P-23) y la revision del Architect.
