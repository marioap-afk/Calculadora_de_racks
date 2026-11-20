# I-64 F4B PB-T3b — Comprobación acotada (CA-T3b) del contrato en seco sobre el paquete v4.5

**Veredicto: EMITIBLE. B 0 / I 0 / M 4.**

**En breve:**
- **El contrato en seco es fiel a la v4.5 para PB-T3b** (§6.0 y §6.2) y a lo que ya aceptó PB-T3a. Las 17 paradas estándar y las comunes (C07, C18, C08, C12, C17) son, letra a letra, las del contrato reemitido de PB-T3a. C13, C14 y C16 son las del paquete. El alcance, las 11 rutas, `UNIT_CHANGE` y el `ForbiddenWriteScope` coinciden con lo que mido yo.
- **D-1 está bien anclado:** S-1 a S-6, en sus métodos y sin `Skip` en la base, diez filas.
- **RR en PB-T3b se lee bien:** la reparación no toca `src/` ni los tres archivos de guardas, y sí puede tocar el doble. La tabla de RR-3 basta sin una v4.6, aunque tres de sus entradas se sacan del §5.3 (M-02).
- **C11 es fiel a F4B-E40 y a F4B-E43.** Basta sin v4.6, porque las dos decisiones son autoridades del contrato y prevén que los contratos de PB-T3 nombren los sustitutos. Pero el §6.0 del paquete queda desfasado para PB-T3b y PB-T3c (M-01). `ExpectedEvidence[0]` es, entero y exacto, el literal de PB-T3 de `F4B_EXACT_EVIDENCES` del sustituto v2.
- **La remedición sobre el cierre de PB-T3a no cambia nada material.** El 99 frente a 98 es la carpeta `Draft/`, y el barrido no encuentra ninguna colisión nueva.
- **La Snapshot K es fiel.**
- **Las CT de PB-T3b no tienen gestos inertes ni lecturas a destiempo.**
- **No hay decisiones nuevas para el Owner.**

**`BOUNDED_RESULT` propuesto (ASCII):**

`EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner`

## 1. Método y límites

**Qué se leyó:**
- **El paquete v4.5,** el custodiado en `5bda5569` (sha256 `64ab03df…`, 282 234 bytes). Es la v4.5 de la §11 de CA-T3a más V45-06, que aplicó su M-06. Para PB-T3b miré el §5.0, el §5.3, el §6.0 y el §6.2.
- **El generador** `make_pbt3b_contract.py`, con `issue_pbt3b.py` y su autoprueba.
- **El seco:** `dry-pbt3b.txt`, `contract-dry-pbt3b.json` (sha256 `5EE38DAB…`, 72 423 bytes) y `neg-pbt3b.txt` (68 de 68).
- **La remedición:** `measure_pbt3b.py`, `pbt3b-anchors-out.txt`, `sweep-pbt3b-close.txt` y la R-21 de `pbt3-measurements.md`.
- **Las dudas** de `generator-doubts-pbt3b.md`.
- **Los dos sustitutos,** con sus límites declarados.
- **El contrato reemitido de PB-T3a** (`485b5651`; es el mismo en `c40eea4a`).
- **El código y las pruebas,** por objetos git en `c40eea4a`.

**Lo que no se hizo:**
- No se leyó nada del worktree. No se ejecutó dotnet, y ninguna orden git escribió.
- La r2 del seco (`*-r2.*` y `delta-r2.txt`) ya está en la carpeta, pero este informe es sobre el seco original, como pide el encargo. La r2 la compruebo cuando se pida.

**Los guiones de esta ronda** (Python con `-I`, escritos con la herramienta de archivos; el checkout va por argumento, así que no llevan rutas locales):
- `cat3b_inspect.py`: el seco frente a PB-T3a;
- `cat3b_c11.py`: C11 y las entradas F4B-E40 y F4B-E43;
- `cat3b_c13.py`: C13, C14 y C16 frente al §6.2;
- `cat3b_evid.py`: la evidencia, entrada por entrada;
- `cat3b_nc2.py`: la propiedad 8 del sustituto v2, por su AST, sin ejecutarlo;
- `cat3b_auth.py`: las `Section` de las autoridades;
- `cat3b_remeasure.py`: la remedición independiente;
- `cat3b_bounded.py`: el `bounded_check` del generador sobre este informe (§11);
- y, de la ronda anterior, `cat3a_sweep.py` y `cat3a_ctx.py`, corridos sobre `c40eea4a`.

**Las salidas:** `cat3b_inspect_out.txt`, `cat3b_c11_out.txt` y `cat3b_sweep_close_out.txt`. No son guiones, aunque llevan el prefijo. Queda declarado.

## 2. Fidelidad del contrato en seco frente a la v4.5 (punto 1)

