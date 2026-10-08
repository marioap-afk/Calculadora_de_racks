# P1b — paquete exacto para el Owner: medición de `codex sandbox` con el binario vigente (BORRADOR)

```text
Naturaleza:  SOLO SUPERVISIÓN (plano a). Borrador de un paquete de decisión del Owner, «solo si sigue siendo necesario tras A-2». NO es una
             autorización, no se ha presentado y nada de lo que describe se ha ejecutado.
Base:        decisiones §55, U-05 (P1b no está cubierta por la autorización estrecha de OD-2d-PROBE; no se ejecuta ahora; si sigue siendo necesaria
             tras A-2, la supervisión prepara un paquete exacto para el Owner) y §13 (mientras A-2 esté pendiente, nada de trabajo de sandbox de P1b).
             README de esta línea §2.1 P1b, OQ-22 (DECIDIDA), OQ-27 y OQ-28; architect-invocation-contract.md §7.
Autoridades: AUTOMATION_PLAN 16.4 (Entrada: P-01; Procesos); adapter codex-cli («Límites»: invocar la CLI puede reescribir config.toml; el pwsh del
             sandbox corre en ConstrainedLanguage); V14 §20.3.3 (fidelidad por el mismo camino) y §20.11 GAP-12; decisiones §50 y §53 (codex-cli
             en P-01 / STOP), §51 (forma de OD-2d-PROBE: huella antes y después de cada operación; sin workspace-write ni cambios de config.toml,
             trust_level o sandbox), §54 (fila «Codex / OD-2d»).
Valores:     ningún valor esperado; el paquete dice qué se ejecuta y qué se registra, no qué debería salir.
```

## 0. Cuándo se presenta y cuándo caduca

- **Se presenta** solo después de que A-2 esté dispuesta (AGREED o no) y solo si P1b sigue siendo necesaria (decisiones §55, U-05). Si no lo es, este
  borrador se archiva sin uso.
- **Caduca** (no se ejecuta nada y hay que rehacerlo) si, antes de ejecutar, cambia el binario, la versión de la app o la huella de `config.toml`
  respecto de §1, o el HEAD de `D:\r62-fixture\arch` respecto del que fije la presentación.
- La autorización es del **Owner**; el orden frente al bloque de medición de A-2 y a OD-2d (OQ-28) y el tope contra el que cuenta, si cuenta contra
  alguno (OQ-27), los decide el **Coordinator de I-62** antes de presentarlo.

## 1. Identidad exacta de lo que se mediría

| Elemento | Valor que el paquete fija | Fuente |
|---|---|---|
| binario | ruta verificada `%LOCALAPPDATA%\OpenAI\Codex\bin\979a96ce184041d1\codex.exe`, SHA-256 `97c57e4e…` (valor completo en el resultado OD-2 vigente), único en el directorio de binarios; nunca «el más reciente» | evidencia §71, §75; adapter `codex-cli` «Límites» |
| versión de la app | `26.1002.6548.0` | evidencia §71, §75 |
| huella de `config.toml` | la vigente al presentar (`9EA26634…` a las 08:20Z del 2026-10-07, evidencia §77) y, si ya existe, la aceptada en OD-2d; un valor distinto en F-0 = no se ejecuta nada | evidencia §75, §77; 16.4 (Entrada) |
| mismo binario que | el bloque de medición de A-2 (P1) y el paquete OD-2d (P2): P1b mide el camino de lectura del binario que usará el revisor | README §2.1 P1, P2 |

## 2. Orden (pendiente: OQ-28)

`<orden que fije el Coordinator: antes del bloque de A-2 | después del bloque y antes de OD-2d | después de OD-2d>`. Consecuencias que anota la
supervisión, sin recomendar ninguna: un cambio de la huella causado por P1b es P-01 / STOP en cualquier orden; si P1b va después del bloque de A-2 o
después de OD-2d, ese cambio deja además caducada la observación de la celda del Architect (decisiones §55, U-02(b): la huella es invalidador) y OD-2d
dejaría de describir la huella vigente.

## 3. Operaciones (exactamente estas; sin reintentos)

Ejecutor: la sesión de supervisión (plano a). Ninguna sesión del fixture, ninguna escritura en el fixture, ningún mensaje a sesiones del fixture.

