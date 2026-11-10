# Análisis de la entrega del intento 2 de F4-T1-DRAFT-CORE

## Hechos

- **Entrega:** RED `500a336d` y GREEN `09955667`, sobre `90a81104`. Cambian nueve de los diez archivos del contrato (`DraftClosePolicy.cs` no cambia).
- **CI push del SHA exacto** (`42886956643`): los cuatro jobs en success. Filtro `FullyQualifiedName~RackCad.Tests.Workspace` 1273/1273; Core 16811/16811. El RED (`42886164382`) falla en 52 pruebas, todas dentro del filtro.
- **Verificación del Controller** `R20261110T194129Z-0d3b`: `EXECUTION_VERIFIED`, 14/14 en pass.
- **Controles nc1, nc2 y nc3:** CONTROL PASS, sin puentes.

## Revisión directa de la sesión

La corrección resuelve I-1 a I-4 tal como los fija el contrato:
- copiador inyectado;
- ranura de Aplicando propiedad de la operación;
- error diferido durante Aplicando;
- claves ambiguas que nunca coinciden;
- sin texto de sobre para Blocking.

Las seis pruebas retiradas son las que el contrato invierte (I-2, I-3, M-10) o las sustituye una versión más fuerte.

Observaciones, ninguna bloqueante:

| Id | Observación | Relación |
|---|---|---|
| DR2-01 | `NotApplying` informa `DraftWasRegistered` verdadero para un borrador ya desligado que no está aplicando | N-4 |
| DR2-02 | Un borrador desligado durante Aplicando sigue informando Aplicando después de `CompleteApply`. Es coherente con INV-F4T1-20 («no cambia nada más»), pero T3 no debe usar borradores desligados | N-4 |
| DR2-03 | Si el host nunca cierra un Actualizar, la ranura queda tomada para todo el proceso | obligación de T4B: `CompleteApply` en `finally` |

## Revisión adversarial independiente

Revisor independiente sin conclusiones previas. Exportó tres SHA con `git archive` a directorios temporales fuera del repositorio y corrió el filtro: 1273/1273 en el GREEN, y en el RED 52 fallas, todas por aserción. Reprodujo las secuencias con un programa de sondeo que no se versiona.

Los 13 hallazgos CORREGIR quedan corregidos en su caso original. La excepción es I-4, que queda parcial por N-1. El texto íntegro está en `independent-review.md`, junto a este archivo.

Hallazgos nuevos:

- **N-1 (BLOQUEANTE para el revisor; INV-F4T1-22).**
  - **Qué cambió:** la regla de «Bom» de `WorkspaceDraftBoundaryTests` pasó de `\bBom` y `Bom\b`, sin distinguir mayúsculas, a `(?-i:Bom(?![a-z])|\bbom(?![a-z])|_bom(?![a-z]))`, que sí las distingue. Ya no detecta `BOM` en mayúsculas, la grafía habitual del repositorio. El contrato del intento 2 dice: «Ninguna regla existente de la guarda se debilita».
  - **Estado del producto:** cumple.
  - **Cobertura que queda:** la guarda recursiva del namespace padre (`WorkspaceBridgeSourceGuardTests`, `\bBom` sin distinguir mayúsculas) sigue detectando en el texto de Drafts una palabra que empiece por BOM. Queda sin detectar un nombre de miembro que termine en BOM (por ejemplo `DraftBOM`).
- **N-2 (IMPORTANTE; D-10, INV-F4T1-19).** `InitialDraftState` toma por referencia el objeto de la lectura (`DraftIntentRead.InitialState`), y ocho pruebas fijan esa identidad.
  - **Riesgo:** si el host conserva y edita en sitio el objeto que entregó en la lectura, cambia el inicial; un Actualizar posterior puede perder esa edición sin aviso.
  - **Letra de INV-F4T1-19:** se cumple, porque inicial y actual nunca son el mismo objeto.
  - **Corrección mínima:** copiar al recibir la lectura y cambiar esas pruebas a igualdad por el comparador. La alternativa es una obligación explícita de T2 y T3.
- **Menores:**
  - **N-3:** el núcleo no comprueba que el copiador devuelva otro objeto; un copiador superficial de `PushBackEditorState` compartiría sus listas.
  - **N-4:** estado e indicador de registro de un borrador desligado.
  - **N-5:** AppliedWithWarnings con la relectura de POST fallida pierde los avisos.
  - **N-6:** se comparten arrays con el resultado.
  - **N-7:** una clave ambigua no se puede observar.
  - **N-8:** quedan restos de detección de E/S y de estado estático en la guarda.
  - **N-9:** faltan escenarios de prueba (claves no ASCII y combinaciones del error diferido).
  - **N-10:** un copiador que lanza deja el error diferido sin aplicar, y un estado inicial nulo lanza dentro del copiador.

## Disposición

Pendiente del Owner (F4-E08). N-1 incumple el texto del contrato. La regla de F4-E02 manda corregir gastando un intento, pero el que queda es el último de F4.

| Opción | Qué supone |
|---|---|
| **(A)** | Corrección del intento 3 en F4-T1, con N-1 a N-10. F4 queda sin intentos para T2, T3, T4A, T4B y Smoke-2. |
| **(B)** | F4-T1 se cierra con limitaciones. El contrato de F4-T2 incluye `WorkspaceDraftBoundaryTests.cs` y `DraftSession.cs`, con N-1 y N-2 como correcciones obligatorias y sus pruebas. N-3 a N-10 pasan a obligaciones de T2, T3 y T4. El intento 3 queda para T2 a T4. |

## Obligaciones que pasan a tareas posteriores, cualquiera que sea la opción

- **T2:**
  - el lector no conserva ni edita el objeto que entrega en una `DraftIntentRead`, mientras N-2 no se corrija en el núcleo;
  - el copiador del piloto es profundo (N-3);
  - el lector emite una sola entrada por clave (N-7);
  - el veto en el host se inclina a Veto ante una excepción (M-6);
  - guarda de M-9.
- **T3:** la interfaz no usa borradores desligados (DR2-02, N-4).
- **T4B:**
  - `CompleteApply` en `finally` (DR2-03);
  - los avisos de POST siguen visibles aunque su relectura falle (N-5).

## Límites

Sin AutoCAD. Este análisis no cambia la verificación `R20261110T194129Z-0d3b` ni los controles nc1..nc3.
