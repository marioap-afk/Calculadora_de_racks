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
9. **La corrida NO ocupa todo el sistema**: `PhysicalBedLength = RequiredBedLength`, anclada en el extremo ALTO.
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
PhysicalBedLength   = RequiredBedLength
valido              <=>  Required <= Available
```

Evidencia numerica (estructura A=5 / B=4 por ajuste manual, demanda corrida 8+5 = 13 fondos):

| | Demand | Required | Available | Valida | PhysicalBedLength |
|---|---|---|---|---|---|
| Gap = 0" | 13 | 648" | 456" | **no** | 648" |
| Gap = 198" | 13 | 648" | 654" | **si** | 648" |

`delta Required = 0"` · `delta Available = 198"` (exactamente el hueco) · `delta PhysicalBedLength = 0"`.

Ademas se retiro el ultimo recorte silencioso: una cama que no cabe conserva su longitud exigida en vez de
recortarse contra el espacio disponible, de modo que el diagnostico puede decir cuanto le falta.

## 5. Limitaciones DECLARADAS (no son descuidos)

1. **El separador central exige hueco mayor que cero**; con hueco 0 se avisa y no se coloca.
2. **Una ranura presente en LOS DOS lados tiene estructura a lo largo de toda la profundidad**: su fondo por lado
   gobierna donde acaban sus CAMAS, no donde acaban sus marcos — la misma regla de I-41.
3. **Un ajuste manual de estructura se aplica a todas las ranuras de ese lado.** Sin ajuste manual cada ranura
   conserva su envolvente, asi que frentes cortos y largos conviven igual que en I-41.
4. **Una ranura ausente en una posicion INTERIOR** deja en el corte frontal de ese lado la linea de postes de su
   frontera (regla de I-33 sobre frentes en blanco). Las ausencias del final si se retiran — el caso habitual.
   Se validara visualmente: es estructura fisica compartida.
5. **Una corrida que cruza el hueco** apoya su extremo bajo tantas pulgadas dentro del lado bajo como mida el
   hueco, porque su longitud es la de su demanda y el hueco no consume demanda. Consecuencia directa de
   `PhysicalBedLength = RequiredBedLength`; **conviene mirarla en AutoCAD**.

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

### 6.2 LO QUE FALLO EN LA RONDA ANTERIOR — revisar esto primero

3. **4 fondos en A y 8 en B, encontradas.** NO debe aparecer ningun error de capacidad. Compruebalo tambien con
   8/5, 3/9 y 8/4, y en las cuatro topologias.
4. **Largueros intermedios.** En un lateral con varios niveles: cada cama debe llevar TODOS sus intermedios, a la
   elevacion de SU cama y siguiendo SU pendiente. Ninguno fuera del vano real de su cama, ninguno cruzado a la
   cama contraria, y `RACKBOMTOTAL` debe cotizar exactamente los que se dibujan.
5. **Corrida corta.** Estructura 5 + 8 con una corrida de 10 fondos: UNA cama, anclada en el extremo ALTO, mas
   corta que el rack, y la estructura sigue siendo 5 + 8. No debe aparecer ninguna segunda estructura ni un
   segundo juego de postes, cabeceras o placas.
6. **Cotas y etiquetas del lateral.** Con niveles y elevaciones distintas en A y en B, ninguna cota ni etiqueta
   puede afirmar un valor de A sobre una pieza de B. Los textos deben leerse bien en los dos lados.
7. **Planta con todos los niveles corridos.** No debe dibujar los largueros posteriores de la interfaz. Cambia UN
   nivel a encontradas y deben aparecer.
8. **UI con el compuesto apagado.** La seccion A/B no debe ocupar espacio: el editor debe verse como el de antes
   de I-42.
9. **`F1` compartida, `F2` solo A, `F3` solo B, `F4` compartida.** Debe construirse en UNA sola estructura.
10. **El HUECO como capacidad.** Con una estructura manual corta (por ejemplo A=5 / B=4) y una corrida que pida
    mas fondos de los que caben, la celda debe quedar bloqueada con hueco 0. Sube el hueco lo suficiente y debe
    volverse valida **sin que la cama se alargue**: solo crece el rack. Comprueba tambien que la cama no encoge ni
    se estira al mover el hueco cuando ya era valida.

