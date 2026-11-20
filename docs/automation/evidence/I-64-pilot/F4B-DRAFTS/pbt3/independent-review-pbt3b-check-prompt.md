> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktrees-de-Codex>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo de la comprobación acotada de PB-T3b (CA-T3b) (enviado por la sesión al revisor de PB-T3 como mensaje; texto exacto)

## 1. CA-T3b, sobre la v4.5 (2026-11-20T05:05:51.005Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T3b (CA-T3b)** antes de emitirla.

**La autorización:** F4B-E35, pregunta 3 (el Owner, «Sí, con esa condición»). La condición: 0 bloqueantes, 0 importantes sin resolver y que la remedición sobre el cierre de PB-T3a no cambie nada material.

**El estado:**
- **PB-T3a está CERRADA** (F4B-E43), con la custodia `c40eea4ad126ec6209dfef40e643e180b364bc97`, GREEN `6919e17a`, intento 2 de 3, y la CI `44668584248`.
- **nc2:** se aceptó por disposición del Owner, que decidió ampliar el sustituto de nc2 a una lista cerrada de literales por paquete. La v2 tiene la revisión APTO, con SHA `C033E96C…`.
- **El worktree está parado**, sin delegación activa. Aun así, el código se lee solo por objetos git desde el checkout principal, en `c40eea4a`.

