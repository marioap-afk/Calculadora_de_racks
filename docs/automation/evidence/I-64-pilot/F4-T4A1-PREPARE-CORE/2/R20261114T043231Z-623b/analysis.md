# I-64 F4-T4A1-PREPARE-CORE, intento 2: análisis conjunto de las revisiones y disposición

**Objeto:**
- la entrega del intento 2: RED `921e3a36` y GREEN `bbd9a35e`, sobre BaseSha `443b5b3a`;
- la verificación `R20261114T043231Z-623b`, EXECUTION_VERIFIED 14/14.

## 1. Revisiones

- **La directa de la sesión** (`direct-review.md`): sin bloqueantes ni importantes, con 3 menores (D-1 a D-3).
- **La adversarial independiente** (`independent-review.md`): ACEPTABLE CON MENORES (0/0/11), con los menores M-1 a M-11.

**Las dos coinciden en que el producto cumple el contrato del intento 2:**
- la tabla de `ConfirmBase` (en Conflict con MATCH sigue en Conflict);
- la atomicidad de «Conservar», con la admisión antes de `keepOn`;
- los pasos 1-8 de las tres operaciones del host, con la aplicabilidad antes de leer;
- los siete resultados de `Prepare` en orden, con Conflict antes de la admisión;
- el plan inmutable, que nunca borra una vista;
- los sobres compuestos sobre la relectura;
- los arrastres (1), (3) y (6);
- R-3;
- cero escrituras authored, y ningún rodeo de guarda en el producto;
- el GREEN no toca `tests/`.

## 2. Clasificación de los menores

| Id | Clase | Contenido | Destino |
|---|---|---|---|
| M-1 | Guarda | La prueba del arrastre (1) también pasa con el código anterior: las dos formas son equivalentes para estados bien formados, porque el copiador es «igual para el comparador». El arrastre está implementado, pero ninguna prueba lo fija | Guarda de fuente sobre `ComposeForWrite`, con autoprueba en negativo, en la primera tarea que toque el materializador (F4-T4B como muy tarde) |
| M-2 | Guarda | La guarda antialias no detecta `var p = pilot; p.Read…`, `pilot?.Read` ni `pilot!.Read`, y su autoprueba cubre por `.StaleBase` un caso que no es el del mecanismo antialias | F4-T4A2 o F4-T4B, la primera que toque `WorkspaceEditIntentGuardTests.cs` |
| M-3 | Guarda | La exención del diagnóstico de admisión es global y solo se cuenta en cinco cuerpos | Ídem, con la cuenta total en el núcleo |
| M-4 | Guarda | La guarda del invariante de R-3 en el núcleo no ve `this.ErrorReason =` ni `??=` | La primera tarea que toque `WorkspacePilotPrepareGuardTests.cs` |
| M-5 | Texto | Un descriptor que no se decodifica se notifica como «vista ya no disponible» | F4-T4B, que es quien usa el plan |
| M-6 | Orden | Un catálogo nulo se evalúa antes que el Conflict | F4-T4B: el llamador siempre da el catálogo. Se anota |
| M-7 | Declaración | El plan guarda `ExpectedVariableRegistryText`, que el contrato no enumera, como parte de la base esperada | Se declara aquí como ampliación aceptada: es parte de la StaleBase que T4B compara en MUTATE |
| M-8 | Documentación | La documentación de `EditConfirmStatus.Waiting` no coincide con el host, y un Applying durante la lectura se trata de forma distinta | F4-T4A2 |
| M-9 | Doble | `Reads()` del doble de la UI no cuenta las tres lecturas nuevas | F4-T4A2, que usa el doble |
| M-10 | Documentación | La atomicidad de la sobrecarga supone que `keepOn` no muta su segundo argumento | Se documenta en la primera tarea que toque `DraftSession.cs` |
| M-11 | RED | `From`, las igualdades del plan y los reenvíos ya eran definitivos en el esqueleto del RED | Desviación menor aceptada: no cambia el comportamiento existente. En tareas futuras, cuerpos inertes también en esos miembros |
| D-1 | Guarda | La guarda antialias fija con igualdad exacta 6 lecturas en el host | Cambio declarado previsto en F4-T4B y F4-T4C si añaden una lectura |
| D-2 | Calidad | Los hash del plan solo mezclan cuentas | Se anota |
| D-3 | Calidad | `UnreadableOriginReason` es una rama defensiva inalcanzable | Se anota |

## 3. Disposición (decisión del Owner)

`F4-DRAFTS/coordinator/owner-f4t4a1-close-decision.md`, respuesta literal: «Cerrar y arrastrar (Recommended)».
- F4-T4A1 queda CLOSED / PASS WITH LIMITATIONS sobre `bbd9a35e`, en el intento 2 de 3.
- Los 14 menores pasan a cambios declarados del paquete de F4-T4A2, o de F4-T4B si es esta la primera que toca la superficie (§2).
- M-1 a M-4 se corrigen allí.

## 4. Controles

Sobre las entradas de `623b`, con su oráculo ordinario y conservando cada salida ordinaria:
- **nc1** `86d0`: NOT PASSED, con la firma exacta de PD-F4-04. Su sustituto `F4T4A1-NC1-IDENTITY-SIGNAL-01` (`578c`) da PASS en 7/7.
- **nc2** `a383`: NOT PASSED, con la firma exacta de PD-F4-01. Su sustituto `F4T4A1-NC2-EXACT-WHITELIST-01` (`e01a`) da PASS en 10/10.
- **nc3** `d471`: CONTROL PASS.

Las dos reglas de sustitución las fija de antemano la ExpectedEvidence del contrato de F4-T4A1.
