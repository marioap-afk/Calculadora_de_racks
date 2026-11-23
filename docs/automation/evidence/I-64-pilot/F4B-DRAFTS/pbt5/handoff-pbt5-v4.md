# Traspaso PB-T5 v4: paquete v2.4, `smoke-run-v2.md` y el guion de la emisión

**Para:** la sesión principal y la recomprobación acotada.
**Base:** la comprobación acotada de la v2.3 ([CA5], `review-v23/bounded-check-pbt5-v23.md`, sha256 `2ac17769…044b`): EMITIBLE TRAS CORRECCIONES, B 0 / I 4 / M 15.
**Reglas seguidas:** nada del worktree; lectura solo por objetos git (`7da361eb` y `ebd304a3`) desde el checkout principal; sin AutoCAD, sin compilar, sin pruebas del producto, sin commits; Python con `python -I <archivo>`. Nada sobrescrito: la v2.3, `smoke-run-v1.md` y el guion revisado (`_tools/emit_pbt5-v1.py`, sha256 `654dd728…`) quedan como estaban.

## 1. Lo entregado

| Archivo | sha256 |
|---|---|
| `I-64-f4b-pbt5-package-v2.4.md` | `6bb02ca2af29c42fb71c5cd629dc2cdc98958d83c822e98e722540bef7d049b7` |
| `smoke-run-v2.md` | `11bcaf105e6c74b9f28c4695fae614814cafced259620dc6b2b7ecb6c3d60d59` |
| `_tools/emit_pbt5.py` (nuevo contenido) | `940d6ddae79eb962f0bb80571de72aef9c2351e1657333f83982ecc4eb617f7e` |
| `_tools/emit_pbt5-v1.py` (copia del revisado, historia; no se ejecuta) | `654dd7283ae7a9fc30fa6b385e4d822e1e448c2c196debf2aac85e700a25ca33` |
| `_tools/test_emit_pure.py` | `f18aa99c5816c94a5069cc7afe4e4c78f718cc4146593268df51b671a0b6e294` |
| `_tools/harness_sr2.ps1` | `625dc66e4d92d456a8f93ee45bf70b5fabe3462cafac02de203dc8f01ecef0bd` |
| `_tools/scan_custody.py` | `fe1f10ece8c48ebd57075659178fa4be304d8873ad82b9d27afb47dafa7d2046` |
| `_tools/probe_selection_shape.py` | `6f2ade9787f5d0b42b66e0b9b29cb9786ecd08bd7eb80dfdff3d7838677b1907` |
| `_tools/probe_selection_vs_s2.py` | `ca57156ad787681478365d5a5f861180710423aa41e02ebe5e28498305c61bf6` |
| `package-v2.3-to-v2.4.diff`, `smoke-run-v1-to-v2.diff`, `emit_pbt5-v1-to-v2.diff` | `8cce6789…0c4a`, `7a5d2fc2…b445`, `8bbdcb79…a424` |

`_tools/check_pbt5.py` cambió su lista y escribe ahora `sha256-v4.txt` (con `x`, sin sobrescribir), sin el texto de lo que casa.

## 2. Cómo se aplicó cada hallazgo

