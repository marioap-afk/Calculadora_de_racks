---
schema: rackcad-initiative/v1
id: I-40
title: Edicion de cabeceras de Push Back
type: feature
status: integrated
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
En los doce puntos —**dentro de una misma sesion y pulsando «Confirmar»**— la cabecera llega completa
con `UseCalculatedHeaderConfiguration = false`.

> ⚠ **CORREGIDO en la ronda 3.** La conclusion que esta seccion saco de ahi —«ninguna frontera pierde
> nada»— era **falsa como afirmacion general**, y la validacion del Owner lo demostro. Lo unico que
> aquella auditoria probo es que el ciclo NO pierde nada **cuando se pulsa «Confirmar» y la sesion sigue
> viva**. El ciclo real del Owner es otro: pulsar **Actualizar** (que dibuja) y volver a entrar con
> **RACKEDITAR** (que construye una ventana nueva). Ver la seccion 4-ter.

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

## 4-ter. Ronda 3 del Owner — el candidato `3669adc` fue RECHAZADO

### Por que las pruebas de la ronda 2 daban un falso verde

Todas mantenian **viva la misma `RackModuleEditSession`** y pulsaban **«Confirmar»** antes de mirar el
resultado. Ese es justamente el unico camino que ya funcionaba. El ciclo del Owner no estaba cubierto
por ninguna prueba:

| Lo que probaba la ronda 2 | Lo que hace el Owner |
|---|---|
| «Confirmar» del panel de modulos | **«Actualizar»**, el boton que dibuja |
| La misma ventana, la misma sesion | **RACKEDITAR**: una ventana NUEVA sobre el diseño embebido en el DWG |

### Las dos fronteras reales

**Frontera 1 — dibujar descartaba lo escenificado.** `RequestDraw` (Actualizar e Insertar) llamaba a
`RequestRecompute()` **sin confirmar** la sesion de modulos. Toda edicion escenificada —una cabecera
personalizada, una copia a otras cabeceras— se descartaba en silencio y el rack se dibujaba **y se
embebia** con la cabecera estandar. Medido: la sesion tenia `H=187 PC=41.5` y el diseño dibujado salia
con `H=192 PC=44` y procedencia calculada. A partir de ahi no habia nada que recuperar.

**Frontera 2 — la sesion sobrevivia a la carga de OTRO rack.** Es la **primera divergencia** entre
procedencia y configuracion, y la que produce el sintoma exacto del caso B. Registro de la Cabecera 1
justo despues de dibujar:

| Punto | `UseCalculatedHeaderConfiguration` | `Height` |
|---|---|---|
| diseño dibujado | `false` | **187** |
| diseño persistido (JSON del DWG) | `false` | **187** |
| `WorkingBaseline` de la ventana nueva | `false` | **187** |
| **sesion de modulos de la ventana nueva** | **`true`** | **192** ← divergencia |

La causa: el **constructor** de `RackPushBackSystemWindow` llama a `LoadNew()`, asi que **ya hay una
sesion viva sobre el rack ESTANDAR** cuando `LoadExisting` carga el rack del dibujo.
`ReseedModuleSession` la **conservaba** porque la *firma* —ids y clases de modulo— coincidia, y
coincide casi siempre: dos racks del mismo tamaño tienen los mismos modulos. El editor quedaba
mostrando las cabeceras **calculadas del rack anterior** mientras la geometria usaba las
**personalizadas del rack cargado**.

La firma solo sirve para distinguir estados **de un mismo rack en recalculo**, que es para lo que se
escribio. **Cargar otro diseño no es un recalculo.**

### Correccion definitiva

| Frontera | Correccion |
|---|---|
| Dibujar | `RequestDraw` **confirma** la sesion antes de recalcular. Pulsar Actualizar o Insertar es aplicar lo que el panel muestra. La transaccion no se debilita: **«Cancelar» sigue siendo el UNICO descarte**; deja de haber un camino que descarta sin decirlo |
| Cargar | `PushBackEditorState.AdoptLoadedBaseline` — los dos caminos de carga (`LoadNew` y `LoadFromDesign`) **tiran la sesion entera**, nunca la reutilizan por coincidencia de firma |
| Resolver | Si no hay configuracion que instalar, el resolver construye la **calculada** y **lo dice**: `UseCalculatedHeaderConfiguration = true`. La bandera describe la configuracion que realmente quedo instalada |
| Persistencia | `ToDomain`/`ToDesign`: sin cabecera guardada, la procedencia es calculada aunque el documento diga lo contrario. Repara documentos escritos por los candidatos rechazados |
| Sesion | `RackModuleEditSession` sanea toda intencion que entre: sin configuracion, procedencia calculada |
| Configurador | Para una cabecera declarada personalizada **no existe fallback**. Si su configuracion faltara, se **bloquea con diagnostico** en vez de abrir sobre la predeterminada |

