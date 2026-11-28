# I-64 Amendment A-14 — Replaneación hacia la ventana RackCad sin bloqueo (borrador v2.2)

```text
Amendment: A-14

Initiative / Unit: I-64

Freeze:
commit = 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311 (imagen en la rama actual tras los rebases: 55e7ca6337fef6dcc14f5c40bdd55daf88d78cbf)
path = docs/initiatives/I-64-proposal-v5.md
blob = dc1924ff5a73a4e529fbc5f995621d29351a091b

Applies-to: all (LIFECYCLE §6; precedente: las A-n de I-63, «Applies-to = all», p. ej.
docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md). Alcanza a la unidad I-64 (tramo 1) y a todas las unidades de I-64 de
los tramos siguientes (C-12), tengan el nombre que tengan.

Amends (Freeze V5), con el delta exacto en §4:
§0 (Actualizar, NavigationTargets, Panel RackCad); §1 (objetivo, no-objetivos y criterios 3, 5, 6, 10, 11 y 12); §2 (cierre y
aceptación de Actualizar); D-01 (solo el comando, :124); D-02; D-03; D-04 (ámbito de mutación propia de «Actualizar todo»); D-05
(texto de pestañas); D-06; D-07 (texto de pestañas); D-08; D-09; D-10 (rack abierto, preview de navegación y vida del borrador);
D-13 (solo «Actualizar todo» y :545); D-14; D-15 (orden, realineación, caracterización por sistema rediseñado); D-16; D-17 (alcance
del veto); D-18 (escenarios 5 a 8 y uno nuevo); D-20; D-21; D-22; D-23; §10; §11; §12.5 (SP-02 y SP-03); §12.6 (asignación por
tramo; OV-08, OV-09, OV-11, OV-14, OV-17 y OV-20; escenarios nuevos); §14 (M-02 y M-03); anexo A (decisiones 1, 6, 8, 10 y 11 y
consecuencias); anexo B (las dos notas). Del brief (que es del Owner), las cláusulas que cita C-01.
Sin cambio: D-01 en el host, la contingencia (b), la obligación de foco y el perfil; D-11; D-12; D-19; §12.1.

Corrects / supersedes:
Ningún texto de A-n anterior cambia. Un solo cambio de alcance:
- A-7 (docs/initiatives/I-64-A-7.md): su "Applies-to: I-64" se amplía a las unidades de I-64 de los tramos 2 y siguientes (C-12), solo
  en su alcance. Así INV-24 y S2-16..S2-22 rigen en las fases de editores de esos tramos (C-13). A-7 sigue activa en la unidad I-64.
- A-1..A-6 y A-8..A-13 (puentes de protocolo) no cambian de alcance: siguen siendo de la unidad I-64. Si una unidad nueva necesitara un
  puente, se pide por su propia A-n, con el Owner y el dueño del protocolo (§11, pregunta P-1).

Relation to F4B (IC-11; F4B-E01):
D-PB-01 rige F4B sin cambio, igual que D-PB-02..D-PB-10, el paquete v3.1, el contrato de PB-T5 y el Smoke-PB en curso. Desde el
cierre de la fase 3, A-14 sustituye SOLO las frases de D-PB-01 sobre la ventana modal (C-08); el resto de D-PB-01 sigue.

Authority (propuesta del borrador; la confirma el Coordinator, LIFECYCLE §3 y §6):
Owner + Architect + Coordinator. Owner por lo OWNER-RESERVED (§11); Architect + Coordinator por las M materiales (§9).

Owner:
Decisiones del 2026-11-23 (tres rondas y RACKPANEL): TOMADAS (registro literal; ruta en el repo PENDING, Q-C1).
Firma del texto exacto: PENDING; la elección que se pide está en §11 (con su lista en lenguaje llano); se pide después de Architect
AGREED y Coordinator AGREED; nunca se infiere.

Architect:
REQUIRED (M material); una revisión formal de solo lectura del commit exacto PENDING; celda y orden PENDING (Q-C2).

Coordinator:
PENDING (materialidad, autoridad, orden de revisión del Architect, AGREED).

Status:
PROPOSED / NOT ACTIVE. Hasta Owner SATISFIED, Architect AGREED y Coordinator AGREED no se emite ningún paquete de fase de A-14.
```

**Fuentes:** el Freeze V5 (citado «V5 `:n`», línea del blob de arriba); el [brief del Owner](I-64-owner-brief.txt) («brief `:n`»); la
disposición del Owner y del Coordinator de F4B (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`,
«OD `:n`»); el [paquete de F4B](I-64-f4b-push-back-completo-package.md) v3.1 («PKG `:n`»); las
[aclaraciones de implementación](I-64-implementation-clarifications.md) («ICL `:n`»); [LIFECYCLE](../INITIATIVE_LIFECYCLE.md) y
[WORKFLOW](../WORKFLOW.md); el registro literal de las decisiones del Owner del 2026-11-23; la replaneación «Replaneación de I-64»
(2026-11-23); el prototipo «RackCad tipo navegador» v3, aprobado por el Owner y solo descriptivo. Las tres últimas se versionan antes de
la revisión del Architect, en la lista única del paso 3 de §10.

## 1. Motivo

**La visión del Owner (2026-11-23):** todo RackCad sin bloquear AutoCAD, desde una ventana tipo navegador: una columna lateral con las
secciones y la lista de los racks del dibujo (con buscador y filtros, porque un dibujo puede tener hasta unos 70 racks), un rack
abierto en el centro, «Actualizar» por rack y «Actualizar todo».

**El hecho que la hace una realineación y no un giro:**
- V5 ya decía que los editores serían editables en el panel: «**Push Back editable en el panel**» (F4, `:793`), «Selectivo, Dinámico y
  Cantilever … editables en el panel» (F5-A, `:794`) y «Cabecera y Cama … editables en el panel» (F5-B, `:795`). D-15, punto 1, dice
  cómo: «el contenido del editor (el shell, que es un `Control`) se separa de su `Window`. La ventana clásica envuelve la misma
  superficie» (`:562-563`).
- F4B, por disposición del Owner y del Coordinator, abrió «Editar todo» como ventana **modal**: «opens the existing Push Back editor
  as a modal/full-size editing surface» (OD `:25-26`); «the drawing cannot be edited while the modal editor is open» (OD `:34`); «Do
  not try to fit the current ~1120 px editor inside the ~320 px panel» (OD `:37`). El paquete lo fija en OB-01.4 (PKG `:174`), con el
  hallazgo H-PB-10 (PKG `:86`). Cita D-15 punto 1 como autoridad (PKG `:60`), pero [OD] manda dentro del paquete (PKG `:59`), y ni el
  paquete ni IC-11 (ICL `:193-206`) declaran la diferencia.
- El Smoke-PB lo certifica: SPB-12 («La paleta no responde; al cerrar la ventana, sí») y SPB-13 («Nada llega») del paquete de PB-T5
  v2.4 (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md:260-261`).

**Por qué es una A-n y no una IC:** el registro de aclaraciones anota las diferencias entre lo implementado y la redacción congelada, y
solo prevalece sobre el Freeze en el **mecanismo** (ICL `:3`). IC-11 añadió F4B porque, según [OD], F4B «extends the proven Workspace
architecture; it does not reopen F4» (OD `:20`); la diferencia con D-15 punto 1 que se describe arriba no se declaró entonces, y A-14 la
declara ahora. A-14 cambia el objetivo, no-objetivos y resultados congelados: un cambio de alcance o de no-objetivos es
`OWNER-RESERVED` (LIFECYCLE §3), y todo cambio a un elemento congelado usa una A-n (LIFECYCLE §6).

## 2. Decisiones del Owner (2026-11-23)

Registro literal: el archivo de decisiones del 2026-11-23 (Q-C1). Respuesta a la primera ronda (literal): «Sí, apruebo los cambios al
plan, la ventana recuerda, rackcad abre la ventana y la lista la vemos después».

| # | Decisión | Efecto en A-14 |
|---|---|---|
| 1 | Aprueba los cambios de la tabla §3 de la replaneación. Las fases de su §4 quedan aprobadas por lectura de la sesión de «los cambios al plan»: la pregunta solo citaba el §3 | C-01 a C-08 |
| 2 | La ventana recuerda su tamaño y su posición. **Lectura de la sesión, que el Owner confirma al firmar:** «posición» incluye si estaba acoplada o flotante y en qué lado | C-10 |
| 3 | RACKCAD abre la ventana | C-07 |
| 4 | La lista de comandos a retirar se ve después | Antes de la fase 9; A-14 no retira ninguno |
| 5 | «RACKPANEL puede quedarse por ahora» | RACKPANEL se conserva; su futuro, con la lista de retiro (C-07) |
| 6 | Q-03: «Adopta el diseño nuevo»: RACKEDITAR abre la misma pantalla rediseñada en una ventana aparte; un solo editor por sistema; «El cambio se declara cuando ocurra». **Deducciones de la sesión, que el Owner confirma al firmar** (C-09): que alcanza también a los comandos de creación del sistema, y que la aprobación del prototipo forma parte de la decisión que pide ADR-0029 D13 | C-09 |
| 7 | Q-09: «Por tramos»: integrar en `main` en hitos que elige el Owner, cada uno con su candidato completo y su validación | C-12 |
| 8 | Q-07: «RK igual; menú viejo desde la ventana»: RK abre la ventana igual que RACKCAD; hasta la fase 9, la ventana tiene una entrada que abre el menú de hoy como respaldo; ningún comando nuevo | C-07 |
| 9 | Q-08: «Ahora no; decidir después»: «Abrir RackCad al iniciar AutoCAD» queda fuera de A-14 y se decide con el diseño de Configuración | C-11, §8 |
| 10 | Q-02: «Primero racks, luego variables»: la fase 4 entrega «Actualizar todo» solo para racks; cuando la sección Variables llegue a la ventana, sus cambios serán un borrador más que «Actualizar todo» también escribe | C-05, C-11 |
| 11 | Q-10: «Sí, en la fase 2»: desde la fase 2, cualquier rack de la lista se abre en su editor de hoy, aunque ese editor siga bloqueando hasta que su sistema se migre | C-02, fase 2 |

**Decisiones previas del mismo día que A-14 recoge:** la columna lateral, sin pestañas arriba, con buscador y filtro; «Actualizar» por
rack y «Actualizar todo» fuera, todo o nada; «Nuevo» abre el sistema elegido, por ahora con su ventana clásica; las secciones Variables
de RackCad, BOM y lista de racks, Biblioteca de diseños y Configuración («se eliminaran muchos de los comandos pero obvio seguirán
existiendo rackproyectar o así»); editores «Actual primero, simplificar después»; «Diseñar antes de migrar»; terminar el Smoke-PB de
F4B cuando el Owner tenga AutoCAD libre.

**Vigencia:** cada cambio de A-14 rige para el producto desde la fase que lo implementa; hasta entonces rige V5 tal cual. Por ejemplo,
SPB-03 de PB-T5 («`RACKCAD` → El menú», PB-T5 v2.4 `:241`) sigue valiendo, porque RACKCAD cambia en la fase 1, después de F4B.

## 3. Cambios

Cada cambio: texto vigente → texto nuevo → autoridad → riesgo que V5 quería evitar y su tratamiento. El delta por cláusula está en §4.

### C-01. §1 Objetivo y alcance

**Vigente** (V5 `:71-72`): «pasar de ventanas modales a un panel persistente y modeless dentro de AutoCAD, con sincronización de contexto
en los dos sentidos, borradores por rack y Actualizar como única frontera de escritura».

**Nuevo:**
- Lo mismo, y además la operación de RackCad se hace desde esa ventana (la «ventana RackCad»): tipo navegador, acoplable o flotante,
  sin bloquear AutoCAD.
- **Alcance verificable** de «la operación de RackCad»: el contenido del menú de RACKCAD, las cuatro secciones (Variables, BOM y lista de
  racks, Biblioteca de diseños, Configuración), «Nuevo» y la lista de los racks del dibujo con su rack abierto. Los demás comandos
  (RACKPROPIEDADES, RACKLAYOUT, RACKRELLENAR, RACKDUPLICAR, RACKRAMPAQUETE, RACKAYUDA, RACKSECCION, QUICKCABECERA, QUICKCAMA y los de
  creación por sistema) siguen como comandos; su destino se ve con la lista de la fase 9.
- Las escrituras en el DWG que inicia la ventana pasan solo por «Actualizar» o «Actualizar todo». La excepción son los flujos clásicos
  que la ventana lanza mientras se migran («Nuevo», «Abrir en editor clásico», la entrada del menú de hoy y las secciones que aún
  abren su ventana actual): escriben como hoy.

**Cláusulas del brief que A-14 cambia de forma explícita.** El brief es del Owner: cambiarlo es `OWNER-RESERVED` y lo hace la decisión
del Owner que se cita en cada caso:
- las pestañas tipo navegador (`:126-132`), por C-02;
- «global batch Apply unless explicitly frozen» (`:475`), por C-05;
- «session restore across application restarts unless trivial» (`:474`) y la restauración entre arranques (`:371`), solo para el
  tamaño y la posición, por C-10;
- «PREPARES BUT DOES NOT IMPLEMENT … Project Variables … Do NOT implement those features here» (`:444-457`), para la sección Variables,
  por C-11;
- «Existing standalone RACKEDITAR behavior should remain usable during migration unless Freeze explicitly changes it» (`:393-394`), por
  C-09. Solo esta cláusula prevé por sí misma que el Freeze la cambie.

**Criterios de éxito** (V5 `:82-95`):

| # | Cambio |
|---|---|
| 3 | Gates: F1 y fase 2 (antes F1 y F6) |
| 5 | Mecanismo: D-08 de C-02; gates: fases 2, 3, 6 y 7 |
| 6 | «entre pestañas» → «entre racks» |
| 10 | «por pestaña» → «por rack abierto o fila de la lista» |
| 11 | Se añaden la fase 2 (arnés con al menos 70 racks) y la fase 4 (arnés de N racks); Smoke-PERF por tramo (C-12) |
| 12 | ID3 se clasifica en el READY del último tramo (C-05, C-12) |
| nuevo | RACKCAD y RK abren la ventana y todo el contenido del menú está en ella (fase 1) |
| nuevo | «Actualizar todo»: todo o nada y un solo paso de Deshacer (fase 4) |
| nuevo | Las secciones funcionan dentro de la ventana sin bloquear (fase 5), salvo Variables, que llega con su A-n (C-11), la inserción desde la Biblioteca, que sigue por el flujo clásico hasta la A-n de la fase 8, y los subdiálogos modales de R-05 |
| nuevo | La ventana recuerda su tamaño y su posición (fase 1) |

