# I-57 F6 — Evidencia AUTH-12..13

```text
Gate                  = F6 / AUTH-12..13 ONLY
F6_START_SHA          = c7c685202a1a6817825a002ce937b95244ba23fd
F6_IMPLEMENTATION_SHA = 415953fefa5484b9d23829eb3b7b3b18b55b4389
F6_VALIDATION_SHA     = 415953fefa5484b9d23829eb3b7b3b18b55b4389
F6_FINAL_SHA          = THIS CLOSURE COMMIT
Proposal              = V5 @ 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
R3                     = EFFECTIVE / UNCHANGED
ADR-0034               = ACCEPTED / INTACT
ADR-0044               = ACCEPTED / UNCHANGED
F6 result              = COMPLETE
F7                     = NOT OPEN
```

El `F5_START_SHA` durable es `97af3a5aaabad5e37026c4592fae47616a76e15e`; el valor `63dce0...` queda
explicitamente descartado. F6 parte del cierre real de F5, `c7c685202a1a6817825a002ce937b95244ba23fd`.

## AUTH-12 — requirements, query e importer

`LibraryBlockRequirement.Key` es una identidad validada de pieza de biblioteca. La extraccion es tipada:

| Payload / kinds | Productor de requirements | Resultado |
|---|---|---|
| `HeaderRunPlan` — Selective, Dynamic, Push Back, Cabecera y Cama | `RackBlockRequirementExtractors.HeaderRun` sobre `LooseInstances` y `Headers[*].Instances` | keys de `BlockName`, primer spelling preservado, duplicados `OrdinalIgnoreCase`, blanks omitidos como hacia el importer legacy |
| `CantileverViewPlan` | `RackBlockRequirementExtractors.Cantilever` | cero requirements; geometria pura |

No hay reflection, JSON, inspeccion de `object` ni inferencia desde `BaseName`. `RackPreparedView<TPayload>` queda
como contrato final tipado con `Kind`, `Address`, `Frame`, `BaseName`, `BlockRequirements` y `Payload`. El mismo
payload producido por el builder se conserva sin interpretacion geometrica.

`AutoCadLibraryBlockQuery` vive en Plugin y solo abre `BlockTable` para observar `Has(requirement.Key)`. No llama
al importer, no abre la biblioteca externa, no crea, repara, normaliza o sanitiza nombres. `BlockLibraryImporter`
permanece en Plugin; `EnsureForPlan` delega la extraccion a Application y la importacion a `EnsureRequirements`.

La secuencia productiva y el port puro comparten el mismo orden:

```text
import permitido    -> Ensure/import -> query final -> FOUND/MISSING definitivo
import no permitido -> query final -> FOUND/MISSING definitivo
zero requirements   -> sin importer y sin query
```

No se conserva ningun `MISSING` previo. Importar no declara exito: el query posterior es la unica observacion
final. La key `RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL` demuestra que `.` viaja literalmente y nunca pasa por
`BlockNaming`.

### BLK-ID y BLK-AVAILABILITY

| Invariante | Resultado |
|---|---|
| BLK-ID-1..3 | PASS — `BaseName` y `Key` nacen de productores distintos; no hay sustitucion, derivacion ni sanitizer cruzado |
| BLK-ID-4 | PASS — la colision de nombre generada no muta la coleccion de requirements |
| BLK-ID-5 | PASS — query recibe exclusivamente `LibraryBlockRequirement` |
| BLK-ID-6 | PASS — importer recibe requirements extraidos del payload, nunca nombres de vista |
| BLK-AVAILABILITY-01 | PASS — import permitido precede query final y termina `FOUND` cuando el import agrega la pieza |
| BLK-AVAILABILITY-02 | PASS — sin permiso se consulta una vez, queda `MISSING` y no hay side effect |
| BLK-AVAILABILITY-03 | PASS — import fallido seguido de query queda `MISSING`, sin false success |
| CT-BLK | GREEN — identidad, dot, empty, whitespace, duplicates, zero requirements y query puro |

## AUTH-13 — comparator authored