### Por que ya no puede existir «Personalizada + configuracion predeterminada»

El estado hibrido —`UseCalculatedHeaderConfiguration == false` con configuracion nula— se repara en
los **tres limites canonicos** por los que puede entrar (resolver, persistencia y sesion), no en la UI.
Como el descriptor deriva «personalizada» de *tener configuracion propia* **y** de la procedencia, y
esas dos ya no pueden contradecirse, la etiqueta y los datos son la misma verdad. El fallback de WPF
que podia disfrazar el hibrido queda ademas retirado.

**Impacto en otros consumidores:** el saneamiento solo actua sobre un estado que **ningun productor
correcto genera**, asi que una cabecera coherente —calculada o personalizada— atraviesa el resolver
exactamente igual que antes. Cubierto por prueba propia, y por las suites del Dinamico, el Selectivo y
Cantilever completas.

## 4-quater. Cuarta entrega — poste derivado y linea de cabeceras

La tercera validacion aprobo el ciclo (personalizar, Actualizar sin Confirmar, RACKEDITAR, copiar,
aplicar, reabrir, Cancelar). Faltaban dos requisitos.

### A. Altura del poste derivado

**Que es.** El poste derivado nace de **dos separadores consecutivos**; no pertenece a ninguna
cabecera. **Su altura nunca estuvo modelada**: `DynamicSystemLateralBuilder.AddDerivedPost` le pasaba
`context.Height` —la altura de la CABECERA— al parametro dinamico `LONGITUD` de su bloque. La
**HEREDABA** (caso b de la auditoria). Su **refuerzo** si era editable
(`DerivedPostReinforcementHeight`), y esa asimetria es justo lo que el Owner reporto.

**Solucion.** `DerivedPostHeight` (`double?`) como **hermano exacto** del de refuerzo: mismo tipo,
mismo sitio (parametro global del rack, no de una cabecera), misma nulabilidad y mismo significado del
vacio. Recorre `PushBackEditorInputs` → `PushBackAdvancedRackParameters` → `DynamicRackSystem` /
`DynamicRackDesign` → documento → resolver → `LONGITUD` del bloque → BOM.

**Compatibilidad.** Vacio = hereda la altura de la cabecera, que es exactamente el comportamiento
historico; un documento anterior no lleva el campo y se comporta igual. Un cambio consecuente y
declarado: el refuerzo «a toda la altura» ahora sigue la altura del **poste derivado** en vez de la de
la cabecera — identico mientras nadie fije la altura del poste, que es lo unico que existia antes.

### B. Linea de cabeceras

**Los dos conceptos que I-40 confundia.** Un rack tiene dos cosas que se llaman «cabecera»:

| | Que es | Donde vive |
|---|---|---|
| **Modulo longitudinal** | Una entrada de `Modules`: la secuencia de fondo cabecera/separador/cabecera, **compartida por todo el rack** | `DynamicRackModuleDesign` |
| **Linea fisica** | La **linea transversal de postes** — la que el lateral dibuja como un **corte** | `postIndex`, `DynamicLateralCorte` |

Cada modulo se materializa **una vez en cada linea que lo cubre**. Por eso **una**
`DynamicRackModuleDesign` **ES todas las instancias** de esa cabecera, y por eso el modelo no podia
expresar «esta linea distinta de aquella». La UI editaba solo el primer concepto.

**Lo que YA existia y decidio la solucion.** `DynamicFrontGeometry.HeaderConfigurationAtPost(system,
module, catalog, postIndex)` es **la unica funcion** que decide que configuracion usa una cabecera **en
una linea**, y la consumen los **tres** interesados: la geometria lateral, el BOM y el preview del
Dinamico. De hecho las cabeceras CALCULADAS ya variaban por linea ahi (se reconstruyen a la altura de
ese poste). Solo faltaba poder decir lo mismo de una personalizada.

**Solucion minima.** `DynamicHeaderLineOverride { PostIndex, ModuleId, Header }` — la configuracion de
UN modulo en UNA linea, direccionada por el **mismo par** que esa funcion ya recibia. El override se
consulta **dentro** de esa unica autoridad, de modo que geometria, BOM y preview lo obedecen por
construccion (AGENTS: la regla vive en una sola funcion). No se duplican racks ni modulos, no se toca
`DynamicSystemLateralBuilder`, y `ModuleId` y GUID no cambian.

