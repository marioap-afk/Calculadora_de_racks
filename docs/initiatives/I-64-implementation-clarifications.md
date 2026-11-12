# I-64 — Aclaraciones de implementación

Registro **append-only** de las diferencias entre lo implementado y la redacción de la Proposal V5 congelada. La Proposal no se edita: `docs/initiatives/I-64-proposal-v5.md` sigue siendo el texto histórico. Donde una aclaración de este archivo y la redacción congelada difieren en el **mecanismo**, manda la aclaración, solo dentro de su alcance.

## IC-01. Las cuatro acciones de navegación de F2 no se ejecutan en contexto de comando

**Autoridad:** ratificación del Owner, literal en `docs/automation/evidence/I-64-pilot/F3-BROWSER/coordinator/owner-f2-close-ratification-f3-authorization.md`. Cierra la desviación D-08 de `docs/automation/evidence/I-64-overnight/overnight-dispositions.md`.

**Texto afectado de la Proposal V5:** D-07, O4-06 e INV-40, donde dicen «en contexto de comando».

**Aclaración:**

> For the four F2 navigation effects, real AutoCAD Smoke demonstrated that the command-context implementation did not produce the required observable effect. The accepted implementation therefore executes the explicit navigation effect in application context under DocumentLock, preserving session identity, quiescence, immediate target revalidation, zero authored writes, echo semantics and focus policy.

Cambió el mecanismo. El contrato observable y de seguridad no cambió.

**Alcance:** solo Seleccionar, Ver, Centrar y Localizar vista. **No** se aplica a Actualizar, a escrituras de datos authored, a mutaciones de editores, a las rutas de escritura de F4, a la integración de editores de F5 ni a operaciones arbitrarias del Plugin, que conservan sus requisitos del Freeze y de su contrato.

**Evidencia:** Smoke-NAV `R20261110T064058Z-c883` (STOP en SN-01 sobre `7bdcbfd9`: dos `*Cancel*`, selección perdida, vista sin cambio) y `R20261110T073451Z-e221` (PASS con limitaciones sobre `003be859`); corrección `F2-T2-HOST-C1`, paquete en `docs/automation/evidence/I-64-pilot/F2-T2-HOST-C1/delta-close-package.md`.

**Invariantes que siguen siendo obligatorios y dónde se cumplen en `003be859`:**

| N.º | Invariante | Dónde |
|---|---|---|
| 1 | Solo por acción explícita del usuario | `WorkspaceNavigationViewModel.NavigateAsync` es la única entrada; ningún refresco ni evento la llama |
| 2 | El documento activo coincide con la `SessionId` de origen | `_authorize` en `WorkspaceNavigationHost.RunOnActiveDocument`, y `WorkspaceNavigationRunner.Run` |
| 3 | Documento en reposo | `CommandInProgress` vacío y `Editor.IsQuiescent`; si no, `Stale`, sin efectos |
| 4 | Sin modal RackCad activo | La política del panel: `WorkspaceActionBlock.ModalActive` deshabilita las acciones. El host no lo comprueba por separado: un modal abierto desde un comando deja `CommandInProgress` no vacío y el host responde `Stale` |
| 5 | `DocumentLock` durante la operación | `using (document.LockDocument())` en `RunOnActiveDocument` |
| 6 | Revalidación inmediata antes de los efectos | `NavigationActionPolicy.Execute`, dentro del bloqueo |
| 7 | El objetivo sigue existiendo | Revalidación de la política; si no, `RejectedTargetGone` |
| 8 | Sigue siendo atribuible según `NavigationTargets` | Revalidación de la política; si no, veredicto de rechazo |
| 9 | Un fallo no produce efecto de selección ni de vista | Un veredicto distinto de `Act` no llama a los efectos |
| 10 | Un fallo invalida y vuelve a pedir la caché del rack | `RefreshRequired` de la decisión y la invalidación de la política |
| 11 | Sin escritura authored en el DWG | Los efectos solo tocan selección implícita y vista; DBMOD sin cambio en Smoke-NAV |
| 12 | Ningún evento ni cambio de contexto navega | Sin camino desde eventos hacia los efectos; guardas de fuente de F2 |
| 13 | Eco por contenido | `NavigationEcho`, sin cambios |
| 14 | El foco vuelve al dibujo | `ReturnFocusToDrawing`, solo tras `Executed` |
| 15 | Ningún `DBObject`, `ObjectId` ni `Transaction` sobrevive | Todo vive dentro de `using`; solo salen valores |

**Efecto en otros textos:** los comentarios de guardas que aún decían «command-context» se corrigen en el mismo commit que este archivo. Las evidencias, los contratos y las decisiones anteriores a C1 conservan su texto: describen lo que era cierto entonces.

