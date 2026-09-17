# I-56 G3 — Architect Review de la Proposal V2

> ```text
> REVIEWED PROPOSAL    = docs/initiatives/I-56-proposal-v2.md
> PROPOSAL_V2_SHA      = fde03863f42b805d5503dbd0d0ece3a7dceb4678   (blob del archivo ab755c38d6c4b14f13a5386cf2a49a5383a14b8f)
> Coordinator on V2    = AGREED
> ARCHITECT            = CHANGES REQUIRED — PROPOSAL V3
> Consensus            = NOT REACHED
> Owner                = NOT REQUESTED
> Workflow V2          = NOT EFFECTIVE
> WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
> I-56 remains governed by Workflow V1.
> ```

**Modo de revision y limites (declarado, P-08 de la propia propuesta).** La orden pide un Architect independiente. Esta sesion es la
misma que redacto la Proposal V1 y la V2, asi que **no puede afirmar independencia**. Para acercarse a ella, la revision adversarial
se ejecuto en **tres subagentes de solo lectura con contexto propio**, a los que solo se dio el texto de la propuesta, las fuentes y
el guion de revision de la orden (sin el razonamiento del redactor); sus hallazgos se verificaron contra las fuentes antes de
consolidarlos aqui, y ninguno se descarto por conveniencia del redactor. Registro:

```text
Review mode: SAME-SESSION ROLE (sesion redactora) + subagentes de contexto propio lanzados desde ella; NO es SEPARATE SESSION ni EXTERNAL HUMAN
Author of reviewed text = session that consolidated this review: YES
```

Verificaciones directas hechas al consolidar: repositorio **publico** y **sin rulesets** (`gh api .../rulesets` → `[]`); los pushes de
tags `archive/*` generaron corridas `event=push` con `head_sha` del commit etiquetado (p. ej. `archive/i-49-a2-pre-rebase-5a3714f`);
AGENTS «Pruebas» (criterio de evidencia de UI del CI) no incluye `ref`; WORKFLOW §4 limita el bootstrap al caso (d); la tabla de
dominios de P-17 coloca el registro de fundaciones por encima del «contrato congelado». Lineas citadas = archivo de la Proposal V2 en
`fde0386`.

La revision no rediseña la V2: todos los cambios pedidos son locales a las decisiones existentes.

---

## AGREED POINTS

1. **Arquetipos como forma de proceso, no niveles de riesgo** (P-03 L330-344): no gobiernan CI, suites, READY, evidencia del Candidato,
   conformidad, Owner Validation ni exact-SHA; incluso EXTENSION mantiene revision del Freeze por el Architect y conformidad del
   Architect. No hay T0–T4, R0–R4 ni Quick CI bajo ningun nombre; `ci.yml` y su poblacion no cambian (P-11, P-13).
2. **UNKNOWN = activado y arquetipo solo hacia arriba tras el Freeze** (L356-362); rechazo de un arquetipo «trivial» fundado en B-01..B-03.
3. **Agrupacion conjuntiva** (mismo problema de usuario **y** fundacion de diseño compartida; infraestructura sola no agrupa) y
   re-prueba coherente con I-53, I-50, I-50×I-51, I-47×I-54 (P-02 L224-241, L307-314).
4. **Version de workflow por reclamo atomico propio, sin herencia ni opt-in/opt-out** (L262-270); una unidad V2 nunca reescribe un
   Freeze V1 (P-02 pasos 1-6; P-09).
5. **Un unico significado de `WORKFLOW_V2_EFFECTIVE_SHA`** (merge normativo = punto de vigencia); la pausa, la verificacion, el tag y un
   merge de correccion no crean otro momento (P-25; verificado en todas las apariciones). T6 nunca asume V1; T3 y T5 fallan cerrado.
6. **Verificacion en codigo de fundaciones consumidas** aunque exista registro (DC-8), y clase A = STOP en salida de Discovery, cierre de
   gate, conformidad y regla 7 de P-17; duda A/B = A; B no autodeclarable.
7. **REQUERIDO por materialidad y no por etiqueta de severidad; estados de consenso sin «AGREED WITH CHANGES»; errata confirmada antes
   del Freeze; Review mode registrado sin afirmar independencia** (P-08).
8. **Revision del Coordinator por gate y conformidad Architect + Coordinator en todos los arquetipos**; RED→GREEN focal con conteo;
   CI del cierre leido antes del siguiente gate; diagnostico antes de gate de correccion (P-10, P-19).
9. **Cambio moderado de la cadencia del Core** con H3 fuerte no adoptada y riesgo residual declarado; Candidato Full intacto (P-11, P-12).
10. **Orden READY correcto** (rebase → CI → conformidad → evidencia) y ninguna reutilizacion por range-diff, patch-id o igualdad de
    arbol (P-12, P-19, P-20); CI post-merge nunca sustituido por el del cierre.
11. **Owner Validation solo se amplia**: matriz OV aditiva al checklist de la guia; comprobacion del DLL construido (P-14).
12. **Tres superficies documentales + tag de integracion** como modelo correcto y minimo en su concepto (P-15, P-20); tag anotado sin
    recursion, con precedente `archive/*`; nombres exactos y orden numerico para `-corr<N>`.
13. **Ordenes por referencia que nombran cada regla ejercida** (respuesta a la contraevidencia de la celda ROADMAP) y plantillas A, B y F
    con listas STOP explicitas (O-1, O-2, O-4, O-6, O-7, O-8, O-9 prevenidos o mejorados).
14. **Sin ORCHESTRATION.md; prompt-architecture consolidada con las plantillas** (P-24).
15. **Capturas historicas preservadas en lo esencial** (ver MATERIAL RISKS §R.1): Coordinator por gate (B-10, B-11, B-40), RED focal
    (B-16, B-18), CI push (B-12, B-17), conformidad (B-26, B-27, B-30, B-31), exact-SHA post-merge (C-05, B-25), invalidacion del
    Candidato (B-33, Q8/Q9), Discovery (B-01, B-03, B-04), primera revision (B-08, B-19, B-21). Las parciales se tratan abajo.

---

## DISAGREEMENTS

### Transicion y `WORKFLOW_V2_EFFECTIVE_SHA`

