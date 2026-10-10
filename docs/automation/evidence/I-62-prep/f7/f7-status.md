# I-62 — Estado de F7 y de sus controles frente a C-24 (borrador r5, 2026-10-10; no declara ningún gate)

> **Naturaleza.** Análisis de solo lectura y borradores, fuera del worktree (`f7-prep/`). Orden: decisiones §66, punto 9 («Avanzar F7 y los controles
> independientes que no dependan de C-24»), y §60, punto 5. Base medida: punta `6d62679409c4cb83d0f4a4cee8fbc1bc945a547b` (= remota), `origin/main`
> `bb0d5522` (= merge-base), `MC_I62` `6f0187cb`; medición a las 2026-10-10T01:23Z con `git ls-remote` (sin fetch). Nada de este archivo declara GATE
> PASS, READY, Candidato, cierre ni integración. Rutas relativas al worktree de I-62 salvo `measured/…` y los borradores de este directorio.
>
> **Corrección de los hechos de partida de la orden** (comprobados en el registro): A-2 AGREED está en decisiones **§57.1** (no en §61); §61.1 es A-3
> AGREED; A-4 AGREED está en **§64.1**; FX-04a / C-25a PASS en §60.1; OD-2f en §65; A4-1 NOT_DEMONSTRATED, FX-02 UNVERIFIED y MAP_INVALID en §66
> (commit `6d626794`, evidencia §121). FX-04a ya **no** está «OPEN / UNVERIFIED»: los borradores F7 del worktree (2026-10-07/08) lo decían.

## 1. Qué exige F7 (texto congelado)

| Elemento | Fuente | Contenido |
|---|---|---|
| Resultado observable | V14 `docs/initiatives/I-62-proposal-v14.md` L944 | «Preparación del cierre sin cambio normativo: borrador factual de FOUNDATIONS en la evidencia, ideas-futuras, paquete OV» |
| Obligaciones del Anexo C | V14 L944 (columna «Obligaciones»: «—») | ninguna: F7 no tiene controles C-xx propios |
| Dependencias | V14 L944 | «no depende de READY-06» |
| Cierre de gate | `docs/INITIATIVE_LIFECYCLE.md` §7 (puntos 1-7; punto 4: «el CI propio del SHA de cierre fue leido y esta verde antes del siguiente gate»); `frontiers.json` F-07 | publicación por la sesión principal; CI del SHA de cierre; revisión del Coordinator del diff contra Freeze + A-n; Core Full si se trata como gate funcional (AGENTS, fila «Cierre de gate funcional V2») |
| Entrada de FOUNDATIONS | LIFECYCLE §4.1 (entrada factual redactada antes de READY-04, revisada por la conformidad); WORKFLOW §11.4 (se publica en el cierre, «ya conformada») | sus `[PENDIENTE]` se resuelven con hechos de F6 GATE PASS y OD-1 (`foundations-draft.md` §2) |

**Consecuencia firme:** V14 D.4 L2544 no da vía de limitación a FX-02 («Si falta: UNVERIFIED; F6 pendiente»). Con FX-02 / C-24 UNVERIFIED (§66.2),
**F6 GATE PASS es imposible**, y con él todo lo que lo presupone: el texto final de FOUNDATIONS, READY-02..09, el Candidato, la OV, el cierre y la
integración. Si F7 puede cerrarse antes que F6 es una pregunta abierta (Q25). Lo que sí avanza sin C-24: los borradores (este directorio) y los
controles de §2 marcados «no».

## 2. Tabla de estado

Leyenda de la columna 2: **sí** = depende de C-24 (FX-02 PASS) o de F6 GATE PASS; **en parte** = una parte avanza sin C-24; **no** = independiente.

