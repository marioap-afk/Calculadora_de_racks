> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v4-delta: los hallazgos de la recomprobación de la v3, uno a uno

**Qué es:** para cada hallazgo de la recomprobación de la v3 (`independent-review-v1.md`, §11: NECESITA v4, B/I/M 0/1/1), el cambio de la v4, con su id del registro de cambios (V4-xx). Todo se leyó por objetos git desde el checkout principal, en `833ea6bf`; el worktree no se tocó.

**Resultado:**
- **los dos hallazgos se aplican:** O-01 (V4-01) y O-02 (V4-02);
- **el diseño no cambia,** ni ningún CT, ni ningún contrato fuera de la letra de E-1, E-4 y E-5;
- **ninguna decisión material nueva.** D-1 sigue siendo la única, con los números corregidos;
- `pbt3-measurements.md` no cambia.

| Hallazgo | Grav. | ¿Se aplica? | Cambio | Id | Dónde |
|---|---|---|---|---|---|
| **O-01** Los números de PB-T4 en D-1 no son exactos | I | Sí, con la letra estricta | **La letra:** E-1 (con (A′)), E-4 (con (A′)) y E-5 exigen que existan todos los miembros que nombran, como las pruebas que sustituyen: S-1 busca el `ComposeMemberEnvelope` de cuatro parámetros (PrepareGuard:107), S-4 el `Post` del piloto (UpdateGuard:140) y S-5 el cuerpo de la `Execute` del piloto (IntegrationGuard:881).<br><br>**La cuenta,** con E-2 y E-6, que caen con E-1 y E-5:<br>• (A): seis ahora y **tres** en PB-T4 (E-3, E-5 y E-6);<br>• (A′): tres ahora y **seis** en PB-T4 (S-3, S-5, S-6, E-1, E-2 y E-4);<br>• **nueve** en las dos.<br><br>Se corrigen la cabecera, las dos tablas (D-PBT3-09b y §8.2), la pregunta, las dos opciones y A-T3-1. La cuenta de (A) supone que PB-T4 conserva `BuildCore` y `Execution<TState>` | V4-01 | §5.3; D-PBT3-09b; §8.2; cabecera; §10 |
| **O-01**, la lectura laxa | — | **Se analiza; no es una opción más** | Con equivalentes «sobre cada uno que exista, y al menos uno», PB-T4 sustituiría solo E-3 con (A) (**siete** en total) y solo S-3, S-5 y S-6 con (A′) (**seis**): (A′) ahorraría una. Es posible, porque en PB-T3b no pasarían en vacío. Pero cubrirían **menos** que las pruebas que sustituyen, porque ya no exigirían el código del piloto: sería debilitar la guarda vieja, y eso solo cabe con un sí explícito del Owner ([OD]). Por eso el paquete fija la letra estricta, y la descripción de la opción (A′) dice la cuenta laxa en una frase. Si el Owner la pidiera, la versión que se emita la escribe en E-1, E-4 y E-5, sin otra decisión | V4-01 | §8.2 |
| O-02 «Ocho en total» se lee como el total de F4B | M | Sí | «En total, por D-1» en las dos tablas, y «Nueve por esta decisión» en las opciones. Una frase en D-PBT3-09b, en §8.2 y en la cabecera: el retiro del piloto en PB-T4 (A-T3-1) trae además las suyas, iguales en las dos | V4-02 | D-PBT3-09b; §8.2; cabecera |

## Lo que no cambia

- Lo que se sustituye ahora, en PB-T3b: **seis con (A) y tres con (A′)**, exacto según la recomprobación (§11.3).
- Las opciones son reales y completas: las mezclas dan cuatro o cinco ahora, y quedan dominadas (§11.3).
- La recomendación de (A) se sostiene con la letra estricta, que es la del contrato: nueve y nueve, y una sola implementación de la escritura.

## D-1, la pregunta lista para AskUserQuestion

- **header** (10 caracteres): `Guardas F4`
- **question** (373 caracteres, contados con `count_question.py` sobre el paquete): «PB-T3b: para que el «Actualizar» del panel renombre y borre vistas, hay que sustituir pruebas de F4 que fijan el texto del escritor del piloto, cada una con registro y una prueba equivalente. Con un solo código para el piloto y el borrador completo son seis ahora y tres en PB-T4; con el borrador completo en código duplicado hasta PB-T4, tres ahora y seis en PB-T4. ¿Cuál?»
- **options** (la recomendada primero):

  | label | description |
  |---|---|
  | `Un solo código (Recomendado)` | Un único algoritmo de escritura para el piloto y el borrador completo. PB-T3b sustituye seis pruebas de F4 (cinco fallan y una quedaría vacía), cada una con su equivalente; al quitar el piloto, PB-T4 sustituirá tres de esos equivalentes. Nueve por esta decisión. |
  | `Código duplicado hasta PB-T4` | El texto del piloto no cambia, salvo la barrera, y el borrador completo va en código propio, copiado, hasta PB-T4. PB-T3b sustituye tres; al quitar el piloto, PB-T4 sustituirá seis (tres pruebas de F4 y tres equivalentes). Nueve por esta decisión. Solo con equivalentes más laxos, que no exijan el código del piloto, serían seis frente a siete. |
  | `Sin renombrar ni borrar` | No se sustituye ninguna. El panel no renombra ni borra vistas en PB-T3: esas dos cosas, que pide la fila de PB-T3, quedan pendientes. |
- **multiSelect:** no.