| Campo | Estado |
|---|---|
| Identidad | `TaskId` `F4B-PBT3B-FULL-WRITE-RUN` (24 caracteres). El contrato es ASCII. Las celdas son las de PB-T3a |
| `Objective` | El del §6.2, con «v4.5» |
| `AllowedWriteScope` (11) | PP, UR y el nuevo `Pilot/IPushBackPilotViewEraser.cs`; las cuatro clases de §5.3 y `PushBackFullWriteSupport.cs`; y los tres archivos de guardas de F4, solo para los `Skip` |
| `UNIT_CHANGE` | PP y UR, como autoridades `UNIT_CHANGE` que citan D-PBT3-09 y D-PBT3-10, §6.0 y F4B-E25 |
| `ForbiddenWriteScope` (294) | **`src/`, 99.** Lo calculé con el algoritmo de [PKC-C] sobre los 915 archivos de `src/` en `c40eea4a`: la cobertura calculada es, como conjunto, la del seco. No deja ningún archivo sin cubrir, no cubre ningún permitido y ninguna entrada está vacía. La entrada de más frente a las 98 de la nota del §6.0 es `src/RackCad.Application/Systems/PushBack/Draft/`, que no existía cuando se midió (duda 1: correcto y no material).<br>**`tests/`, 178.** Core 109: las 28 de PB-T3a sin el prefijo `Workspace/`, los 76 de `Workspace/` (79 menos los tres de D-1) y las 5 clases de PB-T3a. UI 69: 68 más la de paridad de PB-T3a.<br>**La raíz, 17** |
| `Invariants` (15) | INV-PBT3B-01 a 06 y 15, del §6.2. Los comunes 07 a 14 son, letra a letra, los de PB-T3a. INV-PBT3B-02 remite a INV-PBT3A-02 («como»), que está en una autoridad (el contrato de PB-T3a) y cuyo criterio vale para PB-T3b: dos filtros con `ExpectRed` y el de la guarda CT-13 |
| `StopConditions` (34) | **Las 17 estándar:** iguales a las de PB-T3a, P-01 en la K incluida.<br>**C01 a C10, C12, C17 y C18:** las de PB-T3a. C07 es la común, que ya nombra CT-PBT3A-04 y CT-PBT3B-06 y 10. C04 lleva el texto de PB-T3a en lugar de «as PBT3A-C04»; es equivalente.<br>**C13, C14 y C16:** iguales al §6.2, sin las notas (`cat3b_c13.py`: C13, 2 221 caracteres idénticos).<br>**C11:** el §4 |
| `RequiredTests` (31) | Los tres nuevos (13 y 6 con `ExpectRed`, 2 sin él).<br>**`Workspace` de Core, 3 261:** son los `Passed` del RED en cualquiera de las dos lecturas de la TRX. Con la `Theory` S-2 como cinco filas, 3 271 − 10; como una, 3 267 − 6. La nota del §6.2 pide fijarlo con la CI del RED, que no existe al emitir; 3 261 es ese número.<br>**Los 27 heredados,** con sus mínimos: `PushBackFullDraft` pasa a `ExpectRed` false, y la UI de paridad de PB-T3a no entra (M-03) |
| `ExpectedEvidence` (15) | **[0] a [8]:** las de §6.1. [2] lleva la CI del RED de PB-T3b (`ui-tests` en success), y [8] la frase propia de PB-T3b («no toca PP, UR ni `IPushBackPilotViewEraser.cs`, ni los tres archivos de guardas de F4 con los Skip de D-1»).<br>**[9] a [13]:** las cinco viñetas del §6.2.<br>**[14]:** la línea base medida, run `44668584248` sobre `6919e17a`.<br>Hay dos precisiones heredadas (M-03) |
| `Authorities` (62) | Las de la emisión. Las dos decisiones del Owner que cambian C11 están, igual que el sustituto de nc1 con su revisión y sus límites, y el script de nc2 v2. Del nc2 v2 faltan sus límites y su revisión (M-04) |
| Marcadores | El SHA de nc2 v2 (en C11 y en su autoridad), `AuthorityRevision`, `BOUNDED_RESULT`, F4B-E44 y las secciones que trae el commit X (PB-T3b del registro declarado y R-21). Son los previstos |

## 3. D-1 y el `Skip` (punto 2)

**S-1 a S-6, medidos en `c40eea4a`** (`cat3b_remeasure.py`):

| Id | Clase | Atributo, en la línea | Método, en la línea | Filas |
|---|---|---|---|---|
| S-1 | PrepareGuard | `[Fact]`, :121 | :122 | 1 |
| S-2 | PrepareGuard | `[Theory]`, :127 | :133 | 5 `InlineData` |
| S-3 | PrepareGuard | `[Fact]`, :229 | :230 | 1 |
| S-4 | UpdateGuard | `[Fact]`, :126 | :127 | 1 |
| S-5 | IntegrationGuard | `[Fact]`, :895 | :896 | 1 |
| S-6 | IntegrationGuard | `[Fact]`, :902 | :903 | 1 |

