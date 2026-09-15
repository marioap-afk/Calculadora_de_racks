# I-56 G8 — Dry-run historico obligatorio de Workflow V2 Proposal V4

> **DRY-RUN, SOLO DOCUMENTACION.** Este informe reconstruye tres iniciativas ya cerradas como si hubieran
> nacido bajo Workflow V2 Proposal V4. No modifica esas iniciativas, no activa politica y no sustituye
> evidencia historica.
>
> Proposal bajo prueba: `94a4e446b76ff28d8a434252fb859c071cf11db5`
>
> Coordinator = AGREED
>
> Architect = AGREED
>
> Consensus = REACHED
>
> Owner = NOT REQUESTED
>
> Workflow V2 = NOT EFFECTIVE
> WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST

## 0. Metodo, alcance y limites

Se leyeron completos Proposal V4, Evidence Audit y las Architect Reviews V3/V4. La reconstruccion usa
Git, documentos versionados de I-53/I-53S, I-51 e I-48, sus decisiones y la historia de CI ya aceptada
en el Evidence Audit. Las clases son:

- **MEASURED**: observado directamente en Git, documentos o CI.
- **RECONSTRUCTED**: recompuesto de registros parciales concordantes.
- **INFERENCE**: resultado contrafactual de aplicar una regla de V4 a hechos historicos.
- **UNKNOWN**: la fuente no permite una conclusion responsable.

Los numeros de V4 describen obligaciones o una reconstruccion plausible, no hechos ejecutados. No se
inventan duraciones ni el numero de commits internos que habria elegido un executor. Una reduccion de
ceremonia nunca cuenta como evidencia de seguridad.

## 1. Caso A — I-53S / EXTENSION

### A1. Estado inicial, intake y unidad de entrega

| Pregunta | Reconstruccion | Clase |
|---|---|---|
| IDs funcionales | ID6, reutilizar una configuracion de cabecera; ID7, distribuirla a varios destinos | MEASURED |
| Agrupacion | Una iniciativa conceptual I-53: mismo objetivo observable y misma autoridad/fundacion de Header Mutation/Reconciliation | INFERENCE apoyada por I-53 y P-02 |
| Unidades | Fundacion E1 fundida por defecto con el primer consumidor visible, Selectivo; Dinamico permanece otra unidad por sistema, UI, archivos calientes y rollback distintos | INFERENCE |
| Arquetipo de la unidad Selectivo | Unidad bajo Freeze ya revisado, sin disparador propio M-01..M-08; funcionalmente EXTENSION | INFERENCE |
| Dependencia | Freeze conceptual I-53 y fundacion compartida, disponibles antes del trabajo Selectivo | MEASURED/INFERENCE |
| Hotspot | `RackSelectiveWindow.xaml.cs` y su XAML/pruebas; coordinacion con I-49 y documentos compartidos de cierre | MEASURED |
| Base historica | E1 ya estaba integrada cuando abrio I-53S | MEASURED |

P-02 elimina la unidad de fundacion aislada salvo que exista un limite de rollback, una secuenciacion
forzada o un consumidor externo que necesite E1 antes. Ninguna fuente demuestra ese criterio para el
caso historico. Por tanto la reconstruccion principal absorbe E1 en la unidad Selectivo. Si apareciera
esa dependencia, dividir seria legal y conservaria toda la evidencia Git por unidad.

### A2. Discovery Core y expansiones

| DC | Uso de evidencia conceptual | Obligacion sobre la base real de la unidad | Resultado |
|---|---|---|---|
| DC-1 comportamiento | Reusar objetivo y comportamiento congelado de I-53 | Re-verificar flujo Selectivo y comportamiento vigente | REVERIFICAR |
| DC-2 autoridad | Reusar autoridad conceptual de cabecera authored/configurada | Verificar simbolos dueños en codigo actual | REVERIFICAR |
| DC-3 persistencia | Reusar ausencia/camino persistente descrito por I-53 | Verificar DTO/store/legacy y que la unidad no cambia su semantica | REVERIFICAR |
| DC-4 mutacion | Reusar Prepare/reconciliacion conceptual | Recorrer entrada Selectivo → destinos → mutacion → recomputo/dibujo | REVERIFICAR |
| DC-5 consumidores | Reusar inventario conceptual | Buscar lectores actuales, sin limite de saltos para datos/artefactos persistidos | REVERIFICAR |
| DC-6 pruebas | Reusar obligaciones S23 y S27..S32 como punto de partida | Comprobar pruebas/guardas existentes y declarar huecos | REVERIFICAR |
| DC-7 paralelas/hotspots | No heredable | Fetch actual; I-49 y documentos compartidos | SIEMPRE ACTUAL |
| DC-8 fundacion | Reusar la entrada/fuente conceptual | Verificar E1 contra simbolos, pruebas y comportamiento en la base de I-53S | SIEMPRE ACTUAL |
| DC-9 M-01..M-08 | La hipotesis conceptual es ninguno | Evaluar cada disparador; UNKNOWN activa EXP-09 | SIEMPRE ACTUAL |