**AR-01 — HIGH — P-25 escalera de evidencia de orden (L1426-1442)**
- *Problema*: el peldaño 2 («punta registrada tras aceptar el reclamo sin el SHA ⇒ anterior») es autodeclarado; nada prueba que el fetch
  registrado sea **posterior** a la aceptacion del push. WORKFLOW §4.1 ya pide un fetch **antes** del push, que es la punta que la sesion
  tiene a mano. El peldaño 3 (Actions) solo se consulta en T6, nunca para contrastar un «anterior».
- *Escenario*: t−10 s fetch (sin SHA); t−3 s se acepta el push del merge normativo; t0 se acepta el reclamo (rama nueva, sin conflicto); el
  bootstrap registra la punta de t−10 s ⇒ T1/T2 = V1, cuando es T5 = V2. Rompe «reclamo tras el SHA = V2» (CR-01).
- *Cambio requerido*: un «anterior» por el peldaño 2 solo vale si (a) la salida del push aceptado y el fetch posterior constan en el mismo
  registro de comandos en el cuerpo del commit, en ese orden, y (b) la corrida `push` del reclamo no es posterior a la del merge
  normativo; cualquier incoherencia ⇒ T6.

**AR-02 — HIGH — P-25 frente a reclamos V1 existentes y WORKFLOW §2 casos (a)-(c) (L1431, L1438-1441)**
- *Problema*: el peldaño 2 vive en un «bootstrap», que en V1 solo existe en el caso (d); los contratos V1 no tienen campo `workflow`; y
  una herramienta que re-derive por ascendencia tras un rebase rutinario vera que el nuevo padre del commit de reclamo contiene el SHA.
- *Escenarios*: (1) toda iniciativa reclamada dias antes del SHA sin punta registrada cae en T6 y exige decision del Owner; (2) una
  iniciativa grandfathered rebasada se clasifica V2 por `initiative-preflight`/`candidate-check`, contra el contrato I-56 §0.1.
- *Cambio requerido*: la sesion de integracion de I-56 registra en el cuerpo del merge normativo (V1 admite hashes en cuerpos de commit)
  una instantanea `git ls-remote` de todas las ramas de iniciativa remotas **justo antes** del push de `main`, con commit de reclamo y
  `Claim-Id`, y otra **justo despues** (informe post-merge). Rama presente en la primera ⇒ V1 definitivo; solo en la segunda ⇒ T6;
  creada despues ⇒ escalera normal. La clasificacion se identifica por `Claim-Id`, nunca por ascendencia tras un rebase.

**AR-03 — HIGH — T8, P-02 (L268-270) y OWN-E frente al contrato I-56 §0.1**
- *Problema*: «I-49, I-52 and I-55 are grandfathered» es redundante con «any initiative claimed before» salvo que los nombres cubran mas que
  los reclamos existentes. T8 elige precisamente la lectura que vuelve redundante la frase nominal y la presenta como fila por defecto
  («Workflow = V2») que el Owner solo «confirma», sin alternativa definida.
- *Escenario*: I-49 abre una unidad nueva tras el SHA; el Coordinator aplica T8 = V2 antes de que OWN-E exista: V2 aplicada a una linea
  nombrada como grandfathered.
- *Cambio requerido*: T8 como **dos opciones** (T8-a: V2 con archivos propios; T8-b: V1 hasta que se cierre la linea nombrada), con sus
  consecuencias; hasta decidir OWN-E, una unidad nueva de I-49/I-52/I-55 = **STOP**.

**AR-04 — MEDIUM — P-25 «una iniciativa V1 lee sus normas en `WORKFLOW_V2_EFFECTIVE_SHA^1`» (L1448, L1458)**
- *Problema*: AGENTS y la tabla de archivos calientes de WORKFLOW §7 siguen cambiando despues del SHA por ediciones V1 y ADR aceptados;
  congelar todo en `^1` hace que las iniciativas V1 ignoren cambios que no son de la V2. Ademas la tabla de transicion vive en el WORKFLOW
  V2, que la iniciativa V1 no leeria.
- *Cambio requerido*: `^1` aplica solo a las clausulas modificadas por el diff del merge normativo en los archivos de P-24; lo demas, incluida
  la seccion de transicion, se lee de `main` actual.

**AR-05 — MEDIUM — Pausa y tag `integration/I-56` fundados en conceptos V2 no vigentes (L1409-1424)**
- *Problema*: «alcance local (P-17)», «formato de P-20» y la precondicion de proteccion son normas V2, e I-56 es V1; una pausa que limita
  los reclamos de todos no es «local» segun P-17 regla 2. Si la decision se commitea tras el cierre, cambia el SHA de cierre; entre
  verificacion verde y borrado de la rama, el preflight seguiria leyendo la pausa.
- *Cambio requerido*: fundar ambas en la autoridad V1 del Owner (AUTOMATION_PLAN §11; WORKFLOW §2), con alcance «todos los reclamos de
  la ventana»; registrar la pausa antes de 4.5.1; registrar su fin de forma durable (no solo en un tag opcional); el preflight se guia por
  «fin registrado», no por la existencia de la rama.

**AR-06 — MEDIUM — El registro durable del SHA efectivo depende de un tag opcional (L1405, L1409)**
- *Problema*: «derivable de `git log --first-parent main` y la rama de I-56» deja de ser determinista cuando la rama se borra, y mas con
  merges de correccion; si el Owner no decide el tag, el SHA mas importante del proceso no tiene registro durable.
- *Cambio requerido*: regla determinista (primer merge de primer padre en `main` cuya historia de segundo padre contiene el commit
  normativo identificado por un trailer propio) **o** registro durable obligatorio (no opcional).

### Discovery, revision del Architect y registro de fundaciones

**AR-07 — HIGH — P-08 regla anti-churn 5, re-revision acotada al delta (L602-603; plantilla C L1098)**
- *Problema*: excluye secciones sin cambio «salvo interaccion demostrada», sin decir quien la demuestra ni que pasa con un defecto
  material hallado en una seccion sin cambio; la regla 3 solo abre ronda con un REQUERIDO abierto, asi que ese defecto puede quedar «fuera
  de alcance». No se exige entregar al revisor la version completa.
