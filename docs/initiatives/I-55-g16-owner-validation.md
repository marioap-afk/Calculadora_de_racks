# I-55 — G16: matriz canonica de Owner Validation

Documento de procedimiento del gate final. Contiene la matriz **canonica** de validacion del Owner en AutoCAD 2025
de I-55: la del [mapa de implementacion V4](I-55-implementation-map-v4.md) (parte OV) con las enmiendas del
[mapa V5](I-55-implementation-map-v5.md) (parte OV) y las decisiones del Owner ya congeladas
([decisions/I-55.md](../automation/decisions/I-55.md): OD-1..OD-8, M-01 con `phi_t = 0`, `alpha = 0`, OD-7.e A = Relative Frame
Window; desde C16-06 su alcance por orientacion esta en el diseño [C16-06 §G.1](I-55-c16-06-projected-orientation.md)). No es una lista abreviada. Los resultados por escenario, el SHA del Candidato y la identidad del DLL **no** viven aqui:
se registran en la evidencia de cierre, porque este archivo forma parte del Candidato y no puede contener su propio SHA.

Los escenarios que dependen de un fixture que el Owner no tenga se registran **NOT EXECUTED** con el motivo (solo cuando la
propia fila lo permite con «si el Owner dispone de uno»). Ningun escenario se omite en silencio. Un resultado por fila:
`PASS`, `FAIL` o `NOT EXECUTED (motivo)`; no hay «casi pasa».

## 0. Preparacion comun

- Cerrar AutoCAD antes de compilar. Worktree `~/.codex/worktrees/feature-creacion-de-vistas`, arbol limpio, `HEAD` = `FINAL_CANDIDATE_SHA`.
- Antes de NETLOAD registrar: SHA del Candidato, `InformationalVersion` del DLL (debe identificar el SHA), SHA-256 del DLL, version exacta de
  AutoCAD, ruta resuelta y SHA-256 de `blocks-library.dwg` (con el override de `%APPDATA%\RackCad\settings.json` si existe) y los DWG de prueba.
- Dibujo en pulgadas. Un dibujo de prueba recuperable. Para **OV-RED-06** iniciar AutoCAD con la variable de entorno
  `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT=2` (solo el DLL Debug); retirarla despues.
- Opcional: duracion activa del Owner (metrica experimental de I-45).
- La aprobacion la declara el Owner de forma explicita: **APPROVED** o **REJECTED**.

## 1. OV-LEG (vista unica) y OV-BOM

| # | Pasos | Esperado |
|---|---|---|
| OV-LEG-01..06 | Insercion de vista unica y `RACKEDITAR` → Insertar en Selectivo, Dinamico, Push Back compuesto, Cantilever, cabecera y `QUICKCAMA` | Igual que antes de I-55 |
| OV-LEG-07 | `Esc` y `Enter` en cada jig | No queda definicion con datos de RackCad |
| OV-BOM-01 | `RACKBOMTOTAL` y `RACKLISTA` sobre un dibujo con los seis kinds, incluido un Push Back con diagnostico bloqueante y una linea Cantilever invalida (y un Dinamico solo-sistema si el Owner dispone de uno) | Igual que antes de I-55 |

## 2. OV-ID17 (primera vista libre)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID17-01 | Selectivo nuevo (2 fondos) → Insertar planta; `RACKEDITAR` → frontal fondo 2 | 1 rack, 2 vistas; mismo diseno desde cualquier vista |
| OV-ID17-02 | Selectivo nuevo → Insertar lateral → `RACKEDITAR` → frontal y planta | 1 rack, 3 vistas |
| OV-ID17-03..05 | Dinamico nuevo con frontal salida, frontal entrada o planta primero → `RACKEDITAR` → lateral | 1 rack por caso |
| OV-ID17-06 | `RACKCABECERA` → planta primero → `RACKEDITAR` → lateral | 1 cabecera, 2 vistas |
| OV-ID17-07 | Push Back y Cantilever: cada vista como primera | Sin regresion |
| OV-ID17-08 | `Esc` o `Enter` en el jig de una primera vista no frontal | Nada queda |
| OV-ID17-09 | Guardar, cerrar, reabrir → `RACKBOMTOTAL` | Cantidades de 1 rack |
| OV-ID17-10 | Rack legado con `Id` en blanco (si existe) → `RACKEDITAR` → Insertar | Se cura como hoy; la vista nueva comparte el id y sus propiedades |

## 3. OV-ID18 (cola de vistas) y OV-RED (redibujo atomico de hermanas)

