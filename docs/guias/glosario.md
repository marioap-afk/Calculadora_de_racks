# Glosario de RackCad

Vocabulario canónico para documentación, UI y código. Si un término técnico tiene un nombre de tipo
C#, el texto conserva primero la palabra usada por ingeniería de racks.

## Dominio del rack

| Término | Significado en RackCad |
|---|---|
| Cabecera / marco | Estructura lateral formada por postes, placas, horizontales y celosía. En código histórico aparece como `RackFrame` o header |
| Poste | Perfil vertical principal de una cabecera o línea de rack |
| Horizontal | Miembro transversal de una cabecera; sus elevaciones son fuente de verdad para derivar paneles |
| Panel | Espacio entre dos horizontales consecutivas; contiene el arreglo de celosía |
| Celosía | Conjunto de diagonales y horizontales que arriostra la cabecera |
| Larguero | Miembro horizontal que soporta tarimas entre postes |
| Ménsula | Conector del larguero al poste; forma parte de su receta física |
| Placa base | Pieza en la base del poste; puede derivar o sobrescribir su peralte |
| Tarima | Pallet o carga de referencia usada para derivar claros, alturas y cantidad de apoyos |
| Parrilla / deck | Superficie colocada sobre largueros; su conteo se deriva por tarima |
| Separador | Pieza física que mantiene distancia entre fondos o entre elementos definidos por catálogo |
| Bota | Protector de la base del poste |
| Protector lateral | Protección longitudinal que puede sustituir botas individuales en una línea |
| Desviador | Protección o guía de entrada/salida junto a un poste |
| Defensa | Protección de montacargas en los extremos del sistema |
| Guía de entrada | Par de guías en la entrada de un sistema dinámico |

## Geometría y organización

| Término | Significado en RackCad |
|---|---|
| Frente | Módulo horizontal entre postes. Es el término de UI; no usar “bahía” en texto nuevo |
| Frente en blanco | Frente **Activo/En blanco** del dinámico y de Push Back (I-33): en blanco conserva su claro y su estructura y desplaza a los frentes posteriores, pero no lleva ningún nivel ni componente de carga. Su configuración queda dormida para reactivarlo |
| Fondo | Línea de profundidad de un rack; puede tener sus propios frentes, niveles y fondo de tarima |
| Tramo | Subdivisión de un frente, especialmente en “medio frente” |
| Nivel | Elevación de carga dentro de un frente |
| Celda | Intersección frente × nivel en un diseño |
| Claro | Separación libre calculada alrededor o encima de una carga |
| Fondo de tarima | Profundidad física de la carga |
| Fondo de cabecera | Profundidad del marco; puede derivarse del fondo de tarima con una tolerancia |
| Troquel | Rejilla de perforaciones del poste usada para ajustar elevaciones |
| Peralte | Dimensión de canto o profundidad de un perfil/pieza; no equivale a longitud |
| Elevación | Coordenada vertical nominal o resuelta |
| Frontal | Vista a lo largo de los frentes y largueros |
| Lateral | Corte de perfil por una posición de poste |
| Planta | Vista superior usada también como huella de layout |
| Cota | Anotación de dimensión derivada de geometría resuelta |
| Sección (`Section`) | Índice persistido que identifica fondo, corte o poste según la vista |
| BFR | Ancho frontal resuelto usado por el dinámico para módulos y camas |
| IN/OUT | Largueros de entrada y salida del sistema dinámico |

## Sistemas

| Término | Significado en RackCad |
|---|---|
| Selectivo | Rack pallet-driven con matriz de frentes × niveles, vistas frontal/lateral/planta y seguridad |
| Sistema dinámico | Pallet flow con fondos y niveles variables, camas y largueros de entrada/salida |
| Cama de rodamiento / flow bed | Rieles, rodillos y accesorios que trasladan la carga por pendiente |
| Push Back | Sistema futuro que será la prueba del contrato modular nuevo |
| Medio frente | Frente dividido en varios tramos con postes intermedios |
| Doble profundidad | Varios fondos alineados sobre una rejilla horizontal compartida |
| Layout de almacén | Colocación de huellas de rack y pasillos; no es el optimizador IA futuro |
| Cantilever | Sistema de perfil estructural **estándar** (catálogo neutral de I-36), no de larguero-poste-tarima. Nace en I-37 |

