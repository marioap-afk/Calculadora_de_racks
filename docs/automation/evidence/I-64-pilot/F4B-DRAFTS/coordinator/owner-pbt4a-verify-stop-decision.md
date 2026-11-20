# Decisión del Owner sobre la parada de la verificación de PB-T4a (I-64 F4B)

**Fecha:** 2026-11-20.

**Qué pasó:**
- **La verificación del Controller** `R20261120T211710Z-a468` de F4B-PBT4-FULL-READ-PATH dio EXECUTION_BLOCKED / STOP, FailureClass Authority, con S-12, y 13 de sus 14 comprobaciones en verde.
- **Su evidencia de Authority:** `git diff --name-only AuthorityRevision BaseSha` (27c13fab..5aaacce3) da cuatro rutas bajo `docs/`. Una es el generador del contrato, `F4B-PBT4-FULL-READ-PATH/coordinator/make_pbt4a_contract.py`, que no figura en Authorities, y el Controller dijo que esa ruta «cannot be classified under 16.3».
- **Por qué es una lectura más estricta, no un defecto de la entrega:** la regla 16.3 solo prohíbe en ese diff rutas EXTERNAL o con secciones UNIT_CHANGE, y el generador no es una autoridad. El mismo hecho, con el mismo texto en el prompt de la verificación («(no figura en Authorities)»), pasó las verificaciones de PB-T2a a PB-T3c.
- **Antes,** la verificación `R20261120T205531Z-611f` no llegó a arrancar: su prompt superaba el límite de la línea de órdenes de Windows. Es un lanzamiento no productivo, sin invocación del modelo.

La sesión preguntó al Owner con AskUserQuestion, porque una verificación en STOP no se repite sin él. Hay precedente: en PB-T1b, «Regla F4B y reverificar». Este registro copia el texto exacto de la pregunta y la respuesta.

## Pregunta (encabezado «Verify T4a»)

**Pregunta, literal:**

> La verificación del Controller de PB-T4a paró en STOP (Authority, S-12), con 13 de sus 14 comprobaciones en verde. El motivo: el commit de emisión custodia el generador del contrato (make_pbt4a_contract.py), que no figura en la lista de autoridades del contrato, y el Controller dice que «no puede clasificarlo». La regla 16.3 solo prohíbe en ese diff rutas EXTERNAL o con secciones UNIT_CHANGE, y el generador no es una autoridad. Exactamente el mismo hecho pasó las verificaciones de PB-T2a a PB-T3c. El código de PB-T4a no tiene ningún hallazgo: la CI da 4/4 y la revisión independiente 0/0/7. ¿Qué hago?

**Opciones:**
1. **Reverificar con aclaración (Recomendado):** Lanzo una verificación nueva con una frase más en su prompt: una ruta del diff de emisión que no está en Authorities no es EXTERNAL ni UNIT_CHANGE y la regla 16.3 la admite, como en PB-T2a a PB-T3c. La parada se custodia tal cual. Si la nueva vuelve a parar, te vuelve.
2. **Reverificar sin cambios:** Lanzo una verificación nueva con el mismo prompt, confiando en que otra ejecución del Controller lo lea como las anteriores. Es más fiel al procedimiento, pero puede volver a parar por lo mismo.
3. **Parar PB-T4a aquí:** La parada queda como resultado de la verificación y PB-T4a no se cierra. Habría que decidir después cómo seguir.

**Respuesta del Owner, literal:** «Reverificar con aclaración (Recomendado)».

## Lo que queda decidido

- **Una verificación nueva** de F4B-PBT4-FULL-READ-PATH, con la misma delegación (`a53f`) y la misma entrega (`de54`).
- **Su prompt lleva una aclaración,** como disposición del Coordinator (`inputs/coordinator-disposition.md`): una ruta del diff `AuthorityRevision..BaseSha` que no figura en Authorities no pertenece a ninguna clase, no es EXTERNAL ni UNIT_CHANGE, y 16.3 la admite. El Controller lo sigue comprobando él mismo con git.
- **La parada `a468`** se custodia tal cual, como unidad propia.
- **Si la verificación nueva vuelve a parar,** vuelve al Owner.

## Segunda vuelta: la reverificación `R20261120T212638Z-de08` también paró

**Qué pasó:**
- **Authority pasó,** y 13 de las 14 comprobaciones dieron verde.
- **Tests dio STOP con PBT4A-C07.** Lo que midió el Controller cuadra entero: RED Core 19 137 = 19 125 + 5 Failed + 7, GREEN Core 19 137 = 19 130 + 0 + 7, UI 2 451 = 2 430 + 21 en los dos, y las 38 filas de `RequiredTests` iguales a las de la delegación y el contrato.
- **Por qué paró:** por una frase de su prompt, heredada de todas las cadenas de F4B: «Una linea esperada que no se cumple en el GREEN, PBT4A-C07.». Quería decir «si una línea esperada no se cumple, es PBT4A-C07», y el Controller la leyó como si la sesión le avisara de que una no se cumple. En la verificación de PB-T3c, el mismo texto se leyó bien.
- **El resultado de las dos verificaciones:** entre las dos, las 14 comprobaciones dieron verde al menos una vez, y ninguna falló por la entrega.

**Pregunta (encabezado «Verify T4a»), literal:**

> La verificación nueva de PB-T4a pasó Authority, pero volvió a parar (STOP), ahora en Tests con PBT4A-C07. Lo mismo que midió cuadra entero: el RED y el GREEN dan exactamente los totales del contrato y las 38 filas coinciden. Paró por una frase de su prompt, heredada de todas las cadenas: «Una línea esperada que no se cumple en el GREEN, PBT4A-C07». Quería decir «si una línea esperada no se cumple, PBT4A-C07», y el Controller la leyó como un aviso de la sesión de que una no se cumple. En PB-T3c el mismo texto se leyó bien. Entre las dos verificaciones, las 14 comprobaciones han dado verde al menos una vez, y ninguna falló por la entrega. ¿Qué hago?

**Opciones:**
1. **Corregir la frase y reverificar (Recomendado):** Reescribo la frase como condición explícita («si alguna línea esperada no se cumple en el GREEN, es PBT4A-C07; la sesión no te informa de ninguna: compruébalo tú»), la corrijo también en la cadena de PB-T4b y lanzo una tercera verificación, con la aclaración de 16.3. Las dos paradas se custodian tal cual. Si vuelve a parar, te vuelve.
2. **Aceptar por disposición:** Doy la verificación por satisfecha por tu disposición, con las dos corridas: cada comprobación ha pasado en al menos una, y las dos paradas vienen de frases del prompt. Ahorra una corrida, pero PB-T4a cierra sin una verificación limpia de punta a punta.
3. **Parar PB-T4a aquí:** Las paradas quedan como resultado y PB-T4a no se cierra. Habría que decidir después cómo seguir.

**Respuesta del Owner, literal:** «Corregir la frase y reverificar (Recomendado)».

**Lo que queda decidido:**
- **La frase se reescribe como condición explícita** en `make_pbt4a_verify.py` y en `make_pbt4b_verify.py`: «Regla condicional, no un hecho: SI al comprobarlo tú mismo alguna línea esperada no se cumple en el GREEN, es PBT4A-C07; la sesión NO te informa de ninguna línea esperada incumplida, compruébalo tú en los TRX y en el texto.»
- **Una tercera verificación,** con la misma delegación (`a53f`) y la misma entrega (`de54`), con la frase corregida y la aclaración de 16.3.
- **Las paradas `a468` y `de08`** se custodian tal cual, como unidades propias.
- **Si vuelve a parar,** vuelve al Owner.
