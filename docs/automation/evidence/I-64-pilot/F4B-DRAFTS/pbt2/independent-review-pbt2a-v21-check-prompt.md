# Encargo de la comprobación acotada de PB-T2a (enviado por la sesión al revisor de PB-T2 como mensaje, 2026-11-19; texto exacto)

Mensaje de la sesion: haz la COMPROBACION ACOTADA DE PB-T2a, antes de emitirla. El Owner autorizo emitir PB-T2a, PB-T2b y PB-T2c sin volver a el si su revision da 0 bloqueantes y 0 importantes sin resolver y la remedicion no cambia nada material (F4B-E28, Q-PBT2-4).

**Reglas:** las de siempre. Un Worker trabaja AHORA en el worktree, que esta prohibido. Lee por objetos git: HEAD d8ccbcb8. PB-T1c esta en su correccion y aun no ha cerrado.

**Entradas** (en hs65\pbt2\):
- el paquete v2.1: I-64-f4b-pbt2-gate-contract-package-v21.md, con v21-delta.md;
- el generador: make_pbt2a_contract.py, con generator-notes-pbt2a.md (las DUDAS D-1 a D-14, en su §5);
- el seco: dry-pbt2a.txt y contract-dry-pbt2a.json;
- las decisiones del Owner: owner-pbt2-decisions.md (F4B-E28), custodiada en d8ccbcb8.

**Que verificas:**
1. Que el contrato en seco es fiel al contrato propuesto de PB-T2a en la v2.1: Objective, alcance, RequiredTests, invariantes, paradas, evidencia y autoridades. Que aplica bien las cuatro decisiones de F4B-E28 y las lecciones de PB-T1b y PB-T1c: C13 por nombre, notas fuera, Trailer real, build-plugin omitido si Core falla en el RED, Snapshot J y F4B-E25.
2. Que las 14 DUDAS del generador quedan resueltas: para cada una, di que hacer. En particular D-1 (las clases de Core en el ForbiddenWriteScope), D-2 (los nombres de custodia), D-5 (F4B-E28 como autoridad) y D-13 (la declaracion de editable buscada con su texto exacto).
3. Que PB-T2a se puede emitir en cuanto cierre PB-T1c, y que comprobaciones de remedicion tiene que hacer el generador al escribir. Por ejemplo, la declaracion de `editable` en W y la linea base de la CI del cierre de PB-T1c.
4. Si hay decisiones materiales nuevas para el Owner. Se estricto.

**Donde escribes:** hs65\pbt2\independent-review-pbt2a-v21-check.md, con el veredicto (EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE) y su B/I/M arriba.

**Respuesta final:** una linea.
