# Smoke-2, corrida 2: adenda del paso 50

**Clasificación final (firma del Owner, `F4-DRAFTS/coordinator/owner-f4-smoke2-step50-disposition.md`):**
- **Paso 50: CLOSED / FALSE FAIL / OPERATOR ARTIFACT.**
- **Re-smoke dirigido de navegación: PASS**, 7 de 7.
- **Smoke-2: PASS WITH LIMITATIONS.**

El registro original (`run-log-2.md`) no se reescribe: conserva el FAIL del paso 50 tal como se observó entonces. Esta adenda registra lo que vino después, en orden:
1. el diagnóstico;
2. la causa, de coordenadas y disposición del panel;
3. la reproducción intencionada;
4. el mini-smoke dirigido;
5. la reclasificación del Owner.

Capturas: `addendum-step50-screens/`, con su `derivation-record.txt`.

Lo que sigue es el informe de diagnóstico tal como se presentó al Owner antes de su decisión. Su §4 fue la petición; la respuesta del Owner es la opción (a).

---

# Paso 50 de Smoke-2 («Ver» con una selección previa): diagnóstico

2026-11-17, sesión principal, por la disposición del Owner sobre la corrida 2 («reproduce and determine… Do not guess»). Sin cambios de código. AutoCAD de 06:17Z a 06:29Z, con el DLL de C2 (Plugin `798C9366…4E48`) y la copia D1-b10, intacta por SHA-256 y sin guardar. Avisos de inicio y cierre a I-52 entregados.

## 1. Resultado

**No hay defecto de producto en «Ver» con una selección previa.** El FAIL del paso 50 se debe a **clics de la sesión que cayeron fuera del botón**.

Cuando hay un rack seleccionado, la sección «Selección» del panel pasa de 2 a 3 líneas (`Rack seleccionado: 8b3a89ec-` / `f3e0-…`). Eso baja la fila Seleccionar / Ver / Centrar unos 12 px. La sesión pulsó «Ver» en la posición que tenía el botón sin selección, y esos clics cayeron por encima del botón: no se pulsó nada. Por eso el aviso y la vista no cambiaban.

## 2. Evidencia

**a. Lo que se hizo en el paso 50.** La transcripción de la sesión registra cada clic con sus coordenadas, en el marco 1456×819:

| Hora (Z) | Estado | Clic | Resultado registrado |
|---|---|---|---|
| 03:42:26 | A seleccionado (por expresión), tras una captura | «Ver» en (91, **623**) | **funcionó**: «Desde la selección, Ver con Copia 2 de 2 va a la misma planta que por la pestaña» |
| 03:42:36 | igual, sin captura nueva | «Siguiente copia» y «Ver» en (91, **615**) | «Ver no movió la vista» |
| 03:42:49 | igual | «Ver» en (90, **611**) | «sigue sin ir a la copia 1» |
| 03:43:12 | tras Esc Esc (sin selección) | «Ver» en (91, 612) | funcionó |
| 03:43:44 y 03:43:53 | A seleccionado otra vez | «Ver» en (90, **611**) | sin cambios |

El único «Ver» con selección pulsado en la posición correcta (y = 623) funcionó. Todos los que «no hicieron nada» se pulsaron en y = 611–615, la posición del botón sin selección. El registro de la corrida resumió solo los fallidos.

**b. La geometría, medida hoy.** Panel acoplado a la izquierda, como en el Smoke-2:
- Sin selección, o con una selección sin rack (una línea, varios textos, dos racks): «Ver» ocupa y ≈ 640–656, con el centro en 648.
- Con un rack seleccionado (3 líneas en «Selección»): «Ver» ocupa y ≈ 652–670, con el centro en 660.

**c. La reproducción del síntoma, a propósito.** Con A seleccionado, la etiqueta en «Copia 1 de 2» y la vista en la planta:
- clic en (90, 648), la posición sin selección ⇒ **nada**: la vista sigue en la planta y el aviso sigue en «…copia 2 de 2.». Es exactamente el síntoma del paso 50;
- clic en (90, 660), la posición real ⇒ «Vista ajustada a la copia 1 de 2.», con la lateral.

