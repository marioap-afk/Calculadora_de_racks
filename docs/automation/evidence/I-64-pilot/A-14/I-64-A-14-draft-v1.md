# I-64 Amendment A-14 — Replaneación hacia la ventana RackCad sin bloqueo (borrador v1)

```text
Amendment: A-14

Initiative / Unit: I-64

Freeze:
commit = 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311 (imagen en la rama actual tras los rebases: 55e7ca6337fef6dcc14f5c40bdd55daf88d78cbf)
path = docs/initiatives/I-64-proposal-v5.md
blob = dc1924ff5a73a4e529fbc5f995621d29351a091b

Applies-to: I-64

Amends (Freeze V5):
§1 (objetivo; no-objetivos de crear racks, edición por lotes, persistencia de la posición y comandos; criterios de éxito 5, 11 y 12);
D-01 (solo el comando que muestra la paleta); D-02 (creación del controlador); D-03 (contenido de la sesión); D-06 (efectos de
Uno(R)); D-08; D-09; D-13 (solo para «Actualizar todo»); D-15 (realineación y rediseño); D-16; D-20; D-22 (excepción acotada);
D-23 (puntos de extensión que pasan a implementarse); §10 (invariantes afectados); §11 (plan de gates); §12.5 (solo el dibujo
de SP-03); §12.6 (OV-08, OV-09, OV-14 y escenarios nuevos).
Sin cambio: D-01 en el host, la contingencia (b) y la obligación de foco; D-03 en su semántica; D-04; D-05; D-07; D-10; D-11;
D-12; D-14; D-17 en su semántica (alcanza a los borradores nuevos, R-16); D-18; D-19; D-21; §12.1.

Corrects / supersedes:
Ninguna A-n anterior. Añade, no sustituye: A-1..A-13 no cambian (A-7 sigue activa: S2-16..S2-22).

Relation to F4B (IC-11; F4B-E01):
F4B no se reabre ni cambia: ni su paquete (v3.1), ni D-PB-01..D-PB-10, ni el contrato de PB-T5 ni el Smoke-PB en curso.
D-PB-01 («Editar todo» modal) pasa a ser un paso intermedio: rige tal cual hasta que cierre la fase 3 de A-14.

Authority (propuesta del borrador; la confirma el Coordinator, LIFECYCLE §3 y §6):
Owner + Architect + Coordinator. Owner por lo OWNER-RESERVED (alcance, no-objetivos, sustitución de OV, texto literal de
D-PB-01); Architect + Coordinator por las M materiales (sección «Materialidad»).

Owner:
Decisiones del 2026-11-23 sobre la replaneación: TOMADAS (registro literal; ruta en el repo PENDING).
Firma del texto exacto de A-14: PENDING; se pide después de Architect AGREED; nunca se infiere.

Architect:
REQUIRED (M material); revisión formal de este objeto exacto PENDING; celda y orden PENDING (sin orden de revisión).

Coordinator:
PENDING (confirmación de materialidad y autoridad; orden de revisión del Architect; AGREED).

Status:
PROPOSED / NOT ACTIVE. Hasta Owner SATISFIED, Architect AGREED y Coordinator AGREED no se emite ningún paquete de fase de A-14.
```

**Fuentes:** el Freeze V5 (citado «V5 `:n`», la línea n del blob de arriba); el [brief del Owner](I-64-owner-brief.txt) (citado
«brief `:n`»); la disposición del Owner y del Coordinator de F4B (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`,
citada «OD `:n`»); el [paquete de F4B](I-64-f4b-push-back-completo-package.md) v3.1 (citado «PKG `:n`»); las
[aclaraciones de implementación](I-64-implementation-clarifications.md) IC-01..IC-11 (citadas «ICL `:n`»); la replaneación aprobada
por el Owner, «Replaneación de I-64» (2026-11-23; ruta en el repo PENDING); el registro literal de las decisiones del Owner del
2026-11-23 (ruta en el repo PENDING); el prototipo «RackCad tipo navegador» v3, aprobado por el Owner y solo descriptivo (no es
especificación; ruta en el repo PENDING).

## 1. Motivo

**La visión del Owner (2026-11-23):** todo RackCad funciona sin bloquear AutoCAD, desde una sola ventana tipo navegador: una columna
lateral con las secciones y la lista de los racks del dibujo (con buscador y filtros, porque un dibujo puede tener hasta unos 70
racks), un rack abierto en el centro, «Actualizar» por rack y «Actualizar todo».

**El hecho que la hace una realineación y no un giro:**
- V5 ya decía que los editores serían editables en el panel: §11 «**Push Back editable en el panel**» (F4, `:793`), «Selectivo,
  Dinámico y Cantilever … editables en el panel» (F5-A, `:794`) y «Cabecera y Cama … editables en el panel» (F5-B, `:795`). D-15,
  punto 1, define cómo: «el contenido del editor (el shell, que es un `Control`) se separa de su `Window`. La ventana clásica envuelve
  la misma superficie» (`:562-563`).
- F4B, por disposición del Owner y del Coordinator, abrió «Editar todo» como ventana **modal**: «opens the existing Push Back editor
  as a modal/full-size editing surface» (OD `:25-26`); «the drawing cannot be edited while the modal editor is open» (OD `:34`);
  «Do not try to fit the current ~1120 px editor inside the ~320 px panel» (OD `:37`). El paquete lo fija en OB-01.4 (PKG `:174`), con
  el hallazgo H-PB-10 («El panel mide 320 px y el editor pide 1120 × 672», PKG `:86`). El paquete cita D-15 punto 1 como autoridad
  (PKG `:60`), pero [OD] manda dentro del paquete (PKG `:59`), y ni el paquete ni IC-11 (ICL `:193-206`) declaran la diferencia.
- El Smoke-PB de PB-T5 certifica ese comportamiento modal: SPB-12 («La paleta no responde; al cerrar la ventana, sí») y SPB-13
  («Nada llega») del paquete de PB-T5 v2.4 (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md:260-261`).

**Por qué es una A-n y no una IC:** el registro de aclaraciones solo recoge diferencias de **mecanismo** (ICL `:3`). F4B entró por
IC-11 porque «extends the proven Workspace architecture» sin cambiar resultados congelados (OD `:20`). A-14 cambia el objetivo, varios
no-objetivos y resultados congelados. Un cambio de alcance o de no-objetivos es `OWNER-RESERVED` (LIFECYCLE §3), y todo cambio a un
elemento congelado usa una A-n (LIFECYCLE §6).

## 2. Decisiones del Owner (2026-11-23)

Registro literal: el archivo de decisiones del 2026-11-23 (ruta en el repo PENDING). Respuesta del Owner a las cuatro preguntas de la
replaneación (literal): «Sí, apruebo los cambios al plan, la ventana recuerda, rackcad abre la ventana y la lista la vemos después».

| # | Decisión | Efecto en A-14 |
|---|---|---|
| 1 | Aprueba el objetivo y los cambios de la tabla §3 de la replaneación | C-01 a C-07 |
| 2 | La ventana recuerda su tamaño y su posición entre sesiones de AutoCAD | C-08 |
| 3 | RACKCAD abre la ventana | C-06 |
| 4 | La lista de comandos a retirar se ve después | Antes de la fase 9. A-14 no retira ningún comando |
| 5 | «RACKPANEL puede quedarse por ahora» (literal, el mismo día) | RACKPANEL se conserva; su futuro se ve con la lista de retiro antes de la fase 9 (C-06) |

