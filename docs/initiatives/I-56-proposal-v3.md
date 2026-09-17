# I-56 G4 — Workflow V2 Proposal V3

> ```text
> PROPOSAL V3 — NOT CONSENSUS
> Proposal Version = V3
> Coordinator = REVIEW REQUIRED
> Final independent Architect review = NOT OPEN
> Consensus = NOT REACHED
> Owner = NOT REQUESTED
> Dry-run = NOT OPEN
> Workflow V2 = NOT EFFECTIVE
> WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
> I-56 remains governed by Workflow V1.
> ```

Documento de propuesta, no norma vigente. Los verbos normativos describen lo que regiria **si** se aprobara la version completa
por Coordinator y Architect y despues por el Owner (contrato I-56 §0.2), y se integrara conforme a P-25. I-56 y los reclamos
grandfathered siguen V1. No se cambia ningun contrato, rama ni archivo de I-49, I-52 o I-55. T8 ofrece dos interpretaciones
al Owner: una unidad nueva de esas lineas se detiene hasta OWN-E; esta V3 no elige entre ellas.

El estado de esta cabecera es la fotografia de publicacion. La identidad revisada es **commit + ruta + blob**; no se modifica
este archivo para actualizar un veredicto. Las revisiones/decisiones posteriores identificaran esa tupla en
`docs/automation/decisions/I-56.md` cuando su gate autorice crearlo. No existe consenso por el nombre «V3» ni por una cabecera.

## Cambios respecto de V2 — reconciliacion G4

Base publicada: [Proposal V2](I-56-proposal-v2.md), commit `fde03863f42b805d5503dbd0d0ece3a7dceb4678`,
blob `ab755c38d6c4b14f13a5386cf2a49a5383a14b8f`. Entrada adversarial aceptada:
[Architect Review V2](I-56-architect-review-v2.md), publicada en `f97a8b0bfe5813740ac250c1eb8a75ffaa2ae9a8`.
Se conservan las decisiones y racionales no afectados de V2; este documento contiene la propuesta completa y no requiere
combinar clausulas operativas de versiones anteriores. El Evidence Audit sigue cerrado. La orden G4 requiere RC-01..RC-31;
para RC-17 elige expresamente registro versionado y reinicio, y para RC-19 exige Owner al retirar la ultima asignacion.

**Independencia de I-56.** La revision V2 es evidencia adversarial, no consenso independiente. El autocontrol de esta sesion
es interno. Tras aceptacion del Coordinator, la V3 exacta tendra revision final en **SEPARATE SESSION** o **EXTERNAL HUMAN**,
con solo entradas versionadas. No se abre esa revision, el dry-run ni el paquete del Owner en G4.

### Matriz de reconciliacion obligatoria

«RESUELTO EN V3» significa texto reconciliado pendiente de revision, no acuerdo del Architect ni aprobacion del Owner.

| RC / AR | Resolucion verificable | Destino | Estado |
|---|---|---|---|
| RC-01 / AR-01 | Push aceptado y fetch posterior en un mismo registro ordenado; contraste Actions obligatorio para probar anterioridad; incoherencia → T6 | P-25 | RESUELTO EN V3 |
| RC-02 / AR-02 | PRE/POST remotos; PRE prueba V1; identidad Claim-Id estable; no re-derivar por ascendencia tras rebase | P-25 | RESUELTO EN V3 |
| RC-03 / AR-03 | T8-a / T8-b completas y STOP hasta OWN-E | P-02, P-25, §6 | RESUELTO EN V3 |
| RC-04 / AR-04 | Lectura de ^1 solo para clausulas modificadas por activacion; resto desde main actual | P-25 | RESUELTO EN V3 |
| RC-05 / AR-05 | Pausa y registro de I-56 por autoridad V1 del Owner; inicio antes de 4.5.1, fin durable | P-25, §6 | RESUELTO EN V3 |
| RC-06 / AR-06 | Identificacion determinista del primer merge normativo, independiente de rama borrada y de correcciones | P-25 | RESUELTO EN V3 |
| RC-07 / AR-07 | Version completa + delta; revisor decide interacciones; hallazgo material en cualquier seccion es valido | P-08, plantilla C | RESUELTO EN V3 |
| RC-08 / AR-08 | REQUERIDO incluye incorreccion, imposibilidad, no verificabilidad y oraculo ciego; solo emisor rebaja con razon; poda con Architect | P-08 | RESUELTO EN V3 |
| RC-09 / AR-09 | DC-1..6 actuales o diff vacio demostrado sobre su superficie; DC-7..9 siempre | P-02, P-05 | RESUELTO EN V3 |
| RC-10 / AR-10 | Evaluaciones EXP negativas; Coordinator revisa Discovery y ordena expansion omitida antes de clasificar | P-05, P-08 | RESUELTO EN V3 |
| RC-11 / AR-11 | Clase B confirmada por Coordinator y Architect en todos los arquetipos | P-06 | RESUELTO EN V3 |
| RC-12 / AR-12 | Registro descriptivo debajo de la fuente; M-08 evalua ADR/Freeze, no prosa del registro | P-04, P-06, P-17 | RESUELTO EN V3 |
| RC-13 / AR-13 | DC-5 busca lectores de datos persistidos y artefactos de dibujo, sin limite de saltos | P-05 | RESUELTO EN V3 |
| RC-14 / AR-14 | EXP-09 para materialidad UNKNOWN | P-05 | RESUELTO EN V3 |
| RC-15 / AR-15 | Primera revision recibe Discovery completo y acceso al codigo de su base | P-08, plantilla C | RESUELTO EN V3 |
| RC-16 / AR-16 | Poblacion inicial contrastada por entrada con fuente y codigo; A/B y conformidad | P-06 | RESUELTO EN V3 |
| RC-17 / AR-17 | Toda enmienda, aceptacion o decision posterior a READY-04 se versiona y reinicia READY-02 | P-09, P-12, P-15, P-19 | RESUELTO EN V3 |
| RC-18 / AR-18 | Todos los OV aplicables en FINAL_CANDIDATE_SHA; intermedios adicionales | P-14 | RESUELTO EN V3 |
| RC-19 / AR-19 | Asignacion OV en Freeze/delta/A-n; cobertura completa; ultima desasignacion requiere Owner | P-02, P-14, READY-08 | RESUELTO EN V3 |
| RC-20 / AR-20 | Retirar cierre previo, rebasar producto, empujar Candidato solo, evidencia propia y cierre nuevo | P-12, P-20, plantilla F | RESUELTO EN V3 |
| RC-21 / AR-21 | Identidad acordada registrada; diff de Freeze limitado a lineas de estado enumeradas | P-09, READY-09 | RESUELTO EN V3 |
| RC-22 / AR-22 | Freeze/A-n conceptuales en main para hermanas; Applies-to; numeracion continua append-only | P-09 | RESUELTO EN V3 |
| RC-23 / AR-23 | Trailer unico por unidad/artefacto alcanzable desde Candidato; ruta del trailer; sin squash | P-09, READY-09 | RESUELTO EN V3 |
| RC-24 / AR-24 | A-n Coordinator con M negativos razonados y aviso al Architect antes del siguiente gate | P-09 | RESUELTO EN V3 |
| RC-25 / AR-25 | Evidencia posterior fuera del cuerpo del mismo commit; tip propio y CI leido tras rebase | P-10, P-11, plantilla B | RESUELTO EN V3 |
| RC-26 / AR-26 | MERGE_SHA unico = merge verificado; rondas anteriores en Unverified merges; sin alias adicional | P-12, P-15, P-20, plantilla E | RESUELTO EN V3 |
| RC-27 / AR-27 | event + ref + head_sha en criterios; tag push excluido; plan explicito AGENTS y WORKFLOW | P-20, P-24 | RESUELTO EN V3 |
| RC-28 / AR-28 | Proteccion ausente; accion Owner previa a activacion; validacion manual y correccion completa de tags | P-20, P-25 | RESUELTO EN V3 |
| RC-29 / AR-29 | Contencion comparable o STOP Owner; H.1 no renunciable localmente; decisiones sin alcance son locales | P-17 | RESUELTO EN V3 |
| RC-30 / AR-30 | Un dueño por regla; lifecycle usa punteros Git/evidencia; plantillas subordinadas | P-17, P-24 | RESUELTO EN V3 |
| RC-31 / AR-31 | Aprobacion parcial no activa nada; nueva version, nuevo consenso y nueva solicitud Owner | §6, P-25 | RESUELTO EN V3 |

### LOW: disposicion individual RC-32..RC-40

| RC / AR | Disposicion en G4 | Motivo y pendiente para Architect independiente |
|---|---|---|
| RC-32 / AR-32 | PARCIAL: referencias de plantillas precisadas; resto DIFERIDO LOW | Referencia colgante = STOP y ampliar item 7 de P-16 crean obligaciones; no se adoptan por higiene editorial |
| RC-33 / AR-33 | CORREGIDO | Cabecera como fotografia; consenso posterior por identidad commit/ruta/blob, sin editar la version revisada |
| RC-34 / AR-34 | DIFERIDO LOW | Permitir al consumidor editar Known limitations cambia permisos; se conserva no-edicion y se identifica la entrada afectada en sus hallazgos/ideas-futuras |
| RC-35 / AR-35 | DIFERIDO LOW | Definir umbral de transversalidad o consumidores futuros modifica materialidad; permanece pregunta, no nueva definicion |
| RC-36 / AR-36 | DIFERIDO LOW | Secuencia nueva para push rechazado de main es politica Git; V1 vigente sigue mandando, no se inventa autorizacion para pull/rebase del merge |
| RC-37 / AR-37 | DIFERIDO LOW | Fijar base y condiciones nuevas del rango de guardia documental afecta exenciones de evidencia |
| RC-38 / AR-38 | DIFERIDO LOW | Pedir al Owner ruta/version del DLL cargado añade una obligacion manual; permanece comprobacion del DLL entregado de V2 |
| RC-39 / AR-39 | CORREGIDO | READY-05 nombra los cuatro jobs ya exigidos; bloque §7.1 completo antes de OV |
| RC-40 / AR-40 | DIFERIDO LOW | Fallback de measured-sha a logs cambia el procedimiento de evidencia; sin dato recuperable no se declara validado |

Los LOW diferidos son asuntos opcionales de la revision futura, no decisiones tomadas ni permisos de excepcion. Su severidad
no impide que el Architect los reclasifique si al revisar la version completa satisfacen P-08. No se reabre la auditoria.

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
| [Proposal V2](I-56-proposal-v2.md) + [Architect Review V2](I-56-architect-review-v2.md) | Base publicada y RC-01..RC-40 de esta reconciliacion | no se modifican; V1 conserva la historia anterior |

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
| H.1-1 | Solo documentacion y proceso; sin implementacion de producto, pruebas, CI ni scripts | P-21 solo diseña scripts; P-24 solo recomienda archivos; G4 crea un unico documento. Editar `AGENTS.md` (fuera de `docs/**`) en la integracion normativa exige declararlo y una orden que lo autorice (contrato I-56 §12) |
| H.1-2 | Transicion: I-49, I-52, I-55 y toda iniciativa reclamada antes de `WORKFLOW_V2_EFFECTIVE_SHA` grandfathered | P-25 |
| H.1-3 | Vigencia: Coordinator AGREED + Architect AGREED sobre la misma version + Owner APPROVED | Bloque de estado; P-25; §OWN |
| H.1-4 | No modificar contratos activos de I-49, I-52, I-55 | Ninguna regla se aplica a sus reclamos, ramas y contratos existentes; una unidad nueva de esas lineas espera OWN-E y T8 (P-25) |
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
2. DISCOVERY CORE (+ CONDITIONAL)           (P-05) -> revision explicita del Coordinator de DC/EXP; despues confirma agrupacion y arquetipo
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

Resumen de las decisiones completas de §5, sin autoridad independiente. `[BIND]` = restriccion I-56; `[KEEP]` = V1 conservada;
`[CHANGE]` = regla escrita a cambiar; `[NEW]` = mecanismo/practica nueva. Cada fila remite al dueño de P-24.

