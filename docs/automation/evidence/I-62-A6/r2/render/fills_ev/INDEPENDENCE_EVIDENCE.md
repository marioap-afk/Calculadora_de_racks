- **(a) Mutantes con efectos observables.** Se ejecutaron los 27 mutantes del instrumento. 25 se activan: cambian de forma observable `Facts`,
  `FactsSha256`, `Error`, `GitCalls` o `Inputs` en alguna orden. Trece se activan sobre los datos reales del fixture y los demás sobre los datos
  reales más entradas sintéticas o un repositorio sintético.
  - Los 25 activados reciben `REJECT_FACTS` del aceptante fijado con el mutante instalado como instrumento declarado, de modo que ni R1 ni R3
    pueden ser el detector. En 24 la detección es atribuible a la mutación; en M11 no lo es, porque es la misma señal de R2 que daría el error del
    instrumento fijado.
  - Todos los mutantes de hechos críticos (M01-M09, M12, M13, M16, M18, M20-M22 y M24: pertenencia de `Scope`, ascendencia e `Identity`, punto
    efectivo, MV-3, MV-4, MV-6, MV-7, blobs y hashes, cobertura) se detectan en el nivel de los hechos, por R5, R6 o R7.
  - M14 (texto libre) y M17 (palabra clave de esquema no admitida) solo se detectan en el nivel de la decisión (R4), y M11 (sin la comprobación de
    `-I`) solo por R2, a través de lo que informa el propio instrumento.
  - M15 y M23 no se pueden activar desde la línea de órdenes, porque ninguna entrada llega a su código: no se les atribuye detección. Su clase la
    muestran una sonda directa y la inyección de llamadas en un sobre, que R9 rechaza.
- **(b) `clausemap` sin oráculo circular.** Esperados derivados a mano y un instrumento de eco que imprime un sobre fijo; nada importa el
  aceptante. Sobre 12 mapas, uno válido y 11 con un defecto conocido cada uno:
  - el aceptante coincide con los esperados en 12 de 12, y el instrumento en 11 de 12;
  - la parte de mapa de R4 juzga `Authority` = pass como los esperados en 12 de 12;
  - las 17 perturbaciones de un campo se rechazan.
  - El mapa 11 (viola su esquema y conserva la forma) muestra una divergencia entre las dos piezas: residuo G2.
- **(c) R4 ejercida.** Sobre el escenario real y una entrega simulada, sin declarar nada no aplicable:
  - las tres salidas conformes se aceptan;
  - los 13 resultados invertidos se rechazan, cada uno solo en la fila invertida;
  - `not_run` cuenta como distinto de pass.
  - R4 lee la salida del Controller, la delegación, la entrega, la ruta ignorada, la revisión de los términos y `MainSha`. No lee los hechos del
    relevo ni los sobres del instrumento.
- **(d) Derivación del cierre.** Con la tabla de arriba, R9 `MATCH` dentro y `MISMATCH` fuera, sobre datos reales.
- **(e) Primitivas.** Comparten primitiva:
  - `ls-tree -r -z --full-tree`, para MV-3, los identificadores de MV-4 y los blobs;
  - `rev-parse`, para `HEAD`, la rama y la raíz.

  Ninguna de las dos piezas verifica los árboles. Los demás hechos críticos usan primitivas distintas, con la misma lectura de la especificación.
  Dos sondas confirman diversidad real de primitivas (encabezado sangrado; mensaje de commit con CRLF), y una tercera (valla `~~~`) muestra un modo
  común.
- **(f) Sin demostrar.**
  - La independencia de autoría está declarada, no observada: mismo worktree, y fe de erratas v0.2 escrita viendo las dos salidas.
  - Las dos piezas comparten la lectura de la especificación, el intérprete, el binario de Git y el clon.
  - R2 y R9 juzgan lo que el instrumento informa de sí mismo.
  - No hay ninguna salida real de un Controller.
  - La entrega es simulada; Ci, Tests, Remote, Routing, Termination y Denials no se evaluaron.
  - Solo se usaron los 27 mutantes fijos.
  - La ejecución fuera de la sesión de Principal no se demuestra aquí; sí, con datos reales, el lugar y el momento de la regla de A-6 en D.6
    (`r2/r1r4/`).

**Registro de las brechas residuales G1-G6** (decisiones §80, punto 3). Solo un cambio del instrumento o del aceptante cerraría cada una, con
otro blob, otra corrección de A-6 y otra determinación del Owner (A6-8, regla 5). La ronda 2 no cambia ninguna pieza. **Ninguna brecha queda
cerrada**, y una mitigación no ensayada se señala como tal.

