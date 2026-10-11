# I-65 F4-T1-PACKAGE-HARDENING — disposición del Coordinator previa a la verificación (WorkRunId R20261011T155212Z-11d5)

Texto literal entregado al Controller como entrada de la verificación. Se registrará en `docs/automation/decisions/I-65.md` con la custodia,
después de la verificación, para no mover `HEAD` de `CurrentSha`.

- **DEV-I65-F4-01 (desviación de proceso, no bloqueante).** El Worker declaró en `Deviations`, y la transcripción lo confirma, dos
  invocaciones triviales de un intérprete Python (`python3 -c "print(1)"` y un heredoc con `print("skip")`) para comprobar si el intérprete
  existía. No leyeron ni escribieron archivos, no ejecutaron la plantilla, el script generado ni el programa de análisis, y no influyeron en
  ningún resultado. Ni el producto ni las pruebas dependen de Python. El Coordinator declara que **C-08 no se activa en su sustancia** (su
  objeto es impedir una dependencia de Python o la ejecución de la plantilla o del programa de análisis). Al repetirse un patrón ya visto en
  F3 (CD-I65-F3-12), los prompts siguientes del Worker prohíben de forma explícita invocar un intérprete Python por cualquier motivo.
- **Defecto de la aserción del RED** declarado por el Worker en `UnexpectedFindings` (`LosDosLiteralesQueEstabanPartidosSeEscribenEnteros`):
  **sin disposición previa**. El Controller clasifica la entrega tal como está.