| Area | V1 → propuesta / categoria | Evidencia o racional | Salvaguarda / Owner / destino |
|---|---|---|---|
| P-01 ciclo | Practica → secuencia diseño/READY `[NEW]`; Git conservado | EA H.2-13 | No sustituye WORKFLOW; aprobacion global; lifecycle |
| P-02 agrupacion | Sin regla → mismo problema **y** fundacion compartida, unidades Git `[NEW]` | EA Q13, §6 | Evidencia/reclamo propio; T8 pendiente; OWN-J; lifecycle |
| P-03 arquetipos | Sin regla → E/FE/NA `[NEW]` | EA Q4/Q5/Q14 | Nunca reduce CI/Full/OV; OWN-A; lifecycle |
| P-04 materialidad | Practica → M-01..08 `[NEW]` | EA B-01/B-04/B-10/B-40 | UNKNOWN activo, registro no autoridad; OWN-A/B; lifecycle |
| P-05 Discovery | Practica → DC-1..9 + EXP-01..09 y revision Coordinator `[NEW]` | EA B-01..06, C-28 | Base actual y expansiones omitidas; OWN-A/G; lifecycle |
| P-06 fundaciones | Prosa repetida → registro descriptivo verificado `[NEW]` | EA §6, B-35 | A STOP, B ambas confirmaciones; OWN-K/R; reglas lifecycle, metadata FOUNDATIONS |
| P-07 paralelas | Git/rebase conservados + preflights definidos `[NEW]` | EA C-24/B-33/O-4 | Puntas solo evidencia; global; WORKFLOW |
| P-08 Architect | Practica → proporcionalidad con version completa y delta `[NEW]` | EA Q5/B-07/B-15/B-29 | REQUIRED (a)..(d), rebaja emisor; OWN-B; lifecycle |
| P-09 Freeze | Practica → artefacto separado, identidad exacta y A-n `[NEW]` | EA C-23/B-31 | Unicidad/append-only/main para hermanas; OWN-B; lifecycle |
| P-10 gates | Practica por capa → resultado verificable y revision/CI por gate `[NEW]` | EA Q6/B-10/B-12/B-16 | Push propio, evidencia posterior; OWN-G; lifecycle resultados, WORKFLOW orden Git |
| P-11 suites | Core antes de cada push → Core al cierre de gate `[CHANGE]`; Full/LC-UI intactos `[KEEP]` | EA C-08/Q7; valor contrafactual UNKNOWN | Riesgo Core Windows intermedio declarado, sin H3 fuerte; OWN-C; AGENTS |
| P-12 Candidato | Evidencia V1 `[KEEP]` + READY `[NEW]` | EA Q8/Q9/B-33 | Correccion versionada reinicia; Candidato rebasado solo; OWN-D; lifecycle con punteros |
| P-13 cobertura | Sin cambio `[KEEP]` | EA C-06/C-07 | No dispatch sobre MERGE_SHA; OWN-I sin cambio; WORKFLOW |
| P-14 Owner | Disparador/metadata/checklist `[KEEP]` + matriz/asignacion `[NEW]` | EA B-13/B-38/0.5 | Todos OV aplicables en SHA final; OWN-M; guia |
| P-15 documentos | Hashes/momentos `[CHANGE]` + superficies/tag `[NEW]` | EA Q10/Q12/C-25/B-36 | Semantica antes de READY, ejecucion al cierre; OWN-K/H; WORKFLOW §8 |
| P-16 referencias | Practica → referencias precisas y campos `[NEW]` | EA O-1..O-10 y contraevidencia ROADMAP | Contradiccion STOP, no omitir reglas ejercidas; OWN-F; lifecycle |
| P-17 autoridad | Ordenes inconsistentes → dueño por dominio `[CHANGE]` | EA O-1..O-3 | Contencion o STOP Owner; H.1 no renunciable; OWN-L; WORKFLOW §10 |
| P-18 plantillas | Sin norma → procedimientos subordinados `[NEW]` | EA §5.4 | No otra autoridad; OWN-F; PROMPT_TEMPLATES |
| P-19 conformidad | Practica → ambos roles, completa por SHA `[NEW]` | EA B-26..31 | Sin aceptacion invisible; OWN-B; lifecycle |
| P-20 integracion | V1 `[KEEP]` + re-fetch `[CHANGE]` + tags `[NEW]` | EA C-05/O-1/O-2/0.5 | event/ref/head_sha, ruleset previo, tags verificados; OWN-H; WORKFLOW §4.5 |
| P-21 scripts | Solo diseño futuro `[NEW]` | EA C-24/B-13 | Sin implementacion ni decisiones automaticas; aprobacion global; iniciativa futura |
| P-22 metricas | Evaluacion ligera `[NEW]` | EA D.1/Q1 | UNKNOWN no bloquea, no KPI menos pruebas; global; lifecycle |
| P-23 dry-run | Futuro sobre version acordada `[NEW]` | Comprobar preservacion de capturas EA B/C | Si cambia texto, nuevo consenso; global; documento I-56 futuro |
| P-24 archivos | Tres archivos con dominio propio `[NEW]` | Evitar duplicacion/contradicciones | Una autoridad por regla; OWN-K/L/O; plan, no edicion G4 |
| P-25 transicion | Contrato §0 `[BIND]` + evidencia de orden `[NEW]` | AR-01..06 | Claim-Id estable, T6 STOP, T8 Owner; OWN-E/H/P/Q/S; WORKFLOW |
| P-26 no-objetivos | Limites `[BIND]`/`[KEEP]` | EA H.1/H.3 | Sin T0–T4, R0–R4, Quick CI ni auto-merge |

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
- nace con un **contrato delta** (mutable): encabezado, porcion de alcance, enlace a la asignacion OV congelada, archivos calientes y puntos
  del Freeze que ejecuta, **citados por seccion** del archivo de Freeze conceptual; no re-enuncia el Freeze.

**La fundacion se funde con su primer consumidor por defecto.** Una unidad que solo entrega fundacion sin salida verificable (I-53
E1) solo existe si aplica un criterio de division de P-10 (limite de rollback, secuenciacion forzada por archivos calientes, u
otra iniciativa que la necesite integrada antes). Si existe, conserva Candidato Full, merge y CI post-merge; la Owner Validation
se decide por su disparador (cambia comportamiento de dibujo) y por la metadata monotonica, como en V1 — no por costumbre.

**Version de workflow por reclamo propio.** Cada unidad con reclamo atomico, rama y worktree propios se clasifica por su
reclamo segun P-25, con Claim-Id estable; un rebase nunca reclasifica. No hay herencia automatica por figurar en un Freeze
conceptual ni opt-in/opt-out. **Excepcion interpretativa pendiente:** las unidades nuevas de las lineas nombradas I-49/I-52/I-55
se detienen hasta OWN-E y siguen entonces la opcion T8-a o T8-b aprobada. Fuera de esa decision expresa, antes = V1 y despues = V2.
El arquetipo de una unidad V2 parte del Freeze conceptual V2 y solo sube si su delta lo exige; si consume un Freeze V1 sin
arquetipo, se clasifica desde cero por M-01..M-08. Ninguna opcion modifica los archivos ni reclamos existentes de esas lineas.

**Discovery delta por unidad (RC-09).** En su base actual verifica DC-1..DC-6 de su propia superficie. Cada fila tiene uno de
dos resultados: **REVERIFICADA** con evidencia actual, o **SIN CAMBIO DEMOSTRADO** con referencia al Discovery conceptual, su
base y la base actual, inventario de simbolos/rutas/contratos/lectores y diff vacio sobre ese inventario. El nombre de archivo
igual no demuestra estabilidad; una ruta renombrada, contrato modificado o consumidor nuevo exige re-verificacion. Si no se
puede demostrar el inventario completo, se re-verifica. DC-7, DC-8 y DC-9 siempre se hacen contra la base actual; nunca se
heredan ciegamente. El Coordinator revisa todas las filas y EXP segun P-05 antes del diseño.

Una hermana solo consume Freeze conceptual y A-n cuando son alcanzables desde `origin/main` (P-09). La primera unidad autora
puede implementar su propio Freeze versionado en su rama; esa excepcion de autoria no permite consumo entre hermanas desde
ramas sin integrar. El Freeze conceptual sigue siendo la fuente del diseño hasta el cierre de la ultima unidad.

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

**Categoria**: `[NEW]`. **Veredicto de diseño propuesto sobre la hipotesis inicial: ADOPTADA CON MODIFICACIONES (no aprobacion de politica).**

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
| M-08 | **Decision aceptada**: modifica, reinterpreta o contradice un ADR aceptado o la fuente de decision (artefacto de Freeze integrado + A-n) citada por una entrada del registro; una discrepancia solo de la entrada activa EXP-01, no le confiere autoridad | I-54: ADR-0039 acoto el alcance de ADR-0035 (DimensionViews; EA §6) |

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
| DC-2 | Autoridad | Simbolo(s) dueño(s) de cada regla o valor tocado; ADR o artefacto de Freeze que la gobierna; el registro solo enlaza la fuente |
| DC-3 | Persistencia | DTO/Xrecord/store, campos, fallback legacy, preservacion de desconocidos |
| DC-4 | Camino de mutacion | Entrada → estado → persistencia → dibujo, con simbolos |
| DC-5 | Consumidores directamente afectados | Llamadores directos y consumidores entre sistemas; ademas, **todos los lectores de campos, estructuras persistidas y artefactos de dibujo de DC-3**, buscados por claves/simbolos sin limite de saltos; inventario y huecos de busqueda declarados |
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
| EXP-09 | Un disparador de DC-9 sigue UNKNOWN, incluida la distincion crear/modificar: expansion acotada a esa pregunta |

Una ambigüedad de **comportamiento deseado** no es expansion: es decision de alcance y va al Owner.

**Autorizacion.** El Executor la pide, o el Coordinator la ordena aun sin peticion del Executor, con: disparador, pregunta concreta, areas o simbolos acotados y criterio de salida. La
autoriza el **Coordinator**; en FOUNDATION EVOLUTION y NEW ARCHITECTURE el Architect puede exigir una expansion omitida durante su revision o cuestionar la propuesta; el Coordinator formaliza el alcance y no puede cerrar Discovery mientras siga esa pregunta material abierta. Nunca se
autoriza «auditar todo RackCad».

**Revision obligatoria del Discovery antes del diseño (RC-10).** El Executor registra para cada EXP-01..EXP-09 activado/no activado/UNKNOWN, evidencia y motivo de cada negativo (DC-n citado). El Coordinator lee DC-1..DC-9 y esas evaluaciones y pregunta explicitamente **que EXP debio activarse y no se activo**. Puede ordenar una expansion no solicitada, y vuelve a revisar su resultado. Solo despues confirma materialidad, agrupacion y arquetipo. En FE/NA el Architect tambien revisa la suficiencia de las expansiones; en todos los arquetipos conserva su revision de clase B y acceso a Discovery/codigo (P-08). UNKNOWN en DC-9 activa EXP-09 y sigue contando como disparador; no se convierte en «no» para salir.

**Registro del POR QUE.** Seccion «Expansiones» del Discovery:

```text
EXP-id | Disparador | Pregunta | Autorizada por | Area acotada | Resultado | Cerrada: SI / UNKNOWN con dueño de la decision
```

**Criterio de salida del Discovery.** DC-1..DC-9 presentes con clase de evidencia; cada expansion cerrada con respuesta o
convertida en UNKNOWN explicito con quien lo decide; revision de DC/EXP y arquetipo confirmados por el Coordinator en ese orden; y **ninguna discrepancia EXP-01 de
clase A abierta**. Una discrepancia A no se convierte en UNKNOWN para salir: se resuelve antes (P-06 regla 5). Una UNKNOWN sobre si
una discrepancia es A o B cuenta como **A** (fail-closed).

**Salvaguarda.** El registro de fundaciones **no** exime de DC-8: consumir una fundacion exige verificarla contra el codigo en la
base actual. La evidencia de la muestra lo justifica: la prosa de Header Mutation ya era erronea y un registro construido con ella
la habria propagado (EA §6).

### P-06 — Registro de fundaciones (diseño; NO se crea en G4)

**Categoria**: `[NEW]`. **Evidencia**: EA §6, EA Q11 (Rack Identity y View Identity re-descritas en 6 de 9 lineas; restamp 5;
Project Variables 5), EA B-35 (el mismo comentario erroneo registrado 5 veces), EA hechos A.18 (Context Packs sin cambios desde
julio y sin uso como registro).

**Ubicacion recomendada del registro descriptivo: `docs/FOUNDATIONS.md`.** Verificado en el arbol: `docs/architecture/` **no existe**; los
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
2. **Quien añade o cambia**: solo la iniciativa que la **introduce** o la **extiende**. El texto de la entrada se **redacta y versiona en el
   contrato (superficie mutable, P-15) antes de READY-04**, de modo que la conformidad del Architect (P-19) lo revise contra el codigo; el commit de cierre
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
     cierre, identificando en ese hallazgo la entrada afectada si existe, sin editarla por ser solo consumidor (RC-34 diferido). La clasificacion B **no se autodeclara**: la confirman el Coordinator **y el Architect en todos los arquetipos**, con razon y evidencia; hasta ambas confirmaciones cuenta como A. Si la salida de Discovery depende de B, la confirmacion es anterior a esa salida, no se posterga a conformidad.
   - «Fuera de alcance para arreglar» **no** equivale a «seguro ignorar». Una clasificacion dudosa cuenta como A.
   - Nunca se «arregla» la entrada para que coincida con un codigo que contradice un ADR aceptado **o un artefacto de Freeze integrado**; se resuelve la contradiccion con la fuente.
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

**Poblacion inicial, verificacion por entrada (RC-16).** Para cada candidata se registra fuente aceptada/integrada y clausulas, simbolos y pruebas actuales, concordancias y discrepancias. Se aplica completa la regla 5: A consumida = STOP; duda = A; B solo con demostracion y Coordinator + Architect. Una diferencia con Freeze V1 no se legitima copiando codigo. Cada entrada recibe conformidad de ambos contra fuente **y** codigo en la integracion normativa; el Owner considera el contenido inicial en OWN-R. Las entradas sin fuente elegible no se publican como STABLE, y ninguna limitacion pendiente autoriza consumir un invariante contradictorio. FOUNDATIONS es metadata descriptiva: su texto nunca prevalece sobre la fuente que resume.

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
   consulta, van al **cuerpo del commit** de la sesion o gate y al **informe de gate/sesion**. Las que son evidencia de un SHA exacto relevante — base del Candidato (READY-04), identidad de reclamo o snapshots de transicion (P-25) — pasan al archivo de evidencia (P-15).

**Puntos de re-verificacion (no en cada gate):** apertura de sesion (V1); antes de la primera edicion de un archivo de la
interseccion; cierre de gate (**solo deteccion**: si una punta cambio respecto de la registrada en el ultimo commit o informe y
toca la interseccion, se detiene el gate para reclasificar; no se rebasa por esto); preparacion del Candidato (READY-04); antes del
merge (P-20). Si ninguna punta se movio, no se re-mide.

**No cambia la politica Git**: no añade rebases fuera de los de V1 (apertura de sesion e integracion).

### P-08 — Participacion proporcional del Architect

**Categoria**: `[NEW]`, practica de EA H.2-13. Se conserva el modelo de V2: la muestra no justifica quitar la revision de
diseño de una EXTENSION independiente (I-51 B-29); E2/E3 consumian un Freeze ya revisado. Las rondas tardias de I-48 aportaron
hallazgos materiales (EA Q14, C-14); el delta no puede blindar secciones intactas. La revision del Coordinator de Discovery
capturo B-06 y su revision entre gates B-10/B-11/B-40; son dos controles distintos, ambos obligatorios. La conformidad solo del
Coordinator fallo en I-45 (B-26), de ahi Architect + Coordinator en todos los arquetipos.

| Momento | Unidad bajo Freeze ya revisado | EXTENSION independiente | FOUNDATION EVOLUTION | NEW ARCHITECTURE |
|---|---|---|---|---|
| Diseño antes de Freeze | Sin nueva ronda si su delta no activa disparadores; compatibilidad V1 segun P-02 | Revision de Freeze por Architect: M, invariantes, pruebas y OV | Una revision adversarial obligatoria | Rondas Coordinator ↔ Architect hasta acuerdo |
| Re-revision | Un disparador propio saca de esta columna | Cambios requeridos o cambio de elemento congelable | Igual | Igual |
| Implementacion | Coordinator por gate | Coordinator por gate | Coordinator por gate; Architect ante invalidacion | Igual; Coordinator puede pedir revision de gate que implementa extension congelada, con motivo |
| Conformidad final | Architect + Coordinator | Architect + Coordinator | Architect + Coordinator | Architect + Coordinator |