**Persistencia.** El formato **solo crece** con un arreglo opcional `HeaderLineOverrides`, ausente en
todo documento anterior y ausente cuando esta vacio. Ausente significa lo de siempre: cada linea usa la
configuracion del modulo. El **Dinamico nunca los escribe**, asi que su comportamiento es identico —
comprobado por prueba propia y por las suites de Dynamic (205 + 41), Selective (324) y Cantilever (869).

### Modelo de alcance final

**DestinationHeaders × DestinationLines.** Los destinos de una operacion son el PRODUCTO CARTESIANO de
dos ejes INDEPENDIENTES:

| Eje | Valores |
|---|---|
| **Cabeceras destino** | una, varias o todas (lista de seleccion multiple) |
| **Lineas destino** | una, varias o todas (lista de seleccion multiple) |

Cada par `(PostIndex, ModuleId)` del producto es una cabecera FISICA y recibe su PROPIA copia. «Solo
esta», «Esta linea» y «Todas» sobreviven unicamente como **atajos de seleccion** («Esta» / «Todas» en
cada eje), nunca como modelo de datos.

Las tres validan todos los destinos antes de tocar nada, dan a cada destino su **propia copia**,
recomputan **una sola vez** al Confirmar y se revierten enteras con Cancelar. «Copiar de» funciona con
cualquiera de los tres.

> **OWNER DECISION (definitiva).** La unidad de edicion de una cabecera es la **instancia FISICA**,
> identificada por **`(PostIndex, ModuleId)`**, y la jerarquia funcional es
>
> ```
> una cabecera fisica  ⊂  una linea fisica  ⊂  todo el rack
> ```
>
> **«Solo esta cabecera» NO significa el modulo longitudinal en todas sus lineas.** Aquella lectura era
> consecuencia de que el modelo no tenia dimension por linea; con `DynamicHeaderLineOverride` ya puede
> representar la personalizacion de una unica instancia, asi que no se conserva por compatibilidad de
> UI. El candidato `180aef3`, que la mantenia, quedo rechazado.
>
> Consecuencias en la superficie:
> - el **selector de linea** es relevante para «Solo esta cabecera» **y** para «Esta linea»; solo el
>   alcance global lo ignora, porque ahi van todas;
> - la ventana dice siempre que combinacion va a modificar: **Cabecera** + **Linea** + **Aplicar a**;
> - el **ORIGEN** de «Copiar de» es siempre la instancia que el usuario tiene delante —su configuracion
>   de linea si la tiene, y si no la del modulo—; lo que el alcance decide es el **DESTINO**. Cuando el
>   origen solo esta personalizado en OTRA linea, se copia esa (la primera), porque es la
>   personalizacion que el usuario esta senalando;
> - «ya personalizada» —lo que decide que el configurador se abra para EDITAR y no para generar— se
>   evalua tambien sobre la instancia fisica.

## 4-quinquies. Ronda 5 — el candidato `73325cf` fue RECHAZADO

### Por que cambiar el alcance no hacia nada

El alcance solo se evaluaba **dentro de `StageHeaderConfiguration`**, y esa funcion solo se llamaba
cuando el configurador devolvia una configuracion NUEVA. Cambiar el alcance despues no era una
operacion: no habia nada que lo leyera. De ahi el sintoma exacto del Owner — «hay que volver a abrir
Configurar cabecera para que el nuevo alcance tenga efecto».

**Correccion:** la configuracion ORIGEN se recuerda (`pendingHeaderConfiguration`) y **«Aplicar
configuracion a la seleccion»** es una accion explicita que la reparte tantas veces como haga falta.
Elegir destinos no modifica nada por si mismo; Confirmar confirma; Cancelar revierte.

### Poste derivado POR LINEA

El poste derivado nace entre dos separadores CONSECUTIVOS: pertenece a la **linea**, no a ningun
modulo, asi que **no** se metio dentro de `DynamicHeaderLineOverride`. Estructura propia,
`DynamicDerivedPostLineOverride { PostIndex, Height }`, con su lista persistida opcional. La autoridad
unica es `DynamicFrontGeometry.DerivedPostHeightAtPost` —linea → rack → altura heredada de la
cabecera—, y la leen la geometria lateral y el BOM. El valor global sigue como fallback.

### Frontal y posterior — causa raiz