| # | Pasos | Esperado |
|---|---|---|
| OV-ID18-01 | Selectivo nuevo → lote frontal fondo 1 + lateral poste 2 + planta | Tres prompts con nombre y posicion; 1 rack, 3 vistas |
| OV-ID18-02 / 03 | `Esc` en la segunda / `Enter` en la ultima | Solo frontal / frontal y lateral; mensaje de cola parcial |
| OV-ID18-04 / 05 | `RACKEDITAR` desde la lateral → Actualizar; guardar, reabrir, editar desde la planta | Las tres cambian; mismo diseno |
| OV-ID18-06 | `U` tras un lote | Registrar lo observado (R-07) |
| OV-ID18-07 | Dinamico existente → lote entrada + planta → `Esc` en la primera colocacion | Redibujo conservado; sin vistas nuevas; el mensaje dice que las vistas existentes se actualizaron y no se inserto ninguna |
| OV-ID18-08 | Push Back compuesto → frontal A, frontal B y lateral tras una frontera suprimida | Tres vistas; poste real |
| OV-ID18-09 | Cantilever → laterales de dos estaciones + planta | Tres vistas; una regeneracion al final |
| OV-ID18-10 | Selectivo existente de 2 fondos → `RACKEDITAR` → en la lista de vistas a insertar elegir **frontal fondo 2** → Insertar. Repetir y, antes de confirmar la insercion, cancelar la lista (o cerrar el editor sin Insertar) | Se inserta la frontal del fondo 2 **sin** un prompt «que frontal» (G12 lleva la direccion tipada elegida en la lista). Al cancelar antes de confirmar: nada modificado, ni redibujo ni vistas nuevas. *(Correccion de la matriz, C16: el prompt «que frontal» es del camino legado de insercion directa; la costura aceptada de G12 es la lista de direcciones tipadas.)* |
| OV-ID18-11 | Selectivo cuya unica vista es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta | La planta nueva se coloca y la frontal huerfana desaparece en el mismo paso; `RACKBOMTOTAL` cuenta el rack |
| OV-RED-01 | Selectivo con frontal, lateral y planta → `RACKEDITAR` → cambiar el diseno → Insertar **una** vista; repetir en Dinamico, Push Back compuesto, Cantilever y cabecera | En cada kind, las hermanas cambian juntas antes de pedir el punto; despues el jig de la vista nueva |
| OV-RED-02 | Igual que OV-RED-01 → `Esc` en el jig | Las hermanas quedan actualizadas; ninguna vista nueva |
| OV-RED-03 | Igual que OV-RED-01 con una hermana en una capa bloqueada | «No se inserto ninguna vista y no se modifico ninguna vista existente: <vista> esta en la capa bloqueada <capa>» (`PREPARE_FAILED`), con la vista y el remedio; ninguna hermana cambia (si no ocurre, registrar lo observado: la evidencia de rollback es `RackSiblingRedrawRunTests`) |
| OV-RED-04 | Selectivo cuya unica vista propia es la frontal del fondo 2 → encoger a 1 fondo → Insertar planta → `Esc` en el jig; repetir y colocar | Con `Esc`, nada cambia y la frontal huerfana sigue (ni definicion nueva ni borrado; comprobar con `PURGE`); al colocar, la planta aparece y la huerfana desaparece en el mismo paso. Con el DLL Debug; si AutoCAD rechaza el arrastre: registrar y detener (R-28) |
| OV-RED-05 | Selectivo con F/L/P → cambiar → Insertar dos vistas → colocar la primera → `Esc` en la segunda; repetir en Dinamico, Push Back compuesto, Cantilever y cabecera | Hermanas actualizadas antes del primer jig; redibujo y primera vista conservados; la segunda no aparece; registrar la duracion (R-17) |
| OV-RED-06 | DLL Debug con la inyeccion de fallo en la segunda unidad → Insertar en un Selectivo con F/L/P modificado | `REDRAW_ROLLED_BACK`, ninguna hermana cambio (evidencia fisica del rollback) |
| OV-RED-07 | Selectivo con una lateral elegida de `Id` en blanco (si el Owner dispone de uno) → `RACKEDITAR` → Insertar planta | La lateral se redibuja con el `Id` curado y la planta nueva comparte ese `Id` (IC-10) |

## 4. OV-META

| # | Pasos | Esperado |
|---|---|---|
| OV-META-01 | `RACKPROPIEDADES` en un rack → insertar una vista y un lote | Las vistas nuevas tienen las mismas propiedades |
| OV-META-02 | Cotas desactivadas en frontal → lote F + L + P | Cada vista segun su politica |
| OV-META-03 | Rack con vinculo a variable → lote | El vinculo sigue en todas |
| OV-META-04 | Rack cuyas vistas tienen propiedades distintas (si el Owner dispone de uno) → `RACKEDITAR` → Insertar | Rechazo con remedio; nada cambia; Actualizar sigue funcionando; tras unificar con `RACKPROPIEDADES`, Insertar procede |
| OV-META-05 | Dibujo con un bloque RackCad de **otro** rack que este build no interpreta (si el Owner dispone de uno) → `RACKEDITAR` → Insertar en un rack sano | La insercion procede; si no hay bloque disponible, la evidencia es `RackSiblingCustomPropertiesGateTests` |
| OV-META-06 | Rack con propiedades distintas y un bloque no interpretable en el dibujo (si el Owner dispone de ambos) → Insertar → `RACKPROPIEDADES` | Insertar rechaza con el remedio condicionado; `RACKPROPIEDADES` queda en solo lectura y dice la causa |

## 5. OV-PR (correcciones previas de producto)