P-02 exige para DC-1..DC-6 `REVERIFICADA` o `SIN CAMBIO DEMOSTRADO` con inventario y diff vacio; el
nombre de archivo no basta. DC-7..DC-9 nunca se heredan. En la historia no aparece contradiccion de la
fundacion consumida ni disparador propio de I-53S, por lo que **ninguna EXP se reconstruye como
obligatoriamente activada**. Eso es INFERENCE, no prueba de que una ejecucion real no hubiera abierto
EXP-01, EXP-05, EXP-07 o EXP-09. El Coordinator habria revisado las nueve evaluaciones negativas antes
de confirmar agrupacion y arquetipo.

### A3. Participacion del Architect

El diseño conceptual tuvo dos rondas de Architect: V1 produjo H-01..H-07 y V2 produjo RR-01; despues
quedo congelado. I-53S consumio esa autoridad sin desviacion de diseño registrada. Bajo P-08 se trata
como **unidad bajo Freeze ya revisado**: no recibe una nueva ronda de diseño si el Discovery delta no
activa M-01..M-08. Si el Freeze V1 careciera de algun elemento exigido por P-09, la unidad produciria un
Freeze delta aditivo y la revision cubriria compatibilidad + delta. En todos los caminos conserva la
conformidad final completa de Architect + Coordinator.

La discrepancia B-31 pertenece a I-53D y no a la superficie Selectivo. La conformidad de I-53S no tiene
por que descubrir una afirmacion exclusiva del contrato de E3; P-19 si la haria visible al conformar
E3 contra su Freeze/delta. No se atribuye a I-53S una captura fuera de su alcance.

### A4. Freeze y matriz OV

La unidad consumiria las secciones aplicables del Freeze V1 conceptual I-53, con comprobacion de
compatibilidad P-02 y referencia versionada P-09. Su Freeze delta añadiria, si faltaran, obligaciones de
prueba, plan funcional y matriz OV; nunca contradiria el Freeze V1.

Los 14 escenarios historicos asignados a Selectivo permanecen requeridos:

`N01`, `LIVE_SOURCE`, `APPLY_ONE`, `APPLY_MANY`, `APPLY_ALL`, `STANDARD_THEN_CUSTOM`,
`SOURCE_DISAPPEARED`, `HEIGHT_CONFIRM_CANCEL`, `PERALTE`, `INDEPENDENT_COPIES`, `DRAWING_UPDATE`,
`RACKEDITAR_REOPEN`, `SAVE_REOPEN` y `BOM`.

P-14 los congela y asigna a esta unidad. READY-08 impide un hueco. Todos se ejecutan sobre
`FINAL_CANDIDATE_SHA`; una ejecucion sobre otro SHA no se hereda. Retirar o sustituir un escenario, o
quitar su ultima asignacion, sigue reservado al Owner.

### A5. Gates funcionales reconstruidos

1. **Fundacion + primer uso Selectivo**: Prepare/reconciliacion compartida y aplicacion observable a
   uno/muchos/todos los destinos Selectivo, con copias independientes, fallos tipados y pruebas.
2. **Integracion UI Selectivo completa**: origen live, cambio standard/custom, confirmaciones aplicables,
   recomputo/dibujo, reopen/persistencia/BOM. Puede ser el mismo gate si el resultado util solo existe
   junto; V4 no fuerza dos.

El dato historico demuestra un gate visible para I-53S. La division en dos es una opcion de rollback de
la unidad fusionada, no un requisito ni una division por helper/DTO/archivo. El conteo exacto V4 queda
**UNKNOWN (uno o dos)**.

### A6. Pruebas, Candidate y Owner

- Focales RED→GREEN con conteo mayor que cero para S23/S27..S32 y comportamiento visible del gate.
- Core Full local al cierre de cada gate funcional. UI Full local intermedia sigue LC-UI; el CI completo
  corre en cada push.
- El SHA de cierre se empuja como punta y su CI `push/ref/head_sha/jobs` se lee antes del gate siguiente.
- READY-01..09, rebase final, conformidad completa y arbol limpio preceden a fijar
  `FINAL_CANDIDATE_SHA`.
- Sobre ese SHA: Core Full, UI Full, Debug UI, Debug Plugin y CI exacto. La evidencia pre-commit que
  estampa el padre no vale; P-10/P-12 evitan la repeticion historica C-09 al imponer commit → limpio →
  evidencia.
- El DLL se verifica por `InformationalVersion` y SHA-256. El Owner ejecuta los 14 escenarios y la guia
  sobre ese mismo SHA.

### A7. Ceremonia que desaparece o se consolida

- Un reclamo, bootstrap, cierre, merge, CI post-merge y smoke de E1 desaparecen al fusionar E1 con su
  primer consumidor; los de la unidad Selectivo permanecen.
- Los Full hechos antes del commit y repetidos por sello del padre se sustituyen por la secuencia exacta
  P-10/P-12.
