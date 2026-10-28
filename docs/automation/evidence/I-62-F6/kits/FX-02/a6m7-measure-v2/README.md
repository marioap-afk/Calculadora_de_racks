# Kit de la medición A6-7 (I-62), versión 2: una invocación medida del EXECUTION_CONTROLLER de FX-02 con el instrumento B-min (NO LANZADO)

> **Preparado, congelado y autoprobado sin lanzar (decisiones §100, puntos 3 y 4).** Es la actualización incremental del kit `a6m7-measure`
> (FREEZE `459a3440`, que no cambia y sigue custodiado en su carpeta). Se reutilizaron sus componentes compatibles y se cambiaron solo las
> superficies afectadas por A-7 AGREED y por las disposiciones posteriores.
>
> Para construirlo no se ejecutó `codex.exe` ni ninguna CLI de modelo, no se leyó `auth.json` ni ningún valor de `config.toml` (solo su
> SHA-256 y los nombres de clave, como las puertas) y el pwsh del runtime solo se hasheó. Se escribió únicamente en `D:\r62-a6m7\kit2\` y en
> su `-C` nuevo, `D:\r62-a6m7\m7-run2\`, sin el objeto del instrumento (`prep_write.py` no se ejecutó).
>
> No autoriza ninguna invocación ni consumo, no declara la celda ELIGIBLE y no adelanta ningún resultado.

## 0. Qué cambia frente al kit v1 (y por qué)

| Superficie | v1 (`459a3440`) | v2 | Fuente |
|---|---|---|---|
| Trío de Codex | `9ecb9df2…` / 26.1013.3656.0 / `6EFA5B0E…` (107 nombres) | `5c1b5596…` / 26.1023.6123.0 / `0A9F6A34…` (106 nombres; etiqueta `08d187884f2330d1`; acompañantes `4c99510126c2ea89`) | observación pasiva del 2026-10-28 (§100, punto 5) |
| Cierre de VERIFY | R7-A (`make_eic_verify.py`, `designate_m7.py`, `a6m7-designation/1`) | A7-1 (`make_eic_a7.py`, `designate_a7.py`, `check_a7_closure.py`, `a7-designation/4` MEASUREMENT), reutilizados byte a byte de `I-62-A7/r4/` | A-7 AGREED (§100, punto 1) |
| Orden declarada | 12 órdenes, con `contract` | 11 órdenes, sin `contract`, más dos `read-json --file` de CONTRACT y DELEG con los ocho punteros de `Contract` (13 ejecuciones) | D-1 (§100, punto 3; s92) |
| Regla de reconstrucción | v2 `1eceecee…` | v3 `f5f8f269…`: v2 más el conjunto declarado; la cabecera declara la sentencia retirada (F8 del preflight de A-7) | §100, punto 3; notas de s92 |
| `RunId` | `R20261016T000000Z-m7m0` | `R20261028T024440Z-27a9` (forma de AP 16.6) | §92, punto 6 |
| `-C` | `D:\r62-a6m7\m7-run` (`43a8caf6…`, intacto) | `D:\r62-a6m7\m7-run2` (instantánea `1d2d4d1d…`), por el mismo `prep_clone.py` | la copia del TRX vive bajo la ruta del `RunId` |
| Disposición | posterior a §90 | posterior a §100 (`LastSectionBeforeDisposition` 100) | §100 |
| Esquema | `rackcad-a6m7-measurement/1` | `/2`: sin `ContractA5MissingJson` ni `ContractSupersededEqual`; con `ContractScopeJson` y `DelegScopeJson` | D-1 |
| Líneas del Owner | trío anterior | trío vigente (preparadas, no autorizadas) | §100, punto 5 |

Derivación: `s100/patch_kit_v2.py` (ediciones exactas sobre los archivos de v1, cada SHA-256 de partida comprobado contra su FREEZE), más
`build_m7.py` y `prompt.m7.template.txt` reescritos (`s100/build_m7.v2.py`, `s100/prompt.m7.template.v2.txt`). Diffs completos en
`s100/kit-v2/diffs/`.

## 1. Secuencia (en este orden; ninguna se da por cumplida antes)

1. **Coordinator:** la disposición única previa a la medición (borrador: `s100/pre-measurement-disposition-draft.md`), posterior a §100, con
   los marcadores literales de §2. Se nombra a sí misma en `M7-LECTURAS`, porque contiene la disposición de A6-5, regla 6.
2. **Owner:** las líneas `line-od2.txt` y `line-b.txt`, byte a byte, como líneas completas del archivo de decisiones, después de esa disposición.
3. **Supervisión, antes de lanzar:** confirmación pasiva de la identidad; cierre de la app de escritorio de Codex (ningún `codex.exe` vivo);
   I-64 sin trabajo crítico.
4. `python -I prep_write.py`: escritura de preparación de A6-3, regla 1.
5. `bash run_m7.sh launch`: puertas → punto de uso → marcador P-07 → **un** lanzamiento → posterior.
6. Custodia de `out/custody/` y disposición de elegibilidad del Coordinator sobre `out/comparison.json`.

## 2. Marcadores literales de la disposición

```text
M7-CELDA: codex-cli / gpt-6-luna / high; EXECUTION_CONTROLLER de FX-02 en F6
M7-TRIO: BinaryHash 5c1b55966bf9f757dcbf00065c9a6cb862ead55d34b015f1be1939ff18e43943, AppVersion 26.1023.6123.0, huella 0A9F6A344C965BE17650A2559601DB8768AE8DCFAC6BEB7DEEFC615AE4E7F550
M7-INSTRUMENTO: blob 9a829b0eeb1210238052b1feac06cfa8f7964aed; orden declarada <git> --no-replace-objects cat-file blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | <interprete> -I - <orden> ...; <orden> una de: identity, diff, scope, hashes, cleantree, handoff, freetext, clausemap, trx, read-text, read-json; sin contract
M7-INTERPRETE: %LOCALAPPDATA%\Microsoft\WindowsApps\python.exe; Python 3.13.14; SHA-256 ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec
M7-GIT: %USERPROFILE%\AppData\Local\Programs\Git\cmd\git.exe; git version 2.54.0.windows.1; SHA-256 81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5
M7-C: D:\r62-a6m7\m7-run2; instantanea SHA-256 1d2d4d1d92a3c42b4fc07c14c13e963a30624a90c7ee6f962318348368c104ed
M7-CIERRE: A7-1 (a7-designation/4, Kind MEASUREMENT); designacion SHA-256 a5d45905768febd2bca72fa70436562b52ff69306b15ea67d331f7bbf895afad; E SHA-256 0488409ceec42e54edcbb0b9f68b91eb0128a71b5f8edb99b3cb201d853b1cd5; cierre derivado SHA-256 2c5cedf14b9390eae13102084990063951ec00fc8a2bc70b51f039e668049674; registro de derivacion SHA-256 716463df3ba80e1712d1d879260c3db32932c151d1e62a5122a46c8cdc7d50b6
M7-RECONSTRUCCION: reconstruct_runs_m7.py SHA-256 f5f8f269efdb881bc395a0a7a244d7d9fe00bbce23f56306ae6efc0d2a6af834
M7-ESCRITURA: autorizada la escritura de preparacion de A6-3, regla 1, en D:\r62-a6m7\m7-run2; DecisionRef I62-A6-7-M7
M7-KIT: FREEZE.json SHA-256 <SHA-256 de FREEZE.json, que se da fuera del kit>
M7-LECTURAS: disposicion de A6-5, regla 6: §<N>
```

## 3. Orden de lanzamiento

`bash run_m7.sh launch`, sin cambios de forma frente a v1. Lanza `codex.exe` de la etiqueta vigente con `-C D:\r62-a6m7\m7-run2`, `-s read-only`,
`-m gpt-6-luna`, `model_reasoning_effort="high"`, el esquema estricto de v2 y `-o` en `IgnorePath(I)` del `RunId` nuevo.

- **Texto armado:** 17486 caracteres ASCII, SHA-256 `a2d500d83596f9338d7546972b1150218d2a48b8eaf1c4244fe98b50144f14c6`.
- **Ejecuciones:** C1-C18. C4-C16 son las 13 del instrumento; C17 y C18, los contrastes primarios.

## 4. Puertas

Las de v1, sin cambio de lógica: kit, decisiones, identidad antes, P-07, estado del `-C`, pwsh del runtime, Git estático, entorno, P-02,
proyecto de Claude y texto. Comprueban los valores de v2:
- el trío;
- la línea base de nombres `baseline-keynames-0A9F6A34.json`;
- la sección posterior a §100;
- los marcadores de §2;
- el `-C` nuevo.

El estado actual, con las condiciones pendientes marcadas, está en `s100/kit-v2/preflight-static.json`.

## 5. Cobertura de A6-7, regla 1

Como en v1 (OP1-OP9 y RA-RH), con dos cambios:
- **OP2:** «las 11 órdenes declaradas, ejecutadas al menos una vez, y las dos lecturas `read-json --file`»; `Runs` de 13 filas.
- **OP6:** las dos lecturas `read-json --file` de CONTRACT y DELEG con valores iguales a los bytes custodiados (sustituye a `contract`).

Ninguna operación dependía solo de `contract`.

## 6. Cierre de VERIFY de M (A6-7, regla 7; A7-1)

- **Designación** `a7-designation/4` MEASUREMENT (SHA-256 `a5d45905…`): CONTRACT `628d89af`, DELEG `8358efbf`, H `b5699266` y TRX_CURRENT
  `40295387`. TRX_RED no se designa: no existe un TRX real de la corrida de `RedSha`, y el constructor registra `NOT_DESIGNATED`.
- **E de A7-1:** 16 entradas, SHA-256 `0488409c…`. Con el `-C` de v1, sus entradas canónicas son las de la E R7-A de v1.
- **Cierre derivado:** `2c5cedf1…` (7 commits, 89 `RevPaths`, 3 refs, 49 rutas, archivos CONTRACT, DELEG, H y TRX). La única omisión es
  `C1.D.ChainRedSha FIELD_NULL`.
- **Validador de A7-1:** aceptado (`frozen/closure-validation.json`).
- **Cobertura de A-7 §3.5:** LAUNCH_MAY_PROCEED.

## 7. Regla de reconstrucción (v3)

`reconstruct_runs_m7.py` v3 = v2 más el conjunto declarado: una ejecución de `contract` deja el registro no reconstruible, sin solicitud.

El autotest compara v3 con la referencia sobre los 64 registros históricos sin sus ejecuciones de `contract`: el mismo resultado, salvo las dos
diferencias previstas desde v1. Un caso nuevo comprueba que todo registro con `contract` es no reconstruible con v3.

## 8. Autotest (sin lanzar): 114 de 114

`bash selftest_m7.sh`; informe en `selftest-report.json`. Grupos: A {'Holding': 6, 'Total': 6}, B {'Holding': 9, 'Total': 9}, D {'Holding': 7, 'Total': 7}, G {'Holding': 48, 'Total': 48}, N {'Holding': 25, 'Total': 25}, P {'Holding': 9, 'Total': 9}, S {'Holding': 3, 'Total': 3}, U {'Holding': 7, 'Total': 7}. Codex y el pwsh del runtime no se ejecutaron.

## 9. Condiciones que solo se verifican en la sesión real

Las de v1, §10, con la versión 26.1023 sin medir: identidad real antes y después, autenticación, forma de registro de las órdenes, shell y modo
de lenguaje, sobres sin truncar, alias de Python en el sandbox, `git` por nombre, tope de 600 s, fidelidad del modelo, modelo y effort efectivos.

## 10. Archivos

`FREEZE.json` lista el SHA-256 y el tamaño de todos los archivos del kit, el texto armado, la instantánea del `-C`, el oráculo, la designación,
E, el cierre, el registro, la validación y las dos reglas. Su SHA-256 (`M7-KIT`) se da fuera del kit: en el README de la custodia de s100 y en
la evidencia (no puede estar en un archivo congelado sin circularidad).
