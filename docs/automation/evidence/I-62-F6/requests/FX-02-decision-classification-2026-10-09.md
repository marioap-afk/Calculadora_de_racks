# I-62 — FX-02: clasificación de las decisiones pendientes de la solicitud de desbloqueo (decisiones §61, punto 3)

> Preparación de la supervisión (plano a), una sola pasada. Nada ejecutado, nada decidido, nada publicado. Fuentes leídas en el worktree
> `architecture-portabilidad-coordinador-principal`, HEAD `f504f6a9`: solicitud `requests/FX-02-unblock-request-2026-10-09.md` (blob `203d2887`),
> candidata A-4 (blob `27ffa26b`, el mismo que cita §61.2), kit FX-02 (README §1 y §7, `frontiers.json`, plantillas O4 y de designación, tarjeta A2),
> solicitud consolidada, decisiones §50-§61, V14 §20.1, D.3, D.4 y D.6, AP 16.20-16.30 y LIFECYCLE §1, §6 y §10.
> Mandato (§61.3): la solicitud es «un registro de propuestas, no de autorizaciones concedidas»; «No se acepta en bloque una propuesta que cambie
> autoridad, independencia o presupuestos sin comprobarla. A2 no se lanza antes de cumplir sus precondiciones».

**Leyenda.**
- **G1** procedimiento ordinario con autoridad suficiente. Solo entra lo que una cláusula ya asigna al Coordinator del fixture: `frontiers.json`
  `group_a_fixture_coordinator_procedural` («rellenos y actos ordinarios que la norma ya prevé; ninguna lectura de un hueco del texto congelado»),
  README §1 (a) y solicitud consolidada §2.2. Además, no cambia A/I/P.
- **⚑ = «disposición del Coordinator de I-62 requerida» (DCI62).** Es una lectura de un hueco del texto congelado (LIFECYCLE §10: «Coordinator resuelve
  una errata o interpretacion dentro de su dominio»; README §1, grupo b) y **no es ordinaria** hasta que se registre en `decisions/I-62.md`.
  **G1⚑** = procedimiento bajo la autoridad del Coordinator, que bloquea sin depender de A-4 ni del Owner, pero cuya autoridad aún no se ha ejercido.
- **G2** cubierta por una decisión §50-§61 o por el texto congelado literal: basta confirmarla o transcribirla.
- **G3** cambio material que depende de A-4. LIFECYCLE §6: «Una M material exige Architect + Coordinator». Nunca se acepta en bloque.
- **G4** exclusiva del Owner (consumo, OD, apertura de sesión, mensaje). Base: §55.7, «el Owner solo para consumo y decisiones OD reservadas», y §61.5.
- **G5** no bloquea el camino crítico.
- Precedencia de asignación: G4 > G3 > G2 > G1 o G1⚑ si bloquea; si no bloquea, G5.
- **A/I/P** = ¿cambia la autoridad, la independencia o los presupuestos?
- **CP** (camino crítico) = de S04 (A2 abierta) a S25 (Q7 VERIFIED).
  - «Sí Sxx»: bloquea en ese paso.
  - «O4»: no bloquea la Q7 VERIFIED, pero tiene que figurar en la orden O4 antes de S02/Q0; si no, el PASS se pierde en esta ronda (A2 es la última
    sesión de Principal, U-14, y dentro de la ventana no se publica nada, W-2).
  - «No»: posterior al Q7, contingente, o su retirada tiene el mismo efecto.

## §1. Tabla de los cinco grupos