- El contrato de unidad pasa a delta y referencia el Freeze; deja de re-enunciar 18–22% del conceptual.
- Resultados de gate no requieren commits `-CLOSE` adicionales. La evidencia vive una vez en el archivo
  de evidencia y los hechos post-merge en el tag propuesto.
- ROADMAP, Candidate, merge `--no-ff`, CI post-merge, cobertura y Owner Validation permanecen por unidad.

### A8. Capturas TABLE B de I-53

| ID | Captura historica | Etapa V4 | Relacion temporal | Resultado |
|---|---|---|---|---|
| B-05 | Configurador Dinamico devuelve instancia obsoleta y pierde receta | DC-1/DC-4/DC-6 del conceptual; focal RED del gate E3 | SAME | PRESERVADA |
| B-19 | H-01..H-07, siete HIGH de diseño | Revision adversarial del Freeze conceptual P-08/P-09 | SAME | PRESERVADA |
| B-20 | RR-01, frescura al leer PREPARE | Re-revision completa + delta; REQUIRED por verificabilidad P-08 | SAME | PRESERVADA |
| B-31 | Contrato E3 promete aviso/confirmacion inexistente | Conformidad P-19 de E3 contra Freeze/delta y comportamiento | EARLIER que cierre historico | PRESERVADA |
| B-32 | Cobertura de E1 apuntada a MERGE_SHA y limpieza temprana | P-13 + P-20 y tag verificable; chequeo post-merge | EARLIER | PRESERVADA |
| B-33 | `main` avanza durante evidencia E3 | READY-04 / re-fetch P-20; ruta R completa | SAME | PRESERVADA |
| B-35 | Comentario falso repetido sobre RackBlockData | DC-8 verifica codigo; EXP-01 y FOUNDATIONS evita propagar prosa | SAME o EARLIER | PRESERVADA, LOW |
| B-36 | Referencias inexistentes a HANDOFF | P-16 referencia precisa y materializacion normativa | EARLIER | PRESERVADA, proceso |
| B-38 | Siete Owner rounds sin hallazgos | P-14 conserva OV; ausencia de hallazgos no reduce su valor | SAME | CONTROL CONSERVADO |

**CASE A = PASS.** Ninguna captura material relevante se pierde ni se retrasa. La cantidad exacta de
gates contrafactual es UNKNOWN y no se usa para el veredicto.

## 2. Caso B — I-51 / FOUNDATION EVOLUTION

### B1. Intake y arquetipo

I-51 resuelve ID15 como iniciativa propia: su objetivo de duplicacion multi-origen es distinto de I-50,
aunque ambas consuman identidad/restamp. DC-9 activa **M-05** porque extiende el contrato compartido de
restamp e identidad gobernado por ADR-0009/ADR-0034. Por P-03 es **FOUNDATION EVOLUTION**, aunque el diff
sea pequeño y no cree ADR. Hotspots: `RackEnvelopeRestamp`, `RackCloner`, `RackDuplicarCommands`,
`IRackKindHandler` y documentos compartidos con I-49/I-50.

### B2. Discovery y EXP

| Captura | Camino V4 | Resultado |
|---|---|---|
| B-01, N GUIDs para vistas hermanas | DC-2 autoridad + DC-4 restamp + DC-5 handlers/consumidores | PRESERVADA en Discovery |
| B-02, conteo MAX de referencias | DC-1 observable + DC-5 RACKLISTA/BOM sin limite de saltos | PRESERVADA en Discovery |
| B-03, divergencia Id/Name | DC-2 autoridades + DC-4 + DC-6 guardas | PRESERVADA en Discovery |

M-05 activa EXP-04 para consumidores mas alla de un salto/otro sistema; la ausencia inicial de prueba
conductual para preservacion activa EXP-05. El Coordinator revisa negativos EXP antes de clasificar.
Esto conserva los tres HIGH en la etapa historica mas temprana y abre el camino que la historia omitio
para B-30.

### B3. Architect

FOUNDATION EVOLUTION exige al menos una revision adversarial. B-29 (`newId` string permitia que
Cantilever generara otro GUID) satisface P-08 REQUIRED: cambia/verifica el contrato M-05 y deja el
invariante de identidad sin una API que lo haga ejecutable. La correccion `Guid` y NI-1..NI-6 entran al
Freeze. Si la reconciliacion cambia un elemento congelable, la version completa + delta vuelve al mismo
revisor; no se exige una segunda ronda por mera redaccion.

### B4. Freeze y B-30

El Freeze autocontenido incluiria autoridad, clave discriminada, semantica fail-closed, atomicidad por
destino, compatibilidad legacy, consumidores/BOM, `Guid` compartido, no-objetivos, gates, OV e
**invariante → prueba o guarda**. INV-09 (preservar View, Section, SchemaVersion y ExtensionData) no puede
quedar solo como prosa: EXP-05 y P-09 exigen obligacion de prueba; P-08(d) considera REQUIRED un oraculo
ciego y P-19 clasifica un invariante sin prueba como MATERIAL. Por tanto B-30 se detectaria antes del
Freeze o, como ultima barrera, en conformidad antes del Candidate, en vez de despues del merge.

### B5. Gates de comportamiento

