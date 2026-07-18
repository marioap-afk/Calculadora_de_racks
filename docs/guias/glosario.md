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
