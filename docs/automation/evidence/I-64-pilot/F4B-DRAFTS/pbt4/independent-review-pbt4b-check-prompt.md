# Encargo de la comprobación acotada de PB-T4b (CA-T4b) (enviado por la sesión al revisor del paquete de PB-T4 como mensaje; texto exacto)

## 1. CA-T4b, sobre el contrato en seco y la v4.8 (2026-11-21T03:00:05.294Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T4b (CA-T4b)** sobre la v4.8 del paquete de PB-T4, como §20 de tu `independent-review-v1.md`. Las reglas de lectura son las de siempre: solo objetos git en `78a7cef3` (el cierre de PB-T4a), nada en el worktree, sin compilar, Python con `-I` y guiones `ir4_`. Las medidas ya las hizo el autor en copias exportadas; tú las compruebas por texto y por sus salidas.

**Las entradas** (en `hs65\pbt4\`, salvo que diga otra cosa):
- `I-64-f4b-pbt4-gate-contract-package-v48.md` (`6ddc239d…`), con las tablas V46-01..07, V47-01..05 y V48-nn, y el anexo V48-A. Las v4.6 y v4.7 son historia: compáralas solo para ver los deltas desde la v4.5, que es la que revisaste en tu §19.
- `session-pbt4b-design-decisions.md`: las decisiones de la sesión D-S1 a D-S3, D-S5, D-S6 y D-S1 aplicada a dos copias de WPP. D-S4 está en el texto del paquete.
- **Las decisiones del Owner:** `owner-pbt4b-list-477-decision.md` y la nueva `owner-pbt4b-list-478-decision.md`.
- `v47\analysis-outside-477.md`: el análisis medido de las dos pruebas que cayeron fuera de la lista y de las dos copias de WPP.
- `pbt4b-substitution-list-478.tsv` (`8b0fcd0c…`), `pbt4b-copy-map-v5.tsv` (`bda79bb7…`), `pbt4b_hand_edits_v48.py`, `pbt4b-equivalents-review-v48.txt` y los guiones `*_v48.py`.
- `hs65\pbt4bcc\compile\compile-check.md` (`25103dba…`): la precondición 6 de la v4.8, pasos 1 a 7, con sus salidas `out-*-v48-r4q`, `-r4r` y `-r4s`.
- `v48\session-cost-rerun-rule.md`: la regla de la tercera corrida del coste, fijada antes de medirla.

**Qué comprobar:**
1. **Los menores M-07 a M-10 de tu §19:** ¿están aplicados o declarados como pediste?
2. **D-S5** (las cuatro `Theory` partidas por filas):
   - ¿cada mitad tiene el mismo cuerpo y las mismas aserciones del original?;
   - ¿los miembros de datos nuevos reparten las filas sin cambiar ninguna?;
   - ¿la regla ampliada de la partición sigue cerrada (las demás causas paran)?;
   - ¿CT-20 lo fija?
3. **D-S6** (el espacio raíz y el prefijo `FullPanelCopy`):
   - ¿algún filtro de los `RequiredTests` que no sea de las copias selecciona alguna copia?;
   - ¿alguna clase existente choca con el prefijo?;
   - ¿la guarda de espacios de nombres queda cumplida por texto?
4. **Las dos copias de WPP:** ¿sus equivalentes afirman lo que fijan D-PB-04, D-PBT3-09 (e) y D-PBT4-07, sin perder nada que el original protegiera y no dependa del cambio?
5. **La guarda de WHHG** (la sexta tanda): ¿su equivalente es la misma tabla con las tres cuentas 0, 6 y 0, y nada más? ¿Las cuentas salen del GREEN literal del paquete?
6. **La lista de 478 y las cifras:**
   - ¿la lista es la de 477 más esa guarda, byte a byte en el resto?;
   - ¿cuadran los totales del RED y del GREEN, R_C = 291, R_UI = 0, `MinSelected` y la fila n.º 12 con lo medido en la precondición 6?
7. **El paseo de guardas corregido:**
   - ¿la corrección de la expresión de los campos es solo esa?;
   - ¿las dos pruebas que la sesión dio por leídas (`WorkspaceCommandAdvisorGuardTests.TheCatalogBody…` y `WorkspaceHostHardeningGuardTests.TheGuardOfTheLiteralsTrips…`) están bien leídas? Pasan en el GREEN completo; quiero tu lectura de por qué el paseo las marca.
8. **El coste:** no lo decides tú; lo decidirá el Owner con las tres corridas. Dime solo si la regla de `session-cost-rerun-rule.md` es neutral y si ves alguna causa en el código del GREEN que explique el caso `pb-20-ab x4 vistas +200 racks ajenos`, `origen-es-Primary` de la primera corrida (98,2 ms frente a 65,2), o si es compatible con el ruido.
9. **¿Hay alguna decisión nueva para el Owner** aparte del coste? ¿Algo que cambie la lista, o una prueba que pueda caer fuera de las 478 que la precondición 6 no haya corrido? Por ejemplo, la UI fuera de los 15 filtros.

**Formato:** §20 con hallazgos B/I/M (archivo:línea), un veredicto (EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE) y la línea BOUNDED como en las otras rondas. **Si el entorno no te deja escribir el archivo,** me pasas la §20 entera en tu respuesta y la guardo yo.

### 1.1. Complemento enviado durante esta ronda (2026-11-21T03:11:58.801Z)

Mensaje de la sesión, para tu §20. Al registro de la sesión (`hs65\pbt4\session-pbt4b-design-decisions.md`) le añadí dos secciones mientras empezabas:
- **D-S4:** solo un resumen; su texto normativo sigue siendo D-PBT4-21 del paquete;
- **la lectura de la sesión de las dos pruebas del paseo,** la del punto 7 de tu encargo.

No cambia nada más. Léelo en su versión actual. Si ves alguna diferencia entre esas secciones y el paquete v4.8, inclúyela en la §20.