**Entradas (RC-07, RC-15).** La primera revision, en todo arquetipo, recibe el Discovery DC-1..DC-9, EXP y sus evaluaciones
negativas, revision del Coordinator, fuentes consumidas y la Proposal/Freeze completa. El Architect puede inspeccionar el
codigo en la base de ese Discovery. Toda re-revision recibe **la version completa actual + delta explicito + hallazgos previos
y disposicion por ID**. El delta es foco obligatorio, nunca alcance maximo. El **Architect** decide que secciones sin cambio
interactuan y por que; puede registrar un hallazgo material en cualquier seccion, aunque no tenga interaccion con el delta.
La ausencia de hallazgos abiertos en el listado del autor no invalida un hallazgo material que el revisor encuentre.

**Reglas anti-churn (EA Q5, C-14..C-17).**

1. Cada hallazgo se clasifica: aspecto nuevo / corrige mecanismo previo / consecuencia de reconciliacion / detalle de contrato
   o prueba / deriva de paralelas / editorial. La etiqueta de severidad no determina obligatoriedad.
2. **REQUERIDO** si (a) activa M-01..M-08 sobre un elemento congelable; (b) deja un invariante sin obligacion de prueba;
   (c) produce comportamiento observable distinto; **o (d) muestra un elemento congelable materialmente incorrecto, no
   ejecutable o no verificable**, incluido un oraculo que pasa con el invariante violado, o un plan de gates imposible o
   inconsistente. «Ya hay prueba» no basta si es ciega (B-07/B-41); «detalle de contrato» no permite omitir un gate imposible
   (B-15). Precisiones de redaccion sin cambio de significado ni supuesto (d) son OPCIONALES.
3. Una nueva ronda requiere al menos un REQUERIDO abierto: tambien cuenta el hallado en una seccion intacta o un cambio de un
   elemento congelable que requiera verificar la reconciliacion. Los opcionales se agrupan como errata/seguimiento.
4. Solo **quien emitio** el REQUERIDO puede rebajarlo a OPCIONAL, registrando ID, clasificacion anterior/nueva, razon y evidencia
   que demuestra que ya no satisface (a)..(d). El autor o Coordinator no lo rebajan por conveniencia. Si el emisor no esta
   disponible, sigue abierto: STOP para consenso y se escala, sin sustitucion tacita de autoridad.
5. Antes del Freeze el revisor confirma el diff de errata; cualquier cambio semantico vuelve a la revision pertinente. Se
   registra la identidad exacta de la version acordada segun P-09. AGREED no cubre «se aplicara luego».
6. Si los REQUERIDOS son consecuencias o detalles repetidos, Coordinator puede proponer podar detalle que P-09 no exige congelar.
   La poda exige conformidad registrada del **Architect**, lista de clausulas e invariantes conservados y disposicion de cada
   hallazgo por su emisor. No puede retirar significado requerido ni resolver un oraculo ciego eliminando la obligacion.
7. `AGREED` = ningun REQUERIDO abierto en esa version exacta; `CHANGES REQUIRED` en otro caso;
   `BLOCKED — OWNER DECISION` si falta decision reservada. «AGREED WITH CHANGES» no es consenso.
8. El Architect emite plantilla C, no aprueba politica. La revision del Coordinator del Discovery (P-05) y la revision por gate
   (P-10) no se sustituyen con este acuerdo.

**Independencia.** Cada revision declara `Review mode: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN` y si el revisor
es autor del texto. SAME-SESSION ROLE no afirma independencia. La politica general de exigir sesion separada para toda NEW
ARCHITECTURE sigue en U-08, sin decision aqui. **Para la revision final de I-56 rige la orden particular G4:** solo SEPARATE
SESSION o EXTERNAL HUMAN, despues de que Coordinator acepte la V3 exacta y con entradas versionadas. El autocontrol G4 no es esa
revision ni aporta un veredicto de consenso.

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

**Identidad acordada e integridad (RC-21, RC-23).** El registro de decisiones o evidencia, versionado antes de READY-04,
identifica la version acordada por Coordinator y Architect: commit, ruta y blob, con veredictos referidos a esa misma tupla.
No vale «la ultima Vn». La errata confirmada se aplica antes del acuerdo final o se vuelve a confirmar su identidad.

El commit de Freeze solo puede cambiar, en el artefacto, la linea `Frozen` y las lineas de estado de cabecera **enumeradas
literalmente en ese acuerdo**; ninguna clausula, tabla, obligacion ni enlace de significado cambia. Se registra el diff de esas
lineas entre blob acordado y blob congelado. READY-09 compara ambos contenidos y no acepta «cambio inocuo» no enumerado.

Trailer: `Freeze: <unidad-autora> <ruta-repo>`. Si hay artefacto delta, usa
`Freeze-Delta: <unidad> <ruta-repo>`. Para cada artefacto requerido, exactamente **un trailer de su clave y unidad** en commits
alcanzables desde el Candidato. No se cuenta por todas las refs ni por reflog. Una hermana referencia la unidad autora del
Freeze conceptual; no emite otro trailer para aquel. Una unidad que necesita solo su delta emite el trailer delta, sin exigir
un segundo Freeze propio vacio. Se extrae la ruta del trailer, **nunca de un enlace mutable del contrato**; se compara tambien
con la ruta acordada. Duplicado, ausente o redireccionado = fallo de READY-09.

El commit identificado toca el artefacto y es el ultimo que toca esa ruta, comprobado desde el Candidato. No se renombra ni
edita despues. Prohibido squash del commit de Freeze con otros cambios, cherry-pick duplicado o reescritura de su contenido.
Un rebase V1 puede cambiar su SHA y padres, no su trailer ni el contenido acordado/congelado; se verifica de nuevo unicidad,
blobs y diff permitido en la historia alcanzable. Esto protege **identidad documental del acuerdo**, no reutiliza evidencia
de pruebas, builds, conformidad u OV entre SHAs.

La cabecera congelada es historica. Estado posterior en contrato/decisiones; semantica posterior solo en A-n. Ningun Candidate
puede depender de una enmienda no versionada y alcanzable desde su HEAD (P-12).

**Freeze V1 consumido por una unidad V2.** Un Freeze V1 puede vivir en un contrato mutable o en una Proposal con bloques de estado,
asi que la regla del trailer no le aplica. La unidad V2 lista en su Discovery delta las secciones del Freeze V1 que consume y
registra en su archivo de evidencia la referencia de commit de esa lectura; READY-09 comprueba que **esas secciones** no cambiaron
desde esa referencia. Si cambiaron: **STOP** y se repite la comprobacion de compatibilidad de P-02 (pasos 2 a 4).

Reglas de version de la Proposal: cada version abre con «Cambios respecto de Vn-1»; no arrastra bloques de estado, preflights ni
tablas del Owner copiados de la anterior (viven una vez en el contrato o en el registro de decisiones; EA C-18); la version
congelada no depende de versiones previas para leerse.

**Enmiendas (RC-22).** Todo cambio posterior de un elemento congelado se registra como `A-n`, incluso una adicion de pruebas u
OV autorizada solo por Coordinator. Cada entrada contiene: ID, Freeze al que se aplica (unidad autora, ruta e identidad),
`Applies-to: all | <lista explicita de unidades>`, clausulas anteriores y delta exacto, motivo, M-01..M-08 y evidencia,
autoridades que decidieron y referencias de sus veredictos. `all` abarca todas las unidades que consumen ese Freeze, presentes
y futuras; una lista solo las nombradas. Una hermana no puede adoptar una A-n cuyo alcance no la incluye; ampliar alcance
exige nueva A-n. Todas se leen para comprobar alcance; solo las aplicables alteran el contrato de esa unidad.

Las conceptuales viven en `docs/automation/decisions/<I>.md`; las del delta de unidad, en
`docs/automation/decisions/<unidad>.md`. Por flujo/archivo, A-1, A-2, ... sin huecos ni duplicados; solo anexion. Corregir,
revocar o sustituir una A-n exige otra con ID siguiente que cita la anterior, nunca editarla o borrarla. Antes de numerar se
lee la punta remota del flujo; dos hermanas no reservan el mismo numero: coordinan una autora y secuencian la publicacion.
Un conflicto no se resuelve sobreescribiendo la entrada de otra unidad. Rebase conserva IDs y contenido de las entradas ya
publicadas; si descubre colision se detiene y resuelve con Coordinator/Architect antes de consumirlas.

**Visibilidad.** Una unidad hermana consume Freeze conceptual y sus A-n solo cuando sean alcanzables desde `origin/main`
tras fetch y desde su propio Candidato. Si el diseño necesario esta solo en la rama autora, espera su integracion V1; no se
hace cherry-pick para simular publicacion. La primera unidad autora puede consumir su propio Freeze/A-n ya versionados en su
rama. La misma regla se aplica a una A-n conceptual nueva redactada por una unidad posterior: ella es autora de ese delta;
las hermanas esperan que integre. Un delta exclusivo vive en la rama de su unidad y debe estar versionado antes de READY-04.
READY-09 enumera todas las entradas observadas, las aplicables y las excluidas con razon; verifica secuencia, contenido
append-only contra su historia, alcance y disponibilidad. READY-08 usa esa misma version para las asignaciones OV.

**Quien decide:**

| Clase | Condicion | Decide |
|---|---|---|
| Solo Coordinator | Tecnica no congelada sin A-n; o re-secuenciar gates sin cambiar resultados, añadir OV o pruebas de comportamiento **ya congelado**, mediante A-n | Coordinator; cada M-01..M-08 «no activado» con motivo y clausula protegida; llega al Architect antes del siguiente gate |
| Material | Cualquier M sobre un elemento congelado; riesgo nuevo; retirar/debilitar prueba protectora | Architect con version completa + delta, y Coordinator; arquetipo solo sube |
| Owner-reserved | Alcance/no-objetivos; cambio de decision Owner; retirar/sustituir OV o su ultima asignacion; ADR aceptado (nuevo ADR); NuGet u otra materia reservada | Owner, mas revision arquitectonica si activa M |

Una nueva prueba no puede introducir comportamiento nuevo bajo «solo Coordinator». El Architect puede objetar la
clasificacion antes del siguiente gate; una objecion material queda abierta segun P-08. Ninguna A-n local permite a una
hermana cambiar semantica conceptual por su cuenta. **Despues de READY-04**, toda A-n, aceptacion de desviacion o decision
Owner, incluida adicion OV, obliga a la ruta de correccion versionada y reinicio de P-12; no se aplaza semanticamente al cierre.

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
4. **CI push sobre el SHA de cierre en verde y leido** antes de abrir el siguiente gate: `event = push`, `ref = refs/heads/<rama-de-la-unidad>`, `head_sha` exacto, jobs
   requeridos en `success` `[NEW]`. Un rojo se diagnostica leyendo logs, TRX, volcados y artefactos **antes** de formular un gate
   de correccion (EA C-27, O-7).
5. Revision del Coordinator del gate (diff vs Freeze + A-n; hallazgos; desviaciones clasificadas) y **ninguna discrepancia EXP-01
   de clase A abierta** que afecte al gate (P-06 regla 5).
6. **Orden y registro (RC-25):** crear el commit de cierre del gate, confirmar arbol limpio, ejecutar las suites locales requeridas sobre ese HEAD, empujarlo **solo como punta** y leer su CI propio antes del gate siguiente. Commits internos pueden viajar agrupados; el cierre debe ser el tip que Actions mide, nunca un padre al que se atribuye el verde del siguiente. Su cuerpo registra lo conocido al crearlo, no afirma evidencia futura de su propio SHA. La evidencia posterior cita ese SHA en el informe y, duraderamente, en el cuerpo del commit siguiente o archivo de evidencia antes de READY-04; si es el ultimo gate, en el cierre final. No exige un `-CLOSE` adicional por ceremonia.
7. Si WORKFLOW §4.2 obliga a rebasar entre gates, la evidencia anterior sigue historica: se empuja la punta rebasada sola y se lee su CI de rama exacto antes de continuar; si porta el cierre de gate, se produce tambien su Core local requerido. Una nueva punta no hereda evidencia del cierre anterior.

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

**Categoria**: `[NEW]` READY; `[KEEP]` evidencia V1, exact-SHA, cierre documental y roles; `[BIND]` H.1-9..11.
Racional de V2 conservado: Candidato con CI rojo (EA Q9/B-12), base superada (B-33) e invalidaciones Q8. La muestra no demostro
Candidatos declarados antes de completar funcionalidad; no se atribuye tal ahorro a READY.

Antes de fijar `FINAL_CANDIDATE_SHA`, en este orden:

| ID | Condicion |
|---|---|
| READY-01 | Alcance congelado completo; diferimientos decididos por autoridad competente y versionados en A-n si cambian Freeze |
| READY-02 | Gates cerrados segun P-10; decisiones, A-n y documentos de producto listos y versionados |
| READY-03 | Sin REQUERIDOS abiertos, discrepancia A ni decisiones materiales/Owner pendientes |
| READY-04 | Fetch y rebase final segun WORKFLOW 4.5.1; preflight P-07. Si existe cierre previo, primero ruta R de abajo |
| READY-05 | Focales/relevantes sobre el SHA resultante y CI propio de punta de rama: event=push, ref=refs/heads/<rama>, head_sha exacto; **los cuatro jobs** Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI y Build Plugin en success |
| READY-06 | Architect + Coordinator: conformidad completa CONFORMING de ese SHA contra Freeze/delta/A-n versionados (P-19) |
| READY-07 | Arbol limpio, sin cambios ni commits pendientes para el producto; HEAD identificado |
| READY-08 | Matriz y asignacion OV en Freeze/delta/A-n: todo escenario conceptual requerido tiene al menos una unidad; toda obligacion aplicable a esta unidad preparada para el SHA final, checklist de guia incluido; ningun escenario retirado o ultima asignacion quitada sin Owner |
| READY-09 | P-09: acuerdo exacto, diff permitido, trailers unicos alcanzables desde HEAD, rutas de trailers sin edicion posterior; fuente V1 compatible; Freeze/A-n conceptuales visibles en main para hermanas; A-n append-only continuas y alcance/lista completa comprobados |