| # | Pasos | Esperado |
|---|---|---|
| OV-PR1-01 | Push Back con frontera suprimida **antes** del corte elegido → lateral | Poste real |
| OV-PR2-01 | Cantilever con brazos y tensores **visibles** en planta → insertar planta; `RACKEDITAR` → Actualizar | Coincide con la vista previa en ambos casos |
| OV-PR2-02 | Cantilever con brazos y tensores ocultos → insertar planta | Igual que antes de G5 |

## 6. OV-ID19 (`RACKPROYECTAR` / `RPY`)

Decisiones aplicadas: misma clase = Rigid con `alpha = 0`; planta ↔ elevaciones = Orthographic con **Relative Frame Window** (nunca mayoria);
Frontal ↔ Lateral = Orthographic sobre la altura desde C16-07 (antes no expuesto; ver §6.3); superposicion = aviso; tipos fuente mezclados, varias definiciones de un `RackId` y familias mezcladas fallan; orientacion natural
de las plantas proyectadas. Las filas 11, 22, 25 y 26 son las reescritas por el mapa V5.

**Orientacion (C16-06).** `RACKPROYECTAR` pregunta ahora `Orientacion [PRoyectada/PREdeterminada] <PRoyectada>` despues de la clase. Las filas de esta
tabla describen la geometria congelada de G14, que es la orientacion **Predeterminada** en las proyecciones entre clases distintas: en las filas 01, 02, 03,
04, 06 (parte ortografica), 07, 14, 21, 22, 25 y 27 responder **Predeterminada** (escribir `PRE` o elegirla en la lista). En las filas de la misma clase
05, 06 (parte rigida) y 26 responder **Proyectada** (Enter): conserva el giro de cada rack. En el resto la orientacion no cambia el resultado esperado
(Enter). La orientacion **Proyectada** entre clases distintas se valida en la seccion 6.1.

| # | Pasos | Esperado |
|---|---|---|
| OV-ID19-01 | `RACKLAYOUT` independiente de 1 fila × 4 (columnas a lo largo de la corrida) → proyectar las plantas a Frontal | Frontales lado a lado sobre una base comun, **en orden de R ascendente**, con la separacion entre ejes de poste 0 de las plantas; conteos identicos |
| OV-ID19-02 | Dos filas (2 × 4) → Frontal | Aviso de superposicion **antes de pedir puntos**, con los pares; frontales de filas distintas superpuestas |
| OV-ID19-03 | Filas escalonadas sin tramos comunes → Frontal | **Sin** aviso de superposicion |
| OV-ID19-04 | Las 4 frontales de OV-ID19-01 → Planta | Plantas en orientacion natural, sobre una linea comun, con el orden y la separacion originales |
| OV-ID19-05 | Plantas → **Planta** (misma clase) con destino desplazado | Vistas enlazadas del layout con la orientacion de cada rack; conteos identicos; aviso final de que no son copias |
| OV-ID19-06 | Layout de plantas **girado 30° con `ROTATE`** (la rejilla de `RACKLAYOUT` solo es correcta a 0/90/180/270) → Planta (rigido) y → Frontal (ortografico) | Rigido: layout y giros conservados; ortografico: frontales rectas con la separacion medida a lo largo de la corrida girada |
| OV-ID19-07 | `RACKLAYOUT` enlazado 1 × 4 → Frontal | 4 referencias de una definicion nueva; 1 rack con 4 copias antes y despues (`RACKLISTA`, `RACKBOMTOTAL`) |
| OV-ID19-08 | `RACKEDITAR` sobre una vista proyectada → Actualizar | Todas las vistas del rack cambian |
| OV-ID19-09 | Frontal ↔ Lateral | **Supersedida por C16-07** (decision del Owner): se proyecta; se valida en §6.3 OV-C16-07. *(Antes: «No se expone: mensaje sin escribir».)* |
| OV-ID19-10 | Mezclas invalidas: plantas y una frontal; frontal y planta del mismo rack; Cama; MINSERT; escala ≠ 1 | Error explicito **sin pedir puntos** y sin escribir |
| OV-ID19-11 | Vista con `View` desconocido; frontal de un fondo que ya no existe | Error con disposicion y el remedio de la tabla de la Proposal V5 §3.8 que corresponda (no siempre «RACKEDITAR»); nada escrito |
| OV-ID19-12 | Rack con propiedades divergentes en la seleccion; y aparte un bloque ajeno no interpretable | Lo primero falla con remedio; lo segundo no bloquea |
| OV-ID19-13 | `Esc` o `Enter` en punto base o destino | Nada escrito ni importado; `Enter` no vuelve a pedir el punto |
| OV-ID19-14 | Selectivo, Dinamico y Push Back de longitudes distintas en una corrida → Frontal | Frontales alineadas por el eje del poste 0, en orden de R ascendente |
| OV-ID19-15 | Cantilever y Selectivo con corridas paralelas → Frontal | Error explicito sin pedir puntos (familias mezcladas) |
| OV-ID19-16 | Rack con variable vinculada → proyectar → cambiar la variable | La vista usa el valor efectivo y se actualiza |
| OV-ID19-17 | Push Back con diagnostico bloqueante | Error que nombra el rack y el motivo |
| OV-ID19-18 | 100 o mas plantas → Frontal | Termina; registrar duracion y numero de racks |
| OV-ID19-19 | `RACKLAYOUT` enlazado con una celda espejada con `MIRROR` → cualquier clase | Error de reflexion sin pedir puntos |
| OV-ID19-20 | Dos plantas del mismo rack (duplicado legal) en la seleccion | Error que nombra el rack |
| OV-ID19-21 | **Cantilever: plantas → Frontal y → Lateral; frontales → Planta; laterales → Planta**, con un layout de lineas escalonadas en R (para Frontal) y otro escalonado en D (para Lateral) | Cada caso coloca con la separacion y el orden de la fuente sobre K; la ida y vuelta restituye el intervalo de cada linea sobre K (no la coordenada descartada ni las orientaciones) |
| OV-ID19-22 | Fila de plantas con un rack girado 180° → Frontal; despues, girar 180° racks suficientes para ser mayoria → Frontal | Con OD-7.e = A el orden sobre la corrida **no cambia** en ninguno de los dos pasos; registrar lo observado |
| OV-ID19-23 | Dinamico solo-sistema (si el Owner dispone de uno) → proyectar | Se proyecta si su paridad lo permite; si no, error que nombra el rack y nada escrito |
| OV-ID19-24 | Push Back con una hermana de descriptor invalido (si el Owner dispone de uno) → proyectar | Error que nombra el rack y remite a `RACKEDITAR`; nada escrito |
| OV-ID19-25 | Ida y vuelta Planta → Frontal → Planta de la fila anterior con mayoria girada | Conserva los tramos; no promete recuperar las orientaciones descartadas salvo traslacion |
| OV-ID19-26 | Layout con una planta de `Origin ≠ 0` (o `RACKLAYOUT` sobre un bloque redefinido con punto base desplazado) → Planta (Rigid) | Cada vista nueva queda en la posicion del ancla fuente trasladada, sin el desplazamiento del `Origin`; la definicion nueva tiene `Origin = 0` |
| OV-ID19-27 | Corrida cuyo angulo cae junto al limite de la ventana relativa (`3π/4`) | Aviso de cercania antes de pedir el punto; no bloquea |
| OV-ID19-28 | Pieza requerida cuyo bloque no esta en `blocks-library.dwg`; repetir con la biblioteca ausente | Error **antes de pedir puntos**, con la pieza y la causa; nada escrito. Un bloque **opcional** ausente solo avisa |
| OV-ID19-29 | Proyeccion exitosa (varios racks, cada clase) | Todas las vistas previstas aparecen; los conteos de racks y el BOM no cambian; comprobar con `PURGE` que no hay definiciones de rack sin referencias tras un fallo previo al punto o una cancelacion |
| OV-ID19-30 | Forzar un fallo de materializacion si es practicamente posible (p. ej. destino en una capa bloqueada) | Se deshace la operacion completa: ninguna definicion ni referencia nueva. Si no se puede inducir, `NOT EXECUTED` con el motivo (la garantia de rollback de documento esta acreditada por el dictamen de host de AUTH-15) |
| OV-ID19-31 | Guardar, cerrar y reabrir despues de proyectar; `RACKLISTA`/`RACKBOMTOTAL` antes y despues | Las vistas persisten con el mismo `RackId`; mismos racks y mismo BOM; solo cambia el numero de vistas |
| OV-ID19-32 | `RPY` y `RACKAYUDA` | El alias ejecuta el mismo comando; la ayuda documenta `RACKPROYECTAR / RPY` |

