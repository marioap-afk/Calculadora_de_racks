# Ventana RackCad: lo construido (I-64, fuera de la cadena formal)

**Rama:** `architecture/workspace-persistente-rackcad-ventana`, desde `architecture/workspace-persistente-rackcad` (`c46f84d2`).

## Por qué existe esta rama

El Owner pidió el 2026-11-24 ejecutar la ventana sin planear antes:

> «¿Por qué no empezamos a ejecutar ventana por ventana y luego planeamos? Ya tenemos la principal y la de dinámico, vayamos con eso y después planeamos cantilever, requiero que trabajes por tu cuenta durante el día»

Y decidió cómo, con el cuadro de preguntas:
- **Proceso:** una rama nueva desde la de I-64, con pruebas, CI y una revisión independiente por ventana. Sin contrato ni Architect por ahora. La A-14 se pone al día después con lo construido. Nada llega a `main` sin la validación del Owner.
- **AutoCAD:** hoy no se usa. Solo código y pruebas; lo que necesita AutoCAD espera al Owner.
- **Alcance de la ventana principal:** las secciones, el menú de RACKCAD dentro y la lista de racks con buscador y filtros. RACKCAD abre la ventana, y Push Back sigue como hoy dentro de ella.
- **SPB-33 (vía A):** queda en pausa; primero van las ventanas.

Este documento es el registro de lo construido. Sirve para la planeación posterior y **no es un contrato.**

## Lo que hace la ventana

**RACKCAD, RK y RACKPANEL abren la misma paleta**, que no bloquea AutoCAD. Ahora es la ventana RackCad.

**Cabecera:** «RackCad», el número de racks del dibujo con los cambios sin escribir, y un botón para ocultar o mostrar la columna lateral.

**Columna lateral:**
- **Secciones:** Nuevo, Variables, BOM y lista de racks, Biblioteca de diseños y Configuración.
- **«Racks del dibujo»:**
  - buscador por nombre, sin distinguir mayúsculas ni acentos;
  - filtro por sistema;
  - «Solo con cambios sin escribir»;
  - «Seguir la selección de AutoCAD»;
  - «Mostrando N de M»;
  - los racks agrupados por sistema, en el orden Selectivo, Push Back, Dinámico, Cantilever, Cabecera, Cama, y después los demás. Dentro de cada grupo van por nombre, con los números en su orden natural (PB 2 antes que PB 10);
  - el punto de estado de cada rack: verde sin cambios, ámbar modificado, rojo si está posiblemente obsoleto o en conflicto.
- **Abajo:** «Volver a leer la lista» y «Menú clásico de RACKCAD» (RACKMENU).

**Centro:**
- **La página del rack activo** es el panel Workspace de F1 a F4B, con todas sus reglas (borrador, «Editar todo», reconciliación, navegación), sin la tira de pestañas ni su etiqueta. La lista sustituye a las pestañas.
- **Las secciones, en su paso intermedio,** abren su ventana actual por una orden de AutoCAD:

  | Sección | Qué abre |
  |---|---|
  | Nuevo (8 tarjetas) | `RACKNUEVO <sistema>`, o `RACKSECCION` para el perfil estructural |
  | Variables | `RACKVARIABLES` |
  | BOM y lista de racks | `RACKLISTA` y `RACKBOMTOTAL` |
  | Biblioteca de diseños | `RACKBIBLIOTECA` |
  | Configuración | La biblioteca de bloques (Examinar y Restablecer, como el menú) y la carpeta de diseños; no abre ninguna ventana |

## Decisiones de la sesión (para revisar en la planeación)

