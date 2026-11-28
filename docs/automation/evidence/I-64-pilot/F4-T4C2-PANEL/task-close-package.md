# I-64 — F4-T4C2-PANEL: paquete de cierre

**Resultado: CLOSED / PASS WITH LIMITATIONS**, en el intento 2 de 3.

**Autoridades del cierre:**
- la decisión del Owner «Corregir en intento 2» (`F4-DRAFTS/coordinator/owner-f4t4c2-attempt2-decision.md`), que fija el cierre por la regla de D2;
- la decisión D2 (`owner-f4t4c-decisions-d1-d6.md`).

Se cumplen todas las condiciones de D2:
- las dos revisiones con 0 bloqueantes y 0 importantes;
- la CI y la verificación en verde;
- nc1 y nc2 por un sustituto con la firma EXACTA de PD-F4-04 y de PD-F4-01, conservando su salida ordinaria, y nc3 por su oráculo ordinario;
- ningún modo de fallo nuevo.

## 1. Estado resultante

- **El producto:** `b88dabf2`. Pone la UI de Actualizar en el campo del piloto del panel:
  - el botón «Actualizar», con su manejador contenido, ofrecido solo en Dirty, sobre una superficie fijada y sin vuelo;
  - el gesto `Update()`, en el orden del paquete: el botón, lo pendiente, la frontera Update, `RequestUpdate` una vez y la recogida;
  - la presentación de cada `EditUpdateOutcome` sin reclasificar, con Unknown nunca como fallo ni como éxito y `ClosedInError` nunca como éxito (M-03);
  - las cinco reglas de F-1;
  - el diagnóstico diferido de T4A2, de la misma lectura y antes de `CheckNow`.
- **La frontera de INV-F4T4C-23:** en Application solo cambian el cuerpo de `TryCheckPilotEdit` y `EditCheckOutcome`. La integración de F4-T4C1-HOST y de T4B queda intacta.
- **El producto de F4-T4C queda completo** (H0, T4C1 y T4C2, D-T4C-19), y con él el de **F4-T4**.
- **Smoke-2 sigue pendiente y es obligatorio.** Es el §7 del paquete de T4C, que empieza solo después de cerrar ese producto.
- **`attempts`:** 2 de 3. El intento 3 no se gastó.
- **main** sigue en `812baf87`.

## 2. Corridas

**Intento 1** (custodia `bbc259dd`, F4-E69):
- cuatro planificaciones:
  - `af0a`, rechazada por la entrada 102 omitida;
  - `6c20` y `3d22`, rechazadas por un espacio al principio de la entrada 71;
  - `4797`, aceptada. El Owner autorizó la tercera y la cuarta, y la cuarta quitó del prompt la cadena errónea que el propio prompt citaba;
- nc4 `2c68`;
- el Worker `9b55`: RED `a6511c04` y parada antes del GREEN por tres defectos de sus propias pruebas. El producto quedó en un parche.

**Intento 2, la corrección** (contrato `gate-contract-attempt2.json`, BaseSha `a912f645`):
- **la planificación** `R20261115T195135Z-d34e`: fidelidad PASS y A1-A8 8/8, a la primera;
- **el Worker** `R20261115T195540Z-468f`: `claude-sonnet-5-5`/high y 91 turnos. RED `f1f1c7ee`, solo con las tres pruebas, y GREEN `b88dabf2`, solo con `src/`. El GREEN es **idéntico** al parche custodiado: el mismo `patch-id` y las mismas 317 líneas cambiadas;
- **la CI exacta:**
  - GREEN `43758023582`, 4/4: UI Workspace 694/694;
  - RED `43756155557`: Core en failure (10 de 3437) y UI en failure (75 de 694), todo dentro de los filtros;
- **la verificación** `R20261115T205330Z-c877`: **EXECUTION_VERIFIED, 14/14**, `VerifiedSha` `b88dabf2`.

## 3. Revisiones y controles