Las filas 27..32 son de aceptacion del Candidato (aviso de cercania, biblioteca, ausencia de huerfanas, rollback, persistencia y ayuda): completan los
criterios del gate G15 sin cambiar la matriz congelada 01..26.

## 6.1 OV-C16-06 (orientacion de la vista proyectada)

Diseño congelado: [I-55-c16-06-projected-orientation.md](I-55-c16-06-projected-orientation.md). **Solo gira la referencia de bloque colocada, entera**:
geometria, textos, cotas, numeros y etiquetas giran con ella (no se intenta mantener los textos derechos). La definicion de bloque NO cambia: comprobar con
`BEDIT` o `LIST` de la definicion que su contenido es el de siempre; `LIST` de la referencia muestra el angulo de rotacion.

Como leer una fila: gira la vista fuente con `ROTATE` (angulo exacto, sobre su punto de insercion) **antes** de `RACKPROYECTAR`; despues
`RACKPROYECTAR` → seleccion → clase → orientacion → punto base → punto destino, y mide con `LIST` la **rotacion** de cada referencia nueva.
Para el Selectivo (y Dinamico, Push Back y cabecera) la planta tal como se inserta (0°) tiene la **corrida en vertical** (eje Y) y el fondo en horizontal;
la frontal y la lateral tal como se insertan tienen su ancho en horizontal. Esperado general de Proyectada: la corrida (o el fondo, segun la clase) de la vista
nueva queda **paralela y en el mismo sentido** que en la vista fuente, y las vistas nuevas quedan alineadas sobre una recta que pasa por el punto destino.
El otro eje de la vista nueva (la altura de una elevacion, el otro eje de una planta) queda donde lo pone ese giro: en cada fila anotar hacia donde apuntan la
parte superior de los postes o el fondo. Si una vista se ve «desde atras» (orden de postes invertido respecto a la fuente), es un FAIL; que la altura quede de
lado o de cabeza es el comportamiento esperado.