**Decisiones previas del mismo día que A-14 recoge:** navegación por la columna lateral, sin pestañas arriba, con buscador y filtro;
«Actualizar» por rack y «Actualizar todo» fuera de ellos, todo o nada; «Nuevo» abre el sistema elegido, por ahora con su ventana
clásica; las secciones Variables de RackCad, BOM y lista de racks, Biblioteca de diseños y Configuración («se eliminaran muchos de los
comandos pero obvio seguirán existiendo rackproyectar o así»); editores «Actual primero, simplificar después»; rediseño de UI/UX por
sistema «Diseñar antes de migrar»; terminar el Smoke-PB de F4B cuando el Owner tenga AutoCAD libre.

## 3. Cambios, uno por uno

Cada cambio: texto vigente → texto nuevo → autoridad → riesgo que V5 quería evitar y su tratamiento.

### C-01. §1 Objetivo

**Vigente** (V5 §1, `:71-72`): «pasar de ventanas modales a un panel persistente y modeless dentro de AutoCAD, con sincronización de
contexto en los dos sentidos, borradores por rack y Actualizar como única frontera de escritura».

**Nuevo:**
- Lo mismo, y además **toda la operación de RackCad** se hace desde esa ventana (la «ventana RackCad»): tipo navegador, acoplable o
  flotante, sin bloquear AutoCAD.
- La ventana tiene una columna lateral con las secciones (Nuevo, Variables, BOM y lista de racks, Biblioteca de diseños,
  Configuración) y la lista de los racks del dibujo; un rack abierto a la vez en el área central, con su editor; «Actualizar» por rack
  y «Actualizar todo».
- Las escrituras en el DWG que inicia la ventana pasan solo por «Actualizar» o «Actualizar todo». La excepción son los flujos
  clásicos que la ventana lanza mientras se migran («Nuevo», «Abrir en editor clásico» y las secciones que aún abren su ventana
  actual): escriben como hoy.

**Criterios de éxito** (V5 `:82-95`):

| # | Cambio |
|---|---|
| 5 | «Sesiones tipo navegador sin un editor pesado por rack»: mecanismo D-08 tal como queda en C-02; gates: fases 2, 3, 6 y 7 |
| 11 | Rendimiento: se añade la fase 2 (arnés con al menos 70 racks); Smoke-PERF con un dibujo real de unos 70 racks |
| 12 | ID3: la clasificación sigue en READY, ahora tras las fases 4, 6 y 7 (C-04) |
| nuevo | RACKCAD abre la ventana y todo el contenido del menú actual está en ella (fase 1) |
| nuevo | «Actualizar todo»: todo o nada y un solo paso de Deshacer (fase 4) |
| nuevo | Las secciones funcionan dentro de la ventana sin bloquear (fase 5) |
| nuevo | La ventana recuerda su tamaño y su posición (C-08) |

**Autoridad:** `OWNER-RESERVED` (cambio de alcance, LIFECYCLE §3) + Architect + Coordinator (M-01, M-07).

**Riesgo que V5 quería evitar:** reescribir la arquitectura de producto («rewriting all product architecture», brief `:476`) y todos
los editores a la vez («Do NOT rewrite every editor immediately», brief `:377`; «Avoid six parallel editor rewrites», brief `:391`).
**Tratamiento:** la ventana se amplía sobre lo cerrado (F1 a F4B), sin reabrirlo; los editores entran de uno en uno con el contrato
de D-15; los flujos clásicos quedan como respaldo; cada fase tiene su paquete y su orden (§4).

### C-02. D-08: de pestañas a la columna lateral (con D-06 y D-03)

**Vigente:**
- D-08 (`:382-397`): registro ligero por pestaña; preview, como máximo una por sesión; «La primera pulsación en un campo fija la
  preview» (`:388`); «Cerrar: sin dirty se cierra sin preguntar; con dirty … pregunta Descartar o Cancelar» (`:390`); marca
  «Nombre *»; K = 1; estado Error; un sistema aún no integrado da una pestaña de resumen con «Abrir en editor clásico» (`:397`).
- D-06 (`:335-336`): «Uno(R): activa la pestaña de R si existe; si no, crea o reemplaza la preview»; con texto pendiente, la
  pestaña activa sigue y la preview de R va en segundo plano.
- D-03 (`:186`): la sesión contiene «pestañas (con su preview), pestaña activa y borradores».
- El brief pedía pestañas tipo navegador (brief `:126-132`).

**Nuevo:**
- **Sin tira de pestañas.** La navegación es la lista lateral de los racks del dibujo: el navegador de F6 (D-05 `:309-311`), agrupado
  por sistema, con buscador por nombre, filtro por sistema y «solo con cambios sin escribir». El estado de cada fila es el de D-05
  (`:317-325`): InventoryStatus del índice y SessionOverlay de la sesión, superpuesto por la UI.
- **Un rack abierto a la vez** en el área central, en lugar de la pestaña activa. Abrir otro rack es una frontera **bloqueable** de
  D-10 (`:434-435`): el texto pendiente se compromete según ADR-0032 D6 o la acción se aborta. Los borradores de los demás racks
  siguen vivos en memoria y su fila lo muestra.
- **Desaparecen la preview y el fijado:** ya no hay nada que se reemplace. Lo que protegía INV-04 se conserva así: cambiar de rack
  nunca pierde un borrador ni un texto pendiente. La primera pulsación en un campo sigue siendo la intención de edición de D-10
  (`:427-428`).
- **«Cerrar pestaña» pasa a «Descartar cambios»** del rack abierto: pregunta si hay dirty y nunca escribe el DWG (INV-05, INV-09).
- **«Seguir la selección de AutoCAD»**, un interruptor por sesión de documento. Su valor inicial lo fija el paquete de la fase 2.
  - Encendido: Uno(R) abre R en el área central si la frontera lo permite. Con texto pendiente, el rack abierto se queda y la fila de
    R se marca, como en D-06 (`:335-336`).
  - Apagado: Uno(R) solo marca la fila de R.
  - Selección (N) no abre racks ni borradores (D-09, INV-34). Ningún evento escribe selección ni vista (D-06 `:339`, INV-03).
- Los registros ligeros que D-08 pedía por pestaña pasan a ser por fila. K = 1 superficie viva (P-06), sin cambio.
- Un sistema aún no integrado muestra en el área central el resumen y «Abrir en editor clásico» (D-08 `:397`; IC-03).
- **D-03** (`:186`): «pestañas (con su preview), pestaña activa» pasa a «lista, rack abierto e interruptor de seguir la selección».
  Nada cruza de una sesión a otra (INV-07).

**Autoridad:** `OWNER-RESERVED` (cambia lo que pedía el brief, `:126-132`) + Architect + Coordinator (M-03; M-04, porque cambia la
forma del cierre con dirty).

