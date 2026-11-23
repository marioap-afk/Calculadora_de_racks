# I-69 — Proposal V2: reparto Owner/Claude, delta de seguridad y camino minimo

Frozen: NO
Estado: DRAFT — BLOCKED OWNER DECISION (Q-01..Q-04)
Workflow: V2
Autor: Claude, implementador por relevo D-09 (SAME-SESSION ROLE: no es revision independiente)
Base: `feature/selectivo-catalogo-componentes` @ `fd456e8b82cb4d78c8b1cb145e62f89f1b1ad124`;
producto = `origin/main` `812baf879612320653094a8aaf46aca0bac81383`
Arquetipo operativo: FOUNDATION EVOLUTION provisional (D-06); propuesta condicional en §6
IMPLEMENTATION AUTHORIZATION = NO

Paquete tecnico del primer encargo de Claude. No es Freeze, ni GATE PASS, ni autorizacion de
producto. [Proposal V1](I-69-proposal-v1.md) se conserva sin editar como historia; esta version
la sustituye como propuesta de trabajo y mantiene su numeracion (O-xx, P-xx, T1..T15, OV-xx,
F0..F4). Los hechos medidos de este relevo viven en la
[evidencia §17](../automation/evidence/I-69-evidence.md); aqui solo se citan por ID (`I69-Hnn`).

## 1. Que cambia respecto de V1

1. D-09 convierte O-01/O-02 en **entradas de datos del Owner**, no en preguntas: altas y
   renombres llegan por Excel/CSV. Claude no inventa ni completa registros.
2. Se separa con precision el trabajo de datos y el de software (§2). Para largueros, el
   comportamiento pedido ya lo cubre el catalogo: el software necesario es de pruebas,
   validacion de datos y actualizacion de goldens fijados al catalogo (I69-H01).
3. Se mide el delta de seguridad sobre la base actual (§4) y aparece un hecho nuevo que V1 no
   tenia: en PushBack una familia nueva se ofreceria y persistiria, pero su BOM no la cuenta
   (I69-H07). Eso convierte la exposicion compartida en una pregunta con consecuencia concreta.
4. Las preguntas Owner se reducen a cuatro (Q-01..Q-04, §5), cada una con recomendacion.
5. Se propone un protocolo serializado de recepcion de datos con RED observable (§8).

## 2. Reparto Owner / Claude

### 2.1 Datos que completa el Owner (D-09)

| Archivo | Que aporta el Owner | Condicion que Claude verifica al recibir |
|---|---|---|
| `secciones.csv` | Filas `LARGUERO` nuevas al final: `id` estable, `displayName`, `peraltes` (`;`), `mensula` (FK) y medidas opcionales. Renombres: solo `displayName` de IDs existentes, con mapa `Id → texto` | Ids unicos (tambien sin distinguir mayusculas), casing de Ids existentes intacto, ninguna otra fila/columna alterada |
| `mensulas.csv` | Fila destino solo si la FK nueva no existe | `INVALID_MENSULA_REF` ausente en el validador |
| `blocks.csv` | `<ID>` × FRONTAL/LATERAL/PLANTA con `blockName` EXACTO del DWG, o reutilizacion explicita de un bloque existente | Tres vistas por larguero nuevo (el validador NO lo exige: I69-H02) |
| `connection-layout.csv` | `INICIO_PERFIL` FRONTAL y PLANTA; mates LATERAL solo si aplican | Pendiente `LocalYPorParam` = 0 o se caracteriza antes (evidencia §7); aviso: `TROQUEL_CAMA` o mates laterales izquierdo/derecho exponen el larguero en Dinamico y PushBack (I69-H06) |
| `seguridad.csv` | Dos filas: `id`, `displayName`, `type` y comerciales. El `type` decide el comportamiento (§4) | `type` acorde a Q-01/Q-02; ningun otro registro alterado |
| Biblioteca DWG (fuera del repo) | Bloques reales con los parametros que pide el manifiesto (larguero: FRONTAL `LONGITUD`+`PERALTE`, LATERAL `PERALTE`, PLANTA `LONGITUD`+`PERALTE`; I69-H06) | Solo en smoke; bundle/CSV no lo prueban |
| Decisiones | Q-01..Q-04 | Registro por el Coordinator |

Si el Owner edita con Excel: los archivos actuales tienen encodings distintos (I69-H08). Guardar
como "CSV UTF-8" agrega BOM; el lector lo admite, pero es un cambio incidental del encabezado
que se registra. Excel tambien puede reformatear numeros o comillas de filas no tocadas: se
reporta al Owner, Claude no lo corrige.

