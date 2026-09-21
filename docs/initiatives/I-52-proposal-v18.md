# I-52 — Proposal V18: reconciliación del STOP de CT-49

> **PROPOSAL V18 — REVIEW REQUIRED. Gate exclusivamente de diseño y documentación.**
>
> ```text
> Proposal Version = V18
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3A = STOP / HISTORICAL EVIDENCE
> G3 = STOPPED
> G3B = NOT OPEN
> ADR-0036 = PROPOSED
> Implementation = BLOCKED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V18 sustituye únicamente la arquitectura de autoridad de sesión afectada por el STOP de CT-49. No implementa
RACKMIRROR ni guardas productivas, no continúa G3, no ejecuta CT-50, no acepta ADR-0036 y no modifica el Freeze
V17, R3, Shared View Foundation o la autoridad I-49. Las reglas V17 no identificadas como sustituidas continúan
por referencia.

## 1. Estado e identidades

| Autoridad | Identidad / estado |
|---|---|
| Base de publicación | I-52 `560ec72a1cef16e18da5dd92a92eb5fb2af26469`; `origin/main` `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Proposal V17 | blob `dd944d8e9c62b083eebc2b1292789977b880c368`; consenso histórico |
| Freeze V17 | commit `87a0b72a159501a075105b9f6a80e33a083527ac`; blob `5f4933a3aa90d7052f571317107f1831ec02ac6e` |
| G3A | SHA `560ec72a1cef16e18da5dd92a92eb5fb2af26469`; recibo `docs/initiatives/I-52-g3a-ct49-session-host-api-characterization.md`; decisions §114 |
| O-1 | aceptado históricamente para V17; disposición V18: `REQUIRES_REDECISION` |
| ADR-0036 | `PROPOSED`; disposición V18: `NEEDS_AMENDMENT` |
| Freeze V17 | `HISTORICAL / SUPERSEDED FOR AFFECTED CT-49 CONTRACT` |
| I-55 observado | `023020f8228a84d7fc059cd08c7a38d49a117b6a`; impacto contractual `NON-MATERIAL` |

El SHA de publicación es el commit que contiene este archivo y se reporta externamente. Esta Proposal no se
autocalifica como acordada.

## 2. Por qué falló V17

V17 exigía un catálogo cerrado de modos persistentes y un detector específico o autoridad genérica con cobertura
demostrada para cada modo. G3A encontró detectores fiables para estados concretos y una autoridad de long
transaction, pero ninguna API pública que enumere semánticamente todos los entornos persistentes o pruebe cobertura
sobre contextos nativos, verticales y extensiones. Actividad de comandos, quiescence y 1,040 descriptores de
variables tampoco cierran ese universo.

La ausencia no puede transformarse en una lectura runtime E12: faltaría la autoridad cuya lectura permitiría
distinguir seguro de desconocido. Continuar con la lista conocida supondría que un contexto omitido es inocuo. Esa
suposición contradice el fail-closed congelado. CT-49 aplicó correctamente el STOP previsto por V17 y O-1.

## 3. Hechos aceptados de G3A

V18 acepta sin reinterpretar:

1. existen detectores específicos para REFEDIT, BEDIT, ARRAYEDIT Source y BTESTBLOCK;
2. LongTransactionManager observa el predicado de long transaction activa, no todas las sesiones persistentes;
3. no se demostró un registro semántico completo de modos persistentes;
4. no se demostró una autoridad genérica completa para contextos, pares open/close, verticales y extensiones;
5. CMDNAMES, CMDACTIVE y quiescence no prueban ausencia entre comandos;
6. 1,040 system variables no forman una taxonomía semántica ni una garantía de cobertura;
7. `HOST_AUTHORITY = CHARACTERIZED` para el tuple exacto medido;
8. `Overrule.HasOverrule(subject, RXClass)` ofrece granularidad por sujeto y clase en ese tuple;
9. el censo G-M24 del SHA G3A contiene 790 `READ_ONLY`, 340 `DECLARED_MUTATOR`, 229
   `EXCLUDED_WITH_REASON`, 0 `UNCLASSIFIED`, total 1,359; y
