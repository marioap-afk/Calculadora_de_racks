# Encargo: anclajes de PB-T1b sobre el cierre de PB-T1a y generador del contrato de PB-T1b

Lo da la sesión principal al redactor del paquete de PB-T1.

**Dónde está la rama:**
- PB-T1a está **CLOSED / PASS WITH LIMITATIONS**: GREEN `73d30982`, custodia del cierre `5043b49a` (F4B-E14, F4B-E15 y F4B-E16).
- La rama está en `5043b49a`; `git diff a3b63ccc 5043b49a -- src` está vacío (PB-T1a no cambia producto).
- **Ningún Worker trabaja ahora en el worktree:** puedes leerlo, siempre en solo lectura.

**La autoridad:** el paquete custodiado `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.4, en concreto el §6.0 (lo común), el §6.2 (el contrato de PB-T1b), el §5.3 (CT-PBT1B-01 a 16 y el esqueleto) y D-PBT1-02 a D-PBT1-11 y D-PBT1-15 (D2, con las reglas de G-L1 de la v2.4).

**Las reglas:**
- solo lectura en el worktree;
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc;
- `gh`, solo de lectura.


## La decisión nueva del Owner que entra en PB-T1b (F4B-E14, pregunta 2)

**Texto:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1a-decisions.md`, pregunta 2: «Como hoy, regla explícita (Recomendado)».

**El hecho, medido por PB-T1a** (CT-PBT1A-02; `PushBackStateNetRackControlsTests.cs`):
- un rack con `Structure.PalletWeightUnit = "ton"` la conserva al cargar y con «Actualizar»;
- cuando otra edición reconstruye la estructura (`DepthBox=54`), la salida pasa a `"kg"` con el mismo `PalletWeight`;
- la caja `WeightUnitBox` solo ofrece kg y lb, así que decide el estado al reconstruir.

La revisión independiente de PB-T1a (OWNER-2) señala que esto contradice HT1-15 («solo `PostBox` decide el estado») y la medición E-5.

**Lo que pide:** igual que c-02 (D-PBT1-09), PB-T1b reproduce el comportamiento de hoy como **una regla explícita del estado**, con su prueba. RACKEDITAR no cambia.

**Tu tarea con eso, en la v2.5:**
- corrige HT1-15 y el mapa de controles del §5.5 (`WeightUnitBox` decide el estado al reconstruir; c-09);
- diseña la regla, como D-PBT1-09 hace con c-02: dónde vive en `PushBackEditableState` y en qué paso de la salida se aplica, sin cambiar el orden de la carga (D-PBT1-03);
- añade su CT a la lista cerrada de PB-T1b (por ejemplo, CT-PBT1B-17, con líneas literales tomadas de CT-PBT1A-02) y a los recuentos de `RequiredTests` y de la CI del §6.2;
- revisa si toca a PB-T1c (CT-PBT1C-02, «ningún control fuente de verdad salvo…»);
- clasifica cada cambio con L-6.

Lo que no puedas decidir con D-PBT1-09 como patrón es una decisión nueva: dilo y no la tomes.

**Lo demás que dejó PB-T1a** (F4B-E14, pregunta 3; registrado en `docs/ideas-futuras.md`): «lb» y `PalletsDeepBox`, que no llegan solos a la salida, y el portador de botas que añade «Seguridad» sin cambios. PB-T1b los reproduce tal cual, a través de la red de PB-T1a; no necesitan una regla propia. Compruébalo en el diseño de las cargas y de la salida.

**La precondición 5** (la autoridad de PB-T1a) queda satisfecha por F4B-E14. Cítalo en la v2.5.

## Qué haces

1. **Los anclajes de PB-T1b (§6.0, precondición 4)** sobre `5043b49a`:
   - `git diff 5043b49a -- src` vacío frente a la base de la emisión de PB-T1b;
   - las anclas por miembro y línea de ES, RMES, CS, DA y CA que citan D-PBT1-02 a D-PBT1-11 y el esqueleto del §5.3;
   - las reglas de G-L1 de la v2.4 sobre los archivos de su lista: 0 coincidencias hoy;
   - los recuentos de `RequiredTests` del §6.2 (incluidas las 14 pruebas de `PushBackStateNet` que deja PB-T1a);
   - la línea base de la CI: el último run verde de la rama, medido con `gh`, frente a la previsión del cierre de PB-T1a.

   Escríbelo en un apartado nuevo **P** de `pbt1-measurements.md`, con las órdenes y su salida, y deja la salida de las órdenes en `pbt1/pbt1b-anchors-out.txt`.
2. **Clasifica cada diferencia** con L-6 (§8.4): «no material» o «material». Si algo es material, dilo claramente: la emisión de PB-T1b se para y vuelve a la sesión.
3. **El texto del paquete:** el `Objective` del §6.2 y el del §6.3 citan todavía «el paquete del contrato de PB-T1 v2.2». Si hace falta una v2.5 para corregir eso o lo que salga de los anclajes, cópiala a `I-64-f4b-pbt1-gate-contract-package-v25.md` sin tocar la v2.4, con su registro de cambios v2.4 → v2.5, y limita los cambios a lo necesario.
4. **El generador `pbt1/make_pbt1b_contract.py`,** siguiendo a `make_pbt1a_contract.py`:
   - `TaskId` `F4B-PBT1B-LOSSLESS-STATE-TYPE`;
   - el `AllowedWriteScope` (12);
   - el `ForbiddenWriteScope` por el método de [PKC-C]: 117 de `src/`, más lo común del §6.0 para `tests/` y la raíz;
   - los `RequiredTests`, los invariantes (los propios del §6.2 y los comunes 07 a 12 y 14 con el prefijo PBT1B);
   - `ExpectedEvidence`;
   - las paradas: las 17 estándar, las de [DP] literales (C08, C11 y C12, con el prefijo PBT1B), las comunes, C04, C13, C14, C15 y C17;
   - las celdas e `IssuedBy`;
   - un `assert` sobre el texto exacto del esqueleto del §5.3 si se puede anclar en HEAD; si no, dilo;
   - `CUSTODY` con lo que se custodia en la emisión de PB-T1b: la salida de los anclajes, la comprobación acotada `independent-review-pbt1b-check.md` y, si la hay, la v2.5.

   Ejecuta solo `python make_pbt1b_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`, que no escribe nada.

**Tu respuesta final** es corta:
- las rutas;
- la salida del `--dry`;
- la lista de diferencias con su clase;
- **cualquier diferencia material**;
- lo no medido.
