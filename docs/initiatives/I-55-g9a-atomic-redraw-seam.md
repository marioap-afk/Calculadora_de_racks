# I-55 — G9a Atomic sibling redraw seam

Fecha: 2026-09-22

Gate: G9a

`G9A_START_SHA`: `f90b679dcdde1dd9d6f74ff7786c9a45dcbce9fc`

SHA de producto: `f70105a5715707943439db0ea63d342e2001beb6`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-52 observado: `faaf709bf6401dcda91b07f41afe44f490fb916a`

Proposal V5: gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Pertenencia unica y snapshot reusable

`RackSiblingMembership.Classify` es la unica funcion de pertenencia. Recibe la proyeccion del barrido unico de
AUTH-06 y conserva esos facts en el resultado; G9b podra usar el mismo snapshot sin volver a escanear. Clasifica
`Redraw`, `Erase`, `ReadOnly`, `NotMember` y `BlockingUnreadable`.

La fuente elegida participa aun con `Id` vacio o de espacios. Una definicion interpretable y no dependiente de xref
con el `RackId` curado es mutable. Para Selectivo/Cabecera, el Id original de espacios puede formar parte de los Ids
atribuibles; el Id vacio nunca se agrega. Una dependiente de xref atribuible es `ReadOnly`. Un sobre ilegible solo
bloquea cuando su `ProbeId` es atribuible; otro `ProbeId`, ninguno o una dependiente ilegible no atribuible quedan
fuera.

`MutableMembers`, `CustomPropertiesGateMembers` y `AuthoredGateMembers` son el mismo conjunto materializado. Asi los
gates futuros no necesitan otra agrupacion ni otra policy.

## 2. Supervivientes y plan puro

Un superviviente es exclusivamente un miembro `Redraw` con `LayoutReferenceCount >= 1`. Referencias anidadas no
cuentan. Una definicion existente sin referencia directa cuyo propietario sea un layout no es superviviente.

`RackSiblingRedrawPlan<TUnit>` separa unidades de redibujo y borrado, read-only, diagnosticos bloqueantes y
supervivientes. Si hay bloqueantes, no permite mutacion. Si `Survivors` esta vacio y existen unidades `Erase`, produce
`DeferToFirstPlacement`: G9a no borra la unica identidad y G9b debera incluir esas unidades en la transaccion de la
primera colocacion. Si no hay borrados, las unidades sin referencias pueden mutarse sin proteger una identidad.

## 3. Prepare, Mutate y Post

`ISiblingRedrawPort<TUnit>` fija tres fases:

1. `Prepare` produce todas las unidades y sus layers requeridas antes de mutar. La implementacion futura entrega a
   cada unidad el `RackPreparedProductView<TPayload>` ya producido por G7/Foundation; el seam no llama Resolve, no
   replica builders y no interpreta payloads. Imports permitidos por Proposal ocurren en esta fase y no se prometen
   reversibles.
2. `Mutate` recibe la coleccion completa y devuelve `Committed` o `Discarded`, nunca un bool.
3. `Post` recibe el plan y el resultado confirmado. Puede purgar anidadas obsoletas, aplicar cosmetica, reportar y
   hacer un unico regen; no contiene writes authored esenciales.

Una preparacion fallida o cualquier layer requerida bloqueada produce `PrepareFailed` con vista/definition y layer;
`Mutate` queda en cero llamadas. Las layers representan por separado referencias directas afectadas y entidades
existentes dentro de la definicion. Prepare no escribe rack state.

## 4. Propiedad de transaccion y rollback

`SiblingRedrawTransaction.Mutate` posee por completo una unica `Transaction` de AutoCAD para N unidades. Las unidades
reciben esa transaccion ya abierta, no abren ni confirman otra, y contienen solamente writes previamente preparados.
Exito de todas las unidades produce un solo `Commit`. Fallo tipado, unidad nula o excepcion retorna `Discarded` y el
`using` dispone la transaccion sin commit.

La prueba in-memory aplica la unidad 1 a un estado staged, falla la unidad 2 y demuestra que el estado committed sigue
vacio. El orquestador no invoca Post despues de `Discarded`; lo invoca una vez despues de `Committed`. El adaptador no
usa `TopTransaction`, `StartOpenCloseTransaction` ni `LockDocument` dentro de Mutate. El lock de la operacion completa
pertenecera al caller de G9b.

## 5. Inyeccion Debug y limites

`SiblingRedrawDebugFaultInjection` y su llamada existen enteramente bajo `#if DEBUG`. La variable
`RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT=N` lanza antes de la unidad N; el build Release no contiene esa cadena ni el
punto de inyeccion. La validacion fisica OV-RED-06 pertenece al flujo cableado posterior.

G9a permanece sin conectar a `RACKEDITAR Insertar` ni a comandos Selective, Dynamic, Push Back, Cantilever o Header.
No coloca vistas, no escribe `RackId`, no implementa ID17/ID18/ID19, no implementa AUTH-15 y no cambia G8.

## 6. RED -> GREEN

El primer T0 selecciono 17 casos: 14 fallaron por el seam deliberadamente incompleto y 3 guardas ya eran verdaderas.
Despues de implementar y ampliar los oraculos:

```text
RED T0 inicial = 17 seleccionadas / 14 FAIL / 3 PASS
GREEN T0 final = 22/22 PASS
T1 guards = 51/51 PASS
G8 impact = 22/22 PASS
G7 impact = 23/23 PASS
Foundation impact = 86/86 PASS
G3-G6 impact = 56/56 PASS
```

La matriz cubre A-R: pertenencia directa, xref, ilegibles atribuibles y no atribuibles, supervivientes por layout,
layer bloqueada, Prepare-all, una llamada a Mutate, un commit, excepcion y fallo tipado con rollback, Post solo tras
commit, un regen, fault seam Debug, conjunto reusable y el estado solo-huerfanas.

## 7. Archivos y evidencia exacta

Producto:

```text
src/RackCad.Application/Views/Redraw/ISiblingRedrawPort.cs
src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs
src/RackCad.Application/Views/Redraw/RackSiblingRedrawPlan.cs
src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs
src/RackCad.Plugin/Systems/Shared/SiblingRedrawDebugFaultInjection.cs
src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs
src/RackCad.Plugin/Systems/Shared/SiblingRedrawUnits.cs
```

Pruebas:

```text
tests/RackCad.Tests/RackSiblingRedrawTests.cs
tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs
tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs
```

| Evidencia sobre `f70105a5715707943439db0ea63d342e2001beb6` | Resultado |
|---|---|
| Core Full local | 8364/8364 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| Build Plugin Release + ausencia del sentinel Debug | PASS |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35753152868` / 4/4 SUCCESS |

No se agregaron skips. Owner Validation no es requerida porque el seam no esta cableado a ningun flujo visible.

## 8. Cierre

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
AUTH-15 = NOT IMPLEMENTED
Foundation diff = NONE
Schema diff = NONE
Owner Validation = NOT REQUIRED FOR THIS GATE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G9a = COMPLETE
G9b = OPEN
```