**Riesgo que V5 quería evitar:** perder borradores o texto al navegar (PV2-14, INV-04); abrir N editores (brief `:263`); que la
selección cambie lo que el usuario edita. **Tratamiento:** la frontera bloqueable de D-10 al cambiar de rack; Selección (N) sin
abrir; el interruptor apagable; K = 1; el eco por contenido de D-07, sin cambio.

### C-03. §11: F6 sube

**Vigente** (§11 `:796`): «F6 — Document-wide Inventory», con entrada F2 y F3, criterios INV-16 e INV-17 en el índice, INV-34 a
INV-38 y los arneses sintéticos de los escenarios 7 y 8; sexto en el orden.

**Nuevo:** F6 pasa a la **fase 2**, después de la fase 1. Conserva su resultado y sus criterios, y suma los de C-02. Su entrada se
cumple: F2 y F3 están cerradas, más la fase 1. La tira de pestañas de F3 queda sustituida; F3 no se reabre.

**Autoridad:** la resecuenciación sola es Coordinator-only (LIFECYCLE §6); lo que cambia su resultado va con C-02.

**Riesgo que V5 quería evitar:** el coste de una enumeración global antes de tiempo («Antes de F6 no hay enumeración global del
dibujo», D-05 `:296-297`). **Tratamiento:** se conservan el refresco bajo demanda, solo con el panel visible (D-05 `:309-311`, P-05),
y la privacidad del índice (D-05 `:284-295`, INV-36); el arnés sintético se mide con al menos 70 racks.

### C-04. D-09 y D-20: «Actualizar todo»

**Vigente:**
- D-09 (`:405-406`): «Punto de extensión: el contexto expone un conjunto estable de pares (sesión, RackId) para un futuro borrador por
  lotes (residual de ID3, D-20). No se implementa edición de propiedades comunes ni un Actualizar por lotes.»
- No-objetivo (`:78`): «Edición por lotes: residual de ID3 (D-20).»
- D-20 (`:679-683`): el residual exacto de ID3 es «la edición por lotes del brief (propiedades comunes, validación conjunta y mutación
  atómica de varios racks) **y** todo sistema que no quede integrado»; la clasificación se hace en READY, la esperada es `PARTIAL` y
  la acepta el Owner.

**Por qué V5 lo excluyó:**
- El brief lo dejó fuera salvo congelación expresa («global batch Apply unless explicitly frozen», brief `:475`) y admitió que la
  mutación atómica de varios racks quedara para después («The latter may remain a later deliverable if scope becomes excessive»,
  brief `:438`).
- Las garantías de V5 son **por rack**:
  - todo o nada «sobre el authored del rack» (D-13 `:502-503`, INV-12);
  - la relectura y la comparación dentro de la transacción de MUTATE (D-12, INV-06);
  - el ámbito de mutación propia por (sesión, miembros del rack) (D-04 `:241-243`, INV-08; IC-09);
  - un Actualizar del rack A no marca a B (INV-28);
  - un solo Actualizar a la vez (`:545`);
  - un paso de UNDO por Actualizar (O-11, INV-32);
  - Cantilever y Cama sin escritor caller-owned (`:536-539`).

**Nuevo:**
- **«Actualizar» por rack:** sin cambio (D-13).
- **«Actualizar todo»**, fuera de los racks, escribe en una sola operación todos los borradores con cambios de la sesión de documento
  activa, nunca de otra (D-03). Usa la vía de D-13 extendida a N racks:
  - PREPARE para todos: comprobación previa, admisión, plan e importación de biblioteca fuera de la transacción;
  - MUTATE en **una** transacción: la relectura y la comparación de **cada** rack antes de escribir nada, y después las unidades de
    todos;
  - POST para todos.
- **Todo o nada sobre el authored de todos los racks incluidos:** un Conflicto, una admisión rechazada, un preflight fallido o un fallo
  antes de `Commit()` en cualquiera da cero escrituras authored en todos. Sigue admitido el residuo de biblioteca de OM-5.
- **Un solo paso de Deshacer** para toda la operación.
- **Dice cuál falló y por qué:** el resultado lista cada rack no aplicado con su causa (Conflicto y qué cambió, Admisión, Preflight o
  Fallo), visible en la ventana y no solo en el registro (como INV-31).
- **Clasificación por frontera** (D-13 `:521-532`) para el conjunto: si `Commit()` lanza, todos los incluidos pasan a Desconocido, con
  «Comprobar ahora» por rack; si POST falla o hay faltantes, Aplicado con avisos, por rack.
- **Conflicto con el dibujo:** por cada rack en Conflicto, las dos reconciliaciones de D-12 (`:487-493`): «Volver a leer» (descartar y
  recargar) o «Conservar mi borrador»; sin fusión automática. Después se repite «Actualizar todo».
- Solo entran racks de sistemas integrados en la ventana, que ya tienen escritor caller-owned (D-13 `:536-539`).
- **Variables:** la replaneación aprobada incluye las «variables con cambios» en «Actualizar todo». A-14 lo fija como objetivo; en qué
  fase entra queda en Q-02, y sus riesgos en R-09.
- **Cancelar:** solo antes de `Commit()` y sin escrituras authored. El cómo lo fija el paquete de la fase 4.
- **D-09:** el punto de extensión de lotes se usa. La edición de propiedades comunes de varios racks sigue sin implementarse.
- **D-20:** el residual de ID3 pasa a ser «la edición de propiedades comunes de varios racks y todo sistema que no quede integrado».
  La mutación atómica de varios racks y su validación conjunta salen del residual cuando cierre la fase 4. La clasificación sigue en
  READY y la acepta el Owner.
- **No-objetivo** (`:78`): «Edición por lotes» pasa a «Edición de propiedades comunes de varios racks: residual de ID3 (D-20)».

**Autoridad:** `OWNER-RESERVED` (alcance, brief `:475`; no-objetivo; D-20) + Architect + Coordinator (M-04; M-01; M-05, porque
consume la costura de I-55 para N racks; M-08 por prudencia: la nota prevista a ADR-0010 dice «todo o nada el authored del rack»,
anexo B `:1219-1221`).

**Riesgo que V5 quería evitar, y su tratamiento:**

| Lo que V5 protegía | Cómo lo trata A-14 |
|---|---|
| Todo o nada por rack (D-13, INV-12) | Todo o nada sobre la unión, en una sola transacción de MUTATE |
| Relectura dentro de MUTATE, antes de escribir (D-12, INV-06) | La de cada rack, dentro de la misma transacción y antes de cualquier escritura; ningún rack se escribe si otro da Conflicto |
| Ámbito de mutación propia por (sesión, miembros) (D-04, INV-08, IC-09) | Cubre la unión de los miembros de los racks incluidos, de PREPARE a POST |
| Actualizar A no marca B (INV-28) | Se conserva para todo borrador no incluido |
| Un paso de UNDO (O-11, INV-32) | Un paso para toda la operación; S2-05 se repite en el smoke de la fase 4 |
| Un solo Actualizar a la vez (`:545`) | «Actualizar todo» excluye cualquier otro Actualizar de la sesión mientras dura |
| Desconocido, nunca supuesto aplicado (D-13, INV-30) | Para todo el conjunto |
| H-LOCK (D-12 `:494-498`) | Igual, sobre una transacción más larga; S2-13 se repite |
| Duración y bloqueo del documento | Progreso visible; el documento queda bloqueado mientras dura, como durante un comando (R-04) |

