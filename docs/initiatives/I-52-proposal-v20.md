# I-52 — Proposal V20: frontera semántica cerrada y materialización derivada

> **PROPOSAL V20 — PUBLISHED / REVIEW REQUIRED. Diseño y descubrimiento; no implementación.**
>
> ```text
> Selected architecture = RACKCAD-OWNED CLOSED SEMANTIC SNAPSHOT + TRANSACTIONAL DERIVED MATERIALIZATION
> V18 affected contract = MATERIALLY SUPERSEDED IF V20 AGREED
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V20 cambia la frontera del problema. No intenta demostrar que todo AutoCAD está aislado. Propone demostrar
positivamente que la transformación semántica sólo consume estado autoritativo propiedad de RackCad, producir un
snapshot tipado e inmutable, y tratar las entidades AutoCAD como una proyección derivada que se persiste junto con el
payload semántico en una sola transacción del dibujo. La arquitectura seleccionada es un híbrido de ALT-A, ALT-B y
ALT-E. Es viable como contrato de diseño, sujeto a las obligaciones y restricciones de esta propuesta; no existe aún
como producto.

## 1. Estado e identidades exactas

| Autoridad | Identidad / estado |
|---|---|
| I-52 inicial y Proposal V19 | `8c5221deb7b6f2782726a2f91b4e34c7f27843b9` |
| Proposal V19 | blob `17fb0a3eddb3461806bc91c34b397a0d16081a74` |
| paquete Architect V19 | blob `b504bc13b9bf054f552fbb8245f9b7bb6722f388` |
| decisions antes de V20 | blob `9f64135c408365b5e35a72e2b84d192aad89e82c` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Proposal V18 | SHA `1e9d7e39a50e5c6322e1d32823aa9f3130a2d0ae`; blob `827559b504ec6bc8b5f780267a40766c3fa6db8c` |
| Consensus Freeze V18 | SHA `faaf709bf6401dcda91b07f41afe44f490fb916a`; blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4` |
| ADR-0036 V18 | publicación `89ae56137adac138acc522fae0717d4ef75abbe3`; blob `5628947673f2afddf141c0505e92283962459443`; `PROPOSED / AMENDED FOR V18` |
| investigación contextual | SHA `569e019d52adbc45498d1fd37716e5ed6450642d`; `COMPLETE / PARTIAL_ONLY` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 observado inicialmente | `705ae301f696b7f16c8ec006e2b1ab526be46489`; G9a cerrado |
| I-55 re-fetch durante V20 | `fa8f6affa769f87afe795fd310fbc74c43494123`; integración Insertar/G9b |

El SHA y blobs propios de V20 se reportan después del commit. El preflight encontró árbol limpio, upstream exacto,
sin stash ni operación Git incompleta.

## 2. Cierre durable de V19

La sección 121 del registro histórico conserva las dos revisiones exactas recibidas:

```text
Coordinator = AGREED WITH PROPOSAL V19 — ALT-2 REJECTED
Architect = AGREED WITH PROPOSAL V19 — ALT-2 REJECTED
TECHNICAL CONSENSUS V19 = REACHED
ALT-2 = REJECTED
V18 = GOVERNING
```

V20 no reabre `ObservableCommitConsistency`. Lo conserva como condición defensiva de materialización, nunca como
autoridad suficiente de clausura contextual.

## 3. Reencuadre del problema

La pregunta deja de ser «¿podemos impedir u observar todo código de AutoCAD?» y pasa a ser:

> ¿Puede RackCad calcular, validar y persistir el espejo desde datos semánticos propios, de modo que AutoCAD sea el
> almacén transaccional y el materializador de una proyección derivada?

La respuesta de diseño es **sí, de forma condicionada**. Sólo se admite un rack cuando un adapter tipado por `kind`
demuestra que todos los inputs de `μ_k` pertenecen al conjunto semántico cerrado. Una geometría arbitraria, custom
object, field, XREF, overrule o extensión externa nunca se convierte en input semántico por aparecer en el dibujo.
Si un adapter necesita uno de esos inputs, ese rack queda fuera del alcance hasta que exista una autoridad RackCad
tipada y verificable para él.

## 4. Grafo de autoridad

```text
PhysicalSelection
  --READ / HOST-DEPENDENT--> RackIdentity + selected placement facts
  --READ--> AuthoredSemanticAuthority
  --PURE--> ClosedSemanticSnapshot_k
  --PURE--> μ_k(snapshot)
  --PURE--> MirroredSemanticAuthority + one NewRackId
  --PURE--> RackPreparedView<TPayload> / MaterializationPlan
  --PERSIST + MATERIALIZE / HOST-DEPENDENT, one transaction--> AutoCADEntities + embedded authority
```

