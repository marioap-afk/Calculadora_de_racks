# I-68 — Proposal V2: BOM Excel Native Analysis (ID72)

Frozen: NO

Unit: `I-68`. Workflow: V2. Arquetipo propuesto: FOUNDATION EVOLUTION (M-04, M-06). Rama `feature/bom-excel-analisis-nativo`, base `04a70414`.
Autoridad funcional: [especificacion literal](I-68-especificacion-id72.md). Discovery: [evidencia](../automation/evidence/I-68-evidence.md). Decisiones y disposicion de la ronda 1: [decisions/I-68.md](../automation/decisions/I-68.md). ADR: [0051](../adr/0051-exportacion-excel-nativa-sobre-la-plataforma.md), propuesto.

**IMPLEMENTATION AUTHORIZATION = NO** hasta que el Owner resuelva D-0 y el Freeze quede aceptado. Esta version describe la opcion recomendada; otra opcion de D-0 exige otra propuesta.

## 0. Decisiones reservadas al Owner

| ID | Decision | Recomendacion | Consecuencia si se acepta |
|---|---|---|---|
| **D-0** | Via para Table y PivotTable: A extender `XlsxWriter`; B ClosedXML; C2 Open XML SDK; D otra (evidencia §5) | **A**, con ADR-0051 | La especificacion exige STOP antes de editar OOXML a mano. Bloquea el Freeze |
| D-1 | «Nivel» es el peldaño del BOM (`Componente`/`Pieza`), no el nivel fisico, que el BOM no conserva | Usar la columna productiva | AC-05 y AC-06 se cumplen sobre esos dos valores. **No aplican**, porque no pueden ocurrir: AC-09, SC-11, la fila «elementos sin nivel» de la matriz y el dato de OV «item sin nivel». Filtrar por nivel fisico seria expansion de dominio, excluida |
| D-2 | Alcance: solo la exportacion consolidada (RACKBOMTOTAL), unica con Rack y Copias | Excluir la exportacion de un rack | `BomXlsxExporter` no cambia; las ocho entradas a `RackBomWindow` (siete editores y doble clic en el consolidado) siguen sin Table ni dinamica |
| D-3 | Estado inicial del filtro Nivel | «(Todas)» con nota visible en `Resumen`, por la instruccion del Owner de no imponer Pieza | Con «(Todas)» se suman componente y pieza: las filas donde coinciden salen dobles (Larguero de Selectivo, Separador, Tope, Seguridad, Parrilla…) y el total general mezcla ambos. Con Pieza faltan las Camas de Dinamico y Push Back; con Componente faltan los racks planos. **Ningun estado da el BOM fisico completo.** Alternativa medida (P-15): abrir en Pieza |
| D-4 | Validador independiente `DocumentFormat.OpenXml` solo en `tests/` (AGENTS «Dependencias») | Si | El producto sigue sin paquetes. Si el validador rechazara la definicion minima que Excel acepta, se registra y queda como comprobacion de las demas partes |
| D-5 | Cantidad total como valor (preferencia de la especificacion) | Valor | Ver consecuencia en §2. La alternativa formula se midio solo sobre otra definicion |

## 1. Autoridad

- **Cantidad total**: derivada en un unico punto del modelo de exportacion consolidado, como numero, no como formula. No se persiste ni entra a `BillOfMaterials`.
- La normalizacion de copias (`< 1` = 1) ya existe en tres sitios: `ConsolidatedBomBuilder`, el exportador XLSX y el CSV. El exportador usa la que ya tiene; no se refactoriza. I-02 e I-04 enlazan los valores.
- Table y PivotTable viven solo en `Application/Bom`. Domain, Plugin y los constructores BOM no las conocen.

## 2. Semantica de cantidades

**Cantidad** en «Por rack», siempre para **una** copia del rack: Componente = cantidad del componente; Pieza = pieza por unidad × cantidad del componente; plana = cantidad de la linea.