| # | Ítem | G | Base de autoridad | A/I/P | CP | Comprobación / nota |
|---|---|---|---|---|---|---|
| 1 | U-06 (a)(b): «continúa» del Owner (práctica vigente) en S09, S14 y S16, y tras una COORDINATOR_DECISION posterior a S26 | G1⚑ | Ninguna literal: §50 autorizó a *la supervisión* a escribir a *A* en G0/QU/QH (otro actor, otro alcance); §54 «Bus de mensajes»; ev. §71 (R) | NO | Sí S09, S14, S16 | §61.5: «No se piden … mensajes manuales salvo cuando lo exija el contrato vigente». La disposición tiene que declararlos exigidos por O4 |
| 2 | U-06 (c): cada «continúa» se registra como AUTONOMY_GAP | G2 | V14 §20.1: «si su transporte depende del Owner, es un AUTONOMY_GAP (§20.9)»; la fila FX-02 PASS de D.4 no pone condición de autonomía | NO | No | se transcribe a `{AUTONOMY_GAP_RULE}` |
| 3 | U-06 (d): «continúa» en los pasos 1-7 de FX-06 | G5 | fuera de FX-02 (la propia solicitud la excluye) | NO | No | — |
| 4 | U-07: TRX de la CI de push, con un commit del Coordinator del fixture en `fx/u1` antes de S02 | G1⚑ | AP 16.9 #10 exige el TRX. Es una excepción a §50 («Sin más cambios del flujo salvo que esa observación lo exija»), que solo el Coordinator de I-62 puede hacer; §49 «sin sustituto local»; CD-01, grupo b | NO | Sí S21-S22: sin TRX, `Tests` queda not_run y nunca hay VERIFIED | comprobar que no altera la clasificación A de la CI (§51) |
| 5 | U-08: `.gitignore` publicado antes del Q0 | G1⚑ | RAE §2 lo da por hecho («que `.gitignore` ya ignora»); CD-02, grupo b | NO | Sí S03, S18-S25 (`CleanTree`, Salida de 16.4) | — |
| 6 | U-09 (a): literal del marcador `I62-ROLE-BINDING: <BindingId> ACCEPTED` para el binding PLAN | G1⚑ | AP 16.28 no trae marcador para bindings que no son del Principal; la aceptación individual sí existe (AP 16.20: «decisión del Coordinator en `decisions/<unit>.md`») | NO | Sí S15-S16 | la aceptación exige además H-2 (#10); PA-04 lo transcribe |
| 7 | U-09 (c): la supervisión repite `Scope` tras el Q7 | G5⚑ | AP 16.1 («puede rechazar un VERIFIED»); RAE §14.7; W-2 impide hacerlo dentro de la ventana | NO | No (S29) | — |
| 8 | U-09 (d): rechazo de un VERIFIED tras el Q7 como COORDINATOR_DECISION por añadido | G5⚑ | V14 §20.1; AP 16.1 | NO | No (S29) | — |
| 9 | U-09 (e), **sin propuesta en la solicitud**: quién decide A1'-A8' dentro de la ventana (`{IN_WINDOW_ACCEPTANCE_RULE}`, punto 7 de O4) | G3⚑ | T3 (V14 L613) da la decisión al Coordinator, que no puede publicar dentro de la ventana; F.1 paso 3: «P, Coordinator / sin escritura»; A-4 Q-A4-11 pregunta si A4-3 la cubre | **SÍ** (autoridad dentro de la ventana) | Sí S19; O4 punto 7 | resolverla en la revisión de A-4 (Q-A4-11) o por una lectura del Coordinator. Sin ella, S19 no tiene decisor |
| 10 | H-2 = U-10 (a): independencia REQUIRED de una candidata que no ha arrancado | G3 (A4-4) | AP 16.21: «Si falta un REQUIRED … no se acepta» y «UNKNOWN cuenta como NOT_SATISFIED». Hace falta una A-n | **SÍ** (independencia) | Sí S15-S16, S21b-S22 (y S26) | Q-A4-12 (¿M-02/M-05?), Q-A4-13. Conjunto de referencia vacío = SATISFIED en PLAN |
| 11 | U-10 (b): las filas RUNTIME_OBSERVED salen de la invocación medida; A2 no hace invocaciones de observación (0 en los topes) | G1⚑ | §55.9 dice *qué* es la invocación medida, no de dónde salen las filas del preflight de A2 | NO (no sube topes) | Sí S15, S18b, S21b | toca la base de la elegibilidad (P-10): comprobar RAE §13.1 #4-#5 y AP 16.20 paso 3. Para el Controller, la medición es la de A4-1 (#46) |
| 12 | U-11 (a): colocación B, RLA (`Budget` solo baja), `CorrectionScope` vacío | G5⚑ | README §3: V14 §20.4, B.8.4 I-P02 y AP 16.29 solo admiten A o B, y decide el Coordinator de I-62 (CD-05, grupo b). AP 16.28: «entero no mayor que el congelado» | NO | No (S26 va tras el Q7; la RLA puede publicarse después a cambio de un «continúa» más) | con B, A4-2 y `A4-CLAUDE-FX02-CONSUMO` salen del camino a VERIFIED |
| 13 | U-11 (b): oráculo de FX-02 (OQ-19) | G5⚑ | D.4: «PASS: ejecutado y conforme al oráculo»; recetas: «VERIFIED + negativos» | NO | No (S29) | si el oráculo incluye la revisión del Architect, A4-2 pasa a ser necesaria para el PASS |
| 14 | U-11 (c) = F6-OBS-03 V1 (b): Architect de FX-02 por `claude-cli` | G3 (A4-2) | D.3 fija `codex-cli`, cuya celda dio 0/6 (§60.2); §61.4: «no se sustituye en silencio»; OD-3 y CLAUDE-CLI-I62 no cubren FX-02 (A-4 §2) | **SÍ** (adapter y consumo; Proveedor PREFERRED sin cumplir) | No para VERIFIED (S26); para el PASS, según #13 | Q-A4-04; Q-A4-05 (§57.3 ya acepta la elegibilidad de `claude-cli` para ARCHITECT); Q-A4-09 |
| 15 | U-12: Reviewer retirado de esta ronda | G5⚑ | T1 sin REVIEWER; AP 16.21 «REVIEWER (si se pide)»; D.3 «Ningún tope autoriza gasto ni ejecución» | NO | No (S27 sale de la secuencia) | PA-02 retira la fila del Reviewer solo después de esta disposición |
| 16 | U-13 (a): exención de `dotnet test` para las invocaciones READ_ONLY del Controller (O4, punto 8) | **G1** | `group_a`, CD-07: «acto ordinario del Coordinator de la unidad (AP 16.24; V14 C-41 a)»; README §1 (a) | NO | Sí S18, S22, S23 | — |
| 17 | U-13 (a'): exención del Architect, si va en el bloque de la RLA | G5⚑ | AP 16.28: «las exenciones de opción B van en el mismo bloque, con las claves de 16.20», claves que no existen (D-01; FX-06 OQ-05) | NO | No (S26) | en el punto 8 de O4 no hereda ese hueco |
| 18 | U-13 (b): es una obligación que exige exención previa | G2 | AP 16.24: «sin exención no hay lanzamiento»; V14 C-41 (a) | NO | — | — |
| 19 | U-14: R no cuenta en el tope de Principal de la ronda A | G1⚑ | D.3: «Sesiones de Principal ≤ 2»; §51: R temporal, defecto NO MATERIAL, «sin A-n, sin cambio del Freeze» | **SÍ** (presupuesto) | Sí S03b, S04 | comprobar con §55.2 (una sesión más «aumentaría un presupuesto congelado») y con OD-5. Si «R cuenta», A2 es la 3.ª: A-4 no lo cubre (§3.7: «No sube topes») y haría falta consumo del Owner por encima de OD-5 (precedente B3-CONSUMO). **Decidir antes del veredicto de A-4** |
| 20 | U-15: registro saneado de la medición en `fx/u1` (commit de S13) | G1⚑ | CD-09, grupo b; P-16; §55.9 no lo dispone | NO | Sí S13, S15-S16, S18b, S20-S21b | — |
| 21 | U-16 (1): mutaciones de N4, N5, N7a y N7b. Además N8: su premisa («candidata sin credencial `claude-cli`») caducó con OD-3 = A (§56; A-4 Q-A4-08) | G1⚑ | CD-11, grupo b (contenido del oráculo del Coordinator) | NO | O4 (S23) | N8 necesita una mutación nueva o NOT_APPLICABLE con causa |
| 22 | U-16 (2): N6 = NOT_APPLICABLE | G1⚑ | precedente de N11: §51 («no se fabrica una entrada»), §53 y §55.2 | NO | O4 (S23) | quita un negativo congelado de D.3: comprobar que la causa es el conflicto literal con la Salida de 16.4 |
| 23 | U-16 (3): N8-N10 dentro de la ventana, antes del Q7 | G2 | texto congelado literal: V14 B.8.3 y §8.4; AP 16.4 (5) | NO | O4 (punto 15; S25b desaparece) | — |
| 24 | U-16 (4): todo control negativo sobre copias, con su disposición confinada al control | G1⚑ | RAE §10 solo confina nc4 | NO | O4 (S23) | sin revelar ninguna disposición |
| 25 | U-17 (a)(b)(c): custodia de cierres y preflights en Q0/Q7; `Target` de IMPLEMENT y de VERIFY | G1⚑ | CD-14, grupo b; V14 §8.4 y B.9; AP 16.25 | NO | Sí S17-S23 | — |
| 26 | U-18 (a): no se repite el paso 1 de FX-04a | G2 | §60.1: FX-04a PASS, «Sin repetir B3 ni abrir B4» | NO | — | — |
| 27 | U-18 (b): `task_intent` del Q7 = null | G1⚑ | B.8.3: «`null` o la intención siguiente (decisión del Coordinator)». El kit puso CD-15 en (b) por FX-04a y FX-06; tras §60 solo queda el efecto sobre FX-06 | NO | Sí S17/S25 (tiene que estar en O4 antes del Q0) | el Coordinator de I-62 puede declararla acto del fixture |
| 28 | U-18 (c): acción tras el Q7 (bucle del Architect, QH, terminación) | G5⚑ | AP 16.26 T17; V14 §9.1 | NO | No (S28) | condiciona la opción A de FX-06 |
| 29 | U-19: lectura de `**`; ningún commit del Worker solo de evidencia | G1⚑ | CD-17, grupo b; AP 16.5 («sin comodines»); AP 16.1; B.8.6 | NO | Sí S19-S23 | — |
| 30 | U-20: la huella UNVERIFIED de `claude-subagent` no impide el rol WORKER | G1⚑ | CD-18; V14 §7 frente a AP 16.19 | NO | Sí S18b, S20 | toca la elegibilidad (P-10): comprobar que V14 §7 basta |
| 31 | U-21: referencia AUTHOR por el digest del `InstanceId` | G5⚑ | CD-19, grupo b; RAE §14.5 | **SÍ** (independencia) | No (S26) | comprobar P-16/D.7 (digest de un id real) y D.6. La parte NOT_STARTED es H-2 (#10) |
| 32 | U-22: ninguna reejecución BLOCKED del Worker | G5⚑ | fila Worker de D.3 frente a AP 16.8 | **SÍ** (lectura restrictiva de un tope) | No: sin disposición, `{WORKER_RERUN_RULE}` se retira con el mismo efecto (README §4, punto 6) | comprobar que no recorta un derecho que FX-02 necesite |
| 33 | U-23 (a): requisitos del binding de VERIFY | G1⚑ | CD-22; routing §8; AP 16.21, fila «Toda delegación» (REQUIRED ×3 frente a WORKER) | NO (mantiene la independencia literal) | Sí S21b-S22 | comprobar RAE §14.1 punto 1 y §14.6 G1 |
| 34 | U-24: cierre declarado y Q7 antes de esperar | G1⚑ | CD-23; ya existen en V14 B.8.1 (`closure_source` SESSION) y en AP 16.25 («o tras el cierre declarado») | NO | O4 (S17); A4-3, punto 3, la presupone | — |
| 35 | H-3 = U-25 (a): tope de una corrección | G3 (A4-5, separable) | D.3: «+1 solo si hay REWORK»; el pool es de reintentos | **SÍ** (presupuesto) | No (S30) | Q-A4-14. Sin A4-5, `{CORRECTION_RULE}` = ninguna corrección |
| 36 | U-25 (b): ninguna corrección tras un `Tests` not_run sin fuente | G5⚑ | AP 16.11 | NO | No | con U-07 desaparece la causa |
| 37 | U-25 (c): REWORK sin corrección → UNVERIFIED con causa | G5⚑ | D.4 («violación observada» = FAIL; «falta una precondición» = UNVERIFIED) | NO | No (S29) | A4-5, punto 4, lo fijaría |
| 38 | U-26 (a)-(d): D.6 aplicada a A2 | G5⚑ | CD-25, grupo b; D.6 está escrita para FX-04a | **SÍ** (aislamiento/Contexto; añade una condición de PASS que D.4 no pone) | No (S29; las listas A y B ya están en la tarjeta) | comprobar D.4 (la fila FX-02 PASS no tiene condición de aislamiento) y D.6 |
| 39 | U-27: refresco de todos los `StateRef` a `FX-U1.md` | G2 | §51 (F6-OBS-01): «al crecer el archivo, el blob se refresca»; «todo otro `StateRef` al archivo de decisiones» | NO | (S17, S25) | confirmar |
| 40 | U-28: la designación cita la terminación de R | G2 | §52: «terminación de R ACREDITADA»; §51 (R titular del QH2); AP 16.26 T16 | NO | (S08) | confirmar |
| 41 | U-66: B3 frente a escrituras en `fx/u1` | G2 | §58.1: «Usa solo el clon B3 preparado (origen congelado en QH2)»; §60.1: «Sin repetir B3 ni abrir B4» | NO | libera S02 | registrarla «resuelta por hecho»; FX-06 OQ-26 decae |
| 42 | U-68: `-C` del Architect de FX-02 (`arch02`) | G3 | depende de A4-2 (con `claude-cli`, «la medición la fija la A-n»); misma lógica de Cesión que U-03 (§55.10) | NO (por sí solo) | No (S26) | `arch02` se crea en el QU LAUNCHING |
| 43 | U-71 (a): directorios del bloque A2-P2 | G2 | §58.2 (`A` y `arch`; bloque ejecutado, ev. §93) | NO | — | — |
| 44 | U-71 (b): bloque de recuperación con A2 abierta | G5⚑ | A-2 §3.3, reglas 3 y 6 (la disposición que nombre el par puede fijar el directorio) | NO | No (contingente) | nunca `-C A2` con A2 abierta |
| 45 | H-1 = U-09 (b) + U-23 (b): aceptación de bindings dentro de la ventana | G3 (A4-3) | AP 16.20 (aceptación individual; materialización solo para ARCHITECT y REVIEWER), RAE B9 y W-2: hace falta una A-n | **SÍ** (autoridad: base y momento de la aceptación) | Sí S18b-S22; O4 puntos 5-6 (S02) | Q-A4-10 (¿materialización encubierta? AP 16.20: «Sin aceptación fingida»); Q-A4-11 |
| 46 | F6-OBS-03 V1 (a) = A4-1: shell declarada y 1 invocación medida de VERIFY | G3 (A4-1) | §60.2 («preparar la disposición o enmienda … antes de ejecutarla»); §61.4 | **SÍ** (presupuesto: 1 invocación fuera de A2-P2 y de «+ 2 sondas») | Sí S04 (medición previa), S13, S15-S16, S18, S21b-S23 | Q-A4-01..03. La disposición de aplicación nombra celda, shell y trío (A4-1, regla 3). A-4 no fija el directorio: con `-C A2` tras S03, la sonda queda atada a S02 |
| 47 | F6-OBS-03 V3: una actualización que corrija la regresión | G5 | A-2 §3.3, regla 3 (AGREED en §57): bloque por actualización observada, con disposición que nombre el par y consumo del Owner; OD-2 nueva (OD-2-MAT = A, §58) | NO (dentro de A-2) | No (contingente) | si llega antes del AGREED, A4-1 sobra; A4-2 no |
| 48 | PA-01 `{EVIDENCE_DIR}` | **G1** | `group_a`, PF-EVIDENCE_DIR; solicitud consolidada §2.2: «no requieren disposición» | NO | Sí S02 | — |
| 49 | PA-02: topes de D.3 y su `scope` (CD-12) | **G1** | `group_a`, CD-12: «transcripción sin cambio» | NO | Sí S02, S17, S25 | la fila del Reviewer, según U-12; la contabilidad de la corrección, según A4-5 |
| 50 | PA-03 `{PUSH_RULE}` (CD-16) | **G1** | `group_a`, CD-16 | NO | Sí S20 | — |
| 51 | PA-04: transcripción del marcador | **G1** | `group_a`, CD-03-marker | NO | Sí S15-S16 | después de U-09 (a) |
| 52 | Owner, paso 2: decisión solo si la revisión de A-4 halla una consecuencia OWNER-RESERVED (Q-A4-09: `claude-cli` en la topología A, más allá de la fila OD-3 «en B») | G4 (condicional) | LIFECYCLE §6: «una materia OWNER-RESERVED exige Owner»; §55.7 | SÍ si se activa | Sí si se activa (A-4 queda en «BLOCKED — OWNER DECISION») | — |
| 53 | Owner, paso 8: `A4-SONDA-CONSUMO = A` | G4 | A-4 §7; §61.4-§61.5 | **SÍ** (consumo) | Sí S04 | solo con A-4 AGREED y la disposición de aplicación |
| 54 | Owner, paso 8: OD-2 nueva si cambia la huella | G4 (condicional) | OD-2-MAT = A (§58); §59 | — | Sí, si ocurre | — |
| 55 | Owner: `A4-CLAUDE-FX02-CONSUMO = A` | G4 | A-4 §7; OD-3 y CLAUDE-CLI-I62 no la cubren | **SÍ** (consumo) | No para VERIFIED (S26); para el PASS, según #13 | en la misma solicitud que #53 (§61.5) |
| 56 | Owner, paso 10 / S04: apertura de A2 | G4 | D.3: Principal «abierta por el Owner»; OD-5 = «autorización previa de apertura y consumo» dentro de D.3 (depende de U-14) | NO si U-14 = «R no cuenta» | Sí (todo lo posterior) | el modo de permisos lo decide el Owner, sin recomendación |
| 57 | Owner, paso 12 / S09: «continúa» | G4 | acto del Owner; su base es #1 | NO | Sí S10 | — |
| 58 | Owner, paso 15 / S14: «continúa» | G4 | ídem | NO | Sí S15 | — |
| 59 | Owner, paso 16 / S16: «continúa» | G4 | ídem | NO | Sí S17 | — |
| 60 | Owner, paso 21: decisión y «continúa» tras un CHANGES REQUIRED | G4 (condicional) | V14 §20.1 | NO | No | — |
| 61 | Owner, paso 22: cierra la sesión A2 cuando se le indique | G4 | tarjeta, parte 2, punto 7 | NO | No | después de la terminación acreditada (`isRunning` = false dos veces) |

**Recuento (61 filas):**

| Grupo | Filas | Números |
|---|---|---|
| G1 estricto | 5 | #16, #48-#51 |
| G1⚑ | 16 | #1, 4, 5, 6, 11, 19, 20, 21, 22, 24, 25, 27, 29, 30, 33, 34 |
| G2 | 8 | #2, 18, 23, 26, 39, 40, 41, 43 |
| G3 | 7 | #9, 10, 14, 35, 42, 45, 46; #9 con ⚑ |
| G4 | 10 | #52-#61; tres son condicionales (#52, #54, #60) |
| G5 | 15 | 13 de ellas con ⚑ |

A/I/P = SÍ en #9, 10, 14, 19, 31, 32, 35, 38, 45, 46, 53 y 55. Ninguna puede entrar en un bloque de aceptación.

## §2. Lo que de verdad bloquea FX-02 hoy (lista corta para el informe)

1. **A-4 AGREED en A4-1, A4-3 y A4-4** (#46, #45, #10).
   - Sin ella no se pueden escribir los puntos 5-7 de O4 (S02), no hay sonda ni S04, y S13-S23 quedan parados.
   - Estado: guardas mecánicas PENDIENTES (A-4 §6); el Architect está PENDING y la revisión no se ha lanzado (§61.2).
   - A4-2 solo hace falta para S26 y el PASS; A4-5, solo para S30, y es separable.
2. **El Coordinator de I-62 puede emitir ya una sola disposición ordinaria con los 16 G1⚑**, en paralelo a la revisión de A-4: no dependen de A-4.
   - Los 16: U-06 (a)(b), U-07, U-08, U-09 (a), U-10 (b), U-14, U-15, U-16 (1)(2)(4) con N8, U-17, U-18 (b), U-19, U-20, U-23 (a) y U-24.
   - Sin ellos, O4 no se puede publicar de forma que permita el PASS.
   - U-07 (TRX) y U-08 (`.gitignore`) exigen además commits en `fx/u1` y una corrida de CI antes de S02.
3. **U-14 (presupuesto, A/I/P = SÍ): se decide con comprobación y antes del veredicto de A-4.** Si «R cuenta», A2 no puede abrirse: A-4 no lo cubre y
   haría falta otra A-n más consumo del Owner por encima de OD-5.
4. **U-09 (e), hueco que la solicitud no trata:** quién decide A1'-A8' dentro de la ventana (T3 y F.1 paso 3). Bloquea S19. Va a Q-A4-11 o a una
   lectura del Coordinator.
5. **Sonda de A4-1 y su resultado.** Requiere A-4 AGREED, una disposición de aplicación (celda, shell, trío y directorio) y `A4-SONDA-CONSUMO = A`.
   Si falla, FX-02 queda UNVERIFIED (A4-1, regla 4) y solo queda V3.
6. **Owner:** `A4-SONDA-CONSUMO`, la apertura en S04 y los «continúa» de S09, S14 y S16.

**Para VERIFIED no bloquean:**
- A4-2 y `A4-CLAUDE-FX02-CONSUMO`, si U-11 = B. Sí hacen falta para el PASS si el oráculo incluye la revisión del Architect.
- A4-5, U-11, U-12, U-13 (a'), U-21, U-22, U-25 (b)(c), U-26, U-71 (b) y V3.

**Orden resultante:**

```text
guardas de A-4 → revisión → veredicto          ┐
disposición G1⚑ → commits TRX y .gitignore + CI ┘ → O4 (S02) → S03 → sonda A4-1 (consumo del Owner)
→ parte 1 de la tarjeta → S04
```

Opción que puede elegir el Coordinator: medir la sonda de A4-1 en un clon con historia distinto de `A2` desacopla la sonda de S02 y permitiría meter el
bloque de transporte en O4 (sin S13 ni el «continúa» de S14). El coste es dejar sin medir el riesgo de `read-only` en un directorio nuevo (README §5).

## §3. Preparación independiente

**Ya, sin autoridad pendiente:**

| Id | Acción | Por qué es segura ahora |
|---|---|---|
| P1 | Guardas de A-4, delta exacto frente a V14 y A-2, kit, cierre de insumos, auditor e independencia Actor/Session/Context. Es la cabeza del camino crítico | la ordena §61.2; no escribe en el fixture |
| P2 | `frontiers.json`. Cerrar F-UP-FX04A (§60.1), F-A2 (§57.1), F-OD-PROBE (§58.2; ev. §93) y F-OD-2D (§59). Marcar CD-26 «sin B3/B4 futuras» (§58.1, §60.1) y CD-28 (a) (§58.2). Dar S11 y S12 por hechos o sustituidos. Añadir F6-OBS-03 y F-A4 / F-A4-PROBE: A4-1 en S04, S13, S15-S16, S18 y S21b-S23; A4-3/A4-4 en S02, S15-S16, S18b y S21b-S22; A4-2 en S26; A4-5 en S30 (orden de §61.6). Actualizar `starting_point` (`codex_cli` y `fx04a`) | transcribe decisiones ya registradas en un archivo «no normativo»; no marca como decidido ningún G1⚑ |
| P3 | `launch-card-A2.md`. Citar §58.1/§60.1 en #0. En #8, huella `6518EFAB…`, binario `3553cd6e…` y app `26.1002.7124.0` (§59), con la medición pendiente de A4-1. Quitar «propuesta A2» de S11. LB-1: registros Codex de las sondas A2-P2 y de la futura sonda A4-1. **LB-4: añadir `D:\r62-fixture\B3`, que falta.** Actualizar las referencias de la cabecera a §57-§61 | son hechos; el texto inicial no cambia; la tarjeta no se entrega |
| P4 | Plantilla O4, solo como borrador en el staging. Actualizar las notas (CD-26 resuelta; S11 y S12 sustituidos). Rellenar los G1: `{EVIDENCE_DIR}`, `{CAPS_SCOPES}`, `{PUSH_RULE}` y `{EXEMPTIONS}` del Controller. Reescribir el punto 4: la causa ya no es P-01, sino F6-OBS-03. «Bloque posterior»: `ConfigSha256` `6518EFAB…`, `BinarySha256` `3553cd6e…` y `WorkingDirectory` `A2` (§55.10). Los marcadores G1⚑ y G3 se dejan abiertos | es transcripción del grupo (a) y de §55, §58 y §59; publicar es S02 y espera a las disposiciones; antes, búsqueda P-16 |
| P5 | Script del clon `A2` (S03: `--no-local`, `fx/u1`, `core.autocrlf=false`, identidad sintética, remotos `origin` y `github`) y comprobaciones #2, #4-#6 y #8 de la tarjeta (hashes y nombres, sin valores) | se escribe sin ejecutar; solo correrá tras S02 |
| P6 | Borradores, sin commit: el cambio de `fixture.yml` para U-07 (`--logger trx` y subida del artefacto a `fixture-tests`, sin renombrar jobs) y el `.gitignore` para U-08 (`artifacts/orchestration/`, `bin/`, `obj/`, `TestResults/`), con validación de sintaxis fuera de línea | no hay commit en `fx/u1` hasta la disposición (U-07 es una excepción a §50); en cuanto llegue, commit y CI de inmediato |
| P7 | Kit de la sonda de A4-1 en borrador: `cmd.exe` declarada; `git diff --name-only`, `git hash-object`, lectura de JSON y mapa de cláusulas (con la alternativa de Q-A4-03); huella, binario y versión antes y después, sin leer valores; esquema del resultado | §61.4 prohíbe gastar la sonda, no prepararla; se congela tras el AGREED |
| P8 | Plantilla del registro saneado de medición (U-15) y lista de búsqueda P-16 | borrador, sin publicar |
| P9 | Bloque de respuesta solo con los 16 G1⚑, sacado de §3 de la solicitud sin las partes de A-4 y con los SÍ marcados (U-14), para que el Coordinator los resuelva en paralelo a A-4 | es una propuesta, no una decisión |

**Tienen que esperar:**

| Id | Acción | Espera a |
|---|---|---|
| W1 | Publicar O4 (S02) | la disposición G1⚑, A-4 AGREED (puntos 5-7) y los commits de U-07 y U-08 |
| W2 | Commits de TRX y `.gitignore` y su corrida de CI | la disposición de U-07 y U-08 |
| W3 | Clon `A2` (S03) | S02 |
| W4 | Sonda de A4-1 | A-4 AGREED, disposición de aplicación con directorio y `A4-SONDA-CONSUMO = A`; si es en `A2`, tras S03 |
| W5 | Tarjeta del Owner con las dos líneas | texto exacto tras el veredicto de A-4 (memoria: «solo textos exactos») |
| W6 | Bloque de transporte (S13) | resultado de la sonda, U-15 y S10 |
| W7 | Completar y entregar la parte 1 de la tarjeta (S04) | S02, S03, S03b (U-14) y la sonda |
| W8 | RLA | U-11, A4-2 (`EligibleCells`) y, para lanzar, `A4-CLAUDE-FX02-CONSUMO` |
| W9 | Clon `arch02` | A4-2 y el QU LAUNCHING |
| W10 | Mutación nueva de N8 y re-sellado de `negatives.md` y `supervision-checks.md` (hoy `729fc3ef…`) | U-16 y A-4 (comprobaciones de A4-3 y A4-4) |

## §4. Solo el Owner (exacto)

1. **`A4-SONDA-CONSUMO`.** Tiene efecto solo con A-4 AGREED y la disposición del Coordinator que lo aplique. Se entrega el texto de la versión acordada.
   Línea literal de A-4 §7, blob `27ffa26b`:

   ```text
   A4-SONDA-CONSUMO = A (1 invocación read-only de codex-cli fuera de A2-P2, celda gpt-6-luna/high del Controller de FX-02 con la shell declarada cmd.exe, para el trío exacto 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68/26.1002.7124.0/6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32, con huella, binario y versión antes y después; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; no acepta su resultado)
   ```

2. **`A4-CLAUDE-FX02-CONSUMO`.** Va en la misma solicitud que el punto 1 (§61.5: el silencio no es aprobación).

   ```text
   A4-CLAUDE-FX02-CONSUMO = A (uso read-only de claude-cli como Architect de la topología A de FX-02, revisión del contrato de T1, con una celda medida elegible para ARCHITECT; 1 lanzamiento y las reejecuciones del pool de D.3, dentro de los mismos topes; Actor/Session/Context REQUIRED, Provider PREFERRED; sin PRINCIPAL_COORDINATOR, EXECUTION_CONTROLLER ni WORKER; sin escritura; sin lectura de credenciales; no acepta su resultado ni amplía OD-3, OD-5 ni CLAUDE-CLI-I62)
   ```

3. **OD-2 nueva, solo si la huella cambia** antes o durante la sonda o la ventana. Forma de §59:
   `OD-2 = A (línea base <SHA-256 de config.toml>; binario <SHA-256>)`.
4. **Q-A4-09, solo si la revisión de A-4 identifica una consecuencia OWNER-RESERVED.** Hoy no se pide nada.
5. **S04.**
   - Abre una sesión nueva en `D:\r62-fixture\A2` con `claude-opus-5-5` y effort `xhigh`; el modo de permisos lo elige el Owner, sin recomendación.
   - Pega como único mensaje:
     `Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden FX-U1-O4 de docs/automation/decisions/FX-U1.md.`
   - Está cubierto por OD-5 solo si U-14 = «R no cuenta».
6. **`continúa`**, exactamente esa palabra, en S09, S14 y S16, cuando la supervisión lo indique.
   - Solo en caso de un CHANGES REQUIRED tras S26: otro, después de la COORDINATOR_DECISION.
   - No contesta preguntas de A2. Aprobar o denegar los permisos de herramientas es decisión suya, y la supervisión registra cada caso.
7. **Cierra la sesión A2** cuando la supervisión lo indique, tras `isRunning` = false dos veces.

No se pide ahora: si U-14 resultara «R cuenta», haría falta una A-n y una autorización de consumo del Owner por encima de OD-5 (precedente
`B3-CONSUMO`, §58).
