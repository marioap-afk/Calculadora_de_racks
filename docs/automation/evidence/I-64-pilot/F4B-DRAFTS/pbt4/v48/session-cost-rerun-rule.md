# PB-T4b: regla de la tercera corrida del coste, fijada ANTES de medirla

**Fecha:** 2026-11-20. La escribe la sesión antes de lanzar la tercera corrida, para que la regla no dependa del resultado.

**El hecho:** la precondición 6 de la v4.8 (`6ddc239d…`), paso 7, corrió dos veces la sonda del coste de la lectura (`pbt4b-read-cost-probe.cs`) sobre la copia del GREEN (`/d/r4s`). En la primera corrida no se cumple un solo criterio, «sin regresión», en un solo caso: `pb-20-ab x4 vistas +200 racks ajenos`, `origen-es-Primary`. PB-T4b da 98,2 ms, PB-T4a 65,2 ms y el límite es 84,8 ms. En la segunda corrida se cumple todo, con 47,2 ms frente a 46,5 ms. El registro está en `pbt4bcc/compile/compile-check.md`, §3.

**Lo que dice el paquete** (§6.2.1): si no se cumple, la emisión se detiene y vuelve a la sesión, que lo lleva al Owner. **La sesión no decide aceptar el coste.** Lo que hace es medir otra vez y llevarle al Owner las tres corridas.

**La regla de la tercera corrida:**
1. **Cómo se corre:** la misma sonda, sin cambiar nada (ni n, ni los escenarios, ni el orden), sobre una copia exportada nueva del mismo GREEN literal de la v4.8 (`/d/r4t`), con la máquina en las mismas condiciones. Se anota qué otros procesos pesados había en marcha.
2. **Cómo se evalúa:** con todos los criterios de §6.2.1, sin cambiar ninguno.
3. **Qué recomienda la sesión al Owner:**
   - **si la tercera corrida cumple todo,** que lo tome por cumplido: dos de tres corridas cumplen todo, y el único incumplimiento es un caso de la primera;
   - **si la tercera incumple cualquier criterio,** en el mismo caso o en otro, no recomienda aceptarlo. Recomienda una sonda dedicada a ese caso antes de emitir PB-T4b.
4. **Las tres corridas, con sus TRX por sha256,** van con la emisión, sea cual sea la decisión.
