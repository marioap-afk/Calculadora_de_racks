---
schema: rackcad-initiative/v1
id: I-40
title: Edicion de cabeceras de Push Back
type: feature
status: implementing   # ronda 2 del Owner corregida; esperando nueva validacion manual
branch: feature/cabeceras-push-back
base_branch: main
priority:
size:
depends_on: [I-15, I-17, I-18, I-30, I-32, I-33, I-34, I-35, I-39]
conflicts_with: []
context_packs: [system-dynamic-flowbed, ui-editors, persistence, delivery-validation]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
---

# Edicion de cabeceras de Push Back

> **Gate documental abierto.** I-40 **no tiene fila en `docs/ROADMAP.md`**; la apertura la autorizo el
> Owner por instruccion directa, igual que ocurrio con I-35. La fila y el estado en HANDOFF los
> escribe la **sesion de integracion**, como ultimo commit de esta rama (WORKFLOW 4.5.4 y 8). Esta
> sesion no toca ninguno de los dos.

> **Los IDs de requisito son NUEVOS a proposito.** El encargo del Owner llego rotulado como
> «PB-011», «PB-007» y «PB-006», pero esos tres identificadores historicos ya estan **cerrados con
> otro contenido**: PB-011 = editor avanzado de modulos (I-35), PB-007 = edicion masiva de matrices
> de seguridad (I-34) y PB-006 = «Compartido»/«Lado» fuera del dialogo del tope (I-32). Reutilizarlos
> haria ilegible el historial, asi que esta iniciativa usa **PBH-01/02/03** y deja escrita la
> correspondencia:
>
> | Rotulo del encargo | ID de I-40 | Que es |
> |---|---|---|
> | «PB-011» | **PBH-01** | La cabecera personalizada, incluida su ALTURA, es la autoridad efectiva |
> | «PB-007» | **PBH-02** | Aplicar la configuracion a esta cabecera o a todas las cabeceras |
> | «PB-006» | **PBH-03** | Reutilizar la configuracion de OTRA cabecera como COPIA independiente |

## 1. Causa raiz de PBH-01

`RackPushBackSystemWindow.ShowHeaderConfigurator` abria el configurador compartido sobre una COPIA y
**devolvia esa misma copia**, dando por supuesto que la ventana solo la muta en sitio.

No es cierto. `RackFrameConfiguratorViewModel` **reemplaza** su propiedad `Configuration` por un clon
nuevo en tres caminos, todos via `ReplaceConfigurationAndReload`:

| Camino | Gesto del usuario |
|---|---|
| `ApplySimpleConfiguration` | «Aplicar» de la configuracion rapida — **donde se fija una ALTURA propia** |
| `RestoreStandardConfiguration` | «Restaurar cabecera estandar» |
| `LoadProjectFrom` | Abrir un proyecto de cabecera |

Tras cualquiera de ellos, la instancia que Push Back entrego es un objeto **obsoleto** y toda la
edicion vive en otro. Medido sobre la ventana real: partiendo de 132" y generando la cabecera a 156",
`window.Configuration.Height` = **156** mientras la instancia entregada seguia en **132**
(`ReferenceEquals` = False).

**La hipotesis del coordinador remoto queda CONFIRMADA**, y con una precision que la formulacion
original no tenia: no es solo la generacion rapida —son tres caminos— y el que importa para la altura
personalizada es justamente `ApplySimpleConfiguration`.

**Donde NO estaba el defecto.** Toda la infraestructura posterior ya conservaba correctamente una
cabecera propia y **no se toco**: `RackModuleEditSession` (deep-copy canonico de I-17 y procedencia),
`RackModuleReconciliation` por `ModuleId + Kind` —que adapta `Depth` y peralte pero **no** la altura—,
`DynamicEditorDesignAssembler.UpdateHeaderHeightInPlace` (que ya salta las personalizadas),
`DynamicRackSystemResolver`, `DynamicRackSystemBuilder.Refresh`, la persistencia con su fallback
legacy y el BOM. El arreglo es **la frontera**, no el mecanismo.