| Id | Decisión | Por qué |
|---|---|---|
| V-01 | La página del rack **es** el panel Workspace existente, dentro de la ventana; no se reescribió | Las reglas de F1 a F4B (borradores, límites del texto pendiente, «Editar todo», reconciliación) siguen intactas y con sus pruebas. Simplificarla es trabajo de la fase 3 |
| V-02 | La lista sale de los mismos hechos que el resumen del navegador (`ReadBrowserFacts`: definiciones con una referencia directa en espacio modelo) y del mismo acumulador. Se recuerda por sesión hasta un drenado que invalida las cachés de navegación | Una sola regla. El dibujo solo se lee en la ventana del drenado o por petición explícita, como el resumen |
| V-03 | Abrir un rack de la lista crea una vista previa, como lo hace la selección (`BrowserSession.Open`). Pasa por el límite bloqueante del texto pendiente | El modelo de pestañas sigue debajo. Así no se acumulan pestañas, y el borrador fija la pestaña como antes |
| V-04 | «Seguir la selección» apagado: la selección se consume, pero el rack activo se queda y el seleccionado espera detrás, con un recuadro «Seleccionado en AutoCAD: … [Abrir]» | Es lo del prototipo. Reutiliza `keepActive` de F4-T3B2 |
| V-05 | Las secciones lanzan órdenes con `SendStringToExecute`, solo sin comando en curso | No se escribe dentro de otro comando. Cada flujo corre en su propio contexto de comando, como desde la línea de órdenes |
| V-06 | `RACKNUEVO` y `RACKBIBLIOTECA` siguen la ruta exacta del menú: el mismo módulo del registro y el mismo despacho (`DispatchInsertion`, extraído de RACKCAD sin cambiar su cuerpo). Los editores tienen como dueña una ventana WPF corriente e invisible (no un tipo nuevo de ventana), mostrada con `ShowModalWindow`, que lanza el editor cuando se carga | Sin el menú, los editores necesitan una dueña modal para AutoCAD. Las órdenes por sistema (p. ej. `RACKPUSHBACK`) no son equivalentes: dibujan una sola vista, sin el lote de I-60 |
| V-07 | El menú clásico pasa a `RACKMENU`. RK es alias de RACKCAD | Q-07 de la A-14: el menú viejo sigue accesible desde la ventana hasta F9 |
| V-08 | Lo nuevo vive en carpetas propias `VentanaRackCad` (UI, Application y Plugin) | Las guardas de F1 a F4B fijan el contenido exacto de las carpetas `Workspace` y que no tengan ventanas ni diálogos; no se aflojaron |
| V-09 | La paleta empieza en 600 × 600, con un mínimo de 300 × 320. La columna lateral se puede ocultar, y se oculta sola por debajo de 520 de ancho. **No recuerda tamaño ni posición** | Sin AutoCAD no se puede comprobar cómo persistir el estado de la paleta. Queda pendiente (C-10) |
| V-10 | La lista se lee sola **una vez por dibujo**, en el primer drenado. Después, cuando el dibujo cambia, se queda con lo que tenía y dice «La lista puede no estar al día»; se vuelve a leer con «Volver a leer la lista» | Cada lectura de la paleta bloquea el documento y deja un paso vacío en Deshacer (O-18). Releer en cada comando dejaría uno por comando |
| V-11 | Un rack abierto (con pestaña) lleva «Cerrar» en la lista; cerrar sigue las reglas de las pestañas (un borrador protegido pregunta antes, en la página del rack) | Con la tira oculta, era la única forma de descartar el borrador de un rack con confirmación |
| V-12 | Tras lanzar una orden, el foco vuelve al dibujo | La paleta retiene el foco (`KeepFocus`), y la orden y su jig son del dibujo |
| V-13 | La ventana dueña de V-06 es la **única** ventana WPF corriente creada en código, y queda fuera del censo de ADR-0029, que cuenta tipos derivados de `Window`. Una guarda nueva falla si otro archivo de `src` crea una `Window` corriente, o si otro archivo que no sea la puerta de la ventana usa `SendStringToExecute` | ADR-0029 está cerrado y no se toca desde una rama. Esta excepción tiene que pasar a la A-14, o a una decisión sobre el ADR, en la planeación. Mientras tanto, la guarda impide que otra ventana se salga del censo por el mismo camino |
| V-14 | RACKAYUDA no cambia. RACKMENU ya se nombra en la fila de RACKCAD. RACKNUEVO y RACKBIBLIOTECA son puertas de la ventana, como RACKPANEL, que tampoco sale en la ayuda | El censo de la ayuda (I-54 G7) exige un alias registrado por entrada. Añadir alias nuevos es una decisión de nombres para la planeación |
| V-15 | No hay «Fijar» en la ventana. Un borrador protegido fija su rack solo, como antes, y todo rack abierto se ve en la lista con su «Cerrar» | Con la tira oculta, fijar a mano no tenía uso visible; nada se acumula sin verse |
| V-16 | La columna recuerda la última lista leída de cada dibujo (hasta 8 dibujos). Al volver a un dibujo la muestra con el aviso «puede no estar al día», en lugar de vaciarse | Es la regresión I-01 de la segunda revisión: con V-10, volver a un dibujo dejaba la columna vacía hasta releer |