| Arista | Regla |
|---|---|
| selección → identidad | la referencia física localiza definición, `RackId` y vista; no aporta geometría semántica |
| identidad → autoridad | todas las hermanas se leen y acreditan; divergencia o ilegibilidad bloquea |
| autoridad → snapshot | adapter tipado por kind; copia cerrada, schema legible, metadata preservada |
| snapshot → `μ_k` | pura, determinista y sin Autodesk API |
| resultado → plan | Foundation aporta addresses, frames, transforms, preparación y requisitos |
| plan → DWG | AUTH-15 crea/restampa payload y entidades en una única transacción caller-owned |

El bloqueo V18 aparece hoy cuando se pretende que la corrección completa dependa de toda la ejecución host. V20 lo
saca del núcleo semántico. El borde host permanece en selección, lectura/persistencia DWG y materialización.

## 5. Dos correcciones distintas

```text
SemanticMirrorCorrect =
    MirroredAuthoredState == μ_k(SourceAuthoredState)
  AND one NewRackId per logical source/destination
  AND typed bindings + metadata + grouping are preserved/reflected by contract

MaterializationCorrect =
    AutoCAD views faithfully project the persisted mirrored authority
  under the supported RackCad materialization contract.
```

`ContextIsolationAuthority` no es necesaria para probar `μ_k` sobre un snapshot cerrado. Tampoco se sustituye por
otro nombre: la clausura se prueba por propiedad y trazabilidad de cada input. La materialización requiere host
soportado, modos inseguros conocidos inactivos, lock documental, preparación completa, una transacción, rollback y
resultado explícito. No promete que una extensión externa jamás cambie después la representación.

## 6. Tabla de autoridad de datos

| Dato | Autoridad | RackCad-owned | Uso | Snapshot/validación | ¿semántica AutoCAD externa puede alterarlo? |
|---|---|---:|---|---|---|
| `RackId` | envelope RackCad acreditado entre hermanas | sí | semántica/agrupación | sí; GUID y unicidad | sólo cambiando bytes RackCad: corrupción |
| authored payload | DTO tipado de cada kind | sí | semántica | sí; schema + comparator | no sin cambiar estado RackCad |
| project domain | `RackProjectDocument` tipado embebido cuando aplica | sí | semántica | sí | no; un archivo externo no es autoridad del rack DWG |
| project variables | `RACKCAD_PROJECT` en NOD | sí | semántica si se leen | sí; snapshot + `PlanReadSet` | sólo mutando el Xrecord RackCad |
| variable bindings | authored DTO + autoridad I-49 | sí | semántica | sí; por propiedad leída | no sin cambiar estado RackCad |
| sections/profiles | IDs/valores de catálogo RackCad resueltos | sí, si catálogo aprobado | semántica/dibujo | sí; dependencia declarada | recurso cambiado invalida preparación |
| `RackViewAddress` | Foundation codec/address | sí | agrupación/dibujo | sí; typed/canonical | no |
| source transform | `BlockReference` seleccionado | no | colocación `P`, no `μ_k` | sí como fact de materialización | sí; cambio previo exige nueva lectura/aborto |
| block definitions | DWG generado/importado | parcialmente | dibujo | inventariable | sí; no es input de `μ_k` |
| dynamic properties | definición/materialización | parcialmente | dibujo | sólo si plan las declara | sí; prohibidas como semántica implícita |
| layers | DWG | no | dibujo | sí para acceso/estilo | sí; no es input de `μ_k` |
| fields | AutoCAD | no | excluido de semántica cerrada | sólo fail-closed | sí; dependencia semántica ⇒ no admisible |
| XREF | AutoCAD/externo | no | excluido de semántica cerrada | sólo fail-closed | sí; dependencia semántica ⇒ no admisible |
| overrules | tercero/host | no | representación | no como autoridad | puede cambiar vista, no el DTO cerrado |
| library resources | catálogo/biblioteca aprobada | mixto | materialización | requisito/versionado antes de mutar | sí; mismatch bloquea materialización |
| source entity geometry | DWG | no | locator/placement solamente | se puede leer, pero no autoriza `μ_k` | sí; se ignora como semántica |
| physical view representation | DWG | no | proyección derivada | regenerable | sí; corrupción reparable si autoridad sana |
| metadata/`ExtensionData` | envelope/DTO RackCad | sí | round-trip/compatibilidad | sí; preserve/include-by-default | sólo mutando bytes RackCad |
| `DimensionViews` | authored DTO tipado | sí | semántica/exposición | sí | no |

## 7. ALT-A — Detached Semantic Mirror