### C-05. No-objetivo «Crear racks nuevos desde el panel» → «Nuevo»

**Vigente** (`:77`): «Crear racks nuevos desde el panel: los comandos de creación no cambian.»

**Nuevo:**
- Se retira el no-objetivo. La sección «Nuevo» de la ventana ofrece los sistemas del menú actual de RACKCAD.
- **Paso intermedio, desde la fase 1:** «Nuevo» abre la ventana clásica de ese sistema, con el mismo flujo de hoy. Los comandos de
  creación no cambian, y crean y nombran como hoy (I-60).
- **Meta, fase 8:** crear el sistema dentro de la ventana como «Nuevo, sin insertar» e insertarlo desde ella. La meta pide decisiones
  que A-14 **no** toma (Q-04): Insertar desde la ventana (no-objetivo `:75-76`; D-23 `:711`), la identidad nueva (D-22 `:704`,
  INV-16), la creación de vistas (D-PB-04, OD `:76-78`) y el nombre (I-60). Hasta esas decisiones, el no-objetivo `:75-76` sigue
  vigente.

**Autoridad:** `OWNER-RESERVED` (no-objetivo); el paso intermedio no añade semántica nueva. La meta de la fase 8: Owner + Architect +
Coordinator, con su propia A-n.

**Riesgo que V5 quería evitar:** que el panel estampe identidad, invente RackId o escriba fuera de Actualizar (D-22, INV-16, INV-21).
**Tratamiento:** en el intermedio, la ventana solo lanza el flujo clásico, ligado a su sesión, como «Abrir en editor clásico» (D-16
`:590-591`); crea y estampa el flujo clásico, como hoy.

### C-06. D-16 y el no-objetivo de comandos → secciones; RACKCAD abre la ventana

**Vigente:**
- No-objetivo (`:80`): «Cambiar `RACKEDITAR`, `RACKLISTA`, `RACKPROYECTAR` u otros comandos existentes (D-16).»
- D-16 (`:586-587`): «Sin cambio observable en RACKEDITAR, los comandos de creación, RACKLISTA, RACKPROYECTAR, RACKDUPLICAR,
  RACKBOMTOTAL y RACKVARIABLES (INV-19).»
- §14, M-03 (`:996`): no activado mientras «los comandos clásicos permanecen observacionalmente invariantes».
- D-01 (`:124`): la paleta «se muestra con un comando nuevo, `RACKPANEL`»; D-02 (`:151`): el controlador «se crea con el primer
  `RACKPANEL`».

**Hecho:** hoy RACKCAD (alias RK) abre el menú modal con `ShowModalWindow` (`src/RackCad.Plugin/RackMenuCommands.cs:21-34`). El menú
ofrece diseñar los seis sistemas y el larguero, generar un perfil estructural, abrir de la biblioteca de diseños, drive-in
(«Próximamente») y la ruta de la biblioteca de bloques (`src/RackCad.UI/RackMainMenuWindow.xaml`).

**Nuevo:**
- **RACKCAD** abre o muestra la ventana RackCad desde la fase 1 y deja de abrir el menú modal. Todo el contenido del menú queda en la
  ventana; el paquete de la fase 1 fija en qué sección va cada elemento.
- **RACKPANEL** se conserva (decisión del Owner); su futuro se ve con la lista de retiro antes de la fase 9. **RACKPANELDESCARTAR**
  (D-17) se conserva.
- D-01 (`:124`): «se muestra con RACKCAD o RACKPANEL». D-02 (`:151`): el controlador se crea con el primero de los dos.
- **Secciones:** Variables (la función de RACKVARIABLES), BOM y lista de racks (RACKBOMTOTAL y RACKLISTA), Biblioteca de diseños y
  Configuración.
  - Paso intermedio: cada sección abre su ventana actual, que sigue siendo modal y escribe como hoy (subdiálogos de D-02 `:173-174`).
  - La fase 5 las pone dentro de la ventana, sin bloquear.
  - Variables y BOM y lista están ligadas a su sesión de documento (D-03); Biblioteca y Configuración son del usuario.
  - «BOM y lista de racks» consume las autoridades existentes de RACKLISTA y RACKBOMTOTAL, **no** el índice del panel (D-05
    `:284-295`, INV-36).
- Los comandos existentes siguen sin cambio observable (D-16, INV-19) hasta la fase 9, salvo RACKCAD (y su alias, Q-07).
- **Retiro de comandos:** una fase propia, la 9, con la lista que decida el Owner antes. Como cambia D-16, esa lista entra por su propia
  A-n antes del paquete de la fase 9. A-14 no retira ninguno. RACKPROYECTAR y RACKEDITAR se quedan.

**Autoridad:** `OWNER-RESERVED` (no-objetivo; el Owner decidió RACKCAD y RACKPANEL) + Architect + Coordinator: M-03 (se rompe para
RACKCAD la condición de §14 `:996`), M-06 (D-23: región de variables de proyecto, `:708-709`) y M-07.

**Riesgo que V5 quería evitar:** romper el uso clásico (INV-19; STOP de D-16 `:593`). **Tratamiento:** solo cambia RACKCAD, por
decisión del Owner y como cambio declarado (el mismo trato que D-PB-06 en F4B, OD `:130-155`); los demás siguen con INV-19 hasta su
fase; el retiro necesita lista del Owner, A-n y fase propia.

### C-07. D-15 y F4B D-PB-01: el editor vive en el área central

**Vigente:**
- §11 (`:793-795`) y D-15 punto 1 (`:562-563`), citados en el §1 de esta enmienda. Orden de D-15 (`:577-581`): piloto Push Back en
  F4; F5-A Selectivo, Dinámico y Cantilever; F5-B Cabecera y Cama. «No se reescribe lógica de dominio de ningún editor» (`:582`).
- F4B D-PB-01: «Editar todo» abre la ventana existente de Push Back en modo WORKSPACE, modal (OD `:22-49`; PKG OB-01.4 `:174`); «Do not
  build a second full Push Back editor» (OD `:36`); «Do not try to fit the current ~1120 px editor inside the ~320 px panel» (OD `:37`).

**Nuevo:**
- El editor de cada sistema vive en el **área central** de la ventana, sin bloquear AutoCAD. Es la realineación con D-15 punto 1 y
  §11.
- **Push Back primero, tal cual (fase 3):**
  - la fase 3 hace para Push Back el punto 1 de D-15: separa la superficie de su `Window` y la ventana clásica la envuelve;
  - reutiliza lo que F4B ya dejó: el estado sin pérdidas (PB-T1), el modo WORKSPACE con «aplicar», «cerrar» y «escribir» separados
    (PB-T2) y la escritura PREPARE → MUTATE → POST (PB-T3);
  - «Editar todo» deja de abrir la ventana modal;
  - el modo CLASSIC (RACKEDITAR) sigue sin cambio observable (T1B, INV-19);
  - no hay segundo editor: una superficie y un estado.