- **Lo que coincide:** cada nombre aparece una vez en su clase y ninguno lleva `Skip`, en total diez filas. Coincide con R-21 y con el registro declarado (`superseding-pbt3b.md`), que cita las líneas de atributo.
- **El motivo literal del `Skip` (duda 2): no entra en ningún campo del contrato, y la lectura es correcta.**
  - En el §6.2 es una viñeta suelta, como el esqueleto del RED.
  - «Entra así en el contrato literal» (§5.3, l.1144) se refiere al número F4B-E29, que sustituye al marcador (V41-02).
  - El contrato es ASCII y no podría llevar «decisión».
  - El texto exacto está en el §5.3 y en el registro declarado (igual, comprobado), y la cadena para si el Worker lo escribe de otra forma.
  - El Controller lo ve por INV-PBT3B-01 y por [9], que exigen que el diff de los tres archivos solo lleve las líneas de atributo con `Skip`.
- **El filtro UI de paridad de PB-T3a:** no entra en `RequiredTests`, por la letra de «salvo los de PB-T3a con `ExpectRed`». PB-T3b no toca ni la UI ni `Draft/`, y la CI de UI corre la clase entera, así que no se pierde ninguna red. Pero la fila del §6.2 que reañade `PushBackFullDraft` con `ExpectRed` false se olvidó de su gemela de UI (M-03).

## 4. RR en PB-T3b (punto 3)

**La lectura del §6.0:**
- **Lo que no toca la reparación:** el tramo `RedSha..RedRepairSha` no toca `src/` (PP, UR y la interfaz) ni los tres archivos de guardas de F4. Esos tres los cambió el RED, no los añadió, así que RR-2 los excluye, y un error en un `Skip` es la parada de siempre (§6.0, RR-2, l.1323).
- **Dónde lo dicen:** INV-PBT3B-02, la viñeta «Por subtarea» del §6.0, [8] y C18 («a file the RED did not add»), en el mismo sentido.

**INV-PBT3B-02 frente a RR-2 (duda 4): no hay contradicción.**
- **«Clases de prueba que añadió el RED»** es lo que [8] define como `git diff --name-only --diff-filter=A BaseSha RedSha -- tests/`. El doble `PushBackFullWriteSupport.cs` lo añade el RED, así que se puede reparar, como dice la nota del §6.0.
- **El Owner lo iguala él mismo:** RR-2 de F4B-E35 dice «archivos de `tests/` que el commit RED añadió, es decir, las clases de prueba nuevas».
- **Lo que falta:** el doble no tiene literales en la tabla, así que una reparación que cambiara lo que el §5.0 le prescribe (`EraseUnit:<clave>`, aplicar en el sellado, `PreparedBlockMatches` para todas las claves) solo la vería la revisión (M-02).

**La tabla de RR-3 (duda 3): basta sin v4.6.**
- **Contar los literales entre paréntesis es lo correcto.** La fila dice «da los literales», y `2.0`, `2.5`, `3.0` y `3.5`, y las cadenas de E-2 y E-6, son literales en comillas invertidas.
- **La regla DU-24 de PB-T3a** («lo de entre paréntesis fuera de las comillas es referencia») no los excluía por estar en comillas. Si la nueva lectura deja iguales las filas de PB-T3a, como dice la duda, no hay regresión.
- **Las tres entradas que no se enumeran en la fila** se sacan del §5.3 sin criterio, porque allí están enteras:
  - las cuatro líneas de CT-10: `modulo-anidado …` y `ladoB-anidado …`, de prepare y de corrida;
  - las once `<Fn> prepare=Ready diseno=[<valor>]`, con los once valores de CT-01 (ii);
  - la línea entera de CT-06 (ii), que la fila corta en `motivo=[…]`.

  El seco imprime 4, 11 y 1, y la cadena copia la función del generador. Es la misma clase de defecto que la M-03 de CA-T3a y la M-11 de CA-T2c, en tres entradas (M-02).
- **La codificación:** las tres derivadas llevan caracteres fuera de ASCII («aquí», «versión», «») y la lista de la sesión tiene que guardarlos en UTF-8, aunque el contrato sea ASCII.

**La verificación:**
- **n.º 10 y n.º 11:** van enteras en [8], con las órdenes git y la frase de PB-T3b.
- **RR-6:** va en INV-PBT3B-02:
  - `BaseSha..RedSha`, el RED de PB-T3b: el esqueleto en PP y UR, la interfaz, las pruebas, el doble y los `Skip`;
  - `RedSha..RedRepairSha`, solo los archivos de `tests/` añadidos;
  - `RedRepairSha..CurrentSha`, solo PP y UR.

  Es coherente con el GREEN de INV-PBT3B-01.

