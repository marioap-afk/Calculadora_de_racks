# I-65 F4-T1-PACKAGE-HARDENING — disposición del Coordinator para la corrección (intento 1, WorkRunId R20261011T163420Z-5bee)

Texto literal entregado al Controller como entrada de la verificación; registrado en `docs/automation/decisions/I-65.md` §27
(commit `ab2a0fe2`, anterior a la delegación de corrección) junto con el análisis citado por `CorrectionOf`.

- **DEV-I65-F4-02 (desviación de protocolo declarada, AUTOMATION_PLAN 16.8, caso no previsto).** La corrección de una aserción defectuosa
  de una meta-prueba del RED de la cadena (`LosDosLiteralesQueEstabanPartidosSeEscribenEnteros`, cuyo sujeto es otro archivo de pruebas de
  la propia cadena) no admite un RED honesto: no hay comportamiento de producto que desactivar, y forzarlo obligaría al GREEN a tocar
  `ChainRedFiles`. Para esta corrección el RED **no aplica**: un único commit de prueba; `RedSha` = `null`; `RedPart` se evalúa
  `not_applicable` citando el RED vigente `53bfcd94a99f4d6aa201596349d90427ca9f4229` y su corrida push `38154104233` (job Core en
  `failure`). `Tests` comprueba que el diff se limita a esa aserción y a su autoprueba, que no debilita lo observado (un token partido en
  dos literales concatenados sigue sin coincidir) y que la suite Core queda en verde. LIFECYCLE §7 punto 2 queda cumplido por el RED
  acreditado de los comportamientos de T1 en la verificación `R20261011T162337Z-4794`. Si el Controller no admite la disposición, la unidad
  se detiene y decide el Owner.
- **DEV-I65-F4-01** (intento 0, ya aceptada por el Controller): sin efecto en esta corrección; la transcripción del Worker de la corrección
  no contiene ninguna invocación de un intérprete Python.
