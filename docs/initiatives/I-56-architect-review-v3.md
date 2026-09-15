# I-56 G5 — Architect Review independiente de la Proposal V3

> ```text
> REVIEWED PROPOSAL       = docs/initiatives/I-56-proposal-v3.md
> PROPOSAL_V3_SHA         = 4b04b23f8661d0d8a2a656985339c48796861966
> PROPOSAL_V3_BLOB        = 6ad956c0234d0db6fbbe9e0fb877054301acd351
> Coordinator on exact V3 = AGREED
> Review mode: SEPARATE SESSION
> Author of reviewed text = this reviewer: NO
> ```

## AGREED POINTS

1. **Identidad y vigencia.** P-25 da un solo significado a `WORKFLOW_V2_EFFECTIVE_SHA`: el primer merge `--no-ff` de primer padre cuyo segundo padre alcanza el unico commit con trailer `Workflow-V2-Normative: I-56` y cuyo primer padre aun no lo alcanza. La creacion local, el CI, el tag, la limpieza y los merges de correccion no crean otra vigencia. La derivacion sobre commits retenidos en `main` sigue funcionando tras borrar la rama.
2. **Transicion estable.** PRE y POST son snapshots completos y durables; PRE vive en el cuerpo del merge normativo y POST en el registro de activacion. La clasificacion se ata a `Claim-Id` y al primer push aceptado, no a la ascendencia posterior. Una rama presente solo en POST, una prueba temporal incompleta o una contradiccion cae en T6; nunca en V1 por omision. Un rebase no reclasifica.
3. **Carrera de activacion.** La ventana no atomica entre PRE y el push de `main` falla cerrado: un reclamo que aparece solo en POST se trata como T6. La prueba de anterioridad posterior al PRE exige push aceptado, fetch posterior y contraste con Actions; la ausencia de cualquiera de esos datos no acredita anterioridad.
4. **Grandfathering nominal.** T8-a y T8-b son alternativas reales y mutuamente distinguibles. Ninguna es default; hasta OWN-E una unidad nueva de I-49, I-52 o I-55 queda en STOP. Ninguna opcion reescribe contratos activos ni transfiere evidencia. La lectura en `WORKFLOW_V2_EFFECTIVE_SHA^1` se limita a las clausulas que cambie el merge normativo; el resto se lee de `main` actual.
5. **Autoridad V1 de la activacion.** La pausa, el registro de activacion y la configuracion administrativa previa se fundan en AUTOMATION_PLAN §11 y WORKFLOW §2. El inicio se versiona antes de 4.5.1 y el fin queda durable. Reversion y tratamiento de reclamos intermedios permanecen correctamente reservados a OWN-S.
6. **Discovery actual.** Las unidades posteriores re-verifican DC-1..DC-6 o demuestran estabilidad con inventario y diff vacio; DC-7..DC-9 siempre se rehacen. DC-5 incluye lectores de campos, estructuras persistidas y artefactos sin limite de saltos. EXP-09 resuelve materialidad UNKNOWN y la revision negativa del Coordinator puede ordenar expansiones omitidas.
7. **Fundaciones fail-closed.** Una discrepancia A detiene Discovery, gate y conformidad. B exige demostracion y confirmacion de Coordinator y Architect en todo arquetipo. La poblacion inicial exige fuente aceptada o Freeze integrado, codigo, pruebas y conformidad por entrada; `FOUNDATIONS` no prevalece sobre su fuente. El desacuerdo sobre quien debe aprobar su contenido factual se trata en AR-V3-01.
8. **Revision arquitectonica sin blindaje por delta.** Toda ronda recibe la version completa; el delta es foco minimo y el Architect decide interacciones y puede abrir hallazgos en secciones intactas. REQUERIDO cubre contratos incorrectos, inejecutables y no verificables, oraculos ciegos y gates imposibles. Solo el emisor puede rebajar un hallazgo y la poda del Coordinator necesita conformidad del Architect.
9. **Freeze reconstruible.** La identidad acordada se registra como commit, ruta y blob. El commit de Freeze solo cambia las lineas de estado enumeradas, lleva un trailer unico por unidad/artefacto y es la ultima modificacion de la ruta. READY-09 toma la ruta del trailer, comprueba el diff permitido, la unicidad alcanzable, los Freeze V1 consumidos y el conjunto completo de A-n.
10. **Enmiendas.** A-n es append-only, continuo por flujo, con `Applies-to`, fuente, delta, M-01..M-08 y autoridades. Las enmiendas conceptuales deben ser visibles desde `origin/main` para una hermana; la autora puede consumir la propia. Toda enmienda, desviacion, decision o cambio OV posterior a READY-04 crea un SHA nuevo y reinicia READY-02. Ningun Candidato depende de semantica verbal o sin versionar.
11. **Owner Validation.** La matriz congelada es aditiva respecto de la guia. Cada escenario aplicable se ejecuta en `FINAL_CANDIDATE_SHA`; un hito intermedio solo agrega evidencia y solo se reutiliza si conserva literalmente el mismo SHA y todos los invalidadores. La asignacion escenario→unidad vive en Freeze/delta/A-n, READY-08 comprueba cobertura y retirar la ultima asignacion requiere Owner.
12. **Gates y Candidato.** Los gates son resultados de comportamiento independientemente utiles, con RED→GREEN real, al menos una prueba seleccionada y CI propio leido antes del siguiente gate. El Core Full pasa del push interno al cierre de gate sin crear T0–T4, R0–R4 ni Quick CI. El Candidato conserva Core/UI Full locales, builds Debug, CI exacto y Owner Validation cuando aplica.
13. **Recuperacion tras cierre obsoleto.** La ruta R preserva la ronda vieja, devuelve la rama al tip de producto, rebasa sin el cierre, empuja el Candidato solo, repite READY/conformidad/Full/builds/CI/OV y solo entonces recrea y empuja el cierre. El CI del cierre no acredita al padre. Conviene mecanizar la preservacion exacta, pero la secuencia normativa es segura.
14. **Post-merge y tags.** `MERGE_SHA` es el merge de la ronda verificada y las rondas previas quedan en `Unverified merges`. La evidencia exige `event + ref + head_sha`; `refs/tags/*` no acredita rama ni `main`. El formato, regex anclada, tipo anotado, destino, padres, claves, cadena `Corrects` y orden numerico hacen detectables tags ausentes o malformados. La proteccion es prerequisito futuro, no hecho presente: la consulta actual sigue devolviendo cero rulesets. Su creacion necesita OWN-H.
15. **Autoridad documental.** WORKFLOW conserva Git/integracion; AGENTS, evidencia; INITIATIVE_LIFECYCLE, diseño/revision; la guia, procedimiento OV; Freeze/A-n, alcance congelado. `FOUNDATIONS` es descriptivo y `PROMPT_TEMPLATES` subordinado. Una excepcion local no crea precedente y no puede renunciar H.1. El lifecycle materializado usara punteros para no duplicar WORKFLOW.
16. **RC-01..RC-31.** La reconciliacion funciona como sistema en RC-01..RC-15 y RC-17..RC-31. RC-16 queda tecnicamente resuelta por la verificacion fuente+codigo y la conformidad por entrada; su acoplamiento posterior a OWN-R introduce el problema de autoridad y secuencia de AR-V3-01.
17. **RC-32..RC-40.** RC-33 y RC-39 estan corregidas. RC-32 y RC-34..RC-38/40 permanecen LOW de forma explicita y fail-closed donde importa: referencia o dato no recuperable no acredita exito, una B no se autodeclara y V1 sigue gobernando las operaciones no redefinidas. No se escalan solo por seguir diferidas.