## 5. C11, con sus cinco cambios (punto 4)

**La comparación:** `cat3b_c11.py` compara C11 con la de PB-T3a (con el prefijo cambiado) y con F4B-E40 y F4B-E43 (`f4b-dispositions.md` y las dos decisiones, en `c40eea4a`).

| Cambio | Qué dice C11 | Fuente | Fiel |
|---|---|---|---|
| 1. nc2 | `PBT3B-NC2-EXACT-WHITELIST-F4B-01`, «the version 2 of `PBTx-NC2-EXACT-WHITELIST-F4B-01`»: la lista cerrada de literales, una por paquete; el texto revisado sin editar, con `--task PBT3B`; el registro `SATISFIED_UNDER_PBT3B_EXACT_WHITELIST_SUBSTITUTE_F4B`; la cláusula de `inject_opaque.py` | F4B-E43: «de PB-T3b en adelante, el sustituto admite una lista cerrada…, con autoprueba y revisión». El v2 forma el `Id` y el registro con `--task` (`ID_GENERIC.replace('PBTx', tag)`, :458; :508), y `--task PBT3B` se liga al prefijo `F4B-PBT3B-` del `TaskId` | Sí. El SHA es un marcador en el seco: c033e96c… (`limits-declared.md` del v2, igual al del archivo) |
| 2. nc1 | La firma PD-F4B-01, con (a) a (f), y PD-F4-04 como caso particular, hacia `PBT3x-NC1-CASCADE-SIGNAL-01`: SHA-256 `41440edc…` (igual en el archivo custodiado en `c40eea4a`), revisado APTO, con sus límites y con `--task PBT3B`; el registro `SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE` | F4B-E40 (`owner-pbt2c-nc1-decision.md`). Sus (a) a (f) son los de C11, y dice literalmente: «La firma exacta PD-F4-04 sigue valiendo, y es un caso particular de PD-F4B-01». El sustituto forma el registro con su `tag` (:710) | Sí |
| 3. Las diferencias admitidas | Las de [PT1] §6.0 y, para PD-F4B-01, sus (a) a (f) | F4B-E40 | Sí |
| 4. Lo que vuelve al Owner | Las aceptaciones puntuales de nc1 (PB-TC, PB-TC2 intento 1, PB-T2a, PB-T2b y PB-T2c) y la disposición de nc2 de PB-T3a (F4B-E43) no se extienden | F4B-E08, E12, E34, E38, E40 y E43 | Sí |
| 5. El final | «no one-off acceptance extends to PB-T3b» | Lo de siempre | Sí |

**La propiedad 8 del v2** (`cat3b_nc2.py`, leyendo el AST del script, sin ejecutarlo):
- `F4B_EXACT_EVIDENCES` tiene dos literales. `ExpectedEvidence[0]` del seco es igual, entero y exacto, al segundo, el de PB-T3, que es también `ExpectedEvidence[0]` de `485b5651`.
- Así se cumple lo que la revisión del v2 dejaba para cada emisión: el literal de la entrada de la comparación byte a byte está en la lista.

**Una lectura, no un hallazgo.** Con PD-F4-04 «caso particular», la salida de F4 (el sustituto de identidad de F4T3C2) desaparece de C11. Un nc1 que reprodujera la letra de PD-F4-04 sin S-04, o sin el HEAD real en Identity, ahora vuelve al Owner, porque el sustituto de cascada exige (b) y (f). Es más conservador, y es lo que dice el Owner. En F4, PD-F4-04 se dio siempre con S-04 y con el HEAD (f4-dispositions.md:1655-1660).

**¿Basta sin v4.6? Sí, para PB-T3b:**
- las dos decisiones son autoridades del contrato;
- F4B-E40 prevé que «los contratos de PB-T3 que no nombren el sustituto lo usan por esta decisión del Owner», así que nombrarlo es la forma explícita;
- este informe comprueba los cinco cambios.

**El efecto:** el bloque citado del §6.0 («en PB-T3a, y con su prefijo en PB-T3b y PB-T3c») queda desfasado frente al contrato. A quien lea el §6.0 para PB-T3b le parecería una autoridad contradictoria (M-01).

## 6. La remedición sobre el cierre de PB-T3a (punto 5)

