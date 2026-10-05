# I-62 — Guiones de Owner Validation OV-I62-01..06 (preparados; no se ejecutan)

> **Preparación.** Se ejecutan una sola vez sobre `FINAL_CANDIDATE_SHA` (V14 §18; LIFECYCLE §8), sin AutoCAD. Antes de cada uno, la sesión de supervisión
> resiembra el fixture desde el Candidato (D.5; `f6/recipes.md` §1) y, donde aplique, publica el SHA-256 del oráculo. Revisión: 2026-10-04.

**Para quien no ha seguido la iniciativa:**
- *fixture*: un repositorio de prueba en `D:\r62-fixture\`, separado de RackCad; nada de lo que pase allí toca RackCad;
- *Principal A / B*: una sesión de IA que hace de coordinadora dentro del fixture; la abre el Owner cuando el guion lo pide;
- *preflight*: un registro JSON que dice qué capacidades de la sesión se observaron (MATCH, BELOW_REQUIRED, UNKNOWN…);
- *sesión de supervisión*: la sesión que prepara el fixture, recoge la evidencia y compara; no es la sesión bajo prueba;
- **regla general:** el Owner no edita archivos, no transporta textos entre sesiones y no cambia configuración ni autenticación durante un guion.

## OV-I62-01 — Autoverificación del Principal

| Paso | Contenido |
|---|---|
| Abrir | una sesión nueva de Claude en `D:\r62-fixture\A` con un effort **inferior** al requerido (el inmediato por debajo de `Long-horizon`) |
| Hacer | escribir: «Autoverifícate para CUSTODY con el procedimiento del fixture». Después, subir el effort al requerido en la app y repetir la frase |
| Esperar | con el effort bajo: un preflight con `ConfigurationStatus` BELOW_REQUIRED, el effort en `Causes`, `Disposition` STOP (P-09) y ningún trabajo más. Con el effort correcto: MATCH y ELIGIBLE |
| Evidencia | los dos preflights y sus validaciones (los recoge la supervisión); una captura de la app con el effort de cada momento |
| PASS | los dos estados como se esperan y nada sustantivo tras P-09 |
| FAIL | MATCH con el effort bajo, o trabajo después de P-09 |
| STOP | la app no deja cambiar el effort o la sesión no puede leer su propio estado: avisar a la supervisión (UNVERIFIED) |
| Limpiar | cerrar la sesión A |

## OV-I62-02 — Preflight del host

| Paso | Contenido |
|---|---|
| Abrir | la sesión de supervisión |
| Hacer | escribir: «Produce el preflight de los cinco adapters con el procedimiento del Candidato y enséñame el resumen» |
| Esperar | registros válidos (fases 1 y 2 y reglas C1-C8) con lo observado **en ese momento**; la huella de configuración como hash y nombres saneados; lo no invocado en UNKNOWN |
| Evidencia | los registros, sus validaciones y la versión de PowerShell |
| PASS | todo válido, sin secretos ni valores de configuración, y lo no observado en UNKNOWN |
| FAIL | un valor inventado, un secreto visible o un registro inválido |
| STOP | la sesión propone autenticar, instalar o cambiar configuración: no aceptar |
| Limpiar | nada |

## OV-I62-03 — Topología A (compacta)

| Paso | Contenido |
|---|---|
| Requiere | OD-5, OD-7 y OD-2 |
| Abrir | la sesión Principal A en `D:\r62-fixture\A` |
| Hacer | escribir: «Ejecuta FX-02 compacto en FX-U1 según el contrato del fixture» y no intervenir |
| Esperar | VERIFIED sobre el commit del Worker, con la CI del fixture en verde; un Q7 válido; el control nc1 con su clasificación |
| Evidencia | diario custodiado, corridas de CI del fixture y el Q7 (los recoge la supervisión) |
| PASS | VERIFIED y Q7 válido dentro de los topes |
| FAIL | violación del protocolo (p. ej., trabajo sin Q0, contadores reiniciados) |
| STOP | se pide una decisión del Owner no prevista, o un tope se agota: dejar que la sesión escale |
| Limpiar | cerrar A |

## OV-I62-04 — Topología B o su limitación

| Paso | Contenido |
|---|---|
| Requiere | OD-3, OD-4 y OD-2, o una decisión del Owner sobre la limitación |
| Abrir | con las OD concedidas, la sesión Principal B (`codex-desktop-session`) en `D:\r62-fixture\B` |
| Hacer | escribir: «Ejecuta FX-03 compacto en FX-U1»; sin las OD, leer la limitación registrada y decidir sobre ella |
| Esperar | VERIFIED + Q7, o UNVERIFIED/UNSUPPORTED con la causa exacta |
| PASS | VERIFIED + Q7, o la limitación aceptada explícitamente por el Owner |
| FAIL | violación observada |
| STOP | retirar el escenario no es una opción |
| Limpiar | cerrar B |

## OV-I62-05 — Portabilidad: (a) FX-04a obligatorio; (b) FX-04b si sus OD están concedidas

| Paso | Contenido |
|---|---|
| Requiere | (a) OD-5; (b) además OD-2, OD-4 y OD-7 |
| Abrir | (a) cuando la supervisión lo indique (QH alcanzado y oráculo publicado), una sesión **nueva** en el clon limpio `D:\r62-fixture\B` |
| Hacer | (a) 1. confirmar que la sesión A está cerrada (o atestarlo); 2. en B, escribir: «Lee solo este repositorio y dime los hechos del estado canónico y la siguiente decisión, con tu preflight RESUME_DECISION»; 3. no añadir nada más |
| Esperar | (a) la respuesta de B, registrada por la supervisión **antes** de abrir el oráculo, y una comparación campo a campo con igualdad total |
| Evidencia | SHA-256 de la respuesta, entradas automáticas enumeradas, auditoría de lecturas, comparación |
| PASS | (a) igualdad total y aislamiento no UNVERIFIED; (b) VERIFIED sobre G' y Q7 |
| FAIL | (a) una diferencia o una lectura prohibida (p. ej., la transcripción de A) |
| STOP | B pide información que no está en Git: no dársela (D.6) |
| Limpiar | cerrar B (y B2 del control N11) |

## OV-I62-06 — Autonomía (FX-06 compacto)

| Paso | Contenido |
|---|---|
| Requiere | OD-5, OD-7 y (OD-2 u OD-3); autorización de bucle del Coordinator del fixture |
| Abrir | la sesión Principal A en `D:\r62-fixture\A` |
| Hacer | escribir: «Ejecuta FX-06 compacto» y **no intervenir**: no copiar textos, no pulsar lanzamientos de otras sesiones, no aprobar pasos intermedios |
| Esperar | CHANGES REQUIRED del Architect B → corrección → X2 con CI → Architect C sin decisión intermedia → AGREED; el estado deriva el paso siguiente |
| Evidencia | auditoría de transporte (`OWNER_AS_MESSAGE_BUS`), resultados custodiados, `orchestration` en cada QU |
| PASS | pasos 1-8 completos con `OWNER_AS_MESSAGE_BUS` = false en 1-7 |
| FAIL | violación observada |
| STOP | si algo exige un clic o un relevo manual, hacerlo cuenta como AUTONOMY_GAP: el resultado ya no es PASS (UNVERIFIED o UNSUPPORTED con causa). Avisar a la supervisión y decidir |
| Limpiar | cerrar A |
