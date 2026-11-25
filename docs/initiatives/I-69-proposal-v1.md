# I-69 — Proposal V1: ampliacion de componentes Selectivo

Frozen: NO
Estado: DRAFT — BLOCKED OWNER DECISION
Workflow: V2
Arquetipo inicial: EXTENSION
Arquetipo operativo: FOUNDATION EVOLUTION provisional; EXP-09 abierto
IMPLEMENTATION AUTHORIZATION = NO

Propuesta anterior a implementacion. No es Freeze ni autorizacion para alterar
producto. Hechos Git, hashes, mediciones e inventarios actuales viven en
[Discovery/evidencia](../automation/evidence/I-69-evidence.md).

## 1. Objetivo, alcance y agrupacion

Ampliar las opciones existentes de larguero por celda/nivel y de seguridad Selectivo;
incorporar Guarda trasera y Entrepaño caída de agua; actualizar las etiquetas de
largueros que identifique el Owner. Conservar identidades y diseños existentes.

La unidad I-69 agrupa la mejora de componentes del editor por mandato expreso del
Owner. Sus mecanismos de catalogo e identidad/persistencia permiten una propuesta
comun. Si la geometria final exige otra unidad de entrega, resolver antes del Freeze;
no se reclama otra iniciativa para realizar este Discovery.

Fuera de alcance: larguero distinto por lado, duplicacion de Selectivo, rediseño de
editor/niveles, live-edit, Workspace, ID23, rediseño de consolidacion BOM, RAM,
expresiones nuevas y plataforma general de catalogos. I-68/XlsxWriter queda fuera.
I-25 sigue como antecedente diferido, sin cierre ni absorcion implicitos.

## 2. Decisiones reservadas al Owner

| ID | Dato/decision pendiente | Motivo |
|---|---|---|
| O-01 | Nuevos largueros: pieza/codigo estable, displayName final, peraltes, mensula y geometria existente reutilizada o bloques/mates reales nuevos | El nombre no define pieza dibujable ni receta |
| O-02 | Mapa Id existente → displayName final | Conservar Id y cambiar exclusivamente etiquetas pedidas |
| O-03 | Guarda trasera: A/B/C, Id comercial y nombre final | No hay fila/regla funcional; el titulo TRASERA no implementa dibujo |
| O-04 | Entrepaño: A/B/C, Id/nombre, variante alternativa de Parrilla o pieza independiente/coexistente | La segunda PARRILLA carece de contrato seguro de coexistencia |
| O-05 | Si B, cantidad manual total por rack y unidad comercial aceptadas. Si C, vistas, posiciones, dimensiones, lados/fondos/celdas, conteo y bloques reales | No inferir cantidad fisica desde una etiqueta ni multiplicarla dos veces |
| O-06 | Alcance de etiquetas/opciones compartidas en editor de larguero, Dinamico y PushBack | La autoridad actual no es exclusiva Selectivo |

A = UI + persistencia; B = UI + persistencia + BOM; C agrega geometria a B.
El patron generico existente es B: entero manual positivo por rack, longitud BOM 0,
una pieza de receta por unidad. A requiere comportamiento adicional porque hoy la
seleccion generica positiva entra en BOM y cero/vacio no la conserva.

No se piden dimensiones/bloques para B. Si se elige C se solicitara solo la
informacion fisica faltante de esa pieza. Los datos comerciales opcionales no se
inventan. Si se reutiliza geometria basta identificarla inequívocamente y verificar
que sus parametros correspondan a la pieza.

## 3. Camino minimo propuesto

1. Completar O-01..06; cerrar tabla de altas/renombres y contrato por seguridad.
   Revisar efectos compartidos/default y los riesgos de segunda PARRILLA o
   geometria parametrica solo cuando las piezas los hagan aplicables.
2. Resolver EXP-09 y realizar revision Architect segun arquetipo. Despues acordar
   Freeze versionado y obtener autorizacion de implementacion.
3. Largueros compatibles: anexar filas LARGUERO en secciones.csv; modificar solo
   displayName aprobado de IDs actuales; agregar mappings/conexiones/FK reales
   necesarios. Reutilizar provider, opcion, dropdown, BeamId, DTO y receta.
4. Seguridad B: anexar registros de seguridad.csv usando ElementId/Quantity y
   BOM manual, despues de resolver visibilidad en otros sistemas. C solo con
   reglas fisicas aceptadas y un punto unico Application compartido por dibujo/BOM.
5. Ejecutar regresiones, empaquetar catalogos y preparar smoke Owner sobre el
   Candidato exacto siguiendo READY. Inspeccion estatica no acredita PASS funcional.

Filas nuevas al final, sin reordenar ni cambiar encoding. El mandato permite preparar
renombres de displayName; no extiende esa autorizacion a IDs, otras columnas ni
sustitucion masiva. No se crea catalogo paralelo ni migracion por renombre visible.

## 4. Freeze propuesto — invariantes aun no congelados

| Clausula | Contrato propuesto | Obligacion |
|---|---|---|
| P-01 Autoridad | CSV distribuido mantiene autoridad; Id identifica, Label presenta. Lista cerrada O-01/02 | T1–3, T15 |
| P-02 Persistencia | BeamId/BeamPeralte y ElementId/Quantity siguen siendo identidad/datos; renombres no cambian Id, casing ni schema | T4–6, T12 |
| P-03 Legacy | Reabrir/editar conserva seleccion y authored/metadata en sus limites actuales. Etiqueta vigente sale del catalogo | T3–6, T12 |
| P-04 Default | Mantener mecanismo actual del dropdown; medir orden/primera opcion con nombres finales. Cambio del Id inicial requiere decision explicita antes del Freeze | T1–3/T6 y comprobacion de default |
| P-05 Geometria | Propiedades, bloques y mates reales existentes o autorizados; ninguna dimension inferida del texto | T7 y smoke |
| P-06 BOM | ProfileId procede del Id; cantidades/copias conservan receta. Renombre cambia texto, no identidad/cantidad; sin rediseño de consolidacion | T8–9, T13 |
| P-07 Seguridad | Cada pieza declara A/B/C, familia/coexistencia, cantidad/unidad y vistas. Segunda PARRILLA exige contrato explicito | T10–13 |
| P-08 Fallos | Mantener politica por frontera del Discovery §11, sin sustitucion silenciosa nueva ni fail-closed global inventado | T14 |
| P-09 Consumidores | Cerrar O-06, caracterizar etiquetas/filtros compartidos y aplicar solo alcance autorizado | T1/T3/T9/T13 y consumidores afectados |
| P-10 Distribucion | Catalogos aprobados llegan al plugin/bundle con identidad verificada; biblioteca DWG instalada se valida por separado | T15 y smoke |
| P-11 Limites | Sin cambios de no-objetivos/I-68. Schema, reglas, fallos o geometria no soportada disparan STOP y revision antes de implementarlos | Diff contra Freeze y matriz M |

Aliases legacy: ninguno requerido al conservar Id. Renombrar/eliminar Id reabre
P-02/P-03 y M-02; no se resuelve reemplazando texto. No se promete preservacion
recursiva de unknown fields ni bloqueo que el codigo no ofrece ante catalogo ausente.

Consumes: catalogos CSV/compatibilidad legacy; identidad/vistas; authored/effective;
preservacion de desconocidos en limites declarados; copia central de seguridad;
I-61 en coordinacion. Extends previsto: registros/mappings compatibles. Introduces:
ninguna foundation/schema/migracion decidida. Discovery verifica fuentes y simbolos.

## 5. Matriz T1–T15

Todas las filas: PLANNED, no ejecutadas para producto I-69. Las suites citadas son
protecciones existentes; no se afirma que ya cubran piezas no definidas. Cada
comportamiento nuevo tendra RED→GREEN focal con seleccion mayor que cero.
Invariantes previas tendran caracterizacion/verde de regresion, sin fabricar RED.