- *Escenario*: ronda 2 de una FE; la reconciliacion cambia la composicion de una precondicion (patron I-48 AR4–AR7) y contradice en
  silencio una clausula de semantica de fallo intacta (patron B-27); el autor no marca la interaccion; AGREED; se descubre en conformidad o
  en codigo.
- *Cambio requerido*: el revisor recibe la version completa mas el delta; **el revisor** decide la interaccion; todo hallazgo material en
  cualquier seccion se registra y es REQUERIDO si cumple la regla 2. El delta acota lo que el revisor **debe** comprobar, no lo que **puede**
  reportar.

**AR-08 — HIGH — P-08 regla 2, definicion de REQUERIDO (L593-598), y reglas 6-7**
- *Problema*: los tres supuestos se leen como exhaustivos y dejan fuera clases capturadas: B-15 (plan de gates no ejecutable, elemento
  congelable) y B-07 (oraculo de prueba ciego a hardcodes, BLOCKER) puede archivarse como «detallar mas una prueba ya obligada» = OPCIONAL.
  No hay control sobre quien rebaja un REQUERIDO, y la regla 6 permite al Coordinator solo «podar» un contrato y retirar REQUERIDOS.
- *Escenario*: un oraculo que queda en verde con el invariante roto (clase B-07/B-41) se archiva como errata; AGREED; el defecto solo
  aparece si un RED lo expone.
- *Cambio requerido*: añadir (d) REQUERIDO si vuelve un elemento congelable incorrecto, no ejecutable o no verificable (incluye un oraculo
  que puede pasar con el invariante violado y un plan de gates con dependencias insatisfacibles); solo quien emitio el hallazgo puede
  rebajarlo, con registro; la poda de la regla 6 exige conformidad del Architect.

**AR-09 — HIGH — P-02 Discovery delta por unidad (L272-275)**
- *Problema*: la unidad posterior solo re-verifica DC-7, DC-8 y DC-9; no DC-1..DC-6 de su propia superficie, aunque la base pueda haber
  cambiado mucho (57 archivos entre E1 y E3, EA C-12). DC-9 necesita evidencia de autoridad, persistencia y mutacion que el delta no reune.
- *Escenario*: otra iniciativa cambia en `main` el camino de mutacion del configurador del Dinamico (clase B-05) antes del reclamo de E3; no
  es entrada del registro, DC-8 no lo ve, DC-9 se rellena con el DC-4 conceptual obsoleto.
- *Cambio requerido*: el delta re-verifica tambien DC-1..DC-6 de la superficie de la unidad sobre la base actual; puede citar el Discovery
  conceptual como «sin cambio» solo si el diff desde la base de ese Discovery sobre los simbolos y rutas citados es vacio.

**AR-10 — HIGH — P-05 salida del Discovery y autorizacion de expansiones (L440-455); tabla de P-08 (L576)**
- *Problema*: solo el Executor pide expansiones; la salida solo exige que el Coordinator confirme el arquetipo. Nadie comprueba que
  disparadores EXP que debian activarse se activaran. La etapa que capturo B-06 (revision del Coordinator del Discovery) no tiene sustituto
  explicito: P-08 L576 la cita pero solo añade revision por gate.
- *Escenario*: el Discovery registra un hardcode sin su consecuencia (I-48 G1), no pide expansion, el Coordinator confirma arquetipo; el
  defecto de dato silencioso (M-04) llega a diseño o, en EXTENSION, a codigo.
- *Cambio requerido*: el Discovery Core registra la evaluacion negativa de cada EXP-01..EXP-08 («no activado porque DC-n …»); la salida
  exige revision del Coordinator del Discovery contra DC-1..DC-9 y esas evaluaciones, con facultad de ordenar expansiones (Architect en
  FE/NA); corregir L576 para nombrar revision de Discovery y por gate.

**AR-11 — MEDIUM — P-06 regla 5 B (L513-514) frente a P-03 (L330-344)**
- *Problema*: aplazar una discrepancia es una decision protectora, y la confirmacion del Architect solo existe en FE/NA: el arquetipo la
  gobierna, contra «todo lo que protege el producto es identico en los tres».
- *Cambio requerido*: clase B confirmada por Coordinator **y** Architect en todos los arquetipos (puede hacerse dentro de la revision del
  Freeze o de la conformidad que el Architect ya hace).

**AR-12 — MEDIUM — Rango del registro de fundaciones en P-17 (L1009) frente a P-06 regla 5 (L502-503); terminologia (L1009-1013)**
- *Problema*: el registro «descriptivo» queda por encima del Freeze en el dominio de arquitectura, y M-08 dispara por contradecir una
  entrada como si fuera un ADR; las entradas se copian en el cierre sin aceptacion del Owner. P-06 dice que el ADR o el Freeze gobiernan.
  Ademas P-17 sigue diciendo «contrato congelado» tras CR-03.
- *Escenario*: una entrada copiada que va algo mas alla de su Freeze pasa a prevalecer sobre los Freezes de iniciativas posteriores.
- *Cambio requerido*: ADR aceptado / AGENTS > artefacto de Freeze + A-n > registro (puntero sin autoridad); M-08 dispara por contradecir la
  **fuente de decision** de la entrada; discrepancia entrada↔fuente = EXP-01 y manda la fuente; «contrato congelado» → «artefacto de Freeze».

**AR-13 — MEDIUM — DC-5 «un salto» y EXP-04 (L420, L434)**
- *Problema*: circular (solo se descubre consumo lejano mirando lejos) y ciego a consumidores de **datos** (B-02: RACKLISTA/RACKBOMTOTAL
  leen el conteo de referencias).
- *Cambio requerido*: DC-5 incluye a todo lector de los campos, estructuras o artefactos de dibujo nombrados en DC-3 (busqueda por clave o
  simbolo), sin limite de saltos.

**AR-14 — MEDIUM — Un disparador DC-9 en UNKNOWN no tiene via de expansion (L424, L429-438)**
- *Problema*: M-06, M-07, M-08 y «crear vs modificar» de M-01 no tienen EXP que autorice resolverlos; la unica salida es subir el
  arquetipo (a menudo a NA) o escribir «no» sin evidencia, que es la salida insegura.
