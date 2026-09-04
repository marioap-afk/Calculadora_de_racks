# ADR-0032: El editor Selectivo separa valor pendiente de estado comprometido, y cada propiedad tiene una autoridad por fondo

- **Estado:** **aceptado**
- **Fecha:** 2026-09-03 (propuesto) · 2026-09-04 (aceptado)
- **Decisores:** dueño del repo (**acepta**); Arquitecto independiente de I-43 (plan v1.1 y segunda revisión
  del Gate 8.9); Coordinador de I-43 (revisión); Claude (redacción e implementación)
- **Iniciativa relacionada:** I-43 — `feature/selectivo-scopes-fondos`
  ([contrato](../initiatives/I-43-selectivo-scopes-fondos.md))

> **Aceptación del dueño (2026-09-04).** El dueño acepta esta decisión **con el contrato tal como quedó
> implementado**, tras validarlo manualmente en AutoCAD 2025 sobre el DLL construido exactamente desde
> `d582deed5bbd93083261399e45b2ecc3e16088d7` (SHA-256 del DLL
> `f70d89bffad38cf77fd8b5b51e2951512e34f2af5b7050392c590d8ff4a06d87`), con veredicto **PASS TOTAL**: los ejes
> independientes, la selección proyectada, la frontera pendiente/comprometido, la elevación y la cabecera por
> fondo, el frontal y la preview de cada fondo, y la compatibilidad legacy. La aceptación incluye los
> **follow-ups declarados en D14**, que siguen fuera de I-43. A partir de aquí el contenido de este ADR es
> **inmutable** ([adr/README.md](README.md)): solo pueden cambiar su Estado y sus enlaces.
>
> **Este ADR se redactó ANTES de tocar código productivo** (Gate 8.6A del plan de corrección
> post-Gate 8). Fija por escrito el contrato que los gates 8.6B–8.6G implementarán, de modo que la
> implementación se pueda contrastar contra una decisión declarada y no contra la lectura que cada
> agente haga del código.

## Contexto

I-43 dio al editor Selectivo **dos ejes de edición independientes**: *dónde* se escribe
(`TargetFondos`: el fondo actual, uno, varios o todos) y *qué alcance* tiene la escritura dentro de un
fondo (`Scope`: celda, nivel, frente, seleccionadas, todas). El Owner validó el resultado
funcionalmente en AutoCAD 2025 (Gate 8 PASS sobre `de100ed111d8551690f58fbcea2e4a29f0db5909`).

La revisión arquitectónica posterior (Gate 8.5) encontró que ese contrato **no está escrito en ningún
sitio** y que, en el código, está repartido entre las cajas de texto de WPF y el estado de la
aplicación, sin una frontera declarada entre ambos. Las consecuencias son observables:

- `BayCountBox`, `FondoBox`, `CabeceraFondoBox` y `FondosBox` **son a la vez editor y autoridad**. Un
  `LostFocus` sin edición real puede redimensionar o reprofundizar todos los fondos destino; una caja
  vaciada provoca un RESTORE masivo.
- `BuildDesign` lee texto de esas cajas (`ReadWorkingDepthCabecera`, `FondosBox.Text`), de modo que un
  valor **tecleado y no comprometido** puede llegar al documento o estampar el slot de un fondo que ni
  siquiera es destino.
- `FloorBeamRise` tiene tres redacciones incompatibles entre Domain, Application y los valores por
  defecto, y `MaterializeFloorBeamRises` deja el slot del fondo seleccionado en `null`.
- La cabecera custom es autoridad de `(fondo, poste)` desde Gate 4, pero `FondoSystemView` solo copia
  `PostCabeceras` cuando `k == 0`: el frontal de un fondo `k > 0` no muestra su propia cabecera.
- Comentarios y tooltips afirman contratos que dejaron de ser ciertos.

Nada de esto es un defecto de las decisiones de I-43: es que **el contrato nunca se declaró**. Sin un
registro, cada corrección vuelve a discutir qué es autoridad y qué es una caja de texto.

## Decisión

### D1 — La selección es 2D proyectada (O-43-01)

La selección de celdas son **posiciones lógicas 2D `(FrontIndex, LevelIndex)`** que se proyectan sobre
cada fondo de `TargetFondos`. Una posición que un fondo destino no tiene se **omite** (SKIP): nunca se
recorta a un índice vecino, nunca se rellena la matriz y nunca se crea una celda. La selección es
**transitoria**: no se persiste. Ya está implementada (`SelectiveMatrixPosition`,
`SelectiveTargetResolver.ResolveSelected`) y **no se toca**.

### D2 — Editar aplica; no editar no muta (O-43-02)