| # | Pasos (Selectivo salvo que se diga) | Esperado |
|---|---|---|
| P-01 | Planta a 0° (corrida vertical) → Frontal, **Proyectada** | Frontal girada **90°** (queda vertical: su corrida sigue la de la planta) |
| P-02 | Planta a 90° (corrida horizontal, poste 1 a la derecha) → Frontal, Proyectada | Frontal a **180°** (de cabeza), poste 1 a la derecha como en la planta |
| P-03 | Planta a 180° → Frontal, Proyectada | Frontal a **270°** (vertical, sentido opuesto a P-01) |
| P-04 | Planta a 270° (corrida horizontal, poste 1 a la izquierda) → Frontal, Proyectada | Frontal a **0°**: la frontal normal |
| P-05 | Planta a 0° → Lateral, Proyectada | Lateral a **0°** (su fondo, horizontal, sigue el fondo de la planta) |
| P-06 | Planta a 90° → Lateral, Proyectada | Lateral a **90°** |
| P-07 | Planta a 180° → Lateral, Proyectada | Lateral a **180°** |
| P-08 | Planta a 270° → Lateral, Proyectada | Lateral a **270°** |
| P-09 | Frontal a 0° → Planta, Proyectada | Planta a **270°** (corrida horizontal, como la frontal) |
| P-10 | Frontal a 90° → Planta, Proyectada | Planta a **0°** |
| P-11 | Frontal a 180° → Planta, Proyectada | Planta a **90°** |
| P-12 | Frontal a 270° → Planta, Proyectada | Planta a **180°** |
| P-13 | Lateral a 0° → Planta, Proyectada | Planta a **0°** (su fondo sigue el de la lateral) |
| P-14 | Lateral a 90° → Planta, Proyectada | Planta a **90°** |
| P-15 | Lateral a 180° → Planta, Proyectada | Planta a **180°** |
| P-16 | Lateral a 270° → Planta, Proyectada | Planta a **270°** |
| P-17 | Dos plantas **con el mismo giro** (p. ej. ambas a 90°, en posiciones distintas) → Frontal, Proyectada | Ambas frontales con la **misma** rotacion (180°), alineadas sobre una recta que pasa por el punto destino en la direccion de la corrida |
| P-18 | Dos plantas **opuestas** (una a 0° y otra a 180°) → Frontal, Proyectada | Rechazo **antes de pedir puntos**: `SourceOrientationDivergent` con los dos racks y el remedio (proyectar por separado o usar Predeterminada); nada escrito ni importado |
| P-19 | Dinamico: planta a 90° → Frontal (salida), Proyectada | Frontal a **180°** (igual regla que el Selectivo) |
| P-20 | Push Back: lateral a 90° → Planta, Proyectada | Planta a **90°** |
| P-21 | Cabecera: planta a 90° → Lateral, Proyectada | Lateral a **90°** |
| P-22 | Cantilever (su planta a 0° tiene la corrida **horizontal**): planta a 90° → Frontal, Proyectada; y planta a 0° → Lateral | Frontal a **90°**; lateral a **90°** |
| P-23 | Misma clase: planta a 90° → Planta, Proyectada | La planta nueva conserva **90°** (copia rigida del giro) |
| C-01 | Planta a 90° → Frontal, **Predeterminada** | Frontal a **0°** (presentacion normal), como antes de C16-06 |
| C-02 | Planta a 90° → Lateral, Predeterminada | Lateral a **0°** |
| C-03 | Frontal a 90° → Planta, Predeterminada | Planta a **0°** |
| C-04 | Lateral a 90° → Planta, Predeterminada | Planta a **0°** |
| C-05 | Misma clase: planta a 90° → Planta, Predeterminada, eligiendo como punto base una esquina de la planta | Planta a **0°** (`LIST` puede mostrar 0° o 360°); la esquina elegida cae en el punto destino (correccion aceptada de C16-06) |
| C-05b | Misma clase: **dos** plantas contiguas **ambas a 90°** (con pasillo entre ellas) → Planta, Predeterminada | Ambas a **0°**, contiguas y con el mismo pasillo: la disposicion se conserva, girada alrededor del punto base; ninguna se superpone |
| C-05c | Misma clase: dos plantas con **giros distintos** (0° y 180°) → Planta, Predeterminada | Rechazo **antes de pedir puntos**: `SourceRotationsDiffer` con los dos racks y el remedio (usar Proyectada o proyectar cada giro por separado); nada escrito |
| C-06 | Las dos plantas opuestas de P-18 → Frontal, **Predeterminada** | Se proyecta (Predeterminada nunca se bloquea por la divergencia de Proyectada) |
| C-07 | En cualquier fila: `Enter` en la pregunta de orientacion; despues `PR`; despues `PRE` | `Enter` y `PR` usan **Proyectada**; `PRE` usa **Predeterminada**; `Esc` en esa pregunta cancela sin leer ni escribir nada |

## 6.2 OV-UNNAMED (racks sin nombre heredados)