**d. El código.** Lo que se leyó, sin ejecutarlo:
- `WorkspaceNavigationViewModel.NavigateAsync` envía una petición por clic y siempre presenta su resultado;
- `WorkspaceNavigationHost` solo rechaza, y con texto propio, si hay un comando en curso o el editor no está en reposo;
- `WorkspaceNavigationEffects.ViewReference` no depende de la selección implícita.

No se encontró ningún camino en el que la selección haga que «Ver» se ignore en silencio. Las hipótesis del Owner (selección implícita, PICKADD, foco, `SetImpliedSelection`, secuencia selección/vista) quedan descartadas por la reproducción: con PICKFIRST 1, PICKADD 2, PICKAUTO 5 y SELECTIONPREVIEW 3, «Ver» funcionó con todas las preselecciones probadas.

## 3. El mini-smoke dirigido (los 7 casos de la disposición), sobre el producto sin cambios (C2)

Rack representativo: A = PB G (planta y lateral; M = 2). Se capturó la pantalla antes de cada clic.

| # | Caso | Resultado |
|---|---|---|
| 1 | Sin preselección ⇒ Ver | **PASS**: desde la pestaña, copia 1 (lateral) y copia 2 (planta) |
| 2 | Objeto ajeno preseleccionado (una línea; «0 racks y 1 otros objetos») ⇒ Ver | **PASS**: «Vista ajustada a la copia 2 de 2.» (planta); la línea sigue seleccionada |
| 3 | El rack preseleccionado ⇒ Ver | **PASS**: por expresión, con el panel flotante y acoplado (copias 1 y 2, también tras la secuencia de los pasos 49 y 50); con un clic sobre la lateral, la copia 2 (planta) |
| 4 | Varios objetos preseleccionados ⇒ Ver | **PASS**: dos racks (A y B, «2 racks y 0 otros objetos») ⇒ copia 1 (lateral); 1325 textos ⇒ copias 1 y 2 |
| 5 | Seleccionar (con dos racks preseleccionados) | **PASS**: «Rack seleccionado en el dibujo.»; la selección implícita es exactamente `PB G - lateral 1` y `PB G - planta` |
| 6 | Centrar (con dos racks preseleccionados) | **PASS**: «Dibujo centrado en la copia 2 de 2.»; centrado en la planta sin cambiar la escala |
| 7 | Localizar (Lateral, con dos racks preseleccionados) | **PASS**: «Vista Lateral localizada (copia 1 de 1).» |

**Observaciones, no defectos de navegación:**
- la lectura «Selección» del panel se actualiza con retraso tras «Seleccionar» (siguió diciendo «2 racks»), ya anotado en el paso 49;
- la fila de navegación cambia de altura según la selección. Es lo que provocó los clics perdidos de un operador automático; un humano pulsa donde ve el botón.

## 4. Lo que se pide al Owner

La disposición del Owner clasificó el paso 50 como CONFIRMED PRODUCT DEFECT con la evidencia disponible entonces. Con esta evidencia nueva, la sesión **no** abre la corrección NARROW, para no tocar el producto sin causa en el producto. Pide la decisión del Owner:
- **(a)** reclasificar el paso 50 como OBSERVED / ARTEFACTO DEL OPERADOR (clic perdido por el desplazamiento del panel) y aceptar este mini-smoke de 7 casos como el re-smoke dirigido. Con eso el paso 50 queda CLOSED sin corrección, y Smoke-2 puede firmarse PASS WITH LIMITATIONS;
- **(b)** o mantener la clasificación y ordenar otra comprobación (por ejemplo, el Owner pulsando él mismo «Ver» con una preselección).

Opcional, sin ser corrección del paso 50: una mejora de UX de prioridad baja que fije la altura de la sección «Selección», para que los botones no se muevan.