- *Cambio requerido*: EXP-09 «un disparador DC-9 sigue UNKNOWN», acotado a esa pregunta.

**AR-15 — MEDIUM — Entrada de la primera revision del Architect (L583, L649; plantilla C L1097)**
- *Problema*: la revision del Freeze de EXTENSION lee un archivo que omite a proposito el detalle, pero debe comprobar M-01..M-08, lo que
  requiere el Discovery; B-29 se capturo porque la revision tenia las citas de codigo de G1.
- *Cambio requerido*: la primera revision en todo arquetipo recibe el Discovery (DC-1..DC-9 y expansiones) y puede leer codigo en su base.

**AR-16 — MEDIUM — Poblacion inicial del registro (P-06 regla 8 L522-526; regla 5 ultimo punto L516)**
- *Problema*: Header Mutation y Linked Properties se escribirian «desde el codigo» mientras sus fuentes (contratos I-35/I-40, V8 de I-48)
  discrepan del codigo; la regla 5 solo protege frente a ADR, no frente a un Freeze o contrato integrado; no se nombra revisor.
- *Escenario*: la poblacion consagra como regla un comportamiento que contradice un contrato V1; los consumidores pasan DC-8 contra la
  entrada y la pregunta clase A nunca se plantea.
- *Cambio requerido*: la poblacion aplica la regla 5 a toda discrepancia fuente↔codigo (A/B, Known limitations o decision); conformidad de
  la integracion normativa por entrada; la regla 5 cubre «ADR aceptado o Freeze integrado».

### Freeze, gates, Candidato y Owner Validation

**AR-17 — HIGH — Sin via legal para registrar tras READY-04 enmiendas, aceptaciones de desviacion o decisiones del Owner (P-12 L787-799; P-19 L1187; P-09 L671-675; P-14 L858, L876)**
- *Problema*: la ventana prohibe commits documentales; P-19 permite aceptar una desviacion «con registro» en READY-06; una A-n (p. ej. un
  escenario OV añadido durante OV) debe ir a `decisions/<I>.md`; las POLICY DECISIONS del Owner tambien. Ninguna escritura tiene camino, y la
  conformidad y READY-08 leen «Freeze + todas las A-n» cuando esa A-n no esta en el arbol del Candidato.
- *Escenario*: durante OV el Owner acepta una desviacion BEHAVIORAL-WITHIN-FREEZE que toca OV-03: o se escribe (y un commit documental
  invalida el Candidato por un tramite) o no se escribe (y el Candidato fue conforme contra una enmienda no registrada, perdida si la sesion
  cae antes del cierre).
- *Cambio requerido*: fijar la regla: toda A-n, aceptacion de desviacion o decision del Owner tras READY-04 es de clase correccion y
  reinicia en READY-02, **o** se registra literal en el informe de sesion y se escribe solo en el commit de cierre, citada integra por el
  registro de READY-06/OV.

**AR-18 — HIGH — La columna «Momento» de la matriz OV abre un hueco de SHA exacto (P-14 L847, L862-864; P-12 L800-804)**
- *Problema*: la matriz asigna escenarios a «Candidato intermedio / FINAL»; nada exige volver a ejecutar sobre `FINAL_CANDIDATE_SHA` los
  escenarios validados en un hito intermedio. P-12 y P-14 regla 4 ya dicen que un Candidato intermedio no sustituye al final salvo mismo
  SHA, pero la columna invita a la lectura contraria, y el efecto recae sobre una garantia vinculante (EA H.1-8).
- *Escenario*: OV-01 y OV-02 pasan en un SHA intermedio; solo OV-03 (FINAL) se ejecuta en el final; se integra con comportamiento de
  dibujo de gates posteriores sin revalidar OV-01/02 (clase del corpus I-45: el Owner hallo 8 de 17).
- *Cambio requerido*: todo escenario de la matriz se ejecuta sobre `FINAL_CANDIDATE_SHA` (o reutilizacion por mismo SHA); un hito
  intermedio es siempre adicional; la columna pasa a «Hito intermedio adicional: si/no».

**AR-19 — MEDIUM — El subconjunto OV de cada unidad vive en su contrato delta mutable (P-02 L254; P-14 regla 2 L858-859)**
- *Problema*: el Coordinator podria quitar un escenario de todas las unidades sin decision del Owner (retirada de hecho).
- *Cambio requerido*: la asignacion escenario→unidad vive en el Freeze, el Freeze delta o una A-n; todo escenario conceptual queda
  asignado a al menos una unidad; des-asignar exige al Owner; READY-08 comprueba la cobertura de la asignacion.

**AR-20 — HIGH — «Vuelta a 4.5.1 despues del cierre» rompe la regla de push agrupado (P-12 L796-798; P-20 L1207)**
- *Problema*: «el rebase reescribe tambien el commit de cierre» deja como punta empujada el cierre reescrito; el CI corre solo sobre la
  punta (AGENTS «Push agrupado»), asi que el Candidato rebasado no tiene corrida `push` propia, y se publica un cierre cuyo archivo de
  evidencia describe el Candidato anterior.
- *Cambio requerido*: volver la rama a `FINAL_CANDIDATE_SHA` (retirando el cierre), rebasar y empujar el Candidato solo como punta,
  READY-04..09 y evidencia Full, y solo entonces rehacer el cierre; añadirlo a la plantilla F.

**AR-21 — MEDIUM — El artefacto congelado no puede ser identico al texto acordado (P-09 L653-660; P-08 regla 4)**
- *Problema*: el commit de Freeze **debe** modificar el archivo (linea «Frozen»), asi que el blob congelado siempre difiere del acordado;
  la identidad de la version acordada (blob o commit) no se registra; un cambio «inocuo» en una clausula dentro del commit de Freeze pasa
  READY-09 porque ese commit lleva el trailer legitimamente.
- *Cambio requerido*: el diff del commit de Freeze solo puede tocar la linea «Frozen» y las de estado; el commit o blob acordado se registra
  (decisiones o evidencia); READY-09 comprueba que `git diff <acordado>..<commit-de-Freeze> -- <ruta>` se limita a esas lineas.

