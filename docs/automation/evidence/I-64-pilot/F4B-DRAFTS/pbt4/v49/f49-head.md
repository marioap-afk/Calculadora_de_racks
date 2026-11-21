**Estado:** v4.9. Corrige la parte de **PB-T4b** con la comprobación acotada **CA-T4b** (`independent-review-v1.md`, §20: EMITIBLE TRAS CORRECCIONES, 0/1/8) y con la **séptima tanda del Owner**, el coste de la lectura (`owner-pbt4b-cost-decision.md`, sha256 `@@OWNERCOST@@`: «Darlo por cumplido (Recomendado)»). Los registros de la sesión van en `session-pbt4b-design-decisions.md` (sha256 `@@SESSION@@`, con M-03 a M-06 y la corrección de I-01 de la CA-T4b).

**Lo que cambia:**
- **CA-T4b I-01** (V49-01): el equivalente de WHHG `NoneOfThe30…` trae, tras su `Assert.Empty`, **la autoprueba de la guarda** (WHHG:501-526: sus dos variables y sus 13 `Assert.NotEmpty`, literales), sobre su propio `LiteralViolations` y su tabla con 0, 6 y 0. Es lo que hace el equivalente de G-4 con la autoprueba de IG:510-517.
  - **Por qué:** la autoprueba original pasa en vacío en el GREEN. Su tabla (2, 5 y 1) ya no casa con el anfitrión, así que `LiteralViolations` sin mutar ya da tres violaciones y sus `NotEmpty` se cumplen con cualquier mutación.
  - **Lo que cambia:** solo el `CopySha` de `WorkspaceHostHardeningGuardFull.cs`. No cambian la lista, los totales ni la partición;
- **M-01 a M-04 de la CA-T4b** (V49-02 a V49-05), de texto;
- **el coste de la lectura** (V49-06): el Owner lo da por cumplido con dos de tres corridas de la v4.8. **La v4.9 conserva esas corridas,** porque no cambia ninguna de sus entradas (comprobado por sha256 en la precondición 6).

**Lo demás no cambia:**
- la lista de 478, el mapa v5 y el GREEN;
- el esqueleto y el kit;
- D-S1 a D-S6;
- PB-T4a.

**La precondición 6 repite los pasos 1 a 6** con los guiones de la v4.9, en copias nuevas (`/d/r4u` en adelante). Sus resultados van en `pbt4bcc/compile/`.

**Lo que sale de la v4.9 (PB-T4b):**
- las ediciones (`pbt4b_hand_edits_v49.py`);
- el guion de la copia (`make_copies_pbt4b_v49.py`) y su revisión (`pbt4b-equivalents-review-v49.txt`, 114 entradas);
- el RED (`apply_red_pbt4b_v49.py`);
- la autoprueba (`selftest_pbt4b_v49.py`, `v49/selftest_pbt4b_v49-out.txt`);
- y `make_v49.py`, que arma esta versión desde la v4.8.

Los `Skip`, la partición y las comprobaciones son los de la v4.8.

---

## Registro de cambios v4.8 → v4.9

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V49-01** | **CA-T4b I-01** (importante): la autoprueba `TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier` (WHHG:501-526) **pasa en vacío en el GREEN**. La v4.8 daba otra razón («es una autoprueba que inyecta textos sobre el anfitrión y pasa», §4.2) | **Como G-4:** el equivalente de `NoneOfThe30…` (`WorkspaceHostHardeningGuardFull`) trae, tras su `Assert.Empty`, las 13 `Assert.NotEmpty` de la autoprueba, con sus dos variables (`host` y `support`), sobre su `LiteralViolations` y su tabla con 0, 6 y 0. Su texto literal va en el anexo V49-A. **La CA-T4b (§20.10) dice «las 14 `NotEmpty`»: en WHHG:501-526 de `78a7cef3` son 13, y van todas** (`selftest_pbt4b_v49.py` las compara, línea a línea, con el objeto git). §4.2 declara el vacío de la original y corrige la razón. No cambian la lista, los totales ni la partición: en el RED, el `Assert.Empty` cae antes (Red, 1 resultado). Cambia el `CopySha` de esa copia | §4.2; D-PBT4-11; el anexo V49-A |
| V49-02 | CA-T4b M-01 | Declarado en §4.2: las autopruebas de `HostViolations` de PEG que quedan fuera de la lista (PEG:401-416, :418-431, :433-451, :453-468, :470-492 y :516-526; 6 métodos, 31 resultados) **también pasan en vacío en el GREEN**, con la mitigación de la §20.10 | §4.2 |
| V49-03 | CA-T4b M-02 | **Los nombres de las dos copias de WPP de D-S1** («…NeverErasesAView…», «…MakesTheUpdateInvalid…») dicen lo contrario de lo que afirman. Se conservan por el mecanismo (CT-20 (2) y la partición), y **la regla es la del cuerpo**. Va en V48-02 y en §8.3 | V48-02; §8.3 |
| V49-04 | CA-T4b M-03 | En §4.2, WCAG: `ReasonTable` es WCAG:408-419, y las filas que cuenta el paseo son :415-419 (la v4.8 citaba :408 y :447) | §4.2 |
| V49-05 | CA-T4b M-04 | **«`AtOrigin`, NH, `ReadAt` y WHC no cambian»** quiere decir que **D-S4 no los toca** (D-PBT4-21): WHC sí cambia en el GREEN de PB-T4b, por D-PBT4-04 y D-PBT4-08. Corregido en D-PBT4-03. La fila V45-02 de la cabecera de la v4.5 queda como estaba, con esta aclaración | D-PBT4-03; V45-02 (historia) |
| V49-06 | **La séptima tanda del Owner: el coste de la lectura** | «Darlo por cumplido (Recomendado)» (`owner-pbt4b-cost-decision.md`): dos de tres corridas de la sonda de la v4.8 cumplen todo. **La v4.9 conserva el paso 7 de la v4.8** (CT-06 en `/d/r4s` y el coste en `/d/r4s` y `/d/r4t`): la precondición 6 comprueba por sha256 que no cambia ninguna de sus entradas (el GREEN de `src/`, las sondas, el kit y las copias que usan). `pbt4b-precondition6.json` lleva el campo `Cost`, que remite a esas corridas y a la decisión | §6.2.1, paso 7; §8.5; §11 |
| V49-07 | La versión | El `Objective` de PB-T4b cita la v4.8 → **v4.9** y «sus siete tandas». La cabecera cita los registros vigentes (la v4.8 citaba un sha256 viejo del de la sesión, `f8518e78…`). **Las corridas de la v4.9:** `/d/r4u` en adelante | §6.2.1; §11 |

---
