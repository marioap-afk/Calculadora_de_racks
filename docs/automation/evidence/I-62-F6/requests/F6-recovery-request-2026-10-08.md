# I-62 F6 — Solicitud única de recuperación: B3 extraordinaria de FX-04a y bloque de sondas de Codex (A-2; decisiones §57)

A-2 está acordada (decisiones §57; blob `f1e1d6f0…`). A-2 **habilita solicitar**, pero no concede por sí misma, ninguna de las dos aplicaciones.
Cada una necesita **dos actos**: una disposición del Coordinator que la nombre y una autorización de consumo del Owner. Esta solicitud reúne
las dos para que el Owner decida en una sola respuesta. La aceptación por OD-2 de la huella que resulte del bloque de Codex es posterior a la
medición y queda fuera de esta respuesta, porque no se acepta nada por adelantado.

Ninguna parte pide al Owner abrir una sesión para un rol que `claude-cli` ya puede desempeñar. B3 es una sesión de **Principal B**
(RESUME_DECISION), un rol que la autorización CLAUDE-CLI-I62 no cubre (§56, punto 6; §57, punto 3): por eso la abre el Owner.

## 1. B3 extraordinaria de FX-04a (A2-P1)

| Campo | Valor |
|---|---|
| Escenario y fila | FX-04a, fila «Principal B» de V14 D.3 (FX-04b continúa esa sesión y consume con ella su reejecución; la fila «N11: B2» no participa) |
| Acreditación que la habilita | B2 = INVALID_LAUNCH (decisiones §55; KICKOFF_SCHEMA_NOT_DELIVERED). B1 = INVALID_TEST_ORACLE (decisiones §53), que no habilita por sí sola la reejecución (A-2, regla 2) |
| Reejecución | la única extraordinaria de FX-04a en toda F6 (A-2, regla 4); otro fallo del arnés no da otra (regla 6) |
| Tope y total | fila Principal B: 1 sesión + 1 reapertura = 2 (consumidas: B1 y B2) → **3** con la reejecución; total de la ronda FX-04a: ≤ 2 (+1 B2) → **≤ 3 (+1 B2)**; P-07 la comprueba antes del lanzamiento |
| Presupuesto restante (A-2, reglas 4 y 5) | suficiente: el escenario solo necesita una sesión de Principal B con **0** invocaciones de modelo (D.3, fila «FX-04a / invocaciones de modelo = 0»); no se repone ningún otro presupuesto |
| Consumo requerido | una sesión `claude-desktop-session` (`claude-opus-5-5`, `xhigh`), abierta por el Owner, de unos 15-30 minutos (B1: 7 min; B2: 12 min); cuota de la suscripción del Owner. OD-5 (consumo dentro de los topes de D.3) no lo cubre |
| Fuente QH2 preservada (decisiones §57, punto 2) | clon `D:\r62-fixture\B3` (rama `fx/u1` en `cabed547`, árbol limpio, sin sesión previa en esa carpeta) cuyo **único remoto `origin` es la instantánea congelada** `D:\r62-fixture\fx04a-qh2-origin.git` (refs `275d977b…`, idéntica al origen vivo al crearla; `git fsck` limpio; sin remoto propio; un gancho `pre-receive` rechaza toda escritura y se comprobó con un push de prueba rechazado). `git ls-remote origin` de B3 devuelve los hechos de QH2 aunque cambie el `fx/u1` vivo |
| Oráculo y contrato | oráculo v2 `5e7a78d3…` (calculado desde `cabed547`, durable en `707b4daa`) y comparison-contract-v2, sin cambio |
| Texto inicial | `D:\r62-fixture\evidence-out\fx04a-b3\B3-kickoff.md` = mismos bytes que el de B2 (8 065 bytes, SHA-256 `233b058261d2bdb1e7c2f6f16290a32ea6b100df0e430987f8ea7131c4e7d59e`), con el esquema JSON completo. Se entrega **desde el archivo** (Bloc de notas → Ctrl+A → Ctrl+C), nunca desde una copia del chat. La supervisión comprueba el SHA-256 del primer mensaje en cuanto se abre la sesión y, si no coincide, avisa en el acto |
| Aislamiento (D.6) | auditoría previa al lanzamiento como para B2 (entradas automáticas con su SHA-256, sin `CLAUDE.md` global, sin memoria de proyecto para `D:\r62-fixture\B3`) y auditoría de herramientas posterior |
| Después | la supervisión acredita la terminación, publica de forma durable el SHA-256 de la respuesta, solo entonces carga el oráculo v2, compara con el contrato v2 y propone la clasificación al Coordinator |

