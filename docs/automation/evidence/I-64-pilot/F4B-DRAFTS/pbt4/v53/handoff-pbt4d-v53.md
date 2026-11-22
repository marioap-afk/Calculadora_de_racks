> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Traspaso de PB-T4d (paquete v5.3)

**Fecha:** 2026-11-22. **Base:** `6c2820bab4d8b41e3e229e23f3a1638ccbb04735`. **Parte de:** la v5.2 (`pbt4/I-64-f4b-pbt4-gate-contract-package-v52.md`, `466ab3db…f13e`) y su comprobación acotada (`pbt4/independent-review-pbt4d-check.md`, `439837f9…dced3`: EMITIBLE TRAS CORRECCIONES, B 0 / I 2 / M 7). Sigue a `v52/handoff-pbt4d-v52-f4.md` (reglas y carpetas iguales).

**Carpetas:** `S = <scratchpad>\hs65`. Piezas en `S\pbt4\`; registros míos en `S\pbt4\v53\`; corridas en `S\pbt4dcc\compile\` (las salidas de la v5.2 renombradas con `-v52`). **Copias nuevas desde `/d/r6v`** (todas, ensayos incluidos). La siguiente libre: `/d/r6y` (la principal), `/d/r6z` (el control).

## 1. Lo que manda

- `owner-pbt4d-cat4d-decisions.md` (`fa07db38…03fa1`): 48 y 49 se sustituyen (29); cobertura de toda la suite de Core sobre todo `src/` que se queda; **regla:** todo camino que se queda y pierde su única prueba recibe sustituta; PARA si una no se puede escribir o si salen más de 10 nuevas sobre las 29. Lo que cubra la UI se mide con sus filtros.
- Orden de la sesión: I-01, I-02, M-01 a M-07, una fila de cambios por hallazgo.

## 2. Fase 1, las piezas: HECHA

| Pieza | Dónde | sha256 | Cómo |
|---|---|---|---|
| **GREEN** = v5.2 + 11 bloques de solo comentarios (M-05 a) | `pbt4/pbt4d-green-v53.txt` | `51a402092c14c5766e8f3f2ae27419f49f5d1e6e6830c1d26b3ef7e9835d1436` | `python -I make_green_v53.py <repo> ../pbt4d-green-v52.txt ../pbt4d-green-v53.txt make_green_v53-out.txt` (desde `v53/`) |
| **RED** | `pbt4/pbt4d-red-v53.txt` | `de0d3a0e1961eb5c6d2f699eab086984310681bf558c14f09e5486d8ea9bc24e` | `python -I make_red_v53.py <repo> <S>/pbt4dcc/compile/r6h-nd/bucle/necesidad.tsv try/tf ../pbt4d-red-v53.txt make_red-final.txt` (= `try/red-d3.txt`): 23 existentes editados, 3 borrados (DELETE), 5 nuevos |
| **Lista** | `pbt4/pbt4d-retire-list-270.tsv` | `7e65c9e49c00f92c330eecf309976add3e9ecda374f4d8166075e0eccf58081f` | `python -I make_list_v53.py <repo> <S>/pbt4dcc/compile <necesidad.tsv> try/t3 try/list4` (283 filas, 270 con acción; columnas `declaraciones-6c2820ba` y `tramo-editado`) |
| Las 29 | `v53/run/PushBackRetirePilotRunTests.cs` | `3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69` | `make_run_eq_v53.py` |
| Ediciones, M-03 (`UNUSED`, 32; `DELETE_FILES`, 3) | `pbt4/pbt4d_test_edits_v53.py` | ver `sha256sum` | — |
| Módulos v5.3 (RED con DELETE; GREEN con `--pieza`) | `pbt4/{green,apply_green,red,apply_red}_pbt4d_v53.py` | ver `sha256sum` | — |
| Quitar un ayudante | `v53/unused_v53.py` | ver `sha256sum` | — |

**M-03, medido:** IDE0051/52 en `/d/r6v` (17) y `/d/r6x` (0 nuevos) y por texto (`v53/dead_text_v53.py`: `try/dead-text-d3.txt` vacío). **M-02:** la rama la cubre la UI de los filtros (base y GREEN 2/2): sin sustituta. **I-01:** `PushBackPilotSystemRebuild` entero cubierto en el GREEN. **Sustitutas nuevas sobre las 29: 1** (el corpus de CT-PBT3B-06 (iii), M-01, en su sitio).

## 3. Corridas

- `/d/r6v` (ensayo 1, IDE), `/d/r6x` (ensayo 2: IDE + cobertura del GREEN; GREEN Core 19 418 = 19 015 + 0 + 403; UI unión 899 = 869 + 0 + 30).
- **`/d/r6w`, la base, sola:** Core 19 619 = 19 158 + 0 + 461 con cobertura (`r6w-bc/base-core-cov.*`); UI unión 921 = 890 + 0 + 31, tres veces (`base-ui-cov`, `-r-1`, `-r-2`); y cada filtro de los 19 por separado (`base-uif-NN`, `driver-uif-r6w.txt`, en curso).
- **Ruido de UI** (`driver-noise-batch.txt`): `RackViewBatchDialogPresenter.cs:37-38` (getters bound por WPF, CT-14 de `PushBackClassicCharacterizationOwnerTests`, que no cambia): con su filtro solo, base 1,0,1 y GREEN 1,1,1; en la unión, base 1,0,0 y GREEN 0,0,0. **Método (R-T4-38):** la UI de cada rama = unión de 3 corridas de la unión de filtros + cada filtro en su proceso. Con eso el ensayo da **PASA** (`r6x-t2/cov-r6x-4.txt`).
- Censo de C13 sobre el GREEN v5.3: **PASA** (`census-c13-v53.txt` `4707048b…d027`).

## 3b. Orden nueva de la sesión (tras la fase 1)

- **D-S16** (`session-pbt4d-design-decisions.md`, `27010f8f…a52e`): acepta R-T4-38 (la UI = 3 corridas juntas + 1 por filtro, simétrico). **Añadido:** `cov_p6_v53.py` lista «Bajan (informativo)»: toda línea o rama que cubren más corridas en la base que en el GREEN, por componente emparejado (Core, juntas, cada filtro), con tipo PERDIDA (camino perdido → sustituta, cuenta para el límite de 10), VARIA (declarar en `compile-check.md` y el paquete) o ESTABLE. Probado sobre el ensayo: PERDIDA 0, VARIA 2 (las 37-38), ESTABLE 114 (M-02 entre ellas: GREEN juntas 3/3). `p6_json_pbt4d_v53.py` y `make_v53.py` lo recogen (`bajan-sin-perdida`, `@@BAJAN@@`).
- **Al terminar, reportar:** los totales, el PASA/FALLA, la sección «Bajan» y los sha256 de la v5.3, del JSON y de `compile-check.md`.

## 3c. Fase 2 y 3: HECHAS

- **`/d/r6y`, la principal, SOLA** (procesos 0 al empezar, 08:06:00, y al acabar, 08:47:37): 0 errores y 0 `warning CS` en las 5 compilaciones. Core RED 19 418 = 19 014 + 1 + 403 (solo CT-01); Core GREEN 19 418 = 19 015 + 0 + 403; los 18 filtros del contrato, 0 `Failed` en las dos ramas (en el GREEN, con cobertura); la unión con cobertura, 3 veces.
- **`/d/r6z`, el control:** base con las 2 CT, caen las 2; RED con la variante A, cae CT-01 y pasa CT-02.
- **La cobertura** (`post_p6_v53.sh` → `cov-p6-v53.txt` `8090f08a…6615`): **PASA**; 0 líneas sin ejecutar y 0 ramas perdidas; conjunto vigilado de la v5.2, 0 sin cubrir. **Bajan:** 16 801 entradas, PERDIDA 0, VARIA 0, ESTABLE 16 801 (casi todas del filtro `RackCad.UI.Tests.Workspace`, que pierde las pruebas de UI borradas, y las cubre Core); y 118 líneas o ramas solo por el filtro de la clase borrada (`PushBackFieldValidationConcordanceTests`). M-02 (233, rama (0,0)): ESTABLE. Las 37-38: no bajan esta vez; declaradas.
- **RequiredTests:** 88 (83 + 5); solo cambia el n.º 88, `MinSelected` 31. **UI de la CI:** 2 464 = 2 416 + 0 + 48 en las dos ramas.
- **`pbt4d-precondition6.json` `4cc0654934da59a537f0bed66ca992f8ac76ac5d4a0ec14c437d6ccc1a205567`** (todo previsto); **`compile-check.md` `58455b41278b7c009eef564317e642cc3c0c1abb9e962d5243b9737f2e09aa0e`**.
- **La v5.3:** `pbt4/I-64-f4b-pbt4-gate-contract-package-v53.md` **`ae338b2d2ae4cd25d0d78306dad2f7468df94e54d9159c7e1a63f7d06c3e9daf`** (861 176 bytes; `make_v53.py` `4fd7fb6a…b669`, 19 reemplazos; registro `v53/make_v53-out.txt`). Sin rutas locales.

## 4. Lo que faltaba (hecho; las órdenes, para repetir)

1. Al acabar `driver-uif-r6w.txt`: `bash p6_pbt4d_v53.sh /d/r6y 2>&1 | tr -d '\r' > driver-r6y-p6.txt` (SOLA); después `bash p6_control_v53.sh /d/r6z 2>&1 | tr -d '\r' > driver-r6z-ct.txt`.
2. `python -I cov_p6_v53.py <repo> ../../pbt4/pbt4d-green-v53.txt r6j-cv cov-p6-v53.txt BASE_CORE=r6w-bc/base-core-cov.coverage.json BASE_UI=r6w-bc/base-ui-cov.coverage.json BASE_UI=…-r-1… BASE_UI=…-r-2… BASE_UI=r6w-bc/base-uif-NN.coverage.json(x19) GREEN_CORE=r6y-p6/core-green-cov.coverage.json GREEN_UI=r6y-p6/ui-green-cov*.coverage.json(x3) GREEN_UI=r6y-p6/green-uiNN.coverage.json(x18)`.
3. `python -I required_pbt4d_v53.py ../../pbt4/v52/pbt4c-required-6c2820ba.tsv required-pbt4d.tsv required-pbt4d.txt BASE_CORE=r6w-bc/base-core-cov.trx BASE_UI=r6w-bc/base-ui-cov.trx RED_CORE=r6y-p6/core-red.trx RED_UI=r6y-p6/red-uiNN.trx… GREEN_CORE=r6y-p6/core-green.trx GREEN_UI=r6y-p6/green-uiNN.trx…`; `python -I ui_totals_v52.py ui-totals-red.txt red BASE=r6w-bc/base-ui-cov.trx RAMA=r6y-p6/red-ui*.trx` (y green).
4. Escribir `v53/m02.txt` (el texto de M-02 para el registro) y `python -I p6_json_pbt4d_v53.py . r6y-p6 r6w-bc r6z-ct pbt4d-retire-list-270.tsv`.
5. `python -I v53/edit_frags_v53.py` (hecho; repetir si cambia) y `python -I make_v53.py <repo>` (desde `pbt4/`).

## 5. Deslices declarados

- Un `python -I -` con un heredoc vacío, pegado por error en una orden de `sed` (no leía ni ejecutaba nada); se colgó y lo corté con TaskStop.
- Un `python -I -c "print(1)"` lanzado por error al principio de una orden de lectura con `sed`: solo imprimió `1`, no leyó nada.
- Un `python3 --version` innecesario dentro de una orden de `sed` (no ejecuta código).
- Desde aquí, cada orden de Bash se revisa antes de lanzarla: ningún `python` que no sea `python -I <archivo>`.
