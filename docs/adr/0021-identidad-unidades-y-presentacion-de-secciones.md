# ADR-0021: Identidad, unidades y presentación de secciones estructurales

- **Estado:** **propuesto**
- **Fecha:** 2026-07-27 (redacción); pendiente de aceptación expresa del dueño
- **Decisores:** Mario Pérez, dueño del repositorio (**pendiente**); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-36A `architecture/catalogo-secciones-estructurales`

> **Por qué sigue `propuesto`.** La política exacta de IDs es uno de los siete puntos del gate
> `owner-validation` de I-36A y todavía no está aceptada. En la primera ronda el dueño la rechazó
> parcialmente, precisamente porque el ejemplo del encargo para HSS no coincide con la designación EDI
> real (§6). Un ADR `aceptado` cuya decisión central sigue bajo gate diría lo contrario de lo que pasa.
> ADR-0020 sí permanece `aceptado`: la separación sección / miembro fue decidida expresamente y no está
> en discusión.

## Contexto

[ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) crea el catálogo neutral de secciones
estructurales. Ese catálogo importa una fuente externa —**AISC Shapes Database v16.0**— que trae tres
fuerzas nuevas al repositorio y las tres tienen que quedar decididas antes de escribir la primera
fila:

1. **Identidad.** La fuente publica **dos** designaciones por sección: la del *AISC Naming Convention
   for Structural Steel Products for Use in Electronic Data Interchange* (**EDI**, columna B) y la
   etiqueta del manual (**AISC_Manual_Label**, columna C). Para las cuatro familias importadas ambas
   coinciden en 470 de 983 filas y difieren en 513 —todas HSS rectangulares/cuadradas, donde el EDI
   escribe el espesor en decimal (`HSS34X10X.875`) y el manual en fracción (`HSS34X10X7/8`)—. Ninguna
   de las dos es un identificador apto tal cual: llevan `.`, `/` y `-`. Además la fuente tiene
   revisión (v16.0 reemplaza a v15.0), y un identificador que incluya la revisión rompería todo
   diseño guardado en cuanto llegue v17.

2. **Unidades.** [ADR-0005](0005-estrategia-de-unidades.md) fija la **pulgada** como unidad interna
   canónica y prohíbe explícitamente conversión o reinterpretación del DWG. La fuente AISC, en
   cambio, publica **cada** magnitud dos veces. La hoja se reparte en cuatro tramos: `A`–`D` son
   metadata e identidad del bloque estadounidense (`Type`, designación EDI, etiqueta del manual y el
   indicador `T_F`), `E`–`CF` son los **valores estadounidenses**, `CG`–`CH` son las **designaciones
   métricas** (EDI y etiqueta, `W44X408` ↔ `W1100X607`) y `CI`–`FJ` son los **valores métricos**.
   Importar los dos bloques de valores como filas produciría secciones duplicadas; importar solo el
   métrico contradiría la pulgada interna.

3. **Presentación.** El peso comercial de un perfil se nombra en su unidad nativa: un `W12X26`
   «pesa 26 lb/ft», y ese 26 es parte de la designación. Mostrarlo convertido y solo convertido
   («41.7 kg/m») borra la designación que el usuario reconoce; mostrarlo sin equivalencia deja fuera
   al lector métrico.

Hay además dos datos de la fuente que **parecen** el mismo y no lo son: el HSS publica `tnom`
(espesor nominal de pared) y `tdes` (espesor de diseño, ≈0.93·`tnom` para HSS conformado en frío).
Colapsarlos perdería información que ninguna de las dos columnas puede reconstruir.

Esta decisión restringe trabajo futuro en más de un módulo (identidad persistida, unidades,
presentación) y es cara de revertir (los IDs viajan a los documentos guardados), así que se registra
antes de implementar, conforme a los criterios 1 y 2 de [`adr/README.md`](README.md).

## Decisión

### Identidad

1. **El identificador de una sección es `{ID_NAMESPACE}-{FAMILIA}-{DESIGNACION_NORMALIZADA}`**, con
   `FAMILIA` ∈ {`W`, `HSS-RECT`, `C`, `L`} y la designación derivada de la forma **EDI oficial** por
   una normalización determinista: mayúsculas invariantes, se eliminan los espacios y se sustituyen
   `.`, `/` y `-` por `_`. El resultado es ASCII, en mayúsculas, sin espacios, sin barra y sin punto
   decimal. Ejemplos **reales** del catálogo distribuido: `AISC-W-W12X26`, `AISC-C-C10X15_3`,
   `AISC-L-L4X4X1_4`, `AISC-HSS-RECT-HSS4X4X_250`.