- **I-01:** P-09 y SPB-51 reescritos en [P]. En [SR]: el bloque 13 lee la lista local (`s2-selection.local.json`, en la carpeta de la sesión del cierre de PB-T4e, fuera de la del paquete) por su ruta, con su SHA-256 `DD3613CB…F2C1` comprobado; salta las entradas con D1 o D2 (D2 está contado en [BM] M-7: 8 sobres, estilo nulo); comprueba todo antes de escribir; registra sin parar si cada original coincide con el SHA-256 de la lista y con su O1 a O7 de Smoke-2. `estilo.json` lleva la posición en la lista, los papeles, el SHA-256 y las dos comparaciones, nunca la ruta. El bloque 13b hace `s01`/`s02` en `<carpeta-del-Smoke-PB>\spb07\` con `s-copias.json`. El bloque 8 vale para `cNN`, `eNN` y `sNN`. El bloque 10 mide al final la lista (por su SHA-256), los originales de P-09, sus respaldos y copias, y `s01`/`s02`. Los bloques 5 y 9 aceptan las tres clases.
- **I-02:** §6.4 con las 17 paradas enteras; PBT5-C01 con la regla de antes y de después de la emisión; la fila SPB-51 igual.
- **I-03 (estrecho):** la sesión opera sola solo si la emisión y P-01 a P-08 están hechas antes de las 12:00 UTC; 13:00 UTC sin cambio. En [P] §0, §5.4, C11, R-1, S-1, §9; en [SR] §2, §3, §4.10, §5, §6, §7; en [E] (F4B-E62, `next_action`, `NEXT_ACTION`).
- **I-04 (estrecho):** PBT5-C16 solo deja corregir transcripciones; lo que cambie un resultado o pida AutoCAD, al Owner. §6.6: solo la adenda que pida el Owner no cuenta; un paso repetido por otra causa cuenta según la definición de intento.
- **M-01:** `plan()` calcula `yml_new` sobre `HEAD:` y comprueba que no existan el contrato ni el mensaje; en `--commit`, `yml_new` sobre el disco tiene que dar el mismo sha256. El procedimiento de estado parcial, en la cabecera del guion y en [P] §6.7.
- **M-02:** `BOUNDED_RESULT` tiene que empezar por `EMITIBLE;`, llevar `B 0 / I 0` y una vez cada uno `paquete <8 hex>`, `guia <8 hex>` (o «guía») y `guion <8 hex>` (o «guión»), iguales a los del paquete, la guía y el propio guion. El guion exige además ser el `_tools/emit_pbt5.py` de `--src`.
- **M-03:** el paquete, la guía y el registro del Owner, exigidos como copia.
- **M-04:** `hash-object --no-filters`, y `dimkeys.py` desde el Candidato. **Medido** en PowerShell 7.7: la redirección `>` deja los bytes (0 CR) y el id es `57decb54…`, igual que `rev-parse` y que en `7da361eb`.
- **M-05:** c01 rotulada «P-09 (D1)».
- **M-06:** SPB-60/61 con «Comprobar ahora», sin «Actualizar»; en SPB-61, «Descartar y recargar» → «Sí», como el paso 84 de [S2].
- **M-07:** bloque 15 (líneas `ERROR`, eventos de error de acad desde el primer `acad-session`, `settings.json` frente a P-05). El bloque 4 da ahora el SHA-256 de `settings.json`.
- **M-08:** bloque B4, entero, con StrictMode, `catalogs` derivado del worktree y comprobado, y `exit 0`. Por StrictMode, lee `BlockLibraryPath` con `PSObject.Properties` (el fragmento literal de [G] §7.1 fallaría si la propiedad no existe).
- **M-09:** [P] §5.8 y [SR] §7: el registro no copia órdenes con rutas; antes de custodiar, `_tools/scan_custody.py` con los patrones locales (no imprime lo que casa). Los STOP de los bloques 10, 13 y 13b no nombran rutas de originales.
- **M-10:** la lista vive fuera de `--src`; además, el guion para si encuentra en `--src` un archivo `*.local.*` que no sea `_tools/*.local.txt`.
- **M-11:** la regla escrita en [P] §5.6 y [SR] §4.12, con el estado esperado de cada contexto; el bloqueo de la capa, con una lectura `tblsearch`; `-PURGE` con `?` por espacio, como [BM] M-2b, y la comprobación de que no se purgó de más; «variable de sistema» aclarada en [P] §5.3 y [SR] §6 y §9.
- **M-12:** SPB-45, hecho informado; PASS si coincide con la gemela.
- **M-13:** PBT5-C17 con «tras resolver y recomprobar» y la reejecución de C15 antes.
- **M-14:** un orden (`--dry` → build → `--commit`) en [P] §6.7 y [SR] §4.10; la revisión de la evidencia en `F4B-PBT5-SMOKE-PB-GATE-CLOSE/` (§3.3); §2 «Medidos en `7da361eb` y en `ebd304a3`»; título de §4.1; la §292 calcula la historia del listado (preguntas v1 a v2.2; las revisiones que van solo por sha256 salen en su lista).
- **M-15:** `next_action` y `NEXT_ACTION` conservan la nota de OWNER-01 de PB-T4e.

## 3. Lo que no apliqué, o apliqué distinto, y por qué

1. **M-09, último punto (los literales partidos de la carpeta de trabajo en [E] y en `review-v23/sim_collect.py`):** no los cambio. La carpeta de trabajo no es un dato personal (la fijó la sesión principal y su redacción es de forma); los literales van partidos para que la redacción no cambie las constantes del propio guion. Tras la redacción ninguno casa un patrón: `emit_pbt5.py` va como copia (simulado).
2. **M-11, hacer robusto `-INSERT` (leer `BlockScaling` o insertar por ActiveX):** no. Q-PBT5-4 se aplicó con órdenes nativas tecleadas; cambiar la forma de construir sería otra cosa. Va la regla que propone [CA5].
3. **La regla del bloque J a las 12:15 UTC** (PBT5-C11 de la v2.2, M-29): no vuelve. Era de la corrida previa, donde J era opcional; en el Smoke-PB contratado, J está entre los 65 pasos.
4. **Añadidos que no pidió [CA5],** todos de solo lectura o de parada:
   - `yml_new` cambia en `f4b_status` la frase «siguiente, la v2.3 de su paquete, su comprobacion acotada y la emision condicional», que tras la emisión sería falsa; si no aparece una vez, para;
   - el bloque 4 lista los tres últimos `rackcad-*.log` en lugar del de la fecha UTC (no se sabe si el nombre usa la fecha local);
   - si varias `eNN` guardan estilo, SPB-07 usa la primera por orden de `eNN` (convención declarada en [P] §5.7);
   - si `c01-pre` (D1) diera un estilo, es un OBSERVADO y SPB-07 vuelve al Owner, sin copias nuevas de D1 (no hay bloque exacto para ellas, y [BM] M-7 midió el estilo nulo en los 14 sobres de D1).
5. **El guion de la emisión se reescribió en su sitio** (`_tools/emit_pbt5.py`), porque el nombre es el de la orden documentada; el revisado queda en `_tools/emit_pbt5-v1.py`.

## 4. Lo probado, sin AutoCAD

- `ps_parse_any.ps1 -Name smoke-run-v2.md`: 23 bloques, 0 errores, todos con StrictMode, `Stop` y `exit 0`, sin variables libres.
- `harness_sr2.ps1` (archivos falsos fuera de la carpeta del paquete): bloques 13, 13b, 8 (la parte de manifiestos), 10 y 15. **Encontró un fallo, ya arreglado:** en el bloque 10, `$ce -eq 'no existe'` daba verdadero con `$ce = $true` (PowerShell convierte la cadena a booleano); ahora compara solo si es cadena. También comprueba que el bloque 13 no sobrescribe, que el 8 para con la misma copia o una que no existe y que el 10 para con un original cambiado.
- `test_emit_pure.py`: `yml_new` sobre `ebd304a3:…/I-64.yml` cambia 7 líneas, todas entre comillas, con OWNER-01 y la frontera estrecha, y para si ya dice «PB-T5 EMITIDA»; el contrato sale con su cabecera, los 17 invariantes y las 17 paradas en orden, sin remitir a la v2.2 y redactado; `check_bounded` pasa la línea buena y para con «TRAS CORRECCIONES», «NO EMITIBLE», un importante, otro sha256, sin guion o dos líneas; tras la redacción, solo 4 archivos irían por sha256 (`sha256-v3.txt`, `_tools/make_smoke_run_v1.py`, `review-v1/check_lisp_v2.py` y `review-v1/independent-review-pbt5-v1.md`); el paquete, la guía y el registro del Owner van como copia.
- La forma de la lista local (sin leer sus rutas): siete entradas; sus SHA-256 son O1 a O7 de Smoke-2 (`probe_selection_*.py`).

## 5. Para la recomprobación y la emisión

- **La línea `BOUNDED_RESULT` del informe nuevo** tiene que seguir esta forma para que el guion la acepte: `BOUNDED_RESULT: EMITIBLE; B 0 / I 0 / M <n>; …; revisado paquete <8 hex>, guia <8 hex>, guion <8 hex>; …`. Hoy serían `paquete 6bb02ca2`, `guia 11bcaf10` y `guion 940d6dda`.
- **La orden:** `python -I <carpeta del paquete>/_tools/emit_pbt5.py --dry --wt <worktree> --src <carpeta del paquete> --package I-64-f4b-pbt5-package-v2.4.md --guide smoke-run-v2.md --cand ebd304a3773495fa9071de6611936c4c01a7b06c --run 45147867500 --bounded <informe de la recomprobación> --out <carpeta fuera de --src>`; después, `--commit` con lo mismo y `--dry-listing`.
- **Si la recomprobación cambia algo** del paquete, la guía o el guion, sus sha256 cambian y la línea tiene que llevar los nuevos.
- **Sin verificar:** todo lo de AutoCAD (las respuestas nativas de §4.12, `QUIT`, `rcdyn`, `vla-item`, `WaitForInputIdle`); `gh run list --commit` en la versión de `gh` de la sesión; que los originales sigan en las rutas de la lista y cuáles tienen racks Push Back.