10. reactores de terceros permanecen fuera del footprint runtime de overrules.

## 4. Cláusulas V17 sustituidas con precisión

Solo quedan sustituidos los siguientes fragmentos:

| V17 | Sustitución V18 |
|---|---|
| §0.4, requisito de catálogo cerrado y `EditingSessionAuthoritySet` finito | §§6–10: autoridad positiva de aislamiento contextual; hoy no demostrada, por lo que la operación no es habilitable |
| §0.8, filas de modos que dependen del catálogo cerrado | §19: caracterización de canales de efecto y autoridad de aislamiento antes de cualquier reapertura |
| §4.1/ST-20, parte que obtiene PASS de ese conjunto de detectores | §§7–8: PASS requiere `ContextIsolationAuthority = TRUE`; hoy no existe PASS |
| PRE-19, solo su componente de política de sesión | §7; completitud de fuente y orden de relectura no cambian |
| E12, solo la rama basada en leer el conjunto cerrado de sesiones | §8; E12 runtime solo podrá existir después de caracterizar una autoridad suficiente |
| CT-49, solo el cierre por catálogo mode→detector/autoridad | CT-49R de §§14 y 19; el resto de CT-49 se conserva |
| T-M73, solo los escenarios de política de sesión | §19; las obligaciones de campos y XREF se conservan |
| L-34/M-35/R-57 y §19, solo donde presuponen el conjunto cerrado | §§6–10 y 18; sus demás límites y el fail-closed siguen vigentes |
| R-52/R-57, STOP por falta del catálogo | STOP cumplido históricamente; reapertura prospectiva requiere la evidencia de §14 |

No se sustituyen la política granular de overrules, G-M24, T-M75, campos, XREF, reflexión, read-set, copy-only,
identidad/restamp, atomicidad, UX, naming, CT-06, AUTH-15 o PlanReadSet.

## 5. Alternativas A–E

| Alternativa | Evaluación | Veredicto |
|---|---|---|
| A — Positive safe-state authority | Es la forma lógica correcta, pero los predicados conocidos no acotan por sí solos un contexto persistente desconocido que altere representación, DB o API consumida. Requiere una autoridad adicional de aislamiento de efectos. | `REJECTED AS CURRENT EXECUTION`; retenida como forma del contrato futuro |
| B — Command-boundary/session ownership | Un comando propio controla su lifecycle, pero no prueba qué entorno persistía antes, qué estado modeless sigue activo, ni qué extensión intercepta la operación. Nested y transparent commands agravan el límite. | `REJECTED` |
| C — Conservative known-mode blocking | Bloquea REFEDIT/BEDIT/ARRAYEDIT/BTESTBLOCK y long transactions, pero permite por omisión un modo desconocido capaz de invalidar semántica. | `UNSAFE / REJECTED` |
| D — Product scope reduction | Limitar a un tuple exacto elimina hosts no caracterizados, pero host identity no equivale a cierre de modos. Tampoco prueba ausencia de extensiones de terceros. | `INSUFFICIENT ALONE / REJECTED` |
| E — Defer/impossible under current guarantee | Conserva el fail-closed: mientras no exista autoridad positiva que cierre canales de efecto, no hay estado ejecutable soportado. | `SELECTED` |

A y D pueden formar parte de una propuesta futura si aparece evidencia externa suficiente. No son autorización de
implementación en V18.

## 6. Arquitectura elegida

V18 elige **ALT-E**. RACKMIRROR queda diferido bajo la garantía actual. El contrato ya no intenta demostrar que
“no existe ningún modo desconocido”; exige demostrar positivamente que todos los canales externos capaces de
alterar las observaciones relevantes están excluidos o representados por autoridades observables.

