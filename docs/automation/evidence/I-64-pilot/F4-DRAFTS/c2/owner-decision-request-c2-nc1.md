# Decisión del Owner: el control nc1 de F4-T4-HOST-C2

## Dónde está C2

Todo lo demás de la cadena está en verde:
- **el producto:** RED `fe165a63` y GREEN `9abc1320`, con exactamente las dos líneas fijadas en `LateralHeaderDrawer.cs` y la guarda nueva de 4 casos;
- **la CI exacta:** GREEN 4/4. En el RED fallan solo 3 casos de la guarda nueva, por aserción;
- **la verificación del Controller:** EXECUTION_VERIFIED, 14/14;
- **las revisiones:** la directa, 0 B / 0 I / 3 m, y la independiente, 0 B / 0 I / 3 m, sin hallazgos para ti;
- **nc2 y nc3:** CONTROL PASS ordinario.

Falta solo nc1, y la regla D2 no me deja cerrarlo yo.

## Qué pasó en nc1

nc1 le da al Controller una copia de la entrega con un solo cambio: un `CurrentSha` falso (`deadbeef…`). Lo comprobé: la copia difiere de la real solo en ese campo. El Controller debe detectarlo, y lo detectó:
- `Identity` en fallo, con el SHA falso y el HEAD real;
- `Handoff` en fallo primero, por el propio SHA falso;
- resultado BLOCKED / STOP.

Frente a la verificación real difieren seis comprobaciones:

| Comprobación | ¿Prevista? | Por qué falla |
|---|---|---|
| Handoff, Identity, Remote, Ci, Tests | Sí. Son los cinco «consumidores del `CurrentSha`» que admite el sustituto ya autorizado, desde su origen | Comparan con el SHA falso y lo rechazan |
| **Trailer** | **No** | El Controller buscó un campo `Trailer` en la raíz de la entrega. Está en `Worker.Trailer`, igual en la copia y en la real. En la verificación real lo leyó bien: es un error de lectura del Controller, ajeno a la mutación |

## Por qué no lo cerré

- **La firma no es la exacta.** La de PD-F4-04 que fijaste (`owner-f4t3-authorization.md`) exige «all other intended checks match», y Trailer no coincide.
- **El sustituto autorizado tampoco pasa.** Lo ensayé en seco, en una carpeta aparte y sin aplicarlo: da **FAIL** solo por su propiedad 6 («solo difieren los consumidores del `CurrentSha`»), por Trailer. Las otras seis propiedades pasan y su autoprueba es determinista.
- **Tu regla:** sustituto solo con la firma EXACTA, sin generalizar por el nombre de la comprobación. La regla D2 manda volver a ti ante un modo de fallo nuevo.

Así que no apliqué nada ni repetí nc1. La salida ordinaria y su registro de relevo se conservan.

## Lo que necesito que decidas

**¿Cómo se resuelve nc1 en C2?**
- **(A) [recomendada]:** un sustituto con UNA excepción, solo para C2: el sustituto de C1 clonado, más una regla determinista para Trailer.
  - **La regla:** Trailer puede diferir solo si (i) el `Worker.Trailer` de la copia es idéntico al de la entrega real; (ii) la verificación real dio Trailer en verde; y (iii) los dos commits llevan ese trailer. Todo lo demás del sustituto no cambia.
  - El error de lectura se registra como deuda nueva, **PD-F4-08**, y la salida ordinaria se conserva.
  - Después: cierre por D2, el DLL nuevo y Smoke-2 contigo.
- **(B):** repetir nc1 una vez con la misma mutación.
  - Si da la firma exacta, el sustituto de siempre; si da CONTROL PASS, el ordinario; si repite esto, vuelve a ti.
  - Unos 5 minutos, pero es repetir hasta que salga una corrida que valga.
- **(C):** no cerrar C2 por ahora. Se custodia como está, y Smoke-2 se reanuda con el DLL de C1 y S2-03 declarado.

**Si (A): ¿vale la excepción de Trailer para las tareas siguientes de F4?**
- **No [recomendado]:** solo C2. Si vuelve a pasar, vuelvo a ti.
- **Sí:** a partir de ahora, con la misma regla determinista.

Basta con contestar, por ejemplo: «A, solo C2».
