# I-68 — Proposal V1: BOM Excel Native Analysis (ID72)

Frozen: NO

Unit: `I-68`. Workflow: V2. Arquetipo propuesto: FOUNDATION EVOLUTION (M-06). Rama `feature/bom-excel-analisis-nativo`, base `04a70414`.
Autoridad funcional: [especificacion literal](I-68-especificacion-id72.md). Discovery: [evidencia](../automation/evidence/I-68-evidence.md). Decisiones: [decisions/I-68.md](../automation/decisions/I-68.md).

**IMPLEMENTATION AUTHORIZATION = NO** hasta que el Owner resuelva D-0 y el Freeze quede aceptado. Esta version describe la opcion recomendada; cualquier otra de D-0 exige otra propuesta.

## 0. Decisiones reservadas al Owner

| ID | Decision | Recomendacion | Consecuencia si se acepta |
|---|---|---|---|
| **D-0** | Via para Table y PivotTable: A extender `XlsxWriter`; B ClosedXML; C2 Open XML SDK; D otra (evidencia §5) | **A** | La especificacion exige STOP antes de editar OOXML a mano. Bloquea el Freeze |
| D-1 | «Nivel» es el peldaño del BOM (`Componente`/`Pieza`), no el nivel fisico del rack, que el BOM no conserva | Usar la columna productiva | AC-05, AC-06 y AC-09 se cumplen sobre esos valores. Filtrar por nivel fisico seria expansion de dominio, excluida por la especificacion |
| D-2 | Alcance: solo la exportacion consolidada (RACKBOMTOTAL), unica con Rack y Copias | Excluir la exportacion de un rack | `BomXlsxExporter` y `RackBomWindow` no cambian |
| D-3 | Estado inicial del filtro Nivel | «(Todas)» | Las filas donde componente y pieza coinciden (Separador, Tope, Seguridad…) suman ambas; para piezas se elige Pieza. Pieza tampoco es completa: las Camas de Dinamico y Push Back solo existen como Componente |
| D-4 | Validador independiente `DocumentFormat.OpenXml` solo en `tests/` (AGENTS «Dependencias») | Si | El producto sigue sin paquetes |

## 1. Autoridad

- **Cantidad** y **Copias**: no cambian de dueño (evidencia DC-01, DC-02).
- **Cantidad total**: derivada en un unico punto del modelo de exportacion consolidado, como valor numerico, no como formula. No se persiste ni entra a `BillOfMaterials`.
- El multiplicador es el mismo valor de Copias que muestra la fila.
- Los conceptos de Table y PivotTable viven solo en `Application/Bom`. Domain, Plugin y los constructores BOM no los conocen.

## 2. Semantica de cantidades

| Fila de «Por rack» | Cantidad | Copias | Cantidad total |
|---|---|---|---|
| Ejemplo obligatorio | 4 | 3 | **12** |
| Pieza: receta 3 por componente, 2 componentes | 3 × 2 = 6 (una copia) | 3 | **18** |
| Componente, cantidad 2 | 2 | 3 | **6** |
| Pieza plana (Cama) 30 | 30 | 1 | **30** |
| Copias almacenadas < 1 | la misma | se muestra 1 | Cantidad × 1 |

«Total del dibujo» **no** recibe Cantidad total: no tiene Copias y su Cantidad ya esta multiplicada.

## 3. Contrato del libro

Hojas, en orden: `Por rack`, `Total del dibujo`, `Resumen` (nueva). Los encabezados productivos no se renombran.

**`Por rack`, Table nativa `RackCadBOM`.** Columnas actuales en su orden y `Cantidad total` al final. Rango exacto: encabezado + una fila por partida. Autofiltro. Estilo `TableStyleMedium2` con filas alternadas. Sin subtotales, totales, filas decorativas ni vacias, salvo la fila exigida por Excel en un BOM vacio (§4).

**`Resumen`, PivotTable nativa `ResumenBOM`.** Fuente: el nombre de la tabla, nunca un rango.

| Area | Campo pedido | Encabezado productivo |
|---|---|---|
| Filtro 1 | Rack | `Rack` |
| Filtro 2 | Nivel | `Nivel` |
| Fila 1 | Descripcion | `Descripcion` |
| Fila 2 | Longitud | `Longitud (in)` |
| Valor | SUM(Cantidad total) | `Cantidad total`, «Suma de Cantidad total» |

**Definicion minima.** La cache y los campos no listan valores; Excel los reconstruye al abrir (`refreshOnLoad`, `saveData="0"`). Es la forma que evita los archivos no abribles de evidencia P-09.