**Lo que remido yo** (`cat3b_remeasure.py`, en `c40eea4a`):
- **`src/`:** `git diff 18e4b783 c40eea4a -- src/…/Pilot` está vacío, y en todo `src/` solo entran los ocho archivos de `Draft/`. Así, las anclas de PP, UR, UP, PM, WHC y RPC (R-18) no se mueven.
- **S-1 a S-6:** en sus líneas, sin `Skip` y con diez filas (§3).
- **`Workspace/`:** 79 archivos, 76 en la lista explícita.
- **El `ForbiddenWriteScope` de `src/`:** 99, y la cobertura calculada es la del seco.
- **La guarda nueva de PB-T3a** (`PushBackDraftWriteGuardTests`, CT-12): deja nombrar los cinco tipos a `Draft/`, a PP y a UR (`MayNameTheModel`, :48-51). Así que PB-T3b puede nombrarlos en PP y UR, pero no en `IPushBackPilotViewEraser.cs`.
  - **Lo que lee la regla:** el texto entero del archivo (`Read`, :69), con `\b<tipo>\b`, comentarios incluidos.
  - **Las otras dos reglas** (CT-13 y la pureza de CT-14, :287-302) solo leen los ocho de `Draft/`.

**El barrido** (`cat3a_sweep.py` sobre `c40eea4a`, comparado con el de la §11 de CA-T3a sobre `18e4b783`):
- **Lo que se mide:** 176 pruebas que leen `src/` y 18 608 literales.
- **Lo que entra:** solo las de `PushBackDraftWriteGuardTests`, que miran `Draft/`.
- **Lo que sale:** aciertos que el control ya ve, porque ahora `Draft/` existe y está en él.
- **En el grupo B** (PP, UR y la interfaz), un único acierto nuevo: el extractor de `namespace` de la pureza de `Draft/` (:298), que solo se aplica a `Draft/`.

Coincide con `sweep-pbt3b-close.txt`: no hay ninguna colisión nueva.

**Lo demás:**
- **La línea base de la CI:** la de F4B-E43 (run `44668584248`: Core 19 107; UI 2 451, con 2 430 y 21).
- **El censo de los diseños de F4 que llegan al plan:** es el del §7.

**Conclusión:** nada material cambia frente a la v4.5. El 99 se explica, y lo demás es igual.

## 7. Las CT de PB-T3b, el momento de cada lectura y los gestos (punto 7)

**Las recetas que cambió V43-11 y V44-01** ya las revisé en la §10 de CA-T3a:
- **CT-01 (i):** `PalletDepth` 48,5, que se conserva y lee de vuelta los dos valores, y `Erasures` vacía;
- **CT-06 (iii):** 2,5 en `pb-02-modulos`;
- **CT-07 (ii):** 3,0 o 3,5, con las lecturas tras la corrida;
- **CT-10:** ` base-igual=True dirty=True`;
- **CT-12:** las referencias, antes del `Prepare`;
- **CT-15:** 4,5.

No cambia nada desde entonces.

**Las demás:**

| CT | Gesto o lectura | Estado |
|---|---|---|
| 02 | «+ frente», «- frente» con el lateral del último poste, el origen en ese lateral, y un rack con ese lateral solo | Cambian frentes, lo que no es un gesto inerte (lente de M-13) |
| 03 | F-01, longitudes de A y de B con su confirmación (módulos personalizados) | Se conserva (CT-PBT1B-02, F-01 con delta vacío) |
| 04 | Declarar B con un frente; quitar B en `pb-20-ab` | Cambian `SideB` y `Composite` |
| 05 | «PB-NUEVO» frente a «PB-01», y un nombre en blanco | El nombre es una ranura del estado: el borrador queda en Dirty |
| 08 | Necesita un plan `Ready` con borrado, que da «- frente» | Bien |
| 09 | Función pura sobre datos fabricados | Sin gestos |
| 11 | `KeepOn` sobre una base con otro nombre, otra versión menor y otra `CustomProperties` | Queda en Dirty por el nombre del estado frente al de la base nueva |
| 13 y 14 | Guardas de texto | Sin gestos |

**Los diseños de F4 que llegan al plan:**
- **Qué usan:** `WorkspacePilotPrepareTests` (`pb-01-base` y `pb-02-modulos`), `WorkspacePilotUpdateRunTests` (`pb-02-modulos` y una vez `pb-20-ab`), `WorkspacePilotUpdateHostTests` y `WorkspacePilotUpdateRequestTests` (`pb-02-modulos`).
- **Sin gestos que bloqueen:** en ninguna sale `StructureOverride`, ni `SetGap(-…)`, ni `CentralSeparator`.
- **Las demás clases del piloto** (`…HostRunTests`, `…ReconciliationHostTests` y `…EditHostTests`) no llegan al plan.
- **`pb-20-ab`** es compuesto y el piloto no lo admite. Ningún fixture da `bloqueado=True` (R-8).

Así, la barrera no cambia ningún resultado del piloto (INV-PBT3B-04).

**Para el Worker** (no son hallazgos del contrato, pero conviene ponerlos en el prompt):
- **`Transaction`:** ningún archivo de `Pilot/` puede contener la palabra, con mayúscula y ni siquiera en un comentario (UpdateGuard:118-123; IntegrationGuard:81).
- **Los cinco tipos del modelo:** `IPushBackPilotViewEraser.cs` no puede nombrar ninguno de los cinco tipos de CT-PBT3A-12, tampoco en un comentario.

