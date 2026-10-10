# I-62 F6 — Efectos de la reconstrucción sobre pruebas y evidencia anteriores (decisiones §66, punto 4)

## Fuentes y claves

Fuentes, todas leídas sin escribir:

- `ev` = `docs/automation/evidence/I-62-evidence.md`;
- `dec` = `docs/automation/decisions/I-62.md`;
- `F6\` = `docs/automation/evidence/I-62-F6/`.

No hubo trabajo de F5 sobre el fixture. **Ningún resultado de F6 ejecutó Validate(M), Evaluate, Classify o Resolve sobre el mapa del fixture.** La
única ejecución es el cálculo de referencia del kit A4-1 (ev §119); la propia sonda dejó esos pasos «no ejecutada».

Claves de dependencia:

- **(a)** validez de la activación o del mapa;
- **(b)** SHA o blobs concretos del linaje r1 (`fbe25347`, `fx/u1`);
- **(c)** solo la identidad del runtime y los blobs de routing `bba08fc4`, catálogo `166d978d`, descriptor codex-cli `155f3469` y mapa `4d49d3e1`.
  Estos blobs **son idénticos en r2** (I-9).

Opciones: **(b)** repositorio nuevo (recomendada); **(a)** refs `*-r2` en el mismo repositorio; **(c)** sustitución de `main`. «Intacto» significa que
el resultado sigue siendo verdadero y comprobable para r1. «No reutilizable» significa que no invalida nada, pero no sirve como insumo de FX-02 en r2.

## Tabla de efectos

| # | Resultado (dónde) | Estado acreditado | Objetos del fixture | Depende de | (b) | (a) | (c) | Acción / disposición |
|---|---|---|---|---|---|---|---|---|
| 1 | Fixture D.1, pasos 1-4 (`F6\README.md`; ev §62; `F6\fixture\fixture-identity.json`) | hecho | F_seed `930288c5`, F_norm `a9f6c929`, F_eff `fbe25347`, tag, reclamo `eea0114a` | es el propio defecto | r1 congelado como evidencia de F-MAP-1 | r1 sigue siendo `main` del repositorio | `main` se mueve (no FF) | registrar r1 = linaje MAP_INVALID; no se reescribe |
| 2 | Auditoría pública OD-7 (`F6\CI\public-audit.json`; ev §65) | medido | 5 refs y 75 objetos a las 15:48Z | ninguna | intacto; **auditoría nueva** del repositorio r2 | auditoría de los objetos nuevos | ídem + forced update | auditoría P-4 antes de publicar |
| 3 | Recuperación de Actions R1/R2, CI clase A (dec §50; ev §66) | aceptado | `ci/smoke` en GitHub r1; corridas 37515854486, 37515901956 | ninguna | intacto; **clase A por restablecer** en r2 (la primera publicación no dispara `push`) | Actions ya operativo | ídem | comprobar CI de r2 (P-3) |
| 4 | Sonda 1 de OD-4 → FX-04b/C-25b UNSUPPORTED (ev §62) | medido | clon `probe-od4-1` desde `fbe25347` | (c) | intacto | intacto | intacto | — |
| 5 | **FX-05 / C-27 PASS** (dec §47) | PASS (corrida 2) | `fx/u1-fx05*` sobre `eea0114a`; clones fx05 | ninguna (guarda P-16 y digests del estado real) | intacto | intacto | intacto | — («no se repite salvo que su evidencia se invalide»: no se invalida) |
| 6 | OD-2b-PROBE ×2 (ev §64) | medido | A en `54f2a4a8`, arch en `fbe25347` | (c) | intacto | intacto | intacto | — |
| 7 | OD-2: comprobaciones pasivas y P-01 | medido | ninguno | (c) | intacto | intacto | intacto | — |
| 8 | OD-2d-PROBE ×2 (ev §70) | medido (obsoleto después) | A en `ae25b596`, arch en `fbe25347` | (c) | intacto | intacto | intacto | — |
| 9 | **FX-01 / C-23 PASS** (dec §51) | PASS | sesión A, clon A en `54f2a4a8`; preflights `b9c554c` | (c) (catálogo, routing y descriptor de escritorio: iguales en r2) | intacto; la fila de RemoteFacts (`main` = `fbe25347`) sigue comprobable | intacto | la fila de RemoteFacts **deja de comprobarse en vivo** | FX-02 en r2 necesita los preflights CUSTODY del titular nuevo (ya los necesitaba A2) |
| 10 | **C-22 PASS** (dec §51; `F6\FX-U1-chain\chain.json`) | PASS | BOOTSTRAP `1746b404`, G0 `5a3a7d69`, QU `1a4fc9c6`, T1 `d30fb6a9` (blob `628d89af`, `MainSha` `fbe25347`); CI 37538606357, 37538952116, 37543264455, 37543386267 | (b); **(a) implícita**: el contrato se validó con `Test-Json` y coincidencia de blobs, sin Evaluate | intacto como registro; no reutilizable | ídem | ídem | **⚑ PREMISA AFECTADA por MAP_INVALID mismo, no por la reconstrucción.** C-22 dice «hasta un contrato I62 válido». Con 16.13 literal, la emisión de T1 (una evaluación) sobre `fbe25347` acaba en STOP. **Disposición:** mantener C-22 como arranque D.1 (esquemas y custodia), o volver a acreditarlo en r2. El nuevo arranque lo produce sin coste adicional |
| 11 | QH `ae25b596` INVALID, F6-OBS-01; validadores de F4 (ev §69-§71) | QH INVALID; QR/QH2 VALID | estados YAML r1 | (b) | intacto | intacto | intacto | — |
| 12 | Cadena de reparación R (`cc21e1d`→`cabed54`; dec §52) | acreditado | blob de decisión `b7cff537`; CI 37567554756, 37567674783 | (b) | intacto; no reutilizable (el `effective_sha` es inmutable) | ídem | ídem | — |
| 13 | FX-04a B1 (dec §53) | INVALID_TEST_ORACLE | clon B en `cabed54` | (b) | intacto | intacto | intacto | — |
| 14 | FX-04a B2 (dec §55) | INVALID_LAUNCH (2/23 en bruto) | clon B2 en `cabed54` | (b) | intacto | intacto | intacto | — |
| 15 | Instantánea QH2 `D:\r62-fixture\fx04a-qh2-origin.git` (ev §80, §87) | conservada; hook que rechaza push | repositorio aparte, `fx/u1` = `cabed54` | — | **intacta** | intacta | intacta | ninguna opción la toca |
| 16 | **FX-04a B3 / C-25a PASS** (dec §58.1, §60) | PASS 23/23 | clon B3 en `cabed54` desde la instantánea | (b), solo el estado QH2. Ninguno de los 23 campos es EFF, clasificación, `Authority` o mapa | **intacto** | intacto | intacto | **⚑ (bajo)** El oráculo no modela Evaluate/MAP_INVALID. Confirmar en el registro que C-25a mide la portabilidad de la derivación de custodia (B = oráculo), no la corrección normativa del paso siguiente con 16.13. Sin B4 |
| 17 | Bloque A2-P2 (ev §93) | Controller 8/8 por `cmd.exe`; Architect 0/6 STALE (§65 D-2) | A en `ae25b596`, arch en `fbe25347` | (c) | intacto | intacto | intacto | — |
| 18 | U-07 TRX y U-08 `.gitignore` (dec §62; `F6\FX-02\U07-trx-R37949808974`) | publicado; CI 37949808974; TRX 1/1 | `b6d294e`, `c785def` | (b) | intacto; **no reutilizable**: hay que volver a aplicarlos en r2 y hace falta una corrida TRX nueva | ídem | ídem | disposición: U-07/U-08 se extienden a FX-U2 (o variante del seed) |
| 19 | U-14, conteo de sesiones (dec §63.2) | dispuesto: A y R cuentan | sesiones A, R (y B1-B3) | historia r1 | **⚑ AFECTADO:** el nuevo arranque necesita una sesión de Principal. `A4-PRINCIPAL-A2-CONSUMO` y A4-6 dicen «titular A2 de FX-02 designado por T16», y un titular que abre FX-U2 no lo es | ídem | ídem | disposición del Coordinator y línea de consumo del Owner; posible A-n (§66: «No se amplía A-4 por defecto») |
| 20 | Negativos sellados N4-N10 (dec §62-§63.4; `F6\FX-02\s62`, `s63`, `s66`) | N4, N5, N7a y N7b aceptados; N8 vuelto a sellar `25a33a48`; N9/N10 borrador | `negatives.md` sin SHA del fixture; `supervision-checks.md` (`729fc3ef`) cita `628d89a`, `cabed54` y `effective_sha` | (b) parcial | las mutaciones se pueden reutilizar; **`supervision-checks.md` hay que volver a sellarlo** sobre r2; **N9** depende de la semántica de T16 | ídem | ídem | volver a sellar (precedente N8); N9 no es NOT_APPLICABLE sin demostrar que falta su precondición (§63.4) |
| 21 | Referencia AUTHOR, CD-19 / F-1..F-6 (`F6\FX-02\author-ref`, `s63`, `s64`; dec §64.5, §65 D-6, §66.7) | custodiado | sesión autora de `d30fb6a9` | (b) | no reutilizable: el contrato de T1 de r2 tiene un autor nuevo; la discrepancia con B.2 (sin trailer) vuelve a aparecer | ídem | ídem | observación nueva; aplicar D-6 |
| 22 | Orden O4 y clon A2 (ev §118; `F6\FX-02\s65`) | publicado; CI 38002393525 | `95bdc29d`; clon `D:\r62-fixture\A2` (`ExpectedMain` `fbe25347`) | (b) | no reutilizable; **conservar `A2` intacto** (es el directorio de la medición A4-1); clon nuevo `A2-r2` | ídem | los clones r1 ven el forced update | órdenes nuevas para FX-U2 |
| 23 | **Sonda A4-1** R20261010T000542Z-a4-1-probe (dec §66.1; ev §119-§120) | NOT_DEMONSTRATED; consumida, sin repetición | clon A2 en `95bdc29d`; la referencia cita `fbe25347`, `930288c5`, `a9f6c929`, el mapa y MAP_INVALID esperado | veredicto (c) (A-4, regla 5); escenario de referencia (a)+(b) | **intacta**: el veredicto es de capacidades; la referencia MAP_INVALID era correcta para r1. La identidad sigue sin ser elegible | intacta | intacta (la captura de `ls-remote` deja de comprobarse en vivo) | ninguna; no invalida ni pide repetir |
| 24 | Staging de FX-02 (`F6\kits\FX-02\`: `frontiers.json`, `order-FX-U1-O4.template.md`, `designation-A2.template.md`, `launch-card-A2.md`, `controller-contracts.md`, `worker-reviewer-contracts.md`, `drafts/*`, `a4-probe-v3/gates.env`) | preparado (no acreditado) | `cabed54`, `fbe25347`, `628d89af`, `d30fb6a9`, `1746b404`, `95bdc29d` | (b) | **regenerar** con los SHA r2 (mecánico) | ídem | ídem | además, U-17 (b) (dec §62) nombra el contrato de T1 `{d30fb6a9, 628d89af}`: hace falta una disposición para el contrato r2 |
| 25 | Staging de FX-06 (`F6\kits\FX-06\staging\`) | preparado; autopruebas 64/64 y 9/9 sintéticas | fija `cabed54` y blobs «en cabed547»; X v1 y oráculo fijados «para FX-U1» | (b) en los insumos fijados | regenerar los insumos fijados; las autopruebas no cambian | ídem | ídem | — |
| 26 | **Oráculo congelado de FX-02, D-0** (dec §65.4; ev §117) | congelado (sin SHA) | — | **(a)**: PASS = VERIFIED, que exige `Authority` pass | **solo alcanzable en r2**; el oráculo se reutiliza tal cual | alcanzable en `main-r2` con la relectura de `origin/main` | alcanzable | — |
| 27 | C-20b / C-20c (F4, en RackCad) | PASS | no es el fixture | origen del mapa | intactos | intactos | intactos | — |

## Ensayos anteriores que la reconstrucción invalidaría

- **Con (b):** ninguno. Ningún resultado acreditado deja de ser verdadero ni de poder comprobarse. Lo que cambia es la **reutilización**:
  - la cadena de FX-U1, U-07/U-08, O4, el clon A2, la referencia AUTHOR, el staging y `supervision-checks.md` no sirven como insumos de FX-02 en r2;
  - la autorización de la sesión A2 está redactada para un titular T16.
- **Con (c):** se pierde la comprobación en vivo de los hechos acreditados que nombran `main` = `fbe25347`:
  - la fila de RemoteFacts de FX-01;
  - el `ls-remote` de la sonda A4-1;
  - la tarjeta y el clon de A2;
  - además, todos los clones r1 reciben un forced update.

  Es la «sustitución de refs» que §66.4 pide disponer. No se recomienda.
- **Con (a):** ninguno, pero los dos linajes conviven y hay dos trailers en el mismo repositorio.

## Señalados para disposición del Coordinator (con independencia de la opción)

- **⚑ C-22:** la premisa «contrato I62 válido» está afectada por MAP_INVALID mismo. Mantenerlo o volver a acreditarlo en r2.
- **⚑ C-25a:** confirmar el alcance (portabilidad), sin B4.
- **⚑ Presupuesto (U-14, A4-6, `A4-PRINCIPAL-A2-CONSUMO`):** la sesión del nuevo arranque no está cubierta por la redacción literal.
- U-07/U-08 y U-17 (b): extenderlos o volver a emitirlos para FX-U2.
- `supervision-checks.md`: volver a sellarlo. N9: examinar su precondición. AUTHOR: observación nueva.
- FX-02 sigue además bloqueada por A4-1 NOT_DEMONSTRATED. La reconstrucción sola no habilita C-24.