**AR-22 — MEDIUM — Unidades hermanas y enmiendas conceptuales (P-09 L651, L673-675; P-02 L272-275)**
- *Problema*: el Freeze conceptual y sus A-n se escriben en la rama de una unidad; una hermana reclamada desde `origin/main` no los ve hasta
  que esa unidad integre; dos unidades paralelas pueden escribir la misma «A-3»; una A-n pensada para una unidad afecta a todas.
- *Cambio requerido*: una unidad solo consume un Freeze conceptual y sus A-n cuando son alcanzables desde `origin/main`; cada A-n lleva
  `Applies-to: all | <unidades>`; READY-09 lista las A-n leidas y comprueba numeracion sin huecos ni duplicados; las A-n son de solo
  anexion (cambiar una exige otra).

**AR-23 — MEDIUM — Unicidad del trailer `Freeze:` mal acotada (P-09 L653-655; READY-09)**
- *Problema*: «exactamente un commit en la historia» no dice alcanzable desde que ref ni si la clave es unidad o unidad+ruta; un cherry-pick
  lo duplica; un squash del commit de Freeze con una edicion posterior pasa la comprobacion; el enlace del contrato al archivo de Freeze es
  mutable y puede re-apuntarse a otro archivo con su propio trailer.
- *Cambio requerido*: exactamente un trailer `Freeze: <unidad>` alcanzable desde el Candidato por unidad (y uno por Freeze delta); READY-09 y
  la conformidad toman la ruta **del trailer**, no del contrato; prohibido squash o reescritura del contenido del commit de Freeze.

**AR-24 — MEDIUM — Una A-n «solo Coordinator» puede cambiar arquitectura (P-09 L681)**
- *Problema*: «añadir obligaciones de prueba» puede afirmar un comportamiento no congelado (M-trigger de hecho), y el Architect solo lo ve
  en la conformidad final.
- *Cambio requerido*: una A-n del Coordinator declara M-01..M-08 «no activado» con motivo; una obligacion de prueba añadida solo protege
  comportamiento ya congelado; las A-n llegan al Architect antes de abrir el siguiente gate.

**AR-25 — MEDIUM — La evidencia del cierre de gate no cabe donde P-10 la pone (P-10 L713; P-11 L733; plantilla B)**
- *Problema*: la suite Core local y el CI existen solo **despues** del commit de cierre del gate, asi que su cuerpo no puede contenerlos
  (repetiria el defecto «suite antes del commit estampa el padre»); no se exige empujar el SHA de cierre solo como punta (si viaja agrupado
  no tiene corrida); no se dice que hace un rebase de WORKFLOW §4.2 entre gates al «CI del gate anterior leido».
- *Cambio requerido*: la evidencia del gate va al informe del gate y al cuerpo del commit siguiente o al archivo de evidencia (antes de
  READY-04), citando el SHA de cierre; el SHA de cierre se empuja solo como punta; tras un rebase se lee el CI de la punta rebasada antes de
  seguir.

### Tag de integracion

**AR-26 — MEDIUM — El mensaje del tag redefine `MERGE_SHA` (P-20 L1242-1244; P-15 L930; P-12 L820; plantilla E)**
- *Problema*: en WORKFLOW 4.5.6 y P-12, `MERGE_SHA` es el merge cuyo CI se verifica; el tag lo usa para el **primer** merge, que tras una
  correccion es el rojo; `FINAL_CANDIDATE_SHA` y `CLOSURE_SHA` no dicen a que ronda pertenecen.
- *Cambio requerido*: `MERGE_SHA` := merge verificado (destino del tag) y se retira `FINAL_MAIN_SHA`; `Unverified merges:` lista cada ronda
  previa como `merge / candidate / closure / motivo`; los demas campos son de la ronda verificada.

**AR-27 — MEDIUM — La exclusion de las corridas del push de tags no llega a los criterios de evidencia (P-20 regla 5; P-24 L1378)**
- *Problema (verificado)*: una corrida de push de tag tiene `event=push` y `head_sha` = commit etiquetado; el criterio de AGENTS (evidencia de
  UI del CI) y WORKFLOW 4.5.6 no comprueban `ref`, asi que es indistinguible de evidencia real. Los tags `archive/*` sobre SHAs pre-rebase ya
  podrian dar «evidencia» a SHAs sin corrida propia; la V2 añade mas pushes de tags, y P-24 no enmienda AGENTS.
- *Cambio requerido*: añadir `ref = refs/heads/<rama>` (o `refs/heads/main` post-merge) a los criterios de AGENTS y WORKFLOW 4.5.6; declarar
  que un estado rojo de la corrida del tag sobre el commit de merge no es señal de integracion.

**AR-28 — MEDIUM — Proteccion y validacion de tags poco realistas tal como estan escritas (P-20 reglas 3, 8, 9)**
- *Problema (verificado)*: el repositorio es publico y hoy tiene **0 rulesets**; la regla 9 detendria **todas** las integraciones V2 tras el
  SHA si falta la proteccion (posible bloqueo total); el unico admin puede saltarse el ruleset (protege frente a accidentes, no frente a
  manipulacion, y no se dice); la deteccion solo comprueba que el nombre exista (no tipo anotado, destino en el primer padre de `main` ni
  claves del mensaje) y depende de un script inexistente; no se dice si un `-corr<N>` lleva el bloque completo ni a que apunta.
- *Cambio requerido*: el ruleset de tags se crea y se verifica dentro de la integracion de I-56, antes del merge normativo; declarar su
  alcance (accidentes; bypass del admin); comando manual de verificacion en WORKFLOW 4.5 (tipo anotado, destino, claves requeridas, nombre
  `^integration/<unidad>(-corr[1-9][0-9]*)?$`); un `-corr<N>` lleva el bloque corregido completo y apunta al mismo destino salvo que la
  correccion sea del destino.

### Autoridad, plan de archivos y paquete del Owner

