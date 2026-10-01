# Catálogo de modelos — NO NORMATIVO

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Catálogo **mutable y NO NORMATIVO** respecto de los nombres de modelo. [routing.md](routing.md) decide por clase, effort semántico y nivel; este archivo solo traduce eso a modelos y
registra el estado local de cada celda. Cualquier sesión responsable puede actualizar una entrada con su fuente (oficial o medición), su tipo y su fecha, sin Freeze (Proposal V9 §15);
el cambio no altera la elegibilidad de una delegación ya aceptada.

**Tipos de fuente:** `oficial` (documentación del proveedor, con URL y fecha); `caché local` (archivos locales de una herramienta; nunca hace elegible una celda); `medido` (invocación
real registrada en la evidencia de una unidad). El **nivel** (Eficiente, Equilibrado, Frontera) es una asignación de RackCad hecha sobre la descripción oficial. Fuentes de esta versión:
[Discovery de I-61](../../initiatives/I-61-discovery.md) §§12-13 y §17, y la [evidencia de I-61](../evidence/I-61-evidence.md) §§10-13.

**Frescura:** `STALE` = mín(fecha de verificación + 90 días, retiro anunciado). Un retiro con precisión de mes cuenta desde el primer día de ese mes.

## Anthropic (subagentes de la sesión de escritorio)

Autenticación de la sesión: OAuth de una cuenta de claude.ai, sin clave de API ni proveedor de nube (`medido`, evidencia §12). Tipo de plan: UNKNOWN. El CLI `claude` no está
autenticado (`medido`) y no se usa como transporte.

### claude-fable-5-1 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `high`, retiro); asignación de RackCad (nivel)
- Nivel: Frontera
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read` — no medido; subagente × `write-commit-push` — no medido

### claude-opus-5-5 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `medium`, retiro); asignación de RackCad (nivel); medido (estado local)
- Nivel: Frontera
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (medidos: `high` y `xhigh`)
- Consumo cubierto: sí (medido: las invocaciones de subagentes de revisión de I-61 completaron con la autenticación por suscripción, sin aviso de límite; evidencia §13)
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read`/`tool-use` — medido (2026-09-30, modelo heredado de la sesión; effort solicitado `high` aplicado, sin solicitar se hereda `xhigh`); subagente ×
  `write-commit-push` — no medido; petición explícita de este modelo — no medida

### claude-sonnet-5-5 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `high`, retiro); asignación de RackCad (nivel)
- Nivel: Equilibrado
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read` — no medido; subagente × `write-commit-push` — no medido

### claude-haiku-4-5-20251001 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, sin control de effort, retiro); asignación de RackCad (nivel)
- Nivel: Eficiente
- Effort: no tiene control de effort; en una celda de subagente se registra el effort heredado medido
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: octubre de 2026 (`STALE` desde el 2026-10-01)
- Estado local: subagente × `read` — no medido; subagente × `write-commit-push` — no medido

## OpenAI (Codex CLI, solo lectura)

Autenticación: «Logged in using ChatGPT» (`medido`, Discovery §12). Los valores de effort de configuración proceden de la referencia oficial de configuración (evidencia §12), que
advierte que dependen del modelo y del cliente; la página de modelos usa los rótulos Light…Ultra sin correspondencia explícita. Cada valor se mide antes de usarlo.

### gpt-6.1-sol (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models` y `https://learn.chatgpt.com/docs/config-file/config-reference`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort de Light a Ultra; «Max and Ultra depend on your settings»); asignación de RackCad (nivel)
- Nivel: Equilibrado
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: ninguno publicado
- Estado local: CLI × `read`/`tool-use` — no medido

### gpt-6-luna (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models` y `https://learn.chatgpt.com/docs/config-file/config-reference`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort hasta Max, sin Ultra); asignación de RackCad (nivel); medido (estado local)
- Nivel: Eficiente
- Effort: Routine → `low`; Balanced → `high` (Eficiente con más effort); Deep → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: sí (medido: seis invocaciones de G1-C con la autenticación de ChatGPT, sin aviso de límite; evidencia §10)
- Retiro anunciado: ninguno publicado
- Estado local: CLI × `read`/`tool-use` — medido con el modelo solicitado (P1-a4 y P3, 2026-09-30); modelo y effort efectivos no confirmados porque se usó `--ephemeral`; se
  confirman en la sonda PR-1

### gpt-6-astra (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción «most capable», filas de disponibilidad «ChatGPT Credits» y «API Access»); asignación de RackCad (nivel)
- Nivel: Frontera
- Effort: sin correspondencia medida
- Consumo cubierto: UNKNOWN; marcado por la fuente oficial como de créditos o API, así que **no es elegible ni se sondea**
- Retiro anunciado: ninguno publicado
- Estado local: no medido

### gpt-5.5 (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (retiro de ChatGPT y Codex)
- Nivel: sin asignar
- Effort: sin correspondencia medida
- Consumo cubierto: UNKNOWN
- Retiro anunciado: 2026-10-14
- Estado local: no medido

**Solo en caché local (no elegibles, no se sondean como oficiales):** otros identificadores de la caché de modelos de Codex que la página oficial no recomienda (Discovery §13), entre
ellos el modelo configurado por defecto en este equipo. Se reverifican con la fuente oficial antes de añadirlos como entrada.
