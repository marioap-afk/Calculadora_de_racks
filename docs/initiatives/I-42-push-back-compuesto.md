---
schema: rackcad-initiative/v1
id: I-42
title: Push Back compuesto, bidireccional y camas compartidas
type: feature
status: implementing
branch: feature/push-back-compuesto
base_branch: main
priority:
size:
depends_on: [I-18, I-30, I-31, I-32, I-33, I-34, I-35, I-39, I-40, I-41]
conflicts_with: []
context_packs: [system-dynamic-flowbed, ui-editors, persistence, delivery-validation]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
---

# Push Back compuesto, bidireccional y camas compartidas

> **Gate documental abierto.** I-42 **no tiene fila en `docs/ROADMAP.md`**; la apertura la autorizo el
> Owner por instruccion directa, igual que I-35 y I-40. La fila y el estado en HANDOFF los escribe la
> **sesion de integracion**, como ultimo commit de esta rama (WORKFLOW 4.5.4 y 8). Esta sesion no toca
> ninguno de los dos.

## 1. Objetivo

Push Back deja de estar limitado a un solo sentido y pasa a soportar, dentro de **un mismo sistema
fisico**, lado A y lado B enfrentados: cantidades de frentes distintas, niveles y elevaciones
independientes, fondos y tarimas por celda en cada lado, topologia por **celda** (solo A, solo B,
encontradas, corrida) con los dos sentidos de corrida, hueco entre las dos mitades con separador central
opcional, topes donde fisicamente proceda, y una **unica** propiedad fisica de postes, cabeceras, placas,
separadores, camas y BOM.

## 2. Decision arquitectonica

[ADR-0031](../adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md), **propuesto**:
un Push Back compuesto tiene **UNA estructura fisica** y **DOS configuraciones funcionales de
almacenamiento**. Nace propuesto porque gobierna lo que se ve; se acepta —o no— con el veredicto del Owner
en AutoCAD 2025, como ADR-0023 en I-36D y ADR-0029 en I-39A.

## 3. Fuera de alcance (no tocado)

Calculo resistente, cargas, RAM Elements, costos, CNC, shop drawings, soldaduras, tornilleria nueva,
Selectivo, Cantilever, Cama independiente, multirrack, formulas, guardas traseras, rediseno WPF general y
biblioteca nueva de cabeceras. [ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md) sigue
vigente.

## 4. Ronda de correccion (el candidato `6c9f778` fue RECHAZADO)

El Coordinador y las pruebas manuales preliminares del Owner rechazaron el primer candidato. Lo corregido:

1. **Solo-A + solo-B en la MISMA estructura.** La limitacion desaparecio: el contrato de profundidad admite ahora
   un modo NO ANIDADO explicito que solo enciende el compositor del Push Back compuesto y que nunca se persiste.
   `F1` compartida, `F2` solo A, `F3` solo B y `F4` compartida conviven sobre una sola estructura.
2. **Cotas y etiquetas del lateral por LADO**, con la geometria real de cada uno. Ninguna cota afirma ya una
   elevacion de A sobre una pieza de B. Los textos y las cotas ya no se voltean al reflejarse.
3. **Planta**: proyecta la union de piezas fisicas REALES. Una ranura con todos sus niveles corridos ya no dibuja
   los largueros de interfaz que no existen; basta con que un nivel no lo sea para que aparezcan.
4. **Ninguna entrada invalida se corrige en silencio**: un hueco negativo se conserva y bloquea; un ajuste manual
   de estructura invalido NO equivale a restaurar — restaurar es su propio boton.
5. **I-40 sobrevive a la recomposicion**: los modulos se reconcilian fisicamente por posicion contada desde el
   extremo exterior del lado, conservando `ModuleId`, configuracion personalizada y longitud manual.
6. **Largueros intermedios**: pertenecen a la CAMA, se construyen en su marco y viajan con su transformacion. El
   BOM los cuenta con el MISMO builder que los dibuja.
7. **UI**: la seccion compuesta se COLAPSA con el rack de un solo sentido.
8. **Falso error de capacidad 4+8**: la capacidad se mide por CAMA FISICA. La causa era que «encontradas»
   comparaba la demanda de una cama contra el espacio de la otra.
9. **La corrida NO ocupa todo el sistema**: anclada en el extremo ALTO, apoya en el primer soporte valido hacia el
   bajo. (La relacion rigida con `RequiredBedLength` se retiro en la tercera ronda: ver 4-ter.)
10. **Una corrida corta no crea otra estructura**: el sistema sintetico es una receta geometrica y no materializa
    ni un poste.

Ademas, la propia bateria de pruebas de esta ronda destapo **dos defectos mas**, corregidos aqui: el ajuste manual
de estructura no llegaba a la profundidad de las ranuras (el rack crecia pero perdia las personalizaciones de
I-40), y la demanda de una corrida se contaba en modulos en vez de en fondos (un rack con hueco parecia tener una
posicion mas).

## 4-bis. Segunda ronda de correccion (el candidato `e90442a` fue RECHAZADO)

Un solo blocker de contrato: la conclusion «el hueco no aporta capacidad» era **incorrecta**. El hueco pertenece a
la ESTRUCTURA, no a la demanda, asi que un hueco positivo **si** puede volver valida una cama que sin el no cabe.
La causa era que la longitud EXIGIDA se media sobre los modulos reales incluyendo el del hueco: al crecer el hueco
crecian a la vez lo exigido y lo disponible, y por construccion nunca rescataba nada.

Corregido con **una sola autoridad de demanda** (`PushBackBedSpan.DemandLength`) que suma unicamente los modulos
que ALOJAN TARIMA y atraviesa el hueco sin exigirlo. Ahora:

```
RequiredBedLength   = demanda        (no ve el hueco, ni el total, ni la estructura sobrante)
AvailableBedSpan    = estructura     (SI incluye el hueco)
valido              <=>  Required <= Available
```

Evidencia numerica (estructura A=5 / B=4 por ajuste manual, demanda corrida 8+5 = 13 fondos):

| | Demand | Required | Available | Valida |
|---|---|---|---|---|
| Gap = 0" | 13 | 648" | 456" | **no** |
| Gap = 198" | 13 | 648" | 654" | **si** |

`delta Required = 0"` · `delta Available = 198"` (exactamente el hueco). La longitud FISICA de la cama la fija el
apoyo, y por tanto **si** puede moverse con el hueco: ver 4-ter.

Ademas se retiro el ultimo recorte silencioso: la longitud EXIGIDA de una cama que no cabe se conserva entera, de
modo que el diagnostico puede decir cuanto le falta. (En la tercera ronda esto se separo del dibujo: lo exigido se
conserva, y lo que se DIBUJA es el tramo mas largo que la estructura ofrece — ver 4-ter.)

## 4-ter. Tercera ronda de correccion (el candidato `3b55ca7` fue RECHAZADO)

Tres blockers, todos de la misma familia: **autoridades duplicadas**.

### 1. El fondo de la cama CORRIDA es una autoridad PROPIA

`demand = fondo(A) + fondo(B)` era **incorrecto**. Una corrida de 10 fondos sobre una estructura 5 + 8 pide 10, no
13, y las dos estructuras siguen siendo 5 y 8. Se retiro esa suma y se introdujo `PushBackTopologyCell.CorridaDepth`:

- es de la CELDA, no de ningun lado;
- sin valor, la corrida hereda un **default derivado** — la capacidad de la estructura, «la calle atraviesa el
  rack» — exactamente como en I-41 el fondo del frente es el default de la celda;
- los fondos de A y de B quedan **dormantes**, no se borran, y vuelven a gobernar en cuanto la celda deja de ser
  corrida: cambiar de topologia es **reversible**;
- la persistencia es **aditiva y anulable**: un documento que nunca la uso no escribe el campo.

En el editor es el **mismo** campo y los **mismos** cinco alcances; lo que cambia con la topologia de la celda
seleccionada es a que autoridad escribe, y la etiqueta lo dice («Fondo de cama corrida»). Un alcance que mezcla
corridas con celdas que no lo son escribe **solo** en las corridas y lo declara.

### 2. Una sola autoridad de colocacion (el bug visual de la corrida)

La cama parecia arrancar en el **segundo fondo**. La causa era una **doble autoridad**: primero se elegia un rango
discreto de modulos y despues se sobrescribia `StartX` con una resta continua `EndX - RequiredBedLength`. El
resultado quedaba flotando dentro de una posicion, sin apoyarse en nada.

Se retiro la resta. `PushBackBedSpan.ResolveSpan` es ahora la unica autoridad: recorre los apoyos desde el ancla
ALTA hacia la baja y toma el **primero** cuya distancia satisface la demanda. Los apoyos son las lineas de modulo,
las mismas sobre las que un Push Back de un sentido ya coloca su larguero bajo — no se invento ningun concepto.

Con ello se **retira** la relacion rigida `PhysicalBedLength = RequiredBedLength` y la sustituye:

```
RequiredBedLength  <=  ResolvedBedLength  <=  AvailableBedSpan
```

`ResolvedBedLength` **puede** cambiar con el hueco, y debe: si para llegar a su ultimo fondo la cama tiene que
cruzarlo, el apoyo valido queda mas lejos. Lo que el hueco no cambia nunca es la **demanda**, y por eso sigue
pudiendo rescatar una cama que sin el no cabe. La prueba que consagraba la igualdad rigida se retiro; en su lugar
se exige que la cama **apoye siempre en una linea de modulo real** y que el BOM cotice `ResolvedBedLength`.

### 3. La planta no dibujaba ni un larguero intermedio

Se retiraban con el resto de piezas del dinamico y nadie los reponia. Ahora se reponen **por cama**, en el marco de
cada una y con su misma transformacion, igual que ya hacia el lateral: una corrida corta obtiene intermedios en
todo su recorrido —incluida la parte que pisa el otro lado— y ninguno en el tramo de estructura que no usa.

### Lo que destapo la auditoria de esta ronda

Un defecto **propio**, no reportado por el dueno: la longitud de una cama corrida se resolvia siempre en el marco
del RACK, mientras que el dibujo la resolvia en el marco de la CAMA. En sentido `B->A` esos dos marcos no coinciden
—la secuencia de fondos no es simetrica: 5 + hueco + 8 leida al reves no da las mismas fronteras—, asi que el BOM
cotizaba una longitud distinta de la dibujada (294" contra 306" en el caso probado) y el apoyo declarado no era el
real. Corregido: la cama se resuelve en **su** marco, el mismo que usa `PushBackRuns`, y hay una prueba que compara
la longitud cotizada con la dibujada **en los dos sentidos**.

Ademas se retiraron `ModulesForLastPositions` y `FirstPositionOfLast`, que habian quedado sin uso: eran una segunda
manera de decidir donde empieza una cama, y el contrato de esta ronda exige una sola.

## 4-quater. Validacion del dueño en AutoCAD: RECHAZADA (candidato `36fe5d3`)

El dueño valido `36fe5d3` en AutoCAD 2025 y encontro que **solo el primer frente funcionaba**, que la planta no
traia intermedios mas alla del primero, que la corrida seguia desplazada, que el fondo por celda se comportaba como
«todos o ninguno» y que no habia forma de decidir los topes A/B. El hueco y los dos sentidos SI quedaron validados.

Ninguna prueba lo habia detectado porque **todas median el rack entero**: `Assert.NotEmpty(vigas)` pasa con un solo
frente. Esta ronda cambia eso —nada se mide en agregado— y encuentra **siete** defectos.

### 1. La topologia por defecto se quedaba en «Solo A» — LA CAUSA DE «solo quedan las cabeceras»

Un rack NUEVO nace de un sentido, asi que su topologia por defecto es `SoloA`. Declarar el lado B no la revisaba,
de modo que **todas** las celdas seguian siendo Solo A: el lado B aportaba estructura —postes, cabeceras, placas— y
ni una sola cama. Es exactamente lo que el dueño describe.

Corregido donde vive la regla: la topologia por defecto depende de cuantos sentidos tiene el rack, la misma regla
que ya se aplicaba al CARGAR. Una eleccion explicita distinta no se pisa.

### 2. El numero de frentes era del LADO ACTIVO, no del rack

Poner «4 frentes» crecia solo el lado en el que el usuario estuviera. El otro se quedaba con los suyos, en silencio,
y el rack acababa con el primer frente completo y los demas a medias.

La retícula transversal es UNA: el conteo es del RACK y crece y decrece en los dos lados. La asimetria `A=3 / B=4`
—que sigue siendo un requisito— se expresa ahora con **PRESENCIA por ranura**, que el dominio ya sabia representar y
para la que **no habia ningun control**. Se añade la casilla «La ranura existe en este lado», con sus guardas: una
ranura no puede quedarse sin ningun lado, ni un lado sin ninguna ranura, y las dos negativas se explican.

### 3. `CorridaDepth` se convertia en modulos fisicos por la puerta de atras

`resolved.PalletsDeep` recibia el conteo de MODULOS del rango en vez de la DEMANDA en fondos. Un hueco es un modulo
que la cama atraviesa sin almacenar nada, asi que con hueco la celda declaraba **un fondo de mas**: se repartia una
tarima extra a lo largo del riel y **todas** las posiciones quedaban desplazadas. Asi se veia «la cama esta en el
fondo equivocado».

Separado en tres autoridades —demanda, longitud minima y span fisico— y ademas:

- el fondo EFECTIVO de la celda es siempre la demanda;
- `PushBackCellDepth.EndPosition` cuenta posiciones que ALOJAN TARIMA, saltando huecos, de modo que el larguero
  posterior de una corrida cae en su ancla y no un modulo antes;
- el reparto de tarimas se hace sobre la longitud de ALMACENAMIENTO y empuja cada una por los huecos anteriores:
  ninguna cae dentro de un hueco.

En una estructura sin huecos las tres cosas devuelven exactamente lo de siempre.

### 4. La longitud de la cama se resolvia en el marco equivocado en sentido `B->A`

Ya corregido en el commit anterior de esta misma rama y aqui cubierto por pruebas en los dos sentidos.

### 5. No habia forma de decidir los topes A/B

Se añade «Tope lado A» / «Tope lado B» al panel compuesto, con los MISMOS cinco alcances. La autoridad **no cambia**:
sigue siendo el `PushBackRearTopeConfig` de cada lado. La topologia decide cual puede materializarse —un tope vive
en el extremo ALTO de una cama—, y la casilla del lado que no aplica se deshabilita con su motivo conservando lo
elegido.

### 6. La PRESENCIA del lado A no volvia del archivo

El diseño DECLARA las ranuras ausentes en vez de borrarlas (borrarlas desplazaria los indices), pero el editor no
las releia: un rack asimetrico se reabria con las cuatro ranuras en los dos lados.

### 7. Reabrir un rack compuesto CORROMPIA la matriz del lado A

El estado del editor se reconstruye desde el sistema RESUELTO, y el de un rack compuesto es la estructura compartida
`A + hueco + B`, cuyos rangos no estan anidados. Con dos o mas frentes, el siguiente recalculo se caia con «los
frentes con el menor numero de fondos deben compartir la misma posicion inicial». Ahora el lado A se carga contra SU
propio diseño, igual que ya se hacia con el lado B.

### Las pruebas de esta ronda

Nada se mide en agregado. `PushBackCompositeMultiFrontTests` recorre la cadena completa —matriz, estado, diseño,
configuracion por lado, celdas, camas, cortes y planta— con aserciones POR FRENTE y POR NIVEL sobre un rack de
4 x 3 en los dos lados, en las cuatro topologias; `PushBackCompositeTopeTests` fija las cuatro combinaciones de tope
y su dormancia; y en la ventana real se comprueba que el alcance «Celda» cambia UNA celda comprobando las DOCE, que
los cinco alcances escriben exactamente su conjunto, y que un rack completo se reabre con su presencia, sus topes y
su fondo de corrida.

## 4-quinquies. Segunda validacion del dueño: RECHAZADA (candidato `67a24d0`)

Multifrente, multinivel, planta e intermedios quedaron validados. Lo que fallo:

### 1. La cama corrida estaba anclada al extremo EQUIVOCADO (decision fisica)

El dueño lo dijo sin ambiguedad: el extremo BAJO —por donde se carga y se descarga— queda SIEMPRE en el poste
exterior, y el que se mete hacia dentro al pedir menos fondo es el ALTO. El candidato hacia lo contrario: dejaba el
alto pegado a la orilla y metia el bajo, con el pasillo delante inaccesible.

Se invirtio la autoridad de colocacion: `PushBackBedSpan.ResolveSpan` recorre desde el ancla BAJA hacia la alta y
devuelve la posicion del ALTO. Y se separo explicitamente lo que se estaba mezclando:

```
LONGITUDINAL:  manda el BAJO   (fija el origen exterior; el alto se resuelve por demanda)
VERTICAL:      manda el ALTO   (fija elevacion y troquel, I-32; el bajo se deriva por la pendiente)
```

La prueba vinculante compara el mate BAJO de la corrida con el de un Push Back de UN SENTIDO —el oraculo del
producto— en coordenadas reales, no «esta dentro del primer modulo»: detecta un desplazamiento de un fondo entero.

### 2. Subir niveles en A elevaba el lado B

La ronda anterior resolvia los dos lados otra vez con `max(alturaA, alturaB)` y esa altura llegaba a TODAS las
cabeceras. Retirado. Los dos lados comparten la retícula TRANSVERSAL —lineas de postes, ancho, BFR— pero cada uno es
una estructura LONGITUDINAL propia: la compuesta ADOPTA, cabecera por cabecera y por `ModuleId`, la configuracion
que su lado ya resolvio. Con 4 niveles en A y 2 en B, las dos lineas de la interfaz miden distinto, que es lo que
fisicamente son: dos piezas.

### 3. La seguridad no llegaba al segundo pasillo

Un rack compuesto son dos Push Back opuestos: los DOS extremos son cara de carga y ninguno es un extremo alto donde
la seguridad estorbe. Ahora un rack compuesto la coloca en los dos por defecto, sin pedirla. La autoridad no cambia
—sigue siendo la unica que deduplica y excluye GUIA/PARRILLA/TOPE—; lo que declara el rack es en cuantos extremos se
materializa. El tope posterior es otra cosa y sigue siendo por lado, con el default del PRODUCTO para un lado nuevo.

### 4. Los cortes frontales no decian de que lado eran

«Frontal entrada/salida» y «Frontal posterior» solo son inequivocos en un rack de un sentido. Con el compuesto
encendido aparece un selector propio —«Frontal de A» / «Frontal de B»— que califica a los dos botones: los cuatro
cortes son pedibles y su lado es EXPLICITO. Antes el lado salia del «lado activo» de la EDICION, que es un modo que
no se ve en la barra de vistas. (Cuatro botones no caben: la barra ya lleva doce y se recortarian, lo que su propia
prueba de tamaño detecta.)

### 5. «Fondo» y «estructura» parecian dos campos que hacen lo mismo

Eran dos autoridades distintas presentadas como equivalentes. Ahora la seccion de estructura se llama «Estructura
longitudinal del lado», muestra la PROPUESTA automatica y la EFECTIVA, y su campo se llama «ajuste manual» con la
advertencia de que NO es el fondo de almacenamiento. «Fondos frente» dice de que lado es cuando hay dos.

### 6. «Ambos lados», y «la ranura existe en este lado»

El selector de lado gana «Ambos lados»: una operacion de EDICION que escribe la misma intencion en A y en B —no un
tercer lado; no existe en el dominio, en el archivo ni en el dibujo—. Lo que por definicion es de un lado (la
presencia de un frente, el ajuste de estructura) se deshabilita con su motivo, y un campo con valores distintos entre
lados se muestra VACIO: escribir uno lo aplica a los dos, dejarlo vacio conserva el de cada lado.

La casilla de presencia pasa a llamarse «Frente presente en este lado», con la explicacion de para que sirve. La
palabra «ranura» desaparece de la interfaz.

### Lo que destapo la auditoria de esta ronda

Tres controles se saltaban las reglas que la ronda acababa de establecer: «+ frente» y «- frente» crecian solo el
lado activo (la retícula es del RACK), y «+ nivel» / «- nivel» y «En blanco» no seguian la seleccion de edicion.
Corregidos y con prueba.

## 4-sexies. Tercera validacion del dueño: RECHAZADA (candidato `d6e6372`)

El dueño reporto diez puntos. Dos se corrigieron con causa raiz identificada, tres NO son reproducibles desde el
modelo ni desde la ventana (y quedan documentados con lo que se midio), y dos exigen una decision del dueño antes de
tocar nada. Los tres restantes no se alcanzaron en esta corrida.

### CORREGIDOS

**Errores 2 y 3 — seguridad duplicada y protectores en todos los postes. MISMA causa raiz.**

La ronda anterior expreso «dos pasillos» escribiendo `Side = Both` en cada seleccion de seguridad. Eso APAGA las
reglas ADAPTATIVAS de cada familia, que solo se aplican cuando el usuario NO ha elegido lado
(`DynamicLateralGuardPlan.SideAt` y `CopiesAt` empiezan comprobando `Side != None || PostSides.Any()`). El protector
lateral, que legacy pone SOLO en las dos lineas de orilla, pasaba a colocarse en TODAS —y con dos copias— porque
`SideForPost` cae en `Side` para todo poste sin entrada propia.

Corregido donde estaba el error de modelado: **pertenencia, orientacion y extremo son tres ejes y ninguno puede
hablar por otro**. «Dos caras de carga» viaja ahora en su propio eje, `BothEndsAreLoadFaces` —derivado y no
persistido, como `LowEndOnly`—, y `Side` no se toca. La pertenencia vuelve a ser la del usuario o la adaptativa.

Pruebas: los protectores caen EXACTAMENTE en las dos lineas de orilla y en ninguna interior (por coordenada Y, no
por conteo); ninguna pieza de seguridad se materializa dos veces en el mismo sitio con la misma identidad y
orientacion, en las cuatro topologias; y el BOM del protector coincide con lo materializado.

### NO REPRODUCIBLES — se midio y no se encontro el defecto

**Error 1 — misma configuracion A/B, distinta altura en el lateral.** Con A y B identicos (1, 2, 3 y 4 niveles), las
elevaciones de cama de los dos lados son IGUALES hasta la ultima milesima, en el lateral general y en los cortes. Con
A=3/B=2 y A=4/B=2, el lateral y el frontal de CADA lado coinciden en numero de niveles y en separaciones. No se
encontro ninguna divergencia entre builders. Queda una bateria vinculante que la detectaria si apareciera.

**Error 8 — «Frente seleccionado» muestra los defaults de A.** Con A y B deliberadamente distintos en los cinco
campos (posiciones, niveles, fondo, fondo inicial, alto del primer nivel), el panel muestra los del lado elegido,
campo por campo, y volver al otro no deja valores rancios. Queda prueba de ventana campo por campo.

**Error 9 — cambiar un lado hace crecer el otro.** Subir NIVELES en A cambia solo la altura de A (120 → 264, B en
120); subir su FONDO cambia solo su propuesta, su estructura efectiva y su longitud (4/204 → 8/396, B en 4/204); un
ajuste manual en A no toca B. Y al reves. Queda prueba vinculante de las cuatro magnitudes.

Si el dueño puede indicar la configuracion y los pasos exactos con los que los vio, se reproducen y se corrigen.

### ESCALADOS — decisiones que no me corresponden

**Error 6 — la corrida «baja» el larguero.** REPRODUCIDO y medido. Con «Alto 1er nivel» = 10 y dos niveles:

```
encontradas   L1  bajo = 18.48   alto = 31.42
corrida A->B  L1  bajo =  4.48   alto = 31.42
```

El extremo ALTO conserva exactamente su elevacion —la autoridad vertical sigue siendo el alto, como manda el
contrato vigente—. El BAJO cae 14" porque la cama corrida es el doble de larga y la MISMA pendiente sobre el doble
de recorrido baja el doble. No hay ningun offset añadido: es la consecuencia geometrica de la pendiente.