`DynamicSystemFrontalBuilder` dibujaba el poste de cada linea con
`DynamicFrontGeometry.PostHeight(system, postIndex)`, un valor **derivado** que ignora por completo la
configuracion de la cabecera. Por eso personalizar una cabecera cambiaba el lateral y dejaba la
frontal igual. Ahora la frontal lee `HeaderHeightAtPost`, que resuelve por
**`HeaderConfigurationAtPost`** — la MISMA autoridad que consumen la geometria lateral y el BOM.

> ⚠ **Corregido en la ronda 6.** Aquella primera version de `HeaderHeightAtPost` tomaba la cabecera
> **mas alta** de la linea (`Max`), es decir una **envolvente**. Ver 4-sexies.

### Protector lateral del ultimo corte — causa raiz

La regla adaptativa da al primer poste una copia sin espejo y al ultimo una **espejada**, porque
protegen **caras opuestas del pasillo**: ese espejo es **a lo ANCHO** del rack, y por eso la PLANTA lo
dibuja bien. El corte LATERAL mira UNA linea de postes y su eje horizontal es el **FONDO**, asi que
aplicar alli ese espejo volteaba la pieza sobre el fondo. En el lateral el volteo depende UNICAMENTE
del extremo en que la copia se apoya (`mirrored = AtHighEnd`): con extremo alto (Dinamico) sale
exactamente lo mismo que antes; en extremo bajo (Push Back) el ultimo corte deja de invertirse. La
planta no se toco.

## 4-sexies. Ronda 6 — OWNER DECISION: frontal y posterior son CORTES, no envolventes

> **Frontal y Posterior son vistas de CORTE de la primera y la ultima linea fisica,
> respectivamente; no son envolventes de altura.**

### Causa raiz

La ronda 5 arreglo que la frontal ignorase la configuracion de la cabecera, pero resolvio la altura
con un **`Max()`** sobre todas las cabeceras de la linea:

```csharp
// ANTES (04f76cf) — envolvente
foreach (var module in system.Modules) { ... if (configuration.Height > tallest) tallest = ...; }
return tallest > 0.0 ? tallest : derived;
```

Con una cabecera intermedia de 200", la frontal y la posterior mostraban **200"** aunque la primera
midiera 180" y la ultima 120". Una vista de corte no puede estar dominada por una pieza que esta en
otra posicion longitudinal.

### Correccion

`HeaderHeightAtPost(system, catalog, postIndex, DynamicRackEnd end)` delega en
**`HeaderConfigurationAtCut(system, catalog, postIndex, end)`**, que toma las cabeceras del rango de
esa linea, las ordena por su indice longitudinal y elige:

| Vista | `DynamicRackEnd` | Cabecera | `postIndex` |
|---|---|---|---|
| **FRONTAL** (entrada/salida, extremo bajo) | `Exit` | la **PRIMERA** del rango | el de **cada linea** que la vista dibuja |
| **POSTERIOR** (extremo alto) | `Entrance` | la **ULTIMA** del rango | el de **cada linea** que la vista dibuja |

La configuracion sale de **`HeaderConfigurationAtPost`**, la misma autoridad de siempre: no hay logica
paralela y los overrides por linea siguen mandando.

**Los dos ejes no se mezclan:** `end` elige QUE cabecera se tiene delante (eje longitudinal) y
`postIndex` elige de QUE linea es el poste que se dibuja (eje transversal). En un rack con fondos
distintos, cada linea acaba en SU propia cabecera, que es lo correcto.

**Ningun `Max()` mas se toco:** el de `DynamicRackSystemResolver` (altura del rack) y los de las
envolventes de nivel siguen intactos, porque ahi si se quiere el maximo. Tampoco cambiaron el BOM, la
planta, el lateral, el protector del ultimo corte, el modelo ni el formato de alambre.

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

## 7. Checklist de la ronda 3 (6 pasos, solo este defecto)

1. Personalizar la **Cabecera 1** (alto evidente) y pulsar **Actualizar** — **sin** pulsar «Confirmar»
   antes. La geometria debe reflejar el cambio.
2. Ejecutar **RACKEDITAR** sobre ese rack y abrir **«Configurar cabecera...»** en la Cabecera 1: deben
   aparecer **sus valores personalizados**, no los predeterminados.
3. Seleccionar la **Cabecera 2** y copiar desde la Cabecera 1: la 2 recibe **esos** valores.
4. Poner el alcance en **«Todas las cabeceras»**, copiar y pulsar **Actualizar**: ninguna vuelve a la
   forma predeterminada.