1. **Plan de duplicacion observable y contrato de identidad**: grupos logicos, asignacion por destino,
   preservacion y conteos, probado en Application junto con restamp tipado. Se consolida el G3/G4
   historico porque separar planner y helper por capa no produce valor independiente para el usuario.
2. **Comando RACKDUPLICAR multi-origen completo**: seleccion, snapshot, mutacion atomica, transformacion,
   mensajes, varias referencias/destinos y comportamiento visible en AutoCAD.

Dos gates son una INFERENCE razonada. Uno tambien podria ser valido si el primer resultado solo fuera
util con el comando; tres por los archivos historicos no estan justificados por P-10.

### B6. Cadencia de pruebas

Historicamente se registraron cuatro Core Full (G3, G4, G5 y Candidate) y una UI Full (Candidate). Con
dos gates V4 exige dos Core Full de cierre; si el ultimo SHA, tras READY, permanece literalmente como
Candidate y la evidencia conserva clase/SDK/proposito, la ejecucion de esa misma clase puede reutilizarse
por exact-SHA; si cambia el SHA, corre otra vez. UI Full permanece en el Candidate. El numero reconstruido
es **Core 2 minimo, 3 si READY crea otro SHA; UI 1**. Focales RED de G-R1/G-R3b siguen obligatorias y
preservan B-41.

### B7. Camino a Candidate y Owner

Gate funcional → commit limpio → focales/relevantes/Core Full → push/CI exacto leido → revision del
Coordinator. Despues del ultimo gate: documentos visibles listos, rebase/preflight, CI de punta con
cuatro jobs, conformidad Architect + Coordinator, matriz OV y Freeze verificados, arbol limpio. Solo
entonces se fija `FINAL_CANDIDATE_SHA` y se producen Core/UI Full, builds Debug, CI exacto y bloque de
entrega. El Owner ejecuta M1..M9 + guardar/reabrir sobre ese SHA y DLL identificado. `MERGE_SHA`, CI
post-merge, cobertura y limpieza quedan en el tag durable P-20, cerrando el hueco historico.

### B8. Capturas TABLE B de I-51

| ID | Historico | Etapa V4 | Relacion | Resultado |
|---|---|---|---|---|
| B-01 | Discovery: N GUIDs | DC-2/4/5 + EXP-04 | SAME | PRESERVADA |
| B-02 | Discovery: conteo MAX | DC-1/5 | SAME | PRESERVADA |
| B-03 | Discovery: autoridad divergente | DC-2/4/6 | SAME | PRESERVADA |
| B-29 | Architect: string vs `Guid` | Revision adversarial P-08 | SAME | PRESERVADA |
| B-30 | Hallado por I-54 post-merge | EXP-05/P-09/P-19 | EARLIER | PRESERVADA Y ADELANTADA |
| B-41 | RED demostro guardas ciegas | RED→GREEN P-10 + P-08(d) | SAME | PRESERVADA |
| B-35 | Comentario falso repetido | DC-8/EXP-01/FOUNDATIONS | SAME o EARLIER | PRESERVADA, LOW |
| B-36 | Referencias HANDOFF falsas | P-16/materializacion normativa | EARLIER | PRESERVADA, proceso |
| B-38 | Owner sin hallazgos | P-14 conserva M1..M9 | SAME | CONTROL CONSERVADO |

**CASE B = PASS.** Las seis capturas propias se conservan; B-30 se adelanta. El numero exacto de gates
es INFERENCE y no sostiene el PASS.

## 3. Caso C — I-48 / NEW ARCHITECTURE

### C1. Arquetipo

I-48 introduce un kernel, registro y editor generico transversal de propiedades vinculables. Activa
M-07 de forma directa y M-01/M-02/M-04 en sus contratos; P-03 la clasifica **NEW ARCHITECTURE**. No puede
bajar por tamaño ni por afirmar bajo riesgo.

### C2. Discovery y expansiones

El Discovery Core debe abarcar comportamiento, autoridad authored/effective, persistencia `Type` y
valores, mutacion Link/Unlink/Repair/commit, todos los lectores/consumidores, pruebas, ramas, fundaciones
y M-01..M-08. La cardinalidad 1 variable → N propiedades de B-06 cruza DC-4/DC-5/DC-6 y semantica de
fallo. Si el executor vuelve a omitir su consecuencia, la revision obligatoria del Coordinator de
DC/EXP puede ordenar EXP-04/EXP-05/EXP-06, reproduciendo G1.1. B-06 queda como captura plausiblemente en
Discovery y, como minimo, antes del diseño confirmado.

### C3. AR1..AR8 bajo anti-churn

