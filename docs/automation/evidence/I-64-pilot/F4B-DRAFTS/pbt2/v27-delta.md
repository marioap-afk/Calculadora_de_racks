# I-64 F4B — PB-T2: de la v2.6 a la v2.7 del paquete (la reparación del RED, F4B-E35)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **La decisión del Owner F4B-E35, pregunta 4** (`F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`): «Sí, con esas condiciones (Recomendado)». Fija la regla operativa de la reparación del RED, RR-1 a RR-7. Es regla de F4B, empieza en PB-T2b y excepciona AUTOMATION_PLAN 16.8 solo en sus condiciones.
- **El mensaje de la sesión:**
  - incorporar RR en PB-T2b y PB-T2c, con filas V27 de fuente «F4B-E35 (Owner), RR», sin cambiar nada más del diseño;
  - pasar los dos generadores a la v2.7. Eso revierte M-05 (V26-06) solo en la versión: PB-T2b se emite ya con la v2.7;
  - la numeración nueva:
    - F4B-E34, el cierre de PB-T2a;
    - F4B-E35, las decisiones Q3 y Q4, en el mismo commit de custodia del cierre;
    - F4B-E36, la emisión de PB-T2b;
  - los datos del cierre de PB-T2a:
    - `b29551ae`, hija de `5ddeb3b4`;
    - la CI `44480898605`: Core 19 092 y UI 2 439, con 2 418 pasadas y 21 omitidas;
    - `T2A_CLOSE_FACTS`.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v27.md`: la v2.6 con el registro v2.6 → v2.7 (V27-01 a V27-07) delante de los anteriores;
- este archivo;
- los dos generadores en la v2.7, con un bloque común de RR, y sus ejecutores con pruebas en negativo nuevas por RR;
- `issue_pbt2b.py` con la numeración nueva, la plantilla `f4b-e36.md` (antes `f4b-e35.md`) y las de las secciones 258 y 259 al día.

**Resultado:**
- El diseño no cambia, ni lo que se informa al Owner.
- Los contratos de PB-T2b y PB-T2c ganan:
  - la excepción RR en su C07;
  - una parada C18;
  - la cadena RR-6 en su invariante 02;
  - la CI del RED reparado y lo que declara la entrega;
  - la autoridad F4B-E35.

## 1. Los cambios

| Id | Fuente | Qué cambia | Dónde |
|---|---|---|---|
| **V27-01** | F4B-E35 (Owner), RR | **La regla RR en el §6.0,** para PB-T2b y PB-T2c: un párrafo con RR-1 a RR-7 y los textos ASCII literales de C07 y C18. Ver §2: dos lecturas propias de estas subtareas | §6.0, «La reparación del RED» |
| **V27-02** | F4B-E35 (Owner), RR | **Las paradas:**<br>- PBT2x-C07 = el texto de PBT2A-C07 + «; » + la excepción RR, que dice también lo que sigue siendo parada: un segundo error de escritura en el intento (RR-1) y una reparación que necesita cambiar una línea esperada (RR-3);<br>- PBT2x-C18, nueva: una reparación que rompe RR-1 a RR-6. La regla no aplica, y la reparación cuenta como tocar `tests/` tras el RED (16.8): STOP.<br>«Las propias comunes» las nombran | §6.0; los dos contratos |
| **V27-03** | F4B-E35 (Owner), RR | **La cadena RR-6** en INV-PBT2B-02 e INV-PBT2C-02:<br>- `RedSha..RedRepairSha` solo toca las clases de prueba que el RED añadió;<br>- el criterio se repite entero sobre `RedRepairSha`;<br>- `RedRepairSha..CurrentSha` es el GREEN del contrato.<br>En PB-T2c, el criterio repetido incluye las corridas aisladas de INV-PBT2C-03 con la clase reparada | §6.2; §6.3 |
| **V27-04** | F4B-E35 (Owner), RR | **La CI del RED reparado (RR-4):** un párrafo en «La CI del RED», que el contrato añade a esa evidencia.<br>«El RED» y «en local» de la `ExpectedEvidence` común piden el commit «RED repair» y su push, su CI, los TRX y los filtros sobre `RedRepairSha` | §6.0 |
| **V27-05** | F4B-E35 (Owner), RR | **Lo que declara la entrega (RR-5):**<br>- `RedRepairSha`;<br>- la prueba y la línea corregidas;<br>- el error y por qué no cambia ninguna línea esperada;<br>- el diff;<br>- `RedSha`, que sigue siendo el commit RED original | §6.0, la entrega |
| **V27-06** | F4B-E35 (Owner), RR | **La autoridad F4B-E35:**<br>- una fila en el §1;<br>- una autoridad en los dos contratos, AVISO hasta que esté en la revisión;<br>- `rr_check` comprueba el archivo cuando está en la revisión: la respuesta del Owner a la pregunta 4, la regla, que empieza en PB-T2b, y RR-1 a RR-7, cada una una vez | §1; los dos generadores |
| **V27-07** | F4B-E35 (Owner), RR; la sesión | **La versión y la custodia:**<br>- PB-T2b se emite ya con la v2.7;<br>- su custodia gana la v2.5 y la v2.6 en la historia, `v26-delta.md` y `v27-delta.md`;<br>- la de PB-T2c conserva el encargo de CA-T2c y de su recomprobación;<br>- cambian el **Estado** y la cabecera | §6.0, «Custodia»; cabecera |

## 2. Dos lecturas propias de PB-T2b y PB-T2c

RR está escrita para un RED que solo toca `tests/`. En PB-T2b y PB-T2c el RED ya toca `src/`:
- en PB-T2b, la costura inerte de W;
- en PB-T2c, los tres mutantes de W y WC.

Por eso el paquete lee dos tramos así (V27-01):
- **RR-1, «antes del primer commit que toque `src/` (el GREEN)»,** se lee «antes del commit GREEN»;
- **RR-6, «BaseSha..RedSha, solo `tests/`»,** se lee «BaseSha..RedSha, el RED de cada contrato».

**RR-2 sigue igual:** la reparación solo toca archivos de `tests/` que el RED añadió. Nunca la costura inerte ni los mutantes, que son `src/`.

## 3. Los contratos

**PB-T2b:**
- `StopConditions`: 34 → 35 (PBT2B-C18);
- `Authorities`: 44 → 45 (F4B-E35);
- cambian el texto de PBT2B-C07, INV-PBT2B-02, tres líneas de la `ExpectedEvidence` y la de la CI del RED.

**PB-T2c:**
- `StopConditions`: 32 → 33 (PBT2C-C18);
- `Authorities`: 49 → 50 (F4B-E35);
- cambian los mismos textos, con INV-PBT2C-02;
- `CUSTODY_EXTRA` pierde `v26-delta.md`, que ahora custodia la emisión de PB-T2b;
- `v26-delta.md` y `v27-delta.md` pasan a AVISO «de la emisión de PB-T2b».

**No cambian:** el alcance, los demás invariantes y paradas, los RequiredTests ni la línea base.

## 4. Los generadores

- **El bloque común de RR,** idéntico en los dos:
  - la autoridad;
  - los textos ASCII de C07 y C18;
  - las nueve líneas del §6.0, plegadas;
  - la línea de las propias comunes;
  - la de la custodia;
  - las tres líneas de la `ExpectedEvidence`;
  - la fila del §1;
  - la fuente del registro;
  - `rr_check`, extraíble.
- **Lo que comprueban de la v2.7:**
  - esas líneas, literales;
  - el invariante 02 de su subtarea;
  - la cabecera de la v2.7;
  - el registro V27-01 a V27-07, con la fuente en cada fila.
- **`make_pbt2b_contract.py` lleva ya los datos del cierre de PB-T2a:**
  - `PT2A_CLOSE` = `b29551ae`;
  - `PT2A_CLOSE_ENTRY` = F4B-E34;
  - `CI_BASE_T2A` = (`44480898605`, `5ddeb3b4`, 19 092, 2 439, 21).

  Su seco real es sobre `b29551ae`.
- **`make_pbt2c_contract.py`:** la ventana de [OD] pasa de 20 a 22 líneas, porque delante irán las líneas de emisión de PB-T2b y de PB-T2c.

## 5. `issue_pbt2b.py`

- **La numeración:**
  - la emisión es F4B-E36;
  - antes de escribir, `f4b-dispositions.md` tiene `## F4B-E34.` y `## F4B-E35.`, no tiene `## F4B-E36.`, y su última entrada es F4B-E35.
