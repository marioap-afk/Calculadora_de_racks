# Revisión independiente y adversarial de I-64 A-14, borrador v1

## 0. Identidad, modo y método

- **Objeto:** `S/I-64-A-14-draft-v1.md`, SHA-256 `7571e16509b6be965efa9d6f6346f8c7701eb9ab104c9d81595ba39fb3342dfb` (598 líneas). Se cita «D:n».
- **Freeze:** `W/docs/initiatives/I-64-proposal-v5.md`, blob `dc1924ff5a73a4e529fbc5f995621d29351a091b` (comprobado en HEAD de W
  `fa59bd82` y en `55e7ca63`, la imagen de `9b43dafb` en la rama). Se cita «V5:n».
- **Decisiones del Owner:** `S/owner-a14-decisions.md`, con la «Segunda ronda» (Q-03, Q-07, Q-08, Q-09), SHA-256 `22ab78b4…94ed`;
  `S/replaneacion-i64.html` §3, §4 y §7, SHA-256 `79175286…8a4f`.
- **Otras fuentes leídas en W, solo lectura:** `INITIATIVE_LIFECYCLE.md` (entero), `WORKFLOW.md` §10 y §11.4-11.5, A-1, A-7, A-11, A-12 y A-13
  (cabeceras; A-7 en lo que toca a S2-16..22), `I-64-implementation-clarifications.md` (entero), `owner-f4b-disposition.md` (entero, «OD»),
  el paquete de F4B v3.1 (`:1-260`, `:400-476`, «PKG»), `f4b-dispositions.md` (F4B-E01, E02, E62), el paquete de PB-T5 v2.4 (`:240-300`),
  `docs/automation/state/I-64.yml`, el brief del Owner (por líneas), ADR-0029 D7..D13, ADR-0047, `docs/adr/README.md`, y en `src/`:
  `RackMenuCommands.cs`, `RackMainMenuWindow.xaml`, `WorkspacePaletteHost.cs`, `WorkspacePanelCommand.cs`, `UserSettings.cs`,
  `RackSiblingRedrawRun.cs`, `ISiblingRedrawPort.cs` y el censo de `CommandMethod` del Plugin.
- **Modo:** subagente independiente, sesión distinta de la del redactor; W intacto (sin escrituras ni órdenes git que cambien estado);
  sin AutoCAD, sin compilar, sin pruebas. Las notas del redactor se leyeron **después** de formar el juicio; el §5 dice qué aportaron.

## 1. Veredicto

**APTO TRAS CORRECCIONES.**

| | Bloqueantes | Importantes | Menores |
|---|---|---|---|
| Defectos del v1 | 0 | 11 | 14 |
| Por la segunda ronda del Owner (posterior al v1; no son defectos del v1, pero la v2 debe recogerlos) | 2 | 0 | 2 |

Las citas literales de V5, del brief, de [OD], del paquete de F4B, de PB-T5 y del registro del Owner son, salvo los menores M-01 y M-02,
exactas. Todas las líneas que el borrador cita se comprobaron. No hay rutas locales, nombres de personas, correos ni datos de clientes en el
texto. Los problemas están en otra parte: lo que el borrador dice que **no** cambia y sí cambia, y riesgos técnicos que nombra sin
tratarlos.

## 2. Hallazgos por la segunda ronda del Owner (la v2 los debe incorporar)

### R2-01 · BLOQUEANTE (para la v2) · Q-03: «RACKEDITAR adopta el diseño nuevo» contradice varias afirmaciones del v1

- **Decisión:** `owner-a14-decisions.md:34-37`: «Adopta el diseño nuevo (Recomendado)». Según la descripción, RACKEDITAR abre la misma
  pantalla rediseñada en una ventana aparte, hay un solo editor por sistema y «El cambio se declara cuando ocurra».
- **Lo que el v1 dice y deja de valer:**
  - D:306: «Los comandos existentes siguen sin cambio observable (D-16, INV-19) hasta la fase 9, salvo RACKCAD».
  - D:345: «El contrato de adaptador de D-15 (cinco puntos), la caracterización previa por sistema y el criterio de entrada no cambian».
  - D:265-266: «Los comandos de creación no cambian». La ventana de cada sistema es la misma para crear y para editar. Por ejemplo,
    `RackPushBackSystemWindow` la abren `RackPushBackCommands` y el camino de RACKEDITAR (PKG `:171`). Con «un solo editor por sistema»,
    los comandos de creación también cambian cuando llega el diseño nuevo.
  - D:497 (R-07): «RACKEDITAR como respaldo» y D:135-136 («los flujos clásicos quedan como respaldo»). Con el rediseño, RACKEDITAR
    deja de ser un respaldo con la interfaz de hoy.
  - D:504 (R-14) y D:574 (Q-03): ya están resueltos.