La nueva abstracción es `ContextIsolationAuthority`. No es una clase, API o guarda ya existente. Es una obligación
de autoridad: una fuente documentada debe demostrar que, para el host y alcance declarados, todo entorno contextual
que pueda alterar la DB fuente, la representación consultada, la resolución de símbolos o los miembros API
consumidos queda (a) imposible, (b) detectado con semántica completa, o (c) incluido explícitamente en el modelo de
lectura/huella. G3A demostró que las fuentes actuales no satisfacen esa obligación.

Por tanto, V18 no diseña una ruta parcial de ejecución. El resultado presente es `NO SUPPORTED EXECUTION
ENVIRONMENT`. Es una deferencia reversible si aparece una autoridad nueva; no es E12 permanente ni una afirmación
de imposibilidad universal de AutoCAD.

## 7. Modelo formal de estado seguro

Para documento `D`, host `H`, estado de sesión `S`, observaciones de overrules `O` y contexto externo `C`:

```text
SafeOperationalState(D,H,S,O,C) =
    HostExact(H)
  ∧ CurrentDocumentOwned(D)
  ∧ CommandBoundaryCompatible(S)
  ∧ KnownDetectorsInactive(S)
  ∧ NoLongTransaction(S)
  ∧ DatabaseComplete(D)
  ∧ ReadSetStable(D)
  ∧ OverrulesBounded(O)
  ∧ ContextIsolationAuthority(D,H,S,C)
```

Una conjunción solo puede ser TRUE si cada término es TRUE. `FALSE` o `UNKNOWN` en cualquier término impide
RACKMIRROR. En la evidencia actual `ContextIsolationAuthority = UNKNOWN`; por ello
`SafeOperationalState = FALSE_FOR_ADMISSION` aunque todos los demás términos observables parezcan favorables.

| Predicado | Fuente exigida | Alcance | Fallo de lectura | Suficiencia / UNKNOWN |
|---|---|---|---|---|
| `HostExact` | tuple de HostApplicationServices/Application y hashes/versión caracterizados | proceso, producto, build/API exactos | fail-closed futuro | necesario, no suficiente; UNKNOWN bloquea |
| `CurrentDocumentOwned` | DocumentManager y correspondencia document/database | documento invocante y DB fuente | bloquea | necesario; no prueba aislamiento contextual |
| `CommandBoundaryCompatible` | lifecycle documentado del comando y stack observable | invocación, nested/transparent policy | bloquea | necesario; no cubre estado persistente previo |
| `KnownDetectorsInactive` | REFEDITNAME, BLOCKEDITOR, ARRAYEDITSTATE, BLOCKTESTWINDOW | documento/host probado | bloquea | necesario; lista no completa |
| `NoLongTransaction` | LongTransactionManager para el documento | long transactions | bloquea | necesario; cubre solo ese predicado |
| `DatabaseComplete` | autoridad V17 de apertura/completitud y fuente | DB y fuentes declaradas | bloquea | conserva PRE-19 fuera de sesión |
| `ReadSetStable` | P0/R2/R3/R6/MUTATE, fingerprint y PlanReadSet por variable | lecturas realmente consumidas | E10/E11 futuro según fase | conserva V17/I-49; UNKNOWN bloquea |
| `OverrulesBounded` | `HasOverrule(subject,RXClass)` y soporte conservador | sujetos/clases relevantes del tuple exacto | bloquea | granularidad caracterizada; familia/efecto UNKNOWN bloquea |
| `ContextIsolationAuthority` | garantía pública del host o mecanismo caracterizado con cobertura de canales | nativos, modeless, verticales y extensiones que puedan afectar observaciones | bloquea antes de ejecutar | término decisivo; hoy UNKNOWN, por tanto no hay admisión |

“Normal”, “standard” o “sin editor especial” no son valores admisibles para ningún término.

