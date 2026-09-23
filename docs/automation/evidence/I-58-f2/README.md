# I-58 F2 — implementacion AUTH-13

## Apertura historica

F2_START_SHA = d1921dc90abdc3b24e7f05c8beca9ab14a502c49
Freeze blob = f89671cfe9f1036e24211287514414b3f555182a
Characterization blob = 97e15d6d7c00e02495f8d4159477272d00af6880
Discovery blob = f07c9b6c6f6c278c6f7d37b3b68921eac7a1c5b8
F1 = COMPLETE / COORDINATOR AGREED
F2 = OPEN / IMPLEMENTATION AUTHORIZED
IMPLEMENTATION AUTHORIZATION = YES — F2 ONLY

Preflight: fetch completo; HEAD/upstream esperados; arbol limpio, stash vacio, sin operacion Git pendiente.
main = 7097057cf8685bf5ecc09083cba37379d4a4aae8; integration/I-57 conserva objeto
 a5bc02210f740839e2fac37e63fc00512e5ccee6 y destino main. I-55 sigue 08e45e0a;
I-52 a216c8c5: desde F1 solo docs de SDK/evidencia/decisiones. Sin interseccion productiva
remota con AUTH-13, DTOs, stores o resolvers participantes.

## RED de entrada

Se activa AssertFuture sobre la matriz F1 sin cambiar expected ni fixtures; puertos genericos originales
continuan unsupported. Corrida sobre F2_START_SHA + overlay de test (arbol sucio, no evidencia de cierre).

Resultado RED de entrada: 1271 seleccionadas, 882 assertion FAIL (617 Single + 265 Divergent),
389 PASS vacuos Unreadable, 0 skips. `red/f2-entry-red.trx` conserva cada resultado.
Comando: `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter FullyQualifiedName~Frozen_matrix_against_current_port --logger trx;LogFileName=f2-entry-red.trx --results-directory docs/automation/evidence/I-58-f2/red`.

## Implementacion y frontera

Siete archivos productivos, todos en `src/RackCad.Application/Systems/Shared/`:
- `RackAuthoredComparator.cs`: solo habilita la clase partial; puertos genericos unsupported y Selective intactos.
- `RackAuthoredInput.cs`: carrier inmutable; copia la lista, retiene source, kind, raw envelope/Design y prueba de completitud.
- `AuthoredRawReader.cs`: envelope/wrapper, pertenencia, completitud de raw, schema exacto y helpers de tipos.
- `AuthoredRawReader.Shapes.cs`: vocabulario cerrado recursivo de 41 tipos; duplicados/case collisions, tipos y enums antes de mapper/exclusion.
- `AuthoredCanonical.cs`: defaults F-09, rechazos de valores que perderian intencion y exclusiones F-08 por kind.
- `AuthoredTypedValues.cs`: igualdad explicita por campo y secuencia; copia tipada profunda Cantilever.
- `RackAuthoredComparator.FourKinds.cs`: cuatro overloads concretos con salida de dominio, validacion de TODAS las hermanas antes de comparar y materializar.

Canonical privado apoyado en DTOs; nunca se retorna. Las funciones ToDesign/ToDomain/ToConfiguration
existentes se usan solo DESPUES de raw y acreditacion de cada perdida/default conocido; construyen dominio
campo a campo sin resolver. Cantilever se copia explicitamente sin sanar identidad/subarboles. No hay
From(ToDomain(raw)), igualdad JSON, reflection equality, catalogo, geometria, BOM, AutoCAD ni WPF.
La inspeccion de propiedades usada al escribir el inventario fue tooling externo; la autoridad productiva
es el codigo tipado explicito, no ese tooling. Unknown futuro se rechaza antes de deserializar.

Dynamic: global 0 conserva cada Header completo, presence, ModuleId, orden y provenance. Global positivo
excluye solo Header calculado elegible tras lectura completa; custom permanece. No promociona fallback.
PushBack preserva Structure, SideB, Composite, posiciones, piezas y topologias. Las reglas de ausencia de
vectores sin overrides corresponden al writer real; un vector con contenido conserva posiciones. F-09
solo quita herencia trailing fuera de un rango de niveles conocido; no ordena/deduplica colecciones.
Cantilever: outer acredita pertenencia; inner Guid valido sigue authored, sin restamp ni cambio I-37.
Cabecera: snapshot persistible, sin Members/Exceptions ni RefreshPhysicalModel.
Single reconstruye dominio nuevo despues del consenso; no retorna una hermana. Failures tienen Authored=null.
La comparacion no llama AUTH-09: las pruebas externas usan sus delegates reales, sin abrir F3 ni conectar consumers.

## RED -> GREEN por comportamiento

Todas las corridas de iteracion usan F2_START_SHA + overlay declarado, no un arbol limpio de cierre.
Las expectativas y los 1271 fixtures F1 permanecen intactos. Cada seleccion indicada es mayor que cero.

| Bloque | RED/control | GREEN observado |
|---|---|---|
| Carrier + raw shape | entrada F1: Single/Divergent fallan; negativos antes vacuos | carrier snapshot + shape + 389 causas verificadas |
| Schema/unknown/duplicados | desactivar gates reales produce 16 falsos Single y 16 assertion FAIL | los mismos 16 controles vuelven a Unreadable; control acreditable Single por kind |
| Dynamic | 239 RED F1 de Single/Divergent | 340/340 fixtures |
| PushBack | 466 RED F1 de Single/Divergent | 595/595 fixtures, simple y compuesto |
| Cantilever | 98 RED F1 de Single/Divergent | 189/189 fixtures; identidad exterior/interior |
| Cabecera | 79 RED F1 de Single/Divergent | 147/147 fixtures; frontera persistible |
| Materializacion y parity | Header=null/global promovido/modulo/provenance: 4 sabotajes x 3 casos = 12 FAIL | 3 salidas reales con parity/retention; todos los Singles F1 verifican aislamiento profundo |
| Legacy F-09 adicional | strings no vacios: 3 RED; trailing fuera de rango: 3 RED con normalizacion desactivada; escalares que writer pierde: 4 RED; vector sin overrides: 1 RED | los 11 escenarios GREEN; sin cambiar una expectativa F1 |

Paridad observada en autoridad real: Dynamic 7; PushBack simple 7; PushBack compuesto rack=7, A=7, B=9.
Los sabotajes afectan SOLO salida; lectura e igualdad permanecen intactas. `mutations/materialization-controls.json`
registra cuerpo de sabotaje y hashes; TRX por sabotaje. Todos se restauraron byte-for-byte.
Los 389 PASS vacuos ya tienen causa especifica y control positivo Single en `I58F2ReaderTests`.
No se cuenta el unsupported generico como prueba del reader.

Focal completo + impacto final: **2468 PASS / 0 FAIL / 0 SKIP** (`iterations/f2-final-relevant.trx`):
1912 F1, 437 F2, 119 Foundation/Selective. Matriz AUTH-13: 1271 GREEN, CT58-01..32 y MM38 cubiertas;
CT58-22 se acredita por su Fact de Cama/generic unsupported y pruebas heredadas Selective.
Desglose exhaustivo en `coverage-summary.json`. Core Full limpio y builds/CI de cierre se registran
posteriormente contra su SHA literal: esta iteracion NO los sustituye.

Comando focal/impacto: `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~I58F1|FullyQualifiedName~I58F2|FullyQualifiedName~SharedViewFoundation|FullyQualifiedName~SelectiveAuthored" --logger "trx;LogFileName=f2-final-relevant.trx" --results-directory docs/automation/evidence/I-58-f2/iterations`.
Otros filtros, con el mismo proyecto/runner: `Raw_gate_negative_control` (16),
`Real_Single_keeps_fallback_local_parity_and_provenance` (3 por sabotaje),
`F09_trailing_inherit_outside_levels_has_no_new_intent` (3), `Writer_loss_is_rejected_before_accreditation` (4),
`Optional_front_overrides_with_no_content_follow_the_writer_absence_rule` (1), siempre como FullyQualifiedName~.

## Intentos descartados, preflight final y alcance

- Clasificacion del diagnostico en 5 controles estaba demasiado amplia: confundia null-siblings-reversed
  con sibling null y no reconocia Header.Members. Se corrigio el selector del control; outcomes intactos.
- Primer fixture trailing asumio dos niveles cuando Composite declara tres. No cuenta como RED admisible
  de trailing. Se fijo explicitamente el rango a dos y se observo el RED de 3/3 con normalizacion desactivada.