## Lo que queda pendiente o no se pudo comprobar

- **Todo lo de AutoCAD.** Hoy no hubo AutoCAD por decisión del Owner. Falta un smoke corto:
  - RACKCAD, RK y RACKPANEL abren la ventana;
  - cada sección lanza su orden y el editor sale encima de AutoCAD, modal;
  - un rack nuevo insertado aparece en la lista tras el drenado;
  - la lista con 70 racks;
  - el buscador, los filtros y «Seguir la selección»;
  - que la ventana no bloquea AutoCAD;
  - RACKMENU es el menú de antes.
- **Ventana invisible como dueña (V-06):** una prueba de UI comprueba que lanza el trabajo, que una ventana hija modal se abre con ella como dueña y que se cierra. Que quede modal y centrada sobre AutoCAD no se vio.
- **El tamaño y la posición recordados (C-10, V-09).**
- **Deshacer:** cada lectura de la paleta deja todavía un grupo vacío (O-18; es la parte (b) de SPB-33, en pausa). La lista añade solo su primera lectura por dibujo y las que pida el usuario (V-10).
- **SPB-33 (a):** la lista usa los hechos con referencia en espacio modelo, así que un rack sin vistas insertadas no aparece en la lista. La relectura del borrador sigue con el defecto, que está en pausa.
- **«Actualizar todo»** (fase 4) y el editor de Push Back sin modal (fase 3): fuera de este alcance.

## Las pruebas

- **Core, nuevas:** `RackCadWindowListTests` (38 casos): la proyección de la lista, su memoria y su invalidación, `Open`, la tabla de acciones y palabras clave, y las guardas de capas, de la ventana corriente y de `SendStringToExecute` (V-13).
- **UI, nuevas:** `RackCadWindowTests` (41 casos): la lista (grupos, orden, buscador, filtros, estados, lectura ocupada o fallida, la vuelta a un dibujo), abrir, seguir la selección, el texto pendiente (abortado, conservado y con el puerto que falla), «Cerrar» con y sin borrador protegido, las secciones y las puertas, la ventana dueña, la configuración, y la vista (el panel alojado sin tira, los clics, la columna oculta).
- **Comprobación en rojo:**
  - se rompieron a propósito la invalidación de la lista y «Seguir la selección»; sus pruebas fallaron y el código se restauró;
  - se quitó a propósito el arranque de la ventana dueña: sus dos pruebas fallaron a los 15 s con «the modal host did not run its work and close by itself» en lugar de colgar la corrida, y el archivo se restauró.
- **El orden de `ReadRackList` en el host** se fija por el texto del código: su comportamiento necesita AutoCAD.
- **Guardas de F1 a F4B ajustadas,** cada una con su nota «I-64 A-14»:
  - la forma pública del navegador (36);
  - el puerto del resumen, que gana `ReadRackList`;
  - la API de `BrowserSession` (`Open`);
  - los miembros de `BrowserRackSummaries`;
  - la ventana del drenado (3 lecturas);
  - la paleta: la raíz es la ventana, la etiqueta «RackCad» y el tamaño 600;
  - los usings de la paleta;
  - el despacho del menú en `DispatchInsertion`;
  - el censo de comandos (43, con RACKMENU, RACKNUEVO y RACKBIBLIOTECA);
  - la prueba de UI del puerto del resumen.

## La revisión independiente

- **16aebbee: APTO TRAS CORRECCIONES,** con 3 B, 4 I y 11 M. Las correcciones van en el commit siguiente:
  - **B-01:** la ventana dueña no arrancaba sin contenido;
  - **B-02:** la lista se releía en cada comando y dejaba un paso vacío en Deshacer por comando (V-10);
  - **B-03:** el censo de ventanas no admite un tipo nuevo de ventana; ahora es una ventana corriente (V-06);
  - **I-01:** «Cerrar» en la lista (V-11);
  - **I-02:** el foco vuelve al dibujo y los mensajes dicen «Se pidió a AutoCAD…» (V-12);
  - **I-03:** pruebas nuevas del texto pendiente, de la ventana dueña y del orden de ReadRackList;
  - **I-04:** se actualizó la documentación de BrowserRackSummaries y se añadió una guarda de capas para las carpetas VentanaRackCad;
  - **M:** M-01 (la columna se oculta sola), M-03, M-04, M-06, M-07 y M-11 corregidos. M-05, M-08 y M-10 quedaron a medias, como señaló la segunda revisión.