1.b **`ID_NAMESPACE` es la AUTORIDAD que nombra la sección, y es un dato, no una constante.** Lo
   declara la fuente en `StructuralSectionSource.IdNamespace`; `AISC-SHAPES` declara `AISC`, de modo
   que los 983 identificadores existentes son exactamente los mismos. Es distinto de `SourceId`
   porque responden a preguntas distintas: el `SourceId` dice de QUÉ documento salió una fila y puede
   llevar revisión o nombre de producto, mientras que el namespace dice QUIÉN tiene autoridad para
   nombrarla — un token corto y estable que va incrustado en cada diseño guardado y que por eso no
   puede cambiar nunca. Debe ser no vacío y de `A-Z0-9` solamente —el guion queda excluido porque es
   el separador del propio id— y **único** en el catálogo: dos autoridades que compartieran namespace
   permitirían que la misma designación resolviera a un solo id desde dos publicadores distintos, que
   es justo la colisión que este segmento existe para impedir. Un namespace vacío, con forma inválida
   o duplicado es un error.

1.c **La unicidad de la designación EDI es POR FUENTE, no global.** Dos publicadores pueden nombrar
   legítimamente el mismo perfil; el id los distingue por su autoridad. `TryGetByEdiDesignation` sin
   fuente resuelve mientras no haya ambigüedad y **se niega a elegir** cuando la hay.

1.d **El ID se reconstruye siempre a través de la autoridad que declara la fuente.**
   `ExpectedSectionId` **exige** el namespace como argumento en vez de asumir uno por omisión, y el
   validador resuelve la fuente para pasárselo: una sección cuya fuente no exista no puede
   comprobarse, y decirlo es el resultado honesto.

2. **El ID no contiene la revisión de la fuente.** `SourceRevision` (`16.0`) se almacena en su propio
   campo, en cada fila y en el manifiesto. Una sección que sobreviva a una revisión futura conserva
   su ID.

3. **El ID no contiene el grado de material, el peso convertido ni ninguna magnitud.** `DisplayName`
   nunca se usa como clave. Las dimensiones y el peso **no** se derivan interpretando el texto de la
   designación: se leen de sus columnas.

4. **Una colisión de normalización es un error fatal** del importador y del validador, no una
   desambiguación automática. Sobre la v16.0 no existe ninguna: las 983 designaciones EDI de las
   cuatro familias producen 983 IDs distintos.

5. **La designación EDI y el AISC Manual Label se conservan por separado y sin alterar.** El EDI es la
   clave de búsqueda normalizada; el Manual Label es la forma que se muestra al usuario, con su forma
   original (fracciones incluidas). Ambos son únicos dentro de las cuatro familias importadas.

6. **Nota de fidelidad, deliberada y verificable.** Para HSS, el ID sale del **EDI** (`.250`), no del
   Manual Label (`1/4`): `HSS4X4X1/4` tiene ID `AISC-HSS-RECT-HSS4X4X_250`. La regla «el ID se basa en
   la designación EDI» se aplica a las **cuatro** familias sin excepción, porque una regla que cambie
   de fuente según la familia deja de ser determinista. El usuario sigue viendo y buscando
   `HSS4X4X1/4` mediante el Manual Label.

### Unidades

7. **La geometría resuelta de RackCad sigue en pulgadas.** Este ADR **no** reemplaza a ADR-0005, no
   altera `INSUNITS`, no implementa conversión del DWG y no autoriza reescalado.

8. **Para AISC v16.0, el bloque de valores estadounidenses es el valor nativo y canónico.**
   `NativeUnitSystem = US_CUSTOMARY`. Las longitudes se conservan en pulgadas y el peso lineal en
   `lb/ft`, tal como los publica la fuente.

9. **Las columnas métricas oficiales se usan como contraste, no como filas.** No se genera una
   segunda sección por los valores métricos ni se sustituye un valor nativo por su equivalente
   métrico redondeado. Se emplean en pruebas de coherencia con tolerancia compatible con el redondeo
   de AISC: **1 %** para geometría y área (desviación máxima medida sobre las 983 filas: 0.461 %) y
   **5 %** para el peso nominal, porque el peso métrico de la fuente es un **valor nominal de
   designación redondeado por separado**, no una conversión (desviación máxima medida: 4.128 %, en
   `C5X6.7` ↔ `10.4 kg/m`).

10. **La equivalencia se calcula, no se tabula.** Una única función pura convierte `lb/ft` a `kg/m`
    con el factor exacto `0.45359237 / 0.3048`. Las conversiones de longitud y área usan `25.4` y
    `645.16` exactos.

11. **`tnom` y `tdes` son datos distintos y ambos se conservan** para HSS. La geometría futura
    (I-36B) usará el **espesor nominal**; el de diseño queda disponible para cálculo. Ninguno se
    deriva del otro.

12. **`MaterialGrade` es opcional y no se infiere.** No forma parte del ID, no se deduce del catálogo
    AISC —la Shapes Database no publica grado— y su ausencia es el estado normal.

### Presentación

