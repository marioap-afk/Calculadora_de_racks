# I-62 — Recetas de ejecución de F6 (preparación; nada se ejecuta ahora)

> **Preparación.** Fuente: Proposal V14 Anexo D (blob `34ad80ea`) y §17. F6 no está autorizado. Ninguna receta autoriza gasto, sesiones del sistema bajo
> prueba, remotos ni invocaciones: cada una nombra las decisiones del Owner que la desbloquean. Sustituye la parte de escenarios de `../f6-dossier.md`.

## 1. Fixture (D.1): diseño completo

**Directorio** (fuera de todo repositorio real; lo crea la sesión de supervisión al abrir F6, con OD-5):

```text
D:\r62-fixture\
  fixture-origin.git\                 remoto bare local (siempre)
  supervisor\                         clon de trabajo de la sesión de supervisión (plano a)
  A\ , B\ , B2\                       clones de los Principales del sistema bajo prueba (uno por sesión; B y B2 limpios, --no-local)
  evidence-out\                       salidas que se copian a la evidencia real de I-62 (solo hashes y artefactos saneados)
```

**Inicialización:**
1. `git init --bare -b main fixture-origin.git`; `git clone fixture-origin.git supervisor`; `core.autocrlf=false` en todos los clones del fixture.
2. **F_seed** en `main`: copias **byte a byte** de las autoridades de RackCad en MC_I62 (WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, `docs/automation/agent-execution/`,
   esquemas y `compatibility/`), un `AGENTS.md` del fixture marcado `FIXTURE_LOCAL` con los jobs `fixture-build` y `fixture-tests`, una biblioteca .NET mínima
   (una clase y una prueba xUnit) y `FIXTURE-MANIFEST.json` (`ruta → COPIED {blob de origen en MC_I62} | FIXTURE_LOCAL {razón}` + commit de origen).
3. Rama `fixture/i62-norm` con **F_norm** (trailer `Agent-Protocol-Normative: I-62`), fusionada con `--no-ff` en `main` como **F_eff**. El manifiesto la
   marca `TEST-ACTIVATION`. La derivación es por repositorio: RackCad no se activa.
4. Unidad **FX-U1**: commit de reclamo vacío con `Claim-Id` propio en `fx/u1` desde F_eff; push al remoto del fixture.
5. Arranque de §8.6: BOOTSTRAP (preflight CUSTODY y binding PENDING custodiados), decisión de G0 del Coordinator del fixture con los tres marcadores y QU con
   las aceptaciones.
6. `gate-contract/v2` de T1 (`ProtocolSet` I62; `AuthorityRevision` = bootstrap de FX-U1; `EXTERNAL` = F_eff).

**CI:** sin OD-7, el fixture no tiene remoto con CI y `Ci` = `not_run`; FX-02 y FX-04b no pueden ser PASS, y F6 no cierra (D.2). Con OD-7, un repositorio
**privado** del Owner en GitHub con un flujo `fixture-build` + `fixture-tests` (push, `ubuntu-latest`, `dotnet test`); el supervisor lo añade como segundo remoto.

**Identidades sintéticas:** autor y committer `fixture <fixture@example.invalid>`; fechas reales (no fijas) en el fixture vivo; los `Claim-Id` del fixture
empiezan por un prefijo reconocible y no figuran en ninguna tabla PRE real.

**Configuración y secretos de prueba:** ningún secreto real. Las pruebas de saneamiento usan los valores ficticios de C-10 (README §13.4). Una huella de
configuración del fixture es un archivo propio del fixture, nunca el `config.toml` del Owner.

**Test-null:** cuando un adapter no se puede invocar (OD pendiente), su fila se registra con el estado explícito (UNKNOWN, NOT_OBSERVED) y el escenario
queda UNVERIFIED con causa; nunca se simula una invocación.

