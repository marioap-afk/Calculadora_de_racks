# I-58 V3 — diagnostico del fallback vigente

Ejecutar `dotnet run --project docs/automation/evidence/I-58-v3-probe/I58.V3.Probe.csproj`
solo DESPUES de commit del harness, con arbol limpio antes/despues. Conservar salida, SDK,
SHA y duracion fuera del checkout durante la corrida. La seleccion debe ser mayor que cero.

Usa catalogos versionados copiados por build y resolvers/store reales. El transform local
`Proposed` solo caracteriza la propuesta F-08; no compara hermanas ni implementa AUTH-13.
La copia DTO es una comodidad de fixture conocida, NO un materializador productivo acreditado.
Custom+null se observa deliberadamente aunque V3 lo rechace antes del mapper. En composite
el mapper puede reparar ese caso; no se acredita como accepted-authored ni prueba de provenance.
El seed composite usa Snapshot SOLO para construir topologia de fixture; el global de entrada
se restablece expresamente. Snapshot no se propone para materializar Single.

Matrices: global 0/8; calc/custom; null/0/7/9; orden [7,9]/[9,7]/[0,7]/[7,0]
con cuatro mezclas de procedencia; legacy global ausente/null; controles negativos V2;
PushBack compuesto con estructura y ambos lados locales. Registra cada peralte observado.
No prueba todos los campos del futuro materializador ni sibling permutations ni scanner:
esas obligaciones permanecen en CT58-25..32/F1, que sigue cerrado.