**Sobrevive como componente, no sola.** El repositorio ya separa DTOs/stores y builders de Application de la API
Autodesk. El snapshot puede contener authored DTO, metadata, bindings resueltos, `PlanReadSet`, view addresses y
dependencias de catálogo. `μ_k` y validación son puras. No basta tomar «el primer payload»: hay que acreditar todas
las hermanas y cerrar inputs. Sin ALT-B, el snapshot podría capturar una interpretación externa sin saberlo.

Selective usa `SelectivePalletDesignDocument`. Otros kinds usan stores tipados, varios mediante
`RackProjectDocument`; no se unifican en un objeto/JSON universal. Se requiere `ISemanticMirrorAdapter<TDocument>` o
equivalente por kind, con resultado común mínimo de identidad, diagnóstico y planes tipados.

## 8. ALT-B — RackCad-Owned Closed World

**Sobrevive como criterio de admisión.** La prueba es positiva:

```text
RackCadSemanticClosure_k(rack) =
    AllSiblingEnvelopesReadableAndConsistent
  AND AuthoredStateOwnedAndTypedByRackCad
  AND EveryMuInputDeclaredByAdapter_k
  AND AllBindingsResolveThroughRackCadAuthorities
  AND PlanReadSetCarriesEveryActuallyReadVariable
  AND ViewAddressesTypedAndCanonical
  AND NoSemanticDependencyOnFieldXrefCustomObjectOrExternalEvaluator
  AND CatalogDependenciesResolvedThroughApprovedRackCadPorts
```

No incluye «no se encontraron plugins». Un plugin puede estar cargado y ser irrelevante. Un rack deja de estar
cerrado cuando su adapter consume semántica externa, aunque no haya plugins visibles.

## 9. ALT-C — Project-First / Offline Mirror

**Rechazada como arquitectura primaria.** Existe `RackProjectStore` puro y tipado, pero el rack dibujado conserva su
autoridad de reapertura en el payload embebido de cada definición; no existe hoy un vínculo transaccional general con
un archivo de proyecto externo. Persistir primero un archivo y después el DWG crea los casos imposibles de resolver
atómicamente con AutoCAD UNDO: archivo confirmado/DWG fallido y DWG confirmado/archivo fallido.

El componente offline sí se reutiliza para construir `μ_k(snapshot)` en memoria. La persistencia autoritativa del rack
interactivo sigue dentro del DWG para participar en la misma transacción y en UNDO.

## 10. ALT-D — Two-Phase Intent + Materialize

**Rechazada como persistencia de producto.** Un `MirrorArtifact` inmutable en memoria es útil para PREPARE, pruebas y
diagnóstico. Persistirlo antes de materializar introduce racks pendientes, recuperación, idempotencia y dos
autoridades. No hay transaction coordinator entre filesystem y AutoCAD. V20 prohíbe un intent durable silencioso.

Si la materialización falla, el artefacto en memoria se descarta y no existe rack destino. Un mecanismo futuro de
cola/retry sería otra propuesta de producto, con UX, schema y decisión Owner propios.

## 11. ALT-E — Controlled RackCad Materializer

**Sobrevive como componente.** Recibe sólo el resultado semántico cerrado y un plan Foundation ya preparado. El
materializador no interpreta geometría fuente ni decide política de `μ_k`. Usa recursos aprobados, lock documental y
una transacción caller-owned para crear todas las vistas, payloads y un solo `RackId`.

La garantía mínima no es «best effort». Es **all-or-nothing para las escrituras que RackCad controla: estado RackCad
persistido y entidades destino dentro de la transacción**. Una escritura directa de tercero, incluso síncrona dentro
de la ventana del comando, es interferencia externa fuera de esa garantía y puede corromper la proyección o los bytes
RackCad. V20 no afirma detectarla siempre: esa reducción expresa del contrato requiere decisión Owner. Imports de
PREPARE sólo son admisibles bajo el contrato V19: idempotentes/compartibles, sin
RackId, vistas, payload, BOM o cambio semántico de racks existentes. Purga/regen posterior no es authored state.

## 12. ALT-F — Keep V18 / defer

Es el fallback gobernante hasta que V20 alcance consenso, nuevo Freeze y decisión Owner. Conserva seguridad, pero
deja el producto diferido aun cuando el núcleo semántico pueda probarse sin aislamiento global.

## 13. ALT-G — Abandon RACKMIRROR

No seleccionada. El código demuestra suficientes autoridades tipadas y seams transaccionales para una alternativa
defendible. La viabilidad está condicionada a cerrar adapters, sibling authority y materialización, no a inventar una
garantía universal del host.

## 14. Arquitectura seleccionada