**Visible al abrir.** La hoja incluye las celdas ya calculadas, para que el resumen se vea en Vista protegida. Al habilitar la edicion Excel las sustituye por su resultado. El orden previo es determinista e independiente de la cultura, y puede diferir del que aplique Excel al refrescar. La agrupacion previa ignora mayusculas y respeta acentos, como Excel (P-10).

**Actualizacion.** Tras editar la tabla, Actualizar o reabrir refleja los cambios (P-05, P-06). Como toda tabla dinamica, no cambia sola sin una de esas acciones.

## 4. Casos

- **Longitud:** numerica, en pulgadas, sin unidad en el valor y sin conversion. Sin longitud significativa = `0`, como hoy. La misma Descripcion con distinta Longitud queda desglosada.
- **Rack:** nombre logico mostrado hoy, o `(sin nombre)`. Nombres iguales comparten filtro; no se sustituye por GUID.
- **Sin nivel:** hoy no existen. Si una fila llegara con Nivel vacio se conserva vacia, se cuenta y no recibe valor inventado (P-08).
- **Compartidos:** una fila por partida del BOM, sin duplicar para ningun filtro.
- **BOM vacio:** la tabla lleva una fila en blanco, porque Excel no abre una tabla con solo encabezado (P-07), y la PivotTable sigue presente. Es la representacion que Excel mismo usa; no se omite porque Excel si la admite.

## 5. Escritor

Separacion: datos BOM → modelo de exportacion → escritor → tabla y dinamica. `XlsxWriter.Build` conserva su firma y salida para quien no pida Table. Identificadores internos fijos y entradas ZIP con fecha constante, para bytes identicos ante la misma entrada. Nombres de metodos y tipos no se congelan.

## 6. Fallos

Generar no requiere Excel ni automatizacion. Un error sigue el camino actual: `MessageBox` y ningun archivo escrito. No se introduce fallback silencioso.

## 7. Compatibilidad y no-objetivos

Sin cambios: CSV, exportacion de un rack, `Total del dibujo`, constructores BOM, `ConsolidatedBomBuilder`, persistencia, comandos. La posicion de las columnas actuales se conserva.

No-objetivos: ID23, costos, precios, proveedores, formulas BOM, Expression Engine, RAM Elements, reglas o partidas nuevas, consolidacion fisica nueva, nivel fisico en el BOM, Excel COM en produccion, paquetes en producto.

## 8. Punto de extension

El escritor gana una capacidad reutilizable y aditiva: declarar una tabla en una hoja y una tabla dinamica sobre ella. Un solo consumidor en esta iniciativa.

```text
Consumes: BillOfMaterials, ConsolidatedBom, ConsolidatedBomBuilder, XlsxWriter
Extends: XlsxWriter (Table y PivotTable)
Introduces: ninguna fundacion nueva
```

## 9. Invariantes y pruebas

RED esperado en todas: falla por asercion, porque la capacidad no existe, salvo las marcadas CARACT., que fijan lo actual y pasan antes y despues.

| ID | Invariante | AC | Prueba |
|---|---|---|---|
| I-01 | Cantidad total = Cantidad × Copias una vez (4 × 3 = 12) | AC-01 | unitaria |
| I-02 | Multiplicador = Copias mostrada; < 1 se trata como 1 | AC-16 | unitaria |
| I-03 | Pieza: receta × componente × copias (18 en §2) | AC-16 | unitaria |
| I-04 | Σ Cantidad total de Pieza en «Por rack» = Σ Cantidad de Pieza en «Total del dibujo», por categoria, perfil y longitud | SC-2 | unitaria con varios racks y copias |
| I-05 | «Total del dibujo» conserva columnas y valores | SC-2 | CARACT. |
| I-06 | Copias, Longitud, Cantidad, Cantidad total son celdas numericas | AC-08 | estructura |
| I-07 | Table `RackCadBOM`: rango exacto, encabezados iguales a la fila 1, autofiltro, filas alternadas, solo detalle | AC-12 | estructura |
| I-08 | PivotTable: fuente = nombre de tabla sin rango; filtros, filas y valor del §3; agregacion suma | AC-13, AC-14 | estructura |
| I-09 | `refreshOnLoad` activo y celdas previas = Σ del detalle | AC-07 | estructura |
| I-10 | Igual Descripcion y Longitud suman; distinta Longitud separa; orden numerico | AC-02, AC-03, AC-08 | unitaria |
| I-11 | La definicion no lista valores: enteros, variantes de mayusculas y Rack variantes no alteran el XML de cache | evidencia P-09 | estructura |
| I-12 | BOM vacio: nunca tabla solo con encabezado; dinamica presente; sin excepcion | AC-17 | estructura |
| I-13 | Nivel o Descripcion vacios se conservan y se cuentan | AC-09 | unitaria |
| I-14 | Filas de detalle = componentes + piezas; sin duplicar | AC-10 | unitaria |
| I-15 | Misma entrada, bytes identicos; nombres estables | Determinismo | unitaria |
| I-16 | Domain sin conceptos Excel; producto sin `PackageReference` nuevas | ADR-0012 | guarda |
| I-17 | CSV y exportacion de un rack sin cambios | AC-18 | CARACT. |
| I-18 | Paquete valido: XML bien formado, toda relacion resuelta, content-types completos, validador independiente sin errores | AC-11 | estructura |
| I-19 | Cada sistema de §10 cumple I-01..I-14 con su constructor real | AC-18 | integracion pura |

## 10. Regresion por sistema

Con BOM real de su constructor, por `ConsolidatedBomBuilder` y la exportacion.

| Sistema | Forma | Caso que debe verificar |
|---|---|---|
| Selectivo | arbol | componente y pieza iguales; copias > 1 |
| Dinamico | arbol | Cama sin piezas; dos piezas identicas en un componente |
| Push Back | arbol | Cama sin piezas; botas con longitud 0 |
| Cantilever | arbol | recetas distintas con iguales columnas visibles |
| Cabecera | plana | solo filas Pieza; placa con longitud 0 |
| Cama | plana | solo filas Pieza; tope, rodillo y freno con longitud 0 |
| Larguero | arbol | fuera de RACKBOMTOTAL; verifica I-17 |

## 11. Validacion

A: estructura sobre el ZIP (I-06..I-12, I-18). B: validador `OpenXmlValidator` en `tests/` (D-4). C: humo en Excel real con script versionado, instancia limpia por archivo: abrir sin reparacion, Table y PivotTable nativas, filtros Rack, Nivel y combinados, SUM, refresh tras modificar, Vista protegida, BOM vacio. No entra a CI ni a las suites ordinarias.

## 12. Owner Validation

Con el DLL de `FINAL_CANDIDATE_SHA`, un dibujo con varios racks, niveles, copias > 1, descripciones repetidas, longitudes distintas y varios sistemas; RACKBOMTOTAL → Exportar Excel.

| OV-id | Escenario | Sistemas | Datos | Resultado esperado | Unidad | Hito intermedio |
|---|---|---|---|---|---|---|
| OV-1 | Cantidad total | Todos | Nuevo | = Cantidad × Copias, numerica | I-68 | no |
| OV-2 | Filtros de la Table | Todos | Nuevo | Tabla nativa con filtros | I-68 | no |
| OV-3 | Campos de la dinamica | Todos | Nuevo | Rack, Nivel, Descripcion → Longitud, suma | I-68 | no |
| OV-4 | Filtro Rack | Varios | Nuevo | Solo ese rack | I-68 | no |
| OV-5 | Filtro Nivel | Todos | Nuevo | Componente o Pieza | I-68 | no |
| OV-6 | Filtro combinado | Varios | Nuevo | Interseccion correcta | I-68 | no |
| OV-7 | Totales SUM | Todos | Nuevo | Igual a sumar el detalle filtrado | I-68 | no |
| OV-8 | Actualizar tras cambiar el detalle | Uno | Nuevo | El resumen cambia | I-68 | no |
| OV-9 | Sin aviso de reparacion | Todos | Nuevo | Abre limpio con doble clic | I-68 | no |
| OV-10 | Vista protegida (archivo descargado) | Uno | Nuevo | Resumen visible; al habilitar sigue correcto | I-68 | no |
| OV-11 | BOM vacio, si se puede provocar | Uno | Nuevo | Abre sin reparacion | I-68 | no |

## 13. Gates

| Gate | Resultado verificable |
|---|---|
| G0 | Discovery, Freeze aceptado y D-0..D-4 resueltas |
| G1 (F1+F2) | Cantidad total, Table y PivotTable en el XLSX consolidado; I-01..I-16 y I-18 |
| G2 (F3) | Matriz por sistema, multi-rack, vacio; I-17, I-19; rendimiento caracterizado |
| G3 (F4) | Humo en Excel real sobre el SHA y conformidad |
| READY | READY-01..09, Candidato, OV |

## 14. Enrutamiento

Coordinator: Discovery, Freeze, gates. Caracterizacion BOM: perfil Balanced, ya ejecutada. G1 PivotTable: Deep; pruebas: Balanced. Humo: local, sin Computer Use salvo necesidad. Conformidad: revision independiente segun LIFECYCLE §9. Ningun ejecutor declara GATE PASS.