### 2.2 Trabajo de software de Claude (tras Freeze y autorizacion)

| ID | Trabajo | Cuando aplica | Materia |
|---|---|---|---|
| S-01 | Actualizar los goldens fijados al catalogo distribuido: `ShippedCatalogIntegrityTests` (`BeamProfiles.Count` = 4, filas de secciones = 7) con los conteos que resulten de los datos | Siempre que haya altas (I69-H01; precedente I-18a) | Prueba |
| S-02 | Pruebas focales T1..T15 en archivos propios `I69*.cs` y constantes nuevas en `TestCatalogIds`; nunca en `SelectiveEditorOpenTests` | Siempre | Prueba |
| S-03 | Validacion de datos recibidos: alcance del diff, unicidad, FK, mappings ×3 vistas, baseline exacta del validador (I69-H03), SHA-256 e inventario | Cada recepcion | Evidencia |
| S-04 | Titulo de grupo de la familia nueva de Entrepaño en `SelectiveSafetyWindow.GroupTitle` (hoy mostraria el `type` crudo; I69-H05) | Q-02 = familia propia | UI, texto del Owner |
| S-05 | Excluir las dos familias de PushBack en `PushBackSafetyAuthority` y en el filtro del dialogo | Q-03 = solo Selectivo | Consumidor compartido; I-64 toca el archivo del dialogo (I69-H09) |
| S-06 | Conservar el default de celda nueva si los textos finales lo cambian | Q-04b = conservar y el orden medido cambia | UI; activa M-03 si no se decide |
| S-07 | Regla "seleccion sin BOM" | Solo si Q-01/Q-02 = A | Activa M-02/M-06 |
| S-08 | Colocacion, plan unico de dibujo+BOM, campos de configuracion y pruebas fisicas | Solo si Q-01/Q-02 = C | Activa M-02/M-05/M-06; ver §4.3 |
| S-09 | Distribucion: build y `verify-bundle` del catalogo; preparacion de OV (F4) | Siempre | Evidencia |

No se escribe codigo de producto para largueros: provider, opcion, dropdown, BeamId, DTO, receta
y builders ya consumen cualquier fila `LARGUERO` compatible (evidencia §§4-9). Si los datos
reales exigieran algo no soportado (pendiente parametrica no nula, larguero tipo in/out con
contrato LATERAL distinto, distincion de peraltes en el total consolidado), es STOP y propuesta
antes de implementarlo (P-11), no un ajuste incidental.

## 3. Largueros: camino minimo

1. El Owner declara el lote (Ids, textos, peraltes, FK, mapa de renombres) y despues edita los
   CSV (§8). Filas al final, sin reordenar.
2. Claude escribe RED contra lo declarado: alta ausente (T2), texto anterior (T3), golden S-01.
3. Recepcion de datos, commit identificado del Owner, GREEN y regresion T1/T4/T6/T9/T14/T15.
4. Default (P-04): se mide en la UI real con los textos finales. Hoy el primero por `Label` es
   `LARGUERO_ESCALON_CAL14_3_REMACHES` (I69-H04). Un texto nuevo que ordene antes cambia la pieza
   inicial de las celdas nuevas y de las legacy con `BeamId` null; eso es Q-04b.

## 4. Seguridad: mecanismo, delta y opciones

### 4.1 Mecanismo vigente (RECONSTRUCTED sobre la base, I69-H05)

`SelectiveSafetyWindow` recorre `seguridad.csv` en orden. Las familias BOTA/LATERAL/TOPE/
DESVIADOR/GUIA/DEFENSA son exclusivas (combo de variantes); PARRILLA tiene rejilla propia. Un
`type` que no es ninguno de esos produce **una fila por registro** con caja de cantidad (entero
≥ 0; vacio o 0 = no incluir), bajo el titulo `GroupTitle(type)`: `TRASERA` ya muestra "Guardas
traseras"; otro `type` desconocido muestra el texto crudo. Al aceptar, cantidad > 0 crea
`SelectiveSafetySelection { ElementId, Quantity, Side = None }`.

`SelectiveBomBuilder.AddSafetyComponents` cuenta por dibujo solo BOTA/LATERAL y deja TOPE/
PARRILLA/DESVIADOR a sus planes; cualquier otro `type` usa la cantidad manual: componente
`Seguridad`, `ProfileId = ElementId`, descripcion = `Label`, longitud 0, una pieza por unidad.
Los builders F/L/P dibujan solo familias conocidas: un `type` nuevo no dibuja nada. DTO, store,
`DeepCopy` y comparadores ya transportan `ElementId`/`Quantity`; no hace falta campo nuevo.