**RACKCAD-OWNED CLOSED SEMANTIC SNAPSHOT + TRANSACTIONAL DERIVED MATERIALIZATION**.

Combina ALT-A + ALT-B + ALT-E:

1. selección física identifica rack, vistas y colocación;
2. un adapter por kind acredita todas las hermanas y produce snapshot tipado inmutable;
3. cada dependencia semántica pertenece a RackCad y queda declarada;
4. `μ_k` produce un nuevo authored state y se asigna un `NewRackId` por rack lógico/destino;
5. Foundation prepara las vistas y AUTH-15 compone payloads/restamp;
6. una transacción AutoCAD crea autoridad embebida y proyecciones físicas;
7. fallo descarta todo el destino; no queda rack semántico pendiente;
8. edición externa posterior de entidades es corrupción de estado derivado y `Actualizar` puede repararla desde la
   autoridad, siempre que ésta siga legible y consistente.

## 15. `μ_k` como autoridad central

```text
SemanticMirror_k(D) = μ_k(D)
Materialize(V, μ_k(D), G, P, F)
```

`D` es el documento authored tipado, metadata preservada y bindings necesarios; no contiene `DBObject`. Cada kind
mantiene su propio tipo y reglas. No hay mega-DTO. La interfaz común transporta estado de éxito/fallo, identidad,
addresses, `PlanReadSet` y planes tipados, sin convertir payloads a un diccionario universal.

## 16. Selección, identidad y `NewRackId`

`PickRackBlock` ya convierte una referencia seleccionada en definición y envelope. V20 limita esa función a locator.
Después se barren hermanas por `RackId`; todas deben ser legibles, pertenecer al mismo kind y coincidir en authored
state/metadata conforme al comparator. El gap actual de BOM para kinds no Selective se debe cerrar antes de admitirlos.

Se genera exactamente un `NewRackId` por source rack/destination. Todas las vistas nuevas comparten el id y cada
envelope conserva su propio address/metadata. Identidad interior, nombre, referencias y bindings se restampan por el
adapter tipado. Nunca se genera un id por vista.

## 17. Transformaciones y colocación

Se preserva:

```text
G = reflexión en sheet
μ_k = reflexión semántica
F = transformación local de vista reflejada
P' = G · P · F
```

`P`, `G`, `F` y `P'` son hechos/planes de materialización. Sólo entran a semántica cuando una regla de producto tipada
lo declare; en ese caso dejan de ser geometría arbitraria y pasan a ser un valor neutral Foundation. La geometría
dibujada de la fuente no alimenta `μ_k`.

## 18. Persistencia, transacción y commit

El modelo seleccionado no introduce persistencia externa:

```text
ACQUIRE/READ → build immutable artifact → PREPARE resources/plans
→ lock document → final binding/PlanReadSet validation
→ start one AutoCAD transaction
→ write every destination payload + definition + reference
→ commit once
→ POST purge/regen/report
```

El payload vive en `RackBlockData` como `Xrecord` de la definición. Variables viven en el NOD como otro `Xrecord`.
El repositorio ya prueba el patrón de escritura conjunta de registro y múltiples vistas en
`ProjectVariableMutationExecutor`; `SystemBlockWriter.RedefineInTransaction` es caller-owned. V20 consume esos
precedentes por autoridad, sin declarar AUTH-15 implementada.

## 19. UNDO

AutoCAD UNDO debe revertir payload semántico y entidades porque ambos se escriben en la misma transacción/database.
Esta condición descarta project-first externo y persisted intent. Imports previos permitidos pueden permanecer como
recursos compartidos sin producto lógico; no contienen el nuevo `RackId`. La caracterización física de UNDO sigue
siendo obligación G3/Owner Validation futura, no un hecho demostrado por esta propuesta.

## 20. SAVE, reopen, recover y partial load

SAVE conserva envelopes y variables dentro del DWG. Reopen reconstruye el editor y BOM desde payloads tipados; no
requiere geometría como autoridad. Recover o partial load que impida leer alguna hermana, el registro o dependencias
produce `UNKNOWN/STOP`, nunca selección de una representación parcial. El contrato seleccionado no admite un rack
semántico persistido sin ninguna vista. Esa posibilidad pertenece a ALT-D y queda rechazada.

## 21. Fallo de materialización y UX

Si la fase semántica pura falla, no se abre transacción. Si PREPARE falla, no se crea producto; sólo pueden quedar los
recursos compartibles permitidos. Si MUTATE falla, se dispone la transacción sin commit. El usuario ve un error claro y
cero racks destino. No hay estado pending ni recovery misterioso. Si una edición posterior corrompe sólo vistas,
`Actualizar` regenera; si corrompe payload/identidad, la autoridad queda ilegible/divergente y se bloquea con
diagnóstico, sin inventar recuperación.

