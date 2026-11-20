# I-64 F4B PB-T3b — Los cambios del generador en su sitio (r1 → r2 → final)

**Por qué esta nota.** `make_pbt3b_contract.py` se cambió dos veces en su sitio después del seco que leyó la primera ronda de CA-T3b, y no queda copia de las versiones anteriores, ni en la carpeta de la sesión ni en git. CA-T3b lo señaló como observación en su §12 («el generador se cambió en su sitio y su versión anterior […] ya no existe en ninguna parte»). Esta nota dice qué cambió y cómo se comprobó por su comportamiento.

**Cómo se arma el generador.** `make_pbt3b_contract.py` sale de `build_t3b.py`: une los fragmentos nuevos de PB-T3b con bloques copiados tal cual de `make_pbt3a_contract.py` (el final de PB-T3a, sha256 `30595efd…`), y comprueba cada bloque por su primera y su última línea. Los fragmentos se editaron en su sitio, igual que el generador; tampoco queda copia de sus versiones anteriores.

## Las tres versiones

| Versión | Generador | Seco | Contrato en seco |
|---|---|---|---|
| r1 (la primera ronda de CA-T3b) | 141 096 bytes, sha256 `B26527B4…` | `dry-pbt3b.txt` de entonces, 14 AVISOS | 72 423 bytes, `5EE38DAB…`, 62 autoridades |
| r2 (la §12 de CA-T3b) | 142 687 bytes, sha256 `D9D82540…` | `dry-pbt3b-r2.txt`, 14 AVISOS | 73 150 bytes, `395DCBB9…`, 64 autoridades |
| final (la emisión) | 142 917 bytes, sha256 `F0CD5F96…` | `dry-pbt3b.txt`, 12 AVISOS, con `--bounded-file` | 73 249 bytes, `DA96B4B9…`, 64 autoridades |

## r1 → r2: el SHA del sustituto de nc2 v2 y su custodia (la sesión, tras su revisión APTO)

Solo esto, en cuatro fragmentos:
- **las constantes:** `NC2_V2_SHA` pasa del marcador `None` al SHA revisado `c033e96c…8e45`. Entran `NC2_DIR`, `NC2_V2_REVIEW` y `NC2_V2_LIMITS`, la carpeta y las dos rutas de la custodia;
- **la comprobación:** `NC2_V2_OK` exige en la revisión el script, su revisión y sus límites. Si el script está, su sha256 tiene que ser `NC2_V2_SHA`, y la revisión y los límites tienen que nombrarlo. El AVISO dice qué falta;
- **las autoridades:** `PENDING_AUTH` gana los límites y la revisión del sustituto, como los del sustituto de nc1 (M-04 de CA-T3b);
- **la salida en seco:** una línea dice que el SHA está fijado y que falta la custodia.

**Lo que cambia el contrato** (`delta-r2.txt`): las autoridades, de 62 a 64, con el SHA en la del script; y en `PBT3B-C11`, el marcador del SHA por el SHA. Nada más.

## r2 → final: `BOUNDED_RESULT` (la sesión, tras CA-T3b)

Solo la constante `BOUNDED_RESULT`, que pasa del marcador `None` al literal de CA-T3b, con su comentario, que gana una línea:

`EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1`

**Lo que cambia el contrato** (`delta-final.txt`): solo `IssuedBy`, donde el literal sustituye a `<resultado por rellenar>`. En el seco, con el informe de CA-T3b (`--bounded-file`), `bounded_check` corre sin errores y desaparecen los dos AVISOS de la comprobación acotada: de 14 a 12.

## Cómo se comprobó por su comportamiento

- **La §12 de CA-T3b** (12.2 y 12.4), sobre el r2:
  - el delta de los dos secos, hecho por el revisor sin `delta-r2.txt` (`cat3b_r2.py`), es el mismo;
  - las marcas que leyó del generador r1 siguen en su línea: `BOUNDED_RESULT` (:62), `BOUNDED_CHECK` (:1507), `bounded_check` (:1586-1661) y su llamada con `reviewer='CA-T3b'` (:1680); `NC2_V2_SHA` (:82) lleva el SHA;
  - `bounded_check` es, línea a línea, el de `make_pbt3a_contract.py`;
  - las pruebas en negativo pasan de 68 a 69, todas como se esperaba: G02 pasa de «puesto» a «alterado», y G09, el marcador sin rellenar, es nueva.
- **En el final,** el comentario de `BOUNDED_RESULT` gana una línea, así que esas marcas bajan una: :63, :1508, :1587-1662, :1681 y `NC2_V2_SHA` en :83.
- **Las pruebas en negativo del final** (`neg-pbt3b.txt`): 71 de 71. Cambian los casos de `BOUNDED_RESULT`:
  - G01: LISTO PARA EMITIR, error;
  - G06: el marcador, un AVISO más;
  - G07: I 1 frente al informe, error;
  - G10: sin la recomprobación de la §12, error;
  - N12: sin `--bounded-file`, un AVISO más.
- **El contrato final y el r2** solo difieren en `IssuedBy`; **el r2 y el r1**, solo en las autoridades y en C11, como dicen los deltas.