- **Fuentes que hay que enmendar:** V5:586-587 (D-16, sin cambio observable en RACKEDITAR y en los comandos de creación); V5:593 (el STOP de
  D-16 por M-03); V5:570-572 (la caracterización «sigue en verde después de la extracción»); V5:757 (INV-19); V5:996 (la condición de
  M-03); V5:949 (OV-17, «Compatibilidad con RACKEDITAR»: cambia su significado, así que sustituirlo es del Owner, V5:960); el brief
  `:393-394` («Existing standalone RACKEDITAR behavior should remain usable during migration unless Freeze explicitly changes it»);
  ADR-0029 D13 («la caracterización no se edita para que pase»).
- **Corrección:**
  - un cambio C-nn propio: «RACKEDITAR y los comandos de creación de cada sistema rediseñado abren la superficie nueva en una ventana
    aparte, desde la fase 6 (Selectivo, Dinámico y Cantilever), la 7 (Cabecera y Cama) y la 3b (Push Back)»;
  - D-16, INV-19 y M-03 se enmiendan por sistema y por fase;
  - la caracterización previa se mantiene antes de extraer; tras el rediseño, en lugar de editarla, se escribe un registro de
    caracterización sustituta declarado, por sistema, con el patrón de [OD] `:237-243` («create a declared superseding
    characterization/change record. Do not silently weaken the old guard») y el precedente de `F4B-DRAFTS/superseding-characterization.md`;
  - OV-17 se sustituye por un texto nuevo («RACKEDITAR abre la superficie rediseñada y conserva el flujo clásico de escritura») con
    autoridad del Owner;
  - se cita el brief `:393-394` como la cláusula que A-14 cambia de forma explícita.

### R2-02 · BLOQUEANTE (para la v2) · Q-09: «Por tramos» necesita una regla en A-14, no solo una nota

- **Decisión:** `owner-a14-decisions.md:39-42`: «Por tramos (Recomendado)». Se integra en main en hitos que elige el Owner; cada hito lleva
  su candidato completo y la validación del Owner.
- **Lo que el v1 dice:** D:446 («No hay integración parcial en `main` sin decisión del Owner (A64-PV1-21, `:803-805`; Q-09)»), D:580
  (Q-09) y D:501 (R-11).
- **Choque y huecos:**
  - V5:803-805 ya admite la integración parcial con decisión del Owner, así que la decisión no contradice A64-PV1-21: es la que esa regla
    pide. Pero A-14 debe escribirla.
  - Falta el mecanismo, que hoy no existe en las fuentes:
    - LIFECYCLE §2 (`:54-64`): cada unidad de entrega hace su propio READY, Candidato e integración;
    - LIFECYCLE §6 (`:198`): `Applies-to` con la lista de unidades;
    - LIFECYCLE §8 (`:245`, READY-08): la asignación OV por unidad;
    - V5 §12.1 (`:828-829`): un hito intermedio no sustituye al `FINAL_CANDIDATE_SHA`. Con tramos, cada tramo tiene el suyo;
    - V5 §12.5 y §11: Smoke-PERF va antes de READY. ¿En cada tramo, o solo en el último?;
    - D-20 (V5:681-683): ID3 se clasifica en READY. ¿En qué tramo?;
    - D-21 (V5:687-693): ADR-0047 se acepta y las notas del anexo B se escriben en el commit de cierre. Tras el primer tramo, el ADR
      aceptado es inmutable;
    - WORKFLOW §11.5 (`:510-530`): tras el merge vienen la limpieza de rama y worktree y el tag;
    - IC-11 (ICL `:204`) y OB-02.3 de F4B (PKG `:200`): «No hay integración parcial en `main`». Rigen solo dentro de F4B, pero hay que
      decirlo.
- **Corrección:** un cambio C-nn propio con:
  - la decisión literal;
  - la regla de que cada tramo es una unidad de entrega (LIFECYCLE §2), con su orden del Owner, READY-01..09, `FINAL_CANDIDATE_SHA`,
    conformidad y OV de los escenarios que entrega. Precedente de subunidades: I-53S e I-53D, e I-52-AUTH15;
  - ID3 se clasifica solo en el último tramo;
  - Smoke-PERF en cada tramo que cambie el rendimiento, o una regla explícita;
  - una propuesta de cuándo se acepta ADR-0047 (el Owner lo decide en su momento);
  - el ámbito de F4B, sin cambio.
  - La frontera de cada tramo se decide por tramo y no hace falta preguntarla ahora.

### R2-03 · MENOR · Q-07: consecuencias en el texto

- **Decisión:** `owner-a14-decisions.md:44-48`: RK hace lo mismo que RACKCAD; hasta la fase 9, la ventana tiene una entrada que abre el
  menú de hoy como respaldo.
- **Consecuencias:**
  - D:306 y D:578 quedan resueltos. RK ya es el mismo método (`RackMenuCommands.cs:21`: `AliasRackCad() => RackCad()`), así que la
    primera parte se podía deducir del código;
  - INV-A14-02 (D:418) y la lista de retiro de la fase 9 deben incluir esa entrada de respaldo;
  - abrir el menú modal desde la paleta es otro flujo clásico lanzado desde la ventana: entra en R-13 (D:503).

### R2-04 · MENOR · Q-08: decidido, «Ahora no»