**Las entradas** (en `hs65\pbt3\`, salvo que se diga otra cosa):
- **el paquete v4.5:** `I-64-f4b-pbt3-gate-contract-package-v45.md`. Ya lo comprobaste para PB-T3a en tu §11. Ahora tocan el §5.0, el §5.3 y el §6.0 a §6.2 en lo que se refiere a PB-T3b;
- **el generador:** `make_pbt3b_contract.py`, junto con `issue_pbt3b.py` y su `issue_pbt3b-selftest.txt`;
- **el seco:** `dry-pbt3b.txt` y `contract-dry-pbt3b.json` (sha `5EE38DAB…`), con `neg-pbt3b.txt` (68 de 68);
- **la remedición sobre el cierre:** `measure_pbt3b.py`, `pbt3b-anchors-out.txt`, `sweep-pbt3b-close.txt` y la R-21 de `pbt3-measurements.md`;
- **las dudas del generador y de la cadena:** `generator-doubts-pbt3b.md`, con su lectura. Las cuatro primeras las decides tú;
- **los sustitutos:**
  - `hs65\nc1-cascade\limits-declared.md` (PD-F4B-01, `41440edc…`);
  - `hs65\nc2-substitute-f4b-v2\limits-declared.md` (v2, `C033E96C…`) y su script;
- **lo ya aceptado en PB-T3a:** el contrato reemitido, `git show 485b5651:docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json`, y tu informe `independent-review-pbt3a-v43-check.md`.

**Mientras revisas,** el generador volverá a escribir el seco solo para fijar el SHA de nc2 v2 y su custodia. Los resultados irán a `*-r2.*`, con un `delta-r2.txt`. Lo que lees ahora no cambia. Cuando esté, te pediré que compruebes el delta.

**Qué comprobar:**
1. **La fidelidad del contrato en seco frente a la v4.5 para PB-T3b.** Revisa:
   - el alcance: las 11 rutas, y si PP y UR van como UNIT_CHANGE;
   - el ForbiddenWriteScope (duda 1);
   - las 15 invariantes;
   - las 34 paradas, con C07, C18 y la C11 literal;
   - los 31 RequiredTests, con `MinSelected` 3 261;
   - las 15 entradas de evidencia;
   - las 62 autoridades;
   - los marcadores.
2. **D-1 y el `Skip`:**
   - S-1 a S-6 en sus métodos de los tres archivos de guardas de F4;
   - que el motivo literal no esté en el contrato (duda 2);
   - el filtro UI de paridad de PB-T3a.
3. **RR en PB-T3b:**
   - la lectura del §6.0: la reparación no toca `src/` ni los tres archivos de guardas;
   - la tabla de literales de RR-3 (duda 3) y las tres entradas resueltas desde el §5.3. ¿Basta así o hace falta una v4.6?;
   - INV-PBT3B-02 frente a RR-2 y el doble `PushBackFullWriteSupport.cs` (duda 4);
   - la verificación (comprobaciones n.º 10 y n.º 11) y RR-6.
4. **La C11 con sus 5 cambios para F4B-E40 y F4B-E43.** ¿Es fiel a las decisiones? ¿Basta sin una v4.6?
   - El primer `ExpectedEvidence` tiene que ser uno de los literales de `F4B_EXACT_EVIDENCES` del script v2.
5. **La remedición:** ¿cambia algo material frente a lo que mide la v4.5? Esto incluye el 99 frente a 98 y el barrido de las guardas.
6. **La Snapshot K:** que sea fiel a lo custodiado.
7. **Las CT de PB-T3b** frente al momento de cada lectura y a los gestos que no cambian nada, con los diseños de F4 que llegan al plan.
8. **Decisiones nuevas para el Owner,** si las hay.

**Formato:**
- El informe va en `hs65\pbt3\independent-review-pbt3b-check.md`, con secciones numeradas (§1, §2, …).
- El veredicto va en una línea propia: «**Veredicto: EMITIBLE …**» o «**Veredicto: EMITIBLE TRAS CORRECCIONES …**», con B / I / M.
- Propón el literal ASCII de `BOUNDED_RESULT`, que debe empezar por EMITIBLE. Mira cómo lo comprueba `bounded_check` en el generador.

**Reglas:**
- nada en `<worktrees-de-Codex>\...`;
- Python con `-I` y solo en scripts escritos con Write, sin `-c` ni heredoc;
- escribe solo en `hs65\pbt3\`, en ese informe y en guiones con el prefijo `cat3b_`.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal.

## 2. Recomprobación 1 (2026-11-20T05:21:08.886Z)

Recomprobación de CA-T3b sobre el seco r2.

Tu CA-T3b dio EMITIBLE, B 0 / I 0 / M 4. Ahora toca la r2.

**Añade** al final de `hs65\pbt3\independent-review-pbt3b-check.md` una sección «§12. Recomprobación sobre el seco r2». Lleva número y el veredicto en una línea propia: «**Veredicto: EMITIBLE …**». No reescribas lo anterior.

**Entradas** (en `hs65\pbt3\`):
- `contract-dry-pbt3b-r2.json` (sha `395DCBB9…`, 73 150 B) y `dry-pbt3b-r2.txt`;
- `delta-r2.txt`: según él, solo cambian `Authorities` (de 62 a 64) y el SHA en PBT3B-C11;
- `neg-pbt3b-r2.txt` (69 de 69) y `issue_pbt3b-selftest-r2.txt`;
- el generador nuevo, `make_pbt3b_contract.py` (142 687 B, sha `D9D82540…`). Se cambió en su sitio mientras revisabas, solo para fijar `NC2_V2_SHA` y la custodia de la v2. Si leíste la versión anterior, comprueba que lo que cambió es solo eso;
- `issue_pbt3b.py`, con `NC2_V2_EXTRA`: la custodia en `F4B-DRAFTS/nc2-substitute-f4b-v2/`, con la redacción de `limits-declared.md` y de los dos `result-*.json`.

**Qué comprobar:**
1. ¿El delta es completo y exacto?
2. ¿Queda resuelto tu M-04?
3. ¿La custodia de la v2 es fiel? El script tiene que ir byte a byte con su SHA. El informe de revisión va como archivo nuevo, y la copia del v1 no se toca.
4. ¿Tu `BOUNDED_RESULT` sigue pasando el `bounded_check` del generador nuevo? ¿Cambia la cuenta B/I/M?

**Las reglas** son las de antes, con el prefijo `cat3b_`.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal final.