Decidir que el extremo BAJO conserve la elevacion configurada obligaria a subir el ALTO (a ~44" en el ejemplo) y
CONTRADICE el contrato aceptado «en una corrida gobierna verticalmente el lado ALTO» (ADR-0031 §9, I-32). Es una
decision fisica nueva y se escala en vez de elegirla por mi cuenta.

**Error 7 — «Alto 1er nivel» y el cero real.** Hoy el valor se AJUSTA AL TROQUEL MAS CERCANO a esa altura absoluta:

```
troquelGridBase = Y del punto de conexion larguero-poste (PostBeamPoint, FRONTAL)
exit            = gridBase + round((firstLevelHeight - gridBase) / paso) * paso
```

De modo que `0` no significa «el troquel utilizable mas bajo» sino «el troquel mas cercano al cero absoluto», que
puede caer por debajo. Corregirlo a la semantica que el dueño pide —offset sobre el troquel utilizable mas bajo—
cambia el significado de TODOS los valores almacenados: un documento con 4 se reabriria 4" mas arriba.

La seccion I del contrato pide detenerse si no hay forma inequivoca de distinguir documento legacy de documento
nuevo sin una decision de esquema. **No la hay**: ni `PushBackDesignDocument` ni `DynamicRackSystemDocument` llevan
version ni marcador de datum. Ademas la autoridad es COMPARTIDA con Selectivo y Dinamico
(`DynamicLoadBeamGeometry.ResolveLevels`), asi que el cambio no es de Push Back.

Lo que hace falta decidir:
1. si el datum nuevo es exactamente `troquelGridBase` o el primer troquel por encima de la placa;
2. si el cambio aplica a los tres sistemas o solo a Push Back;
3. el marcador de esquema aditivo (`FirstLevelDatum`, ausente = legacy) que permita reabrir lo viejo sin moverlo.

### NO ALCANZADOS en esta corrida

Errores 4 (envolvente de cabeceras por linea fisica), 5 (orientacion del larguero ALTO) y 10 (UX y planta de topes).
Siguen abiertos y sin tocar.

## 4-septies. Decisiones cerradas del dueño: datum de «Alto 1er nivel»

### CERRADO — el datum, y es COMPARTIDO con el Dinamico

**Caracterizacion (lo que se pidio en Q).** La altura del primer nivel la decide un solo sitio:
`DynamicRackSystemResolver`, que lo usan por igual el DINAMICO y el PUSH BACK —el Push Back compone al dinamico—, y
que llama a `DynamicLoadBeamGeometry.ResolveLevels`. La retícula sale del catalogo:

```
gridBase = Y del mate TROQUEL_LARGUERO del poste, vista FRONTAL, resuelta con su peralte
paso     = SelectiveRackDefaults.TroquelPaso
```

Medido con el catalogo vigente y el poste por defecto: `gridBase = 0.6053"`, `paso = 2"`, es decir troqueles en
0.6053, 2.6053, 4.6053… El valor NO sale de ninguna constante y **cada perfil puede tener el suyo**.

**El defecto.** La regla historica trataba el numero como una elevacion ABSOLUTA y la ajustaba al troquel MAS
CERCANO (`SnapToNearestTroquel`). Consecuencias: «0» no significa nada fisico —da «el troquel mas proximo al cero
absoluto»— y con un poste cuya retícula empiece mas arriba puede caer POR DEBAJO del piso. El «4"» observado es el
sintoma de esa arbitrariedad, no una regla de negocio.

**La correccion.** Nueva autoridad NEUTRAL `RackFirstLevelDatum` (Application/Systems/Shared):

- `LowestUsablePunch(gridBase, paso)` — el primer troquel que no queda bajo el piso;
- `RawElevation(valor, datum, gridBase, paso)` — desde donde se cuenta;
- `ToLowestPunchOffset(elevacionFisica, gridBase, paso)` — la conversion inversa, para migrar sin mover.

`ResolveLevels` recibe el datum (por omision, el historico: **ningun llamador que no lo pase cambia**). El
DINAMICO queda incluido porque el dato de usuario es el mismo y lo resuelve el mismo sitio — no son dos offsets
especiales, es una autoridad.

**Compatibilidad.** `FirstLevelDatum` es aditivo y anulable en el diseño, en el documento (`WhenWritingNull`), en el
sistema resuelto y en el snapshot. Ausente = lectura historica. Un rack NUEVO nace con el datum del producto. Las
pruebas comparan GEOMETRIA FISICA, no JSON: un documento historico —Push Back o Dinamico— reabre con el larguero en
la misma elevacion, y la migracion se hace midiendo la elevacion ya resuelta y re-expresandola, sin restar ninguna
constante. Y el marcador sobrevive al SNAPSHOT, que es lo que hace que `RACKEDITAR` no mueva un rack nuevo.

### CERRADO — error 6, la inversion vertical, con los desempates que faltaban

El dueño retiro «verticalmente gobierna el ALTO» y fijo los dos desempates que no tenian equivalente. La regla
vigente, en `PushBackElevations`, es UNA y va en un solo sentido:

1. el larguero de ENTRADA es el ANCLA: conserva EXACTAMENTE el troquel que su nivel le dio desde el datum del
   producto, y no se mueve para mejorar la pendiente;
2. el POSTERIOR se DERIVA: se enumeran los troqueles de la reticula y gana (a) el de menor error de pendiente
   contra 7/192; (b) a igualdad, el mas cercano al ALTO teorico (`contacto bajo + subida nominal sobre la
   longitud real de la cama`); (c) a igualdad, el de menor elevacion.

El tercer desempate anterior —la cercania al resultado PRE-I-32— pertenecia a la seleccion del BAJO y se
RETIRA con ella. `PushBackBedRotation.TheoreticalExitY`, que solo servia a aquel criterio, se elimina; no queda
ninguna ruta viva que fije el alto o elija el bajo.

**Evidencia medida** sobre el escenario de los goldens (inserciones, pulgadas):

| celda | ANTES low | ANTES high | AHORA low | AHORA high | pendiente | resolver exit |
|---|---|---|---|---|---|---|
| F0 N1 | 12.6053 | 16.6053 | 6.6053 | 10.6053 | 0.034398 (igual) | 6.6053 |
| F0 N2 | 84.6053 | 88.6053 | 78.6053 | 82.6053 | 0.034398 (igual) | 78.6053 |
| F1 N1 | 12.6053 | 12.6053 | 6.6053 | 6.6053 | 0.040668 (igual) | 6.6053 |
| F1 N2 | 84.6053 | 84.6053 | 78.6053 | 78.6053 | 0.040668 (igual) | 78.6053 |

El larguero bajo vuelve EXACTAMENTE a la altura pedida —antes estaba 6" por encima— y **la pendiente de cada
cama no cambia**: la celda entera baja, la cama no se reinclina. Es la comprobacion de que se invirtio el ancla
y no se toco el criterio de seleccion.

**Un arreglo que salio con la inversion.** El corte frontal POSTERIOR leia `EntranceElevation` del resolver
compartido, asi que era una SEGUNDA autoridad vertical: el mismo larguero fisico salia en dos troqueles segun
la vista. Ahora consume `PushBackElevations.HighContext`, igual que el bajo consume el suyo, y el override de
elevaciones vale en LOS DOS extremos del builder compartido (sin contexto —el Dinamico, siempre— nada cambia).
`LocateCell` tambien busca contra esa misma elevacion: si no, no encontraba la celda y se perdian en silencio
el tope posterior y el filtro de celdas de una corrida.

**Pruebas.** `PushBackVerticalAuthorityTests` compara PIEZAS DIBUJADAS: el larguero alto sale a la misma
elevacion en el lateral y en el frontal posterior, el bajo tambien, el bajo se queda en la altura pedida sea
cual sea el fondo, y el alto se separa de la del resolver en cuanto el fondo lo pide.
`PushBackHighTieBreakTests` —que SUSTITUYE a `PushBackLegacyTieBreakTests`— re-enumera la reticula por su
cuenta con un rango mucho mas ancho y comprueba que el elegido es el optimo GLOBAL y que los tres desempates
resuelven en el orden del dueño. Cuatro goldens se mueven (los dos laterales y los dos frontales); planta y
BOM quedan intactos, y eso acota el cambio.

### CERRADOS — errores 5 y 10 (geometria): el marco de la CAMA es la autoridad

Los dos tenian la MISMA causa raiz, y se encontro midiendo, no leyendo: la planta pedia las piezas del extremo
ALTO a la planta LOCAL del lado que posee ese extremo. En una CORRIDA eso es falso — el larguero alto de una
corrida no esta en la linea posterior de ese lado, sino al final del recorrido, en el otro extremo del rack—, y
ademas llegaba con la orientacion del marco contrario.

**Medido antes del arreglo**, en un rack de dos ranuras y 792" de largo:

| topologia | extremo ALTO real de la cama | larguero en PLANTA | mano |
|---|---|---|---|
| Corrida A→B | X = 791.8 | X = **396** (la interfaz) | **False**, deberia ser True |
| Corrida B→A | X = 0.2 | X = **396** (la interfaz) | **True**, deberia ser False |
| Solo A / Solo B / Encontradas | interfaz | interfaz ✔ | ✔ |

El corte LATERAL, que si resuelve por cama, decia lo correcto en los cinco casos: las dos vistas se
contradecian, y el tope —que cuelga de ese larguero— se iba con el.

**La correccion.** La planta compuesta ya no compone «por lado» sino **por CAMA**: usa los mismos lotes de
`PushBackCompositeContent.Batches` que el lateral, construye cada cama con el builder de un Push Back de un
sentido EN SU MARCO —el local del lado o el sintetico de la corrida— y la lleva al mundo con UNA sola reflexion
rigida. No hay ninguna regla nueva de orientacion: la mano sale de la reflexion, como en el lateral.

**Consecuencia sobre la orientacion (error 5).** La mano del larguero ALTO ya no puede venir del lado, del
indice del frente ni de «es el ultimo modulo»: la impone el SENTIDO FISICO del flujo. Una cama que avanza hacia
+X lleva su bajo sin espejo y su alto espejado; una reflejada, lo contrario.

**Pruebas** (`PushBackRunFrameTests`, 51 casos). Se comparan PIEZAS DIBUJADAS contra el eje que publica
`PushBackRunGeometry`:

- en el extremo de cada cama hay un larguero con la mano que su sentido impone, y no hay ningun larguero de
  extremo que no acabe en una cama — se comprueba por EXISTENCIA, no emparejando por cercania, porque con camas
  encontradas los dos largueros altos caen en la misma linea de la interfaz;
- planta y lateral coinciden pieza a pieza en X y en mano;
- en planta, todo larguero alto y todo tope pertenecen al extremo alto de una cama real;
- una ranura con un nivel corrido y otro encontradas tiene TRES largueros altos en tres sitios, no dos: es el
  caso donde contar piezas no distinguia el defecto porque dos coincidian;
- el BOM cuenta UN tope por cama con intencion activa en su lado alto — ni uno mas;
- y los casos ESPECIALES que el dueño enumero: frente corto contra largo, hueco que la cama atraviesa, ranura
  presente en un solo lado, en las cuatro topologias y en los dos sentidos.

Gotcha util: el lateral GENERAL dibuja la envolvente del rack, no la cama de cada ranura, asi que con ranuras
heterogeneas hay que mirar los CORTES por poste. Y ninguna vista tiene relacion 1:1 con el BOM — el lateral
general colapsa ranuras, los cortes muestran cada frente en los dos que lo flanquean y la planta colapsa
niveles—, asi que el conteo se contrasta contra las camas, no contra un dibujo.

### CERRADO — error 4: la envolvente se resuelve POR LINEA FISICA TRANSVERSAL

**La regla del dueño**: `RequiredHeaderEnvelope(linea) = la maxima envolvente que exigen los frentes FISICAMENTE
ADYACENTES a esa linea`. Ni el maximo del rack, ni un frente arbitrario, ni un frente remoto que la linea no
sostiene. Una linea INTERMEDIA si se extiende aunque uno de sus dos frentes sea corto, porque sostiene tambien al
otro.

**Lo primero fue MEDIR, y el rack de un solo sentido ya cumplia.** Con frentes 5/8/6/9 las cinco lineas caen
exactamente donde manda la regla (posiciones 5, 8, 8, 9, 9), y la linea exterior del frente de 5 no se alarga
porque exista otro de 9. `DynamicDepthGeometry.AtPost` ya unia los rangos de los frentes adyacentes y la planta,
el BOM y los cortes ya lo consumian. Eso no se toca; ahora queda FIJADO por pruebas.

**El defecto estaba en el COMPUESTO.** `PushBackCompositeLayout.SlotRange` daba la profundidad ENTERA a toda
ranura presente en los dos lados. Medido en un rack de 17 posiciones con A=[5,8,6] y B=[8,8,8], las cuatro lineas
declaraban `1..17` y la planta ponia postes en TODAS las posiciones de cada linea — incluidas las que ninguna de
las dos camas de esa ranura alcanza. Es literalmente «se extienden cabeceras segun los frentes grandes incluso
donde fisicamente no son necesarias».

**La correccion.** Una ranura declara sus TRAMOS de profundidad (`DepthSegments`): lo que demanda su lado A,
pegado al arranque, y lo que demanda su lado B, pegado al final. La cobertura de una linea es la UNION de los
tramos de sus frentes adyacentes, y los materializadores preguntan «esta ESTA posicion cubierta» en vez de «entre
que dos». El rango continuo del frente NO cambia —su claro, su ancho y sus coordenadas siguen siendo los de
antes—; lo que cambia es donde existe su estructura.

Tres casos NO declaran tramos, y en ellos nada cambia:

- una ranura de un solo lado, cuyo rango ya era exacto;
- una ranura con alguna celda CORRIDA: su cama atraviesa la interfaz y necesita apoyos en todo el recorrido;
- una ranura cuyos dos lados llegan a la interfaz: los tramos se juntan a traves de ella, que es lo que sostiene
  el separador central.

Y los tramos de una cobertura se FUSIONAN cuando se tocan o se solapan. Sin eso, dos frentes adyacentes que
comparten profundidad producian dos tramos identicos y un poste de frontera se contaba dos veces; el rack de un
sentido lo detecto de inmediato.

**Resultado medido** (misma configuracion): la linea exterior de la ranura de fondo 5 pierde los postes de las
posiciones 6, 7 y 8, y la de la ranura de fondo 6 pierde el de la 8. Las lineas intermedias, que sostienen
tambien a la ranura profunda, no pierden ninguno.

**SECCION K — la identidad de I-40 se conserva.** No se toco ningun `ModuleId`, `HeaderConfiguration`,
`HeaderLineOverride` ni `DerivedPostLineOverride`: solo cambia QUE modulos materializa cada linea. Hay prueba de
que la secuencia de `ModuleId` y de `Kind` es identica antes y despues de acortar una ranura, y de que un
`HeaderLineOverride` puesto sobre una linea sigue aplicandose despues del recorte.

**Pruebas** (`PushBackHeaderEnvelopeTests`): el ejemplo literal del dueño (5/8/6/9) linea a linea; un frente
remoto que no alarga ninguna; el defecto compuesto medido en la linea exterior; la cobertura continua cuando los
dos lados llegan a la interfaz; la corrida que conserva toda la profundidad; el BOM que deja de cotizar lo que la
planta deja de dibujar; y las dos de identidad de I-40.

### CERRADO — error 10 (UX): intencion, aplicabilidad y materializacion son TRES cosas

El modelo ya era correcto —`PushBackRearTopeConfig` por lado con sus `OffCells`, y no se creo ninguna autoridad
nueva—, pero la superficie no separaba los tres hechos ni decia DONDE iba a aparecer la pieza.

**La superficie es ahora un tipo NEUTRAL en Application**, `PushBackTopeSurface`, que devuelve
`PushBackCompositeEditorState.TopeSurface(ranura, nivel)`: la topologia que la celda tiene de verdad (ya
degradada por presencia), el sentido efectivo, que lado es EFECTIVO y si el extremo alto cae en la linea
INTERIOR o en la EXTERIOR. Calcularla dentro del code-behind habria sido una segunda autoridad, que es de donde
salio este error.

Las diez preguntas del dueño:

| # | Pregunta | Respuesta |
|---|---|---|
| 1 | que casilla es intencion y cual efectividad | la casilla es INTENCION y no se borra; el tooltip dice si hoy es EFECTIVA y, si no, por que |
| 2 | se entiende donde acabara el tope | si: el texto y el tooltip dicen «linea INTERIOR, la del centro» o «linea EXTERIOR del lado alto, al final del recorrido» |
| 3 | los cinco alcances afectan lo esperado | probado sobre los controles reales: celda 1, nivel 3, frente 2, todo 6 |
| 4 | encontradas = dos decisiones independientes | si, y el texto lo dice con esas palabras |
| 5 | SoloA/SoloB deshabilitan el lado no aplicable | si, con su motivo, conservando la intencion |
| 6 | corrida muestra solo el HIGH aplicable | si, y ademas dice en que sentido y donde acaba |
| 7 | cambiar sentido mueve efectividad sin borrar intencion | probado: la efectividad salta A↔B y las dos intenciones siguen guardadas y marcadas |
| 8 | planta coloca el tope en el HIGH real | corregido — ver el error 5/10 geometrico |
| 9 | el espejo de B es correcto | corregido — misma correccion |
| 10 | el BOM cuenta lo materializado | si: un tope por cama con intencion activa en su lado alto |

**Seccion N — una autoridad para todas las salidas.** Lateral, planta, frontal y BOM leen la misma cama: el
lateral y la planta por lote de `PushBackCompositeContent.Batches`, el frontal por el contexto de elevaciones y
el BOM por `PushBackRuns`. No queda ningun builder que calcule su propia X de tope.

**Seccion P — la seguridad no se movio.** El error 10 toca zona vecina, asi que hay regresion explicita: editar
topes no mueve ni una pieza de seguridad (firma completa de la planta antes y despues) y ninguna seleccion
adquiere `Side = Both`. Los seis controles de `PushBackCompositeSafetyOwnershipTests` siguen verdes.

**Seccion Q — error 1 revalidado tras la inversion vertical.** Con A y B identicos las camas de los dos lados
quedan a la MISMA altura hasta la ultima milesima; en cuanto se cambia el «Alto 1er nivel» de B, dejan de
coincidir. La primera afirmacion no pasa por casualidad.

**Pruebas**: `PushBackTopeSurfaceTests` (14) sobre el modelo puro, incluida la que comprueba que lo que la
superficie PROMETE es donde la cama acaba de verdad; y siete pruebas WPF nuevas sobre los controles reales de la
ventana.

## 4-nonies. Validacion del dueño post-5a73b92: RECHAZADA — los siete hallazgos

El dueño convirtio un rack de CUATRO frentes a compuesto, declaro el lado B en uno solo y encontro siete cosas.
Cada una se REPRODUJO antes de tocar nada.

### BLOQUEO — la celda seleccionada era del LADO, no del RACK (hallazgo 4)

Reproducido con los controles reales. El usuario elige el frente mirando el rack (lado A), cambia al lado B para
declararlo, y el selector vuelve a 0:

```
A: combo=1 selA=1  ->  quiere F1: combo=0 selB=0  ->  escribio en: 0
A: combo=2 selA=2  ->  quiere F2: combo=0 selB=0  ->  escribio en: 0
A: combo=3 selA=3  ->  quiere F3: combo=0 selB=0  ->  escribio en: 0
```

Cada lado guardaba su propia celda primaria, asi que la casilla de presencia escribia SIEMPRE en la ranura 0 —
«aparentemente solo F1 puede hacerse compuesto». La celda es una posicion FISICA del rack y existe en los dos
lados: ahora se lleva al lado que se va a editar ANTES de cambiar. Medido despues: `0,1` → `0,1,2` → `0,1,2,3`.

### CAPACIDAD, PRESENCIA y TOPOLOGIA son tres estados (hallazgo 1)

| estado | de quien es | que declara |
|---|---|---|
| CAPACIDAD | del rack | «existe el lado B como posibilidad» |
| PRESENCIA | de cada frente | «este frente tiene fisicamente lado B» |
| TOPOLOGIA | de cada celda | Solo A / Solo B / Encontradas / Corrida |

Tres correcciones, una por eje:

- **Activar la capacidad ya no declara presencia.** El modelo inicializa el lado, iguala la retícula y lo deja
  AUSENTE en todos los frentes. Y mientras ningun frente lo tenga, el rack **no es compuesto**: sigue siendo
  fisicamente el de un solo sentido, misma longitud y mismo BOM.
- **La topologia efectiva es por frente.** El default del rack pasaba a «encontradas» y toda celda sin entrada
  propia lo heredaba, tuviera lado B o no. `TopologyAt` devuelve ahora la que la celda puede construir de verdad;
  la intencion guardada vive en `StoredTopologyAt` y vuelve a mandar en cuanto el frente reciba su lado B.
- **La segunda cara de carga es por LINEA.** `BothEndsAreLoadFaces` era un bool del rack: una cara B que existe
  solo en F1 convertia en cara de carga las lineas de todos los demas frentes, y ahi aparecian las botas y los
  protectores que el dueño vio «invertidos». Ahora la seleccion declara `SecondLoadFacePosts` —las lineas cuyos
  frentes adyacentes tienen lado B— y las demas conservan su regla adaptativa legacy.

### EL DATUM DE LOS DOS LADOS (hallazgo 2)

Medido: con la misma intencion a la vista, el lado A arrancaba en 4" y el B en 6" —dos defaults distintos— y sus
camas quedaban un troquel entero aparte (`lowZ` 6.4836 contra 8.4836). Al crear el lado B ahora parte de la MISMA
intencion de «Alto 1er nivel» que el lado A; no se copia nada mas. Medido despues: los dos en 4" y `lowZ` 6.4836
en los tres niveles.

### FRONTAL B CONTRA LATERAL B (hallazgo 3)

Medido con los dos lados deliberadamente DISTINTOS (A=4", B=18"): el corte frontal de cada lado coincide
exactamente con las elevaciones de sus camas, y el lateral tambien. No habia una segunda autoridad: lo que el
dueño vio era el sintoma del hallazgo 2, con el lado B entero un troquel mas arriba. Queda fijado con pruebas
geometricas del lado B en concreto, porque las simetricas A/B no recorrian esa ruta.

### LA DUPLICACION DE LOS CORTES (hallazgos 5 e I)

Un corte lateral es una PROYECCION y dibuja cada cosa UNA vez. El compositor construye por CAMA, asi que dos
frentes contiguos con la misma configuracion emitian sus largueros, sus apoyos y sus camas superpuestos:

```
linea 2: antes(un sentido) 696/696 distintas    despues(compuesto) 1317/696
linea 3: antes             696/696              despues            1317/696
linea 4: antes             696/696              despues             696/696   (una sola ranura adyacente)
```

Dos piezas identicas una encima de otra se ven como una pieza «con dos manos» y como «dos largueros en el poste
reforzado». Se deduplica por identidad fisica —pieza, posicion, mano y rotacion— ya en coordenadas de rack. No
afecta al BOM, que cuenta camas.

**Y la mano del larguero alto ya era correcta.** Medida contra la regla del dueño —el escalon apunta al CENTRO de
la cabecera a la que se conecta— en las cuatro topologias, los dos sentidos y con frentes cortos contra largos:
correcta en todos los casos. La cabecera es el MODULO en el que la cama termina (el que acaba en esa linea si
avanza hacia +X, el que empieza en ella si avanza hacia −X); elegirla por proximidad es lo que la hacia parecer
invertida, porque en la interfaz terminan DOS cabeceras, una por lado.

### LA CORRIDA SE ANCLABA EN EL LADO EQUIVOCADO (hallazgo 7)

Reproducido con A=10" y B=30": la cama corrida se anclaba en **30.6053** —la altura del lado B— aunque carga por
el pasillo de A, y su desviador se quedaba en 76.6053 siguiendo los niveles de A, veintiseis pulgadas por debajo
de su propia cama.

`BuildCorrida` tomaba las elevaciones del lado ALTO, que era la regla correcta **mientras mandaba el alto**. La
decision final del dueño invirtio la autoridad vertical y esto se quedo atras. Ahora las elevaciones son las del
lado BAJO —el pasillo por el que se carga—; los PERALTES del larguero posterior siguen siendo los del alto,
porque esa pieza esta fisicamente alli. Medido despues: `lowZ` 10.6053, y el desviador a 6" bajo su cama.

### LA BARRA DE ACCIONES (hallazgo 6)

Convivian TRES separaciones escritas a mano boton por boton (6, 8 y 14) y anchos minimos sueltos (74, 86, 158…).
Ahora hay dos tokens de ritmo —separacion entre acciones hermanas y separacion de cambio de grupo— y todos los
anchos son multiplos del paso de 8. El selector de lado frontal ya no fija alto propio: comparte linea con los
botones. Es composicion local; el shell no se toca.

### Pruebas

`PushBackPartialCompositeTests` (14) y `PushBackHighEndHandTests` (34) sobre el modelo, mas seis pruebas WPF con
los CONTROLES REALES: declarar B frente a frente, quitarlo y reponerlo en uno intermedio, que cambiar de lado no
mueve el cursor, y el ritmo de la barra. Las fixtures que quieren el rack compuesto entero ahora lo declaran
frente a frente, que es el contrato nuevo.

## 4-decies. Validacion del dueño post-82e918b: RECHAZADA — los ocho hallazgos

Cada uno se REPRODUJO antes de tocar nada.

### 1. Los topes en PLANTA solo salian en el primer frente

En PLANTA la X corre con la profundidad y la Y con la retícula transversal, asi que un frente se identifica por su
**Y**. Se buscaba «el frente cuyo `EndX` esta mas cerca» y, como TODOS los frentes comparten profundidad, eso
devolvia siempre el mismo. En un rack compuesto —donde cada cama se dibuja sobre una copia con una sola ranura
activa— caia en un frente EN BLANCO, que no tiene niveles efectivos, y no emitia tope.

Medido con 4 frentes: **1 tope en planta; ahora 4** (y 8 con camas encontradas). De paso, el larguero posterior de
cada frente recupera SU peralte: antes todos heredaban el del primero.

### 7. Doble larguero en poste reforzado — estaba en el camino LEGACY

No era el compuesto: en un rack de un solo sentido, los cortes interiores dibujaban el larguero de entrada DOS
veces en el mismo punto, porque dos frentes contiguos que arrancan en la misma posicion lo proyectan uno encima
del otro. Un corte es una PROYECCION y dibuja cada cosa una vez. La deduplicacion que el compositor ya tenia se
subio a `PushBackPlanComposer` y ahora la usan los dos caminos con la MISMA clave fisica.

### 3. El desviador — solo en el pasillo por el que se carga

Un desviador guia la tarima AL ENTRAR, asi que vive en el extremo por el que se CARGA; y cual es ese extremo lo
dicen las CAMAS, no la presencia de un lado.

| topologia | antes | ahora |
|---|---|---|
| Solo A | X=0 | X=0 |
| Encontradas | X=0 (el lado B se quedaba sin el) | X=0 y X=792 |
| Corrida A→B | X=0 | X=0 |
| Corrida B→A | **X=0 — el extremo ALTO** | X=792, su extremo bajo |

El pasillo se declara **por LINEA**, asi que un rack compuesto PARCIAL no lo reparte a las lineas de los frentes
que siguen siendo de un solo sentido.

### 5. Las defensas del otro lado

La defensa de montacargas solo salia en el pasillo de A. El extremo lejano de una linea con segunda cara de carga
recupera su longitud automatica, asi que el otro pasillo lleva la suya. Sin segunda cara —cualquier rack de un
sentido— la regla es la de PB-009 y no cambia nada.

### 6. La orientacion de las botas

La copia de la cara LEJANA repetia la mano de la cercana. Los dos pasillos miran en sentidos opuestos: la pieza
que protege uno esta girada respecto de la que protege el otro. Ahora la copia lejana es la IMAGEN ESPEJO, en
botas y en protectores.

### 2 y 4. El tope y su autoridad

La regla vertical que el dueño describe —«X troqueles arriba del larguero de salida»— **ya existia y es unica**:
`PushBackRearTopeBuilder.ElevationY` = rise-and-snap canonico + DOS troqueles. Lo que estaba mal era la
REFERENCIA: el lateral media desde la elevacion que el resolver compartido dio al nivel, no desde la DERIVADA del
larguero alto. El tope flotaba sobre un larguero que ya no esta ahi y ademas discrepaba del corte frontal, que si
consumia la derivada. Otro resto de la inversion vertical.

### 8. «En blanco» es la unica autoridad visible

Convivian dos controles para la MISMA decision: «En blanco» (I-33) y «Frente presente en este lado». Uno quitaba
cabeceras donde no debia y el otro no, y el segundo solo se comportaba bien con el lado activo en «Ambos».

Ahora la intencion es una: un frente EN BLANCO conserva su claro y su estructura y no lleva ninguna carga en ese
lado, que es exactamente lo que significa «este frente no tiene lado B». **La presencia por lado se DERIVA de
ella** y el checkbox duplicado se retiro de la ventana. Tres consecuencias:

- funciona con cualquier lado activo, no solo con «Ambos»;
- un lado puede quedarse ENTERO en blanco mientras el otro sostenga el rack —es la capacidad declarada y sin
  usar—, pero dejar el RACK entero en blanco se rehusa: la guarda es del rack, no de cada lado;
- una ranura en blanco en los DOS lados sigue existiendo como frente en blanco. Antes se saltaba, y eso encogia la
  retícula transversal y corria todos los frentes siguientes — justo lo que «en blanco» promete no hacer.

### Goldens

Tres pines, con evidencia y acotados: **planta** (el peralte del larguero posterior del frente 1 pasa de 5" a
3.5", que es el suyo) y los **dos laterales** (los topes bajan 6" hasta su larguero real, de 16.6053/88.6053 a
10.6053/82.6053 en el frente 0). El frontal posterior y el BOM quedan intactos.

### Pruebas

`PushBackTopePlantaAndDiverterTests` (18) y `PushBackBlankFrontAuthorityTests` (9), mas la migracion de las
pruebas de ventana a la casilla «En blanco». Nada de `NotEmpty`: los topes de la planta se cuentan POR BANDA
TRANSVERSAL de cada frente, el desviador por extremo de pasillo y la seguridad comparando las manos de los dos
pasillos.

## 5-pre-ter. Ronda 8C (S1): los protectores de bota eligen UBICACIONES, no espejos

Ultima deuda funcional de I-42, registrada desde la ronda 6F. Una bota protege el POSTE del golpe del montacargas, y
el montacargas ataca por un PASILLO: la pregunta que el selector debe responder es **que cara de ataque proteger**.

### La causa, medida

Dos defectos, en dos capas distintas:

1. **La eleccion del usuario nunca llegaba.** `PushBackSafetyAuthority.RestrictToLowEnd` colapsaba el lado general a
   `Izquierda` antes de que nadie lo leyera. Resultado medido en un rack de dos ranuras:

   | | ANTES Izquierda | ANTES Derecha | ANTES Ambas |
   |---|---|---|---|
   | Push Back simple | 3 botas · BOM 3 | **3 · 3 (identico)** | **3 · 3 (identico)** |
   | Push Back compuesto | 6 · 6 | **6 · 6 (identico)** | **6 · 6 (identico)** |

   El selector era **inerte**: las tres opciones daban exactamente la misma bota.

2. **`Ambas` duplicaba ubicaciones.** Con dos caras de carga, la autoridad compartida emitia CUATRO copias sobre DOS
   sitios — dos piezas dibujadas y contadas sobre el mismo poste. No se veia porque el colapso impedia llegar ahi,
   pero estaba en la autoridad.

La raiz es la de siempre en esta iniciativa: **un solo eje diciendo dos cosas**. `SafetySide` mezclaba PERTENENCIA
(que ubicacion se protege) con ORIENTACION (como va la pieza), y al restringir el extremo se perdia la primera.

### El contrato

| opcion | ubicaciones fisicas |
|---|---|
| Ninguno | ninguna |
| Izquierda | el extremo CERCANO |
| Derecha | el extremo LEJANO |
| Ambas | los dos, **una vez cada uno** |

Y una ubicacion **solo existe donde ese extremo es una CARA DE ATAQUE**: el cercano siempre lo es; el lejano lo es en
Selectivo y Dinamico —que cargan por los dos extremos— y en un Push Back COMPUESTO, donde se declara POR LINEA. El
extremo lejano de un Push Back de un solo sentido esta contra muro y no se protege.

La **ORIENTACION** es un eje aparte y la decide la cara (`SelectiveSafetyEnds.Mirror`): los dos pasillos miran en
sentidos opuestos, asi que la pieza del lejano es la imagen espejo de la del cercano. Cambiarla no crea ni mueve
ninguna proteccion — que es exactamente lo que la ronda 6E confundio.

### Medido, despues

| | Izquierda | Derecha | Ambas |
|---|---|---|---|
| Push Back simple | 3 · BOM 3 | **0 · 0** (muro) | 3 · 3 |
| Push Back compuesto | **3 · 3** (pasillo A) | **3 · 3** (pasillo B) | **6 · 6** (union) |

Tres conjuntos fisicos DISTINTOS y disjuntos, `Ambas` es su union exacta, y ninguna opcion pone dos botas sobre el
mismo poste. Dibujo = BOM en las cuatro. Un blanco sigue quitando la necesidad sin mudar nada: ninguna ubicacion
NUEVA aparece, y ninguna cae en la interfaz.

### Contencion: cada familia con su contrato

La bota NO es la unica familia que lee ese lado, y las otras tienen contratos validados que esta ronda **no toca**:

| familia | lee Izquierda/Derecha como | ronda |
|---|---|---|
| **bota** | UBICACION fisica (cara de ataque) | S1, esta ronda |
| **protector lateral** | ORIENTACION en su sitio | I-32, intacto |
| **desviador** | siempre en el extremo bajo | R1, intacto |

Por eso el colapso del lado general se CONSERVA —las otras dos familias lo necesitan— y lo que se añade es la
eleccion original (`AuthoredSide`), derivada y no persistida como `LowEndOnly`, que solo lee la bota. Una entrada POR
POSTE nunca se colapsa: es del usuario y se lee literal.

**La suite completa paso sin tocar una sola prueba existente** (4014 nucleo · 974 UI), salvo una que afirmaba
literalmente el defecto —«un Derecha explicito aterriza en el extremo bajo»— y ahora afirma que no coloca nada donde
no hay pasillo, y que una eleccion que si pide el pasillo lo sigue colocando en su poste.

### LEGACY

El campo nuevo es ADITIVO y nulo por omision: toda seleccion que no pase por una autoridad restrictiva —Selectivo,
Dinamico, y todo documento anterior— se lee por su lado de siempre. Los DEFECTOS no cambian: un rack nuevo abre en
`Ambas`, asi que un Push Back simple sigue protegiendo su pasillo y un compuesto los dos, sin pedirlo (R6). Lo unico
que cambia es lo que antes era imposible: una eleccion explicita de `Izquierda` o `Derecha` ahora significa algo.

### PENDIENTE

Ninguno registrado. S1 queda cerrado.

## 5-pre-bis. Ronda 8B: un corte muestra EL APOYO que coincide con su plano

La Owner Validation confirmo V1 —las etiquetas laterales— y rechazo V2. La ronda 8 habia concluido que la
asignacion de los cortes ya era correcta; el dueño reprodujo el caso que demuestra lo contrario y fijo el contrato.

### La causa: la vista inferia el papel de su NOMBRE

«Frontal» y «Posterior» dicen DONDE esta el plano de corte, no que papel tiene la pieza que aparece alli. El
pipeline los usaba como si fueran roles: `Posterior(lado)` = «el extremo alto de ese lado», `EntradaSalida(lado)` =
«su extremo bajo». De ahi salian los tres errores, medidos en el escenario del dueño —compuesto A+B, un frente en
blanco en A, nivel 1 en corrida A→B:

| corte | la cama corrida ahi… | ANTES | AHORA |
|---|---|---|---|
| Frontal A | empieza | BAJO ✓ | BAJO |
| Posterior A | solo pasa | **nada** | **INTERMEDIO** |
| Posterior B | solo pasa | **ALTO + TOPE** | **INTERMEDIO** |
| Frontal B | termina | **nada** | **ALTO + TOPE** |
| Posterior A, nivel 3 corto | ya termino antes | **ALTO + TOPE** | **nada** |

Medido pieza a pieza sobre el mismo diseño: `Posterior A` pasa de `REDONDO x5 + TOPE x5` a
`INFINITO x1 + REDONDO x5 + TOPE x5`; `Frontal B` de `IN_OUT x8` a `IN_OUT x8 + REDONDO x1 + TOPE x1`;
`Posterior B` de `REDONDO x9 + TOPE x9` a `INFINITO x1 + REDONDO x8 + TOPE x8`.

### La autoridad: `SupportAtCut`

`PushBackRunSupports` responde, para cada cama y cada plano, cual de sus apoyos coincide:

| relacion del corte con la cama | resultado |
|---|---|
| antes de su bajo | NADA |
| en su bajo | BAJO |
| entre apoyos, dentro de su tramo | INTERMEDIO |
| en su alto | ALTO |
| despues de su alto | NADA |

No es una regla nueva: es la que el constructor del larguero intermedio ya aplicaba en el lateral —una cama ocupa
desde el arranque de su frente hasta la X de su larguero posterior (`PushBackCellDepth.RearX`)— dicha una sola vez y
consultable por las cuatro vistas. **Un plano no muestra un larguero porque la cama pase cerca: tiene que haber un
apoyo fisico en esa frontera.**

La identidad manda sobre la coordenada. Cuando el hueco es CERO las dos lineas interiores comparten X y siguen
siendo dos lineas fisicas distintas: quien desempata es el LADO al que pertenece el extremo, no el numero. Una
corrida las atraviesa las dos, y las dos muestran su apoyo intermedio. La tolerancia solo compara coordenadas que ya
denotan la misma frontera; nunca elige que apoyo es.

### El corte se arma por PAPEL, no por nombre

`PushBackCompositeFrontal` construye cada corte en TRES pasadas sobre el mismo builder de un solo sentido, una por
papel, con las celdas que a cada uno le corresponden. El marco lo aporta la primera; de las otras se toman solo sus
piezas. La SEGURIDAD sigue siendo del pasillo: solo la lleva la cara exterior.

El apoyo intermedio se materializa con el larguero intermedio de esa frontera —su pieza y su peralte, por las
autoridades que ya existen— y **nunca** con un tope: el tope pertenece al alto y solo al alto, asi que
`TopeAt(...)` exige `SupportAtCut == HIGH` y no hace ninguna busqueda propia. De ahi se siguen solos los tres casos
del dueño: nivel 3 corto sin apoyo → sin tope; nivel 1 en la linea interior → intermedio, sin tope; nivel 1 en la
cara exterior de B → alto, con tope.

Su ELEVACION es la interpolacion entre las de sus dos extremos, que las dos autoridades de corte ya dan: la cama es
una rampa recta, asi que un punto intermedio esta sobre la recta que las une. No es una tercera regla de elevacion.

### Lo que NO cambio

La fisica no se recalcula: ni la mano del alto, ni su posicion, ni el ancla del tope, ni la aplicabilidad. La ronda
solo decide QUE APOYO proyecta cada plano. **El inventario no se mueve**: las piezas fisicas ya existian y el BOM es
el mismo. Intactos R1–R7E, V1 y V3 de la ronda 8.

### Contratos de prueba retargeteados

Treinta y dos pruebas afirmaban el corte SUPERSEDIDO —«el alto esta en el posterior de su lado»—. Su INTENCION se
conserva entera y ahora consultan la autoridad en vez de fijar el corte: la agreement frontal/lateral pregunta en
que corte termina la cama, la de topes pregunta si algun plano coincide con su alto, y las de la ronda 8 pasan a
afirmar el contrato nuevo. Una cama que termina DENTRO del rack no aparece en ningun frontal — y eso tambien se
afirma: cada corte dibuja exactamente tantos largueros altos como camas terminan en el.

### PENDIENTE

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas.

## 5-pre. Ronda 8: higiene de vista (V1, V2, V3)

Las vistas son CONSUMIDORAS del modelo fisico. No vuelven a decidir que run existe, que lado existe, que extremo es
alto o bajo, ni que tope aplica: preguntan que elementos fisicos intersectan su proyeccion y los representan. Esta
ronda cierra los tres pendientes de higiene que quedaban registrados.

### V1 — la letra de un lado que ahi no almacena

**Reproducido.** Un compuesto de cuatro ranuras con A en blanco en la 2, B en blanco en la 3 y los dos en blanco en
la 4. Todos los cortes laterales —incluido el de la ranura sin ningun lado— salian rotulados «A» y «B».

**Causa.** `PushBackSideAnnotations` rotulaba todo lado DECLARADO (`PushBackSideSystem.IsPresent`), que es una
propiedad del RACK: dice que ese lado existe en alguna parte. Y la funcion nunca recibia QUE ranuras muestra el
corte, asi que no podia distinguir. Tres conceptos distintos se habian colapsado en uno:

| concepto | quien lo declara | que autoriza |
|---|---|---|
| la ranura fisica existe | la reticula transversal (ronda 2) | postes, placas, cabeceras |
| el lado esta declarado | `IsPresent` | que el rack sea compuesto |
| ese lado ALMACENA aqui | los niveles efectivos de su frente en esa ranura | **la letra** |

**Correccion.** `PushBackFunctionalSides` responde la tercera pregunta, reutilizando la autoridad de blanco que ya
usan el resolver, el BOM y el resto de los constructores —la regla no se duplica—, y el lateral le pasa las mismas
ranuras que gobiernan su contenido.

| corte | ranuras que muestra | A almacena | B almacena | antes | ahora |
|---|---|---|---|---|---|
| poste 1 | 1 | si | si | A,B | A,B |
| poste 2 | 1,2 | si (1) | si (1,2) | A,B | A,B |
| poste 3 | 2,3 | si (3) | si (2) | A,B | A,B |
| poste 4 | 3,4 | si (3) | no | A,B | **A** |
| poste 5 | 4 | no | no | A,B | **(ninguna)** |

La planta sigue la misma regla sobre el rack entero. **La geometria no se toca**: postes, placas, cabeceras, camas,
largueros y topes son la misma pieza en la misma posicion — la correccion solo quita o pone una anotacion, y las
anotaciones nunca entran al BOM.

### V2 — la cara de salida de una corrida

**Auditado y medido.** El contrato ya se cumple, y la ronda lo fija con pruebas en vez de reescribirlo. La autoridad
no es el lado ni el sentido: es que extremo del run cae en cada cara, que es lo que `PushBackCompositeFrontal`
pregunta (`LowSide == side` para el corte de entrada, `HighSide == side` para el de salida).

| escenario | A entrada | A salida | B entrada | B salida |
|---|---|---|---|---|
| corrida A→B full-span | IN/OUT x6 | — | — | **REDONDO x6 + TOPE x6** |
| corrida B→A full-span | — | **REDONDO x6 + TOPE x6** | IN/OUT x6 | — |
| corrida A→B corta | IN/OUT x6 | — | — | REDONDO x6 + TOPE x6 |
| corrida A→B, tope «Ninguno» en B | IN/OUT x6 | — | — | **REDONDO x6, sin tope** |
| encontradas | IN/OUT x6 | REDONDO+TOPE x6 | IN/OUT x6 | REDONDO+TOPE x6 |

La cara de salida NO se trata como otra entrada: el lado alto no proyecta ningun IN/OUT y el lado bajo no proyecta
ningun REDONDO. Una corrida corta no inventa nada en el exterior opuesto: su alto sigue perteneciendo a su lado
alto, con las mismas piezas. Y la frontal de salida **no recalcula** mano ni ancla: proyecta las que las rondas 5B y
5D fijaron, comprobado contra esas mismas autoridades.

**Encontradas no son corrida:** son dos runs, con dos altos y dos topes, y ninguno se colapsa por proyeccion — el
mismo rack en corrida tiene exactamente la mitad de topes.

### V3 — auditoria de proyeccion

**NEAREST.** Dos usos con riesgo semantico, los dos clasificados:

| uso | que resuelve | veredicto |
|---|---|---|
| `NearestColumn` (corte bajo) | la columna transversal de un larguero | **legitimo**: las columnas teselan el eje, la mas cercana es la unica posible y una instancia ya dibujada no lleva mas identidad |
| `NearestLowLevel` (corte bajo) | el nivel de un larguero | legitimo, y ya exigia coincidir dentro de tolerancia |
| `PostMateWorld` (tope) | el POSTE que sirve de ancla | riesgoso pero **Owner-validado** (rondas 4B/5/5D) y fail-CLOSED: sin poste no coloca nada. La ronda 8 no lo toca (§10) y añade guardias de coherencia planta/lateral |

**DEDUP.** No hay ninguna deduplicacion por coordenada en el pipeline de vistas Push Back. Las dos agrupaciones que
existen usan IDENTIDAD SEMANTICA —`(Source, SourceFrontIndex, Reflected)` y niveles distintos dentro del grupo—, asi
que dos camas fisicas distintas no pueden fundirse aunque se proyecten encima. Fijado por prueba.

**FAIL-OPEN.** Uno encontrado, en el filtro por celda del corte bajo: un larguero que no se puede atribuir a ninguna
celda se CONSERVA. Se mantiene el comportamiento —dejarlo caer borraria geometria legitima por un fallo de
correspondencia, que es peor— pero deja de ser una via silenciosa: la identificacion se extrae a `IdentifyCell`, con
un resultado explicito, y `UnidentifiedEndBeams` la hace medible. Una prueba de guardia comprueba que en el rack
compuesto —el unico que filtra por celda— no se ejerce nunca, en las tres topologias y con un blanco de por medio.

**PREVIEW.** El preview del editor y el dibujo final salen del MISMO ensamblador de vistas sobre el mismo diseño; se
comprueba pieza a pieza sobre el corte de salida.

### Lo que NO cambio

**Ningun golden se movio, y ninguna primitiva fisica cambio.** El unico delta observable de la ronda son las letras
A/B que dejan de afirmar un almacenamiento inexistente. BOM sin cambios: las anotaciones no entran, y V2 y V3 no
alteran ningun inventario. Intactos R1–R7E completos.

### PENDIENTE

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas. Sigue separada.

## 4-octvicies. Ronda 7E: el TIPO de defensa vive en la seccion de su lado

La Owner Validation de 7D confirmo las secciones por lado, la configuracion por poste, la independencia A/B y que los
blancos no contaminan el lado contrario. Quedaba un problema de contrato: las secciones configuraban POSTES, pero el
TIPO de defensa seguia eligiendose en una fila general al fondo de la ventana. Una misma decision partida en dos
sitios y, en un compuesto, una sola para los dos pasillos.

El dueño fijo el contrato final: **cada seccion contiene su tipo, su «Ninguno» y su configuracion por poste**, como
las de topes; y el tipo es INDEPENDIENTE POR LADO, con el modelo preparado para mas de un tipo futuro.

### La identidad del tipo: la CARA lo declara

La ronda 7D ya habia establecido que un compuesto tiene dos pasillos —el extremo cercano y el lejano de la cobertura
de cada linea— y que el lado A ataca por uno y el B por el otro. Lo que faltaba era que cada pasillo pudiera decir
QUE pieza usa. `SafetyFacePiece` lo expresa con tres estados y solo tres:

| estado | significado |
|---|---|
| la cara no declara nada (null) | usa el `ElementId` de la seleccion — el comportamiento historico y el de todo documento anterior |
| `None` | esa cara no lleva ninguna pieza |
| una pieza | esa cara lleva esa |

Es DERIVADA y no se persiste, exactamente como `LowEndOnly` y `BothEndsAreLoadFaces`: quien la persiste es el diseno
de cada lado (`PushBackDesign.DefensePieceId` y `PushBackSideDesign.DefensePieceId`), y la autoridad del sistema la
vuelve a imponer en cada limite que posee, asi que ningun documento puede traerla rancia.

`«Ninguno»` NUNCA es una pieza: no hay `PieceId = NINGUNO` en el BOM, ni bloque ficticio, ni primitiva vacia. Se
traduce a «esa cara no resuelve ningun id», y los tres constructores —lateral, frontal y planta, que es la que
alimenta el BOM— se detienen ahi. Ese `«(ninguno)»` es ademas UN solo valor, compartido con el tope de la ronda 7C:
son dos lecturas de la misma decision sobre familias distintas, y es un valor persistido, asi que dos constantes
iguales acabarian separandose.

### Cuatro ejes, separados

| eje | quien decide |
|---|---|
| **TIPO** del lado | el selector de su seccion |
| **INTENCION** por poste | la rejilla de su seccion |
| **APLICABILIDAD** | la fisica: ¿existe esa cara de ataque? (`DynamicDefenseFaces`, ronda 7D) |
| **COLOCACION** | la geometria |

Y la regla: hay pieza en `(lado, linea)` **si y solo si** el tipo del lado no es «Ninguno», su intencion por poste lo
pide y la cara existe.

### La configuracion queda DORMIDA, no se destruye

Cambiar el tipo no toca la rejilla por poste: son dos ejes. Poner un lado en «Ninguno» deja de materializar, y
volver a elegir una pieza devuelve exactamente los postes que habia — incluso con los DOS lados en «Ninguno», donde
la familia se conserva solo como portadora de esa intencion y no dibuja ni cuenta nada.

### La fila general se retira para Push Back

No basta con ocultarla: la familia DEFENSA sale de la lista que la ventana ofrece, y las secciones pasan a ser
quienes traen la seleccion a existencia. Mientras algun lado tenga pieza, la familia existe; si los dos dicen
«Ninguno» y nadie ha decidido postes, no existe. Los demas sistemas que usan esa fila no cambian en nada: el
contenido de la lista lo decide cada ventana.

### Medido

| escenario | dibujo | BOM |
|---|---|---|
| A = Ninguno, B = Defensa | solo las caras de B | igual al dibujo |
| A = Defensa, B = Ninguno | solo las caras de A | igual al dibujo |
| ambos Defensa | cada lado con SU patron por poste (7D intacto) | igual al dibujo |
| ambos Ninguno | ninguna | 0, y ninguna linea con id `(ninguno)` |

Un frente en blanco sigue quitando APLICABILIDAD sin mover nada: la linea afectada pierde la cara de SU lado, la
apagada a mano sigue apagada, y el otro lado conserva las suyas. Cancelar no persiste ni el tipo ni los postes;
aceptar persiste los dos. Guardar y recuperar devuelve los tipos de A y de B por separado, en los dos sentidos.
Abrir Seguridad con el lado activo en A o en B da lo mismo.

**LEGACY:** un rack que nunca eligio tipo dibuja exactamente lo que dibujaba, y su seccion abre en la pieza que ya
usaba —no en «Ninguno»—: la ausencia de eleccion no es una eleccion. En cuanto el usuario acepta con la nueva UI, el
tipo queda explicito por lado.

### Lo que NO cambio

Ninguna regla fisica ni ninguna posicion. **Ningun golden se movio.** Intactos R1–R6, los contratos utiles de R7, y
todo 7B/7C/7D —incluidos el blanco de topes acotado al lado, el «Ninguno» de topes y los patrones opuestos por
poste—, con pruebas de regresion propias. El flag `defensaPerPostElsewhere` que la ronda 7D habia introducido queda
retirado: era un paso intermedio que esta ronda supera al sacar la familia entera de la lista.

### PENDIENTES REGISTRADOS

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas.
- **V1 — View hygiene:** el lateral de una seccion dibuja la letra de un lado que no existe funcionalmente.
- **V2 — Frontales:** una corrida full-span debe mostrar en la cara de salida el HIGH y el tope con semantica de
  vista posterior.
- **V3 — Planta / view hygiene:** auditoria de nearest-Y, deduplicacion y fail-open.

## 4-septvicies. Ronda 7D: la defensa se edita POR LADO, como los topes

La Owner Validation de 7C confirmo los topes —el blanco de un lado ya no contamina el otro, «Ninguno» existe— y
confirmo que el ON/OFF por poste atraviesa la ruta completa. Pero seguia aplicandose siempre al lado A, y el dueño
fijo el contrato de UI que faltaba: **la defensa debe trabajar como los topes**, con una superficie por lado dentro
de «Elementos de seguridad», y la ventana principal fuera de la decision.

### Donde se perdia el lado: en ninguna frontera

La auditoria no encontro ningun sitio donde el lado se cayera de camino al dibujo. Encontro algo mas simple: **el
lado no estaba expresado en ninguna parte de la UI.** Medido en un compuesto de tres ranuras:

| lo que ofrecia Seguridad | |
|---|---|
| superficies de defensa | UNA, la fila de la familia |
| columnas de su rejilla | «Entrada/Salida» y «Posterior» — el vocabulario de un rack de un solo sentido |
| lo que movia la columna baja | `-4.75\|53.494`, la cara del lado A |
| lo que movia la columna alta | nada: se pintaba con la regla de un rack de un sentido, salia apagada aunque el lado B si llevara defensa, y la casilla ya marcada no disparaba ningun evento |
| lo que decidia el lado activo de la ventana principal | nada — abrir desde A o desde B daba secciones y dibujo identicos |

Es decir: la unica columna operable era la del lado A, y el lado B no tenia donde editarse. Por eso toda decision
acababa en A.

### La identidad: lado + linea fisica, sin almacen nuevo

La ronda 6D ya habia establecido que la seguridad es del RACK y que un compuesto tiene DOS pasillos, uno en cada
extremo de la cobertura de cada linea; el constructor coloca el del cercano con `ExitLength` y el del lejano con
`EntranceLength`. **El registro por poste ya distinguia las dos caras, con un campo independiente para cada una.** Lo
que faltaba era NOMBRARLAS por lado. `PushBackDefenseSides` es donde eso esta escrito, una sola vez:

- el lado **A** ataca por el extremo **cercano**; el lado **B**, por el **lejano**;
- `LengthOf` / `AutoOf` / `Set` leen y escriben la cara de un lado y **nunca** tocan la otra;
- `Merge` funde lo que decidio una superficie sobre la cara de su lado, dejando la del otro exactamente como estaba.

No hay codificacion: ni desplazamientos de indice, ni signos, ni coordenadas redondeadas, ni GUIDs. Es la misma
pareja de campos que el dibujo lee, con el nombre fisico que le corresponde en un rack compuesto.

### La aplicabilidad deja de estar escondida en el bucle que dibuja

`DynamicDefenseFaces` extrae —sin cambiarla— la regla de 6D: una cara existe si la linea existe y ese extremo mira a
un pasillo y no al interior del rack. La consumen el constructor de planta **y** la UI, asi que la rejilla no puede
volver a pintar «apagado» donde el rack si lleva defensa. Es neutral: un rack dinamico no declara ningun tramo
interior y responde que si, igual que antes.

Sobre lo que la estructura resuelta todavia no conoce, la aplicabilidad es **fail-open**: deshabilitar una fila por
una lectura vieja le quitaria al usuario una decision que el rack si admite, y la fisica vuelve a filtrar al dibujar.

### La UX, con el patron de los topes

En un compuesto «Elementos de seguridad» ofrece ahora, en este orden:

```
Defensa de montacargas — lado A     [Configurar…]
Defensa de montacargas — lado B     [Configurar…]
Topes posteriores — lado A          [Configurar…]
Topes posteriores — lado B          [Configurar…]
```

Cada seccion tiene su titulo, su estado legible, su **copia de trabajo** y su boton, y aplica a SU lado al aceptar.
La rejilla que abre recibe la cara **explicitamente** —nombre, extremo y en que lineas existe— y muestra una sola
columna, la de ese lado; un poste sin cara de ataque ahi aparece deshabilitado y dice por que. La fila de la familia
deja de llevar boton por poste: la decision se toma en un solo sitio, como el dueño exigio para los topes en 7B.

Un rack de UN SOLO SENTIDO ofrece **una** seccion, sin etiqueta de lado, y conserva la rejilla historica de los dos
extremos: PB-009 dejo el posterior apagado por defecto pero nunca prohibido, y esta ronda no viene a quitar esa
capacidad.

### Medido por la ruta real

Con las ventanas reales —Push Back → Seguridad → rejilla de cada seccion → aceptar → commit → resolve → dibujo:

| | linea 1 | linea 2 | linea 3 | linea 4 |
|---|---|---|---|---|
| pedido en el lado A | ON | OFF | ON | OFF |
| pedido en el lado B | OFF | ON | OFF | ON |
| registros | `P0[A=auto B=0]` | `P1[A=0 B=auto]` | `P2[A=auto B=0]` | `P3[A=0 B=auto]` |
| dibujado | `-4.75\|0` | `796.75\|53.494` | `-4.75\|106.988` | `796.75\|160.482` |

Cuatro piezas, las cuatro pedidas, ninguna mas. BOM = dibujo. Abrir Seguridad con el lado activo en A o en B da
secciones, registros y dibujo identicos.

Un frente en blanco en A retira la cara de A en esa linea y **no** toca la de B; la intencion guardada no se mueve
de linea ni cambia de lado. Cancelar no persiste ninguna de las dos secciones; aceptar persiste las dos. Guardar y
recuperar devuelve los registros campo por campo. Al encoger el rack, las lineas que conservan identidad conservan
su intencion **por lado**, la retirada no deja fantasma y una nueva nace con el automatico.

### Lo que NO cambio

Ninguna primitiva ni regla fisica: la regla de 6D es la misma expresion, movida a un sitio donde la UI tambien puede
leerla. **Ningun golden se movio.** Intactos: R1, R2 (los blancos conservan su ranura y la reticula no se compacta),
R3, R4/R5 (topes por run, StopA/StopB, HIGH, orientacion, anclaje, poste reforzado), R6 (27/27, alturas A/B,
cabeceras, botas), los contratos utiles de R7, y todo lo que 7B/7C dejaron en los topes —incluidos el blanco acotado
al lado y «Ninguno»—, con pruebas de regresion propias.

Las doce pruebas E2E de 7C conducian la superficie retirada; su INTENCION se conserva entera, reescrita sobre la
seccion, que es lo que el usuario tiene delante.

### PENDIENTES REGISTRADOS

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas.
- **V1 — View hygiene:** el lateral de una seccion dibuja la letra de un lado que no existe funcionalmente.
- **V2 — Frontales:** una corrida full-span debe mostrar en la cara de salida el HIGH y el tope con semantica de
  vista posterior.
- **V3 — Planta / view hygiene:** auditoria de nearest-Y, deduplicacion y fail-open.

El pendiente que 7C registro sobre la columna del extremo alto queda **CERRADO**: con una seccion por lado, la
rejilla de cada lado representa su propia cara y su propia aplicabilidad.

## 4-sextvicies. Ronda 7C: los tres defectos de la Owner Validation de 7B

La ronda 7B tenia CI verde y su reorganizacion funcionaba, pero la Owner Validation encontro TRES defectos. Ninguno
se ha corregido a ojo: de cada uno se reprodujo el sintoma, se midio la causa por la ruta real y se corrigio ahi.

### Defecto 1 — un blanco de un lado ponia en blanco el otro

**Sintoma.** Con un frente EN BLANCO en el lado A, ese mismo frente salia en blanco tambien en la seccion del lado B
dentro de «Elementos de seguridad», aunque B existiera fisicamente ahi.

**Causa, medida.** Las dos secciones abrian su rejilla con la MISMA lista de niveles, la del lado ACTIVO:

| lado | `EffectiveLevelCounts` | lo que recibia su rejilla (antes) |
|---|---|---|
| A (activo, F2 en blanco) | `[3,0,3]` | `[3,0,3]` |
| B (los tres frentes) | `[3,3,3]` | `[3,0,3]` ← el blanco de A |

**Correccion.** Cada seccion abre con los niveles de SU lado (`RearTopeLevels(side)`), y el seam de prueba lleva esa
lista para que una prueba vea exactamente lo que recibe la rejilla real. Ahora A recibe `[3,0,3]` y B `[3,3,3]`.
Cambiar el lado activo no cambia ninguna de las dos: el activo es contexto, no autoridad. Y el blanco no compacta el
indice del frente fisico —las dos rejillas siguen teniendo tres columnas—, asi que las dos hablan del mismo rack.

### Defecto 2 — cambiar el ON/OFF por poste no cambiaba nada (BLOQUEANTE)

**Sintoma.** Apagar o encender un poste desde la rejilla real no producia ningun cambio en el rack. El dueño rechazo
de antemano cualquier prueba de nivel bajo como evidencia: el contrato por poste de 7B pasaba, luego el fallo estaba
en el CABLEADO.

**Traza, frontera por frontera.** Se recorrio el dato con la ventana real, no con delegados:

| frontera | esperado | observado (antes) |
|---|---|---|
| rejilla: la fila del poste | casilla ON, «Auto» ON | igual |
| gesto: apagar la casilla | el extremo pasa a ser del usuario | «Auto» sigue ON |
| `SafetyDefensaGridWindow.OnOk` | escribe un registro | **descarta la fila entera** |
| `SelectiveSafetyWindow.Result` | lleva el registro | sin registro |
| `safetySelections` / diseño | idem | sin registro |
| dibujo y BOM | una defensa menos | **identicos** |

La causa es la tercera fila: una fila SIN registro nace automatica en los dos extremos, y `OnOk` descartaba toda fila
con los dos «Auto» marcados —«todo automatico = ningun registro»— sin mirar la casilla. La casilla era un adorno.

**Correccion.** La CASILLA es el ON/OFF y manda sobre «Auto»: un extremo cuya casilla cambio respecto de lo que la
fila mostro deja de ser automatico, y se escribe. Apagado = longitud CERO explicita. Tocarla desmarca «Auto» en el
acto, para que la decision se vea; y encender un extremo cuyo automatico era cero propone la longitud que la regla da
para ese poste, en vez de dejar un cero que solo podia terminar en el error de validacion.

**Medido por la ruta real**, con una linea base que recorre la misma ruta sin tocar nada:

| paso | defensas en planta |
|---|---|
| linea base | 8 |
| apagar el extremo bajo del poste 2 | 7 — falta exactamente `-4.75\|53.494` |
| volver a encenderlo | 8 — identidad |

Dibujo y BOM se mueven igual. Y la casilla aparece apagada al reabrir la rejilla: la decision existe tambien en la
pantalla, no solo en el modelo.

**Un segundo hallazgo, dentro del mismo defecto.** Al hacerlo visible salio a la luz que el viaje de ida y vuelta
PERDIA la cara lejana: `PushBackSafetyAuthority.RestrictToLowEnd` borraba la marca de automatico del extremo lejano
de todo registro. Se justificaba en que «un automatico lejano volveria a 12/36», y eso dejo de ser cierto en cuanto
PB-009 llego al plan: hoy `DynamicForkliftDefensePlan` ya resuelve ese automatico a CERO por la marca `LowEndOnly`.
Borrarlo no defendia de nada, y en un rack COMPUESTO —donde ese extremo es un pasillo de verdad (ronda 6D)— lo
convertia en un cero explicito que ya no volvia. El borrado se retira; la defensa que imitaba la sigue dando la
regla. Un rack de un solo sentido no cambia en nada, y un extremo lejano fijado A MANO se sigue honrando.

### Defecto 3 — no existia «Ninguno»

**Sintoma.** La unica forma de quitar el tope era apagar celda por celda, y despues no habia manera de leer la
decision: la ausencia se confundia con «todavia no lo he tocado».

**Correccion.** «Ninguno» es ahora una opcion EXPLICITA del mismo selector de tipo, siguiendo el patron visual que ya
existia. Es del OBJETIVO de su seccion —en un compuesto hay una por lado, asi que elegirlo en A no dice nada de B—,
se persiste como cualquier id y sobrevive a RACKEDITAR. La seccion lo DICE en su estado, sin abrir la rejilla.

Los dos alcances conviven y ninguno pisa al otro: «Ninguno» resume el objetivo, la rejilla decide celda a celda. Por
eso NO borra la mascara por celda, y volver a elegir una pieza devuelve exactamente las celdas que habia.

La pregunta fisica pasa a ser `PushBackRearTopeConfig.Draws(frente, nivel)` = la mascara MAS la decision de objetivo,
y la consumen los cinco sitios que materializan el tope —lateral, frontal, planta y las dos ramas del BOM—, para que
dibujo y lista no puedan discrepar. El EDITOR sigue leyendo `At`, que es lo que hace la decision reversible.

### Las pruebas recorren la ruta REAL

El dueño rechazo la evidencia de nivel bajo, asi que las pruebas de la defensa conducen la ventana Push Back → la
ventana real «Elementos de seguridad» → su rejilla real por poste → Aceptar → commit → resolve → primitiva y BOM. Lo
unico que se sustituye es el `ShowDialog`, que una prueba no puede ejecutar: las ventanas y sus controles son los
mismos que ve el usuario.

### Lo que NO cambio

Ningun golden se movio. Intactas: la fisica del tope (rondas 4B, 5 y 5D), StopA/StopB independientes, las alturas
A/B (6D), las cabeceras, las botas (6F), 27/27 (6A), I-40 e I-41. La suite de nucleo pasa entera y la de UI tambien.

### PENDIENTES REGISTRADOS

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas.
- **V1 — View hygiene:** el lateral de una seccion dibuja la letra de un lado que no existe funcionalmente.
- **V2 — Frontales:** una corrida full-span debe mostrar en la cara de salida el HIGH y el tope con semantica de
  vista posterior.
- **V3 — Planta / view hygiene:** auditoria de nearest-Y, deduplicacion y fail-open.
- **NUEVO — la rejilla por poste y la segunda cara de carga:** en un rack compuesto, la columna del extremo ALTO se
  pinta con la regla de un rack de un solo sentido, asi que muestra «apagado» en una linea cuyo pasillo lejano SI
  lleva defensa. No pierde geometria —esta ronda cerro esa via—, pero la columna no dice toda la verdad. Requiere
  llevar la segunda cara de carga por poste hasta la rejilla, que es un cambio de la semantica cerrada en 6D y no
  esta autorizado en esta ronda.

## 4-quinquetvicies. Ronda 7 (auditoria conservada, diseño RECHAZADO) y ronda 7B: seguridad en su ventana

### Lo que la ronda 7 dejo, y lo que el dueño rechazo

**Se conserva** su auditoria de `ActiveSide`: 43 usos en el arbol —los de Cantilever son de otro sistema— y ninguna
autoridad fantasma. Ningun consumidor fisico resuelve nada mirando `ActiveSide`; es contexto de seleccion
(`ActiveSide`, `SetActiveSide`, `MirrorSelection`), lectura del submodelo (`Active`, `Of(side)`,
`EditTargets`/`EditSides`) y baseline (`Snapshot`/`Restore`). Lo que faltaba era el CONTRATO, y las 18 pruebas de
`PushBackCompositeEditingContractTests` lo fijan: cambiar de lado es una identidad, editar A no toca B, el hueco SI
es compartido, `ModuleId` y el override de cabecera sobreviven, los Restore estan acotados, un blanco en un lado no
deshabilita el otro, Snapshot/Restore es transaccional cruzando un cambio de lado, Accept persiste los dos lados y
el lado B ausente no resucita.

**Se retira** su diseño de defensa: `DefenseSideA` / `DefenseSideB` como dos interruptores por LADO. El dueño lo
rechazo porque dentro de un mismo lado puede querer P1 si, P2 no, P3 si — el lado es contexto, no la identidad de la
intencion. Retirado por completo: los dos `bool?` del dominio y del DTO, el estado del editor, la traduccion del
resolver, el filtro del dibujo, la reflexion y las dos casillas de la ventana principal. Como 7fcfed9 nunca se
integro ni se valido, no se arrastra una abstraccion rechazada por compatibilidad.

### 7B, hallazgo de la auditoria: las dos capacidades YA EXISTIAN

Antes de diseñar nada se audito la ventana «Elementos de seguridad» (`SelectiveSafetyWindow`), y resulto que el
producto ya tenia lo que el dueño pedia:

| capacidad | donde vive | estado |
|---|---|---|
| defensa POR POSTE | `SelectiveSafetySelection.DefensaPosts` (`SafetyPostDefense` por poste) con su editor `SafetyDefensaGridWindow` — «Defensa de montacargas por poste» | ya existia; ahora fijado por contrato |
| identidad estable del poste | `SafetyPostDefense.PostIndex` = la LINEA transversal fisica | ya existia; estable porque un blanco no compacta la reticula (ronda 2) |
| tope en Elementos de seguridad | `PushBackRearTopeSection` — «Topes posteriores», con su copia de trabajo | ya existia desde 2026-07-24 |

Lo que si habia que corregir era una **duplicacion**: la ventana principal llevaba una segunda superficie para el
tope (`TopeSideACheck`, `TopeSideBCheck`, alcance y «Aplicar topes»), añadida en una ronda anterior porque la
ventana de seguridad solo alcanzaba al LADO ACTIVO. Una decision con dos sitios donde tomarla acaba divergiendo.

### La correccion

1. **La ventana principal deja de editar el tope.** Se retiran los cinco controles y su codigo. La fisica no se toca:
   sigue siendo el `RearTope` de cada lado y sus `OffCells`.
2. **La ventana de seguridad edita los DOS lados.** En un rack compuesto ofrece UNA seccion por lado —«Topes
   posteriores — lado A» y «— lado B»—, cada una con su copia de trabajo y su boton «Configurar…», y aplica cada una
   a SU lado al aceptar. Un rack de un solo sentido construye exactamente una seccion sin etiqueta, como siempre: era
   la unica capacidad que le faltaba a esta ventana, y con ella la duplicacion deja de hacer falta.
3. **La defensa por poste queda fijada por contrato** sobre el mecanismo existente, sin tocarlo.

### Defensa por poste: el contrato, medido

La intencion de un poste es su entrada en `DefensaPosts`, y el ON/OFF de cada cara es su LONGITUD: cero = esa cara
no lleva defensa. Un poste SIN entrada sigue la regla automatica de 12"/36", que es el comportamiento legacy.

| caso | resultado |
|---|---|
| P0 y P2 encendidos | defensa exactamente en las lineas 0 y 2 |
| P1 y P3 encendidos | exactamente en 1 y 3, y en ninguna otra |
| tocar P1 | no toca P2 |
| con un blanco delante | la linea conserva su indice: la intencion no se mueve |
| cambiar de lado activo | no toca ninguna intencion |
| cambiar el hueco | tampoco |
| guardar y RACKEDITAR | vuelven las cinco entradas, con su ON/OFF |
| intencion sobre una cara que no aplica | no crea pieza, y no salta a otro poste ni a la interfaz |
| documento LEGACY sin entradas | dibuja exactamente lo que dibujaba |
| poste que deja de existir | no deja defensa fantasma; uno nuevo toma el defecto automatico |

Dibujo = BOM en las cuatro combinaciones probadas.

### Lo que NO cambio

Ninguna primitiva de dibujo: el movimiento del tope es de EDICION. Sin editar nada, el plano es identico antes y
despues de abrir y cancelar la ventana de seguridad. **Ningun golden se movio.** Intactos: la fisica del tope
(rondas 4B, 5 y 5D), StopA/StopB independientes, alturas A/B, cabeceras, botas (6F), 27/27 (6A), I-40 e I-41.

### Contratos de prueba sustituidos

Seis pruebas de UI conducian la superficie retirada de la ventana principal. Su INTENCION —las cuatro combinaciones
por lado, los cinco alcances, la aplicabilidad por celda— se conserva, reescrita sobre la autoridad que la ventana
de seguridad usa (`ApplyRearTope` / `RearTopeAt` / `TopeSurface`), y se añade
`MainWindow_DoesNotOwnRearTopeEditor`, que comprueba que los cinco controles ya no existen.

### PENDIENTES REGISTRADOS

- **S1 — Safety general:** semantica global de Izquierda / Derecha / Ambas para las botas.
- **V1 — View hygiene:** el lateral de una seccion dibuja la letra de un lado que no existe funcionalmente.
- **V2 — Frontales:** una corrida full-span debe mostrar en la cara de salida el HIGH y el tope con semantica de
  vista posterior.
- **V3 — Planta / view hygiene:** auditoria de nearest-Y, deduplicacion y fail-open.

## 4-quateretvicies. Ronda 6E (RECHAZADA) y ronda 6F: la bota va donde hay una cara de ATAQUE

### 6E: rechazada funcionalmente por el dueño, y retirada

La ronda 6E leyo el defecto «Izquierda produce lo mismo que Ambas» como un problema de SEMANTICA DEL SELECTOR y lo
corrigio haciendo que `Ambas = Izquierda ∪ Derecha`, lo que en la practica ponia **dos copias espejadas en la misma
posicion**. El dueño confirmo que eso NO resuelve el problema fisico: la bota no es una eleccion de mano, es una
proteccion de POSTE contra el impacto del montacargas, y el montacargas ataca por la cara de CARGA.

Retirado por completo del codigo:

- `PushBackSafetyAuthority`: el parametro `keepChosenSide`, su reenvio por `RestrictToAisles` y el
  `Defaults()` que forzaba `Left`;
- `SelectiveSafetyEnds.CopiesForPost`: el `Ambas` que devolvia dos copias con un solo pasillo;
- los cuatro contratos de prueba que 6E habia reescrito, que vuelven a su forma anterior;
- los **cuatro pines dorados**, que vuelven a los de la ronda 6D. El BOM de botas del escenario dorado vuelve de
  **6 a 3**: aquel 3 → 6 salia unicamente de duplicar cada bota sobre si misma y no estaba aprobado.

Se conserva, en cambio, la parte de 6E que sigue siendo cierta y util: la bateria de escenarios con blancos y
compuestos parciales, reescrita en la ronda 6F sobre el contrato fisico correcto.

**Registrado como deuda separada, fuera de I-42:** el selector Izquierda / Derecha / Ambas de los protectores de
bota no modela las caras fisicas que hay que proteger, y ese defecto existe tambien fuera del Push Back compuesto.
No se rediseña aqui.

### 6F: la regla fisica

Una bota protege el poste del impacto del montacargas, y el montacargas ataca por la cara de CARGA — en un Push
Back, la del extremo BAJO. La cara ALTA suele dar a muro, columna o espacio no operativo. Un rack COMPUESTO tiene
DOS caras de ataque, una por lado, en los dos exteriores.

### El defecto, reproducido

Las dos copias de una linea se atornillan a los extremos de su COBERTURA de profundidad. Eso vale mientras esos
extremos SEAN caras de ataque. Con frentes en blanco —una columna de nave— la cobertura de esa linea se acorta y su
extremo pasa a caer en la interfaz entre los dos lados.

Medido, compuesto de tres ranuras con **las dos primeras de A en blanco**:

| linea | cobertura | ANTES | AHORA |
|---|---|---|---|
| Y=53.494 | [396, 792] | **X=395.61 — contra la columna** y X=792.39 | solo X=792.39 |
| Y=106.988 | [0, 792] | X=−0.39 y X=792.39 | igual |

Es la MISMA familia del error de la defensa de montacargas que la ronda 6D cerro, y se corrige con la MISMA
declaracion fisica: la estructura ya declara su tramo interior, y una bota no se materializa en una cara que caiga
ahi. **Un blanco QUITA la necesidad; no muda la pieza a otro borde.**

La correccion vive en el primero de los tres ejes: `SelectiveSafetyPlacement.AppendAtPost` admite un filtro de
PERTENENCIA —«¿existe esta cara?»— que no toca ni la posicion ni la orientacion. Con `null` todas las caras aplican,
que es lo que hace el Dinamico.

### Medido, los ocho escenarios

| caso | ANTES | AHORA |
|---|---|---|
| A) simple, 3 frentes | 2 botas en X=−0.39 | **igual** |
| B) compuesto completo | 4: dos en cada exterior | **igual** |
| **C) blanks A 0,1 (columna)** | **4, una en X=395.61 (INTERIOR)** | **3, ninguna interior** |
| C2) blank A 0 | 4, todas exteriores | igual |
| D) blanks B 0,1 | 3, todas exteriores | igual |
| E) blanks A+B 0,1 | 2 | igual |
| F) parcial compuesto | 3 | igual |
| G) corrida | 4 | igual |

