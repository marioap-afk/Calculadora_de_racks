# I-64 F4B — PB-T2: de la v2.7 a la v2.8 del paquete (la verificación con una reparación del RED)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **Las recomprobaciones de la v2.7:**
  - la §11 de `independent-review-pbt2b-v23-check.md` (**CA-T2b**): EMITIBLE TRAS CORRECCIONES, nuevos B 0 / I 1 / M 4;
  - la §10 de `independent-review-pbt2c-v25-check.md` (**CA-T2c**): EMITIBLE TRAS CORRECCIONES, nuevos B 0 / I 1 / M 3.

  Sin decisiones nuevas para el Owner. La importante es la misma en las dos: falta la segunda mitad de RR-7, que la verificación del Controller lea la reparación como parte de la entrega.
- **El mensaje de la sesión:**
  - corregir I-01 en el §6.0, en la autoridad F4B-E35 de los contratos y en lo que el contrato dice a la verificación, como F4B-E25 hizo con 16.3;
  - las menores M-01 a M-04;
  - `BOUNDED_RESULT` no cambia hasta la §12 de CA-T2b y la §11 de CA-T2c.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v28.md`, con el registro v2.7 → v2.8 (V28-01 a V28-06) delante de los anteriores;
- este archivo;
- los dos generadores en la v2.8. El de PB-T2b recibe también el `bounded_check` robusto de PB-T2c;
- sus ejecutores, con pruebas en negativo nuevas;
- `issue_pbt2b.py` y sus plantillas, con la v2.8 en la custodia de X.

**Resultado:** el diseño no cambia, ni lo que se informa al Owner.

## 1. Los cambios

| Id | Fuente | Qué cambia | Dónde |
|---|---|---|---|
| **V28-01** | CA-T2b §11 I-01; CA-T2c §10 I-01 | **La verificación con una reparación (RR-7).** Cómo se leen las comprobaciones de AUTOMATION_PLAN 16.9 que la tocan:<br>- **la n.º 10 (`Tests`):** acredita el RED con `RedSha` y la CI del RED original, con sus TRX. Su cláusula «`git diff --name-only RedSha CurrentSha` no toca `RT` ∪ `ChainRedFiles`» se comprueba desde `RedRepairSha`;<br>- **la n.º 11 (`Trailer`):** incluye el commit «RED repair»;<br>- **el Controller comprueba** RR-1 (uno solo, hijo directo de `RedSha`, antes del GREEN), RR-2 (solo clases que añadió el RED), RR-4 (la CI del RED reparado), RR-5 (declarado) y RR-6 con el tramo `RedSha..RedRepairSha`. En PB-T2c, además, que ese tramo es solo la clase nueva y no toca W ni WC, aunque el GREEN los revierta. Si algo falla, es C18.<br>Va en el §6.0, en la autoridad F4B-E35 de los contratos y en una entrada nueva de la `ExpectedEvidence` | §6.0, «La verificación con una reparación del RED»; los dos contratos |
| **V28-02** | CA-T2b §11 M-01; CA-T2c §10 M-01; la sesión | **La ayuda mecánica de RR-3, en dos grados** (precisión de la sesión):<br>- **parada C07, una violación cierta de RR-3:** que una línea literal que el paquete fija para la clase reparada no siga byte a byte en ella. En PB-T2b, las tres listas de CT-PBT2B-03 y las líneas fijas de CT-PBT2B-01 a 07. En PB-T2c, las 49 líneas de CT-PBT2C-03 y las fijas de CT-PBT2C-01 y 02, comparadas entre `RedSha` y `RedRepairSha`. PBT2x-C07 lo dice;<br>- **hallazgo para la revisión, no parada:** los indicios heurísticos, que pueden dar falsos positivos, como una línea que solo se mueve de sitio. Son una cadena `<campo>=` añadida o quitada en `git diff -U0 RedSha RedRepairSha`, o un cambio en el primer argumento de un `Assert.Equal`. Van a Findings y a la revisión, con la declaración de RR-5 | §6.0; PBT2x-C07; la `ExpectedEvidence` de cada contrato |
| **V28-03** | CA-T2b §11 M-02; CA-T2c §10 M-02 | **El `Trailer` de la entrega** es el de los commits RED, «RED repair» (si lo hay) y GREEN | §6.0, la entrega |
| **V28-04** | CA-T2b §11 M-03; CA-T2c §10 M-03 | **La parte positiva de RR-3** llega a C07 («changes only the when or the how of a reading or a gesture the package already prescribes») y a C18 («or anything other than … (RR-3)»). C07 dice además que es esta parada una línea literal que el paquete fija para la clase reparada y no sigue byte a byte en ella (V28-02) | §6.0, «En el contrato»; los dos contratos |
| **V28-05** | CA-T2b §11 M-04 | **El `bounded_check` de PB-T2b** es ya la función robusta de PB-T2c, con el revisor `CA-T2b`: secciones sin número, como «§R», y un error claro si no se emparejan. Lleva sus pruebas en negativo | `make_pbt2b_contract.py` |
| **V28-06** | La sesión | **La custodia de PB-T2b** gana la v2.7 en la historia y `v28-delta.md`, y PB-T2b se emite con la v2.8 | §6.0, «Custodia» |

## 2. Los contratos

**En los dos contratos:**
- C07 y C18 cambian de texto (V28-04);
- la entrega nombra el trailer del commit «RED repair» (V28-03);
- la autoridad F4B-E35 añade cómo la lee la verificación (V28-01);
- la `ExpectedEvidence` gana una entrada, de 9 a 10, con lo que lee la verificación (V28-01) y la ayuda de RR-3 de su subtarea (V28-02);
- la autoridad del paquete nombra el registro V28.

**PB-T2b:** `CUSTODY_EXTRA` gana `v28-delta.md` y pasa a 10.

**PB-T2c:** `v28-delta.md` pasa a AVISO «de la emisión de PB-T2b».

**No cambian:** el alcance, los invariantes, el número de paradas, los RequiredTests ni la línea base.

## 3. Los generadores

- **El bloque común de RR** sustituye al de la v2.7 en los dos. Cambian C07, C18, la custodia y la entrega, y lleva además:
  - las nueve líneas de la verificación, literales: la cabecera, la n.º 10, la n.º 11, lo que comprueba el Controller, lo propio de PB-T2c, las dos ayudas de RR-3, los indicios y la línea del contrato;
  - el resumen para la autoridad;
  - `rr28_check`, para la cabecera y el registro V28, con la fuente de cada fila.
- **Las ventanas:**
  - la cabecera pasa a las 20 primeras líneas;
  - la fila de [OD], a las 24.

  Al escribir, el paquete de HEAD lleva delante las líneas de emisión.
- **`bounded_check` en PB-T2b:** con las recomprobaciones de la v2.7, la §11 de CA-T2b y la §10 de CA-T2c tienen una importante cada una. Así, el informe entero no pasa, y es lo esperado hasta la §12 y la §11. Los ejecutores lo prueban:
  - el informe cortado antes de la recomprobación nueva vale;
  - el informe entero da error.

## 4. Lo que no cambia, a propósito

- **`BOUNDED_RESULT`:** lo pone la sesión tras la §12 de CA-T2b y la §11 de CA-T2c.
- **El texto de RR-1 a RR-7 y las lecturas de la v2.7 (D-1 y D-2):** aceptadas por la sesión.

## 5. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`17872be8`, `5ddeb3b4` y `b29551ae`), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura: `_work/c/make_v28.py`, `patch_v28.py`, `patch_runs_v28.py` y `patch_issue_v28.py`.
- **Ni dotnet ni git de escritura.**