## 22. Compatibilidad con comandos existentes

| Flujo | Hallazgo actual | Obligación V20 |
|---|---|---|
| `RACKEDITAR` | lee envelope tipado y redibuja hermanas | acreditar authored authority de todas las hermanas por kind |
| `Actualizar`/redraw | planifica desde payload y puede reescribir payload + definición | hacerlo consumidor exclusivo de semantic authority |
| `RACKDUPLICAR` | clona geometría física y restampa payload | gap: no es precedente de regeneración semántica; futuro alineamiento separado |
| Insertar vista/hermanas | usa identidad y addresses | consumir snapshot/plan Foundation y no geometría fuente |
| BOM | handlers construyen BOM desde `embed.Design` + variables | extender comparator a todos los kinds antes de admisión |
| Selective editor | DTO authored propio + resolver de variables | adapter Selective explícito y `PlanReadSet` |
| Dynamic/Header/PushBack/Cantilever/Cama | stores/handlers tipados | adapter por kind, sin fallback universal |
| save/reopen | payload y NOD están en DWG | pruebas físicas round-trip sobre Candidate |
| UNDO | writes DB participan en transacción | prueba física payload + todas las vistas + identidad |

V20 sigue la dirección existente de authored payload → resolver/builder → redraw, pero exige completar esa dirección.
No afirma que hoy todas las rutas sean proyecciones puras.

## 23. Viabilidad project-first por capa

| Pieza | Clasificación |
|---|---|
| DTOs, stores, schema, comparators | `PURE DOMAIN/APPLICATION` |
| `μ_k`, identity rules, grouping, reflection helpers | `PURE DOMAIN/APPLICATION` por construir/consumir |
| `RackProjectStore` de archivo | `PROJECT PERSISTENCE`; útil offline, no autoridad interactiva |
| authored envelopes/variables en Xrecord | `AUTOCAD-BOUND PERSISTENCE` |
| selección, transforms, DB scan, block/layer access | `AUTOCAD-BOUND` |
| builders/resolvers/BOM | mayormente `PURE DOMAIN/APPLICATION`; catálogos como ports |
| block library/imports | `RESOURCE-BOUND` |
| reactors/overrules/verticales externos | fuera de semantic closed world; representación host |
| fidelidad/UNDO/save físicos | `UNKNOWN` hasta G3/Owner Validation |

La mayor parte conceptual del espejo puede resolverse sin Autodesk API; selección, persistencia y materialización no.

## 24. CE20-01..20

| CE | Autoridad afectada | Semántica / materialización | Detección, rollback y recuperación | ¿CIA? |
|---|---|---|---|---|
| CE20-01 geometría fuente alterada, payload sano | derived source view | semántica intacta; materialización ignora geometría | sibling/payload gate; reconstruir | no |
| CE20-02 payload alterado tras selección | authored authority | snapshot viejo no puede persistirse contra binding cambiado | relectura final de identidad/payload/PlanReadSet; abortar | no global; frescura acotada sí |
| CE20-03 variable cambia durante transformación | variable authority | puede cambiar `μ_k` | `PlanReadSet` por variable realmente leída; abortar/replanificar | no |
| CE20-04 biblioteca distinta | resource/materialization | semántica intacta; proyección no acreditada | requisito/version mismatch antes de MUTATE; abortar | no |
| CE20-05 reactor altera entidades creadas | derived destination | el artifact inmutable sigue correcto; vista/payload pueden quedar corruptos | si causa excepción, T revierte; mutación silenciosa puede no detectarse y queda fuera de garantía como interferencia externa | no para `μ_k`; sí haría falta para prometer inmunidad material global, promesa que V20 retira |
| CE20-06 overrule cambia geometría visible | presentación | semántica y DB pueden estar intactas; apariencia externa | no se promete apariencia de terceros; render/Owner test | no |
| CE20-07 custom object altera valor consumido | input no cerrado | invalida semántica | adapter debe rechazar dependencia o tiparla como RackCad authority | sí si se pretendiera consumirlo; por eso se excluye |
| CE20-08 materialización falla a mitad | destino staged | semantic calculation válida, producto no existe | una transacción; rollback total; reintento desde comando | no |
| CE20-09 archivo project persiste y DWG falla | dos autoridades | divergencia | arquitectura ALT-C rechazada; no ocurre en seleccionada | no, problema distribuido |
| CE20-10 DWG confirma y semantic persistence falla | dos commits | entidades sin autoridad | payload y entidades en mismo T; un commit | no |
| CE20-11 save/reopen | DWG persisted authority | debe conservar semantic + views | round-trip exacto; ilegible/parcial ⇒ STOP | no |
| CE20-12 `RACKEDITAR` mirrored | embedded authority | debe abrir `μ_k(D)` y todas las hermanas | adapter/store/editor tests + Owner Validation | no |
| CE20-13 `Actualizar` mirrored | semantic authority | debe regenerar determinísticamente | comparator + materialization fixtures | no |
| CE20-14 BOM mirrored | authored + variables | debe cotizar `μ_k(D)` | handler tipado, sibling equality, PlanReadSet | no |
| CE20-15 `RACKDUPLICAR` mirrored | hoy geometry + payload | clon exacto, no prueba proyección semántica | compatibilidad caracterizada; no usar como oráculo V20 | no |
| CE20-16 usuario edita entidades derivadas | derived state | semántica intacta si payload sano | Actualizar; si payload cambia, corruption/STOP | no |
| CE20-17 plugin carga entre fases | host representation | no altera snapshot por mera carga; puede afectar materialización | contrato materializador + rollback/diagnóstico | no para `μ_k`; riesgo residual de vista |
| CE20-18 callback muta source authority | RackCad payload/NOD | invalida source binding; el snapshot ya construido no cambia | relectura acotada antes de write; una mutación posterior es corrupción externa y puede no detectarse en esta ejecución | no para la prueba pura; sí para inmunidad universal, retirada del contrato |
| CE20-19 callback muta sólo destino physical | derived state | semantic artifact intacto; vista corrupta | excepción staged revierte; mutación silenciosa puede requerir diagnóstico/Actualizar posterior | no para semántica; riesgo material explícito |
| CE20-20 dos racks comparten variables | variable authority | ambos leen mismo valor, identidades separadas | snapshot + PlanReadSet por rack/lectura; no global union | no |