**Solo cambia el caso que el dueño reporto.** El rack simple no cambia; el compuesto completo protege sus dos
exteriores con posiciones FISICAMENTE DISTINTAS, sin duplicar por espejo sobre la misma cara.

### BOM

El BOM de botas es el numero de protecciones materializadas, y sigue al dibujo: 3 en el caso C (antes 4, con una
imposible). **Ningun golden se movio** en 6F —el escenario dorado es de un solo sentido y no declara interior— y el
BOM de botas del dorado vuelve a **3**, el valor anterior a la 6E rechazada.

### Lo que no se toco

Alturas A/B (6D-A), defensa de montacargas (6D-B), cabeceras (6B/6C), intermedios 27/27 (6A), y todo lo cerrado en
las rondas 1 a 5D.

### Pruebas

`PushBackBootLowFaceTests` (40 casos): la cara de ataque como unica ubicacion valida en los ocho escenarios, las dos
caras distintas del compuesto, la ausencia de duplicados por espejo, el caso del dueño con su blanco, los simetricos
de A y B, el blanco doble, el parcial, el rack simple sin cambio, dibujo = BOM, y **dos regresiones explicitas**: la
defensa de 6D y los 27/27 de 6A. Anulando el filtro de cara fallan 3, todas del caso con blancos.

