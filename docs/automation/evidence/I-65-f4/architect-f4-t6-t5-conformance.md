# I-65 F4 (T6 y T5) — revisión de conformidad del Architect (custodia)

- Rol: Architect de I-65, `SAME-SESSION ROLE` (subagente `claude-opus-5-5`/`high`, contexto independiente del Coordinator), solo lectura.
- Fecha: 2026-10-14. Alcance: `git diff ef353615..d0f0ad5e -- src tools tests RackCad.sln` (T6 `ef353615..6422e5b0`, 20 archivos; T5
  `6422e5b0..e610ea49`, 9 archivos; `git diff f1d9a51d e610ea49 -- src` vacío), contra la Proposal V3.1 (Frozen), ADR-0050, la concreción
  `I-65-f4-implementation.md` §5, §6, §9-§11, las decisiones §36 y §40-§45, la revisión anterior (A65-F4-01..11), el paquete de validación de
  ingeniería §2 y §3 F, los contratos custodiados de T6 y T5, las verificaciones del Controller y las disposiciones del Coordinator.
  Lecturas solo con `git -C <worktree>` (`show`, `diff`, `grep`, `log`); sin escribir, compilar, ejecutar pruebas ni intérprete. Controles
  del Owner: K1/K2 sin cambios en el checkout principal ni en los demás worktrees; K5 sin accesos fuera del worktree (transcripción SHA-256
  `05C727B79989EFA2264F5A1771324360F996B6A928D7488841F22459190E62FC`).
- Texto: resumen fiel del informe del subagente.

## Veredicto

**CONFORMANT WITH OBSERVATIONS.** Ningún hallazgo REQUIRED; 10 observaciones OPTIONAL. Tres conviene decidirlas antes de pedir la primera
ejecución real: A65-F4R-06, 08 y 10.

## Respuestas (resumen fiel)

1. **Freeze, ADR-0050 y fronteras:** conforme. Producción solo en `StructuralReadbackComparison.cs`, `StructuralReadback.cs` (solo añadidos),
   `RamTransferReaders.cs`, `RamDumpReader.cs`, el comando nuevo del Plugin y un único `Content` del `.csproj`; Domain, UI, `Plugin/Workspace`,
   Systems, Units, la solución y los `.csproj` de tools y pruebas intactos. G1 con la única ruta permitida compartida por las dos guardas; G2, G4,
   INV-11 e INV-16 sin violaciones; INV-14 con exactamente cuatro rutas y sin literales partidos; tolerancias 0.02 in, 0.1° y 0.1 % y `OTHER`
   sin cambio (prueba dedicada); las constantes nuevas (1e-6 lbf de carga manual nula, CD-I65-F4-31; 1e-6 de ligadura) no son tolerancias de §4;
   el informe sigue diciendo que no valida el diseño; `Incomplete` nunca es PASS.
2. **T6 frente a A65-F4-01..06:** resueltas (euclídea con diagonal; carga relativa al manual y caso cero; `Incomplete` con decisión obligatoria;
   ligadura estricta a `values` tras `Passed` y `STALE`, entradas iguales rechazadas, `inputs` con huellas y cuatro SHA-256; inyección
   reconocida solo en `tools/` y siempre REJECTED; categoría vacía solo sin cargas con el comentario NOT MEASURED; ensayo general PASS con −90/270
   y FAIL con +90, porcentaje de 1 decimal y brazo invertido). **D1 aceptada como limitación**, no defecto: `read_back` no registra extremos; el
   propio script contrasta l1..l3; la conectividad del generado se evalúa contra el manual. Ningún PASS en falso nuevo; un rechazo en falso
   posible y en cerrado (volcado tras guardar y reabrir, A65-F4R-02); persiste un PASS en falso heredado de combinaciones (A65-F4R-06).
3. **T5 (`RACKRAMPAQUETE`):** conforme con D-13, D-08, D-12 e INV-12 y con §6/§11: flujo completo con lectura única del sobre, tabla junto a
   `Assembly.Location`, catálogo por `TryLoad`, entrada de F4-T2, publicación única en la carpeta embebida; las tres comprobaciones `!= Valid`
   informan y vuelven antes de publicar; excepciones como `Unavailable`; perfil solo de la tabla (regla C10); prohibiciones con reglas C11-C14 y
   C18; tabla solo en la salida de compilación; censos 37 → 38 solo con `RACKRAMPAQUETE` (coherente con CD-I65-F4-33 según la referencia local
   de `origin/main`); con la tabla del producto, `Unsupported` sin paquete.
