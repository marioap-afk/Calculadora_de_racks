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
- **«Actualizar»** entrega a `RACKDINAMICOAPLICAR` una copia del borrador, que redibuja el rack con la mitad de dibujo de RACKEDITAR (`ApplyDynamicEdit`, extraída de `EditDynamic`; ahora además devuelve cuántas vistas redibujó y cuántas borró, y RACKEDITAR ignora ese valor). Solo actualiza, sin vista nueva, y es una orden: un «U» la deshace. El borrador queda «escrito» solo si AutoCAD redibujó al menos una vista; si no redibujó ninguna, sigue sin escribir y el mensaje lo dice (I-08). Una escritura en la que una vista se redibuja y otra falla cuenta como «escrito», como en RACKEDITAR: queda declarado (M-28).

**La puerta (corregida tras la segunda revisión):**
- Cada petición lleva un número. La respuesta vale solo para esa petición, y se aplica a la sesión y al rack que había **al pedirla**.
- Solo una petición espera a la vez. Mientras AutoCAD escribe, el editor no se puede editar.
- «Dejar de esperar» olvida la petición: si su orden llega a correr después, no hace nada.
- La petición se guarda con el dibujo activo. Si la orden corre en otro dibujo, no hace nada y lo dice.
- `RACKDINAMICOABRIR` ya no pregunta el rack en la línea de órdenes: toma la petición guardada. Escrita a mano, sin petición, solo avisa.
- Al entregar la orden, el foco vuelve al dibujo.
- El nombre se envía recortado, como lo hace el clásico.
- **Tras la tercera revisión:**
  - la petición queda en espera **antes** de entregarse, así que una respuesta que llega durante la llamada vale (M-22);
  - cada petición lleva la sesión para la que se hizo. M-24 queda **declarado**: la puerta no la compara con la sesión activa, porque las guardas de F1 impiden que un archivo del Plugin fuera de `Plugin/Workspace` llegue al controlador, y el anfitrión de la paleta no lee la instantánea. La sesión presentada sigue al dibujo activo por el aviso del host, que corre antes de cualquier clic en la paleta, y la orden ya no actúa en otro dibujo que el de la petición;
  - una petición en espera para un dibujo que se cerró se olvida y se avisa a la puerta, también sin borradores (M-23).

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| D-01 | El borrador (`DynamicDraftEditor`, en Application) es la misma recomposición que el editor clásico. Con valores de hasta dos decimales, el diseño que guarda es idéntico byte a byte, en un rack nuevo, en uno cargado y en uno editado. Las diferencias están en D-04 | El editor nuevo escribe lo que escribiría el clásico. Las reglas nuevas del Owner (la altura por poste, la cama editable, el largo IN/OUT por frente) son cambios de cálculo declarados y van en una entrega aparte, con sus pruebas |
| D-02 | La lectura y la escritura son órdenes de AutoCAD, no lecturas de la paleta | No bloquean el documento desde la paleta (O-18) y corren en el documento activo, así que no se puede leer el rack de otro dibujo |
| D-03 | `RACKEDITAR` sigue con el editor clásico, que también es «Abrir en editor clásico» | Hasta que el Owner valide el editor nuevo en AutoCAD |
| D-04 | Tres diferencias declaradas con el clásico (I-03): (1) el borrador conserva la precisión completa de un valor, y el clásico lo redondea a dos decimales cuando relee sus casillas (1200 mm de fondo: 47.2441 contra 47.24); (2) el estilo de cota se conserva tal como se leyó, y el clásico descarta un estilo que el dibujo no tiene (en ambos casos el dibujo acota con su estilo actual y los tamaños de RackCad); (3) un poste que no está en el catálogo da paso al poste por defecto, como en el clásico, pero el editor lo dice, y un poste del catálogo escrito con otras mayúsculas es ese poste | El borrador nunca cambia un valor que el usuario no tocó. Las tres tienen prueba |
| D-05 | El sistema publicado se recompone siempre sobre una copia: el sistema es la resolución del último diseño válido. «Actualizar» envía el diseño copiado por su JSON y el sistema resuelto otra vez desde esa copia | Nada de lo que se edite después alcanza lo que escribe AutoCAD (B-02). Un recálculo que falla deja el último sistema válido (M-14), y la vuelta atrás borra su error, así que un valor rechazado no apaga «Actualizar» (I-09) |
| D-06 | Los elementos del editor (campos, celdas y cabeceras de la matriz) viven lo que el editor y se refrescan en su sitio | Confirmar un campo ya no destruye la casilla con el foco ni el botón bajo el ratón (I-05) |
| D-07 | **Sustituida por D-08** (2026-11-24). Decía que el cierre del dibujo no esperaba al borrador Dinámico, porque hacía falta una segunda autoridad de cierre | El Owner autorizó cambiar la regla de cierre |
| D-08 | **Un dibujo con un Dinámico sin escribir no se cierra** (decisión 11 del prototipo: «Que avise antes de cerrar»). El consejero de cierre instalado es ahora el de la ventana (`RackCadWindowCloseAdvisor`), que envuelve al de siempre (el relevo sobre el host): el host decide por sus borradores, como antes, y la ventana veta si tiene cambios sin escribir o escribiéndose, con un aviso en la línea de órdenes que nombra los racks. Un fallo es veto, como en el relevo. Al salir de AutoCAD vale lo mismo, dibujo por dibujo | El Owner lo autorizó el 2026-11-24: «te autorizo cambiar la regla de cierre». Esa regla está escrita en `WorkspaceLifecycleGuardTests`, un original de la lista cerrada de PB-T4b: su guarda del controlador y la base de ese archivo en las tres guardas de la lista (`PushBackRetirePilotFullCopyGuardTests`, `PushBackEditAllFullCopyGuardTests`, `WorkspaceFullCopyGuardTests`) se fijan de nuevo, con nota. Los paquetes de PB-T4 en `evidence` no se tocan |

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
- `RackCadDynamicEditorTests` (37):
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
- **I-06:** «Cerrar el editor», la poda y el aviso de pérdida. El veto queda como decisión, D-07. Después, el veto se hizo (D-08).
- **M-09 a M-14, M-17 y M-18:** corregidos, con prueba donde aplica.
- **M-10:** el nombre va recortado.
- **M-11:** los peraltes del catálogo.
- **M-15 y M-16:** declarados. M-15 es un paso del smoke; M-16 está en «Fuera de esta entrega».
- **M-19:** este documento.

**La tercera revisión de `d6e59d6b`:** APTO TRAS CORRECCIONES, con 0 B, 2 I y 5 M. Todo va en el commit siguiente, con prueba:
- **I-08:** «escrito» solo si se redibujó alguna vista. La regla es pura (`RackDoorRules.AnswerForApply`), con prueba en Core, y una guarda comprueba que la orden contesta con ella.
- **I-09:** la vuelta atrás de un valor rechazado, o de un «Restaurar layout» que falla, borra el error del recálculo.
- **M-22 y M-23:** la puerta, arriba. **M-24:** declarado, arriba.
- **M-25:** pruebas nuevas:
  - la respuesta durante la llamada;
  - la sesión en cada petición;
  - la lectura y la escritura en espera de un dibujo cerrado;
  - el valor rechazado seguido de «Actualizar».
  El foco real de I-05 sigue sin prueba: las vistas se prueban con clics simulados, y eso queda para el smoke.