**Disposición que se pide al Coordinator (A-2, regla 4):**

```text
B3 = AUTORIZADA (A2-P1): reejecución extraordinaria única de FX-04a, fila Principal B, por la acreditación INVALID_LAUNCH de B2 (§55); clon D:\r62-fixture\B3 con origin = la instantánea congelada de QH2 (refs 275d977b…); texto inicial B3-kickoff.md (233b0582…); oráculo v2 5e7a78d3…; presupuesto restante confirmado (0 invocaciones de modelo)
```

**Autorización que se pide al Owner:**

```text
B3-CONSUMO = A (una sesión de Principal B, claude-desktop-session, claude-opus-5-5 xhigh, abierta por el Owner en D:\r62-fixture\B3, por encima del tope de D.3 que cubre OD-5; texto inicial pegado desde B3-kickoff.md; ninguna otra sesión)
```

## 2. Bloque nuevo de sondas de solo lectura de Codex (A2-P2)

| Campo | Valor |
|---|---|
| Disparador | el runtime medido (binario `3b8f6e33…`, OD-2d-PROBE, decisiones §51) quedó invalidado por actualizaciones automáticas de la app de Codex, con las sondas congeladas agotadas (decisiones §55, punto 11). Actualizaciones observadas: 2026-10-06T18:56Z, 2026-10-07T02:14Z y 2026-10-07T11:57Z |
| **Par exacto** (identidad del bloque) | `BinaryHash` = `3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68` (`%LOCALAPPDATA%\OpenAI\Codex\bin\9691020b546a15b2\codex.exe`, único `codex.exe`); `AppVersion` = `26.1002.7124.0` |
| Huella resultante | `config.toml` = `6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32` (4 777 bytes), escrita el 2026-10-07T20:05:22Z y estable desde entonces (A-2, regla 3: el par cuenta como observado con su huella resultante estable). Claves cambiadas frente a `9EA26634…` (comparación por clave, sin valores): `mcp_servers.node_repl.command`, `mcp_servers.node_repl.env.{BROWSER_USE_CODEX_APP_VERSION, CODEX_CLI_PATH, NODE_REPL_NODE_MODULE_DIRS, NODE_REPL_NODE_PATH, NODE_REPL_TRUSTED_CODE_PATHS, NODE_REPL_TRUSTED_SERVICES, SKY_CUA_NATIVE_PIPE_DIRECTORY}` y `notify` (las mismas 9 que en la actualización anterior) |
| **Tope** | como máximo **2** sondas de solo lectura en este bloque; un solo bloque para esta actualización; se cuentan aparte de los lanzamientos de Codex de la ronda y del «+ 2 sondas» de los totales; P-07 comprueba el tope del bloque antes de cada sonda; las sondas no usadas caducan si hay otra actualización |
| Sondas propuestas | sin modelo antes: `codex --version` y `codex login status`. **Sonda 1:** celda del Controller (`gpt-6-luna`, `high`, `read-only`). **Sonda 2:** celda del Architect (`gpt-6.1-sol`, `high`, `read-only`) en `D:\r62-fixture\arch`. Directorio de la sonda 1: la decisión pendiente U-71 (FX-02 CD-28); la propuesta de la preparación es `D:\r62-fixture\A`, que ya existe (`A2` se crea al publicar la orden FX-U1-O4) |
| Controles | huella, binario, versión de la app y comparación por clave antes y después de cada operación; sin `workspace-write`; sin cambios de `config.toml`, `trust_level` ni sandbox; un cambio de huella durante el bloque lo invalida (A-2, regla 6) |
| Consumo requerido | 2 invocaciones de modelo de `codex-cli` de solo lectura (cuota del plan de ChatGPT del Owner), de minutos |
| Lo que sigue | si las sondas salen bien, la supervisión publica el paquete exacto medido y el Owner decide **después** OD-2 sobre la huella resultante (`OD-2 = A (línea base <huella>; binario 3553cd6e…)`). Hasta entonces sigue el STOP P-01 de `codex-cli` |
| Riesgo | la app se ha actualizado sola tres veces en unas 41 horas: si vuelve a hacerlo antes de usar el bloque, este caduca y hace falta otro con la misma autoridad. El Owner puede pausar las actualizaciones (decisión suya); la preparación de A-3 y de MATERIAL_ADAPTER_FINGERPRINT trata este problema |

