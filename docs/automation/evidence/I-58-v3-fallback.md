# I-58 — AR58-V2-01: caracterizacion V3 del fallback vigente

## Identidad, metodo y alcance

PROBE_VALIDATION_SHA: c978fdeadd840f40f53a9ab6bb1e46d74c4b2c36.
Harness versionado ANTES de ejecutar, arbol limpio antes/despues, SDK resuelto 8.0.423.
Catalogos versionados copiados por MSBuild: [identidades exactas](I-58-v3-source-identities.txt).
Mismo objeto catalogo en cada pareja original/proyectado; ningun cambio de resolver/store/DTO.
Fixture base Dynamic: tarima40x48x50,1000kg; fondos4; frente2; modulos header longitud48;
header presente132x48, posts POSTE_OMEGA_3X3, peralte variable. Codigo completo en
[I-58-v3-probe/Program.cs](I-58-v3-probe/Program.cs), instrucciones y limites en su README.

`dotnet run --project docs/automation/evidence/I-58-v3-probe/I58.V3.Probe.csproj`
Exit0; wall7.1721519s; SELECTED124; ASSERTIONS1205; NEGATIVE_CONTROLS4.
[Log completo por caso](I-58-v3-probe.log), [metadata](I-58-v3-probe-meta.json).
Desglose: Dynamic50 + PushBack simple50 + sabotajes4 + PushBack compuesto20 =124 >0.
Los4 sabotajes DEBEN detectar desigualdad; no son4 tests del comparator ni un GREEN AUTH-13.

La proyeccion local al harness copia mediante DTO conocido y solo borra calculated Header si global>0.
No compara hermanas, no escanea unknowns, no devuelve Single ni se presenta como materializador productivo.
Los mappers usados en fixtures no acreditan fidelidad para todos los valores F-07/09: eso sigue en F1.
Este diagnostico prueba paridad de peralte para los casos seleccionados; las comprobaciones de frame
(height/depth/peralte/conteos horizontals/panels) son complementarias, NO equivalencia completa de geometria/BOM.

## Dynamic — matriz de un header (16 casos)

| Global authored | Procedencia | Header null | Header peralte0 | Header peralte7 | Header peralte9 |
|---|---|---|---|---|---|
| 0 | calculated | 3 | 3 | 7 | 9 |
| 0 | custom | 3* | 3 | 7 | 9 |
| 8 | calculated | 8 | 8 | 8 | 8 |
| 8 | custom | 8* | 8 | 8 | 8 |

Cada celda es PostPeralte RESUELTO por DynamicRackSystemResolver actual. Proyectado V3 coincide.
*Custom+null es una entrada deliberada para observar tolerancia actual; resolver reconstruye calculated.
V3 la declara Unreadable ANTES del mapper: no se cuenta como authored acreditado ni equivalencia admitida.
Default/catalog observado sin header positivo =3; no se introduce ese3 en el global authored heredado.

## Push Back de un sentido — matriz real (16 casos)

| Structure.PostPeralte | Procedencia | Header null | Header peralte0 | Header peralte7 | Header peralte9 |
|---|---|---|---|---|---|
| 0 | calculated | 3 | 3 | 7 | 9 |
| 0 | custom | 3* | 3 | 7 | 9 |
| 8 | calculated | 8 | 8 | 8 | 8 |
| 8 | custom | 8* | 8 | 8 | 8 |

Cada celda es `PushBackResolver.Resolve(design).Structure.PostPeralte`, ejecutado realmente.
Mismo asterisco custom+null. No se deduce este resultado solamente de la prueba Dynamic.

## Multiples headers — Dynamic32 + PushBack32

Para C/C, U/U, C/U y U/C (C calculated, U custom), se ejecutaron TODAS las celdas siguientes
por separado en ambos resolvers. Procedencia no filtra el fallback. El log conserva cada combinacion.

| Global | Orden de peraltes por modulo | Dynamic | PushBack Structure | Proyeccion V3 |
|---|---|---|---|---|
| 0 | [7,9] | 7 | 7 | mismo |
| 0 | [9,7] | 9 | 9 | mismo |
| 0 | [0,7] | 7 | 7 | mismo |
| 0 | [7,0] | 7 | 7 | mismo |
| 8 | cada una de las4 secuencias | 8 | 8 | mismo |

Orden de modulos es material; NO es el orden de hermanas. [7,9] vs [7,0] coincide hoy en7 pero V3
elige Divergent conservador por payload por modulo; no se afirma que ese segundo9 sea irrelevante para
otros transportes/particiones. La propuesta requiere acuerdo Architect/Coordinator sobre ese coste.

## Legacy y controles negativos

Global DTO ausente y null se deserializaron expresamente: dominio0, calculated header7, resultado7
para Dynamic y PushBack (4 casos). Se comprobo save/reopen con store real en casos simples acreditables:
el global sigue0 y el resultado mantiene peralte. Header.PostPeralte nullable en header presente usa0
por mapper de cabecera; no se equipara Header ausente a Header presente con0 bajo global0.

Los4 controles de V2 quitaron SOLO Header calculado con global0: Dynamic7->3,9->3; PushBack7->3,9->3.
El assert de parity los rechazo. Son reproduccion del defecto de diseño de V2 sobre resolvers actuales;
no son RED->GREEN de un comparator implementado. Promover global puede conservar parity numerica pero
violaria authored; CT58-31 exige tambien assert independiente de global/provenance/modulo.