- **Quedan sin tocar:** M-02 (el diálogo de «Examinar…» toma la ventana activa) y M-09 (las abreviaturas de RACKNUEVO a mano).
- **fa00d092 (segunda revisión): APTO TRAS CORRECCIONES,** con 0 B, 2 I y 8 M. Las correcciones van en el commit siguiente:
  - **I-01:** la columna se vaciaba al volver a un dibujo; ahora recuerda la última lista de cada dibujo (V-16);
  - **I-02:** la excepción al censo de ADR-0029 queda registrada (V-13) y una guarda limita la `Window` corriente a la ventana dueña;
  - **M-01:** las pruebas de la ventana dueña tienen un temporizador de seguridad, con su comprobación en rojo;
  - **M-03:** prueba de «Cerrar» con un borrador protegido (pregunta y «Cancelar» lo conserva); «Fijar», decidido en V-15;
  - **M-04:** pruebas de `AllowOpen` con el texto conservado y con el puerto que falla, y del aviso cuando no se sigue la selección;
  - **M-05:** la guarda de `SendStringToExecute` (V-13);
  - **M-06:** los avisos de la selección y del cambio de rack ya no hablan de pestañas;
  - **M-07:** decidido en V-14;
  - **M-08:** este documento.
- **M-02 de la segunda revisión, sin cambio:** la lista dice «puede no estar al día» después de cualquier orden, incluso un `ZOOM`, porque todo `CommandEnded` cuenta como cambio. Es el coste de V-10. Afinarlo exige saber qué órdenes cambian el dibujo, y eso toca el modelo de sesión de F1 a F4B.
- **El CI de `ff0da9b1` falló** en la prueba de la ventana dueña, por el temporizador de M-01 y no por un cuelgue. Lo que pasaba, según la traza de la tercera revisión (M-21):
  - todas las pruebas de UI corren en un solo hilo STA, al que `StaTestRunner` entrega cada prueba con la prioridad más alta;
  - las colecciones de xUnit corren en paralelo, así que el bucle modal de `ShowDialog` de esta prueba ejecutaba **dentro de sí** las pruebas de otras clases (las del editor clásico de Dinámico) durante más de 20 s;
  - con prioridad Background, el temporizador de seguridad (también en Background, encolado antes) corría primero y cerraba la ventana. Mi primer diagnóstico («layout o render de otras superficies») no era el correcto;
  - **`96b9e727`** publica el trabajo con prioridad Normal, que es correcta también en AutoCAD, pero la prueba solo pasaba porque Normal adelantaba al temporizador: tardaba de 18 a 37 s;
  - **la corrección de fondo** es que las pruebas de la ventana dueña corran solas, en una colección sin paralelismo (`RackModalHostWindowTests`), con el temporizador en prioridad Normal;
  - que la ventana dueña se comporta igual dentro de AutoCAD queda para el smoke.
- **Tercera revisión (`ff0da9b1` y `96b9e727`): APTO,** con un M cada uno:
  - **M-20:** la guarda de la `Window` corriente ve ahora también `new()` con tipo destino y `typeof(Window)`, y tiene pruebas de su patrón;
  - **M-21:** la colección sin paralelismo y este diagnóstico. Con la misma combinación que fallaba en el CI, las dos pruebas de la ventana dueña tardan ahora menos de 1 s, frente a los 18 a 37 s de antes, en dos corridas.

## Dinámico en la ventana (línea D, primera entrega)

**Rama:** `architecture/workspace-persistente-rackcad-dinamico`, encima de la de la ventana.

**Lo que hace:**
- Un rack Dinámico de la lista muestra «Editar aquí». La ventana pide a AutoCAD que lea su diseño con `RACKDINAMICOABRIR`, una orden sin marca de Deshacer y sin historial, y abre el editor nuevo en el centro. Que no deje nada en Deshacer está por comprobar en AutoCAD (M-15).
- **El editor sigue la «versión despejada» aprobada,** con las pestañas Rack, Matriz (la de inicio) y Avanzado:
  - **Rack:** «Lo esencial» de todo el rack, con «Varía» cuando hay valores distintos; «Calculado», y «Poste y anotaciones» plegado.
  - **Matriz:** frente × nivel, con un solo dato por celda (la tarima, ancho × alto). Clic, Ctrl+clic, Mayús+clic desde el ancla, F1…, N1… y «Todo». El inspector de la selección lleva la tarima y los largueros de las celdas y los datos de sus frentes, y permite dejar un frente en blanco o activarlo.
  - **Avanzado:** altura manual, separadores, poste derivado y los módulos con rótulos en español (M1…, Cabecera o Separador, Sí o No).
