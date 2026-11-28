# Traspaso de PB-T4d (paquete v5.4, solo de texto)

**Fecha:** 2026-11-22. Sigue a `v53/handoff-pbt4d-v53.md` (reglas, carpetas y órdenes iguales). **Errata del traspaso v5.3:** el sha256 de `owner-pbt4d-cat4d-decisions.md` acaba en `…03fa1` (no `…0d75`).

**Encargo:** la recomprobación 1 de CA-T4d (`pbt4/independent-review-pbt4d-check.md`, «## Recomprobación 1 (v5.3)», línea 166 en adelante): EMITIBLE, B 0 / I 0 / M 6 (M-08 a M-13). D-S16 nuevo sha `bd02d210…01c8`.
1. M-08: JSON con sha256 de los JSON de UI del GREEN (3 juntas + 18 por filtro) y sha COMPLETO de los 19 por filtro de la base; §6.4.2 con las claves exactas (con `RedDeleted` y `Noise`). Sin correr pruebas.
2. M-09: [13] de §6.4.1, «D-S11 a D-S16».
3. M-10: «Lo que cambia» con §0, §1, §6.0, §7 y §9, y la columna «Dónde».
4. M-11 (b): ESTABLE solo por la UI exige también un filtro en su proceso → `cov_p6_v53.py`, recorrer el análisis sobre los mismos JSON. **PARA si sale PERDIDA o VARIA nueva.**
5. M-12: (a) WPUR del RED: comentarios huérfanos de la 48 y la 49 (`:50`, `:52-54`) y el resumen de la clase (`:13-15`); (b) cabecera de la pieza GREEN `:6` → `pbt4d-green-v54.txt` si cambia el sha; (c) porqué de la fila 282 (`WriteViolations`); (d) fila o tramo del `using` de `WorkspaceDraftPilotIntentStaleBaseTests.cs:6`. RED/lista nuevos como `-v54`.
6. M-13: frase de 436 → 537 líneas.
7. Reglas: si el RED solo cambia comentarios (+ fila de lista), basta reaplicar en copia nueva desde `/d/r70`, compilar con 0 `warning CS` y comprobar que el código sin comentarios del RED es el de la v5.3. **PARA si cambia código que se ejecuta.**
8. Entrega: v5.4 con tabla V54-nn («Recomprobación 1 de CA-T4d M-08»…), sha256 de la v5.4, de las piezas, del JSON, de `compile-check.md` y de `cov-p6`; una línea por menor.

## Estado: HECHA

- **v5.4:** `pbt4/I-64-f4b-pbt4-gate-contract-package-v54.md` `e37843fc…fc63` (869 447 bytes; `pbt4/make_v54.py` `31251f35…a1eb`, desde la v5.2 con fragmentos `v54/frag/` de `v54/edit_frags_v54.py` y la cabecera de la v5.3 tomada de la v5.3 con lo de M-10).
- **Piezas:** RED `pbt4/pbt4d-red-v54.txt` `80f8cc0c…81db` (`v54/make_red_v54.py`; solo comentarios de WPUR, `v54/codeonly-v53-v54.txt`); GREEN `pbt4/pbt4d-green-v54.txt` `aaf6cab8…fbf0` (la v5.3 con la cabecera corregida); lista `pbt4/pbt4d-retire-list-271.tsv` `05a4d3d1…75ac` (`v54/make_list_v54.py`: 284 filas, 271 con acción; + fila `directiva` del `using`; porqué de `WriteViolations`).
- **Reaplicación** `/d/r70` (`p6_reapply_v54.sh`, `driver-r70-ra.txt` `2044f0fc…5f36`): 5 compilaciones 0/0; tests/ cambia de texto solo en WPUR, código sin comentarios igual; src/ igual que `/d/r6y`.
- **Cobertura** `cov-p6-v54.txt` `9c1bde53…bd34` (`cov_p6_v54.py`, ESTABLE solo por UI exige un filtro en su proceso): PASA; Bajan 16 801: PERDIDA 0, VARIA 0, ESTABLE 16 801 (+118); igual que antes.
- **JSON** `pbt4d-precondition6.json` `52e03e06…8989` (`p6_json_pbt4d_v54.py`; `GreenUiCoverage`, `Green.UI[n].Json`, `BaseCoverage.UIByFilter.Runs` completos, `Reapply`); **`compile-check.md`** `d13afdb0…de42`. Los de la v5.3, como `-v53`.
- Órdenes: `bash post_p6_v54.sh r6y-p6 r6w-bc r6z-ct pbt4d-retire-list-271.tsv r70-ra` (en `pbt4dcc/compile/`); `python -I make_v54.py <repo>` (en `pbt4/`).