**Disposición que se pide al Coordinator (A-2, regla 3):**

```text
BLOQUE-CODEX = AUTORIZADO (A2-P2): par (3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68, 26.1002.7124.0) con huella resultante estable 6518EFAB…; ≤ 2 sondas read-only: Controller gpt-6-luna/high en <directorio según U-71; propuesta D:\r62-fixture\A>, Architect gpt-6.1-sol/high en D:\r62-fixture\arch
```

**Autorización que se pide al Owner:**

```text
BLOQUE-CODEX-CONSUMO = A (≤ 2 invocaciones read-only de codex-cli para el par exacto 3553cd6e…/26.1002.7124.0, con huella, binario y versión antes y después de cada una; sin workspace-write ni cambios de config.toml, trust_level o sandbox)
```

## 3. Respuesta única del Owner

Las dos autorizaciones pueden darse juntas o por separado: `B3-CONSUMO = A` y/o `BLOQUE-CODEX-CONSUMO = A`. Cada una solo tiene efecto con la
disposición del Coordinator que la nombra. No se pide ninguna otra apertura de sesión: las revisiones de Architect y Reviewer siguen por
`claude-cli` (decisiones §57, punto 3).

## 4. Añadido (append-only): comparación por clave suspendida; controles del bloque de Codex sin lectura de valores

Esta sección se añade sin cambiar las secciones 1 a 3. Se publica antes de cualquier disposición o autorización del bloque de Codex para que
ninguna operación del bloque se ejecute con controles distintos de los publicados.

- **Por qué.** La comparación por clave de `config.toml` lee valores en el host, aunque no los registra ni los publica. Puede hacerse con digests
  HMAC por clave, con la clave solo en el scratchpad local, o revirtiendo un valor y volviendo a calcular el hash.
  - Los términos con que el Owner aprobó OD-2 dicen «nunca valores» (`owner-decision-packets.md`, OD-2, fila «Seguridad»).
  - El texto materializado lo repite: «Nunca se leen ni se registran valores de configuración» (`agent-execution/README.md`).
  - El historial de esas lecturas se publica con la candidata A-3, en el paquete de decisión OD-2-MAT, sección «Lectura de valores». No afirma ni
    niega si las aprobaciones anteriores las cubrían.
- **Suspensión.** La sesión no hace comparaciones por clave desde la decisión S-01 de la preparación de A-3, posterior al commit `b0884216`. Ese
  commit custodia la última localización por clave (evidencia §87; §2 de esta solicitud, fila «Huella resultante»). Solo una autorización expresa
  del Owner podría reanudarlas, y tendría que nombrar el programa y su blob, las superficies que lee y su caducidad. No se pide ahora.
- **Controles del bloque de Codex (§2).** La fila «Controles» queda así: SHA-256 de todo `config.toml`, nombres de clave saneados y estructura,
  identidad del binario y versión de la app, antes y después de cada operación, sin comparación por clave. Un cambio de huella se sigue
  detectando; ya no se localiza qué claves cambiaron.
  - `BLOQUE-CODEX-CONSUMO = A` no cubre leer valores.
  - La disposición `BLOQUE-CODEX` del Coordinator que nombre el bloque se entiende con estos controles.
  - El resto del bloque no cambia: par, tope, celdas, sin `workspace-write` y sin cambios de `config.toml`, `trust_level` ni sandbox.
