# I-58 F3 — seam, compatibilidad y paquete de conformidad

F3_START_SHA = `13a1be8c51a61fcc8f0810963fdd867d2a32a05f`.
F1/F2 = COMPLETE / COORDINATOR AGREED, comunicado por Owner al autorizar F3.
Freeze blob `f89671cfe9f1036e24211287514414b3f555182a`; Characterization
`97e15d6d7c00e02495f8d4159477272d00af6880`; Discovery `f07c9b6c6f6c278c6f7d37b3b68921eac7a1c5b8`.
A-n NONE. No REQUIRED material abierto. Revision completa externa pendiente, no Candidate.

## Preflight e intersecciones

Fetch/prune antes de editar, branch architecture/shared-view-authored-comparators y upstream exactos,
HEAD esperado, arbol limpio, stash vacio y sin merge/rebase/cherry-pick pendiente.
main `7097057cf8685bf5ecc09083cba37379d4a4aae8`; integration/I-57 objeto
`a5bc02210f740839e2fac37e63fc00512e5ccee6`, destino `7097057cf8685bf5ecc09083cba37379d4a4aae8`.
I-55 `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; I-52 `d1506ac68bddf5880fdf934ca39cf824d9d64a40`.
Ninguna punta avanzo desde F2. Se reinspecciono el diff de ambas ramas contra main en Shared,
Persistence y Domain: vacio. AUTH09/10/13 y hot files sin nueva interseccion material.
El fetch final detecto I-52 d99dafeda20bcb48e3fc4af7093e9982d60b9288: nueve documentos V34 propios;
Proposal V34 §10 declara AUTH01..13 unchanged. Sin src ni contradiccion material; inspeccionado antes
de continuar. Ver [preflight final](preflight.json) para la repeticion anterior a READY-05.

## Resultado y alcance

F3 no cambia ningun archivo productivo. Agrega `I58F3SeamTests.cs` y docs/state/evidencia propios.
La composicion pura consume los puertos existentes AUTH09/10 y autoridades reales; no crea adapter
universal, igualdad alternativa, wiring I-55 ni consumer visible. Diff acumulado productivo contra main:
los mismos siete archivos Shared de F2. Domain, DTOs, stores, SchemaVersionPolicy, resolvers,
UI, Plugin, geometria/BOM, Cama policy, AUTH15, I-52, I-55, I-57 y R3 intactos.
FOUNDATIONS/HANDOFF/ROADMAP no se editan. [Draft factual](FOUNDATIONS-draft.md) preparado antes de READY-04.

La [matriz de conformidad](conformance.md) mapea F-01..13, autoridades, pruebas y OV.
La [matriz CT32/MM38](ct-mm-matrix.md) enlaza cada fila a sus fixtures y ejecucion real.
PASS en esos documentos es un hecho de pruebas, no un veredicto CONFORMING.

## Pruebas conductuales y controles

Entrada limpia: 41/41 PASS de I58F2MaterializationTests + SharedViewFoundationF5Tests/F6Tests,
sobre F3_START_SHA (`entry/entry.trx`). Ya existe la seam; no hay implementacion productiva nueva
para la cual inventar RED. Se agregan pruebas de integracion y se acredita sensibilidad con sabotajes.

56 escenarios: siete modos (Dynamic global0, global positivo, custom; PB simple y compuesto; Cantilever;
Cabecera) x cuatro inputs (Single, Divergent, Unreadable, A/B/X MixedUnreadable) x dos ordenes de hermanas.
Una operacion Single llama AUTH09 y su autoridad una vez y AUTH10/builder una vez; los fallos llaman cero.
Planes reales no vacios; Cantilever reutiliza vistas de la computation. Aislamiento profundo y parity
se comprueban despues de resolve, prepare y mutacion de working/effective; no hay mocks del resultado.

`run-seam-controls.py` modifica SOLO la composicion test-only, nunca I-57 ni otra produccion.
Restauracion byte-for-byte en finally verificada en `controls/summary.json`.

| Sabotaje | Ejecucion real provocada | RED observado sobre 56 seleccionadas |
|---|---|---:|
| double-authority | dos autoridades reales por AUTH09 | 14 assertion FAIL |
| prepare-reresolve | AUTH10 vuelve a llamar AUTH09 y autoridad real | 14 assertion FAIL |
| double-builder | dos builders reales por AUTH10 | 14 assertion FAIL |
| without-working-copy | authored entregado directamente; se omite check temprano para llegar a mutacion real | 14 assertion FAIL |
| resolve-on-failure | AUTH09 se invoca aun con Authored null | 42 assertion FAIL |

98 violaciones detectadas en total; los casos no afectados permanecen PASS. No se cuentan errores de
compilacion, cero seleccion o excepciones ajenas como RED. El control de copia falla por valores/estado
mutados, no por guardia textual. Logs/TRX y reemplazos exactos registrados en controls/.

Comandos:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter FullyQualifiedName~I58F3SeamTests --logger 'trx;LogFileName=seam-green.trx' --results-directory docs/automation/evidence/I-58-f3/iterations
python docs/automation/evidence/I-58-f3/run-seam-controls.py
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter 'FullyQualifiedName~I58F1|FullyQualifiedName~I58F2|FullyQualifiedName~I58F3|FullyQualifiedName~SharedViewFoundation|FullyQualifiedName~SelectiveAuthored' --logger 'trx;LogFileName=f3-impact.trx' --results-directory docs/automation/evidence/I-58-f3/iterations
```

