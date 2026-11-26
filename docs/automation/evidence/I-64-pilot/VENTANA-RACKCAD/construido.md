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
