# I-65 F3-T2 — análisis del rechazo de la planificación `R20261011T072116Z-aea0`

- **Hecho:** la aceptación del paquete dio A1, A2, A3, A4, A6 y A7 en `pass`, **A5 en `fail`** y **A8 en `fail`**. Disposición (AUTOMATION_PLAN
  16.5, la más grave): **STOP (P-03)**. No se invocó al Worker.
- **A5:** `Invariants[2]` (INV-10) difiere del contrato en un carácter: el contrato contiene la secuencia literal `—` (una barra invertida;
  en el texto JSON del archivo, `\\u2014`) y la delegación la devolvió con **dos** barras invertidas. Causa raíz: una trampa de
  transcripción en el propio contrato (una barra invertida dentro de un invariante) agravada por la nota del prompt de planificación, que
  hablaba de la «barra duplicada» del texto JSON y llevó al Controller a duplicarla en el valor.
- **A8:** `Executor.Transport` = «subagente de la sesión de escritorio» (con tilde) frente a «subagente de la sesion de escritorio» de la celda
  elegible del contrato. Causa raíz: el Controller «corrigió» la ortografía del campo (misma familia de error que las tildes de I-63; lección de
  I-61).
- **Disposición del Coordinator (CD-I65-F3-11):** el trabajo no cambia; se **reemite** el contrato de F3-T2 con el mismo alcance, los mismos
  invariantes y las mismas paradas, sustituyendo la secuencia con barra invertida por texto ASCII sin barra («FAILED - DISCARD MODEL, con la
  raya U+2014 escrita en el script como secuencia de escape»), y el prompt de planificación nombra explícitamente el valor exacto de
  `Executor.Transport`. Sin incremento de `attempts` (16.8: el STOP no cambia el trabajo). La nueva planificación cuenta como invocación de
  Codex (7 de 14 en F3).
