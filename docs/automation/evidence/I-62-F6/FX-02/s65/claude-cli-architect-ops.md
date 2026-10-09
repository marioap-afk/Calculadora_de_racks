# FX-02 — Architect por `claude-cli` (A4-2): operaciones que necesita y operaciones medidas (decisiones §65, punto 9)

> Supervisión, plano a. Determinación pedida por D-3: «determinar exactamente qué operaciones necesita el Architect de FX-02 y cuáles están medidas.
> No tratar UNKNOWN como elegible. Solicitar únicamente la medición adicional imprescindible y su autoridad de consumo. No editar descriptor o
> catálogo basándose en supuestos». No se edita nada aquí.

## 1. Qué necesita el Architect de FX-02

- **Adapter (AUTOMATION_PLAN 16.19):** nueve operaciones; «Una operación UNVERIFIED que el rol necesita hace que la celda no sea elegible». El
  descriptor `adapters/claude-cli.md` no declara ninguna NO APLICA, así que el Architect necesita **2-9**: observar (preflight), renderizar (texto e
  insumos), invocar, observar el resultado (contrato de salida `rackcad-architect-review-result/v1`), cancelar (tope de tiempo), confirmar la
  terminación (T17; ningún proceso del lanzamiento vivo), clasificar procesos (árbol del lanzamiento) y declarar la huella (P-11).
- **Contrato de T1:** ARCHITECT / REVIEW_DESIGN, `Mandatory` ARCHITECTURE_REVIEW.effort, .level y .read; independencia frente a AUTHOR con Actor,
  Session y Context REQUIRED y Provider PREFERRED.
- **Elegibilidad completa (A4-2, regla 1):** invocación medida, requisitos obligatorios en MATCH o ABOVE_REQUIRED, consumo cubierto distinto de
  UNKNOWN y celda no `STALE`.

## 2. Qué está medido (evidencia de las revisiones de A-2, A-3 y A-4 y caracterización `541956eb`)

| Operación o requisito | Estado | Evidencia |
|---|---|---|
| 2 observar | MEDIDA | `auth status --json` (loggedIn) sin leer credenciales; versión, SHA-256 y Authenticode del binario 2.1.293 (`8693c4a0…`); modelo y effort RUNTIME_OBSERVED del `init` y de la transcripción |
| 3 renderizar | MEDIDA | fidelidad del texto (C1); `PromptSha256` por corrida |
| 4 invocar | MEDIDA | C1, C3 y seis revisiones con salida 0 |
| 5 observar el resultado | **PARCIAL** | `structured_output` medido solo con los esquemas de los kits de revisión, no con el canónico `rackcad-architect-review-result/v1` (faltan, entre otros, `ResultId`, `ReviewerBinding`, `InputFidelityEvidenceRef` e `IndependenceEvidence`) |
| 6 cancelar | MEDIDA | C2: proceso terminado por el invocador, sin mensaje `result`, transcripción conservada |
| 7 confirmar la terminación | **PARCIAL** | solo para el proceso raíz (fin del proceso y mensaje `result`); nada sobre descendientes |
| 8 clasificar procesos | **SIN MEDIR** | solo se registró el PID del proceso lanzado |
| 9 declarar la huella | **SIN MEDIR** | ninguna declaración; con `--safe-mode` no entran `CLAUDE.md` ni la memoria, pero no se ha medido qué archivo de configuración rige la corrida |
| introspección (Actor y Sesión) | MEDIDA | `session_id` de `init` y `result` igual al asignado (RUNTIME_OBSERVED) |
| ARCHITECTURE_REVIEW.effort / .level / .read | MEDIDA | `claude-opus-5-5` (Frontera) con effort `xhigh` observado (≥ Deep → `high`: ABOVE_REQUIRED); herramientas Read, Grep y Glob |
| consumo cubierto | MEDIDO, no catalogado | todas las corridas completaron con la autenticación por suscripción, sin aviso de límite; el catálogo no tiene entrada de la celda `claude-cli` y dice que la CLI «no está autenticada … y no se usa como transporte» |

**Conclusión:** la celda no es elegible hoy. Faltan las operaciones 5 (con el esquema canónico), 7 (árbol completo), 8 y 9, y la entrada de la celda en
el catálogo. Hasta entonces, por §64.3, el binding del Architect por `claude-cli` ni se invoca ni se acepta.

## 3. Medición imprescindible (propuesta)

Una sola invocación de solo lectura de `claude-cli` 2.1.293 (binario `8693c4a0…`), celda `claude-opus-5-5` effort `xhigh`, receta medida (`--safe-mode`,
solo Read, Grep y Glob, `--session-id` nuevo, texto por stdin), en el plano a, en un clon temporal de lectura del fixture (nunca `A2`, `arch` ni
`arch02`), sin reintento, que mida a la vez:
- **5:** `--json-schema` = el esquema canónico `rackcad-architect-review-result/v1`, con validación posterior de la salida;
- **7 y 8:** el árbol de procesos del lanzamiento (padre e hijos, muestreado durante la corrida con `Win32_Process`), su clasificación y la
  confirmación de que no queda ningún proceso del árbol al terminar;
- **9:** la huella declarable: SHA-256 y nombres saneados de las claves (nunca valores) del archivo de configuración de usuario que rige la corrida
  (`%USERPROFILE%\.claude\settings.json`) antes y después, con lo que el `init` muestra de la configuración efectiva; o, si no rige ninguno, la
  demostración para «ninguna», que acepta el Coordinator.

**Autoridad de consumo.** `A4-CLAUDE-FX02-CONSUMO` cubre el lanzamiento del Architect en FX-02, no una medición previa, y `CLAUDE-CLI-I62` es para las
revisiones de I-62. Línea propuesta para el Owner:

```text
CLAUDE-CLI-MEDICION-FX02 = A (1 invocación read-only de claude-cli 2.1.293, binario 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa, celda claude-opus-5-5 xhigh, solo Read, Grep y Glob, en un clon temporal de lectura del fixture, para medir las operaciones 5 (esquema canónico de la revisión del Architect), 7, 8 y 9 de su descriptor; sin reintento; sin escritura; sin lectura de credenciales ni de valores de configuración; no acepta su resultado ni amplía OD-3, CLAUDE-CLI-I62 ni A4-CLAUDE-FX02-CONSUMO)
```

## 4. Secuencia (sin retrasar el camino a VERIFIED)

- El catálogo que usa el Controller es la copia del fixture, y su blob invalida la medición de A4-1 (A4-1, regla 5). Por eso la entrada de la celda
  `claude-cli` en el catálogo del fixture, y la actualización del descriptor con lo medido, van **después de la última invocación del Controller de
  FX-02** (Q7 final, incluida una corrección de A4-5 si la hubiera) y antes del bloque de autorización de la revisión (punto 17 de O4). Encaja con
  D-0: la revisión del Architect es posterior al PASS del piloto.
- La medición de §3 no toca el fixture y puede hacerse en cualquier momento antes de ese bloque.
- **Riesgo:** la app de escritorio ya instaló `claude-code` 2.1.295 junto a 2.1.293. Si borra 2.1.293 antes de la revisión, toda la medición de
  `claude-cli` queda `STALE` y habría que medir el binario nuevo.