## DISAGREEMENTS

### AR-V3-01

- **Severity:** MEDIUM
- **Proposal V3 section / P-id:** P-06, «Poblacion inicial, verificacion por entrada»; §6 OWN-K y OWN-R; P-25, «Precondiciones y unidad de aprobacion».
- **Exact problem:** V3 define `FOUNDATIONS` como metadata descriptiva y somete cada entrada inicial a verificacion de fuente + codigo y conformidad de Coordinator + Architect. Sin embargo, OWN-R reserva al Owner el «contenido de poblacion inicial», mientras OWN-K ya cubre la decision de crear y ubicar el registro. Esto asigna al Owner autoridad sobre hechos descriptivos y deja una secuencia ambigua: el paquete completo del Owner se aprueba antes de abrir los gates normativos, pero la poblacion que OWN-R dice aprobar se produce y verifica dentro de esos gates. Si OWN-R significa aprobar una poblacion aun inexistente, no es una decision concreta; si significa aprobarla despues, contradice la unidad y orden de aprobacion descritos por §6/P-25.
- **Concrete failure scenario:** Coordinator y Architect verifican una entrada contra un ADR aceptado, los simbolos y las pruebas. El Owner discrepa con la redaccion factual o no emite un veredicto separado sobre esa entrada. Un executor puede (a) tratar la opinion del Owner como autoridad que cambia una descripcion contra fuente/codigo, violando P-17; (b) considerar incompleto el paquete y bloquear indefinidamente la activacion; o (c) aplicar la regla de aprobacion parcial y exigir V4/reconsenso por una correccion factual que no cambia la politica aprobada.
- **Required change:** retirar el contenido factual de la poblacion de OWN-R. OWN-K puede reservar al Owner la creacion, ubicacion y alcance del registro. La aceptacion de cada entrada debe ser una comprobacion tecnica de Coordinator + Architect contra fuente, codigo y pruebas, con discrepancia A = STOP. Si el Owner desea cambiar lo que debe ser, debe hacerlo mediante la autoridad normativa correspondiente —ADR, alcance o decision de politica—, no aprobando metadata. Eliminar OWN-R o redefinirlo solo como una decision de politica concreta que no duplique OWN-K ni convierta hechos en autoridad; aclarar la secuencia de materializacion.