- **G1. Palabra clave de esquema no admitida (MV-1).**
  - *Hecho no demostrado:* que el aceptante detecte, en el nivel de los hechos, una salida del instrumento que no falla cerrada ante una palabra
    clave de esquema no admitida.
  - *Riesgo:* aceptación de hechos de MV-1 falsos.
  - *Mitigación:* R4 siempre en `MATCH` (A6-5, regla 2), cuya parte de mapa detecta un `Authority` en pass con defectos.
  - *Cobertura real:* ensayada solo en la evidencia de independencia: con el mutante M17 y un Controller que decide pass, R4 rechaza. No ensayada
    con datos de la ronda ni con un Controller real.
  - *Blob:* cerrarla exige cambiar el aceptante (detectar la palabra clave por sí mismo); no se cambia.
  - *Efecto normativo:* MV-1 no es un hecho crítico de A6-9, regla 3. Su independencia en el nivel de los hechos no está demostrada.
- **G2. Mapa que viola su esquema.**
  - *Hecho no demostrado:* que las dos piezas coincidan en cuándo existen MV-2..MV-6 en un mapa que viola su esquema pero conserva la forma.
  - *Riesgo:* rechazo falso sistemático de hechos correctos. Las salidas quedarían sin aceptar, con STOP P-04 y FX-02 UNVERIFIED. No produce
    aceptaciones falsas.
  - *Mitigación:* prueba previa de los hechos sobre los datos de la ronda (A6-5, regla 6). **No ensayada**: el fixture B1 no existe. La precondición
    MV-1..MV-7 de A5-1 haría improbable ese mapa en B1, pero no está comprobado.
  - *Cobertura real:* la divergencia está demostrada en un mapa sintético (D11).
  - *Blob:* cerrarla exige cambiar una pieza y una fe de erratas de la especificación.
  - *Efecto normativo:* no afecta a lo que se acepta; puede impedir la ruta hacia C-24.
- **G3. Órdenes que el aceptante no recalcula.**
  - *Hecho no demostrado:* una recomputación independiente de `freetext`, `cleantree`, `trx`, `handoff`, `read-text`, `read-json` y la parte A5 de
    `contract`.
  - *Riesgo:* aceptación de hechos falsos de esas órdenes. Para `Tests`, `Handoff` y la parte A5 de `Contract` no hay ningún control del aceptante
    en el nivel de la decisión.
  - *Mitigación:*
    - R4 para `CleanTree` y `FreeText`;
    - contraste con la orden primaria (A6-4, regla 2);
    - oráculo sellado (A6-6).
  - *Cobertura real:*
    - R4 sobre `CleanTree` y `FreeText` está ensayada: el mutante M14 y las inversiones de resultado se rechazan.
    - El contraste primario y el oráculo **no están ensayados**: no hay Controller real ni ronda.
  - *Blob:* cerrarla exige cambiar el aceptante.
  - *Efecto normativo:* la independencia de A6-9, regla 3, se limita a los hechos críticos. Para `Tests`, `Handoff` y la parte A5 de `Contract`,
    la verificación descansa en el Controller y en el oráculo.
- **G4. Aislamiento y lecturas informados por el propio instrumento.**
  - *Hecho no demostrado:* que el aceptante observe por sí mismo el aislamiento y las llamadas a Git del instrumento.
  - *Riesgo:* un instrumento que ocultara llamadas o informara mal de `-I` pasaría R2 y R9.
  - *Mitigación:*
    - el blob fijado y la comprobación del punto de uso (A6-3, regla 3) garantizan que se ejecuta el código fijado, que registra sus llamadas;
    - la auditoría de la orden declarada, con `-I`, en el registro de sesión (A6-4, regla 3).
  - *Cobertura real:*
    - la comprobación del punto de uso está ensayada sobre datos reales (`r2/r1r4/`);
    - la auditoría del registro de sesión **no está ensayada** con un registro real de Codex.
  - *Blob:* cerrarla exige que el aceptante observe las llamadas por sí mismo, lo que exige otro aceptante.
  - *Efecto normativo:* la auditoría de lecturas (P-22) depende de lo que declara el instrumento fijado, con la cobertura declarada en A6-4,
    regla 4.
- **G5. Parte de mapa de R4 de un solo lado.**
  - *Hecho no demostrado:* la detección de un `Authority` en fail falso con un mapa válido.
  - *Riesgo:* aceptación de un STOP falso, que detiene la ronda sin motivo.
  - *Mitigación:* oráculo sellado: una salida aceptada distinta del oráculo es FAIL (A6-5, regla 5). **No ensayada**: no hay oráculo ni ronda.
  - *Cobertura real:* la asimetría está demostrada con el mapa válido V0.
  - *Blob:* cerrarla exige cambiar el aceptante.
  - *Efecto normativo:* el error va en la dirección conservadora: detiene, nunca da por buena una entrega, y no permite un VERIFIED falso.
- **G6. Encabezado sangrado y mensaje de commit con CRLF.**
  - *Hecho no demostrado:* una lectura común de esas dos construcciones.
  - *Riesgo:* rechazo falso.
  - *Mitigación:* prueba previa de los hechos sobre los datos de la ronda (A6-5, regla 6). **No ensayada.**
  - *Cobertura real:* la divergencia está demostrada con sondas sintéticas.
  - *Blob:* cerrarla exige cambiar una pieza y una fe de erratas de la especificación.
  - *Efecto normativo:* no afecta a lo que se acepta; puede impedir la ruta hacia C-24.