- **Una sola forma de confirmar:** lo escrito va al borrador al salir del campo o con Enter. Salir de un campo sin cambiar su texto no cambia nada. **Un solo alcance:** la selección. Un valor que no se puede recalcular se rechaza en su campo, y el borrador, la matriz y la selección quedan como estaban.
- **«Modificado»** significa que el borrador difiere de lo último que se leyó del dibujo o se escribió en él. No depende de los gestos.
- **Un frente en blanco** conserva sus niveles y celdas, como en el editor clásico (I-33). «Lo esencial» no los toca, y el inspector los muestra en solo lectura con el motivo.
- **Los peraltes de larguero** solo admiten los valores del catálogo para el larguero de cada celda, como las listas del clásico.
- **«Restaurar layout…»** pide confirmación y dice qué se pierde. **«Cerrar el editor»** pregunta antes si hay cambios.
- **El borrador sobrevive al cambio de rack,** y la lista lo marca: «Modificado (editor de la ventana)», o «Escribiendo en el dibujo».
- **«Actualizar»** entrega a `RACKDINAMICOAPLICAR` una copia del borrador, que redibuja el rack con la mitad de dibujo de RACKEDITAR (`ApplyDynamicEdit`, extraída de `EditDynamic` sin cambiar su cuerpo). Solo actualiza, sin vista nueva, y es una orden: un «U» la deshace.

**La puerta (corregida tras la segunda revisión):**
- Cada petición lleva un número. La respuesta vale solo para esa petición, y se aplica a la sesión y al rack que había **al pedirla**.
- Solo una petición espera a la vez. Mientras AutoCAD escribe, el editor no se puede editar.
- «Dejar de esperar» olvida la petición: si su orden llega a correr después, no hace nada.
- La petición se guarda con el dibujo activo. Si la orden corre en otro dibujo, no hace nada y lo dice.
- `RACKDINAMICOABRIR` ya no pregunta el rack en la línea de órdenes: toma la petición guardada. Escrita a mano, sin petición, solo avisa.
- Al entregar la orden, el foco vuelve al dibujo.
- El nombre se envía recortado, como lo hace el clásico.

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| D-01 | El borrador (`DynamicDraftEditor`, en Application) es la misma recomposición que el editor clásico. Con valores de hasta dos decimales, el diseño que guarda es idéntico byte a byte, en un rack nuevo, en uno cargado y en uno editado. Las diferencias están en D-04 | El editor nuevo escribe lo que escribiría el clásico. Las reglas nuevas del Owner (la altura por poste, la cama editable, el largo IN/OUT por frente) son cambios de cálculo declarados y van en una entrega aparte, con sus pruebas |
| D-02 | La lectura y la escritura son órdenes de AutoCAD, no lecturas de la paleta | No bloquean el documento desde la paleta (O-18) y corren en el documento activo, así que no se puede leer el rack de otro dibujo |
| D-03 | `RACKEDITAR` sigue con el editor clásico, que también es «Abrir en editor clásico» | Hasta que el Owner valide el editor nuevo en AutoCAD |
| D-04 | Tres diferencias declaradas con el clásico (I-03): (1) el borrador conserva la precisión completa de un valor, y el clásico lo redondea a dos decimales cuando relee sus casillas (1200 mm de fondo: 47.2441 contra 47.24); (2) el estilo de cota se conserva tal como se leyó, y el clásico descarta un estilo que el dibujo no tiene (en ambos casos el dibujo acota con su estilo actual y los tamaños de RackCad); (3) un poste que no está en el catálogo da paso al poste por defecto, como en el clásico, pero el editor lo dice, y un poste del catálogo escrito con otras mayúsculas es ese poste | El borrador nunca cambia un valor que el usuario no tocó. Las tres tienen prueba |
| D-05 | El sistema publicado se recompone siempre sobre una copia: el sistema es la resolución del último diseño válido. «Actualizar» envía el diseño copiado por su JSON y el sistema resuelto otra vez desde esa copia | Nada de lo que se edite después alcanza lo que escribe AutoCAD (B-02), y un recálculo que falla deja el último sistema válido (M-14) |
| D-06 | Los elementos del editor (campos, celdas y cabeceras de la matriz) viven lo que el editor y se refrescan en su sitio | Confirmar un campo ya no destruye la casilla con el foco ni el botón bajo el ratón (I-05) |
| D-07 | **El cierre del dibujo no espera al borrador Dinámico.** Si el dibujo se cierra con cambios sin escribir, la ventana olvida el borrador y avisa de que esos cambios se perdieron. Los borradores de dibujos cerrados se podan siempre | Vetar el cierre exige una segunda autoridad de cierre: suscribirse a `BeginDocumentClose` fuera del puente (prohibido por INV-F1-T2-02) o envolver al consejero del host (F4-T3C2 fija una sola autoridad). **Es una decisión para la planeación:** llevar los borradores de la ventana al host, o admitir un segundo consejero |