| Ronda | Hallazgo recuperable | Clase P-08 | REQUIRED/OPTIONAL | ¿Nueva ronda permitida? |
|---|---|---|---|---|
| AR1 | B-07, oraculo ciego a hardcodes; 1 BLOCKER/5 MATERIAL/2 MINOR | aspecto nuevo + contrato/prueba | BLOCKER/MATERIAL REQUIRED; editorial UNKNOWN | SI |
| AR2 | B-08, dos vinculos rotos bloquean Repair; 1 BLOCKER/4 MATERIAL/3 MINOR | aspecto nuevo sobre superficie antes fuera de alcance | REQUIRED | SI |
| AR3 | 2 MATERIAL/3 MINOR | detalle/consecuencia, desglose exacto no recuperable | MATERIAL REQUIRED; MINOR UNKNOWN | SI |
| AR4 | 2 MATERIAL/3 MINOR | consecuencia de reconciliacion | REQUIRED | SI |
| AR5 | 2 MATERIAL/3 MINOR | consecuencia de reconciliacion | REQUIRED | SI |
| AR6 | 2 MATERIAL/1 MINOR | consecuencia de reconciliacion | REQUIRED | SI |
| AR7 | B-09 y otro material historico: relectura evita acreditacion | consecuencia de afirmacion/mecanismo V7 | REQUIRED | SI |
| AR8 | 0/0/0, acuerdo exacto | confirmacion final | sin REQUIRED abierto | Cierra consenso |

La evidencia no permite reclasificar cada MINOR ni afirmar que alguna ronda desapareceria. AR4–AR7
tenian al menos un MATERIAL aceptado y satisfacen P-08(a)..(d); V4 las conserva. El posible ahorro esta
en podar detalle no congelable y agrupar erratas, no en suprimir una ronda que encontro un REQUIRED.

### C4. Freeze

Se congelan autoridad authored/effective, modelo Literal/Reference, identidad `VariableId`, persistencia
y mapping cerrado de `Type`, fallback/preservacion, Link/Unlink, fail-closed, reparabilidad atomica,
lookup unificado, re-acreditacion de commit, compatibilidad/legacy, no-objetivos, puntos de extension,
obligaciones de prueba con oraculos capaces de fallar, OV y resultados funcionales de gates.

No se congelan nombres de helpers, archivos exactos, orden interno de commits, forma privada del
resolver ni detalle mecanico que preserve los invariantes. La Proposal final debe ser autocontenida;
deja de depender de encadenar V1..V7 para conocer el acuerdo.

### C5. Gates funcionales reconstruidos

1. **Registro semantico + persistencia**: identidad, descriptors, snapshots, mapping cerrado, round-trip,
   duplicados/fallos.
2. **Kernel de mutacion y reparacion**: Link/Unlink/FindBroken/Repair, N→1, autoridad y acreditacion de
   relectura, con fallos atomicos.
3. **Editor e integracion de vistas**: editor generico, consentimiento por lote, insercion/reopen y
   preservacion de `PropertyValues`.
4. **Prueba real de generalidad**: `PalletTolerance`, migracion de `VerticalClearance`, flujo completo y
   comportamiento visible.

Cuatro coincide con los hitos historicamente verificables, pero es una INFERENCE. Correcciones B-10,
B-11/B-40 o B-12 no son gates planificados separados: bloquean y corrigen el gate funcional que
incumplen.

### C6. Revision del Coordinator

- B-10 aparece al revisar el gate 1 contra la gramatica persistida congelada.
- B-40 aparece al revisar el gate 2 contra autoridad authored y alcance de consentimiento.
- B-11 aparece al revisar el gate 3 recorriendo insercion/RACKEDITAR y preservacion congelada.

P-10 exige esta revision antes de abrir el siguiente gate. La conformidad P-19 queda como segunda
barrera; no sustituye la revision intermedia.

### C7. CI y B-12

Cada cierre de gate se empuja como punta propia y su CI completo se **lee** antes del siguiente. El rojo
de UI de G4E habria detenido la apertura de G4F. Logs, TRX, hangdumps y artefactos se leen antes de crear
un gate de correccion. B-12 se captura en el primer SHA donde se manifiesta, evitando que sobreviva dos
gates y evitando G4G.1/G4G.2 basados en hipotesis no contrastadas. Es EARLIER y reduce retrabajo sin
reducir seguridad.

### C8. READY y `acecde6`

`acecde6` no satisface READY-05: su CI propio no tiene los cuatro jobs en success. Tampoco puede fijarse
como `FINAL_CANDIDATE_SHA`; P-12 exige READY antes de nombrarlo. La historia puede conservarlo como
intento fallido, pero no como Candidate final. El ahorro de un Candidate invalidado es una consecuencia
plausible de reglas explícitas, no una afirmacion de que V4 impide todo intento futuro.

### C9. Owner y DLL

P-14 exige `InformationalVersion` con sufijo del Candidate y SHA-256 del DLL antes de entregarlo. El
bloque §7.1 se completa antes de OV. Esto detecta que un DLL de otro SHA no acredita el Candidate aunque
el arbol `src` coincida. B-13 queda abordado; el DLL historicamente cargado permanece UNKNOWN y no se
convierte retroactivamente en PASS.

### C10. Capturas tardias del Architect