| # | Pasos | Esperado |
|---|---|---|
| U-01 | Selectivo **con nombre** → planta → `RACKPROYECTAR` → Lateral | PASS: se crea la definicion y la referencia; mismo `RackId`; la vista nueva lleva el **mismo nombre logico** (`RACKLISTA`) |
| U-02 | Selectivo **sin nombre** (nombre en blanco en su editor, o un rack heredado sin nombre): `RACKLISTA` antes → `RACKPROYECTAR` planta → Lateral → `RACKLISTA` despues → guardar, cerrar y reabrir → `RACKLISTA` | Antes: «(sin nombre)». La proyeccion **se completa** (definicion y referencia creadas), mismo `RackId`, sin nombre inventado («Rack», «Selectivo», «Sin nombre»…). Despues y tras reabrir: sigue «(sin nombre)»; un solo rack; el nombre del **bloque** de AutoCAD no esta vacio |

## 6.3 OV-C16-07 (proyeccion entre elevaciones: Frontal ↔ Lateral)

Diseño congelado: [I-55-c16-07-elevation-projection.md](I-55-c16-07-elevation-projection.md). La Frontal y la Lateral solo comparten la **altura**: la vista
nueva se alinea por su **linea de suelo** y su altura queda **paralela y en el mismo sentido** que la de la vista origen (Proyectada) o vertical hacia arriba
(Predeterminada). Nada se gira dentro de la definicion; solo la referencia de bloque, entera. La variante destino es la canonica del rack: la lateral del
**poste 1** (Cantilever: estacion 1) y la frontal del **fondo 1** (Dinamico: salida; Push Back: Entrada/Salida lado A).

Como leer una fila: gira la vista fuente con `ROTATE` (angulo exacto, sobre su punto de insercion) **antes** de `RACKPROYECTAR`; elige como **punto base** un
punto de la **linea de suelo** de la vista origen y un punto destino libre; mide con `LIST` la **rotacion** de la referencia nueva. Esperado de posicion: la linea de
suelo de la vista nueva pasa por el punto destino; en Predeterminada ademas la cara del frente (fondo 0) de la lateral, o el eje del primer poste de la frontal, queda
en la vertical del punto destino.

| # | Pasos (Selectivo salvo que se diga) | Esperado |
|---|---|---|
| OV-C16-07-01 | Frontal sin girar (0°) → Lateral, **Proyectada** (`Enter`) | Se completa (ya no `PairNotExposed`). Lateral a **0°**: la vista normal, suelo alineado con el destino; un solo rack, mismo `RackId` |
| OV-C16-07-02 | Lateral sin girar (0°) → Frontal, **Proyectada** | Frontal a **0°**, suelo alineado con el destino |
| OV-C16-07-03 | Frontal **girada 90°** → Lateral, Proyectada | Lateral a **90°** (su altura apunta hacia donde apunta la de la frontal); repetir a 180° y 270°: lateral a **180°** y **270°** |
| OV-C16-07-04 | Lateral **girada 90°** → Frontal, Proyectada | Frontal a **90°**; repetir a 180° y 270°: frontal a **180°** y **270°** |
| OV-C16-07-05 | Frontal girada 90° → Lateral, **Predeterminada** (`PRE`) | Lateral a **0°** (presentacion normal de RackCad), con su suelo en la horizontal indicada arriba |
| OV-C16-07-06 | Lateral girada 90° → Frontal, **Predeterminada** | Frontal a **0°** |
| OV-C16-07-07 | Dinamico: frontal (salida) → Lateral y lateral → Frontal, Proyectada, sin girar | Ambas se completan a **0°** (lateral del poste 1; frontal de salida) |
| OV-C16-07-08 | Push Back: lateral girada 90° → Frontal, Proyectada | Frontal (Entrada/Salida, lado A) a **90°** |
| OV-C16-07-09 | Cantilever: frontal → Lateral (Proyectada, sin girar) y lateral de una estacion → Frontal (Predeterminada) | Lateral de la estacion 1 a **0°**; frontal a **0°** |
| OV-C16-07-10 | Cabecera: lateral → Frontal | Rechazo **antes de pedir puntos**: `PairNotExposed` («Esta vista no se puede proyectar»): la cabecera no tiene frontal; nada escrito |
| OV-C16-07-11 | Dos frontales de **una misma fila** (mismo suelo, sin girar) → Lateral, Proyectada | Aviso de **superposicion** antes de pedir puntos; las dos laterales quedan en el mismo lugar (es la proyeccion ortografica: una fila vista de lado) |
| OV-C16-07-12 | Dos frontales **opuestas** (una a 0° y otra a 180°) → Lateral, Proyectada; despues lo mismo en Predeterminada | Proyectada: rechazo **antes de pedir puntos** con `SourceOrientationDivergent` y los dos racks; Predeterminada: se proyecta, ambas laterales a 0° |
| OV-C16-07-13 | Selectivo **sin nombre**: frontal → Lateral; `RACKLISTA` antes y despues; guardar, cerrar y reabrir | Se completa; mismo `RackId`; sigue «(sin nombre)» |
| OV-C16-07-14 | Tras OV-C16-07-01: `RACKLISTA`, `RACKBOMTOTAL`, guardar/reabrir y `RACKEDITAR` desde la lateral proyectada → Actualizar | Un rack con una vista mas; mismo BOM; `RACKEDITAR` actualiza todas las vistas del rack |

