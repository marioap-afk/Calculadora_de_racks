# f7-prep — índice y resumen de cambios (r5, 2026-10-10)

Borradores fuera del worktree. Base de cada archivo: su blob en `6d626794`, copiado sin cambios en `base-6d626794/`. Diff unificado de cada uno en
`diffs/<archivo>.diff`. Ningún archivo del worktree ni de `D:\r62-fixture` se modificó. Ningún gate, READY, Candidato, cierre ni integración se declara.

## Entregables nuevos

| Archivo | Contenido | SHA-256 (prefijo) |
|---|---|---|
| `f7-status.md` | qué exige F7; tabla de 27 items con dependencia de C-24, fuente, estado, siguiente paso y resultado ejecutado ahora | `9bc6417fd0a05504` |
| `independent-controls.md` | 12 controles ejecutados en solo lectura (IC-01..IC-12), los que no se pueden ejecutar y qué necesitan, y los escenarios/controles de F6 independientes de C-24 con su bloqueo exacto | `6ffab9007cde02d5` |
| `measured/` | `readonly-checks.sh` y su salida `readonly-checks-6d626794.txt` (K-01..K-12), `c20b-dryrun-6d626794.json`, `ci-6d626794.json`; scripts de edición de los borradores (`make_frontiers_r4.py`, `edit_ideas_r5.py`, `edit_ov_od1_r5.py`, `compact_json.py`) | ver `independent-controls.md` cabecera |

## Borradores actualizados (copia completa en este directorio)

| Archivo | Blob base → r5 | Líneas +/− | Resumen |
|---|---|---|---|
| `foundations-draft.md` | `2f9e0987` → `87d0eb36` | +68 / −27 | r5: §0 re-medido en `6d626794`; `Decision source` con A-1..A-4 y sus blobs (A-3 sujeta a Q23); `Protecting tests` con C-25a PASS, C-24 UNVERIFIED y la reserva de §66.4; `Known limitations` con el hecho `read-only` re-medido (A2-P2 y A4-1), F6-OBS-03, A4-1 NOT_DEMONSTRATED (regla 4), 3 de 3 actualizaciones + 1 cambio de huella, FX-04a PASS de mismo proveedor; §3 con filas nuevas (A-2, A-3, A-4, C-25a, F6-OBS-03, A4-1, FX-02, MAP_INVALID) y la causa desfasada de FX-03; §4.4 (cambios r5), §4.5 (hechos firmes no incluidos: `claude-cli`, revisiones por CLI, OAuth, MAP_INVALID); §5 reescrito |
| `ready-dry-run.md` | `49f08c0b` → `85e5b217` | +31 / −1 | nota r5; §0.1 medido en `6d626794`; §1.1 por READY (prevalece sobre §1): dependencia de C-24, estado, bloqueo exacto y ensayo; corrección del patrón de A-n (el anexo de A-3 no es A-n); §3 con las lecturas usadas |
| `closure-integration-checklist.md` | `cd2c90c9` → `1c620e7d` | +47 / −1 | nota r5; `amendment_refs` → A-1..A-4; §9.1 (trailer 0/0, MV-7 en seco, A-3 y el mapa, DC-07); §10.1: Q10 DECIDIDA, Q14 y Q19 DECIDIDAS EN PARTE, Q21 RESUELTA POR HECHO, Q22 → Q22', Q5/Q11/Q12/Q16 ampliadas; nuevas Q23 (A-3/C-43), Q24 (resiembra y MAP_INVALID), Q25 (orden F6/F7), Q26 (OD-1 frente a A-2..A-4) |
| `frontiers.json` | `3496bec3` → `46f349f6` | +94 / −48 | r4: base re-medida; F-02 y F-16 RESUELTAS; F-03 reescrita (OD-2f, A2-P2 y A4-1 consumidos); F-01, F-04..F-14 actualizadas; nuevas F-17 (A-3/C-43), F-18 (A-4), F-19 (MAP_INVALID), F-20 (recuperación de FX-02). Formato de la base conservado (`compact_json.py`) |
| `ideas-futuras-disposition.md` | `30ed5d1b` → `193e6cf6` | +22 / −6 | GAP-01 → ANOTAR (claude-cli en revisiones reales); GAP-02 (ejercido en plano a); GAP-03 (Q19 en parte); GAP-04/06 (condición de CERRAR cumplida); F6-N01 (3 actualizaciones + cambio de huella; A-2/A-3 AGREED); F6-N03; nuevas F6-N10..F6-N15; §4 (A-3/C-43 y hallazgos opcionales de A-2..A-4) |
| `ov-packets.md` | `fb41b9a0` → `c359568c` | +12 / −7 | nota r5; OV-I62-03 sin la cobertura de A-4 (Q22') y con MAP_VALID requerido (Q24); OV-I62-04 con OD-3 = A y causa de C-26 desfasada; OV-I62-05 (a) sin relanzamiento (A-2 L170) y precedente B3; OV-I62-06 con Architect `claude-cli` admisible (D.8 + OD-3); §8 con Q22' y Q24 y OD-2f |
| `od1-packet.md` | `7d77c625` → `9908f307` | +7 / −2 | r5: Freeze V14 + A-1..A-4; fila «Relación con A-2..A-4»; Q26 |

## Cobertura

Los siete archivos de `docs/automation/evidence/I-62-prep/f7/` tienen aquí su versión r5/r4; `f7/measured/c20b-dryrun-e9473425.json` queda como
historial y su sucesor en seco es `measured/c20b-dryrun-6d626794.json`.