Las filas Git y evidencia del futuro lifecycle seran **punteros** a WORKFLOW y AGENTS, no autoridad duplicada (P-24).
Solo entonces se fija FINAL_CANDIDATE_SHA y se produce evidencia V1 completa: Core y UI Full local, builds Debug UI y Plugin,
CI exacto y Owner donde aplique. Orden local: commit → arbol limpio → evidencia sobre HEAD → registro. Se completa el bloque
de entrega de la guia §7.1 **antes de comenzar OV**; los resultados manuales se añaden despues. No se rellena anticipadamente
el veredicto del Owner. «Cuatro jobs» no permite omitir un job requerido nuevo por el workflow vigente.

**Ventana estable desde READY-04 (RC-17).** Solo los **hechos de ejecucion** (base, resultados CI, conformidad contra fuentes ya
versionadas, suites, builds, veredictos OV sin alterar alcance, metricas) pueden quedar en el informe de sesion y trasladarse
al archivo de evidencia en el cierre. No se hace commit intermedio para registrar esos hechos mientras el Candidato sea el
mismo. Una decision o semantica nueva no es un hecho de ejecucion y no puede quedar invisible hasta cierre.

| Evento tras READY-04 | Via legal |
|---|---|
| A-n de cualquier clase | Detener intento; versionar A-n con autoridad correspondiente, crear SHA nuevo y reiniciar READY-02 |
| Aceptacion de desviacion | Detener; versionar aceptacion y A-n si afecta Freeze; nuevo SHA, READY-02; nueva conformidad |
| Owner POLICY DECISION | Detener; versionar texto/alcance de decision y A-n necesaria; nuevo SHA y READY-02 |
| Adicion, sustitucion, retirada o reasignacion OV | P-14/P-09 para quien decide; versionar matriz mediante A-n, nuevo SHA y READY-02 |
| NON-CONFORMING, prueba/CI rojo u OV fallida | Diagnosticar; corregir codigo/documento o decidir con autoridad, versionar, nuevo SHA y READY-02; no aceptar verbalmente para pasar |
| Correccion descubierta, incluso documental | Si se cambia ahora, commit de correccion y READY-02; una errata no necesaria para conformidad puede registrarse como seguimiento sin cambiar semantica ni legitimar incumplimiento |
| Resultado PASS o FAIL que no cambia requisitos | Informe de ejecucion; archivo de evidencia al cierre; FAIL bloquea y conduce a correccion, no a cierre exitoso |

Reiniciar READY-02 incluye volver a recorrer READY-03..09, incluido fetch READY-04 aunque no se espere avance de main. Las
evidencias requeridas se producen de nuevo sobre el SHA nuevo. Se conserva la ronda invalidada como historia en el commit de
correccion/evidencia antes del nuevo READY-04. El registro de una decision no se posterga ni se llama «editorial» para evitar
ese reinicio. Una discrepancia A no puede aceptarse como desviacion (P-06).

**Ruta R — main avanza despues de CLOSURE_SHA (RC-20).** Antes de continuar integracion:

1. STOP. Identificar la ronda anterior (FINAL_CANDIDATE_SHA, CLOSURE_SHA, base, evidencia); comprobar arbol limpio, que el cierre
   contiene solo el delta documental permitido por WORKFLOW 4.5.4 y que no hay trabajo ajeno/posterior que se perderia.
2. Preservar el cierre anterior y su evidencia mediante la via de archivo segura de WORKFLOW §3; verificar que queda
   recuperable antes de retirarlo de la punta de la rama. Volver la rama de iniciativa al **tip de producto** anterior,
   FINAL_CANDIDATE_SHA. No borrar contenido ni usar reset destructivo sobre suciedad. Si el cierre mezclo producto o hay commits
   posteriores, detener y separar explicitamente con Coordinator; no tratarlo como cierre documental.
3. Fetch y rebase **del producto sin el cierre** sobre origin/main (WORKFLOW 4.5.1). Resolver conflictos; toda correccion o
   decision necesaria se versiona antes de READY-04. El cierre viejo no viaja en el rebase ni se cherry-pickea entero despues.
4. Empujar **el tip de producto rebasado solo**, con force-with-lease cuando V1 lo exige. Esperar su corrida propia de rama;
   no crear/pushar el cierre nuevo agrupado con el Candidato. Ejecutar READY, conformidad completa, Full local y builds,
   CI y OV aplicables sobre ese nuevo FINAL_CANDIDATE_SHA. Ninguna evidencia anterior se hereda.
5. Solo con esa evidencia y Owner donde aplique, **recrear** el cierre: regenerar estado compartido sobre la nueva base y
   evidencia nueva; conservar la ronda anterior como invalidada. No copiar su PASS como validacion de la ronda nueva.
6. Commit del cierre nuevo, guardia documental y push solo como punta, CI propio (4.5.4). Continuar re-fetch y 4.5.5–4.5.7;
   si main vuelve a avanzar, repetir R. No hay exencion por igualdad de arbol o por ser «solo rebase».

**Intermedios.** Un SHA parcial no se entrega ni se llama Candidato. Si el plan necesita un hito del Owner, se declara
Candidato intermedio con evidencia V1 completa y READY-02/03/05/07/08 respecto de sus gates; no sustituye al final. Todos los
OV aplicables se ejecutan en FINAL_CANDIDATE_SHA. Solo se reutiliza una ejecucion intermedia si su SHA es **literalmente el
mismo** final y conserva clase, proposito, alcance, AutoCAD y bloques; se aplican los invalidadores de AGENTS.

**Invalidacion V1.** Cualquier SHA nuevo invalida identidad: rebase, correccion o commit documental. Arbol sucio invalida
evidencia local; SDK resuelto distinto la dependiente del SDK; version de AutoCAD/bloques distinta la del Owner. Clases no se
sustituyen. El cierre documental autorizado tiene obligaciones propias por WORKFLOW 4.5.4; no hereda la evidencia ni se vuelve
Candidato de producto por una declaracion. Cualquier cambio de producto exige otro Candidato.

```text
FINAL_CANDIDATE_SHA = SHA que porta el producto validado de la ronda
CLOSURE_SHA = cierre documental de esa ronda, con su propia evidencia
MERGE_SHA = merge de esa ronda en main cuyo CI post-merge se verifica; en el tag, la ronda ya verificada
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
OV-id | Escenario | Por que aplica (disparador) | Sistema(s) | Datos (DWG nuevo / legacy) | Resultado esperado | Unidad(es) asignada(s) | Hito intermedio adicional: si/no
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
4. **SHA final obligatorio (RC-18):** cada escenario aplicable, incluidos los de la guia, se ejecuta sobre **FINAL_CANDIDATE_SHA** de la unidad. Un hito intermedio es adicional y exige Candidato intermedio con evidencia V1 completa (P-12). Solo si su SHA permanece literalmente como FINAL_CANDIDATE_SHA y no cambia proposito/alcance, AutoCAD ni biblioteca, ni otro invalidante aplicable de AGENTS, se reutiliza esa ejecucion. «Ya paso antes» sobre otro SHA no cubre un escenario final.
   **Asignacion (RC-19):** escenario→unidad vive en Freeze conceptual, Freeze delta o A-n, nunca solo en contrato mutable. Todo escenario conceptual requerido queda asignado al menos a una unidad y cada unidad ejecuta todos los que le aplican. Reasignar exige A-n explicita; retirar la ultima asignacion, retirar o sustituir el escenario exige **Owner**. Una asignacion conservada en otra unidad no excusa una obligacion que el disparador o checklist hace aplicable a esta. READY-08 comprueba cobertura completa y alcance de todas las A-n; un hueco detiene la entrega. Una modificacion tras READY-04 reinicia segun P-12.
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
| **Freeze** | `<I>-freeze.md`, la Proposal congelada o `<unidad>-freeze-delta.md` (P-09) | Solo los elementos congelados, incluida la matriz OV | **Inmutable** tras el commit de Freeze; enmiendas A-n fuera de el |
| **Evidencia operativa** | `docs/automation/evidence/<unidad>-evidence.md` (carpeta V1 existente) | Base del reclamo y evidencia de orden respecto de `WORKFLOW_V2_EFFECTIVE_SHA` (P-25); referencia de un Freeze V1 consumido (P-09); bloque de Candidato (guia §7.1), base del Candidato (READY-04), conformidad (resultado y SHA), registros de Owner Validation, fila de metricas (P-22), nombre del tag de integracion. No contiene el SHA del propio commit de cierre (va al tag) | Antes de READY-04, en commits documentales de la rama (p. ej. registros de Candidatos intermedios); desde READY-04, los hechos de ejecucion van al cierre; correcciones o decisiones nuevas invalidan y reinician antes del nuevo READY-04 (P-12) |

Los hechos **posteriores al merge** no caben en ninguna de las tres (aun no existen al cerrar la rama): van al **tag anotado
`integration/<unidad>`** de P-20, que no es un commit.

**Durante diseño e implementacion**, solo documentos locales de la iniciativa: contrato; Discovery; Proposal vigente o plan;
Freeze; registro de decisiones `docs/automation/decisions/<I>.md` **solo cuando hay decision material** (Owner, enmienda A-n,
resultado de revision de Architect); archivo de evidencia cuando hay evidencia que registrar. Las puntas observadas de ramas siguen P-07. Los resultados de gate producidos despues del commit se registran segun P-10: informe y cuerpo del siguiente commit o archivo de evidencia, nunca en el cuerpo del mismo SHA que aun no existia al redactarlo.

Excepciones que siguen su momento V1: el **archivo** de un ADR nace antes de implementar la decision (WORKFLOW §8); la propia fila
de ROADMAP en sus tres momentos (WORKFLOW §2).

**Guias y README** cuando cambia comportamiento visible: en el **ultimo gate de implementacion**, antes del Candidato, para que
la documentacion forme parte de lo terminado (AGENTS punto 4; WORKFLOW §8 «en la misma rama, antes de integrar») `[KEEP]`.

**Frontera semantica (RC-17).** La ventana de evidencia no autoriza demorar A-n, decisiones Owner ni aceptaciones de desviacion: se versionan como correccion y reinician P-12. El cierre solo transcribe resultados contra contratos ya versionados, no crea retroactivamente la semantica contra la que se declaro conforme el Candidato.

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
| Puntas ordinarias observadas de ramas | Cuerpo del commit e informe de gate/sesion | Excepciones exactas de P-07/P-25: base del Candidato en evidencia y snapshots PRE/POST de activacion en merge/tag; no estado vivo en contrato |
| Hechos posteriores al merge (`CLOSURE_SHA`, `MERGE_SHA`, CI post-merge, cobertura del Candidato, limpieza) | **Tag anotado `integration/<unidad>`** (P-20; MERGE_SHA de la ronda verificada y Unverified merges con rondas anteriores) | Archivo de evidencia apunta al nombre del tag; ningun documento escribe PENDING |
| Estado vivo del proyecto | HANDOFF | — |
| Plan y registro de cierre | ROADMAP | — |
| Contratos de fundaciones reutilizables | ADR aceptado / artefacto de Freeze integrado + A-n | FOUNDATIONS resume y enlaza; ARCHITECTURE enlaza el registro descriptivo |

**Resolucion de la duplicacion actual** sin borrar la unica autoridad durable: la Proposal deja de re-enunciar estado (P-09); las
salidas de Architect no se re-transcriben en la orden siguiente, se citan por ID de hallazgo; el contrato de una unidad de entrega
es delta (P-02); el bloque de HANDOFF es corto y enlaza al archivo de evidencia; las referencias a «HANDOFF §8-12» se corrigen a la
seccion real en la integracion normativa (U-13). **No se reescriben registros historicos** (AGENTS «Registro historico, no
precedente»); la poda de los bloques historicos actuales de HANDOFF queda en U-14. No se crea ningun documento grande nuevo: el
archivo de evidencia sustituye la «Evidencia final» del contrato y las copias de la evidencia del Candidato en varios sitios.

**Cambio de la regla de hashes** `[CHANGE]`: AGENTS («No copiar conteos de tests ni hashes de commit fuera de `docs/HANDOFF.md`
(seccion 12)») y WORKFLOW §8 pasarian a: hashes, corridas y conteos de ejecucion de una unidad viven en los cuerpos de commit, en su archivo de
evidencia y en su tag de integracion; las identidades exactas de acuerdos/Freeze y decisiones de transicion se registran en decisiones cuando P-09/P-25 lo requieren (sin copiar resultados de suites); HANDOFF enlaza; contrato, ROADMAP, indices y documentos normativos no los contienen. La guia
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

**Categoria**: `[CHANGE]` WORKFLOW §10 y AUTOMATION_PLAN §2. Racional V2 conservado: sus ordenes actuales difieren, no ubican
consistentemente ADR/decisiones/ordenes; EA O-1..O-3 muestran ordenes que contradicen normas. Se propone un dueño por dominio.

| Dominio | Autoridad y subordinacion |
|---|---|
| Hechos | Git, codigo, CI, pruebas y builds describen lo ocurrido; vencen afirmaciones falsas, no reescriben lo que debe cumplirse |
| Arquitectura | Convenciones de AGENTS y ADR aceptados (conflicto segun reglas de abajo) > artefacto de Freeze + A-n en su alcance; FOUNDATIONS solo resume y enlaza, sin rango normativo propio |
| Evidencia y pruebas | AGENTS «Pruebas» y «Reutilizacion»; guia manual como procedimiento; Freeze solo añade obligaciones |
| Git, integracion, cadencia documental, transicion | WORKFLOW; ADR de proceso aceptado en su alcance; AUTOMATION_PLAN solo implementa su ejecutor; contrato no cambia politica Git |
| Diseño y revision | INITIATIVE_LIFECYCLE: agrupacion, materialidad, Discovery, Freeze/A-n, revision, orden de READY y referencia precisa P-16; pasos Git/evidencia solo enlazan a WORKFLOW/AGENTS |
| Alcance de iniciativa | Decision explicita Owner registrada con alcance > Freeze + A-n > Proposal previa al Freeze; no renuncia a garantias H.1 |
| Plantillas | PROMPT_TEMPLATES es **procedimental y subordinado** a los dueños anteriores; no define requisitos, estados ni formato de evidencia por su cuenta |
| Estado y plan | HANDOFF y ROADMAP; no normativos |
| Orden de gate | Precisa y estrecha dentro del alcance autorizado; no amplifica ni sustituye las fuentes |
| Historia / ADR propuesto | Sin autoridad normativa ni precedente |

**Reglas de conflicto (RC-29/30):**

1. Cada regla tiene un dueño. WORKFLOW gobierna Git/integracion, AGENTS evidencia y lifecycle diseño/revision. Una fila READY
   con enlaces no crea autoridad duplicada. Si dos documentos pretenden definir la misma regla de modo incompatible: STOP,
   citar ambas clausulas y corregir la subordinada con la autoridad competente.
2. Frente a WORKFLOW/lifecycle, AGENTS manda sobre evidencia. Frente a lifecycle, WORKFLOW manda sobre Git/integracion y
   documentos compartidos. Un conflicto real entre dominios que no sea simple referencia errada no se decide por conveniencia.
3. ADR aceptado que declara explicitamente una excepcion a convencion general manda dentro de su alcance escrito, sujeto a
   H.1; conflicto no declarado entre ADR y AGENTS/WORKFLOW = STOP y Owner. ADR-0033 sigue propuesto y no gana autoridad aqui.
4. «La mas estricta» solo sirve si los requisitos son **comparables por contencion**: cumplir uno satisface integramente el
   otro (por ejemplo, todas las mismas condiciones mas una). Se conserva provisionalmente el que contiene al otro; el conflicto
   se registra y se resuelve antes del gate afectado. Si cumplir uno impide el otro, o no se demuestra contencion: **STOP y
   decide Owner**. Rebase vs conservar SHA no se ordena por «mas estricto»; se aplica la secuencia de nuevo Candidato P-12.
5. Las garantias de EA H.1 no son renunciables por excepcion local del Owner, Coordinator ni Architect. No se transforma un
   permiso de sesion en precedente general. Una orden que amplie/redefina norma detiene el gate; Coordinator puede resolver
   solo errata o interpretacion dentro de competencia, y politica corresponde al Owner.
6. Dato real contrario a prosa descriptiva: corregir la prosa en su momento permitido. Si afecta premisa consumida, EXP-01 A
   detiene; ni codigo ni registro legitiman incumplir ADR/Freeze (P-06).

**Decisiones Owner durables.** Se registran en `docs/automation/decisions/<I>.md`, con texto, razon, fecha, autoridad y alcance
`ESTA INICIATIVA / ESTE GATE` o `POLITICA`. Sin etiqueta, incluidas decisiones anteriores a V2: **locales**, no precedente.
Una politica propuesta solo rige para otras iniciativas cuando se materializa en su documento dueño mediante integracion
autorizada; una excepcion repetida no la activa. Si altera Freeze, se formaliza A-n. Si llega tras READY-04, P-12 reinicia.
La pausa transversal y registro de activacion de I-56 se autorizan por **V1**, no por este modelo aun propuesto (P-25).

### P-18 — Plantillas (procedimientos subordinados)

**Categoria**: `[NEW]`. Referencias de esta propuesta para revisar el diseño; P-24 indica donde se materializarian. Antes de
usar una plantilla como orden vigente se sustituyen referencias propuestas por ruta/seccion/clausula efectivamente publicadas.
Las plantillas no activan V2 ni añaden autoridad. Referencias comunes: P-16 campos de orden; P-17 conflictos;
AGENTS «Pruebas — definicion de terminado» puntos 1, 2, 5 y «Reutilizacion de evidencia»;
WORKFLOW §2 bootstrap/ROADMAP, §4.1 reclamo, §4.2 sesion, §4.5 integracion; guia manual §7.1 entrega.

**A. Coordinator Bootstrap Template**

```text
I-NN — BOOTSTRAP; workflow determinado por P-25, no elegido
Objetivo/IDs; agrupacion provisional P-02 (ambas condiciones); arquetipo provisional P-03/M-01..08
Autorizacion: fila ROADMAP o Owner, WORKFLOW §2 caso d; reclamo §4.1; fila sin estado en curso §2
Preflight: WORKFLOW §4.2 y P-07; Claim-Id, evidencia de clasificacion P-25 (T8 pendiente = STOP)
Contrato TEMPLATE: alcance, enlaces, Consumes/Extends/Introduces, coordinacion conocida, sin hashes
Despues de bootstrap: Discovery P-05 DC-1..9, EXP-01..09 con negativos razonados
Coordinator revisa Discovery y EXP omitidos antes de confirmar materialidad/arquetipo
No-touch: <ramas/rutas>; STOP: rama remota existente, interseccion funcional nueva, EXP pendiente,
  discrepancia A, disparador nuevo/UNKNOWN, pausa activa, orden temporal ambiguo, contradiccion de fuentes
