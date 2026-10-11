# Bloque A2-P2, sonda 2 reasignada al Controller (CONTROLLER_V5): resultado (R20261011T102825Z-a2p2-v5-controller)

> Supervisión, plano a. Autoridad: decisiones §73, puntos 4-7 (opción B), sobre el bloque A2-P2 de §69 (OD-2 = A y `BLOQUE-CODEX-CONSUMO` = A
> del Owner). Kit congelado en `2304d5ff` (`kits/FX-02/a2p2-v5-run`); texto armado por las puertas `6437e184…`, igual al lanzado y distinto del
> de la sonda 1. **Una sola invocación, sin reintento. Con ella el bloque queda agotado (2/2).**

## Hechos

- `codex exec` read-only con la receta 16.4 literal, sin shell declarada; celda observada `gpt-6-luna` / `high`;
  `-C D:\r62-fixture\A2`. **Salida 0 en ≤ 415 s de 600**, sin tope. Identidad igual antes y después (huella `F909F480…`,
  `d13914ce…`, `26.1007.2314.0`, acompañantes iguales); `HEAD` y árbol sin cambios; `status --ignored` sin cambios durante la corrida; escenario
  retirado sin diferencias; origen estable.
- **Ejecución completa:** 42 invocaciones (38 correctas), todas con el `pwsh` del runtime;
  ninguna escritura, red ni `ls-remote` completados; **sin F6-OBS-03**. Respuesta de 28 375 caracteres con los 24 pasos y los dos
  objetos canónicos (válidos contra sus esquemas). Uso: 39137 tokens de salida (18845 de razonamiento).
- **Veredicto de `compare`: NOT_DEMONSTRATED** (§73.7: ninguna elegibilidad por resultados parciales). 6 claves de
  paso fallidas, más un vínculo de evidencia y dos comprobaciones de los objetos canónicos.

## Por operación y acción

| Operación / acción | Demostrada |
|---|---|
| `OP_U0_utf8` | **no** |
| `OP_A_ejecucion` | sí |
| `OP_B_git_y_hashes` | sí |
| `OP_C_fidelidad_de_entradas` | **no** |
| `OP_D_trx_y_ci` | sí |
| `OP_E_mapa_I62` | **no** |
| `OP_F_esquemas_canonicos` | **no** |
| `PLAN_delegation_v2` | **no** |
| `VERIFY_controller_verification_v2` | **no** |

`Fidelity.AllFaithful` = True: todas las claves de fidelidad posteriores y todos los campos canónicos con texto no ASCII son
exactos. `AuthorityResolution` exacta. Los 14 `Checks`, `Classification`, `Disposition` y `FailureClass` coinciden con la referencia.

## Fidelidad UTF-8 del `pwsh` nativo (paso 0, medido)

- Modo de lenguaje: **ConstrainedLanguage**.
- Salida por defecto: **no fiel** (U+FFFD en la salida cruda de C1 y C2).
- `[Console]::OutputEncoding`: **no se puede fijar** («Cannot set property. Property setting is supported only on core types in this language mode.»).
- `$OutputEncoding`: se fija sin error, pero la salida de la consola sigue sin ser fiel (relectura en C2 con U+FFFD).
- **Método fiel de solo lectura: `OTHER`**, python -c lee UTF-8 y emite JSON con ensure_ascii=True para preservar puntos de código. Comprobado exacto en C4 y aplicado después: la fidelidad de
  todas las lecturas posteriores y de los objetos canónicos es exacta.
- Cada invocación avisa de que el `profile.ps1` no se puede cargar por el modo de lenguaje (la receta no pasa `-NoProfile`). No impidió ninguna lectura.

## Causas (NOT_DEMONSTRATED), clasificadas (`failure-classes.json`)

**Errores de la celda (inequívocos):**
- paso 0 a): T1 se leyó con un método que ConstrainedLanguage bloquea y no se repitió; puntos de código con claves mal nombradas (`OP_U0`);
- paso 12: `json:Schema` sin comillas, contra la instrucción (`OP_C`);
- paso 10: cita `C43` con 42 invocaciones (`OP_C`, vínculo);
- PLAN: `IssuedBy` sin el paréntesis final;
- VERIFY: la `Evidence` de Scope omite 1 de 15 rutas, presente en su propia salida.

**Ambigüedad del oráculo o del texto (no decide el veredicto):**
- paso 21: `mv4-mismatches` = 31 frente a 7; el oráculo cuenta MV-4 literal, pero `clause_map.py` informa ×31, igual que la celda;
- paso 21: `mv7-pointer-16` y `-163` con la frase del puntero, porque la descripción de la clave no pide yes/no expresamente.

`OP_E` es el único grupo que falla solo por estas dos ambigüedades; `OP_U0`, `OP_C`, PLAN y VERIFY fallan por errores inequívocos, así que el
veredicto no depende de ellas.

## Frente a la sonda 1

La omisión dominante desapareció: la sonda 1 hizo 4 comandos en ~105 s con 83 claves omitidas; esta, 42 invocaciones en ≤ 415 s, sin ninguna
omisión de paso. Lo que queda son errores puntuales de copia, formato y vínculo.