- **M-26:** este documento y los comentarios.
- **Comprobación en rojo:** sin borrar el error en la vuelta atrás falló la prueba de I-09, y registrando la petición después de entregarla falló la de M-22. Los archivos se restauraron.

**La cuarta revisión, solo del commit de la tercera (`38be6ac3`): APTO,** con 0 B, 0 I y 4 M. Lo que se hizo con cada M:
- **M-27:** a la prueba de la lectura de un dibujo cerrado le faltaba `[Fact]`, así que no corría. Ya lo tiene.
- **M-28:** declarado, arriba.
- **M-29:** los textos de la respuesta ya no prometen lo que la línea de órdenes no da.
- **M-30:** el comentario de la guarda de `Window` volvió junto a su patrón. Que la guarda de I-08 solo compruebe el texto queda así: la orden es código de AutoCAD sin pruebas aquí.

**Sin AutoCAD:** la lectura, la escritura, el editor en la paleta y que `RACKDINAMICOABRIR` no deje nada en Deshacer esperan el smoke del Owner.

## Dinámico, segunda entrega (2026-11-24, en curso)

El Owner pidió seguir con Dinámico antes de Cantilever. Las piezas pendientes de sus decisiones del prototipo van en este orden:
1. el aviso antes de cerrar (D-08, hecho);
2. el largo IN/OUT por frente y la chincheta de la lista (hecho, con las observaciones del smoke);
3. los cambios de cálculo (la altura por poste, la cama editable; hecho);
4. la seguridad, «Insertar vista…» y un Dinámico nuevo desde la ventana (hecho).

**Lo hecho en el paso 2:**
- **El largo IN/OUT es del frente** (decisión 3: «Sí, es por frente»).
  - El campo pasa del inspector de la celda a «Frentes de la selección» y escribe todos los niveles de los frentes activos.
  - El dibujo lo sigue guardando por nivel, como el clásico, así que un frente con niveles distintos dice «Varía».
  - Un «Varía» vacío no hace nada (I-04): para volver un frente entero a «calculado» se escribe un valor y luego se borra.
- **La chincheta** («Fijar la lista», en la cabecera): fijada, la lista se queda; sin fijar, se oculta sola al mostrar un rack o una sección, y «Mostrar la lista» la devuelve.
- **Lo que se corrigió del smoke:**
  - O-1: la cabecera sigue cada lectura de la lista;
  - O-2: una selección nueva del rack ya activo lleva a su página, dentro del mismo dibujo;
  - O-3: por debajo de 700 px el inspector va debajo de la matriz.
- **Sin cambio:** O-4 (los monitores) depende de AutoCAD y de V-09 (la paleta no recuerda su posición).

**Lo hecho en el paso 3** (respuestas del Owner: el paso actual se mantiene, la cama va por frente, la pendiente es la subida por pie de cada frente y el ancho de la cama sigue a la tarima):
- **La altura por poste.** El dibujo y el BOM ya daban a cada poste la altura del frente más alto que toca. Ahora el editor también la muestra en «Calculado»:
  - la altura de cabecera (la más alta);
  - la altura por poste (P1 · P2 …);
  - la subida de la cama de cada frente.

  Una prueba fija que el dibujo y el BOM usan las dos alturas de un rack con frentes de 5 y 4 niveles, y que la altura manual de cabecera manda en todos los postes.
- **La cama por frente**, en «Avanzado» → «Cama y pendiente». Escribe los frentes activos de la selección de «Matriz»:
  - el rodillo: 1.9 o 2.5 in del catálogo; vacío = 1.9;
  - el paso: vacío = el del rodillo (2 in con 1.9, 3 in con 2.5). Un paso menor que el rodillo se rechaza, y no se cambia a un rodillo mayor que el paso escrito;
  - un freno cada N rodillos: vacío = por el fondo de tarima, como siempre;
  - la pendiente en in por pie: vacío = 7/16.

  Dos frentes con camas distintas dicen «Varía».
- **El dibujo** guarda los cuatro valores en el frente solo cuando tienen valor. Un rack sin cambios de cama se escribe igual que antes (prueba).
- **El BOM** separa las líneas de «Cama» por la cama del frente, por ejemplo « · rodillo 2.5" · paso 4" · freno cada 6 rodillos». Una cama como antes conserva su línea.
- **El editor clásico** no muestra estos valores, pero los conserva al abrir y escribir un rack que los tiene (prueba).
- **Sin cambio:** el ancho de la cama sigue a la tarima.
- **Core Full de `c6676989`** dio 2 fallos: los dos censos de campos del estado no clasificaban los cuatro campos nuevos del frente, y el comparador del estado editable de Push Back, que comparte la clase del frente, no los comparaba. Se corrigió en `db0edb48`: Core Full 19073/0/403 y Release limpio.

**Lo hecho en el paso 4** (decisiones 6 y 10 del prototipo):
- **La pestaña «Extremos y seguridad»**, entre «Matriz» y «Avanzado» (decisión 6: «Seguridad por poste en sección propia; guía de entrada marcada en la matriz»).
  - **La guía de entrada:**
    - las celdas con guía llevan una «G» en la matriz, y el lector de pantalla lo dice;
    - las marcas salen del mismo plan que usan todas las vistas y el BOM: dos piezas por celda;
    - se pone o se quita con «En todas las celdas» y «En ninguna», o celda a celda con la casilla «Guía de entrada» del inspector, que dice «Varía» (indeterminada) cuando la selección mezcla;
    - un frente en blanco conserva sus celdas dormidas, como en el clásico.
  - **«Seguridad por poste»:** una tabla de solo lectura con lo que el dibujo coloca en cada poste: botas, protector lateral, desviadores en salida y en entrada, defensas y guías.
    - Se lee de las mismas proyecciones que el BOM: la planta para botas, protectores y defensas, y los dos cortes frontales para desviadores y guías.
    - Una prueba fija que la suma de la tabla es la del BOM, familia por familia.
  - **«Editar seguridad…»** abre el diálogo de seguridad del editor clásico, con sus mismos argumentos, sobre copias de la seguridad del borrador. Con «Aceptar», el borrador toma el resultado y se recalcula; con «Cancelar», no cambia nada.
    - El diálogo se muestra como modal de AutoCAD, por un puerto del host (`RackCadModalPort`, con `ShowModalWindow`), igual que los editores clásicos.
    - Sin ese puerto, la ventana no lo ofrece y dice por qué.
- **D-09 (decisión de la sesión, para revisar en la planeación).** La tabla del prototipo era editable: un selector por poste para botas, protector, desviadores y defensas. Aquí es de solo lectura, y se edita en el diálogo clásico.
  - **Motivo:** el propio prototipo dejaba [PENDIENTE] el reparto por defecto en cada poste y la longitud de las defensas.
  - **Motivo:** el modelo de seguridad tiene reglas de precedencia por poste y por lado (I-42) que el diálogo ya aplica y prueba.
  - **Motivo:** el vocabulario de lados del diálogo («Izquierda es la salida y derecha la entrada») no es el del prototipo («salida y entrada»).
  - Llevar la edición a la tabla es una iniciativa propia, con las respuestas del Owner a esos tres puntos.
