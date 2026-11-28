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

## 3. Recomprobación 2 (2026-11-20T07:20:29.664Z)

Recomprobación de CA-T3b sobre la v4.6 (la reemisión de PB-T3b, intento 2).

**Qué pasó:** PB-T3b se emitió (F4B-E44, BASE `eab27a9e`) con el contrato `AE1E7462…`. El Worker del intento 1 (`R20261120T055235Z-a45b`) paró antes del RED, sin commits.
- Con el esqueleto del §6.2 de la v4.5 había dos CS0121 en pruebas de F4 que no se pueden editar: `WorkspacePilotUpdateRunTests.cs:669` y `:799`. Las sobrecargas nuevas `Execute` y `ComparePost` del modelo completo volvían ambiguo un `null`.
- El Owner eligió «Renombrar los métodos nuevos».
- Después, el barrido ampliado encontró un tercer choque, por reflexión, en `WorkspacePilotPrepareTests.cs:2155-2160` (`Assert.Single(... "Prepare")`). La sesión le aplica el mismo remedio.
- Los nombres nuevos son `ExecuteFull`, `ComparePostFull` y `PrepareFull`.

Ni tu CA-T3b ni la §12 compilaron ni ejecutaron el esqueleto, y es lo que faltó.

**Añade** al final de `hs65\pbt3\independent-review-pbt3b-check.md` una sección «§13. Recomprobación sobre la v4.6». Que tenga ese título exacto, porque `bounded_check` reconoce «Recomprobación sobre la vX.Y». Pon el veredicto en línea propia, «**Veredicto: EMITIBLE …**», y no reescribas lo anterior.

**Entradas:**
- **En `hs65\pbt3\`:**
  - la v4.6, `I-64-f4b-pbt3-gate-contract-package-v46.md` (`09210abe…`, 313 702 B), con su `v46-delta.md` (V46-01 a V46-10), `make_v46.py`, `check_v46.py` y `check-v46-out.txt`;
  - el barrido de sobrecargas y nombres: `overload_sweep.py`, `overload-sweep-eab27a9e.txt`, `name_check_v46.py`, `name-check-v46-eab27a9e.txt` y la R-22 de `pbt3-measurements.md`;
  - el generador de la reemisión, `make_pbt3b_contract.py` (`f8b69f2d…`);
  - el seco, `dry-pbt3b-reissue.txt` y `contract-dry-pbt3b-reissue.json` (`65ad7868…`), con `neg-pbt3b-reissue.txt` (88 de 88);
  - `issue_pbt3b_rev2.py` con su selftest y su `--dry-objects`;
  - `f4b-e45.md`.
- **En `hs65\pbt3bcc\`:**
  - la decisión del Owner y su nota de la sesión: `owner-pbt3b-overload-decision.md`;
  - el análisis, `analysis.md`, con C-1 ya en tres nombres;
  - la compilación de comprobación, en `compile\`:
    - `compile-check.md` y `out-run1.txt` (dos nombres: A reproduce los CS0121 y B compila);
    - `compile_check3.sh` y `out-run3.txt` (tres nombres, la compilación y **la suite completa de Core ejecutada**). Esta todavía corre; mírala al final y, si no está, dilo.
- **En `hs65\pbt3bcc\attempt1\`:** el esqueleto del intento 1, `blocked-skeleton.patch`, y su `worker-handoff.json`.

**Qué comprobar:**
1. **¿La v4.6 aplica fielmente la decisión y su nota?**
   - Que estén los tres nombres, que no cambie ninguna firma existente y que ninguna prueba existente se toque.
   - Que no quede nada del API de la v4.5 en lo normativo de PB-T3b y de PB-T3c.
   - Que `Precheck` se quede (V46-08). ¿Es correcto?
2. **¿El barrido de sobrecargas es completo?** Revisa los `null`, `default` y tipos convertibles, la reflexión, los grupos de métodos, los constructores y las conversiones implícitas. Busca tú también, sobre `eab27a9e` por objetos git, otras pruebas que lean por reflexión PP, UR o la interfaz.
3. **¿Lo que la v4.6 cambiaba solo como texto de referencia para PB-T3b sigue siendo correcto ahora que rige PB-T3b?** Revisa la tabla de RR-3, con la fila del doble y el literal de CT-13, y la nota de M-03.
4. **El delta del seco frente al emitido:** solo `Objective`, C13, las autoridades (de 64 a 66), `IssuedBy` y las revisiones. ¿Es completo?
5. **El régimen de corrección:** CorrectionOf, la base y las precondiciones nuevas (la 12).
6. **¿Hay alguna decisión nueva para el Owner?** En particular: ¿el tercer nombre necesita pregunta o basta la nota de la sesión?
7. **La salida de `out-run3.txt`,** si ya está: ¿solo fallan S-1 a S-6?

**El literal:** propón el `BOUNDED_RESULT` final, que debe nombrar la recomprobación de la v4.6 y pasar el `bounded_check` del generador nuevo.

**Las reglas** son las de siempre, con el prefijo `cat3b_`. El worktree está parado, así que léelo solo por objetos git desde el checkout principal.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.

### 3.1. Complemento enviado durante esta ronda (2026-11-20T07:28:21.547Z)

Mensaje de la sesión, para tu §13: ya está la segunda comprobación. El resumen está en `hs65\pbt3bcc\compile\compile-check.md`, en la sección «Segunda comprobación».
- **`out-run3.txt`:** con los tres nombres compilan los tres proyectos, y la suite completa de Core pasa entera, 19 107 de 19 107, con 0 fallos.
- **`out-run3ctrl.txt`:** es el control negativo. Con `Prepare` en lugar de `PrepareFull` falla exactamente la prueba de reflexión `WorkspacePilotPrepareTests.ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing`, 1 de 91.
- **Sobre D-1:** sin los `Skip`, S-1 a S-6 también pasan sobre el esqueleto inerte. Mi frase del análisis, «solo pueden fallar S-1 a S-6», se cumple con 0 fallos. Si prefieres que la redacte de otra forma, dilo.