## IC-02. Obligaciones de validación posteriores a F2

**Autoridad:** la misma ratificación.

**Smoke-NAV = PASS WITH LIMITATIONS.** SN-01 a SN-07 y SN-10 PASS medidos. SN-09 medido y aceptable: primera acción ≤ 798 ms; siguientes, 139-270 ms.

| Limitación | Estado | Obligación |
|---|---|---|
| SN-08 no se provocó directamente, porque la ejecución final es síncrona | **LIMITED / covered structurally but not directly provoked**. No cuenta como demostrada de forma independiente | Antes de la Owner Validation final, ejercitar donde sea posible que una petición de la sesión o documento A nunca afecta a B. Las pruebas multi-documento de F3 y F4 pueden aportar evidencia determinista |
| Se usaron dibujos sintéticos | Abierta | Owner Validation final con racks reales |
| SN-06c y la parte de pestañas de SN-10 dependen de F3 | Abierta | Comprobar cuando exista el Candidato con F3 |

**Añadido a la Owner Validation final**, sobre racks reales representativos, como mínimo: Seleccionar, Ver, Localizar vista, copia k/M donde haya varias referencias y un caso de seguridad al cambiar de documento o sesión. Observar además Deshacer y Rehacer tras navegar la vista: hoy es **UNCHARACTERIZED**, no FAILED, y no bloquea el cierre de F2.

## IC-03. «Abrir en editor clásico» se entrega en F5

**Autoridad:** la misma ratificación, decisión C-F3-04.

D-08 y D-15 piden que un sistema aún no integrado ofrezca una pestaña de resumen con «Abrir en editor clásico». La entrega es por etapas: F3 da la superficie ligera de resumen y navegación, y la acción de abrir el editor clásico queda para **F5**. F3 no incluye ningún lanzamiento temporal de comandos. **La obligación de producto de D-08 y D-15 sigue vigente hasta su entrega en F5.**

## IC-04. La lectura de resumen de F3 no es una operación del drenaje

**Autoridad:** decisión C-F3-02 del Owner («resolved lazily when the active tab surface is materialized») y disposición F3-E09 de la sesión. Se comunica al Owner.

**Texto afectado de la Proposal V5:** P-03, P-04 y D-04.

**Aclaración:** el drenaje conserva exactamente sus operaciones de D-04. El resumen del rack de la pestaña que se materializa se lee después, en el aviso que el controlador emite cuando el drenaje ya terminó, con el documento en reposo y una sola vez por materialización. Una lectura en un aviso de cambio de documento o de comando responde `Busy` sin bloquear el documento y se reintenta en el drenaje siguiente. Un clic del usuario lee directamente, con las mismas comprobaciones de documento, comando y sesión.

**Alcance:** solo el resumen de F3. No autoriza otras lecturas del DWG en avisos o manejadores.

## IC-04, redacción aceptada por el Owner

**Autoridad:** orden `I-64-pilot/F4-DRAFTS/coordinator/owner-f3-ratification-f4-authorization.md`. **Esta redacción sustituye a la de arriba.** IC-04 no es un permiso para leer desde eventos de AutoCAD.

Los manejadores de notificación de eventos de AutoCAD **no pueden** leer el DWG: solo encolan o agrupan pistas.

Una lectura ligera de resumen puede ocurrir **después** de procesar la notificación, como operación separada, programada o visible para el usuario, y solo con las condiciones normales de contexto de aplicación: SessionId y documento correctos, documento activo cuando se requiera, reposo, sin modal RackCad, `DocumentLock`, operación acotada y sin que sobreviva ningún `DBObject` ni `Transaction`.

La lectura **no puede** ocurrir sincrónicamente dentro de `ObjectModified`, `ObjectErased`, `ImpliedSelectionChanged`, `CommandEnded` ni de un callback de Idle o de evento simplemente porque se disparó.

Si el drenaje termina y después programa o materializa la superficie activa por la ruta normal de lectura, es aceptable. «Después del drenaje» es aceptable; «dentro del manejador de la notificación» no lo es.

**Conformidad del producto:** la disposición F4-E05 registra que la lectura de `206e0f7a` ocurre aún dentro de la llamada a `Application.Idle`, y encarga a F4-T2 resolverlo.

## IC-05. Obligaciones que F3 traslada a validaciones posteriores

**Autoridad:** la misma orden. SN-06c; la parte de pestañas de SN-10; validación en host de que el manejo de avisos no lee el DWG; validación en host de que los racks borrados desaparecen correctamente; Owner Validation con racks reales representativos. F3 es PASS WITH LIMITATIONS, no un navegador validado en host.

## IC-06. Piloto acotado de Push Back en F4 (F4-PILOT-PUSHBACK-01)