**Copias**: colocaciones fisicas del rack, el maximo de referencias directas entre sus bloques de vista. No cuenta vistas. Limite vigente que no se cambia: una misma vista insertada dos veces cuenta 2.

**Cantidad total = Cantidad × Copias mostrada**, una sola vez.

| Fila | Cantidad | Copias | Cantidad total |
|---|---|---|---|
| Ejemplo obligatorio | 4 | 3 | **12** |
| Pieza: receta 3 por componente, 2 componentes | 6 | 3 | **18** |
| Componente, cantidad 2 | 2 | 3 | **6** |
| Pieza plana (Cama) | 30 | 1 | **30** |
| Copias almacenadas < 1 | la misma | se muestra 1 | Cantidad × 1 |

**Es un valor exportado, no una formula.** Si el usuario edita Cantidad o Copias, Cantidad total no se recalcula; una fila nueva la trae vacia hasta que se escriba. Solo editar Cantidad total mueve el resumen (P-13).

«Total del dibujo» **no** recibe Cantidad total: no tiene Copias y su Cantidad ya esta multiplicada.

## 3. Contrato del libro

Hojas, en orden: `Por rack`, `Total del dibujo`, `Resumen` (nueva). Los encabezados productivos no se renombran.

**`Por rack`, Table nativa `RackCadBOM`.** Columnas actuales en su orden y `Cantidad total` al final. Rango exacto. Autofiltro. Estilo `TableStyleMedium2` con filas alternadas. Solo partidas de detalle.

**`Resumen`, PivotTable nativa `ResumenBOM`.** Fuente: el nombre de la tabla, nunca un rango.

| Area | Campo pedido | Encabezado productivo |
|---|---|---|
| Filtro 1 | Rack | `Rack` |
| Filtro 2 | Nivel | `Nivel` |
| Fila 1 | Descripcion | `Descripcion` |
| Fila 2 | Longitud | `Longitud (in)` |
| Valor | SUM(Cantidad total) | `Cantidad total`, «Suma de Cantidad total» |

**Definicion minima.** La cache y los campos no listan valores; Excel los reconstruye al abrir (`refreshOnLoad`, `saveData="0"`). Evita las formas que Excel no abre (P-09). Si D-3 eligiera Pieza, solo Nivel listaria sus dos literales fijos (P-15).

**Visible al abrir.** La hoja lleva las celdas ya calculadas: se ven en Vista protegida (P-14). Al abrir normalmente, o al habilitar la edicion, Excel las sustituye por su resultado (P-03, P-13, P-14). El orden previo es determinista e independiente de la cultura, y puede diferir del que aplique Excel. La agrupacion previa ignora mayusculas y respeta acentos, como Excel (P-10).

**Nota visible** en `Resumen!D1`, fuera del area de la dinamica: «Con Nivel en (Todas) se suman componentes y sus piezas; para contar piezas elige Pieza. Las camas de Dinamico y Push Back solo aparecen como Componente.» Se conserva al actualizar (P-15).

**Actualizacion.** Enlazada a la tabla: una fila nueva amplia la fuente. Tras editar, Actualizar o reabrir refleja el cambio (P-13). Sin una de esas acciones no cambia.

## 4. Casos

- **Longitud:** numerica, en pulgadas, sin unidad y sin conversion. Sin longitud significativa = `0`, como hoy. Misma Descripcion con distinta Longitud queda desglosada, en orden numerico (P-13).
- **Rack:** nombre logico mostrado hoy, o `(sin nombre)`; no se sustituye por GUID. Nombres iguales comparten filtro; Excel funde tambien los que solo difieren en mayusculas.
- **Descripcion vacia** (posible en `BomBuilder`): se conserva y se cuenta (P-08, P-13).
- **Compartidos:** una fila por partida del BOM, sin duplicar para ningun filtro.
- **BOM vacio:** dos casos. Sin racks: la tabla lleva una fila en blanco, porque Excel no abre una tabla con solo encabezado (P-07), y la dinamica sigue presente, mostrando «(en blanco)». Rack con BOM sin filas: igual en `Por rack`; `Total del dibujo` conserva la fila sintetica que ya produce el constructor.