`IRackAuthoredComparatorPort<TInput,TAuthored>` devuelve `Single`, `Divergent` o `Unreadable`. El adapter
Selective llama a `SelectiveAuthoredAuthority.Resolve`, la autoridad estructural include-by-default vigente, y
proyecta su resultado sin resolver geometria ni elegir hermana. Esa autoridad conserva estructura, bindings y
fuentes authored, expressions, `CustomProperties`, `SchemaVersion` y extension data conforme a su contrato.

| Kind | Adapter | Resultado |
|---|---|---|
| Selective | reutiliza `SelectiveAuthoredAuthority` | Single / Divergent / Unreadable |
| Dynamic | adapter explicito sin equivalencia demostrada | Unreadable |
| Push Back | adapter explicito sin equivalencia demostrada | Unreadable |
| Cantilever | adapter explicito sin equivalencia demostrada | Unreadable |
| Cabecera | adapter explicito sin equivalencia demostrada | Unreadable |
| Cama | adapter explicito sin equivalencia demostrada | Unreadable |

El contrato compartido no contiene serializacion JSON, comparator generico, `First`, mayoria, representante ni
fallback. `CT-AUTH = GREEN`; divergence y unreadable siempre devuelven autoridad nula.

## RED -> GREEN y evidencia automatizada

La primera corrida RED fallo en compilacion con siete `CS0246`: los contratos AUTH-12/13 aun no existian. El
GREEN focal cubre los expected de F6 y selecciona mas de cero pruebas.

El arbol estaba limpio y `HEAD` era el SHA exacto de validacion antes de producir la evidencia local:

| Evidencia | Resultado |
|---|---|
| T0 AUTH-12/13 focal | 18/18 PASS |
| T1 impacto + CT + paridad | 136/136 PASS |
| Core Full local | 8,236 passed; 0 failed; 0 skipped |
| UI Full local | 1,581 passed; 0 failed; 17 historical skipped |
| Build Debug UI | success; 0 errors; 0 warnings |
| Build Debug Plugin | success; 0 errors; solo los dos `MSB3277` conocidos |
| CI push exacta | run `35499301422`; `event=push`; branch exacta; head exacto; 4/4 jobs `success` |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35499301422>

## Paridad, schema y scope guard

El extractor nuevo produce el mismo conjunto y orden de keys que el pipeline legacy para planes con instancias
loose, groups, duplicados y blanks. El importer sigue usando `WblockCloneObjects`, conserva su cache y devuelve el
mismo conteo; se agrega solo la observacion final. Geometria, naming, BOM y errores permanecen en sus autoridades.
El adapter Selective proyecta los mismos tres outcomes y la misma instancia authored de la autoridad vigente.

El diff F6 no toca DTO de persistencia, `RackEmbedDocument`, schema, catalogos, assets, builders geometricos,
resolvers, comandos, BOM, materializers, I-52, I-55 ni ADR. `schema diff = NONE`; `observable behavior diff =
NONE`, confirmado por OV-FND-03 importation. AUTH-15, creacion caller-owned y consumer policy permanecen fuera
de alcance.

## DLL y biblioteca para OV-FND-03

```text
DLL path       = src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll
ProductVersion = 1.0.0+415953fefa5484b9d23829eb3b7b3b18b55b4389
DLL SHA-256    = 8C210802692029E237FF8732C64385DF6D02D0654BE94BD8AC8EE0E80C7ABB23
Library path   = D:\Base_de_datos_AutoCAD_V.0.dwg
Library SHA-256= B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
```

La matriz manual A-E y su resultado durable estan en
[`I-57-f6-owner-validation.md`](I-57-f6-owner-validation.md). El Owner ejecuto los cinco casos sobre el SHA,
DLL, AutoCAD 2025 y biblioteca identificados: A-E, names, geometry, BOM y save/reopen quedaron `PASS`, sin
regresion visible. El build exacto de AutoCAD no fue suministrado y no se infiere. AUTH-12/13 y F6 quedan
`COMPLETE`; AUTH-15 no fue implementada y F7 no fue abierta.