Para `Frentes`, `Fondo de tarima` y `Fondo de cabecera` se conserva el gesto **«el usuario EDITA →
`LostFocus` o Enter → aplica sobre `TargetFondos`»**. Un `LostFocus` o un Enter **sin edición real
produce mutación cero**. Retipear explícitamente el mismo valor visible **sí cuenta como edición**. No
se añaden botones «Aplicar» para estos campos en I-43.

### D3 — El frontal de cada fondo usa su propia cabecera (O-43-03)

Cada frontal `Fk` representa físicamente al fondo `k`: el frontal `Fk` y la preview del fondo `k` usan
la cabecera custom `(FondoIndex = k, PostIndex = i)` cuando existe y es usable. Se retira la asimetría
«custom solo en el frontal del fondo 0».

### D4 — `TargetFondos` es un eje independiente del `Scope`

`TargetFondos` (externo, qué fondos) y `Scope` (interno, qué alcance dentro de un fondo) **no se
fusionan**. Toda operación explícita de escritura es el producto **`Scope` × `TargetFondos`**. El
conjunto de destinos **nunca queda vacío**.

### D5 — Pendiente ≠ comprometido

- `BayCountBox`, `FondoBox`, `CabeceraFondoBox` y `FondosBox` son **editores de valores pendientes**.
  Su texto no es autoridad de nada.
- La **autoridad comprometida** es el estado: `FondoMatrices[k].Depth`,
  `FondoMatrices[k].CabeceraOverride`, `FondoMatrices.Count` y las matrices (`Bays`, `FloorBeams`,
  `BayHeights`, `BaySegments`, `FloorBeamRiseOverrides`), con la matriz **viva** como fondo
  seleccionado.
- **`BuildDesign` lee exclusivamente estado comprometido.** Ningún texto pendiente llega a un slot sin
  pasar por un commit.
- Una asignación **programática** a una de esas cajas no marca el campo como editado: es un `Show` del
  valor comprometido. Un `Show` sobre un campo con edición pendiente sin commit ni descarte explícito
  previo es un **defecto**.

El commit de la **matriz de trabajo** al slot del fondo seleccionado se conserva íntegro y sin cambio
de semántica; lo único que cambia es la **fuente** de `Depth`/`CabeceraOverride`, que pasa a ser el
propio slot en lugar de las cajas.

### D6 — El commit de pendientes es atómico, en dos fases y ordenado

Toda **frontera transaccional** (dibujar, actualizar, insertar, guardar en biblioteca, BOM,
personalizar o restablecer poste, elementos de seguridad, medio frente, las operaciones de alcance, el
gesto propio de cada campo y el cambio de fondo visible) compromete los campos pendientes así:

1. **Fase 1 — preparar.** Se validan **todos** los campos implicados que estén editados, sin mutar
   nada, acumulando errores. Si **alguno** es inválido, la acción se **aborta**: no se aplica ninguno,
   ningún slot, matriz, `FondoMatrices.Count` ni `TargetFondos` cambia, y el status nombra el campo
   inválido. El texto inválido **permanece en su caja** (no se auto-repara en una frontera).
2. **Fase 2 — aplicar.** Solo si todos son válidos: dentro de un único `DeferRecompute`, se aplica
   cada campo y se refresca su caja con el valor comprometido. **Una operación → un recompute.**

El **orden fijo** es `FondosBox` → `BayCountBox` → `FondoBox` → `CabeceraFondoBox`: la estructura
define la topología contra la que se resuelven los valores. **`TargetFondos` se re-resuelve después de
cada cambio estructural** («Todos» se re-expande, un conjunto explícito se poda, «Actual» sigue al
fondo visible), de modo que los valores ven el conjunto final. Un fondo creado por el commit es un
clon del estado **comprometido** del fondo 0 y solo recibe un valor pendiente si pertenece a ese
`TargetFondos` resuelto.

Un campo sin edición se salta en ambas fases: el commit es **idempotente**.

La **auto-reparación** de `BayCountBox` y `FondosBox` ante texto inválido (restaurar el valor
comprometido con aviso) sobrevive **solo en el gesto propio del campo y sin hermanos editados**: es un
descarte explícito, no un commit. Un gesto propio estructural que encuentra un hermano editado
inválido **aborta y lo nombra**, en lugar de descartarlo en silencio.

### D7 — Commit implícito de celda: solo la celda visible

Navegar entre celdas comete **únicamente la celda visible**; no es una frontera transaccional y no
proyecta nada sobre `TargetFondos`. Las escrituras multi-fondo ocurren **solo** por una operación
explícita comprometida.

### D8 — `FloorBeamRise` es directo por `(fondo, frente)`

La elevación del larguero a piso es una propiedad **directa** de cada `(FondoIndex, FrontIndex)`. Todo
frente tiene un valor tras crearse, cargarse o redimensionarse; `null` solo puede existir
transitoriamente al leer un documento legacy, antes de materializarlo. El valor **global** persistido
queda **exclusivamente como compatibilidad de lectura** (el resolver sigue haciendo
`override ?? global` para documentos antiguos): deja de ser una autoridad de escritura.

### D9 — Cabecera custom por `(fondo, poste)`: `Height` es de la receta, `Depth` del fondo

- La autoridad es la fila del fondo: `PostCabeceras` para el fondo 0, `ExtraFondoPostCabeceras[k-1]`
  para `k ≥ 1`. Cada destino recibe una **copia profunda independiente**: dos fondos nunca comparten
  instancia.
- **`Height` pertenece a la receta** que el usuario configuró: es idéntica en todos los destinos y no
  se ajusta por fondo. Cuando difiere de la altura resuelta de algún destino, se **avisa** — no se
  corrige.
- **`Depth` pertenece al fondo destino**, no a la receta: la impone la autoridad de profundidad del
  fondo (`CabeceraFondoOverride`, si no `tarima − 6"`). Una configuración almacenada **no** es una
  segunda autoridad de profundidad.
- Un destino donde ese poste no existe se **omite y se reporta**; nunca se crea.

### D10 — Autoridades (tabla vinculante)

| Propiedad | Autoridad comprometida | Editor pendiente | Escritura productiva |
|---|---|---|---|
| Frentes por fondo | `FondoMatrices[k].Bays.Count` (vivo: `state.Bays`) | `BayCountBox` | `ApplyBayCountToTargets` vía commit |
| Fondo de tarima | `FondoMatrices[k].Depth` | `FondoBox` | `ApplyPalletDepthToTargets` vía commit |
| Fondo de cabecera | `FondoMatrices[k].CabeceraOverride` (0 = derivado) | `CabeceraFondoBox` | `ApplyCabeceraDepthToTargets` vía commit |
| Número de fondos | `FondoMatrices.Count` | `FondosBox` | `ApplyFondoCountFromBox` vía commit |
| Elevación a piso | fila directa por `(k, f)` | `FrontRiseBox` («Aplicar elevación») | `ApplyFloorBeamRiseToTargets` |
| Cabecera custom | fila del fondo `k`, índice `i` | configurador | `ApplyCabeceraToTargets` |
| `Depth` de una custom | el fondo `k` — no la receta | — | imposición del fondo al escribir y al leer |
| `Height` de una custom | la receta (configurador) | configurador | `ApplyCabeceraToTargets` |
| Celda (7 campos) | `CellAt(fondo, frente, nivel)` | editor de celda | `ApplyToTargets` (explícito) / commit de celda visible (implícito) |
| `TargetFondos` / `TargetMode` | estado (nunca vacío) | popup | `FollowCurrentFondo` / `FollowAllFondos` / `SetTargetFondos`; re-resolución tras cada cambio estructural |

### D11 — Persistencia aditiva, sin cambio de esquema

No hay cambio de DTO, de `SchemaVersion` (sigue `1.0`) ni de stores. Los campos nuevos son
**aditivos y anulables**, con fallback legacy y test de round-trip. Lo único que cambia en lo
persistido es la **forma, no el significado**, de la elevación por frente: los documentos nuevos y los
reabiertos llevan valor explícito en todos los frentes. **No hay migración de datos.** Si la
implementación creyera necesitar un cambio de DTO o de versión, se detiene y lo reporta.

### D12 — La preferencia de `TargetFondos` es del editor, no del documento

La última elección de «Fondos destino» se recuerda entre aperturas como **preferencia de usuario**
(`UserSettings.SelectiveTargetFondos`, `%APPDATA%\RackCad\settings.json`). Se persiste la
**intención** («Todos» / «Actual» / un conjunto explícito), no el conjunto resuelto, y se re-resuelve
contra el rack que se abre. **Nunca entra al diseño ni al `.dwg`**: dos racks guardados con
preferencias distintas serializan idénticamente.

### D13 — Compatibilidad legacy

Los documentos anteriores a I-43 **dibujan igual**: el resolver mantiene el coalesce
`override ?? global`, las filas de cabeceras ausentes dan cabecera estándar y la vista de un fondo
`k > 0` sin datos queda vacía. Los documentos guardados por builds `de100ed` **sanan al abrirlos**, sin
migración. Un build anterior leyendo un documento nuevo se comporta como con `de100ed`.

### D14 — Lo que queda explícitamente fuera de I-43