## 8. Tabla de decisión runtime y de gate

Esta tabla especifica una arquitectura futura; V18 no implementa las lecturas.

| Momento | Condición | Resultado |
|---|---|---|
| V18 actual | `ContextIsolationAuthority = UNKNOWN` | STOP de arquitectura; no comando productivo, no E12 fingido |
| Recaracterización futura | existe fuente candidata, pero cobertura incompleta o no probada | CT-49R STOP; Proposal/Freeze no habilitan G3B |
| Recaracterización futura | todos los términos están demostrados para alcance exacto | elegible para nuevo consenso/freeze y reapertura de G3A; todavía no es implementación |
| Runtime futuro autorizado | cualquier lectura requerida falla o da UNKNOWN/FALSE en P0 | fail-closed con error asignado por el contrato revalidado |
| Relectura R6 futura | cambia una observación consumida | E10 según contrato preservado |
| Inicio MUTATE futuro | cambia/falla una observación consumida | E11 + rollback según contrato preservado |
| Host fuera del tuple autorizado | identidad legible pero no soportada | E12 futuro, solo después de caracterizar la autoridad |

## 9. Alcance de host y versión

La caracterización actual cubre exclusivamente el tuple AutoCAD 2025 medido por G3A. No se generaliza por major,
nombre comercial, DLL compatible ni familia API. Un host exacto es requisito necesario. No prueba por sí solo
cierre contextual y no convierte vanilla AutoCAD, un vertical o una instalación sin plugins declarados en estado
seguro. Cualquier propuesta futura debe definir además cómo demuestra la ausencia o mediación de extensiones que
puedan afectar las observaciones.

## 10. Modelo de modo y sesión

REFEDIT, BEDIT, ARRAYEDIT Source, BTESTBLOCK y long transactions siguen siendo estados conocidos que una futura
política deberá bloquear o modelar. Su detectabilidad no representa totalidad. Command stack y quiescence son
señales locales, no autoridades de sesión. Contextos modeless, persistent open/close, nested/transparent, verticales
y de terceros permanecen dentro del canal que `ContextIsolationAuthority` debe cerrar.

No habrá descubrimiento oportunista en runtime ni política “known list + allow otherwise”. Una nueva señal solo
reduce UNKNOWN cuando su predicado, alcance, errores y cobertura estén demostrados.

## 11. Interacción con overrules

Se conserva la política V17 y la caracterización G3A: consulta por sujeto y clase; clasificación de familia por
dibujo, transformación u ocupación; irrelevancia o no aplicabilidad demostrada no penaliza; UNKNOWN relevante
bloquea; indicador global no implica `EVERYTHING UNKNOWN`; reactores de terceros no son overrule footprint.

La granularidad de overrules no resuelve sesiones: un contexto puede alterar selección de DB, símbolo, evaluación o
representación sin registrar un overrule relevante. A la inversa, `ContextIsolationAuthority` no elimina la
obligación de `OverrulesBounded`.

## 12. Relación con campos y XREF

Se preservan la causalidad recursiva de campos, hojas EDIT fijas, hojas STATE variables y ciclos sin cota finita
como UNKNOWN. Se preservan las reglas de XREF, carga/completitud, fuente y read-set. Estos modelos caracterizan qué
leer y cuándo falla su representación; no prueban que el contexto externo no cambie la autoridad de esas lecturas.
Por ello son términos complementarios y no reemplazos de aislamiento contextual.

## 13. Carry-forward de G-M24 y T-M75

El censo histórico de 1,359 sitios y `UNCLASSIFIED = 0` queda citado como resultado exacto del SHA G3A, no como
prueba transferida al commit documental V18 ni a un futuro Candidato. El mecanismo de discovery arbitrario de
T-M75 se conserva: constructors, methods, getters/setters, events, operators, callbacks y handlers transitivos
permanecen dentro del universo; una familia nueva no escapa por no estar en fixtures.

