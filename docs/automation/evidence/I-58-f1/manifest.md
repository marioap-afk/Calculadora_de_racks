# I-58 F1 — manifiesto CT/MM

Cada ID de fixture remite a `matrix.json`: teoria, kind, expectativa, estado F1 y SHA-256 del carrier original.
El hash identifica evidencia; nunca decide igualdad authored. `row-mapping.json` contiene el mapping exhaustivo.

| Fila | Casos mapeados | Disposicion |
|---|---:|---|
| CT58-01 | 16 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-02 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-03 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-04 | 8 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-05 | 28 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-06 | 13 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-07 | 13 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-08 | 74 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-09 | 40 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-10 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-11 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-12 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-13 | 8 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-14 | 20 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-15 | 36 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-16 | 78 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-17 | 24 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-18 | 12 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-19 | 40 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-20 | 43 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-21 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-22 | 4 | Impacto heredado + Cama/Marker; sin cambiar expected de I-57 |
| CT58-23 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-24 | 18 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-25 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-26 | 262 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-27 | 266 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-28 | 261 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-29 | 280 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-30 | 250 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-31 | 240 | Baseline verde / oracle futuro activo solo mediante patch RED |
| CT58-32 | 218 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C01 | 5 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C02 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C03 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C04 | 7 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C05 | 3 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C06 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C07 | 6 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-C08 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D01 | 10 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D02 | 8 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D03 | 6 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D04 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D05 | 6 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D06 | 10 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D07 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D08 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D09a | 8 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D09b | 70 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D09c | 16 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D09d | 8 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D09e | 30 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-D10 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H01 | 6 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H02 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H03 | 1 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H04 | 7 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H05 | 5 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H06 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H07 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-H08 | 3 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P01 | 1 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P02 | 3 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P03 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P04 | 4 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P05 | 6 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P06 | 5 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P07 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |
| MM-P08 | 2 | Baseline verde / oracle futuro activo solo mediante patch RED |

CT58-25: cuatro tipos publicos concretos y autoridades reales en `I58F1FutureOracles`; AUTH10 recibe el resultado tipado y un builder test-only (plan vacio para tres kinds, plan real del assembler Cantilever). Prueba la frontera y el conteo; no implementa wiring ni nuevos builders productivos.
CT58-26/28: cada payload mutado divergente de dos hermanas tiene ademas par igual `materialized-mutant`, para inspeccionar el valor cambiado y el aislamiento de la salida futura. Inventario de assertions campo a campo en `I58F1DomainOracle.cs`; no reflection, no JSON equality.
CT58-29..32: matrices Dynamic/PB simple/compuesto con hermanas vs modulos, globals actuales/legacy, mezclas de procedencia, presencia, primer positivo, conservadurismo y locales A/B.
Los self-checks de fixtures, aislamiento y sabotajes validan el ORACULO con modelos preparados. No son GREEN AUTH-13. Schema/unknown futuros siguen VACUOUS BASELINE PASS; F2 debe ejecutar mutantes de reglas con su implementacion real.
