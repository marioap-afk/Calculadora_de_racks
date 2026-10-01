# Análisis — salida inválida de la reverificación (g3-cama-d1a, R20261001T040500Z-d387)

Redacta: Coordinator de I-61 (SAME-SESSION ROLE). Base: AUTOMATION_PLAN 16.8 («No se reescribe un prompt sin `analysis.md` versionado»), 16.9 y README §8.

## Hecho

La reverificación (reejecución 1 de 2 de la fase VERIFICATION, decidida en el `analysis.md` de R20261001T035734Z-74c2) produjo una salida válida contra el esquema. Su contenido:

- las **14 comprobaciones en `pass`**, con `Ci` y `Tests` con `RedPart` = `pass`;
- `Classification` = `EXECUTION_BLOCKED`, `Disposition` = `STOP`, `FailureClass` = `StopCondition` y `TriggeredStopConditions` = `[S-04]`.

El motivo declarado (Findings 1): en `relay-record-work.json`, `Exit.HeadSha` = `b677cb57` y `Entry.HeadSha` = `b8dfc900`, y el Controller lo trata como contradicción entre dos
fotos del mismo relevo.

## Causa raíz

1. **Lectura errónea de la semántica del registro.**
   - `Exit` es el estado anterior a la invocación del Worker (README §3.1: `HEAD` = remoto = `BaseSha`). `Entry` es el estado posterior (README §3.4).
   - Entre ambos, el Worker hizo los commits RED y GREEN y su push durante la cesión: que `HEAD` pase de `BaseSha` a `CurrentSha` es el resultado esperado del trabajo.
   - Los demás hechos (`RemoteFacts`, Git actual y la entrega) concuerdan en `b8dfc900`, y el propio Controller dejó `Identity` y `Remote` en `pass`. No hay evidencia contradictoria.
2. **Salida incoherente según la regla congelada.**
   - README §8: «VERIFIED ⇔ 14 en pass»; con todas en `pass`, `FailureClass` debe ser `NONE`.
   - La regla del relevo devuelve `False` (`okVerified` y `okClass`): es **INVALID_OUTPUT**, un fallo de transporte de la fase (16.9, modos de fallo; 16.11), con disposición `BLOCKED`.
   - El delta del prompt citaba solo una dirección del bicondicional («EXECUTION_VERIFIED solo con las 14 en pass»), lo que deja hueco a esta salida.

## Decisión del Coordinator

1. La salida se registra como `INVALID_OUTPUT` (Disposition `BLOCKED`) y no respalda ningún veredicto. No hay cambio del trabajo ni de `attempts`.
2. Se reescribe la plantilla del prompt de verificación (`verify-template.md` del scratchpad; se custodia con esta decisión) solo en el delta, con dos aclaraciones factuales tomadas de las
   autoridades:
   - **Semántica Exit/Entry** (README §3.1 y §3.4): Exit es antes de la invocación del Worker y Entry después. Que `HEAD`/`ls-remote` pasen de `BaseSha` a `CurrentSha` entre ambos es lo
     esperado.
   - **Bicondicional del README §8:** `EXECUTION_VERIFIED` **si y solo si** las 14 están en `pass`. Una parada se expresa con la comprobación que la evidencia.

   No se cambia ninguna otra línea: ni el contrato base, ni el perfil, ni las entradas.
3. Se reejecuta la fase VERIFICATION con `RunId` nuevo: reejecución **2 de 2**, la última admitida (16.8); otra salida inválida → STOP P-04. Cuenta en el tope de invocaciones de G3
   como reejecución de §9.
4. Los controles nc1-nc3, si llega a haber VERIFIED, usarán esta misma plantilla (README §10: el mismo prompt salvo la ruta de entrada y el `RunId`).
5. Se **propone**, sin bloquear, para la decisión de G3: incorporar ambas aclaraciones al perfil CONTROLLER_VERIFICATION o al README §8.