CE20-02/03/18 exigen frescura acotada de autoridades RackCad. No resucitan `ObservableChannelClosure`: el contrato no
garantiza que terceros no corrompan bytes RackCad durante o después de la operación. Garantiza el artefacto que RackCad
calculó y las escrituras que su materializador emitió atómicamente; una escritura ajena silenciosa puede escapar de la
ejecución y se trata como corrupción externa al releer. Ésta es una reducción material, deliberada y revisable del
contrato V18, no una supuesta capacidad técnica de detección.

## 25. Reactores y trabajo diferido reinterpretados

- CE-04 geometry reactor: afecta materialización derivada, no `μ_k`.
- CE-05 source callback: sólo afecta semántica si modifica payload/variables RackCad; entra al binding/read-set.
- CE-06 overrule visual: no altera authored authority; queda fuera de garantía visual propia de terceros.
- CE-09 deferred destination change: corrupción posterior de proyección; no convierte geometría en autoridad.
- CE-12 extension load: irrelevante por sí misma; importa sólo si cambia datos RackCad o la materialización.

Trabajo diferido que RackCad programe para completar authored state seguiría siendo parte de la operación y queda
prohibido. POST sólo puede purgar, regenerar y reportar; no puede completar payload, identidad o vistas esenciales.

## 26. La pregunta central

> ¿Necesitamos probar que AutoCAD está aislado, o sólo que AutoCAD no es la autoridad de aquello cuya corrección
> queremos garantizar?

Para la transformación semántica, basta lo segundo y además hay que probar positivamente el conjunto de inputs. Para
la persistencia y la materialización se necesita el contrato transaccional soportado de AutoCAD, no aislamiento de
todo el proceso. Una extensión puede corromper un DWG incluso dentro de un callback síncrono; V20 excluye de la
garantía las escrituras directas de terceros igual que una edición externa y exige que Owner acepte esa frontera. V20
acota el éxito propio a autoridad semántica RackCad y a las escrituras atómicas de su materializador bajo el mismo
contrato que debe gobernar edición/redraw. Un fallo propio o excepción durante la operación revierte y reporta; una
mutación ajena silenciosa no se declara detectable.

## 27. Foundation, I-49 y AUTH-15

Foundation AUTH-01..13 permanece `CONSUME FROM MAIN`: `RackViewAddress`, codec, availability, frame, snapshot físico,
clasificación, agrupación neutral, `Transform2D`, resolve ports, `RackPreparedView<TPayload>`, nombres, boundary de
requisitos y comparator. V20 no copia esas autoridades ni asigna política RACKMIRROR a Foundation.

`PlanReadSet` puede formar parte del snapshot semántico. Por cada variable realmente leída conserva por separado
`SymbolId + complete RootCauses(variable)`; no hay unión global, representative chain ni `RecoveryUnit`. Se consume
I-49 por referencia y A3-R2 sólo prevalece en su scope congelado.