- La ventana deja de ser una paleta de 320 px: es acoplable o flotante y redimensionable. La fase 3 empieza midiendo si el editor
  cabe y funciona en ella. Si no, la contingencia (b) de D-01, `ShowModelessWindow`, por una A-n material (`:127-128`).
- **D-PB-01 pasa a ser un paso intermedio:** rige tal cual hasta que cierre la fase 3. SPB-12 y SPB-13 certifican ese paso
  intermedio y no se reetiquetan.
- **Rediseño (línea D, decisión del Owner «Diseñar antes de migrar»):**
  - Selectivo, Dinámico, Cantilever, Cabecera y Cama se migran una sola vez, con su diseño nuevo aprobado por el Owner en un
    prototipo. Es la decisión del Owner que exige ADR-0029 D13 para cambiar un comportamiento observable;
  - Push Back se rediseña después (fase 3b);
  - pendiente (Q-03): qué pasa con la ventana clásica de cada sistema rediseñado, porque D-15 punto 1 dice que la ventana clásica
    envuelve **la misma** superficie.
- El contrato de adaptador de D-15 (cinco puntos), la caracterización previa por sistema y el criterio de entrada no cambian. El
  rediseño es de superficie; la lógica de dominio no se reescribe (`:582`).

**Autoridad:** Architect + Coordinator (M-05), según la tabla §3 de la replaneación. Además Owner, porque D-PB-01 y OD `:37` son texto
literal del Owner y el rediseño cambia comportamiento observable (ADR-0029 D13). La aprobación del Owner de la tabla §3 lo cubre; su
firma del texto sigue pendiente.

**Riesgo que F4B y V5 querían evitar:** que el editor (1120 × 672) no quepa en el panel (H-PB-10); un segundo editor (OD `:36`); que
RACKEDITAR cambie (INV-19). **Tratamiento:** medición primero; contingencia de D-01; una superficie y un estado; T1B en verde.

### C-08. No-objetivo de persistencia de la posición → la ventana recuerda tamaño y posición

**Vigente:**
- No-objetivo (`:79`): «Persistir el panel o sus borradores, incluida la posición de la paleta entre sesiones (brief,
  «MULTI-DOCUMENT»).»
- D-22 (`:702-703`): «No hay campos persistidos nuevos, ni escrituras en el NOD ni XData del panel, ni archivos aparte del registro,
  ni escrituras en el registro de Windows o el perfil desde el código de RackCad (INV-21).»
- D-01 (`:122`): «se construye sin `toolID` y sin manejadores `Load`/`Save`; RackCad no pide a AutoCAD persistir estado de la paleta».
- D-23 (`:712`): «Restauración entre arranques», punto de extensión. Brief: «Persistent cross-launch workspace restoration is future
  scope» (`:371`).

**Hechos:** hoy la paleta se coloca igual en cada creación (`WorkspacePaletteHost.ApplyInitialPlacement`, F1-T3B-HOST-UI-C1, sin
persistencia ni perfil), y el diff de perfil de Smoke-1 salió limpio. RackCad ya tiene un almacén de ajustes del usuario fuera del DWG
(`src/RackCad.Application/Settings/UserSettings.cs`; AGENTS.md lo cita para la ruta de la biblioteca de bloques).

**Nuevo:**
- La ventana recuerda su **tamaño y su posición** entre sesiones de AutoCAD. Solo eso: los borradores, el rack abierto, los filtros y
  la sesión siguen sin persistirse.
- **Vía:** los ajustes de usuario de RackCad, no el estado de paleta de AutoCAD. Así D-01 `:122` queda literal y el diff de perfil
  (S1-11) sigue valiendo. La mecánica no se congela.
- **D-22 e INV-21**, excepción acotada: el rectángulo de la ventana en los ajustes de usuario de RackCad. Nada en el DWG, el NOD,
  XData, el registro de Windows ni el perfil.
- Si el rectángulo guardado no cae entero en un monitor visible, o no se puede leer, se usa la colocación inicial de hoy. Precedente:
  el STOP de Smoke-1 en SM-03, con la paleta inutilizable y fuera de la pantalla.
- Entra en la fase 1 (propuesta de este borrador; la replaneación no la sitúa).

**Autoridad:** `OWNER-RESERVED` (no-objetivo; el Owner lo decidió) + Architect + Coordinator (M-02: rompe la condición de §14
`:995`, «sin campos nuevos … (D-22)»).

**Riesgo que V5 quería evitar:** tocar el perfil de AutoCAD o la configuración que vigila I-52 (D-01 `:140-147`; §12.1 `:826-827`) y
adelantar la restauración entre arranques. **Tratamiento:** la vía no toca el perfil; S1-11 sigue; solo tamaño y posición.

### C-09. D-01 sigue igual

No cambian: un `PaletteSet` por proceso con un solo control raíz WPF (`:120-121`); sin `toolID` ni `Load`/`Save` (`:122`, gracias a la
vía de C-08); acoplable y flotante (`:123`); cerrar oculta (`:125`); la contingencia (b) por A-n material (`:127-128`); la obligación de
foco congelada (`:130-135`) y su prueba (INV-24, con A-7: S2-16 a S2-22), que ahora alcanza todos los controles de cada editor
alojado; el perfil (`:140-147`). Lo único que cambia de D-01 es el comando que muestra la paleta (`:124`, C-06).

### C-10. Invariantes y OV afectados

**Invariantes existentes** (V5 §10, `:737-778`) que se reescriben al emitir la fase que los toca. El texto exacto lo fija el paquete
de esa fase y lo revisa el Coordinator:

| Invariante | Cambio | Fase |
|---|---|---|
| INV-02 | «10 pestañas» → «entradas de la lista» | 2 |
| INV-03 | «la pestaña activa, la preview ni la lista» → «el rack abierto ni la lista» | 2 |
| INV-04 | regla de preview → cambiar de rack nunca pierde borrador ni texto (INV-A14-04) | 2 |
| INV-09 | cerrar pestaña → descartar o cambiar de rack, sin escribir | 2 |
| INV-12, INV-32 | se extienden a «Actualizar todo» (INV-A14-07, 08) | 4 |
| INV-16 | sin cambio hasta las decisiones de la fase 8 (Q-04) | 8 |
| INV-19 | excepción declarada: RACKCAD (C-06) | 1 |
| INV-21 | excepción acotada: tamaño y posición (C-08) | 1 |
| INV-24 | todos los controles de cada editor alojado | 3, 6, 7, 3b |
| INV-34 | «nunca crea pestañas» → «nunca abre racks ni borradores» | 2 |
| INV-36, INV-38 | «pestaña» → «rack abierto» y «lista» | 2 |

**Invariantes nuevos** (IDs provisionales; la numeración la fija el Coordinator):