5. **RACKEDITAR** de nuevo: todas las cabeceras siguen mostrando esos mismos valores y siguen marcadas
   «Personalizada».
6. Personalizar una cabecera, pulsar **Cancelar** y luego **Actualizar**: no debe quedar ninguna
   cabecera personalizada — la transaccion sigue siendo transaccion.

## 8. Checklist de la cuarta entrega (8 pasos)

1. Push Back con **dos frentes** (⇒ tres lineas de postes) y varias cabeceras.
2. «Avanzado» → **«Altura del poste derivado»**: fijar un valor evidente y **Actualizar**. El poste
   derivado del dibujo cambia de longitud; vaciar el campo lo devuelve a la altura de la cabecera.
3. Seleccionar una cabecera, poner **«Aplicar a: Esta linea de cabeceras»** y elegir **Linea 1**.
4. «Configurar cabecera...», cambiar alto y algo mas, cerrar y **Actualizar**.
5. Comprobar en el dibujo que **el corte de la linea 1 cambio y el de la linea 2 no**.
6. **RACKEDITAR**: las diferencias entre lineas siguen ahi, y la altura del poste derivado tambien.
7. Cambiar a **«Todas las cabeceras»**, configurar y Confirmar: **las dos lineas** quedan iguales.
8. Repetir una operacion por linea y pulsar **Cancelar**: no queda ninguna linea alterada.

## 9. Checklist de la decision final (8 pasos)

1. Push Back con **dos frentes** (⇒ tres lineas) y varias cabeceras. Elegir **Cabecera 1**.
2. **Linea: Linea 1**, **Aplicar a: Solo esta cabecera**. Configurar un alto evidente y **Actualizar**.
3. Comprobar en el dibujo que **solo el corte de la Linea 1** cambio: la **misma cabecera en la Linea 2
   sigue igual**, y las demas cabeceras de la Linea 1 tambien.
4. **RACKEDITAR**: la diferencia sigue ahi.
5. Cambiar a **Esta linea de cabeceras** sobre la **Linea 2** y configurar: **todas** las cabeceras de
   la Linea 2 cambian, y la Linea 1 no.
6. Cambiar a **Todas las cabeceras** y configurar: **todas las lineas** quedan iguales, sin rastro de
   las personalizaciones por linea anteriores.
7. Volver a **Solo esta cabecera** sobre **(Linea 2, Cabecera 1)** y cambiarla: **solo esa** cambia.
8. Repetir una operacion de una sola instancia y pulsar **Cancelar**: no queda ningun cambio.

## 10. Checklist de la ronda 5 (8 pasos)

1. Push Back con **tres frentes** (⇒ cuatro lineas) y varias cabeceras.
2. Seleccionar **Cabecera 1**, destino **Cabecera 1 × Linea 1**, «Configurar cabecera...», cambiar el
   alto y **Actualizar**. Solo esa instancia cambia — y la **frontal** cambia con el lateral.
3. **SIN volver a configurar**: marcar **todas las lineas** y pulsar **«Aplicar configuracion a la
   seleccion»**. La Cabecera 1 cambia en todos los cortes.
4. Marcar **Cabeceras 1 y 3** × **Lineas 1 y 2** y aplicar: cambian exactamente esas cuatro.
5. **Todas** × **Linea 2**: solo la Linea 2. **Todas** × **Todas**: el rack completo.
6. Con **Lineas 1 y 2** marcadas, fijar **«Altura del poste derivado»** y aplicar: solo esos postes
   derivados cambian.
7. Generar **todos los cortes laterales**: el **ultimo** ya no dibuja el protector invertido, y la
   **planta** sigue igual.
8. **Actualizar → RACKEDITAR**: todo lo confirmado sobrevive. Repetir una operacion y **Cancelar**: no
   queda nada.

## 11. Checklist de la ronda 6 (5 pasos)

1. Push Back con **varias lineas** y al menos **tres cabeceras** longitudinales.
2. Dejar la **primera** cabecera ALTA (p. ej. 180"), una **intermedia** MAS ALTA (200") y la **ultima**
   BAJA (120"). **Actualizar**.
3. Ver la **FRONTAL**: debe mostrar **180"**, nunca 200".
4. Ver la **POSTERIOR**: debe mostrar **120"**, nunca 200".
5. Cambiar SOLO la cabecera intermedia y **Actualizar**: cambian sus **cortes laterales** y **ni la
   frontal ni la posterior** se mueven. La planta y el protector del ultimo corte siguen igual.
