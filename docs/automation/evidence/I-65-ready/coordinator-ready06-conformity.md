# I-65 — READY-06: conformidad del Coordinator sobre el SHA propuesto `4a03b65b`

- **SHA revisado:** `4a03b65b375d468c220dd68400d5dfd955bf043e`, nuevo SHA propuesto del Candidato final (decisiones §114). Su evidencia
  de READY-05 está en `813d0cee` (§115, evidencia §117).
- **Autoridades:** [INITIATIVE_LIFECYCLE](../../../INITIATIVE_LIFECYCLE.md) §8-§9; Freeze
  [Proposal V3.1](../../../initiatives/I-65-proposal-v3.1.md); [ADR-0050](../../../adr/0050-interoperabilidad-analisis-estructural.md);
  decisiones §104-§115.
- **Revisiones del Architect** (subagente de solo lectura):
  - ronda 1 sobre `507806bf`: NON-CONFORMING por A65-FC-01 ([informe](architect-ready06-r1-conformance.md));
  - ronda 2 sobre `4a03b65b`: **CONFORMING**, 0 REQUIRED, A65-FC-01..13 cerrados y A65-FC-14..19 OPTIONAL
    ([informe](architect-ready06-r2-conformance.md)).

## Veredicto del Coordinator: **CONFORMING** sobre `4a03b65b`

No hay desviaciones abiertas MATERIAL, OWNER-RESERVED ni BEHAVIORAL-WITHIN-FREEZE, ni EXP-01 de clase A.

## 1. Conformidad por arquetipo (NEW ARCHITECTURE, materialidad M-07)

| Ámbito | Base | Resultado |
|---|---|---|
| Freeze: D-01..D-15, INV-01..16, matriz §4, OV §5, Fundaciones §7, no-objetivos §9 | La [revisión de F7](../../../initiatives/I-65-f7-conformance-review.md) sobre el producto `51fc82f1`, más el delta de R-D11 (`6a58c2b1`: solo distribución; [conformidad de R-D11](../I-65-rd11/coordinator-rd11-conformity.md)) y la reverificación independiente del Architect en la ronda 1. El árbol fuera de `docs/` de `4a03b65b` es idéntico al de `6a58c2b1` | CONFORME |
| ADR-0050, Decisiones 1-9 y cláusula de compatibilidad | ADR sin cambios desde `ed8154c3` (`c18def42`); ADR-0005, 0017, 0021 y 0040 sin cambios | CONFORME |
| ADR-0003 (contrato de cuatro DLL) | Inventario del bundle de `4a03b65b`: 4 DLL de RackCad, 0 de Autodesk, catálogos permitidos y la tabla en su ruta exacta | CONFORME |
| Entradas de FOUNDATIONS | Factuales frente al código, según las dos rondas del Architect; se publican al cerrar con `Status: STABLE` literal (A65-FC-08) | CONFORME |
| Documentación de producto | README, guía de I-65 y guía de despliegue, con las cuatro correcciones de §111/§112 | CONFORME |
| Matriz OV y procedimientos | OV-I65-01..08 preparadas para el SHA final; HB-01..05 con las nueve condiciones del Owner; oráculos aprobados conservados; HB-02 = 160 (medido); lectura de HB-04 en CD-I65-FC-13 | CONFORME |

## 2. Desviaciones y su clase (LIFECYCLE §9)

| ID | Desviación | Clase | Estado |
|---|---|---|---|
| A65-FC-01 | Oráculo de HB-02 con 158 en lugar de 160 | NON-MATERIAL | Corregido en `4a03b65b` (cerrado por el Architect) |
| DEV-I65-FC-01 | Oráculo de huella de HB-04 imposible entre racks distintos (error del Coordinator en R-D11 §6 y §110) | NON-MATERIAL | Corregido por CD-I65-FC-13 antes de cualquier ejecución |
| DEV-I65-FC-02 | Cifra de comprobaciones de HB-02 sin el inventario base (error del Coordinator) | NON-MATERIAL | Corregido (A65-FC-01) |
| DEV-I65-FC-03 | En la ronda 2, el Architect ejecutó `git mktree --missing` (prohibido: escribe un objeto). Escribió el árbol suelto e inalcanzable `c81dde4a…` en el almacén común; no cambió refs, índice, archivos ni evidencia | NON-MATERIAL | Declarado por el propio Architect y verificado por el Coordinator (`cat-file -t` = tree). No se borra, porque borrarlo es otra escritura; `gc` lo poda. Precedente: CD-I65-F5-15 |

