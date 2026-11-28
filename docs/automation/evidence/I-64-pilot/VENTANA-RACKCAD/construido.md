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
| S-02 | Dos diferencias declaradas, cada una con prueba: (1) el borrador conserva la precisión completa, y el clásico redondea a tres decimales el peralte de poste y las separaciones al releer sus cajas (3.14159 contra 3.142); (2) un poste que no está en el catálogo da paso al primero de la lista, como en el clásico, pero el borrador lo dice. Un poste del catálogo escrito con otras mayúsculas es ese poste; con el catálogo de hoy, de un solo poste, el clásico termina en el mismo. **Y las diferencias de los gestos del clásico** (M-01 de la revisión de la entrega 1; declaradas, sin prueba del clásico): en el clásico basta con hacer clic en otra celda para que la caja de la celda se relea y se copie en ella si «difiere» (`TryCommitEditedCell`), así que (3) un frente, un alto, un largo o un claro manual con más de tres decimales queda redondeado a tres (47.2441 pasa a 47.244); (4) un peralte de larguero que el catálogo no admite para ese larguero pasa al primero de su lista, porque el combo eligió el índice 0 (`RefreshPeralteCombo`); y (5) su «Aplicar a» copia las siete magnitudes de la celda en cada destino (`CopyFrom`), donde la ventana solo escribe el campo editado. El borrador no hace ninguna de las tres. La equivalencia de S-01 vale para las mismas ediciones sin navegar la matriz del clásico | El borrador no cambia un valor que nadie tocó |
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

## Selectivo en la ventana (línea D), correcciones de la revisión de la entrega 1

**Rama:** `architecture/workspace-persistente-rackcad-selectivo`, sobre E1-B (`dc23b820`). Corrige los hallazgos de la revisión independiente de `8830a6b9` (E1-A, APTO) y `dc23b820` (E1-B, APTO TRAS CORRECCIONES): I-01 y M-01 a M-11. Sin AutoCAD y sin tocar Dinámico: su comportamiento y sus pruebas no cambian.

**Cómo quedó cada hallazgo:**

| Hallazgo | Estado | Qué se hizo |
|---|---|---|
| I-01 | CORREGIDO | «Larguero a piso en N1» con frentes que difieren: un clic lo pone en todos los frentes de la selección, nunca lo quita. El `false` que manda WPF desde una casilla indeterminada significa «todos». El rótulo dice «Larguero a piso en N1 (varía; marcar lo pone en todos)», como el draft (S-20). Es la única casilla del editor de Selectivo con «varía»: las demás (placa base, numeración, nombre, tarimas, cotas por vista, «con largueros» de cada tramo, «Ver detalle») son de todo el rack o de una fila y nunca están indeterminadas |
| M-01 | DECLARADO | S-02 de E1-A recoge ahora las tres diferencias de los gestos del clásico: el redondeo a tres decimales al cambiar de celda, el peralte fuera del catálogo que pasa al primero de la lista y «Aplicar a» que copia las siete magnitudes |
| M-02 | CORREGIDO por la rama de Dinámico (S-25; ver «Selectivo: M-02 con la regla de Dinámico», más abajo) | Por orden del Owner, la regla de las vistas hermanas con diseños distintos se construye una vez, genérica para los dos sistemas, en la rama de Dinámico, y después se integra aquí y se aplica a RACKSELECTIVOABRIR y RACKSELECTIVOAPLICAR. Esta corrección no la toca: la puerta sigue leyendo y escribiendo sobre `blocks[0]` |
| M-03 | CORREGIDO | `LoadFrom` comprueba los frentes antes de copiar el diseño, así que la rama «El diseño leído no tiene frentes: el editor conserva el rack que tenía» ocurre de verdad (S-29) |
| M-04 | CORREGIDO | Los tramos escritos y sin aplicar son un cambio: el editor queda «Modificado», «Cerrar el editor» pregunta y D-08 veta el cierre del dibujo. «Actualizar» no escribe y dice «Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.». Otro frente u otros fondos de la vista los descartan con un aviso en la línea de mensajes, y «Descartar» también. Se añade «Descartar tramos» (S-21) |
| M-05 | CORREGIDO | Con cotas heredadas (`DimensionViews` nulo), si las tres casillas vuelven a como las mostraba la política cargada, el valor vuelve al cargado exacto y el editor a «Limpio» (S-22) |
| M-06 | CORREGIDO | Los rechazos de S-04 dicen lo que se puede hacer: «Cierra el editor (los cambios de la ventana se pierden) y vuelve a abrirlo con «Editar aquí».» (S-24) |
| M-07 | CORREGIDO | Con toda la selección en N1 en el piso, sin larguero a piso en ningún fondo de la vista, el larguero, su peralte y el claro quedan desactivados, con «Sin larguero (piso)» y «No en N1» (S-23) |
| M-08 | CORREGIDO | Una guarda de fuente en Core fija `SetWindowWork(_window.UnwrittenWorkOf)` en `WorkspacePaletteHost.cs` |
| M-09 | CORREGIDO | Un Selectivo sin nombre interno sale en el veto de cierre y en la poda con el nombre del sobre, el de la lista de la ventana, nunca con su GUID (S-26) |
| M-10 | CORREGIDO | «Estilo de cota» es una lista: «(Automático)», los estilos del dibujo (RACKSELECTIVOABRIR los lee con `ReadDimensionStyleNames`, como RACKEDITAR, y los manda en `SelectiveDesignRead.DimensionStyles`, campo nuevo al final y opcional) y el estilo cargado aunque falte en el dibujo, como el clásico (S-27) |
| M-11 | CORREGIDO | «Escribiendo…» deja de ofrecer «Dejar de esperar» cuando la orden ya tomó la petición (S-28), y el rechazo de S-04 nombra las dos holguras si cambiaron las dos (S-24) |

**Las decisiones nuevas:**

| Id | Decisión | Por qué |
|---|---|---|
| S-20 | «Larguero a piso en N1» con los frentes de la selección distintos: la casilla queda indeterminada, su rótulo dice «(varía; marcar lo pone en todos)» y un clic lo pone en todos (el `false` de WPF desde la casilla indeterminada se lee como «todos»). Desde «todos», el clic siguiente lo quita de todos | El draft aprobado: «marcar lo pone en todos» (`toggleFloor`: si no están todos, los pone) |
| S-21 | Los tramos del inspector que difieren de los del frente en el borrador (comparados como se muestran, con hasta tres decimales, así que una fila sin tocar nunca está pendiente) son un cambio. «Actualizar» se niega con su texto, y su botón queda desactivado con ese motivo. Otro frente o un cambio de «Fondos de la vista» los descarta y lo dice: «Los tramos sin aplicar de F2 se descartaron: se eligió otro frente u otros fondos de la vista.». Otra celda del mismo frente los conserva. «Descartar» del editor los descarta sin aviso propio. «Sin medio frente» y «Aplicar tramos» los resuelven. Se añade el botón «Descartar tramos» (vuelve a los del borrador), para que «descártalos» tenga un gesto directo | El clásico era modal y preguntaba por la celda sin aplicar antes de dibujar; la ventana no lo es, y lo pendiente no puede desaparecer en silencio ni dejar un «Limpio» falso. El botón no estaba en el encargo: sin él, la única forma de descartar sin perder lo demás era cambiar de frente |
| S-22 | Las cotas por vista vuelven al valor cargado exacto cuando las tres casillas vuelven a como las mostraba la política cargada: es «no tocadas» de `DimensionViewPolicy.FromEditor`. **Diferencia declarada con el clásico:** el clásico, en ese caso, guarda 7 (las tres vistas) en un rack heredado; el borrador conserva el nulo. El dibujo es el mismo, porque el nulo muestra las tres | Salir de un campo sin cambiarlo no cambia nada; un falso «Modificado» vetaba el cierre del dibujo |
| S-23 | El larguero, su peralte y el claro se desactivan solo si **toda** la selección es N1 sin larguero en **ningún** fondo de la vista (en «Todos los fondos», basta un fondo con larguero a piso para ofrecerlos). Lo que guardan se conserva. La tarima y el largo manual del larguero siguen editables | El draft aprobado (`'Sin larguero (piso)'`, el claro con `'No en N1'`). Lo que no se dibuja no debe dejar el borrador «Modificado» |
| S-24 | Los rechazos de S-04 terminan con «Cierra el editor (los cambios de la ventana se pierden) y vuelve a abrirlo con «Editar aquí».», y nombran todas las holguras que cambiaron, siempre en el mismo orden (la holgura vertical, la tolerancia horizontal): «Los vínculos de la holgura vertical y de la tolerancia horizontal cambiaron…», «Los valores de lo que gobierna la holgura vertical y la tolerancia horizontal cambiaron…» | «Descartar» vuelve a lo que se abrió y no relee el dibujo, así que «Vuelve a abrir el rack» no se podía hacer sin cerrar el editor |
| S-25 | M-02 (las vistas hermanas con diseños distintos): la regla `RackSiblingDesigns` llegó por la rama de Dinámico (`8fd26a14`) y se aplica a RACKSELECTIVOABRIR y RACKSELECTIVOAPLICAR (`5052a0ca`) | Orden del Owner: una sola regla, genérica para los dos sistemas |
| S-26 | El nombre de un Selectivo en el veto de cierre y en la poda: el del borrador; si está vacío, el del sobre tal como lo tenía la lista de la ventana al abrir el editor (se guarda con el borrador, de solo lectura); el id solo si tampoco hay ese | El dibujo y la lista muestran el nombre del sobre; el GUID no le dice nada al usuario |
| S-27 | «Estilo de cota» es una lista: «(Automático)» primero, los estilos del dibujo (recortados, sin repetir, sin vacíos), y luego el estilo con el que abrió el rack y el actual, si el dibujo no los tiene. Elegir «(Automático)» guarda el estilo vacío. Sin estilos leídos, la lista tiene «(Automático)» y el estilo cargado | Como el clásico (`SetDimensionStyles` y `SelectDimStyle`): un error al escribir ya no puede guardar un estilo que no existe |
| S-28 | Cuando la orden toma la escritura (`RequestStarted`), la ventana avisa al editor de Selectivo (`WriteTaken`), que deja de decir «pulsa «Dejar de esperar»» y dice «AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.». La escritura siguiente empieza otra vez sin tomar | Lo resolvió así la ventana para Dinámico tras I-02 de su quinta revisión (`CanStopWaiting` y su texto); el editor de Dinámico no se toca |
| S-29 | `LoadFrom` con un diseño sin frentes: se comprueba antes de copiarlo (la copia pasa por el almacén, que lo rechaza), así que el borrador conserva el rack que tenía, recompuesto, y lo dice en `LoadNotice`. Por la puerta sigue siendo inalcanzable: `AnswerForSelectiveOpen` lo rechaza antes | Lo que hace el clásico (`LoadDesign` vuelve sin cambiar nada), y lo que el comentario ya prometía |

**Las pruebas** (las sondas P1, P2, P3, P8 y P9 de la revisión pasaron a ser pruebas):
- `RackCadSelectiveEditorReviewTests` (UI, nueva, 15):
  - I-01: el modelo de la vista (indeterminada, «todos», desde «todos» quita, `null` no cambia nada) y la vista con WPF real: `CheckBoxAutomationPeer` → `IToggleProvider.Toggle` con selección mixta pone el larguero a piso en F1 y F2, la casilla queda marcada y el rótulo cambia; el segundo clic lo quita de los dos (P8);
  - M-04: P9 (tramos pendientes, otro cambio, «Actualizar» no escribe y lo dice, «Cerrar el editor» pregunta, aplicados se escriben); solo tramos pendientes, y deshacerlos es «Limpio»; otro frente y otros fondos los descartan con su aviso, otra celda del mismo frente no; «Descartar tramos» y «Descartar»; los tramos de un frente partido sin tocar no están pendientes (30 y 30.0 son el mismo); la vista ofrece «Descartar tramos» solo con tramos pendientes y «Actualizar» muestra el motivo;
  - M-05: P2, y una política presente con un bit desconocido que vuelve exacta;
  - M-07: F2·N1 en el piso desactiva el larguero, el peralte y el claro con sus textos, y escribir ahí no cambia nada; F1·N1 con larguero a piso y una selección mixta los ofrecen; en dos fondos, un fondo con larguero a piso basta;
  - M-10: la lista con los estilos del dibujo, el cargado conservado, «(Automático)» y la vuelta a «Limpio»;
  - M-11: el texto antes y después de `WriteTaken`, y la escritura siguiente;
  - M-03: P3, un diseño sin frentes no lanza y conserva el rack que había;
  - P1: «Descartar» y la vuelta al último válido no cambian el diseño en siete formas de rack, y ningún frente queda con tramos pendientes al abrir.
- `RackCadSelectiveWindowTests` (UI, +4): los tramos pendientes solos vetan el cierre (D-08) y la lista marca «Modificado»; el Selectivo sin nombre interno sale con el nombre de la lista en el veto y en la poda; la lectura con estilos llena la lista; tras `RequestStarted` el editor ya no ofrece «Dejar de esperar».
- `RackCadSelectiveEditorTests` (UI): la prueba de los tramos cambia en una línea, con su nota (tras «Partir en tramos» el editor queda «Modificado», S-21) y comprueba además «Actualizar».
- `SelectiveRackDoorTests` (Core, +2, y dos con el texto entero): los textos de S-24; las dos holguras a la vez (vínculo y valor); el campo nuevo de la lectura es opcional; el cableado de RACKSELECTIVOABRIR lee los estilos solo para un rack que abre, después de la regla y antes de responder.
- `RackCadWindowCloseAdvisorTests` (Core, +1): la guarda de M-08 (una sola instalación, con `UnwrittenWorkOf`, y sin `UnwrittenDynamicWorkOf`).
- Las pruebas de Dinámico no cambian.

**Comprobación en rojo:** doce piezas rotas a propósito, una a una, con las clases afectadas tras cada una (en UI, `RackCadSelectiveEditorReviewTests`, `RackCadSelectiveEditorTests` y `RackCadSelectiveWindowTests`, 58 casos; en Core, `SelectiveRackDoorTests` y `RackCadWindowCloseAdvisorTests`, 21):
1. **I-01**, el setter que escribe el `false` de WPF tal cual: fallan las dos de I-01, la del modelo de la vista y la de la vista con `IToggleProvider.Toggle`;
2. **M-04**, los tramos pendientes fuera de «Modificado»: fallan P9, la de solo tramos pendientes, la del veto en la ventana y la de los tramos que ya existía;
3. **M-04**, sin aviso al cambiar de frente: falla la del aviso;
4. **M-05**, las casillas de cotas siempre «tocadas»: falla P2;
5. **M-07**, N1 en el piso editable: fallan las dos de M-07;
6. **M-10**, la lista sin los estilos del dibujo: fallan la del editor y la de la ventana;
7. **M-11**, el editor sin aviso de que la orden tomó la escritura: falla la de la ventana;
8. **M-09**, el veto con el GUID: falla la suya;
9. **M-03**, la copia antes de comprobar: falla P3, con la excepción de antes;
10. **M-06**, el texto viejo («Vuelve a abrir el rack…»): fallan las tres de Core que lo fijan;
11. **M-11**, solo la primera holgura: falla la de las dos holguras;
12. **M-08**, `SetWindowWork(_window.UnwrittenDynamicWorkOf)`: falla la guarda de fuente en Core.

M-02 no tiene comprobación en rojo: no está en esta corrección. Cada archivo se restauró desde sus bytes originales con fecha nueva (la lección de E1-B), se comparó byte a byte con la copia y se buscaron restos; después se recompiló todo con `--no-incremental` antes de las suites completas.

**Los números** (sobre las fuentes restauradas y recompiladas con `--no-incremental`):
- **UI completa:** 2615 correctas, 0 con error, 48 omitidas (2663): la base de E1-B, 2596, más las 19 nuevas. Las focales de Selectivo, la ventana y Dinámico, juntas: 163 de 163.
- **Core Full:** 19091 correctas, 0 con error, 403 omitidas (19494): la base, 19088, más las 3 nuevas.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que vi en Dinámico, sin tocarlo** (para su rama):
- **I-01:** la casilla «Guía de entrada» (`DynamicEditorView.xaml`, `SelectionGuide`) tiene el mismo patrón: `IsThreeState="False"` sobre un `bool?` que devuelve `null` con celdas mixtas, y un setter que escribe el `false` que manda WPF, así que un clic con la selección mixta quita la guía de todas las celdas. Tampoco dice «varía».
- **M-02:** RACKDINAMICOABRIR, RACKDINAMICOAPLICAR y RACKDINAMICOINSERTAR leen y escriben sobre `blocks[0]`; es la regla que llega por su rama.
- **M-11:** el editor de Dinámico sigue diciendo «pulsa «Dejar de esperar»» aunque la orden ya tomó la petición; la ventana sí lo resuelve (`CanStopWaiting` y `WaitingText`).

**Lo que queda abierto:**
- **M-02**, por la rama de Dinámico.
- **El smoke del Owner en AutoCAD:** lo de E1-B, más la casilla «varía», los tramos pendientes, la lista de estilos con los del dibujo y el texto tras «Started».
- S-11 sigue para la revisión; S-21 (el botón «Descartar tramos») y S-22 (el nulo conservado) son nuevas y conviene que las vea.

## Dinámico, correcciones de la revisión de Selectivo (2026-11-25)

La revisión de la entrega 1 de Selectivo (hallazgos I-01 y M-02) encontró dos fallos que Dinámico también tenía. Por orden del Owner («Corrige también Dinámico en su rama») se corrigieron aquí, sobre `dc82a43c` (commit `8fd26a14`). Fuera de estas dos correcciones Dinámico no cambia.

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

### Selectivo: M-02 con la regla de Dinámico

- **Qué:** RACKSELECTIVOABRIR y RACKSELECTIVOAPLICAR (`src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs`) llaman a
  `RackSiblingDesigns.Compare(pending.RackId, Views(blocks))` justo después de `FindRackBlocks` y antes de `blocks[0]`; si las vistas no
  coinciden, la lectura responde `SelectiveDesignRead.Refused(..., siblings.Reason)` y la escritura `RackDesignWritten(..., false, siblings.Reason)`,
  sin escribir nada. Reutiliza el `Views(blocks)` de Dinámico. El texto del motivo es cierto para Selectivo: `ApplySelectiveEdit` (el
  «Actualizar» de RACKEDITAR) redibuja en sitio todas las vistas con el mismo GUID con el diseño de la elegida; no se cambió.
- **Prueba:** `RackSiblingDesignsTests.TheSelectiveCommandsOfTheDoorCompareTheViewsBeforeReadingOrWriting` (teoría, 2 casos) fija una sola
  llamada por orden, después de `FindRackBlocks`, antes de `blocks[0]` y, en la escritura, antes de `ApplySelectiveEdit`, y la respuesta con
  `siblings.Reason`; el recuento del archivo pasa de 3 a 5. La regla con dos diseños de Selectivo iguales y distintos ya estaba probada
  (`TheRuleIsTheSameForSelectiveWithItsOwnComparator`). Focales (`RackSiblingDesignsTests`, `SelectiveRackDoorTests`,
  `RackCadWindowListTests`, `CustomPropertiesCommandGuardTests`): 126/126. Build Debug del Plugin: 0 errores.
- **Rojo:** sin la llamada en RACKSELECTIVOAPLICAR fallan 2 de 14 (`...SelectiveCommands...(RACKSELECTIVOAPLICAR)` y el recuento de 5);
  restaurada con fecha nueva, 14/14 y recompilado.

## Selectivo en la ventana (línea D), pulido de la entrega 1 antes de la entrega 2 (tercera tanda, β)

**Rama:** `architecture/workspace-persistente-rackcad-selectivo`, sobre `1f0bd3df`, sin commit. Orden del Owner: «Continúa» (tras el smoke de
la entrega 1). Cinco encargos: la fidelidad con el draft aprobado (las 10 «A» de la revisión de fidelidad y las «B» que son solo texto), la
accesibilidad, el posible fallo A de `SetSegments`, las pruebas que faltaban de prioridad A y las listas desplegables (O-D del smoke). Sin
AutoCAD y sin tocar Dinámico (ni su vista ni `DynamicFieldItem`, que comparten los dos editores).

**1. Fidelidad con el draft.** Donde el plan había reducido el draft, manda el draft.

| Hallazgo | Estado | Qué se hizo |
|---|---|---|
| F-01 | CORREGIDO | «Lo esencial» son los tres grupos del draft: **Estructura** (Frentes (bahías), Niveles, Fondos, Tarimas por nivel), **Tarima** (Ancho, Alto, Profundidad) y **Larguero y holguras** (Larguero, Peralte del larguero, Holgura horizontal, Holgura vertical), con sus pistas. Cada campo escribe **todo el rack, en todos los fondos**, por el mismo camino que «Todo» de la matriz con «Todos los fondos» (SF-01) |
| F-02 | CORREGIDO | Poste y Peralte del poste pasan a «Poste y anotaciones», primeros, con la pista del draft |
| F-06 | CORREGIDO | Con dos o más fondos, la tabla **«Fondos»**: Fondo · Frentes · Profundidad de tarima · Profundidad de cabecera (vacía: «Auto: n») · Separación al siguiente («Último» en el último) · Estado, con el texto de cabecera y la nota del maestro del draft. Cada fila escribe solo en su fondo. Sale el selector «Fondos de la vista» de la pestaña Rack y la lista «Separaciones entre fondos» (SF-02, SF-03) |
| F-09 | CORREGIDO | «Calculado» es el del draft: Largo del larguero (valor o rango), Altura de cabecera «≈ n in», Separación N2 → N3, Profundidad de cabecera (del fondo 1), Cabeceras, Largueros, Posiciones de tarima y, con doble fondo, Separadores; cada uno con su pista. Las cuentas de piezas salen del BOM (SF-04). Salen «Altura por poste» (va a Matriz), «Largo entre ejes» y «Profundidad total» |
| F-15 | CORREGIDO | Bajo la matriz, **«Altura por poste»**: P1…Pn «≈ n in» por cada fondo de la vista, marcados si una altura manual manda (borde de aviso), con la nota del draft |
| F-16 | CORREGIDO | Bajo la matriz, la **leyenda**, y las marcas en la celda: punto de aviso = «Modificada en el borrador», punto de información = «Distinta del resto», «M» = valor manual; «varía» (con doble fondo), la raya de piso y «½». Sin «Valor no válido» ni T/P (SF-05) |
| F-20 | CORREGIDO | El inspector tiene el alcance **Celda · Frente · Nivel · Todo** («Celdas (n)» con varias) y la línea **«Destino: n celdas [en el fondo k / en los fondos 1 y 2] · frentes …»** (SF-06) |
| F-23 | CORREGIDO | «Largo de larguero» y «Claro debajo», sin la explicación en el rótulo. Junto al largo, el valor automático: «Automático: 96 in.» vacío, «Vacío = automático (96 in).» con un valor manual, más la pista del draft; el claro, «Vacío = automático» y su pista (el draft no da número) (SF-07) |
| F-24 | CORREGIDO | La sección del frente se titula como el draft («Frente 2», «Frentes 1, 2 · fondos 1 y 2») con su resumen («3 niveles · larguero a piso · medio frente»). «Altura manual de cabecera» con «Calculada: ≈ n in.» (o «Vacía = calculada (≈ n in).») y «Manda en los dos postes del frente.»; la elevación con «Sobre el primer troquel; un frente nuevo copia la del último.» (SF-08) |
| F-27 | CORREGIDO | «Postes y cabeceras» es una tabla con encabezados Poste · Peralte propio (in) · **Altura** («Altura (por fondo)» con doble fondo: «≈ n in / ≈ m in») · Cabecera, y el resumen «n postes · n peraltes propios · n cabeceras personalizadas». La caja dice «Del rack: 3» cuando está vacía |

Las «B» que son solo texto también: F-03 (Holgura horizontal / vertical con sus pistas), F-04 («Fondos» con su pista), F-05 (Profundidad en
Tarima, «Hoy «Fondo de tarima».»), F-07 (el texto de «Lo esencial» del draft; el vocabulario queda en la cabecera de «Fondos»), F-08 (pistas
de Niveles, Tarimas por nivel, Peralte), F-10 (pistas de «Calculado»), F-11 (orden de «Poste y anotaciones»: campos, casillas, «Mostrar
tarimas», cotas), F-12 (resumen en el encabezado plegado: «Poste 3 in · cotas: Estándar · con tarimas»), F-13 (nota de «Mostrar tarimas» del
draft), F-14 (rótulos limpios con la explicación como pista), F-18 («Ver detalle en celdas»), F-19 (sin «; «varía» si no coinciden»), F-21
(títulos del inspector), F-22 (grupos Tarima · Larguero (con resumen) · Manual (con resumen)), F-25 (tabla de tramos con encabezados, «(resto)
≈ n in», «Partir el frente», «Agregar tramo»), F-28 (la nota de Avanzado remite a «Poste y anotaciones») y F-30 («Ningún frente está
partido.» / «Frentes partidos: … .»). F-17 (el selector de fondos) se hizo como el draft, botones segmentados, por O-D (punto 5). **Quedan:**
F-26 (tope y parrilla en la nota de N1: entrega 2), F-29 (los botones desactivados «Editar cabecera…» / «Reutilizar cabecera…»: no es solo
texto, y el texto de hoy, «usa RACKEDITAR», es el cierto mientras no exista el editor de cabeceras) y F-31 (la pregunta de cierre con
«Actualizar y cerrar»: es comportamiento nuevo).

**Las decisiones nuevas** (prefijo SF, para no chocar con las S-nn de las otras tandas):

| Id | Decisión | Por qué |
|---|---|---|
| SF-01 | «Lo esencial» escribe por `SelectiveDraftEditor.WithinScope(null, …)`: la misma escritura del borrador que «Todo» de la matriz con «Todos los fondos» (las mismas `Set…` sobre todas las posiciones y frentes), sea cual sea la vista de la matriz, que se restaura después (también si la escritura falla). No dice «Parte de lo elegido no existe en todos los fondos»: es todo el rack. Lee igual, con «Varía» si los fondos o las celdas difieren | El draft: «Escribe en todo el rack, en todos los fondos» (`editCells(…, true)` sobre `allCells`); el encargo: «por el mismo camino que la matriz con «Todo»» |
| SF-02 | Con **un** fondo, «Profundidad de cabecera» queda en el grupo Tarima («Auto: 42 in (la de la tarima − 6 in).»). **Diferencia declarada con el draft**, que solo la tiene en la tabla «Fondos» | Quitarla dejaba sin forma de dar a un rack de un fondo su propia profundidad de cabecera, que el clásico sí tiene |
| SF-03 | El «Estado» de una fila de «Fondos»: «Correcto», o «1 problema: <motivo>» del último valor rechazado en la fila (en rojo) | El borrador nunca guarda un valor no válido (se rechaza en el campo): el único problema de una fila es el valor que se acaba de rechazar. El motivo a la vista, porque en una tabla no cabe debajo de la caja |
| SF-04 | Las cuentas de «Calculado» salen del BOM: `SelectiveBomCounts.Of(system, catalog)` suma por categoría los componentes de `SelectiveBomBuilder.Build` (Cabecera, Larguero, Separador); no cuenta nada de nuevo. «Posiciones de tarima» = celdas × tarimas por nivel en todos los fondos (no es una pieza del BOM). Se calcula al leerse, una vez por recomposición, y la vista solo lo vuelve a leer con la pestaña Rack a la vista (el BOM de un rack de 60 × 6 × 2 cuesta ~70 ms). «≈» como el draft, aunque el valor es el del resolvedor | Encargo: «sin inventar conteos de BOM: … usa las reglas del BOM existentes» |
| SF-05 | La leyenda no tiene «Valor no válido» (la ventana rechaza un valor no válido antes de que llegue al borrador: ninguna celda lo puede tener) ni T/P (entrega 2). «Modificada» compara cada celda (tarima, tarimas, larguero, peralte, largo y claro manuales) y el larguero a piso de su frente con lo último leído o escrito: la base se toma al abrir, al descartar y tras escribir; una celda nueva está modificada. «Distinta del resto» = otro contenido que el más común de la vista (como el draft); una modificada dice modificada | Lo que el draft muestra, sin marcas que no pueden ocurrir |
| SF-06 | El alcance del inspector lo fijan los gestos de la matriz (clic = Celda; F… = Frente; N… = Nivel; «Todo» = Todo) y sus botones: «Celda» deja la principal; «Frente», todas las celdas de los frentes de la selección; «Nivel», los niveles de la selección en todos los frentes; «Todo», todo lo de los fondos de la vista. Títulos del draft («Frente 1 · Nivel 1», «n celdas», «Frentes 1, 2», «Niveles 2», «Todo el rack», «Todo el fondo 2»). «Destino» cuenta cada posición en cada fondo de la vista que la tiene | `setScope` y `targetText` del draft |
| SF-07 | Los valores automáticos y calculados los da `SelectiveDraftEditor.AutomaticValues(celdas, frentes)`: el MISMO resolvedor sobre una copia del último diseño válido sin esos valores manuales (el largo de su frente, donde manda el nivel más ancho, también otro manual, y la altura del frente). Van en la pista del campo, no dentro de la caja: WPF no tiene «placeholder» y la plantilla del campo es la de Dinámico, que no se toca | Sin repetir reglas de geometría; «vacío = automático» dice lo que de verdad saldría al vaciarlo |
| SF-08 | La pista de la elevación dice «un frente nuevo copia la del último» y no «nace con 4» (el draft) | En el borrador, como en el clásico, un frente nuevo copia la elevación del último; 4 solo sin origen (`SelectiveEditorState`) |
| SF-09 | El selector «Todos los fondos · Fondo 1 · Fondo n» de la matriz son botones segmentados (como el draft), con nombre de automatización «Fondos de la vista: Fondo 2» y estado «elegido» | F-17 y O-D (punto 5): sin lista desplegable |

**2. Accesibilidad** (`sprint\a11y.md`, sin tocar Dinámico): cada celda se llama por su posición, única en la matriz («Frente 2 · Nivel 3:
42×60», más «; en el piso, sin larguero», «modificada en el borrador», «distinta del resto», «con un valor manual»; una que no existe, «Frente 2 ·
Nivel 3: no existe en los fondos de la vista»); la cabecera de fila, «Nivel 1, el piso: seleccionar la fila»; los controles repetidos por fila
llevan su fila: «Tramo 2: longitud», «Tramo 2 con largueros», «Quitar el tramo 2», «P3: peralte propio», «Fondo 2: profundidad de tarima» (y
los demás de «Fondos»); los botones segmentados, «Alcance de la edición: Frente», «Fondos de la vista: Fondo 1»; la casilla de larguero a
piso se llama como su rótulo («Larguero a piso en N1», o con «(varía; …)»); de paso, las tres casillas de cotas, «Mostrar cotas en la
frontal», etc. Los campos dan su pista como `HelpText`. Lo demás de `a11y.md` (orden de tabulación, flechas, atajos, foco tras acciones,
`LiveSetting`) queda como estaba, para otra vuelta.

**3. El posible fallo A de `SetSegments`: no es un fallo.** Con «Todos los fondos», `FrontWidth` toma el ancho del primer fondo que tiene el
frente, pero el resolvedor da a todos los fondos que tienen un frente el MISMO ancho (el del fondo maestro, o el más ancho si el maestro tiene
una columna vacía ahí), y el diálogo clásico comprueba el ancho del frente en el fondo visible (`TramosFullWidth`). Prueba nueva contra el
clásico real (`RackSelectiveWindow.TramosFullWidthForTest` en los dos fondos) con el fondo 1 más estrecho en F1: el mismo ancho en los dos
fondos del clásico y en los tres alcances del borrador, y los mismos tramos rechazados o escritos en cada alcance. Sin sistema (antes de la
primera recomposición) el ancho es 0 y no se comprueba, como el clásico con 0 (probado también).

**4. Las pruebas que faltaban (prioridad A).** `SelectiveDraftEditorGapTests` (UI, nueva, 16), sobre el borrador: el tope de 4 fondos, las
separaciones al crecer (la última) y al reducir (se conservan), el alcance que desaparece, el mismo número sin escritura; menos frentes y el
frente nuevo que copia la elevación; los rechazos (≤ 0, NaN, infinito) de profundidad, cabecera, separación, ancho, largo y claro sin escribir
nada, y la separación que no existe; la elevación 0 como valor; «vacío = automático» del largo, el claro, la altura y la cabecera, que
devuelven el dibujo a la regla (y el JSON al del rack abierto); el automático con otro manual que manda; la coerción del peralte con un
larguero que no lo admite y el id del catálogo; `SetBeamPeralte` todo o nada con largueros distintos; menos niveles; menos de dos tramos y sin
sistema; el ancho compartido contra el clásico (punto 3); las escrituras con «Todos» sobre fondos de forma distinta y la que no alcanza nada
(«no se escribió nada», no un éxito); `WithinScope`; `CopySafety`/`ReplaceSafety` (solo lo que dibuja o cuenta, copias, nulo = vacío); las
normalizaciones de la carga (fondo sin bahías propias, profundidades 0, elevación 0 y negativa, frente vacío rellenado y avisado, nulo lanza);
la tolerancia vinculada de solo lectura con su vínculo en los estados finales. `RackCadSelectiveWindowGapTests` (UI, nueva, 7), del modelo
de la vista y la ventana: «Editar aquí» otra vez sobre un borrador con cambios o escribiéndose no lo reemplaza, y sobre uno limpio lo reemplaza
y desengancha el viejo; «Descartar» con la base ilegible deja el borrador como está y lo dice; con una base que no recompone deja el editor de
solo lectura; confirmar el cierre mientras AutoCAD escribe no cierra; «Actualizar» con la puerta ocupada o de un editor ya olvidado no escribe;
cerrar la pestaña del rack desde la lista no pierde el borrador (y sigue vetando el cierre del dibujo); agregar y quitar tramos quita el de su
fila, con «(resto) ≈ n in». No se añadieron las «B» (textos, `Count == 0` de las lecturas, errores de `Recompose` distintos del peralte 0).

**5. O-D, las listas desplegables lejos del control.** Es de la ventana alojada, no de Selectivo: la paleta es un `PaletteSet` con `AddVisual`
(WPF en un `HwndSource` hijo de la ventana de AutoCAD), y una lista desplegable o una ayuda emergente de WPF es un `Popup`, otra ventana de
nivel superior que WPF coloca en coordenadas de pantalla convertidas con el PPP de su raíz. Con dos monitores de escala distinta, si la
ventana hija de la paleta y la del `Popup` no tienen el mismo contexto de PPP (AutoCAD aloja la paleta con el suyo), la conversión usa la
escala de un monitor para el otro y el `Popup` cae desplazado hacia el origen (a la izquierda), que es lo que se vio; que también le pase a la
ayuda emergente apunta a lo mismo. **Arreglo local hecho:** el selector de fondos de la matriz ya no es una lista (SF-09, como el draft), y el
de la pestaña Rack desaparece (SF-01). **No hecho:** un arreglo general (fijar el contexto de PPP del hilo al abrir el `Popup`, o recolocarlo
en `Opened`) no se puede comprobar sin AutoCAD en dos monitores de escala distinta; quedan con lista Poste, Larguero, Peralte, Cotas y Estilo
de cota. Para el smoke: ¿pasa con la paleta en el monitor principal (el de la escala del sistema)?, ¿y en el otro?

**Rendimiento** (una muestra por caso, Release, sonda temporal ya borrada; la de E1 era de `sprint\perf.md`): con 60 × 6 × 2 (720 celdas: 60
frentes × 6 niveles × 2 fondos; antes decía «1440 celdas», M-04 de la revisión del pulido) un
clic en la matriz pasa de ~9 a ~34 ms (14 ms son `AutomaticValues`: copia y resolución del diseño) y una edición de ~16 a ~60 ms, sin el BOM
de «Calculado», que solo se calcula con la pestaña Rack a la vista (~70 ms más). Con 10 × 6 × 1: clic ~4 ms, edición ~5 ms, BOM ~3 ms. Por
debajo de los 100 ms por gesto.

**Las pruebas:**
- Nuevas, UI: `RackCadSelectiveFidelityTests` (14: los grupos y textos de «Lo esencial», la escritura de todo el rack igual a «Todo» con
  «Todos los fondos» con la vista en el fondo 2, la tabla «Fondos» y su estado, «Calculado» contra el BOM, que sigue a cada recomposición,
  el alcance y «Destino», los grupos del inspector, los automáticos contra el resolvedor, «Altura por poste», las marcas de la leyenda, la tabla
  de postes, los nombres únicos, y la vista con WPF real: botones de fondos y de alcance, leyenda, chips, nombres de celdas, de «Fondos», de
  postes y de tramos por el `AutomationPeer`, sin errores de enlace), `SelectiveDraftEditorGapTests` (16) y `RackCadSelectiveWindowGapTests` (7).
- Cambiadas: `RackCadSelectiveEditorTests` (los títulos del inspector del draft, «Fondo 2» escribe por la tabla «Fondos» y el inspector, «Lo
  esencial» dice «Varía» con la vista en un fondo, el post y su peralte en «Poste y anotaciones», los rótulos de «Calculado», los botones de
  fondos en la vista) y `RackCadSelectiveEditorReviewTests` (la pista del claro cuando no está bloqueado).

**Comprobación en rojo:** doce piezas rotas a propósito, una a una, con las cuatro clases afectadas tras cada una
(`RackCadSelectiveFidelityTests`, `SelectiveDraftEditorGapTests`, `RackCadSelectiveEditorTests`, `RackCadSelectiveWindowGapTests`: 59
casos); en todas falla al menos una:
1. «Lo esencial» escribe en los fondos de la vista (sin `WithinScope(null)`): falla la de la escritura de todo el rack;
2. la fila de «Fondos» escribe la cabecera en todos los fondos: fallan la de la tabla y la de la pestaña Rack;
3. «Frente» del alcance no toma el frente: falla la del alcance;
4. el automático no vacía el largo manual: fallan tres (las dos del borrador y la del inspector);
5. «Largueros» cuenta las cabeceras del BOM: falla la de «Calculado»;
6. la altura manual marca un solo poste: falla la de «Altura por poste»;
7. lo escrito no es la base de «Modificada»: falla la de las marcas;
8. la celda se llama por su texto: falla la de la vista (nombres sin posición ni únicos);
9. la casilla de larguero a piso con su nombre fijo de antes: falla la de la vista;
10. «Quitar» repetido en cada tramo: fallan la de los nombres y la de la vista;
11. los botones de fondos no eligen el fondo y 12. los botones de alcance no eligen el alcance: falla la de la vista.

Cada archivo se restauró desde su copia, con fecha nueva y el mismo SHA-256 que antes de romperlo (comprobado por el script), se buscaron
restos y se recompiló la solución con `--no-incremental` antes de las suites completas. Las pruebas de los huecos (punto 4) fijan
comportamiento que ya existía; no tienen arreglo que romper, salvo `AutomaticValues` y `WithinScope`, que entran en 1 y 4.

**Los números** (sobre las fuentes restauradas, con la solución recompilada en Debug con `--no-incremental`):
- **UI completa:** 2654 correctas, 0 con error, 48 omitidas (2702): la base de `1f0bd3df`, 2617, más las 37 nuevas (14 + 16 + 7).
- **Core Full:** 19105 correctas, 0 con error, 403 omitidas (19508): la base, sin pruebas nuevas en Core.
- **Build Debug de la solución (Plugin incluido):** 0 errores (las advertencias MSB3277 de las referencias de AutoCAD y las de los
  analizadores de xunit ya estaban).

**Riesgos y preguntas:**
- La rama `…-selectivo-e2-seguridad` (C2) y `…-selectivo-e2-insertar` (A2) tocan la misma vista y el mismo modelo: al integrarlas habrá
  conflictos en `SelectiveEditorView.xaml` y `SelectiveEditorViewModel.cs` (sobre todo en el inspector y en las listas de `Rebuild`).
- SF-02 (la profundidad de cabecera con un fondo, fuera del draft) y SF-04 («≈» sobre un valor exacto) son para que el Owner las vea.
- «Distinta del resto» marca, con dos tarimas a partes iguales, la mitad de las celdas: es lo que hace el draft; si molesta en el smoke, se
  quita la marca y se deja la leyenda.
- O-D sigue abierto para las listas que quedan (punto 5).
- El smoke del Owner en AutoCAD: lo de la entrega 1, más todo lo de esta sección.

### Correcciones de la revisión del pulido (`review-polish.md` sobre `6f6c9161`)

**Rama:** la misma, sobre `6f6c9161`, sin commit. Sin AutoCAD y sin tocar Dinámico. Veredicto de la revisión: APTO TRAS CORRECCIONES
(0 B, 1 I, 4 M).

| Hallazgo | Estado | Qué se hizo |
|---|---|---|
| I-01 | CORREGIDO | «Calculado» cuenta con `SelectiveDraftEditor.BomCounts()`: el BOM de un sistema resuelto de una **copia** del último diseño válido, como `AutomaticValues`, y no de `System`, que comparte con `Design` los objetos de las cabeceras personalizadas. El BOM sigue imponiendo en el sitio la profundidad del fondo sobre lo que lee (`EffectiveCustomAt`, ARQ-43-10, diferido y sin tocar), pero ya sobre objetos propios. Leer «Calculado» no cambia `Design` ni el JSON de «Actualizar»; con la pestaña Rack a la vista, un cambio deshecho vuelve a «Limpio», y lo que se escribe no depende de la pestaña que se miró. Coste: una copia y una resolución más por lectura, solo con la pestaña Rack (unos 3 ms en Debug a 60 × 6 × 2, según P7 de la revisión) |
| M-02 | CORREGIDO | `SelectiveDraftEditor.WidthMasterFondo` es `SelectiveDepthLayout.MasterFondoIndex(System)`, la regla del resolvedor: el fondo con más frentes, el primero si empatan. La excepción del resolvedor, la columna vacía en el maestro, no se da en el borrador: todo frente tiene al menos un nivel y la carga rellena uno vacío. La «M» de la celda cuenta el claro manual, y el largo manual solo en el fondo maestro. Un largo manual de otro fondo no lleva «M», y el nombre accesible añade «con un largo manual que no cambia el dibujo». La pista del largo dice qué fondo manda (ver los textos debajo de la tabla). El resumen «Manual» («n celdas con valor manual») sigue contando lo que el borrador guarda |
| M-03 | CORREGIDO | La comparación de «Calculado» con el BOM ya no es tautológica: se compara contra un BOM hecho aparte. Ese BOM sale del JSON de `SnapshotForWrite()`, leído otra vez, resuelto con un resolvedor propio y sumado por categoría en la prueba. Cubre cuatro racks: dos fondos (7/22/6), la personalizada fuera de su regla (7/22/6), medio frente (9/22/6) y bota (7/22/6), además del de un fondo (3/8). Prueba nueva de que «Calculado» solo se anuncia y se cuenta con la pestaña Rack visible, con la vista WPF real y el contador `CalculatedBuilds`. La tabla «Fondos» escribe una profundidad de tarima nueva en cada fila (50 y 44), solo en su fondo, con la vista en el otro |
| M-04 | CORREGIDO | «60 × 6 × 2» son 720 celdas, no 1440 (corregido arriba, en «Rendimiento») |
| M-01 | SE QUEDA | «Frente», «Nivel» y «Todo» no se vuelven a expandir tras un cambio de forma o de vista. El draft aprobado hace lo mismo: guarda una selección explícita y `setFview` conserva el alcance (`Main.dc.html:1360-1391`). «Destino» dice la verdad: cuenta lo que de verdad se escribe. Para el smoke: en los tres casos de P4 de la revisión, el título y el botón marcado pueden engañar |

**Textos de la pista del largo (M-02).**
- En un fondo que no es el maestro: «En el fondo 1 manda el ancho del fondo 2 (el maestro, el de más frentes)». Con un largo manual sigue «: este largo no cambia el dibujo.».
- Con «Todos los fondos»: «Manda el mayor del frente en el fondo 2 (el maestro, el de más frentes)». Con un largo manual sigue «; en el fondo 1 el largo manual no cambia el dibujo.».
- En el maestro, o con un solo fondo: la pista de antes, «El mayor del frente fija su ancho.».

**El «Lista de materiales» del clásico tiene la misma mutación (anotada, NO arreglada).**
- **Por lectura:** `ShowBom_Click` (`src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs`) llama a `SelectiveBomBuilder.Build(lastSystem, catalog)`. `lastSystem` comparte con el estado del clásico (`SelectiveEditorState.ExtraFondoPostCabeceras`) los objetos de las cabeceras personalizadas, porque `BuildDesign` copia las listas y no las cabeceras, y el resolvedor tampoco las copia.
- **OBSERVADO** con una sonda temporal, ya borrada, que hizo esa misma llamada sin el diálogo modal sobre el rack de I-01: la personalizada del fondo 2 pasa de 42 a 34 en el estado del clásico (es el mismo objeto), y el diseño siguiente del clásico ya la lleva en 34. Eso afecta a Actualizar, Insertar y Guardar en biblioteca.
- La geometría y las cuentas no cambian, porque todo lector impone la regla. Lo que cambia es el JSON que se escribe después de abrir el BOM. Es ARQ-43-10, o un `Copy` antes del BOM del clásico: para la planeación.

**Pruebas.**
- **Nuevas, cuatro:**
  - `SelectiveDraftEditorGapTests.TheCountsOfTheBomAreThoseOfTheWrittenDesignAndCountingThemChangesNoDesign`;
  - `RackCadSelectiveFidelityTests.ReadingCalculatedChangesNothingOfTheDraftAndOnTheRackTabAChangeUndoneIsCleanAgain`, con la vista WPF real en Rack: «Holgura horizontal» 4 → 5 → 4 y «Mostrar tarimas» sí → no dejan «Limpio», y el JSON y la profundidad 42 no cambian;
  - `RackCadSelectiveFidelityTests.CalculatedIsOnlyCountedWhileTheRackTabIsShownAndItShowsWhatTheDraftIsNow`;
  - `RackCadSelectiveFidelityTests.AManualLengthInAFondoThatIsNotTheWidthMasterIsNoManualMarkAndItsHintSaysTheDrawingDoesNotUseIt`.
- **Cambiadas:** `TheCalculatedValuesAreThoseOfTheDraftAndTheCountsAreTheBom` y `TheTableOfTheFondosWritesEachFondoAndSaysItsState`.
- **Fixture nuevo:** `SelectiveWindowFixtures.TwoFondosWithACustomCabeceraOutOfItsRule`, el rack de `FewerFondosThatDropACustomCabeceraSayItToo`.
- **Seam nuevo, solo para pruebas:** `SelectiveEditorViewModel.CalculatedBuilds`, interno.

**Comprobación en rojo.** Siete piezas rotas, una a una, con script. Tras cada una se compiló y se corrieron cinco clases (`RackCadSelectiveFidelityTests`, `SelectiveDraftEditorGapTests`, `RackCadSelectiveEditorTests`, `RackCadSelectiveWindowGapTests` y `RackCadSelectiveEditorReviewTests`: 78 casos, 78 en verde antes de romper). En todas falla al menos una:
1. «Calculado» cuenta sobre el sistema del borrador: falla la de la pestaña Rack;
2. `BomCounts` cuenta sobre `System`: fallan la del borrador y la de la pestaña Rack;
3. la «M» con cualquier largo manual: falla la de M-02;
4. la pista dice siempre «El mayor del frente fija su ancho.»: falla la de M-02;
5. sin el `if (_selectedTab == 0)` de `Rebuild`: falla la nueva de M-03;
6. sin el aviso de `SelectedTab` al mostrar Rack: falla la nueva de M-03;
7. la fila de «Fondos» escribe la profundidad en todos los fondos: fallan la de la tabla y `TheScopeOfTheFondosShowsWhereItWritesAndWhatDiffersSaysVaries`.

Cada archivo se restauró desde su copia, con fecha nueva y el mismo SHA-256 que antes de romperlo (lo comprobó el script). Después se recompiló la solución con `--no-incremental`.

**Los números** (sobre las fuentes restauradas, solución recompilada en Debug con `--no-incremental`):
- **UI completa:** 2658 correctas, 0 con error, 48 omitidas (2706). Es la base de `6f6c9161` (2654) más las 4 nuevas.
- **Core Full:** 19105 correctas, 0 con error, 403 omitidas (19508). Es la base, sin pruebas nuevas en Core.
- **Build Debug de la solución (Plugin incluido):** 0 errores. Las advertencias MSB3277 de las referencias de AutoCAD y las de los analizadores de xunit ya estaban; ninguna sale de los archivos tocados.

## Dinámico: B-1 y los hallazgos de la revisión de su corrección (2026-11-25, noche)

Tercera tanda, encargo α, sobre `8fd26a14`, sin commit. Corrige el fallo B-1 del smoke de la entrega 1 de Selectivo y los hallazgos de la revisión independiente de `8fd26a14` (I-01, I-02 y M-01), más M-11 de Dinámico. Sin AutoCAD.

**1. B-1: «Insertar vista…» dejaba la vista nueva con el diseño anterior (bloqueante).**
- **Lo que se vio en el smoke:**
  - DIN 1 tenía la lateral y la planta con pesos de 1100.
  - Se cambió el peso de N1 a 1200 sin «Actualizar» y se pidió «Insertar vista…» → «Frontal - salida».
  - La ventana dijo «AutoCAD redibujó 2 vistas del rack con el borrador e insertó 1 vista nueva.».
  - La lateral y la planta quedaron con 1200; la frontal nueva, con 1100.
- **La causa exacta (OBSERVADO, leído en el código):**
  - Con inserción, `ApplyDynamicEdit` redibuja las vistas que existen con el borrador (`BuildDynamicPayload(design, id, name, …)`) y prepara la vista nueva con la cola ID18 (`RackViewBatchProducts.Dynamic(…, embed, project, authorized.AuthoredInput)`).
  - Como el rack ya existe, la cola usa `RackProductPreparer.PrepareExisting`. Este compone el sobre de la vista nueva con la identidad, el nombre y el diseño de su FUENTE (`RackProductPrepare.cs`: `source.Id, source.Name, source.Design`). La geometría, en cambio, sí sale del sistema del borrador: el puerto de resolución devuelve siempre el sistema editado.
  - `RackViewBatchProducts.Dynamic` le pasaba como fuente la vista elegida tal como estaba en el dibujo (`embed`). El diseño serializado del borrador, que ya calculaba, solo se usaba para un rack nuevo.
  - Por eso la vista nueva tenía la geometría del borrador, pero en su sobre el diseño y el nombre de antes. La regla de M-02 lo detectaba después, pero no lo evitaba.
- **La corrección:**
  - Una regla nueva y pura, `RackInsertedViewSource.Edited(source, rackId, rackName, serializedDesign)`, en `src/RackCad.Application/Views/Preparation/RackInsertedViewSource.cs`.
    - Devuelve el sobre elegido (con su vista, su corte, sus propiedades personalizadas y sus datos de extensión, I-11) con la identidad, el nombre y el diseño que la edición escribe en las vistas existentes.
    - Un rack nuevo no tiene fuente: `null` sigue siendo `null`.
  - `RackViewBatchProducts.Dynamic` se la pasa a la cola, con el mismo diseño serializado que ya usaba para un rack nuevo.
  - Es una llamada nueva a `RackEmbedComposer.Compose`, así que entra en el censo T-GRD-02 con su clasificación: compone con el sobre real (`source`) y no pasa `null`.
- **RACKEDITAR tenía el mismo fallo, y el arreglo también lo cubre.** «Insertar» del editor clásico de Dinámico, con cambios en la ventana, pasa por la misma mitad (`EditDynamic` → `ApplyDynamicEdit`). **Por tanto, `main` también tiene el fallo.**
- **Selectivo está bien (OBSERVADO por lectura; no se ejecutó en AutoCAD).**
  - La rama `!UpdateOnly` de `RackSelectivoCommands.EditSelective` reconcilia primero (`LinkedPropertyReconciler.Reconcile`).
  - Prepara la vista nueva con `SelectiveInsertPort.TryPrepare`, que compone su fuente con el diseño reconciliado (`curedSource = ComposeEnvelope(source, rackId, rackName, …, authoredJson)`).
  - Redibuja las hermanas con ese mismo `authoredJson`.
  - Nota: con «Insertar», las líneas de `DrawSelectiveViewFromAuthored` del final de `EditSelective` nunca se ejecutan, porque esa rama ya ha vuelto antes. No es un fallo.
- **Push Back, Cantilever y Cabecera tienen el mismo fallo (OBSERVADO; no se tocó).**
  - Su «Insertar» de RACKEDITAR redibuja las vistas existentes con el diseño de la ventana (`BuildPushBackPayload`, `BuildCantileverPayload` y `BuildCabeceraPayload`, con `design` o `config`).
  - Pero a su cola le pasa `embed` como fuente (`RackViewBatchProducts.PushBack`, `.Cantilever` y `.Header`).
  - También está en `main`. El arreglo sería la misma línea en cada fábrica, con su prueba. Lo decide el Owner.

**2. `RackSiblingDesigns`: I-01 e I-02 de la revisión.**
- **I-01: las definiciones de un xref no deben contar. Ya no contaban (OBSERVADO).**
  - `FindRackBlocks` lee con `RackBlockFinder.ScanEnvelopes`, que se salta toda definición con `IsFromExternalReference`.
  - Ese es el mismo dato que `RackSiblingScan` entrega a la membresía como `IsXrefDependent`. La membresía lo clasifica como `ReadOnly`, fuera de `MutableMembers`.
  - Las dos comparan además el Id sin distinguir mayúsculas y dejan fuera los sobres ilegibles. El conjunto de la puerta es, por tanto, el mismo que el de producción.
  - La revisión no había leído `ScanEnvelopes`. No hubo que cambiar el código. Una prueba fija la equivalencia: la membresía con una definición de xref y tres guardas de fuente (`FindRackBlocks`, `ScanEnvelopes` y `TraverseCore`).
  - **Pregunta abierta (INFERIDO, sin AutoCAD):** las definiciones de un rack que está DENTRO de un xref suelen aparecer en el dibujo anfitrión como símbolos dependientes («xref|bloque», `IsDependent`), no como `IsFromExternalReference`.
    - Si conservan su Xrecord, la producción (RACKEDITAR) y la puerta las cuentan a la vez como vistas del rack.
    - En `main` pasa lo mismo, y aquí no se cambió: cambiarlo solo en la puerta volvería a separar los dos conjuntos.
- **I-02: el Id se compara sin distinguir mayúsculas, como en `FindRackBlocks` (CORREGIDO).**
  - El comparador de AUTH-13 lee el Id de cada sobre distinguiendo mayúsculas (`AuthoredRawReader`). Ahora, una vista con el Id del rack en otras mayúsculas se le presenta con el Id pedido, en una copia en memoria: el sobre del dibujo no cambia.
  - Un Id de otro rack sigue sin coincidir. Selectivo no compara Ids, así que no cambia.
  - **D-14 (decisión de la sesión):** si las hermanas tienen Ids que solo difieren en mayúsculas:
    - la puerta abre el rack, y «Actualizar» las iguala (escribe el Id de la primera en todas);
    - «Insertar vista…» lo sigue rechazando por la regla de producción: `TryAuthorize` compara con el Id exacto de la vista elegida, y la cola falla con «no se preparo la cola ID18; no se modifico ningun bloque»;
    - es la opción cerrada ante la duda, y un «Actualizar» lo resuelve.

**3. M-01: la casilla «Guía de entrada» con la selección vacía (CORREGIDO).**
- Una selección sin celdas activas ya no puede escribir. Por ejemplo, la de un frente en blanco, que es donde queda la selección al ponerlo en blanco.
  - La casilla se desactiva: `IsEnabled` pasa a `CanEditSelectionGuide`, que exige al menos una celda activa en la selección.
  - El setter tampoco escribe: no recompone, no cambia el mensaje y no deja el borrador «Modificado».
- Antes, un clic llamaba a `SetGuide([], true)`, que devolvía `true` sin cambiar nada, y el borrador se recomponía. Como «Modificado» se calcula por diferencia, en la práctica no aparecía un falso «Modificado», pero sí se escribía y se cambiaba el mensaje.

**4. M-11 de Dinámico (CORREGIDO, como S-28 de Selectivo).**
- Cuando la orden toma la escritura (`RequestStarted`), la ventana avisa (`WriteTaken`) al editor de Dinámico de ese rack, o al del Dinámico nuevo.
- Desde ese momento el editor ya no dice «pulsa «Dejar de esperar»», sino el texto de S-28: «AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.».
- Ese texto vale para el motivo de «Actualizar», para el de «Insertar vista…» y para cualquier intento de editar mientras se escribe.
- La escritura siguiente vuelve a empezar sin tomar.
- **Al integrar con la rama de Selectivo:** en esa rama, el mismo `OnRequestStarted` de `RackCadWindowViewModel` avisa además al editor de Selectivo. Las dos líneas van juntas.

**5.** La sección «Dinámico, correcciones de la revisión de Selectivo» ya no dice «sin commit»: dice «(commit `8fd26a14`)», como en la rama de Selectivo.

**Las pruebas:**
- Core, `RackInsertedViewDesignTests` (nuevo, 4 casos):
  - **B-1 sin AutoCAD.** La cola ID18 prepara la vista nueva de un rack del dibujo (lateral y planta con 1100) con un borrador de 1200. Se compone como `RackViewBatchProducts.Dynamic`, con el comparador AUTH-13 y el preparador reales. La vista nueva lleva el diseño editado, igual al de las vistas redibujadas (1200), y `RackSiblingDesigns` dice que las tres coinciden.
  - **Lo que fue B-1.** Con la vista elegida tal como estaba, la vista nueva sale con 1100 y las hermanas no coinciden.
  - **La regla.** Da también el nombre y la identidad de la edición, conserva lo que lleva el sobre elegido y no cambia el sobre del dibujo; con `null` devuelve `null`.
  - **La guarda de fuente.**
    - `RackViewBatchProducts.Dynamic` pasa `RackInsertedViewSource.Edited(source, rackId, rackName, serialized)` con el mismo `serialized`.
    - `ApplyDynamicEdit` le pasa el mismo `design, id, name` con el que redibuja (los tres `BuildDynamicPayload`).
    - `EditDynamic` (RACKEDITAR) pasa por `ApplyDynamicEdit`.
- Core, `RackSiblingDesignsTests` (+2):
  - I-02: el rack pedido en otras mayúsculas, hermanas con mayúsculas distintas, el diseño y el nombre siguen decidiendo, otro Id no es este rack, el sobre no cambia, y lo mismo en Push Back;
  - I-01: la equivalencia de los dos conjuntos.
- Core, los dos censos de `RackEmbedComposer.Compose` cuentan la composición nueva con su clasificación: T-GRD-02 en `CustomPropertiesEnvelopeGuardTests` y T-ENV-12 en `CustomPropertiesEnvelopeTests` (pasa de 11 a 12 llamadas). La primera Core Full se paró en T-ENV-12, que no estaba en las clases focales; se corrigió y se repitió la suite completa.
- UI, `DynamicSafetyTabTests` (+2), M-01:
  - en el modelo de la vista, con un frente en blanco en el dibujo y su celda elegida: la casilla no se ofrece; escribirla no cambia nada (solo se refresca la casilla, sin mensaje y sin «Modificado», y el diseño y las celdas sin guía quedan iguales); de vuelta en una celda activa, escribe;
  - en la vista con WPF real: desactivada en la celda del frente en blanco, activa en una celda activa.
- UI, `RackCadDynamicEditorTests` (+1), M-11:
  - el texto antes y después de que la orden tome la escritura, en el motivo de «Actualizar», en el de «Insertar vista…» y al intentar editar;
  - la escritura siguiente;
  - el Dinámico nuevo.

**Comprobación en rojo:** ocho piezas rotas a propósito, una a una. Tras cada una se recompiló y se ejecutaron las clases afectadas: en Core, `RackInsertedViewDesignTests`, `RackSiblingDesignsTests` y `CustomPropertiesEnvelopeGuardTests` (40 casos); en UI, `DynamicSafetyTabTests`, `RackCadDynamicEditorTests` y `RackCadWindowTests` (108).
1. **B-1, la regla**, devolviendo la vista elegida tal como estaba (el comportamiento de antes): fallan la de B-1 y la de la regla.
2. **B-1, el Plugin**, con `RackViewBatchProducts.Dynamic` pasando `source` sin la regla: falla la guarda de la composición.
3. **I-02**, el Id comparado distinguiendo mayúsculas: falla la de I-02.
4. **I-01**, `ScanEnvelopes` sin saltarse las definiciones de xref: falla la de la equivalencia.
5. **M-01, el modelo de la vista**, sin la guarda de la selección vacía: falla la del modelo de la vista.
6. **M-01, la vista**, con la casilla activada por `CanEditGuide`, como antes: falla la de WPF real.
7. **M-11, la ventana**, sin avisar al editor de que la orden tomó la escritura: falla la de M-11.
8. **M-11, el editor**, diciendo siempre «Dejar de esperar»: falla la de M-11.

Cada archivo se restauró desde sus bytes originales con fecha nueva y se comprobó su SHA-256 contra el de antes de romperlo (los siete coinciden). Después se recompiló todo con `--no-incremental` antes de las suites completas.

**Los números**, sobre `8fd26a14` con estas correcciones, recompilado con `--no-incremental`:
- UI completa: 2548/0/48 (2596), que son las 2545 de la base más las 3 nuevas;
- Core Full: 19094/0/403 (19497), que son las 19088 de la base más las 6 nuevas;
- Plugin Debug: 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- El smoke en AutoCAD de B-1, por «Insertar vista…» y por RACKEDITAR «Insertar» con cambios. Hay que leer por COM que la vista nueva lleva el peso del borrador y que la ventana vuelve a abrir el rack.
- El mismo fallo en Push Back, Cantilever y Cabecera, que también está en `main`.
- La pregunta de los símbolos dependientes de un xref (I-01).

## Selectivo en la ventana (línea D), E2: «Extremos y seguridad», el modelo (tercera tanda, C2)

**Rama:** local `architecture/workspace-persistente-rackcad-selectivo-e2-seguridad`, sobre `684ca5b2` (la parte C del sprint del 2026-11-25, que dejó un modelo parcial: T/P por selección y una tabla con solo el desviador y la bota). C2 completa el modelo puro. Sin XAML, sin AutoCAD, sin commit (la sesión revisa y commitea).

**Lo que se construyó:**
- **La tabla «Seguridad por poste» contada como el BOM** (`SelectivePostSafetySummary`, nuevo, en `src/RackCad.Application/Systems/Selective/`), con el patrón de `DynamicPostSafetySummary`: construye las MISMAS proyecciones que cuenta `SelectiveBomBuilder` y reparte cada pieza a su poste:
  - **Botas y Protector lateral:** de la planta que cuenta el BOM (`SelectiveBomBuilder.CountingPlanta`), al poste más cercano. Un protector lateral reemplaza las botas de su poste, como en el dibujo (antes, la tabla de la parte C decía el lado de la bota también ahí).
  - **El lado de la bota** sale de la resolución única de caras (`SelectiveSafetySelection.BootFacesAt`, la que consume la planta por `SelectiveSafetyEnds.BootCopiesForPost`), y solo donde la planta pone botas.
  - **Desviadores (niveles):** los puntos habilitados de `SelectiveDesviadorPlan` (su cantidad física es la línea del BOM). Cada nivel una vez aunque lleve desviador en las dos caras; la cantidad cuenta las dos piezas.
  - **Un poste intermedio de medio frente** (el plan del desviador lo trata como columna real) tiene su propia fila, entre los dos postes de su frente: la columna sigue sumando lo del BOM. La planta y los cortes solo ponen botas, protectores y separadores en los postes de cabecera.
  - **Separadores entre fondos:** los del corte lateral de cada poste (`SelectiveBomBuilder.CountingCortes`), que es de donde los cuenta el BOM.
  - `SelectiveBomBuilder` gana esas dos proyecciones `internal` y las usa él mismo: el BOM no cambia y la tabla no duplica ninguna regla.
- **T y P por frente × larguero** (`SelectiveSafetyView`, reescrito sobre el de la parte C): la traducción de los niveles de la matriz con el «Larguero a piso» de CADA fondo, leída del sistema resuelto (C2-01), la lectura y la escritura sobre el alcance del selector de fondos (C2-02), «En todas las celdas» / «En ninguna» (`WriteAll`, C2-07), y `CellsOn`/`AllOff` para la tarjeta de cada familia. Una escritura devuelve `SelectiveSafetyEdit`: lo escrito, lo que no se escribió y por qué (sin larguero, fondos que no coinciden, celda que no existe) y el aviso.
- **El modelo de la vista** (`SelectiveEditorViewModel.Safety.cs`): `PostSafety` con las columnas del draft, la fila `PostSafetyTotal` y `SafetySummary` («4 botas · 1 protector · 12 desviadores · 8 separadores, en 4 postes.»; sin separadores con un fondo, como el draft). La tabla se cuenta una vez por recomposición (se guarda junto al sistema que la produjo). `SetTopeEverywhere` / `SetParrillaEverywhere`. El aviso de cada escritura de T/P es el suyo, no el genérico de «no existe en todos los fondos».
- **Ediciones a archivos compartidos** (pequeñas):
  - `src/RackCad.Application/Systems/Selective/SelectiveBomBuilder.cs`: `CountingPlanta` y `CountingCortes` (`internal`), y `AddSafetyComponents` / `AddSeparadorComponents` las llaman en lugar de construir la planta y los cortes en línea. Sin cambio de comportamiento.
  - `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: una línea en `RebuildSelection` (`OnSafetyChanged()`), para que la selección y una recomposición refresquen T/P y la tabla.

**Los [VERIFICAR] de la spec (`spec-seguridad.md`), resueltos contra el código:**
1. **Numeración de niveles.** Las celdas apagadas de tope y parrilla (`TopeOffCells`, `ParrillaOffCells`) cuentan los niveles de LARGUERO resueltos, no el piso: el resolver (`SelectiveGeometryResolver.ResolveFondo`) no crea nivel para la tarima de piso de un frente sin «Larguero a piso», el diálogo clásico llama a sus filas «Larg. 1», «Larg. 2»… (`SafetyTopeGridWindow`), y `SelectiveTopePlan` y `SelectiveParrillaPlan` leen `bay.Levels[lvl]` de cada fondo. En la matriz de la ventana N1 es la fila de piso: N1 de un frente con «Larguero a piso» es el larguero 1; sin él, N1 no es celda y N2 es el larguero 1. Los niveles del desviador son otra cosa: filas de carga por poste (`SelectiveDesviadorPlan`), con N1 en el primer troquel aunque no haya larguero a piso («Nivel 1 (piso)» del diálogo clásico), así que su «N1–N3» coincide con la numeración de la matriz.
2. **Qué es «Izquierda» tras espejar.** «Izquierda» es la cara CERCANA: el extremo bajo del eje de profundidad propio del rack (`SelectiveDepthLayout.Offsets` pone en 0 el poste delantero del fondo 1); «Derecha», la cara lejana (el poste trasero del último fondo). La planta pone la copia «Izquierda» en el poste delantero y la refleja al trasero para «Derecha» (`SelectivePlantaBuilder`); en la frontal, «Derecha» es la copia espejada en su sitio. Nada lo lee en coordenadas del mundo, y esta rama no tiene una orden de espejo para Selectivo (RACKMIRROR es I-52, no integrada): espejar las vistas con MIRROR de AutoCAD no cambia el diseño, así que «Izquierda» sigue siendo la cara del fondo 1 aunque en pantalla quede a la derecha. Es lo que pide la decisión 13 (el nombre va con el rack, no con la carga ni con la pantalla). Prueba: la bota «Izquierda» de un poste está en la misma X de planta que la copia delantera de un poste con las dos, y la «Derecha» en la trasera.
3. **El separador entre fondos.** No es una familia del catálogo de seguridad: es el perfil separador `DynamicRackDefaults.SeparatorCatalogId` (`SpacerProfiles`), con rol `HeaderBlockRole.Separator`, automático en cada hueco entre dos fondos contiguos que llegan a un poste (`SelectiveSeparadorPlan.GapsAt`), apilado en vertical cada ~100 in (`SeparatorLevelCalculator`, `SelectiveLateralBuilder.SeparatorMaxSpacing`). El BOM lo cuenta en los cortes laterales (la planta lo colapsa a una línea por frente), agrupado por LONGITUD (el hueco). Su longitud se cambia en «Rack» › «Fondos»; no se edita en «Editar seguridad…».

**`RestoreLastValid` y la seguridad:** comprobado. `RestoreLastValid` vuelve a cargar el último diseño válido (`LoadState`), y esa carga reemplaza la seguridad por la del diseño (`ReplaceSafety(design.SafetySelections)`); el diseño válido lleva la seguridad que tenía el borrador al recomponer. Prueba: T apagada en una celda y recompuesta; `ReplaceSafety` con otra familia; una recomposición que no corre (peralte de poste inválido); `RestoreLastValid` devuelve exactamente la seguridad anterior (comparada por su JSON), y la siguiente recomposición la lleva al sistema. Sin cambio de código.

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| C2-01 | **El supuesto «larguero a piso del fondo 1» se descarta.** Un nivel de la matriz se traduce a su larguero con el «Larguero a piso» de SU fondo, leído del sistema resuelto (`FloorPalletCount` de la bahía, la regla del resolver y del plan del desviador) | El BOM aplica cada celda apagada al mismo larguero de ese frente en cada fondo (la parrilla, en todos; el tope, en los fondos de sus puntos). Con el supuesto, en «Fondo 2» la marca de un frente cuyo fondo 2 no tiene larguero a piso se veía un nivel desplazada y no cuadraba con el BOM. Prueba: lo que muestra la matriz fondo por fondo × 2 tarimas = las parrillas del BOM, antes y después de escribir |
| C2-02 | T y P son del larguero en todo el rack (decisión 5). Con «Todos los fondos», si los fondos de la vista que tienen esa fila no coinciden en «Larguero a piso» de ese frente, esa fila es un larguero distinto en cada fondo: **no se escribe** y el aviso pide elegir un fondo en el selector. Con un fondo elegido se escribe su larguero, y si otro fondo no tiene el mismo «Larguero a piso» el aviso dice que allí la marca se ve en otro nivel | Escribir los dos largueros cambiaría celdas que no se eligieron; no escribir nada sin decirlo dejaría el clic sin efecto. La lectura no cambia: «varía» cuando los fondos difieren. **Para revisar con el Owner** si prefiere otra regla |
| C2-03 | El lado de la bota se lee de la resolución única de caras (`BootFacesAt`) y solo donde la planta pone botas | Es la pertenencia que consumen la planta, los cortes y el BOM; donde un protector lateral reemplaza las botas, la tabla no dice un lado que no se dibuja |
| C2-04 | Un poste intermedio de medio frente tiene fila propia, «F2 · intermedio 1», entre los postes de su frente | El plan del desviador le pone desviadores y el BOM los cuenta; sin fila, la columna no sumaría o los atribuiría al poste equivocado. El draft no nombra esos postes: **texto provisional** |
| C2-05 | «Desviadores (niveles)»: los niveles en tramos («N1–N3», «N1, N3–N4») y entre paréntesis las piezas, contando las dos caras | El draft dice «N1–Nk (k)»; con celdas apagadas los niveles no son siempre seguidos, y la cantidad tiene que ser la del BOM |
| C2-06 | «Protector lateral»: «Sí, extremo» en el primer y el último poste, «Sí» en otro, y «(n)» con más de una pieza (guía a los dos lados) | Texto del draft («Sí, extremo»), que solo pensaba en los extremos; el clásico permite protectores en cualquier poste |
| C2-07 | «En ninguna» deja la variante elegida con todas sus celdas apagadas (no dibuja ni cuenta nada), y «En todas las celdas» la recupera. El «Aceptar» del diálogo clásico, con todo apagado, quita la variante. **Diferencia declarada**: el dibujo y el BOM son los mismos | Si la ventana quitara la variante, volver a poner T exigiría el diálogo |
| C2-08 | La bota en las dos caras dice «Izquierda y Derecha», en UNA constante (`SelectiveSafetyView.BothSides`), **provisional**: pregunta abierta al Owner (el clásico dice «Ambas») | Encargo de la tercera tanda |
| C2-09 | La tabla lee la última recomposición válida (lo que escribiría «Actualizar») y se cuenta una vez por sistema | El BOM cuenta el sistema resuelto; contar la planta y los cortes en cada lectura de la vista sería caro en un rack grande |

**Las pruebas:**
- Core, `tests/RackCad.Tests/SelectivePostSafetySummaryTests.cs` (nuevo, 12): la equivalencia con el BOM por familia (botas, protector lateral, desviador, separadores) en cinco racks (un fondo, dos fondos, dos fondos en esquina, medio frente, dos fondos con medio frente); el protector lateral que reemplaza las botas; el lado de la bota como cara física (con las X de la planta); los desviadores en las dos caras; los niveles del desviador sin larguero a piso; el poste intermedio; los separadores por corte y el poste al que no llega un fondo; sin sistema o sin catálogo.
- UI, `tests/RackCad.UI.Tests/Workspace/SelectiveSafetyViewTests.cs` (de 5 a 13): la rejilla del clásico; la traducción de cada fondo contra los niveles del resolver; fondos que no coinciden en «Larguero a piso»: lo que muestra la matriz = lo que cuenta el BOM, antes y después de escribir desde «Fondo 2»; desde «Todos», la fila ambigua no se escribe y el resto sí, y «varía»; T apagada = topes del BOM; P y la variante sin elegir; «En ninguna» / «En todas las celdas»; `RestoreLastValid` con la seguridad; la tabla = BOM; la constante provisional; los textos del modelo de la vista (columnas, «Total», resumen, recuento tras recomponer); los niveles en tramos y el poste intermedio; el editor de la ventana con «Editar seguridad…».
- **Comprobación en rojo:** siete piezas rotas a propósito, una a una, con su clase de pruebas tras cada una (script y salida en el scratchpad de la sesión, `hs65\selectivo\c2-red\`):
  1. el lado de la bota sin mirar si la planta pone botas: falla la del protector lateral que las reemplaza;
  2. los niveles del desviador repetidos con dos caras: falla la de las dos caras;
  3. el poste intermedio pegado al poste de cabecera más cercano: fallan las dos de medio frente de la equivalencia y la del poste intermedio;
  4. la traducción con el «Larguero a piso» del fondo 1 (el supuesto descartado): fallan la de la traducción contra el resolver, la de «lo que muestra la matriz = lo que cuenta el BOM» y la de «Todos»;
  5. escribir desde «Todos» aunque los fondos no coincidan: falla la de «Todos»;
  6. `LoadState` sin `ReplaceSafety` (la seguridad fuera de `RestoreLastValid`): falla la de `RestoreLastValid`, y con ella las dos del modelo de la vista que abren un rack con seguridad (la carga usa el mismo camino);
  7. la tabla sin recontar tras una recomposición: falla la del modelo de la vista.

  Cada archivo se restauró de su copia con fecha nueva y se comparó byte a byte con ella; `SelectiveDraftEditor.cs` queda sin cambios. Las suites de abajo corrieron después, con `--no-incremental`.
- **Focales UI:** `SelectiveSafetyViewTests` 13/13; `RackCadSelectiveEditorTests`, `SelectiveDraftEditorEquivalenceTests` y `RackCadSelectiveWindowTests` 53/53; con `SelectiveTopeSide*`, 106/106.
- **Core Full:** 19100 correctas, 0 con error, 403 omitidas (19503): la base de E1-B (19088) más las 12 nuevas.
- **Build Debug del Plugin:** 0 errores.

**Lo que queda abierto:**
- **La pantalla** (fuera de C2): la pestaña «Extremos y seguridad» en `SelectiveEditorView.xaml`, las marcas T/P en las celdas de «Matriz» (el modelo de la celda, `SelectiveCellItem`, aún no las lleva: es una edición del modelo de la vista compartido), la sección «Seguridad de la celda» del inspector, los textos de las tarjetas de familia (`{{fm.summary}}`, `{{e.safNote}}`) y el puerto modal de «Editar seguridad…» (`SafetyEditor` por `IRackCadModalPort`, D-09).
- **Preguntas al Owner:** C2-02 (qué hace «Todos» con fondos que no coinciden en «Larguero a piso»), C2-08 («Izquierda y Derecha» o «Ambas») y el texto de C2-04.
- Los conteos de piezas de las tarjetas (topes y parrillas) deben salir del BOM (`SelectiveBomBuilder.Build`, categorías `Tope` y `Parrilla`), como en la prueba; no hay regla propia en la ventana.

## Selectivo en la ventana (línea D), entrega 2: «Insertar vista…» y un Selectivo nuevo, del lado de AutoCAD

**Rama:** `architecture/workspace-persistente-rackcad-selectivo-e2-insertar` (local), desde `dc23b820`. La parte A del sprint (`31d3f1bf`) hizo `RACKSELECTIVOINSERTAR`; la tercera tanda (A2, sin commit) hizo `RACKSELECTIVONUEVO` y revisó la extracción de la parte A. Sin pantalla y sin AutoCAD.

**Lo que se construyó:**
- **Parte A (`31d3f1bf`), revisada en A2:**
  - `SelectiveDoorWrite` lleva `InsertViews` y `NewRack`, como `RackDoorWrite` de Dinámico.
  - `SelectiveDoorInsertion` (Application, archivo nuevo): las vistas que ofrece «Insertar vista…» en el orden de «Insertar varias…» del clásico (una frontal por fondo, un lateral por poste con corte, la planta), las elegidas que el rack puede dibujar (`DrawableViews`), las respuestas (las de Dinámico) y `RefusalOf`.
  - `RACKSELECTIVOINSERTAR` relee el rack y el registro, planea como `RACKSELECTIVOAPLICAR` (`PlanSelectiveApply`), deja las vistas que se pueden dibujar y corre la mitad «Insertar» de RACKEDITAR.
  - **La extracción `InsertSelectiveViews`** (la rama `!UpdateOnly` de `EditSelective`): comparada línea a línea con `dc23b820`, sin sangría, el cuerpo es el mismo. Cambian tres cosas:
    1. repite al principio las cuatro líneas de `EditSelective` que leen el diseño, el sistema y el nombre (`DesignToInsert` y `SystemToInsert` dan los mismos objetos, así que `system.Name = name` pone el mismo valor otra vez);
    2. las vistas llegan como argumento, y RACKEDITAR pasa `window.InsertionRequest.Views`, lo mismo que leía la rama;
    3. cada `return;` antes de colocar es `return -1;` (cinco), y el final devuelve las vistas colocadas.
  - RACKEDITAR ignora lo que devuelve y sale con `return;` como antes; el aviso de unidades (I-05) sigue antes, en `EditSelective`. **RACKEDITAR no cambia.**
- **A2, el Selectivo nuevo de la ventana** (como `RACKDINAMICONUEVO`):
  - **La puerta** (`RackCadRackDoorPort`): un rack nuevo (`NewRack`) va primero a su orden, `RACKSELECTIVONUEVO`, en su propio hueco (`_pendingSelectiveNew`, `TakePendingSelectiveNew`), que «Dejar de esperar» (`Abandon`) suelta. Se quitó el rechazo de la parte A («El Selectivo nuevo de la ventana todavía no se puede insertar.»). Sin vistas o sin identidad, la puerta no da la orden y dice el motivo de la regla (`RefusalOf`). La copia de la petición (identidad recortada, estados finales y vistas propias) es una sola función para las tres órdenes de escritura.
  - **La regla pura** `SelectiveDoorInsertion.PlanNewRack` (Application) decide antes del despacho. No inserta nada si:
    - no hay borrador (diseño y sistema), o la petición no es de un rack nuevo;
    - no hay vistas o identidad (`RefusalOf`);
    - ya hay en el dibujo un rack con esa identidad, con el texto de Dinámico (A2-03);
    - una holgura está vinculada a una variable (A2-01);
    - ninguna vista elegida se puede dibujar (A2-02).

    Si no, da las vistas a insertar en su orden y sin repetir.
  - **La orden** `RACKSELECTIVONUEVO` (`RackCadRackDoorCommands.cs`): toma la petición, avisa que la tomó (`Started`), comprueba el dibujo, planea con la regla y pasa por `RackMenuCommands.DispatchInsertion`, el despacho del clásico abierto desde «Nuevo» (RACKNUEVO), con el nombre automático de I-60 si el borrador no tiene nombre. La respuesta (`AnswerForNewRack`, la de Dinámico) cuenta las vistas de esa identidad que quedaron en el dibujo y da el nombre del sobre.
  - **La petición** `SelectiveDraftOutcome.NewRackRequest` (UI, en `ISelectiveEditOutcome.cs`): la `SelectiveInsertionRequest` que arma el clásico en «Insertar varias…» de un rack nuevo, con la identidad de la ventana, su nombre y las vistas en su orden.
  - **Los censos**, cada uno con su nota:
    - las órdenes pasan de 50 a 51 (`SelectiveEditorOpenTests`);
    - `A14SelectiveDoorCommands` añade `RACKSELECTIVONUEVO` (`CustomPropertiesCommandGuardTests`);
    - la guarda de capas de `RackCadWindowListTests` añade `InsertSelectiveViews` y `DispatchInsertion` (A2-05).

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| A2-01 | `RACKSELECTIVONUEVO` no inserta un rack nuevo con una holgura vinculada a una variable, y lo dice | El despacho de «Nuevo» escribe el documento solo con el diseño (`SelectivePalletDesignDocument.From`), como el clásico abierto desde RACKNUEVO, que no ofrece vínculos: el vínculo se perdería sin aviso. Hoy la ventana no lo produce (decisión 12: un borrador nuevo lleva holguras literales); es una defensa |
| A2-02 | Las vistas a insertar son las elegidas que el borrador puede dibujar, con los mismos hechos que usa el despacho (`SelectiveViewAvailabilityFacts` con el catálogo del Plugin). Una vista que ya no se puede dibujar se deja fuera sin aviso, como en `RACKSELECTIVOINSERTAR` de la parte A; si no queda ninguna, no se inserta nada | El despacho no prepara una vista que no puede dibujar, y la respuesta ya dice cuántas vistas se colocaron |
| A2-03 | Antes de insertar, la orden comprueba que no hay en el dibujo un rack con la identidad que dio la ventana, con el mismo texto que `RACKDINAMICONUEVO` | El rack nuevo sería otra vista de un rack que ya existe |
| A2-04 | La orden no avisa de las unidades: lo hace el despacho de «Nuevo» (I-05), una sola vez | Avisar dos veces sería ruido (el mismo motivo que da `DispatchInsertion`) |
| A2-05 | La guarda de capas añade `InsertSelectiveViews` y `DispatchInsertion`: solo las órdenes de la puerta los llaman, nunca los archivos de la paleta | Colocan vistas con el jig y bloquean el documento (punto 3 del censo de E2) |
| A2-06 | `RACKSELECTIVONUEVO` no compara las vistas hermanas (M-02) | Un rack nuevo no tiene |

**Las pruebas:**
- `SelectiveDoorInsertionTests` (Core; 12 de la parte A más 8 nuevas):
  - `PlanNewRack`: las vistas que se pueden dibujar, en orden y sin repetir; cada rechazo con su texto; la identidad que ya está en el dibujo, con el texto de `RACKDINAMICONUEVO` (se busca en la fuente); la holgura vinculada rechazada y las literales aceptadas;
  - el cableado, sobre la fuente:
    - `RACKSELECTIVONUEVO` en su orden (petición, `Started`, otro dibujo, regla, despacho con las vistas del plan, respuesta), sin aviso de unidades propio;
    - `RACKSELECTIVOINSERTAR` en su orden (la parte A no tenía prueba de cableado);
    - RACKEDITAR inserta por la mitad extraída y sale: los pasos de la antigua rama, en su orden, y sus cinco `return -1;`;
    - la puerta manda el rack nuevo a su orden antes que «Insertar vista…», con la regla, y suelta su hueco.
- `SelectiveNewRackRequestTests` (UI, 4, archivo nuevo): la petición del Selectivo nuevo es la del clásico abierto desde «Nuevo» (la misma clase, vistas, vista antigua, identidad, nombre, diseño y sistema), con la primera vista frontal, lateral o planta; guarda su copia de las vistas; sin vistas no hay petición.
- **Comprobación en rojo:** diez piezas rotas a propósito, una a una, con su prueba tras cada una; cada una falló:
  1. sin comprobar la identidad del dibujo;
  2. sin rechazar la holgura vinculada;
  3. las vistas elegidas sin filtrar (fallan dos);
  4. «Dejar de esperar» sin soltar el hueco del rack nuevo;
  5. el rechazo de la parte A conservado en la puerta;
  6. la orden con las vistas de la petición en lugar de las del plan;
  7. un archivo de la paleta que llama a `DispatchInsertion` (la guarda de capas);
  8. la orden sin registrar (fallan los dos censos y la prueba del cableado);
  9. RACKEDITAR sin `return;` tras la inserción (seguiría a «Actualizar»);
  10. la petición de UI sin sus vistas (fallan las cuatro).

  Cada archivo se restauró de su copia con fecha nueva, para que la compilación siguiente lo recompile, y se comprobó que no quedan restos.
- **Focales:**
  - Core: `SelectiveDoorInsertionTests`, `SelectiveRackDoorTests`, `RackCadWindowListTests`, `CustomPropertiesCommandGuardTests`, `SelectiveEditorOpenTests`, `LinkedPropertyViewPayloadTests`, `LegacyViewPayloadCompositionCharacterizationTests`, `RackUnitsGuardSourceTests` e `I60BatchCreationNamingTests`: 222 de 222 (214 en `31d3f1bf`);
  - UI: `SelectiveNewRackRequestTests`, `RackInsertionRequestTests`, `EditorViewBatchRequestTests`, `RackEditorSessionTests`, `RackCadSelectiveEditorTests`, `SelectiveDraftEditorEquivalenceTests` y las guardas: 127 de 127, sobre las fuentes restauradas.
- **Un fallo de la parte A que encontró Core Full:** `SelectiveDoorInsertionTests.cs` declaraba el namespace `RackCad.Tests.Workspace`, y la guarda de I-23 (`NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`) exige `RackCad.Tests`. La primera pasada de Core Full dio 19107 correctas, 1 con error (esa guarda) y 403 omitidas, y ese fallo es el rojo. Con el namespace `RackCad.Tests`, la guarda pasa.
- **Core Full:** 19108 correctas, 0 con error, 403 omitidas (19511). Es la base de E1, 19088, más las 12 de la parte A y las 8 de A2.
- **Build Debug del Plugin:** 0 errores, sobre las fuentes restauradas (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- **La pantalla,** según `sprint/spec-insertar-nuevo.md`:
  - «Insertar vista…» en el editor de Selectivo, con `SelectiveDoorInsertion.ViewOptions` en el diálogo del clásico (`RackSystemKind.SelectiveRack`);
  - «Selectivo en esta ventana» en «Nuevo» (`SelectiveDraftEditor.CreateNew`) y `MarkInserted`;
  - el cierre que nombra «un Selectivo nuevo» y D-10.
- **Para integrar:** cuando llegue M-02 (`…-selectivo-m02`), `RACKSELECTIVOINSERTAR` necesita la comparación de las vistas hermanas, como `RACKSELECTIVOAPLICAR`, porque escribe el borrador en todas las vistas antes de insertar.
- **Declarado de la parte A:** en `RACKSELECTIVOINSERTAR`, «redibujadas» son las vistas que había menos las borradas, no las que redibujó la mitad de dibujo, que no lo devuelve. Si no hacía falta redibujar (el borrador ya es el dibujo), la respuesta dice igual que las redibujó; el borrador queda «Limpio», que es cierto.
- **Sin AutoCAD:** `RACKSELECTIVONUEVO` y `RACKSELECTIVOINSERTAR` esperan el smoke: la colocación con el jig, el nombre automático, que un «U» deshaga la inserción y la cancelación de la colocación.

## Selectivo en la ventana (línea D), entrega 2, «Restaurar layout…», pantalla (P2)

**Rama:** local `architecture/workspace-persistente-rackcad-selectivo-e2-restaurar`, avanzada a `938823b2` (la rama de Selectivo con la operación `SelectiveLayoutRestore` y `RestoreOptions`/`RestoreLayout()` ya integrados, `ffbcc485`). Sin commit, sin push, sin AutoCAD. Spec: `sprint/spec-restaurar.md`; decisión 8 del Owner.

**Lo que se construyó:**
- **El botón** «Restaurar layout…» en la cabecera del editor, entre «Descartar» y «Cerrar el editor», como Dinámico (`SecondaryButtonStyle`, nombre de automatización «Restaurar el layout estándar», habilitado con `CanEdit`). Sin menú «Más» (E1 no lo tiene).
- **El panel en línea** (`RestorePanel`) bajo el mensaje del editor, donde Dinámico pone su confirmación, con los textos de la spec §4:
  - «¿Restaurar el layout?» y «Vuelve a la geometría que sale de las tarimas en {todos los fondos | el rack}. Marca lo que quieres restaurar:»;
  - las seis casillas de la decisión 8 con su recuento, «Largo de larguero (3)»; con nada que quitar, «Largo de larguero (ninguno)», deshabilitada y marcada (no cuenta);
  - **todas marcadas al abrir**, también al reabrir (no recuerdan la elección; spec §4);
  - «Ver lo que se pierde», plegado al abrir: una línea por valor, solo de las casillas marcadas, con los textos y el orden de `lossList` del draft;
  - «Va al borrador: el dibujo no cambia hasta «Actualizar».»;
  - «Restaurar lo marcado» (estilo de peligro: el secundario con el token de error, porque el shell no tiene botón de peligro), deshabilitado si ninguna casilla marcada tiene algo; «Cancelar» (nombre de automatización «Cancelar la restauración del layout», para no repetir el «Cancelar» de la pregunta de cierre).
- **Sin nada manual** el panel no se abre y el editor dice «No hay nada manual que quitar: el layout ya es el que sale de las tarimas.» (draft `:1561`).
- **El aviso** (spec §6), en el mensaje del editor: «Layout restaurado en el borrador: 7 valores manuales quitados (2 largos de larguero, 1 claro, …). «Descartar» vuelve a lo último que se leyó o se escribió.», en singular con un valor, y «Se conservan: 1 cabecera personalizada.» si quedaron casillas desmarcadas con valores. El borrador pasa a «Modificado», las celdas pierden la «M» y la selección se conserva.
- **El panel y la pregunta de cierre se cierran entre sí** (draft `:1562`). Mientras AutoCAD escribe, el panel se cierra y no se abre. Abierto, sigue al borrador: cada recomposición recuenta casillas y líneas y conserva lo que el usuario marcó; si ya no queda nada que quitar, se cierra.
- **La frontera de `CommitPendingEditors`** (spec §5): antes de abrir y de restaurar, la vista confirma el texto del campo con el foco (y el nombre). Si un campo tiene un valor escrito y rechazado (su error a la vista), no se restaura y el editor lo nombra: «No se puede restaurar el layout: «Alto de tarima» tiene un valor que no se aplicó (…). Corrígelo o vuelve a escribir el que tenía.» (texto provisional), porque la recomposición devolvería a la caja el valor del borrador sin aviso.
- **Application** (`SelectiveLayoutRestore.cs`, archivo de esta parte):
  - `SelectiveLayoutRestoreBox` (las seis casillas) y `Options.Includes`;
  - `Losses(editor)`, las líneas de «Ver lo que se pierde»;
  - `Result.Count(box)`, `Result.Breakdown` y `Result.Kept` (lo que conservan las casillas desmarcadas);
  - `Result.Notice` pasa al texto exacto de la spec §6. Reemplaza el provisional «Restaurado: …» de la parte del modelo: dos asertos de `SelectiveLayoutRestoreTests` se actualizaron a ese texto.
- **El modelo de la vista** en su archivo parcial, `SelectiveEditorViewModel.Restore.cs`: `SelectiveRestoreBoxItem`, `IsRestoreOpen`, `RestoreBoxes`, `RestoreIntro`, `RestoreLosses`, `IsRestoreLossesExpanded`, `RestorePeralteNote`, `CanConfirmRestore`, `OpenRestore`/`ConfirmRestore`/`CancelRestore`. `RestoreOptions` y `RestoreLayout()` se mudaron ahí desde `SelectiveEditorViewModel.cs`, con la misma firma.

**Supuestos provisionales** (preguntas abiertas al Owner, aislados):

| Id | Supuesto | Dónde se cambia |
|---|---|---|
| R-01 | **Alcance = todo el rack** (como el draft aprobado), no el selector de fondos: con «Fondo 2» en la matriz restaura igual todos los fondos, y los peraltes propios se restauran siempre. La operación trabaja sobre el alcance del borrador, así que se llama con `WithinScope(null, …)` (como hizo el pulido) | Un solo valor: `SelectiveEditorViewModel.RestoreFollowsFondoView` (`false`). Con `true` sigue el selector: «el fondo n» en el texto, y la casilla de peraltes propios desmarcada y deshabilitada con «Los peraltes propios son de todo el rack: elige «Todos los fondos» para restaurarlos.» (regla I-43). Las dos ramas tienen prueba |
| R-02 | Las casillas no recuerdan la elección: cada apertura vuelve a todo marcado | `OpenRestore` (`everyBoxChecked: true`) |
| R-03 | «Se conservan:» dice las categorías desmarcadas con su recuento («1 claro, 1 cabecera personalizada»), con el mismo formato del desglose | `SelectiveLayoutRestoreResult.Notice` |
| R-04 | El texto del valor rechazado que detiene la restauración (la spec no lo da) | `PendingFieldError` |

**Diferencias con el clásico y con Dinámico** (spec §8): el clásico no tiene orden global (se restaura pieza a pieza); la ventana sí, con casillas. Peralte propio y cabecera personalizada son casillas separadas (en el clásico «Restablecer poste» las une); con las dos marcadas el resultado es el del clásico, comparado byte a byte por `EveryBoxCheckedIsWhatTheClassicEditorBuildsForThePlainRack`. Va al borrador, con confirmación y aviso con recuento. Dinámico es todo o nada; Selectivo, por casillas. **Nota:** la operación recompone al final y `Apply` del modelo de la vista recompone otra vez (dos recomposiciones por clic, no una por escritura).

**Ediciones a archivos compartidos** (pequeñas y marcadas `E2 Restaurar`):
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: el botón en la cabecera (entre «Descartar» y «Cerrar el editor») y el panel tras `MessageText`. Son dos bloques, porque el botón va en la fila de botones y el panel debajo.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml.cs`: tres manejadores y `CommitFocusedText`, antes de `OnCloseYesClick`.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`:
  - sale el bloque «E2-B «Restaurar layout…»» (mudado al parcial);
  - una línea en `SetConfirmingClose` (`if (value) CloseRestore();`);
  - una línea en `Rebuild`, tras `RebuildSelection()` (`RefreshRestore();`).
- `RackCadWindowViewModel.cs` y `RackCadWindowView.xaml(.cs)`: sin cambios.

**Las pruebas:**
- UI, `tests/RackCad.UI.Tests/Workspace/SelectiveRestorePanelTests.cs` (nuevo, 15; 12 del modelo de la vista y 3 de vista con WPF real en STA):
  - el panel abre con todo marcado, recuentos, texto y líneas en orden;
  - casilla «(ninguno)» deshabilitada y marcada, y «Restaurar lo marcado» deshabilitado sin nada que quitar;
  - nada manual: no abre y avisa;
  - desmarcar quita sus líneas y la restauración la conserva, con el aviso exacto, «Modificado», sin «M» y con la selección; todo marcado = el rack sin personalizaciones;
  - singular y «Descartar»;
  - cada apertura vuelve a todo marcado y plegado, y «Cancelar» no toca nada;
  - el panel sigue al borrador;
  - pregunta de cierre ↔ panel;
  - escribiendo;
  - el valor rechazado;
  - todo el rack con «Fondo 2» en la vista (R-01) y la rama del selector;
  - la vista: botón, panel, casillas ligadas, líneas, nombres de automatización únicos, «Restaurar lo marcado», «Descartar», «Cancelar», sin errores de enlace; botón deshabilitado sin `CanEdit` (escribiendo y rack que no recompone); aviso sin abrir.
- UI, `SelectiveLayoutRestoreTests` (de 10 a 12): las líneas de `Losses` (textos y orden del draft, tantas como cuenta cada casilla) y el aviso con «Se conservan» y en singular; dos asertos al texto de §6.
- **Comprobación en rojo:** diez piezas rotas a propósito, una a una, con las dos clases tras cada una (script y salida en el scratchpad de la sesión, `hs65\selectivo\p2-restaurar\`): cada una falló:
  1. el alcance del selector de fondos en lugar de todo el rack: falla la de R-01 («Fondo 2» en la vista);
  2. sin refrescar el panel en `Rebuild`: fallan la del panel que sigue al borrador, la de «escribiendo» y la de vista con escritura;
  3. la pregunta de cierre sin cerrar el panel: falla la de la pregunta ↔ panel;
  4. una apertura que no vuelve a marcar todo: falla la de cada apertura;
  5. sin la frontera del valor rechazado: falla la del valor rechazado;
  6. las líneas sin filtrar por casilla: fallan cinco (las del modelo de la vista y las dos de vista);
  7. una casilla «(ninguno)» habilitada: fallan tres;
  8. el aviso sin «Se conservan»: fallan tres (dos de `SelectiveLayoutRestoreTests` y una del panel);
  9. el panel que se abre sin nada manual: fallan la del modelo de la vista y la de vista;
  10. el panel de la vista ligado a otra bandera: falla la de vista.

  El script verifica que cada archivo vuelve byte a byte a su copia (y le da fecha nueva); los SHA-256 de los cuatro archivos tocados coinciden con los de antes de la comprobación. Las suites de abajo corrieron después.
- **UI completa:** 2705 correctas, 0 con error, 48 omitidas (2753): la base de `938823b2` más las 17 nuevas (15 del panel y 2 de `SelectiveLayoutRestoreTests`); 27 de 27 en las dos clases focales.
- **Core Full:** 19144 correctas, 0 con error, 403 omitidas (19547). Esta parte no añade pruebas de Core, así que es el número de la base `938823b2`; las guardas de fuente de Core pasan con el archivo parcial nuevo.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- Las preguntas al Owner: R-01 (alcance, spec §10) y R-02 (recordar la elección).
- Smoke en AutoCAD: el panel con el tamaño real de la ventana y el foco del teclado (Tab por las casillas, Enter en «Restaurar lo marcado»).
- **Para integrar:** las tres partes añaden su sección al final de este archivo (conflicto trivial de documentación). En la cabecera de `SelectiveEditorView.xaml`, P3 añade «Insertar vista…» junto a «Actualizar» (como Dinámico) y esta parte entre «Descartar» y «Cerrar el editor»: no se tocan.

### Entrega 2, P3, pantalla: «Insertar vista…» y «Selectivo en esta ventana»

**Rama:** local `architecture/workspace-persistente-rackcad-selectivo-e2-insertar`, avanzada a `938823b2` (todo lo que no es pantalla de la entrega 2 ya integrado). Worktree `D:\w64e2insertar`. Sin commit, sin AutoCAD. P1 (seguridad) y P2 (restaurar) se construyeron a la vez en otros worktrees.

**Lo que se construyó** (como la segunda entrega de Dinámico, paso 4 y D-10):
- **«Insertar vista…»** en la cabecera del editor de Selectivo, entre «Actualizar» y «Descartar» (el orden del draft).
  - Elige las vistas con el diálogo de «Insertar varias…» del clásico, como modal de AutoCAD por el puerto del host (`IRackCadModalPort`). Las opciones son `SelectiveDoorInsertion.ViewOptions`: una frontal por fondo, un lateral por cada poste con corte y la planta, en el orden del clásico.
  - El diálogo va con `RackSystemKind.SelectiveRack` (`SelectiveViewChoice`, archivo nuevo). Con el sistema de Dinámico (`RackCadModalDialogs.ChooseViews`) el diálogo quitaba las frontales por fondo: ese sistema no las expone.
  - Envía `RequestSelectiveUpdate` con `InsertViews` (RACKSELECTIVOINSERTAR). La respuesta es la de una escritura (`MarkWritten`): escrito ⇒ «Limpio» contra el dibujo; si no redibujó nada, el borrador sigue sin escribir y el aviso es un error.
  - Sin cambios no se desactiva: redibuja las vistas que hay tal cual. «Actualizar» sigue siendo solo actualizar.
  - Se desactiva, con su texto, por las reglas de Dinámico (sin puerto o sin acción de la ventana, escribiendo, sin borrador válido, con error) y por las dos de «Actualizar» de Selectivo: un rack de solo lectura (S-08) y tramos sin aplicar (S-21).
  - El clic lleva antes a la casilla el nombre recién escrito, como «Actualizar».
  - El nombre de automatización y la ayuda siguen al rótulo: «Insertar vistas del rack» / «Insertar el rack nuevo en el dibujo». La ayuda de un rack nuevo es la nota del draft sin el nombre: «Inserta el rack nuevo en el dibujo, con una identidad nueva, con las vistas que elijas.» (la de Dinámico es fija).
- **«Selectivo en esta ventana»** en «Nuevo», encima del botón de Dinámico y de las tarjetas de siempre (el orden del draft). AutomationName «Nuevo Selectivo en esta ventana».
  - Crea el borrador con `SelectiveDraftEditor.CreateNew` (los valores por defecto del clásico) y la identidad del rack (un GUID) al crearlo. Su editor se aloja en la página «Nuevo» (`NewSelectiveRackHost`). Pedido otra vez, muestra el mismo.
  - El editor dice al abrir el texto del draft: «Selectivo nuevo en esta ventana, con los valores por defecto del editor clásico. Todavía no está en el dibujo.»
  - «Actualizar» no se ofrece hasta que está en el dibujo: «Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».». El botón de la cabecera se llama «Insertar en el dibujo…» y envía la escritura con `NewRack` (RACKSELECTIVONUEVO). Lo que envía pasa `SelectiveDoorInsertion.PlanNewRack` (prueba).
  - Colocado (`AnswerForNewRack`): `MarkInserted` con el nombre que dio el dibujo (el automático de I-60, solo si el borrador no tenía nombre, M-04). El mismo editor es desde ahí el del rack: «Actualizar» e «Insertar vista…». «Nuevo» vuelve a mostrar sus sistemas y la ventana dice el aviso de la orden.
  - **D-10:** la ventana no relee la lista ni abre el rack sola; el aviso pide releer. Releída la lista y abierto el rack, aparece el mismo editor (prueba).
  - Cancelada la colocación: sigue nuevo, con el aviso de error.
  - **I-03:** «Nuevo» sigue al dibujo, y un Selectivo nuevo de otro dibujo no se escribe en el activo (lo dice).
  - **D-08:** con cambios veta el cierre, y el aviso lo nombra «un Selectivo nuevo» (el texto del consejero ya era genérico: «un rack nuevo de la ventana»). La poda de un dibujo cerrado dice «Se cerró el dibujo de un Selectivo nuevo…».
  - «Descartar» de un rack nuevo dice «Borrador descartado.» y vuelve a los valores por defecto (M-03 de Dinámico).

**Ediciones a archivos compartidos** (pequeñas y localizadas; lo nuevo va en archivos propios):
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: `!_isNewRack` en `CanUpdate`; la primera línea de `UpdateBlockedText`; el texto de «Descartar» de un rack nuevo; `CanInsertView` e `InsertViewBlockedText` en `RaiseState`.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: un bloque `<!-- E2 P3 -->`, el botón `InsertViewButton`. `SelectiveEditorView.xaml.cs`: su manejador `OnInsertViewClick`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`:
  - la clase pasa a `partial`;
  - `ShowNuevoCards` mira también el Selectivo nuevo;
  - `ActiveSelectiveEditor` excluye un rack nuevo (como Dinámico);
  - en `OnSelectiveRead`, las dos suscripciones pasan a `WireSelective(editor)`;
  - `UpdateSelective` pasa a `WriteSelective(editor, insertViews)` con `InsertViews` y `NewRack`;
  - el rack nuevo colocado en `OnDesignWritten`;
  - «un Selectivo nuevo» en `SelectiveNameOf`;
  - `RaiseNewSelective()` en `RaiseNewDynamic`;
  - una línea en `StartNewDynamic`: no empieza un Dinámico nuevo si hay un Selectivo nuevo.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: el anfitrión `NewSelectiveRackHost` y el botón `NewSelectiveHereButton`, dos bloques `<!-- E2 P3 -->` a tres líneas uno del otro. `RackCadWindowView.xaml.cs`: una línea en `OnViewModelPropertyChanged`, `ShowNewSelectiveContent` y `OnNewSelectiveHereClick`.
- Archivos nuevos: `SelectiveEditorViewModel.Insert.cs`, `RackCadWindowViewModel.Selective.cs` y `SelectiveViewChoice.cs` (UI), y `tests/RackCad.UI.Tests/Workspace/RackCadSelectiveInsertTests.cs`.
- **Para integrar con P1:** `WireSelective` es el cableado de todo editor de Selectivo de la ventana (el abierto con «Editar aquí» y el nuevo). La asignación de `SafetyEditor` de P1 debe ir allí, para que también la tenga el Selectivo nuevo.

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| P3-01 | **Supuesto provisional:** las vistas se eligen con el diálogo clásico de «Insertar varias…», como Dinámico, y no con el panel del draft (583-605). Aislado en `SelectiveViewChoice`: el editor solo conoce `ViewChooser`. **Diferencia declarada** con el draft | Encargo; ya probado en AutoCAD con Dinámico; deja elegir varios laterales y el orden |
| P3-02 | El texto de «Nuevo» es el del draft sin «{nextName}»: «… si no le pones nombre, toma el automático (I-60).» | El siguiente nombre (`RackNewRackName`) solo se lee en el Plugin bloqueando el documento, y la lista de la ventana puede estar vieja (O-18): un nombre previsto podría ser falso. La spec lo pedía así si no había API |
| P3-03 | «Insertar vista…» también se bloquea con un rack de solo lectura (S-08) y con tramos sin aplicar (S-21), con los textos de «Actualizar» | La escritura llevaría el borrador sin esos tramos y diría «Limpio» |
| P3-04 | «Nuevo» aloja UN rack nuevo del dibujo a la vez, de cualquiera de los dos sistemas. Si se pide el otro, muestra el que hay y dice: «En «Nuevo» ya hay un rack nuevo sin insertar: insértalo en el dibujo o cierra su editor antes de empezar otro.» (**texto provisional**) | El editor del rack nuevo reemplaza la página, así que desde la pantalla no se llega (los botones no se ven). Es una defensa, para que los dos anfitriones nunca se vean a la vez |
| P3-05 | **Como Dinámico, no como el draft:** «Descartar» de un Selectivo nuevo vuelve a los valores por defecto y no cierra el editor (el draft lo borraba: «Se descartó el Selectivo nuevo: no estaba en el dibujo.»). La pregunta de «Cerrar el editor» es la genérica, sin el botón «Insertar en el dibujo…» del draft | «Todo como Dinámico» (M-03) |
| P3-06 | Las opciones con un fondo dicen «Frontal · fondo 1», como el clásico (el draft dice «Frontal») | Son las del clásico |
| P3-07 | La frase «Debajo siguen las tarjetas de siempre, que abren el editor clásico con RACKNUEVO.» del draft no se añadió: queda la de Dinámico («Elige el sistema: …») | Cambio mínimo de la página compartida; dicen lo mismo |
| P3-08 | Declarado, como Dinámico (M-08): el diálogo de vistas se abre aunque la puerta espere otra petición; la escritura se rechaza después con su aviso | La regla de una petición en vuelo es de la ventana |

**Las pruebas** (`tests/RackCad.UI.Tests/Workspace/RackCadSelectiveInsertTests.cs`, nuevo, 18):
- las opciones del clásico con uno y dos fondos (etiquetas y direcciones);
- el diálogo con el sistema de Selectivo: guarda las frontales por fondo, el orden elegido y la cancelación;
- «Insertar vista…» envía copias del borrador con las vistas; su respuesta es la de una escritura, y deja «Limpio»;
- sin cambios se ofrece; si no redibujó, sigue sin escribir;
- los textos que la bloquean (sin puerto, fuera de la ventana, tramos sin aplicar, escribiendo, tomada) y su aviso de estado;
- el Selectivo nuevo: se hace, se inserta como rack nuevo (lo acepta `PlanNewRack`), toma el nombre del dibujo y se reabre con el mismo editor;
- con nombre propio lo conserva; si no se colocó, sigue nuevo;
- la orden tomada; el dibujo del rack nuevo (I-03); sin la puerta de Selectivo o sin dibujo no se ofrece;
- un rack nuevo a la vez; «Descartar» y «Cerrar el editor»;
- el consejero de cierre (el texto entero) y la poda nombran «un Selectivo nuevo»;
- con WPF real (STA) y sin errores de enlace:
  - la cabecera: el botón entre «Actualizar» y «Descartar», su rótulo y su nombre, el clic y el estado;
  - el editor de un rack nuevo: el rótulo, «Actualizar» apagado y su texto, y el nombre recién escrito que va con el clic;
  - la página «Nuevo»: el botón, su texto y su orden, el editor alojado, «Nuevo» tras insertar y la misma vista en la página del rack.
- **Comprobación en rojo:** dieciséis piezas rotas a propósito, una a una, con la clase de pruebas tras cada una; cada una hizo fallar al menos una prueba (de 1 a 5). Script y salida en el scratchpad de la sesión, `hs65\selectivo\p3-red\`. Las piezas:
  1. el diálogo con el sistema de Dinámico;
  2. la escritura sin las vistas;
  3. la escritura sin la marca de rack nuevo;
  4. el rack colocado que sigue nuevo;
  5. «Actualizar» ofrecido a un rack nuevo;
  6. el cierre sin «un Selectivo nuevo»;
  7. las tarjetas de «Nuevo» con un Selectivo nuevo;
  8. «Nuevo» sin refrescar al cambiar de dibujo;
  9. los tramos sin aplicar que no bloquean;
  10. el nombre del dibujo que reemplaza el del borrador;
  11. el clic sin el nombre recién escrito;
  12. el estado de «Insertar vista…» sin avisar;
  13. el rótulo que no vuelve a «Insertar vista…»;
  14. el texto de «Descartar» de un rack nuevo;
  15. dos racks nuevos en «Nuevo»;
  16. la ventana que no le da el diálogo al editor.

  La 2 y la 3 se repitieron con un patrón que no tocara la escritura de Dinámico, que tiene la misma línea. Cada archivo se restauró de su copia con fecha nueva y se comprobó igual byte a byte.
- **Focales:** `RackCadSelectiveInsertTests` 18/18; con las clases de la ventana de Selectivo y de Dinámico, `RackCadWindowTests`, `SelectiveDraftEditor*`, `SelectiveSafetyViewTests`, `SelectiveLayoutRestoreTests`, `SelectiveNewRackRequestTests`, `DynamicSafetyTabTests` y `RackModalHostWindowTests`, 269/269.
- **UI completa:** 2706 correctas, 0 con error, 48 omitidas (2754). Es la base de `938823b2`, 2688 correctas (2736), más las 18 nuevas: la base sale por diferencia y no se corrió aparte.
- **Core Full:** 19144 correctas, 0 con error, 403 omitidas (19547). P3 no toca Core: es la cifra de `938823b2`.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- **Preguntas al Owner:** P3-01 (panel del draft o diálogo clásico), P3-02 (mostrar el nombre previsto) y el texto de P3-04.
- **Sin AutoCAD:** el smoke de la pantalla, con la colocación, el nombre automático y el «U» de una inserción, que tampoco se probó con Dinámico.
- **Integración** con P1 (`WireSelective`) y P2 (los dos botones de la cabecera quedan juntos en el mismo `WrapPanel`).

## Selectivo en la ventana (línea D), entrega 2, «Extremos y seguridad», pantalla (P1)

**Rama:** local `architecture/workspace-persistente-rackcad-selectivo-e2-seguridad`, avanzada a `938823b2` (la integración de las tres partes sin pantalla). Worktree `D:\w64e2seguridad`. Sin commit, sin push, sin AutoCAD. Spec: `sprint/spec-seguridad.md` (sus [VERIFICAR] ya los resolvió C2; manda el código).

**Lo que se construyó:**
- **La pestaña «Extremos y seguridad»** (`SafetyTab`), entre «Matriz» y «Avanzado» como en Dinámico (`SelectedTab`: 0 Rack, 1 Matriz, 2 Extremos y seguridad, 3 Avanzado), con los textos del draft:
  - «En las celdas: frente × nivel» y «Para elegir celdas, márcalas en «Matriz»: inspector › «Seguridad de la celda».»;
  - una tarjeta por familia, **T Topes** y **P Parrillas** (`SelectiveSafetyFamilyCard`, refrescadas en su sitio): letra, título, resumen («En 3 de 4 celdas con larguero · 6 piezas.», las piezas de las categorías `Tope` y `Parrilla` del BOM; «, en todos los fondos.» en la parrilla con varios fondos), configuración (saque, compartido/por fondo, frontal; vistas, ancho y cantidad de la parrilla) y «En todas las celdas» / «En ninguna», deshabilitados cuando no escriben nada; sin variante, «Sin variante elegida: no se dibujan.»;
  - «Extremos y fondos» (`EndLines`): protector lateral, botas, desviadores (longitud y primer nivel validados por `SelectiveDesviadorPlan`) y separadores entre fondos (o «no hay, el rack tiene un solo fondo»);
  - «Seguridad por poste»: el resumen, la DataGrid de solo lectura con las columnas exactas del draft y la fila «Total» al final, en negrita (`PostSafetyTable`), la nota del draft, la línea de estado (`SafetyEditText`) y «Editar seguridad…».
- **T y P en las celdas de «Matriz»** (decisiones 4 y 5): «P» abajo a la izquierda y «T» abajo a la derecha, como el draft, con los colores del shell (`ShellStatusWarningBrush`, `ShellStatusSuccessBrush`) y su entrada en la leyenda. Para todo el rack: la marca se lee con la traducción de C2 (el «Larguero a piso» de cada fondo); N1 en el piso no la lleva; con «Todos los fondos» y fondos que no coinciden, la marca va en cursiva. El nombre de UI Automation de la celda añade «con tope» / «con parrilla». El modelo de la celda es parcial: las marcas viven en `SelectiveEditorViewModel.Safety.cs` (`SelectiveCellItem.SetSafety`).
- **«Seguridad de la celda» en el inspector**, tras «Tarima» como en el draft: una casilla por familia con su etiqueta de estado («Tope (T)», «Tope (varía; marcar lo pone en todas)», «Tope: sin variante elegida», «Tope: no en el piso sin larguero»; igual para la parrilla), deshabilitada cuando no puede escribir, y la nota del draft («Van por frente y nivel en todo el rack, no por fondo, como hoy…») con lo que se deja fuera cuando la selección tiene celdas en el piso. Selección mixta: un clic en la casilla indeterminada (que WPF convierte en `false`) pone la familia en todas las celdas, nunca la quita (S-20 / D-11). La escritura es la de C2 (`SetTope`/`SetParrilla`), un paso por acción.
- **«Editar seguridad…» por el puerto modal** (D-09): `RackCadSelectiveSafetyDialog` (archivo nuevo) construye `SelectiveSafetyWindow` con los argumentos del clásico (`RackSelectiveWindow.Safety_Click`: todos los elementos del catálogo, los postes del fondo más largo, los niveles de larguero resueltos, los fondos, el plan de las parrillas y el sistema resuelto) sobre copias de la seguridad del borrador, y lo muestra por `IRackCadModalPort`; la ventana lo asigna a `SafetyEditor` cuando tiene el puerto. «Aceptar» = `ReplaceSafety` + recomposición; «Cancelar» = nada.
- **Lo que lee el BOM se cuenta una vez por recomposición y solo mientras la pestaña se ve**, como «Calculado» en «Rack»: la tabla, las tarjetas y «Extremos y fondos» comparten un recuento (`SelectiveSafetyView.Count`), sobre un sistema propio (P1-01).

**Ediciones a archivos compartidos** (pequeñas y marcadas `E2 seguridad`):
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`:
  - `SelectiveCellItem` pasa a `partial` (una palabra y una línea del comentario);
  - `SelectedTab`: el comentario de los índices y una línea que llama a `OnSafetyTabShown()`;
  - `TakeBaselineCells` y `RebuildSelection`: la instantánea `SafetyMarks()` y `+ marks.StateOf(k, f, l)` en la clave de estado de la celda (P1-02); en el bucle de las celdas, `item.SetSafety(...)` (dos líneas) y `states.AddRange(SafetyStates(...))`;
  - `RaiseState`: una línea, `RaiseSafetyState()`.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: el bloque principal es la pestaña (`<!-- E2 seguridad: «Extremos y seguridad» … -->`, antes de «Avanzado»); además, tres sitios pequeños con el mismo comentario: las dos marcas en la plantilla de la celda (con la celda a 40 de alto, `MinHeight` del botón de 32 a 40, y el `Grid` interior con `MinHeight="32"`, P1-12), las dos entradas de la leyenda (y su comentario, que ya no dice «T y P son de la entrega 2») y la sección «Seguridad de la celda» del inspector.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml.cs`: tres manejadores en un bloque (las dos acciones de las tarjetas y «Editar seguridad…»).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: un método privado propio, `WireSelectiveSafety(SelectiveEditorViewModel editor)` (asigna `SafetyEditor` con el puerto modal), y una sola línea que lo llama en `OnSelectiveRead`, tras crear el editor. Al integrar con P3, esa llamada va dentro de su `WireSelective(editor)`, que cablea todo editor de Selectivo de la ventana (el de un rack del dibujo y el de un Selectivo nuevo).
- `src/RackCad.Application/VentanaRackCad/SelectiveDraftEditor.cs`: `ResolveCopy()`, nuevo, tras `BomCounts()` (el mismo `Copy` + `_resolver`).
- `src/RackCad.Application/VentanaRackCad/SelectiveSafetyView.cs` (de C2): `SelectiveSafetyMark`, `SelectiveSafetyMarks` (T/P leídos una vez por refresco de la matriz), `SelectiveSafetyCounts` y `Count`, `GridLevelOf` (la traducción de una bahía, que `GridLevel` ahora llama) y `PostRows` sobre la copia.
- `tests/RackCad.UI.Tests/Workspace/RackCadSelectiveEditorTests.cs`: el censo de pestañas pasa de tres a cuatro, con nota (el nombre del método, «…ThreeTabs…», no se cambió para no chocar con P2 y P3).
- **No se tocaron** `RackCadModalDialogs.cs`, `RackCadWindowView.xaml(.cs)` ni el Plugin.

**Las decisiones:**

| Id | Decisión | Por qué |
|---|---|---|
| P1-01 | La seguridad de la pestaña (la tabla, las tarjetas y «Extremos y fondos») y `SelectiveSafetyView.PostRows` se cuentan sobre un sistema resuelto de una COPIA del último diseño válido (`SelectiveDraftEditor.ResolveCopy`), no sobre `editor.System`. **Corrige un hallazgo latente de C2** | La planta y los cortes que lee `SelectivePostSafetySummary`, y el BOM, imponen en su sitio la profundidad del fondo a cada cabecera personalizada (ARQ-43-10), y `System` comparte esas cabeceras con `Design`: con la pestaña en pantalla, mostrarla cambiaba el diseño que se escribe y el editor se ponía «Modificado» solo. La misma regla que «Calculado» (I-01 de la revisión del pulido). Prueba con `TwoFondosWithACustomCabeceraOutOfItsRule` y rojo 1 |
| P1-02 | Cambiar T o P de una celda la marca «Modificada en el borrador» (solo esa celda) y volver a ponerla la deja limpia | El draft lo cuenta en la marca de la celda (`mod` compara `topeAt`/`parAt` con lo leído). La clave de estado de la celda añade T/P de su larguero en su fondo |
| P1-03 | Con «Todos los fondos» y fondos que no coinciden en una celda (C2-02), la marca va **en cursiva**, con el texto «Tope: varía entre los fondos de la vista» en el tooltip y «el tope varía entre los fondos» para el lector de pantalla. **Provisional** | El draft no tiene el caso; la cursiva es la forma de «varía» de la matriz |
| P1-04 | La pestaña lee el BOM una vez por recomposición y solo mientras se ve; al mostrarla se cuenta una vez | Como «Calculado» (M-03 de la revisión del pulido): una edición en «Matriz» no cuenta nada. Prueba con `SafetyCountBuilds` y rojo 4 |
| P1-05 | «Botas» en «Extremos y fondos» dice cuántos postes llevan bota (de la tabla, contada como el BOM) y remite a la tabla para el lado: «Botas: en 3 de 4 postes, el lado de cada uno en «Seguridad por poste»; un poste puede tener la suya.» En lugar del «general en …» del draft. **Texto provisional** | La general de la bota (I-42) tiene automático del sistema, blancos y dos lados; decir aquí cuál es la general sería otra regla, y la tabla ya dice el lado de cada poste (decisión 13) |
| P1-06 | «Protector lateral» sale de la tabla: «en los postes de los extremos, P1 y P4.» si son exactamente los dos extremos; si no, la lista («en P1, P3.»); «elegido, en ningún poste.» si no coloca ninguno | El draft solo pensaba en los extremos; el clásico permite cualquier poste (como C2-06) |
| P1-07 | La tarjeta de topes con varios fondos y un fondo elegido en el diálogo dice «en el fondo N, compartido entre fondos» / «en el fondo N, uno por fondo». **Texto provisional** | El draft solo tiene el fondo central automático |
| P1-08 | «Total» es la última fila de la DataGrid, en negrita (`PostSafetyTable`; sin ordenar columnas), no un pie aparte; `PostSafety` sigue siendo solo los postes | Una sola tabla accesible, con las columnas alineadas, como el `tfoot` del draft |
| P1-09 | Sin el puerto modal, «Editar seguridad…» está deshabilitado y la línea dice «El diálogo de seguridad no está disponible aquí: edita la seguridad con RACKEDITAR (el editor clásico).»; con AutoCAD escribiendo, el texto de la espera (`WritingText`) | El Selectivo de la ventana no tiene «Abrir en editor clásico» (Dinámico sí); RACKEDITAR es lo que dicen las demás líneas del editor |
| P1-10 | El texto visible bajo la tabla es el del draft sin «(D-09)» ni la frase del prototipo sobre la doble profundidad ([PENDIENTE] del prototipo) | Son notas del prototipo, no de la pantalla |
| P1-11 | El diálogo se construye en un archivo propio (`RackCadSelectiveSafetyDialog`), no en `RackCadModalDialogs` | Para integrar sin chocar con P3, que puede tocar ese archivo para «Insertar vista…» |
| P1-12 | La celda de «Matriz» pasa de 32 a 40 de alto (la del draft) y su contenido ocupa el alto de la celda: «P» y «T» quedan en las esquinas de abajo, y las marcas de E1 (modificada, distinta, «M») en las de arriba, como en el draft | Revisado en un render de la vista (WPF real, prueba temporal borrada después): con 32 de alto y el contenido centrado, las marcas se pegaban a la tarima («P40×60T») y el punto de «Distinta» tapaba la «P» |

**Supuestos provisionales** (preguntas abiertas al Owner, aislados):
- La bota en las dos caras dice «Izquierda y Derecha», en una constante, `SelectiveSafetyView.BothSides` (C2-08; el clásico dice «Ambas»).
- P1-03 (la cursiva y sus textos), P1-05 (la línea de las botas) y P1-07 (el fondo explícito del tope): cada uno en un sitio de `SelectiveEditorViewModel.Safety.cs` (`SafetyStates`/`TopeTip`, `BuildEndLines`, `TopeConfig`).

**Las pruebas:**
- `tests/RackCad.UI.Tests/Workspace/SelectiveSafetyTabTests.cs` (nuevo, 20):
  - modelo de la vista: las marcas T/P donde hay larguero y no en el piso, y su texto accesible; T apagada quita la marca y la celda queda «Modificada» (solo esa), y volver a ponerla la limpia; fondos que no coinciden: cursiva en «Todos» y la marca de cada fondo; la casilla del inspector (quitar, «varía», un clic en «varía» pone en todas); casillas sin variante o en el piso, deshabilitadas, con su texto y la nota; las tarjetas con las piezas del BOM, «En ninguna» / «En todas las celdas» y sin variante; la tarjeta de topes con dos fondos; «Extremos y fondos» con un fondo y con dos; la tabla con su «Total» = BOM; mostrar la pestaña no cambia el diseño con una cabecera personalizada fuera de su regla; «Editar seguridad…» (sin delegado, cancelar, aceptar) y mientras AutoCAD escribe;
  - WPF real (STA): el diálogo de la ventana = la llamada del clásico, palabra por palabra (aceptados sin cambios dan la misma seguridad); el diálogo por el puerto (cancelar = null, aceptar = su resultado sin tocar el borrador); la ventana ofrece el diálogo solo con el puerto y lo abre por él (puerta doble propia de la prueba); la pestaña entre «Matriz» y «Avanzado»; la pestaña sin errores de enlace, con los textos y las columnas del draft, el «Total» en negrita, los nombres de UI Automation de los botones de las tarjetas (únicos, con la familia) y el clic en «En ninguna»; las marcas como insignias en sus colores y la leyenda; el clic en la casilla indeterminada por su `IToggleProvider`; lo que lee el BOM, contado solo con la pestaña visible.
- **Comprobación en rojo:** ocho piezas rotas a propósito, una a una, con sus pruebas tras cada una (script y salida en el scratchpad de la sesión, `hs65\selectivo\p1seg\red.py`, `red-summary.txt`, `red-out.txt`); cada una falló por su motivo:
  1. la seguridad contada sobre el sistema del borrador, sin copia: falla la de la cabecera personalizada (la profundidad pasa de 42 a 34 in);
  2. T y P fuera de la clave de estado de la celda: falla la de «Modificada»;
  3. la casilla en «varía» sin la regla «marcar lo pone en todas»: fallan las dos (la del modelo de la vista y el clic por `IToggleProvider`);
  4. lo que lee el BOM anunciado aunque la pestaña no se vea: falla la del recuento (2 en lugar de 1);
  5. la ventana sin asignar el diálogo: falla la de la ventana con el puerto;
  6. la marca que varía entre fondos dicha como puesta: falla la de los fondos que no coinciden;
  7. las piezas de las tarjetas contadas de la rejilla, no del BOM: falla la de las tarjetas (4 piezas en lugar de 8);
  8. la fila «Total» sin su marca: fallan la de la tabla y la de la vista (no va en negrita).

  Cada archivo se restauró de su copia con fecha nueva y se comparó byte a byte con ella (idénticos los ocho); la compilación final, correcta.
- **Focales UI:** `SelectiveSafetyTabTests`, `SelectiveSafetyViewTests`, `RackCadSelective*`, `SelectiveDraftEditor*`, `SelectiveLayoutRestoreTests`, `SelectiveNewRackRequestTests`, `SelectiveTopeSide*`, `DynamicSafetyTabTests` y `RackCadWindowTests`: 257 de 257 antes de la comprobación en rojo; después, las dos completas de abajo sobre las fuentes finales.
- **UI completa:** 2708 correctas, 0 con error, 48 omitidas (2756), sobre las fuentes finales. La base `938823b2` no se midió aparte: son sus pruebas más las 20 nuevas de `SelectiveSafetyTabTests` (el censo de pestañas cambia una aserción, no añade prueba), es decir 2736.
- **Core Full:** 19144 correctas, 0 con error, 403 omitidas (19547), igual que la base `938823b2` medida en este worktree antes de tocar nada (19144 / 0 / 403): P1 no añade pruebas de Core.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- **Sin AutoCAD:** «Editar seguridad…» por `RackCadModalPort` (el modal real sobre AutoCAD) y la pestaña en la paleta esperan el smoke.
- **Preguntas al Owner:** las de C2 (C2-02, C2-08, C2-04) y P1-03, P1-05 y P1-07.
- **Para integrar con P2 y P3:** los tres trabajan en `SelectiveEditorView.xaml`, `SelectiveEditorViewModel.cs` y `RackCadWindowViewModel.cs`; las ediciones de P1 están listadas arriba. Si P3 añade miembros a `IRackCadSelectiveDoorPort`, la puerta doble de `SelectiveSafetyTabTests` necesita los suyos.

### Entrega 2: correcciones de la revisión independiente (`review-e2.md` sobre `6c62f61c`)

**Rama:** `architecture/workspace-persistente-rackcad-selectivo` (worktree `D:\w64s`), sobre `6c62f61c`, sin commit ni push. Sin AutoCAD. Dinámico no se tocó. Veredicto de la revisión: APTO TRAS CORRECCIONES (0 B, 1 I, 4 M y pruebas que faltaban).

| Hallazgo | Estado | Qué se hizo |
|---|---|---|
| I-1 | CORREGIDO EN EL ORIGEN | Era la tercera vez del mismo defecto: un lector del sistema del borrador cambiaba las cabeceras personalizadas del diseño. Antes fueron «Calculado» y la pestaña de seguridad; ahora, las opciones de «Insertar vista…». `SelectiveDraftEditor.Recompose` resuelve ahora `System` sobre una copia propia del diseño (`Copy(design)`). `SnapshotForWrite` resuelve el sistema de la escritura sobre otra deserialización de su JSON. Así, `System` y `Design` (y los del snapshot) no comparten ningún objeto, y ningún lector de `System` puede cambiar el diseño que se escribe. Los lectores siguen imponiendo la profundidad del fondo en su sitio (`EffectiveCustomAt`, ARQ-43-10, diferido y sin tocar), pero ya solo sobre el sistema. «Insertar vista…» sigue armando sus opciones sobre `editor.System`. `ResolveCopy()` se queda: da un sistema propio a quien impone o lo guarda (el BOM, los planes de la seguridad y el diálogo clásico), y así `System` sigue como lo dio el resolvedor; las pruebas de P1-01 lo comprueban. `BomCounts()` queda en una línea: `SelectiveBomCounts.Of(ResolveCopy(), Catalog)`. **La equivalencia con el clásico no cambia:** `SelectiveDraftEditorEquivalenceTests` sigue en verde sin tocarla, porque compara el JSON de `Design`, que se construye igual que antes. Coste: una copia más por recomposición y una deserialización más por escritura |
| M-1 | CORREGIDO (por I-1) | RACKSELECTIVOINSERTAR (`DrawableViews`) y RACKSELECTIVONUEVO (`PlanNewRack`) siguen leyendo los cortes de `write.System` antes de reconciliar. Ese sistema ya no comparte las cabeceras con `write.Design`, así que lo que se reconcilia y se escribe es el `DesignJson` enviado, el que la ventana guarda como «Limpio». El Plugin no cambia |
| M-2 | CORREGIDO | `EditSafety` pasa por `PendingFieldError` antes de abrir el diálogo, como `Safety_Click` del clásico con `CommitPendingEditors`. La línea de mensajes dice: «No se puede abrir «Editar seguridad…»: «Alto de tarima» tiene un valor que no se aplicó (El alto de tarima debe ser un número mayor que cero). Corrígelo o vuelve a escribir el que tenía.» El campo conserva lo escrito y su error. La vista confirma antes el campo con el foco (`CommitFocusedText`), como «Restaurar layout…». `PendingFieldError` recibe ahora el principio del texto; el de «Restaurar» no cambia |
| M-3 | CORREGIDO EN PARTE, CON DIFERENCIA DECLARADA | La ventana guarda, por dibujo (sesión), los estilos de cota de la última lectura de un Selectivo de ese dibujo (`SelectiveDesignRead.DimensionStyles`). Un Selectivo nuevo de «Nuevo» los recibe (`ForNewRack(…, dimensionStyles)`), sin leer el dibujo y sin esperar a AutoCAD. La poda olvida los de un dibujo cerrado. Ver la diferencia declarada debajo |
| M-4 | CORREGIDO | En un Selectivo nuevo, el aviso de «Restaurar» termina en ««Descartar» vuelve a los valores por defecto.». Lo da `SelectiveLayoutRestoreResult.NoticeFor(newRack)`, con las dos colas en constantes (`DiscardText`, `NewRackDiscardText`). Lo mismo en el aviso de S-09 al reducir frentes o fondos, que tenía la misma cola. Una vez insertado el rack, vuelve el texto de un rack del dibujo |
| M-5 | PRUEBAS NUEVAS | «Editar seguridad…» en un Selectivo nuevo, cableado en `WireSelective`. La guarda de B-1 en Selectivo, de comportamiento y de fuente (ver «Pruebas») |

**Diferencias declaradas nuevas.**
- **M-3: estilos de cota de un Selectivo nuevo.** Si la ventana todavía no leyó ningún Selectivo de ese dibujo, «Estilo de cota» ofrece solo «(Automático)». El clásico abierto desde RACKNUEVO sí recibe los del dibujo. Además, la lista es la de esa última lectura: un estilo creado después en el dibujo no aparece hasta la siguiente lectura («Editar aquí» de un Selectivo del mismo dibujo). Leer el dibujo para el rack nuevo obligaría a pedírselo a AutoCAD y esperar su respuesta (otra orden de la puerta), y la ventana no bloquea AutoCAD.
- **M-1: profundidad escrita, frente al clásico.** El caso es una cabecera personalizada fuera de su regla en el fondo 2 o siguientes, como la que trae un documento antiguo. «Insertar vista…» de la ventana escribe la profundidad guardada (42), igual que RACKSELECTIVOAPLICAR. Según la revisión (INFERIDO, no medido aquí), el «Insertar varias…» del clásico (RACKEDITAR) escribe la impuesta (34), porque arma sus cortes sobre un sistema que comparte las cabeceras con su diseño. La geometría y el BOM son los mismos, porque todo lector impone la regla. El clásico no se tocó (ARQ-43-10).

**Pruebas.**
- **Nuevas en UI (6):**
  - `SelectiveDraftEditorGapTests.NoReaderOfTheSystemOfTheDraftOrOfItsWriteChangesTheirDesign`: la genérica. Recorre `System` y el sistema de la escritura con el BOM, los cortes, la planta (`BuildPlan` y `Build`), las opciones y las vistas dibujables de «Insertar vista…», la tabla por poste y el plan de parrillas. `Design` serializado y el JSON que se escribe no cambian, y ninguna personalizada de un sistema es un objeto de su diseño;
  - `RackCadSelectiveInsertTests.InsertarVistaOverACustomCabeceraOutOfItsRuleChangesNothingOfTheDraftAndAChangeUndoneIsCleanAgain`: el escenario de la revisión (S1, S2 y S6) por la ventana y su puerta doble, sobre `TwoFondosWithACustomCabeceraOutOfItsRule` (42 contra la regla de 34). «Insertar vista…» abierto y cancelado no cambia nada, y el nombre de ida y vuelta queda «Limpio». Lo enviado lleva 42. Tras la respuesta, una edición deshecha vuelve a «Limpio», sin trabajo sin escribir que vete el cierre (D-08);
  - `RackCadSelectiveInsertTests.ANewSelectivoOffersTheDimensionStylesOfTheLastReadOfItsDrawing` (M-3);
  - `SelectiveSafetyTabTests.Edit_safety_does_not_open_with_a_value_written_and_refused_in_a_field_as_the_classic_editor` (M-2, S9);
  - `SelectiveSafetyTabTests.A_new_Selectivo_of_Nuevo_has_the_safety_dialog_through_the_modal_port_of_the_host` (M-5, S7);
  - `SelectiveRestorePanelTests.ForANewSelectivoTheNoticeSaysThatDiscardGoesBackToTheDefaults` (M-4, con «Restaurar», «Descartar», S-09 y el rack ya insertado).
- **Nuevas en Core (3):**
  - `SelectiveDoorInsertionTests.RackSelectivoInsertarAndNuevo_WriteTheDesignTheWindowSent_WhateverTheirPlanReadsOfTheSystem` (M-1, S8). La escritura se arma desde `SnapshotForWrite` como la arma la ventana; después de `DrawableViews` y de `PlanNewRack`, el diseño serializado es el `DesignJson` enviado (42);
  - `SelectiveInsertedViewDesignTests` (archivo nuevo, M-5, B-1 en Selectivo), dos pruebas:
    - de comportamiento: la vista nueva preparada como `TryPrepare` lleva el borrador (alto 72, no el 60 del dibujo) y queda hermana (`Same`) de la existente redibujada como `PrepareMember`; con la vista tal como la tenía el dibujo, que era B-1, divergirían;
    - de fuente: `TryPrepare` y `PrepareMember` componen con el mismo `authoredJson`, que `InsertSelectiveViews` saca de `window.DesignToInsert` reconciliado, y RACKSELECTIVOINSERTAR le pasa `write.Design`.
- **Dobles de prueba ampliados:** en `RackCadSelectiveInsertTests`, `SelectiveDoorDouble.AnswerLastSelectiveRead` y `Rig.OpenEditor` aceptan estilos de cota; en `SelectiveSafetyTabTests`, `WindowRig` tiene `StartNew()`.

**Comprobación en rojo.** Se hizo sobre `6c62f61c` sin ningún cambio de producto, con las pruebas nuevas ya escritas:
- I-1: fallan las dos (lo enviado lleva 34, no 42);
- M-1: falla (lo escrito lleva 34 y lo enviado 42);
- M-2: falla (el diálogo se abre: 1 vez en lugar de 0);
- M-4: falla (la cola sigue siendo «lo último que se leyó o se escribió»);
- M-3 también falla (solo «(Automático)»).

Las dos de M-5 ya pasan en la base, porque guardan lo que funciona. La de fuente de B-1 se comprobó rompiendo `PrepareMember` a propósito, para que compusiera con `envelope.Design`: falla. El archivo se restauró con `git checkout` y no quedó ninguna diferencia.

**Los números** (fuentes finales, Debug):
- **UI completa:** 2749 correctas, 0 con error, 48 omitidas (2797). Es la base de `6c62f61c` (2743 / 0 / 48) más las 6 nuevas. `SelectiveDraftEditorEquivalenceTests`: 14 de 14, sin cambios.
- **Core Full:** 19147 correctas, 0 con error, 403 omitidas (19550). Es la base (19144 / 0 / 403) más las 3 nuevas.
- **Build Debug del Plugin:** 0 errores. Las 2 advertencias son las MSB3277 de las referencias de AutoCAD, que ya estaban.

**Lo que queda abierto.**
- **Sin AutoCAD:** que RACKSELECTIVOINSERTAR y RACKSELECTIVONUEVO escriban en el dibujo el JSON enviado con una personalizada fuera de su regla queda para el smoke; aquí se comprobó el diseño que reciben.
- El «Lista de materiales» y el «Insertar varias…» del clásico siguen imponiendo sobre su estado (ARQ-43-10, para la planeación).

## Selectivo en la ventana (línea D), smoke de la entrega 2 en AutoCAD (2026-11-26)

Orden del Owner: «También realiza tú la prueba de la entrega 2, si existen errores corrígelos». La hizo la sesión, por control de pantalla, sobre `56e057f4` (Debug recompilado con el árbol limpio; huellas comprobadas antes de empezar) y una copia de trabajo del dibujo de preparación, que nunca se guardó (su huella al terminar es la del principio). FILEDIA volvió a 1.

**Lo que pasó** (OBSERVADO):
- **«Extremos y seguridad»:** la pestaña con sus tarjetas, «Extremos y fondos» y «Seguridad por poste»; «Editar seguridad…» abre el diálogo clásico como modal de AutoCAD sobre copias del borrador; bota «Ambas» → 6 botas, «Izquierda y Derecha» en P1..P3; tope configurado → «En 7 de 7 celdas con larguero · 14 piezas.»; la casilla «Tope (T)» de una celda quita su T; «Por poste…» con P2 «Izquierda» → la tabla lo dice y el total baja a 5. «Actualizar» escribe la seguridad en las dos vistas (leído por COM: el mismo diseño, con `TopeOffCells` de la celda quitada) y las botas se ven en el dibujo.
- **«Restaurar layout…»:** sin nada manual, su aviso; con un largo manual, el panel con las seis casillas, «Ver lo que se pierde», «Restaurar lo marcado» desactivado sin casillas con valores; restaurado solo en el borrador vuelve a «Limpio», y con el valor ya escrito queda «Modificado», sin M y con la selección; «Descartar» lo devuelve.
- **«Insertar vista…»** con el borrador modificado: el diálogo del clásico, AutoCAD redibuja primero las vistas que había y luego coloca la nueva; las tres vistas de SEL-A llevan el mismo diseño (COM).
- **Un Selectivo nuevo:** «Insertar en el dibujo…» con dos vistas, nombre automático «Selectivo 1», el aviso de la ventana y las dos vistas con el mismo Id y diseño (COM). Con otro nuevo «Modificado», `CLOSE` → «No» a guardar → el dibujo no se cierra y el veto nombra «un Selectivo nuevo».
- **B-1 de Dinámico:** DIN 1 con el peso cambiado → «Insertar vista…» → la vista nueva y la que había llevan el mismo diseño, con el peso nuevo (COM).

**El fallo que se corrigió:**
- **SP-01** (diálogo clásico, también en `main`, visible desde «Editar seguridad…»): el editor por poste decía «"(por defecto)" usa la selección general (Derecha).» con la general en «Ambas». `SafetyPerPostWindow` leía `labels[(int)defaultSide - 1]`, y las etiquetas empiezan por «Ninguno»: Ambas salía «Derecha», Derecha «Izquierda» e Izquierda «Ninguno». Solo era el texto: los valores por poste ya se leían y escribían bien. Ahora lee `labels[(int)defaultSide]`.
- **Pruebas:** `SafetyPerPostIntroTests` (UI, 7, archivo nuevo): el texto con las etiquetas de lado y con las de ubicación (Entrada/Salida, Posterior, Ambas), y «= no lleva.» sin general. En rojo sobre `56e057f4` sin el cambio: fallan las 6 con general.

**Observaciones, sin cambio:**
- El tope elegido sin «Configurar…» no se guarda, y la rejilla del tope ofrece un «Larg. 4» en un frente que no tiene larguero a piso: son del clásico (la ventana le pasa los mismos argumentos que `RackSelectiveWindow.Safety_Click`).
- «Largo de larguero (n)» cuenta valores por nivel, también el de un nivel en el piso.
- Con un Selectivo nuevo «Modificado», la cabecera de la ventana dice «todo escrito» (cuenta los racks del dibujo); el veto sí lo cuenta. Es la cabecera compartida con Dinámico.
- Tras restaurar un borrador que vuelve a ser el dibujo, el aviso nombra «Descartar» con el botón desactivado.

**No probado:** el «U» de una inserción y cancelar la colocación; el teclado en el panel de «Restaurar»; una cabecera personalizada fuera de su regla escrita por RACKSELECTIVOINSERTAR o RACKSELECTIVONUEVO.

## Cuarta tanda, new: «Nuevo» con borradores (SD-06)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-new`, sobre `ac3fd1b0` (worktree `D:\w4new`). Sin commit, sin push, sin AutoCAD. Sin prototipo propio: parte de lo construido para «Nuevo» (P3 de Selectivo y paso 4 de Dinámico) y de la ventana principal del prototipo (`proto-rackcad\project\Main.dc.html`: «aparece en la lista … como «Nuevo, sin insertar», con su editor»), con la columna del prototipo de Largueros (las páginas abiertas salen debajo de «Nuevo», con su estado).

**La orden del Owner** (SD-06, sobre P3-04): «En lugar de nuevo, podríamos manejar borradores, tipo, borrador 1, borrador 2 etc, para poder tener varios nuevos, mejora la propuesta».

**Lo que se construyó:**
- **Varios borradores a la vez.** Cada «Selectivo en esta ventana» y cada «Dinámico en esta ventana» empieza un borrador nuevo: «Borrador 1», «Borrador 2»…, numerados por dibujo, de cualquiera de los dos sistemas con editor en la ventana. P3-04 (un solo rack nuevo y el rechazo del segundo) se quitó: `SecondNewRackText` y `RefuseSecondNewRack` ya no existen.
- **Un borrador ES un rack nuevo de la ventana**, como lo hicieron P3 y el paso 4 de Dinámico (ND-01): su editor vive en los diccionarios de los editores con `IsNewRack`. Por eso no cambian la puerta, la petición única en vuelo, «Insertar en el dibujo…» (RACKSELECTIVONUEVO, RACKDINAMICONUEVO), «Dejar de esperar», D-10 ni la poda de un dibujo cerrado. Lo nuevo es un registro al lado (`RackCadWindowViewModel.Drafts.cs`): el número, cuál se muestra y el elemento de cada uno.
- **En la columna, debajo de «Nuevo»**, los borradores del dibujo, cada uno con su punto y su etiqueta de estado («Sin insertar», «Modificado», «Escribiendo»), su título (el nombre del rack, o «Borrador N») y el sistema. Un clic lo abre. «Nuevo» lleva a su inicio, como en el prototipo de Largueros (ND-06).
- **La barra de borradores, sobre la página «Nuevo»** (un solo bloque):
  - en el inicio de «Nuevo», la lista «Borradores sin insertar de este dibujo», con «Abrir», «Duplicar» y «Descartar el borrador» por fila; un borrador con cambios pregunta en línea antes de descartarse;
  - con un borrador abierto, su tira: título y estado, «Todos los borradores», «Duplicar» y «Descartar el borrador», que también pregunta;
  - debajo, como antes, el editor del borrador abierto (o las tarjetas, en el inicio).
- **«Nuevo» muestra un borrador a la vez.** `NewSelectiveEditor` y `NewDynamicEditor` son ahora el editor del borrador abierto del dibujo, según su sistema; los anfitriones de la vista no cambian. El borrador abierto se recuerda por dibujo: al volver a un dibujo se ve el que tenía abierto.
- **Cada borrador se inserta por separado** con «Insertar en el dibujo…». «Borrador N» es una etiqueta de la ventana y nunca se envía como nombre: sin nombre, el dibujo le da el automático de I-60 (prueba). Insertado, sale de la lista y «Nuevo» vuelve a su inicio; el resto de los borradores sigue igual.
- **«Nuevo sigue al dibujo»:** la columna y la lista solo muestran los borradores del dibujo de la ventana; un borrador de otro dibujo no se escribe en el activo (la regla de siempre) y «Abrir» de un elemento de otro dibujo se rechaza.
- **El consejero de cierre los nombra a todos (D-08):** cada borrador con trabajo sale por su nombre, «Borrador 1 (Selectivo nuevo)», o «Nave A (Borrador 1, Selectivo nuevo)» si su rack tiene nombre (`RackCadDraftNaming.NoticeName`; la etiqueta, desde la revisión, punto 6). Ya no salen dos «un Selectivo nuevo» iguales. La poda de un dibujo cerrado dice lo mismo.
- **Mejoras de la propuesta, construidas** (baratas y coherentes):
  - **renombrar:** el nombre del rack, en la casilla de su editor; la columna y la lista lo siguen, y la etiqueta «Borrador N» no cambia (ND-03);
  - **«Duplicar»:** un borrador nuevo con una copia del último diseño válido, sin nombre, con su propia identidad y con la base de «Modificado» de su original (ND-07);
  - **el límite:** 8 borradores por dibujo, con su texto (ND-04);
  - **la cabecera** cuenta los borradores: «12 racks · todo escrito · 2 borradores sin insertar». Era una observación del smoke de la entrega 2: con un rack nuevo «Modificado» la cabecera decía «todo escrito».
- **El punto de entrada único:** `RackCadWindowViewModel.OpenAsNewDraft(RackCadDraftSeed seed)`, que devuelve `RackCadDraftOpening` (`Opened`, `Reason`, `Draft`). Lo usan «… en esta ventana» (`RackCadDraftSeed.Defaults(kind)`) y «Duplicar».
  - **Utilidades** («Abrir de la biblioteca»): `OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, entry.Name, "del diseño «" + entry.Name + "» de la biblioteca"))`, sobre el `RackProject` que carga la biblioteca y su `RackDesignLibraryEntry`. Da el nombre como el clásico abierto desde la biblioteca: un Selectivo, el de su documento; los demás sistemas, `entry.Name` (corrección 3 de la revisión). `FromProject(project, origin, name)` queda para cualquier otro proyecto, con el nombre que da quien llama.
  - **Cabecera** («Nuevo › Cabecera en esta ventana»): añade su caso en `DraftDoorOf`, `CreateDraftEditor`, `IsLive`, `StateOf`, `CopyOf` y `ForgetDraftEditor` (los seis métodos por sistema, en el mismo archivo, documentado en la clase). Los números, la columna, la lista, el límite y los avisos la siguen solos. `RackCadDraftNaming.SystemLabel(RackSystemKind.Selective)` ya dice «Cabecera».
  - **Rechazos** (dicho en el mensaje de la ventana y devuelto en `Reason`): sin semilla; un sistema sin editor en la ventana («Push Back todavía no tiene editor en la ventana: se abre con su editor clásico.»); sin la puerta que escribe ese sistema; sin dibujo; el límite; y un diseño que no se puede abrir.

**Ediciones a archivos compartidos** (pequeñas y marcadas `T4 new`; lo nuevo va en archivos propios):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`:
  - `HeaderText` pasa a `WithDrafts(RacksHeaderText)`; el cuerpo de antes es `RacksHeaderText`, sin cambios;
  - `NewDynamicEditor` es el editor del borrador abierto, y `ShowNuevoCards` es «sin borrador abierto»;
  - `StartNewDynamic()` llama al punto de entrada (una línea);
  - en `OnSelectiveEditorChanged` y `OnDynamicEditorChanged`, una línea al principio (`RefreshDraftOf`);
  - `SelectiveNameOf` (ahora de instancia) y el nuevo `DynamicNameOf` nombran un borrador por `DraftNoticeName`. `UnwrittenDynamicNames` y la poda de Dinámico usan `DynamicNameOf`, que sustituye a la expresión repetida;
  - en `ShowPage`, una línea: «Nuevo» vuelve a su inicio;
  - en `RaiseNewDynamic`, una línea al principio: `RaiseDrafts()`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Selective.cs`: `NewSelectiveEditor` es el editor del borrador abierto; `ShowNewSelectiveEditor` ya no mira el Dinámico; `StartNewSelective()` llama al punto de entrada (una línea); salen `SecondNewRackText` y `RefuseSecondNewRack`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`, dos sitios marcados `<!-- T4 new (SD-06) -->`:
  1. en la plantilla de las secciones de la columna, la sección queda en un `StackPanel` con, debajo, la lista de borradores, visible solo en «Nuevo». El botón de la sección no se tocó ni se sangró de nuevo;
  2. la barra `DraftsBar`, acoplada arriba en el centro, tras el aviso de espera.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml.cs`: un bloque de seis manejadores tras `OnNewSelectiveHereClick`.
- **Archivos nuevos:**
  - `src/RackCad.UI/VentanaRackCad/RackCadDrafts.cs`: `RackCadDraftSeed`, `RackCadDraftOpening`, `RackCadDraftNaming` (las reglas puras) y `RackCadDraftItem`;
  - `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Drafts.cs`;
  - `tests/RackCad.UI.Tests/Workspace/RackCadNewDraftsTests.cs`.
- **No se tocaron** los editores (`SelectiveEditorViewModel*`, `DynamicEditorViewModel`, sus vistas), Application, el Plugin ni las guardas.

**Las decisiones** (para el Owner las marcadas como supuesto; cada una en un solo sitio):

| Id | Decisión | Dónde se cambia |
|---|---|---|
| ND-01 | Un borrador es el rack nuevo de la ventana de P3 y del paso 4, con todas sus reglas; el registro de borradores va al lado y no las toca | `RackCadWindowViewModel.Drafts.cs` |
| ND-02 | **Supuesto.** El número de un borrador nuevo es el más bajo que no usa ningún borrador vivo de su dibujo («Borrador 1» y «Borrador 3» dan «Borrador 2»; sin borradores se vuelve a «Borrador 1»). La alternativa es numerar siempre hacia arriba | `RackCadDraftNaming.NextNumber` |
| ND-03 | **Supuesto.** «Borrador N» es la etiqueta de la ventana y no cambia; el nombre del rack es el de su editor (renombrar = escribir el nombre), y la ventana lo muestra cuando lo tiene. Nunca se envía «Borrador N» como nombre | `RackCadDraftNaming.Title`, `RackCadDraftItem.Refresh` |
| ND-04 | **Supuesto.** Hasta 8 borradores por dibujo; el noveno se rechaza con «Ya hay 8 borradores sin insertar en este dibujo: inserta o descarta alguno antes de empezar otro.» y se muestra la lista | `RackCadDraftNaming.MaxPerDrawing` |
| ND-05 | **Supuesto (como P3).** Un borrador sin cambios frente a su base no veta el cierre del dibujo, y la poda no lo nombra. Su base son los valores por defecto, el diseño de la biblioteca o, en una copia, la base de su original (ND-07), así que no tiene nada que perder. La copia de un borrador con cambios no está «sin cambios» | `UnwrittenWorkOf` (sin cambio de regla) |
| ND-06 | «Nuevo» en la columna lleva a su inicio, y un borrador se abre desde su elemento, como en el prototipo de Largueros. La sección «Nuevo» sigue marcada mientras un borrador suyo está abierto (no se reconstruyen las secciones, para no destruir el botón del clic) | `ShowPage`, `ShowDraftsHome` |
| ND-07 | «Duplicar» copia el último diseño válido (los tramos escritos y sin aplicar no son diseño), sin nombre (dos racks no comparten nombre por accidente) y con identidad propia. **Corregido en la revisión (punto 1):** la copia conserva la base de «Modificado» de su original. La copia de un borrador con cambios nace «Modificado», así que su trabajo veta el cierre, la poda lo nombra, y «Descartar el borrador» y «Cerrar el editor» preguntan, pase lo que pase con el original. Su «Descartar» vuelve a esa base (los valores por defecto o el diseño de la biblioteca), no a la copia | `DuplicateDraft`, `RackCadDraftSeed.CopyOf`, `TakeDesignAsEdit` |
| ND-08 | «Descartar el borrador» de la lista o de la tira es lo mismo que «Cerrar el editor» de un borrador (el mismo método): con cambios pregunta en línea, y mientras AutoCAD lo escribe no se puede | `DiscardDraft`, `ForgetDraftEditor` |
| ND-09 | Un diseño de la biblioteca toma el nombre que le da el clásico abierto desde la biblioteca (`FromLibrary`). Las holguras vinculadas a variables se abren con el valor literal guardado en el diseño, no con el de la variable, como en el clásico abierto desde la biblioteca (la carga sin el registro), y el editor lo dice (decisión 12, A2-01). Un diseño que no se recompone se rechaza, en lugar de abrir un borrador de solo lectura | `CreateDraftEditor`, `RackCadDraftSeed.FromProject` |
| ND-10 | La barra de borradores va acoplada arriba del centro, como los avisos de la ventana: en el inicio de «Nuevo» queda sobre el título «Nuevo» | `RackCadWindowView.xaml`, `DraftsBar` |

**Propuestas para el Owner, no construidas** («mejora la propuesta»; no son baratas o necesitan una decisión):
- **Borradores que sobreviven** al cierre del dibujo o de AutoCAD (guardados en el dibujo o en un archivo). Hoy viven en la memoria de la ventana, como los borradores de los racks.
- **«Insertar todos»:** cada borrador necesita su colocación con el jig, así que hoy se insertan uno a uno.
- **«Duplicar como borrador» un rack del dibujo** (un RACKDUPLICAR dentro de la ventana): cabe con `OpenAsNewDraft` y la instantánea de su editor, pero un Selectivo del dibujo puede tener holguras vinculadas, que se perderían (ND-09).
- **El texto del «Descartar» del editor de un borrador:** dice «Borrador descartado.» y deja el borrador (vuelve a como se abrió), mientras que «Descartar el borrador» lo quita. Propuesta: «Cambios descartados: el borrador vuelve a como se abrió.». No se cambió para no tocar los editores, que editan otras partes.
- **El nombre previsto** en la lista («se insertará como Selectivo 3»): sigue sin poderse saber sin leer el dibujo (P3-02).
- **«Guardar en la biblioteca» de un borrador** lo construye Utilidades; puede tomar el borrador abierto (`ActiveDraft`).

**Las pruebas:**
- `tests/RackCad.UI.Tests/Workspace/RackCadNewDraftsTests.cs` (nuevo, 16):
  - las reglas de los números y de los nombres;
  - varios borradores numerados, cuál se muestra y la cabecera;
  - el inicio de «Nuevo» y abrir desde el elemento;
  - el elemento sigue al nombre y al estado (y es el mismo objeto);
  - descartar, con y sin cambios, y el número que se libera;
  - un borrador que AutoCAD escribe no se descarta ni se copia;
  - «Duplicar» en Selectivo y en Dinámico;
  - insertar uno deja los demás, con el nombre automático;
  - los borradores son de su dibujo;
  - el veto y la poda nombran a todos los que tienen trabajo;
  - el límite de 8;
  - el punto de entrada con un Selectivo y un Dinámico de la biblioteca (nombre, origen, copia, holguras vinculadas) y sus rechazos;
  - con WPF real (STA) y sin errores de enlace: la columna (orden, nombres de automatización, estado, el clic y «Nuevo») y la barra (lista, tira, «Duplicar», «Todos los borradores», la pregunta en línea con «Cancelar» y «Confirmar», y sin barra en otra página).
- **Pruebas que cambian por SD-06, cada una con su nota en el código:**
  - `RackCadDynamicEditorTests.ANewDynamicIsMadeHereAndInsertedWithTheViewsChosenAsANewRack`: pedido otra vez, ya no muestra el mismo rack (P3-04) sino otro borrador, que la prueba descarta; el aviso dice «Borrador 1 (Dinámico nuevo)»;
  - `RackCadSelectiveInsertTests.ANewSelectivoIsMadeHereAndInsertedWithTheViewsChosenAsANewRack`: lo mismo, con «Borrador 1 (Selectivo nuevo)»;
  - `RackCadSelectiveInsertTests.ANamedNewSelectivoKeepsItsNameAndIsWrittenWithIt`: «Nave A (Selectivo nuevo)», y desde la revisión «Nave A (Borrador 1, Selectivo nuevo)»;
  - `RackCadSelectiveInsertTests.NuevoHoldsOneNewRackOfTheDrawingAtATime` fijaba P3-04. Pasa a `NuevoHoldsDraftsOfBothSystemsAndShowsTheOneThatIsOpen`;
  - `RackCadSelectiveInsertTests.TheCloseOfTheDrawingNamesTheNewSelectivoAndItsPruneSaysWhatWasLost`: el texto entero del veto y de la poda, con «Borrador 1 (Selectivo nuevo)».
- **Censos y guardas:** ninguno se cambió. Los archivos nuevos no usan AutoCAD, el Plugin ni el bloqueo del documento (guarda de capas), y ninguna clase nueva es una ventana.
  - **Lo que encontró la primera Core Full:** 19146 / 1 / 403. La guarda `ProjectVariablesConformanceTests.NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS` prohíbe nombrar el mapa de vínculos fuera de Application, y vio el token en `document.HasPropertyValues`, en `RackCadWindowViewModel.Drafts.cs`.
  - **La corrección:** se cambió por `HasBindingEntry` de las dos holguras (`HasLinkedHolgura`), que es la pregunta del documento, sin tocar la guarda. Ese fallo es su rojo.
  - **Después:** las clases de las guardas (`ProjectVariablesConformanceTests`, `RackCadWindowListTests`, `RackCadWindowCloseAdvisorTests`) dan 69 de 69, y las focales de UI 87 de 87.
- **Comprobación en rojo:** doce piezas rotas a propósito, una a una, sobre el código final. Tras cada una se compiló y corrieron `RackCadNewDraftsTests`, `RackCadSelectiveInsertTests` y `RackCadDynamicEditorTests` (87 casos). Cada pieza hizo fallar al menos una prueba, por su motivo:
  1. el número sin reutilizar el más bajo libre: 3;
  2. `NewSelectiveEditor` como el primer Selectivo nuevo y no el abierto (la regla de P3): 9;
  3. descartar con cambios sin preguntar: 2;
  4. el veto que nombra «un Selectivo nuevo», como antes: 5;
  5. «Duplicar» que copia el nombre: 1;
  6. sin límite por dibujo: 1;
  7. la lista con los borradores de otro dibujo: 2;
  8. el elemento que no sigue al editor: 3;
  9. «Nuevo» que no vuelve a su inicio: 4;
  10. un enlace roto en la columna (el oyente de las trazas de WPF): 3;
  11. la cabecera que no cuenta los borradores: 2;
  12. un borrador insertado que no sale de la lista: 3.

  El script vuelve a escribir cada archivo con sus bytes originales y fecha nueva, y comprueba su SHA-256: los doce, idénticos. Después se recompiló con `--no-incremental`. Script y salida en el scratchpad de la sesión: `hs65\t4\new\red.py` y `red-summary.txt`.
- **Los números**, sobre las fuentes finales recompiladas con `--no-incremental`:
  - **UI completa:** 2772 correctas, 0 con error, 48 omitidas (2820). Es la base `ac3fd1b0` (2756 / 0 / 48) más las 16 nuevas; las pruebas que cambian no añaden casos (una cambia de nombre). Se corrió dos veces, antes y después de la corrección de la guarda, con el mismo resultado.
  - **Core Full:** 19147 correctas, 0 con error, 403 omitidas (19550), sobre las fuentes finales: la base `ac3fd1b0`, porque esta parte no añade pruebas de Core. La corrida anterior a la corrección de la guarda dio 19146 / 1 / 403 (arriba).
  - **Build Debug del Plugin:** 0 errores (las advertencias MSB3277 de las referencias de AutoCAD ya estaban).

**Lo que queda abierto:**
- **Sin AutoCAD:** la columna y la barra en la paleta real (anchos, foco, ratón), y dos borradores insertados seguidos con sus nombres automáticos.
- **Las preguntas al Owner:** ND-02, ND-03, ND-04 y ND-05, y las propuestas de arriba.
- **Para integrar:**
  - Utilidades cambia su método aislado por `OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, entry.Name, "del diseño «" + entry.Name + "» de la biblioteca"))`;
  - Cabecera añade su caso en los seis métodos por sistema;
  - las partes que tocan `RackCadWindowViewModel.cs` y `RackCadWindowView.xaml` tienen arriba la lista de estas ediciones.

### Cuarta tanda, new: correcciones de la revisión

Sobre `c93f65f3` (el commit de la sesión con la parte), en `D:\w4new`. Sin commit, sin push, sin AutoCAD.

| Punto | Qué se hizo |
|---|---|
| 1, MEDIO: pérdida sin aviso | Una copia de «Duplicar» nacía «Limpio» frente a sí misma. Con B1 modificado → «Duplicar» (B2) → B1 descartado, B2 era el único ejemplar y se perdía sin aviso. Ahora la copia **conserva la base de «Modificado» de su original**: se abre sobre esa base (`RackCadDraftSeed.CopyOf(base, copia, origen)`, con la base que da el editor, `BaselineDesignJson`) y toma el diseño copiado como una edición (`TakeDesignAsEdit`). Así está «Modificado» exactamente cuando lo estaba su original, y lo protege todo lo que ya protege un borrador con cambios: el veto del cierre (`UnwrittenWorkOf`), la poda, la pregunta de «Descartar el borrador» y también la de «Cerrar el editor» del propio editor, que una marca en `DraftEntry` no habría alcanzado. Su «Descartar» vuelve a esa base: los valores por defecto, o el diseño de la biblioteca. Se eligió esto sobre la marca porque cierra los cuatro caminos sin tocar las reglas de los editores. ND-05 y ND-07, ajustados |
| 2, BAJO-MEDIO: un aviso que nadie ve | La respuesta de AutoCAD a un borrador que no está a la vista (otro borrador abierto, otra página u otro dibujo) se decía solo en su editor oculto. Ahora se repite también en el mensaje de la ventana, con su etiqueta: «Borrador 1: AutoCAD no insertó el rack nuevo…» (`EchoHiddenDraft`). Pasa con la inserción no colocada de Selectivo y de Dinámico, y con «Dejar de esperar». El borrador a la vista no repite el aviso |
| 3: nombre de un Dinámico de la biblioteca | `RackCadDraftSeed.FromLibrary(project, entryName, origin)` nombra el borrador como lo hace el clásico abierto desde la biblioteca (`EditorModules.OpenFromLibrary`). Un Selectivo toma el nombre de su documento (`LoadForNew`; vacío, el automático de I-60, no el del archivo). Los demás sistemas toman `entry.Name` (`LoadDesignForNew(…, entry.Name, …)`); en un Dinámico es el nombre del archivo, porque su documento no guarda nombre. `FromProject(project, origin, name)` sigue para cualquier proyecto, con el nombre que da quien llama. Lo dice el contrato de las dos fábricas. **La llamada de Utilidades:** `OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, entry.Name, "del diseño «" + entry.Name + "» de la biblioteca"))` |
| 4: texto de las holguras | «Las holguras vinculadas a variables se abren con el valor literal guardado en el diseño, no con el de la variable: un rack nuevo de la ventana no lleva vínculos.» (`LinkedHolgurasText`) |
| 5: otro dibujo | «Abrir», «Duplicar» y «Descartar el borrador» sobre un borrador de otro dibujo dicen «Ese borrador es de otro dibujo: vuelve a ese dibujo para abrirlo.» (y «… para duplicarlo.», «… para descartarlo.»). «Ese borrador ya no está en la ventana.» queda para el que ya no existe (`NotHereText`) |
| 6: nombres repetidos | Un borrador con nombre sale en el veto y en la poda con su etiqueta: «Nave A (Borrador 1, Selectivo nuevo)». Dos borradores con el mismo nombre ya no se leen igual |
| 7: resaltado | El elemento de la columna está marcado como abierto solo mientras se ve «Nuevo» (`SetActive(IsNuevoPage && …)`) |
| 8: excepciones | `DuplicateDraft` está contenido, como las demás acciones públicas: un fallo se dice en el mensaje de la ventana («La ventana tuvo un fallo contenido: …») y devuelve un rechazo |

**Ediciones de esta corrección:**
- **A archivos compartidos:**
  - `DynamicEditorViewModel.cs`: una palabra, `partial`;
  - `RackCadWindowViewModel.cs`: cuatro líneas `EchoHiddenDraft`, dos en `StopWaiting` y dos en `OnDesignWritten`.
- **En los archivos propios:** `RackCadDrafts.cs`, `RackCadWindowViewModel.Drafts.cs` y las pruebas.
- **Archivos nuevos:** `SelectiveEditorViewModel.Drafts.cs` y `DynamicEditorViewModel.Drafts.cs`, los parciales con `BaselineDesignJson` y `TakeDesignAsEdit`.
- **La vista no cambia.**

**Las pruebas:**
- `RackCadNewDraftsTests`, de 16 a 21 pruebas. Las cinco nuevas:
  - el escenario del punto 1, con la pregunta de descartar, la de «Cerrar el editor», el veto y la poda;
  - la copia de un borrador limpio, de uno de la biblioteca y de un Dinámico;
  - el aviso del punto 2 en Selectivo y en Dinámico, con «Dejar de esperar» y sin repetirlo a la vista;
  - el nombre de la biblioteca (punto 3);
  - «Duplicar» contenido (punto 8).
- **Asertos cambiados:**
  - en `RackCadNewDraftsTests`: «Duplicar» (la copia nace «Modificado» y su «Descartar» va a los valores por defecto), los nombres con etiqueta, el texto de las holguras, otro dibujo y el resaltado fuera de «Nuevo»;
  - en `RackCadSelectiveInsertTests.ANamedNewSelectivoKeepsItsNameAndIsWrittenWithIt`: «Nave A (Borrador 1, Selectivo nuevo)».
- **Comprobación en rojo de los puntos 1 y 2:** seis piezas rotas a propósito, una a una, sobre el código final, con las tres clases focales tras cada una (92 casos):
  - R1a: la copia nace sobre su propio diseño, como antes; fallan 3 (las dos del punto 1 y la de «Duplicar»);
  - R1b: la copia de un Dinámico sin la base de su original; falla 1;
  - R2a, R2b y R2c: sin el eco de la respuesta de un Selectivo oculto, de «Dejar de esperar» y de un Dinámico oculto; falla 1 cada una;
  - R2d: el eco también para el borrador a la vista; falla 1.

  El script guarda ahora, antes de cada rotura, una copia del original fuera del worktree. Cada archivo se restauró con el mismo SHA-256. Script y salida: `hs65\t4\new\red2.py` y `red2-summary.txt`, en el scratchpad de la sesión.
- **Un reinicio de la máquina** cortó la primera corrida de esta comprobación y dejó R2c en el código (`if (false) EchoHiddenDraft(editor, …)`). Se encontró en `git diff`, se restauró y se revisaron las otras cinco roturas antes de seguir. Después: las focales y las guardas, y otra vez la comprobación en rojo completa.
- **Los números:** sobre las fuentes finales, recompiladas con `--no-incremental`:
  - `RackCadNewDraftsTests`: 21 de 21;
  - las tres focales: 92 de 92;
  - con la ventana, Selectivo, Dinámico, sus pestañas de seguridad y «Restaurar»: 215 de 215;
  - las guardas de Core que leen las fuentes de la UI (`ProjectVariablesConformanceTests`, `RackCadWindowListTests`, `RackCadWindowCloseAdvisorTests` y `WorkspacePaletteHostSourceGuardTests`): 114 de 114;
  - la vista no cambió, así que no se corrió la UI completa. Con las 5 nuevas, la UI completa debería dar 2777 / 0 / 48, sin medir.

## Cuarta tanda, ins: «Insertar vista…» como panel y los tramos de medio frente (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-ins`, sobre `ac3fd1b0`, worktree `D:\w4ins`. Sin commit, sin push, sin
AutoCAD. Prototipo `dialogos` (DG-01 a DG-11); el Owner aceptó las once propuestas sin nota (`owner-decisions-2026-11-26.md`). No se
borró código clásico: el diálogo «Insertar varias vistas» (`RackViewBatchDialog`) y «Tramos del frente n» (`SelectiveSegmentsWindow`)
siguen en los editores clásicos.

**1. «Insertar vista…» es un panel en la página del editor (DG-01 a DG-08), en Selectivo y en Dinámico.**
- **DG-01.** El botón «Insertar vista…» (o «Insertar en el dibujo…» de un rack nuevo) ya no abre el diálogo clásico como modal de
  AutoCAD (era el supuesto provisional P3-01): abre un panel bajo la cabecera del editor. No es modal: AutoCAD, la ventana y el editor
  siguen libres. El botón queda marcado mientras el panel está abierto (borde de información, estado de UI Automation «panel abierto»).
  «Insertar N vistas» hace la misma escritura de antes (RACKSELECTIVOINSERTAR / RACKSELECTIVONUEVO / RACKDINAMICOINSERTAR /
  RACKDINAMICONUEVO, con la escritura previa del borrador de la decisión 10); «Cancelar» cierra sin escribir. El Plugin no cambia.
- **El panel:** «Insertar vistas de {rack}» (o «Insertar el Selectivo / Dinámico nuevo en el dibujo»), la frase de entrada, a la izquierda
  «Vistas de este rack» en los grupos **Frontal · Lateral · Planta** (una casilla por vista con su nombre corto, «Fondo 1», «Salida»,
  «Poste 3», y su número de orden cuando está marcada), «Todos los postes» / «Ninguno» en los laterales; a la derecha «Orden de
  colocación», con «Subir», «Bajar» (apagados en los extremos) y «Quitar» por fila; abajo «Insertar N vistas», «Cancelar», el motivo y
  las dos notas del prototipo. Por debajo de 560 px de ancho el orden va debajo de las vistas.
- **DG-02.** El orden es el de marcado. Un rack del dibujo abre sin nada marcado; un rack nuevo, con la primera vista de cada grupo
  («Frontal · fondo 1», «Lateral · poste 1», «Planta» en Selectivo). Cada apertura empieza de nuevo; pedido otra vez con el panel abierto,
  se queda como está.
- **DG-03.** Cada vista es una casilla: una vez por pasada.
- **DG-04.** «En el dibujo ya hay: frontal y lateral (según la última lectura de la lista)…»: los tipos de la lista de la ventana, nunca
  de qué fondo o de qué poste (eso es otra entrega).
- **DG-05.** El panel se abre también cuando no se puede insertar, y el motivo va junto a «Insertar N vistas»: rack de solo lectura
  (S-08), tramos sin aplicar (S-21), otra petición de la ventana esperando a AutoCAD (dolor 9), sin borrador válido o con error. El botón
  de la cabecera solo se apaga mientras AutoCAD escribe ese rack (y sin la acción de la ventana). Mientras escribe, el panel se cierra.
- **DG-06.** Sin nombre previsto: «Rack nuevo: se inserta con una identidad nueva y, si no le pones nombre, toma el automático (I-60).»
- **DG-07.** Un solo panel para todos los sistemas (ver «Punto de entrada», abajo). Filtra con la misma política de exposición que el
  diálogo clásico (`RackViewExposure`, `Batch`): la Cama no tiene vistas.
- **DG-08.** El panel sigue al borrador: en cada recomposición vuelve a pedir las vistas que el borrador puede dibujar. Una vista marcada
  que ya no existe queda en el orden, tachada, con «Ya no existe en el borrador: no se insertará», y «Insertar N vistas» no la cuenta ni la
  envía (antes se quitaba sin aviso, A2-02; el filtro del Plugin sigue como defensa).

**2. Los tramos de medio frente (DG-09 a DG-11).**
- **DG-09.** Ya lo hacía S-10: un gesto explícito, «Aplicar tramos». Ahora se apaga si no hay tramos sin aplicar, si hay menos de dos
  (dolor 16: un tramo es un frente completo, para eso está «Sin medio frente») o si el reparto no cabe. Junto al título, la etiqueta
  «Tramos sin aplicar».
- **DG-10.** El dato que el prototipo dejaba [PENDIENTE] existe y el resolvedor ya lo usa (`SelectiveMedioFrente`): cada poste
  intermedio ocupa `2·(troquelX + inicioX)`, el troquel del larguero del poste (sigue a su peralte) y el arranque de la ménsula del
  larguero (INICIO_PERFIL X); **3.5 in con el catálogo de hoy**. `SelectiveSegmentFit` (Application, nuevo) lo lee con las mismas llamadas
  que el BOM y el dibujo, y decide si cabe con `SelectiveMedioFrente.Resolve`, sin regla propia. El «(resto)» lo descuenta; si no queda
  resto, una línea roja lo dice en vivo («No cabe: los tramos suman 98 in y el poste intermedio ocupa ≈ 3.5 in; el frente mide ≈ 100 in y
  no queda resto para el último tramo.»), «Aplicar tramos» se apaga, y `SelectiveDraftEditor.SetSegments` lo rechaza con el mismo texto.
  Con «Todos los fondos» manda el fondo más justo, y el texto lo nombra si el rack tiene más de un fondo. La nota bajo la tabla dice cuánto
  ocupa cada poste.
- **DG-11.** Sobre la tabla, la tira de tramos, de solo lectura: azul (token de información) con largueros, rayada (token de borde) sin
  largueros, gris (texto secundario) cada poste intermedio, con 6 px fijos; cada tramo con su parte del frente (`SegmentStripPanel`, un
  panel proporcional nuevo) y su longitud; borde rojo si no cabe; leyenda «Con largueros · Sin largueros · Poste intermedio»; nombre de UI
  Automation «F2: tramo 1 de 44 in, con largueros; poste intermedio; tramo 2 (resto) de ≈ 52.5 in, sin largueros.».
- **Del prototipo, de paso:** dónde se escribe con dos fondos o más («Se escribe en F2 de los fondos 1 y 2: los fondos de la vista que lo
  tienen.», dolor 12), y lo que dice cada gesto: la semilla, «El tramo 2 deja de ser el resto: escribe su longitud.», «El tramo 1 pasa a
  ser el resto: su longitud (20 in) ya no cuenta.» (dolor 15), «Con un solo tramo…» y «Sin tramos…».

**Punto de entrada para los editores que migren (DG-07: Push Back, Cantilever, Cabecera).**
- `RackViewInsertChoice` (`src/RackCad.Application/VentanaRackCad/RackViewInsertChoice.cs`, puro): las vistas, los grupos en su orden, el
  orden de colocación, «Todos los postes / Ninguno», la semilla y `Follow`. Normaliza solo los órdenes de hoy (los laterales de Push Back al
  revés, la planta de Cantilever antes de las estaciones; prueba).
- `RackViewInsertPanelViewModel` + `RackViewInsertPanelView` (`src/RackCad.UI/VentanaRackCad/`). Un editor:
  1. crea el panel con su `RackSystemKind`, sus vistas como `RackViewInsertOption(Label, Address)`, un `RackViewInsertPanelContext`
     (nombre, rack nuevo, nombre del sistema, cambios, motivo de bloqueo, tipos ya en el dibujo, ayuda de los tres grupos) y las acciones de
     insertar y de cerrar;
  2. llama `Follow(vistas, contexto)` en cada recomposición y `Update(contexto)` en cada cambio de estado; lo cierra al escribir;
  3. aloja `RackViewInsertPanelView` con `DataContext="{Binding InsertPanel}"` (y su evento `Inserting` para llevar antes el nombre).
- Selectivo y Dinámico lo hacen en sus parciales `SelectiveEditorViewModel.Ins.cs` y `DynamicEditorViewModel.Ins.cs`; la ventana les da
  los tipos de la lista y la petición en vuelo (`RackCadWindowViewModel.Ins.cs`).

**Decisiones y supuestos para el Owner** (prefijo IN, aislados):

| Id | Decisión | Dónde se cambia |
|---|---|---|
| IN-01 | El panel va bajo la cabecera, en un `ScrollViewer` de 380 px de alto como máximo, para que las pestañas sigan usables con el panel abierto (en el prototipo la página entera se desplaza) | `InsertPanelScroll` en las dos vistas |
| IN-02 | **Supuesto** (DG-02 para Dinámico): un Dinámico nuevo empieza con la primera vista de cada grupo («Frontal · salida», el primer lateral con corte, «Planta»), la regla del draft de Selectivo generalizada | `RackViewInsertChoice.SeedFirstOfEachGroup` |
| IN-03 | **Supuesto** (DG-04): los tipos salen de la última lectura de la lista (V-10, D-10), y el texto lo dice; tras insertar, la lista no se relee sola y el texto puede quedar viejo hasta «Volver a leer la lista». Sin el rack en la lista: «La lista del dibujo no dice qué vistas tiene ya este rack…» | `RackListViewModel.ViewKindsOf`, `RackViewInsertPanelViewModel.ExistingText` |
| IN-04 | El motivo «otra petición» es el texto de siempre de la puerta ocupada (`DoorBusyText`), sin nombrar el rack que se escribe (el prototipo decía «AutoCAD está escribiendo {rack}») | `RackCadWindowViewModel.Ins.cs`, `OtherRequestText` |
| IN-05 | Sin el puerto modal del host, «Insertar vista…» ahora sí se ofrece: el panel no es un modal de AutoCAD. Dos pruebas fijaban lo contrario y cambian, con nota | — |
| IN-06 | Textos: «ordénalas en «Orden de colocación»» y «márcalas en «Vistas de este rack»» en lugar de «a la derecha / a la izquierda», porque en estrecho el orden va debajo | `RackViewInsertPanelViewModel.Lead`, `EmptyText` |
| IN-07 | Una escritura que la ventana rechaza (borrador de otro dibujo) deja el panel abierto con lo elegido; se cierra cuando AutoCAD empieza a escribir | `RefreshInsertPanel` |
| IN-08 | Abrir el panel cierra la pregunta de «Cerrar el editor» (prototipo). El panel y «Restaurar layout…» pueden estar abiertos a la vez | `OpenInsertPanel` |
| IN-09 | La página de un rack de Push Back, Cantilever, Cabecera o Cama del prototipo, que anunciaba qué vistas tendrá el panel cuando migre su editor, no se construyó: es narración del prototipo. El panel reutilizable sí está | — |
| IN-10 | DG-10 con «Todos los fondos»: el fondo más justo decide; el texto nombra el fondo si el rack tiene más de uno. Una longitud sin escribir cuenta 0 en el resto en vivo, como el diálogo clásico | `SelectiveSegmentFit.Of` |
| IN-11 | **Diferencia declarada con el clásico:** «Tramos del frente n» acepta un reparto que no cabe por los postes (y se dibuja como frente completo sin aviso, dolor 13); la ventana lo rechaza. El clásico no se tocó | `SelectiveDraftEditor.SetSegments` |
| IN-12 | La tira da a un tramo sin longitud un 2.5 % del ancho y «¿?», para que se vea | `SelectiveEditorViewModel.SegmentStrip` |

**Ediciones a archivos compartidos** (pequeñas y marcadas `T4 ins`):
- `src/RackCad.Application/VentanaRackCad/SelectiveDraftEditor.cs`: cinco líneas en `SetSegments` (la comprobación de DG-10, tras la del
  diálogo clásico).
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: una línea en `RaiseState` (`RaiseStateIns()`) y otra en `Rebuild`
  (`RefreshInsertPanel()`); en la región «Medio frente», una línea por gesto (`SegmentGesture`, `SegmentRemovedGesture`, `_segmentHint`),
  `RaiseSegmentsIns()` en `RaiseSegments` y en `OnSegmentRowChanged`, `UpdateRestText` que llama a `RestTextOf()`, y una línea en
  `SegmentNote` (un solo tramo).
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.Insert.cs`: sale `ViewChooser`; `InsertView()` abre el panel;
  `InsertViewBlockedText` ya no pide el elegidor y añade la otra petición.
- `src/RackCad.UI/VentanaRackCad/DynamicEditorViewModel.cs`: la clase pasa a `partial`; sale `ViewChooser`; `InsertView()` abre el panel;
  `InsertViewBlockedText` igual que Selectivo; una línea en `RaiseState` y otra en `Rebuild`.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml(.cs)` y `DynamicEditorView.xaml(.cs)`: el botón «Insertar vista…» (se enciende
  con `CanOpenInsertPanel` y se marca con el panel abierto), el anfitrión del panel tras el último panel en línea de la cabecera (un
  bloque `<!-- T4 ins -->`), y el manejador `OnInsertPanelInserting`; en Dinámico, además, `xmlns:local` en la raíz. En la vista de
  Selectivo, el bloque «Medio frente» del inspector (etiqueta, dónde, tira, línea roja, nota, gesto y «Aplicar tramos» con
  `CanApplySegments`).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: en `Wire` (Dinámico) sale la línea del `ViewChooser` y entra
  `WireInsertPanel(editor)`; una línea en `RaiseWaiting` (`RefreshInsertPanels()`).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Selective.cs`: en `WireSelective`, `WireInsertPanel(editor)` en lugar del
  `ViewChooser`.
- `src/RackCad.UI/VentanaRackCad/RackListViewModel.cs`: `ViewKindsOf(rackId)`, nuevo.
- `src/RackCad.UI/VentanaRackCad/RackCadModalDialogs.cs`: sale `ChooseViews` (la ventana ya no muestra el diálogo).
- `src/RackCad.UI/VentanaRackCad/SelectiveViewChoice.cs`: sale `Choose`; `Options` da `RackViewInsertOption`.
- Archivos nuevos: `RackViewInsertChoice.cs` y `SelectiveSegmentFit.cs` (Application); `RackViewInsertPanelViewModel.cs`,
  `RackViewInsertPanelView.xaml(.cs)`, `SelectiveEditorViewModel.Ins.cs`, `DynamicEditorViewModel.Ins.cs`, `RackCadWindowViewModel.Ins.cs`
  y `SegmentStripPanel.cs` (UI).
- **Censos y guardas:** ninguno falló. El panel es un `UserControl` (fuera del censo de ventanas de ADR-0029) y las guardas de capas de
  `RackCadWindowListTests` pasan sin cambios.

**Las pruebas:**
- Nuevas en Core: `RackViewInsertChoiceTests` (7: los grupos y su orden en los cinco sistemas, la exposición y la Cama, el orden de
  marcado y los gestos imposibles, «Todos los postes / Ninguno», la semilla, el panel que sigue al borrador, los nombres cortos) y
  `SelectiveSegmentFitTests` (5: lo que ocupa un poste es el hueco que deja el resolvedor, el resto es su último tramo, un reparto que el
  clásico aceptaba y el resolvedor dibuja como frente completo se rechaza, varios postes y el fondo, longitudes sin escribir y sin sistema).
- Nuevas en UI: `RackViewInsertPanelTests` (11: un rack del dibujo y uno nuevo, números y gestos, el motivo de DG-05 con tramos y con otra
  petición que llega y se va, solo lectura y sin acción, el panel que sigue al borrador, Dinámico y su rack nuevo, la pregunta de cierre, la
  guarda de que ningún archivo de la ventana muestra el diálogo clásico, y dos de vista con WPF real: casillas por `IToggleProvider`, nombres
  únicos, «Bajar», el motivo, «Insertar 2 vistas», y el panel en estrecho y en el editor de Dinámico) y `SelectiveHalfFrontInsTests` (5: el
  resto y la tira, el reparto que no cabe, los gestos, dónde con dos fondos, y la vista con WPF real: la tira proporcional, el poste de 6 px,
  el borde de error y «Aplicar tramos»). `InsertPanelTestSupport` hace con el panel lo que el elegidor de los dobles hacía con el diálogo.
- Cambiadas, con nota en el código:
  - `RackCadSelectiveInsertTests` y `RackCadDynamicEditorTests`: eligen las vistas con el panel; la prueba del diálogo de Selectivo pasa a
    ser la del panel que lo sustituye; las de «sin el puerto modal no se ofrece» se invierten (IN-05); las de tramos sin aplicar comprueban
    el motivo en el panel.
  - `SelectiveDraftEditorGapTests` y `RackCadSelectiveWindowGapTests`: el resto descuenta los postes (25 → 14.5 in y 55 → 48 in), y «una
    pulgada menos que el frente» ya no cabe (DG-10).
- **Comprobación en rojo:** quince piezas rotas a propósito, una a una, con script (`hs65\t4\ins\red.py` y `red8b.py` en el scratchpad
  de la sesión; salida en `red-out.txt` y `red-out-8b.txt`). Tras cada una se compiló y se corrieron las clases afectadas (en Core,
  `RackViewInsertChoiceTests` y `SelectiveSegmentFitTests`; en UI, las dos nuevas, `RackCadSelectiveInsertTests`,
  `RackCadDynamicEditorTests` y las dos de los huecos):
  1. las vistas en el orden del sistema, sin los grupos: falla la de los grupos;
  2. sin el filtro de exposición: falla la de la exposición y la Cama;
  3. una vista perdida sale del orden (lo de A2-02): fallan la de Core y la de UI de DG-08;
  4. el rack nuevo sin la semilla: fallan la del Selectivo nuevo y la del Dinámico nuevo;
  5. el motivo sin la otra petición en vuelo, y 6. la ventana que no refresca los paneles al cambiar la puerta: falla la de DG-05;
  7. la ventana sin los tipos de la lista: falla la de DG-04;
  8. el panel que no se cierra al escribir, quitado solo en `RefreshInsertPanel`: **sobrevivió**, porque `RaiseState` también lo cierra;
     quitado en los dos caminos (8b): fallan cuatro;
  9. `SetSegments` sin la comprobación de los postes: fallan una de Core y dos de UI;
  10. el resto sin descontar los postes: fallan cuatro de Core y seis de UI;
  11. «Aplicar tramos» sin mirar si cabe: fallan dos;
  12. la tira con los tramos iguales: falla la de la vista;
  13. quitar el último tramo sin decir qué longitud deja de contar: falla la de los gestos;
  14. un archivo de la ventana que vuelve a nombrar `RackViewBatchDialog`: falla la guarda.

  Cada archivo se restauró desde sus bytes, con fecha nueva y el mismo SHA-256 (lo comprobó el script), se buscaron restos y se recompiló
  la solución con `--no-incremental` antes de las suites completas.
- **Los números** (fuentes finales, solución en Debug recompilada con `--no-incremental`):
  - **UI completa:** 2771 correctas, 1 con error, 48 omitidas (2820): la base de `ac3fd1b0` (2756 / 0 / 48, 2804) más las 16 nuevas. La
    que falló es `RackModalHostWindowTests.TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses`, por su temporizador de seguridad de 15 s
    (tardó 19 s: las siete partes corrían sus suites a la vez; la suite duró 58 min). Sola, sus 3 pruebas pasan en 0.9 s. No toca nada de
    esta parte.
  - **Core Full:** 19159 correctas, 0 con error, 403 omitidas (19562): la base (19147 / 0 / 403) más las 12 nuevas.
  - **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias son las MSB3277 de las referencias de AutoCAD y las de los
    analizadores de xunit, que ya estaban; ninguna sale de los archivos de esta parte.

**Lo que queda abierto:**
- **El smoke en AutoCAD:** el panel en la paleta real (ancho, desplazamiento, foco y teclado), la colocación de varias vistas en el orden
  elegido, el rack nuevo con su nombre automático, y la tira con un medio frente aplicado y dibujado.
- **DG-04, la otra entrega:** marcar cada vista exacta que ya está en el dibujo (de qué fondo, de qué poste) necesita leerla del dibujo.
- Push Back, Cantilever y Cabecera usarán el panel cuando migren sus editores (punto de entrada arriba).
- Las preguntas IN-02, IN-03 e IN-04, y la diferencia IN-11.

### Correcciones de la revisión (sobre `b515bafe`)

Sin commit, sin push, sin AutoCAD. La revisión independiente de `b515bafe` dio nueve puntos; están todos corregidos y probados.

| Punto | Qué se hizo | Prueba |
|---|---|---|
| 1 [MEDIO] | Mientras AutoCAD escribe, el panel se oculta pero no se pierde (`_suspendedInsertPanel` en `SelectiveEditorViewModel.Ins.cs` y `DynamicEditorViewModel.Ins.cs`). La ventana llama `WriteHandedOver()` cuando la petición quedó `IsStarted` y cuando llega su respuesta, también durante la llamada (M-22): desde ahí no vuelve. Si la puerta la rechaza (Busy, NoDocument, Failed), `EndWriting` lo devuelve con lo elegido (IN-07). «Cancelar» lo descarta. Un «Dejar de esperar» de una escritura que ya empezó no lo devuelve | `RackViewInsertPanelTests.AWriteTheDoorRefusesBringsThePanelBackWithWhatWasChosenAndAWriteHandedOverDoesNot` (Selectivo y Dinámico; rechazada, empezada con respuesta «no escrito», «Dejar de esperar» y respuesta durante la llamada) |
| 2 [MEDIO, regresión] | «Aplicar tramos» también se ofrece cuando los fondos de la vista no tienen los mismos tramos en el frente (`SegmentsDifferAcrossFondos`), aunque las filas, leídas del primer fondo, no estén pendientes. `_segmentsPending` y «Modificado» no cambian | `SelectiveHalfFrontInsTests.AplicarTramosEqualsTheFondosWhoseTramosOfTheFrontDiffer` (F2 [30, resto] en el fondo 1 y entero en el 2, con «Todos los fondos») |
| 3 [BAJO] | `SelectiveSegmentFit` distingue «sin sistema o sin el frente» (no conocido, no se comprueba) de «el frente existe con ancho ≤ 0» (conocido, no cabe: «No cabe: el frente no tiene ancho (≈ 0 in), así que no se puede partir en tramos.»), y `SetSegments` lo rechaza. La tira pide un frente con ancho (`HasWidth`) | `SelectiveSegmentFitTests.AFrontThatExistsWithNoWidthIsKnownAndDoesNotFit` |
| 4 | `RaiseSegmentsIns` vuelve a calcular el «(resto)»: una recomposición que cambia el ancho o los postes lo actualiza, con las filas escritas | `SelectiveHalfFrontInsTests.TheRestFollowsARecompositionThatChangesTheFront` (el ancho de tarima de todo el rack pasa a 48 in y F2 deja de medir 100 in) |
| 5 | Los números del orden cuentan solo lo que se inserta: `RackViewInsertEntry.Position`, `PositionOf` entre las insertadas, `IndexOf` para la fila; una fila tachada no lleva número ni insignia | `RackViewInsertChoiceTests.ThePanelFollowsTheDraft…` y `RackViewInsertPanelTests.ThePanelFollowsTheDraft…` (la tachada sin número, «1: Planta») |
| 6 | Cada lectura de la lista (`Groups` de `RackListViewModel`) refresca el estado de inserción de los editores, y con él «En el dibujo ya hay…» | `RackViewInsertPanelTests.AReadingOfTheListRefreshesWhatThePanelSaysTheDrawingHas` |
| 7 | «Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.» es un motivo de `InsertViewBlockedText` en Selectivo y en Dinámico, con un `Func` de la ventana (`OtherDrawingText`, como `OtherRequestText`). `Present()` refresca los paneles al cambiar de dibujo | `RackViewInsertPanelTests.ThePanelSaysWhenTheDraftIsOfAnotherDrawingAndFollowsTheChangeOfDrawing`. Las dos de «el rack nuevo pertenece a su dibujo» (`RackCadSelectiveInsertTests`, `RackCadDynamicEditorTests`) comprueban ahora el motivo en el panel, en lugar del mensaje que daba la escritura rechazada, con nota |
| 8 | «Todos los postes» y «Ninguno» se crean solo en el grupo con cortes (un `ContentControl` con la plantilla `CutsButtons`), así que ningún botón oculto repite su nombre | En la prueba de vista del panel, todos los botones, también los ocultos, tienen nombres únicos, y «Marcar todos los postes» sale una vez |
| 9 | Prueba de Core sobre la salida real de `SelectiveFrontalBuilder`: el poste intermedio aparece exactamente cuando `SelectiveSegmentFit` dice que cabe (30 in, y medio pulgar de resto), y el frente queda entero cuando no (medio poste de más, una pulgada menos que el frente) | `SelectiveSegmentFitTests.TheFrontalPlantsTheIntermediatePostExactlyWhenTheSplitFits` |

**Ediciones a archivos compartidos, nuevas:**
- `RackCadWindowViewModel.cs`: una línea en el manejador de `RackList.PropertyChanged` (punto 6), `WriteHandedOver()` en las dos ramas `IsStarted` y en las dos respuestas de `OnDesignWritten` (punto 1), y `Contained(RefreshInsertPanels)` al final de `Present()` (punto 7).
- `SelectiveEditorViewModel.Insert.cs` y `DynamicEditorViewModel.cs`: dos líneas en `InsertViewBlockedText` (el otro dibujo).
- Lo demás está en archivos de esta parte: `RackViewInsertChoice.cs`, `SelectiveSegmentFit.cs`, `RackViewInsertPanelViewModel.cs`, `RackViewInsertPanelView.xaml`, los dos `.Ins.cs` de los editores y `RackCadWindowViewModel.Ins.cs`.

**Comprobación en rojo de los puntos 1 a 3** (script `hs65\t4\ins\red-rev.py` en el scratchpad de la sesión, salida en `red-rev-out.txt`):
cuatro piezas rotas a propósito, una a una; tras cada una se compiló y se corrieron las clases afectadas, y cada una hizo fallar su
prueba:
1. (R1a) el panel que se pierde al escribir, como en `b515bafe`: falla la del punto 1;
2. (R1b) la respuesta que llega durante la llamada sin dar la escritura por entregada (el panel volvía tras escribir): falla la del punto 1;
3. (R2) «Aplicar tramos» solo con tramos pendientes, la regresión: falla la del punto 2;
4. (R3) un frente sin ancho que se salta, «no conocido», y por tanto cabe: falla la del punto 3.

Cada archivo se restauró desde sus bytes, con fecha nueva y el mismo SHA-256 (lo comprobó el script); después se recompiló la solución con
`--no-incremental`.

**Los números:**
- **UI completa:** 2777 correctas, 0 con error, 48 omitidas (2825): las 2772 de `b515bafe` (contando la del temporizador de la ventana
  dueña, que esta vez pasó) más las 5 nuevas (3 en `RackViewInsertPanelTests` y 2 en `SelectiveHalfFrontInsTests`); 38 min.
- **Core, clases focales** (`RackViewInsertChoiceTests`, `SelectiveSegmentFitTests`, `RackCadWindowListTests`, `SelectiveDoorInsertionTests`):
  87 de 87, con las 2 nuevas de `SelectiveSegmentFitTests`. Core Full no se volvió a correr (no lo pide el encargo de correcciones): por
  cuenta, 19161 / 0 / 403.
- **Build Debug de la solución (Plugin incluido), `--no-incremental`:** 0 errores; las advertencias son las de siempre.
- La compilación y las pruebas se repitieron después del reinicio de la máquina, sobre los 16 archivos tal como quedaron en `D:\w4ins`.

## Cuarta tanda, ut: Configuración, Biblioteca de diseños y Ayuda en la ventana (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-ut`, en `ac3fd1b0`, worktree `D:\w4ut`. Sin commit, sin push y sin AutoCAD.
Prototipo «utilidades», UT-01 a UT-12: el Owner aceptó las doce propuestas sin nota (`owner-decisions-2026-11-26.md`). Como el editor
clásico desaparece al final de I-64 (SD-02), aquí entran dos de las seis capacidades del clásico de Selectivo, «Guardar en biblioteca» y
«Abrir de la biblioteca», para Selectivo y Dinámico. El código clásico no se borró: RACKMENU, RACKBIBLIOTECA y RACKAYUDA siguen igual.

**Lo que se construyó:**
- **«Biblioteca de diseños» es una página de la ventana** (`RackCadLibraryPage` y `RackCadLibraryViewModel`), sin ventana aparte. Ya no
  ofrece el botón que abría la biblioteca clásica (RACKBIBLIOTECA).
  - Lee la carpeta al mostrarse, con «Volver a leer la carpeta» y después de guardar; la primera vez que se muestra «Nuevo», también.
  - La lista: Nombre, Sistema y Modificado, de la más reciente a la más antigua. Tiene un buscador por nombre o archivo, sin distinguir
    mayúsculas ni acentos, el filtro «Sistema» con su cuenta por sistema y «Mostrando n de m».
  - **UT-07:** los nombres de los sistemas son los de la columna («Dinámico», «Cama»…). La biblioteca clásica no cambia.
  - **UT-06:** los archivos que no se pueden leer salen en un aviso, con cuántos son y su nombre. `RackDesignLibrary.Read` es la misma
    lectura que `List`; `List` devuelve sus `Entries` y sigue igual.
  - Una carpeta que no existe dice «La carpeta todavía no existe: se crea al guardar el primer diseño.». Si es una carpeta elegida, añade
    cómo volver a la de siempre. Una carpeta vacía dice cómo guardar.
  - El inspector del diseño elegido da el sistema, la fecha y el archivo, qué hace «Abrir» y de dónde sale el nombre. A partir de 620 px va
    al lado de la lista y, por debajo, debajo de ella.
  - «Cambiarla en Configuración» lleva a «Configuración».
- **«Abrir» (UT-03).** Un diseño nunca reemplaza un rack del dibujo.
  - Selectivo y Dinámico se abren como rack nuevo en «Nuevo»: la identidad es nueva y el nombre es el del diseño. El editor dice ««X» se abrió
    como Selectivo nuevo en esta ventana, con una identidad nueva. Todavía no está en el dibujo.». El dibujo no cambia hasta «Insertar en el
    dibujo…», que es la puerta de siempre (RACKSELECTIVONUEVO o RACKDINAMICONUEVO).
  - La llamada está en **un solo método**, `RackCadWindowViewModel.OpenDesignAsNewRack(RackProject, string)`, sobre el mecanismo de hoy: un
    rack nuevo a la vez (P3-04). Si ya hay uno, no abre nada y dice «En «Nuevo» ya hay un rack nuevo sin insertar: insértalo en el dibujo o
    cierra su editor antes de abrir un diseño de la biblioteca.». **Al integrar la parte `new`, la sesión cambia solo esta llamada al punto
    de entrada de los borradores.**
  - Push Back, Cantilever, Cabecera, Cama y Larguero, y también Selectivo y Dinámico si la ventana no tiene su puerta, abren su editor
    clásico aparte, ya con el diseño, sin la lista clásica. Para eso hay una orden nueva, `RACKBIBLIOTECAABRIR`, en su propio archivo:
    - la ventana guarda una petición numerada con el archivo y el dibujo activo **antes** de entregar la orden, y la puerta la suelta si la
      entrega falla;
    - la orden la toma una sola vez. Si se escribe a mano sin petición, si la petición es de otro dibujo o si el archivo ya no existe, solo
      lo dice;
    - abre el editor con la misma ventana dueña invisible y el mismo módulo del registro que RACKBIBLIOTECA (`RackCadLibraryFlows`,
      `RackDesignLibrary.EntryOf`) y dibuja con `DispatchInsertion`.
- **«Guardar en biblioteca…» (UT-04).** Los editores de Selectivo y de Dinámico tienen ahora **«Más»** en la cabecera. Sus entradas se abren
  debajo del botón, en la misma página (sin Popup, por O-D), y la primera es «Guardar en biblioteca…».
  - Lleva a la página de la biblioteca con una línea: el nombre (el del rack), el archivo que resulta y lo que se guarda: «Se guarda lo que
    ves en el editor, borrador incluido (este rack tiene cambios sin escribir). No toca el dibujo.».
  - Un nombre vacío no se guarda. Los caracteres que Windows no admite pasan a «_», y la línea lo dice.
  - Si el archivo ya existe, la línea dice qué es («Es un diseño de Cama guardado el …», o que es un archivo que la biblioteca no pudo leer)
    y ofrece «Reemplazar» o «Cambiar el nombre».
  - Se guarda el último borrador válido. Selectivo se guarda como el clásico, con `SelectiveLibraryExport.FromEffective`: solo literales,
    sin vínculos. Dinámico se guarda con su diseño y su sistema, como «Guardar proyecto» del clásico.
  - Un valor escrito y rechazado detiene el guardado y se nombra, como «Restaurar layout…». También lo detienen unos tramos sin aplicar
    (Selectivo) y un borrador sin recomponer.
  - Si el editor se cierra, su línea de guardado desaparece.
  - Un borrador abierto desde la biblioteca conserva, al volver a guardarse, los campos del archivo que este build no conoce (I-11).
- **«Órdenes y atajos» (UT-09 a UT-11)** es una página de la ventana (`RackCadHelpPage` y `RackCadHelpViewModel`), detrás del botón
  «Ayuda» de la cabecera. No es una sexta sección: `RackCadWindowPage.Ayuda` no está en la columna.
  - Tiene la lista de RACKAYUDA (`RackCommandReference`, la misma fuente), un buscador por orden, atajo o texto (sin acentos) y la cuenta.
  - Cada orden lleva su atajo, o «sin atajo», y «En la ventana», que es un enlace cuando hay una sección que hace lo mismo.
  - «Ejecutar» va por la puerta de la ventana, solo sin comando en curso, y el foco vuelve al dibujo. Solo ejecuta las órdenes de
    `RackCadHelpCommands.Runnable`, que son las de la lista menos RACKCAD. Una prueba comprueba que las dos listas no se separen.
  - **UT-11:** la ayuda gana RACKMENU, RACKPUSHBACK (RPB), RACKSECCION y RACKVARIABLES (RVA). RACKCAD dice lo que hace hoy, y el párrafo ya
    no dice «o cámbialo en la configuración». El diálogo clásico RACKAYUDA muestra lo mismo y no pinta la pastilla de un atajo vacío.
- **«Configuración» (UT-01, UT-02 y UT-12)** es una página propia (`RackCadSettingsPage`). Dice «Ajustes de este usuario en este equipo
  (<ruta>)» y tiene:
  - la biblioteca de bloques, como antes; su «Restablecer» solo se ofrece con una ruta elegida;
  - **la carpeta de diseños,** con «Cambiar…» (el selector de carpetas de Windows, `OpenFolderDialog`), «Restablecer» y su estado:
    elegida o predeterminada, y si todavía no existe;
  - **«Abrir RackCad al iniciar AutoCAD»,** una casilla apagada por defecto (`UserSettings.OpenRackCadOnStartup`);
  - **«Acerca de RackCad»:** la versión (la informativa, con el SHA corto), la carpeta de catálogos y la del registro de errores, en cajas
    de solo lectura que se pueden copiar.
- **La lectura al arrancar (UT-01).** `PluginInitializer.Initialize` llama a `RackCadStartup.Arm()`.
  - Con la casilla apagada no se crea nada, como antes.
  - Con la casilla encendida, un temporizador del despachador del hilo principal espera hasta 2 minutos a que haya un dibujo y entonces
    ejecuta lo mismo que RACKCAD (`new RackMenuCommands().RackCad()`), una sola vez.
- **«Nuevo»** (`RackCadNuevoExtras`) muestra al final:
  - los tres diseños más recientes, con «Ver», que lleva a la biblioteca con el diseño elegido;
  - «Dónde quedó el menú clásico (RACKMENU)», plegado, elemento por elemento (11);
  - «Ver todas las órdenes», que abre la página de la ventana en lugar del diálogo de RACKAYUDA.
- **UT-05:** no se borran ni se renombran diseños; eso se hace en la carpeta. **UT-08:** «Diseñar drive-in» ya no estaba en la ventana.
- `docs/guias/despliegue.md`, sección 6:
  - añade RACKPUSHBACK y RACKSECCION;
  - ya no remite a `RackFrameCommands.Aliases.cs`, que no existe;
  - dice dónde están la carpeta de diseños, la casilla y «Acerca de» en «Configuración».

**Ediciones a archivos compartidos** (lo nuevo va en archivos propios):
- `RackCadWindowViewModel.cs`:
  - el valor `Ayuda` en `RackCadWindowPage`;
  - `editor.SaveToLibraryAction = StartSaveToLibrary` en `Wire`;
  - `ForgetUtilitiesOf(entry.Editor)` en `ForgetSelective` y en `Forget`;
  - `OnUtilityPageShown(page)` al final de `ShowPage`;
  - `RaiseUtilitySettings()` al final de `RaiseSettings`.

  Una línea cada uno, marcada «T4 ut».
- `RackCadWindowViewModel.Selective.cs`: una línea en `WireSelective` (`SaveToLibraryAction`).
- `RackCadWindowView.xaml`: tres bloques marcados `<!-- T4 ut -->`:
  - el botón «Ayuda» de la cabecera;
  - el final de «Nuevo», que sustituye la frase y el botón «Ver todas las órdenes (RACKAYUDA)»;
  - las tres páginas, que sustituyen las de «Biblioteca de diseños» y «Configuración», contiguas, al final de la rejilla.
- `RackCadWindowView.xaml.cs`: salen `OnBrowseBlockLibraryClick` y `OnResetBlockLibraryClick`, que se mudaron con la página.
- `SelectiveEditorView.xaml` y `DynamicEditorView.xaml`: un bloque `<!-- T4 ut -->` («Más») después de «Cerrar el editor».
- `DynamicEditorViewModel.cs` y `RackCadSettingsAccess.cs`: la clase pasa a `partial`.
- `RackDesignLibrary.cs`: `List` pasa por `Read`, y se añade `EntryOf`.
- `UserSettings.cs`: la propiedad `OpenRackCadOnStartup`.
- `RackCommandReference.cs` y `RackCommandHelpWindow.cs`: las entradas y los textos de UT-11.
- `RackCadClassicCommandPort.cs` (Plugin): implementa `IRackCadUtilityCommandPort`, con la petición pendiente y su `Send`, en un bloque
  «T4 ut» al final.
- `PluginInitializer.cs`: la llamada a `Arm()`.
- Archivos nuevos:
  - Application: `RackCadUtilities.cs` y `RackDesignLibraryListing.cs`;
  - UI: los tres modelos de vista y las cuatro vistas de arriba, `RackCadUtilityStyles.xaml`, `RackCadUtilitySettings.cs`,
    `RackCadLibraryFlows.cs`, `RackCadWindowViewModel.Utilities.cs`, `RackCadWindowView.Utilities.cs` y los parciales `.Library.cs` de los
    dos editores y de sus vistas;
  - Plugin: `RackBibliotecaCommands.cs` y `VentanaRackCad/RackCadStartup.cs`.

**Decisiones y supuestos de la sesión** (prefijo UT-S, para el Owner):

| Id | Decisión o supuesto | Por qué |
|---|---|---|
| UT-S01 | «Más» abre sus entradas debajo del botón, dentro de la cabecera, no como menú emergente | Un Popup de WPF cae desplazado con dos monitores de escala distinta (O-D). Las otras partes de la tanda pueden añadir sus entradas en el mismo panel |
| UT-S02 | El nombre que se escribe al guardar es el del archivo y el del diseño: en Selectivo va en el documento, y la biblioteca lista ese nombre. El rack del dibujo no cambia | El prototipo lista el diseño con el nombre escrito. Con el nombre del rack dentro y otro archivo, «Cambiar el nombre» dejaría dos diseños con el mismo nombre |
| UT-S03 | «Cambiar el nombre» propone el primer «nombre (n)» libre, desde 2, no siempre « (2)» | « (2)» podía chocar otra vez |
| UT-S04 | RACKRAMPAQUETE sigue fuera de la ayuda | La pregunta 11 dejaba su resumen [PENDIENTE]. Añadirlo es una línea de `RackCommandReference`, más el par en el censo |
| UT-S05 | La página no muestra las marcas «Propuesta» y «Texto corregido» del prototipo | Eran marcas de la pregunta; el Owner ya la aceptó |
| UT-S06 | «Acerca de» no dice que la ventana recuerda su tamaño y su posición, y la pista de la casilla no dice «donde la dejaste» | No está construido (V-09, C-10) |
| UT-S07 | La lectura al arrancar usa un temporizador del despachador (cada 500 ms, hasta 2 minutos) y ningún evento de AutoCAD | `WorkspaceBridgeSourceGuardTests` exige que el puente del Workspace sea el único suscriptor de los eventos de AutoCAD (`Idle` incluido). Se prueba en AutoCAD en su propio smoke |
| UT-S08 | `RACKBIBLIOTECAABRIR` es una orden nueva, en su propio archivo, y no una variante de RACKBIBLIOTECA | Es la forma de las puertas `RACK…ABRIR`: toma una petición numerada y, escrita a mano, solo avisa. Mueve el censo de órdenes de 51 a 52 |
| UT-S09 | «Guardar en biblioteca…» necesita el editor abierto. Un rack de la lista sin editor no lo ofrece | Se guarda «lo que ves en el editor». Sin editor habría que leer el dibujo |
| UT-S10 | Un diseño abierto desde la ventana conserva sus campos desconocidos (I-11) al volver a guardarse, pero **no al insertarse**: RACKSELECTIVONUEVO y RACKDINAMICONUEVO no llevan el proyecto de origen. Es una diferencia declarada con el clásico | Llevarlo exige cambiar la escritura de la puerta, que es de otra entrega |
| UT-S11 | En un rack nuevo abierto desde la biblioteca, «Descartar» vuelve al diseño de la biblioteca. Los avisos de «Restaurar» y de S-09 siguen diciendo «vuelve a los valores por defecto» (`NewRackDiscardText`) | Es inexacto. Se corrige con la parte `new`, que redefine el borrador nuevo |
| UT-S12 | «En la ventana» de RACKEDITAR dice «Página del rack: «Editar aquí» (Selectivo y Dinámico)» | El prototipo decía «Abrir en editor clásico», un botón que la ventana no tiene |
| UT-S13 | Cada orden de la ayuda es un bloque de dos o tres líneas, no una tabla de cinco columnas | La paleta acoplada no tiene ancho para cinco columnas |
| UT-S14 | Sin la puerta de Selectivo o de Dinámico, un diseño de ese sistema va a su editor clásico | Es la regla de UT-03 para los sistemas que aún no se editan en la ventana |
| UT-S15 | La ventana no muestra `SelectiveTargetFondos` | Es del editor clásico, no del usuario (inventario §2), y el prototipo no lo mostraba |

**Censos y guardas cambiados,** cada uno con su nota:
- `CustomPropertiesHelpCensusTests` (UI): los cuatro pares de UT-11 (`T4UtHelp`). Un atajo vacío es una orden sin atajo, y la barrida del
  Plugin ya no lo busca. Hay dos mutaciones nuevas.
- `CustomPropertiesCommandGuardTests` (Core): `A14UtilityCommands = { "RACKBIBLIOTECAABRIR" }` en la base.
- `SelectiveEditorOpenTests` (Core): el censo bruto de órdenes pasa de 51 a 52.
- `WorkspaceBridgeSourceGuardTests` no se tocó: el primer arranque, con `Application.Idle`, lo rompía, y se rehízo con el temporizador
  (UT-S07).

**Las pruebas:**
- `RackCadUtilitiesTests` (UI, nueva, 34). Usa una carpeta temporal por prueba y no lee ni escribe los ajustes de la máquina.
  - La biblioteca:
    - la lectura, con los nombres de la ventana y el archivo roto;
    - la carpeta que no existe frente a la vacía;
    - el buscador sin acentos y el filtro con sus cuentas;
    - el inspector.
  - «Abrir»:
    - un Selectivo y un Dinámico como rack nuevo, que se insertan después por su puerta;
    - el rechazo con un rack nuevo ya en «Nuevo»;
    - Cama y Larguero por la puerta numerada, y AutoCAD ocupado;
    - sin la puerta de Selectivo o sin la de las utilidades.
  - «Guardar»:
    - el borrador incluido y solo literales;
    - el nombre vacío y los caracteres de Windows;
    - «Reemplazar» y «Cambiar el nombre», también sobre un archivo ilegible;
    - «Cancelar», y el editor cerrado;
    - el valor rechazado;
    - un Dinámico nuevo;
    - un editor sin ventana no lo ofrece.
  - La ayuda:
    - la página fuera de la columna;
    - la lista y sus «En la ventana»;
    - las órdenes ejecutables contra la lista;
    - la búsqueda;
    - «Ejecutar», el ocupado y «En la ventana».
  - «Configuración»: la carpeta, la casilla, «Acerca de», sin las utilidades y el acceso real.
  - «Nuevo»: los recientes y el mapa del menú.
  - Las vistas con WPF real, en STA y sin errores de enlace:
    - la cabecera y la ayuda por clics;
    - la biblioteca: lista, elección, abrir, el guardado desde «Más» del editor y el inspector debajo en estrecho;
    - el conflicto;
    - «Configuración»;
    - «Nuevo»;
    - «Más» de Dinámico.
- `RackCadUtilitiesCoreTests` (Core, nueva, 33):
  - la lectura, la carpeta y `EntryOf`;
  - el nombre del archivo;
  - las órdenes que ejecuta la ayuda, y que todas están registradas en el Plugin;
  - las reglas de la puerta y de la orden;
  - la casilla, que está apagada por defecto, también en un `settings.json` viejo;
  - por la fuente: el orden de la puerta (guarda la petición, entrega la orden, la suelta si falla, devuelve el foco), el de
    RACKBIBLIOTECAABRIR, `Initialize` y el arranque (sin eventos de AutoCAD).
- **Comprobación en rojo:** trece piezas rotas a propósito, una a una, con script (`hs65\t4\ut\red\` del scratchpad de la sesión). Cada una se compiló y pasó por sus pruebas focales, y en todas falla al menos una. Cada archivo se restauró con sus bytes (SHA-256 comprobado):
  1. UT-06, los archivos ilegibles se saltan en silencio: fallan 1 de Core y 2 de UI;
  2. UT-07, la biblioteca vuelve a decir «Sistema dinámico»: fallan 3;
  3. UT-03, un segundo rack nuevo no se rechaza: falla 1;
  4. UT-03, ningún diseño se abre en la ventana: fallan 4;
  5. UT-04, se guarda sin mirar si el archivo existe: fallan 3;
  6. UT-04, el nombre escrito no es el del diseño: fallan 2;
  7. UT-04, el valor rechazado no detiene el guardado: falla 1;
  8. UT-04, el editor cerrado no se lleva su línea: falla 1;
  9. UT-10, «Ejecutar» acepta RACKCAD: fallan 4;
  10. UT-11, RACKMENU sale de la ayuda: fallan 4, con el censo de la ayuda;
  11. la puerta guarda la petición después de entregar la orden: falla 1 (Core, por la fuente);
  12. UT-01, el arranque no mira la casilla: falla 1 (Core);
  13. en la vista, «Más» no abre su panel: falla 1.

  Después se recompiló la solución con `--no-incremental` antes de las suites completas.

**Los números:**
- **UI completa:** 2791 correctas, 1 con error y 48 omitidas (2840). Es la base de `ac3fd1b0`, 2756/0/48, más las 36 nuevas (34 de
  `RackCadUtilitiesTests` y las 2 mutaciones nuevas del censo de la ayuda). La que falla es
  `RackModalHostWindowTests.TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses`, por su temporizador de seguridad de 21 s, con las siete
  partes compilando y probando a la vez (la UI tardó 53 min). Sola pasa: 3/3 en 1 s. Es el caso de M-21; esta parte no toca la ventana dueña.
- **Core Full:** 19180 correctas, 0 con error y 403 omitidas (19583). Es la base, 19147/0/403, más las 33 de `RackCadUtilitiesCoreTests`; los censos cambiados no añaden casos.
- **Build Debug de la solución (Plugin incluido),** con `--no-incremental`: 0 errores. Las únicas advertencias son las MSB3277 de las
  referencias de AutoCAD, que ya estaban.

**Lo que queda abierto:**
- **El smoke propio de UT-01 en AutoCAD:** la casilla encendida abre la ventana con el primer dibujo, una sola vez, también cuando AutoCAD
  arranca sin dibujo y se abre uno después.
- **En AutoCAD:**
  - RACKBIBLIOTECAABRIR con un Push Back y con una Cabecera;
  - el selector de carpetas de Windows dentro de la paleta (la ventana activa es la dueña, como «Examinar…», M-02);
  - «Más» en la paleta acoplada;
  - un diseño de la biblioteca insertado con «Insertar en el dibujo…».
- La integración con `new` (`OpenDesignAsNewRack`), con `ins` y `cb` (sus entradas de «Más» van en el mismo panel) y con `va` y `bom`
  (sus páginas vecinas en `RackCadWindowView.xaml`).
- UT-S10 y UT-S11.

### Cuarta tanda, ut: correcciones de la revisión independiente (sobre `11d60d88`)

**Rama:** la misma, sobre el commit `11d60d88` de la sesión, sin commit, sin push y sin AutoCAD. Un reinicio de la computadora cortó el
trabajo a mitad: se retomó desde el árbol que quedó (5 archivos) y se terminó.

| Punto | Estado | Qué se hizo |
|---|---|---|
| 1 (medio) | CORREGIDO | `RackCadLibrarySaveSource` lleva `Refusal`, un `Func<string>`. Cada editor da el suyo, `SaveRefusal()`: un valor escrito y rechazado (`PendingFieldError` en Selectivo; en Dinámico, el primer campo con error) y después `SaveToLibraryBlockedText`, es decir, tramos sin aplicar, el rack de solo lectura, el borrador sin recomponer o su error. «Más» lo usa, y `Commit` lo vuelve a preguntar antes de construir el diseño, después de comprobar que el editor sigue abierto y antes de mirar el conflicto, también en «Reemplazar». Si se niega, no se escribe nada, la línea se queda y el mensaje dice la causa; corregido el editor, la misma línea guarda |
| 2 (medio-bajo) | CORREGIDO | `RackCadStartup.OnTick` vuelve sin hacer nada si no hay dibujo, o si no hay reposo (`CommandInProgress`, o `!Editor.IsQuiescent`, la guarda de `Send` de las puertas), y lo intenta en el tick siguiente |
| 3 (bajo-medio) | CORREGIDO | Sondea cada 2 s, sin límite, hasta el primer dibujo en reposo. Se detiene al abrir la ventana o con `Disarm`. Sale `MaxTicks`. UT-S07 queda al día (abajo) |
| 4 (bajo-medio) | CORREGIDO, CON UN PENDIENTE | Lo que «Configuración» y «Biblioteca» muestran de los ajustes y del disco va en una caché (`UtilityConfig`): la carpeta de diseños, si está elegida, si existe (`Directory.Exists`), la casilla, la ruta de los ajustes, «Acerca de» y la biblioteca de bloques (ruta, estado, si falta, si está elegida). **Solo se lee** al mostrar «Configuración» (por `RaiseSettings`, que ahora llama a `RefreshUtilitySettings`) o «Biblioteca», y después de «Cambiar…», de «Restablecer» o de la biblioteca de bloques. Al cargar la ventana, al refrescar o al pasar por otra página no se lee nada (prueba con un doble que cuenta las lecturas). La página de Configuración enlaza las copias de la caché (`ConfigBlockLibraryPath`, `ConfigBlockLibraryStatus`, `ConfigBlockLibraryMissing`, `ConfigDesignLibraryPath`), y no las lecturas vivas. La biblioteca muestra la carpeta de su última lectura. La casilla solo vuelve a leer su valor. **Pendiente (UT-S16):** cuando el usuario abre una de esas dos páginas, la lectura (ajustes, existencia de la carpeta y lista de la carpeta) sigue en el hilo de la interfaz. Sacarla exige una lectura asíncrona con el despachador de la paleta y sus pruebas; no es barato |
| 5 | CORREGIDO | En `Read()`, un conflicto solo sigue si su archivo está en la carpeta leída (`_saveConflict &= File.Exists(SavePath())`): cambiar de carpeta, o que se borre el archivo, lo quita |
| 6 | CORREGIDO | `RackCadStartup.Disarm()` detiene y suelta el temporizador; `PluginInitializer.Terminate` la llama |
| 7 | CORREGIDO | `RackCadLibraryFileName`: CON, PRN, AUX, NUL, COM1-9 y LPT1-9 (sin distinguir mayúsculas, con o sin extensión: lo que hay antes del primer punto) llevan «_» (`CON_`, `con_.txt`). La línea lo dice: «(«con» es un nombre que Windows reserva: el archivo lleva «_»)». El documento conserva el nombre escrito |
| 8 | CORREGIDO | (a) La ventana avisa a la biblioteca en cada presentación (`Present` llama a `_library?.RaiseOffers()`), así que «Abrir como nuevo…» y «Abrir en el editor clásico…» siguen al dibujo presentado. (b) «Recientes de la biblioteca» dice por qué no hay ninguno (`RecentEmptyText`): no se leyó todavía, no se pudo leer la carpeta, la carpeta no existe, ningún archivo se pudo leer, o está vacía |

**Supuestos al día:**
- **UT-S07 (sustituye al de arriba):** la lectura al arrancar usa un temporizador del despachador cada 2 s, sin límite, y ningún evento de
  AutoCAD. Abre la ventana una vez, con el primer dibujo en reposo, y se detiene al abrirla o cuando se descarga el plugin.
- **UT-S16 (nuevo):** la lectura del disco al abrir «Configuración» o «Biblioteca» sigue en el hilo de la interfaz (punto 4).
- **UT-S17 (nuevo):** «Nuevo» ya no lee la carpeta la primera vez que se muestra. Los recientes salen de la última lectura de «Biblioteca» en
  esta ventana; antes dicen «Abre «Biblioteca de diseños» para ver los diseños recientes.». Es la consecuencia del punto 4.

**Ediciones a archivos compartidos en esta vuelta:**
- `RackCadWindowViewModel.cs`, dos líneas «T4 ut»: `RaiseSettings` llama ahora a `RefreshUtilitySettings()` (antes, a `RaiseUtilitySettings()`),
  y `Present` llama a `_library?.RaiseOffers()`.
- `PluginInitializer.cs`: `Terminate` llama a `RackCadStartup.Disarm()`.
- Lo demás son archivos de la parte:
  - `RackCadUtilities.cs`, `RackCadLibraryViewModel.cs` y `RackCadWindowViewModel.Utilities.cs`;
  - `SelectiveEditorViewModel.Library.cs` y `DynamicEditorViewModel.Library.cs`;
  - `RackCadSettingsPage.xaml`, `RackCadNuevoExtras.xaml` y `RackCadStartup.cs`.

**Las pruebas:**
- `RackCadUtilitiesTests` (UI), de 34 a 43. Las nuevas:
  - el valor rechazado después de «Más», con «Guardar» y con «Reemplazar»;
  - los tramos sin aplicar;
  - el campo rechazado en Dinámico;
  - nada se lee hasta mostrar «Configuración» o «Biblioteca», con la vista WPF real y el doble que cuenta las lecturas;
  - el conflicto, que no sobrevive a otra carpeta ni al archivo borrado;
  - el nombre reservado;
  - «Abrir», que sigue al dibujo presentado;
  - el texto de los recientes en sus cinco casos.

  Cambian dos, con su nota: el estado de la carpeta después de guardar se ve al volver a mostrar «Configuración», y los recientes de «Nuevo»
  (modelo y vista) aparecen después de leer la biblioteca. El doble de los ajustes cuenta ahora sus lecturas.
- `RackCadUtilitiesCoreTests` (Core), de 33 a 51:
  - los doce nombres reservados y los seis que solo lo parecen (`CONSOLA`, `COM10`, `COM0`, `LPT`, «Nave CON», `A.CON`);
  - la guarda de fuente del arranque, de nuevo: el orden dibujo → reposo → `Stop()` → RACKCAD, 2 s, sin `MaxTicks` ni `_ticks`, y
    `Terminate` → `Disarm` → `Stop` (que suelta el temporizador).
- **Comprobación en rojo** de los puntos 1, 2 y 3. Cada archivo se restauró con sus bytes (SHA-256 comprobado), y después se recompiló la
  solución con `--no-incremental`. Cada pieza rota hace fallar sus pruebas:
  1. sin volver a preguntar al editor en `Commit`: fallan 4 de UI (las del valor rechazado con «Guardar» y con «Reemplazar», la de los
     tramos y la de Dinámico);
  2. sin la guarda de reposo en `OnTick`: falla la guarda de fuente del arranque (Core);
  3. otra vez cada 500 ms, y otra vez con `MaxTicks`: en cada caso falla la misma guarda de fuente.

**Los números:**
- **UI completa:** 2801 correctas, 0 con error y 48 omitidas (2849): las 2840 de la vuelta anterior más las 9 nuevas. Esta vez también pasó
  la de la ventana dueña.
- **Core, las clases focales** (`RackCadUtilitiesCoreTests` con 51, las guardas del Plugin, de la paleta y del puente, `RackCadWindowListTests`
  y los dos censos de órdenes), sobre la solución recompilada: 322/322. La misma selección dio 326/326 antes de la comprobación en rojo, con
  las mismas fuentes y sin fallos en ninguna de las dos; la diferencia está en cuántos casos de teoría descubre xUnit. Core Full no se corrió:
  el encargo pide las focales.
- **Build Debug de la solución (Plugin incluido),** con `--no-incremental`: 0 errores. Las únicas advertencias son las MSB3277 de las
  referencias de AutoCAD, que ya estaban.

## Cuarta tanda, cb: el editor de cabeceras en la ventana

**Rama:** local `architecture/workspace-persistente-rackcad-t4-cb`, en `ac3fd1b0` (worktree `D:\w4cb`), sin commit ni push. Sin AutoCAD.
Prototipo `cabecera` (CB-01 a CB-13) y respuestas del Owner del 2026-11-26 (todas aceptadas; CB-04 y CB-13 con nota).

**Lo que se construyó:**
- **El editor de cabeceras** (CB-01 a CB-13), un solo editor con dos entradas, sin ventanas modales:
  - `CabeceraDraftEditor` (Application, `VentanaRackCad/CabeceraDraftEditor.cs`): el borrador puro sobre una copia de la
    `RackFrameConfiguration`. Lo mantiene normalizado con las reglas del configurador clásico (H1…Hn por elevación, un panel entre cada dos
    horizontales, altura = última horizontal + 4 in de remate), con una diferencia: un panel pertenece a su horizontal de abajo (el
    prototipo), así que mover o agregar horizontales no le cambia los valores a otro panel.
  - `CabeceraEditorViewModel` y `CabeceraEditorView` (UI, `VentanaRackCad/`): las pestañas **Cabecera, Alzado (la inicial), Postes y
    placas y Avanzado**, con los campos de Selectivo y Dinámico (`DynamicFieldItem`, `SelectiveChoiceItem`, la misma forma de confirmar).
- **Cabecera** (CB-02, CB-03, CB-06): «Lo esencial» (Plantilla, Poste, Alto, Profundidad, Peralte del poste). Plantilla y Alto
  regeneran; con cambios propios preguntan en línea «¿Regenerar desde la plantilla?» con la lista de lo que se pierde. Poste,
  profundidad y peralte se escriben sin regenerar. «Calculado» (altura del poste, paneles con y sin celosía, horizontales y dobles,
  profundidad, lo que da la plantilla), «Cambios respecto al estándar» por pieza y en español, y «Comprobación» con todos los avisos.
- **Alzado** (CB-04, CB-12): el marco de lado, a escala, es el selector: clic, Mayús+clic (rango del mismo tipo), Ctrl+clic, y
  «Paneles / Horizontales / Todo». Inspector del panel: Arreglo, Dirección, Perfil de la diagonal, **Claro** (nota del Owner), las
  conexiones filtradas por su papel, «Dividir en dos», «Combinar con el de arriba», «Volver al estándar». Inspector de la horizontal:
  Elevación (una) o Desplazar (varias), Perfil, Cantidad, Notas, «Duplicar arriba», «Eliminar». «Agregar panel arriba» con «Claro del
  panel nuevo» (44 in por defecto).
- **Postes y placas** (CB-07): «Igual en los dos postes» encendido si son iguales; apagado, dos columnas. Al encenderlo con postes
  distintos, el derecho toma lo del izquierdo (lo dice).
- **Avanzado**: los tres valores de celosía en troqueles, y «Del modelo, sin control» de solo lectura (paso, claro estándar, remate,
  diagonal doble, horizontal doble).
- **«Restaurar estándar…»** (CB-05) en línea, con las casillas Horizontales, Paneles, Postes y refuerzos, Placas y Celosía, su recuento,
  «Ver lo que se pierde» y «Restaurar lo marcado». Vuelve a la plantilla con el alto con que se generó, el poste y la profundidad de
  ahora. «Descartar» vuelve a lo abierto.
- **Dentro de Selectivo** (CB-01 B, CB-08, CB-09, CB-10): las tres capacidades del clásico de Selectivo que faltaban.
  - En «Avanzado» › «Postes y cabeceras», cada poste lleva su cabecera por fondo («Calculada» o «Personalizada») con **«Editar
    cabecera…»** y, si es personalizada, **«Volver a la calculada»** (hoy «Restablecer poste»).
  - «Editar cabecera…» abre el editor de cabeceras en el lugar del editor de Selectivo, sobre una copia (la semilla de «Personalizar»
    del clásico). La profundidad es la del fondo, bloqueada y con su origen; el peralte es el propio del poste (vacío = el del rack).
  - La cabecera de la ventana es «‹ Selectivo A › Cabecera de P2 · Fondo 1», con «Aplicar al borrador», «Copiar a otros postes…»,
    «Descartar cambios» y «Restaurar estándar…». Nada aquí escribe en AutoCAD.
  - **«Aplicar al borrador»** pasa por el protocolo del clásico (I-53: `SelectiveHeaderBatchPlanner` y `ApplyHeaderBatch`), que da a
    cada copia la profundidad de su fondo y el peralte de su poste y revisa la altura. Por debajo del nivel de carga pregunta en línea
    («Cabecera demasiado baja» › «Aplicar igual»); si solo difiere del alto resuelto, lo dice al aplicar.
  - **«Copiar a otros postes…»**: casillas por poste y fondo, «Todos» y «Ninguno», «Aplicar aquí y copiar a n destinos». Todo se
    prepara antes de escribir nada: si un destino no vale, no se copia nada.
  - **«‹ Selectivo A»** con cambios sin aplicar pregunta «¿Volver sin aplicar?» (Aplicar y volver / Volver sin aplicar / Seguir editando).
  - Una cabecera con cambios sin aplicar es trabajo sin escribir del rack: el Selectivo queda «Modificado» y el dibujo no se cierra (D-08).
- **La cabecera suelta** (CB-01 A): «Editar aquí» de un rack «Cabecera N» de la lista, con su puerta numerada.
  - `RACKCABECERAABRIR` (sin marca de Deshacer ni historial) y `RACKCABECERAAPLICAR` (`RackCadCabeceraDoorCommands.cs`), con las reglas
    de las otras puertas: número de petición, otro dibujo, «Dejar de esperar», `RequestStarted`, la comparación de las vistas hermanas
    (M-02), y la respuesta de la escritura con `RackDoorRules.AnswerForApply`.
  - Las reglas puras: `CabeceraDoorRules.AnswerForOpen` y `PlanApply` (Application, `CabeceraDoor.cs`), con `IRackCadCabeceraDoorPort`,
    `CabeceraDoorWrite` y `CabeceraDesignRead`.
  - La mitad de dibujo `RackCabeceraCommands.ApplyCabeceraUpdate` (archivo parcial nuevo): redibuja en sitio la lateral y la planta con
    el mismo GUID, como «Actualizar» de RACKEDITAR.
  - En la ventana: la oferta «Editar aquí», el editor en la página del rack, «Actualizar» con una copia, el editor bloqueado mientras
    AutoCAD escribe, «Limpio» solo si se redibujó alguna vista, el borrador de otro dibujo que no se escribe, la poda de un dibujo
    cerrado, la marca de la lista y el veto de cierre (D-08).

**CB-04, el claro del panel (nota del Owner: «falta una opción para cambiar el claro del panel»).** Investigado en el dominio:
- **El claro de un panel no es un dato.** Es la distancia entre sus dos horizontales: `BracingPanel.ClearHeight => EndElevation −
  StartElevation`, y esas elevaciones se copian de las horizontales en cada normalización.
- **`RackFrameConfiguration.PanelClear` (44 in) sí existe y se guarda** (`RackFrameProjectDocument.PanelClear`), pero no se puede
  cambiar: `RackFrameConfigurationFactory.Build` crea una configuración nueva y genera siempre con su valor por defecto, y solo «Agregar
  horizontal» y «Duplicar» del clásico lo leen.
- **Lo construido:** el campo «Claro» del inspector del panel. Escribirlo mueve la horizontal de arriba del panel y todo lo que hay
  encima en la diferencia, desde el panel más bajo elegido, así que los demás paneles conservan su claro y el poste crece o baja (el
  remate sigue en la última horizontal). Con varios paneles, los iguala.
- **Propuesta, sin construir (supuesto CB4-02):** si el Owner quiere un claro estándar propio por cabecera, que el factory reciba
  `PanelClear` (hoy fijo en 44) y que «Lo esencial» lo ofrezca como un dato más que regenera. Es un cambio de cálculo; se declara aquí.

**CB-13, la cara de montaje (nota del Owner: «no sirve para nada»).** La ventana no la ofrece: ni en el panel, ni en la horizontal, ni
en la leyenda. El dato no se borra y vive en:
- `BracingPanel.MountingFace` y `FrameHorizontal.MountingFace` (`src/RackCad.Domain/RackFrames/`), tipo `FrameSide` (Front, Back, Both);
- `RackFrameProjectDocument` (se guarda), `AuthoredTypedValues` (compara diseños), `RackFrameConfigurationFactory` (siempre Front);
- `BracingPanelMemberBuilder.GetPhysicalFaces`: **«Both» genera las diagonales dos veces** (una por cara), así que en un dibujo que ya la
  tenga, el modelo físico y lo que cuente el BOM de miembros las duplican. La ventana conserva el valor guardado; «Restaurar estándar…» y
  «Volver al estándar» lo devuelven a Front, porque copian el panel de la plantilla.
- y en el configurador clásico (`BracingSegmentEditorRow`, `HorizontalEditorRow`, `RackFrameConfiguratorViewModel`, su vista previa).

**Las decisiones (prefijo CB4, para no chocar con las de las otras partes):**

| Id | Decisión | Por qué |
|---|---|---|
| CB4-01 | Un panel pertenece a su horizontal de abajo (el prototipo, `rebuildPs`), no al par de horizontales del clásico. Dividir da a las dos mitades los valores del panel; combinar conserva los del de abajo; un panel agregado arriba es estándar (diagonal simple) | Con la regla del clásico, un panel modificado deja de contar como tal tras otro cambio de horizontales (inventario, dolor 7) |
| CB4-02 | El claro del panel se escribe moviendo lo de arriba (ver arriba). El claro estándar (`PanelClear`) queda de solo lectura en «Avanzado» | El dominio no tiene el claro como dato; el estándar no lo usa el factory |
| CB4-03 | La plantilla de una cabecera abierta se toma de `StandardBaselineId` («TEMPLATE-id»); si no está en el catálogo, la estándar. El alto con que se generó, que el modelo no guarda, es el alto al abrir; «Alto» y «Plantilla» lo cambian | Es lo único que el dibujo guarda de la plantilla |
| CB4-04 | Dentro de Selectivo, el peralte vacío es el del rack, y el campo muestra vacío cuando la cabecera tiene el del rack. La copia guarda el peralte efectivo, como el clásico, y el protocolo decide qué escribe en «Peralte propio» del poste (cero si es el del rack) | El clásico siembra con el peralte efectivo del poste; así abrir y aplicar sin tocar no cambia el poste |
| CB4-05 | «Aplicar al borrador» es el EDIT del protocolo de I-53 sobre ese poste y ese fondo solamente (el alcance de la tabla, no el selector de fondos). La copia a otros postes es el mismo EDIT en cada destino, con el peralte del poste destino: así no cambia el peralte propio de ningún destino, como DISTRIBUTE del clásico, y todo se prepara antes de escribir | La casilla del prototipo es un par poste × fondo; DISTRIBUTE cruza todos los postes con todos los fondos |
| CB4-06 | «Volver a la calculada» quita la personalizada de ese poste en ese fondo y no toca el peralte propio del poste | El peralte propio es de todo el rack y se escribe en su columna. En el clásico, «Restablecer poste» con todos los fondos también lo borraba |
| CB4-07 | Mientras el editor de una cabecera está abierto, toma el lugar del editor de Selectivo (la vista del editor cambia su contenido) y el de Selectivo conserva su estado debajo | El prototipo: «‹ Selectivo 2 › Cabecera de P2 · Fondo 1» |
| CB4-08 | Una cabecera con cambios sin aplicar deja el Selectivo «Modificado» (una línea en `RefreshDirty`) | D-08: si no, cerrar el dibujo perdería esos cambios sin aviso |
| CB4-09 | `ApplyCabeceraUpdate` es una COPIA de la rama `UpdateOnly` de `EditCabecera`, no una extracción, para no cambiar RACKEDITAR en esta tanda. Una guarda de fuente en Core comprueba que las dos hacen los mismos pasos en el mismo orden | Las mitades de Dinámico y Selectivo se extrajeron con el mismo cuerpo; aquí extraer exigía reordenar `EditCabecera`, que mezcla la actualización con la inserción |
| CB4-10 | Los textos de la cabecera se dan en español y con acentos (la ventana escribe sus propios avisos en «Comprobación»; de `FrameModelValidator` solo toma los de ids que el catálogo no conoce) | Inventario, dolor 6 |
| CB4-11 | «Estado físico» de la horizontal no se ofrece (CB-12); «Notas» sí | Pregunta 12 aceptada |

**Supuestos para el Owner** (aislados):
- CB4-03 (la plantilla y el alto de una cabecera abierta): `CabeceraDraftEditor.TemplateIdOf` y el constructor.
- CB4-05 (el alcance de «Aplicar al borrador» y de la copia): `SelectiveDraftEditor.WriteCabecera`.
- CB4-06 (el peralte en «Volver a la calculada»): `SelectiveDraftEditor.ResetCabeceraAt`.
- Los textos de los avisos y de las preguntas en línea, del prototipo donde los hay; los demás, provisionales.

**Lo que falta (no se construyó en esta tanda):**
- **Dinámico** › «Módulos» › «Editar cabecera…» y «Copiar a otros módulos…» (CB-01 B, CB-08: profundidad = longitud del módulo y peralte
  del rack, los dos bloqueados; `CabeceraDraftContext` ya admite el peralte bloqueado). Dinámico tiene su propio protocolo de lotes
  (`DynamicHeaderBatchState`) y su nota sigue diciendo que la cabecera se edita en RACKEDITAR.
- **«Guardar como plantilla…»** (CB-11) y el cambio de cálculo «la plantilla propia repite la forma»: el formato de plantilla
  (`RackFrameTemplate`) no guarda los paneles uno por uno, así que repetir la forma exige ampliarlo. Hoy la lista de plantillas ya
  incluye las propias de `%APPDATA%\RackCad\user-templates.json`, marcadas «(propia)», y usarlas regenera como hoy.
- **«Insertar vista…» de la cabecera suelta**: espera el panel reutilizable de la parte `ins` (DG-07). «Nuevo › Cabecera en esta
  ventana» espera los borradores de la parte `new` (fuera de esta tanda por el encargo).
- La marca de error en la pestaña («punto rojo»): un valor no válido no llega al borrador y se queda en su campo con el motivo, pero la
  pestaña no se marca.
- El editor de cabeceras clásico sigue en RACKEDITAR, en «Nuevo» (tarjeta «Cabecera»), en Push Back y en Dinámico.

**Ediciones a archivos compartidos** (pequeñas y marcadas `T4 cb`):
- `src/RackCad.Application/VentanaRackCad/SelectiveDraftEditor.cs`: la clase pasa a `partial` (una palabra).
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: `SelectivePostRow` pasa a `partial`; una línea en `Rebuild`
  (`RefreshCabeceraCells();`) y una en `RefreshDirty` (`|| CabeceraPending`).
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: un bloque (`T4 cb (begin)` … `(end)`) en «Avanzado» › «Postes y cabeceras»:
  la celda «Cabecera» de cada poste (ahora con su cabecera por fondo y los dos botones) y la nota de debajo, que ya no dice «usa
  RACKEDITAR».
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml.cs`: una línea en el constructor (`WireCabecera();`).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: una línea en el constructor (`WireCabeceraDoor(door);`); `IsCabecera` en
  `DoorRequest`; `ShowPanel` y `HasRackEditor` cuentan el editor de cabeceras; una rama en `StopWaiting`, `OnDesignWritten`,
  `OnRequestStarted` y `DraftStateOf`; una condición en `OnDesignRead`; una línea en `UnwrittenWorkOf`, `PruneDeadSessions` y
  `RaiseDynamic`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: un bloque, la oferta `CabeceraOfferPanel` con «Editar aquí», tras la de Selectivo.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml.cs`: dos nombres en `OnViewModelPropertyChanged` y una línea en `ShowRackContent`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: la clase pasa a `partial` y una línea en `Abandon`.
- `src/RackCad.Plugin/RackCabeceraCommands.cs`: la clase pasa a `partial` (una palabra); `EditCabecera` no cambia.
- Archivos nuevos: `CabeceraDraftEditor.cs`, `CabeceraDoor.cs`, `SelectiveDraftEditor.Cabecera.cs` (Application);
  `CabeceraEditorViewModel.cs`, `CabeceraEditorView.xaml(.cs)`, `SelectiveEditorViewModel.Cabecera.cs`, `SelectiveEditorView.Cabecera.cs`,
  `RackCadWindowViewModel.Cabecera.cs`, `RackCadWindowView.Cabecera.cs` (UI); `RackCadCabeceraDoorCommands.cs`,
  `RackCadRackDoorPort.Cabecera.cs`, `RackCabeceraCommands.Door.cs` (Plugin); y las pruebas.

**Censos y guardas que cambiaron por esta parte** (cada uno con su nota en la prueba):
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 51 a 53 (RACKCABECERAABRIR, RACKCABECERAAPLICAR).
- `CustomPropertiesCommandGuardTests`: `A14CabeceraDoorCommands` entra en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: las órdenes de la puerta de la cabecera
  (`RackCadCabeceraDoorCommands.cs`) leen y escriben el dibujo en su contexto de orden, como las de la puerta a un rack; y
  `ApplyCabeceraUpdate` entra en la lista de lo que ningún otro archivo de la puerta puede llamar.
- Dos guardas globales de nombres fallaron con la primera versión y se corrigió el código, no la guarda:
  `WorkspaceIdleSchedulingGuardTests` (G10: `.SetPost(` una sola vez en `src`) y `WorkspacePilotReconciliationGuardTests` (`Update()`
  solo en el campo del piloto). `CabeceraDraftEditor.SetPost` pasó a `SetPostProfile` y el `Update()` del editor de cabeceras a
  `RequestWrite()`, como hizo Dinámico (B-01 de su primera revisión).

**Las pruebas:**
- `tests/RackCad.UI.Tests/Workspace/CabeceraEditorTests.cs` (nuevo, 21):
  - el borrador: la cabecera estándar normalizada y sin cambios; el claro del panel (CB-04) con uno y dos paneles; los cambios contra
    la plantilla y «Restaurar estándar» por casillas; el alto y la plantilla que regeneran y dicen antes lo que se pierde; dividir,
    combinar, agregar arriba, mover, desplazar, duplicar y eliminar, con sus rechazos; «Descartar» y lo escrito como base; la
    profundidad y el peralte dentro de un rack; «Igual en los dos postes»; «Comprobación» en español con la altura frente al rack;
  - el modelo de la vista: abre en «Alzado» con las filas de arriba abajo y el primer panel elegido, clic, Mayús y Ctrl; sin cara de
    montaje y con «Claro» (CB-13, CB-04); la pregunta de regenerar y su cancelación; el panel de «Restaurar estándar…»; la cabecera
    suelta («Actualizar» solo con cambios, bloqueada mientras escribe, «Limpio» al escribir, «Cerrar el editor» con su pregunta);
  - dentro de Selectivo: la cabecera de cada poste y fondo con su profundidad; «Aplicar al borrador» deja el rack «Modificado» y
    «Volver a la calculada» lo limpia; la pregunta de la altura bajo el nivel de carga; la copia a otros postes con la profundidad de su
    fondo y el peralte de su poste; «‹ Selectivo» con cambios pregunta;
  - con WPF real (STA) y sin errores de enlace: las cuatro pestañas, las filas del alzado con sus nombres, el clic en una fila, los
    nombres únicos de los botones; y el editor de la cabecera que toma el lugar del de Selectivo y lo devuelve.
- `tests/RackCad.UI.Tests/Workspace/RackCadCabeceraWindowTests.cs` (nuevo, 7), con una puerta doble que implementa
  `IRackCadCabeceraDoorPort`: la oferta solo con esa puerta y la respuesta solo de su petición; la regla de la puerta; «Actualizar» con
  una copia, el editor bloqueado, la orden que tomó la petición, la respuesta no escrita y la escrita, el veto de cierre (D-08); «Dejar
  de esperar» y la respuesta tardía; el borrador de otro dibujo y la poda; «Cerrar el editor» y la marca de la lista; y la página del
  rack que aloja la vista (STA).
- `tests/RackCad.Tests/Workspace/CabeceraDoorTests.cs` (Core, nuevo, 7): las reglas (`AnswerForOpen`, `PlanApply`) y el cableado sobre la
  fuente (las dos órdenes en su orden, la comparación de hermanas antes de `blocks[0]`, «escrito» solo por la regla, la mitad de dibujo
  con los mismos pasos que la rama `UpdateOnly` de `EditCabecera`, y los huecos de la puerta con su `Abandon`).
- **Comprobación en rojo:** doce piezas rotas a propósito, una a una, con script (`hs65\t4\cb\red.py` en el scratchpad de la sesión;
  cada archivo vuelve a sus bytes con el mismo SHA-256, lo comprueba el script). En todas falla al menos una prueba:
  1. el claro mueve solo la horizontal de arriba: fallan 2;
  2. el panel ofrece la cara de montaje: falla 1;
  3. la cabecera sin aplicar no cuenta en «Modificado» (D-08): falla 1;
  4. la altura severa no pregunta: falla 1;
  5. la copia no toma el peralte de su poste: falla 1;
  6. «Restaurar» ignora la casilla Paneles: fallan 2;
  7. el alto regenera sin preguntar: falla 1;
  8. la respuesta de la escritura no llega a la cabecera: falla 1;
  9. el consejero de cierre no cuenta las cabeceras: falla 1;
  10. la página del rack no aloja la cabecera: falla 1;
  11. la puerta abre cualquier tipo (Core): falla 1;
  12. la mitad de dibujo sin el nombre de la planta (Core): falla 1.

**Los números** (fuentes finales, solución en Debug, después de la comprobación en rojo):
- **UI completa:** 2784 correctas, 0 con error, 48 omitidas (2832): la base de `ac3fd1b0` (2756 / 0 / 48) más las 28 nuevas.
- **Core Full:** 19154 correctas, 0 con error, 403 omitidas (19557): la base (19147 / 0 / 403) más las 7 de `CabeceraDoorTests`.
- **Build Debug de la solución (Plugin incluido):** 0 errores; las 2 advertencias son las MSB3277 de las referencias de AutoCAD, que ya estaban.
- Focales de la ventana, Selectivo y Dinámico (`Cabecera*`, `RackCadSelective*`, `SelectiveSafetyTab*`, `SelectiveRestorePanel*`,
  `RackCadWindowTests`, `RackCadDynamicEditorTests`): 295 de 295.

**Lo que queda abierto para el smoke del Owner en AutoCAD:**
- que `RACKCABECERAABRIR` no deje nada en Deshacer;
- `RACKCABECERAAPLICAR` con lateral y planta, y su «U»;
- «Editar cabecera…» → «Aplicar al borrador» → «Actualizar» de un Selectivo, y la personalizada en el dibujo;
- el veto de cierre con una cabecera sin aplicar;
- el editor de cabeceras en la paleta (anchos, foco y ratón en el alzado).

### Correcciones de la revisión

Sobre `a2544580` (el commit de esta parte), sin commit ni push y sin AutoCAD. Son los siete puntos de la revisión independiente. Cada uno
tiene su prueba; los cuatro primeros, además, su comprobación en rojo.

| # | Hallazgo | Qué se hizo |
|---|---|---|
| 1 | CONFIRMADO: «Copiar a otros postes…» al mismo poste en otro fondo devolvía a 0 el peralte propio recién editado (la copia llevaba el peralte viejo del poste y su EDIT lo escribía después del origen) | Un destino con el mismo poste que el editado toma el peralte editado (`SelectiveDraftEditor.WriteCabecera`); los demás destinos siguen tomando el de su poste |
| 2 | «Copiar a otros postes…» convertía una cabecera calculada y sin cambios en personalizada, en el origen y en todos los destinos | Se copia solo con cambios o desde una personalizada (`CanCopy`: `IsDirty \|\| _wasCustom`), como «Reutilizar» del clásico, que rechaza un origen estándar (SourceUnusable). El motivo va junto al botón (`CopyBlockedText`) |
| 3 | «Alto» y «Plantilla» regeneraban sin preguntar y perdían las «Notas» de las horizontales y la cara de montaje | Las notas se conservan en las horizontales cuya elevación queda (también al restaurar las horizontales). Una nota cuya elevación no queda y una cara distinta de la frontal cuentan como pérdidas, así que regenerar pregunta y las lista (`RegenerationLossesForTemplate` y `RegenerationLossesForHeight`) |
| 4 | «Restaurar estándar…» con solo «Horizontales» perdía valores de paneles que «Ver lo que se pierde» no listaba | `RestoreLosses` lista los paneles cuyo tramo no queda («P1: su tramo de 4 a 54 in no queda; pierde celosía en X y toma los valores de la plantilla»), salvo si el panel de la plantilla que ocupa su inicio tiene sus mismos valores. Lista también las notas que se pierden y las caras que vuelven a la frontal |
| 5 | «Claro» y «Agregar panel arriba» aceptaban cualquier valor mayor que 0 | El claro mínimo de un panel es `CabeceraDraftEditor.MinimumClear` (4 in), con su motivo en el campo y bajo «Agregar panel arriba». «Elevación», «Desplazar» y «Duplicar arriba» no dejan una horizontal a menos de 4 in de otra. «Dividir en dos» exige mitades de 4 in como mínimo (supuesto CB4-12) |
| 6 | La semilla de un poste calculado salía de la plantilla interna y el estándar del editor de `header-templates.json` más las propias | `CabeceraSeed(fondo, poste, plantillas)` siembra con las plantillas del propio editor (la «STD-3P» de esa lista); el editor de Selectivo carga la lista una vez y la pasa a la semilla y al editor |
| 7 | El nombre no seguía la regla del clásico | Se copia la regla de `EditCabecera`: el nombre interior no se toca (AUTH-15 lo admite en blanco). La lectura ya no pone el nombre del sobre dentro de la cabecera, la ventana envía el nombre interior tal cual y la orden nombra los bloques con el interior o, si está vacío, con el del sobre. La base de «Limpio» es el JSON que se envió (`MarkWritten(…, sentJson)`) |

**Decisión nueva:**

| Id | Decisión | Por qué |
|---|---|---|
| CB4-12 | El claro mínimo de un panel es 4 in, el de «Dividir en dos», y ahora cada mitad de un panel dividido debe tener al menos 4 in (antes bastaba con que el panel tuviera 4 in, y las mitades podían quedar de 2 in). Un marco que la plantilla generó con un panel de cierre más corto se conserva: solo se rechaza una escritura | Corrección 5: un solo mínimo para todos los gestos |

**Pruebas:**
- `tests/RackCad.UI.Tests/Workspace/CabeceraEditorReviewTests.cs` (nuevo, 8): la copia al mismo poste en otro fondo (1); la calculada sin
  cambios que no se copia y la personalizada que sí (2); las notas que se conservan y la que se pregunta al regenerar, y la cara como
  pérdida de regenerar y de restaurar (3); los paneles que lista «Restaurar» con solo «Horizontales» y las notas conservadas al restaurar
  (4); el claro mínimo en el borrador, en el campo y bajo «Agregar panel arriba», con «Elevación», «Desplazar» y «Dividir» (5); la semilla
  y el estándar de las mismas plantillas (6).
- `RackCadCabeceraWindowTests` (+1, 7): una cabecera con el nombre interior en blanco lo conserva en la lectura y en la escritura, el
  veto de cierre la nombra con el nombre de la lista y, escrita, queda «Limpio» sobre lo enviado.
- `CabeceraDoorTests` (Core, 7, con dos cambiadas): la lectura no toca el nombre interior, y la orden nombra los bloques con la regla
  de `EditCabecera` sin escribir el nombre de la cabecera.
- Cambiadas: en `CabeceraEditorTests`, las pérdidas de regenerar pasan a `RegenerationLossesForHeight`, y la prueba de los destinos de la
  copia hace antes un cambio (2).
- **Comprobación en rojo:** nueve piezas rotas a propósito, una a una, con script (`hs65\t4\cb\red2.py` en el scratchpad de la sesión;
  cada archivo vuelve a sus bytes con el mismo SHA-256). En todas falla al menos una prueba:
  - R1, la copia al mismo poste en otro fondo con el peralte viejo: falla 1;
  - R2, copiar una calculada sin cambios: falla 1;
  - R3, regenerar sin contar notas ni caras: fallan 2;
  - R3b, regenerar sin conservar las notas: falla 1;
  - R4, restaurar las horizontales sin listar los paneles: fallan 2;
  - R5, el claro sin mínimo: falla 1;
  - R6, la semilla con la plantilla interna: falla 1;
  - R7a, la lectura que pone el nombre del sobre dentro (Core): falla 1;
  - R7b, la ventana que envía el nombre de la lista: falla 1.
- **Los números** (fuentes finales, solución en Debug):
  - **UI completa:** 2793 correctas, 0 con error, 48 omitidas (2841): las 2784 del commit `a2544580` más las 9 nuevas.
  - **Core, focales** (`CabeceraDoorTests`, `RackCadWindowListTests`, `WorkspaceIdleSchedulingGuardTests`, `WorkspacePilotReconciliationGuardTests`,
    `SelectiveEditorOpenTests`, `CustomPropertiesCommandGuardTests`): 289 correctas, 0 con error, 1 omitida. Las correcciones no añaden
    pruebas de Core (cambian dos de `CabeceraDoorTests`) y Core Full no se volvió a correr: su número sigue siendo el de `a2544580`
    (19154 / 0 / 403).
  - **Build Debug de la solución (Plugin incluido):** 0 errores; las 2 advertencias son las MSB3277 de siempre.

## Cuarta tanda, va: «Variables» y la pestaña «Propiedades» (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-va`, desde `ac3fd1b0`. Prototipo `variables` (VA-01 a VA-10) con las respuestas del Owner del 2026-11-26: todas aceptadas, y **VA-07 con nota** («Hay que crear una pestaña en cada rack en donde se puedan escribir las propiedades»). Sin commit, sin push, sin AutoCAD.

**Lo que se construyó:**
- **La página «Variables» de la ventana** (`VariablesPageViewModel`, `VariablesPageView`, `RackCadWindowViewModel.Variables.cs`, `RackCadWindowView.Variables.cs`). Sustituye al paso intermedio cuando la puerta de la ventana la sirve (`IRackCadVariablesDoorPort`); sin esa puerta (los dobles de las pruebas de antes) la sección sigue con su paso intermedio.
  - Una página por dibujo, como los borradores de rack. La primera vez que se muestra lee el dibujo con `RACKVARIABLESLEER` (sin marca de Deshacer ni historial): el registro y el barrido de racks que lee RACKVARIABLES (`ReadProjectVariables`) y las propiedades del proyecto. «Más…» › «Volver a leer del dibujo» la vuelve a leer, solo sin cambios.
  - Cabecera Limpio / Modificado / Escribiendo, «N cambios sin escribir», «Actualizar», «Descartar» y «Más…» (Comprobar, Volver a leer, Abrir la ventana clásica (RACKVARIABLES)). El motivo de un «Actualizar» desactivado va debajo, a la vista.
  - **Pestaña «Variables de proyecto»:** la lista (Nombre, Valor o fórmula, Valor con «(antes N)», Racks, Estado; dolores 7 y 8) y el inspector de la elegida: Nombre y Valor o fórmula (van al borrador al salir del campo o con Enter; salir sin cambiar no hace nada), el valor con su pista, «Racks que la usan» por nombre con «Abrir» (abre la página del rack), «Fórmulas que la usan», «Eliminar» / «Quitar» y «Desvincular todos y eliminar…» con el motivo debajo (dolores 9 y 17). «Nueva variable» añade una fila propia (dolor 3).
  - **«Referencias rotas»** solo si hay alguna (dolor 11), por rack y por nombre, con lo que queda. «Reparar…» y «Desvincular todos y eliminar…» preguntan en línea con lo que cambia y van al borrador, sin casillas (VA-03, dolor 10); «No reparar» y «No eliminar» los quitan.
  - **«Lo que escribirá «Actualizar»»:** cada cambio en una línea y los racks que se redibujan. **VA-04:** una tarjeta por cada rack con un borrador sin escribir en la ventana que usa una variable que cambia («Ir a …»), y «Actualizar» desactivado con «Antes, escribe o descarta el borrador de …». Se recalcula cuando cambia el borrador del rack.
  - **Registro bloqueado:** el aviso con el motivo de Application; no se ofrece crear ni editar; reparar y las propiedades del proyecto siguen.
  - **Pestaña «Propiedades del proyecto»** (VA-06), con el mismo «Actualizar»: nombre, valor de varias líneas (`AcceptsReturn`, dolor 16), estado y «Quitar», «Nueva propiedad» (hasta 50) y «Se quitarán al actualizar: …». La tabla es un control reutilizable (`CustomPropertiesTableView` sobre `ICustomPropertiesTable`), el mismo de la pestaña de un rack.
  - Ningún identificador interno a la vista (VA-09): ni el GUID del rack o de la variable, ni la clave de la propiedad (sale «Holgura vertical», «Tolerancia horizontal»), ni «Length», ni el nombre del bloque. Los errores de las fórmulas en español, en su campo y con el nombre de la variable (dolor 6). VA-05: no se admiten nombres repetidos (las dos filas lo dicen).
  - **Un solo «Actualizar», todo o nada** (VA-02): `RACKVARIABLESAPLICAR` relee el registro, el barrido y la colección del proyecto, exige que el registro y la colección sean los que leyó la página (`VariablesDoorRules.PlanWrite`), planea el borrador entero y lo escribe: el registro y todos los racks que cambian en **una** ejecución de `ProjectVariableMutationExecutor` (una transacción) y después la colección del proyecto (`CustomPropertiesExecutor.ExecuteProjectDraft`, en el borde físico; corrección 3). Es una sola orden: un «U». Escrito, la página olvida lo leído y vuelve a leer el dibujo; los editores de Selectivo **limpios** de los racks redibujados se cierran (mostraban lo de antes) y se dice. Sin escribir, el borrador sigue, con el motivo.
  - La puerta es la de siempre: una sola petición en vuelo para la página y los editores (S-16), con su número, el dibujo de la petición, «Dejar de esperar», `RequestStarted` y la poda de un dibujo cerrado. **D-08:** una página con cambios (o escribiéndose) es trabajo sin escribir y el dibujo no se cierra; el veto la nombra «Variables».
- **La pestaña «Propiedades» al final de los editores de Selectivo y de Dinámico** (VA-07, nota del Owner; `RackPropertiesTabViewModel`, `RackPropertiesTabView`, `SelectiveEditorViewModel.Properties.cs`, `DynamicEditorViewModel.Properties.cs`).
  - `RACKSELECTIVOABRIR` y `RACKDINAMICOABRIR` leen además las propiedades del rack por el borde físico de siempre (`CustomPropertiesExecutor.ReadRack`, la autoridad de RACKPROPIEDADES) y la ventana se las da al editor.
  - Son parte del borrador del editor: lo dejan «Modificado», «Descartar» las devuelve, «Cerrar el editor» pregunta, D-08 veta el cierre, y un valor no válido apaga «Actualizar» con «Propiedades: hay N valores no válidos.». Mientras AutoCAD escribe, la tabla no se edita.
  - «Actualizar» (`RACKSELECTIVOAPLICAR`, `RACKDINAMICOAPLICAR`) redibuja el rack como siempre y, en la misma orden, escribe el borrador de la pestaña en todas sus vistas sobre una lectura nueva (`CustomPropertiesExecutor.ExecuteRackDraft`, con la forma de `ExecuteRack`, y `CustomPropertiesCommit.ForRackDraft` en Application). La respuesta lo dice y trae lo que el dibujo guarda ahora (con los ids que acuñaron las altas); la pestaña sigue desde ahí. Si no se pudieron escribir, el rack queda escrito, las propiedades siguen en el borrador y la respuesta dice por qué.
  - **Vistas con propiedades distintas** (VA-08): «Contenido de cada vista», ninguna elegida al abrir; «Usar esta» solo en las que Application da por fuente segura; la casilla solo cuenta con una vista elegida; «Unificar en el borrador» pasa su contenido al borrador, que se puede retocar, y «Actualizar» lo escribe en todas.
- **RACKAYUDA** documenta RACKVARIABLES (RVA), que no estaba (VA-10).

**Application (cambios declarados):**
- `ProjectVariablesBatch` (nuevo): el borrador entero de la página es **un** plan. Hasta ahora Application planeaba una operación cada vez; encadenar esos planes juzgaría cada estado intermedio, y un borrador válido en conjunto puede pasar por uno inválido (bajar A mientras se cambia la fórmula B = A − 5). El lote juzga **el estado final** con las reglas de cada operación y reutiliza sus piezas: la reparación es `RepairBrokenRack`; «Desvincular todos y eliminar» materializa el valor de **ahora**, como la operación única; los consumidores salen de `ProjectVariableConsumerDiscovery` y cada rack se resuelve contra el registro final con el mismo resolvedor. Con una sola operación, el lote planea lo mismo que el preflight único: hay pruebas de equivalencia de renombrar, cambiar valor, eliminar (con y sin un rack que la usa), desvincular y eliminar, y reparar. **Corregido en la revisión:** la primera versión no comprobaba en «Eliminar» los racks que usan la variable, así que no era equivalente al `Delete` único (corrección 1).
- `RegistryMutationKind.Replace` (nuevo): el lote sustituye el registro entero, y `RegistryCommit.Prepare` solo lo escribe sobre el registro contra el que se planeó (misma forma persistida); si no, nada.
- `ProjectVariableFormulaText` (nuevo): la «forma de claves» de una fórmula de la página (`{§clave§}`), para que siga a sus variables por identidad mientras se renombran; usa el léxico de las fórmulas, así que solo toca nombres. Los ids de las variables nuevas los acuña el lote, nunca la página.
- `ProjectVariableDiagnosticText` (nuevo): los códigos de diagnóstico de una fórmula en español.
- `CustomPropertiesBatch` (nuevo): un borrador de propiedades entero como un cambio, con las mutaciones de siempre en un orden que no juzga estados intermedios (bajas, renombres por un nombre temporal único, valores, altas), y sus problemas por fila y campo.
- `CustomPropertiesCommit.Draft.cs` (nuevo, `CustomPropertiesCommit` pasa a `partial`): `ForRackDraft`, `PlanDraft` y `DraftFingerprint`.
- `RackCadVariablesDoor.cs` y `RackCadRackProperties.cs` (nuevos, en `VentanaRackCad`): la puerta de la página y la pestaña de un rack, con sus reglas puras.

**Las decisiones (supuestos para el Owner):**

| Id | Decisión | Por qué |
|---|---|---|
| VA-S1 | Las órdenes de la página son `RACKVARIABLESLEER` (lee, sin Deshacer) y `RACKVARIABLESAPLICAR` (escribe); el prototipo las dejaba [PENDIENTE] | Siguen el patrón de las de la ventana (`RACKSELECTIVOABRIR` / `…APLICAR`) |
| VA-S2 | VA-10 a medias: RACKVARIABLES y RACKPROPIEDADES **siguen abriendo las ventanas clásicas** por su orden, y la página ofrece la clásica en «Más…». No se reencaminan a la ventana | El encargo pide que el clásico siga funcionando por su orden; reencaminadas, la clásica se quedaría sin orden. Es un cambio pequeño cuando el clásico se retire (fin de I-64) |
| VA-S3 | «Desvincular todos y eliminar» conserva el valor de la variable **de ahora** (antes del borrador) | Es «su valor de ahora» del prototipo y lo que hace la operación única |
| VA-S4 | El lote no redibuja un rack cuya variable cambia de definición pero no de valor (6 → «=3 * 2»); la operación única sí lo redibujaba | El diseño efectivo es el mismo |
| VA-S5 | Una variable que **ya** no tenía valor y que el borrador no toca no es un error del borrador | Es la regla de la operación única: no hacer fallar lo que tenía valor |
| VA-S6 | Un nombre con llaves «{ }» se rechaza en la página | La fórmula nombra entre llaves: no se podría usar |
| VA-S7 | Variables y propiedades del proyecto se comprueban las dos antes de escribir nada, pero se escriben en dos transacciones de la misma orden (un «U»). Si la segunda fallara después de la primera, la respuesta lo dice y la relectura muestra el dibujo | El ejecutor de variables es su propia transacción; unirlas exige tocarlo. Para la planeación |
| VA-S8 | Tras escribir, la página vuelve a leer el dibujo (otra orden sin Deshacer) en lugar de calcular lo escrito | El dibujo es la autoridad, como en el bucle de RACKVARIABLES |
| VA-S9 | Un rack nuevo de la ventana no tiene la pestaña editable: dice que sus propiedades siguen en RACKPROPIEDADES | El despacho de «Nuevo» no escribe propiedades; puede llegar con los borradores de la parte `new` |
| VA-S10 | «Insertar vista…» no escribe las propiedades de la pestaña: siguen en el borrador hasta «Actualizar» | Solo «Actualizar» es la escritura completa del editor, como en el prototipo |
| VA-S11 | Las propiedades se escriben **después** del redibujo, en la misma orden, y su comprobación compara las colecciones distintas del rack, no los handles: una vista lateral que el redibujo borra no cuenta como cambio | El redibujo conserva las propiedades de cada sobre (I-54) |
| VA-S12 | El rótulo de una vista en «Contenido de cada vista» sale del sobre («Vista lateral · corte en el poste N» con la sección + 1, «Vista frontal · fondo N») | Sin AutoCAD no se comprobó la numeración de la sección |
| VA-S13 | La columna no marca «Variables» con su estado y la cabecera no lo cuenta en «cambios sin escribir» | Las secciones son registros fijos de la ventana compartida; el veto de cierre sí lo cuenta |

**Las ediciones a archivos compartidos** (pequeñas, marcadas `T4 va`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `DoorRequest.IsVariables`; el constructor llama a `InitializeVariables()`; `ShowPage` y `Present` avisan a la página; `UnwrittenWorkOf` añade «Variables»; `PruneDeadSessions` llama a `PruneVariables`; `StopWaiting`, `OnRequestStarted`, `RaiseWaiting` y `WaitingText` atienden la petición de la página; `OnSelectiveEditorChanged` refresca los conflictos (VA-04); `OnSelectiveRead`, `OnDesignRead`, `WriteSelective`, `WriteDynamic` y `OnDesignWritten` pasan las propiedades del rack.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml(.cs)`: `xmlns:local`; la sección «Variables» con `VariablesPageHost` y el paso intermedio, con su visibilidad; dos llamadas a `ShowVariablesPage()`.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: `RefreshDirty`, `CanUpdate`, `UpdateBlockedText`, `Discard`, `BeginWriting` y `EndWriting`, una línea cada uno.
- `src/RackCad.UI/VentanaRackCad/DynamicEditorViewModel.cs`: `partial`; `UpdateDirty`, `CanUpdate`, `UpdateBlockedText`, `Discard`, `BeginWriting` y `EndWriting`, una línea cada uno.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml` y `DynamicEditorView.xaml`: la pestaña «Propiedades» al final, en un bloque.
- `src/RackCad.UI/RackCommandReference.cs`: la entrada de RACKVARIABLES.
- `src/RackCad.Application/VentanaRackCad/RackCadRackDoor.cs`: campos opcionales al final de `RackDoorWrite`, `RackDesignRead`, `RackDesignWritten`, `SelectiveDoorWrite` y `SelectiveDesignRead`.
- `src/RackCad.Application/ProjectVariables/MutationPlan.cs` y `RegistryCommit.cs`: `Replace`. `src/RackCad.Application/CustomProperties/CustomPropertiesCommit.cs`: `partial`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: `partial`, `AbandonVariables` en `Abandon`, `PendingWrite.Properties`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs`: la lectura de las propiedades en las dos aperturas y su escritura tras las dos «Actualizar».
- `src/RackCad.Plugin/CustomPropertiesExecutor.cs` (el borde físico): `ExecuteRackDraft`, con la forma de `ExecuteRack`, y `ExecuteProjectDraft`, con la de `ExecuteProject` (corrección 3).

**Censos y guardas que cambian, con su nota:**
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 51 a 53.
- `CustomPropertiesCommandGuardTests`: `A14VariablesDoorCommands` en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `RackCadVariablesDoorCommands.cs` es un archivo de órdenes de la puerta, como `RackCadRackDoorCommands.cs` (tampoco bloquea ni abre transacciones: lee con `ReadProjectVariables` y `CustomPropertiesExecutor`).
- `CustomPropertiesEdgeGuardTests` (T-GRD-04): `ExecuteRackDraft` entra en la lista de ejecutores del borde, con su llamada única a `CustomPropertiesCommit.ForRackDraft` y sin las de los otros dos; tras la revisión, también `ExecuteProjectDraft`, y `PluginCallersOutsideTheEdge` vigila además `CustomPropertiesData.Write` y `CustomPropertiesWriteGuard`. El borde vuelve a ser un solo archivo: ningún otro archivo del Plugin nombra la autoridad, el commit, la escritura del proyecto ni su guarda (la primera versión tenía un segundo escritor del proyecto, `CustomPropertiesBatchWriter`, ya retirado).
- `CustomPropertiesProjectTests` (T-PRJ-04): la guarda del ejecutor de Proyecto mira también `ExecuteProjectDraft`, con un paso más (comparar con lo leído) y cuatro mutaciones nuevas.
- `CustomPropertiesHelpCensusTests` (UI): RACKVARIABLES/RVA en la ayuda.
- `RackCadSelectiveEditorTests` (UI): el censo de pestañas termina en «Propiedades».

**Las pruebas nuevas:**
- Core, `ProjectVariablesBatchTests` (27): el borrador vacío; un renombre, un cambio de valor, «Desvincular todos y eliminar» y una reparación, cada uno igual al preflight único; el borrador válido en conjunto cuyo paso solo no lo es; dos variables nuevas que se usan por clave; el valor que no cambia no redibuja; los nombres repetidos; seis errores de definición en español, sin id; el ciclo; eliminar con una fórmula que la usa, y cambiarla en el mismo borrador; una fórmula que usa una eliminada; el commit de `Replace` sobre el registro planeado y sobre otro; la forma de claves; el borrador de propiedades (intercambio de nombres, alta, baja, problemas por fila); las reglas de la puerta (lo leído, lo cambiado, las propiedades del proyecto, nada que escribir) y la respuesta.
- Core, `RackPropertiesRulesTests` (4): la lectura de un rack, la escritura en todas las vistas solo sobre lo leído, el borrador que rompe una regla y la unificación desde una fuente segura.
- UI, `RackCadVariablesPageTests` (15): la página (valores, racks por nombre, motivo de «Eliminar»), el borrador y «Descartar», el error en su campo, la variable nueva que sigue a otra por identidad y los nombres repetidos, «Desvincular todos y eliminar» y «Reparar» en línea, las propiedades del proyecto, la ventana (una lectura, una petición, D-08, la orden que la tomó, la respuesta escrita y la relectura, la respuesta de otro número, el fallo, «Dejar de esperar»), VA-04 con un Selectivo modificado, el editor limpio que se cierra, la sección sin la puerta, y las vistas con WPF real (sin errores de enlace, la pregunta en línea, la tabla de propiedades, el inspector debajo en estrecho, la ventana que aloja la página).
- UI, `RackCadRackPropertiesTabTests` (7): la pestaña en el borrador del editor y «Descartar», el valor no válido que apaga «Actualizar», la unificación tras elegir y confirmar, Dinámico con la misma pestaña, la ventana que da y envía las propiedades y las que no se escribieron, y la vista de los dos editores con WPF real.

**Comprobación en rojo:** seis piezas rotas a propósito, una a una, con su clase de pruebas tras cada una; cada archivo se restauró de sus bytes con fecha nueva y se comprobó igual:
1. el lote sin la regla «lo que tenía valor lo conserva»: falla la del borrador válido en conjunto (el paso solo ya no se rechaza);
2. el commit de `Replace` sin comparar el registro planeado: falla la del commit sobre otro registro;
3. la pestaña de un rack sin comparar lo que se leyó: falla la de la escritura solo sobre lo leído;
4. la página sin conflictos (VA-04): falla la del Selectivo modificado;
5. el consejero de cierre sin la página (D-08): falla la de la ventana;
6. la pestaña fuera del «Modificado» del editor de Selectivo: fallan cinco de las siete de la pestaña.

**Los números:**
- **UI completa:** 2777 correctas, 1 con error, 48 omitidas (2826): la base, 2756, más las 22 nuevas (15 + 7) y las cambiadas. El error es `RackModalHostWindowTests.TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses` por su temporizador de seguridad (M-21), con cinco suites de UI de otras partes corriendo a la vez en la máquina (50 min); sola, la clase pasa 3 de 3.
- **Core Full:** 19176 correctas, 2 con error, 403 omitidas (19581): la base, 19147, más las 31 nuevas. Los dos errores eran míos y se corrigieron después: `LengthUnitsAuthorityGuardTests` (dos textos de ayuda escribían «[mm]» a mano; ahora salen de `LengthUnits.Authority`) y `WorkspacePilotReconciliationGuardTests` (la página tenía un `Update()`, nombre que la guarda reserva al campo del panel; ahora es `RequestWrite()`, como los editores). Esas dos guardas y las clases nuevas, otra vez: 81 de 81 (1 omitida); las focales de UI tras el cambio: 59 de 59. **Las dos suites completas no se repitieron** después de estos dos cambios de texto y de nombre (73 min la de Core con la máquina compartida): queda para la sesión.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de siempre).

**Lo que queda abierto:**
- VA-10 completo: reencaminar RACKVARIABLES y RACKPROPIEDADES a la ventana (VA-S2), cuando el clásico se retire.
- Una sola transacción para variables y propiedades del proyecto (VA-S7); las propiedades de un rack nuevo (VA-S9, con los borradores de la parte `new`); la marca de «Variables» en la columna (VA-S13).
- **Sin AutoCAD (smoke):** que `RACKVARIABLESLEER` no deje nada en Deshacer; que `RACKVARIABLESAPLICAR` escriba el registro y los racks con un solo «U»; la pestaña «Propiedades» escrita tras el redibujo y su «U»; los rótulos de las vistas en la unificación (VA-S12); la página en la paleta (foco, anchos).

### Correcciones de la revisión

Revisión independiente de `89cd65f9` (commit de la sesión con esta parte). Para cada punto se escribió primero la prueba que reproduce el fallo, se vio en rojo sobre `89cd65f9` (14 de Core y 1 de UI fallaron; la de «Eliminar sin racks», que ya era equivalente, pasaba) y después la corrección. Pruebas nuevas: `ProjectVariablesBatchReviewTests` (Core, 9), una más en `RackCadVariablesPageTests` (UI), dos casos más en `TGrd04_LaGuardaDetectaOtroArchivoDelPluginQueDecide` y cuatro mutaciones más en `TPrj04_LaGuardaDetectaUnEjecutorDeProyectoQueSeSaleDelContrato`.

| Punto | Qué pasaba | Qué se hizo |
|---|---|---|
| 1 [ALTO] | «Eliminar» del lote no miraba los racks que usan la variable. Si después de leer la página un rack se vinculaba a ella en RACKEDITAR (o un «U» deshacía un desvínculo), «Actualizar» escribía el registro sin la variable y dejaba una referencia rota | En las bajas «Eliminar», `ProjectVariableConsumerDiscovery.DiscoverConsumers` sobre el barrido de ahora, como `ProjectVariableMutationPreflight.Delete`: un barrido que falla es error, y un rack que la usa también, con su nombre y «usa «Desvincular todos y eliminar…»». Prueba de equivalencia con el `Delete` único con y sin rack |
| 2 [MEDIO-BAJO] | VA-S7: si la segunda transacción (propiedades del proyecto) devolvía un error, la página decía «escritas» y vaciaba sus filas; si lanzaba, la orden respondía «No se pudo escribir» con el registro y los racks ya confirmados | `VariablesDoorWritten.PropertiesError`; la escritura de las propiedades tiene su propio `try` en `RACKVARIABLESAPLICAR`; la página dice el error (en rojo) y conserva el borrador de propiedades sobre la relectura |
| 3 [MEDIO, guardas] | `CustomPropertiesBatchWriter` era un segundo escritor del proyecto fuera del borde | Retirado. La escritura es `CustomPropertiesExecutor.ExecuteProjectDraft`, con el contrato de `ExecuteProject` (relee, acredita, exige escribible, compara con lo leído, aplica el borrador, consulta la guarda, serializa, escribe y confirma una vez). Guardas: `Executors`, T-PRJ-04 y `PluginCallersOutsideTheEdge` (ver «Censos y guardas») |
| 4 [BAJO] | Eliminar A y renombrar B como «A» (o crear una «A» nueva) con una fórmula que la usa daba «Hay más de una variable con ese nombre» | Las fórmulas se ligan contra el registro sin las variables que se eliminan: una variable eliminada deja libre su nombre. La fórmula que usa una eliminada se sigue rechazando antes, por su clave |
| 5 [BAJO] | Reparar un rack y «Desvincular todos y eliminar» una variable que el mismo rack usa daba «no tiene un valor que conservar» | El valor de ahora se resuelve sobre el diseño ya reparado del rack cuando lo hay |
| 6 [BAJO] | Una variable sin tocar con valor ≤ 0 (ya así en el dibujo) bloqueaba todo el borrador | VA-S5: sin redefinir y con el mismo resultado que ahora, no es un error del borrador |
| 7 [LATENTE] | Con un registro no utilizable (PresentButUnreadable, IncompatibleMajor) el lote seguía planeando reparaciones, contra un registro vacío | El lote no planea nada, reparaciones incluidas (I-47, ADR-0034 §8). La página bloqueada solo ofrece reparar si muestra alguna referencia rota |

**Comprobación en rojo de 1 a 3**, cada pieza rota a propósito y restaurada de sus bytes con fecha nueva:
- R1, «Eliminar» sin mirar los racks: falla `DeletingAVariableThatARackUsesIsRefusedAsTheSingleDelete`;
- R2a, la respuesta sin `PropertiesError`: falla `AWriteWhosePropertiesFailedSaysSoApartFromTheVariables`;
- R2b, la página que no mira `PropertiesError`: falla `PropertiesOfTheProjectThatCouldNotBeWrittenStayInTheDraftAfterTheVariablesAreWritten` (UI);
- R3, `PluginCallersOutsideTheEdge` sin la escritura del proyecto ni su guarda: fallan los dos casos nuevos de `TGrd04_LaGuardaDetectaOtroArchivoDelPluginQueDecide`.

La prueba del `try` propio de la segunda transacción es de fuente (la orden es código de AutoCAD sin pruebas aquí) y falló sobre `89cd65f9`.

**Los números tras las correcciones** (las dos suites completas, repetidas, sobre las fuentes restauradas):
- **Core Full:** 19193 correctas, 0 con error, 403 omitidas (19596): la base, 19147, más las 31 de la parte y las 15 de la revisión (9 de `ProjectVariablesBatchReviewTests`, 2 casos de T-GRD-04 y 4 mutaciones de T-PRJ-04). Incluye `LengthUnitsAuthorityGuardTests` y `WorkspacePilotReconciliationGuardTests`, que fallaban en la primera Core Full y se corrigieron antes del commit `89cd65f9`.
- **UI completa:** 2779 correctas, 0 con error, 48 omitidas (2827): la base, 2756, más las 22 de la parte y 1 de la revisión.
- **Build Debug del Plugin:** 0 errores (las 2 advertencias MSB3277 de siempre).
- Las dos suites corrieron a la vez, sobre las fuentes restauradas tras la comprobación en rojo, con otras partes usando la máquina (34 min la de Core, 30 la de UI).

## Cuarta tanda, seg: la seguridad sin diálogo, en Selectivo y Dinámico (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-seg`, desde `ac3fd1b0`. Worktree `D:\w4seg`. Sin commit, sin push y sin AutoCAD. Prototipo `seguridad` (SG-01 a SG-09) con las respuestas del Owner del 2026-11-26: todas aceptadas salvo SG-02, que lleva nota; y SD-14 («Quitarlo»).

**Lo que se construyó:**
- **SG-01. La seguridad se edita en «Extremos y seguridad», sin diálogo,** en los dos sistemas:
  - **«Seguridad por poste» se edita** (`RackCadSafetyTableView`, una sola vista para los dos sistemas):
    - la bota de cada poste: «General (…)», «Ninguna» y los tres lados; un poste con la suya lleva «Propia»;
    - el protector lateral de cada poste;
    - los niveles del desviador, N1…Nk, como botones;
    - los separadores entre fondos (Selectivo, de solo lectura);
    - en Dinámico, las defensas de salida y de entrada (casilla y longitud) y las guías;
    - la fila «Total», contada como el BOM, y el título de cada columna de familia, que muestra esa familia en el inspector.
  - **«Variante y medidas»** (`RackCadSafetyInspectorView`, también compartido) lleva, de la familia elegida:
    - la variante del catálogo;
    - las medidas que estaban en las seis rejillas del diálogo;
    - lo que coloca, contado como el BOM;
    - sus notas y sus botones.

    Va a la derecha de la pestaña; por debajo de 900 px, debajo, como el inspector de «Matriz».
  - La cabecera de las tarjetas T y P (Selectivo) y G (Dinámico) muestra su familia en el inspector.
  - **«Editar seguridad…» sale de la ventana** en los dos sistemas: sustituye a D-09. La ventana ya no asigna `SafetyEditor`, ni siquiera con el puerto modal. El puerto sigue para «Insertar vista…». El diálogo clásico sigue en RACKEDITAR y en «Abrir en editor clásico» de Dinámico.
  - **La forma de confirmar es la del editor:** una lista, un nivel o una casilla van al borrador en el momento; una medida, al salir del campo o con Enter. Un valor que no vale se queda en su campo con el motivo y no escribe nada. El editor pasa a «Modificado», «Descartar» lo recupera y «Actualizar» lo dibuja (RACKSELECTIVOAPLICAR / RACKDINAMICOAPLICAR, sin cambios).
- **Elegir la variante basta** (dolor 3). La familia entra con los valores del diálogo clásico aceptado sin tocar nada (`RackCadSafetyEdit.Fresh`):
  - la bota, en los dos lados de cada poste;
  - el protector lateral de Selectivo, en P1 («Izquierda») y en el último poste («Derecha»), la siembra `OrillaDefaults`; el de Dinámico, con la regla de los extremos;
  - el desviador, en las dos caras, de 18 in y con el primer nivel a 18 in, en todos los niveles;
  - el tope, uno central compartido, con saque de 3 in y sin frontal;
  - la parrilla, en la frontal y en la lateral, una por tarima;
  - la guía, en todas las celdas;
  - la defensa, automática en cada poste.
- **SG-02, con la nota del Owner:** cada sistema conserva sus palabras. Ver «Lo que encontré», debajo.
- **SG-03. Defensas de Dinámico:**
  - una longitud vacía es la automática: 12 in en orilla y 36 in en intermedio, que se recalcula al añadir o quitar frentes;
  - una escrita es propia y se queda aunque coincida con la automática. Se guarda con `ExitAuto`/`EntranceAuto` de PB-010, como Push Back;
  - la casilla quita o pone la defensa de ese extremo;
  - «Todas automáticas» vacía los registros;
  - **es un cambio de cálculo declarado:** el diálogo clásico de Dinámico, sin columna «Auto», no guardaba una fila igual a su automática.
- **SG-04. El protector lateral de Dinámico:**
  - la tabla muestra «Regla de los extremos»;
  - cambiar un poste escribe antes los dos extremos de la regla (solo cambia el poste tocado) y lo avisa: «El protector lateral va ahora por poste…»;
  - «Volver a la regla de los extremos» la recupera;
  - un poste puesto en «No lleva» queda escrito, así que vaciar todos los postes no devuelve la regla sin decirlo (T4S-03).
- **SG-05. El desviador por poste y nivel, en la tabla,** con la numeración de «Matriz». El lado, la longitud y el primer nivel van en el inspector. En Selectivo, la nota del claro sale del plan del desviador (`ClearanceIssues`, la del diálogo clásico), no del [PENDIENTE] del prototipo. «En todos los niveles» y «En ninguno» solo tocan las celdas que la tabla muestra: una celda dormida de un frente en blanco se queda como estaba.
- **SG-06. «Sin variante» pregunta antes, en línea, qué se pierde:**
  - la variante, su configuración, los postes con la suya y las celdas o los niveles apagados;
  - «Quitar …» la quita, con un aviso, y «Descartar» la recupera; «Conservar» no cambia nada;
  - mientras pregunta, la lista sigue mostrando la variante.
- **SG-07. Lado del tope:**
  - con dos fondos o más y «Uno central, compartido entre fondos», el lado se desactiva y dice por qué;
  - «Ninguno (no se dibuja)» solo aparece si el rack leído lo tiene;
  - con un fondo no hay «Cuántos» ni «Fondo».
- **SG-08. Los postes intermedios de medio frente** tienen fila propia, «F1 · intermedio 1», con sus niveles de desviador y sin bota ni protector. La fila sale del plan del desviador, no del resumen. El resumen solo lista los postes con alguna pieza, y la fila no puede desaparecer al apagar todos sus niveles: entonces no se podría volver a encender desde la tabla.
- **SG-09.** Push Back queda fuera, con su diálogo y sus secciones por lado (I-42).
- **SD-14. Sin «≈» en las alturas de cabecera y de poste del editor de Selectivo:**
  - «Calculado» › «Altura de cabecera»;
  - las fichas de «Altura por poste» de «Matriz»;
  - la altura por fondo de «Avanzado»;
  - la pista «Calculada: … in» de la altura manual del frente.

  El «≈» del ancho de un frente y del «(resto)» de los tramos se queda: no son alturas.

**Lo que encontré en el código (SG-02; no se unificó nada):**
- **Selectivo:**
  - la bota usa «Ninguno / Izquierda / Derecha / Ambas», las `SideLabels`, porque `bootUsesPlacementNames` es falso (I-43, R2-10);
  - el desviador, «Izquierdo / Derecho / Ambas», en masculino;
  - el tope, «Izquierda = el extremo bajo, Derecha = el alto»;
  - la ventana ya decía «Izquierda», «Derecha» e «Izquierda y Derecha» (C2-08).

  En la ventana: «Izquierda / Derecha / Izquierda y Derecha». El protector lateral añade «Ambos (frente-puente)», y el desviador con varios fondos, «Izquierda (cara del fondo 1)» y «Derecha (cara del fondo n)», como el prototipo.
- **Dinámico:**
  - la introducción de su diálogo dice «Izquierda es la salida y derecha la entrada» (`RackCadModalDialogs.DynamicSafetyIntroduction`);
  - la bota ofrece «Entrada/Salida / Posterior»: `bootUsesPlacementNames: true`, heredado de I-42 «sin decisión registrada para este sistema»;
  - el desviador dice «Izquierdo / Derecho»; la defensa, «Salida / Entrada»;
  - la ventana ya decía «salida · entrada» y «Salida = extremo bajo · entrada = extremo alto».

  Por el código, en los tres casos **Izquierda / Entrada-Salida = el extremo bajo = la salida**, y **Derecha / Posterior = el extremo alto = la entrada**:
  - la bota, por `BootFacesAt`: la cara cercana es la del extremo bajo;
  - el desviador y el protector lateral, por `SelectiveSafetyEnds.CopiesForPost`: «Left» va al extremo bajo.

  En la ventana: «Salida / Entrada / Salida y entrada» en las cinco familias, con los mismos ordinales. El dato no cambia.
- **Push Back:** «Entrada/Salida / Posterior» en su diálogo, sin tocar (SG-09).
- Las palabras viven en un solo sitio, `RackCadSafetyWords` (en `RackCadSafetyPanel.cs`).

**Corrección del desviador de Dinámico, decisión del Owner 2026-11-26** («Sí, corrígelo en Dinámico»). Es un **cambio de cálculo declarado**.
- **El hallazgo:** en Dinámico, el desviador del último poste leía la columna del penúltimo.
  - `SelectiveDesviadorPlan.CellKey` daba `Math.Min(poste, frentes − 1)` sin `DesviadorCellsAreByPost`: la lectura histórica, que PB-002 corrigió solo para Push Back (`ideas-futuras.md` la tenía pendiente de esta decisión).
  - La rejilla del diálogo clásico de Dinámico tiene una columna por poste desde I-33. La columna del último poste no gobernaba nada, y la del penúltimo gobernaba los dos.
- **El mecanismo es el de PB-002:** la misma regla `CellKey`, por poste. La lee el lateral, los dos frontales, la planta y el BOM, y ahora también la cumple el Dinámico que lleva la marca `SelectiveDesviadorConfig.CellsByPost`.
- **La política de lo existente es un supuesto (T4S-13).**
  - PB-002 no dejó una política para los dibujos existentes. Su bandera es derivada y la autoridad de Push Back la impone siempre, sin migrar nada (`docs/initiatives/I-32-correcciones-push-back.md`, `decisions/I-32.md`).
  - Por eso se eligió la que no cambia el dibujo de un rack existente hasta que alguien edita su desviador, y la marca se **persiste**: es el discriminante de dos épocas, como `BotaSidesDeclared` de I-42.
  - En el documento se llama `DesviadorCellsByPost`. Se escribe solo cuando es verdadera, así que un documento anterior, uno de Selectivo y uno de Push Back no cambian ni un byte. El comparador de AUTH-13 (`AuthoredTypedValues`, `AuthoredCanonical`) la compara, y su ausencia vale falso.
- **Quién lleva la marca:**
  - un Dinámico **creado de cero**: el borrador nuevo de la ventana y el RACKNUEVO clásico (`DynamicSafetyDefaults.BuildForDynamic`; Push Back sigue con `Build`);
    - un diseño de la biblioteca guardado antes de esta corrección es un diseño existente (T4S-13);
    - se abre y se inserta sin la marca, porque `LoadFrom` y la carga del clásico toman la seguridad guardada;
    - conserva su lectura hasta que se edita su desviador;
  - un desviador que se elige por primera vez, en la ventana (`RackCadSafetyEdit.Fresh`) o en el diálogo clásico de Dinámico (`newDesviadorByPost`);
  - un desviador existente **en cuanto se edita en la ventana**: un nivel, «En todos los niveles» / «En ninguno», el lado, la longitud o el primer nivel.
- **Cómo pasa un rack existente** (`RackCadSafetyEdit.DesviadorToPosts`):
  - el último poste toma las celdas de la columna que leía, y las suyas, que nadie leía, se van;
  - así el dibujo queda igual (probado con las piezas de cada poste) y después se aplica la edición;
  - desde ahí cada poste gobierna el suyo.
- **El diálogo clásico conserva la marca al aceptar** (`SelectiveSafetyWindow`, un campo más en su fila). Sin eso, RACKEDITAR desharía la corrección al reescribir la selección. Un rack existente que se acepta allí sigue con la lectura histórica, como antes.
- **La tabla de la ventana ya no liga el último poste al penúltimo, y su aviso desaparece.**
  - Cada celda muestra lo que el dibujo lee en su poste.
  - Un clic escribe ese poste: en un rack existente, primero lo pasa a la lectura por poste.
  - Salen el «· niveles de P2» y la nota del inspector.
- **Archivos:**
  - Domain `SelectiveSafetyConfig.cs` (`CellsByPost` y su `DeepCopy`);
  - Application:
    - `SelectiveDesviadorPlan.cs` (`CellKey`);
    - `Persistence/SafetySelectionDocuments.cs` y `Persistence/SelectivePalletDesignDocument.cs` (el campo `DesviadorCellsByPost`);
    - `Systems/Shared/AuthoredTypedValues.cs` y `AuthoredCanonical.cs`;
    - `Systems/Dynamic/DynamicSafetyDefaults.cs` (`BuildForDynamic`);
    - `VentanaRackCad/DynamicDraftEditor.cs` (una línea), `RackCadSafetyEdit.cs` y `RackCadSafetyRows.cs` (`DeflectorWriteKey`, sin `DeflectorSharedWith`);
  - UI:
    - `SelectiveSafetyWindow.cs` (conserva la marca; el parámetro `newDesviadorByPost`);
    - `Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` (dos líneas: `BuildForDynamic` y `newDesviadorByPost: true`);
    - `VentanaRackCad/RackCadModalDialogs.cs` (una línea);
    - `RackCadSafetyPanel.cs`;
  - `docs/ideas-futuras.md`: el punto pendiente queda decidido y corregido.

**Las decisiones** (prefijo T4S):

| Id | Decisión | Por qué |
|---|---|---|
| T4S-01 | Las ediciones trabajan sobre una copia de la seguridad del borrador y cambian la selección EN SU SITIO: sus otros campos se quedan. Escriben los mismos datos que el «Aceptar» del clásico: la bota, su lado y su colocación (`Side`/`PostSides` y `Bota.Placement`/`Bota.Posts`, los mismos ordinales); el protector, por poste; el desviador y las rejillas, sus celdas apagadas. Una prueba lo fija: el diálogo clásico abierto sobre lo que escribió la ventana y aceptado sin tocar da el mismo diseño, byte a byte | El clásico sigue en RACKEDITAR y no debe cambiar un rack que la ventana dejó |
| T4S-02 | **Supuesto.** La general de la bota de un rack leído sin colocación general (un documento anterior a I-42) cambia solo el lado. Sin colocación, el dibujo lee el lado, así que el resultado es el mismo y volver al lado leído deja el borrador «Limpio». El clásico guardaría además la colocación (**diferencia declarada**). Una familia nueva sí la lleva, como el clásico | «Modificado» es una diferencia con el dibujo, nunca un gesto |
| T4S-03 | **Supuesto.** En Dinámico, «No lleva» de un poste del protector se guarda escrito, así que vaciar todos los postes no vuelve a la regla de los extremos; para eso está su botón. En Selectivo, sin lado general, «No lleva» es la ausencia del poste, el «(por defecto)» del editor por poste | Una regla que vuelve sola sin decirlo pondría protectores que nadie eligió |
| T4S-04 | **Supuesto.** Parrilla: una cantidad que no cabe en alguna celda con parrilla se rechaza en su campo, como el «Aceptar» del clásico pero sin MessageBox. Un ancho con el que no cabe ninguna en ninguna celda también se rechaza, porque la familia no dibujaría nada. Si no cabe en algunas, se avisa en el inspector («No cabe ninguna parrilla en …»), como el pie del clásico. Las celdas se nombran por su larguero («F1 · larguero 1»): el mismo larguero es N1 en un frente con larguero a piso y N2 en uno sin él | El prototipo rechazaba los dos en su modelo de un solo claro. El clásico solo rechaza la cantidad |
| T4S-05 | Un valor que no vale se rechaza en su campo y no llega al borrador, como en el resto del editor. No hay «valor no válido» en el borrador, ni marca en la pestaña, ni «Actualizar» apagado por él, que es como lo mostraba el prototipo | Patrón vigente de los editores de la ventana: el borrador nunca guarda un valor que no vale |
| T4S-06 | **El puente al diálogo clásico se queda en el código, sin uso en la ventana:** `SafetyEditor`, `CanEditSafety`, `SafetyEditText` y `EditSafety` de los dos modelos de vista, `RackCadSelectiveSafetyDialog`, y `RackCadModalDialogs.BuildDynamicSafetyDialog` y `EditDynamicSafety`. Sus pruebas siguen. Se borra con el clásico, al final de I-64 | Encargo: no borrar código clásico en esta tanda. Una prueba de la ventana fija que no se asigna |
| T4S-07 | La línea de la tarjeta de topes es la del prototipo (`famCfg`), compartida con el inspector. Con un fondo dice el lado («Saque 3 in · Izquierda y Derecha · en la frontal: no.»). Antes decía «en los postes posteriores», fuera cual fuera el lado, y con «Ambas» hay tope en la cara cercana y en la lejana (`SelectiveSafetyPlacement.TopeSpots`). Con un fondo elegido: «en el fondo 2, uno central, compartido entre fondos» | Dolor 14: textos que no dicen lo que hay |
| T4S-08 | El inspector va al lado de la pestaña desde 900 px de ancho (el de «Matriz», desde 700). Se coloca solo en la rejilla que lo contiene (`RackCadSafetyInspectorView.Place`), así que las vistas de los sistemas no ganan código | La tabla de Dinámico mide unos 1000 px; se desplaza en horizontal |
| T4S-09 | Selectivo: la tabla y el inspector leen lo que la pestaña ya contaba (`SelectiveSafetyView.Count`, una vez por recomposición y solo con la pestaña a la vista, P1-04). Sobre el mismo sistema propio salen también las filas, el plan de la parrilla y el claro del desviador (`SelectiveSafetyCounts`, tres campos nuevos). Dinámico: las filas salen del resumen que ya cuenta cada recomposición | Sin un recuento más del BOM |
| T4S-10 | «En ninguna» de la guía y «En ninguno» del desviador dejan la variante y sus medidas, como C2-07: no se dibuja ni se cuenta nada | Volver a ponerlas no exige elegir otra vez la variante |
| T4S-11 | El protector lateral de Selectivo puesto en «No lleva» en todos sus postes conserva la variante: «Elegido, en ningún poste: no se dibuja.». El «Aceptar» del clásico la descartaría (**diferencia declarada**, como C2-07) | El mismo dibujo y el mismo BOM; «Solo en los extremos» la recupera |
| T4S-13 | **Supuesto.** La corrección del desviador de Dinámico no cambia el dibujo de un rack existente hasta que su desviador se edita en la ventana: la marca persistida y el paso que conserva el dibujo, arriba. La otra lectura, la de PB-002 aplicada tal cual (todo Dinámico por poste desde ya), movería el desviador del último poste en los racks cuyas celdas lo tocan | Lo que pidió el Coordinator si PB-002 no dejó política: PB-002 solo dice que su bandera es derivada y no persiste |
| T4S-14 | **Cambio declarado del diálogo clásico** (revisión de «seg»). La rejilla de defensas de Dinámico en RACKEDITAR muestra la columna «Auto» por extremo, como la de Push Back (PB-010), y su texto añade «Marca «Auto»…». Un extremo automático no guarda longitud; uno propio se guarda aunque coincida con la automática. Una fila que el usuario no toca vuelve como entró, también la de un rack guardado antes (sin procedencia, abre con «Auto» desmarcado) | Sin la columna, «Aceptar» sin tocar nada deshacía SG-03: guardaba los dos extremos como propios y descartaba la fila igual a su automática |
| T4S-12 | Las medidas del inspector muestran lo que usa el dibujo: la longitud y el primer nivel del desviador, validados por su plan, y el saque efectivo del tope. Un valor antiguo que no vale se ve con el que lo sustituye, y salir del campo sin tocarlo no escribe nada. La variante se muestra por su id del catálogo, también cuando el documento la escribió con otras mayúsculas | Como «Extremos y fondos» de E2, que ya leía las medidas validadas |

**Ediciones a archivos compartidos** (pequeñas y marcadas «T4 seg»):
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs` (SD-14): cuatro textos de altura sin «≈» y sus comentarios.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.Safety.cs` (el parcial de E2 seguridad):
  - `TopeConfig` llama a la línea compartida (T4S-07);
  - la tarjeta gana `PickName` y `VariantName`, y `Set` un parámetro opcional;
  - una línea en `RaiseSafetyTab` y otra en `RaiseSafetyState` refrescan el panel;
  - el comentario de la clase.
- `src/RackCad.UI/VentanaRackCad/DynamicEditorViewModel.cs`:
  - `partial` en la declaración;
  - una línea en `RebuildSafety` (`RebuildSafetyPanel(rows)`) y otra en `RaiseState` (`RefreshSafetyPanel()`);
  - el texto de `GuideSummary` sin guía: «Para añadirla, elige su variante en «Variante y medidas».».
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: el contenido de la pestaña `SafetyTab`, en UN bloque marcado `<!-- T4 seg … -->`:
  - la rejilla con el inspector;
  - las tarjetas, con su cabecera como botón;
  - «Seguridad por poste» con la tabla nueva;
  - «Extremos y fondos» al final, como el prototipo.

  Se quitan la `DataGrid` de solo lectura, su nota y «Editar seguridad…».
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml.cs`: `OnFamilyPickClick` nuevo; se quita `OnEditSafetyClick`.
- `src/RackCad.UI/VentanaRackCad/DynamicEditorView.xaml`:
  - `xmlns:local` en la cabecera;
  - el contenido de `SafetyTab`, en UN bloque marcado `<!-- T4 seg … -->`: la tarjeta de la guía con su cabecera, la tabla nueva, «Extremos» al final y el inspector. Se quitan la `DataGrid`, su texto y «Editar seguridad…».
- `src/RackCad.UI/VentanaRackCad/DynamicEditorView.xaml.cs`: `OnGuidePickClick` en lugar de `OnEditSafetyClick`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`:
  - `Wire` ya no asigna `SafetyEditor` de Dinámico;
  - se quita el método `WireSelectiveSafety`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Selective.cs`: `WireSelective` ya no lo llama (una línea, con su comentario).
- `src/RackCad.Application/VentanaRackCad/SelectiveSafetyView.cs`:
  - tres campos `init` en `SelectiveSafetyCounts` (`Rows`, `ParrillaPlan`, `DesviadorClearance`), que llena `Count`;
  - el texto «Elige primero la variante de … en «Extremos y seguridad».», donde decía «en «Editar seguridad…»», y el comentario de la clase.
- **Archivos nuevos:**
  - Application: `RackCadSafetyEdit.cs` (las ediciones, puras) y `RackCadSafetyRows.cs` (las filas de la tabla de cada sistema);
  - UI: `RackCadSafetyPanel.cs` (el modelo de la vista compartido, el inspector y las palabras), `RackCadSafetyPanelItems.cs`, `RackCadSafetyTableView.xaml(.cs)`, `RackCadSafetyInspectorView.xaml(.cs)`, `SelectiveEditorViewModel.Seg.cs` y `DynamicEditorViewModel.Seg.cs`;
  - pruebas: `tests/RackCad.UI.Tests/Workspace/RackCadSafetyPanelTests.cs`.
- **Pruebas existentes que cambian, cada una con su nota:**
  - `SelectiveSafetyTabTests`:
    - la ventana ya no ofrece el diálogo, ni con el puerto;
    - el Selectivo nuevo edita su seguridad en la ventana;
    - la vista de la pestaña: la tabla nueva, las cabeceras de las tarjetas y sin «Editar seguridad…»;
    - la línea de la tarjeta de topes (T4S-07);
  - `SelectiveSafetyViewTests`: el texto de «Elige primero…»;
  - `RackCadDynamicEditorTests`: la ventana ya no ofrece el diálogo de Dinámico;
  - `RackCadSelectiveEditorTests` y `RackCadSelectiveFidelityTests`: SD-14;
  - Core, por la corrección del desviador. Core Full falló en el censo del estado del piloto: «fields without a class: SelectiveDesviadorConfig.CellsByPost».
    - `WorkspaceDraftPilotStateTests` y los dos censos de Push Back (`PushBackEditableStateCopyTests` y `PushBackFullDraftLoadTests`) clasifican `SelectiveDesviadorConfig.CellsByPost` como editable, como sus hermanos. Push Back lo copia y lo compara por estructura, sin otro cambio.
    - `I58F1DomainOracle` lo compara y lo muta, como `BootSidesDeclared`.

**Las pruebas:**
- `RackCadSafetyPanelTests` (UI, nueva, 24):
  - **Selectivo:**
    - los valores por defecto de cada familia, con la tabla igual al BOM, y «Descartar»;
    - la equivalencia con el «Aceptar» del clásico (T4S-01), en WPF real;
    - la bota de un poste y la general;
    - el protector lateral en ningún poste, «Ambos» y «Solo en los extremos»;
    - los niveles del desviador y el poste intermedio con todo apagado (SG-08), «En ninguno» y «En todos los niveles»;
    - los valores rechazados en su campo (longitud, primer nivel, saque, cantidad y ancho de parrilla) sin escribir nada;
    - «Sin variante» con su pregunta, «Conservar», «Quitar» y «Descartar» (SG-06);
    - el lado del tope (SG-07);
    - todo bloqueado mientras AutoCAD escribe;
    - un cambio hecho en otra pestaña llega a la tabla al mostrarla (P1-04);
    - sus palabras;
  - **Dinámico:**
    - sus palabras y la tabla igual al BOM, familia por familia;
    - la regla de los extremos del protector (SG-04);
    - las defensas (SG-03), con un frente más: la propia de 12 in se queda y la automática pasa a 36;
    - la corrección del desviador:
      - un Dinámico nuevo lleva la marca y su último poste gobierna el suyo;
      - un rack existente sin la marca se lee como antes (la columna del último poste no la lee nadie; la del penúltimo gobierna los dos);
      - el paso a la lectura por poste conserva el dibujo, poste por poste, y la marca va al documento y vuelve;
      - en la tabla, un clic en P3 cambia solo P3 y después uno en P2 solo P2;
      - el diálogo clásico conserva la marca, y un rack existente sigue sin ella;
      - un desviador elegido en la ventana la lleva;
    - la guía en el inspector;
  - **por la puerta (doble):** «Actualizar» entrega a RACKSELECTIVOAPLICAR y a RACKDINAMICOAPLICAR la copia del borrador con la seguridad editada en la ventana (la bota propia, la defensa propia con su marca); mientras AutoCAD escribe, la tabla y el inspector no escriben nada, y la copia enviada no cambia;
  - **las vistas en WPF real (STA):**
    - las dos pestañas, sin errores de enlace con todas las familias en el inspector;
    - nombres de UI Automation presentes y únicos;
    - el clic en un nivel, la lista de la bota de un poste, la cabecera de una tarjeta y la casilla de una defensa por su `IToggleProvider`;
    - el inspector debajo a 600 px y al lado a 1500.
- **Comprobación en rojo:** catorce piezas rotas a propósito, una a una, con `RackCadSafetyPanelTests` tras cada una: nueve del panel y cinco de la corrección del desviador.
  - Los scripts son `S\hs65\t4\seg\tools\red.py`, `red2.py`, `red3.py` y `red4.py`.
  - Las salidas son `S\hs65\t4\seg\red-out.txt`, `red-out-2.txt`, `red-out-3.txt` y `red-out-4.txt`.
  1. La defensa escrita igual a la automática vuelve a ser automática (la heurística del clásico): falla la de las defensas.
  2. El protector lateral de Dinámico sale de la regla sin escribir los extremos: falla la de la regla.
  3. Los postes intermedios salen solo del resumen: falla la del poste intermedio con todo apagado.
  4. «Sin variante» quita la familia sin preguntar: falla la de la pregunta.
  5. La pestaña de Selectivo no refresca la tabla al mostrarse:
     - en la primera pasada **sobrevivió**: la otra llamada (`RaiseSafetyState`) refrescaba la tabla en cada cambio hecho con la pestaña a la vista;
     - se añadió la prueba de un cambio hecho en otra pestaña, y con ella falla.
  6. La bota de un poste se escribe solo en la matriz histórica, sin su colocación: falla la equivalencia con el clásico.
  7. Dinámico con las palabras de Selectivo (la rotura intercambia las dos): fallan cinco, las dos de palabras y tres de Selectivo.
  8. El inspector nunca baja en estrecho: falla la de la vista de Selectivo.
  9. Dinámico pasa la seguridad al borrador sin recomponer: fallan cinco, entre ellas la de la puerta, porque lo enviado no la llevaba.
  - **La corrección del desviador:**
    - **A.** La regla de la celda ignora la marca, y el Dinámico vuelve a la lectura histórica. Fallan dos pruebas:
      - la del Dinámico nuevo, cuyo último poste gobierna el suyo;
      - la de la tabla, que ya no liga los postes.

      La del rack existente sigue verde, como debe.
      - En `red3.py` el texto no se encontró, porque `SelectiveDesviadorPlan.cs` usa CRLF; la pieza se repitió con `red4.py`.
    - **B.** El paso a la lectura por poste solo pone la marca y no da al último poste la columna que leía. Falla la del paso, porque el dibujo cambia.
    - **C.** El diálogo clásico no conserva la marca al aceptar. Falla la del diálogo.
    - **D.** La marca no va al documento. Falla la del paso, en la vuelta por el documento.
    - **E.** Un Dinámico nuevo no lleva la marca. Fallan dos pruebas:
      - la del Dinámico nuevo;
      - la del diálogo clásico (un Dinámico de RACKNUEVO).

  Cada archivo se restauró desde sus bytes, con fecha nueva, y su huella se comparó con la de antes: son idénticas.
- **Los números:** las pasadas finales, con la corrección del desviador y los censos:
  - **UI completa:** 2780 correctas, 0 con error y 48 omitidas, de 2828. Es la base (2756/0/48) más las 24 de `RackCadSafetyPanelTests`.
  - **Core Full:** 19147 correctas, 0 con error y 403 omitidas, de 19550: igual que la base.
  - **Plugin Debug:** 0 errores. Hay 2 advertencias, las MSB3277 de las referencias de AutoCAD, que ya existían.
  - **Lo que pasó por el camino:**
    - Una pasada anterior de la UI completa se anuló a los 49 min, con «Proceso de host de pruebas bloqueado». Fue antes de la corrección y con la máquina cargada por las demás partes.
    - En esa pasada falló el temporizador de `RackModalHostWindowTests` («the modal host did not run its work and close by itself»).
    - La final, con `--blame`, terminó entera y en verde en 26 min.
    - La primera Core Full de la corrección se cortó a propósito, tras el fallo del censo del piloto. Se clasificó el campo nuevo y se volvió a pasar entera.

**Lo que queda abierto:**
- **El smoke del Owner en AutoCAD:**
  - la pestaña en la paleta: anchos, el inspector abajo en 600 px y el teclado en la tabla;
  - «Actualizar» con la seguridad editada en la ventana, en los dos sistemas;
  - que RACKEDITAR abra lo escrito y su diálogo lo acepte igual (T4S-01 está probado sin AutoCAD).
- **Para el Owner:** T4S-02, T4S-03, T4S-04, T4S-11, T4S-13 (la política de los racks existentes en la corrección del desviador de Dinámico) y T4S-14 (la columna «Auto» en la rejilla clásica de defensas de Dinámico).
- **Push Back (SG-09):** adopta este diseño en su propio prototipo.
- **El puente al diálogo clásico (T4S-06)** se borra con el clásico.
- **Para integrar:**
  - la parte `ins` también toca `DynamicEditorView.xaml` (puede añadir el mismo `xmlns:local`) y el cableado de los editores en `RackCadWindowViewModel`;
  - `cb` y `va` tocan `SelectiveEditorViewModel.cs`; las ediciones de esta parte son las de SD-14, cuatro líneas;
  - si alguna añade `partial` a `DynamicEditorViewModel`, es la misma palabra;
  - la corrección del desviador toca archivos que comparten los tres sistemas:
    - Domain `SelectiveSafetyConfig`;
    - `SelectiveDesviadorPlan`;
    - los documentos de seguridad;
    - los comparadores de AUTH-13;
    - el diálogo clásico `SelectiveSafetyWindow`.

    Son ediciones pequeñas, y sus comentarios dicen «I-64, cuarta tanda» y la decisión del Owner.

**Correcciones de la revisión** (revisión independiente de «seg», 2026-11-26, sobre `7223d558`)
1. **[GRAVE] AUTH-13 no leía la marca del desviador.**
   - **La causa:** el lector crudo (`AuthoredRawReader.Shapes.cs`, `SafetySelectionDocument`) lee la seguridad con una lista cerrada de miembros y tomaba `DesviadorCellsByPost` como «unknown member».
   - **El efecto:** todo Dinámico con la marca, los nuevos y los editados en la ventana, era ilegible para el comparador.
     - Con dos vistas o más, RACKDINAMICOABRIR, APLICAR e INSERTAR lo rechazaban («No se pudo comprobar que las vistas…»).
     - Con una vista, fallaba la inserción de vistas.
   - **El arreglo:** `case "DESVIADORCELLSBYPOST"`, un booleano anulable.
   - **Otras listas cerradas:**
     - el lector crudo es la única lista cerrada de los miembros de la seguridad; los demás «unknown member» del código son de su envoltura;
     - los comparadores de AUTH-13 (`AuthoredTypedValues`, `AuthoredCanonical`) ya comparaban el campo, y los censos del estado y el oráculo `I58F1DomainOracle` ya lo tenían desde la corrección;
     - el clasificador de cambios de Push Back cubre `$.Structure.SafetySelections` entero;
     - AUTH-15 no lee la seguridad.
   - **Pruebas:**
     - `AuthoredRawReaderSafetyCensusTests` (Core, nueva, 3 pruebas):
       - **el censo:** cada miembro que escriben `SafetySelectionDocument`, `PostSideDocument`, `GridCellDocument`, `PostDefenseDocument` y `BootPostDocument` se saca por reflexión y se da al lector, uno a uno, en el fixture rico de I-58 F1, en Dinámico y en Push Back; ninguno sale «unknown member»;
       - **su control:** un miembro que no escribe nadie sí sale «unknown member» en cada sitio;
       - **los valores de la marca:** ausente y falso son la lectura histórica y se leen igual; verdadero es otro diseño; un `1` es ilegible.
     - `RackSiblingDesignsTests`: dos vistas de un Dinámico nuevo (`DynamicDraftEditor.CreateNew`, con la marca) coinciden («Same»). El puerto `RackAuthoredComparatorPorts.Dynamic()` las lee («Single»), con una vista y con dos, y devuelve la marca.
     - `RackInsertedViewDesignTests`: un Dinámico existente sin la marca, cuyo desviador se edita en la ventana, la gana. Sus vistas redibujadas coinciden, y la cola de ID18 le inserta una vista nueva que también la lleva.
     - Las pruebas que ya existían siguen construyendo con `DynamicSafetyDefaults.Build`, sin la marca: son los racks existentes, que no cambian.
2. **[MEDIO] El diálogo clásico deshacía SG-03.**
   - **La causa:** `SelectiveSafetyWindow` abría la rejilla de defensas con `autoPerEnd: defensaLowEndOnly`, que es falso en Dinámico.
     - «Aceptar» sin tocar nada guardaba los dos extremos como propios, sin procedencia, y descartaba la fila igual a su automática.
     - Así, la salida propia de 12 in de P3 escrita en la ventana pasaba a automática y, con un frente más, a 36 in (PB-010).
   - **El arreglo:** `autoPerEnd: true`. El diálogo solo ofrece la defensa a Dinámico, porque Push Back edita la suya en su ventana, así que el cambio es de Dinámico. Queda declarado como T4S-14.
   - **Prueba** (UI, `RackCadSafetyPanelTests`):
     - P3 lleva una salida propia de 12 in escrita en la ventana, y P2 una fila del clásico anterior;
     - RACKEDITAR › Defensas › «Aceptar» sin tocar devuelve los dos registros iguales;
     - con un frente más, P3 sigue con 12 in.
3. **Diseños de la biblioteca guardados antes de la corrección:** son diseños existentes (T4S-13). Está aclarado en «Quién lleva la marca»: los que nacen con la marca son los creados de cero.

**Comprobación en rojo de las correcciones** (`S\hs65\t4\seg\tools\red5.py`, salida `S\hs65\t4\seg\red-out-5.txt`):
- **R1. El lector crudo vuelve a no conocer `DesviadorCellsByPost`.** Fallan las cuatro pruebas que dependen de leer la marca:
  - el censo;
  - la de los valores de la marca;
  - las dos vistas de un Dinámico nuevo;
  - la del rack existente editado en la ventana.

  El control del censo sigue verde.
- **R2. La rejilla clásica de defensas de Dinámico, otra vez sin «Auto».** Falla la del diálogo clásico.
- Los dos archivos se restauraron desde sus bytes, y su huella es idéntica a la de antes.

**Los números de las correcciones:** las pasadas finales, con las dos correcciones.
- **UI completa:** 2781 correctas, 0 con error y 48 omitidas, de 2829. Es una prueba más que en la entrega, la del diálogo clásico.
- **Core Full:** 19152 correctas, 0 con error y 403 omitidas, de 19555. Son cinco más: las 3 del censo, la de las vistas de un Dinámico nuevo y la del rack existente editado.
- **Plugin Debug:** 0 errores, con las 2 advertencias MSB3277 que ya existían.
- La UI y Core se compilaron desde cero (`--no-incremental`), y el Plugin después.
- La UI completa corrió con `--blame` y terminó entera.

## Cuarta tanda, bom: «BOM y lista de racks» en la ventana (BM-01 a BM-11)

**Rama:** local `architecture/workspace-persistente-rackcad-t4-bom` (worktree `D:\w4bom`), sobre `ac3fd1b0`. Sin commit, sin push, sin AutoCAD.
Prototipo `protos\bom` (BM-01 a BM-11); el Owner aceptó las once propuestas sin nota, y SD-02 mete en la ventana el BOM del borrador del
clásico de Selectivo. Las tres ventanas modales (RACKLISTA, RACKBOMTOTAL y el BOM de un rack) pasan a ser UNA página de la ventana, sin modal y
sin bloquear AutoCAD. No se borró código clásico.

**Lo que se construyó:**
- **La página** (`RackBomPageView` y `RackBomPageViewModel`, en `src/RackCad.UI/VentanaRackCad/`), con los textos del prototipo:
  - **la cabecera:** «BOM y lista de racks», el estado («Sin leer», «Leyendo el dibujo», «Al día», «Puede no estar al día», «Sin total», «No se
    pudo leer»), «Leído a las HH:MM» (y «corrió una orden a las HH:MM»), «Calcular de nuevo», «Exportar Excel» y «Exportar CSV» del dibujo
    completo;
  - **los avisos en línea:** la lectura en curso con su número y «Dejar de esperar»; lo que impide el total, rack por rack con su motivo y
    «Abrir» (BM-05); los racks que quedan fuera del total, nombrados arriba de él (dolor 3); «Puede no estar al día» con «Calcular de nuevo»;
    la pregunta al exportar un BOM desactualizado («Calcular y exportar», «Exportar el de las HH:MM», «Cancelar»); el archivo guardado, con
    «Abrir», «Mostrar en la carpeta», «Guardar como…» y «Cerrar aviso»; y los racks con cambios sin escribir en los editores de la ventana,
    con «Ver su BOM» (BM-02);
  - **las cifras:** racks en el total («de N en el dibujo»), copias, componentes y piezas × copias;
  - **«Racks del dibujo»:** la tabla (rack y vistas con su punto de estado y su etiqueta «Bloqueado», «Fuera del total» o «Con cambios»,
    sistema, copias, componentes —«pieza suelta» en una cama o una cabecera sola—, piezas de una copia y «En el dibujo»), en el orden de la
    columna lateral y ordenable por cuatro encabezados, y el inspector con el BOM del rack elegido: «Abrir», «Ver en el dibujo», «Siguiente
    copia» (con más de una copia), «Escrito en el dibujo» / «Borrador del editor», la nota de bloqueado o fuera del total, el resumen («n
    componentes y m piezas por copia · × k copias = … piezas en el dibujo»), el árbol de componentes que se despliegan varios a la vez, con
    «c/u» y «Por copia» en cada pieza (dolores 7 y 9), «Desplegar todo» / «Plegar todo», las piezas de un rack plano y sus dos «Exportar»;
  - **«BOM total»:** las partidas del dibujo con su columna «Racks», la marca «varias descripciones» y, desplegada, la nota del hallazgo de
    I-68, «Sale de: …» (rack por rack, con sus copias) y las piezas con su longitud (dolor 8); el filtro por componente, «Desplegar todo» /
    «Plegar todo», el pie «Total del dibujo» y la nota de las tres hojas del Excel.
- **La lectura** es `RACKBOMLEER` (`src/RackCad.Plugin/RackInventarioCommands.Ventana.cs`), una orden con `NoUndoMarker | UsePickSet |
  Redraw | NoHistory` que toma la petición que espera en la puerta, dice que la tomó (`Started`), comprueba el dibujo de la petición, lee UNA
  vez la tabla de bloques (con el conteo de referencias) y el registro de variables, carga el catálogo una vez (si falla, la lectura falla
  con su motivo) y entrega los hechos a la regla pura `RackBomReadingRules.Compose`
  (`src/RackCad.Application/VentanaRackCad/RackBomPage.cs`) con el manejador de cada tipo (su `BomLabel`, `OutputBlockedReason` y `BuildBom`).
  La regla es la de RACKBOMTOTAL (I-42, I-47 G13): la autoridad sobre todas las vistas, la puerta de un rack bloqueado antes de construir
  nada, el vínculo roto que aborta el total y el diseño ilegible que queda fuera con su motivo. Añade lo de la página: el BOM de cada rack que
  sí se puede cotizar aunque no haya total (BM-05), solo los racks con copias (BM-06), las palabras de la columna lateral (BM-07) y la ruta y
  el nombre del dibujo para los archivos. En la línea de órdenes deja un resumen de una línea («RackCad: BOM leído para la ventana RackCad: 6
  racks.»).
- **La puerta:** `IRackCadBomPort : IRackCadRackDoorPort` (en `RackBomPage.cs`), implementada por `RackCadRackDoorPort` en su parcial
  `RackCadRackDoorPort.Bom.cs` con el mismo `Send` que la puerta a un rack (documento activo, sin orden en curso, la petición guardada antes de
  entregarla, el foco de vuelta al dibujo) y su propio hueco, que «Dejar de esperar» (`Abandon`) suelta. En la ventana la lectura es una
  petición más de la puerta: la única en vuelo para toda la ventana (`DoorRequest.IsBom`), numerada, con «Dejar de esperar», la respuesta solo
  para su número y su dibujo, la respuesta durante la llamada (M-22) y la poda de un dibujo cerrado.
- **Cuándo se lee (BM-01, BM-S2):** solo tras un gesto del usuario: la primera vez que la página se abre con un clic para un dibujo, y
  después con «Calcular» (sin lectura) o «Calcular de nuevo». Cambiar de dibujo con la página a la vista no envía nada: un dibujo nunca leído
  dice «Sin leer» y ofrece «Calcular». Si AutoCAD está ocupado, la página lo dice y no vuelve a pedir sola. Tras cualquier orden que no sea la
  propia lectura, «Puede no estar al día»; una lectura que se deja de esperar, que no se entrega o cuya respuesta llega con otro dibujo activo
  deja la anterior como «puede no estar al día». Cada dibujo guarda su última lectura; al volver a él, se muestra como «puede no estar al
  día» (como V-16 de la lista).
- **El BOM del borrador (BM-02, SD-02):** un rack con cambios sin escribir en un editor de la ventana (Selectivo o Dinámico) se ve «Escrito en
  el dibujo» o «Borrador del editor». El del borrador se calcula en la ventana, sin leer el dibujo, con el BOM del manejador sobre copias:
  Selectivo, `SelectiveBomBuilder.Build` sobre `SelectiveDraftEditor.ResolveCopy()` (el BOM impone sobre lo que lee, I-1 de la revisión de
  la entrega 2); Dinámico, `SystemBomBuilder.Build` sobre el sistema de `SnapshotForWrite()`. Verlo no cambia el borrador (prueba). Se exporta
  lo que se ve, con «(borrador)» en el nombre, y sin la pregunta de BOM desactualizado (no es del dibujo).
- **Los archivos (BM-03, BM-04):** «Exportar» guarda sin diálogo junto al dibujo: «<dibujo> - BOM.xlsx/.csv» y «<dibujo> - <rack> - BOM[
  (borrador)].xlsx/.csv» (`RackBomFiles`; en Documentos sin reemplazar nada y con escritura atómica, BM-S5), con los exportadores de I-68 sin cambios salvo el CSV del dibujo, que gana «Cantidad_total»
  (cambio declarado del formato, BM-04). El aviso en línea sustituye al `MessageBox`; «Guardar como…» abre el diálogo del sistema y escribe allí
  el mismo archivo. `IRackBomFiles` (con `RackBomFileShell`, el disco y el shell de Windows) deja probar todo sin tocar el disco.
- **El total (BM-08):** `ConsolidatedBomBuilder.Provenance` dice, alineado con `GrandTotal`, de qué racks sale cada partida (cantidad de una
  copia y copias) y qué descripciones junta, con la MISMA clave de la unión (extraída a `KeyOf`, sin cambiar el total). La suma no cambia.
- **La tabla y los filtros (BM-09):** la tabla sigue el buscador (sin mayúsculas ni acentos), el sistema y «Solo con cambios sin escribir»
  de la columna lateral; el total, las cifras y el Excel del dibujo son siempre el dibujo completo.
- **«Ver su BOM» (BM-10):** un botón en la página del rack (visible con un rack activo) abre la página con ese rack elegido. Los editores de
  la ventana siguen sin BOM (decisión 7 de Dinámico); los ocho botones de los editores clásicos no cambian.
- **RACKLISTA, RL, RACKBOMTOTAL y RB (BM-11)** abren la ventana en esta página, en su pestaña («Racks del dibujo» o «BOM total»), y leen
  el dibujo DENTRO de su propia orden (la misma lectura que RACKBOMLEER, sin marca de Deshacer): la página recibe esa lectura para el dibujo
  activo y no se envía una segunda orden. La página ya no ofrece los botones «Abrir la lista de racks» / «Abrir el BOM total».

**Supuestos y decisiones de la sesión** (prefijo BM-S; cada uno aislado en un sitio):

| Id | Supuesto o decisión | Dónde se cambia |
|---|---|---|
| BM-S1 | **«Puede no estar al día» = corrió una orden.** La ventana no sabe si el dibujo cambió: la página lo infiere de la presentación del host (`CommandActive` o `ModalActive`) después de su lectura, sin contar su propia lectura: RACKBOMLEER desde que dice que tomó la petición (`Started`) —o RACKLISTA / RACKBOMTOTAL, que leen dentro de sí— hasta ver el dibujo en reposo otra vez. Una lectura que se deja de esperar, que no se entrega o cuya respuesta es de otro dibujo deja la anterior como «puede no estar al día» (corrección 1 de la revisión). También cuenta un «Actualizar» de un editor (escribe el dibujo) y una lectura de un editor (no lo cambia). **Hueco declarado:** con la paleta oculta el host no presenta nada, así que una orden de entonces no se ve | `RackBomPageViewModel.Present` |
| BM-S2 | **Ninguna orden sale de la ventana sin un gesto del usuario** (corrección 4 de la revisión; antes la página enviaba sola la lectura pendiente al ver el dibujo libre). La página envía RACKBOMLEER solo tras un clic: al abrirla con un clic para un dibujo nunca leído, con «Calcular» / «Calcular de nuevo» y con «Calcular y exportar». RACKLISTA, RL, RACKBOMTOTAL y RB leen el dibujo dentro de su propia orden (la misma función que RACKBOMLEER) y entregan la lectura a la página sin enviar otra orden. Las cinco órdenes llevan `NoUndoMarker \| UsePickSet \| Redraw` (RACKBOMLEER, además, `NoHistory`). Cambiar de dibujo con la página a la vista no envía nada («Sin leer», «Calcular»); AutoCAD ocupado se dice y no se reintenta solo. Para el smoke: que la selección previa y el foco se conservan (`UsePickSet`, `Redraw`; sin AutoCAD no se pudo comprobar) | `RackBomPageViewModel.Shown` y `Present`; `ReadFlags` y `OpenBomPage` en `RackInventarioCommands.Ventana.cs` |
| BM-S3 | RACKLISTA y RACKBOMTOTAL ya no abren sus ventanas: sus cuerpos clásicos siguen, sin registrar, como `RackListaClassic` y `RackBomTotalClassic` (sus guardas de fuente pasan igual). Volver a la ventana clásica antes del smoke es devolver dos atributos | `RackInventarioCommands.cs`, `RackInventarioCommands.BomTotal.cs` |
| BM-S4 | Lo que en RACKBOMTOTAL abortaba TODO el listado y no es un rack bloqueado ni un vínculo roto (un tipo sin manejador, una definición colocada que no se puede clasificar) impide aquí el total como ellos (sin total ni exportación del dibujo, con su línea en el aviso), pero la tabla sigue con el BOM de los demás (BM-05 generalizada). Un registro de variables que existe y no se puede leer deja la página sin lectura | `RackBomReadingRules.Compose` |
| BM-S5 | **Pregunta 3:** «junto al dibujo», sin carpeta de salida en «Configuración». Junto a un dibujo guardado, un archivo con el mismo nombre se reemplaza sin preguntar (es la exportación anterior del mismo BOM). Un dibujo nunca guardado va a Documentos, y el aviso lo dice; allí un archivo con ese nombre puede ser de otro «Drawing1», así que **nunca se reemplaza**: el nuevo se numera («Drawing1 - BOM (2).xlsx») y el aviso lo dice; si no se puede saber si existe, se toma como existente. Dos racks cuyo nombre da el mismo archivo (el mismo nombre, o «PB/1» y «PB:1» tras limpiar) llevan los ocho primeros caracteres de su identidad («Planta - PB_1 (a1b2c3d4) - BOM.xlsx»). La escritura es **atómica**: un temporal en la misma carpeta y `File.Replace` (o `File.Move` si no había archivo), el patrón de `AtomicFile`, que es interno de Application y escribe texto. Si no se puede escribir (por ejemplo, abierto en Excel), el aviso lo dice y ofrece «Guardar como…» (corrección 3 de la revisión) | `RackBomPageViewModel.DoExport`, `RackBomFiles.RackFileName` y `WithNumber`, `RackBomFileShell.Write` y `FallbackFolder` |
| BM-S6 | `Cantidad_total` del CSV del dibujo: en una fila de rack, «Cantidad» × copias; en «TOTAL DEL DIBUJO», ya multiplicada, igual a «Cantidad». El CSV se escribe como lo escribía el clásico (UTF-8 sin marca) | `ConsolidatedBomCsvExporter.Row`, `RackBomPageViewModel.Text` |
| BM-S7 | «Con cambios» y «Borrador del editor» son de los editores de la ventana con cambios sin escribir (no un rack nuevo, que no está en el dibujo). El punto de estado de un rack sin editor de la ventana es el de la columna lateral (el piloto de Push Back incluido), pero el piloto no tiene BOM de borrador | `RackCadWindowViewModel.Bom.cs` (`HasUnwrittenDraft`, `DraftBomOf`, `StateOf`) |
| BM-S8 | «Ver en el dibujo» y «Siguiente copia» hacen activo el rack (como un clic en la lista, que lo marca) y usan la navegación de su página («Ver»; la siguiente copia cuando ya se conoce el número de copias), sin salir de la página del BOM. Si el rack abierto tiene texto sin aplicar y el navegador no cambia de rack, la página lo dice y no navega | `ViewBomRackAsync`, una condición en `OnBrowserChanged` |
| BM-S9 | Durante la lectura del BOM se ven dos «Dejar de esperar» con la misma acción: el del panel de espera de la ventana y el del aviso de la página (nombres de automatización distintos) | `RackBomPageView.xaml` |
| BM-S10 | Las ayudas de RACKAYUDA de RACKLISTA y RACKBOMTOTAL no cambian (siguen siendo ciertas en lo que dicen); es de la parte `ut` o de F9 | — |
| BM-S11 | Por debajo de 760 px de ancho, el inspector va debajo de la tabla (como O-3 de Dinámico) | `RackBomPageView.xaml.cs` |

**Ediciones a archivos compartidos** (pequeñas y marcadas `T4 bom`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `DoorRequest` gana `IsBom`; una línea en el constructor (`WireBom()`); el texto de
  espera de una lectura del BOM en `WaitingText`; una rama en `StopWaiting` (`BomStopped()`); una en la poda de una petición huérfana; una
  línea al final de `ShowPage` (`BomPageShown`); una en `Present` (`PresentBom`); y `!_bomKeepsPage` en `OnBrowserChanged`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: dos bloques `<!-- T4 bom -->`: la página «BOM y lista de racks» pasa a ser un
  `ContentControl` (`BomPage`) en lugar del paso intermedio, y el botón «Ver su BOM» en la página del rack, antes de `PanelToggleButton`.
  `RackCadWindowView.xaml.cs`: una línea en el constructor (`BomPage.Content = new RackBomPageView(...)`) y el manejador del botón.
- `src/RackCad.UI/VentanaRackCad/RackListViewModel.cs`: `KindOrder` y `NaturalNameComparer` pasan de `private` a `internal` (la página
  ordena como la lista y una prueba fija que las palabras son las mismas).
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: `partial` y una línea en `Abandon` (`AbandonBomRead`).
- `src/RackCad.Plugin/RackInventarioCommands.cs` y `RackInventarioCommands.BomTotal.cs`: sus atributos `[CommandMethod]` (y los alias)
  se mudan a `RackInventarioCommands.Ventana.cs`; los cuerpos clásicos no cambian (BM-S3).
- `src/RackCad.Application/Bom/ConsolidatedBom.cs`: la clave de la unión se extrae a `KeyOf` (el mismo texto, con el separador U+0001), y
  se añaden `Provenance` y sus dos registros. `ConsolidatedBomCsvExporter.cs`: la columna `Cantidad_total` (BM-04).
- Archivos nuevos: `RackBomPage.cs` (Application), `RackCadRackDoorPort.Bom.cs` y `RackInventarioCommands.Ventana.cs` (Plugin),
  `RackBomPageViewModel.cs`, `RackBomPageView.xaml(.cs)`, `RackBomFiles.cs` y `RackCadWindowViewModel.Bom.cs` (UI), y las dos clases de
  prueba.

**Censos y guardas que cambian, con su nota:**
- `CustomPropertiesCommandGuardTests`: `A14BomCommands = { "RACKBOMLEER" }` entra en la base; RACKLISTA, RL, RACKBOMTOTAL y RB siguen en el
  censo de apertura (cambian de archivo, no de nombre).
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 51 a 52.
- `BomExcelCharacterizationTests.I17_ConsolidatedCsv_SinCambios`: el CSV esperado gana `Cantidad_total` (BM-04).
- `RackCadWindowTests.ACardOfNuevoAndAButtonOfAPageRunTheirClassicAction`: el botón de una página que corre su acción clásica pasa a ser
  RACKAYUDA de «Nuevo», porque «BOM y lista de racks» ya no tiene botones clásicos.
- Las guardas de capas y de `SendStringToExecute` de `RackCadWindowListTests` pasan sin cambios: el parcial de la puerta usa `Send`, y la
  orden de lectura vive fuera de `Plugin/VentanaRackCad` (que no admite `LockDocument` ni `StartTransaction`).

**Las pruebas:**
- Core, `tests/RackCad.Tests/Workspace/RackBomReadingTests.cs` (nuevo, 17 casos): la lectura de cuatro sistemas con sus copias,
  vistas y palabras, y el rack sin copias fuera; el registro leído una vez y el mismo para todos; el registro ilegible; el rack bloqueado (la
  puerta antes del BOM, sin total, los demás con BOM); el vínculo roto; el tipo sin manejador; la definición colocada sin clasificar y la no
  colocada; el Selectivo con vistas distintas fuera del total; el diseño ilegible y el manejador que lanza; el dibujo sin racks; las palabras
  y el orden; la procedencia del total (con descripciones unidas, sin cambiar la suma); los nombres de los archivos y la carpeta; el CSV con
  `Cantidad_total`; y, sobre la fuente, RACKBOMLEER (sin marca de Deshacer, una lectura del registro y de la tabla, la regla, el orden de la
  petición, sin escribir), RACKLISTA y RACKBOMTOTAL hacia la página con los cuerpos clásicos sin registrar, y la puerta (`Send` y `Abandon`).
- UI, `tests/RackCad.UI.Tests/Workspace/RackCadBomPageTests.cs` (nuevo, 27 casos): la primera lectura sola, su número, una sola
  petición para la ventana; solo la respuesta de su número; la respuesta durante la llamada; «Dejar de esperar» y la respuesta tardía; AutoCAD
  ocupado y la lectura pendiente; «Puede no estar al día» tras una orden y no tras la propia lectura; las lecturas por dibujo, la vuelta a un
  dibujo y el dibujo cerrado; el rack bloqueado con «Abrir»; el que queda fuera; la lectura fallida; el orden de la columna y los
  encabezados; el filtro de la columna; las filas; el inspector con varios componentes desplegados, «c/u» y «Por copia»; el borrador con sus
  dos fuentes, su exportación y que verlo no cambia el borrador; el total con su procedencia y su filtro; exportar junto al dibujo con
  «Abrir», «Mostrar en la carpeta» y «Guardar como…»; el dibujo sin guardar y el fallo de escritura; la pregunta del BOM desactualizado;
  RACKLISTA y RACKBOMTOTAL; «Ver su BOM»; «Ver en el dibujo» y «Siguiente copia» sin salir de la página; sin la puerta del BOM; las palabras
  iguales en la columna y la página; y con WPF real (STA), sin errores de enlace: la página y sus botones (lectura, fila elegida, borrador,
  un componente desplegado por su `IToggleProvider`, la exportación y su aviso, las pestañas, el encabezado que ordena, nombres de
  automatización únicos), el inspector debajo de la tabla en estrecho, y la ventana que aloja la página y ofrece «Ver su BOM».

**Comprobación en rojo:** dieciocho piezas rotas a propósito, una a una, con su clase de pruebas tras cada una (script y salida en el
scratchpad de la sesión, `hs65\t4\bom\red.py`, `red-out.txt` y `red-out-2.txt`); cada una falló, y cada archivo volvió byte a byte a su copia
(SHA-256 comprobado por el script):
1. el total se hace aunque haya racks que lo impiden: fallan 4 (bloqueado, vínculo roto, tipo sin manejador, definición sin clasificar);
2. la puerta de I-42 se pregunta después de construir el BOM: fallan 3;
3. el registro no llega al manejador: falla la del registro leído una vez;
4. se listan los racks sin copias: falla la de la lectura;
5. la procedencia sin las copias de cada rack: falla la de la procedencia;
6. `Cantidad_total` sin multiplicar por las copias: fallan la nueva y la caracterización I17;
7. RACKBOMLEER con marca de Deshacer: falla la guarda de fuente;
8. «Dejar de esperar» que no suelta la lectura del BOM en la puerta: falla la guarda de la puerta;
9. la propia lectura cuenta como un cambio del dibujo: falla la de «Puede no estar al día»;
10. se acepta la respuesta de otro número: falla la de la respuesta;
11. la tabla no sigue la columna lateral: falla la del filtro;
12. el inspector ignora el borrador del editor: falla la del borrador;
13. exportar con el BOM desactualizado no pregunta: falla la de la pregunta;
14. «Ver en el dibujo» se va a la página del rack: falla la de la navegación;
15. la página no lee sola la primera vez: fallan 24 (casi todas parten de esa lectura);
16. el borrador se exporta sin «(borrador)»: falla la del borrador;
17. el inspector no baja en estrecho: falla la de la vista estrecha;
18. una lista nueva de filas pierde el rack elegido en la tabla: falla la de la vista.

La 18 es un fallo real que encontró la prueba de vista antes de corregirlo: con `SelectedValue` ligado en los dos sentidos, cada lista nueva
de filas (un filtro, un orden, cada presentación del host) borraba el rack elegido. Ahora la vista sincroniza la selección (`SyncSelection`)
y solo un clic del usuario elige.

**Los números** (solución recompilada en Debug con `--no-incremental`, sobre las fuentes finales):
- **UI completa:** 2783 correctas, 0 con error, 48 omitidas (2831): la base de `ac3fd1b0` (2756 / 0 / 48) más las 27 nuevas.
- **Core Full:** 19164 correctas, 0 con error, 403 omitidas (19567): la base (19147 / 0 / 403) más las 17 nuevas.
- **Build Debug de la solución (Plugin incluido):** 0 errores. Las advertencias son las MSB3277 de las referencias de AutoCAD y las de los
  analizadores de xunit, que ya estaban; ninguna sale de los archivos de esta parte.

**Lo que queda abierto:**
- **El smoke del Owner en AutoCAD:** RACKBOMLEER sin paso en Deshacer (lee la tabla de bloques y el registro en una transacción, como
  RACKSELECTIVOABRIR, cuyo smoke también espera); la lectura al abrir la página con un clic y dentro de RACKLISTA / RACKBOMTOTAL, con la
  selección previa y el foco conservados (BM-S2); «Puede no estar
  al día» tras un MOVE y no tras la lectura (BM-S1); los archivos junto al dibujo, «Abrir» con Excel, «Mostrar en la carpeta» y «Guardar
  como…» desde la paleta; «Ver en el dibujo» y «Siguiente copia»; un dibujo de unos 70 racks (tiempo de la lectura y de la tabla).
- **Para el Owner:** BM-S1 (qué cuenta como cambio), BM-S4 (el total impedido por un tipo sin manejador), BM-S5 (reemplazar sin preguntar, y
  Documentos para un dibujo sin guardar) y BM-S6.
- **No entra en esta tanda:** el BOM de varios racks elegidos (pregunta 9: «no en esta entrega»); los hallazgos de I-68 sobre la clave del
  total, el total físico y el Excel de un rack sin tabla (son reglas del BOM, no de la pantalla: la página solo los dice); el teclado en la
  tabla y en el árbol más allá del de WPF; la lista desplegable del filtro por componente tiene el riesgo O-D de dos monitores.
- **Para integrar:** `RackCadWindowViewModel.cs` (siete líneas marcadas), `RackCadWindowView.xaml` (dos bloques), `RackListViewModel.cs`
  (dos palabras) y `RackCadRackDoorPort.cs` (dos líneas) los tocan también otras partes; la parte `ut` cambia la ayuda y la parte `va` la
  página «Variables», que la prueba de `RackCadWindowTests` ya no usa.

### Cuarta tanda, bom: correcciones de la revisión (sobre `5d8c28fc`)

En `D:\w4bom`, sobre el commit `5d8c28fc` de la parte (lo hizo el coordinador tras el reinicio). Sin commit, sin push, sin AutoCAD. Los
puntos 1 a 3 llevan primero su prueba focal, que reproduce el fallo con la corrección deshecha; después, la corrección y la comprobación en
rojo.

1. **[MEDIO] Un BOM viejo se veía «Al día» y se exportaba sin preguntar.** Causa: la marca de «la orden que corre es la propia lectura»
   (`_ownCommandPending`) se ponía al entregar la petición y no la soltaban ni «Dejar de esperar» ni una lectura no entregada, así que una
   orden del usuario después no marcaba nada. Ahora la propia lectura cuenta solo desde que RACKBOMLEER dice que tomó la petición
   (`door?.Started(pending.RequestId)`, el mismo aviso que ya usan las escrituras: `OnRequestStarted` admite la lectura del BOM y llama a
   `BomTaken`) hasta el reposo tras su respuesta. `ReadingStopped` suelta la marca y, por prudencia, deja la lectura que tenía el dibujo como
   «puede no estar al día» («una lectura posterior no se completó»). Con la petición tomada, la espera dice «AutoCAD está leyendo el dibujo
   para el BOM: la respuesta llega al terminar la lectura.» y no ofrece «Dejar de esperar», como una escritura tomada. Pruebas:
   `ACommandAfterTheReadingSaysItMayBeOutOfDateAndOnlyTheReadThatStartedIsNotOne` (reescrita: una orden vista entre la entrega y `Started` sí
   marca) y `AReadThatIsAbandonedOrNotHandedOverLeavesTheReadingPossiblyOutOfDateAndExportAsks` (nueva).
2. **[MEDIO] Una exportación quedaba armada y salía después.** Causa: con la respuesta dentro de la llamada (M-22), `_afterRead` se asignaba
   después de `Deliver`, que ya había pasado, y la exportación salía con la siguiente lectura. Ahora «Calcular y exportar» la arma ANTES de
   entregar la lectura y la desarma si no se entregó; «Calcular» la desarma siempre (nunca exporta). Prueba:
   `AnExportThatWaitsForTheReadIsMadeOnceAndNeverGoesOffLater` (nueva: la respuesta en la llamada exporta una vez, nada sale después, y una
   lectura no entregada no deja nada armado).
3. **[MEDIO] Se podía perder un archivo del usuario.** En Documentos (dibujo nunca guardado) nunca se reemplaza un archivo: el nuevo se
   numera, « (2)», « (3)»…, y el aviso lo dice (`RackBomFiles.WithNumber`; `IRackBomFiles` gana `Exists`). Dos racks cuyo nombre da el mismo
   archivo llevan los ocho primeros caracteres de su identidad (`RackBomFiles.RackFileName` con los racks de la lectura). La escritura es
   atómica (`RackBomFileShell.Write`: temporal en la misma carpeta y `File.Replace` o `File.Move`; `AtomicFile` es `internal` de Application y
   escribe texto, así que se reproduce su patrón). BM-S5 actualizado. Pruebas: `AFileOfTheUserIsNeverReplacedInTheDocumentsAndTwoRacksWithOneNameGetTwoFiles`
   y `TheFilesOfTheMachineAreWrittenAtomically` (nuevas; la segunda escribe en una carpeta temporal real y, como una escritura interrumpida
   no se puede provocar en una prueba, fija el patrón en la fuente), y los nombres en `TheFilesGoNextToTheDrawingWithTheirAutomaticNames`.
   `ADrawingNeverSavedExportsToTheDocumentsAndAFailureOffersSaveAs` cambia: el segundo archivo es «Drawing1 - BOM (2).xlsx».
4. **[MEDIO-BAJO] Lectura sin clic.** Se quitó la lectura «pendiente» (`Wanted`) y su envío al presentar el dibujo libre. RACKLISTA, RL,
   RACKBOMTOTAL y RB leen el dibujo dentro de su propia orden (`ReadBomOfDrawing(document, 0, …)`, la misma función que RACKBOMLEER) y la
   entregan con `AskBomPage(tab, reading)`; la ventana trae el estado del host y la da a la sesión activa (`Deliver(…, readByRunningCommand:
   true)`: el resto de esa orden es la lectura, no un cambio) antes de mostrar la página, que así no envía nada. Cambiar de dibujo con la
   página a la vista no envía nada («Sin leer» y «Calcular»; el botón dice «Calcular» sin lectura y «Calcular de nuevo» con ella). Las cinco
   órdenes llevan `ReadFlags = NoUndoMarker | UsePickSet | Redraw` (RACKBOMLEER, además, `NoHistory`). BM-S2 actualizado. Pruebas:
   `ABusyAutoCadIsSaidAndNothingIsSentLaterWithoutAClick`, `TheReadingsAreKeptPerDrawingAndComingBackToOneSaysItMayBeOutOfDate` y
   `RackListaAndRackBomTotalHandTheirReadingToThePageOnTheirTabWithNoSecondCommand` (reescritas) y las guardas de fuente de la orden
   (`TheListAndTheBomTotalOpenThePageOfTheWindowAndTheClassicFlowsStayUnregistered`: leer dentro, sin `RequestBomRead` ni `Send`).
5. **[BAJO] La sesión de la petición no se comprobaba.** La puerta no ve las sesiones del host (M-24), así que la comprobación está en la
   ventana: al llegar la respuesta (dentro de RACKBOMLEER, en el dibujo activo) trae el estado del host y solo la usa si el dibujo activo es
   el de la petición; si no, la descarta con su aviso («La lectura n.º N no es del dibujo para el que se pidió…»). Prueba:
   `AnAnswerThatArrivesWithAnotherDrawingActiveIsNotUsed`.
6. **[BAJO] Un catálogo que falla salía como tipo desconocido.** `ReadBomOfDrawing` carga el catálogo una vez, antes de `Compose`; si falla
   o no hay, la lectura falla con su motivo («No hay BOM: no se pudo cargar el catálogo de RackCad.»). Y `Compose`, si resolver el manejador
   lanza, devuelve una lectura fallida, no «tipo desconocido». Pruebas: `AHandlerThatCannotBeResolvedIsAFailedReadingNotAnUnknownKind` y la
   guarda de fuente de RACKBOMLEER (el catálogo antes de la regla).
7. **[BAJO] Si `OutputBlockedReason` lanzaba, el rack salía y el total se hacía.** Ahora el rack queda «Bloqueado» («no se pudo comprobar si
   se puede cotizar (falló su diagnóstico).») e impide el total, como abortaba el clásico (regla de I-42). Prueba:
   `AGateThatThrowsBlocksTheRackAndTheTotal`.
8. **El nombre del rack (cambio declarado):** es el primer nombre no vacío entre sus vistas (`RackListBuilder`, como la columna lateral);
   RACKBOMTOTAL usaba el nombre del sobre de la vista representativa. Prueba: `TheNameOfARackIsTheFirstNameNotEmptyAmongItsViews`.
9. **Las guardas `GUARDA_RACKBOMTOTAL_*`** de `SelectiveBomAuthorityTests` vigilan ahora el camino vivo: la orden
   (`RackInventarioCommands.Ventana.cs`) y las reglas (`RackBomPage.cs`): «la orden no resuelve variables» (en los dos archivos), el registro
   leído una vez, la autoridad antes de cotizar, el vínculo roto (`RackBomRackState.BrokenLink`), el sobre indescifrable
   (`includeReferenceCount: true`) y la identidad no inventada. Siguen, sin cambios, las guardas que leen el cuerpo clásico sin registrar
   (`RackInventarioCommands.BomTotal.cs`): `PushBackBomCommandGuardTests`, `KindHandlerGuardSourceTests`, `G16UnnamedRackTests`,
   `I60RackNameWiringGuardTests`, `PushBackRoundTripSourceGuardTests` y `WorkspaceNavigationHostTests`; sus propiedades en el camino vivo las
   fijan las pruebas puras de `RackBomReadingTests` (la puerta de I-42 antes del BOM y sin total, el tipo sin manejador, la agrupación por
   identidad y «(sin nombre)»). Reapuntarlas o retirarlas es de F9, cuando se retire el cuerpo clásico.

**Archivos de la corrección:** `src/RackCad.Application/VentanaRackCad/RackBomPage.cs`, `src/RackCad.Plugin/RackInventarioCommands.Ventana.cs`,
`src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Bom.cs`, `src/RackCad.UI/VentanaRackCad/RackBomPageViewModel.cs`,
`RackBomPageView.xaml`, `RackBomFiles.cs`, `RackCadWindowViewModel.Bom.cs` y `RackCadWindowViewModel.cs` (compartido: tres líneas marcadas
`T4 bom` en `OnRequestStarted`, `WaitingText` y `StopWaiting`), y las pruebas `tests/RackCad.Tests/Workspace/RackBomReadingTests.cs`,
`tests/RackCad.Tests/SelectiveBomAuthorityTests.cs` y `tests/RackCad.UI.Tests/Workspace/RackCadBomPageTests.cs`.

**Comprobación en rojo de las correcciones** (script y salida en el scratchpad de la sesión, `hs65\t4\bom\red2.py` y `red2-out.txt`; cada
archivo volvió byte a byte a su copia, SHA-256 comprobado por el script):
1. punto 1 entero (la propia lectura cuenta desde la entrega, y `ReadingStopped` no suelta la marca ni marca nada, como en `5d8c28fc`):
   fallan 2, `ACommandAfterTheReading…OnlyTheReadThatStartedIsNotOne` y `AReadThatIsAbandonedOrNotHandedOver…`;
2. solo la cuenta desde la entrega (no desde `Started`): falla la primera;
3. solo una lectura detenida que no marca «puede no estar al día»: falla la segunda;
4. punto 2 (la exportación se arma después de entregar la lectura y «Calcular» no la desarma, como en `5d8c28fc`): falla
   `AnExportThatWaitsForTheReadIsMadeOnceAndNeverGoesOffLater`;
5. punto 3, en Documentos se reemplaza el archivo que estaba: fallan 2 (`AFileOfTheUserIsNeverReplaced…` y `ADrawingNeverSaved…`);
6. punto 3, dos racks con el mismo nombre dan el mismo archivo: fallan la de la página y la de los nombres en Core;
7. punto 3, el archivo se escribe directamente, sin temporal: falla `TheFilesOfTheMachineAreWrittenAtomically` (por la fuente);
8. punto 4, cambiar de dibujo con la página a la vista envía una lectura: fallan 8;
9. punto 4, la lectura de RACKLISTA / RACKBOMTOTAL no llega a la página: falla la de RACKLISTA y RACKBOMTOTAL;
10. punto 4, RACKLISTA / RACKBOMTOTAL piden una segunda orden: falla la guarda de fuente de la orden;
11. punto 5, la respuesta se usa aunque el dibujo activo sea otro: falla `AnAnswerThatArrivesWithAnotherDrawingActiveIsNotUsed`;
12. punto 6, un fallo al resolver el manejador se dice como tipo desconocido: falla `AHandlerThatCannotBeResolved…`;
13. punto 7, un diagnóstico que lanza deja el rack fuera y hace el total: falla `AGateThatThrowsBlocksTheRackAndTheTotal`;
14. punto 9, la orden viva sin el conteo de referencias directas: fallan 2, la guarda de fuente de RACKBOMLEER y
    `GUARDA_EL_SOBRE_INDESCIFRABLE_DISTINGUE_COLOCADO_DE_NO_COLOCADO`;
15. punto 9, las reglas vivas no distinguen el vínculo roto: falla la prueba pura del vínculo roto (`GUARDA_UN_VINCULO_ROTO_ABORTA_EL_TOTAL`
    no cae con esta pieza, porque `RackBomRackState.BrokenLink` sigue en otras líneas de las reglas: la guarda de fuente es la red débil y la
    prueba pura la fuerte);
16. y dos de la primera tanda, reescritas sobre el código nuevo: RACKBOMLEER con marca de Deshacer (falla la guarda de fuente) y el borrador
    exportado sin «(borrador)» (falla la del borrador).

**Los números tras las correcciones:**
- **UI completa** (se tocó la vista): 2788 correctas, 0 con error, 48 omitidas (2836): las 2783 de la primera entrega más las 5 nuevas
  (`RackCadBomPageTests` pasa de 27 a 32 casos).
- **Core, las clases focales** (14: `RackBomReadingTests`, que pasa de 17 a 20 casos, `SelectiveBomAuthorityTests`, los censos
  `CustomPropertiesCommandGuardTests` y `SelectiveEditorOpenTests`, `BomExcelCharacterizationTests` y las guardas que leen la orden, la puerta
  o el cuerpo clásico: `RackCadWindowListTests`, `WorkspaceBridgeSourceGuardTests`, `WorkspaceHostSourceGuardTests`,
  `WorkspaceNavigationHostTests`, `PushBackBomCommandGuardTests`, `PushBackRoundTripSourceGuardTests`, `KindHandlerGuardSourceTests`,
  `G16UnnamedRackTests` e `I60RackNameWiringGuardTests`): 433 correctas, 0 con error. Core Full no se volvió a correr.
- **Plugin en Debug:** 0 errores; 2 advertencias, las MSB3277 de las referencias de AutoCAD que ya estaban.

## Cuarta tanda: integración en la rama de Selectivo (sesión, 2026-11-26)

Las siete partes se integraron con `merge --no-ff`, cada una con su commit y el de las correcciones de su revisión independiente.
Las siete revisiones se hicieron sobre el commit exacto de cada parte. Los informes de los agentes y de las revisiones están en el
scratchpad de la sesión, en `hs65/t4/<parte>/`.

| Orden | Parte | Commit de la parte | Correcciones | Merge |
|---|---|---|---|---|
| 1 | new («Nuevo» con borradores) | `c93f65f3` | `4dfe1e30` | `14a0625c` |
| 2 | ins (panel «Insertar vista…») | `b515bafe` | `ab82779b` | `305d1dbf` y `a1961939` |
| 3 | ut (Biblioteca, Configuración, Ayuda) | `11d60d88` | `d2d019c3` | `6a460082` |
| 4 | cb (editor de cabeceras) | `a2544580` | `21a481d1` | `bb2e1611` |
| 5 | va (Variables y Propiedades) | `89cd65f9` | `4af80253` | `bac98861` |
| 6 | seg (seguridad sin diálogo y desviador de Dinámico) | `7223d558` | `4a658678` | `4910e33f` |
| 7 | bom («BOM y lista de racks») | `5d8c28fc` | `b2ebd0cd` | este commit |

### Hallazgos graves de las revisiones, ya corregidos

- **va:** «Eliminar» del lote no comprobaba los racks que usan la variable. Un rack vinculado después de leer la página quedaba
  con una referencia rota.
- **seg:** la lectura cruda de AUTH-13 no conocía `DesviadorCellsByPost`. Todo Dinámico con la marca salía ilegible y la ventana
  no lo abría, no lo actualizaba ni le insertaba vistas. Ahora hay un censo que pasa cada campo de los documentos de seguridad por
  el lector cerrado.
- **Medios:**
  - ins: el panel se perdía si la puerta rechazaba la escritura; «Aplicar tramos» no igualaba fondos distintos.
  - new: «Duplicar» podía perder trabajo sin aviso.
  - ut: «Guardar» no volvía a comprobar el editor; el arranque no esperaba a AutoCAD en reposo.
  - cb: la copia al mismo poste en otro fondo revertía el peralte.
  - bom:
    - un BOM viejo salía «Al día»;
    - una exportación quedaba armada;
    - un Excel podía pisarse;
    - una lectura salía sin un gesto del usuario.

### Lo que cambió al juntar las partes

- **ut con new:** `OpenDesignAsNewRack` abre el diseño por el punto de entrada de los borradores,
  `OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, nombre, "del diseño «…» de la biblioteca"))`, como un «Borrador N» más.
  - Salen `OneNewRackLibraryText` y `OpenedFromLibraryText`.
  - La prueba «con un rack nuevo en Nuevo no se abre» pasa a «se abre como otro borrador» (SD-06).
- **ins con new y ut:** las pruebas de los borradores y de la biblioteca eligen las vistas con el panel (`InsertWith`), no con
  `ViewChooser`. El motivo «otro dibujo» se lee junto al botón.
- **cb, va y bom en la ventana:**
  - `DoorRequest` lleva `IsCabecera`, `IsVariables` e `IsBom`.
  - `StopWaiting` y el texto de espera atienden las tres.
  - El «Modificado» de Selectivo cuenta la cabecera sin aplicar y la pestaña «Propiedades».
  - `Abandon` de la puerta suelta las peticiones de las tres.
- **seg con ins:** `Wire` de Dinámico ya no asigna el diálogo de seguridad (seg) ni el de vistas (ins).
- **Censos:**
  - la base de órdenes suma las de ut, cb, va y bom;
  - el censo de órdenes del Plugin pasa de 51 a 57;
  - la guarda de capas de `RackCadWindowListTests` exime en UNA condición las órdenes de las puertas de rack, de cabecera y de
    «Variables», con la expresión de cb (que prohíbe también `ApplyCabeceraUpdate`).
- **Ayuda:** ut y va añadían RACKVARIABLES. Queda una sola entrada, con el texto de va, y el censo de la ayuda la cuenta una vez,
  en la lista de ut.
- **`RackCadWindowTests.ACardOfNuevoAndAButtonOfAPageRunTheirClassicAction`:** con BOM y la ayuda ya en la ventana, el botón de
  página que ejecuta una acción clásica es el paso intermedio de «Variables» (sin la puerta de la página).

### Números sobre la integración

- **UI completa:** 2960 / 0 / 48, que es la base 2756 más las 204 de las siete partes. Corrió completa con una prueba en rojo (la
  de arriba, que se ajustó); después pasó su clase entera, 41/41.
- **Core Full:** 19290 / 0 / 403, que es la base 19147 más las 143 de las partes.
- **Solución en Debug (Plugin incluido):** 0 errores.

### Pendiente

- **El smoke de la cuarta tanda en AutoCAD**, con la lista de cada parte en su sección. Hay puntos que solo se ven en AutoCAD:
  - RACKBOMLEER, RACKVARIABLESLEER y RACKCABECERAABRIR, sin rastro en Deshacer;
  - la casilla de arranque;
  - el panel de vistas en la paleta;
  - un Dinámico nuevo con dos vistas, abierto, actualizado y con vistas insertadas;
  - «Auto» en RACKEDITAR › Defensas.
- **La validación del Owner** de la cuarta tanda y de los supuestos de cada parte (ND, IN, UT-S, CB4, BM-S, VA-S y T4S).
- **Lo que falta de cada parte**, en su sección:
  - cb: las cabeceras de Dinámico, «Guardar como plantilla…» y «Nuevo › Cabecera»;
  - va: VA-10 completo;
  - ut: UT-S16, la lectura de la carpeta fuera del hilo de la interfaz;
  - bom: el BOM de varios racks.

### CI de la integración en Linux: reglas de Windows para los nombres de archivo

El primer CI de `b0d367e7` dio 5 pruebas de Core en rojo (UI y la compilación, en verde). En el runner de Linux,
`Path.GetInvalidFileNameChars` admite «:», «*» o «\», y una ruta como «C:\L\a.rackcad.json» no está enraizada. El código de ut
(nombre de archivo de un diseño, petición de RACKBIBLIOTECAABRIR, «Cambiar el nombre») y el de bom (nombres de los archivos del BOM) tomaba esas
reglas del sistema. En Windows, donde corre RackCad, daban lo correcto. La corrección es `WindowsFileNames` (Application): las reglas de Windows
escritas una vez y usadas en los dos sitios. `WindowsFileNamesTests` fija las reglas en cualquier sistema y, en Windows, comprueba que dan lo
mismo que `Path`.

## Cuarta tanda: smoke en AutoCAD (sesión, 2026-11-26, 15:48 a 16:08)

- **Orden del Owner:** «Prueba en AutoCAD primero» (respuesta a «¿Qué sigue ahora?»). AutoCAD estaba cerrado.
- **DLL:** Debug de esta rama en `57c91b49` (CI verde, run 45899305011), compilado con `--no-incremental` y el árbol limpio.
  - SHA-256 (prefijo): Plugin `516842683D703054`, UI `BFAD218D671C5894`, Application `30F63E970537ADDE`, Domain `41429E7B9AA72673`.
  - Cargado con NETLOAD «Load Once»; FILEDIA se puso a 0 solo para la ruta y se devolvió a 1.
- **Dibujo:** una copia de trabajo de `smoke-sel-prep.dwg` (huella `2FA0BC027C615D53`). No se guardó: al salir con `_QUIT` → «No», la copia
  y el original siguen con la misma huella.
- **Dejado como estaba:**
  - la paleta, a su tamaño;
  - la configuración del usuario, sin tocar (la casilla de arranque no se cambió);
  - el diseño de prueba que se guardó en la carpeta de diseños por defecto, borrado al terminar.
- **Queda en la carpeta del smoke:** el Excel del BOM exportado, junto a la copia.

### Resultados (OBSERVADO)

| Paso | Parte | Resultado | Observación |
|---|---|---|---|
| N-1 | new | PASA | «Selectivo en esta ventana» y luego «Dinámico en esta ventana» crean «Borrador 1 · Selectivo» y «Borrador 2 · Dinámico» bajo «Nuevo», con su estado. La cabecera dice «5 racks · todo escrito · 2 borradores sin insertar». La barra tiene «Todos los borradores», «Duplicar» y «Descartar el borrador». |
| N-2 | new | PASA | Insertar el Dinámico (dos vistas) → «AutoCAD insertó el rack nuevo «Dinámico 1» con 2 vistas»; sale de la lista de borradores. |
| N-3 | new | PASA | Con un borrador «Modificado», CLOSE → «No» a guardar → el dibujo no se cierra: «…tiene cambios sin escribir en SEL-SMOKE T4 borrar (Borrador 2, Selectivo nuevo)…». El borrador sin cambios no se nombra (ND-05). |
| I-1 | ins | PASA, con F-1 | SEL-A › «Insertar vista…»: el panel se abre en la paleta, sin nada marcado. Marcadas Planta (1) y Lateral · poste 3 (2) → «Insertar 2 vistas» → «redibujó 2 vistas del rack con el borrador e insertó 2 vistas nuevas». Leído por COM: el bloque «SEL-A - planta» se creó antes que «SEL-A - lateral 3» (el orden elegido). «En el dibujo ya hay: frontal y lateral» (DG-04). |
| **F-1** | ins | **FALLO → CORREGIDO** | Junto a «Frontal» y «Planta» (grupos sin cortes laterales) el panel mostraba el texto `RackCad.UI.VentanaRackCad.RackViewInsertGroupItem`, en Selectivo y en Dinámico. Un `ContentControl` sin plantilla mostraba el nombre del tipo del grupo (corrección 8 de la revisión). Ahora queda plegado y vacío salvo en el grupo con cortes. |
| S-1 | seg | PASA | SEL-A › «Extremos y seguridad»: variante «Protector bota tipo H» en el inspector (6 botas en 3 postes) y P2 a «Izquierda» desde la tabla («Propia · 1 pieza», total 5). Vocabulario de Selectivo: «Izquierda = la cara del fondo 1…». Sin «≈» en las alturas (SD-14). |
| S-2 | seg | PASA | Dinámico nuevo con dos vistas (lleva `DesviadorCellsByPost`). En la tabla, el desviador de P2 se apaga sin tocar el de P1 (el último poste ya tiene el suyo). «Editar aquí» lo abre desde el dibujo (AUTH-13 lee la marca), «Actualizar» redibuja las 2 vistas e «Insertar vista…» (Planta) pasa la comparación de hermanas y la coloca. |
| S-3 | seg | PASA | RACKEDITAR de ese Dinámico › «Elementos de seguridad (5)…» › Defensas «Configurada…»: la rejilla tiene «Auto» por extremo (T4S-14). Se canceló sin escribir. |
| C-1 | cb | PASA | SEL-A › Avanzado › P2 «Editar cabecera…». Con la cabecera calculada sin cambios, «Copiar a otros postes…» no se ofrece, y dice por qué. «Claro» de P1 de 44 a 48 → «lo de arriba se movió con él. La altura del poste es 244 in» → «Aplicar al borrador» → P2 «Personalizada, 244 in» → «Actualizar». Al volver a abrir desde el dibujo: P2 244 in. |
| C-2 | cb | PASA | Cabecera suelta (creada con QUICKCABECERA) › «Editar aquí» (RACKCABECERAABRIR): «Claro» 46 → «Actualizar» → «cabecera actualizada; sus vistas (lateral/planta) se redibujaron». «U» → RACKCABECERAAPLICAR. Aislado: LINE, «Editar aquí», «U» → LINE (la apertura no deja paso en Deshacer). |
| B-1 | bom | PASA | «BOM y lista de racks» con un clic: lee («BOM leído para la ventana RackCad: 6 racks; 1 queda fuera del total»). LINE antes, y luego «U» deshace LINE (RACKBOMLEER sin paso en Deshacer). Tras el «U», la página dice «Puede no estar al día · corrió una orden» (BM-S1). |
| B-2 | bom | PASA | RL abre la página con la lectura hecha dentro de su orden («Al día»). «Exportar Excel» deja «<dibujo> - BOM.xlsx» junto al dibujo, con «Abrir», «Mostrar en la carpeta» y «Guardar como…». |
| V-1 | va | PASA, con F-2 y F-3 | «Variables»: HV de 6 a 8 → «Lo que escribirá «Actualizar»» → «Actualizar» → «AutoCAD escribió el borrador de «Variables» y redibujó 1 rack (4 vistas)». Un solo «U» deshace RACKVARIABLESAPLICAR, y la página releída dice HV 6. RACKVARIABLESLEER sin paso en Deshacer: tras releer, «U» va a RACKSELECTIVOINSERTAR. |
| **F-2** | va | **FALLO → CORREGIDO** | Tras un «Actualizar» correcto, la página mostraba en rojo «Hay un comando en curso en AutoCAD…». La relectura se pide dentro de la respuesta de RACKVARIABLESAPLICAR, con la orden aún en curso, y la puerta la rechaza como Busy. El rojo seguía después de una relectura correcta. Ahora ese Busy no se dice (la página lee al volver a presentarse, cuando la orden terminó) y «Volver a leer del dibujo» limpia un motivo anterior. |
| **F-3** | va | **FALLO → CORREGIDO** | La fila de la tabla de variables salía centrada y sus columnas no coincidían con los encabezados. La plantilla de `SecondaryButtonStyle` centra su contenido. La fila lleva ahora una plantilla que lo estira. |
| V-2 | va | PASA | SEL-A › «Propiedades» › «Nueva propiedad» «Zona = Prueba T4» → «Actualizar» → «…redibujó 4 vistas del rack… También escribió sus propiedades.» Al volver a abrir desde el dibujo, la propiedad está. |
| U-1 | ut | PASA | «Más» › «Guardar en biblioteca…» → página con la línea del nombre → guardado → «Abrir como nuevo en esta ventana» → «Borrador 2: Selectivo nuevo en esta ventana, del diseño «…» de la biblioteca» (por `OpenAsNewDraft`, integración ut + new); P2 conserva su cabecera de 244. «Recientes de la biblioteca» en «Nuevo» lo lista. |
| U-2 | ut | PASA | «Ayuda» › «Órdenes y atajos» (20 órdenes, «Ejecutar»). «Configuración»: carpetas, «Abrir RackCad al iniciar AutoCAD» apagada y «Acerca de RackCad» con «1.0.0 (57c91b49)». |
| U-3 | ut | NO PROBADO | La casilla de arranque necesita el bundle instalado; aquí el plugin entra por NETLOAD. |
| I-2 | ins | NO PROBADO | El motivo en el panel con una petición en vuelo o en solo lectura (está en pruebas de la UI). |

### Observaciones

- **O-1 (anterior a la cuarta tanda; para planear):** «Volver a leer la lista» deja un paso vacío en Deshacer. El «U» siguiente no hace nada visible ni nombra orden alguna (aislado: LINE, «Volver a leer la lista», «U» sin nombre, «U» → LINE).
  - La lectura de la lista es de `WorkspaceNavigationHost` / la lista del navegador (anterior a esta tanda).
  - No se tocó.
- **O-2:** en un editor de unos 950 px de ancho, «Más» pasa a una segunda línea de la barra. Es cosmético.
- **O-3:** SEL-C queda «Fuera del total» del BOM: sus vistas tienen diseños divergentes, preparados así en el dibujo del smoke. Es lo esperado.
- **O-4:** al teclear el nombre de un diseño, un clic triple en el campo solo seleccionó «A» de «SEL-A». El diseño se guardó como «SEL-SMOKE T4 borrar»: un error del operador, no del producto.

### Las correcciones del smoke, con sus números

- **Archivos:** `RackViewInsertPanelView.xaml` (F-1), `RackCadWindowViewModel.Variables.cs` y `VariablesPageViewModel.cs` (F-2), `VariablesPageView.xaml` (F-3).
- **Pruebas:**
  - `RackViewInsertPanelTests`: ningún texto del panel muestra el nombre de un tipo;
  - `RackCadVariablesPageTests`: la relectura tras escribir, rechazada como Busy (nueva), y la fila alineada con los encabezados.
- **Comprobación en rojo:** con cada corrección deshecha, su prueba falla (las cuatro piezas). Restauradas con la misma huella.
- **UI completa:** 2961 / 0 / 48 (2960 + 1). Core no cambia: solo cambió la UI.

## Quinta tanda, pp: el peralte de poste para todos los postes, en Selectivo y Dinámico (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t5-pp`, desde `041e1799`. Sin prototipo: es un arreglo que pidió el Owner el
2026-11-26, literal: «en el selectivo no hay forma de colocar un peralte de poste en todos los postes, tienes que ir uno por uno, no sé si
sea el mismo caso en el dinámico, hay que arreglar eso». Sin commit, sin push, sin AutoCAD.

**Lo que había:**
- **Selectivo:** el peralte del rack («Peralte del poste») vivía dentro de «Rack» › «Poste y anotaciones», que arranca plegado, y solo
  llegaba a los postes sin peralte propio. En «Avanzado» › «Postes y cabeceras» solo había «Peralte propio» poste por poste: quitar los
  propios era una casilla cada vez (o «Restaurar layout…» › «Peraltes propios», que además ofrece quitar el resto del layout).
- **Dinámico:** un solo «Peralte de poste» para todo el rack, en el mismo expansor plegado.

**Lo que se construyó:**
- **Selectivo, «Avanzado» › «Postes y cabeceras» › «Todos los postes»:** una fila sobre la tabla con un campo de peralte (lleno con el del
  rack) y «Aplicar a todos».
  - «Aplicar a todos» (o Enter en el campo) pone ese valor como el peralte del rack y quita todos los peraltes propios, **en el
    borrador**. Salir del campo no escribe nada.
  - El mensaje dice cuántos propios quitó y cuáles: «Peralte de 4 in en todos los postes del borrador: se quitaron 2 peraltes propios
    (P1: 5 in, P3: 6 in). «Descartar» vuelve a lo último que se leyó o se escribió.» (en un rack nuevo: «… a los valores por defecto»).
  - Con el mismo valor y ningún propio: «Todos los postes ya tienen 3 in y ninguno tiene peralte propio: el borrador no cambió.», y el
    rack sigue «Limpio».
  - Debajo, lo que hará: «Pone este peralte como el del rack y quita los 2 peraltes propios: P1 (5 in), P3 (6 in).».
  - Un valor que no es un número mayor que cero se rechaza en la fila y no escribe nada. Mientras AutoCAD escribe, la fila no se usa.
  - Es una escritura de **todo el rack**, sea cual sea el fondo que muestra la matriz: el peralte propio es del poste en todos los fondos.
- **Selectivo, «Rack» › «Lo esencial» › «Peralte de poste»:** un cuarto grupo con «Peralte del poste», visible sin abrir el expansor.
  - Su texto: «Para todos los postes. Un peralte propio, poste por poste, en «Avanzado».», o, si hay propios, «Para todos los postes salvo los
    de peralte propio: P1, P3. «Aplicar a todos», en «Avanzado», los quita.».
  - Escribe lo mismo que escribía el campo del expansor: el peralte del rack, sin tocar los propios. El campo **se movió**, no se duplicó
    (PP-S1).
- **Dinámico, «Rack» › «Lo esencial»:** «Peralte de poste» al final, visible sin abrir el expansor, con el texto «Para todos los postes,
  también los de las cabeceras personalizadas.». Escribe lo mismo que antes.
  - **No hay otro lugar donde el peralte vaya poste por poste o módulo por módulo** (PP-S6): el constructor pone el peralte del rack en
    todas las cabeceras, también las personalizadas (`DynamicRackSystemBuilder.ApplyPostPeralte`), y el editor clásico de una cabecera no
    puede dar a un módulo otro distinto (lo sella con el global, §7.12.6 de `RackDynamicSystemWindow`). Por eso Dinámico no lleva
    «Aplicar a todos».
- **Lo que escribe el dibujo sigue siendo «Actualizar»** (o «Insertar en el dibujo…» de un rack nuevo): ninguna orden nueva, ninguna
  petición a la puerta hasta ese gesto.

**Application:**
- `SelectiveDraftEditor.AllPosts.cs` (nuevo, parcial de `SelectiveDraftEditor`): `SetPeralteForAllPosts(peralte)` (el peralte del rack y
  ningún propio; el que llama recompone, como en toda escritura del borrador), `OwnPostPeraltes()` y el resultado
  `SelectivePeralteForAllPosts` con lo que quitó y su aviso (`NoticeFor(newRack)`). No cambia ningún cálculo: el dibujo ya leía el peralte
  de cada poste del sistema resuelto (`SelectivePostGeometry.PostPeralteAt`), propio o del rack.

**Las decisiones (supuestos para el Owner):**

| Id | Decisión | Por qué |
|---|---|---|
| PP-S1 | El peralte del rack **se mueve** de «Poste y anotaciones» a «Lo esencial» (Selectivo: un cuarto grupo «Peralte de poste» (en el commit `ad70515c` se llamaba «Poste»; corrección 5), tras los tres del draft aprobado; Dinámico: al final). No queda una segunda caja en el expansor. El resumen del expansor plegado sigue diciendo «Poste 3 in · cotas: …» | Dos cajas del mismo valor confunden; el expansor conserva el poste (el perfil), la escala y las cotas |
| PP-S2 | El campo de «Lo esencial» escribe como antes: solo el peralte del rack, los propios se quedan. Su texto dice qué postes tienen uno propio y que «Aplicar a todos» los quita | El encargo pone la acción que quita los propios en «Avanzado»; un campo que los borrara al salir de él sería una pérdida sin aviso |
| PP-S3 | «Todos los postes» es una fila encima de la tabla, no una fila de la rejilla; su campo empieza con el peralte del rack y lo sigue mientras el usuario no escriba otro. Salir del campo no escribe; Enter y el botón sí | La columna «Peralte propio» no es la del rack. Como la acción quita valores, la confirma un gesto explícito |
| PP-S4 | «Aplicar a todos» no toca las cabeceras personalizadas: siguen personalizadas. El dibujo toma el peralte de cada poste del sistema, como ya lo hacía el campo del rack | Es lo mismo que hacen el campo del rack y «Restaurar layout…» › «Peraltes propios» |
| PP-S5 | El mensaje termina como el de «Restaurar layout…»: lo que hace «Descartar» para ese rack | Es la otra acción que quita valores manuales en lote |
| PP-S6 | Dinámico no lleva «Aplicar a todos»: no tiene peraltes por poste ni por módulo que quitar | Ver arriba: el peralte de poste de Dinámico es uno para todo el rack |

**Las ediciones a archivos existentes** (marcadas `T5 pp`; ninguna en los archivos compartidos de la lista del encargo:
`RackCadWindowViewModel.cs`, `RackCadWindowView.xaml(.cs)`, `RackCadWindowViewModel.Drafts.cs`, `RackCadRackDoorPort.cs`,
`RackMenuCommands`, censos ni guardas):
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: el grupo «Peralte de poste» en `EssentialLayout`; `BuildEssentials` añade
  `RackPeralteEssential()`; `BuildAnnotationFields` ya no tiene el peralte; `RaiseState` llama a `RaiseAllPosts()`; tres comentarios.
- `src/RackCad.UI/VentanaRackCad/SelectiveEditorView.xaml`: la fila «Todos los postes» en «Avanzado», en un bloque, y la nota de la tabla
  («Peralte propio vacío = el del rack («Rack» › «Lo esencial» › «Peralte de poste», o «Todos los postes» arriba)»).
- `src/RackCad.UI/VentanaRackCad/DynamicEditorViewModel.cs`: `BuildEssentials` añade `PostPeralteEssential()`; `BuildPostFields` ya no
  tiene el peralte.

**Archivos nuevos:** `SelectiveDraftEditor.AllPosts.cs` (Application), `SelectiveEditorViewModel.AllPosts.cs`,
`SelectiveEditorView.AllPosts.cs` y `DynamicEditorViewModel.AllPosts.cs` (UI).

**Pruebas que cambian, con su nota:**
- `RackCadSelectiveFidelityTests.TheEssentialsAreTheThreeGroupsOfTheDraftAndThePostIsInPostAndAnnotations` pasa a
  `TheEssentialsAreTheGroupsOfTheDraftWithThePeralteOfThePostAndThePostIsInPostAndAnnotations`: cuatro grupos («Peralte de poste» con «Peralte del
  poste») y el expansor sin `peralte` (PP-S1).
- `RackCadSelectiveEditorTests.TheRackTabWritesTheEssentialsTheAnnotationsAndShowsTheCalculatedValues`: el peralte se escribe en
  `Essentials`, no en `AnnotationFields`.

**Las pruebas nuevas:**
- Core, `SelectivePeralteForAllPostsTests` (7): el peralte del rack y ningún propio, con el sistema recompuesto (cada poste lo tiene); el
  mismo peralte que quita los propios; nada que cambiar; los cuatro valores no válidos (0, negativo, NaN, infinito); todo el rack con la
  vista en el fondo 2; los textos del aviso.
- UI, `PostPeralteForAllPostsTests` (14): «Aplicar a todos» en el borrador (mensaje, tabla, resumen, «Lo esencial», «Modificado», nada a
  AutoCAD hasta «Actualizar», que entrega el diseño con el peralte, y «Descartar»); todo el rack con la vista en un fondo y el aviso de un
  rack nuevo; nada que cambiar y «Limpio»; los valores rechazados en la fila; la fila mientras AutoCAD escribe; la fila que sigue al
  peralte del rack; «Lo esencial» › «Peralte de poste» y sus textos; Dinámico en «Lo esencial» y en todas las cabeceras; y tres vistas con WPF real,
  sin errores de enlace: la fila de «Avanzado» (nombres únicos, salir del campo no escribe, el error, el botón, Enter, escribiendo) y
  «Lo esencial» de Selectivo y de Dinámico con «Poste y anotaciones» plegado.

**Comprobación en rojo:** cinco piezas rotas a propósito, en cuatro corridas, con copia de los originales fuera del worktree; restauradas
de esa copia y comprobadas con su SHA-256; `git diff` sin restos («RED», `if (false)`):
1. R1, `SetPeralteForAllPosts` sin quitar los propios: fallan 3 de Core y 4 de UI;
2. R2 (Selectivo: «Lo esencial» sin el peralte) junto con R5: fallan 7, entre ellas la de fidelidad y la del editor que cambian;
3. R5 sola (la fila sin refrescarse con el borrador): fallan la de la fila que sigue al rack y la de la vista de «Avanzado»;
4. R3 (Dinámico con el peralte otra vez en el expansor) junto con R4 (el botón «Aplicar a todos» sin su manejador): fallan las dos de
   Dinámico (R3) y la de la vista de «Avanzado» (R4), cada una por su pieza.

**Los números:**
- **UI completa:** 2975 correctas, 0 con error, 48 omitidas (3023): la base, 2961, más las 14 nuevas (39 min, con otras partes usando la
  máquina).
- **Core Full:** 19299 correctas, 0 con error, 403 omitidas (19702): la base, 19292, más las 7 nuevas (28 min).
- **Build Debug de la solución (Plugin incluido):** 0 errores.
- Las dos suites corrieron sobre las fuentes restauradas tras la comprobación en rojo.

**Lo que queda abierto:**
- **Smoke en AutoCAD:** un Selectivo con peraltes propios › «Avanzado» › «Aplicar a todos» › «Actualizar»: la frontal y la planta con el
  mismo peralte en todos los postes, y un solo «U» que lo deshace (es el «Actualizar» de siempre); la fila en la paleta a ~950 px (O-2).
  Dinámico: el campo en «Lo esencial» y «Actualizar».
- **La validación del Owner** de PP-S1 a PP-S6.
- Push Back tiene su propio «Peralte poste» en su editor clásico; queda para la parte `pb`, que lleva ese editor a la ventana.

### Correcciones de la revisión

Revisión independiente de `ad70515c` (el commit de la sesión con esta parte). Siete puntos; cada uno con su prueba, y el 1 y el 2 además con
comprobación en rojo.

| Punto | Qué pasaba | Qué se hizo |
|---|---|---|
| 1 [MEDIO-BAJO] | Con más peralte, cada poste intermedio de un «medio frente» ocupa más (el troquel del larguero se corre con el peralte del poste). F2 partido en 2 tramos con 0.6 in de resto: «Aplicar a todos» de 3 a 4 dejaba el resto en −0.4, y el dibujo pone el frente entero sin aviso, contra DG-10. El campo del rack en «Lo esencial» tenía el mismo riesgo | Las dos escrituras del peralte del rack (`SetPeralteForAllPosts` y la nueva `SetRackPostPeralte`, que ahora usa «Lo esencial») escriben, recomponen y revisan cada frente partido de cada fondo con `SelectiveSegmentFit.Of`. Si uno que cabía deja de caber, deshacen la escritura entera, recomponen de vuelta y la rechazan nombrando el frente y el motivo: «No se aplicó el peralte de 4 in: el medio frente de F2 dejaría de caber, porque cada poste intermedio ocupa más con más peralte. No cabe: … Ajusta antes sus tramos en «Matriz» (inspector del frente).». El motivo va a la fila o al campo. Un partido que ya no cabía antes no es motivo (PP-S8) |
| 2 [BAJO] | «Aplicar a todos» sin escribir nada leía el texto de la fila, redondeado a 3 decimales: un rack de 3.1496 in quedaba «Modificado» con 3.15 | Con la fila sin tocar, o con lo mismo que muestra, se aplica el peralte del rack tal cual (PP-S9) |
| 3 [BAJO] | Las placas base con peralte propio (`LeftBasePlate`/`RightBasePlate.PeralteOverride`) de una cabecera personalizada no siguen al poste, y nada lo decía. La pista de Dinámico prometía «también los de las cabeceras personalizadas» sin más | Selectivo: `CabecerasWithOwnPlatePeralte()` las cuenta, y el aviso, la pista de la fila y la de «Lo esencial» lo dicen («La cabecera personalizada de P2 tiene placas base con peralte propio: no cambian con el poste.»). Dinámico: la pista añade «1 cabecera tiene placas base con peralte propio: no cambian con el poste.» cuando hay alguna |
| 4 [BAJO] | «Descartar» y la respuesta de «Actualizar» dejaban en la fila lo escrito y su motivo | `ResetAllPosts()` en `Discard`, en `TakeBaseline` y en `MarkWritten` (la respuesta de «Actualizar», que no pasa por `TakeBaseline`) |
| 5 [BAJO] | El grupo «Poste» de «Lo esencial» tenía el mismo nombre de automatización que la lista «Poste» del expansor | El grupo se llama «Peralte de poste» |
| 6 [BAJO] | El `AutomationProperties.Name` de la fila estaba en un `Border`, que no tiene par de automatización: el grupo no llegaba al lector de pantalla | La fila es un `GroupBox` con cabecera «Todos los postes»; la prueba mira el árbol de automatización (control `Group`, su nombre, la caja y el botón dentro) |
| 7 | Faltaban pruebas | «Aplicar a todos» con una cabecera personalizada (PP-S4: sigue personalizada, conserva su altura y su placa), con un medio frente (punto 1), y Dinámico con la cabecera personalizada de un módulo (toma el peralte del rack, sigue personalizada, su placa conserva el suyo) |

**Supuestos nuevos para el Owner:**

| Id | Decisión | Por qué |
|---|---|---|
| PP-S8 | Solo se rechaza el peralte que **rompe** un medio frente que cabía. Uno que ya no cabía (un dibujo antiguo, que se dibuja entero) no bloquea el cambio | El peralte no es lo que lo rompe; bloquearlo dejaría el rack sin poder cambiar de peralte |
| PP-S9 | Escribir en la fila exactamente lo que muestra (3.15 para 3.1496) cuenta como «el peralte del rack» | La fila muestra el del rack con tres decimales; quien escribe lo mismo que ve no pide un cambio de 0.0004 in |
| PP-S10 | La regla del punto 1 vale para las dos escrituras de esta parte. El peralte propio de un poste en «Avanzado», el larguero, el ancho de tarima y demás cambios de las entregas anteriores siguen sin revisar los medios frentes; queda declarado, no corregido | Fuera del alcance de esta parte; es el mismo riesgo de DG-10 en las escrituras de antes, para planear |

**Ediciones a archivos existentes en las correcciones** (marcadas `T5 pp`):
- `SelectiveDraftEditor.AllPosts.cs`: `SetRackPostPeralte`, la revisión de los medios frentes, `CabecerasWithOwnPlatePeralte`,
  `OwnPlatePeralteText` y `PlatesKept` en el resultado.
- `SelectiveEditorViewModel.AllPosts.cs`: el valor exacto, las placas en las pistas, `ResetAllPosts` y «Lo esencial» por
  `SetRackPostPeralte`.
- `SelectiveEditorViewModel.cs`: el nombre del grupo, y una línea `ResetAllPosts()` en `Discard`, `TakeBaseline` y `MarkWritten`.
- `SelectiveEditorView.xaml`: la fila como `GroupBox`, dentro del mismo bloque `T5 pp`; la nota de la tabla nombra «Peralte de poste».
- `DynamicEditorViewModel.AllPosts.cs`: la pista con las placas.
- Pruebas: `SelectivePeralteForAllPostsTests` (+3), `PostPeralteForAllPostsTests` (+5 y dos ajustadas: el árbol de automatización y el
  nombre del grupo), y `RackCadSelectiveFidelityTests` con «Peralte de poste».

**Comprobación en rojo de 1 y 2**, con copia de los originales fuera del worktree, restaurados de esa copia con su SHA-256 y sin restos en
`git diff`:
- R6, sin la revisión de los medios frentes: fallan `APeralteThatAMedioFrenteNoLongerFitsIsRefusedWholeAndNamesTheFront` (Core) y
  `APeralteThatAMedioFrenteNoLongerFitsIsRefusedInTheRowAndInTheEssentials` (UI);
- R7, la fila leída siempre de su texto redondeado: falla `ApplyingTheRowAsItShowsThePeralteOfTheRackDoesNotRoundIt`.

**Los números tras las correcciones:**
- **Clases focales:** UI 446/446, Core 58/58.
- **UI completa:** 2980 correctas, 0 con error, 48 omitidas (3028): las 2975 de la parte más las 5 nuevas.
- **Core Full:** 19302 correctas, 0 con error, 403 omitidas (19705): las 19299 de la parte más las 3 nuevas.
- **Build Debug de la solución (Plugin incluido):** 0 errores.
- Las dos suites corrieron a la vez, sobre las fuentes restauradas tras la comprobación en rojo.

## Quinta tanda, lg: «Nuevo › Larguero», borrador en construcción (LG-01 a LG-03)

**Rama:** local `architecture/workspace-persistente-rackcad-t5-lg`, sobre `041e1799` (worktree `D:\w5lg`). Sin commit, sin push, sin AutoCAD.
Prototipo `largueros` (preguntas 1 a 13) y las notas del Owner, que mandan sobre él:

- **LG-01** (sobre la pregunta 1): la sección de largueros es para CREAR largueros a partir de secciones y ménsulas. Una sección puede ser la
  de un larguero de línea que ya existe, pero se llama por su tipo («Escalón», «Tubo», «Canal CPS…»), no «Larguero escalón…». Se elige la
  ménsula.
- **LG-02** (sobre la pregunta 2, «¿una página o dos?»): una sola página, con un filtro de secciones según el catálogo.
- **LG-03** (sobre la pregunta 3, el larguero en el dibujo): déjalo WIP.

**Lo que se construyó:**
- **«Nuevo» › «Componentes en esta ventana» › «Larguero en esta ventana · en construcción».** Abre la página «Nuevo › Larguero» en el centro
  de la ventana. La columna marca «Nuevo» mientras se ve; la miga «Nuevo» vuelve a su inicio. La página dice arriba lo que es: la ficha
  «En construcción» y la banda «Borrador en construcción» («…Todavía no lo guarda en la biblioteca ni lo dibuja: nada sale de aquí. Para
  guardar un larguero hoy, usa «Abrir el editor clásico».»).
- **La sección, en una sola página (LG-02),** de dos catálogos: los largueros de RackCad (`secciones.csv`, rol LARGUERO, 8) y las secciones
  AISC habilitadas (1011). Filtros «Catálogo» (Todos / Largueros de RackCad / Perfiles AISC, con su cuenta), «Tipo» (los del catálogo
  elegido, con su cuenta) y «Buscar» (nombre, nombre en el catálogo o id, sin mayúsculas, acentos ni separadores). La lista va agrupada por
  tipo, con una cabecera por tipo que no se puede elegir, y en orden natural (W10X12 antes que W10X100; L…1-1/8 antes que …1-1/4). Cada
  fila dice su catálogo y su detalle: los peraltes, el calibre y la ménsula de su fila (RackCad), o el peso y el peralte (AISC).
- **Cada sección se llama por su tipo (LG-01):** «Escalón calibre 14», «Escalón con ángulo», «Escalón de ajuste infinito», «Escalón invertido
  calibre 14», «Escalón invertido con ángulo», «Escalón troquel redondo», «Tubo 1.9"» e «In n out de canal de 6"» (tipo «Canal»); una
  sección AISC, por su designación («C10X15.3»). Los tipos unen los dos catálogos: «Escalón», «Tubo», «Canal», «Ángulo», «Viga W» y
  «Viga S (IPS)». La fila conserva al lado el nombre del catálogo y su id.
- **El peralte, la longitud a corte y la ménsula.** Un larguero de RackCad elige entre los peraltes de su fila; una sección AISC tiene el
  suyo, del catálogo, fijo. La longitud a corte se escribe en pulgadas; un valor que no es un número mayor que 0 se marca en su campo y el
  resumen lo pide. La ménsula se elige de `mensulas.csv` (LG-01); sigue a la sección (la de su fila, o ninguna con una sección AISC) hasta
  que el usuario elige una.
- **El resumen:** el nombre que tomaría (LG-05: «Larguero 96 in · P3»), la sección y su tipo, el catálogo con el nombre y el id de la fila,
  el peralte, la longitud, la ménsula (alto y calibre, y si es la de su fila), el calibre (RackCad) o el peso del perfil (AISC, como
  RACKSECCION: «15.3 lb/ft (22.8 kg/m) · 122.4 lb (55.5 kg) a 96 in»), la lista de materiales con la receta única del larguero
  (`LargueroBomBuilder.Component`: el larguero, su perfil ×1 y sus ménsulas ×2) y lo que falta («Falta: Elige la ménsula: una sección AISC
  no trae ménsula en el catálogo.»).
- **«Volver a los valores por defecto»:** los del editor clásico (el primer larguero, peralte 3, 96 in y su ménsula).
- **«Abrir el editor clásico»:** la tarjeta clásica «Larguero» (RACKNUEVO Larguero), un gesto del usuario por la misma puerta de las
  tarjetas. Es el que guarda un larguero hoy.
- **«Lo que falta para terminar esta página»,** plegado al final de la página: la lista de abajo, para quien la pruebe.
- **Un catálogo que falla se dice arriba, no se simula** (dolor 7): el AISC que no se puede leer (falla cerrado, como RACKSECCION y
  Cantilever) deja solo los largueros de RackCad; un catálogo de RackCad sin largueros o sin ménsulas también se dice.

**Qué no se construyó (LG-03) y el camino de escritura:**
- **Nada se escribe en el dibujo ni en la biblioteca,** y ninguna orden sale de la página salvo «Abrir el editor clásico». No hay orden
  nueva ni puerta numerada: no hacían falta.
- **El único camino de escritura existente del prototipo** es «Insertar en el dibujo…» del perfil estructural, que hoy es RACKSECCION (un
  bloque suelto, con su ventana modal). No se trajo a esta página: con LG-02 («una sola página») la página del perfil estructural del
  prototipo no se construyó, y la tarjeta «Perfil estructural» de «Nuevo» sigue abriendo RACKSECCION como antes. Declarado: el clásico de
  RACKSECCION no se tocó.
- **La tarjeta clásica «Larguero»** sigue en «Nuevo» y sigue abriendo `RackLargueroWindow` (RACKNUEVO Larguero): la página no guarda, así
  que no la sustituye. El código clásico no se tocó.

**Ediciones a archivos compartidos** (marcadas `T5 lg`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`, tres líneas:
  1. el valor `Larguero` al final de `RackCadWindowPage`;
  2. en `ShowPage`, `RaiseLargueroPage();` tras `OnUtilityPageShown(page)`;
  3. en `RebuildSections`, la sección activa es `page == _page || IsUnderNuevo(page)` («Nuevo» marcado con la página del larguero).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`, **dos** bloques `<!-- T5 lg -->` (supuesto LG-S11):
  1. en «Nuevo», tras la banda «Paso intermedio…» y antes de `RackCadNuevoExtras`: el título «Componentes en esta ventana» y el botón
     `LargueroHereButton`;
  2. al final de la rejilla del centro, tras `AyudaPage`: `LargueroPageHost`, con la página `LargueroPage` (`DataContext="{Binding LargueroWorkbench}"`).
- **No se tocaron** `RackCadWindowView.xaml.cs`, `RackCadWindowViewModel.Drafts.cs`, `RackCadRackDoorPort.cs`, `RackMenuCommands`, el
  Plugin, los editores, los catálogos ni el código clásico.

**Archivos nuevos:**
- Application: `src/RackCad.Application/Systems/Larguero/LargueroSections.cs` (`LargueroSectionSource`, `LargueroSectionOption`,
  `LargueroSectionNaming`, `LargueroNaturalOrder`, `LargueroSectionType`, `LargueroSectionCatalog`) y `LargueroDraftSummary.cs`
  (`LargueroDraftSummary`, `LargueroSummaryLine`, `LargueroBomPreviewLine`). Puros.
- UI: `LargueroWorkbenchViewModel.cs` (con `LargueroChoiceOption` y `LargueroSectionRow`), `LargueroWorkbenchView.xaml(.cs)`,
  `RackCadWindowViewModel.Larguero.cs` y `RackCadWindowView.Larguero.cs`, en `src/RackCad.UI/VentanaRackCad/`.
- Pruebas: `tests/RackCad.Tests/LargueroSectionsTests.cs` y `tests/RackCad.UI.Tests/Workspace/RackCadLargueroWorkbenchTests.cs`.

**Supuestos para el Owner** (prefijo LG-S, cada uno en un solo sitio):

| Id | Supuesto | Dónde se cambia |
|---|---|---|
| LG-S01 | La página es de la ventana (`RackCadWindowPage.Larguero`), bajo «Nuevo», y **no un «Borrador N»** del dibujo: un larguero es un componente de la biblioteca, no un rack, no se inserta y no tiene puerta que lo escriba. Por eso no cuenta en la cabecera ni en la lista de borradores, no veta el cierre de un dibujo (D-08) y no depende del dibujo (abre sin dibujo y sigue igual al cambiar de dibujo). Hay una página por ventana; lo elegido se conserva mientras vive la ventana | `RackCadWindowViewModel.Larguero.cs` |
| LG-S02 | Los tipos unen los dos catálogos: «Escalón», «Tubo», «Canal», «Ángulo», «Viga W» y «Viga S (IPS)». De AISC: HSS → «Tubo», C → «Canal», L → «Ángulo», W → «Viga W», S → «Viga S (IPS)». El «Canal CPS…» de la nota cae en «Canal» cuando el catálogo tenga su fila | `LargueroSectionNaming.TypeOf` |
| LG-S03 | El tipo de un larguero de RackCad sale de su columna `family` cuando no dice «Larguero»; si no, de las palabras de su nombre y su id («escalón», «tubo», «canal»). Su nombre es el del catálogo sin «Larguero» delante ni «con ménsula …» detrás, con la palabra del tipo escrita como el tipo («Larguero escalon troquel redondo» → «Escalón troquel redondo»). Dos largueros con el mismo nombre por tipo llevan su id. No se añadió una columna `tipo` a `secciones.csv`: hoy el Owner puede fijar el tipo en Excel con la columna `family` | `LargueroSectionNaming.TypeOf`, `NameOf`; `LargueroSectionCatalog.Build` |
| LG-S04 | «Tipo» es una lista desplegable con su cuenta, no los botones de familia del prototipo (pregunta 12), porque los tipos cambian con el catálogo. La página arranca en «Largueros de RackCad». Un tipo que el catálogo nuevo no tiene vuelve a «Todos los tipos» | `LargueroWorkbenchViewModel.RefreshTypes`, `ResetToDefaults` |
| LG-S05 | Una sección elegida que el filtro oculta **sigue elegida**, y la página lo dice («La sección elegida, «Tubo 1.9"», no coincide con el filtro: sigue elegida.»). El inspector clásico elegía la primera visible sin decirlo | `LargueroWorkbenchViewModel.OutsideFilterText` |
| LG-S06 | El peralte de una sección AISC es el de su catálogo, fijo: d de W, C y S; el alto de un HSS; el ala larga de un L. Al cambiar de sección, el peralte elegido se queda si la nueva lo admite; si no, el primero de la nueva | `LargueroSectionCatalog.DepthOf`, `ChooseSection` |
| LG-S07 | La ménsula sigue a la sección (la de su fila; ninguna con una sección AISC) hasta que el usuario elige una; desde entonces se conserva al cambiar de sección. «Volver a los valores por defecto» la vuelve a seguir | `LargueroWorkbenchViewModel.ChooseSection` |
| LG-S08 | El nombre previsto de LG-05 con una sección AISC lleva su designación en lugar de «P<peralte>»: «Larguero 96 in · C10X15.3». En la lista de materiales el larguero lleva los nombres de esta página («Escalón calibre 14 · P3»), no los de `DescribeBeam` (corregido en la revisión, punto 6). Sin ménsula elegida, la lista no pone la de la fila | `LargueroDraftSummary` |
| LG-S09 | La lista es una `ListBox` virtualizada de 320 px de alto, con su propio desplazamiento dentro de la página (el doble desplazamiento del dolor 19 sigue; aceptado en un borrador) | `LargueroWorkbenchView.xaml` |
| LG-S10 | Los catálogos se leen la primera vez que se abre la página, no al abrir la ventana. El AISC con el mismo proveedor que RACKSECCION y Cantilever (`CsvStructuralSectionCatalogProvider` sobre `CatalogDirectory`) | `RackCadWindowViewModel.Larguero.cs` |
| LG-S11 | Dos bloques XAML en `RackCadWindowView.xaml` y no uno: la tarjeta va dentro de «Nuevo» y la página es hermana de las demás páginas en la rejilla del centro | `RackCadWindowView.xaml` |

**Lo que falta para terminarlo** (para el Owner; la página lo lista también, plegado):
1. **«Guardar en la biblioteca…» (pregunta 6).** `LargueroDesign` guarda solo `BeamProfileId` (un larguero de RackCad), `Peralte`, `Length` y
   `MensulaOverride`. Una sección AISC necesita guardar también su catálogo y su id: es un cambio del formato del diseño (`.rackcad.json`,
   I-11) y de `RackDesignValidation`. Decisión del Owner: ¿un campo nuevo (catálogo + id de sección) o un diseño de larguero nuevo?
2. **Dibujarlo (LG-03, pregunta 3).** No hay un bloque del larguero completo (el perfil con sus 2 ménsulas). ¿Se inserta como pieza suelta,
   como el perfil estructural, con una puerta numerada propia? Para una sección AISC haría falta la geometría de RACKSECCION y, con la
   familia S, su advertencia de ADR-0023.
3. **La vista (pregunta 8):** el alzado esquemático con el peralte y la ménsula a escala. Sigue [PENDIENTE] cómo se apoya la ménsula en el
   poste.
4. **«Exportar el BOM»** a Excel y CSV en «Más» (pregunta 7).
5. **El nombre del larguero** (un campo con el automático de la pregunta 5 como sugerencia) y **abrir aquí un larguero de la biblioteca**
   (hoy, «Abrir» de la biblioteca lo lleva al editor clásico por RACKBIBLIOTECAABRIR).
6. **El perfil estructural (RACKSECCION) y la «sola página»:** decidir si el inspector se funde en esta página (filtro «Perfiles AISC» +
   «Insertar en el dibujo…») o sigue como herramienta aparte. Hoy sigue clásico, con su tarjeta.
7. **Que Selectivo y Dinámico elijan un larguero hecho aquí** (pregunta 4): otra iniciativa.
8. **Los nombres y tipos de los largueros de RackCad** salen de su nombre (LG-S03); una columna `tipo` (o `family` con el tipo) en
   `secciones.csv` los fijaría.
9. **El smoke en AutoCAD:** la página en la paleta real (anchos, foco, desplazamiento de la lista con mil secciones).
10. **Quitar la tarjeta clásica «Larguero»** cuando la página guarde (el clásico desaparece al final de I-64).

**Las pruebas:**
- `tests/RackCad.Tests/LargueroSectionsTests.cs` (Core, nueva, 17 casos), sobre los catálogos reales y catálogos pequeños hechos en la
  prueba:
  - los 8 largueros llamados por su tipo, sin «Larguero» ni «ménsula» en el nombre, con el nombre del catálogo al lado;
  - la columna `family` manda cuando dice otra cosa que «Larguero» (con «Canal CPS 4"»), y un tipo del catálogo va antes de «Otro»;
  - dos largueros con la misma sección llevan su id;
  - las secciones AISC por familia, con su peralte y su peso;
  - el orden por tipo, catálogo y orden natural (seis pares), y los tipos que unen los dos catálogos;
  - el filtro por catálogo, tipo y texto, sin acentos ni separadores;
  - un catálogo que no se lee;
  - el resumen: la receta de `LargueroBomBuilder.Component`, el nombre previsto, una sección AISC sin y con ménsula, y lo que falta.
- `tests/RackCad.UI.Tests/Workspace/RackCadLargueroWorkbenchTests.cs` (UI, nueva, 18 casos):
  - la tarjeta abre la página bajo «Nuevo», con los valores del clásico; la miga vuelve; la página conserva lo elegido;
  - no es un borrador: ni la lista de borradores, ni la cabecera, ni la puerta; solo «Abrir el editor clásico» corre una acción
    (RACKNUEVO Larguero);
  - abre sin dibujo y sigue igual al cambiar de dibujo;
  - el filtro por catálogo y tipo, la búsqueda y «Ninguna sección coincide…»;
  - el filtro que oculta la sección elegida; un nulo o una cabecera no son elecciones;
  - una sección AISC (peralte fijo, ménsula que falta) y la ménsula que sigue a la sección hasta que el usuario elige;
  - la longitud no válida (cuatro casos);
  - «Volver a los valores por defecto»;
  - los nombres de automatización únicos y con posición;
  - el catálogo AISC que falla, el de RackCad sin largueros ni ménsulas, y un fallo contenido en el mensaje de la ventana;
  - con WPF real (STA) y sin errores de enlace: la tarjeta, la página, los filtros, la lista (cabeceras apagadas, nombres), el resumen, el
    campo marcado, los botones y la miga; y en estrecho, el resumen debajo y el aviso del catálogo.
- **Censos y guardas:** ninguno cambió. La página es un `UserControl` (fuera del censo de ventanas de ADR-0029); los archivos nuevos de
  `VentanaRackCad` no nombran AutoCAD, el Plugin, `LockDocument` ni `SendStringToExecute` (guarda de capas de `RackCadWindowListTests`), y
  el código de Application no usa `System.IO.Path`.
- **Una mirada a la página:** con una prueba temporal (ya borrada) se dibujó la ventana con WPF real (`RenderTargetBitmap`) en «Nuevo», en
  la página a 1200 px, con «Perfiles AISC › Canal» y en 640 px (`hs65\t5\lg\render-*.png`, scratchpad de la sesión). Corrigió dos cosas
  antes de las pruebas finales: en estrecho, el título y los botones de la cabecera de la página se pisaban (ahora van en una sola fila que
  se parte), y la lista de materiales decía «Larguero: Larguero 96 in · P3» (ahora «Larguero: Escalón calibre 14 · P3», con los nombres de esta página).
- **Comprobación en rojo:** diecinueve piezas rotas a propósito, una a una, con script (`hs65\t5\lg\red.py` en el scratchpad de la sesión;
  salida en `red-out.txt`, `red-summary.txt` y `red-logs\`). Antes de cada rotura el script copia el archivo FUERA del worktree
  (`hs65\t5\lg\red-backup\`); después lo restaura con sus bytes, con fecha nueva, y comprueba su SHA-256: las 20 restauraciones, iguales.
  Tras cada pieza se compiló y corrió su clase focal (Core: `LargueroSectionsTests`; UI: `RackCadLargueroWorkbenchTests`):
  1. R1, el nombre conserva «Larguero» delante: fallan 4;
  2. R2, el nombre conserva «con ménsula …» detrás: fallan 4;
  3. R3, el tipo no mira la columna `family`: falla 1;
  4. R4, el filtro no mira el catálogo: falla 1;
  5. R5, el filtro no mira el tipo: fallan 2;
  6. R6, orden ordinal en lugar de natural: falla 1;
  7. R7, dos largueros con la misma sección se leen igual: falla 1;
  8. R8, una sección AISC sin ménsula cuenta como completa: fallan 2;
  9. R9, la lista de materiales pone la ménsula de la fila sin ménsula elegida: **sobrevivió** en la primera vuelta (ninguna prueba miraba
     la lista de un larguero de RackCad sin ménsula del catálogo); se añadió el aserto a `TheSummarySaysWhatIsMissing` y en la segunda vuelta
     falla 1;
  10. R10, la ménsula no sigue a la sección: fallan 4;
  11. R11, un filtro que oculta la sección elegida la cambia por la primera visible: falla 1;
  12. R12, un tipo que el catálogo nuevo no tiene se queda en el filtro: falla 1;
  13. R13, la columna no marca «Nuevo» con la página del larguero: falla 1;
  14. R14, `ShowPage` no avisa de la página del larguero: falla 1 (la de la vista: la página no se muestra);
  15. R15, una longitud de 0 vale: falla 1 (el caso «0» de la teoría);
  16. R16, el catálogo AISC que falla no se dice: fallan 2;
  17. R17, las cabeceras de tipo de la lista se pueden elegir (vista): falla 1;
  18. R18, la tarjeta de «Nuevo» no abre la página (vista): falla 1;
  19. R19 (segunda vuelta, tras precalcular las claves de la búsqueda), la búsqueda no mira el nombre en el catálogo ni el id: falla 1.

  Después se buscaron restos en `git diff` y en los archivos nuevos (`// R…`, `if (false`, `Where(o => true)`): ninguno. Se recompiló la
  solución con `--no-incremental` antes de las suites completas.
- **Los números,** sobre las fuentes finales, con la solución recompilada con `--no-incremental` en Debug:
  - **UI completa:** 2979 correctas, 0 con error, 48 omitidas (3027): la base 2961 / 0 / 48 más las 18 nuevas.
  - **Core Full:** 19309 correctas, 0 con error, 403 omitidas (19712): la base 19292 / 0 / 403 más las 17 nuevas.
  - **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias son las MSB3277 de las referencias de AutoCAD y las de los
    analizadores de xunit, que ya estaban.
  - Las dos suites corrieron a la vez (33 min la UI, 32 min Core).

### Quinta tanda, lg: correcciones de la revisión (sobre `855a935c`)

Worktree `D:\w5lg`, sobre el commit `855a935c` de la sesión. Sin commit, sin push, sin AutoCAD. Los ocho puntos de la revisión independiente,
cada uno con su prueba; los puntos 1 a 3, además, con su comprobación en rojo.

| Punto | Qué se hizo | Prueba |
|---|---|---|
| 1 [MEDIO] Catálogos leídos una sola vez | Cada vez que se abre la página se vuelven a llamar los dos cargadores (`LargueroWorkbenchViewModel.Reload`, desde `OpenLargueroWorkbench`). Los dos proveedores tienen caché por la firma de sus archivos: sin cambios dan la misma instancia y la página no se reconstruye. Si cambió una instancia (un CSV editado en Excel) o lo que la página dice de los catálogos, se reconstruye y conserva la sección por (catálogo, id), los filtros, la búsqueda, la longitud, el peralte (si la sección lo admite) y la ménsula (si sigue en el catálogo). Una sección que salió del catálogo se dice («La sección elegida, «Tubo 1.9"», ya no está en el catálogo: elige otra.») y no se elige otra en su lugar. Una relectura que falla conserva la última lectura buena y lo dice («…no se pudo volver a leer (…): la página sigue con la última lectura.»). Si antes no había ninguna sección que elegir, la página empieza con la de por defecto. **De paso:** el catálogo de RackCad se lee con el proveedor (`JsonRackCatalogProvider.FromBaseDirectory().Load()`, seam `LargueroRackCatalogLoader`), que lanza, y no con `UiSupport.LoadCatalogSafe`, que daba un catálogo vacío en silencio. Así «El catálogo de RackCad no se pudo leer (…).» sale de verdad, en lugar de su consecuencia («no trae largueros») | `RackCadLargueroWorkbenchTests`: `EveryOpeningReadsTheCatalogsAgainAndKeepsWhatWasChosen`, `ASectionThatLeftTheCatalogIsSaidAndAReadingThatFailsKeepsTheLastOne`, `ACatalogOfRackCadThatCannotBeReadIsSaid` |
| 2 [MEDIO-BAJO] «Sin terminar» y «completo» a la vez | El nombre de un larguero con sección AISC es su designación («Larguero 96 in · C10X15.3») y no necesita peralte: sale aunque el catálogo no dé la profundidad. Un larguero de RackCad cuya fila no declara peraltes no se puede completar, y `Missing` lo dice: «El catálogo no da los peraltes de este larguero (columna peraltes de secciones.csv).». Resultado: el nombre queda vacío exactamente cuando algo de lo que necesita está en `Missing` | Core: `TheNameAndWhatIsMissingNeverContradictEachOther` (los dos casos y un barrido de todas las secciones reales, con y sin longitud, peralte y ménsula); UI: `ALargueroWhoseRowDeclaresNoPeralteSaysWhyItIsNotComplete` |
| 3 [BAJO] Peralte con más de 3 decimales | `LargueroSectionNaming.Number` da hasta tres decimales cuando ese es el valor exacto y, si no, todos los dígitos que hacen falta para volver a leerlo igual. Las opciones del peralte, el resumen y el nombre leen el mismo double del catálogo | Core: `APeralteWithMoreThanThreeDecimalsCompletesTheLarguero`; UI: `APeralteWithMoreThanThreeDecimalsIsOfferedAsItIsAndCompletesTheLarguero` |
| 4 Búsqueda y tipos | Las claves de búsqueda incluyen el tipo y sus alias, así que «tubo» encuentra los PTR, como dice la pista. La pista dice ahora «por tipo» y pone de ejemplo «PTR», «CPS» e «IPR». Tipos con los nombres del mercado mexicano: C de AISC → «Canal CPS», HSS → «Tubo rectangular (PTR)», W → «Viga W (IPR)», S → «Viga S (IPS)». El canal in-n-out de RackCad (formado en frío) es «Canal», otro tipo distinto del CPS (laminado en caliente). El tubo de 1.9" de RackCad es «Tubo redondo», otro tipo distinto del PTR. Supuestos LG-S02 y LG-S03, al día abajo | Core: `TheSearchSeesTheTypeAndTheNamesOfTheMarket`, `TheWordsOfALargueroOfRackCadSayItsTypeWhenTheCatalogDoesNot` y las de tipos, al día; UI: la búsqueda «tubo» y «CPS» en `TheSectionIsChosenWithTheFilterByCatalogAndType` |
| 5 [BAJO] `Fold` fuera del BMP | Una mitad de par sustituto va tal cual. **De paso:** el orden natural solo toma como número los dígitos ASCII. Un dígito de otra escritura (٣) hacía que `double.Parse` lanzara | Core: `ACharacterOutsideTheBmpOrADigitOfAnotherScriptDoesNotThrow` |
| 6 Texto del BOM clásico | La nota de la página dice «La receta del BOM de Selectivo (un perfil y 2 ménsulas por larguero), con los nombres de esta página.». El comentario del código y LG-S08 ya no dicen que se describe «como el BOM clásico»: `DescribeBeam` da el nombre de la fila («Larguero escalón calibre 14 con ménsula 3 remaches · P3»), y la página da el suyo («Escalón calibre 14 · P3») | Core: aserto en `TheSummaryOfALargueroOfRackCadUsesTheRecipeOfTheLargueroAndTakesTheAutomaticName` (no es `DescribeBeam`); UI: el texto de la nota en la prueba de vista |
| 7 Valor por defecto | `LargueroSectionCatalog.Default` es el primer larguero en el orden del catálogo (`BeamProfiles[0]` con id), no el primero de la lista por tipo. Supuesto LG-S13 | Core: `TheDefaultIsTheFirstLargueroInTheOrderOfTheCatalog` |
| 8 Texto de la tarjeta | «…Todavía no lo guarda ni lo dibuja; para guardarlo, usa la tarjeta «Larguero».» | UI: el texto de la tarjeta en la prueba de vista |

**Supuestos al día** (sustituyen a los de arriba con el mismo número):

| Id | Supuesto | Dónde se cambia |
|---|---|---|
| LG-S02 | Tipos, en este orden: «Escalón», «Tubo redondo», «Tubo rectangular (PTR)», «Canal», «Canal CPS», «Ángulo», «Viga W (IPR)», «Viga S (IPS)» y «Otro». En AISC: HSS → PTR, C → CPS, W → IPR, S → IPS, L → «Ángulo». **«Viga W (IPR)»** sigue por analogía a «Viga S (IPS)»: la revisión nombraba IPR solo como alias de búsqueda. «Canal» es el canal formado en frío de RackCad (in-n-out); «Canal CPS», el laminado en caliente | `LargueroSectionNaming` |
| LG-S03 | Las palabras de un larguero de RackCad sin `family` propia: «escalón» → «Escalón»; «PTR», o «tubo» con «rectangular» o «cuadrado» → «Tubo rectangular (PTR)»; «tubo» → «Tubo redondo»; «CPS» → «Canal CPS»; «canal» → «Canal». La columna `family` manda cuando no dice «Larguero» | `LargueroSectionNaming.TypeOf` |
| LG-S08 | Con una sección AISC, el nombre previsto lleva su designación y no necesita el peralte. La lista de materiales usa los nombres de esta página, no los de `DescribeBeam`. Sin ménsula elegida, no pone la de la fila | `LargueroDraftSummary` |
| LG-S10 | Cada apertura de la página relee los dos catálogos. El de RackCad, con su proveedor, que dice el fallo, y no con `LoadCatalogSafe` | `RackCadWindowViewModel.Larguero.cs`, `LargueroWorkbenchViewModel.Reload` |
| LG-S12 (nuevo) | Al releer, la sección se conserva por (catálogo, id). Una que salió del catálogo se dice y no se sustituye. Una relectura que falla sigue con la última lectura buena y lo dice. Sin ninguna sección antes, la de por defecto | `LargueroWorkbenchViewModel.Reload` y `Read` |
| LG-S13 (nuevo) | El valor por defecto es `BeamProfiles[0]`, como pidió la revisión. **Diferencia con el clásico:** el combo de `RackLargueroWindow` ordena por rótulo (`UiSupport.ToOptions`, `CurrentCultureIgnoreCase`) y toma el primero de ese orden. Con el catálogo de hoy los dos dan `LARGUERO_ESCALON_CAL14_3_REMACHES`. Si el Owner quiere la regla exacta del clásico, es una línea | `LargueroSectionCatalog.Build` |

**Ediciones a archivos compartidos en esta vuelta:** `RackCadWindowView.xaml`, una línea dentro del bloque `<!-- T5 lg -->` de «Nuevo» (el
texto de la tarjeta, punto 8). `RackCadWindowViewModel.cs` no cambió. Lo demás va en los archivos de la parte: `LargueroSections.cs`,
`LargueroDraftSummary.cs`, `LargueroWorkbenchViewModel.cs`, `LargueroWorkbenchView.xaml`, `RackCadWindowViewModel.Larguero.cs` y las dos
clases de prueba.

**Comprobación en rojo de los puntos 1 a 3:** nueve piezas rotas a propósito, una a una, con el mismo script (`hs65\t5\lg\red.py`, piezas
R20 a R28; salida en `red-rev-out.txt` y `red-logs\`). Antes de cada rotura, una copia del archivo FUERA del worktree; las nueve
restauraciones, con el mismo SHA-256. Cada pieza hace fallar su prueba:
1. R20, volver a abrir la página no relee los catálogos: fallan 3 (las tres del punto 1);
2. R21, la relectura no conserva la sección elegida (toma la de por defecto): fallan 2;
3. R22, la relectura reconstruye aunque nada cambió: falla 1;
4. R23, el catálogo de RackCad se lee otra vez con `LoadCatalogSafe`: falla 1;
5. R24, el fallo de lectura del catálogo de RackCad no se dice: falla 1;
6. R25, una relectura que falla pierde la última lectura buena: falla 1;
7. R26, el nombre vuelve a exigir el peralte a una sección AISC: falla 1 (Core);
8. R27, un larguero sin peraltes en el catálogo no dice por qué falta: falla 1 (Core);
9. R28, los números vuelven a redondearse a tres decimales: falla 1 (Core).

Después: ningún resto en `git diff` ni en los archivos de la parte, y la solución recompilada con `--no-incremental`.

**Las pruebas, al día:** `LargueroSectionsTests` (Core) pasa de 17 a 23 casos y `RackCadLargueroWorkbenchTests` (UI), de 18 a 23. Las
existentes cambian solo por los tipos nuevos (punto 4) y por el texto del BOM (punto 6), cada cambio con su nota en el código.

**Los números,** con la solución recompilada con `--no-incremental` en Debug:
- **UI completa:** 2984 correctas, 0 con error, 48 omitidas (3032): las 2979 de `855a935c` más las 5 nuevas.
- **Core, las clases focales** (`LargueroSectionsTests`, `BeamCatalogTests`, `RackCadWindowListTests`, `NamespaceFolderGuardTests`,
  `WorkspacePaletteHostSourceGuardTests`, `RackCadWindowCloseAdvisorTests`, `ProjectVariablesConformanceTests`, `CantileverSourceGuardTests`
  y `MainMenuStructuralSectionAccessGuardTests`): 265 de 265. Core Full no se volvió a correr (el encargo pide las focales); por cuenta,
  19315 / 0 / 403 (las 19309 de `855a935c` más las 6 nuevas).
- **Build Debug de la solución (Plugin incluido):** 0 errores; 83 advertencias, las mismas de antes (MSB3277 de AutoCAD y analizadores de
  xunit).

## Quinta tanda, al: «Almacén» (layout de almacén) en la ventana

**Rama:** local `architecture/workspace-persistente-rackcad-t5-al`, sobre `041e1799` (worktree `D:\w5al`). Sin commit, sin push, sin AutoCAD.
Prototipo `protos\almacen` (preguntas 1 a 11); el Owner aceptó las once sin nota. No se borró código clásico: RACKLAYOUT, RLY,
RACKRELLENAR y RR siguen abriendo sus diálogos (pregunta 10) y su cuerpo no cambió.

**Lo que se construyó:**
- **La sección «Almacén»** de la columna, entre «BOM y lista de racks» y «Biblioteca de diseños» (pregunta 1), con la página
  `WarehousePageView` / `WarehousePageViewModel`, sin modal y sin bloquear AutoCAD:
  - **la cabecera:** «Almacén», el estado de la lectura («Sin leer», «Leyendo el dibujo», «Lectura al día», «Puede no estar al día»,
    «No se pudo leer», «Escribiendo»), «Colocar N racks» (primario, con su motivo cuando está apagado), «Volver a leer» y «Restablecer
    valores»;
  - **los avisos en línea:** «Escribiendo en el dibujo» con lo que coloca y «Dejar de esperar» hasta que la orden toma la petición;
    «La rejilla no cabe en el edificio» con «Colocar de todos modos» y «Seguir ajustando»; «El dibujo cambió desde la lectura» con
    «Volver a leer ahora»; y el mensaje de la página con «Cerrar aviso»;
  - **las dos pestañas** «Rejilla» (hoy RACKLAYOUT) y «Rellenar un área» (hoy RACKRELLENAR), cada una marcada «(con problemas)» si
    algo suyo no vale, con su nota;
  - **«Rack semilla»:** la lista de los racks del dibujo con planta (según la última lectura de la lista), con «· con cambios sin
    escribir» si tienen un borrador en la ventana; los que no tienen planta se nombran («Sin planta, no se ofrecen: …»); la huella
    leída y cuántas plantas tiene el rack; los avisos de borrador sin escribir (pregunta 4), de varias plantas (preguntas 3 y 7) y de la
    planta dentro del área (dolor 13);
  - **«Rejilla»:** filas y columnas, con su error en el campo y el de la rejilla (más de un rack, como mucho 1000);
  - **«Sitio»:** la capa, de una lista con las capas del dibujo (pregunta 8); el contorno leído o el motivo y el arreglo (dolor 10);
    el texto de las 16 cuerdas (dolor 11); holgura a muros y a columnas;
  - **«Pasillos»** para las dos pestañas (pregunta 11), «Filas espalda con espalda» con el hueco debajo, que solo se edita con ella
    (dolor 7), y «La fila pareja no se espejea…» (dolor 6); en «Rellenar un área», «Probar también la orientación girada 90°», apagada
    con espalda con espalda y con su motivo;
  - **«Copias»:** «Enlazadas / Independientes» en «Rejilla» con lo que es cada una y el nombre de las independientes (dolor 14);
    «Siempre enlazadas» en «Rellenar un área»;
  - **«Verificar encaje (opcional)»**, plegable, con los dos lados del edificio en el vocabulario de un solo par de ejes (dolor 9) y lo
    que mira (solo bordes);
  - **«Resultado»:** cuatro cifras por pestaña (racks nuevos, la rejilla, lo que ocupa y el encaje; o caben, orientación, rejilla
    probada y descartadas) y los problemas (se salen del edificio, la fila sola, nada cabe, las columnas, los huecos de la numeración,
    más de 5000 racks);
  - **«Esquema»** (pregunta 2): una casilla por rack, no a escala, con las etiquetas, la semilla, lo nuevo, lo que se sale, lo que
    queda fuera del contorno y lo que cae sobre una columna, su leyenda, los pasillos como huecos y su nombre de UI Automation; hasta 30
    filas × 24 columnas, y si no, lo dice.
- **El cálculo es el de hoy** (pregunta 9): `WarehouseLayoutRules` (Application, puro) llama a `WarehouseGridPlanner.Plan` desde el
  origen de la semilla y a `WarehouseFitChecker` sobre un edificio con su esquina en la de la semilla, como `ConfirmFits` de RACKLAYOUT,
  y a `WarehouseAutoFill.Fill` sobre `WarehouseSite.FromBoundary`, como RACKRELLENAR. La validación es la de los dos diálogos clásicos
  (`LocalizedNumberParser`, la coma decimal incluida), dicha campo por campo. El tope de 5000 del relleno, que hoy sale como error de
  AutoCAD, se dice en la página (dolor 12). Las pruebas de Core comparan la página con las llamadas de las órdenes clásicas.
- **La puerta** (`IRackCadWarehousePort`, en `WarehouseLayoutDoor.cs`; `RackCadRackDoorPort.Warehouse.cs` en el Plugin, con el mismo
  `Send` y su `Abandon`), con dos órdenes en `RackLayoutCommands.Ventana.cs`:
  - **`RACKALMACENLEER`** (`NoUndoMarker | UsePickSet | Redraw | NoHistory`): toma la petición, dice que la tomó (`Started`), comprueba
    el dibujo y lee la planta de la semilla (la seleccionada en AutoCAD; si no hay, la que la página leyó antes; si no, la única),
    medida con `SeedOf` —la medida de RACKLAYOUT, extraída de `ReadPlantaSeed` sin cambiar su cuerpo—, cuántas plantas tiene, el sitio
    de la capa con `ReadSiteFromLayer` de RACKRELLENAR y las capas del dibujo;
  - **`RACKALMACENCOLOCAR`**: vuelve a leer la planta de la petición (por su handle) y, para rellenar, el sitio; si la marca
    (`Fingerprint`) no es la de la lectura de la página, no coloca nada y entrega la lectura nueva (pregunta 5); si no, planea con las
    mismas reglas sobre lo que leyó ahora (`PlanPlace`), avisa de las unidades (I-05) y coloca con las mitades de las órdenes clásicas
    (`PlaceGrid` / `PlaceFill`: una transacción y una regeneración, el patrón de rendimiento de siempre). Es una orden: un «U» lo
    deshace todo. Su respuesta trae la lectura hecha después de colocar, en la misma orden (F-2: no se pide una lectura dentro de la
    respuesta de una escritura).
- **En la ventana:** una petición más de la puerta, la única en vuelo para toda la ventana (`DoorRequest.IsWarehouse`), numerada, con
  «Dejar de esperar» hasta que la orden la toma, la respuesta solo para su número y su dibujo (si llega con otro dibujo activo, no se
  usa y se dice), la respuesta durante la llamada (M-22) y la poda de un dibujo cerrado. La petición no lleva rack, para que las ramas
  de los editores (que buscan su borrador por el rack) nunca la tomen por suya.
- **Solo tras un gesto del usuario** se lee: la primera vez que se abre la sección con un clic para un dibujo, «Llevar al almacén…»,
  otra semilla u otra capa elegida, «Volver a leer» y «Restablecer valores» cuando cambia la capa. Cambiar de dibujo no envía nada.
  Cada dibujo guarda su semilla y su última lectura; tras cualquier orden que no sea la propia (la lectura, o la colocación desde que la
  orden la toma hasta el reposo), «Puede no estar al día» (como BM-S1). «Colocar» no se bloquea por eso: vuelve a leer antes de escribir.
- **Los valores se recuerdan por usuario** (pregunta 6) en `settings.json` (`UserSettings.WarehouseLayout`, `IRackCadWarehouseSettings`),
  nunca en el dibujo; «Restablecer valores» vuelve a los de las ventanas clásicas. «Almacén» no tiene nada sin escribir: no veta cerrar el
  dibujo (D-08).
- **«Llevar al almacén…»** en la página de cada rack (pregunta 1): abre «Almacén» con ese rack como semilla, en «Rejilla», y lo lee; sin
  planta, el botón se apaga y dice por qué.
- **«Nuevo»:** la frase de las órdenes que siguen en AutoCAD ya no nombra RACKLAYOUT y dice que el layout está en «Almacén».
- **No aplica a «Almacén»** (no es un sistema de rack): borradores de «Nuevo», panel de vistas, pestaña «Propiedades», «Guardar en
  biblioteca…», seguridad y BOM del borrador. Las copias que coloca cuentan en el BOM total como hoy (enlazadas, como copias; independientes,
  como racks propios).

**Cambios declarados fuera de la ventana:**
- `WarehouseAutoFillResult.Omitted` (Application, aditivo): las celdas candidatas descartadas, con su motivo. Es el veredicto que el bucle
  ya calculaba y tiraba; las celdas que se quedan, los recuentos y la orientación no cambian (prueba).
- `RackLayoutCommands.SeedOf` (Plugin): la medida de la semilla, extraída de `ReadPlantaSeed` con el mismo cuerpo; RACKLAYOUT y
  RACKRELLENAR la usan como antes.

**Supuestos y decisiones** (prefijo AL-S, cada uno en un sitio):

| Id | Supuesto o decisión | Dónde se cambia |
|---|---|---|
| AL-S1 | Las órdenes son `RACKALMACENLEER` (lee, sin Deshacer) y `RACKALMACENCOLOCAR` (escribe); el prototipo las dejaba [PENDIENTE] | `WarehouseLayoutRules.ReadCommand`, `PlaceCommand` |
| AL-S2 | **Pregunta 3:** la semilla es la planta seleccionada en AutoCAD; sin selección, la que la página leyó antes (su handle); si no, la única. Con varias y ninguna elegida, o con varias seleccionadas, la lectura no tiene semilla y dice qué hacer | `ReadSeedFacts` en `RackLayoutCommands.Ventana.cs` |
| AL-S3 | **Pregunta 5, la marca:** en «Rejilla», la planta (handle de la referencia y de su definición, origen, huella, desfase, giro, escala, el hash de su diseño, si su tipo se conoce y si tiene datos embebidos); en «Rellenar un área», además el sitio (contorno, obstáculos e ignorados). No cuenta cuántas plantas hay | `WarehouseLayoutRules.Fingerprint` |
| AL-S4 | La lista de capas es la de las capas propias del dibujo (sin las dependientes de un xref), por nombre, con la elegida aunque no exista | `ReadLayerNames`, `WarehousePageViewModel.BuildLayers` |
| AL-S5 | Los valores se guardan tal como se escriben (también uno no válido) y se cargan la primera vez que se muestra la página. **Corregido en la revisión (punto 6):** lo escrito en una caja se guarda al perder el foco, al colocar, al restablecer y al salir de la página; un interruptor, una elección o una capa, al momento | `WarehousePageViewModel.SetValues`, `CommitValues`, `LoadValues` |
| AL-S6 | Las semillas salen de la lista de la columna (sus vistas, como IN-03): un rack con planta insertada después no se ofrece hasta «Volver a leer la lista» | `RacksOfTheList` |
| AL-S7 | «Puede no estar al día» = corrió una orden que no es la propia, como BM-S1 (también volver a un dibujo) | `WarehousePageViewModel.Present` |
| AL-S8 | La lectura lee siempre la planta, el sitio de la capa elegida y las capas, también en «Rejilla» | `ReadWarehouse` |
| AL-S9 | El tipo sin manejador bloquea «Rejilla» (como RACKLAYOUT) y no «Rellenar un área» (RACKRELLENAR no lo comprueba, dolor 12): el cálculo de hoy | `WarehouseLayoutRules.Assess` |
| AL-S10 | El aviso de unidades (I-05) sale en la línea de órdenes al colocar, como en las órdenes clásicas; la página no lo dice (solo `RackUnitsGuard` lee INSUNITS) | `RACKALMACENCOLOCAR` |
| AL-S11 | Mientras AutoCAD coloca, la página tiene su propio «Dejar de esperar», además del de la ventana (como BM-S9) | `WarehousePageView.xaml` |
| AL-S12 | El esquema usa los tokens de siempre: la semilla rellena en azul, lo nuevo con borde azul, lo que se sale con borde rojo discontinuo, lo de fuera del contorno con borde gris discontinuo y lo de una columna en amarillo | `CellShape`, `CellText` |
| AL-S13 | Por debajo de 760 px de ancho, el resultado va debajo de los valores | `WarehousePageView.xaml.cs` |
| AL-S14 | Pregunta 10: RLY y RR abren los diálogos clásicos hasta la validación del Owner en AutoCAD; después, abrir «Almacén» con el rack designado es un cambio pequeño (como VA-S2) | — |
| AL-S15 | Con «Almacén» a la vista, un rack seleccionado en AutoCAD no lleva la ventana a su página (seleccionar la planta semilla es parte de la página, pregunta 3); la columna sigue la selección. En las demás páginas, como antes | `WarehouseKeepsPage`, dos condiciones en `RackCadWindowViewModel.cs` |

**Ediciones a archivos compartidos** (pequeñas y marcadas `T5 al`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `RackCadWindowPage.Almacen`; la entrada de `SectionOrder`; `DoorRequest.IsWarehouse`;
  una línea en el constructor (`WireWarehouse()`); `WaitingText` (dos líneas); `StopWaiting` (el texto con la petición tomada y una rama);
  una rama en la poda de una petición huérfana; una línea al final de `ShowPage` (`WarehousePageShown(page)`) y otra en `Present`
  (`PresentWarehouse`); y `!WarehouseKeepsPage` en `OnBrowserChanged` y en `FollowSelectionOfTheActiveRack` (AL-S15).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: dos bloques `<!-- T5 al -->`: «Llevar al almacén…» en la página del rack, tras
  «Ver su BOM», y el `ContentControl` `WarehousePage`, tras el del BOM. `RackCadWindowView.xaml.cs`: una línea en el constructor y el
  manejador del botón.
- `src/RackCad.UI/VentanaRackCad/RackListViewModel.cs`: `Entries` (interno, una línea).
- `src/RackCad.UI/VentanaRackCad/RackCadNuevoExtras.xaml`: la frase de las órdenes clásicas.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: una línea en `Abandon` (`AbandonWarehouse`).
- `src/RackCad.Plugin/RackLayoutCommands.cs`: `SeedOf`, extraída de `ReadPlantaSeed` (ver arriba).
- `src/RackCad.Application/Layout/WarehouseAutoFill.cs`: `Omitted` (ver arriba).
- `src/RackCad.Application/Settings/UserSettings.cs`: la propiedad `WarehouseLayout`.
- Archivos nuevos: `WarehouseLayoutDoor.cs`, `WarehouseLayoutRules.cs` (Application, `VentanaRackCad`), `WarehouseLayoutPreferences.cs`
  (Application, `Settings`); `RackLayoutCommands.Ventana.cs`, `RackCadRackDoorPort.Warehouse.cs` (Plugin); `WarehousePageViewModel.cs`,
  `WarehousePageView.xaml(.cs)`, `RackCadWindowViewModel.Warehouse.cs`, `WarehouseLayoutSettings.cs` (UI); y las dos clases de prueba.

**Censos y guardas que cambian, con su nota:**
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 57 a 59.
- `CustomPropertiesCommandGuardTests`: `A14WarehouseCommands = { "RACKALMACENCOLOCAR", "RACKALMACENLEER" }` en la base.
- `RackUnitsGuardSourceTests.EachInsertionPath_CallsTheGuard_OncePerOperation_NoAliasDoubleWarn`: `RackLayoutCommands.Ventana.cs`, una llamada.
- El orden de las secciones: `RackCadWindowTests.TheSectionsAreTheOnesOfThePrototype…`, `RackCadUtilitiesTests.AyudaIsAPageOfTheWindow…` y
  `RackCadNewDraftsTests` (la columna con los borradores) ganan «Almacén».
- Las guardas de capas y de `SendStringToExecute` de `RackCadWindowListTests` pasan sin cambios: el parcial de la puerta usa `Send` y las
  órdenes viven fuera de `Plugin/VentanaRackCad`.

**Las pruebas:**
- Core, `tests/RackCad.Tests/Workspace/WarehouseLayoutPageTests.cs` (nuevo, 17 casos):
  - los valores por defecto son los de los dos diálogos; el error de cada campo, el hueco solo con su casilla, la coma decimal, el edificio
    con los dos lados o ninguno, la rejilla de más de un rack y como mucho 1000;
  - la rejilla es `WarehouseGridPlanner.Plan` desde el origen de la semilla (celdas, etiquetas, totales y la fila sola de espalda con
    espalda); el encaje da los mismos racks fuera que `WarehouseFitChecker.Check` con el sitio de `ConfirmFits`;
  - el tipo sin manejador bloquea la rejilla y no el relleno; las independientes sin datos embebidos se rechazan;
  - el relleno es `WarehouseAutoFill.Fill` con los mismos argumentos (celdas y recuento), y el esquema tiene todas las candidatas (las
    guardadas, las de fuera y las de una columna); las celdas descartadas no cambian lo que el relleno guarda (la nave en L de
    `WarehouseAutoFillTests`); más de 5000 racks, nada que quepa, una capa sin contorno y una capa sin leer; la semilla dentro del área;
  - la marca sigue a la planta (origen, giro, diseño, otra planta) y, para rellenar, al sitio; «Colocar» coloca lo que mostró el esquema
    y nada cuando el dibujo cambió (con el texto); la rejilla que no cabe solo con «Colocar de todos modos»; los valores no válidos nunca;
  - los textos de la línea de órdenes (los de RACKLAYOUT y RACKRELLENAR) y de la página;
  - sobre la fuente: RACKALMACENLEER sin marca de Deshacer, con la selección, el orden de la petición, la medida de RACKLAYOUT
    (`SeedOf`, también en `ReadPlantaSeed`), el sitio de RACKRELLENAR y sin escribir nada él mismo; RACKALMACENCOLOCAR con marca de
    Deshacer, en este orden: tomar, `Started`, el dibujo, la relectura de esa planta, `PlanPlace`, la respuesta sin colocar, el aviso de
    unidades, `PlaceGrid` / `PlaceFill` y la lectura de después; la puerta con `Send` y `AbandonWarehouse`.
- UI, `tests/RackCad.UI.Tests/Workspace/RackCadWarehousePageTests.cs` (nuevo, 19 casos), con una puerta doble que implementa
  `IRackCadWarehousePort`:
  - la sección en su sitio y su primer clic, que lee una vez (y nada más: otra página, otro dibujo, la vuelta);
  - las semillas con planta y los que no la tienen; otra semilla y otra capa se leen; «Volver a leer» conserva la planta leída;
  - el borrador con cambios y las varias plantas, que avisan y no bloquean; varias sin elegir, que bloquean con su motivo;
  - el resultado y el esquema que siguen a cada valor; los errores en su campo, la marca de la pestaña, el tope del esquema; el hueco y
    la orientación girada con espalda con espalda; el relleno con sus cifras, su leyenda y la semilla dentro;
  - «Colocar»: una petición con la marca de la lectura y los valores, el editor bloqueado, «Dejar de esperar» hasta `Started`, la orden
    propia que no marca nada, la respuesta con la lectura nueva, sin segunda orden y con los valores guardados;
  - el dibujo que cambió (la lectura nueva entra en la página); la rejilla que no cabe y su pregunta en línea;
  - una petición para toda la ventana; la respuesta de otro número y la de otro dibujo, que no se usan; «Dejar de esperar» de la lectura
    y de la colocación, y la colocación ya tomada, que no se suelta; la entrega fallida y el dibujo cerrado;
  - «Puede no estar al día» tras una orden ajena y no tras la propia; los valores recordados y «Restablecer valores», que vuelve a leer
    si cambia la capa; el dibujo que se cierra sin veto; la ventana sin la puerta de la página;
  - «Llevar al almacén…» (sin planta, apagado con su motivo; con planta, lee); una selección en AutoCAD con «Almacén» a la vista (AL-S15);
  - con WPF real (STA) y sin errores de enlace: la página y sus controles (el valor escrito, el esquema con sus nombres, el error, el rack
    elegido que se conserva al rehacer las listas, la elección como gesto, el relleno y su capa, «Colocar» y su aviso, «Dejar de esperar»,
    «Cerrar aviso», las copias, nombres únicos y «Restablecer»); la pregunta del encaje y el aviso «El dibujo cambió…»; el resultado debajo
    en estrecho; y la ventana que aloja la página, la sección en la columna y «Llevar al almacén…» en la página del rack, sin nombres de
    tipo en la página (F-1).

**Comprobación en rojo:** dieciocho piezas rotas a propósito (diecisiete y la U2b), una a una, con script (`hs65\t5\al\red.py` en el scratchpad de la sesión,
salida en `red-out.txt`). Antes de cada rotura el script guarda una copia del archivo FUERA del worktree; después compila, corre la clase de
la pieza y devuelve el archivo a sus bytes, con el mismo SHA-256 (lo comprueba). Tras la última se buscaron restos en `git diff`: ninguno.
1. (C1) la rejilla sin el origen de la semilla: fallan 3;
2. (C3) «Colocar» sin comparar la marca: falla 1;
3. (C4) la marca sin el giro: falla 1;
4. (C5) el relleno sin las celdas de fuera del contorno: fallan 2;
5. (C6) una rejilla que no cabe se coloca sin «Colocar de todos modos»: falla 1;
6. (C7) RACKALMACENLEER con marca de Deshacer: falla 1;
7. (C8) RACKALMACENCOLOCAR sin el aviso de unidades: fallan 2 (la de la fuente y `RackUnitsGuardSourceTests`);
8. (U1) la página lee sola al presentarse un dibujo: fallan 8;
9. (U2) la respuesta de la lectura sin contar como orden propia: **sobrevivió**, porque `Taken` ya la cuenta (la orden siempre dice
   `Started` antes de responder); quitadas las dos (U2b): falla `AReadingMayBeOutOfDateAfterACommandThatIsNotItsOwn`;
10. (U3) una orden después de la lectura no la marca: fallan 2;
11. (U4) la ventana acepta la respuesta de otro número: falla 1;
12. (U5) «Dejar de esperar» no avisa a la página: fallan 3;
13. (U6) «Colocar» no pregunta con la rejilla fuera del edificio: fallan 2;
14. (U7) los valores no se recuerdan: fallan 2;
15. (U8) la vista no repone el rack elegido: falla 1, la de la vista;
16. (U9) la sección fuera de la columna: fallan 2;
17. (U10) «Llevar al almacén…» sin leer: falla 1.

**Los números** (solución en Debug recompilada con `--no-incremental` sobre las fuentes finales, tras la comprobación en rojo):
- **UI completa:** 2980 correctas, 0 con error, 48 omitidas (3028): la base de `041e1799` (2961 / 0 / 48) más las 19 nuevas; 32 min, con
  otras partes usando la máquina.
- **Core Full:** 19310 correctas, 0 con error, 403 omitidas (19713): la base (19292 / 0 / 403) más las 17 nuevas y el caso nuevo de
  `RackUnitsGuardSourceTests`; 34 min.
- **Build Debug de la solución (Plugin incluido):** 0 errores. Las advertencias son las MSB3277 de las referencias de AutoCAD y las de los
  analizadores de xunit, que ya estaban; ninguna sale de los archivos de esta parte.

**Lo que queda abierto:**
- **El smoke en AutoCAD:**
  - que RACKALMACENLEER no deje paso en Deshacer y conserve la selección previa y el foco (`UsePickSet`, `Redraw`);
  - la semilla por la selección, con varias plantas;
  - un solo «U» tras RACKALMACENCOLOCAR, con una rejilla enlazada, una independiente y un relleno;
  - «No se colocó nada» tras mover la planta entre la lectura y «Colocar»;
  - el tiempo con un layout grande;
  - la página en la paleta (anchos, foco, el esquema).
- **Pregunta 10 completa:** RLY y RR abren «Almacén» con el rack designado cuando el Owner valide la página (AL-S14).
- El aviso de unidades en la página (AL-S10).
- Una lectura pedida mientras otra está en vuelo (otra semilla u otra capa durante la lectura) no se encola: la página dice «Sin leer» y
  pide «Volver a leer».
- **Para el Owner:** AL-S1 a AL-S15.
- **Para integrar:** `RackCadWindowViewModel.cs` (once líneas marcadas), `RackCadWindowView.xaml` (dos bloques), `RackListViewModel.cs` (una
  línea), `RackCadRackDoorPort.cs` (una línea) y los tres censos de órdenes los tocan también las otras partes de la tanda (pb, ct, cm):
  el censo de `SelectiveEditorOpenTests` suma sus órdenes a las 59 de esta parte.

### Quinta tanda, al: correcciones de la revisión (sobre `1fd0f38d`)

En `D:\w5al`, sobre el commit `1fd0f38d` de la parte (lo hizo el coordinador). Sin commit, sin push, sin AutoCAD. Los nueve puntos de la
revisión independiente, cada uno con su prueba; los puntos 1, 4 y 6 (y además el 5, el 7 y el 8) con comprobación en rojo.

| Punto | Qué pasaba | Qué se hizo | Prueba |
|---|---|---|---|
| 1 | Un fallo después de escribir (la regeneración o la lectura posterior) salía como «No se pudo colocar» con `Placed=false`: «Colocar» seguía encendido y un segundo «Colocar» apilaba otra rejilla | `PlaceGrid` y `PlaceFill` ganan `regen = true` (RACKLAYOUT y RACKRELLENAR regeneran igual); RACKALMACENCOLOCAR las llama con `regen: false`. Un solo `try` cubre hasta la confirmación de la transacción; desde ahí la regeneración, la lectura posterior y la línea de órdenes tienen su propio `try`, y la respuesta es siempre `Placed=true` (con una lectura fallida si no se pudo leer). La página dice que se colocó y, sin la lectura de después, apaga «Colocar» con «La página no pudo volver a leer el dibujo después de colocar: pulsa «Volver a leer» antes de colocar otra vez.» (`MustReread`, hasta la lectura siguiente) | UI `APlacementWhoseReadingAfterFailedSaysItWasPlacedAndAsksToReadAgain`; Core, la fuente de RACKALMACENCOLOCAR (ninguna respuesta `false` tras colocar, los dos `try`, `regen: false`, y `if (regen)` en las dos mitades) |
| 2 | Las referencias se colocan en la capa actual; si es la del sitio, la siguiente lectura las toma como obstáculos | Sin cambiar el cálculo (pregunta 9): la lectura trae la capa actual (`WarehouseReading.CurrentLayer`, de `Database.Clayer`) y, en «Rellenar un área», la página lo dice en «Sitio» antes de «Colocar» (`WarehouseLayoutRules.CurrentLayerWarning`). No bloquea | Core `TheCurrentLayerOfTheDrawingOnTheLayerOfTheSiteIsSaidBeforeColocar`; UI `TheCurrentLayerOnTheLayerOfTheSiteIsSaidBeforePlacing` y la vista |
| 3 | Pasadas repetidas en «Colocar» | La relectura de comparación no lee las capas, y en «Rejilla» tampoco el sitio (la marca no lo usa). La lectura de después no vuelve a recorrer la tabla de bloques: es la de comparación (colocar no toca la semilla) con las plantas recontadas por lo colocado (una copia enlazada es una planta más; una independiente, un rack propio), el sitio de antes salvo que la capa actual sea la del sitio (entonces se relee) o que no se hubiera leído («Rejilla»: una vez, aquí), y las capas. `ReadWarehouse` gana `readSite` y `readLayers` | Core, la fuente: `readSite: !grid, readLayers: false`, un solo `FindRackBlocks` en el archivo, `AfterPlacing` sin `ReadSeedFacts` y con el recuento |
| 4 | La relectura de «el dibujo cambió» pedía la selección en una orden sin `UsePickSet` | RACKALMACENCOLOCAR lleva `UsePickSet \| Redraw` (sin `NoUndoMarker`: un «U» lo deshace), coherente con AL-S2: la lectura nueva se hace como la de la página, con la planta seleccionada | Core, la fuente: los indicadores de la orden y `useSelection: true` en esa relectura |
| 5 | «Verificar encaje (opcional)» salía centrado (la plantilla de `SecondaryButtonStyle` centra su contenido, como F-3) | Un estilo propio, `DisclosureButton`, cuya plantilla respeta `HorizontalContentAlignment` (estirado), y un `DockPanel` con la marca ▸/▾, el título a la izquierda y el resumen a la derecha, como el prototipo | UI, la vista: el resumen termina en el borde derecho del botón, la marca empieza en el izquierdo, y la marca cambia al abrir |
| 6 | Cada tecla releía y reescribía `settings.json`; `UserSettings` perdía los campos que no conoce; un tipo equivocado en `WarehouseLayout` ponía el archivo entero en cuarentena | Lo escrito en una caja se guarda al perder el foco (`CommitValues`, desde `LostKeyboardFocus` de la vista), al colocar, al restablecer y al salir de la página; un interruptor, una elección o una capa, al momento. `UserSettings` gana `[JsonExtensionData]` (el patrón de I-11): los campos de otra versión sobreviven a una carga y un guardado. `WarehouseLayout` se lee con `LenientWarehouseLayoutConverter`: un valor ilegible solo deja esa sección en los valores de las ventanas clásicas, con su línea en el registro, y el archivo no va a cuarentena; un archivo que no es JSON sigue yendo (I-03) | Core `WarehouseLayoutSettingsTests` (nuevo, 3 pruebas con 6 casos): un campo desconocido sobrevive; cuatro secciones ilegibles (número por texto, texto por interruptor, un texto y una lista) no ponen en cuarentena y dejan el resto; un archivo que no es JSON sí. UI: la prueba de los valores (teclear no guarda, `CommitValues` sí y una vez), `TheValuesTypedAreRememberedWhenThePageIsLeftAndASwitchAtOnce` y la vista (la caja que pierde el foco guarda) |
| 7 | «Llevar al almacén…» durante una lectura no leía nada | Se apaga mientras la página espera a AutoCAD (lectura o colocación), con su motivo; y una semilla o una capa elegidas mientras la página lee lo dicen: «AutoCAD todavía está leyendo la planta: cuando termine, pulsa «Volver a leer» para leer lo que elegiste.» | UI `LlevarAlAlmacenWaitsForTheReadOfThePageAndAChoiceDuringItIsSaid` |
| 8 | La lista de capas quedaba sin elegir cuando la capa solo difería en mayúsculas | `SelectedLayer` es la capa de la lista equivalente sin distinguir mayúsculas, y elegirla no relee (las capas de AutoCAD no distinguen mayúsculas) | UI `TheLayerChosenIsTheOneOfTheDrawingWhateverItsCase` y la vista |
| 9 | La prueba de «Puede no estar al día» usaba otro orden que AutoCAD | Caso nuevo con el orden real (la orden empieza, `CommandWillStart`, antes de decir que tomó la petición), para la lectura y para la colocación: la respuesta deja la lectura al día, y solo una orden después del reposo vuelve a marcarla. El código no cambió | UI `InTheOrderOfAutoCadTheOwnReadAndTheOwnPlacementLeaveTheReadingUpToDate` |

**Ediciones de esta corrección:**
- **A archivos compartidos:** `src/RackCad.Application/Settings/UserSettings.cs` (`[JsonConverter]` en `WarehouseLayout`, `ExtensionData` y dos
  `using`); `src/RackCad.Plugin/RackLayoutCommands.cs` y `RackLayoutCommands.Fill.cs` (`regen = true` en `PlaceGrid` y `PlaceFill`, una
  línea cada una más la firma).
- **En los archivos de la parte:** `WarehouseLayoutPreferences.cs` (el convertidor), `WarehouseLayoutDoor.cs` (`CurrentLayer`),
  `WarehouseLayoutRules.cs` (`CurrentLayerWarning`), `RackLayoutCommands.Ventana.cs`, `WarehousePageViewModel.cs`, `WarehousePageView.xaml(.cs)`
  y `RackCadWindowViewModel.Warehouse.cs`.
- **Pruebas:** `WarehouseLayoutPageTests` (Core, de 17 a 18, y la de la fuente de RACKALMACENCOLOCAR reescrita), `WarehouseLayoutSettingsTests`
  (Core, nuevo, 6 casos) y `RackCadWarehousePageTests` (UI, de 19 a 26).

**Comprobación en rojo** (`hs65\t5\al\red2.py` en el scratchpad de la sesión, con la mecánica de `red.py`; salida en `red2-out.txt`): nueve
piezas rotas a propósito, una a una. Antes de cada rotura se guarda una copia del archivo fuera del worktree; después se compila, se corre
la clase y se devuelve el archivo a sus bytes, con el mismo SHA-256 (lo comprueba el script). Tras la última, sin restos en `git diff`.
1. (R1a) la página no espera «Volver a leer» tras colocar sin lectura: falla `APlacementWhoseReadingAfterFailed…`;
2. (R1b) la lectura de después sin su propio `try`: falla la de la fuente de RACKALMACENCOLOCAR;
3. (R4) RACKALMACENCOLOCAR sin `UsePickSet`: falla la de la fuente;
4. (R6a) cada tecla guarda los ajustes: fallan 3 (la de los valores, la de salir de la página y la de la vista);
5. (R6b) sin el convertidor de `WarehouseLayout`: fallan los 4 casos de la sección ilegible (el archivo va a cuarentena);
6. (R6c) sin `[JsonExtensionData]`: falla la del campo de otra versión;
7. (R5) «Verificar encaje» con `SecondaryButtonStyle`: falla la de la vista (el resumen queda centrado);
8. (R7) «Llevar al almacén…» encendido durante la lectura: falla `LlevarAlAlmacenWaits…`;
9. (R8) la capa que distingue mayúsculas: fallan 2 (la del modelo y la de la vista).

**Los números** (solución en Debug recompilada con `--no-incremental`, sobre las fuentes finales, tras la comprobación en rojo):
- **Clases focales:** Core (`WarehouseLayoutPageTests`, `WarehouseLayoutSettingsTests`, `UserSettingsStoreTests`, `SelectiveTargetPreferenceTests`,
  `RackUnitsGuardSourceTests`, `KindHandlerGuardSourceTests`, `SelectiveDuplicationFailClosedTests`, `DiagnosticsNegativeTests`,
  `RackCadUtilitiesCoreTests`): 144 de 144; UI `RackCadWarehousePageTests`: 26 de 26.
- **UI completa:** 2987 correctas, 0 con error, 48 omitidas (3035): las 2980 de `1fd0f38d` más las 7 nuevas; 21 min.
- **Core Full** (se tocó `UserSettings`): 19317 correctas, 0 con error, 403 omitidas (19720): las 19310 de `1fd0f38d` más la prueba nueva de
  `WarehouseLayoutPageTests` y los 6 casos de `WarehouseLayoutSettingsTests`; 14 min.
- **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias son las de siempre (MSB3277 y los analizadores de xunit) y
  ninguna sale de los archivos de esta parte.

## Quinta tanda, cm: las camas de rodamiento en la ventana (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t5-cm`, sobre `041e1799` (worktree `D:\w5cm`). Sin commit, sin push, sin AutoCAD.
Prototipo `camas` (preguntas 1 a 8): el Owner aceptó las ocho propuestas sin nota (`owner-decisions-2026-11-26.md`). No se borró código
clásico: RACKEDITAR de una cama sigue abriendo `RackFlowBedWindow` (D-03), «Nuevo» conserva la tarjeta «Cama» (RACKNUEVO Cama) y QUICKCAMA
no cambia (dolor 13).

**Lo que se construyó:**
- **El editor de la cama en la ventana** (`CamaEditorViewModel` y `CamaEditorView`, UI), sobre un borrador puro (`CamaDraftEditor` y
  `CamaDraftRules`, Application, `VentanaRackCad/CamaDraftEditor.cs`):
  - **Una sola página (pregunta 1):** «Lo esencial» (Tipo de cama, Rodillo, Profundidad de la cama, Profundidad de tarima), «Calculado»
    (Riel, Rodillos, Frenos, Paso —en otro color y «Manual (en «Avanzado»)» cuando es manual— y Tope), «Esquema de la cama» y «Avanzado»
    plegado (Paso de rodillos, Freno cada N rodillos), cuyo resumen dice qué es manual también plegado (dolor 7). Al final, la pestaña
    «Propiedades» (VA-07), así que el editor tiene dos pestañas: «Cama» y «Propiedades» (CM-S2).
  - **La cabecera:** el nombre (con lo que significa vacío), Limpio / Modificado / Escribiendo, «N valores cambiados», UN botón que escribe
    («Actualizar», o «Insertar en el dibujo…» en una cama nueva), «Descartar», «Restaurar layout…», «Cerrar el editor» (pregunta en línea
    con cambios) y «Más» › «Guardar en biblioteca…». La línea de debajo dice que la cama es de una sola vista, así que no tiene «Insertar
    vista…», y que «Actualizar» redibuja su bloque y todas sus copias (pregunta 5).
  - **Una sola forma de confirmar:** un campo, al salir o con Enter; una lista, al elegir. Salir de un campo sin cambiar su texto no hace
    nada. «Actualizar vista» no existe: todo se recalcula solo (dolores 3 y 4).
  - **Los valores no válidos (dolor 5):** cada uno se queda en su campo, marcado (borde y texto en rojo), todos a la vez; el borrador no lo
    toma (la regla de la ventana) y el botón que escribe se apaga con el motivo: «Corrige el valor marcado para actualizar.», «Corrige los 2
    valores marcados para actualizar.» (en una cama nueva, «… para insertarla.»). Mientras haya alguno, «Calculado» y el esquema dicen que
    hay que corregir (CM-S5). Se quita escribiéndolo bien, volviendo a escribir el que tenía, con «Descartar» o con «Restaurar layout…».
  - **Pregunta 4, el paso:** la regla de la cama de Dinámico: un entero de pulgadas y no menor que el rodillo («Los rodillos se cuelgan en
    los troqueles: …», «El paso no puede ser menor que el rodillo: mínimo 2 in.»). Una cama del dibujo con un paso así (el clásico aceptaba
    2.5, o 1 con el rodillo de 1.9) se abre marcada, con «Avanzado» abierto, y «Actualizar» pide corregirla. Un rodillo mayor que el paso
    manual se rechaza bajo su lista y el rodillo no cambia.
  - **Pregunta 3, «Freno cada N rodillos»:** en «Avanzado»; vacío = por la profundidad de tarima, como siempre. **Cambio declarado** del
    documento de la cama: `FlowBedDocument.BrakeEveryRollers`, que el constructor ya leía (paso 3 de Dinámico) y el documento no guardaba.
    Solo se escribe con valor (`JsonIgnore` `WhenWritingNull`): una cama sin él se escribe byte a byte como antes (prueba con el texto
    exacto), y un build anterior lo guarda como campo desconocido sin usarlo. El BOM del manejador lo lee del dibujo (prueba).
  - **Pregunta 6, «Restaurar layout…»:** en línea, con «Paso de rodillos manual (4 in)» y «Freno cada N rodillos (8)», marcadas; una
    casilla sin valor se apaga. Sin nada manual lo dice («No hay nada manual que quitar: …») y no se abre. Va al borrador.
  - **Pregunta 7, la pendiente:** la cama suelta sigue horizontal; no hay nada que construir.
  - **Pregunta 8, de Pushback a Dinámica:** con la tarima vacía (como la guarda el clásico) toma 48 in y lo dice. Una Pushback conserva su
    tarima y su «Freno cada N» (campos apagados, «No aplica en Pushback… Conserva su valor.»; CM-S7).
  - **Las reglas de la geometría no cambian:** «Calculado», el esquema, sus posiciones y el BOM salen de los mismos `FlowBedLateralBuilder`
    y `FlowBedBomBuilder` que el dibujo. Con los valores por defecto del clásico: 42 rodillos y 1 freno a 56 in; Pushback de 96 in, 45
    rodillos; rodillo de 2.5, 29 rodillos y 1 freno (pruebas). La capacidad del catálogo (110 / 150 kg) sale como pista del rodillo
    (CM-S14).
  - **El esquema (pregunta 2):** una línea a escala, plegable, abierta por defecto: el riel, la zona del tope, los rodillos y los frenos
    donde los pone el constructor, y las marcas de la escala (cada 12, 24, 48 o 96 in según el largo); la leyenda; la lista de posiciones
    («Rodillos 1 a 23: de 7 a 51 in, cada 2 in.», «Freno 1: a 56 in.», cortada a ocho líneas y «… y N líneas más.»); y la nota de que se
    mide desde el troquel del tope y no lleva cotas. Lo dibuja un elemento propio (`CamaSchemaStrip`) con los tokens del shell, y su nombre
    de UI Automation es el texto del esquema («Esquema de la cama: tope, 42 rodillos y 1 freno (a 56 in) en 96 in de riel.»).
- **La puerta** (`CamaDoor.cs` en Application; `RackCadRackDoorPort.Cama.cs` y `RackCadCamaDoorCommands.cs` en el Plugin), con las reglas
  de las otras: número de petición, la petición guardada antes de entregar la orden, otro dibujo, «Dejar de esperar» (`Abandon`),
  `RequestStarted`, la copia de la cama que espera es de la puerta, y la comparación de las vistas hermanas (M-02):
  - `RACKCAMAABRIR` (`NoUndoMarker | NoHistory`): lee la cama con la regla pura `CamaDoorRules.AnswerForOpen` (la de EditCama: otro tipo o
    datos inválidos no abren) y las propiedades del rack (VA-07).
  - `RACKCAMAAPLICAR`: planea otra vez sobre el dibujo (`PlanApply`), redibuja el bloque en sitio con `RackCamaCommands.ApplyCamaUpdate`
    (nombre en blanco = el del sobre, I-61; el documento de origen viaja, I-11) y escribe después la pestaña «Propiedades». Un «U» lo
    deshace. «Escrito» solo si se redibujó (`AnswerForApply`).
  - `RACKCAMANUEVO`: inserta la cama nueva con el despacho de «Nuevo» (`DispatchInsertion` con `FlowBedInsertionRequest`: el jig y el
    nombre automático de I-60), sin insertar sobre una identidad que ya existe, y cuenta los bloques colocados (`AnswerForNewCama`).
  - `ApplyCamaUpdate` es una **copia** de la mitad de dibujo de EditCama, no una extracción (CM-S11): la guarda I-61 lee el texto de
    EditCama; una guarda de fuente nueva fija que las dos hacen los mismos pasos en el mismo orden.
- **En la ventana** (`RackCadWindowViewModel.Cama.cs`, `RackCadWindowView.Cama.cs`):
  - **«Editar aquí» de una cama de la lista** (dolor 15), con su recuadro en la página del rack (`RackCadCamaOfferPanel`). El editor se abre
    con la respuesta de ESA petición; la lista lo marca («Abierto en la ventana», «Modificado (editor de la ventana)»); «Actualizar» envía
    una copia, el editor se bloquea mientras AutoCAD escribe, «Limpio» solo si se redibujó; el borrador de otro dibujo no se escribe; la
    poda de un dibujo cerrado dice lo que se perdió; y el dibujo no se cierra con una cama sin escribir (D-08).
  - **«Cama en esta ventana»** de «Nuevo» (`RackCadNewCamaButton`): un borrador más («Borrador N · Cama», SD-06) por el punto de entrada
    (`OpenAsNewDraft(RackCadDraftSeed.Defaults(Cama))`), con su caso en los seis métodos de `RackCadWindowViewModel.Drafts.cs`. «Insertar
    en el dibujo…» envía RACKCAMANUEVO; «Borrador N» nunca se envía como nombre. Insertada, la cama toma el nombre del dibujo, sale de la
    lista de borradores, «Nuevo» vuelve a su inicio y el aviso pide releer la lista; al abrirla, es el mismo editor, con «Actualizar»
    (D-10, CM-S4). Sin colocar, sigue como borrador. «Duplicar» y «Descartar el borrador» funcionan como en los otros sistemas.
  - **Biblioteca:** «Abrir» abre una cama aquí, como borrador, con el nombre de la entrada (como el clásico abierto desde la biblioteca), y al
    insertarla lleva el documento del diseño, con sus campos desconocidos (CM-S13). «Más» › «Guardar en biblioteca…» guarda lo que ve el
    editor, borrador incluido, con los campos desconocidos de su origen (del dibujo o de la biblioteca, I-11, como el clásico); un valor
    marcado lo detiene y se nombra.
  - **BOM:** la página «BOM y lista de racks» muestra el BOM del borrador de una cama del dibujo con cambios (BM-02: el del manejador,
    sobre una copia); una cama nueva no, porque no está en el dibujo (BM-S7). «Ver su BOM» ya estaba en la página del rack.
- **El clásico** (`RackFlowBedWindow`) no muestra «Freno cada N», pero lo conserva al escribir o guardar una cama que lo trae, como el
  clásico de Dinámico conserva la cama de un frente (prueba).
- **Los avisos de los borradores** dicen «Cama nueva» (y «Cabecera nueva»), no «Cama nuevo» (`RackCadDraftNaming.NewText`, CM-S9).

**Decisiones y supuestos para el Owner** (prefijo CM-S; cada uno en un solo sitio):

| Id | Decisión o supuesto | Dónde se cambia |
|---|---|---|
| CM-S1 | Los nombres de las órdenes, que el prototipo dejaba [PENDIENTE]: `RACKCAMAABRIR`, `RACKCAMAAPLICAR`, `RACKCAMANUEVO`, por analogía con Dinámico | `RackCadRackDoorPort.Cama.cs`, `RackCadCamaDoorCommands.cs` |
| CM-S2 | La nota de VA-07 («una pestaña en cada rack») manda sobre la pregunta 1 («sin pestañas»): la página de la cama es la pestaña «Cama», y «Propiedades» va al final | `CamaEditorView.xaml` |
| CM-S3 | «Insertar en el dibujo…» de una cama nueva no abre el panel de vistas: la cama no tiene vistas que elegir (DG-07, pregunta 5); envía RACKCAMANUEVO, que pide el punto con el jig. No hay «Insertar otra copia…» (la pregunta 5 lo dejaba abierto): COPY de AutoCAD, o «Duplicar» de un borrador | `CamaEditorViewModel.RequestWrite` |
| CM-S4 | Insertada una cama nueva, «Nuevo» vuelve a su inicio y el mismo editor es el de la cama del dibujo cuando se abre de la lista (D-10, como Selectivo y Dinámico); el prototipo la dejaba abierta en su página. La ventana no relee la lista sola | `RackCadWindowViewModel.Cama.cs`, `CamaWritten` |
| CM-S5 | Un valor rechazado no entra al borrador (la regla de la ventana; el prototipo lo guardaba y la cama pasaba a «Modificado»), pero se queda en su campo, marcado, y apaga el botón que escribe. Con algún valor marcado, «Calculado» y el esquema no calculan con el último valor válido: dicen «Corrige…» | `CamaEditorViewModel.CommitField`, `MarkedKeys` |
| CM-S6 | Un paso escrito igual al del rodillo (2 in con el de 1.9) se guarda como manual, como lo escribió el usuario (la cama de Dinámico lo guarda vacío para no cambiar su línea del BOM; el BOM de una cama no lleva el paso) | `CamaDraftEditor.SetPitch` |
| CM-S7 | Una Pushback conserva su profundidad de tarima y su «Freno cada N» (el prototipo: «conserva su valor»); el clásico escribe 0 de tarima en una Pushback. No cambian ni el dibujo ni el BOM (el constructor no los lee en una Pushback) | `CamaDraftEditor.SetBedType` |
| CM-S8 | El nombre previsto («Cama 3») no se muestra: no se sabe sin leer el dibujo (P3-02). El campo dice «Vacío = el nombre automático al insertarla («Cama N», I-60).» | `CamaEditorViewModel.NameHint` |
| CM-S9 | «Cama nueva» y «Cabecera nueva» en los avisos de los borradores (antes «… nuevo» para todo sistema) | `RackCadDraftNaming.NewText` |
| CM-S10 | El esquema es un elemento propio dibujado con los tokens del shell; su escala sigue la del prototipo (12 in hasta 120 in de riel, luego 24, 48 y 96) | `CamaSchemaStrip`, `CamaEditorViewModel.SchemaTickStep` |
| CM-S11 | `ApplyCamaUpdate` es una copia de la mitad de dibujo de EditCama, con guarda de fuente de los pasos (como CB4-09) | `RackCamaCommands.Door.cs`, `CamaDoorTests` |
| CM-S12 | La tarjeta «Cama» de «Nuevo» conserva su nombre, como las de Selectivo y Dinámico (el prototipo la llamaba «Cama (editor clásico)») | `RackCadWindowViewModel.NewSystems` |
| CM-S13 | Una cama nueva abierta desde la biblioteca lleva su documento al insertarse (sus campos desconocidos llegan al dibujo, I-11), a diferencia de RACKSELECTIVONUEVO y RACKDINAMICONUEVO (UT-S10) | `RackCadWindowViewModel.SourceDesignJsonOf` |
| CM-S14 | La capacidad del rodillo (110 / 150 kg del catálogo) es solo una pista: el cálculo de rodillos por capacidad y de frenos N-1 sigue siendo futuro (memoria del dominio); la regla de hoy, paso fijo y freno cada N o por la tarima, no cambia | `CamaEditorViewModel.Rebuild` |
| CM-S15 | **Mezclar builds (para el smoke; punto 7 de la revisión).** Un DLL anterior a esta parte que hace RACKEDITAR sobre una cama con «Freno cada N» conserva `BrakeEveryRollers` como campo desconocido del documento (I-11), pero dibuja sin él: pone los frenos por la tarima. El bloque queda entonces con unos frenos y el documento con otro valor, así que el BOM de un build nuevo (que lee el documento) cuenta los frenos cada N mientras el dibujo tiene otros. En el smoke, la cama con «Freno cada N» se escribe y se edita con el mismo DLL; un RACKEDITAR con un DLL viejo se vuelve a escribir con «Actualizar» del nuevo | `FlowBedDocument.BrakeEveryRollers` |

**Ediciones a archivos compartidos** (marcadas `T5 cm`, casi todas de una línea: en `OpensHere` e `IsEditorAlive` la condición sigue en una segunda línea, `NewText` es un método nuevo de cuatro líneas y `A14CamaDoorCommands` una declaración nueva con su comentario; lo nuevo va en archivos propios):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `DoorRequest` gana `IsCama`; en el constructor `WireCamaDoor(door)`;
  `ShowPanel` y `HasRackEditor` cuentan el editor de la cama; una rama en `StopWaiting`, `OnDesignWritten` y `OnRequestStarted`; una
  condición en `OnDesignRead` y en `DraftStateOf`; una línea en `UnwrittenWorkOf`, `PruneDeadSessions`, `RaiseDynamic` y `RaiseNewDynamic`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Drafts.cs`: el caso de la cama en los seis métodos por sistema, y «Cama nueva» en
  el texto de «Descartar el borrador» y en el de un borrador abierto de un diseño (`OpenedText`).
- `src/RackCad.UI/VentanaRackCad/RackCadDrafts.cs`: `NoticeName` usa `NewText` (nuevo, cuatro líneas).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Utilities.cs`: el mapa del menú clásico («Diseñar cama de rodamiento»),
  `OpensHere`, el editor de `OpenDesignAsNewRack` e `IsEditorAlive`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Bom.cs`: `DraftBomOf`, `HasUnwrittenDraft` y `UnwrittenDraftsOf`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: tres líneas, cada una un control de esta parte: el recuadro «Editar aquí» de la
  cama (tras el de la cabecera), el anfitrión `NewCamaRackHost` (tras el del Selectivo nuevo) y «Cama en esta ventana» (tras «Dinámico en
  esta ventana»).
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml.cs`: `WireCamaView()` en el constructor y `if (ShowCamaContent()) return;` en
  `ShowRackContent`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: `AbandonCama(requestId)` en `Abandon`.
- `src/RackCad.Plugin/RackCamaCommands.cs`: la clase pasa a `partial` (EditCama no cambia).
- `src/RackCad.Application/Persistence/FlowBedDocument.cs`: `BrakeEveryRollers` (pregunta 3, cambio declarado).
- `src/RackCad.UI/Systems/FlowBed/RackFlowBedWindow.xaml.cs` (el clásico de la cama): conserva `BrakeEveryRollers`.
- **Archivos nuevos:** Application: `VentanaRackCad/CamaDraftEditor.cs`, `VentanaRackCad/CamaDoor.cs`; UI: `CamaEditorViewModel.cs`,
  `CamaEditorView.xaml(.cs)`, `CamaSchemaStrip.cs`, `RackCadWindowViewModel.Cama.cs`, `RackCadWindowView.Cama.cs`,
  `RackCadCamaOfferPanel.xaml(.cs)`, `RackCadNewCamaButton.xaml(.cs)`; Plugin: `VentanaRackCad/RackCadCamaDoorCommands.cs`,
  `VentanaRackCad/RackCadRackDoorPort.Cama.cs`, `RackCamaCommands.Door.cs`; y las pruebas.

**Censos y guardas que cambian, con su nota:**
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 57 a 60 (RACKCAMAABRIR, RACKCAMAAPLICAR, RACKCAMANUEVO).
- `CustomPropertiesCommandGuardTests`: `A14CamaDoorCommands` entra en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `RackCadCamaDoorCommands.cs` es un archivo de órdenes de la puerta, como
  el de la cabecera, y `ApplyCamaUpdate` entra en la lista de lo que ningún otro archivo de la puerta puede llamar.
- Las tres fallaron con la primera Core focal (su rojo) y pasan con la nota.

**Las pruebas:**
- Core, `tests/RackCad.Tests/Workspace/CamaDraftRulesTests.cs` (nuevo, 21 casos): los valores por defecto (42 rodillos y 1 freno a 56 in,
  las mismas instancias que el constructor, las líneas de posiciones), Pushback y el rodillo de 2.5, el freno cada N, el corte a nueve
  líneas y el riel sin rodillos, el documento sin «Freno cada N» igual byte a byte y con él guardado y leído por el BOM, el paso
  (pregunta 4, nueve casos), la cama leída con un paso no válido, los problemas según el tipo, el rodillo rechazado, de Pushback a Dinámica
  (pregunta 8), «Restaurar layout…» (pregunta 6), las comprobaciones de los valores y el BOM del borrador.
- Core, `tests/RackCad.Tests/Workspace/CamaDoorTests.cs` (nuevo, 9): las reglas (`AnswerForOpen`, `PlanApply`, `AnswerForApply`,
  `AnswerForNewCama`, `RefusalForNewCama`) y el cableado sobre la fuente: las tres órdenes en su orden (sin Deshacer en la lectura,
  «escrito» solo por la regla), la mitad de dibujo con los mismos pasos que EditCama y los huecos de la puerta con su copia y su `Abandon`.
- UI, `tests/RackCad.UI.Tests/Workspace/CamaEditorTests.cs` (nuevo, 12): el editor sin la ventana (lo que calcula y el esquema, el
  borrador y «Descartar», los valores marcados y el motivo, la pregunta 4, la 8, el rodillo rechazado, la 6, «Cerrar el editor», la
  escritura y su base, la cama nueva y su inserción, el freno cada N con Pushback y «Guardar en biblioteca» con un valor sin aplicar).
- UI, `tests/RackCad.UI.Tests/Workspace/RackCadCamaWindowTests.cs` (nuevo, 12), con una puerta doble de la cama: la oferta solo con esa
  puerta y la respuesta solo de su petición; «Actualizar» con una copia, la orden que la tomó, la respuesta no escrita y la escrita, D-08
  y la marca de la lista; «Dejar de esperar» y la respuesta tardía; el borrador de otro dibujo y la poda; «Cerrar el editor»; la cama leída
  con un paso no válido; la cama nueva de «Nuevo» (borrador, veto por su etiqueta, sin colocar, colocada con su nombre y el mismo editor
  al abrirla de la lista); «Duplicar» y «Descartar el borrador»; la biblioteca (guardar con los campos desconocidos y abrir como borrador,
  que viaja con su documento); el BOM del borrador; y con WPF real (STA): la página del rack con su recuadro y el editor, «Nuevo» con su
  botón y el editor de la cama nueva, y el editor con sus dos pestañas, el esquema, ningún texto con el nombre de un tipo (F-1), nombres
  de automatización únicos, un campo que se confirma al perder el foco y «Restaurar layout…» en línea.
- UI, `FlowBedEditorWindowTests` (+1): el clásico conserva «Freno cada N» desde el dibujo y desde la biblioteca, y una cama nueva no lo tiene.

**Comprobación en rojo:** diecinueve piezas rotas a propósito, una a una, con script (`hs65\t5\cm\red.py` en el scratchpad de la sesión;
salida en `red-out-1.txt` y `red-out-2.txt`). Antes de cada rotura, el script guarda una copia del original fuera del worktree
(`hs65\t5\cm\red-backup\`); tras cada una compila y corre las clases focales (Core: `CamaDraftRulesTests` y `CamaDoorTests`; UI:
`CamaEditorTests`, `RackCadCamaWindowTests` y `FlowBedEditorWindowTests`) y devuelve el archivo a sus bytes (SHA-256 comprobado). Las
diecinueve hicieron fallar al menos una prueba:
- C1, el documento escribe «Freno cada N» también vacío; C2, el paso no se comprueba entero; C3, de Pushback a Dinámica sin los 48 in;
  C4, la puerta guarda la cama de la ventana y no su copia; C5, la mitad de dibujo sin el nombre del bloque; C6, RACKCAMAABRIR con marca
  de Deshacer;
- U1, un valor rechazado no marca el editor (fallan 3); U2, el veto del cierre sin las camas (fallan 2); U3, la cama sin borrador de
  «Nuevo» (fallan 5); U4, la cama nueva colocada sigue nueva; U5, la página del rack no aloja el editor; U6, el BOM no ve el borrador; U7,
  la biblioteca abre la cama en el clásico; U8, «Restaurar layout…» ignora sus casillas; U9, el clásico pierde «Freno cada N»; U10, una
  cama leída con un problema en «Avanzado» no lo abre; U11, «Dejar de esperar» no devuelve la cama; U12, un valor rechazado no se queda en
  su campo; U13, un valor rechazado en la tarima de una Pushback detiene el guardado.

Después se buscaron restos en `git diff` (ningún `if (false`) y se recompiló la solución con `--no-incremental` antes de las suites
completas. Los tres censos de arriba fallaron con la primera Core focal, que es su rojo.

**Los números** (fuentes finales, solución en Debug recompilada con `--no-incremental`):
- **UI completa:** 2986 correctas, 0 con error, 48 omitidas (3034, 32 min): la base `041e1799` (2961 / 0 / 48) más las 25 nuevas (12 + 12 + 1).
- **Core Full:** 19322 correctas, 0 con error, 403 omitidas (19725, 32 min): la base (19292 / 0 / 403) más las 30 nuevas (21 + 9); los censos cambiados no añaden casos.
- **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias son las MSB3277 de las referencias de AutoCAD y las de los
  analizadores de xunit, que ya estaban.
- Focales: Core de la cama 30/30; guardas y censos de Core que leen la ventana, la puerta, el Plugin y la persistencia 565/0/1; UI de la
  cama y del clásico 30/30; UI de la ventana, los borradores, las utilidades, el BOM, la cabecera, el panel de vistas y los censos de
  ventanas y de la ayuda 255/0/4.

**Lo que queda abierto:**
- **El smoke en AutoCAD:** que `RACKCAMAABRIR` no deje nada en Deshacer; `RACKCAMAAPLICAR` con el bloque y sus copias, y su «U»;
  `RACKCAMANUEVO` con el jig, el nombre automático y la cancelación de la colocación; el editor y el esquema en la paleta (anchos, foco,
  ratón y la escala con un riel largo); la pestaña «Propiedades» escrita tras el redibujo.
- RACKEDITAR de una cama sigue con el editor clásico (D-03) hasta que el clásico se retire al final de I-64; QUICKCAMA no cambia.
- La pregunta 5 dejaba abierto si hace falta «Insertar otra copia…»: no se construyó (CM-S3).
- Para el Owner: CM-S1 a CM-S15.
- **Para integrar:** `RackCadWindowViewModel.cs`, `RackCadWindowViewModel.Drafts.cs`, `RackCadWindowView.xaml(.cs)`, `RackCadRackDoorPort.cs`
  y los tres censos los tocan también las otras partes de la tanda: cada línea de esta parte lleva `T5 cm`. Los números de los censos de
  órdenes se suman con los de las otras partes (esta añade 3).

### Quinta tanda, cm: correcciones de la revisión (sobre `bddc2203`)

En `D:\w5cm`, sobre el commit `bddc2203` de la sesión con esta parte. Sin commit, sin push, sin AutoCAD. Los siete puntos de la revisión
independiente, cada uno con su prueba; los puntos 1 a 6, además, con su comprobación en rojo.

| Punto | Qué pasaba | Qué se hizo | Prueba |
|---|---|---|---|
| 1 | La poda de la cama decía su propia línea y la poda que corría después la pisaba: al cerrarse un dibujo con una cama y un Dinámico modificados, solo se nombraba el Dinámico | Las podas ya no dicen nada: cada una da los nombres de lo que pierde a `LostOnPrune` (nuevo, `RackCadWindowViewModel.Prune.cs`), y `PruneDeadSessionsAndSayLost`, lo que llama `Present`, dice UNA línea con todos al terminar `PruneDeadSessions` (ordenados como los nombra el veto del cierre). Usan el punto de entrada las podas de Cabecera, Cama, «Variables» (como «Variables») y la de Dinámico y Selectivo | `RackCadCamaWindowTests.AClosedDrawingNamesEveryDraftItLostInOneLine` |
| 2 | Pushback → Dinámica → Pushback dejaba la cama «Modificado» (la tarima se quedaba en los 48 in de la pregunta 8) y vetaba el cierre | El borrador recuerda la tarima de antes del relleno y la devuelve al volver a Pushback si nadie escribió otra; escribirla, o «Descartar», olvida ese recuerdo | `CamaDraftRulesTests.PushbackDinamicaPushbackGivesThePalletBackWhenNobodyWroteIt`, `CamaEditorTests.PushbackDinamicaPushbackWithNothingWrittenIsLimpioAgain`, `RackCadCamaWindowTests.PushbackDinamicaPushbackDoesNotVetoTheClose`; y `APushbackWithNoPallet…` cambia con nota |
| 3 | «Descartar» estaba apagado con solo un valor rechazado (el borrador sin cambios) | `CanDiscard => (_dirty \|\| _refused.Count > 0) && CanEdit` | `CamaEditorTests.DescartarIsOfferedWithOnlyAValueRefusedAndTakesItAway` |
| 4 | El BOM del borrador de la página «BOM y lista de racks» contaba con el último valor válido aunque hubiera un valor marcado (contra CM-S5) | Con marcas no hay BOM del borrador (`CamaEditorViewModel.DraftBom()` devuelve null) y la página muestra el del dibujo | en `RackCadCamaWindowTests.ThePageOfTheBomShowsTheBomOfTheDraftOfACamaWithChanges` |
| 5 | Faltaba la guarda de la vista de EditCama (`RackCommandSupport.DecodeView(embed).HasAddress`) | `CamaDoorRules.AnswerForOpen` y `PlanApply` reciben la vista y la sección del sobre y comprueban `RackViewCodec.Decode(kind, view, section).HasAddress` después del tipo y antes del diseño, con el texto del clásico: «Descriptor de vista de cama inválido.» (en el plan, «… No se escribió nada.»). Las órdenes les pasan `embed?.View` y `embed?.Section ?? -1`, como `DecodeView`. **Hallazgo:** con el tipo «cama» el códec de hoy siempre da una dirección (coaccionada, «CamaDescriptorIgnored», cuando el descriptor no es el canónico), así que la guarda es paridad con EditCama y defensa ante un cambio del códec; su rechazo se prueba con la sobrecarga interna que recibe la vista ya decodificada | `CamaDoorTests.AViewThatDoesNotDecodeIsRefusedAsEditCamaRefusesItAndTheDescriptorsOfACamaDecode` |
| 6 | Un rechazo viejo de la tarima o del freno (escrito en Dinámica) sobrevivía a una escritura en Pushback y volvía, marcado, al pasar otra vez a Dinámica | Se olvida al pasar a Pushback y al escribir (`ForgetRefusalsThatDoNotApply`) | `CamaEditorTests.AnOldRefusalOfThePalletDoesNotComeBackAfterAWriteInPushback`; `GuardarEnBibliotecaNamesTheFieldOfAValueThatWasNotApplied` cambia con nota (ya no vuelve) |
| 7 | La consecuencia de mezclar builds no estaba escrita | CM-S15 (arriba, junto a CM-S14): un DLL anterior que hace RACKEDITAR conserva `BrakeEveryRollers` como campo desconocido pero dibuja sin él, y el BOM nuevo cuenta los frenos cada N mientras el bloque tiene otros. Para el smoke | — |

**Ediciones de esta corrección:**
- **A archivos compartidos** (marcadas `T5 cm`):
  - `RackCadWindowViewModel.cs`: en `PruneDeadSessions`, el `Say` de Dinámico y Selectivo pasa a `lost.ForEach(LostOnPrune)` (una línea en lugar de dos); en `Present`, `Contained(PruneDeadSessionsAndSayLost)` en lugar de `Contained(PruneDeadSessions)`.
  - `RackCadWindowViewModel.Cabecera.cs` (parte cb): el `Say` de su poda pasa a `lost.ForEach(LostOnPrune)`.
  - `RackCadWindowViewModel.Variables.cs` (parte va): el `Say` de su poda pasa a `LostOnPrune("«Variables»")`. El texto cambia: la línea común («Se cerró el dibujo de «Variables» con cambios sin escribir en la ventana: …») sustituye a «Se cerró un dibujo con cambios sin escribir en «Variables»: …», que ninguna prueba fijaba.
- **Archivo nuevo:** `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Prune.cs`.
- **Archivos de la parte:** `CamaDraftEditor.cs`, `CamaDoor.cs`, `RackCadCamaDoorCommands.cs`, `CamaEditorViewModel.cs`, `RackCadWindowViewModel.Cama.cs` y las pruebas.

**El punto de entrada de las podas, para integrar** (las otras partes de la tanda añaden las suyas): la poda de un sistema va en
`PruneDeadSessions`, en una línea como las demás, y por cada borrador con trabajo que olvida llama `LostOnPrune(nombre)`, con el mismo
nombre que le da el veto del cierre (D-08). Ninguna poda llama `Say`: lo dice `PruneDeadSessionsAndSayLost`, una vez.

**Comprobación en rojo** (`hs65\t5\cm\red2.py`, salida en `red-out-3.txt`; copias fuera del worktree en `red-backup-2\`; cada archivo
vuelto a sus bytes con el mismo SHA-256): ocho piezas, y cada una hace fallar su prueba:
- R1, la poda de la cama dice otra vez su propia línea: falla la de las dos camas del dibujo cerrado;
- R2 y R2b, la tarima no vuelve al pasar a Pushback: fallan dos de Core y dos de UI (la del editor y la del veto);
- R3, «Descartar» exige un cambio del borrador: falla la suya;
- R4, el BOM del borrador ignora las marcas: falla la del BOM;
- R5 y R5b, la lectura o el plan sin la guarda de la vista: falla la suya en cada caso;
- R6, el rechazo viejo no se olvida: fallan dos.

Después se buscaron restos en `git diff` y se recompiló la solución con `--no-incremental`.

**Los números:**
- **UI completa:** 2991 correctas, 0 con error, 48 omitidas (3039, 28 min): las 2986 de `bddc2203` más las 5 nuevas (3 en `CamaEditorTests` y 2 en `RackCadCamaWindowTests`; la del BOM y las dos cambiadas no añaden casos).
- **Core, focales** (las clases de la cama y las guardas y censos de la ventana, la puerta, el Plugin, la persistencia y el códec de vistas): 567 correctas, 0 con error, 1 omitida (las 565 de la entrega más las 2 nuevas de Core: `PushbackDinamicaPushback…` y `AViewThatDoesNotDecode…`). Core Full no se volvió a correr (el encargo pide las focales); por cuenta, 19324 / 0 / 403.
- **Build Debug de la solución (Plugin incluido), `--no-incremental`:** 0 errores.

## Quinta tanda, ct: Cantilever en la ventana (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t5-ct`, sobre `041e1799` (worktree `D:\w5ct`). Sin commit, sin push, sin AutoCAD.
Prototipo `cantilever` (CT-01 a CT-12): el Owner aceptó las doce propuestas sin nota (`owner-decisions-2026-11-26.md`). No se borró código
clásico: RCT, RACKCANTILEVER, RACKEDITAR y la tarjeta «Cantilever» de «Nuevo» (RACKNUEVO) siguen con el editor clásico, como pide el prototipo
(«Cantilever (editor clásico)»).

**Lo que se construyó:**
- **El borrador** (`CantileverDraftEditor`, Application): una copia de `CantileverLineDesign` recalculada con LA autoridad del editor clásico
  (`CantileverLineEditorAssembler`), así que la línea, su BOM y sus vistas son las del clásico; no cambia ningún cálculo.
  - Una sola forma de confirmar: cada campo se comprueba por su regla (número, mínimo) y, si se rechaza, el borrador no cambia.
  - Una línea que todavía no se resuelve sí es un borrador válido (la nueva abre «bloqueada», como en el clásico): no se escribe y
    **«Para que la línea se resuelva»** (CT-05) dice qué falta y dónde, con «Ir a …»: la sección de columna, la de base, la longitud de base, la
    sección y el corte del brazo por omisión, el claro entre niveles, los brazos de celda sin sección o sin corte, el tensor estructural sin
    sección y la altura o la cantidad manual sin valor; si no falta ninguna de esas piezas y la línea sigue sin resolverse, los diagnósticos
    bloqueantes del resolvedor.
  - Los brazos de las celdas se escriben con la matriz de la línea (`CantileverLineArmMatrix`): una copia por celda, y una excepción igual al
    brazo por omisión no se guarda. «Todo» escribe la excepción en todas las celdas (CT-06); el brazo por omisión no toca las excepciones.
  - «Modificado» = el JSON del borrador difiere de su base (lo leído o lo último escrito; los valores por defecto o la biblioteca en una línea
    nueva). «Actualizar» envía copias: el diseño copiado y la línea resuelta de esa copia (D-05 de Dinámico).
- **La puerta** (`CantileverDoor.cs`, Application; `RackCadCantileverDoorCommands.cs` y `RackCadRackDoorPort.Cantilever.cs`, Plugin), con las
  reglas de las otras puertas (número de petición, otro dibujo, «Dejar de esperar», `RequestStarted`, la comparación de vistas hermanas):
  - `RACKCANTILEVERABRIR` (sin marca de Deshacer ni historial): el diseño de la línea y las propiedades del rack;
  - `RACKCANTILEVERAPLICAR`: la mitad de dibujo de «Actualizar» de RACKEDITAR y después la pestaña «Propiedades»; un «U»;
  - `RACKCANTILEVERINSERTAR`: «Insertar» de RACKEDITAR (el borrador en las vistas que hay y después las nuevas, decisión 10 de Dinámico);
  - `RACKCANTILEVERNUEVO`: una línea nueva de la ventana por el despacho de «Nuevo», con el nombre automático de I-60.
- **La mitad de dibujo de RACKEDITAR, extraída** (`RackCantileverCommands.ApplyCantileverEdit`): todo lo que `EditCantilever` hacía después de
  cerrar su ventana, con el resultado del editor como argumento (`ICantileverEditOutcome`, que implementan la ventana clásica y
  `CantileverDraftOutcome`), sin cambiar su cuerpo, como `ApplyDynamicEdit`. Devuelve cuántas vistas redibujó y cuántas laterales obsoletas
  borró; RACKEDITAR no mira la respuesta.
- **B-1 en Cantilever (CT-11):** `RackViewBatchProducts.Cantilever` prepara la vista nueva desde `RackInsertedViewSource.Edited(…)`, la regla de
  Dinámico. Corrige también «Insertar» de RACKEDITAR con cambios.
- **El editor de la ventana** (`CantileverEditorViewModel` y sus parciales `.Arms`, `.Bracing` y `.Window`; `CantileverEditorView`), sin modales:
  - **Cabecera:** nombre, estado (Limpio, Modificado, Escribiendo; «Nueva, sin insertar»), «Actualizar» (o «Insertar en el dibujo…» de una
    línea nueva), «Insertar vista…», «Descartar», «Restaurar layout…», «Cerrar el editor» con su pregunta en línea, «Más» › «Guardar en
    biblioteca…», el motivo de un «Actualizar» apagado y la lista de lo que falta.
  - **Línea:** «Lo esencial» (Estructura; Caras y altura, con los campos dormidos desactivados y su motivo, dolor 7; Piezas mínimas), «Calculado»
    (altura común, intervalos, paneles por intervalo, separadores, tensores, brazos) y, plegados, «Niveles» (troquel del primer nivel contado
    desde 1, CT-09; un claro para la línea, CT-12; factor superior) y «Dibujo» (brazos y tensores en la planta).
  - **Brazos** (la pestaña inicial): la matriz compacta estación × nivel (× lado), con la longitud de corte en cada celda (CT-01), «E» de
    excepción, «T» de placa con tope y «•» de cambiada en el borrador; «Ver detalle en celdas» añade la sección. Clic, Ctrl+clic, Mayús+clic,
    «E1…», «N1…» y «Todo»; los segmentos Celda · Estación · Nivel · Lado · Todo ensanchan la selección. El inspector edita el brazo de la
    selección (sección, arreglo, corte, pendiente, placa de montaje y de extremo) y ofrece «Volver al brazo por omisión». Los lados se llaman
    «+Y» y «−Y» (CT-02).
  - **Componentes** (CT-03): Columna y base · Brazo por omisión · Separador · Tensor, como paneles, sin Aceptar, Cancelar ni Restaurar propios,
    con su «Receta» y su «Diagnóstico». La sección se busca en una lista dentro de la página (CT-04), con la designación del catálogo
    («HSS4X4X1/4», «C4X4.5», dolor 6) y el id en la ayuda; la familia la fija la pieza (W, C, C o L; cualquiera para el brazo, C si es apareado).
  - **Arriostramiento** (CT-10): cantidad automática o manual, alturas, lo derivado, la secuencia de un intervalo (de arriba abajo) y, en
    «Avanzada», la tabla de tramos (Desde, Hasta, Alto, Con tensores, Dividir, Quitar, Agregar tramo arriba), que es un borrador hasta «Aplicar
    secuencia»: mientras tanto «Actualizar» se niega y dice por qué. Volver a «Automática» pregunta en línea; volver a «Avanzada» ofrece
    «Recuperar mis tramos» o «Empezar desde la regla» (dolor 17).
  - **Propiedades** (VA-07), al final: la tabla de la pestaña de los otros editores; es parte del borrador y solo «Actualizar» la escribe.
- **La ventana** (`RackCadWindowViewModel.Cantilever.cs`, `RackCadWindowView.Cantilever.cs`): «Editar aquí» de un Cantilever de la lista
  (dolor 19), «Cantilever en esta ventana» de «Nuevo» como «Borrador N» (los seis métodos de los borradores), «Insertar vista…» con el panel
  reutilizable (Frontal · Lateral por estación · Planta, DG-07), «Abrir» de la biblioteca como borrador (UT-03), «Guardar en biblioteca…»
  (UT-04), el BOM del borrador en «BOM y lista de racks» (BM-02: el BOM del clásico sobre una copia), el veto de cierre (D-08) y la poda de un
  dibujo cerrado.

**Las decisiones y los supuestos** (prefijo CT5; cada uno en un sitio):

| Id | Decisión o supuesto | Dónde se cambia |
|---|---|---|
| CT5-01 | **Supuesto.** La línea nueva abre con los valores del clásico (pregunta 5, aceptada). El clásico la abre con un claro de 0 in, que la estación rechaza («debe ser un numero positivo»): la lista de lo que falta lo nombra, y «Ir a «Línea» › «Niveles»» abre el grupo. Con un nivel y 24 in de claro la columna queda más corta que el arriostramiento y el resolvedor lo dice. No se cambió ningún cálculo | `CantileverDraftEditor.Missing` |
| CT5-02 | Un valor que su campo rechaza no llega al borrador; una línea que no se resuelve sí es un borrador (no se escribe, y la lista lo dice) | `CantileverDraftEditor.Write`, `Edit` |
| CT5-03 | **Diferencia declarada con el clásico.** Un factor de claro superior menor que 1/3 se rechaza en su campo (el clásico lo dejaba escribir y la línea quedaba bloqueada) | `CantileverDraftEditor.Write` |
| CT5-04 | Las opciones son segmentos y la sección se elige en una lista de la página (como mucho 40 filas; se escribe para acotar), sin desplegables | `CantileverChoiceItem`, `CantileverSectionPickerItem` |
| CT5-05 | «Aplicar secuencia» comprueba cada fila (números, «Hasta» sobre «Desde», al menos un tramo); si los tramos son un arriostramiento válido lo decide el resolvedor al recalcular, como en el clásico, y la lista lo dice | `CantileverDraftEditor.ApplySegments` |
| CT5-06 | «Restaurar layout…» vuelve a la regla y conserva los valores como datos dormidos (la altura manual, la cantidad manual, los tramos), como los conserva el clásico | `CantileverDraftEditor.Restore` |
| CT5-07 | **Corregido en la revisión (punto 4).** La regla de RACKEDITAR (`LoadExisting(…, embed.Name, …)`): el editor muestra y escribe el nombre del sobre; el nombre interior del diseño no se toca, y a un diseño sin nombre no se le escribe uno. Renombrar escribe los dos, como `Name_Changed` del clásico. Un diseño de la biblioteca toma el nombre de su entrada, como `LoadDesignForNew` | `CantileverDraftEditor.Name`, `Load`, `SetName`; `RackCadWindowViewModel.OnCantileverRead` |
| CT5-08 | **No construido (pregunta 8):** «Insertar pieza suelta…» por RACKCANTILEVERPIEZA. El panel lo dice; la pieza suelta sigue en los configuradores del clásico (RCT). Reproducir las vistas de cada configurador (el brazo a 96 in y en el troquel 0, dolor 14) es una entrega propia | `CantileverEditorViewModel.PieceNote` |
| CT5-09 | `ApplyCantileverEdit` es una extracción, no una copia (a diferencia de CB4-09): RACKEDITAR la llama con su ventana y su cuerpo no cambió | `RackCantileverCommands.cs` |
| CT5-10 | Dolor 12, declarado como M-28 de Dinámico: cada vista se redibuja en su transacción y una que falla no detiene a las demás; toda la orden es un «U» | `ApplyCantileverEdit` |
| CT5-11 | La selección abre en E1 · N1. El inspector va al lado de la matriz desde 900 px y, por debajo, debajo | `SelectFirstCell`, `CantileverEditorView.InspectorBesideWidth` |
| CT5-12 | **Diferencia con el prototipo.** La matriz se desplaza entera, con sus rótulos, en lugar de dejarlos fijos | `CantileverEditorView.xaml`, `MatrixScroll` |
| CT5-13 | «Guardar en biblioteca…» exige una línea resuelta (como el clásico) y guarda `RackProject.ForCantilever` del borrador con el nombre escrito y los metadatos de su origen (I-11) | `RackCadWindowViewModel.BuildCantileverProject` |
| CT5-14 | El almacén de proyectos no lee una línea sin secciones (`RackDesignValidation.IsUsableCantilever`): la base del borrador se guarda también como diseño, y «Descartar» y «Duplicar» de una línea nueva no pasan por el JSON | `CantileverDraftEditor.BaselineDesign` |

**Ediciones a archivos compartidos** (pequeñas y marcadas `T5 ct`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `DoorRequest.IsCantilever`; el constructor (`WireCantileverDoor(door)`); `ShowPanel` y
  `HasRackEditor`; una rama en `StopWaiting`, `OnDesignWritten`, `OnRequestStarted` y `DraftStateOf`; una condición en `OnDesignRead`; una
  línea en `UnwrittenWorkOf`, `PruneDeadSessions`, `RaiseDynamic` y `RaiseNewDynamic`.
- `RackCadWindowViewModel.Drafts.cs`: un caso en cada uno de los seis métodos por sistema.
- `RackCadWindowViewModel.Ins.cs`: una línea en `RefreshInsertPanels`.
- `RackCadWindowViewModel.Bom.cs`: una línea en `DraftBomOf`, `HasUnwrittenDraft` y `UnwrittenDraftsOf`.
- `RackCadWindowViewModel.Utilities.cs`: `OpensHere`, el editor de `OpenDesignAsNewRack` e `IsEditorAlive`.
- `RackCadDrafts.cs`: el nombre de un proyecto Cantilever en `NameOf`.
- `RackCadWindowView.xaml`: tres bloques `<!-- T5 ct (n of 3) -->`: la oferta «Editar aquí» en la página del rack, el anfitrión del borrador en
  la rejilla de páginas y el botón «Cantilever en esta ventana» de «Nuevo». No caben en uno: están en tres contenedores distintos, como los de
  Dinámico y Selectivo.
- `RackCadWindowView.xaml.cs`: dos nombres en `OnViewModelPropertyChanged`, una línea para el borrador nuevo y una en `ShowRackContent`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: una línea en `Abandon`.
- `src/RackCad.Plugin/RackCantileverCommands.cs`: la extracción de `ApplyCantileverEdit` (CT5-09).
- `src/RackCad.Plugin/Views/RackViewBatchProducts.cs`: B-1 (CT-11).
- `src/RackCad.UI/Systems/Cantilever/RackCantileverWindow.xaml.cs`: la clase implementa `ICantileverEditOutcome` (una palabra).
- Archivos nuevos: `CantileverDraftEditor.cs` y `CantileverDoor.cs` (Application); `CantileverEditorItems.cs`, `CantileverEditorViewModel.cs`,
  `.Arms.cs`, `.Bracing.cs`, `.Window.cs`, `CantileverEditorView.xaml(.cs)`, `RackCadWindowViewModel.Cantilever.cs`,
  `RackCadWindowView.Cantilever.cs` y `Systems/Cantilever/ICantileverEditOutcome.cs` (UI); `RackCadCantileverDoorCommands.cs` y
  `RackCadRackDoorPort.Cantilever.cs` (Plugin); y las dos clases de prueba.

**Censos y guardas que cambian, con su nota:**
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 57 a 61.
- `CustomPropertiesCommandGuardTests`: `A14CantileverDoorCommands` en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `RackCadCantileverDoorCommands.cs` es un archivo de órdenes de la puerta, y
  `ApplyCantileverEdit` entra en la lista de lo que ningún otro archivo de la puerta puede llamar.
- Tres guardas leían el cuerpo de `EditCantilever`, que ahora delega en su mitad de dibujo, y leen también `ApplyCantileverEdit`, cada una con
  su nota: `CantileverPluginSourceGuardTests.EditBody` (los dos cuerpos, en su orden), `I60RackNameWiringGuardTests` (un caso más) y
  `RackEditPreflightCharacterizationTests` (el preflight de Cantilever). Su rojo: la primera pasada de las guardas de Core dio 5 fallos en
  `CantileverPluginSourceGuardTests`, que esta nota corrige.

**Las pruebas:**
- `tests/RackCad.Tests/Workspace/CantileverDraftEditorTests.cs` (Core, nueva, 31 casos):
  - la línea nueva es la del clásico y dice qué le falta y dónde; «Descartar» de una línea nueva vuelve a sus valores (CT5-14);
  - «Lo esencial» da el mismo diseño, la misma línea y el mismo BOM que el clásico con los mismos valores;
  - trece valores que su campo rechaza y no cambian el borrador;
  - los campos dormidos y su modo; el troquel contado desde 1;
  - los brazos de la selección, la excepción igual al brazo por omisión que no se guarda, «Varía», «Todo» (CT-06) y «Volver al brazo por
    omisión»;
  - el arreglo apareado, que exige canal en los dos sentidos; la placa de extremo; la base que sigue a la columna; el tensor estructural;
  - la secuencia avanzada: materializar, aplicar, volver a la regla y recuperar los tramos;
  - «Restaurar layout…» con sus cuatro casillas;
  - la copia para escribir, que no sigue al borrador; lo escrito como base; el nombre;
  - las reglas de la puerta;
  - por la fuente: las cuatro órdenes (sin marca de Deshacer al leer, hermanas antes de `blocks[0]`, la respuesta por la regla), la extracción
    de la mitad de dibujo y B-1, con la regla sobre un sobre de Cantilever.
- `tests/RackCad.UI.Tests/Workspace/RackCadCantileverWindowTests.cs` (UI, nueva, 20):
  - **la puerta:** la oferta solo con su puerta y la respuesta solo de su petición; «Actualizar» con copias, el editor bloqueado, la orden que
    la tomó, la respuesta no escrita y la escrita (D-08); «Dejar de esperar» y la respuesta tardía; el borrador de otro dibujo y la poda;
    «Cerrar el editor» con su pregunta;
  - **el editor:** el valor rechazado que se queda en su campo; la matriz (clic, Ctrl, Mayús, cabeceras, «Todo», el alcance, «E», «•»), el
    inspector y el brazo por omisión; la doble cara y el alcance «Lado»; los componentes, la lista de secciones por familia y «Ir a …»; la tabla
    de tramos, que bloquea «Actualizar» hasta aplicarse, y sus preguntas; «Restaurar layout…»;
  - **la ventana:** la línea nueva como «Borrador N», con su lista de lo que falta, insertada como rack nuevo con las vistas elegidas y el
    nombre de I-60; «Insertar vista…» con las estaciones; el BOM del borrador y «Guardar en biblioteca…»; un diseño de la biblioteca como
    borrador; «Propiedades» leída, editada y enviada por «Actualizar»; «Duplicar»;
  - **con WPF real (STA), sin errores de enlace:**
    - las cinco pestañas con «Brazos» primero;
    - el clic en una celda, un segmento y la lista de secciones;
    - nombres de automatización únicos;
    - la línea nueva en estrecho, con el inspector debajo e «Ir a …»;
    - la página del rack y «Nuevo», que alojan el editor.
- **Comprobación en rojo:** catorce piezas rotas a propósito, una a una, con script (`S\hs65\t5\ct\red.py`; salidas en `red-out-1.txt` y
  `red-log-r9.txt`). Antes de romper cada archivo se guarda una copia fuera del worktree; después vuelve a sus bytes con el mismo SHA-256 (lo
  comprueba el script), y no quedó ningún resto en `git diff`.
  - Las piezas y lo que fallaba con cada una:
    1. un entero sin su mínimo: 4 de Core;
    2. el brazo de una celda escrito en el brazo por omisión: 2;
    3. la copia para escribir con el diseño vivo: 1;
    4. sin el claro en lo que falta: 1 de Core y 2 de UI;
    5. «Descartar» por el JSON de la base: 1;
    6. la lectura de otro número: 1 de UI;
    7. el veto sin las líneas Cantilever: 2;
    8. «Actualizar» con tramos sin aplicar: 1;
    9. la marca «E» de la celda: **sobrevivió**, porque las pruebas solo veían celdas con excepción y cambiadas a la vez. Se añadió el brazo
       por omisión cambiado («•» sin «E»), y con eso falla 1;
    10. la puerta que abre cualquier tipo: 1;
    11. B-1 deshecho: 1;
    12. RACKCANTILEVERABRIR con marca de Deshacer: 1;
    13. el segmento de una opción que no escribe: 1;
    14. «Actualizar» que envía el diseño vivo: 1.
  - Después se recompiló la solución con `--no-incremental`.

**Los números** (la solución en Debug, recompilada con `--no-incremental` sobre las fuentes finales):
- **UI completa:** 2981 correctas, 0 con error y 48 omitidas (3029): la base, 2961/0/48, más las 20 nuevas.
- **Core Full:** 19324 correctas, 0 con error y 403 omitidas (19727): la base, 19292/0/403, más las 31 de `CantileverDraftEditorTests` y el caso
  nuevo de `I60RackNameWiringGuardTests`.
- **Plugin Debug:** 0 errores; las 2 advertencias son las MSB3277 de las referencias de AutoCAD, que ya estaban.

**Lo que queda abierto:**
- **No construido:**
  - «Insertar pieza suelta…» (RACKCANTILEVERPIEZA, pregunta 8, CT5-08);
  - las cotas de nivel en «Calculado» y el diagrama dibujado de la secuencia, que hoy es una lista de tramos (el prototipo los dejaba
    [PENDIENTE]);
  - «Abrir en editor clásico» en la página del rack, que la ventana no tiene para ningún sistema (UT-S12).
- **El smoke del Owner en AutoCAD:**
  - que RACKCANTILEVERABRIR no deje paso en Deshacer;
  - un solo «U» para RACKCANTILEVERAPLICAR;
  - RACKCANTILEVERINSERTAR y RACKCANTILEVERNUEVO con el jig, el orden elegido y el nombre de I-60;
  - B-1: una vista insertada con cambios que se vuelve a abrir igual que sus hermanas;
  - que RACKEDITAR siga igual tras la extracción;
  - el editor en la paleta: anchos, foco, ratón en la matriz y la lista de secciones.
- **Para el Owner:** CT5-01, CT5-03, CT5-05 y CT5-12.
- **Para integrar:**
  - las otras partes también tocan `RackCadWindowViewModel.cs`, `.Drafts.cs`, `RackCadWindowView.xaml(.cs)` y `RackCadRackDoorPort.cs`; las
    ediciones de esta parte son las de la lista de arriba;
  - el censo de órdenes suma las 4 de esta parte a las de las demás.

### Quinta tanda, ct: correcciones de la revisión

Sobre `44c0a439` (el commit de la sesión con esta parte), en `D:\w5ct`. Sin commit, sin push y sin AutoCAD. Son los seis puntos de la
revisión independiente; cada uno tiene su prueba, y los puntos 1 a 5 también su comprobación en rojo.

| Punto | Qué se hizo | Prueba |
|---|---|---|
| 1 [MEDIA-ALTA] | «Aplicar secuencia» redondeaba cada cota a 4 decimales. Con un claro de 20 in (columna de 106.666… in), «Empezar desde la regla», un cambio de «Con tensores» y «Aplicar», el último tramo pasaba la punta por 1e-9 y la línea quedaba bloqueada. Ahora cada fila guarda sus cotas exactas (`CantileverSegmentRow.FromValue`, `ToValue`, desde `RowsOf`) y `CantileverDraftEditor.Keep` las usa mientras el texto siga siendo `Num(original)`, como `Keep` del clásico. «Dividir» y «Agregar tramo arriba» parten de las cotas exactas | Core: `ApplyingTheSequenceOfTheRuleKeepsItsExactElevationsAndTheLineStillResolves`. UI: `ApplyingTheSequenceOfTheRuleInTheTableKeepsTheLineResolved`, también con «Dividir» |
| 2 [MEDIA-BAJA] | El factor de claro superior rechazaba «0.3333», el valor que su propio mensaje muestra. Ahora un texto cuyo redondeo de pantalla es `Num(1/3)` guarda 1/3 exacto; «0.3332» se sigue rechazando | Core: `TheTopClearFactorAcceptsTheThirdItShowsAndStoresItExact` |
| 3 [BAJA] | Había dos «Espesor» en el inspector y dos en el brazo por omisión, y dos «Tipo» en «Componentes» (la placa de extremo y el tensor). Ahora: «Placa de montaje: espesor», «Placa de extremo: espesor», «Placa de extremo: tipo» y «Tipo de tensor» | UI: `EveryFieldAndButtonOfEachTabAndPanelHasAUniqueName`. Recorre cada pestaña y cada panel de «Componentes» con los expansores abiertos, y mira cada campo, casilla, lista y botón visibles (menos la pestaña «Propiedades», que es la vista compartida de `va`) |
| 4 [BAJA] | CT5-07 corregido con la regla del clásico. El editor muestra y escribe el nombre del sobre (`LoadExisting(…, embed.Name, …)`) y no toca el nombre interior: a un diseño sin nombre no se le escribe uno. Renombrar escribe los dos, como `Name_Changed`. «Modificado» cuenta también el nombre, y «Descartar» lo devuelve. El supuesto CT5-07 queda al día en su tabla | Core: `TheNameIsTheOneOfTheEnvelopeAndTheInnerNameIsWrittenOnlyByARename`. UI: `TheEditorShowsAndWritesTheNameOfTheEnvelopeAndLeavesTheInnerNameAlone` |
| 5 [BAJA] | «Duplicar» conservaba `Line.Id`. Ahora la copia se hace con `DuplicateWithNewIdentity()`, y su base toma la misma identidad nueva y queda sin nombre. Así la copia está «Modificado» exactamente cuando lo estaba su original | UI: `ADuplicateOfACantileverDraftIsAnotherDraftWithTheSameDesign`, ampliada: identidad nueva, y la copia de un borrador sin cambios (un diseño de la biblioteca) sale limpia |
| 6 [COSMÉTICO] | «Guardar en biblioteca…» de «Más» quedaba centrado. Ahora usa la plantilla de fila de la vista (`RowButton`, alineado a la izquierda y con borde). Dinámico tiene lo mismo y no se tocó | La prueba de vista de siempre (nombres y enlaces) |

**Comprobación en rojo** (`S\hs65\t5\ct\red.py`, piezas RV1 a RV5; salida en `red-log-rv.txt`): cinco piezas rotas a propósito, una a una,
con una copia de cada original fuera del worktree. En todas falla al menos una prueba:
- RV1, la cota que se guarda redondeada a 4 decimales: fallan 1 de Core y 1 de UI;
- RV2, «0.3333» rechazado: falla 1 de Core;
- RV3, los dos espesores del inspector con el mismo nombre: falla la de unicidad;
- RV4, el nombre interior antes que el del sobre: falla 1 de UI;
- RV5, la copia sin identidad nueva: falla 1 de UI.

Cada archivo volvió a sus bytes (SHA-256 comprobado por el script), y no quedaron restos en `git diff`. Después se recompiló la solución con
`--no-incremental`.

**Archivos de las correcciones:**
- `CantileverDraftEditor.cs` (Application);
- en la UI: `CantileverEditorItems.cs`, `CantileverEditorViewModel.cs`, `.Arms.cs` y `.Bracing.cs`, `CantileverEditorView.xaml` y
  `RackCadWindowViewModel.Cantilever.cs`;
- las dos clases de prueba.

Ningún archivo compartido cambia en esta vuelta.

**Los números:**
- **Clases focales de Core** (`CantileverDraftEditorTests`, que pasa de 31 a 34, y las guardas y censos que tocan esta parte):
  217 de 217. Core Full no se volvió a correr (el encargo pide las focales): por cuenta, 19327 / 0 / 403 (19324 más las 3 nuevas).
- **UI completa:** 2984 correctas, 0 con error y 48 omitidas (3032): las 2981 de la entrega más las 3 nuevas. `RackCadCantileverWindowTests` pasa de 20 a 23.
- **Build Debug de la solución (Plugin incluido),** con `--no-incremental`: 0 errores. Las advertencias son las MSB3277 de las referencias de
  AutoCAD y las de los analizadores de xunit, que ya estaban.

## Quinta tanda, pb: Push Back en la ventana (2026-11-26)

**Rama:** local `architecture/workspace-persistente-rackcad-t5-pb`, sobre `041e1799` (worktree `D:\w5pb`). Sin commit, sin push, sin AutoCAD.
Prototipo `pushback` (las doce preguntas aceptadas sin nota) y, para la seguridad, SG-02 (nota del Owner: Push Back = «Entrada/Salida /
Posterior») y SG-09. No se borró código clásico: RACKPUSHBACK, RPB, RACKEDITAR y el «Editar todo» del panel del piloto siguen igual.

**Lo que se construyó:**
- **El borrador** (`PushBackDraftEditor`, Application, `VentanaRackCad/PushBackDraftEditor.cs`). No añade modelo: el estado es el
  `PushBackEditableState` del borrador completo del piloto (los dos lados, la intención compuesta, la cola dormida, la autoridad de módulos
  y los valores del rack) y cada recomposición es la pasada de la ventana clásica como código de producto (`PushBackEditableStateSeat.Pass`).
  - Un rack nuevo es la secuencia de la ventana clásica al abrirse (LoadNew, pasada, seguridad por defecto, pasada); uno leído, la de
    RACKEDITAR (`LoadExisting`). Una prueba de Core ata las dos con el comparador del estado, sobre todo el corpus de `Fixtures/I64`.
  - Un diseño abierto y no tocado se escribe como se leyó (los canónicos, byte a byte); una recomposición sin cambios no lo cambia.
  - Los gestos del inspector escriben las celdas y los frentes de la selección en los lados que pide el selector (`EditSides`), solo
    donde la celda existe en ese lado; «Lo esencial» escribe los dos lados (`allSides`). Los niveles y los carriles pasan por el estado de
    cada lado (`AdjustLevels`, `AdjustPositions`), así que los peraltes posteriores y los topes siguen al nuevo número.
  - Un gesto que no se recompone se deshace con una copia profunda del estado (`Capture` / `Restore`); el último sistema válido se queda.
  - «Actualizar» recibe copias: el diseño por su JSON y el sistema resuelto otra vez de esa copia (`SnapshotForWrite`).
- **La puerta** (`PushBackDoor.cs` en Application; `RackCadRackDoorPort.PushBack.cs`, `RackCadPushBackDoorCommands.cs` y
  `RackPushBackWindowDrawing.cs` en el Plugin), con las reglas de las otras: petición numerada guardada ANTES de entregar la orden, el
  dibujo de la petición, «Dejar de esperar», `RequestStarted`, la poda de un dibujo cerrado y la comparación de las vistas hermanas.
  - `RACKPUSHBACKABRIR` (`NoUndoMarker | NoHistory`): el diseño de la primera vista y las propiedades del rack (VA-07). Un Push Back de un
    sentido que guarda la mitad dormida de un lado B (`DormantCompositeTail`) no se abre aquí, como no lo admite el borrador completo del
    piloto: dice «usa RACKEDITAR».
  - `RACKPUSHBACKAPLICAR`: la mitad de dibujo de «Actualizar» de RACKEDITAR con el borrador; un «U» la deshace. Después escribe la pestaña
    «Propiedades». «Escrito» solo si redibujó alguna vista (`RackDoorRules.AnswerForApply`).
  - `RACKPUSHBACKINSERTAR` («Insertar vista…»): primero el borrador en las vistas que hay, después las nuevas, enlazadas, con el lote de ID18.
  - `RACKPUSHBACKNUEVO` («Insertar en el dibujo…» de un borrador): el despacho de «Nuevo» (`DispatchInsertion`) con la identidad de la
    ventana y el nombre automático de I-60 (`PushBackDoorInsertion.NewRackRequest`).
- **El editor** (`PushBackEditorViewModel` y sus parciales `.Ins.cs` y `.More.cs`; `PushBackEditorView.xaml(.cs)`), con las pestañas Rack,
  Matriz (la inicial), Extremos y seguridad, Avanzado y Propiedades, y la cabecera Limpio / Modificado / Escribiendo con «Actualizar»,
  «Insertar vista…», «Descartar», «Restaurar layout…», «Cerrar el editor» y «Más» › «Guardar en biblioteca…».
  - **Rack:** «Lo esencial» del rack entero (profundidad, ancho, alto y peso de tarima, frentes, carriles por frente, niveles, posiciones de
    fondo, alto del 1er nivel, claro sobre la tarima, tolerancia) con «Varía»; «Dos sentidos (lado B)» (la casilla, el hueco y el separador
    central con su aviso); «Calculado»; «Poste y anotaciones», plegado (poste, cotas, unidad del peso, peralte de poste, escala, las tres
    casillas, «Mostrar cotas en» y «Tarimas de referencia» con «En todas» / «En ninguna»).
  - **Matriz:** frente × nivel con la tarima en la celda, «T» (tope posterior), «M» (largo o posiciones de fondo propios) y, con lado B,
    «A» / «B» / «C» (las camas) y «varía» donde los lados difieren; clic, Ctrl+clic, Mayús+clic, F1…, N1… y «Todo». Encima, con lado B,
    «Lado A / Lado B / Ambos lados». El inspector: «Tarima», «Fondo de la celda» (posiciones de fondo propias y «Dibujar tarima»), «Tope
    posterior», «Camas en A y B» (con lado B), «Largueros» (plegado: los dos largueros y sus peraltes del catálogo, el peralte posterior y el
    largo manual) y el frente (plegado: «En blanco», carriles, niveles, posiciones de fondo, «Empieza en la posición», alto del 1er nivel).
  - **Extremos y seguridad:** la tarjeta «Tope posterior» (cuántas celdas, «En todas las celdas» / «En ninguna», el tipo y el saque de cada
    lado), «Seguridad por poste» editable con «Variante y medidas» (PB-S03, en su subsección) y «Extremos» con lo que coloca cada familia
    contado como el BOM del borrador.
  - **Avanzado:** «Parámetros del rack» (altura de cabecera, altura del poste derivado, altura del refuerzo, separadores y separación, y
    «Reforzar poste derivado»), «Módulos» con la longitud editable de cada módulo y la cabecera «Calculada» / «Personalizada» de solo
    lectura, y, con lado B, «Estructura longitudinal por lado» (propuesta, efectiva y ajuste manual).
  - **Propiedades:** la pestaña de VA-07, la misma de Selectivo y Dinámico.
- **La ventana** (`RackCadWindowViewModel.PushBack.cs`, `RackCadWindowView.PushBack.cs`):
  - «Editar aquí» en la página de un Push Back de la lista, con su puerta; la página aloja el editor y «Ver el estado y la navegación del
    rack» alterna con el panel del piloto (pregunta 10: la navegación queda en el panel);
  - «Nuevo» › «Push Back en esta ventana»: un «Borrador N» más (SD-06), con «Duplicar», «Descartar el borrador» y «Abrir» de la lista;
  - la Biblioteca abre un Push Back como borrador (`OpenDesignAsNewRack`), y «Guardar en biblioteca…» lo guarda como el clásico
    (`RackProject.ForPushBack`);
  - el BOM de la página `bom` ve el borrador con cambios («Borrador del editor», `PushBackBomBuilder` sobre una copia);
  - D-08: un Push Back con cambios sin escribir, o escribiéndose, veta el cierre del dibujo y la poda lo nombra.

**Las preguntas del prototipo:**

| # | Estado |
|---|---|
| 1. Tope en la matriz | Hecho: «T» en la celda, la casilla del inspector (tres estados: con «varía», un clic lo pone en todas) y «En todas / En ninguna» |
| 2. Dos sentidos | Hecho: «Lado A / Lado B / Ambos lados» solo con lado B, empieza en «Lado A»; «varía» en «Ambos»; un nivel de un solo lado dice «(A)» o «(B)» |
| 3. Camas en A y B | Hecho: una lista con «Encontradas (dos camas)», «Solo A», «Solo B», «Corrida A → B», «Corrida B → A»; la celda lleva A, B o C |
| 4. Vocabulario | Hecho (ver los rótulos de arriba) |
| 5. Campos duplicados | Hecho: no se muestran «Fondos base», «Peralte IN/OUT base», los espejos ni los selectores «Frente» / «Nivel»; sus valores siguen en el modelo |
| 6. «Restaurar layout…» | Hecho: seis casillas marcadas, «Ver lo que se pierde», «Restaurar lo marcado» al borrador |
| 7. Módulos sin «Confirmar» | Hecho: la longitud va al borrador al salir del campo; vacía = la calculada (conserva una cabecera personalizada) |
| 8. Lados de bota y defensa | Hecho: «Entrada/Salida» y «Posterior», por lado, en la tabla y en el inspector (PB-S03) |
| 9. Comprobar con el dibujo | **No construido** (PB-S04) |
| 10. Lo que sobra del piloto | La navegación queda en el panel («Ver el estado y la navegación del rack»); «Instrumentación» a Configuración, **no construido** (PB-S05) |
| 11. Poste derivado por línea y «Reutilizar» | Hecho como dice: de solo lectura en la tabla de módulos; su edición, con el editor de cabeceras |
| 12. «Dibujar tarima» por celda | Hecho: en el inspector, y «Tarimas de referencia» en «Rack» › «Poste y anotaciones» |

**Decisiones y supuestos** (prefijo PB-S, para el Owner):

| Id | Decisión o supuesto | Dónde se cambia |
|---|---|---|
| PB-S01 | El borrador es el estado editable del borrador completo y la pasada de la Seat; la ventana no tiene recomposición propia. Un cambio de cálculo no se pidió ni se hizo: la regla asimétrica de la cama, PB-002 del desviador por poste y las cabeceras como cortes son las de siempre | `PushBackDraftEditor` |
| PB-S02 | La selección de la matriz es del editor (celdas, primaria y ancla), no la de la matriz de cada lado: con «Ambos lados» una celda que solo tiene B se puede elegir, y los gestos se escriben en cada lado donde la celda existe | `PushBackEditorViewModel` (`_selected`) |
| PB-S18 | Junto a PB-S02 (revisión de `01e76902`, punto 4): «Dejar el frente en blanco» y «Activar» deciden **lado por lado**. Con «Ambos lados», un frente en blanco en A y presente en B ofrece «Dejar en blanco» y deja B en blanco. El clásico (`SetFrontActive`) sale antes si el lado activo ya está como se pide, y con «Ambos» el activo es A: su casilla sale marcada y el único gesto es reactivar los dos. El rechazo es el del clásico, palabra por palabra, en cualquier rack: «Al menos un frente del rack debe permanecer activo.» | `PushBackDraftEditor.SetBlank`, `PushBackEditorViewModel.IsPrimaryFrontBlank` |
| PB-S03 | **Hecho** (subsección «PB-S03: la seguridad poste por poste»): la tabla «Seguridad por poste» editable y el inspector «Variante y medidas» de la cuarta tanda, con «Entrada/Salida / Posterior» por lado (I-42) para botas y defensa; lo que escribe es lo del diálogo clásico para la misma elección, comprobado en Core contra una transcripción del clásico | `PushBackSafetyEdit`, `RackCadSafetyPanel.PushBack.cs` |
| PB-S04 | **Falta** la pregunta 9 («Comprobar con el dibujo» y el conflicto con «Conservar mi borrador» / «Descartar y volver a leer»): como Dinámico y Selectivo (M-16), el editor no detecta un cambio del dibujo mientras se edita | — |
| PB-S05 | El panel del piloto de Push Back sigue como está (su campo, «Editar todo», «Instrumentación»): sus guardas de F1-F4B fijan su contenido. Si el piloto tiene un borrador del mismo rack, son dos borradores; el del piloto queda «posiblemente obsoleto» después de un «Actualizar» de la ventana | — |
| PB-S06 | La mitad de dibujo de la puerta repite la de `EditPushBack` paso a paso (mismos servicios y mismo orden), sin extraerla: unas veinte guardas de F4B fijan el cuerpo de ese método. Se unen al retirar el clásico | `RackPushBackWindowDrawing` |
| PB-S07 | La XAML compartida de la ventana no cambia: la oferta «Editar aquí», «Push Back en esta ventana» y el anfitrión del borrador se crean en código, junto a los de los otros sistemas | `RackCadWindowView.PushBack.cs` |
| PB-S08 | «Posiciones de fondo» de «Lo esencial» escribe todos los frentes de los dos lados y no toca el «Fondos base» del rack (pregunta 5: sale de la pantalla sin borrar su valor; el reparto de fondos lo dan los frentes) | `BuildEssentials` |
| PB-S09 | Un hueco negativo y un ajuste de estructura menor que 2 se rechazan en su campo; el clásico los guarda y bloquea la salida. El separador central con hueco 0 sí se guarda y bloquea «Actualizar» con el diagnóstico del clásico | `BuildTwoWayFields`, `BuildStructure` |
| PB-S10 | Un Push Back de un sentido con la cola dormida de un lado B no se abre en la ventana | `PushBackDoorRules.AnswerForOpen` |
| PB-S11 | «Insertar vista…» ofrece los cortes del lado B solo con lado B; la planta, un lateral por poste con corte y los dos cortes de cada lado | `InsertOptions` |
| PB-S12 | «Cerrar el editor» con cambios pregunta «Descartar y cerrar» / «Seguir editando», como Dinámico; el prototipo añadía «Actualizar y cerrar» | `PushBackEditorView.xaml` |
| PB-S13 | Al cambiar un larguero, su peralte se queda si el catálogo lo admite para el nuevo y toma el primero que admite si no, como las listas del clásico | `KeepAllowed` |
| PB-S14 | El nombre que llevan las vistas lo pone `PushBackDoorRules.Named` (el del borrador, o el del sobre si está vacío, como RACKEDITAR): ningún archivo nuevo del Plugin escribe un `Name` | `PushBackDoorRules.Named` |
| PB-S15 | El tipo de tope «Ninguno (no se dibuja)» es una opción de la lista, como en la sección del diálogo clásico | `BuildTopeChoices` |
| PB-S16 | El protector lateral de Push Back conserva «Izquierda / Derecha / Ambos (frente-puente)» de su guía, como el diálogo clásico: «Entrada/Salida / Posterior» nombra caras, y el protector va siempre en «Entrada/Salida» | `RackCadSafetyWords.SideGuard` |
| PB-S17 | Las seis diferencias con el «Aceptar» del clásico de la subsección PB-S03 (una familia por edición; el orden de la lista; la variante que se queda sin nada; los niveles del desviador del lado más alto; volver a «Auto» en una cara de un compuesto, que el clásico pierde; un desviador nuevo sin «Configurar…»). La quinta es un defecto del clásico, para el Owner | `PushBackSafetyEdit` |

**Ediciones a archivos compartidos** (de una línea, marcadas `T5 pb`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: `DoorRequest.IsPushBack`; `WirePushBackDoor(door)` en el constructor;
  `ShowPanel` y `HasRackEditor` cuentan el editor de Push Back; una rama en `StopWaiting`, otra en `OnDesignWritten` y otra en
  `OnRequestStarted`; `OnDesignRead` ignora una petición de Push Back; `UnwrittenWorkOf` y `PruneDeadSessions` llaman a lo suyo; una
  condición en `DraftStateOf`; `RaisePushBack()` al final de `RaiseNewDynamic`.
- `RackCadWindowViewModel.Drafts.cs`: el caso `PushBack` en los seis métodos por sistema.
- `RackCadWindowViewModel.Bom.cs`: una línea en `DraftBomOf`, `HasUnwrittenDraft` y `UnwrittenDraftsOf`.
- `RackCadWindowViewModel.Utilities.cs`: `OpensHere` admite Push Back; `OpenDesignAsNewRack` elige su editor.
- `RackCadWindowViewModel.Ins.cs`: una línea en `RefreshInsertPanels`.
- `RackCadWindowView.xaml.cs`: una línea en el constructor, en `OnViewModelPropertyChanged` y en `ShowRackContent`.
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: una línea en `Abandon`.
- **La XAML de la ventana no cambia** (PB-S07).

**Archivos nuevos:**
- Application: `VentanaRackCad/PushBackDraftEditor.cs` y `VentanaRackCad/PushBackDoor.cs`.
- UI: `PushBackEditorViewModel.cs`, `PushBackEditorViewModel.Ins.cs`, `PushBackEditorViewModel.More.cs`, `PushBackEditorView.xaml(.cs)`,
  `PushBackDoorInsertion.cs`, `RackCadWindowViewModel.PushBack.cs` y `RackCadWindowView.PushBack.cs`, en `VentanaRackCad`.
- Plugin: `VentanaRackCad/RackCadRackDoorPort.PushBack.cs`, `VentanaRackCad/RackCadPushBackDoorCommands.cs` y `RackPushBackWindowDrawing.cs`.
- Pruebas: `tests/RackCad.Tests/Workspace/PushBackDraftEditorTests.cs` y `tests/RackCad.UI.Tests/Workspace/RackCadPushBackEditorTests.cs`.

**Censos y guardas cambiados,** cada uno con su nota en el código:
- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 57 a 61.
- `CustomPropertiesCommandGuardTests`: `A14PushBackDoorCommands` (las cuatro órdenes) en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `RackCadPushBackDoorCommands.cs` es un archivo de órdenes de la puerta,
  y `RackPushBackWindowDrawing` entra en la lista de lo que solo las órdenes alcanzan.
- `PushBackEditAllNameWritersGuardTests.CT_PBT1B_15`: el censo de archivos del Plugin que nombran Push Back pasa de 15 a 19 (las cuatro
  piezas de la puerta); las siete asignaciones de `Name` y su orden no cambian.
- `RackCadNewDraftsTests.OpenAsNewDraftRefusesWhatTheWindowCannotOpenAndSaysWhy`: Push Back ya tiene editor; sin su puerta se rechaza por la
  puerta («Aquí no se puede crear un rack en el dibujo.»).

**Las pruebas:**
- Core, `PushBackDraftEditorTests` (39 casos): la carga igual a la de RACKEDITAR en todo el corpus (canónicos, antiguos y A/B) y sin cambios
  al recomponer; los canónicos byte a byte; el rack nuevo y su relectura; la copia para AutoCAD; las celdas en los lados de la selección
  y «Lo esencial» en los dos; las camas y el tope de una corrida; el tope en el diseño; el fondo propio y la tarima (una propiedad cada
  vez) y el fondo de una corrida; niveles y carriles por el estado de cada lado y el frente en blanco; la longitud de un módulo y su
  vuelta a la calculada; «Restaurar layout» por partes; el recálculo rechazado; el lado B que nace ausente y queda dormido; las reglas de
  la puerta; y, por el texto, las órdenes (sin marca de Deshacer en la lectura, `Started` antes de dibujar, comparación y plan antes de
  la mitad de dibujo), la mitad de dibujo (preflight antes del primer redibujo, `regen: false`, una sola regeneración, sin escritores de
  `Name`) y la puerta (la petición antes de la orden, `Abandon`).
- UI, `RackCadPushBackEditorTests` (16): la oferta y solo la respuesta de su número; el rack que la puerta rechaza; «Actualizar» con copias,
  bloqueado mientras escribe, `Started`, «escrito» y «Descartar» a lo escrito; «Dejar de esperar» y la respuesta tardía; otro dibujo y el
  dibujo cerrado; «Cerrar el editor»; la selección y el inspector; el valor rechazado; el tope en la matriz y el inspector; los dos lados y
  las camas; un módulo y «Restaurar layout…»; el borrador de «Nuevo» insertado como rack nuevo con su nombre automático; «Insertar vista…»;
  la Biblioteca y el BOM del borrador; y con WPF real (STA), sin errores de enlace: la vista del editor (cinco pestañas, «Matriz» la
  inicial, celdas con nombres únicos, el selector de lados, ningún nombre de tipo en pantalla, nombres únicos de la cabecera, «Restaurar
  layout…» en línea, el inspector debajo en estrecho) y la ventana (la oferta, el editor alojado, el panel y vuelta, «Push Back en esta
  ventana» con su anfitrión).
- **Comprobación en rojo:** doce piezas rotas a propósito, una a una, con script (`hs65\t5\pb\red.py` del scratchpad de la sesión; salida en
  `red-core-log.txt`, `red-ui-log.txt` y `red-out.txt`). El script guarda antes una copia del original FUERA del worktree, compila, corre la
  clase focal y restaura; las doce volvieron con el mismo SHA-256, y después se buscó en `git diff` cualquier resto (`if (false)`): no
  quedó ninguno. Cada pieza hizo fallar su prueba:
  1. la copia para AutoCAD es el diseño vivo: falla la de «Actualizar» con copias;
  2. la respuesta de la lectura se toma sin mirar su número: falla la de la oferta;
  3. un valor rechazado no devuelve el estado: **sobrevivió** en la primera pasada (el diseño publicado es el último válido, así que no
     cambiaba); la prueba se reforzó (el campo vuelve a su valor y el gesto siguiente recompone) y ahora falla;
  4. las celdas se escriben en los dos lados con cualquier selector: falla la de los lados (Core);
  5. el veto del cierre sin Push Back: falla la de «Actualizar» (D-08);
  6. la longitud de un módulo sin confirmar en la sesión: fallan 2 (Core);
  7. `RACKPUSHBACKABRIR` con marca de Deshacer: falla la guarda de la orden (Core);
  8. «Restaurar layout» que no devuelve los fondos propios: falla la de «Restaurar layout» (Core);
  9. el tope dibujado sin mirar las camas: falla la de las camas (Core);
  10. la puerta abre un rack con la cola dormida: falla la de la lectura de la puerta (Core);
  11. el selector de lados no cambia lo que se escribe: fallan 2 (los dos lados y la vista);
  12. el rack nuevo que AutoCAD insertó sigue siendo nuevo: falla la del borrador de «Nuevo».

**Los números** (sobre las fuentes finales):
- **Core Full:** 19331 correctas, 0 con error, 403 omitidas (19734), compilado con `--no-incremental`: la base (19292) más las 39 de
  `PushBackDraftEditorTests`. La primera pasada, antes de ajustar los censos, dio 19327 / 4 / 403: los cuatro de arriba.
- **UI completa:** 2977 correctas, 0 con error, 48 omitidas (3025): la base (2961) más las 16 de `RackCadPushBackEditorTests`; la prueba
  cambiada de `RackCadNewDraftsTests` no añade casos. Se corrió dos veces, antes y después de reforzar la prueba de la pieza 3, con el
  mismo resultado.
- **Plugin en Debug:** 0 errores; 2 advertencias, las MSB3277 de las referencias de AutoCAD que ya estaban.

**Lo que queda abierto:**
- **Falta para terminar Push Back en la ventana:** «Comprobar con el dibujo» y el conflicto (PB-S04, pregunta 9); «Instrumentación» a «Configuración» y retirar «Editar todo» del panel del piloto (PB-S05,
  pregunta 10); el editor de cabeceras de Push Back (otra iniciativa, pregunta 11).
- **El smoke en AutoCAD:** que RACKPUSHBACKABRIR no deje paso en Deshacer; que un «U» deshaga RACKPUSHBACKAPLICAR entero; «Insertar
  vista…» de un rack de dos sentidos con los cortes de B; un borrador insertado con su nombre automático y reabierto con «Editar aquí»;
  RACKEDITAR abre lo que escribió la ventana igual que antes; la paleta real (anchos, foco).
- **Para el Owner:** PB-S02 y PB-S18, PB-S08, PB-S09, PB-S10, PB-S12, PB-S13, PB-S16 y PB-S17 (con el defecto del clásico de su punto 5).
- **Para integrar:** las ediciones de una línea de arriba; los censos de órdenes (`SelectiveEditorOpenTests`, `CustomPropertiesCommandGuardTests`)
  y `RackCadWindowListTests` también los tocan las otras partes que añaden puertas.

### PB-S03: la seguridad poste por poste

Sobre `24adfee7`, con las mismas reglas (sin commit, sin push, sin AutoCAD). «Extremos y seguridad» de Push Back lleva ahora la tabla
«Seguridad por poste» editable y el inspector «Variante y medidas» de la cuarta tanda (SG-09: Push Back adopta este diseño sin tocar su
autoridad de seguridad), con las vistas compartidas (`RackCadSafetyTableView`, `RackCadSafetyInspectorView`) y el modelo compartido
(`RackCadSafetyPanelViewModel`, con un parcial nuevo para Push Back). El aviso de PB-S03 (`SafetyPendingText`) se quitó.

- **Las familias** son las que admite su autoridad (`PushBackSafetyAuthority`): botas, protector lateral, desviadores y defensa
  (`RackCadSafetyEdit.FamiliesOf(PushBack)`). El tope posterior sigue en su tarjeta y en la «T» de «Matriz».
- **El vocabulario** (SG-02 y pregunta 8 del prototipo): las botas y la defensa dicen «Entrada/Salida» y «Posterior», **por lado**.
  - Botas: «General (…)», «Ninguno», «Entrada/Salida», «Posterior» y «Ambas», como la sección del clásico; la general muestra
    «, automática» mientras nadie la elige.
  - Defensa, en un rack de un sentido: «Defensa Entrada/Salida (in)» y «Defensa Posterior (in)».
  - Defensa, con lado B: «Defensa lado A (in)» y «Defensa lado B (in)», cada una la «Entrada/Salida» de su lado.
  - El protector lateral conserva «Izquierda / Derecha / Ambos (frente-puente)» de su guía (supuesto PB-S16).
- **Por lado** (I-42), con lado B:
  - dos columnas, «Botas · lado A» y «Botas · lado B»;
  - en el inspector de las botas: «Tipo de protector · lado A» y «· lado B» (con «Ninguno (este lado no lleva)»), «Ubicación general» de
    cada lado (elegida o automática) y «Todos los postes al general» de cada lado;
  - en el de la defensa: «Tipo de defensa» de cada lado y «Todas automáticas» de cada lado.
- **«Ninguno» en el tipo de un lado** no pregunta nada: no se pierde nada, la ubicación y los postes quedan dormidos y vuelven al elegir
  otra vez un tipo, como en el clásico. «Sin variante» del protector lateral o del desviador pregunta antes qué se pierde (SG-06), como la
  cuarta tanda, y «Descartar» lo recupera.
- **La defensa de un poste:**
  - la casilla encendida pone la medida de la regla como propia (12 in en un poste de orilla, 36 in en uno intermedio), como la propone la
    rejilla del clásico;
  - una medida vacía es la automática (el «Auto» del clásico); en «Posterior» de un rack de un sentido, la automática es sin defensa
    (PB-009);
  - una medida escrita es propia y se queda.
- **El desviador:**
  - va por poste (PB-002); sus niveles son los del lado más alto cuando hay lado B;
  - no tiene lado que elegir (PB-003): se escribe en el lado que devuelve la rejilla de Push Back, el del extremo bajo (Izquierdo);
  - la longitud y el primer nivel se validan en su campo;
  - uno nuevo llega como lo acepta su rejilla sin tocar nada (18 in y el primer nivel a 18 in, en todas las celdas, en el lado del
    extremo bajo);
  - cambiar la variante de uno que ya existe cambia solo la variante: sus medidas y sus celdas se escriben como las tiene su fila, como el
    «Aceptar» del clásico sin «Configurar…».
- **La tabla se cuenta como el BOM de Push Back** (`PushBackSafetyRows`):
  - las botas de cada lado, de `PushBackBootPlan`;
  - los desviadores, de `PushBackDiverterPlan`;
  - el protector lateral y los dos extremos de la defensa, de la planta compartida (con el tipo de cada cara y si la línea la tiene);
  - el pie «Total» suma las filas.

**Lo que escribe es lo del diálogo clásico para la misma elección.** `PushBackSafetyEdit` (Application, puro) trabaja sobre una copia
(`PushBackSafetyCopy`: la lista, el tipo de defensa de cada lado, la bota resuelta de la última recomposición y las caras de cada lado), y
`PushBackDraftEditor.ReplaceSafety` la devuelve: la lista, el tipo de defensa del lado A y, con lado B, el del lado B, que el clásico no toca
en un rack de un sentido. Familia por familia:
- **Botas:** las dos secciones (`BuildBootSection`, `PushBackBootSection`) y su «Aceptar» (`ApplyBootSections`, `BootCarrierId`). La
  selección se reconstruye con su portador, una pieza, sin lado general, la configuración entera de cada lado, el lado B vacío en un rack
  de un sentido y `BootSidesDeclared`. Un poste pasa por la rejilla por poste (los postes con decisión propia, en su orden), y la general
  solo se elige cuando cambia, como su lista.
- **Defensa:** las dos secciones (`BuildDefenseSection`, `DefensePieceIdOf`) y su «Aceptar» (`ApplyDefenseSections`,
  `EnsureDefenseSelection`).
  - El tipo de cada lado va a su estado.
  - La familia existe mientras un lado tiene tipo, o solo como portadora de los postes con los dos en «Ninguno». Sin tipo en ningún lado y
    sin nada propio en ningún poste, la familia sale y un poste no tiene dónde guardar una decisión (el clásico tampoco la escribe): las
    casillas de la tabla quedan deshabilitadas hasta elegir un tipo.
  - Se reconstruye con una pieza, sin lado y restringida al extremo bajo.
  - Un rack de un sentido toma la rejilla de dos extremos; uno con lado B funde cara a cara; una línea que ya no existe no deja registro.
  - Cada registro pasa por la rejilla como ella lo muestra: un extremo automático no guarda longitud, y una longitud se relee con «0.##».
- **Protector lateral:** la ventana por poste y el «Aceptar» del diálogo. Abierta sobre la regla, la ventana muestra los dos extremos; el
  poste toma su valor («No lleva» se escribe); la familia se reescribe con los postes en su orden y sin lado general. «Volver a la regla de
  los extremos» es la ventana con todos los postes en «(por defecto)».
- **Desviador:** la rejilla (`SafetyDesviadorGridWindow.BuildResult`):
  - las celdas apagadas en el orden de la rejilla, y después las dormidas de una columna sin niveles (I-33);
  - una celda más allá de los niveles de su poste se va;
  - la longitud y el primer nivel como los muestra la rejilla;
  - el lado que la rejilla devuelve en Push Back (Izquierdo: se abre con `showDesviadorSide: false` y elige su `LowEndSide`) y una pieza.
  - El cambio de variante de uno que ya existe no pasa por la rejilla: es la fila del diálogo (`PushBackSafetyEdit.DesviadorRow`), con sus
    medidas sin validar y todas sus celdas apagadas, también las que quedan por encima de los niveles de su poste.

**Comprobado en Core** con `PushBackSafetyEditTests` (21 casos), que usa una transcripción del clásico independiente de
`PushBackSafetyEdit` (la clase `Classic` de la prueba, con las líneas de origen nombradas):
- Cada prueba toma dos borradores del mismo rack: la ventana edita uno, la transcripción escribe el otro como su «Aceptar».
- Los dos diseños tienen que ser iguales y el BOM de seguridad también, y distintos del de antes, para que ninguna prueba quede vacía. La
  lista de seguridad se lee en un orden (supuesto PB-S17, 2).
- Casos de botas: el poste, la general, «Ninguno» y la vuelta, la matriz de `pb-06-seguridad` y los dos lados.
- Casos de defensa: «Posterior» encendida, una medida, apagada, la automática, «Todas automáticas», «Ninguno» con los postes en espera,
  «Ninguno» sin nada propio (la familia sale y un poste no tiene dónde guardarse), cada cara con lado B y la defensa leída de un dibujo.
- Casos de protector lateral: el poste desde la regla, «No lleva» y la vuelta a la regla.
- Casos de desviador: niveles y medidas, una medida no válida, uno nuevo (en el lado del extremo bajo), el cambio de variante sin la rejilla
  (una celda por encima de los niveles de su poste y una medida no válida se quedan) y con lado B.
- Las filas de la tabla, contadas como el BOM, en los dos tipos de rack.

**Diferencias con el clásico, declaradas** (supuesto PB-S17):
1. Una edición escribe **una familia**. El «Aceptar» del clásico reconstruye todas las del diálogo a la vez; las demás no cambian su
   diseño.
2. **El orden de la lista se conserva.** El clásico deja primero las familias del diálogo, después la defensa y después las botas.
3. **Una familia que se queda sin nada conserva su variante:** el protector lateral con todos los postes en «No lleva», o el desviador sin
   ningún nivel. El clásico la quita; el dibujo y el BOM son los mismos (C2-07, como la cuarta tanda; la prueba lo mide).
4. **Los niveles del desviador son los del lado más alto.** La rejilla del clásico muestra los del lado activo, así que aceptarla desde el
   lado más bajo quita las celdas apagadas de los niveles que solo tiene el otro.
5. **Volver a «Auto» en una cara cuya otra cara es automática**, en un rack con lado B: la rejilla de una cara del clásico quita esa fila
   (automática en los dos extremos) antes de fundirla, así que pierde el gesto y la medida se queda. Se midió en Core
   (`BackToAutoOnAFaceWhoseOtherFaceIsAutomaticIsWrittenWhereTheClassicGridLosesIt`). La ventana escribe el gesto. **Es un defecto del
   clásico, para el Owner;** no se tocó el clásico.
6. **Un desviador o un protector lateral nuevos llegan con elegir la variante.** El desviador del clásico pide además «Configurar…».

**Archivos.**
- Nuevos:
  - Application: `VentanaRackCad/PushBackSafetyEdit.cs`, `VentanaRackCad/PushBackSafetyRows.cs` y `VentanaRackCad/PushBackDraftEditor.Seg.cs`.
  - UI: `VentanaRackCad/RackCadSafetyPanel.PushBack.cs` y `VentanaRackCad/PushBackEditorViewModel.Seg.cs`.
  - Pruebas: `tests/RackCad.Tests/Workspace/PushBackSafetyEditTests.cs` y `tests/RackCad.UI.Tests/Workspace/RackCadPushBackEditorTests.Seg.cs`.
- Míos, cambiados:
  - `PushBackDraftEditor.cs` (parcial);
  - `PushBackEditorViewModel.cs` (una línea en `Rebuild`; sin `SafetyPendingText`);
  - `PushBackEditorView.xaml` (la pestaña con la tabla y el inspector, en su `Grid`, como Dinámico);
  - `RackCadPushBackEditorTests.cs` (parcial).

**Ediciones a los modelos y vistas compartidos de seg,** pequeñas y marcadas `T5 pb`:
- `RackCadSafetyEdit.cs`: el valor `PushBack` del enum y sus familias en `FamiliesOf`.
- `RackCadSafetyRows.cs`: `RackCadSafetyRow.BootsB`.
- `RackCadSafetyPanel.cs`:
  - la clase pasa a `partial`;
  - `IsSelective` es solo Selectivo: antes era «no Dinámico», y Push Back no es ninguno de los dos;
  - `PostFoot` tiene el texto de Push Back;
  - una línea al principio de `RefreshTable` y otra de `RefreshInspector` llevan a Push Back a su parcial;
  - en `RackCadSafetyWords`, `Side` y `SideGuard` tienen la rama de Push Back, y hay un `Placement` nuevo.
- `RackCadSafetyPanelItems.cs`: `RackCadSafetyRowItem.BootsB`.
- `RackCadSafetyTableView.xaml`:
  - el título de las botas sale del panel;
  - hay una columna para las botas del lado B;
  - las columnas de la defensa salen también en Push Back, con sus títulos del panel (los nombres de Dinámico no cambian);
  - las guías, solo en Dinámico;
  - un estilo nuevo, `DefenseColumns`.

**Un fallo que encontró la prueba de la vista** (no llegó a ningún commit): la escritura del parcial devolvía
`host?.WritePushBackSafety(...) ?? "…"`. Como una escritura correcta responde `null`, toda escritura correcta salía en el campo como error.
Se corrigió, y es la pieza S9 de la comprobación en rojo.

### Correcciones de la revisión (sobre `24adfee7`)

Las seis observaciones de la revisión independiente, en el mismo worktree. Cada una tiene su prueba, y las tres primeras su pieza en rojo
(también la 4, la 5 y la 6).

1. **«Dejar el frente en blanco» con «Ambos lados» siempre se rechazaba** (CONFIRMADO). El lado A se ponía en blanco primero y, al
   evaluar B, `WhySlotCannotBeRemoved` veía A ya ausente. Además era más estricto que el clásico.
   - Ahora va por la ruta del clásico (`SetFrontActive`): cada lado que cambia pasa por `PushBackCompositeEditorState.SetSlotPresent`,
     que deja un lado entero en blanco mientras el otro sostiene el rack. El gesto se escribe cuando algún lado lo tomó (`applied |=`).
   - Solo se rechaza cuando ningún lado lo tomó: «Al menos un frente del rack debe permanecer activo.», el texto del clásico. (Un rack de
     un sentido decía otro, sin «del rack»; la revisión de `01e76902` lo unificó con el del clásico, y PB-S18 declara la decisión lado
     por lado.)
   - Código: `PushBackDraftEditor.SetBlank(sides, front, blank)` y `PushBackEditorViewModel.SetFrontBlank`.
   - Pruebas: Core `PushBackReviewCorrectionsTests` (los dos lados; B entero en blanco mientras A sostiene; una ranura ausente en B puesta
     en blanco en A; el último frente del rack) y UI `BlankWithBothSidesIsWrittenInTheTwoSidesAsTheClassicWindowDoes`.
2. **El BOM del borrador se saltaba la puerta de salida** (PLAUSIBLE). `PushBackDraftBom` devuelve ahora `null` con
   `Draft.IsOutputBlocked` o con `LastError`, como `OutputBlockedReason` del BOM del dibujo (I-42). Prueba UI
   `TheBomOfADraftThatTheGateBlocksIsNotCounted`: una estructura manual de 1 fondo, que bloquea la salida.
3. **«Descartar» de un rack que se abrió sin recomponerse no volvía atrás** (CONFIRMADO).
   - El editor recibe el JSON leído (`readDesignJson`, desde `OnPushBackRead`). Mientras el borrador no tiene un diseño que recomponga, ese
     JSON es su base de «Modificado» y lo que «Descartar» carga otra vez, aunque no recomponga.
   - Al recomponer, el borrador queda «Modificado» frente al dibujo hasta «Actualizar».
   - Prueba UI: `DescartarOfARackReadWithoutADesignOfItsOwnLoadsWhatWasRead`.
   - Ningún diseño del corpus abre así, y tampoco las variantes probadas: o recomponen, o la carga misma lo rechaza. Por eso la prueba
     construye el editor sobre un borrador sin diseño.
4. **`TopeDrawnAt` ignoraba el tipo «Ninguno»** (CONFIRMADO). Ahora mira `!Side(side).RearTopeConfig().IsNone`, así que la «T» de la
   matriz y el resumen dicen lo mismo que «Extremos». Prueba Core `TheTopeOfTheMatrixFollowsTheTypeNoneOfItsSide`: la intención se queda,
   dormida.
5. **Una guarda que ata la copia de la mitad que dibuja** (PB-S06).
   - `RackPushBackWindowDrawing.Apply` lleva ahora, desde la preflight hasta el final, el cuerpo de `EditPushBack` copiado tal cual, con
     cuatro diferencias declaradas: la entrada de la ventana en lugar de la ventana clásica, las respuestas con las cuentas, el prefijo del
     payload y las palabras del mensaje final. Lleva también su propio `IsValidPushBackDescriptor`.
   - `PushBackWindowDrawingMirrorGuardTests` compara los dos cuerpos normalizados (sin comentarios ni espacios) y las dos comprobaciones
     del descriptor. Tiene su autoprueba: tres cambios en un solo lado la hacen fallar.
   - La guarda del texto de la primera entrega busca ahora `!IsValidPushBackDescriptor(viewBlock.Embed)` antes del primer redibujo.
6. **PB-S10 solo se aplicaba en la puerta** (BAJA). `CreatePushBackDraft` rechaza también un diseño con la mitad dormida de un lado B, de
   la biblioteca o de «Duplicar», con el aviso de la puerta. Prueba UI
   `NuevoFromADesignWithTheDormantHalfOfASideBIsRefusedWithTheWordsOfTheDoor` (`pb-21-cola`).

### PB-S03 y la revisión: pruebas, comprobación en rojo y números

**Las pruebas nuevas:**
- Core:
  - `PushBackSafetyEditTests`: PB-S03, contra la transcripción del clásico (su cuenta, en la subsección PB-S03);
  - `PushBackReviewCorrectionsTests` (3): los puntos 1 y 4;
  - `PushBackWindowDrawingMirrorGuardTests` (3): el punto 5.
  - En `PushBackDraftEditorTests` cambió el texto que busca una guarda (punto 5); no hay casos nuevos.
- UI, `RackCadPushBackEditorTests.Seg.cs` (11, la clase pasa a `partial`):
  - de PB-S03, el modelo (las palabras y la tabla contada, la defensa con su medida de la regla y la vacía automática, los dos lados,
    «Ninguno» sin pregunta y «Sin variante» con pregunta y «Descartar», bloqueado mientras AutoCAD escribe);
  - de PB-S03, la vista con WPF real (columnas, nombres únicos, sin errores de enlace ni nombres de tipo; escribe desde sus celdas; el
    inspector al lado y debajo en estrecho) y la del rack de dos sentidos;
  - de la revisión, los puntos 1, 2, 3 y 6.

**Comprobación en rojo:**
- Dieciocho piezas rotas a propósito, una a una, con script: `hs65\t5\pb\red2.py` del scratchpad de la sesión, con su salida en
  `red2-log.txt` y `red2-out.txt`.
- El script guarda antes una copia del original FUERA del worktree, compila, corre las clases focales (Core, UI o las dos) y restaura.
- Las dieciocho volvieron con el mismo SHA-256, y después se buscó en `git diff` cualquier resto: no quedó ninguno.
- Cada pieza hizo fallar su prueba:
  - S1, las botas sin `BootSidesDeclared`: fallan 6 (Core).
  - S2, la general elegida aunque no cambie: falla la de la general.
  - S3, la defensa de un compuesto sin fundir cara a cara: falla la de las dos caras.
  - S4, la casilla encendida como automática: fallan 3 (una medida de la regla, la defensa leída y las filas).
  - S5, el protector lateral sin los extremos al salir de la regla: falla la de la ventana por poste.
  - S6, la longitud del desviador sin validar como la rejilla: **sobrevivió** en la primera pasada, porque un desviador nuevo ya nace con
    18 in. Se añadió la prueba de una medida no válida (`AMeasureOfTheDesviadorThatIsNotValidIsTheDefaultOneThroughItsGrid`), y ahora
    falla.
  - S7, las filas sin las botas del lado B: falla la de las filas.
  - S8, el tipo de defensa del lado A sin escribir: fallan 5.
  - S9, una escritura correcta dicha como error: falla la del modelo (UI).
  - S10, sin la columna del lado B: fallan 2 (UI).
  - S11, Push Back leído como Selectivo: fallan 2 (UI).
  - S12, la tabla compartida para Push Back: fallan 5 (UI).
  - V1, «En blanco» rechazado con «Ambos lados»: fallan 2 en Core y 1 en UI.
  - V2, el BOM sin la puerta de salida: falla la del BOM (UI).
  - V3, «Descartar» sin la base leída: falla la de «Descartar» (UI).
  - V4, la «T» que no mira «Ninguno»: falla la del tope (Core).
  - V5, la copia de la mitad que dibuja sin el aviso de unidades: falla la guarda espejo.
  - V6, «Nuevo» con la cola dormida: falla la de «Nuevo» (UI).

**Los números** (sobre las fuentes finales):
- **Core Full:** 19356 correctas, 0 con error, 403 omitidas (19759), compilado con `--no-incremental`. Son las 19331 de la primera entrega más las 25 nuevas.
- **UI completa:** 2988 correctas, 0 con error, 48 omitidas (3036). Son las 2977 de la primera entrega más las 11 nuevas.
- **Plugin en Debug:** 0 errores; 2 advertencias, las MSB3277 de las referencias de AutoCAD que ya estaban.

### Correcciones de la revisión de PB-S03 (sobre `01e76902`)

Los seis hallazgos de la revisión independiente, en el mismo worktree, sin commit. Los de código tienen su prueba y su pieza en rojo.

1. **Cambio de variante de un desviador que ya existe** (media-baja).
   - Antes pasaba por la rejilla (`WriteDesviador`): una longitud o un primer nivel no válidos quedaban en 18 in, y se borraban las celdas
     apagadas por encima de los niveles de su poste.
   - Ahora `SetVariant` escribe la fila del diálogo, como el «Aceptar» del clásico sin «Configurar…» (`PushBackSafetyEdit.DesviadorRow`):
     la otra variante, una pieza, el lado que tenía (Izquierdo o Derecho; si no, Ambos), las medidas sin validar, la lectura por poste y
     todas las celdas apagadas. En el clásico la fila conserva todo porque sus niveles salen de `SelectiveDesviadorPlan.Build(null, …)`,
     vacíos, y `AllCellsOff` da falso.
   - **Supuesto medido:** se reconstruye la fila; no se muta solo el `ElementId` de la selección. Un desviador recién nacido guarda
     `AuthoredSide = Ambos` (la seguridad por defecto pasa por la autoridad). El clásico escribe una fila nueva, y la autoridad le pone el
     lado de la fila (Izquierdo). Mutar solo el id conservaría «Ambos»: la pieza R1x lo mide. Lo que muestra la fila solo cambia en la variante.
   - Prueba Core `AVariantChangeOfADesviadorWritesItsRowAsTheClassicAcceptWithoutItsGrid`, contra la transcripción nueva
     `Classic.DesviadorVariant`. Recorre tres pasos:
     - la variante sobre el desviador recién nacido;
     - N4 de P2 apagado, los frentes de P2 bajados a 3 niveles y la variante otra vez: la celda se queda;
     - una longitud de 7 in y la variante otra vez: los 7 in se quedan.
   - La prueba que se llamaba «…AndAVariantChangeKeepsTheRow» no cambiaba ninguna variante. Ahora se llama
     `ANewDesviadorComesAsItsGridAcceptsItOnTheSideOfTheLowEnd` y comprueba lo que dice (punto 2).
2. **Lado de un desviador nuevo** (baja).
   - `WriteDesviador` escribe ahora `PushBackSafetyEdit.DesviadorGridSide` (Izquierdo). Es el lado que devuelve la rejilla del clásico de
     Push Back: se abre con `showDesviadorSide: false`, elige su `LowEndSide` y `BuildResult` lo devuelve.
   - Un desviador nuevo, y cualquiera que pase por la rejilla, queda en el diseño con `Side` y `AuthoredSide` en Izquierdo, como en el clásico.
   - `RackCadSafetyEdit.Fresh` no cambió, así que Selectivo y Dinámico siguen igual: sus pruebas de seguridad, en verde.
   - La transcripción modela el lado que fuerza la rejilla (`Classic.GridSide`), no el de la ventana ni el guardado, en `Classic.Desviador`
     y en `Classic.NewDesviador`.
3. **Defensa con los dos lados en «Ninguno» y sin familia** (baja).
   - **Elegido: deshabilitar.** Es el patrón del panel: la casilla de la defensa ya tiene `IsEnabled` ligado en su plantilla y la medida
     `IsReadOnly`, como mientras AutoCAD escribe.
   - `PushBackSafetyEdit.DefenseHoldsPosts` dice si una decisión tiene dónde escribirse: un lado con tipo, o la familia como portadora.
   - Si no la tiene:
     - cada casilla queda deshabilitada y su medida, de solo lectura, con «Sin tipo: elige uno para decidirla»;
     - un clic que llegue igual (por código) no escribe y dice por qué, con un mensaje de error visible.
   - La nota del inspector dice ahora lo que pasa: «"Ninguno" en el tipo conserva lo que cada poste tiene de propio… Si ningún lado tiene
     tipo y ningún poste tiene nada propio, la defensa sale del rack y sus casillas quedan deshabilitadas; al elegir un tipo vuelven las
     automáticas.» El aviso de «Ninguno» lo dice también cuando no había nada propio.
   - Pruebas:
     - Core `WithNoTypeOfDefenseAndNothingByPostADecisionOfAPostHasNowhereToGoAsInTheClassic`, también contra la transcripción: el clásico
       tampoco escribe nada;
     - UI `WithNoTypeOfDefenseAndNothingByPostItsBoxesAreDisabledUntilATypeIsChosen`.
4. **«Dejar el frente en blanco»** (baja).
   - Sin cambio de comportamiento: la decisión lado por lado queda declarada como **PB-S18**, junto a PB-S02 en la tabla y en «Para el
     Owner».
   - El rechazo es el del clásico (`SetFrontActive`) en cualquier rack: «Al menos un frente del rack debe permanecer activo.».
   - Prueba ajustada: `PushBackDraftEditorTests.LevelsAndPositionsGoThroughTheStateOfEachSideAndABlankFrontKeepsItsLevels`, en un rack
     de un sentido.
5. **Textos.**
   - El comentario de `Canonical` dice lo que hace: el diseño serializado, con la lista de seguridad en un orden; el estado editable no se
     compara.
   - La cuenta de `PushBackSafetyEditTests` queda en un solo sitio, la subsección PB-S03, ya con los dos casos nuevos.
6. **El nombre del título de las botas.**
   - `RackCadSafetyPanelViewModel.BootsHeadName`: con lado B dice «Botas del lado A: ver su tipo y sus medidas», como «Botas del lado B: …».
   - Sin lado B, y en Selectivo y Dinámico, sigue el de siempre.
   - La XAML compartida lo liga: una línea de `RackCadSafetyTableView.xaml`, marcada `T5 pb`.
   - Prueba: las dos vistas de Push Back (WPF real).

**Comprobación en rojo:**
- Siete piezas, una a una, con `hs65\t5\pb\red3.py` del scratchpad de la sesión; su salida está en `red3-log.txt` y `red3-out.txt`.
- El script guarda antes una copia FUERA del worktree, compila, corre las clases focales y restaura.
- Las siete volvieron con el mismo SHA-256, y en `git diff` no quedó ningún resto.
- Cada pieza hizo fallar su prueba:
  - R1, la variante otra vez por la rejilla (el código de `01e76902`): falla la del cambio de variante (la celda de P2).
  - R1x, solo el `ElementId` (la medición del supuesto del punto 1): la primera versión de la prueba **sobrevivió**, porque el desviador
    ya había pasado por la rejilla. Se añadió el paso sobre el desviador recién nacido, y ahora falla por `AuthoredSide` (Ambos frente a
    Izquierdo).
  - R2, el lado de la selección en lugar del de la rejilla: falla la del desviador nuevo (`AuthoredSide`).
  - R3 (Core), la defensa siempre con portador: fallan la de Core y la de UI.
  - R3 (panel), las casillas siempre habilitadas: falla la de UI.
  - R4, el texto de un sentido: falla `PushBackDraftEditorTests`.
  - R6, el nombre fijo del título de las botas: falla la vista de dos sentidos.

**Los números** (sobre las fuentes finales, solo la rama pb):
- **Core Full:** 19358 correctas, 0 con error, 403 omitidas (19761), compilado con `--no-incremental`. Son las 19356 de antes más las 2
  nuevas.
- **UI completa:** 2989 correctas, 0 con error, 48 omitidas (3037). Son las 2988 de antes más la nueva.
- **Plugin en Debug** (la solución entera): 0 errores; 2 advertencias, las MSB3277 de siempre.

## Quinta tanda: integración en la rama de Selectivo (sesión, 2026-11-26/27)

Las seis partes se integraron con `merge --no-ff`, cada una con su commit y el de las correcciones de su revisión independiente. Push
Back lleva además PB-S03, la seguridad poste por poste, en un segundo commit, y las correcciones de su revisión independiente en un
tercero (`773f0a6e`), integradas en un segundo merge de la rama pb.

| Orden | Parte | Commit de la parte | Correcciones | Merge |
|---|---|---|---|---|
| 1 | pp (peralte de todos los postes, pedido del Owner) | `ad70515c` | `b49907d6` | `a74a00b7` |
| 2 | lg («Nuevo › Larguero», WIP) | `855a935c` | `5f96b9d4` | `bf07b251` |
| 3 | al («Almacén») | `1fd0f38d` | `b1217383` | `359628a0` |
| 4 | cm (camas de rodamiento) | `bddc2203` | `2bb19604` | `cdc50113` |
| 5 | ct (Cantilever) | `44c0a439` | `5c0acdba` | `bec64511` |
| 6 | pb (Push Back) | `24adfee7` | `01e76902` (con PB-S03) | `dac8a3a3` (CI verde, run 45937475349) |
| 7 | pb, correcciones de la revisión de PB-S03 | — | `773f0a6e` | este commit |

### Lo que cambió al juntar las partes

- **`DoorRequest`** lleva `IsWarehouse`, `IsCama`, `IsCantilever` e `IsPushBack`. La ventana atiende cama, Cantilever y Push Back en los
  mismos sitios: `ShowPanel`, `HasRackEditor`, `OnDesignRead`, `OnDesignWritten`, `StopWaiting`, `OnRequestStarted`, `DraftStateOf`,
  `UnwrittenWorkOf`, el BOM del borrador, `OpensHere`/`IsEditorAlive` de la biblioteca, los seis métodos de los borradores y
  `Abandon` de la puerta.
- **La poda:** la de Cantilever y la de Push Back pasan a `LostOnPrune`, el aviso único de cm. Cerrar un dibujo con cambios sin escribir
  en varios sistemas da una sola línea con todos los nombres, sin que un aviso pise al otro.
- **Censos:**
  - la base de órdenes suma las de al, cm, ct y pb;
  - el censo de órdenes del Plugin pasa de 57 a 70;
  - la guarda de capas de `RackCadWindowListTests` exime en UNA condición las órdenes de las puertas de cama, Cantilever y Push Back, y
    prohíbe fuera de ellas `ApplyCamaUpdate`, `ApplyCantileverEdit` y `RackPushBackWindowDrawing`.
- **`RackCadLargueroWorkbenchTests`** cuenta la sección «Almacén».

### Números sobre la integración

- UI completa: 3109 / 0 / 48 en `dac8a3a3`; 3110 / 0 / 48 con las correcciones de PB-S03 (+1).
- Core Full: 19481 / 0 / 403 en `dac8a3a3`; 19483 / 0 / 403 con las correcciones de PB-S03 (+2).
- Solución en Debug (Plugin incluido), con `--no-incremental`: 0 errores.

### Pendiente

- El smoke de la quinta tanda en AutoCAD: hecho, en la sección siguiente.
- La validación del Owner de los supuestos de cada parte: PP-S, LG-S, AL-S, CM-S, CT5 y PB-S (con PB-S18, nuevo en las correcciones
  de PB-S03). PB-S17 incluye un defecto del diálogo clásico de Push Back con «Auto» en dos sentidos, que pide decisión del Owner.

## Quinta tanda: smoke en AutoCAD (sesión, 2026-11-26, 23:08 a 23:52)

- **Orden del Owner:** «Continúa con la siguiente fase», con el plan anunciado en el chat: la prueba en AutoCAD cuando terminaran CI y la
  revisión de PB-S03. AutoCAD estaba cerrado.
- **DLL:** Debug de esta rama en `468261f1` (CI verde, run 45943267227), compilado con `--no-incremental`. El commit solo añadió números a
  construido.md sobre el árbol compilado.
  - SHA-256 (prefijo): Plugin `4E93750F2A10DB1D`, UI `3BD01D37F6A7ED60`, Application `8A40866221FA2359`, Domain `13CF722A4061AB03`.
  - Cargado con NETLOAD «Load Once»; FILEDIA a 0 solo para la ruta y de vuelta a 1.
- **Dibujo:** una copia de trabajo de `smoke-sel-prep.dwg` (huella `2FA0BC027C615D53`). No se guardó: tras `_QUIT` → «No», la copia y el
  original siguen con la misma huella.
- **Dejado como estaba:** la paleta, a su tamaño; la configuración del usuario, sin tocar; no se guardó ningún diseño en la biblioteca.

### Resultados (OBSERVADO)

| Paso | Parte | Resultado | Observación |
|---|---|---|---|
| P-1 | pp | PASA | SEL-A › «Rack»: «Peralte de poste» en «Lo esencial», «Para todos los postes». «Avanzado» › fila «Todos los postes»: con un peralte propio de 4 in en P2, «Aplicar a todos» con 5 → «Peralte de 5 in en todos los postes del borrador: se quitó 1 peralte propio (P2: 4 in)». «Actualizar» redibujó las 2 vistas. Leído en el sobre: `PostPeralte` 5 y `PostPeraltes` [0,0,0]. Tras el «U», 3 y [0,0,0]. Ver O-1. |
| P-2 | pp | PASA | DIN 1 › «Rack» › «Lo esencial»: «Peralte de poste», «Para todos los postes, también los de las cabeceras personalizadas». De 3 a 4 → «Actualizar» redibujó su vista; un solo «U» deshizo RACKDINAMICOAPLICAR. |
| L-1 | lg | PASA | «Nuevo» › «Larguero en esta ventana · en construcción»: la marca «En construcción» y el aviso de borrador. Catálogo «Perfiles AISC (1011)» + «C6» → «Mostrando 3 de 1019», agrupados en «Canal CPS»; C6X10.5 con su peralte de catálogo; sin ménsula, «Falta: Elige la ménsula». Con ménsula: «El larguero está completo», la lista de materiales (1 perfil, 2 ménsulas por larguero). No escribió nada: la cabecera siguió en «todo escrito». Ver O-2. |
| A-1 | al | PASA | Con la planta de SEL-A en el dibujo y la lista releída: «Almacén» leyó la semilla (RACKALMACENLEER, 42.78 × 203 in, sin contorno en RACKCAD_SITIO), con el esquema A1, B1, C1, D1 y «3 racks nuevos». LINE, «Volver a leer» y «U» → LINE: la lectura no deja paso en Deshacer. |
| A-2 | al | PASA | «Colocar 3 racks» → «3 racks nuevos (4 × 1, enlazados) y 4 etiquetas en la capa RACKCAD_LAYOUT». Un solo «U» deshizo RACKALMACENCOLOCAR, y la página pasó a «Puede no estar al día · El dibujo cambió desde la lectura». También avisó de que ya había 4 plantas de SEL-A, de que la semilla es la que esté seleccionada y de que repetir el layout lo deja encima. |
| C-1 | cm | PASA | «Cama en esta ventana» (42 rodillos, 1 freno) → «Insertar en el dibujo…» → el jig → «AutoCAD insertó la cama nueva «Cama 1»» (I-60), 45 piezas. Ver O-3. |
| C-2 | cm | PASA | «Cama 1» de la lista: fondo de 96 a 72 (30 rodillos) → «Actualizar» → «todas sus copias reflejan el cambio». Un solo «U» deshizo RACKCAMAAPLICAR. |
| T-1 | ct | PASA | «Cantilever en esta ventana»: «Para que la línea se resuelva (6 cosas)» con sus «Ir a…». Columna W10X12, base de 48, brazo W10X12 de 36 y claro de 60. El brazo no cabía en la placa con 2 troqueles: el aviso lo dijo y con 3 se resolvió (2 estaciones, 48.5 in). «Insertar con 3 vistas» (frontal, lateral · estación 1 y planta, en ese orden) → «Cantilever 1», «COMPLETED (3/3)». Ver O-4. |
| T-2 | ct | PASA | «Cantilever 1»: brazo de 36 a 30 → «Actualizar» redibujó 3 vistas. Un solo «U» deshizo RACKCANTILEVERAPLICAR. Tras cerrar el editor: LINE, «Editar aquí» (leyó 36, lo que quedó tras el «U») y «U» → LINE: RACKCANTILEVERABRIR no deja paso en Deshacer. |
| B-1 | pb | PASA | «Push Back en esta ventana» → «Insertar con 3 vistas» (frontal entrada/salida A, lateral · poste 1 y planta) → «Push Back 1» con 3 vistas. |
| B-2 | pb | PASA | PB 1 › «Editar aquí» → «Actualizar» → «sistema Push Back actualizado desde la ventana RackCad». Un solo «U» deshizo RACKPUSHBACKAPLICAR. Tras cerrar el editor: LINE, «Editar aquí» y «U» → LINE: RACKPUSHBACKABRIR no deja paso en Deshacer. |
| B-3 | pb | PASA | PB 1 › «Extremos y seguridad» › «Seguridad por poste»: el desviador N2 de P2 apagado (total de 6 a 5) → «Actualizar»; tras el «U», un REDO. En el sobre: `DesviadorOffCells` [{Frente 1, Level 1}], `Side` y `AuthoredSide` Izquierdo (corrección 2 de PB-S03). RACKEDITAR abrió PB 1 en el editor clásico sin error y se canceló. |
| G-1 | todas | PASA | Las páginas nuevas en la paleta real (1290 × 860 px): ningún nombre de tipo a la vista. |

### Observaciones

- **O-1 (de la herramienta, no de RackCad):** tras «Actualizar» de SEL-A, el primer «U» deshizo un paso vacío y el segundo deshizo
  RACKSELECTIVOAPLICAR. Entre los dos había leído el sobre por COM, y esa lectura deja un «Group of commands» en Deshacer. En DIN 1, sin
  COM de por medio, un solo «U» deshizo RACKDINAMICOAPLICAR. Desde ahí el smoke no lee por COM entre una escritura y su «U». Los
  «Group of commands» sueltos de la lista de Deshacer son los de O-1 de la cuarta tanda (las lecturas de la paleta) y los de COM.
- **O-2 (cosmético, lg):** antes de elegir la ménsula, su lista vacía se ve gris, como deshabilitada, aunque se abre y deja elegir.
- **O-3 (cosmético, anterior):** el aviso de la línea de órdenes al insertar una cama o un Push Back nuevos, o la planta de SEL-A, dice
  «Punto de inserción de la cabecera». El Cantilever dice «de la vista Cantilever».
- **O-4 (ct):**
  - «Ir a «Línea» › «Niveles»» del claro cambia de pestaña y abre el grupo (`LevelsOpen`). El grupo, sin embargo, queda al final de la
    pestaña, fuera de la vista: hay que bajar hasta él. Un clic del operador en su cabecera lo plegó y otro lo volvió a abrir.
  - El aviso de la placa muestra nombres internos («lado PositiveY», «Aumenta VerticalPunchCount»). Viene del dominio de I-37C
    (`CantileverArmConnectionMetricsResolver.cs:355`, 2026-07) y no de esta tanda.
- **O-5 (limitación declarada M-16, anterior a la quinta tanda, con consecuencia):** tras un «U», el editor de la ventana sigue en
  «Limpio» con lo que se deshizo. SEL-A siguió mostrando 5 in con el dibujo en 3. Ningún editor de la ventana recibe la señal de una
  orden ajena: `MarkDraftsPossiblyStale` solo marca los borradores del piloto. Si después se edita otra cosa y se pulsa «Actualizar», se
  vuelve a escribir lo deshecho sin aviso. «Cerrar el editor» y «Editar aquí» leen el dibujo de nuevo. Para el Owner.
- **Falso hallazgo del operador, retirado:** «Volver a leer la lista» parecía no releer, sin mensaje. Una revisión de solo lectura no
  encontró nada de la quinta tanda en ese camino. Invocado por UI Automation, el botón releyó en el acto: 8 racks, con «Cama 1»,
  «Cantilever 1», «Push Back 1» y la planta de SEL-A. Los clics del operador caían a la derecha del botón, más estrecho de lo que se
  midió en una captura reducida. Lo mismo pasó con un campo del Cantilever: el clic caía debajo. Con la captura a tamaño completo, los dos
  funcionaron.
  - Lo que sí queda de esa revisión, anterior a T5 (`fa00d092`, V-10): un «Busy» o un «Rejected» del host a una relectura pedida por el
    usuario no cambia el texto de la lista, que se queda en «puede no estar al día». No pasó en este smoke, pero si pasa, el usuario no
    sabe por qué. Una prueba sin AutoCAD lo reproduciría con un doble que conteste «Busy» a la relectura.

### Pendiente

- La validación del Owner de los supuestos de la quinta tanda y de O-5.
- La limpieza de los worktrees y las ramas locales de la quinta tanda, con aviso al Owner.

## Sexta tanda, b1: B-1 en el clásico de Push Back y de Cabecera (2026-11-27)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-b1`, sobre `f02f519b` (worktree `D:\w6b1`). El commit de la parte es
`dc0ada4c`, hecho por la sesión; las correcciones de su revisión, al final de esta sección, van sin commit. Sin push y sin AutoCAD. La orden
del Owner del 2026-11-27: arreglar los defectos sin borrar el editor clásico; el arreglo va en el propio clásico y en la rama de I-64, no en
`main`.

**1. B-1 en Push Back: CORREGIDO, en RACKEDITAR «Insertar» y en «Insertar vista…» de la ventana.**
- **El defecto (confirmado en esta base con una prueba, sin AutoCAD):**
  - «Insertar» con el borrador modificado redibuja con el borrador las vistas que hay, y coloca la vista nueva con el diseño y el nombre
    ANTERIORES en su sobre. La geometría de la vista nueva sí es la del borrador.
  - Las vistas hermanas quedan divergentes: la ventana ya no abre el rack (M-02, «Las vistas de este rack no tienen el mismo diseño…»), y
    RACKEDITAR abre un diseño u otro según la vista elegida.
- **La causa (OBSERVADO, leído en el código):**
  - `RackViewBatchProducts.PushBack` pasaba a la cola de ID18, como fuente, la vista elegida tal como estaba en el dibujo (`source`, línea 96
    de la base).
  - `RackProductPreparer.PrepareExisting` compone el sobre de la vista nueva con la identidad, el nombre y el diseño de esa fuente
    (`RackProductPrepare.cs`, 156 a 163), y su geometría con el sistema editado. Es el mecanismo de B-1 de Dinámico.
  - Las dos mitades de dibujo que insertan llaman a esa fábrica con el mismo `design, id, name` con que redibujan las vistas que hay (tres
    `BuildPushBackPayload(design, id, name, …)` en cada una):
    - RACKEDITAR «Insertar»: `RackPushBackCommands.EditPushBack` (`RackPushBackCommands.cs:250`);
    - «Insertar vista…» de la ventana: RACKPUSHBACKINSERTAR → `RackPushBackWindowDrawing.Apply` (`RackCadPushBackDoorCommands.cs:173` y
      `RackPushBackWindowDrawing.cs:114`).
  - **Por tanto, el camino de la ventana también tenía B-1.** El mismo arreglo lo corrige.
- **El arreglo:**
  - La regla de Dinámico y Cantilever en la fábrica: `RackInsertedViewSource.Edited(source, rackId, rackName, serialized)`, con el mismo
    `serialized` que la fábrica ya daba como diseño de un rack nuevo.
  - Un rack nuevo (el menú, RACKPUSHBACKNUEVO, la biblioteca) no tiene fuente: `Edited(null)` sigue siendo `null`, y la cola usa
    `NewRackCreationContext` como antes.
  - `EditPushBack` y `RackPushBackWindowDrawing.Apply` no cambian. Siguen iguales `PushBackWindowDrawingMirrorGuardTests` y las guardas de
    F4B que fijan su texto.

**2. B-1 en Cabecera: CORREGIDO, en RACKEDITAR «Insertar».**
- **La causa, la misma:** `RackViewBatchProducts.Header` pasaba `source` (línea 117 de la base), y `RackCabeceraCommands.EditCabecera`
  redibuja la lateral y la planta con `BuildCabeceraPayload(config, id, name, …)`.
- **El arreglo, el mismo:** la regla, con `Project(RackProject.ForSelective(configuration), innerSource)`.
- **La puerta de cabeceras de la ventana no tiene B-1 (OBSERVADO, con una guarda): no inserta vistas.**
  - Tiene dos órdenes: RACKCABECERAABRIR, que lee, y RACKCABECERAAPLICAR (`RackCadCabeceraDoorCommands.cs:20` y `:67`).
  - La segunda solo redibuja en sitio con `ApplyCabeceraUpdate` (`RackCabeceraCommands.Door.cs:21`), sin cola de ID18.
  - «Insertar vista…» de una cabecera no existe en la ventana (DG-07). Una cabecera nueva va por `DispatchInsertion`, sin fuente.

**3. Selectivo: no tiene B-1 (comprobado con pruebas; no se tocó).**
- **«Insertar varias…» del clásico sobre un rack del dibujo:**
  - `InsertBatch_Click` (`RackSelectiveWindow.xaml.cs:2548`) manda todas las vistas en una petición, y `EditSelective` las pasa a
    `InsertSelectiveViews` (`RackSelectivoCommands.cs:132`).
  - Esa mitad usa UN `SelectiveInsertPort`: cada vista se prepara con `TryPrepare` desde la misma fuente, compuesta con el borrador
    reconciliado (`curedSource`, `RackSelectivoInsertIntegration.cs:145`).
  - Las hermanas se redibujan con el mismo `authoredJson` (`PrepareMember`, línea 209).
- **`SelectiveNew`:** solo lo llama `DispatchInsertion`, para un rack nuevo (`RackMenuCommands.cs:195`), y sin fuente. Todas sus vistas
  salen del mismo documento autorado y de su serialización, y no hay vistas que redibujar.

**Supuestos para la validación del Owner:**

| Id | Supuesto | Por qué |
|---|---|---|
| T6b1-S1 | **Lo que se guarda.** Con «Insertar» de RACKEDITAR (Push Back y Cabecera) o con «Insertar vista…» de Push Back en la ventana, la vista nueva guarda en su sobre el diseño y el nombre del borrador, iguales byte a byte a los de las vistas redibujadas. Sin cambios en el editor, la vista nueva lleva el diseño re-serializado, como sus hermanas (que se reescriben de todos modos), y no el texto crudo de la vista elegida | La regla de Dinámico (nota 1 de `review-b1.md`) y de Cantilever (CT-11) |
| T6b1-S2 | **Un texto de la línea de órdenes, en un caso límite, solo en Cabecera.** «Insertar» desde RACKEDITAR sobre una cabecera legada sin identidad (Id vacío o en blanco), que toma la de la ventana, sigue rechazado antes de tocar la geometría con «no se preparo la cola ID18; no se modifico ningun bloque.». Lo que sigue cambia: antes, `RACK_ID_INVALID`; ahora, `AUTHORED_UNREADABLE: …` (la puerta AUTH-13). En Push Back no ocurre (corregido en la revisión, M-1): el preflight de los sobres de `EditPushBack` rechaza antes de la cola toda vista cuya identidad no es la del rack, y la ventana encuentra las vistas por esa identidad | M-04 de `review-b1.md`: el mismo cambio que ya tuvo Dinámico |

**Las pruebas (Core):**
- **`tests/RackCad.Tests/Workspace/PushBackCabeceraInsertedViewDesignTests.cs` (nueva, 10 casos):**
  - **Push Back, con la regla:**
    - el dibujo: un Push Back con la lateral de P1 y la planta; el borrador: el tope posterior de F1 N2 apagado y el rack renombrado;
    - se compone como las dos mitades (el proyecto de `PushBackDoorRules.PlanApply` y el nombre de `PushBackDoorRules.Named`, los mismos
      valores que usa RACKEDITAR) y como la fábrica, con el comparador AUTH-13 y el preparador reales;
    - la vista nueva (frontal posterior A) lleva el diseño editado, igual byte a byte al de las redibujadas, con el tope apagado, la identidad
      y el nombre nuevo; `RackSiblingDesigns` da `Same`.
  - **Push Back, lo que fue B-1:** con la vista elegida tal como estaba, la vista nueva lleva el diseño y el nombre de antes, y las hermanas
    dan `Divergent`.
  - **Push Back, la vista legada sin identidad** (Id vacío y en blanco, T6b1-S2): antes, `RACK_ID_INVALID` en la aceptación; ahora la
    rechaza la puerta AUTH-13 (`AUTHORED_UNREADABLE`) antes de resolver y de preparar, con la vista elegida como evidencia o sin ninguna.
    **Retirada en las correcciones de la revisión (M-1):** Push Back no llega a ese camino; la sustituye una guarda del preflight.
  - **Push Back, la guarda de fuente (falla en la base):**
    - la fábrica pasa `RackInsertedViewSource.Edited(source, rackId, rackName, serialized)` con el mismo `serialized`;
    - `EditPushBack` y `RackPushBackWindowDrawing.Apply` le dan el `design, id, name, embed, project` con que redibujan (tres
      `BuildPushBackPayload` en cada una);
    - RACKPUSHBACKINSERTAR llama a `Apply` con el borrador de la ventana.
  - **Cabecera, los mismos cuatro:**
    - con la regla: la lateral; RACKEDITAR con 6 in más de fondo y la cabecera renombrada; «Insertar» › planta;
    - lo que fue B-1;
    - la vista legada sin identidad;
    - la guarda de fuente (falla en la base), que fija además que la puerta de cabeceras no inserta: dos órdenes, `ApplyCabeceraUpdate`, y
      ni cola ni `TryAuthorize`.
- **`tests/RackCad.Tests/Workspace/SelectiveBatchInsertedViewDesignTests.cs` (nueva, 3 casos).** Pasan en la base, porque Selectivo está
  bien:
  - «Insertar varias…» sobre un rack del dibujo (la frontal con 60 in; el borrador con 72; la planta y la lateral de cada poste): un
    puerto, una fuente y una preparación por vista; todas llevan el borrador y son hermanas (`Same`) de la frontal redibujada;
  - un Selectivo nuevo de varias vistas (frontal, planta y laterales) preparado como `SelectiveNew`: todas con el mismo diseño
    serializado, la misma identidad y el mismo nombre (`Same`);
  - la guarda de fuente de los dos caminos:
    - «Insertar varias…» → `RequestInsertViews` → `InsertSelectiveViews`: un `SelectiveInsertPort`, `TryPrepare` por vista, y la cola
      coloca lo preparado;
    - `SelectiveNew`, sin fuente, y llamado solo desde el `case SelectiveInsertionRequest` de `DispatchInsertion` (un rack nuevo).
- **Ningún censo ni guarda cambia.** La regla ya está en los censos de `RackEmbedComposer.Compose` (T-GRD-02 y T-ENV-12), y la fábrica no
  compone ningún sobre nuevo.

**Comprobación en rojo:** tres piezas rotas a propósito, una a una, con script (`S\hs65\t6\b1\red.sh`; la salida, en `red-out.txt`).
Antes de romper cada archivo se guardó una copia fuera del worktree, y después volvió a sus bytes con el mismo SHA-256 (lo comprueba el
script). Tras cada pieza se ejecutaron las clases focales: las dos nuevas, `SelectiveInsertedViewDesignTests`,
`RackInsertedViewDesignTests` y `CantileverDraftEditorTests` (54 casos).
1. **Push Back:** la fábrica pasa a la cola la vista elegida tal como estaba (B-1 de la base). Falla 1: la guarda de Push Back.
2. **Cabecera:** lo mismo en `.Header`. Falla 1: la guarda de Cabecera.
3. **La regla:** `RackInsertedViewSource.Edited` devuelve la vista elegida tal como estaba. Fallan 9: las 6 de comportamiento nuevas de
   Push Back y de Cabecera (la vista nueva con la regla y las cuatro de la vista legada sin identidad) y 3 de Dinámico y Cantilever.

Sobre la base, antes de la corrección, las dos guardas de fuente fallaban y las demás pasaban, porque prueban la regla y lo que fue B-1.
`git diff` no tiene restos de las piezas.

**Los números** (la solución en Debug, recompilada con `--no-incremental` sobre las fuentes finales):
- **UI completa:** 3110 correctas, 0 con error y 48 omitidas (3158), las mismas que la base: esta parte no toca la UI.
- **Core Full:** 19496 correctas, 0 con error y 403 omitidas (19899): las 19483 de la base más las 13 nuevas (10 de Push Back y Cabecera y 3
  de Selectivo).
- **Plugin Debug:** 0 errores. Las advertencias MSB3277 de las referencias de AutoCAD ya estaban.

**Lo que queda abierto:**
- **El smoke en AutoCAD:**
  - RACKEDITAR sobre un Push Back y sobre una cabecera: un cambio, y «Insertar» una vista;
  - «Insertar vista…» de Push Back en la ventana con el borrador modificado;
  - en los tres, leer por COM que la vista nueva lleva el diseño y el nombre del borrador, y que la ventana vuelve a abrir el rack.
- **Preexistente, no de esta parte:** I-02 de `review-b1.md`. Si falla el redibujo de una vista que existe, la cola coloca igual la vista
  nueva (`_ => Applied()`): la garantía de B-1 solo vale cuando todos los redibujos salen bien. **Corregido en las correcciones de la
  revisión, abajo.**
- **Para integrar:** de los archivos compartidos, esta parte solo toca `RackViewBatchProducts.cs`, en dos bloques marcados `T6 b1`. Los
  dos archivos de prueba son nuevos.

### Correcciones de la revisión (sobre `dc0ada4c`)

Sobre el commit de la parte, sin commit (la sesión lo hace). Sin AutoCAD. Las mismas reglas: prueba que falla, arreglo, comprobación en rojo
y supuestos declarados.

**1. I-02: la cola colocaba la vista nueva aunque una vista existente no se hubiera redibujado (CORREGIDO).**
- **Escenario A, la causa (OBSERVADO, leído en el código):**
  - en las cinco mitades de dibujo que insertan, la respuesta del redibujo a la cola era siempre `_ => Applied()`: `EditPushBack`,
    `RackPushBackWindowDrawing.Apply`, `EditCabecera`, `ApplyDynamicEdit` y `ApplyCantileverEdit`;
  - `ViewBlockDraw.RedrawInPlace` convierte cualquier excepción en un fallo, y los bucles solo dejan de contar esa vista (Cantilever atrapa
    la excepción de `RedefineBlock` y sigue);
  - la cola colocaba entonces las vistas nuevas, con el borrador (B-1), junto a una vista con el diseño de antes: hermanas divergentes.
- **El arreglo: una regla pura en Application, `RackInsertRedrawOutcome`** (`src/RackCad.Application/Views/Batch/RackInsertRedrawOutcome.cs`):
  - `Of(vistas, obsoletas, redibujadas, borradas)` da `Applied()` solo si se redibujaron todas las que quedan (las vistas menos las
    obsoletas ya borradas, o menos todas las obsoletas cuando esperan a la primera vista nueva, punto siguiente);
  - si no, da el corte del plan que ya existía (`RolledBack`, `RackViewBatchPlan.cs` 359-361) con un diagnóstico propio:
    «REDRAW_INCOMPLETE: no se pudo redibujar N de las M vistas que ya había del rack, así que no se colocó ninguna vista nueva (sus hermanas
    quedarían con diseños distintos). Lo que sí se redibujó con el borrador no se deshizo: «U» deshace la orden entera, y «Actualizar»
    vuelve a intentarlo.»;
  - **por qué no un tipo nuevo:** la máquina de estados de ID18 (I-55) es un contrato con sus pruebas, y su corte `RolledBack` ya hace lo
    que hace falta (no colocar nada). La palabra «ROLLED_BACK» de la línea «RackCad ID18: REDRAW_ROLLED_BACK (0/N).» engañaría, así que
    el diagnóstico que la sigue dice lo contrario: lo redibujado no se deshizo (T6b1-S3);
  - las cinco mitades pasan a la cola esa regla, con sus cuentas, calculadas después del bucle y del borrado: en Cabecera, laterales más
    plantas y ninguna obsoleta.
- **Escenario B: todas las vistas que había quedaban obsoletas (decisión: opción (a), con el borrado después de colocar).**
  - **Antes:** con `survivors == 0`, las obsoletas no se borraban ni se redibujaban, y la cola colocaba la vista nueva. Con B-1 corregido,
    la vista nueva lleva el borrador y las conservadas el diseño de antes: `Divergent`, y la puerta M-02 rechaza después la ventana. Ejemplo:
    un Push Back cuya única vista es la lateral de un poste que el cambio elimina, e «Insertar» la planta. Dinámico y Cantilever tenían el
    mismo efecto.
  - **Ahora:** con «Insertar», esas vistas obsoletas esperan a la primera vista nueva (`ErasesStaleAfterPlacement`).
    - La cola coloca las vistas nuevas con el borrador.
    - Si colocó al menos una, la mitad de dibujo borra después las obsoletas y lo dice: «todas las vistas que había quedaban obsoletas con
      el cambio; tras colocar la vista nueva se borró 1.».
    - Si no colocó ninguna, no borra nada y lo dice: «no se colocó ninguna vista nueva, así que las vistas que el cambio deja obsoletas se
      conservan y el rack no cambió.».
  - **Por qué (a) y no (b):**
    - con (b), rechazar, el usuario quedaba atascado: «Actualizar» tampoco aplica ese cambio, porque no borra el último vínculo;
    - (a) contando la vista nueva como superviviente antes de colocarla borraría las obsoletas primero, y una colocación cancelada (Esc en
      el jig) dejaría el rack sin ninguna vista;
    - borrar después de colocar nunca deja el rack sin vistas, y es la regla de Selectivo (`DeferToFirstPlacement`).
  - «Actualizar» no cambia: sigue diciendo que todas las vistas quedaron obsoletas y que no se eliminó el último vínculo. Ese aviso ahora
    solo sale con «Actualizar» (`&& window.UpdateOnly`), en Push Back (las dos mitades) y en Cantilever.
  - **La ventana:** `RackDoorRules.AnswerForInsert` toma el borrador como escrito cuando no se redibujó ninguna vista, se borraron
    obsoletas y se insertaron vistas nuevas (solo puede ser este caso, porque con la regla la cola no coloca si falta un redibujo). Dice:
    «AutoCAD insertó 1 vista nueva con el borrador y borró 1 vista que ya no existe con el cambio.». Antes decía que el borrador seguía sin
    escribir y aconsejaba «Actualizar» (T6b1-S4).
- Selectivo no tiene I-02: su redibujo de hermanas es transaccional (`RackSiblingRedrawRun`), y un fallo vuelve atrás antes de colocar.

**2. M-1: T6b1-S2 no ocurre en Push Back (CORREGIDO, el texto y la prueba).**
- En `EditPushBack`, con una vista sin identidad, la identidad es la de la ventana, y el preflight de los sobres (antes de la fábrica de la
  cola) rechaza toda vista cuya identidad no es esa: «una vista ligada de este rack no es Push Back o pertenece a otro sistema». La
  ventana encuentra las vistas por la identidad del rack.
- T6b1-S2 dice ahora que vale solo para Cabecera. La teoría de Push Back que probaba ese camino inalcanzable se retiró; la sustituye una
  guarda: el preflight de los sobres está antes de la fábrica en las dos mitades, y la puerta busca las vistas por la identidad.

**3. M-2 (CORREGIDO).**
- Un caso nuevo de Push Back de dos sentidos (`pb-20-ab`, con el tope de F1 N1 apagado): desde la lateral elegida, la cola prepara el corte
  `PushBackCut(EntradaSalida, B)` y la lateral del último poste con corte.
  - `Edited` no cambia la vista ni la sección de la fuente: siguen siendo las de la lateral elegida.
  - Las vistas nuevas toman las suyas de la dirección pedida: frontal con la sección de entrada/salida de B
    (`EncodeSection(EntradaSalida, B)`) y lateral con el índice del poste.
  - Las dos llevan el borrador, la identidad y el nombre, y son hermanas (`Same`) de las redibujadas.
  - El preparador de la prueba construye ahora el plan de cada clase de vista como `RackViewBatchProducts.PushBackPlan`.
- Las guardas de conteo (`[CommandMethod(` de la puerta de cabeceras, `BuildPushBackPayload` y `BuildCabeceraPayload`, el puerto único de
  «Insertar varias…») llevan un mensaje que dice qué protegen y qué hacer si cambian.

**4. M-3 (ACEPTADO, como en Dinámico y Cantilever).**
- `RackInsertedViewSource.Edited` compone con `RackEmbedComposer.Compose`, que rechaza una fuente con una clave de `ExtensionData` igual a
  un miembro declarado (`RequireNoDeclaredMemberInExtensionData`).
- Con un sobre así, «Insertar» de Push Back y de Cabecera falla ahora al construir la fábrica de la cola. Eso ocurre siempre antes de
  tocar la geometría: la fábrica se construye antes de `PrepareAll`, y los dos van antes del primer redibujo.
- Antes del arreglo de B-1, esa vista fallaba al componer su propio redibujo, ya dentro del bucle. Además, el lector crudo de AUTH-13
  rechaza los miembros desconocidos.

**5. M-4 (CORREGIDO).** La sección y el informe decían «Sin commit»; ahora dicen que el commit de la parte es `dc0ada4c`.

**6. El frontal del lado B de un Push Back que deja de tener dos sentidos (OBSERVADO con una prueba; CORREGIDO).**
- **La causa:**
  - el bucle de las frontales de `EditPushBack` y de `RackPushBackWindowDrawing.Apply` no evaluaba la disponibilidad;
  - el constructor de frontales de un rack de un sentido ignora el lado (`PushBackSystemFrontalBuilder.BuildPlan`: «solo tiene lado A»);
  - y la sección se reescribe tal como venía (I-42).
  - Así, al quitar el lado B, la frontal de B se redibujaba con la geometría de A bajo el descriptor de B. Es el hecho que el piloto de
    F4B ya había medido y declarado como limitación (V2-20, M-08: «un rack de un sentido que ya trae un frontal con `Section` 2, que deja
    RACKEDITAR al quitar B»).
  - El panel la clasifica como `FrontalBOfOneWay` y dice «usa RACKEDITAR».
- **La prueba** (dominio, sin AutoCAD): `pb-20-ab` con el lado B quitado en el borrador.
  - El corte de entrada/salida de B pasa de disponible a `VariantNotPresent` con los hechos que calcula la mitad de dibujo.
  - El plan de ese corte en el rack de un sentido es idéntico, pieza a pieza, al del lado A.
  - La disposición del piloto da `FrontalBOfOneWay`.
- **El arreglo, en las dos mitades (la guarda espejo sigue igual):** la frontal que no está disponible es obsoleta, como la lateral de un
  poste que ya no existe. No se redibuja, y se borra con las demás obsoletas: en el acto si queda alguna vista, o tras la primera vista
  nueva con «Insertar» (punto 1). Con eso, «usa RACKEDITAR» del panel lo resuelve (T6b1-S5).

**Supuestos nuevos para la validación del Owner:**

| Id | Supuesto | Por qué |
|---|---|---|
| T6b1-S3 | **Un texto nuevo en la línea de órdenes.** Si falla el redibujo de una vista que ya había, «Insertar» no coloca ninguna vista nueva. La línea dice «RackCad ID18: REDRAW_ROLLED_BACK (0/N). REDRAW_INCOMPLETE: …», con lo que pasó y con que lo ya redibujado no se deshizo. La ventana recibe la respuesta de siempre para «redibujó, pero no insertó» | La máquina de estados de ID18 no se toca; el diagnóstico corrige lo que «ROLLED_BACK» sugiere |
| T6b1-S4 | **Lo que se guarda y un texto de la ventana.** Con «Insertar», si todas las vistas que había quedan obsoletas con el cambio, la vista nueva se coloca con el borrador y después se borran las obsoletas; si se cancela la colocación, no cambia nada. La ventana lo da por escrito: «AutoCAD insertó N vistas nuevas con el borrador y borró M vistas que ya no existen con el cambio.». Vale para Push Back, Dinámico y Cantilever, en RACKEDITAR y en la ventana; en Selectivo, solo el texto de la ventana, porque ya borraba con la primera colocación | Opción (a) del escenario B, con el borrado después de colocar |
| T6b1-S5 | **Lo que se guarda.** Al quitar el lado B de un Push Back (RACKEDITAR o la ventana), sus frontales de B son obsoletas: se borran, y ya no se redibujan con la geometría de A | El punto 6; la regla de las laterales obsoletas |

**Las pruebas (Core):**
- **`tests/RackCad.Tests/Workspace/InsertRedrawCompletenessTests.cs` (nueva):**
  - la regla, con nueve combinaciones: todo redibujado; un redibujo fallido; todos fallidos; una obsoleta borrada; una superviviente sin
    redibujar; una obsoleta que no se pudo borrar; el escenario B con una y con dos; nada;
  - cuándo esperan las obsoletas (cinco casos) y los textos;
  - **por la cola real de ID18** (`RackViewBatchPlan`, con un puerto de prueba):
    - con la respuesta de antes (`Applied()`), un redibujo fallido coloca las dos vistas nuevas: el defecto reproducido;
    - con la regla, `REDRAW_ROLLED_BACK`, ninguna colocación y el diagnóstico `REDRAW_INCOMPLETE`;
    - el escenario B coloca;
  - la respuesta de la ventana en el escenario B, y que las demás no cambian;
  - **guardas de fuente** (fallan sobre `dc0ada4c`):
    - las cinco mitades pasan `RackInsertRedrawOutcome.Of(…)` con sus cuentas, y ninguna `Applied()`;
    - las cuatro con obsoletas las borran después de la cola cuando ninguna sobrevive, y lo dicen;
    - solo «Actualizar» dice que se conservó el último vínculo;
    - el bucle de frontales de las dos mitades de Push Back descarta la frontal no disponible antes de redibujarla;
  - **el punto 6, en el dominio:** lo de arriba.
- **`PushBackCabeceraInsertedViewDesignTests`:** −2 (la teoría inalcanzable), +2 (la guarda del preflight y el caso de dos sentidos), y
  mensajes en las guardas de conteo. **`SelectiveBatchInsertedViewDesignTests`:** el mensaje de su guarda de conteo.

**Comprobación en rojo:**
- Siete piezas, una a una, con script: `S\hs65\t6\b1\corr\red.sh`, con `mutate.py` para cambiar el texto exacto y la salida en
  `red-out.txt`.
- Antes de cada pieza se guardaron copias de los siete archivos fuera del worktree, y después volvieron a sus bytes con el mismo SHA-256
  (lo comprueba el script). `git diff` no tiene restos.
- Clases focales: `InsertRedrawCompletenessTests`, `PushBackWindowDrawingMirrorGuardTests`, `PushBackCabeceraInsertedViewDesignTests`,
  `RackCadWindowListTests` y `SelectiveDoorInsertionTests` (119 casos).

0. **La base:** las cinco mitades y la respuesta de la ventana tal como están en `dc0ada4c`, con la regla presente pero sin usar. Fallan 15:
   - las cinco guardas de la respuesta a la cola;
   - las cuatro del borrado tras la primera vista nueva;
   - las dos de la frontal no disponible;
   - las tres del aviso del último vínculo;
   - la de la respuesta de la ventana en el escenario B.

   Las dos de la cola real pasan, porque reproducen lo de antes: con `Applied()`, un redibujo fallido coloca las dos vistas.
1. **La regla siempre deja colocar** (el escenario A, como era): fallan 6.
2. **Las obsoletas nunca esperan a la primera vista nueva** (el escenario B, como era): fallan 5.
3. **Cantilever vuelve a responder `Applied()`:** falla 1, su guarda.
4. **La copia de la ventana de Push Back redibuja la frontal no disponible:** fallan 2, la guarda espejo y la de la frontal.
5. **La ventana no da por escrito el escenario B:** falla 1.
6. **RACKEDITAR de Push Back nunca borra tras la primera vista nueva** (solo el clásico): fallan 2, la guarda espejo y la del borrado.

**Los números** (la solución en Debug, recompilada con `--no-incremental` sobre las fuentes finales):
- **UI completa:** 3110 correctas, 0 con error y 48 omitidas (3158), las mismas: estas correcciones no tocan la UI. `AnswerForInsert`
  está en Application y se prueba en Core.
- **Core Full:** 19529 correctas, 0 con error y 403 omitidas (19932). Son las 19496 de `dc0ada4c` más las 33 de
  `InsertRedrawCompletenessTests`; `PushBackCabeceraInsertedViewDesignTests` queda igual (−2 +2).
- **Plugin Debug:** 0 errores. Las advertencias MSB3277 de las referencias de AutoCAD ya estaban.

**Lo que queda abierto:**
- **Pregunta para el Owner (preexistente, no se tocó):** con un redibujo fallido, la ventana da el borrador por escrito si se redibujó alguna
  vista (`AnswerForApply` y `AnswerForInsert` cuentan las redibujadas, no las que faltan). La línea de órdenes da el motivo. Con
  «Actualizar» pasa lo mismo. Cambiarlo tocaría lo que la ventana da por escrito en los cuatro sistemas.
- **El smoke en AutoCAD de estas correcciones:**
  - el escenario B: la única lateral obsoleta, e «Insertar» la planta, una vez colocándola y otra cancelando el jig;
  - quitar el lado B con una frontal de B en el dibujo, con «Actualizar» y con «Insertar»;
  - un redibujo fallido no se puede provocar a mano: queda cubierto por las pruebas de la regla y de la cola real.

## Sexta tanda, pb: los defectos del diálogo clásico de seguridad de Push Back (PB-S17)

- **Orden del Owner (2026-11-27):** «Arregla los defectos, aún no hay que borrar el editor clásico, […] hay que arreglar lo de pushback y
  hazme las preguntas requeridas». El editor clásico se queda: sus defectos se corrigen en él.
- Sobre `f02f519b`, en la rama local `architecture/workspace-persistente-rackcad-t6-pb`, sin push y sin AutoCAD. El commit lo hizo la
  sesión (`2f1e9e4e`); las correcciones de su revisión, al final de esta sección, van sobre él.
- De los seis puntos de PB-S17 (subsección «PB-S03: la seguridad poste por poste»), el **5** y el **4** son defectos del clásico y se
  arreglaron en él. El 1, el 2, el 3 y el 6 son diferencias de diseño y no se tocaron: quedan como preguntas (abajo).

### Punto 5: «Auto» en una cara de un rack de dos sentidos (el defecto principal)

- **El defecto, reproducido:** en un Push Back de dos sentidos, volver a «Auto» la defensa de un poste en la rejilla de un lado no hacía
  nada si la otra cara de ese poste ya era automática. La decisión anterior, una medida o «apagado», se quedaba.
- **Causa:**
  - `SafetyDefensaGridWindow.BuildFaceResult` quitaba de su resultado la fila que quedaba automática en las dos caras.
  - Ese resultado no sustituye la lista: `PushBackDefenseSection.Configure` lo funde por su cara (`PushBackDefenseSides.Merge`,
    `PushBackDefenseSection.cs:217`), y la fusión solo escribe los postes que vienen en él. El poste ausente conservaba su cara de antes.
- **Arreglo:** `BuildFaceResult` ya no quita esas filas (`SafetyDefensaGridWindow.cs:552-556`, marcado `T6 pb`).
  - La fusión escribe el «Auto» y después retira las filas automáticas en las dos caras (`PushBackDefenseSides.cs:177`), así que lo
    guardado queda igual de limpio.
  - El modo de una cara solo lo abre la sección de defensa de un Push Back de dos sentidos. Selectivo, Dinámico y el Push Back de un
    sentido usan el «Aceptar» de dos extremos, que no cambió.
  - La ventana ya escribía esto: ahora el clásico escribe lo mismo que la ventana.
- **Prueba** (UI, WPF real en STA: la ventana clásica, «Elementos de seguridad» y la rejilla real de la sección; solo se sustituye el
  `ShowDialog`): `PushBackClassicSafetyDefectsTests.AutoAgainOnTheFaceOfASideWhoseOtherFaceIsAutomaticGoesBackToTheRule`.
  - Un compuesto de tres ranuras. En la defensa del lado A, el poste 2 apagado: queda un registro propio y la planta cambia.
  - Otra vez, «Auto» en ese poste: no queda ningún registro y la planta dibuja lo de antes.
  - Sobre la base falló: el registro `{PostIndex 1, ExitAuto false, ExitLength 0}` seguía.

### Punto 4: los niveles del desviador en un rack de dos sentidos

- **El defecto, reproducido:**
  - La rejilla del desviador recibía los niveles por poste del lado ACTIVO (`RackPushBackSystemWindow.DesviadorLevelsPerPost`).
  - El desviador es por poste (PB-002), y el dibujo lo pone en las camas de los DOS lados (`PushBackDiverterPlan.Resolve`), con una sola
    celda apagada por poste y nivel.
  - Abierta desde el lado más bajo, la rejilla no ofrecía los niveles que solo tiene el otro: un desviador dibujado no se podía apagar, y
    «Aceptar» perdía una celda apagada allí (una columna más corta no es dormida, `SafetyDormantCells`).
  - Lo escrito dependía del lado activo, aunque I-42 (ronda 7D) fijó que «Elementos de seguridad» ofrece lo mismo desde A que desde B.
- **Por qué es un defecto y no una diferencia de diseño:** el comentario de PB-002 en el propio método dice que la cuenta es la misma regla
  que usa el dibujo. El defecto que PB-002 corrigió es exactamente ese: celdas que el usuario ve dibujadas y no puede apagar. En un rack
  de dos sentidos el dibujo usa los dos lados.
- **Arreglo:** con lado B, poste a poste el mayor de los dos lados (`RackPushBackSystemWindow.xaml.cs:1486`, marcado `T6 pb`), como
  `PushBackDraftEditor.DesviadorLevelsPerPost` de la ventana. Un rack de un sentido sigue con el código de antes.
- **Prueba** (UI, STA): `PushBackClassicSafetyDefectsTests.TheDesviadorGridOfATwoSidedRackHasTheLevelsTheDrawingPlacesFromEitherSide`.
  - El frente 2 del lado B con un cuarto nivel; el lado A con tres. El dibujo pone un desviador en N4 de los postes 2 y 3.
  - La cuenta del clásico desde A es la del dibujo, y la misma que desde B.
  - La rejilla real, abierta como la abre «Configurar…» desde A, ofrece la celda N4 del poste 2 y conserva su «apagado» al aceptar.
  - Sobre la base falló: `[3, 3, 3, 3]` desde A, frente a `[3, 4, 4, 3]` del dibujo.

### La transcripción `Classic` y las pruebas de la ventana

- `PushBackSafetyEditTests.Classic` sigue al clásico corregido:
  - `FaceGrid` ya no quita la fila automática en las dos caras;
  - la rejilla del desviador toma sus niveles de `Levels`, la transcripción de `DesviadorLevelsPerPost` corregido.
- La prueba que medía la pérdida (`BackToAutoOnAFaceWhoseOtherFaceIsAutomaticIsWrittenWhereTheClassicGridLosesIt`) pasa a
  `BackToAutoOnAFaceWhoseOtherFaceIsAutomaticIsWrittenAsTheCorrectedClassicGrid`. La ventana y el clásico escriben lo mismo en la cara de
  A, en la de B y con «Todas automáticas», que antes solo se medía en la ventana.
- Nueva: `TheDesviadorOfATwoSidedRackWithMoreLevelsOnSideBIsWrittenAsTheCorrectedClassicFromSideA`, sobre el rack del punto 4. Desde A,
  la celda N4 del poste 2 se apaga igual en los dos, y apagar después otra celda desde A la conserva.
- La ventana no cambia de conducta. Sus comentarios (`PushBackSafetyEdit`, `PushBackDraftEditor.Seg`) ya no declaran estas dos
  diferencias, y `RackCadPushBackEditorTests` (con `.Seg`) sigue en verde.

### Los otros puntos de PB-S17: diferencias de diseño, para el Owner

No se tocaron. Ninguno pierde lo que el usuario decidió.
1. **Una familia por edición.** El «Aceptar» del clásico (`Safety_Click`, `RackPushBackSystemWindow.xaml.cs:2609-2622`) vacía la lista y
   la reconstruye con todo lo que muestra el diálogo: es cómo trabaja un diálogo modal. Lo que reescribe es lo que el diálogo muestra.
2. **El orden de la lista.** El clásico deja primero las familias del diálogo, después la defensa (`:2937`) y después las botas (`:2743`).
   Cada familia tiene una sola selección, y el dibujo y el BOM la buscan por su tipo: el orden no cambia nada visible.
3. **Una familia que se queda sin nada.** El clásico no escribe el protector lateral sin ningún poste que dibuje
   (`SelectiveSafetyWindow.cs:983-988`) ni el desviador con todas sus celdas apagadas (`:893-894`). El dibujo y el BOM son los mismos. Es
   el diálogo compartido de Selectivo, Dinámico y Push Back: cambiarlo tocaría los tres. La ventana conserva la variante (C2-07).
4. **(Punto 6) Un desviador nuevo.** El clásico solo lo escribe tras «Configurar…» (`SelectiveSafetyWindow.cs:893`, con el botón
   «Configurar…» a la vista, `:637-642`), como el tope y la parrilla del mismo diálogo, desde `a9f1c137`. La ventana lo escribe al elegir la
   variante. También es del diálogo compartido.

### Supuestos para el Owner

| Id | Supuesto | Dónde |
|---|---|---|
| T6pb-S1 | En el clásico de un Push Back de dos sentidos, «Auto» en un poste de la rejilla de un lado devuelve esa cara a su automático aunque la otra cara ya lo sea. Si las dos quedan automáticas, el registro del poste sale de lo guardado. Antes se quedaba la decisión anterior. **Con los dos lados en «Ninguno»** (revisión de `2f1e9e4e`, M3; medido): si la última decisión propia vuelve a «Auto», ese «Aceptar» deja la familia DEFENSA sin registros (la sección del otro lado se abrió con el registro y lo cuenta como intención), y el «Aceptar» siguiente la quita de la lista. Es lo mismo que hace la ventana, y no cambia ni el dibujo ni el BOM (con los dos lados en «Ninguno» no se dibuja defensa) | `SafetyDefensaGridWindow.BuildFaceResult`, `EnsureDefenseSelection` |
| T6pb-S2 | En el clásico de un Push Back de dos sentidos, la rejilla del desviador muestra en cada poste los niveles del lado más alto, se abra Seguridad desde A o desde B. Desde el lado más bajo aparecen celdas nuevas (las que el dibujo ya ponía), y una celda apagada allí ya no se pierde al aceptar. **La columna «dormida»** (revisión de `2f1e9e4e`, M2; medido): si en un poste el lado activo tiene en blanco sus frentes vecinos y el otro lado tiene b > 0 niveles, la columna ya no está dormida, y una celda apagada guardada en un nivel ℓ ≥ b se pierde al aceptar (antes se conservaba desde el lado activo, y se perdía desde el otro). Hoy no cambia el dibujo; al reactivar el frente, el desviador de ese nivel vuelve a dibujarse. La ventana hace lo mismo. El efecto bueno: «todo apagado», que decide si la familia se escribe, se mide ahora sobre el máximo de los dos lados | `RackPushBackSystemWindow.DesviadorLevelsPerPost` |

### Comprobación en rojo

- Script `hs65\t6\pb\red\red.sh` del scratchpad de la sesión, con su salida en `red-out.txt` y `red-log.txt`.
- Antes de romper, las copias originales y las corregidas se guardaron FUERA del worktree. Cada pieza pone la copia rota, compila, corre la
  clase focal y restaura la corregida.
- Las cuatro volvieron con el mismo SHA-256, y en `git diff` no quedó ningún resto (`if (false)`, `true ||`).
- Cada pieza hizo fallar su prueba, y solo esa:
  - R1, `BuildFaceResult` de `f02f519b`: falla la del punto 5 (UI).
  - R2, `DesviadorLevelsPerPost` de `f02f519b`: falla la del punto 4 (UI).
  - R3, la transcripción `FaceGrid` sin corregir: falla `BackToAutoOnAFaceWhoseOtherFaceIsAutomaticIsWrittenAsTheCorrectedClassicGrid`
    (Core).
  - R4, la transcripción de los niveles con el lado activo: falla
    `TheDesviadorOfATwoSidedRackWithMoreLevelsOnSideBIsWrittenAsTheCorrectedClassicFromSideA` (Core).

### Archivos

- Cambiados, marcados `T6 pb`:
  - `src/RackCad.UI/SafetyDefensaGridWindow.cs` (`BuildFaceResult`);
  - `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (`DesviadorLevelsPerPost`);
  - solo comentarios: `src/RackCad.Application/VentanaRackCad/PushBackSafetyEdit.cs` y `PushBackDraftEditor.Seg.cs`;
  - `tests/RackCad.Tests/Workspace/PushBackSafetyEditTests.cs` (la transcripción y las pruebas).
- Nuevo: `tests/RackCad.UI.Tests/PushBackClassicSafetyDefectsTests.cs`.
- No se tocó ningún archivo compartido de la ventana (`RackCadWindowViewModel*`, `RackCadWindowView*`), ningún censo y ninguna guarda.

### Números

Sobre las fuentes de `2f1e9e4e`, solo la rama pb (los de las correcciones, en su apartado):
- **Core Full:** 19484 correctas, 0 con error, 403 omitidas (19887), compilado con `--no-incremental`. Son las 19483 de la base más la
  nueva de `PushBackSafetyEditTests`; la reescrita no añade casos.
- **UI completa:** 3112 correctas, 0 con error, 48 omitidas (3160). Son las 3110 de la base más las 2 de `PushBackClassicSafetyDefectsTests`.
- **Solución en Debug** (Plugin incluido), con `--no-incremental`: 0 errores. En el Plugin, las dos MSB3277 de siempre
  (`Microsoft.VisualBasic` y `System.Drawing`).

### Correcciones de la revisión (sobre `2f1e9e4e`)

La revisión independiente no encontró nada bloqueante ni importante: seis puntos menores, en el mismo worktree y sobre el commit de la
sesión. La sesión hace el commit.

1. **M1 (latente): la fusión de las secciones de defensa parte de una lista vacía.**
   - `ApplyDefenseSections` fundía las dos secciones sobre `Copy(selection.DefensaPosts)`. Hoy esa lista siempre está vacía, porque el
     «Aceptar» del diálogo no devuelve DEFENSA. Pero si un «Aceptar» la conservara (la parte seg lo cambia en paralelo para conservar
     familias), un poste que una sección devolvió a «Auto» recuperaría su cara de antes: el defecto del punto 5 por otro camino.
   - Ahora parte de `new List<SafetyPostDefense>()`, como la ventana (`PushBackSafetyEdit.WriteDefense`)
     (`RackPushBackSystemWindow.xaml.cs:2884-2887`, marcado `T6 pb`). Las dos secciones ya traen todos los registros del rack en rango,
     cada una con su cara, así que hoy el resultado es el mismo. La transcripción `Classic.Defense` dice lo mismo.
   - Prueba UI `TheSectionsAreMergedFromNothingSoADialogThatKeepsTheDefenseDoesNotBringTheOldFaceBack`. El poste 2 del lado A se apaga.
     Después, «Auto» en él con un `SafetyDialog` que devuelve la lista tal como llegó, con DEFENSA y su registro viejo. No queda ningún
     registro, y la planta es la de antes.
2. **M2: el caso de la columna «dormida», en T6pb-S2.**
   - Declarado en la tabla de supuestos: la celda apagada por encima de los niveles del otro lado se pierde, y «todo apagado» se mide
     sobre el máximo de los dos lados.
   - Medido en Core con `AColumnDormantOnTheActiveSideTakesTheLevelsOfTheOtherSideAndDropsAnOffCellAboveThem`, contra la transcripción
     corregida: la ventana y el clásico escriben lo mismo, la celda N4 del poste 1 se va, y al reactivar el frente su desviador vuelve.
3. **M3: los dos lados en «Ninguno», en T6pb-S1.**
   - Medido con `WithBothSidesInNoneTheLastOwnDecisionBackToAutoTakesTheFamilyOutAsTheWindow`, en la ventana y en el clásico. La familia
     no sale en el mismo «Aceptar»: queda sin registros, porque la sección del otro lado se abrió con el registro y lo cuenta como
     intención. Sale en el «Aceptar» siguiente.
   - No se dibuja ni se cuenta nada en ningún paso.
4. **M4: el `Result` de la rejilla de defensa.** Su comentario dice ahora que, en el modo de una cara, trae todos los registros, también
   las filas automáticas en las dos caras, y que hay que fundirlo por la cara (`PushBackDefenseSides.Merge`), no sustituir la lista con él.
5. **M5: la ruta real del desviador y la cara B.**
   - Había una forma razonable sin un seam grande. `SelectiveSafetyWindow.EditDesviador` tiene ahora el seam `DesviadorDialog`, como los
     de la defensa y del tope del mismo diálogo (`SelectiveSafetyWindow.cs:755` y `:774`, marcado `T6 pb`): recibe la rejilla real y
     devuelve su `BuildResult`, o null para cancelar. Sin el seam, la rejilla se muestra como siempre. Es la única edición del diálogo
     compartido, y no cambia su conducta.
   - La prueba del punto 4 recorre ahora la ruta entera: la ventana clásica, «Elementos de seguridad» y «Configurar…» del desviador (el
     botón real), la rejilla que construye `EditDesviador`, «Aceptar» y el dibujo.
     - Desde A, la rejilla ofrece los niveles del dibujo `[3, 4, 4, 3]`. Se apaga la celda N4 del poste 2: el desviador de N4 del poste 2
       sale del dibujo (`[3, 3, 4, 3]`), y el del poste 3 se queda.
     - Otro «Aceptar» desde A sin tocar nada la conserva.
     - Desde B, la rejilla es la misma.
   - La prueba del punto 5 es ahora una `Theory` con la cara A y la cara B.
6. **M6: la transcripción `Classic.FaceGrid`** comprueba ahora la longitud de una cara encendida y no automática (> 0), como
   `BuildFaceResult` y como `TwoEndGrid`. Ninguna prueba escribe una longitud no válida en una cara, así que no tiene pieza en rojo:
   modela el rechazo de la rejilla.

**Comprobación en rojo:**
- Cinco piezas, una a una, con `hs65\t6\pb\rev\red2.sh` del scratchpad de la sesión; su salida está en `red2-out.txt` y `red2-log.txt`.
- El script guarda antes una copia FUERA del worktree, compila, corre la clase focal y restaura. Las piezas volvieron con el mismo
  SHA-256, y en `git diff` no quedó ningún resto.
- Cada pieza hizo fallar sus pruebas:
  - V1, la fusión otra vez sobre los registros de la selección: falla la de M1 (UI).
  - V2, `DesviadorLevelsPerPost` con el lado activo: falla la del punto 4 por la ruta real (UI).
  - V3, `BuildFaceResult` quitando otra vez la fila automática en las dos caras: fallan 3 (UI): la del punto 5 en la cara A y en la cara
    B, y la de M1.
  - V4, la transcripción `FaceGrid` sin corregir: fallan 2 (Core): la del punto 5 y la de M3.
  - V5, la transcripción de los niveles con el lado activo: fallan 2 (Core): la del punto 4 y la de M2.

**Archivos de las correcciones:**
- `RackPushBackSystemWindow.xaml.cs` (`ApplyDefenseSections`);
- `SafetyDefensaGridWindow.cs` (el comentario de `Result`);
- `SelectiveSafetyWindow.cs` (el seam `DesviadorDialog`, marcado `T6 pb`);
- `PushBackSafetyEditTests.cs` (la transcripción y dos pruebas nuevas);
- `PushBackClassicSafetyDefectsTests.cs` (la prueba de M1, la del punto 4 por la ruta real y la `Theory` del punto 5).

**Números** (sobre las fuentes finales, solo la rama pb):
- **Core Full:** 19486 correctas, 0 con error, 403 omitidas (19889), compilado con `--no-incremental`. Son las 19484 de `2f1e9e4e` más
  las 2 nuevas de `PushBackSafetyEditTests` (M2 y M3).
- **UI completa:** 3114 correctas, 0 con error, 48 omitidas (3162). Son las 3112 de `2f1e9e4e` más 2. `PushBackClassicSafetyDefectsTests`
  pasa de 2 casos a 4: la `Theory` del punto 5 (A y B), la de M1 y la del punto 4 por la ruta real.
- **Solución en Debug** (Plugin incluido), con `--no-incremental`: 0 errores. En el Plugin, las dos MSB3277 de siempre.

## Sexta tanda, lib: los campos desconocidos al insertar desde la biblioteca (UT-S10, CM-S13)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-lib`, sobre `f02f519b` (worktree `D:\w6lib`). La sesión hizo el commit de
esta parte: `39addd8f`. Las correcciones de su revisión (abajo) van sobre él, sin commit. Sin push y sin AutoCAD. No se tocó el formato del
larguero (parte lg) ni código clásico.

**El defecto.** Un Dinámico, un Push Back o una línea Cantilever abiertos de la biblioteca en la ventana («Abrir» de «Biblioteca de
diseños», `OpenDesignAsNewRack`) e insertados con «Insertar en el dibujo…» llegaban al dibujo sin los campos JSON que esta versión no
conoce y con la versión de esquema de esta versión. El clásico abierto desde la biblioteca sí los lleva (I-11, matriz B5):
- su ventana da el proyecto del archivo (`SourceProjectToInsert`) a la petición de inserción (`EditorModules.cs` 85, 124 y 162);
- el despacho de «Nuevo» se lo pasa a `RackViewBatchProducts` (`RackMenuCommands.cs` 174, 216 y 229);
- y este escribe el diseño interior de cada vista nueva con `WithSourceMetadataFrom` de ese proyecto (`RackViewBatchProducts.cs` 68, 96
  y 140).

**La causa.**
- Las tres peticiones del rack nuevo de la ventana se hacían con `sourceProject: null`: `DynamicDraftOutcome.NewRackRequest`,
  `PushBackDoorInsertion.NewRackRequest` y `CantileverDraftOutcome.NewRackRequest`.
- Las escrituras de la puerta (`RackDoorWrite`, `PushBackDoorWrite`, `CantileverDoorWrite`) no tenían cómo llevar el origen.
- La ventana sí guardaba el proyecto del archivo (`_librarySources`, desde UT-04), pero solo lo usaba para «Guardar en biblioteca…» y,
  en la cama, para insertar (`SourceDesignJsonOf`, CM-S13).

**Sistema por sistema** («anidado» = la carga del proyecto; se conserva solo donde I-11 la conserva):

| Sistema | Qué lleva el bloque | El clásico desde la biblioteca | La ventana, antes | La ventana, ahora |
|---|---|---|---|---|
| Dinámico | el proyecto | la raíz (campos desconocidos y versión); `DynamicSystem` no conserva campos propios (límite de I-11) | nada | lo mismo que el clásico |
| Push Back | el proyecto | la raíz y la carga `PushBack` (sus campos y su versión) | nada | lo mismo que el clásico |
| Cantilever | el proyecto | la raíz y la carga `Cantilever` | nada | lo mismo que el clásico |
| Cama | su `FlowBedDocument` | la carga `FlowBed`; la raíz no tiene dónde ir | lo mismo (CM-S13) | sin cambio |
| Selectivo | su `SelectivePalletDesignDocument`, no el proyecto | nada | nada | sin cambio (Owner: «Dejarlo así») |
| Cabecera | el proyecto | la raíz, si el archivo la trae | no se abre en la ventana: va a su clásico por RACKBIBLIOTECAABRIR | sin cambio |

- **Selectivo, por qué no es un defecto de la ventana.** La ventana hace lo mismo que el clásico, y I-11 no lo promete:
  - `SelectiveInsertionRequest` no tiene origen (`RackInsertionRequest.cs` 263);
  - el clásico abierto de la biblioteca guarda el proyecto solo para «Guardar en biblioteca» (`RackSelectiveWindow.xaml.cs` 3322-3330
    y 3358) y dibuja desde el dominio (`RackMenuCommands.cs` 190-202);
  - el bloque lleva un `SelectivePalletDesignDocument` nuevo (`RackViewBatchProducts.cs` 35 y 47), no el proyecto: la raíz del archivo
    no tiene dónde ir;
  - y el contrato de I-11 lo excluye: «El interior del selectivo es un `SelectivePalletDesignDocument` (no es uno de los cuatro límites;
    solo su envelope se preserva)» (`docs/initiatives/I-11-persistencia-uniforme.md` 215-216 y 241-247).
  - Desde I-47, ese documento tiene `ExtensionData` (`SelectivePalletDesignDocument.cs` 138-151), pero para el diseño del dibujo; la
    biblioteca es «un artefacto DERIVADO» (I-47 G15, `RackSelectiveWindow.xaml.cs` 3348). UT-S10 decía «Es una diferencia declarada con el
    clásico»: para el Selectivo era inexacto.
- **Cabecera:** la ventana no la abre (`OpensHere`, `RackCadWindowViewModel.Utilities.cs` 102-106). Va a su clásico, que da el proyecto
  a la inserción (`EditorModules.cs` 207, `RackMenuCommands.cs` 160-161). Una cabecera guardada con `RackFrameProjectStore` queda fuera de
  I-11 por decisión del Owner (`docs/automation/decisions/I-11.md`).

**El arreglo** (como la cama: el documento del diseño de la biblioteca viaja en JSON con la escritura de la puerta).
- **`RackLibrarySourceDesign`** (Application, archivo nuevo):
  - `Of(project)` es el JSON del proyecto del archivo tal como el almacén lo vuelve a escribir: la raíz con sus campos desconocidos y su
    versión, sin degradarla, y la carga de Push Back y de Cantilever con los suyos;
  - `Read(json, kind)` es el proyecto que el clásico da al despacho; si no es un proyecto legible de ese sistema, null, y el rack se
    escribe como antes.
- **Las escrituras** ganan `SourceDesignJson`, opcional y al final, como `CamaDoorWrite`: `RackDoorWrite`, `PushBackDoorWrite` y
  `CantileverDoorWrite`.
- **La ventana** lo llena solo para un rack nuevo que se abrió de la biblioteca (`LibrarySourceDesignOf`). Mandan null:
  - un rack del dibujo («Actualizar», «Insertar vista…»);
  - un borrador de «Nuevo»;
  - una copia de un borrador («Duplicar»).
- **Las tres `NewRackRequest`** ganan `sourceDesignJson`, opcional, y lo leen con `Read`: la petición lleva el proyecto, como la del clásico.
- **El Plugin:**
  - RACKDINAMICONUEVO pasa el JSON a `NewRackRequest`; la puerta lo guarda en `PendingWrite`;
  - RACKPUSHBACKNUEVO y RACKCANTILEVERNUEVO también lo pasan; su puerta ya guardaba la escritura entera (`with`).
  - El despacho (`RackMenuCommands.DispatchInsertion`, `RackViewBatchProducts`) no cambia: es el del clásico.

**Ediciones a archivos compartidos**, marcadas `T6 lib`, de una o dos líneas cada una:
- UI:
  - `RackCadWindowViewModel.cs` (`WriteDynamic`), `.PushBack.cs` (`WritePushBack`) y `.Cantilever.cs` (la escritura de una línea): el
    argumento `SourceDesignJson:`;
  - `RackCadWindowViewModel.Utilities.cs`: `LibrarySourceDesignOf`, nuevo;
  - `IDynamicEditOutcome.cs`, `ICantileverEditOutcome.cs` y `PushBackDoorInsertion.cs`: el parámetro.
- Application: `RackCadRackDoor.cs`, `PushBackDoor.cs` y `CantileverDoor.cs`, el miembro nuevo.
- Plugin:
  - `RackCadRackDoorPort.cs`: `PendingWrite` y la rama del rack nuevo;
  - `RackCadRackDoorCommands.cs`, `RackCadPushBackDoorCommands.cs` y `RackCadCantileverDoorCommands.cs`: el argumento.
- Pruebas: `RackCadUtilitiesTests` y `RackCadCantileverWindowTests` pasan a `partial`; lo nuevo va en sus `.Lib.cs`.
- Ningún censo ni guarda cambia: no hay órdenes nuevas.

**Supuestos** (prefijo T6lib-S, para el Owner):

| Id | Decisión o supuesto | Dónde se cambia |
|---|---|---|
| T6lib-S1 | Cambia lo que se guarda en el dibujo, no lo que se ve. Un Dinámico, un Push Back o una línea Cantilever de la biblioteca, insertados desde la ventana, conservan los campos desconocidos y la versión de su archivo, como en el clásico (B5). UT-S10 queda resuelto para esos tres | `RackLibrarySourceDesign`, `LibrarySourceDesignOf` |
| T6lib-S2 | El Selectivo no cambia: ni el clásico ni la ventana llevan algo del archivo al dibujo (arriba). **Decidido por el Owner (2026-11-27): «Dejarlo así»** | — |
| T6lib-S3 | Solo el borrador que abrió «Abrir» de la biblioteca lleva su archivo. Su copia con «Duplicar» no lo lleva, ni al insertarse ni al «Guardar en biblioteca…». Es así en todos los sistemas, también en la cama (CM-S13). **Decidido por el Owner (2026-11-27): «No, la copia es nueva»**; lo fijan pruebas desde las correcciones de la revisión | `_librarySources`, `DuplicateCore` |
| T6lib-S4 | El origen viaja como copia en JSON, igual que en la cama, y no como el objeto: lo que espera en la puerta es de la puerta (M-22). El Plugin lo lee de nuevo con el almacén | `RackLibrarySourceDesign.Read` |

**Las pruebas:**
- UI (`RackCad.UI.Tests`), 5 nuevas. El archivo de la biblioteca trae un campo desconocido y la versión 2.7 en la raíz, y un campo
  desconocido en su carga (con la versión 1.9 en Push Back y Cantilever):
  - `RackCadUtilitiesTests.Lib.cs`:
    - un Dinámico, por «Abrir» de la página y «Insertar en el dibujo…»: la escritura lleva el diseño; la petición de RACKDINAMICONUEVO
      lleva el proyecto; el diseño interior es el del clásico, con la raíz y sin el anidado;
    - un Dinámico nuevo de «Nuevo» no lleva nada;
    - una cabecera de la biblioteca va a su clásico por la puerta numerada.
  - `RackCadPushBackEditorTests.Lib.cs`: lo mismo con un Push Back (la raíz y la carga `PushBack`). Ya en el dibujo, su «Actualizar»
    no manda origen.
  - `RackCadCantileverWindowTests.Lib.cs`: lo mismo con una línea Cantilever (la raíz y la carga `Cantilever`). Una línea nueva de
    «Nuevo» no manda origen.
- Core (`RackLibrarySourceDesignTests`), 3 nuevas:
  - el viaje, y el diseño interior igual al del clásico, con el origen y sin él;
  - nada viaja sin diseño, y no se lee el de otro sistema ni lo que no es un proyecto;
  - por la fuente: las tres órdenes NUEVO pasan el JSON a `NewRackRequest`, y la puerta del Dinámico lo guarda.

**Comprobación en rojo:** doce piezas rotas a propósito, una a una, con un script (`hs65\t6\lib\red\` del scratchpad de la sesión).
- Antes de romper nada, cada archivo se copió fuera del worktree. Después de cada prueba se restauró con sus bytes (SHA-256 comprobado) y
  con la hora de ahora, para que el build siguiente lo compilara.
- En cada caso falla al menos una prueba, y en el rojo solo fallan las pruebas de esta parte:
  1. R1, la conducta de la base: la ventana no manda el diseño (`LibrarySourceDesignOf` da null). Fallan 3 de UI: Dinámico, Push Back y
     Cantilever.
  2. R2: `RackLibrarySourceDesign.Read` no devuelve el proyecto. Fallan 3 de UI y 2 de Core.
  3. R3: RACKDINAMICONUEVO no pasa el JSON. Falla 1 de Core (por la fuente).
  4. R4: la puerta no guarda el JSON del Dinámico nuevo. Falla 1 de Core.
  5. R5: RACKPUSHBACKNUEVO no pasa el JSON. Falla 1 de Core.
  6. R6: RACKCANTILEVERNUEVO no pasa el JSON. Falla 1 de Core.
  7. R7, R8 y R9: cada `NewRackRequest` ignora el JSON. Falla la prueba de UI de su sistema.
  8. R10, R11 y R12: cada escritura de la ventana (`WriteDynamic`, `WritePushBack`, la de la línea) no lo lleva. Falla la prueba de UI de
     su sistema.
- Después: ningún resto en `git diff` (`if (false)`, `true ||`, `=> null;`, `SourceDesignJson: null`). Ningún carácter de control en los
  archivos cambiados. La solución se recompiló con `--no-incremental` antes de las suites completas.

**Los números** (suites completas con `--no-build`, sobre la solución recompilada con `--no-incremental`):
- **UI completa:** 3115 correctas, 0 con error y 48 omitidas (3163). Es la base `f02f519b`, 3110/0/48, más las 5 nuevas.
- **Core Full:** 19486 correctas, 0 con error y 403 omitidas (19889). Es la base, 19483/0/403, más las 3 de `RackLibrarySourceDesignTests`.
- **Build Debug de la solución (Plugin incluido),** con `--no-incremental`: 0 errores. Las únicas advertencias son las MSB3277 de las
  referencias de AutoCAD, que ya estaban.

**Lo que comprobará el smoke en AutoCAD:**
- poner en un archivo de la biblioteca de un Dinámico, de un Push Back y de una línea Cantilever un campo desconocido y una versión menor
  superior, en la raíz y en la carga;
- «Abrir» en la ventana e «Insertar en el dibujo…»;
- leer el `Design` del bloque nuevo: conserva la raíz y, en Push Back y Cantilever, la carga, como B5 de I-11.

**Preguntas para el Owner, ya respondidas (2026-11-27):**
1. **Selectivo:** ¿un Selectivo de la biblioteca debe llevar al dibujo los campos desconocidos de su diseño (`SelectivePalletDesignDocument`,
   que los conserva desde I-47)? Sería una promesa nueva, más allá de I-11, en el clásico y en la ventana. La raíz del archivo no tiene
   dónde ir en el bloque.
   - **Respuesta del Owner: «Dejarlo así».** El Selectivo no cambia (T6lib-S2).
2. **«Duplicar»:** ¿la copia de un borrador abierto de la biblioteca debe heredar su archivo de origen (al insertarse y al «Guardar en
   biblioteca…»), en todos los sistemas, también en la cama?
   - **Respuesta del Owner: «No, la copia es nueva».** Queda como está (T6lib-S3), y las pruebas de abajo lo fijan.

### Sexta tanda, lib: correcciones de la revisión (sobre `39addd8f`)

**Sobre** el commit `39addd8f` de la sesión, en `D:\w6lib`. Sin commit, sin push y sin AutoCAD. La revisión independiente no encontró nada
bloqueante ni importante; estos son sus puntos menores.

| Punto | Estado | Qué se hizo |
|---|---|---|
| 1 | CORREGIDO | `RackLibrarySourceDesign.Read` deja una advertencia en el log de I-03 (`RackLog.Warning`, contexto `RackLibrarySourceDesign.LogContext`) cuando recibe un JSON y no lo usa: no se vuelve a leer, lo escribió un MAJOR más nuevo o es un proyecto de otro sistema. El aviso dice el sistema y el porqué. El rack se inserta igual, sin los campos de su archivo, como antes. Sin JSON (un rack que no viene de la biblioteca) no se dice nada. Se eligió el log y no una nota en la respuesta del Plugin: la lectura ocurre en medio de la orden de inserción, que no escribe en la línea de órdenes, igual que el aviso de catálogo vacío de `RackCatalogLoader` (I-03) |
| 2 | CORREGIDO | Las guardas del Plugin ya no son una regex sobre el texto. `CallsOf` toma los argumentos de cada llamada, cortados en sus comas de primer nivel, sobre el código sin comentarios. `Passes` acepta el valor en cualquier posición, por posición o con nombre (la parte pr añadirá `Properties:` a `PendingWrite` y a las tres órdenes NUEVO). No acepta una llamada ni un argumento comentados, ni el valor dentro de otra llamada. Una prueba de la guarda lo fija con código sintético. Guarda nueva: las colas de Push Back (`RequestPushBackUpdate`, `SetPushBackPending`) y de Cantilever (`Copy`, `RequestCantileverUpdate`) guardan la escritura entera de la ventana (una copia con `with` que no da otro valor a `SourceDesignJson`), y las órdenes NUEVO la toman de `pending.Write` |
| 3 | CORREGIDO | Una prueba por sistema de «Abrir → Duplicar → insertar la copia»: la copia manda `SourceDesignJson == null` (T6lib-S3) y el original lo sigue mandando (Dinámico, Push Back, Cantilever y cama). «Actualizar» tras insertar: una prueba nueva en Dinámico, y en Cantilever, dentro de la prueba de la biblioteca (Push Back ya la tenía) |
| 4 | CORREGIDO | El XML doc de `SourceDesignJson` en las tres escrituras dice que aquí lleva el JSON de un `RackProject` y en `CamaDoorWrite` un `FlowBedDocument`. Los tres outcomes usan `using RackCad.Application.VentanaRackCad;` y no el nombre completo. El script del rojo lee los totales en español («Pruebas totales», «Correcto», «Incorrecto»). La sección dice ahora que el commit es `39addd8f` |

**Las pruebas, sumadas:**
- UI: 5 nuevas.
  - `RackCadDynamicEditorTests.Lib.cs` (nuevo, la clase pasa a `partial`): el «Actualizar» de un Dinámico de la biblioteca ya insertado, y su copia;
  - `RackCadPushBackEditorTests.Lib.cs`: la copia de un Push Back;
  - `RackCadCantileverWindowTests.Lib.cs`: la copia de una línea, y el «Actualizar» dentro de la prueba que ya había;
  - `RackCadCamaWindowTests.Lib.cs` (nuevo, la clase pasa a `partial`): la copia de una cama.
- Core, `RackLibrarySourceDesignTests`: pasa de 3 a 5 pruebas y entra en la colección `RackLog`, que se ejecuta en serie porque el log es
  del proceso.
  - la advertencia del log (lo que se dice y lo que no);
  - la prueba de la guarda con código sintético;
  - la guarda de las órdenes, sin depender del orden de los argumentos;
  - la guarda de las colas.

**Comprobación en rojo de las correcciones:** once piezas rotas una a una, con un script (`hs65\t6\lib\red2\` del scratchpad de la sesión).
Se hizo como en el rojo anterior: copia fuera del worktree, restauración con SHA-256 comprobado y hora nueva. En cada caso falla al menos
una prueba:
1. C1 (punto 1): `Read` no escribe la advertencia. Falla 1 de Core (la del log).
2. C2 y C3 (punto 2): el `with` de la cola de Push Back o de Cantilever da `SourceDesignJson = null`. Falla 1 de Core (la de las colas).
3. C4 (punto 2): el argumento de RACKPUSHBACKNUEVO, dentro de un comentario `/* */`. Falla 1 de Core (la de las órdenes).
4. C5 (punto 2): la llamada buena de RACKDINAMICONUEVO solo en un comentario `//`. Falla 1 de Core.
5. C6 y C7: la cola del Dinámico nuevo, o RACKCANTILEVERNUEVO, sin el JSON. Falla 1 de Core.
6. C8 (punto 3): la copia de «Duplicar» hereda el diseño de la biblioteca. Fallan las 4 pruebas de la copia: Dinámico, Push Back,
   Cantilever y cama.
7. C9, C10 y C11 (punto 3): «Actualizar» de un rack del dibujo manda el diseño. Falla la prueba de su sistema.

Después: ningún resto en `git diff` y ningún carácter de control. Todos los archivos cambiados terminan sus líneas en CRLF, sin BOM. La
solución se recompiló con `--no-incremental` antes de las suites completas.

**Los números de las correcciones** (suites completas con `--no-build`, sobre la solución recompilada con `--no-incremental`):
- **UI completa:** 3120 correctas, 0 con error y 48 omitidas (3168). Es `39addd8f`, 3115/0/48, más las 5 nuevas.
- **Core Full:** 19488 correctas, 0 con error y 403 omitidas (19891). Es `39addd8f`, 19486/0/403, más 2: `RackLibrarySourceDesignTests`
  pasa de 3 a 5.
- **Build Debug de la solución (Plugin incluido),** con `--no-incremental`: 0 errores. Las únicas advertencias son las MSB3277 de las
  referencias de AutoCAD y las de los analizadores de xUnit, que ya estaban.

## Sexta tanda, pr: «Propiedades» en los borradores nuevos (VA-S9)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-pr`, sobre `f02f519b` (worktree `D:\w6pr`). Sin commit, sin push, sin AutoCAD.

**La respuesta del Owner** (2026-11-27, sobre VA-S9): «Sí, y se escriben al insertarlo». La pestaña es la de VA-07 (nota del Owner: «Hay que crear
una pestaña en cada rack en donde se puedan escribir las propiedades»).

**Lo que había** (VA-S9, en «Cuarta tanda, va»):
- un borrador de «Nuevo» mostraba la pestaña «Propiedades» de solo lectura, con «Las propiedades de este rack no se leyeron con el editor: siguen
  en RACKPROPIEDADES.»: la ventana nunca le daba una lectura (`RackPropertiesTabViewModel` sin `Load`);
- las cinco escrituras de la ventana enviaban `Properties: null` para un rack nuevo (`!editor.IsNewRack`; en la cama, `IsNewRack ? null : …`);
- la petición nueva de Dinámico que espera en la puerta (`_pendingNew`) no guardaba propiedades, y ninguna orden NUEVO las escribía. No era un
  defecto visible en la base, porque nada las enviaba: era una pieza que la función nueva necesitaba (M-3 de la revisión).

**Lo que se construyó:**
- **La pestaña de un rack nuevo** (`RackPropertiesRules.ForNewRack()`, Application): escribible, vacía, sin huella (no se leyó nada) y marcada
  `NewRack` (campo opcional nuevo, al final de `RackPropertiesRead`).
  - La carga el único punto de entrada de los borradores (`OpenDraftCore` → `LoadNewRackProperties`): cualquier borrador de «Nuevo», con los
    valores por defecto, de la biblioteca o una copia.
  - Los cinco sistemas con borrador de «Nuevo»: Selectivo, Dinámico, Push Back, Cantilever y cama.
  - **Cabecera:** no tiene borrador de «Nuevo» (no hay orden NUEVO de cabecera; «Nuevo › Cabecera en esta ventana» sigue sin construir,
    `RackCadWindowViewModel.Cabecera.cs`) ni pestaña «Propiedades» en su editor: no hay nada que cambiar.
- **Es parte del borrador**, como en un rack del dibujo: lo deja «Modificado» («Sin insertar, con cambios» en la lista de «Nuevo»), «Descartar»
  la vacía, «Cerrar el editor» y «Descartar el borrador» preguntan, D-08 veta el cierre y la poda lo nombra. Mientras AutoCAD inserta, la tabla no
  se edita. La nota de la tabla dice cuándo se escribe (T6pr-S1).
- **Una propiedad no válida apaga «Insertar en el dibujo…»** con el motivo de la pestaña («Propiedades: hay 1 valor no válido.») en los cinco
  editores: `InsertViewBlockedText` de Selectivo, Dinámico, Push Back y Cantilever, y `PrimaryBlockedText` de la cama, que antes la excluía para
  una cama nueva. Sin esto, la inserción habría seguido y la pestaña no habría enviado nada (`BuildWrite` no envía un borrador con problemas).
- **La petición NUEVO lleva la pestaña:** las cinco escrituras de la ventana envían `PropertiesWrite()` también para un rack nuevo. «Insertar
  vista…» de un rack del dibujo sigue sin enviarla (VA-S10, sin cambio). La puerta de Dinámico la guarda ahora en la petición que espera
  (antes no hacía falta: nada la enviaba).
- **La orden NUEVO la escribe tras la inserción, en la misma orden** (un «U», como las APLICAR con VA-07):
  `RackPropertiesDoorData.WithNewRackProperties` (Plugin), en las cinco órdenes (RACKSELECTIVONUEVO, RACKDINAMICONUEVO, RACKCAMANUEVO,
  RACKCANTILEVERNUEVO y RACKPUSHBACKNUEVO), solo cuando AutoCAD colocó el rack:
  1. lee el rack recién insertado por el borde de siempre (`CustomPropertiesExecutor.ReadRack`, la autoridad de RACKPROPIEDADES);
  2. Application decide sobre qué se escribe (`RackPropertiesRules.NewRackFingerprint`): una colección escribible y SIN propiedades; si no, el
     motivo (T6pr-S3);
  3. escribe con `CustomPropertiesExecutor.ExecuteRackDraft`, el borde de VA-07, sin cambios: relee, compara la huella y planea en Application;
  4. escriba o no, vuelve a leer el rack, y la respuesta es la regla de Application (`RackPropertiesRules.AnswerForNewRack`), que lleva lo que
     el dibujo guarda ahora (T6pr-S8).
- **La respuesta en la ventana** (`RackPropertiesTabViewModel.Answered`; en cada editor, `PropertiesWritten` pasa a llamarla, una línea). Para un
  rack del dibujo hace lo de antes. Para un rack nuevo que AutoCAD colocó, la pestaña pasa a ser la del rack del dibujo:
  - escritas: lo que el dibujo guarda ahora, con los ids que acuñaron las altas, limpia; desde ahí, «Actualizar» las escribe como en cualquier
    rack;
  - sin nada que escribir: también toma la lectura del rack, así que se pueden escribir después con «Actualizar»;
  - no escritas, con el rack ya insertado: el aviso dice el motivo y, si el rack las admite, «Siguen en su pestaña «Propiedades»: escríbelas con
    «Actualizar».»; las filas quedan como nuevas sobre lo que el rack guarda y el editor, «Modificado» (T6pr-S4);
  - una respuesta sin lectura: «Las propiedades no se leyeron con el editor: para editarlas aquí, vuelve a leerlo del dibujo.», nunca una base
    que no leyó (texto de las correcciones de la revisión, abajo; antes remitía a RACKPROPIEDADES).
- **«Duplicar»** copia las filas de la pestaña como filas nuevas de la copia (`WithPropertiesOf`, `CopyRowsOf`; T6pr-S7).

**Las decisiones (supuestos para el Owner):**

| Id | Decisión | Por qué |
|---|---|---|
| T6pr-S1 | La nota de la tabla de un borrador: «Nombre y valor de texto. Van al borrador y se escriben al insertarlo con «Insertar en el dibujo…», en todas sus vistas y en la misma orden.» (desde la revisión, M-2, sin «del rack nuevo»: vale para una cama y una línea) | Es la nota de VA-07 para un rack que aún no está en el dibujo |
| T6pr-S2 | Se escriben **después** de la inserción, en la misma orden (un «U» deshace el rack y sus propiedades). Si no se pueden escribir, el rack queda insertado y la respuesta lo dice | Es la regla de «Actualizar» con VA-07 (VA-S11): las propiedades no deshacen la escritura del rack |
| T6pr-S3 | Solo se escriben sobre el rack recién insertado mientras no tiene ninguna propiedad y su colección es escribible. Si el dibujo las tiene en solo lectura (por ejemplo, con un bloque de RackCad que esta versión no interpreta: la regla de RACKPROPIEDADES), no se escriben y se dice por qué | Nunca se escribe encima de lo que el editor no vio |
| T6pr-S4 | Sin escribir con el rack insertado: las filas siguen en su pestaña, como nuevas, y el editor queda «Modificado» para escribirlas con «Actualizar». Si el rack no las admite (solo lectura), la pestaña muestra su motivo y las filas no se conservan; el aviso dice que no se escribieron. **Ampliado en la revisión (M-1):** si además la orden no pudo volver a leer el rack, las filas también se conservan, sin base («No se pudieron volver a leer del dibujo tras la inserción y estas filas no se escribieron. «Actualizar» las escribe solo si en el dibujo sigue sin haber propiedades.»): el editor sigue «Modificado», D-08 veta el cierre, el aviso dice que siguen en la pestaña, y su «Actualizar» las escribe solo donde aún no hay ninguna propiedad. **Owner (2026-11-27): «Insertar y avisar», como está** | Lo mismo que «Actualizar» cuando no las escribe, sobre el rack que ya existe |
| T6pr-S5 | Un borrador con solo propiedades es «Modificado»: veta el cierre (D-08), la poda lo nombra y «Descartar el borrador» pregunta | Son trabajo sin escribir, como el resto del borrador (ND-05 no cambia: sin cambios no veta) |
| T6pr-S6 | Una propiedad no válida apaga «Insertar en el dibujo…» con el motivo de la pestaña | Si no, el rack se insertaría sin sus propiedades |
| T6pr-S7 | «Duplicar» copia las filas como filas nuevas de la copia. **Owner (2026-11-27): «Sí, copiarlas», como está** | La copia es el borrador entero; las propiedades no son del diseño, así que se copian aparte |
| T6pr-S8 | Tras insertar, la orden vuelve a leer siempre las propiedades del rack, también sin nada que escribir | La pestaña del rack insertado se queda con su lectura y «Actualizar» puede escribirla; antes quedaba sin leer hasta reabrirlo |
| T6pr-S9 | Sin cambio, para que se sepa: «Guardar en biblioteca…» de un borrador no guarda sus propiedades | Viven en el sobre del rack, no en el diseño |

**Application (cambios declarados):** `RackCadRackProperties.cs`: `RackPropertiesRead.NewRack` (opcional, al final), `ForNewRack`,
`NewRackFingerprint`, `AnswerForNewRack` y los textos `NewRackWrittenText` y `NewRackKeptText`.

**Las ediciones a archivos compartidos** (pequeñas, marcadas `T6 pr`):
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`: la condición de `Properties:` en `WriteSelective` y `WriteDynamic`.
- `RackCadWindowViewModel.Cama.cs`, `.Cantilever.cs` y `.PushBack.cs`: la misma condición, una línea cada uno.
- `RackCadWindowViewModel.Drafts.cs`: una línea en `OpenDraftCore` (`LoadNewRackProperties`) y la de `DuplicateCore` (`WithPropertiesOf`).
- Los editores: `PropertiesWritten` → `Answered` (Selectivo, Dinámico, cama, Cantilever y Push Back); `OnPropertiesTabChanged` levanta el
  estado de la inserción para un rack nuevo (Selectivo, Dinámico y Push Back; la cama y el Cantilever ya lo levantaban); una línea en el
  motivo de la inserción de cada uno (en la cama, la condición).
- `RackPropertiesTabViewModel.cs`: `IsNewRack`, la nota, `Answered` y `CopyRowsOf`.
- Plugin: `RackPropertiesDoorData.cs` (`WithNewRackProperties`), las cinco órdenes NUEVO (la respuesta pasa por él) y `RackCadRackDoorPort.cs`
  (la petición nueva de Dinámico guarda la pestaña).
- **Archivo nuevo:** `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.NewRackProperties.cs`.
- **No cambian:** el borde (`CustomPropertiesExecutor`), sus guardas, los censos de órdenes (no hay órdenes nuevas), RACKPROPIEDADES ni el
  editor clásico.

**Las pruebas nuevas:**
- Core, `NewRackPropertiesTests` (11): la pestaña de un rack nuevo; el borrador escrito en todas las vistas del rack recién insertado, con los ids
  de Application, y que la escritura de la pestaña, sin huella, nunca pasa el plan de «Actualizar»; los rechazos (el rack ya tiene propiedades,
  vistas distintas, un bloque que no se interpreta, nada leído); la respuesta (sin nada que escribir, escritas, no escritas con y sin conservar,
  no colocado); y, sobre la fuente, que las cinco órdenes NUEVO responden por `WithNewRackProperties` con las propiedades de su petición tras la
  inserción (teoría de 5), que la guarda ve una orden que las olvida, y que la puerta guarda las de un Dinámico nuevo y las escribe por el borde
  en el orden: leer, decidir en Application, escribir, volver a leer, responder.
- UI, `RackCadNewDraftPropertiesTests` (27), con un doble de la puerta de los cinco sistemas; cinco teorías de los cinco sistemas:
  - la pestaña editable, con su nota, en el borrador («Modificado», D-08, «Descartar») y la propiedad no válida que apaga «Insertar en el
    dibujo…» sin enviar nada;
  - «Insertar en el dibujo…» envía las filas sin huella, la pestaña queda bloqueada mientras AutoCAD escribe, y con la respuesta queda la del rack
    del dibujo, limpia, y «Actualizar» escribe sobre esa lectura;
  - las no escritas siguen en la pestaña, «Modificado», y «Actualizar» las envía con la huella de la lectura;
  - sin propiedades no envía nada y la pestaña toma la lectura del rack; sin lectura en la respuesta, no toma ninguna base;
  - «Duplicar» copia las filas, que son de la copia;
  - y dos hechos: un rack que no las admite (solo lectura) y la vista de los cinco editores con WPF real (STA), sin errores de enlace.

**Comprobación en rojo:** diez piezas rotas a propósito, una a una, sobre el código final, con sus pruebas focales tras cada una.
Antes de cada rotura se guardó una copia del original fuera del worktree; después, el archivo se restauró de esa copia con su SHA-256
comprobado (las 17 restauraciones, iguales), y `git diff` quedó sin restos. Script y salida en el scratchpad de la sesión:
`hs65\t6\pr\red.py`, `red-summary-1.txt` y `red-summary.txt`.

1. R1, la ventana no carga la pestaña de un rack nuevo: fallan 22 de las 27 de la UI (pasan las 5 de «sin propiedades», que no la
   necesitan).
2. R2, las cinco escrituras de la ventana sin la pestaña de un rack nuevo: 5, una por sistema.
3. R3, la pestaña sin conservar las filas no escritas: 5.
4. R4, los cinco motivos de la inserción sin la pestaña: 5.
5. R5, la regla de Application escribe sobre un rack nuevo que ya tiene propiedades: 1.
6. R6, RACKPUSHBACKNUEVO sin `WithNewRackProperties` (fuente): 2, su caso de la teoría y la comprobación de la propia guarda.
7. R7, la puerta pierde la pestaña de un Dinámico nuevo (fuente): 1.
8. R8, «Duplicar» sin copiar las filas: 5.
9. R9, la pestaña de un rack insertado sin lectura sigue como nueva: 5.
10. R10, los datos de la puerta sin volver a leer el rack tras insertarlo (fuente): 1.

La primera corrida de R6, R7 y R10 usó el binario de Core compilado con R5 (el script no recompila para una rotura de fuente): cada una tuvo
un fallo de más, el de R5. Se repitieron tras recompilar Core restaurado, con los números de arriba.

**Los números:**
- **UI completa:** 3136 correctas, 1 con error, 48 omitidas (3185). Es la base `f02f519b` (3110 / 0 / 48) más las 27 nuevas.
  - El error es `RackModalHostWindowTests.TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses`, por su temporizador de seguridad (M-21),
    con otras suites de otras partes corriendo a la vez en la máquina (39 min). Sola, la clase pasa 3 de 3.
- **Core Full:** 19494 correctas, 0 con error, 403 omitidas (19897). Es la base (19483 / 0 / 403) más las 11 nuevas.
- **Focales**, antes de la comprobación en rojo (las dos suites completas, que las incluyen, se corrieron después, sobre las fuentes
  restauradas y recompiladas):
  - UI, `RackCadNewDraftPropertiesTests` y `RackCadRackPropertiesTabTests`: 34 de 34;
  - Core, las clases nuevas y las guardas del borde y de las puertas (`NewRackPropertiesTests`, `RackPropertiesRulesTests`,
    `CustomPropertiesEdgeGuardTests`, `CamaDoorTests`, `SelectiveDoorInsertionTests`, `CantileverDraftEditorTests` y
    `RackCadWindowListTests`): 186 de 186.
- **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias MSB3277 de siempre.

**Lo que queda para el smoke en AutoCAD:**
- un Selectivo de «Nuevo» con dos propiedades, insertado con dos vistas: las dos vistas las tienen (RACKPROPIEDADES las muestra), la pestaña queda
  «Limpio» con ellas, y un solo «U» quita el rack y sus propiedades;
- lo mismo con un Dinámico, una cama, un Cantilever y un Push Back;
- con el jig cancelado: nada escrito, el borrador sigue con sus propiedades;
- tras insertarlo, cambiar una propiedad y «Actualizar»: se escribe sobre lo que se leyó;
- (M-5 de la revisión) unas propiedades que no se escribieron al insertar no salen como error en la ventana: el aviso es el de la inserción,
  con «Pero sus propiedades no se escribieron: …», igual que en «Actualizar» (VA-S11). Mirar que se lea bien.

### Correcciones de la revisión (sobre `b447675e`)

Sobre `b447675e` (el commit de la sesión con esta parte), en `D:\w6pr`. Sin commit, sin push, sin AutoCAD.

**Respuestas del Owner (2026-11-27):** T6pr-S4, «Insertar y avisar», como está; T6pr-S7, «Sí, copiarlas», como está. Las dos están en la tabla
de supuestos.

| Punto | Qué pasaba | Qué se hizo |
|---|---|---|
| 1 (importante) | La inserción cancelada (el jig, nada colocado) con filas en la pestaña no tenía prueba | Teoría de los cinco sistemas (`ACancelledInsertionKeepsTheRowsInTheDraftAndTheNextOneSendsThemAgain`): la orden responde «no insertó» (`RackDoorRules.AnswerForNewRack(id, rack, 0, null)`; en la cama, `CamaDoorRules.AnswerForNewCama`). La pestaña sigue `IsNewRack`, editable y con su fila. El borrador sigue «Sin insertar, con cambios» y `UnwrittenWorkOf` lo nombra. Un segundo «Insertar en el dibujo…» reenvía la fila, sin huella. El código ya lo hacía (`Answered` no toca nada sin escritura): la prueba lo fija, y su rotura está abajo (R11) |
| 2, M-1 | Si la orden no podía volver a leer el rack tras no escribir las propiedades (`after == null`), la pestaña quedaba sin lectura, sus filas se perdían y el editor quedaba «Limpio» | Las filas se conservan sobre una pestaña sin base (`RackPropertiesRules.NotReadAfterInsertion()`: escribible, sin huella, con su nota). El editor sigue «Modificado», D-08 veta el cierre y el aviso añade «Siguen en su pestaña «Propiedades»: escríbelas con «Actualizar».». Su «Actualizar» envía las filas sin huella, y la puerta (`RackPropertiesDoorData.WithRackProperties`, la de VA-07) solo escribe sin huella sobre una colección sin ninguna propiedad: la misma regla de la inserción (`NewRackFingerprint`). Con huella, como antes. T6pr-S4, ampliado |
| 3, M-2 | Los rechazos decían «el rack nuevo» también para una cama y una línea Cantilever | Textos neutros: «no se pudieron leer del dibujo.», «el dibujo no admite que se escriban.», «en el dibujo ya hay propiedades que el editor no leyó, y no se escriben encima.» y, en la puerta, «no se encontró en el dibujo tras la inserción.». La nota de la tabla de un borrador tampoco dice ya «del rack nuevo» (T6pr-S1) |
| 4, M-3 | El informe y esta sección presentaban la petición nueva de Dinámico, que no guardaba las propiedades, como un defecto | Precisado arriba: no era un defecto visible en la base, porque nada las enviaba; era una pieza que la función nueva necesitaba |
| 5, M-5 | — | Anotado para el smoke: unas propiedades no escritas tras insertar no salen como error, igual que en «Actualizar» (VA-S11) |
| 6 | «…siguen en RACKPROPIEDADES» remitiría a la propia ventana cuando la parte rl haga que RACKPROPIEDADES la abra | «Las propiedades no se leyeron con el editor: para editarlas aquí, vuelve a leerlo del dibujo.» (`RackPropertiesTabViewModel.NotReadText`), que vale con las dos |

**Ediciones de esta corrección:**
- `RackCadRackProperties.cs` (Application): `NotReadAfterInsertion()` y su texto; los rechazos neutros; la respuesta dice «Siguen…» también sin
  relectura.
- `RackPropertiesTabViewModel.cs`: `NotReadText`, la nota neutra, la nota de una pestaña sin base, y `Answered`, que sin relectura conserva las
  filas sobre `NotReadAfterInsertion()`.
- `RackPropertiesDoorData.cs` (Plugin): en `WithRackProperties`, una escritura sin huella usa `NewRackFingerprint`; con huella, nada cambia. Y
  el texto neutro de `WithNewRackProperties`.
- Las pruebas: `RackCadNewDraftPropertiesTests` (dos teorías de cinco, el texto nuevo de la pestaña sin lectura) y `NewRackPropertiesTests`
  (los textos neutros con una cama y una línea, la respuesta sin relectura, la pestaña sin base y, sobre la fuente, que «Actualizar» decide
  con la regla de Application antes del borde).

**Comprobación en rojo:** cinco piezas más, rotas una a una con el mismo script, que ahora recompila tras restaurar una pieza compilada, así
que una rotura solo de fuente corre sobre binarios restaurados. Las cinco restauraciones salieron con el mismo SHA-256 y `git diff` quedó sin
restos. Salida: `hs65\t6\pr\red-summary-3.txt`.

1. R11, `Answered` vacía la pestaña cuando no se insertó nada: fallan 5, la teoría del punto 1.
2. R12, sin relectura la pestaña pierde las filas (UI): 5, la teoría de M-1.
3. R13, la respuesta sin relectura no dice que siguen (Application): 1.
4. R14, «Actualizar» sin la regla para una escritura sin huella (fuente de la puerta): 1.
5. R15, un rechazo vuelve a decir «el rack nuevo»: 1.

**Los números**, sobre las fuentes finales restauradas y recompiladas:
- **Focales:**
  - UI, `RackCadNewDraftPropertiesTests` y `RackCadRackPropertiesTabTests`: 44 de 44 (las 34 de antes y las 10 nuevas);
  - Core, las mismas siete clases de antes: 187 de 187 (una prueba nueva).
- **UI completa:** 3146 correctas, 1 con error, 48 omitidas (3195): las 3185 de antes más las 10 nuevas. El error es otra vez
  `RackModalHostWindowTests.TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses`, por su temporizador (M-21), con la máquina cargada (53 min).
- **`FullyQualifiedName~RackModalHostWindowTests` sola** (M-21): 3 de 3, antes y después de la UI completa. La salida queda en el scratchpad de
  la sesión: `hs65\t6\pr\modalhost-sola.txt` y `modalhost-sola-2.txt`.
- **Core Full:** 19495 correctas, 0 con error, 403 omitidas (19898): las 19897 de antes más la nueva.
- **Build Debug de la solución (Plugin incluido):** 0 errores; las advertencias MSB3277 de siempre.

## Sexta tanda, bom: el BOM clásico de Selectivo y los medios frentes que no caben (2026-11-27)

**Rama:** `architecture/workspace-persistente-rackcad-t6-bom`, sobre `f02f519b`, sin commit ni push. Sin AutoCAD.

El editor clásico se queda: sus defectos se corrigen en el propio clásico. Hay cuatro hallazgos, en dos familias:

| Defecto | Dónde | Estado |
|---|---|---|
| ARQ-43-10 en el clásico: «Lista de materiales» e «Insertar varias…» cambian la cabecera personalizada guardada | `RackSelectiveWindow` | CORREGIDO en el clásico, no en `EffectiveCustomAt` |
| Dolor 13 / IN-11: el diálogo «Tramos del frente n» acepta un medio frente que no cabe con su poste intermedio | `SelectiveSegmentsWindow` | REAL y CORREGIDO |
| PP-S10: en la ventana, otras ediciones (el peralte de un poste, el larguero, la tarima, la holgura…) dejan sin sitio un medio frente que cabía | `SelectiveEditorViewModel.Apply` | CORREGIDO, para todas las escrituras |
| Las mismas ediciones en el clásico | `RackSelectiveWindow` | NO corregido: pregunta para el Owner (P-1) |

### 1. ARQ-43-10 en el editor clásico

**Causa.**
- `SelectiveCabeceraAuthority.EffectiveCustomAt` (`SelectiveCabeceraAuthority.cs:78-88`) impone en el sitio la profundidad del fondo sobre
  la receta guardada de una cabecera personalizada. La usan:
  - el BOM (`SelectiveBomBuilder.cs:456`);
  - los cortes (`SelectiveLateralBuilder.cs:101` y `:130`);
  - la planta (`SelectivePlantaBuilder.cs:123`).
- El resolvedor pasa al sistema las cabeceras del diseño tal cual (`SelectiveGeometryResolver.cs:190` y `:203`).
  - En el clásico, esas cabeceras son los objetos de su estado: `SelectiveEditorState.BuildDesign` copia las listas, no las cabeceras
    (`SelectiveEditorState.cs:1369-1383`).
- En `RackSelectiveWindow`, dos gestos leían ese sistema:
  - `ShowBom_Click` (`:2471`) cuenta sobre `lastSystem`;
  - `InsertBatch_Click` (`:2554`) arma sus opciones con `Cortes(system)`.

  Los dos cambiaban el estado y, con él, lo que el clásico escribe después: «Actualizar», «Insertar…» y «Guardar en biblioteca».
- **Escenario (OBSERVADO en la base, con la prueba nueva):** dos fondos, y en P1 del fondo 2 una personalizada de 42 in, cuando la regla de
  ese fondo da 34 (`TwoFondosWithACustomCabeceraOutOfItsRule`).
  - Tras «Lista de materiales», el diseño del clásico lleva 34.
  - Con «Insertar varias…» abierto y cancelado, también.
  - Si se acepta «Insertar varias…», se envía `"Depth":34` (INFERIDO: los cortes se leen antes de armar la petición, sobre los mismos
    objetos; la prueba ya falla en el paso anterior).

**Decisión: el arreglo va en el clásico, no en `EffectiveCustomAt`.**
- ARQ-43-10 (`docs/ideas-futuras.md:740`) dice que lo correcto es:
  - imponer la profundidad una sola vez, en la frontera de escritura;
  - dejar el lector puro;
  - y hacer obligatoria la copia profunda en esa frontera.

  Es un rediseño con superficie propia, no un cambio de una línea.
- I-43 lo difirió a propósito, y lo fija una guarda: `SelectiveCabeceraViewPurityTests.EffectiveCustomAt_StillImposesTheFondoDepthInPlace_BecauseArq4310IsDeferred`
  (`:261`). Su comentario dice: «si alguien purifica EffectiveCustomAt, esto avisa».
- El comentario del método promete además conservar la identidad para quien la compare (`SelectiveCabeceraAuthority.cs:71-76`).
- Cambiar el método contradice un contrato decidido y debilita una guarda. Por eso se hizo como en la ventana: el sistema trabaja sobre
  copias propias.

**Arreglo.** Todos los sistemas del clásico pasan por `BuildSystem` (`:2882`): `lastSystem`, «Insertar varias…», «Elementos de
seguridad…», «Actualizar» e «Insertar…». Ahí el sistema recibe una copia propia de cada cabecera personalizada (`OwnCustomCabeceras`,
`:2908`).
- La copia usa el clon canónico (`CloneCabecera` = `RackFrameProjectStore.DeepCopy`, I-17).
- Una cabecera que no es un marco usable se copia por su documento, que es lo que le daría guardarla y abrirla.
- Los lectores siguen imponiendo la regla, ahora sobre la copia: la geometría y las cuentas no cambian.
- `EffectiveCustomAt` no se tocó, y su guarda sigue en verde.

**Supuesto T6bom-S1, para el Owner.**
- El caso: una personalizada fuera de su regla en el fondo 2 o siguientes, como la trae un documento antiguo.
  - **Ahora**, el clásico escribe en todos sus caminos la profundidad guardada (42), como RACKSELECTIVOAPLICAR y la ventana.
  - **Antes** escribía la impuesta (34) en tres casos: después de «Lista de materiales», con «Insertar varias…» y en un rack nuevo con
    cortes. Este último porque `RackViewBatchProducts.SelectiveNew` lee los cortes antes de serializar (`RackViewBatchProducts.cs:32-35`).
- El dibujo y el BOM son los mismos, porque todo lector impone la regla.
- Con esto desaparece la diferencia M-1 declarada en la entrega 2 («Insertar varias…» del clásico escribía 34).

**Pruebas** (UI, WPF real en STA, con el clásico mostrado fuera de pantalla), en `SelectiveClassicCustomCabeceraReadTests`:
- `TheBomOfTheClassicCountsLikeTheWrittenDesignAndChangesNoCustomCabeceraOfIt`:
  - hace clic de verdad en «Lista de materiales»; el diálogo se lee y se cierra desde su propio bucle modal;
  - el diseño sigue con 42 y su JSON no cambia;
  - los componentes que muestra el diálogo son los del BOM del JSON escrito, leído y resuelto otra vez.
- `InsertarVariasOfTheClassicChangesNothingCancelledAndSendsTheDesignTheUserSaw`:
  - «Insertar varias…» cancelado no cambia nada;
  - aceptado con todas las vistas, envía el JSON de antes (42);
  - ninguna personalizada de `SystemToInsert` es un objeto de `DesignToInsert`;
  - el sistema sigue dando 34 como cabecera efectiva.

**Comprobación en rojo.**
- Sin la llamada a `OwnCustomCabeceras` fallan las dos: «Expected 42, Actual 34», y `"Depth":34` en el JSON.
- El archivo se restauró de la copia guardada fuera del worktree, y su SHA-256 coincide.

### 2. El medio frente que no cabe: dolor 13 y PP-S10 son el mismo defecto por dos puertas

**El defecto (OBSERVADO en el dominio).**
- Si al último tramo no le queda sitio, `SelectiveMedioFrente.Resolve` devuelve null y el frente se dibuja entero, sin aviso
  (`SelectiveMedioFrente.cs:71-74`).
  - Pasa igual en el frontal, la planta, el BOM, las parrillas, los topes y los desviadores.
- Cada poste intermedio ocupa `2·(troquelX + inicioX)`: 3.5 in con el catálogo de hoy, y 3.603 in con el larguero «escalón infinito».
- Lo fijan dos pruebas de Core que ya había:
  - `SelectiveSegmentFitTests.ASplitTheClassicDialogAcceptedAndTheResolverDrawsAsTheWholeFrontIsRefused` (`:74`);
  - `TheFrontalPlantsTheIntermediatePostExactlyWhenTheSplitFits` (`:157`), sobre la salida real del frontal.
- El punto 13 del inventario es **real**.

**Las dos puertas.**
- **Dolor 13 / IN-11: la entrada de los tramos en el clásico.**
  - El diálogo solo rechazaba una suma que igualara o superara el frente (`SelectiveSegmentsWindow.cs:276`, en la base).
  - Su propio comentario llamaba «aproximado» al resto en vivo (`:237`).
  - La ventana ya rechazaba ese reparto (DG-10, que el Owner aceptó el 2026-11-26).
- **PP-S10: cualquier otra escritura de la ventana.** Un reparto que cabía deja de caber si crece lo que ocupa el poste o si el frente se
  estrecha. Lo pueden provocar:
  - el peralte propio del poste izquierdo del frente (el troquel se corre con él);
  - el larguero de sus celdas (el arranque de la ménsula);
  - el ancho de la tarima, las tarimas por nivel, la holgura horizontal o un largo manual;
  - el poste, y los fondos o frentes que cambian el ancho.

  T5 pp solo lo comprobaba para el peralte del rack.

**Arreglo de la puerta 1 (el clásico).**
- `SelectiveSegmentsWindow` recibe, como dato opcional, cómo encajan los tramos (`SelectiveSegmentFit.Of`).
- `EditTramos` (`:1000`) se lo da sobre `lastSystem` y sobre los «Fondos destino», que es donde escribe; decide el más justo.
- El resto en vivo descuenta los postes intermedios.
- Al «Aceptar», un reparto que no cabe se rechaza con el mismo texto que la ventana.
- Las comprobaciones de antes se quedan, y sin ese dato el diálogo hace lo de siempre. Por eso no cambian sus pruebas de caracterización.

**Arreglo de la puerta 2 (la ventana).** Es una guarda para todas las escrituras, en el único camino de escritura del editor:
`SelectiveEditorViewModel.Apply`, dos líneas marcadas `T6 bom`. Está en un archivo nuevo, `SelectiveDraftEditor.Splits.cs`.
- `GuardSplits()`, antes de la escritura, anota los medios frentes que caben, con sus tramos, y la última recomposición válida.
- `KeepTheSplits(guard)`, después de recomponer: si uno que cabía ya no cabe con los mismos tramos, devuelve la recomposición de antes y
  da el motivo.
- `Apply` lo trata como una escritura que no se puede recomponer: `RestoreLastValid`, la selección como estaba y el motivo en el campo.
- **No cuentan como motivo:**
  - un reparto que ya no cabía antes, como un dibujo antiguo que se dibuja entero (la regla PP-S8);
  - unos tramos nuevos, porque `SetSegments` los comprueba al escribirlos (DG-10);
  - un frente que desaparece con sus tramos;
  - «Sin medio frente».
- Las dos escrituras de T5 pp conservan su texto propio, porque rechazan antes.

**Supuestos para el Owner.**

| Id | Decisión | Por qué |
|---|---|---|
| T6bom-S2 | El diálogo clásico «Tramos del frente n» rechaza un reparto que no cabe con sus postes intermedios, con el texto de la ventana. Por ejemplo, en un frente de 96 in: «No cabe: los tramos suman 94.25 in y el poste intermedio ocupa ≈ 3.5 in; el frente mide ≈ 96 in y no queda resto para el último tramo.» Si el rack tiene varios fondos, añade «en el fondo n». La línea en vivo descuenta el poste: con 91.5 in dice «Último tramo (calculado) ≈ 1 in del frente de 96 in; el poste intermedio ocupa ≈ 3.5 in.». Antes decía «≈ 4.5 in» | Es DG-10, la regla que el Owner aceptó para la ventana. El clásico la tenía como diferencia declarada (IN-11), y el 2026-11-27 el Owner aprobó «La ventana más estricta que el clásico (PB-S09, CT5-03, IN-11)»: «Sí, más estricta». Este arreglo va más allá en IN-11: iguala el diálogo del clásico a la ventana, solo a la entrada de los tramos (ver las correcciones sobre `9dd5baa5`) |
| T6bom-S3 | En la ventana, una edición que deje sin sitio un medio frente que cabía se rechaza entera, con este texto en el campo: «No se aplicó el cambio: el medio frente de F2 dejaría de caber. No cabe: … Ajusta antes sus tramos en «Matriz» (inspector del frente).». Si son varios, los nombra todos y explica el primero: «… los medios frentes de F1 y F2 dejarían de caber. En F1: No cabe: …» | Es PP-S10. Vale también para «Restaurar layout…»: si lo que se quita (los peraltes propios, un largo manual) deja sin sitio un medio frente que no se quita con «Medio frente» (desmarcado, o en un fondo fuera del alcance), no se restaura nada, y el mensaje dice qué marcar (ver las correcciones sobre `9dd5baa5`) |

**Pruebas.**
- **UI, `SelectiveClassicMedioFrenteFitTests`:** `TheClassicDialogRefusesASplitTheDrawingWouldIgnoreAndAcceptsOneThatFits`.
  - Hace clic de verdad en «Medio frente…» de F1 del clásico.
  - Con medio poste de más:
    - la línea en vivo y «Aceptar» dicen el texto de `RefusalOf`;
    - el diálogo sigue abierto y no se escribe nada.
  - Con una pulgada de resto, lo acepta y lo escribe, y la línea en vivo descuenta el poste.
- **UI, `SelectiveSplitGuardWindowTests`.** Escribe por los campos reales de la ventana, sobre F2 partido con 0.6 in de resto:
  - el peralte propio de P2 a 4 in, en su fila, se rechaza sin dejar nada; 3.5 in sí se aplica;
  - «Ancho de tarima» de F2 de 44 a 43 y «Holgura horizontal» de 4 a 3 se rechazan; 45 sí se aplica;
  - lo que no es motivo: un reparto que ya no cabía, «Sin medio frente» y «Frentes» de 2 a 1.
- **Core, `SelectiveSplitGuardTests` (4):**
  - el peralte propio: vuelve la recomposición de antes, con los mismos objetos `Design` y `System`, y `RestoreLastValid` deja el JSON
    de antes;
  - el larguero «escalón infinito», con 0.05 in de resto: el texto dice «≈ 3.603 in»;
  - la holgura, con dos frentes partidos, que da el texto en plural;
  - los casos que no son motivo.

**Comprobación en rojo.** Las copias de los originales se guardaron fuera del worktree y se restauraron. Se comprobaron con SHA-256 y en
`git diff` no quedan restos.
- **Puerta 1:** con el diálogo de la base y sin el dato en `EditTramos`, falla la prueba del clásico: el diálogo acepta el reparto y se
  cierra.
- **Puerta 2:** sin las dos líneas de `Apply` y con `KeepTheSplits` neutralizado, fallan:
  - 2 de las 3 pruebas de UI, las que rechazan;
  - 3 de las 4 de Core.

  Las que comprueban lo que no es motivo pasan en los dos casos, como deben.

**Dinámico:** no tiene medio frente. Sus `DepthSegments` son los tramos de profundidad de I-42, que el editor no edita, así que no se
tocó.

### 3. Preguntas para el Owner

- **P-1 (el clásico y PP-S10): RESPONDIDA el 2026-11-27, «Dejarlo así».** La edición del clásico no se toca. Va con la decisión del
  mismo día «La ventana más estricta que el clásico (PB-S09, CT5-03, IN-11)», «Sí, más estricta».
- **P-2 (los dibujos antiguos): RESPONDIDA el 2026-11-27, «Sí, avisar al abrir».** Hecho en los dos editores, en las correcciones
  sobre `9dd5baa5`.
- **P-3.** La validación de T6bom-S1 a T6bom-S4.

### Archivos

- **Producto:**
  - `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs`: `BuildSystem`, `OwnCustomCabeceras` y `OwnCopy`, y la llamada al
    diálogo en `EditTramos` (marcado `T6 bom`);
  - `src/RackCad.UI/Systems/Selective/SelectiveSegmentsWindow.cs`: el dato opcional `fit`, la línea en vivo y la comprobación al
    «Aceptar»;
  - `src/RackCad.UI/VentanaRackCad/SelectiveEditorViewModel.cs`: dos líneas en `Apply` (marcadas `T6 bom`);
  - `src/RackCad.Application/VentanaRackCad/SelectiveDraftEditor.Splits.cs`, nuevo.
- **Pruebas nuevas:**
  - `tests/RackCad.UI.Tests/SelectiveClassicCustomCabeceraReadTests.cs` (2, con su clase de apoyo `SelectiveClassicShown`);
  - `tests/RackCad.UI.Tests/SelectiveClassicMedioFrenteFitTests.cs` (1);
  - `tests/RackCad.UI.Tests/Workspace/SelectiveSplitGuardWindowTests.cs` (3);
  - `tests/RackCad.Tests/Workspace/SelectiveSplitGuardTests.cs` (4).
- **No se tocaron** `EffectiveCustomAt`, el Plugin, los censos ni las guardas.

### Números

Fuentes finales, Debug, con las fuentes restauradas tras las comprobaciones en rojo.

- **Focales:**
  - UI, `~Selective|~PostPeralte|~RackCadSelective`: 521 de 521;
  - Core, `~SelectiveSplitGuardTests|~SelectivePeralteForAllPostsTests|~SelectiveSegmentFitTests`: 21 de 21.
- **UI completa:** 3116 correctas, 0 con error, 48 omitidas (3164). Es la base (3110 / 0 / 48) más las 6 nuevas.
- **Core Full:** 19487 correctas, 0 con error, 403 omitidas (19890). Es la base (19483 / 0 / 403) más las 4 nuevas. Se corrió en dos
  partes, que suman toda la suite:
  - todo salvo `WorkspacePilotUpdateIntegrationGuardTests`: 19367 / 0 / 399 (19766), en 22 min;
  - esa clase sola: 120 / 0 / 4 (124), en 33 min.
- **Por qué en dos partes:** con las cuatro partes de la tanda usando la máquina, la suite entera pareció colgarse dos veces en esa clase,
  sin CPU. No estaba colgada, solo era lentísima:
  - cada caso de la clase vuelve a leer los ~1100 archivos de `src`;
  - leerlos costaba entonces 25-37 s (medido);
  - así, cada caso tardaba de 50 a 80 s.

  Antes de cortarlas, las dos corridas completas llevaban 19346 y 19327 correctas, con 0 errores.
- **Build Debug:** la solución (Plugin incluido) compila con 0 errores. Las advertencias son las de antes: las MSB3277 de las referencias
  de AutoCAD y las de los analizadores de xUnit.

### Correcciones de la revisión (sobre `9dd5baa5`)

**Rama:** la misma, sobre `9dd5baa5` (el commit de la sesión con esta parte), sin commit ni push. Sin AutoCAD.

La revisión independiente no encontró nada bloqueante ni importante. Esto atiende sus cuatro hallazgos menores y las respuestas del
Owner del 2026-11-27.

| Punto | Qué pasaba | Qué se hizo |
|---|---|---|
| P-1 (Owner: «Dejarlo así») | La edición del clásico de Selectivo (peralte, larguero, tarima, holgura) puede dejar sin sitio un medio frente que cabía | **No se toca.** La pregunta queda respondida en los términos de la decisión del Owner del mismo día: «La ventana más estricta que el clásico (PB-S09, CT5-03, IN-11)», «Sí, más estricta». Rechazar esas ediciones es la regla de la ventana (T6bom-S3); el clásico no la tiene, a propósito |
| P-2 (Owner: «Sí, avisar al abrir») | Un rack que ya traía un medio frente que no cabe se abría y se dibujaba entero sin que ningún editor lo dijera | Los dos editores lo dicen al abrir, sin cambiar nada. Ver los textos debajo de la tabla y el supuesto T6bom-S4 |
| 1 [MENOR] | Tras un rechazo de la guarda se perdía la política de cotas «cargada»: `RestoreLastValid` llamaba a `LoadState(Design)`, que sobrescribe `_loadedDimensionViews`, y `SetDimensionViews` compara con ella | `RestoreLastValid` la conserva, como ya conservaba `LoadNotice`. Escenario de la revisión (política `null`, desmarcar «Lateral», un peralte rechazado y volver a marcar «Lateral»): el editor queda «Limpio» y sin política, no con `{F,L,P}` explícito |
| 2 [MENOR] | Faltaban pruebas de lo declarado | Pruebas nuevas: T6bom-S3 por «Restaurar layout…»; la ventana con dos fondos («F2 del fondo 2»); en el clásico, «Elementos de seguridad…», «Actualizar» e «Insertar planta». Las dos últimas comparan los cortes, la planta y el frontal de cada fondo de `SystemToInsert` con los de un sistema resuelto otra vez desde el JSON que se escribe |
| 3 [MENOR] | El texto de T6bom-S3 daba por hecho que quitar los peraltes propios no restaura nada, y eso solo vale con «Medio frente» desmarcado, o con el fondo fuera del alcance | S3 precisado (abajo). Si el rechazo sale de «Restaurar layout…», el mensaje añade qué hacer, según el caso |
| 4 [MENOR] | `using` sobrantes en `SelectiveDraftEditor.Splits.cs`; la salida de las corridas en rojo no se guardó | Quitados (`System.Globalization` y `RackCad.Application.Systems.Selective`; el `cref` va calificado). La salida de las corridas en rojo de esta corrección está en la carpeta de la parte, en el scratchpad (`red2\red-run1.txt` y `red2\red-run2.txt`), junto con el guion de sabotaje |

**El aviso al abrir (P-2).**
- **Qué dice:** «El medio frente de F2 no cabe con sus postes intermedios: se dibuja como frente entero.»
  - Con más de un fondo, nombra el fondo: «El medio frente de F2 del fondo 2 no cabe…».
  - Con varios, los nombra todos: «Los medios frentes de F1 y F2 del fondo 2 no caben con sus postes intermedios: se dibujan como frentes
    enteros.»
- **De dónde sale:** de un solo sitio, `SelectiveSegmentFit.OpeningNotice` (Application). Lee el sistema resuelto con la misma regla que
  el dibujo (`SelectiveMedioFrente.Resolve`).
- **En la ventana:** `SelectiveDraftEditor.LoadFrom` lo añade al aviso de la carga (`LoadNotice`). El editor lo muestra al abrir en su
  línea de mensajes, como los demás avisos de la carga. Vale para un rack del dibujo y para un diseño de la biblioteca abierto como rack
  nuevo (`OpenedText`).
- **En el clásico:** `LoadDesign` marca la recomposición de la carga, y `RunRecompute` lo deja en la línea de estado, en rojo, junto a los
  demás avisos de la carga (`LatchWarning`).
  - Solo lo hace al abrir. Las ediciones siguen como estaban (P-1).
- No cambia nada: los tramos se quedan, el editor queda «Limpio» y lo que se escribe es lo leído.

**Los supuestos, corregidos y nuevo.**

| Id | Decisión | Por qué |
|---|---|---|
| T6bom-S2 (precisado) | El diálogo clásico «Tramos del frente n» rechaza el reparto que no cabe, con el texto de la ventana | El 2026-11-27 el Owner aprobó «La ventana más estricta que el clásico (PB-S09, CT5-03, IN-11)», con un «Sí, más estricta». Este arreglo va más allá en IN-11: iguala el diálogo del clásico a la ventana, solo a la entrada de los tramos. Si el Owner prefiere que el clásico siga como antes, basta con no pasar `fit` en `EditTramos` |
| T6bom-S3 (precisado) | En la ventana, una edición que deja sin sitio un medio frente que cabía se rechaza entera, con su texto en el campo | Con «Restaurar layout…», el rechazo solo puede salir de lo que se quita sin quitar los tramos: con «Medio frente» desmarcado, o con el frente en un fondo fuera del alcance (si la restauración sigue al selector de fondos). Entonces el mensaje añade: «Para restaurar igual, marca también «Medio frente»: quita esos tramos.» o «Para restaurar igual, elige «Todos los fondos»: «Medio frente» quita también esos tramos.» |
| T6bom-S4 (nuevo) | El aviso al abrir nombra el fondo solo si el rack tiene más de uno. En la ventana va en la línea de mensajes, con el estilo de los avisos de la carga; en el clásico, en la línea de estado, en rojo | Es la forma de los demás textos de los dos editores. El Owner pidió «frente y fondo»: con un solo fondo, el fondo no distingue nada |

**Pruebas nuevas: 8 de UI y 1 de Core.**
- **`SelectiveSplitGuardWindowTests` (+4):**
  - `WithTwoFondosTheRefusalNamesTheFrontAndItsFondo`;
  - `TheOpeningOfARackWithASplitThatDoesNotFitSaysWhichAndChangesNothing`: con un fondo y con dos, y uno que cabe no dice nada;
  - `RestaurarLayoutThatWouldLeaveASplitWithNoRoomIsRefusedAndSaysToCheckMedioFrente`: P2 tiene un peralte propio de 2 in, y F2 cabe con
    0.6 in. Con «Peraltes propios» solo, se rechaza con el texto y nada cambia. Con «Medio frente» marcado también, se restaura;
  - `ARefusedWriteKeepsThePolicyOfDimensionsThatWasRead` (punto 1).
- **`SelectiveClassicMedioFrenteFitTests` (+1):** `TheClassicSaysAtTheOpeningWhichSplitDoesNotFitAndChangesNothing`, con un fondo, con dos
  y con uno que cabe.
- **`SelectiveClassicCustomCabeceraReadTests` (+3):**
  - `ActualizarOfTheClassicSendsTheDesignTheUserSawWithTheSameGeometry`;
  - `InsertarPlantaOfTheClassicSendsTheDesignTheUserSawWithTheSameGeometry`;
  - `ElementosDeSeguridadOfTheClassicOpenedAndCancelledChangesNoCustomCabecera`.

**Comprobación en rojo** (copias en el scratchpad, restauradas y comprobadas con SHA-256, sin restos en `git diff`).
- **Corrida 1.** Se sabotearon cinco cosas a la vez, cada una con su prueba:
  - sin el aviso de `LoadFrom`;
  - sin el de `RunRecompute`;
  - sin conservar `_loadedDimensionViews`;
  - sin la sugerencia de «Restaurar layout…»;
  - y sin `OwnCustomCabeceras`.

  Fallan 8 de 14:
  - las dos de apertura (ventana y clásico);
  - la de «Restaurar layout…»;
  - la de la política de cotas;
  - y las cuatro del clásico que leen o envían el sistema (BOM, «Insertar varias…», «Actualizar» e «Insertar planta»).

  «Elementos de seguridad…» pasa también en rojo: ese diálogo no lee los cortes ni la planta, así que no impone nada. La prueba guarda que
  siga así.
- **Corrida 2.** Con `KeepTheSplits` neutralizado, fallan 5 de las 7 pruebas de `SelectiveSplitGuardWindowTests`:
  - `WithTwoFondosTheRefusalNamesTheFrontAndItsFondo`;
  - las otras cuatro que esperan un rechazo.

  Pasan, como deben, la de apertura y la de lo que no es motivo.

**Más pruebas.**
- **Core, `SelectiveSplitGuardTests` (+1):** `TheOpeningOfADesignWithSplitsThatDoNotFitSaysWhichAndChangesNothing`. Comprueba el texto en
  plural de `LoadNotice`, que coincide con `OpeningNotice`, y que los tramos se quedan.
- **Su comprobación en rojo** (corrida 3): sin el aviso de `LoadFrom`, falla esa prueba y pasan las otras cuatro.

**Números** (fuentes finales, Debug, restauradas tras las corridas en rojo).
- **Focales:**
  - UI, `~Selective|~PostPeralte|~RackCadSelective`: 529 de 529, que son las 521 de antes más 8;
  - Core, `~SelectiveSplitGuardTests|~SelectivePeralteForAllPostsTests|~SelectiveSegmentFitTests|~SelectiveCabeceraViewPurityTests|~SelectiveDoorInsertionTests`: 53 de 53.
- **UI completa:** 3124 correctas, 0 con error y 48 omitidas (3172). Son las 3116 de `9dd5baa5` más las 8 nuevas.
- **Core Full: no se repitió aquí.** Lo pidió la sesión tras la pausa del Owner, y la correrá ella sobre la rama integrada.
  - Antes de la pausa, la parte sin `WorkspacePilotUpdateIntegrationGuardTests` dio 19368 / 0 / 399 (19767). Es la de `9dd5baa5` más la prueba nueva de Core.
  - La clase lenta no llegó a terminar.
- **Build Debug de la solución (Plugin incluido):** 0 errores. Las 70 advertencias son las de antes: MSB3277 de AutoCAD y los analizadores
  de xUnit.

## Sexta tanda, rl: RLY y RR abren «Almacén»; RACKVARIABLES y RACKPROPIEDADES abren la ventana (2026-11-27)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-rl`, sobre `f02f519b` (worktree `D:\w6rl`). Sin commit, sin push, sin AutoCAD.
Tres encargos: el de la parte (AL-S14, respuesta del Owner «La página «Almacén»») y dos añadidos por el coordinador: A) «La ventana, ya»
para RACKVARIABLES y RACKPROPIEDADES (VA-S2 completo); B) «la ventana recuerda» su tamaño y su posición (A-14 §3). B **no se construyó**: las
guardas de F1-T3B fijan lo contrario (ver «B: lo que lo impide»). No se borró código clásico.

### rl: RLY y RR abren la página «Almacén» (AL-S14)

**Lo que había:** RLY y RR eran órdenes propias (`[CommandMethod("RLY")]`, `[CommandMethod("RR")]`) que llamaban al cuerpo de RACKLAYOUT y de
RACKRELLENAR. No son la misma orden que la completa, así que se cambian solo los atajos (la opción prudente del encargo): RACKLAYOUT y
RACKRELLENAR siguen abriendo sus diálogos clásicos, sin cambios. El menú clásico (RACKMENU) nunca tuvo el layout: el diálogo clásico sigue
por la orden completa y por «Ejecutar» de «Órdenes y atajos», que ejecuta la orden completa.

**Lo que se construyó:**
- **Las órdenes** (`RackLayoutCommands.Ventana.cs`): RLY y RR se mudan a este archivo con `NoUndoMarker | UsePickSet | Redraw` (como RL y RB) y
  llaman a `OpenWarehousePage(Grid | Fill)`:
  1. el rack designado es el de la selección hecha antes del atajo (`SelectedRacks`: cada referencia seleccionada, por los datos de su
     definición, como RACKLAYOUT lee el rack que se elige; `WarehouseLayoutRules.DesignatedSeed`);
  2. abre la ventana (`ShowRackCadWindow`) y le pregunta, durante la llamada, qué leer (`WarehousePageAsk`, evento `WarehousePageAsked`);
  3. lee dentro de su propia orden lo que la página pidió, con la lectura de RACKALMACENLEER (`ReadWarehouse`, `useSelection: true`: si lo
     seleccionado es la planta, es la semilla, AL-S2);
  4. entrega la lectura (`WarehouseShortcutRead`) y deja su línea en la línea de órdenes (`ShortcutCommandLineText`). Ninguna orden sale de la
     ventana: la puerta no recibe ninguna petición.
- **La página** (`WarehousePageViewModel.AskedByShortcut` / `DeliverByShortcut`): toma la pestaña del atajo; el rack designado como semilla o,
  sin designar, la que toma a mano (la que ya tenía; si no, el rack activo con planta; si no, el primero con planta); la capa de sus valores
  y la planta leída antes. La lectura del atajo es orden propia (no deja «Puede no estar al día»), y no toca una lectura o una colocación de
  la página que espera su respuesta. La ventana la muestra sin leer nada (`Shown(mayRead: false)`).
- **La ayuda** («Órdenes y atajos» y RACKAYUDA): los resúmenes de RACKLAYOUT y RACKRELLENAR dicen que el atajo abre «Almacén» y la orden
  completa el diálogo clásico; «En la ventana» de los dos lleva a «Almacén» (antes decía «Solo como orden», que la quinta tanda no actualizó).
  `docs/guias/despliegue.md`, la misma frase en sus dos filas.

**Supuestos para el Owner:**

| Id | Supuesto o decisión | Dónde se cambia |
|---|---|---|
| T6rl-S1 | Solo cambian los atajos: RLY y RR abren «Almacén»; RACKLAYOUT y RACKRELLENAR abren los diálogos clásicos, como antes. Al revés (las completas a la ventana y los atajos al clásico) sería un cambio de dos atributos | `RackLayoutCommands.Ventana.cs` |
| T6rl-S2 | «Designado» = seleccionado en AutoCAD antes del atajo. El atajo no pregunta por la línea de órdenes: sin selección, la página toma su semilla como a mano. Con varios racks seleccionados no toma ninguno y la página lo dice («Hay 2 racks seleccionados en AutoCAD: RLY no tomó ninguno como semilla…») | `WarehouseLayoutRules.DesignatedSeed` |
| T6rl-S3 | El atajo lee siempre, también si la página ya tenía lectura: queda «Lectura al día». A mano, volver a la página no relee | `AskedByShortcut` |
| T6rl-S4 | Mientras la página espera a AutoCAD (su lectura o su colocación), el atajo no lee ni cambia la semilla y lo dice, como «Llevar al almacén…» (corrección 7 de la quinta tanda); sí cambia de pestaña | `AskedByShortcut`, `ShortcutWaitText` |
| T6rl-S5 | RLY y RR ya no avisan de las unidades (I-05) al empezar: no insertan nada. El aviso sigue al «Colocar» (AL-S10) | `OpenWarehousePage` |
| T6rl-S6 | La línea de órdenes: «RackCad: «Almacén» › «Rejilla» en la ventana RackCad: planta de «…» leída, … in, N plantas en el dibujo; sitio …», sin número de petición, tras la línea «ventana abierta» de RACKPANEL | `ShortcutCommandLineText` |
| T6rl-S7 | «Ejecutar» de RACKLAYOUT y RACKRELLENAR en «Órdenes y atajos» sigue ejecutando la orden completa (el diálogo clásico); «En la ventana» lleva a «Almacén», sin elegir pestaña | `RackCadHelpViewModel.WindowOf` |

### A: RACKVARIABLES y RACKPROPIEDADES abren su página en la ventana («La ventana, ya»)

**Lo que se construyó:**
- **RACKVARIABLES y RVA** abren la sección «Variables», como un clic: la primera vez para el dibujo se lee, dentro de la orden; ya leída, no se
  relee; un borrador con cambios se queda (VA-S2 completo).
- **RACKPROPIEDADES y RPR:** el rack seleccionado antes de la orden (uno); si no hay, o hay varios, la misma pregunta del clásico
  («Selecciona un rack o [Proyecto]»). Abren la página del rack y la pestaña «Propiedades» de su editor: con el editor abierto, al momento;
  si no, con «Editar aquí», cuya lectura corre dentro de la orden, y la pestaña cuando respondió. Con «Proyecto», «Variables» › «Propiedades
  del proyecto».
- **El mecanismo** (`RackCadWindowShortcuts`, `RackCadRackDoorPort.Shortcut.cs`, `RackCadWindowViewModel.Shortcuts.cs`):
  1. la orden abre la ventana y le pregunta (`ShortcutAsked`);
  2. la ventana usa sus propios gestos («Editar aquí», la primera lectura de «Variables»), que piden su lectura a la puerta;
  3. mientras la orden pregunta, la puerta guarda esa petición como siempre, pero NO la entrega a AutoCAD: solo anota su orden (`KeptInline`,
     en `Send`; solo las seis lecturas de `RackCadShortcutRules.InlineReads`; cualquier otra petición, escrituras incluidas, se rechaza como
     durante cualquier orden);
  4. la orden corre esa lectura dentro de sí (el mismo método de RACKSELECTIVOABRIR, RACKDINAMICOABRIR, RACKPUSHBACKABRIR,
     RACKCANTILEVERABRIR, RACKCAMAABRIR o RACKVARIABLESLEER), que toma la petición y responde por la puerta;
  5. dice a la ventana que terminó (`ShortcutDone`): la ventana muestra la pestaña.
- **Las ventanas clásicas siguen en el menú clásico:** RACKMENU gana dos botones tras «Abrir de la biblioteca de diseños», «Variables de
  proyecto (ventana clásica)» y «Propiedades personalizadas (ventana clásica)». Son acciones tipadas (`MainMenuAction.ProjectVariablesClassic`,
  `CustomPropertiesClassic`) que el host corre al cerrarse el menú: los mismos bucles de antes, con su nombre nuevo (`RackVariablesClassic`,
  `RackPropiedadesClassic`, sin registrar).
- **«Más…» de «Variables»:** el botón de la ventana clásica pasa a «Abrir el menú clásico (RACKMENU), con la ventana clásica de Variables»
  (antes ejecutaba RACKVARIABLES, que ahora abriría la misma página).
- **La ayuda:** los resúmenes de RACKVARIABLES y RACKPROPIEDADES dicen que abren la ventana y que la clásica sigue en RACKMENU (el de
  RACKPROPIEDADES sigue sin nombrar variables ni fórmulas: su guarda); «En la ventana» de RACKPROPIEDADES: «Página del rack: su pestaña
  «Propiedades»». `docs/guias/despliegue.md`, la fila de RACKVARIABLES.

**Supuestos para el Owner:**

| Id | Supuesto o decisión | Dónde se cambia |
|---|---|---|
| T6rl-S8 | Las cuatro órdenes llevan `NoUndoMarker \| UsePickSet \| Redraw`: no dejan paso en Deshacer (la lectura corre dentro de ellas) y conservan la selección, que designa el rack. Antes no llevaban marcas: escribían en su propio bucle | `RackCadWindowShortcuts.WindowFlags` |
| T6rl-S9 | RACKPROPIEDADES sin selección pregunta lo mismo que el clásico; con varios racks seleccionados, lo dice y pregunta | `RackPropiedades()`, `RackCadShortcutRules.DesignatedRack` |
| T6rl-S10 | Una cabecera suelta no tiene pestaña «Propiedades» en la ventana: la ventana lo dice y remite a la ventana clásica del menú. Un rack que la lista no tiene: «pulsa «Volver a leer la lista» y vuelve a escribir RACKPROPIEDADES» | `ShowRackPropertiesByShortcut` |
| T6rl-S11 | Con otra petición de la ventana en vuelo, RACKPROPIEDADES abre la página del rack pero no su editor, y lo dice; RACKVARIABLES muestra la página sin leerla | `ShowRackPropertiesByShortcut`, `ShowVariablesByShortcut` |
| T6rl-S12 | Las ventanas clásicas, en RACKMENU con dos botones nuevos (el menú clásico no tenía Variables ni Propiedades). «Más…» de «Variables» abre RACKMENU | `RackMainMenuWindow.xaml`, `RackMenuCommands.RackMenu` |
| T6rl-S13 | Dos mecanismos para lo mismo: RLY y RR usan su pregunta y su entrega propias (`WarehousePageAsk`); RACKVARIABLES y RACKPROPIEDADES, la lectura guardada en la puerta. El segundo sirve también para «Almacén» (RACKALMACENLEER sería una lectura más de la lista); unificarlos es un cambio pequeño, para la planeación | — |

**Guardas reapuntadas, con su nota (ninguna otra comprobación se quitó):**
- `CustomPropertiesCommandGuardTests` (I-54 G7): las dos expresiones del atributo de RACKPROPIEDADES y RPR, y el original de tres mutaciones,
  llevan ahora `RackCadWindowShortcuts.WindowFlags`. Siguen: los dos únicos registros, el alias que llama al mismo método, la pregunta, la
  palabra clave, el bucle del Rack y del Proyecto por el borde, una sola lectura de la elección, la ventana dos veces, nada de autoridad,
  bloques, barrido ni transacciones en el archivo.
- `ProjectVariablesWorkspaceTests.GUARDA_EXISTE_UN_COMANDO_PERMANENTE`: el registro de RACKVARIABLES lleva las marcas; además, el bucle clásico
  sigue en el archivo y RACKMENU lo corre. Las demás guardas de G16 pasan sin cambios (el bucle clásico es el mismo).
- `PushBackEditAllNameWritersGuardTests.CT_PBT1B_15…`: el censo de los archivos del Plugin que nombran PushBack pasa de 19 a 20 con
  `RackCadWindowShortcuts.cs` (corre RACKPUSHBACKABRIR dentro de RACKPROPIEDADES); no escribe ningún `Name`: las siete asignaciones y su orden
  no cambian. Lo encontró la Core Full.
- El censo de órdenes no cambia (70; ningún nombre nuevo: RLY y RR cambian de archivo, como RL y RB en BM-11). El censo de la ayuda tampoco
  (los mismos pares orden/atajo). Las de las carpetas de la ventana pasan: la parte nueva de la puerta no bloquea, no abre transacciones ni
  entrega órdenes; las órdenes y su ayudante viven en la raíz del Plugin.

### B: «la ventana recuerda» su tamaño y su posición — no construido, lo que lo impide

La A-14 §3 (2026-11-23) aprueba que la ventana recuerde su tamaño, su posición y su estado acoplado o flotante. Las guardas de F1-T3B
(`WorkspacePaletteHostSourceGuardTests`, INV-F1-T3B-02/06 y F1-T3B-HOST-UI-C1) fijan exactamente lo contrario, y cualquier forma de construirlo
las rompe:
- `ThePaletteIsBuiltWithOnlyTheTitleRackCadAndNoToolIdentifier` y `TheFirstPlacementAddsNoPersistenceIdentity…`: la paleta se construye con
  `new PaletteSet("RackCad")`, sin `Guid`, y no tiene identidad persistida. El mecanismo propio de AutoCAD (que guarda tamaño, posición y
  acople en el perfil) necesita un GUID estable;
- `ThePaletteHasNoLoadNorSaveHandlers`: sin `Load` ni `Save`;
- `ThePaletteHostNeverReadsOrWritesTheDrawingNorTheCommandLineNorTheDisk` y la misma lista de F1-T3B-HOST-UI-C1: sin `UserSettings`,
  `System.IO` ni registro, así que la configuración de RackCad tampoco vale desde el host (y solo el host puede nombrar `PaletteSet`);
- `RepeatedShowNeverReappliesTheFirstPlacement…`: `Show()` es exactamente `_palette.Visible = true;` y la geometría (`Location`, `Size`, `Dock`)
  solo se toca en la primera colocación; la comprobación de «fuera de la pantalla» al mostrar la rompe;
- `TheFirstPlacementAddsNoPersistenceIdentity…`: exactamente dos suscripciones y nada de `SizeChanged` ni `PaletteSetMoved`.
La regla del encargo es no debilitar ninguna guarda y explicarlo. **Pregunta para la sesión/Owner:** ¿se reapuntan esas guardas a la decisión
de la A-14 §3? La propuesta, lista para construir:
1. `new PaletteSet("RackCad", PaletteId)` con un GUID fijo en el host; AutoCAD guarda y repone tamaño, posición y acople en el perfil, sin
   `Load`/`Save` propios y sin tocar el disco. La primera colocación de hoy queda como la de la primera vez (se aplica antes del primer
   `Visible`; si AutoCAD la repone después, gana lo guardado: lo comprueba el smoke).
2. Al mostrarla, si está flotante y su barra de título no cae en ninguna área de trabajo de los monitores de ahora, vuelve al rectángulo de la
   primera colocación en el monitor principal (una regla pura en Application, con pruebas sin AutoCAD).
3. Las guardas, reapuntadas con la misma dureza: exactamente ese GUID; `Show()` = `Visible` + esa comprobación; la geometría, solo en la
   primera colocación y en esa comprobación; siguen sin `Load`/`Save`, sin disco y con dos suscripciones.
4. Smoke: cerrar AutoCAD con la paleta flotante en el segundo monitor y reabrirla (misma posición y tamaño); acoplada a la izquierda (sigue
   acoplada); desconectar el segundo monitor (aparece en el principal); `UT-S06` («Acerca de» dice que la recuerda) cuando funcione.

### Pruebas

- **Core**, `tests/RackCad.Tests/Workspace/WarehouseShortcutTests.cs` (nuevo, 6): el rack designado (ninguno, uno por cualquier vista y mayúsculas,
  varios con su nota en RLY y en RR); la pregunta; las líneas de órdenes del atajo y la de RACKALMACENLEER, que no cambió al compartir su
  resumen; por la fuente, RLY y RR con sus marcas en `RackLayoutCommands.Ventana.cs` y fuera de los archivos clásicos, RACKLAYOUT y RACKRELLENAR
  como únicos registros de los suyos y con sus ventanas, y el orden de `OpenWarehousePage` (designar, abrir, preguntar, leer con la selección,
  entregar, decir) sin petición de la puerta, sin `SendStringToExecute`, sin escribir y sin el aviso de unidades.
- **Core**, `tests/RackCad.Tests/Workspace/RackCadShortcutTests.cs` (nuevo, 5): las seis lecturas que corren dentro de la orden y nada más (ni
  escrituras ni otras lecturas); el rack de RACKPROPIEDADES y la pregunta del clásico; la línea de órdenes; por la fuente, las cuatro órdenes y
  sus bucles clásicos, RACKMENU que los corre, `Open` (abrir, preguntar guardando, correr, decir), `RunRead` con exactamente esas seis y sus
  métodos, y `Send` que guarda antes de mirar el comando en curso y de entregar.
- **UI**, `RackCadWarehousePageTests` (+8): RLY con un rack seleccionado (pestaña, semilla, una lectura del atajo y ninguna de la puerta, el
  resto de RLY como orden propia, AL-S15); RR con la capa recordada y otra semilla antes; sin selección, la semilla a mano y luego la que tiene
  con su planta; varios racks; la página que espera (en este dibujo y en otro, donde la página mostrada no intenta leer); sin planta y con la
  lectura fallida; la ventana sin la puerta.
- **UI**, `RackCadVariablesPageTests` (+5): RACKVARIABLES (una lectura, la de la orden; después, ninguna); «Proyecto»; RACKPROPIEDADES con el editor
  cerrado («Editar aquí» y la pestaña al responder) y abierto (la pestaña, sin lectura); lo que dice sin rack en la lista, con otra petición en
  vuelo y con la lectura rechazada; la ventana sin la puerta.
- **UI**, `RackMainMenuStructuralSectionTests` (+2 casos): los dos botones clásicos, tras la biblioteca, con su manejador y su acción tipada.
- **UI**, `RackCadUtilitiesTests` (+1): la ayuda de RACKLAYOUT y RACKRELLENAR («En la ventana», el resumen, «Ejecutar» sigue siendo la completa).

### Comprobación en rojo

Dieciséis piezas rotas a propósito, una a una, con script (`hs65\t6\rl\red.py` en el scratchpad de la sesión; salida en `red-out-1.txt` y
`red-out.txt`). Antes de cada rotura, el script guarda una copia del archivo FUERA del worktree (`red-orig`). Después compila, corre la clase de
la pieza y devuelve el archivo a sus bytes, con el mismo SHA-256 (lo comprueba en los dieciséis casos).

Hubo dos pasadas:
- en la primera, cinco piezas no se aplicaron (el script buscaba `\n` en archivos con CRLF) y no guardaba el nombre de la prueba que fallaba;
- en la segunda se aplicaron esas cinco, y A2 otra vez, ya con nombres.
Tras la última, sin restos en `git diff`.

| Pieza | Qué se rompió | Resultado |
|---|---|---|
| R1 | el atajo no toma el rack designado | fallan 3: `RlyOpensRejilla…`, `RrOpensRellenarUnArea…` y `AShortcutWhoseReadingHasNoSeedOrFailed…` |
| R2 | la lectura del atajo no cuenta como orden propia | falla `RlyOpensRejilla…` (el resto de RLY la marcaba «Puede no estar al día») |
| R3 | el atajo lee aunque la página espere a AutoCAD | fallan `WhileThePageWaitsForAutoCad…` y `WhileARequestOfThePageWaitsInAnotherDrawing…` |
| R4 | la página mostrada por el atajo intenta leer (`mayRead: true`) | falla 1 en su clase, la que lo vigila: `WhileARequestOfThePageWaitsInAnotherDrawing…` (primera pasada, sin nombre) |
| R5 | el atajo pide la capa por defecto y no la de los valores | falla 1 en su clase: `RrOpensRellenarUnArea…` (primera pasada) |
| R6 | varios racks seleccionados designan el primero | falla 1 en su clase: `TheSeedIsTheOneRackSelected…` (primera pasada) |
| R7 | RLY sigue llamando a RACKLAYOUT | falla 1 en su clase: `RlyAndRrOpenThePageOnTheirTab…` (primera pasada) |
| R8 | la ayuda no lleva RACKLAYOUT a «Almacén» | falla `TheHelpSaysThatRlyAndRrOpenAlmacen…` |
| A1 | `Send` entrega la lectura a AutoCAD también durante la orden | falla `TheCommandRunsInsideItselfTheRead…` (la de la fuente) |
| A2 | RACKPROPIEDADES no recuerda el rack cuya pestaña mostrar | fallan `RackPropiedadesOpensTheTabPropiedades…` y `RackPropiedadesSaysWhy…` |
| A3 | con el editor abierto no se muestra la pestaña | falla 1 en su clase: `RackPropiedadesOpensTheTabPropiedades…` (primera pasada) |
| A4 | «Proyecto» no elige «Propiedades del proyecto» | falla 1 en su clase: `RackPropiedadesWithProyecto…` (primera pasada) |
| A5 | con otra petición en vuelo no dice por qué | falla 1 en su clase: `RackPropiedadesSaysWhy…` (primera pasada) |
| A6 | cualquier orden corre dentro del comando | falla 1 en su clase: `OnlyTheReadsOfTheWindowRunInsideTheCommand` (primera pasada) |
| A7 | el botón clásico de Variables no pide su acción | falla 1 en su clase: el caso de Variables de `TheClassicWindowsOfVariablesAndProperties…` (primera pasada) |
| A8 | RACKVARIABLES sin las marcas de la ventana | falla 1: la guarda reapuntada `GUARDA_EXISTE_UN_COMANDO_PERMANENTE` (primera pasada) |

### Números

Solución en Debug recompilada con `--no-incremental` sobre las fuentes finales, tras la comprobación en rojo.
- **Build Debug de la solución (Plugin incluido):** 0 errores. Las advertencias son las de siempre (MSB3277 y los analizadores de xunit);
  ninguna sale de los archivos de esta parte.
- **Clases focales:** Core, 451 de 451 (las dos clases nuevas, las guardas reapuntadas y las de órdenes, ayuda, ventana, unidades, paleta y
  BOM). UI, 236 de 236, con 3 omitidas (Almacén, Variables, utilidades, ayuda, menú clásico, diálogos, ventana, BOM y la pestaña «Propiedades»).
- **UI completa:** 3126 correctas, 0 con error, 48 omitidas (3174): la base 3110 / 0 / 48 más las 16 nuevas (8 + 5 + 2 + 1); 36 min, a la vez
  que la Core.
- **Core Full:** 19493 correctas, 1 con error, 403 omitidas (19897); 45 min.
  - El error era el censo de `PushBackEditAllNameWritersGuardTests`, que se reapuntó (ver «Guardas»).
  - Esa clase, otra vez: 1 de 1 (la Core Full no se repitió tras este cambio, que es solo el literal del censo en la prueba).
  - Cuenta final: 19494 / 0 / 403, es decir, la base 19483 más las 11 nuevas (6 + 5).

### Para integrar

- **Archivos compartidos** (cambios marcados `T6 rl`): `RackCadWindowViewModel.cs` (una línea en el constructor, `WireShortcuts()`);
  `RackCadRackDoorPort.cs` (una línea en `Send` y el comentario de la clase); `RackCommandReference.cs` (cuatro resúmenes);
  `RackCadHelpViewModel.cs` (tres entradas de `WindowOf`); `RackMenuCommands.cs` (dos ramas en `RackMenu`); `RackMainMenuWindow.xaml(.cs)` y
  `MainMenuAction.cs` (dos botones y dos acciones); `CustomPropertiesCommandGuardTests.cs` y `ProjectVariablesWorkspaceTests.cs` (las guardas
  reapuntadas); `PushBackEditAllNameWritersGuardTests.cs` (el censo, de 19 a 20: si otra parte añade un archivo del Plugin que nombre PushBack,
  los dos aumentos se suman); `docs/guias/despliegue.md`.
- **Con st:** RLY, RR, RACKVARIABLES y RACKPROPIEDADES solo leen (`NoUndoMarker`), como RL y RB; la señal de «orden ajena» no debería marcar
  los editores por ellas. Las lecturas que corren dentro de RACKPROPIEDADES son las de «Editar aquí», que ya son propias.
- **Con at:** `KeptInline` rechaza cualquier escritura durante estas órdenes, también RACKACTUALIZARTODO.
- **Para el smoke en AutoCAD:** RLY con la planta (y con otra vista) seleccionada, sin selección y con dos racks; RR con una capa recordada; que
  ninguna de las cuatro deja paso en Deshacer y que conservan la selección; RACKPROPIEDADES con un Selectivo, un Push Back y una cabecera, con
  y sin el editor abierto, y con «Proyecto»; RACKVARIABLES la primera vez y la segunda; los dos botones de RACKMENU (las ventanas clásicas
  escriben y un «U» las deshace como antes).

### Sexta tanda, rl: correcciones de la revisión (sobre `2c9345c6`)

Corrige la revisión independiente de la parte y aplica la respuesta del Owner sobre B («Sí, cambiarlas», 2026-11-27). Hecho en `D:\w6rl`, sobre
el commit `2c9345c6` del coordinador. Sin commit, sin push, sin AutoCAD.

**A. «La ventana recuerda» su tamaño, su posición y su acople (A-14 §3; las guardas de F1-T3B, reapuntadas por el Owner)**

- **La identidad:** `new PaletteSet("RackCad", PaletteId)`, con `PaletteId` un GUID constante (`4dc6f19b-296f-424d-8e74-82c7312e2c6a`) que no
  debe cambiar nunca. AutoCAD guarda bajo él, en el perfil del usuario, el tamaño, la posición y el acople, y los repone en la sesión
  siguiente. El host no escribe nada.
- **La incógnita** (¿AutoCAD repone el estado en el constructor o al primer `Visible`?), tratada sin depender de la respuesta:
  - un `Load` mínimo, suscrito justo después de construir la paleta (antes que nada), que solo anota que había estado guardado
    (`_restoredByAutoCad`); sin `Save`;
  - `ApplyInitialPlacement` sigue en el constructor (antes del primer `Visible`) y no coloca nada si llegó ese `Load`.
  - Si AutoCAD repone al primer `Visible`, lo guardado se pone encima de la primera colocación, que queda como la de la primera vez.
  - Si repone antes de `Visible` y avisa con `Load`, la primera colocación no se aplica.
  - **El único caso que no cubre:** AutoCAD repone dentro del propio `new PaletteSet(...)` y lanza su `Load` antes de que exista ninguna
    suscripción. Entonces la primera colocación pisaría lo guardado: se vería en el smoke, porque la paleta no recordaría nada.
  - La vía alternativa, si pasa: colocar en el primer `Show()` después de `Visible`, solo si no llegó `Load`. No la elegí: en el caso de reponer
    al primer `Visible` sin `Load`, pisaría lo guardado.
- **La comprobación de pantalla** (`KeepOnScreen`, desde `Show()` tras `Visible`; SM-03 del Smoke-1 fue una paleta fuera de la pantalla):
  - solo con la paleta flotante;
  - en píxeles de dispositivo: `Location`, `Size` y `Screen.WorkingArea` de cada monitor (`System.Windows.Forms.Screen`), así que los monitores
    con escalas distintas se comparan en las mismas unidades;
  - la regla pura es `RackCadPalettePlacement.KeepOnScreen` (Application):
    - su barra de título (32 px) en un área de trabajo, con al menos 120 px de ancho, y no mayor que esa área: se queda;
    - mayor que el área que tiene su título: se recorta a esa área y se mete dentro;
    - con el título en ningún área (un monitor que ya no está, o que cambió): va al área principal, a 80 px de su esquina, recortada a ella;
  - un fallo se contiene y se registra.
- El Plugin gana `<UseWindowsForms>true</UseWindowsForms>`, para `Screen`; nada más lo usa.
- **Efectos, declarados (T6rl-S16; el caso no cubierto de arriba es T6rl-S17, para el smoke):**
  - AutoCAD guarda ese estado en el perfil del usuario: es por perfil, y queda un resto tras desinstalar RackCad;
  - el GUID es fijo: cambiarlo olvidaría, para todos, dónde estaba la paleta.
  - UT-S06 («Acerca de» no dice que la recuerda) sigue igual hasta que el smoke lo confirme.
- **Guardas reapuntadas en `WorkspacePaletteHostSourceGuardTests`, con la misma dureza, citando la A-14 §3:**
  - `ThePaletteIsBuiltWithTheTitleRackCadAndOneConstantIdentity` (antes «…NoToolIdentifier»): exactamente
    `new PaletteSet("RackCad", PaletteId)` y la constante con ese GUID; dos `Guid` (el tipo y su constructor) y dos `PaletteId` en todo el
    archivo; sin `NewGuid`, `Guid.Parse`, `toolID` ni otra asignación;
  - `ThePaletteHasOnlyTheMinimalLoadThatNotesAndNoSave` (antes «…NoLoadNorSaveHandlers»):
    - una sola suscripción `_palette.Load += OnPaletteLoad;`;
    - el cuerpo exacto del manejador (anotar y contener);
    - una sola asignación de la marca;
    - fuera de ese `Load`, nada de `Load`, `Save`, `Saving`, `PalettePersist` ni `ConfigurationSection`;
  - `TheFirstPlacementAddsNoPersistenceIdentity…` sale solo `\bGuid\b`. `Load` y lo demás siguen prohibidos en el código sin el `Load` mínimo,
    igual que registro, `UserSettings`, disco, perfil, `SizeChanged` y los movimientos. Las suscripciones son exactamente tres;
  - `RepeatedShowNeverReappliesTheFirstPlacement…`:
    - el orden del constructor: construir, `Load` (sin nada entre medias), `AddVisual`, la primera colocación, y después las otras dos
      suscripciones;
    - `Show()` es exactamente `_palette.Visible = true; KeepOnScreen();`, y `KeepOnScreen` se declara y se llama una vez;
    - la geometría solo se toca en la primera colocación y en la regla de pantalla;
    - la regla, en orden: solo flotante, `Location`, `Size`, `Screen` y su `WorkingArea`, la regla pura, y `Size`/`Location` solo si la regla
      lo dice. Nunca acopla, ni toca la colocación ni el mínimo;
  - `TheFirstCreationConfigures…`: además, la primera instrucción de la colocación es `if (_restoredByAutoCad) return;`;
  - `TheHostSubscribesTo…`: tres suscripciones, la tercera el `Load`;
  - `EveryEntryOfThePaletteHost…`: también `OnPaletteLoad` y `KeepOnScreen`, contenidos y registrados;
  - sin cambios: `ExactlyOnePaletteSetIsConstructed…` (`:80-92`), `ThePaletteHostHasNoTimer…` (`:244-252`) y
    `ThePaletteHostPublicSurface…` (`:285-301`).
  - Fuera de ese archivo, la misma decisión movía dos guardas más del host. Las reapunté con su nota, sin quitarles nada:
    - `WorkspaceBrowserHostGuardTests.ThePaletteHostOnlyPassesTheSummaryPort…`: tres suscripciones, una de ellas el `Load` exacto;
    - `WorkspaceNavigationHostTests.ThePaletteHostOnlyGainsTheNavigationPort…`: su lista exacta de `using` gana
      `RackCad.Application.VentanaRackCad` (la regla pura), y tiene tres suscripciones.
  - Los comentarios del host (`WorkspacePaletteHost.cs`, la clase y la primera colocación) y la cabecera del bloque de la prueba, al día.
- **Smoke en AutoCAD:**
  1. la primera vez (perfil sin estado guardado): la paleta sale acoplada a la derecha, 600 × 600, como hoy;
  2. flotante en el segundo monitor, cerrar AutoCAD y abrirlo: misma posición y tamaño;
  3. acoplada a la izquierda: sigue acoplada a la izquierda;
  4. con el segundo monitor desconectado, o con la escala cambiada: aparece en el principal, recortada si hace falta;
  5. guardada más grande que el área de trabajo: se recorta;
  6. comprobar que el estado guardado no lo pisa la primera colocación (el caso no cubierto de arriba).

**B. Hallazgos de la revisión**

| Punto | Qué pasaba | Qué se hizo | Prueba |
|---|---|---|---|
| 1 (importante) | RACKPROPIEDADES ya no ofrecía la ventana clásica para racks que esta sí atendía: un Selectivo con una referencia rota a una variable, o vistas con diseños distintos. El editor de la ventana no se abre, y la orden solo decía «no se abrió el editor» | **T6rl-S14:** en esa rama, la línea de órdenes y el aviso de la ventana añaden «Usa «Propiedades personalizadas (ventana clásica)» del menú clásico (RACKMENU).» (`RackCadShortcutRules.ClassicPropertiesText`) | UI, `RackPropiedadesSaysWhy…` (el texto exacto de la línea de órdenes y del aviso) |
| 2 (importante) | Remedios que mandaban al usuario al mismo sitio donde está | Donde habla la ventana, la clásica de RACKMENU (ver abajo) | — (no había pruebas que fijaran esos textos) |
| 3 | La guarda de I-54 G7 vigila ahora el bucle clásico, no la orden registrada | Una nota en `CustomPropertiesCommandGuardTests`. Guarda nueva `RackCadShortcutTests.TheRackOfTheCommandIsReadOnlyFromTheDataOfItsDefinition`: `RackIdOf` con un único `InDocumentTransaction.Run`, `ForRead`, los datos de la definición y su `catch`; `SelectedRacks` con una transacción y un `ForRead`; ni `ForWrite`, `ForNotify`, `UpgradeOpen`, `Write(`, `AppendEntity`, `Erase(` ni `SetImpliedSelection` en ellos ni en el ayudante | Core, la guarda nueva |
| 4 | El `Refresh()` de la pregunta presentaba «Variables», que se lee sola si está a la vista sin leer. Esa lectura se quedaba la única de la orden, y RACKPROPIEDADES decía «la ventana espera…» aunque la lectura corría dentro | Mientras la orden pregunta (`_shortcutAsking`), `PresentVariables` no lee. RACKVARIABLES llama a la lectura de la página también cuando ya estaba a la vista, y lee como mucho una vez | UI `AnUnreadPageOfVariablesTakesNoReadWhileACommandAsks` |
| 5 | Textos: el nombre de automatización del botón clásico de «Variables», y el paso intermedio de la ventana sin puerta, que ejecutaba RACKVARIABLES y volvía a la propia ventana | El nombre: «Abrir el menú clásico, con la ventana clásica de Variables». El paso intermedio abre RACKMENU (`Tag="ClassicMenu"`, «Abrir el menú clásico») y dice dónde está la ventana clásica | UI `ACardOfNuevoAndAButtonOfAPage…` (el paso ejecuta `ClassicMenu`) |
| 6 | La Core Full no se repitió tras reapuntar el censo | Repetida (ver los números) | — |

**Los remedios del punto 2** (T6rl-S15; marcados `T6 rl`):
- `ProjectVariablesBatch.cs` (dicho dentro de la página «Variables»): «…si sigue igual, revísalo con «Variables de proyecto (ventana clásica)» del
  menú clásico (RACKMENU).»
- `RackPropertiesTabViewModel.ReadOnlyText` (dentro de la propia pestaña): «…siguen en «Propiedades personalizadas (ventana clásica)» del menú
  clásico (RACKMENU).»
  - **Para integrar con pr:** pr cambia también este texto. Mi cambio es solo el final de la frase; se queda la versión de pr si ya nombra
    dónde están.
- `RackProjectionRemedy.cs`:
  - propiedades divergentes o ilegibles: RACKPROPIEDADES (la pestaña «Propiedades» del rack en la ventana) o, si su editor no se abre, la
    clásica de RACKMENU;
  - referencia rota: RACKVARIABLES («Referencias rotas», «Reparar» en la ventana) o la clásica de RACKMENU.
  - Sin acentos ni comillas angulares, como el resto del archivo: `RackProjectionRemedyTests.EVERY_REMEDY_CARRIES_A_MESSAGE_WITHOUT_ACCENTS` lo
    exige, y la primera versión lo rompió.
- `RackSiblingCustomPropertiesGate.cs`: el mismo código `CUSTOM_PROPERTIES_DIVERGENT`, con los dos sitios, también sin acentos.
- Las guías: `docs/guias/variables-y-formulas.md` (RACKVARIABLES abre la sección de la ventana, y la clásica sigue en RACKMENU; «Referencias
  rotas», en las dos) y `docs/guias/vistas-ligadas-y-proyeccion.md` (unificar, en las dos).

**Pruebas nuevas y cambiadas:**
- Core, `RackCadShortcutTests` (+4):
  - la guarda de solo lectura del punto 3;
  - tres de la regla de pantalla: se queda donde está en una pantalla, también entre dos, con 150 px del título en la principal o con su pie
    fuera; vuelve a la principal desde un monitor que ya no está, desde encima de la pantalla o con poco título a la vista, recortada si era
    más grande; recortada dentro de la pantalla que tiene su título; un área vacía y ninguna.
- Core, `WorkspacePaletteHostSourceGuardTests`: las dos renombradas y las reapuntadas de arriba (el número de pruebas no cambia).
- UI, `RackCadVariablesPageTests` (+1, `AnUnreadPageOfVariablesTakesNoReadWhileACommandAsks`); `RackPropiedadesSaysWhy…` comprueba T6rl-S14.
- UI, `RackCadWindowTests.ACardOfNuevoAndAButtonOfAPage…`: el paso intermedio ejecuta `ClassicMenu`.

**Comprobación en rojo:** nueve piezas rotas a propósito, una a una, con la mecánica de `red.py` (`hs65\t6\rl\red2.py` en el scratchpad de la
sesión; salida en `red2-out.txt`). Antes de cada rotura, una copia del archivo fuera del worktree; después, la compilación, la clase de la
pieza y la restauración con el mismo SHA-256, comprobada en los nueve casos. Sin restos en `git diff`.

| Pieza | Qué se rompió | Falla |
|---|---|---|
| K1 | la primera colocación sin su compuerta `_restoredByAutoCad` | `TheFirstCreationConfiguresAnExplicitDeviceIndependentPlacement…` |
| K2 | `Show()` sin `KeepOnScreen()` | `RepeatedShowNeverReappliesTheFirstPlacement…` |
| K3 | el `Load` que no anota solo la marca | `ThePaletteHasOnlyTheMinimalLoadThatNotesAndNoSave` |
| K4 | la regla deja una paleta mayor que su pantalla | `APaletteLargerThanTheScreenThatHoldsItsTitleIsCutToItThere` |
| K5 | la regla deja una paleta fuera de las pantallas | `APaletteOffTheScreensGoesBackToThePrimaryOneCutToIt` |
| K6 | RACKPROPIEDADES sin remitir a la clásica (T6rl-S14) | `RackPropiedadesSaysWhyWhenTheRackCannotBeOpened` |
| K7 | «Variables» lee sola mientras la orden pregunta | `AnUnreadPageOfVariablesTakesNoReadWhileACommandAsks` |
| K8 | RACKVARIABLES no lee la página ya a la vista | `AnUnreadPageOfVariablesTakesNoReadWhileACommandAsks` |
| K9 | `RackIdOf` abre la referencia para escribir | `TheRackOfTheCommandIsReadOnlyFromTheDataOfItsDefinition` |

**Los números** (sobre las fuentes finales, tras la comprobación en rojo):
- **Build Debug de la solución (Plugin incluido)**, con `--no-incremental`: 0 errores. Las advertencias son las de siempre (MSB3277 y los
  analizadores de xunit); ninguna sale de los archivos de esta corrección.
- **Clases focales:**
  - Core: 822 de 822, con 6 omitidas. Incluye:
    - las guardas del host (paleta, navegación, navegador, aviso de cierre, piloto, órdenes y puente);
    - `RackCadShortcutTests`, `WarehouseShortcutTests` y `RackProjectionRemedyTests`;
    - las guardas de I-54 G7 y de G16;
    - las de Variables, la lista de la ventana, el censo de Push Back, el de órdenes y el menú clásico.
  - UI: 162 de 162 (Variables, la ventana, la pestaña «Propiedades», Almacén, utilidades y el menú clásico).
- **Las suites completas no se corrieron:** por orden del coordinador, tras la pausa que pidió el Owner, para no saturar la máquina. Las corre
  la sesión sobre la rama integrada, también el censo de Push Back reapuntado antes, que quedaba por confirmar en la Core Full.
  - Las cuentas esperadas: UI 3127 / 0 / 48 (3126 + 1) y Core 19498 / 0 / 403 (19494 + 4).
- Una Core Full y una UI completa empezadas antes de la pausa se detuvieron sin resultado.

## Sexta tanda, seg: los diálogos clásicos de seguridad guardan como la ventana (2026-11-27)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-seg` (worktree `D:\w6seg`), sobre `f02f519b`. Sin commit, sin push y sin
AutoCAD. Respuesta del Owner del 2026-11-27, literal: «Guardarla como la ventana». Los dolores 3, 4 y 6 del inventario del grupo
«seguridad» (`protos\seguridad\inventario.md`). El editor clásico se queda: el arreglo está en el propio diálogo clásico.

**El defecto.** El «Aceptar» del diálogo clásico «Elementos de seguridad» (`SelectiveSafetyWindow.OnOk`, el de RACKEDITAR de Selectivo,
de Dinámico y de Push Back) descartaba sin avisar lo que la ventana guarda. Líneas de la base `f02f519b`, en
`src/RackCad.UI/SelectiveSafetyWindow.cs`:
- **Dolor 3, elegir la variante no bastaba:**
  - el tope sin «Configurar…» (`if (row.TopeConfigured)`, :843) y el desviador sin «Configurar…» (`row.DesviadorConfigured`, :886);
  - la bota, cuyo lado general parte de «Ninguno» (:512), sin cambiarlo y sin postes (`drawsSomewhere`, :976-982);
  - el protector lateral de Selectivo sin «Por poste…» (el mismo `drawsSomewhere`; sus extremos solo los sembraba «Por poste…», :595).
- **Dolor 4, todo apagado quitaba la familia,** con su variante y sus medidas: `AllCellsOff` del tope (:846), de la parrilla (:868), del
  desviador (:887) y de la guía (:945); `drawsSomewhere` de la defensa (:932-935) y de la bota y el protector lateral (:976-982).
- La ventana guarda en todos esos casos: `RackCadSafetyEdit.Fresh` y `PushBackSafetyEdit.SetVariant` para una familia nueva, y C2-07,
  T4S-10, T4S-11 y PB-S17 3 para una familia sin nada. Por eso, además, RACKEDITAR sobre un rack que la ventana dejó así le quitaba la
  familia con solo «Aceptar».

**El arreglo** (solo `SelectiveSafetyWindow.cs`):
- `OnOk` ya no pide «Configurar…» al tope ni al desviador, ni quita una familia que no dibuja nada. Solo «Ninguno» de su lista la quita.
- Un desviador nuevo sin «Configurar…» se escribe como su rejilla lo acepta sin tocar: los valores de su fila y el lado que la rejilla
  devuelve. En Push Back, sin selector, es el del extremo bajo (`SafetyDesviadorGridWindow.LowEndSide`, PB-003), como escribe la ventana.
- `TakeDefaultsWhenChosen` (nuevo): cuando la lista de una familia pasa de «Ninguno» a una variante y su fila no tiene nada propio, la
  fila toma, a la vista, lo que la ventana da a una familia nueva:
  - la bota, el lado general «Ambas»;
  - el protector lateral de Selectivo, los dos extremos (`OrillaDefaults`), y su botón dice «Por poste (2)…».

  Las demás familias ya tenían sus valores por defecto en la fila. Cambiar de una variante a otra no toca la fila, como `SetVariant` de la
  ventana.
- La parrilla no cambia en el dolor 3: no tiene lista de variante en el clásico (no es una familia exclusiva), así que su «Configurar…» es
  lo que la elige. Una vez elegida, todas sus celdas apagadas la conservan.
- Un seam de prueba nuevo, `DesviadorDialog`, con el patrón de `TopeDialog`: sustituye solo la modalización de la rejilla del desviador.
- Sin uso, sale el `using RackCad.Application.Systems.Dynamic`. El comentario de la clase `RackCadSafetyEdit` (Application) ya no dice que
  el clásico quita la familia.

**Dolor 6 (Dinámico pierde una defensa igual a la automática): ya no pasa; no se tocó nada.**
- La cuarta tanda (T4S-14) abrió la rejilla de defensas de Dinámico con la columna «Auto» (`SelectiveSafetyWindow.EditDefensa`,
  `autoPerEnd: true`, :713 de la base). Con ella, la rejilla guarda siempre un extremo propio (`var stores = autoPerEnd || …`,
  `SafetyDefensaGridWindow.cs:682-684`).
- La heurística que lo perdía queda solo para `autoPerEnd: false`, que ya nadie pasa: las dos construcciones de la rejilla
  (`SelectiveSafetyWindow.EditDefensa` y `RackPushBackSystemWindow.ShowDefenseDialog`) pasan `true`. No se borró: es de la parte pb.
- Una prueba lo deja medido: en P3, un poste de orilla, «Auto» desmarcado guarda sus 12 in como propios, y con un frente más siguen siendo
  12 (`Dinamico_an_own_defense_equal_to_the_automatic_one_is_kept_by_the_classic_grid`, verde también en la base).

**Lo que el clásico guarda ahora y antes descartaba:**

| Sistema | Caso | Lo que guarda «Aceptar» |
|---|---|---|
| Selectivo, Dinámico | Bota elegida sin tocar su lado | La variante, con lado y colocación «Ambas» (el lado pasa a «Ambas» al elegirla) |
| Selectivo, Dinámico | Bota con el lado general en «Ninguno» y sin postes, o con todos en «Ninguno» | La variante, con lado «Ninguno»: no dibuja |
| Selectivo | Protector lateral elegido sin «Por poste…» | La variante, en P1 «Izquierda» y en el último poste «Derecha» |
| Selectivo | Protector lateral sin ningún poste | La variante, sin postes: no dibuja |
| Dinámico, Push Back | Protector lateral con todos los postes en «Ninguno» | La variante, con esos postes: no dibuja |
| Selectivo | Tope elegido sin «Configurar…» | Uno central compartido, «Ambas», saque 3 in, sin frontal, todas las celdas |
| Selectivo | Tope o parrilla con todas las celdas apagadas | La variante y sus medidas: no dibuja ni cuenta |
| Selectivo, Dinámico | Desviador elegido sin «Configurar…» | 18 in, primer nivel a 18 in, todas las celdas, «Ambas» (Dinámico: por poste) |
| Push Back | Desviador elegido sin «Configurar…» | Lo mismo, en el lado del extremo bajo (Izquierdo), como su rejilla |
| Selectivo, Dinámico, Push Back | Desviador con todos sus niveles apagados | La variante y sus medidas: no dibuja ni cuenta |
| Dinámico | Guía con todas las celdas apagadas | La variante: no dibuja ni cuenta |
| Dinámico | Defensa con todos sus extremos apagados | La variante, con sus registros: no dibuja ni cuenta |

Dejan de ser diferencias declaradas: la del clásico en C2-07, T4S-10 y T4S-11, y las 3 y 6 de PB-S17 (lo demás de PB-S17 es de la parte pb).

**Los supuestos** (para la validación del Owner):

| Id | Supuesto | Por qué |
|---|---|---|
| T6seg-S1 | En el clásico, elegir la variante de la bota (Selectivo y Dinámico) pone su lado general en «Ambas», a la vista; la del protector lateral de Selectivo pone los dos extremos, y su botón dice «Por poste (2)…». Solo al pasar de «Ninguno» a una variante con la fila vacía: cambiar de una variante a otra no toca la fila, y si la fila ya tenía algo (tras «Ninguno» y otra vez una variante en el mismo diálogo) se queda lo que muestra. **Revisión de 794a599c:** en Selectivo, elegir el protector lateral lo pone en los dos extremos y **quita las botas de esos dos postes**, igual que la ventana (el protector lateral reemplaza las botas de su poste). Antes, sin «Por poste…», el clásico no guardaba nada; ahora **cambian el dibujo y el BOM** del clásico | Que lo que «Aceptar» guarda esté a la vista antes de aceptar. Es lo que la ventana da a una familia nueva (`Fresh`) |
| T6seg-S2 | Un tope o un desviador elegidos sin «Configurar…» se guardan con los valores de la rejilla aceptada sin tocar. El botón sigue diciendo «Configurar…» hasta que se abre | La ventana los guarda igual; el texto del botón no cambió |
| T6seg-S3 | Una familia sin nada (celdas, niveles, postes o extremos apagados) conserva su variante y sus medidas en el clásico. No se dibuja ni se cuenta nada, como antes. Solo «Ninguno» de su lista la quita | La respuesta del Owner y C2-07 |
| T6seg-S4 | La parrilla, que en el clásico no tiene lista de variante, se sigue eligiendo con su «Configurar…». Con todas sus celdas apagadas se queda y no se dibuja (el Owner: «Sí, como la ventana»). **Revisión de 794a599c:** se quita con un botón nuevo, «Quitar», junto a su «Configurar…» (T6seg-S6). **Con dos parrillas en el catálogo** el dibujo y el BOM toman la PRIMERA de la lista (`SelectedOfType`, `SelectiveBomBuilder.AddParrillaComponents`): una apagada que va antes taparía a otra que dibuja. Por eso una parrilla con todo apagado solo se guarda mientras ninguna otra del diálogo dibuja. Hoy el catálogo tiene una sola, `PARRILLA_GENERICA` | La respuesta del Owner; que una parrilla apagada no cambie lo que dibuja otra |
| T6seg-S5 | **Revisión de 794a599c (el Owner: «Solo lo que dibuja»).** El botón «Elementos de seguridad (n)…» de los editores clásicos de Selectivo y Dinámico cuenta solo las familias que ponen algo en el dibujo (`SelectiveSafetyWindow.PlacesSomething`). No cuentan una familia con todas sus celdas, niveles, postes o extremos apagados, ni un tope en «Ninguno». Los niveles son los que el diálogo abre: los de la última resolución en Selectivo (ahora el número se recalcula también al resolver), y los del editor en Dinámico. Una familia sin regla de dibujo, solo con cantidad, sigue contando por su cantidad; hoy el catálogo no tiene ninguna. Lo que se guarda no cambia: el filtro del diseño (`SafetyDraws`, `DynamicEditorSafety.Draws`) sigue igual | La decisión del Owner |
| T6seg-S6 | **Revisión de 794a599c.** Dos textos del diálogo de Selectivo: el botón nuevo «Quitar» de la parrilla (tooltip: «Quita la parrilla del rack al «Aceptar». «Configurar…» la vuelve a poner.») y la introducción, que decía «Topes, desviadores y parrillas se eligen por su rejilla» y ahora dice «Topes y desviadores entran al elegir su variante, con sus valores por defecto; su rejilla elige dónde van y sus medidas. La parrilla se pone con su «Configurar…» y se quita con «Quitar».». Dinámico y Push Back tienen su propia introducción, que no cambia | Que el diálogo diga lo que hace ahora |

**Las transcripciones del clásico:**
- **Push Back:** la clase `Classic` de `PushBackSafetyEditTests` sigue al clásico corregido, con comentarios `T6 seg`. Son tres
  cambios y una pieza nueva:
  - `OkLateral` guarda también el protector lateral que no dibuja;
  - `Grid` guarda también un desviador sin celdas;
  - `DesviadorVariant` ya no mira `AllCellsOff`;
  - **nueva:** `NewDesviadorWithoutGrid`, la variante elegida y «Aceptar» sin «Configurar…».

  La defensa de esa clase es de la parte pb y no se tocó.
- **Selectivo y Dinámico no tienen transcripción:** sus pruebas abren el diálogo real, como la equivalencia de T4S-01
  (`RackCadSafetyPanelTests`) y las referencias de `SelectiveSafetyTabTests` y `DynamicSafetyTabTests`, y siguen en verde.
- No hubo que debilitar ninguna guarda.

**Las pruebas:**
- **`ClassicSafetyAcceptTests`** (UI, nueva, 24 casos). Usa el diálogo real, construido como lo construye su editor clásico
  (`RackCadSelectiveSafetyDialog.Build`, `RackCadModalDialogs.BuildDynamicSafetyDialog` y el seam `SafetyWindowDialog` del editor clásico
  de Push Back).
  - **Dolor 3, Selectivo:** bota, protector lateral, tope y desviador.
    - Se elige solo la variante y «Aceptar».
    - La familia está, y el diseño es byte a byte el de la ventana tras elegir la misma variante en «Variante y medidas».
  - **Dolor 3, Dinámico:** las cinco familias, de la misma forma. La bota y el desviador fallaban en la base; el protector lateral, la guía
    y la defensa ya pasaban.
  - **Los valores a la vista** (T6seg-S1):
    - la bota pasa de «Ninguno» a «Ambas», y otra variante deja «Izquierda»;
    - «Ninguno» y otra vez la variante devuelve «Ambas»;
    - el protector lateral pasa a «Por poste (2)…».
  - **Un diálogo sin tocar no guarda nada:** la parrilla no tiene lista de variante.
  - **Dolor 4, Selectivo:** tope, parrilla, desviador, protector lateral y bota.
    - La ventana los deja sin nada: «En ninguna», «En ninguno», «No lleva» en cada poste, o el lado general en «Ninguno».
    - El BOM de la familia es 0.
    - RACKEDITAR sobre eso, con «Aceptar» sin tocar, da el mismo diseño, byte a byte.
  - **Dolor 4, Dinámico:**
    - guía, protector lateral, bota y defensa, de la misma forma;
    - el desviador, con «Ninguno» en su rejilla real (seam `DesviadorDialog`): conserva su variante, sus medidas y su lectura por poste, y
      su BOM es 0.
  - **Push Back:**
    - un desviador nuevo elegido sin «Configurar…» queda con `Side` y `AuthoredSide` Izquierdo, 18 in y 18 in, sin celdas apagadas, como lo
      escribe la ventana;
    - el protector lateral con los cuatro postes en «Ninguno» y el desviador con todas las celdas apagadas conservan su variante, y no se
      coloca ningún desviador.
  - **Dolor 6:** la prueba de la defensa de arriba.
- **`PushBackSafetyEditTests`** (Core), contra la transcripción corregida:
  - `ALateralLeftWithNoPostKeepsItsVariantAsTheClassicAndDrawsNothing` (antes
    `…WhereTheClassicDropsItAndDrawsTheSame`): ahora cada paso es `Same`, y el clásico conserva la familia.
  - `ADesviadorLeftWithNoLevelKeepsItsVariantAsTheClassicAndDrawsNothing` (nueva).
  - `ANewDesviadorComesAsItsGridAcceptsItOnTheSideOfTheLowEnd` recorre además el camino sin «Configurar…».
- **En la base** (con solo el seam): de las 24, fallan 19 y pasan 5. Las 5 son la de la parrilla sin tocar, las tres de Dinámico que ya se
  guardaban y la del dolor 6.

**Comprobación en rojo:**
- Siete piezas rotas a propósito, una a una, con `S\hs65\t6\seg\tools\red.py`. Las salidas están en `red-out.txt` y `red-log.txt`, en
  la misma carpeta.
- El script guarda antes una copia de cada original FUERA del worktree, compila, corre la clase focal y restaura el archivo desde sus
  bytes.
- Las siete volvieron con la misma huella SHA-256 (`08c607d4…` y `c2dc1542…`). En `git diff` no quedó ningún resto.
- Cada pieza hizo fallar sus pruebas:
  - **R1, el arreglo entero revertido** (solo queda el seam): fallan 19 de 24, las mismas que en la base.
  - **R2, sin los valores al elegir la variante:** fallan 4, la bota de Selectivo y de Dinámico, el protector lateral de Selectivo y la de
    los valores a la vista.
  - **R3, el desviador nuevo con el lado de su fila y no el de su rejilla:** falla la del desviador nuevo de Push Back.
  - **R4, otra vez «Configurar…» para el tope y el desviador:** fallan 4, el tope y el desviador de Selectivo, el de Dinámico y el de Push
    Back.
  - **R5, de vuelta las condiciones de «todo apagado»** (celdas, extremos y postes): fallan 11, todas las del dolor 4.
  - **C1, la transcripción que vuelve a quitar el protector lateral que no dibuja:** falla la del protector lateral (Core).
  - **C2, la transcripción que vuelve a quitar el desviador sin celdas:** falla la del desviador sin niveles (Core).

**Archivos:**
- `src/RackCad.UI/SelectiveSafetyWindow.cs`: el arreglo y el seam.
- `src/RackCad.Application/VentanaRackCad/RackCadSafetyEdit.cs`: un comentario, marcado `T6 seg`.
- `tests/RackCad.UI.Tests/ClassicSafetyAcceptTests.cs`: nuevo.
- `tests/RackCad.Tests/Workspace/PushBackSafetyEditTests.cs`: la transcripción y tres pruebas, marcadas `T6 seg`.
  - **Para integrar:** la parte pb toca la misma clase `Classic` (también `Grid`, `OkLateral`, `NewDesviador` y su resumen). Mis cambios
    allí son de pocas líneas, cada uno con su comentario `T6 seg`, y la nota de la clase va en su cuerpo, no en el resumen.
- **Dos avisos de la revisión de pb, comprobados:**
  - **Push Back de dos sentidos.** El «Aceptar» corregido ya no lee niveles para decidir nada: ni `levelsPerFrente`, que es del lado
    activo y por frente, ni `desviadorLevelsPerPost`. Una familia nueva sin configurar toma los valores de su fila, sin celdas apagadas, y
    «todo apagado» ya no se mira. Así, la corrección de pb de `DesviadorLevelsPerPost` (el lado más alto) no choca con este arreglo, y el
    rack de dos sentidos no vuelve a depender del lado activo por aquí. Las pruebas de Push Back de esta parte son de un sentido.
  - **Las familias que el diálogo no muestra** (la defensa de Push Back, con `includeDefensa: false`, y la bota, en sus secciones) no se
    conservan ni se tocan: el resultado sale solo de las filas del diálogo, como antes. Nada de este arreglo depende de cómo arranca
    `ApplyDefenseSections`.
- `docs/automation/evidence/I-64-pilot/VENTANA-RACKCAD/construido.md`: esta sección.

**Los números** (sobre las fuentes finales):
- **UI completa:** 3134 correctas, 0 con error y 48 omitidas, de 3182. Es la base (3110/0/48) más las 24 de `ClassicSafetyAcceptTests`.
  Corrió con `--blame` y terminó entera.
- **Core Full:** 19484 correctas, 0 con error y 403 omitidas, de 19887. Es la base (19483/0/403) más la prueba nueva de
  `PushBackSafetyEditTests`.
  - La primera pasada se anuló: «Proceso de host de pruebas bloqueado», con la máquina cargada por las otras partes. Llevaba 19384
    correctas, 0 con error y 400 omitidas.
  - Se repitió entera con `--blame-hang`, sin bloqueo.
  - Tras quitar un comentario del resumen de `Classic` (sin cambio de código), `PushBackSafetyEditTests` se volvió a compilar y a pasar:
    22 de 22.
- **Solución en Debug** (Plugin incluido), compilada con `--no-incremental`: 0 errores. Las del Plugin son las 2 MSB3277 de siempre. Mi
  archivo de pruebas tenía una advertencia xUnit2013, corregida.
- **Las focales, durante el trabajo:** las clases de UI que tocan los diálogos de seguridad (Push Back, «Safety», las de la bota y el tope
  de Selectivo, `DialogWindow*`, el censo de ventanas y `BlankFront*`): 892 correctas, 0 con error y 8 omitidas, de 900.

**Para el Owner:**
- Los supuestos T6seg-S1 a T6seg-S6. S4 y S5 ya llevan su respuesta («Sí, como la ventana» y «Solo lo que dibuja»), aplicada en las
  correcciones de abajo.
- El dolor 6 ya no pasa desde T4S-14. No hay nada que validar, salvo T4S-14 mismo, que seguía pendiente.

### Correcciones de la revisión (sobre 794a599c)

La revisión independiente de la parte seg y dos respuestas del Owner, en el mismo worktree, sin commit.

1. **El seam de la rejilla del desviador chocaba con el de pb** (`c79d3869`): el mismo nombre con otro tipo daría CS0102 al integrar.
   - Se adoptó, byte a byte, la declaración y el uso de pb: `Func<SafetyDesviadorGridWindow, SafetyDesviadorGridWindow.DesviadorResult>`,
     antes de `EditDesviador`, con su comentario «T6 pb». Las 30 líneas, de la declaración a `row.DesviadorConfigured = true;`, son
     idénticas a las de pb (comparadas sin saltos de línea).
   - Las pruebas devuelven `grid.BuildResult()` (null = cancelar). Sale el ayudante que fijaba por reflexión el `Result` de la rejilla.
2. **La parrilla** (el Owner: «Sí, como la ventana»).
   - En el clásico no había forma de quitarla: su fila no tiene lista de variante, y apagarla toda ahora la conserva.
   - **Arreglo:** el botón «Quitar», junto a su «Configurar…» (T6seg-S6). Está deshabilitado mientras la parrilla no está puesta.
     «Aceptar» ya no la guarda, y lo que tenía su rejilla se queda en la fila mientras el diálogo está abierto, así que «Configurar…» la
     vuelve a poner.
   - **Dos parrillas en el catálogo** (T6seg-S4): una con todo apagado solo se guarda mientras ninguna otra del diálogo dibuja, para no
     taparla. La prueba usa una segunda parrilla inventada.
3. **El contador** (el Owner: «Solo lo que dibuja»; T6seg-S5).
   - `SelectiveSafetyWindow.PlacesSomething` dice si una selección pone algo en el dibujo, con los niveles con que abre el diálogo. Lo usan
     los dos editores clásicos.
   - El editor de Selectivo recalcula el número también al resolver (`RunRecompute`), así que un rack abierto cuenta con sus niveles.
   - El filtro de lo que se guarda no cambió.
4. **Push Back de dos sentidos desde el lado A**, con el lado B un nivel más alto en su primer frente:
   - un desviador nuevo sin «Configurar…» sale sin celdas apagadas y en el lado del extremo bajo;
   - su rejilla, con todo apagado, conserva la familia, con todas las celdas que la rejilla mostró apagadas.
   - La prueba no depende de qué niveles da `DesviadorLevelsPerPost`, que pb cambia: compara con los de la propia rejilla.
5. **Estado muerto:** salen `Row.DesviadorLevelCounts`, su `SelectiveDesviadorPlan.Build` del constructor y su asignación en
   `EditDesviador`. Ya nada los leía.
6. **Comentarios y textos viejos:**
   - `SelectiveSafetyView.WriteAll` decía que el «Aceptar» clásico quita la familia;
   - `PushBackSafetyEdit.DesviadorRow` daba la razón vieja («`AllCellsOff` is false»);
   - la introducción del diálogo de Selectivo (T6seg-S6).
7. **T6seg-S1:** queda declarado que el protector lateral de Selectivo elegido sin más quita las botas de sus dos postes y cambia el dibujo y
   el BOM del clásico.
8. **De punta a punta por los editores clásicos reales** (sí fue razonable):
   - **El seam:** cada editor gana `SafetyWindowDialog`, como el de Push Back: sustituye solo el `ShowDialog` de «Elementos de seguridad…»
     (`RackSelectiveWindow.Safety_Click`, `RackDynamicSystemWindow.Safety_Click`).
   - **Selectivo**, por `Safety_Click`:
     - tope, desviador y bota en «Ninguno»: «Elementos de seguridad (2)…»;
     - el diseño que guarda (`BuildDesignForTest`, con el filtro `SafetyDraws`) lleva las tres;
     - un rack que la ventana dejó con su tope apagado se abre con `LoadExisting`: «(1)…», y guarda el tope.
   - **Dinámico:** un rack nuevo con la bota puesta en «Ninguno» cuenta una familia menos de las que guarda, y la bota se guarda.

**Archivos de las correcciones:**
- `src/RackCad.UI/SelectiveSafetyWindow.cs`: el seam de pb, «Quitar», la regla de las dos parrillas, `PlacesSomething`, la introducción y el
  estado muerto. Vuelve el `using RackCad.Application.Systems.Dynamic`, que necesita `PlacesSomething`.
- `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs` y `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs`: el
  seam y el contador, marcados `T6 seg`.
- `src/RackCad.Application/VentanaRackCad/SelectiveSafetyView.cs` y `PushBackSafetyEdit.cs`: un comentario cada uno.
- `tests/RackCad.UI.Tests/ClassicSafetyAcceptTests.cs`: el seam de pb y 5 pruebas nuevas, de 24 a 29.

**Comprobación en rojo de las correcciones:**
- Siete piezas, una a una, con `S\hs65\t6\seg\tools\red2.py`. Las salidas están en `red2-out.txt` y `red2-log.txt`.
- Antes, una copia de los tres archivos fuera del worktree.
- Las siete volvieron con la misma huella SHA-256 (`f972a271…`, `3546516c…` y `eac27f39…`). En `git diff` no quedó ningún resto.
- Cada pieza hizo fallar sus pruebas:
  - **Q1, «Quitar» sin efecto:** falla la de «Quitar».
  - **Q2, sin la regla de las dos parrillas:** falla la de las dos parrillas.
  - **Q3, el contador de Selectivo cuenta todo otra vez:** falla la del editor clásico de Selectivo.
  - **Q4, lo mismo en Dinámico:** falla la del editor clásico de Dinámico.
  - **Q5, Selectivo sin recontar al resolver:** falla la del editor clásico de Selectivo, en el rack abierto con su tope apagado.
  - **Q6, el desviador nuevo pide «Configurar…» otra vez:** fallan 5. Entre ellas, la del Push Back de dos sentidos desde el lado A y las
    del desviador nuevo de Selectivo, Dinámico y Push Back.
  - **Q7, todo apagado quita el desviador otra vez, con los niveles del lado activo:** fallan 4. Entre ellas, la del Push Back de dos
    sentidos y la del Push Back de un sentido.

**Los números de las correcciones** (sobre las fuentes finales):
- **Solución en Debug** (Plugin incluido): 0 errores.
  - Primero se compiló con `--no-incremental`: 84 advertencias, ninguna en los archivos de esta parte.
  - Después, de forma incremental: solo las 2 MSB3277 del Plugin de siempre.
- **Focales de UI:** 481 correctas, 0 con error y 4 omitidas, de 485. Son:
  - `ClassicSafetyAcceptTests`, 29;
  - `RackCadSafetyPanelTests`, las de la pestaña de seguridad de Selectivo y Dinámico, y las de migración al shell de los editores clásicos
    (`*ShellMigrationTests`, que leen su botón «Elementos de seguridad…»);
  - las del tope, la bota y las rejillas de Selectivo, `DialogWindow*`, el censo de ventanas y `BlankFrontDesviador*`;
  - las de seguridad, botas, defensas y topes del clásico de Push Back, y sus caracterizaciones.
- **Focal de Core:** `PushBackSafetyEditTests`, 22 de 22.
- **Las suites completas** no se corrieron sobre estas correcciones: lo pidió el Coordinator, para no saturar la máquina. Las correrá la
  sesión sobre la rama integrada.
  - Una pasada empezada antes se detuvo con la pausa del Owner, sin resultado.
  - Las últimas completas son las de 794a599c: UI 3134/0/48 y Core 19484/0/403.

## Sexta tanda, st: el editor de la ventana tras una orden ajena (O-5, M-16)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-st`, sobre `f02f519b` (worktree `D:\w6st`). Sin commit, sin push, sin AutoCAD.

**Órdenes del Owner (2026-11-27, literales):**
- sobre O-5: «Avisar y ofrecer releer». **M-16 queda resuelto con esa decisión**, para los seis editores de la ventana (Selectivo, Dinámico,
  Push Back, Cantilever, cama y cabecera). El editor clásico no se toca;
- sobre el BOM y «Almacén», que decían «Puede no estar al día» tras cualquier orden, incluso un ZOOM: «Afinarlo», que solo avise con órdenes
  que cambian el dibujo. La misma señal afinada sirve para los editores, para el BOM (BM-S1) y para «Almacén» (AL-S7).

**El defecto (O-5 del smoke de la quinta tanda):** tras un «U» (o RACKEDITAR, un MOVE…), el editor de la ventana seguía «Limpio» con lo que se
deshizo, y un «Actualizar» posterior lo volvía a escribir sin aviso. La causa: la señal de una orden ajena (`MarkDraftsPossiblyStale` del
drenaje) solo marca los borradores del piloto (`WorkspaceHostCore.MarkPilotDraftsPossiblyStale`), y el estado de los editores de la ventana
(`DraftStateOf`) solo miraba `IsWriting`/`IsDirty`.

### La señal

- **Cuándo corre una orden:** la que ya seguía el BOM (BM-01). Cada presentación del host dice si en el dibujo de la ventana corre una orden
  (el panel toma su instantánea dentro de CommandWillStart y otra vez al volver al reposo) o un modal de RackCad. `RackCadCommandWatch` sigue
  cada orden desde su primera presentación hasta que el dibujo vuelve al reposo, o hasta que la ventana presenta otro dibujo.
- **Si cambió el dibujo (afinado):** el primer recurso de la lista del encargo, el hint `DatabaseObjectChanged`. El modelo de sesión no lo hacía
  llegar a la ventana (el drenaje lo junta con `CommandEnded` en `MarkDraftsPossiblyStale` y solo lo usa el piloto). El cambio mínimo:
  - `WorkspaceSession.DrawingChangeCount`: cuenta, en memoria, cada `DatabaseObjectChanged` que la sesión recibe (los ajenos en `EnqueueHint` y
    los de la mutación propia del piloto en `EnqueueOwnHint`). Es un evento de objeto (añadido, borrado, modificado, des-añadido o re-añadido);
    la contabilidad asociativa ya no llega (IC-10). No se drena, no lee nada y no cambia nada más de la sesión;
  - `WorkspaceHostSnapshot.DrawingChangeCount` (último parámetro, con valor por omisión 0): el de la sesión activa, que pone
    `WorkspaceHostCore.GetSnapshot`;
  - `WorkspacePanelViewModel.PresentedDrawingChangeCount`: el de la última instantánea presentada.
  - Una orden **cambió el dibujo** si ese número se movió mientras corría. Un ZOOM, un PAN o un REGEN no cambian ningún objeto; un «U», un ERASE,
    un MOVE o RACKEDITAR sí.
  - No se tocaron `EventBridgeCore`, `AutoCadEventBridge`, el controlador, la paleta, `TryDrain` ni las guardas (ver «Riesgo y guardas»).
- **Lo propio, con la puerta numerada:** la puerta entrega UNA petición numerada a la vez, y su orden la toma (contesta dentro de sí o dice que
  la tomó, `RequestStarted`). Una orden que empezó con una petición de la puerta esperando, sin tomar, para el mismo dibujo, y que la tomó, es
  la orden de la puerta: las …ABRIR de «Editar aquí», las …APLICAR, …INSERTAR y …NUEVO, y las lecturas sin marca del BOM, de «Variables» y de
  «Almacén». No marca, aunque escriba. **Todo lo demás es ajeno:** una orden sin petición esperando; una que terminó sin tomar la petición que
  esperaba (la orden de la puerta no había corrido y el usuario corrió otra); o una durante la cual la ventana dejó de esperar.
- **La regla:** al terminar una orden ajena que cambió el dibujo, se marcan los editores de ese dibujo. La marca no lee nada: ninguna orden sale
  de la ventana sin un gesto del usuario. Un borrador nuevo de «Nuevo» no está en el dibujo y nunca se marca.
- **El BOM y «Almacén»** (`RackBomPageViewModel.Present`, `WarehousePageViewModel.Present`): cada página sigue las órdenes con su propio
  `RackCadCommandWatch` y conserva su regla de lo propio (su lectura desde que la orden la tomó; para el BOM, también RACKLISTA y RACKBOMTOTAL).
  Ahora dicen «Puede no estar al día» **al terminar** la orden y solo si cambió el dibujo; un «Actualizar» de un editor sigue contando (cambia el
  dibujo).

### Lo que se ve

- **El aviso** (`RackEditorDrawingChangePanel`, el mismo control en los seis editores, bajo su cabecera), con el texto de
  `RackEditorDrawingChange`:
  - limpio: «El dibujo cambió desde que se leyó (corrió una orden en AutoCAD). Lo que muestra el editor puede no ser lo que hay ahora en el
    dibujo: pulsa «Volver a leer del dibujo».»;
  - con cambios sin escribir: «El dibujo cambió desde que se leyó (corrió una orden en AutoCAD). Tienes cambios sin escribir: «Actualizar»
    escribirá el borrador encima de lo que hay en el dibujo. «Volver a leer del dibujo» descarta tus cambios y lee lo que hay ahora.»
- **«Volver a leer del dibujo»** es la lectura de «Editar aquí» de su sistema (la puerta …ABRIR), con el gesto del usuario, para el rack del
  editor y en su dibujo. Su respuesta sustituye el editor aunque tenga cambios (la petición lleva `IsReread`); «Editar aquí» sin ese gesto
  sigue sin sustituir un borrador con cambios, como antes. Ningún editor tenía un «Volver a leer» propio: el «Descartar» de los seis vuelve a
  lo último leído o escrito, no relee el dibujo.
- **«Actualizar» con cambios** sobre un dibujo marcado pregunta antes, en línea, con el patrón de las preguntas de los editores: «¿Escribir el
  borrador encima de lo que hay ahora en el dibujo? Si la orden que corrió cambió este rack, ese cambio se pierde.» · «Escribir encima» /
  «Cancelar». Un editor limpio no tiene nada que escribir, así que no pregunta.
- **La lista de racks:** «Abierto en la ventana (posiblemente obsoleto)» o «Modificado (editor de la ventana) (posiblemente obsoleto)», con la
  marca «Obsoleto», las palabras que ya usa el piloto (`RackListViewModel`, «(posiblemente obsoleto)»). `RackListViewModel` no se tocó: el estado
  sale de `DraftStateOf` de la ventana.
- **La marca se quita** al volver a leer (el editor nuevo no la tiene) y con una escritura propia que AutoCAD hizo (`MarkWritten` de los seis
  editores). Una escritura que AutoCAD no hizo la deja.

### Supuestos para el Owner

| Id | Supuesto | Por qué |
|---|---|---|
| T6st-S1 | La marca (y el «Puede no estar al día» del BOM y de «Almacén») aparece al **terminar** la orden, con el dibujo de vuelta al reposo, no al empezar. La hora de «corrió una orden a las…» es la del final | «Una orden ajena terminada»; y si cambió el dibujo solo se sabe al final |
| T6st-S2 | «Cambiar el dibujo» = un evento de objeto de su base de datos mientras corría la orden (añadido, borrado, modificado…), no de la contabilidad asociativa. Una variable de sistema no lo es. Un ZOOM o un PAN en el espacio modelo no cambian objetos; **dentro de una ventana gráfica de una presentación** (MSPACE) sí, porque la ventana gráfica es un objeto, y marcan. Un cambio de capa sí marca: cambia el dibujo. **Por comprobar en AutoCAD** (el smoke): ZOOM, PAN y REGEN no marcan; «U», ERASE y MOVE sí | Es lo que el host ya distingue (`DatabaseObjectChanged`, IC-10), sin una lista de órdenes que mantener |
| T6st-S3 | Lo propio se distingue con la puerta numerada: solo la orden que tomó la petición que esperaba es de la ventana. Una orden ajena que cambió el dibujo marca aunque no toque los racks abiertos (un LINE, un cambio de capa): la ventana no sabe qué objetos cambió. Marcan también, si cambian el dibujo, las órdenes clásicas que lanza la ventana («Abrir en editor clásico», las tarjetas de «Nuevo»), la actualización del piloto del panel y un modal de RackCad. RACKLISTA y RACKBOMTOTAL solo leen: no marcan | La puerta es lo único que identifica una orden sin leer su nombre; marcar de más solo cuesta un aviso y una relectura, y marcar de menos es el defecto |
| T6st-S4 | Si la ventana presenta otro dibujo antes de que termine la orden, no sabe si cambió el suyo: lo marca (la respuesta prudente de antes). Un cambio de la base de datos sin orden (con el dibujo en reposo) no marca | El número es de la sesión activa; un cambio sin orden no es «una orden de AutoCAD» |
| T6st-S5 | «Insertar vista…» espera mientras el editor está marcado: «El dibujo cambió desde que se leyó: pulsa «Volver a leer del dibujo» antes de insertar vistas.» (con cambios: «…escribe antes el borrador con «Actualizar», o vuelve a leer del dibujo, para insertar vistas.») | Insertar escribe primero el borrador en las vistas que hay, también sin cambios: sería el mismo «escribir encima» sin aviso |
| T6st-S6 | «Volver a leer del dibujo» con cambios los descarta sin otra pregunta: el aviso ya lo dice, y el botón es el gesto | Como «Descartar», que tampoco pregunta |
| T6st-S7 | Las órdenes que corren con la paleta **oculta** no se ven (la ventana no se presenta mientras está oculta): ni los editores, ni el BOM, ni «Almacén» se marcan. Es el hueco declarado de BM-S1 | Cubrirlo con el número pide contar los cambios fuera de una orden vista, y entonces los cambios diferidos tras una escritura propia marcarían de más; queda como pregunta |
| T6st-S8 | Volver a leer crea un editor nuevo: la pestaña y la selección vuelven a las de un editor recién abierto | Es la lectura de «Editar aquí», sin un camino de recarga en su sitio para cada sistema |

**BM-S1 y AL-S7 quedan afinados** por T6st-S1 a S4: «Puede no estar al día» = terminó una orden que no es la propia y que cambió el dibujo.

**Preguntas para el Owner:**
1. Una escritura de «Variables» (RACKVARIABLESAPLICAR, de la puerta) puede redibujar racks cuyo editor está abierto. Por la orden («las de la
   puerta NO marcan») no los marca. Un borrador con cambios ya está protegido (VA-04 avisa en la página y su «Actualizar» se negaría, S-04). Uno
   limpio queda con los valores de antes. ¿Debe marcarlos?
2. La paleta oculta (T6st-S7): ¿basta el hueco declarado, o se quiere marcar al volver a mostrarla si el dibujo cambió mientras tanto (con el
   riesgo de marcar de más tras una escritura propia)?
3. M-02 de la segunda revisión (la LISTA dice «puede no estar al día» tras cualquier orden, incluso un ZOOM) es la misma pregunta para la lista,
   pero viene del drenaje (`TryDrain`: todo `CommandEnded` invalida el caché) y la lista es de la parte ui: no se tocó. ¿Se afina igual?

### Riesgo y guardas del cambio en el modelo de sesión

- **El cambio:** una propiedad de solo lectura y dos incrementos en `WorkspaceSession`, un campo con valor por omisión al final de
  `WorkspaceHostSnapshot` y una línea en `WorkspaceHostCore.GetSnapshot`. `EventBridgeCore`, `TryDrain`, los hints, el drenaje, el alcance de la
  mutación propia y los adaptadores del Plugin no cambian.
- **Riesgo:** bajo. Coste constante por evento de objeto (un `long`), sin lectura ni efecto. Todos los `new WorkspaceHostSnapshot(...)` de siete
  argumentos siguen compilando (valen 0). La igualdad del registro incluye el número; ninguna prueba compara instantáneas enteras.
- **Guardas:** ninguna se editó ni se debilitó. Una saltó durante el trabajo: `WorkspaceModelBoundaryTests.ThereIsNoPerTabStateOrPerTabSubscriptions`
  prohíbe «Tab» (sin mayúsculas) en las propiedades de `WorkspaceSession`, y el primer nombre, `DatabaseChangeCount`, lo contiene
  («Da**tab**ase»). Se renombró a `DrawingChangeCount`. Pasan las de frontera del modelo (`WorkspaceModelBoundaryTests`,
  `WorkspaceBrowserBoundaryTests`, `WorkspaceNavigationEchoTests`, `WorkspaceNavigationAuthorityTests`), la de la instantánea
  (`WorkspaceHostPortGuardTests.TheSnapshotTheUiReceivesHoldsOnlyValuesAndSessionKeys`), las de fuente del host y las tres de copia completa
  (no se tocó ningún original de sus listas).

### Archivos

**Nuevos** (`T6 st`):
- `RackCadCommandWatch.cs`: una orden desde su inicio hasta su fin, y si cambió el dibujo.
- `RackEditorDrawingChange.cs`: la marca, los textos, «Volver a leer del dibujo», la pregunta de «Actualizar» y el motivo de «Insertar vista…».
- `RackEditorDrawingChangePanel.xaml(.cs)`: el aviso, el botón y la pregunta en línea; nombres de automatización únicos («Volver a leer este
  rack del dibujo», «Escribir el borrador encima del dibujo», «No escribir encima del dibujo»).
- `RackCadWindowViewModel.DrawingChange.cs`: la señal de los editores, lo propio (`IsOwn`), la marca de los seis diccionarios, el estado de la
  lista y la relectura.
- Un parcial `.DrawingChange.cs` por editor: `DynamicEditorViewModel`, `SelectiveEditorViewModel`, `PushBackEditorViewModel`,
  `CantileverEditorViewModel`, `CamaEditorViewModel` y `CabeceraEditorViewModel` (las dos últimas pasan a `partial`).
- Pruebas: `tests/RackCad.UI.Tests/Workspace/RackCadDrawingChangeTests.cs` y `tests/RackCad.Tests/Workspace/WorkspaceDrawingChangeCountTests.cs`.

**Ediciones a archivos existentes** (marcadas `T6 st`, una o pocas líneas cada una):
- Application: `WorkspaceSession.cs`, `WorkspaceHostSnapshot.cs`, `WorkspaceHostCore.cs` (arriba).
- UI: `WorkspacePanelViewModel.cs` (`PresentedDrawingChangeCount`).
- `RackCadWindowViewModel.cs`: `IsReread` en `DoorRequest`; `IsReread: _rereading` en «Editar aquí» de Selectivo y Dinámico; la condición
  «no se reemplazó» con `!request.IsReread`; `ForgetCommandRunRequest` en «Dejar de esperar»; el estado «(posiblemente obsoleto)» en
  `DraftStateOf`; `Contained(PresentDrawingChange)` en `Present`.
- `RackCadWindowViewModel.Cabecera.cs`, `.Cama.cs`, `.Cantilever.cs`, `.PushBack.cs`: `IsReread` en su «Editar aquí» y en su condición.
- `RackCadWindowViewModel.Bom.cs` y `.Warehouse.cs`: el número de cambios a `Present` de la página.
- `RackBomPageViewModel.cs` y `WarehousePageViewModel.cs`: la regla afinada en `Present`.
- Los seis editores: una línea en `RequestWrite` (`AsksBeforeWrite`), en `MarkWritten` (`Clear`) y en `RaiseState` (`Refresh`); en los cuatro
  que insertan vistas, una en `InsertViewBlockedText`.
- Las seis vistas de editor: el control `DrawingChangePanel` bajo la cabecera.
- Pruebas existentes al día: `RackCadBomPageTests` y `RackCadWarehousePageTests`. Su doble del host cuenta ahora los cambios: una orden de esas
  pruebas cambia el dibujo mientras corre (un MOVE), salvo que diga que no (un ZOOM). Tres pruebas comprobaban el aviso al EMPEZAR la orden y
  ahora lo comprueban al terminar (T6st-S1), con una línea más que presenta el reposo: `ACommandAfterTheReadingSaysItMayBeOutOfDate…` (dos
  veces), `AReadingMayBeOutOfDateAfterACommandThatIsNotItsOwn` e `InTheOrderOfAutoCadTheOwnReadAndTheOwnPlacement…`. Ninguna otra aserción cambió.

### Pruebas

`RackCadDrawingChangeTests` (UI), con el doble de la puerta de los seis sistemas y el doble del puerto del host (que cuenta los cambios):
- **La señal:** una orden ajena que cambia el dibujo marca al terminar y no lee nada (×6); un ZOOM no marca y un «U» o un ERASE sí (×6); las
  órdenes de la puerta no marcan, aunque escriban: «Editar aquí» y «Actualizar» con la orden corriendo dentro de la llamada (M-22), y «Editar
  aquí» de otro rack del mismo dibujo; una orden de otro dibujo no marca y una del suyo sí; una orden que no tomó la petición que esperaba, o
  durante la cual la ventana dejó de esperar, es ajena; un modal de RackCad cuenta; un borrador de «Nuevo» no se marca; un cambio sin orden no
  marca, y otro dibujo presentado durante la orden sí (T6st-S4).
- **El aviso y el botón en los seis editores:** el modelo (`VolverALeerDelDibujoIsTheReadOfEditarAqui…`, ×6; con cambios, los descarta, ×6) y la
  vista WPF real en STA (`EveryEditorViewShowsTheNotice…`, ×6: oculto sin la marca, visible con ella, el botón lee por la puerta de su sistema).
  «Editar aquí» sin el gesto sigue sin sustituir un borrador con cambios; el botón se apaga mientras AutoCAD escribe y, con otra petición
  esperando, dice por qué.
- **La confirmación de «Actualizar»:** pregunta, «Cancelar» no escribe, «Escribir encima» escribe una vez y la respuesta escrita quita la marca
  (×6; la vista, `EveryEditorViewAsksInline…`, ×6); una escritura no hecha deja la marca y «Descartar» quita la pregunta.
- **«Insertar vista…»** espera con la marca en Dinámico, Selectivo, Push Back y Cantilever.
- **El BOM y «Almacén»:** `AZoomLeavesTheBomUpToDateAndAUOrAnEraseDoesNot` y `AZoomLeavesTheReadingUpToDateAndAUOrAnEraseDoesNot`.
- **Core:** `WorkspaceDrawingChangeCountTests`: los cinco eventos de objeto cuentan, propios o no; una orden, una selección o la contabilidad
  asociativa no; el drenaje no lo pone a cero; la instantánea lleva el de la sesión activa, cada dibujo el suyo, y 0 sin dibujo.
- **RED sobre la base** (las 13 primeras, solo con la API que ya existía): 12 fallaron. La lista seguía en «Abierto en la ventana» tras la orden
  ajena (×6) y «Actualizar» escribía sin preguntar: 1 escritura donde se esperaban 0 (×6).
- **Comprobación en rojo** (copias del original fuera del worktree, restauradas después; `git diff` sin restos):
  - sin `Contained(PresentDrawingChange)` en `Present` (la señal): fallaron las 56 de `RackCadDrawingChangeTests` (todas necesitan la marca en
    algún paso);
  - sin `ended.ChangedTheDrawing` (la señal afinada) en la ventana, en el BOM y en «Almacén», y a la vez sin `AsksBeforeWrite` en
    `RequestWrite` de Dinámico (la confirmación): 12 de 116 fallaron, justo las esperadas: `AZoomThatChangesNothingDoesNotMark…` (×6),
    `AZoomLeavesTheBomUpToDate…`, `AZoomLeavesTheReadingUpToDate…`, y las cuatro de la confirmación de Dinámico
    (`ActualizarOfADraftWithChanges…`, `ActualizarWithChangesAsksInline…`, `EveryEditorViewAsksInline…` y `AWriteThatAutoCadDidNotDo…`);
  - con `IsOwn` siempre falso (lo propio): falló `TheCommandsOfTheDoorOfTheWindowDoNotMark…` (1 de 56);
  - sin los dos incrementos de `WorkspaceSession` (el número): fallaron las 2 de `WorkspaceDrawingChangeCountTests`; las 19 de
    `WorkspaceModelBoundaryTests` siguieron en verde.

### Números

- **UI completa:** 3168 correctas, 0 con error, 48 omitidas (total 3216; la base daba 3110/0/48: +56 de `RackCadDrawingChangeTests` y +2 del BOM
  y de «Almacén»).
- **Core Full:** 19485 correctas, 0 con error, 403 omitidas (total 19888; la base daba 19483/0/403: +2 de `WorkspaceDrawingChangeCountTests`).
- **La solución en Debug** (Plugin incluido): 0 errores; el Plugin se volvió a compilar tras los últimos comentarios, con 0 errores.
- Archivos tocados sin caracteres de control (escaneo de los 43 cambiados o nuevos).

### Correcciones de la revisión (sobre 5114345e)

Las tres respuestas del Owner del 2026-11-27 y los hallazgos de la revisión independiente, en una sola ronda. Sin commit (la sesión lo hace).

**A. Las respuestas del Owner:**
- **A1. «Variables» y los editores limpios («Sí, marcarlo»).** Cambia una conducta declarada en VA: hasta aquí, una escritura de «Variables»
  CERRABA los editores limpios de Selectivo de los racks que redibujó («Se cerró el editor de…»).
  - Ahora los marca «El dibujo cambió», con «Volver a leer del dibujo» (`MarkRedrawnByVariables`). La página dice «El editor de … muestra lo
    que había antes: pulsa en él «Volver a leer del dibujo».».
  - Un editor con cambios sigue protegido como antes (VA-04 en la página, S-04 en su «Actualizar»).
  - La escritura misma es de la puerta y no marca por la señal.
- **A2. La paleta oculta («Sí, al volver a mostrarla»).** Va en la ventana, con los números por sesión, sin tocar `WorkspacePaletteHost`:
  - al presentarse de nuevo un dibujo sin que se viera ninguna orden, si en ese dibujo **empezaron órdenes y cambió la base de datos**, sus
    editores se marcan (y el BOM y «Almacén» dicen «Puede no estar al día»);
  - un cambio sin ninguna orden (lo que rodea a una orden, campos) no marca: así no marcan de más los cambios que llegan tras una escritura
    propia;
  - cierra el hueco de T6st-S7 y de BM-S1.
- **A3. La lista tras un ZOOM («Sí, afinarlo»).** Sin tocar `TryDrain`:
  - `RackListViewModel` recuerda `DrawingChangeCount` de su última lectura de cada dibujo, y cuando el host contesta que la olvidó
    (`Outdated`), solo dice «La lista puede no estar al día…» si el número se movió;
  - si no se movió, la lista que tiene es la del dibujo y no dice nada;
  - son tres líneas marcadas `T6 st` en el archivo y un bloque nuevo junto a `RefreshStates` (la parte ui tocó otras líneas del mismo
    archivo).
  - **M-02 de la segunda revisión queda resuelto.**

**B. Los hallazgos:**
- **I-2:** no se tocó el puente ni `SubjectOf`. T6st-S2 añade sus casos al smoke (abajo).
- **I-3 (el BOM decía «Al día» con una orden en curso que ya había cambiado el dibujo):**
  - el BOM y «Almacén» se marcan **de forma provisional al EMPEZAR** la orden, como antes, y la marca se quita al terminar si el número no se
    movió o si la orden era la suya;
  - una lectura del BOM que se deja de esperar durante la orden deja la marca fija (como antes);
  - la hora de «corrió una orden a las…» vuelve a ser la del inicio. El afinado del ZOOM se conserva.
- **M-1 (órdenes fundidas):**
  - la instantánea lleva `CommandStartCount`, el número de inicios de orden de la sesión, que el puente ya cuenta para el piloto
    (`ForeignCommandStartsOf`, leído en `WorkspaceHostCore.GetSnapshot` a través de `IWorkspaceBridgeLink`, sin tocar el puente);
  - una orden es propia (de la puerta, o la lectura del BOM o de «Almacén») solo si empezó **exactamente una** mientras corría. Un script,
    una macro `^C^C`, `(command …)` o la orden de la puerta justo tras otra, sin reposo entre ellas, son ajenas.
- **M-2:** con los mismos números se resuelven «Otro dibujo» (T6st-S4) y la regla «Volviste a este dibujo» del BOM y de «Almacén»:
  - si la ventana presenta otro dibujo antes de que termine la orden, el veredicto espera a que ese dibujo vuelva a presentarse;
  - volver a un dibujo dice «Puede no estar al día» solo si en él empezaron órdenes y cambió la base de datos. Antes lo decía siempre.
  - Es el mismo mecanismo de A2 (`RackCadCommandWatch`, rehecho: veredictos `Command`, `Unseen` y `Return`).
- **M-3, M-5:** declarados (T6st-S9 y S10).
- **M-4:** una prueba parametrizada por orden de la puerta, con cambios en el dibujo; ninguna marca.
- **M-6:**
  - la pregunta en línea usa el estilo `QuestionPanel` de las otras preguntas (copiado en el control, que no hereda los recursos de cada
    vista);
  - la cabecera dice `UpdateBlockedText` cuando «Escribir encima» no puede, como los otros cinco.
- **M-7:** por orden del coordinador (tras la pausa que pidió el Owner) no se corrieron las suites completas en esta ronda, para no saturar la
  máquina: solo las focales y la solución en Debug. La sesión las corre sobre la rama integrada.
- Los nombres que usará at (`DrawingChange.IsChanged`, `AsksBeforeWrite`, `MarkWritten` → `Clear`) no cambiaron.

**Supuestos al día** (sustituyen a los de arriba con el mismo número; los demás siguen):

| Id | Supuesto | Por qué |
|---|---|---|
| T6st-S1 | Los **editores** se marcan al terminar la orden. El **BOM y «Almacén»** se marcan provisionalmente al empezar (I-3), con la hora del inicio, y la marca se quita al terminar si la orden no cambió el dibujo o era la suya | En el editor nada se escribe mientras corre la orden (la puerta contesta ocupada); en el BOM se puede exportar durante una COPY |
| T6st-S2 | «Cambiar el dibujo» = un evento de objeto de su base de datos mientras corría la orden (no la contabilidad asociativa; una variable de sistema no lo es). **Comprobaciones del smoke (I-2), para decidir después si marcan de más:** ZOOM y PAN en el espacio modelo y DENTRO de una ventana gráfica de una presentación (la ventana gráfica es un objeto: se espera que marque); QSAVE; cambiar de pestaña Modelo/presentación; FIELDEVAL o una regeneración con campos; una matriz asociativa (editarla, y una orden que la recalcule); REGEN; LAYER (marca: cambia el dibujo) | Es lo que el host ya distingue (`DatabaseObjectChanged`, IC-10), sin una lista de órdenes; no se toca el puente ni `SubjectOf` (F4) |
| T6st-S4 | Si la ventana presenta otro dibujo antes de que termine la orden, el veredicto espera a que ese dibujo vuelva a presentarse y lo dan los números (M-2). Un cambio de la base de datos sin ninguna orden no marca | El número es de la sesión; un cambio sin orden no es «una orden de AutoCAD» |
| T6st-S7 | Con la paleta oculta, las órdenes no se ven; al presentarse de nuevo el dibujo, si empezaron órdenes en él y cambió la base de datos, se marcan los editores y el BOM y «Almacén» dicen «Puede no estar al día» (A2). Un ZOOM con la paleta oculta no marca | La respuesta del Owner, sin tocar los manejadores de la paleta |
| T6st-S9 | **M-3:** con el panel en error, o la sesión degradada, a mitad de una orden: la ventana no presenta el dibujo (o lo presenta como degradado), así que la orden se deja y su veredicto espera a que el dibujo vuelva a presentarse sano. Una sesión degradada no recibe eventos: sus números no se mueven y una orden de entonces no marca | La degradación ya apaga la sincronización de esa sesión (F1); no se reabre aquí |
| T6st-S10 | **M-5:** mientras se edita una cabecera embebida de un Selectivo (`SelectiveEditorView.Cabecera.cs:42`, la vista del Selectivo cambia su contenido por el de la cabecera), no se ve el aviso del Selectivo; vuelve a verse al volver al editor del rack. La cabecera embebida nunca se marca (es una copia del borrador) | La vista de la cabecera embebida no tiene la cabecera del rack |
| T6st-S11 | **M-1:** una orden es de la puerta solo si tomó la petición y empezó exactamente una. Una orden transparente durante una de la puerta (un `'ZOOM` mientras se coloca una vista) cuenta como segunda y la vuelve ajena: marca si cambió el dibujo. Las órdenes `EXECUTEFUNCTION` (el contexto de órdenes de la paleta y del piloto) no se cuentan como inicios | Es la cuenta del puente, sin cambiarla; marcar de más solo cuesta un aviso |
| T6st-S12 | **A1:** una escritura de «Variables» marca los editores limpios de Selectivo de los racks que redibujó, en lugar de cerrarlos (VA). La marca va con «Volver a leer del dibujo»; la página lo dice | La respuesta del Owner, «Sí, marcarlo» |
| T6st-S13 | **A3:** la lista solo dice «puede no estar al día» si cambió la base de datos desde su lectura (`DrawingChangeCount`); con el mismo número, la lista que tiene es la del dibujo aunque el host la haya olvidado tras un ZOOM | La respuesta del Owner, «Sí, afinarlo»; la lista sigue sabiendo que hay que releer cuando el dibujo cambió |

**Archivos de la ronda** (marcados `T6 st`):
- Application:
  - `WorkspaceHostSnapshot.cs`: `CommandStartCount`, último parámetro con 0 por omisión;
  - `WorkspaceHostCore.cs`: una línea en `GetSnapshot`, con `_bridge.ForeignCommandStartsOf(id) ?? 0`. El puente, `EventBridgeCore` y
    `IWorkspaceBridgeLink` no cambian.
- UI:
  - `WorkspacePanelViewModel.cs`: `PresentedCommandStartCount`;
  - `RackCadCommandWatch.cs`: rehecho, con un estado por dibujo y los veredictos;
  - `RackCadWindowViewModel.DrawingChange.cs`: los veredictos y `MarkRedrawnByVariables`;
  - `RackCadWindowViewModel.Variables.cs`: marca en lugar de cerrar;
  - `RackCadWindowViewModel.cs`: el número a la lista;
  - `.Bom.cs` y `.Warehouse.cs`: los dos números;
  - `RackBomPageViewModel.cs` y `WarehousePageViewModel.cs`: lo provisional, los veredictos y la regla «Return» por los números;
  - `RackListViewModel.cs`: A3;
  - `RackEditorDrawingChangePanel.xaml`: `QuestionPanel`;
  - `CabeceraEditorViewModel.DrawingChange.cs`: `UpdateBlockedText`.
- Pruebas:
  - `RackCadDrawingChangeTests`: el doble cuenta los inicios y las páginas; pruebas de A2, M-1, M-2, M-4 y A3; la de «un cambio sin orden»
    pasa a la de la paleta oculta;
  - `RackCadBomPageTests` y `RackCadWarehousePageTests`: el doble cuenta los inicios por dibujo. Vuelven a sus aserciones originales las
    cuatro que 5114345e había pasado al final de la orden. ZOOM provisional; COPY con «Exportar» (I-3); lectura fundida (M-1, BOM); vuelta y
    paleta oculta (M-2, A2);
  - `RackCadVariablesPageTests`: el editor limpio se marca y se relee (A1);
  - `WorkspaceDrawingChangeCountTests`: la instantánea lleva los inicios.

**Pruebas de la ronda:**
- `RackCadDrawingChangeTests` (64, antes 56):
  - `ShowingThePaletteAgainMarksTheEditors…` (A2): un ZOOM y un cambio sin orden no marcan, un ERASE con la paleta oculta sí;
  - `ACommandLeftForAnotherDrawingIsJudged…` (M-2): sin marca al irse, y al volver el veredicto lo dan los números;
  - `ACommandOfTheDoorRunTogetherWithAnotherCommandIsNotOwn` (M-1);
  - `NoCommandOfTheDoorMarksTheEditors…` (M-4), ×5: la lectura del BOM, la de «Almacén», la escritura de «Variables», INSERTAR y NUEVO,
    cada una tomando su petición, con un punto y cambiando el dibujo;
  - `TheListIsNotSaidOutOfDateAfterAZoomAndItIsAfterAnErase` (A3).
- `RackCadBomPageTests` (+3):
  - `ExportingDuringACopyThatRunsAsks…` (I-3);
  - `TheReadRunTogetherWithAnotherCommand…` (M-1);
  - `ComingBackOrShowingThePaletteAgain…` (M-2, A2);
  - el ZOOM, con la marca provisional mientras corre.
- `RackCadWarehousePageTests` (+1): `ComingBackOrShowingThePaletteAgain…`, y el ZOOM provisional.
- `RackCadVariablesPageTests`: `TheCleanEditorOfARackTheWriteRedrewIsMarkedSoItIsReadAgain` (A1; antes `…IsClosed…`).
- `WorkspaceDrawingChangeCountTests` (+1): `TheSnapshotCarriesTheCommandStartsOfTheActiveSession…`.
- **Pruebas que fijaban la conducta de antes, al día** (con una línea y una nota cada una):
  - `RackCadWindowTests.AnOutdatedListIsKeptAndSaysSo…` y `…ComingBackToADrawingShowsTheListReadThere…`: la orden que hizo olvidar la lista
    cambia ahora el dibujo (A3);
  - `RackCadBomPageTests.TheReadingsAreKeptPerDrawingAndComingBackToOne…`: volver sin órdenes dice «Al día», y con órdenes que cambiaron el
    dibujo, «Volviste a este dibujo» (M-2);
  - `RackCadWarehousePageTests.TheSectionIsBetweenTheBomAndTheLibrary…`: volver sin órdenes deja «Lectura al día» (M-2).
- **Comprobación en rojo de la ronda** (copias fuera del worktree, restauradas; `git diff` sin restos): seis reversiones a la vez, cada una
  en una parte distinta. Fallaron 12 de 186, justo las suyas:
  - sin el veredicto `Unseen`/`Return` (A2, M-2): las de la paleta oculta (editores, BOM, «Almacén») y la vuelta del BOM;
  - sin «exactamente una orden» (M-1): la de la puerta fundida y la lectura fundida del BOM;
  - sin el veredicto que espera al dibujo (M-2): `ACommandLeftForAnotherDrawing…`;
  - sin la marca provisional del BOM (I-3): la COPY con «Exportar», el ZOOM del BOM y la original de BM-01, que comprueba el aviso al
    empezar;
  - sin la marca de «Variables» (A1): la de «Variables»;
  - sin el número de la lista (A3): la de la lista.
  - En Core, sin `CommandStartCount` en `GetSnapshot`, falló la prueba de los inicios.

**Números de la ronda:**

- **Focales UI** (`RackCadDrawingChangeTests`, `RackCadBomPageTests`, `RackCadWarehousePageTests`, `RackCadVariablesPageTests`,
  `RackCadWindowTests`): 186 correctas, 0 con error, 0 omitidas.
- **Focales Core** (`WorkspaceDrawingChangeCountTests` y las guardas del modelo, del puerto, del host, del piloto y de copia completa:
  `WorkspaceHostPortGuardTests`, `WorkspaceModelBoundaryTests`, `WorkspaceHostSourceGuardTests`, `WorkspaceHostDrainTests`,
  `WorkspacePilotEditGuardTests`, `WorkspaceFullCopyGuardTests`, `WorkspaceBrowserBoundaryTests`): 289 correctas, 0 con error, 7 omitidas.
- **La solución en Debug** (Plugin incluido): 0 errores.
- **Suites completas:** no se corrieron en esta ronda (M-7). Las de 5114345e daban UI 3168/0/48 y Core Full 19485/0/403; esta ronda añade
  8 pruebas a `RackCadDrawingChangeTests`, 3 al BOM, 1 a «Almacén» y 1 a Core (se esperan UI 3180/0/48 y Core 19486/0/403).
- Sin caracteres de control ni finales de línea mezclados en los 21 archivos de la ronda; las copias de la comprobación en rojo, restauradas
  e idénticas.

## Sexta tanda, at: «Actualizar todo» (2026-11-27)

**Rama:** local `architecture/workspace-persistente-rackcad-t6-at`, desde `f02f519b`. Sin commit, sin push, sin AutoCAD.

**La orden del Owner (2026-11-27):** «También hay que continuar con el actualizar todo, basicamente dejsr terminada I-64». Las decisiones
del 2026-11-23 (replaneación de I-64) y la especificación de resultados de la A-14, C-05 («D-09 y D-20: «Actualizar todo»»).

### Lo que se construyó

- **En la ventana:**
  - **«Actualizar todo (N)»** en la cabecera de la ventana, junto a «Ayuda», fuera de los editores. N es el número de borradores que
    escribiría.
    - Apagado, con su motivo en la pista: si no hay nada que escribir, si otra petición de la ventana espera a AutoCAD, o si un borrador
      lo impide.
    - Si un borrador lo impide, un aviso bajo la cabecera lo nombra con su causa.
  - **El panel de «Actualizar todo»** (bajo la cabecera, como el aviso de la espera):
    - la pregunta en línea cuando un borrador está marcado «posiblemente desactualizado»: «Escribir todo de todos modos» o «Cancelar»;
    - el aviso de lo que lo impide;
    - al terminar, el resumen y la lista: cada borrador con su sistema y «Escrito», «Escrito, con avisos», «No se pudo escribir» (con su
      causa), «No aplicado» («No aplicado: falló otro rack del conjunto»), «Cancelado» o «Se desconoce». «Entendido» la quita.
  - **Mientras dura:** el aviso de la espera de siempre, con su texto propio («Esperando a que AutoCAD escriba N borradores con
    «Actualizar todo»…»), y «Dejar de esperar» hasta que la orden toma la petición. Después: «AutoCAD está escribiendo los borradores de
    «Actualizar todo» en una sola operación; la barra de estado de AutoCAD muestra el progreso y ESC lo cancela antes de confirmar».
  - **Al terminar,** cada editor queda como tras su propio «Actualizar»: «Limpio» sobre lo que se envió, o «Modificado» con su causa. La
    página «Variables», como tras el suyo: cierra los editores limpios de los racks que redibujó y vuelve a leer el dibujo.
- **Application** (`src/RackCad.Application/VentanaRackCad/RackCadUpdateAll.cs`, nuevo; sin AutoCAD, con sus pruebas en Core):
  - `UpdateAllItem`, `UpdateAllWrite`, `UpdateAllItemResult`, `UpdateAllWritten` y la puerta `IRackCadUpdateAllDoorPort`;
  - `UpdateAllEligibility`, la condición 4 de la A-14 adaptada (abajo);
  - `UpdateAllRun`, la composición pura sobre un puerto del anfitrión (`IUpdateAllUnitPort`):
    1. abre UNA transacción exterior;
    2. relee y compara TODOS los borradores dentro de ella, sin escribir;
    3. si alguno se rechaza, cero escrituras, y cada rechazo con su causa;
    4. escribe en orden; si uno falla, deshace todo;
    5. ESC solo antes de confirmar;
    6. confirma una vez; si la confirmación falla, todos «Se desconoce».
  - `UpdateAllText`: los textos, compartidos por la ventana y la orden.
- **Plugin:**
  - `RACKACTUALIZARTODO` (`src/RackCad.Plugin/VentanaRackCad/RackCadUpdateAllCommands.cs`, nuevo): una orden (un «U»), sin
    `NoUndoMarker`. Toma la petición de la puerta y ejecuta `UpdateAllRun` sobre su anfitrión de AutoCAD:
    - las comprobaciones de cada sistema son las de su orden APLICAR;
    - las escrituras llaman a las mismas funciones de aplicar (tabla abajo).
  - La transacción exterior (`src/RackCad.Plugin/Systems/Shared/RackUpdateAllTransaction.cs`, nuevo): fuera de la carpeta de la puerta,
    que no abre transacciones (guarda de capas).
  - La puerta (`src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.UpdateAll.cs`, nuevo), con las reglas de las demás: la petición se
    guarda ANTES de entregar la orden, con el dibujo activo y sus propias copias; «Dejar de esperar» la descarta (`AbandonUpdateAll`).
  - `BlockLibraryImporter.ImportFailures` (un contador; la importación no cambia), para que una importación de la biblioteca que falle
    dentro de la transacción sea un fallo del conjunto (T6at-S3).
- **Lo que no cambia:**
  - el «Actualizar» de cada rack;
  - la costura de un rack de I-55 (`RackSiblingRedrawRun`, `ISiblingRedrawPort`, `SiblingRedrawTransaction`) y su contrato. No se tocó
    ningún archivo de la costura: «Actualizar todo» es una composición nueva por encima de las órdenes APLICAR;
  - las funciones de aplicar de los seis sistemas y de «Variables»: se llaman tal cual.

### La elegibilidad (condición 4 de la A-14, adaptada a los seis editores)

| Borrador del dibujo activo | «Actualizar todo» |
|---|---|
| «Modificado» y su «Actualizar» puede escribir | Entra |
| Limpio | No entra |
| De «Nuevo», sin insertar (necesita su punto de inserción) | No entra (no lo impide) |
| De otro dibujo | No entra: espera a su dibujo |
| Escribiéndose | Lo impide: «AutoCAD lo está escribiendo; espera a que termine» |
| Su «Actualizar» bloqueado (un valor marcado, un error, la secuencia sin aplicar, la pestaña «Propiedades» no válida, «Variables» con VA-04) | Lo impide, con el motivo de su propio «Actualizar» |
| Marcado «posiblemente desactualizado» (la marca de la parte st) | Entra, y «Actualizar todo» pregunta antes, en línea |

- **El orden:** «Variables» primero (decisión Q-02); después los racks por sistema, en el orden de la lista (Selectivo, Push Back,
  Dinámico, Cantilever, Cabecera, Cama); dentro de cada sistema, por nombre.
- **El punto para la parte st:** `RackCadWindowViewModel.UpdateAllStaleProbe`, un predicado (dibujo, rack; `null` para «Variables») →
  marcado. La sesión lo conecta a la marca de st al integrar; las pruebas lo sustituyen por un doble.

### Sistema por sistema: ¿su aplicar cabe en la transacción exterior?

Revisión de solo lectura de las ocho funciones, hasta la API de AutoCAD. Todo lo que escriben en la base de datos pasa por
`TransactionManager.StartTransaction()`. Con una transacción ya abierta, esas transacciones son anidadas: lo que confirman no llega al
dibujo hasta que se confirma la exterior, y si la exterior se aborta, se descarta. Ninguna pide nada al usuario: ni prompts, ni jig, ni
ventanas; tampoco hay `SendStringToExecute`. Ninguna escribe archivos ni configuración. Lo demás:

| Sistema | Función de aplicar (la de su orden APLICAR) | Lo que hace fuera de sus transacciones | Entra |
|---|---|---|---|
| Dinámico | `RackDinamicoCommands.ApplyDynamicEdit` | importa bloques de la biblioteca antes de cada vista; purga lo no referenciado; un `Regen` | Sí, con T6at-S2 a S4 |
| Selectivo | `RackSelectivoCommands.ApplySelectiveEdit`, con `RackDoorRules.PlanSelectiveApply` y el registro releído | lo mismo; además una lectura del nombre del bloque con `StartOpenCloseTransaction` (solo lectura; si falla, el prefijo de las definiciones anidadas es «Corte») | Sí, con T6at-S2 a S4 |
| Push Back | `RackPushBackWindowDrawing.Apply`, solo en sitio (sin vistas nuevas) | lo mismo que Dinámico | Sí, con T6at-S2 a S4 |
| Cantilever | `RackCantileverCommands.ApplyCantileverEdit`, solo en sitio | no importa bloques; un `Regen` | Sí, con T6at-S4 |
| Cama | `RackCamaCommands.ApplyCamaUpdate` | importa; un `Regen` dentro de cada llamada (`FlowBedDrawService`, `regen: true`) | Sí, con T6at-S2 a S4 |
| Cabecera | `RackCabeceraCommands.ApplyCabeceraUpdate` | lo mismo que Selectivo | Sí, con T6at-S2 a S4 |
| Pestaña «Propiedades» | `RackPropertiesDoorData.WithRackProperties` → `CustomPropertiesExecutor.ExecuteRackDraft` | nada | Sí |
| «Variables» | `ProjectVariableMutationExecutor.Execute` y `CustomPropertiesExecutor.ExecuteProjectDraft` | importa en su preparación; purga y `Regen` tras su transacción | Sí, con T6at-S2 a S4 y S6 |

- **Excluido, con su motivo:**
  - **«Insertar vista…» y los racks nuevos:** `RackViewBatchDriver.Place` se niega con una transacción abierta (`TOP_TRANSACTION_OPEN`).
    Además, colocar vistas pide puntos con el jig. La puerta rechaza un elemento que inserte (`UpdateAllItem.IsWellFormed`), y la ventana
    no los incluye.
- **Hallazgo de la revisión, anterior a esta tanda y sin tocar:** en `RackCommandSupport.EraseViewBlocks`, el contador de vistas borradas
  sube antes de confirmar. Si falla a mitad, la transacción se deshace, pero el recuento devuelto las cuenta. Es igual en el «Actualizar»
  de cada rack. Para la planeación.

### Las decisiones (supuestos para el Owner)

| Id | Decisión | Por qué |
|---|---|---|
| T6at-S1 | Las funciones de aplicar de cada sistema abren sus transacciones, que quedan **anidadas** en la exterior. La condición 1 de la A-14 dice «ninguna lectura ni unidad abre otra transacción» | El encargo pide reusar las funciones de aplicar de las órdenes APLICAR. La forma estricta sería la de `ProjectVariableMutationExecutor`: preparar fuera, mutar con fachadas que reciben la transacción del llamador (`RedrawInTransaction`) y regenerar al final. Exige reescribir el aplicar de los seis sistemas: queda para la planeación |
| T6at-S2 | **Corregido en la revisión de `c4985a2e` (I-2).** Dentro de la transacción no se importa nada: lo que falta se importa fuera de ella y el conjunto se vuelve a escribir. Ver «Correcciones de la revisión (sobre `c4985a2e`)» | El importador debe correr fuera de la transacción del dibujo |
| T6at-S3 | Cada aplicar regenera, como hoy, dentro de la transacción; «Actualizar todo» regenera una vez más al final, tras confirmar o abortar | Quitar esas regeneraciones exige tocar las seis funciones. Cuesta una regeneración por rack, lo mismo que N «Actualizar»; el smoke lo mide |
| T6at-S4 | **Corregido en la revisión de `c4985a2e` (B-1).** Un rack cuenta como escrito solo si se redibujaron o borraron todas sus vistas y, releído dentro de la transacción, sus vistas son las que deben quedar y llevan un mismo diseño. Si no, falla el conjunto | Los ayudantes del redibujo se tragan el fallo de una vista, y el rack quedaría a medias dentro de una operación que se confirma |
| T6at-S5 | Si la pestaña «Propiedades» de un rack no se puede escribir, falla el conjunto entero. En el «Actualizar» de un rack, el rack queda escrito y las propiedades no | Todo o nada sobre lo authored: en un conjunto, un rack escrito a medias también es «a medias» |
| T6at-S6 | **«Variables» entra** (decisión Q-02), la primera. Si redibujaría un rack que también va en el conjunto, no se escribe nada y se nombran los dos. Un Selectivo se vuelve a planear sobre el registro releído tras «Variables» (`PlanSelectiveApply`), y si ya no coincide, falla el conjunto. Sus dos transacciones (VA-S7) quedan dentro de la exterior: en el conjunto son todo o nada | Así ningún borrador de rack se escribe resuelto contra variables viejas: los que «Variables» redibuja no pueden ir en el conjunto (y la ventana ya los bloquea con VA-04), y los demás no dependen de lo que cambia. La comprobación de la orden es la autoridad |
| T6at-S7 | **Cancelar:** «Dejar de esperar» hasta que la orden toma la petición; después, ESC en AutoCAD (`HostApplicationServices.UserBreak`), que se mira antes de cada escritura y antes de confirmar. Cancelado, no se escribe nada | La paleta no puede atender clics mientras la orden corre: es el mismo hilo de AutoCAD |
| T6at-S8 | **El progreso:** en la ventana, el aviso de la espera; en AutoCAD, la barra de progreso de la barra de estado (`ProgressMeter`) y una línea por fase en la línea de órdenes («comprobando N borradores…», «escribiendo N borradores…») | La ventana no se repinta mientras corre la orden |
| T6at-S9 | Si la confirmación (`Commit`) lanza, todos los incluidos quedan «Se desconoce»: el editor sigue «Modificado», con «No se sabe si AutoCAD lo escribió… Vuelve a leerlo del dibujo antes de seguir». Un fallo después de confirmar (la regeneración final) es un aviso en el resumen y no deshace nada | La A-14 pide «Desconocido» con «Comprobar ahora» por rack. Los editores de la ventana no tienen ese estado; releer es «Volver a leer del dibujo» de la parte st, o cerrar el editor y «Editar aquí» |
| T6at-S10 | La condición 3 de la A-14 (texto pendiente en un borrador no abierto) la cubren los bloqueos de cada editor (la secuencia sin aplicar de Cantilever, los tramos de Selectivo, los valores marcados) y el foco de WPF, que confirma el campo al pulsar el botón de la cabecera | No se conoce un camino que deje texto conservado en un borrador no abierto, como ya decía la A-14 |
| T6at-S11 | La pregunta «posiblemente desactualizado» es de «Actualizar todo» (su panel) y nombra los borradores marcados; la marca y el «Actualizar» por rack son de la parte st | El encargo: no construir la marca, dejar el punto (`UpdateAllStaleProbe`) |
| T6at-S12 | Cada borrador del conjunto lleva el número de la petición de «Actualizar todo», y su escritura es la que mandaría su «Actualizar», con la pestaña «Propiedades» | Así la orden escribe lo mismo que cada APLICAR, y las pruebas lo comparan campo por campo |
| T6at-S13 | **Corregido en la revisión de `c4985a2e` (menor 7).** El coste de N barridos: de 5 a 9 barridos por rack, más dos lecturas del registro de variables por Selectivo. Si hace falta importar bloques, el conjunto se lee y se escribe dos veces | La captura compartida es una mecánica posible, no congelada; el smoke mide el tiempo con 43 a 70 racks y los umbrales los fijan Coordinator y Owner (D-18) |

### Para integrar con st (aviso a la sesión)

- `UpdateAllStaleProbe` se conecta a la marca de st.
- Si st quita la marca en el «escrito» de cada editor (su `MarkWritten`), «Actualizar todo» la quita también, porque deja cada editor con
  las mismas llamadas que su «Actualizar» (`PushBackWritten`, `CamaWritten`…, y para Dinámico y Selectivo, las de `OnDesignWritten`). Si
  st la quita en `OnDesignWritten`, hay que añadir esa línea en `WrittenOf` (`RackCadWindowViewModel.UpdateAll.cs`).

### Las ediciones a archivos compartidos (pequeñas, marcadas `T6 at`)

- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.cs`, cuatro líneas:
  - `WireUpdateAllDoor(door)` en el constructor;
  - el texto de espera de «Actualizar todo» en `WaitingText`;
  - su rama en `StopWaiting`;
  - `RaiseUpdateAll()` en `RaiseWaiting`.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowViewModel.Variables.cs`: lo que sigue a una escritura de la página pasa a
  `VariablesWrittenFor`, que comparten su «Actualizar» y «Actualizar todo». Sin cambio de conducta.
- `src/RackCad.UI/VentanaRackCad/RackCadWindowView.xaml`: el botón en la cabecera y el panel bajo el aviso de la espera, en dos bloques. Los
  clics van en `RackCadWindowView.UpdateAll.cs` (nuevo).
- `src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs`: `AbandonUpdateAll` en `Abandon`.
- `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs`: el contador `ImportFailures`, sin cambiar la importación.
- Archivos nuevos: `RackCadUpdateAll.cs` (Application), `RackCadWindowViewModel.UpdateAll.cs` y `RackCadWindowView.UpdateAll.cs` (UI),
  `RackCadUpdateAllCommands.cs`, `RackCadRackDoorPort.UpdateAll.cs` y `Systems/Shared/RackUpdateAllTransaction.cs` (Plugin).

### Censos y guardas que cambian, con su nota

- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 70 a 71 (`RACKACTUALIZARTODO`).
- `CustomPropertiesCommandGuardTests`: `A14UpdateAllCommands` en la base.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `RackCadUpdateAllCommands.cs` es un archivo de órdenes de la puerta,
  como los de APLICAR. Sigue sin `LockDocument` ni `StartTransaction`: su transacción exterior vive en `Systems/Shared`.
- `PushBackEditAllNameWritersGuardTests` (CT-PBT1B-15): el censo de archivos del Plugin que nombran Push Back pasa de 19 a 21, con
  `RackCadUpdateAllCommands.cs` y `RackCadRackDoorPort.UpdateAll.cs`. Ninguno escribe un `Name`: las siete asignaciones y su orden no
  cambian.
- Ninguna guarda se debilitó.

### Las pruebas nuevas

- **Core, `UpdateAllRunTests` (13):**
  - la elegibilidad (3): quién entra y quién no, el orden, lo que lo impide con su causa, la marca de st;
  - la composición sobre un doble del anfitrión (8):
    - la relectura de todos dentro de la transacción antes de escribir, y la confirmación una vez;
    - un rechazo de la relectura (y una lectura que lanza): cero escrituras, cada rechazo con su causa;
    - una escritura que falla (o lanza): las anteriores se deshacen y las siguientes no se escriben;
    - la confirmación que falla: «Se desconoce»;
    - cancelar antes, entre escrituras y justo antes de confirmar;
    - «Variables» que redibujaría un rack del conjunto;
    - los avisos y un fallo después de confirmar;
    - una petición que no puede empezar o que no es válida (un rack nuevo, «Insertar vista…», dos escrituras);
  - la orden y la puerta por su fuente (2):
    - una orden, con Deshacer;
    - una transacción exterior y una regeneración al final;
    - las comprobaciones y las funciones de aplicar de cada sistema;
    - Push Back solo en sitio;
    - el contador de importaciones;
    - la petición guardada antes de entregarla y `AbandonUpdateAll`.
- **UI, `RackCadUpdateAllTests` (16):**
  - por cada uno de los seis editores (teoría, 6 casos): su escritura en «Actualizar todo» es, campo por campo, la de su «Actualizar»
    (diseño, sistema, nombre, pestaña «Propiedades», estados vinculados), el editor queda «Escribiendo» y después «Limpio» con el mensaje;
  - los seis editores y «Variables» en una petición, «Variables» primero:
    - ningún otro «Actualizar» mientras dura;
    - la orden que la tomó;
    - un Push Back que falla al escribirse: los siete siguen «Modificado», la lista dice cuál y por qué, y los demás «No aplicado»;
    - después, todo escrito y la página que vuelve a leer;
  - un rechazo de la relectura y una confirmación que falla;
  - limpios, borradores de «Nuevo» y borradores de otro dibujo no entran;
  - un valor marcado lo impide con su causa, y al corregirlo entra;
  - VA-04: «Variables» con un Selectivo modificado que usa la variable lo impide;
  - otra petición que espera lo impide; «Dejar de esperar» deja editar, y una respuesta tardía no hace nada;
  - la puerta que lo rechaza;
  - la marca de st: la pregunta en línea, «Cancelar» y «Escribir todo de todos modos»;
  - **la vista con WPF real (STA):**
    - el botón con su número, su nombre accesible y su motivo;
    - la pregunta en línea;
    - la lista con un nombre accesible por línea;
    - ningún nombre de tipo, nombres únicos y ningún error de enlace;
    - estrecha, a 600.

### Comprobación en rojo

Cinco piezas rotas a propósito, una a una, con una copia del original fuera del worktree. Tras cada una corrieron las dos clases nuevas, y
el archivo se restauró de sus bytes y se comprobó igual por su huella. Al final, `git diff` no tiene restos (`if (false`, `true ||`).

| Rotura | Lo que falla |
|---|---|
| R1. Todo o nada en la relectura: la composición escribe aunque una relectura se rechace | Core: `ARefusalOfTheReReadWritesNothingAndEveryRefusalIsNamedWithItsCause` y `VariablesThatWouldRedrawARackWithADraftOfTheSetWriteNothingAndNameBoth`. UI: `ARefusalOfTheReReadNamesItsDraftAndEveryOtherIsNotApplied` |
| R2. Todo o nada en la escritura: una escritura que falla no deshace las anteriores | Core: `AWriteThatFailsDiscardsTheOnesBeforeItAndNoneAfterItIsWritten`. UI: `TheSixEditorsAndThePageVariablesGoInOneRequestVariablesFirstAndOneFailureWritesNone` y la de la vista |
| R3. Elegibilidad: un borrador con su «Actualizar» bloqueado entra | Core: `NothingWithChangesSaysSoAndADraftThatIsWritingOrBlockedPreventsItWithItsCause`. UI: `ADraftWhoseActualizarIsBlockedPreventsItWithItsCauseAndNothingGoes` y `TheVariablesOfARackWithADraftPreventItAsTheirOwnActualizarDoes` |
| R4. Elegibilidad: un borrador de «Nuevo» entra | Core: `ModifiedDraftsGoCleanOnesAndNewOnesDoNotAndVariablesGoFirstThenTheSystemsOfTheListThenTheName`. En UI no falla: un borrador de «Nuevo» recién creado no está modificado |
| R5. La pregunta: la ventana no pregunta por un borrador «posiblemente desactualizado» | UI: `ADraftMarkedPossiblyStaleIsIncludedAndAsksFirstInLine` y la de la vista |

### Los números

- **UI completa:** 3126 correctas, 0 con error, 48 omitidas (3174). Es la base, 3110, más las 16 nuevas.
- **Core Full:** 19494 correctas, 2 con error, 403 omitidas (19899). Es la base, 19483, más las 13 nuevas. Los dos errores eran míos y se
  corrigieron después:
  - `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`: la clase nueva declaraba `RackCad.Tests.Workspace`;
    ahora declara `RackCad.Tests`, como las demás;
  - `PushBackEditAllNameWritersGuardTests.CT_PBT1B_15…`: el censo de archivos que nombran Push Back pasa de 19 a 21 (ver «Censos y
    guardas»).
  - Tras corregirlos volvieron a correr esas dos guardas, la clase nueva y los tres censos: 154 de 154.
  - **La Core Full repetida sobre el árbol final:** 19496 correctas, 0 con error, 403 omitidas (19899).
- **La primera UI completa** dio 6 errores de enlace («BindingExpression path error: 'Error' property not found…») en pruebas de vistas
  de seguridad y de Push Back de otras partes. Corrió a la vez que la Core Full, con las suites de otras seis partes en la máquina.
  - Esas pruebas pasan solas, y también con la clase nueva y sus propias clases (91 de 91). La segunda UI completa pasó entera.
  - Las pruebas de UI comparten un solo hilo STA y una fuente de trazas de enlace global: es interferencia entre pruebas bajo carga.
  - La prueba de la vista nueva tampoco da errores de enlace cuando se vacía la cola del despachador antes de mirar.
- **Solución en Debug (Plugin incluido), con `--no-incremental`:** 0 errores. Las advertencias son las de siempre: los analizadores de
  xUnit y MSB3277.

### Lo que comprobará el smoke en AutoCAD

1. **Un solo «U».** Tres racks de sistemas distintos, «Modificado»: «Actualizar todo (3)», y después un «U» los deshace los tres, con una
   sola entrada en la lista de Deshacer.
2. **Cero escrituras si uno falla.**
   - En la relectura: tras abrir los editores, borra en AutoCAD la vista de uno; «Actualizar todo» no escribe ninguno y nombra el que falta.
   - En la escritura: haz fallar la escritura de uno solo de los racks (por ejemplo, con la capa de su vista bloqueada; si así no falla,
     otra forma). Ningún rack cambia y la lista nombra el que falló.
   - En los dos casos, comprobar en el sobre de los otros racks que su diseño sigue igual.
3. **El tiempo con muchos racks:** el dibujo de la prueba de 43 racks (o 70): «Actualizar todo» con 10 y con todos modificados, con el
   tiempo y la regeneración (T6at-S3). Ahí se ve si las regeneraciones por rack pesan.
4. **La importación dentro de la transacción (T6at-S2):** un borrador que pida un bloque de la biblioteca que el dibujo no tiene (una
   seguridad nueva). Comprobar si se importa o si «Actualizar todo» dice que no pudo. Y tras un fallo de otro rack, qué queda en la tabla
   de bloques.
5. **ESC** durante una escritura larga: no se escribe nada (T6at-S7).
6. **«Variables» y racks juntos:** la variable cambia, y un rack sin vínculo y otro con vínculo modificado; lo segundo lo impide (VA-04).
7. **La pantalla tras abortar:** que se vea lo que hay en el dibujo, sin restos dibujados de lo deshecho (la regeneración final).

### Lo que queda

- **El smoke en AutoCAD:** la lista de arriba. Hasta entonces, el todo o nada se ha probado sobre la composición pura y sobre la forma del
  código de la orden.
- **Al integrar con st:** conectar `UpdateAllStaleProbe` y comprobar que una escritura correcta de «Actualizar todo» quita la marca (ver
  «Para integrar con st»).
- **Según lo que diga el Owner o el smoke:**
  - la forma estricta de la transacción (Q-at-1);
  - quitar las regeneraciones por rack (T6at-S3) y una captura compartida del dibujo (T6at-S13), si el tiempo con muchos racks pesa;
  - un estado «Desconocido» con «Comprobar ahora» en los editores (T6at-S9).

### Preguntas para el Owner

Respondidas el 2026-11-27: ver «Las respuestas del Owner» en «Correcciones de la revisión (sobre `c4985a2e`)».

- **Q-at-1 (T6at-S1):** ¿se acepta «Actualizar todo» con las transacciones de cada sistema anidadas en la exterior, o se pide la forma
  estricta de la A-14 (una sola transacción, sin transacciones de las unidades)? La segunda exige reescribir el aplicar de los seis sistemas
  en la forma de `ProjectVariableMutationExecutor`.
- **Q-at-2 (T6at-S5):** si las propiedades de un rack no se pueden escribir, ¿falla el conjunto entero (lo construido) o el rack se da por
  escrito, como en su «Actualizar»?
- **Q-at-3 (T6at-S6):** ¿«Variables» dentro de «Actualizar todo», como se construyó, o fuera hasta su propia A-n?
- **Q-at-4 (T6at-S9):** sin un «Comprobar ahora» por rack, ¿basta que el editor diga «vuelve a leerlo del dibujo»?

### Correcciones de la revisión (sobre `c4985a2e`)

La revisión independiente de la parte at sobre `c4985a2e` (el commit de la sesión) dio un bloqueante, dos importantes y nueve menores.
Las correcciones van sobre ese commit, sin commit propio.

#### Las respuestas del Owner (2026-11-27)

| Pregunta | Respuesta, literal | Lo que queda |
|---|---|---|
| Q-at-1 (T6at-S1) | «Aceptar las anidadas» | Las transacciones de cada sistema quedan anidadas en la exterior. No hay que reescribir el aplicar de los seis sistemas |
| Q-at-2 (T6at-S5) | «Falla todo el conjunto» | Si la pestaña «Propiedades» de un rack no se puede escribir, falla el conjunto, como se construyó |
| Q-at-3 (T6at-S6) | «Incluir Variables» | «Variables» entra, la primera, como se construyó |
| Q-at-4 (T6at-S9) | «Basta «vuelve a leerlo»» | Tras una confirmación que falla, «Se desconoce» con «vuelve a leerlo del dibujo». Además lo impide el siguiente «Actualizar todo» (menor 1) |

#### Punto por punto

| Punto | Qué pasaba | Qué se hizo |
|---|---|---|
| **B-1** (bloqueante) | Si el redibujo de una vista fallaba, los ayudantes se tragaban la excepción (`SystemBlockWriter.RedrawInPlace`, `LateralHeaderDrawService.RedrawInPlace`; Cantilever sigue con la vista siguiente). El rack salía «Escrito, con avisos» y la transacción exterior se confirmaba con el rack a medias | Tras cada aplicar, y tras su pestaña «Propiedades», el rack se relee dentro de la transacción (`FindRackBlocks`) y se compara (`RackSiblingDesigns.Compare`). Lo juzga una regla pura, `UpdateAllVerification.AfterRackWrite`: cada vista se redibujó o se borró, quedan las que deben quedar y llevan un mismo diseño. Si no, falla el conjunto. Las vistas que faltan ya no son un aviso. **T6at-S4 rehecho** |
| **I-1** | `RackCommandSupport.EraseViewBlocks` subía el recuento antes de confirmar. Si fallaba a mitad, la transacción se deshacía, pero el recuento contaba las vistas | Cuenta en una variable propia y la devuelve tras el `Commit`. Solo cambia el caso de fallo del «Actualizar» por rack (T6at-S15). La relectura de B-1 también lo cubre |
| **I-2, la importación** | La biblioteca se importaba dentro de la transacción exterior, en contra de su regla («OUTSIDE the drawing transaction»). La tabla de bloques puede quedar abierta para escritura tras el primer rack, y `WblockCloneObjects` fallaría en uno posterior | Mientras la transacción exterior está abierta, `BlockLibraryImporter.EnsureForPlan` no importa (`RackUpdateAllScope`, nuevo, en `Systems/Shared`). Anota lo que falta en el dibujo: lo que la biblioteca tiene va a importarse; lo que no tiene es un aviso del borrador. Si una pasada necesita bloques: (1) se deshace; (2) `ImportOutside` los importa sin ninguna transacción abierta, como hace cada aplicar antes de la suya; (3) el conjunto se relee y se escribe otra vez. Si siguen faltando, o la importación falla, fallan los borradores que los necesitan (T6at-S16) |
| **I-2, la purga** | `PurgeUnreferenced`, que debe correr sobre el dibujo confirmado, corría dentro de la transacción exterior | Con el ámbito activo, los ayudantes solo juntan los candidatos. La orden purga una vez, tras el `Commit` de la exterior y en la misma orden (un «U») |
| **I-2, `UpgradeOpen`** | Fallaría sobre objetos ya abiertos para escritura en la transacción exterior: la definición que se acaba de redibujar (`RackBlockRenamer.SyncName`) y el NOD que abrió el registro de variables (`CustomPropertiesData.Write`) | `if (!x.IsWriteEnabled) x.UpgradeOpen();` en los dos. El «Actualizar» por rack no cambia |
| Menor 1 | «Se desconoce» no impedía el siguiente «Actualizar todo» | Se guarda (dibujo, rack) con su editor. Mientras ese editor siga modificado, es un bloqueo con su motivo: «No se sabe si el último «Actualizar todo» lo escribió…». Se quita al releer (cerrar y abrir con «Editar aquí» da otro editor) o al descartar (T6at-S17) |
| Menor 2 | El aviso de lo que lo impide salía sin pulsar nada | Sale solo tras el clic, en el panel. Antes, la pista del botón da el motivo (T6at-S18) |
| Menor 3 | Los bloques de biblioteca que faltan solo avisaban en la cama | El ámbito anota, por borrador, lo que no tienen ni el dibujo ni la biblioteca. En los seis sistemas es «Escrito, con avisos»: «Faltan en la biblioteca de bloques X: se escribió sin esas piezas, como en su «Actualizar»» (T6at-S19) |
| Menor 4 | La línea de órdenes decía «… actualizado…» por cada aplicar aunque el conjunto se deshiciera | La última línea lo dice: «RackCad «Actualizar todo»: lo que dicen las líneas anteriores se deshizo. No se escribió nada: …». Los mensajes de cada aplicar no se suprimen: habría que tocar las seis funciones (T6at-S14) |
| Menor 5 | La condición 3: un valor escrito sin salir del campo no cuenta hasta Intro o hasta salir del campo. Si es el único cambio, el botón sale apagado | El motivo lo dice: «No hay borradores con cambios sin escribir en este dibujo. Un valor que acabas de escribir cuenta cuando pulsas Intro o sales de su campo.» Añadido al smoke |
| Menor 6 | La fila repetía «No aplicado. No aplicado: falló otro rack…», y dos racks con el mismo nombre daban el mismo nombre accesible | La causa empieza por lo que falló («Falló otro rack del conjunto.»), y cada fila lleva su posición: «1. PB 1 (Push Back): …» |
| Menor 7 | T6at-S13 contaba mal | De 5 a 9 barridos por rack: de 4 a 8 con «Propiedades», más la relectura de B-1. Más dos lecturas del registro por Selectivo, y el doble si hay que importar. Corregido en la tabla |
| Menor 8 | R4 no fallaba en UI: el borrador de «Nuevo» de la prueba no estaba modificado | `CleanDraftsNewDraftsAndDraftsOfAnotherDrawingDoNotGo` lo modifica |
| Menor 9 | Las suites completas eran previas al commit | Vueltas a correr sobre el árbol final (abajo) |

#### Los supuestos nuevos

| Id | Decisión | Por qué |
|---|---|---|
| T6at-S14 | Dentro de «Actualizar todo» siguen saliendo los mensajes de línea de órdenes de cada aplicar. Si el conjunto no se confirma, la última línea dice que se deshizo | Suprimirlos exige tocar las seis funciones de aplicar, compartidas con RACKEDITAR |
| T6at-S15 | `EraseViewBlocks` devuelve las borradas solo tras confirmar. En el «Actualizar» de un rack, un borrado que falla a mitad ya no se anuncia como hecho | Era un recuento falso. El caso de éxito no cambia |
| T6at-S16 | Si una pasada necesita bloques de la biblioteca que el dibujo no tiene, se deshace, se importan fuera y se repite. El coste se dobla solo en ese caso. Un bloque que sigue faltando tras importarlo falla sus borradores | Es la fase previa que pedía la revisión, sin calcular a mano lo que necesita cada sistema: se ven exactamente los planos de cada aplicar. Lo importado se queda aunque el conjunto falle (el residuo de biblioteca de OM-5) |
| T6at-S17 | «Se desconoce» impide el siguiente «Actualizar todo» mientras el mismo editor siga modificado. Releer (otro editor) o descartar lo quita | La condición 4 de la A-14: un estado que pide acción del usuario |
| T6at-S18 | Lo que impide «Actualizar todo» solo sale en el panel tras el clic | Ningún aviso sin un gesto; la pista del botón lo dice antes |
| T6at-S19 | El aviso de bloques que faltan nombra las claves de la biblioteca, en los seis sistemas | Es lo mismo que el «Actualizar» de cada rack deja fuera, ahora dicho |

#### Para integrar con st (preparado)

- **RACKACTUALIZARTODO es una orden propia de la puerta.** No debe marcar «El dibujo cambió». La sesión la añade a lo que st cuenta como propio, junto a las APLICAR: es la constante `RackCadRackDoorPort.UpdateAllCommand`.
- **La marca:** `UpdateAllStaleProbe` se conecta a `DrawingChange.IsChanged` y `AsksBeforeWrite`: (dibujo, rack) → la marca del editor de ese rack.
- **El camino de éxito ya pasa por lo mismo que el «Actualizar» de cada editor, así que st quita su marca igual:**
  - Push Back, Cantilever, cama y cabecera, por sus `…Written`;
  - Dinámico y Selectivo, por su `MarkWritten`, en `WrittenOf`;
  - «Variables», por `EndWriting`.

#### Lo que comprobará además el smoke en AutoCAD

- Un rack que importe un bloque de la biblioteca, colocado en SEGUNDO lugar del conjunto. Se importa fuera de la transacción, el conjunto se escribe en la segunda pasada, y un solo «U» lo deshace.
- Renombrar un rack y mirar el nombre de sus bloques (`SyncName` con la definición ya abierta).
- Tras confirmar, contar las definiciones huérfanas (la purga diferida) y comprobar que ninguna vista pierde geometría.
- La condición 3: escribir en un campo sin salir de él y pulsar «Actualizar todo». La pista dice el motivo; al salir del campo, el botón se enciende.
- Las vistas de cada sistema tras escribir. Si un sistema dejara a propósito una vista sin tocar, ahora el conjunto lo rechazaría (B-1): comprobarlo con racks de cada sistema y todas sus vistas.

#### Pruebas nuevas y cambiadas

- **Core, `UpdateAllRunTests`:**
  - `ARackHalfWrittenIsNotWrittenInsideTheSet` (B-1: la regla pura);
  - `BlocksTheDrawingLacksAreImportedOutsideTheTransactionAndTheSetIsWrittenAgain` y `AnImportThatFailsOrBlocksStillLackingAfterItWriteNothingAndNameTheDraftsThatNeedThem` (I-2);
  - `TheSetReReadsEveryRackAfterItsWriteImportsOutsideItsTransactionAndPurgesAfterTheCommit`: por su fuente, la relectura, el ámbito, la importación fuera, la purga tras el `Commit`, `UpgradeOpen`, el recuento de borradas y los menores 3 y 4.
- **UI, `RackCadUpdateAllTests`:**
  - «Se desconoce» lo impide hasta descartar o releer;
  - el aviso solo tras el clic;
  - el borrador de «Nuevo» modificado;
  - las filas con su posición y sin repetir;
  - el motivo con la pista del valor escrito.

#### Comprobación en rojo

Cinco piezas rotas a propósito, una a una, con una copia del original fuera del worktree. Tras cada una corrieron `UpdateAllRunTests`
(17) y `RackCadUpdateAllTests` (16), y el archivo se restauró de sus bytes y se comprobó por su huella. Al final, `git diff` no tiene
restos.

| Rotura | Lo que falla |
|---|---|
| R6. B-1: la regla pura da por escrito un rack a medias | Core: `ARackHalfWrittenIsNotWrittenInsideTheSet` |
| R7. I-2: se confirma aunque falten bloques por importar | Core: `BlocksTheDrawingLacksAreImportedOutsideTheTransactionAndTheSetIsWrittenAgain` y `AnImportThatFailsOrBlocksStillLackingAfterItWriteNothingAndNameTheDraftsThatNeedThem` |
| R8. Menor 1: «Se desconoce» no impide el siguiente | UI: `AConfirmationThatFailsLeavesEveryDraftUnknownAndToBeReadAgain` |
| R9. Menor 2: el aviso sale sin el clic | UI: `ADraftWhoseActualizarIsBlockedPreventsItWithItsCauseAndNothingGoes` |
| R4, repetida (menor 8): un borrador de «Nuevo» entra | Core: `ModifiedDraftsGoCleanOnesAndNewOnesDoNotAndVariablesGoFirstThenTheSystemsOfTheListThenTheName`. UI, ahora también: `CleanDraftsNewDraftsAndDraftsOfAnotherDrawingDoNotGo` |

#### Los números

Por orden de la sesión (pausa del Owner, para no saturar la máquina), **no se corrieron las suites completas**: las correrá la sesión sobre
la rama integrada. Las completas sobre `c4985a2e` (antes de estas correcciones) dieron UI 3126 / 0 / 48 y Core Full 19496 / 0 / 403.

- **Focales de Core, sobre el árbol final: 657 correctas, 0 con error.**
  - Las clases: `UpdateAllRunTests` (17) y las guardas de los archivos tocados del Plugin y de los censos.
    - Caller-owned: `CallerOwned*`.
    - Biblioteca de bloques: `SharedViewFoundationF1*`, `I59F3*`.
    - Propiedades: `CustomPropertiesEdgeGuardTests`, `CustomPropertiesProjectTests`, `CustomPropertiesCommandGuardTests`.
    - Push Back: `PushBackViewEraseGuardTests`, `PushBackEditAllNameWritersGuardTests`, `PushBackClassicCharacterizationPluginGuardTests`,
      `PushBackRoundTripSourceGuardTests`, `PushBackPluginSourceGuardTests`, `PushBackDraftEditorTests`,
      `PushBackWindowDrawingMirrorGuardTests`.
    - Dibujo y redibujo: `RackDefinitionCreatorGuardTests`, `ProjectVariableMutationExecutorTests`, `SelectiveDuplicationFailClosedTests`,
      `DrawServicePlanBaselineTests`, `I55G15Auth15ConsumerTests`.
    - Cantilever: `CantileverPluginSourceGuardTests`, `CantileverDraftEditorTests`.
    - Cama y cabecera: `I61CamaEditWiringGuardTests`, `CabeceraDoorTests`, `CamaDoorTests`.
    - Censos y ventana: `NamespaceFolderGuardTests`, `RackCadWindowListTests`, `SelectiveEditorOpenTests`.
- **Focales de UI, sobre el árbol final:** `RackCadUpdateAllTests` (16), `RackCadVariablesPageTests` y `RackCadWindowTests`: 74
  correctas, 0 con error.
- **Lo que suman a las completas:** Core, +4 (13 a 17 en `UpdateAllRunTests`); UI, +0 (las pruebas de los menores amplían las 16
  existentes). Esperado tras integrar: Core 19500 / 0 / 403 y UI 3126 / 0 / 48, sobre la base de esta parte.
- **Solución en Debug (Plugin incluido), sobre el árbol final:** 0 errores, 2 advertencias, las MSB3277 de siempre. Con `--no-incremental`,
  antes de la pausa y sobre el mismo código, también 0 errores.

## Sexta tanda, ui: la lista que no decía por qué, los detalles del smoke de la quinta tanda y los añadidos (2026-11-27)

- **Orden del Owner (2026-11-27):** «Arregla los defectos…». Encargo de la sesión: la parte «ui» del `BRIEF.md` de la sexta tanda (puntos 1 a
  4) y los añadidos del coordinador (puntos 5 a 9) y del Owner (punto 10).
- **Base:** `f02f519b`. Rama local `architecture/workspace-persistente-rackcad-t6-ui`, sin commit (lo hace la sesión).
- **Cada defecto:** primero la prueba, roja sobre la base; luego el arreglo más estrecho; al final, la comprobación en rojo con todos los
  arreglos revertidos (abajo). Los cambios en archivos compartidos van marcados `T6 ui`.

### 1. La lista que no decía por qué no se releyó

- **Causa:** `RackListViewModel.Load` solo cambiaba el texto ante un `Busy` cuando no había lista o el host decía «puede no estar al día», y
  ante un `Rejected` solo cuando no había lista. Con una lista ya leída, un `Busy` (orden en curso) o un `Rejected` (la petición no es del
  dibujo activo, o no hay dibujo activo) a «Volver a leer la lista» dejaba el texto en «La lista puede no estar al día…».
- **Arreglo:** solo en la relectura pedida por el usuario (`userRequested`), el texto dice el motivo; una presentación sigue sin cambiarlo.
  - `Busy`: el texto que la lista ya decía cuando la propia ventana ve la orden en curso, «El dibujo no se puede leer ahora (hay un comando
    en curso); vuelve a intentarlo al terminar.» (ahora una constante para los dos caminos).
  - `Rejected`: «La lista no se volvió a leer: el dibujo de la ventana no es el dibujo activo de AutoCAD (cambió o se cerró). Vuelve a
    intentarlo con el dibujo a la vista.».
  - Un puerto que no ofrece la lista (`NotOffered`): «La lista de racks no está disponible.».
  - La lista que había se conserva en los tres casos.
- **Pruebas (`RackCadWindowTests`, 3 nuevas):** la lista leída y «puede no estar al día», y la relectura con `Busy`, `Rejected`, `NotOffered`
  y luego `Ready`; un `Busy` a la relectura de un dibujo sin lista ya no dice «Leyendo la lista del dibujo…»; una presentación con `Busy` o
  `Rejected` sigue sin cambiar el texto (guarda).
- **Supuesto T6ui-S1:** los tres textos de arriba.

### 2. O-2: la lista vacía de la ménsula parecía deshabilitada

- **Causa:** el tema de WPF (Aero2) dibuja un `ComboBox` no editable con el mismo degradado gris esté o no habilitado, e ignora su
  `Background`. Vacía, sin texto, la lista de la ménsula se veía como una deshabilitada. Las otras listas de la página tienen siempre un valor.
- **Arreglo (`LargueroWorkbenchView.xaml`):** mientras no hay ménsula elegida y el catálogo trae alguna, la lista muestra «Elige la ménsula»
  dentro de la caja, con el color del texto (`ShellTextPrimaryBrush`) y sin capturar el clic (`IsHitTestVisible="False"`). Al elegir una,
  desaparece. La caja sigue siendo la del tema, como las demás listas de la ventana.
- **Prueba (`RackCadLargueroWorkbenchTests`, WPF real en STA):** con el larguero de RackCad no hay texto; con una sección AISC la lista está
  habilitada, vacía y con «Elige la ménsula» dentro de su caja, con el color del texto y sin capturar el clic; elegida, el texto se va.
- **Supuesto T6ui-S2:** «Elige la ménsula» dentro de la lista; el gris del tema no se cambia (haría falta una plantilla propia de ComboBox).

### 3. O-3: «Punto de inserción de la cabecera» al insertar otra cosa

- **Causa:** el jig compartido (`BlockPlacement.HeaderInsertionJig`) pregunta «Punto de insercion de la cabecera: » cuando quien lo llama no
  nombra nada. RACKCAMANUEVO, RACKPUSHBACKNUEVO y RACKSELECTIVOINSERTAR llegan a él por caminos que no nombran nada y que comparten con el
  clásico (el despacho de «Nuevo», `DispatchInsertion`, y la mitad «Insertar» de RACKEDITAR, `InsertSelectiveViews`).
- **Arreglo:** `RackJigPrompt` (nuevo, `src/RackCad.Application/Views/Placement/RackJigPrompt.cs`): el texto que nombra quien llama manda
  (la vista Cantilever nombra el suyo); si no, el del ámbito que abrió la orden; si no, el de la cabecera de siempre. Cada una de las tres
  órdenes abre su ámbito alrededor de su inserción (`using (RackJigPrompt.Scope(…))`), y el jig lo resuelve con `RackJigPrompt.Resolve`. El
  ámbito es del hilo de la orden y al cerrarse devuelve el que había. Ninguna otra orden abre uno: RACKCABECERA y las demás dicen lo mismo.
  - RACKCAMANUEVO: «Punto de insercion de la cama: ».
  - RACKPUSHBACKNUEVO: «Punto de insercion de la vista Push Back: » (una por vista, como el Cantilever).
  - RACKSELECTIVOINSERTAR: «Punto de insercion de la vista Selectivo: ».
  - Sin tilde, como todos los avisos de la línea de órdenes del Plugin («Punto de insercion de la vista Cantilever: »).
- **Pruebas (`RackJigPromptTests`, Core, 10):** el texto fuera de un ámbito, el de dentro y el nombrado; los ámbitos anidados y cerrados
  dos veces; el ámbito de otro hilo; los tres textos; y, como texto (Core no puede cargar el Plugin), que cada orden abre su ámbito con su
  texto alrededor de su única inserción, que el jig resuelve con `RackJigPrompt.Resolve` sin el literal, y el censo: solo esas tres órdenes
  abren un ámbito.
- **Supuesto T6ui-S3:** los tres textos, y que solo cambian esas tres órdenes (ver preguntas).

### 4. O-4 (Cantilever)

- **a) «Ir a «Línea» › «Niveles»»:**
  - **Causa:** `GoTo` cambia de pestaña y abre el grupo (`LevelsOpen`), pero el grupo está al final de «Línea» y nadie lo traía a la vista.
  - **Arreglo (`CantileverEditorView.xaml.cs`):** tras el «Ir a» de los niveles, la vista hace el layout de las pestañas (el contenido de una
    pestaña entra al árbol al elegirla) y llama a `LevelsExpander.BringIntoView()`.
  - **Prueba (`RackCadCantileverWindowTests`, WPF real):** una línea nueva a 1290 × 860, «Ir a» del claro: la pestaña «Línea», el grupo
    abierto y su borde superior dentro de la vista de su `ScrollViewer`. En la base empezaba en 666 px con una vista de 488 px.
  - **Supuesto T6ui-S4:** el grupo se trae a la vista (queda arriba si no cabe entero).
- **b) Los nombres internos del aviso de la placa:**
  - **Causa:** el lado se concatenaba como el enum («PositiveY») en `CantileverStationLevelLayoutResolver` (el prefijo «Nivel 1, lado …»)
    y en `CantileverStationResolver`; el aviso de `CantileverArmConnectionMetricsResolver` (I-37C) nombraba la propiedad
    («VerticalPunchCount») y sus mensajes iban sin tildes.
  - **Arreglo:** `CantileverDiagnosticText.Side` (en `CantileverDiagnostic.cs`): «+Y» / «−Y», como la matriz de «Brazos» y el editor
    clásico. Todos los mensajes del resolver de la placa con los nombres de la ventana y sus tildes: «Aumenta «Troqueles verticales»: más
    troqueles…», «placa de montaje», «Margen vertical», «paso» (por «pitch»), «retícula», «índice», «número», «recibió»… El lado, con
    «+Y» / «−Y», en los 5 mensajes del resolver de niveles y los 3 del de estación que lo nombran (con las tildes de esas líneas).
  - **Pruebas:** `CantileverStationCorrectionTests`: cada conexión inválida de `InvalidConnections`, en sencilla y en doble, sin nombres
    internos ni palabras sin tilde (11); y el aviso de la placa completo, con «lado +Y» y, en doble, «lado −Y». `CantileverArmTests` fijaba
    «VerticalPunchCount»: ahora fija «Aumenta «Troqueles verticales»: más troqueles» y que no aparece la propiedad.
  - **Supuesto T6ui-S5:** los textos nuevos; los ve también el editor clásico de Cantilever, que usa el mismo dominio.

### 5. UT-S11 y 10: lo que dice «Descartar» de un borrador

- **Causa:** los avisos de un Selectivo nuevo (`SelectiveLayoutRestoreResult.NewRackDiscardText`, en «Restaurar layout…», «Aplicar a todos» y
  al reducir frentes o fondos) decían ««Descartar» vuelve a los valores por defecto.», también en un borrador abierto de la biblioteca o en
  una copia, que vuelven a su diseño o a la base de su original. En Cantilever, la línea bajo el nombre de una línea nueva decía «Valores
  por defecto del editor clásico.» también con un diseño de la biblioteca. Dinámico, Push Back y Cama no tenían ese texto. El mensaje tras
  «Descartar» de un borrador era «Borrador descartado.» en Selectivo, Dinámico, Push Back y Cama, y «Cambios descartados: la línea nueva
  vuelve a como se abrió.» en Cantilever.
- **Arreglo:**
  - **Punto 10 (decisión del Owner):** `RackCadDraftNaming.DiscardedText` = «Cambios descartados: el borrador vuelve a como se abrió.», el
    mismo en los cinco editores (Selectivo, Dinámico, Push Back, Cama y Cantilever). Los racks del dibujo siguen con su texto.
  - **UT-S11:** `NewRackDiscardText` = ««Descartar» vuelve a como se abrió el borrador.» (con las palabras del Owner). En Cantilever, una
    línea nueva dice «Sin cambios desde que se abrió.» o «Con cambios desde que se abrió.».
- **Pruebas:** `RackCadNewDraftsTests` (2: el mensaje en Selectivo, Dinámico y un Selectivo de la biblioteca, que vuelve a su diseño; el
  aviso de «Restaurar layout…» de un borrador de la biblioteca); una en `RackCadPushBackEditorTests`, `RackCadCamaWindowTests` y
  `RackCadCantileverWindowTests` (con la línea bajo el nombre de una línea de la biblioteca). Actualizadas, porque fijaban los textos viejos:
  `SelectiveRestorePanelTests` y `RackCadSelectiveInsertTests` (una línea cada una).
- **Supuestos:** T6ui-S6 (los textos de UT-S11 y de la línea nueva de Cantilever) y T6ui-S7 (el del Owner, en los cinco sistemas).

### 6. «Distinta del resto» marcaba la mitad de las celdas

- **Causa (`SelectiveEditorViewModel`, la matriz):** el contenido «usual» era el primero de los más comunes también cuando otro era igual de
  común. Con dos tarimas a partes iguales, una mitad salía «distinta» de un «resto» que era la otra mitad; con tres a tercios, dos tercios.
- **Arreglo:** el contenido usual es el que tienen MÁS de la mitad de las celdas de la vista (lo que dice el comentario: «el que tienen la
  mayoría de las celdas»). Sin él, ninguna celda se marca; la leyenda sigue. Con 3 celdas de 40 × 60 y 2 de 44 × 55 (la prueba de siempre),
  las 2 siguen distintas.
- **Prueba (`RackCadSelectiveFidelityTests`, teoría de 2):** dos y tres tarimas en partes iguales, ninguna celda distinta ni «distinta del
  resto» en su nombre accesible.
- **Supuesto T6ui-S8:** la mayoría estricta.

### 7. «Largo de larguero (n)» cuenta el nivel de piso: no es un defecto

- `SelectiveGeometryResolver.BayBeamLength` (`src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:369-386`) recorre todos
  los niveles del frente, el del piso sin larguero incluido: el largo de un frente es el mayor de sus niveles, manual o automático, y todos
  sus largueros comparten ese largo (la separación de postes). Un largo manual en el nivel de piso cambia el dibujo aunque ese nivel no
  lleve larguero, así que «Restaurar layout…» lo cuenta y lo quita bien (`SelectiveLayoutRestore.cs:201` y `:257`). No se cambió.

### 8. Avisos que nombraban «Descartar» con el botón apagado

- **Causa:** los avisos de Selectivo añadían siempre la frase de «Descartar», también cuando la operación dejaba el borrador como el dibujo
  («Limpio», «Descartar» apagado): «Restaurar layout…» (`NoticeFor`), «Aplicar a todos» (`SelectivePeralteForAllPosts.NoticeFor`) y lo que se
  pierde al reducir frentes o fondos (`LossText`, que además se calculaba antes de saber si el borrador seguía modificado). Lo mismo en
  «Restaurar layout…» de Cantilever (««Descartar» lo deshace.»).
- **Arreglo:** la frase solo cuando «Descartar» está activo: `NoticeFor(newRack, canDiscard = true)` en los dos avisos de Application (el
  valor por omisión conserva la conducta para el resto de llamadores), el editor les pasa `CanDiscard`; `LossText` se calcula tras
  `UpdateDirty`; Cantilever, con su `CanDiscard`.
- **Pruebas:** `SelectiveRestorePanelTests` (una nueva: restaurar, «Aplicar a todos» y menos frentes, con el borrador de vuelta al dibujo) y
  `RackCadCantileverWindowTests` (una nueva). Dos pruebas existentes de `SelectiveRestorePanelTests` restauraban hasta dejar el borrador limpio
  y esperaban la frase: ahora hacen además otro cambio (una altura de tarima), con el que «Descartar» sigue activo y la frase se dice.
- **Supuesto T6ui-S9.**
- **Queda (pregunta):** el mismo patrón en la seguridad sin diálogo, «Se quitó «…» del borrador, con sus medidas. «Descartar» lo recupera.»
  (`RackCadSafetyPanel.cs:827`), cuando lo quitado solo estaba en el borrador. Arreglarlo pide tocar los tres anfitriones del panel, uno de
  ellos de Push Back (la parte pb): no se tocó.

### 9. «Más» pasaba a una segunda línea a unos 950 px

- **Causa:** la cabecera de cada editor era un solo `WrapPanel` con el nombre, el estado y los botones: cuando no cabían, el último o los dos
  últimos botones («Más», «Cerrar el editor») pasaban solos a la línea siguiente. En las pruebas, la barra de Selectivo pide unos 905 px; en
  la paleta, el editor mide 946 px con la paleta a 1200 px, 846 a 1100 y 696 a 950.
- **Arreglo (los cinco editores: Selectivo, Dinámico, Push Back, Cama y Cantilever):** los botones van en un grupo (`WrapPanel
  x:Name="HeadButtons"`) dentro de la cabecera: junto al nombre mientras caben, como antes, o todos juntos en la línea siguiente. Solo se
  añadieron las dos etiquetas; las líneas de dentro no se reindentaron, para que la mezcla con otras partes sea limpia.
- **Prueba (`RackCadEditorHeadTests`, nueva, WPF real; 5 editores × 4 anchos):** a 1036, 950, 846 y 696 px, todos los botones visibles en
  una línea, y a 946 px o más, en la del nombre. En la base fallaban 9 de 20 (todas a 846 o 696 px).
- **Supuesto T6ui-S10.**

### La comprobación en rojo

- **Cómo, de una vez para todos los puntos:**
  - una copia de las 27 fuentes cambiadas, fuera del worktree;
  - `git checkout` de esas fuentes a la base, con las pruebas y el tipo nuevo `RackJigPrompt` en su sitio;
  - las pruebas de esta parte;
  - las copias de vuelta, con `git diff` idéntico al de antes (comprobado con `cmp`) y sin restos (`if (false)`, `true ||`).
- **UI:** fallan 24 de 36. Son todas las de los puntos 1, 2, 4 a, 5, 6, 8, 9 y 10, y las dos actualizadas que fijan el texto del Owner. Pasan
  las 12 que también deben pasar sobre la base: la guarda de la presentación (punto 1) y los anchos en que la cabecera ya cabía (950 y 1036 px
  en los cinco editores, y 846 px en Cama).
- **Core:** fallan 18 de 23: las 5 guardas de O-3, las 11 conexiones inválidas y el aviso de la placa (O-4 b), y `CantileverArmTests`. Pasan
  las 5 pruebas puras de `RackJigPrompt`, que no dependen del arreglo.
- Además, cada prueba nueva se vio en rojo sobre la base antes de escribir su arreglo.

### Números

Medidos sobre las fuentes restauradas, con la solución recompilada en Debug con `--no-incremental`.

- **UI completa:** 3144 correctas, 0 con error y 48 omitidas (3192). Sobre la base, 3110 + 34 nuevas:
  - 3 en `RackCadWindowTests`;
  - 1 en `RackCadLargueroWorkbenchTests`;
  - 3 en `RackCadCantileverWindowTests`;
  - 2 en `RackCadNewDraftsTests`;
  - 1 en `RackCadPushBackEditorTests` y 1 en `RackCadCamaWindowTests`;
  - 2 en `RackCadSelectiveFidelityTests`;
  - 1 en `SelectiveRestorePanelTests`;
  - 20 en `RackCadEditorHeadTests`.
- **Core Full:** 19505 correctas, 0 con error y 403 omitidas (19908). Sobre la base, 19483 + 22 nuevas: 10 en `RackJigPromptTests` y 12 en
  `CantileverStationCorrectionTests`.
- **Solución en Debug (Plugin incluido):** 0 errores y 84 advertencias, las de siempre (MSB3277 de las referencias de AutoCAD y analizadores
  de xunit).
- **Archivos:** sin caracteres de control y con CRLF. El `sed -i` de Git Bash los había pasado a LF; se devolvieron a CRLF con `unix2dos`.

### Preguntas para el Owner

- **O-3, las demás órdenes que todavía dicen «de la cabecera»:** RACKSELECTIVONUEVO, RACKPUSHBACKINSERTAR, RACKDINAMICONUEVO y
  RACKDINAMICOINSERTAR (la ventana), y el clásico RACKNUEVO de Cama, Push Back, Selectivo y Dinámico y RACKEDITAR «Insertar». El encargo pedía no
  cambiar las demás. Si se quiere, cada orden de la ventana es una línea (`using (RackJigPrompt.Scope(…))`); para el clásico habría que decidir
  si el texto lo nombra el sistema (en `RackViewPlacement`), lo que cambiaría también RACKEDITAR.
- **8, la seguridad sin diálogo:** ver «Queda» del punto 8.
- **5, el Cantilever nuevo:** «Sin cambios desde que se abrió.» pierde la información «valores por defecto del editor clásico» en una línea
  nueva desde «Nuevo». Si se quiere, la ventana puede decir de dónde viene el borrador (sabe si es de la biblioteca o una copia).

### Sexta tanda, ui: correcciones de la revisión (sobre `331f9f18`)

Vienen de la revisión independiente de la parte ui y de la respuesta del Owner sobre O-3. Sin commit (lo hace la sesión).

#### O-3, respuesta del Owner: «Todas», en la ventana y en el clásico

- **Arreglo:** cada orden que inserta otra cosa que una cabecera abre el ámbito de lo que inserta (`using var jigPrompt =
  RackJigPrompt.Scope(…)`), y RACKCABECERA sigue con «de la cabecera».
  - **En el despacho de «Nuevo» (`RackMenuCommands.DispatchInsertion`):** `RackJigPrompt.ForSystem(request.Kind)`. Cubre RACKNUEVO, el menú,
    RACKBIBLIOTECA y todas las «…NUEVO» de la ventana (RACKSELECTIVONUEVO, RACKDINAMICONUEVO, RACKPUSHBACKNUEVO y RACKCAMANUEVO). Las dos
    puertas que tenían su propio ámbito (cama y Push Back) ya no lo abren. Una cabecera (`RackSystemKind.Selective`) y Cantilever, que nombra
    el suyo, no cambian.
  - **En RACKEDITAR:** `RackJigPrompt.ForEnvelopeKind(embed.Kind)` alrededor del editor del rack, que cubre su mitad «Insertar».
  - **En la ventana:** RACKSELECTIVOINSERTAR, RACKDINAMICOINSERTAR y RACKPUSHBACKINSERTAR.
  - **En el clásico directo:** RACKSELECTIVO, RACKPUSHBACK, RACKSISTEMADINAMICO y QUICKCAMA.
  - **Textos:** «Punto de insercion de la cama: », «…de la vista Selectivo: », «…de la vista Push Back: » y «…de la vista Dinamico: ».
  - Un ámbito sin texto (el de una cabecera) conserva el que había abierto.
- **Pruebas (`RackJigPromptTests`):**
  - la guarda que fija el conjunto completo de archivos que abren un ámbito, y que RACKCABECERA no lo nombra;
  - una teoría de 9 órdenes que abren su ámbito antes de su trabajo;
  - las tablas `ForSystem` (7) y `ForEnvelopeKind` (8);
  - el ámbito sin texto.
- **Supuesto T6ui-S3 (revisado):** los cuatro textos, sin tildes como todos los avisos del jig («insercion», «Dinamico»), y las órdenes de
  arriba.

#### 1. «Descartar» de una copia («Duplicar»)

- **Causa:** la copia de un borrador modificado nace «Modificado» sobre la base de su original, y su «Descartar» vuelve a esa base. Los
  textos decían «el borrador vuelve a como se abrió», «vuelve a como se abrió el borrador» y, en Cantilever, «Con cambios desde que se
  abrió.» en una copia recién abierta sin tocar.
- **Arreglo:**
  - Cada editor sabe que es una copia porque solo una copia toma un diseño como cambio (`TakeDesignAsEdit`).
  - El texto del Owner se queda para un borrador.
  - Una copia dice «Cambios descartados: la copia vuelve a como se abrió su original.» (`RackCadDraftNaming.CopyDiscardedText`), en los
    cinco editores.
  - Los avisos de Selectivo de una copia dicen ««Descartar» vuelve a como se abrió el original de la copia.»
    (`SelectiveLayoutRestoreResult.CopyDiscardText`, `DiscardTextOf`).
  - En Cantilever, una copia dice «Sin cambios respecto de cómo se abrió su original.» o «Con cambios respecto de cómo se abrió su
    original.».
- **Pruebas:** `RackCadNewDraftsTests` (una nueva, junto a `TheCopyOfADraftIsModifiedExactlyWhenItsOriginalWas`: Selectivo con su aviso
  de «Restaurar layout…», el original con el texto del Owner, y Dinámico); `RackCadCantileverWindowTests` (una nueva: la copia limpia y la
  modificada); `RackCadPushBackEditorTests` y `RackCadCamaWindowTests` (la copia, en la prueba del punto 10).
- **Supuesto T6ui-S11:** los textos de la copia.

#### 2. «Más» abierto saltaba de línea el grupo

- **Causa:** las entradas de «Más» (`MorePanel`) estaban dentro del grupo `HeadButtons`; abiertas, el grupo crecía y se iba entero a la
  línea siguiente.
- **Arreglo (los cinco editores):** `MorePanel` es una fila propia bajo la cabecera, alineada a la izquierda, como `CloseConfirmPanel`.
- **Prueba (`RackCadEditorHeadTests`):** cada editor a 1036, 950, 846 y 696 px, también como borrador de «Nuevo» (con «Insertar en el
  dibujo…») y con «Más» abierto. Los botones no se mueven al abrirlo, y sus entradas quedan debajo.
- **Supuesto T6ui-S12:** las entradas de «Más» ya no se abren debajo del botón, sino en una fila bajo la cabecera (cambia UT-S01).

#### 3. La prueba del orden `Rebuild` / `UpdateDirty`

- `SelectiveRestorePanelTests`: la aserción `Assert.Equal(vm.CanDiscard, …)` pasaba también con el orden viejo. Ahora fija `CanDiscard`
  falso, «Limpio» y el mensaje exacto («…Volver a subirlos no los recupera.»).

#### 4. «Distinta del resto» en «Todos los fondos»

- **Causa:** la mayoría se contaba sobre las celdas de cada fondo (fondo, frente, nivel), pero la matriz marca su celda si difiere cualquier
  fondo. Con el fondo 1 de 4 × 2 celdas con A y el fondo 2 de 3 × 2 con B, A era la mayoría (8 de 14) y se marcaban 6 de 8.
- **Arreglo (`SelectiveEditorViewModel`):** la mayoría se cuenta sobre las celdas de la matriz. Cada una cuenta con lo que muestra: los
  contenidos distintos de sus fondos. En ese caso, las 6 celdas con A y B son el resto, y solo F4 (A sola) es distinta.
- **Pruebas (`RackCadSelectiveFidelityTests`):** ese caso (solo F4, 2 celdas), y 3/2/2 sin mayoría, sin marcas.
- **Supuesto T6ui-S8 (revisado):** la mayoría estricta, sobre las celdas de la matriz; con 3/2/2 no se marca ninguna.

#### 5. Cantilever, lo que quedaba

- **Arreglo:**
  - **`CantileverStationResolver`:** los detalles del pase final, con tildes («patrón de conexión», «índice», «usó», «elevación»). La
    comparación dice «… es X in en el pase final y el layout predijo Y in»: «resuelta» no concordaba con «el borde…» ni con «el claro
    libre». También con tildes el resto de sus mensajes («estación», «configuración», «número», «recibió», «mínima», «colocación»…).
  - **`CantileverStationLevelLayoutResolver`:** los mensajes que quedaban, con tildes, y «paso» por «pitch».
  - **`CantileverArmResolver`:** «placa de montaje» por «placa de conexion», y «placa de extremo» por «placa final», como la ventana. «Altura
    extra de tope», con tildes. «El valor del espesor…», donde decía «El valor de el espesor…».
  - **`CantileverDraftEditor.SideLabel`:** delega en `CantileverDiagnosticText.Side`, un solo sitio.
- **Pruebas:**
  - `CantileverStationCorrectionTests`: los literales de los resolvers de estación, niveles y placa, leídos como texto, sin palabras sin tilde
    ni nombres internos (3).
  - `CantileverArmTests`: los mensajes de las placas (1).

#### 6. La cabecera del editor de Cabecera

- **Arreglo (`CabeceraEditorView.xaml`):** los botones de la cabecera suelta (`StandaloneHeadButtons`) y los de la cabecera de un poste
  (`EmbeddedHeadButtons`) son un grupo, como en los otros cinco editores.
- **Prueba (`RackCadEditorHeadTests`):** la cabecera suelta a 1036, 950, 846 y 696 px, con sus botones en una línea.

#### Fuera de esta parte

- La lista que avisa tras un ZOOM es de la parte st. No se tocó el drenaje.

#### La comprobación en rojo

- **Cómo:**
  - una copia, fuera del worktree, de las fuentes cambiadas desde `331f9f18`;
  - esas fuentes devueltas a `331f9f18`, menos `RackJigPrompt.cs`, el tipo que usan las pruebas;
  - además, `LossText` puesto otra vez antes de `UpdateDirty`, para probar la corrección 3 por sí sola;
  - las pruebas de las correcciones;
  - las copias de vuelta, con `git diff` idéntico al de antes (`cmp`) y sin restos.
- **UI:** fallan 22 de 51:
  - la copia en Selectivo, Dinámico, Cantilever, Push Back y Cama (5);
  - la prueba de la corrección 3, con el orden viejo (1);
  - «Distinta del resto» en «Todos los fondos» (1);
  - la cabecera con «Más» abierto en los cinco editores a 1036, 950 y 696 px (15);
  - la cabecera de Cabecera a 696 px (1).
- **Pasan las que también deben pasar sobre `331f9f18`:** las cabeceras con «Más» cerrado, la de «Más» abierto a 846 px (el grupo ya iba
  entero a la segunda línea), la de Cabecera a 1036, 950 y 846 px, 3/2/2 sin mayoría y el texto del Owner para un borrador.
- **Core:** fallan 14 de 37:
  - las 9 órdenes y el censo de O-3;
  - los literales de los resolvers de estación y de niveles (el de la placa ya estaba corregido en `331f9f18`);
  - los 2 de `CantileverArmTests`.
  - Pasan las pruebas puras de `RackJigPrompt`, con el tipo en su sitio.

#### Números

Por orden de la sesión (hay otros agentes en la máquina), aquí no se corrieron las suites completas: las corre la sesión sobre la rama
integrada.

- **Solución en Debug, Plugin incluido:** 0 errores.
- **UI, focales:** 420 correctas, 0 con error. Son 15 clases:
  - `RackCadEditorHeadTests`, `RackCadNewDraftsTests`, `RackCadCantileverWindowTests`, `RackCadPushBackEditorTests`, `RackCadCamaWindowTests`;
  - `SelectiveRestorePanelTests`, `RackCadSelectiveFidelityTests`, `CabeceraEditorTests`, `RackCadUtilitiesTests`, `PostPeralteForAllPostsTests`;
  - `RackCadSelectiveInsertTests`, `RackCadWindowTests`, `RackCadLargueroWorkbenchTests`, `RackCadDynamicEditorTests`, `RackCadSelectiveWindowTests`.
- **Core, focales:** 1581 correctas, 0 con error y 4 omitidas (1585):
  - todo Cantilever, `RackJigPromptTests`, `SelectivePeralteForAllPostsTests` y `SelectiveLayoutRestore*`;
  - las guardas que leen los archivos del Plugin tocados (`RackCadWindowListTests`, `RackViewPlacementGuardTests`, `G9bInsertWiringGuardTests`,
    `InsertRedrawLayerAccessCharacterizationTests`, `PushBackWindowDrawingMirrorGuardTests`, `I60RackNameWiringGuardTests` y las demás de la
    lista).
- **Pruebas nuevas de estas correcciones:**
  - UI, 28: 24 en `RackCadEditorHeadTests` (ahora 44), 1 en `RackCadNewDraftsTests`, 1 en `RackCadCantileverWindowTests` y 2 en
    `RackCadSelectiveFidelityTests`.
  - Core, 26: 22 en `RackJigPromptTests` (ahora 32), 3 en `CantileverStationCorrectionTests` y 1 en `CantileverArmTests`.
  - Sobre 3144 / 19505 de `331f9f18`, la UI completa debería dar 3172 y Core Full 19531, sin contar lo que traigan otras partes.
- **Archivos:** sin caracteres de control y con CRLF (los tres que la herramienta escribió en LF, devueltos con `unix2dos`).

## Sexta tanda: decisiones del Owner (2026-11-27)

**Órdenes en el chat (literales):**
- «Arregla los defectos, aún no hay que borrar el editor clásico, probablemente mejor lo dejaremos por ahora a menos que elimine
  dificultades de escalabilidad en un futuro, hay que arreglar lo de pushback y hazme las preguntas requeridas».
- «También hay que continuar con el actualizar todo, basicamente dejsr terminada I-64».

**Consecuencias:**
- **El editor clásico se QUEDA.** Esto deja sin efecto la parte de SD-02 que decía «el clásico se elimina al final de I-64». Sus
  defectos se corrigen en el propio clásico.
- **Nada va a `main` como hotfix:** los arreglos llegan con I-64.

**Respuestas (AskUserQuestion), resumidas; la opción elegida va entre comillas:**

| Tema | Respuesta |
|---|---|
| El editor de la ventana tras un «U» (O-5, M-16) | «Avisar y ofrecer releer» (parte st) |
| Los diálogos clásicos de seguridad que descartaban sin avisar | «Guardarla como la ventana» (parte seg) |
| Hotfix en `main` | «No, que lleguen con I-64» |
| F4B (Push Back completo con la ventana MODAL) | «Cerrarla como reemplazada» por Push Back en la ventana; se avisa antes de liberar su material |
| RLY y RR | «La página «Almacén»» (parte rl) |
| RACKVARIABLES y RACKPROPIEDADES | «La ventana, ya» (parte rl) |
| La paleta recuerda su tamaño; cambiar las guardas de F1-T3B | «Sí, cambiarlas» (parte rl) |
| Validación para llevar I-64 a `main` | «Prueba guiada al final»: guía en el chat, DLL del SHA final |
| «Propiedades» en un borrador nuevo (VA-S9) | «Sí, y se escriben al insertarlo» (parte pr); el dibujo sin propiedades: «Insertar y avisar»; «Duplicar» las copia |
| Mejoras de borradores | Solo el texto de «Descartar» (parte ui) |
| «Puede no estar al día» tras cualquier orden | «Afinarlo»: solo órdenes que cambian el dibujo, en los editores, el BOM, «Almacén» y la lista (parte st) |
| «Variables» y los editores limpios que redibuja | «Sí, marcarlo», en lugar de cerrarlos (parte st) |
| La paleta oculta | «Sí, al volver a mostrarla» (parte st) |
| El aviso «de la cabecera» en las demás órdenes | «Todas», en la ventana y en el clásico (parte ui) |
| La parrilla con todo apagado en el clásico | «Sí, como la ventana»; el clásico gana «Quitar», porque no tenía cómo quitarla (parte seg) |
| El contador «Elementos de seguridad (n)…» | «Solo lo que dibuja» (parte seg) |
| «Actualizar» con vistas que no se redibujan | «Sí, que diga qué falló» (parte b1) |
| Medio frente que deja de caber en el editor CLÁSICO | «Dejarlo así»; avisar al abrir un dibujo que ya lo trae: «Sí» (parte bom) |
| Selectivo conserva los campos desconocidos de la biblioteca; «Duplicar» hereda el archivo | «Dejarlo así»; «No, la copia es nueva» (parte lib) |
| «Actualizar todo» | «Aceptar las anidadas»; las propiedades que fallan: «Falla todo el conjunto»; «Incluir Variables»; «Basta «vuelve a leerlo»» (parte at) |
| **Largueros** | **«Hay que dejarlo Wip, entra en otra iniciativa»**: la parte lg (`e05c21cf`) NO se integra y queda en su rama local para esa iniciativa; la página sigue como el borrador WIP de la quinta tanda |

**Aprobados tal como están:**
- T4S-13 (el desviador de Dinámico en racks existentes) y T4S-14 (la columna «Auto» del clásico de Dinámico);
- CB-13, solo ocultar la cara de montaje; CB4-02, basta el claro por panel;
- RCT sigue en el clásico (CT5-08); «Abrir en editor clásico» no se añade en todos (UT-S12);
- BM-S4 y BM-S5;
- la ventana más estricta que el clásico (PB-S09, CT5-03 e IN-11);
- PB-S10 y PB-S17, puntos 1-4 y 6;
- S-11 (la raya de piso) y Q5 (la pestaña «Rack» completa);
- **los ~230 supuestos menores ya registrados en este documento:** «Apruebo todos».

## Sexta tanda: integración en la rama de Selectivo (sesión, 2026-11-27)

Diez partes, cada una con su revisión independiente y sus correcciones. Cada merge es `--no-ff`. lg no se integra (ver arriba).

| Orden | Parte | Qué | Punta de la parte | Merge |
|---|---|---|---|---|
| 1 | b1 | B-1 en Push Back (clásico y ventana) y en Cabecera; I-02 (la cola no coloca si falta un redibujo); el frontal B | `5ff8c008` | `8a8dc159` |
| 2 | pb | PB-S17: el «Auto» de la defensa y los niveles del desviador en el clásico de Push Back | `c79d3869` | `ac01e9bd` |
| 3 | lib | Los campos desconocidos de la biblioteca llegan al dibujo (Dinámico, Push Back, Cantilever) | `bb385dc4` | `4c522ba0` |
| 4 | pr | «Propiedades» en los borradores nuevos, escritas al insertar | `7e16f992` | `6a3ea124` |
| 5 | bom | El clásico de Selectivo ya no cambia las cabeceras personalizadas; medio frente que no cabe | `c6ddb281` | `c7a51269` |
| 6 | rl | RLY/RR abren «Almacén»; RACKVARIABLES y RACKPROPIEDADES abren la ventana; la paleta recuerda | `29341e2b` | `41bc3859` |
| 7 | seg | El «Aceptar» clásico de seguridad guarda como la ventana | `5de0b0cd` | `2a2f58dc` |
| 8 | st | El editor tras una orden ajena y el aviso afinado | `347555ef` | `c03aba1f` |
| 9 | at | «Actualizar todo» | `3cb0072b` | `9655bed3` |
| 10 | ui | La lista dice por qué; detalles del smoke; los avisos de inserción | `bab26f84` | `bbfbb5a5` |

**Lo que se resolvió al juntar:**
- **lib y pr**, en la puerta y en los tres envíos de la ventana: se conservan las dos cosas. La condición de pr (`insertViews == null ||
  editor.IsNewRack`) y `SourceDesignJson` de lib.
- **rl y pr**, en `RackPropertiesTabViewModel`: se conserva el texto de pr (`NotReadText`, «vuelve a leerlo del dibujo»), que ya no remite a
  RACKPROPIEDADES y lo fija `RackCadNewDraftPropertiesTests`. Los demás remedios de rl (T6rl-S15) siguen.
- **seg y bom**, en `RackSelectiveWindow`: el aviso de bom al abrir y el recuento de seg, uno tras otro, después de la recomposición.
- **seg y pb**, el seam `DesviadorDialog`: seg adoptó exactamente el de pb, y queda uno solo.
- **st, pr y Cantilever**, en «Insertar vista…»: las dos condiciones de bloqueo, una tras otra.
- **at y st**, en «Variables»: la función compartida de at (`VariablesWrittenFor`) con el marcado de st.
  - `UpdateAllStaleProbe` se conecta a `IsDrawingChanged` de st.
  - RACKACTUALIZARTODO responde dentro de su orden, así que st la da por propia y no marca los editores.
- **El censo de archivos de `PushBackEditAllNameWritersGuardTests`:** 19 + 1 (rl) + 2 (at) = 22.
- **ui y st**, en la lista: una relectura pedida por el usuario y rechazada dice por qué (ui). Una presentación solo dice «puede no estar
  al día» si el dibujo cambió desde la lectura (st).
- **ui y bom**, en «Restaurar layout…»: el aviso de ui (si «Descartar» está activo, y si es una copia) y el consejo de bom cuando se
  rompería un medio frente.
- **La puerta de prueba de st** implementa los eventos nuevos de los atajos de rl (`e965912a`).
- **Dos pruebas** que no contemplaban la señal afinada de st (en el commit de cierre de la integración):
  - `AReadOfTheUserThatTheHostDoesNotDoSaysWhyAndKeepsTheList` (ui) ahora simula que el dibujo cambió (`DrawingChangeCount = 1`), como las
    pruebas de la lista que st actualizó;
  - `RlyOpensRejillaWithTheSelectedRackAsTheSeedReadInsideItsCommandAndSendsNothing` (rl): el resto de RLY solo lee
    (`changesTheDrawing: false`). Lo había anticipado la revisión de rl.

### Números sobre la integración

- **Solución en Debug, Plugin incluido:** 0 errores.
- **Core Full:** 19638 / 0 / 403.
- **UI completa:** 3367 / 2 / 48, con los dos fallos de arriba.
  - Tras ajustar esas dos pruebas, sus clases pasan enteras (`RackCadWindowTests` y `RackCadWarehousePageTests`: 80 / 0).
  - La UI queda en 3369 / 0 / 48. El CI la corre entera.
- Con el AutoCAD del Owner abierto, la UI completa tardó 30 min, y la Core Full 10.

### Pendiente

- El smoke de la sexta tanda en AutoCAD, por la sesión, con lo que cada parte dejó para él:
  - el escenario B y el frontal B (b1);
  - las propiedades al insertar y al cancelar (pr);
  - el aviso al abrir (bom);
  - la paleta que recuerda y la regla de pantalla (rl);
  - la señal afinada: ZOOM, PAN, QSAVE, presentaciones y matrices (st);
  - «Actualizar todo», con un solo «U», cero escrituras si uno falla, un bloque importado en segundo lugar, el renombrado, las
    definiciones huérfanas y el tiempo con muchos racks (at);
  - los avisos de inserción (ui).
- La prueba guiada del Owner sobre el SHA final, y el cierre formal de I-64:
  - la A-14 al día;
  - F4B cerrada como reemplazada;
  - ADR, HANDOFF y ROADMAP;
  - el merge a `main`.

## Sexta tanda: smoke en AutoCAD (sesión, 2026-11-27, 12:21 a 13:15)

- **Orden del Owner:** «Haz la prueba en autocad». AutoCAD estaba cerrado. La comprobación se hizo en dos tramos, con un arreglo entre ellos.
- **DLL del primer tramo:** Debug de esta rama en `265bee31` (CI verde, run 46047726415), compilado sobre el árbol limpio.
  - SHA-256 (prefijo): Plugin `DF8E6B6A9E47D785`, UI `E571B727DD02D137`, Application `AF55B79C6B3BD20E`, Domain `B8436BCC72FA3434`.
- **DLL del segundo tramo:** Debug en `25ab972c`, el arreglo de la paleta (abajo). Solo cambia el Plugin: `303193222EEC9DB8`. UI,
  Application y Domain son los mismos bytes que en el primer tramo.
- **Carga:** NETLOAD «Load Once» en cada arranque; FILEDIA a 0 solo para la ruta y de vuelta a 1.
- **Dibujo:** una copia de trabajo de `smoke-sel-prep.dwg` (huella `2FA0BC027C615D53`). Se cerró siempre sin guardar. Al final, la copia
  conserva la misma huella y la fecha del 25-nov.
- **Diagnóstico:** se cargó una sola vez un DLL de diagnóstico propio de la sesión (`RcDiag`, fuera del repo). Es de solo lectura: lee por
  reflexión lo que devuelve la paleta y las áreas de trabajo de las pantallas. No escribe en el dibujo ni en la paleta.
- **Dejado como estaba:** la paleta, a 80,80 con unos 600 × 600 (lo que guarda el perfil); FILEDIA en 1; la configuración del usuario, sin
  tocar a mano; ningún diseño guardado en la biblioteca.

### Resultados (OBSERVADO)

| Paso | Parte | Resultado | Observación |
|---|---|---|---|
| S6-1 | at | PASA | SEL-A › «Avanzado» › «Todos los postes»: peralte 5 con «Aplicar a todos», sin «Actualizar». El botón pasó a «Actualizar todo (1)». DIN 1 › «Rack»: peralte 4 → «Actualizar todo (2)». Al pulsarlo: «AutoCAD escribió 2 borradores en una sola operación. Un «U» en AutoCAD lo deshace todo», con SEL-A (Selectivo) y DIN 1 (Dinámico) en «Escrito». |
| S6-2 | at | PASA | Un solo «U» deshizo RACKACTUALIZARTODO. Leído por COM después del «U»: `PostPeralte` 3 en SEL-A y 3 en DIN 1 (los valores de antes). |
| S6-3 | st | PASA | Tras ese «U», el editor abierto de DIN 1 quedó marcado («El dibujo cambió desde que se leyó…») y seguía mostrando 4. En la lista, SEL-A y DIN 1 llevaban la marca roja. «Volver a leer del dibujo» quitó el aviso y DIN 1 volvió a verde. |
| S6-4 | st | PASA | ZOOM › Extensión con el editor de DIN 1 abierto y limpio: no lo marcó. |
| S6-5 | rl | PASA | RACKVARIABLES abrió la página «Variables» dentro de la ventana. |
| S6-6 | rl | PASA | RLY abrió «Almacén» › «Rejilla» dentro de la ventana. |
| S6-7 | pr, ui | PASA | «Nuevo» › «Push Back en esta ventana» › «Propiedades»: «Zona» = «T6». «Insertar en el dibujo…» con solo la frontal: AutoCAD pidió «Punto de insercion de la vista Push Back» (ya no «de la cabecera», O-3 de la quinta tanda). En el sobre de «Push Back 1 - frontal entrada-salida» está la propiedad «Zona». Ver O-1. |
| S6-8 | pr | PASA | Un solo «U» deshizo RACKPUSHBACKNUEVO, con la vista y sus propiedades. Entre la inserción y el «U» hubo una lectura por COM, y esta vez no dejó un paso vacío (ver O-1 de la quinta tanda). |
| S6-9 | b1 | PASA | PB 1 › «Editar aquí» › F1 N1, «Claro sobre la tarima» de 6 a 8, sin «Actualizar» → «Insertar vista…» › «Planta» → «AutoCAD redibujó 1 vista del rack con el borrador e insertó 1 vista nueva» (RACKPUSHBACKINSERTAR, «COMPLETED (1/1)»). Leído por COM: los sobres de «PB 1 - lateral 1» y «PB 1 - planta» llevan el mismo `Design`, idéntico byte a byte (`ClearHeight` 8, 6, 6). Solo cambian `View` y `Section`. El editor quedó en «Limpio». |
| S6-10 | rl | FALLA → arreglado en `25ab972c` | La paleta no recordaba su posición. Se dejó en 60,60 (1290 × 860) y se cerró AutoCAD; el perfil guardó `FloatInfo` 60,60. Al reabrir, volvió en 80,80 con el tamaño correcto. Dentro de una misma sesión, un segundo RACKCAD también la llevaba de 60,60 a 80,80. Causa y arreglo: S6-R1. |
| S6-11 | rl | PASA (sobre `25ab972c`) | Con la paleta en 60,60 al cerrar, al reabrir AutoCAD el primer RACKCAD la mostró en 60,60 con 1290 × 860, y un segundo RACKCAD no la movió. |
| S6-12 | rl | PASA (sobre `25ab972c`) | Regla de pantalla dentro de la sesión: con la paleta en 100,1200 (fuera de la pantalla), RACKCAD la trajo a 80,80 con su tamaño. |
| S6-13 | rl | PASA (sobre `25ab972c`) | Regla de pantalla al abrir la sesión: AutoCAD guardó la paleta con 1198 de alto, más que el área de trabajo (1032). Al reabrir, el primer RACKCAD la recortó a 66,0–1356,1032. Ver O-2. |
| S6-14 | st | OBSERVACIÓN | Cambiar a Layout1 y volver a Model (CTAB) con el editor de SEL-A abierto y limpio: lo marcó, y la lista pasó a «puede no estar al día». «Volver a leer del dibujo» quitó la marca. Ver O-3. |

### S6-R1: la paleta volvía a 80,80 (causa medida y arreglo)

- **Lo medido con el diagnóstico:**
  - la paleta estaba en 60,60–1350,920, y `PaletteSet.Location` devolvió `{X=60, Y=1350}`;
  - `DeviceIndependentLocation` devolvió lo mismo;
  - `Size` devolvió bien 1290 × 860;
  - con DPI 96, el área de trabajo de la pantalla principal es 0,0–1920,1032.

  En AutoCAD 2025, el getter da el borde **derecho** como Y.
- **Consecuencia:** `KeepOnScreen` (T6 rl) creía que la barra de título estaba por debajo de la pantalla y llevaba la paleta a la principal,
  a 80 de la esquina. Pasaba en cada `Show()`, también en el primero de la sesión. Por eso se perdía la posición guardada. El setter de
  `Location` sí funciona.
- **Arreglo (`25ab972c`):**
  - La regla ya no lee `Location`: toma la esquina donde WPF muestra la ventana RackCad (`_root.PointToScreen`), en píxeles de dispositivo
    como `Size` y las áreas de trabajo. Solo escribe `Location`.
  - Si la ventana no está en pantalla (`PresentationSource` nulo), no mide ni mueve.
  - Sin `DllImport` ni reflexión: las invariantes de F1-T3B siguen.
  - La guarda de la fuente fija que la regla nunca lee `Location`.
- **Números:** Core Full 19638 / 0 / 403 sobre `25ab972c`. El Plugin en Debug, con sus dependencias: 0 errores y 0 advertencias. El
  arreglo no toca la UI.

### Observaciones

- **O-1 (de la prueba, no de RackCad):** el valor de la propiedad quedó como «T6» seguido de una tabulación. La tecla Tab del operador entró
  en el campo de valor, que admite varias líneas. No es un defecto de pr.
- **O-2 (cosmético, rl):** cuando la regla mueve la paleta, la deja unos 6 px a la derecha de donde estaba el marco (S6-13: 66 en lugar
  de 60), porque mide la esquina del contenido y no la del marco. Solo pasa cuando la paleta estaba fuera de alcance.
- **O-3 (st, para el Owner):** la señal cuenta cualquier objeto del dibujo añadido, borrado o modificado. Cambiar de presentación modifica
  objetos (la presentación y sus ventanas), así que marca los editores aunque ningún rack haya cambiado. El aviso es conservador, coherente
  con lo diseñado (O-5: «Avisar y ofrecer releer») y no pierde nada. Afinarlo exigiría filtrar por objetos de RackCad en el puente de
  eventos, que está bajo las invariantes de F1. QSAVE no se probó, para no guardar la copia.
- **O-4 (de la herramienta):** AutoCAD no guarda en el perfil una posición flotante fuera de la pantalla; se queda con la última válida.
  Por eso S6-13 usó una paleta más alta que el área de trabajo en lugar de una fuera de la pantalla.
- **O-5 (del equipo):** a mitad del smoke, Windows avisó de batería baja (5 %); el cargador se conectó y no se perdió nada.

### Lo que este smoke no cubrió

- **at:**
  - cero escrituras si uno falla;
  - un bloque importado en segundo lugar;
  - el renombrado;
  - las definiciones huérfanas;
  - el tiempo con muchos racks.
- **pr:** cancelar la colocación.
- **bom:** el aviso al abrir un medio frente que no cabe.
- **seg:** el «Aceptar» del clásico.
- **b1:** el escenario B y el frontal B.
- **st:** PAN, QSAVE y matrices.

Quedan para la prueba guiada del Owner sobre el SHA final, o como limitación declarada si el Owner no las pide.

### Decisión del Owner sobre O-3 (2026-11-27)

- **La pregunta:** cambiar de presentación marca los editores abiertos aunque ningún rack cambió.
- **La respuesta:** «Dejarlo así».
  - Se acepta como aviso conservador en I-64.
  - Queda anotado como mejora futura en `docs/ideas-futuras.md`.
  - No cambia el código: la punta de esta rama tras el smoke es la de la prueba guiada del Owner.

## Prueba guiada del Owner sobre `0a34fe71`: G-1, «Calculado» de Dinámico con 10 frentes (2026-11-27)

- **Lo que vio el Owner** (captura en el chat): en un Dinámico nuevo con 10 frentes, en un monitor vertical, el bloque «Calculado» de la
  pestaña «Rack» se desordenó.
  - El valor de «Altura por poste» (P1 204 · P2 204 · …) ocupaba todo el ancho y se cortaba a la derecha.
  - Su etiqueta se partía letra a letra con ancho casi cero: quedaba un hueco en blanco.
  - «Subida de la cama» salía en vertical, una letra por línea.
- **Causa:** la plantilla de «Calculado» de Dinámico ponía el valor a la derecha (`DockPanel.Dock="Right"`) sin partirlo. Un valor largo se
  quedaba con todo el ancho y la etiqueta, la que sí se parte, se quedaba sin sitio. El monitor vertical solo lo hace más visible, porque
  la paleta es más estrecha.
- **Los otros sistemas:**
  - Push Back y Selectivo ya partían el valor, con un máximo de 300 px, así que no se desbordaban. En una paleta estrecha, sin embargo,
    apretaban la etiqueta.
  - Cabecera y Cantilever usan el mismo patrón, pero sus valores son cortos (no listan poste a poste). No se tocaron.
- **Arreglo:**
  - en Dinámico, Push Back y Selectivo, la fila es una rejilla de dos columnas: la etiqueta conserva su ancho (hasta 180 px) y el valor
    ocupa el resto, partido y a la derecha;
  - con espacio de sobra se ve como antes;
  - en Dinámico, cada «P1 204» y «F1 14» lleva un espacio que no se parte, así que el valor solo se parte entre las piezas, en « · ».
- **Prueba nueva** (UI, WPF real en STA): `DynamicSafetyTabTests.TenFrontsInANarrowPaletteWrapTheValuesAndKeepTheLabelsWhole`.
  - Un Dinámico con 10 frentes, dispuesto a 420 px de ancho.
  - Cada etiqueta ocupa como mucho dos líneas, y cada valor queda al lado de su etiqueta y dentro de la lista.
  - Las listas por poste y por frente se parten, y «P11» va en una sola pieza.
  - **Comprobación en rojo:** sin el arreglo, la etiqueta «Altura por poste» medía 255 px de alto (unas 16 líneas de 16 px).
- **Números:**
  - las pruebas de UI de los editores de Dinámico, Selectivo y Push Back: 144 / 0 / 0;
  - el proyecto UI en Release: 0 advertencias.
  - La UI completa la corre el CI.
- **En AutoCAD:** pendiente. El DLL que el Owner tiene cargado es el de `0a34fe71`; el nuevo se compila cuando cierre AutoCAD.

## Séptima tanda, cab: la cabecera nueva desde la ventana e «Insertar vista…» de una cabecera (2026-11-27)

**Rama:** local `architecture/workspace-persistente-rackcad-t7-cab`, sobre `8362d3de` (worktree `D:\w7cab`). Sin commit, sin push, sin
AutoCAD. Hecha en paralelo con la parte per («Perfil estructural», `D:\w7per`).

**La orden del Owner (2026-11-27):** «Falta la ventana de cabecera en I-64, igual la de Perfil estructural». Sus respuestas: Cabecera, «Las
dos» («Nuevo › Cabecera en esta ventana» con su borrador, «Insertar en el dibujo…» y un solo «U», e «Insertar vista…» en una cabecera
existente); RACKCABECERA, «Abren la ventana». El editor clásico se queda y nada va a `main`.

### Lo que se construyó

- **«Nuevo» › «Cabecera en esta ventana»** (botón en «Nuevo», tras «Cantilever en esta ventana»):
  - un borrador («Borrador N», SD-06) por la entrada única `OpenAsNewDraft`, con sus seis casos por sistema en
    `RackCadWindowViewModel.Drafts.cs` (puerta, editor, vida, estado, copia y olvido), como Push Back, Cantilever y la cama;
  - los valores por defecto de RACKCABECERA: `HardcodedStandardRackFrameService.CreateDefault()` (la plantilla estándar «STD-3P», su poste,
    132 in de alto y 42 de profundidad), sin nombre (T7cab-S1, S2);
  - el editor de cabeceras de la ventana (el de la cuarta tanda, cb) para ese borrador, con «Insertar en el dibujo…» en lugar de
    «Actualizar», «Descartar», «Restaurar estándar…», «Cerrar el editor» y la pestaña **«Propiedades»** de los borradores nuevos (pr, VA-S9);
  - **«Insertar en el dibujo…»** abre el panel de DG-07 con las vistas que ofrece el clásico, lateral y planta (T7cab-S3). «Insertar con N
    vistas» pide RACKCABECERANUEVO, que inserta por el despacho de «Nuevo» (`DispatchInsertion`, el mismo camino que la tarjeta «Cabecera»):
    la colocación con el jig («Punto de insercion de la cabecera», T7cab-S4), el nombre automático de I-60 («Cabecera N»), las propiedades
    escritas tras la inserción en la misma orden (`WithNewRackProperties`) y un solo «U». Con el jig cancelado no se escribe nada y el
    borrador sigue con sus cambios;
  - al colocarse, el borrador pasa a ser una cabecera del dibujo: toma el nombre que le dio el dibujo y su base es lo enviado con ese nombre
    (T7cab-S11). La ventana lo dice con el texto de la puerta («AutoCAD insertó el rack nuevo «Cabecera 3» con 2 vistas…»);
  - «Duplicar» y «Descartar el borrador», los de los demás borradores (T7cab-S10).
- **«Insertar vista…» en una cabecera del dibujo** (botón en la cabecera del editor, junto a «Actualizar»):
  - el mismo panel y las mismas reglas que Push Back y Dinámico: se abre aunque no se pueda insertar y dice por qué; otra petición, otro
    dibujo o la marca «El dibujo cambió» (st, T6st-S5) lo detienen; no escribe la pestaña «Propiedades» (VA-S10);
  - RACKCABECERAINSERTAR: las comprobaciones de RACKCABECERAAPLICAR (la petición, el dibujo, las vistas hermanas, el plan) y la mitad de
    dibujo nueva `RackCabeceraCommands.ApplyCabeceraInsert`, una COPIA de la rama `!window.UpdateOnly` de `EditCabecera` (CB4-09, como
    `ApplyCabeceraUpdate`): AUTH-13, el preflight de I-11, el aviso de unidades, la cola de ID18 preparada desde el sobre y el proyecto de la
    vista elegida con el borrador (**B-1**: `RackViewBatchProducts.Header(document, config, id, name, embed, project, …)`), el redibujo de
    las hermanas y la regla del redibujo incompleto (`RackInsertRedrawOutcome.Of`, b1): si un redibujo falla, no se coloca la vista nueva;
  - la respuesta es `RackDoorRules.AnswerForInsert`, como en Push Back.
- **La pestaña «Propiedades» de una cabecera suelta** (VA-07), también al abrirla con «Editar aquí» (T7cab-S6): RACKCABECERAABRIR la lee,
  RACKCABECERAAPLICAR y «Actualizar todo» la escriben (`WithRackProperties`), y RACKPROPIEDADES con una cabecera abre su pestaña: su lectura
  (RACKCABECERAABRIR) pasa a ser una de las que corren dentro de la orden (`RackCadShortcutRules.InlineReads`).
- **RACKCABECERA y RCB abren la ventana** en «Nuevo» › «Cabecera en esta ventana», con el patrón de los atajos de rl
  (`RackCadWindowShortcuts.Open`, destino nuevo `RackCadShortcutTarget.NewCabecera`, marcas `WindowFlags`): no leen ni escriben, y la línea
  de órdenes dice «RackCad: «Nuevo» › «Cabecera en esta ventana» en la ventana RackCad: Borrador N, con los valores por defecto del
  configurador clásico; …» (T7cab-S7). El cuerpo clásico de RACKCABECERA se conserva, sin registrar, como `RackCabeceraClassic()`
  (T7cab-S8).
- **El configurador clásico sigue alcanzable:** RACKMENU › «Diseñar cabecera» (el módulo `HeaderEditorModule`, sin pasar por RACKCABECERA),
  la tarjeta «Cabecera» de «Nuevo» (RACKNUEVO Cabecera, T7cab-S5), RACKEDITAR y QUICKCABECERA no cambian.
- **«Actualizar todo» y el veto al cerrar** (comprobado lo que ya hacían; solo se completó lo que faltaba):
  - «Actualizar todo» ya contaba las cabeceras sueltas; ahora una cabecera de «Nuevo» no entra (como los demás borradores sin insertar) y su
    escritura lleva la pestaña «Propiedades», como su «Actualizar»;
  - el veto de cierre (D-08), la poda de un dibujo cerrado y la lista ya contaban las cabeceras; ahora nombran un borrador por su nombre de
    borrador («Borrador 1 (Cabecera nueva)»);
  - «El dibujo cambió» no marca nunca una cabecera de «Nuevo» (no está en el dibujo).

### Supuestos para el Owner

| Id | Supuesto o decisión | Dónde se cambia |
|---|---|---|
| T7cab-S1 | **El borrador nace sin nombre.** El clásico muestra el de la plantilla («Estandar (3 paneles)»), que I-60 también trata como «sin nombre»; al insertar, los dos toman «Cabecera N». Así la cabecera se comporta como los demás borradores («si no le pones nombre, toma el automático») | `CreateCabeceraDraft` |
| T7cab-S2 | **Los valores por defecto** son los de RACKCABECERA y de la tarjeta «Cabecera»: `HardcodedStandardRackFrameService.CreateDefault()` (plantilla «STD-3P», su poste, 132 in, 42 in), con el catálogo de la ventana, como los demás borradores | `CreateCabeceraDraft` |
| T7cab-S3 | **Las vistas** de «Insertar en el dibujo…» e «Insertar vista…» son lateral y planta (las de «Insertar en AutoCAD», «Insertar planta» e «Insertar varias…» del clásico). En una cabecera nueva el panel abre con las dos marcadas (DG-02, la primera de cada grupo), lateral primero; «Insertar en AutoCAD» del clásico solo insertaba la lateral | `CabeceraEditorViewModel.InsertOptions` |
| T7cab-S4 | **El aviso del jig** es el de siempre de la cabecera, «Punto de insercion de la cabecera», para cada vista (lateral o planta): ya nombra lo que se inserta (O-3 de ui lo conservó para la cabecera). No hay un texto nuevo «de la vista de la cabecera» | `RackJigPrompt` (sin cambios) |
| T7cab-S5 | **La tarjeta «Cabecera» de «Nuevo»** sigue abriendo el configurador clásico (RACKNUEVO Cabecera), como las tarjetas de los demás sistemas (CM-S12); «Cabecera en esta ventana» va encima, con los botones «… en esta ventana». El clásico sigue también en RACKMENU › «Diseñar cabecera» | `RackCadWindowView.Cabecera.cs`, `NewSystems` |
| T7cab-S6 | **La pestaña «Propiedades» también en una cabecera del dibujo** (abierta con «Editar aquí»), no solo en la nueva: tras insertarla, su pestaña es la del dibujo (pr), así que al reabrirla debe ser la misma. Consecuencia: **T6rl-S10 deja de valer**; RACKPROPIEDADES con una cabecera abre su pestaña en la ventana (antes remitía a la ventana clásica de RACKMENU) | `CabeceraEditorViewModel.New.cs`, `RackCadCabeceraDoorCommands`, `RackCadShortcutRules.InlineReads` |
| T7cab-S7 | **RACKCABECERA y RCB:** cada orden abre un borrador nuevo, como un clic en el botón (hasta el límite de 8 por dibujo); no leen ni escriben y llevan las marcas de los atajos de rl (`NoUndoMarker \| UsePickSet \| Redraw`; antes RACKCABECERA no llevaba ninguna) | `RackCabeceraCommands.RackCabecera` |
| T7cab-S8 | **El cuerpo clásico de RACKCABECERA** (el configurador modal y su primera vista con `DrawAndPlace`) se conserva sin registrar, como `RackCabeceraClassic()`: el menú clásico llega al mismo configurador por su botón (`HeaderEditorModule`), así que hoy nadie lo llama. No se borra por la orden del Owner | `RackCabeceraCommands.cs` |
| T7cab-S9 | **«Abrir» de un diseño de cabecera de la biblioteca** sigue en el configurador clásico (no se pidió; `OpensHere` no cambia). La entrada de los borradores ya admite un proyecto de cabecera (lo usa «Duplicar») | `RackCadWindowViewModel.Utilities.cs` (sin cambios) |
| T7cab-S10 | **«Duplicar»** de una cabecera de «Nuevo»: la copia no tiene nombre y toma el marco de su original como un cambio sobre la base de su original; su «Descartar» vuelve a los valores por defecto, con el texto de T6 ui | `CreateCabeceraDraft`, `TakeDesignAsEdit` |
| T7cab-S11 | **Tras insertarla**, el editor toma el nombre que le dio el dibujo (I-60: «Cabecera N», también si tenía el nombre de una plantilla) y su base es la cabecera enviada con ese nombre: queda «Limpio» | `CabeceraEditorViewModel.MarkInserted` |
| T7cab-S12 | **Textos nuevos:** el botón de «Nuevo» y su explicación; «Cabecera nueva · plantilla «…». Todavía no está en el dibujo: se coloca con «Insertar en el dibujo…»…»; «Una cabecera nueva se escribe en el dibujo con «Insertar en el dibujo…».»; el título del panel, «Insertar la Cabecera nueva en el dibujo» (femenino; los demás sistemas no cambian); la ayuda de los grupos lateral y planta; el resumen de RACKCABECERA en «Órdenes y atajos» (y en `despliegue.md`), su «En la ventana» y la línea de «Dónde quedó el menú clásico» | los archivos de arriba |
| T7cab-S13 | **Los textos de la puerta** que dicen «rack» se reutilizan para la cabecera («AutoCAD insertó el rack nuevo «Cabecera 3»…», «…redibujó 1 vista del rack…»), como ya hacía «Actualizar» de una cabecera | `RackDoorRules` (sin cambios) |

### Las ediciones a archivos compartidos (pequeñas, marcadas `T7 cab`)

- Application: `RackCadShortcuts.cs` (el destino `NewCabecera`, su texto de línea de órdenes, `RACKCABECERAABRIR` en `InlineReads`, y fuera el
  texto de T6rl-S10).
- UI: `RackCadWindowViewModel.cs` (una línea en `RaiseNewDynamic`), `.Drafts.cs` (los seis casos), `.Ins.cs` (una línea), `.DrawingChange.cs`
  (una condición), `.UpdateAll.cs` (dos líneas), `.NewRackProperties.cs` (un caso), `.Shortcuts.cs` (el destino, el caso de la cabecera en
  RACKPROPIEDADES), `.Utilities.cs` (un texto); `RackCadWindowView.xaml.cs` (una línea en el constructor); `RackViewInsertPanelViewModel.cs`
  (el artículo del título); `RackCommandReference.cs` y `RackCadHelpViewModel.cs` (una entrada cada uno).
- Plugin: `RackCadWindowShortcuts.cs` (un caso), `RackCadUpdateAllCommands.cs` (las propiedades de la cabecera).
- Archivos nuevos: `CabeceraEditorViewModel.New.cs`, `CabeceraDoorInsertion.cs` (UI); las pruebas `RackCadNewCabeceraTests.cs` (UI).
- Archivos de la cabecera (de cb): `CabeceraDoor.cs`, `CabeceraDraftEditor.cs` (`BaselineJson`, `TakeAsEdit`), `CabeceraEditorViewModel.cs`,
  `CabeceraEditorView.xaml(.cs)`, `RackCadWindowViewModel.Cabecera.cs`, `RackCadWindowView.Cabecera.cs`, `RackCadCabeceraDoorCommands.cs`,
  `RackCadRackDoorPort.Cabecera.cs`, `RackCabeceraCommands.cs` y `RackCabeceraCommands.Door.cs`.
- **Para integrar con per:** per añade también un destino a `RackCadShortcutTarget` (RACKSECCION), una línea en `OnShortcutAsked` y una
  línea en el constructor de `RackCadWindowView`, junto a las de esta parte: se suman. Los censos de órdenes (+2 aquí) también se suman.

### Censos y guardas que cambian, con su nota

- `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: de 71 a 73 (RACKCABECERAINSERTAR y RACKCABECERANUEVO; RACKCABECERA y RCB
  cambian de cuerpo, no de nombre).
- `CustomPropertiesCommandGuardTests`: `A14CabeceraDoorCommands` gana esos dos nombres; el censo de apertura no cambia.
- `PushBackCabeceraInsertedViewDesignTests.TheQueueOfCabeceraPreparesTheNewViewFromTheEditedSource…`: la guarda pedía exactamente esto al
  añadir «Insertar vista…» de una cabecera («it must prepare its new view with RackViewBatchProducts.Header (B-1) and answer the queue with
  RackInsertRedrawOutcome (I-02); then change this count»). Se reapuntó con la misma dureza: cuatro órdenes; la puerta no prepara cola (su
  mitad de dibujo sí); `ApplyCabeceraUpdate` sigue sin cola ni `TryAuthorize`; `ApplyCabeceraInsert` prepara desde el sobre y el proyecto de
  la vista, redibuja las dos clases de vista con `config, id, name` y responde a la cola con la regla.
- `InsertRedrawCompletenessTests.EveryDrawingHalfThatInsertsAnswersTheQueueWithTheRule`: una sexta mitad, `ApplyCabeceraInsert`.
- `RackCadWindowListTests.TheFoldersOfTheWindowKeepTheirLayers`: `ApplyCabeceraInsert` entra en la lista de lo que ningún otro archivo de la
  puerta puede llamar.
- `RackCadShortcutTests.OnlyTheReadsOfTheWindowRunInsideTheCommand` y `TheCommandRunsInsideItselfTheRead…`: siete lecturas (RACKCABECERAABRIR,
  T7cab-S6); las escrituras de la cabecera siguen fuera.
- `NewRackPropertiesTests.EveryNuevoCommandWritesTheTabOfItsRequest…`: un sexto caso, RACKCABECERANUEVO.
- `RackUnitsGuardSourceTests.EachInsertionPath_CallsTheGuard…`: `RackCabeceraCommands.Door.cs`, una vez (la de `RackCabeceraCommands.cs`
  sigue en 3: el cuerpo clásico de RACKCABECERA se conserva).
- `UpdateAllRunTests`: la cabecera escribe su pestaña «Propiedades».
- UI: `CabeceraEditorTests.TheViewHasItsFourTabs…` (cinco pestañas en una cabecera suelta, y el botón nuevo en los nombres únicos);
  `RackCadNewDraftsTests.OpenAsNewDraftRefuses…` (la cabecera ya tiene editor: sin su puerta se rechaza por la puerta, y el ejemplo de
  «sin editor» pasa a Larguero); `RackCadUpdateAllTests` (la forma de la escritura de la cabecera lleva `NewRack`, `Inserts` y la pestaña);
  `RackCadNewDraftPropertiesTests` (las siete teorías de pr ganan el caso de la cabecera).
- Ninguna guarda se debilitó.

### Las pruebas

- **Core, `CabeceraDoorTests` (+6):** la regla de la puerta (`RefusalOf`); la lectura con las propiedades y la escritura de la pestaña en
  RACKCABECERAAPLICAR; RACKCABECERAINSERTAR por su fuente (en orden: la petición, el dibujo, las hermanas, el plan, la mitad de dibujo, la
  cuenta de lo insertado y `AnswerForInsert`, sin «escrito» a mano ni propiedades); RACKCABECERANUEVO por su fuente (despacho de «Nuevo» con
  la identidad de la ventana, luego las propiedades) y `CabeceraDoorInsertion`; **la guarda espejo** de `ApplyCabeceraInsert` contra la rama
  «Insertar» de `EditCabecera` (veinte pasos en el mismo orden, y las diferencias declaradas); RACKCABECERA abre la ventana y el clásico sigue
  en el menú (el cuerpo conservado, el botón «Diseñar cabecera» y `HeaderEditorModule`). Y la puerta: los cuatro huecos y su abandono.
- **UI, `RackCadNewCabeceraTests` (13, nuevo):** el borrador con los valores del clásico y sin nombre, sin «Actualizar»; «Insertar en el
  dibujo…» (las vistas, el orden, la copia, la puerta, el bloqueo); la cabecera colocada (el nombre de I-60, «Limpio», su página después y su
  «Actualizar»); el jig cancelado (nada escrito, el borrador con sus cambios y su nombre, y otra vez); «Duplicar» y «Descartar»; «Insertar
  vista…» de una cabecera del dibujo (lo enviado, la respuesta, el redibujo incompleto); la puerta que rechaza (el panel vuelve), otra
  petición en vuelo y «El dibujo cambió» (que no marca el borrador); la pestaña «Propiedades» leída, escrita, bloqueada mientras escribe y
  descartada; las propiedades de una cabecera nueva al insertarla; RACKCABECERA (dos veces) y RACKPROPIEDADES con una cabecera; «Actualizar
  todo», el veto y la poda; y la vista con WPF real (STA): el botón de «Nuevo» tras el de Cantilever, el editor en «Nuevo», el panel, nombres
  únicos, sin errores de enlace, y la vista que pasa a la página del rack al insertarse.
- **UI, `RackCadNewDraftPropertiesTests` (+7):** el caso de la cabecera en las siete teorías de pr.

### Comprobación en rojo

Veintiocho piezas rotas a propósito, una a una, con script (`S\hs65\t7\cab\red.py`; salida en `red-out.txt` y `red-out-2.txt`). Antes de
cada rotura, el script copia el archivo FUERA del worktree (`S\hs65\t7\cab\red-orig`); aplica un cambio exacto (si no coincide una sola vez,
no la aplica); compila si hace falta; corre las clases focales (Core: `CabeceraDoorTests`, `PushBackCabeceraInsertedViewDesignTests`,
`InsertRedrawCompletenessTests`, `RackCadShortcutTests`, `NewRackPropertiesTests`; UI: `RackCadNewCabeceraTests`,
`RackCadNewDraftPropertiesTests`, `CabeceraEditorTests`, `RackViewInsertPanelTests`, `RackCadCabeceraWindowTests`); y devuelve el archivo
desde la copia con el mismo SHA-256 (lo comprueba en las veintiocho). Las piezas que solo se leen como texto (el Plugin) corrieron antes que
las que recompilan Core, para no usar un binario roto (la lección de pr). Al final, la solución se recompiló con `--no-incremental` y
`git diff` no tiene restos (`if (false`, `true ||`).

| Pieza | Qué se rompió | Fallan |
|---|---|---|
| C1 | `RefusalOf` deja pasar una cabecera nueva sin vistas (Application) | 1: `TheRuleOfTheDoorRefusesAWriteWithNoCabecera…` |
| C2 | RACKCABECERAABRIR fuera de `InlineReads` (Application) | 2: las dos de `RackCadShortcutTests` |
| C3 | RACKCABECERANUEVO escribe `null` en lugar de las propiedades de su petición | 2: `ANewCabeceraIsInsertedByTheDispatchOfNuevo…` y el caso de la cabecera de `EveryNuevoCommandWritesTheTabOfItsRequest…` |
| C4 | B-1: la cola de «Insertar vista…» sin el sobre de la vista elegida | 2: la guarda espejo y la de B-1 |
| C5 | I-02: la cola de «Insertar vista…» responde `Applied()` | 3: la guarda espejo, la de B-1 y el caso de `ApplyCabeceraInsert` de `EveryDrawingHalfThatInsertsAnswersTheQueueWithTheRule` |
| C6 | `RunRead` sin RACKCABECERAABRIR | 1: `TheCommandRunsInsideItselfTheRead…` |
| C7 | RACKCABECERAABRIR sin las propiedades | 1: `TheReadCarriesThePropertiesOfTheRack…` |
| C8 | la puerta guarda la cabecera nueva en el hueco de «Actualizar» | 1: `TheDoorKeepsTheCabeceraRequestsInTheirOwnSlots…` |
| C9 | RACKCABECERA sin las marcas de los atajos | 1: `RackCabeceraOpensTheWindow…` |
| U1 | sin el caso de la cabecera en `DraftDoorOf` | 18 (11 de `RackCadNewCabeceraTests`, las de la cabecera en las teorías de pr) |
| U2 | el borrador toma el nombre de la plantilla | 4 |
| U3 | «Insertar en el dibujo…» no pide una cabecera nueva | 5 |
| U4 | la petición nueva sin la pestaña «Propiedades» | 5: `ANewCabeceraWritesItsPropertiesAtItsInsertion` y los casos de la cabecera de cuatro teorías de pr |
| U5 | la respuesta no hace de la cabecera nueva una del dibujo | 7 |
| U6 | la cabecera insertada no toma el nombre que le dio el dibujo | 1 |
| U7 | el panel sin la planta | 3 |
| U8 | «El dibujo cambió» no detiene «Insertar vista…» | 1 |
| U9 | un borrador nuevo de cabecera se marca «El dibujo cambió» | 1 |
| U10 | «Actualizar todo» trata una cabecera nueva como del dibujo | 0 en la primera pasada: la prueba miraba que no entrara, y no entraba (la cabecera nueva no puede «Actualizar»), pero ahora la impedía con su motivo. Se añadió a la prueba que no la impide; en la segunda pasada, 1: `ActualizarTodoAndTheVetoOfTheClose…` |
| U11 | RACKCABECERA no abre «Cabecera en esta ventana» | 1 |
| U12 | RACKPROPIEDADES no abre el editor de la cabecera | 1 |
| U13 | «Duplicar» no copia el marco de su original | 1 |
| U14 | el título del panel en masculino | 1 |
| U15 | «Nuevo» sin el botón (`BuildCabeceraChrome`) | 1 (la vista en STA) |
| U16 | la pestaña «Propiedades» no cuenta en «Modificado» | 9 |
| U17 | «Descartar» de un borrador con el texto de una cabecera del dibujo | 1 |
| U18 | «Actualizar» ofrecido a una cabecera nueva y con una propiedad no válida | 1 |
| U19 | «Editar aquí» no carga las propiedades leídas | 1 |

La segunda pasada (`red-out-2.txt`: U10, C3 y C5, con los nombres completos de los casos de las teorías) corrió C3 y C5 sin recompilar sobre
el binario de Core que había dejado C2; por eso añade las dos de `RackCadShortcutTests`, que no son de esas piezas. Los números de C3 y C5 de
la tabla son los de la primera pasada.

### Los números

Sobre las fuentes finales, con la solución recompilada en Debug con `--no-incremental` tras la comprobación en rojo:
- **UI completa:** 3390 correctas, 0 con error, 48 omitidas (3438); 18 min. Es la base de `8362d3de` (3370 / 0 / 48) más las 20 nuevas: las 13
  de `RackCadNewCabeceraTests` y los 7 casos de la cabecera en las teorías de pr.
- **Core Full:** 19647 correctas, 0 con error, 403 omitidas (20050); 9 min. Es la base (19638 / 0 / 403) más las 9 nuevas: 6 de
  `CabeceraDoorTests` y un caso más en `InsertRedrawCompletenessTests`, `NewRackPropertiesTests` y `RackUnitsGuardSourceTests`.
- **La solución en Debug (Plugin incluido):** 0 errores. **En Release:** 0 errores y ninguna advertencia `MSB`, `CS`, `NU` ni `NETSDK`; las 82
  que quedan son de los analizadores de xUnit, de antes y de otros archivos.
- Los 46 archivos cambiados o nuevos, sin caracteres de control y con finales de línea CRLF en todo el archivo.

### Lo que comprobará el smoke en AutoCAD

1. **RACKCABECERA** (y RCB): la ventana se muestra en «Nuevo» con «Borrador N» y el editor de cabeceras (pestaña «Alzado», plantilla
   «Estandar (3 paneles)», 132 in); la línea de órdenes dice «RackCad: «Nuevo» › «Cabecera en esta ventana»…». Un «U» no deshace nada de
   esta orden.
2. **«Nuevo» › «Cabecera en esta ventana»:** lo mismo. La tarjeta «Cabecera» y RACKMENU › «Diseñar cabecera» siguen abriendo el
   configurador clásico, que inserta como antes.
3. **Insertar una cabecera nueva:** cambiar un panel a «X», añadir en «Propiedades» «Zona» = «A», «Insertar en el dibujo…» → el panel con
   «Lateral» y «Planta» marcadas → «Insertar con 2 vistas». AutoCAD pide «Punto de insercion de la cabecera» dos veces. Se ven las dos
   vistas; la ventana dice «AutoCAD insertó el rack nuevo «Cabecera N» con 2 vistas…». Leer por COM: las dos vistas llevan el mismo `Design`,
   el nombre «Cabecera N» (también el interior) y la propiedad «Zona». **Un solo «U»** quita las dos vistas y la propiedad.
4. **El jig cancelado:** Esc en el primer punto. Nada escrito; el borrador sigue «Sin insertar, con cambios», con su propiedad.
5. **Después de insertarla:** «Volver a leer la lista», abrir «Cabecera N»: el mismo editor, «Limpio». Un cambio y «Actualizar»
   (RACKCABECERAAPLICAR) redibuja las dos vistas y escribe la pestaña; un «U».
6. **«Insertar vista…» en una cabecera existente** con un cambio sin escribir (el claro de un panel): «Planta» → RACKCABECERAINSERTAR
   redibuja la lateral con el cambio y coloca la planta. Leer por COM: los sobres de las dos vistas llevan el mismo `Design` byte a byte
   (B-1), y la ventana vuelve a abrir la cabecera (sin el rechazo de M-02). Un «U» lo deshace todo.
7. **«El dibujo cambió»:** con el editor de una cabecera abierto, un ERASE de otra cosa: el aviso, e «Insertar vista…» dice por qué no inserta
   hasta «Volver a leer del dibujo».
8. **RACKPROPIEDADES** con una cabecera seleccionada: abre la página de la cabecera y su pestaña «Propiedades».
9. **«Actualizar todo»** con una cabecera modificada y un borrador de cabecera nueva modificado: «Actualizar todo (1)»; el borrador no entra.
10. **Cerrar el dibujo** con un borrador de cabecera modificado: el veto lo nombra («Borrador N (Cabecera nueva)»).

### Preguntas para el Owner (respondidas el 2026-11-27)

1. **T7cab-S6, la pestaña «Propiedades» en las cabeceras del dibujo** (y RACKPROPIEDADES que la abre, en lugar de la ventana clásica):
   ¿se queda? Recomendación: sí. **Respuesta del Owner: «Sí, también las existentes»**, como se construyó.
2. **T7cab-S3, las vistas marcadas al abrir el panel de una cabecera nueva:** las dos (lateral y planta, la regla del panel en todos los
   sistemas) o solo la lateral («Insertar en AutoCAD» del clásico). Recomendación: las dos. **Respuesta del Owner: aprobado**, lateral y
   planta marcadas.
3. **T7cab-S5, la tarjeta «Cabecera» de «Nuevo»:** que siga abriendo el configurador clásico (como las demás tarjetas) o que abra «Cabecera
   en esta ventana». Recomendación: como las demás. **Respuesta del Owner: aprobado**, la tarjeta sigue con el clásico y el botón va aparte.
4. **T7cab-S9, un diseño de cabecera de la biblioteca:** que se abra también como borrador de la ventana. Recomendación: sí, en otra tanda.
   **Respuesta del Owner: «Sí, en otra tanda»**: anotado en `docs/ideas-futuras.md` («I-64 séptima tanda, cab — lo que queda para otra
   tanda»), sin construirlo.

### Séptima tanda, cab: correcciones de la revisión (sobre 258c16b1)

Sobre `258c16b1` (el commit de la sesión con esta parte), en `D:\w7cab`. Sin commit, sin push, sin AutoCAD. La revisión no tuvo bloqueantes.
Las cuatro respuestas del Owner están arriba, en «Preguntas para el Owner».

**I-1, la elección: EXTRAER.** La guarda espejo de la copia `ApplyCabeceraInsert` solo comprobaba veinte fragmentos en orden, y quitar el
`return` tras un preflight rechazado o tras una cola de ID18 que no se preparó no la rompía. Como el clásico se queda, la copia habría sido
para siempre. Se extrajo:
- la rama `!window.UpdateOnly` de `EditCabecera` es ahora `ApplyCabeceraInsert(document, blockId, embed, project, config, id, name,
  window.InsertViews); return;`, y `ApplyCabeceraInsert` es esa rama tal como era: el borde de AUTH-13, el preflight de I-11, el aviso de
  unidades, la cola de ID18 desde el sobre de la vista elegida (B-1), el redibujo de las hermanas, una regeneración y la colocación con la
  regla del redibujo incompleto (b1). Los mensajes y el orden no cambian;
- lo que se quitó de la copia para que fuera idéntica: la comprobación de una cabecera o unas vistas nulas (el clásico no la tenía; en la
  puerta ya la hacen `RefusalOf` y `PlanApply`) y el cálculo de la identidad, que hace ahora quien llama con la regla de RACKEDITAR (el GUID
  del sobre; el de la petición, o el de la ventana clásica, solo si el sobre no tiene);
- lo que queda en `EditCabecera` es «Actualizar» (sin cambios: los bloques `if (!window.UpdateOnly)` que había dentro nunca corrían en esa
  rama). `ApplyCabeceraUpdate` sigue siendo una copia de esa rama (CB4-09), con su guarda;
- **por qué se puede:** el cuerpo movido es el mismo código, con las mismas variables; el único cambio de forma es que la identidad llega
  calculada. El clásico hace exactamente lo mismo que antes.
- Esto sustituye lo dicho arriba de `ApplyCabeceraInsert` como «una COPIA de la rama `!window.UpdateOnly` de `EditCabecera`».

| Punto | Qué se hizo | Prueba |
|---|---|---|
| I-1 | Extracción (arriba). Guardas reapuntadas con la misma dureza: `RackUnitsGuardSourceTests` (en `RackCabeceraCommands.cs` 2 avisos: `RackCabeceraClassic`, sin llamador, y QUICKCABECERA; en `.Door.cs` 1; la prueba de «Insertar» de `EditCabecera` pasa a una propia: la llamada es la rama de `!window.UpdateOnly`, «Actualizar» no avisa y la mitad avisa antes del primer redibujo); `G9bInsertWiringGuardTests` y `RackViewBatchDriverGuardTests` (el borde de AUTH-13 y `authorized.Blocks` están en `.Door.cs`; el segundo comprueba además que no siguen en `RackCabeceraCommands.cs`); `InsertRedrawCompletenessTests` (la fila de `EditCabecera` se va, la de `ApplyCabeceraInsert` cubre las dos órdenes, y una prueba nueva fija que `EditCabecera` no tiene cola propia); `PushBackCabeceraInsertedViewDesignTests` (`EditCabecera` llama a la mitad con su sobre; la cola, con el sobre, se comprueba en la mitad) | Core: `CabeceraDoorTests.RackEditarInsertarAndInsertarVistaOfTheWindowRunTheOneDrawingHalfOfInsertar` (la llamada exacta y su sitio, nada de «Insertar» en lo que queda de `EditCabecera`, la identidad en la puerta, y los únicos llamadores); `TheDrawingHalfOfInsertarStopsBeforeAnyGeometryWhenTheEdgeThePreflightOrTheQueueRefuse` (las tres paradas exactas con su `return (0, 0)`, antes del primer redibujo; los pasos en orden; ningún otro `return`; y que la guarda muerde con cuatro mutaciones: sin cada `return` y sin el aviso de unidades); `InsertRedrawCompletenessTests.TheInsertarOfACabeceraIsItsOneDrawingHalf`; `RackUnitsGuardSourceTests.EditCabeceraInsert_IsGatedByUpdateOnly_AndItsHalfWarnsBeforeTheFirstRedraw` |
| M-1 | `HandedOverOf` de «Actualizar todo» entrega también la cabecera (`WriteHandedOver`) | UI: `RackCadNewCabeceraTests.ActualizarTodoWritesTheTabOfTheCabeceraAndItsPanelDoesNotComeBackAfterDejarDeEsperar` |
| M-2 | El comentario de `RackCabeceraClassic` dice que no tiene llamador y por qué el clásico sigue alcanzable (RACKMENU › «Diseñar cabecera» y la tarjeta «Cabecera», por `HeaderEditorModule` y `DispatchInsertion`); `RackUnitsGuardSourceTests` lo anota junto a su recuento. No se borró | — (comentarios) |
| M-3 | Una línea en `docs/guias/generacion-cabecera-lateral.md`, `catalogos-y-plantillas.md`, `validacion-manual-autocad.md` y `README.md`: RACKCABECERA abre la ventana en «Nuevo» › «Cabecera en esta ventana»; el configurador, en RACKMENU › «Diseñar cabecera» | — (documentación) |
| M-4 | La ayuda emergente del botón sale de `InsertViewToolTip`: en una cabecera nueva, «Coloca la cabecera nueva en el dibujo con las vistas que elijas, en su orden; si no le pones nombre, toma el automático.» (T7cab-S14); en una del dibujo, la de antes. Push Back tiene lo mismo: no se tocó, anotado en `docs/ideas-futuras.md` | UI: la vista en STA (`NuevoOffersCabeceraEnEstaVentana…`), antes y después de insertarla |
| M-5 | Pruebas nuevas: «otro dibujo» para «Insertar en el dibujo…» y para «Insertar vista…» (el panel lo dice, no se envía nada, y de vuelta en su dibujo el borrador inserta); el comportamiento de `CabeceraDoorInsertion.NewRackRequest` (la cabecera, la identidad, sin proyecto, la primera vista y todas en su orden; `null` sin vistas o sin cabecera); y en rojo `RackCadUpdateAllCommands.cs` (la pestaña) y el lado UI de «Actualizar todo» | UI: `ANewCabeceraAndInsertarVistaOfAnotherDrawingAreNotWrittenInTheActiveOne`, `TheRequestOfANewCabeceraCarriesTheCabeceraTheIdentityAndEveryViewInItsOrder`, `ActualizarTodoWritesTheTabOfTheCabecera…`; Core: `UpdateAllRunTests` (ya estaba) |
| M-6 | El texto de T7 cab de `A14CabeceraDoorCommands` entra en su `<summary>`; el historial del censo de `SelectiveEditorOpenTests` gana la línea de 71 a 73 | — (comentarios) |

**Supuesto nuevo:**

| Id | Supuesto | Dónde se cambia |
|---|---|---|
| T7cab-S14 | La ayuda emergente de «Insertar en el dibujo…» de una cabecera nueva: «Coloca la cabecera nueva en el dibujo con las vistas que elijas, en su orden; si no le pones nombre, toma el automático.» | `CabeceraEditorViewModel.NewRackInsertToolTip` |

**Comprobación en rojo:** once piezas, una a una, con `S\hs65\t7\cab\red2.py` (la mecánica de `red.py`: copia fuera del worktree, cambio
exacto, clases focales y restauración con el mismo SHA-256; las de Core leen el texto sin recompilar, sobre binarios limpios). Salida en
`red2-out.txt`. Todas fallan:

| Pieza | Qué se rompió | Fallan |
|---|---|---|
| X1 | «Insertar» sigue tras un preflight rechazado (sin su `return`) | 1: `TheDrawingHalfOfInsertarStopsBeforeAnyGeometry…` |
| X2 | «Insertar» sigue tras una cola de ID18 que no se preparó | 1: la misma |
| X3 | «Insertar» sigue tras el rechazo del borde de AUTH-13 | 1: la misma |
| X4 | RACKEDITAR «Insertar» ya no llama a la mitad compartida (cae en «Actualizar») | 4: `RackEditarInsertarAndInsertarVista…`, `TheInsertarOfACabeceraIsItsOneDrawingHalf`, la de B-1 y la de unidades |
| X5 | RACKCABECERAINSERTAR toma la identidad de la petición y no la del sobre | 2: `InsertarVistaTakesItsRequest…` y `RackEditarInsertarAndInsertarVista…` |
| X6 | RACKACTUALIZARTODO escribe la cabecera sin su pestaña | 1: `UpdateAllRunTests.TheCommandIsOneCommandWithUndo…` |
| Y1 | «Actualizar todo» no entrega la cabecera | 1: `ActualizarTodoWritesTheTabOfTheCabecera…` |
| Y2 | el elemento de «Actualizar todo» de la cabecera sin su pestaña | 1: la misma |
| Y3 | la ayuda del botón de una cabecera nueva habla de las vistas del dibujo | 1: la vista en STA |
| Y4 | la petición de una cabecera nueva lleva solo la primera vista | 1: `TheRequestOfANewCabeceraCarries…` |
| Y5 | el editor no sabe que su borrador es de otro dibujo (el panel no lo dice) | 1: `ANewCabeceraAndInsertarVistaOfAnotherDrawingAreNotWrittenInTheActiveOne` |

Tras la última pieza, `git diff` sin restos y la solución recompilada con `--no-incremental`.

**Los números** (fuentes finales, solución recompilada en Debug con `--no-incremental` tras la comprobación en rojo):
- **UI completa:** 3393 correctas, 0 con error, 48 omitidas (3441); 22 min. Las 3390 de `258c16b1` más las 3 nuevas de `RackCadNewCabeceraTests`.
- **Core Full:** 19648 correctas, 0 con error, 403 omitidas (20051); 14 min. Las 19647 de `258c16b1`, menos la guarda espejo retirada, más
  las dos de I-1 en `CabeceraDoorTests`; en `RackUnitsGuardSourceTests` e `InsertRedrawCompletenessTests` una fila de teoría sale y entra
  una prueba propia (el número no cambia).
- **La solución en Debug (Plugin incluido):** 0 errores. **En Release:** 0 errores y ninguna advertencia `MSB`, `CS`, `NU` ni `NETSDK` (las
  82 que quedan son de los analizadores de xUnit, de antes).
- Los archivos tocados, sin caracteres de control y con CRLF en todo el archivo; `git diff --check`, limpio.
