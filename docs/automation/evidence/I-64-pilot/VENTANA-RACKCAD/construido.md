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
4. la seguridad, «Insertar vista…» y un Dinámico nuevo desde la ventana.

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
- **Las pruebas:**
  - `DynamicSafetyTabTests` (9): las marcas de la matriz contra el BOM, la casilla del inspector, «En todas» y «En ninguna» con un frente en blanco, la tabla por poste contra el BOM, el diálogo contra el del clásico, el puerto, el bloqueo mientras AutoCAD escribe y el orden de las pestañas.
  - En `RackCadDynamicEditorTests`:
    - la ventana solo ofrece el diálogo de seguridad con el puerto del host;
    - «Insertar vista…» envía el borrador con las vistas elegidas;
    - su respuesta es la de una escritura;
    - sin el puerto no se ofrece.
  - En `RackCadWindowListTests`, la regla de la respuesta de la inserción y la elección de la orden en el puerto.
  - Los censos de órdenes pasan de 45 a 46.
  - Comprobaciones en rojo:
    - siete piezas rotas a propósito: las celdas con guía, quitar la guía, el eje de la tabla por poste, el diálogo sin guía, el resultado del diálogo ignorado, las vistas de la inserción perdidas y la marca de la matriz;
    - cada una hace fallar su prueba;
    - un control sin cambio de comportamiento pasa.

**Smoke exploratorio de la sesión (2026-11-24, por orden del Owner, sobre `78fd5cb1`):**
- pasan todos los pasos, también M-15 (la lectura no deja nada en Deshacer), I-04, I-05 con el ratón real, I-09, M-11, el «U» de «Actualizar» y D-07, que entonces era solo un aviso de pérdida;
- observaciones:
  - la cabecera no se refresca al «Volver a leer la lista»;
  - seleccionar el rack ya activo no salta a su página;
  - el inspector queda cortado al ancho inicial;
  - con varios monitores, la paleta y las modales clásicas abren en el principal.