| ID | Invariante | RED esperado | Fase |
|---|---|---|---|
| INV-A14-01 | RACKCAD abre o muestra la ventana y no abre el menú modal; con la ventana abierta, AutoCAD sigue usable | RACKCAD aún llama a `ShowModalWindow` con el menú | 1 |
| INV-A14-02 | Todo elemento del menú actual de RACKCAD tiene su entrada en la ventana | un elemento sin entrada | 1 |
| INV-A14-03 | La ventana recupera tamaño y posición; un rectángulo ilegible o fuera de los monitores visibles da la colocación inicial; nunca escribe DWG, registro de Windows ni perfil | un rectángulo fuera de pantalla se restaura tal cual | 1 |
| INV-A14-04 | Cambiar el rack abierto nunca escribe el DWG ni pierde un borrador o un texto pendiente; un campo inválido aborta el cambio | el texto se pierde o el cambio ocurre con un campo inválido | 2 |
| INV-A14-05 | Búsqueda, filtros y «solo con cambios» usan solo el índice y el overlay: no leen el DWG | un filtro que lee el DWG | 2 |
| INV-A14-06 | Con «Seguir la selección» apagado, Uno(R) no cambia el rack abierto; encendido, sigue D-06; nunca escribe selección ni vista | el interruptor apagado cambia el rack abierto | 2 |
| INV-A14-07 | «Actualizar todo»: un Conflicto, una admisión rechazada o un fallo antes de `Commit()` en cualquier rack da cero escrituras authored en todos, y el resultado nombra cada rack y su causa | un commit por rack deja racks escritos | 4 |
| INV-A14-08 | «Actualizar todo» es una sola transacción de MUTATE y un solo paso de UNDO; la frontera de `Commit()` clasifica el conjunto | N transacciones o N pasos | 4 |
| INV-A14-09 | «Actualizar todo» opera solo sobre los borradores de su sesión de documento | un borrador de otra sesión se escribe | 4 |
| INV-A14-10 | El ámbito de mutación propia cubre la unión de los miembros; un borrador no incluido conserva INV-28 | un borrador escrito queda marcado por sus propias pistas | 4 |
| INV-A14-11 | El editor alojado no anota ninguna petición clásica (OB-01.3 se conserva) y ninguna tecla de sus campos llega a AutoCAD | un gesto anota `InsertRequested` | 3 |

**OV** (§12.6, `:927-960`). Retirar o sustituir un escenario exige al Owner; reasignarlo, una A-n (`:960`; LIFECYCLE §8):

| OV | Propuesta | Autoridad |
|---|---|---|
| OV-08 «Varias pestañas» | se sustituye por «Varios racks con borradores vivos en la lista lateral» | Owner |
| OV-09 «Preview y fijado» | se sustituye por «Rack abierto y Seguir la selección de AutoCAD» | Owner |
| OV-14 «Selección (N) sin abrir pestañas» | se sustituye por «Selección (N) sin abrir racks ni borradores» | Owner |
| OV-05 | sin cambio; pasa a la fase 2 | — |
| OV-A14-01..07 (provisionales) | RACKCAD abre la ventana; tamaño y posición, incluido un monitor retirado; «Actualizar todo» (éxito, un fallo que no deja nada escrito, un U); secciones sin bloquear; «Nuevo» intermedio; lista con unos 70 racks reales; editor de Push Back en la ventana sin bloquear | Coordinator (sobre lo que congela A-14) |

## 4. Plan de fases

**Reglas comunes:**
- Cada fase necesita su **propio paquete con revisión independiente** y la **orden del Owner** (y del Coordinator) antes de emitirse,
  como F4B (F4B-E01, F4B-E02). Sigue la cadena de F4 y F4B: contrato de gate por tarea, planificación, Worker con RED y GREEN, CI
  exacta, verificación, revisiones, controles y cierre.
- El presupuesto por tarea lo fija la orden de cada fase (en F4B: 1/3 por tarea, y el intento 3 vuelve al Owner, D-PB-10).
- No hay integración parcial en `main` sin decisión del Owner (A64-PV1-21, `:803-805`; Q-09).
- Cada hito del Owner necesita su propio Candidato completo (§12.1 `:828-829`), y las reglas de host de §12.1 siguen: RELEASE de I-52,
  una sola sesión de AutoCAD, sin cambios de perfil ni de seguridad.
- Ninguna fase depende de I-63 (MASTER-I63-I64-02 §6).

| Fase | Objetivo | Depende de | Gate de V5 | Hito del Owner |
|---|---|---|---|---|
| **0 · F4B** | Cerrar PB-T5 tal como está emitida: CI exacta, Smoke-PB, revisión independiente, firma del Owner y gate-close | — | IC-11 (entre F4 y F5-A); sin cambio | Smoke-PB (en curso) |
| **1 · La ventana** | El panel pasa a ser la ventana principal de RackCad: columna lateral con las secciones y todo el contenido del menú de RACKCAD; «Nuevo» y las secciones abren sus ventanas actuales; RACKCAD abre la ventana; tamaño y posición (C-08). Antes de nada, mide cómo lanzar los flujos clásicos desde la ventana (R-13) | 0 | Nueva; sustituye al menú modal. Conserva D-01, P-01 a P-09 y las obligaciones de foco | Smoke corto |
| **2 · Racks del dibujo** | La lista lateral (C-02 y C-03): estado, buscador, filtro por sistema, «solo con cambios», «Seguir la selección», Selección (N); probada con al menos 70 racks | 1 | **F6** (sustituye y amplía); sustituye la tira de pestañas de F3, sin reabrirla | — (OV-05 en la final) |
| **3 · Push Back en la ventana** | El editor completo de Push Back en el área central, sin bloquear; los cambios van al borrador; «Actualizar» del rack; teclado y foco dentro de la ventana (C-07). «Editar todo» deja de ser modal | 2 | Completa para Push Back el punto 1 de D-15; sustituye a D-PB-01 | Smoke principal (con la 4) |
| **4 · Actualizar todo** | C-04: todo o nada, un Deshacer, progreso y lista de lo que falló | 3 (y Q-02) | Nueva; saca del residual de ID3 la mutación atómica de varios racks (D-20) | Smoke principal |
| **5 · Secciones** | Variables, BOM y lista de racks, Biblioteca de diseños y Configuración dentro de la ventana, sin bloquear | 1 (en paralelo) | Nueva; implementa la región de variables de D-23 | — |
| **D · Diseño por sistema** | Sin código: un prototipo de UI/UX por sistema que aprueba el Owner. Orden: Selectivo, Dinámico y Cantilever (antes de la 6); Cabecera y Cama (antes de la 7); Push Back (antes de la 3b) | — (en paralelo desde ya) | No es un gate (LIFECYCLE §7): da la decisión del Owner que pide ADR-0029 D13 | Revisión del prototipo |
| **6 · Selectivo, Dinámico, Cantilever** | Sus editores en la ventana, con su diseño nuevo y el patrón de la fase 3 | 3 y su prototipo | **F5-A** (sustituye): sus criterios por sistema (`:794`) y la variante caller-owned de Cantilever (`:536-539`) | Smoke |
| **7 · Cabecera y Cama** | Lo mismo, con su diseño nuevo | 6 y su prototipo | **F5-B** (sustituye): sus criterios (`:795`) y la variante caller-owned de Cama | — |
| **3b · Push Back rediseñado** | El editor de Push Back con su diseño nuevo; reutiliza el estado y el escritor de F4B | 4 y su prototipo | Nueva | Smoke |
| **8 · Nuevo en la ventana** | «Nuevo» crea el sistema en la lista como «Nuevo, sin insertar» y lo inserta desde la ventana | 3, 6 y Q-04 | Nueva; necesita su A-n (C-05) | — |
| **9 · Retiro de comandos** | Los comandos sustituidos se retiran o avisan de que su función está en la ventana, según la lista del Owner | 5 a 8 y la lista del Owner (con su A-n) | Nueva; cambia D-16 e INV-19 para los comandos de la lista | — |
| **F7 · Smoke-PERF · READY** | Rendimiento y regresión; Smoke-PERF con un dibujo real de unos 70 racks; READY | 1 a 9, incluida la 3b | **F7**, **Smoke-PERF** y **READY** (se conservan) | Smoke-PERF; OV final |