| ID | Escenario y oraculo | Proteccion / trabajo previsto |
|---|---|---|
| T1 | Cuatro opciones actuales conservan Id y siguen seleccionables | SeccionesCatalogTests/BeamCatalogTests y UI real CellBeamBox |
| T2 | Cada alta una sola vez en fuente y dropdown | Auditoria de unicidad + provider/UI; RED por ausencia del Id aprobado |
| T3 | Renombre muestra texto exacto aprobado, Id intacto | UI/Label/BOM, mapa O-02 y orden/default; RED contra texto anterior |
| T4 | Diseño antiguo reabre y vuelve a guardar con misma seleccion | SelectivePalletDesignDocumentTests y LoadExisting; fixture con Id real y metadata |
| T5 | Alta → guardar → reabrir conserva Id/peralte | DTO/store + UI/AutoCAD; comparar identidad, no solo etiqueta |
| T6 | Editar otra propiedad/celda/fondo conserva larguero elegido | SelectiveEditorStateTests/UI; archivo I-69 propio para evitar hotfile I-64 |
| T7 | PieceId, bloque, PERALTE/LONGITUD y offsets correctos por vista | GeometryResolver/Frontal/Lateral/Planta/PostGeometry; fixture real + DWG en smoke |
| T8 | BOM de alta tiene Id, receta, cantidad y mensulas correctas | LargueroBomBuilderTests/SelectiveBomBuilderTests; oraculo manual pequeño, independiente del builder |
| T9 | Opciones previas conservan geometria/receta/cantidad por fondo/copias; solo texto aprobado cambia | SelectivePerFondoTests/ConsolidatedBomBuilderTests; I-68 como regresion sin edicion |
| T10 | Guarda trasera aparece con identidad correcta | UI real; RED por ausencia actual; alcance compartido O-06 |
| T11 | Entrepaño aparece con identidad/familia correctas | UI real; RED por ausencia; coexistencia/alternativa segun O-04 |
| T12 | Ambas seguridades sobreviven guardar/reabrir/editar/DeepCopy | SafetySelectionDocumentsTests/SelectiveSafetyConfigTests + DTO/store/UI |
| T13 | A/B/C coincide en BOM/geometria; cantidad/copias correctas | B: cantidad manual por rack, sin dibujo. A/C: oraculos O-03..05; C con plan fisico comun e independencia de flags visuales |
| T14 | Perfil, FK destino, mapping, DWG, conexion y seguridad ausentes siguen politica actual por frontera | ID desconocido no vacio; PostGeometry/RackSingleViewPlacement/RackViewBatchPlan; conservar gates authored/bindings |
| T15 | Catalogos llegan a output/bundle sin perder filas/encoding | build-bundle/verify-bundle, inventario/SHA-256; instalacion y catalogo efectivo. DWG externo requiere evidencia separada |

Comprobaciones transversales necesarias: orden/default, filtros compartidos,
coexistencia de Parrilla y parametros DWG reales si hay geometria nueva. Se vinculan
a estas filas; no abren validacion amplia de Selectivo.

La consolidacion de igual Id/longitud/receta con peraltes distintos es limite previo.
Si una pieza nueva exige distinguirlos en total, resolver alcance antes del Freeze,
sin cambiar incidentalmente el motor BOM.

Entrega futura: focales/relevantes durante iteracion; Core Full local despues del
commit limpio de cierre de gate; CI propio con cuatro jobs; Full Core/UI del
Candidato y Owner Validation segun AGENTS/WORKFLOW. Ausencia de SDK no permite
sustituir la clase local por CI ni cerrar un gate.

## 6. Owner Validation acotada

Preparar una copia de diseño legacy pequeño y un diseño con altas, con resultados
esperados O-01..06. Usar DLL/bundle del FINAL_CANDIDATE_SHA exacto. En esta fase no
hay DLL/Candidato I-69.

