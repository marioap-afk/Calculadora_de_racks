# I-64 — F4-T4A1-PREPARE-CORE: paquete de cierre

Lo prepara la sesión responsable el 2026-11-14. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E51 a F4-E53.

**Autoridades del cierre:**
- la autorización de preparación de F4-T4 del Owner y del Coordinator (`F4-DRAFTS/coordinator/owner-f4t4-preparation-authorization.md`);
- las decisiones del Owner:
  - la descomposición (`owner-f4t4-decomposition-decision.md`);
  - la corrección en el intento 2 (`owner-f4t4a1-attempt2-decision.md`);
  - el cierre (`owner-f4t4a1-close-decision.md`: «Cerrar y arrastrar»).

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `bbd9a35e242788f7fa5864dccb3e9d3353b043ee`, CI `43554278196` push 4/4 |
| Verificación | `R20261114T043231Z-623b`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `443b5b3a` (la emisión del contrato del intento 2); RED `921e3a36`, GREEN `bbd9a35e` |
| Contrato | `gate-contract-attempt2.json` (AuthorityRevision `6d7c2223`), que parte de `gate-contract.json` (AuthorityRevision `c76f01da`, el paquete de F4-T4 v3) |
| Intentos | 2 de 3. El intento 1 se detuvo antes del RED, sin commits (F4-E52) |
| Config | Snapshot H (F4-E46) en todas las comprobaciones de la cadena |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E53; limitaciones en el §5) |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `4765` (intento 1) | aceptada a la primera | Fidelidad PASS en las ocho colecciones y A1-A8 8/8 |
| `3201` (nc4) | con su oráculo | Solo A3 en fail |
| `9d4c` (intento 1) | BLOCKED/STOP | F4T4A1-C01 y C02, sin commits: dos pruebas existentes fuera del alcance. Análisis y parche custodiados |
| `81dc` (intento 2) | aceptada a la primera | Fidelidad PASS con las 21 rutas y A1-A8 8/8, con `CorrectionOf` |
| `deb3` (intento 2) | entrega | Worker `claude-sonnet-5-5`/high, 182 turnos y 0 denegaciones, sin procesos en segundo plano. **RED** `921e3a36`: fallan 97 pruebas de Core por aserción, todas dentro del filtro, y UI pasa entera. **GREEN** `bbd9a35e`: las 21 rutas del contrato, sin tocar `tests/` |
| `623b` | `EXECUTION_VERIFIED`, 14/14 | Sobre `bbd9a35e`, con el RED acreditado |

**CI exacta:**
- **RED** `43553634960`: Core en failure; UI en success, como pide `ExpectRed` false. El build del Plugin no corre en el RED (`needs: tests`).
- **GREEN** `43554278196`: los cuatro jobs en success.
  - Filtros: 2843/2843 en Core y 458/458 en UI.
  - Suites completas: Core 18381/18381. En UI, 2095 pasadas, 0 fallidas y 17 condicionales ya existentes sin ejecutar.

## 3. Revisiones y controles

- **Revisión directa:** sin bloqueantes ni importantes, con 3 menores (`direct-review.md`).
- **Revisión adversarial independiente:** ACEPTABLE CON MENORES, 0/0/11 (`independent-review.md`). La clasificación y el destino de cada menor están en `analysis.md`.

