# I-58 — F1 / CHARACTERIZATION + RED

F1_START_SHA = fd13dd8d57da5c27889dec93dd288ae5259fc318
Freeze blob = f89671cfe9f1036e24211287514414b3f555182a
Characterization V3 = 97e15d6d7c00e02495f8d4159477272d00af6880
Discovery V3 = f07c9b6c6f6c278c6f7d37b3b68921eac7a1c5b8
Production diff = NONE. SDK resuelto = 8.0.423.

## Preflight

Worktree existente: C:/Users/alejandra-mendoza/.codex/worktrees/architecture-shared-view-authored-comparators.
Rama/upstream: architecture/shared-view-authored-comparators / origin/architecture/shared-view-authored-comparators.
Fetch --all --prune ejecutado antes de editar. HEAD y remoto eran F1_START_SHA, arbol limpio, stash vacio,
sin merge/cherry-pick/revert/rebase/sequencer/bisect incompletos. Freeze/Characterization/Discovery cotejados por blob.
origin/main = 7097057cf8685bf5ecc09083cba37379d4a4aae8.
integration/I-57 objeto = a5bc02210f740839e2fac37e63fc00512e5ccee6; target = mismo main indicado.
I-55 = 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d.
I-52 = a59eee6dcd4f7ea7b598a28a62c349304ba517a9: desde el preflight previo solo decisiones/evidencia propias;
ningun cambio material concurrente de AUTH13, DTO/stores/resolvers o contratos AUTH09/10/13. No rebase ni nuevo claim.

## Matriz y fixtures

[Manifest CT58-01..32 y MM completo](manifest.md), [mapping por fila](row-mapping.json),
[casos/kind/expectativa/estado/identidad de carrier](matrix.json), [identidades de fuentes](source-sha256.json).
1.271 casos: Dynamic 340, PushBack 595, Cantilever 189, Cabecera 147.
Siete archivos I58F1*.cs en tests/RackCad.Tests contienen fixtures, matriz explicita, tests y oraculos futuros.
Carrier TEST-ONLY: RackId, hermanos completos, identidad de origen, envelope y Design raw originales,
completitud acreditada, vista/seccion/placement/contexto opaco. No se cambia API productiva.
No skip, trait oculto, reflection equality, raw JSON equality como escape ni nuevas dependencias.
Las operaciones JSON editan fixtures; la comprobacion de no-op valida solo que el estimulo se haya escrito.
Las assertions de salida recorren campos tipados explicitamente; no determinan un outcome alternativo.

## Ejecuciones y limites

Baseline inicial sobre F1_START_SHA limpio: 86/86 impacto heredado, 0 skips; 55.4795168 s, SDK 8.0.423.
Baseline F1 limpio sobre 42c49cc2bfe815fec6a27ece5234e04e4dfdc457: 1.389/1.389, 0 skips; 31.5386801 s.
[Metadata baseline](clean-baseline-meta.json), [metadata inicial](baseline-meta.json).

RED ronda 1 = base 42c49cc2 + [patch exacto](red-round1.patch), SHA-256
ebcc58b18473a077cccdfeec0104d3449618217be11f1ac0cf0adde2b02ede51.
No se atribuye ese RED a arbol limpio. Seleccion 1.005: 624 FAIL por FUTURE_OUTCOME, 381 PASS vacuos, 0 skips.
Todos los fallos: Assert.Equal, esperado Single/Divergent, actual Unreadable. Cero errores de compilacion/setup
ni fallos ajenos en esa corrida. [Assertions por nombre/caso/causa](red-round1-assertions.json),
[comando/SDK/base/patch/tiempo](red-meta.json), [TRX y logs originales comprimidos](initial-runs.zip).
Restauracion byte a byte confirmada, arbol limpio; ningun SHA deliberadamente rojo.

La matriz final agrega permutaciones de identidad y pares iguales de los payloads mutados para CT26/28.
Preparacion posterior: 1.912/1.912 F1 y 86/86 impacto, 0 omitidos. Estas corridas con cambios aun sin commit
NO acreditan el SHA de cierre. La ronda final repetira RED sobre la matriz completa y quedara en el recibo.

Setup descartado, NO RED: [detalle](discarded-setup.json). Primera guarda detecto que el offset de montaje
Cantilever ya era 2 (mutacion no-op, 5 fallos por construccion de matriz). Segundo intento puso null en el
fixture de seam y bloqueo el assembler (1 fallo). Correccion de fixture: base resoluble conserva 2; el par
MM-C03 separado compara null contra 2. No se cambio expectativa congelada ni autoridad productiva.

VACUOUS BASELINE PASS no acredita schema gate, unknown scan, version handling, wrong-kind ni reader.
Self-checks de proyeccion/fidelidad/aislamiento y controles negativos no son GREEN AUTH13. El port actual
sigue Unsupported -> Unreadable; eso no es un defecto. Selective y Cama conservan sus tests heredados.
F2 debera conectar carrier equivalente y repetir oraculos y sabotajes sobre su implementacion real.

El conservadurismo [7,9] vs [7,0] permanece Divergent. Global0 conserva payload/presencia/procedencia/orden;
global positivo excluye solo calculated Header elegible, siempre despues del gate raw/schema/unknown.
La paridad diagnostica comprueba Structure y locales A/B, con catalogo fijo FUERA del comparator.
La seam CT25 prueba AUTH09 con cuatro autoridades reales y AUTH10 sin re-resolve; builders test-only
expresan la frontera de AUTH10, no wiring productivo. Los mutants CT26 de intencion inactiva no exigen
que el catalogo resuelva ese diseño para reconocer su payload; no se agrega una condicion de admision.

## Cierre exact-SHA

Pendiente al crear este archivo: Core Full local con HEAD limpio, builds Debug UI/Plugin y CI push exacta.
El recibo posterior distinguira F1_CLOSURE_SHA del SHA documental de publicacion. No hereda evidencias.
UI local no aplica a este diff puro de caracterizacion; LC-UI exige ui-tests en la CI propia de cierre.
No Owner Validation en F1, sin comportamiento productivo nuevo. No Candidate ni cierre de iniciativa.

Comandos de evidencia: dotnet test tests/RackCad.Tests/RackCad.Tests.csproj (Core Full);
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug;
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug.
Focal: --filter FullyQualifiedName~I58F1.
Impacto: --filter 'FullyQualifiedName~SharedViewFoundationF5Tests|FullyQualifiedName~SharedViewFoundationF6Tests|FullyQualifiedName~SelectiveAuthored'.
RED: --filter FullyQualifiedName~I58F1CharacterizationTests.Frozen_matrix_against_current_port.
Cada corrida conserva comando, SDK, identidad, conteo y log/TRX; ningun filtro cero se acepta.

Consensus Freeze V3 = INTACT
F1 = VALIDATION IN PROGRESS
F2 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO
