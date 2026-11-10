# I-68 — Especificación funcional del Owner (ID72, texto literal)

Orden del Owner del 2026-11-09, pegada en la sesión y conservada aquí sin cambios. Es la autoridad funcional de la
iniciativa. El texto llegó cortado en su última palabra («PivotTabl»); el corte no afecta a ningún requisito.
`<NEXT_FREE_INITIATIVE_ID>` se resolvió como I-68.

```text
Coordina exclusivamente una nueva iniciativa de RackCad:

# <NEXT_FREE_INITIATIVE_ID> — BOM Excel Native Analysis
## ID72 — BOM Excel: Cantidad total, tabla y tabla dinámica

Workflow:
V2

Initial Archetype:
EXTENSION

Priority:
HIGH / IMMEDIATE IMPROVEMENT

Archetype escalation rule:
Si Discovery demuestra que la biblioteca Excel actual no soporta de forma fiable tablas dinámicas nativas y se requiere introducir una nueva foundation, cambiar de biblioteca, manipular OOXML directamente o crear infraestructura reutilizable sustancial, STOP antes del Freeze y evalúa reclasificar a FOUNDATION EVOLUTION.

No conviertas una limitación de biblioteca en un parche frágil sin revisión arquitectónica.

======================================================================
GOAL
======================================================================

Mejorar el exportador BOM Excel de RackCad para que el archivo exportado pueda
analizarse inmediatamente en Excel sin que el usuario tenga que:

- añadir columnas manualmente;
- crear fórmulas;
- convertir rangos a tabla;
- construir una tabla dinámica;
- crear campos calculados.

La exportación debe entregar:

1. detalle BOM existente;
2. nueva columna numérica "Cantidad total";
3. detalle convertido en tabla nativa de Excel;
4. nueva hoja con tabla dinámica nativa real.

======================================================================
AUTHORITIES
======================================================================

Antes de modificar producción, lee y sigue:

- docs/WORKFLOW.md
- AGENTS.md
- docs/INITIATIVE_LIFECYCLE.md
- docs/FOUNDATIONS.md
- docs/initiatives/PROMPT_TEMPLATES.md
- protocolo I-61 vigente si aplica
- implementación actual del exportador BOM
- contratos actuales de BOM
- tests actuales de exportación Excel
- biblioteca y versión exactas utilizadas para XLSX

Use:

REFERENCE OVER REPETITION.

No asumir semántica de campos desde nombres únicamente.

======================================================================
FIRST REQUIRED DISCOVERY
======================================================================

Antes de implementar:

1. localizar exportador BOM actual;
2. identificar biblioteca Excel y versión exacta;
3. identificar formato XLSX generado;
4. confirmar soporte REAL para:
   - Excel Table;
   - PivotTable nativa;
   - Pivot cache;
   - filtros;
   - refresh;
   - numeric data fields;
5. confirmar si el soporte funciona en la versión actualmente referenciada;
6. identificar limitaciones/licenciamiento relevantes.

No sustituir PivotTable por:

- resumen estático;
- fórmulas SUMIFS;
- hoja agregada manualmente;
- pseudo-pivot visual.

Si la biblioteca no soporta una PivotTable nativa válida:
STOP y presenta alternativas antes de cambiar arquitectura o dependencia.

======================================================================
CURRENT BOM SEMANTICS
======================================================================

Discovery debe identificar exactamente el significado actual de:

Cantidad
Copias

y dónde se calculan.

Regla requerida:

Cantidad total = Cantidad × Copias

pero el multiplicador debe aplicarse EXACTAMENTE UNA VEZ.

Investiga especialmente si:

- Cantidad ya incorpora copias en algún sistema;
- Copias representa cantidad física, cantidad de racks, cantidad de vistas u otro concepto;
- algún consumidor/exportador ya multiplica ambos;
- BOM consolidado y BOM por rack tienen semánticas distintas;
- existen filas compartidas/no asociadas a un nivel.

No corregir síntomas creando doble multiplicación.

Freeze debe documentar claramente:

Cantidad
Copias
Cantidad total

con ejemplos.

======================================================================
QUANTITY TOTAL
======================================================================

Agregar una columna:

Cantidad total

Regla:

Cantidad total = Cantidad × Copias

Ejemplo obligatorio:

Cantidad = 4
Copias = 3
Cantidad total = 12

Requirements:

- conservar las columnas Cantidad y Copias;
- no sustituirlas;
- Cantidad total debe exportarse como valor numérico;
- no como string;
- no como número con unidad incrustada;
- debe poder utilizarse directamente en SUM, filtros y PivotTable;
- precisión/decimal behavior debe seguir la semántica BOM actual.

Preferir calcular el valor en RackCad/export model antes que depender de una
fórmula Excel, salvo que la autoridad vigente justifique lo contrario.

No crear doble autoridad para la cantidad.

======================================================================
EXCEL DETAIL TABLE
======================================================================

Convertir el detalle BOM en una TABLE nativa de Excel.

Requirements:

- nombre estable y válido;
- encabezados;
- filtros automáticos;
- estilo con filas alternadas;
- todas las partidas BOM de detalle;
- nueva columna Cantidad total;
- conservar los campos actuales;
- rango exacto de datos;
- tipos numéricos preservados.

No incluir dentro de la tabla de detalle:

- subtotales auxiliares;
- total general;
- filas decorativas;
- encabezados secundarios;
- filas vacías artificiales.

La tabla debe constituir la fuente de datos de la PivotTable.

Freeze debe definir el nombre estable.

Ejemplo conceptual:

RackCadBOM

pero el nombre final debe respetar convenciones actuales del repo/Excel.

======================================================================
PIVOT TABLE
======================================================================

Agregar una nueva hoja con una PIVOTTABLE NATIVA REAL.

Source:

native Excel BOM detail table

Required layout:

FILTERS:
1. Rack
2. Nivel

ROWS:
1. Descripción
2. Longitud

VALUES:
Sum of Cantidad total

Debe ser:

SUM
NO COUNT.

La PivotTable debe estar presente y funcional al abrir el archivo en Excel.

Debe poder actualizarse desde la tabla origen.

No aceptar un resumen estático equivalente como sustituto.

======================================================================
PIVOT FIELD SEMANTICS
======================================================================

Verifica nombres reales de las columnas actuales.

Si actualmente los encabezados son exactamente:

Rack
Nivel
Descripción
Longitud

úsalos.

Si difieren:
documenta mapping y preserva los encabezados productivos salvo decisión de
Freeze.

No renombres columnas existentes únicamente para facilitar la API de PivotTable
sin justificar compatibilidad.

======================================================================
LONGITUD
======================================================================

Longitud debe mantenerse como valor numérico utilizable por PivotTable.

Requirements:

- unidad consistente dentro de la exportación;
- no incrustar la unidad en el valor numérico;
- distinguir longitudes diferentes;
- partidas con misma Descripción pero distinta Longitud deben quedar
  desglosadas por la PivotTable.

Ejemplo:

Descripción A / 40
Descripción A / 48

→ dos agrupaciones de fila distintas.

Discovery debe determinar:

- unidad actual;
- tipo exportado;
- formato de celda;
- cómo se representan piezas sin longitud.

No introducir conversión de unidad nueva salvo necesidad acreditada.

======================================================================
RACK
======================================================================

El filtro Rack debe conservar la identidad/señalización actual del BOM.

Discovery debe determinar si actualmente representa:

- RackId;
- nombre lógico;
- tipo de sistema;
- etiqueta de BOM;
- otra autoridad.

No sustituir silenciosamente identidad estable por display text o viceversa.

El filtro debe permitir analizar uno o varios racks según comportamiento
normal de Excel.

======================================================================
NIVEL
======================================================================

Partidas sin nivel deben:

- permanecer en el detalle;
- permanecer contabilizadas;
- ser identificables en la PivotTable;
- NO recibir un nivel inventado;
- NO duplicarse para asociarlas artificialmente a niveles.

Discovery debe caracterizar cómo se representa hoy "sin nivel":

- null;
- vacío;
- token específico;
- otra semántica.

Freeze debe decidir una representación Excel consistente.

Preferir preservar la semántica real.

No inventar "Nivel 0" si no existe como concepto de producto.

======================================================================
SHARED / NON-LEVEL ITEMS
======================================================================

Revisar explícitamente componentes:

- compartidos por rack;
- globales;
- sin nivel;
- repetidos entre niveles;
- generados por distintos sistemas.

Cantidad total debe reflejar su cantidad física real.

No duplicar una partida únicamente para que aparezca bajo varios filtros de
Nivel.

La PivotTable debe sumar exactamente lo que existe en el detalle.

======================================================================
PIVOT REFRESH
======================================================================

Acceptance requires demostrar que la PivotTable puede actualizarse después de
modificar el detalle.

Investiga qué soporta la biblioteca actual:

- RefreshDataOnOpen;
- RefreshOnLoad;
- RefreshData;
- pivot cache refresh;
- source table expansion.

Required behavior:

la PivotTable debe estar vinculada a la TABLE,
no a un rango fijo que deje fuera filas nuevas.

Validación mínima:

1. generar XLSX;
2. abrir/modificar detalle de forma controlada;
3. actualizar PivotTable;
4. comprobar resultado actualizado.

No afirmar refresh capability solo porque el archivo contiene XML de PivotTable.

======================================================================
EMPTY BOM
======================================================================

Cubrir explícitamente exportación con BOM vacío.

Definir comportamiento válido.

Possible examples to evaluate:

- detail sheet with headers + empty native table;
- summary sheet present with safe empty-state;
- PivotTable omitted with explicit reason if Excel/library cannot create a
  valid pivot with zero rows.

Do NOT decidirlo por conveniencia de implementación.

Freeze debe seleccionar un comportamiento que produzca un XLSX válido y que
abra sin reparación.

Un BOM vacío no debe causar excepción ni archivo corrupto.

======================================================================
NATIVE XLSX VALIDITY
======================================================================

Acceptance requires:

- archivo abre en Microsoft Excel;
- no aparece reparación de contenido;
- no aparece "removed records";
- Table es reconocida como Table nativa;
- PivotTable es reconocida como PivotTable nativa;
- filtros funcionan;
- valores son SUM y no COUNT;
- source relationship es válido;
- workbook relationships son válidas.

No validar únicamente leyendo el XLSX con la misma biblioteca que lo escribió.

Preferir, donde sea posible:

A. structural/OpenXML validation;
B. independent reader;
C. real Excel smoke.

======================================================================
MULTI-RACK / MULTI-LEVEL COVERAGE
======================================================================

La matriz debe incluir:

- un rack;
- varios racks;
- un nivel;
- varios niveles;
- varios valores de Copias;
- misma descripción/misma longitud;
- misma descripción/distinta longitud;
- elementos sin nivel;
- elementos compartidos;
- BOM vacío.

Verificar filtros:

Rack
Nivel
Rack + Nivel combinados.

======================================================================
SYSTEM COMPATIBILITY
======================================================================

El exportador puede ser consumido por varios sistemas.

Discovery debe identificar todos los productores/consumidores actuales.

Como mínimo construir una matriz real de cobertura.

No asumir que una prueba de Selectivo acredita:

- Dinámico;
- Push Back;
- Cantilever;
- Cabecera;
- Cama;
- otros sistemas productivos.

La mejora de Excel no debe alterar la semántica BOM propia de cada sistema.

======================================================================
NO BOM DOMAIN EXPANSION
======================================================================

Esta iniciativa NO implementa:

- ID23 Calculated BOM Items;
- costos;
- precios;
- proveedores;
- fórmulas BOM;
- Expression Engine integration;
- RAM Elements;
- nuevas reglas de BOM;
- nueva consolidación física;
- nuevas partidas.

Solo mejora la representación/exportación de los datos BOM ya producidos,
salvo Cantidad total que es una derivación explícita:

Cantidad × Copias.

======================================================================
LIBRARY DECISION
======================================================================

Esta iniciativa debe comenzar intentando usar la biblioteca/version actual.

Si soporta correctamente:

Excel Table
+
native PivotTable

→ usarla.

Si soporta Table pero NO native PivotTable:

STOP antes de:

- actualizar dependencia mayor;
- cambiar de biblioteca;
- introducir Microsoft Office Interop;
- editar OOXML manualmente.

Presenta opciones con impacto:

A. actualizar biblioteca actual;
B. usar otra biblioteca;
C. OpenXML SDK / OOXML;
D. otra opción soportada.

Evalúa:

- licencia;
- runtime requirements;
- distribución;
- AutoCAD compatibility;
- .NET compatibility;
- determinismo;
- tests;
- mantenimiento.

No introducir Excel COM/Office Interop como dependencia productiva sin
arquitectura explícita.

La generación del XLSX idealmente debe funcionar sin Excel instalado.

======================================================================
TABLE/PIVOT OWNERSHIP
======================================================================

Preferir una separación conceptual:

BOM data
      ↓
Excel export model
      ↓
Workbook writer
      ├── Detail Table
      └── PivotTable

No introducir conceptos de PivotTable dentro del dominio BOM.

Los objetos específicos de la biblioteca Excel deben permanecer en la capa
de exportación/adaptador.

======================================================================
DETERMINISM
======================================================================

En la medida posible, la salida debe ser determinista respecto al BOM de entrada.

Evitar:

- nombres de tabla aleatorios;
- nombres de hojas dependientes de timestamps;
- GUIDs innecesarios en contratos visibles;
- orden arbitrario de columnas.

Los IDs internos requeridos por XLSX pueden ser generados por la biblioteca,
pero no deben alterar semántica observable.

======================================================================
TEST STRATEGY
======================================================================

Require:

A. Pure/unit tests
- Cantidad total;
- apply multiplier once;
- types;
- empty/non-level semantics.

B. Workbook structure tests
- detail Table exists;
- expected columns;
- numeric cell types;
- stable table name;
- Pivot sheet exists;
- source is detail Table;
- field layout correct;
- value aggregation = SUM.

C. Independent XLSX validation
- workbook opens structurally;
- no invalid relationships;
- no corrupt XML.

D. Real Excel smoke if environment available
- open without repair;
- filters;
- PivotTable visible;
- Rack filter;
- Nivel filter;
- combined filters;
- refresh after detail modification.

Do not require Excel-installed UI tests in ordinary Core test suites.

======================================================================
ACCEPTANCE CASES
======================================================================

AC-01:
Cantidad = 4
Copias = 3
Cantidad total = 12

AC-02:
same Descripción
same Longitud
→ Pivot sums Cantidad total

AC-03:
same Descripción
different Longitud
→ separate row groups

AC-04:
Rack filter works

AC-05:
Nivel filter works

AC-06:
Rack + Nivel filters work together

AC-07:
Pivot totals equal corresponding detail SUM

AC-08:
numeric Longitud remains grouped numerically

AC-09:
items without Nivel remain identifiable and counted

AC-10:
shared items are not duplicated

AC-11:
workbook opens in Excel without repair

AC-12:
detail range is native Excel Table

AC-13:
summary is native Excel PivotTable

AC-14:
Pivot value field is SUM(Cantidad total), not COUNT

AC-15:
modifying detail + refresh updates summary

AC-16:
multiple racks / levels / copies

AC-17:
empty BOM

AC-18:
all existing BOM-producing systems retain compatible export behavior

======================================================================
PERFORMANCE
======================================================================

This is not expected to be a performance-heavy initiative.

Still characterize:

- typical BOM;
- larger BOM;
- workbook generation;
- PivotTable generation.

Do not introduce Excel automation requiring Excel process startup during normal
export unless explicitly authorized.

No arbitrary performance thresholds before baseline.

======================================================================
I-61 EXECUTION
======================================================================

Use the currently integrated execution protocol.

Coordinator owns:

- Discovery;
- Freeze;
- Gate PASS.

Controller may:

- classify tasks;
- choose executor/model/effort;
- delegate;
- verify exact-SHA.

Recommended task routing:

BOM/current exporter characterization
→ Balanced

Excel library/API investigation
→ Balanced/Deep depending documentation complexity

Cantidad total + Table
→ Efficient/Balanced

native PivotTable integration
→ Balanced/Deep

XLSX structural validation
→ Balanced

Excel smoke / GUI validation
→ host/Computer Use only where necessary

Final conformance
→ independent review according to current protocol.

Do not let Worker declare Gate PASS.

======================================================================
ARCHITECT
======================================================================

Initial archetype:
EXTENSION.

Architect reviews Freeze according to Workflow V2.

Material triggers requiring STOP / possible Foundation Evolution:

- current library cannot generate native PivotTables;
- dependency replacement;
- major library upgrade;
- direct OOXML manipulation;
- new reusable Excel infrastructure;
- Microsoft Office runtime dependency;
- changes to BOM domain semantics beyond Cantidad total.

Do not hide a dependency/platform change inside an Extension.

======================================================================
FUNCTIONAL GATES
======================================================================

Keep scope compact.

Suggested:

F0 — Discovery / characterization / Freeze
     - exporter
     - Cantidad/Copias semantics
     - library/version
     - native Table/Pivot capability

F1 — Cantidad total + Native Detail Table

F2 — Native PivotTable

F3 — Multi-rack/multi-level/empty/system regression

F4 — Real XLSX / Excel validation + conformance

READY — Final Candidate

If F1 and F2 are trivial under current library, consolidate them.

If PivotTable requires architectural work:
STOP and replan before expansion.

======================================================================
OWNER VALIDATION
======================================================================

Plan focused Owner Validation because output is directly user-facing.

Use at least one representative BOM with:

- several racks;
- several levels;
- copies > 1;
- repeated descriptions;
- different lengths;
- item without level.

Owner should verify in real Excel:

1. Cantidad total;
2. filters in detail Table;
3. PivotTable fields;
4. Rack filter;
5. Nivel filter;
6. combined filter;
7. SUM totals;
8. refresh after detail change;
9. no Excel repair warning.

Also validate empty BOM behavior if practical.

======================================================================
SUCCESS CRITERIA
======================================================================

1. Cantidad and Copias semantics are confirmed.

2. Cantidad total multiplies exactly once.

3. Cantidad total is numeric.

4. detail BOM is a native Excel Table.

5. Table contains only detail records.

6. PivotTable is native Excel functionality.

7. Pivot source is the native detail Table.

8. filters are Rack and Nivel.

9. row hierarchy is Descripción → Longitud.

10. value field is SUM(Cantidad total).

11. non-level items remain correctly counted.

12. shared elements are not duplicated.

13. longitudes remain numeric and consistently dimensioned.

14. workbook opens without repair.

15. Pivot can refresh after detail changes.

16. empty BOM behavior is valid.

17. existing BOM systems remain compatible.

18. no RAM/Expression/Calculated-BOM scope enters the initiative.

19. ID72 is documented as CLOSED only after exact-SHA evidence and Owner
    validation according to current Workflow.

======================================================================
FIRST RESPONSE REQUIRED
======================================================================

Before production implementation return:

1. real repo preflight;
2. current origin/main;
3. next free Initiative-Id;
4. branch/worktree claim;
5. active hotspot conflicts;
6. Workflow V2 / execution protocol status;

7. exact BOM exporter implementation;

8. all current BOM export consumers/systems;

9. current fields/headers/order;

10. exact semantics of Cantidad;

11. exact semantics of Copias;

12. evidence whether multiplication already occurs anywhere;

13. exact Excel library;

14. exact library version;

15. license/package status relevant to redistribution;

16. native Excel Table support verdict;

17. native PivotTable support verdict;

18. refresh support verdict;

19. whether Excel installation is required;

20. current Longitud value/unit/type;

21. current Rack field semantics;

22. current Nivel field semantics;

23. current representation of no-level items;

24. behavior with shared BOM items;

25. current empty BOM behavior;

26. proposed Cantidad total authority;

27. proposed stable Table name;

28. proposed summary sheet name;

29. proposed Pivot field mapping;

30. proposed XLSX validation strategy;

31. proposed real-Excel smoke;

32. regression matrix by system;

33. whether EXTENSION remains correct archetype;

34. dependency/library-change assessment;

35. Consumes / Extends / Introduces;

36. proposed Freeze;

37. Architect package;

38. functional gates;

39. Owner Validation plan;

40. initial routing plan;

41. explicit status:

IMPLEMENTATION AUTHORIZATION = NO

until:

- Cantidad/Copias semantics are confirmed;
- native PivotTable support of the actual library/version is verified;
- Freeze is accepted.

Do not implement a static summary as a substitute for a native PivotTabl
```