B-08 era un aspecto nuevo material sobre una superficie que G1 habia llamado fuera de alcance: P-08
permite hallar material en cualquier seccion y no limita la revision al listado del autor. B-09 nace de
una afirmacion introducida por reconciliacion: la re-revision recibe version completa + delta y el delta
no limita alcance. AR4–AR7 conservan su ronda porque cada una tenia REQUIRED material. Las reglas
anti-churn solo podrian retirar una ronda si no quedara REQUIRED, condicion que la historia contradice.

### C11. Capturas TABLE B de I-48

| ID | Historico | Etapa V4 | Relacion | Resultado |
|---|---|---|---|---|
| B-06 | Coordinator G1.1 | Revision DC/EXP; EXP-04/05/06 | SAME o EARLIER | PRESERVADA |
| B-07 | AR1, oraculo ciego | P-08 REQUIRED(d), Freeze prueba | SAME | PRESERVADA |
| B-08 | AR2, Repair bloqueado | Revision completa, hallazgo en seccion intacta permitido | SAME | PRESERVADA |
| B-09 | AR7, relectura sin acreditacion | Version completa + delta; REQUIRED | SAME | PRESERVADA |
| B-10 | Coordinator post G4A | Revision de gate persistencia | SAME | PRESERVADA |
| B-11 | Coordinator post G4C | Revision gate UI/insercion | SAME | PRESERVADA |
| B-12 | CI rojo no leido | CI de cierre leido antes del gate siguiente | EARLIER | PRESERVADA Y ADELANTADA |
| B-13 | DLL de identidad incierta | P-14 InformationalVersion + SHA-256 antes de OV | EARLIER | PRESERVADA; hecho historico sigue UNKNOWN |
| B-40 | Coordinator post G4B | Revision gate kernel/editor vs autoridad/consentimiento | SAME | PRESERVADA |
| B-35 | Comentario falso repetido | DC-8/EXP-01/FOUNDATIONS | SAME o EARLIER | PRESERVADA, LOW |
| B-36 | Referencias HANDOFF falsas | P-16/materializacion normativa | EARLIER | PRESERVADA, proceso |
| B-38 | Owner sin hallazgos | P-14 conserva OV | SAME | CONTROL CONSERVADO |

**CASE C = PASS.** Ninguna captura material se pierde ni se retrasa; B-12/B-13 se adelantan. No se
afirma una reduccion de rondas Architect que la evidencia no sostiene.

## 4. Comparacion de ceremonia

`>=` conserva la cota historica del audit. Un rango V4 es una INFERENCE condicionada al numero de gates
o a que el SHA de cierre siga siendo el Candidate. Las corridas CI incluyen hechos historicos de rama;
el conteo V4 real depende de commits internos y por ello no se inventa.

| Campo | A historico I-53S | A V4 | Cambio | B historico I-51 | B V4 | Cambio | C historico I-48 | C V4 | Cambio |
|---|---:|---:|---|---:|---:|---|---:|---:|---|
| Discovery rounds | 0 propias | 1 delta | INCREASE formal; reuse acotado | 1 | 1 Core + EXP-04/05 | SAME/INCREASE acotado | 2 | 1 Core + expansiones; numero UNKNOWN | UNKNOWN |
| Architect rounds de diseño | 0 propias | 0 si delta sin M | SAME | 1 | >=1 | SAME o INCREASE si REQUIRED | 8 | hasta acuerdo; historia sostiene 8 | SAME |
| Implementation gates | 1 | 1–2 | SAME/UNKNOWN | 3 | 1–2 | REDUCTION probable | 10 + 5 diagnostico | 4 funcionales estimados; correcciones dentro | REDUCTION probable |
| Core Full local | 2 | 1–2 + solo otra si SHA cambia | SAME/UNKNOWN | 4 | 2–3 | REDUCTION probable | >=11 | >=4; final puede coincidir con ultimo cierre | REDUCTION probable |
| UI Full local | 2 | 1 final; intermedia LC-UI opcional | REDUCTION | 1 | 1 | SAME | >=10 | 1 final + cualquier intermedia voluntaria | REDUCTION normativa |
| Candidate attempts | 1 | 1 planificado; real UNKNOWN | SAME/UNKNOWN | 1 | 1 planificado; real UNKNOWN | SAME/UNKNOWN | 2 SHAs + 2 no commit | 1 planificado; real UNKNOWN | REDUCTION probable |
| CI attempts rama | 4 | UNKNOWN; al menos cierre(s)+Candidate si distinto | UNKNOWN | 9 (8 push+dispatch) | UNKNOWN | UNKNOWN | 27 | UNKNOWN; cada push sigue CI | UNKNOWN |
| Owner rounds | 1 | 1 final | SAME | 1 | 1 final | SAME | 1 | 1 final | SAME |
| Git units | E1 + I-53S = 2 para fundacion+Selectivo | 1 por fusion | REDUCTION | 1 | 1 | SAME | 1 | 1 | SAME |
| Cierre/doc ops | E1 e I-53S, copias multiples | 1 cierre concentrado + tag | REDUCTION | 1 cierre, evidencia copiada y post-merge ausente | 1 cierre + evidencia/tag | SAME en operaciones, REDUCTION de copia | 1 cierre, 5 copias y PENDING | 1 cierre + evidencia/tag | SAME en operaciones, REDUCTION de copia |

