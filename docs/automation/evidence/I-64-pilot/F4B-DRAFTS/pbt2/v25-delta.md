# I-64 F4B — PB-T2: de la v2.4 a la v2.5 del paquete (para emitir PB-T2b)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **La recomprobación de la v2.4,** la §10 de `independent-review-pbt2b-v23-check.md`: **EMITIBLE, nuevos B 0 / I 0 / M 4**, sin decisiones nuevas para el Owner. Confirma las correcciones de CA-T2b y el defecto de la tolerancia de los fixtures (V24-11).
- **El mensaje de la sesión:**
  - aplicar M-08 a M-11 en una v2.5;
  - poner el `BOUNDED_RESULT` que propone la §10, con la mención de la v2.5;
  - custodiar el encargo literal de las dos rondas, con una sustitución local de su ruta de perfil.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v25.md`: la v2.4 con el registro v2.4 → v2.5 (V25-01 a V25-04) delante de los anteriores;
- este archivo;
- **los generadores alineados:**
  - `make_pbt2b_contract.py`, con la v2.5, M-08 y el `BOUNDED_RESULT` nuevo;
  - `make_pbt2c_contract.py`, que nace sobre la v2.5;
- `custody_prompt_pbt2b.py`: la sustitución local de M-11 y su comprobación;
- el seco y las pruebas en negativo de PB-T2b repetidos con la v2.5.

**Resultado:**
- Nada cambia el diseño, las CT ni lo que se informa al Owner.
- No hay decisiones nuevas.

## 1. Los cambios

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V25-01** | M-08 | **`bounded_check` lee también las recomprobaciones.**<br>- **Qué exige:** cada una con su encabezado («## §n. Recomprobación sobre la vX.Y») y su línea «**Veredicto de §n: … Nuevos: B n / I n / M n.**». La última tiene que ser EMITIBLE, con B 0 e I 0, y `BOUNDED_RESULT` la nombra («recomprobacion vX.Y: EMITIBLE, B n / I n / M n»).<br>- **Su forma:** la misma función en los dos generadores, con el revisor como parámetro: `CA-T2b` en PB-T2b y `CA-T2c` en PB-T2c.<br>- **La prueba en negativo de la sesión:** una §10 con I 1 aborta (`ca-s10-importante`). Hay otras cuatro: con B 1, NO EMITIBLE, sin la mención en `BOUNDED_RESULT` y sin la línea del veredicto | `make_pbt2b_contract.py` y `make_pbt2c_contract.py`. El texto del paquete no cambia |
| **V25-02** | M-09 | **La tolerancia no entra en ninguna geometría de Push Back:** `AutoBeamLength` la recibe y no la usa, el resolutor solo comprueba que es positiva y la guarda en el sistema, y ningún diagnóstico bloqueante la mira. Se corrigen la frase del §5.0 y la de V24-11, que decían «solo entra en la geometría transversal». La conclusión no cambia | §5.0; el registro V24-11 |
| **V25-03** | M-10 | **`ArchivoTemporal` se borra antes de cada clic en «Guardar en biblioteca»,** también con la costura devolviendo `null`. El `archivo=False` de CT-PBT2B-07 ya no depende de un archivo que haya dejado una corrida local abortada | §5.0; CT-PBT2B-07 |
| **V25-04** | M-11 | **La custodia de PB-T2b** gana esta v2.5, `v25-delta.md` y el encargo literal de CA-T2b y de su §10 (`independent-review-pbt2b-v23-check-prompt.md`, escrito por la sesión), junto al informe | §6.0, «Custodia»; `CUSTODY_EXTRA` del generador |

**`BOUNDED_RESULT`** (`make_pbt2b_contract.py`) es el literal de la §10 con la mención de la v2.5:
`EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5)`.
El nuevo `bounded_check` lo acepta con el informe real.

## 2. M-11: la custodia del encargo y su ruta de perfil

**El problema:** el encargo nombra una ruta de perfil seguida de la carpeta de worktrees de Codex. La redacción común de `issue_pbt1c.py` solo sustituye la parte del perfil (`<perfil-de-usuario>`) y deja la carpeta, y FORBIDDEN la rechaza.

**La solución:** `custody_prompt_pbt2b.py` aplica antes una sustitución local, que la deja como `<worktrees-de-Codex>\...`.
- **Para el script de emisión:** da `prompt_text(ruta)`, que hace la sustitución local, exige que quede una sola vez y aplica después la redacción común (`red_text`, con FORBIDDEN).
- **El script de emisión de PB-T2b no existe todavía.** Al escribirlo, la custodia de ese archivo pasa por `prompt_text()`. La forma de uso está en el encabezado de `custody_prompt_pbt2b.py`.
- **Su comprobación:** `python -I custody_prompt_pbt2b.py` carga los ayudantes de `issue_pbt1c.py` sin `import yaml`, así que corre con `-I`. Da:
  - sin la sustitución local, FORBIDDEN rechaza el texto;
  - con ella, el texto sale limpio, con `<worktrees-de-Codex>\...` una vez (en la ruta) y `<checkout-principal>` dos veces (en la nota de redacción y en la ruta del checkout);
  - solo cambian las dos líneas de las rutas (58 y 59), más la nota de redacción de arriba, que nombra también `<worktrees-de-Codex>` entre sus marcadores (corrección posterior, con la v2.6).

## 3. El texto que comparan los generadores y cambia

- **En los dos generadores:** la frase de custodia del §6.0 (V25-04) y la cabecera y el registro V25.
- **En PB-T2b:**
  - la huella del §5.0 de `ArchivoTemporal`, que es V25-03;
  - una huella nueva en CT-07 (V25-03);
  - dos comprobaciones de V25-02: la frase nueva del §5.0, y que «geometría transversal» no aparece ni en el §5.0 ni en la fila V24-11.
- **Nada más cambia:** los invariantes, las paradas, el Objective, la costura, las clases, la ExpectedEvidence, la línea base y los RequiredTests siguen igual.

## 4. Lo que no cambia, a propósito

- **`v24-delta.md` conserva su frase sobre la geometría transversal (§2).** Es el delta revisado de la v2.4, y la corrección vale desde este archivo y desde la v2.5. Ver la DUDA en las notas del generador de PB-T2b.
- **El registro de mediciones:** no gana apartado.

## 5. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`17872be8`, `5ddeb3b4` y sus antecesores), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura: `_work/c/make_v25.py`, `_work/c/splice_bc.py` y `custody_prompt_pbt2b.py`.
- **Ni dotnet ni git de escritura.**
- **Incidencias:** ver las notas de los generadores.