- Primer sabotaje raw no desactivo schema por el tratamiento CRLF del script: 12 FAIL/4 PASS. Se descarto
  como control completo; repeticion verificada 16/16 FAIL por falso Single. Fuentes restauradas en finally. Los bytes CRLF del primer mutante se conservan en
  `mutations/reader-mutant-original-bytes.json`; su .txt es la presentacion normalizada.

Fetch previo al commit: main/I-57/I-55 intactos. I-52 avanzo de a216c8c5 a
`d1506ac68bddf5880fdf934ca39cf824d9d64a40`; solo 8 archivos propios de docs/decisiones/evidencia V33,
sin AUTH-13, DTO, store, resolver ni interseccion material. SDK resuelto: 8.0.423.
Freeze, Characterization y Discovery mantienen blobs vinculantes. No Domain/DTO/store/resolver/UI/Plugin,
ROADMAP/HANDOFF, I-55/I-52, A-n, Candidate ni integracion. F3 no abierto; G12 no desbloqueado.
UI focal local: no aplica a este diff de autoridad pura sin consumer UI; builds y CI UI siguen requeridos.
Owner Validation: no activada por este diff sin comportamiento visible; no se declara validacion manual.
Sin findings materiales abiertos ni desviacion semantica. Revision Coordinator pendiente de F2 completo.


## Primer SHA de implementacion rechazado por guarda estructural

`39bb0b14fac7853cbcb431a7d9f82dc226d600a5` ejecuto Core Full local limpio, SDK 8.0.423:
10584 PASS / 1 FAIL / 0 SKIP, 10585 total, 101.1730661 s de comando. Fallo exclusivamente
`NamespaceFolderGuardTests.EveryProductionFile_DeclaresExactlyOneBlockScopedNamespace`:
los seis archivos nuevos usaban namespace file-scoped. No se empujo como cierre ni se continuaron builds.
[Recibo y TRX exactos](rejected-39bb0b14/local-receipt.json). Correccion: namespaces con llaves en esos
seis archivos, sin cambio de comportamiento ni debilitamiento de la guarda. El SHA nuevo requiere su
propio Core Full limpio/builds/CI; no hereda nada del intento rechazado.

Focal correctivo (NamespaceFolderGuardTests + I58F2): 444/444 PASS, 0 skips; iterations/namespace-focal-green.trx.


## Recibo de cierre F2 para revision Coordinator

F2_CLOSURE_SHA = `e27df1be2fbca55303c6cde6d6f27c5f35ba6b10`.
[Recibo exacto](closure-receipt.json), [CI leida](ci-closure.json) y [corrida 35911641905](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35911641905).
Todas las clases locales empezaron y terminaron con arbol limpio en ese SHA; SDK resuelto 8.0.423.

| Clase sobre F2_CLOSURE_SHA | Resultado | Duracion de comando medida |
|---|---|---:|
| Core Full local | 10585/10585 PASS, 0 FAIL, 0 SKIP | 103.7474497 s |
| UI Debug build | 0 errores, 0 advertencias | 9.290486 s |
| Plugin Debug build | 0 errores, solo 2 MSB3277 conocidos | 5.3087884 s |
| CI push de rama exacta | 4/4 required jobs SUCCESS | consultar timestamps de corrida |

Core Full incluye el focal F2 completo, CT32/MM38, conversion causal de 389 vacuos y regresiones
Foundation/Selective/Cama/generic unsupported. Version informativa Plugin Debug: `1.0.0+e27df1be2fbca55303c6cde6d6f27c5f35ba6b10`.
Logs/TRX locales originales en `closure/`. No se repitio UI local: no hay cambios UI ni consumer;
la CI propia de cierre si incluye UI Full. No Owner Validation ni Candidate.

La publicacion de este recibo es otro SHA documental: no hereda Core/builds ni transfiere su CI al cierre.
Requiere CI propia y se entrega identificada separadamente. Una incidencia del helper prepush (nombre de
encoding utf8-sig) se corrigio antes del push; no cambio codigo ni evidencia local y no genero SHA nuevo.
No findings materiales abiertos ni A-n. Corregida la guarda de namespaces; los intentos descartados quedan
registrados arriba. El alcance productivo sigue limitado a siete archivos Shared; Domain, DTOs, stores,
resolvers, UI y Plugin intactos; Freeze/Characterization/Discovery byte-for-byte intactos.

Consensus Freeze V3 = INTACT
F1 = COMPLETE / COORDINATOR AGREED
F2 = COMPLETE / COORDINATOR REVIEW REQUIRED
F3 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO
