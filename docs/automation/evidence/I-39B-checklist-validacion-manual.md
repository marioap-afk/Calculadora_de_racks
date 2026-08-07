# I-39B — Checklist de validación manual en AutoCAD 2025 (Owner)

> Estado: **APROBADA**. Registro factual del resultado proporcionado por el dueño; no incluye capturas ni detalles no proporcionados.
> Contrato: [`../../initiatives/I-39B-interaccion-editores-ricos.md`](../../initiatives/I-39B-interaccion-editores-ricos.md) ·
> Auditoría: [`I-39B-auditoria-editores-ricos.md`](I-39B-auditoria-editores-ricos.md) ·
> Estado: [`../state/I-39B.yml`](../state/I-39B.yml)

## Resultado — EJECUTADA Y APROBADA (2026-08-07)

**`OWNER_APPROVED_I39B_MANUAL_VALIDATION`.** El Owner validó en **AutoCAD 2025** el checklist completo
de **31 puntos** sobre el DLL Debug del SHA candidato, y **todos se cumplen**. Sin observaciones ni
defectos bloqueantes.

| Campo | Valor |
|---|---|
| **SHA que recibió el veredicto** | `5755845051a5f10bd06367f1f97aed42e180dc9a` |
| SHA-256 del DLL | `CF718FE661536BBFA5683FC94E3D0DDBBD492D1CABAE5EE9953552C127A4CECC` |
| `AssemblyInformationalVersion` | `1.0.0+5755845051a5f10bd06367f1f97aed42e180dc9a` |
| CI del candidato | run `31214414417`, 4/4 |
| Base | `origin/main` `3853cd4` — **no avanzó**, así que **no hubo rebase** y el árbol validado es el integrado |
| Validador | dueño del repositorio |
| Resultado global | **aprobado** |

El commit de cierre documental posterior a `5755845` **no cambia `src/` ni `tests/`**, de modo que no
altera el binario validado (mismo criterio que I-31, I-35 e I-39A).

## Artefacto validado