| Id | Operación | ¿Ejecuta el binario? | ¿Trabajo de modelo? | Directorio | Qué se registra |
|---|---|---|---|---|---|
| F-0 | observación pasiva inicial (conjunto F de §4) | no | no | — | línea base de la medición |
| O-1 | forma A: `codex sandbox` → `cmd /c type <ruta con barras invertidas>` sobre `<archivo del cierre previsto, ≤ 24 000 bytes, con caracteres no ASCII>` | sí (`codex sandbox`) | **no** | lectura en `D:\r62-fixture\arch` | orden exacta, inicio y fin UTC, código de salida, SHA-256 de la salida, comparación con los bytes canónicos del blob (normalización solo de fin de línea, §20.3.3), caracteres no ASCII conservados y número de U+FFFD, presencia y posición de la línea de diagnóstico del perfil |
| F-1 | conjunto F | no | no | — | comparación con F-0 |
| O-2 | forma B: rango de líneas con M ≤ 150 sobre `<archivo del cierre previsto, > 24 000 bytes>`: `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Get-Content -LiteralPath <ruta> -Encoding utf8 \| Select-Object -Skip N -First M"'` | sí | **no** | `arch` | igual que O-1, por tramo |
| F-2 | conjunto F | no | no | — | comparación con F-1 |
| O-3 | forma C: búsqueda con un patrón no ASCII presente en `<archivo del cierre previsto>`: `cmd /c 'chcp 65001 >nul & pwsh -NoProfile -Command "Select-String -LiteralPath <ruta> -Encoding utf8 -Pattern <patrón> \| ForEach-Object { [string]$_.LineNumber + [char]58 + $_.Line }"'` | sí | **no** | `arch` | igual que O-1, por línea encontrada |
| F-3 | conjunto F | no | no | — | comparación con F-2 |
| O-4 | control negativo: `Get-Content` directo en la consola por defecto sobre el archivo de O-2 | sí | **no** | `arch` | igual que O-1 |
| F-4 | conjunto F | no | no | — | comparación con F-3 |
| O-5 | diagnóstico del perfil, envoltorio exterior **con** perfil: la lectura de O-1 con el `pwsh` del runtime como `pwsh -Command '<orden>'` | sí | **no** | `arch` | igual que O-1, con la forma exacta del envoltorio |
| F-5 | conjunto F | no | no | — | comparación con F-4 |
| O-6 | diagnóstico del perfil, envoltorio exterior **sin** perfil: la misma lectura como `pwsh -NoProfile -Command '<orden>'` | sí | **no** | `arch` | igual que O-5 |
| F-6 | conjunto F | no | no | — | comparación con F-5 y con F-0 |

- **Rutas y patrones:** los marcadores `<…>` se sustituyen al presentar el paquete por rutas relativas a `arch` con su blob en el HEAD fijado, elegidas
  por los criterios de la columna (formas del contrato §7); nunca una ruta fuera de `arch`.
- **Sin reintentos:** una operación que falle o dé un resultado no interpretable se registra tal cual y la medición se detiene; repetirla exige volver al
  Owner.
- **Tope por operación:** propuesta de 600 s con el árbol del proceso terminado y su muerte confirmada (análogo al tope de 16.4; no es texto congelado
  para `codex sandbox`). La autorización lo fija.
- **Ninguna otra operación:** ni `codex exec`, ni `--version`, ni `login status`, ni lectura de credenciales, ni registro o exposición de valores de
  `config.toml` (solo el SHA-256 del archivo, los nombres saneados y la comparación HMAC por clave calculada en proceso, como en evidencia §71 y
  decisiones §51 «estructura saneada y comparación por clave»), ni `workspace-write`, ni cambio de `config.toml`, `trust_level` o el modo de sandbox.

## 4. Observaciones de la huella antes y después de cada operación (conjunto F; pasivas)