## 2. Arquitectura de PBH-02 y PBH-03

Una sola operacion conceptual, `RackModuleEditSession.ApplyHeaderConfiguration`:

```
configuracion efectiva -> deep-copy -> validar TODOS los destinos
                       -> aplicar atomicamente a uno o varios ModuleId
                       -> un solo commit/recalculo
```

- **Atomica**: cada destino se valida antes de mover un byte del estado escenificado. Un destino malo
  deja la sesion intacta; no hay aplicacion parcial que deshacer.
- **Copias independientes**: cada destino recibe su propio `RackFrameProjectStore.DeepCopy` (I-17).
  Modificar un destino despues jamas alcanza al origen ni a los demas.
- **PBH-03** (`CopyHeaderConfiguration`) resuelve el origen dentro de la sesion y **delega**. Cero
  persistencia nueva: la copia viaja dentro del modulo que el diseno ya llevaba, y la prueba lo
  demuestra comparando el JSON serializado de un rack que llego por copia con el de un rack
  personalizado directamente — **identicos byte a byte**.
- **PBH-02** ofrece «Solo esta cabecera» / «Todas las cabeceras». Las cabeceras que ningun corte
  dibuja se excluyen POR DENTRO —la misma compuerta de I-33 que la edicion por modulo ya aplicaba— y la
  linea de estado informa la omision (redaccion fijada en la ronda 2; ver 4-bis).
- `SetHeaderConfiguration` pasa a ser el caso de un destino de esa misma operacion, con el mismo
  comportamiento observable de antes.

**`RackModuleHeaderScope` es un tipo nuevo a proposito.** `SelectiveApplyScope`,
`DynamicRackCellScope` y `SelectionMatrixScope` direccionan una CELDA de una matriz frente x nivel (o
poste x nivel), y un modulo no tiene esos ejes: los modulos son **UNA secuencia longitudinal** (I-35).
Reutilizar un alcance de matriz obligaria a inventar un modulo por frente o por poste que el modelo no
tiene. Lo que se reutiliza es **la forma de la interaccion** —«Aplicar a:» + validar-todo-y-aplicar—,
no el tipo.

## 3. Hallazgo adyacente que SI bloqueaba

Los descriptores de la sesion se numeraban **todos como 1**: una intencion no lleva `Index` propio y
`RackModuleDescriptor.Describe(designs)` no se lo daba. De esa numeracion salen las etiquetas del
selector de modulos y de la lista «Copiar de:»; con todas iguales **no hay forma de elegir un
origen**, asi que PBH-03 seria inusable. Corregido (numeracion por posicion), con regresion propia.
Corrige de paso la numeracion del selector de modulos que I-35 dejo, visible pero sin prueba.

## 4. Fuera de alcance (no tocado)

Fondos por celda/nivel/frente, tarimas, cotas, Selectivo, edicion masiva general, dependencias entre
racks, biblioteca persistente de cabeceras, entidades persistentes nuevas, GUID, formato de alambre,
Dinamico, Cantilever, Cama, cabecera independiente, catalogos, bloques, comandos/aliases, Plugin y
dependencias NuGet. **Ningun archivo de Plugin, Domain, `assets/`, `deploy/` ni `.github/` cambia**, y
el contrato WPF de I-39 y `RackEditorVisualShell` quedan intactos: no hay infraestructura de shell
nueva ni logica de Push Back dentro del shell comun.

El configurador compartido **no se modifica** (decision 5 del Owner en I-35): sigue sin
Aceptar/Cancelar y sin `DialogResult`; Confirmar/Cancelar siguen viviendo en la sesion de Push Back.
La guarda `Fact7` de I-35 sigue verde.

## 4-bis. Ronda 2 del Owner — el candidato `e31902b` fue RECHAZADO

El Owner observo: la altura cambiaba y la geometria era correcta, pero la cabecera **no quedaba
conservada como personalizada**; al reabrir «Configurar cabecera...» aparecia la **predeterminada**, y
copiar la Cabecera 1 y aplicar a todas dejaba **todas en predeterminada**.