| Campo | Valor |
|---|---|
| Iniciativa | I-39B — Adopción del contrato funcional común en los seis editores ricos |
| Rama | `architecture/interaccion-editores-ricos` |
| Claim-Id | `c752689d-714a-450b-a90e-53ff2729bc40` |
| DLL a cargar | `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| Worktree | `.claude\worktrees\architecture-interaccion-editores-ricos` |

**El DLL es el del worktree de la iniciativa, no el del principal.** Cierra AutoCAD antes de recompilar.

## Qué cambió, y qué no

**Cambió, y es lo que hay que validar:**

1. **Cerrar deja de perder trabajo en silencio.** Las cuatro rutas —botón, Escape, la X y `Alt+F4`— pasan
   ahora por un único punto en las dos ventanas que **declaran** un ámbito transaccional. Las otras
   cuatro no declaran ninguno y cierran directo, igual que antes.
2. **Push Back se cierra con Escape**, cosa que antes no hacía: era la única de las seis sin `IsCancel`.
   Se le añadió **después** de la política, no antes.
3. **El Dinámico dice cuándo su vista previa está obsoleta** y apaga las cinco acciones de dibujo
   mientras lo esté, con el motivo en el tooltip. Antes seguían habilitadas y el error llegaba al pulsar.
4. **La Cama apaga «Insertar en AutoCAD» cuando la captura es inválida**, con su motivo. Antes seguía
   habilitado y el error llegaba al pulsar.

**No cambió:** geometría, BOM, persistencia, wire format, GUID, catálogos, bloques DWG,
materialización, ni ninguna regla de producto. Tampoco el contrato de inserción de la Cabecera.

## Checklist

### Configurador de cabecera

| # | Punto | Resultado |
|---|---|---|
| 1 | Con ediciones manuales o excepciones, **Escape** pregunta antes de cerrar | APROBADO |
| 2 | Lo mismo con la **X** de la ventana | APROBADO |
| 3 | Lo mismo con **`Alt+F4`** | APROBADO |
| 4 | Lo mismo con el botón **Cerrar** | APROBADO |
| 5 | **Confirmar** el descarte cierra la ventana y no dibuja nada | APROBADO |
| 6 | **Cancelar** el descarte deja la ventana abierta con todo intacto | APROBADO |
| 7 | **Sin** ediciones manuales, cerrar no pregunta nada | APROBADO |
| 8 | **Insertar** y **Actualizar** siguen cerrando sin preguntar, y dibujan lo mismo que antes | APROBADO |
| 9 | «Restaurar estándar» y «Generar cabecera» siguen preguntando igual que antes | APROBADO |

### Push Back

| # | Punto | Resultado |
|---|---|---|
| 10 | Con un cambio de módulo pendiente («Cambios pendientes: confirma o cancela»), **Escape** pregunta | APROBADO |
| 11 | Lo mismo con la **X** y con **`Alt+F4`** | APROBADO |
| 12 | Lo mismo con el botón **Cerrar** | APROBADO |
| 13 | **Confirmar** cierra; **Cancelar** deja la ventana abierta y el módulo escenificado intacto | APROBADO |
| 14 | **Sin** cambios de módulo pendientes, cerrar no pregunta | APROBADO |
| 15 | Insertar y Actualizar siguen cerrando sin preguntar y dibujan lo mismo | APROBADO |
| 16 | Confirmar y Cancelar del editor de módulo siguen funcionando igual | APROBADO |

### Dinámico

| # | Punto | Resultado |
|---|---|---|
| 17 | Con una captura inválida, la vista previa **se conserva** y el estado dice que corresponde al último cálculo válido | APROBADO |
| 18 | Las cinco acciones de dibujo quedan **apagadas**, y su tooltip explica por qué | APROBADO |
| 19 | Al corregir la captura y recalcular, vuelven a habilitarse y dibujan lo mismo que antes | APROBADO |

### Cama

| # | Punto | Resultado |
|---|---|---|
| 20 | Con una captura inválida, «Insertar en AutoCAD» queda **apagado** con su motivo visible | APROBADO |
| 21 | Al corregirla, vuelve a habilitarse e inserta igual que antes | APROBADO |

### Regresión

| # | Punto | Resultado |
|---|---|---|
| 22 | Los **seis** editores abren, calculan, dibujan y cierran con normalidad | APROBADO |
| 23 | Selectivo, Cantilever, Cama y Cabecera siguen cerrando **sin** diálogo cuando no hay nada pendiente | APROBADO |
| 24 | Geometría y BOM de los cinco sistemas sin cambios | APROBADO |

## Alcance que I-39B **no** cubrió

Todo el alcance interno quedó **resuelto o cerrado con una desviación justificada y medida**, registrada
en [`I-39B-decisiones-tecnicas.md`](I-39B-decisiones-tecnicas.md):

- **Cantilever**: RESUELTO. Los avisos no bloqueantes dejan de pintarse con el rojo de error.
- **`EditorStatusPalette`**: ADOPTADA, con consumidor productivo real. Ya no es cierto que la
  infraestructura de status no tenga ninguno.
- **`EditorAction` / `EditorActionBar`**: **no se adoptan**, con motivo funcional demostrado: no saben
  declarar acción por defecto ni cancelación, y sustituir los botones rompería el contrato de teclado
  que I-39B acaba de fijar. D6 queda cumplido por el mecanismo que ya usaban las ventanas.
- **Cama y Cabecera al shell**: **no se migran**, con desviación explícita. El mínimo del arquetipo A
  (`1120×672`) es mayor que el tamaño inicial completo de la Cama (`1080×640`), y la Cabecera perdería
  su layout de paneles persistido, que es una capacidad de producto.
- **Foco inicial de la Cabecera**: RESUELTO.

Siguen fuera por **exclusión previa y expresa**, no por diferimiento:

- **Contrato de inserción paralelo de la Cabecera**: ADR-0029 no autoriza tocar persistencia ni
  identidad, y proteger el cierre no lo requirió.
- **Defecto de reseed de Push Back**: pérdida silenciosa ajena al cierre, preexistente.
- **Merge incondicional del shell acotado** (arquetipo B, I-39A).

## Adenda — puntos añadidos al cerrar el alcance interno

### Cantilever

| # | Punto | Resultado |
|---|---|---|
| 25 | Una línea que resuelve **con avisos** muestra su texto en **ámbar**, no en el rojo de error | APROBADO |
| 26 | Un fallo real sigue en rojo, y una línea sin avisos sigue en verde | APROBADO |
| 27 | Los avisos no bloquean: insertar y actualizar siguen disponibles | APROBADO |

### Configurador de cabecera

| # | Punto | Resultado |
|---|---|---|
| 28 | Al abrir, el foco está en el **árbol del modelo**; escribir no altera ningún campo por accidente | APROBADO |
| 29 | Tabular desde ahí recorre la ventana en orden coherente | APROBADO |

### Cama y Cabecera — sin migración al shell

| # | Punto | Resultado |
|---|---|---|
| 30 | Ambas conservan **su tamaño, su fondo y su composición actuales**: no se migraron al shell, por la desviación registrada en `I-39B-decisiones-tecnicas.md` | APROBADO |
| 31 | La Cabecera conserva su **layout de paneles persistido** y «Restablecer paneles» sigue funcionando | APROBADO |
