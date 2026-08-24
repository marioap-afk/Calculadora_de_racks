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

## 4. Limitaciones DECLARADAS (no son descuidos)

1. **No pueden coexistir una ranura presente solo en A y otra presente solo en B.** La reticula de
   profundidad compartida exige que los rangos de los frentes aniden; se reporta con un error explicito.
2. **Las cotas y etiquetas de nivel del corte lateral siguen las elevaciones del lado A**, el de
   referencia: una sola tabla frente/nivel/elevacion no puede describir dos pasillos.
3. **La planta colapsa los niveles**, asi que una ranura cuyos niveles fueran *todos* corridos seguiria
   mostrando los largueros posteriores de la interfaz.
4. **El separador central exige hueco mayor que cero**; con hueco 0 se avisa y no se coloca.
5. **Mover la envolvente de un lado reconstruye la secuencia de modulos** y pierde personalizaciones de
   modulo — mismo comportamiento que ya tenia un rack de un sentido.

Las cuatro primeras estan anotadas tambien en [ideas-futuras](../ideas-futuras.md) con por que cada una
necesitaria su propia iniciativa.

## 5. Checklist de validacion manual en AutoCAD 2025

DLL a cargar con `NETLOAD` (construido desde el HEAD candidato, con AutoCAD cerrado):

```
C:\Users\alejandra-mendoza\.claude\worktrees\feature-push-back-compuesto\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
```

### 5.1 Legacy — lo primero, y es la condicion de todo lo demas

1. Abre un rack Push Back dibujado **antes** de I-42 con `RACKEDITAR`. Debe abrirse como de un solo
   sentido, con «Rack de dos sentidos» **apagado** y toda la seccion compuesta deshabilitada, sin pedir
   ninguna reconfiguracion.
2. Pulsa **Actualizar** sin tocar nada. El dibujo debe quedar **identico**: mismos largueros, mismas
   camas, mismos topes, mismo BOM.

### 5.2 Declarar el lado B y la estructura compartida

3. Rack nuevo. Enciende «Rack de dos sentidos (lado B)». Aparece la segunda mitad **sobre la misma
   estructura**: el lado A no se mueve de sitio ni cambia de altura.
4. Con el **selector de lado** cambia a «Lado B» y vuelve a «Lado A». La matriz Frente x Nivel, la celda
   seleccionada y los cinco alcances trabajan sobre el lado elegido, y al volver la configuracion y la
   seleccion del otro lado siguen **intactas**.
5. Pon **3 frentes en A y 4 en B**. La cuarta ranura debe existir **solo** en la mitad de B, y las lineas
   de postes y el BFR deben ser **unicos** para los dos lados.
6. Pon **2 niveles en A y 5 en B**. Los postes se dimensionan por la mayor demanda y cada lado dibuja
   **sus** elevaciones.

### 5.3 Topologia por celda

7. En un mismo frente: nivel 1 **corrida**, nivel 2 **encontradas**, nivel 3 **solo A**, nivel 4 **solo
   B**. Los cuatro deben coexistir.
8. **Encontradas**: DOS camas fisicas, con pendientes opuestas y los extremos ALTOS enfrentados en el
   centro. Desde **Seguridad**, prueba tope ninguno / solo A / solo B / ambos.
9. **Corrida**: UNA sola cama que atraviesa A + hueco + B, con **una** pendiente continua y como mucho
   **UN** tope, en su extremo alto.
10. Cambia el sentido de la corrida (**A→B** y **B→A**). El extremo ALTO debe moverse **fisicamente** al
    otro lado y el tope debe seguirlo. No es un espejo grafico.
11. Vuelve la celda a **encontradas**. La elevacion propia del lado que estaba bajo debe **reaparecer**:
    no se perdio mientras hubo corrida.

### 5.4 Interfaz central

12. **Hueco**: llevalo de 0 a un valor positivo. El rack debe **alargarse exactamente esa medida**. Con
    hueco 0, las **dos** lineas de postes de la interfaz deben seguir existiendo.
13. **Separador central** con hueco positivo: aparece **UNA** sola pieza —la misma que ya usa el rack— y
    `RACKBOMTOTAL` la cuenta **una vez**. Con hueco 0 se avisa y no se coloca.

### 5.5 Estructura efectiva por lado

14. Sube la **estructura del lado activo** por encima de la propuesta y pulsa «Aplicar estructura»: el
    rack crece por ese lado.
15. Bajala **por debajo** de la propuesta: NO debe corregirse sola. Debe avisarse, y las celdas que ya no
    caben deben quedar **bloqueadas con su motivo**.
16. «Restaurar estructura»: vuelve a la propuesta **actual**.

### 5.6 Fondos, tarimas y vistas

17. Fondos y tarimas **por celda en los dos lados**: son independientes; cada tarima sigue la pendiente de
    **su** cama y **ninguna** aparece en el BOM.
18. **Cuatro cortes frontales** (entrada/salida y posterior de cada lado): insertalos y actualizalos. Una
    celda **corrida** NO debe mostrar larguero posterior en la linea interior de su lado BAJO.
19. **Planta y laterales**: llevan las etiquetas **A** y **B**, y la planta muestra un larguero de
    entrada/salida en los **dos** pasillos.

### 5.7 BOM y round trip

20. `RACKBOMTOTAL`: la estructura **no** se duplica por tener dos lados; una corrida cuenta **UNA** cama a
    la longitud del rack entero y dos encontradas cuentan **DOS**.
21. Guarda, cierra y reabre con `RACKEDITAR`: topologia, sentido, hueco, separador, estructura manual y
    las dos configuraciones vuelven **identicas**, con el **mismo GUID**.
22. `RACKDUPLICAR`: produce una copia **independiente**.

## 6. Criterio de aprobacion

El veredicto del Owner en AutoCAD 2025 es el gate: CI verde es necesario y **no** suficiente, porque las
pruebas no ven los bloques DWG reales. Si se aprueba, ADR-0031 pasa a `aceptado` con el modelo
implementado y sus cinco limitaciones declaradas.