Esta tabla no cuenta conformidad, Freeze, READY o CI leido como sobrecarga prescindible: son controles
de seguridad propuestos. Tampoco trata el CI post-merge por SHA nuevo como redundante eliminable.

## 5. Matriz de seguridad consolidada

| Caso/captura | Etapa historica | Etapa V4 | Tiempo | Analisis |
|---|---|---|---|---|
| A B-05 | Discovery | Discovery conceptual/delta | SAME | Defecto real conservado |
| A B-19/B-20 | Architect V1/V2 | Revision/re-revision completa | SAME | Todos REQUIRED |
| A B-31 | Gate/cierre E3 | Conformidad E3 | EARLIER | Corrige contradiccion documental antes de Candidate |
| A B-32 | Integracion | P-13/P-20 | EARLIER | Evita cobertura sobre objeto incorrecto |
| A B-33 | Candidate | READY-04/P-20 | SAME | Ruta R conserva exact-SHA |
| B B-01..03 | Discovery | DC/EXP | SAME | Captura mas temprana conservada |
| B B-29 | Architect | Revision adversarial | SAME | API tipada queda congelada |
| B B-30 | Iniciativa posterior | EXP-05/Freeze/conformidad | EARLIER | Cierra hueco de prueba antes de Candidate |
| B B-41 | RED focal | RED→GREEN focal | SAME | Oraculo debe demostrar fallo |
| C B-06 | Coordinator G1.1 | Revision Coordinator de DC/EXP | SAME/EARLIER | Control distinto de Architect |
| C B-07..09 | AR1/AR2/AR7 | Revision completa + delta | SAME | Anti-churn no poda REQUIRED |
| C B-10/B-11/B-40 | Coordinator entre gates | Coordinator por gate | SAME | No se difiere a conformidad |
| C B-12 | CI G4E/G4F, leido en G4G | CI leido al cerrar primer gate rojo | EARLIER | Menos retrabajo; misma clase de evidencia |
| C B-13 | Audit posterior | Identidad DLL antes de OV | EARLIER | Evita atribucion por arbol/timestamp |
| Transversal B-35/B-36 | Discovery/cierre | DC-8/P-16 | SAME/EARLIER | LOW/proceso, conservadas |
| Transversal B-38 | Owner | Owner final | SAME | 0 hallazgos no autoriza eliminar OV |

**Resultado de matriz:** `MATERIAL LOST = 0`; `MATERIAL LATER = 0`. Las capturas adelantadas usan la
misma evidencia o una barrera adicional y no sustituyen controles posteriores.

## 6. Hipotesis H1..H9

| H | Evidencia A/B/C | Supports | Contradicts | UNKNOWN | ¿Cambio V4? |
|---|---|---|---|---|---|
| H1 rondas proporcionales | A 0 propias bajo Freeze; B una; C ocho con REQUIRED | Si: escala con novedad/hallazgos | No | Efecto causal futuro | NO |
| H2 CI leido al cierre | C habria detenido B-12 en G4E | Si, captura mas temprana | No | Fallos que solo aparezcan al Candidate | NO |
| H3 Core en cierre/Candidate | A evita sello del padre; B/C reducen repeticiones sin quitar final | Apoya forma moderada | No captura historica de Full local contradice | Valor contrafactual Core-Windows | NO |
| H4 re-fetch definido | A B-33 queda detenido en READY; hotspots no se remiden por gate | Si | No | Frecuencia futura de carreras | NO |
| H5 cierre concentrado | Los tres eliminan copias/PENDING, preservan fuente unica/tag | Si | No | Coste operacional del tag | NO |
| H6 referencia sobre repeticion | A contrato delta; B/C no re-pegan reviews/reglas | Si | Ordenes historicas silenciosas causaron desvios; P-16 exige referencia precisa | Cumplimiento real | NO |
| H7 registro fundaciones | A verifica E1; B reutiliza restamp; C introduce Linked Properties | Si, reduce re-descripcion con DC-8 fail-closed | B-35 prueba que copiar prosa seria peligroso, ya cubierto | Obsolescencia futura/eficacia | NO |
| H8 unidades funcionales | A fusiona fundacion sin uso; B/C agrupan capas en resultados | Si | No | Conteo exacto de gates | NO |
| H9 modo/independencia | Historia fue SAME-SESSION ROLE; V4 lo hace visible | Apoya medir el modo | No comparacion controlada | Valor causal de independencia | NO |

Ninguna hipotesis queda “probada” por el dry-run. H9 permanece UNKNOWN en su efecto, como exige el
audit; no hace falta cambiar V4 porque U-08 ya reserva la politica general y P-08 obliga registrar modo.

## 7. Stress test del paquete de decisiones del Owner

El dry-run no decide ninguna fila. Evalua si el paquete existente basta y si alguna decision quedo
innecesaria, ausente, ambigua o sobrecargada.