**AR-29 — MEDIUM — «La mas estricta gana hasta resolver» sin resolutor, plazo ni prueba de comparabilidad (P-17 regla 5 L1030-1031)**
- *Problema*: hay reglas incomparables (p. ej. «rebasar antes del merge» vs «no crear SHA nuevo tras el Candidato»); no se dice si una
  excepcion local del Owner puede renunciar a una garantia de seguridad (el caso exacto de O-2); las decisiones antiguas sin etiqueta de
  alcance (p. ej. `decisions/I-50.md`) no tienen alcance por defecto.
- *Cambio requerido*: la regla 5 solo aplica cuando un requisito contiene al otro; si no, STOP y decide el Owner; declarar que las
  garantias de EA H.1 no se renuncian por excepcion local; las decisiones sin etiqueta o anteriores a la V2 son locales y no citables.

**AR-30 — MEDIUM — Solapamiento de autoridad en el plan de archivos (P-24 L1364-1381; P-17)**
- *Problema*: INITIATIVE_LIFECYCLE recibe P-10 y P-12, cuyos READY-04/05 e invalidacion re-enuncian WORKFLOW 4.5.1/4.5.2 y AGENTS; segun la
  regla 3 de P-17 ese solapamiento es defecto de redaccion (la V2 naceria en STOP). PROMPT_TEMPLATES no tiene fila en P-17, pero contendria
  contenido normativo (reglas DEBE de P-16 y el formato del tag que consumen scripts).
- *Cambio requerido*: las filas READY de Git y evidencia en el ciclo de vida son solo punteros; PROMPT_TEMPLATES entra en P-17 como
  procedimental y subordinado; las reglas de P-16 van al ciclo de vida y el formato del tag y el esqueleto de evidencia a WORKFLOW §4.5/§8.

**AR-31 — MEDIUM — §6 sin semantica de aprobacion parcial (L1491-1516)**
- *Problema*: §0.2 aprueba una **version**; si el Owner aprueba la version pero rechaza OWN-H (tags), CR-04/U-11 se reabren y P-15 queda sin
  hogar para los hechos post-merge.
- *Cambio requerido*: todo OWN-x rechazado o modificado genera una nueva version de la Proposal, re-acordada por Coordinator y Architect,
  antes de la vigencia; decirlo explicitamente.

### LOW

| ID | Seccion | Problema | Cambio requerido |
|---|---|---|---|
| AR-32 | P-16; plantillas | No hay regla de STOP ante referencia a una clausula inexistente (O-5, «tier I-45»); las plantillas citan P-ids y «(AGENTS)» sin clausula; «no cubiertas adecuadamente» da margen para omitir | Referencia colgante = STOP; las plantillas citan destinos normativos; item 7 = «toda condicion especifica del gate» |
| AR-33 | Bloque de estado (L3-14) | La Proposal dice Coordinator = REVIEW REQUIRED aunque el Coordinator acordo esta version exacta; editar el bloque cambiaria la version | Identidad de version = commit/blob; el estado de consenso se registra en `decisions/I-56.md`, no en la Proposal |
| AR-34 | P-06 reglas 2 y 5 B | Un consumidor no puede editar la entrada pero la regla 5 B manda anotarla | En su cierre, un consumidor solo puede anexar a Known limitations, con B confirmada |
| AR-35 | M-06, P-03 NA | «Consumiran» y «transversal» sin definir | Consumidor futuro solo si esta declarado (ROADMAP, contrato o ADR); transversal = ≥2 sistemas/kinds o un comando fuera de la iniciativa |
| AR-36 | P-20 re-fetch antes del merge | Carrera entre el fetch y el push de `main` | Si el push de `main` se rechaza, se descarta el merge local y se vuelve a 4.5.1; nunca `pull`/rebase de `main` local sobre el merge |
| AR-37 | P-11 guardia documental | Sin base definida para el rango del gate | Base = SHA de cierre del gate anterior (o merge-base); rango sin rebase ni merge; si no, UNKNOWN = FALLO |
| AR-38 | P-14 regla 5 | Solo se comprueba el DLL construido; B-13 fue el DLL **cargado** | En el bloque de la guia §7 el Owner confirma en sesion la ruta cargada y `InformationalVersion` |
| AR-39 | READY-05; P-12 | READY-05 nombra solo `ui-tests` y P-10 exige todos los jobs; no se dice que la OV empieza con el bloque §7.1 completo | READY-05 = los 4 jobs; orden «§7.1 completo → OV» |
| AR-40 | P-20; plantilla F | O-10 (`measured-sha.txt` no descargable por permisos) no tiene regla ni campo | Plantilla F/E nombra como se verifica `measured-sha` cuando no se puede descargar (log de la corrida) |

---

## MATERIAL RISKS

**R.1 Preservacion de capturas historicas** (EA TABLE B; «PARTIAL» remite al hallazgo que la completa):

| Captura | Etapa V1 | Etapa V2 | Preservada |
|---|---|---|---|
| B-01, B-03, B-04 | Discovery | Discovery Core DC-2/DC-3/DC-4/DC-8 | SI |
| B-02 | Discovery | DC-5 / EXP-04 | PARCIAL (AR-13) |
| B-05 | Discovery conceptual, arreglo en E3 | Discovery conceptual + delta | PARCIAL (AR-09) |
| B-06 | Revision del Coordinator del Discovery | Solo confirmacion de arquetipo | PARCIAL (AR-10) |
| B-07, B-15 | Architect primera ronda | Primera revision FE/NA | PARCIAL (AR-08) |
| B-08, B-09, B-19, B-21 | Architect | Revision FE/NA y re-revision | SI |
| B-20, B-22 | Architect rondas posteriores | Re-revision del delta | PARCIAL (AR-07) |
| B-23, B-24 | Architect (sonda, editorial) | Revision o RED de gate; errata | SI / PARCIAL |
| B-29 | Architect tras G1 | Revision FE o del Freeze de EXTENSION | PARCIAL (AR-15) |
| B-10, B-11, B-40 | Coordinator entre gates | P-10 condicion 5 | SI (gates mas grandes = retrabajo mayor) |
| B-16, B-18 | RED focal | P-10 condicion 2 | SI |
| B-41 | Demostraciones RED de guardas | P-10 condicion 2 | PARCIAL (nombrar guardas e invariantes) |
| B-12, B-17 | CI push | P-10 condicion 4; CI en cada punta | SI (AR-25 para punta sola) |
| B-26, B-27, B-30, B-31 | Conformidad / post-merge | P-19 en todos los arquetipos | SI (AR-17 para el registro) |
| B-13 | Ninguna | P-14 regla 5 | PARCIAL (AR-38) |
| B-38 y corpus I-45 (8 de 17) | OV por cambio de dibujo | Matriz aditiva + checklist | PARCIAL (AR-18, AR-19) |
| C-05, B-25 | CI del MERGE_SHA | Sin cambio | SI (AR-26, AR-27) |
| B-33, Q8/Q9 | Fetch al producir evidencia | READY-04/05 + re-fetch antes del merge | SI (AR-20, AR-36) |