## 8. La Snapshot K (punto 6)

- **P-01** es la de PB-T3a (y la de PB-T2c), con la ruta de `snapshot-K/snapshot-k.json`.
- **El registro** no ha cambiado desde `56cb026f`: `git diff 56cb026f c40eea4a -- …/F4-ENVIRONMENT/` está vacío. Lo comprobado en la §5 de CA-T3a (`ConfigSha256` `E1F69847…`, sonda `2a9a` en Pass) sigue valiendo.

## 9. Decisiones del Owner (punto 8)

**Ninguna nueva.**
- **C11** aplica F4B-E40 y F4B-E43, ya tomadas.
- **Las cuatro dudas** se resuelven con lectura del paquete (§2 a §5).
- **3 261** es un número, no una elección.
- **El filtro de paridad** no cambia ninguna red.

## 10. Hallazgos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-01 | M | §6.0 del paquete, el bloque citado de `PBT3x-C11` («y con su prefijo en PB-T3b y PB-T3c»); `PBT3B-C11` | **El problema:** C11 de PB-T3b se aparta, a propósito y con fidelidad, del bloque citado del paquete: nc2 con el v2 (F4B-E43), nc1 con PD-F4B-01 y su sustituto (F4B-E40). Las decisiones mandan y son autoridades, pero el §6.0 sigue diciendo que C11 de PB-T3b y PB-T3c es el de PB-T3a con su prefijo | Registrarlo en F4B-E44: C11 de PB-T3b sigue a F4B-E40 y F4B-E43, con los cinco cambios de esta §5. Antes de PB-T3c, llevar el texto al paquete (v4.6, solo de texto) para que el generador de PB-T3c lo copie del paquete y no lo derive |
| M-02 | M | §6.0, la tabla de RR-3, filas de PB-T3b | **El problema:** tres entradas no se enumeran en la fila: las cuatro líneas de CT-10, las once `<Fn> …` de CT-01 (ii) y la línea cortada de CT-06 (ii). Se sacan del §5.3 de forma mecánica (4, 11 y 1). Además, el doble `PushBackFullWriteSupport.cs` se puede reparar y no tiene literales | Ahora basta: la cadena usa la derivación del generador y la lista va en UTF-8. En la v4.6 del M-01, escribir las 16 líneas en la fila y dar al doble sus literales del §5.0 (`EraseUnit:` y `PreparedBlockMatches`) |
| M-03 | M | §6.2, lo heredado de §6.1: `ExpectedEvidence[3]`, `RequiredTests` y la línea base | **El problema:** tres textos de PB-T3a pasan a PB-T3b sin adaptar:<br>• [3] dice «las autopruebas de las tres guardas», pero en PB-T3b la guarda es CT-13, con las autopruebas de E-2 y E-6;<br>• la fila que reañade `PushBackFullDraft` con `ExpectRed` false no reañade la UI de paridad;<br>• el GREEN previsto de Core, 19 128 resultados, supone que la TRX cuenta las cinco filas de S-2; con una sola, son 19 124. «O la diferencia explicada por el diff» lo cubre | Ninguno impide emitir. En la v4.6 del M-01: «las autopruebas de CT-13 y de E-2 y E-6»; `PushBackFullDraftWindowParityTests` 1, false; y «19 128 o 19 124 resultados, según cuente la TRX la `Theory` S-2» |
| M-04 | M | `Authorities` y la custodia del sustituto de nc2 v2 | **El problema:** en el seco, el v2 entra solo como script, con el SHA por rellenar, mientras que el de nc1 entra con su revisión y sus límites | Fijar el SHA `C033E96C…` y añadir como autoridades sus límites declarados y su revisión. La r2, ya en la carpeta, parece hacerlo; lo compruebo cuando se pida |

## 11. El `BOUNDED_RESULT`, comprobado

`cat3b_bounded.py` aplica la `bounded_check` de `make_pbt3b_contract.py` (`reviewer='CA-T3b'`, como en su llamada) a este informe, al literal y a las líneas de la v4.5.

**Lo que da:** 0 errores. El veredicto está en la forma de PB-T2, es de la familia EMITIBLE y no tiene importantes, así que no hace falta ninguna fila de registro. El literal empieza por «EMITIBLE;», lleva «B 0 / I 0 / M 4» y es ASCII. El informe no lleva rutas locales.

## §12. Recomprobación sobre la v4.5: el seco r2

**Veredicto: EMITIBLE.** B/I/M = **0 / 0 / 1**

**En breve:**
- **El delta es completo y exacto.** Del seco original al r2 solo cambian `Authorities` (de 62 a 64) y el SHA de `PBT3B-C11`.
- **El M-04 queda resuelto.**
- **La custodia de la v2 es fiel:**
  - el script va byte a byte, con su SHA;
  - la revisión va como archivo nuevo;
  - la copia del v1 no se toca.
- **Una menor nueva (M-05):** la custodia deja fuera los ayudantes que citan sus notas y su revisión, como ya pasó con el v1.
- **El `bounded_check` del generador nuevo es el mismo** y da 0 errores con el literal final.
- **No hay decisiones nuevas para el Owner.**

**Sobre el título de esta sección.** El encargo pedía «§12. Recomprobación sobre el seco r2». Pero el `bounded_check` del generador (`make_pbt3b_contract.py:1586-1661`, el mismo que el de PB-T3a) solo reconoce recomprobaciones `## §<n>. Recomprobación (sobre|de) la vX.Y`. Con «sobre el seco r2», el título contaría como un encabezado de recomprobación sin forma válida, y daría error (§12.5). Por eso el título nombra la v4.5, el paquete con el que se generó el r2, y dice después que es sobre el seco r2.

**`BOUNDED_RESULT` final (ASCII):**

`EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1`

### 12.1 Método y límites

**Qué se leyó:**
- **El seco r2:** `contract-dry-pbt3b-r2.json` (sha256 `395DCBB9…`, 73 150 bytes) y `dry-pbt3b-r2.txt`.
- **El delta:** `delta-r2.txt`.
- **Las pruebas:** `neg-pbt3b-r2.txt` (69 de 69) e `issue_pbt3b-selftest-r2.txt`.
- **El generador nuevo:** `make_pbt3b_contract.py` (142 687 bytes, sha256 `d9d82540…`).
- **`issue_pbt3b.py`,** con `NC2_V2_EXTRA`.

**Los guiones de esta ronda:**
- **`cat3b_r2.py`:** el diff, campo a campo y entrada a entrada, de los dos secos, hecho sin `delta-r2.txt`, y el SHA de C11 frente al script del v2;
- **`cat3b_bounded.py`:** el `bounded_check` sobre este informe;
- **`cat3b_title.py`:** el mismo `bounded_check` con el título que pedía el encargo, cambiado solo en memoria.

**Lo que no se hizo:** no se ejecutó dotnet ni se leyó nada del worktree.

**El generador anterior no se puede comparar byte a byte:** no queda ninguna copia, ni en `hs65` ni en git. Lo comprobé por su comportamiento (§12.4).

### 12.2 El delta (punto 1)

**Completo y exacto** (`cat3b_r2.py`):
- **Las claves** son las mismas, y cambian solo `Authorities` y `StopConditions`.
- **En `Authorities`,** la entrada del script v2 cambia el marcador por `c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45`, y entran dos autoridades nuevas: `nc2-substitute-f4b-v2/limits-declared.md` y `nc2-substitute-f4b-v2/review/independent-review-substitute-f4b-v2.md`.
- **En `StopConditions`,** solo cambia `PBT3B-C11`, y su texto r2 es el original con el marcador cambiado por el SHA, y nada más.
- **Lo demás:** el r2 es ASCII y no lleva ningún marcador en las autoridades ni en las paradas. Lo demás es igual, incluidos `ExpectedEvidence` (con `[0]`, el literal de PB-T3 de la lista del v2) e `IssuedBy`.
- **`delta-r2.txt`** dice lo mismo.
- **`dry-pbt3b-r2.txt`** cambia en tres líneas: la cabecera (64 autoridades, 73 150 bytes, `395DCBB9…`), la de C11 (el SHA fijado) y un AVISO de emisión, que ahora nombra los tres archivos de la v2 que trae X.

### 12.3 El M-04 (punto 2)

**Resuelto:**
- **El SHA** del sustituto de nc2 v2 está fijado en C11 y en su autoridad, y es el del archivo revisado (`c033e96c…`, igual al de `limits-declared.md`).
- **Sus límites declarados y su revisión** entran como autoridades, como los del sustituto de nc1:
  - los límites, «M-V1 a M-V4, que no se corrigen»;
  - la revisión, «Recomprobación v2… APTO, B 0 / I 0 / M 4».

### 12.4 La custodia de la v2 y el generador nuevo (punto 3)

**`issue_pbt3b.py`, en X:**
- **El script, byte a byte:**
  - `v2_script()` lee los bytes locales y exige que su SHA-256 sea `NC2_V2_SHA` del generador (:109-115);
  - también exige que no lleve CR, que decodificar y codificar dé los mismos bytes, que la redacción no lo cambie y que no tenga rutas locales;
  - `clean()` devuelve el texto sin tocarlo (:34-41), y `wr()` escribe `text.encode('utf-8')` en modo binario (`issue_pbt1c.py:53-56`);
  - el resultado es el mismo archivo: 41 362 bytes, `c033e96c` (la autoprueba r2, l.17).
  - **Si el sustituto ya estuviera en la revisión,** X no lo escribe y exige su SHA (:495).
