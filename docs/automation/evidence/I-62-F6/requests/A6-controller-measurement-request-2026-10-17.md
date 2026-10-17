# I-62 — Medición del Controller de FX-02 con el instrumento B-min (A6-7): solicitud para la identidad vigente (preparada; decisiones §90, puntos 3 y 4)

> Supervisión, plano a. **Preparación: no se pide nada y no se invoca nada.** Sustituye al borrador
> [`A6-controller-measurement-request-draft-2026-10-11.md`](A6-controller-measurement-request-draft-2026-10-11.md), que se conserva: binario, versión y
> huella cambiaron, así que esa solicitud no se reutiliza (decisiones §90, punto 4). El bloque A2-P2 del par anterior sigue agotado (2/2); la medición
> de A6-7 se cuenta aparte (A6-7, regla 3) y su consumo exige la línea (b) del Owner.

## 1. Identidad vigente de Codex (observación pasiva, sin ejecutar Codex)

Tres observaciones iguales el 2026-10-17 a las 04:58:01Z, 05:31:06Z y 06:12:15Z ([`m7/identity/`](../../I-62-A6/m7/identity/obs1.json)):

| Elemento | Autorizado antes (§73) | Observado |
|---|---|---|
| `BinaryHash` (único `codex.exe`) | `d13914ce…` | `9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d` (`%LOCALAPPDATA%\OpenAI\Codex\bin\e78bad9887fa50cf\`) |
| `AppVersion` | `26.1007.2314.0` | `26.1013.3656.0` |
| Huella (SHA-256 de `config.toml`) | `F909F480…` | `6EFA5B0E1E971F55CC925DB17550325A090FB9155398087C4089444E33DDBE1F`; mismos 107 nombres de claves que la línea base de `F909F480` |
| Acompañantes | `34b1a406…` | `2aaa7f5a6e240a4c` |
| pwsh del runtime | `362a356c…` | `362a356c…` (sin cambio) |

El estado de autenticación no es observable de forma pasiva: se comprueba en la sesión real.

## 2. Lo que el Coordinator tiene que disponer antes de la medición

1. **Lecturas normativas del aceptante (A6-5, regla 6).** Texto propuesto en [`m7/readings/readings.md`](../../I-62-A6/m7/readings/readings.md), §7:
   24 filas, ninguna divergencia de lectura; confirmar o corregir las 12 filas AMBIGUOUS-ACCEPTOR-CHOICE y la exclusión (d) (lecturas propias del
   aceptante en `Identity`, `Trailer`, `FreeText` y `CleanTree`, que seguirían la regla 6 con S-03).
2. **Divergencia D-1** entre instrumento y aceptante (U-19 en A3 y A4): solo rechazo falso; la medición no la activa, pero nc2 de la ronda de FX-02
   sí podría (A6-9, regla 2: la decide el Coordinator).
3. **Disposición de medición** (A6-7, reglas 3 y 7): una sola sección posterior a §90 con los marcadores literales del kit
   ([`a6m7-measure/README.md`](../kits/FX-02/a6m7-measure/README.md), §2) y, en ella, estas decisiones:
   - **Cierre de VERIFY:** R7-A, con la lectura que exige de RAE §15.2 I6 y de A6-2, regla 3: la comprobación «todo blob coincide con el de la
     `AuthorityRevision`» rige la clase de autoridades; el objeto verificado y los registros de la ventana se fijan por su blob en
     `I.Target.Commit`, y D, H, el TRX y el contrato por el blob de sus bytes custodiados (el contrato no existe en la AuthorityRevision). Con
     R7-B la medición no puede demostrar tres operaciones de A6-7, regla 1 ([banco](../../I-62-A6/m7/bench/README.md), §2, §5 y §8).
   - **Designación por rutas** (`a6m7-designation/1`) y reglas del kit KPR-1..KPR-3 (O3 y O6 de la ronda 4).
   - **Regla de reconstrucción de `Runs`:** `reconstruct_runs_m7.py`, SHA-256 `1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab`. La
     referencia de las rondas 3 y 4 (`5dc9f78e…`) no reconstruye ninguna ejecución hecha con el pwsh del runtime (rechaza el componente `.cache` de su
     ruta); v2 añade solo eso y dos representaciones de la orden documentadas en registros reales (`-NoProfile -Command`, `cmd.exe /c`).
   - **Paso de entrega exacta** con el nombre abreviado del objeto (`9a829b0eeb12`), para que no cuente como ejecución del instrumento.
   - **Lectura de `AuthorityResolution`** (V5 o R3; difieren en `RevisionSha` de las filas UNIT_*): el comparador acepta las dos y registra cuál
     coincide; la disposición puede fijar una.
   - **Kit:** `FREEZE.json` SHA-256 `459a3440af3d9b553d55d4cfc84b7a40ab3a58ab2ac178af01a23f144ba3cae8`; `-C` `D:\r62-a6m7\m7-run` con su instantánea
     `43a8caf6…`; DecisionRef `I62-A6-7-M7` para la escritura de preparación.

## 3. Líneas del Owner (después de la disposición de medición; no se piden todavía)

**OD-2 sobre la huella nueva** (A2-P2, regla 4; P-01: ninguna invocación con una huella sin aceptar):

```text
OD-2 = A (línea base 6EFA5B0E1E971F55CC925DB17550325A090FB9155398087C4089444E33DDBE1F; binario 9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d)
```

SHA-256 de la línea: `3b33dd56cbae5fddab9a9431e7ad9b83170e6782e0161385bc324789634b7f25`.

**Línea (b) de A-6 §7 con el trío vigente** (único cambio frente al texto acordado: el trío):

```text
A6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación de medición read-only de codex-cli, celda gpt-6-luna/high del EXECUTION_CONTROLLER de FX-02 en F6, con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete que nombre la disposición, para el trío exacto 9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d/26.1013.3656.0/6EFA5B0E1E971F55CC925DB17550325A090FB9155398087C4089444E33DDBE1F, con huella, binario y versión antes y después; fuera de A2-P2, sin reutilizar su bloque agotado ni la invocación consumida de A4-1; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; solo con A-6 AGREED, con A6-ADR0046-9-FX02 = A y con la disposición del Coordinator que la nombre; distinta de A6-ADR0046-9-FX02 y no implícita en ella; no acepta su resultado ni amplía OD-2 ni OD-5)
```

SHA-256 de la línea: `dda4b3f9c1db0183a38e5400777a2a21d524aa4b92ad684d5c5cae1b2fc4cf27`.

## 4. Opción previa que decide el Coordinator: caracterización del runtime nuevo

La medición es única y sin reintento (A6-7, regla 4), y la versión 26.1013 no está medida: cómo registra las órdenes, qué shell usa, si añade
`-NoProfile`, el modo de lenguaje y si el alias de Python arranca bajo su sandbox. A2-P2, regla 3, admite **un** bloque nuevo de hasta dos sondas de
solo lectura para el par nuevo, ya observado con huella estable, con una disposición que lo nombre y la autorización del Owner. Una sonda de
caracterización sin el instrumento reduciría ese riesgo antes de consumir la medición; exigiría su propio kit mínimo, que no está preparado, y esta
línea del Owner además de OD-2:

```text
BLOQUE-CODEX-CONSUMO = A (≤ 2 invocaciones read-only de codex-cli para el par exacto 9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d/26.1013.3656.0, con huella, binario y versión antes y después de cada una; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores)
```

Sin esa opción, la medición se lanza directamente con el kit, que tolera las tres representaciones documentadas y falla cerrado ante las demás.

## 5. Condiciones de lanzamiento

1. Las disposiciones de §2 y las dos líneas de §3 registradas; las líneas, completas y después de la disposición de medición.
2. Identidad antes igual al trío y a los demás campos de §1; cualquier diferencia es P-01 y exige otra OD-2 y otra línea (b).
3. La app de escritorio de Codex cerrada por el Owner: hoy hay procesos `codex.exe` vivos y la puerta P-02 rechaza.
4. I-64 sin trabajo crítico; ninguna sesión sobre `D:\r62-a6m7\m7-run`.
5. `python -I prep_write.py` (escritura de preparación de A6-3, regla 1) y, después, exactamente `bash run_m7.sh launch`: puertas, punto de uso
   (i)-(viii), marcador P-07, **un** lanzamiento con tope de 600 s, Entrada, reconstrucción, aceptante en el mismo `-C` y comparación.
6. Custodia del resultado y disposición de elegibilidad del Coordinator. El veredicto del comparador es una propuesta, nunca una declaración de
   ELIGIBLE.

## 6. Invalidadores (A6-7, regla 5; cualquiera anula la medición, sin reutilización)

`BinaryHash`, `AppVersion`, huella, estado de autenticación, modelo, effort, blobs de catálogo y de routing, instancia del host, blob del
instrumento, orden declarada, intérprete declarado, Git declarado, regla de reconstrucción de `Runs` y cierre de VERIFY.

## 7. Qué se verifica solo durante la sesión real

Identidad antes y después; autenticación; el punto de uso con el pwsh del runtime y su modo de lenguaje; la representación real de las órdenes en
26.1013; que el sobre llega sin alterar, separable de stderr y sin truncar (clausemap ronda los 44 KB); que el alias de Python arranca bajo el sandbox;
`git` por nombre en el entorno real; el tiempo; que el modelo copia literalmente las órdenes y completa el esquema; modelo y effort efectivos.

## 8. Qué no hace

No produce una verificación, ni VERIFIED, ni C-24 PASS (A6-7, regla 2); no usa ni entrega el oráculo de C-24; no crea B1 ni abre FX-U2; no declara la
celda elegible.
