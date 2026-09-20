# I-57 F3 — Evidencia AUTH-05

```text
Gate                  = F3 / AUTH-05 ONLY
F3_START_SHA          = 7126f06488fbcd1a6cf892731ed46a5ea4087098
F3_IMPLEMENTATION_SHA = 923deb6e861c50287f3c970e835ac7a4eb7a67cc
F3_VALIDATION_SHA     = 3102368ba54190e93cc6daf1d6d7e1ef72ba4266
Proposal              = V5 @ 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
R3                     = EFFECTIVE / UNCHANGED
ADR-0044               = ACCEPTED / UNCHANGED
F3 result              = COMPLETE
F4                     = NOT OPEN
```

## Resultado de AUTH-05

`RackViewFrame` es un value neutral de Application con mapa y orientacion de ejes fisicos `R/D/H`, origen
fisico, intervalo `[KMin,KMax]`, centro derivado exactamente como `(KMin + KMax) / 2`, convencion de extremos,
offset tipado por variante y bounds dibujados opcionales. `RackViewFrameResult` representa de forma tipada una
fuente ausente, address no soportada, variante no disponible, geometria vacia, ejes repetidos, orientacion
invalida y numeros no finitos o invertidos. No existe frame cero de fallback, clamping ni cambio de address.

Los adapters proyectan hechos desde las autoridades vigentes:

| Kind | Fuente productiva reutilizada |
|---|---|
| Selective | `SelectiveDepthLayout`, `SelectivePostGeometry`, `SelectiveLateralBuilder` |
| Dynamic | `DynamicFrontGeometry`, `DynamicDepthGeometry`, `DynamicSystemLateralBuilder` |
| Push Back | estructura Dynamic resuelta, `PushBackSystemLateralBuilder`, `PushBackCompositeSystem` |
| Cantilever | `CantileverLineAssembly.Envelope`, estaciones resueltas y `CantileverViewPlan.Bounds` |
| Cabecera | `RackFrameConfiguration.Depth` resuelto |
| Cama | `FlowBedConfiguration.LaneDepth` |

El adaptador Cantilever obtiene el span fisico de la envolvente del assembly y conserva por separado
`CantileverViewPlan.Bounds`; nunca deriva `KMin/KMax` de esos bounds. Los cortes Selective, Dynamic y Push Back
conservan el indice semantico y leen la coordenada del poste real. Push Back conserva ademas lado y extremo
explicitos. Los adapters no contienen API AutoCAD, WPF, policy de consumer ni builders alternativos.

## RED → GREEN y CT-05

La primera seleccion focal fallo al compilar porque `RackViewFrame`, el mapa de ejes, los values fisicos y el
resultado tipado todavia no existian. El GREEN final contiene 10 pruebas AUTH-05 que cubren los seis kinds,
centro, ejes/orientacion, origen, convencion de extremos, offset, bounds separados, fallos cerrados, Dynamic
Entrance/Exit, post real, Push Back por post/lado/extremo, Cantilever `Station(n)`, Cabecera lateral/planta y la
unica vista vigente de Cama.

CT-05 original permanece intacta y verde. No aparecio contradiccion material ni fue necesario modificar una
autoridad geometrica existente.

## Evidencia automatizada sobre el SHA validado

El arbol estaba limpio antes de producir la evidencia sobre
`3102368ba54190e93cc6daf1d6d7e1ef72ba4266`.

| Evidencia | Resultado |
|---|---|
| AUTH-05 focal | 10/10 PASS |
| Core Full local | 8,184 passed; 0 failed; 0 skipped |
| UI Full local | 1,581 passed; 0 failed; 17 historical skipped |
| Build Debug completo | success; 0 errors; solo los 2 `MSB3277` conocidos de referencias AutoCAD |
| CI push exacta | run `35489659734`; event `push`; branch exacta; head exacto; 4/4 jobs success |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35489659734>

## OV-FND-01

El Owner ejecuto la matriz sobre el SHA exacto
`3102368ba54190e93cc6daf1d6d7e1ef72ba4266` y confirmo explicitamente:

```text
Environment = AutoCAD 2025
Exact build = NOT SUPPLIED BY OWNER

Selective  = PASS
Dynamic    = PASS
Push Back  = PASS
Cantilever = PASS
Cabecera   = PASS
Cama       = PASS

Global = PASS 6/6
```

Paridad adicional confirmada: ningun bloque extra, perdido o renombrado; actualizacion in-place correcta;
save/reopen PASS; BOM PASS; comandos y mensajes PASS; sin regresiones visibles.

La guia de validacion manual §7 exige registrar la version de AutoCAD, que el Owner identifico como
`AutoCAD 2025`; no exige un numero de build separado para aprobar esta ronda. La ausencia del build exacto se
conserva literalmente como metadata no suministrada y no rebaja los seis resultados funcionales.

## Scope guard y cierre

El diff productivo de F3 contiene solamente `RackViewFrame` y adapters AUTH-05; el unico diff adicional son sus
pruebas. No existen `PreparedViewPlan` ni implementaciones adelantadas de AUTH-06..13. No cambiaron DWG, schema,
persistencia, readers, codec, availability, BOM, bloques, nombres, comandos ni transacciones.

F3 queda `COMPLETE`. Este recibo no abre F4, no declara Candidato, no integra la rama y no modifica Proposal V5,
R3 ni ADR-0044.