- **«Insertar vista…»**, en la cabecera del editor (decisión 10: «primero escribe el borrador en las vistas existentes»).
  - Elige las vistas con el diálogo de «Insertar varias…» del clásico: frontal de salida, frontal de entrada, un lateral por cada poste con corte, y planta.
  - La orden nueva `RACKDINAMICOINSERTAR` hace la mitad de dibujo de RACKEDITAR con inserción, como «Insertar» del clásico sobre un rack del dibujo:
    1. redibuja todas las vistas del rack con el borrador;
    2. coloca las nuevas con el jig, como copias enlazadas (la misma identidad).
  - La respuesta cuenta las vistas del rack antes y después:
    - «escrito» quiere decir que se redibujaron las vistas existentes;
    - la respuesta dice cuántas vistas nuevas se colocaron, o que no se colocó ninguna (la colocación se canceló o no se pudo preparar);
    - si no se redibujó ninguna vista existente, el borrador sigue sin escribir, aunque se haya insertado alguna.
  - «Actualizar» sigue siendo solo actualizar.
- **Un Dinámico nuevo desde la ventana.**
  - En «Nuevo» hay un botón «Dinámico en esta ventana», encima de las tarjetas de siempre, que siguen abriendo el editor clásico (RACKNUEVO).
  - Abre en la propia página «Nuevo» el editor de la ventana con los valores por defecto del clásico.
  - La identidad del rack se da al crearlo. «Actualizar» no se ofrece hasta que el rack está en el dibujo, y lo dice.
  - El botón de la cabecera se llama «Insertar en el dibujo…» y elige las vistas con el mismo diálogo.
  - La orden nueva `RACKDINAMICONUEVO` inserta el rack con el mismo despacho que el editor clásico abierto desde «Nuevo», con el nombre automático de I-60.
  - La respuesta cuenta las vistas colocadas con esa identidad y da el nombre que quedó en el dibujo.
  - Al insertarlo, el mismo editor pasa a ser el del rack del dibujo: «Actualizar» e «Insertar vista…».
  - Si la colocación se cancela, el borrador sigue nuevo en la ventana.
  - Un Dinámico nuevo con cambios es trabajo sin escribir: el dibujo no se cierra (D-08), y el aviso lo nombra «un Dinámico nuevo».
- **D-10 (decisión de la sesión).** Tras insertar el rack nuevo, la ventana no vuelve a leer la lista ni lo abre sola: la respuesta llega dentro de la orden de AutoCAD, y la lectura de la lista desde la paleta bloquea el documento (O-18). El aviso pide «vuelve a leer la lista y ábrelo». Al abrirlo aparece el mismo editor.
- **Las pruebas:**
  - `DynamicSafetyTabTests` (9): las marcas de la matriz contra el BOM, la casilla del inspector, «En todas» y «En ninguna» con un frente en blanco, la tabla por poste contra el BOM, el diálogo contra el del clásico, el puerto, el bloqueo mientras AutoCAD escribe y el orden de las pestañas.
  - En `RackCadDynamicEditorTests`:
    - la ventana solo ofrece el diálogo de seguridad con el puerto del host;
    - «Insertar vista…» envía el borrador con las vistas elegidas;
    - su respuesta es la de una escritura;
    - sin el puerto no se ofrece;
    - el Dinámico nuevo se hace e inserta como rack nuevo y luego es un rack del dibujo;
    - si no se colocó, sigue nuevo;
    - sin la puerta no se ofrece;
    - la página «Nuevo» aloja su editor.
  - En `RackCadWindowListTests`, las reglas de la respuesta de la inserción y del rack nuevo, y la elección de la orden en el puerto.
  - Los censos de órdenes pasan de 45 a 47.
  - Comprobaciones en rojo:
    - siete piezas rotas a propósito: las celdas con guía, quitar la guía, el eje de la tabla por poste, el diálogo sin guía, el resultado del diálogo ignorado, las vistas de la inserción perdidas y la marca de la matriz;
    - cada una hace fallar su prueba;
    - un control sin cambio de comportamiento pasa;
    - del Dinámico nuevo, tres más: la marca de rack nuevo perdida en la escritura, el rack insertado que no deja de ser nuevo y «Actualizar» ofrecido a un rack nuevo;
    - la última sobrevivió a la primera versión de la prueba, que solo lo comprobaba sin cambios; la prueba se reforzó con un borrador modificado y ahora la detecta.

**Smoke exploratorio de la sesión (2026-11-24, por orden del Owner, sobre `78fd5cb1`):**
- pasan todos los pasos, también M-15 (la lectura no deja nada en Deshacer), I-04, I-05 con el ratón real, I-09, M-11, el «U» de «Actualizar» y D-07, que entonces era solo un aviso de pérdida;
- observaciones:
  - la cabecera no se refresca al «Volver a leer la lista»;
  - seleccionar el rack ya activo no salta a su página;
  - el inspector queda cortado al ancho inicial;
  - con varios monitores, la paleta y las modales clásicas abren en el principal.

**La revisión independiente de la segunda entrega** (quinta revisión, `78fd5cb1..7f6a5970`, de solo lectura): APTO TRAS CORRECCIONES. Las tres importantes se corrigieron:
- **I-01:** en el corte lateral de un poste compartido, dos frentes con el mismo fondo y camas o pendientes distintas dibujaban una sola cama, pero el BOM contaba las dos. La clave de las camas del corte incluye ahora la cama y la pendiente del frente. Un rack como antes agrupa igual (prueba).
- **I-02:** «Dejar de esperar» mientras se colocaban vistas abandonaba una orden que ya corría.
  - Con «Insertar vista…», la ventana quedaba «Modificado» contra el dibujo, y un «Descartar» podía escribir después la base vieja.
  - Con un rack nuevo, el editor quedaba atascado.
  - Corrección: cada orden de escritura avisa a la ventana de que tomó la petición, y desde ahí «Dejar de esperar» se desactiva y explica que hay que terminar o cancelar en AutoCAD; la respuesta siempre llega. Una petición que la orden aún no tomó se puede abandonar como antes (prueba).
  - Además, el editor de un rack nuevo nunca es el de la página de un rack, así que su vista no se aloja en dos sitios.
- **I-03:** la página «Nuevo» no seguía el cambio de dibujo, y el rack nuevo de un dibujo podía insertarse en otro.
  - Corrección: la ventana refresca «Nuevo» al cambiar de dibujo, y un borrador de otro dibujo no se escribe en el activo: lo dice (prueba).

Los menores:
- **Corregidos:**
  - M-01: un frente nuevo hereda la cama y la pendiente de su plantilla.
  - M-02: el `Snapshot` del resolvedor copia los cuatro campos. Las copias de Push Back no los copian; Push Back no los usa.
  - M-03: los textos de cierre, de «Descartar» y de pérdida valen para un rack nuevo.
  - M-04: el rack nuevo toma el nombre que le dio el dibujo (I-60).
  - M-06: el paso debe ser un número entero de pulgadas (los rodillos cuelgan de los troqueles), y el paso del rodillo se guarda vacío, así que la cama y su línea del BOM no cambian. Por eso, con los rodillos de hoy, el rechazo de un rodillo mayor que el paso ya no se alcanza; se conserva como guarda.
  - M-08: el comentario de la puerta.
- **Declarados, sin cambio:**
  - M-05: una inserción que redibuja pero no coloca ninguna vista se muestra como mensaje normal, no como error.
  - M-07: faltan pruebas del reparto por poste en planta (hoy se compara el total) y de la vuelta atrás de la seguridad. Sí se añadió la del corte de I-01.
  - M-08: el diálogo de vistas se abre aunque la puerta esté ocupada con otro editor; la escritura se rechaza después, con su aviso.
  - M-04, relacionado: el editor del rack insertado se reutiliza al abrir el rack sin volver a leerlo. Es la misma limitación de M-16 (sin detección de cambios del dibujo).