## 5. Escritor

Datos BOM → modelo de exportacion → escritor → tabla y dinamica. `XlsxWriter.Build` conserva firma y salida para quien no pida Table. Identificadores internos fijos y entradas ZIP con fecha constante, para bytes identicos ante la misma entrada. Nombres de metodos y tipos no se congelan.

## 6. Fallos (M-04)

Generar no requiere Excel. Un error sigue el camino actual: `MessageBox` y ningun archivo escrito.

El escritor podria generar sin error un archivo que Excel no abre. Las tres formas medidas son: tabla con solo encabezado, valores listados con Longitud toda entera sin `containsInteger`, y variantes de mayusculas listadas aparte (P-07, P-09). I-11, I-12 e I-18 las impiden.

**Si el refresco de apertura no ocurre**, la hoja muestra las celdas calculadas al exportar, correctas para ese momento, pero la dinamica no tiene valores en sus filtros hasta pulsar Actualizar, que la recupera (P-14). Es el mismo estado de la Vista protegida. Se observo una vez sin causa conocida en un bucle de automatizacion (P-12) y no se reprodujo al intentarlo (P-14).

## 7. Compatibilidad y no-objetivos

Sin cambios: CSV, exportacion de un rack, `Total del dibujo`, constructores BOM, `ConsolidatedBomBuilder`, persistencia, comandos. La posicion de las columnas actuales se conserva.

**Diferencia preexistente que se hara visible.** `Total del dibujo` une por categoria, longitud y receta, ignorando Perfil y Descripcion: Camas de distinto BFR o largueros de distinto peralte colapsan bajo la primera descripcion, mientras `Por rack` y `Resumen` los mantienen separados. No se corrige aqui.

Excel: medido solo en 16.0 build 20527, interfaz en español; otras versiones sin verificar.

No-objetivos: ID23, costos, precios, proveedores, formulas BOM, Expression Engine, RAM Elements, reglas o partidas nuevas, consolidacion fisica nueva, nivel fisico en el BOM, Excel COM en produccion, paquetes en producto.

## 8. Punto de extension

```text
Consumes: BillOfMaterials, ConsolidatedBom, ConsolidatedBomBuilder, XlsxWriter
Extends: ninguno
Introduces: capacidad de Table y PivotTable nativas en XlsxWriter (punto de extension, M-06)
```

La entrada factual en `docs/FOUNDATIONS.md` se redacta antes de READY-04 (LIFECYCLE §4.1). Un solo consumidor en esta iniciativa.

## 9. Invariantes y pruebas

`RED` = falla por asercion antes de implementar. `CARACT` = fija lo actual; se escribe antes del primer cambio del escritor y pasa antes y despues. `GUARDA` = pasa antes y despues.

