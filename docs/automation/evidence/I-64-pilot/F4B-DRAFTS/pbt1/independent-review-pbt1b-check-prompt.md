> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: la comprobación acotada de PB-T1b (paquete v2.5, anclajes y generador del contrato). Es la última barrera antes de emitir PB-T1b

Eres un revisor independiente y adversarial de I-64 F4B (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No redactaste nada de lo que revisas.

**Reglas:**
- solo lectura;
- sin compilar ni ejecutar pruebas;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc;
- `gh`, solo de lectura.

**Dónde está la rama:** el worktree `<worktree-de-I-64>`, en `architecture/workspace-persistente-rackcad`, con HEAD `5043b49a` (el cierre de PB-T1a, F4B-E16). Usa siempre `git -C <worktree>`, sin checkout. No hay ningún Worker en el worktree.

**Qué está decidido:**
- **El paquete de PB-T1 v2.4,** custodiado en `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`, divide PB-T1 en tres subtareas. PB-T1a está cerrada.
- **D2 del Owner** (F4B-E11), con sus 10 condiciones y su límite: `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`.
- **F4B-E14** (`…/coordinator/owner-pbt1a-decisions.md`):
  - pregunta 2: c-09 al reconstruir, «Como hoy, regla explícita»;
  - pregunta 3: «lb», `PalletsDeepBox` y el portador de botas, «Tal cual».
- **F4B-E15:** la verificación del Controller lleva tres comprobaciones deterministas obligatorias (`…/coordinator/owner-pbt1a-nc1-decisions.md`).
- **La autorización de emitir** (F4B-E09, pregunta 4, y F4B-E11): PB-T1b se emite sin volver al Owner si esta comprobación da 0 bloqueantes, 0 importantes sin resolver y **ninguna decisión material nueva**. Una decisión que el Owner ya tomó (c-09) no es nueva; aplicarla de un modo que vaya más allá de lo que decidió, sí.

## Entradas, en `<scratchpad>\hs65\pbt1\`

- `I-64-f4b-pbt1-gate-contract-package-v25.md`, con el registro de cambios v2.4 → v2.5 (V25-01 a V25-17);
- `pbt1-measurements.md`, apartado P (los anclajes de PB-T1b sobre `5043b49a`), con `pbt1b-anchors-out.txt` y `measure_pbt1b_anchors.py`;
- `make_pbt1b_contract.py`. Puedes ejecutarlo solo en seco: `python make_pbt1b_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`;
- `check_v25_skeleton.py`;
- `pbt1b-instructions.md`, lo que pidió la sesión;
- para comparar, la v2.4 custodiada (en HEAD) y las comprobaciones acotadas anteriores, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt1/`.

## Qué haces

1. **Los anclajes (§6.0, precondición 4):**
   - ¿se sostienen las anclas de D-PBT1-02 a D-PBT1-11 y del esqueleto del §5.3 sobre `5043b49a`?
   - ¿G-L1 da 0 de verdad, y sus autopruebas saltan?
   - ¿los recuentos y la línea base de la CI son correctos?
   - ¿alguna diferencia declarada «no material» lo es en realidad?
2. **c-09 (V25-06), con la decisión del Owner (F4B-E14, pregunta 2):**
   - ¿D-PBT1-09b (`ResolveWeightUnit` en `AdoptCarriers`) reproduce exactamente el comportamiento de hoy, como regla explícita, igual que D-PBT1-09 con c-02?
   - ¿RACKEDITAR sigue sin cambiar?
   - ¿CT-PBT1B-17 lo fija con líneas literales trazables a CT-PBT1A-02?
   - ¿la fila (f) del paso 1, con dos ramas y PBT1B-C14, es legítima (como N-M-03 con c-02) o esconde una decisión nueva?
   - ¿HT1-15 y el §5.5 quedan bien corregidos? ¿Afecta a PB-T1c más de lo que dice V25-10?
3. **OWNER-3 y OWNER-4** (F4B-E14, pregunta 3): ¿el diseño los reproduce tal cual, sin regla propia, como dice V25-09?
4. **El generador de PB-T1b:** ¿el JSON que produciría coincide con el §6.2 de la v2.5? Compara:
   - las autoridades;
   - el `AllowedWriteScope` (12) y el `ForbiddenWriteScope` (213: ¿el método de [PKC-C] da eso?);
   - los invariantes (15);
   - los `RequiredTests` (23);
   - las paradas (33), con las de [DP] literales, C04, C13, C14, C15 y C17;
   - `ExpectedEvidence`;
   - las celdas e `IssuedBy`;
   - el `assert` del esqueleto y lo que custodia.
5. **D2:** ¿las condiciones que tocan a PB-T1b (CT-PBT1B-06, 07, 08, 14, 15 y 16, G-L1 con las reglas de la v2.4, INV-PBT1B-07c y C17) siguen completas y comprobables tras la v2.5?
6. **PB-T1b por sí misma:** ¿se puede emitir y cerrar? Mira el RED con esqueleto inerte, el criterio de INV-PBT1B-02, las dos guardas de D2 que pasan en el RED y la previsión de la CI.
7. **Decisiones materiales nuevas para el Owner:** sé estricto, con la regla de arriba.

## El informe

En español, en `pbt1/independent-review-pbt1b-check.md`, sin rutas absolutas locales, con:
- el veredicto arriba;
- §1, método y límites;
- §2, los anclajes;
- §3, c-09;
- §4, OWNER-3 y OWNER-4;
- §5, el generador;
- §6, D2;
- §7, PB-T1b;
- §8, los hallazgos (Id, gravedad, contenido, evidencia y corrección);
- §9, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M y las decisiones para el Owner.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, las decisiones nuevas y la ruta.