Conclusion: **B se obtiene sin codigo de producto en Selectivo**, con una fila de datos cuyo
`type` no sea una familia conocida. A y C no.

### 4.2 Superficies afectadas por una familia nueva (I69-H07)

| Superficie | Guarda (`TRASERA`) o `type` nuevo | `type` = PARRILLA |
|---|---|---|
| Selectivo: dialogo | Fila de cantidad | Segunda fila PARRILLA con rejilla propia |
| Selectivo: dibujo/BOM | Sin dibujo; BOM manual | Dibujo y BOM toman solo la PRIMERA PARRILLA: la segunda se persiste y se pierde en silencio (EXP-08) |
| PushBack: dialogo y autoridad | Se ofrece y se admite (denylist GUIA/PARRILLA/TOPE/DEFENSA) | Excluida (PB-VAL-06) |
| PushBack: BOM | **No aparece**: `SystemBomBuilder` solo cuenta instancias dibujadas de familias conocidas, sin respaldo manual | No aplica |
| Dinamico | No se ofrece (allowlist) | No se ofrece |
| Persistencia/round-trip | Sin cambio de esquema | Sin cambio de esquema |

### 4.3 Opciones por elemento

- **A — UI + persistencia, sin BOM.** No existe: hoy cantidad > 0 entra en BOM y 0/vacio no se
  persiste. Exige una regla nueva (S-07), M-02/M-06. Sin uso de producto identificado.
- **B — UI + persistencia + BOM manual.** Existe (4.1). El Owner acepta una cantidad entera
  total por rack logico, multiplicada por copias en el consolidado, sin dibujo.
- **C — B + geometria.** Requiere vistas, regla de posicion (lados/fondos/celdas/niveles),
  dimensiones y parametros, regla de conteo comun a dibujo y BOM (AGENTS regla 2), bloques
  reales y probablemente campos de configuracion compartidos (`AuthoredRawReader.Shapes`,
  `AuthoredCanonical`, `AuthoredTypedValues`). Para la Guarda trasera se superpone con I-25
  (backlog de guardas traseras): elegir C aqui absorberia I-25, cosa que este relevo no decide.

Relacion Entrepaño/Parrilla:

- **E1 — familia propia** (p. ej. `type` = `ENTREPANO`): coexiste con Parrilla, no toca su plan ni
  su BOM; con B no hay interaccion por celda. Necesita S-04 para un titulo legible.
- **E2 — variante excluyente de PARRILLA**: hoy es inseguro (segunda PARRILLA perdida). Hacerlo
  seguro exige volver exclusiva la familia PARRILLA (cambia la UI de Parrilla en diseños
  existentes) y dar al entrepaño bloques con `FRENTE`/`FONDO`: M-03/M-05/M-06.
- **E3 — segunda familia de cubierta con geometria** (C con coexistencia): contrato de celdas
  ocupadas y plan generalizado; el costo mayor.

### 4.4 Recomendacion para el Coordinator

1. **Guarda trasera: B con `type` = `TRASERA`** (vocabulario ya documentado en
   `CatalogEntries` y en el titulo del dialogo). Motivo de producto: el mandato pide seguir el
   patron vigente, no fabricar componentes fisicos y no inventar dimensiones; B entrega
   disponibilidad, persistencia y BOM con la cantidad que decide el Owner. C queda en I-25.
   Advertencia para el futuro: si I-25 dibuja `TRASERA`, sus selecciones pasarian de cantidad
   manual a conteo por dibujo, como BOTA; lo decide esa iniciativa.
2. **Entrepaño: E1 + B**, familia propia que coexiste con Parrilla. E2 cambia una familia
   existente y su autoridad en PushBack; E1 no toca nada existente.
3. **Exposicion: solo Selectivo** (S-05). Motivo de producto: el mandato dice "Agregar a
   Selectivo", y en PushBack la pieza se veria y se guardaria pero no llegaria al BOM. No es la
   opcion mas barata: aceptar la exposicion no requiere codigo, pero deja esa incoherencia.

No se elige ninguna en nombre del Owner; son las respuestas propuestas para Q-01..Q-03.

## 5. Preguntas que siguen exigiendo decision del Owner