**Autoridad:** `OWNER-RESERVED` (LIFECYCLE §3) + Architect + Coordinator (M-01, M-07).

**Riesgo que V5 quería evitar:** reescribir la arquitectura (brief `:476`) y todos los editores a la vez (brief `:377`, `:391`).
**Tratamiento:** se amplía lo cerrado (F1 a F4B) sin reabrirlo; los editores entran de uno en uno con el contrato de D-15; cada fase tiene
paquete y orden (§6).

### C-02. D-08: de pestañas a la columna lateral (con D-03, D-06, D-07, D-09 y D-10)

**Vigente:** D-08 (`:382-397`): registro ligero por pestaña; preview, como máximo una por sesión; «La primera pulsación en un campo fija
la preview» (`:388`); «Cerrar: sin dirty se cierra sin preguntar; con dirty … pregunta Descartar o Cancelar» (`:390`); estado Error
(`:396`); un sistema no integrado da una pestaña de resumen con «Abrir en editor clásico» (`:397`). D-06 (`:335-336`): «Uno(R): activa
la pestaña de R si existe; si no, crea o reemplaza la preview». D-10 (`:425-426`, `:440`).

**Nuevo:**
- **Sin tira de pestañas.** La navegación es la lista lateral de los racks del dibujo: el navegador de F6 (D-05 `:309-311`), agrupado por
  sistema, con buscador por nombre, filtro por sistema y «solo con cambios sin escribir». El estado de cada fila es el de D-05
  (`:317-325`).
- **Un rack abierto a la vez** en el área central, en lugar de la pestaña activa. Abrir otro rack desde la ventana es una frontera
  **bloqueable** de D-10 (`:434-435`): el texto pendiente se compromete según ADR-0032 D6 o la acción se aborta.
- **La pestaña preview de D-08 desaparece; la preview de navegación de D-10 se conserva.** Un rack abierto sin intención de edición
  muestra la lectura ligera (sin StaleBase ni AUTH-06) y su superficie no acepta entrada hasta la intención de edición (`:425-428`);
  INV-27 sigue. La primera pulsación en un campo, «Editar» o el doble clic siguen siendo la intención de edición.
- **Vida del borrador** (D-10 `:440`): «hasta cerrar la pestaña (limpia o descartada)» pasa a «hasta que el rack deja de estar abierto
  estando Limpio, o hasta que se descarta». Lo demás sigue: retirarse la sesión (D-17) o salir de AutoCAD. Es la misma regla que cerrar
  una pestaña limpia. Los borradores con cambios siguen vivos al cambiar de rack: un borrador dirty sigue vivo al dejar el rack, y su
  fila lo muestra. No se sale de un rack con texto pendiente: la frontera bloqueable lo compromete o aborta el cambio.
- **«Cerrar pestaña» pasa a «Descartar cambios»** del rack abierto: pregunta si hay dirty y nunca escribe el DWG (INV-05, INV-09).
- **«Seguir la selección de AutoCAD»**, interruptor por sesión de documento, **encendido** al crear la sesión: es el comportamiento que V5
  congela (D-06 `:335`; criterio 3; OV-04).
  - Encendido: Uno(R) abre R en el área central. Con texto pendiente en el rack abierto, ese rack se queda y la fila de R se marca, como
    en D-06 (`:335-336`).
  - Apagado, por elección del usuario: Uno(R) solo marca la fila de R.
  - Selección (N) no abre racks ni borradores (D-09, INV-34). Ningún evento escribe selección ni vista (D-06 `:339`, INV-03).
- Los registros ligeros que D-08 pedía por pestaña pasan a ser por fila. K = 1 superficie viva (P-06), sin cambio.
- **«Abrir en editor clásico»** (decisión Q-10): desde la fase 2, para cualquier rack de la lista, ligado a su sesión (D-03) y con el
  aviso de D-16 (`:590-591`) si hay borrador. **Limitación declarada:** el editor clásico sigue siendo modal y bloquea AutoCAD hasta que
  su sistema se migre. IC-03 (que lo dejaba para F5) queda sustituida por esta decisión.
- Un sistema aún no integrado muestra en el área central el resumen y «Abrir en editor clásico».

**Autoridad:** `OWNER-RESERVED` (brief `:126-132`; IC-03 venía de la decisión C-F3-04 del Owner) + Architect + Coordinator (M-03; M-04,
por la forma nueva del cierre con dirty).

**Riesgo que V5 quería evitar:** perder borradores o texto al navegar (PV2-14, INV-04); abrir N editores (brief `:263`); que la selección
cambie lo que se edita; acumular borradores en memoria (P-07). **Tratamiento:** la frontera bloqueable al cambiar de rack; Selección (N)
sin abrir; el interruptor; la vida del borrador Limpio; K = 1; el eco de D-07 sin cambio.

### C-03. La palabra «pestaña» en las cláusulas que no cambian de semántica; F3 y sus obligaciones

- **Regla de lectura**, para obligaciones futuras: en las cláusulas de V5 que A-14 no cambia en su semántica, «pestaña» o «pestaña
  activa» se lee «rack abierto»; «pestañas» como conjunto se lee «racks de la lista»; «registro ligero por pestaña» se lee «por fila»;
  «la pestaña preview» desaparece (C-02); «preview de navegación» se conserva.
- **Precedencia:** donde §4 o §5 dan a una línea un delta propio, ese delta prevalece sobre la regla de lectura.
- **Líneas de V5 a las que se aplica la regla** (lista cerrada): `:56`, `:89`, `:93`, `:105`, `:155`, `:186`, `:199`, `:234`, `:259`,
  `:299`, `:325`, `:337-338`, `:349`, `:352`, `:365`, `:367`, `:371`, `:380`, `:394-397`, `:402`, `:404`, `:434`, `:460`, `:545`,
  `:549-551`, `:598-599`, `:708`, `:740-743`, `:745`, `:747`, `:753`, `:756`, `:763`, `:772`, `:775-776`. Varias tienen además delta
  propio en §4 o §5, que prevalece. Las que solo tienen delta propio: `:335-336`, `:382-393`, `:440`, `:648-649`, `:659`, `:908` y
  `:940-952`.
- **Historia que no se reescribe:** los gates cerrados (F2 `:791`, F3 `:792`, §13 `:975`), los hitos ejecutados (SN-06 `:871`, SN-10
  `:875`, S2-01 `:881`, S2-02 `:882`, S2-08 `:888`) y las tablas de disposición de §18 a §21 (por ejemplo `:1078`, `:1084`, `:1085`,
  `:1123`, `:1142`, `:1144`) describen lo que se certificó o se dispuso entonces.