## 7. Registro

Cada escenario se registra con: identificador, `PASS` | `FAIL` | `NOT EXECUTED (motivo)`, observaciones y, en un fallo, su clasificacion
(defecto de producto, de prueba o fixture, de entorno, de documentacion o contradiccion material). El veredicto global lo declara el Owner:
**APPROVED** o **REJECTED**. Un solo `FAIL` requerido detiene la integracion.

## 8. Rondas de validacion

Los registros de una ronda rechazada se conservan; no se reescriben ni se reutilizan para el Candidato siguiente.

### Ronda 1 — Candidato `beb9597b42d67bb118b230124946dbe9004b31ae`: REJECTED

Evidencia historica del Candidato rechazado: CI de push `36638354422` (4/4), cobertura `36639301144` (medida = Candidato), Core 11723/0/0 y UI
1621/0/17 omitidas. El Owner reporto **OV-LEG correcto** sobre ese SHA y rechazo el Candidato por las filas requeridas de OV-ID17:

| Fila | Observado en `beb9597b` | Clasificacion |
|---|---|---|
| OV-ID17-02 | Selectivo nuevo con Lateral primero: `RackCad ID18: PREFLIGHT_FAILED (0/0). ONE_RACK_REQUIRED`; la lateral no se inserta | Defecto de producto (C16-01) |
| OV-ID17-03..05 | Dinamico nuevo (frontal salida, entrada o planta primero) → `RACKEDITAR` → Insertar lateral: «una vista de este rack tiene un diseno interior de otro tipo» | Defecto de producto (C16-02) |
| OV-ID17-06 | `RACKCABECERA` → planta primero → `RACKEDITAR` → Insertar lateral: el mismo error | Defecto de producto (C16-02) |

**C16-01 — causa raiz.** No era un requisito de rack existente que entrara en el camino de rack nuevo: la peticion que el menu entrega al Plugin llegaba
con la lista ordenada de vistas **vacia**. Un lateral del Selectivo lleva su poste como seccion y la peticion historica de vista unica no la tiene, asi que
`SelectiveInsertionRequest` no producia ninguna direccion; ademas, los modulos del menu de Selectivo y Dinamico reconstruian la peticion desde el token de vista
legado y descartaban una cola. El plan de lote veia `Count == 0` y respondia `ONE_RACK_REQUIRED` (0/0), regla que **se conserva** para rack nuevo y existente.
Simbolos: `SelectiveInsertionRequest` (`RackInsertionRequest.cs`), `SelectiveEditorModule.Build` y `DynamicEditorModule.Build` (`EditorModules.cs`).

**C16-02 — causa raiz.** El diseno interior que persisten todas las primeras vistas es correcto y del tipo esperado (oraculo con los lectores de produccion,
para Dinamico, Cabecera, Push Back y Cantilever, en cada vista soportada, y Selectivo por su lector). Lo que fallaba era
`RackUnsupportedSiblingInsert.TryAuthorize`: entregaba a `RACKEDITAR` las definiciones de **todos** los racks del dibujo (`membership.Members`, con los ajenos como
`NotMember`) en lugar de las del rack que se edita (`MutableMembers`); el preflight de tipo erroneo, que no se toca, encontraba «otro tipo» en un rack ajeno. Con otro
rack del mismo tipo en el dibujo no abortaba, pero redibujaba ese rack con el diseno del que se edita. Lo usan Dinamico, Cabecera, Push Back y Cantilever.

**Correccion:** el bloque de `RACKEDITAR` sale de `MutableMembers`; el Selectivo lateral lleva el poste 0 como el Dinamico; los modulos devuelven la peticion de
la ventana. Ver el registro de la ronda 2 en la evidencia de cierre.

### Ronda 2 — Candidato `bc622f129e7348175042dcd2cb672d10f6dc5fc1`: REJECTED (OV-ID19-01)

OV-ID17 (smoke correctivo) y las secciones anteriores pasaron. `RACKPROYECTAR` no pudo materializar la primera definicion: AUTH-15 respondio
`InvalidEnvelope: sobre ausente o sin Id/Kind/Name`. **Campo:** `Name` (Id y Kind salen de la intencion aceptada). **Causa:** el sobre proyectado se compone desde
`source.Name`, el nombre visible del rack, que puede ser vacio; el nombre base del bloque es otra autoridad (AUTH-11) y no estaba vacio. **Correccion:**
`RackProjectionEnvelopeName.WithLogicalName` compone desde una copia en memoria del sobre fuente con un nombre usable; el sobre del dibujo no se repara y AUTH-15 no cambia.

### Ronda 3 — Candidato `50f6c8bff65944d3f62b62eec76ec0eaefe28d76`: REJECTED (OV-ID19-01)