| Observación | Cómo | Regla |
|---|---|---|
| SHA-256 de `%USERPROFILE%\.codex\config.toml` | hash del archivo, sin leer valores | distinto de F-0: **P-01 / STOP**, ninguna operación más y vuelta al Owner (decisiones §51, fila OD-2d-PROBE) |
| nombres saneados de secciones y claves y comparación por clave | lista ordenada sin valores y comparación HMAC por clave calculada en proceso (ningún valor se registra ni se expone), como en la evidencia §71 | una clave nueva, retirada o cambiada: P-01 / STOP y diff de nombres registrado (16.4, Entrada) |
| binario | ruta, SHA-256 y unicidad en `%LOCALAPPDATA%\OpenAI\Codex\bin\` | distinto de §1: STOP |
| versión de la app | versión instalada | distinta de §1: STOP |
| `D:\r62-fixture\arch` | HEAD, `git status` limpio, remotos | cambio: STOP y registro (la medición no escribe en `arch`) |
| procesos | lista cerrada de 16.4 con la ruta de `arch` en la línea de órdenes; `codex-windows-sandbox-service.exe` (excluido por nombre en 16.4) registrado con PID y `CreationDate` | ningún proceso de la operación vivo después de cada O-k |
| registros de sesión | número, nombres y tamaños de las entradas nuevas en `%USERPROFILE%\.codex\sessions\`, sin leer su contenido | se registran; ninguna regla de esta línea los da por esperados ni por ausentes |

## 5. Implicaciones del sandbox

- `codex sandbox` ejecuta las órdenes en el sandbox de Windows de `codex-cli`, con la misma cuenta de sandbox y el mismo `PATH` que `codex exec`
  (`docs/automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/README.md` §3, con `codex-cli` 0.159.2: «Es el mismo binario, la misma
  cuenta de sandbox y el mismo `PATH` que usa `codex exec`»; hecho de un binario anterior); el `pwsh` del sandbox corre en
  ConstrainedLanguage (adapter `codex-cli`, «Límites»).
- Es una ejecución del binario de Codex con P-01 abierto para su uso futuro (decisiones §50, §53): por eso exige una autorización del Owner que la
  nombre (16.4, Entrada: «ninguna invocación de Codex más hasta que decida el Owner»), y por eso la huella se observa antes y después de cada
  operación (adapter `codex-cli`, «Límites»: invocar la CLI puede reescribir `config.toml`).
- Una primera ejecución del subcomando con el binario vigente puede crear o actualizar artefactos propios del sandbox; esta línea no sabe cuáles.
  Lo observable se registra con el conjunto F; lo que F no cubre queda fuera de la cobertura declarada.
- Límites de lo que mide: P1b mide el camino de `codex sandbox` con el envoltorio que elige quien lo llama. No fija qué envoltorio usará `codex exec`
  en las llamadas de herramienta del modelo (OD-2d-PROBE observó ambas formas con el binario anterior), ni la captura interna de `codex exec`
  (truncamientos: GAP-10, GAP-11). La comprobación tras la corrida sobre el registro de sesión sigue siendo la decisiva (contrato §7).

## 6. Consumo

| Concepto | Consumo |
|---|---|
| invocaciones de modelo | **0**: `codex sandbox` no invoca ningún modelo (evidencia §25 y §27: «`codex sandbox`, que no invoca ningún modelo»). Si apareciera una señal de uso de modelo (tokens, `turn_context`), se registra, la medición se detiene y se vuelve al Owner |
| ejecuciones de `codex sandbox` | exactamente 6 (O-1..O-6), sin reintentos |
| observaciones pasivas | 7 (F-0..F-6), sin ejecutar el binario |
| `architect_launches`, `transport_reruns` y demás topes del bucle de FX-06 (D.8; A-1) | ninguno: P1b es plano a y anterior a la ventana |
| fila «Sondas previas» de D.3 (agotada, decisiones §55, U-04) o bloque nuevo de A-2 (≤ 2 sondas `read-only`) | **pendiente: OQ-27** (si P1b cuenta contra alguno de los dos o contra ninguno) |
| tiempo | como máximo 6 × el tope por operación de §3 |

## 7. Resultado que se custodia

`R:` `docs/automation/evidence/I-62-F6/OD-2/<RunId>/result.json` (patrón de las mediciones OD-2): identidad de §1; por operación, la orden exacta, los
instantes, el código de salida, el SHA-256 de la salida, la comparación con los bytes canónicos y el conjunto F antes y después; la conclusión factual
(FAITHFUL, FAITHFUL_NORMALIZED o la degradación observada, y la presencia del diagnóstico del perfil con cada envoltorio). Las salidas en bruto quedan
fuera de todo clon del fixture; ningún valor de configuración, credencial ni transcripción en claro.

## 8. Línea de decisión propuesta para el Owner (sintaxis; no es texto de ninguna autoridad)

```text
P1b = A (medición de codex sandbox con el binario <SHA-256 exacto> y la app <versión exacta>: operaciones O-1..O-6 de p1b-owner-packet.md en D:\r62-fixture\arch@<HEAD>, sin modelo y sin reintentos; huella de config.toml, binario y versión de la app antes y después de cada operación, con comparación por clave; un cambio de la huella: P-01 / STOP; orden: <OQ-28>; tope contra el que cuenta: <OQ-27>; tope por operación: <segundos>)
P1b = R (no se autoriza)
```