## Cantilever (I-37)

Vocabulario del primer sistema construido sobre secciones estructurales estándar. Los términos que ya
existen arriba —troquel, peralte, nivel, frente— **se reutilizan y no se redefinen**.

| Término | Significado en RackCad |
|---|---|
| Miembro (`StructuralMember`) | Lo que una **sección** es cuando se usa: rol, longitud y orientación. La sección es la forma; el miembro es la pieza. Concepto de [ADR-0020](../adr/0020-catalogo-neutral-de-secciones-estructurales.md), materializado en I-37A |
| Columna | Perfil vertical de un cantilever. Es el `Poste` del glosario general aplicado a este sistema; en Cantilever se dice **columna**, que es como lo nombra el ROADMAP |
| Base | Perfil horizontal que sale de la columna hacia el pasillo y apoya en el piso. **No** es la `Placa base` de una cabecera: un cantilever puede llevar ambas |
| Placa frontal | Tapa del extremo libre de la base. Sin troqueles |
| Placa posterior | Placa del extremo de la base que se conecta a la columna. Sus troqueles los **gobierna la columna** |
| Placa inferior de columna | Placa al pie de la columna, perpendicular a su eje, con contorno igual a la envolvente de su sección |
| Cartabón | Refuerzo triangular entre la placa posterior y la parte superior de la base. **No** es una ménsula |
| Ménsula | **PROHIBIDA en Cantilever.** Ya designa el conector larguero→poste, con catálogo (`mensulas.csv`) y FK propias. Un voladizo de cantilever es un **brazo**; un refuerzo triangular es un **cartabón** |
| Plano de conexión base–columna | Datum `y = 0` del subensamble: el plano de contacto entre la cara de conexión de la columna y la placa posterior. La base sale en `+Y` y el eje de perforación de la conexión es `+Y` |
| Datum de troquel | Identidad **lógica** de una perforación: coordenada transversal, elevación y eje. Dos agujeros del mismo tornillo comparten datum aunque sus centros 3D difieran por el espesor de una placa |
| Patrón de conexión | `CantileverColumnBaseConnectionPattern`: la **única** autoridad de los agujeros que comparten la placa posterior y la columna. No hay dos algoritmos |
| Longitud nominal de corte | Longitud de la pieza para el BOM futuro. En el MVP **es igual** a la longitud geométrica; **no** incluye tolerancias ni preparación de extremos y **no** está liberada para CNC |
| Brazo | Voladizo portante de un cantilever, que sale de la columna hacia el pasillo. **Nunca «ménsula»** |
| Arreglo del cuerpo | Cuántos perfiles forman un brazo: **perfil sencillo**, **canal doble encontrado** o **canal doble espalda con espalda** |
| Canal doble encontrado | Dos canales con las **aberturas enfrentadas**, tocándose por las puntas de los patines en el plano central |
| Canal doble espalda con espalda | Dos canales con los **dorsos de las almas en contacto** y las aberturas hacia afuera |
| Placa de conexión | Placa de la raíz del brazo que se atornilla a la columna. Su cara exterior es donde **inicia** el perfil cortado |
| Tapa | Placa perpendicular al eje del brazo que cierra su extremo libre, sin extensión |
| Tope | La misma placa de la tapa, extendida **hacia arriba** para retener la carga. No altera el corte del perfil |
| Pendiente (`RisePer12`) | Inclinación del brazo, en subida por cada 12 in. Única autoridad: los grados se derivan, no se guardan. El extremo libre sube en **ambos** lados |
| Estación | Una columna con su base o bases y sus brazos por nivel. La unidad que I-37C compone y que el BOM cotiza. **No** lleva posición longitudinal: eso es la línea |
| Góndola sencilla | Estación de **una cara**: una columna, una base y un brazo por nivel, todos en el mismo lado activo |
| Góndola doble | Estación de **dos caras**: **una sola** columna y **una sola** placa inferior, con **dos** bases espejadas y **dos** brazos por nivel. No son dos estaciones |
| Lado activo | En góndola sencilla, el único lado (`PositiveY` o `NegativeY`) donde viven la base y todos los brazos. El opuesto **no existe** como celda |
| Nivel | Una elevación de la estación, compartida por los dos lados en góndola doble. Se identifica por su **índice de troquel inferior**, no por una cota libre |
| Claro libre | Distancia vertical entre la **parte superior del cuerpo** del brazo inferior y la **parte inferior del cuerpo** del superior, medidas en el **plano de conexión**. Ni ejes, ni centros de troquel, ni bordes de placa |
| Retícula regular | `CantileverColumnRegularPunchGrid`: la **única** autoridad de las elevaciones de troquel regular de la columna. La consumen igual I-37A y la estación |
| Margen superior (`TopClearFactor`) | Fracción del claro libre que la columna debe dejar sobre lo último ocupado. Default `1/3`, y nunca menos |
| Componente (BOM) | Lo **atornillable** que el BOM cotiza como una unidad con su receta de piezas: una columna con su base o bases, y cada brazo. Los **troqueles no son piezas** |

