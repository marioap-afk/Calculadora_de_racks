# I-62 — Guiones de Owner Validation OV-I62-01..06 (preparados; no se ejecutan)

> **Preparación.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33. La ejecución final es sobre `FINAL_CANDIDATE_SHA` (V14 §18; LIFECYCLE
> §8). Sin AutoCAD. Ninguno se ejecuta ahora. Antes de cada uno, la sesión de supervisión publica el fixture resembrado desde el Candidato (D.5) y el oráculo
> donde aplique. El Owner nunca cambia autoridades, contratos ni el estado real.

## OV-I62-01 — Autoverificación del Principal

- **Prerrequisitos:** OD-5; fixture resembrado desde el Candidato; FX-U1 en I62 tras G0; el procedimiento del README §12-§13 del Candidato.
- **Acción del Owner:** abrir en el fixture una sesión del sistema bajo prueba (Principal A) con un effort **inferior** al requerido (por ejemplo, el inmediato
  por debajo de `Long-horizon`). Pedirle que se autoverifique. Después, subir el effort al requerido y repetir.
- **Salida visible esperada:**
  - con el effort inferior, un `preflight/v1` con `ConfigurationStatus` BELOW_REQUIRED, `Causes` con el effort y `Disposition` STOP (P-09), sin trabajo
    sustantivo;
  - con el effort correcto, MATCH y ELIGIBLE;
  - cada observación, ligada a su instante de `get_session`.
- **Qué registrar:** los dos preflights con sus validaciones, la hora de cada cambio de effort y la captura de lo que muestra la app.
- **PASS:** los dos estados coinciden con lo esperado y nada sustantivo ocurre con BELOW_REQUIRED. **FAIL:** MATCH con el effort inferior, o trabajo tras P-09.
- **No cambiar:** el modelo, el perfil ni los requisitos de `routing.md`.

## OV-I62-02 — Preflight del host

- **Prerrequisitos:** el procedimiento de preflight del Candidato (README §13); el estado del host en ese momento.
- **Acción del Owner:** pedir a la sesión que produzca el preflight de los cinco adapters con el procedimiento del Candidato, y revisarlo.
- **Salida visible esperada:**
  - siete registros VALID (fases 1 y 2, C1-C8), con el estado observado **en ese momento**;
  - huella de `config.toml` como hash y nombres saneados, nunca valores ni credenciales;
  - las candidatas que no se invocaron, en UNKNOWN.
- **Qué registrar:** los registros, sus validaciones y la versión de PowerShell.
- **PASS:** todos válidos, sin secretos y con lo no observado en UNKNOWN. **FAIL:** un valor supuesto, un secreto o un registro inválido.
- **No cambiar:** la configuración ni la autenticación de ningún runtime.

## OV-I62-03 — Topología A (compacta, D.5)

- **Prerrequisitos:** OD-5, OD-7 y OD-2; binario medido.
- **Acción del Owner:** abrir el Principal A en el fixture y dejar que ejecute FX-02 compacto (Codex: planificación 1, verificación 1, nc1 1, pool 2; Worker 1).
- **Salida visible esperada:** VERIFIED sobre G con la CI del fixture; Q7 conforme a B.8.4; nc1 con su clasificación.
- **Qué registrar:** el diario custodiado, las corridas de CI del fixture y el Q7.
- **PASS:** VERIFIED y Q7 válido dentro de los topes. **FAIL:** violación del protocolo.
- **No cambiar:** el contrato ni los topes.

## OV-I62-04 — Topología B o su limitación

- **Prerrequisitos:** OD-3, OD-4 y OD-2, o la decisión del Owner sobre la limitación.
- **Acción del Owner:** si las OD están concedidas, abrir el Principal B y ejecutar FX-03 compacto; si no, decidir sobre la limitación registrada (el
  escenario no se retira).
- **Salida visible esperada:** VERIFIED + Q7, o UNVERIFIED o UNSUPPORTED con su causa exacta.
- **Qué registrar:** el resultado y su causa.
- **PASS:** VERIFIED + Q7, o limitación aceptada explícitamente por el Owner. **FAIL:** violación observada.
- **No cambiar:** retirar el escenario.

## OV-I62-05 — Portabilidad: (a) FX-04a obligatorio; (b) FX-04b si sus OD están concedidas

- **Prerrequisitos (a):** OD-5; QH alcanzado por A; oráculo publicado fuera del host (solo su SHA-256 en la evidencia).
- **Acción del Owner (a):**
  1. atestar la terminación de A, si no la observa la sesión de supervisión;
  2. abrir B (`codex-desktop-session`, o una sesión nueva de Claude registrada como variante) en un **clon limpio** del fixture en QH, sin la memoria de A;
  3. pedir a B su preflight RESUME_DECISION, los hechos y la siguiente decisión.
- **Salida visible esperada (a):** la respuesta de B (hechos, decisión y entradas enumeradas), publicada antes de abrir el oráculo; la comparación mecánica
  campo a campo.
- **Qué registrar (a):** el SHA-256 de la respuesta, la enumeración de entradas automáticas y la auditoría de lecturas.
- **PASS (a):** igualdad total y aislamiento no UNVERIFIED. **FAIL (a):** cualquier diferencia o una lectura prohibida.
- **(b):** con OD-2, OD-4 y OD-7, B continúa hasta VERIFIED sobre G' y Q7. Si no, el Owner decide la limitación, que no afecta a (a).
- **No cambiar:** entregar a B nada que no esté en Git (D.6).

## OV-I62-06 — Autonomía (FX-06 compacto)

- **Prerrequisitos:** OD-5, OD-7 y (OD-2 u OD-3); `ReviewLoopAuthorization` del fixture; binding de Architect con invocación medida.
- **Acción del Owner:** abrir el Principal A y observar, **sin intervenir**, la secuencia 1-8 de D.8.
- **Salida visible esperada:** CHANGES REQUIRED de B → corrección → X2 con CI → Architect C materializado sin decisión intermedia → AGREED; el estado
  canónico deriva el paso siguiente.
- **Qué registrar:** la auditoría de transporte (`OWNER_AS_MESSAGE_BUS`), los resultados custodiados y el estado `orchestration` en cada QU.
- **PASS:** pasos 1-8 completos con `OWNER_AS_MESSAGE_BUS` = false en 1-7. **FAIL:** violación observada. Si no, UNVERIFIED o UNSUPPORTED con causa, y el Owner
  decide (el escenario no se retira).
- **No cambiar:** transportar prompts o resultados a mano. Si ocurre, es AUTONOMY_GAP y nunca PASS.