- Comprobaciones en rojo de las correcciones:
  - cinco piezas rotas a propósito: la clave del corte sin la cama, el aviso de la orden ignorado, un borrador de otro dibujo escrito, «Nuevo» sin refrescar al cambiar de dibujo y el nombre del rack nuevo sin tomar;
  - cada una hace fallar su prueba.

**Smoke exploratorio de la segunda entrega** (2026-11-24, hecho por la sesión por orden del Owner, sobre `e69c8fb7`, en una copia de trabajo):
- Pasan todos los pasos:
  - la cama y la pendiente: el rechazo del paso, la subida de 24 in, «Actualizar» y la línea de «Cama» del BOM con el rodillo 2.5;
  - la guía: «En ninguna», por celda desde el inspector y su «G»;
  - la tabla por poste;
  - «Editar seguridad…» como modal de AutoCAD;
  - «Insertar vista…» con la planta enlazada («Vistas: lateral, planta»), con «Dejar de esperar» desactivado durante la colocación (I-02);
  - el Dinámico nuevo insertado como «Dinámico 1» y reabierto con el mismo editor y el nombre del dibujo (M-04);
  - el veto al cerrar con un cambio sin escribir, con su aviso en la línea de órdenes (D-08), y «Actualizar» sobre el rack insertado.
- Observaciones:
  - O-5, por investigar: con la paleta acoplada, tres clics de ratón en «Editar aquí» no hicieron nada, y UI Automation sí pulsó el botón. No se sabe si es de la herramienta de control de pantalla o del producto.
  - O-6: en «Calculado», un valor largo se pega a su etiqueta.
  - O-7: la «G» es pequeña y tenue en la celda seleccionada.
  - O-8: al pasar a «Modificado», el contenido sube unos 12 px.
- No se probó el «U» de una inserción.
- **O-6 a O-8 corregidas** (orden del Owner), todo en la vista del editor:
  - O-6: el valor de «Calculado» deja 16 px a su etiqueta, la etiqueta larga parte línea y la lista crece a 520 px;
  - O-7: la «G» es una insignia con borde en el color de «correcto», en negrita y de 10 pt, distinta del color de la selección;
  - O-8: la línea «Sin cambios que escribir.» se oculta sin colapsar, así que el editor no se mueve al pasar a «Modificado».
- Cada corrección tiene su prueba de vista con WPF real, y cada prueba falla si se revierte su corrección.
- O-5 sigue por investigar.
- **O-5 investigada (no se reproduce):** en un AutoCAD nuevo con `ae811e2d`, siete intentos con el ratón abrieron el editor:
  - con la paleta flotante;
  - acoplada;
  - acoplada y ensanchada;
  - tras abrir el rack desde la lista;
  - tras una ventana de selección cancelada;
  - tras arrastres desde el borde de la paleta y desde la fila de «Seguir la selección»;
  - con la secuencia exacta del smoke, desde AutoCAD recién arrancado.

  Lo más probable es que, en aquella sesión, los clics de la herramienta de control de pantalla los consumiera la activación de la paleta acoplada. Se cierra como no reproducible; si el Owner la ve con su ratón, se reabre con el estado de la paleta y lo que se hizo antes. En la misma sesión se vieron en AutoCAD la insignia «G» nueva (O-7) y el editor sin saltos al pasar a «Modificado» (O-8).

## Selectivo en la ventana (línea D), E1-A

**Rama:** `architecture/workspace-persistente-rackcad-selectivo`, desde `dc82a43c` (la punta de Dinámico). E1-A son los datos y la puerta, sin pantalla: la pantalla es E1-B.

**Lo que se construyó:**
- **El borrador** (`SelectiveDraftEditor`, en Application): un Selectivo sin WPF ni AutoCAD.
  - Es la misma recomposición que el editor clásico: el estado de I-20 (una matriz por fondo), las entradas que el clásico lee de sus cajas, el armado de `BuildDesign` del estado y el resolvedor de geometría. No añade geometría.
  - Escribe con las mismas operaciones del estado que usa el clásico (I-43): las celdas por el resolvedor de destinos, los frentes y los fondos por sus `Apply…ToTargets`.
  - **Un solo alcance** (decisión 2): «Todos los fondos», donde empieza, o «Fondo n». Lo que se lee sobre el alcance dice «Varía» si los fondos difieren, y cuenta lo que un fondo no tiene: un frente que solo existe en un fondo se escribe ahí y el otro se indica.
  - **Por celda:** la tarima (frente × alto), las tarimas por nivel, el larguero y su peralte (solo los del catálogo para ese larguero, como la lista del clásico), y el largo y el claro manuales.
  - **Por frente:** los niveles (un número exacto), el larguero a piso y su elevación, la altura manual (`HeightOverride`, decisión 9) y los tramos de medio frente, con las comprobaciones del diálogo clásico.
  - **Por fondo y del rack:** el número de fondos, los frentes, la profundidad de tarima y de cabecera, las separaciones, el poste y su peralte, las anotaciones (con «Mostrar tarimas») y las cotas.
  - **Por poste:** el peralte propio, con la regla de la caja clásica: vacío o igual al del rack es «toma el del rack». Las cabeceras personalizadas se conservan y se leen; se editan en el editor clásico.
  - **Las holguras** (decisión 12): la ventana no ofrece «=Variable». Una holgura vinculada abre con su valor efectivo y es de solo lectura (`IsLinked`, con el motivo y el nombre de la variable). Al escribir, su estado final es el que se leyó, así que el vínculo y el literal congelado se conservan, como en el clásico cuando no se toca. `LinkedPropertyEditor` y lo de I-47/I-48 no se tocan.
  - `Recompose()` con `LastError` conserva el último diseño y sistema válidos. Son copias por el JSON del documento, nunca objetos del estado. `RestoreLastValid()` devuelve las entradas al último válido y borra el error (I-09). `SnapshotForWrite()` da copias y los estados finales de las holguras.
- **La puerta** (`RackCadRackDoor.cs`, aditiva):
  - `IRackCadSelectiveDoorPort` extiende la puerta de Dinámico con las mismas reglas: numeración, otro dibujo, «Dejar de esperar» y `RequestStarted`. La de Dinámico, sus dobles y sus pruebas no cambian.
  - `SelectiveDesignRead` lleva el diseño efectivo (JSON), los estados de las holguras, los nombres de las variables (solo para mostrarlos), el tipo, el nombre y, si no abre, el motivo.
  - `SelectiveDoorWrite` lleva las copias del borrador y los estados finales.
  - Las reglas, puras y con prueba en Core: `RackDoorRules.AnswerForSelectiveOpen` y `PlanSelectiveApply`.
- **Las órdenes** (`RackCadRackDoorCommands.cs`):
  - `RACKSELECTIVOABRIR`, sin marca de Deshacer ni historial, lee lo que lee RACKEDITAR antes de abrir: el diseño, el registro de variables y `SelectiveEditorOpen.Resolve`. Una referencia rota bloquea la apertura igual que en RACKEDITAR.
  - `RACKSELECTIVOAPLICAR` relee el rack y el registro, planea la escritura y redibuja con la mitad de dibujo de «Actualizar». Contesta con `RackDoorRules.AnswerForApply`. Un «U» la deshace.