**Limpieza:** borrar `D:\r62-fixture\` y, con OD-7, el repositorio del fixture lo archiva el Owner. Ningún ref del fixture se empuja a RackCad (FX-05 lo
comprueba).

## 2. Recetas

Formato de cada receta: arranque, archivos, órdenes, topología, adapters, decisiones del Owner, transiciones esperadas, evidencia, resultados y limpieza. La
evidencia real va a `docs/automation/evidence/I-62-F6/<FX>/<RunId>/` (saneada) y los topes son los de D.3; P-07 se comprueba antes de cada lanzamiento.

### FX-01 — autoverificación (C-23)

| Campo | Contenido |
|---|---|
| Arranque | fixture §1 hasta el paso 5 |
| Topología / adapters | Principal A = `claude-desktop-session` (sesión del sistema bajo prueba abierta por el Owner) |
| Decisiones del Owner | OD-5 (ensayo y apertura de la sesión) |
| Órdenes | dentro de la sesión A: tres preflights por procedimiento (README §12-§13), con el Owner cambiando el effort entre ellos; cada observación por `get_session` ligada al instante |
| Transiciones esperadas | ninguna de custodia: son turnos de la sesión, no invocaciones |
| Evidencia | `FX-01/<RunId>/preflights/*.json` + validaciones; disposición por acción (P-09 con effort bajo; MATCH con el correcto) |
| PASS / UNVERIFIED / UNSUPPORTED | PASS: las tres disposiciones correctas; UNVERIFIED: sin OD-5 o sin `get_session`; UNSUPPORTED: no aplica |
| Limpieza | cerrar la sesión A |

### FX-02 — topología A (C-24)

| Campo | Contenido |
|---|---|
| Arranque | FX-01 + contrato T1 |
| Topología / adapters | A (`claude-desktop-session`); Controller y Architect `codex-cli`; Worker y Reviewer `claude-subagent` |
| Decisiones del Owner | OD-5, **OD-7** (CI), OD-2 (huella de `codex-cli`) y la medición del binario |
| Órdenes | Q0 → planificación (Controller) → aceptación A1'-A8' → Worker (R y G) → verificación → negativos nc1-nc4 y N4-N10 → Q7 |
| Transiciones esperadas | BOOTSTRAP → QU → Q0 → (diario) → Q7 VERIFIED |
| Evidencia | `FX-02/<RunId>/` (contrato, delegación, relevos, verificación, controles, Q7) |
| PASS / UNVERIFIED / UNSUPPORTED | PASS: VERIFIED + negativos con su disposición; UNVERIFIED: falta OD-7, OD-2 o la medición; FAIL: violación del protocolo |
| Limpieza | ninguna (FX-04a continúa la unidad) |

### FX-03 — topología B (C-26)

| Campo | Contenido |
|---|---|
| Topología / adapters | Principal `codex-desktop-session`; Controller y Worker `codex-cli` (Worker con escritura); Reviewer y Architect `claude-cli` |
| Decisiones del Owner | OD-3 (autenticar `claude-cli`), OD-4 (escritura de `codex-cli`), OD-2, OD-5 |
| Resultado | PASS, o UNVERIFIED/UNSUPPORTED con causa para la decisión del Owner (OV-I62-04); el escenario no se retira |

### FX-04a — reanudación y decisión (C-25a): **lista para un comando tras OD-5**

| Campo | Contenido |
|---|---|
| Arranque | FX-02 hasta su Q7; si no llegó, QH tras BOOTSTRAP con T1 planificada (hechos más pobres, registrado así) |
| Topología / adapters | A (titular que libera) y B (`codex-desktop-session`, o una sesión Claude nueva como variante de mismo proveedor registrada aparte) |
| Decisiones del Owner | **solo OD-5**; no depende de OD-3, OD-4 ni OD-7 |
| Órdenes | 1. A publica QH (T2 planificada, RELEASED); 2. el supervisor acredita la terminación de A (`isRunning`) o el Owner la atesta; 3. el Coordinator calcula el oráculo desde el estado en QH y publica **solo su SHA-256** en la evidencia real; 4. B arranca en un clon limpio `--no-local` en QH con solo las entradas de D.6; 5. preflight de B para RESUME_DECISION; 6. el supervisor registra el SHA-256 de la respuesta de B; 7. comparación mecánica; 8. N11 con B2 sobre un clon con un hecho retirado |
| Transiciones esperadas | ninguna nueva (QH); la decisión esperada es QR → Q0 de la ventana k+1 → CONTROLLER_PLANNING de T2, binding REUSE/REBIND |
| Evidencia | `FX-04a/<RunId>/` (QH, oráculo con su hash previo, respuesta con su hash previo, auditoría de lecturas, comparación, N11) |
| PASS / UNVERIFIED / FAIL | PASS: igualdad total y aislamiento no UNVERIFIED; FAIL: una diferencia o una lectura prohibida; UNVERIFIED: falta OD-5, la terminación de A o el preflight de B |
| Prototipo medido | `fx04a/fx04a_proto.py`: fixture mínima, QH válido, reconstrucción desde un clon `--no-local`, comparación PASS, artefacto transitorio invisible y N11 → UNKNOWN. En F6 el reconstructor mecánico se sustituye por la sesión B |
| Limpieza | cerrar B y B2 |

### FX-04b — continuación (C-25b)

| Campo | Contenido |
|---|---|
| Decisiones del Owner | OD-5, OD-2, OD-4, OD-7 y `repo-write` de B para CUSTODY |
| Órdenes | preflight CUSTODY de B → designación (T16, o T22 si `main` del fixture avanzó) → QR → Q0 → planificación → Worker con escritura → verificación → Q7 |
| Resultado | PASS: VERIFIED sobre G' y Q7 conforme a B.8.4; UNVERIFIED/UNSUPPORTED con causa (no altera FX-04a); FAIL impide cerrar F6 |

### FX-05 — plano (c) sin efecto real (C-27)

| Campo | Contenido |
|---|---|
| Decisiones del Owner | OD-5 |
| Órdenes | el Coordinator del fixture declara un gate dentro del fixture; después se intenta un artefacto que nombre una unidad real o el remoto de RackCad |
| Detección | comparación antes y después de las refs remotas de RackCad, `state/I-62.yml` y decisiones reales; búsqueda de identificadores reales en el fixture |
| Resultado | PASS: rechazo (P-16) y estado real intacto |

### FX-06 — autonomía real (C-39)

| Paso (D.8) | Quién actúa | ¿RELAY o autoridad? | ¿Puede pasar por el Owner? |
|---|---|---|---|
| 1. publicar X con un defecto sembrado | Principal A | relay (trabajo del Principal) | no |
| 2. materializar el binding de B bajo la RLA, cierre, fidelidad, reserva y lanzamiento automático | Principal A | relay **dentro** de la autorización previa del Coordinator (§20.5.1) | **no**: un clic humano aquí es un relevo manual |
| 3. resultado CHANGES REQUIRED custodiado | Architect B → A | relay | no |
| 4. ingestión automática | A | relay | no |
| 5. X2, PUBLISHED, CI_VERIFIED | A | relay | no |
| 6. materializar C sin decisión intermedia, abrir la solicitud siguiente, invocar | A | relay dentro de la RLA | no |
| 7. AGREED con disposiciones CLOSED | Architect C → A | relay | no |
| 8. derivar la decisión pendiente del Owner o el siguiente gate | estado canónico | autoridad: la decisión, si la hay, es del Owner; el siguiente gate, del Coordinator | sí, solo para decidir |

**Autoridad genuina antes de la corrida:** la `ReviewLoopAuthorization` con su autorización de materialización (Coordinator del fixture) y OD-5/OD-7 y OD-2 u
OD-3 (Owner). Ninguna otra intervención humana es legítima en los pasos 1-7.

**Clasificación del AUTONOMY_GAP actual** (lanzar una sesión limpia de Architect exige un clic del Owner cuando no hay transporte limpio autorizado, MEASURED en
la revisión de A-1):
- **hoy, en I-62 bajo I-61:** es una limitación esperada del protocolo vigente. I-62 no es efectivo, FX-06 no se está ejecutando y la revisión de A-1 es una
  revisión de LIFECYCLE, no un paso del piloto. Se registra como AUTONOMY_GAP (OWNER_CLICK_REQUIRED), sin consecuencia de gate;
- **si ocurriera en FX-06:** el clic es un relevo manual → AUTONOMY_GAP y el resultado **no** es PASS (C-37, D.8 «no se finge el bucle»). No es FAIL: FAIL es
  una violación del protocolo, y un relevo declarado no lo es. Es **UNVERIFIED** si la causa es una OD pendiente (OD-2 u OD-3: el transporte existe pero no
  está autorizado), y **UNSUPPORTED** si, con las OD concedidas, se mide que ningún runtime disponible puede actuar como Architect independiente invocado
  por A (D.8).

**Lo que I-62 debe aportar para cerrar el hueco** (de Track H, `../transport-options.md`): un adapter de Architect con las operaciones 2-7 en DISPONIBLE
(observar, renderizar, invocar, observar el resultado, cancelar y confirmar la terminación) que A pueda lanzar sin intervención humana: `codex-cli` con OD-2,
`claude-cli` con OD-3, o un adapter nuevo de sesión de escritorio con una operación «crear sesión» programática.

| Campo | Contenido |
|---|---|
| Decisiones del Owner | OD-5, OD-7 y OD-2 u OD-3; un binding de Architect con invocación medida |
| Topes | Architect 2 + reejecuciones 2 = 4 (≤ 9); Principal 1 + 1 |
| Resultado | PASS solo con `OWNER_AS_MESSAGE_BUS` = false en 1-7; UNVERIFIED/UNSUPPORTED con causa (OV-I62-06); FAIL impide cerrar F6 |