### PENDIENTES REGISTRADOS (siguen sin corregir)

- **P1 — UI / ActiveSide:** control de proteccion/defensa por cara o lado, semanticamente correcto.
- **P2 — Safety general:** revision global de la semantica de Izquierda / Derecha / Ambas para las botas, fuera del
  contrato especifico de I-42.
- **P3 — View hygiene:** el lateral de una seccion dibuja la letra de un lado que no existe funcionalmente.
- **P4 — Frontales:** una corrida full-span debe mostrar en la cara de salida el HIGH y el tope con semantica de
  vista posterior.

## 4-teretvicies. Ronda 6D: A y B con alturas independientes, y la defensa solo en una cara de carga

Cierre de la fase 6. Dos defectos de la validacion del dueño, independientes. Se conservan 6A (BOM = piezas
fisicas), 6B (una pieza, una altura en todas las vistas) y 6C (la demanda sale de las camas reales).

---

### 6D-A — la altura de A se imponia a B

**Reproduccion.** Compuesto de dos ranuras, lado A con CUATRO niveles y lado B con TRES. Demanda fisica de cada
cama: A = 264", B = 192". Los DOCE postes del corte lateral —los seis de A y los seis de B— salian a **264"**, y
los dos frontales y el BOM tambien.

**Causa: granularidad, no formula.** 6C ya resolvia la demanda por cama, pero la escribia en `front.Height`, que es
una propiedad de la LINEA transversal. Una cabecera, sin embargo, vive en una linea **y** en una posicion
longitudinal, y esa segunda coordenada decide a que lado sirve: en la estructura compuesta —A + hueco + B
invertido— la primera mitad de la profundidad es de A y la segunda de B. Colapsar las dos en un maximo por linea es
lo que alargaba los postes de B.