| # | Item | ¿Depende de C-24? | Fuente (ruta:línea) | Estado r5 | Siguiente paso / bloqueo exacto | Resultado ejecutado ahora (solo lectura) |
|---|---|---|---|---|---|---|
| 1 | Borrador factual de FOUNDATIONS (texto) | en parte | V14 L944; LIFECYCLE §4.1; `docs/automation/evidence/I-62-prep/f7/foundations-draft.md` §2 | **r5 hecho** (`f7-prep/foundations-draft.md`): A-1..A-4 con blobs; C-25a PASS; hechos `read-only`, F6-OBS-03 y A4-1 de `codex-cli`; 3 de 3 actualizaciones + 1 cambio de huella | `[PENDIENTE]` restantes: estado de C-24/C-39/C-42 (7, 8)/C-32/C-37 en F6 GATE PASS (**sí**), análisis de §66.4 sobre C-22/C-23/C-25a/C-27, Q3, Q23, OD-1, blob del catálogo y binario de `codex-cli` al redactar | DC-08 en seco (K-11): las 5 clases, `I61_P3`, 25 `I62F4*Tests.cs`, 13/13 esquemas I62, 5 descriptores, 5 esquemas de hechos y `artifacts/` existen en `6d626794` |
| 2 | Forma de la entrada (R o N) | no | `closure-integration-checklist.md` §10 Q3 | abierta | decisión del Coordinator (Q3) | — |
| 3 | Disposición de `ideas-futuras.md` | en parte | V14 L944; `I-62-prep/f7/ideas-futuras-disposition.md` | **r5 hecho**: GAP-01 (ANOTAR), GAP-02, GAP-03, GAP-04/06 (condición de CERRAR cumplida), F6-N01, F6-N03; nuevas F6-N10..F6-N15; §4 de A-n | GAP-02 y la parte F6 de GAP-09 dependen de que FX-02/FX-06 se ejecuten (**sí**); el resto, del Coordinator | blob de `docs/ideas-futuras.md` sin cambio: `570507d1` (K-06) |
| 4 | Paquete OV: OV-I62-01 (autoverificación) | no (en parte si Q4 la une a OV-I62-03) | V14 §18 L958; `ov-packets.md` §2 | sin cambio de fondo | Q4, Q5, **Q24** (resiembra MAP_VALID) | — |
| 5 | Paquete OV: OV-I62-02 (preflight del host) | no | V14 §18 L959; `ov-packets.md` §3 | sin cambio | se ejecuta sobre el Candidato | — |
| 6 | Paquete OV: OV-I62-03 (FX-02 compacto) | **sí** | V14 §18 L960; D.5 L2560; `ov-packets.md` §4 | **r5:** A-4 no cubre D.5 (A-4 L336): el compacto no tiene las rutas de A4-1..A4-6 | **Q22'**, Q24, CD-05/OQ-25; F6-OBS-03 | — |
| 7 | Paquete OV: OV-I62-04 (topología B o limitación) | no | V14 §18 L961; `ov-packets.md` §5 | **r5:** OD-3 = A (§56); causa de C-26 desfasada | reclasificación de FX-03 por el Coordinator; tarjeta de limitación al Owner en la OV | — |
| 8 | Paquete OV: OV-I62-05 (a) (FX-04a compacto) | en parte | V14 §18 L962; D.3 L2448-L2449 (QH tras BOOTSTRAP con T1 si no hubo Q7); `ov-packets.md` §6 | **r5:** precedente B3 PASS con el texto íntegro (`233b0582…`); A-2 no se aplica a D.5 (A-2 L170) | Q6, Q17, **Q22'** (sin relanzamiento tras un INVALID_LAUNCH), Q24 | — |
| 9 | Paquete OV: OV-I62-05 (b) (FX-04b o limitación) | no | V14 §18 L962; `ov-packets.md` §6 | sin cambio; Controller con F6-OBS-03 sin la ruta de A-4 | Q16; tarjeta de limitación | — |
| 10 | Paquete OV: OV-I62-06 (FX-06 compacto) | en parte | V14 §18 L963; D.8 L2633; `ov-packets.md` §7 | **r5:** Architect por `claude-cli` admisible por D.8 con OD-3 = A «si el Coordinator lo dispone» | Q19 (FX-06), Q22', Q24, disposición del Architect | — |
| 11 | Paquete OD-1 | no | V14 §18 L970 (OD-1: «antes de READY-03»); `od1-packet.md` | **r5 hecho**: Freeze V14 + A-1..A-4; ADR-0048 `e1bd8d91` sin cambio | Q26; no se pide hasta que el Coordinator lo ordene | K-06: blob `e1bd8d91` en `6d626794` |
| 12 | Ensayo en seco de READY-01..09 | READY-02..09 **sí**; 01 y 03 en parte | LIFECYCLE §8; `ready-dry-run.md` | **r5 hecho** (§0.1 y §1.1) | ver `ready-dry-run.md` §1.1 | K-01..K-12 |
| 13 | Lista de cierre e integración | **sí** (ejecución); no (preparación) | WORKFLOW §11.3-§11.6; `closure-integration-checklist.md` | **r5 hecho** (§9.1, §10.1; Q22', Q23..Q26) | todo tras el Candidato | K-05, K-10 |
| 14 | Fronteras F-01..F-20 | — | `frontiers.json` | **r4 hecho**: F-02 y F-16 resueltas; nuevas F-17..F-20 | — | — |
| 15 | F7 GATE PASS | **depende de Q25** | V14 L944; LIFECYCLE §7 punto 4 | no evaluable | Q25 (orden frente a F6); publicación por la sesión principal; CI y Core Full del SHA de cierre de F7 | — |
| 16 | C-20b en seco (derivación del mapa = mapa) | no | V14 Anexo C L2358 | EQUAL | se repite sobre el merge local en la integración | `clause_map.py check bb0d5522 6d626794` → `EQUAL (MV-2..MV-6)`, 31 archivos, 15/0/53/2, `MapBlob` `4d49d3e1` (`measured/c20b-dryrun-6d626794.json`, SHA-256 `bbad9c9e…`) |
| 17 | C-21 (vigencia de `MC_I62`) | no | V14 L2360 | vigente | se rompe si A-3 se materializa (Q23), si cambia ADR-0048 (Q2) o en el cierre (Q1) | K-03: 0 archivos de la lista cerrada de 16.13 cambiados en `6f0187cb..6d626794` |
| 18 | MV-7 sin historia (punto de entrada, 16.13, punteros) | no | AUTOMATION_PLAN 16.13 (MV-7); `closure-integration-checklist.md` §3 fila 5 | conforme en la punta (no es la comprobación sobre el merge) | se repite sobre `MERGE_LOCAL_SHA` con la guarda C-20a | K-10: sección de WORKFLOW ×1, 16.13 ×1, WORKFLOW §12 nombra 16.13, puntero ×2 (§16 y 16.3), 16.3 sin puntero = base |
| 19 | Evidencia de no aplicabilidad del checklist de la guía (READY-08) | no | `ready-dry-run.md` READY-08 | sin cambio | decisión del Coordinator | K-04: 0 archivos fuera de `docs/` y `tests/` (977 en total, 58 en `tests/`) |
| 20 | Trailer `Agent-Protocol-Normative: I-62` (Q15) | no | V14 §14.1; AP 16.14 | inexistente | decisión del Coordinator (Q15) | K-05: 0 en la rama, 0 en `origin/main` |
| 21 | Identidad del Freeze y de las A-n (READY-09 en seco) | no | LIFECYCLE §6, §8 READY-09 | conforme en seco | Q12 (ampliada), Q18; patrón de A-n corregido | K-08/K-09: V14 sin diff `1d5cdbec`→`fb49fceb`; registro `0f6b8608`; trailers de `fb49fceb`: solo `Co-Authored-By`; A-1..A-4 iguales en su commit de acuerdo y en la punta, sin commits posteriores |
| 22 | Visibilidad de A-2..A-4 en contrato y estado (Q11) | no | LIFECYCLE §8 READY-09 («visibilidad de todas las A-n») | **ausente** | decisión del Coordinator; escritura de la sesión principal (no de este carril) | K-12: `amendment_refs: []`; 0 menciones de A-2..A-4 en el contrato; sin `a2_status`..`a4_status` en el estado |
| 23 | DC-07 (hermanas) | no | WORKFLOW; `closure-integration-checklist.md` §8 | sin cambio | repetir en el momento | K-02: `main` `bb0d5522`, I-52 `fb6b5648`, I-64 `39b45f36` |
| 24 | CI de la punta | no | AGENTS; WORKFLOW §11.5 | verde | — (señal de salud) | corrida 38012502295, `push`, `6d626794`, 4/4 `success` (`measured/ci-6d626794.json`) |
| 25 | A-3: materialización y C-43 (Q23) | no | `I-62-A-3.md` L840, L1136, L1190-L1192; decisiones §61.1 | AGREED, no materializada; deuda O4 | decisión del Coordinator (gate, `MC_I62` nuevo, resiembra, C-43) | no ejecutable: C-43 se ejecuta «sobre el validador o el procedimiento materializados» (A-3 L1136) |
| 26 | Cobertura de D.5 (Q22') | en parte | A-2 L170; A-4 L336; Q-A2-03 | sin cobertura | decisión del Coordinator (posible A-n; posible OWNER-RESERVED) | — |
| 27 | Resiembra frente a MAP_INVALID (Q24) | no (depende del carril MAP_INVALID) | V14 D.1 L2390-L2397; D.5 L2558; evidencia §119 | abierta | resultado de §66.3 y decisión del Coordinator | — |

## 3. Qué no depende de C-24 y se adelantó ahora

- Borradores r5/r4 de los siete archivos F7 (`CHANGES.md` resume los cambios; `diffs/` los muestra línea a línea frente a la copia de `6d626794`).
- Comprobaciones K-01..K-12, C-20b en seco y lectura de la CI de la punta: todas conformes; ningún hallazgo nuevo de deriva (`measured/`).
- Hallazgos documentales (no corregidos en el worktree): la matriz de `docs/automation/evidence/I-62-F6/README.md` está desfasada (fila C-25a de L54
  en UNVERIFIED frente a L21 y §60.1; filas FX-03 de L24 y C-26 de L56 con «OD-3 = RECHAZAR», superada por §56; C-24 de L53 con bloqueos de §55); el
  campo `f6_status` de `docs/automation/state/I-62.yml` sigue en la redacción de §46-§48. Son de la sesión principal.

## 4. Lo que el Coordinator tiene que decidir (solo lo que bloquea F7 o el cierre)

Q22' (D.5 sin A-2/A-4), Q23 (A-3 y C-43), Q24 (resiembra y MAP_INVALID), Q25 (orden F6/F7), Q26 (OD-1 frente a A-2..A-4), Q11 (visibilidad de las
cuatro A-n), además de las abiertas de §10.1. Las decisiones de F6 independientes de C-24 (reclasificación de FX-03, Q16, FX-06 opción B y su
Architect) están en `independent-controls.md` §3.