| OV | Accion del Owner | Resultado esperado |
|---|---|---|
| OV-01 | Abrir dropdown por celda/nivel | Lista completa aprobada, sin duplicados, default acordado |
| OV-02 | Revisar renombres | Coincidencia literal O-02; mismo Id seleccionado |
| OV-03 | Seleccionar cada alta y su peralte | Opcion y propiedades aplicables correctas |
| OV-04 | Guardar/cerrar/reabrir/editar | Misma seleccion en una celda legacy y una nueva |
| OV-05 | Elegir Guarda trasera | Disponible, configurable y persistente segun A/B/C |
| OV-06 | Elegir Entrepaño y comprobar relacion con Parrilla | Disponible, persistente y con coexistencia/exclusion acordada |
| OV-07 | Revisar BOM y vistas aplicables | IDs/textos/cantidades/copias/geometria segun contrato de cada pieza |

Registrar evidencia del mismo Candidato; no ampliar a Workspace, expresiones, RAM,
exportador XLSX ni a todo Selectivo.

## 7. Gates y criterios de salida

| Gate | Resultado util | Estado |
|---|---|---|
| F0 | Discovery, decisiones Owner, materialidad, revision y Freeze acordado | PARCIAL: caracterizacion entregada; decisiones/Freeze pendientes |
| F1 | Altas/renombres de largueros seleccionables y compatibles | NO AUTORIZADO |
| F2 | Ambas seguridades disponibles, persistentes y con alcance acordado | NO AUTORIZADO |
| F3 | Regresion BOM/geometria/save-reopen y distribucion acreditadas | NO AUTORIZADO |
| F4 | Preparar OV: escenarios, fixtures, resultados esperados y procedimiento de entrega; cerrar antes de READY | NO AUTORIZADO |
| READY | Orden READY-01..09, declaracion de Candidato; despues OV/cierre | NO INICIADO |

F1–F3 pueden consolidarse si el alcance congelado es trivial, sin omitir evidencia.
F4 solo entrega la preparacion verificable de OV y cierra antes de READY-02; no
incluye su ejecucion. Una vez cerrados los gates y cumplido READY-01..09 se declara
el Candidato. Despues se ejecuta Owner Validation sobre ese SHA y se tramita el
cierre segun Workflow. OV y cierre no son gates pendientes que READY deba esperar.

## 8. Routing I-61 inicial

| Trabajo | Perfil propuesto | Condicion |
|---|---|---|
| Catalogo/modelo/consumidores | Balanced | Lectura acotada; el Discovery presente ya identifica el camino |
| Identidad/persistencia/materialidad | Balanced; Deep si persiste ambiguedad | Resolver EXP-09 y revisar contrato completo |
| Filas/etiquetas/UI minima | Routine (nivel Eficiente), o Balanced | Solo tras Freeze/autorizacion; verificar elegibilidad real de celda/modelo |
| Geometria/BOM/regresion | Balanced | Escalar si aparece contrato material |
| Revision antes de Freeze | Architect independiente segun LIFECYCLE | FOUNDATION EVOLUTION provisional exige revision adversarial |
| Conformidad final | Revisor independiente + Coordinator | Version completa, disposicion por finding y evidencia exacta |

Antes de ejecucion delegada formal: medir entorno/celdas, emitir contrato/paquete/
relevo con referencias exactas, ceder worktree de forma exclusiva, Controller de
solo lectura y custodia conforme AUTOMATION_PLAN §16. Esta tabla no acredita
EXECUTION_VERIFIED ni autoriza Worker. GATE PASS pertenece al Coordinator.

## 9. Condicion para continuar

El siguiente paso depende de O-01..06, no de permiso para repetir Discovery ya
autorizado. Con esos datos se concreta la propuesta, resuelve arquetipo y revisa
la version completa. Freeze/autorizacion de producto son pasos posteriores.

IMPLEMENTATION AUTHORIZATION = NO