**Correccion.** `PushBackHeaderHeight.Zones` resuelve la demanda **por lado y por linea** —una cama aporta a la
linea de su izquierda y a la de su derecha, y al lado o LADOS a los que pertenece— y la publica como TRAMOS DE
PROFUNDIDAD (`DynamicHeaderHeightZone`), que el compuesto escribe en la estructura. `DynamicFrontGeometry.PostHeightAt`
responde en la posicion en que se pregunta; sin zonas declaradas responde `PostHeight`, que es lo que hacia antes,
y por eso el Dinamico y todo Push Back de un solo sentido dibujan igual.

Los tramos de cada lado ya los publicaba el compuesto: su extremo EXTERIOR (su pasillo) y su INTERIOR (la linea que
mira al hueco). No se parte por la mitad: con profundidades distintas la mitad no cae donde acaba A.

Consumidores actualizados, todos por la MISMA funcion: la cabecera (`HeaderConfigurationAtPost`), sus separadores y
sus postes derivados (el corte lateral resuelve un contexto por tramo), el corte frontal de cada lado —que ahora
pregunta DENTRO de su tramo, porque sobre el rack entero el extremo posterior de A caia en la cabecera del otro
lado— y el BOM, que ya consumia la misma funcion.

**Medido:**

| caso | ANTES (A y B) | AHORA A | AHORA B |
|---|---|---|---|
| A=4 niveles / B=3 | 264 / 264 | **264** | **192** |
| A=3 / B=2 | 192 / 192 | **192** | **120** |
| A=2 / B=4 | 264 / 264 | **120** | **264** |
| A profundo bajo (d8, 2 niv) / B corto alto (d4, 4 niv) | 264 / 264 | **120** | **264** |
| corrida A→B | 120 / 120 | **120** | **120** |

Cada altura es exactamente la que un rack SIMPLE de esos niveles y ese fondo resuelve. Y una CORRIDA sigue dando el
mismo valor en los dos tramos: es una pieza compartida de verdad. La regla no inventa diferencias, solo deja de
imponerlas.

**Envolvente local y 6B intactos:** un frente profundo remoto sigue sin subir una cabecera ajena, y la misma pieza
fisica —la de un lado— mide lo mismo en su lateral y en sus dos frontales.

**I-40:** un override manual manda sobre los dos lados —es del rack— y Restore devuelve la propuesta ACTUAL de cada
uno: 264 para A y 192 para B.

---

### 6D-B — con un blanco, la defensa saltaba a la cara posterior del lado contrario

**Reproduccion.** Compuesto de tres ranuras, encontradas. Sin blancos las defensas estan en X = −4.75 (el pasillo
de A, cuatro) y X = 508.75 (el de B, cuatro). Poniendo el lado A EN BLANCO en la ranura 0:

| | ANTES | AHORA |
|---|---|---|
| pasillo de A (X = −4.75) | 3 (pierde la de su ranura en blanco) | 3 |
| pasillo de B (X = 508.75) | 4 | 4 |
| **interior del rack (X = 247.25)** | **1 — contra la cara posterior del lado contrario** | **ninguna** |

**Causa.** `AppendPlantaDefensas` coloca la defensa en los extremos de la COBERTURA de profundidad de su linea. Eso
es correcto mientras esos extremos sean caras de carga. Un lado en blanco acorta la cobertura de su linea, y su
extremo pasa a caer en la interfaz con el otro lado: dentro del rack, sin pasillo al que mirar.

**Correccion.** La estructura declara su TRAMO INTERIOR —el hueco entre los dos lados—, y una defensa no se
materializa en un extremo que caiga ahi. El Dinamico no declara ninguno y dibuja exactamente igual que siempre. No
se toca la reticula: un blanco conserva su ranura, y cada defensa que sobrevive esta donde estaba —el blanco QUITA,
nunca MUEVE.

**No se añadio ningun control por lado.** La intencion de defensa sigue siendo global, tal como el dueño pidio para
esta ronda.

**BOM de seguridad:** dibujo y BOM siguen coincidiendo pieza a pieza. La unica diferencia es la defensa que nunca
debio existir, que desaparece de los dos a la vez.

---

### Goldens

**Ninguno se movio.** El escenario dorado es de un solo sentido: no declara zonas ni tramo interior, asi que
ninguna de las dos correcciones lo alcanza.

### 6A intacto

Dibujo 27 / BOM 27, fijado tambien en las pruebas de esta ronda.

### Contratos SUSTITUIDOS

Dos ayudantes de prueba de 6B y 6C exigian **una sola altura por corte**, que es justo lo que el contrato del dueño
sustituye. Ahora leen la altura DENTRO del tramo de un lado, y `SharedPhysicalHeader_UsesRequiredLocalEnvelope` y
`DifferentSideLevels_UseMaxPhysicalRequirementAtSharedLine` afirman que cada lado recoge SU demanda en vez de un
maximo comun. La linea INTERIOR se excluye de la ventana de lectura: ahi los dos lados tienen su propio poste, uno
contra otro.

### Pruebas

`PushBackSideIndependentHeightTests` (35 casos): las alturas independientes en los dos sentidos, profundidad y
altura como ejes distintos, la coherencia por lado en las diez topologias, la pieza realmente compartida, el rack
simple sin cambio, override y Restore, y las cuatro de seguridad con blancos. Anulando las zonas fallan 6 pruebas
de altura; anulando la regla de cara de carga fallan 3 de seguridad. Evidencia separada.

### PENDIENTES REGISTRADOS (fuera de 6D, no corregidos)

- **UI / ActiveSide — U1:** control independiente de defensa por lado (`Defensa A si/no`, `Defensa B si/no`) en
  lugar de «ambos o ninguno». El dueño lo pidio explicitamente; esta ronda solo garantiza que la intencion actual no
  se materialice en un borde absurdo.
- **View hygiene — V1:** el lateral de una seccion donde un lado no existe funcionalmente dibuja la letra de ese
  lado. La etiqueta debe depender de la presencia funcional real de la seccion.
- **View hygiene / frontales — V2:** una corrida que llega hasta la cara opuesta debe representar en esa frontal el
  extremo HIGH —larguero alto y tope— con semantica de vista posterior. Simetrico para B→A.

## 4-duoetvicies. Ronda 6C: la altura de cabecera sale de las CAMAS REALES

La ronda 6B dejo UNA sola autoridad, consumida por el corte lateral, los dos frontales y el BOM. Esa conquista se
conserva intacta. Lo que 6C corrige es el **input** de esa autoridad.

### La causa

La estructura de un rack compuesto es una sola: A + hueco + B invertido. Sus frentes se resuelven con esa
profundidad **SINTETICA**, y el resolver dinamico deriva de ella la elevacion de entrada del ultimo nivel, que es la
que gobierna la altura de la cabecera. **Ninguna cama recorre esa profundidad.** En unas encontradas de 5+5 fondos
hay dos camas de cinco, no una de once.

Medido, sobre unas encontradas de dos ranuras y dos niveles por lado:

| | frente | entrada nivel alto | teorico | comercial |
|---|---|---|---|---|
| rack SIMPLE de 5 fondos | deep=5 | 86.6053 | 114.6053 | **120** |
| estructura COMPUESTA | deep=11 | **96.6053** | 124.6053 | **132** |

Diez pulgadas de entrada que ninguna cama pide, y con ellas un pie comercial de poste. El larguero alto que las dos
camas dibujan de verdad esta en Y = 78.6053, el mismo que en el rack simple.

### La correccion

**La regla de cabecera NO cambia.** Sigue siendo la de `DynamicHeaderHeightCalculator`: entrada del ultimo nivel,
mas el peralte de su larguero, mas un tercio del espacio libre, redondeado al pie comercial. Lo unico que cambia es
de donde sale la elevacion.

`PushBackHeaderHeight` resuelve la demanda **por cama**: aplica esa misma funcion al frente de la cama —en su propio
marco, con su propia profundidad— y toma el maximo de las camas que usan cada frente. Una cama de mas se traduce en
una demanda de mas; una cama que no existe no aporta ninguna.

Con una salvaguarda: si el larguero ALTO que la cama dibuja de verdad queda por encima de la entrada que su frente
resolvio, manda el larguero. Son dos elevaciones de la MISMA pieza fisica y la cabecera tiene que contener la mas
alta. En un rack de un solo sentido la entrada resuelta es siempre la mayor, asi que la salvaguarda no cambia nada.

Se escribe en `front.Height` de la estructura compuesta — **el mismo sitio** del que la ronda 6B hizo leer a las
tres vistas y al BOM. Sigue habiendo una sola autoridad; solo se corrigio lo que responde.

### Encontradas frente a corrida, sin preguntar por topologia

La regla pregunta por CAMA, asi que la distincion sale sola:

- unas **encontradas** son dos camas independientes: cada una aporta su demanda y gana el maximo, nunca la suma;
- una **corrida** si atraviesa fisicamente los dos lados, y su demanda sale de su propia cama, la que de verdad
  recorre esa longitud.

### Medido, por escenario

| caso | ANTES | AHORA | por que |
|---|---|---|---|
| A) simple 1 frente d5 | 120 | **120** | sin cambio |
| A2) simple 2 frentes d5/d5 | 120 | **120** | sin cambio |
| K) simple 5/8/6/9 | 120/120/120/132/132 | **igual** | sin cambio; envolvente local intacta |
| B) compuesto solo A | 132 | **120** | sus camas son de cinco fondos |
| C) compuesto solo B | 132 | **120** | idem |
| D) encontradas d5/d5 | 132 | **120** | dos camas de cinco, no una de once |
| E) encontradas d8/d4 | 132 | **120** | manda la cama de 8, no la suma 8+4 |
| F) encontradas niveles 3/2 | 204 | **192** | manda la cama de tres niveles |
| G) corrida A→B | 132 | **120** | su cama real, no la profundidad sintetica |
| H) corrida B→A | 132 | **120** | idem |
| J) blank A en la ranura 0 | 132 y 120 | **120** | el blanco no aporta demanda |

En todos ellos el corte lateral, los dos cortes frontales y el BOM siguen coincidiendo, ahora en el valor
fisicamente correcto.

### Blancos y overrides

Una ranura en blanco por los dos lados no tiene camas, asi que su frente no aporta demanda: su `Height` queda en 0 y
la linea contigua toma la del frente que si carga —`DynamicFrontGeometry.PostHeight` ya hacia ese maximo por linea—.
La ranura sigue existiendo y su linea se sigue dibujando.

I-40 intacto: un override manual manda sobre la propuesta derivada, y Restore lo borra y devuelve la propuesta
**ACTUAL**, recalculada sobre las camas de ahora. Medido: con override 156" y anadiendo despues un tercer nivel al
lado A, Restore devuelve 192 —no 120 ni 156—.

### Goldens

**Ninguno se movio.** El escenario dorado es de un solo sentido, y esta correccion solo alcanza al camino compuesto.

### 6A intacto

Dibujo 27 / BOM 27 y las cuatro pruebas de seguridad siguen verdes, sin tocar codigo de seguridad.

### Pruebas

`PushBackPhysicalHeaderHeightTests` (19 casos): encontradas sin suma, profundidades distintas, niveles distintos,
lado en blanco, corrida con su propia cama, no-fuga remota, rack simple sin cambio, las cuatro consumidoras de
acuerdo en diez topologias, override y Restore. Anulando la nueva resolucion fallan 15.

## 4-unetvicies. Ronda 6: el BOM cuenta piezas fisicas, y una linea tiene UNA altura

Dos familias, dos defectos independientes, cada uno con su reproduccion y su evidencia. Ninguna decision cerrada de
las rondas 1 a 5D se toca.

---

### 6A — el BOM facturaba 42 largueros intermedios para un plano de 27

**Reproduccion.** Rack Push Back de UN SOLO SENTIDO, un frente de ocho fondos con seis niveles y una cama por
nivel de 3 a 8 fondos (I-41, fondo por celda):

| nivel | fondo efectivo | X del alto | fronteras que la cama recorre | dibujados |
|---|---|---|---|---|
| 1 | 3 | 150 | 54, 102 | 2 |
| 2 | 4 | 198 | 54, 102, 150 | 3 |
| 3 | 5 | 246 | 54, 102, 150, 195 | 4 |
| 4 | 6 | 294 | 54, 102, 150, 195, 246 | 5 |
| 5 | 7 | 342 | 54, 102, 150, 195, 246, 294 | 6 |
| 6 | 8 | 396 | las siete | 7 |

**Dibujado 27. Facturado 42.**

**Causa.** `PushBackBomBuilder` tenia DOS caminos. El compuesto ya contaba los intermedios con el MISMO builder que
los dibuja (`PushBackIntermediateBeamLateralBuilder.BuildFor`). El de un solo sentido conservaba la cuenta heredada
del Dinamico —`SystemBomBuilder`: `Supports(front).Count x niveles`, es decir TODAS las fronteras de la estructura
por cada nivel— que es anterior a I-41 y no pregunta por el fondo EFECTIVO de la celda: 7 x 6 = 42.

**Correccion.** Una sola cuenta, `EmitIntermediates`, que materializa las piezas con el builder del dibujo y las
agrupa. Las dos rutas solo se diferencian en QUE camas enumeran —las del compuesto por lote de cama, las del rack de
un solo sentido por frente con todos sus niveles—; el conteo vive en un unico sitio. **Un rack sin fondos por celda
cuenta exactamente lo mismo que antes**, porque entonces cada nivel recorre todas las fronteras de su frente: por eso
ningun golden se movio.

**AFTER: dibujo 27, BOM 27.** Y `ResolvedPhysicalBedLength` no se toca: la cama, el bajo y el alto conservan sus
autoridades.

**Encontradas NO se sobrecorrige:** siguen comprando el doble de altos, topes e intermedios que una cama sola,
porque son dos camas. La cuenta no deduplica por posicion en ningun momento.

### 6A — seguridad: auditado, sin defecto que corregir

La cadena de seguridad ya cumple lo que esta ronda exige, y ahora esta fijado por pruebas:

- el BOM de seguridad se construye desde las piezas DIBUJADAS (planta + los dos cortes frontales), no desde una regla
  paralela, asi que dibujo y BOM no pueden divergir;
- `Side` sigue siendo PERTENENCIA en bota, protector y defensa, y solo el DESVIADOR —la unica familia donde el lado
  significa literalmente «que pasillo»— recibe `Both`. **Dos caras de carga no se traduce a `Side = Both`**;
- un rack parcialmente compuesto conserva la seguridad de las zonas no compuestas: con lado B en una sola ranura, la
  seguridad es un subconjunto estricto de la del rack con B en las tres, y no aparece ninguna pieza nueva;
- un blanco conserva su ranura fisica y su reticula de seguridad sin crear almacenamiento.

---

### 6B — la altura de cabecera no coincidia entre el lateral y las frontales

**Reproduccion.** Rack COMPUESTO, dos ranuras, dos niveles por lado, cinco fondos por lado:

| linea | corte LATERAL | frontal ENTRADA | frontal POSTERIOR | BOM (longitud de cabecera) |
|---|---|---|---|---|
| L0 | **132** | **120** | **120** | 132 |
| L1 | **132** | **120** | **120** | 132 |
| L2 | **132** | **120** | **120** | 132 |

Un pie comercial de diferencia en la MISMA pieza. Con tres niveles en el lado A: 204 en el lateral y 192 en la
frontal. En un rack de un solo sentido las dos vistas ya coincidian — el defecto es exclusivamente del compuesto.

**Que vista estaba equivocada, decidido por evidencia y no por criterio.** El BOM compra el poste de **132**, el
mismo que dibuja el lateral. La frontal era la unica en desacuerdo con la pieza que se fabrica.

**Causa.** `PushBackCompositeFrontal.Build` construye el corte sobre el sistema **LOCAL del lado**, que es un modelo
de trabajo con sus propias alturas resueltas, y el poste tomaba de ahi su `LONGITUD`. Pero ese poste es la MISMA
pieza fisica que el lateral dibuja y que el BOM compra, y esa pertenece a la estructura **COMPUESTA**. Dos modelos
respondiendo por la misma pieza.

**Correccion.** El corte frontal recibe la altura de la LINEA FISICA, resuelta sobre la estructura compuesta por la
MISMA funcion que ya responde en el lateral y en el BOM (`DynamicFrontGeometry.HeaderHeightAtPost`). Se traduce la
linea local a su linea compuesta y se pregunta; no se ajusta nada graficamente. Con `null` —un rack de un solo
sentido, o el Dinamico— el comportamiento es exactamente el anterior.

**AFTER: las tres vistas y el BOM dicen 132** (204 con el lado de tres niveles), en las doce topologias probadas.

**Envolvente LOCAL, sin fuga remota.** Medido sobre un rack de fondos 5/8/6/9: las lineas valen 120, 120, 120, 132,
132 — solo las dos que TOCAN el frente de nueve fondos suben. Quitando ese frente, las lineas 0 a 2 miden
exactamente lo mismo: un frente profundo remoto no sube una cabecera ajena. Y con el lado A en blanco en una ranura,
subir A a tres niveles no mueve la altura de la linea que A no carga.

**I-40 intacto.** Un override manual de altura sobrevive a la resolucion compuesta y lo consumen las dos vistas;
Restore lo elimina y devuelve la PROPUESTA ACTUAL, no el valor que el override tenia.

---

### Goldens

**Ninguno se movio, ni en 6A ni en 6B.** El escenario dorado no tiene fondos por celda —cada nivel recorre todas las
fronteras de su frente, asi que su cuenta de intermedios ya era la fisica— y no es compuesto, asi que su frontal ya
leia la unica estructura que tiene.

### Pruebas

- `PushBackPhysicalBomAndSafetyTests` (26 casos): BOM de intermedios = apoyos fisicos en diez racks, el corte por
  fondo de celda, la independencia del refuerzo, encontradas con sus dos juegos, y las cuatro de seguridad
  (pertenencia, blanco, dos caras != `Side.Both`, dibujo = BOM).
- `PushBackHeaderHeightAuthorityTests` (32 casos): la misma cabecera fisica en todas las vistas sobre doce
  topologias, el BOM = altura resuelta, la envolvente local, la no-fuga remota, A/B independientes, la pieza
  compartida, blancos, dos blancos consecutivos, override y Restore.

Anulando la cuenta compartida fallan 3 pruebas, todas de intermedios. Anulando la altura de linea fallan 10, todas
de cabecera compuesta. Los dos defectos tienen evidencia separada.

### Declarado, NO resuelto en esta ronda

La altura de una linea compuesta se deriva de la `EntranceElevation` del frente COMPUESTO, que abarca la profundidad
de los dos lados. En una topologia de camas ENCONTRADAS esa profundidad no corresponde a ninguna cama real —son dos
camas de cinco fondos, no una de diez—, de modo que el valor resultante puede ser un pie comercial mas alto de lo
que la geometria dibujada necesita (el larguero mas alto medido esta en Y = 78.6 y la cabecera resuelve 132). Esta
ronda hace que **todas las vistas y el BOM digan lo mismo**, que es el defecto reportado; **si esa altura comun debe
seguir saliendo de la profundidad compuesta o de la demanda real de cada cama es una decision FISICA del dueño**, y
no se toma aqui.

## 4-vicies. Correccion aislada 5D: anclaje del tope al espejar, y el poste reforzado no duplica el apoyo

Dos defectos INDEPENDIENTES de la validacion del dueño. Cada uno tiene su causa, su correccion y su evidencia; no
se usa uno para tapar el otro. La regla de orientacion de 5B/5C queda cerrada y no se reabre.

---

### DEFECTO A — el tope espejado quedaba 1.75" desplazado