## Arquitectura y persistencia

| Término | Significado en RackCad |
|---|---|
| Diseño | Intención editable y persistible, sin coordenadas finales |
| Resolver | Servicio puro que valida y materializa reglas geométricas |
| Sistema resuelto | Resultado con posiciones y dimensiones calculadas |
| Builder | Servicio puro que convierte un sistema resuelto en un plan de vista o BOM |
| Plan | Representación independiente de AutoCAD que un adapter puede materializar |
| DrawService | Adapter del Plugin que convierte un plan en entidades/bloques AutoCAD |
| Kind | Identificador estable del tipo de rack en el sobre persistido |
| GUID | Identidad lógica del rack que liga todas sus vistas |
| Sobre / `RackEmbedDocument` | Contrato con schema, Kind, vista, sección, GUID, nombre y JSON del diseño |
| Xrecord | Mecanismo de AutoCAD usado para guardar el sobre en la definición de bloque |
| Round-trip | Guardar, leer, editar y volver a guardar sin pérdida ni cambio de identidad |
| DTO / Document | Forma versionada de persistencia con fallbacks legacy explícitos |
| Legacy | Documento producido por una versión anterior que debe conservar compatibilidad definida |
| BOM | Lista de materiales; puede ser plana o por componentes y recetas |
| Catálogo | CSV/JSON versionado con perfiles, FKs, bloques, vistas o seguridad |
| Context Pack | Manifiesto ligero que selecciona documentos y áreas relevantes para una iniciativa |

## Proceso

| Término | Significado en RackCad |
|---|---|
| Iniciativa | Unidad de trabajo con un contrato, una rama, un worktree y un Pull Request |
| Reclamo | Primer push aceptado de la rama que reserva una iniciativa |
| Claim-Id | UUID inmutable que identifica el reclamo |
| Gate | Condición externa o decisión que impide reanudar una fase |
| Estado versionado | Archivo `docs/automation/state/<initiative>.yml` usado para reanudar el ejecutor |
| Decisión versionada | Evidencia del dueño bajo `docs/automation/decisions/` que puede resolver un gate |
| Integrada | Iniciativa contenida en `main`; “completada” en su rama no implica integración |
| Worktree | Checkout registrado y exclusivo de una iniciativa durante toda su vida |
