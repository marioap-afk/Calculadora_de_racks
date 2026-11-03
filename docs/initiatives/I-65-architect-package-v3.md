# I-65 — Paquete de revisión del Architect, ronda 3 (Proposal V2.1)

## 1. Encargo

Re-revisión de la **versión completa** Proposal V2.1 ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5). Ronda 2:
`CHANGES REQUIRED` sobre `8f9ac48c` (custodia en [architect-r2-verdict.md](../automation/evidence/I-65-f0/architect-r2-verdict.md)).
A65-V1-01 sigue `OPEN — BLOCKED BY P1` por diseño. Se pide dictaminar el resto y, en especial, si **P1 v3** es la sonda definitiva que el
Owner puede autorizar (una sola corrida con licencia).

## 2. Entradas

| Entrada | Ruta |
|---|---|
| Proposal V2.1 (completa; §13 y §14 = disposiciones de las rondas 1 y 2) | `docs/initiatives/I-65-proposal-v2.1.md` |
| Borrador ADR-0050 | `docs/adr/0050-interoperabilidad-analisis-estructural.md` |
| Sonda P1 v3 (no ejecutada) | `docs/automation/evidence/I-65-f0/ram-probe-p1.py` |
| Veredictos anteriores | `docs/automation/evidence/I-65-f0/architect-r1-verdict.md`, `architect-r2-verdict.md` |
| Proposal V2 (sustituida) | `docs/initiatives/I-65-proposal-v2.md` |

## 3. Delta V2 → V2.1

1. FP-01 nombra P1 v3; FP-02 incluye la regla de giro y las tolerancias de lectura de vuelta del script; FP-03 incluye Q-O5.
2. G1 incluye `src/RackCad.UI/**` y `src/RackCad.Plugin/**` con la lista de permiso de un archivo.
3. V1 solo de una cara `PositiveY`; doble cara y `NegativeY` → `Unsupported` (D-04, IDL-10, INV-05, §9, Q-O7).
4. IDL-08: nudo brazo–columna fuera del tramo de la columna → `Invalid`. `REF-CANT-01` nombra su base y las entradas del golden.
5. D-06: fila de mapeo de tubos cuadrados (`HSS_SQR`); deltas de propiedades registrados y reconocimiento del Owner sobre ±0.5 %.
6. D-09 regla 7: lee valores por defecto y fija solo `cardinal_point`; regla 9 nueva: tolerancias de lectura de vuelta medidas en P1.
7. D-11: contrato de unidades del volcado (sin `units.set`; solo `LB_IN`; cambio en la interfaz condicionado a P1-U1); cargas manuales
   como cargas en miembro.
8. D-14: F6 congelado fuera de V1.
9. D-15 / ADR-0050: `StructuralSectionUnits` consume el propietario nuevo bit a bit; referencias a ADR-0021 y ADR-0040.
10. INV-05, INV-13, INV-14 (RED nuevo), INV-16 (guarda de todo `src/**`).
11. OV-I65-07 nuevo (inyección de fallo de capacidad); lista de la guía pasa a OV-I65-08; F4 incluye OV-I65-07.
12. P1 v3: la puerta aborta ante artefactos de una corrida anterior; todos los setters de la regla 7 ejercitados y leídos; valores no
    redondos leídos en `LB_IN` y `N_MM` (paso P2); giro no redondo; vector de peso propio en un caso dedicado.

## 4. Preguntas

| ID | Pregunta |
|---|---|
| AQ3-01 | ¿Queda algún `REQUIRED` abierto distinto de A65-V1-01? |
| AQ3-02 | ¿P1 v3 mide todo lo que V3 debe congelar (D-03, D-04, D-08, regla 7, tolerancias de la regla 9, P1-U1 para el volcado) y es segura? |
| AQ3-03 | Con P1 ejecutada y Q-O1, Q-O2, Q-O3, Q-O5 y Q-O7 respondidas, ¿la V3 resultante sería congelable sin otra ronda de diseño, salvo la incorporación de valores medidos? |

## 5. Formato

El de las rondas anteriores, con `PRIOR_FINDINGS` para A65-V1-nn y A65-V2-nn.