AUTH-15 sigue `I-52 OWNED / NOT IMPLEMENTED`: caller-owned creation, materialización específica del espejo,
identidad/restamp y atomicidad de todas las vistas destino. I-55 G9a es precedente coordinable, no implementación de
AUTH-15 ni transferencia de autoridad.

## 28. Schema y migración

No se requiere por diseño un schema universal nuevo. Cada kind conserva DTO, versionado y `ExtensionData`. La
implementación puede necesitar metadata aditiva para declarar versión de la semántica reflejada o dependencias, pero
esa necesidad se decide por adapter y exige compatibilidad/round-trip. Racks legacy sólo se admiten si el adapter puede
promoverlos sin perder autoridad; fallback ambiguo bloquea. No se crea archivo de proyecto obligatorio.

## 29. Estrategia de pruebas futura

- propiedades puras de `μ_k`: involución donde aplique, identidad, bindings, metadata y casos por kind;
- closed-world gate: cada dependencia permitida y cada field/XREF/custom-object externo rechazado;
- sibling comparator por todos los kinds, incluida divergencia sólo en schema/`ExtensionData`;
- source geometry perturbada con payload intacto produce el mismo semantic result;
- `PlanReadSet` por variable, cambio entre snapshot/commit y causas SCC completas;
- preparación/materialización: una T, un commit, fault en cada unidad, cero producto parcial;
- UNDO, SAVE/reopen/recover/partial-load físicos;
- edición, Actualizar, BOM, insertar vista y duplicación sobre mirrored rack;
- overrule/reactor altera sólo representación: autoridad permanece; corrupción de payload bloquea;
- selección filtrada siempre demuestra más de cero casos, conforme AGENTS.md.

No se implementa ni ejecuta ninguna de estas pruebas en V20.

## 30. Owner Validation futura

Debe verificar sobre el Candidate exacto: flujo visible de selección/línea, identidad única multi-vista, apariencia por
kind, error sin producto parcial, UNDO/REDO, SAVE/reopen, RACKEDITAR, Actualizar, BOM y biblioteca real. Debe distinguir
una vista RackCad incorrecta de un overrule externo de presentación. No sustituye suites, CI ni pruebas puras.

## 31. Autocrítica obligatoria

| Pregunta | Respuesta |
|---|---|
| Q1 ¿corrección enteramente desde estado RackCad? | Sí para kinds/adapters que pasan closure; no para todo rack por defecto. |
| Q2 ¿input `μ_k` físico? | No debe haberlo. Transform/placement son materialización; si una regla exige geometría arbitraria, el rack no es admisible. |
| Q3 ¿tercero altera autoridad sin cambiar estado RackCad? | No bajo la definición: cambiar significado sin bytes propios indica dependencia externa y falla closure. |
| Q4 ¿materialización reparable? | Sí si payload/variables siguen íntegros y todo builder es determinista; debe probarse por kind. |
| Q5 ¿editar/BOM/Actualizar ya usan semántica? | En gran parte sí; edit/BOM usan payload. Hay gaps: comparator no Selective y duplicación física. |
| Q6 ¿persistencia y UNDO coherentes? | Sí sólo si payload y vistas viven en una transacción DWG; no con archivo project-first. |
| Q7 ¿save/reopen sin geometría autoridad? | El modelo embebido lo permite; exige pruebas de round-trip/partial-load. |
| Q8 ¿closed world reconocible positivamente? | Sí mediante adapter tipado + dependency manifest + authorities, no inventario de plugins. |
| Q9 ¿scope RackCad-owned conserva valor? | Sí: el producto dibuja racks RackCad; Owner debe aceptar la exclusión explícita de semántica externa. |
| Q10 ¿CIA renombrada? | No: no se afirma clausura host; se excluyen inputs externos y se limita la garantía materializadora. |

## 32. Comparación cualitativa

| Alternativa | Seguridad semántica | Materialización/rollback | Compatibilidad/coste | Riesgo externo residual | Veredicto |
|---|---|---|---|---|---|
| A sola | alta si input cerrado | no resuelve persistencia | adapters medianos | input implícito | componente |
| B sola | scope defendible | no crea vistas | restricciones/migración moderadas | representación | componente |
| C | alta offline | commit distribuido/UNDO roto | migración alta | bajo offline | rechazada |
| D durable | artifact claro | pendientes/dos autoridades | schema/UX altos | recovery | rechazada |
| E sola | input limpio | atomicidad viable | seam material | adapter podría mentir | componente |
| F | segura por deferencia | ninguna | cero | ninguno nuevo | fallback vigente |
| G | elimina riesgo | no producto | coste de oportunidad | ninguno | no seleccionada |
| híbrido A+B+E | alta y comprobable por kind | una T, rollback total | cambio material pero alineado | corrupción posterior de derived state | **seleccionado** |