Selectivo Planta → Lateral: AUTH-15 respondio `InvalidBlockName: nombre de bloque vacio` con el sobre ya valido (`Name = Rack`) y el nombre base vacio.
**Causa (C16-04):** para un rack sin nombre, `LinkedLateral(null, post)` devuelve null y la composicion del Selectivo no tenia el respaldo generado que si tienen el lateral
del Dinamico y el de Push Back; la insercion historica funcionaba porque sustituia el literal «Selectivo» por su cuenta, en el Plugin. **Oraculo historico:** nombre base = nombre del rack
o «Selectivo», seccion `LinkedLateral(base, pick − 1)` con `pick` el numero de poste FISICO (el corte se busca por `PostIndex == pick − 1`), por lo que el sufijo es `PostIndex + 1` y
no la posicion entre los cortes. **Correccion:** `RackViewBaseName.SelectiveLateral` (autoridad AUTH-11) y las composiciones de nombre por sistema extraidas a
`RackViewProductNames` (Application) para probarlas sin AutoCAD. **Observacion abierta (C16-05):** un rack sin nombre se veia como «(sin nombre)» en `RACKLISTA` y, al agregarle una
vista proyectada cuyo sobre llevaba el nombre «Rack» (AUTH-15 exige uno), pasaba a verse como «Rack»; resuelta por la decision de la ronda 4.

### Ronda 4 — Candidato `66a9a504`: REJECTED por decision del Coordinador (C16-05)

El Candidato no se compila para Owner Validation ni se ejecuta OV-ID19-01 sobre el. **Decision:** un rack logico con `Name` nulo, vacio o solo espacios NO es proyectable; ningun nombre
sintetico esta permitido («Rack» incluido), porque ID19 agrega una vista ligada del MISMO rack (no una identidad nueva, a diferencia de `RACKDUPLICAR`). Es politica de producto de I-55: no esta
en la Shared View Foundation, no debilita `InvalidEnvelope` ni el requisito de `Name` no vacio de AUTH-15 (que se cumple antes de materializar porque solo llegan racks con nombre).

**Politica:** `RackProjectionFailureCode.UnnamedRackNotProjectable`, etapa `Group` del plan puro G14, evaluada antes de `AUTHORITY GATES` (antes de cualquier `Services.Authored`). Falla la operacion completa y
reporta cada RackId sin nombre (OD-3). `RackProjectionServices.LogicalName` (servicio nuevo, obligatorio) lo respalda en el Plugin con `RackProjectionKindSession.LogicalName()`: el nombre que llevan las vistas del propio rack,
o null. `RackProjectionEnvelopeName.LogicalName` devuelve null sin nombre (sin respaldo) y `WithLogicalName` devuelve la fuente intacta; `RackDuplicationPlan.FallbackName` (agregado en la ronda 3) se revirtio.
Un rack con nombre conserva el MISMO nombre logico en el sobre proyectado; `Envelope.Name` y el nombre base del bloque siguen siendo autoridades distintas. C16-04 (`RackViewBaseName.SelectiveLateral`) se conserva.

**RED antes del arreglo (`G16UnnamedRackTests`, 23 de 31 en rojo):** A) Selectivo sin nombre llegaba a `Completed` (preparaba y materializaba); B) nombrado + sin nombre + nombrado tambien terminaba `Completed`;
C) Selectivo con nombre Planta→Lateral ya se aceptaba (verde, se conserva); D) sin cambios en el sobre fuente. Matriz de sistemas (Selectivo, Dinamico, Push Back, Cantilever, Cabecera × null/""/"   "): todas en rojo.
**Despues:** 31/31; se reescribieron los tests de la ronda 2/3 que fijaban el respaldo «Rack» (`G16ProjectedEnvelopeTests`, `G16BlockNameTests`).

### Ronda 5 — Candidato `3f06994adf7a30d8c2627b3fdc63393e4a489997`: REJECTED por decision de producto del Owner

No por un defecto frente a su orden: el Owner cambio el comportamiento requerido. **C16-05 queda REVOCADA**: `RACKPROYECTAR` DEBE proyectar racks heredados sin
nombre y conservarlos sin nombre (sin «Rack», «Selectivo» ni «Sin nombre» inventados). AUTH-15 se corrigio en su propia unidad (`I-52-AUTH15-C1`, integrada,
`integration/I-52-AUTH15-C1`): exige Id y Kind, nunca Name. I-55 se reconcilio sobre ese `main` y retiro `UnnamedRackNotProjectable` (filas OV-UNNAMED U-01/U-02).
En la misma ronda el Owner exigio **C16-06** (orientacion Proyectada por defecto y Predeterminada explicita; filas OV-C16-06 en §6.1) y corrigio la fila
**OV-ID18-10** (la costura de G12 es la lista de direcciones tipadas, no un prompt «que frontal»).

### Ronda 6 — Candidato `681152697822470d16cc5609349be0f2fc5421ae`: REJECTED por el Owner (C16-07)

La validacion del Owner expuso un hueco de producto: Selectivo Frontal → Lateral se detiene en `AVAILABLE` con `PairNotExposed`, con y sin nombre (no es un
defecto de nombre ni de AUTH-15). La exclusion Frontal ↔ Lateral (OD-7.a A) queda **REVOCADA** por el Owner: `RACKPROYECTAR` debe admitir Frontal → Lateral y
Lateral → Frontal, en Proyectada y en Predeterminada, en todo sistema con ambas clases. Correccion: C16-07 (filas en §6.3; la fila OV-ID19-09 queda supersedida).
