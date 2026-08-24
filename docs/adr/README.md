# ADR — Architecture Decision Records

Registro de decisiones de arquitectura de RackCad. Cada ADR captura UNA decisión con su contexto,
para que nunca vuelva a re-litigarse sin saber por qué se tomó (el HANDOFF ya necesitaba avisos
"NO re-proponer": los ADR son la solución estructural a eso).

## Formato y numeración

- Archivo: `NNNN-slug-en-kebab.md` (4 dígitos, secuencial, **nunca se reutiliza un número**, ni
  siquiera el de un ADR rechazado).
- Plantilla: [plantilla.md](plantilla.md). Secciones: Estado, Contexto, Decisión, Alternativas
  consideradas, Consecuencias, Referencias.
- Estados: `propuesto` → `aceptado` | `rechazado`; un aceptado puede pasar a
  `reemplazado por ADR-NNNN` u `obsoleto`. **Solo el dueño del repo acepta o rechaza.**
  Los agentes pueden redactar ADRs en estado `propuesto`.

## Cuándo crear un ADR

Crear uno ANTES de implementar cuando la decisión cumpla al menos uno:

1. **Restringe trabajo futuro** en más de un módulo o capa (contratos, registros, formatos de
   persistencia, esquema de catálogos, unidades).
2. **Es cara de revertir** (formatos en disco/DWG, nombres públicos, dependencias).
3. **Cierra un debate recurrente** o una opción que alguien volvería a proponer (validación de
   cargas, SQLite, optimizador IA…).
4. **Es una excepción a una convención** de AGENTS.md (p. ej. permitir un paquete NuGet de build).

No crear ADR para: elecciones locales de implementación, nombres internos, decisiones reversibles
en una sesión. Esas van en comentarios de código o en el cuerpo del commit.

## Cuándo modificar / reemplazar

- Un ADR `aceptado` es **inmutable** en su contenido: solo cambian su Estado y sus enlaces. Se
  permiten correcciones tipográficas y una sección final "Notas posteriores" con fecha.
- Para cambiar la decisión: escribir un ADR nuevo que la reemplace y marcar el viejo como
  `reemplazado por ADR-NNNN`. **Nunca borrar un ADR**: la historia de decisiones es el valor.
- Un `propuesto` sí puede editarse libremente hasta que el dueño lo acepte o rechace.

## Índice

| # | Título | Estado |
|---|---|---|
| [0001](0001-ramas-por-iniciativa.md) | Ramas por iniciativa técnica, no por herramienta | aceptado |
| [0002](0002-secuencia-dinamico-modular.md) | Secuencia de integración de la rama del dinámico modular | aceptado |
| [0003](0003-referencias-autocad-para-ci.md) | Referencias AutoCAD para compilación en CI | aceptado |
| [0004](0004-estrategia-de-versiones-de-autocad.md) | Estrategia de versiones de AutoCAD | aceptado |
| [0005](0005-estrategia-de-unidades.md) | Estrategia de unidades | aceptado |
| [0006](0006-autocad-solo-en-plugin.md) | AutoCAD solo en RackCad.Plugin | aceptado |
| [0007](0007-catalogos-csv-excel-first.md) | Catálogos CSV Excel-first sin base de datos | aceptado |
| [0008](0008-secciones-unificadas-por-rol.md) | Perfiles estructurales unificados en secciones.csv por rol | reemplazado por [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) |
| [0009](0009-identidad-guid-embebida-en-dwg.md) | Identidad de rack mediante GUID embebido en el DWG | aceptado |
| [0010](0010-actualizar-redibuja-insertar-liga-vistas.md) | Actualizar redibuja e Insertar agrega una vista ligada | aceptado |
| [0011](0011-parametros-dinamicos-con-patron-array.md) | Parámetros dinámicos mediante definiciones compartidas con patrón ARRAY | aceptado |
| [0012](0012-producto-sin-dependencias-nuget.md) | Código de producto sin dependencias NuGet | aceptado |
| [0013](0013-parrilla-una-por-tarima.md) | Parrilla una por tarima, contada en `SelectiveFrontalBuilder.ParrillaRow` | aceptado |
| [0014](0014-copia-central-seguridad-selectivo.md) | Copia centralizada de la selección de seguridad del selectivo en `DeepCopy` | aceptado |
| [0015](0015-entrada-numerica-localizada.md) | Entrada numérica localizada sin separador de miles | aceptado |
| [0016](0016-cantidad-parrilla-acotada.md) | Cantidad de parrilla acotada por la UI y por el builder | aceptado |
| [0017](0017-validacion-cargas-diferida-ram-elements.md) | Validación estructural de cargas diferida a RAM Elements | aceptado |
| [0018](0018-optimizador-layout-ia-diferido.md) | Optimizador de layout con IA diferido; `RACKLAYOUT` determinista vigente | aceptado |
| [0019](0019-shell-visual-de-editores-por-composicion.md) | Shell visual de editores por composición y slots, agnóstico al sistema | aceptado |
| [0020](0020-catalogo-neutral-de-secciones-estructurales.md) | Catálogo neutral de secciones estructurales | aceptado |
| [0021](0021-identidad-unidades-y-presentacion-de-secciones.md) | Identidad, unidades y presentación de secciones estructurales | aceptado |
| [0022](0022-geometria-parametrica-de-secciones-estructurales.md) | Geometría paramétrica y representación prismática de secciones estructurales | aceptado |
| [0023](0023-geometria-visual-derivada-perfiles-s.md) | Geometría visual derivada para los perfiles S (IPS) | aceptado |
| [0024](0024-fundacion-cantilever-base-columna.md) | Fundación Cantilever: diseño en Domain, resolución en Application y autoridad compartida base–columna | aceptado |
| [0025](0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) | El brazo Cantilever: cuerpo simple o compuesto, y su conexión a la columna | aceptado |
| [0026](0026-estacion-cantilever-niveles-altura-y-bom.md) | La estación Cantilever: caras, niveles, altura y BOM por componentes | aceptado |
| [0027](0027-linea-cantilever-intervalos-y-arriostramiento.md) | La línea Cantilever: intervalos, distribución de paneles y arriostramiento | aceptado |
| [0028](0028-cantilever-persistencia-vistas-editor-y-dibujo.md) | Cantilever visible: persistencia, registro, vistas, editor y materialización | aceptado |
| [0029](0029-contrato-funcional-comun-de-ventanas-wpf.md) | Contrato funcional común de ventanas WPF | aceptado |
| [0030](0030-fondo-por-celda-push-back-y-envolvente-derivada.md) | El fondo de Push Back es de la celda; el del frente es una envolvente derivada | aceptado |
| [0031](0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) | El Push Back compuesto tiene UNA estructura física y DOS configuraciones funcionales | propuesto |