| ID | Pregunta | Recomendacion | Hereda de V1 |
|---|---|---|---|
| Q-01 | Guarda trasera: ¿B (cantidad entera total por rack, BOM "Seguridad", sin dibujo) o C? | B, `type` `TRASERA` | O-03, O-05 |
| Q-02 | Entrepaño caída de agua: ¿familia propia que coexiste con Parrilla (E1) o variante excluyente de Parrilla (E2)? ¿B o C? Si E1, texto del titulo de grupo | E1 + B; titulo lo da el Owner | O-04, O-05 |
| Q-03 | ¿Las dos piezas existen solo en Selectivo, o tambien en PushBack? Hoy aparecerian en PushBack sin BOM | Solo Selectivo | O-06 (seguridad) |
| Q-04 | a) ¿Se acepta que los renombres se vean en todo sistema/BOM que use ese Id (autoridad unica)? b) Si los textos finales cambian el primer larguero de la lista, ¿se conserva `LARGUERO_ESCALON_CAL14_3_REMACHES` como pieza inicial o se acepta el nuevo primero? | a) si; b) solo se pregunta si el orden medido cambia | O-06 (largueros), P-04 |

Los datos de altas/renombres (ex O-01/O-02) no son preguntas: son la entrada de F1 (D-09).

Decisiones del Coordinator, no del Owner:

- C-01 Protocolo de recepcion (§8): R1 recomendado.
- C-02 Clasificacion final de arquetipo (§6) tras Q-01..Q-04, con Architect segun LIFECYCLE §5.
- C-03 Plan de gates (§9) y, si Q-03 = solo Selectivo, coordinacion con I-64 por el archivo del
  dialogo PushBack (I69-H09).

## 6. Materialidad propuesta (DC-09 con evidencia)

Escenario recomendado (Q-01 B/TRASERA; Q-02 E1+B; Q-03 solo Selectivo; Q-04a si; default sin
cambio o cambio aceptado):

| ID | Estado propuesto | Evidencia |
|---|---|---|
| M-01 | NO ACTIVADO | CSV sigue siendo la autoridad; sin catalogo paralelo |
| M-02 | NO ACTIVADO | Solo valores nuevos de `BeamId`/`ElementId` sobre DTO existentes; `Quantity` conserva su significado |
| M-03 | NO ACTIVADO si Q-04 queda decidido | Renombres = lo pedido; default se mide; diseños existentes sin cambio |
| M-04 | NO ACTIVADO | Politica por frontera de la evidencia §11 intacta (T14) |
| M-05 | A DECIDIR por Coordinator/Architect | S-05 amplia la lista de exclusion de PushBack solo para dos familias nuevas, sin cambiar el trato de familias ni documentos existentes. Propuesta: no activado |
| M-06 | NO ACTIVADO | B usa el camino generico; S-04 es presentacion |
| M-07 | NO ACTIVADO | Sin mecanismo transversal |
| M-08 | NO ACTIVADO | ADR-0007/0020 (catalogo legacy) y ADR-0014 (copia central) intactos |

Propuesta: **EXTENSION** en ese escenario si el Coordinator acepta M-05 como no activado; si no,
FOUNDATION EVOLUTION con revision adversarial. Cualquier A (S-07), C (S-08), E2 o aceptar la
exposicion en PushBack sin BOM mantiene FOUNDATION EVOLUTION (M-02/M-03/M-05/M-06). La decision
es del Coordinator (LIFECYCLE §3); bajar exige ademas al Architect si participo.

## 7. Freeze propuesto — deltas sobre V1 (sin congelar)

P-01..P-11 de V1 se conservan. Cambios propuestos:

| Clausula | Delta V2 |
|---|---|
| P-01 Autoridad | La lista cerrada sale del lote declarado por el Owner y de los CSV recibidos, no de O-01/O-02 como preguntas |
| P-04 Default | Oraculo concreto: Id inicial medido en la UI real antes y despues de los datos; cambio solo con Q-04b |
| P-07 Seguridad | Segun Q-01/Q-02: `type`, B, cantidad manual por rack, sin dibujo; Entrepaño no es PARRILLA salvo E2 |
| P-09 Consumidores | Matriz §4.2 como oraculo: Selectivo si; PushBack segun Q-03; Dinamico no. Largueros: RackLargueroWindow si; Dinamico/PushBack solo con `TROQUEL_CAMA` o mates laterales |
| P-10 Distribucion | Inventario SHA-256 y encoding/EOL por archivo recibido (I69-H08) |
| P-11 Limites | Se añade: Claude no edita datos del Owner; un dato incompleto se devuelve, no se completa |

