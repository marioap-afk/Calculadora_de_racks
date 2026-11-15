# Sonda T4C-H0: guía del operador

Esto es un **diagnóstico**, no una validación del producto. La sesión mide todos los datos de máquina: hashes, horas, registro y versiones. Tú solo operas y anotas lo que ves.

## Antes de empezar (lo hace la sesión)

- **El RELEASE** de la ventana de host de I-52.
- **Las comprobaciones**, ya hechas:
  - no hay ningún AutoCAD abierto;
  - no hay ninguna variable de inyección;
  - no hay ningún bundle de RackCad;
  - no hay restos del Autoloader.
- **La copia desechable** del dibujo que elijas, con el SHA-256 del original medido antes.

## Reglas durante toda la sesión

1. Si al cargar aparece un aviso de seguridad, elige **«Cargar una vez»**. Nunca «Cargar siempre».
2. Ten abierta **solo la copia**: ningún otro dibujo.
3. Si AutoCAD se cierra de golpe: **no envíes ningún informe** y, al volver a abrir, **no recuperes nada**. Anota el paso en el que estabas.
4. **Antes de escribir `U` o `REDO`, haz clic en la línea de comandos** y comprueba que el cursor está allí: el primer clic puede solo activar la ventana. **Nunca pulses Enter ni la barra espaciadora con el foco en el panel.** Si al escribir el texto aparece en el panel, no pulses Enter: bórralo y vuelve a hacer clic en la línea de comandos.
5. **Una sola sonda a la vez,** y solo con la línea de comandos libre.
6. **Espera a «terminado»,** hasta 2 minutos. Si a los 2 minutos no dice «terminado», no pulses nada más: anótalo y avísame.
7. Si el recuadro dice **«no está en reposo»** con la línea de comandos libre, no insistas: anótalo y avísame. Es un dato de la medición.

**Antes de cada sonda, la línea de base de Deshacer:** abre la lista desplegable de Deshacer y anota las dos o tres entradas de arriba. Después de la sonda, anota las entradas nuevas que hayan aparecido y su nombre.

## Pasos

1. Abre AutoCAD y abre **la copia** del dibujo.
2. Escribe `NETLOAD` y elige `<salida>\RackCad.Plugin.dll`.
3. Escribe `RACKPANEL`. Debajo del navegador de racks aparece el recuadro «Sonda H0», con dos botones y la casilla «Contar fotogramas de WPF (fila 1)», marcada.

   **Para aquí y avísame.** Compruebo el registro (que el DLL es el correcto y que no hay inyecciones) y te doy el visto bueno.
4. **Contexto vacío, sin selección:**
   1. haz clic en la línea de comandos y pulsa Esc dos veces;
   2. toma la línea de base de Deshacer;
   3. pulsa **«H0: contexto vacío»**;
   4. espera a «terminado» y anota las entradas nuevas de Deshacer.
5. **Contexto vacío, con selección:**
   1. haz clic sobre un objeto cualquiera del dibujo, de modo que muestre sus pinzamientos, sin ejecutar ninguna orden;
   2. toma la línea de base de Deshacer;
   3. pulsa **«H0: contexto vacío»**;
   4. espera a «terminado» y anota si la selección desapareció y qué salió en la línea de comandos.
6. **Actualizar con el campo limpio:**
   1. abre el rack piloto de Push Back en el navegador, sin cambiar nada;
   2. anota si el rack quedó seleccionado, con pinzamientos;
   3. toma la línea de base de Deshacer;
   4. pulsa **«H0: Actualizar (sonda)»**;
   5. espera a «terminado» y anota.
7. **Actualizar con cambios:**
   1. cambia «Escala de anotación» a otro valor válido y pulsa Enter **en ese cuadro**. El campo queda con cambios. Comprueba que el cuadro no quedó con texto a medio escribir;
   2. anota si el rack está seleccionado, con pinzamientos;
   3. toma la línea de base de Deshacer;
   4. pulsa **«H0: Actualizar (sonda)» una vez** y no toques nada hasta «terminado»;
   5. anota:
      - si el recuadro dijo «en vuelo»;
      - si el dibujo cambió;
      - qué salió en la línea de comandos (por ejemplo, `*Cancel*`);
      - las entradas nuevas de Deshacer y sus nombres.
8. **Undo y Redo:**
   1. haz clic en la línea de comandos, escribe `U` y pulsa Enter. Anota si se deshizo **todo** el Actualizar de una vez, con las vistas como antes, o solo una parte;
   2. haz clic en la línea de comandos, escribe `REDO` y pulsa Enter. Anota si volvió todo.
9. **Repetición:**
   1. vuelve a dejar el campo con cambios. Si el panel pide «Recargar», recarga antes;
   2. repite el paso 7 una o dos veces.
10. **Opcional:** desmarca «Contar fotogramas de WPF» y repite el paso 7 una vez.
11. **Al final:** cierra el dibujo **sin guardar** y cierra AutoCAD. Avísame.

Después, recojo el registro y compruebo que el original no cambió.

## Lo que me mandas: solo observaciones

Por cada paso del 4 al 10:
- qué viste en el recuadro;
- qué salió en la línea de comandos;
- las entradas de Deshacer, antes y después;
- en los pasos 8 y 9, qué se deshizo y qué se rehízo.

Dime también cualquier cosa que no esperaras: un aviso, un cuadro de diálogo, un cierre, una pausa larga.