- **Lo que sigue.** La decisión del Owner OD-2-MAT (línea base material frente a huella exacta, materia reservada al Owner por decisiones §57,
  punto 4) se añadirá a esta misma solicitud en una sección posterior, después del commit que publique la candidata A-3. Así la respuesta del Owner
  sigue siendo una sola.

## 5. Añadido (append-only, tras el commit de A-3): decisión del Owner OD-2-MAT y respuesta única

Esta sección se añade después del commit que publica la candidata A-3 (`91e29886`), como anunciaba el §4. Las secciones 1 a 4 no cambian.

**OD-2-MAT** ([paquete](../../I-62-A3/od2-material-baseline-owner-packet.md); decisiones §57, punto 4).
- **Qué se decide.** Si OD-2 deja de ser el SHA-256 exacto de todo `config.toml` y pasa a una línea base material. Ese cambio afecta a autoridad
  reservada al Owner y no puede resolverse por disposición del Coordinator.
- **Mientras no se decida.** OD-2 sigue siendo la huella exacta, P-01 no cambia y `codex-cli` sigue en STOP con `6518EFAB…` sin aceptar.
- **Qué no hace ninguna opción.** Ninguna acepta una huella ni autoriza leer valores.
- **Recomendación de la preparación (etiquetada; no es una decisión).** A ahora. D queda para estudiarla después de F6, si el Coordinator lo
  ordena. Los motivos y los riesgos de cada opción están en el paquete.
- **Líneas literales.** Son las cuatro del paquete, copiadas de ese archivo. El Owner responde con una sola:

```text
OD-2-MAT = A (mantener OD-2 como aceptación del SHA-256 exacto de todo config.toml; cada cambio sigue siendo P-01 y una OD-2 nueva)
```

```text
OD-2-MAT = B (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): línea base material de codex-cli por clase cerrada de claves volátiles, neutralizadas en la receta y comparadas por nombre y con digests con clave por clave, que leen valores en el host sin registrarlos; efectiva solo tras medir la neutralización, esa A-n AGREED y una OD-2 exacta de anclaje; P-01 de I-61 y P-01/P-11 en la cesión sin cambio; esa A-n necesita su propia autorización del Owner para leer valores, que esta línea no da)
```

```text
OD-2-MAT = C (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): como B, pero con predicados de valor sobre las claves volátiles evaluados mecánicamente sin registrar valores, en lugar de neutralizarlas en la receta; esa A-n necesita su propia autorización del Owner para leer valores, que esta línea no da)
```

```text
OD-2-MAT = D (preparar la A-n siguiente (número según LIFECYCLE §6 al publicarse): CODEX_HOME dedicado para codex-cli con OD-2 exacta sobre su config.toml; antes, una medición read-only autorizada aparte; el Owner autentica la CLI en ese directorio; sin línea base material)
```

**Respuesta única del Owner (todas las decisiones pendientes de esta solicitud).** Cada línea es independiente. El silencio no es decisión.

| Decisión | Línea válida | Efecto |
|---|---|---|
| B3 de FX-04a | `B3-CONSUMO = A (…)`, texto literal del §1 | una sesión de Principal B abierta por el Owner. Tiene efecto con la disposición `B3 = AUTORIZADA` del Coordinator |
| Bloque de Codex | `BLOQUE-CODEX-CONSUMO = A (…)`, texto literal del §2 | como máximo 2 sondas de solo lectura para el par exacto, con los controles del §4. Tiene efecto con la disposición `BLOQUE-CODEX = AUTORIZADO` del Coordinator. La OD-2 sobre la huella resultante se pide después de medir |
| OD-2-MAT | una de las cuatro líneas de arriba | A: sin cambio. B, C o D: encargo de la A-n siguiente, sin efecto operativo hasta que esa A-n esté AGREED y materializada |

No se pide abrir ninguna sesión para roles que `claude-cli` ya cubre. Si el Coordinator ordena la revisión formal de A-3 por el Architect, se hace
por `claude-cli` (decisiones §57, punto 3).