### AR-V3-02

- **Severity:** LOW
- **Proposal V3 section / P-id:** §6 OWN-I y «Aprobacion parcial (RC-31)»; P-13.
- **Exact problem:** OWN-I dice expresamente «Cobertura sin cambio» y «no decision tecnica nueva», pero aparece dentro de la tabla de decisiones de politica, queda PENDIENTE y entra literalmente en la regla «si Owner rechaza/modifica un OWN-x». Un item informativo se convierte asi en una aprobacion separada capaz de invalidar el paquete completo.
- **Concrete failure scenario:** el Owner aprueba todas las decisiones de cambio y no responde de forma individual a OWN-I porque no hay nada que decidir. La lectura literal deja la aprobacion incompleta; o una aclaracion editorial de la descripcion de cobertura obliga innecesariamente a V4 y reconsenso.
- **Required change:** retirar OWN-I de la lista de decisiones o marcarlo fuera de la semantica de aprobacion parcial como declaracion informativa `[KEEP]`. La cadencia de cobertura permanece sin cambio y no necesita un veredicto independiente.

## MATERIAL RISKS

1. **Controles manuales hasta P-21.** READY-09, la auditoria de tags, PRE/POST y varios preflights dependen de ejecucion humana exacta. La semantica falla cerrado, pero un error operativo puede producir STOP o retrabajo hasta que existan las herramientas futuras.
2. **Ruleset inexistente.** El repositorio sigue con `[]` rulesets. OWN-H y la comprobacion previa al merge normativo son prerequisitos reales; sin ellos P-20 detiene la publicacion del tag y la V2 no debe activarse parcialmente.
3. **Churn de `main`.** La ruta R es segura, pero un trunk que avance repetidamente puede forzar varias rondas completas de Candidato, conformidad y OV. No es perdida de evidencia; es riesgo de livelock y costo.
4. **Revision separada general pendiente.** La revision final de I-56 cumple `SEPARATE SESSION`; U-08 deja sin decidir si NEW ARCHITECTURE futura debe exigir siempre ese modo. La independencia historica de la muestra sigue UNKNOWN.
5. **Obsolescencia del registro.** DC-8 y clase A protegen al consumidor, pero RC-34 mantiene LOW la restriccion que impide al consumidor corregir `Known limitations`. Una discrepancia puede permanecer visible en varios cierres hasta que una iniciativa introductora/extensora actualice la entrada.
6. **Reversion sin politica aun.** OWN-S debe resolverse antes de activar. V3 acierta al no improvisar rollback, pero hasta esa decision no existe tratamiento aprobado para reclamos entre activacion y desactivacion.

## REQUIRED CHANGES

| # | Disagreement | Cambio requerido para V4 |
|---|---|---|
| RC-V3-01 | AR-V3-01 | Mantener `FOUNDATIONS` descriptivo: quitar la aprobacion factual del Owner, dejar creacion/ubicacion/alcance en OWN-K y hacer de cada entrada una conformidad tecnica fuente+codigo+pruebas; aclarar el orden de materializacion |
| RC-V3-02 | AR-V3-02 | Sacar la cobertura sin cambio de la lista de decisiones o excluirla expresamente de la aprobacion parcial |