- **Decisión:** `owner-a14-decisions.md:50-53`.
- **Corrección:** D:579 pasa de pregunta a decisión registrada (fuera de A-14; se decide con el diseño de Configuración).

## 3. Hallazgos del v1

### I-01 · IMPORTANTE · La lista «Sin cambio» declara intactas cláusulas que los cambios sí alteran, y faltan cláusulas en «Amends»

- **Cita:** D:21-22: «Sin cambio: … D-04; D-05; D-07; D-10; D-11; D-12; D-14; … D-18; D-19; D-21; §12.1». D:517: «No cambia … D-03, D-04,
  D-05, D-07, D-10, D-11, D-12, D-14 o D-17 en su semántica».
- **Lo que contradicen las fuentes:**
  - **D-04** (V5:241-243): el ámbito de mutación propia «se indexa por (sesión, miembros del rack)». C-04 lo cambia a «la unión de los
    miembros» (D:251) e INV-A14-10 (D:426). Es un cambio de D-04 (O-15).
  - **D-05** (V5:259, «pestañas y borradores»; V5:298-299, «las tres entradas: el contexto, las pestañas y el navegador»; V5:324-325, «El
    rechazo por admisión … se muestra en la pestaña»): las tres frases nombran pestañas.
  - **D-10** (V5:425-426, la preview de navegación; V5:440, la vida del borrador «hasta cerrar la pestaña»): ver I-05.
  - **D-14** (V5:549-551): «cerrar una pestaña limpia» y «Una pestaña con dirty … nunca es preview».
  - **D-18** (V5:647-651): los escenarios 5 («cambio de pestaña») y 6 («10 pestañas ligeras») y la puerta F6 de los escenarios 7 y 8.
  - **D-21** (V5:689-690): ADR-0010 se complementa con «todo o nada». La nota prevista (V5:1219-1221) dice «el authored del rack», y C-04
    la contradice. M-08 lo reconoce (D:538), pero el texto de la nota no se enmienda.
  - **Faltan en «Amends»:**
    - §0 (V5:53: Actualizar es «la única frontera de escritura en el DWG desde el panel»; V5:56: `NavigationTargets` es «común al
      contexto, las pestañas y el navegador»);
    - §2 (V5:105: «Cierre … política de pestaña, panel y documento»);
    - §14 (M-02 y M-03, V5:995-996: se reabren, y el v1 lo dice en §7 sin enmendarlo);
    - el **anexo A**: decisión 1 «sin estado persistido por RackCad» (V5:1188), 6 «Pestañas … preview» (V5:1199), 8 «todo o nada sobre
      el authored del rack» (V5:1202-1203), 10 «comandos clásicos sin cambio» (V5:1205), 11 «Persistencia: ninguna propia» (V5:1207) y
      las consecuencias (V5:1215). Todas se copiaron a ADR-0047 (`:18`, `:27`, `:30`, `:33`, `:34`);
    - el **anexo B**: las notas de ADR-0010 (V5:1219-1221) y de ADR-0032, «cambiar de pestaña o de rack» (V5:1223).
- **Por qué importa:** LIFECYCLE §6 (`:199`: «clausula anterior y delta exacto»; `:203`: «No existe enmienda semántica invisible»). D:522
  solo promete actualizar ADR-0047 «cuando A-14 esté activa», sin enmendar el anexo del que nace.
- **Corrección:**
  - pasar D-04, D-05, D-10, D-14, D-18 y D-21 a «Amends», cada una con su delta o con la regla «se reescribe al emitir la fase N»;
  - añadir §0, §2, §14 y los anexos A y B, con el texto nuevo de las decisiones 1, 6, 8, 10 y 11 y de las dos notas;
  - quitar esas cláusulas de D:517.

### I-02 · IMPORTANTE · La tabla de invariantes, OV y obligaciones heredadas está incompleta

- **Cita:** D:399-411 (invariantes afectados) y D:431-437 (OV).
- **Faltan:**
  - **INV-15** (V5:753: «N pestañas → K instancias»): su oráculo se define por pestañas;
  - **INV-37** (V5:775: «el rechazo por admisión vive en la pestaña»);
  - **INV-08** (V5:746: el ámbito por (sesión, miembros)): el v1 añade INV-A14-10 sin marcar el cambio de INV-08;
  - **INV-20** (V5:758): R-16 (D:506) dice «INV-20 se extiende», pero la tabla no lo recoge;
  - **INV-02** (V5:740): el ciclo «`RACKPANEL` repetido» debe incluir «RACKCAD repetido»;
  - **INV-05, INV-18 e INV-25** (V5:743, :756, :763): nombran la pestaña;
  - **OV-11** «Cambiar de pestaña sin escribir» (V5:943) y **OV-20** «Cerrar el panel o una pestaña» (V5:952): cambiar su texto es
    sustituirlos, y eso exige al Owner (V5:960);
  - **SP-02** (V5:908), «cambio … de pestaña y 10 pestañas ligeras». El v1 solo enmienda SP-03 (D:20);
  - **IC-02** (ICL `:53`) e **IC-05** (ICL `:89`): las obligaciones «SN-06c y la parte de pestañas de SN-10» se quedan sin objeto, y D:475
    dice «IC-01 e IC-02 siguen».