**Autoridad:** disposición del Owner `F4-DRAFTS/coordinator/owner-f4-disposition-2.md` (F4-E09), que acepta la opción (b) de C-F4-01. Es una aclaración escalonada de D-15 y de F5; no cambia el Freeze.

**Propósito:** F4 implementa una superficie de edición compacta de Push Back sobre el **mismo** estado editable de Push Back, para probar la arquitectura de `DraftSession`, dirty, obsolescencia y Actualizar. No es la migración completa del editor de Push Back.

**Requisitos:**

1. un único estado editable subyacente;
2. ningún modelo de producto paralelo;
3. ninguna segunda autoridad persistida;
4. un conjunto de campos cerrado y enumerado de forma explícita;
5. los campos y funciones no soportados quedan como residual de F5;
6. F4 no intenta reproducir el editor heredado completo;
7. la misma semántica de `DraftSession` tiene que poder soportar después la superficie completa de F5.

**Campos:** el contrato enumera los campos exactos del piloto antes de implementar T2 y T3, y el Worker no los elige. Se prefiere el grupo coherente más pequeño que ejerza la edición escalar, la validación, dirty, el texto pendiente, la comparación de obsolescencia y la escritura con relectura.

**F5:** las capacidades del editor heredado de Push Back que el piloto compacto no representa siguen siendo trabajo de migración de F5.

**Redacción del cierre:** un PASS de F4 **no** se describe como «editor de Push Back migrado». Significa que Push Back aporta el piloto de edición acotado con el que se prueban la semántica de `DraftSession` y la de Actualizar.

## IC-07. Alcance y capa no editable del piloto de Push Back (F4-T2)

**Autoridad:** IC-06 (requisito 5: los campos y funciones no soportados son residual de F5) y la disposición F4-E18. Es una aclaración escalonada de D-11 y de D-12 para el piloto, no cambia el Freeze y se comunica al Owner.

**Punto 1, alcance (D-11):**
- El piloto admite solo diseños Push Back de un solo lado (`!IsComposite`) cuya cola dormida quede vacía al materializarse.
- Un rack A/B, o uno que aparque cola, se muestra como «No editable aquí: fuera del piloto (F5)», con la indicación de usar RACKEDITAR y sin lanzarlo (C-F4-07).
- La edición de racks A/B en el panel es residual de F5.
- **Smoke-2:** los dibujos deben incluir al menos un rack Push Back de un solo lado, admisible y con anotaciones activas.

**Punto 2, capa no editable (D-12):**
- Lo que el piloto edita sale del borrador.
- Todo lo authored que el piloto no edita sale de la base vigente (`InitialDraftState`). Esto incluye la autoridad de módulos y la metadata de origen del proyecto.
- Es análogo a la reconciliación de I-48 que cita D-12 («sobre la versión actual»).
- Después de «Conservar mi borrador», la capa no editable es la de la versión nueva. Un borrador Limpio compone exactamente la base y nunca escribe un cambio authored que la superficie no muestre.

**Residual de F5:** cuando los módulos sean editables, el comparador y el copiador del borrador incorporarán la autoridad de módulos.

## IC-08. Marcado de obsolescencia grueso por sesión en F4-T2B2B

**Autoridad:** D-12 y D-13 del Freeze, el paquete de F4-T2B2 (D-T2B2-11) y la disposición F4-E34. Es una aclaración escalonada del marcado de los borradores en F4. No cambia el Freeze y se comunica al Owner.

**Cuándo se marca:**
- Cuando el drenaje de una sesión produce `MarkDraftsPossiblyStale`, el host marca **todos** los borradores del piloto de esa sesión.
- El marcado es grueso por sesión porque las pistas no llevan rack.

**Qué es el marcado:**
- Es solo una pista: no crea Conflicto y no lee el dibujo.
- La comprobación real la hacen «Comprobar ahora» y la recarga perezosa, siempre por acción del usuario.

**De dónde nace la pista:** `MarkDraftsPossiblyStale` nace de cualquier cambio de base de datos y de cualquier `CommandEnded`, también de comandos que no modifican nada (`WorkspaceSession.cs:52-56`).

**Obligación vinculante para F4-T4B:**
- Las pistas que produce la mutación propia de Actualizar no marcan los borradores. El mecanismo lo fija T4B.
- Ese mecanismo debe respetar dos hechos:
  - las pistas se agrupan por sesión y por tipo (`WorkspaceSession.cs:26` y `:46-57`);
  - se drenan después del ámbito.
- Además, no puede suprimir `InvalidateNavigationCaches`.
- **Oráculo:** «tras Actualizar A, el borrador B no queda marcado» (INV-28).

**Smoke-2 y Smoke-PERF:** miden la frecuencia del marcado y el coste de las recargas que provoca.