Orden de la replaneación: 0 → 1 → 2 → 3 → 4 → 6 → 7 → 3b → 8 → 9 → F7. La línea D corre en paralelo desde ya; la fase 5 puede avanzar en
paralelo desde que cierre la 1.

**Correspondencia de gates de V5 con las fases de A-14:**

| V5 | Estado (I-64.yml) | A-14 |
|---|---|---|
| F1 y Smoke-1 | CLOSED / PASS | Sin cambio; no se reabren |
| F2 y Smoke-NAV | CLOSED / PASS WITH LIMITATIONS | Sin cambio; IC-01 e IC-02 siguen |
| F3 | CLOSED / PASS WITH LIMITATIONS | Sin cambio como historia; la fase 2 sustituye su tira de pestañas |
| F4 y Smoke-2 | CLOSED / PASS WITH LIMITATIONS | Sin cambio |
| F4B (IC-11) | PB-T5 emitida | Fase 0, sin cambio |
| F5-A | AUTHORIZED / WAITING FOR F4B (OD `:269`) | Fase 6, con su prototipo; su autorización: Q-05 |
| F5-B | pendiente | Fase 7, con su prototipo |
| F6 | pendiente | Fase 2 (sube y se amplía) |
| — | — | Fases nuevas: 1, 3, 4, 5, 3b, 8 y 9; línea D |
| F7 | pendiente | F7; entra con las fases 1 a 9 |
| Smoke-PERF | — | Se conserva (§12.5); SP-03 sobre un dibujo real de unos 70 racks |
| READY | — | Se conserva; la clasificación de ID3, tras las fases 4, 6 y 7 |

## 5. Riesgos

| ID | Riesgo | Tratamiento |
|---|---|---|
| R-01 | El editor de Push Back es grande para una paleta (H-PB-10) | Ventana flotante y redimensionable; la fase 3 empieza midiendo; si la paleta no sirve, contingencia (b) de D-01 por A-n material |
| R-02 | Teclado y foco con un editor completo dentro de la paleta. En Smoke-1, un Ctrl+V lanzó PASTECLIP; se clasificó como incidente de entrada del operador, no como defecto de producto | La obligación de foco de D-01 está congelada; la fase 3 la prueba en todos los controles del editor (S2-16 a S2-22 de A-7) |
| R-03 | 70 racks en un dibujo | La fase 2 mide la lista y el inventario con al menos 70 racks; SP-03 de Smoke-PERF con un dibujo real |
| R-04 | «Actualizar todo» puede tardar y mantiene el documento bloqueado; un «Actualizar» grande de un rack se mide en SPB-23 (dato del Smoke-PB en curso, aún sin versionar) | Progreso visible; una sola operación con un Deshacer; si algo falla, no se escribe nada; cancelar solo antes de `Commit()`, definido en la fase 4 |
| R-05 | Ventanas pequeñas que siguen siendo modales (Configurar cabecera, Seguridad, BOM, Guardar en biblioteca) | Se quedan modales al principio, como subdiálogos de D-02, y se listan para migrarlas en sus fases |
| R-06 | RACKEDITAR cambia un rack que tiene borrador en la ventana | Resuelto en F4: el borrador pasa a «posiblemente obsoleto» y «Actualizar» da Conflicto y pide elegir (D-12, D-16) |
| R-07 | El rediseño cambia la forma de trabajar con cada sistema | Prototipo aprobado antes de programar, smoke por sistema y RACKEDITAR como respaldo; Q-03 |
| R-08 | El coste del proceso: cada fase lleva la cadena completa | Las fases 1, 2 y 5 son ligeras; la 3 y la 4 son las grandes, como F4B |
| R-09 | Variables en «Actualizar todo»: dos autoridades (el registro de variables de ADR-0034 y los sobres de los racks); un cambio de variable alcanza a racks sin borrador; el ejecutor de variables usa `FindRackBlocks` (D-11 `:451`) y el panel `RackSiblingMembership.Classify`; la StaleBase de Selectivo incluye el registro entero (D-12 `:475-477`); hoy RACKVARIABLES escribe al momento, dentro de su bucle de rondas (D-04 `:215-216`); un borrador de variables no tiene RackId, y la clave de D-10 es (sesión, RackId) | Q-02; el paquete que lo implemente lo trata con el Architect antes del contrato; el veto de D-17 cubre también ese borrador |
| R-10 | Privacidad del índice frente a «BOM y lista de racks»: el prototipo muestra «70 racks» en la cabecera, un recuento de proyecto que INV-36 no admite para el índice; dos enumeraciones a la vista (R-11 de V5) | La sección consume RACKLISTA y RACKBOMTOTAL, no el índice; los rótulos del índice siguen INV-36; SP-06 |
| R-11 | Rama de vida larga: muchas fases sin integración parcial; I-64 ya se rebasó dos veces | Q-09 |
| R-12 | Restaurar la ventana fuera de la pantalla (precedente: el STOP de Smoke-1 en SM-03) | INV-A14-03 |
| R-13 | Lanzar flujos clásicos que piden puntos o abren modales desde la ventana («Nuevo», «Abrir en editor clásico», las secciones): IC-01 midió que la vía en contexto de comando no daba el efecto de navegación esperado; para estos flujos la vía no está medida (`UNKNOWN`) | La fase 1 lo mide antes de implementar; sin efectos fuera de la sesión (D-03) |
| R-14 | Dos superficies por sistema tras el rediseño, si RACKEDITAR conserva la ventana de hoy (D-15 punto 1; OD `:36`; ADR-0029 D11 y D13) | Q-03, antes del paquete de la fase 6 |
| R-15 | La fase 8 choca con el no-objetivo de Insertar, con D-22 e INV-16 y con D-PB-04 | Q-04; su propia A-n |
| R-16 | El veto de cierre (D-17) tiene que cubrir los borradores nuevos (variables y «Nuevo, sin insertar») | Lo exige el paquete de la fase que los cree; INV-20 se extiende |

## 6. Lo que A-14 no hace

- No reabre F1, F2, F3, F4 ni F4B, ni sus hitos.
- No cambia el contrato de PB-T5 ni el Smoke-PB en curso; SPB-12 y SPB-13 conservan su resultado.
- No retira ningún comando ni decide la lista de retiro.
- No autoriza emitir ninguna fase: cada una necesita su paquete, su revisión y la orden del Owner.
- No toca `src/` ni `tests/`, ni `main`.
- No cambia la semántica de escritura ya cerrada (PREPARE / MUTATE / POST, todo o nada por rack, clasificación por frontera, un paso
  de UNDO); «Actualizar todo» la extiende a N racks sin cambiarla para un rack.