Los follow-ups identificados en la revisión arquitectónica —purificación del lector efectivo, retirada
del global de Domain, extracción de la carga fuera de la ventana, unificación de resultados y
resolvers, limpieza de API legacy, seams de diálogo, política de esquema, un estado de frente
dedicado, el *pallet stop* por lado (SPLIT), los frentes en blanco, el aviso «tramos no caben» y el
resumen que describe el fondo 0— **no se implementan en I-43** y viven en
[`docs/ideas-futuras.md`](../ideas-futuras.md). Tocarlos «de paso» invalida el gate.

## Alternativas consideradas

- **Introducir un ViewModel o un estado de frente dedicado** para el editor Selectivo — resuelve la
  frontera pendiente/comprometido de raíz, pero es un refactor transversal de `RackSelectiveWindow`
  con superficie enorme, imposible de validar por gates pequeños y de revertir con `git revert`. Se
  descarta **para I-43** y queda como follow-up.
- **Añadir botones «Aplicar» a Frentes / Fondo de tarima / Fondo de cabecera**, en lugar de conservar
  el gesto de foco — haría trivial la frontera, pero el Owner decidió conservar el gesto actual
  (O-43-02). Descartada por decisión de producto, no por técnica.
- **Dejar que la configuración custom lleve su propia profundidad** — es lo que haría un editor de
  recetas genérico, pero crea una **segunda autoridad** de profundidad que se contradice con la del
  fondo en cuanto la misma receta se aplica a fondos distintos, que es exactamente lo que hace el
  apply multi-fondo. Descartada.
- **Migrar los documentos para materializar la elevación por frente** — innecesaria: el coalesce del
  resolver ya produce el mismo dibujo y los documentos sanan al abrirse. Una migración cambiaría
  archivos del usuario sin ganancia observable. Descartada.
- **Guardar la preferencia de destinos dentro del diseño** — haría que un dibujo cambiara según quién
  lo abrió y que dos racks discreparan sobre un valor que describe al editor. Descartada.
- **Commit campo a campo en lugar de atómico** — es lo que hace hoy el código; deja el estado a medio
  aplicar cuando un campo posterior es inválido. Descartada a favor de validar todo antes de mutar.

## Consecuencias

- **Positivas:** la frontera entre texto en pantalla y estado queda declarada, de modo que un
  `LostFocus` accidental deja de poder reescribir todos los fondos; el documento no puede recibir un
  valor que el usuario no comprometió; el frontal de cada fondo muestra su propia cabecera; la
  elevación por frente deja de depender de un global ambiguo; el contrato se puede contrastar contra
  este registro en vez de re-derivarlo del código.
- **Negativas / costos aceptados:** aparece un helper de campo pendiente y una operación de commit que
  todas las fronteras deben invocar — un olvido en una frontera nueva reintroduce el problema, y por
  eso el contrato exige una tabla explícita de fronteras y aserciones sobre el `Show`. El orden fijo de
  aplicación es una convención que hay que respetar al añadir un campo pendiente. La elevación por
  frente pasa a escribirse explícitamente en todos los documentos nuevos (más bytes, mismo
  significado). El gesto sigue siendo «foco», con su ambigüedad inherente, porque el Owner lo decidió
  así.
- **Qué vigilar:** que ninguna asignación programática a las cuatro cajas marque el campo como editado;
  que `BuildDesign` no vuelva a leer texto; que la imposición de profundidad no se convierta en una
  mutación durante una lectura; y que la preferencia de destinos no se cuele en el DTO.

## Referencias

- Plan de corrección arquitectónica post-Gate 8 de I-43, versión 1.1 (Arquitecto independiente,
  2026-09-03): decisiones O-43-01/02/03, autoridades, invariantes INV-01…INV-17, plan por gates
  8.6A–8.6G y follow-ups excluidos.
- [Contrato de la iniciativa I-43](../initiatives/I-43-selectivo-scopes-fondos.md).
- [`docs/ideas-futuras.md`](../ideas-futuras.md) — follow-ups fuera de alcance.
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md) — contrato funcional común de ventanas
  WPF (transacción, acciones y motivos de bloqueo), del que esta decisión es la especialización del
  Selectivo para el par pendiente/comprometido.
- [ADR-0030](0030-fondo-por-celda-push-back-y-envolvente-derivada.md) — precedente de «la propiedad
  pertenece a la celda; lo del frente es derivado», aquí aplicado a `Depth` de una cabecera.
- `de100ed111d8551690f58fbcea2e4a29f0db5909` — candidato con Gate 8 PASS funcional del Owner, sobre el
  que se hizo la revisión arquitectónica y al que se refieren todas las citas del plan.