### La causa raiz REAL (medida, no supuesta)

Se audito el ciclo completo de estado en sus doce puntos —tras `StageHeaderConfiguration`, tras
`Commit`, `PendingModuleCommit`, ensamblador y reconciliacion, `design.Structure.Modules`, el
`AssociatedFrameConfiguration` resuelto, `UseCalculatedHeaderConfiguration`, `Snapshot`,
`AcceptComputation`, `ReseedModuleSession`, `HeaderConfigurationCopy` al reabrir, y la
serializacion/deserializacion— **incluidos recalculos ajenos** (agregar un frente, cambiar niveles).
**Ninguna frontera pierde nada**: en los doce puntos la cabecera llega completa con
`UseCalculatedHeaderConfiguration = false`. Ese diagnostico esta fijado por pruebas.

La perdida estaba **dentro del configurador compartido**, y es anterior a I-40:

> `RackFrameConfiguratorViewModel.ApplySimpleConfiguration` —el boton «Aplicar» del modo
> **«Configuracion rapida»**, que es el modo en el que el configurador **siempre** arranca— no edita la
> cabecera: la **RECONSTRUYE desde la plantilla**. Conserva solo alto, fondo, poste, peralte y nombre;
> `PanelClear`, horizontales, paneles, placas y excepciones vuelven al estandar.

Medido: `PanelClear` 41.5 → 44, horizontales 5 → 6, elevacion 33 → 4, con el alto correcto en 187.
De ahi exactamente los sintomas: **el alto funciona** (por eso la geometria cambiaba) y **todo lo demas
queda predeterminado**. Y como la Cabecera 1 quedaba asi, copiarla propagaba una configuracion
estandar a todas.

Antes de I-40 esto era inocuo: ninguna cabecera de Push Back era personalizada, asi que no habia nada
que reconstruir. La funcion nueva —seguir editando una cabecera propia— es la que lo vuelve destructivo.

### Correcciones (todas del lado de Push Back; el configurador compartido NO se toca)

| Defecto | Correccion |
|---|---|
| 1 | Una cabecera **ya personalizada** se reabre en el **editor avanzado** (`IsAdvancedEditor`, propiedad publica del ViewModel). Se EDITA de forma incremental en vez de ofrecer por delante un «Aplicar» que la regenera. Una cabecera **calculada** se sigue abriendo en modo rapido: ahi generar ES lo que se quiere |
| 2 | Como **origen** de una copia solo se ofrecen cabeceras **personalizadas** (opcion A del Owner). Sin ninguna, el control se deshabilita y dice por que. El camino de copia **ya no tiene fallback** al ultimo sistema resuelto: no puede propagar una configuracion recalculada |
| 3 | La secuencia exacta del reporte queda cubierta por una regresion extremo a extremo, incluida la reapertura de cada cabecera y el round-trip de persistencia |
| 4 | «Solo esta cabecera» / «Todas las cabeceras» — sin el termino arquitectonico «aplicables». Las cabeceras que ningun corte dibuja (I-33) se excluyen **por dentro** y la linea de estado dice **cuantas** cambiaron, **cuales** y **cuantas se omitieron y por que**. La pantalla separa las tres operaciones: **Configuracion**, **Reutilizar**, **Alcance** |

**Ademas**, las cabeceras se nombran como el usuario las cuenta —«Cabecera 1», «Cabecera 2»— y no por
su posicion en la secuencia de modulos, que intercala separadores y hacia que las cabeceras de un rack
real se llamaran 1, 3, 6 y 8.

**Limitacion declarada, no resuelta.** Si el usuario cambia a «Configuracion rapida» dentro del
configurador y pulsa «Aplicar» sobre una cabecera personalizada, la regeneracion desde la plantilla
ocurre igual: es el comportamiento historico de una ventana **compartida** con el Dinamico y la
cabecera independiente, y cambiarlo esta fuera del alcance de I-40. Lo que I-40 corrige es que ese ya
no sea el camino que se le pone delante.