- No cambia el host ni la obligación de foco de D-01, ni D-03, D-04, D-05, D-07, D-10, D-11, D-12, D-14 o D-17 en su semántica.
- No modifica A-1..A-13 ni IC-01..IC-11.
- No decide la fase 8 (Q-04) ni qué pasa con la ventana clásica de cada sistema rediseñado (Q-03).
- No fija umbrales de rendimiento (D-18) ni cambia las reglas de host de §12.1.
- No clasifica ID3: sigue en READY.
- No acepta ADR-0047 (propuesto); como un ADR propuesto puede editarse hasta su aceptación (`docs/adr/README.md`), se actualiza para
  reflejar A-14 cuando A-14 esté activa.

## 7. Materialidad

Propuesta del borrador; la confirma el Coordinator (LIFECYCLE §3). El arquetipo sigue siendo NEW ARCHITECTURE.

| ID | Estado | Razón |
|---|---|---|
| M-01 | **YES** | «Actualizar todo» acepta una escritura de varios racks; nace un borrador de variables por sesión de documento; la ventana guarda su rectángulo |
| M-02 | **YES** | Se rompe la condición de §14 `:995` («sin campos nuevos … (D-22)»): el rectángulo en los ajustes de usuario; la fase 8 tocaría la identidad |
| M-03 | **YES** | Se rompe la condición de §14 `:996`: RACKCAD deja de abrir el menú modal; la fase 9 retirará comandos |
| M-04 | **YES** | Semántica de fallo nueva: todo o nada sobre varios racks, Desconocido para el conjunto, la lista de lo que falló; el cierre con dirty cambia de forma (C-02) |
| M-05 | **YES** | El contrato de adaptador se realinea (C-07) y la costura de I-55 se consume para N racks |
| M-06 | **YES** | Se implementan puntos de extensión de D-23: el borrador por lotes y la región de variables |
| M-07 | **YES** | La ventana pasa a alojar toda la operación de RackCad |
| M-08 | **YES (por prudencia)** | La nota prevista a ADR-0010 (anexo B `:1219-1221`) dice «todo o nada el authored del rack»; las variables en «Actualizar todo» tocan la autoridad de ADR-0034 |

```text
M-01 = YES
M-02 = YES
M-03 = YES
M-04 = YES
M-05 = YES
M-06 = YES
M-07 = YES
M-08 = YES (por prudencia)

Owner = REQUIRED (OWNER-RESERVED: alcance, no-objetivos, sustitucion de OV-08, OV-09 y OV-14, texto literal de D-PB-01)
Architect = REQUIRED (PENDING; sin orden de revision)
Coordinator = PENDING
```

## 8. Camino de aprobación

1. Este borrador v1 pasa por una revisión independiente adversarial y da una v2, como el paquete de F4B (IR2, IR3).
2. El Coordinator confirma la materialidad y la autoridad (LIFECYCLE §3 y §6) y ordena **una** revisión formal del Architect del
   objeto exacto (commit), con la celda acreditada o una sonda nueva si la elegibilidad cambió.
3. El Architect revisa en solo lectura la versión completa: V5, A-1..A-13, IC-01..IC-11, el paquete de F4B y A-14 (LIFECYCLE §5).
   Resultado: AGREED o CHANGES REQUIRED.
4. Coordinator AGREED.
5. El Owner firma el texto exacto (SATISFIED), en especial lo `OWNER-RESERVED`.
6. Se versionan A-14 (`docs/initiatives/I-64-A-14.md`, append-only), el registro literal de las decisiones del Owner, la replaneación y
   el prototipo (este último sin datos de ejemplo que parezcan rutas locales), y se actualiza el estado de I-64.
7. Activación. A-14 no cambia F4B, así que puede activarse con F4B abierta; el paquete de la fase 1 espera a que F4B cierre.

## 9. Preguntas pendientes

| ID | Pregunta | Para |
|---|---|---|
| Q-01 | La lista de comandos a retirar, antes de la fase 9 | Owner |
| Q-02 | Las variables en «Actualizar todo»: ¿la fase 4 escribe solo racks y las variables entran cuando cierre la 5, o la 4 espera a la 5? | Owner + Coordinator |
| Q-03 | Tras el rediseño de un sistema, ¿RACKEDITAR adopta el diseño nuevo (cambio observable declarado, como D-PB-06) o conserva la ventana de hoy (dos superficies, contra D-15 punto 1 y OD `:36`)? | Owner + Architect |
| Q-04 | Fase 8: Insertar desde la ventana, la identidad nueva (D-22, INV-16), la creación de vistas (D-PB-04) y el nombre (I-60); con su A-n | Owner + Architect + Coordinator |
| Q-05 | ¿La autorización de F5-A («AUTHORIZED / WAITING FOR F4B», OD `:269`) queda sustituida por la orden propia de la fase 6? | Owner |
| Q-06 | La sustitución de OV-08, OV-09 y OV-14 | Owner |
| Q-07 | El alias RK de RACKCAD: ¿abre también la ventana? ¿El menú modal queda accesible por otra vía hasta la fase 9? | Owner |
| Q-08 | El prototipo muestra en Configuración «Abrir RackCad al iniciar AutoCAD», que nadie ha decidido; A-14 no lo incluye (cambiaría P-01 y la restauración entre arranques) | Owner |
| Q-09 | Con diez fases por delante, ¿se mantiene «sin integración parcial» (A64-PV1-21) o el Owner decide integrar por tramos? | Owner |
| Q-10 | «Abrir en editor clásico» (IC-03 lo deja para F5): con la lista de todos los racks en la fase 2, ¿se adelanta a la fase 2? | Coordinator + Owner |
| Q-11 | Dónde se versionan las decisiones del Owner, la replaneación y los prototipos (WORKFLOW §10 apunta a `docs/automation/decisions/<I>.md`; I-64 los ha guardado en sus carpetas de evidencia) | Coordinator |
| Q-12 | La celda del Architect para A-14 | Coordinator |

## 10. Evidencia

- **Freeze:** `docs/initiatives/I-64-proposal-v5.md`, blob `dc1924ff5a73a4e529fbc5f995621d29351a091b`.
- **F4B:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`; `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`
  (F4B-E01); `docs/initiatives/I-64-f4b-push-back-completo-package.md`; `docs/initiatives/I-64-implementation-clarifications.md`
  (IC-03, IC-11).
- **PB-T5:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md` (SPB-12, SPB-13, SPB-23).
- **Producto:** `src/RackCad.Plugin/RackMenuCommands.cs`; `src/RackCad.UI/RackMainMenuWindow.xaml`;
  `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs`; `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs`;
  `src/RackCad.Application/Settings/UserSettings.cs`.
- **Decisiones del Owner del 2026-11-23, replaneación y prototipo:** PENDING (Q-11).
- **Evidencia de la unidad:** la sección de `docs/automation/evidence/I-64-evidence.md` que registre A-14: PENDING.

Este artefacto es append-only desde su commit. Cualquier corrección, revocación o sustitución requiere la siguiente A-n.