- **Choque de fondo:** D:476 y D:510 dicen que F3 «no se reabre», pero la fase 2 borra resultados que F3 certificó: la regla de preview de
  INV-04, el escenario 6 e INV-15 (V5:792). Eso exige sustituir pruebas certificadas, y ADR-0029 D13 prohíbe editarlas para que pasen.
- **Corrección:**
  - completar la tabla;
  - llevar OV-11 y OV-20 a la lista del Owner;
  - declarar SP-02 y las obligaciones de IC-02 e IC-05;
  - escribir que los resultados de F3 que A-14 retira quedan sustituidos, sin reabrir F3, con un registro declarado de las pruebas que se
    sustituyen (el mismo patrón de R2-01).

### I-03 · IMPORTANTE · «Actualizar todo»: la vía que se declara no existe tal cual, y el coste para N racks no se trata

- **Cita:**
  - D:217-221: «Usa la vía de D-13 extendida a N racks … MUTATE en **una** transacción»;
  - D:241-242: M-05, «porque consume la costura de I-55 para N racks»;
  - D:494 (R-04): «Progreso visible».
- **Lo que dicen las fuentes:**
  - V5:507-508 declara como vía la costura integrada de I-55, **por rack**.
  - En el código, `RackSiblingRedrawRun.Execute(membership, port)` (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:30-33`)
    recibe la pertenencia de **un** rack y un puerto tipado por sistema. `port.Mutate` es la transacción única de esa corrida.
  - Una transacción para N racks de sistemas distintos no es la costura «extendida»: es una orquestación nueva por encima o una
    modificación de la costura.
  - V5:32-33: el Freeze de I-55 V5 prevalece. V5:533-535: no se puede cambiar el contrato observable de la costura para Insertar
    (INV-30). V5:449-450: el Architect acordó no elevar a quien gobierna I-55 solo porque el consumo era sin cambios.
  - **El coste:** el barrido AUTH-06 «abre las entidades de todas las definiciones RackCad» (V5:459-461) y se repite por rack en la
    relectura dentro de MUTATE. Con unos 70 racks son N barridos completos dentro de una transacción. R-04 no lo mide.
  - En la fase 4 solo Push Back está integrado (D:231). Selectivo, con el registro de variables en su StaleBase (V5:475-477), y los demás
    sistemas entran después, y el plan no exige que las fases 6 y 7 repitan INV-A14-07 y 08 para sus sistemas.
- **Corrección:**
  - declarar la vía de «Actualizar todo» como una **composición aditiva** de escritores caller-owned bajo una sola transacción y un solo
    bloqueo, sin cambiar `RackSiblingRedrawRun` ni su contrato para Insertar. Si eso no se puede, M-08 frente al Freeze de I-55 y
    coordinación antes del paquete de la fase 4;
  - extender INV-39 (guardas caller-owned) a esa composición;
  - pedir en el paquete de la fase 4 un arnés con N racks que mida el coste de N relecturas y admitir una sola captura AUTH-06 compartida
    como mecánica posible, sin congelarla;
  - exigir que las fases 6, 7 y 3b extiendan las pruebas de «Actualizar todo» a sus sistemas.

### I-04 · IMPORTANTE · «Actualizar todo» no dice qué pasa con el texto pendiente conservado ni qué borradores entran según su estado

- **Cita:** D:216: «escribe en una sola operación todos los borradores con cambios de la sesión de documento activa».
- **Lo que dicen las fuentes:**
  - Con K = 1 (V5:393), solo el rack abierto tiene superficie viva.
  - Los demás borradores pueden tener **texto pendiente conservado** de una frontera no bloqueable (V5:436-437). Ese texto «no se
    compromete ni se auto-repara (INV-18)» y vuelve a su caja al rematerializar.
  - D-13 paso 1 (V5:509) compromete el texto antes de construir, y la nota prevista a ADR-0032 (V5:1222-1225) solo admite ese compromiso
    en las fronteras bloqueables.
  - El v1 no dice si esos borradores se rematerializan uno a uno, se excluyen o bloquean la operación.
  - Tampoco dice qué pasa con un borrador en Conflicto ya conocido (haría fallar siempre el todo o nada), en Desconocido (V5:531-532, que
    pide «Comprobar ahora» antes), en Error (V5:396) o en Posiblemente obsoleto.
- **Corrección:** fijar la regla en A-14 e incluirla en INV-A14-07. Una propuesta:
  - un borrador con texto pendiente conservado, o en Conflicto, Desconocido o Error, impide «Actualizar todo»;
  - la ventana lo nombra en la lista de lo que no se aplicó, sin comprometer su texto;
  - el rack abierto compromete su texto según D6, o la acción se aborta.

### I-05 · IMPORTANTE · Al quitar la preview, el v1 confunde la pestaña preview de D-08 con la preview de navegación de D-10, y deja sin final la vida de un borrador

- **Cita:** D:156: «**Desaparecen la preview y el fijado:** ya no hay nada que se reemplace».
- **Lo que dicen las fuentes:**
  - D-10 (V5:425-426): la **preview de navegación** es una lectura ligera, sin StaleBase ni AUTH-06, cuya superficie «no acepta entrada
    hasta la intención de edición». INV-27 (V5:765) protege el paso de la preview a la primera edición.
  - Un rack abierto en el área central y aún sin editar es exactamente ese estado. Si «desaparece la preview», no queda escrito.
  - D-10 (V5:440): la vida del borrador acaba «hasta cerrar la pestaña (limpia o descartada)». Sin pestañas, un borrador Limpio no muere
    nunca. Con unos 70 racks se acumulan borradores en memoria (P-07, V5:635), y el v1 solo define «Descartar cambios» con dirty (D:159).
- **Corrección:**
  - decir que el rack abierto sin intención de edición conserva la lectura ligera de D-10 e INV-27;
  - fijar el final de un borrador Limpio: por ejemplo, se libera al cambiar de rack, o al descartarlo;
  - añadir la obligación de prueba correspondiente.

### I-06 · IMPORTANTE · El valor inicial de «Seguir la selección» cambia comportamiento congelado y el v1 lo deja a un paquete

- **Cita:** D:161: «Su valor inicial lo fija el paquete de la fase 2».
- **Lo que dicen las fuentes:**
  - V5 congela que Uno(R) abre el rack: D-06 (V5:335), el criterio 3 (V5:86) y OV-04, «Seleccionar un rack en el dibujo y que el panel lo
    siga» (V5:936).
  - Con el interruptor apagado por defecto, Uno(R) «solo marca la fila de R» (D:163). Eso es un cambio de comportamiento observable
    congelado (LIFECYCLE §3, M-03), y un paquete de fase no tiene autoridad para hacerlo.
  - El prototipo trae `follow: true` (notas del redactor §3).
- **Corrección:** congelar «encendido» como valor inicial, que es el de V5 y el del prototipo. «Apagado» queda como elección del usuario
  dentro de la sesión. Así no hace falta preguntar al Owner.

### I-07 · IMPORTANTE · La fase 5 (secciones dentro de la ventana) choca con la frontera de escritura y con el brief, y no es «ligera»

- **Cita:**
  - D:115-117: «Las escrituras en el DWG que inicia la ventana pasan solo por «Actualizar» o «Actualizar todo»»;
  - D:302: «La fase 5 las pone dentro de la ventana, sin bloquear»;
  - D:458: la fase 5 depende de «1 (en paralelo)»;
  - D:498 (R-08): «Las fases 1, 2 y 5 son ligeras».
- **Lo que dicen las fuentes:**
  - **Variables:** hoy RACKVARIABLES escribe al momento (V5:215-216). El brief dice «NO LIVE MUTATION» (`:77`) y «Actualizar debe seguir
    siendo la frontera explícita de commit» (`:94`). Unas Variables dentro de la ventana necesitan un borrador de variables y una frontera
    de escritura: dependen de lo mismo que R-09 y Q-02, no solo de la fase 1.
  - **Biblioteca de diseños:** «Abrir de la biblioteca» crea e inserta un rack (`RackMainMenuWindow.xaml:134-135`). Hacerlo dentro de la
    ventana choca con el no-objetivo de Insertar (V5:75-76) y con la fase 8 (D:463, Q-04).
  - **Configuración:** escribe `settings.json` (`UserSettings.cs:9`, `:59-60`). **BOM** exporta XLSX o CSV (PKG `:123`, H-PB-33).
    D-22 (V5:702) prohíbe «archivos aparte del registro», y la excepción de C-08 solo cubre el rectángulo (D:375).
  - **El brief:** «PREPARES BUT DOES NOT IMPLEMENT … ID20 Computed Summary; … Project Variables … Do NOT implement those features here»
    (`:444-457`), que V5 incorpora a sus no-objetivos (V5:74). Implementar las secciones de Variables y BOM cambia un no-objetivo:
    `OWNER-RESERVED`. El Owner lo aprobó con sus secciones, pero el v1 no lo declara.
- **Corrección:**
  - la fase 5 entrega dentro de la ventana solo lo que no escribe el DWG;
  - Variables dentro de la ventana va con su borrador y su frontera, tras resolver Q-02, y depende de ese diseño;
  - Biblioteca, mientras no llegue la fase 8, lanza el flujo clásico para insertar;
  - D-22 recibe excepciones nombradas: los ajustes de usuario que escribe Configuración y los archivos que el usuario exporta desde BOM;
  - el no-objetivo del brief `:444-457` se declara cambiado;
  - se corrige R-08.

### I-08 · IMPORTANTE · La privacidad del índice solo se nombra: la guarda de INV-36 es por carpeta y deja de proteger cuando BOM y lista viven dentro de la ventana

- **Cita:** D:304-305 (la sección «consume las autoridades existentes de RACKLISTA y RACKBOMTOTAL, **no** el índice») y D:500 (R-10).
- **Lo que dicen las fuentes:**
  - INV-36 (V5:774) se verifica con una guarda de fuente: «ningún archivo **fuera de las carpetas del panel** (métricas, `ProjectSummary`,
    BOM, `TotalRacks`, agregación, listas, comandos) referencia sus tipos».
  - Si el código de las secciones BOM, lista y Variables vive en las carpetas de la ventana, esa guarda ya no le impide consumir el índice.
    La regla del v1 queda sin oráculo.
  - El v1 tampoco cita la frontera del Master: «Métricas, providers, población y agregación → I-63» (V5:260, MASTER-I63-I64-02 §3).
    Alojar BOM y lista solo está previsto como consumidor o superficie: «`ProjectSummary` (ID20) podrá alojarse después en el panel como
    consumidor o superficie» (V5:312-313, D-23).
- **Corrección:**
  - un invariante nuevo, con su RED, por el que los tipos del índice, de `NavigationTargets` y de los hechos neutrales no los referencia
    ningún archivo de las secciones, aunque esté dentro de la ventana;
  - citar D-05 `:312-313` y MASTER-I63-I64-02 §3 como la base de la sección «BOM y lista de racks».

### I-09 · IMPORTANTE · C-08: la evidencia de perfil no se hereda, falta una cláusula del brief y el archivo de ajustes es compartido

- **Cita:** D:373-374: «Así D-01 `:122` queda literal y el diff de perfil (S1-11) sigue valiendo»; D:385: «S1-11 sigue».
- **Lo que dicen las fuentes:**
  - S1-11 y el «DIFF CLEAN» de Smoke-1 certifican un build sin persistencia (`I-64.yml`, `f1_t3b_host_ui_c1_status`: «sin GUID,
    persistencia, registro, perfil»).
  - Un build que guarda y aplica el rectángulo (con `Dock`, `DeviceIndependentSize` y la ubicación del `PaletteSet`) necesita su propio
    diff, con el procedimiento F1-T3C, en el smoke de la fase 1. La evidencia es por SHA exacto (LIFECYCLE §8 `:250-253`).
  - Falta el brief `:474` («session restore across application restarts unless trivial», OUT OF SCOPE). El v1 solo cita `:371`.
  - `settings.json` comparte `BlockLibraryPath`, `DesignLibraryPath` y `SelectiveTargetFondos` (`UserSettings.cs:12-27`). Una paleta no
    modal que guarda su copia puede pisar lo que otra ventana cambió mientras tanto.
- **Corrección:**
  - S1-11 se repite en el smoke de la fase 1, con el procedimiento F1-T3C y el acuse de I-52;
  - se cita el brief `:474`;
  - INV-A14-03 añade «guardar el rectángulo no altera los demás ajustes del archivo»;
  - se aclara si el estado acoplado o flotante y el lado de acoplamiento forman parte de «la posición», como mecánica no congelada.

### I-10 · IMPORTANTE · El objetivo «toda la operación de RackCad» no tiene fases ni criterio que lo verifiquen

- **Cita:** D:110-111 («**toda la operación de RackCad** se hace desde esa ventana») y D:126 (el criterio nuevo: «todo el contenido del
  menú actual está en ella»).
- **Lo que dicen las fuentes:**
  - El Plugin registra además RACKPROPIEDADES, RACKLAYOUT, RACKRELLENAR, RACKDUPLICAR, RACKRAMPAQUETE, RACKAYUDA, QUICKCABECERA y
    QUICKCAMA (el censo de `CommandMethod`).
  - Ninguno está en el menú ni en las cuatro secciones, ni tiene fase. READY no podría verificar el objetivo tal como está escrito
    (LIFECYCLE §5: un elemento congelable «no verificable» es REQUIRED).
- **Corrección:** definir el objetivo verificable como el contenido del menú de RACKCAD, las cuatro secciones y la lista de racks. Los demás
  comandos siguen como comandos y su destino se ve con la lista de la fase 9, que el Owner ya dejó para después. No hace falta otra pregunta.

### I-11 · IMPORTANTE · El camino de aprobación versiona A-14 y sus fuentes después de la revisión del Architect

- **Cita:** D:558-559 (el Architect revisa «el objeto exacto (commit)») frente a D:564-565 (paso 6: A-14, las decisiones del Owner y la
  replaneación se versionan después de la firma); D:598 («append-only desde su commit»).
- **Lo que dicen las fuentes:**
  - En I-64, cada A-n se versiona PROPOSED y se revisa sobre su commit. Si la revisión da CHANGES REQUIRED, la corrección es la A-n
    siguiente: A-12 → A-13 (A-13 `:10-12`, `:27-28`), A-8 → A-11, y A-7 «sin editar» (`I-64.yml`, `f1_a7_status`).
  - LIFECYCLE §5 (`:161-162`): el Architect recibe las «fuentes consumidas». Hoy el registro del Owner y la replaneación solo existen fuera
    del repo.
- **Corrección:**
  - fijar el orden: la revisión independiente y la v2; después versionar la v2 como `docs/initiatives/I-64-A-14.md` PROPOSED **junto con**
    el registro literal del Owner y la replaneación (Q-11 se resuelve antes); después la revisión del Architect de ese commit;
  - si el Architect pide cambios, la corrección es A-15;
  - después Coordinator AGREED, la firma del Owner y la activación en el estado.

### Menores

| ID | Cita | Fuente | Corrección |
|---|---|---|---|
| M-01 | D:479 y D:576: «AUTHORIZED / WAITING FOR F4B» (OD `:269`) | El literal está en OD `:293`. `:269` dice «F5-A remains authorized but WAITING» | Citar `:269` y `:293` |
| M-02 | D:229: «Volver a leer» como nombre de D-12 | V5:488 lo llama «Descartar y recargar» | Usar el nombre de V5, o decir que es el rótulo del producto |
| M-03 | D:76: el registro de aclaraciones «solo recoge diferencias de **mecanismo**» | ICL `:3` registra las diferencias entre lo implementado y el Freeze; la precedencia solo vale para el mecanismo. IC-11 introdujo una fase entera | Reescribir: un cambio de resultado no se puede hacer por IC |
| M-04 | D:28-29: «F4B no se reabre ni cambia … ni D-PB-01..D-PB-10» y, a continuación, «D-PB-01 … pasa a ser un paso intermedio» | Es contradictorio | «D-PB-01 rige F4B sin cambio; A-14 la sustituye desde el cierre de la fase 3» |
| M-05 | D:36-37: la firma del Owner, sin el texto exacto | Patrón de A-11 a A-13 (A-13 `:34`: «Exact choice to be requested») | Escribir el texto de la firma y nombrar lo `OWNER-RESERVED` (C-01, C-02, C-04, C-05, C-06, C-08, D-PB-01 y OD `:37`, R2-01, R2-02, OV-08, 09, 11, 14, 17 y 20, la autorización de F5-A). Así Q-05 y Q-06 no necesitan preguntas aparte |
| M-06 | D:335: «La ventana deja de ser una paleta de 320 px» | C-09 (D:389): sigue siendo un `PaletteSet` | «Deja de medir 320 px» |
| M-07 | D:162: «Uno(R) abre R … si la frontera lo permite» | D-10 (V5:434): las fronteras bloqueables son las que inicia el usuario dentro del panel. Una selección en AutoCAD no lo es. D:162-163 ya deja el rack abierto si hay texto pendiente | Quitar «si la frontera lo permite»: sin texto pendiente, abre; con texto pendiente, marca |
| M-08 | D:225-226: la lista de «cada rack no aplicado con su causa» | Un rack sin fallo propio no se aplica porque falló otro, y no tiene causa definida | Añadir la causa «No aplicado: falló otro rack del conjunto» |
| M-09 | D:254: «excluye cualquier otro Actualizar **de la sesión**» | V5:545 no lo limita a la sesión | «Ningún otro Actualizar mientras dura» |
| M-10 | D:437: OV-A14-01..07, con autoridad «Coordinator» | LIFECYCLE §6 (`:204-205`): el Coordinator solo añade OV de comportamiento **ya** congelado. Estos los congela A-14 | Autoridad: la de A-14 |
| M-11 | D:124 y D:484: SP-03 «con un dibujo real de unos 70 racks» | V5:909: «sobre los dibujos más grandes del Owner» | «Los más grandes del Owner, al menos uno de unos 70 racks» |
| M-12 | D:88: «Aprueba … la tabla §3» | El registro literal pregunta solo por §3 (`owner-a14-decisions.md:8`). Las fases (§4) quedan aprobadas por lectura de «los cambios al plan» | Decir que es lectura de la sesión |
| M-13 | D:311 y D:458: «región de variables de proyecto» de D-23 | V5:708-709: son regiones de inspector «en la pestaña y en Selección (N)», no secciones | Decir que la sección es nueva y que D-23 solo es su antecedente |
| M-14 | D:565: el prototipo, «sin datos de ejemplo que parezcan rutas locales» | El repo es público. El prototipo y la evidencia de Smoke-PERF con un dibujo real pueden llevar nombres de racks o de proyectos de clientes (precedente: `s2-selection.local.json` no se custodia, PB-T5 v2.4 `:14`) | Redactar también nombres de proyectos y de clientes |

## 4. Comprobaciones que pasaron

- La identidad del Freeze y la imagen `55e7ca63`, que es antepasada de HEAD con el mismo blob, como hizo A-7.
- Las citas de V5 por línea. Cotejadas todas: :71-72, :75-80, :82-95, :120-147, :151, :173-174, :186, :215-216, :241-243, :284-297, :309-311,
  :317-325, :335-339, :382-397, :405-406, :427-428, :434-435, :451, :475-477, :487-498, :502-503, :521-532, :536-539, :545, :562-563,
  :577-582, :586-593, :679-683, :702-704, :708-712, :737-778, :793-796, :803-805, :826-829, :927-960, :995-996, :1219-1221.
- El brief (`:126-132`, `:263`, `:371`, `:377`, `:391`, `:438`, `:475`, `:476`), [OD] (`:20`, `:22-49`, `:25-26`, `:34`, `:36`, `:37`,
  `:76-78`, `:130-155`), el paquete de F4B (`:59`, `:60`, `:86`, `:174`), IC-11 (`:193-206`), SPB-12 y SPB-13 (`:260-261`), D-PB-10,
  F4B-E01, E02 y E62, el código de RACKCAD y RK, el menú, `ApplyInitialPlacement`, `UserSettings` y AGENTS `:342-343`.
- Los estados de la tabla de correspondencia (`I-64.yml`: F1 y Smoke-1 PASS; F2, F3 y F4 PASS WITH LIMITATIONS; dos rebases).
- F4B y PB-T5: A-14 no toca el contrato ni el Smoke-PB en curso. SPB-03 (RACKCAD → el menú, PB-T5 v2.4 `:241`) sigue valiendo porque
  RACKCAD cambia en la fase 1, después de F4B. Hay que decirlo en la v2: cada cambio rige desde el paquete de su fase.
- Las decisiones del Owner están bien atribuidas, la respuesta literal es exacta y RACKPANEL está como decidido. El v1 no se atribuye
  ninguna decisión que el Owner no tomara.
- No hay rutas locales, nombres, correos ni datos de clientes en el texto.

## 5. Contraste con las notas del redactor (leídas después)

- Las notas coinciden en lo que el v1 sí trata: K-01..K-15, el Freeze rebasado, la celda del Architect medida sobre otra configuración.
- Lo que las notas no vieron, y esta revisión añade, es I-01 a I-11:
  - lo que la lista «Sin cambio» deja fuera, y los anexos A y B;
  - las invariantes y los OV que faltan;
  - la vía de N racks frente a la costura de I-55, y el coste de AUTH-06;
  - el texto pendiente conservado al «Actualizar todo»;
  - la preview de navegación y la vida del borrador;
  - el valor inicial de «Seguir la selección» frente a OV-04;
  - las escrituras de la fase 5 frente al brief;
  - la guarda de INV-36 por carpeta;
  - la repetición de S1-11;
  - el alcance de «toda la operación»;
  - el orden de versionado.
- Las notas dicen que el valor inicial de «Seguir la selección» lo «nadie lo decidió». Es cierto, pero V5 ya congela el comportamiento
  equivalente a «encendido» (I-06), así que el valor no queda libre para el paquete.
- Ninguna afirmación de las notas resultó falsa.

## 6. Preguntas que de verdad deben ir al Owner

Solo dos. Las demás se resuelven con las fuentes, ya están contestadas o van en la firma.

**P-1 (sustituye a Q-02): Variables dentro de la ventana y «Actualizar todo».** El Owner aprobó que «Actualizar todo» escriba «racks y
variables con cambios». Hoy RACKVARIABLES escribe al momento, y el brief exige «NO LIVE MUTATION».

| Opción | Efecto |
|---|---|
| a) La fase 4 escribe solo racks. Las variables entran en «Actualizar todo» cuando la sección Variables esté en la ventana, como borrador, con su propio paquete (y su A-n si cambia D-10 o D-12) | La fase 4 no espera. Variables se diseña con R-09 resuelto |
| b) La fase 4 espera a la sección Variables y sale con racks y variables | Una fase 4 más grande y más tarde |
| c) Variables, dentro de la ventana, sigue escribiendo al momento, como hoy | Contradice «NO LIVE MUTATION» y el objetivo de C-01 |

**Recomendada: a).**

**P-2 (Q-10): ¿Cuándo llega «Abrir en editor clásico»?** IC-03 (decisión C-F3-04 del Owner) lo deja para F5, que ahora es la fase 6. Desde
la fase 2, la lista muestra todos los racks, también los de sistemas no integrados, que sin esa acción solo tendrían el resumen hasta la
fase 6.

| Opción | Efecto |
|---|---|
| a) En la fase 2. La fase 1 ya mide cómo lanzar flujos clásicos desde la ventana (R-13) | Cada rack de la lista se puede editar ya, en su ventana clásica |
| b) Se queda en la fase 6, como dice IC-03 | De la fase 2 a la 6, los racks no integrados solo tienen el resumen |

**Recomendada: a).**

**Lo que no necesita pregunta:**

| Pregunta | Por qué |
|---|---|
| Q-01, la lista de retiro | Ya decidido: se ve antes de la fase 9 |
| Q-04, la fase 8 | Se decide con su propia A-n, no ahora |
| Q-05, la autorización de F5-A | Va en el texto de la firma (M-05) |
| Q-06, la sustitución de OV | Va en el texto de la firma (M-05), con OV-11, OV-17 y OV-20 |
| Q-03, Q-07, Q-08 y Q-09 | Contestadas |
| Q-11 y Q-12 | Son del Coordinator. Q-11, antes de la revisión del Architect (I-11) |
| Los comandos de creación con el diseño nuevo | Se deduce de Q-03 («un solo editor por sistema»; la misma ventana): se registra en la v2 y en la firma |
| El valor inicial de «Seguir la selección» | Encendido, como en V5 (I-06) |
| El alcance de «toda la operación» | Se acota a lo aprobado (I-10) |
| La frontera de cada tramo | Se decide por tramo (R2-02) |