- **F3 no se reabre.** La fase 2 retira resultados que F3 certificó:
  - la regla de preview de INV-04;
  - la tira de pestañas y el cierre de pestaña (INV-09);
  - las pruebas de INV-02, INV-15 e INV-38 que se formulan con pestañas;
  - los escenarios 5 («cambio de pestaña») y 6 («10 pestañas ligeras»).

  Sus pruebas **no se editan** (ADR-0029 D13): se conservan intactas, marcadas `Skip` con un motivo que nombra un **registro declarado de
  sustitución** de la fase 2, y el comportamiento nuevo vive en pruebas propias. Es el patrón de
  `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (SUS-nn). El registro enumera cada prueba sustituida.
- **Obligaciones de IC-02 e IC-05** (ICL `:53`, `:89`): «SN-06c y la parte de pestañas de SN-10» se quedan sin objeto. Las sustituyen el
  eco con el rack abierto y la igualdad de objetivos desde el contexto, el rack abierto y la lista (INV-38), en la OV del tramo que
  entregue la fase 2. Las demás obligaciones de IC-02 e IC-05 siguen.

**Autoridad:** con C-02; la sustitución de las obligaciones de IC-02 e IC-05 (ratificación del Owner) es `OWNER-RESERVED`.

### C-04. §11: F6 sube

**Vigente** (V5 `:796`): F6, con entrada F2 y F3, criterios INV-16 e INV-17 en el índice, INV-34 a INV-38 y los arneses de los escenarios
7 y 8; sexto en el orden.

**Nuevo:** F6 pasa a la **fase 2**. Conserva su resultado y sus criterios y suma los de C-02 y C-03. Su entrada se cumple (F2 y F3
cerradas), más la fase 1.

**Autoridad:** la resecuenciación sola es Coordinator-only (LIFECYCLE §6); lo que cambia su resultado va con C-02.

**Riesgo que V5 quería evitar:** el coste de una enumeración global antes de tiempo (D-05 `:296-297`). **Tratamiento:** el refresco bajo
demanda solo con el panel visible (D-05 `:309-311`, P-05), la privacidad del índice (D-05 `:284-295`, INV-36) y el arnés con al menos 70
racks.

### C-05. D-09 y D-20: «Actualizar todo»

**Vigente:**
- D-09 (`:405-406`): «… para un futuro borrador por lotes (residual de ID3, D-20). No se implementa edición de propiedades comunes ni un
  Actualizar por lotes.»
- No-objetivo (`:78`): «Edición por lotes: residual de ID3 (D-20).»
- D-20 (`:679-683`): el residual de ID3 es «la edición por lotes del brief (propiedades comunes, validación conjunta y mutación atómica de
  varios racks) **y** todo sistema que no quede integrado»; se clasifica en READY; la esperada es `PARTIAL` y la acepta el Owner.

**Por qué V5 lo excluyó:** el brief lo dejó fuera salvo congelación expresa (`:475`) y admitió dejarlo para después (`:438`). Las
garantías de V5 son **por rack**: todo o nada «sobre el authored del rack» (D-13 `:502-503`, INV-12); la relectura dentro de MUTATE (D-12,
INV-06); el ámbito de mutación propia por (sesión, miembros del rack) (D-04 `:241-243`, INV-08, IC-09); INV-28; un solo Actualizar a la vez
(`:545`); un paso de UNDO (O-11, INV-32). Y la vía declarada es **la costura de I-55 por rack** (`:507-508`): `RackSiblingRedrawRun.Execute`
recibe la pertenencia de **un** rack y un puerto por sistema (`src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs:30-32`).

**Nuevo:**
- **«Actualizar» por rack:** sin cambio (D-13).
- **«Actualizar todo»**, fuera de los racks, escribe en una sola operación todos los borradores **elegibles** con cambios de la sesión de
  documento activa, nunca de otra (D-03), con **progreso visible** mientras dura y la lista de lo que no se aplicó al terminar
  (replaneación §4, fase 4).
- **Es una composición nueva** por encima de la costura de un rack de I-55, no la costura «extendida». A-14 no fija su mecánica. Fija:
  - **el resultado:** todo o nada sobre el authored de todos los racks incluidos (un Conflicto, una admisión rechazada, un preflight
    fallido o un fallo antes de `Commit()` en cualquiera da cero escrituras authored en todos; sigue admitido el residuo de biblioteca de
    OM-5);
  - **que la costura no cambia:** ni `RackSiblingRedrawRun`, ni `ISiblingRedrawPort`, ni `SiblingRedrawTransaction`, ni su contrato
    observable para Insertar (INV-30); el «Actualizar» por rack sigue igual.
- **Las condiciones que el paquete de la fase 4 tiene que demostrar antes de su contrato:**
  1. **Una transacción y un Deshacer.** Los N racks se escriben en **una** transacción de MUTATE, con la relectura y la comparación de cada
     rack dentro de ella y antes de cualquier escritura, y dejan **un** paso de UNDO. Ninguna lectura ni unidad abre otra transacción;
     INV-39 (guardas caller-owned) se extiende a la composición. Se comprueba en host en el smoke de la fase 4 (como S2-05 y S2-13).
  2. **El coste de N barridos.** El barrido AUTH-06 «abre las entidades de todas las definiciones RackCad» (`:459-461`) y se repite por rack
     en PREPARE y en la relectura de MUTATE. Un arnés con N ≥ 70 racks lo mide y la medición se registra. Una sola captura compartida es
     una mecánica posible, no congelada. Los umbrales siguen D-18 (los fijan Coordinator y Owner tras la línea base).
  3. **El texto pendiente de los borradores no abiertos (salvaguarda).** Con K = 1 solo el rack abierto tiene superficie viva. El texto
     pendiente **conservado** nace en una frontera no bloqueable y «no se compromete ni se auto-repara» (D-10 `:436-437`, INV-18; la nota a
     ADR-0032 del anexo B solo admite el compromiso en fronteras bloqueables). En el modelo de C-02, cambiar de rack es bloqueable, y las
     fronteras no bloqueables (cambio de documento, ocultar el panel) dejan el texto en el rack que **sigue abierto** en su sesión, que al
     volver lo recupera en su caja. No se conoce un camino que deje texto conservado en un borrador no abierto de la sesión activa. La
     condición es una salvaguarda por si apareciera: un borrador así **impide** «Actualizar todo»; no se escribe nada, la ventana lo
     nombra y su texto no se compromete. El rack abierto compromete su texto según D6, porque «Actualizar todo» es una frontera bloqueable
     para él, o la acción se aborta.
  4. **La elegibilidad por estado.** Un borrador en **Conflicto** (exige reconciliación explícita, D-12 `:487-493`), **Desconocido** (exige
     «Comprobar ahora», `:531-532`) o **Error** (ofrece Recargar o Descartar, `:396`) **impide** «Actualizar todo», con cero escrituras y su
     nombre y su causa en la lista. Un borrador Dirty marcado «posiblemente obsoleto» entra: la comprobación previa (O-04) y la relectura
     deciden. Un borrador Limpio no entra.
- **Dice cuál falló y por qué:** cada rack no aplicado aparece con su causa (Conflicto y qué cambió, Admisión, Preflight, Fallo, uno de los
  estados de la condición 4) o con «No aplicado: falló otro rack del conjunto»; visible en la ventana, no solo en el registro (como INV-31).
- **Clasificación por frontera** (D-13 `:521-532`) para el conjunto: si `Commit()` lanza, todos los incluidos pasan a Desconocido, con
  «Comprobar ahora» por rack; si POST falla o hay faltantes, Aplicado con avisos, por rack.
- **Conflicto con el dibujo:** por rack, las dos reconciliaciones de D-12: «Descartar y recargar» (`:488`; en la ventana, «Volver a leer») o
  «Conservar mi borrador» (`:489-491`); sin fusión automática.
- **Ningún otro Actualizar mientras dura** (`:545`), en esa sesión ni en otra.
- **Cancelar:** solo antes de `Commit()` y sin escrituras authored; el cómo lo fija el paquete de la fase 4.
- **Sistemas:** en la fase 4 solo entra Push Back. Las fases 6, 7 y 3b extienden las pruebas de «Actualizar todo» (INV-A14-11 a 15) y su
  OV (OV-A14-06) a sus sistemas; Cantilever y Cama, con su variante caller-owned (`:536-539`).
- **Variables** (decisión Q-02): la fase 4 entrega «Actualizar todo» solo para racks. Cuando la sección Variables llegue a la ventana (C-11),
  sus cambios serán un borrador más que «Actualizar todo» también escribe, con su propia A-n.
- **D-09:** el punto de extensión de lotes se usa; la edición de propiedades comunes de varios racks sigue sin implementarse. **D-20:** el
  residual pasa a «la edición de propiedades comunes de varios racks y todo sistema que no quede integrado». La clasificación se hace en el
  READY del último tramo (C-12) y la acepta el Owner. **No-objetivo** (`:78`): «Edición por lotes» pasa a «Edición de propiedades comunes
  de varios racks: residual de ID3 (D-20)».

**Autoridad:**
- Del cambio: `OWNER-RESERVED` (brief `:475`; no-objetivo; D-20) + Architect + Coordinator (M-04, M-01, M-05; M-08 por la nota a ADR-0010).
- **De la composición:** la aprueban Architect + Coordinator en el paquete de la fase 4, con su revisión independiente, antes de emitir su
  contrato. Si la composición exigiera cambiar la costura o su contrato, es M-08 frente al Freeze integrado de I-55, que prevalece (V5
  `:32-33`): STOP antes del contrato, y la decisión sube a quien gobierna ese Freeze (V5 `:449-450`) y, si choca, al Owner (WORKFLOW §10).

**Riesgo que V5 quería evitar, y su tratamiento:**

| Lo que V5 protegía | Cómo lo trata A-14 |
|---|---|
| Todo o nada por rack (D-13, INV-12) | Todo o nada sobre la unión, en una transacción (condición 1) |
| Relectura dentro de MUTATE (D-12, INV-06) | La de cada rack, dentro de la misma transacción y antes de escribir; ningún rack se escribe si otro da Conflicto |
| Ámbito de mutación propia (D-04, INV-08, IC-09) | La unión de los miembros de los racks incluidos, de PREPARE a POST |
| INV-28 | Para todo borrador no incluido |
| Un paso de UNDO (O-11, INV-32) | Un paso para la operación (condición 1) |
| Texto pendiente nunca comprometido fuera de una frontera bloqueable (INV-18) | Condición 3 |
| Un estado que pide acción del usuario (Conflicto, Desconocido, Error) | Condición 4 |
| La costura de I-55 (V5 `:32-33`, `:533-535`) | Composición encima, sin cambiarla; si no se puede, STOP |
| Coste y bloqueo del documento | Condición 2; progreso visible (R-04) |

### C-06. No-objetivo «Crear racks nuevos desde el panel» → «Nuevo»

**Vigente** (`:77`): «Crear racks nuevos desde el panel: los comandos de creación no cambian.»

**Nuevo:**
- Se retira el no-objetivo. La sección «Nuevo» ofrece los sistemas del menú de RACKCAD.
- **Paso intermedio, desde la fase 1:** «Nuevo» abre la ventana clásica del sistema, con el flujo de hoy (crea y nombra como hoy, I-60). Con
  el rediseño, esa ventana es la nueva de cada sistema (C-09).
- **Meta, fase 8:** crear el sistema dentro de la ventana como «Nuevo, sin insertar» e insertarlo desde ella. Se decide con **su propia
  A-n**, antes del paquete de la fase 8: Insertar desde la ventana (no-objetivo `:75-76`; D-23 `:711`), la identidad nueva (D-22 `:704`,
  INV-16), la creación de vistas (D-PB-04, OD `:76-78`) y el nombre (I-60). Hasta entonces el no-objetivo `:75-76` sigue.

**Autoridad:** `OWNER-RESERVED` (no-objetivo). La A-n de la fase 8: Owner + Architect + Coordinator.

**Riesgo que V5 quería evitar:** que el panel estampe identidad o escriba fuera de Actualizar (D-22, INV-16, INV-21). **Tratamiento:** en el
intermedio solo se lanza el flujo clásico, ligado a su sesión; crea y estampa el flujo clásico, como hoy.

### C-07. D-16 y el no-objetivo de comandos: RACKCAD, RK, el menú de hoy, RACKPANEL y las secciones

**Vigente:** no-objetivo (`:80`); D-16 (`:586-587`); §14 M-03 (`:996`); D-01 (`:124`); D-02 (`:151`).

**Hecho:** RACKCAD y su alias RK (`AliasRackCad() => RackCad()`) abren hoy el menú modal con `ShowModalWindow`
(`src/RackCad.Plugin/RackMenuCommands.cs:21-34`). El menú ofrece diseñar los seis sistemas y el larguero, generar un perfil estructural,
abrir de la biblioteca de diseños (que crea e inserta un rack), drive-in («Próximamente») y la ruta de la biblioteca de bloques
(`src/RackCad.UI/RackMainMenuWindow.xaml`).

**Nuevo:**
- **No-objetivo** (`:80`): «Cambiar `RACKEDITAR`, `RACKLISTA`, `RACKPROYECTAR` u otros comandos existentes (D-16).» pasa a: «Cambiar
  los comandos existentes fuera de los cambios declarados de A-14 (C-07 y C-09) y de la lista de retiro de la fase 9, que entra por su
  propia A-n (D-16).»
- **RACKCAD y RK** abren o muestran la ventana desde la fase 1 y dejan de abrir el menú modal (decisión Q-07). Todo el contenido del menú
  queda en la ventana; el paquete de la fase 1 fija en qué sección va cada elemento. La ruta de la biblioteca de bloques, que el menú
  ya guarda hoy en los ajustes de usuario, la guarda la ventana desde la fase 1, por la excepción 2 de C-10.
- **Entrada de respaldo:** hasta la fase 9, la ventana tiene una entrada que abre el menú de hoy, tal cual. Es un flujo clásico lanzado
  desde la ventana (R-13). Ningún comando nuevo.
- **RACKPANEL** se conserva; su futuro, con la lista de retiro. **RACKPANELDESCARTAR** (D-17) se conserva.
- D-01 (`:124`): «se muestra con RACKCAD, RK o RACKPANEL». D-02 (`:151`): el controlador se crea con el primero de ellos.
- **Secciones** (detalle en C-11): Variables, BOM y lista de racks, Biblioteca de diseños y Configuración.
- Los demás comandos siguen sin cambio observable (D-16, INV-19), salvo los cambios declarados de C-09, hasta la fase 9.
- **Retiro de comandos:** fase 9, con la lista que decida el Owner antes. Como cambia D-16, la lista entra por su propia A-n antes del
  paquete de la fase 9. Incluye la entrada de respaldo. RACKPROYECTAR y RACKEDITAR se quedan.

**Autoridad:** `OWNER-RESERVED` (no-objetivo; decisiones 3, 5 y 8) + Architect + Coordinator (M-03, M-07).

**Riesgo que V5 quería evitar:** romper el uso clásico (INV-19; STOP de D-16 `:593`). **Tratamiento:** solo cambian RACKCAD y RK, por
decisión del Owner y como cambio declarado; el menú de hoy sigue a mano; el retiro necesita lista, A-n y fase propia.

### C-08. D-15 y F4B D-PB-01: el editor vive en el área central

**Vigente:** §11 (`:793-795`) y D-15 punto 1 (`:562-563`), citados en el §1; orden de D-15 (`:577-581`); «No se reescribe lógica de dominio»
(`:582`). F4B D-PB-01 (OD `:22-49`; PKG OB-01.4 `:174`), con «Do not build a second full Push Back editor» (OD `:36`) y «Do not try to fit
the current ~1120 px editor inside the ~320 px panel» (OD `:37`).

**Nuevo:**
- El editor de cada sistema vive en el **área central** de la ventana, sin bloquear AutoCAD: la realineación con D-15 punto 1 y §11.
- **Push Back primero, tal cual (fase 3):**
  - la fase 3 hace para Push Back el punto 1 de D-15: separa la superficie de su `Window`, y la ventana clásica la envuelve;
  - reutiliza lo que dejó F4B: el estado sin pérdidas (PB-T1), el modo WORKSPACE con aplicar, cerrar y escribir separados (PB-T2) y la
    escritura PREPARE → MUTATE → POST (PB-T3);
  - «Editar todo» deja de abrir la ventana modal;
  - en la fase 3, RACKEDITAR de Push Back no cambia de forma observable (T1B en verde, INV-19); su rediseño llega en la 3b (C-09);
  - una superficie y un estado: no hay segundo editor.
- La ventana deja de **medir** 320 px: sigue siendo el mismo `PaletteSet` (C-13), acoplable o flotante y redimensionable. La fase 3 empieza
  midiendo si el editor cabe y funciona en ella; si no, la contingencia (b) de D-01 por A-n material (`:127-128`).
- **D-PB-01** (OD `:22-49`) rige F4B sin cambio. Desde el cierre de la fase 3, A-14 sustituye **solo** las frases sobre la ventana modal:

  | OD | Texto literal | Desde el cierre de la fase 3 |
  |---|---|---|
  | `:25-26` | «as a modal/full-size editing surface» | **Sustituido:** «Editar todo» abre el editor completo de Push Back en el área central de la ventana RackCad, sin bloquear AutoCAD |
  | `:34` | «the drawing cannot be edited while the modal editor is open.» | **Sustituido:** el dibujo se puede usar mientras el editor está abierto; rigen D-01 (foco) y D-04 (pistas y drenaje) |
  | `:37` | «Do not try to fit the current ~1120 px editor inside the ~320 px panel.» | **Sustituido:** la ventana deja de medir 320 px; la fase 3 mide primero si el editor cabe (R-01) |
  | `:22-24`, `:26` | «The Workspace exposes: "Editar todo"», «loaded from the Workspace draft» | **Se conserva** |
  | `:29-33` | el editor lee del borrador; su «Apply/Actualizar» solo modifica el borrador; «ZERO authored DWG writes»; el Workspace es la autoridad del «Actualizar» final; PREPARE / MUTATE / POST de T4 hace la escritura | **Se conserva** |
  | `:36` | «Do not build a second full Push Back editor.» | **Se conserva** |
  | `:38` | «RACKEDITAR behavior must remain observably compatible.» | **Se conserva** hasta la fase 3b de Push Back, donde rige C-09 |
  | `:39-49` | los dos modos, CLASSIC y WORKSPACE; «The mode must be explicit»; «Do not let a Workspace invocation accidentally execute the classic drawing-write path»; el rótulo en modo WORKSPACE | **Se conserva** |

  OB-01.4 del paquete de F4B («modal», PKG `:174`) cae con `:34`. SPB-12 y SPB-13 certifican el comportamiento de F4B y no se
  reetiquetan. El paquete de la fase 3 fija cómo se presenta el editor alojado (por ejemplo, el rótulo «Aplicar al borrador»), sin
  tocar lo que se conserva.
- **Rediseño (línea D, «Diseñar antes de migrar»):** Selectivo, Dinámico, Cantilever, Cabecera y Cama se migran una vez, con su diseño nuevo
  aprobado por el Owner en un prototipo; Push Back se rediseña después (3b). El rediseño es de superficie: la lógica de dominio no se
  reescribe (`:582`). Lo que pasa con RACKEDITAR, en C-09.
- El contrato de adaptador de D-15 (cinco puntos) y el criterio de entrada por sistema no cambian. La caracterización previa cambia para
  los sistemas rediseñados (C-09).

**Autoridad:** Architect + Coordinator (M-05) y Owner: OD `:25-26`, `:34` y `:37` son texto literal del Owner, y el rediseño cambia
comportamiento observable (ADR-0029 D13). La aprobación del §3 de la replaneación lo cubre; la firma nombra exactamente lo que se
sustituye y lo que se conserva (§11).

**Riesgo que F4B y V5 querían evitar:** que el editor (1120 × 672) no quepa (H-PB-10); un segundo editor (OD `:36`); que RACKEDITAR cambie
sin decisión. **Tratamiento:** medición primero; contingencia de D-01; una superficie; T1B en verde en la fase 3.

### C-09. RACKEDITAR y los comandos de creación adoptan el diseño nuevo de cada sistema (decisión Q-03)

**Vigente:**
- D-16 (`:586-587`): «Sin cambio observable en RACKEDITAR, los comandos de creación, …»; su STOP (`:593`): «Si la extracción de superficies
  (D-15) cambiara algo observable de un comando clásico, sería M-03 y **STOP**».
- D-15 (`:570-572`): la caracterización previa «Siguen en verde después de la extracción (INV-19)».
- INV-19 (`:757`); §14 M-03 (`:996`); OV-17 «Compatibilidad con RACKEDITAR» (`:949`).
- Brief (`:393-394`): «Existing standalone RACKEDITAR behavior should remain usable during migration unless Freeze explicitly changes it».
- ADR-0029 D13: «la caracterización no se edita para que pase»; cambiar un comportamiento observable «exige decisión del Owner».

**Nuevo:**
- **En la fase de cada sistema rediseñado** (6: Selectivo, Dinámico y Cantilever; 7: Cabecera y Cama; 3b: Push Back), RACKEDITAR abre la
  **misma superficie rediseñada** que la ventana RackCad, en una ventana aparte. Hay un solo editor por sistema.
- **Los comandos de creación también (deducción de la sesión, que el Owner confirma al firmar):** la respuesta del Owner nombra
  RACKEDITAR. La ventana de cada sistema es la misma para crear y para editar (por ejemplo, la de Push Back la abren
  `RackPushBackCommands` y el camino de RACKEDITAR, PKG `:171`). Con «un solo editor por sistema», el cambio alcanza también a los comandos
  de creación de ese sistema.
- **El camino de escritura clásico no cambia:** RACKEDITAR conserva `FindRackBlocks`, el commit por vista y sus mensajes (D-16 `:587`);
  cambia la superficie, no la escritura.
- **El cambio se declara en la fase de cada sistema** (decisión Q-03: «El cambio se declara cuando ocurra»):
  - la caracterización previa del sistema se escribe y se pone en verde **antes** de extraer la superficie, como pide D-15;
  - tras el rediseño, sus pruebas **no se editan**: se conservan intactas, marcadas `Skip` con un motivo que nombra un **registro declarado
    de caracterización sustituta** del sistema, y el comportamiento nuevo vive en pruebas propias. Es el patrón de [OD] (`:237-243`: «create
    a declared superseding characterization/change record. Do not silently weaken the old guard») y su precedente
    `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`;
  - **la decisión del Owner que exige ADR-0029 D13** (deducción de la sesión, que el Owner confirma al firmar): la forman la decisión
    Q-03, la aprobación del prototipo del sistema (línea D) y **la orden del Owner que emite la fase de ese sistema, que nombra el registro
    de cambios declarados**, fila por fila, como D-PB-06 nombró sus correcciones (SUS-01 a SUS-04). Un prototipo visual puede no mostrar
    todo lo que fija D13 (Enter, Escape, foco, tabulación, caminos de cierre); lo que la orden no nombre no está autorizado y es STOP.
- **D-16, INV-19 y el STOP:** rigen por sistema hasta su fase. En esa fase, el cambio declarado no es STOP. Cualquier cambio observable de un
  comando clásico que el paquete de la fase **no** declare sigue siendo M-03 y STOP.
- **§14 M-03:** se reabre para RACKCAD y RK (fase 1) y para RACKEDITAR y los comandos de creación de cada sistema rediseñado, solo con cambio
  declarado.
- **OV-17** se sustituye por «RACKEDITAR abre, en una ventana aparte, la superficie de su sistema (la rediseñada, desde la fase de ese
  sistema) y conserva su camino clásico de escritura».
- **Respaldo:** hasta la fase de cada sistema, RACKEDITAR y «Abrir en editor clásico» abren su ventana de hoy. Desde esa fase ya no hay un
  respaldo con la interfaz de hoy; el respaldo es la escritura clásica de RACKEDITAR.

**Autoridad:** `OWNER-RESERVED` (brief `:393-394`; OV-17) + Architect + Coordinator (M-03, M-05; ADR-0029 D13).

**Riesgo que V5 quería evitar:** cambiar el uso clásico sin decisión (INV-19, M-03) y tener dos editores por sistema (D-15 punto 1, OD
`:36`). **Tratamiento:** un solo editor; el cambio, declarado por sistema y por fase, con su registro; la escritura clásica, intacta.

### C-10. Persistencia: tamaño y posición de la ventana, y las demás excepciones de D-22

**Vigente:** no-objetivo (`:79`): «Persistir el panel o sus borradores, incluida la posición de la paleta entre sesiones (brief,
«MULTI-DOCUMENT»).»; D-22 (`:702-703`: «… ni archivos aparte del registro, ni escrituras en el registro de Windows o el perfil desde el
código de RackCad (INV-21)»); D-01 (`:122`); D-23 (`:712`); brief (`:371`, `:474`).

**Hechos:** hoy la paleta se coloca igual en cada creación (`WorkspacePaletteHost.ApplyInitialPlacement`, F1-T3B-HOST-UI-C1, sin persistencia
ni perfil); el diff de perfil de Smoke-1 salió limpio sobre ese build. RackCad tiene un almacén de ajustes del usuario fuera del DWG
(`src/RackCad.Application/Settings/UserSettings.cs`), que ya guarda `BlockLibraryPath`, `DesignLibraryPath` y `SelectiveTargetFondos`
(`:13-27`).

**Nuevo:**
- **No-objetivo** (`:79`) pasa a: «Persistir el panel o sus borradores entre sesiones (brief, «MULTI-DOCUMENT»), salvo el tamaño y la
  posición de la ventana (C-10).»
- La ventana recuerda su **tamaño y su posición** entre sesiones de AutoCAD (decisión 2). **Lectura de la sesión, que el Owner confirma al
  firmar:** «posición» incluye si estaba acoplada o flotante y en qué lado. Qué valores se guardan es mecánica no congelada. Los
  borradores, el rack abierto, los filtros y la sesión siguen sin persistirse.
- **Vía:** los ajustes de usuario de RackCad, no el estado de paleta de AutoCAD. Así D-01 `:122` queda literal.
- Si el rectángulo guardado no cae entero en un monitor visible, o no se puede leer, se usa la colocación inicial de hoy (precedente: el STOP
  de Smoke-1 en SM-03, con la paleta fuera de la pantalla).
- Guardar el rectángulo no altera los demás ajustes del archivo, aunque otra ventana los haya cambiado mientras tanto.
- **El diff de perfil no se hereda:** S1-11 certificó un build sin persistencia. El smoke de la fase 1 repite S1-11 con el procedimiento
  F1-T3C y el acuse de I-52 (D-01 `:140-147`).
- **D-22 e INV-21, excepciones nombradas**, cada una nacida de una acción del usuario (mover o cambiar el tamaño de la ventana cuenta como
  tal) y ninguna en el DWG, el NOD, XData, el registro de Windows ni el perfil:
  1. el rectángulo de la ventana (fase 1);
  2. los ajustes que el usuario cambia en la ventana, en el mismo almacén: desde la fase 1, la ruta de la biblioteca de bloques, que el
     menú de hoy ya guarda ahí (`RackMainMenuWindow.xaml`, «Biblioteca de bloques (.dwg)»; `UserSettings.cs:13`); desde la fase 5, los
     demás ajustes de Configuración;
  3. los archivos que el usuario exporta desde BOM y lista (XLSX o CSV, como hoy RACKBOMTOTAL; fase 5);
  4. los diseños que el usuario guarda en la biblioteca, si la sección lo ofrece (fase 5; lo que decidió D-PB-07 para Push Back sigue).
- «Abrir RackCad al iniciar AutoCAD» queda fuera (decisión Q-08).

**Autoridad:** `OWNER-RESERVED` (no-objetivo; brief `:474`) + Architect + Coordinator (M-02: rompe la condición de §14 `:995`).

**Riesgo que V5 quería evitar:** tocar el perfil o la configuración que vigila I-52 (D-01 `:140-147`; §12.1 `:826-827`) y adelantar la
restauración entre arranques. **Tratamiento:** la vía no toca el perfil; S1-11 se repite; solo tamaño y posición.

### C-11. Las secciones dentro de la ventana (fase 5)

**Vigente:** el brief (`:444-457`) prepara superficies para «Project Variables» sin implementarlas, y V5 lo hace no-objetivo (`:74`); D-23
(`:708-709`) prevé regiones de inspector «en la pestaña y en Selección (N)»; D-05 (`:260`): «Métricas, providers, población y agregación →
I-63»; D-05 (`:312-313`): «`ProjectSummary` (ID20) podrá alojarse después en el panel como consumidor o superficie»; INV-36 (`:774`), con
su guarda «ningún archivo fuera de las carpetas del panel … referencia sus tipos».

**Nuevo:**
- Las secciones son **nuevas**. Las regiones de inspector de D-23 son solo su antecedente; no se implementan como regiones.
- **Paso intermedio, desde la fase 1:** cada sección abre su ventana actual, que sigue siendo modal y escribe como hoy (subdiálogos de D-02
  `:173-174`).
- **Fase 5, dentro de la ventana y sin bloquear, solo lo que no escribe el DWG:**
  - **BOM y lista de racks**, ligada a su sesión de documento (D-03). Consume las autoridades existentes de RACKLISTA y RACKBOMTOTAL como
    consumidor o superficie (D-05 `:312-313`; reparto de MASTER-I63-I64-02 §3, V5 `:260`), **no** el índice del panel. Exporta archivos por
    la excepción 3 de C-10.
  - **Biblioteca de diseños:** mientras no llegue la A-n de la fase 8, abrir un diseño para insertarlo lanza el flujo clásico de hoy.
  - **Configuración:** escribe los ajustes de usuario por la excepción 2 de C-10. «Abrir RackCad al iniciar AutoCAD» se decide con su diseño
    (Q-08).
- **Variables** (decisión Q-02): dentro de la ventana, con un **borrador de variables** por sesión de documento que escribe «Actualizar
  todo». Depende de la fase 4 y de **su propia A-n**, antes de su paquete, porque toca:
  - D-10 (la clave de un borrador es (sesión, RackId), `:410`);
  - D-12 (la StaleBase de Selectivo incluye el registro entero, `:475-477`);
  - D-11 (el ejecutor de variables usa `FindRackBlocks`, `:451`);
  - D-17 (el veto debe cubrir ese borrador);
  - ADR-0034 (la autoridad del registro).

  Hasta esa A-n, la sección Variables abre la ventana de hoy.
- **Privacidad del índice:** los archivos de las secciones no referencian los tipos del índice, de `NavigationTargets` ni de los hechos
  neutrales, aunque estén dentro de las carpetas de la ventana. La guarda de INV-36 se amplía a ellos (INV-A14-10).

**Autoridad:** `OWNER-RESERVED` (brief `:444-457`; las secciones, decisión del Owner) + Architect + Coordinator (M-06, M-07; M-01 y M-08
para Variables, en su A-n).

**Riesgo que V5 quería evitar:** escribir el DWG fuera de Actualizar («NO LIVE MUTATION», brief `:77`, `:94`); que el índice alimente
métricas (INV-36, MASTER-I63-I64-02). **Tratamiento:** la fase 5 no escribe el DWG; Variables solo con borrador, «Actualizar todo» y su
A-n; la guarda ampliada.

### C-12. Integración por tramos (decisión Q-09)

**Vigente:** A64-PV1-21 (V5 `:803-805`): «no hay cierre ni integración parcial de la iniciativa por resecuenciación; una integración parcial
exige decisión del Owner»; §12.6 (`:929`): «Todos los escenarios se asignan a la unidad **I-64** (única unidad)»; D-20 (`:681-683`); D-21
(`:687-693`); §12.5. Dentro de F4B: IC-11 (ICL `:204`) y OB-02.3 (PKG `:200`): «No hay integración parcial en `main`».

**Nuevo:** la decisión del Owner es la que pide A64-PV1-21. I-64 se integra en `main` **por tramos**.
- **Un tramo es una unidad de entrega** (LIFECYCLE §2 `:54-64`), con su propio resultado verificable y:
  - la orden del Owner que lo abre y la que confirma su frontera;
  - READY-01..09 en su orden y su `FINAL_CANDIDATE_SHA` (LIFECYCLE §8), con la conformidad de Architect + Coordinator (READY-06, §9);
  - su propio bootstrap y su Discovery delta, que vuelve a comprobar DC-01..06 en su base y DC-07..09 al día (LIFECYCLE §2 `:60-64`);
  - su Owner Validation sobre ese SHA, con OV propios que cubren lo que entra en ese tramo (regla de abajo) más el checklist de la guía
    (LIFECYCLE §8 `:255-259`; Q-09: «Cada hito lleva su candidato completo y tu validación»);
  - su archivo de evidencia y su tag `integration/<unidad>` (WORKFLOW §11.4 y §11.6);
  - la integración de WORKFLOW §4.5: rebase final, Candidato, cierre documental (HANDOFF y ROADMAP), merge `--no-ff`, CI posterior al merge
    sobre `MERGE_SHA` (§4.5.6), comprobación diferida de la cobertura del Candidato (§4.5.7) y **limpieza solo después de las dos** (§4.5,
    paso 6; §3);
  - antes del Candidato, el equivalente de F7 para lo que entrega: regresión, instrumentación, la guía de validación manual (WORKFLOW
    §11.4) y el README si cambian comandos (WORKFLOW §8).
- **Unidades:** el tramo 1 es la unidad I-64, en su rama y con su Claim-Id. La limpieza de §4.5 retira esa rama y su worktree. Cada tramo
  siguiente es una unidad nueva, con su reclamo, rama, worktree desde `origin/main` y fila en ROADMAP (WORKFLOW §2 y §4.1), y consume V5 y las
  A-n cuando son visibles desde `origin/main` (LIFECYCLE §2 `:63-64`). A-14 las alcanza («Applies-to: all») y también A-7, cuyo alcance
  amplía la cabecera; los puentes A-1..A-6 y A-8..A-13 siguen siendo de la unidad I-64. Precedentes: las tres integraciones de I-53 (I-53, I-53S, I-53D) y I-52-AUTH15. El nombre de cada unidad lo fija
  el Coordinator en el bootstrap de esa unidad.
- **OV por tramo (regla):** §12.6 deja de asignar todo a una sola unidad. La Owner Validation de cada tramo cubre lo que entra en él:
  - los OV del comportamiento que el tramo entrega por primera vez;
  - para cada sistema que el tramo lleva a la ventana, los OV de edición en la ventana: borrador, Actualizar, Conflicto, UNDO, foco y
    teclado, resultados visibles y «Actualizar todo» con ese sistema, repetidos para él;
  - en el último tramo, además, OV-07 (varios sistemas) y OV-22 (los registros completos de rendimiento).

  La tabla de §5 aplica esta regla al corte propuesto. Reasignar un OV exige A-n y retirarlo, Owner (LIFECYCLE §8).
- **ID3** se clasifica solo en el READY del último tramo: D-20 necesita saber qué sistemas quedaron integrados. Los tramos anteriores no la
  clasifican.
- **F4B:** IC-11 y OB-02.3 rigen dentro de F4B sin cambio: F4B no se integra sola y llega a `main` dentro del tramo 1.
- **Hitos, PROPUESTA para que el Owner los confirme al firmar.** La frontera de cada tramo se confirma también al abrirlo:

  | Tramo | Fases | Qué llega a `main` |
  |---|---|---|
  | 1 | 0, 1, 2, 3 y 4 | La ventana con los racks y Push Back (el ejemplo del Owner) y «Actualizar todo»: F1 a F4B, la ventana, la lista, «Abrir en editor clásico», Push Back en la ventana y «Actualizar todo» para racks |
  | 2 | 5 | Las secciones dentro de la ventana; Variables, con su A-n |
  | 3 | 6 | Selectivo, Dinámico y Cantilever, rediseñados |
  | 4 | 7 y 3b | Cabecera, Cama y Push Back rediseñados |
  | 5 | 8 y 9 | «Nuevo» en la ventana y el retiro de comandos; READY final y clasificación de ID3 |

  - Con este corte, los tramos 2 y 3 dependen solo del tramo 1: la fase 6 no espera a la 5. Su orden, o si van en paralelo, lo fija el
    Coordinator al abrirlos (WORKFLOW §2: sin archivos calientes compartidos).
  - **Alternativa visible en la firma:** el tramo 1 hasta la fase 3, que es el ejemplo literal del Owner («tras la ventana con los racks y
    Push Back»). Con ella, la fase 4 forma su propio tramo antes de los de las fases 5 y 6, para no atar la 6 a la 5.
  - **Pros y contras de las dos opciones:**

    | | Tramo 1 = fases 0 a 4 (propuesta) | Tramo 1 = fases 0 a 3 (alternativa) |
    |---|---|---|
    | Primera integración en `main` | **Más tarde:** espera además a la fase 4, la de más riesgo (la composición sobre la costura de I-55 puede dar STOP, C-05). El tramo 1 ya lleva F1 a F4B, y la rama envejece más (R-11), contra «Usas antes lo nuevo y la rama no envejece» de Q-09 | **Antes:** llega con Push Back en la ventana, como en el ejemplo del Owner |
    | Smoke principal de las fases 3 y 4 | Entero, en el tramo 1 | Partido en dos |
    | Ciclos de unidad | Cinco tramos | Seis tramos: la fase 4 va sola |
    | Nota a ADR-0010, si ADR-0047 se acepta en el tramo 1 | Completa: el tramo 1 ya entrega «Actualizar todo» | Sin «Actualizar todo», que llega en una nota posterior |
    | Los tramos de las fases 5 y 6 | Dependen solo del tramo 1 | Dependen del tramo 1 y del de la fase 4 |

- **Si el Owner elige la alternativa o corrige los hitos al firmar:** la regla de C-12 (tramo = unidad de entrega, con su READY y OV por lo
  que entra) no cambia, y A-14 se activa igual. La tabla de tramos y la asignación de OV que resultan se versionan por la **A-n siguiente**,
  antes del READY-01 del primer tramo afectado: reasignar OV exige A-n (LIFECYCLE §8). Si el afectado es el tramo 1, que ya está abierto,
  sus fases siguen y lo que espera a esa A-n es su READY-01; los tramos posteriores afectados no se abren antes de ella.
- **Smoke-PERF por tramo (regla, con su valor por defecto):** el READY de cada tramo exige el Smoke-PERF de §12.5 completo sobre el
  Candidato del tramo (§12.5 `:902`: «Sobre un Candidato completo»): SP-01 a SP-06, con los dibujos reales del Owner y su aceptación
  explícita. Es una sesión de host del Owner por tramo: cinco con el corte propuesto. **Autoridad: el Owner, a propuesta del
  Coordinator.** El valor por defecto lo confirma el Owner en la firma; una reducción por tramo (por ejemplo, SP-01 y SP-05, porque la
  ventana está en todos, más los SP de los escenarios que el tramo toca) solo entra por la A-n siguiente, antes del READY-01 del tramo
  al que se aplique.
- **ADR-0047 (regla, con su valor por defecto):** un ADR propuesto puede editarse hasta su aceptación (`docs/adr/README.md:37`); uno aceptado
  es inmutable y solo admite una nota posterior fechada (D-21 `:692`); las notas del anexo B se escriben al aceptarlo, en el commit de cierre
  (D-21 `:692-693`).
  - Por defecto, ADR-0047 sigue `propuesto` y se actualiza con los textos de §4 (anexo A).
  - **Quién lo cierra y cuándo:** el Owner decide si lo acepta en el READY de cada tramo, y su decisión queda en el registro de cierre de
    ese tramo. El Architect revisa la forma en la conformidad (READY-06).
  - Si se acepta antes del último tramo, recoge solo las decisiones ya entregadas. Las notas del anexo B se escriben entonces solo con lo
    entregado: con el corte propuesto, el tramo 1 ya entrega «Actualizar todo» y las dos notas pueden ir completas. Lo que entreguen los
    tramos siguientes va en un ADR nuevo que lo complemente, con sus propias notas posteriores.

**Autoridad:** `OWNER-RESERVED` (integración parcial, A64-PV1-21; los hitos; Smoke-PERF por tramo: el Owner, a propuesta del
Coordinator) + Coordinator (las reglas de proceso dentro de WORKFLOW y LIFECYCLE) + Architect y Coordinator (el `Applies-to` de A-14 y la
ampliación del alcance de A-7, que es la autoridad de A-7).

**Riesgo que V5 quería evitar:** cerrar o integrar a medias sin que el resultado quede verificado. **Tratamiento:** cada tramo tiene la
evidencia completa de una unidad; ninguna se hereda de otro tramo (LIFECYCLE §7, punto 7; §8).

### C-13. D-01 sigue igual

No cambian: un `PaletteSet` por proceso con un solo control raíz WPF (`:120-121`); sin `toolID` ni `Load`/`Save` (`:122`, por la vía de
C-10); acoplable y flotante (`:123`); cerrar oculta (`:125`); la contingencia (b) (`:127-128`); la obligación de foco (`:130-135`) y su prueba
(INV-24 con A-7: S2-16 a S2-22), que ahora alcanza todos los controles de cada editor alojado, también en las unidades de los tramos
siguientes, por la ampliación del alcance de A-7 (cabecera); el perfil (`:140-147`). Cambia solo el
comando que muestra la paleta (`:124`, C-07).

## 4. Cláusulas de V5: qué cambia, cómo y con qué autoridad

«Con C-nn» significa: la autoridad de ese cambio.

| Cláusula | ¿Cambia? | Delta | Autoridad |
|---|---|---|---|
| §0 Actualizar (`:53`) | Sí | «La única frontera de escritura en el DWG desde el panel» → «Actualizar» y «Actualizar todo» son las únicas fronteras de escritura en el DWG desde la ventana; los flujos clásicos que la ventana lanza escriben como hoy | Con C-01 y C-05 |
| §0 `NavigationTargets` (`:56`) | Texto | «el contexto, las pestañas y el navegador» → «el contexto, el rack abierto y la lista» | Con C-02 |
| §0 Panel RackCad (`:64`) | Sí | Se añade «Ventana RackCad»: el mismo `PaletteSet` del panel cuando aloja la operación de RackCad (C-01) | Con C-01 |
| §1 | Sí | C-01, C-02, C-05, C-06, C-07, C-10 | Owner + Architect + Coordinator |
| §2 Cierre (`:105`) | Sí | «política de pestaña, panel y documento» → «del rack abierto, de la ventana y del documento» | Con C-02 |
| §2 Aceptación de Actualizar (`:106`) | Sí | Se añade «Actualizar todo»: puerto de la ventana ligado a su sesión → la composición de C-05 | Con C-05 |
| D-01 | Solo `:124` | C-07, C-13 | Con C-07 |
| D-02 (`:151`) | Sí | El controlador se crea con el primer RACKCAD, RK o RACKPANEL | Con C-07 |
| D-03 (`:186`) | Texto | «pestañas (con su preview), pestaña activa» → «lista, rack abierto e interruptor de seguir la selección» | Con C-02 |
| D-04 (`:241-243`) | Sí | Para «Actualizar todo», el ámbito de mutación propia se indexa por (sesión, unión de los miembros de los racks incluidos), de PREPARE a POST; para un rack, sin cambio | Architect + Coordinator (M-04) |
| D-05 (`:259`, `:299`, `:325`) | Texto | Regla de C-03; «se muestra en la pestaña» → «se muestra en el área central del rack abierto». La privacidad (`:284-295`) y el reparto del Master no cambian; INV-36 se amplía (C-11) | Con C-02 y C-11 |
| D-06 (`:335-338`) | Sí | Efectos de Uno(R) y del interruptor (C-02) | Con C-02 |
| D-07 (`:349`, `:352`, `:365`, `:367`, `:371`) | Texto | Regla de C-03; la semántica (eco, revalidación, objetivos) no cambia | Con C-02 |
| D-08 | Sí | C-02 | Con C-02 |
| D-09 (`:402-406`) | Sí | «abrir el rack (preview o fijada)» → «abrirlo en el área central»; el punto de extensión se usa (C-05) | Con C-02 y C-05 |
| D-10 (`:425-426`, `:434`, `:440`) | Sí | Preview de navegación conservada; vida del borrador; «Actualizar todo» es frontera bloqueable para el rack abierto (C-02, C-05) | Con C-02 y C-05 |
| D-11 | No | — | — |
| D-12 | No | «Actualizar todo» usa sus dos reconciliaciones por rack. El cambio que traiga Variables (`:475-477`) va en su A-n | — |
| D-13 | Solo «Actualizar todo» y `:545` | Por rack, sin cambio; C-05; `:545`: «la pestaña» → «el rack» | Con C-05 |
| D-14 (`:549-551`) | Texto | «cambiar de pestaña» → «cambiar de rack abierto»; «cerrar una pestaña limpia» → «dejar un rack limpio»; «Una pestaña con dirty … nunca es preview ni se reemplaza (D-08), y cerrarla pregunta» → «un rack con dirty o texto pendiente conserva su borrador al dejarlo, y descartarlo pregunta» | Con C-02 |
| D-15 | Sí | Orden (§6); punto 1 para Push Back en la fase 3 (C-08); caracterización por sistema rediseñado (C-09) | Owner + Architect + Coordinator |
| D-16 | Sí | C-07 y C-09 | Owner + Architect + Coordinator |
| D-17 | Alcance | El veto cubre también los borradores nuevos (de variables y, tras su A-n, «Nuevo, sin insertar»); su semántica no cambia; INV-20 se extiende | Architect + Coordinator |
| D-18 (`:647-651`, `:659-660`) | Sí | Escenario 5: «cambio de pestaña» → «cambio de rack abierto con superficie materializada» (fase 2 y fases de editor); 6: «10 pestañas ligeras» → «10 racks con borrador vivo en la lista: recuento de superficies y suscripciones» (fase 2); 7 y 8: gate F6 → fase 2; nuevo: «Actualizar todo» con N ≥ 70 racks, arnés en la fase 4 y en su smoke. Umbrales, sin cambio | Coordinator + con C-02 y C-05 |
| D-19 | No | — | — |
| D-20 | Sí | Residual y momento (C-05, C-12) | Owner |
| D-21 (`:687-693`) | Sí | ADR-0010 se complementa también con «Actualizar todo» (nota del anexo B, abajo); el momento de aceptar ADR-0047 (C-12) | Owner (acepta el ADR) + Architect + Coordinator (M-08) |
| D-22 | Sí | C-10 (cuatro excepciones nombradas) | Owner + Architect + Coordinator |
| D-23 | Sí | Borrador por lotes: se implementa (C-05); regiones de inspector: antecedente de las secciones (C-11); Insertar: A-n de la fase 8 (C-06); restauración entre arranques: solo tamaño y posición (C-10); índice incremental y K > 1: sin cambio | Con C-05, C-06, C-10 y C-11 |
| §10 | Sí | §5 | Con cada C-nn |
| §11 | Sí | §6 | Owner + Coordinator |
| §12.1 | No | Cada hito de cada tramo sigue sus reglas | — |
| §12.5 (`:908-909`) | Sí | SP-02: «cambio … de pestaña y 10 pestañas ligeras» → «cambio de rack abierto y 10 racks con borrador vivo»; SP-03: «sobre los dibujos más grandes del Owner, al menos uno de unos 70 racks, incluidos cambios frecuentes de referencias (R-10)»; Smoke-PERF en el READY de cada tramo, con la regla y el valor por defecto de C-12 | Coordinator + Owner (aceptación de §12.5 y regla por tramo) |
| §12.6 | Sí | Asignación por tramo (C-12); OV-08, 09, 11, 14, 17 y 20 sustituidos; escenarios nuevos (§5) | Owner (sustituir) + la autoridad de A-14 (añadir) |
| §14 (`:995-996`) | Sí | M-02 y M-03 se reabren, por su propia regla («Si una condición se rompe, M-02 se reabre») y por C-07, C-09 y C-10; ver §9 | Coordinator |
| Anexo A, decisión 1 (`:1188`) | Sí | «un `PaletteSet` por proceso, que RACKCAD, RK y RACKPANEL muestran; RackCad guarda en sus ajustes de usuario solo el tamaño y la posición de la ventana; con el foco en un campo, ninguna tecla llega a AutoCAD» | Con C-07 y C-10 |
| Anexo A, decisión 6 (`:1199`) | Sí | «**Navegación:** lista lateral de los racks del dibujo con buscador y filtros; un rack abierto a la vez; registros ligeros por fila; K superficies materializadas; la selección de AutoCAD abre el rack mientras "Seguir la selección" está encendido, que es el valor inicial» | Con C-02 |
| Anexo A, decisión 8 (`:1202-1203`) | Sí | Se añade: «"Actualizar todo", composición sobre esa costura: todo o nada sobre el authored de todos los racks incluidos, una transacción y un paso de UNDO» | Con C-05 |
| Anexo A, decisión 10 (`:1205-1206`) | Sí | «comandos clásicos sin cambio observable, salvo los cambios declarados: RACKCAD y RK abren la ventana; RACKEDITAR y los comandos de creación de cada sistema rediseñado abren su superficie nueva en una ventana aparte, con su camino clásico de escritura; adaptadores escalonados con caracterización D13/D9, sustituida por registro declarado donde el Owner cambia el comportamiento, y escritores caller-owned; diferir un sistema es del Owner» | Con C-07 y C-09 |
| Anexo A, decisión 11 (`:1207`) | Sí | «ninguna en el DWG; fuera de él, solo los ajustes de usuario y los archivos que el usuario pide (C-10); ni identidad estampada ni RackId inventado» | Con C-10 |
| Anexo A, consecuencias (`:1214-1215`) | Sí | «dos semánticas de Actualizar conviviendo (clásica y del panel)» → «tres: la clásica, la del rack y "Actualizar todo"»; «Insertar y `BindingIntent` siguen en el editor clásico» → «… hasta la A-n de la fase 8» | Con C-05 y C-06 |
| Anexo B, nota a ADR-0010 (`:1219-1221`) | Sí | «… escribe todo o nada el authored del rack —o, con "Actualizar todo", el de todos los racks incluidos— y no ofrece Insertar. Las decisiones de este registro para `RACKEDITAR` siguen vigentes sin cambio.» | Con C-05 (M-08) |
| Anexo B, nota a ADR-0032 (`:1223`) | Sí | «(cambiar de pestaña o de rack, Actualizar)» → «(cambiar el rack abierto, Actualizar o "Actualizar todo")» | Con C-02 y C-05 |

ADR-0047 (propuesto) copia el anexo A (`:18`, `:27`, `:30`, `:33`, `:34`). Se actualiza con estos textos cuando A-14 esté activa
(`docs/adr/README.md:37`).

## 5. Invariantes, pruebas y OV

**Invariantes existentes que se reescriben** al emitir la fase que los toca. El paquete de esa fase fija el texto exacto y el Coordinator lo
revisa:

| Invariante | Cambio | Fase |
|---|---|---|
| INV-02 (`:740`) | Los ciclos incluyen «RACKCAD repetido» y «RK repetido»; «10 pestañas» → «10 racks con borrador vivo» | 1, 2 |
| INV-03 (`:741`) | «activar una pestaña» → «abrir un rack»; «pestaña, preview ni lista» → «rack abierto ni lista» | 2 |
| INV-04 (`:742`) | Regla de preview → INV-A14-04 (registro de sustitución, C-03) | 2 |
| INV-05 (`:743`) | «Cambiar de pestaña, rack o documento» → «cambiar de rack abierto o de documento» | 2 |
| INV-07 (`:745`) | «pestañas» → «racks abiertos y listas» | 2 |
| INV-08 (`:746`) | Para «Actualizar todo», el ámbito por la unión de miembros (INV-A14-14) | 4 |
| INV-09 (`:747`) | «nuevas para pestaña y panel» → «para descartar, cambiar de rack y cerrar la ventana» | 2 |
| INV-12, INV-32 (`:750`, `:770`) | Se extienden a «Actualizar todo» (INV-A14-11, 12) | 4 |
| INV-15 (`:753`) | «N pestañas → K instancias» → «N racks abiertos sucesivamente → K instancias» | 2 |
| INV-16 (`:754`) | Sin cambio hasta la A-n de la fase 8 | 8 |
| INV-18 (`:756`) | «cambio de pestaña» → «cambio de rack abierto»; se añade «Actualizar todo» (INV-A14-15) | 2, 4 |
| INV-19 (`:757`) | Excepciones declaradas: RACKCAD y RK (fase 1); RACKEDITAR y creación por sistema rediseñado (C-09, INV-A14-17) | 1, 6, 7, 3b |
| INV-20 (`:758`) | Alcanza a los borradores nuevos (D-17) | 5, 8 |
| INV-21 (`:759`) | Excepciones nombradas de C-10 | 1, 5 |
| INV-24 (`:762`) | Todos los controles de cada editor alojado | 3, 6, 7, 3b |
| INV-25 (`:763`) | «la pestaña pasa a Error» → «el rack abierto pasa a Error» | 2 |
| INV-27 (`:765`) | Sin cambio: la preview de navegación se conserva (C-02, INV-A14-07) | 2 |
| INV-34 (`:772`) | «nunca crea pestañas» → «nunca abre racks ni borradores» | 2 |
| INV-36 (`:774`) | La guarda alcanza los archivos de las secciones (INV-A14-10) | 1, 5 |
| INV-37 (`:775`) | «el rechazo por admisión vive en la pestaña» → «en el área central del rack abierto» | 2 |
| INV-38 (`:776`) | «el contexto, la pestaña y el navegador» → «el contexto, el rack abierto y la lista» | 2 |
| INV-39 (`:777`) | Se extiende a la composición de «Actualizar todo» | 4 |

**Invariantes nuevos.** Los IDs son los definitivos de A-14; el prefijo «A14» evita choques con los INV de V5:

| ID | Invariante | RED esperado | Fase |
|---|---|---|---|
| INV-A14-01 | RACKCAD y RK abren o muestran la ventana y no abren el menú modal; con la ventana abierta, AutoCAD sigue usable | RACKCAD aún llama a `ShowModalWindow` con el menú | 1 |
| INV-A14-02 | Todo elemento del menú de RACKCAD tiene su entrada en la ventana, y hasta la fase 9 hay una entrada que abre el menú de hoy | un elemento sin entrada, o falta la de respaldo | 1 |
| INV-A14-03 | La ventana recupera tamaño y posición; un rectángulo ilegible o fuera de los monitores visibles da la colocación inicial; guardar no altera los demás ajustes; nunca escribe DWG, registro de Windows ni perfil | un rectángulo fuera de pantalla se restaura tal cual, o se pisa otro ajuste | 1 |
| INV-A14-04 | Cambiar el rack abierto nunca escribe el DWG ni pierde un borrador o un texto pendiente; un campo inválido aborta el cambio | el texto se pierde o el cambio ocurre con un campo inválido | 2 |
| INV-A14-05 | Búsqueda, filtros y «solo con cambios» usan solo el índice y el overlay: no leen el DWG | un filtro que lee el DWG | 2 |
| INV-A14-06 | «Seguir la selección» empieza encendido en cada sesión de documento; encendido sigue D-06; apagado, Uno(R) no cambia el rack abierto; nunca escribe selección ni vista | la sesión nace con el interruptor apagado, o apagado cambia el rack abierto | 2 |
| INV-A14-07 | Un rack abierto sin intención de edición muestra la lectura ligera, sin StaleBase ni AUTH-06, y no acepta entrada hasta la intención | abrir un rack captura StaleBase | 2 |
| INV-A14-08 | Un borrador Limpio se libera cuando su rack deja de estar abierto; uno dirty sigue vivo | los borradores Limpios se acumulan, o se pierde uno dirty | 2 |
| INV-A14-09 | «Abrir en editor clásico» existe para cualquier rack de la lista, ligado a su sesión, y avisa si hay borrador | un rack no integrado sin la acción, o sin el aviso | 2 |
| INV-A14-10 | Ningún archivo de las secciones referencia los tipos del índice, de `NavigationTargets` ni de los hechos neutrales, aunque esté en las carpetas de la ventana | la guarda pasa con una referencia desde una sección | 1, 5 |
| INV-A14-11 | «Actualizar todo»: un Conflicto, una admisión rechazada, un preflight fallido o un fallo antes de `Commit()` en cualquier rack da cero escrituras authored en todos; el resultado nombra cada rack con su causa, también «No aplicado: falló otro rack del conjunto» | un commit por rack deja racks escritos | 4 (y 6, 7, 3b) |
| INV-A14-12 | «Actualizar todo» es una transacción de MUTATE y un paso de UNDO; la frontera de `Commit()` clasifica el conjunto; la composición no abre transacciones anidadas ni cambia el contrato de la costura para Insertar | N transacciones o N pasos; una prueba existente de la costura en rojo | 4 (y 6, 7, 3b) |
| INV-A14-13 | «Actualizar todo» opera solo sobre los borradores de su sesión de documento | un borrador de otra sesión se escribe | 4 (y 6, 7, 3b) |
| INV-A14-14 | El ámbito de mutación propia cubre la unión de los miembros; un borrador no incluido conserva INV-28 | un borrador escrito queda marcado por sus propias pistas | 4 (y 6, 7, 3b) |
| INV-A14-15 | Un borrador con texto pendiente conservado (salvaguarda), o en Conflicto, Desconocido o Error, impide «Actualizar todo» sin escribir nada y sin comprometer su texto; el rack abierto compromete según D6 o aborta | se escribe con un borrador inelegible, o se compromete un texto conservado | 4 (y 6, 7, 3b) |
| INV-A14-16 | El editor alojado no anota ninguna petición clásica (OB-01.3) y ninguna tecla de sus campos llega a AutoCAD | un gesto anota `InsertRequested` | 3 (y 6, 7, 3b) |
| INV-A14-17 | En cada sistema rediseñado, las pruebas de la caracterización vieja siguen intactas y en `Skip` con el motivo que nombra su registro; el comportamiento nuevo tiene pruebas propias; RACKEDITAR conserva su camino clásico de escritura | una caracterización vieja editada para pasar | 6, 7, 3b |

**OV.** Retirar o sustituir un escenario exige al Owner; reasignarlo, una A-n (`:960`; LIFECYCLE §8).

| OV | Propuesta | Autoridad |
|---|---|---|
| OV-08 «Varias pestañas» | «Varios racks con borradores vivos en la lista lateral» | Owner |
| OV-09 «Preview y fijado» | «Rack abierto, preview de navegación y Seguir la selección de AutoCAD» | Owner |
| OV-11 «Cambiar de pestaña sin escribir» | «Cambiar de rack abierto sin escribir» | Owner |
| OV-14 «Selección (N) sin abrir pestañas» | «Selección (N) sin abrir racks ni borradores» | Owner |
| OV-17 «Compatibilidad con RACKEDITAR» | «RACKEDITAR abre, en una ventana aparte, la superficie de su sistema (la rediseñada, desde la fase de ese sistema) y conserva su camino clásico de escritura» | Owner |
| OV-20 «Cerrar el panel o una pestaña no modifica el dibujo» | «Cerrar la ventana, cambiar de rack o descartar no modifica el dibujo» | Owner |
| OV-04, OV-05 | Sin cambio; OV-05 se entrega en la fase 2 | — |

**OV nuevos** (IDs definitivos de A-14; autoridad: la de A-14, LIFECYCLE §6 `:204-205`):

| OV | Escenario |
|---|---|
| OV-A14-01 | RACKCAD y RK abren la ventana; la entrada del menú de hoy funciona |
| OV-A14-02 | La ventana recuerda tamaño y posición, también acoplada o flotante; con un monitor retirado, aparece en un sitio visible |
| OV-A14-03 | La lista con los racks reales del Owner, en sus dibujos más grandes (al menos uno de unos 70 racks): buscador, filtros, «solo con cambios» y «Seguir la selección» |
| OV-A14-04 | «Abrir en editor clásico» desde la lista, para un rack de un sistema aún no migrado, con su limitación declarada (bloquea) |
| OV-A14-05 | El editor de un sistema en el área central, sin bloquear AutoCAD: foco y teclado dentro del editor (por sistema) |
| OV-A14-06 | «Actualizar todo»: éxito con varios racks y un solo U; un rack inelegible o en conflicto, sin que se escriba nada y con la lista de lo que falló; progreso visible (por sistema) |
| OV-A14-07 | Las secciones dentro de la ventana, sin bloquear |
| OV-A14-08 | «Nuevo» en su paso intermedio: abre la ventana del sistema y crea como hoy |
| OV-A14-09 | RACKEDITAR y la creación de un sistema rediseñado abren su superficie nueva en una ventana aparte y escriben como hoy |

**Asignación por tramo** (PROPUESTA, la regla de C-12 aplicada al corte propuesto):

| Tramo | OV de su Owner Validation |
|---|---|
| 1 (fases 0 a 4) | OV-01 a OV-06, OV-08 a OV-26 y OV-A14-01 a 06 y 08, con Push Back como sistema editable |
| 2 (fase 5) | OV-A14-07. Si Variables entra en este tramo, además OV-10, OV-12, OV-13, OV-21 y OV-A14-06 con borradores de variables; si no, el tramo y los OV de Variables los fija su A-n (C-11) |
| 3 (fase 6) | Para Selectivo, Dinámico y Cantilever: OV-09, OV-10, OV-11, OV-12, OV-13, OV-19, OV-23, OV-24, OV-25, OV-26, OV-A14-05, OV-A14-06, OV-17 y OV-A14-09 |
| 4 (fases 7 y 3b) | Lo mismo para Cabecera, Cama y Push Back rediseñado |
| 5 (fases 8 y 9) | Los OV que fijen las A-n de la fase 8 y de la lista de retiro; OV-07 (varios sistemas); OV-22 (los registros completos de rendimiento) |

Cada tramo ejecuta los suyos y el checklist de la guía sobre su `FINAL_CANDIDATE_SHA`. OV-17 va al tramo 1 con el texto sustituido:
RACKEDITAR abre su superficie de hoy hasta la fase de cada sistema.

## 6. Plan de fases

**Reglas comunes:**
- Cada fase necesita su **propio paquete con revisión independiente** y la **orden del Owner** (y del Coordinator) antes de emitirse, como
  F4B (F4B-E01, F4B-E02). Sigue la cadena de F4 y F4B: contrato de gate por tarea, planificación, Worker con RED y GREEN, CI exacta,
  verificación, revisiones, controles y cierre.
- El presupuesto por tarea lo fija la orden de cada fase (en F4B: 1/3 por tarea, y el intento 3 vuelve al Owner, D-PB-10).
- Cada hito del Owner necesita su propio Candidato completo (§12.1 `:828-829`), con las reglas de host de §12.1.
- Se integra por tramos (C-12). Ninguna fase depende de I-63 (MASTER-I63-I64-02 §6).

| Fase | Objetivo | Depende de | Gate de V5 | Hito del Owner |
|---|---|---|---|---|
| **0 · F4B** | Cerrar PB-T5 tal como está emitida | — | IC-11; sin cambio | Smoke-PB (en curso) |
| **1 · La ventana** | Columna lateral con las secciones (en su paso intermedio) y todo el menú de RACKCAD, más la entrada de respaldo; «Nuevo» abre las ventanas actuales; RACKCAD y RK abren la ventana; tamaño y posición (C-10). Antes de nada, mide cómo lanzar flujos clásicos desde la ventana (R-13). README (WORKFLOW §8) | 0 | Nueva; sustituye al menú modal | Smoke corto, con S1-11 repetido |
| **2 · Racks del dibujo** | La lista (C-02, C-03, C-04): estado, buscador, filtros, «solo con cambios», «Seguir la selección», Selección (N), preview de navegación, «Abrir en editor clásico» para cualquier rack (Q-10); al menos 70 racks; registro de sustitución de las pruebas de F3 | 1 | **F6** (sube y se amplía); sustituye la tira de F3 sin reabrirla; adelanta IC-03 | — |
| **3 · Push Back en la ventana** | C-08: el editor en el área central, sin bloquear; «Editar todo» deja de ser modal | 2 | Punto 1 de D-15 para Push Back; sustituye las frases modales de D-PB-01 | Smoke principal (con la 4) |
| **4 · Actualizar todo** | C-05, solo racks, con progreso visible; las condiciones 1 a 4 | 3 | Nueva; saca del residual de ID3 la mutación atómica de varios racks | Smoke principal; con el corte propuesto es la última fase del tramo 1, que cierran su READY y su Owner Validation |
| **5 · Secciones** | C-11: BOM y lista, Biblioteca y Configuración dentro de la ventana; Variables con su borrador y su A-n | 1 (BOM y lista, Biblioteca, Configuración); 4 y su A-n (Variables) | Nueva | — |
| **D · Diseño por sistema** | Sin código: un prototipo de UI/UX por sistema que aprueba el Owner. Orden: Selectivo, Dinámico y Cantilever (antes de la 6); Cabecera y Cama (antes de la 7); Push Back (antes de la 3b) | — (en paralelo desde ya) | No es un gate (LIFECYCLE §7); da la decisión que pide ADR-0029 D13 | Revisión del prototipo |
| **6 · Selectivo, Dinámico, Cantilever** | Sus editores en la ventana, con su diseño nuevo; RACKEDITAR y su creación los adoptan (C-09); «Actualizar todo» para ellos | 3, 4 y sus prototipos | **F5-A** (sustituye): criterios por sistema (`:794`) y la variante caller-owned de Cantilever | Smoke |
| **7 · Cabecera y Cama** | Lo mismo | 6 y sus prototipos | **F5-B** (sustituye) | — |
| **3b · Push Back rediseñado** | Su diseño nuevo; reutiliza el estado y el escritor de F4B; RACKEDITAR lo adopta (C-09) | 4 y su prototipo | Nueva | Smoke |
| **8 · Nuevo en la ventana** | «Nuevo, sin insertar» e insertar desde la ventana | 3, 6 y su A-n (C-06) | Nueva | — |
| **9 · Retiro de comandos** | Según la lista del Owner, incluida la entrada de respaldo | 5 a 8, la 3b y la lista con su A-n | Nueva; cambia D-16 e INV-19 para la lista | — |
| **F7 · Smoke-PERF · READY** | En cada tramo, su equivalente (C-12); en el último, F7, Smoke-PERF y READY completos | Las fases de su tramo | **F7**, **Smoke-PERF** y **READY** (se conservan, por tramo) | Smoke-PERF; OV del tramo |

Orden de la replaneación: 0 → 1 → 2 → 3 → 4 → 6 → 7 → 3b → 8 → 9 → F7. La línea D corre en paralelo desde ya; la fase 5 avanza en paralelo
desde la 1 en lo que no escribe el DWG. La fase 6 depende también de la 4 porque sus sistemas entran en «Actualizar todo» (C-05).

**Correspondencia de gates de V5 con las fases de A-14:**

| V5 | Estado (I-64.yml) | A-14 |
|---|---|---|
| F1 y Smoke-1 | CLOSED / PASS | Sin cambio; no se reabren |
| F2 y Smoke-NAV | CLOSED / PASS WITH LIMITATIONS | Sin cambio; IC-01 sigue; IC-02, con la sustitución de C-03 |
| F3 | CLOSED / PASS WITH LIMITATIONS | No se reabre; la fase 2 sustituye resultados con registro (C-03) |
| F4 y Smoke-2 | CLOSED / PASS WITH LIMITATIONS | Sin cambio |
| F4B (IC-11) | PB-T5 emitida | Fase 0, sin cambio |
| F5-A | «F5-A remains authorized but WAITING» (OD `:269`); «AUTHORIZED / WAITING FOR F4B» (OD `:293`) | Fase 6. La autorización queda sustituida por la orden propia de la fase 6 (en la firma, §11) |
| F5-B | pendiente | Fase 7 |
| F6 | pendiente | Fase 2 |
| — | — | Nuevas: 1, 3, 4, 5, 3b, 8 y 9; línea D |
| F7, Smoke-PERF, READY | pendientes | Por tramo (C-12); los completos en el último |

## 7. Riesgos

| ID | Riesgo | Tratamiento |
|---|---|---|
| R-01 | El editor de Push Back es grande para una paleta (H-PB-10) | Ventana redimensionable; la fase 3 empieza midiendo; contingencia (b) de D-01 por A-n material |
| R-02 | Teclado y foco con un editor completo en la paleta. En Smoke-1, un Ctrl+V lanzó PASTECLIP; se clasificó como incidente de entrada del operador, no como defecto de producto | Obligación de foco congelada; la fase 3 la prueba en todos los controles (S2-16 a S2-22 de A-7; INV-A14-16) |
| R-03 | 70 racks en un dibujo | Arnés de la fase 2 con al menos 70; SP-03 en los dibujos más grandes del Owner |
| R-04 | «Actualizar todo» puede tardar y bloquea el documento; un «Actualizar» grande de un rack se mide en SPB-23 (Smoke-PB en curso, sin versionar) | Condición 2 de C-05; progreso visible; cancelar solo antes de `Commit()` |
| R-05 | Ventanas pequeñas que siguen siendo modales (Configurar cabecera, Seguridad, BOM, Guardar en biblioteca) | Se quedan modales al principio (subdiálogos de D-02) y se listan para sus fases |
| R-06 | RACKEDITAR cambia un rack que tiene borrador en la ventana | Resuelto en F4: «posiblemente obsoleto» y Conflicto al «Actualizar» (D-12, D-16) |
| R-07 | El rediseño cambia la forma de trabajar con cada sistema, también en RACKEDITAR (C-09) | Prototipo aprobado antes de programar; smoke por sistema; cambio declarado con registro; la escritura clásica de RACKEDITAR no cambia |
| R-08 | El coste del proceso | La fase 1 es ligera; la 2 lleva F6 (Deep en V5 §13 `:982`), el arnés de 70 racks y el registro de sustitución de F3; la 5 no es ligera si lleva Variables (C-11); la 3 y la 4 son las grandes. Cada tramo añade un ciclo completo de unidad: bootstrap, Discovery delta, READY, Owner Validation e integración (C-12) |
| R-09 | Variables en «Actualizar todo»: dos autoridades (el registro de ADR-0034 y los sobres); un cambio de variable alcanza a racks sin borrador; `FindRackBlocks` frente a `RackSiblingMembership.Classify`; la StaleBase de Selectivo incluye el registro; hoy RACKVARIABLES escribe al momento (D-04 `:215-216`) | La A-n de Variables, antes de su paquete (C-11) |
| R-10 | Privacidad del índice frente a «BOM y lista de racks»: el prototipo muestra «70 racks» en la cabecera, un recuento de proyecto | La sección consume RACKLISTA y RACKBOMTOTAL; INV-A14-10; los rótulos del índice siguen INV-36; SP-06 |
| R-11 | Rama de vida larga | Integración por tramos (C-12) |
| R-12 | Restaurar la ventana fuera de la pantalla | INV-A14-03 |
| R-13 | Lanzar flujos clásicos que piden puntos o abren modales desde la ventana («Nuevo», «Abrir en editor clásico», la entrada del menú de hoy, las secciones): IC-01 midió que la vía en contexto de comando no dio el efecto de navegación; para estos flujos la vía no está medida (`UNKNOWN`) | La fase 1 lo mide antes de implementar; sin efectos fuera de la sesión (D-03) |
| R-14 | La composición de «Actualizar todo» no cabe sobre la costura de I-55 sin cambiarla | Condición de C-05: STOP y autoridad del Freeze de I-55 |
| R-15 | La fase 8 choca con el no-objetivo de Insertar, con D-22 e INV-16 y con D-PB-04 | Su propia A-n (C-06) |
| R-16 | El veto de cierre (D-17) tiene que cubrir los borradores nuevos | INV-20 se extiende en las fases que los crean |
| R-17 | La evidencia con dibujos reales (Smoke-PERF, prototipos) puede llevar nombres de racks, proyectos o clientes, y el repo es público | Se redacta antes de custodiar; lo que no se pueda redactar se custodia solo por SHA-256 (precedente: la lista local de Smoke-2, PB-T5 v2.4 `:17`) |

## 8. Lo que A-14 no hace

- No reabre F1, F2, F3, F4 ni F4B, ni sus hitos; lo que la fase 2 sustituye de F3 va con registro (C-03).
- No cambia el contrato de PB-T5 ni el Smoke-PB en curso; SPB-03, SPB-12 y SPB-13 conservan su resultado.
- No retira ningún comando ni decide la lista de retiro.
- No autoriza emitir ninguna fase: cada una necesita su paquete, su revisión y la orden del Owner.
- No toca `src/` ni `tests/`, ni `main`.
- No cambia la semántica de escritura cerrada para un rack (PREPARE / MUTATE / POST, todo o nada por rack, clasificación por frontera, un paso
  de UNDO) ni la costura de I-55. «Actualizar todo» es una composición encima (C-05).
- No decide la mecánica de «Actualizar todo»: fija su resultado, sus condiciones y quién la aprueba.
- No cambia el host ni la obligación de foco de D-01, ni D-11, D-12 o D-19, ni las reglas de host de §12.1.
- No modifica el texto de A-1..A-13. Solo amplía el alcance de A-7 a las unidades de los tramos (cabecera); los puentes A-1..A-6 y
  A-8..A-13 siguen siendo de la unidad I-64. Tampoco modifica IC-01, IC-04 ni IC-06 a IC-11. IC-03 queda sustituida por la decisión Q-10, y las obligaciones de IC-02 e IC-05 que
  nombran pestañas, por C-03.
- No incluye «Abrir RackCad al iniciar AutoCAD» (Q-08).
- No decide la fase 8, las Variables en la ventana ni la lista de retiro: cada una va por su A-n.
- No fija umbrales de rendimiento (D-18).
- No clasifica ID3: va en el READY del último tramo.
- No acepta ADR-0047: fija su valor por defecto (sigue `propuesto`) y quién decide aceptarlo y cuándo (C-12).
- No sustituye de D-PB-01 más que sus frases sobre la ventana modal (C-08).

## 9. Materialidad

Propuesta del borrador; la confirma el Coordinator (LIFECYCLE §3). El arquetipo sigue siendo NEW ARCHITECTURE.

| ID | Estado | Razón |
|---|---|---|
| M-01 | **YES** | «Actualizar todo» acepta una escritura de varios racks; la ventana guarda su rectángulo; el borrador de variables (en su A-n) |
| M-02 | **YES** | Se rompe la condición de §14 `:995` («sin campos nuevos … (D-22)»): las excepciones de C-10; la fase 8 tocaría la identidad |
| M-03 | **YES** | Se rompe la condición de §14 `:996`: RACKCAD y RK (fase 1); RACKEDITAR y la creación por sistema rediseñado (C-09); la fase 9 |
| M-04 | **YES** | Todo o nada sobre varios racks, Desconocido para el conjunto, la elegibilidad, la lista de lo que falló; el cierre con dirty cambia de forma (C-02) |
| M-05 | **YES** | Se realinea el contrato de adaptador (C-08) y cambia la caracterización por sistema (C-09); la composición consume la costura de I-55 para N racks |
| M-06 | **YES** | Se implementa el punto de extensión del borrador por lotes (D-23); Insertar, en la A-n de la fase 8 |
| M-07 | **YES** | La ventana aloja la operación de RackCad |
| M-08 | **YES (por prudencia)** | La nota a ADR-0010 cambia (anexo B); Variables toca ADR-0034 (en su A-n); el rediseño ejerce ADR-0029 D13 con decisión del Owner |

```text
M-01 = YES
M-02 = YES
M-03 = YES
M-04 = YES
M-05 = YES
M-06 = YES
M-07 = YES
M-08 = YES (por prudencia)

