# Decisión del Owner sobre PB-T4c (I-64 F4B): una tercera prueba fuera de la lista

**Fecha:** 2026-11-21. Después del cierre de PB-T4b (`6adff90d`, F4B-E53) y de la remedición de PB-T4c para la v5.0 del paquete de PB-T4.

**Por qué se preguntó:** la emisión condicional de PB-T4c (`owner-pbt4-decisions.md`, pregunta 4) exige que la lista emitida sea la que vio el Owner. Esa lista es de 2: «la lista de operaciones del puerto y el censo de archivos de tu decisión D2 sobre el nombre, que pasa de 14 a 15 sin cambiar nada más», es decir PEG:155-163 y CT-PBT1B-15.

Al medirla sobre `6adff90d`, cae una tercera que no está en esa lista:
- **La prueba:** `WorkspaceFullCopyGuardTests.CT_PBT4B_20_…` (CT-PBT4B-20, que añadió PB-T4b).
- **Lo que comprueba:** que cada uno de los 21 archivos originales de «Saltar y duplicar» es su base salvo los `Skip` de PB-T4b. Su `SkipLine` solo devuelve a `[Fact]` los `Skip` cuyo motivo empieza por «I-64 F4B PB-T4b: », así que el `Skip` de PB-T4c en PEG deja el archivo distinto de su base.
- **La medida:** se hizo en una copia exportada, con solo los dos `Skip`, sin GREEN y con la suite completa de Core. Dio 19 605 = 19 144 + 1 `Failed` + 460 `Skipped`. La única que falla es CT-PBT4B-20 (`igual-a-la-base=False skip=5`).
- **La causa:** no depende del diseño de PB-T4c, sino del propio `Skip`. La v4.9 ya preveía la misma causa para PB-T4d (§6.4).

El análisis, con la opción alternativa, está en `v50/remeasure_pbt4c_v50-out.txt` y en la sonda `v50/out-probe-skips-r5a/`.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Lista T4c»)

**Pregunta, literal:**

> PB-T4c («Editar todo»): para emitirla sin preguntarte, su lista tenía que ser la que viste, 2 pruebas sustituidas (la lista de operaciones del puerto y el censo de archivos de D2). Al medirla cae una tercera fuera de esa lista: CT-PBT4B-20, la prueba que PB-T4b añadió para vigilar el guion de «Saltar y duplicar». Comprueba que cada archivo original es su base salvo los Skip de PB-T4b, y el Skip de PB-T4c en uno de esos archivos la hace fallar. Lo produce el propio Skip, no el diseño; ya estaba previsto que pasara en PB-T4d, pero no en PB-T4c. Lo medí en una copia exportada: con solo los dos Skip, cae esa prueba y ninguna más. ¿Cómo sigo?

**Opciones:**
1. **Añadirla a la lista (Recomendado):** La lista de PB-T4c pasa de 2 a 3. CT-PBT4B-20 se salta con registro y su equivalente es su copia literal, que además reconoce el Skip de PB-T4c; sus líneas esperadas no cambian. Si aparece otra fuera de la lista, te vuelve.
2. **Otro diseño sin tocarla:** No se toca la prueba de esa lista y «Editar todo» sale por un segundo puerto en otro objeto, con sus reenvíos. La lista queda en 1, pero cambia una decisión de diseño del paquete y tiene efectos sin medir. No lo recomiendo.
3. **No emitir PB-T4c:** PB-T4c queda sin emitir hasta que decidas otra cosa; F4B se detiene antes de «Editar todo».

**Respuesta del Owner, literal:** «Añadirla a la lista (Recomendado)».

## Lo que queda decidido

- **La lista de PB-T4c pasa de 2 a 3:** PEG:155-163, CT-PBT1B-15 y CT-PBT4B-20.
  - CT-PBT4B-20 se salta con registro.
  - Su equivalente es su copia literal, en una clase nueva. Solo cambia una cosa: también devuelve la línea exacta del `Skip` de PB-T4c en PEG. Sus líneas esperadas no cambian.
- **Es la lista que vio el Owner** para la condición de emisión de `owner-pbt4-decisions.md`, pregunta 4. Si aparece otra prueba que falle fuera de ella, vuelve al Owner.
