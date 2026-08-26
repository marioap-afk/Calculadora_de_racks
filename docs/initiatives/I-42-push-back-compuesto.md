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