La futura guarda sigue obligada a clasificar todo sitio alcanzable y a producir RED ante `UNCLASSIFIED`. Cualquier
cambio en Plugin, helpers alcanzables, referencias API o reglas del censo exige regeneración. Una futura evidencia
de Candidato debe ejecutarse sobre su SHA exacto conforme a AGENTS.md.

## 14. Representabilidad y reglas para reabrir G3

CT-50 no se ejecuta. G3B no se abre. La secuencia mínima es:

```text
V18 exact review → technical consensus
→ new Freeze that records ALT-E and CT-49R
→ O-1 redecision
→ external/new authority evidence becomes available
→ CT-49R exact characterization
→ if consistent, explicit G3 reopening decision
→ G3B / CT-50 later
```

Consenso y Freeze V18 por sí solos no reabren G3: congelan la deferencia. CT-49R debe demostrar
`ContextIsolationAuthority`, volver a verificar la compatibilidad de todos los términos del predicado y someter
cualquier cambio material a Proposal/review/freeze nuevos. Si no aparece autoridad, G3 permanece detenido.

## 15. Disposición O-1

```text
O1_DISPOSITION = REQUIRES_REDECISION
```

El fail-closed se preserva y fortalece, pero el alcance pasa de una operación futura condicionada por catálogo
cerrado a ningún entorno ejecutable actualmente soportado. Esa reducción material cambia el significado práctico de
L-34/M-35/R-57/R-58 y requiere decisión del Owner sobre deferir, cancelar o patrocinar nueva autoridad. La
aceptación histórica V17 permanece registrada y no se extiende a V18.

## 16. Disposición ADR-0036

```text
ADR0036_DISPOSITION = NEEDS_AMENDMENT
ADR-0036 = PROPOSED
```

La decisión central de espejo semántico por copia continúa siendo pertinente; no hace falta reemplazar el ADR. Sin
embargo, sus pasajes que mandatan catálogo finito de modos, `EditingSessionAuthoritySet` y cierre CT-49 por esa vía
son materialmente inconsistentes con ALT-E. Un gate ADR posterior debe enmendarlos después del consenso V18. Este
gate no edita ni acepta el ADR.

## 17. Disposición del Freeze

```text
V17 Freeze = HISTORICAL / IMMUTABLE
V17 Freeze = SUPERSEDED FOR AFFECTED CT-49 CONTRACT
NEW FREEZE = REQUIRED AFTER V18 CONSENSUS
```

El Freeze `87a0b72a159501a075105b9f6a80e33a083527ac` no se modifica. Sus autoridades no afectadas se llevan por
referencia. El nuevo Freeze deberá enumerar las cláusulas sustituidas, ALT-E, CT-49R, la disposición O-1/ADR y la
regla de que Freeze no reabre G3.

## 18. Invalidación y reutilización

Los hallazgos G3A pueden citarse bajo estas condiciones:

- host y overrule: mismo tuple exacto de producto/program/market/API, mismas versiones y hashes binarios y misma
  semántica pública; cualquier diferencia exige recaracterización;
- G-M24: conteos y clasificación solo describen el árbol/call-sites del SHA `560ec72...`; cualquier cambio
  alcanzable o evidencia de otro SHA regenera el censo;
- campos/XREF: solo mientras sus cláusulas V17 y superficies consumidas no cambien;
- T-M75: se conserva el mecanismo de descubrimiento, no un permiso para reutilizar evidencia exact-SHA;
- cualquier contradicción entre evidencia nueva y la Proposal exige STOP y evaluación material.

La cita arquitectónica no sustituye evidencia local, CI, runtime ni Owner Validation requerida sobre un futuro SHA.

## 19. Obligaciones de prueba y caracterización

No se crean pruebas en este gate. Antes de reabrir G3 deben existir:

1. **CT-49R:** fuente, predicado, scope, errores y demostración de cobertura de `ContextIsolationAuthority`;
2. **host matrix:** tuple exacto admitido y rechazo de cada host/build fuera de alcance;
3. **session matrix:** known modes activos/inactivos, entre comandos, cancelación/cierre, nested, transparent,
   modeless, vertical y extension contexts;
4. **authority adversarial cases:** un contexto no enumerado que afecte DB/API/representación debe bloquear o quedar
   mediado por la autoridad;
5. **overrule cases:** granularidad por sujeto/clase, familias relevantes, UNKNOWN y relectura;
6. **G-M24/T-M75:** censo completo, fixtures arbitrarios y `UNCLASSIFIED ⇒ RED` sobre el SHA correspondiente;
7. **T-M73:** causalidad recursiva, EDIT/STATE, ciclos, campos y XREF; y
8. **CT-50:** solo después de reabrir G3, matriz completa de representabilidad.

Una guardia de fuente no sustituye prueba conductual. Cualquier selección focal debe demostrar más de cero pruebas.

## 20. Impacto en Owner Validation

No existe comportamiento productivo V18 que el Owner pueda validar en AutoCAD. Owner Validation permanece
pendiente y dormida. La redecisión O-1 evalúa el nuevo alcance/deferencia; no reemplaza validación manual. Si una
arquitectura futura habilita ejecución, sus escenarios se redefinen contra el nuevo Freeze y se ejecutan sobre el
`FINAL_CANDIDATE_SHA` exacto, misma versión AutoCAD y misma biblioteca de bloques.

## 21. Migración desde V17

1. conservar V17, Freeze y recibo G3A como historia inmutable;
2. revisar V18 por SHA exacto;
3. alcanzar consenso técnico o publicar V19;
4. crear nuevo Freeze sin reescribir el anterior;
5. obtener redecisión O-1;
6. enmendar ADR-0036 mediante gate separado;
7. mantener G3 detenido hasta que exista autoridad externa nueva;
8. ejecutar CT-49R y solo entonces decidir si G3B puede abrirse.

No hay migración de código, datos, DWG ni pruebas en este gate.

## 22. V17 explícitamente sin cambio

Permanecen vigentes por referencia: `μ_k`; reflexión semántica; eje/hoja y matemáticas; mirror read-set y
PlanReadSet I-49 por variable; exposición; combinaciones soportadas; restricciones de fuente; completitud y
relectura; política granular de overrules; fields/XREF; G-M24/T-M75; axis acquisition; naming final; `NewRackId`;
copy-only; identity/restamp; atomicidad; UX/mensajes/errores; CT-06; AUTH-15; caller-owned creation;
materialización específica; Foundation AUTH-01..13; y Owner Validation futura. Ninguna se declara implementada.

## 23. Checklist de revisión

- [ ] Las identidades y hechos G3A son exactos.
- [ ] Las cláusulas sustituidas no amplían el delta más allá de CT-49.
- [ ] ALT-A..E están comparadas y ALT-E es necesaria con la evidencia actual.
- [ ] Cada término de `SafeOperationalState` tiene fuente, scope, error y semántica UNKNOWN.
- [ ] No se asume inocuo un modo desconocido ni suficiente un command boundary/host exacto.
- [ ] G3/G3B/CT-50 permanecen detenidos.
- [ ] O-1 exige redecisión; ADR-0036 exige enmienda y sigue PROPOSED.
- [ ] El Freeze V17 queda inmutable e histórico; nuevo Freeze requerido.
- [ ] Los resultados G3A se citan con límites exactos y no se transfieren como evidencia a otro SHA.
- [ ] No hay afirmación de implementación, Candidate, Owner Validation o integración.

```text
PROPOSAL V18 = PUBLISHED / REVIEW REQUIRED
TECHNICAL CONSENSUS = NOT REACHED
G3A = STOP / HISTORICAL EVIDENCE
G3 = STOPPED
ADR-0036 = PROPOSED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
