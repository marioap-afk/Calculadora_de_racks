# Disposicion del Owner D1 (F4B-E11) para la verificacion de F4B-PBTC2-DECLARED-CORRECTIONS, intento 1

Fuente literal: owner-d1-d2-decisions.md (copia en la carpeta de la corrida del Worker R20261118T113927Z-037c; se custodia en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/ al cerrar).

Hechos:
- La primera invocacion del Worker de la delegacion R20261118T055036Z-c73e (WorkRunId R20261118T055535Z-38d3) paro en el paso 1 (medicion previa de D-PBTC2-07) por PBTC2-C14: fila T2-3, BOM de 15 lineas con el valor exacto frente a 14 con el redondeado (postes 150 x32 y 150.0004 x4 frente a 150 x36). Sin commits ni push.
- El Owner decidio (D1, literal): "CONTINUE WITH THE FIX. Ratify exact-value preservation." ... "Do NOT reintroduce rounding merely to preserve the previous BOM grouping." ... "resume/relaunch the same correction work WITHOUT consuming a product attempt. Preserve the stop and Owner decision in evidence. No new planning is required unless the contract itself changes."
- La segunda invocacion (R20261118T113927Z-037c) repitio la medicion (m8b) con resultado identico linea a linea al de la primera y continuo con el RED y el GREEN.

Alcance de esta disposicion para la verificacion:
- Es autoridad para PBTC2-C14 en este intento: el efecto de T2-3 medido (identico al de la primera invocacion) NO es una parada; cualquier otra diferencia si lo seria.
- La existencia de dos invocaciones del Worker para la misma delegacion es la reanudacion ordenada por el Owner y no consume attempts (Attempt sigue en 1); la entrega evaluada es la de R20261118T113927Z-037c.
- No cambia ningun otro invariante ni parada del contrato.