Iniciativa `docs/adr-retroactivos` (I-07): los ADR-0006…0018 retro-documentan las trece decisiones de la
antigua tabla de HANDOFF §7, una por ADR, y fueron **aceptados por el dueño el 2026-07-22** («Sí,
apruebo»; decisión versionada en
[`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md)). Con la integración de I-07 esas
decisiones dejan de conservarse en HANDOFF §7 y pasan a estos registros; la matriz decisión → ADR vive en
el [contrato de I-07](../initiatives/I-07-adr-retroactivos.md). El ADR-0002 histórico
(`0002-secuencia-dinamico-modular.md`) conserva su archivo de evidencia hermano
(`0002-paso0-evidencia.md`), no indexado por ser un apéndice de la misma decisión, no un ADR aparte. Los
ADR-0017 y 0018 registran diferimientos por decisión del dueño con respaldo documental; su limitación de
evidencia queda escrita en cada registro.

Iniciativa I-36A (`architecture/catalogo-secciones-estructurales`): **ADR-0020 reemplaza a ADR-0008**.
El reemplazo es de **autoridad conceptual** —`StructuralSection` pasa a ser la autoridad neutral de la
sección transversal, sin rol de miembro—, **no de comportamiento**: `secciones.csv` y su división por
`rol` siguen operando sin cambio como catálogo legado de los miembros actuales hasta que las
migraciones futuras, una por configurador, las retiren. **ADR-0021 NO reemplaza a ADR-0005**: la
pulgada sigue siendo la unidad geométrica interna y la política del DWG no cambia; ADR-0005 solo
recibió una nota posterior con el enlace. Ambos ADRs recogen decisiones vinculantes del dueño
versionadas en [`docs/automation/decisions/I-36A.md`](../automation/decisions/I-36A.md).

**ADR-0021 quedó `aceptado` el 2026-07-28**, junto con el gate `owner-validation` de I-36A y sus siete
puntos. Estuvo deliberadamente en `propuesto` durante tres rondas —su decisión central, la política
exacta de IDs, seguía bajo gate y el dueño la había rechazado parcialmente en la primera—, y pasó a
`aceptado` solo cuando la aceptación fue expresa: identidad
`{ID_NAMESPACE}-{FAMILIA}-{EDI_NORMALIZADO}`, `ID_NAMESPACE` como autoridad explícita declarada por la
fuente, `AISC-SHAPES` con namespace `AISC`, la revisión fuera del ID, y para `HSS4X4X1/4` su Manual
Label visible con EDI `HSS4X4X.250` e ID `AISC-HSS-RECT-HSS4X4X_250`.

Iniciativa I-36B (`architecture/geometria-secciones-estructurales`): **ADR-0022 quedó `aceptado` el
2026-07-28** y no reabre a ADR-0020 ni a ADR-0021. Nació `propuesto` a propósito: sus decisiones
—origen en el centroide tabulado, Z longitudinal, sección frente a instancia prismática, radios solo
derivables de forma documentada, wireframe sin líneas ocultas y bloque interno en vez de biblioteca por
designación— son **verificables sobre el dibujo real**, y el gate `owner-validation` de I-36B incluía
AutoCAD, así que aceptarlas antes de que el dueño las mirase habría dicho lo contrario de lo que
ocurría. El dueño ejecutó el smoke focalizado y el checklist completo y aprobó los doce puntos.

Dos apuntes que el registro deja explícitos y conviene no perder. **Los canales C son
`TabulatedDerived`, no una geometría idéntica a la de una librería comercial**: al compararlos, el dueño
constató que les falta la conicidad de los patines, los redondeos de punta y las transiciones del
laminado, y que esa diferencia es justamente la que explica el error de área conocido del 5.545 %. La
aceptó sin bloquear, con la condición expresa de **no inventar** esas dimensiones aquí. Y la **mejora
visual de perfiles laminados junto con los perfiles IPS/S** queda **diferida como requisito futuro
obligatorio** a una iniciativa separada, que deberá mantener apartada la geometría visual de la
tabulada y **no alterar** esta última.

Iniciativa I-36D (`feature/perfiles-aisc-s`): **ADR-0023 quedó `aceptado` el 2026-07-28** y es esa iniciativa
separada. **No reemplaza** a ADR-0020, ADR-0021 ni ADR-0022: los **extiende** para el caso que
ADR-0022 difirió por nombre. Introduce la primera geometría de RackCad que **no** es trazable punto a
punto a un dato publicado, y por eso separa dos autoridades: la **tabulada** (AISC conserva identidad,
dimensiones, `A`, peso, propiedades y centroide tabulado) y la **visual derivada** (RackCad declara
como propias la pendiente `1:6`, la interpretación de `tf` como espesor medio del vuelo libre, el
radio visual del filete, la terminación de punta y las advertencias). El motivo está medido, no
supuesto: la AISC Shapes Database v16.0 **no publica pendiente de patín ni ningún radio explícito**, y
una S sin pendiente se lee como una **W** —una pérdida de familia, no de detalle, que es lo que la
distingue del caso ya aceptado de los canales C—. La regla **degenera exactamente** en la de ADR-0022
cuando la pendiente es cero, así que no bifurca el modelo geométrico. La autoridad viaja en un eje
**ortogonal** a `SectionFidelity` (`TabulatedConstrained` frente a `VisualDerived`), que no cambia.
Como en I-36B, **solo el dueño podía aceptarla y solo después de ver el dibujo real**: ejecutó la
validación manual en AutoCAD 2025 y **ADR-0023 quedó `aceptado` el 2026-07-28**, con los nueve puntos
de su sección de aceptación y sin observaciones. La aceptación **no amplía** el alcance: no autoriza
extender la convención a otras familias, ni radio de punta, ni sólidos 3D, ni que I-37 defina geometría
de S por su cuenta.

Iniciativa I-37A (`architecture/cantilever-base-columna`): **ADR-0024 quedó `aceptado` el 2026-07-29**. Es el primer ADR
del lado **consumidor** del catálogo neutral y fija cuatro cosas que condicionan todo lo que Cantilever
construya encima: el **diseño vive en Domain con el id de sección como texto** (Domain no puede referenciar
Application, y `StructuralSectionId` tiene constructor privado y ningún `JsonConverter`, así que el DTO
guardaría texto de todos modos); el **resultado es híbrido**, con un tipo por **naturaleza física** —perfil
del catálogo, placa, cartabón, troquel— y el rol como enum dentro del plan de miembro, en vez de una
jerarquía por rol que multiplicaría los `switch`; el **patrón de conexión base–columna tiene una sola
autoridad**, y su coincidencia se comprueba sobre un **datum lógico** (X, Z y eje) y nunca comparando
centros 3D separados por el espesor de una placa; y **`NominalCutLength` existe, es igual a la longitud
geométrica y no está liberada para fabricación**. Toda dimensión exterior se deriva de
`StructuralSectionGeometry.Bounds`, nunca de `d`/`bf`/`tw`/`tf`, lo que mantiene intacta la frontera que
ADR-0020 y ADR-0022 fijaron. **No decide** vistas, persistencia, registros, BOM ni peso: cada una es una
decisión de su propia iniciativa.

A diferencia de ADR-0022 y ADR-0023, cuyo gate era **ver el dibujo**, ADR-0024 se aceptó **sobre el
código**: I-37A no dibuja nada —sin vistas, editor, persistencia ni Plugin—, así que lo verificable de sus
contratos son las invariantes y las guardas, no una captura. Veredicto normativo
`OWNER_APPROVED_ADR_0024` sobre el SHA técnico `1552367`. La aceptación **no** autoriza vistas, UI,
AutoCAD, persistencia de proyecto, brazos, estaciones, separadores, arriostres, BOM, peso, cálculo,
fabricación ni cambios a I-36.

Iniciativa I-37B (`architecture/cantilever-brazo`): **ADR-0025 quedó `aceptado` el 2026-07-29**. Extiende ADR-0024 de
forma **aditiva** y decide las cuatro cosas que la base no tenía: que el cuerpo del brazo es una
**colección** de miembros con el arreglo como enum —y no una subclase por arreglo, que multiplicaría los
`switch` de tipo—; que la longitud capturada es el **corte del perfil** y no incluye placas, así que cambiar
un espesor **mueve** el brazo en vez de acortarlo; que el brazo **selecciona** troqueles ya resueltos de la
columna y **observa** su pitch en vez de recalcularlo; y que un perfil demasiado aperaltado para sus filas
**se rechaza** en lugar de estirar la placa en silencio. La orientación canónica del canal se **leyó** de
`ChannelSectionGeometryBuilder` —dorso a −X, patines abriendo a +X— y de ahí salen los dos arreglos dobles
sin leer una sola dimensión tabulada.

Se aceptó **sobre el código**, como ADR-0024 y por la misma razón: I-37B no dibuja. Veredicto normativo
`OWNER_APPROVED_ADR_0025_WITH_CURRENT_DATUM` sobre el SHA técnico `00d8126`. El sufijo del veredicto no es
adorno: la aceptación **conserva expresamente el datum actual**, según el cual la cara exterior de la placa
de conexión es el **origen del plano de corte**. Con pendiente y corte a escuadra eso **no** es quedar a
ras —una zona del perfil penetra visualmente la placa y la opuesta deja holgura, y las dos magnitudes se
reportan **por separado**—, así que lo aceptado es una **aproximación visual declarada** y no una geometría
exacta; el **corte inclinado** y la **preparación de extremo** siguen fuera de alcance. La aceptación **no**
autoriza estación, niveles, doble cara, separadores, arriostres, línea, BOM, peso, persistencia,
`RackSystemKind`, registros, editor, preview, vistas, AutoCAD, bloques, fabricación, cálculo resistente ni
cambios funcionales en I-36 o I-37A.

Iniciativa I-37C (`architecture/cantilever-estacion-bom`): **ADR-0026 quedó `aceptado` el 2026-07-29**. Extiende ADR-0024 y
ADR-0025 hacia el primer **compositor**, y decide las cinco cosas que ninguna de las dos piezas tenía: que
una **góndola doble** es **una** columna con **dos** bases espejadas y no dos estaciones —modelarla como dos
subensambles duplicaría la columna y el BOM contaría mal—; que los **templates** de estación no llevan los
valores que la estación gobierna, para que no exista una altura o un índice dormidos; que la dependencia
circular **altura → troqueles → niveles → altura** se resuelve con una **secuencia explícita** sobre una
**autoridad única de retícula** que I-37A también consume, y con un **pase final verificado** que falla
cerrado si difiere; que el **claro libre** se mide cuerpo a cuerpo en el plano de conexión y el ajuste es
**obligatorio hacia arriba** a un índice de troquel; y que el **componente del BOM** es lo atornillable —una
columna con su base o bases, y cada brazo—, con los troqueles fuera y con un brazo idéntico en los dos lados
contando como **el mismo** componente.

Se aceptó **sobre el código**, como las dos anteriores de la línea y por la misma razón: I-37C todavía no
dibuja. Veredicto normativo `OWNER_APPROVED_ADR_0026` sobre el SHA técnico `e1c3cab`, con **dos
caracterizaciones previas** —la retícula regular y las métricas de conexión del brazo— y **veintitrés**
regresiones comprobadas en rojo. Sus veintidós puntos aceptados están enumerados en su propia sección, y
**no se reabren en I-37D**: la iniciativa final del MVP compone la estación, no vuelve a decidir cómo se
resuelve.

Dos de esos puntos merecen leerse juntos, porque son la respuesta a una circularidad real. La **retícula
regular** tiene una sola autoridad, que I-37A también consume, y **acumula** —no multiplica— para preservar
exactamente lo que I-37A enviaba; su **dominio** es numérico y derivado de la precisión del `double`, no un
límite comercial. Con eso la dependencia altura→troqueles→niveles→altura se resuelve como una secuencia de
once pasos cuyo pase final se **verifica** contra el layout previo y **falla cerrado** si difiere.

Iniciativa I-37D (`feature/cantilever-mvp-final`): **ADR-0027 y ADR-0028 nacieron `propuestos` y quedaron
`aceptados` el 2026-08-03**, tras la validación manual del Owner en AutoCAD 2025
(`OWNER_APPROVED_I37D_MANUAL_VALIDATION`, sin defectos bloqueantes). Entre los dos cierran el MVP. **ADR-0027** decide la geometría de la línea: que un intervalo es del **par** de estaciones
adyacentes —y no de una de ellas, que es lo que convertiría a la primera o la última en un caso especial y
haría contar un separador dos veces—; que la cantidad de paneles arriostrados es una **regla**,
`max(1, ceil((H − 72)/60))`, que reproduce las doce filas de la tabla de producto y sigue respondiendo en la
altura trece; que los paneles se agrupan **desde abajo en bloques de dos** con el bloque incompleto arriba y
sólo el remanente repartido a los extremos; que el **corte de un separador** lo dictan los **agujeros de las
placas de columna** y nunca la separación entre centros, porque restar un ancho de columna cableado deja de
valer al cambiar de sección; y que un tensor **cold rolled** necesita **adaptadores**, porque una varilla no se
taladra y atornilla como un perfil y sin ellos el BOM no se puede comprar.

**ADR-0028** decide cómo se hace visible: se persiste la **intención** versionada y nunca el resultado
resuelto; el sistema entra en el **registro vigente** y ningún string de kind vive fuera de él; las tres vistas
son **planes puros** que sólo el Plugin materializa, de modo que una vista se puede probar sin AutoCAD; el
editor va **sobre el shell visual común** y una operación de matriz produce **una** regeneración y no una por
celda; y la materialización **reutiliza** el Drawing vigente en vez de copiar otro sistema. Su gate es el
**veredicto manual del Owner en AutoCAD 2025**: CI verde es necesario y **no** suficiente, porque las pruebas
no ven los bloques DWG reales.

Iniciativa I-39A (`architecture/contrato-funcional-ventanas-wpf`): **ADR-0029 nació `propuesto` y quedó
`aceptado` el 2026-08-07** por el dueño, con el veredicto `OWNER_APPROVED_I39A_MANUAL_VALIDATION` sobre el
candidato validado en AutoCAD 2025 — el mismo criterio de ADR-0023 en I-36D: un ADR que gobierna lo que se ve
no se acepta a ciegas, sino después de ver la ventana real. **No reemplaza a ninguna ADR: complementa
ADR-0019**, que decide cómo se compone lo visual, mientras
ADR-0029 decide cómo se comporta lo funcional —estado, transacción, acciones y motivos de bloqueo, validación,
preview, dirty, cierre, teclado, foco, ownership, tamaño, diagnóstico y recomputación observable—. ADR-0019
permanece `aceptado` e inmutable y ninguna de sus seis reglas se reabre. Sus decisiones de fondo: el inventario
de ventanas se hace **por tipo y nunca por `x:Name`**; cuatro arquetipos con asignación obligatoria; estados
**ortogonales** en vez de un enum lineal; el preview con **dos ejes**, autoridad y frescura; cinco grados en un
valor capturado, donde una entrada inválida **no** sobrescribe en silencio un valor aplicado válido; acciones
que declaran semántica y **motivo visible al bloquearse**; Escape que **no** puede perder cambios en silencio y
un único camino de cierre para botón, Escape, `Alt+F4` y botón de sistema; dirty como propiedad de un **ámbito**
y no de la ventana; y el contrato de tamaño **por arquetipo**, de modo que el arquetipo B no hereda los mínimos
del editor rico A. **No** decide MVVM, **no** unifica los mecanismos de recomputación, **no** fija resolución ni
DPI mínimos y **no** autoriza corregir el defecto de Escape de Push Back, que espera a I-39B. Decisiones
vinculantes del Owner en [`docs/automation/decisions/I-39.md`](../automation/decisions/I-39.md).