| ID | Invariante | Cubre | Clase |
|---|---|---|---|
| I-01 | Cantidad total = Cantidad × Copias una vez (4 × 3 = 12) | AC-01 | RED |
| I-02 | Multiplicador = Copias mostrada; < 1 se trata como 1 | AC-16 | RED |
| I-03 | Pieza: receta × componente × copias = 18 | AC-16 | RED |
| I-04 | Pieza: Σ Cantidad total en «Por rack» = Σ Cantidad en «Total del dibujo», por categoria, perfil y longitud a 2 decimales | SC-2 | RED |
| I-04b | Componente: igual, excluyendo la fila sintetica de racks planos o vacios | SC-2 | RED |
| I-05 | «Total del dibujo» conserva columnas, valores y la union de §7 | SC-2 | CARACT |
| I-06 | Copias, Longitud, Cantidad y Cantidad total son celdas numericas | AC-08 | RED |
| I-07 | Table `RackCadBOM`: rango exacto, encabezados = fila 1, autofiltro, filas alternadas, solo detalle | AC-12 | RED |
| I-08 | PivotTable: fuente = nombre de tabla sin rango; filtros, filas y valor del §3; agregacion suma | AC-13, AC-14 | RED |
| I-09 | `refreshOnLoad` activo; celdas previas = Σ del detalle | AC-07 | RED |
| I-10 | Celdas previas: igual Descripcion y Longitud suman; distinta separa; orden numerico (el oraculo real es S-06) | AC-02, 03, 08 | RED |
| I-11 | La definicion no lista valores: enteros y variantes de mayusculas no alteran el XML de cache | §6 | RED |
| I-12 | BOM vacio, con cero racks y con un rack sin filas: nunca tabla solo con encabezado; dinamica presente; sin excepcion | AC-17 | RED |
| I-13 | Descripcion vacia se conserva y se cuenta | §4 | RED |
| I-14 | Conteo independiente: Selectivo de C vanos y un fondo da C+1 cabeceras; Cantidad total = (C+1) × Copias; una sola fila por receta | AC-10 | RED |
| I-15 | Misma entrada, bytes identicos; fechas de entrada ZIP constantes; nombres estables | Determinismo | RED |
| I-16 | `BillOfMaterials`, `ConsolidatedRackBom`, constructores y Domain sin conceptos Excel; producto sin `PackageReference` nuevas | ADR-0012 | GUARDA |
| I-17 | CSV y exportacion de un rack sin cambios | AC-18 | CARACT |
| I-18 | Paquete valido: XML bien formado, toda relacion resuelta, content-types completos, validador independiente sin errores | AC-11 | RED |
| I-19 | Cada uno de los seis sistemas de RACKBOMTOTAL cumple I-01, I-04, I-04b e I-06..I-08 con su constructor real | AC-18 | RED |
| I-20 | Nota visible presente y fuera del area de la dinamica | D-3 | RED |

## 10. Regresion por sistema

Con BOM real de su constructor, por `ConsolidatedBomBuilder` y la exportacion.

| Sistema | Forma | Caso que debe verificar |
|---|---|---|
| Selectivo | arbol | Larguero y Separador con componente y pieza iguales; cabeceras compartidas (I-14); copias > 1 |
| Dinamico | arbol | Cama sin piezas; dos piezas identicas en un componente |
| Push Back | arbol | Cama sin piezas; botas con longitud 0 |
| Cantilever | arbol | recetas distintas con iguales columnas visibles |
| Cabecera | plana | solo filas Pieza; placa con longitud 0 |
| Cama | plana | solo filas Pieza; tope, rodillo y freno con longitud 0 |
| Larguero | arbol | fuera de RACKBOMTOTAL; solo I-17 |

## 11. Validacion

A: estructura sobre el ZIP. B: `OpenXmlValidator` en `tests/` (D-4). C: humo en Excel real con script versionado; esperados calculados desde el rango de la tabla, no desde la dinamica. No entra a CI ni a las suites ordinarias. Se ejecuta al cerrar G2, G4 y **sobre `FINAL_CANDIDATE_SHA`**.

| ID | Comprobacion | Cubre |
|---|---|---|
| S-01 | Abre sin reparacion: titulo y registros de reparacion | AC-11 |
| S-02 | ListObject y PivotTable nativos; fuente = tabla | AC-12, 13 |
| S-03 | Filtro Rack, uno y varios | AC-04 |
| S-04 | Filtro Nivel, ambos valores | AC-05 |
| S-05 | Rack + Nivel | AC-06 |
| S-06 | Misma Descripcion y Longitud suman; distinta separa; orden numerico con 9, 10, 100, 144 | AC-02, 03, 08 |
| S-07 | Total = SUM de la columna de la tabla | AC-07 |
| S-08 | Agregacion = xlSum | AC-14 |
| S-09 | Editar `Cantidad total`, y añadir fila con su valor, y Actualizar: cambia. Editar solo Copias o Cantidad: no cambia | AC-15 |
| S-10 | Vista protegida visible; al habilitar edicion refresca | §3 |
| S-11 | Con Excel ya abierto y otro libro; y sin refresco, Actualizar recupera | §6 |
| S-12 | BOM vacio abre sin reparacion | AC-17 |