- **La mitad de dibujo:** `ApplySelectiveEdit`, extraída de `EditSelective` con el mismo cuerpo. RACKEDITAR la usa igual e ignora lo que devuelve (cuántas vistas redibujó y cuántas borró). La recibe un `ISelectiveEditOutcome` (UI): la ventana clásica lo es, y la puerta pasa un `SelectiveDraftOutcome`.

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| S-01 | El borrador es la misma recomposición que el clásico: con valores de hasta tres decimales, el diseño que guarda es idéntico byte a byte (rack nuevo, abierto, editado, dos fondos, medio frente, cabeceras personalizadas, peraltes por poste, holgura vinculada). Las diferencias están en S-02 | El editor nuevo escribe lo que escribiría el clásico |
| S-02 | Dos diferencias declaradas, cada una con prueba: (1) el borrador conserva la precisión completa, y el clásico redondea a tres decimales el peralte de poste y las separaciones al releer sus cajas (3.14159 contra 3.142); (2) un poste que no está en el catálogo da paso al primero de la lista, como en el clásico, pero el borrador lo dice. Un poste del catálogo escrito con otras mayúsculas es ese poste; con el catálogo de hoy, de un solo poste, el clásico termina en el mismo | El borrador no cambia un valor que nadie tocó |
| S-03 | Al abrir, el borrador recompone, porque la carga del clásico termina en un recálculo. Un rack abierto y escrito sin tocar se escribe como lo escribiría el clásico, con sus normalizaciones de carga. Es distinto de Dinámico, que escribe lo que leyó (D-01) | El mismo resultado que el clásico, y un «Modificado» que vuelve a «Limpio» si se deshace un cambio |
| S-04 | La escritura relee el rack y el registro y no escribe si el vínculo de una holgura cambió desde la apertura, si la variable que la gobierna tiene otro valor o si el rack ya no abriría. Cualquier otro cambio del dibujo mientras se edita sigue sin detectarse (M-16 de Dinámico) | El clásico es modal y la ventana no: sin esto, escribir el estado abierto desharía en silencio un vínculo nuevo, o dibujaría con el valor viejo de la variable |
| S-05 | La rama de `EditSelective` que insertaba una vista nueva después de «Actualizar» no pasó a `ApplySelectiveEdit`: RACKEDITAR inserta por su propio camino y nunca la alcanzaba (inventario, dolor 17) | La mitad extraída es solo «Actualizar» |
| S-06 | El nombre con el que abre el borrador es el del documento interno, como lo muestra el clásico. Vacío, la escritura usa el del sobre, como RACKEDITAR | Es lo que hace el clásico |

**Las pruebas:**
- `SelectiveDraftEditorEquivalenceTests` (UI, 14): la equivalencia con el clásico en los casos de S-01, el frente que solo tiene un fondo, el rechazo de una holgura vinculada, las dos diferencias de S-02, la instantánea como copia, el recálculo fallido con la vuelta al último válido y los peraltes del catálogo.
- `SelectiveRackDoorTests` (Core, 12):
  - la lectura: abre como RACKEDITAR, la holgura vinculada con su valor, su literal y su nombre, la referencia rota bloquea, otro tipo no abre;
  - la escritura: se planea con los estados abiertos y conserva el vínculo, y se niega si cambió el vínculo, el valor de la variable o el rack ya no abre;
  - el cableado de las órdenes, de la puerta y de la mitad extraída, sobre la fuente.
- **Los censos:**
  - las órdenes pasan de 47 a 49 (`SelectiveEditorOpenTests`), y `CustomPropertiesCommandGuardTests` añade `A14SelectiveDoorCommands`;
  - la guarda de capas de `RackCadWindowListTests` añade `ApplySelectiveEdit` y `ReadProjectVariables`: solo las órdenes de la puerta los usan;
  - la caracterización de I-55 lee ahora el cuerpo de `ApplySelectiveEdit`. Cada base fijada de nuevo lleva su nota.
- **Comprobación en rojo:**
  - el diseño publicado sin copia: falló la prueba de las cabeceras personalizadas (una profundidad nueva lo alcanzaba sin recalcular);
  - los estados finales sin el valor nuevo de una holgura del rack: falló la de las ediciones;
  - las separaciones cargadas con el valor por defecto: fallaron dos;
  - la escritura sin comparar el vínculo, y sin comparar el valor de la variable: falló cada una de las suyas en Core;
  - los archivos se restauraron.
- **Core Full:** 19088 correctas, 0 con error, 403 omitidas (la base de `dc82a43c`, 19076, más las 12 nuevas).

**Lo que queda abierto:**
- La pantalla es E1-B: el editor de la ventana, «Editar aquí» para Selectivo y el consejero de cierre (D-08) con sus borradores.
- **Sin AutoCAD:**
  - que `RACKSELECTIVOABRIR` no deje nada en Deshacer espera el smoke, como M-15. A diferencia de la de Dinámico, lee el registro con la lectura de RACKEDITAR, que bloquea el documento y abre una transacción dentro de la orden;
  - esa lectura recorre todos los sobres del dibujo, como RACKEDITAR, en cada apertura y en cada escritura.
- `RACKEDITAR` sigue con el editor clásico.
- Para la entrega 2: la seguridad (se conserva), «Insertar vista…», un Selectivo nuevo y «Restaurar layout…».

## Selectivo en la ventana (línea D), E1-B

**Rama:** `architecture/workspace-persistente-rackcad-selectivo`, sobre E1-A (`8830a6b9`). E1-B es la pantalla: el editor de Selectivo en la ventana, «Editar aquí» para Selectivo y el consejero de cierre con sus borradores. Sin AutoCAD: la pantalla espera el smoke del Owner.