## 5. Checklist de validacion manual en AutoCAD 2025

DLL a cargar con `NETLOAD` (el del worktree de la iniciativa, WORKFLOW seccion 6):

```
C:\Users\<usuario>\.claude\worktrees\feature-cabeceras-push-back\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

Cerrar AutoCAD antes de cualquier recompilacion del worktree.

### PBH-01 — la cabecera personalizada manda

1. `RACKPUSHBACK`: crear un rack con **varias cabeceras** (fondo suficiente para 3 o mas modulos).
2. «Editar modulos» -> seleccionar una cabecera -> «Configurar cabecera...».
3. En el configurador, **cambiar la ALTURA** por la pestana de configuracion rapida y pulsar
   «Aplicar». Cerrar el configurador.
4. En Push Back, comprobar que la cabecera queda marcada **«Personalizada»**, y pulsar **Confirmar**.
5. Verificar en el dibujo que **la geometria cambio**: esa cabecera es mas alta que las demas en la
   vista lateral, y el BOM la refleja.
6. Guardar, **cerrar y reabrir** el dibujo; `RACKEDITAR` sobre el rack: la altura personalizada sigue
   ahi y la cabecera sigue marcada «Personalizada».

### PBH-02 — alcance

7. Configurar una cabecera (altura distinta a la calculada).
8. Poner **«Aplicar a:» = Todas las cabeceras aplicables** y repetir «Configurar cabecera...».
   Confirmar **una sola vez**.
9. Verificar que **todas** las cabeceras del rack quedaron con esa configuracion, en el dibujo y en el
   BOM.
10. Verificar que **ningun separador** cambio, y que un rack con un frente en blanco no altera las
    cabeceras que I-33 no dibuja.

### PBH-03 — copia independiente

11. Configurar la **Cabecera 1** con una altura reconocible.
12. Seleccionar la **Cabecera 2**, elegirla en «Copiar de:» -> «Copiar configuracion». Confirmar.
13. Modificar la **Cabecera 2** (otra altura) y Confirmar.
14. Verificar que la **Cabecera 1 sigue con su altura original**: la copia era independiente.

### Transversal

15. **Cancelar** tras una aplicacion a todas: el rack vuelve a como estaba.
16. El **GUID** del rack no cambia en ninguno de los pasos anteriores (`RACKEDITAR` lo sigue
    encontrando).

## 6. Checklist REDUCIDO de la ronda 2 (repetir solo lo que fallo)

1. Personalizar una cabecera (cambiar el alto) y **Confirmar**. La geometria cambia.
2. **Volver a abrir «Configurar cabecera...» sobre esa misma cabecera**: debe abrir en **«Editor
   avanzado»** y mostrar la configuracion personalizada, no la predeterminada.
3. Cambiar ahi **otra** propiedad (p. ej. un panel o una horizontal) y Confirmar: el alto anterior
   **sigue**. Se puede editar de forma incremental.
4. Con solo esa cabecera personalizada, seleccionar otra: «Copiar configuracion de:» ofrece
   **unicamente** «Cabecera 1». Con ninguna personalizada, el control esta **deshabilitado** y explica
   por que.
5. Copiar Cabecera 1 sobre la seleccionada con **«Solo esta cabecera»**: recibe los valores reales de
   la 1.
6. Repetir con **«Todas las cabeceras»** y Confirmar UNA vez: **todas** quedan con la configuracion de
   la Cabecera 1, ninguna en predeterminada, y la linea de estado dice **cuantas** cambiaron.
7. Reabrir cada cabecera: todas muestran esos mismos valores.
8. Modificar una de ellas: las demas **no** cambian.
9. Repetir la operacion multiple y **Cancelar**: ninguna cabecera queda alterada.
10. Guardar, cerrar y reabrir el dibujo: todo lo anterior sigue igual y el **GUID** no cambio.