No hace falta rediseñar P-01..P-25. Los dos cambios son locales a P-06/P-13, §6 y sus referencias.

## OPTIONAL IMPROVEMENTS

1. En la materializacion de la ruta R, dar nombre canonico al tag `archive/*` de la ronda retirada y verificar que el objeto remoto pelado sea el `CLOSURE_SHA` viejo antes de mover la rama. La secuencia actual ya exige preservacion recuperable; esto reduce variacion operativa.
2. Presentar OWN-H en dos bloques dentro del paquete: decision/autorizacion administrativa del Owner y comprobaciones tecnicas ya acordadas. No cambia el contenido de V3, pero evita que regex, comandos y campos parezcan opciones de producto.
3. Marcar cada LOW RC-32/34..38/40 con un destino concreto de deuda al materializar, sin promoverlo por ello a gate de activacion.
4. Cuando P-21 se implemente en otra iniciativa, probar con tags malformados, `corr10` vs `corr2`, tag ligero, destino fuera del primer padre, Claim-Id duplicado y ausencia de `ref` en los datos de CI.

## OWNER DECISIONS REVIEW

| ID | Clasificacion | Evaluacion |
|---|---|---|
| OWN-A | Correctamente Owner-reserved | Cambia la politica de arquetipos y su efecto; no toca evidencia |
| OWN-B | Correctamente Owner-reserved | Fija participacion, anti-churn, Freeze y conformidad |
| OWN-C | Correctamente Owner-reserved | Cambia la cadencia Core escrita en AGENTS; conserva Full final |
| OWN-D | Correctamente Owner-reserved | Fija READY y el uso limitado de Candidatos intermedios |
| OWN-E | Correctamente Owner-reserved | T8-a/T8-b son opciones explicitas; alcance y cierre nominal requieren Owner |
| OWN-F | Correctamente Owner-reserved | La obligacion global de referencia y la superficie de orden son politica; la redaccion mecanica sigue tecnica |
| OWN-G | Correctamente Owner-reserved | Retira ceremonia y cambia el default de expansiones, sin volver a votar OWN-B/C |
| OWN-H | Correctamente Owner-reserved, con componentes tecnicos | Namespace, durabilidad, ruleset y accion administrativa requieren Owner; re-fetch, regex y verificaciones son tecnicas y V3 ya dice que no se votan por separado |
| OWN-I | Tecnica; no debe requerir decision separada | Declara ausencia de cambio de cobertura; AR-V3-02 |
| OWN-J | Correctamente Owner-reserved | Agrupacion/unidades es politica de proceso, con evidencia propia intacta |
| OWN-K | Correctamente Owner-reserved | Tres superficies, cadencia y autorizacion para crear/ubicar el registro son politica documental; no debe absorber aprobacion factual de entradas |
| OWN-L | Correctamente Owner-reserved | Fija autoridad por dominio, alcance de excepciones y subordinacion |
| OWN-M | Correctamente Owner-reserved | Matriz OV aditiva, asignacion congelada y SHA final gobiernan validacion del Owner |
| OWN-N | Correctamente Owner-reserved | Solo el Owner acepta el ADR de Workflow V2; debe existir antes de implementar la norma |
| OWN-O | Correctamente Owner-reserved | Es la autorizacion expresa que exige el contrato para editar AGENTS.md |
| OWN-P | Correctamente Owner-reserved | Obligatoriedad, excepciones, inicio/fin y salida de bloqueo de la pausa son politica transversal V1 |
| OWN-Q | Correctamente Owner-reserved en su parte de politica/caso ambiguo | No hay default para identidad legacy; la recoleccion y contraste de evidencia siguen siendo tecnicos |
| OWN-R | Tecnica; no debe requerir decision separada | El contenido de metadata descriptiva se verifica, no se convierte en autoridad del Owner; AR-V3-01 |
| OWN-S | Correctamente Owner-reserved | Reversion/desactivacion y reclamos intermedios cambian la politica de transicion |

