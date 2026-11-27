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