La orientacion era correcta. Lo que estaba mal era DONDE se dibujaba el bloque al cambiar de mano.

**Causa.** El tope mata por su ORIGEN sobre un punto medido del poste, y ese punto es un SEMIANCHO: se mide hacia el
lado al que mira la pieza. El SELECTIVO ya lo resuelve asi desde siempre (`SelectiveLateralBuilder`:
`mateX = AtFront ? postX + troquel.X : postX - troquel.X`, y esos dos casos son exactamente los que llevan
`Mirror = true` y `Mirror = false`). El Push Back, en cambio, tomaba el signo del espejo de la **COLOCACION**, que en
el marco de una cama es una constante. Mientras el tope tambien iba siempre sin espejo las dos expresiones coincidian
—por eso la ronda 4B quedo bien—; desde 5B el tope puede ir espejado, y entonces el bloque se dibujaba con su origen
del lado contrario: desplazado DOS veces el punto medido, `2 x 0.875" = 1.75"`.

**El 1.75" no se hardcodea.** No aparece ninguna constante nueva: es el mismo desplazamiento de siempre —el punto
medido del poste, leido del catalogo— con el signo que le corresponde. La autoridad reutilizada es
`PushBackRearTopeBuilder.AnchorX(columnX, anchorLocalX, topeMirrored)`, y la usan el corte lateral y la planta.