## 8. Protocolo de recepcion de datos (propuesta C-01)

Una sola parte escribe a la vez en el worktree y en la rama. Sin reset, clean, stash ni descarte
de cambios. El Coordinator publica en la rama desde su propio clon: lo hace con el turno cedido
y Claude empieza cada turno con `git fetch` y la verificacion de WORKFLOW §3.

1. Claude entrega el turno: `HEAD` = `origin/<rama>`, arbol limpio, sin operaciones en curso.
2. **R1 (recomendado):** el Owner declara el lote en el chat (Ids, textos, `type`, peraltes,
   FK, mapa de renombres); el Coordinator lo registra en `decisions/I-69.md`. Claude escribe y
   publica el RED contra ese lote y devuelve el turno. El lote declarado sirve ademas como
   oraculo independiente del CSV: detecta errores de transcripcion de Excel.
3. El Owner edita solo los catalogos acordados en el worktree, cierra Excel y avisa que
   archivos toco. No ejecuta operaciones Git.
4. Claude verifica que `git status` lista solo esos archivos, registra su SHA-256 y el alcance
   del diff (filas nuevas al final, `displayName` de los Ids mapeados) y reporta todo cambio
   incidental sin corregirlo.
5. Commit dedicado "I-69: datos del Owner ..." solo con esos archivos, sin edicion de Claude,
   con la lista y los SHA-256 en el cuerpo. Despues, GREEN y el resto del gate.

**R2 (alternativa):** el Owner edita primero; Claude commitea solo pruebas y mide su RED en un
checkout temporal de medicion de ese commit en ruta corta (MAX_PATH), retirado al terminar. Evita
la declaracion previa a cambio de un checkout adicional de solo lectura.

## 9. Gates y criterios de salida

| Gate | Resultado verificable | Entrada | Pruebas |
|---|---|---|---|
| F0 | Q-01..Q-04 y C-01..C-03 decididos; revision segun arquetipo; Freeze acordado | Este paquete | — |
| F1 | Altas y renombres de larguero seleccionables, persistentes, dibujables y en BOM | Datos de largueros (§8) | T1..T9, T14 y T15 de largueros |
| F2 | Ambas seguridades disponibles segun Q-01..Q-03, con regresion y distribucion acreditadas (absorbe F3 si se elige B) | Datos de seguridad (§8) y S-04/S-05 si aplican | T10..T15 y regresion T9 |
| F3 | Solo si se elige C: geometria y conteo fisicos | Datos fisicos de C | T7/T13 de C |
| F4 | Preparacion verificable de OV: fixtures legacy y nueva, resultados esperados OV-01..OV-08, identidad del DLL; cierra antes de READY | Gates previos cerrados | — |
| READY | READY-01..09 en orden; despues Candidato, OV y cierre | — | Full Core + UI del Candidato |

Cada gate cierra con RED→GREEN focal y conteo > 0, Core Full local tras el commit de cierre con
arbol limpio, CI propio del SHA de cierre y revision del Coordinator (LIFECYCLE §7; AGENTS).
F1 y F2 son independientes: puede cerrarse primero el que reciba antes sus datos.

## 10. Matriz T1–T15 actualizada

`RED` = comportamiento nuevo (falla antes de los datos o del codigo); `CAR` = caracterizacion de
invariante previo (verde antes y despues, sin fabricar RED). Todas PLANNED.

