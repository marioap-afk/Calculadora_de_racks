# I-65 F4-T4 — analysis.md: planificación de la corrección rechazada (P-03)

Redactado por el Coordinator según AUTOMATION_PLAN §16.11.

## Hechos

- Planificación de la corrección (intento 3, Codex 22) sobre el contrato del intento 2 sin reemitir. Salida válida contra el esquema.
- A5 `fail`: `Authorities[0].Section` del contrato termina en «D-13, §3 (INV-13) y §4»; la delegación escribe en su lugar el carácter
  de control U+0016 seguido de «7» en las dos posiciones del símbolo de sección. Las otras cinco colecciones son idénticas.
- `CorrectionOf.AnalysisSha256` de la delegación no coincide con el SHA-256 del análisis de la corrección (difieren cinco caracteres
  hexadecimales). El prompt de la sesión daba ese SHA-256 en mayúsculas; el esquema exige `^[0-9a-f]{64}$`.
- A8 dio `fail` en la primera evaluación porque la sesión no pasó el `ChainBaseSha` de la cadena al script de aceptación; con él, `pass`.

## Causa raíz

Corrupción de transcripción del Controller en caracteres no ASCII (el mismo modo que CD-I65-F3-11 con tildes) y un prompt de la sesión
que obligaba a convertir un hash a minúsculas.

## Decisión del Coordinator (CD-I65-F4-19)

STOP P-03: la delegación se rechaza antes de invocar al Worker; no se usa. Se reemite la planificación con el mismo contrato, el SHA-256
en minúsculas y el aviso de copiar el símbolo de sección literalmente, sin secuencias de escape. No cambia el trabajo ni consume
`attempts`. Presupuesto: la reemisión es la invocación 23; con la verificación y los controles de T4 (24-27) y T5 (28-32), F4 necesita 32,
dos más que el tope de 30: se pedirá autorización al Owner antes de exceder.