Informe: base/reclamo/puntas con fecha, Claim-Id, clasificacion y evidencia, archivos, DC/EXP, revision requerida
```

**B. Executor Gate Prompt Template**

```text
I-NN — G<n>; objetivo verificable; alcance F-x..F-y; no-objetivos; invariantes y rutas/calientes
Freeze/delta: <identidad y ruta>; A-n aplicables: <IDs>; referencia ultima observacion de paralelas
Reglas: WORKFLOW §4.2 sesion; AGENTS «Pruebas» punto 2 RED; «0 seleccionadas = FALLO»;
  P-10 cierre/CI; P-11 Core local cierre (propuesta a materializar en AGENTS); P-09 enmiendas
Orden: commit de cierre -> arbol limpio -> Core local requerido -> push de ese cierre solo como punta
  -> CI propio leido (event=push, ref=refs/heads/<rama>, head_sha, cuatro jobs success) -> siguiente gate
Si rebase entre gates: nueva punta sola, CI propio y evidencia exigida antes de continuar (P-10)
Registro posterior: informe y siguiente commit/archivo de evidencia; no cuerpo del mismo commit
No-touch: <rutas>, Freeze; STOP: CI anterior no verde/leido, interseccion cambiada, M nuevo,
  discrepancia A, MATERIAL/OWNER-RESERVED, compilacion Plugin bloqueada por AutoCAD, contradiccion