- **La plantilla** `f4b-e35.md` pasa a ser `f4b-e36.md`, con un párrafo de RR y otro de la v2.6 y la v2.7. La evidencia sigue en la §258 y la §259.
- **La custodia de X:**
  - el paquete v2.7 con su línea de emisión;
  - en la historia, `package-v25.md`, `package-v26.md`, `v26-delta.md` y `v27-delta.md`, además de lo de antes.
- **Los marcadores, ya puestos:**
  - `CLOSE_T2A` = `b29551ae`;
  - `T2A_CLOSE_FACTS`, en una línea;
  - `STOP_N` = 35.
- **El estado:**
  - las tres sustituciones parciales siguen encontrando su texto en las líneas que dejó el cierre: `f4b_status`, `GATE` y `REMAINING_BUDGET`;
  - el `NEXT_ACTION` de X ya dice que PB-T3 va tras el cierre de PB-T2c por la autorización condicional (F4B-E35) y que el Worker puede reparar el RED.
- **El `--selftest`** lee por objetos git de `b29551ae`:
  - el estado real, en el que las ocho líneas del cierre están una vez cada una y se sustituyen;
  - `f4b-dispositions.md`;
  - la evidencia;
  - lo custodiado en `pbt2/`;
  - F4B-E35.

## 6. Lo que no cambia, a propósito

- El texto de RR no cambia: el paquete lo cita y lo aplica, y las dos lecturas del §2 van declaradas.
- PB-T2a, ya emitida con la v2.2, no gana RR.
- `BOUNDED_RESULT` no cambia todavía en ningún generador. La recomprobación de la v2.7 la pide la sesión, y `bounded_check` exigirá que el literal la nombre.

## 7. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal: `17872be8`, `5ddeb3b4` y `b29551ae`, este por el mensaje de la sesión. Sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura: `_work/c/make_v27.py`, los parches `patch_*_v27.py` y los ayudantes de `_work/c/`.
- **Ni dotnet ni git de escritura.**