## 12. Owner Validation

Con el DLL de `FINAL_CANDIDATE_SHA`, un dibujo con varios racks, Copias > 1, descripciones repetidas y longitudes distintas; RACKBOMTOTAL → Exportar Excel. «Sistemas del dibujo» = Selectivo, Dinamico, Push Back, Cantilever, Cabecera y Cama, los disponibles; Larguero no entra a RACKBOMTOTAL.

| OV-id | Escenario | Disparador | Sistemas | Datos | Resultado esperado | Unidad | Hito intermedio |
|---|---|---|---|---|---|---|---|
| OV-1 | Cantidad total | Columna nueva | Sistemas del dibujo | Nuevo | = Cantidad × Copias, numerica | I-68 | no |
| OV-2 | Filtros de la Table | Table nueva | Sistemas del dibujo | Nuevo | Tabla nativa con filtros | I-68 | no |
| OV-3 | Campos de la dinamica | Dinamica nueva | Sistemas del dibujo | Nuevo | Rack, Nivel, Descripcion → Longitud, suma | I-68 | no |
| OV-4 | Filtro Rack | Dinamica nueva | Varios racks | Nuevo | Solo ese rack | I-68 | no |
| OV-5 | Filtro Nivel | Dinamica nueva | Sistemas del dibujo | Nuevo | Componente o Pieza | I-68 | no |
| OV-6 | Filtro combinado | Dinamica nueva | Varios racks | Nuevo | Interseccion correcta | I-68 | no |
| OV-7 | Totales SUM | Dinamica nueva | Sistemas del dibujo | Nuevo | Igual a sumar `Cantidad total` filtrada. Comparar con `Total del dibujo` por categoria y longitud, no por descripcion (§7) | I-68 | no |
| OV-8 | Actualizar tras editar la columna `Cantidad total` | Enlace a la tabla | Uno | Nuevo | El resumen cambia; editar solo Copias o Cantidad no lo cambia | I-68 | no |
| OV-9 | Sin aviso de reparacion | Paquete nuevo | Sistemas del dibujo | Nuevo | Abre limpio con doble clic | I-68 | no |
| OV-10 | Vista protegida | Celdas previas | Uno | Nuevo | Resumen visible; al habilitar sigue correcto | I-68 | no |
| OV-11 | BOM vacio, si se puede provocar | Caso vacio | Uno | Nuevo | Abre sin reparacion | I-68 | no |

**Cierre (SC-19):** ID72 solo se declara CLOSED tras evidencia exact-SHA y Owner Validation de `FINAL_CANDIDATE_SHA`, conforme a WORKFLOW §11.5.

## 13. Gates

Primero I-05 e I-17 (CARACT), antes del primer cambio del escritor.

| Gate | Resultado verificable |
|---|---|
| G0 | Discovery, D-0..D-5 resueltas, ADR-0051 aceptado y Freeze |
| G1 (F1) | Cantidad total y Table nativa; I-01..I-07, I-13, I-16 |
| G2 (F2) | PivotTable nativa; I-08..I-12, I-15, I-18, I-20; S-01..S-11 |
| G3 (F3) | Matriz por sistema, multi-rack y vacio; I-14, I-19, S-12; rendimiento caracterizado |
| G4 (F4) | Humo completo sobre el SHA y conformidad |
| READY | READY-01..09, Candidato, humo sobre `FINAL_CANDIDATE_SHA`, OV |

## 14. Enrutamiento

Coordinator: Discovery, Freeze y gates. Caracterizacion BOM: Balanced, ya ejecutada. Cantidad total y Table: Balanced. PivotTable: Deep. Humo: local, sin Computer Use salvo necesidad. Conformidad: revision independiente (LIFECYCLE §9). Ningun ejecutor declara GATE PASS.