**R.2 Otros riesgos que no exigen necesariamente cambio**

1. **SAME-SESSION ROLE seguira siendo lo habitual**: el autor revisa su propio Discovery y su Proposal, tambien en conformidad; el sesgo de
   relectura de EA Q5 persiste. Esta misma revision es un ejemplo (ver «Modo de revision»).
2. **Unidades bajo Freeze ya revisado** sin revision de diseño del Architect: solo dos casos de evidencia (E2, E3); un Freeze delta que
   «solo añade» no tiene revision del Architect antes de la conformidad.
3. **Primera unidad fundida grande** (E1 17h17): el unico control de tamaño es «1-3 sesiones» (U-05) y la division de P-10.
4. **Una clase A hallada por una iniciativa sobre una fundacion que consumen otras ramas activas** no tiene regla de aviso (clase B-30).
5. **Regresiones de Core solo-Windows** permanecen ocultas mas tiempo cuanto mas grande es el gate (coste de retrabajo, no de escape: el
   Candidato Full sigue intacto).
6. **Livelock de integracion con `main` activo**: cada rebase repite conformidad, evidencia Full y OV (U-09 sin cota).
7. **Comprobaciones fail-closed manuales** (READY-09, auditoria de tags, preflights) hasta que existan los scripts de P-21.
8. **`decisions/<I>.md` es mutable sin mecanismo de integridad**: una edicion silenciosa cambia «Freeze + A-n» sin tocar el archivo congelado.
9. **Sin semantica de reversion**: si hubiera que revertir el merge normativo, T1..T8 no cubre reclamos entre el SHA y la reversion.
10. **Pausa indefinida** si la verificacion roja exige volver a consenso, bloqueando tambien `fix/*` sin excepcion definida.
11. **Unidades V2 que consumen Freezes V1 vivos** (I-49 aun enmienda bajo V1): cada enmienda dispara STOP y recompatibilidad.
12. **La prevencion de O-1..O-3 depende de que el executor detecte la contradiccion**; G1 muestra que a menudo obedecio la orden.
13. **Estado rojo de corridas de tags** visible en el commit de merge, malinterpretable por personas (U-15).
14. **Cabecera de estado desactualizada** en la propia Proposal V2 (Coordinator = REVIEW REQUIRED) mientras el Coordinator la acordo
    (AR-33).

---

## REQUIRED CHANGES

| # | Disagreement | Cambio (resumen; detalle en el hallazgo) |
|---|---|---|
| RC-01 | AR-01 | Peldaño 2 valido solo con push aceptado y fetch posterior en el mismo registro, y contraste con Actions; si no, T6 |
| RC-02 | AR-02 | Instantaneas `ls-remote` antes y despues del push del merge normativo; clasificacion por `Claim-Id` |
| RC-03 | AR-03 | T8 como opciones T8-a/T8-b; STOP para unidades nuevas de I-49/I-52/I-55 hasta OWN-E |
| RC-04 | AR-04 | `^1` solo para las clausulas cambiadas por el merge normativo |
| RC-05 | AR-05 | Pausa y tag de I-56 como decisiones V1 del Owner; registro de inicio antes de 4.5.1 y de fin durable |
| RC-06 | AR-06 | Registro determinista u obligatorio del SHA efectivo |
| RC-07 | AR-07 | Re-revision con version completa; el revisor decide interacciones; todo hallazgo material se registra |
| RC-08 | AR-08 | REQUERIDO (d); rebaja solo por el emisor; poda con conformidad del Architect |
| RC-09 | AR-09 | Discovery delta re-verifica DC-1..DC-6 de su superficie salvo diff vacio |
| RC-10 | AR-10 | Evaluacion negativa de EXP; revision del Coordinator del Discovery con facultad de ordenar expansiones |
| RC-11 | AR-11 | Clase B confirmada por Coordinator y Architect en todos los arquetipos |
| RC-12 | AR-12 | Registro sin rango de autoridad; M-08 sobre la fuente; terminologia «artefacto de Freeze» |
| RC-13 | AR-13 | DC-5 incluye lectores de datos persistidos sin limite de saltos |
| RC-14 | AR-14 | EXP-09 para disparadores DC-9 en UNKNOWN |
| RC-15 | AR-15 | Primera revision recibe el Discovery y puede leer codigo |
| RC-16 | AR-16 | Poblacion inicial con regla 5 completa y conformidad por entrada |
| RC-17 | AR-17 | Regla explicita para A-n, desviaciones y decisiones del Owner tras READY-04 |
| RC-18 | AR-18 | Todo escenario OV sobre el SHA final; hitos intermedios solo adicionales |
| RC-19 | AR-19 | Asignacion escenario→unidad en Freeze/A-n; des-asignar exige Owner |
| RC-20 | AR-20 | Procedimiento de vuelta a 4.5.1 tras el cierre con el Candidato empujado solo |
| RC-21 | AR-21 | Diff del commit de Freeze limitado; identidad de la version acordada registrada |
| RC-22 | AR-22 | A-n conceptuales alcanzables desde `main`, `Applies-to`, numeracion comprobada, solo anexion |
| RC-23 | AR-23 | Unicidad del trailer por unidad desde el Candidato; ruta tomada del trailer; sin squash |
| RC-24 | AR-24 | A-n del Coordinator con M-01..M-08 declarados; al Architect antes del siguiente gate |
| RC-25 | AR-25 | Evidencia del gate fuera del commit de cierre; SHA de cierre como punta sola; CI tras rebase |
| RC-26 | AR-26 | `MERGE_SHA` = merge verificado; rondas previas en `Unverified merges` |
| RC-27 | AR-27 | `ref` en los criterios de evidencia de AGENTS y WORKFLOW 4.5.6 (en P-24) |
| RC-28 | AR-28 | Ruleset creado y verificado en la integracion de I-56; comando de verificacion de tags; contenido de `-corr<N>` |
| RC-29 | AR-29 | Comparabilidad, garantias H.1 no renunciables, decisiones sin etiqueta = locales |
| RC-30 | AR-30 | Punteros en el ciclo de vida; rango de PROMPT_TEMPLATES; ubicacion de reglas P-16 y formato del tag |
| RC-31 | AR-31 | Aprobacion parcial = nueva version re-acordada |
| RC-32..RC-40 | AR-32..AR-40 | Cambios LOW de la tabla correspondiente |