**Lo que se construyó:**
- **El editor** (`SelectiveEditorViewModel` y `SelectiveEditorView`, en `src/RackCad.UI/VentanaRackCad/`), paralelo al de Dinámico, con sus mismos estilos, tokens y controles del shell:
  - **La cabecera:** el nombre del rack, el estado Limpio / Modificado / Escribiendo, «Actualizar», «Descartar» y «Cerrar el editor», con su pregunta en línea si hay cambios. «Modificado» compara el JSON del diseño del borrador con el último leído o escrito, y el nombre recortado: deshacer un cambio vuelve a «Limpio». El aviso de la carga (`LoadNotice`) se dice al abrir.
  - **Las pestañas** Rack, Matriz (la inicial) y Avanzado. Sin vista previa (decisión 11).
  - **La forma de confirmar** es la de Dinámico: un campo va al borrador al salir de él o con Enter, y salir sin cambiar su texto no hace nada (un «Varía» vacío tampoco). Un valor que el borrador rechaza se queda en su campo con el motivo y no escribe nada. Un cambio que no se puede recomponer vuelve al último válido (`RestoreLastValid`, I-09 de Dinámico), con la selección como estaba, y no apaga «Actualizar».
  - **Matriz** frente × nivel sobre los fondos de la vista:
    - la celda muestra la tarima, ancho × alto (decisión 1), y «varía» en cursiva si los fondos no coinciden; «Ver detalle en celdas» añade las tarimas por nivel y el peralte;
    - el selector «Todos los fondos / Fondo 1 / Fondo n» (decisión 2) llama a `SetFondoScope`; empieza en «Todos» y con un fondo no aparece;
    - la raya de piso en N1 (decisión 4, ver S-11) y «½» en la cabecera de la columna de un frente en tramos (decisión 7);
    - la selección (clic, Ctrl, Mayús desde el ancla, F1…, N1… y «Todo») la lleva el modelo de la vista; el borrador recibe posiciones (`SelectiveMatrixPosition`) y frentes explícitos.
  - **El inspector,** a la derecha de la matriz (debajo, por debajo de 700 px, como Dinámico):
    - de las celdas: ancho y alto de tarima, tarimas por nivel, larguero y peralte (listas del catálogo; el peralte, solo los que admite el larguero: `BeamPeraltes`), largo manual del larguero y claro manual;
    - de los frentes: «Larguero a piso en N1» y su elevación, los niveles, la altura manual (vacía = calculada, decisión 9) y los tramos de medio frente (S-10);
    - cada escritura dice en la línea de mensajes su `Notice` (lo que no existe en un fondo de la vista, y que se escribió solo donde existe), y el inspector dice antes qué frentes de la selección solo existen en algún fondo.
  - **Vocabulario** (decisión 3): «Fondo» es la línea; «Profundidad» es la medida.
  - **Rack:**
    - «Lo esencial»: poste, peralte de poste, frentes, profundidad de tarima y de cabecera (en los fondos de la vista), número de fondos, separaciones entre fondos, tolerancia horizontal y holgura vertical;
    - una holgura vinculada (`IsLinked`) muestra su valor efectivo de solo lectura, con el nombre de la variable y el motivo (`LinkedReadOnlyReason`), sin «=Variable» ni control de vínculo (decisión 12). `LinkedPropertyEditor` no se toca: solo no aparece en esta vista;
    - «Calculado», de solo lectura (S-14);
    - «Poste y anotaciones», plegado: placa base, numerar frentes y niveles, nombre del rack, «Mostrar tarimas» con su nota (decisión 10), escala de texto, cotas (detalle, «Mostrar cotas en» frontal, lateral y planta, y estilo).
  - **Avanzado:** el peralte propio de cada poste (`SetPostPeralte`; vacío o igual al del rack = el del rack), la cabecera de cada poste por fondo («calculada» o «personalizada», `HasCustomCabecera`; se edita en el clásico) y los frentes en tramos.
  - **La pérdida que era silenciosa** (S-09).
- **La ventana** (`RackCadWindowViewModel`, `RackCadWindowView`, `RackListViewModel`):
  - los borradores de Selectivo van por (sesión, rack), como los de Dinámico; sobreviven al cambio de rack, la lista los marca igual y se podan con las sesiones muertas, diciendo lo que se perdió;
  - «Editar aquí» se ofrece para un Selectivo cuando la puerta es `IRackCadSelectiveDoorPort`; con una puerta de solo Dinámico no hay oferta;
  - la lectura (`RequestSelectiveDesign`) llega por `SelectiveRead` y abre el editor sobre el diseño efectivo, con los estados de las holguras y los nombres de las variables;
  - «Actualizar»: `SnapshotForWrite()` → `SelectiveDoorWrite` con la sesión y los estados finales → `RequestSelectiveUpdate`; la respuesta llega por `DesignWritten`. Escrito: el borrador vuelve a «Limpio» sobre lo escrito y «Descartar» vuelve ahí; fallo: sigue «Modificado» con el motivo;
  - una sola petición en vuelo para los dos sistemas (S-16), con «Dejar de esperar», la numeración, las respuestas tardías o de otro número ignoradas, la petición que la orden ya tomó, el borrador de otro dibujo que no se escribe en el activo y el estado «Escribiendo»: todo como Dinámico;
  - la página del rack aloja la vista del editor de Selectivo y la conserva mientras vive su borrador; «Ver el estado y la navegación del rack» vale para los dos editores.
- **El consejero de cierre** (D-08): `UnwrittenWorkOf(session)` cuenta los borradores modificados o escribiéndose de Dinámico y de Selectivo, y es lo que pregunta el consejero (`WorkspacePaletteHost`). Su texto ya no nombra un sistema (S-15).

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| S-07 | El editor de Selectivo usa los mismos campos que el de Dinámico (`DynamicFieldItem`, sin cambios) y su forma de confirmar. Lo propio es poco: una lista del catálogo (`SelectiveChoiceItem`: poste, larguero, peralte y detalle de cotas) y la selección en el modelo de la vista | Una sola forma de confirmar en las dos pantallas. El larguero y el peralte solo pueden ser del catálogo, como las listas del clásico, y escribir el id de un larguero no es razonable |
| S-08 | Un rack que el borrador no recompone al abrirlo (`LoadFrom` = false) se abre de solo lectura, con el motivo: «Actualizar» desactivado, ningún campo editable, y «Cerrar el editor» sin pregunta. Se edita con RACKEDITAR | No hay un diseño válido contra el que medir «Modificado», al que volver (`RestoreLastValid` lo necesita) ni que escribir |
| S-09 | Si reducir frentes o fondos quita cabeceras personalizadas o peraltes propios de postes que desaparecen, la línea de mensajes lo dice después de escribir: cuántos, que volver a subirlos no los recupera y que «Descartar» vuelve a lo último leído o escrito. El borrador hace lo mismo que el clásico; el aviso es de la ventana | El clásico los borra sin decir nada (inventario) |
| S-10 | Los tramos de medio frente (decisión 7) se escriben con «Aplicar tramos»: las filas del inspector son un borrador hasta aplicarlas, con la semilla y los gestos del diálogo clásico («Partir en tramos»: un tramo con largueros, con su longitud por escribir, y el resto; «+ Agregar tramo»; «Quitar»). «Sin medio frente» se aplica en el acto. Las comprobaciones son las de `SetSegments`. Es la única excepción a la forma única de confirmar | Una partición a medio escribir no es válida (el borrador la rechaza), así que no se puede escribir fila a fila. Es el «Aceptar» del diálogo, sin el diálogo |
| S-11 | La raya de piso (decisión 4) marca N1 de un frente **sin** «Larguero a piso», la tarima en el piso, como el draft aprobado (su leyenda: «En el piso, sin larguero»). Un frente con larguero a piso no la lleva. El encargo de E1-B decía «cuando el frente tiene larguero a piso»: se siguió el draft aprobado. **Para revisar**; la otra lectura es un cambio de una línea (`IsOnFloor`) | Manda el draft aprobado por el Owner con la propuesta de la decisión 4 |
| S-12 | El alcance de fondos vive en el borrador y se elige en «Matriz» y en «Rack», con el mismo selector, porque «Lo esencial» escribe los frentes y las profundidades en los fondos de la vista. Cambiarlo no cambia el borrador ni lo deja «Modificado» | Decisión 2: lo que ves es donde escribes, también en «Rack» |
| S-13 | Una celda que ningún fondo de la vista tiene no se puede elegir («—», botón desactivado). Una columna que solo tienen algunos fondos va en cursiva y su nombre accesible dice cuáles. Al cambiar el alcance, la selección se queda con lo que existe | Decisión 2: «un frente que solo existe en un fondo lo indica» |
| S-14 | «Calculado» lee lo que resolvió la última recomposición válida, sin cálculo propio: la altura de cabecera más alta; la altura de cada poste por fondo (con la de su cabecera personalizada, como la frontal); el largo del larguero de cada frente (el fondo maestro); el largo entre ejes de los postes extremos (la rejilla maestra); la profundidad de cabecera por fondo y, con más de un fondo, la total. Los conteos del draft (cabeceras, largueros, posiciones) no están | Esos conteos saldrían de reglas que la ventana no tiene: son las del BOM |
| S-15 | El texto del consejero de cierre ya no nombra un sistema: «Escríbelos con «Actualizar» en su editor (un rack nuevo de la ventana, con «Insertar en el dibujo…») o descártalos». La prueba de Core que fija el texto (`RackCadWindowCloseAdvisorTests`) cambia, con nota. `UnwrittenDynamicWorkOf` se conserva con su comportamiento, y sus pruebas no cambian | El texto anterior decía «un Dinámico nuevo» y valía solo para Dinámico; el nuevo vale para los dos |
| S-16 | La petición en vuelo lleva si es de Selectivo: la respuesta de una puerta no vale para la petición del otro sistema aunque el número coincida | Una sola petición para los dos sistemas, como pide el encargo, sin que una lectura de Dinámico abra un Selectivo ni al revés |
| S-17 | El peralte del larguero ofrece los peraltes que admiten todos los largueros de la selección (la intersección; con un solo larguero, los suyos). Sin peralte común, la lista queda vacía y lo dice | Cualquier valor de la lista se puede escribir en todas las celdas elegidas |
| S-18 | Los valores se muestran con hasta tres decimales, como las cajas del clásico. El borrador conserva la precisión completa (S-02), y mirar un campo sin cambiarlo no lo redondea | El borrador no cambia un valor que nadie tocó |
| S-19 | «Extremos y seguridad», «Insertar vista…», un Selectivo nuevo y «Restaurar layout…» no están, ni como botones desactivados. La seguridad del borrador se conserva intacta: el editor no la toca | Son la entrega 2 |