13. **El peso se muestra primero en la unidad nativa de la fuente y después en la equivalente**, con
    la designación del manual al frente:

    - fuente imperial: `W12X26 — 26 lb/ft (38.7 kg/m)`
    - una fuente métrica futura: `Designación — 41.7 kg/m (28 lb/ft)`

    El número nativo se imprime tal como está tabulado (`26`, no `26.0`); el equivalente se redondea a
    un decimal. El valor sale **siempre** de `WeightPerLength`, nunca de leer el `26` dentro de
    `W12X26`.

    El encargo que abrió la iniciativa ilustraba este formato con `W12X28 — 28 lb/ft (41.7 kg/m)`.
    **`W12X28` NO es una sección real de la v16.0**: la serie salta de `W12X26` a `W12X30`. Ese
    ejemplo se conserva únicamente como **fixture SINTÉTICO** que fija el formato al carácter, y así
    está etiquetado en la prueba que lo reproduce; los ejemplos reales usan `W12X26`.

14. **El formateador es puro**: vive en `RackCad.Application`, no depende de WPF ni de AutoCAD, usa
    `CultureInfo.InvariantCulture` y está cubierto por pruebas.

## Alternativas consideradas

- **Incluir la revisión en el ID (`AISC-V16-W-W12X26`)** — haría autoexplicativo el origen de cada
  fila y rompería todos los diseños guardados en cuanto llegue la v17, obligando a una migración por
  revisión. Rechazada: la revisión es un atributo, no identidad.

- **Usar el AISC Manual Label como base del ID** — produce `AISC-HSS-RECT-HSS4X4X1_4`, más familiar
  para el usuario. Rechazada: obligaría a elegir fuente por familia (EDI para W/C/L, Label para HSS,
  ya que en las otras tres coinciden), y el propósito del EDI —según su propio nombre y según el
  Readme de la fuente— es exactamente ser la forma estable para intercambio electrónico de datos. El
  Label se conserva íntegro para presentación y búsqueda.

- **Usar el Manual Label sin normalizar como clave** — deja `/`, `.` y `-` dentro de un identificador
  que viajará a rutas, JSON, nombres y comparaciones. Rechazada.

- **Importar el bloque métrico como filas propias** — duplicaría 983 secciones por 1 966 y crearía dos
  identidades para la misma pieza física. Rechazada: es contraste, no catálogo.

- **Sustituir los valores nativos por los métricos y convertir a pulgadas al leer** — introduce un
  doble redondeo (AISC redondea al pasar a métrico; nosotros redondearíamos al volver) sobre datos que
  la fuente ya publica en pulgadas. Rechazada.

- **Convertir el peso a kg/m y mostrar solo eso** — borra la designación comercial que el usuario
  reconoce. Rechazada; se muestran ambos, nativo primero.

- **Colapsar `tnom` y `tdes` en un solo espesor** — pierde información no reconstruible y obligaría a
  elegir en I-36A una decisión que corresponde a I-36B (geometría) y al cálculo. Rechazada.

## Consecuencias

- **Positivas**: los IDs sobreviven a las revisiones de la fuente; la normalización es una función
  pura, probada y sin colisiones sobre el cuerpo real de datos; la pulgada interna y la política del
  DWG quedan intactas, así que I-36A no puede alterar un dibujo existente; el contraste métrico
  convierte la duplicidad de la fuente en una **prueba** en vez de en deuda; el usuario ve el peso en
  la unidad en la que compra el perfil y, a la vez, su equivalente.

- **Negativas / costos aceptados**: el ID de un HSS no se parece a su etiqueta de manual
  (`AISC-HSS-RECT-HSS4X4X_250` para `HSS4X4X1/4`), lo que exige que toda UI futura muestre el Manual
  Label y no el ID; la tolerancia del contraste de peso es necesariamente amplia (5 %) porque compara
  contra un valor nominal redondeado, así que detecta errores de mapeo de columna, no de precisión;
  conservar `tnom` y `tdes` obliga a que cada consumidor elija explícitamente cuál usa.

- **A vigilar**: que nadie derive dimensiones o peso interpretando el texto de una designación; que
  ninguna iniciativa futura reintroduzca la revisión dentro del ID; que la existencia de columnas
  métricas no se lea como autorización para convertir el DWG (eso sigue prohibido por ADR-0005 y
  diferido a su propia iniciativa); y que `MaterialGrade` no se rellene por inferencia.

## Referencias

- [ADR-0020: Catálogo neutral de secciones estructurales](0020-catalogo-neutral-de-secciones-estructurales.md)
- [ADR-0005: Estrategia de unidades](0005-estrategia-de-unidades.md) (**no** reemplazado; ver su nota posterior)
- Contrato: [`docs/initiatives/I-36A-catalogo-secciones-estructurales.md`](../initiatives/I-36A-catalogo-secciones-estructurales.md)
- Decisión versionada del dueño: [`docs/automation/decisions/I-36A.md`](../automation/decisions/I-36A.md)
- Evidencia reproducible: [`docs/automation/evidence/I-36A-catalogo-secciones-estructurales.md`](../automation/evidence/I-36A-catalogo-secciones-estructurales.md)
- Guía: [`docs/guias/secciones-estructurales.md`](../guias/secciones-estructurales.md)
- Fuente: AISC Shapes Database v16.0, hoja `Readme` (glosario de variables y convención EDI) y hoja
  `Database v16.0`
