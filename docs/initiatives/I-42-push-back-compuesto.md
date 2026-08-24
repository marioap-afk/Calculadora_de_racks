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

### 6.2 LO QUE FALLO EN LA ULTIMA RONDA — revisar esto ANTES QUE NADA

3. **Corrida de 10 sobre una estructura 5 + 8.** Enciende el lado B, pon la celda en **corrida** y escribe **10**
   en «Fondo de cama corrida». La cama debe alojar **10** fondos, no 13. La estructura debe seguir siendo **5 en A
   y 8 en B**: ni un poste de mas. Vuelve la celda a **encontradas** y las dos camas por lado deben reaparecer
   intactas; vuelve a **corrida** y el 10 debe seguir escrito.
4. **La corrida NO arranca en el segundo fondo.** Con una corrida que atraviesa el rack, su extremo bajo debe
   apoyar en la **primera** linea de modulo. Repitelo con hueco 0 y con hueco positivo: el arranque no se mueve.
   Con una corrida CORTA, su extremo bajo debe caer sobre una linea de modulo — nunca a media posicion.
5. **La PLANTA dibuja largueros intermedios.** Comprueba las cuatro topologias. Con una corrida corta, los
   intermedios deben cubrir todo su recorrido (incluida la parte que pisa el otro lado) y ninguno delante de su
   extremo bajo.
6. **El HUECO.** Con una estructura manual corta y una corrida que no quepa, la celda debe quedar bloqueada con
   hueco 0 y volverse valida al subirlo. La **demanda** en fondos no cambia; la longitud fisica de la cama **si**
   puede crecer si tiene que cruzar el hueco para alcanzar su ultimo fondo.

### 6.2-bis Lo que fallo en rondas previas — sigue en revision

7. **4 fondos en A y 8 en B, encontradas.** NO debe aparecer ningun error de capacidad. Compruebalo tambien con
   8/5, 3/9 y 8/4, y en las cuatro topologias.
8. **Largueros intermedios.** En un lateral con varios niveles: cada cama debe llevar TODOS sus intermedios, a la
   elevacion de SU cama y siguiendo SU pendiente. Ninguno fuera del vano real de su cama, ninguno cruzado a la
   cama contraria, y `RACKBOMTOTAL` debe cotizar exactamente los que se dibujan.
9. **Una corrida corta no crea otra estructura.** Con el caso del punto 3 en pantalla: no debe aparecer ninguna
   segunda estructura, ni un segundo juego de postes, cabeceras o placas.
10. **Cotas y etiquetas del lateral.** Con niveles y elevaciones distintas en A y en B, ninguna cota ni etiqueta
   puede afirmar un valor de A sobre una pieza de B. Los textos deben leerse bien en los dos lados.
11. **Planta con todos los niveles corridos.** No debe dibujar los largueros posteriores de la interfaz. Cambia UN
   nivel a encontradas y deben aparecer.
12. **UI con el compuesto apagado.** La seccion A/B no debe ocupar espacio: el editor debe verse como el de antes
   de I-42.
13. **`F1` compartida, `F2` solo A, `F3` solo B, `F4` compartida.** Debe construirse en UNA sola estructura.
### 6.3 Declarar el lado B y la estructura compartida

14. Rack nuevo. Enciende «Rack de dos sentidos (lado B)». Aparece la seccion compuesta y la segunda mitad
    **sobre la misma estructura**: el lado A no se mueve de sitio ni cambia de altura.
15. Con el **selector de lado** cambia a «Lado B» y vuelve a «Lado A». La matriz Frente x Nivel, la celda
   seleccionada y los cinco alcances trabajan sobre el lado elegido, y al volver la configuracion y la
   seleccion del otro lado siguen **intactas**.
16. Pon **3 frentes en A y 4 en B**. La cuarta ranura debe existir **solo** en la mitad de B, y las lineas
   de postes y el BFR deben ser **unicos** para los dos lados.
17. Pon **2 niveles en A y 5 en B**. Los postes se dimensionan por la mayor demanda y cada lado dibuja
   **sus** elevaciones.

### 6.4 Topologia por celda

18. En un mismo frente: nivel 1 **corrida**, nivel 2 **encontradas**, nivel 3 **solo A**, nivel 4 **solo
   B**. Los cuatro deben coexistir.
19. **Encontradas**: DOS camas fisicas, con pendientes opuestas y los extremos ALTOS enfrentados en el
   centro. Desde **Seguridad**, prueba tope ninguno / solo A / solo B / ambos.
20. **Corrida**: UNA sola cama que atraviesa A + hueco + B, con **una** pendiente continua y como mucho
   **UN** tope, en su extremo alto.
21. Cambia el sentido de la corrida (**A→B** y **B→A**). El extremo ALTO debe moverse **fisicamente** al
    otro lado y el tope debe seguirlo. No es un espejo grafico.
22. Vuelve la celda a **encontradas**. La elevacion propia del lado que estaba bajo debe **reaparecer**:
    no se perdio mientras hubo corrida.

### 6.5 Interfaz central

23. **Hueco**: llevalo de 0 a un valor positivo. El rack debe **alargarse exactamente esa medida**. Con
    hueco 0, las **dos** lineas de postes de la interfaz deben seguir existiendo.
24. **Separador central** con hueco positivo: aparece **UNA** sola pieza —la misma que ya usa el rack— y
    `RACKBOMTOTAL` la cuenta **una vez**. Con hueco 0 se avisa y no se coloca.

### 6.6 Estructura efectiva por lado

25. Sube la **estructura del lado activo** por encima de la propuesta y pulsa «Aplicar estructura»: el
    rack crece por ese lado.
26. Bajala **por debajo** de la propuesta: NO debe corregirse sola. Debe avisarse, y las celdas que ya no
    caben deben quedar **bloqueadas con su motivo**.
27. «Restaurar estructura»: vuelve a la propuesta **actual**.

### 6.7 Fondos, tarimas y vistas

28. Fondos y tarimas **por celda en los dos lados**: son independientes; cada tarima sigue la pendiente de
    **su** cama y **ninguna** aparece en el BOM.
29. **Cuatro cortes frontales** (entrada/salida y posterior de cada lado): insertalos y actualizalos. Una
    celda **corrida** NO debe mostrar larguero posterior en la linea interior de su lado BAJO.
30. **Planta y laterales**: llevan las etiquetas **A** y **B**, y la planta muestra un larguero de
    entrada/salida en los **dos** pasillos.

### 6.8 BOM y round trip

31. `RACKBOMTOTAL`: la estructura **no** se duplica por tener dos lados; una corrida cuenta **UNA** cama a
    la longitud del rack entero y dos encontradas cuentan **DOS**.
32. Guarda, cierra y reabre con `RACKEDITAR`: topologia, sentido, hueco, separador, estructura manual y
    las dos configuraciones vuelven **identicas**, con el **mismo GUID**.
33. `RACKDUPLICAR`: produce una copia **independiente**.

## 7. Criterio de aprobacion

El veredicto del Owner en AutoCAD 2025 es el gate: CI verde es necesario y **no** suficiente, porque las
pruebas no ven los bloques DWG reales. Si se aprueba, ADR-0031 pasa a `aceptado` con el modelo
implementado y sus cinco limitaciones declaradas.