Resultados finales y selecciones: ver `test-summary.json`; sin nuevos skips ni expectativas debilitadas.
F1 1271 fixtures/CT32/MM38 intactos; 389 antiguos vacuous siguen acreditados por reader causal real F2.
Selective sigue su autoridad historica; Cama/genericos quedan cerrados. No se reconvierte DTO en la entrada
AUTH09 ni se devuelve effective como authored. Parity PB compuesto: rack=7, A=7, B=9.

## Intentos descartados y limites

Primer build de la prueba nueva tuvo dos errores de fixture/harness: acceso a PushBackDesign donde
RetentionViolations recibe Structure y nombre View en vez de RackViewAddress.Kind. Corregidos solo
en la prueba; `iterations/setup-compile.log` no cuenta como RED conductual. Despues: 56/56 GREEN.
No fallo productivo ni contradiccion nueva. Sin decisiones pendientes de producto, A-n o desviacion material.

La prueba cuenta invocaciones a las autoridades vigentes, no instrumenta sus llamadas internas.
Cantilever Build conserva su unica llamada interna a CantileverLineResolver y sus fuentes no cambian;
el consumer de la seam no llama ese resolver. PushBack conserva composicion interna vigente.
Estas pruebas acreditan composicion pura permitida por F-13; no afirman consumo productivo por I-55.

OV58-04 applicable at Candidate = NO para diff actual puro sin consumer visible. No AutoCAD, no PASS
manual. Asignacion congelada intacta y evaluacion incluida para revision externa; vease conformance.md.
UI focal local no aplica a este diff; builds Debug y UI Full de CI requeridos. No Candidate ni UI Full
local de Candidate en esta fase. Core Full local de cierre se ejecuta despues del commit limpio exacto.

## Identidad de cierre y frontera externa

Los resultados locales/CI posteriores al commit se publican mediante recibo separado identificado por
SHA; no se atribuyen retrospectivamente a iteraciones con overlay. El SHA propuesto para conformance
sera la punta publicada con CI propia y focales propios, nunca una identidad inferida por igualdad de arbol.
Core Full/builds de F3_CLOSURE_SHA solo acreditan ese SHA; una publicacion documental no los hereda.
READY-06 espera Coordinator + Architect CONFORMING exactos; FINAL_CANDIDATE_SHA no declarado.