## Push Back compuesto —20 casos

Fixture: A5 fondos/3 niveles, B4 fondos/2 niveles, un frente por lado, primera altura4, beam4, topology
Encontradas. Topologia de modulos generada por resolver real y Snapshot SOLO para seed del fixture;
despues global se vuelve a fijar en0/8. IDs header observados: M1,M3,M5,B:M4,B:M1.
El diseño candidato no se obtiene de Snapshot: la proyeccion copia el input y aplica F-08 localmente.

| Global | Procedencia comun | Header peraltes en ese orden | Structure | Local A | Local B |
|---|---|---|---|---|---|
| 0 | cada una de calc y custom | [null,0,null,0,null] | 3 | 3 | 3 |
| 0 | cada una de calc y custom | [7,9,7,9,7] | 7 | 7 | 7 |
| 0 | cada una de calc y custom | [9,7,9,7,9] | 9 | 9 | 9 |
| 0 | cada una de calc y custom | [0,7,0,7,0] | 7 | 7 | 7 |
| 8 | cada una de calc y custom | cada secuencia anterior | 8 | 8 | 8 |
| 0 | A calc/B custom y A custom/B calc | A headers7; B headers9 | 7 | 7 | 9 |
| 8 | A calc/B custom y A custom/B calc | A headers7; B headers9 | 8 | 8 | 8 |

Las primeras5 filas agrupan16 casos y las ultimas2 agrupan4. Proyectado V3 conserva todos los resultados,
incluidos locales A/B. Casos custom+null no son inputs acreditables; DTO repara su flag y quedan SOLO
como observacion del camino actual. Las mezclas A7/B9 SI usan headers presentes y flags conservados.
Particion y reversion B explican por que no basta guardar un unico primer positivo del rack.

## Eleccion V3 y limites de suficiencia

Con global>0: gate raw completo y despues calculated Header=null. Codigo ResolvePostPeralte escoge
global y Resolve reconstruye header; probe confirma peralte y campos fisicos seleccionados. Custom no se
excluye. Con global0: presencia + payload PERSISTIBLE completo tipado por modulo. No inventa un header
minimo ni dimensiones para que pase la copia de editor; no cambia global ni flag; no elige primer sibling.
Es suficiente para no perder fallback porque no elimina ninguno de sus valores/posiciones, y conserva
los payloads transportados. NO se afirma minimalidad: Name/Height/posts en calculada/global0 quedan
conservadores, con posible Divergent aun si resolved actual igual. Ese coste debe revisarse expresamente.
No se promueve una comparacion geometrica ni se usa resolver/catalogo en igualdad.

Los asserts actuales verifican global, moduleId/provenance, peralte en cada header retenido, aislamiento
por identidad y mutacion de peralte; NO cada objeto nested futuro ni permutations de siblings. CT58-25..32
exigen esas pruebas con API real y negativos independientes. Comparator sigue unsupported; F1 NOT OPEN.

## Corridas y evidencia de regresion seleccionada

- 43910a9c56b6154ab4d0ebd98dadfd2dc1c36a49: fallo de compilacion CS1061 (Panels vs BracingPanels),
  exit1,25.6240357s, arbol limpio;0 casos. FALLO de preparacion, no evidencia conductual ni RED admisible.
  [log](I-58-v3-probe-setup-failure.log), [metadata](I-58-v3-probe-setup-failure-meta.json).
- 946b223ebc5b2b20b1ff1d117f11b95e14608776:120 casos/1181 asserts/4 negativos, exit0,11.481415s,
  arbol limpio; [log](I-58-v3-probe-initial.log), [metadata](I-58-v3-probe-initial-meta.json).
- c978fdeadd840f40f53a9ab6bb1e46d74c4b2c36:124 casos/1205 asserts/4 negativos, exit0,7.1721519s,
  arbol limpio; corrida completa indicada arriba. SDK8.0.423 en las tres. No herencia entre SHAs.
- Core focal sobre c978fdeadd840f40f53a9ab6bb1e46d74c4b2c36 limpio:69/69,0 fallos,0 omitidos,
  wall17.7357463s; SDK8.0.423; [log](I-58-v3-core.log), [metadata](I-58-v3-core-meta.json).
  TRX inspeccionado: DynamicRackSystemResolverTests11, PushBackCompositeStructureTests15,
  RackModuleEditSessionTests25, SharedViewFoundationF6Tests18. Seleccion total69 >0, clases esperadas reales.
  El termino OR RackModuleReconciliationTests no encontro clase; NO se atribuyen pruebas a ese termino.

Comando focal REAL:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter 'FullyQualifiedName~DynamicRackSystemResolverTests|FullyQualifiedName~PushBackCompositeStructureTests|FullyQualifiedName~RackModuleEditSessionTests|FullyQualifiedName~RackModuleReconciliationTests|FullyQualifiedName~SharedViewFoundationF6Tests' --logger 'trx;LogFileName=I58-v3-core.trx' --results-directory "$env:TEMP/I58-v3-results"
```

No se exige/cierra gate funcional, Candidate ni cierre documental de iniciativa en esta correccion D/F0.
No Full local/UI local/Owner AutoCAD nuevos; no reclamar esas clases. CI historica de054606ac es propia
de ese SHA, no acredita el probe ni los commits V3. Identidad del paquete V3 en evidencia canonica.
