# FX-04a B2 — informe de la supervisión para el Coordinator de I-62 (U-01 de la ronda única)

Registro: evidencia §78-§79. Ningún valor de este informe se pega en una sesión del fixture.

| Punto | Hecho |
|---|---|
| Lanzamiento | B2 `local_97ac0196…`, abierta por el Owner a las 20:06:01Z en `D:\r62-fixture\B2` (`cabed547`), `claude-opus-5-5`, `xhigh` |
| Desviación | **KICKOFF_SCHEMA_NOT_DELIVERED**, causada por la supervisión. La copia de la tarjeta de la mañana en el chat sustituía el esquema por un marcador, y el Owner pegó esa copia. B2 recibió la instrucción, pero no `response.v2.schema.json` (mensaje `fcfdfda7…`, 1 394 caracteres, frente a `B2-kickoff.md` `233b0582…`, 8 065 bytes). La parte que faltaba estaba preparada (`9ec1be82…`), pero no se pegó |
| Terminación | acreditada: `isRunning` = false a las 20:58:59Z y a las 21:00:04Z; última actividad a las 20:18:03Z |
| Preflight | `P20261007T201013Z-25e8`, RESUME_DECISION, MATCH / ELIGIBLE |
| Aislamiento (D.6) | no UNVERIFIED: B2 solo usó `get_session("self")`, leyó solo el clon y su scratchpad, escribió solo en su scratchpad y dejó el clon sin cambios; cobertura declarada |
| Respuesta | SHA-256 canónico `74e69f35b7c3fcfb0bcdac63da98339d25c58bf07f35c24402b798463d009587`, durable en `4ea66233` antes de cargar el oráculo |
| Oráculo v2 | `5e7a78d3…` (archivo `ae384682…`), igual al publicado en `707b4daa` |
| Resultado mecánico (contrato v2) | **FAIL bruto, 2/23** (`facts.branch` y `facts.claim_id`); 20 campos ausentes y `facts.protocol` con otra forma. Sin normalización posterior ni tercer oráculo |
| Causa | sin el esquema, B2 no conocía las claves ni los formatos del contrato y usó una estructura propia; el resultado mide la entrega, no la reconstrucción |
| Propuesta de la supervisión (no es una clasificación) | B2 = **INVALID_LAUNCH**; FX-04a y C-25a = **UNVERIFIED**. No PASS (la igualdad es 2/23). No FAIL (la diferencia no prueba una reconstrucción errónea) |
| Observación de B2 | declara `UNKNOWN` que conste acreditada, en el estado canónico del fixture, la terminación de la sesión R; la acreditación existe en la evidencia de RackCad |

**Decisiones pedidas (U-01):**
- **U-01a:** clasificación de B2 y de FX-04a/C-25a.
- **U-01b:** ¿se autoriza una tercera sesión de Principal B (B3), por encima del tope de 2 de D.3? Sería con el mismo clon, el mismo texto inicial y el oráculo v2 ya publicado. Control de entrega: el Owner pega desde el archivo y la supervisión comprueba el SHA-256 del primer mensaje antes de aceptar la corrida.
- **U-01c:** si no hay B3, ¿queda FX-04a cerrada como UNVERIFIED con limitación para OV? Con eso se liberan las escrituras de FX-02 en el origen y OD-2d-PROBE.

Mientras no se decida U-01, no hay escrituras en el origen del fixture ni OD-2d-PROBE (decisiones §53).