### 6.3 Declarar el lado B y la estructura compartida

11. Rack nuevo. Enciende «Rack de dos sentidos (lado B)». Aparece la seccion compuesta y la segunda mitad
    **sobre la misma estructura**: el lado A no se mueve de sitio ni cambia de altura.
12. Con el **selector de lado** cambia a «Lado B» y vuelve a «Lado A». La matriz Frente x Nivel, la celda
   seleccionada y los cinco alcances trabajan sobre el lado elegido, y al volver la configuracion y la
   seleccion del otro lado siguen **intactas**.
13. Pon **3 frentes en A y 4 en B**. La cuarta ranura debe existir **solo** en la mitad de B, y las lineas
   de postes y el BFR deben ser **unicos** para los dos lados.
14. Pon **2 niveles en A y 5 en B**. Los postes se dimensionan por la mayor demanda y cada lado dibuja
   **sus** elevaciones.

### 6.4 Topologia por celda

15. En un mismo frente: nivel 1 **corrida**, nivel 2 **encontradas**, nivel 3 **solo A**, nivel 4 **solo
   B**. Los cuatro deben coexistir.
16. **Encontradas**: DOS camas fisicas, con pendientes opuestas y los extremos ALTOS enfrentados en el
   centro. Desde **Seguridad**, prueba tope ninguno / solo A / solo B / ambos.
17. **Corrida**: UNA sola cama que atraviesa A + hueco + B, con **una** pendiente continua y como mucho
   **UN** tope, en su extremo alto.
18. Cambia el sentido de la corrida (**A→B** y **B→A**). El extremo ALTO debe moverse **fisicamente** al
    otro lado y el tope debe seguirlo. No es un espejo grafico.
19. Vuelve la celda a **encontradas**. La elevacion propia del lado que estaba bajo debe **reaparecer**:
    no se perdio mientras hubo corrida.

### 6.5 Interfaz central

20. **Hueco**: llevalo de 0 a un valor positivo. El rack debe **alargarse exactamente esa medida**. Con
    hueco 0, las **dos** lineas de postes de la interfaz deben seguir existiendo.
21. **Separador central** con hueco positivo: aparece **UNA** sola pieza —la misma que ya usa el rack— y
    `RACKBOMTOTAL` la cuenta **una vez**. Con hueco 0 se avisa y no se coloca.

### 6.6 Estructura efectiva por lado

22. Sube la **estructura del lado activo** por encima de la propuesta y pulsa «Aplicar estructura»: el
    rack crece por ese lado.
23. Bajala **por debajo** de la propuesta: NO debe corregirse sola. Debe avisarse, y las celdas que ya no
    caben deben quedar **bloqueadas con su motivo**.
24. «Restaurar estructura»: vuelve a la propuesta **actual**.

### 6.7 Fondos, tarimas y vistas

25. Fondos y tarimas **por celda en los dos lados**: son independientes; cada tarima sigue la pendiente de
    **su** cama y **ninguna** aparece en el BOM.
26. **Cuatro cortes frontales** (entrada/salida y posterior de cada lado): insertalos y actualizalos. Una
    celda **corrida** NO debe mostrar larguero posterior en la linea interior de su lado BAJO.
27. **Planta y laterales**: llevan las etiquetas **A** y **B**, y la planta muestra un larguero de
    entrada/salida en los **dos** pasillos.

### 6.8 BOM y round trip

28. `RACKBOMTOTAL`: la estructura **no** se duplica por tener dos lados; una corrida cuenta **UNA** cama a
    la longitud del rack entero y dos encontradas cuentan **DOS**.
29. Guarda, cierra y reabre con `RACKEDITAR`: topologia, sentido, hueco, separador, estructura manual y
    las dos configuraciones vuelven **identicas**, con el **mismo GUID**.
30. `RACKDUPLICAR`: produce una copia **independiente**.

## 7. Criterio de aprobacion

El veredicto del Owner en AutoCAD 2025 es el gate: CI verde es necesario y **no** suficiente, porque las
pruebas no ven los bloques DWG reales. Si se aprueba, ADR-0031 pasa a `aceptado` con el modelo
implementado y sus cinco limitaciones declaradas.