**Revisiones** (de todo T4C2, `1cce48c0..b88dabf2`):
- **la directa de la sesión** (`direct-review.md`): **0/0/2**;
- **la adversarial independiente** (`independent-review.md`): **ACEPTABLE CON MENORES, 0/0/5**, sin hallazgos OWNER. Las dos decisiones conservadoras de la entrega son fieles a la regla 3 de D-T4C-03:
  - el resultado no se recoge en una vista previa ni con el campo en Error;
  - los avisos se pierden si la superficie cambia.

**Controles** sobre las entradas de `c877`:

| Control | Corrida | Salida ordinaria | Resultado |
|---|---|---|---|
| nc1 | `R20261115T205814Z-685c` | CONTROL NOT PASSED: Handoff primero y luego Identity, con BLOCKED/STOP y S-04 | **Firma exacta de PD-F4-04.** Sustituto `F4T4C2-NC1-IDENTITY-SIGNAL-01`: PASS, determinista, 7/7 |
| nc2 | `R20261115T210357Z-11b3` | CONTROL NOT PASSED: Contract primero y luego Scope, con la ruta quitada, BLOCKED/STOP y S-04 | **Firma exacta de PD-F4-01.** Sustituto `F4T4C2-NC2-EXACT-WHITELIST-01`: PASS, determinista, 10/10 |
| nc3 | `R20261115T211015Z-bda6` | **CONTROL PASS** (FreeText, REWORK) | ordinario |

La primera medición de procesos de entrada de nc2 contó como participantes dos `bash.exe` de la propia sesión, que llevaban la ruta del worktree: el falso P-02 conocido. Se repitió desde su script, con 0 participantes. La medición falsa se conserva aparte, en `F4-T4C2-PANEL-nc2-substitute/2/R20261115T210357Z-11b3/processes-entry-false-p02.json`, con SHA-256 `88cb6920772a647197666c45dbfc8eebb973bd8d1072ba8abcfff525193858be`.

## 4. Limitaciones y arrastres

**Los menores de las revisiones:**
- **M-01:** «Aplicando» y el botón oculto pueden quedar obsoletos tras el Error del campo, porque `_inFlight` solo se refresca con `PresentProgress` y con el cambio de superficie. «Recargar» solo repinta.
- **M-02:** la guarda de la fila 18 no examina los cuerpos de las ayudas de presentación de nombre distinto.
- **M-03:** queda un comentario obsoleto en la cabecera de `PresentCheck`.
- **M-04:** el diff del rango completo incluye 86 rutas de `docs/` de los commits de custodia de la sesión. El alcance del Worker se comprueba por sus tramos, y es exacto.
- **M-05:** la tabla H-19 y H-21 del paquete de F4-T4A2 queda desfasada tras D-T4C-12. Es documental.
- **De la revisión directa:**
  - D-1: el botón exige que no haya una elección de T4A2 pendiente;
  - D-2: la no recogida en la vista previa o con el campo en Error.

**Tratamiento:**
- ninguno cambia la escritura ni el comportamiento ratificado;
- M-01 y M-02 son candidatos a una corrección de la UI, y M-01 lo observa Smoke-2;
- M-03 y M-05 son documentales;
- M-04 es de proceso.

Se declaran como limitaciones.

**KnownLimitations de la entrega:**
- el producto lo cubren pruebas de Core y de UI, sin AutoCAD;
- la primera validación física es Smoke-2;
- no se afirma nada de Undo antes de Smoke-2.

**Smoke-2** (§7 del paquete), ahora desbloqueado, con el producto, un dibujo real del Owner y el Owner como operador. La guía está preparada en borrador.

**Deudas que siguen:** H-1, PD-F4-07 y las PD-F4-01 y PD-F4-04, que se repitieron con su firma exacta.

## 5. Siguiente

1. **Smoke-2:** la sesión cierra la guía del operador con los nombres y textos finales del producto, y el Owner elige el dibujo.
2. **Después:** el cierre formal de F4, sujeto a la custodia.