---

## OPTIONAL IMPROVEMENTS

1. Plantilla C: campo «autor del texto revisado = revisor: si/no» junto a Review mode.
2. Plantilla A: aclarar que un arquetipo provisional con UNKNOWN del intake no tiene efecto antes del Discovery.
3. DC-8 puede anexar «verificada por <unidad> <fecha>» al archivo de evidencia del consumidor (dato de frescura para P-22 sin editar el
   registro).
4. Suite Core local en cada push de cierre de sesion (WORKFLOW §4.3): cadencia natural que acota el riesgo R.2-5 sin clasificar cambios.
5. Trailer `Amendment: <I> A-n` y hash del blob del Freeze en el archivo de evidencia para mecanizar READY-09 y la completitud de A-n.
6. Gramatica estricta `Clave: valor` con claves obligatorias para el mensaje del tag.
7. Plantilla F: «excluir `ref=refs/tags/*`» y «SHA de cierre empujado solo como punta».
8. Hacer obligatoria la pausa de reclamos en la ventana de integracion de I-56 (elimina la mayoria de carreras T5/T6).
9. Automatizar las instantaneas de AR-02 en `initiative-preflight` (U-17) y cerrar U-17 con el procedimiento manual.
10. Dividir OWN-E en decision del Owner (T8, pausa, tag de I-56) y decision tecnica (escalera de evidencia).
11. Deduplicar la retirada de «Core local antes de cada push», que aparece en OWN-C y OWN-G.
12. Convencion de nombres en `docs/automation/evidence/` para no confundir `<unidad>-evidence.md` con los `I-xx-autocad-validation.md`
    existentes.

---

## OWNER DECISIONS REVIEW

| ID | Veredicto | Nota |
|---|---|---|
| OWN-A | Correctamente reservada al Owner | Podria agruparse con OWN-B (ambas fijan profundidad de revision) |
| OWN-B | Correctamente reservada al Owner | — |
| OWN-C | Correctamente reservada al Owner | Cambia AGENTS; solapa con OWN-G |
| OWN-D | Correctamente reservada al Owner | La definicion de Candidato intermedio roza lo tecnico |
| OWN-E | Reservada, pero sobrecargada | T8 debe presentarse como opciones (AR-03); la pausa, como decision V1 transversal (AR-05); la escalera de evidencia es tecnica (Coordinator + Architect) |
| OWN-F | Correctamente reservada al Owner | Solo el estatus normativo y los campos obligatorios; la redaccion es tecnica |
| OWN-G | Correctamente reservada al Owner | Deduplicar con OWN-B y OWN-C |
| OWN-H | Correctamente reservada al Owner | Añadir alternativa si se rechazan los tags (AR-31) y que la configuracion del ruleset es una accion del Owner (AR-28) |
| OWN-I | Podria ser global (tecnica) | Confirmar «sin cambio» no necesita decision especifica; inocuo |
| OWN-J | Correctamente reservada al Owner | — |
| OWN-K | Reservada, pero amplia | La regla A/B de discrepancias es tecnica/arquitectonica; la creacion de archivos nuevos (P-24) pertenece a la autoridad (OWN-L) |
| OWN-L | Correctamente reservada al Owner | Incluir irrenunciabilidad de garantias H.1 por excepcion local, alcance por defecto de decisiones sin etiqueta y rango de PROMPT_TEMPLATES (AR-29, AR-30) |
| OWN-M | Mixta | La matriz OV aditiva es del Owner; la comprobacion del DLL es tecnica (la guia §7 ya pide SHA-256) |

**Decisiones del Owner que faltan:**

1. **ADR de Workflow V2 (U-06)**: reservada al Owner y ausente de §6; WORKFLOW §8 exige el ADR antes de implementar, asi que debe decidirse
   antes de los gates normativos.
2. **Orden expresa que autorice editar `AGENTS.md`** (contrato I-56 §12).
3. **Semantica de aprobacion parcial** (AR-31).
4. **Pausa de reclamos obligatoria u opcional**, y sus excepciones (`fix/*`).
5. **Politica de reversion o desactivacion** de la V2 (R.2-9).
6. **Tratamiento de reclamos V1 existentes sin registro** (AR-02).
7. **Aprobacion del contenido de la poblacion inicial de FOUNDATIONS** (P-06 regla 8; AR-16).
8. **Creacion del ruleset de tags** como accion del Owner (AR-28).

---

## CONSENSUS STATUS

```text
ARCHITECT = CHANGES REQUIRED — PROPOSAL V3

Coordinator on V2 = AGREED
Consensus = NOT REACHED   (solo seria REACHED con ARCHITECT = AGREED sobre esta V2 exacta)
Owner = NOT REQUESTED
Workflow V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```

Motivo: 0 BLOCKER; 10 HIGH (AR-01, AR-02, AR-03, AR-07, AR-08, AR-09, AR-10, AR-17, AR-18, AR-20); 21 MEDIUM (AR-04..AR-06,
AR-11..AR-16, AR-19, AR-21..AR-31); 9 LOW (AR-32..AR-40). Ninguno exige una decision del Owner **antes** del consenso: AR-03 pide presentar T8
como opciones dentro de OWN-E, no decidirlo ahora. Todos los cambios son locales a decisiones existentes; no se pide rediseño.