**Controles** sobre las entradas de `623b`, con su oráculo ordinario y conservando cada salida ordinaria:

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261114T043649Z-86d0` | **CONTROL NOT PASSED**, con la firma exacta de PD-F4-04.<br>- Handoff es la primera en fail, porque la entrega contiene el SHA falso.<br>- Identity falla, con evidencia que nombra el SHA falso y el HEAD.<br>- `EXECUTION_BLOCKED` / STOP con S-04.<br>- Solo difieren Handoff e Identity.<br>Se conserva tal cual |
| sustituto de nc1, `F4T4A1-NC1-IDENTITY-SIGNAL-01` | `R20261114T044042Z-578c` | **PASS**: 7/7 propiedades y autoprueba determinista. Es copia del de F4-T3C2, con solo el identificador y el ámbito cambiados. nc1 = SATISFIED_UNDER_F4T4A1_NC1_IDENTITY_SIGNAL_SUBSTITUTE |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261114T044123Z-a383` | **CONTROL NOT PASSED**, con la firma exacta de PD-F4-01.<br>- Scope falla sobre `PushBackPilotMaterializer.cs`, con F4T4A1-C01.<br>- Contract falla antes, solo por la exactitud de la lista blanca (20 frente a 21).<br>- `EXECUTION_BLOCKED` / STOP.<br>Se conserva tal cual |
| sustituto de nc2, `F4T4A1-NC2-EXACT-WHITELIST-01` | `R20261114T044532Z-e01a` | **PASS**: 10/10 propiedades y autoprueba determinista. Es copia del de F4-T3C1, con solo el identificador y el ámbito cambiados. nc2 = SATISFIED_UNDER_F4T4A1_EXACT_WHITELIST_SUBSTITUTE |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261114T044552Z-d471` | **CONTROL PASS**: FreeText es la única en fail, `EXECUTION_REWORK_REQUIRED` / REWORK |

**Origen de las reglas de sustitución:** las dos están fijadas de antemano en la ExpectedEvidence del contrato de F4-T4A1 (PD-F4-01, PD-F4-04 y PD-F4-05, solo con firma exacta).

**Aviso de límite en el registro de nc1:** el registro de relevo cuenta un «limit reached». Es el texto de la parada S-11, que el Controller leyó del contrato; no es un aviso del CLI.

## 4. Producto entregado

El PREPARE de solo lectura de F4-T4, **sin escritura authored, MUTATE, POST, Undo ni UI**.

- **Núcleo de borradores (`DraftSession`):**
  - `ConfirmBase`, el «Comprobar ahora» general, con la tabla de cinco filas. En Conflict con MATCH, el borrador sigue en Conflict, así que INV-F4T1-11 no cambia.
  - La sobrecarga atómica de `KeepMineOnCurrentVersion` con `keepOn`, que es una ampliación declarada de INV-F4T1-04.
- **Host y puerto:**
  - las operaciones `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload`, que siguen los pasos de la comprobación y de la recarga, con la aplicabilidad antes de una sola lectura;
  - los tipos `EditConfirmStatus`, `EditConfirmOutcome`, `PilotUnknownOrigin` y `EditIntentStatus.Kept`;
  - tres valores opcionales nuevos en `PilotFieldView`;
  - los reenvíos de la compuerta y del controlador;
  - los textos A1-01 y A1-02.
- **El PREPARE puro** (`PushBackPilotUpdatePlan.cs`):
  - la instantánea `From`;
  - los siete resultados en orden, con Conflict antes de la admisión;
  - Invalid en los casos (a)-(e), entre ellos el de que el piloto nunca borra una vista;
  - el plan inmutable, con los diseños y los sobres salientes por vista compuestos sobre la relectura, y la expectativa de POST;
  - una sola llamada a `Compose`, censada.
- **Materializador:** los arrastres (1), (3) y (6) de F4-E25, más el (2) en «Conservar».
- **Calidad:** R-3 por guarda y C2-M-8.

## 5. Limitaciones y arrastres

- **El plan aún no tiene llamador.** Lo usará F4-T4B dentro de su transacción.
- **Fuera de esta subtarea:**
  - de F4-T4B y F4-T4C: la relectura caller-owned, MUTATE, POST, Undo, la importación y el contexto de comando;
  - de F4-T4A2: los gestos del panel;
  - de Smoke-2: la relectura real en un dibujo.
- **Los 14 menores de las revisiones**, con el destino de cada uno en `analysis.md` §2:
  - van a F4-T4A2, o a F4-T4B si es la primera que toca esa superficie;
  - M-1 a M-4, de calidad de guardas, se corrigen allí;
  - M-7 y M-11 se declaran aceptados.
- **Desviaciones registradas del intento 1:**
  - suites en segundo plano;
  - el rodeo de guarda, resuelto en el intento 2 con el uso declarado y una guarda antialias.