**Fuera de esta entrega:**
- la vista previa;
- la seguridad y los extremos;
- el editor de cabeceras (va a otra iniciativa);
- «Insertar vista…»;
- la detección de un cambio del dibujo mientras se edita (el conflicto). Tras un «U» de `RACKDINAMICOAPLICAR`, o un RACKEDITAR del mismo rack, el editor sigue «Limpio» con lo que escribió; «Descartar» dice «lo último que se leyó o se escribió», no «lo que hay en el dibujo» (M-16);
- un Dinámico nuevo desde la ventana, que sigue saliendo por «Nuevo» (RACKNUEVO), con el editor clásico;
- las tres reglas nuevas de cálculo.

**Las pruebas:**
- `DynamicDraftEditorEquivalenceTests` (9): la equivalencia con el clásico, las tres diferencias declaradas (D-04), la copia para AutoCAD y el recálculo fallido.
- `RackCadDynamicEditorTests` (33):
  - el editor: confirmar sin cambios, «Varía» vacío, la vuelta atrás de un recálculo que falla (con la matriz y la selección), el frente en blanco, los peraltes del catálogo, el ancla de Mayús, los elementos conservados, «Descartar» y «Cerrar el editor»;
  - la puerta, con un doble: los números de petición, «Dejar de esperar», la respuesta tardía, la sesión al pedir, la escritura con copias y el editor bloqueado, dos dibujos con el mismo rack, AutoCAD ocupado y la poda de un dibujo cerrado;
  - las vistas: el editor en la página del rack, el aviso de espera y su botón, y los botones de celda que sobreviven a una confirmación.
- **Comprobación en rojo:**
  - con la instantánea apuntando al diseño y al sistema vivos, fallaron sus dos pruebas;
  - sin la regla de «sin cambios» en el campo, falló la suya;
  - los dos archivos se restauraron.
- El censo de comandos pasa a 45, con RACKDINAMICOABRIR y RACKDINAMICOAPLICAR, que ahora viven en `RackCadRackDoorCommands.cs`.
- **Las guardas:**
  - solo las dos puertas de la ventana entregan órdenes a AutoCAD;
  - los archivos que llama la paleta no usan los ayudantes que bloquean el documento; solo los usan las órdenes de la puerta (M-05).

**La revisión independiente de `345c4c41`:** APTO TRAS CORRECCIONES, con 2 B, 5 I y 11 M.
- **B-01:** `Update()` pasó a `RequestWrite()`.
- **B-02 e I-07:** la puerta corregida (arriba) y D-05.
- **I-03:** D-04.
- **I-04:** salir de un campo sin cambiarlo no hace nada, y «Modificado» se mide contra el dibujo.
- **I-05:** D-06.
- **I-06:** «Cerrar el editor», la poda y el aviso de pérdida. El veto queda como decisión, D-07.
- **M-09 a M-14, M-17 y M-18:** corregidos, con prueba donde aplica.
- **M-10:** el nombre va recortado.
- **M-11:** los peraltes del catálogo.
- **M-15 y M-16:** declarados. M-15 es un paso del smoke; M-16 está en «Fuera de esta entrega».
- **M-19:** este documento.

**Sin AutoCAD:** la lectura, la escritura, el editor en la paleta y que `RACKDINAMICOABRIR` no deje nada en Deshacer esperan el smoke del Owner.