**Las pruebas** (en `tests/RackCad.UI.Tests/Workspace/`):
- `RackCadSelectiveEditorTests` (22):
  - el editor: abre en «Matriz», «Limpio», con la tarima por celda, la raya de piso y el nivel más alto arriba; el detalle de las celdas; la selección;
  - el estado: «Modificado» que vuelve a «Limpio» al deshacer, el nombre con espacios, «Escribiendo» sin poder editar, lo escrito como base de «Descartar», «Cerrar el editor»;
  - el alcance: «Varía», la columna de un solo fondo, escribir en «Fondo 2», la selección limpia al cambiar de alcance;
  - escrituras por celda y por frente con su aviso; el valor rechazado en su campo; los peraltes del catálogo;
  - la holgura vinculada de solo lectura, con su variable y sin «=»;
  - el cambio que no se recompone, con la vuelta al último válido y «Actualizar» encendido;
  - `LoadFrom` fallido: solo lectura y «Actualizar» desactivado; el aviso de la carga;
  - el aviso de pérdida al reducir frentes y fondos, y su ausencia sin nada personalizado;
  - «Rack», los peraltes propios por poste y los tramos de medio frente;
  - las vistas, en STA: se construye, sus tres pestañas, sin errores de enlace (un oyente de las trazas de WPF), la matriz se pinta, sin `LinkedPropertyEditor`; los botones de la matriz, la raya de piso y el inspector debajo en estrecho.
- `RackCadSelectiveWindowTests` (17), con una puerta doble que implementa `IRackCadSelectiveDoorPort`:
  - la oferta solo con esa puerta; «Editar aquí» → lectura → editor; la respuesta de otro número, o de la puerta de Dinámico, ignorada; la respuesta durante la llamada; el rechazo con su motivo;
  - una sola petición para los dos sistemas, «Dejar de esperar» y la respuesta tardía; AutoCAD ocupado;
  - la escritura con copias y estados finales, el editor bloqueado, «Limpio» al escribir; la escritura fallida sigue «Modificado»; «Dejar de esperar» de una escritura; la orden que ya tomó la petición; el borrador de otro dibujo;
  - el borrador sobrevive al cambio de rack; cerrar el editor; el consejero cuenta los borradores de Selectivo con los de Dinámico; los borradores de un dibujo cerrado;
  - Dinámico sigue igual con la puerta doble;
  - la vista de la ventana: aloja el editor de Selectivo, el panel en su lugar y la vista olvidada al cerrar.
- **Censos y guardas:** ninguno falló por lo nuevo. El editor es un `UserControl`, no una ventana, así que `WindowCensusGuardTests` no lo cuenta; la guarda de capas de `RackCadWindowListTests` pasa (los archivos nuevos no usan AutoCAD, el Plugin ni el bloqueo del documento). La única prueba que ya existía y cambia es la del texto del consejero de cierre en Core (`RackCadWindowCloseAdvisorTests`, S-15), con su nota. Las pruebas de Dinámico no cambian.
- **Comprobación en rojo:** siete piezas rotas a propósito, una a una, con la prueba de las dos clases nuevas tras cada una:
  1. «Modificado» que no vuelve a «Limpio» (sin comparar con la base): fallan la del estado, la de «Escribiendo», la de la escritura correcta, la de la orden que tomó la petición y la del consejero;
  2. la respuesta de otro número aceptada: falla la de «Editar aquí»;
  3. la holgura vinculada editable: fallan la de la holgura y la de la vista (busca la caja de solo lectura);
  4. el consejero sin los borradores de Selectivo: falla la suya;
  5. un enlace roto en la vista (`InspectorTitel`): el oyente de las trazas de WPF lo recoge y falla la prueba de «sin errores de enlace»; falla también la de los botones;
  6. la pérdida sin aviso: fallan las de frentes y fondos;
  7. sin la vuelta al último válido: falla la del cambio que no se recompone.

  Cada archivo se restauró de su copia y se comprobó que no quedan restos. La copia restaurada conservaba su fecha antigua, así que la compilación siguiente no la recompiló y la primera pasada completa de UI corrió con la última rotura: se tocaron las fechas, se recompiló y se repitió (abajo).
- **Core Full:** 19088 correctas, 0 con error, 403 omitidas: la base de E1-A, sin pruebas nuevas en Core (solo cambia el texto de una).
- **UI completa:** 2596 correctas, 0 con error, 48 omitidas (2644), con las 39 nuevas, sobre las fuentes restauradas. Las de la ventana, Dinámico y la equivalencia de Selectivo, juntas: 166 de 166.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- **El smoke del Owner en AutoCAD:** «Editar aquí» de un Selectivo, «Actualizar» y su «U», el veto al cerrar con un Selectivo sin escribir, el editor en la paleta (foco real, ratón, anchos), y que `RACKSELECTIVOABRIR` no deje nada en Deshacer (E1-A).
- S-11 para la revisión: el sentido de la raya de piso.
- Sigue sin detectarse un cambio del dibujo mientras se edita, salvo lo de S-04 (M-16 de Dinámico).
- `RACKEDITAR` sigue con el editor clásico.
- **La entrega 2:** «Extremos y seguridad» (decisiones 5, 6 y 13), «Insertar vista…», un Selectivo nuevo en la ventana y «Restaurar layout…» (decisión 8).

## Dinámico, correcciones de la revisión de Selectivo (2026-11-25)

La revisión de la entrega 1 de Selectivo (hallazgos I-01 y M-02) encontró dos fallos que Dinámico también tenía. Por orden del Owner («Corrige también Dinámico en su rama») se corrigieron aquí, sobre `dc82a43c`, sin commit. Fuera de estas dos correcciones Dinámico no cambia.