## 3. READY-01..09 sobre `4a03b65b`

| ID | Estado | Base |
|---|---|---|
| READY-01 | CUMPLE | Alcance congelado completo; diferimientos decididos por el Owner |
| READY-02 | CUMPLE | Gates cerrados; decisiones hasta §114 versionadas; documentos de producto listos; sin A-n |
| READY-03 | CUMPLE | 0 REQUIRED abiertos (A65-FC-01 cerrado); 0 discrepancias A; ninguna decisión pendiente sobre el contenido |
| READY-04 | CUMPLE | Fetch fresco a las 03:53:58Z (§116): `origin/main` = `bb0d5522` = merge-base; I-64 sin integrar; ADR 0050 libre. Se repite al registrar esta conformidad y otra vez justo antes de declarar |
| READY-05 | CUMPLE | Core 14392/14392; UI 1637 superadas y 17 omitidas; builds sin errores; CI 40814761082 4/4 (§115, §117) |
| READY-06 | CUMPLE | Architect CONFORMING (ronda 2) y Coordinator CONFORMING (este documento) |
| READY-07 | CUMPLE | Árbol limpio; sin cambio de producto pendiente; Candidato = `4a03b65b`; la punta solo añade `docs/` |
| READY-08 | CUMPLE | Matriz completa; HB-01..05 como complemento aprobado; oráculos satisfacibles |
| READY-09 | CUMPLE | Acuerdo `11062a91` (`3872fcc0`) = `a9d7080b^`; `a9d7080b` cambia solo tres líneas permitidas y deja el blob `4e6e1feb`; 0 commits sobre el Freeze hasta `4a03b65b`; sin Freeze delta; `amendment_refs: []`; ADR-0050 sin cambios ([registro](ready09-4a03b65b.txt)) |

## 4. Disposición de A65-FC-14..19 (OPTIONAL; LIFECYCLE §9: las mejoras quedan como seguimiento)

| ID | Disposición |
|---|---|
| A65-FC-14 | La copia redactada `expected-inputs-ov02.redacted.json` lleva en `_redaction.originalPath` el nombre fijo `expected-inputs.json` que escribe la herramienta de redacción (línea 38). El valor de referencia es el de `identity.txt` (`f1a7c036…`). Se registra sin cambiar la copia |
| A65-FC-15 | Las herramientas ejecutadas para `4a03b65b` son los mismos archivos que las del README de `candidate-507806bf/tools`. Se volvieron a medir a las 04:17Z: `fc_evidence.sh` `e468546f…`, `fc_identity.ps1` `93402567…` y `bundle_evidence.ps1` `a0cde570…` |
| A65-FC-16 | **Precisión operativa sin cambio de oráculo:** las listas de recientes no se miden. «Referencias en el registro» se refiere a las exportaciones B-04 de `wr_hostsnap.ps1` |
| A65-FC-17 | **Precisión operativa sin cambio de oráculo:** en O8-06 y O8-09 se anota la vista elegida y HB-04 (a) usa esa misma. Con `.dwl` presente al copiar, HB-04 (a) queda NOT_EVALUATED |
| A65-FC-18 | EDITORIAL: se corrige en el cierre documental. Son la cabecera «§111-§112», la cita del preflight repetido en READY-04 y la redacción de §116 sobre `rackmirror`, que añade ADR-0036 sin chocar con 0050 |
| A65-FC-19 | **Precisión operativa sin cambio de oráculo:** WR2-AC-05 cubre también los scripts de `dump-OV01-gen` y `dump-OV02-gen` |

**Cómo se aplican A65-FC-16, 17 y 19:**

- **Propuesta:** se aplican como precisiones dentro de las autorizaciones de W-R1 y W-R2. Solo aclaran qué se mide y qué se anota; no
  añaden ni cambian pasos u oráculos.
- **Alternativa, si el Owner la prefiere:** una ronda documental más antes de declarar, con SHA nuevo, Full, CI y relectura del delta.
  Son unos 40 minutos, sin Codex ni host.
- **Decide el Owner.**

## 5. Lo que esta conformidad no hace

- No declara READY ni `FINAL_CANDIDATE_SHA`.
- No autoriza las ventanas.
- No integra.
- No abre AutoCAD ni RAM.