4. **Pruebas:** observan de verdad, con RED honesto (T6 131/272 por aserción o excepción; T5 por corrección desactivada). Puntos débiles: tipos y
   hashes de T6 sin RED propio; la guarda de T5 es textual (posición en el texto, no flujo de control; C16 ignora condiciones con paréntesis); la
   observación del comando es estructural (inherente, CD-I65-F4-41); los vectores de +90 del ensayo son supuestos del autor.
5. **Primera comparación real:** sin defectos de código bloqueantes; precondiciones y riesgos: (a) filas `proposed` → sin paquete ni inyección
   hasta la verificación del ingeniero, un cambio autorizado de la tabla y recompilar (A65-F4R-10); (b) el comando nunca se ejecutó en AutoCAD
   (A65-F4R-08); (c) combinaciones sin M-05 medido (A65-F4R-06); (d) ligadura en la misma sesión (A65-F4R-02); (e) comportamientos no medidos de
   la API, que fallan en cerrado.
6. **A65-F4-07..11:** sin cambio sustancial; A65-F4-05 añade un caso ignorado más (sin cargas y categoría vacía).

## Hallazgos (todos OPTIONAL)

| ID | Ubicación | Hallazgo | Recomendación |
|---|---|---|---|
| A65-F4R-01 | `StructuralReadbackComparison.cs:503-597`; `RamTransferReaders.cs` `ReadValues` | D1: `values` trae l1/l2/l3, giro, punto cardinal, offsets, factores y liberaciones; se validan y se descartan | Ligarlos sin tocar la plantilla (l1 fija el sentido I→J) |
| A65-F4R-02 | `StructuralReadbackComparison.cs:419` | Una sola constante de 1e-6 para in y lbf; supone volcado en la misma sesión; guardar y reabrir podría dar REJECTED en falso | Dos constantes con unidad; «volcar sin guardar ni reabrir» en D6/D7, o medirlo |
| A65-F4R-03 | `RamToolCompare.cs:47-55` | `--combinations-decision` aceptada sin las opciones de combinaciones no leídas y presente en un informe PASS | Rechazarla sin esas opciones o etiquetarla no aplicable |
| A65-F4R-04 | `RamToolCompare.cs:64-127` | En inyección exige volcados legibles antes de reconocer la marca y no aplica el control de entradas iguales | Fijar en el procedimiento G qué se pasa, o permitir omitirlos |
| A65-F4R-05 | `RamToolCompare.cs:147-157` | Independencia del manual solo por ruta o bytes | Rechazar si (id, descripción) del manual coinciden con los `values` |
| A65-F4R-06 | `StructuralReadbackComparison.cs:1128-1129`; fila de combinaciones | Por defecto se asume que las combinaciones se leen (M-05 sin medir); casos sin cargas ignorados sin listarlos | En la primera corrida, declararlas no leídas con decisión salvo medición en E4; listar los casos ignorados |
| A65-F4R-07 | `RackRamElementsCommands.cs:116-118` | `Valid` con `Table == null` mostraría «Valid.» en un fallo | Informar `Invalid` |
| A65-F4R-08 | `RackRamElementsCommands.cs`; `.csproj` | El comando nunca se ejecutó en AutoCAD; la herramienta no crea un paquete con copia de la tabla (OV-I65-03) | Smoke en AutoCAD sin RAM; capturar ya `Unsupported` con la tabla del producto (OV-I65-03) |
| A65-F4R-09 | RED `d655071b` y `e413b9a2`; guarda de T5 | Tipos y hashes sin RED; guarda textual sin flujo de control | Limitación registrada; la cubre la validación manual |
| A65-F4R-10 | `assets/.../section-material-map.json`; §8.2 | Precondición bloqueante: con filas `proposed` no hay paquete ni inyección; verificar exige ingeniero, un cambio autorizado de la tabla (fuera de T5/T6; attempts 6/6) y recompilar | Planificar esa autorización antes de pedir la corrida en RAM |

## No comprobado sin ejecutar

El estado real de pruebas y CI (se basa en lo registrado); la copia del `Content` a la salida; `Assembly.Location` y el comportamiento del
comando dentro de AutoCAD; si `origin/main` local está al día (sin fetch); la precisión de RAM tras guardar y reabrir; formato de
`str(category)` y `str(direction.value)`, tipo de los ids, −90 frente a 270, lectura de combinaciones (M-05) y categoría de `CM`; determinismo
entre culturas; equivalencia de `UTF8.GetBytes(embed.Design)` con los bytes persistidos del sobre.