## 33. Riesgos residuales e invalidadores

Son materiales: adapter omite un input; comparator no cubre un kind; builder lee host fuera del plan; biblioteca no
acreditada; payload y entidades no comparten T; una ruta usa geometría como autoridad; POST completa estado esencial;
UNDO/save no conserva ambos; `Actualizar` no reproduce; BOM/editor eligen hermana arbitraria; schema pierde metadata;
Owner no acepta que presentación de terceros quede fuera de garantía.

Cualquiera exige corrección de Proposal antes del Freeze. Descubrir que un input necesario no puede volverse
RackCad-owned devuelve ese kind a V18/deferred; no amplía silenciosamente el closed world.

## 34. Disposición de V18, Freeze, O-1 y ADR

Si Coordinator y Architect acuerdan V20:

```text
V18 affected contract = MATERIALLY SUPERSEDED BY V20
Freeze V18 = HISTORICAL / IMMUTABLE after replacement Freeze
New Freeze = REQUIRED
O-1 V18 = HISTORICAL; NEW OWNER DECISION REQUIRED
ADR-0036 = NEW AMENDMENT REQUIRED; remains PROPOSED
```

Hasta entonces V18, Freeze V18 y O-1 V18 siguen gobernando. Esta publicación no los reemplaza por sí sola.

## 35. Regla de reapertura G3

G3 permanece STOPPED. Sólo puede reabrirse después de: consenso exacto V20, reconciliación con main/I-55/I-49,
Freeze nuevo exacto, recheck Coordinator/Architect, nueva decisión Owner, enmienda ADR-0036 y decisión explícita de
reapertura. G3 debe caracterizar adapters/closure/materialización; G3B y CT-50 siguen cerrados hasta entonces.

## 36. I-55

I-55 se observó primero en `705ae301f696b7f16c8ec006e2b1ab526be46489` y avanzó durante la preparación de V20 a
`fa8f6affa769f87afe795fd310fbc74c43494123`. El delta cablea Insertar con barrido único, gates de propiedades/authored,
Resolve/Prepare, redibujo G9a y colocación G8; conserva Actualizar y falla cerrado cuando AUTH-13 carece de comparator
demostrado. Su seam prepara todas las unidades y muta N hermanas con una transacción/commit, descartando ante fallo.

Clasificación fresca: impacto contractual I-52 `NON-MATERIAL`; coordinación `MATERIAL`; seam potencialmente
reutilizable `YES, SUBJECT TO RECONCILIATION`. Refuerza el precedente de materialización y el requisito de comparator,
pero no redefine Foundation, I-52 o AUTH-15 y no resuelve closed-world ni materialización específica de espejo. Al
integrarse en una base futura, el censo API y las pruebas I-52 deben regenerarse sobre el nuevo SHA.

## 37. Alcance de publicación

Sólo se crean esta Proposal y su paquete Architect, y se anexa el registro I-52. No se modifican V18/V19, Freeze,
ADR-0036, research, G3A, Foundation, I-49, `src`, `tests` o `eng`. No hay implementación ni ejecución de gates.

## 38. Checklist de revisión

- [ ] Refutar que payload/NOD tipados basten como autoridad por kind.
- [ ] Encontrar cualquier input real de `μ_k` tomado de geometría/semántica externa.
- [ ] Verificar que closure sea positiva y no inventario negativo de plugins.
- [ ] Revisar sibling consistency para todos los kinds y el gap actual no Selective.
- [ ] Verificar `PlanReadSet` exacto y sin unión global.
- [ ] Refutar la separación semántica/materialización con CE20-01..20.
- [ ] Confirmar una transacción para payload, identidad y todas las vistas.
- [ ] Revisar UNDO/SAVE/reopen y ausencia de commit distribuido.
- [ ] Revisar compatibilidad con editar, Actualizar, BOM, insertar vista y duplicar.
- [ ] Confirmar que la apariencia alterada por terceros no se presenta como correcta.
- [ ] Confirmar Foundation por consumo, I-49 por referencia y AUTH-15 no implementada.
- [ ] Confirmar que V18 sigue gobernando hasta consenso/Freeze/Owner/ADR nuevos.

```text
PROPOSAL V20 = PUBLISHED / REVIEW REQUIRED
SELECTED ARCHITECTURE = RACKCAD-OWNED CLOSED SEMANTIC SNAPSHOT + TRANSACTIONAL DERIVED MATERIALIZATION
TECHNICAL CONSENSUS = NOT REACHED
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
