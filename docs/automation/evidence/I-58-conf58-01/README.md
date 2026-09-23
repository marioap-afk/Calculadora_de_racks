# CONF58-01 — SchemaVersion exterior whitespace

Entrada rechazada para READY-06: `2e141b034b485f5faa55d412b476e99ebad23c5c`.
F1/F2 previamente acordados. F3 tecnicamente completo, conformidad bloqueada por CONF58-01.
Finding comunicado por Owner; corregir no emite su disposicion final ni declara CONFORMING.

## Contrato y preflight

Discovery V3 §matriz schema/unknown, incorporada por Freeze F-10: whitespace exterior puede recortarse;
string vacio/blanco no es ausencia y falla. Esta correccion implementa ese mismo contrato; A-n NONE.
Freeze blob `f89671cfe9f1036e24211287514414b3f555182a`, Characterization
`97e15d6d7c00e02495f8d4159477272d00af6880`, Discovery `f07c9b6c6f6c278c6f7d37b3b68921eac7a1c5b8` intactos.

Antes de escribir: fetch/prune, rama architecture/shared-view-authored-comparators y upstream coinciden
con entrada, arbol limpio, stash vacio, sin operacion Git incompleta. main
`7097057cf8685bf5ecc09083cba37379d4a4aae8`; integration/I-57 objeto
`a5bc02210f740839e2fac37e63fc00512e5ccee6`, peeled=main. I-55
`08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; I-52
`d99dafeda20bcb48e3fc4af7093e9982d60b9288`. Ninguna punta avanzo desde la publicacion F3;
sin interseccion nueva en AUTH09/10/13, DTOs, stores o resolvers. Ver preflight-final.json al publicar.

## RED antes del fix, sin commit rojo

Se agrega I58Conf58SchemaWhitespaceTests sin editar produccion. Reader blob pre-fix:
`e68513bdf9c0a2da87c355824b75630b621a1531`, identico al HEAD de entrada.
84/84 assertion FAIL, 0 PASS, 0 SKIP, expected Single / actual Unreadable, todos en la asercion de la
pareja padded. El control conocido sin whitespace pasa antes de esa asercion: setup acreditado.
[Recibo RED](red/receipt.json), [TRX](red/whitespace-red.trx), fuente exacta previa en red/pre-fix-test.cs.txt.
No se versiona ni publica un SHA deliberadamente rojo; es evidencia de entrada + overlay de tests declarado.

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter FullyQualifiedName~Known_schema_with_exterior_whitespace_is_Single --logger 'trx;LogFileName=whitespace-red.trx' --results-directory docs/automation/evidence/I-58-conf58-01/red
```

## Fronteras cubiertas

28 fronteras x 3 variantes (espacios, tab/CR/LF, whitespace Unicode) = 84 casos. En GREEN se prueban
ambos ordenes de siblings equivalentes. Inventario de nombres exactos en boundaries.json.

| Frontera | Casos base |
|---|---:|
| Envelope 1.0 en los cuatro kinds | 4 |
| Wrapper 1.0/2.0 en los cuatro kinds | 8 |
| Payload PushBack 1.0 / Cantilever 1.0 | 2 |
| Cabecera Header wrapped / Header plano 1.0 | 2 |
| Dynamic Module.Header, calculated/custom x global0/positivo | 4 |
| PushBack Structure.Module.Header, calculated/custom x global0/positivo | 4 |
| Dynamic HeaderLineOverrides.Header, global0/positivo | 2 |
| PushBack Structure.HeaderLineOverrides.Header, global0/positivo | 2 |

Los Headers calculated que luego se excluyen por global positivo siguen pasando primero el raw/schema
gate. Se acreditan por separado modulo, override de linea, custom y Header plano; no se extrapola una
prueba root a todas las fronteras nested.

## Diff productivo unico

Solo AuthoredRawReader.Schema: conservar null legacy, exigir string, obtener valor, Trim local para
interpretacion y comparar contra 1.0/2.0 conocidos. No reescribe raw, DTO ni string authored.
SchemaVersionPolicy, DTOs, stores, serializers, canonical equality, resolvers y toda otra produccion intactos.
No Freeze/Characterization/Discovery, A-n, Candidate, main, integracion ni tag.

## GREEN y regresion

Focal reader/schema: 988/988 PASS, 0 FAIL/SKIP; green/reader-schema.trx y reader-summary.json.
Desglose: 84 whitespace, 392 malformed/future, 4 authored-name no-trim, 100 casos CT58-06/07/08,
408 F2 reader. Cada invalid cuenta con positivo Single en la misma frontera y se comprueba en ambos ordenes.

Los 14 valores invalidos por frontera cubren vacio/blanco, 1.1, padded 1.1, 2.1, padded 2.1, future major,
signo positivo/negativo, segmento extra, overflow, numero JSON, objeto y array. Padded 1.1 sigue
Unreadable en las 28 fronteras; no se acepta una version futura por Trim. Names authored con whitespace
siguen Divergent en cuatro kinds. Diagnostic exige SchemaVersion + unrecognized schema y Authored=null.

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter 'FullyQualifiedName~I58Conf58SchemaWhitespaceTests|FullyQualifiedName~I58F2ReaderTests' --logger 'trx;LogFileName=reader-schema.trx' --results-directory docs/automation/evidence/I-58-conf58-01/green
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter 'FullyQualifiedName~I58|FullyQualifiedName~SharedViewFoundation|FullyQualifiedName~SelectiveAuthored' --logger 'trx;LogFileName=impact.trx' --results-directory docs/automation/evidence/I-58-conf58-01/green
```

Impacto F1/F2/F3: 3104/3104 PASS, 0 FAIL/SKIP. CT schema rows: green/impact.trx y green/impact-summary.json. CT58-06/07/08
reutilizan los oraculos originales, no cambian expected ni fixtures. CT09/17 y el resto CT32/MM38 se
reverifican en la matriz F1 completa. Cero nuevos skips o debilitamiento de pruebas.

## Identidad, estado y siguiente revision

La punta rechazada 2e141b03 queda invalidada para conformidad. Sus evidencias historicas y las de F2/F3
conservan sus identidades, no se trasladan al fix. Cierre nuevo: commit limpio -> Core Full local -> UI/Plugin
Debug -> push -> CI propia 4/4. Publicacion de recibo/paquete: focal/impacto y CI propios del nuevo SHA.
Los recibos posteriores identifican literalmente cierre y punta propuesta; una publicacion documental
no hereda Core/builds. READY-06 PENDING, sin Candidate hasta veredictos exactos externos.

La evaluacion OV58-04 sigue NO para el diff puro sin consumer visible; no se ejecuta AutoCAD ni se
declara PASS manual. No nuevo comportamiento visible ni modificacion a la asignacion congelada.

CONF58-01 = CORRECTED / COORDINATOR RE-REVIEW REQUIRED
F1 = COMPLETE / COORDINATOR AGREED
F2 = COMPLETE / CORRECTION APPLIED
F3 = COMPLETE / CONFORMANCE RE-REVIEW REQUIRED
READY-06 = PENDING
FINAL_CANDIDATE_SHA = NOT DECLARED
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO
