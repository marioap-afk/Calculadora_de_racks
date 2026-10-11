# I-65 F4-T1-PACKAGE-HARDENING — análisis del Coordinator tras la entrega BLOCKED (intento 0)

Coordinator I-65, 2026-10-11. Cadena `F4-T1-PACKAGE-HARDENING`; delegación `R20261011T154535Z-39b9`; trabajo `R20261011T155212Z-11d5`
(RED `53bfcd94`, GREEN `a6fa5a35`); verificación `R20261011T162337Z-4794` = `EXECUTION_REWORK_REQUIRED`/`REWORK`, `FailureClass` `Ci`.

## 1. Hechos

- La implementación de T1 está completa y empujada: plantilla (tercera puerta, prefijo `gate: `, giro módulo 360, descripción no comparada,
  escritura robusta), regla de carpeta, carpeta embebida, ediciones de guardas A65-F3-05 y 06. El simulacro del Coordinator (módulo falso,
  no RAM) confirma el comportamiento del script (SIM-01..07, S1-S6).
- La corrida push de `a6fa5a35` es roja por **una** prueba de la selección (13 635 de 13 636 en verde):
  `StructuralAnalysisRamElementsHardeningGuardTests.LosDosLiteralesQueEstabanPartidosSeEscribenEnteros`.
- Causa raíz: la prueba del RED busca el token entre comillas (`"…"`), pero en `StructuralAnalysisRamElementsGuardTests.cs` el literal entero
  vive **dentro de una cadena C#** y aparece con la comilla escapada (`\"…\"`). El código que la prueba observa es correcto (A65-F3-05 cumplido);
  el defecto está solo en el patrón de la aserción.
- El Worker no podía corregirla: la regla de la cadena prohíbe editar las pruebas del RED después del commit RED (AUTOMATION_PLAN 16.8). Se
  detuvo con `BLOCKED` sin parada S/C, como debía.

## 2. El caso que 16.8 no prevé

Corregir la aserción toca `ChainRedFiles`, así que, por 16.8, la entrega de corrección «exige RED»: un commit RED cuyo job Core falle y un
GREEN que no toque `RT ∪ ChainRedFiles`. Aquí eso es imposible de forma honesta: la corrección es **solo** de una prueba cuyo sujeto es otro
archivo de pruebas de la propia cadena (una meta-prueba). No hay comportamiento de producto que desactivar en el RED ni que activar en el GREEN:
la aserción corregida pasa en cuanto se escribe, y cualquier truco para que el RED falle (reintroducir el literal partido y quitarlo en el GREEN)
obligaría al GREEN a tocar `ChainRedFiles`, que 16.8 también prohíbe. LIFECYCLE §7 punto 2 exige RED→GREEN para «cada comportamiento nuevo y
bugfix»; una aserción defectuosa de una prueba no es ninguna de las dos cosas, y los comportamientos de T1 ya tienen su RED (`53bfcd94`, 103
fallos por aserción en la selección).

## 3. Decisión (CD-I65-F4-07)

- **Corrección** de la cadena (16.8: tras el REWORK de la verificación, `attempts` 0 → 1, en un commit antes de emitir la delegación),
  con `CorrectionOf` = este análisis: el Worker cambia **solo** el patrón de esa aserción para que admita la barra invertida opcional antes de
  la comilla (y añade, en el mismo archivo, una autoprueba del patrón con entradas sintéticas: partido → falla; entero con y sin escape → pasa);
  ningún otro archivo, ningún cambio de producto.
- **DEV-I65-F4-02 (desviación de protocolo declarada):** para esta corrección el Coordinator dispone que el RED de 16.8 **no aplica**: la
  entrega se compone de un único commit de prueba sin RED nuevo; en la verificación, `RedPart` se evalúa `not_applicable` citando el RED
  vigente `53bfcd94` (acreditado por la verificación `R20261011T162337Z-4794`), y `Tests` comprueba que el diff de la corrección se limita a esa aserción y a su
  autoprueba, que no debilita lo que la prueba observa (sigue fallando ante un literal partido) y que la suite Core queda en verde. Si el
  Controller no admite la disposición, la unidad se detiene y la decisión pasa al Owner.
- **DEV-I65-F4-01:** las dos invocaciones triviales de Python se mantienen como desviación no bloqueante; el prompt de la corrección prohíbe
  invocar un intérprete Python por cualquier motivo.
- Presupuesto de Codex tras la corrección: planificación de la corrección (1), su verificación (1) y nc1-nc3 (3).