Owner = REQUIRED (OWNER-RESERVED: §11)
Architect = REQUIRED (PENDING; sin orden de revision)
Coordinator = PENDING
```

## 10. Camino de aprobación

1. **Hecho:** la revisión independiente adversarial del v1 (APTO TRAS CORRECCIONES), la v2 con su disposición, la comprobación acotada
   de la v2 (EMITIBLE TRAS CORRECCIONES), la v2.1 con la suya, la comprobación de la v2.1 (EMITIBLE TRAS CORRECCIONES) y esta v2.2 con la
   suya.
2. El Coordinator resuelve dónde se versionan las fuentes (Q-C1).
3. Se versionan **en un mismo commit**, con lo que no se pueda redactar custodiado solo por SHA-256 (R-17):
   - esta enmienda, como `docs/initiatives/I-64-A-14.md`, PROPOSED;
   - el registro literal de las decisiones del Owner del 2026-11-23;
   - la replaneación «Replaneación de I-64»;
   - el prototipo «RackCad tipo navegador» v3, redactado;
   - todas las revisiones y comprobaciones independientes de A-14 hechas hasta ese commit (la del v1 y las de la v2, la v2.1 y las que
     siga habiendo) y sus tablas de disposición, con la disposición por ID que pide LIFECYCLE §5 `:162-163`.

   Desde ese commit A-14 es append-only.
4. El Coordinator confirma la materialidad y la autoridad (Q-C3) y ordena **una** revisión formal del Architect sobre ese commit, con la
   celda acreditada o una sonda nueva si la elegibilidad cambió (Q-C2).
5. El Architect revisa en solo lectura la versión completa: V5, A-1..A-14, IC-01..IC-11, el paquete de F4B y las fuentes consumidas (LIFECYCLE §5).
   Si pide cambios, la corrección es **A-15**: A-14 no se edita (precedente: A-12 → A-13; A-7 «sin editar»).
6. Coordinator AGREED.
7. El Owner firma la elección de §11.
8. Activación en el estado de I-64. A-14 no cambia F4B, así que puede activarse con F4B abierta; el paquete de la fase 1 espera a que F4B
   cierre.

## 11. La firma del Owner

Se pide después de Architect AGREED y Coordinator AGREED, sobre el commit exacto (PENDING). **Elección exacta que se pide** (la firma es
sobre esta frase; la lista de debajo es una guía):

> «SATISFIED — autorizo I-64-A-14 tal como está en el commit `<SHA>`, incluidas sus materias OWNER-RESERVED:
> (1) el objetivo y su alcance verificable, con las cláusulas del brief que cambia (C-01);
> (2) la lista lateral sin pestañas, con "Seguir la selección" encendido al empezar, y "Abrir en editor clásico" para cualquier rack
> desde la fase 2, con su limitación declarada, que sustituye a IC-03 (C-02);
> (3) la sustitución de las obligaciones de IC-02 e IC-05 que nombran pestañas (C-03);
> (4) "Actualizar todo", con su regla de elegibilidad, y el residual nuevo de ID3 (C-05);
> (5) "Nuevo" (C-06);
> (6) RACKCAD y RK abren la ventana, con la entrada del menú de hoy hasta la fase 9, y RACKPANEL se conserva (C-07);
> (7) de D-PB-01, la sustitución solo de "as a modal/full-size editing surface" (OD `:25-26`), de OD `:34` y de OD `:37`, desde el cierre
> de la fase 3, conservando el resto de D-PB-01 (C-08);
> (8) RACKEDITAR adopta el diseño nuevo de cada sistema, con cambio declarado, y confirmo las dos lecturas de la sesión: que alcanza
> también a los comandos de creación de ese sistema, y que la orden de emisión de cada fase rediseñada, con su registro de cambios
> declarados, completa la decisión que pide ADR-0029 D13 (C-09);
> (9) el tamaño y la posición de la ventana, con la lectura de la sesión que confirmo de que "posición" incluye acoplada o flotante y
> en qué lado, y las cuatro excepciones de D-22, incluida la ruta de la biblioteca de bloques, que la ventana guarda desde la fase 1
> (C-10);
> (10) las secciones, incluida Variables con su propia A-n (C-11);
> (11) la integración por tramos con la regla de C-12, el Smoke-PERF completo con mis dibujos reales en el READY de cada tramo salvo una
> A-n que lo reduzca, y estos hitos: [tramo 1 = fases 0 a 4, como se propone | tramo 1 = fases 0 a 3, la alternativa | los que corrijo:
> …]; si elijo la alternativa o corrijo los hitos, la tabla de tramos y de OV que resulte la versiona la A-n siguiente antes del READY-01
> del primer tramo afectado;
> (12) la sustitución de OV-08, OV-09, OV-11, OV-14, OV-17 y OV-20 (§5);
> (13) que la autorización de F5-A queda sustituida por la orden propia de la fase 6.»

La elección de hitos de la materia (11) no es un rechazo: es parte de la firma y sigue la regla de C-12. Si el Owner no acepta alguna otra
materia, lo dice al firmar y A-14 no se activa: la corrección es la A-n siguiente, que la sustituye para la activación (precedente: A-12 →
A-13).

**Pregunta para la firma (P-1), fuera de la frase:** ¿se amplía a las unidades de los tramos 2 y siguientes alguno de los puentes de
protocolo vigentes de la unidad I-64, A-4 (I64-SCOPE-BRIDGE-01, Scope) o A-6 (I64-FREETEXT-BRIDGE-01, FreeText)?
- **Por defecto, no.** Siguen siendo de la unidad I-64, que el Owner autorizó «only for this I-64 unit-scoped exception» (A-2, A-3 y A-4
  `:33`; A-5 `:35`; A-6 `:33`), y A-14 no los toca.
- Si una unidad nueva necesita uno, lo pide su propia A-n, con el Owner y el dueño del protocolo, como hicieron A-2..A-6.
- Si el Owner quiere ampliarlo ya, lo dice al firmar. La ampliación es `OWNER-RESERVED` y va por la A-n siguiente: A-14 no la hace.

**En lenguaje llano, qué apruebas:**

1. RackCad se usa desde una sola ventana que no bloquea AutoCAD: allí está el menú de hoy, cuatro secciones, «Nuevo» y la lista de los
   racks del dibujo.
2. No hay pestañas. A la izquierda hay una lista con buscador y filtros, y se abre un rack cada vez. Si seleccionas un rack en el
   dibujo, se abre en la ventana; puedes apagarlo.
3. Desde la fase 2, cualquier rack de la lista se puede abrir en su editor de hoy, que seguirá bloqueando AutoCAD hasta que su sistema se
   pase a la ventana.
4. Unas comprobaciones que decías que se harían «con pestañas» se hacen con la lista.
5. «Actualizar todo» escribe de una vez todos los racks con cambios, o ninguno, con un solo Deshacer y viendo el progreso. Si un rack
   tiene un conflicto, un error o un estado por comprobar, no se escribe nada y se te dice cuál y por qué. Primero solo racks; las
   variables, después.
6. «Nuevo», por ahora, abre la ventana de siempre del sistema. Crear dentro de la ventana se decide más adelante.
7. RACKCAD y RK abren la ventana nueva. Hasta el retiro de comandos, la ventana tiene un botón que abre el menú de hoy. RACKPANEL sigue.
8. Cuando termine la fase 3, «Editar todo» de Push Back deja de abrir una ventana que bloquea. Todo lo demás que decidiste para él sigue:
   un solo editor, que solo cambia el borrador y nunca ejecuta por error la escritura de siempre.
9. **(Confirmas una lectura nuestra.)** Cuando un sistema tenga su diseño nuevo, RACKEDITAR abrirá esa misma pantalla nueva en una
   ventana aparte, como decidiste. Entendemos que también los comandos que crean ese sistema, porque usan la misma ventana. Los dos
   seguirán escribiendo en el dibujo como hoy. Antes de esa fase te llegará la lista de cada cambio, y lo que no esté en la lista no se
   hace.
10. **(Confirmas una lectura nuestra.)** La ventana recuerda su tamaño y su sitio: entendemos que también si estaba acoplada o flotante, y
    en qué lado. RackCad guarda en tus ajustes solo eso, la ruta de la biblioteca de bloques (como ya hace el menú de hoy), lo que cambies
    en Configuración y los archivos que tú exportes o guardes. Nada nuevo en el dibujo.
11. Las secciones (Variables, BOM y lista de racks, Biblioteca, Configuración) entran en la ventana en la fase 5. Variables, con una
    enmienda propia.
12. I-64 llega a `main` por tramos. Cada tramo tiene su candidato, tu validación con escenarios de lo que entra y **la prueba de
    rendimiento completa, con tus dibujos reales**: una sesión tuya en AutoCAD por tramo, cinco con la propuesta. Se puede reducir más
    adelante, pero solo con una enmienda que decides tú.
    - **Propuesta:** el primer tramo llega hasta «Actualizar todo». La prueba principal queda entera, pero la primera entrega a `main` es
      más tarde: espera a la fase de más riesgo.
    - **Alternativa:** acabar antes, con Push Back en la ventana. Llega antes, pero hace falta un tramo más.
    - Si eliges la alternativa o cambias los tramos, hace falta una enmienda más antes de cerrar el primer tramo afectado.
13. Seis escenarios de la validación final que hablaban de pestañas o de RACKEDITAR se cambian por sus equivalentes nuevos.
14. La autorización que ya diste a la fase de Selectivo, Dinámico y Cantilever se sustituye por una orden nueva cuando llegue su turno.

**Y una pregunta aparte (P-1):** los dos atajos de verificación que autorizaste solo para I-64, el de Scope y el de FreeText, ¿valen
también para los tramos siguientes? Si no dices nada, no valen: cada tramo que los necesite te lo pedirá por separado.

## 12. Pendientes y decisiones pendientes de autoridad

**Decisiones pendientes de autoridad** (las tomó el redactor; ninguna es del Owner hasta que la confirme quien se indica):

| # | Decisión del redactor | Dónde | Quién decide | Cuándo |
|---|---|---|---|---|
| 1 | Corte de tramos propuesto: tramo 1 = fases 0 a 4 | C-12 | Owner (el Coordinator, la viabilidad) | En la firma, materia (11) |
| 2 | El tramo 1 es la unidad I-64; los siguientes, unidades nuevas | C-12 | Coordinator | En la orden de revisión del Architect (§10, paso 4) |
| 3 | `Applies-to: all` de A-14 y la ampliación del alcance de A-7 (solo A-7; los puentes no se amplían) | Cabecera | Coordinator + Architect (la autoridad de A-7) | Orden de revisión (paso 4) y revisión formal (paso 5) |
| 3b | Ampliar algún puente de protocolo (A-4, A-6) a las unidades nuevas | §11, P-1 | Owner y el dueño del protocolo, + Coordinator + Architect; por defecto, no se amplía | En la firma (P-1); la ampliación, por la A-n siguiente |
| 4 | La regla de lectura de «pestaña», su lista y su precedencia | C-03 | Coordinator + Architect | Revisión formal (paso 5) |
| 5 | Las pruebas de F3 que se retiran, en `Skip` con registro | C-03 | Coordinator + Architect | Revisión formal; el registro, en el paquete de la fase 2 |
| 6 | La vida del borrador Limpio | C-02 | Architect + Coordinator | Revisión formal (paso 5) |
| 7 | La elegibilidad de «Actualizar todo», con la condición 3 como salvaguarda | C-05 | Architect + Coordinator; el Owner la ve en la firma, materia (4) | Revisión formal y firma |
| 8 | Quién aprueba la composición de «Actualizar todo» | C-05 | Architect + Coordinator | Revisión formal (paso 5) |
| 9 | La fase 6 depende también de la 4 | §6 | Coordinator | Coordinator AGREED (paso 6) |
| 10 | La extensión de Q-03 a los comandos de creación | C-09 | Owner | En la firma, materia (8) |
| 11 | Que la orden de cada fase rediseñada, con su registro, complete la decisión de ADR-0029 D13 | C-09 | Owner | En la firma, materia (8), y en cada orden de fase rediseñada |
| 12 | «Posición» incluye acoplada o flotante y el lado | C-10 | Owner | En la firma, materia (9) |
| 13 | Las cuatro excepciones de D-22, con la ruta de la biblioteca de bloques desde la fase 1 y los diseños guardados | C-10 | Owner + Architect + Coordinator | Revisión formal y firma, materia (9) |
| 14 | Variables, con su propia A-n | C-11 | Coordinator; la A-n, Owner + Architect + Coordinator | Coordinator AGREED; la A-n, antes del paquete que lleve Variables |
| 15 | La regla de OV por tramo, su tabla, y los IDs definitivos de INV-A14 y OV-A14 | C-12, §5 | La autoridad de A-14 (Architect + Coordinator); el Owner, en la firma, materias (11) y (12) | Revisión formal y firma |
| 16 | Smoke-PERF completo por tramo como valor por defecto | C-12 | El Owner, a propuesta del Coordinator | En la firma, materia (11); una reducción, por A-n |
| 17 | ADR-0047 sigue `propuesto` por defecto | C-12 | Owner (lo acepta); el Architect, la forma | En el READY de cada tramo |
| 18 | La lista de retiro va por su A-n e incluye la entrada de respaldo | C-07 | Owner (la lista); la A-n, Owner + Architect + Coordinator | Antes del paquete de la fase 9 |
| 19 | IC-03 sustituida por Q-10; IC-02 e IC-05 por C-03 | C-02, C-03 | Owner | En la firma, materias (2) y (3) |
| 20 | D-07 en «Amends» por su texto | Cabecera, §4 | Coordinator | Coordinator AGREED (paso 6) |

**Pendientes, con el documento que los cierra y cuándo:**

| Pendiente | Quién | Documento que lo cierra | Cuándo |
|---|---|---|---|
| Q-C1: dónde se versionan las fuentes del paso 3 (WORKFLOW §10 `:398` apunta a `docs/automation/decisions/<I>.md`, que no existe para I-64; I-64 los ha guardado en sus carpetas de evidencia) | Coordinator | La orden del Coordinator que autoriza el commit del paso 3 | Antes del paso 3 |
| Q-C2: la celda del Architect (la de A-8..A-13 se acreditó sobre un binario y una configuración que ya no rigen) | Coordinator | La orden de revisión del Architect, con una sonda si hace falta | Paso 4 |
| Q-C3: la materialidad y la autoridad de §9 | Coordinator | La orden de revisión del Architect | Paso 4 |
| Q-C4: el nombre de cada unidad de los tramos 2 en adelante | Coordinator | El bootstrap de cada unidad (WORKFLOW §2 y §4.1) | Al abrir cada tramo |
| La revisión formal, en especial la composición de «Actualizar todo» (C-05) y la vía de C-10 | Architect | Su veredicto sobre el commit del paso 3 | Paso 5 |
| La elección de hitos, o su corrección | Owner | La firma (§11); si cambia la tabla, la A-n siguiente | Firma; la A-n, antes del READY-01 del primer tramo afectado |
| Una reducción de Smoke-PERF en un tramo | El Owner, a propuesta del Coordinator | La A-n siguiente | Antes del READY-01 de ese tramo; sin ella, rige el valor por defecto |
| P-1: ampliar algún puente de protocolo a las unidades nuevas | Owner y el dueño del protocolo | La firma (§11); la ampliación, la A-n siguiente | Firma; por defecto no se amplía |
| La aceptación de ADR-0047 | Owner | El registro de cierre del tramo | En el READY de cada tramo; sin decisión, sigue `propuesto` |
| El registro de sustitución de F3 | Coordinator + Architect | El paquete de la fase 2 | Antes de su contrato |
| La composición de «Actualizar todo» | Architect + Coordinator | El paquete de la fase 4 | Antes de su contrato |
| El registro de cambios declarados de cada sistema rediseñado | Owner | La orden de emisión de la fase 6, 7 o 3b | Al emitir cada una |
| Variables en la ventana | Owner + Architect + Coordinator | Su A-n | Antes del paquete que la lleve |
| La fase 8 | Owner + Architect + Coordinator | Su A-n | Antes de su paquete |
| La lista de retiro | Owner; la A-n, Owner + Architect + Coordinator | Su A-n | Antes del paquete de la fase 9 |
| El commit de A-14 y su sección de evidencia | La sesión | El commit del paso 3 | Paso 3 |

## 13. Evidencia

- **Freeze:** `docs/initiatives/I-64-proposal-v5.md`, blob `dc1924ff5a73a4e529fbc5f995621d29351a091b`.
- **F4B:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`;
  `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` (F4B-E01); `docs/initiatives/I-64-f4b-push-back-completo-package.md`;
  `docs/initiatives/I-64-implementation-clarifications.md` (IC-02, IC-03, IC-05, IC-11);
  `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (patrón de sustitución).
- **PB-T5:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md` (SPB-03, SPB-12, SPB-13, SPB-23).
- **Producto:** `src/RackCad.Plugin/RackMenuCommands.cs`; `src/RackCad.UI/RackMainMenuWindow.xaml`;
  `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs`; `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs`;
  `src/RackCad.Application/Settings/UserSettings.cs`; `src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs`.
- **Decisiones del Owner, replaneación, prototipo, todas las revisiones y comprobaciones independientes y sus disposiciones:** se versionan
  con A-14, en la lista del paso 3 de §10; las rutas, Q-C1.
- **Evidencia de la unidad:** la sección de `docs/automation/evidence/I-64-evidence.md` que registre A-14: PENDING.

Este artefacto es append-only desde el commit que lo versiona (§10, paso 3). Cualquier corrección, revocación o sustitución requiere la
siguiente A-n.