- **La revisión, como archivo nuevo:** sale de la de la sesión (`pbt2acc2/nc2-substitute-f4b/review/independent-review-substitute-f4b.md`, 25 356 bytes) y va a `nc2-substitute-f4b-v2/review/independent-review-substitute-f4b-v2.md` (25 625 bytes con la nota de redacción). Antes de escribir, X exige que ninguno de los destinos exista (:498-501).
- **El v1 no se toca:**
  - `F4B-DRAFTS/pbt2a-nc2-substitute/independent-review-substitute-f4b.md` no está entre los destinos; solo lo nombra un comentario (:79);
  - la copia local del v1, `base-6ccaf12d-original.py`, no se custodia;
  - el v1 sigue siendo el de F4B-E34 en su carpeta.
- **La redacción:**
  - **`limits-declared.md`** cambia la ruta de la revisión en `hs65` por la de su custodia (`V2_LOCAL_FIX`, una sola vez) y gana la nota de redacción; X exige que nombre el SHA;
  - **los dos `result-*.json`** llevan las rutas locales como marcadores y siguen siendo JSON válido;
  - **en `checks-summary.txt` y `real-cases-summary.txt`** solo cambian los finales de línea (5 y 26 CRLF a LF), y eso no afecta a ningún SHA exigido.
- **Las rutas de los destinos** coinciden con las de las tres autoridades del r2.

**Lo que no entra (M-05):**
- **Los ayudantes de la v2:** `summarize.py`, `summarize_checks.py`, `verify_literals.py`, `checks/` y `data/`, aunque `notes.md` cita `checks/` y `verify_literals` (l.8 y l.15).
- **Los guiones de la revisión** (`review/selftest_matrix*.py`, `mutations*.py` y sus salidas), que la revisión custodiada cita (l.71, l.87 y l.134-228).
- **Es lo mismo que en la custodia del v1** (`F4B-DRAFTS/pbt2a-nc2-substitute/`), así que no rompe ningún precedente. Pero, por el criterio de DU-23, lo que una autoridad cita como su método se custodia, redactado.

**El generador nuevo, por su comportamiento:**
- **Las marcas que leí del anterior siguen en la misma línea:** `BOUNDED_RESULT` (:62), `BOUNDED_CHECK` (:1507), el `bounded_check` (:1586-1661) y su llamada con `reviewer='CA-T3b'` (:1680). Así que nada se insertó antes de ellas.
- **`NC2_V2_SHA`** (:82) lleva el SHA.
- **El `bounded_check`** es, línea a línea, el de `make_pbt3a_contract.py`.
- **Las pruebas en negativo** pasan de 68 a 69, y todas salen como se esperaba. G02 pasa de «puesto» a «alterado», y G09 es nueva: el marcador sin rellenar. Lo demás es igual.
- **Las salidas solo cambian en lo que dice el encargo:** el SHA y la custodia de la v2.

### 12.5 El `BOUNDED_RESULT` (punto 4)

**El literal final pasa:** `cat3b_bounded.py`, con la `bounded_check` del generador nuevo y `reviewer='CA-T3b'`, sobre este informe entero con la §12, da 0 errores.
- **La cuenta del primer veredicto no cambia** (B 0 / I 0 / M 4).
- **La §12 añade** «recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1», que el literal tiene que nombrar.
- **Con el literal anterior,** el que no nombra la recomprobación, da el error esperado.

**Con el título que pedía el encargo,** «## §12. Recomprobación sobre el seco r2», el mismo `bounded_check` da el error «un encabezado de recomprobación no tiene la forma "## <sección>id. Recomprobación (sobre|de) la vX.Y"». Por eso el título lleva la v4.5.

### 12.6 Hallazgo nuevo

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-05 | M | `NC2_V2_EXTRA` de `issue_pbt3b.py` (la custodia de `F4B-DRAFTS/nc2-substitute-f4b-v2/`) | **El problema:** la custodia lleva el script, las notas, los resúmenes, las corridas reales, los límites y la revisión. No lleva los ayudantes que citan `notes.md` (`checks/`, `verify_literals.py`) ni los guiones de la revisión (`review/selftest_matrix*.py`, `mutations*.py` y sus salidas). Es lo mismo que pasó con el v1 | Añadirlos a `NC2_V2_EXTRA`, redactados, en `review/` y `checks/` dentro de la carpeta de la v2, por el criterio de DU-23. Si se dejan fuera, decirlo en la nota de redacción de `notes.md` |

**Observación (no es hallazgo):** el generador se cambió en su sitio y su versión anterior, la del seco `5EE38DAB…`, ya no existe en ninguna parte. Para la custodia conviene dejar un diff del generador junto a `delta-r2.txt`.