**Medido** sobre la escalera de ocho fondos, que mezcla las dos manos en un mismo rack (punto medido = 0.875"):

| vista | cama | HIGH X | mano HIGH | tope insercion | mano tope | contacto ANTES | esperado | error |
|---|---|---|---|---|---|---|---|---|
| LATERAL | 3 fondos | 150 | espejado | 149.125 | normal | 150 | 150 | 0 |
| LATERAL | **4 fondos** | 198 | normal | **197.125** | espejado | **196.25** | 198 | **-1.75** |
| LATERAL | **5 fondos** | 246 | normal | **245.125** | espejado | **244.25** | 246 | **-1.75** |
| LATERAL | 6 fondos | 294 | espejado | 293.125 | normal | 294 | 294 | 0 |
| LATERAL | **7 fondos** | 342 | normal | **341.125** | espejado | **340.25** | 342 | **-1.75** |
| LATERAL | 8 fondos | 396 | espejado | 395.125 | normal | 396 | 396 | 0 |

DESPUES el error es 0 en las seis. Las tres camas de mano espejada mueven su INSERCION (197.125 -> 198.875,
245.125 -> 246.875, 341.125 -> 342.875) precisamente para que el CONTACTO no se mueva; las tres de mano normal no
cambian ni un milesimo, que es la ronda 4B intacta.

**Lo que NO cambio:** la regla de mano del HIGH, la mano del tope, la posicion semantica de 4B, X/Z del HIGH, la
profundidad, la aplicabilidad, el PieceId y el BOM.

---

### DEFECTO B — dos largueros en un poste REFORZADO

**Causa, y no es una deduplicacion.** En un poste derivado y reforzado, `FIN_POSTE` es la interfaz donde acaba el
perfil primario y empieza el refuerzo, asi que el apoyo se DIBUJA una `finPoste.X` antes de su frontera de modulos.
El filtro de aplicabilidad —«¿este apoyo cae dentro de esta cama?»— comparaba la X **dibujada** contra el larguero
alto, de modo que el apoyo de la frontera donde ACABA la cama se colaba por delante. Dos piezas para un solo apoyo
fisico.

La correccion no toca el refuerzo ni deduplica por posicion: el apoyo lleva ahora su **FRONTERA**
(`DynamicIntermediateBeamSupport.BoundaryX`), que es su identidad fisica, y el filtro pregunta por ella. Un refuerzo
cambia el POSTE; no añade un segundo apoyo funcional a la cama.

**Las dos primitivas, medidas** (escalera de ocho fondos, cama de 4 fondos, unico limite vano-vano del rack):

| pieza | procedencia | insercion | frontera a la que sirve | veredicto |
|---|---|---|---|---|
| `LARGUERO_ESCALON_TROQUEL_REDONDO` | `PushBackLoadBeamGeometry.HighBeams` | 198 | 198 | el ALTO de la cama, correcto |
| `LARGUERO_ESCALON_INFINITO` | `PushBackIntermediateBeamLateralBuilder` | **195** | **198** | **duplicado: la misma frontera** |

Conteo por cama, con y sin refuerzo:

| cama | ANTES reforzado | ANTES sin refuerzo | AHORA reforzado | AHORA sin refuerzo |
|---|---|---|---|---|
| 3 fondos | 2 | 2 | 2 | 2 |
| **4 fondos** | **4** | **3** | **3** | **3** |
| 5 fondos | 4 | 4 | 4 | 4 |
| 6 fondos | 5 | 5 | 5 | 5 |
| 7 fondos | 6 | 6 | 6 | 6 |
| 8 fondos | 7 | 7 | 7 | 7 |

La comprobacion mas limpia: el numero de largueros funcionales de una cama ya NO depende del refuerzo. Y el apoyo de
195 no se ha borrado — la cama de 5 fondos, que pasa de largo por esa frontera, lo conserva.

**No se deduplica a ciegas.** Unas camas ENCONTRADAS siguen conservando sus DOS largueros altos y sus DOS topes: son
dos camas distintas aunque topen en la misma frontera. La decision de la ronda 5B se mantiene.

---

### Goldens

**Ninguno se movio.** El escenario dorado tiene su tope sin espejo —su frontera alta es el final de una cabecera— y
no tiene ningun limite vano-vano donde acabe una cama, asi que ninguno de los dos defectos lo alcanza. Los seis pines
quedan byte a byte donde estaban.

### BOM

Sin cambio, antes y despues, con refuerzo y sin el (escalera de ocho fondos): 42 intermedios, 6 topes, 6 IN/OUT y 6
altos. Encontradas sigue comprando 4 topes y 4 largueros altos con dos niveles — dos camas, dos piezas.

**Observacion REPORTADA, no arreglada aqui:** el BOM de un rack de un solo sentido cuenta 42 intermedios —siete
fronteras por seis niveles— mientras el dibujo pone 27, porque ahi el conteo sigue siendo el del Dinamico y no aplica
el fondo POR CELDA. Es una brecha anterior a esta ronda, ajena al poste reforzado —no cambia con el refuerzo— y de la
familia de I-41, no de I-42.

### Pruebas

- `PushBackRearTopeContactPointTests` (9 casos), con `MirroringRearTope_PreservesPhysicalContactPoint` sobre mano
  normal, mano invertida, corrida corta, solo A, solo B, encontradas y las dos corridas. Comparan el CONTACTO
  FISICO, no la insercion.
- `PushBackReinforcedPostBeamTests` (7 casos): 1 bajo / N intermedios / 1 alto por cama, con y sin refuerzo; el caso
  medido; la independencia del refuerzo; encontradas intactas; y los conteos del BOM.

Anulando el signo del anclaje fallan 5 pruebas, todas de tope. Anulando la frontera del apoyo fallan 3, todas de
poste reforzado. Los dos defectos tienen evidencia separada.

## 4-undevicies. Correcciones aisladas 5B y 5C: la mano del larguero ALTO es la de un INTERMEDIO en esa posicion

### La regla, cerrada por el dueño (SUSTITUYE a la de la ronda 5)

El dueño **retiro** la regla de la ronda 5 —«ultimo poste, orientacion normal; primer poste o poste interior,
invertido»— y la sustituyo por esta:

> El larguero HIGH / de salida debe tener EXACTAMENTE LA MISMA ORIENTACION QUE TENDRIA UN LARGUERO INTERMEDIO
> COLOCADO EN ESA MISMA POSICION FISICA. No inventar una regla nueva para el HIGH. El programa YA orienta
> correctamente los largueros intermedios. REUTILIZAR ESA AUTORIDAD.

La ronda 5C añade la otra mitad: esa mano se decide UNA vez y la PLANTA la **transporta**. Ninguna vista la
recalcula.

Y una separacion que el dueño cerro explicitamente: **la mano del tope sale de su larguero, pero su POSICION es la
que quedo validada en la ronda 4B**. Corregir la orientacion no puede mover la pieza.

### Lo que habia, medido

La mano del alto la fijaba `DynamicLoadBeamGeometry.Placements`, que en el marco de una cama es un «alto SIEMPRE
espejado» — una constante, no una lectura de la estructura. Reproducido en la estructura de OCHO fondos que el dueño
uso, con una cama por nivel de 3 a 8 fondos (`PushBackHighEndHandTests.DepthLadder`):

| cama | frontera (X) | modulo que TERMINA ahi | un intermedio iria | el alto iba | ¿coincide? |
|---|---|---|---|---|---|
| 3 fondos | 150 | cabecera | espejado | espejado | si |
| 4 fondos | 198 | vano | normal | **espejado** | **no** |
| 5 fondos | 246 | vano | normal | **espejado** | **no** |
| 6 fondos | 294 | cabecera | espejado | espejado | si |
| 7 fondos | 342 | vano | normal | **espejado** | **no** |
| 8 fondos | 396 | cabecera (extremo) | espejado | espejado | si |

Tres de seis mal, y las tres son las que acaban en un VANO: es el patron que el dueño reporto. En su rack tambien
salia mal la cama de 8 fondos, lo que solo puede ocurrir si el ultimo modulo de SU estructura es un vano — el
reparto cabecera/vano depende del rack, la regla no.

### La correccion

1. **La autoridad se EXTRAE, no se duplica.** `DynamicIntermediateBeamGeometry` ya decidia la mano de sus apoyos con
   `previous.IsHeader`. Eso pasa a ser `HandAtBoundary(modulo que termina ahi)` y se expone
   `HandAtDepthX(estructura, x)`, que responde por posicion fisica. El builder de intermedios sigue llamandola: una
   sola decision, dos consumidores. **No hay ninguna regla nueva en el Push Back.**
2. **El larguero alto la consume** en `PushBackLoadBeamGeometry.HighBeams`, al CREAR la pieza. Sin frontera en esa X
   la autoridad no opina y la pieza conserva lo que traia.
3. **5C — la planta transporta.** `PushBackSystemPlantaBuilder` sustituia el larguero conservando el espejo del
   builder dinamico, que nunca pasaba por la autoridad. Medido en una corrida CORTA: el lateral ponia su alto en
   X=102 sin espejo —la frontera es un vano— y la planta lo ponia espejado. Ahora la planta lee la misma respuesta.
4. **El tope hereda.** `PushBackRearTopeBuilder.Mirrored` era una CONSTANTE en las elevaciones, asi que el tope no
   seguia a su larguero. Pasa a ser una sola relacion —el tope va con la mano contraria a la de SU alto— en las
   vistas de PROFUNDIDAD. El corte FRONTAL no: ahi la X corre con la retícula transversal y el espejo de una pieza
   no habla del escalon, que se ve de canto; conserva la orientacion que el dueño valido.
5. **La deduplicacion de proyeccion distingue camas.** Dos camas ENCONTRADAS topan en la misma frontera y, desde que
   la mano sale de la POSICION y ya no del sentido, sus dos largueros altos coinciden tambien en el espejo. Una
   clave que solo mirara posicion, pieza y espejo los habria colapsado en uno. La clave incluye ahora el MARCO de la
   cama (sistema fuente + reflejada), que es lo que las distingue — **sin** introducir una regla de mano distinta
   para A y para B, que es justo lo que el dueño prohibio.

### Posicion y orientacion son dos autoridades (decision del dueño)

La mano se aplica al CREAR la instancia del larguero, no en su colocacion. La POSICION del tope se sigue derivando
de la mano que trae la colocacion —la que la ronda 4B cerro y el dueño valido—, asi que un cambio de espejo del alto
**no puede** volver a desplazar el tope. Todas las pruebas de posicion de la ronda 4B siguen verdes con sus X
esperadas SIN modificar.

### Auditoria

| VISTA | AUTORIDAD DE LA MANO DEL ALTO | ANTES | DESPUES |
|---|---|---|---|
| LATERAL | geometria dinamica en el marco de la cama | «siempre espejado» | **la del intermedio en esa X** |
| PLANTA | el espejo del larguero dinamico sustituido | podia discrepar del lateral | **transporta la del lateral** |
| FRONTAL | retícula transversal | sin cambio | sin cambio |
| BOM | sin geometria | sin cambio | sin cambio |

### Goldens: dos pines movidos, con evidencia por primitiva

La frontera alta del escenario dorado esta en X=300, donde TERMINA una cabecera, asi que un intermedio ahi va
espejado. Los siete flags que la ronda 5 habia volteado vuelven, uno por uno, al valor que el dueño validó en la
ronda 4B, y los dos pines regresan EXACTAMENTE a los hashes que tenian entonces (`52386252…` y `02EE98BA…`) — la
comprobacion mas limpia de que la regla retirada era la anomalia:

| vista | pieza | ronda 5 | ahora |
|---|---|---|---|
| LATERAL | larguero alto X=300 Y=10.6053 | normal | espejado |
| LATERAL | larguero alto X=300 Y=82.6053 | normal | espejado |
| LATERAL | tope X=299.125 Y=94.1563 | espejado | normal |
| PLANTA | larguero alto X=300 Y=0.75 | normal | espejado |
| PLANTA | larguero alto X=300 Y=54.244 | normal | espejado |
| PLANTA | tope X=299.125 Y=1.5 | espejado | normal |
| PLANTA | tope X=299.125 Y=54.994 | espejado | normal |

Siete primitivas y en las siete cambia SOLO el espejo: ni una X, ni una Y, ni un anclaje, ni una rotacion, ni una
pieza, ni una cantidad. Los dos frontales y el BOM quedan intactos y sus pines no se tocan.

### Pruebas de contrato ACTUALIZADAS

- `PushBackRunFrameTests` y `PushBackPartialCompositeTests` afirmaban que la mano del ALTO sale del SENTIDO del
  flujo. Ahora la piden a la autoridad del intermedio; el larguero BAJO no entra en esta correccion y conserva la
  suya.
- `PushBackRearTopeAnchorTests` fijaba la mano del tope como una CONSTANTE en las elevaciones. Esa parte del
  contrato queda **explicitamente sustituida** por decision del dueño: el tope sigue a su larguero. En el marco
  identidad —donde el alto iba siempre espejado— las dos formulas dan el mismo valor, que es por lo que aquel
  escenario quedaba bien; lo que cambia es que ahora el tope SIGUE a su larguero en vez de ser una constante.

### Pruebas nuevas

`PushBackHighEndHandTests` (15 casos): la tabla de 3 a 8 fondos contra los apoyos INTERMEDIOS que el programa coloca
de verdad —no contra una regla reescrita en la prueba—, la coherencia lateral/planta en las cinco topologias, las
dos camas fisicas de unas encontradas tras la deduplicacion, el BOM invariante, y la prueba fuerte de la corrida
corta, que exige a la vez: la mano del intermedio, planta = lateral, el tope contrario a su larguero y **la X del
tope en 101.125, la que cerro la ronda 4B**.

Anulando la autoridad, cinco de los 15 fallan — incluidas exactamente las camas de 4, 5 y 7 fondos.

## 4-duodevicies. Correccion aislada 4B: en PLANTA la profundidad del tope la manda su larguero

### El defecto

Una corrida CORTA acaba DENTRO de la estructura: su extremo alto no cae en ninguna linea de postes. Medido en el
HEAD anterior, con el contacto alto en X = 101.845:

| vista | X del tope |
|---|---|
| LATERAL | 101.125 — del lado por el que llega la tarima |
| FRONTAL | correcto |
| **PLANTA** | **102.875** — al otro lado del larguero |

Aplicabilidad, pieza y conteo eran correctos. Solo la posicion longitudinal en planta estaba mal.

### La causa, y quien usa que

`PushBackRearTopeBuilder.PostMateWorld` busca el anclaje en el POSTE MAS CERCANO. Tiene exactamente DOS
llamadores de produccion:

| llamador | eje que resuelve | veredicto |
|---|---|---|
| `PushBackSystemFrontalBuilder` | en FRONTAL la X corre con la retícula TRANSVERSAL, y ahi el poste ES la linea | correcto, no se toca |
| `PushBackSystemPlantaBuilder` | en PLANTA la X corre con la PROFUNDIDAD | **la Y si, la X no** |

En planta los dos ejes no tienen la misma autoridad: la **Y** es transversal y la manda el poste; la **X** es
profundidad y la manda el LARGUERO ALTO de la cama. Cuando el extremo alto coincide con el poste del borde —lo
normal— las dos coinciden y nadie lo nota. Una corrida corta lo separa.

### La correccion

En la planta, la X del tope se mide desde la insercion del larguero alto que se acaba de colocar, con el MISMO
punto medido del poste y el MISMO signo de espejo que ya se usaban:

```
depth = largueroAlto.X + (espejado ? -anclaje.X : anclaje.X)
```

No se resta ninguna constante: solo cambia DESDE DONDE se mide. La Y sigue saliendo del mate del poste, que ahi
es exacto. `PostMateWorld` conserva su semantica y el frontal no cambia.

### Medido

| CASO | contacto alto | LATERAL | PLANTA antes | PLANTA ahora | delta |
|---|---|---|---|---|---|
| corrida corta A→B | 101.845 | 101.125 | **102.875** | 101.125 | 0 |
| corrida corta B→A | simetrico | igual al lateral | al otro lado | igual al lateral | 0 |
| corrida con el alto SOBRE el poste | 791.845 | 791.125 | 791.125 | 791.125 | 0 |
| encontradas | 395.85 / 396.15 | 395.12 / 396.88 | igual | igual | 0 |
| solo A / solo B | 395.85 / 396.15 | 395.12 / 396.88 | igual | igual | 0 |
| con calle y multifrente | por cama | por cama | igual | igual | 0 |

### Auditoria

| VISTA | AUTORIDAD DE LA X | RESULTADO EN LA CAMA CORTA |
|---|---|---|
| LATERAL | colocacion del larguero alto de la cama | 101.125 |
| FRONTAL A/B | poste (ahi la X es transversal) | correcto, sin cambio |
| PLANTA | **larguero alto de la cama** (antes: poste mas cercano) | 101.125 |
| BOM | sin geometria | sin cambio |

### Pruebas

`ShortRun_PlantaRearTopeUsesTheRunsHighPlacement` en los dos sentidos —identifica la cama corta por su contacto
alto, no por cercania, y exige que la planta coincida con el lateral y que NO quede nada al otro lado del
larguero— y `LegacyRearTopePlacement_IsUnchanged` para un rack de un solo sentido. Ademas, la comprobacion 1:1 de
la ronda 4 vuelve a exigir la X de la planta en TODOS los casos, incluida la corrida corta, que estaba exceptuada.
Tres de los 27 fallan sin la correccion.

**Ningun golden se movio.**

## 4-septendecies. Correccion aislada 4: el TOPE es del extremo ALTO de su cama

### Lo que ya estaba bien

Medido antes de tocar nada, en las cinco topologias: el LATERAL, los dos FRONTALES y el BOM ya seguian las camas.
Una corrida A→B pone su unico tope en X≈791 y solo aparece en el frontal posterior de B; una B→A en X≈0.88 y solo
en el de A; unas encontradas ponen dos, en 395.12 y 396.88. `PushBackRearTopeBuilder.ElevationY` no se toco.

### Los dos defectos, medidos

**1 — La PLANTA volvia a decidir la aplicabilidad.** El builder de un solo sentido pregunta «¿ALGUN nivel de este
frente tiene tope?» con un `Any` sobre TODOS los niveles, y la planta compuesta lo invoca por LOTE de camas. Un
lote no cubre todos los niveles del frente: los que pertenecen a otra topologia quedan fuera, pero su intencion
seguia contando.

Caso medido — ranura con el nivel 1 CORRIDO A→B (su alto esta en B, al final del rack) y el nivel 2 SOLO-A con su
tope APAGADO:

| vista | ANTES | AHORA |
|---|---|---|
| LATERAL | 791.12 | 791.12 |
| FRONTAL posterior de B | 791-equivalente | igual |
| FRONTAL posterior de A | vacio | vacio |
| BOM | 2 topes | 2 topes |
| **PLANTA** | 791.12 **y 395.12** (dos topes fantasma en la interfaz) | 791.12 |

Los dos topes de mas los pedia la intencion DORMANTE que el lado A guarda para el nivel corrido, que no es una
cama suya. Ninguna otra vista los dibujaba.

**2 — El BOM tomaba la VARIANTE del rack, no de la cama.** La aplicabilidad ya se preguntaba por cama
(`run.Source.RearTope`), pero el `PieceId` salia de `system.RearTope`, que en un compuesto es la configuracion de
un solo lado. Con topes distintos en A y en B:

| | ANTES | AHORA |
|---|---|---|
| dibujos | `LARGUERO_ESCALON_TOPE_DE_3` en A, `POSTE_3_1_5_8_TOPE` en B | igual |
| BOM | `LARGUERO_ESCALON_TOPE_DE_3` **x8** | `LARGUERO_ESCALON_TOPE_DE_3` x4 + `POSTE_3_1_5_8_TOPE` x4 |

### Las correcciones

Ninguna autoridad nueva. Se ACOTA la entrada y se pregunta a la cama:

- `PushBackCompositePlanta` construye cada lote con la intencion de tope **acotada a las camas de ese lote**: los
  niveles que el lote no materializa viajan apagados con el mecanismo de siempre —las celdas OFF—, y la decision
  la sigue tomando el mismo builder.
- `PushBackBomBuilder.AddRunRearTopes` agrupa por `(pieza de la cama, longitud)`: la variante sale de
  `run.Source.RearTope`, la misma que ya decidia si el tope existe.

### Auditoria de autoridades

| CONSUMIDOR | AUTORIDAD ANTES | AUTORIDAD DESPUES |
|---|---|---|
| intencion | configuracion por lado (dormante incluida) | la misma |
| run | `PushBackRuns` → `HighSide` / `HighSupport` | la misma |
| LATERAL | la cama, en su marco | la misma |
| FRONTAL | la cama, en el corte del lado alto | la misma |
| **PLANTA** | `Any` sobre todos los niveles del lado | **las camas del lote** |
| **BOM (aplicabilidad)** | la cama | la misma |
| **BOM (variante)** | `system.RearTope` (sesgada a un lado) | **`run.Source.RearTope`** |

Ninguna vista vuelve a decidir aplicabilidad.

### Un defecto NUEVO, medido y NO corregido

Una corrida CORTA —cuyo extremo alto cae DENTRO de la estructura, no en una linea de postes— recibe en PLANTA una
X distinta de la del lateral: **lateral 101.125, planta 102.875**, con el contacto alto en 101.845. El lateral lo
pone del lado por el que llega la tarima; la planta, al otro lado del larguero.

La causa es que en planta el punto de anclaje se busca en el POSTE MAS CERCANO
(`PushBackRearTopeBuilder.PostMateWorld`), y el extremo alto de una corrida corta no cae en ninguna linea de
postes. Corregirlo toca esa funcion, que comparten las TRES vistas y tambien un rack de un solo sentido — fuera
del alcance declarado de esta corrida. Queda REPORTADO, con su medida y su causa. La aplicabilidad, la pieza, el
conteo, el lateral y el frontal de esa misma corrida corta SI son correctos.

### Pruebas

`PushBackRearTopeRunAuthorityTests` (24 casos). Derivan lo esperado de CADA cama con la misma autoridad que coloca
la pieza —el builder en el marco de la cama, mas la reflexion rigida— y exigen correspondencia **1:1** en lateral,
frontal, planta y BOM: existencia, lado alto, X, Z, ranura, nivel y `PieceId`. Cubren las quince situaciones del
encargo. Dos de los 24 fallan sin la correccion.

**Ningun golden se movio.**

## 4-sexdecies. Correccion aislada 3: el DATUM de «Alto 1er nivel» se transporta, no se re-decide

### Las dos perdidas de autoridad, medidas

Retícula real del poste: base 0.6053", paso 2", troquel utilizable mas bajo 0.6053".

**A — la estructura compuesta perdia el datum.** `CopySharedStructuralIntent` copiaba pallet, peraltes, poste,
separadores, cabecera, anotaciones… y NO `FirstLevelDatum`. La sub-estructura de cada lado y la compuesta leian
«Alto 1er nivel» con la semantica HISTORICA:

| Alto 1er nivel | solo A | al declarar B |
|---|---|---|
| 7" | LOW = 8.6053 | LOW = **6.6053** |
| 5" | LOW = 6.6053 | LOW = **4.6053** |

Declarar el lado B bajaba el primer nivel del lado A **un troquel entero**, sin que nadie lo pidiera.

**B — la ventana FABRICABA el datum.** `ReadInputs` construye unos inputs NUEVOS en cada recalculo, y sus valores
por defecto imponian el datum del PRODUCTO. Un documento guardado con la semantica historica y Alto = 5 pasaba de
LOW 4.6053 a **6.6053** al recalcular.

Los valores son IMPARES a proposito: con valores pares las dos lecturas caen en el mismo troquel y el defecto no
se ve.

### Lo corregido

1. `CopySharedStructuralIntent` TRANSPORTA el datum. Solo eso: no convierte.
2. La ventana lo guarda en un campo al cargar y lo devuelve en `ReadInputs`. Deja de ser autoridad.
3. El diseno con el que la ventana reconstruye el lado B lleva el datum del documento; sin el, la matriz de B
   volvia con la semantica historica mientras la compuesta usaba la del documento.
4. **UNA sola conversion**, en `PushBackEditorState.LoadFromDesign`: un documento SIN marcador se re-expresa sobre
   el datum del producto MIDIENDO su geometria ya resuelta —`RackFirstLevelDatum.ToLowestPunchOffset` sobre la
   retícula real— y no se mueve ni una milesima. No se resta ninguna constante.

`PushBackElevations` no se toco. La fisica del HIGH tampoco: pendiente, `HighInsertion`, tie-break, orientacion,
`CorridaDepth` y `ResolveSpan` siguen igual, y ninguna prueba de HIGH cambio.

### Auditoria de autoridades

| SEAM | DATUM QUE ENTRA | DATUM QUE SALE | CONVERSION |
|---|---|---|---|
| Persistence (`PushBackDesignDocument`) | campo del archivo (ausente = historico) | el mismo | ninguna |
| `LoadExisting` / `LoadDesignForNew` | el del diseno | el mismo | ninguna |
| **`PushBackEditorState.LoadFromDesign`** | el del diseno | `LowestUsablePunch` | **LA UNICA** — legacy re-expresado midiendo su troquel |
| Window state (`firstLevelDatum`) | el que la carga devolvio | el mismo | ninguna |
| `ReadInputs` | el campo de la ventana | el mismo | ninguna (antes: FABRICABA) |
| `PushBackEditorDesignAssembler` | `inputs.FirstLevelDatum` | `design.Structure.FirstLevelDatum` | ninguna |
| `CopySharedStructuralIntent` | el del diseno compartido | el mismo | ninguna (antes: LO PERDIA) |
| Composite structure (`Compose` / `SideStructuralDesign`) | el copiado | el mismo | ninguna |
| `DynamicRackSystemResolver` | `design.FirstLevelDatum` | lo aplica y lo copia al sistema | ninguna |
| `PushBackElevations` | las elevaciones ya resueltas | las mismas | ninguna |

Una sola conversion, en una frontera identificada. Todo lo demas, TRANSPORTE.

### Medido, antes y despues

| CASO | LOW ANTES | LOW TRAS RECALCULAR | LOW TRAS GUARDAR/ABRIR |
|---|---|---|---|
| nuevo, Alto = 0 | 0.6053 | 0.6053 | 0.6053 |
| nuevo, Alto = 5 | 6.6053 | 6.6053 | 6.6053 |
| nuevo, Alto = 7 | 8.6053 | 8.6053 | 8.6053 |
| legacy, Alto = 5 | 4.6053 | 4.6053 (guarda 4, datum nuevo) | 4.6053 |
| legacy, Alto = 7 | 6.6053 | 6.6053 (guarda 6, datum nuevo) | 6.6053 |
| A antes de declarar B (Alto = 7) | 8.6053 | — | — |
| A despues de declarar B | 8.6053 (antes **6.6053**) | 8.6053 | 8.6053 |
| B con la misma intencion visible | 8.6053, mismo troquel que A | — | — |
| Dinamico legacy | su geometria historica, sin cambio | — | — |

### Pruebas

`PushBackFirstLevelDatumTransportTests` (29 casos) y dos de ventana:
`Window_Recompute_DoesNotMoveALegacyRacksFirstLevel` y `RackEditar_CompositeKeepsBothSidesOnTheSamePunch`.
Afirman marcador, numero persistido, **indice de troquel** —comprobando ademas que la elevacion cae EN la
retícula—, LOW, LOW del lateral, LOW del frontal, alineacion A/B y round-trip. Ocho de las 29 y las dos de ventana
fallan sin la correccion.

Se ACTUALIZO una prueba de contrato: `LoadedDesign_KeepsItsOwnFirstLevelHeight_NotTheNewRackDefault` afirmaba que
un documento legacy con Alto = 9 vuelve mostrando 9. Con la conversion vuelve mostrando 8, porque 9 se ajustaba al
troquel 8.6053 y ese troquel medido desde el utilizable mas bajo son 8". Lo que la prueba defiende —cargar NO es
un diseno nuevo, el valor no cae al 4" del rack nuevo— sigue en pie, y ahora ademas afirma la GEOMETRIA, que es lo
que el encargo pide comparar.

**Ningun golden se movio.**

## 4-quindecies. Correccion aislada 2C: un corte FRONTAL proyecta solo las lineas de SU lado

### El contrato

La estructura fisica global es la UNION de lo que necesitan los dos lados:

```
GlobalHeaderLineExists(line) = NeedsA(line) || NeedsB(line)
```

y la PLANTA la dibuja entera, porque representa el rack. Un corte FRONTAL no: es de un lado.

```
FrontalA muestra line = NeedsA(line)
FrontalB muestra line = NeedsB(line)
```

### Lo que el dueño vio

Ranura 1 en blanco en A, con almacenamiento en B. La linea exterior de esa ranura existe en el rack porque B la
necesita —y la planta la dibujaba bien—, pero el corte frontal de A la dibujaba tambien, sin poseerla.

| vista | lineas ANTES | lineas AHORA |
|---|---|---|
| estructura global | 0, 1, 2, 3 | 0, 1, 2, 3 |
| PLANTA | 0, 1, 2, 3 | 0, 1, 2, 3 |
| FRONTAL de B | 0, 1, 2, 3 | 0, 1, 2, 3 |
| **FRONTAL de A** | **0**, 1, 2, 3 | 1, 2, 3 |

### La causa y la correccion

El corte de un lado se construye sobre la sub-estructura de ESE lado, pero decidia sus lineas con
`BoundaryExists`, que lleva una excepcion: «los bordes exteriores del rack existen siempre». Es cierta para el
RACK y falsa para un lado. Con la primera ranura en blanco en A, el borde 0 pasaba esa excepcion y el corte de A
lo dibujaba aunque no tuviera ni un claro suyo al lado.

`DynamicFrontActivation.BoundaryBelongsTo` es la MISMA continuidad —una linea sostiene algo si tiene un claro
activo a izquierda o a derecha— sin esa excepcion, evaluada sobre la activacion del lado. El corte frontal
compuesto la inyecta como filtro OPCIONAL, igual que el desviador en la correccion 1B: sin filtro —cualquier rack
de un solo sentido— el corte es exactamente el de siempre y los dos bordes del rack siguen existiendo aunque su
primera ranura este en blanco.

El filtro alcanza a lo que va ATORNILLADO a esa linea —poste, placa, botas, protectores, defensas, desviador—,
porque una linea que no se dibuja no puede llevar nada encima. **Solo es proyeccion**: no cambia `Compose`, ni el
`BoundaryExists` global, ni la retícula, ni los modulos, ni el BOM, ni el lateral, ni la planta.

### Medido

| CASO | GLOBAL | FRONTAL A | FRONTAL B | PLANTA |
|---|---|---|---|---|
| A en blanco / B activo (ranura 1 de 3) | 0,1,2,3 | **1,2,3** | 0,1,2,3 | 0,1,2,3 |
| B en blanco / A activo (ranura 1 de 3) | 0,1,2,3 | 0,1,2,3 | **1,2,3** | 0,1,2,3 |
| blanco interior aislado, solo A | 0,1,2,3 | 0,1,2,3 | 0,1,2,3 | 0,1,2,3 |
| dos blancos seguidos, solo A (de 4) | 0,1,2,3,4 | **0,1,3,4** | 0,1,2,3,4 | 0,1,2,3,4 |
| en blanco en A y en B | 0,1,2,3 | **1,2,3** | **1,2,3** | 0,1,2,3 |

El corte POSTERIOR sigue la misma regla, comprobado lado por lado.

### Pruebas

`PushBackFrontalHeaderOwnershipTests` (9), que afirman el ÍNDICE de cada linea —identificado con la formula del
propio layout, no por cercania—, la vista, el lado y el conteo exacto, mas los `ModuleId` y la longitud del rack
para fijar que la fisica global no se movio. **Cuatro de las nueve fallan** sin la correccion; las otras cinco ya
eran correctas, incluida la de los dos blancos seguidos —ahi la regla de continuidad ya acertaba— y la de un rack
de un solo sentido, que conserva sus dos bordes.

**Ningun golden se movio.**

## 4-quaterdecies. Correccion aislada 2B: la declaracion fisica del lado B sobrevive a «En blanco»

### Lo que quedaba abierto

La correccion 2 dejo los dos lados con la MISMA regla pero con datos asimetricos. El lado A declara su ausencia
aparte (`AbsentSlotsA`) y conserva su frente en el diseno, asi que su declaracion fisica sobrevive. El lado B la
expresaba con una entrada NULA en su propia lista, de modo que una ranura cuyo ancho declaraba solo B lo perdia al
ponerla en blanco: las lineas pasaban de `0, 97.49, 150.99, 204.48` a `0, 53.49, 106.99, 160.48` y todo lo que
venia detras se movia. Dos semanticas incompatibles para la misma decision del dueño.

### El esquema elegido: ADITIVO y simetrico

`PushBackCompositeDesign.AbsentSlotsB`, hermana exacta de `AbsentSlotsA`, con su campo homonimo en el documento.
El frente de una ranura EN BLANCO del lado B viaja **completo** —con su `IsActive` en false— y quien dice que no
almacena es la lista. Nada mas cambia de forma.

Compatibilidad, sin migracion destructiva:

| documento | `SideB.Fronts[slot]` | `AbsentSlotsB` | se lee como |
|---|---|---|---|
| anterior, ranura con almacenamiento | frente | ausente | almacena — igual que siempre |
| anterior, ranura en blanco | **null** | ausente | no almacena, y **no se le inventa** declaracion fisica |
| nuevo, ranura en blanco | frente completo | contiene la ranura | no almacena, y conserva ancho, BFR y override |

El campo se OMITE del JSON cuando esta vacio, asi que un rack sin ninguna ranura en blanco escribe exactamente el
mismo archivo que antes.

### El orden del round-trip

`LoadCompositeFromDesign` aplicaba la presencia del lado B **antes** de `LoadFromDesign`, que rehace la matriz
entera desde el diseno resuelto — y para una ranura en blanco fabricaba un relleno `PalletCount = 1` sin
`IsActive`. Al reabrir un rack guardado, las ranuras que el usuario habia dejado en blanco volvian ACTIVAS y con
un ancho por defecto. Ahora el frente viaja completo, el relleno legacy nace inactivo y la presencia se aplica
DESPUES de reconstruir la matriz. Medido: sin ese cambio, reabrir un documento de forma anterior resucita la
ranura; con el, no.

### Auditoria de las dos semanticas

| | ANTES | DESPUES |
|---|---|---|
| lado A, blanco | frente en el diseno + `AbsentSlotsA` | igual |
| lado B, blanco | **entrada nula** (pierde la declaracion fisica) | frente en el diseno + `AbsentSlotsB` |
| declaracion fisica | `StoredFront(slot)` — solo la tenia A | `StoredFront(slot)` — la tienen los dos |
| almacenamiento activo | `Front(slot)` | igual |
| persistencia | `AbsentSlotsA` | `AbsentSlotsA` + `AbsentSlotsB`, aditivo |

No quedan dos semanticas: la entrada nula solo sobrevive como LECTURA de documentos anteriores, y ya no se
escribe.

### Medido

| caso | ancho antes | ancho en blanco | ancho al reactivar | almacenamiento |
|---|---|---|---|---|
| ancho solo en B | 97.49 | 97.49 (antes **53.49**) | 97.49 | B a 0, A intacto |
| override de larguero solo en B | 111 | 111 (antes se perdia) | 111 | B a 0 |
| blanco interior en B | lineas y ranuras posteriores identicas | — | — | solo esa ranura |
| patron no contiguo en B | identico tras guardar y abrir | — | — | ranuras 1 y 3 sin B |
| round-trip | identico | — | — | sin resurreccion |
| RACKEDITAR | identico, tambien con un documento anterior | — | — | sin resurreccion |

### Pruebas

`PushBackBlankSideBDeclarationTests` (9) y `BlankB_RackEditarPreservesBlankState` en la ventana real, que ademas
degrada el diseno a la forma anterior para comprobar los dos caminos. **Cinco de las nueve fallan** si se
reinstaura la entrada nula, y la de RACKEDITAR falla si se reinstaura el orden anterior. Las 11 de la correccion
2 siguen verdes.

Se ACTUALIZO una prueba de contrato: `ASlotCanBeRetiredFromOneSide_AndItsConfigurationStaysDormant` afirmaba que
el frente en blanco de B viajaba como null. Es justo lo que esta corrida cambia, asi que ahora afirma lo que la
prueba queria decir: el frente viaja completo e INACTIVO, la ranura esta declarada en `AbsentSlotsB` y sus
niveles siguen dormidos.

**Ningun golden se movio.** La UX no cambia: sigue habiendo una sola casilla, «En blanco».

## 4-terdecies. Correccion aislada 2: «EN BLANCO» conserva la RANURA FISICA

### La decision

Una ranura EN BLANCO existe fisicamente en la retícula; lo que no existe es el ALMACENAMIENTO de ese lado en ella.
No borra el slot, no compacta indices, no mueve los frentes posteriores y no retira la estructura.

Son TRES preguntas distintas y no un solo booleano:

1. la RANURA fisica — la posicion transversal del rack;
2. el ALMACENAMIENTO de un lado en esa ranura;
3. la NECESIDAD de una LINEA de cabecera — si sostiene algo a izquierda o a derecha.

### La causa raiz

`PushBackCompositeStructure.SideStructuralDesign` **saltaba** con `continue` toda ranura sin frente en ninguno de
los dos lados, mientras `Compose` la conservaba. Resultado: retícula compuesta de N frentes y sub-estructura local
de N-1. Y como el puente ranura → indice local es la IDENTIDAD —lo dice el propio comentario del resolver—, cada
ranura posterior al blanco leia la configuracion de la SIGUIENTE y la ultima se quedaba sin ninguna.

Medido con 3 ranuras y la primera en blanco en los dos lados, antes de tocar nada:

```
A: localFronts=2  map=[0,1,2]  fronts=[-,F,-]
runs: [s1/n1 A->A] [s1/n1 B->B] [s1/n2 A->A] [s1/n2 B->B]      <- la ranura 2 no tiene ninguna
```

La ranura 1 recibia la estructura de la 2 y la 2 desaparecia entera. Es exactamente lo que el dueño vio como
«poner F1 en blanco borra tambien F3». El mismo defecto con la ranura del MEDIO en blanco (perdia la 3) y con dos
blancos seguidos en un rack de 4 (perdia la 4).

### La estrategia: PRESERVAR LOS SLOTS

De las dos opciones, la preferida: **la sub-estructura de cada lado conserva TODOS los slots fisicos**, con una
representacion en blanco, igual que `Compose`. Asi `localIndex == slot` es cierto POR CONSTRUCCION y no hace falta
volver real el mapa ni repartir `-1` por los consumidores. No se mezcla con la otra estrategia: no queda ninguna
compactacion implicita.

Y una segunda lectura, porque la ranura fisica y el almacenamiento son preguntas distintas:
`PushBackSideConfiguration.StoredFront(slot)` devuelve la DECLARACION FISICA de la ranura tenga o no
almacenamiento, y de ella salen el ancho de la bahia y el override de larguero en `Compose`. Sin eso, marcar una
ranura en blanco encogia su bahia a una calle y corria todas las lineas posteriores — medido: con la ranura 0 a
dos calles, las lineas pasaban de `0, 97.49, 150.99, 204.48` a `0, 53.49, 106.99, 160.48`.

### Lineas de cabecera

`NeedsHeaderLine` ya existe y no hacia falta inventarla: es `DynamicFrontActivation.BoundaryExists`, que se
pregunta por CONTINUIDAD —los dos bordes exteriores siempre existen; una linea interior existe si alguno de sus
dos claros esta activo— sobre la retícula COMPUESTA, cuyo `IsActive` es la UNION de los dos lados. De ahi salen
las tres reglas sin ningun caso especial: `A|B|A` conserva todas sus lineas, `A|B|B|A` no tiene la de en medio, y
una ranura en blanco solo en A conserva la suya porque B almacena ahi.

### Medido, antes y despues

| escenario | ranuras | lineas presentes | camas | antes |
|---|---|---|---|---|
| F1 en blanco (A+B) | 3 | 0,1,2,3 | ranuras 1 y 2 completas | **la ranura 2 perdia todas** |
| F2 en blanco (A+B) | 3 | 0,1,2,3 | ranuras 0 y 2 completas | **la ranura 2 perdia todas** |
| F3 en blanco (A+B) | 3 | 0,1,2,3 | ranuras 0 y 1 completas | igual |
| F2+F3 en blanco (A+B) | 4 | 0,1,**3**,4 | ranuras 0 y 3 completas | **la ranura 3 perdia todas** |
| en blanco solo en A | 3 | 0,1,2,3 | la ranura conserva las camas de B | igual |
| en blanco solo en B | 3 | 0,1,2,3 | la ranura conserva las camas de A | igual |
| en blanco en A+B | 3 | todas | hueco fisico, sin camas | igual |
| patron no contiguo (F2 y F4 de 5) | 5 | todas | ranuras 0, 2 y 4 completas | **la 4 perdia todas** |

Las posiciones de las lineas, los indices de ranura, la longitud del rack y los `ModuleId` de I-40 son IDENTICOS
al mismo rack sin el blanco en los ocho escenarios.

### Un defecto NUEVO, medido y NO corregido

Una ranura cuyo ancho lo declara **solo el lado B** pierde ese ancho al ponerla en blanco EN B: sus lineas pasan
de `0, 97.49, 150.99, 204.48` a `0, 53.49, 106.99, 160.48`. El caso simetrico —ancho solo en A, en blanco en A—
**si** queda corregido.

La razon es que los dos lados representan la ausencia de forma distinta: A la declara aparte (`AbsentSlotsA`) y
conserva su frente en el diseno, mientras B la expresa con una entrada NULA en su propia lista, de modo que de una
ranura suya en blanco no queda declaracion fisica que recuperar. Hacerlo simetrico es cambiar la forma persistida
del lado B, que es justo el area del fallo de resurreccion de presencia de B que el encargo manda no abrir en esta
corrida. Queda REPORTADO, sin tocar.

### Pruebas

`PushBackBlankSlotIdentityTests` (11), con aserciones POR RANURA contra el mismo rack sin el blanco: ranuras,
posiciones de linea, lineas presentes, camas por ranura, identidad `run.Slot == run.SourceFrontIndex`, celdas sin
camas en las ranuras en blanco, `ModuleId` estables y el patron no contiguo sobreviviendo a guardar y reabrir.
**Seis de las once fallan sin la correccion.** Ningun golden se movio: la retícula no cambia donde no habia
defecto.

## 4-duodecies. Correccion aislada 1B: la FRONTAL coincide con la LATERAL

### La regla

El lateral es el oraculo: el dueño lo valido. La frontal no vuelve a resolver la fisica. Para una misma
`PushBackRun`, el corte lateral, el frontal de entrada/salida y el frontal posterior son solo tres PROYECCIONES de
la misma cama: el mismo LOW, el mismo HIGH, los mismos largueros, los mismos elementos aplicables.

### Lo medido antes de tocar nada

Reproducido por el camino real —`PushBackCompositeEditorAssembler` → `PushBackRuns` → los tres builders— con los
dos lados a alturas deliberadamente distintas (A = 4", B = 18") para que leer el lado equivocado se viera en la Z.

Las **elevaciones** de los largueros ya coincidian en las tres vistas, en las diez topologias probadas: corrida
A→B y B→A, un nivel y varios, niveles asimetricos por lado, calle, corrida corta, ranuras mixtas y direcciones
opuestas en la misma linea. Ese no era el defecto.

Lo que NO coincidia:

| pieza / vista | lateral (oraculo) | frontal ANTES | frontal AHORA |
|---|---|---|---|
| DESVIADOR, pasillo B de una corrida A→B | ninguno | **6 piezas** en Z = 18.6053 y 84.6053 | ninguno |
| DESVIADOR, pasillo A de una corrida B→A | ninguno | **6 piezas** en Z = 4.6053 y 70.6053 | ninguno |
| DESVIADOR por nivel, N1 A→B / N2 B→A | cada nivel en su pasillo | los dos niveles en los dos pasillos | cada nivel en su pasillo |
| plan que el panel muestra en «posterior de B» | corte posterior | **corte de entrada/salida** | corte posterior |
| lado de los cortes construidos | — | el lado que se esta EDITANDO | el lado del selector de VISTA |

### Las tres causas

**1. La seguridad se saltaba el filtro de celdas entero.** `FilterCells` solo filtraba los largueros IN/OUT; todo
lo demas pasaba tal cual. El desviador se emite por POSTE sobre el sistema LOCAL de cada lado, asi que el pasillo
alto de una corrida —que no carga nada— dibujaba desviadores a la altura de SUS propios niveles. Es el mismo
defecto que la correccion 1 quito del lateral, vivo en la otra vista: un desviador en el pasillo alto es
literalmente inventar un segundo LOW.

La correccion pasa la PERTENENCIA al punto de emision. `AppendFrontal` acepta un predicado OPCIONAL
`(poste, nivel)` que el Push Back compuesto deriva del MISMO conjunto de camas que gobierna los largueros; un
poste es la frontera de hasta dos claros, asi que lleva desviador si cualquiera de los dos tiene cama en ese
nivel — la misma adyacencia con la que el builder compartido ya decide su existencia y sus niveles. Sin predicado
—cualquier rack de un solo sentido— el corte es byte-identico al de siempre. **No se decide por cercania**: la
identidad viene del run, no de una X.

**2. La seccion frontal se comparaba sin decodificar.** El panel elegia el plan con
`section == (int)PushBackFrontalEnd.Posterior`. La seccion lleva EXTREMO y LADO desde I-42: la 3 —posterior de
B— no es 1, asi que caia en el corte de entrada/salida. Solo acertaba en el lado A. Ahora se decodifica.

**3. Los cortes frontales se construian para el lado que se esta EDITANDO.** `BuildFrom` recibia
`composite.ActiveSide` mientras el rotulo y la seccion usaban el selector de vista: con «Editando A» y «Frontal de
B» el panel mostraba el pasillo de A llamandolo B. Y cambiar el selector solo repintaba, sin reconstruir, de modo
que el plan seguia siendo el del lado anterior hasta que otra edicion forzara un recalculo. El corte frontal es
una VISTA: sigue a su selector.

### Auditoria de consumidores del desviador, actualizada

| consumidor | autoridad hoy | estado |
|---|---|---|
| dominio / intencion | la del usuario | correcta, intacta |
| LATERAL compuesto | run → LowEnd | corregido en la correccion 1 |
| LATERAL de un solo sentido | builder dinamico | correcto: ahi izquierda si es el extremo bajo |
| **FRONTAL compuesto** | **pertenencia derivada de las camas** | **CORREGIDO en esta corrida** |
| PLANTA | clasificacion por LINEA (`LoadingAisles`) | **PENDIENTE** — por linea, no por cama |
| BOM | `SystemBomBuilder` compartido | **PENDIENTE** — otra autoridad |

Las dos pendientes exigen tocar `SelectiveSafetyEnds` / `SystemBomBuilder`, compartidos con el Selectivo y el
Dinamico, y siguen fuera del alcance declarado.

### Pruebas

`PushBackFrontalMatchesLateralTests` (16) recorre CADA cama y afirma, contra el lateral que el dueño valido, la Z
baja y la alta, la COLUMNA de su ranura —calculada con la misma formula con la que el builder la coloca, no
buscada por proximidad—, el `PieceId`, el peralte del lado que corresponde, que el lado contrario no inventa un
segundo extremo y que no sobra ningun larguero que ninguna cama pida. Los diez casos obligatorios estan cubiertos.
`TheFrontalCuts_FollowTheViewSelector_NotTheEditedSide` fija el seam de la ventana con los controles reales.

**Ningun golden se movio**: la geometria de los largueros no cambia en ninguna vista; lo que desaparece son
desviadores que ninguna cama pedia.

## 4-undecies. Correccion aislada 1: el DESVIADOR, con el LOW del run como unica autoridad

### El contrato

`Diverter(run) = LowEnd(run)`. Un desviador guia la tarima al ENTRAR, asi que pertenece siempre al extremo por el
que se carga ESA cama. No depende del lado A/B, ni de izquierda/derecha en coordenadas de rack, ni del extremo
alto, ni de la elevacion que el resolver compartido le dio al nivel.

### La causa raiz

El contenido compuesto se construye por camas —`PushBackRuns` → `PushBackCompositeContent`— pero el DESVIADOR
habia quedado fuera de ese pipeline: el corte lateral compuesto lo heredaba del builder dinamico, que conserva la
regla de un rack de un solo sentido, **izquierda = extremo bajo, derecha = extremo alto**. En un rack compuesto
eso es falso: el lado B tiene su entrada a la DERECHA. El lado A acertaba por coincidencia.

Ademas, una clasificacion por LINEA no puede expresar que en la MISMA linea el nivel 1 corra A→B y el nivel 2
B→A. La cama si tiene esa granularidad.

### Medido antes y despues

| caso | LOW esperado | desviador ANTES | desviador AHORA |
|---|---|---|---|
| Solo A | X=0, Z=70.6053 | (0, 70.6053) ✔ | (0, 70.6053) |
| Solo B | X=792, Z=70.6053 | (792, **100.6053**) ✘ | (792, 70.6053) |
| Encontradas A | X=0, Z=70.6053 | (0, 70.6053) ✔ | (0, 70.6053) |
| Encontradas B | X=792, Z=70.6053 | (792, **100.6053**) ✘ | (792, 70.6053) |
| Corrida A→B | X=0 unicamente | (0, 70.6053) ✔ | (0, 70.6053) |
| Corrida B→A | X=792 unicamente | (**0**, 70.6053) ✘ | (792, 70.6053) |
| N1 A→B / N2 B→A | 2 piezas: (0, N1) y (792, N2) | **4 piezas**, dos por extremo ✘ | 2 piezas, cada nivel en su extremo |
| A=4" / B=18" | B nivel 2 en Z=84.6053 | (792, **100.6053**) ✘ | (792, 84.6053) |

El ultimo caso es tambien el de la seccion 7 del encargo: el corte FRONTAL de B ya decia 84.6053, asi que el
lateral y el frontal discrepaban. Ahora coinciden.

### La correccion

`PushBackDiverterPlan` construye el desviador **en el marco de la cama**, donde el flujo avanza siempre hacia +X
y por tanto el extremo bajo es el arranque del frente. Toma la Z de `PushBackElevations.LowInsertions` del sistema
de esa cama, y el primer nivel conserva su contrato Selectivo (mide desde el troquel del poste). La posicion y la
mano en el mundo las pone la MISMA reflexion rigida que ya mueve el riel, los rodillos, los largueros y el tope.

`PushBackCompositeContent.Lateral` lo emite por lote, y el corte lateral compuesto RETIRA los desviadores del
plan dinamico base para que no queden dos autoridades. **`PushBackElevations` no se toco.** Un rack de un solo
sentido no pasa por aqui y su comportamiento es identico.

### Auditoria de consumidores del desviador

| consumidor | autoridad hoy | estado |
|---|---|---|
| dominio / intencion (`SelectiveSafetySelection`, off-cells) | la del usuario | correcta, no se toca |
| LATERAL compuesto | **run → LowEnd → `PushBackElevations` del lado** | CORREGIDO en esta corrida |
| LATERAL de un solo sentido | builder dinamico (legacy) | correcto: ahi izquierda SI es el extremo bajo |
| FRONTAL por lado | sistema LOCAL del lado + contexto de elevaciones bajo | ya era correcto; comprobado contra el lateral |
| PLANTA | clasificacion por LINEA (`LoadingAisles`) | **PENDIENTE**: aproximacion por linea, no por cama |
| BOM | `SystemBomBuilder` compartido, por seleccion y postes | **PENDIENTE**: otra autoridad, ronda Safety/BOM |

Las dos filas pendientes exigen tocar `SelectiveSafetyEnds` / `SystemBomBuilder`, que es infraestructura
compartida con el Selectivo y el Dinamico. Por instruccion expresa del encargo **no se tocan en esta corrida** y
quedan reportadas para la ronda de Safety/BOM.

## 4-octies. Auditoria adversarial final (seccion T)

Despues de cerrar 4, 5, 6 y 10 se recorrio el checklist del dueño buscando lo que quedara vivo. **Se encontraron
dos cosas, y las dos se corrigieron**; el resto se declara medido.

### ENCONTRADO 1 — el desviador del extremo ALTO seguia leyendo la elevacion del resolver

`DynamicSafetyLateralBuilder` dibuja los DOS extremos en UNA sola pasada, asi que es el unico sitio donde una
vista necesita las dos elevaciones a la vez. El desviador de la izquierda ya consultaba el contexto; el de la
derecha usaba `rightLoad.EntranceElevation` — correcto mientras el alto era el ancla, y falso desde la
inversion. Se quedaba colgando de un larguero que ya no esta ahi.

**La correccion**: el contexto de elevaciones lleva ahora un ACOMPAÑANTE del extremo alto
(`RackLevelElevations.HighEnd`), que `PushBackElevations.Context` rellena. Ningun sistema con una sola elevacion
por nivel —el Dinamico— lo rellena, y sin el nada cambia. La prueba es falsable a proposito: comprueba que el
desviador cae en la elevacion DERIVADA **y** que esa elevacion NO es la del resolver.

### ENCONTRADO 2 — el `LocateCell` del corte posterior

Ya descrito con el error 6: buscaba la celda comparando contra la elevacion del resolver, asi que tras la
inversion no encontraba ninguna y se perdian EN SILENCIO el tope posterior y el filtro de celdas de una corrida.

### DECLARADO — el rack nunca se queda corto

La altura de cabecera y la longitud del poste se calculan con la elevacion del RESOLVER, y I-32 fijo que la
correccion de pendiente NO las mueve. Con la inversion el larguero alto BAJA, asi que la pregunta es si alguna
vez SUBE por encima de aquella elevacion — porque entonces el poste se quedaria corto. **Medido en 108
combinaciones** de fondo (2..12), niveles (1, 2, 4) y altura inicial (4, 6, 12, 30): nunca. El peor caso deja el
alto derivado 4" por DEBAJO. Queda fijado como prueba.

### RECORRIDO Y MEDIDO, sin hallazgos

| Area | Que se busco | Resultado |
|---|---|---|
| ELEVACIONES | `ChooseLowTroquel`, `legacyInsertion`, `TheoreticalExitY` | no existen; se eliminaron con la politica |
| ELEVACIONES | quien lee `EntranceElevation` en Push Back | solo como FALLBACK de `LocateCell`, que es su contrato |
| ELEVACIONES | autoridad duplicada | los 10 consumidores de `PushBackElevations` pasan por `Resolve`/`Context` |
| ELEVACIONES | datum y migracion | `RackFirstLevelDatum` intacto; sus 26 pruebas verdes |
| GEOMETRIA | mates bajo/alto, hueco, espejos, camas parciales | 24 escenarios especiales en `PushBackRunFrameTests` |
| ESTRUCTURA | fuga de un frente remoto | prueba explicita con 5/5/9 |
| ESTRUCTURA | identidad por linea | `ModuleId` y `Kind` identicos tras acortar; override de I-40 sigue aplicando |
| ESTRUCTURA | round trip | la cobertura se RECONSTRUYE al reabrir (los tramos son derivados y no se persisten) |
| TOPES | intencion / aplicabilidad / materializacion | tres conceptos separados, con prueba de que la promesa coincide con el dibujo |
| TOPES | divergencia planta / lateral / BOM | planta y lateral coinciden pieza a pieza; el BOM cuenta camas |
| SEGURIDAD | regresion de 91fc259 | editar topes no mueve ni una pieza; ninguna seleccion adquiere `Side = Both` |
| UI | lado A/B/Ambos, primer nivel, frente seleccionado | las pruebas de las rondas anteriores siguen verdes |
| PERSISTENCIA | forma en disco | `DepthSegments` vive en el DISEÑO, que nunca se serializa: el DTO del documento no cambia |
| PUSH BACK | GUIA | sigue sin admitir ninguna, como desde I-18 |

## 5. Limitaciones DECLARADAS (no son descuidos)

1. **El separador central exige hueco mayor que cero**; con hueco 0 se avisa y no se coloca.
2. **Una ranura presente en LOS DOS lados tiene estructura a lo largo de toda la profundidad**: su fondo por lado
   gobierna donde acaban sus CAMAS, no donde acaban sus marcos — la misma regla de I-41.
3. **Un ajuste manual de estructura se aplica a todas las ranuras de ese lado.** Sin ajuste manual cada ranura
   conserva su envolvente, asi que frentes cortos y largos conviven igual que en I-41.
4. **Una ranura ausente en una posicion INTERIOR** deja en el corte frontal de ese lado la linea de postes de su
   frontera (regla de I-33 sobre frentes en blanco). Las ausencias del final si se retiran — el caso habitual.
   Se validara visualmente: es estructura fisica compartida.
5. **Una corrida que cruza el hueco** lo atraviesa sin gastar demanda en el, pero su longitud FISICA si lo
   incluye: para llegar a su ultimo fondo tiene que salvarlo. Su extremo bajo apoya siempre en una linea de modulo
   real; **conviene mirarlo en AutoCAD**.

6. **El fondo de una cama corrida se guarda en la entrada de topologia de la celda**, y esa entrada fija la
   topologia por defecto vigente al escribirla. Como el editor no permite cambiar el default del rack, no es
   observable; queda declarado en `docs/ideas-futuras.md` con el coste real de resolverlo.

## 6. Checklist de validacion manual en AutoCAD 2025

DLL a cargar con `NETLOAD` (construido desde el HEAD candidato, con AutoCAD cerrado):

```
C:\Users\alejandra-mendoza\.claude\worktrees\feature-push-back-compuesto\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

### 6.1 Legacy — lo primero, y es la condicion de todo lo demas

1. Abre un rack Push Back dibujado **antes** de I-42 con `RACKEDITAR`. Debe abrirse como de un solo
   sentido, con «Rack de dos sentidos» **apagado** y la seccion compuesta COLAPSADA (no ocupa espacio en la
   barra lateral), sin pedir ninguna reconfiguracion.
2. Pulsa **Actualizar** sin tocar nada. El dibujo debe quedar **identico**: mismos largueros, mismas
   camas, mismos topes, mismo BOM.

### 6.1-bis LOS CUATRO ERRORES DE ESTA RONDA — mirar esto ANTES QUE NADA

Son los puntos que este candidato dice haber cerrado. Cada uno se ve en el dibujo, sin abrir el modelo.

**Error 6 — la altura la manda el extremo BAJO.**

3. Pon «Alto 1er nivel» en un valor concreto (por ejemplo 10"). En el corte lateral, el larguero de
   ENTRADA/SALIDA tiene que quedar a esa altura, ajustado a su troquel — **la misma con cualquier fondo**.
4. Cambia el FONDO de la celda (4 → 6 → 8 → 10). El larguero de entrada **no se mueve ni una vez**. El que sube
   es el POSTERIOR, y sube mas cuanto mas larga es la cama.
5. Cambia la TOPOLOGIA de una celda entre encontradas y corrida. El larguero de entrada **sigue donde estaba**:
   antes se hundia hasta 14" al pasar a corrida, y eso era el defecto.
6. La CAMA sigue apoyada en sus dos largueros y la pendiente sigue siendo la de siempre: la celda entera baja,
   la cama no se reinclina.

**Error 5 — la orientacion del larguero ALTO.**

7. Con camas ENCONTRADAS, los dos largueros posteriores se miran en el centro: la mano de uno es la contraria
   del otro.
8. Con una CORRIDA A→B y luego B→A, el larguero alto cambia de extremo **y de mano**. Comparalo con un Push Back
   de un solo sentido del mismo fondo: tiene que ser la misma pieza, girada.
9. Frentes de fondos distintos y una ranura presente en un solo lado: ningun larguero alto queda del reves.

**Error 10 — los topes.**

10. En PLANTA, el tope de una CORRIDA aparece **al final del recorrido de la calle**, en la orilla opuesta a su
    pasillo de carga — no en el centro del rack. Ese era el defecto: se dibujaba en la interfaz.
11. El tope de la planta y el del corte lateral estan en la MISMA columna. Antes discrepaban.
12. En el panel de topes, marca A y B, pon la celda en corrida A→B: la casilla de A se deshabilita y **sigue
    marcada**, y el texto dice que solo B es efectivo y donde va a caer la pieza. Cambia a B→A: se invierte, y
    **ninguna de las dos marcas se pierde**.
13. Los cinco alcances (celda, seleccion, nivel, frente, todo) escriben lo que dicen.
14. El BOM cuenta exactamente los topes que ves dibujados, ni uno mas.

**Error 4 — cabeceras por linea.**

15. Rack de un solo sentido con frentes de fondos **5, 8, 6 y 9**. La linea EXTERIOR del frente de 5 termina
    donde termina ese frente; la que separa 5 y 8 llega hasta 8; la que separa 6 y 9, hasta 9. Ningun frente
    lejano alarga una linea.
16. Rack COMPUESTO con la ranura 1 a fondo 5 y las demas a 8, en los dos lados. La linea exterior de la ranura
    corta **pierde las cabeceras** de las posiciones que ninguna de sus camas alcanza. Antes llegaban hasta el
    final.
17. Una ranura con una celda CORRIDA conserva la profundidad entera: su cama cruza y necesita apoyos en todo el
    recorrido.
18. Con separador central, sigue apareciendo donde los dos lados llegan a la interfaz.
19. El BOM de cabeceras baja exactamente lo que la planta dejo de dibujar.

### 6.2 LO QUE EL DUEÑO ENCONTRO EN LA SEGUNDA VALIDACION — revisar esto ANTES QUE NADA

3. **Corrida corta: el BAJO fijo, el ALTO movil.** Pon una corrida con menos fondo del disponible. Su extremo de
   carga tiene que quedar pegado al poste EXTERIOR; el que se mete hacia dentro es el otro.
4. **Corrida 6 / 8 / 10 en el mismo rack.** El extremo bajo no se mueve entre las tres; el alto si, y hacia fuera.
   Compruebalo tambien en el otro sentido.
5. **Lado A: cambiar el fondo base del frente funciona.** Y solo toca al lado A.
6. **Lado B: lo mismo, independiente.**
7. **Ambos lados:** aplica los mismos niveles y fondos a A y B en UNA sola accion. Con valores distintos entre lados,
   el campo aparece VACIO —ni el de A ni el de B— y escribir uno lo aplica a los dos.
8. **Ya no hay campos duplicados de fondo contra estructura.** «Fondos frente» es almacenamiento; «Estructura
   longitudinal del lado» muestra propuesta, efectiva y ajuste manual. Comprueba que se entiende cual es cual.
9. **Rack compuesto nuevo: seguridad en LOS DOS pasillos**, sin activarla a mano, y los dos topes posteriores
   encendidos.
10. **Topes: ninguno / A / B / ambos.**
11. **Frontal de A — entrada/salida**, con el selector de lado de la barra de vistas.
12. **Frontal de A — posterior.**
13. **Frontal de B — entrada/salida.**
14. **Frontal de B — posterior.** Los cuatro tienen que ser cuatro dibujos distintos.
15. **A = 4 niveles y B = 2:** el lado B NO crece con A. Las dos lineas de la interfaz pueden medir distinto.
16. **Cambiar solo A a 5 niveles:** ni una pieza de B se mueve.
17. **A = 3 y B = 4** con «Frente presente en este lado». Confirma que la UX se entiende.

### 6.2-bis Lo que fallo en la PRIMERA validacion — sigue en revision

18. **4 frentes x 3 niveles, los dos lados.** Pon «Frentes» = 4 y aplica 3 niveles a TODO. Los CUATRO frentes deben
   traer sus camas en los tres niveles, en los dos lados. Ni uno puede quedarse solo con cabeceras y postes.
19. **PLANTA: largueros intermedios en F1, F2, F3 y F4.** Compruebalo en las cuatro topologias.
20. **Cada corte lateral.** Recorre todos: cada uno debe traer los niveles y los largueros de los frentes que tiene
   al lado, no solo la estructura.
21. **Fondo de UNA celda.** Selecciona F2 / nivel 2, alcance «Celda», cambia el fondo. SOLO esa cama cambia; las
   otras once siguen igual. Repitelo con «Nivel», «Frente» y «Todo», y con «Restaurar fondo».
22. **Corrida 10 dentro de 5 + 8.** La cama debe alojar 10 fondos, apoyar en una linea de modulo real y NO arrancar
   desplazada hacia dentro. Con hueco, ninguna tarima puede caer dentro del hueco.
23. **Topes: ninguno / solo A / solo B / ambos**, desde «Tope posterior por lado». Con una sola cama, la casilla del
   lado que no tiene extremo alto debe estar deshabilitada y explicar por que — sin perder lo elegido.
24. **Estructura 8 con cama 4.** Sube la estructura del lado a 8 y deja una celda en 4: esa cama solo ocupa 4 fondos
   y en el tramo sobrante NO hay riel, rodillo, intermedio ni tarima. Otro nivel puede usar los 8 a la vez.
25. **A = 3 y B = 4.** Con 4 frentes, desmarca «La ranura existe en este lado» en el cuarto frente del lado A. La
    cuarta ranura debe quedar solo en B, sobre UNA sola estructura. Intenta quitarla tambien de B: debe rehusarse.

### 6.2-bis Lo que fallo en rondas anteriores — sigue en revision

26. **Corrida de 10 sobre una estructura 5 + 8.** Enciende el lado B, pon la celda en **corrida** y escribe **10**
   en «Fondo de cama corrida». La cama debe alojar **10** fondos, no 13. La estructura debe seguir siendo **5 en A
   y 8 en B**: ni un poste de mas. Vuelve la celda a **encontradas** y las dos camas por lado deben reaparecer
   intactas; vuelve a **corrida** y el 10 debe seguir escrito.
27. **La corrida NO arranca en el segundo fondo.** Con una corrida que atraviesa el rack, su extremo bajo debe
   apoyar en la **primera** linea de modulo. Repitelo con hueco 0 y con hueco positivo: el arranque no se mueve.
   Con una corrida CORTA, su extremo bajo debe caer sobre una linea de modulo — nunca a media posicion.
28. **La PLANTA dibuja largueros intermedios.** Comprueba las cuatro topologias. Con una corrida corta, los
   intermedios deben cubrir todo su recorrido (incluida la parte que pisa el otro lado) y ninguno delante de su
   extremo bajo.
29. **El HUECO.** Con una estructura manual corta y una corrida que no quepa, la celda debe quedar bloqueada con
   hueco 0 y volverse valida al subirlo. La **demanda** en fondos no cambia; la longitud fisica de la cama **si**
   puede crecer si tiene que cruzar el hueco para alcanzar su ultimo fondo.

### 6.2-bis Lo que fallo en rondas previas — sigue en revision

30. **4 fondos en A y 8 en B, encontradas.** NO debe aparecer ningun error de capacidad. Compruebalo tambien con
   8/5, 3/9 y 8/4, y en las cuatro topologias.
31. **Largueros intermedios.** En un lateral con varios niveles: cada cama debe llevar TODOS sus intermedios, a la
   elevacion de SU cama y siguiendo SU pendiente. Ninguno fuera del vano real de su cama, ninguno cruzado a la
   cama contraria, y `RACKBOMTOTAL` debe cotizar exactamente los que se dibujan.
32. **Una corrida corta no crea otra estructura.** Con el caso del punto 3 en pantalla: no debe aparecer ninguna
   segunda estructura, ni un segundo juego de postes, cabeceras o placas.
33. **Cotas y etiquetas del lateral.** Con niveles y elevaciones distintas en A y en B, ninguna cota ni etiqueta
   puede afirmar un valor de A sobre una pieza de B. Los textos deben leerse bien en los dos lados.
34. **Planta con todos los niveles corridos.** No debe dibujar los largueros posteriores de la interfaz. Cambia UN
   nivel a encontradas y deben aparecer.
35. **UI con el compuesto apagado.** La seccion A/B no debe ocupar espacio: el editor debe verse como el de antes
   de I-42.
36. **`F1` compartida, `F2` solo A, `F3` solo B, `F4` compartida.** Debe construirse en UNA sola estructura.
### 6.3 Declarar el lado B y la estructura compartida

37. Rack nuevo. Enciende «Rack de dos sentidos (lado B)». Aparece la seccion compuesta y la segunda mitad
    **sobre la misma estructura**: el lado A no se mueve de sitio ni cambia de altura.
38. Con el **selector de lado** cambia a «Lado B» y vuelve a «Lado A». La matriz Frente x Nivel, la celda
   seleccionada y los cinco alcances trabajan sobre el lado elegido, y al volver la configuracion y la
   seleccion del otro lado siguen **intactas**.
39. Pon **3 frentes en A y 4 en B**. La cuarta ranura debe existir **solo** en la mitad de B, y las lineas
   de postes y el BFR deben ser **unicos** para los dos lados.
40. Pon **2 niveles en A y 5 en B**. Los postes se dimensionan por la mayor demanda y cada lado dibuja
   **sus** elevaciones.

### 6.4 Topologia por celda

41. En un mismo frente: nivel 1 **corrida**, nivel 2 **encontradas**, nivel 3 **solo A**, nivel 4 **solo
   B**. Los cuatro deben coexistir.
42. **Encontradas**: DOS camas fisicas, con pendientes opuestas y los extremos ALTOS enfrentados en el
   centro. Desde **Seguridad**, prueba tope ninguno / solo A / solo B / ambos.
43. **Corrida**: UNA sola cama que atraviesa A + hueco + B, con **una** pendiente continua y como mucho
   **UN** tope, en su extremo alto.
44. Cambia el sentido de la corrida (**A→B** y **B→A**). El extremo ALTO debe moverse **fisicamente** al
    otro lado y el tope debe seguirlo. No es un espejo grafico.
45. Vuelve la celda a **encontradas**. La elevacion propia del lado que estaba bajo debe **reaparecer**:
    no se perdio mientras hubo corrida.

### 6.5 Interfaz central

46. **Hueco**: llevalo de 0 a un valor positivo. El rack debe **alargarse exactamente esa medida**. Con
    hueco 0, las **dos** lineas de postes de la interfaz deben seguir existiendo.
47. **Separador central** con hueco positivo: aparece **UNA** sola pieza —la misma que ya usa el rack— y
    `RACKBOMTOTAL` la cuenta **una vez**. Con hueco 0 se avisa y no se coloca.

### 6.6 Estructura efectiva por lado

48. Sube la **estructura del lado activo** por encima de la propuesta y pulsa «Aplicar estructura»: el
    rack crece por ese lado.
49. Bajala **por debajo** de la propuesta: NO debe corregirse sola. Debe avisarse, y las celdas que ya no
    caben deben quedar **bloqueadas con su motivo**.
50. «Restaurar estructura»: vuelve a la propuesta **actual**.

### 6.7 Fondos, tarimas y vistas

51. Fondos y tarimas **por celda en los dos lados**: son independientes; cada tarima sigue la pendiente de
    **su** cama y **ninguna** aparece en el BOM.
52. **Cuatro cortes frontales** (entrada/salida y posterior de cada lado): insertalos y actualizalos. Una
    celda **corrida** NO debe mostrar larguero posterior en la linea interior de su lado BAJO.
53. **Planta y laterales**: llevan las etiquetas **A** y **B**, y la planta muestra un larguero de
    entrada/salida en los **dos** pasillos.

### 6.8 BOM y round trip

54. `RACKBOMTOTAL`: la estructura **no** se duplica por tener dos lados; una corrida cuenta **UNA** cama a
    la longitud del rack entero y dos encontradas cuentan **DOS**.
55. Guarda, cierra y reabre con `RACKEDITAR`: topologia, sentido, hueco, separador, estructura manual y
    las dos configuraciones vuelven **identicas**, con el **mismo GUID**.
56. `RACKDUPLICAR`: produce una copia **independiente**.

## 7. Criterio de aprobacion

El veredicto del Owner en AutoCAD 2025 es el gate: CI verde es necesario y **no** suficiente, porque las
pruebas no ven los bloques DWG reales. Si se aprueba, ADR-0031 pasa a `aceptado` con el modelo
implementado y sus cinco limitaciones declaradas.