| ID | Prueba A/B/C | Resultado del stress test |
|---|---|---|
| OWN-A | A EXTENSION bajo Freeze, B FE por M-05, C NA por M-07 | NECESARIA; limites correctos, sin gobernar evidencia |
| OWN-B | 0/1/8 rondas y conformidad universal | NECESARIA; separa diseño proporcional de seguridad final |
| OWN-C | Cadencia A/B/C conserva Core final y cierres | NECESARIA; no sobrecargada |
| OWN-D | `acecde6` y B-33 prueban READY/invalidacion | NECESARIA |
| OWN-E | Casos historicos no son reclamos nominales I-49/I-52/I-55 | NO EJERCITADA; sigue necesaria para transicion, no falta dato del dry-run |
| OWN-H | B-32, MERGE_SHA ausente y tags propuestos | NECESARIA; incluye accion administrativa previa ya explicita |
| OWN-J | A fusiona E1; B/C quedan una unidad | NECESARIA y no ambigua en estos casos |
| OWN-K | B-35 y re-descripcion de fundaciones | NECESARIA; Owner decide existencia/ubicacion/proposito, no hechos tecnicos |
| OWN-L | Conflictos de autoridad/fuente en DC-8/P-17 | NECESARIA; sin decision factual delegada al Owner |
| OWN-M | 14 escenarios A y OV final B/C | NECESARIA; asignacion y retiro estan separados |
| OWN-N | V2 es decision arquitectonica normativa | NECESARIA; no se acepta tacitamente |
| OWN-O | Materializacion requeriria editar AGENTS | NECESARIA como autorizacion expresa; no ejercida en G8 |
| OWN-P | Pausa transversal no aparece en A/B/C historicos | NO EJERCITADA; alcance ya explicitado, no se decide aqui |
| OWN-Q | Los tres reclamos recuperados no prueban legacy ambiguo | NO EJERCITADA; sigue necesaria para casos sin Claim-Id |
| OWN-S | Ningun caso prueba reversion de politica | NO EJERCITADA; sigue necesaria para desactivacion segura |

No aparece una decision nueva. OWN-E/P/Q/S no pueden validarse empiricamente con estos casos, pero su
materia de transicion sigue identificada y no contradice la reconstruccion. Ninguna fila esta
sobrecargada de detalle tecnico que deba decidir Coordinator/Architect. Cobertura permanece informacion
`[KEEP]`, fuera de la tabla de decisiones y sin respuesta separada.

Resultado por defecto observado: **ninguna decision es innecesaria, ninguna decision nueva falta,
ninguna ambiguedad nueva aparece y ninguna fila queda sobrecargada por estos tres casos**. Las cuatro
filas sin ejercicio empirico quedan declaradas como cobertura parcial, no aprobadas por ausencia de
evidencia.

**OWNER-PACKAGE STRESS TEST = PASS, con cobertura empirica parcial declarada para OWN-E/P/Q/S.**

## 8. Architect check del dry-run

```text
Review object = este documento, antes de su commit
Review mode   = SAME-SESSION ROLE
Reviewer author of text = YES
Independence claimed = NO
Scope = read-only check del dry-run; no nueva Proposal review
Execution = despues del borrador inicial, antes del commit
```

Checklist adversarial:

| Comprobacion | Resultado |
|---|---|
| Capturas I-53/I-51/I-48 omitidas | Ninguna propia/material de los casos; B-35/B-36/B-38 transversales incluidas |
| PASS derivado de evidencia ausente | No: conteos/gates no recuperables son UNKNOWN o INFERENCE |
| Uso correcto de V4 | P-02/03/05/08/09/10/11/12/14/15/16/19/20 aplicadas sin activar politica |
| Reduccion ceremonial usada como seguridad | No; matrices separan ceremonia y capturas |
| Captura material perdida o mas tarde | Ninguna |
| Contradiccion de Proposal expuesta | Ninguna |
| Cambio semantico requerido | Ninguno |

**ARCHITECT DRY-RUN CHECK = PASS (SAME-SESSION ROLE; no independiente).** Este check no modifica el
veredicto previo del Architect sobre Proposal V4 ni crea uno nuevo.

## 9. Veredicto

| Caso | Veredicto | Base |
|---|---|---|
| A — I-53S / EXTENSION | **PASS** | Freeze conceptual reutilizado con delta/reverificacion; 14 OV finales; 0 capturas perdidas |
| B — I-51 / FOUNDATION EVOLUTION | **PASS** | M-05 preserva Discovery/revision; B-30 se adelanta; 0 capturas perdidas |
| C — I-48 / NEW ARCHITECTURE | **PASS** | Rondas REQUIRED conservadas; Coordinator/CI/READY/DLL preservan o adelantan capturas |

Condiciones globales:

- capturas materiales perdidas: **0**;
- contradicciones de Proposal: **0**;
- nuevas decisiones Owner requeridas antes del paquete existente: **0**;
- restricciones vinculantes I-56 violadas: **0**;
- Proposal V5 requerida: **NO**.

```text
DRY-RUN = PASS
CONSENSUS V4 = PRESERVED

OWNER = NOT REQUESTED
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