**1. La casilla «Guía de entrada» con «varía» quitaba en vez de poner (I-01 en Dinámico).**
- Con una selección mixta, la casilla del inspector está indeterminada. WPF, al hacer clic, la pasa a «sin marcar», no a «marcada», y la ventana quitaba la guía de todas las celdas de la selección.
- Ahora, mientras la casilla dice «varía», un clic pone la guía en todas las celdas activas de la selección y nunca la quita. Una vez igualadas, la casilla funciona como cualquier otra.
- Con la selección mixta, el rótulo es el del prototipo aprobado: «Guía de entrada (varía; marcar la pone en todas)».
- Las otras cuatro casillas del editor («Numerar frentes», «Numerar niveles», «Colocar nombre de rack» y «Reforzar el poste derivado») son de todo el rack y no pueden quedar indeterminadas: no cambian.

**2. La puerta leía y escribía sobre la primera vista (M-02 en Dinámico).**
- RACKDINAMICOABRIR abría el diseño del primer bloque que da `FindRackBlocks`, y RACKDINAMICOAPLICAR y RACKDINAMICOINSERTAR escriben el borrador en todas las vistas. Si las vistas hermanas tenían diseños distintos, la ventana abría uno cualquiera y «Actualizar» lo escribía en todas.
- **La regla nueva**, pura y genérica, está en `src/RackCad.Application/VentanaRackCad/RackSiblingDesigns.cs` (archivo propio, para no chocar con la rama de Selectivo):
  - `RackSiblingDesigns.Compare(string rackId, IReadOnlyList<RackSiblingView> views)` → `RackSiblingDesignsVerdict(Outcome, Reason, Diagnostic)`, con `Coincide`;
  - `RackSiblingView(SourceIdentity, Envelope)`: el bloque (su handle) y su sobre, con el diseño;
  - los resultados son `Same`, `Divergent` y `Unverifiable`;
  - usa el comparador que ya decide AUTHORED_DIVERGENT para cada tipo (`RackAuthoredComparatorPorts`): Selectivo con su autoridad, y Dinámico, Push Back, Cantilever y Cabecera con AUTH-13. No inventa otra igualdad.
- **Las tres órdenes** la llaman justo después de `FindRackBlocks`, antes de leer `blocks[0]` y antes de escribir nada. Si las vistas no coinciden:
  - RACKDINAMICOABRIR no abre el rack: «Las vistas de este rack no tienen el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar; «Actualizar» las iguala.»;
  - RACKDINAMICOAPLICAR y RACKDINAMICOINSERTAR no escriben nada, con el mismo motivo;
  - RACKDINAMICONUEVO no la usa: el rack todavía no está en el dibujo.
- **El texto es cierto (comprobado por lectura):** «Actualizar» de RACKEDITAR en Dinámico (`UpdateOnly`) redibuja en sitio todos los bloques que da `FindRackBlocks`, el mismo conjunto que compara la regla, con el diseño y el nombre del rack abierto desde la vista elegida. Hay dos salvedades: una vista lateral cuyo corte ya no existe se borra, y una vista que no se puede redibujar conserva su diseño.
- **D-11 (decisión de la sesión).** Mientras la casilla dice «varía», cualquier escritura pone la guía, también el «sin marcar» que manda WPF; una escritura nula no hace nada. No se usa una casilla de tres estados.
- **D-12 (decisión de la sesión).** La regla compara lo que compara el comparador del tipo, y nada más:
  - en los cuatro tipos de AUTH-13 cuenta también el nombre del rack: la ventana muestra uno y lo escribe en todas las vistas;
  - no cuentan la vista y el corte de cada bloque, sus propiedades personalizadas ni los valores por defecto heredados que el comparador documenta;
  - en Selectivo, su autoridad compara solo el diseño;
  - una sola vista coincide sin leerse, así que la puerta la abre como antes.
- **D-13 (decisión de la sesión, para revisar en la planeación).** Si no se puede comprobar que coincidan, la puerta no abre ni escribe (cerrada ante la duda), con su propio texto: «No se pudo comprobar que las vistas de este rack tengan el mismo diseño: ábrelo con RACKEDITAR desde la vista que quieras conservar.»
  - **Cuándo pasa:** una vista que el comparador no sabe leer (por ejemplo, un campo que no conoce en el diseño, como los que conserva una versión más nueva), vistas de tipos distintos, un tipo sin comparador demostrado (Cama) o una vista sin sobre.
  - **Por qué otro texto:** el del encargo («no tienen el mismo diseño») no sería cierto, y aquí «Actualizar» no siempre lo arregla.
  - **Riesgo:** un Dinámico de varias vistas con un campo desconocido en su diseño ya no se abre en la ventana. RACKEDITAR sí lo abre.

**Las pruebas:**
- UI, en `DynamicSafetyTabTests` (de 12 a 14):
  - `WhenTheGuideVariesAWriteOfTheBoxPutsItInEveryCellOfTheSelection`, del modelo de la vista: con «varía», el «sin marcar» y el «marcar» ponen la guía en las cinco celdas, una escritura nula no cambia nada, el rótulo cambia y, ya igualadas, desmarcar la quita;
  - `AClickOnTheIndeterminateGuideBoxPutsTheGuideInEveryCellOfTheSelection`, de vista con WPF real: la casilla indeterminada, con su rótulo, se alterna con `CheckBoxAutomationPeer` (`IToggleProvider.Toggle`). El primer clic pone la guía en todas las celdas y deja el rack «Limpio»; el segundo la quita de todas.
- Core, `RackSiblingDesignsTests` (nuevo, 12 casos):
  - un Dinámico con el mismo diseño en cuatro vistas distintas (dos laterales, frontal y planta): coinciden;
  - con diseños distintos: no coinciden, en cualquier orden y aunque solo difiera una de tres vistas;
  - el nombre cuenta;
  - una sola vista, aunque no se pueda leer, y ninguna: coinciden;
  - lo que el comparador ignora no las separa: dos valores por defecto heredados (la escala de anotación y la numeración de frentes, ausentes), la vista, el corte y las propiedades personalizadas;
  - lo que no puede leer no es igualdad: un campo desconocido, tipos distintos, Cama y una vista sin sobre;
  - Selectivo y Push Back, con sus comparadores;
  - la guarda de fuente: las tres órdenes llaman a la regla una vez, después de `FindRackBlocks` y antes de `blocks[0]` y de `ApplyDynamicEdit`, y responden con su motivo y salen (tres casos); RACKDINAMICONUEVO no la llama, y las vistas son todos los bloques del rack, cada uno por su handle.
- **Comprobación en rojo**, cada arreglo roto a propósito:
  1. el modelo de la vista escribe lo que manda WPF, como antes: fallan las dos pruebas de UI nuevas, con las cinco celdas sin guía;
  2. el rótulo fijo en la vista: falla la prueba de vista;
  3. la regla dice siempre «coinciden»: fallan seis pruebas (las de divergencia y de lectura imposible, y la de la lista vacía, por cómo se rompió);
  4. la regla abierta ante la duda (lo que no se puede comprobar, coincide): fallan dos;
  5. RACKDINAMICOABRIR sin la regla, y RACKDINAMICOAPLICAR con la regla después de `blocks[0]`: fallan tres pruebas de la guarda.

  Se restauraron por copia, con fecha nueva y el mismo hash que antes de romperlos, y se recompiló con `--no-incremental`.
- **Los números**, sobre `dc82a43c` con estas correcciones:
  - UI completa: 2545/0/48 (2593);
  - Core Full: 19088/0/403 (base de `dc82a43c`: 19076/0/403; las 12 nuevas son las de `RackSiblingDesignsTests`);
  - Plugin Debug: 0 errores.