| ID | Oraculo | Tipo | Proteccion / ubicacion |
|---|---|---|---|
| T1 | Los 4 Ids actuales siguen en la fuente y en `CellBeamBox`; Id inicial medido (P-04) | CAR | `BeamCatalogTests`, `SeccionesCatalogTests`; prueba UI propia `I69*` |
| T2 | Cada alta declarada aparece una vez en filas y en opciones; FK de mensula resuelve; tres mappings; baseline del validador intacta | RED | `ShippedCatalogIntegrityTests` (S-01) + prueba propia |
| T3 | Texto exacto del mapa en `Label`, dropdown y descripcion BOM (`Label · Pn`); Id intacto | RED | Prueba propia; `LargueroBomBuilderTests` como regresion |
| T4 | Diseño legacy con Id real reabre y guarda igual | CAR | `SelectivePalletDesignDocumentTests`, `PersistenceReopenPreservationTests` |
| T5 | Alta → guardar → reabrir conserva `BeamId`/`BeamPeralte` (store y `LoadExisting`) | RED | Prueba propia Core + UI |
| T6 | Editar otra celda/propiedad conserva el larguero elegido | CAR | `SelectiveEditorStateTests` como modelo; archivo propio, no `SelectiveEditorOpenTests` |
| T7 | Alta: `PieceId`, bloque, `PERALTE`/`LONGITUD` e `INICIO_PERFIL` por vista | RED | Resolver y builders F/L/P con catalogo real; DWG en smoke |
| T8 | BOM de alta: `ProfileId`, receta perfil + 2 mensulas por FK, cantidad por instancia; oraculo manual | RED | `SelectiveBomBuilderTests` como modelo |
| T9 | Opciones previas: huella `Category|ProfileId|Length|Quantity` del BOM de un diseño fijo igual antes y despues | CAR | `SelectivePerFondoTests`, `ConsolidatedBomBuilderTests`; I-68 sin edicion |
| T10 | Guarda trasera: fila y titulo en el dialogo Selectivo con su Id; exposicion segun Q-03; ausente en Dinamico | RED | Prueba UI propia; S-05 si aplica |
| T11 | Entrepaño: fila, familia y titulo; con E1 la Parrilla conserva fila, plan y BOM al elegir ambos | RED | Prueba UI propia; `SelectiveParrillaPlacementTests` como regresion |
| T12 | Ambas sobreviven guardar/reabrir/editar/`DeepCopy` y reabrir el dialogo | RED | `SafetySelectionDocumentsTests`, `SelectiveSafetyConfigTests` como modelo |
| T13 | B: componente `Seguridad`, `ProfileId` = Id, `Label`, longitud 0, cantidad manual por rack, × copias; sin instancias en F/L/P | RED (BOM) / CAR (sin dibujo) | `SelectiveBomBuilderTests`, `ConsolidatedBomBuilderTests` |
| T14 | Fallos por frontera (§11) sin cambio para las altas; si Q-03 = solo Selectivo, PushBack las rechaza y no las ofrece | CAR / RED (S-05) | `RackSingleViewPlacementTests`, `RackViewBatchPlanTests`, `PushBackSafetyBomTests` |
| T15 | Catalogos en output y bundle sin perder filas ni encoding; inventario SHA-256 | CAR | `build-bundle.ps1`, `verify-bundle.ps1`; DWG en smoke |

## 11. Owner Validation acotada

OV-01..OV-07 de V1 se conservan, con estos resultados esperados de B: OV-05/OV-06, fila bajo su
titulo con cantidad entera; BOM con la cantidad dada; sin dibujo. Se añade:

| OV | Accion del Owner | Resultado esperado |
|---|---|---|
| OV-08 | Abrir los dialogos de seguridad de PushBack y Dinamico | Las dos piezas no aparecen (si Q-03 = solo Selectivo) |

Sobre el `FINAL_CANDIDATE_SHA`, con el DLL Debug del worktree identificado por SHA-256.

## 12. Validacion disponible y routing

Medido en este relevo (evidencia §17): Core Full y UI Full locales sobre `fd456e8b`, build Debug
de UI y Plugin, AutoCAD 2025 instalado y cerrado. No medido: biblioteca DWG del Owner, AutoCAD
en uso, CI de los commits de este relevo. Ninguna de esas corridas es evidencia de producto
I-69 ni de un gate.

Este encargo no uso ejecucion delegada (AUTOMATION_PLAN §16). Para los gates se propone que
Claude implemente directamente como sesion responsable; si el Coordinator ordena delegacion, se
aplica §16 completo (contrato, planificacion, elegibilidad medida, cesion y verificacion).

## 13. Trazabilidad V1 → V2

| V1 | V2 |
|---|---|
| O-01, O-02 | Entradas de datos del Owner (D-09), §2.1 y §8 |
| O-03 / O-04 / O-05 | Q-01 / Q-02 (A/B/C y cantidad incluidas) |
| O-06 | Q-03 (seguridad) y Q-04 (largueros, etiquetas, default) |
| §3 camino minimo | §3 y §4; pasos 3-4 de V1 conservados |
| P-01..P-11 | §7 (deltas) |
| T1..T15 | §10 (mismos Ids, oraculos concretos) |
| OV-01..OV-07 | §11, mas OV-08 |
| F0..F4, READY | §9; F3 se absorbe en F2 si se elige B |

IMPLEMENTATION AUTHORIZATION = NO