CI rojo: diagnosticar logs/TRX/volcados antes de proponer correccion; condiciones especificas: <lista>
Informe: SHA, conteos focales >0, resultados/tiempos medidos, CI event/ref/head_sha/jobs, hallazgos/desviaciones
```

**C. Architect Review Template**

```text
I-NN — REVIEW <ronda>; modo SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; autor=revisor: si/no
Entrada: VERSION COMPLETA <commit/ruta/blob> + delta + lista/disposicion de hallazgos previos
Primera revision: Discovery DC-1..9, EXP y revision Coordinator; codigo de su base disponible
Foco obligatorio: delta; alcance disponible: version completa; Architect decide interacciones
Todo hallazgo material es valido, tambien fuera del delta (P-08); evaluar M y supuesto (d)
Salida: AGREED POINTS / DISAGREEMENTS / MATERIAL RISKS / REQUIRED CHANGES / OPTIONAL IMPROVEMENTS
Cada hallazgo: ID, elemento, razon, evidencia, requerido/opcional; rebaja solo emisor con razon registrada
CONSENSUS STATUS: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION, ligado a commit/ruta/blob
I-56 final: solo SEPARATE SESSION o EXTERNAL HUMAN; no se abre en G4
```

**D. Conformance Review Template**

```text
I-NN — CONFORMANCE <SHA>; Architect + Coordinator; Review mode; revision completa P-19
Contra: Freeze, fuente V1, delta y A-n versionadas; integridad READY-09 P-09
Traza: invariante -> codigo -> prueba/guarda; oraculo capaz de fallar; no-objetivos y extension conformes
READY-08: asignaciones OV completas; entradas FOUNDATIONS contrastadas con fuente/codigo; clase A = ninguna
CONFORMING | NON-CONFORMING; desviaciones con ID/clase/autoridad
Tras READY-04, aceptar desviacion o cambiar requisito: commit de correccion/A-n -> READY-02
Solo resultado contra fuentes ya versionadas puede ir al informe y transcribirse en cierre
```

**E. Final Handoff Template**

```text
Unidad / Claim-Id / Workflow; evidencia previa: docs/automation/evidence/<unidad>-evidence.md
Tag integration/<unidad> o correccion numerica valida; formato y verificaciones P-20 (futuro WORKFLOW §4.5)
FINAL_CANDIDATE_SHA / CLOSURE_SHA / MERGE_SHA: todos de la ronda verificada; MERGE_SHA = destino del tag
Unverified merges: cada ronda previa merge/candidate/closure/motivo, o none
Post-merge CI: run, event=push, ref=refs/heads/main, head_sha=MERGE_SHA, jobs, cobertura
Candidate coverage: dispatch, candidate_sha, measured-sha y artefacto; no usar head_sha del dispatch
Cleanup: rama local/remota/worktree y fecha; proteccion tags comprobada; correccion cita anterior
Owner POLICY refs; PRODUCT OV ids sobre FINAL_CANDIDATE_SHA, DLL SHA-256; entradas FOUNDATIONS; metricas
Tag ausente/malformado = desviacion, no integracion acreditada por existencia de nombre
La corrida ref=refs/tags/* no cuenta como evidencia de rama ni post-merge
```

**F. Candidate & Integration Prompt Template**

```text
I-NN — READY + INTEGRATION; P-12 READY-01..09; WORKFLOW 4.5.1–4.5.7 y §4 paso 6
Evidencia: AGENTS «Pruebas» punto 1 + «Reutilizacion»; guia §7.1 completa antes de OV; escenarios <IDs>
Documentos compartidos de cierre <lista>; P-15; proteccion y tag P-20
Si main avanza despues del cierre: P-12 ruta R
  preservar cierre viejo -> retirar cierre de punta -> tip de producto -> rebase sin cierre
  -> push Candidato solo -> READY/conformidad/Full/builds/CI/Owner sobre SHA nuevo
  -> recrear cierre con evidencia nueva y ronda anterior invalidada -> push cierre solo y CI propio
  -> re-fetch -> merge manual -> CI main exacto con cobertura -> dispatch cobertura Candidato -> limpieza -> tag
STOP: READY falso/UNKNOWN, main avanzado, arbol sucio, DLL incorrecto, M/A/Owner pendiente,
  CI/artefacto faltante; event=push Y ref=refs/heads/<rama> Y head_sha exacto, no refs/tags/*
Despues READY-04: nueva A-n/decision/aceptacion/OV -> versionar correccion -> READY-02, nunca esperar cierre
No limpiar/tag hasta 4.5.6 y 4.5.7 verificadas; rojo se corrige en rama, no en main
No sustituir cobertura Candidato por dispatch de MERGE_SHA; contradiccion = citar fuentes y STOP
Informe: plantilla E y evidencia por cada SHA; no heredar por arbol ni agrupar Candidato con cierre
```

### P-19 — Conformidad final

**Categoria**: `[NEW]` (codifica practica de I-45 CR1–CR3). **Evidencia**: la conformidad de I-45 hallo 4 HIGH tras «NONE» del
Coordinator (EA B-26) y dos remedios incompatibles creados por un gate (EA B-27); INV-09 de I-51 quedo sin prueba de comportamiento
y lo hallo I-54 despues del merge (EA B-30); contrato y fila de I-53D con «aviso y confirmacion» que V2 §7.8 no tenia (EA B-31).

**Definicion**: resultado implementado **vs** artefacto de Freeze (Freeze + enmiendas). **No es una ronda de rediseño**: una mejora
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
| EDITORIAL | Errata sin cambio de significado; si implica comportamiento distinto no es editorial | Coordinator; si se modifica tras READY-04, correccion y reinicio P-12, no legitimacion tardia en cierre |
| NON-MATERIAL | Helper, archivo, tecnica local que preserva invariantes | Coordinator |
| BEHAVIORAL-WITHIN-FREEZE | Detalle visible no congelado (texto de un mensaje no validado por OV) | Coordinator; si toca un escenario OV, Owner |
| MATERIAL | Disparador M-01..M-08 sobre un elemento congelado; invariante sin prueba (B-30) | Architect + Coordinator (en EXTENSION reclasifica) |
| OWNER-RESERVED | Alcance, no-objetivos, decision del Owner, escenario OV retirado, ADR aceptado | Owner |

`NON-CONFORMING` impide READY-06; se corrige (SHA nuevo) o se acepta la desviacion por quien corresponde con registro **versionado** y A-n si cambia Freeze; tras READY-04 se invalida el intento y reinicia P-12. La nueva conformidad se hace contra esa version, nunca contra un acuerdo verbal. Una
discrepancia EXP-01 de clase A abierta (P-06 regla 5) hace el resultado **NON-CONFORMING** y no se acepta como desviacion: se
resuelve por las vias de esa regla. La conformidad se hace contra **Freeze + A-n** (P-09).

### P-20 — Integracion, post-merge y registro durable

**Categoria**: `[KEEP]` mecanica V1 WORKFLOW 4.5.1–4.5.7 y §4 paso 6, sin commits directos en main ni merges automaticos;
`[CHANGE]` precision re-fetch antes del merge; `[NEW]` tag durable. Racional de V2: informes solos pierden hechos post-merge
(EA 0.5); commit posterior crea recursion; notas Git son poco visibles; un siguiente cierre ajeno puede no llegar. Se conserva
el tag anotado como opcion propuesta en OWN-H, no se crea ahora.

Secuencia: READY/rebase (4.5.1), Candidato exacto (4.5.2), dispatch opcional (4.5.2.bis), Owner (4.5.3), cierre con guardia y
CI propio (4.5.4); **fetch antes de 4.5.5** y, si main avanzo, P-12 ruta R; merge manual --no-ff (4.5.5), CI exacto de main
con cobertura (4.5.6), dispatch de cobertura del Candidato desde main (4.5.7), limpieza segura (§4 paso 6) y tag. El Candidato
rebasado y el cierre nuevo se empujan como puntas en momentos separados; ninguna corrida sobre el cierre acredita su padre.
Una correccion post-merge se hace en rama siguiendo V1 y vuelve a tener su ronda de Candidato/cierre/merge; no se arregla main
directamente. Una carrera con push rechazado no autoriza saltarse V1; la precision adicional de AR-36 queda LOW pendiente.

**Criterio CI (RC-27).** Evidencia de gate/Candidato: `event=push`, `ref=refs/heads/<rama-de-la-unidad>`, `head_sha=SHA exigido`,
jobs requeridos success. Post-merge: `event=push`, **`ref=refs/heads/main`**, `head_sha=MERGE_SHA`, cuatro jobs y artefacto de
cobertura. Se comprueba la ref de la corrida/evento, no solo un estado asociado al commit ni el nombre de rama inferido de
head_sha. `workflow_dispatch` mide el input candidate_sha: se verifica measured-sha y artefacto, no se atribuye evidencia de
push por su head_sha. `refs/tags/*` nunca acredita evidencia de rama ni post-merge. Un tag push rojo tampoco invalida por si
solo una corrida de main que cumple el criterio; se distingue de la corrida requerida, sin ocultarlo. Si no se puede
determinar event/ref/head_sha, no se declara evidencia. P-24 materializa estos criterios en AGENTS y WORKFLOW, sin editar CI.

**Proteccion previa (RC-28).** La consulta del preflight G4 devuelve **cero rulesets**. No existe hoy la proteccion propuesta.
Si Owner aprueba OWN-H, debe autorizar la accion administrativa de crear proteccion contra actualizacion/borrado de
`integration/*` y comprobarla **antes del merge normativo de I-56**, junto con permiso operativo para crear tags nuevos.
Se verifica configuracion efectiva, patron, restricciones, estado activo y bypass; se registra evidencia de esa comprobacion.
No se espera a la primera integracion V2 para descubrir el bloqueo. Protege de errores accidentales, no garantiza resistencia
al administrador capaz de bypass o de cambiar la configuracion. G4 no configura GitHub. Si Owner rechaza esta dependencia,
no se activa una parte de la propuesta: §6 exige version nueva que resuelva el registro durable.

**Reglas de tags (unidades V2; para I-56 requiere decision V1 especifica P-25):**

1. Crear tag **anotado** despues de verificar 4.5.6/4.5.7 y limpieza. El tag registra; no reemplaza ninguna de esas compuertas.
2. `MERGE_SHA` tiene un solo significado: merge de la **ronda verificada**, destino del tag. FINAL_CANDIDATE_SHA y CLOSURE_SHA
   son de esa misma ronda. Los merges rojos/no verificados anteriores van separados con sus propios candidate/closure/motivo.
   No hay alias alternativo de «main final». WORKFLOW 4.5.6 puede evaluar un MERGE_SHA que resulte rojo; ese intento solo pasa
   a Unverified merges cuando una ronda posterior se registra, nunca al campo del merge verificado.
3. Nombre exacto `integration/<unidad>`; correcciones `integration/<unidad>-corr<N>` con N entero positivo, orden **numerico**.
   Para consumir se usa regex anclada `^integration/<unidad-escapada>(-corr[1-9][0-9]*)?$`; no prefijos (I-53 no coincide con
   I-53S), no corr0, ceros iniciales ni sufijos libres. Cada correccion nueva usa N = maximo N publicado + 1 (base equivale a 0), sin duplicar nombres; se verifica la cadena Corrects, no se inventan registros para llenar huecos historicos.
4. Nunca mover, sobrescribir ni forzar un tag publicado. Correccion contiene el **bloque completo corregido**, `Corrects:` con
   nombre y objeto tag previo y `Correction reason:`. Apunta al mismo MERGE_SHA salvo correccion demostrada de destino; en ese
   caso aporta toda la evidencia de la ronda del nuevo destino. No basta una nota incremental; se conserva toda la historia.
5. El mensaje tiene estas claves requeridas, una vez cada una (none explicito cuando corresponde):

   ```text
   Initiative: <I>
   Unit: <unidad>
   Workflow: V2
   Claim-Id: <uuid>
   FINAL_CANDIDATE_SHA: <40 hex>
   CLOSURE_SHA: <40 hex>
   MERGE_SHA: <40 hex, destino verificado>
   Unverified merges: none | <merge / candidate / closure / motivo por cada ronda previa>
   Post-merge CI: run=<id> event=push ref=refs/heads/main head_sha=<MERGE_SHA> jobs=<resultados> coverage-artifact=<identidad>
   Candidate coverage: run=<id> event=workflow_dispatch candidate_sha=<FINAL_CANDIDATE_SHA> measured-sha=<mismo SHA> artifact=<identidad>
   Cleanup: local-branch=<resultado> remote-branch=<resultado> worktree=<resultado> date=<fecha>
   Evidence: docs/automation/evidence/<unidad>-evidence.md
   Corrects: none | <tag anterior y su objeto>
   Correction reason: none | <razon verificable>
   ```

6. **Verificacion manual hoy, sin script requerido.** Tras `git fetch --tags origin`, usar
   `git for-each-ref refs/tags/integration/ --format='%(refname) %(objecttype) %(objectname)'` y seleccionar por la regex exacta;
   `git cat-file -t refs/tags/<nombre>` debe dar `tag`; `git cat-file -p refs/tags/<nombre>` muestra destino/mensaje y `type commit`;
   `git rev-parse refs/tags/<nombre>^{commit}` debe igualar MERGE_SHA. Comprobar destino en
   `git rev-list --first-parent origin/main` y padres con `git show -s --format=%P <MERGE_SHA>`: merge --no-ff de la ronda,
   CLOSURE_SHA como segundo padre y Candidato en su ascendencia, con delta de cierre documental permitido. Leer el mensaje:
   claves unicas/no vacias, SHAs validos, unidad/Claim-Id correctos, refs/runs/jobs/cobertura/limpieza concordantes con evidencia.
   Verificar tambien cadena de correcciones, numeracion, Corrects y alcance. Estos son comandos de lectura propuestos para
   WORKFLOW §4.5, no una herramienta nueva; cada salida desconocida queda UNKNOWN, no success por existir un nombre.
7. La sesion de integracion y el preflight de la siguiente integracion comparan las unidades V2 integradas (Claim-Id y registros
   de transicion) con tags validos. Ausente, ligero, destino incorrecto, mensaje malformado o correccion sin anterior =
   **DESVIACION DE PROCESO**. Tambien se señalan sufijos malformados del nombre exacto de unidad (p. ej. -corr0), sin confundir unidades hermanas. No se elige silenciosamente el tag anterior si el mayor N es invalido. Recuperar hechos verificables
   y publicar base faltante o siguiente correccion completa, sin mover el existente. Si el nombre base lo ocupa un tag
   malformado, corr1 puede corregirlo citando su objeto y defecto; las demas condiciones deben cumplirse.
8. Datos que no se pueden recuperar se reportan **UNKNOWN** y se escala la reparacion; no se fabrica un registro «verificado».
   Un bloqueo de permisos/artefactos no se resuelve atribuyendo un measured-sha por analogia (RC-40 pendiente). Los tags guardan
   el veredicto y las identidades cuando caducan los logs, pero no prometen que los artefactos vivan indefinidamente.
9. Consumo toma el base o mayor N **solo despues** de validar cadena y bloque completo; se consulta proteccion antes de publicar.
   Sin proteccion aprobada/activa se detiene la publicacion y se escala, no se finge durabilidad. No se cambia `ci.yml` para evitar
   corridas de tags (U-15); no hay creacion, merge ni limpieza automatica en I-56.

### P-21 — Scripts futuros (solo diseño; no se implementan en I-56)

**Categoria**: `[NEW]` (diseño). `eng/**` no se toca en I-56 (EA H.1-1). Se implementarian en una iniciativa propia, con reclamo y
Candidato. Regla comun: **fail-closed**; una comprobacion requerida que no puede determinarse devuelve UNKNOWN y sale con codigo de
fallo; una seleccion que no selecciona nada es FALLO (AGENTS).

| Script | Entradas | Salidas | Semantica de fallo | Queda como decision humana |
|---|---|---|---|---|
| `initiative-preflight.ps1` | Worktree; rama; lista de areas probables; tabla WORKFLOW §7 | Estado Git (arbol, stash, operaciones en curso, divergencia con `origin/main`); clasificacion por Claim-Id previa a toda inferencia de ascendencia, snapshots PRE/POST y conflictos T6/T8 segun P-25; ramas activas con rutas `merge-base..punta`; intersecciones clasificadas como candidato funcional / archivo / documento compartido de cierre; entradas de FOUNDATIONS extendidas por contratos de ramas activas (UNKNOWN para contratos V1); puntas observadas para el informe y el cuerpo del commit, nunca para el contrato | Error de fetch, arbol sucio, operacion en curso o worktree ocupado = FALLO; clasificacion funcional no determinable = UNKNOWN visible (nunca «independiente») | Si una interseccion es dependencia funcional; secuenciar o dividir |
| `candidate-check.ps1` | SHA propuesto; identidad de unidad/Freeze/delta; registro de acuerdo y A-n; ruta del contrato | READY-04/05/07/09 comprobables mecanicamente: base al dia, CI push del SHA (event, ref de rama exacta, head_sha, jobs), arbol limpio, archivo congelado con trailer unico, ruta extraida del trailer, diff de estado autorizado y A-n integras/visibles segun READY-09; plantilla del bloque de Candidato de la guia §7.1 | Cualquier condicion no verde o UNKNOWN = FALLO; nunca rellena lineas por analogia | READY-01/02/03/06/08 (alcance completo, conformidad, OV); declarar el Candidato |
| `post-merge-check.ps1` | `MERGE_SHA`; SHA del Candidato | Corrida de CI de `MERGE_SHA` con los cuatro jobs; artefacto de cobertura; dispatch del Candidato con `measured-sha` == Candidato; veredicto de si la limpieza procede; borrador del mensaje del tag `integration/<unidad>` (P-20); unidades V2 integradas antes sin tag valido (tipo, nombre, destino, claves y correcciones segun P-20) | Corrida ausente, job no `success`, artefacto ausente, `measured-sha` distinto o corrida de `ref` distinto de `refs/heads/main` = FALLO; no borra nada ni crea el tag | Ejecutar la limpieza; crear y publicar el tag; que hacer ante un rojo (siempre en la rama) |
| `evidence-report.ps1` | Iniciativa; rango de commits | Tabla de corridas por SHA y clase (excluye corridas de `refs/tags/*`); unidades V2 integradas sin tag validado por todas las reglas de P-20; Full afirmados en cuerpos de commit vs corridas medidas; fila de metricas P-22 con UNKNOWN explicitos; entradas del registro de fundaciones con simbolos o pruebas citados inexistentes | Datos no recuperables = UNKNOWN (nunca 0); no clasifica valor | Interpretar metricas; marcar entradas obsoletas |

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
| H9 Revision independiente vs de rol | P-08 `Review mode` (obligacion general pendiente U-08; I-56 final separada por orden G4) | Tasas de hallazgos equivalentes entre modos en una comparacion controlada |

### P-23 — Dry-run obligatorio futuro (metodo y casos)

**Categoria**: `[NEW]`. G4 **no** ejecuta el dry-run normativo; la revision adversarial interna de §11 no es el dry-run ni lo abre.

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

### P-24 — Conjunto minimo de archivos normativos y dueño unico

**Categoria**: `[NEW]` recomendacion. G4 solo crea esta Proposal; no modifica normas ni crea archivos de este plan.
Se conserva la recomendacion de V2 de tres archivos: `docs/INITIATIVE_LIFECYCLE.md`, `docs/FOUNDATIONS.md` y
`docs/initiatives/PROMPT_TEMPLATES.md`. No se crea `ORCHESTRATION.md`: no tiene dominio distinto. Tampoco se añade otro
«workflow-v2» en docs/process. Ubicacion de FOUNDATIONS sigue U-02; su autoridad no depende de la carpeta.

| Dueño | Contenido que define | Contenido que solo referencia |
|---|---|---|
| WORKFLOW §4.5 | Git/integracion, push de puntas, re-fetch, vuelta tras cierre, evidencia CI post-merge event/ref/head_sha, verificacion manual de tags y formato completo de tag P-20 | Composicion de Full local en AGENTS; escenarios de guia |
| WORKFLOW §8 | Cadencia y tres superficies documentales, ubicacion de hashes/conteos, **esqueleto del archivo de evidencia** | Semantica de Freeze/A-n del lifecycle |
| WORKFLOW §10 y nueva seccion transicion | Autoridad P-17 y mecanismo P-25, snapshots, identidad Claim-Id, activacion | Decisiones Owner registradas; fuentes de evidencia |
| AGENTS «Pruebas» punto 1 y «Reutilizacion» | Cadencia Core propuesta P-11; criterios de CI de rama: **event=push + ref=refs/heads/<rama> + head_sha exacto + job success**; clases/invalidadores sin cambio | Hashes/cadencia documental a WORKFLOW §8 |
| INITIATIVE_LIFECYCLE | P-01..05, revision P-08, Freeze/A-n P-09, resultados de gates P-10, orden READY P-12, referencia precisa P-16, conformidad P-19, metricas P-22 | READY-04 a WORKFLOW 4.5.1; READY-05 y evidencia a AGENTS/WORKFLOW; cierre/rebase/post-merge a WORKFLOW; OV a guia; no copiar sus reglas |
| Guia manual §7/§7.1 | Matriz OV aditiva, asignacion/final SHA y entrega DLL P-14; bloque de entrega | READY en lifecycle; clases/reutilizacion en AGENTS; destino de evidencia en WORKFLOW §8 |
| FOUNDATIONS | Solo metadata descriptiva de fuentes verificadas P-06 | ADR/Freeze + A-n y codigo/pruebas; reglas de poblacion/verificacion en lifecycle, no autoridad arquitectonica independiente |
| PROMPT_TEMPLATES | Procedimientos y campos de P-18, ejemplos de Freeze/delta | Reglas obligatorias P-16 en lifecycle; formato de tag y evidencia en WORKFLOW; ninguna autoridad de ciclo de vida propia |
| TEMPLATE | Campos de contrato mutable y enlaces: workflow, agrupacion, arquetipo/M, Consumes/Extends/Introduces, coordinacion, Discovery, Freeze, asignacion OV, decisiones/evidencia | No hashes, puntas ni matriz OV mutable como fuente; estado posterior no modifica Proposal |

**Modificaciones existentes futuras:** WORKFLOW §4/§5 y §7 enlazan los procedimientos pertinentes; AGENTS requiere orden expresa
fuera de docs (contrato I-56 §12), incluye ref en su criterio de UI del CI y remite hashes a WORKFLOW; AUTOMATION_PLAN §2 remite
a WORKFLOW §10 sin activar ejecutor ni cambiar limites. Guia incorpora final-SHA y P-12 para cambios tras READY-04. Indices de
iniciativas/ADR y context packs enlazan, ARCHITECTURE enlaza FOUNDATIONS; ROADMAP/HANDOFF solo en cierre de I-56. Se corrigen
referencias a secciones HANDOFF inexistentes al materializar, sin reescribir historia. ADR de Workflow requiere OWN-N antes de
gates normativos conforme a WORKFLOW §8; no queda para despues de implementar.

Cada regla tiene exactamente un dueño en esta tabla. Un texto explicativo en la Proposal puede resumir la secuencia para
evaluarla; al materializar, lifecycle/plantillas usan punteros con clausula, no re-enuncian Git ni evidencia. Una discrepancia
de copias no se resuelve creando una segunda autoridad. No se implementan los scripts de P-21 ni se modifica `.github/**`.

### P-25 — Transicion y WORKFLOW_V2_EFFECTIVE_SHA

**Categoria**: `[NEW]` sobre `[BIND]` contrato I-56 §0.1/§0.2. No existe ni se asigna SHA efectivo en esta propuesta. I-56
permanece V1 hasta su cierre, incluido el proceso con que integra las normas V2. No se cambian contratos activos ajenos.

**Precondiciones y unidad de aprobacion.** Coordinator y Architect acuerdan la misma identidad de Proposal; para I-56 final el
Architect es sesion separada o humano externo; dry-run futuro P-23; Owner aprueba la version completa (§6). Despues se abren
gates normativos, con autorizacion expresa de AGENTS y decisiones previas requeridas. Todas las normas entran en **un unico
merge normativo**, por V1. Si no caben juntas o se rechaza una parte, STOP y nueva version/reconsenso/Owner; no activacion parcial.

**Identificacion durable determinista (RC-06).** En la materializacion, exactamente un commit de I-56 lleva trailer
`Workflow-V2-Normative: I-56`, identificando el conjunto normativo aprobado. No lo llevan Proposals, revisiones, cierres,
merges de correccion ni tags. El merge normativo es el **primer merge en el orden de `git rev-list --first-parent --reverse origin/main` (orden de la cadena, no fechas de autor/committer) cuyo segundo padre alcanza ese commit con trailer**, y cuyo primer padre aun no lo alcanza. Se comprueba el
trailer exacto en la historia del segundo padre y su unicidad en esa historia. Falta, duplicidad o incumplimiento estructural
detienen la identificacion; no se adivina por asunto/fecha/nombre de rama. La autorizacion de integracion identifica tambien
el conjunto normativo y la version aprobada. La regla usa commits retenidos en main, no una rama que luego se borrara.

`WORKFLOW_V2_EFFECTIVE_SHA` es ese merge --no-ff normativo. El momento de vigencia es la **aceptacion remota del push de main
que lo publica**. Ni su creacion local, ni el CI posterior, ni pausa, tag, limpieza o merge de correccion crean otra vigencia.
El SHA no se escribe dentro de si mismo. `integration/I-56` lo repetira como dato de activacion, aunque su destino MERGE_SHA
sea una ronda posterior corregida; el dato debe coincidir con la derivacion anterior, nunca redefinirla.

**PRE/POST de reclamos remotos (RC-02).** La sesion V1 de integracion toma y conserva dos snapshots completos:

| Captura | Momento / contenido / destino |
|---|---|
| PRE | `git ls-remote --heads origin` justo antes de publicar el merge normativo; guardar comando/salida, fecha UTC, main observado, y por rama de iniciativa: ref, tip, commit de reclamo original verificable y Claim-Id extraido del reclamo aceptado. La salida y tabla se incorporan al **cuerpo del merge normativo** antes de empujarlo. Si se retrasa o cambia la preparacion, repetir PRE y regenerar cuerpo/merge; no afirmar que un snapshot viejo es «justo antes» |
| POST | Mismo comando y tabla inmediatamente despues de la aceptacion remota del push de main, en el mismo registro ordenado que muestra el push aceptado y el merge publicado. Se conserva en el informe post-merge y en el mensaje durable de activacion `integration/I-56` autorizado por V1; no se añade un commit recursivo a main |

PRE incluye todos los reclamos remotos, no solo iniciativas del caso (d) con bootstrap. I-56, I-49, I-52 e I-55 se identifican
sin editar sus contratos. Una rama presente en PRE con identidad valida prueba **V1 definitivo**; presente solo en POST = T6,
porque pudo nacer antes o despues del push en esa ventana. Ambas tablas se cruzan por **Claim-Id**, conservando refs/commits
como evidencia. Cambio de tip, nombre de rama o rebase no crea reclamo nuevo. UUID duplicado entre reclamos distintos,
ausente o imposible de vincular = conflicto de identidad, no se asigna uno inventado. Tratamiento de legacy sin registro es
OWN-Q: se pide decision apoyada en evidencia y no se reescribe contrato V1 ni se aplica V2 por descarte. Snapshot incompleto
o identidad no resuelta se reporta; antes del merge debe quedar resuelto su tratamiento, no ocultarse la omision.

**Clasificacion estable y escalera (RC-01).** Primero se busca una clasificacion durable previa por Claim-Id, incluidos PRE
y decisiones T6: se conserva. Solo un reclamo **aun no clasificado**, fuera del PRE, usa las pruebas de su **primer push
aceptado**, nunca el padre actual de un commit rebasado. No basta volver a encontrar el mismo trailer en otra historia.

1. Base original documentada de ese primer push contiene el efectivo: posterior (V2, salvo opcion expresa T8-b). Un rebase
   posterior que introduce el efectivo no sirve para esta prueba. Si otra evidencia contradice la identidad/orden: T6.
2. Base original no contiene el efectivo y se pretende probar anterioridad: se exige **un mismo registro ordenado de
   comandos/salidas** que muestre (a) primer push de reclamo aceptado y (b) fetch **posterior** con origin/main aun sin el
   efectivo. Un fetch anterior, fecha aislada o frase «hice fetch» no prueba nada. Ese registro va al cuerpo del primer commit
   posterior (bootstrap solo si V1 lo requiere) y a evidencia operativa permitida; no exige crear bootstrap a contratos V1.
   Se contrasta obligatoriamente Actions: corrida **push de esa rama y SHA original de reclamo**, frente a corrida **push de
   main y SHA normativo**. Si la corrida del reclamo es posterior a la normativa, hay incoherencia → **T6**, aun si el registro
   parece probar anterioridad. Ausencia de datos para ese contraste tampoco valida el peldaño 2: T6. Actions solo corrobora;
   sus horas de creacion no demuestran por si solas el orden de aceptacion.
3. Base antigua + fetch posterior ya contiene efectivo, solo POST, registro incompleto o incoherente, o ninguna prueba
   concluyente: **T6, STOP y Owner con la evidencia disponible**. No hay V1 por defecto. Una decision que confirme posterior
   con base obsoleta lleva a T5; la que pruebe anterior se registra V1 por Claim-Id. No se permite elegir workflow por comodidad.

Para reclamos nuevos creados despues de POST sigue la escalera; un fetch actual correcto tendra el efectivo. Si usaron base
obsoleta y no existe prueba temporal concluyente, T6; no se infiere anterioridad por ausencia del SHA en sus padres. Resultado
en contrato propio V2 como valor de workflow, y Claim-Id/pruebas en evidencia/cuerpo de commit. Los V1 conservan su contrato:
su PRE/decision durable basta sin imponer campos nuevos. Una herramienta futura consulta primero identidad/clasificacion;
**ningun rebase reclasifica** una iniciativa ya clasificada.

**Pausa y tag de activacion con autoridad V1 (RC-05).** El Owner decide si pausa todos los reclamos de la ventana de
activacion, su inicio, excepciones (incluidos fix) y resolucion de bloqueo; autoridad actual: AUTOMATION_PLAN §11 y WORKFLOW
§2, no P-17 propuesto. Se registra y publica en `docs/automation/decisions/I-56.md` **antes de 4.5.1**, con alcance transversal
de ventana, y Coordinator la comunica. La propuesta recomienda terminar solo tras verificacion V1 4.5.6/4.5.7 y registro
durable de fin; obligatoriedad/excepciones quedan OWN-P, no decididas en G4.

El mismo paquete V1 autoriza de forma explicita el registro `integration/I-56` y la configuracion de proteccion antes del
merge (OWN-H), y hace **obligatorio** en esa activacion el registro durable de POST y del fin de pausa cuando la haya. Su
mensaje incluye formato completo P-20, `Workflow: V1`, WORKFLOW_V2_EFFECTIVE_SHA derivado, PRE (referencia al cuerpo del merge),
POST completo y `Claim pause: none | start=<...> end=<...> decision=<referencia>`. Es una decision V1 del Owner con contenido
expreso, no V2 autoaplicada a I-56. El tag se crea despues de las compuertas y limpieza; hasta que el fin este registrado se
considera activa la pausa acordada. No se deduce el fin de que una rama exista o desaparezca ni de una comunicacion efimera.
Si no se aprueba este mecanismo durable, no se activa silenciosamente: nueva propuesta de registro y consenso (§6).

**Tabla de verdad (orden por Claim-Id, no por ascendencia actual):**

| ID | Condicion comprobada | Workflow | Accion |
|---|---|---|---|
| T1 | PRE o prueba valida de anterior; sin pausa infringida | V1 | Clasificacion definitiva por Claim-Id |
| T2 | Anterior probado; reclamo dentro de pausa iniciada antes de vigencia | V1 | Desviacion; se aplica suspension/excepcion segun decision V1 de pausa; no cambia workflow |
| T3 | Posterior probado; pausa activa | V2, salvo decision expresa T8-b | Desviacion y STOP hasta fin durable o excepcion explicita de pausa; nunca V1 por violar pausa |
| T4 | Posterior probado; base original contiene efectivo; sin pausa | V2, salvo T8-b | Normal |
| T5 | Posterior confirmado desde T6, base original obsoleta | V2, salvo T8-b | Desviacion y STOP; rebase conforme V1 sobre main actual antes de trabajo sustantivo, mas pausa si aplica |
| T6 | Orden/identidad no determinable o contradiccion | Sin clasificar | STOP; Coordinator escala, Owner decide por evidencia; nunca V1 por defecto |
| T7 | Nueva unidad de diseño V1, posterior, fuera de las tres lineas nominales | V2 | Compatibilidad P-02; fuente V1 sin reescritura; delta propio |
| T8 | Intento de unidad nueva de I-49/I-52/I-55 con interpretacion nominal pendiente | Pendiente de OWN-E | **STOP antes de reclamar/implementar**, aun si el orden temporal parece claro; si ya se reclamo por error, no continuar ni clasificar por defecto |

**OWN-E ofrece ambas opciones, ninguna elegida:**

- **T8-a:** grandfathering nominal cubre reclamos/ramas/contratos existentes. Unidad nueva posterior = V2 con contrato,
  Freeze delta y A-n **propios**, compatibilidad P-02 y sin escribir archivos de la iniciativa V1. Sus evidencias son propias.
- **T8-b:** grandfathering cubre la linea nombrada hasta su cierre registrado. Las unidades nuevas que ejecutan alcance de
  esa linea mientras siga abierta se gobiernan V1 aunque su reclamo sea posterior. Es una **excepcion nominal explicita del
  Owner**, no herencia general ni clasificacion por rebase. OWN-E debe identificar alcance de cada linea y acto/registro que
  determina su cierre; tras ese cierre, un reclamo nuevo sigue regla general V2. No se permite ampliar indefinidamente la linea
  para obtener V1. No modifica los contratos existentes ni transfiere evidencia entre unidades.

Hasta OWN-E ninguna opcion tiene preferencia ni valor por omision; si Owner altera una opcion, §6 exige nueva version acordada.

**Lectura de normas por V1 despues de vigencia (RC-04).** Se consulta main actual, incluida esta seccion de transicion y
archivos calientes. **Solo las clausulas cambiadas por el diff del merge normativo** se leen para V1 en
`WORKFLOW_V2_EFFECTIVE_SHA^1`; la integracion proporciona mapa de esas clausulas y sus destinos. No se congela todo AGENTS o
WORKFLOW: cambios ajenos a activacion siguen leyendose de main actual. Clausula V2 nueva no impone obligaciones a V1 salvo el
mecanismo de clasificacion/lectura aprobado expresamente. Conflicto posterior sobre aplicabilidad de clausula: STOP y Owner,
no eleccion silenciosa de todo un archivo antiguo. I-56 conserva V1 despues de su propio merge.

**Verificacion roja / reversion.** Un merge normativo publicado ya fija vigencia; rojo no hace otro SHA efectivo ni convierte
reclamos nuevos en V1. Si se acordo pausa, permanece hasta fin durable segun la decision. Correccion por rama V1 de I-56 y
nuevo merge con CI propio; si cambia la politica aprobada, reconsenso y Owner. Reversion/desactivacion es OWN-S pendiente:
no hay rollback automatico ni borrado del efectivo historico. Antes de activar debe quedar decidida la politica para reclamos
entre activacion y eventual reversion, sin re-clasificacion por mero cambio de ascendencia. Migracion voluntaria de V1 queda
fuera de I-56 (U-10).

### P-26 — No-objetivos

**Vinculantes** `[BIND]` (EA H.1): sin cambio de producto; sin cambio de pruebas de producto; sin implementacion de CI; sin
implementacion de scripts; sin V2 retroactiva; sin T0–T4; sin R0–R4; sin Quick CI; sin eliminar la validacion Full del Candidato;
sin eliminar la Owner Validation; sin relajar exact-SHA; sin identidad de evidencia por mismo arbol; sin merges automaticos.

**Especificos de esta propuesta:**

- Sin puntuaciones numericas de agrupacion, materialidad o riesgo.
- Sin clasificacion automatica de arquetipos; la herramienta futura solo aporta datos.
- Sin decidir una obligacion general de revision independiente para todo arquetipo (U-08); la revision final particular de I-56 **si** exige sesion separada o humano externo por orden G4.
- Sin poblar el registro de fundaciones copiando prosa existente.
- Sin reescribir registros historicos, HANDOFF pasado ni contratos cerrados.
- Sin cambiar la poblacion ni los disparadores de `ci.yml`; sin cambiar la cadencia de cobertura.
- Sin convertir la duracion activa del Owner en campo obligatorio.
- Sin aceptar, rechazar ni reabrir ADR-0033 (sigue `propuesto`).
- Sin guardias de patch-id ni igualdad de arbol como sustituto de rebase o de evidencia.
- Sin crear `docs/ORCHESTRATION.md`.
- Sin mover ni forzar tags `integration/*`, sin notas de Git y sin commits de registro sobre `main` para los hechos posteriores al merge.
- Sin herencia automatica de workflow entre reclamos; solo la interpretacion nominal T8-b podria crear la excepcion expresa pendiente del Owner. Sin reescribir Freeze V1 consumido.

---

## 6. Decisiones reservadas al Owner (paquete futuro; NO se solicitan ahora)

Se presenta despues de acuerdo Coordinator + Architect independiente sobre la **misma version exacta** y dry-run P-23.
Todo queda **PENDIENTE**, incluidas las opciones recomendadas. Decisiones tecnicas propuestas no equivalen a aprobacion global.

| ID | Decision de politica reservada | Destino |
|---|---|---|
| OWN-A | Arquetipos y su efecto limitado en Discovery/diseño; no evidencia | P-03..05 |
| OWN-B | Participacion del Architect, anti-churn y conformidad/Freeze | P-08/09/19 |
| OWN-C | Core local al cierre de gate en vez de cada push interno; no adopcion de H3 fuerte | P-11, AGENTS |
| OWN-D | READY y Candidatos intermedios sin reducir evidencia final | P-12 |
| OWN-E | Interpretacion nominal **T8-a o T8-b**, alcance/cierre de lineas si B; transicion por reclamo y no retroactividad | P-25 |
| OWN-F | Obligacion de referencia precisa y campos de orden; plantillas subordinadas | P-16/18 |
| OWN-G | Retirar ceremonia de -CLOSE por gate y Discovery expandido por defecto; no vuelve a votar Core/revision ya en OWN-C/B | P-05/10 |
| OWN-H | Re-fetch y retorno tras cierre; namespace de tags durables, proteccion contra borrado/reescritura; **autorizar creacion/verificacion del ruleset antes del merge normativo**, y registro de activacion de I-56 por V1 | P-20/25 |
| OWN-I | Cobertura sin cambio, explicitado en paquete; no decision tecnica nueva | P-13 |
| OWN-J | Agrupacion conjuntiva y unidades con fundacion/primer consumidor; evidencia propia por unidad | P-02 |
| OWN-K | Tres superficies y cadencia documental, hashes/conteos; crear y ubicar registro descriptivo | P-06/15/24 |
| OWN-L | Autoridad por dominio, H.1 no renunciable localmente, decisiones sin alcance locales, plantillas subordinadas | P-17/24 |
| OWN-M | Matriz OV aditiva y asignacion congelada, todos los aplicables en SHA final; intermedios adicionales | P-14 |
| OWN-N | ADR de Workflow V2: decision y aceptacion antes de implementar gates normativos (WORKFLOW §8); no aceptacion tacita | P-24 |
| OWN-O | Orden expresa para editar AGENTS.md, fuera del alcance docs de I-56; no la concede G4 | Contrato §12 |
| OWN-P | Pausa transversal V1 obligatoria u opcional, inicio, excepciones fix/emergencia, fin durable y salida de bloqueo prolongado | P-25 |
| OWN-Q | Reclamos legacy sin Claim-Id/registro suficiente: tratamiento con evidencia sin defaults ni reescritura ajena | P-25 |
| OWN-R | Contenido de poblacion inicial FOUNDATIONS, despues de verificacion y conformidad por entrada | P-06 |
| OWN-S | Politica de reversion/desactivacion y tratamiento de reclamos entre activacion y reversion | P-25 |

OWN-N..S recogen las materias de politica faltantes señaladas por la revision. El diseño de la escalera/snapshots, reglas A/B,
verificacion del DLL construido, formato de campos y comandos manuales es trabajo tecnico de Coordinator + Architect dentro
del paquete; no se pregunta al Owner por detalles que no cambian politica. Proponer una redaccion aqui no la vuelve vigente.

**Aprobacion parcial (RC-31).** Ninguna aprobacion parcial activa parte de la Proposal. Si Owner rechaza/modifica un OWN-x o
no aprueba integra la identidad de consenso: (1) registrar las decisiones individuales; (2) redactar version nueva coherente;
(3) Coordinator y Architect vuelven a acordar **esa** version exacta; (4) solicitar nuevamente aprobacion completa del Owner.
Por ejemplo rechazar tags exige rediseñar el registro durable y repetir consenso, no activar lifecycle sin P-20. No se
materializa por fragmentos mientras tanto. El mismo circuito rige cambios exigidos por dry-run o nueva opcion de politica.

**Aprobacion global.** P-01, P-07, diseño de P-21, P-22/23/24/26 y detalles sin decision especifica entran en la aprobacion
global; no estan aprobados por omision. Las decisiones adicionales pendientes se resuelven antes de activar. Si su resolucion
altera semantica del texto acordado (por ejemplo reversion), se sigue el circuito de version nueva; no se inserta una clausula
no revisada durante materializacion.

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
| 5 | Silencio entre Candidato y merge | WORKFLOW 4.5 | Re-fetch antes del merge; si main avanzo, P-12 ruta R y vuelta a 4.5.1 con Candidato solo antes del cierre nuevo | P-20, OWN-H |
| 6 | Criterio UI del CI sin ref explicita | AGENTS punto 1; WORKFLOW 4.5.6 para post-merge | Exigir ref de rama exacta junto con event/head_sha/job; excluir tag push | P-20/P-24, OWN-H y aprobacion global |

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
prohibidos por EA H.1 y esta propuesta no los contiene bajo ningun nombre (autocontrol de V1 §11 SC-01..SC-03 y de esta V3 §11).

## 10. Preguntas abiertas

| ID | Pregunta | Quien la cierra |
|---|---|---|
| U-01 | ¿`INITIATIVE_LIFECYCLE.md` separado por dominio o fundido en WORKFLOW? | Coordinator + Architect |
| U-02 | ¿`docs/FOUNDATIONS.md` o `docs/architecture/foundations.md`? | Coordinator + Architect (dentro de OWN-K) |
| U-03 | ¿El dispatch de cobertura del Candidato en la rama (4.5.2.bis) y el de `main` (4.5.7) sobre el mismo SHA medido son la misma evidencia, dado que la definicion del workflow puede diferir? | Architect; si cambia politica, Owner (OWN-I) |
| U-04 | ¿Basta la suite Core local al cierre de gate, o la clase Core-Windows exige mas? Se mide con P-22 | Evaluacion de la V2 |
| U-05 | ¿«1-3 sesiones» (WORKFLOW §2) debe sustituirse por los criterios de division de P-10? | Coordinator + Architect; Owner si cambia |
| U-06 | ¿La V2 se registra ademas como ADR (WORKFLOW §8: «ADR si es decision de fondo»)? | Owner |
| U-07 | Identificacion y registro de activacion | Diseño P-25: derivacion determinista y registro de activacion obligatorio bajo decision V1 expresa; aprobacion OWN-H/P pendiente |
| U-08 | ¿Toda NEW ARCHITECTURE debe exigir revision en sesion separada? | Owner, con evidencia P-22; I-56 final ya tiene obligacion particular por orden G4 |
| U-09 | ¿Podria una conformidad acotada al delta de un rebase sustituir a la completa en algun caso? Hoy P-19 exige la completa y cada rebase repite conformidad y evidencia del Candidato (coste sin cota; se mide en P-22 como Candidate attempts) | Architect; Owner si relaja |
| U-10 | ¿Migracion caso por caso de una iniciativa grandfathered a V2? | Owner, fuera de I-56 |
| U-11 | ¿Donde se registran de forma durable `MERGE_SHA`, CI post-merge y cobertura del Candidato? | **Resuelta como diseño propuesto**, no aprobada: tag anotado `integration/<unidad>` (P-20); OWN-H pendiente |
| U-12 | ¿«Aproximadamente cinco» iniciativas para la evaluacion general es la escala adecuada? | Coordinator + Architect (P-22; ya no depende de cubrir los tres arquetipos) |
| U-13 | ¿Como se nombra en HANDOFF la seccion real que sustituye a «§8-12»? | Coordinator (editorial) |
| U-14 | ¿Se poda la narrativa historica acumulada en HANDOFF, y con que regla compatible con «registro historico»? | Owner |
| U-15 | **Nueva en V2.** El push de un tag dispara una corrida de CI sobre el commit etiquetado (medido en `archive/*`): coste extra por integracion y posible confusion con la evidencia post-merge. P-20 la declara no-evidencia; evitarla exigiria filtrar tags en `ci.yml`, cambio de CI fuera de I-56 | Owner (en otra iniciativa, si se quiere) |
| U-16 | Proteccion de tags hoy ausente | Diseño P-20: crear/verificar antes de merge normativo si OWN-H aprueba; no accion en G4 |
| U-17 | ¿Conviene automatizar PRE/POST y comprobacion Claim-Id? | Coordinator + Architect; procedimiento manual P-25 ya definido, no depende de script futuro |
| U-18 | **Nueva en V2.** Una unidad V2 que consume un Freeze V1 sin revision del Architect registrada recibe como minimo la revision del Freeze de una EXTENSION independiente (P-02 punto 5); ¿basta para Freezes V1 de FOUNDATION EVOLUTION o NEW ARCHITECTURE? | Architect |

## 11. Autocontrol adversarial G4 (interno, sin consenso)

Esta seccion especifica las comprobaciones previas a publicar. No es revision final independiente, dry-run normativo ni
decision del Owner. El informe de sesion registra la ejecucion de la lectura adversarial; CI solo comprueba la publicacion,
no demuestra que el diseño de proceso sea correcto. Las fuentes son contrato, audit cerrado, V2 publicada, review V2, V1
vigente y orden G4; el borrador en cuarentena no es autoridad ni base de esta redaccion.

| Caso adversarial | Resultado que exige el texto reconciliado |
|---|---|
| Claim V1 en PRE, rebase posterior incluye efectivo | P-25 busca Claim-Id primero: sigue V1, no re-deriva por padres |
| Fetch viejo sin efectivo, push reclamo despues de activacion | No satisface registro push→fetch ni contraste: T6, STOP; nunca V1 por defecto |
| Solo POST / UUID ausente o duplicado | T6/OWN-Q; no inventar identidad ni orden |
| Unidad nueva nominal sin OWN-E | STOP; T8-a y T8-b siguen alternativas, no defaults |
| Rama I-56 borrada / merge de correccion | Derivacion por trailer y primer padre conserva efectivo original |
| Norma V1 no modificada por activacion cambia despues | Lee main actual, no todo ^1 |
| Hallazgo en seccion intacta / oraculo pasa con invariante roto | Re-revision tiene texto completo y REQUIRED (d); solo emisor rebaja con razon |
| Executor omite EXP / Discovery conceptual envejecido | Coordinator revisa negativos y ordena expansion; DC-1..6 actuales o diff vacio probado |
| Persistencia tiene lector lejano | DC-5 lo busca por campo/clave sin limite de saltos |
| Registro contradice fuente consumida; Executor lo llama B | A hasta prueba y ambas confirmaciones; no salida por limite de alcance |
| Poblacion inicial copia comportamiento distinto a Freeze integrado | Verificacion y conformidad por entrada; no normalizar contradiccion en registro |
| A-n o decision Owner verbal despues READY-04 | Commit de correccion, SHA nuevo y READY-02; no conformidad sobre semantica invisible |
| Hermana ve A-n solo en otra rama / A-3 duplicada | STOP por visibilidad/numeracion; no cherry-pick ni sobreescritura para seguir |
| Freeze commit cambia una clausula bajo trailer legitimo | READY-09 compara blob acordado y diff limitado a estado; falla |
| OV pasado en intermedio de SHA distinto | Se ejecuta otra vez en final; READY-08 verifica asignacion completa |
| Ultimo escenario se desasigna desde contrato mutable | No altera Freeze; retirar ultima asignacion exige Owner + A-n |
| main avanza con cierre ya publicado | Preservar/retirar cierre, rebasar producto, push Candidato solo, evidencia propia, cierre nuevo |
| Gate body dice «CI de este SHA verde» antes de existir | No es evidencia; orden P-10 exige reporte posterior y CI de punta |
| Push tag con event=push y mismo head_sha | ref=refs/tags excluida; no sustituye main ni rama |
| Tag ligero/corr10 malformado; corr9 valido | Detectar desviacion, no fallback silencioso; correccion completa numerica |
| Ruleset ausente | No afirmar proteccion; Owner debe autorizar/configurar/verificar antes de activacion |
| Politicas de seguridad incompatibles | No ordenar «mas estricta» sin contencion; STOP Owner, H.1 no renunciable localmente |
| Owner aprueba solo lifecycle, rechaza tags | Ninguna activacion parcial; nueva version, mismo-version consenso y nueva aprobacion |

**Alcance de G4:** solo `I-56-proposal-v3.md`; V1/V2/review/audit/contrato y normas compartidas quedan intactos. No se implementan
herramientas, CI ni producto. Se mantienen sin T0–T4, R0–R4, Quick CI, equivalencia por arbol ni merges automaticos. Las preguntas
U-01..18 y decisiones OWN pendientes no se resuelven por silencio; LOW diferidos estan individualizados en la matriz inicial.

```text
PROPOSAL V3 = PUBLISHED / NOT CONSENSUS (estado de entrega tras commit y push; antes, borrador G4)
COORDINATOR = REVIEW REQUIRED
FINAL INDEPENDENT ARCHITECT REVIEW = NOT OPEN
OWNER = NOT REQUESTED
DRY-RUN = NOT OPEN
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