No falta una materia de Owner de las enumeradas por la revision V2: ADR, autorizacion de AGENTS, pausa, legacy, ruleset y rollback estan presentes. OWN-H es amplio pero distingue autorizacion de mecanica. La regla de aprobacion parcial es correcta: ninguna seleccion de decisiones activa una fraccion de V3; despues de retirar los dos no-decision items, todo rechazo o cambio de politica exige una version coherente nueva y reconsenso.

## HISTORICAL CAPTURE PRESERVATION

| Captura | Mecanismo V3 | Evaluacion |
|---|---|---|
| B-01..B-05 | DC-1..DC-6 con codigo; DC-5 sin limite de saltos; delta actual; DC-9/EXP | **PRESERVADA.** GUID/restamp, lectores persistidos, autoridades, mutacion y codigo obsoleto se investigan antes del Freeze |
| B-06 | Revision negativa obligatoria del Coordinator sobre DC/EXP, con facultad de ordenar expansion | **PRESERVADA.** No depende de que el Executor detecte su propia omision |
| B-07 | P-08 REQUERIDO (d): oraculo que pasa con invariante roto | **PRESERVADA Y EXPLICITADA.** No puede archivarse como detalle u OPTIONAL |
| B-08/B-09 | Version completa en toda re-revision; secciones intactas revisables; REQUIRED por comportamiento/materialidad | **PRESERVADA.** El delta no blinda `RepairBroken` ni las consecuencias tardias de `RemoveAll` |
| B-10/B-11/B-40 | Coordinator revisa cada gate funcional antes del siguiente; gates por resultado | **PRESERVADA.** Mantiene el control que encontro gramatica ampliada, perdida de propiedades y autoridad/consentimiento incorrectos |
| B-12/B-17 | Push del cierre como punta propia; CI leido antes del siguiente gate; READY-05 event/ref/head/jobs | **PRESERVADA Y ADELANTADA.** El cuelgue Windows y CRLF Linux no esperan al Candidato final |
| B-15 | P-08 REQUERIDO (d) incluye plan de gates imposible/inconsistente | **PRESERVADA Y EXPLICITADA.** No se trata como detalle |
| B-16/B-18/B-41 | RED→GREEN focal real, conteo no cero, invariante nombrado y oraculo no ciego | **PRESERVADA.** Mantiene la observacion del defecto antes del verde |
| B-20/B-22 | Re-revision con version completa + delta obligatorio; hallazgo material en seccion intacta abre ronda | **PRESERVADA.** Consecuencias de reconciliacion y defectos introducidos por la version previa siguen visibles |
| B-26/B-27/B-30/B-31 | Conformidad final Architect + Coordinator en todo arquetipo; dueño unico por dominio; desviacion/decision versionada y READY-02 | **PRESERVADA Y REUBICADA.** Contradicciones normativas, huecos de prueba y prosa divergente no pasan por aceptacion verbal o cierre tardio |
| B-33 | READY-04 fetch/rebase y ruta R si ya existe cierre | **PRESERVADA.** Una base superada invalida el intento y toda evidencia se repite sobre el SHA nuevo |
| Defectos del Owner del corpus I-45 | Matriz OV aditiva al checklist vigente, asignacion congelada, todos los escenarios aplicables sobre FINAL_CANDIDATE_SHA, DLL trazable | **PRESERVADA.** V3 no reduce la validacion manual ni permite trasladarla desde otro SHA |

No se identifica una captura obligatoria demorada hasta despues del punto en que historicamente fue util. El unico desacuerdo relacionado con `FOUNDATIONS` no elimina DC-8, clase A ni conformidad; corrige quien tiene autoridad para aceptar la descripcion.

## CONSENSUS STATUS

```text
ARCHITECT = CHANGES REQUIRED — PROPOSAL V4

Coordinator on exact V3       = AGREED
Architect on exact V3         = CHANGES REQUIRED — PROPOSAL V4
Consensus                     = NOT REACHED
Owner                         = NOT REQUESTED
Dry-run                       = NOT COMPLETE
Workflow V2                   = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA     = DOES NOT EXIST
```

Resultado: 0 BLOCKER, 0 HIGH, 1 MEDIUM y 1 LOW. Los dos cambios requeridos son locales y no necesitan decision del Owner antes del consenso. V3 no se activa por el acuerdo del Coordinator ni por esta revision; I-56 sigue gobernada por Workflow V1.
